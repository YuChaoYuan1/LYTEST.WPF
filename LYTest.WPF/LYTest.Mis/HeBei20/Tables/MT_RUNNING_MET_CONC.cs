namespace LYTest.Mis.HeBei20.Tables
{
    //电能表常数试验结论
    public class MT_RUNNING_MET_CONC : MT_MET_CONC_Base
    {
        private string _is_valid = "";
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID
        {
            get { return _is_valid; }
            set { _is_valid = value; }
        }
        private string _ir_start_reading = "";
        /// <summary>
        /// 起始度数
        /// </summary>
        public string IR_START_READING
        {
            get { return _ir_start_reading; }
            set { _ir_start_reading = value; }
        }
        private string _ir_end_reading = "";
        /// <summary>
        /// 结束度数
        /// </summary>
        public string IR_END_READING
        {
            get { return _ir_end_reading; }
            set { _ir_end_reading = value; }
        }
        private string _ir_reading = "";
        /// <summary>
        /// 走字度数
        /// </summary>
        public string IR_READING
        {
            get { return _ir_reading; }
            set { _ir_reading = value; }
        }
        private string _fee = "";
        /// <summary>
        /// 费率
        /// </summary>
        public string FEE
        {
            get { return _fee; }
            set { _fee = value; }
        }
        private string _running_type_code = "";
        /// <summary>
        /// 实现方式
        /// </summary>
        public string RUNNING_TYPE_CODE
        {
            get { return _running_type_code; }
            set { _running_type_code = value; }
        }
        private string _fee_start_time = "";
        /// <summary>
        /// 起始费率时间
        /// </summary>
        public string FEE_START_TIME
        {
            get { return _fee_start_time; }
            set { _fee_start_time = value; }
        }
        private string _ir_pules = "";
        /// <summary>
        /// 走字脉冲数
        /// </summary>
        public string IR_PULES
        {
            get { return _ir_pules; }
            set { _ir_pules = value; }
        }
        private string _register_read = "";
        /// <summary>
        /// 标准表度数
        /// </summary>
        public string REGISTER_READ
        {
            get { return _register_read; }
            set { _register_read = value; }
        }
        private string _ar_ts_reading_err = "";
        /// <summary>
        /// 电量误差
        /// </summary>
        public string AR_TS_READING_ERR
        {
            get { return _ar_ts_reading_err; }
            set { _ar_ts_reading_err = value; }
        }
        private string _err_up = "";
        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP
        {
            get { return _err_up; }
            set { _err_up = value; }
        }
        private string _err_down = "";
        /// <summary>
        /// 误差下限
        /// </summary>
        public string ERR_DOWN
        {
            get { return _err_down; }
            set { _err_down = value; }
        }

        private string _both_way_power_flag = "";
        /// <summary>
        /// BOTH_WAY_POWER_FLAG
        /// </summary>
        public string BOTH_WAY_POWER_FLAG
        {
            get { return _both_way_power_flag; }
            set { _both_way_power_flag = value; }
        }
    }
}