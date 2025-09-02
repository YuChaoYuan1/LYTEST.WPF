using System;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 由电源供电的时钟试验试验表
    /// </summary>
    public class M_QT_DAYTIME_MET_CONC : M_QT_CONC_Basic
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
        /// 电能表常数
        /// </summary>
        public string METER_CONST_CODE { get; set; }

        /// <summary>
        /// 时基频率（秒脉冲频率）
        /// </summary>
        public string TIME_FREQUENCY { get; set; }

        /// <summary>
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }

        /// <summary>
        /// 实际值
        /// </summary>
        public string ACTUER_VALUE { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public string AVE_VALUE { get; set; }

        /// <summary>
        /// 误差化整值
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }

        /// <summary>
        /// 误差限
        /// </summary>
        public string VALUE_ABS { get; set; }

        /// <summary>
        /// 是否预热
        /// </summary>
        public string CLEAR_DATA_RST { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}