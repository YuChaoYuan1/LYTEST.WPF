using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.Meter;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 密钥更新
    /// </summary>
    class FK_Encryption_UpdateKey : VerifyBase
    {
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("密钥更新试验检定开始...", EnumLogType.流程信息);

            base.Verify();

            bool[] canExecute = new bool[MeterNumber];
            bool[] testBackUp = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                testBackUp[i] = MeterInfo[i].YaoJianYn;
                if (!MeterInfo[i].YaoJianYn) continue;
                canExecute[i] = true;
                ResultDictionary["当前项目"][i] = "密钥更新";
            }
            RefUIData("当前项目");
            bool[] bResult = new bool[MeterNumber];
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
                        ClassName = "CDLT698";
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
            //自动更新的条件
            if (VerifyConfig.FailureRate > 0 && ConfigHelper.Instance.VerifyModel == "自动模式")     //故障率不是0说明需要报警
            {
                bool[] IsResoult = new bool[MeterNumber];

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;
                    IsResoult[i] = GetMeterResult(i, Test_No);//获取表位的结论
                }
                int yaojianCount = EquipmentData.MeterGroupInfo.YaoJian.Count<bool>(a => a == true);//要检定的数量
                int OkCount = IsResoult.Count(a => a == true); //合格的数量
                if (yaojianCount < 1)
                {
                    MessageAdd($"停止密钥更新，要检表位数量为0。", EnumLogType.流程信息);
                    VerifyConfig.UpdateKeyAndUpdateData = false;
                    return;
                }
                float passRate = OkCount / (float)yaojianCount * 100;//n%
                if (yaojianCount < 1 || (100 - passRate) >= VerifyConfig.FailureRate)
                {
                    MessageAdd($"停止密钥更新，不合格率超过{VerifyConfig.FailureRate}%！", EnumLogType.流程信息);

                    if (DialogResult.No == MessageBox.Show($"{yaojianCount - OkCount}块不合格，不合格率超过{VerifyConfig.FailureRate}%，是否继续下装密钥？\n", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification))
                    {
                        VerifyConfig.UpdateKeyAndUpdateData = false;
                        return;
                    }
                }
            }

            if (Stop) return;

            //更新方式，更新哪些表位
            bool ShowMessage = false;
            for (int i = 0; i < MeterNumber; i++)
            {
                switch (VerifyConfig.KeyUpdataModel)
                {
                    case "全部更新":
                        break;
                    case "只更新合格表位":
                        if (!GetMeterResult(i, Test_No)) //如果表位不合格就不进行密钥更新
                        {
                            canExecute[i] = false;
                            //meterInfo[i].YaoJianYn = false;//避免改变录入值，引发额外问题
                        }
                        break;
                    case "弹出对话框等待":
                        if (!GetMeterResult(i, Test_No)) //如果表位不合格就不进行密钥更新
                        {
                            ShowMessage = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (VerifyConfig.KeyUpdataModel == "只更新合格表位")
            {
                bool IsHaveCheck = false;
                foreach (var item in canExecute)
                {
                    if (item)
                    {
                        IsHaveCheck = true;
                        break;
                    }
                }

                if (!IsHaveCheck)
                {
                    MessageAdd("所有表位均存在不合格项目，秘钥下装终止", EnumLogType.错误信息);
                    VerifyConfig.UpdateKeyAndUpdateData = false;
                    return;
                }
            }

            if (ShowMessage)
            {
                if (DialogResult.No == MessageBox.Show("有表位检定结果不合格，是否继续下装密钥？\n", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) return;
            }
            if (Stop) return;
            if (EquipmentData.StdInfo.Ua < U * 0.8 || EquipmentData.StdInfo.Ia > 0.0002 || EquipmentData.StdInfo.Ib > 0.0002 || EquipmentData.StdInfo.Ic > 0.0002)
            {
                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
            }
            if (Stop) return;

            if (ConfigHelper.Instance.VerifyModel == "手动模式" && ConfigHelper.Instance.IsKeyUpdate == "是")
            {
                if (DialogResult.No == MessageBox.Show("准备开始密钥更新，请确认是否继续？\n", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) return;
            }

            if (!IsDemo)
            {
                ReadMeterAddrAndNo();   //获取所有表的表号
                if (Stop) return;
                try
                {
                    for (int i = 0; i < 2; i++) //重复一次
                    {
                        if (OneMeterInfo == null)
                        {
                            MessageAdd("请检测是否已勾选要检表....", EnumLogType.提示信息);
                            VerifyConfig.UpdateKeyAndUpdateData = false;
                            break;
                        }

                        Identity(true); //身份认证

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
                                MessageAdd("开始" + MeterLogicalAddressType + "密钥更新", EnumLogType.流程信息);
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
                                MessageAdd("正在进行【读取状态信息】操作...", EnumLogType.流程信息);

                                string[] keyInfo = MeterProtocolAdapter.Instance.ReadData("密钥状态");
                                for (int j = 0; j < MeterNumber; j++)
                                {
                                    if (!MeterInfo[j].YaoJianYn || !canExecute[j]) continue;
                                    strKeyInfo[j] = keyInfo[j];
                                }

                            }
                            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                            {
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
                            if (!MeterInfo[j].YaoJianYn || !canExecute[j]) continue;

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

                    MeterProtocolAdapter.Instance.SecurityParameter(0x00); //不启用安全模式
                    VerifyConfig.UpdateKeyAndUpdateData = true;
                }
                finally
                {
                    //恢复表位要检状态
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        MeterInfo[i].YaoJianYn = testBackUp[i];
                    }
                }
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
                }
                else
                {
                    if (meter.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        if (OneMeterInfo.DgnProtocol.HaveProgrammingkey)
                            bResult[i] = EncryptionThread.WorkThreads[i].UpdateKeyResult;
                        else
                            bResult[i] = strKeyInfo[i] != null && (strKeyInfo[i].EndsWith("FFFFF") || strKeyInfo[i].EndsWith("F"));
                    }
                    else if (meter.DgnProtocol.ClassName == "CDLT698")
                    {
                        bResult[i] = !string.IsNullOrWhiteSpace(meter.EsamKey) && meter.EsamKey != "00000000000000000000000000000000";

                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                        {
                            MeterLogicalAddressType = MeterLogicalAddressType == MeterLogicalAddressEnum.管理芯 ? MeterLogicalAddressEnum.计量芯 : MeterLogicalAddressEnum.管理芯;
                            bResult[i] = meter.EsamKey != "00000000000000000000000000000000" && bResult[i];
                        }
                    }

                    result = bResult[i] ? ConstHelper.合格 : ConstHelper.不合格;
                    if (!bResult[i])
                        NoResoult[i] = "ESAM密钥信息不正确";
                }

                ResultDictionary["结论"][i] = result;
                if (VerifyConfig.Dog_Type == "国网加密机")
                {
                    if (meter.DgnProtocol.ClassName == "CDLT6452007")
                        ResultDictionary["检定信息"][i] = strKeyInfo[i];
                    else if (meter.DgnProtocol.ClassName == "CDLT698")
                    {
                        MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                        {
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

                //add
                if (IsDemo)
                {
                    ResultDictionary["检定信息"][i] = "7FFFFFFFF07F80FF8000000000000000";
                }
            }
            RefUIData("检定信息");
            RefUIData("结论");

            if (IsDoubleProtocol) //IR46表,并且需要检定前进行加密解密
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    MeterInfo[i].MD_ProtocolName = TemClassName[i];  //恢复成原来的协议
                }
                UpdateMeterProtocol();//每次开始检定，更新一下电表协议
            }
            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("密钥更新试验检定结束...", EnumLogType.流程信息);
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
