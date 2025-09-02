using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CLOU.SocketModule.Packet;
using CLOU;
using CLOU.Enum;
using CLOU.Struct;

namespace E_CL3115
{
    #region CL3115标准表联机指令
    /// <summary>
    /// 标准表联机/脱机请求包
    /// </summary>
    internal class CL3115_RequestLinkPacket : CL3115SendPacket
    {
        public bool IsLink = true;

        public CL3115_RequestLinkPacket()
            : base()
        { }

        /*
         * 81 30 PCID 09 a0 02 02 40 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x02);
            buf.Put(0x40);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "联机标准表。";
            return strResolve;
        }
    }
    /// <summary>
    /// 标准表，联机返回指令
    /// </summary>
    internal class CL3115_RequestLinkReplyPacket : CL3115RecvPacket
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
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion

    #region CL3115标准表读版本指令
    /// <summary>
    /// 标准表读版本请求包
    /// </summary>
    internal class CL3115_RequestReadVersionPacket : CL3115SendPacket
    {
        public bool IsLink = true;

        public CL3115_RequestReadVersionPacket()
            : base()
        { }

        /*
         * 81 30 PCID 09 a0 02 02 40 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xC9);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "读版本。";
            return strResolve;
        }
    }
    /// <summary>
    /// 标准表，版本返回指令
    /// </summary>
    internal class CL3115_RequestReadVersionReplyPacket : CL3115RecvPacket
    {
        public string CLTVer
        {
            get;
            private set;
        }
        public string DeviceType
        {
            get;
            private set;
        }
        public string VerNo
        {
            get;
            private set;
        }
        public string SerialNo
        {
            get;
            private set;
        }
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x39)
                {
                    if (data.Length >= 36)
                    {
                        ReciveResult = RecvResult.OK;
                        ByteBuffer buf = new ByteBuffer(data);
                        buf.Get();
                        CLTVer = ASCIIEncoding.UTF8.GetString(buf.GetByteArray(7)).Replace("\0", " ");
                        DeviceType = ASCIIEncoding.UTF8.GetString(buf.GetByteArray(11)).Replace("\0", " ");
                        VerNo = ASCIIEncoding.UTF8.GetString(buf.GetByteArray(5)).Replace("\0", " ");
                        SerialNo = ASCIIEncoding.UTF8.GetString(buf.GetByteArray(12)).Replace("\0", " ");
                    }
                    else
                    {
                        ReciveResult = RecvResult.DataError;
                    }
                }
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

    #region CL3115读取标准表常数
    /// <summary>
    /// 读取真实本机常数
    /// </summary>
    internal class CL3115_RequestReadStdMeterConstPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdMeterConstPacket()
            : base()
        { }

        /*
         * 81 30 PCID 09 a0 02 02 40 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x02);
            buf.Put(0x40);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取标准表常数。";
            return strResolve;
        }
    }
    /// <summary>
    /// 读取真实本机常数返回包
    /// </summary>
    internal class CL3115_RequestReadStdMeterConstReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestReadStdMeterConstReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdMeterConstReplayPacket";
        }
        /// <summary>
        /// 本机常数
        /// </summary>
        /// <returns></returns>
        public int meterConst { get; private set; }

        protected override void ParseBody(byte[] data)
        {
            if (data.Length != 0x08) return;
            ByteBuffer buf = new ByteBuffer(data);

            //去掉 命令字 50
            buf.Get();

            //去掉0x02
            buf.Get();
            //去掉0x02
            buf.Get();
            //去掉0x40
            buf.Get();

            //表常数
            meterConst = buf.GetInt_S();
            ReciveResult = RecvResult.OK;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + meterConst.ToString();
            return strResolve;
        }
    }
    #endregion

    #region CL3115读取标准表信息
    /// <summary>
    /// 读取标准表信息
    /// </summary>
    internal class CL3115_RequestReadStdInfoPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdInfoPacket()
            : base()
        { }

        /*
         * 81 30 PCID 0e a0 02 3f ff 80 3f ff ff 0f CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x3F);
            buf.Put(0xFF);
            //buf.Put(0x80);
            buf.Put(0xFF);
            buf.Put(0x3F);
            buf.Put(0xFF);
            buf.Put(0xFF);
            buf.Put(0x0F);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取标准表信息。";
            return strResolve;
        }
    }

    /// <summary>
    /// 读取标准表信息返回包
    /// </summary>
    internal class CL3115_RequestReadStdInfoReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestReadStdInfoReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdInfoReplayPacket";
        }
        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>
        public stStdInfo PowerInfo 
        { 
            get; 
            set; 
        }

        protected override void ParseBody(byte[] data)
        {
            stStdInfo tagInfo = new stStdInfo();
            ByteBuffer buf = new ByteBuffer(data);
            if (buf.Length != 0xAE) return;
            int[] arrDot = new int[9];

            //去掉 命令字
            buf.Get();

            //去掉0x02
            buf.Get();
            //去掉0x3f
            buf.Get();
            //去掉0xff
            buf.Get();

            //电压电流
            tagInfo.Uc = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Ub = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Ua = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Ic = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Ib = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Ia = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            //频率
            tagInfo.Freq = BitConverter.ToInt32(buf.GetByteArray(4), 0) / 100000F;
            //过载标志
            buf.Get();
            //0x80
            buf.Get();
            //档位
            tagInfo.Scale_Uc = buf.Get();
            tagInfo.Scale_Ub = buf.Get();
            tagInfo.Scale_Ua = buf.Get();
            tagInfo.Scale_Ic = buf.Get();
            tagInfo.Scale_Ib = buf.Get();
            tagInfo.Scale_Ia = buf.Get();
            //真实本机常数
            buf.GetByteArray(4);
            //功率相角
            //buf.GetByteArray(4);
            tagInfo.SAngle = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            //0x3f
            buf.Get();
            //相位
            tagInfo.Phi_Uc = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.Phi_Ub = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.Phi_Ua = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.Phi_Ic = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.Phi_Ib = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.Phi_Ia = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            //0xff
            buf.Get();
            //C相 B相 A相 相角
            tagInfo.PhiAngle_C = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.PhiAngle_B = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.PhiAngle_A = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            //C相 B相 A相 有功功率因素
            tagInfo.PowerFactor_C = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.PowerFactor_B = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            tagInfo.PowerFactor_A = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            //active powerfactor
            tagInfo.COS = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            //reactive powerfactor
            tagInfo.SIN = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F);
            //0xff
            buf.Get();
            //
            tagInfo.Pc = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Pb = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Pa = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.P = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Qc = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Qb = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Qa = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Q = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            //0x0f
            buf.Get();
            //apparent power
            tagInfo.Sc = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Sb = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.Sa = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
            tagInfo.S = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArray(4), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));

            PowerInfo = tagInfo;
            this.ReciveResult = RecvResult.OK;
        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("返回：{0}V,{1}V,{2}V,{3}A,{4}A,{5}A,{6},{7},{8},{9},{10},{11},{12}W,{13}Var,{14}VA", PowerInfo.Ua, PowerInfo.Ub, PowerInfo.Uc, PowerInfo.Ia, PowerInfo.Ib, PowerInfo.Ic, PowerInfo.Phi_Ua, PowerInfo.Phi_Ub, PowerInfo.Phi_Uc, PowerInfo.Phi_Ia, PowerInfo.Phi_Ib, PowerInfo.Phi_Ic, PowerInfo.P, PowerInfo.Q, PowerInfo.S);
            return strResolve;
        }

        private sbyte GetByteFromByteArray(byte pArray)
        {
            string Fmt16 = Convert.ToString(pArray, 16);
            sbyte ReturnValue = (Convert.ToSByte(Fmt16, 16));
            return ReturnValue;
        }
    }
    #endregion

    #region CL3115读取标准表电能
    /// <summary>
    /// 读取标准表电能
    /// </summary>
    internal class CL3115_RequestReadStdMeterTotalNumPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdMeterTotalNumPacket()
            : base()
        { }

        /*
         * 81 30 PCID 09 a0 02 20 10 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x20);
            buf.Put(0x10);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取标准表电能。";
            return strResolve;
        }
    }
    /// <summary>
    /// 读取电能
    /// </summary>
    internal class CL3115_RequestReadStdMeterTotalNumReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestReadStdMeterTotalNumReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdMeterTotalNumReplayPacket";
        }
        /// <summary>
        /// 累计电能 8字节，放大10000倍，低字节先传
        /// </summary>
        /// <returns></returns>
        public float MeterTotalNum 
        { 
            get; 
            private set; 
        }


        /// <summary>
        /// 成功返回数据: 81 PCID 30 11 50 02 20 10 llE1 CS
        /// </summary>
        /// <param name="data"></param>
        protected override void ParseBody(byte[] data)
        {
            if (data.Length != 0x0c)
            {
                ReciveResult = RecvResult.FrameError;
                return;
            }
            ByteBuffer buf = new ByteBuffer(data);

            //去掉 命令字 50
            buf.Get();
            //去掉0x02
            buf.Get();
            //去掉0x20
            buf.Get();
            //去掉0x10
            buf.Get();
            //累计电能,放大10000倍
            float fStdMeter = BitConverter.ToInt64(buf.GetByteArray(8), 0);
            MeterTotalNum = fStdMeter / 10000;
            ReciveResult = RecvResult.OK;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + MeterTotalNum.ToString();
            return strResolve;
        }
    }
    #endregion

    #region CL3115读取标准表累计脉冲数
    /// <summary>
    /// 读取电能累计脉冲数
    /// </summary>
    internal class CL3115_RequestReadStdMeterTotalPulseNumPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdMeterTotalPulseNumPacket()
            : base()
        { }

        /*
         * 81 30 PCID 09 a0 02 40 80 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x40);
            buf.Put(0x80);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取标准表累计脉冲数。";
            return strResolve;
        }
    }
    /// <summary>
    /// 读取电能累计脉冲数
    /// </summary>
    internal class CL3115_RequestReadStdMeterTotalPulseNumReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestReadStdMeterTotalPulseNumReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdMeterTotalPulseNumReplayPacket";
        }
        /// <summary>
        /// 电能累计脉冲数8字节，低字节先传 ,CLT协议（UINT8）/变量定义SIN8
        /// </summary>
        /// <returns></returns>
        public long Pulsenum { get; private set; }


        /// <summary>
        /// 成功返回数据:81 PCID 30 11 50 02 40 80 llPulsenum1 CS
        /// </summary>
        /// <param name="data"></param>
        protected override void ParseBody(byte[] data)
        {
            //if (data.Length != 0x11)
            //{
            //    ReciveResult = RecvResult.FrameError;
            //    return;
            //}
            ByteBuffer buf = new ByteBuffer(data);
            //buf.Initialize();
            //去掉 命令字 50
            buf.Get();

            //去掉0x02
            buf.Get();
            //去掉0x40
            buf.Get();
            //去掉0x80
            buf.Get();

            //累计电能,放大10000倍
            Pulsenum = buf.GetLong_S();
            ReciveResult = RecvResult.OK;

        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + Pulsenum.ToString();
            return strResolve;
        }
    }
    #endregion

    #region CL3115读取走字数据
    /// <summary>
    /// 读取电能走字数据
    /// </summary>
    internal class CL3115_RequestReadStdMeterZZDataPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdMeterZZDataPacket()
            : base()
        { }

        /*
         * 81 30 PCID 0a a0 02 60 10 80 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x60);
            buf.Put(0x10);
            buf.Put(0x80);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取标准表走字数据。";
            return strResolve;
        }
    }
    /// <summary>
    /// 读取电能走字数据
    /// </summary>
    internal class CL3115_RequestReadStdMeterZZDataReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestReadStdMeterZZDataReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdMeterZZDataReplayPacket";
        }

        /// <summary>
        /// 累计电能 放大10000倍
        /// </summary>
        /// <returns></returns>
        public float meterTotalNum { get; private set; }

        /// <summary>
        /// 电能当前脉冲累计值
        /// </summary>
        public long meterPulseNum { get; private set; }

        /*
         * 成功返回数据
         *  81 PCID 30 1a 50
         * 02 60 
         * 10 
         * 00 00 00 00 00 00 00 00 //累计电能 放大10000倍
         * 80
         * 00 00 00 00 00 00 00 00 //电能当前脉冲累计值
         * CS
         * 失败返回Cmd 33
         * 81 PCID 30 06 33 CS 
         */
        protected override void ParseBody(byte[] data)
        {
            //if (data.Length != 0x1A)
            //{
            //    ReciveResult = RecvResult.FrameError;
            //    return;
            //}
            ByteBuffer buf = new ByteBuffer(data);
            //去掉 命令字 50
            buf.Get();

            //去掉0x02
            buf.Get();
            //去掉0x60
            buf.Get();
            //去掉0x10
            buf.Get();

            //累计电能
            meterTotalNum = buf.GetLong_S() / 10000f;

            //去掉0x80
            buf.Get();

            meterPulseNum = buf.GetLong_S();
            ReciveResult = RecvResult.OK;

        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("返回：累计电能:{0},脉冲数:{1}", meterTotalNum, meterPulseNum);
            return strResolve;
        }
    }
    #endregion

