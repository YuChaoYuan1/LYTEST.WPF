namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 6.2 检定出库明细获取接口
    /// </summary>
    public class InOutWhDtl : CommonDatas
    {
        public string barCode { get; set; } //	条形码,
        public string boxBarCode { get; set; }  //	箱条码,
        public string pileNo { get; set; }  //	垛号,
        public string taskNo { get; set; }  //	检定任务号,
        public string ioTaskNo { get; set; }    //	出入库任务号,
        public string devCls { get; set; }  //	设备分类,
        public string arriveBatchNo { get; set; }	//	到货批次号
    }
}
