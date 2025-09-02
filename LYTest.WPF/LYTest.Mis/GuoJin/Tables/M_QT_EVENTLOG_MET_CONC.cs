using System;

namespace LYTest.Mis.GuoJin
{
    //事件记录试验
    public class M_QT_EVENTLOG_MET_CONC : M_QT_CONC_Basic
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
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 事件前后（原始记录）
        /// </summary>
        public string EVENT_ABOUT { get; set; }

        /// <summary>
        /// 事件状态（原始记录）
        /// </summary>
        public string EVENT_TYPE { get; set; }

        /// <summary>
        /// 事件记录发生时刻（原始记录）
        /// </summary>
        public string EVENT_START_TIME { get; set; }

        /// <summary>
        /// 总次数（原始记录）
        /// </summary>
        public string TOTAL { get; set; }

        /// <summary>
        /// 事件记录结束时刻（原始记录）
        /// </summary>
        public string EVENT_END_TIME { get; set; }
    }
}