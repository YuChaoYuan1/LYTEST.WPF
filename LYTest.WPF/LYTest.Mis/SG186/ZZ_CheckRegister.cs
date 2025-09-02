namespace LYTest.Mis.SG186
{
    public struct ZZ_CheckRegister
    {
        public string Id { get; set; }
        /// <summary>
        /// 记录标识
        /// 本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        /// </summary>
        public string Read_id { get; set; }

        /// <summary>
        /// *电能表标识
        /// 由校表台系统填写,具体填写不做要求,可以为空
        /// </summary>
        public string METER_ID { get; set; }

        /// <summary>
        /// *示数类型
        /// 
        /// </summary>
        public string READ_TYPE_CODE { get; set; }

        /// <summary>
        /// 条码号
        /// </summary>
        public string BAR_CODE { get; set; }

        /// <summary>
        /// 出厂编号
        /// 
        /// </summary>
        public string MADE_NO { get; set; }

        /// <summary>
        /// 计度器位数
        /// </summary>
        public string READING_DIGITS { get; set; }

        /// <summary>
        /// 起始示数
        /// </summary>
        public string LAST_READING { get; set; }

        /// <summary>
        /// 计度器示数
        /// </summary>
        public string REGISTER_READ { get; set; }

        /// <summary>
        /// 走字误差
        /// 
        /// </summary>
        public string RUNING_ERR { get; set; }

        /// <summary>
        /// 总(尖峰谷)起码
        /// </summary>
        public string T_LAST_READING { get; set; }

        /// <summary>
        /// 总(尖峰谷)止码
        /// 
        /// </summary>
        public float T_END_READING { get; set; }

        /// <summary>
        /// 费率时段电能示值误差
        /// 
        /// </summary>	
        public string AR_TS_READING_ERR { get; set; }

        /// <summary>
        /// 计数器示值组合误差
        /// </summary>
        public string COMP_ERR { get; set; }

        /// <summary>
        /// 装拆初始读数
        /// </summary>	
        public string IR_LAST_READING { get; set; }
    }
}
