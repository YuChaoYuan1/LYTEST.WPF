namespace LYTest.Mis.NanRui.Tables
{
    //电能表最大需量清零次数变更事件试验结论
    public class MT_DEMAND_TMNL_CONC : MT_MET_CONC_Base
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
        /// 异常标志,
        /// </summary>
        public string ANOMALY_SIGN { get; set; }

        /// <summary>
        /// 起止标志,
        /// </summary>
        public string START_STOP_MARK { get; set; }


    }
}