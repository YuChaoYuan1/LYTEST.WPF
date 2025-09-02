using CLOU;
using Enum;
using SocketModule.Packet;
using Struct;
using System;
using System.Collections.Generic;
using System.Text;
using ZH;

namespace E_CL309
{

    #region CL309功率源

    #region CL309源联机指令
    /// <summary>
    /// 源联机/脱机请求包
    /// </summary>
    internal class CL309_RequestLinkPacket : CL309SendPacket
    {
        public bool IsLink = true;

        public CL309_RequestLinkPacket()
            : base()
        { }

        /*
         * 81 01 PCID 06 C9 00 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xC9);  //命令           
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 源联机 返回指令
    /// </summary>
    internal class Cl309_RequestLinkReplyPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 36)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x39)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region CL309关源指令
    /// <summary>
    /// 控制源 请求包
    /// </summary>
    internal class CL309_RequestPowerOffPacket : CL309SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流
        public static float oldUa = 0.00F;
        public static float oldUb = 0.00F;
        public static float oldUc = 0.00F;
        public static float oldIa = 0.00F;
        public static float oldIb = 0.00F;
        public static float oldIc = 0.00F;


        //private const int AskDat = 0xA0;  //
        //private const int WrtDat = 0xA3; //
        //private const int WrtAry = 0xA6;//
        //private const int EchOk = 0x30;	//
        //private const int EchErr = 0x33;	//
        //private const int EchBsy = 0x35; //
        //private const int EchInh = 0x36;	//

        //private const int HeadPos = 0;
        //private const int RxIDPos = 1;
        //private const int TxIDPos = 2;
        //private const int FlenPos = 3;
        //private const int ComdPos = 4;
        //private const int FRAME_ID = 0x81;

        //private const int PagePos = 5;
        //private const int GrpPos = 6;
        //private const int AryPos = 6;
        //private const int Start0Pos = 7;
        //private const int Start1Pos = 8;
        //private const int LenPos = 9;

        //private const int Grp0Pos = 7;
        //private const int Grp1Pos = 8;
        //private const int Grp2Pos = 9;
        //private const int Grp3Pos = 10;
        //private const int Grp4Pos = 11;
        //private const int Grp5Pos = 12;
        //private const int Grp6Pos = 13;
        //private const int Grp7Pos = 14;

        //private const int LOCAL_ID = 0x05;
        //private const int OBJ_ID = 0x01;


        //private const int PAGE0 = 0;
        //private const int PAGE1 = 1;
        //private const int PAGE2 = 2;
        //private const int PAGE3 = 3;
        //private const int PAGE4 = 4;
        private const int PAGE5 = 5;
        //private const int PAGE6 = 6;

        //private const int GROUP0 = 0x01;
        private const int GROUP1 = 0x02;
        private const int GROUP2 = 0x04;
        //private const int GROUP3 = 0x08;
        //private const int GROUP4 = 0x10;
        //private const int GROUP5 = 0x20;
        private const int GROUP6 = 0x40;
        //private const int GROUP7 = 0x80;

        private const int DATA0 = 0x01;
        private const int DATA1 = 0x02;
        private const int DATA2 = 0x04;
        private const int DATA3 = 0x08;
        private const int DATA4 = 0x10;
        private const int DATA5 = 0x20;
        private const int DATA6 = 0x40;
        private const int DATA7 = 0x80;

        //private const int COS_SIN_BEISHU = 10000;//COSIN放大倍数
        private const int JIAODU_BEISHU = 10000;//角度放大倍数
        //private const int PINLV_BEISHU = 10000;//频率放大倍数


        private double m_UaXwValue = 0;//Ua相位角度
        private double m_UbXwValue = 240;//Ub相位角度
        private double m_UcXwValue = 120;//UC相位角度
        private double m_IaXwValue = 0;//IA相位角度
        private double m_IbXwValue = 240;
        private double m_IcXwValue = 120;

        private float _Ua;
        private float _Ub;
        private float _Uc;

        private float _Ia;
        private float _Ib;
        private float _Ic;

        private float _PhiUa;
        private float _PhiUb;
        private float _PhiUc;

        private float _PhiIa;
        private float _PhiIb;
        private float _PhiIc;

        private float _Hz;

        //private int _iChange = 0x00;

        private bool _bCurrentDB;

        public bool _bIsUpDateVoltage = true;
        #endregion
        public CL309_RequestPowerOffPacket()
            : base()
        { }

        public void SetPara()
        {
            SetPara(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 50, false);
        }

        public void SetPara(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, float PhiUa, float PhiUb, float PhiUc,
            float PhiIa, float PhiIb, float PhiIc, float Hz, bool bDBOut)
        {
            //Ia 是否变化， 
            //if (oldIa == Ia)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                //_iChange += DATA5;
                oldIa = Ia;
            }
            //Ib 是否变化， 
            //if (oldIb == Ib)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                //_iChange += DATA4;
                oldIb = Ib;
            }
            //Ic 是否变化， 
            //if (oldIc == Ic)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                //_iChange += DATA3;
                oldIc = Ic;
            }

            ////Ua 是否变化， 
            //if (oldUa == Ua)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                //_iChange += DATA2;
                oldUa = Ua;
            }
            ////Ub 是否变化， 
            //if (oldUb == Ub)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                //_iChange += DATA1;
                oldUb = Ub;
            }
            ////Uc 是否变化， 
            //if (oldUc == Uc)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                //_iChange += DATA0;
                oldUc = Uc;
            }

            _Ua = Ua;
            _Ub = Ub;
            _Uc = Uc;

            _Ia = Ia;
            _Ib = Ib;
            _Ic = Ic;

            _PhiIa = PhiIa;
            _PhiIb = PhiIb;
            _PhiIc = PhiIc;
            _PhiUa = PhiUa;
            _PhiUb = PhiUb;
            _PhiUc = PhiUc;
            _Hz = Hz;

            _bCurrentDB = bDBOut;
        }

        public void SetPara(float U, float I, string str_Glys, float Hz, int iClfs,  bool bDBOut, Cus_PowerYuanJiang HABC)
        {
            float m_UaValue = 0f;
            float m_UbValue = 0f;
            float m_UcValue = 0f;
            float m_IaValue = 0f;
            float m_IbValue = 0f;
            float m_IcValue = 0f;

            //r_Glys = "-1.0";            
            switch (HABC)
            {

                case Cus_PowerYuanJiang.H:
                    m_UaValue = U;
                    m_UbValue = U;
                    m_UcValue = U;
                    m_IaValue = I;
                    m_IbValue = I;
                    m_IcValue = I;
                    break;
                case Cus_PowerYuanJiang.A:
                    m_UaValue = U;
                    m_UbValue = U;
                    m_UcValue = U;
                    m_IaValue = I;
                    m_IbValue = 0;
                    m_IcValue = 0;
                    break;
                case Cus_PowerYuanJiang.B:
                    m_UaValue = U;
                    m_UbValue = U;
                    m_UcValue = U;
                    m_IaValue = 0;
                    m_IbValue = I;
                    m_IcValue = 0;
                    break;
                case Cus_PowerYuanJiang.C:
                    m_UaValue = U;
                    m_UbValue = U;
                    m_UcValue = U;
                    m_IaValue = 0;
                    m_IbValue = 0;
                    m_IcValue = I;
                    break;
            }
            //m_UaXwValue = 0;
            //m_UbXwValue = 240;
            //m_UcXwValue = 120;
            //m_IaXwValue = 0;
            //m_IbXwValue = 240;
            //m_IcXwValue = 120;
            //m_FreqValue = 50;

            // 三相四线有功 = 0,
            //三相四线无功 = 1,
            //三相三线有功 = 2,
            //三相三线无功 = 3,
            //二元件跨相90 = 4,
            //二元件跨相60 = 5,
            //三元件跨相90 = 6,

            switch (iClfs)
            {

                case 0:  //三相四线有功表
                    m_UaXwValue = 0;
                    m_UbXwValue = 240;
                    m_UcXwValue = 120;
                    m_IaXwValue = 0;
                    m_IbXwValue = 240;
                    m_IcXwValue = 120;
                    break;
                case 2:  //三相三线有功表
                    m_UbValue = 0;
                    m_UaXwValue = 30;
                    m_UbXwValue = 0;
                    m_UcXwValue = 90;
                    m_IaXwValue = 0;
                    m_IbXwValue = 0;
                    m_IcXwValue = 120;
                    break;
                case 1: //三相四线真无功表(QT4)
                    m_UaXwValue = 0;
                    m_UbXwValue = 240;
                    m_UcXwValue = 120;
                    m_IaXwValue = 270;
                    m_IbXwValue = 150;
                    m_IcXwValue = 30;
                    break;
                case 3: //三相三线真无功表(Q32)
                    m_UbValue = 0;
                    m_UaXwValue = 30;
                    m_UbXwValue = 0;
                    m_UcXwValue = 90;

                    m_IaXwValue = 270;
                    m_IbXwValue = 0;
                    m_IcXwValue = 30;
                    break;
                case 6: //三元件跨相90无功表(Q33)
                    m_UaXwValue = 30;
                    m_UbXwValue = 270;
                    m_UcXwValue = 150;

                    m_IaXwValue = 270;
                    m_IbXwValue = 150;
                    m_IcXwValue = 30;
                    break;
                case 4: //二元件跨相90无功表(Q90)
                    m_UbValue = 0;
                    m_UaXwValue = 30;
                    m_UbXwValue = 0;
                    m_UcXwValue = 270;

                    m_IaXwValue = 270;
                    m_IbXwValue = 0;
                    m_IcXwValue = 30;
                    break;
                case 5: //二元件跨相60无功表(Q60)
                    m_UbValue = 0;
                    m_UaXwValue = 0;
                    m_UbXwValue = 0;
                    m_UcXwValue = 120;

                    m_IaXwValue = 270;
                    m_IbXwValue = 0;
                    m_IcXwValue = 30;
                    break;
                case 7: //单相表
                    m_UbValue = 0;
                    m_UcValue = 0;
                    m_UaXwValue = 0;
                    m_IaXwValue = 0;
                    m_UbXwValue = m_UaXwValue;
                    m_UcXwValue = m_UaXwValue;
                    m_IbXwValue = m_IaXwValue;
                    m_IcXwValue = m_IaXwValue;
                    break;
            }


            SetAcSourcePowerFactor(str_Glys, (byte)iClfs, false);

            SetPara(m_UaValue, m_UbValue, m_UcValue, m_IaValue, m_IbValue, m_IcValue, (float)m_UaXwValue, (float)m_UbXwValue, (float)m_UcXwValue,
            (float)m_IaXwValue, (float)m_IbXwValue, (float)m_IcXwValue, Hz, bDBOut);

        }
        /// <summary>
        /// 设置源功率因数
        /// </summary>
        /// <param name="Glys">功率因数如(0.5L,1.0,-1,-0,0.5C)</param>
        /// <param name="jxfs">0-三相四线有功表PT4;1-三相三线有功表P32;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60)</param>
        /// <returns></returns>
        public bool SetAcSourcePowerFactor(string Glys, byte jxfs, bool PH)
        {
            //jxfs 0-三相四线有功表；1-三相三线有功表;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);
            //4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60);

            double XwUa = 0;
            double XwUb = 0;
            double XwUc = 0;
            double XwIa = 0;
            double XwIb = 0;
            double XwIc = 0;
            int n = 1;
            string strGlys = Glys;

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
            string LC = GetUnit(strGlys);
            double LcValue;
            if (LC.Length > 0)
            {
                LcValue = Convert.ToDouble(strGlys.Replace(LC, ""));
            }
            else
            {
                LcValue = Convert.ToDouble(strGlys);
            }

            double Phi;
            switch (jxfs)
            {
                case 0:  //三相四线有功表
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
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
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;

                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;

                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                case 1:  //三相三线有功表
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 90;
                    if (LcValue > 0)
                    {
                        XwIa = 0;
                        XwIb = 0;
                        XwIc = 120;
                        Phi = 1 * Phi;
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
                        Phi = 1 * Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;
                        XwIb = 0;
                        XwIc = 120 - Phi;

                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (PH == false)
                    {
                        XwIa += 30;
                        XwIc += 30;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                case 3: //三相三线真无功表(Q32)
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 90;

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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (PH == false)
                    {
                        XwIa += 30;
                        XwIc -= 30;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                case 7: //单相表
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
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
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;
                    }
                    XwUb = XwUa;
                    XwUc = XwUa;
                    XwIb = XwIa;
                    XwIc = XwIa;
                    break;
            }
            //tmpOk = SetAcSourcePhasic(XwUa, XwUb, XwUc, XwIa, XwIb, XwIc);
            m_UaXwValue = XwUa;
            m_UbXwValue = XwUb;
            m_UcXwValue = XwUc;

            m_IaXwValue = XwIa;
            m_IbXwValue = XwIb;
            m_IcXwValue = XwIc;

            return true;
        }

        private string GetUnit(string chrVal)  //得到量程的单位 //chrVal带单位的值如 15A
        {
            string cUnit = "";
            //_ = new byte[256];
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] chrbytes = ascii.GetBytes(chrVal);
            for (int i = 0; i < chrbytes.Length; ++i)
            {
                if (chrbytes[i] > 57)
                {
                    cUnit = chrVal.Substring(i);
                    break;
                }

            }
            return cUnit;
        }
        /*
         * 81 30 PCID 09 a0 02 02 40 CS
         */
        protected override byte[] GetBody()
        {//fjk,xiugai
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();

            byte[] byt_Value = new byte[67];

            byt_Value[0] = PAGE5;

            byt_Value[1] = GROUP1 + GROUP2 + GROUP6;

            //GROUP1 
            byt_Value[2] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5; //len = 8          
            #region
            int tmpvalue = (int)(_PhiUc * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 3, 4);

            tmpvalue = (int)(_PhiUb * JIAODU_BEISHU);//
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 7, 4);

