namespace LYTest.Mis.NanRui.Tables
{
    //营业报停控试验结论
    public class MT_ELECON_TMNL_CONC : MT_MET_CONC_Base
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
        private string _detect_num = "";
        /// <summary>
        /// A13
        /// </summary>
        public string DETECT_NUM
        {
            get { return _detect_num; }
            set { _detect_num = value; }
        }
        private string _detect_type = "";
        /// <summary>
        /// A11
        /// </summary>
        public string DETECT_TYPE
        {
            get { return _detect_type; }
            set { _detect_type = value; }
        }

        private string _unpass_reason = "";
        /// <summary>
        /// A14
        /// </summary>
        public string UNPASS_REASON
        {
            get { return _unpass_reason; }
            set { _unpass_reason = value; }
        }

        private string _rtu_warn_date1 = "";
        /// <summary>
        /// 终端警告发生时间1,终端警告发生时间
        /// </summary>
        public string RTU_WARN_DATE1
        {
            get { return _rtu_warn_date1; }
            set { _rtu_warn_date1 = value; }
        }
        private string _warn_code1 = "";
        /// <summary>
        /// 警告代码1,警告代码
        /// </summary>
        public string WARN_CODE1
        {
            get { return _warn_code1; }
            set { _warn_code1 = value; }
        }
        private string _warn_content1 = "";
        /// <summary>
        /// 警告内容1,警告内容
        /// </summary>
        public string WARN_CONTENT1
        {
            get { return _warn_content1; }
            set { _warn_content1 = value; }
        }
        private string _warn_date1 = "";
        /// <summary>
        /// 警告时间1,警告时间
        /// </summary>
        public string WARN_DATE1
        {
            get { return _warn_date1; }
            set { _warn_date1 = value; }
        }
        private string _trip_out_num1 = "";
        /// <summary>
        /// 跳闸输出次数1,跳闸输出次数
        /// </summary>
        public string TRIP_OUT_NUM1
        {
            get { return _trip_out_num1; }
            set { _trip_out_num1 = value; }
        }
        private string _rtu_warn_date2 = "";
        /// <summary>
        /// 终端警告发生时间2,终端警告发生时间
        /// </summary>
        public string RTU_WARN_DATE2
        {
            get { return _rtu_warn_date2; }
            set { _rtu_warn_date2 = value; }
        }
        private string _warn_code2 = "";
        /// <summary>
        /// 警告代码2,警告代码
        /// </summary>
        public string WARN_CODE2
        {
            get { return _warn_code2; }
            set { _warn_code2 = value; }
        }
        private string _warn_content2 = "";
        /// <summary>
        /// 警告内容2,警告内容
        /// </summary>
        public string WARN_CONTENT2
        {
            get { return _warn_content2; }
            set { _warn_content2 = value; }
        }
        private string _warn_date2 = "";
        /// <summary>
        /// 警告时间2,警告时间
        /// </summary>
        public string WARN_DATE2
        {
            get { return _warn_date2; }
            set { _warn_date2 = value; }
        }
        private string _trip_out_num2 = "";
        /// <summary>
        /// 跳闸输出次数2,跳闸输出次数
        /// </summary>
        public string TRIP_OUT_NUM2
        {
            get { return _trip_out_num2; }
            set { _trip_out_num2 = value; }
        }
        private string _rtu_warn_date3 = "";
        /// <summary>
        /// 终端警告发生时间3,终端警告发生时间
        /// </summary>
        public string RTU_WARN_DATE3
        {
            get { return _rtu_warn_date3; }
            set { _rtu_warn_date3 = value; }
        }
        private string _warn_code3 = "";
        /// <summary>
        /// 警告代码3,警告代码
        /// </summary>
        public string WARN_CODE3
        {
            get { return _warn_code3; }
            set { _warn_code3 = value; }
        }
        private string _warn_content3 = "";
        /// <summary>
        /// 警告内容3,警告内容
        /// </summary>
        public string WARN_CONTENT3
        {
            get { return _warn_content3; }
            set { _warn_content3 = value; }
        }
        private string _warn_date3 = "";
        /// <summary>
        /// 警告时间3,警告时间
        /// </summary>
        public string WARN_DATE3
        {
            get { return _warn_date3; }
            set { _warn_date3 = value; }
        }
        private string _trip_out_num3 = "";
        /// <summary>
        /// 跳闸输出次数3,跳闸输出次数
        /// </summary>
        public string TRIP_OUT_NUM3
        {
            get { return _trip_out_num3; }
            set { _trip_out_num3 = value; }
        }
        private string _rtu_warn_date4 = "";
        /// <summary>
        /// 终端警告发生时间4,终端警告发生时间
        /// </summary>
        public string RTU_WARN_DATE4
        {
            get { return _rtu_warn_date4; }
            set { _rtu_warn_date4 = value; }
        }
        private string _warn_code4 = "";
        /// <summary>
        /// 警告代码4,警告代码
        /// </summary>
        public string WARN_CODE4
        {
            get { return _warn_code4; }
            set { _warn_code4 = value; }
        }
        private string _warn_content4 = "";
        /// <summary>
        /// 警告内容4,警告内容
        /// </summary>
        public string WARN_CONTENT4
        {
            get { return _warn_content4; }
            set { _warn_content4 = value; }
        }
        private string _warn_date4 = "";
        /// <summary>
        /// 警告时间4,警告时间
        /// </summary>
        public string WARN_DATE4
        {
            get { return _warn_date4; }
            set { _warn_date4 = value; }
        }
        private string _trip_out_num4 = "";
        /// <summary>
        /// 跳闸输出次数4,跳闸输出次数
        /// </summary>
        public string TRIP_OUT_NUM4
        {
            get { return _trip_out_num4; }
            set { _trip_out_num4 = value; }
        }
    }
}