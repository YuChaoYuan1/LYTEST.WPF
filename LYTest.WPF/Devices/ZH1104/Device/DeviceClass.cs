using System;
using ZH.SocketModule.Packet;

namespace ZH
{
    #region ZH1104功耗板

    #region ZH1104功耗测试联机指令
    /// <summary>
    /// ZH1104功耗测试联机/脱机请求包
    /// </summary>
    internal class ZH1104_RequestLinkPacket : ZH1104SendPacket
    {
        public bool IsLink = true;

        public ZH1104_RequestLinkPacket()
            : base()
        {

        }

        /*
         * 81 30 PCID 09 a0 02 02 40 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x88);
            buf.Put(0x0F);
            buf.Put(0x00);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "联机标准表。";
            return strResolve;
        }
    }
    /// <summary>
    /// ZH1104功耗测试，联机返回指令
    /// </summary>
    internal class ZH1104_RequestLinkReplyPacket : ZH1104RecvPacket
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
    #endregion




    #region ZH1104功耗测试读指令
    /// <summary>
    /// ZH1104功耗测试联机/脱机请求包
    /// </summary>
    internal class ZH1104_ReadGHDataRelayPacket : ZH1104SendPacket
    {

        //        3000H—读取电压回路、电流回路计算结果参数
        //发送：68H+RID+FEH+LEN+10H+3000H+CS
        //LEN：0x08

        //返回：68H+FEH+RID+LEN+90H+REG+DATA0+DATA1….DATAn+CS

        public ZH1104_ReadGHDataRelayPacket(int bw)
            : base()
        {
            this.ToID = Convert.ToByte(bw);
        }


        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);          //命令 
            buf.Put(0x30);
            buf.Put(0x00);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// ZH1104功耗测试，联机返回指令
    /// </summary>
    internal class ZH1104_ReadGHDataReplyPacket : ZH1104RecvPacket
    {

        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>
        //public float[] fldata  { get; private set; }
        //public override bool ParsePacket(byte[] buf)
        public float[] Fldata { get; private set; }
        protected override void ParseBody(byte[] data)
        {

        }

        public override bool ParsePacket(byte[] data)
        {
            bool resul = false;
            ByteBuffer buf = new ByteBuffer(data);
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {

                ReciveResult = RecvResult.Unknow;

                if (data[4] == 0x90)
                {
                    Fldata = new float[15];
                    ReciveResult = RecvResult.OK;
                    resul = true;
                    buf.Get(); //68
                    buf.Get();//toID
                    buf.Get();//myID
                    buf.Get();//len
                    buf.Get();// 90
                    buf.Get();// 30
                    buf.Get();//00

                    for (int i = 0; i < Fldata.Length; i++)
                    {

                        Fldata[i] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / 100000.0000f);
                    }

                }
            }
            return resul;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion


    //add yjt 20230103 新增零线电流控制启停
    #region 零线电流控制启停
    /// <summary>
    /// 零线电流控制启停
    /// </summary>
    internal class ZH1104_StartZeroCurrentLinkPacket : ZeroCurrentSendPacket
    {
        public int _A_kz = 0;
        public int _BC_kz = 0;
        public bool IsLink = true;

        public ZH1104_StartZeroCurrentLinkPacket()
            : base()
        {

        }

        public void SetPara(int A_kz, int BC_kz)
        {
            _A_kz = A_kz;
            _BC_kz = BC_kz;
        }

        /*
         * 68 01 FE 0A 13 40 00 00 01 A7
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x40);
            buf.Put(0x00);
            buf.Put(Convert.ToByte(_A_kz));
            buf.Put(Convert.ToByte(_BC_kz));
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "联机标准表。";
            return strResolve;
        }
    }
    /// <summary>
    /// 零线电流控制启停
    /// </summary>
    internal class ZH1104_StartZeroCurrentLinkReplyPacket : ZeroCurrentRecvPacket
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
    #endregion

    #endregion

    #region 2000H 接地故障试验控制继电器
    /// <summary>
    /// 接地故障试验控制继电器/脱机请求包
    /// </summary>
    internal class ZH1104_RequestJDGZPacket : ZH1104SendPacket
    {
        //public int ua = 0;
        //public int ub = 0;
        //public int uc = 0;
        //public int un = 0;

        public byte ua = 0x00; //A
        public byte ub = 0x00;  //B
        public byte uc = 0x00;  //C
        public byte un = 0x00;  //N

        public ZH1104_RequestJDGZPacket(int bw, int uN)
            : base()
        {
            if (uN == 01)
            {
                this.ToID = 0xff;
            }
            else
            {
                this.ToID = Convert.ToByte(bw);
            }
        }

        public void SetPara(int intUa, int intUb, int intUc, int intUn)
        {
            //ua = intUa;
            //ub = intUb;
            //uc = intUc;
            //un = intUn;

            ua = Convert.ToByte(intUa);
            ub = Convert.ToByte(intUb);
            uc = Convert.ToByte(intUc);
            un = Convert.ToByte(intUn);
        }

        /*
         * ਁ68H+RID+FEH+LEN+13H+2000H+DATA+CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x20);
            buf.Put(0x00);

            buf.Put(ua);
            buf.Put(ub);
            buf.Put(uc);
            buf.Put(un);

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "联机功能板。";
            return strResolve;
        }
    }
    /// <summary>
    /// 接地故障试验控制继电器/返回指令
    /// </summary>
    internal class ZH1104_RequestJDGZReplyPacket : ZH1104RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[4] == 0x93)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }

        public override bool ParsePacket(byte[] data)
        {
            bool resul = false;
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                ReciveResult = RecvResult.Unknow;
                foreach (byte idexData in data)
                {
                    if (idexData == 0x93)
                    {
                        ReciveResult = RecvResult.OK;
                        resul = true;
                        break;
                    }
                }
            }
            return resul;
        }
        public override string GetPacketResolving()
        {
            return "返回：" + ReciveResult.ToString();
        }
    }
    #endregion

}
