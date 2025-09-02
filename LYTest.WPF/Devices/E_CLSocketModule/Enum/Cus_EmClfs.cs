using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace E_CLSocketModule.Enum
{
    /// <summary>
    /// 测量方式
    /// 
    ///   三相四线 = 0,
    ///   三相三线 = 1,
    ///   二元件跨相90 = 2,
    ///   二元件跨相60 = 3,
    ///   三元件跨相90 = 4,
    ///   单相 = 5,
    ///   
    /// </summary>
    [ComVisible(true)]
    public enum Cus_EmClfs
    {
        /// <summary>
        /// 测量方式-三相四线
        /// </summary>
        PT4 = 0,
        /// <summary>
        /// 测量方式-三相三线
        /// </summary>
        PT3 = 1,        
        /// <summary>
        /// 测量方式-二元件跨相90
        /// </summary>
        EK90 = 2,
        /// <summary>
        /// 测量方式-二元件跨相60
        /// </summary>
        EK60 = 3,
        /// <summary>
        /// 测量方式-三元件跨相90
        /// </summary>
        SK90 = 4,
        /// <summary>
        /// 测量方式-单相
        /// </summary>
        P = 5
    }
}
