using System;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 交流电压试验结论
    /// </summary>
    public class M_QT_ACVOLTAGE_MET_CONC : M_QT_CONC_Basic
    {
        /// <summary>
        /// 测试耐压值 （伏）
        /// </summary>
        public string VOLT_TEST_VALUE { get; set; }

        /// <summary>
        /// 耐压对象
        /// </summary>
        public string VOLT_OBJ { get; set; }

        /// <summary>
        /// 漏电流阀值
        /// </summary>
        public string LEAK_CURRENT_LIMIT { get; set; }

        /// <summary>
        /// 表位漏电流阀值
        /// </summary>
        public string POSITION_LEAK_LIMIT { get; set; }

        /// <summary>
        /// 漏电流
        /// </summary>
        public string LEAK_CURRENT { get; set; }

        /// <summary>
        /// 试验结果（无用了）
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}