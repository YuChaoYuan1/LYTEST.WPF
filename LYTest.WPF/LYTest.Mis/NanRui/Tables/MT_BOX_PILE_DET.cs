namespace LYTest.Mis.NanRui.Tables
{
    //周转箱组垛明细信息
    public class MT_BOX_PILE_DET
    {

        /// <summary>
        /// 检定任务单号
        /// </summary>
        public string DETECT_TASK_NO { get; set; }

        /// <summary>
        /// 系统编号
        /// </summary>
        public string SYS_NO { get; set; }

        /// <summary>
        /// 站台号
        /// </summary>
        public string PLATFORM_NO { get; set; }

        /// <summary>
        /// 箱条码 
        /// </summary>
        public string BOX_BAR_CODE { get; set; }

        /// <summary>
        /// 垛条码
        /// </summary>
        public string PILE_NO { get; set; }

        /// <summary>
        /// 写入时间:平台写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }

        /// <summary>
        /// 处理标记:0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }

        /// <summary>
        /// 处理时间:仓储系统处理时间
        /// </summary>
        public string HANDLE_DATE { get; set; }

        /// <summary>
        /// EQUIP_CATEG
        /// </summary>
        public string EQUIP_CATEG { get; set; }


    }
}