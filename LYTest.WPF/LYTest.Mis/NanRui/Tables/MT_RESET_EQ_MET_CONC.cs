namespace LYTest.Mis.NanRui.Tables
{
    //电能表清零
    public class MT_RESET_EQ_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 请零前电量
        /// </summary>
        public string EQ { get; set; }
        /// <summary>
        /// 清零后电量
        /// </summary>
        public string RESET_EQ { get; set; }
    }
}