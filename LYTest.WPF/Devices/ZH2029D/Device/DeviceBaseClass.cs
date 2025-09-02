using System;
using System.Collections.Generic;

using System.Text;

namespace CLOU
{

    #region CL2029D

    /// <summary>
    /// 2029D 多功能控制板接收基类
    /// </summary>
    internal class CL2029DRecvPacket : ClouRecvPacket_CLT11
    {
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 2029D多功能控制板发送包基类
    /// </summary>
    internal class CL2029DSendPacket : ClouSendPacket_CLT11
    {
        public CL2029DSendPacket()
            : base()
        {
            ToID = 0x22;
            MyID = 0x01;
        }

        public CL2029DSendPacket(bool needReplay)
            : base(needReplay)
        {
            ToID = 0x22;
            MyID = 0x01;
        }

        protected override byte[] GetBody()
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
