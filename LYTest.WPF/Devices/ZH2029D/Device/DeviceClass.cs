using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CLOU.SocketModule.Packet;
using CLOU;
using CLOU.Enum;
using CLOU.Struct;

namespace CLOU
{

    #region CL2029D切换继电器命令2013-10-31
    /// <summary>
    /// 2029D切换继电器请求包
    /// </summary>
    internal class CL2029D_RequestSetSwitchPacket : CL2029DSendPacket
    {
        public bool IsLink = true;
        /// <summary>
        /// 继电器ID
        /// </summary>
        private int iRelayID = 0;
        /// <summary>
        /// 控制类型0,断开；1，闭合
        /// </summary>
        private int iControlType = 0;

        public CL2029D_RequestSetSwitchPacket()
            : base(true)
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iID">继电器ID</param>
        /// <param name="iType">继电器控制类型0，断开；1，闭合</param>
        public void SetPara(int iID, int iType)
        {
            this.iRelayID = iID;
            this.iControlType = iType;
        }
        /*
         * 81 22 01 16 84 FF 0C FF FF FF FF FF FF FF FF FF FF FF FF 00 00 CS
         */
        protected override byte[] GetBody()
        {
            byte[] byt_List = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x84);          //命令 
            buf.Put(0xFF);
            buf.Put(0x0C);
            buf.Put(byt_List);

            buf.Put(Convert.ToByte(iRelayID));
            buf.Put(Convert.ToByte(iControlType));

            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 2029D多功能控制板切换继电器返回指令
    /// </summary>
    internal class CL2029D_RequestSetSwitchReplyPacket : CL2029DRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 19)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x85)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }



    #endregion

}
