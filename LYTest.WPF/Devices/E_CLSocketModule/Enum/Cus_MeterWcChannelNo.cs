using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace E_CLSocketModule.Enum
{
    /// <summary>
    /// 电能误差通道号
    /// </summary>
    public enum Cus_MeterWcChannelNo
    {
        /// <summary>
        /// 
        /// </summary>
        正向有功 = 0,
        /// <summary>
        /// 
        /// </summary>
        正向无功 = 2,
        /// <summary>
        /// 
        /// </summary>
        反向有功 = 1,
        /// <summary>
        /// 
        /// </summary>
        反向无功 = 3,
        /// <summary>
        /// 
        /// </summary>
        需量 = 4,
        /// <summary>
        /// 
        /// </summary>
        时钟 = 5
    }
}
