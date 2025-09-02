using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Enum;
using ZH.Struct;

namespace ZH3501

{



    #region ZH3501多功能板联机指令
    /// <summary>
    /// 多功能板联机/脱机请求包
    /// </summary>
    internal class ZH3501_SendLinkRelayPacket : ZH3001SendPacket
    {
        public byte _byteID = new byte(); //ID
        public byte _byteRelayType = new byte();// 继电器状态


        // 0x880F—联机寄存器
        //发送：68H+RID+SID+LEN+13H+880FH+DATA+CS  （DATA=1bytes）
        //LEN：0x09
        //    DATA内容：00
        //       应答：68H+FEH+01H+09H+93H+880FH+4B+CS。表示在线。

        public ZH3501_SendLinkRelayPacket()
            : base()
        { }

     
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
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// 多功能板，联机返回指令
    /// </summary>
    internal class ZH3501_RequesttLinkRelayReplyPacket : ZH3001RecvPacket
    {
        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>

        //public override bool ParsePacket(byte[] buf)
        protected override void ParseBody(byte[] data)
        {
            //stStdInfo tagInfo = new stStdInfo();
            //ByteBuffer buf = new ByteBuffer(data);
            //if (data == null || data.Length != 8)
            //    ReciveResult = RecvResult.DataError;
            //else
            //{
            //    if (data[0] == 0x50)
            //        ReciveResult = RecvResult.OK;
            //    else
            //        ReciveResult = RecvResult.Unknow;
            //}
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
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion
    #region ZH3501多功能板控制指令
    /// <summary>
    /// ZH3501多功能板控制指令/脱机请求包
    /// </summary>
    internal class ZH3501_SendContonlRelayPacket : ZH3001SendPacket
    {
        public byte _byteID = new byte(); //ID
        public byte _byteRelayType = new byte();// 继电器状态
                                                //       发送：68H+RID+SID+LEN+13H+8800H+DATA+CS  （DATA=2bytes）
                                                //LEN：0x0A
                                                //DATA内容：01 00—2bytes， 第1路载波模块继电器断开
                                                //01 01---2bytes， 第1路载波模块继电器闭合
                                                //02 00—2bytes， 第2路载波模块继电器断开
                                                //02 01---2bytes， 第2路载波模块继电器闭合
                                                //03 00—2bytes， 第3路载波模块继电器断开
                                                //03 01---2bytes， 第3路载波模块继电器闭合
                                                //04 00—2bytes， 第4路载波模块继电器断开
                                                //04 01---2bytes， 第4路载波模块继电器闭合
                                                //FF 00---2bytes,  所有4路继电器都断开         //新增
                                                //FF 01---2bytes,  所有4路继电器都闭合         //新增


        public ZH3501_SendContonlRelayPacket()
            : base()
        { }

        public void SetPara(int id, int relayType)
        {

            _byteID = Convert.ToByte(id);
            _byteRelayType = Convert.ToByte(relayType);
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13);          //命令 
            buf.Put(0x88);
            buf.Put(0x00);

            buf.Put(_byteID);
            buf.Put(_byteRelayType);

            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// ZH3501多功能板控制指令返回指令
    /// </summary>
    internal class ZH3501_RequesttContonlRelayReplyPacket : ZH3001RecvPacket
    {
        /// <summary>
        /// 获取源信息
        /// </summary>
        /// <returns></returns>

        //public override bool ParsePacket(byte[] buf)
        protected override void ParseBody(byte[] data)
        {
            //stStdInfo tagInfo = new stStdInfo();
            //ByteBuffer buf = new ByteBuffer(data);
            //if (data == null || data.Length != 8)
            //    ReciveResult = RecvResult.DataError;
            //else
            //{
            //    if (data[0] == 0x50)
            //        ReciveResult = RecvResult.OK;
            //    else
            //        ReciveResult = RecvResult.Unknow;
            //}
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
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion
}
