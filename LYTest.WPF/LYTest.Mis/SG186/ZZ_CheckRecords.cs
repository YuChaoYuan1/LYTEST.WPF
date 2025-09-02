namespace LYTest.Mis.SG186
{
    public struct ZZ_CheckRecords
    {
        
        /// <summary>
        /// 记录标识
        /// 本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        /// </summary>
        public string Read_id { get; set; }

        /// <summary>
        /// *发送批号
        /// 由校表台系统填写,具体填写不做要求,可以为空
        /// </summary>
        public string Send_sn { get; set; }

        /// <summary>
        /// *走字编号
        /// 由校表台系统填写,具体填写不做要求,可以为空
        /// </summary>
        public string Walk_no { get; set; }

        /// <summary>
        /// 申请编号
        /// </summary>
        public string App_no { get; set; }

        /// <summary>
        /// 电能表标识
        /// 为电能表的唯一内码信息,取任务信息中的电能表标识
        /// </summary>
        public string Meter_id { get; set; }

        /// <summary>
        /// 走字人员
        /// </summary>
        public string RUNING_PERSON_NAME { get; set; }

        /// <summary>
        /// 走字日期
        /// </summary>
        public string RUNING_DATE { get; set; }

        /// <summary>
        /// 核验人员
        /// </summary>
        public string Checker_no { get; set; }

        /// <summary>
        /// 核验日期
        /// 实体为字符型19位
        /// </summary>
        public string Chk_rec_date { get; set; }

        /// <summary>
        /// 走字台编号
        /// </summary>
        public string Runing_desk_no { get; set; }

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
        /// 走字指针类型
        /// </summary>
        public string Pointer_type_code { get; set; }

        /// <summary>
        /// 标准表示数平均值
        /// </summary>	
        public string STD_READING_AVG { get; set; }

        /// <summary>
        /// 标准表相对误差
        /// </summary>	
        public string STD_RELATIVE_ERR { get; set; }

        /// <summary>
        /// 示数组合误差
        /// </summary>
        public string Comp_err { get; set; }

        /// <summary>
        /// 是否已校时
        /// 缺省为否
        /// </summary>	
        public string TIME_CALIBRATE_FLAG { get; set; }

        /// <summary>
        /// 走字结论
        /// 1合格，0不合格
        /// </summary>
        public string Conc_code { get; set; }

        /// <summary>
        /// *走字说明
        /// </summary>
        public string Runing_remark { get; set; }

        /// <summary>
        /// *供电单位
        /// 保留，厂家可以不填
        /// </summary>	
        public string Org_no { get; set; }

        public string Bar_code { get; set; }
    }
}
