using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Enum;
using ZH.Struct;

namespace ZH

{



    internal class ZH1106SwitchPacket : ZH3001SendPacket
    {
        //    A．2500H偶次谐波启动寄存器
        //一个字节表示，Bit0为A相偶次谐波，Bit1为B相偶次谐波，Bit2为C相偶次谐波。对应的Bit位置1为启动偶次谐波发生，置0为停止偶次谐波发生。
        //例如：68 01 01 09 13 25 00 03 3C
        //回复：68 01 01 09 93 25 00 4B F4
        /// <summary>
        /// 状态
        /// </summary>


        private byte m_SwitchID = 0x00;

        public ZH1106SwitchPacket()
            : base(true)
        {
            ToID = 0x01;
            MyID = 0x01;
        }
        /// <summary>
        /// 设置继电器状态        
        /// </summary>
        /// <param name="Status"></param>
        public void SetPara(string strA, string strB, string strC)
        {
            m_SwitchID = Convert.ToByte(Convert.ToInt32(strC + strB + strA, 2));




        }
        public override string GetPacketName()
        {
            return "ZH1106SwitchPacket";
        }

        /*
         * Data的组织方式为：广播标志(0XFF) +（1 byte ListLen） + （12 bytes List）。
        */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();//19976574528
            buf.Initialize();

            //68 01 01 09 13 25 00 03 3C
            buf.Put(0x13);
            buf.Put(0x25);
            buf.Put(0x00);
            buf.Put(m_SwitchID);
            ;
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 指令，返回数据包
    /// </summary>
    internal class ZH1106SwitchReplayPacket : ZH3001RecvPacket
    {


        /// <summary>
        /// 返回路数
        /// </summary>
        public byte Pos
        {
            get;
            private set;
        }
        public override string GetPacketName()
        {
            return "CL2050SwitchReplayPacket";
        }



        protected override void ParseBody(byte[] data)
        {



            if (data == null || data.Length != 4)
            {
                ReciveResult = RecvResult.DataError;
            }
            else
            {
                if (data[data.Length - 1] == 0x4b)
                    ReciveResult = RecvResult.OK;
                else
                {
                    ReciveResult = RecvResult.NOCOMMAND;
                }
            }

        }
    }



    internal class ZH1106ReadSwitchPacket : ZH3001SendPacket
    {
        //        B．2501H偶次谐波电流采样值
        //设备模式：1个字节表示，0x00为三相模式，0x01为单相模式；
        //电流值：指数形式表示，5个字节，最低字节为指数index，高4字节为数据体sidata。
        //解析时使用数学函数fdata = pow(10, index)计算出缩小或放大倍数。再将fdata* sidata = 最终真实值；
        //数据排列为： 设备模式 + A相正半波电流值 + A相负半波电流值 + B相正半波电流值 + B相负半波电流值 + C相正半波电流值 + C相负半波电流值
        //例如： 68 01 01 08 10 25 01 3C
        //回复： 68 01 01 27 90 25 01 00 00 00 00 00 FB 00 00 00 00 FB 00 00 00 00 FB 00 00 00 00 FB 00 00 00 00 FB 00 00 00 00 FB 93

        public ZH1106ReadSwitchPacket()
            : base(true)
        {
            ToID = 0x01;
            MyID = 0x01;
        }
        /// <summary>
        /// 设置继电器状态        
        /// </summary>
        /// <param name="Status"></param>
        public void SetPara(string strA, string strB, string strC)
        {
            //m_SwitchID = Convert.ToByte(Convert.ToInt32(strC + strB + strA, 2));
        }
        public override string GetPacketName()
        {
            return "ZH1106ReadSwitchPacket";
        }

        /*
         * Data的组织方式为：广播标志(0XFF) +（1 byte ListLen） + （12 bytes List）。
        */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();

            //68 01 01 08 10 25 01 3C
            buf.Put(0x10);
            buf.Put(0x25);
            buf.Put(0x01);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 指令，返回数据包
    /// </summary>
    internal class ZH1106ReadSwitchReplayPacket : ZH3001RecvPacket
    {



        public float[] floatcurrent = new float[6];
        /// <summary>
        /// 返回路数
        /// </summary>
        public byte Pos
        {
            get;
            private set;
        }
        public override string GetPacketName()
        {
            return "CL2050SwitchReplayPacket";
        }



        protected override void ParseBody(byte[] data)
        {



            if (data == null || data.Length != 34)
            {
                ReciveResult = RecvResult.DataError;
            }
            else
            {
                if (data[0] == 0x90)
                {
                    ByteBuffer buf = new ByteBuffer(data);
                    buf.Get(); //0x90
                    buf.Get();  //0x25
                    buf.Get(); //0x01
                    buf.Get(); //0x00
                    floatcurrent[0] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
                    floatcurrent[1] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
                    floatcurrent[2] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
                    floatcurrent[3] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
                    floatcurrent[4] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));
                    floatcurrent[5] = Convert.ToSingle(BitConverter.ToInt32(buf.GetByteArrayRev(4, true), 0) / Math.Pow(10, 0 - GetByteFromByteArray(buf.Get())));

                    ReciveResult = RecvResult.OK;
                }
                else
                {
                    ReciveResult = RecvResult.NOCOMMAND;
                }



            }

        }


        private sbyte GetByteFromByteArray(byte data)
        {
            string Fmt16 = Convert.ToString(data, 16);
            return (Convert.ToSByte(Fmt16, 16));
        }
    }

}
