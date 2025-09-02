using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace E_CLSocketModule.Enum
{
    #region 功率方向
    /// <summary>
    /// 功率方向
    /// </summary>
    [ComVisible(true)]
    public enum Cus_EmPowerFangXiang
    {
        /// <summary>
        /// 组合有功
        /// </summary>
        ZHP = 0,

        /// <summary>
        /// 正向有功
        /// </summary>
        ZXP = 1,

        /// <summary>
        /// 反向有功
        /// </summary>
        FXP = 2,

        /// <summary>
        /// 正向无功
        /// </summary>
        ZXQ = 3,

        /// <summary>
        /// 反向无功
        /// </summary>
        FXQ = 4,

        /// <summary>
        /// 第一象限无功
        /// </summary>
        Q1 = 5,

        /// <summary>
        /// 第二象限无功
        /// </summary>
        Q2 = 6,

        /// <summary>
        /// 第三象限无功
        /// </summary>
        Q3 = 7,

        /// <summary>
        /// 第四象限无功
        /// </summary>
        Q4 = 8,

        /// <summary>
        /// 错误的、未赋值的
        /// </summary>
         Error= 9,

    }
    #endregion

}