            tmpvalue = (int)(_PhiUa * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 11, 4);

            tmpvalue = (int)(_PhiIc * JIAODU_BEISHU);//电流角度 C-B-A
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 15, 4);

            tmpvalue = (int)(_PhiIb * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 19, 4);

            tmpvalue = (int)(_PhiIa * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 23, 4);

            #endregion

            //GROUP2
            byt_Value[27] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5 + DATA6 + DATA7; //len = 33
            #region
            tmpvalue = (int)(_Uc * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 28, 4);
            byt_Value[32] = (byte)Convert.ToSByte(-4);

            tmpvalue = (int)(_Ub * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 33, 4);
            byt_Value[37] = (byte)Convert.ToSByte(-4);

            tmpvalue = (int)(_Ua * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 38, 4);
            byt_Value[42] = (byte)Convert.ToSByte(-4);

            tmpvalue = (int)(_Ic * 1000000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 43, 4);
            byt_Value[47] = (byte)Convert.ToSByte(-6);

            tmpvalue = (int)(_Ib * 1000000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 48, 4);
            byt_Value[52] = (byte)Convert.ToSByte(-6);

            tmpvalue = (int)(_Ia * 1000000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 53, 4);
            byt_Value[57] = (byte)Convert.ToSByte(-6);

            //

