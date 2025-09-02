namespace LYTest.Mis.SG186
{
    /// <summary>
    /// 电能表检定结论
    /// </summary>
    public struct CheckResult
    {
        /// <summary>
        /// 结论记录标识
        /// 本实体记录的唯一标识,由校表台系统填写,具体填写不做要求
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 记录标识
        /// 该字段为外键字段
        /// </summary>	
        public string Read_id { get; set; }

        /// <summary>
        /// 电能表标识
        /// 电能表标识为电能表的唯一内码信息,取任务信息中的电能表标识
        /// </summary>	
        public string Meter_id { get; set; }

        /// <summary>
        /// 正反向有无功
        /// 正向有功/正向无功/反向有功/反向无功
        /// </summary>	
        public string BOTH_WAY_POWER_FLAG { get; set; }

        /// <summary>
        /// 起动试验结论
        /// 校验台软件写入
        /// </summary>	
        public string START_CONC_CODE { get; set; }

        /// <summary>
        /// 潜动试验结论
        /// 校验台软件写入
        /// </summary>
        public string CREEP_CONC_CODE { get; set; }

        /// <summary>
        /// 耐压试验结论
        /// 校验台软件写入
        /// </summary>	
        public string VOLT_CONC { get; set; }

        /// <summary>
        /// 起动电流值
        /// 校验台软件写入
        /// </summary>	
        public string START_CURRENT { get; set; }

        /// <summary>
        /// 起动时间
        /// 校验台软件写入,如:1.5代表1.5分钟
        /// </summary>	
        public string START_DATE { get; set; }

        /// <summary>
        /// 耐压试验值
        /// 校验台软件写入
        /// </summary>
        public float VOLT_TEST_VALUE { get; set; }

        /// <summary>
        /// 误差上限
        /// 校验台软件写入
        /// </summary>
        public string ERR_UPPER_LIMIT { get; set; }

        /// <summary>
        /// 误差下限
        /// 校验台软件写入
        /// </summary>	
        public string ERR_LOWER_LIMIT { get; set; }

        /// <summary>
        /// 备用结论
        /// </summary>
        public string ELECT_CONC_CODE { get; set; }
    }
}
