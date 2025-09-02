namespace LYTest.Mis.NanRui.Tables
{
    //检定系统设备信息
    public class MT_DETECT_SYS_INFO
    {
        /// <summary>
        /// A2
        /// </summary>
        public string SYS_NO { get; set; }
        /// <summary>
        /// 系统名称
        /// </summary>
        public string SYS_NAME { get; set; }
        /// <summary>
        /// A14
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }
    }
}