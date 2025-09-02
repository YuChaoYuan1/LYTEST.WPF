namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 6.14 检定分项结论接口 重复字段
    /// </summary>
    public class CommonDatas_DETedTestData
    {
        /// <summary>
        /// 	电能表综合结论记录标识
        /// </summary>
        public string veriMeterRsltId { get; set; }
        /// <summary>
        /// 	地区代码
        /// </summary>
        public string areaCode { get; set; }
        /// <summary>
        /// 	供电单位编号
        /// </summary>
        public string psOrgNo { get; set; }
        /// <summary>
        /// 	设备标识
        /// </summary>
        public string devId { get; set; }
        /// <summary>
        /// 	资产编号
        /// </summary>
        public string assetNo { get; set; }
        /// <summary>
        /// 	条形码
        /// </summary>
        public string barCode { get; set; }
        /// <summary>
        /// 	检定任务单编号
        /// </summary>
        public string veriTaskNo { get; set; }
        /// <summary>
        /// 	设备分类
        /// </summary>
        public string devCls { get; set; }
        /// <summary>
        /// 	系统编号
        /// </summary>
        public string sysNo { get; set; }
        /// <summary>
        /// 	设备档案编号
        /// </summary>
        public string plantNo { get; set; }
        /// <summary>
        /// 设备单元编号
        /// </summary>
        public string plantElementNo { get; set; }
        /// <summary>
        /// 专机编号
        /// </summary>
        public string machNo { get; set; }
        /// <summary>
        /// 表位编号
        /// </summary>
        public string devSeatNo { get; set; }
        /// <summary>
        /// 	检定/校准日期
        /// </summary>
        public string veriCaliDate { get; set; }



        //private T AutoCopy<T>(CommonDatas_DETedTestData parent)
        //{
        //    T child = default;
        //    int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < numVisuals; i++)
        //    {
        //        Visual visual = (Visual)VisualTreeHelper.GetChild(parent, i);
        //        child = visual as T;
        //        if (child == null)
        //        {
        //            child = GetVisualChild<T>(visual);
        //        }
        //        if (child != null)
        //        {
        //            break;
        //        }
        //    }
        //    return child;
        //}
    }
}
