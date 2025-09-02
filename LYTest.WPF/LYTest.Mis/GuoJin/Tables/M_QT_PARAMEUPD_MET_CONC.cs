using System;

namespace LYTest.Mis.GuoJin
{
    //电能表参数更新
    public class M_QT_PARAMEUPD_MET_CONC : M_QT_CONC_Basic
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
        /// 试验分项
        /// </summary>
        public string TEST_SUB { get; set; }
        /// <summary>
        /// 检测项目
        /// </summary>
        public string TEST_ITEMS { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

    }
}