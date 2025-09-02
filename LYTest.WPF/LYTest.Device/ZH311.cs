using LY.SocketModule.Packet;
using LYTest.Device.SocketModule;
using LYTest.Device.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace LYTest.Device
{
    public class ZH311 : DriverBase
    {
        /// <summary>
        /// 连机
        /// </summary>
        public int Connect()
        {
            ZH311_RequestLinkPacket rc3 = new ZH311_RequestLinkPacket();
            ZH311_RequestLinkReplyPacket recv3 = new ZH311_RequestLinkReplyPacket();
            //合成的报文

            if (SendPacketWithRetry(rc3, recv3))
            {
                bool r = recv3.ReciveResult == RecvResult.OK;
                return r ? 0 : 1;
            }
            else
            {
                return 1;
            }
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
        public bool ReadEnergy(out float[] flaEnergy)
        {
            flaEnergy = new float[16];

            Dictionary<string, string> strDictionary = new Dictionary<string, string>
                    {
                        { "101f", "" }
                    };
            bool r = SendData(0x10, strDictionary, out byte[] strRev);
            if (r)
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

        private bool ReadEnergy1026(out float[] flaEnergy)
        {
            flaEnergy = new float[16];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1026", "" }
            };
            bool r = SendData(0x10, strDictionary, out byte[] strRev);
            if (r)
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
                    return true;
                }
                flaEnergy = instValue;
                return false;
            }
            return false;

        }

        //add yjt 20230103 读取标准表走字电量
        /// <summary>
        /// 读取标准表走字电量,不用
        /// </summary>
        /// <returns></returns>
        public string[] ReadEnrgyZh311(out string[] flaEnergy)
        {
            flaEnergy = new string[12];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1003", "" }
            };
            SendData(0x10, strDictionary, out byte[] strRev);
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
        public bool ReadHarmonicEnergy(out float[] flaHarmonic)
        {
            flaHarmonic = new float[12];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1004", "" }
            };
            bool r = SendData(0x10, strDictionary, out byte[] strRev);
            if (r)
            {
                byte[] byteDa = new byte[10];
                float[] instValue = new float[192];
                for (int i = 0; i < instValue.Length; i++)
                {
                    Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                    instValue[i] = Convert.ToSingle(Encoding.ASCII.GetString(byteDa));
                }
                flaHarmonic = instValue;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 1005H-谐波相角
        /// </summary>
        /// <param name="flaHarmonicAngle"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool ReadHarmonicAngle(out float[] flaHarmonicAngle)
        {
            flaHarmonicAngle = new float[12];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1005", "" }
            };

            return SendData(0x10, strDictionary, out byte[] strRev);

        }

        /// <summary>
        /// 1006H-谐波有功功率
        /// </summary>
        /// <param name="flaHarmonicActivePower"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool ReadHarmonicActivePower(out float[] flaHarmonicActivePower)
        {
            flaHarmonicActivePower = new float[12];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1006", "" }
            };
            bool r = SendData(0x10, strDictionary, out byte[] strRev);
            if (r)
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



            return r;

        }
        /// <summary>
        /// 1007H-谐波有功电能
        /// </summary>
        /// <param name="flaHarmonicActiveEnergy"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool ReadHarmonicActiveEnergy(out float[] flaHarmonicActiveEnergy)
        {
            flaHarmonicActiveEnergy = new float[12];
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1007", "" }
            };
            bool r = SendData(0x10, strDictionary, out byte[] strRev);
            if (r)
            {
                byte[] byteDa = new byte[10];
                float[] instValue = new float[192];
                for (int i = 0; i < instValue.Length; i++)
                {
                    Array.Copy(strRev, 3 + i * 10, byteDa, 0, 10);
                    instValue[i] = Convert.ToSingle(System.Text.Encoding.ASCII.GetString(byteDa));
                }
                flaHarmonicActiveEnergy = instValue;
            }


            return r;



        }

        /// <summary>
        /// 1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="cmd">0x10 读 ，0x13写</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool StdGear(byte cmd)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "1008", "" }
            };
            if (cmd == 0x10)
            {
                dic.Add("1008", "");
            }

            return SendData(cmd, dic, out _);

        }


        #region  设置被检表参数

        /// <summary>
        /// 1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="stdCmd">0x10 读 ，0x13写</param>
        /// <param name="stdUIGear"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int StdGear2(byte stdCmd, ref ulong stdConst, ref double[] stdUIGear)
        {
            ASCIIEncoding charToASCII = new ASCIIEncoding();
            byte[] strSendAll = new byte[26];
            strSendAll[0] = 0x10;
            strSendAll[1] = 0x08;

            byte[] strSend = new byte[2];


            char[] charBuf = stdUIGear[0].ToString().ToCharArray();
            int lenght = 2;
            if (charToASCII.GetBytes(charBuf).Length < 2)
            { lenght = charToASCII.GetBytes(charBuf).Length; }
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
            strSend.CopyTo(strSendAll, 2);

            charBuf = stdUIGear[1].ToString().ToCharArray();
            lenght = 2;
            if (charToASCII.GetBytes(charBuf).Length < 2)
            { lenght = charToASCII.GetBytes(charBuf).Length; }
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
            strSend.CopyTo(strSendAll, 4);

            charBuf = stdUIGear[2].ToString().ToCharArray();
            lenght = 2;
            if (charToASCII.GetBytes(charBuf).Length < 2)
            { lenght = charToASCII.GetBytes(charBuf).Length; }
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
            strSend.CopyTo(strSendAll, 6);

            charBuf = stdUIGear[3].ToString().ToCharArray();
            lenght = 2;
            if (charToASCII.GetBytes(charBuf).Length < 2)
            { lenght = charToASCII.GetBytes(charBuf).Length; }
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
            strSend.CopyTo(strSendAll, 8);

            charBuf = stdUIGear[4].ToString().ToCharArray();
            lenght = 2;
            if (charToASCII.GetBytes(charBuf).Length < 2)
            { lenght = charToASCII.GetBytes(charBuf).Length; }
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
            strSend.CopyTo(strSendAll, 10);

            charBuf = stdUIGear[5].ToString().ToCharArray();
            lenght = 2;
            if (charToASCII.GetBytes(charBuf).Length < 2)
            { lenght = charToASCII.GetBytes(charBuf).Length; }
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend, 0, lenght);
            strSend.CopyTo(strSendAll, 12);

            charBuf = stdConst.ToString().ToCharArray();
            lenght = 12;
            if (charToASCII.GetBytes(charBuf).Length <= 10)
            { lenght = charToASCII.GetBytes(charBuf).Length; }

            //byte[] s= charToASCII.GetBytes(charBuf);
            byte[] strSend2 = new byte[10];
            Array.Copy(charToASCII.GetBytes(charBuf), 0, strSend2, 0, lenght);
            strSend2.CopyTo(strSendAll, 14);

            if (SendData(stdCmd, strSendAll, out byte[] strRev))
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
        public bool CalibrationPowerAmplitude()
        {
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "1009", "" }
            };
            return SendData(0x13, strDictionary, out _);

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
        public bool CalibrationPowerAngle()
        {

            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "100A", "" }
            };
            return SendData(0x10, dic, out _);

        }
        /// <summary>
        /// 读取瞬时测量数据
        /// </summary>
        /// <param name="instValue"></param>
        /// <returns></returns>
        public bool ReadInstMetricAll(out float[] instValue)
        {
            instValue = new float[34];
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "1000", "" },
                { "1001", "" },
                { "1002", "" }
            };
            return SendData(0x10, dic, out byte[] strRev);


        }

        /// <summary>
        ///  100bH-模式设置
        /// </summary>
        /// <param name="stdCmd"></param>
        /// <returns></returns>
        public bool StdMode(byte stdCmd)
        {

            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100B", "" }
            };
            if (stdCmd == 0x10)
            {
                strDictionary.Add("1008", "");
            }

            return SendData(stdCmd, strDictionary, out _);

        }
        /// <summary>
        /// 100cH-启停标准表累积电能
        /// </summary>
        /// <param name="startOrStopStd">字符’1’表示清零当前电能并开始累计（ascii 码读取）</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool StartStdEnergy(int startOrStopStd)
        {
            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100C", startOrStopStd.ToString() }
            };
            return SendData(0x13, strDictionary, out _);

        }


        /// <summary>
        /// 读取瞬时测量数据
        /// </summary>
        /// <param name="instValue"></param>
        /// <returns></returns>
        public bool ReadstdZH311Param(out float[] instValue, out string errmsg)
        {
            instValue = new float[29];

            Dictionary<string, string> strDictionary = new Dictionary<string, string>
                {
                    { "1000", "" },
                    { "1001", "" },
                    { "1002", "" }
                };
            SendData(0x10, strDictionary, out byte[] strRev);

            byte[] byteDa = new byte[10];
            int intCount = 1;
            if (strRev.Length == 297)
            {
                for (int i = 0; i < instValue.Length; i++)
                {
                    if (i == 6 || i == 17) intCount += 1;
                    Array.Copy(strRev, 1 + 2 * intCount + i * 10, byteDa, 0, 10);
                    instValue[i] = Convert.ToSingle(Encoding.ASCII.GetString(byteDa));
                }
                errmsg = "";
                return true;



            }
            else errmsg = $"长度错误{strRev.Length}";

            return false;
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
        public bool SetPulseType(string pulseType)
        {

            Dictionary<string, string> strDictionary = new Dictionary<string, string>
            {
                { "100E", pulseType }
            };
            return SendData(0x13, strDictionary, out _);

        }

        /// <summary>
        /// 脉冲校准误差 字符串形式(10字节)W， 单位 分数值。
        /// 误差板的值 × 1000000 下发，譬如误差板计算的误差0.01525%，则0.01525% ×1000000 = 152.5，
        /// 则下发字符串 ”152.5”。
        /// </summary>
        ///  <param name="Error">误差板的值</param>
        /// <returns></returns>
        public bool SetPulseCalibration(string Error)
        {
            byte[] strSendAll = new byte[12];
            strSendAll[0] = 0x10;
            strSendAll[1] = 0x12;
            byte[] strSend2 = new byte[10];
            char[] charBuf = Error.ToCharArray();
            int lenght = 10;

            ASCIIEncoding charToASCII = new ASCIIEncoding();

            byte[] bytes = charToASCII.GetBytes(charBuf);
            if (bytes.Length < 10)
            { lenght = bytes.Length; }
            Array.Copy(bytes, 0, strSend2, 0, lenght);
            strSend2.CopyTo(strSendAll, 2);
            if (SendData(0x13, strSendAll, out _))
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        #region 指令


        private bool SendData(byte cmd, byte[] strDictionary, out byte[] RevbyteData)
        {
            RevbyteData = new byte[1];
            ZH311_RequestDataPacket rc3 = new ZH311_RequestDataPacket();
            rc3.SetPara(cmd, strDictionary);
            ZH311_RequestDataReplyPacket recv3 = new ZH311_RequestDataReplyPacket();
            if (SendPacketWithRetry(rc3, recv3))
            {
                RevbyteData = recv3.revbyteData;
                return true;
            }
            else
            {
                return false;
            }
        }


        ///// <summary>
        ///// 处理指令类 485端口
        ///// </summary>
        ///// <param name="cmd">命令码</param>
        ///// <param name="strDictionary">数据字典</param>
        ///// <param name="RevbyteData"></param>
        ///// <param name="FrameAry"></param>
        ///// <returns></returns>
        //private int SendData(int index, byte cmd, byte[] byteSend, out byte[] RevbyteData )
        //{
        //     RevbyteData = new byte[1];
        //    ZH311_RequestDataPacket rc3 = new ZH311_RequestDataPacket();
        //    ZH311_RequestDataReplyPacket recv3 = new ZH311_RequestDataReplyPacket();
        //    rc3.SetPara(cmd, byteSend);

        //    return 0;

        //}

        /// <summary>
        /// 处理指令类
        /// </summary>
        /// <param name="cmd">命令码</param>
        /// <param name="strDictionary">数据字典</param>
        /// <param name="RevbyteData"></param>
        /// <returns></returns>
        private bool SendData(byte cmd, Dictionary<string, string> strDictionary, out byte[] RevbyteData)
        {
            RevbyteData = new byte[0];
            ZH311_RequestDataPacket rc3 = new ZH311_RequestDataPacket();
            ZH311_RequestDataReplyPacket recv3 = new ZH311_RequestDataReplyPacket();
            rc3.SetPara(cmd, strDictionary);
            if (SendPacketWithRetry(rc3, recv3))
            {
                RevbyteData = recv3.revbyteData;
                return true;
            }
            else
            {
                return false;
            }

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
                            //recv.ParsePacket(ReFrameAry);
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


        ///// <summary>
        ///// 发送命令
        ///// </summary>
        ///// <param name="stPort">端口号</param>
        ///// <param name="sp">发送包</param>
        ///// <param name="rp">接收包</param>
        ///// <returns></returns>
        //private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        //{
        //    sp.IsNeedReturn = true;
        //    return driverBase.SendData(stPort, sp, rp);
        //    ;
        //}


        #endregion

    }


    #region ZH311表联机指令
    /// <summary>
    /// 表联机/脱机请求包
    /// </summary>
    internal class ZH311_RequestLinkPacket : ZH311SendPacket
    {
        public bool IsLink = true;

        public ZH311_RequestLinkPacket()
            : base()
        { }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);          //命令 
            buf.Put(0x10);
            buf.Put(0x00);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// ZH311表联机 返回指令
    /// </summary>
    internal class ZH311_RequestLinkReplyPacket : ZH311RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x90)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region 启停功能部分
    /// <summary>
    /// 标准表联机/脱机请求包
    /// </summary>
    internal class ZH3130_RequestStartStopacket : ZH311SendPacket
    {
        public byte[] byteData = new byte[0];
        public ZH3130_RequestStartStopacket()
            : base()
        { }

        public void SetPara(string strData)
        {
            byteData = Encoding.ASCII.GetBytes(strData);
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            //buf.Put(0xA0);          //命令 
            //buf.Put(0x02);
            buf.Put(byteData);
            buf.Put(0x0D);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// 标准表，联机返回指令
    /// </summary>
    internal class ZH3130_RequestStartStoReplyPacket : ZH311RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 8)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            return "返回：" + ReciveResult.ToString();
        }
    }
    #endregion


    #region 模式设置
    /// <summary>
    /// 标准表联机/脱机请求包
    /// </summary>
    internal class ZH3130_RequestSetModepacket : ZH311SendPacket
    {
        public byte[] _autoOrManuaDatal = new byte[0];
        public byte[] _connectionModeData = new byte[0];
        public byte[] _tableData = new byte[0];


        public ZH3130_RequestSetModepacket()
            : base()
        { }

        public void SetPara(string autoOrManuaDatal, string connectionModeData, string tableData)
        {
            _autoOrManuaDatal = Encoding.ASCII.GetBytes(autoOrManuaDatal);
            _connectionModeData = Encoding.ASCII.GetBytes(connectionModeData);
            _tableData = Encoding.ASCII.GetBytes(tableData);

        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            //buf.Put(0xA0);          //命令 
            //buf.Put(0x02);
            buf.Put(_autoOrManuaDatal);
            buf.Put(_connectionModeData);
            buf.Put(_tableData);
            buf.Put(0x0D);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// 标准表，联机返回指令
    /// </summary>
    internal class ZH3130_RequestSetModeReplyPacket : ZH311RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 8)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            return "返回：" + ReciveResult.ToString();
        }
    }
    #endregion
    #region ZH311表联机指令
    /// <summary>
    /// 表数据请求包
    /// </summary>
    internal class ZH311_RequestDataPacket : ZH311SendPacket
    {
        private byte _cmd = 0x10;


        byte[] byteData = new byte[0];
        public ZH311_RequestDataPacket()
            : base()
        { }

        public void SetPara(byte cmd, byte[] data)
        {
            _cmd = cmd;
            byteData = data;
        }

        public void SetPara(byte Cmd, Dictionary<string, string> dictionaryData)
        {
            _cmd = Cmd;

            int dataLenght = 0;
            int dataIndex = 0;
            foreach (var dic in dictionaryData)
            {
                string strRegn = dic.Key;
                if (strRegn.Length == 4)
                {
                    dataLenght += dic.Key.Length / 2;
                }
                else
                { continue; }
                if (_cmd == 0x10) continue;
                dataLenght += dic.Value.Length / 2;
            }

            byteData = new byte[dataLenght];



            foreach (var dic in dictionaryData)
            {


                string strRegn = dic.Key;
                if (strRegn.Length == 4)
                {

                    dic.Key.ToBytes().CopyTo(byteData, dataIndex);
                    dataIndex += 2;
                }
                else
                { continue; }
                if (_cmd == 0x10) continue;


                dic.Value.ToBytes().CopyTo(byteData, dataIndex);
                dataIndex += dic.Value.Length / 2;


            }





            //byte[] strSend2 = new byte[10];
            //ASCIIEncoding charToASCII = new ASCIIEncoding();
            //Array.Copy(charToASCII.GetBytes(dic.ToString().ToCharArray()), 0, strSend2, 0, 10);
            //strSend2.CopyTo(byteData, 12);


        }




        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(_cmd);          //命令 
            buf.Put(byteData);

            return buf.ToByteArray();
        }


    }
    /// <summary>
    /// ZH311表 数据 返回指令
    /// </summary>
    internal class ZH311_RequestDataReplyPacket : ZH311RecvPacket
    {

        public byte[] revbyteData = new byte[0];
        protected override void ParseBody(byte[] data)
        {
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x90)
                {
                    ReciveResult = RecvResult.OK;
                    revbyteData = data;
                }
                else
                {
                    ReciveResult = RecvResult.Unknow;
                }
            }
        }
        public override string GetPacketResolving()
        {
            return base.GetPacketResolving();
        }
    }
    #endregion

    #region ZH311源
    /// <summary>
    ///  源发送包基类
    /// </summary>
    internal class ZH311SendPacket : ZHSendPacket_CLT11
    {
        public ZH311SendPacket()
            : base()
        {
            ToID = 0x01;
            MyID = 0x01;
        }

        public ZH311SendPacket(bool needReplay)
            : base(needReplay)
        {
            ToID = 0x01;
            MyID = 0x01;
        }

        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// ZH311 源接收基类
    /// </summary>
    internal class ZH311RecvPacket : ZHRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
