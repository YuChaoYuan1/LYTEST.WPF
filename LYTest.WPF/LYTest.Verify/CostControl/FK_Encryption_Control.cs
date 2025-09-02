using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.CostControl
{

    /// <summary>
    /// 远程控制 远程拉合闸
    /// </summary>
    class FK_Encryption_Control : VerifyBase
    {

        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("远程控制验检定开始...", EnumLogType.流程信息);

            base.Verify();

            if (Stop) return;
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            string[] strRun = new string[MeterNumber];
            //string keyitem = Cus_CostControlItem.远程控制;
            //string[] resultKey = new string[MeterNumber];
            //object[] resultValue = new object[MeterNumber];

            bool[] bSumResult = new bool[MeterNumber];

            //获取所有表的表地址
            if (Stop) return;
            ReadMeterAddrAndNo();

            if (IsDemo)
            {
                EncryptionThread = new ViewModel.CheckController.MulitThread.MulitEncryptionWorkThreadManager(MeterNumber / LYTest.MeterProtocol.MeterProtocolManager.Instance.GetChannelCount())
                {
                    WorkThreads = new ViewModel.CheckController.MulitThread.EncryptionWorkThread[MeterNumber]
                };

                for (int i = 0; i < MeterNumber; i++)
                {
                    EncryptionThread.WorkThreads[i] = new ViewModel.CheckController.MulitThread.EncryptionWorkThread
                    {
                        RemoteControlResult = true
                    };
                }
            }
            else
            {
                if (Stop) return;

                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                //Identity();
                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                Identity(true);

                if (Stop) return;
                MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
                DateTime readTime = DateTime.Now;  //读取GPS时间

                MessageAdd("开始写表时间......", EnumLogType.提示信息);
                if (Stop) return;
                MeterProtocolAdapter.Instance.WriteDateTime(readTime);
                ResultDictionary["写时间"].Fill(readTime.ToString());
                RefUIData("写时间");
                if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯 ;

                if (Stop) return;
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                    SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄);

                if (Stop) return;
                SendCostCommand(Cus_EncryptionTrialType.解除保电);
                ResultDictionary["解除保电"].Fill("√");
                RefUIData("解除保电");
                if (Stop) return;
                SendCostCommand(Cus_EncryptionTrialType.远程拉闸);
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (EncryptionThread.WorkThreads[i].RemoteControlResult)
                {
                    ResultDictionary["拉闸"][i] = "√";
                }
                else
                {
                    ResultDictionary["拉闸"][i] = "×";
                    NoResoult[i] = "拉闸失败";
                    bSumResult[i] = false;
                }
            }
            RefUIData("拉闸");
            if (!IsDemo)
            {
                WaitTime("拉闸", 60);
            }


            MessageAdd("读取状态运行字3", EnumLogType.提示信息);
            string[] strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (IsDemo)
                    ResultDictionary["拉闸状态字3"][i] = "√";
                else
                {
                    if (strReadData[i] == null || strReadData[i] == "")
                    {
                        strRun[i] = "×";
                        bSumResult[i] = false;
                    }
                    else
                    {
                        int chr = Convert.ToInt16(strReadData[i], 16);
                        if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                            chr = Number.BitRever(chr, 16);

                        if ((chr & 0x40) == 0x40)
                        {
                            ResultDictionary["拉闸状态字3"][i] = "√";
                            bSumResult[i] = true;
                        }
                        else
                        {
                            ResultDictionary["拉闸状态字3"][i] = "×";
                            NoResoult[i] = "拉闸状态字3不符";
                            bSumResult[i] = false;
                        }
                    }
                }
            }
            RefUIData("拉闸状态字3");

            SendCostCommand(Cus_EncryptionTrialType.远程合闸);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (EncryptionThread.WorkThreads[i].RemoteControlResult)
                {
                    ResultDictionary["合闸"][i] = "√";
                }
                else
                {
                    ResultDictionary["合闸"][i] = "×";
                    NoResoult[i] = "合闸失败";
                    bSumResult[i] = false;
                }
            }
            RefUIData("合闸");

            if (!IsDemo)
            {
                MessageAdd("读取状态运行字3", EnumLogType.提示信息);
                strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (IsDemo)
                {
                    ResultDictionary["合闸状态字3"][i] = "√";
                }
                else
                {
                    if (strReadData[i] == null || strReadData[i] == "")
                    {
                        ResultDictionary["合闸状态字3"][i] = "×";
                        NoResoult[i] = "没有读取到合闸状态字3";
                        bSumResult[i] = false;
                    }
                    else
                    {
                        int chr = Convert.ToInt32(strReadData[i], 16);

                        if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                            chr = Number.BitRever(chr, 16);

                        if ((chr & 0x40) == 0x40)
                        {
                            ResultDictionary["合闸状态字3"][i] = "×";
                            NoResoult[i] = "合闸状态字3不符";
                            bSumResult[i] = false;
                        }
                        else
                        {
                            ResultDictionary["合闸状态字3"][i] = "√";
                        }
                    }
                }
            }
            RefUIData("合闸状态字3");


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (Stop) return;
                ResultDictionary["结论"][i] = bSumResult[i] ? ConstHelper.合格 : ConstHelper.不合格;
            }
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("远程控制验检定结束...", EnumLogType.流程信息);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "写时间","解除保电","拉闸","拉闸状态字3","拉闸电表状态","合闸","合闸状态字3","合闸电表状态", "结论" };
            return true;
        }
    }
}
