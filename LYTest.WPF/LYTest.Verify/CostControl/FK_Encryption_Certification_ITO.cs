using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using LYTest.MeterProtocol;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 身份认证--计量芯片
    /// </summary>
    public class FK_Encryption_Certification_ITO : VerifyBase
    {
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("身份认证试验检定开始...", EnumLogType.流程信息);

            base.Verify();
            bool[] bResult = new bool[MeterNumber];
            if (Stop) return;


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = "身份认证";
            }
            RefUIData("当前项目");
            //初始化设备
            if (Stop) return;

            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;

            //获取所有表的表地址
            if (Stop) return;
            MessageAdd("正在获取所有表的表地址", EnumLogType.提示信息);
            ReadMeterAddrAndNo();

            if (Stop) return;
            if (IsDemo)
            {
                EncryptionThread = new ViewModel.CheckController.MulitThread.MulitEncryptionWorkThreadManager(MeterNumber / MeterProtocolManager.Instance.GetChannelCount())
                {
                    WorkThreads = new ViewModel.CheckController.MulitThread.EncryptionWorkThread[MeterNumber]
                };

                for (int i = 0; i < MeterNumber; i++)
                {
                    EncryptionThread.WorkThreads[i] = new ViewModel.CheckController.MulitThread.EncryptionWorkThread
                    {
                        CertificationResult = 1
                    };
                }
            }
            else
            {

                if (Stop) return;
                MessageAdd("正在进行计量芯身份认证", EnumLogType.提示信息);
                Identity(true, false);
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                string strResult = ConstHelper.不合格;

                if (meter.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    switch (EncryptionThread.WorkThreads[i].CertificationResult)
                    {
                        case 1:
                            strResult = "公钥合格";
                            break;
                        case 2:
                            strResult = "私钥合格";
                            break;
                    }
                    if (EncryptionThread.WorkThreads[i].CertificationResult == 0)
                    {
                        bResult[i] = false;
                        NoResoult[i] = "身份认证失败";
                    }
                    else
                        bResult[i] = true;
                }
                else if (meter.DgnProtocol.ProtocolName == "CDLT698")
                {
                    if (meter.SessionKey.Length > 0)
                    {
                        if (meter.EsamStatus == 0)
                            strResult = "公钥合格";
                        else
                            strResult = "私钥合格";
                        bResult[i] = true;
                    }
                }
                if (meter.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    if (EncryptionThread.WorkThreads[i].curIdentityInfo.EsamNo != null)
                    {
                        ResultDictionary["检定信息"][i] = BitConverter.ToString(EncryptionThread.WorkThreads[i].curIdentityInfo.EsamNo).Replace("-", "") + "(" + strResult + ")";
                    }
                }
                else if (meter.DgnProtocol.ProtocolName == "CDLT698")
                {
                    ResultDictionary["检定信息"][i] = strResult;
                }

                if (bResult[i])
                {
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }
                else
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }
            }

            RefUIData("检定信息");
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
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
