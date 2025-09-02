using System;

namespace LYTest.Mis.GuoJin
{
    //测量重复性试验
    public class M_QT_MEASREPE_MET_CONC : M_QT_CONC_Basic
    {
        /// <summary>
        /// 负载电流
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }
        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }
        /// <summary>
        /// 电流相别
        /// </summary>
        public string IABC { get; set; }
        /// <summary>
        /// 频率
        /// </summary>
        public string FREQ { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 限制
        /// </summary>
        public string VALUE_ABS { get; set; }
        /// <summary>
        /// 偏差化整值
        /// </summary>
        public string INT_DEVIATION_ERR { get; set; }
        /// <summary>
        /// 实际误差
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        /// 误差平均值
        /// </summary>
        public string AVE_ERR { get; set; }
        /// <summary>
        /// 误差化整值
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }
        /// <summary>
        /// 标准偏差
        /// </summary>
        public string DEVIATION_LIMT { get; set; }
        /// <summary>
        /// 允许值
        /// </summary>
        public string PERMISSIBLE_VALUE { get; set; }
        /// <summary>
        /// 偏差值
        /// </summary>
        public string DEVIATION_ERR { get; set; }
        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

    }
}