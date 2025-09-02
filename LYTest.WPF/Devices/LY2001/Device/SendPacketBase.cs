using LY.SocketModule;
using LY.SocketModule.Packet;
using System;

namespace LY.Device
{
    internal abstract class SendPacketBase : SendPacket
    {

        /// <summary>
        /// 包头
        /// </summary>
        protected byte PacketHead = 0x68;
        /// <summary>
        /// 发信节点
        /// </summary>
        public byte MyID = 0xFE;
        /// <summary>
        /// 受信节点
        /// </summary>
        protected byte ToID = 0xFF;
        //0xFE, 0x13

        public SendPacketBase() { IsNeedReturn = true; }
        public SendPacketBase(bool needReplay) { IsNeedReturn = needReplay; }

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
            byte packetLength = (byte)(body.Length + 5);//帧头4字节+CS一字节
            buf.Put(packetLength);      //帧长度
            buf.Put(body);              //数据域 
            byte chkSum = GetChkSum(buf.ToByteArray());
            buf.Put(chkSum);
            return buf.ToByteArray();
        }

        protected abstract byte[] GetBody();


        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        protected byte GetChkSum(byte[] bytData)
        {
            byte bytChkSum = 0x00;
            for (int int_Inc = 1; int_Inc < bytData.Length; int_Inc++)
            {
                bytChkSum ^= bytData[int_Inc];
            }
            return bytChkSum;
        }

        /// <summary>
        /// 二进制字符串转换成16进制Hex
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte Str2ToByte(string str2)
        {
            int num = Convert.ToInt32(str2, 2);
            return Convert.ToByte(num);
        }
    }

    /// <summary>
    ///接收数据包基类
    /// </summary>
    internal abstract class RecvPacketBase : RecvPacket
    {
        /// <summary>
        /// 包头
        /// </summary>
        protected byte PacketHead = 0x81;
        /// <summary>
        /// 发信节点
        /// </summary>
        protected byte MyID = 0x80;
        /// <summary>
        /// 受信节点
        /// </summary>
        protected byte ToID = 0x10;
        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="buf">缓冲区接收到的数据包内容</param>
        /// <returns>解析是否成功</returns>
        public override bool ParsePacket(byte[] buf)
        {
            if (buf.Length < 9) return false;

            //第一步，验证包长度
            //第二步，验证包结构
            //第三步，拆帧
            ByteBuffer pack = new ByteBuffer(buf);
            int iLocalSum = 1;
            PacketHead = pack.Get();
            ToID = pack.Get();
            MyID = pack.Get();
            byte dataLength = pack.Get();
            if (buf.Length < dataLength || dataLength < 5) return false;
            byte[] data = pack.GetByteArray(dataLength - 5);
            pack.Get();

            while (buf[dataLength - iLocalSum] == 0)
            {
                iLocalSum++;
            }
            //计算校验码
            GetChkSum(buf, 1, dataLength - iLocalSum);


            //if (chkCode != chkSum) return false;
            ParseBody(data);
            return true;
        }
        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        protected byte GetChkSum(byte[] bytData, int startPos, int length)
        {
            byte bytChkSum = 0;
            for (int int_Inc = startPos; int_Inc < length; int_Inc++)
            {
                bytChkSum ^= bytData[int_Inc];
            }
            return bytChkSum;
        }
        /// <summary>
        /// 解析数据域
        /// </summary>
        /// <param name="data">数据域</param>
        protected abstract void ParseBody(byte[] data);


        /// <summary>
        /// 单个字节由低位向高位取值，
        /// </summary>
        /// <param name="input">单个字节</param>
        /// <param name="index">起始0,1,2..7</param>
        /// <returns></returns>
        protected int GetbitValue(byte input, int index)
        {
            int value;
            value = index > 0 ? input >> index : input;
            return value &= 1;
        }

        /// <summary>
        /// 3字节转换为Float
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="dotLen"></param>
        /// <returns></returns>
        protected float Get3ByteValue(byte[] bytData, int dotLen)
        {
            float data = bytData[0] << 16;
            data += bytData[1] << 8;
            data += bytData[2];

            data = (float)(data / Math.Pow(10, dotLen));
            return data;
        }

        ///<summary>
        /// 替换byteSource目标位的值
        ///</summary>
        ///<param name="byteSource">源字节</param>
        ///<param name="location">替换位置(0-7)</param>
        ///<param name="value">替换的值(1-true,0-false)</param>
        ///<returns>替换后的值</returns>
        protected byte ReplaceTargetBit(byte byteSource, short location, bool value)
        {
            byte baseNum = (byte)(Math.Pow(2, location + 1) / 2);
            return ReplaceTargetBit(location, value, byteSource, baseNum);
        }

        ///<summary>
        /// 替换byteSource目标位的值
        ///</summary>
        ///<param name="location"></param>
        ///<param name="value">替换的值(1-true,0-false)</param>
        ///<param name="byteSource"></param>
        ///<param name="baseNum">与 基数(1,2,4,8,16,32,64,128)</param>
        ///<returns></returns>
        private byte ReplaceTargetBit(short location, bool value, byte byteSource, byte baseNum)
        {
            bool locationValue = GetbitValue(byteSource, location) == 1;
            if (locationValue != value)
            {
                return (byte)(value ? byteSource + baseNum : byteSource - baseNum);
            }
            return byteSource;
        }
    }
}
