namespace LYTest.Mis.HeBei20.Tables
{
    //费控告警试验结论
    public class MT_WARNNING_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 告警结论
        /// </summary>
        public string WARN_CONC_CODE { get; set; }
    }
}