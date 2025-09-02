namespace LYTest.Mis.ShangHai20.Tables
{
    //时段投切误差结论
    public class MT_TS_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 费率
        /// </summary>
        public string FEE { get; set; }
        /// <summary>
        /// 测试起始时间
        /// </summary>
        public string TS_START_TIME { get; set; }
        /// <summary>
        /// 实际投切时间
        /// </summary>
        public string TS_REAL_TIME { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public string START_TIME { get; set; }

        /// <summary>
        /// 实现方式
        /// </summary>
        public string TS_WAY { get; set; }
        /// <summary>
        /// 投切误差
        /// </summary>
        public string TS_ERR_CONC_CODE { get; set; }
        /// <summary>
        /// 误差限
        /// </summary>
        public string TS_ERR { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }
        /// <summary>
        /// 误差下限
        /// </summary>
        public string ERR_DOWM { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public string VOLT { get; set; }


    }
}