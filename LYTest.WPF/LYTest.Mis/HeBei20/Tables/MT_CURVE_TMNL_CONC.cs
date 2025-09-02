namespace LYTest.Mis.HeBei20.Tables
{
    //曲线数据试验结论
    public class MT_CURVE_TMNL_CONC : MT_MET_CONC_Base
    {

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// A13
        /// </summary>
        public string DETECT_NUM { get; set; }

        /// A11
        /// </summary>
        public string DETECT_TYPE { get; set; }

        /// <summary>
        /// A14
        /// </summary>
        public string FROZEN_CODE { get; set; }

        /// <summary>
        /// A15
        /// </summary>
        public string FROZEN_ITEM { get; set; }

        /// <summary>
        /// A16
        /// </summary>
        public string FROZEN_CONTENT { get; set; }

        /// <summary>
        /// A14
        /// </summary>
        public string UNPASS_REASON { get; set; }

        /// <summary>
        /// 冻结时间,冻结时间
        /// </summary>
        public string FROZEN_TIME { get; set; }


    }
}