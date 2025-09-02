using System;
 

namespace ZH
{

    #region ZH3001源
    /// <summary>
    ///  源发送包基类
    /// </summary>
    internal class ZH3001SendPacket : ZHSendPacket_CLT11
    {
        public ZH3001SendPacket()
            : base()
        {
            ToID = 0x13;
            MyID = 0xFE;
        }

        public ZH3001SendPacket(bool needReplay)
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
    /// ZH3001 源接收基类
    /// </summary>
    internal class ZH3001RecvPacket : ZHRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
