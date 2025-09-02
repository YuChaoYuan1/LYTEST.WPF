using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Enum;
using ZH.Struct;

namespace E_ZH3001
{
    #region ZH3001功率源
    //读源测量值
    #region ZH3001读取标准表交流幅度值
    /// <summary>
    /// 读取标准表交流幅度值 请求包
    /// </summary>
    internal class ZH3001_RequestReadAmplitudePacket : ZH3001SendPacket
    {
        public ZH3001_RequestReadAmplitudePacket()
            : base()
        { }

        /*
         * 68 01 01 08 10 10 00 08 。
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);          //命令 
            buf.Put(0x10);
            buf.Put(0x00);

            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "读取标准表交流幅度值。";
        }
    }

    /// <summary>
    /// 读取标准表交流幅度值返回包
    /// </summary>
    internal class ZH3001_RequestReadAmplitudeReplayPacket : ZH3001RecvPacket
    {
        public ZH3001_RequestReadAmplitudeReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "ZH3001_RequestReadAmplitudeReplayPacket";
        }
        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>
        public stStdInfo PowerInfo { get; private set; }

        public string DxString(String Str)
        {
            string Str_tmp = "";
            for (int i = 0; i < Str.Length / 2; i++)
            {
                Str_tmp = Str.Substring(i * 2, 2) + Str_tmp;
            }
            return Str_tmp;
        }

        protected override void ParseBody(byte[] data)
        {
            stStdInfo tagInfo = new stStdInfo();
            if (data.Length != 0x21) return;
            string strTemp = "";
            double lgTemp;
            int intA = 0;
            for (int i = data.Length - 1; i > 2; i--)
            {
                strTemp += data[i].ToString("X2");
            }
            if (strTemp.Substring(0, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(0, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(2, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Ic = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(10, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(10, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(12, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Uc = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(20, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(20, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(22, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Ib = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(30, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(30, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(32, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Ub = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(40, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(40, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(42, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Ia = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(50, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(50, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(52, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Ua = float.Parse(lgTemp.ToString());
            PowerInfo = tagInfo;
        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("返回：{0}V,{1}V,{2}V,{3}A,{4}A,{5}A", PowerInfo.Ua, PowerInfo.Ub, PowerInfo.Uc, PowerInfo.Ia, PowerInfo.Ib, PowerInfo.Ic);
            return strResolve;
        }

        //private sbyte GetByteFromByteArray(byte data)
        //{
        //    string Fmt16 = Convert.ToString(data, 16);
        //    return (Convert.ToSByte(Fmt16, 16));
        //}
    }
    #endregion ZH3001读取标准表交流幅度值 

    #region ZH3001读取标准表交流相频值 
    /// <summary>
    /// 读取标准表交流相频值 请求包
    /// </summary>
    internal class ZH3001_RequestReadAnglePacket : ZH3001SendPacket
    {
        public ZH3001_RequestReadAnglePacket()
            : base()
        { }

        /*
         * 68 01 01 08 10 10 01 08 。
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);          //命令 
            buf.Put(0x10);
            buf.Put(0x01);

            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "读取标准表交流相频值。";
        }
    }

    /// <summary>
    /// 读取标准表交流相频值返回包
    /// </summary>
    internal class ZH3001_RequestReadAngleReplayPacket : ZH3001RecvPacket
    {
        public ZH3001_RequestReadAngleReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "ZH3001_RequestReadAmplitudeReplayPacket";
        }
        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>
        public stStdInfo PowerInfo { get; private set; }

        public string DxString(String Str)
        {
            string Str_tmp = "";
            for (int i = 0; i < Str.Length / 2; i++)
            {
                Str_tmp = Str.Substring(i * 2, 2) + Str_tmp;
            }
            return Str_tmp;
        }

        protected override void ParseBody(byte[] data)
        {
            stStdInfo tagInfo = new stStdInfo();
            if (data.Length != 0x35) return;
            string strTemp = "";
            double lgTemp;
            int intA = 0;
            for (int i = data.Length - 16; i > 2; i--)
            {
                strTemp += data[i].ToString("X2");
            }
            if (strTemp.Substring(0, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(0, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(2, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Freq = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(10, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(10, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(12, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Phi_Ic = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(20, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(20, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(22, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Phi_Uc = float.Parse(lgTemp.ToString());
            tagInfo.PowerFactor_C = (float)Math.Cos(Math.Abs(lgTemp - tagInfo.Phi_Ic) * Math.PI / 180);

            if (strTemp.Substring(30, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(30, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(32, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Phi_Ib = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(40, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(40, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(42, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Phi_Ub = float.Parse(lgTemp.ToString());
            tagInfo.PowerFactor_B = (float)Math.Cos(Math.Abs(lgTemp - tagInfo.Phi_Ib) * Math.PI / 180);

            if (strTemp.Substring(50, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(50, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(52, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Phi_Ia = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(60, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(60, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(62, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Phi_Ua = float.Parse(lgTemp.ToString());
            tagInfo.PowerFactor_A = (float)Math.Cos(Math.Abs(lgTemp - tagInfo.Phi_Ia) * Math.PI / 180);
            PowerInfo = tagInfo;
        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("返回：{0}V,{1}V,{2}V,{3}A,{4}A,{5}A,{6},{7},{8},{9},{10},{11},{12}W,{13}Var,{14}VA", PowerInfo.Ua, PowerInfo.Ub, PowerInfo.Uc, PowerInfo.Ia, PowerInfo.Ib, PowerInfo.Ic, PowerInfo.Phi_Ua, PowerInfo.Phi_Ub, PowerInfo.Phi_Uc, PowerInfo.Phi_Ia, PowerInfo.Phi_Ib, PowerInfo.Phi_Ic, PowerInfo.P, PowerInfo.Q, PowerInfo.S);
            return strResolve;
        }

        //private sbyte GetByteFromByteArray(byte data)
        //{
        //    string Fmt16 = Convert.ToString(data, 16);
        //    return (Convert.ToSByte(Fmt16, 16));
        //}
    }
    #endregion ZH3001读取标准表交流相频值 

    #region ZH3001读取标准表交流功率值 
    /// <summary>
    /// 读取标准表交流功率 请求包
    /// </summary>
    internal class ZH3001_RequestReadPowerPacket : ZH3001SendPacket
    {
        public ZH3001_RequestReadPowerPacket()
            : base()
        { }

        /*
         * 68 01 01 08 10 10 01 08 。
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);          //命令 
            buf.Put(0x10);
            buf.Put(0x02);

            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "读取标准表交流相频值。";
        }
    }

    /// <summary>
    /// 读取标准表交流功率值返回包
    /// </summary>
    internal class ZH3001_RequestReadPowerReplayPacket : ZH3001RecvPacket
    {
        public ZH3001_RequestReadPowerReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "ZH3001_RequestReadPowerReplayPacket";
        }
        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>
        public stStdInfo PowerInfo { get; private set; }

        public string DxString(String Str)
        {
            string Str_tmp = "";
            for (int i = 0; i < Str.Length / 2; i++)
            {
                Str_tmp = Str.Substring(i * 2, 2) + Str_tmp;
            }
            return Str_tmp;
        }

        protected override void ParseBody(byte[] data)
        {
            stStdInfo tagInfo = new stStdInfo();
            if (data.Length != 0x30) return;
            string strTemp = "";
            double lgTemp;
            int intA = 0;
            for (int i = data.Length - 1; i > 2; i--)
            {
                strTemp += data[i].ToString("X2");
            }
            if (strTemp.Substring(0, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(0, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(2, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.S = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(10, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(10, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(12, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Q = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(20, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(20, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(22, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.P = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(30, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(30, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(32, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Qc = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(40, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(40, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(42, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Pc = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(50, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(50, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(52, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Qb = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(60, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(60, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(62, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Pb = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(70, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(70, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(72, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Qa = float.Parse(lgTemp.ToString());

            if (strTemp.Substring(80, 2) != "00") intA = 256 - Convert.ToInt32(strTemp.Substring(80, 2), 16);
            lgTemp = double.Parse(Convert.ToInt64(DxString(strTemp.Substring(82, 8)), 16).ToString()) / Math.Pow(10, intA);
            tagInfo.Pa = float.Parse(lgTemp.ToString());

            PowerInfo = tagInfo;
        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("返回：{0}V,{1}V,{2}V,{3}A,{4}A,{5}A,{6},{7},{8},{9},{10},{11},{12}W,{13}Var,{14}VA", PowerInfo.Ua, PowerInfo.Ub, PowerInfo.Uc, PowerInfo.Ia, PowerInfo.Ib, PowerInfo.Ic, PowerInfo.Phi_Ua, PowerInfo.Phi_Ub, PowerInfo.Phi_Uc, PowerInfo.Phi_Ia, PowerInfo.Phi_Ib, PowerInfo.Phi_Ic, PowerInfo.P, PowerInfo.Q, PowerInfo.S);
            return strResolve;
        }

        //private sbyte GetByteFromByteArray(byte data)
        //{
        //    string Fmt16 = Convert.ToString(data, 16);
        //    return (Convert.ToSByte(Fmt16, 16));
        //}
    }
    #endregion




    #region ZH3001源联机指令
    /// <summary>
    /// 源联机/脱机请求包
    /// </summary>
    internal class ZH3001_RequestLinkPacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        public ZH3001_RequestLinkPacket()
            : base()
        { }

        /*
        // 68 01 01 09 15 A0 00 01  BD //PC联机
        // 68 01 01 09 15 A0 00 02  BE //PC脱机
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x15);          //命令 
            buf.Put(0xA0);
            buf.Put(0x00);
            buf.Put(0x01);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 源联机 返回指令
    /// </summary>
    internal class ZH3001_RequestLinkReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH3001设置电压  电流幅值 


    /// <summary>
    /// 设置电压，电流幅值
    /// </summary>
    internal class ZH3001_RequestPowerAmplitudePacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流

        private UIPara _UIpara;

        #endregion

        public ZH3001_RequestPowerAmplitudePacket()
            : base()
        {
            _UIpara = new UIPara();
        }

        public void SetPara(double Ua, double Ub, double Uc, double Ia, double Ib, double Ic)
        {
            _UIpara.Ua = Ua; _UIpara.Ub = Ub; _UIpara.Uc = Uc;
            _UIpara.Ia = Ia; _UIpara.Ib = Ib; _UIpara.Ic = Ic;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x20);
            buf.Put(0x00);

            buf.Put(GetBytesDot4(_UIpara.Ua));
            buf.Put(GetBytesDot4(_UIpara.Ia));
            buf.Put(GetBytesDot4(_UIpara.Ub));
            buf.Put(GetBytesDot4(_UIpara.Ib));
            buf.Put(GetBytesDot4(_UIpara.Uc));
            buf.Put(GetBytesDot4(_UIpara.Ic));

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("升源：{0},{1},{2},{3},{4},{5}", _UIpara.Ua, _UIpara.Ub, _UIpara.Uc, _UIpara.Ia, _UIpara.Ib, _UIpara.Ic);
            return strResolve;
        }
    }

    internal class ZH3001_RequestPowerAmplitudeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }


    #endregion ZH3001设置电压电流幅值

    #region ZH3001设置相频 

    #region 发送ZH3001设置相频
    internal class ZH3001_RequestPowerAnglePacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流

        private PhiPara _UIpara;
        private double _Hz;
        #endregion

        public ZH3001_RequestPowerAnglePacket()
            : base()
        {
            _UIpara = new PhiPara();
        }

        public double DouA(double douA)
        {
            double douB = douA;
            if (douB < 0) douB += 360;
            if (douB > 360) douB -= 360;
            return douB;
        }
        public void SetPara(double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq)
        {
            _UIpara.PhiUa = DouA(PhiUa);
            _UIpara.PhiUb = DouA(PhiUb);
            _UIpara.PhiUc = DouA(PhiUc);
            _UIpara.PhiIa = DouA(PhiIa);
            _UIpara.PhiIb = DouA(PhiIb);
            _UIpara.PhiIc = DouA(PhiIc);
            _Hz = Freq;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x20);
            buf.Put(0x01);

            buf.Put(GetBytesDot4(_UIpara.PhiUa));
            buf.Put(GetBytesDot4(_UIpara.PhiIa));
            buf.Put(GetBytesDot4(_UIpara.PhiUb));
            buf.Put(GetBytesDot4(_UIpara.PhiIb));
            buf.Put(GetBytesDot4(_UIpara.PhiUc));
            buf.Put(GetBytesDot4(_UIpara.PhiIc));
            buf.Put(GetBytesDot4(_Hz));

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("升源：{0},{1},{2},{3},{4},{5},{6}", _UIpara.PhiUa, _UIpara.PhiUb, _UIpara.PhiUc, _UIpara.PhiIa, _UIpara.PhiIb, _UIpara.PhiIc, _Hz);
            return strResolve;
        }
    }


    #endregion 发送ZH3001设置相频

    #region 返回设置相频
    internal class ZH3001_RequestPowerAngleReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }

    }

    #endregion 返回设置相频
    #endregion ZH3001设置相频

    #region ZH3001设置200BH --- 升源参数设定(本指令不需要8000H系列的指令启动升源)
    internal class ZH3001_RequestPowerOnPacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流相位 频率

        int _jxfs;
        //68 01 01 09 15 80 01 01  9C //三相四线方式
        //68 01 01 09 15 80 01 02  9F //三相三线方式

        private UIPara _UIpara1;
        private PhiPara _UIpara;
        private double _Hz;

        int _Mode;
        //68 01 01 09 15 80 02 01  9F  //启动功率源
        //68 01 01 09 15 80 02 00 9E  //关闭功率源
        #endregion

        public ZH3001_RequestPowerOnPacket()
            : base()
        {
            _UIpara = new PhiPara();
        }

        public double DouA(double douA)
        {
            double douB = douA;
            if (douB < 0) douB += 360;
            if (douB > 360) douB -= 360;
            return douB;
        }

        public void SetPara(int jxfs, double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq, int Mode)
        {
            _jxfs = jxfs;
            _UIpara1.Ua = Ua;
            _UIpara1.Ub = Ub;
            _UIpara1.Uc = Uc;
            _UIpara1.Ia = Ia;
            _UIpara1.Ib = Ib;
            _UIpara1.Ic = Ic;
            _UIpara.PhiUa = DouA(PhiUa);
            _UIpara.PhiUb = DouA(PhiUb);
            _UIpara.PhiUc = DouA(PhiUc);
            _UIpara.PhiIa = DouA(PhiIa);
            _UIpara.PhiIb = DouA(PhiIb);
            _UIpara.PhiIc = DouA(PhiIc);
            _Hz = Freq;
            _Mode = Mode;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x20);
            buf.Put(0x0B);
            //string[] Ubb, Ucc, Iaa, Ibb, Icc, PUaa, PUbb, PUcc, PIaa, PIbb, PIcc, Freqq;

            //jxfs
            //if (_jxfs == 00 || _jxfs == 01 || _jxfs == 05)
            if (_jxfs == 00 || _jxfs == 05)
            {
                buf.Put(0x01);  // 三相四线
            }
            else
            {
                buf.Put(0x02); // 三相三线
            }


            buf.Put(GetBytesDot4(_UIpara1.Ua));
            buf.Put(GetBytesDot4(_UIpara.PhiUa));
            buf.Put(GetBytesDot4(_UIpara1.Ub));
            buf.Put(GetBytesDot4(_UIpara.PhiUb));
            buf.Put(GetBytesDot4(_UIpara1.Uc));
            buf.Put(GetBytesDot4(_UIpara.PhiUc));
            buf.Put(GetBytesDot4(_UIpara1.Ia));
            buf.Put(GetBytesDot4(_UIpara.PhiIa));
            buf.Put(GetBytesDot4(_UIpara1.Ib));
            buf.Put(GetBytesDot4(_UIpara.PhiIb));
            buf.Put(GetBytesDot4(_UIpara1.Ic));
            buf.Put(GetBytesDot4(_UIpara.PhiIc));
            buf.Put(GetBytesDot4(_Hz));

            //Mode
            buf.Put(Convert.ToByte(_Mode));

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("升源：{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", _jxfs, _UIpara1.Ua, _UIpara.PhiUa, _UIpara1.Ub, _UIpara.PhiUb, _UIpara1.Uc, _UIpara.PhiUc, _UIpara1.Ia, _UIpara.PhiIa, _UIpara1.Ib, _UIpara.PhiIb, _UIpara1.Ic, _UIpara.PhiIc, _Hz, _Mode);
            return strResolve;
        }
    }

    internal class ZH3001_RequestPowerOnReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion 发送ZH3001设置200BH --- 升源参数设定(本指令不需要8000H系列的指令启动升源)

    #region ZH3001设置2100H----设置负载电流快速变化实验的时间与启动标志
    internal class ZH3001_RequestLoadCurrentPacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 时间

        private double _Time1;
        private double _Time2;

        int _Mode;

        #endregion

        //public ZH3001_RequestLoadCurrentPacket()
        //    : base()
        //{

        //}

        public void SetPara(double Time1, double Time2, int Mode)
        {
            _Time1 = Time1;
            _Time2 = Time2;
            _Mode = Mode;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x21);
            buf.Put(0x00);

            string str_Temp, str_Temp2;
            string[] time11, time22;

            //time1
            str_Temp = _Time1.ToString();
            if (str_Temp.Length > 6) str_Temp = str_Temp.Substring(0, 6).TrimEnd('0');
            time11 = str_Temp.Split('.');
            if (time11.Length > 1)
            {
                str_Temp = str_Temp.Replace(".", "");
                str_Temp = int.Parse(str_Temp).ToString("X8") + Convert.ToString(256 - time11[1].Length, 16);
            }
            else
            {
                str_Temp = int.Parse(str_Temp).ToString("X8") + "00";
            }
            for (int i = 0; i < 4; i++)
            {
                str_Temp2 = str_Temp.Substring(i * 2, 2);
                buf.Put(Convert.ToByte(str_Temp2, 16));
            }

            //Time2
            str_Temp = _Time2.ToString();
            if (str_Temp.Length > 6) str_Temp = str_Temp.Substring(0, 6).TrimEnd('0');
            time22 = str_Temp.Split('.');
            if (time22.Length > 1)
            {
                str_Temp = str_Temp.Replace(".", "");
                str_Temp = int.Parse(str_Temp).ToString("X8") + Convert.ToString(256 - time22[1].Length, 16);
            }
            else
            {
                str_Temp = int.Parse(str_Temp).ToString("X8") + "00";
            }
            for (int i = 0; i < 4; i++)
            {
                str_Temp2 = str_Temp.Substring(i * 2, 2);
                buf.Put(Convert.ToByte(str_Temp2, 16));
            }

            //Mode

            buf.Put(Convert.ToByte(_Mode));


            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("升源：{0},{1},{2},", _Time1, _Time2, _Mode);
            return strResolve;
        }
    }

    internal class ZH3001_RequestLoadCurrentReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion 发送ZH3001设置2100H----设置负载电流快速变化实验的时间与启动标志

    #region ZH3001设置2100H----负载电流快速变化实验的时间与启动标志 新 旧软件拷贝
    /// <summary>
    /// 开始或停止走字指令请求包
    /// </summary>
    internal class ZH3001_RequestSetCurrentChangePacket : ZH3001SendPacket
    {
        public int _TonTime = 0;

        public int _ToffTime = 0;

        public byte _StartType = 0x00;


        public ZH3001_RequestSetCurrentChangePacket()
            : base()
        { }

        public void SetPara(int TonTime, int ToffTime, string StartType)
        {
            _TonTime = TonTime;
            _ToffTime = ToffTime;
            _StartType = Convert.ToByte(Convert.ToInt32(StartType.PadLeft(8, '0'), 2));
        }
        //2100H----设置负载电流快速变化实验的时间与启动标志//2021.04.09
        //输出时间Ton：4个字节表示，单位0.01s。
        //输出时间Ton：4个字节表示，单位0.01s。
        //启动标志：1个字节表示，bit0-A相，bit1-B相，bit2-C相，其他bit位无效。
        //如：68 01 01 11 13 21 00 00 00 13 88 00 00 27 10 07 88
        //返回：68 01 01 09 93 21 00 4B F0
        //设置步骤：
        //PC端具体操作流程如下：
        //步骤1：设置负载电流快速变化实验的时间与启动标志
        //步骤2：设置电压电流幅值相位频率。
        //步骤3：升源
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x21);
            buf.Put(0x00);
            buf.PutInt(_TonTime);
            buf.PutInt(_ToffTime);
            buf.Put(Convert.ToByte(_StartType));

            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 设置负载电流快速变化实验的时间与启动标志
    /// </summary>
    internal class ZH3001_RequestSetCurrentChangeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion ZH3001源开始或停止走字指令NEW

    #region ZH3001设置2101H----交流电压骤降和中断实验的时间与启动标志 
    /// <summary>
    /// 开始或停止走字指令请求包
    /// </summary>
    internal class ZH3001_RequestAC_VoltageCurrentPacket : ZH3001SendPacket
    {
        public int _TonTime = 0;

        public int _ToffTime = 0;

        public int _count = 0;
        public byte _proportion = 0;

        public byte _StartType = 0x00;


        public ZH3001_RequestAC_VoltageCurrentPacket()
            : base()
        { }

        public void SetPara(int TonTime, int ToffTime, int count, int proportion, string StartType)
        {
            _TonTime = TonTime;
            _ToffTime = ToffTime;
            _count = count;
            _proportion = Convert.ToByte(proportion);
            _StartType = Convert.ToByte(Convert.ToInt32(StartType.PadLeft(8, '0'), 2));
        }
        //2101H----设置交流电压暂降与短时中断试验参数与启动标志//2023.02.03
        //电压保持正常输出时间：4个字节表示，单位0.01s。
        //电压中断或暂降持续时间：4个字节表示，单位0.01s。
        //试验总次数：4个字节表示，单位次, 设置为0试验不会启动。
        //电压下降比率：1个字节表示，0-100，（中断设置100，暂降1-99），表示为电压降低设定值百分比输出。
        //启动试验标志：1个字节表示，bit0-A相，bit1-B相，bit2-C相，其他bit位无效。
        //如：68 01 01 16 13 21 01 00 00 01 F4 00 00 03 E8 00 00 00 0A 14 07 04
        //返回：68 01 01 09 93 21 01 4B F1
        //设置步骤：
        //PC端具体操作流程如下：
        //步骤1：设置负载电流快速变化实验的时间与启动标志
        //步骤2：设置电压电流幅值相位频率。
        //步骤3：升源
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x21);
            buf.Put(0x01);
            buf.PutInt(_TonTime);
            buf.PutInt(_ToffTime);
            buf.PutInt(_count);
            buf.Put(Convert.ToByte(_proportion));
            buf.Put(Convert.ToByte(_StartType));

            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 设置负载电流快速变化实验的时间与启动标志
    /// </summary>
    internal class ZH3001_RequestAC_VoltageCurrentReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            //if (data == null || data.Length != 4)
            //    ReciveResult = RecvResult.DataError;
            //else
            //{
            //    if (data[0] == 0x93)
            //        ReciveResult = RecvResult.OK;
            //    else
            //        ReciveResult = RecvResult.Unknow;
            //}

            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[data.Length - 1] == 0x4B)
                    ReciveResult = RecvResult.OK;
                else if (data[data.Length - 1] == 0x45)
                    ReciveResult = RecvResult.FrameError;
                else if (data[data.Length - 1] == 0x88)
                    ReciveResult = RecvResult.Locking485;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion ZH3001源开始或停止走字指令NEW

    #region ZH3001设置接线模式 
    /// <summary>
    /// 设置接线模式 请求包
    /// </summary>
    internal class ZH3001_RequestPowerModePacket : ZH3001SendPacket
    {

        private int _Mode;
        //68 01 01 09 15 80 01 01  9C //三相四线方式
        //68 01 01 09 15 80 01 02  9F //三相三线方式
        public void SetPara(int Mode)
        {
            _Mode = Mode;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x15);          //命令 
            buf.Put(0x80);
            buf.Put(0x01);
            if (_Mode == 00 || _Mode == 01)
            {
                buf.Put(0x01);
            }
            else
            {
                buf.Put(0x02);
            }

            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "设置接线模式。";
        }
    }
    /// <summary>
    /// 设置接线模式 返回指令
    /// </summary>
    internal class ZH3001_RequestPowerModeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion 设置接线模式 

    #region ZH3001更新全部状态寄存器_源更新 
    /// <summary>
    /// 更新全部状态寄存器 请求包 源更新
    /// </summary>
    internal class ZH3001_RequestPowerUpdatePacket : ZH3001SendPacket
    {

        private byte[] _Mode = new byte[] { 0x8F, 0xFF };

        //68 01 01 0A 13 20 0A 8F FF 43
        //Bit0：A相电压幅度 Bit1：A相电流幅度 Bit2：B相电压幅度 Bit3：B相电流幅度
        //Bit4：C相电压幅度 Bit5：C相电流幅度 Bit6：A相电压相位 Bit7：A相电流相位
        //Bit8：B相电压相位 Bit9：B相电流相位 Bit10：C相电压相位 Bit11：C相电流相位
        //Bit15：频率更新

        public void SetPara(byte[] Mode)
        {
            _Mode = Mode;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x20);
            buf.Put(0x0A);
            buf.Put(_Mode[0]);
            buf.Put(_Mode[1]);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "更新全部状态寄存器。";
        }
    }
    /// <summary>
    /// 更新全部状态寄存器 返回指令
    /// </summary>
    internal class ZH3001_RequestPowerUpdateReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001更新全部状态寄存器   

    #region ZH3001启动或者关闭交流源
    /// <summary>
    /// 开源 请求包
    /// </summary>
    internal class ZH3001_RequestPowerOn_OffPacket : ZH3001SendPacket
    {
        byte _Mode = 0x00;
        public void SetPara(byte Mode)
        {
            _Mode = Mode;
        }
        //68 01 01 09 15 80 02 01  9F  //启动功率源
        //68 01 01 09 15 80 02 00 9E  //关闭功率源

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x15);          //命令 
            buf.Put(0x80);
            buf.Put(0x02);
            buf.Put(_Mode);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "开启功率源。";
        }
    }
    /// <summary>
    /// 开源 返回指令
    /// </summary>
    internal class ZH3001_RequestPowerOn_OffReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001开源指令NEW  

    #region ZH3001设置数字闭环状态
    /// <summary>
    /// 设置数字闭环状态 请求包
    /// </summary>
    internal class ZH3001_RequestSetPowerLoopTypePacket : ZH3001SendPacket
    {
        public byte _powerType = 0x80;

        //8007H---设置数字闭环状态 闭环状态：一个字节表示，0x80表示禁止闭环，0x00表示允许闭环
        //68 01 01 09 15 80 07 80 1B  // 

        public void SetPara(byte PowerType)
        {
            _powerType = PowerType;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x15);          //命令 
            buf.Put(0x80);
            buf.Put(0x07);
            buf.Put(_powerType);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "设置数字闭环状态。";
        }
    }
    /// <summary>
    /// 开源 返回指令
    /// </summary>
    internal class ZH3001_RequestSetPowerLoopTypeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001开源指令NEW

    #region ZH3001手动挡与自动挡的切换
    /// <summary>
    /// 手动挡与自动挡的切换 请求包
    /// </summary>
    internal class ZH3001_RequestSetPowerGearPacket : ZH3001SendPacket
    {
        protected byte _powerGear = 0x00;
        //8004H---手动挡与自动挡的切换必须先将源输出，再切换至手动挡。若需换挡，
        //则先将手动挡标志清除，然后再下发幅值升源。
        //一个字节表示。
        //Bit7:电压手动挡
        //Bit6:电流手动挡
        //对应位置0则为自动挡
        //68 01 01 09 15 80 04 C0 58 // 

        public void SetPara(string Ugear, string Igear)
        {
            string strGear = Ugear + Igear + "000000";
            _powerGear = Byte.Parse(string.Format("{0:X}", System.Convert.ToInt32(strGear, 2)));

        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x15);          //命令 
            buf.Put(0x80);
            buf.Put(0x04);
            buf.Put(_powerGear);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "手动挡与自动挡的切换。";
        }
    }
    /// <summary>
    /// 开源 返回指令
    /// </summary>
    internal class ZH3001_RequestSetPowerGearReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001开源指令NEW


    #region ZH3001设置常规谐波

    #region 发送ZH3001设置常规谐波
    internal class ZH3001_RequestPowerGetHarmonicPacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流

        // private UIParam _UIpara;
        private byte _HarmonicType = 0x3F;
        private int _HarmonicOpenOrClose = 0;
        private float[] _HarmonicContent = new float[60];
        private float[] _HarmonicPhase = new float[60];
        #endregion

        public ZH3001_RequestPowerGetHarmonicPacket()
            : base()
        {

            // _UIpara = new UIParam();
        }
        /// <summary>
        /// 设置谐波参数
        /// </summary>
        /// <param name="HarmonicContent">谐波含量 2---61</param>
        /// <param name="HarmonicPhase">谐波相位 2---61</param>
        /// <param name="HarmonicType">谐波类型 Bit0-Ua、Bit1-Ub、Bit2-Uc、Bit3-Ia、Bit4-Ib、bit5-Ic，所有相别按同一谐波数据设置则将Bit0-Bit5都置为1</param>
        /// <param name="HarmonicOpenOrClose">00关闭，01打开</param>
        public void SetPara(float[] HarmonicContent, float[] HarmonicPhase, byte HarmonicType, int HarmonicOpenOrClose)
        {


            _HarmonicType = HarmonicType;
            _HarmonicOpenOrClose = HarmonicOpenOrClose;
            _HarmonicContent = HarmonicContent;
            _HarmonicPhase = HarmonicPhase;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x20);
            buf.Put(0x08);
            buf.Put(_HarmonicType);
            string str_Temp, str_Temp2;
            //str_Temp = _UIpara.Ua.ToString("0");
            //str_Temp = "0";
            //_ = int.Parse(str_Temp).ToString("X8") + "FC";
            for (int i = 0; i < _HarmonicContent.Length; i++)
            {
                int intHarmonicContent = Convert.ToInt32(_HarmonicContent[i] * 100);
                int intHarmonicPhase = Convert.ToInt32(_HarmonicPhase[i] * 100);
                str_Temp = intHarmonicContent.ToString("X8") + "FC";
                for (int k = 0; k < 5; k++)
                {
                    str_Temp2 = str_Temp.Substring(k * 2, 2);
                    buf.Put(Convert.ToByte(str_Temp2, 16));
                }

                str_Temp = intHarmonicPhase.ToString("X8") + "FE";
                for (int k = 0; k < 5; k++)
                {
                    str_Temp2 = str_Temp.Substring(k * 2, 2);
                    buf.Put(Convert.ToByte(str_Temp2, 16));
                }
            }
            buf.Put(Convert.ToByte(_HarmonicOpenOrClose));
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "";// string.Format("升源：{0},{1},{2},{3},{4},{5}", _UIpara.Ua, _UIpara.Ub, _UIpara.Uc, _UIpara.Ia, _UIpara.Ib, _UIpara.Ic);
            return strResolve;
        }
        /// <summary>
        /// 组帧
        /// </summary>
        /// <returns>完整的数据包内容</returns>
        public override byte[] GetPacketData()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(PacketHead);        //帧头
            buf.Put(ToID);              //发信节点
            buf.Put(MyID);              //受信节点
            byte[] body = GetBody();
            if (body == null)
                return null;
            int packetLength = (int)(body.Length + 6);//帧头4字节+CS一字节
            buf.PutInt2(packetLength);      //帧长度
            buf.Put(body);              //数据域 
            byte chkSum = GetChkSum(buf.ToByteArray());
            buf.Put(chkSum);
            return buf.ToByteArray();
        }
    }
    #endregion 发送设置常规谐波

    #region 返回设置常规谐波

    internal class ZH3001_RequestGetHarmonicReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }

    }

    #endregion 返回设置常规谐波

    #endregion ZH3001设置常规谐波

    #region ZH3001设置特殊谐波
    /// <summary>
    /// 开源 请求包
    /// </summary>
    internal class ZH3001_RequestPowerHarmonicPacket : ZH3001SendPacket
    {


        //        谐波设置对象：一个字节表示，Bit0-Ua、Bit1-Ub、Bit2-Uc、Bit3-Ia、Bit4-Ib、bit5-Ic，对应Bit位置1表示更新该相特殊谐波。
        //特殊谐波：一个字节表示，0表示正常谐波；1表示方形波，2表示尖顶波，3表示次谐波，4表示奇次谐波，5表示偶次谐波。
        //数据排列如: 谐波设置对象+特殊谐波标志。
        //指令如： 68 01 01 0A 13 20 09 09 01 38
        //返回：   68 01 01 09 93 20 09 4B F8

        public byte _byteData = 0x3f;
        public int _HarmonicType = 1;
        public void SetPara(byte byteData, int HarmonicType)
        {
            _byteData = byteData;
            _HarmonicType = HarmonicType;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x20);
            buf.Put(0x09);
            buf.Put(_byteData);
            buf.Put(Convert.ToByte(_HarmonicType));
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "设置特殊谐波。";
        }
    }
    /// <summary>
    /// 开源 返回指令
    /// </summary>
    internal class ZH3001_RequestPowerHarmonicReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001NEW
    //手动控制交流源
    #region 校准源    
    #region ZH3001  	交流源校准 进入模式
    /// <summary>
    /// 交流源校准 进入模式 请求包
    /// </summary>
    internal class ZH3001_RequestCalibrationPowerPacket : ZH3001SendPacket
    {
        protected byte _calibrationPower = 0x81;
        //5000H---一个字节表示，0x81表示缺省，0x80表示进入校准模式，0x00退出校准模

        //68 01 01 09 13 50 00 81 4B // 

        public void SetPara(byte CalibrationPower)
        {
            _calibrationPower = CalibrationPower;


        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x50);
            buf.Put(0x00);
            buf.Put(_calibrationPower);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "交流源校准 进入模式。";
        }
    }
    /// <summary>
    /// 交流源校准 进入模式 返回指令
    /// </summary>
    internal class ZH3001_RequestCalibrationPowerReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001开源指令NEW

    #region ZH3001  	启动校准  
    /// <summary>
    /// 启动校准 请求包
    /// </summary>
    internal class ZH3001_RequestStartCalibrationPowerPacket : ZH3001SendPacket
    {
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x50);
            buf.Put(0x00);
            buf.Put(0x01);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "启动校准。";
        }
    }
    /// <summary>
    /// 启动校准 返回指令
    /// </summary>
    internal class ZH3001_RequestStartCalibrationPowerReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion ZH3001开源指令NEW

    #region ZH3001校准标准幅值下发


    /// <summary>
    /// 校准标准幅值下发
    /// </summary>
    internal class ZH3001_RequestCalibrationPowerAmplitudePacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流

        private UIPara _UIpara;

        #endregion

        public ZH3001_RequestCalibrationPowerAmplitudePacket()
            : base()
        {
            _UIpara = new UIPara();
        }

        public void SetPara(double Ua, double Ub, double Uc, double Ia, double Ib, double Ic)
        {
            _UIpara.Ua = Ua; _UIpara.Ub = Ub; _UIpara.Uc = Uc;
            _UIpara.Ia = Ia; _UIpara.Ib = Ib; _UIpara.Ic = Ic;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x50);
            buf.Put(0x0A);

            buf.Put(GetBytesDot4(_UIpara.Ua));
            buf.Put(GetBytesDot4(_UIpara.Ia));
            buf.Put(GetBytesDot4(_UIpara.Ub));
            buf.Put(GetBytesDot4(_UIpara.Ib));
            buf.Put(GetBytesDot4(_UIpara.Uc));
            buf.Put(GetBytesDot4(_UIpara.Ic));

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("校准电压电流源：{0},{1},{2},{3},{4},{5}", _UIpara.Ua, _UIpara.Ub, _UIpara.Uc, _UIpara.Ia, _UIpara.Ib, _UIpara.Ic);
            return strResolve;
        }
    }




    internal class ZH3001_RequestCalibrationPowerAmplitudeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }

    }


    #endregion ZH3001设置电压电流幅值

    #region ZH3001 校准相位 


    internal class ZH3001_RequestCalibrationPowerAnglePacket : ZH3001SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流

        private PhiPara _UIpara;


        #endregion

        public ZH3001_RequestCalibrationPowerAnglePacket()
            : base()
        {
            _UIpara = new PhiPara();
        }

        public double DouA(double douA)
        {
            double douB = douA;
            if (douB < 0) douB += 360;
            if (douB > 360) douB -= 360;
            return douB;
        }
        public void SetPara(double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc)
        {
            _UIpara.PhiUa = DouA(PhiUa);
            _UIpara.PhiUb = DouA(PhiUb);
            _UIpara.PhiUc = DouA(PhiUc);
            _UIpara.PhiIa = DouA(PhiIa);
            _UIpara.PhiIb = DouA(PhiIb);
            _UIpara.PhiIc = DouA(PhiIc);

        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);
            buf.Put(0x20);
            buf.Put(0x01);

            buf.Put(GetBytesDot4(_UIpara.PhiUa));
            buf.Put(GetBytesDot4(_UIpara.PhiIa));
            buf.Put(GetBytesDot4(_UIpara.PhiUb));
            buf.Put(GetBytesDot4(_UIpara.PhiIb));
            buf.Put(GetBytesDot4(_UIpara.PhiUc));
            buf.Put(GetBytesDot4(_UIpara.PhiIc));
            buf.Put(GetBytesDot4(50));

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            return $"升源：{_UIpara.PhiUa},{_UIpara.PhiUb},{_UIpara.PhiUc},{_UIpara.PhiIa},{_UIpara.PhiIb},{_UIpara.PhiIc}";
        }
    }





    internal class ZH3001_RequestCalibrationPowerAngleReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }

    }


    #endregion ZH3001设置相频
    #endregion

    #region ZH3001设置停止或恢复大电流长时间工作保护机制 
    /// <summary>
    /// 设置停止或恢复大电流长时间工作保护机制
    /// </summary>
    internal class ZH3001_SetPowerSafePacket : ZH3001SendPacket
    {
        private int _Mode;
        public void SetPara(int Mode)
        {
            _Mode = Mode;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x21);
            buf.Put(0x06);
            if (_Mode == 00)
            {
                buf.Put(0x00);
            }
            else
            {
                buf.Put(0x01);
            }

            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            return "设置保护机制。";
        }
    }
    /// <summary>
    /// 设置保护机制 返回指令
    /// </summary>
    internal class ZH3001_SetPowerSafeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion 设置停止或恢复大电流长时间工作保护机制  
    #endregion


}
