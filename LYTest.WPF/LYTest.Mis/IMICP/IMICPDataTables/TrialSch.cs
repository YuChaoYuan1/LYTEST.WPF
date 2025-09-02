namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    ///  6.4 试验方案数据
    /// </summary>
    public class TrialSch
    {
        public string trialSchId { get; set; }  //	检定方案标识
        public string schNo { get; set; }   //	检定方案编号
        public string schName { get; set; } //	检定方案名称
        public string devCls { get; set; }  //	设备分类
        public string schType { get; set; } //	方案类型
        public string detectMode { get; set; }	//	检定方式

    }
}
