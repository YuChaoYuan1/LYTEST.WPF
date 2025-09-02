namespace LYTest.Mis.HeBei20.Tables
{
    //需量周期误差结论
    public class MT_DEMAND_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// 需量周期时间
        /// </summary>
        public string DEMAND_PERIOD { get; set; }

        /// <summary>
        /// 滑差时间
        /// </summary>
        public string DEMAND_TIME { get; set; }

        /// <summary>
        /// 滑差次数
        /// </summary>
        public string DEMAND_INTERVAL { get; set; }

        private string _REAL_DEMAND;
        /// <summary>
        /// 实际需量
        /// </summary>
        public string REAL_DEMAND
        {
            get { return _REAL_DEMAND; }
            set
            {
                if (value.Length > 8)
                    _REAL_DEMAND = value.Substring(0, 8);
                else
                    _REAL_DEMAND = value;
            }
        }

        /// <summary>
        /// 实际周期
        /// </summary>
        public string REAL_PERIOD { get; set; }

        /// <summary>
        /// 需量周期误差
        /// </summary>
        public string DEMAND_PERIOD_ERR { get; set; }

        /// <summary>
        /// 标准表需量值
        /// </summary>
        public string DEMAND_STANDARD { get; set; }

        /// <summary>
        /// 需量周期误差限
        /// </summary>
        public string DEMAND_PERIOD_ERR_ABS { get; set; }

        /// <summary>
        /// 需量清零结果
        /// </summary>
        public string CLEAR_DATA_RST { get; set; }

        /// <summary>
        /// 周期误差结论
        /// </summary>
        public string PERIOD_CONC_CODE { get; set; }

    }
}