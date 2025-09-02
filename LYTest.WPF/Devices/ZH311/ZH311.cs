using E_ZH311;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH.Struct;

namespace ZH
{
    public class ZH311
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private readonly StPortInfo m_PowerSourcePort = null;

        private readonly DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZH311()
        {
            m_PowerSourcePort = new StPortInfo();
            driverBase = new DriverBase();
        }

        #region IClass_Interface 成员
        /// <summary>
        /// 初始化UDP端口
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">帧命令最长等待时间</param>
        /// <param name="WaitSencondsPerByte">帧字节等待时间</param>
        /// <param name="IP">Ip地址</param>
        /// <param name="RemotePort">远程端口</param>
        /// <param name="LocalStartPort">本地端口</param>
        /// <returns></returns>
        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort)
        {
            m_PowerSourcePort.m_Exist = 1;
            m_PowerSourcePort.m_IP = IP;
            m_PowerSourcePort.m_Port = ComNumber;
            m_PowerSourcePort.m_Port_IsUDPorCom = true;
            m_PowerSourcePort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_PowerSourcePort.m_Port_Setting, m_PowerSourcePort.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {
                return 1;
            }

            return 0;
        }
        /// <summary>
        /// 初始化Com 口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_PowerSourcePort.m_Exist = 1;
            m_PowerSourcePort.m_IP = "";
            m_PowerSourcePort.m_Port = ComNumber;
            m_PowerSourcePort.m_Port_IsUDPorCom = false;
            m_PowerSourcePort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {
                return 1;
            }
            return 0;
        }
        /// <summary>
        /// 连机
        /// </summary>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH311_RequestLinkPacket rc3 = new ZH311_RequestLinkPacket();
            ZH311_RequestLinkReplyPacket recv3 = new ZH311_RequestLinkReplyPacket();
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool linkClockOk = recv3.ReciveResult == RecvResult.OK;
                        string Clockmessage = string.Format("表联机{0}。", linkClockOk ? "成功" : "失败");
                        return linkClockOk ? 0 : 1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {

                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

        }
        /// <summary>
        /// 脱机
        /// </summary>
        /// <returns></returns>
        public int DisConnect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }


        /// <summary>
        /// 读取累积电量
        /// </summary>
        /// <param name="flaEnergy">
        /// 101f:
        /// A相有功电能，A相无功电能，A相视在电能，
        /// B相有功电能，B相无功电能，B相视在电能，
        /// C相有功电能，C相无功电能，C相视在电能，
        /// 总有功电能，总无功电能，总视在电能.
        /// (新加4个)总有功电能I，总有功电能II，总无功电能III，总无功电能IV.
        /// 1026:
        /// A相有功电能I,B相有功电能I,C相有功电能I,总有功电能I,
        /// A相有功电能II,B相有功电能II,C相有功电能II,总有功电能II,
        /// A相无功电能III,B相无功电能III,C相无功电能III,总无功电能III,
        /// A相无功电能IV,B相无功电能IV,C相无功电能IV,总无功电能IV. 
        /// </param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadEnergy(out float[] flaEnergy, out string[] FrameAry)
        {
            flaEnergy = new float[16];
            FrameAry = new string[1];
            try
            {
                //int result = readEnergy1026(out float[] etmp, out FrameAry);
                //if (result == 0)
                //{
                //    flaEnergy[9] = Math.Abs(etmp[3]) + Math.Abs(etmp[7]);
                //    flaEnergy[10] = Math.Abs(etmp[15]) + Math.Abs(etmp[11]);
                //    //11视在
                //    flaEnergy[12] = etmp[3];//正有
                //    flaEnergy[13] = etmp[7];//反有
                //    flaEnergy[14] = etmp[11];//反无
                //    flaEnergy[15] = etmp[15];//正无
                //    return result;
                //}
                //else
                {
                    Dictionary<string, string> strDictionary = new Dictionary<string, string>
                    {
                        { "101f", "" }
                    };
                    int r = SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
                    if (r == 0)
                    {
                        byte[] byteDa = new byte[10];
                        float[] instValue = new float[16];
                        int count = (strRev.Length - 3) / 10;
                        if (strRev.Length >= 3 + (16 * 10))
                        {
                            for (int i = 0; i < instValue.Length && i < count; i++)
                            {
                                Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                                if (float.TryParse(Encoding.ASCII.GetString(byteDa), out float tmp))
                                {
                                    instValue[i] = tmp;
                                }
                            }
                        }
                        else if (strRev.Length >= 3 + (12 * 10))
                        {
                            for (int i = 0; i < instValue.Length && i < count; i++)
                            {
                                Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                                if (float.TryParse(Encoding.ASCII.GetString(byteDa), out float tmp))
                                {
                                    instValue[i] = tmp;
                                }
                            }
                        }
                        flaEnergy = instValue;
                        if (count > 12)
                        {
                            flaEnergy[9] = Math.Max(Math.Abs(instValue[12]), Math.Abs(instValue[13]));
                            flaEnergy[10] = Math.Max(Math.Abs(instValue[15]), Math.Abs(instValue[14]));
                        }
                    }
                    return r;
                }
            }
            catch
            {
                return -1;
            }
        }

        private int ReadEnergy1026(out float[] flaEnergy, out string[] FrameAry)
        {
            flaEnergy = new float[16];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1026", "" }
            };
            try
            {
                int r = SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
                if (r == 0)
                {
                    byte[] byteDa = new byte[10];
                    float[] instValue = new float[16];
                    if (strRev.Length >= 3 + (16 * 10))
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                            if (float.TryParse(Encoding.ASCII.GetString(byteDa), out float tmp))
                            {
                                instValue[i] = tmp;
                            }
                        }
                        flaEnergy = instValue;
                        return 0;
                    }
                    flaEnergy = instValue;
                    return 2;
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        //add yjt 20230103 读取标准表走字电量
        /// <summary>
        /// 读取标准表走字电量,不用
        /// </summary>
        /// <returns></returns>
        public string[] ReadEnrgyZh311(out string[] flaEnergy, out string[] FrameAry)
        {
            flaEnergy = new string[12];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1003", "" }
            };
            SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
            //List<byte[]> listbyte = new List<byte[]>();
            byte[] byteDa = new byte[10];
            if (strRev.Length == 123)
            {
                for (int i = 0; i < flaEnergy.Length; i++)
                {

                    Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                    flaEnergy[i] = Encoding.ASCII.GetString(byteDa);
                }
            }
            return flaEnergy;
        }

        /// <summary>
        /// 1004H-谐波含量
        /// </summary>
        /// <param name="flaHarmonic"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadHarmonicEnergy(out float[] flaHarmonic, out string[] FrameAry)
        {
            flaHarmonic = new float[12];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1004", "" }
            };
            try
            {
                int r = SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
                try
                {
                    byte[] byteDa = new byte[10];
                    float[] instValue = new float[192];
                    for (int i = 0; i < instValue.Length; i++)
                    {
                        Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                        instValue[i] = Convert.ToSingle(Encoding.ASCII.GetString(byteDa));
                    }
                    flaHarmonic = instValue;

                }
                catch (Exception)
                {
                }
                return r;
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 1005H-谐波相角
        /// </summary>
        /// <param name="flaHarmonicAngle"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadHarmonicAngle(out float[] flaHarmonicAngle, out string[] FrameAry)
        {
            flaHarmonicAngle = new float[12];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1005", "" }
            };
            try
            {
                return SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 1006H-谐波有功功率
        /// </summary>
        /// <param name="flaHarmonicActivePower"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadHarmonicActivePower(out float[] flaHarmonicActivePower, out string[] FrameAry)
        {
            flaHarmonicActivePower = new float[12];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1006", "" }
            };
            try
            {
                int r = SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
                try
                {
                    byte[] byteDa = new byte[10];
                    float[] instValue = new float[192];
                    for (int i = 0; i < instValue.Length; i++)
                    {
                        Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                        instValue[i] = Convert.ToSingle(Encoding.ASCII.GetString(byteDa));
                    }
                    flaHarmonicActivePower = instValue;

                }
                catch (Exception)
                {
                }
                return r;
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 1007H-谐波有功电能
        /// </summary>
        /// <param name="flaHarmonicActiveEnergy"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadHarmonicActiveEnergy(out float[] flaHarmonicActiveEnergy, out string[] FrameAry)
        {
            flaHarmonicActiveEnergy = new float[12];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1007", "" }
            };
            try
            {
                int r = SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
                byte[] byteDa = new byte[10];
                float[] instValue = new float[192];
                for (int i = 0; i < instValue.Length; i++)
                {
                    Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                    instValue[i] = Convert.ToSingle(System.Text.Encoding.ASCII.GetString(byteDa));
                }
                flaHarmonicActiveEnergy = instValue;

                return r;
            }
            catch (Exception)
            {
            }
            return 0;


        }

        /// <summary>
        /// 1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="stdCmd">0x10 读 ，0x13写</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int StdGear(byte stdCmd, out string[] FrameAry)
        {
            //stdUIGear = new float[7];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1008", "" }
            };
            if (stdCmd == 0x10)
            {
                strDictionary.Add("1008", "");
            }
            try
            {
                return SendData(stdCmd, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }


        #region  设置被检表参数

        /// <summary>
        /// 1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="stdCmd">0x10 读 ，0x13写</param>
        /// <param name="stdUIGear"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int StdGear2(byte stdCmd, ref ulong stdConst, ref double[] stdUIGear, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ASCIIEncoding charToASCII = new ASCIIEncoding();
            try
            {
                byte[] frame = new byte[26];
                frame[0] = 0x10;
                frame[1] = 0x08;
                //char[] charBuf = new char[2];
                byte[] strSend = new byte[2];
                //char[] charBuf2 = new char[10];

                char[] charBuf = stdUIGear[0].ToString().ToCharArray();
                int lenght = 2;
                if (charToASCII.GetBytes(charBuf).Length < 2)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
                strSend.CopyTo(frame, 2);

                charBuf = stdUIGear[1].ToString().ToCharArray();
                lenght = 2;
                if (charToASCII.GetBytes(charBuf).Length < 2)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
                strSend.CopyTo(frame, 4);

                charBuf = stdUIGear[2].ToString().ToCharArray();
                lenght = 2;
                if (charToASCII.GetBytes(charBuf).Length < 2)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
                strSend.CopyTo(frame, 6);

                charBuf = stdUIGear[3].ToString().ToCharArray();
                lenght = 2;
                if (charToASCII.GetBytes(charBuf).Length < 2)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
                strSend.CopyTo(frame, 8);

                charBuf = stdUIGear[4].ToString().ToCharArray();
                lenght = 2;
                if (charToASCII.GetBytes(charBuf).Length < 2)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
                strSend.CopyTo(frame, 10);

                charBuf = stdUIGear[5].ToString().ToCharArray();
                lenght = 2;
                if (charToASCII.GetBytes(charBuf).Length < 2)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
                strSend.CopyTo(frame, 12);

                charBuf = stdConst.ToString().ToCharArray();
                lenght = 12;
                if (charToASCII.GetBytes(charBuf).Length <= 10)
                { lenght = charToASCII.GetBytes(charBuf).Length; }

                //byte[] s= charToASCII.GetBytes(charBuf);
                byte[] strSend2 = new byte[10];
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend2, 0, lenght);
                strSend2.CopyTo(frame, 14);

                // return sendData(stdCmd, strDictionary, out strRev, out FrameAry);
                if (SendData(stdCmd, frame, out byte[] strRev, out FrameAry) == 0)
                {
                    if (strRev.Length > 26)
                    {
                        byte[] b = new byte[12];
                        int index = 0;
                        for (int i = 15; i < strRev.Length; i++)
                        {
                            b[index] = strRev[i];

                            index++;
                        }
                        string s = Encoding.ASCII.GetString(b);
                        double.TryParse(s, out double v);
                        stdConst = (ulong)v;
                    }

                    return 0;
                }
                else
                {
                    return 1;
                }



            }
            catch
            {

                return -1;
            }
        }
        #endregion

        /// <summary>
        /// 1009H-电压电流标准值
        /// </summary>
        /// <param name="Ua"></param>
        /// <param name="Ub"></param>
        /// <param name="Uc"></param>
        /// <param name="Ia"></param>
        /// <param name="Ib"></param>
        /// <param name="Ic"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int CalibrationPowerAmplitude(out string[] FrameAry)
        {
            FrameAry = new string[1];

            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1009", "" }
            };
            try
            {
                return SendData(0x13, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 100aH-相位标准值 下发
        /// </summary>
        /// <param name="PhiUa">A相电压角度</param>
        /// <param name="PhiUb">B相电压角度</param>
        /// <param name="PhiUc">C相电压角度</param>
        /// <param name="PhiIa">A相电流角度</param>
        /// <param name="PhiIb">B相电流角度</param>
        /// <param name="PhiIc">C相电流角度</param>
        /// <returns></returns>
        public int CalibrationPowerAngle(out string[] FrameAry)
        {

            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100A", "" }
            };
            try
            {
                return SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 读取瞬时测量数据
        /// </summary>
        /// <param name="instValue"></param>
        /// <returns></returns>
        public int ReadInstMetricAll(out float[] instValue, out string[] FrameAry)
        {
            instValue = new float[34];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1000", "" },
                { "1001", "" },
                { "1002", "" }
            };
            try
            {
                return SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }

        }

        /// <summary>
        ///  100bH-模式设置
        /// </summary>
        /// <param name="stdCmd"></param>
        /// <param name="strModeJxfs"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int StdMode(byte stdCmd, out string[] FrameAry)
        {

            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100B", "" }
            };
            if (stdCmd == 0x10)
            {
                strDictionary.Add("1008", "");
            }
            try
            {
                return SendData(stdCmd, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 100cH-启停标准表累积电能
        /// </summary>
        /// <param name="startOrStopStd">字符’1’表示清零当前电能并开始累计（ascii 码读取）</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int StartStdEnergy(int startOrStopStd, out string[] FrameAry)
        {
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100C", startOrStopStd.ToString() }
            };

            try
            {
                return SendData(0x13, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 1028H-间谐波有功功率
        /// </summary>
        /// <param name="flaHarmonicActivePower"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadInterHarmonicActivePower(out float[] flaHarmonicActivePower, out string[] FrameAry)
        {
            flaHarmonicActivePower = new float[12];
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1028", "" }
            };
            try
            {
                int r = SendData(0x10, strDictionary, out byte[] strRev, out FrameAry);
                try
                {
                    byte[] byteDa = new byte[10];
                    float[] instValue = new float[384];
                    for (int i = 0; i < instValue.Length; i++)
                    {
                        Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                        instValue[i] = Convert.ToSingle(Encoding.ASCII.GetString(byteDa));
                    }
                    flaHarmonicActivePower = instValue;

                }
                catch (Exception ex)
                {
                    string a = ex.ToString();
                }
                return r;
            }
            catch (Exception)
            {

                return -1;
            }
        }

        /// <summary>
        /// 1029H-设置谐波采样模式
        /// </summary>
        /// <param name="ModelType">0/1 普通采样模式/谐波采样模式</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ChangeHarmonicModel(int ModelType, out string[] FrameAry)
        {
            FrameAry = new string[1];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1029", ModelType.ToString() }
            };

            try
            {
                return SendData(0x13, strDictionary, out byte[] strRev, out FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }

        ///// <summary>
        ///// 设置挡位
        ///// </summary>
        ///// <param name="UA"></param>
        ///// <param name="UB"></param>
        ///// <param name="UC"></param>
        ///// <param name="IA"></param>
        ///// <param name="IB"></param>
        ///// <param name="IC"></param>
        ///// <returns></returns>
        //public bool setMeterDWZH311(float UA, float UB, float UC, float IA, float IB, float IC)
        //{

        //ZH3130_RequestSendDataReplyPacket rcPowerHarmonic = new ZH3130_RequestSendDataReplyPacket();

        //float UMax = Math.Max(Math.Max(UA, UB), UC);
        //int UValue = rcPowerHarmonic.GetDYString(UMax);
        //byte[] bydata = Encoding.ASCII.GetBytes((UValue).ToString());

        //float IMax = Math.Max(Math.Max(IA, IB), IC);
        //int IValue = rcPowerHarmonic.GetDLString(IMax);
        //byte[] bydata1 = Encoding.ASCII.GetBytes((IValue).ToString());

        //string[] FrameAry = new string[1];
        //byte[] strRev = new byte[0];
        //float[] stdUIGear = new float[0];
        ////Dictionary<string, string> strDictionary = new Dictionary<string, string>();

        //byte[] byteDate = new byte[26];
        //byteDate[0] = 0x10;
        //byteDate[1] = 0x08;

        //byteDate[2] = 0x30;
        //byteDate[3] = bydata[0];
        //if (bydata1.Length >= 2)
        //{
        //    byteDate[4] = bydata1[0];
        //    byteDate[5] = bydata1[1];
        //}
        //else
        //{
        //    byteDate[4] = 0x00;
        //    byteDate[5] = bydata1[0];
        //}

        //byteDate[6] = 0x30;
        //byteDate[7] = bydata[0];


        //if (bydata1.Length >= 2)
        //{
        //    byteDate[8] = bydata1[0];
        //    byteDate[9] = bydata1[1];
        //}
        //else
        //{
        //    byteDate[8] = 0x00;
        //    byteDate[9] = bydata1[0];
        //}


        //byteDate[10] = 0x30;
        //byteDate[11] = bydata[0];

        //if (bydata1.Length >= 2)
        //{
        //    byteDate[12] = bydata1[0];
        //    byteDate[13] = bydata1[1];
        //}
        //else
        //{
        //    byteDate[12] = 0x00;
        //    byteDate[13] = bydata1[0];
        //}



        //byteDate[14] = 0x30;
        //byteDate[15] = 0x30;
        //byteDate[16] = 0x30;
        //byteDate[17] = 0x30;
        //byteDate[18] = 0x30;
        //byteDate[19] = 0x30;
        //byteDate[20] = 0x30;
        //byteDate[21] = 0x30;
        //byteDate[22] = 0x30;
        //byteDate[23] = 0x30;
        //byteDate[24] = 0x30;
        //byteDate[25] = 0x30;

        //sendData(0x13, byteDate, out strRev, out FrameAry);

        //    return true;


        //}
        /// <summary>
        /// 读取瞬时测量数据
        /// </summary>
        /// <param name="instValue"></param>
        /// <returns></returns>
        public int ReadstdZH311Param(out float[] instValue, out string errmsg)
        {
            instValue = new float[29];
            try
            {
                string[] FrameAry = new string[1];
                byte[] strRev = new byte[0];
                Dictionary<string, string> strDictionary = new Dictionary<string, string>
                {
                    { "1000", "" },
                    { "1001", "" },
                    { "1002", "" }
                };
                SendData(0x10, strDictionary, out strRev, out FrameAry);
                List<byte[]> listbyte = new List<byte[]>();
                byte[] byteDa = new byte[10];
                int intCount = 1;
                if (strRev.Length == 297)
                {
                    try
                    {
                        for (int i = 0; i < instValue.Length; i++)
                        {
                            if (i == 6 || i == 17) intCount += 1;
                            Array.Copy(strRev, 1 + 2 * intCount + i * 10, byteDa, 0, 10);
                            instValue[i] = Convert.ToSingle(Encoding.ASCII.GetString(byteDa));
                        }
                        errmsg = "";
                        return 1;
                    }
                    catch (Exception ex)
                    {
                        errmsg = ex.Message;
                        for (int i = 0; i < instValue.Length; i++)
                        {
                            instValue[i] = 0;
                        }
                    }

                    //stdZh311.Ua = Convert.ToSingle(instValue[0]);
                    //stdZh311.Ub = Convert.ToSingle(instValue[2]);
                    //stdZh311.Uc = Convert.ToSingle(instValue[4]);

                    //stdZh311.Ia = Convert.ToSingle(instValue[1]);
                    //stdZh311.Ib = Convert.ToSingle(instValue[3]);
                    //stdZh311.Ic = Convert.ToSingle(instValue[5]);

                    //stdZh311.Phi_Ua = Convert.ToSingle(instValue[6]);
                    //stdZh311.Phi_Ub = Convert.ToSingle(instValue[8]);
                    //stdZh311.Phi_Uc = Convert.ToSingle(instValue[10]);

                    //stdZh311.Phi_Ia = Convert.ToSingle(instValue[7]);
                    //stdZh311.Phi_Ib = Convert.ToSingle(instValue[9]);
                    //stdZh311.Phi_Ic = Convert.ToSingle(instValue[11]);

                    //stdZh311.Freq = Convert.ToSingle(instValue[12]);
                    //stdZh311.Freq = Convert.ToSingle(instValue[12]);
                    //stdZh311.Freq = Convert.ToSingle(instValue[12]);

                    //stdZh311.Pa = Convert.ToSingle(instValue[17]);
                    //stdZh311.Pb = Convert.ToSingle(instValue[20]);
                    //stdZh311.Pc = Convert.ToSingle(instValue[23]);

                    //stdZh311.Qa = Convert.ToSingle(instValue[18]);
                    //stdZh311.Qb = Convert.ToSingle(instValue[21]);
                    //stdZh311.Qc = Convert.ToSingle(instValue[24]);

                    //stdZh311.Sa = Convert.ToSingle(instValue[19]);
                    //stdZh311.Sb = Convert.ToSingle(instValue[22]);
                    //stdZh311.Sc = Convert.ToSingle(instValue[25]);

                    //stdZh311.P = Convert.ToSingle(instValue[26]);
                    //stdZh311.Q = Convert.ToSingle(instValue[27]);
                    //stdZh311.S = Convert.ToSingle(instValue[28]);
                }
                else errmsg = $"长度错误{strRev.Length}";
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
            }

            return 2;
        }


        /// <summary>
        ///   设置脉冲
        /// </summary>
        /// <param name="pulseType">
        ///有功脉冲 设置字符’1’
        ///无功脉冲 设置字符’2’
        ///UA脉冲 设置字符’3’
        ///UB脉冲 设置字符’4’
        ///UC脉冲 设置字符’5’
        ///IA脉冲 设置字符’6’
        ///IB脉冲 设置字符’7’
        ///IC脉冲 设置字符’8’
        ///PA脉冲 设置字符’9’
        ///PB脉冲 设置字符’10’
        ///PC脉冲 设置字符’11’
        ///</param>
        /// <returns></returns>
        public int SetPulseType(string pulseType)
        {

            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100E", pulseType }
            };
            try
            {
                return SendData(0x13, strDictionary, out byte[] strRev, out string[] FrameAry);
            }
            catch (Exception)
            {

                return -1;
            }
        }

        /// <summary>
        /// 脉冲校准误差 字符串形式(10字节)W， 单位 分数值。
        /// 误差板的值 × 1000000 下发，譬如误差板计算的误差0.01525%，则0.01525% ×1000000 = 152.5，
        /// 则下发字符串 ”152.5”。
        /// </summary>
        ///  <param name="Error">误差板的值</param>
        /// <returns></returns>
        public int SetPulseCalibration(string Error)
        {
            try
            {
                byte[] strSendAll = new byte[12];
                strSendAll[0] = 0x10;
                strSendAll[1] = 0x12;
                char[] charBuf = new char[2];
                byte[] strSend2 = new byte[10];
                charBuf = Error.ToCharArray();
                int lenght = 10;

                ASCIIEncoding charToASCII = new ASCIIEncoding();
                if (charToASCII.GetBytes(charBuf).Length < 10)
                { lenght = charToASCII.GetBytes(charBuf).Length; }
                Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend2, 0, lenght);
                strSend2.CopyTo(strSendAll, 2);


                if (SendData(0x13, strSendAll, out byte[] strRev, out string[] FrameAry) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

            }
            catch (Exception)
            {

                return -1;
            }

        }

        #region 指令


        private int SendData(byte cmd, byte[] strDictionary, out byte[] RevbyteData, out string[] FrameAry)
        {
            FrameAry = new string[1];
            RevbyteData = new byte[1];
            //strDictionary = new Dictionary<string, string>();          
            ZH311_RequestDataPacket rc3 = new ZH311_RequestDataPacket();
            ZH311_RequestDataReplyPacket recv3 = new ZH311_RequestDataReplyPacket();
            rc3.SetPara(cmd, strDictionary);
            FrameAry[0] = BytesToString(rc3.GetPacketData());
            if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
            {
                RevbyteData = recv3.revbyteData;
            }
            else
            {
                return 1;
            }
            return 0;
        }


        ///// <summary>
        ///// 处理指令类 485端口
        ///// </summary>
        ///// <param name="cmd">命令码</param>
        ///// <param name="strDictionary">数据字典</param>
        ///// <param name="RevbyteData"></param>
        ///// <param name="FrameAry"></param>
        ///// <returns></returns>
        //private int sendData(int index, byte cmd, byte[] byteSend, out byte[] RevbyteData, out string[] FrameAry)
        //{
        //    FrameAry = new string[1];
        //    RevbyteData = new byte[1];
        //    //strDictionary = new Dictionary<string, string>();          
        //    ZH311_RequestDataPacket rc3 = new ZH311_RequestDataPacket();
        //    ZH311_RequestDataReplyPacket recv3 = new ZH311_RequestDataReplyPacket();
        //    rc3.SetPara(cmd, byteSend);
        //    FrameAry[0] = BytesToString(rc3.GetPacketData());
        //    //if (SendPacketWithRetry(m_arrRs485Port[index], rc3, recv3))
        //    //{
        //    //    RevbyteData = recv3.revbyteData;


        //    //}
        //    //else
        //    //{
        //    //    return 1;
        //    //}
        //    return 0;

        //}

        /// <summary>
        /// 处理指令类
        /// </summary>
        /// <param name="cmd">命令码</param>
        /// <param name="strDictionary">数据字典</param>
        /// <param name="RevbyteData"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        private int SendData(byte cmd, Dictionary<string, string> strDictionary, out byte[] RevbyteData, out string[] FrameAry)
        {
            FrameAry = new string[1];
            RevbyteData = new byte[0];
            //strDictionary = new Dictionary<string, string>();          
            ZH311_RequestDataPacket rc3 = new ZH311_RequestDataPacket();
            ZH311_RequestDataReplyPacket recv3 = new ZH311_RequestDataReplyPacket();
            rc3.SetPara(cmd, strDictionary);
            FrameAry[0] = BytesToString(rc3.GetPacketData());
            if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
            {
                RevbyteData = recv3.revbyteData;
            }
            else
            {
                return 1;
            }
            return 0;

        }

        /// <summary>
        /// 设置发送标志位
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public int SetSendFlag(bool Flag)
        {
            sendFlag = Flag;
            return 0;
        }
        /// <summary>
        /// 解析设备返回的的报文
        /// </summary>
        /// <param name="MothedName">方法名称</param>
        /// <param name="ReFrameAry">设备返回报文</param>
        /// <param name="ReAry">解析的数据</param>
        /// <returns></returns>
        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            int iRevalue = 0;
            ReAry = new string[1];

            switch (MothedName)
            {
                case "Connect":
                    {
                        try
                        {
                            ZH311_RequestLinkReplyPacket recv = new ZH311_RequestLinkReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "DisConnect":
                    {
                        ReAry[0] = string.Empty;
                    }
                    break;
                case "ReadGPSTime":
                    return -1;
                default:
                    {
                        ReAry[0] = "Null this Data";
                    }
                    break;
            }
            return iRevalue;
        }


        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="stPort">端口号</param>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {
            for (int i = 0; i < RETRYTIEMS; i++)
            {
                if (driverBase.SendData(stPort, sp, rp) == true)
                {
                    return true;
                }
                Thread.Sleep(300);
            }
            return false;
        }
        /// <summary>
        /// bytes转 string
        /// </summary>
        /// <param name="bytesData"></param>
        /// <returns></returns>
        private string BytesToString(byte[] bytesData)
        {
            string strRevalue = string.Empty;
            if (bytesData == null || bytesData.Length < 1)
                return strRevalue;

            for (int i = 0; i < bytesData.Length; i++)
            {
                strRevalue += Convert.ToString(bytesData[i], 16);
            }
            return strRevalue;
        }


        #endregion

        #endregion

    }
}
