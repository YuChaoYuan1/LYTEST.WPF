using System;

namespace LYTest.Mis.GuoJin
{
    //计度器总电能示值误差试验
    public class M_QT_VSAGANDSHORT_MET_CONC : M_QT_CONC_Basic
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
        /// 尖
        /// </summary>
        public string FEE1_VALUE { get; set; }
        /// <summary>
        /// 峰
        /// </summary>
        public string FEE2_VALUE { get; set; }
        /// <summary>
        /// 平
        /// </summary>
        public string FEE3_VALUE { get; set; }
        /// <summary>
        /// 谷
        /// </summary>
        public string FEE4_VALUE { get; set; }
        /// <summary>
        /// 费率
        /// </summary>
        public string FEE { get; set; }
        /// <summary>
        /// 各分时电量之和
        /// </summary>
        public string TIME_ELECT_SUM { get; set; }
        /// <summary>
        /// 总电量
        /// </summary>
        public string ELECT_SUM { get; set; }
        /// <summary>
        /// 误差限
        /// </summary>
        public string VALUE_ERR_ABS { get; set; }
        /// <summary>
        /// 误差
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
        /// 示值误差
        /// </summary>
        public string VALUE_ERR { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 总次数
        /// </summary>
        public string SWIT_TIMES { get; set; }
        /// <summary>
        /// 尖次数
        /// </summary>
        public string FEE1_TIMES { get; set; }
        /// <summary>
        /// 峰次数
        /// </summary>
        public string FEE2_TIMES { get; set; }
        /// <summary>
        /// 平次数
        /// </summary>
        public string FEE3_TIMES { get; set; }
        /// <summary>
        /// 谷次数
        /// </summary>
        public string FEE4_TIMES { get; set; }
    }
}