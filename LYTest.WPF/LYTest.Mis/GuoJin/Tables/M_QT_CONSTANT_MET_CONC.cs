using System;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 电能表常数试验表
    /// </summary>
    public class M_QT_CONSTANT_MET_CONC : M_QT_CONC_Basic
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
        /// 起始度数
        /// </summary>
        public string IR_START_READING { get; set; }

        /// <summary>
        /// 结束度数
        /// </summary>
        public string IR_END_READING { get; set; }

        /// <summary>
        /// 走字度数
        /// </summary>
        public string IR_READING { get; set; }

        /// <summary>
        /// 电能表常数
        /// </summary>
        public string METER_CONST_CODE { get; set; }

        /// <summary>
        /// 电能表位数
        /// </summary>
        public string METER_DIGITS { get; set; }

        /// <summary>
        /// 走字脉冲数
        /// </summary>
        public string IR_PULES { get; set; }

        /// <summary>
        /// 示值误差
        /// </summary>
        public string AR_TS_READING_ERR { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }

        /// <summary>
        /// 误差下限
        /// </summary>
        public string ERR_DOWN { get; set; }

        /// <summary>
        /// 控制方式
        /// </summary>
        public string CONTROL_METHOD { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        public string FEE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}