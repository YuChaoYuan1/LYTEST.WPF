using System;
using System.Collections.Generic;

using System.Text;

namespace CLOU
{

    #region CL2029B
    /// <summary>
    /// 2029B发送包基类
    /// </summary>
    internal class CL2029BSendPacket : ClouSendPacket_CLT11
    {
        public CL2029BSendPacket()
            : base()
        {
            ToID = 0x42;
            MyID = 0x01;
        }

        public CL2029BSendPacket(bool needReplay)
            : base(needReplay)
        {
            ToID = 0x42;
            MyID = 0x01;
        }

        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 2029B 接收基类
    /// </summary>
    internal class CL2029BRecvPacket : ClouRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
