using System;

namespace LYTest.Mis.GuoJin
{
    //射频电磁场抗扰度试验
    public class M_QT_RFINDUCTION_MET_CONC : M_QT_CONC_Basic
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
        ///误差1
        /// </summary>
        public string ERROR1 { get; set; }
        /// <summary>
        ///误差2
        /// </summary>
        public string ERROR2 { get; set; }
        /// <summary>
        ///误差平均值
        /// </summary>
        public string AVE_ERR { get; set; }
        /// <summary>
        ///误差取整ERR_ABS
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }
        /// <summary>
        ///误差限
        /// </summary>
        public string ERR_ABS { get; set; }
        /// <summary>
        ///相对误差（原始记录）
        /// </summary>
        public string INT_VARIA_ERR { get; set; }
        /// <summary>
        ///误差|
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        ///射频电磁场抗扰度频率带
        /// </summary>
        public string RF_FIELD { get; set; }
    }
}