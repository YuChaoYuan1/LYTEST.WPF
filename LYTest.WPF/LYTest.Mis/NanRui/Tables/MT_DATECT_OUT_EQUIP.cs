namespace LYTest.Mis.NanRui.Tables
{
    /// <summary>
    /// 检定设备出库明细
    /// </summary>
    public class MT_DATECT_OUT_EQUIP
    {
        /// <summary>
        /// 检定任务单编号
        /// </summary>
        public string DETECT_TASK_NO { get; set; }
        /// <summary>
        /// 用表申请任务编号
        /// </summary>
        public string IO_TASK_NO { get; set; }
        /// <summary>
        ///设备类别
        /// </summary>
        public string EQUIP_CATEG { get; set; }
        /// <summary>
        /// 设备条形码
        /// </summary>
        public string BAR_CODE { get; set; }
        /// <summary>
        /// 箱条形码
        /// </summary>
        public string BOX_BAR_CODE { get; set; }
        /// <summary>
        /// 垛号
        /// </summary>
        public string PILE_NO { get; set; }
        /// <summary>
        /// 抽检复检标识
        /// </summary>
        public string PLATFORM_NO { get; set; }
        /// <summary>
        /// 系统编号
        /// </summary>
        public string SYS_NO { get; set; }
        /// <summary>
        /// 平台写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// <summary>
        /// 处理标记
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// 处理时间
        /// </summary>
        public string HANDLE_DATE { get; set; }
        public string ARRIVE_BATCH_NO { get; set; }
        public string REDETECT_FLAG { get; set; }
        public string EMP_NO { get; set; }
        /// <summary>
        /// 站台号
        /// </summary>
        public string PLATFORM_TYPE { get; set; }
    }
}
