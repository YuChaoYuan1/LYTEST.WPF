using System;
using ZH;

namespace CLOU
{

    #region CL309
    /// <summary>
    /// 309 功率源接收基类
    /// </summary>
    internal class CL309RecvPacket : ClouRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 309功率源发送包基类
    /// </summary>
    internal class CL309SendPacket : ClouSendPacket_CLT11
    {
        public CL309SendPacket()
            : base()
        {
            ToID = 0x01;
            MyID = 0x07;
        }

        public CL309SendPacket(bool needReplay)
            : base(needReplay)
        {
            ToID = 0x01;
            MyID = 0x07;
        }

        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
