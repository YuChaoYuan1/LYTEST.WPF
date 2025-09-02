namespace LYTest.Mis.HeBei20.Tables
{
    //费率时段电能示值误差
    public class MT_TIME_ERROR_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 功率方向
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
        /// 费率
        /// </summary>
        public string FEE_RATIO { get; set; }

        /// <summary>
        /// 控制方式
        /// </summary>
        public string CONTROL_WAY { get; set; }

        /// <summary>
        /// 费率起始时间
        /// </summary>
        public string FEE_START_TIME { get; set; }

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
        /// 示值误差
        /// </summary>
        public string VALUE_CONC_CODE { get; set; }

        /// <summary>
        /// 合格脉冲数
        /// </summary>
        public string QUALIFIED_PULES { get; set; }

        /// <summary>
        /// 总分电量值差(kWh)
        /// </summary>
        public string TOTAL_READING_ERR { get; set; }
    }
}
