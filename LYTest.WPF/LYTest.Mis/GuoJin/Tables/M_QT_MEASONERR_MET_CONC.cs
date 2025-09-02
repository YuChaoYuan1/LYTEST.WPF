namespace LYTest.Mis.GuoJin
{
    //测量及监测误差试验表
    public class M_QT_MEASONERR_MET_CONC : M_QT_CONC_Basic
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
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }
        /// <summary>
        /// 试验值
        /// </summary>
        public string TEST_VALUES { get; set; }
        /// <summary>
        /// 试验平均值
        /// </summary>
        public string TEST_VALUES_AVG { get; set; }
        /// <summary>
        /// 试验平均值取整
        /// </summary>
        public string TEST_VALUES_INT { get; set; }
        /// <summary>
        /// 限值
        /// </summary>
        public string TEST_VALUES_ABS { get; set; }
        /// <summary>
        /// 实际误差
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        /// 误差平均值
        /// </summary>
        public string AVE_ERR { get; set; }
        /// <summary>
        /// 误差化整值
        /// </summary>
        public string INT_CONV_ERR { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 功率
        /// </summary>
        public string POWER { get; set; }

        /// <summary>
        /// 被测试表试验值，多个用‘|’分割
        /// </summary>
        public string TEST_VALUES1 { get; set; }
        /// <summary>
        /// 被测试表试验平均值
        /// </summary>
        public string TEST_VALUES_AVG1 { get; set; }
        /// <summary>
        /// 被测试表试验平均值取整值
        /// </summary>
        public string TEST_VALUES_INT1 { get; set; }
    }
}