    #region CL3115读取各项电压电流谐波幅值
    /// <summary>
    /// 读取各项电压电流谐波幅值
    /// </summary>
    internal class CL3115_RequestReadStdMeterHarmonicArryPacket : CL3115SendPacket
    {
        private byte ucHPhase;
        private ushort usHAryStart;
        private byte ucHLen;
        private byte page;
        public CL3115_RequestReadStdMeterHarmonicArryPacket()
            : base()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phase">0:Uc,1:Ub,2:Ua,3:Ic,4:Ib,5:Ia,6:PhiUc;7:PhiUb,8:PhiUa,9:PhiIc,10:PhiIb,11:PhiIa</param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        public void SetPara(int phase, int start, int len)
        {
            if (phase >= 6)
            {
                page = 0x02;
                ucHPhase = Convert.ToByte(0x10 + phase - 6);
            }
            else
            {
                page = 0x03;
                ucHPhase = Convert.ToByte(phase);
            }
            int n = 4;
            usHAryStart = Convert.ToUInt16(start * n);
            ucHLen = Convert.ToByte(len * n);
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA5);
            buf.Put(page);
            buf.Put(ucHPhase);
            buf.PutUShort_S(usHAryStart);
            buf.Put(ucHLen);
            return buf.ToByteArray();

        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取各项电压电流谐波幅值。";
            return strResolve;
        }
    }
    /// <summary>
    /// 返回各项电压电流谐波幅值
    /// </summary>
    internal class CL3115_RequestReadStdMeterHarmonicArryReplayPacket : CL3115RecvPacket
    {
        public float[] fHarmonicArryData;
        public CL3115_RequestReadStdMeterHarmonicArryReplayPacket()
            : base()
        {
        }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdMeterHarmonicArryReplayPacket";
        }

        //protected override void ParseBody(byte[] data)
        //{
        //    if (data.Length < 5)
        //    {
        //        ReciveResult = RecvResult.FrameError;
        //        return;
        //    }
        //    ByteBuffer buf = new ByteBuffer(data);
        //    int iCount = data.Length - 2 / 4;
        //    fHarmonicArryData = new float[iCount];
        //    buf.Initialize();
        //    //去掉0x55
        //    buf.Get();

        //    //去掉0x03
        //    buf.Get();
        //    for (int i = 0; i < iCount; i++)
        //    {
        //        fHarmonicArryData[i] = buf.GetIntE1();
        //    }
        //    ReciveResult = RecvResult.OK;
        //}
        protected override void ParseBody(byte[] data)
        {
            if (data.Length < 5)
            {
                ReciveResult = RecvResult.FrameError;
                return;
            }
            ByteBuffer buf = new ByteBuffer(data);
            //去掉0x55
            buf.Get();

            //去掉0x03
            buf.Get();
            byte ary = buf.Get();
            byte start1 = buf.Get();
            byte start2 = buf.Get();
            byte len = buf.Get();
            int iCount = Convert.ToInt32(len)/4;
            fHarmonicArryData = new float[iCount];
            for (int i = 0; i < iCount; i++)
            {
                fHarmonicArryData[i] = (float)(buf.GetInt_S() / 10000f);
            }
            ReciveResult = RecvResult.OK;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }

    #endregion

    #region CL3115读取各项电压电流波形数据
    /// <summary>
    /// 读取各项电压电流波形数据
    /// </summary>
    internal class CL3115_RequestReadStdMeterWaveformArryPacket : CL3115SendPacket
    {
        private byte ucWPhase;

        private ushort usWAryStart;

        private byte ucWLen;

        public CL3115_RequestReadStdMeterWaveformArryPacket()
            : base()
        {
        }
        /// <summary>
        /// 设置合成参数
        /// </summary>
        /// <param name="bWp">相别</param>
        /// <param name="bWaStart">起始参数</param>
        /// <param name="bLen">长度</param>
        public void SetPara(byte bWp, ushort bWaStart, byte bLen)
        {
            switch (bWp)
            { 
                case 0x00://Uc
                    ucWPhase = 0x22;
                    break;
                case 0x01://Ub
                    ucWPhase = 0x1D;
                    break;
                case 0x02://Ua
                    ucWPhase = 0x1A;
                    break;
                case 0x03://Ic
                    ucWPhase = 0x2D;
                    break;
                case 0x04://Ib
                    ucWPhase = 0x2A;
                    break;
                case 0x05://Ia
                    ucWPhase = 0x25;
                    break;
                default:
                    ucWPhase = 0x22;
                    break;
            }
            usWAryStart = bWaStart;
            ucWLen = bLen;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取各项电压电流波形数据。";
            return strResolve;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA5);
            buf.Put(0x03);
            buf.Put(ucWPhase);
            buf.PutUShort_S(usWAryStart);
            buf.Put(ucWLen);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 返回各项电压电流波形数据
    /// </summary>
    internal class CL3115_RequestReadStdMeterWaveformArryReplayPacket : CL3115RecvPacket
    {
        public float [] fWaveformData;

        public CL3115_RequestReadStdMeterWaveformArryReplayPacket()
            : base()
        {
        }
        public override string GetPacketName()
        {
            return "CL3115_RequestReadStdMeterWaveformArryReplayPacket";
        }

        protected override void ParseBody(byte[] data)
        {
            if (data.Length < 3)
            {
                ReciveResult = RecvResult.FrameError;
                return;
            }
            ByteBuffer buf = new ByteBuffer(data);
            //去掉0x55
            buf.Get();

            //去掉0x03
            buf.Get();
            byte ary = buf.Get();
            byte start1 = buf.Get();
            byte start2 = buf.Get();
            byte len = buf.Get();
            int iCount = Convert.ToInt32(len) / 2;
            fWaveformData = new float[iCount];
            for (int i = 0; i < iCount; i++)
            {
                fWaveformData[i] = Convert.ToSingle(BitConverter.ToInt16(buf.GetByteArray(2),0)/100f);
            }
            ReciveResult = RecvResult.OK;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }

    #endregion

    #region CL3115设置标准表常数
    /// <summary>
    /// 设置标准表常数
    /// </summary>
    internal class CL3115_RequestSetStdMeterConstPacket : CL3115SendPacket
    {
        /// <summary>
        /// 本机常数，4字节，低字节先传
        /// </summary>
        private int stdMeterConst;

        public CL3115_RequestSetStdMeterConstPacket()
            : base()
        {

        }

        /// <summary>
        /// 设置本机常数
        /// </summary>
        /// <param name="meterconst">本机常数</param>
        /// <param name="needReplay">是否需要回复</param>
        public CL3115_RequestSetStdMeterConstPacket(int meterconst, bool needReplay)
            : base(needReplay)
        {
            stdMeterConst = meterconst;
        }

        public void SetPara(int meterconst)
        {
            stdMeterConst = meterconst;
        }
        /*
         * 81 30 PCID 0d a3 00 04 01 uiLocalnum CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x00);
            buf.Put(0x04);
            buf.Put(0x01);
            buf.PutInt_S(stdMeterConst);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置标准表常数：" + stdMeterConst.ToString();
            return strResolve;
        }
        public override int WaiteTime()
        {
            return 500;
        }
    }
    /// <summary>
    /// 设置标准表常数返回包
    /// </summary>
    internal class CL3115_RequestSetStdMeterConstReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetStdMeterConstReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestSetStdMeterConstReplayPacket";
        }


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

    #region CL3115设置标准表参数
    /// <summary>
    /// 设置标准表参数
    /// </summary>
    internal class CL3115_RequestSetParaPacket : CL3115SendPacket
    {
        private byte _YouGongSetData;
        private byte _ClfsSetData;
        private byte _CalcType;
        /// <summary>
        /// 
        /// </summary>
        public CL3115_RequestSetParaPacket():base()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_Clfs">测量方式</param>        
        public void SetPara(Cus_Clfs _Clfs, Cus_PowerFangXiang glfx, int calcType,bool bAuto)
        {

            if (glfx == Cus_PowerFangXiang.ZXP || glfx == Cus_PowerFangXiang.FXP)
                _YouGongSetData = 0x00;
            else
                _YouGongSetData = 0x40;

            _CalcType = Convert.ToByte(calcType);
            if (bAuto)
            {
                switch (_Clfs)
                {
                    case Cus_Clfs.PT4:
                        _ClfsSetData = 0x08;
                        break;
                    case Cus_Clfs.PT3:
                        _ClfsSetData = 0x48;
                        break;
                    case Cus_Clfs.EK90:
                        _ClfsSetData = 0x44;
                        break;
                    case Cus_Clfs.EK60:
                        _ClfsSetData = 0x42;
                        break;
                    case Cus_Clfs.SK90:
                        _ClfsSetData = 0x41;
                        break;
                    default:
                        _ClfsSetData = 0x08;
                        break;
                }
            }
            else
            {
                switch (_Clfs)
                {
                    case Cus_Clfs.PT4:
                        _ClfsSetData = 0x88;
                        break;
                    case Cus_Clfs.PT3:
                        _ClfsSetData = 0xC8;
                        break;
                    case Cus_Clfs.EK90:
                        _ClfsSetData = 0xC4;
                        break;
                    case Cus_Clfs.EK60:
                        _ClfsSetData = 0xC2;
                        break;
                    case Cus_Clfs.SK90:
                        _ClfsSetData = 0xC1;
                        break;
                    default:
                        _ClfsSetData = 0x88;
                        break;
                }
            }

        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);
            buf.Put(0x00);
            buf.Put(0x09);
            buf.Put(0x20);
            buf.Put(_ClfsSetData);
            buf.Put(0x11);
            buf.Put(_YouGongSetData);
            buf.Put(0x00);
            buf.Put(_CalcType);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置标准表参数，测量方式：" + _ClfsSetData.ToString() + "有功无功：" + _YouGongSetData.ToString();
            return strResolve;
        }
        public override int WaiteTime()
        {
            return 500;
        }
    }
    /// <summary>
    /// 设置标准表参数返回包
    /// </summary>
    internal class CL3115_RequestSetParaReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetParaReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestSetParaReplayPacket";
        }

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

    #region CL3115返回指令
    class CL3115_ReplyOkPacket : CL3115RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data.Length != 1)
            {
                this.ReciveResult = RecvResult.DataError;
            }
            else if (data[0] == 0x30)
            {
                this.ReciveResult = RecvResult.OK;
            }
            else
            {
                this.ReciveResult = RecvResult.DataError;
            }
        }
    }
    #endregion

    #region CL3115设置档位
    /// <summary>
    /// 设置档位
    /// </summary>
    internal class CL3115_RequestSetStdMeterDangWeiPacket : CL3115SendPacket
    {
        /// <summary>
        /// C相电压档位
        /// </summary>
        private Cus_StdMeterURange ucUcRange;
        /// <summary>
        /// B相电压档位
        /// </summary>
        private Cus_StdMeterURange ucUbRange;
        /// <summary>
        /// A相电压档位
        /// </summary>
        private Cus_StdMeterURange ucUaRange;
        /// <summary>
        /// C相电流档位
        /// </summary>
        private Cus_StdMeterIRange ucIcRange;
        /// <summary>
        /// B相电流档位
        /// </summary>
        private Cus_StdMeterIRange ucIbRange;
        /// <summary>
        /// C相电流档位
        /// </summary>
        private Cus_StdMeterIRange ucIaRange;

        /// <summary>
        /// 通一设置档位,默认需要回复
        /// </summary>
        /// <param name="uRange">电压档位</param>
        /// <param name="iRange">电流档位</param>
        public CL3115_RequestSetStdMeterDangWeiPacket(Cus_StdMeterURange uRange, Cus_StdMeterIRange iRange)
            : base()
        {
            ucUaRange = uRange;
            ucUbRange = uRange;
            ucUcRange = uRange;
            ucIaRange = iRange;
            ucIbRange = iRange;
            ucIcRange = iRange;
        }
        /// <summary>
        /// 通一设置档位
        /// </summary>
        /// <param name="uRange">电压档位</param>
        /// <param name="iRange">电流档位</param>
        /// <param name="needReplay">是否需要回复</param>
        public CL3115_RequestSetStdMeterDangWeiPacket(Cus_StdMeterURange uRange, Cus_StdMeterIRange iRange, bool needReplay)
            : base(needReplay)
        {
            ucUaRange = uRange;
            ucUbRange = uRange;
            ucUcRange = uRange;
            ucIaRange = iRange;
            ucIbRange = iRange;
            ucIcRange = iRange;
        }

        /// <summary>
        /// 设置档位
        /// </summary>
        /// <param name="uaRange">A相电压档位</param>
        /// <param name="ubRange">B相电压档位</param>
        /// <param name="ucRange">C相电压档位</param>
        /// <param name="iaRange">A相电流档位</param>
        /// <param name="ibRange">B相电流档位</param>
        /// <param name="icRange">C相电流档位</param>
        public CL3115_RequestSetStdMeterDangWeiPacket(Cus_StdMeterURange uaRange, Cus_StdMeterURange ubRange, Cus_StdMeterURange ucRange, Cus_StdMeterIRange iaRange, Cus_StdMeterIRange ibRange, Cus_StdMeterIRange icRange)
            : base()
        {
            ucUaRange = uaRange;
            ucUbRange = ubRange;
            ucUcRange = ucRange;
            ucIaRange = iaRange;
            ucIbRange = ibRange;
            ucIcRange = icRange;
        }
        /// <summary>
        /// 设置档位
        /// </summary>
        /// <param name="uaRange">A相电压档位</param>
        /// <param name="ubRange">B相电压档位</param>
        /// <param name="ucRange">C相电压档位</param>
        /// <param name="iaRange">A相电流档位</param>
        /// <param name="ibRange">B相电流档位</param>
        /// <param name="icRange">C相电流档位</param>
        /// <param name="needReplay">是否需要回复</param>
        public CL3115_RequestSetStdMeterDangWeiPacket(Cus_StdMeterURange uaRange, Cus_StdMeterURange ubRange, Cus_StdMeterURange ucRange, Cus_StdMeterIRange iaRange, Cus_StdMeterIRange ibRange, Cus_StdMeterIRange icRange, bool needReplay)
            : base(needReplay)
        {
            ucUaRange = uaRange;
            ucUbRange = ubRange;
            ucUcRange = ucRange;
            ucIaRange = iaRange;
            ucIbRange = ibRange;
            ucIcRange = icRange;
        }

        /*
         * 81 30 PCID 0F A3 02 02 3F ucUcRange ucUbRange ucUaRange ucIcRange ucIbRange ucIaRange CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x02);
            buf.Put(0x02);
            buf.Put(0x3F);
            buf.Put((byte)ucUcRange);
            buf.Put((byte)ucUbRange);
            buf.Put((byte)ucUaRange);
            buf.Put((byte)ucIcRange);
            buf.Put((byte)ucIbRange);
            buf.Put((byte)ucIaRange);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("设置标准表档位：{0},{1},{2},{3},{4},{5}", ucUaRange.ToString(), ucUbRange.ToString(), ucUcRange.ToString(), ucIaRange.ToString(), ucIbRange.ToString(), ucIcRange.ToString());
            return strResolve;
        }
        public override int WaiteTime()
        {
            return 500;
        }
    }
    /// <summary>
    /// 设置标准表接线方式返回包
    /// </summary>
    internal class CL3115_RequestSetStdMeterDangWeiReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetStdMeterDangWeiReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestSetStdMeterDangWeiReplayPacket";
        }


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

    #region CL3115设置接线方式
    /// <summary>
    /// 设置接线方式
    /// </summary>
    internal class CL3115_RequestSetStdMeterLinkTypePacket : CL3115SendPacket
    {
        private byte _SetData;
        /// <summary>
        /// 
        /// </summary>
        public CL3115_RequestSetStdMeterLinkTypePacket():base()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_Clfs">测量方式</param>
        /// <param name="bAuto">自动，手动</param>
        public void SetPara(Cus_Clfs _Clfs, bool bAuto)
        {
            if (bAuto)
            {
                switch (_Clfs)
                {
                    case Cus_Clfs.PT4:
                        _SetData = 0x08;
                        break;
                    case Cus_Clfs.PT3:
                        _SetData = 0x48;
                        break;
                    case Cus_Clfs.SK90:
                        _SetData = 0x44;
                        break;
                    case Cus_Clfs.EK90:
                        _SetData = 0x42;
                        break;
                    case Cus_Clfs.EK60:
                        _SetData = 0x41;
                        break;
                    default:
                        _SetData = 0x08;
                        break;
                }
            }
            else
            {
                switch (_Clfs)
                {
                    case Cus_Clfs.PT4:
                        _SetData = 0x88;
                        break;
                    case Cus_Clfs.PT3:
                        _SetData = 0xC8;
                        break;
                    case Cus_Clfs.SK90:
                        _SetData = 0xC4;
                        break;
                    case Cus_Clfs.EK90:
                        _SetData = 0xC2;
                        break;
                    case Cus_Clfs.EK60:
                        _SetData = 0xC1;
                        break;
                    default:
                        _SetData = 0x88;
                        break;
                }
            }

        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);
            buf.Put(0x00);
            buf.Put(0x01);
            buf.Put(0x20);
            buf.Put(_SetData);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置接线方式：" + _SetData.ToString();
            return strResolve;
        }
        public override int WaiteTime()
        {
            return 500;
        }
    }
    /// <summary>
    /// 设置标准表接线方式返回包
    /// </summary>
    internal class CL3115_RequestSetStdMeterLinkTypeReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetStdMeterLinkTypeReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestSetStdMeterLinkTypeReplayPacket";
        }


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

    #region CL3115设置标准表显示
    /// <summary>
    /// 置标准表界面
    /// 由于谐波数据和波形数据仅在对应界面下获取，读取谐波数据和波形数据前必须将界面切到对应界面
    /// 界面设置命令在界面切换过程中享有最高优先级，因此为不影响上位机和使用人员的正常操作
    /// 在不需读取谐波数据和波形数据后，将界面设置为清除上位机设置。
    /// </summary>
    internal class CL3115_RequestSetStdMeterScreenPacket : CL3115SendPacket
    {
        /// <summary>
        /// 标准表界面指示
        /// </summary>
        private Cus_StdMeterScreen meterScreen;

        /// <summary>
        /// 设置标准表界面
        /// </summary>
        /// <param name="meterscreen">标准表界面指示</param>
        public CL3115_RequestSetStdMeterScreenPacket()
            : base()
        {
        }
        public void SetPara(int formType)
        {
            meterScreen = Cus_StdMeterScreen.清除设置界面;
            switch (formType)
            {
                case 1:
                    meterScreen = Cus_StdMeterScreen.功率测量界面;
                    break;
                case 2:
                    meterScreen = Cus_StdMeterScreen.电压电流相位角界面;
                    break;
                case 3:
                    meterScreen = Cus_StdMeterScreen.电能误差界面;
                    break;
                case 4:
                    meterScreen = Cus_StdMeterScreen.电能误差启动界面;
                    break;
                case 5:
                    meterScreen = Cus_StdMeterScreen.走字测试界面;
                    break;
                case 6:
                    meterScreen = Cus_StdMeterScreen.走字测试启动界面;
                    break;
                case 7:
                    meterScreen = Cus_StdMeterScreen.矢量图;
                    break;
                case 9:
                    meterScreen = Cus_StdMeterScreen.谐波柱图界面;
                    break;
                case 10:
                    meterScreen = Cus_StdMeterScreen.谐波列表界面;
                    break;
                case 11:
                    meterScreen = Cus_StdMeterScreen.波形界面;
                    break;
                case 254:
                    meterScreen = Cus_StdMeterScreen.清除设置界面;
                    break;
                default:
                    break;
            }
        }

        /*
         * 81 30 PCID 0a a3 00 10 80 ucARM_Menu CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            //buf.Put(0x00);
            //buf.Put(0x08);
            //buf.Put(0x01);
            buf.Put(0x00);
            buf.Put(0x10);
            buf.Put(0x80);
            buf.Put((byte)meterScreen);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置标准表显示：" + meterScreen.ToString();
            return strResolve;
        }
    }
    /// <summary>
    /// 设置标准表显示返回包
    /// </summary>
    internal class CL3115_RequestSetStdMeterScreenReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetStdMeterScreenReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestSetStdMeterScreenReplayPacket";
        }

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

    #region CL3115设置标准表测量方式
    /// <summary>
    /// 设置3115标准表测量方式
    /// </summary>
    internal class CL3115_RequestSetStdMeterUsE1typePacket : CL3115SendPacket
    {
        private byte _SetData;
        /// <summary>
        /// 
        /// </summary>
        public CL3115_RequestSetStdMeterUsE1typePacket()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_Clfs">测量方式</param>        
        public void SetPara(int glfx)
        {
            //if (glfx == Cus_PowerFangXiang.ZXP || glfx == Cus_PowerFangXiang.FXP)
            //    _SetData = 0x00;
            //else
            //    _SetData = 0x40;
            switch (glfx)
            {
                case 1://Psum
                    _SetData = 0x00;
                    break;
                case 2://Qsum
                    _SetData = 0x40;
                    break;
                //case 0://Psum
                //    _SetData = 0x00;
                //    break;
                //case 1://Pa
                //    _SetData = 0x04;
                //    break;
                //case 2://Pb
                //    _SetData = 0x08;
                //    break;
                //case 3://Pc
                //    _SetData = 0x0C;
                //    break;
                //case 4://Qsum
                //    _SetData = 0x40;
                //    break;
                //case 5://Qa
                //    _SetData = 0x44;
                //    break;
                //case 6://Qb
                //    _SetData = 0x48;
                //    break;
                //case 7://Qc
                //    _SetData = 0x4C;
                //    break;
                //case 8://Ssum
                //    _SetData = 0x80;
                //    break;
                //case 9://Sa
                //    _SetData = 0x84;
                //    break;
                //case 10://Sb
                //    _SetData = 0x88;
                //    break;
                //case 11://Sc
                //    _SetData = 0x8C;
                //    break;
            }
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);
            buf.Put(0x00);
            buf.Put(0x08);
            buf.Put(0x01);
            buf.Put(_SetData);
            buf.Put(0x00);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置测量方式：" + _SetData.ToString();
            return strResolve;
        }
        public override int WaiteTime()
        {
            return 500;
        }
    }
    /// <summary>
    /// 设置3115标准表测量方式返回包
    /// </summary>
    internal class CL3115_RequestSetStdMeterUsE1typeReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetStdMeterUsE1typeReplayPacket() : base() { }
        public override string GetPacketName()
        {
            return "CL3115_RequestSetStdMeterConstReplayPacket";
        }

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

    #region CL3115启动标准表
    /// <summary>
    /// 请求启动标准表指令包
    /// 返回0x4b成功
    /// </summary>
    internal class CL3115_RequestStartTaskPacket : CL3115SendPacket
    {
        /// <summary>
        /// 控制类型 
        /// </summary>
        /// <param name="iType"></param>
        public CL3115_RequestStartTaskPacket()
            : base()
        {

        }
        /// <summary>
        /// 控制类型 0，停止；1，开始计算电能误差；2，开始计算电能走字
        /// </summary>
        private int iControlType;

        public void SetPara(int iType)
        {
            this.iControlType = iType;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);
            buf.Put(0x00);
            buf.Put(0x08);
            buf.Put(0x10);
            buf.Put(Convert.ToByte(iControlType));
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = string.Format("控制标准表：{0}-{1}", iControlType, iControlType == 0 ? "停止" : (iControlType == 1 ? "开始计算电能误差" : "开始计算电能走字"));
            return strResolve;
        }
        public override int WaiteTime()
        {
            return 500;
        }
    }
    /// <summary>
    /// 控制标准表启动、停止、开始走字，返回指令
    /// </summary>
    internal class CL3115_RequestStartTaskReplyPacket : CL3115RecvPacket
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

    #region CL3115设置电能误差检定参数
    /// <summary>
    /// 设置电能误差检定参数
    /// </summary>
    internal class CL3115_RequestSetStdMeterCalcParamsPacket : CL3115SendPacket
    {
        //校验圈数（脉冲数）
        private int pulseNum;
        //被检表常数
        private int testConst;

        public CL3115_RequestSetStdMeterCalcParamsPacket()
            : base()
        {
        }
        public void SetPara(int iPulse,int iConst)
        {
            pulseNum = iPulse;
            testConst = iConst;
        }

        //81 30 PCID 16 a3 00 14 08 lPnum 01 llTestcnt CS
        protected override byte[] GetBody()
        {

            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x00);
            buf.Put(0x14);
            buf.Put(0x08);
            buf.PutInt_S(pulseNum);
            buf.Put(0x01);
            buf.PutLong_S(testConst,8,1);
            return buf.ToByteArray();
        }
        public override int WaiteTime()
        {
            return 500;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置电能误差检定参数，校验圈数：" + pulseNum.ToString() + "被检表常数：" + testConst.ToString();
            return strResolve;
        }
    }
    /// <summary>
    /// 设置电能误差检定参数返回包
    /// </summary>
    internal class CL3115_RequestSetStdMeterCalcParamsReplayPacket : CL3115RecvPacket
    {
        public CL3115_RequestSetStdMeterCalcParamsReplayPacket()
            : base()
        {
        }

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

    #region CL3115读取电能误差（仅CL1115主副表版本）

    /// <summary>
    /// 读取电能误差
    /// </summary>
    internal class CL3115_RequestReadStdMeterErrorPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdMeterErrorPacket()
            : base()
        {
        }


        //81 30 PCID 09 a0 02 40 04 CS
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x02);
            buf.Put(0x40);
            buf.Put(0x04);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "读取电能误差。";
            return strResolve;
        }
    }
    /// <summary>
    /// 电能误差返回包
    /// </summary>
    internal class CL3115_RequestReadStdMeterErrorReplayPacket : CL3115RecvPacket
    {
        public float fError = -1f;

        //81 PCID 30 0d 50 02 40 04 lErr1 CS
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 8)
                ReciveResult = RecvResult.DataError;
            else
            {
                ByteBuffer buf = new ByteBuffer(data);
                //cmd
                buf.Get();
                buf.Get();
                buf.Get();
                buf.Get();
                fError = BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F;
                //byte[] bErr = new byte[4];
                //Array.Copy(data, 4, bErr, 0, 4);
                //Array.Reverse(bErr);
                //fError = BitConverter.ToSingle(bErr, 0);
                ReciveResult = RecvResult.OK;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }

    #endregion


    #region CL3115读取最近一次电能误差及误差计算次数
    /// <summary>
    /// 读取最近一次电能误差和次数
    /// </summary>
    internal class CL3115_RequestReadStdMeterLastErrorPacket : CL3115SendPacket
    {
        public CL3115_RequestReadStdMeterLastErrorPacket()
            : base()
        {
        }
        //81 30 PCID 0C a0 00 04 20 02 40 04 CS
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA0);          //命令 
            buf.Put(0x00);
            buf.Put(0x04);
            buf.Put(0x20);
            buf.Put(0x02);
            buf.Put(0x40);
            buf.Put(0x04);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "读取最近一次电能误差和次数。";
            return strResolve;
        }
    }
    /// <summary>
    /// 读取最近一次电能误差和次数返回包
    /// </summary>
    internal class CL3115_RequestReadStdMeterLastErrorReplayPacket : CL3115RecvPacket
    {
        //误差
        public float fError = -1f;
        //次数
        public int iNumber = -1;

        //81 PCID 30 11 50 00 04 20 E_Num 02 40 04 lErr1 CS
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 12)
                ReciveResult = RecvResult.DataError;
            else
            {
                ByteBuffer buf = new ByteBuffer(data);
                //cmd
                buf.Get();
                buf.Get();
                buf.Get();
                buf.Get();
                iNumber = Convert.ToInt32(buf.Get());

                buf.Get();
                buf.Get();
                buf.Get();
                fError = BitConverter.ToInt32(buf.GetByteArray(4), 0) / 10000F;

                //iNumber = Convert.ToInt32(data[4]);

                //byte[] bErr = new byte[4];
                //Array.Copy(data, 8, bErr, 0, 4);
                //Array.Reverse(bErr);
                //fError = BitConverter.ToSingle(bErr, 0);
                ReciveResult = RecvResult.OK;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }

    #endregion

    #region CL3115设置电压电流幅值和相位 @C_B
    internal class CL3115_ReplyAmplitudeAndPhasePacket : CL3115SendPacket
    {
        long c_u_v, b_u_v, a_u_v, c_i_v, b_i_v, a_i_v, c_u_p, b_u_p, a_u_p, c_i_p, b_i_p, a_i_p;
        byte u_v, i_v;
        public CL3115_ReplyAmplitudeAndPhasePacket(long c_u_v, long b_u_v, long a_u_v, long c_i_v, long b_i_v, long a_i_v, long c_u_p, long b_u_p, long a_u_p, long c_i_p, long b_i_p, long a_i_p, byte u_v, byte i_v)
            : base()
        {
            ToID = 0x01;
            MyID = 0x30;
            this.c_u_v = c_u_v;
            this.b_u_v = b_u_v;
            this.a_u_v = a_u_v;
            this.c_i_v = c_i_v;
            this.b_i_v = b_i_v;
            this.a_i_v = a_i_v;
            this.c_u_p = c_u_p;
            this.b_u_p = b_u_p;
            this.a_u_p = a_u_p;
            this.c_i_p = c_i_p;
            this.b_i_p = b_i_p;
            this.a_i_p = a_i_p;
            this.u_v = u_v;
            this.i_v = i_v;
        }
        //public void SetPara(long c_u_v, long b_u_v, long a_u_v, long c_i_v, long b_i_v, long a_i_v, long c_u_p, long b_u_p, long a_u_p, long c_i_p, long b_i_p, long a_i_p,byte u_v,byte i_v)
        //{
        //    this.c_u_v = c_u_v;
        //    this.b_u_v = b_u_v;
        //    this.a_u_v = a_u_v;
        //    this.c_i_v = c_i_v;
        //    this.b_i_v = b_i_v;
        //    this.a_i_v = a_i_v;
        //    this.c_u_p = c_u_p;
        //    this.b_u_p = b_u_p;
        //    this.a_u_p = a_u_p;
        //    this.c_i_p = c_i_p;
        //    this.b_i_p = b_i_p;
        //    this.a_i_p = a_i_p;
        //    this.u_v = u_v;
        //    this.i_v = i_v;
        //}
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x50);
            buf.Put(0x02);
            buf.Put(0x05);
            buf.Put(0x3F);
            buf.PutLong_S(c_u_v, 4, 1);
            buf.Put(u_v);
            buf.PutLong_S(b_u_v, 4, 1);
            buf.Put(u_v);
            buf.PutLong_S(a_u_v, 4, 1);
            buf.Put(u_v);
            buf.PutLong_S(c_i_v, 4, 1);
            buf.Put(i_v);
            buf.PutLong_S(b_i_v, 4, 1);
            buf.Put(i_v);
            buf.PutLong_S(a_i_v, 4, 1);
            buf.Put(i_v);

            buf.Put(0x3F);
            buf.PutLong_S(c_u_p, 4, 1);
            buf.PutLong_S(b_u_p, 4, 1);
            buf.PutLong_S(a_u_p, 4, 1);
            buf.PutLong_S(c_i_p, 4, 1);
            buf.PutLong_S(b_i_p, 4, 1);
            buf.PutLong_S(a_i_p, 4, 1);

            return buf.ToByteArray();
        }
    }
    #endregion

    #region CL3115设置校正值 @C_B
    internal class CL3115_RequestSetCorrectionValuePacket : CL3115SendPacket
    {
        public CL3115_RequestSetCorrectionValuePacket()
            : base()
        {

        }
        /// <summary>
        /// 调整值
        /// </summary>
        private int adjustmentValue;

        public void SetPara(int iType, float referenceValue,float cl3115Value)
        {
            if (iType == 0)//相位
            {
                //dec. correction value =(external reference value-CL3115 value) * 10000
                adjustmentValue = Convert.ToInt32((referenceValue - cl3115Value) * 10000);
            }
            else//幅值
            { 
                //dec. correction value =（（external reference value - CL3115 valueexternal reference） /external reference value） * 1000000
                adjustmentValue = Convert.ToInt32(((referenceValue - cl3115Value) / referenceValue) * 1000000);
            }
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);
            buf.Put(0x06);
            buf.Put(0x01);
            buf.Put(0x80);
            buf.PutInt_S(adjustmentValue);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置校正值：" + adjustmentValue.ToString();
            return strResolve;
        }
    }
    /// <summary>
    /// 设置校正值，返回指令
    /// </summary>
    internal class CL3115_RequestSetCorrectionValueReplyPacket : CL3115RecvPacket
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

    #region CL3115设置相别信息 @C_B
    internal class CL3115_RequestSetPhaseInformationPacket : CL3115SendPacket
    {
        public CL3115_RequestSetPhaseInformationPacket()
            : base()
        {

        }
        /// <summary>
        /// 调整值
        /// </summary>
        private byte adjustmentFlag;

        public void SetPara(int phase, int amplitudeOrAngle)
        {
            if (amplitudeOrAngle == 0)//相位
            {
                switch (phase)
                {
                    case 1://Voltage phase L2, angle
                        adjustmentFlag = 0x21;
                        break;
                    case 2://Voltage phase L3, angle
                        adjustmentFlag = 0x31;
                        break;
                    case 3://Current phase L1, angle
                        adjustmentFlag = 0x41;
                        break;
                    case 4://Current phase L2, angle
                        adjustmentFlag = 0x51;
                        break;
                    case 5://Current phase L3, angle
                        adjustmentFlag = 0x61;
                        break; 
                    default:
                        adjustmentFlag = 0x00;
                        break;
                }
            }
            else//幅值
            {
                switch (phase)
                {
                    case 0://Voltage phase L1, amplitude
                        adjustmentFlag = 0x10;
                        break;
                    case 1://Voltage phase L2, amplitude
                        adjustmentFlag = 0x20;
                        break;
                    case 2://Voltage phase L3, amplitude
                        adjustmentFlag = 0x30;
                        break;
                    case 3://Current phase L1, amplitude
                        adjustmentFlag = 0x40;
                        break;
                    case 4://Current phase L2, amplitude
                        adjustmentFlag = 0x50;
                        break;
                    case 5://Current phase L3, amplitude
                        adjustmentFlag = 0x60;
                        break;
                    default:
                        adjustmentFlag = 0x00;
                        break;
                }
                
            }
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);
            buf.Put(0x00);
            buf.Put(0x20);
            buf.Put(0x02);
            buf.Put(adjustmentFlag);
            return buf.ToByteArray();
        }
        public override string GetPacketResolving()
        {
            string strResolve = "设置相别信息：" + adjustmentFlag.ToString();
            return strResolve;
        }
    }
    /// <summary>
    /// 设置相别信息，返回指令
    /// </summary>
    internal class CL3115_RequestSetPhaseInformationReplyPacket : CL3115RecvPacket
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
}
