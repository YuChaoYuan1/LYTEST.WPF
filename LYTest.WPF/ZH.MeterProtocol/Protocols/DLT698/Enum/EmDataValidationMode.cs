using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /// <summary>
    /// 数据验证模式
    /// </summary>
    public enum EmDataValidationMode
    {
        /// <summary>
        ///数据验证码
        /// </summary>
        SID_MAC = 0,
        /// <summary>
        /// 随机数
        /// </summary>
        RN = 1,
        /// <summary>
        /// 随机数+数据MAC 
        /// </summary>
        RN_MAC = 2,
        /// <summary>
        /// 安全标识  
        /// </summary>
        SID = 3

    }
}
