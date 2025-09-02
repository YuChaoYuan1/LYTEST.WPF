namespace LYTest.Mis.NanRui.Tables
{
    /// <summary>
    /// 时段费率功能
    /// </summary>
    public class MT_FEE_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// A13
        /// </summary>
        public string DETECT_NUM { get; set; }
        /// <summary>
        /// A11
        /// </summary>
        public string DETECT_TYPE { get; set; }
        /// <summary>
        /// A14
        /// </summary>
        public string RTU_WARN_DATE { get; set; }
        /// <summary>
        /// A15
        /// </summary>
        public string WARN_CODE { get; set; }
        /// <summary>
        /// A16
        /// </summary>
        public string WARN_CONTENT { get; set; }
        /// <summary>
        /// A17
        /// </summary>
        public string WARN_DATE { get; set; }

        /// <summary>
        /// A14
        /// </summary>
        public string UNPASS_REASON { get; set; }

        /// <summary>
        /// 测量点号,
        /// </summary>
        public string DETECT_POINT_NO { get; set; }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 负载电流
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }

        /// <summary>
        /// 电流相别
        /// </summary>
        public string CUR_PHASE_CODE { get; set; }
    }
}