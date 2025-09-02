using System;
using System.Collections.Generic;
using System.Text;

namespace CLOU
{
    /// <summary>
    /// 台体 工作流状态
    /// </summary>
    public enum WorkFlow
    {
        None,
        Unknow,
        YuRe,
        QiDong,
        QianDong,
        DuiSeBiao,
        JiBenWC,
        ZouZi,
        XLZQWC,
        Dgn,
        NaiYa
    }


    #region =========== 载波类型枚举 ===========
    /// <summary>
    /// 载波类型枚举
    /// </summary>
    public enum Cus_CarrierType
    {
        /// <summary>
        /// 非载波
        /// </summary>
        Null = 0,
        /// <summary>
        /// 晓程
        /// XiaoCheng
        /// </summary>
        晓程 = 1,
        /// <summary>
        /// 东软
        /// Neusoft
        /// </summary>
        东软 = 2,
        /// <summary>
        /// 鼎信
        /// DingXin
        /// </summary>
        鼎信 = 3,
        /// <summary>
        /// 瑞斯康
        /// RiseComm
        /// </summary>
        瑞斯康 = 4
    }
    #endregion
}
