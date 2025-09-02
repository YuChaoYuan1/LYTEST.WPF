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


    #region ZH1104功耗测试
    /// <summary>
    /// ZH1104功耗测试接收基类
    /// </summary>
    internal class ZH1104RecvPacket : CLT11RecvPacket
    {
        public ZH1104RecvPacket() : base(0xFE, 0xFE) { }
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// ZH1104功耗测试发送包基类
    /// </summary>
    internal class ZH1104SendPacket : CLT11SendPacket
    {
        public ZH1104SendPacket()
            : this(true) { }

        public ZH1104SendPacket(bool needReplay)
            : base(0xFE, 0xFE, needReplay) { }

        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    #endregion


    #region 零线电流
    /// <summary>
    /// 零线电流测试接收基类
    /// </summary>
    internal class ZeroCurrentRecvPacket : CLT11RecvPacket
    {
        public ZeroCurrentRecvPacket() : base(0xFE, 0x01) { }
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 零线电流测试发送包基类
    /// </summary>
    internal class ZeroCurrentSendPacket : CLT11SendPacket
    {
        public ZeroCurrentSendPacket()
            : this(true) { }

        public ZeroCurrentSendPacket(bool needReplay)
            : base(0xFE, 0x01, needReplay) { }

        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
