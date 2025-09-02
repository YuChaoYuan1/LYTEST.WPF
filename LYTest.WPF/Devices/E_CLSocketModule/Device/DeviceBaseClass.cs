using System;
using System.Collections.Generic;

using System.Text;

namespace E_CLSocketModule
{
    #region  303
    /// <summary>
    /// 303 控源设备 接收包基类
    /// </summary>
    abstract class CL303RecvPacket : ClouRecvPacket_NotCltTwo
    {
    }
    /// <summary>
    /// 
    /// </summary>
    abstract class CL303SendPacket : ClouSendPacket_NotCltTwo
    {
        public CL303SendPacket()
            : base(true, 0x20)
        {

        }
    }
    #endregion 


}
