namespace LYTest.Mis.NanRui.Tables
{
    //1）记录存放检定任务安排情况，本实体主要包括：任务编号、明细编号、申请编号、执行班组、工作人员、安排数量等属性。
    // 2）通过检定任务安排业务，由录入产生记录。
    // 3）该实体主要由检定、校准业务使用。
    // 4）所有12种检定类别对应的检定任务都保存在该张表中。
    // 5）样品批次号字段在样品比对，全性能试验，抽样检定，招标选型产品检测和招标前批次检测有效。
    // 6）委托单位，委托类别，委托人，委托日期，委托人联系方式只在委托检测中有效。
    public class MT_DETECT_TASK
    {
        /// <summary>
        /// A1
        /// </summary>
        public string DETECT_TASK_NO { get; set; }
        /// <summary>
        /// 任务优先级
        /// </summary>
        public decimal TASK_PRIO { get; set; }
        /// <summary>
        /// 01 自动  02  人工
        /// </summary>
        public string DETECT_MODE { get; set; }
        /// <summary>
        /// A2
        /// </summary>
        public string SYS_NO { get; set; }
        /// <summary>
        /// 到货的批次号
        /// </summary>
        public string ARRIVE_BATCH_NO { get; set; }
        /// <summary>
        ///  A2  关联代码分类的设备类别实体记录，单相电能表、三相电能表、互感器、采集终端
        /// </summary>
        public string EQUIP_CATEG { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        public string MODEL_CODE { get; set; }
        /// <summary>
        /// 检定方案实体记录的唯一性标识
        /// </summary>
        public decimal SCHEMA_ID { get; set; }
        /// <summary>
        /// 关联检定方案的检定方案标识，如果没有复检方案-1
        /// </summary>
        public decimal REDETECT_SCHEMA { get; set; }
        /// <summary>
        /// 第一次检定完成后，是否继续再次复检，0：否，其它：是
        /// </summary>
        public string REDETECT_FLAG { get; set; }
        /// <summary>
        /// 复检数量
        /// </summary>
        public decimal REDETECT_QTY { get; set; }
        /// <summary>
        /// 设备数量
        /// </summary>
        public decimal EQUIP_QTY { get; set; }
        /// <summary>
        /// 总垛数
        /// </summary>
        public decimal PILE_QTY { get; set; }
        /// <summary>
        /// 0-未执行；1-执行中；2-执行完毕；3-暂停；4-中止
        /// </summary>
        public string TASK_STATUS { get; set; }
        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }
        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// 平台写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// <summary>
        /// ERP_BATCH_NO
        /// </summary>
        public string ERP_BATCH_NO { get; set; }
        /// <summary>
        /// 任务类型：22表示自动铅封任务
        /// </summary>
        public string TASK_TYPE { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public string EQUIP_STATUS_CODE { get; set; }
    }
}