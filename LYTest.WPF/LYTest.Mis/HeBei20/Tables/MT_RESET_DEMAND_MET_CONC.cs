namespace LYTest.Mis.HeBei20.Tables
{
    //最大需量清零
    public class MT_RESET_DEMAND_MET_CONC : MT_MET_CONC_Base
    {

        /// <summary>
        /// 清零前需量电量
        /// </summary>
        public string DEMAND { get; set; }

        /// <summary>
        /// 清零后需量电量
        /// </summary>
        public string RESET_DEMAND { get; set; }
    }
}