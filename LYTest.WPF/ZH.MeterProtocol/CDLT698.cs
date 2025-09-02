using LYTest.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Device;
using LYTest.MeterProtocol.Protocols;
using LYTest.MeterProtocol.Protocols.ApplicationLayer;
using LYTest.MeterProtocol.Protocols.DLT698;
using LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;

namespace LYTest.MeterProtocol
{
    public class CDLT698 : CDLT698Base
    {
        /// <summary>
        /// 读地址
        /// </summary>
        /// <returns></returns>
        public override string ReadAddress()
        {

            string addr = string.Empty;

            CptReadAddress(ref addr);

            return addr;
        }

        /// <summary>
        /// 强制地址
        /// </summary>
        /// <param name="outAddr">返回地址</param>
        /// <returns></returns>
        private bool CptReadAddress(ref string outAddr)
        {
            //a)功能：请求读电能表通信地址，仅支持点对点通信。
            //b)地址域：AA…AAH
            //c)控制码：C=13H
            //d)数据域长度：L=00H
            int errCode = 0;

            StPackParas para = new StPackParas
            {
                MeterAddr = "AAAAAAAAAAAA",
                SecurityMode = EmSecurityMode.ClearText,
                OD = new List<string>() { "40010200" },
                SidMac = new StSIDMAC(),
                GetRequestMode = EmGetRequestMode.GetRequestNormal
            };

            List<object> objList = new List<object>();
            List<object> report = new List<object>();
            ReadData(para, ref objList, ref report, ref errCode);
            if (objList.Count > 0)
            {
                outAddr = objList.ToArray()[0].ToString();
                return true;
            }
            else
                return false;
        }

        #region -----------------接口成员-----------------------

        /// <summary>
        /// 通信测试
        /// </summary>
        /// <returns></returns>
        public override bool ComTest()
        {
            return !string.IsNullOrEmpty(ReadAddress());
        }

