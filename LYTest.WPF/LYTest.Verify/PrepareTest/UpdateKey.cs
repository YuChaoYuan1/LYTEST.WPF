using LYTest.Core.Enum;
using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.PrepareTest
{

    /// <summary>
    /// 密钥更新 --检定前
    /// </summary>
    class UpdateKey : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            bool[] canExecute = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                canExecute[i] = true;
                ResultDictionary["当前项目"][i] = "密钥更新";
            }
            RefUIData("当前项目");
            bool[] bResult = new bool[MeterNumber];
            bool[] testBackUp = new bool[MeterNumber];
            bool[] resultArray = new bool[MeterNumber];
            string[] strKeyInfo = new string[MeterNumber];

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


            for (int i = 0; i < MeterNumber; i++)
            {
                testBackUp[i] = MeterInfo[i].YaoJianYn;
            }
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;

            if (!IsDemo)
            {

                ReadMeterAddrAndNo();   //获取所有表的表号
                if (Stop) return;
                for (int i = 0; i < 2; i++) //重复一次
                {
                    if (OneMeterInfo == null)
                    {
                        MessageAdd("请检测是否已勾选要检表....", EnumLogType.提示信息);
                        break;
                    }

                    if (Stop) return;

                    //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                    //Identity(); //身份认证
                    //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                    Identity(true); //身份认证

                    if (Stop) return;

                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄, canExecute);
                    }


                    if (VerifyConfig.Dog_Type == "国网加密机")
                    {
                        if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                        {
                            SendCostCommand(Cus_EncryptionTrialType.密钥更新_01, canExecute);
                            SendCostCommand(Cus_EncryptionTrialType.密钥更新_02, canExecute);
                            SendCostCommand(Cus_EncryptionTrialType.密钥更新_03, canExecute);
                            SendCostCommand(Cus_EncryptionTrialType.密钥更新_04, canExecute);
                            SendCostCommand(Cus_EncryptionTrialType.密钥更新_05, canExecute);
                        }
                        else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                        {
                            SendCostCommand(Cus_EncryptionTrialType.密钥更新_698, canExecute);

                            if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                            {
                                MeterLogicalAddressType = MeterLogicalAddressType == MeterLogicalAddressEnum.管理芯 ? MeterLogicalAddressEnum.计量芯 : MeterLogicalAddressEnum.管理芯;
                                MessageAdd("开始" + MeterLogicalAddressType + "密钥更新", EnumLogType.流程信息);
                                SendCostCommand(Cus_EncryptionTrialType.密钥更新_698, canExecute);
                            }


                        }
                    }
                    else
                    {
                        SendCostCommand(Cus_EncryptionTrialType.融通秘钥更新, canExecute);
                    }

                    if (VerifyConfig.Dog_Type == "国网加密机")
                    {
                        if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                        {
                            MessageAdd("正在进行【读取状态信息】操作...", EnumLogType.提示信息);

                            string[] keyInfo = MeterProtocolAdapter.Instance.ReadData("密钥状态");
                            for (int j = 0; j < MeterNumber; j++)
                            {
                                if (!MeterInfo[j].YaoJianYn) continue;
                                strKeyInfo[j] = keyInfo[j];
                            }

                        }
                        else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                        {
                            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                            //Identity();
                            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                            Identity(true);
                        }
                    }
                    else
                    {
                        SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄, canExecute);
                    }

                    #region 不合格表位重检
                    List<string> listRetry = new List<string>();
                    for (int j = 0; j < MeterNumber; j++)
                    {
                        if (!MeterInfo[j].YaoJianYn) continue;

                        //当前表位检定结论
                        //如果有表位检定不合格，则重检
                        //这里要将读回状态字加入到不合格的判断
                        resultArray[j] = EncryptionThread.WorkThreads[j].UpdateKeyResult;
                        if (resultArray[j])
                        {
                            canExecute[i] = false;
                            //meterInfo[j].YaoJianYn = false;
                        }
                        else
                        {
                            canExecute[i] = true;
                            //meterInfo[j].YaoJianYn = true;
                            listRetry.Add((j + 1).ToString());
                        }
                    }
                    if (listRetry.Count == 0)
                        break;
                    else
                        MessageAdd(string.Format("表位 {0} 执行第{1}次密钥更新失败!", string.Join(",", listRetry.ToArray()), i + 1), EnumLogType.提示信息);
                    #endregion
                }

                //恢复表位要检状态
                for (int i = 0; i < MeterNumber; i++)
                {
                    MeterInfo[i].YaoJianYn = testBackUp[i];
                }

                MeterProtocolAdapter.Instance.SecurityParameter(0x00); //不启用安全模式
            }


            string[] resultKey = new string[MeterNumber];
            object[] resultValue = new object[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                string result = ConstHelper.合格;
                if (IsDemo)
                {
                    bResult[i] = true;

                    //add yjt 20220320 新增演示模式数据和结论
                    meter.EsamKey = "7FFFFFFFF07F80FF8000000000000000";
                    ResultDictionary["结论"][i] = result;
                    ResultDictionary["检定信息"][i] = meter.EsamKey;
                }
                else
                {
                    if (meter.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        if (OneMeterInfo.DgnProtocol.HaveProgrammingkey)
                            bResult[i] = (EncryptionThread.WorkThreads[i].UpdateKeyResult);
                        else
                            bResult[i] = (strKeyInfo[i] != null && strKeyInfo[i].EndsWith("FFFFF"));
                    }
                    else if (meter.DgnProtocol.ClassName == "CDLT698")
                    {
                        bResult[i] = meter.EsamKey != "00000000000000000000000000000000";

                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                        {
                            MeterLogicalAddressType = MeterLogicalAddressType == MeterLogicalAddressEnum.管理芯 ? MeterLogicalAddressEnum.计量芯 : MeterLogicalAddressEnum.管理芯;
                            bResult[i] = meter.EsamKey != "00000000000000000000000000000000" && bResult[i];
                        }
                    }

                    result = bResult[i] ? ConstHelper.合格 : ConstHelper.不合格;

                    //modify yjt 20220320 修改将检定信息结论放在非演示模式里面
                    ResultDictionary["结论"][i] = result;
                    if (VerifyConfig.Dog_Type == "国网加密机")
                    {
                        if (meter.DgnProtocol.ClassName == "CDLT6452007")
                            ResultDictionary["检定信息"][i] = strKeyInfo[i];
                        else if (meter.DgnProtocol.ClassName == "CDLT698")
                        {

                            if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                            {
                                MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                                ResultDictionary["检定信息"][i] = meter.EsamKey;
                                MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
                                ResultDictionary["检定信息"][i] = ResultDictionary["检定信息"][i] + "," + meter.EsamKey;
                            }
                            else
                            {
                                ResultDictionary["检定信息"][i] = meter.EsamKey;

                            }

                        }

                    }
                    else
                    {
                        ResultDictionary["检定信息"][i] = EncryptionThread.WorkThreads[i].curKeyInfo.ToString();
                    }
                }

                //modify yjt 20220320 修改将检定信息结论放在非演示模式里面
                //ResultDictionary["结论"][i] = result;
                //if (VerifyConfig.Dog_Type == "国网加密机")
                //{
                //    if (meter.DgnProtocol.ClassName == "CDLT6452007")
                //        ResultDictionary["检定信息"][i] = strKeyInfo[i];
                //    else if (meter.DgnProtocol.ClassName == "CDLT698")
                //        ResultDictionary["检定信息"][i] = meter.EsamKey;
                //}
                //else
                //{
                //    ResultDictionary["检定信息"][i] = EncryptionThread.WorkThreads[i].curKeyInfo.ToString();
                //}
            }
            RefUIData("检定信息");
            RefUIData("结论");

            if (IsDoubleProtocol)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    MeterInfo[i].MD_ProtocolName = TemClassName[i];  //恢复成原来的协议
                }
                UpdateMeterProtocol();//每次开始检定，更新一下电表协议
            }
            MessageAdd("检定完成", EnumLogType.提示信息);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "当前项目", "检定信息", "结论" };
            return true;
        }
    }
}
