namespace LYTest.Mis.NanRui.Tables
{ 
    /// 检定设备信息中间表
    /// <summary>
    /// 检定设备信息中间表
    /// </summary>
public class MT_METER_POSI
    {
        /// 检定任务单编号
        /// <summary>
        /// 检定任务单编号
        /// </summary>
        public string DETECT_TASK_NO { get; set; }
        /// 条形码
        /// <summary>
        /// 条形码
        /// </summary>
        public string BAR_CODE { get; set; }
        /// 装置编号
        /// <summary>
        /// 装置编号
        /// </summary>
        public string EQUIP_NO { get; set; }
        /// 表位号
        /// <summary>
        /// 表位号
        /// </summary>
        public string EQUIP_INDX { get; set; }
        /// 平台写入时间
        /// <summary>
        /// 平台写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// 处理标记
        /// <summary>
        /// 处理标记
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// 处理时间
        /// <summary>
        /// 处理时间
        /// </summary>
        public string HANDLE_DATE { get; set; }
    }
}
