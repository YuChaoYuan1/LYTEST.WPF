namespace LYTest.Mis.HeBei20.Tables
{
    //标准偏差
    public class MT_DEVIATION_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        ///   0-正向有功, 1-反向有功, 2-正向无功, 3-反向无功
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }
        /// <summary>
        /// ABC, AC, A, B, C
        /// </summary>
        public string IABC { get; set; }
        /// <summary>
        /// Imax, 0.5Imax
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// LOAD_VOLTAGE
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }
        /// <summary>
        /// FREQ
        /// </summary>
        public string FREQ { get; set; }
        /// <summary>
        /// PF
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 1, 2, 3…
        /// </summary>
        public string DETECT_CIRCLE { get; set; }
        /// <summary>
        /// 1, 2, 3…
        /// </summary>
        public string SIMPLING { get; set; }
        /// <summary>
        /// 误差，以“|”分割
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        /// 误差平均值
        /// </summary>
        public string AVE_ERR { get; set; }
        /// <summary>
        /// 平均误差化整值
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }
    }
}