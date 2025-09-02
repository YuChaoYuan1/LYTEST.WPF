using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.MeterProtocol;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.Multi
{
    //add yjt 20220314 新增通讯测试功能
    public class Dgn_CommTest : VerifyBase
    {
        /// <summary>
        /// 通讯测试
        /// </summary>                                                                 
        public override void Verify()
        {
            MessageAdd("通讯测试试验检定开始...", EnumLogType.流程信息);

            bool CheckComm = false; //是否校验通讯地址
            base.Verify();

            int intChannelCount = MeterProtocolManager.Instance.GetChannelCount();
            bool[] result = new bool[MeterNumber];
            if (!CheckVoltage())
            {
                return;
            }

            MessageAdd("正在进行通讯测试", EnumLogType.提示信息);

            TestMeterInfo FirstMeter = MeterInfo[FirstIndex];


            string[] address = new string[MeterNumber];
            DateTime[] arrReadData = new DateTime[0];

            for (int re = 0; re < 3; re++)
            {

                if (IsDemo)
                {
                    //address = new string[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        address[i] = "";
                        result[i] = true;
                    }
                }
                else
                {
                    WaitTime("通信测试", 1);
                    if (CheckComm)
                    {
                        arrReadData = MeterProtocolAdapter.Instance.ReadDateTime();
                    }
                    else
                    {
                        if (FirstMeter.DgnProtocol.ClassName == "CDLT698")
                        {
                            if (Stop) return;
                            MessageAdd("正在进行【读取表地址表号】操作...", EnumLogType.提示信息);
                            List<string> LstOad = new List<string>
                            {
                                "40010200" //通信地址
                            };
                            Dictionary<int, object[]> dic1 = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormal);

                            LstOad = new List<string>
                            {
                                "40020200" //表号
                            };
                            Dictionary<int, object[]> dic2 = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormal);

                            Dictionary<int, object[]> dic = new Dictionary<int, object[]>();
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (!MeterInfo[i].YaoJianYn) continue;
                                if (dic1.ContainsKey(i) && dic2.ContainsKey(i))
                                {
                                    if (dic1[i] != null && dic1[i].Length > 0 && dic2[i] != null && dic2[i].Length > 0)
                                        dic.Add(i, new object[] { dic1[i][0], dic2[i][0] });
                                }
                            }

                            //address = new string[MeterNumber];
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (!MeterInfo[i].YaoJianYn) continue;
                                TestMeterInfo meter = MeterInfo[i];

                                if (dic.ContainsKey(i))
                                {
                                    if (dic[i].Length > 1)
                                    {
                                        meter.MD_PostalAddress = dic[i][0].ToString(); //通信地址
                                        meter.MD_MeterNo = dic[i][1].ToString(); //表号
                                        address[i] = meter.MD_PostalAddress;
                                    }
                                }
                            }
                            EquipmentData.Controller.UpdateMeterProtocol();
                        }
                        else
                        {
                            if (FirstMeter.MD_ProtocolName.IndexOf("645") != -1 && MeterNumber / intChannelCount == 1)
                                address = MeterProtocolAdapter.Instance.ReadAddress();
                            else
                                result = MeterProtocolAdapter.Instance.CommTest();
                        }
                    }
                    MessageAdd("处理结果...", EnumLogType.提示信息);
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    //TestMeterInfo meter = MeterInfo[i];

                    #region 校验通信地址                
                    if (CheckComm)
                    {
                        DateTime dataTime = new DateTime(2001, 1, 1, 0, 0, 0);
                        if (arrReadData[i].ToString() != null && arrReadData[i].ToString() != "" && arrReadData[i] != dataTime)
                            result[i] = true;
                    }
                    else
                    {
                        if ((FirstMeter.MD_ProtocolName.IndexOf("645") != -1 && !IsDemo && MeterNumber / intChannelCount == 1) || FirstMeter.DgnProtocol.ClassName == "CDLT698")
                        {
                            string inAddr = EquipmentData.MeterGroupInfo.GetMeterInfo(i, "MD_POSTAL_ADDRESS");
                            if (string.IsNullOrWhiteSpace(inAddr))
                            {
                                result[i] = !string.IsNullOrWhiteSpace(address[i]);
                            }
                            else
                            {
                                result[i] = inAddr.Equals(address[i]);
                            }
                        }
                        if (IsDemo)
                            result[i] = true;
                    }
                    #endregion 校验通信地址 结束
                }

                bool retry = false;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!result[i])
                    {
                        retry = true;
                        break;
                    }
                }
                if (!retry) break;
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                if (!MeterInfo[i].YaoJianYn) continue;
                TestMeterInfo meter = MeterInfo[i];

                if (!CheckComm && FirstMeter.MD_ProtocolName.IndexOf("645") != -1 && MeterNumber / intChannelCount == 1)
                    meter.MD_PostalAddress = address[i];

                if (result[i])
                {
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                    ResultDictionary["第一路485"][i] = address[i];
                }
                else
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                    ResultDictionary["第一路485"][i] = $"{address[i]}({meter.MD_PostalAddress})";
                }
                //新加 演示模式 通讯测试
                if (IsDemo)
                {
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                    ResultDictionary["第一路485"][i] = meter.MD_PostalAddress;
                }
            }
            EquipmentData.Controller.UpdateMeterProtocol();

            RefUIData("第一路485");
            RefUIData("结论");

            MessageAdd("检定完成", EnumLogType.提示信息);
            MessageAdd("通讯测试试验检定结束...", EnumLogType.流程信息);
        }


        protected override bool CheckPara()
        {
            ResultNames = new string[] { "第一路485", "第二路485", "结论" };
            return true;
        }
    }
}
