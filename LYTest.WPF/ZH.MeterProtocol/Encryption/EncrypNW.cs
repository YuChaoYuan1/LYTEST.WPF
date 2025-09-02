using System.Text;

namespace LYTest.MeterProtocol.Encryption
{
    public class EncrypNW
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        private static bool IsLink { set; get; }


        public int SouthCloseDevice(out string Message)
        {
            int intRst = EncrypAPINW.CloseDevice();
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthDataClear1(int Flag, string PutRand, string PutDiv, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(128);
            int intRst = EncrypAPINW.DataClear1(Flag, PutRand, PutDiv, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;


        }

        public int SouthDecreasePurse(int Flag, string PutRand, string PutDiv, string PutData, out string OutEndata, out string Message)
        {
            OutEndata = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.DecreasePurse(Flag, PutRand, PutDiv, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutEndata = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;
        }

        public int SouthEncForCompare(string PutKeyid, string PutDiv, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.EncForCompare(PutKeyid, PutDiv, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthEncMacWrite(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutFileID, string PutDataBegin, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.EncMacWrite(Flag, PutRand, PutDiv, PutEsamNo, PutFileID, PutDataBegin, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthIdentityAuthentication(int Flag, string PutDiv, out string OutRand, out string OutEndata, out string Message)
        {
            OutRand = "";
            OutEndata = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutRandTmp = new StringBuilder();
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.IdentityAuthentication(Flag, PutDiv, OutRandTmp, OutDataTmp);
            if (intRst == 0)
            {
                OutRand = OutRandTmp.ToString().Replace("\0", "");
                OutEndata = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthIncreasePurse(int Flag, string PutRand, string PutDiv, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.IncreasePurse(Flag, PutRand, PutDiv, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthInfraredAuth(int Flag, string PutDiv, string PutEsamNo, string PutRand1, string PutRand1Endata, string PutRand2, out string OutRand2Endata, out string Message)
        {
            OutRand2Endata = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.InfraredAuth(Flag, PutDiv, PutEsamNo, PutRand1, PutRand1Endata, PutRand2, OutDataTmp);
            if (intRst == 0)
            {
                OutRand2Endata = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthInfraredRand(out string OutRand1, out string Message)
        {
            OutRand1 = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.InfraredRand(OutDataTmp);
            if (intRst == 0)
            {
                OutRand1 = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthInitPurse(int Flag, string PutRand, string PutDiv, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.InitPurse(Flag, PutRand, PutDiv, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthKeyUpdateV2(int PutKeySum, string PutKeyState, string PutKeyId, string PutRand, string PutDiv, string PutEsamNo, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(2048);
            int intRst = EncrypAPINW.KeyUpdateV2(PutKeySum, PutKeyState, PutKeyId, PutRand, PutDiv, PutEsamNo, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthMacCheck(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, string PutMac, out string Message)
        {
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            //StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.MacCheck(Flag, PutRand, PutDiv, PutApdu, PutData, PutMac);
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthMacWrite(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutFileID, string PutDataBegin, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.MacWrite(Flag, PutRand, PutDiv, PutEsamNo, PutFileID, PutDataBegin, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }


        public int SouthOpenDevice(string szType, string cHostIp, int uiPort, int timeout, out string Message)
        {
            //Message = "";

            //StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.OpenDevice(szType, cHostIp, uiPort, timeout);
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthParameterElseUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, out string OutEndata, out string Message)
        {
            OutEndata = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(1024);
            int intRst = EncrypAPINW.ParameterElseUpdate(Flag, PutRand, PutDiv, PutApdu, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutEndata = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public int SouthParameterUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder();
            int intRst = EncrypAPINW.ParameterUpdate(Flag, PutRand, PutDiv, PutApdu, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;
        }

        public int SouthPrice1Update(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(512);
            int intRst = EncrypAPINW.Price1Update(Flag, PutRand, PutDiv, PutApdu, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;
        }

        public int SouthPrice2Update(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(512);
            int intRst = EncrypAPINW.Price2Update(Flag, PutRand, PutDiv, PutApdu, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;
        }

        public int SouthSwitchChargeMode(int Flag, string PutRand, string PutDiv, string PutData, out string OutData, out string Message)
        {
            OutData = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(128);
            int intRst = EncrypAPINW.SwitchChargeMode(Flag, PutRand, PutDiv, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutData = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;
        }

        public int SouthUserControl(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutData, out string OutEndata, out string Message)
        {
            OutEndata = "";
            //Message = "";
            if (!IsLink)
            {
                Message = "没有联接加密机.";
                return -1;
            }
            StringBuilder OutDataTmp = new StringBuilder(128);
            int intRst = EncrypAPINW.UserControl(Flag, PutRand, PutDiv, PutEsamNo, PutData, OutDataTmp);
            if (intRst == 0)
            {
                OutEndata = OutDataTmp.ToString().Replace("\0", "");
            }
            Message = CheckResult(intRst);
            return intRst;

        }

        public bool UnLink()
        {
            int rst = SouthCloseDevice(out _);
            return rst == 0;
        }

        /// <summary>
        /// 获取错误码的描述
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string CheckResult(int result)
        {
            string strErr = "";
            if (result != 0)
            {
                switch (result)
                {
                    case 48:
                        strErr = "无设备或设备无效";
                        break;
                    case 56:
                        strErr = "创建socket 句柄失败";
                        break;
                    case 57:
                        strErr = "连接服务器失败";
                        break;
                    case 64:
                        strErr = "客户端发送数据失败";
                        break;
                    case 65:
                        strErr = "客户端接收数据失败";
                        break;
                    case 100:
                        strErr = "打开设备失败";
                        break;
                    case 160:
                        strErr = " 连接密码机失败";
                        break;
                    case 161:
                        strErr = "操作权限不够";
                        break;
                    case 162:
                        strErr = "USBKey 不是操作员";
                        break;
                    case 163:
                        strErr = "服务器发送数据失败";
                        break;
                    case 164:
                        strErr = "服务端接收报文失败";
                        break;
                    case 165:
                        strErr = "密码机加密数据失败";
                        break;
                    case 166:
                        strErr = "密码机导出密钥失败";
                        break;
                    case 167:
                        strErr = "密码机计算MAC 失败";
                        break;
                    case 168:
                        strErr = " 服务器已断开连接";
                        break;
                    case 169:
                        strErr = "数据无效";
                        break;
                    case 170:
                        strErr = "密码机收发报文错误";
                        break;
                    case 171:
                        strErr = "密码机故障";
                        break;
                    case 172:
                        strErr = "数据库出错";
                        break;
                    case 202:
                        strErr = " 打开加密机错误";
                        break;
                    case 203:
                        strErr = " 关闭加密机错误";
                        break;
                    case 306:
                        strErr = " 取随机数错误";
                        break;
                    case 700:
                        strErr = " 密钥导出错误";
                        break;
                    case 810:
                        strErr = " MAC校验错误";
                        break;
                    case 900:
                        strErr = " 数据加密错误";
                        break;
                    case 902:
                        strErr = " MAC计算错误";
                        break;
                    case 1100:
                        strErr = " 认证错误，红外认证时比对密文";
                        break;
                    case 1601:
                        strErr = " 不支持的加密机类型";
                        break;
                    case 1107:
                        strErr = "USBKey 权限不正确";
                        break;
                    case 1501:
                        strErr = "    参数1错误";
                        break;
                    case 1502:
                        strErr = "    参数2错误";
                        break;
                    case 1503:
                        strErr = "    参数3错误";
                        break;
                    case 1504:
                        strErr = "    参数4错误";
                        break;
                    case 1505:
                        strErr = "    参数5错误";
                        break;
                    case 1506:
                        strErr = "    参数6错误";
                        break;
                    case 1507:
                        strErr = "    参数7错误";
                        break;
                    case 1508:
                        strErr = "    参数8错误";
                        break;

                    default:
                        if (result >= 700 && result <= 712)
                        {
                            strErr = "客户端导出密钥失败";
                        }
                        else if (result >= 800 && result <= 810)
                        {
                            strErr = "计算MAC失败";
                        }
                        else if (result >= 900 && result <= 910)
                        {
                            strErr = "加密数据失败";
                        }
                        else if (result >= 1000 && result <= 1010)
                        {
                            strErr = "数据长度错";
                        }
                        else if (result >= 1108 && result <= 1111)
                        {
                            strErr = "操作USBKey 失败";
                        }
                        else
                        {
                            strErr = "未知其他错误";
                        }
                        break;
                }
                strErr = "错误码：" + result + " 错误描述:" + strErr + "。";
            }
            return strErr;
        }

    }
}
