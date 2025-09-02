namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 计度器总电能示值组合误差
    /// </summary>
    public class MT_HUTCHISON_COMBINA_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }
        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 费率:1：尖，2：峰，3：平，4：谷，5：总
        /// </summary>
        public string FEE_RATIO { get; set; }
        /// <summary>
        /// 控制方式
        /// </summary>
        public string CONTROL_METHOD { get; set; }
        /// <summary>
        /// 走字时间(分钟)
        /// </summary>
        public string IR_TIME { get; set; }
        /// <summary>
        /// 走字度数(千瓦时)
        /// </summary>
        public string IR_READING { get; set; }
        /// <summary>
        /// 误差上限（s/d）
        /// </summary>
        public string ERR_UP { get; set; }
        /// <summary>
        /// 误差下限（s/d）
        /// </summary>
        public string ERR_DOWN { get; set; }
        /// <summary>
        /// 电压(1为1%)
        /// </summary>
        public string VOLTAGE { get; set; }
        /// <summary>
        /// 总分电量值差(kWh)
        /// </summary>
        public string TOTAL_READING_ERR { get; set; }

        /// <summary>
        /// 总示值增量
        /// </summary>
        public string TOTAL_INCREMENT { get; set; }
        /// <summary>
        /// 各费率示值增量和
        /// </summary>
        public string SUMER_ALL_INCREMENT { get; set; }
        /// <summary>
        /// 尖示值增量
        /// </summary>
        public string SHARP_INCREMENT { get; set; }
        /// <summary>
        /// 峰示值增量
        /// </summary>
        public string PEAK_INCREMENT { get; set; }
        /// <summary>
        /// 平示值增量
        /// </summary>
        public string FLAT_INCREMENT { get; set; }
        /// <summary>
        /// 谷示值增量
        /// </summary>
        public string VALLEY_INCREMENT { get; set; }
        /// <summary>
        /// 示值组合误差
        /// </summary>
        public string VALUE_CONC_CODE { get; set; }
        /// <summary>
        /// 费率示值。标识传的数据是什么顺序, 例：“尖|峰|平|谷|总”、“总|平|尖|峰|谷”等
        /// </summary>
        public string FEE_VALUE { get; set; }
        /// <summary>
        /// 起始示值。数据为尖、峰、平、谷、总数据的拼接，各数据间以“|”符间隔
        /// </summary>
        public string START_VALUE { get; set; }
        /// <summary>
        /// 结束示值。数据为尖、峰、平、谷、总数据的拼接，各数据间以“|”符间隔
        /// </summary>
        public string END_VALUE { get; set; }
        /// <summary>
        /// 电能增量。数据为尖、峰、平、谷、总数据的拼接，各数据间以“|”符间隔
        /// </summary>
        public string ELE_INCREMENT { get; set; }

    }
}
