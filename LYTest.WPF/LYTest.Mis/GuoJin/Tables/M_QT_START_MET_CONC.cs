using System;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 起动试验
    /// </summary>
    public class M_QT_START_MET_CONC : M_QT_CONC_Basic
    {
        /// <summary>
        /// 读取标识
        /// </summary>
        public string READ_ID { get; set; }
        
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
        /// 起动电流
        /// </summary>
        public string START_CURRENT { get; set; }

        /// <summary>
        /// 起动时间
        /// </summary>
        public string START_TIME { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}