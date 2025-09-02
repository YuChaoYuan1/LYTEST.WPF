using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.PrepareTest
{
    class PreWriteAddress : VerifyBase
    {
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "条码中的地址", "表内地址", "结论" };
            return base.CheckPara();
        }
        public override void Verify()
        {
            MessageAdd("写地址开始...", EnumLogType.流程信息);

            base.Verify();
            string[] writeAddress = new string[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                writeAddress[i] = ExtractAddress(meter.MD_BarCode);
                ResultDictionary["条码中的地址"][i] = writeAddress[i];
            }
            RefUIData("条码中的地址");

            PowerOn();
            WaitTime("升电压", 10);

            if (Stop) return;
            bool[] needWrite = new bool[MeterNumber];
            bool reTry = false;
            {
                string[] address = MeterProtocolAdapter.Instance.ReadAddress();
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;
                    if (string.IsNullOrWhiteSpace(address[i]))
                    {
                        reTry = true;
                        break;
                    }
                }
                if (Stop) return;
                if (reTry)
                {
                    string[] addrTmp = MeterProtocolAdapter.Instance.ReadAddress();
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;
                        if (string.IsNullOrWhiteSpace(address[i]))
                        {
                            address[i] = addrTmp[i];
                        }
                    }
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;

                    if (address[i].Equals(ResultDictionary["条码中的地址"][i]))
                    {
                        ResultDictionary["表内地址"][i] = address[i];
                        needWrite[i] = false;
                    }
                    else
                    {
                        needWrite[i] = true;
                    }
                }
                RefUIData("表内地址");
            }

            if (Stop) return;

            string[] rst_addr = MeterProtocolAdapter.Instance.WriteAddress(writeAddress, needWrite);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!needWrite[i]) continue;
                if (!string.IsNullOrWhiteSpace(rst_addr[i]) && rst_addr[i].Equals(writeAddress[i]))
                {
                    ResultDictionary["表内地址"][i] = rst_addr[i];
                    needWrite[i] = false;
                }
            }
            if (Stop) return;
            if (Array.IndexOf(needWrite, true) >= 0)
            {
                string[] addrTmp = MeterProtocolAdapter.Instance.WriteAddress(writeAddress, needWrite);
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!needWrite[i]) continue;
                    if (!string.IsNullOrWhiteSpace(addrTmp[i]) && addrTmp[i].Equals(writeAddress[i]))
                    {
                        rst_addr[i] = addrTmp[i];
                        ResultDictionary["表内地址"][i] = rst_addr[i];
                        needWrite[i] = false;
                    }
                }
            }
            RefUIData("表内地址");

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                if (!string.IsNullOrWhiteSpace(ResultDictionary["表内地址"][i]))
                {
                    EquipmentData.MeterGroupInfo.SetMeterInfo(i, "MD_POSTAL_ADDRESS", ResultDictionary["表内地址"][i]);
                    meter.MD_PostalAddress = rst_addr[i];
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }
                else
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
            }

            EquipmentData.Controller.UpdateMeterProtocol();

            RefUIData("结论");
            MessageAdd("写地址完成", EnumLogType.提示与流程信息);
        }

        private string ExtractAddress(string barcode)
        {
            string addr = string.Empty;
            int starti = ConfigHelper.Instance.Address_StartIndex - 1;
            if (starti < 0) starti = 0;
            int needLen = starti + ConfigHelper.Instance.Address_Len;

            if (barcode.Length >= needLen)
            {
                if (ConfigHelper.Instance.Address_LeftToRight)
                {
                    addr = barcode.Substring(starti, ConfigHelper.Instance.Address_Len).PadLeft(12, '0');
                }
                else
                {
                    addr = barcode.Substring(barcode.Length - needLen, ConfigHelper.Instance.Address_Len).PadLeft(12, '0');
                }
            }
            return addr;
        }
    }
}
