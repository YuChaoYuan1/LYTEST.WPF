using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZH.Rx_31.Enum
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
    public enum WireMode
    {
        /// <summary>
        /// 测量方式-三相四线
        /// </summary>
        三相四线 = 0,
        /// <summary>
        /// 测量方式-三相三线
        /// </summary>
        三相三线 = 1,
        /// <summary>
        /// 测量方式-二元件跨相90
        /// </summary>
        二元件跨相90 = 2,
        /// <summary>
        /// 测量方式-二元件跨相60
        /// </summary>
        二元件跨相60 = 3,
        /// <summary>
        /// 测量方式-三元件跨相90
        /// </summary>
        三元件跨相90 = 4,
        /// <summary>
        /// 测量方式-单相
        /// </summary>
        单相 = 5
    }
}
