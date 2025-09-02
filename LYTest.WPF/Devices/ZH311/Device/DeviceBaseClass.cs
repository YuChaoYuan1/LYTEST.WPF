using System;
 

namespace ZH
{

    #region ZH311源
    /// <summary>
    ///  源发送包基类
    /// </summary>
    internal class ZH311SendPacket : ZHSendPacket_CLT11
    {
        public ZH311SendPacket()
            : base()
        {
            ToID = 0x01;
            MyID = 0x01;
        }

        public ZH311SendPacket(bool needReplay)
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
    /// ZH311 源接收基类
    /// </summary>
    internal class ZH311RecvPacket : ZHRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
