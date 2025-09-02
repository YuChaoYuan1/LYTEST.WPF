using LY.SocketModule;
using LY.SocketModule.Packet;
using System;

namespace LY.Device
{
    #region 发送返回接口
    /// <summary>
    /// 发送命令
    /// </summary>
    internal class SendContrnlTypePacket : LYSendPacket
    {
        /// <summary>
        /// 功能码。用字符串然后解析成16进制
        /// </summary>
        public string Cmd = "";
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data;

        public byte ControlTyep = 0x10;
        public SendContrnlTypePacket()
            : base()
        { }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="contrnlType">控制类型，读还是写0x10,0x13</param>
        /// <param name="cmd">命令码1002</param>
        /// <param name="bwNum">表位号</param>
        /// <param name="data1">数据</param>
        public void SetPara(string cmd, byte[] data1)
        {

            Cmd = cmd;
            Data = data1;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13); //cmd1002
            buf.Put(HexStrrinToToHexByte(Cmd)); //cmd1002
            buf.Put(Data);
            return buf.ToByteArray();
        }

        /// <summary>
        /// 16进制的字符串放到字节数组中
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] HexStrrinToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString = hexString.Insert(hexString.Length - 1, 0.ToString());
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
    /// <summary>
    /// 返回命令
    /// </summary>
    internal class RecvContrnlTypeReplyPacket : RecvPacketBase
    {
        //public object OutData ;
        protected override void ParseBody(byte[] data)
        {
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                switch (data.Length)
                {
                    case 16:
                        ReciveResult = RecvResult.OK;
                        break;
                    case 9:
                        if (data[4] == 0x4B)
                        {
                            ReciveResult = RecvResult.OK;
                        }
                        else
                        {
                            ReciveResult = RecvResult.OrderFail;
                        }
                        break;
                    default:
                        ReciveResult = RecvResult.Unknow;
                        break;
                }

            }
        }
    }

    #endregion


    #region 读取数据
    /// <summary>
    /// 发送命令
    /// </summary>
    internal class LY2001_ReadSendPacket : LYSendPacket
    {
        /// <summary>
        /// 功能码。用字符串然后解析成16进制
        /// </summary>
        public string Cmd = "";
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data;

        public LY2001_ReadSendPacket()
            : base()
        { }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="cmd">命令码1002</param>
        /// <param name="data">数据</param>
        public void SetPara(string cmd, byte[] data)
        {

            Cmd = cmd;
            Data = data;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10); //读
            buf.Put(HexStrrinToToHexByte(Cmd)); //cmd1002
            if (Data.Length > 0)
                buf.Put(Data);
            return buf.ToByteArray();
        }

        /// <summary>
        /// 16进制的字符串放到字节数组中
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] HexStrrinToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString = hexString.Insert(hexString.Length - 1, 0.ToString());

            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
    /// <summary>
    /// 返回命令
    /// </summary>
    internal class LY2001_ReadReplyPacket : RecvPacketBase
    {
        public byte[] Data { get; private set; }

        protected override void ParseBody(byte[] data)
        {
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x90)
                {
                    Data = new byte[data.Length - 1];
                    Array.Copy(data, 1, Data, 0, Data.Length);
                    ReciveResult = RecvResult.OK;
                }
                else
                    ReciveResult = RecvResult.OrderFail;
            }
        }
    }

    #endregion



    #region 读版本

    internal class ZH1104_RequestVersionPacket : LYSendPacket
    {
        public bool IsLink = true;

        public ZH1104_RequestVersionPacket(int bw)
            : base()
        {
            this.ToID = Convert.ToByte(bw);
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);
            buf.Put(0x30);
            buf.Put(0x02);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            return "读版本";
        }
    }

    internal class ZH1104_RequestVersionReplyPacket : LYRecvPacket
    {
        public string Version { get; private set; }
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 10)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[4] == 0x90)
                {
                    Version = BitConverter.ToString(data, 7, 6).Replace("-", "");
                    ReciveResult = RecvResult.OK;
                }
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override bool ParsePacket(byte[] data)
        {
            if (data == null || data.Length < 10)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[4] == 0x90)
                {
                    Version = BitConverter.ToString(data, 7, 6).Replace("-", "");
                    ReciveResult = RecvResult.OK;
                }
                else
                    ReciveResult = RecvResult.Unknow;
            }

            return true;
        }
        public override string GetPacketResolving()
        {
            return $"返回：{Version}";
        }
    }
    #endregion

    #region 发送返回基类
    /// <summary>
    ///  源发送包基类
    /// </summary>
    internal class LYSendPacket : SendPacketBase
    {
        public LYSendPacket()
            : base()
        {
            ToID = 0x01;
            MyID = 0xFE;
        }

        public LYSendPacket(bool needReplay)
            : base(needReplay)
        {
            ToID = 0x01;
            MyID = 0x01;
        }


        public void SetID(byte toID, byte myID)
        {
            ToID = toID;
            MyID = myID;
        }
        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 接收基类
    /// </summary>
    internal class LYRecvPacket : RecvPacketBase
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
