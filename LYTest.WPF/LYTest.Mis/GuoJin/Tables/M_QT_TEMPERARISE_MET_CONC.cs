using System;

namespace LYTest.Mis.GuoJin
{
    //温升试验
    public class M_QT_TEMPERARISE_MET_CONC : M_QT_CONC_Basic
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
        /// 试验部位
        /// </summary>
        public string RUNNING_TYPE_CODE { get; set; }
        /// <summary>
        /// 测试值
        /// </summary>
        public string TEST_VALUE { get; set; }
        /// <summary>
        /// 限值
        /// </summary>
        public string VALUE_ABS { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public string TEMPERARISE { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public string DURATION { get; set; }
        /// <summary>
        /// 测试温度值
        /// </summary>
        public string TEST_TEMPERARISE { get; set; }
        /// <summary>
        /// 改变值
        /// </summary>
        public string CHANGE_THE_VALUE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}