        /// <summary>
        /// 广播校时
        /// </summary>
        /// <param name="datetime">日期时间</param>
        /// <returns></returns>
        public override bool BroadcastTime(DateTime datetime)
        {
            string str_DateTime = datetime.ToString("yyMMddHHmmss");
            //功能：强制从站与主站时间同步
            //b)控制码：C=08H
            //c)数据域长度：L=06H
            //d)数据域：YYMMDDhhmmss(年.月.日.时.分.秒)
            byte[] data = new byte[6];
            byte[] tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + str_DateTime, 16));
            Array.Copy(tmp, data, 6);
            return true;

        }

        /// <summary>
        /// 读取电量(分费率读取)
        /// </summary>
        /// <param name="energyType">功率类型:
        ///     0-组合有功电能,
        ///     1-正向有功电能,
        ///     2-反向有功电能,
        ///     3-组合无功1电能,
        ///     4-组合无功2电能,
        ///     5-第1象限无功电能,
        ///     6-第2象限无功电能,
        ///     7-第3象限无功电能,
        ///     8-第4象限无功电能</param>
        /// <param name="tariffType">费率类型：
        ///     0-总电能，
        ///     1-峰，
        ///     2-平，
        ///     3-谷，
        ///     4-尖
        /// </param>
        /// <returns>-1读失败</returns>
        public override float ReadEnergy(byte energyType, byte tariffType)
        {
            return ReadEnergyExtension(energyType, tariffType, 2);
        }
        public override FloatE ReadEnergyR(byte energyType, byte tariffType)
        {
            return ReadEnergyExtensionR(energyType, tariffType, 2);
        }

        public override float ReadEnergyExtension(byte energyType, byte tariffType, int DecimalDigits)
        {
            int errCode = 0;

            if (energyType < 0 || tariffType < 0 || tariffType > 63)
                return -1f;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出电量

            int dot = 4;//2/4
            if (DecimalDigits <= 2) dot = 2;

            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            //电量 00t00d00
            List<string> LstOad = new List<string>
            {
                "00" + energyType.ToString() + "00" + dot + "00"
            };

            StPackParas DitPackParas = new StPackParas();
            DitPackParas.MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : DitPackParas.MeterAddr = "AAAAAAAAAAAA";
            DitPackParas.SecurityMode = EmSecurityMode.ClearText;
            DitPackParas.OD = LstOad;
            DitPackParas.SidMac = new StSIDMAC();
            DitPackParas.GetRequestMode = EmGetRequestMode.GetRequestNormal;
            ReadData(DitPackParas, ref LstObj, ref report, ref errCode);

            float[] energys = new float[LstObj.Count];

            int i = 0;
            foreach (object obj in LstObj)
            {
                if (dot == 4)
                {
                    energys[i] = float.Parse(obj.ToString()) / 10000f;
                }
                else
                {
                    energys[i] = float.Parse(obj.ToString()) / 100f;
                }
                i++;
            }

            if (energys.Length > 0)
            {
                if (tariffType == 0)
                    return energys[0];
                else
                {
                    if (energys.Length > index)
                        return energys[index];
                    else
                        return -1;
                }
            }
            else
                return -1;
        }

        public override FloatE ReadEnergyExtensionR(byte energyType, byte tariffType, int DecimalDigits)
        {
            int errCode = 0;

            if (energyType < 0 || tariffType < 0 || tariffType > 63)
                return -1m;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出电量

            int dot = 4;//2/4
            if (DecimalDigits <= 2) dot = 2;

            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            //电量 00t00d00
            List<string> LstOad = new List<string>
            {
                "00" + energyType.ToString() + "00" + dot + "00"
            };

            StPackParas DitPackParas = new StPackParas();
            DitPackParas.MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : DitPackParas.MeterAddr = "AAAAAAAAAAAA";
            DitPackParas.SecurityMode = EmSecurityMode.ClearText;
            DitPackParas.OD = LstOad;
            DitPackParas.SidMac = new StSIDMAC();
            DitPackParas.GetRequestMode = EmGetRequestMode.GetRequestNormal;
            ReadData(DitPackParas, ref LstObj, ref report, ref errCode);

            FloatE[] energys = new FloatE[LstObj.Count];

            int i = 0;
            foreach (object obj in LstObj)
            {
                if (dot == 4)
                {
                    energys[i] = (decimal.Parse(obj.ToString()) / 10000m).ToString("F4");
                }
                else
                {
                    energys[i] = (decimal.Parse(obj.ToString()) / 100m).ToString("F2");
                }
                i++;
            }

            if (energys.Length > 0)
            {
                if (tariffType == 0)
                    return energys[0];
                else
                {
                    if (energys.Length > index)
                        return energys[index];
                    else
                        return -1;
                }
            }
            else
                return -1;
        }
        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="energyType">功率类型:
        ///     0-组合有功电能,
        ///     1-正向有功电能,
        ///     2-反向有功电能,
        ///     3-组合无功1电能,
        ///     4-组合无功2电能,
        ///     5-第1象限无功电能,
        ///     6-第2象限无功电能,
        ///     7-第3象限无功电能,
        ///     8-第4象限无功电能</param>
        /// <param name="freezeTimes">无用参数</param>
        /// <returns></returns>
        public override float[] ReadEnergys(byte energyType, int freezeTimes)
        {
            int errCode = 0;
            float[] energy = new float[0];
            if (energyType < 0) return energy;

            //电量
            StPackParas DitPackParas = new StPackParas
            {
                MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : "AAAAAAAAAAAA",
                SecurityMode = EmSecurityMode.ClearText,

                OD = new List<string> { "00" + energyType.ToString() + "00200" },
                SidMac = new StSIDMAC(),
                GetRequestMode = EmGetRequestMode.GetRequestNormal,

            };

            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            ReadData(DitPackParas, ref LstObj, ref report, ref errCode);

            int i = 0;
            float[] energys = new float[LstObj.Count];
            foreach (object obj in LstObj)
            {
                energys[i] = float.Parse(obj.ToString());
                i++;
            }
            energy = energys;
            return energy;
        }

        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="energyType">功率类型:
        ///     0-组合有功电能,
        ///     1-正向有功电能,
        ///     2-反向有功电能,
        ///     3-组合无功1电能,
        ///     4-组合无功2电能,
        ///     5-第1象限无功电能,
        ///     6-第2象限无功电能,
        ///     7-第3象限无功电能,
        ///     8-第4象限无功电能</param>
        /// <returns></returns>
        public override float[] ReadEnergy(byte energyType)
        {
            return ReadEnergyExtension(energyType, 2);
        }

        public override float[] ReadEnergyExtension(byte energyType, int DecimalDigits)
        {
            int errCode = 0;

            if (energyType < 0) return new float[0];

            int dot = 4;
            if (DecimalDigits <= 2) dot = 2;

            StPackParas DitPackParas = new StPackParas
            {
                MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : "AAAAAAAAAAAA",
                SecurityMode = EmSecurityMode.ClearText,
                OD = new List<string> { "00" + energyType.ToString() + "00" + dot + "00" },
                SidMac = new StSIDMAC(),
                GetRequestMode = EmGetRequestMode.GetRequestNormal
            };

            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            ReadData(DitPackParas, ref LstObj, ref report, ref errCode);

            float[] energys = new float[LstObj.Count];
            int intNo = 0;
            foreach (object obj in LstObj)
            {
                if (dot == 4)
                {
                    energys[intNo] = float.Parse(obj.ToString()) / 10000f;
                }
                else
                {
                    energys[intNo] = float.Parse(obj.ToString()) / 100f;
                }
                intNo++;
            }
            return energys;
        }

        /// <summary>
        /// 读取需量(所有费率读取)
        /// </summary>
        /// <param name="energyType">
        ///     1-正向有功最大需量，
        ///     2-反向有功最大需量，
        ///     3-组合有功1最大需量，
        ///     4-组合有功2最大需量，
        ///     5-第一象限最大需量，
        ///     6-第二象限最大需量，
        ///     7-第三象限最大需量，
        ///     8-第四象限最大需量，
        /// </param>
        /// <returns></returns>
        public override float[] ReadDemand(byte energyType)
        {

            int errCode = 0;
            //电量
            List<string> LstOad = new List<string>
            {
                "10" + energyType.ToString() + "00200"
            };

            StPackParas DitPackParas = new StPackParas
            {
                MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : "AAAAAAAAAAAA",
                SecurityMode = EmSecurityMode.CiphertextMac,
                OD = LstOad,
                SidMac = new StSIDMAC(),
                GetRequestMode = EmGetRequestMode.GetRequestNormal,
            };

            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            ReadData(DitPackParas, ref LstObj, ref report, ref errCode);

            float[] demands = new float[LstObj.Count];
            for (int i = 0; i < LstObj.Count; i++)
            {
                demands[i] = float.Parse(LstObj[i].ToString());
            }
            return demands;
        }

        /// <summary>
        /// 读日期时间
        /// </summary>
        /// <param name="int_Type">读取类型 </param>
        /// <param name="str_DateTime">返回时间 yy-mm-dd hh:mm:ss</param>
        /// <returns></returns>
        public override DateTime ReadDateTime()
        {
            int errCode = 0;

            StPackParas para = new StPackParas
            {
                MeterAddr = MeterAddress,
                SecurityMode = EmSecurityMode.ClearText,
                OD = new List<string>() { "40000200" },
                SidMac = new StSIDMAC(),
                GetRequestMode = EmGetRequestMode.GetRequestNormal
            };

            List<object> objList = new List<object>();
            List<object> report = new List<object>();
            ReadData(para, ref objList, ref report, ref errCode); //20080229000010

            if (objList == null || objList.Count <= 0)
                return DateTime.MinValue;
            else
            {
                string t = objList[0].ToString();
                return new DateTime(Convert.ToInt32(t.Substring(0, 4)), Convert.ToInt32(t.Substring(4, 2)), Convert.ToInt32(t.Substring(6, 2)),
                                    Convert.ToInt32(t.Substring(8, 2)), Convert.ToInt32(t.Substring(10, 2)), Convert.ToInt32(t.Substring(12, 2)));
            }
        }

        /// <summary>
        /// 读取时段
        /// </summary>
        /// <returns></returns>
        public override string[] ReadPeriodTime()
        {

            string[] periodTime = new string[0];
            float periodCount = ReadData("04000203", 1, 0);     //读取时段数
            //04010001
            string[] data = ReadDataBlock("04010001", 3);     //读取时段块
            if (data.Length > 0)
            {
                if (data.Length >= periodCount)
                {
                    Array.Resize(ref periodTime, Convert.ToInt16(periodCount));
                    for (int i = 0; i < Convert.ToInt16(periodCount); i++)
                    {
                        if (ReadPeriodTimeType == 0)
                            periodTime[i] = data[i];
                        else
                            periodTime[i] = data[i].Substring(2, 4) + data[i].Substring(0, 2);
                    }
                }
                else
                {
                    Array.Resize(ref periodTime, Convert.ToInt16(data.Length));
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (ReadPeriodTimeType == 0)
                            periodTime[i] = data[i];
                        else
                            periodTime[i] = data[i].Substring(2, 4) + data[i].Substring(0, 2);
                    }
                }
            }
            return periodTime;
        }


        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns></returns>
        public override bool ClearDemand(string endata)
        {

            byte[] byt_RevData = new byte[0];
            bool bln_Sequela = false;
            byte[] byteSendData = GetBytesArry(endata.Length / 2, endata, true);  //密文
            return ExeCommand(0x43, 0x05, GetAddressByte(), 0x10, byteSendData, ref bln_Sequela, ref byt_RevData);
        }

        /// <summary>
        /// 清空电量
        /// </summary>
        ///  <param name="endata">密文 </param>
        /// <returns></returns>
        public override bool ClearEnergy(string endata)
        {
            byte[] byt_RevData = new byte[0];
            bool bln_Sequela = false;
            byte[] byteSendData = GetBytesArry(endata.Length / 2, endata, true);  //密文
            return ExeCommand(0x43, 0x05, GetAddressByte(), 0x10, byteSendData, ref bln_Sequela, ref byt_RevData);
        }

        /// <summary>
        /// 钱包初始化
        /// </summary>
        ///  <param name="endata">密文 </param>
        /// <returns></returns>
        public override bool InitPurse(string endata)
        {
            //a)功能：钱包初始化            
            //c)数据域长度：L=08H  DI (4) + UserID(4) + 密文(16)

            byte[] byt_Data = new byte[24];
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + "070103FF", 16)), byt_Data, 4);
            string str_UserID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + str_UserID.Substring(str_UserID.Length - 8), 16)), 0, byt_Data, 4, 4);
            Array.Copy(GetBytesArry(16, endata, false), 0, byt_Data, 8, 16);

            return true;

        }

        /// <summary>
        /// 清空事件记录
        /// </summary>
        /// <param name="int_Type">操作类型</param>
        /// <param name="dataFlag">事件清零内容 事件总清零=FFFFFFFF   分项事件清零=DI3DI2DI1FF</param>
        /// <returns></returns>
        public override bool ClearEventLog(string dataFlag)
        {

            //a)功能：清空电能表内存储的全部或某类事件记录数据。
            //b)控制码：C=1BH
            //c)数据域长度：L=0CH
            //1）事件总清零 PAOP0OP1OP2O＋C0C1C2C3＋FFFFFFFF；
            //2）分项事件清零 PAOP0OP1OP2O＋C0C1C2C3＋事件记录数据标识（DI0用FF表示）
            byte[] byt_Data = new byte[12];
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDLPassword + protocolInfo.ClearDLClass, 16)), byt_Data, 4);
            string str_Tmp = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + str_Tmp.Substring(str_Tmp.Length - 8), 16)), 0, byt_Data, 4, 4);
            str_Tmp = "00000000" + dataFlag;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + str_Tmp.Substring(str_Tmp.Length - 8), 16)), 0, byt_Data, 8, 4);

            return true;
        }

        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns></returns>
        public override bool ClearEventLog(string dataFlag, string endata)
        {

            //a)功能：当前最大需量及发生时间数据清零
            //b)控制码：C=19H
            //c)数据域长度：L=08H  password (4) + UserID(4)
            byte[] data = new byte[28];

            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDemandPassword + protocolInfo.ClearDemandClass, 16)), data, 4);
            string userID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + userID.Substring(userID.Length - 8), 16)), 0, data, 4, 4);
            Array.Copy(GetBytesArry(20, endata, false), 0, data, 8, 20);

            return true;
        }

        /// <summary>
        /// 设置多功能脉冲端子输出脉冲类型
        /// </summary>
        /// <param name="pulse">端子输出脉冲类型</param>
        /// <returns></returns>
        public override bool SetPulseCom(byte pulse)
        {
            List<string> da = new List<string>
            {
                "F2070201",//参数1:路号
                pulse.ToString().PadLeft(2, '0')  //参数2:工作模式
            };

            StSIDMAC sidMac = new StSIDMAC
            {
                Data = new Dictionary<string, List<string>> { { "F2077F00", da } }
            };

            StPackParas packPara = new StPackParas
            {
                MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : "AAAAAAAAAAAA",
                SidMac = sidMac,
                SecurityMode = EmSecurityMode.ClearText,
                OD = new List<string> { "F2077F00" }
            };

            int errCode = 0;
            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            Operation(packPara, ref LstObj, ref report, ref errCode);
            return true;

        }


        /// <summary>
        /// 冻结命令
        /// </summary>
        /// <param name="int_Type">操作类型 0=广播冻结，1=普通冻结</param>
        /// <param name="dateHour">冻结时间，MMDDhhmm(月.日.时.分)数据域99DDhhmm表示以月为周期定时冻结，9999hhmm表示以日为周期定时冻结，999999mm表示以小时为周期定时冻结，99999999为瞬时冻结。</param>
        /// <returns></returns>
        public override bool FreezeCmd(string dateHour)
        {

            return true;
        }
        #endregion

        /// <summary>
        /// 把任意16进制字符串转换为指定长度的byte数组
        /// </summary>
        /// <param name="len">数组长度</param>
        /// <param name="value">要转换的字符串</param>
        /// <param name="reverse">true翻转，false不翻转</param>
        /// <returns></returns>
        private byte[] GetBytesArry(int len, string value, bool reverse)
        {
            byte[] data = new byte[len];
            string tmp = value;

            if (value.Length > len * 2)
                tmp = value.Substring(value.Length - len * 2);
            else if (value.Length < len * 2)
                tmp = value.PadLeft(len * 2 - value.Length, '0');

            if (reverse)
            {
                for (int i = 0; i < len; i++)
                    data[len - 1 - i] = Convert.ToByte(tmp.Substring(i * 2, 2), 16);
            }
            else
            {
                for (int i = 0; i < len; i++)
                    data[i] = Convert.ToByte(tmp.Substring(i * 2, 2), 16);
            }
            return data;
        }

        /// <summary>
        /// 读取安全模式参数
        /// <paramref name="state">01-启用，00-不启用</paramref>
        /// </summary>
        /// <returns></returns>
        public override bool SecurityParameter(byte state)
        {
            List<string> da = new List<string> { state.ToString("D2") };//参数2:工作模式

            StSIDMAC sidMac = new StSIDMAC
            {
                Data = new Dictionary<string, List<string>> { { "F1010200", da } }
            };

            StPackParas packPara = new StPackParas
            {
                MeterAddr = (MeterAddress.Length > 0) ? MeterAddress.PadLeft(12, '0') : "AAAAAAAAAAAA",
                SidMac = sidMac,
                SecurityMode = EmSecurityMode.ClearText,
                OD = new List<string> { "F1010200" }
            };

            int errCode = 0;
            List<object> LstObj = new List<object>();
            List<object> report = new List<object>();
            return SetData(packPara, ref LstObj, ref report, ref errCode);

        }

        /// <summary>
        /// 数据链路层
        /// </summary>
        private DataLinkLayer datalink = null;

        /// <summary>
        /// 应用连接请求
        /// </summary>
        /// <param name="frameData">帧</param>
        /// <param name="MeterAdd">表地址</param>
        /// <param name="ConnectInfo">认证机制信息</param>
        /// <returns>帧长度</returns>
        public override bool AppConnection(StConnectMechanismInfo ConnectInfo, ref List<object> LstObj, ref List<object> reportdata, ref int errCode)
        {
            datalink = new DataLinkLayer(MeterAddress) { APDU = new ApplicationConnectAPDU(ConnectInfo, EmSecurityMode.ClearText) };
            byte[] sendData = datalink.PackFrame(EmServieType.CONNECT_Request);
            byte[] recvData = new byte[0];

            ExeCommand(sendData, ref recvData);
            if (recvData.Length < 2)
                return false;

            return ParseFrame(recvData, ref LstObj, ref reportdata, ref errCode);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="objList">返回内容列表</param>
        public override bool ReadData(StPackParas para, ref List<object> objList, ref List<object> reportdata, ref int errCode)
        {
            byte[] recvData = new byte[0];
            byte[] sendData;
            if (para.SecurityMode == EmSecurityMode.ClearText) //明文
            {
                datalink = new DataLinkLayer(para.MeterAddr)
                {
                    APDU = new ReadDataAPDU(para.OD, EmSecurityMode.ClearText, para.GetRequestMode)
                };
                sendData = datalink.PackFrame(EmServieType.GET_Request);
            }
            else //明文 + mac
            {
                datalink = new DataLinkLayer(para.MeterAddr)
                {
                    APDU = new ReadDataAPDU(para.OD, EmSecurityMode.ClearTextMac, para.GetRequestMode, para.SidMac.Rand)
                };
                sendData = datalink.PackFrame(EmServieType.GET_Request);
            }

            ExeCommand(sendData, ref recvData);

            if (recvData.Length < 2) return false;

            bool t = ParseFrame(recvData, ref objList, ref reportdata, ref errCode);

            for (int i = 0; i < objList.Count; i++)
            {
                if (objList[i] == null || t == false)
                {
                    string v = "返回报文：" + BitConverter.ToString(recvData);
                    v += "\r\n" + errCode;
                    v += "\r\n========================================================================================";
                    Console.WriteLine(v);
                }
            }
            return t;
        }

        /// <summary>
        /// 读载波，HPLC ID信息
        /// </summary>
        /// <param name="sendData"></param>
        /// <returns></returns>
        public override Dictionary<string, string> ReadHplcID()
        {
            CL3762_RequestAFN10ReplayPacket req = new CL3762_RequestAFN10ReplayPacket();
            CL3762_RequestAFN10Packet res = new CL3762_RequestAFN10Packet(112);


            Thread.Sleep(1000);//做延时处理，不然有时候会导致485报文发送失败2015年9月22日 13:23:43
            base.SendData(res, req);

            return req.Data;
        }

        /// <summary>
        /// 读取记录数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        public override bool ReadRecordData(StPackParas DataInfo, ref List<object> data, ref Dictionary<string, List<object>> DicObj, int recordNo, List<string> rcsd, ref int errCode)
        {
            data = new List<object>();
            DicObj = new Dictionary<string, List<object>>();
            errCode = 0;
            byte[] sendData;
            byte[] recvData = new byte[0];


            if (DataInfo.SecurityMode == EmSecurityMode.ClearText)
            {
                datalink = new DataLinkLayer(DataInfo.MeterAddr)
                {
                    APDU = new ReadDataAPDU(DataInfo.OD, EmSecurityMode.ClearText, DataInfo.GetRequestMode)
                };
                sendData = datalink.PackFrame(EmServieType.GET_Request);
            }
            else
            {
                datalink = new DataLinkLayer(DataInfo.MeterAddr)
                {
                    APDU = new ReadDataAPDU(DataInfo.OD, EmSecurityMode.ClearTextMac, DataInfo.GetRequestMode, DataInfo.SidMac.Rand)
                    {
                        RecordNo = recordNo,  //读取记录型数据时，上N次记录
                        CSD = rcsd,                    //RCSD集合
                    }

                };
                sendData = datalink.PackFrame(EmServieType.GET_Request);
            }

            ExeCommand(sendData, ref recvData);
            if (recvData.Length < 2)
                return false;
            bool bResult = ParseFrame(recvData, ref data, ref DicObj, ref errCode);

            return bResult;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="objList">返回内容列表</param>
        public bool SetData(StPackParas dataInfo, ref List<object> objList, ref List<object> reportdata, ref int errCode)
        {
            byte[] recvData = new byte[0];

            SetData(out byte[] sendData, dataInfo);

            ExeCommand(sendData, ref recvData);
            if (recvData.Length < 2)
                return false;

            return ParseFrame(recvData, ref objList, ref reportdata, ref errCode);
        }

        /// <summary>
        /// 设置属性组帧
        /// </summary>
        /// <param name="frameData">返回帧</param>
        /// <param name="dataInfo">参数信息</param>
        /// <returns>帧长度</returns>
        public int SetData(out byte[] frameData, StPackParas dataInfo)
        {
            datalink = new DataLinkLayer(dataInfo.MeterAddr) { APDU = new SetDataAPDU(dataInfo.OD, dataInfo.SidMac, dataInfo.SecurityMode) };
            frameData = datalink.PackFrame(EmServieType.SET_Request);
            return frameData.Length;

        }

        /// <summary>
        /// 操作组帧
        /// </summary>        
        /// <param name="dataInfo">参数信息</param>
        /// <returns>帧长度</returns>
        public override bool Operation(StPackParas dataInfo, ref List<object> objList, ref List<object> reportdata, ref int errCode)
        {

            byte[] recvData = new byte[0];

            datalink = new DataLinkLayer(dataInfo.MeterAddr) { APDU = new OperationAPDU(dataInfo.OD, dataInfo.SidMac, dataInfo.SecurityMode) };
            byte[] sendData = datalink.PackFrame(EmServieType.ACTION_Request);

            ExeCommand(sendData, ref recvData);

            return ParseFrame(recvData, ref objList, ref reportdata, ref errCode);
        }

        /// <summary>
        /// 分帧操作，针对密钥下装，必须分帧
        /// </summary>
        /// <param name="frameDatas">帧数据</param>
        /// <param name="dataInfo">参数</param>
        /// <returns>帧长</returns>
        public override bool OperationSubFrame(StPackParas dataInfo, ref List<object> objList, ref List<object> reportdata, ref int errCode)
        {
            int frameNo = 0;
            bool bResult = true;

            datalink = new DataLinkLayer(dataInfo.MeterAddr) { APDU = new OperationAPDU(dataInfo.OD, dataInfo.SidMac, dataInfo.SecurityMode) };
            List<byte[]> frameDatas = datalink.PackOpertionSubFrame();

            foreach (byte[] sendData in frameDatas)
            {
                byte[] recvData = new byte[0];
                ExeCommand(sendData, ref recvData);

                if (frameNo == frameDatas.Count - 1)
                    bResult = ParseFrame(recvData, ref objList, ref reportdata, ref errCode);
                else
                    bResult = ParseSubFrame(recvData, frameNo);

                if (!bResult) break;

                frameNo++;
            }
            return bResult;

        }

        /// <summary>
        /// 解析分帧确认帧
        /// </summary>
        /// <param name="recvFrame">接收帧</param>        
        /// <param name="frameNo">帧序号</param>
        /// <returns></returns>
        public bool ParseSubFrame(byte[] recvFrame, int frameNo)
        {
            if (recvFrame.Length <= 0) return false;
            datalink = new DataLinkLayer();
            return datalink.ParseSubFrame(recvFrame, frameNo);

        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="recvFrame">帧</param>
        /// <param name="data">返回数据</param>
        /// <param name="errCode">错误代码</param>
        /// <returns></returns>
        public bool ParseFrame(byte[] recvFrame, ref List<object> data, ref List<object> reportdata, ref int errCode)
        {
            if (recvFrame.Length <= 0) return false;
            datalink = new DataLinkLayer();
            return datalink.ParseFrame(recvFrame, ref errCode, ref data, ref reportdata);

        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="ReceiveFrame">帧</param>
        /// <param name="Data">返回数据</param>
        /// <param name="errCode">错误代码</param>
        /// <returns></returns>
        public bool ParseFrame(byte[] ReceiveFrame, ref List<object> CsdData, ref Dictionary<string, List<object>> DicObj, ref int errCode)
        {
            datalink = new DataLinkLayer();
            return datalink.ParseFrame(ReceiveFrame, ref errCode, ref CsdData, ref DicObj);

        }

        //public override bool ReadRecordData(StPackParas dataInfo, int recordNo, ref List<object> LstCsd, ref Dictionary<string, List<object>> DicObj, ref int errCode)
        //{
        //    LstCsd = new List<object>();
        //    DicObj = new Dictionary<string, List<object>>();
        //    errCode = 0;
        //    byte[] revData = new byte[0];
        //    byte[] sendData;
        //    if (dataInfo.SecurityMode == EmSecurityMode.ClearText)
        //    {
        //        datalink = new DataLinkLayer(dataInfo.MeterAddr)
        //        {
        //            APDU = (APDU)new ReadDataAPDU(dataInfo.OD, EmSecurityMode.ClearText, dataInfo.GetRequestMode)
        //        };
        //        sendData = datalink.PackFrame(EmServieType.GET_Request);
        //    }
        //    else
        //    {
        //        datalink = new DataLinkLayer(dataInfo.MeterAddr)
        //        {
        //            APDU = (APDU)new ReadDataAPDU(dataInfo.OD, EmSecurityMode.ClearTextMac, dataInfo.GetRequestMode, dataInfo.SidMac.Rand)
        //        };
        //        sendData = datalink.PackFrame(EmServieType.GET_Request);
        //    }
        //    ExeCommand(sendData, ref revData);
        //    if (revData.Length < 2)
        //        return false;
        //    return ParseFrame(revData, ref LstCsd, ref DicObj, ref errCode);
        //}



        #region 物联网表--多芯表
        readonly int cxcount = 3;//失败后重新发送的次数

        //发送数据，助阵
        public byte[] IOTMete_Framing(byte controlType, byte[] data)
        {
            List<byte> fram = new List<byte>
            {
                //帧头
                0x7E,
                0x7E,
                0x7E,
                0x5A
            };

            byte len = (byte)(0x06 + data.Length);
            //长度
            fram.Add(len);
            //功能码
            fram.Add(controlType);
            if (data.Length > 0)
            {
                //数据区
                fram.AddRange(data);
            }



            //校验位
            byte cs = 0;
            for (int i = 0; i < fram.Count; i++)
            {
                cs += fram[i];
            }
            fram.Add(cs);

            //帧尾
            fram.Add(0x7E);
            fram.Add(0xA5);
            return fram.ToArray();
        }

        /// <summary>
        /// 解析帧
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public byte IOTMete_ParseFrame(byte[] recvData)
        {
            string str = "";
            for (int i = 0; i < recvData.Length; i++)
            {
                str += recvData[i].ToString("x2");
            }
            str = str.ToUpper();
            //str = str.Substring(0, str.LastIndexOf("7EA5")+4);
            if (recvData.Length < 9)
            {
                return 0x01;
            }
            int len = recvData[4] + 3;
            byte[] recvData2 = new byte[len];
            Array.Copy(recvData, recvData2, len);
            if (str.IndexOf("7E7E7E5A") == -1)
            {
                return 0x01;
            }
            byte cs = 0x00;
            //判断校验位是否正确
            for (int i = 0; i < recvData2.Length - 3; i++)
            {
                cs += recvData[i];
            }
            if (cs != recvData[recvData2.Length - 3])
            {
                return 0x01;
            }
            //if (recvData[recvData.Length - 4] == 0x00)
            //{
            //    Console.WriteLine($"接收数据{str}--成功");
            //    return true;
            //}
            //byte cs = 0x00;
            ////判断校验位是否正确
            //for (int i = 0; i < recvData.Length-3; i++)
            //{
            //    cs += recvData[i];
            //}
            //if (cs!= recvData[recvData.Length - 3])
            //{
            //    return false;
            //}


            if (recvData[6] == 0x00)
            {
                Console.WriteLine($"接收数据{str}--成功");
            }
            else
            {
                Console.WriteLine($"接收数据{str}--失败");
            }
            return recvData[6];
        }

        /// <summary>
        ///  物联网表--复位
        /// </summary>
        /// <returns></returns>
        public override bool IOTMete_Reset()
        {
            bool resoukt = false;
            for (int i = 0; i < cxcount; i++)
            {
                byte control = 0x00;
                byte[] data = new byte[0];
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData) == 0x00;
                if (resoukt) break;
            }
            return resoukt;
        }
        /// <summary>
        ///物联网表-- 连接电表
        /// </summary>
        /// <param name="Address">表地址</param>
        /// <returns></returns>
        public override bool IOTMete_Connect(string Address, string ping)
        {

            bool resoukt = false;
            for (int j = 0; j < cxcount; j++)
            {
                byte control = 0x01;
                byte[] data;
                if (ping != "") //说明有密码比如123456
                {

                    //把每一位补成俩位，加上48后在转成16进制--就是ascii
                    string possword = "";
                    for (int i = 0; i < ping.Length; i++)
                    {
                        possword += (int.Parse(ping[i].ToString()) + 48).ToString("x");
                    }
                    data = GetBytesArry(possword.Length / 2 + 6, possword + Address, true);
                }
                else
                {
                    data = GetBytesArry(6, Address, true);
                }
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData) == 0x00;
                if (resoukt) break;
            }
            return resoukt;

        }
        /// <summary>
        ///  物联网表--预处理--需要等待35秒以上
        /// </summary>
        /// <returns></returns>
        public override bool IOTMete_Pretreatment()
        {
            bool resoukt = false;
            for (int i = 0; i < cxcount; i++)
            {
                byte control = 0x07;
                byte[] data = new byte[0];
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData) == 0x00;
                if (resoukt) break;
            }
            return resoukt;
        }

        /// <summary>
        ///  物联网表--预处理-状态查询
        /// </summary>
        /// <returns></returns>
        public override bool IOTMete_PretreatmentSelect()
        {
            bool resoukt = false;
            for (int i = 0; i < cxcount; i++)
            {
                byte control = 0x08;
                byte[] data = new byte[0];
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData) == 0x00;
                if (resoukt) break;
            }
            return resoukt;
        }
        /// <summary>
        ///  物联网表--切换到光脉冲
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <returns></returns>
        public override bool IOTMete_SetLightPulse(byte pulseType)
        {
            bool resoukt = false;
            for (int i = 0; i < cxcount; i++)
            {
                byte control = 0x09;
                byte[] data = new byte[2];
                data[0] = 0x00;
                data[01] = pulseType;
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData) == 0x00;
                if (resoukt) break;
            }
            return resoukt;
        }
        /// <summary>
        ///   切换电表到检定模式(待测电能表蓝牙模式切换)：0x02--参数：脉冲类型，通道数量，发射频率，频段
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="powerGear">频段00全频段。01带内频段，02带外频段</param>
        /// <param name="powerGear">通道数量</param>
        /// <returns>0成功，1失败，3未授权</returns>
        public override byte IOTMete_SetMeterTestModel(byte pulseType, byte powerGear, byte frequencyBand, byte passagewayCount)
        {

            byte resoukt = 0xFF;
            for (int i = 0; i < cxcount; i++)
            {
                byte control = 0x02;
                byte[] data = new byte[6];
                data[0] = 0x01;
                data[1] = pulseType;
                data[2] = powerGear;
                data[3] = frequencyBand;
                data[4] = 0x01;
                data[5] = passagewayCount;
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData);
                if (resoukt == 0x00) break;
            }
            return resoukt;
        }

        /// <summary>
        ///  切换转换器到检定模式(转换器蓝牙模式切换)---脉冲类型，发射功率，通信模式
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="model">模式--00普通检定模式--10脉冲跟随模式</param>
        /// <returns></returns>
        public override bool IOTMete_SetConverterModel(byte pulseType, byte powerGear, byte model)
        {
            bool resoukt = false;
            for (int i = 0; i < cxcount; i++)
            {
                byte control = 0x03;
                byte[] data = new byte[4];
                data[0] = 0x01;
                data[1] = pulseType;
                data[2] = powerGear;
                data[3] = model;
                byte[] recvData = new byte[0];
                byte[] sendData = IOTMete_Framing(control, data);
                ExeCommand(sendData, ref recvData);
                resoukt = IOTMete_ParseFrame(recvData) == 0x00;
                if (resoukt) break;
            }
            return resoukt;
        }

        #endregion




    }
}
