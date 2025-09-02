namespace LYTest.Mis.NanRui.Tables
{
    /// <summary>
    /// 误差实验
    /// </summary>
    public class MT_ERROR_IT_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 负荷标志,满轻载标志，01额定负荷，02下限负荷
        /// </summary>
        public string LOAD_FLAG { get; set; }
        /// <summary>
        /// 误差项,见附录D：互感器误差项
        /// </summary>
        public string ERR_ITEM { get; set; }
        /// <summary>
        /// 比差,比差
        /// </summary>
        public string RATIO_ERR { get; set; }
        /// <summary>
        /// 角差,角差
        /// </summary>
        public string ANGLE_ERR { get; set; }
        /// <summary>
        /// 比差修约值,比差修约值
        /// </summary>
        public string RATIO_AMEND_ERR { get; set; }
        /// <summary>
        /// 角差修约值,角差修约值
        /// </summary>
        public string ANGLE_AMEND_ERR { get; set; }
    }
}