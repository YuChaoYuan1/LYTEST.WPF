using LYTest.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LYTest.MeterProtocol.Encryption;
using LYTest.Core.Struct;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.MeterProtocol.Struct;
using LYTest.Core.Model.Meter;

namespace LYTest.ViewModel.CheckController.MulitThread
{
    public class EncryptionWorkThread
    {
        #region 定义
        /// <summary>
        /// 当前密钥更新类型
        /// </summary>
        public Cus_UpdateKeyType curUpdateKeyType;

        /// <summary>
        /// 身份认证信息
        /// </summary>
        public StIdentityInfo curIdentityInfo;
        /// <summary>
        /// 当前密钥信息
        /// </summary>
        public StKeyInfo curKeyInfo;
        /// <summary>
        /// 密钥更新信息
        /// </summary>
        public StKeyUpdateInfo curKeyUpdateInfo;
        /// <summary>
        /// 当前测试类型
        /// </summary>
        private Cus_EncryptionTrialType curTrialType;

        /// <summary>
        /// 
        /// </summary>
        private Thread workThread = null;

        /// <summary>
        /// 运行标志
        /// </summary>
        private bool IsStop { get; set; }

        /// <summary>
        /// 工作完成标志
        /// </summary>
        private bool workOverFlag = false;

        /// <summary>
        /// 当前对应表信息
        /// </summary>
        public TestMeterInfo Meter { get; set; }

        /// <summary>
        /// 当前通道序列
        /// </summary>
        public int CurrentChannelIndex { get; set; }

        /// <summary>
        /// 该表位远程密钥清零结果:True-成功，False-失败
        /// </summary>
        public bool ClearKeyResult { get; set; }
        /// <summary>
        /// 该表位远程密钥恢复结果:True-成功，False-失败
        /// </summary>
        public bool CoverKeyResult { get; set; }
        /// <summary>
        /// 该表位远程更新密钥结果:True-成功，False-失败
        /// </summary>
        public bool UpdateKeyResult { get; set; }

        /// <summary>
        /// 身份认证结果:True-成功，False-失败
        /// </summary>
        public int CertificationResult { get; set; }

        /// <summary>
        /// 远程拉合闸结果
        /// </summary>
        public bool RemoteControlResult { get; set; }

        /// <summary>
        /// 98级电量清零
        /// </summary>
        public bool ClearEnergyResult { get; set; }
        #endregion

        /// <summary>
        /// 停止当前工作任务
        /// </summary>
        public void Stop()
        {
            IsStop = true;
        }

        /// <summary>
        /// 工作线程是否完成
        /// </summary>
        /// <returns></returns>
        public bool IsWorkFinish()
        {
            return workOverFlag;
        }

