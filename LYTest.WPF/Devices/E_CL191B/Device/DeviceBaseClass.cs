using System;
using System.Collections.Generic;

using System.Text;

namespace CLOU
{

    #region CL191B时基源
    /// <summary>
    /// 时基源发送包基类
    /// </summary>
    internal class CL191BSendPacket : ClouSendPacket_CLT11
    {
        public CL191BSendPacket()
            : base()
        {
            ToID = 0x01;
            MyID = 0x07;
        }

        public CL191BSendPacket(bool needReplay)
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
    /// <summary>
    /// 191B 时基源接收基类
    /// </summary>
    internal class CL191BRecvPacket : ClouRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
