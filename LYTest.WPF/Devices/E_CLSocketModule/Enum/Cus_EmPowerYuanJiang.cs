using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace E_CLSocketModule.Enum
{
    /// <summary>
    /// 功率元件
    /// </summary>
    [ComVisible(true)]
    public enum Cus_EmPowerYuanJiang
    {
        /// <summary>
        /// 错误的、未赋值的
        /// 
        /// </summary>
        Error = 0,

        /// <summary>
        /// 合元
        /// </summary>
        H = 1 ,

        /// <summary>
        /// A元
        /// </summary>
        A = 2,

        /// <summary>
        /// B元
        /// </summary>
        B = 3,

        /// <summary>
        /// C元
        /// </summary>
        C = 4,
    }
}