            tmpvalue = (int)(_Hz * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 58, 4); ; //
            //是否更新电压
            if (_bIsUpDateVoltage)
            {
                byt_Value[62] = DATA0 + DATA1 + DATA2; //频率更新字
            }
            else
            {
                byt_Value[62] = 0X00;
            }
            #endregion

            //GROUP6
            byt_Value[63] = DATA0 + DATA1 + DATA2;
            #region

            byt_Value[64] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5 + DATA6;//fjk,相位更新控制



            //byt_Value[64] = //
            if (_bIsUpDateVoltage)
            {
                byt_Value[65] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5;// +DATA6; //Convert.ToByte(_iChange);// fjk，幅值更新字
            }
            else
            {
                byt_Value[65] = DATA3 + DATA4 + DATA5;// +DATA6; //Convert.ToByte(_iChange);// fjk，幅值更新字
            }
            if (_bCurrentDB)
            {
                byt_Value[66] = 0x0;//电流对标??????fjk TODO:数据字典不明
            }
            else
            {
                byt_Value[66] = 0x0;//不对标
            }
            #endregion
            buf.Put(0xA3);
            buf.Put(byt_Value);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("关源：{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", _Ua, _Ub, _Uc, _Ia, _Ib, _Ic, _PhiUa, _PhiUb, _PhiUc, _PhiIa, _PhiIb, _PhiIc);
            return strResolve;
        }
    }
    /// <summary>
    /// 关源 返回指令
    /// </summary>
    internal class Cl309_RequestPowerOffReplyPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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
    #endregion

    #region CL309升源指令
    /// <summary>
    /// 控制源 请求包
    /// </summary>
    internal class CL309_RequestPowerOnPacket : CL309SendPacket
    {
        public bool IsLink = true;

        #region //存储 电压电流
        public static double oldUa = 0.00F;
        public static double oldUb = 0.00F;
        public static double oldUc = 0.00F;
        public static double oldIa = 0.00F;
        public static double oldIb = 0.00F;
        public static double oldIc = 0.00F;
        public static double oldPhia = 0.00F;
        public static double oldPhib = 0.00F;
        public static double oldPhic = 0.00F;


        //private const int AskDat = 0xA0;  //
        //private const int WrtDat = 0xA3; //
        //private const int WrtAry = 0xA6;//
        //private const int EchOk = 0x30;	//
        //private const int EchErr = 0x33;	//
        //private const int EchBsy = 0x35; //
        //private const int EchInh = 0x36;	//

        //private const int HeadPos = 0;
        //private const int RxIDPos = 1;
        //private const int TxIDPos = 2;
        //private const int FlenPos = 3;
        //private const int ComdPos = 4;
        //private const int FRAME_ID = 0x81;

        //private const int PagePos = 5;
        //private const int GrpPos = 6;
        //private const int AryPos = 6;
        //private const int Start0Pos = 7;
        //private const int Start1Pos = 8;
        //private const int LenPos = 9;

        //private const int Grp0Pos = 7;
        //private const int Grp1Pos = 8;
        //private const int Grp2Pos = 9;
        //private const int Grp3Pos = 10;
        //private const int Grp4Pos = 11;
        //private const int Grp5Pos = 12;
        //private const int Grp6Pos = 13;
        //private const int Grp7Pos = 14;

        //private const int LOCAL_ID = 0x05;
        //private const int OBJ_ID = 0x01;


        //private const int PAGE0 = 0;
        //private const int PAGE1 = 1;
        //private const int PAGE2 = 2;
        //private const int PAGE3 = 3;
        //private const int PAGE4 = 4;
        private const int PAGE5 = 5;
        //private const int PAGE6 = 6;

        //private const int GROUP0 = 0x01;
        private const int GROUP1 = 0x02;
        private const int GROUP2 = 0x04;
        //private const int GROUP3 = 0x08;
        //private const int GROUP4 = 0x10;
        //private const int GROUP5 = 0x20;
        private const int GROUP6 = 0x40;
        //private const int GROUP7 = 0x80;

        private const int DATA0 = 0x01;
        private const int DATA1 = 0x02;
        private const int DATA2 = 0x04;
        private const int DATA3 = 0x08;
        private const int DATA4 = 0x10;
        private const int DATA5 = 0x20;
        private const int DATA6 = 0x40;
        private const int DATA7 = 0x80;

        //private const int COS_SIN_BEISHU = 10000;//COSIN放大倍数
        private const int JIAODU_BEISHU = 10000;//角度放大倍数
        //private const int PINLV_BEISHU = 10000;//频率放大倍数


        //private double m_UaXwValue = 0;//Ua相位角度
        //private double m_UbXwValue = 240;//Ub相位角度
        //private double m_UcXwValue = 120;//UC相位角度
        //private double m_IaXwValue = 0;//IA相位角度
        //private double m_IbXwValue = 240;
        //private double m_IcXwValue = 120;



        private UIPara m_UIpara;

        private PhiPara m_Phipara;

        private double _Hz;

        private int _iChange = 0x00;

        private bool _bCurrentDB;
        /// <summary>
        /// true 逆相序，false 正相序
        /// </summary>
        private bool _bln_NXX;
        #endregion

        public CL309_RequestPowerOnPacket()
            : base()
        { }


        public void SetPara(UIPara uipara, PhiPara phipara, Cus_PowerYuanJiang HABC, string str_Glys, float Hz, int iClfs,  bool bDBOut,  bool isNXX)
        {
            m_UIpara = uipara;
            m_Phipara = phipara;
            _Hz = Hz;

            _bCurrentDB = bDBOut;
            _bln_NXX = isNXX;

            ////Ia 是否变化， 
            //if (oldIa == uipara.Ia)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                _iChange += DATA5;
                oldIa = uipara.Ia;
            }
            //Ib 是否变化， 
            //if (oldIb == uipara.Ib)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                _iChange += DATA4;
                oldIb = uipara.Ib;
            }
            //Ic 是否变化， 
            //if (oldIc == uipara.Ic)
            //{
            //    _iChange += 0x00;
            //}
            //else
            {
                _iChange += DATA3;
                oldIc = uipara.Ic;
            }

            //Ua 是否变化， 
            if (oldUa == uipara.Ua)
            {
                _iChange += 0x00;
            }
            else
            {
                _iChange += DATA2;
                oldUa = uipara.Ua;
            }
            //Ub 是否变化， 
            if (oldUb == uipara.Ub)
            {
                _iChange += 0x00;
            }
            else
            {
                _iChange += DATA1;
                oldUb = uipara.Ub;
            }
            //Uc 是否变化， 
            if (oldUc == uipara.Uc)
            {
                _iChange += 0x00;
            }
            else
            {
                _iChange += DATA0;
                oldUc = uipara.Uc;
            }
            int iYuanjian = (int)HABC - 1;
            SetAcSourcePowerFactor(str_Glys, (byte)iClfs, true, iYuanjian);
        }

        public void SetPara(UIPara uipara, PhiPara phipara, double Hz)
        {
            m_UIpara = uipara;
            m_Phipara = phipara;
            _Hz = Hz;
        }

        public void SetPara(float U, float I, string str_Glys, float Hz, int iClfs,  bool bDBOut, Cus_PowerYuanJiang HABC)
        {
            //r_Glys = "-1.0";            
            switch (HABC)
            {

                case Cus_PowerYuanJiang.H:
                    m_UIpara.Ua = U;
                    m_UIpara.Ub = U;
                    m_UIpara.Uc = U;
                    m_UIpara.Ia = I;
                    m_UIpara.Ib = I;
                    m_UIpara.Ic = I;
                    break;
                case Cus_PowerYuanJiang.A:
                    m_UIpara.Ua = U;
                    m_UIpara.Ub = U;
                    m_UIpara.Uc = U;
                    m_UIpara.Ia = I;
                    m_UIpara.Ib = 0;
                    m_UIpara.Ic = 0;
                    break;
                case Cus_PowerYuanJiang.B:
                    m_UIpara.Ua = U;
                    m_UIpara.Ub = U;
                    m_UIpara.Uc = U;
                    m_UIpara.Ia = 0;
                    m_UIpara.Ib = I;
                    m_UIpara.Ic = 0;
                    break;
                case Cus_PowerYuanJiang.C:
                    m_UIpara.Ua = U;
                    m_UIpara.Ub = U;
                    m_UIpara.Uc = U;
                    m_UIpara.Ia = 0;
                    m_UIpara.Ib = 0;
                    m_UIpara.Ic = I;
                    break;
            }
            //m_UaXwValue = 0;
            //m_UbXwValue = 240;
            //m_UcXwValue = 120;
            //m_IaXwValue = 0;
            //m_IbXwValue = 240;
            //m_IcXwValue = 120;
            //m_FreqValue = 50;

            // 三相四线有功 = 0,
            //三相四线无功 = 1,
            //三相三线有功 = 2,
            //三相三线无功 = 3,
            //二元件跨相90 = 4,
            //二元件跨相60 = 5,
            //三元件跨相90 = 6,

            switch (iClfs)
            {

                case 0:  //三相四线有功表
                    m_Phipara.PhiUa = 0;
                    m_Phipara.PhiUb = 240;
                    m_Phipara.PhiUc = 120;
                    m_Phipara.PhiIa = 0;
                    m_Phipara.PhiIb = 240;
                    m_Phipara.PhiIc = 120;
                    break;
                case 2:  //三相三线有功表
                    m_UIpara.Ub = 0;
                    m_Phipara.PhiUa = 30;
                    m_Phipara.PhiUb = 0;
                    m_Phipara.PhiUc = 90;
                    m_Phipara.PhiIa = 0;
                    m_Phipara.PhiIb = 0;
                    m_Phipara.PhiIc = 120;
                    break;
                case 1: //三相四线真无功表(QT4)
                    m_Phipara.PhiUa = 0;
                    m_Phipara.PhiUb = 240;
                    m_Phipara.PhiUc = 120;
                    m_Phipara.PhiIa = 270;
                    m_Phipara.PhiIb = 150;
                    m_Phipara.PhiIc = 30;
                    break;
                case 3: //三相三线真无功表(Q32)
                    m_UIpara.Ub = 0;
                    m_Phipara.PhiUa = 30;
                    m_Phipara.PhiUb = 0;
                    m_Phipara.PhiUc = 90;

                    m_Phipara.PhiIa = 270;
                    m_Phipara.PhiIb = 0;
                    m_Phipara.PhiIc = 30;
                    break;
                case 6: //三元件跨相90无功表(Q33)
                    m_Phipara.PhiUa = 30;
                    m_Phipara.PhiUb = 270;
                    m_Phipara.PhiUc = 150;

                    m_Phipara.PhiIa = 270;
                    m_Phipara.PhiIb = 150;
                    m_Phipara.PhiIc = 30;
                    break;
                case 4: //二元件跨相90无功表(Q90)
                    m_UIpara.Ub = 0;
                    m_Phipara.PhiUa = 30;
                    m_Phipara.PhiUb = 0;
                    m_Phipara.PhiUc = 270;

                    m_Phipara.PhiIa = 270;
                    m_Phipara.PhiIb = 0;
                    m_Phipara.PhiIc = 30;
                    break;
                case 5: //二元件跨相60无功表(Q60)
                    m_UIpara.Ub = 0;
                    m_Phipara.PhiUa = 0;
                    m_Phipara.PhiUb = 0;
                    m_Phipara.PhiUc = 120;

                    m_Phipara.PhiIa = 270;
                    m_Phipara.PhiIb = 0;
                    m_Phipara.PhiIc = 30;
                    break;
                case 7: //单相表
                    m_UIpara.Ub = 0;
                    m_UIpara.Uc = 0;
                    m_Phipara.PhiUa = 0;
                    m_Phipara.PhiIa = 0;
                    m_Phipara.PhiUb = m_Phipara.PhiUa;
                    m_Phipara.PhiUc = m_Phipara.PhiUa;
                    m_Phipara.PhiIb = m_Phipara.PhiIa;
                    m_Phipara.PhiIc = m_Phipara.PhiIa;
                    break;
            }

            int iYuanjian = (int)HABC - 1;
            SetAcSourcePowerFactor(str_Glys, (byte)iClfs, false, iYuanjian);

            SetPara(m_UIpara, m_Phipara, HABC, str_Glys, Hz, iClfs,  bDBOut,  false);

        }
        /// <summary>
        /// 设置源功率因数
        /// </summary>
        /// <param name="Glys">功率因数如(0.5L,1.0,-1,-0,0.5C)</param>
        /// <param name="jxfs">0-三相四线有功表PT4;1-三相三线有功表P32;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60)</param>
        /// <returns></returns>
        public bool SetAcSourcePowerFactor(string Glys, byte jxfs, bool PH, int iYuanjian)
        {
            //jxfs 0-三相四线有功表；1-三相三线有功表;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);
            //4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60);
            #region
            double XwUa = 0;
            double XwUb = 0;
            double XwUc = 0;
            double XwIa = 0;
            double XwIb = 0;
            double XwIc = 0;
            int n = 1;

            string strGlys = Glys;

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
            string LC = GetUnit(strGlys);
            double LcValue;
            if (LC.Length > 0)
            {
                LcValue = Convert.ToDouble(strGlys.Replace(LC, ""));
            }
            else
            {
                LcValue = Convert.ToDouble(strGlys);
            }

            double Phi;
            switch (jxfs)
            {
                case 0:  //三相四线有功表
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
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
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;

                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;

                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                case 1:  //三相三线有功表
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
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
                            if (XwIa < 0) XwIa += 360;
                            if (XwIa >= 360) XwIa -= 360;
                            XwIb = 240 - Phi;
                            if (XwIb < 0) XwIb += 360;
                            if (XwIb >= 360) XwIb -= 360;
                            XwIc = 120 - Phi;
                            if (XwIc < 0) XwIc += 360;
                            if (XwIc >= 360) XwIc -= 360;
                        }
                        else if (iYuanjian == 1)
                        {
                            Phi = 1 * Phi;
                            XwIa = 330;
                            if (XwIa < 0) XwIa += 360;
                            if (XwIa >= 360) XwIa -= 360;
                            XwIb = 0;
                            if (XwIb < 0) XwIb += 360;
                            if (XwIb >= 360) XwIb -= 360;
                            XwIc = 0;
                            if (XwIc < 0) XwIc += 360;
                            if (XwIc >= 360) XwIc -= 360;
                        }
                        else if (iYuanjian == 3)
                        {
                            Phi = 1 * Phi;
                            XwIa = 0;
                            if (XwIa < 0) XwIa += 360;
                            if (XwIa >= 360) XwIa -= 360;
                            XwIb = 0;
                            if (XwIb < 0) XwIb += 360;
                            if (XwIb >= 360) XwIb -= 360;
                            XwIc = 90 - Phi;
                            if (XwIc < 0) XwIc += 360;
                            if (XwIc >= 360) XwIc -= 360;
                        }
                    }
                    if (LC == "C")
                    {
                        if (iYuanjian == 0)
                        {
                            Phi = -1 * Phi;
                            XwIa = 0 - Phi - 6;
                            if (XwIa < 0) XwIa += 360;
                            if (XwIa >= 360) XwIa -= 360;

                            XwIb = 240 - Phi - 6;
                            if (XwIb < 0) XwIb += 360;
                            if (XwIb >= 360) XwIb -= 360;

                            XwIc = 120 - Phi - 6;
                            if (XwIc < 0) XwIc += 360;
                            if (XwIc >= 360) XwIc -= 360;
                        }
                        else if (iYuanjian == 1)
                        {
                            Phi = -1 * Phi;
                            XwIa = 60;
                            if (XwIa < 0) XwIa += 360;
                            if (XwIa >= 360) XwIa -= 360;

                            XwIb = 240 - Phi - 6;
                            if (XwIb < 0) XwIb += 360;
                            if (XwIb >= 360) XwIb -= 360;

                            XwIc = 120 - Phi - 6;
                            if (XwIc < 0) XwIc += 360;
                            if (XwIc >= 360) XwIc -= 360;
                        }
                        else if (iYuanjian == 3)
                        {
                            Phi = -1 * Phi;
                            XwIa = 0 - Phi - 6;
                            if (XwIa < 0) XwIa += 360;
                            if (XwIa >= 360) XwIa -= 360;

                            XwIb = 240 - Phi - 6;
                            if (XwIb < 0) XwIb += 360;
                            if (XwIb >= 360) XwIb -= 360;

                            XwIc = 120;
                            if (XwIc < 0) XwIc += 360;
                            if (XwIc >= 360) XwIc -= 360;
                        }
                    }
                    if (PH == false)
                    {
                        XwIa += 30;
                        XwIc += 30;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
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
                                if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = Phi;
                                XwIa = 0 - Phi;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 120 - Phi;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case 1:
                                if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = Phi;
                                XwIa = 30 - Phi;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 120 - Phi;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case 3:
                                if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = Phi;
                                XwIa = 0 - Phi;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 90 - Phi;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                        }
                    }
                    if (LC == "C")
                    {
                        switch (iYuanjian)
                        {
                            case 0:
                                if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = n + Phi;
                                XwIa = 0 - Phi - 6;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 120 - Phi - 6;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case 1:
                                if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = n + Phi;
                                XwIa = 330;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 120 - Phi - 6;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case 3:
                                if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = n + Phi;
                                XwIa = 0 - Phi - 6;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 330;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                        }
                    }
                    if (PH == false)
                    {
                        XwIa += 30;
                        XwIc -= 30;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
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
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                case 7: //单相表
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
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
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;
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
        ///// <summary>
        ///// 计算角度 分相计算
        ///// </summary>
        ///// <param name="int_Clfs">测量方式</param>
        ///// <param name="str_Glys">功率因数</param>
        ///// <param name="bln_NXX">逆相序</param>
        ///// <returns>返回数组，数组元素为各相ABC相电压电流角度</returns>
        //private Single[] GetPhiGlys(int int_Clfs, string str_Glys, int int_Element, bool bln_NXX)
        //{

        //    string str_CL = str_Glys.ToUpper().Substring(str_Glys.Length - 1, 1);
        //    Double dbl_XS = 0;
        //    if (str_CL == "C" || str_CL == "L")
        //        dbl_XS = Convert.ToDouble(str_Glys.Substring(0, str_Glys.Length - 1));
        //    else
        //        dbl_XS = Convert.ToDouble(str_Glys);
        //    Double dbl_Phase;

        //    if (int_Clfs == 1 || int_Clfs == 3 || int_Clfs == 6)
        //        dbl_Phase = Math.Asin(Math.Abs(dbl_XS));                              //无功计算
        //    else
        //        dbl_Phase = Math.Acos(Math.Abs(dbl_XS));                              //有功计算

        //    dbl_Phase = dbl_Phase * 180 / Math.PI;      //角度换算
        //    if (dbl_XS < 0) dbl_Phase = 180 + dbl_Phase;         //反向
        //    if (str_CL == "C") dbl_Phase = 360 - dbl_Phase;
        //    if (dbl_Phase < 0) dbl_Phase = 360 + dbl_Phase;

        //    Single sng_UIPhi = Convert.ToSingle(dbl_Phase);
        //    Single[] sng_Phi = new Single[6];

        //    if (bln_NXX)
        //    {
        //        sng_Phi[0] = 0;         //Ua
        //        sng_Phi[1] = 240;       //Ub
        //        sng_Phi[2] = 120;       //Uc
        //    }
        //    else
        //    {
        //        sng_Phi[0] = 0;         //Ua
        //        sng_Phi[1] = 120;       //Ub
        //        sng_Phi[2] = 240;       //Uc
        //    }


        //    sng_Phi[3] = sng_Phi[0] + sng_UIPhi;       //Ia
        //    sng_Phi[4] = sng_Phi[1] + sng_UIPhi;       //Ib
        //    sng_Phi[5] = sng_Phi[2] + sng_UIPhi;       //Ic

        //    if (int_Clfs == 2 || int_Clfs == 3)
        //    {
        //        sng_Phi[2] += 60;       //Uc
        //        sng_Phi[3] += 30;       //Ia
        //        sng_Phi[4] += 30;       //Ib
        //        sng_Phi[5] += 30;       //Ic
        //    }

        //    sng_Phi[3] %= 360;       //Ia
        //    sng_Phi[4] %= 360;       //Ib
        //    sng_Phi[5] %= 360;



        //    //0, 240, 120, 0, 240, 120
        //    //0, 240, 120, 180, 60, 300
        //    //0, 240, 120, 30, 270, 150
        //    //0, 240, 120, 210, 90, 330,

        //    return sng_Phi;
        //}

        private string GetUnit(string chrVal)  //得到量程的单位 //chrVal带单位的值如 15A
        {
            string cUnit = "";
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] chrbytes = ascii.GetBytes(chrVal);
            for (int i = 0; i < chrbytes.Length; ++i)
            {
                if (chrbytes[i] > 57)
                {
                    cUnit = chrVal.Substring(i);
                    break;
                }

            }
            return cUnit;
        }
        /*
         * 81 30 PCID 09 a0 02 02 40 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();

            byte[] byt_Value = new byte[67];

            byt_Value[0] = PAGE5;

            byt_Value[1] = GROUP1 + GROUP2 + GROUP6;

            //GROUP1
            byt_Value[2] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5; //len = 8          
            #region
            int tmpvalue = (int)(m_Phipara.PhiUc * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 3, 4);

            tmpvalue = (int)(m_Phipara.PhiUb * JIAODU_BEISHU);//
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 7, 4);

            tmpvalue = (int)(m_Phipara.PhiUa * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 11, 4);

            tmpvalue = (int)(m_Phipara.PhiIc * JIAODU_BEISHU);//电流角度 C-B-A
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 15, 4);

            tmpvalue = (int)(m_Phipara.PhiIb * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 19, 4);

            tmpvalue = (int)(m_Phipara.PhiIa * JIAODU_BEISHU);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 23, 4);

            #endregion

            //GROUP2
            byt_Value[27] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5 + DATA6 + DATA7; //len = 33
            #region
            tmpvalue = (int)(m_UIpara.Uc * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 28, 4);
            byt_Value[32] = (byte)Convert.ToSByte(-4);

            tmpvalue = (int)(m_UIpara.Ub * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 33, 4);
            byt_Value[37] = (byte)Convert.ToSByte(-4);

            tmpvalue = (int)(m_UIpara.Ua * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 38, 4);
            byt_Value[42] = (byte)Convert.ToSByte(-4);

            tmpvalue = (int)(m_UIpara.Ic * 1000000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 43, 4);
            byt_Value[47] = (byte)Convert.ToSByte(-6);

            tmpvalue = (int)(m_UIpara.Ib * 1000000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 48, 4);
            byt_Value[52] = (byte)Convert.ToSByte(-6);

            tmpvalue = (int)(m_UIpara.Ia * 1000000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 53, 4);
            byt_Value[57] = (byte)Convert.ToSByte(-6);


            tmpvalue = (int)(_Hz * 10000);
            Array.Copy(BitConverter.GetBytes(tmpvalue), 0, byt_Value, 58, 4); ; //

            byt_Value[62] = DATA0 + DATA1 + DATA2;
            #endregion

            //Group6
            byt_Value[63] = DATA0 + DATA1 + DATA2; //
            #region
            byt_Value[64] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5;// + DATA6;//

            //byt_Value[62] = DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5 + DATA6;
            //byt_Value[64] = //
            byt_Value[65] = Convert.ToByte(_iChange);// DATA0 + DATA1 + DATA2 + DATA3 + DATA4 + DATA5;// +DATA6; //

            if (_bCurrentDB)
            {
                byt_Value[66] = 0;//电流对标
            }
            else
            {
                byt_Value[66] = 0x0;//不对标
            }
            #endregion

            buf.Put(0xA3);
            buf.Put(byt_Value);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("升源：{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", m_UIpara.Ua, m_UIpara.Ub, m_UIpara.Uc, m_UIpara.Ia, m_UIpara.Ib, m_UIpara.Ic, m_Phipara.PhiUa, m_Phipara.PhiUb, m_Phipara.PhiUc, m_Phipara.PhiIa, m_Phipara.PhiIb, m_Phipara.PhiIc);
            return strResolve;
        }
    }
    /// <summary>
    /// 升源 返回指令
    /// </summary>
    internal class Cl309_RequestPowerOnReplyPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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
    #endregion

    #region CL309谐波总开关
    internal class CL309_RequestPowerXBZongSwitchPacket : CL309SendPacket
    {
        #region 私有
        /// <summary>
        /// 写数组
        /// </summary>
        private readonly byte Cmd = 0xA3;
        /// <summary>
        /// 页
        /// </summary>
        private readonly byte Page = 0x05;
        /// <summary>
        /// 组,8bit每位一组0-7
        /// </summary>
        private readonly byte Grp = 0x20;
        /// <summary>
        /// 一组中的有效Data，0-7
        /// </summary>
        private readonly byte DataX = 0x7F;
        /// <summary>
        /// 0关，1开
        /// </summary>
        private readonly byte _xbSwitch;
        #endregion
        /// <summary>
        /// 谐波开关
        /// </summary>
        /// <param name="xbSwitch">0关，1开</param>
        public CL309_RequestPowerXBZongSwitchPacket(byte xbSwitch)
            : base()
        {
            _xbSwitch = 0x00 == xbSwitch ? (byte)0x00 : (byte)0xFF;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(Cmd);
            buf.Put(Page);
            buf.Put(Grp);
            buf.Put(DataX);

            byte[] byt_Data = new byte[] { (byte)(_xbSwitch | 0x01), _xbSwitch, _xbSwitch, _xbSwitch };
            buf.Put(byt_Data);
            buf.Put(byt_Data);
            buf.Put(byt_Data);
            buf.Put(byt_Data);
            buf.Put(byt_Data);
            buf.Put(byt_Data);
            buf.Put(0x7F);

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("谐波开关字：{0}", _xbSwitch);
            return strResolve;
        }

    }
    internal class CL309_RequestPowerXBZongSwitchReplyPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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
    #endregion

    #region CL309谐波分相开关

    internal class CL309_RequestPowerXBFenXiangSwitchPacket : CL309SendPacket
    {
        #region 私有
        /// <summary>
        /// 写数组
        /// </summary>
        private readonly byte Cmd = 0xA3;
        /// <summary>
        /// 页
        /// </summary>
        private readonly byte Page = 0x05;
        /// <summary>
        /// 组,8bit每位一组0-7
        /// </summary>
        private readonly byte Grp = 0x20;
        /// <summary>
        /// 一组中的有效Data，0-7
        /// </summary>
        private byte DataX = 0x00;
        /// <summary>
        /// 0关，1开
        /// </summary>
        private int[] _int_xbSwitch;
        #endregion
        public CL309_RequestPowerXBFenXiangSwitchPacket()
            : base()
        {
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(Cmd);
            buf.Put(Page);
            buf.Put(Grp);
            buf.Put(DataX);

            byte[] byt_Data = GetbitData(_int_xbSwitch);

            buf.Put(byt_Data);

            return buf.ToByteArray();
        }

        private byte[] GetbitData(int[] Datas)
        {
            List<byte> listData = new List<byte>();
            if (Datas.Length >= 32)
            {
                string strTmp = string.Empty;
                for (int i = 0; i < 32; i++)
                {
                    strTmp = Datas[i].ToString() + strTmp;
                    if (strTmp.Length == 8)
                    {
                        byte byteData = Convert.ToByte(strTmp, 2);
                        listData.Add(byteData);

                        strTmp = string.Empty;

                    }
                }
            }

            return listData.ToArray();
        }

        public void SetPara(byte UIType, int[] int_xbSwitch)
        {
            _int_xbSwitch = int_xbSwitch; //应该为32位的数组

            //相别A相电压 = 0,B相电压 = 1,C相电压 = 2,A相电流 = 3,B相电流 = 4,C相电流 = 5
            switch (UIType)
            {
                case 0:
                    {
                        DataX = 0x02;
                    }
                    break;
                case 1:
                    {
                        DataX = 0x01;
                    }
                    break;
                case 2:
                    {
                        DataX = 0x00;
                    }
                    break;
                case 3:
                    {
                        DataX = 0x05;
                    }
                    break;
                case 4:
                    {
                        DataX = 0x04;
                    }
                    break;
                case 5:
                    {
                        DataX = 0x03;
                    }
                    break;
            }
        }
    }

    internal class CL309_RequestPowerXBFenXiangSwitchReplayPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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

    #endregion

    #region CL309谐波设置 幅值和相位,fjk
    /// <summary>
    /// 谐波设置 发送
    /// </summary>
    internal class CL309_RequestPowerXieBoPacket : CL309SendPacket
    {
        #region 私有
        /// <summary>
        /// 写数组
        /// </summary>
        private readonly byte Cmd = 0xA6;
        /// <summary>
        /// 页
        /// </summary>
        private readonly byte Page = 0x05;
        /// <summary>
        /// 页中的组，一字节8位"00GG G???"，"GG G" Group，"???" Data,
        /// </summary>
        private static byte Ary;
        private const byte AryUa = 0x02;
        private const byte AryUb = 0x01;
        private const byte AryUc = 0x00;
        private const byte AryIa = 0x05;
        private const byte AryIb = 0x04;
        private const byte AryIc = 0x03;
        private const byte AryPhi = 0x08;
        ///// <summary>
        ///// 当前加谐波的元件
        ///// A相电压 = 0,
        ///// B相电压 = 1,
        ///// C相电压 = 2,
        ///// A相电流 = 3,
        ///// B相电流 = 4,
        ///// C相电流 = 5
        ///// </summary>
        //private byte _UIType;
        /// <summary>
        /// 1幅值，0相位
        /// </summary>
        private byte _UIorPhi;
        /// <summary>
        /// 幅值，INT4E1，[0]=基波100%
        /// </summary>
        private float[] _sng_Value = new float[64];
        /// <summary>
        /// 相位，SINT4，[0]=基波相位
        /// </summary>
        private float[] _sng_Phase = new float[64];
        /// <summary>
        /// 需要设置的最大次数
        /// </summary>
        private int _CLen;
        #endregion
        /// <summary>
        /// 数组的起始下标
        /// </summary>
        public byte start0 = 0x00;
        /// <summary>
        /// 数组的起始下标
        /// </summary>
        public byte start1 = 0x00;

        public CL309_RequestPowerXieBoPacket()
            : base()
        { }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="UIType">A相电压 = 0,B相电压 = 1,C相电压 = 2,A相电流 = 3,B相电流 = 4,C相电流 = 5</param>
        /// <param name="UIorPhi">1幅值，0相位</param>
        /// <param name="sng_Value">各次谐波幅值或相位，含基波</param>
        public void SetPara(byte UIType, byte UIorPhi, float[] sng_Value)
        {
            //_UIType = UIType;
            _UIorPhi = UIorPhi;
            #region 字典顺序正好反了
            if (0x01 == UIorPhi)//幅值，
            {
                if (0x02 == UIType)
                {
                    Ary = AryUc;
                }
                else if (0x00 == UIType)
                {
                    Ary = AryUa;
                }
                else if (0x05 == UIType)
                {
                    Ary = AryIc;
                }
                else if (0x03 == UIType)
                {
                    Ary = AryIa;
                }
                else if (0x01 == UIType)
                {
                    Ary = AryUb;
                }
                else if (0x04 == UIType)
                {
                    Ary = AryIb;
                }
            }
            else
            {
                if (0x02 == UIType)
                {
                    Ary = (byte)(0x00 + 0x08);
                }
                else if (0x00 == UIType)
                {
                    Ary = (byte)(0x02 + 0x08);
                }
                else if (0x05 == UIType)
                {
                    Ary = (byte)(0x03 + 0x08);
                }
                else if (0x03 == UIType)
                {
                    Ary = (byte)(0x05 + 0x08);
                }
                else if (0x01 == UIType)
                {
                    Ary = (AryUb + AryPhi);
                }
                else if (0x04 == UIType)
                {
                    Ary = AryIb + AryPhi;
                }

            }
            #endregion
            _CLen = sng_Value.Length;
            _sng_Value = sng_Value;
            _sng_Phase = sng_Value;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(Cmd);
            buf.Put(Page);
            buf.Put(Ary);
            buf.Put(start0);
            buf.Put(start1);

            byte[] byt_Data;
            byte Len;
            if (0x01 == _UIorPhi)//幅值
            {
                _CLen = _sng_Value.Length;
                Len = (byte)(_CLen > 32 ? 32 * 5 : _CLen * 5);
                byt_Data = new byte[Len];
                for (int i = 0; i < _CLen && i < 32; i++)//最高到32次
                {
                    Array.Copy(BitConverter.GetBytes((int)(_sng_Value[i] * 10000)), 0, byt_Data, 5 * i, 4);
                    byt_Data[5 * i + 4] = (byte)Convert.ToSByte(-4);
                }
            }
            else//相位
            {
                _CLen = _sng_Phase.Length;
                Len = (byte)(_CLen > 32 ? 32 * 4 : _CLen * 4);
                byt_Data = new byte[Len];
                for (int i = 0; i < _CLen && i < 32; i++)//最高到32次
                {
                    Array.Copy(BitConverter.GetBytes((int)(_sng_Phase[i] * 10000)), 0, byt_Data, 4 * i, 4);
                }
            }

            buf.Put(Len);
            buf.Put(byt_Data);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = string.Format("谐波幅值、相位：{0}", "设置");
            return strResolve;
        }

    }
    /// <summary>
    /// 谐波设置 返回
    /// </summary>
    internal class CL309_RequestPowerXieBoReplyPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region CL309读取过载信息
    /// <summary>
    /// 读取源过载信息请求包
    /// </summary>
    internal class CL309_RequestReadPowerOverInfoPacket : CL309SendPacket
    {
        public bool IsLink = true;

        public CL309_RequestReadPowerOverInfoPacket()
            : base()
        { }

        /*
         * 81 01 PCID 08 a0 02 00 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xC9);  //命令           
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 源联机 返回指令
    /// </summary>
    internal class CL309_RequestReadPowerOverInfoReplyPacket : CL309RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 36)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region CL309 读取源版本号
    /// <summary>
    /// 读取源版本 请求包
    /// </summary>
    internal class CL309_RequestReadVerPacket : CL309SendPacket
    {
        public bool IsLink = true;

        public CL309_RequestReadVerPacket()
            : base()
        { }

        /*
         * 81 01 PCID 06 C9 00 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xC9);  //命令           
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取源版本号";
            return strResolve;
        }
    }

    /// <summary>
    /// 读取CL309版本号返回包
    /// </summary>
    internal class CL309_RequestReadVersionReplayPacket : CL309RecvPacket
    {
        public CL309_RequestReadVersionReplayPacket() : base() { }

        /// <summary>
        /// 读取到的版本号
        /// 默认值为Unknown
        /// </summary>
        public string Version { get; private set; }


        protected override void ParseBody(byte[] data)
        {
            Version = ASCIIEncoding.UTF8.GetString(data);
        }
        public override string GetPacketResolving()
        {
            string strResolve = "源版本号：" + Version;
            return strResolve;
        }
    }
    #endregion

    #region CL309更新源
    /// <summary>
    /// 更新源 请求包
    /// </summary>
    internal class CL309_RequestUpdateUIPacket : CL309SendPacket
    {
        public bool IsLink = true;

        public CL309_RequestUpdateUIPacket()
            : base()
        { }

        /*
         * 81 01 PCID 06 C9 00 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);  //命令           
            buf.Put(0x05);
            buf.Put(0x44);
            buf.Put(0x80);
            buf.Put(0x07);
            buf.Put(0x0B);
            buf.Put(0x3F);
            buf.Put(0x3F);
            buf.Put(0x01);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "更新源";
            return strResolve;
        }
    }
    /// <summary>
    /// 更新309源返回包
    /// </summary>
    internal class CL309_RequestUpdateUIPacketReplayPacket : CL309RecvPacket
    {
        public CL309_RequestUpdateUIPacketReplayPacket() : base() { }
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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
    #endregion

    #region CL309电压跌落、逐渐
    /// <summary>
    /// 更新源 请求包
    /// 试验项目标志：
    /// BIT0:电压跌落和短时中断
    /// BIT1:电压逐渐变化
    /// BIT2:逐渐关机和启动
    /// 0：无效 1：启动项目
    /// 注意：启动该试验之前，必须已经按要求输出功率源
    /// </summary>
    internal class CL309_RequestFallOrStepUPacket : CL309SendPacket
    {
        public bool IsLink = true;
        private readonly byte byt_FallorStep = 0;
        private readonly string str_TypeName = "";
        /// <summary>
        /// 0:电压跌落和短时中断,1:电压逐渐变化,2:逐渐关机和启动
        /// </summary>
        /// <param name="UType"></param>
        public CL309_RequestFallOrStepUPacket(int UType)
            : base()
        {
            if (UType == 0)
            {
                byt_FallorStep = 1;
                str_TypeName = "电压跌落和短时中断";
            }
            else if (UType == 1)
            {
                byt_FallorStep = 2;
                str_TypeName = "电压逐渐变化";
            }
            else if (UType == 2)
            {
                byt_FallorStep = 4;
                str_TypeName = "逐渐关机和启动";
            }
            else
            {
                byt_FallorStep = 00;
                str_TypeName = "停止或无效的类型。";
            }
        }

        /*
         * 81 01 PCID 06 C9 00 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);  //命令           
            buf.Put(0x05);
            buf.Put(0x01);
            buf.Put(0x80);
            buf.Put(byt_FallorStep);
            buf.Put(0x01);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "源特殊控制:" + str_TypeName;
            return strResolve;
        }
    }
    /// <summary>
    /// 更新309源返回包
    /// </summary>
    internal class CL309_RequestFallOrStepUPacketReplayPacket : CL309RecvPacket
    {
        public CL309_RequestFallOrStepUPacketReplayPacket() : base() { }
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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
    #endregion

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
            CL188LinkOk = 0x36,
            /// <summary>
            /// 标准表脱机成功
            /// </summary>
            Cl311UnLinkOk = 0x37
        }
    }
    #endregion


    internal class ClearOLPacket : CL309SendPacket
    {
        public ClearOLPacket()
            : base()
        { }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(new byte[] { 0xA3, 0x02, 0x01, 0x80, 0x00 });
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            return "";
        }
    }

    internal class ClearOLReplayPacket : CL309RecvPacket
    {
        public ClearOLReplayPacket() : base() { }
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
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

}
