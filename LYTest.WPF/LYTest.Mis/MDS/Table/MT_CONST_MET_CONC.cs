namespace LYTest.Mis.MDS.Table
{
    /// <summary>
    /// 电能表常数试验结论（走字试验）
    /// </summary>
    public class MT_CONST_MET_CONC : MT_MET_CONC_Base
    {


        ///// <summary>
        ///// A10       0：无效 1：有效
        ///// </summary>
        //public string IS_VALID { get; set; }

        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }

        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }

        /// <summary>
        /// 实际脉冲数
        /// </summary>
        public string REAL_PULES { get; set; }

        /// <summary>
        /// 理论脉冲(合格脉冲数)
        /// </summary>
        public string QUALIFIED_PULES { get; set; }

        private string _START_READING;
        /// <summary>
        /// 起始度数
        /// </summary>
        public string START_READING
        {
            get { return _START_READING; }
            set
            {
                if (value.Length > 8)
                    _START_READING = value.Substring(0, 8);
                else
                    _START_READING = value;
            }
        }

        private string _END_READING;
        /// <summary>
        /// 结束度数
        /// </summary>
        public string END_READING
        {
            get { return _END_READING; }
            set
            {
                if (value.Length > 8)
                    _END_READING = value.Substring(0, 8);
                else
                    _END_READING = value;
            }
        }

        /// <summary>
        /// 走字差值 =结束度数 - 起始度数
        /// </summary>
        public string DIFF_READING { get; set; }

        private string _STANDARD_READING;
        /// <summary>
        /// 标准表度数
        /// </summary>
        public string STANDARD_READING
        {
            get { return _STANDARD_READING; }
            set
            {
                if (value.Length > 8)
                    _STANDARD_READING = value.Substring(0, 8);
                else
                    _STANDARD_READING = value;
            }
        }

        /// <summary>
        /// 误差：以“|”分割
        /// </summary>
        public string ERROR { get; set; }

        /// <summary>
        /// 误差下限
        /// </summary>
        public string ERR_DOWN { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }

        /// <summary>
        /// 常数误差
        /// </summary>
        public string CONST_ERR { get; set; }

        /// <summary>
        /// 控制方式
        /// </summary>
        public string CONTROL_METHOD { get; set; }

        /// <summary>
        /// 示数类型
        /// </summary>
        public string READ_TYPE_CODE { get; set; }

        /// <summary>
        /// 装拆起始度数
        /// </summary>
        public string IR_LAST_READING { get; set; }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        public string FEE_RATIO { get; set; }


        /// <summary>
        /// 电压
        /// </summary>
        public string VOLT { get; set; }


        //以下为新加入

        /// <summary>
        /// 费率起始时间
        /// </summary>
        public string FEE_START_TIME { get; set; }

        /// <summary>
        /// 总分电量值
        /// </summary>
        public string DIVIDE_ELECTRIC_QUANTITY { get; set; }
    }
}
