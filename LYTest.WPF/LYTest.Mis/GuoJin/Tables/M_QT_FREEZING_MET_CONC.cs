using System;

namespace LYTest.Mis.GuoJin
{
    //冻结功能试验
    public class M_QT_FREEZING_MET_CONC : M_QT_CONC_Basic
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
        /// 冻结值
        /// </summary>
        public string READ_VALUE { get; set; }


        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 冻结类别（原始记录）
        /// </summary>
        public string FREE_CATEG { get; set; }

        /// <summary>
        /// 冻结方式（原始记录）
        /// </summary>
        public string FREE_MODE { get; set; }

        /// <summary>
        /// 冻结前电流（原始记录）
        /// </summary>
        public string FREE_BEFORE_CURRENT { get; set; }

        /// <summary>
        /// 冻结时电流（原始记录）
        /// </summary>
        public string FREEING_CURRENT { get; set; }

        /// <summary>
        /// 冻结后电流（原始记录）
        /// </summary>
        public string FREE_AFTER_CURRENT { get; set; }
    }
}