        /// <summary>
        /// 启动工作线程
        /// </summary>
        /// <param name="paras"></param>
        public void Start(Cus_EncryptionTrialType trialType)
        {
            curTrialType = trialType;

            //add yjt 20220621 新增当表位已经是私钥和公钥 不密钥更新和恢复
            if (trialType.ToString().IndexOf("密钥更新") != -1 && Meter.EsamStatus == 1)
            {
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位已经是私钥状态，跳过密钥更新", EnumLogType.提示与流程信息);
                UpdateKeyResult = true;
                workOverFlag = true;
                return;
            }
            else if (trialType.ToString().IndexOf("密钥恢复") != -1 && Meter.EsamStatus == 0)
            {
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位已经是公钥状态，跳过密钥恢复", EnumLogType.提示与流程信息);
                CoverKeyResult = true;
                workOverFlag = true;
                return;
            }

            switch (trialType)
            {
                case Cus_EncryptionTrialType.身份认证:
                    workThread = new Thread(StartWorkForCertification);
                    break;
                case Cus_EncryptionTrialType.密钥恢复:
                    workThread = new Thread(StartWorkForClearKeyInfo);
                    break;
                case Cus_EncryptionTrialType.密钥更新:
                    workThread = new Thread(StartWorkForUpdateKey);
                    break;
                case Cus_EncryptionTrialType.主控密钥更新:
                    curUpdateKeyType = Cus_UpdateKeyType.主控密钥;
                    workThread = new Thread(StartWorkForUpdateOneKey);
                    break;
                case Cus_EncryptionTrialType.远程密钥更新:
                    curUpdateKeyType = Cus_UpdateKeyType.远程密钥;
                    workThread = new Thread(StartWorkForUpdateOneKey);
                    break;
                case Cus_EncryptionTrialType.参数密钥更新:
                    curUpdateKeyType = Cus_UpdateKeyType.参数密钥;
                    workThread = new Thread(StartWorkForUpdateOneKey);
                    break;
                case Cus_EncryptionTrialType.身份密钥更新:
                    curUpdateKeyType = Cus_UpdateKeyType.身份密钥;
                    workThread = new Thread(StartWorkForUpdateOneKey);
                    break;
                case Cus_EncryptionTrialType.融通秘钥更新:
                    workThread = new Thread(StartRongTongUpDateKey);
                    break;
                case Cus_EncryptionTrialType.密钥更新_01:
                case Cus_EncryptionTrialType.密钥更新_02:
                case Cus_EncryptionTrialType.密钥更新_03:
                case Cus_EncryptionTrialType.密钥更新_04:
                case Cus_EncryptionTrialType.密钥更新_05:
                    workThread = new Thread(StartWorkForUpdateOneKey);
                    break;
                case Cus_EncryptionTrialType.密钥恢复_01:
                case Cus_EncryptionTrialType.密钥恢复_02:
                case Cus_EncryptionTrialType.密钥恢复_03:
                case Cus_EncryptionTrialType.密钥恢复_04:
                case Cus_EncryptionTrialType.密钥恢复_05:
                    workThread = new Thread(StartWorkForRecoverOneKey);
                    break;
                case Cus_EncryptionTrialType.远程拉闸:
                case Cus_EncryptionTrialType.远程合闸:
                case Cus_EncryptionTrialType.远程保电:
                case Cus_EncryptionTrialType.解除保电:
                case Cus_EncryptionTrialType.远程报警:
                case Cus_EncryptionTrialType.解除报警:
                case Cus_EncryptionTrialType.直接合闸:
                    if (VerifyBase.OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                        workThread = new Thread(StartWorkForUserControl);
                    else if (VerifyBase.OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                        workThread = new Thread(StartWorkForUserControl698);
                    break;
                case Cus_EncryptionTrialType.电量清零:
                    break;
                case Cus_EncryptionTrialType.ESAM数据回抄:
                    workThread = new Thread(StartWorkForReadEsam);
                    break;
                case Cus_EncryptionTrialType.密钥更新_698:
                    workThread = new Thread(StartWorkForUpdateKey698);
                    break;
                case Cus_EncryptionTrialType.密钥恢复_698:
                    workThread = new Thread(StartWorkForRecoverKey698);
                    break;

            }
            workThread.Start();
        }

        /// <summary>
        /// 身份认证
        /// </summary>
        private void StartWorkForCertification()
        {
            //初始化标志
            IsStop = false;
            workOverFlag = false;
            CertificationResult = 0;

            curIdentityInfo = new StIdentityInfo();
            //身份验证

            EquipmentData.Controller.MessageAdd("正在进行安全认证...", EnumLogType.提示信息);
            CertificationResult = IdentityAuthen645(out curIdentityInfo.Rand, out curIdentityInfo.EsamNo);

            if (CertificationResult == 0)
            {
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位首次安全认证失败!...", EnumLogType.提示信息);
            }
            else
            {
                Meter.EsamStatus = CertificationResult == 1 ? 0 : 1;
                Meter.Rand = BitConverter.ToString(curIdentityInfo.Rand).Replace("-", "");
                Meter.EsamId = BitConverter.ToString(curIdentityInfo.EsamNo).Replace("-", "");
            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 读取ESAM信息
        /// </summary>
        private void StartWorkForReadEsam()
        {
            //初始化标志
            IsStop = false;
            workOverFlag = false;

            //读取ESAM模块信息
            EquipmentData.Controller.MessageAdd("读取ESAM模块信息...", EnumLogType.提示信息);
            if (!ReadESAMDataInfo(ref curKeyInfo))
            {
                if (!ReadESAMDataInfo(ref curKeyInfo))
                {
                    EquipmentData.Controller.MessageAdd($"第{ Meter.MD_Epitope}表位读取ESAM失败...", EnumLogType.提示信息);
                }
            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 远程控制
        /// </summary>
        private void StartWorkForUserControl()
        {
            //初始化标志
            IsStop = false;
            workOverFlag = false;

            string data = "1A";
            if (curTrialType == Cus_EncryptionTrialType.远程拉闸)
                data = "1A";
            else if (curTrialType == Cus_EncryptionTrialType.远程合闸)
                data = "1B";
            else if (curTrialType == Cus_EncryptionTrialType.直接合闸)
                data = "1C";
            else if (curTrialType == Cus_EncryptionTrialType.远程报警)
                data = "2A";
            else if (curTrialType == Cus_EncryptionTrialType.解除报警)
                data = "2B";
            else if (curTrialType == Cus_EncryptionTrialType.远程保电)
                data = "3A";
            else if (curTrialType == Cus_EncryptionTrialType.解除保电)
                data = "3B";

            //发送控制命令
            string msg = string.Format("发送控制【{0}】命令...", curTrialType.ToString());
            EquipmentData.Controller.MessageAdd(msg, EnumLogType.提示信息);
            data += "00" + DateTime.Now.AddMinutes(5).ToString("yyMMddHHmmss");
            //if (App.MethodAndBasic.IsNeiMengWarningPrice)//内蒙
            //{
            //    data += "00" + DateTime.Now.AddMinutes(5).ToString("ssmmHHddMMyy");
            //}
            //else
            //{
            //    data += "00" + DateTime.Now.AddMinutes(5).ToString("yyMMddHHmmss");
            //}
            //if (App.UserSetting.EncryptionType == EncryType.融通加密机)
            //{
            //    data += (Convert.ToInt16(curKeyInfo.KeyVer) - 1).ToString().PadLeft(2, '0');
            //}

            RemoteControlResult = UserControl(curIdentityInfo.Rand, curIdentityInfo.EsamNo, data);
            if (!RemoteControlResult)
            {
                RemoteControlResult = UserControl(curIdentityInfo.Rand, curIdentityInfo.EsamNo, data);
                if (!RemoteControlResult)
                    EquipmentData.Controller.MessageAdd($"第{ Meter.MD_Epitope}表位{msg}失败...", EnumLogType.提示信息);
            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 密钥更新
        /// </summary>
        private void StartWorkForUpdateKey()
        {
            //初始化标志
            IsStop = false;
            workOverFlag = false;
            UpdateKeyResult = false;


            if (Meter.DgnProtocol.HaveProgrammingkey)  //09密钥
            {

                //身份验证
                EquipmentData.Controller.MessageAdd("正在进行安全认证...", EnumLogType.提示信息);

                CertificationResult = IdentityAuthen645(out byte[] rand2, out byte[] esamNo);
                if (CertificationResult == 0)
                {
                    EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位首次安全认证失败!...", EnumLogType.提示信息);
                    IsStop = true;
                    workOverFlag = true;
                    return;
                }

                if (IsStop) return;
                //读取ESAM模块信息

                EquipmentData.Controller.MessageAdd("读取ESAM模块信息...", EnumLogType.提示信息);
                if (!ReadESAMDataInfo(ref curKeyInfo))
                {
                    if (!ReadESAMDataInfo(ref curKeyInfo))
                    {
                        EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位读取ESAM模块信息失败...", EnumLogType.提示信息);
                        IsStop = true;
                        workOverFlag = true;
                        return;
                    }
                }

                if (curKeyInfo.KeyVer != 0)
                {
                    if (IsStop) return;
                    //执行密钥版本信息清零操作
                    EquipmentData.Controller.MessageAdd("执行密钥版本信息清零操作...", EnumLogType.提示信息);

                    if (!ClearKeyInfo(rand2, esamNo))
                        EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位密钥版本信息清零失败...", EnumLogType.提示信息);

                    if (IsStop) return;
                    //读取ESAM模块信息
                    EquipmentData.Controller.MessageAdd("读取ESAM模块信息...", EnumLogType.提示信息);

                    if (!ReadESAMDataInfo(ref curKeyInfo))
                    {
                        if (!ReadESAMDataInfo(ref curKeyInfo))
                        {
                            EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位读取ESAM失败...", EnumLogType.错误信息);
                            IsStop = true;
                            workOverFlag = true;
                            return;
                        }
                    }
                }

                if (IsStop) return;
                //密钥更新
                EquipmentData.Controller.MessageAdd("正在进行远程密钥更新操作...", EnumLogType.提示信息);

                if (!UpdataEncyKey645_09(rand2, esamNo, curKeyInfo))
                {

                    if (!UpdataEncyKey645_09(rand2, esamNo, curKeyInfo))
                    {
                        EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位远程密钥更新失败...", EnumLogType.错误信息);
                        IsStop = true;
                        workOverFlag = true;
                        return;
                    }
                }
                UpdateKeyResult = true;
            }
            else
            {
                if (IsStop) return;
                //执行密钥版本信息恢复操作
                EquipmentData.Controller.MessageAdd("执行密钥更新操作...", EnumLogType.提示信息);

                if (!UpdataEncyKey645_13(curIdentityInfo.Rand, curIdentityInfo.EsamNo, curKeyInfo.EsamCoreInfo))
                {
                    EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位密钥更新失败...", EnumLogType.错误信息);
                    IsStop = true;
                    workOverFlag = true;
                    return;
                }
                UpdateKeyResult = true;
            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 密钥清零
        /// </summary>
        private void StartWorkForClearKeyInfo()
        {
            //初始化标志
            IsStop = false;
            workOverFlag = false;
            ClearKeyResult = false;

            if (Meter.DgnProtocol.HaveProgrammingkey)
            {
                if (curKeyInfo.KeyVer != 0)
                {
                    if (IsStop) return;
                    //执行密钥版本信息清零操作
                    EquipmentData.Controller.MessageAdd("执行密钥版本信息清零操作...", EnumLogType.提示信息);
                    //Thread.Sleep(300);
                    if (!ClearKeyInfo(curIdentityInfo.Rand, curIdentityInfo.EsamNo))
                    {
                        EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位密钥信息清零失败...", EnumLogType.错误信息);
                    }

                    if (IsStop) return;
                    //读取ESAM模块信息
                    EquipmentData.Controller.MessageAdd("读取ESAM模块信息...", EnumLogType.提示信息);
                    //Thread.Sleep(300);
                    if (!ReadESAMDataInfo(ref curKeyInfo))
                    {
                        //Thread.Sleep(300);
                        if (!ReadESAMDataInfo(ref curKeyInfo))
                        {
                            EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位读取ESAM失败...", EnumLogType.错误信息);
                            IsStop = true;
                            workOverFlag = true;
                            return;
                        }
                    }
                }
                ClearKeyResult = true;
            }
            else//新表
            {
                if (IsStop) return;
                //执行密钥版本信息清零操作
                EquipmentData.Controller.MessageAdd("执行密钥恢复操作...", EnumLogType.提示信息);
                //Thread.Sleep(300);
                if (!RecoverKeyInfo(curIdentityInfo.Rand, curIdentityInfo.EsamNo, curKeyInfo.EsamCoreInfo))
                {
                    EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位密钥恢复失败...", EnumLogType.错误信息);

                    IsStop = true;
                    workOverFlag = true;
                    return;
                }
                ClearKeyResult = true;

            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 密钥恢复
        /// </summary>
        private void StartWorkForRecoverOneKey()
        {
            //初始化标志
            IsStop = false;
            workOverFlag = false;
            CoverKeyResult = false;

            if (Meter.DgnProtocol.HaveProgrammingkey)
            {
                CoverKeyResult = true;
            }
            else//新表
            {
                if (IsStop) return;
                //执行密钥版本信息恢复操作
                EquipmentData.Controller.MessageAdd(string.Format("正在进行【{0}】操作...", curTrialType), EnumLogType.提示信息);

                if (!RecoverKeyInfoForOne(curIdentityInfo.Rand, curIdentityInfo.EsamNo, curKeyInfo.EsamCoreInfo))
                {
                    EquipmentData.Controller.MessageAdd(string.Format("第{0}表位{1}失败...", Meter.MD_Epitope, curTrialType), EnumLogType.提示信息);
                    IsStop = true;
                    workOverFlag = true;
                    return;
                }
                CoverKeyResult = true;

            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 密钥恢复
        /// </summary>
        /// <param name="rand2">随机数2</param>
        /// <param name="esamNo">Esam值</param>
        /// <param name="chipInfor">芯片信息</param>
        /// <returns></returns>
        private bool RecoverKeyInfo(byte[] rand2, byte[] esamNo, string chipInfor)
        {
            //执行密钥恢复函数
            string r2 = BitConverter.ToString(rand2).Replace("-", "");
            string esamno = BitConverter.ToString(esamNo).Replace("-", "");
            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');
            string msg = "";
            string[] keyID = new string[] { "00010203", "04050607", "08090A0B", "0C0D0E0F", "10111213" };

            bool rst = true;
            for (int i = 0; i < 5; i++)
            {
                string outData = "";
                //密钥恢复用00，密钥更新用01
                bool r = EncrypGW.Meter_Formal_KeyUpdateV2(20, "00", keyID[i], r2, MD_MeterNo, esamno, chipInfor, ref outData, ref msg);
                if (!r)
                {
                    rst = false;
                    continue;
                }
                string mac = outData.Substring(288, 8);
                string encry = outData.Substring(0, 288);
                string keyinfo1 = "";
                for (int j = 0; j < 4; j++)
                {
                    string tmp = encry.Substring(j * 72, 72);
                    keyinfo1 += DxString(tmp.Substring(8, 64)) + DxString(tmp.Substring(0, 8));
                }

                //密钥清零 .或更新
                //Thread.Sleep(600);
                if (MeterProtocolAdapter.Instance.UpdateKeyInfo(Meter.MD_Epitope - 1, mac, DxString(keyinfo1)))
                {
                    EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 第{1}条密钥恢复成功", Meter.MD_Epitope, i + 1), EnumLogType.提示信息);
                }
                else
                {
                    EquipmentData.Controller.MessageAdd($"第{ Meter.MD_Epitope}表位 第{ i + 1}条密钥恢复失败", EnumLogType.错误信息);

                    //EquipmentData.Controller.MessageAdd( string.Format("第{0}表位 第{1}条密钥恢复失败", Meter.MD_Epitope, i + 1), EnumLogType.提示信息);
                    rst = false;
                    break;
                }
            }


            return rst;
        }

        /// <summary>
        /// 密钥恢复
        /// </summary>
        /// <param name="rand2">随机数2</param>
        /// <param name="esamNo">Esam值</param>
        /// <param name="chipInfor">芯片信息</param>
        /// <returns></returns>
        private bool RecoverKeyInfoForOne(byte[] rand2, byte[] esamNo, string chipInfor)
        {
            //执行密钥更新函数
            string r2 = BitConverter.ToString(rand2).Replace("-", "");
            string esamno = BitConverter.ToString(esamNo).Replace("-", "");
            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');
            string msg = "";
            string outData = "";

            string encyKey = "";
            switch (curTrialType)
            {
                case Cus_EncryptionTrialType.密钥恢复_01:
                    encyKey = "00010203";
                    break;
                case Cus_EncryptionTrialType.密钥恢复_02:
                    encyKey = "04050607";
                    break;
                case Cus_EncryptionTrialType.密钥恢复_03:
                    encyKey = "08090A0B";
                    break;
                case Cus_EncryptionTrialType.密钥恢复_04:
                    encyKey = "0C0D0E0F";
                    break;
                case Cus_EncryptionTrialType.密钥恢复_05:
                    encyKey = "10111213";
                    break;
            }

            //密钥恢复用00，密钥更新用01
            bool r = EncrypGW.Meter_Formal_KeyUpdateV2(20, "00", encyKey, r2, MD_MeterNo, esamno, chipInfor, ref outData, ref msg);
            if (!r) return false;

            string mac = outData.Substring(288, 8);
            string encry = outData.Substring(0, 288);
            string keyinfo1 = "";
            for (int i = 0; i < 4; i++)
            {
                string tmp = encry.Substring(i * 72, 72);
                keyinfo1 += DxString(tmp.Substring(8, 64)) + DxString(tmp.Substring(0, 8));
            }

            //密钥更新
            //Thread.Sleep(300);
            if (MeterProtocolAdapter.Instance.UpdateKeyInfo(Meter.MD_Epitope - 1, mac, DxString(keyinfo1)))
            {
                EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 {1}成功", Meter.MD_Epitope, curTrialType), EnumLogType.提示信息);
                return true;
            }
            else
            {
                EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 {1}失败", Meter.MD_Epitope, curTrialType), EnumLogType.错误信息);

                return false;

            }
        }

        /// <summary>
        /// 密钥更新
        /// </summary>
        /// <param name="rand2">随机数</param>
        /// <param name="esamNo">Esam值</param>
        /// <param name="chipInfor">芯片信息</param>
        /// <returns></returns>
        private bool UpdataEncyKey645_13(byte[] rand2, byte[] esamNo, string chipInfor)
        {

            //执行密钥更新函数
            string r2 = BitConverter.ToString(rand2).Replace("-", "");
            string esamno = BitConverter.ToString(esamNo).Replace("-", "");
            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');
            string msg = "";

            string[] keyID = new string[] { "00010203", "04050607", "08090A0B", "0C0D0E0F", "10111213" };

            bool rst = true;
            for (int i = 0; i < 5; i++)
            {
                string outData = "";
                //密钥恢复用00，密钥更新用01
                bool r = EncrypGW.Meter_Formal_KeyUpdateV2(20, "01", keyID[i], r2, MD_MeterNo, esamno, chipInfor, ref outData, ref msg);
                if (!r)
                {
                    rst = false;
                    continue;
                }

                string mac = outData.Substring(288, 8);
                string encry = outData.Substring(0, 288);
                string keyinfo1 = "";
                for (int j = 0; j < 4; j++)
                {
                    string tmp = encry.Substring(j * 72, 72);
                    keyinfo1 += DxString(tmp.Substring(8, 64)) + DxString(tmp.Substring(0, 8));
                }

                //密钥更新
                //Thread.Sleep(300);
                if (MeterProtocolAdapter.Instance.UpdateKeyInfo(Meter.MD_Epitope - 1, mac, DxString(keyinfo1)))
                {
                    EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 第{1}条密钥更新成功", Meter.MD_Epitope, i + 1), EnumLogType.提示信息);
                }
                else
                {
                    EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 第{1}条密钥更新失败", Meter.MD_Epitope, i + 1), EnumLogType.错误信息);
                    rst = false;
                    break;
                }
            }
            return rst;
        }

        /// <summary>
        /// 密钥更新
        /// </summary>
        /// <param name="rand2">随机数2</param>
        /// <param name="esamNo">Esam值</param>
        /// <param name="chipInfor">芯片信息</param>
        /// <returns></returns>
        private bool UpdataEncyKeyForOne(byte[] rand2, byte[] esamNo, string chipInfor)
        {

            //执行密钥更新函数
            string r2 = BitConverter.ToString(rand2).Replace("-", "");
            string esamno = BitConverter.ToString(esamNo).Replace("-", "");
            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');
            string msg = "";
            string encyKey = "";

            switch (curTrialType)
            {
                case Cus_EncryptionTrialType.密钥更新_01:
                    encyKey = "00010203";
                    break;
                case Cus_EncryptionTrialType.密钥更新_02:
                    encyKey = "04050607";
                    break;
                case Cus_EncryptionTrialType.密钥更新_03:
                    encyKey = "08090A0B";
                    break;
                case Cus_EncryptionTrialType.密钥更新_04:
                    encyKey = "0C0D0E0F";
                    break;
                case Cus_EncryptionTrialType.密钥更新_05:
                    encyKey = "10111213";
                    break;
            }

            //用私钥认证
            string outData = "";
            //密钥恢复用00，密钥更新用01
            bool r = EncrypGW.Meter_Formal_KeyUpdateV2(20, "01", encyKey, r2, MD_MeterNo, esamno, chipInfor, ref outData, ref msg);
            if (!r) return false;

            string mac = outData.Substring(288, 8);
            string encry = outData.Substring(0, 288);
            string keyinfo1 = "";
            for (int i = 0; i < 4; i++)
            {
                string tmp = encry.Substring(i * 72, 72);
                keyinfo1 += DxString(tmp.Substring(8, 64)) + DxString(tmp.Substring(0, 8));
            }

            //密钥更新
            //Thread.Sleep(300);
            if (MeterProtocolAdapter.Instance.UpdateKeyInfo(Meter.MD_Epitope - 1, mac, DxString(keyinfo1)))
            {
                EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 {1}成功", Meter.MD_Epitope, curTrialType), EnumLogType.提示信息);
                return true;
            }
            else
            {
                EquipmentData.Controller.MessageAdd(string.Format("第{0}表位 {1}失败", Meter.MD_Epitope, curTrialType), EnumLogType.提示信息);
                return false;
            }
        }

        /// <summary>
        /// 密钥更新    698 
        /// </summary>
        /// <param name="keyState">密码状态：0-私钥，1-公钥</param>
        /// <param name="esamId">Esam值</param>
        /// <param name="sessionKey">会话key</param>
        /// <param name="MD_MeterNo">分散因子，表号</param>
        /// <returns></returns>
        private bool UpdataEncyKeyFor698(int keyState, string esamId, string sessionKey, string MD_MeterNo)
        {
            try
            {

                MD_MeterNo = MD_MeterNo.PadLeft(16, '0');  //8个字节 

                bool r = EncrypGW.Obj_Meter_Formal_GetTrmKeyData(keyState, esamId, sessionKey, MD_MeterNo, "00", out string sid1, out string attachData1, out string trmKeyData1, out string mac1);
                if (!r)
                {
                    EquipmentData.Controller.MessageAdd($"获取keyState={keyState}密文失败,{esamId},{sessionKey},{MD_MeterNo}", EnumLogType.提示信息);
                    return false;
                }
                //TODO:820680
                string taskData = "070105F1000700020209" + "82" + (trmKeyData1.Length / 2).ToString("x4") + trmKeyData1 + "5E" + sid1 + "0B" + attachData1 + "04" + mac1 + "00";

                var a = trmKeyData1.Length;

                bool r1 = EncrypGW.Obj_Meter_Formal_GetMeterSetData(3, esamId, sessionKey,  taskData, out string sid2, out string attachData2, out string data2, out string mac2);

                if (!r1) return false;

                List<string> da = new List<string> { data2 };

                StSIDMAC sidMac = new StSIDMAC()
                {
                    SID = sid2,
                    AttachData = attachData2,
                    MAC = mac2,
                    Data = new Dictionary<string, List<string>>() { { "F1000700", da } }
                };

                StPackParas packPara = new StPackParas()
                {
                    SidMac = sidMac,
                    MeterAddr = Meter.MD_PostalAddress,
                    OD = new List<string>() { "F1000700" },
                    SecurityMode = EmSecurityMode.CiphertextMac,
                };

                //由于ZHDevices在表位多时，通信过程中会存在数据丢失的情况，因增加延时来错误通信时间，减少数据丢失

                //Random Rand = new Random();
                //int rand = Rand.Next(10, EquipmentData.Equipment.MeterCount * 1500);
                //Thread.Sleep(rand); //随机错开拼发

                //发送数据给电表
                int errCode = 0;
                List<object> LstObj = new List<object>();
                List<object> report = new List<object>();
                bool r2 = MeterProtocolAdapter.Instance.OperationSubFrame(Meter.MD_Epitope - 1, packPara, ref LstObj, ref report, ref errCode);
                if (!r2) return false;

                //验证返回值
                bool r3 = EncrypGW.Obj_Meter_Formal_VerifyMeterData(keyState, 3, esamId, sessionKey, LstObj[0].ToString(), LstObj[1].ToString(), out string outData);
                if (!r3) return false;

                if (outData.Length <= 0) return false;
                if (outData.Length < 16) return false;
                if (outData.Substring(14, 2) == "00") return true;
            }
            catch (Exception ex)
            {
                EquipmentData.Controller.MessageAdd($"密钥更新keyState={keyState},{esamId},{sessionKey},{MD_MeterNo}。{ex}", EnumLogType.错误信息);
            }
            return false;
        }

        /// <summary>
        /// 远程控制    698 
        /// </summary>
        /// <returns></returns>
        private bool UserControlFor698()
        {
            string taskData = "";
            string dataFlag = "";

            switch (curTrialType)
            {
                case Cus_EncryptionTrialType.远程拉闸:
                    dataFlag = "80008100";
                    taskData = "07013E800081000101020451F20502011100120000030001" + GetDateTimes(DateTime.Now) + "010005";
                    break;
                case Cus_EncryptionTrialType.远程合闸:
                case Cus_EncryptionTrialType.直接合闸:
                    dataFlag = "80008200";
                    taskData = "07013F800082000101020251F2050201160101" + GetDateTimes(DateTime.Now) + "010005";
                    break;
                case Cus_EncryptionTrialType.远程保电:
                    dataFlag = "80017F00";
                    taskData = "07013F80017F000001" + GetDateTimes(DateTime.Now) + "010005";
                    break;
                case Cus_EncryptionTrialType.解除保电:
                    dataFlag = "80018000";
                    taskData = "07013F800180000001" + GetDateTimes(DateTime.Now) + "010005";
                    break;
                case Cus_EncryptionTrialType.远程报警:
                    dataFlag = "80007F00";
                    taskData = "07013E80007F000001" + GetDateTimes(DateTime.Now) + "010005";
                    break;
                case Cus_EncryptionTrialType.解除报警:
                    dataFlag = "80008000";
                    taskData = "07013E800080000001" + GetDateTimes(DateTime.Now) + "010005";
                    break;
            }
            return MeterProtocolAdapter.Instance.Operation(Meter.MD_Epitope - 1, 3, 5, taskData, dataFlag, EmSecurityMode.CiphertextMac);

        }

        /// <summary>
        /// 获取DateTimeS时间字符串，698
        /// ，即yyyyMMddhhmmss，十六进制
        /// </summary>
        /// <returns></returns>
        private string GetDateTimes(DateTime dt)
        {
            string s = dt.Year.ToString("X4");
            s += dt.Month.ToString("X2");
            s += dt.Day.ToString("X2");
            s += dt.Hour.ToString("X2");
            s += dt.Minute.ToString("X2");
            s += dt.Second.ToString("X2");
            return s;
        }

        /// <summary>
        /// 按字节反转
        /// </summary>
        private static string DxString(string data)
        {
            int Len = data.Length / 2;
            string s = "";
            for (int i = 0; i < Len; i++)
            {
                s = data.Substring(i * 2, 2) + s;
            }
            return s;
        }
        /// <summary>
        /// 每次更新一条密钥
        /// </summary>
        private void StartWorkForUpdateOneKey()
        {
            IsStop = false;
            workOverFlag = false;
            UpdateKeyResult = false;

            if (Meter.DgnProtocol.HaveProgrammingkey) //09密钥
            {
                //curUpdateKeyType=主控密钥 / 远程密钥 / 参数密钥 / 身份密钥
                EquipmentData.Controller.MessageAdd($"正在进行【{curUpdateKeyType}】操作...", EnumLogType.提示信息);

                if ((Meter.FKType != 1 && curUpdateKeyType == Cus_UpdateKeyType.主控密钥) ||
                    (!curKeyUpdateInfo.UpdateKeyPublic && curUpdateKeyType == Cus_UpdateKeyType.远程密钥) ||
                    (Meter.FKType == 1 && curUpdateKeyType == Cus_UpdateKeyType.远程密钥))
                {
                    if (!GetEncyKey(ref curKeyInfo))
                    {
                        if (!GetEncyKey(ref curKeyInfo))
                        {
                            EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位获取密文失败...", EnumLogType.提示信息);
                            workOverFlag = true;
                            return;
                        }
                    }
                }

                if (curKeyUpdateInfo.GetKeyInfoSucc)//获取密文成功，则进行下装
                {
                    if (!MeterProtocolAdapter.Instance.UpdataEncyKey645_09(Meter.MD_Epitope - 1, curKeyUpdateInfo, curUpdateKeyType))
                    {
                        if (!MeterProtocolAdapter.Instance.UpdataEncyKey645_09(Meter.MD_Epitope - 1, curKeyUpdateInfo, curUpdateKeyType))
                        {
                            EquipmentData.Controller.MessageAdd($"第{ Meter.MD_Epitope}表位【{curUpdateKeyType}】更新失败...", EnumLogType.提示信息);
                            workOverFlag = true;
                            return;
                        }
                    }
                    UpdateKeyResult = true;
                }
                else
                    UpdateKeyResult = false;


            }
            else
            {
                //密钥更新
                EquipmentData.Controller.MessageAdd($"正在进行【{curTrialType}】操作...", EnumLogType.提示信息);

                if (!UpdataEncyKeyForOne(curIdentityInfo.Rand, curIdentityInfo.EsamNo, curKeyInfo.EsamCoreInfo))
                {
                    EquipmentData.Controller.MessageAdd(string.Format("第{0}表位{1}失败...", Meter.MD_Epitope, curTrialType), EnumLogType.提示信息);
                    workOverFlag = true;
                    return;
                }
                UpdateKeyResult = true;
            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 更新密钥    698
        /// </summary>
        private void StartWorkForUpdateKey698()
        {
            //初始化标志
            workOverFlag = false;
            UpdateKeyResult = false;

            EquipmentData.Controller.MessageAdd($"正在进行【{curTrialType}】操作...", EnumLogType.提示信息);

            if (UpdataEncyKeyFor698(1, Meter.EsamId, Meter.SessionKey, Meter.MD_MeterNo))
                UpdateKeyResult = true;
            else
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位{curTrialType}失败...", EnumLogType.提示信息);

            //恢复标志
            workOverFlag = true;
        }

        /// <summary>
        /// 密钥恢复    698
        /// </summary>
        private void StartWorkForRecoverKey698()
        {
            //初始化标志
            workOverFlag = false;
            CoverKeyResult = false;

            EquipmentData.Controller.MessageAdd($"正在进行【{curTrialType}】操作...", EnumLogType.提示信息);

            if (UpdataEncyKeyFor698(0, Meter.EsamId, Meter.SessionKey, Meter.MD_MeterNo))
                CoverKeyResult = true;
            else
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位{curTrialType}失败...", EnumLogType.提示信息);

            //恢复标志
            workOverFlag = true;
        }

        /// <summary>
        /// 远程控制    698
        /// </summary>
        private void StartWorkForUserControl698()
        {
            //初始化标志
            workOverFlag = false;
            RemoteControlResult = false;

            EquipmentData.Controller.MessageAdd($"正在进行【{curTrialType}】操作...", EnumLogType.提示信息);

            if (UserControlFor698())
                RemoteControlResult = true;
            else
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位{curTrialType}失败...", EnumLogType.提示信息);

            //恢复标志
            workOverFlag = true;
        }

        /// <summary>
        /// 融通加密机秘钥更新
        /// </summary>
        private void StartRongTongUpDateKey()
        {
            IsStop = false;
            workOverFlag = false;
            UpdateKeyResult = false;

            EquipmentData.Controller.MessageAdd("正在进行【融通密钥更新试验】操作...", EnumLogType.提示信息);
            Meter.MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');
            CertificationResult = IdentityAuthen645(out curIdentityInfo.Rand, out curIdentityInfo.EsamNo);
            if (CertificationResult == 1)
            {
                if (!UpdataEncyKey645_09(curIdentityInfo.Rand, curIdentityInfo.EsamNo, curKeyInfo))
                {
                    if (!UpdataEncyKey645_09(curIdentityInfo.Rand, curIdentityInfo.EsamNo, curKeyInfo))
                    {
                        EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位获取密文失败...", EnumLogType.提示信息);
                        workOverFlag = true;
                        return;
                    }
                }
                UpdateKeyResult = true;
            }
            else if (CertificationResult == 2)
            {
                UpdateKeyResult = true;
            }

            //恢复标志
            IsStop = true;
            workOverFlag = true;
        }

        /// <summary>
        /// 远程控置：跳合闸、报警、解除报警、保电、解除保电
        /// </summary>
        /// <param name="bwindex"></param>
        /// <param name="address"></param>
        /// <param name="Rand"></param>
        /// <param name="esamNo"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool UserControl(byte[] rand2, byte[] esamNo, string data)
        {
            string r2 = BitConverter.ToString(rand2).Replace("-", "");
            string esamno = BitConverter.ToString(esamNo).Replace("-", "");
            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');

            int flag = MeterProtocolAdapter.Instance.MeterProtocols[Meter.MD_Epitope - 1].IdentityStatus;
            bool r = EncrypGW.UserControl(flag, r2, MD_MeterNo, esamno, data, out string endData);
            if (!r)
                r = EncrypGW.UserControl(flag, r2, MD_MeterNo, esamno, data, out endData);

            if (r)
            {
                if (MeterProtocolAdapter.Instance.UserControl(Meter.MD_Epitope - 1, endData))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 安全认证,返回:0-失败,1-公钥认证成功,2-私钥认证成功。【09密钥有效，13密钥有效，698无效】
        /// </summary>
        /// <param name="rand2">随机数2</param>
        /// <param name="esamNo">esamNo</param>
        /// <returns></returns>
        private int IdentityAuthen645(out byte[] rand2, out byte[] esamNo)
        {
            string rand = "", endata = "";
            rand2 = null;
            esamNo = null;

            //1. 公钥认证获取随机数和密文 ，认证成功函数返回
            MeterProtocolAdapter.Instance.MeterProtocols[Meter.MD_Epitope - 1].IdentityStatus = 0; //认证状态
            string div = Meter.MD_MeterNo.PadLeft(16, '0');
            if (Meter.DgnProtocol.HaveProgrammingkey)
            {
                div = "0000000000000001";
            }
            if (!EncrypGW.IdentityAuthentication(0, div, ref rand, ref endata))
            {
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位获取身份认证密文失败...", EnumLogType.提示信息);
                return 0;
            }
            if (MeterProtocolAdapter.Instance.IdentityAuthentication(Meter.MD_Epitope - 1, rand, endata, div, out rand2, out esamNo)) return 1;

            //Thread.Sleep(200);
            if (MeterProtocolAdapter.Instance.IdentityAuthentication(Meter.MD_Epitope - 1, rand, endata, div, out rand2, out esamNo)) return 1;

            //2. 公钥认证失败，用密钥认证获取随机数和密文
            //Thread.Sleep(200);
            MeterProtocolAdapter.Instance.MeterProtocols[Meter.MD_Epitope - 1].IdentityStatus = 1;
            if (!EncrypGW.IdentityAuthentication(1, Meter.MD_MeterNo, ref rand, ref endata))
            {
                EquipmentData.Controller.MessageAdd($"第{Meter.MD_Epitope}表位身份认证失败...", EnumLogType.提示信息);

                return 0;
            }
            //私钥认证
            if (MeterProtocolAdapter.Instance.IdentityAuthentication(Meter.MD_Epitope - 1, rand, endata, Meter.MD_MeterNo, out rand2, out esamNo)) return 2;

            //Thread.Sleep(200);
            if (MeterProtocolAdapter.Instance.IdentityAuthentication(Meter.MD_Epitope - 1, rand, endata, Meter.MD_MeterNo, out rand2, out esamNo)) return 2;

            return 0;
        }

        /// <summary>
        /// 读取ESAM模块密钥文件信息
        /// </summary>
        /// <param name="keyinfo">密钥文件信息</param>
        /// <param name="iResult"></param>
        private bool ReadESAMDataInfo(ref StKeyInfo keyinfo)
        {
            //获取密钥文件信息
            string msg = "";
            if (MeterProtocolAdapter.Instance.ReadKeyReturnData(Meter.MD_Epitope - 1, out keyinfo, out byte[] bMac))
            {
                if (!Meter.DgnProtocol.HaveProgrammingkey)
                {
                    int flag = MeterProtocolAdapter.Instance.MeterProtocols[Meter.MD_Epitope - 1].IdentityStatus;
                    return EncrypGW.Meter_Formal_MacCheck(flag, BitConverter.ToString(curIdentityInfo.Rand).Replace("-", ""), Meter.MD_MeterNo.PadLeft(16, '0'), "04d6860066", keyinfo.EsamCoreInfo, BitConverter.ToString(bMac).Replace("-", ""), ref msg);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// ESAM模块信息清零（先私钥，再公钥）
        /// </summary>
        /// <param name="iResult">认证结果</param>
        /// <param name="address">地址</param>
        /// <returns></returns>
        public bool ClearKeyInfo(byte[] Rand2, byte[] EsamNo)
        {
            //执行清零函数
            string r2 = BitConverter.ToString(Rand2).Replace("-", "");
            string esamno = BitConverter.ToString(EsamNo).Replace("-", "");
            string pKeyinfo1 = "00000000";
            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');

            //用私钥认证
            MeterProtocolAdapter.Instance.MeterProtocols[Meter.MD_Epitope - 1].IdentityStatus = 1;
            bool bReturn = EncrypGW.ClearKeyInfo(1, MD_MeterNo, r2, esamno, pKeyinfo1, out string key1, out string keyinfo1);

            if (!bReturn)
            {
                //如果认证失败,再用公钥认证
                MeterProtocolAdapter.Instance.MeterProtocols[Meter.MD_Epitope - 1].IdentityStatus = 0;
                bReturn = EncrypGW.ClearKeyInfo(0, MD_MeterNo, r2, esamno, pKeyinfo1, out key1, out keyinfo1);
            }
            if (bReturn)
            {
                //密钥清零
                //Thread.Sleep(300);
                if (MeterProtocolAdapter.Instance.ClearKeyInfo(Meter.MD_Epitope - 1, key1, keyinfo1))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 09密钥
        /// </summary>
        /// <param name="isLocalMeter"></param>
        /// <param name="keyinfo"></param>
        /// <returns></returns>
        private bool GetEncyKey(ref StKeyInfo keyinfo)
        {
            //设置密钥信息明文
            int num = Convert.ToInt16(keyinfo.KeyVer);
            curKeyUpdateInfo = new StKeyUpdateInfo
            {
                MeterDiv = Meter.MD_MeterNo.PadLeft(16, '0'),
                MeterRand = BitConverter.ToString(curIdentityInfo.Rand).Replace("-", ""),
                MeterEsamNo = BitConverter.ToString(curIdentityInfo.EsamNo).Replace("-", ""),
                主控密钥明文 = "010104" + (num + 1).ToString().PadLeft(2, '0'),
                远程密钥明文 = "010102" + (num + 2).ToString().PadLeft(2, '0'),
                参数密钥明文 = "010101" + (num + 3).ToString().PadLeft(2, '0'),
                身份密钥明文 = "010103" + (num + 4).ToString().PadLeft(2, '0'),
                UpdateKeyPublic = CertificationResult == 1,
                LocalMeter = Meter.FKType == 1,
            };

            //获取密钥更新密文
            return EncrypGW.KeyUpdate(ref curKeyUpdateInfo);
        }

        /// <summary>
        /// 密钥更新 645_09
        /// </summary>
        /// <param name="address"></param>
        /// <param name="isLocalMeter"></param>
        /// <param name="iResult"></param>
        private bool UpdataEncyKey645_09(byte[] rand2, byte[] esamNo, StKeyInfo keyinfo)
        {
            bool bUpdateKeyPublic = false;

            string r2 = BitConverter.ToString(rand2).Replace("-", "");
            string esamno = BitConverter.ToString(esamNo).Replace("-", "");
            //设置密钥信息明文
            string[] putKeyinfo = new string[4];
            int num = Convert.ToInt16(keyinfo.KeyVer);
            putKeyinfo[0] = "010104" + (num + 1).ToString().PadLeft(2, '0');
            putKeyinfo[1] = "010102" + (num + 3).ToString().PadLeft(2, '0');
            putKeyinfo[2] = "010101" + (num + 2).ToString().PadLeft(2, '0');
            putKeyinfo[3] = "010103" + (num + 4).ToString().PadLeft(2, '0');

            string MD_MeterNo = Meter.MD_MeterNo.PadLeft(16, '0');
            //获取密钥更新密文
            bool bReturn = EncrypGW.KeyUpdate(MD_MeterNo, r2, esamno, putKeyinfo[0], putKeyinfo[1], putKeyinfo[2], putKeyinfo[3],
                                              out string key1, out string keyinfo1, out string key2, out string keyinfo2, out string key3, out string keyinfo3, out string key4, out string keyinfo4);

            if (CertificationResult == 1)
                bUpdateKeyPublic = true;

            if (bReturn)//更新密钥
            {
                if (MeterProtocolAdapter.Instance.UpdataEncyKey(Meter.MD_Epitope - 1, (Meter.FKType == 1), bUpdateKeyPublic, putKeyinfo, key1, keyinfo1, key2, keyinfo2, key3, keyinfo3, key4, keyinfo4))
                    return true;
            }
            return false;
        }
    }
}
