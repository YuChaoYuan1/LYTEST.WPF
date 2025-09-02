namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 密钥更新结论
    /// </summary>
    public class MT_ESAM_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string KEY_NUM { get; set; }


        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string KEY_VER { get; set; }


        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string KEY_TYPE { get; set; }
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string KEY_STATUS { get; set; }
    }
}