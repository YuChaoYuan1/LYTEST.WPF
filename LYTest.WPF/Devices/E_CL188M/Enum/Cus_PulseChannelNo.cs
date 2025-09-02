using System;
using System.Collections.Generic;
using System.Text;

namespace CLOU.Enum
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

    /// <summary>
    /// 多功能误差通道号
    /// </summary>
    public enum Cus_DgnWcChannelNo
    {
        /// <summary>
        /// 
        /// </summary>
        电能误差 = 0,
        /// <summary>
        /// 
        /// </summary>
        日计时脉冲 = 1,
        /// <summary>
        /// 
        /// </summary>
        需量脉冲 = 2
    }

    

    /// <summary>
    /// 光电头选择位
    /// </summary>
    public enum Cus_PulseType
    {
        /// <summary>
        /// 
        /// </summary>
        脉冲盒 = 0,
        /// <summary>
        /// 
        /// </summary>
        光电头 = 1
    }


}
