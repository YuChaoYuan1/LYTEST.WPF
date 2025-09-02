using LY3522.SocketModule;
using LY3522.SocketModule.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LY3522.Device
{
    #region 发送返回接口
    /// <summary>
    /// 发送命令
    /// </summary>
    internal class SendContrnlTypePacket : LYSendPacket
    {

        public byte ID = 0xFE;
        /// <summary>
        /// 功能码。用字符串然后解析成16进制
        /// </summary>
        public string Cmd = "";
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data;    
        /// <summary>
        /// 读还是写
        /// </summary>
        public byte ControlTyep = 0x10;
        /// <summary>
        /// 表位号
        /// </summary>
        public byte bwNum = 0xff;
        public SendContrnlTypePacket()
            : base()
        { }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="cmd">命令码1002\
        /// Bit8：0=上位机读取、1=控温板返回
        /// Bit7-1：0=无；
        /// 1 = 读取所有通道温度；
        /// 2 = 设置所有加热通道温度；
        /// 3 = 读取单通道温度；
        /// 4 = 设置单加热通道温度；
        /// 5 = 设置风扇；
        /// 6= 设置电磁锁；
        /// 7 = 设置指示灯；</param>
        /// <param name="data1">数据</param>
        public void SetPara(byte controlType,string cmd, byte[] data1)
        {
            Cmd = cmd;
            Data = data1;
            ControlTyep = controlType;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(ControlTyep);
            buf.Put(hexStrrinToToHexByte(Cmd)); //cmd1002
            buf.Put(Data);
            return buf.ToByteArray();
        }

        /// <summary>
        /// 16进制的字符串放到字节数组中
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] hexStrrinToToHexByte(string hexString)
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
        public object OutData ;
        protected override void ParseBody(byte[] data)
        {
            if (data == null )
                ReciveResult = RecvResult.DataError;
            else
            {
                ReciveResult = RecvResult.OK;
                if (data.Length > 12)
                {
                    ByteBuffer buf2 = new ByteBuffer(data);
                    buf2.Get();//读还是写
                    var cmd= buf2.GetUShort();//命令码
                    if (cmd == 0x1000)
                    {
                        int dataLen = data.Length / 5;
                        float[] value = new float[dataLen];
                        for (int i = 0; i < dataLen; i++)
                        {
                            int id = buf2.Get();
                            value[id-1] = buf2.GetInt() / 100;
                        }
                        OutData = value;
                    }
   
                }
            }
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
            MyID = 0x01;
        }

        public LYSendPacket(bool needReplay)
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
