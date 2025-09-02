using System;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 需量示值误差试验
    /// </summary>
    public class M_QT_DEMANDERR_MET_CONC: M_QT_CONC_Basic
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
        /// 试验时间
        /// </summary>
        public string TEST_TIME { get; set; }

        /// <summary>
        /// 需量周期时间
        /// </summary>
        public string DEMAND_PERIOD { get; set; }

        /// <summary>
        /// 滑差时间
        /// </summary>
        public string DEMAND_TIME { get; set; }

        /// <summary>
        /// 滑差次数
        /// </summary>
        public string DEMAND_INTERVAL { get; set; }

        /// <summary>
        /// 实际需量
        /// </summary>
        public string REAL_DEMAND { get; set; }

        /// <summary>
        /// 实际周期
        /// </summary>
        public string REAL_PERIOD { get; set; }

        /// <summary>
        /// 需量示值误差
        /// </summary>
        public string DEMAND_VALUE_ERR { get; set; }

        /// <summary>
        /// 标准表需量值
        /// </summary>
        public string DEMAND_STANDARD { get; set; }

        /// <summary>
        /// 需量示值误差限
        /// </summary>
        public string DEMAND_ERR_ABS { get; set; }

        /// <summary>
        /// 需量清零结果
        /// </summary>
        public string CLEAR_DATA_RST { get; set; }

        /// <summary>
        /// 示值误差结论
        /// </summary>
        public string VALUE_CONC_CODE { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public string TEMPERATURE { get; set; }

        /// <summary>
        /// 控制方法
        /// </summary>
        public string CONTROL_METHOD { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }

        /// <summary>
        /// 误差下限
        /// </summary>
        public string ERR_DOWM { get; set; }
    }
}