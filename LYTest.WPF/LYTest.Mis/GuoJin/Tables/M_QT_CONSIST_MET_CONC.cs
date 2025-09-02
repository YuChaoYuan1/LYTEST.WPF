namespace LYTest.Mis.GuoJin
{
    //误差一致性试验
    public class M_QT_CONSIST_MET_CONC : M_QT_CONC_Basic
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
        /// 脉冲数
        /// </summary>
        public string PULES { get; set; }

        /// <summary>
        /// 误差值
        /// </summary>
        public string ERR_VALUE { get; set; }

        /// <summary>
        /// 误差值
        /// </summary>
        public string ERROR { get; set; }

        /// <summary>
        /// 所有表位平均误差
        /// </summary>
        public string ALL_AVG_ERROR { get; set; }

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
        /// 误差平均
        /// </summary>
        public string AVE_ERR { get; set; }

        /// <summary>
        /// 样品均值
        /// </summary>
        public string SAMPLE_AVE { get; set; }

        /// <summary>
        /// 化整误差
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }

        /// <summary>
        /// 所有表位化整值平均误差
        /// </summary>
        public string ALL_INT_ERROR { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

       


    }
}