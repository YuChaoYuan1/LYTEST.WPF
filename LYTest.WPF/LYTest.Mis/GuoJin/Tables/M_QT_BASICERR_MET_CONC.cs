using System;

namespace LYTest.Mis.GuoJin
{
    //基本误差试验表
    public class M_QT_BASICERR_MET_CONC : M_QT_CONC_Basic
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
        /// 误差化整值
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }

        /// <summary>
        /// 误差限值
        /// </summary>
        public string ERR_ABS { get; set; }

        /// <summary>
        /// 实际误差
        /// </summary>
        public string ERROR { get; set; }


        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 误差平均值
        /// </summary>
        public string AVE_ERR { get; set; }

    }
}