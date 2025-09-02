namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 试验方案明细集合
    /// </summary>
    public class TrialSchDtl
    {
        public string trialSchDtlId { get; set; }   //	检定方案明细标识,
        public string trialSchId { get; set; }  //	检定方案标识,
        public string veriItemParaNo { get; set; }  //	检测项参数编码,
        public string detectSn { get; set; }    //	检定项参数顺序号,
        public string paraValue { get; set; }	//	参数具体值,

    }
}
