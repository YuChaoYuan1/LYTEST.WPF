namespace LYTest.Mis.NanRui.Tables
{
    //用表申请生成的出库设备明细表。
    public class MT_DETECT_OUT_EQUIP
    {
        /// <summary>
        /// A1
        /// </summary>
        public string DETECT_TASK_NO { get; set; }

        /// <summary>
        /// 用表申请任务编号
        /// </summary>
        public string IO_TASK_NO { get; set; }

        /// <summary>
        ///  A2  关联代码分类的设备类别实体记录，单相电能表、三相电能表、互感器、采集终端
        /// </summary>
        public string EQUIP_CATEG { get; set; }

        /// <summary>
        /// A6
        /// </summary>
        public string BAR_CODE { get; set; }

        /// <summary>
        /// 箱设备所在周转箱的条码
        /// </summary>
        public string BOX_BAR_CODE { get; set; }

        /// <summary>
        /// 垛号
        /// </summary>
        public string PILE_NO { get; set; }

        /// <summary>
        /// 站台号
        /// </summary>
        public string PLATFORM_NO { get; set; }

        /// <summary>
        /// A2
        /// </summary>
        public string SYS_NO { get; set; }

        /// <summary>
        /// 平台写入时间
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

        /// <summary>
        /// 到货批次号
        /// </summary>
        public string ARRIVE_BATCH_NO { get; set; }

        /// <summary>
        /// 复检标识
        /// </summary>
        public string REDETECT_FLAG { get; set; }

        /// <summary>
        /// 人员编号
        /// </summary>
        public string EMP_NO { get; set; }

        /// <summary>
        /// 台体类型: 01-单相人工检定台；02-三相人工检定台；03-三相人工走字台
        /// </summary>
        public string PLATFORM_TYPE { get; set; }

    }
}