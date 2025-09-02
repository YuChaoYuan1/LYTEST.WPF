namespace LYTest.Mis.SG186
{
    /// <summary>
    /// 检定记录
    /// </summary>
    public  struct CheckRecords
    {       
	    /// <summary>
	    /// 记录标识
        /// 本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
	    /// </summary>
        public string Read_id { get; set; }

        /// <summary>
        /// *发送批号
        /// 由校表台系统填写,具体填写不做要求，可为空
        /// </summary>
        public string Send_sn { get; set; }

        /// <summary>
        /// 申请编号
        /// 取任务中的申请编号
        /// </summary>
        public string App_no { get; set; }

        /// <summary>
        /// 检定编号
        /// 由校表台系统填写,具体填写不做要求，不能为空
        /// </summary>	
        public string Chk_no { get; set; }

        /// <summary>
        /// 电能表标识
        /// 为电能表的唯一内码信息,取任务信息中的电能表标识
        /// </summary>
        public string Meter_id { get; set; }

        /// <summary>
        /// 检定人员
        /// </summary>	
        public string CHECKER_NAME { get; set; }

        /// <summary>
        /// 检定日期
        /// </summary>
        public string Chk_date { get; set; }

        /// <summary>
        /// 核验人员
        /// </summary>	
        public string Checker_no { get; set; }

        /// <summary>
        /// 核验日期
        /// </summary>	
        public string Chk_rec_date { get; set; }

        /// <summary>
        /// 标准装置检定证书号
        /// </summary>
        public string Equip_cert_no { get; set; }

        /// <summary>
        /// 检定台编号
        /// </summary>	
        public string Chk_desk_no { get; set; }

        /// <summary>
        /// 挂表位置
        /// </summary>	
        public string Meter_loc { get; set; }

        /// <summary>
        /// 温度
        /// 如有小数则四舍五入
        /// </summary>	
        public float Temp { get; set; }

        /// <summary>
        /// 湿度
        /// 如有小数则四舍五入
        /// </summary>	
        public float Humidity { get; set; }

        /// <summary>
        /// 检定依据
        /// (校验台软件写入)
        /// </summary>
        public string Chk_basis { get; set; }

        /// <summary>
        /// 校核常数
        /// (校验台软件写入)
        /// </summary>	
        public string Chk_const { get; set; }

        /// <summary>
        /// 检定结论
        /// 1合格，0不合格
        /// </summary>	
        public string Chk_conc { get; set; }

        /// <summary>
        /// 证书编号
        /// 检定证书的编号
        /// </summary>	
        public string Cert_id { get; set; }

        /// <summary>
        /// 检定有效日期
        /// 校验台软件写入 YYYYMMDD
        /// </summary>
        public string Chk_valid_date { get; set; }

        /// <summary>
        /// *检定说明
        /// </summary>	
        public string Chk_remark { get; set; }

        /// <summary>
        /// *供电单位
        /// 保留，厂家可以不填写
        /// </summary>
        public string Org_no { get; set; }

        /// <summary>
        /// 接收人
        /// 数据接收人员姓名
        /// </summary>	
        public string Recipient_name { get; set; }

        /// <summary>
        /// 送检单位
        /// </summary>
        public string Org_company { get; set; }

        /// <summary>
        /// 检定状态
        /// </summary>
        public string Detect { get; set; }

    }
}
