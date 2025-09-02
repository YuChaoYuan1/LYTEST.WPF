namespace LYTest.Mis.HeBei20.Tables
{
    //费控取消保电试验结论
    public class MT_EC_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 费控取消保电结论
        /// </summary>
        public string EC_CONC_CODE { get; set; }


    }
}