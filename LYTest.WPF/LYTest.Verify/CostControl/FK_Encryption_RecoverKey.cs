using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.Meter;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 密钥恢复
    /// </summary>
    class FK_Encryption_RecoverKey : VerifyBase
    {

        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("密钥恢复试验检定开始...", EnumLogType.流程信息);

            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = "密钥恢复";
            }
            RefUIData("当前项目");
            if (Stop) return;
            string[] strKeyInfo = new string[MeterNumber];
            bool[] bResult = new bool[MeterNumber];

            string[] TemClassName = new string[MeterNumber];
            if (IsDoubleProtocol) //IR46表,并且需要检定前进行加密解密
            {
                MessageAdd($"双协议{VerifyConfig.Test_CryptoModel}", EnumLogType.流程信息);
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

            if (!IsDemo)
            {

                if (Stop) return;
                //初始化设备
                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示与流程信息);
                    return;
                }

                if (Stop) return;
                //获取所有表的表号
                ReadMeterAddrAndNo();

                if (Stop) return;

                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                //Identity();
                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                Identity(true);

                if (Stop) return;

                //第一次读取密钥状态字
                //'-------------------------------------回抄芯片发行信息文文件 用于调用密钥更新方法------------------------------------------
                //'PutChipInfor  芯片发行信息文件(001A 文件)数据,通过,078001FF 命令从电表ESAM 抄读所得,005AH字节
                //'数据回抄标识共8字节、4部分组成，数据排列如下表所示：目录标识(2字节)+文件标识(2字节)+相对起始地址(2字节)+读取数据长度(2字节)

                //        '输入参数(远程密钥信息文件)
                //        "DF01001A0000005A"

                //"正在读取ESAM模块数据..."

                //密钥状态字3
                //"04000508"
                MessageAdd($"{VerifyConfig.Dog_Type},{OneMeterInfo.DgnProtocol.ClassName}", EnumLogType.流程信息);
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄);
                }
                if (VerifyConfig.Dog_Type == "国网加密机")
                {
                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        SendCostCommand(Cus_EncryptionTrialType.密钥恢复_01);
                        SendCostCommand(Cus_EncryptionTrialType.密钥恢复_02);
                        SendCostCommand(Cus_EncryptionTrialType.密钥恢复_03);
                        SendCostCommand(Cus_EncryptionTrialType.密钥恢复_04);
                        SendCostCommand(Cus_EncryptionTrialType.密钥恢复_05);
                    }
                    else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                    {
                        SendCostCommand(Cus_EncryptionTrialType.密钥恢复_698);
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                        {
                            MeterLogicalAddressType = MeterLogicalAddressType == MeterLogicalAddressEnum.管理芯 ? MeterLogicalAddressEnum.计量芯 : MeterLogicalAddressEnum.管理芯;
                            MessageAdd("开始" + MeterLogicalAddressType + "密钥恢复", EnumLogType.流程信息);
                            SendCostCommand(Cus_EncryptionTrialType.密钥恢复_698);
                        }
                    }
                }
                else
                {
                    SendCostCommand(Cus_EncryptionTrialType.密钥恢复);
                }


                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    //MessageAdd("等待5秒后读取密钥状态字...", EnumLogType.提示信息);
                    WaitTime("等待5秒后读取密钥状态字...", 5);
                    strKeyInfo = MeterProtocolAdapter.Instance.ReadData("密钥状态");
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    MessageAdd("恢复后身份认证", EnumLogType.流程信息);
                    Identity(true); //身份认证
                }
            }

            //string[] strResultKey = new string[MeterNumber];
            //object[] objResultValue = new object[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;

                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                string strResult = ConstHelper.合格;
                if (IsDemo)
                {
                    bResult[i] = true;
                }
                else
                {
                    if (meter.DgnProtocol.ClassName == "CDLT6452007")
                        bResult[i] = EncryptionThread.WorkThreads[i].CoverKeyResult && strKeyInfo[i] != null && strKeyInfo[i].EndsWith("00000000");
                    else if (meter.DgnProtocol.ClassName == "CDLT698")
                    {
                        bResult[i] = meter.EsamKey == "00000000000000000000000000000000";
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                        {
                            MeterLogicalAddressType = MeterLogicalAddressType == MeterLogicalAddressEnum.管理芯 ? MeterLogicalAddressEnum.计量芯 : MeterLogicalAddressEnum.管理芯;
                            bResult[i] = meter.EsamKey == "00000000000000000000000000000000" && bResult[i];
                        }
                    }

                    if (!bResult[i]) strResult = ConstHelper.不合格;
                }

                if (!bResult[i])
                    NoResoult[i] = "ESAM密钥信息不正确";

                ResultDictionary["结论"][i] = strResult;
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
                if (IsDemo)
                {
                    ResultDictionary["检定信息"][i] = "00000000000000000000000000000000";
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
                UpdateMeterProtocol();//更新一下电表协议
            }

            // 吉林省时,恢复密钥时清空该表所有检定数据
            if (ConfigHelper.Instance.KeyCheckInGiLin)
            {
                EquipmentData.CheckResults.ClearAllResult();

                for(int i=0;i< MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn)continue;
                    CheckResultBll.Instance.DeleteResultFromTempDb_JLJM(i);
                }

            }

            MessageAdd("密钥恢复试验检定结束...", EnumLogType.提示与流程信息);
        }

        #region IWorkDone 成员

        /// <summary>
        /// 等待其它操作完成
        /// </summary>
        public void WorkDone()
        {

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
        #endregion

    }
}
