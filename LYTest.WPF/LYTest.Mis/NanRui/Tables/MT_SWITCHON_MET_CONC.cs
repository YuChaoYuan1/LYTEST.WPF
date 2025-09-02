namespace LYTest.Mis.NanRui.Tables
{
    //费控合闸试验结论
    public class MT_SWITCHON_MET_CONC : MT_MET_CONC_Base
    {

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// 费控合闸结论
        /// </summary>
        public string SWITCH_ON_CONC_CODE { get; set; }
    }
}