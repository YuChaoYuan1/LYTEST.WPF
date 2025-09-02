using System;

namespace LYTest.Mis.GuoJin
{
    //变差要求试验
    public class M_QT_ERROR_MET_CONC : M_QT_CONC_Basic
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
        /// 一次误差
        /// </summary>
        public string ONCE_ERR { get; set; }

        /// <summary>
        /// 一次误差平均值
        /// </summary>
        public string AVG_ONCE_ERR { get; set; }

        /// <summary>
        /// 一次误差化整值
        /// </summary>
        public string INT_ONCE_ERR { get; set; }

        /// <summary>
        /// 二次误差
        /// </summary>
        public string SENC_ERR { get; set; }

        /// <summary>
        /// 二次误差平均值
        /// </summary>
        public string AVG_SENC_ERR { get; set; }

        /// <summary>
        /// 二次误差化整值
        /// </summary>
        public string INT_SENC_ERR { get; set; }

        /// <summary>
        /// 脉冲数
        /// </summary>
        public string PULES { get; set; }

        /// <summary>
        /// 变差
        /// </summary>
        public string VARIA_ERR { get; set; }

        /// <summary>
        /// 变差取整
        /// </summary>
        public string INT_VARIA_ERR { get; set; }

        /// <summary>
        /// 变差限
        /// </summary>
        public string VALUE_ABS { get; set; }

        /// <summary>
        /// 误差
        /// </summary>
        public string ERROR { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}