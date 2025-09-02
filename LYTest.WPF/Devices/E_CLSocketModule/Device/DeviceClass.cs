using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using E_CLSocketModule.SocketModule.Packet;

namespace E_CLSocketModule
{
   
    #region 303功率源

    /// <summary>
    /// 源联机指令
    /// 0x52是字母"R"的ASC码
    /// ox4F是O的ASC码
    /// </summary>
    internal class CL303_RequestLinkPacket : CL303SendPacket
    {

        /// <summary>
        /// 是否是联机
        /// </summary>
        public bool IsLink = true;


        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            if (IsLink)
                buf.Put(0x52);
            else
                buf.Put(0x4F);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 读取源状态
    /// </summary>
    internal class CL303_RequestReadPowerStatePacket : CL303SendPacket
    {
        protected override byte[] GetBody()
        {
            return new byte[1] { 0x42 };
        }
    }
    /// <summary>
    /// 返回源状态
    /// </summary>
    internal class CL303_RequestReadPowerStateReplyPacket : CL303RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            ;
        }
    }
    /// <summary>
    /// 读取CL303源版本号
    /// 发送字母V 的ASC码
    /// </summary>
    internal class CL303_RequestReadVersionPacket : CL303SendPacket
    {
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x56);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 读取CL303版本号返回包
    /// </summary>
    internal class CL303_RequestReadVersionReplayPacket : CL303RecvPacket
    {
        public CL303_RequestReadVersionReplayPacket() : base() { }

        /// <summary>
        /// 读取到的版本号
        /// 默认值为Unknown
        /// </summary>
        public string Version { get; private set; }


        protected override void ParseBody(byte[] data)
        {
            Version = ASCIIEncoding.UTF8.GetString(data);
        }
    }
    /// <summary>
    /// 设置频率指令
    /// ox33
    /// 返回：CLNormalRequestResultReplayPacket
    /// </summary>
    internal class CL303_RequestSetFreqPacket : CL303SendPacket
    {
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x33);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 请求关源指令
    /// 返回:CLNormalRequestResultReplayPacket
    /// </summary>
    internal class CL303_RequestSetPowerOffPacket : CL303SendPacket
    {
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x45);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// ox30指令升源
    /// </summary>
    internal class CL303_RequestSetPowerOnPacket0x30 : CL303SendPacket
    {
        /// <summary>
        /// 电压参数
        /// </summary>
        public struct UPara
        {
            public float Ua;
            public float Ub;
            public float Uc;
            public float Ia;
            public float Ib;
            public float Ic;
        }
        private UPara m_Upara;
        public struct PhiPara
        {
            public float PhiUa;
            public float PhiUb;
            public float PhiUc;
            public float PhiIa;
            public float PhiIb;
            public float PhiIc;
        }
        private PhiPara m_Phipara;
        private byte m_xwkz = 63;
        private byte m_xiebo = 0;
        private float m_freq = 50.0f;
        /// <summary>
        /// true 逆相序，false 正相序
        /// </summary>
        private bool _bln_NXX = false;

        /// <summary>
        /// 谐波开关设置,//fjk 修改为m_xiebo =   原先m_xwkz =
        /// </summary>
        public bool OpenXieBo { set { m_xiebo = value ? (byte)0xFF : (byte)0; } }
        /// <summary>
        /// 设置频率,默认值50hz
        /// </summary>
        public float Freq { set { m_freq = value; } get { return m_freq; } }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="upara">电压参数</param>
        /// <param name="phipara"></param>
        public void SetPara(byte clfs, UPara upara, PhiPara phipara)
        {
            m_Upara = upara;
            m_Phipara = phipara;
            if (clfs == 7)
                m_xwkz &= 9;
            else if (clfs == 6)
                m_xwkz &= 45;
        }

        /// <summary>
        /// 设置源功率因数
        /// </summary>
        /// <param name="Glys">功率因数如(0.5L,1.0,-1,-0,0.5C)</param>
        /// <param name="jxfs">0-三相四线有功表PT4;1-三相三线有功表P32;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60)</param>
        /// <returns></returns>
        public bool SetAcSourcePowerFactor(string Glys, byte jxfs, bool PH, int iYuanjian,bool IsNxx)
        {
            //jxfs 0-三相四线有功表；1-三相三线有功表;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);
            //4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60);
            _bln_NXX = IsNxx;
            #region
            float XwUa = 0;
            float XwUb = 0;
            float XwUc = 0;
            float XwIa = 0;
            float XwIb = 0;
            float XwIc = 0;
            string strGlys = "";
            string LC = "";
            double LcValue = 0;
            float Phi = 0;
            int n = 1;

            strGlys = Glys;

            if (jxfs == 0)// 三相四线有功 = 0,
            { jxfs = 0; }
            else if (jxfs == 1)//三相四线无功 = 1,
            { jxfs = 2; }
            else if (jxfs == 2)//三相三线有功 = 2,
            { jxfs = 1; }
            else if (jxfs == 3)//三相三线无功 = 3,
            { jxfs = 3; }
            else if (jxfs == 4)//二元件跨相90 = 4,
            { jxfs = 5; }
            else if (jxfs == 5)//二元件跨相60 = 5,
            { jxfs = 6; }
            else if (jxfs == 6)//三元件跨相90 = 6,
            { jxfs = 4; }
            else if (jxfs == 7)
            { jxfs = 7; }


            if (Glys == "0") strGlys = "0L";
            if (Glys == "-0") strGlys = "0C";
            LC = GetUnit(strGlys);
            if (LC.Length > 0)
            {
                LcValue = Convert.ToDouble(strGlys.Replace(LC, ""));
            }
            else
            {
                LcValue = Convert.ToDouble(strGlys);
            }

            switch (jxfs)
            {
                case 0:  //三相四线有功表
                    Phi = Convert.ToSingle(Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5));
                    XwUa = 0;
                    XwUb = 240;
                    XwUc = 120;
                    if (LcValue > 0)
                    {
                        XwIa = 0;
                        XwIb = 240;
                        XwIc = 120;
                        Phi = 1 * Phi;

                    }
                    else if (LcValue < 0)
                    {
                        XwIa = 180;
                        XwIb = 60;
                        XwIc = 300;
                        Phi = -1 * Phi;
                    }
                    if (LC == "L")
                    {
                        Phi = 1 * Phi;
                        XwIa = XwUa -Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb = XwIb + 360;
                        if (XwIb >= 360) XwIb = XwIb - 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;

                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb = XwIb + 360;
                        if (XwIb >= 360) XwIb = XwIb - 360;

                        XwIc = XwUc - Phi;

                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    break;
                case 1:  //三相三线有功表
                    Phi = Convert.ToSingle(Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5));
                    XwUa = 30;
                    XwUb = 240;
                    XwUc = 90;
                    if (LcValue > 0)
                    {
                        if (iYuanjian == 0)
                        {
                            XwIa = 0;
                            XwIb = 240;
                            XwIc = 120;
                            Phi = 1 * Phi;
                        }
                        else if (iYuanjian == 1)
                        {
                            XwIa = 30;
                            XwIb = 270;
                            XwIc = 150;
                            Phi = 1 * Phi;
                        }
                        else if (iYuanjian == 3)
                        {
                            XwIa = 330;
                            XwIb = 210;
                            XwIc = 90;
                            Phi = 1 * Phi;
                        }
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 180;
                        XwIb = 0;
                        XwIc = 240;
                        Phi = -1 * Phi;
                    }
                    if (LC == "L")
                    {
                        if (iYuanjian == 0)
                        {
                            Phi = 1 * Phi;
                            XwIa = 0 - Phi;
                            if (XwIa < 0) XwIa = XwIa + (360);
                            if (XwIa >= 360) XwIa = XwIa - (360);
                            XwIb = 240 - Phi;
                            if (XwIb < 0) XwIb = XwIb + (360);
                            if (XwIb >= 360) XwIb = XwIb - (360);
                            XwIc = 120 - Phi;
                            if (XwIc < 0) XwIc = XwIc + 360;
                            if (XwIc >= 360) XwIc = XwIc - 360;
                        }
                        else if (iYuanjian == 1)
                        {
                            Phi = 1 * Phi;
                            XwIa = 330;
                            if (XwIa < 0) XwIa = XwIa + (360);
                            if (XwIa >= 360) XwIa = XwIa - (360);
                            XwIb = 0;
                            if (XwIb < 0) XwIb = XwIb + (360);
                            if (XwIb >= 360) XwIb = XwIb - (360);
                            XwIc = 0;
                            if (XwIc < 0) XwIc = XwIc + 360;
                            if (XwIc >= 360) XwIc = XwIc - 360;
                        }
                        else if (iYuanjian == 3)
                        {
                            Phi = 1 * Phi;
                            XwIa = 0;
                            if (XwIa < 0) XwIa = XwIa + (360);
                            if (XwIa >= 360) XwIa = XwIa - (360);
                            XwIb = 0;
                            if (XwIb < 0) XwIb = XwIb + (360);
                            if (XwIb >= 360) XwIb = XwIb - (360);
                            XwIc = 90 - Phi;
                            if (XwIc < 0) XwIc = XwIc + 360;
                            if (XwIc >= 360) XwIc = XwIc - 360;
                        }
                    }
                    if (LC == "C")
                    {
                        if (iYuanjian == 0)
                        {
                            Phi = -1 * Phi;
                            XwIa = 0 - Phi - 6;
                            if (XwIa < 0) XwIa = XwIa + 360;
                            if (XwIa >= 360) XwIa = XwIa - 360;

                            XwIb = 240 - Phi - 6;
                            if (XwIb < 0) XwIb = XwIb + (360);
                            if (XwIb >= 360) XwIb = XwIb - (360);

                            XwIc = 120 - Phi - 6;
                            if (XwIc < 0) XwIc = XwIc + 360;
                            if (XwIc >= 360) XwIc = XwIc - 360;
                        }
                        else if (iYuanjian == 1)
                        {
                            Phi = -1 * Phi;
                            XwIa = 60;
                            if (XwIa < 0) XwIa = XwIa + 360;
                            if (XwIa >= 360) XwIa = XwIa - 360;

                            XwIb = 240 - Phi - 6;
                            if (XwIb < 0) XwIb = XwIb + (360);
                            if (XwIb >= 360) XwIb = XwIb - (360);

                            XwIc = 120 - Phi - 6;
                            if (XwIc < 0) XwIc = XwIc + 360;
                            if (XwIc >= 360) XwIc = XwIc - 360;
                        }
                        else if (iYuanjian == 3)
                        {
                            Phi = -1 * Phi;
                            XwIa = 0 - Phi - 6;
                            if (XwIa < 0) XwIa = XwIa + 360;
                            if (XwIa >= 360) XwIa = XwIa - 360;

                            XwIb = 240 - Phi - 6;
                            if (XwIb < 0) XwIb = XwIb + (360);
                            if (XwIb >= 360) XwIb = XwIb - (360);

                            XwIc = 120;
                            if (XwIc < 0) XwIc = XwIc + 360;
                            if (XwIc >= 360) XwIc = XwIc - 360;
                        }
                    }
                    if (PH == false)
                    {
                        XwIa = XwIa + 30;
                        XwIc = XwIc + 30;
                    }
                    break;
                case 2: //三相四线真无功表(QT4)
                    XwUa = 0;
                    XwUb = 240;
                    XwUc = 120;
                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 150;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 330;
                        XwIc = 210;
                        n = -1;
                    }
                    if (LC == "L")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb = XwIb + 360;
                        if (XwIb >= 360) XwIb = XwIb - 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                        else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = n + Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb = XwIb + 360;
                        if (XwIb >= 360) XwIb = XwIb - 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    break;
                case 3: //三相三线真无功表(Q32)
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 90;

                    if (LcValue > 0)
                    {
                        if (iYuanjian == 0)
                        {
                            XwIa = 270;
                            XwIb = 0;
                            XwIc = 30;
                        }
                        else if (iYuanjian == 1)
                        {
                            XwIa = 300;
                            XwIb = 0;
                            XwIc = 0;
                        }
                        else if (iYuanjian == 3)
                        {
                            XwIa = 0;
                            XwIb = 0;
                            XwIc = 0;
                        }
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 0;
                        XwIc = 210;
                        n = -1;
                    }

                    if (LC == "L")
                    {
                        switch (iYuanjian)
                        {
                            case 0:
                                if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                //Phi = Phi;
                                XwIa = 0 - Phi;
                                if (XwIa < 0) XwIa = XwIa + 360;
                                if (XwIa >= 360) XwIa = XwIa - 360;

                                XwIb = 0;


                                XwIc = 120 - Phi;
                                if (XwIc < 0) XwIc = XwIc + 360;
                                if (XwIc >= 360) XwIc = XwIc - 360;
                                break;
                            case 1:
                                if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                //Phi = Phi;
                                XwIa = 30 - Phi;
                                if (XwIa < 0) XwIa = XwIa + 360;
                                if (XwIa >= 360) XwIa = XwIa - 360;

                                XwIb = 0;


                                XwIc = 120 - Phi;
                                if (XwIc < 0) XwIc = XwIc + 360;
                                if (XwIc >= 360) XwIc = XwIc - 360;
                                break;
                            case 3:
                                if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                //Phi = Phi;
                                XwIa = 0 - Phi;
                                if (XwIa < 0) XwIa = XwIa + 360;
                                if (XwIa >= 360) XwIa = XwIa - 360;

                                XwIb = 0;


                                XwIc = 90 - Phi;
                                if (XwIc < 0) XwIc = XwIc + 360;
                                if (XwIc >= 360) XwIc = XwIc - 360;
                                break;
                        }
                    }
                    if (LC == "C")
                    {
                        switch (iYuanjian)
                        {
                            case 0:
                                if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                                else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                //Phi = n + Phi;
                                XwIa = 0 - Phi - 6;
                                if (XwIa < 0) XwIa = XwIa + 360;
                                if (XwIa >= 360) XwIa = XwIa - 360;

                                XwIb = 0;


                                XwIc = 120 - Phi - 6;
                                if (XwIc < 0) XwIc = XwIc + 360;
                                if (XwIc >= 360) XwIc = XwIc - 360;
                                break;
                            case 1:
                                if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                                else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                //Phi = n + Phi;
                                XwIa = 330;
                                if (XwIa < 0) XwIa = XwIa + 360;
                                if (XwIa >= 360) XwIa = XwIa - 360;

                                XwIb = 0;


                                XwIc = 120 - Phi - 6;
                                if (XwIc < 0) XwIc = XwIc + 360;
                                if (XwIc >= 360) XwIc = XwIc - 360;
                                break;
                            case 3:
                                if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                                else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                //Phi = n + Phi;
                                XwIa = 0 - Phi - 6;
                                if (XwIa < 0) XwIa = XwIa + 360;
                                if (XwIa >= 360) XwIa = XwIa - 360;

                                XwIb = 0;


                                XwIc = 330;
                                if (XwIc < 0) XwIc = XwIc + 360;
                                if (XwIc >= 360) XwIc = XwIc - 360;
                                break;
                        }
                    }
                    if (PH == false)
                    {
                        XwIa = XwIa + 30;
                        XwIc = XwIc - 30;
                    }
                    break;
                case 4: //三元件跨相90无功表(Q33)
                    XwUa = 30;
                    XwUb = 270;
                    XwUc = 150;
                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 150;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 330;
                        XwIc = 210;
                        n = -1;
                    }

                    if (LC == "L")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb = XwIb + 360;
                        if (XwIb >= 360) XwIb = XwIb - 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                        else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb = XwIb + 360;
                        if (XwIb >= 360) XwIb = XwIb - 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    break;
                case 5: //二元件跨相90无功表(Q90)
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 270;

                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 0;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 0;
                        XwIc = 210;
                        n = -1;
                    }

                    if (LC == "L")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                        else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    break;
                case 6: //二元件跨相60无功表(Q60)
                    XwUa = 0;
                    XwUb = 0;
                    XwUc = 120;

                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 0;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 0;
                        XwIc = 210;
                        n = -1;
                    }
                    if (LC == "L")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = Convert.ToSingle(Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = Convert.ToSingle((-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5)));
                        else Phi = Convert.ToSingle(180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc = XwIc + 360;
                        if (XwIc >= 360) XwIc = XwIc - 360;
                    }
                    break;
                case 7: //单相表
                    Phi = Convert.ToSingle(Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5));
                    XwUa = 0;
                    if (LcValue > 0)
                    {
                        XwIa = 0;
                        Phi = 1 * Phi;

                    }
                    else if (LcValue < 0)
                    {
                        XwIa = 180;
                        Phi = -1 * Phi;
                    }
                    if (LC == "L")
                    {
                        Phi = 1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa = XwIa + 360;
                        if (XwIa >= 360) XwIa = XwIa - 360;
                    }
                    XwUb = XwUa;
                    XwUc = XwUa;
                    XwIb = XwIa;
                    XwIc = XwIa;
                    break;
            }
            #endregion

            //Single[] sng_PhiXX = GetPhiGlys((int)jxfs, Glys, (int)iYuanjian, PH); //根据测试方式、功率因数、元件、相序计算三相电压电流角度
            //tmpOk = SetAcSourcePhasic(XwUa, XwUb, XwUc, XwIa, XwIb, XwIc);
            m_Phipara.PhiUa = XwUa;
            m_Phipara.PhiUb = XwUb;
            m_Phipara.PhiUc = XwUc;
            m_Phipara.PhiIa = XwIa;
            m_Phipara.PhiIb = XwIb;
            m_Phipara.PhiIc = XwIc;
            if (true == _bln_NXX)//电压电流逆相序
            {
                switch (jxfs)
                {
                    //三相三线有功表
                    case 1:
                        m_Phipara.PhiUb = 0;//XwUc;
                        m_Phipara.PhiUc = 330;//XwUb;
                        break;
                    default:
                        m_Phipara.PhiUb = XwUc;
                        m_Phipara.PhiUc = XwUb;

                        break;
                }
                m_Phipara.PhiIb = XwIc;
                m_Phipara.PhiIc = XwIb;
            }
            else
            {
                m_Phipara.PhiUb = XwUb;
                m_Phipara.PhiUc = XwUc;

                m_Phipara.PhiIb = XwIb;
                m_Phipara.PhiIc = XwIc;
            }
            return true;
        }

        private string GetUnit(string chrVal)  //得到量程的单位 //chrVal带单位的值如 15A
        {
            int i = 0;
            string cUnit = "";
            byte[] chrbytes = new byte[256];
            ASCIIEncoding ascii = new ASCIIEncoding();
            chrbytes = ascii.GetBytes(chrVal);
            for (i = 0; i < chrbytes.Length; ++i)
            {
                if (chrbytes[i] > 57)
                {
                    cUnit = chrVal.Substring(i);
                    break;
                }

            }
            return cUnit;
        }

        public override string GetPacketName()
        {
            return "CL303_RequestSetPowerOnPacket0x30";
        }

        public CL303_RequestSetPowerOnPacket0x30()
        {
            this.ToID = 0x20;
        }


        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x30);
            buf.Put(m_xwkz);                //相位控制
            buf.Put(m_xiebo);               //谐波开关
            buf.Put(get10bitData(Freq));    //频率
            //UA
            buf.Put(GetUScale(m_Upara.Ua));       //
            buf.Put(get10bitData(m_Upara.Ua));    //
            buf.Put(get10bitData(m_Phipara.PhiUa)); //
            //Ub
            buf.Put(GetUScale(m_Upara.Ub));       //
            buf.Put(get10bitData(m_Upara.Ub));    //
            buf.Put(get10bitData(m_Phipara.PhiUb)); //

            //Uc
            buf.Put(GetUScale(m_Upara.Uc));       //
            buf.Put(get10bitData(m_Upara.Uc));    //
            buf.Put(get10bitData(m_Phipara.PhiUc)); //

            //Ia
            buf.Put(GetIScale(m_Upara.Ia));       //
            buf.Put(get10bitData(m_Upara.Ia));    //
            buf.Put(get10bitData(m_Phipara.PhiIa)); //
            //Ib
            buf.Put(GetIScale(m_Upara.Ib));       //
            buf.Put(get10bitData(m_Upara.Ib));    //
            buf.Put(get10bitData(m_Phipara.PhiIb)); //
            //Ic
            buf.Put(GetIScale(m_Upara.Ic));       //樊江凯，修改--档位错误
            buf.Put(get10bitData(m_Upara.Ic));    //
            buf.Put(get10bitData(m_Phipara.PhiIc)); //
            if (buf.ToByteArray().Length != 139)
                throw new Exception(GetPacketName() + "数据包长度不对");
            return buf.ToByteArray();
        }
        private byte GetIScale(Single sngI)
        {
            if (sngI <= 0.012) return 80;            //"50";
            else if (sngI <= 0.03) return 81;        //"51";
            else if (sngI <= 0.06) return 82;        // "52";
            else if (sngI <= 0.12) return 83;       // "53";
            else if (sngI <= 0.3) return 84;        //"54";
            else if (sngI <= 0.6) return 85;        //"55";
            else if (sngI <= 1.2) return 86;        //"56";
            else if (sngI <= 3) return 87;          // "57";
            else if (sngI <= 6) return 88;          //"58";
            else if (sngI <= 12) return 89;         //"59";
            else if (sngI <= 30) return 90;         //"5a";
            else if (sngI <= 60) return 91;         //"5b";
            else if (sngI <= 120) return 92;        // "5c";
            else return 92;// "5c";
        }

        private byte GetUScale(Single sngU)
        {
            if (sngU <= 57 * 1.2) return 64;//"40";
            else if (sngU <= 120) return 65;// "41";
            else if (sngU <= 264) return 66;// "42";
            else if (sngU <= 480) return 67;//"43";
            else if (sngU <= 900) return 68;//"44";
            else return 66;// "42";
        }
    }
    /// <summary>
    /// 相同电流电压源输出指令
    /// 返回4B则成功
    /// </summary>
    internal class CL303_RequestSetPowerOnPacket0x35 : CL303SendPacket
    {
        private byte m_clfs = 0;
        private byte m_xwkz = 0;
        private bool m_xiebo = false;
        private float m_U = 0;
        private float m_I = 0;
        private bool m_isDuiSheBiao = false;
        private float m_phi = 0;
        /// <summary>
        /// 设置频率，默认为50HZ
        /// </summary>
        public float Freq = 50;
        /// <summary>
        /// 电压百分比
        /// </summary>
        public float PercentOfU = 100F;
        /// <summary>
        /// 是否是潜动
        /// </summary>
        public bool IsCreeping = false;
        /// <summary>
        /// 元件
        /// </summary>
        public Enum.Cus_EmPowerYuanJiang m_YuanJian = Enum.Cus_EmPowerYuanJiang.H;

        /// <summary>
        /// 设置源控制参数
        /// </summary>
        /// <param name="clfs">
        /// 测量方式
        /// 0表示PT4       1表示QT4    2表示P32 
        /// 3表示Q32       4表示Q60    5表示Q90
        /// 6表示Q33       7表示P
        /// </param>
        /// <param name="xiebo"></param>
        /// <param name="U"></param>
        /// <param name="I"></param>
        /// <param name="duisebiao"></param>
        /// <param name="Phi"></param>
        public void SetPara(byte clfs, byte xwkz, bool xiebo,
                            float U, float I, bool duisebiao, float Phi)
        {
            m_clfs = clfs;
            m_isDuiSheBiao = duisebiao;
            m_xwkz = 63;
            m_xwkz = xwkz;
            m_xiebo = xiebo;
            m_U = U;
            m_I = I;
            m_phi = Phi;
            //计算不平衡负载
            updateClfs();       //更新测量方式字节 

        }

        /// <summary>
        /// 计算测量方式:
        /// </summary>
        private void updateClfs()
        {
            m_clfs = (byte)(m_isDuiSheBiao ? m_clfs ^ 128 : m_clfs & 127);  //对标标志
            m_clfs = (byte)(m_clfs & 191);                                  //缓降
            m_clfs = (byte)(m_clfs & 223);                                  //闭环
            //m_clfs = 0x00;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            /*
            接线方式          1Byte    	
            不平衡负载        1Byte    	
            波段开关          1 Byte   	
            谐波开关          1Byte    	
            电压档位          1 Byte	2008512 20090034
            电流档位          1 Byte	
            电压幅度          4 Byte	值×6553600
            电流幅度          4 Byte	值×6553600
            φ                4 Byte	值×6553600
            频率              4 Byte	值×6553600
            负载率            4 Byte	值×6553600

             */
            byte boduan = GetBoDuan();
            buf.Put(0x35);              //CMD    
            buf.Put(m_clfs);            //接线方式
            buf.Put(boduan);                 //只让加电流开关有效
            if (m_isDuiSheBiao)
                buf.Put(0x0f);
            else
                buf.Put(m_xwkz);            //相位开关 即ABC电压ABC电流，哪相输出哪相关闭
            buf.PutInt(0xFFFFFFFF);
            buf.PutUShort(0xFFFF);
            if (m_xiebo)
                buf.Put(0xFF);
            else
                buf.Put(0);
            buf.Put(GetUScale(m_U));
            if (m_isDuiSheBiao)
                buf.Put(86);
            else
                buf.Put(GetIScale(m_I));
            buf.PutInt((int)(m_U * 65536));
            buf.PutInt((int)(m_I * 65536 * 100));
            buf.PutInt((int)(m_phi * 65536));
            buf.PutInt((int)(Freq * 65536));
            buf.Put(0);
            buf.Put(100);
            buf.PutUShort(0);
            //81:20:00:26:35:01:18:3f:ff:ff:ff:ff:ff:ff:00:42:57:00:dc:00:00:00:96:00:00:00:5a:00:00:00:32:00:00:00:64:00:00:46
            //            35-00-18-3F-FF-FF-FF-FF-FF-FF-00-42-57-00-DC-00-00-00-96-00-00-00-5A-00-00-00-32-00-00-00-64-00-00
            return buf.ToByteArray();
        }
        /// <summary>
        /// 获取波段控制
        /// </summary>
        /// <returns></returns>
        private byte GetBoDuan()
        {
            byte Tb = 0;
            if (IsCreeping)
                Tb = 0x80;
            byte Tb2 = 0;
            if (PercentOfU == 0 || PercentOfU == 100)
                Tb2 = 0x10;
            else if (PercentOfU == 110)
                Tb2 = 0x20;
            else if (PercentOfU == 115)
                Tb2 = 0x30;
            else if (PercentOfU == 120)
                Tb2 = 0x40;
            Tb += Tb2;

            if (m_I > 0F)
                Tb += 8;
            Tb += (byte)(((byte)m_YuanJian) - 1);
            return Tb;
        }

        private byte GetIScale(Single sngI)
        {
            if (sngI <= 0.012) return 80;            //"50";
            else if (sngI <= 0.03) return 81;        //"51";
            else if (sngI <= 0.06) return 82;        // "52";
            else if (sngI <= 0.12) return 83;       // "53";
            else if (sngI <= 0.3) return 84;        //"54";
            else if (sngI <= 0.6) return 85;        //"55";
            else if (sngI <= 1.2) return 86;        //"56";
            else if (sngI <= 3) return 87;          // "57";
            else if (sngI <= 6) return 88;          //"58";
            else if (sngI <= 12) return 89;         //"59";
            else if (sngI <= 30) return 90;         //"5a";
            else if (sngI <= 60) return 91;         //"5b";
            else if (sngI <= 120) return 92;        // "5c";
            else return 92;// "5c";
        }

        private byte GetUScale(Single sngU)
        {
            if (sngU <= 57 * 1.2) return 64;//"40";
            else if (sngU <= 120) return 65;// "41";
            else if (sngU <= 264) return 66;// "42";
            else if (sngU <= 480) return 67;//"43";
            else if (sngU <= 900) return 68;//"44";
            else return 66;// "42";
        }
    }
    /// <summary>
    /// 设置电压跌落试验请求包0x36
    /// 返回包:CLNormalRequestResultReplayPacket
    /// </summary>
    internal class CL303_RequestSetVoltageFallOffPacket : CL303SendPacket
    {
        /// <summary>
        /// 试验方式
        /// </summary>
        public byte VerifyType = 0;

        public override string GetPacketName()
        {
            return "CL303_RequestSetVoltageFallOffPacket";
        }

        public CL303_RequestSetVoltageFallOffPacket()
        {
            this.ToID = 0x20;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="upara">电压参数</param>
        /// <param name="phipara"></param>
        public void SetPara(byte verifyType)
        {
            VerifyType = verifyType;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x36);
            buf.Put(VerifyType);
            return buf.ToByteArray();
        }

        //protected override byte FrameLengthByteCount
        //{
        //    get { throw new NotImplementedException(); }
        //}
    }
    /// <summary>
    /// 设置谐波请求包[0x32]
    /// </summary>
    internal class Cl303_RequestSetXieBoPacket : CL303SendPacket
    {
        /// <summary>
        /// 是否加谐波
        /// </summary>
        public bool AddXieBo { get; set; }
        /// <summary>
        /// 当前加谐波的元件
        ///  A相电压 = 0,
        ///B相电压 = 1,
        ///C相电压 = 2,
        ///A相电流 = 3,
        ///B相电流 = 4,
        ///C相电流 = 5
        /// </summary>
        public byte YuanJian { get; set; }
        float[] content = new float[64];
        float[] phase = new float[64];
        public Cl303_RequestSetXieBoPacket()
        {
            AddXieBo = true;
        }

        public void SetPara(float[] contents, float[] phases)
        {
            content = contents;
            phase = phases;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.SetLength(424);
            buf.Position = 0;
            buf.Put(0x32);                      //CMD
            buf.Put(YuanJian);                  //当前相位
            buf.PutUShort(0xFFFF);              //波开关
            buf.Put(0xFF);                      //含量状态
            for (int i = 0; i < 21; i++)
            {
                byte[] bytData = To10Bit(content[i]);
                buf.Put(bytData);//含量
                bytData = To10Bit(phase[i]);
                buf.Put(bytData);//相位
            }
            return buf.ToByteArray();
        }

        /// <summary>
        /// 转换成10个Bit的值
        /// </summary>
        /// <param name="sValue">转换值</param>
        /// <returns></returns>
        private byte[] To10Bit(Single sng_Value)
        {
            string sData = Convert.ToString(sng_Value);
            if (sData.IndexOf('.') <= 0) sData += ".";
            sData += "0000000000";
            sData = sData.Substring(0, 9);
            byte[] bPara = ASCIIEncoding.ASCII.GetBytes(sData);
            Array.Resize(ref bPara, bPara.Length + 1);
            bPara[9] = 48;          //Convert.ToByte((sng_Value - Math.Floor(sng_Value)) == 0 ? 46 : 48);
            return bPara;
        }

    }


    #endregion

    #region 其它
    /// <summary>
    /// 结论返回
    /// 0x4b:成功
    /// </summary>
    internal class CLNormalRequestResultReplayPacket : ClouRecvPacket_NotCltTwo
    {
        public CLNormalRequestResultReplayPacket()
            : base()
        {
        }
        /// <summary>
        /// 结论
        /// </summary>
        public virtual ReplayCode ReplayResult
        {
            get;
            private set;
        }

        public override string GetPacketName()
        {
            return "CLNormalRequestResultReplayPacket";
        }
        protected override void ParseBody(byte[] data)
        {
            if (data.Length == 2)
                ReplayResult = (ReplayCode)data[1];
            else
                ReplayResult = (ReplayCode)data[0];
        }

        public enum ReplayCode
        {
            /// <summary>
            /// CLT11返回
            /// </summary>
            CLT11OK = 0x30,
            /// <summary>
            /// 响应命令，表示“OK”
            /// </summary>
            Ok = 0x4b,
            /// <summary>
            /// 响应命令，表示出错
            /// </summary>
            Error = 0x33,
            /// <summary>
            /// 响应命令，表示系统估计还要忙若干mS
            /// </summary>
            Busy = 0x35,
            /// <summary>
            /// 误差板联机成功
            /// </summary>
            CL321inkOk = 0x36,
            /// <summary>
            /// 标准表脱机成功
            /// </summary>
            Cl311UnLinkOk = 0x37
        }
    }
    #endregion
}
