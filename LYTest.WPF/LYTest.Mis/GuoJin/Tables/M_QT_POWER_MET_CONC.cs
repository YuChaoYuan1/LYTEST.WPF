using System;

namespace LYTest.Mis.GuoJin
{
    //电能表功耗试验表
    public class M_QT_POWER_MET_CONC : M_QT_CONC_Basic
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
        /// 功率
        /// </summary>
        public string ACT_POWER { get; set; }
        /// <summary>
        /// 功率误差限
        /// </summary>
        public string ACT_POWER_ERR { get; set; }
        /// <summary>
        /// 试验线路
        /// </summary>
        public string TEST_LINE { get; set; }
        /// <summary>
        /// 功率类型
        /// </summary>
        public string POWER_TYPE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}