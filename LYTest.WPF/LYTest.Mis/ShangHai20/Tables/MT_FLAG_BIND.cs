namespace LYTest.Mis.ShangHai20.Tables
{
    class MT_FLAG_BIND : MT_MET_CONC_Base
    {
        ///// <summary>
        ///// 检定任务单号
        ///// </summary>
        //public string DETECT_TASK_NO { get; set; }

        ///// <summary>
        ///// 系统编号
        ///// </summary>
        //public string SYS_NO { get; set; }

        /// <summary>
        /// 本次通知序列号
        /// </summary>
        public string WEB_NOTICE_NO { get; set; }

        ///// <summary>
        /////  设备类别  关联代码分类的设备类别实体记录，单相电能表、三相电能表、互感器、采集终端
        ///// </summary>
        //public string EQUIP_CATEG { get; set; }

        ///// <summary>
        ///// 设备条码
        ///// </summary>
        //public string BAR_CODE { get; set; }

        /// <summary>
        /// 标识码
        /// </summary>
        public string FLAG_CODE { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public string BIND_DATE { get; set; }

        /// <summary>
        /// 绑定人员
        /// </summary>
        public string BIND_NO { get; set; }

        ///// <summary>
        ///// 写入时间
        ///// </summary>
        //public string WRITE_DATE { get; set; }

        ///// <summary>
        ///// 处理标记          0-未处理（默认）；1-处理中；2-已处理
        ///// </summary>
        //public string HANDLE_FLAG { get; set; }

        ///// <summary>
        ///// 处理时间
        ///// </summary>
        //public string HANDLE_DATE { get; set; }

    }
}
