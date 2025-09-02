using LYTest.Core.Enum;
using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 报警功能
    /// </summary>
    class FK_Encryption_Warning : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            base.Verify();

            //初始化设备
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;
            string[] TemClassName = new string[MeterNumber];
            if (IsDoubleProtocol) //IR46表,并且需要检定前进行加密解密
            {
                string ClassName = "";
                switch (VerifyConfig.Test_CryptoModel)
                {
                    case "根据表协议":
                        ClassName = OneMeterInfo.DgnProtocol.ClassName;
                        break;
                    case "根据698协议":
                        ClassName = "CDLT698";
                        break;
                    case "根据645协议":
                        ClassName = "CDLT6452007";
                        break;
                    default:
                        break;
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    TemClassName[i] = MeterInfo[i].MD_ProtocolName;//保存旧的协议
                    MeterInfo[i].MD_ProtocolName = ClassName;  //修改协议进行加密解密
                }
                UpdateMeterProtocol();//更新一下电表协议
            }
            if (Stop) return;
            //获取所有表的表地址
            ReadMeterAddrAndNo();
            if (Stop) return;
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return;

            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            //Identity();
            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            Identity(true);

            if (Stop) return;

            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄);
            }

            //string[]  str = MeterProtocolAdapter.Instance.ReadData("报警金额1限值");

            //string[] str = new string[MeterNumber];
            //for (int i = 0; i < MeterNumber; i++)
            //{
            //    str[i] = Convert.ToInt64(0 * 100).ToString();
            //}
            //MeterProtocolAdapter.Instance.WriteData("报警金额1限值", str);

            //str = MeterProtocolAdapter.Instance.ReadData("报警金额1限值");
            //解除报警回因为报警预设金额限制不超过
            if (OneMeterInfo.FKType == 1) //只需要本地费控的情况
            {
                MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                int intMoney = (int)(float.Parse("150") * 100);
                MeterProtocolAdapter.Instance.InitPurse(intMoney);
                WaitTime("初始化报警功能", 15);
            }
            SendCostCommand(Cus_EncryptionTrialType.远程报警);

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                ResultDictionary["远程报警"][i] = "×";
                if (EncryptionThread.WorkThreads[i].RemoteControlResult)
                {
                    ResultDictionary["远程报警"][i] = "√";
                }
                else
                {
                    bResult[i] = false;
                }
            }
            RefUIData("远程报警");
            MessageAdd( "读取状态运行字3", EnumLogType.提示信息);
            string[] state3 = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                if (state3[i] == null || state3[i] == "") continue;


                ResultDictionary["远程报警状态字3"][i]= "×";

                if (MeterInfo[i].DgnProtocol.ClassName == "CDLT6452007")
                {
                    if ((Convert.ToInt32(state3[i], 16) & 0x80) == 0x80)
                        ResultDictionary["远程报警状态字3"][i] = "√";
                    else
                    {
                        bResult[i] = false;
                        NoResoult[i] = "远程报警状态字3不正确";
                    }

                }
                else if (MeterInfo[i].DgnProtocol.ClassName == "CDLT698")
                {
                    // 如0104 == 00000001 --00000100 == bit0-bit7 bit8--bit15
                    if ((Convert.ToInt32(state3[i], 16) & 0x100) == 0x100)
                        ResultDictionary["远程报警状态字3"][i] = "√";
                    else
                        bResult[i] = false;
                }
            }
            RefUIData("远程报警状态字3");

            SendCostCommand(Cus_EncryptionTrialType.解除报警);

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                ResultDictionary["解除报警"][i] = "×";
                if (EncryptionThread.WorkThreads[i].RemoteControlResult)
                {
                    ResultDictionary["解除报警"][i] = "√";
                }
                else
                {
                    bResult[i] = false;
                    NoResoult[i] = "解除报警状态字3不正确";
                }
            }
            RefUIData("解除报警");

            MessageAdd( "读取状态运行字3", EnumLogType.提示信息);
            

            state3 = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                if (state3[i] == null || state3[i] == "") continue;

                ResultDictionary["解除报警状态字3"][i] = "×";

                if (MeterInfo[i].DgnProtocol.ClassName == "CDLT6452007")
                {
                    if ((Convert.ToInt32(state3[i], 16) & 0x80) == 0x0)
                        ResultDictionary["解除报警状态字3"][i] = "√";
                    else
                        bResult[i] = false;

                }
                else if (MeterInfo[i].DgnProtocol.ClassName == "CDLT698")
                {
                    if ((Convert.ToInt32(state3[i], 16) & 0x100) == 0x0)
                        ResultDictionary["解除报警状态字3"][i] = "√";
                    else
                        bResult[i] = false;
                }

            }
            RefUIData("解除报警状态字3");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                ResultDictionary["结论"][i]= bResult[i] ? ConstHelper.合格 : ConstHelper.不合格;
         
            }
            //string[]  str = MeterProtocolAdapter.Instance.ReadData("报警金额1限值");
            //str = new string[MeterNumber];
            //for (int i = 0; i < MeterNumber; i++)
            //{
            //    str[i] = Convert.ToInt64(100*100).ToString();
            //}
            //MeterProtocolAdapter.Instance.WriteData("报警金额1限值", str);
            //str = MeterProtocolAdapter.Instance.ReadData("报警金额1限值");
            if (IsDoubleProtocol)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    MeterInfo[i].MD_ProtocolName = TemClassName[i];  //恢复成原来的协议
                }
                UpdateMeterProtocol();//每次开始检定，更新一下电表协议
            }
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "远程报警", "远程报警状态字3", "解除报警", "解除报警状态字3", "结论" };
            return true;
        }
    }
}
