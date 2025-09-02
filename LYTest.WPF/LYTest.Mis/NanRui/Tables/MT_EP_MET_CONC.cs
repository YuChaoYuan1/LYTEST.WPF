namespace LYTest.Mis.NanRui.Tables
{
    //费控保电试验结论
    public class MT_EP_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// 保电结论
        /// </summary>
        public string EH_CONC_CODE { get; set; }

    }
}