using System;

namespace LYTest.Mis.GuoJin
{
    //电能表电流回路阻抗试验
    public class M_QT_IMPCURRENT_MET_CONC : M_QT_CONC_Basic
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
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }

        /// <summary>
        /// 阻抗值
        /// </summary>
        public string IMPEDAN_VALUES { get; set; }
        /// <summary>
        /// 阻抗平均值
        /// </summary>
        public string IMPEDAN_VALUES_AVG { get; set; }
        /// <summary>
        /// 阻抗取整值
        /// </summary>
        public string IMPEDAN_VALUES_INT { get; set; }
        /// <summary>
        /// 阻抗限值
        /// </summary>
        public string IMPEDAN_VALUES_ABS { get; set; }
        /// <summary>
        /// 检测值
        /// </summary>
        public string THE_READINGS { get; set; }
        /// <summary>
        /// 修约值
        /// </summary>
        public string REVISED_VALUE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 电压值
        /// </summary>
        public string VOLTAGE { get; set; }
        /// <summary>
        /// 电流值
        /// </summary>
        public string CURRENT { get; set; }

    }
}