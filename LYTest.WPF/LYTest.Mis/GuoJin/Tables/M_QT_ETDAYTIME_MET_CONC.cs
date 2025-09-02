using System;

namespace LYTest.Mis.GuoJin
{
    //环境温度对由电源供电的时钟试验的影响试验
    public class M_QT_ETDAYTIME_MET_CONC : M_QT_CONC_Basic
    {
        private string _load_current = "";
        /// <summary>
        /// 负载电流
        /// </summary>
        public string LOAD_CURRENT
        {
            get { return _load_current; }
            set { _load_current = value; }
        }
        private string _load_voltage = "";
        /// <summary>
        /// 电压
        /// </summary>
        public string LOAD_VOLTAGE
        {
            get { return _load_voltage; }
            set { _load_voltage = value; }
        }
        private string _both_way_power_flag = "";
        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG
        {
            get { return _both_way_power_flag; }
            set { _both_way_power_flag = value; }
        }
        private string _iabc = "";
        /// <summary>
        /// 电流相别
        /// </summary>
        public string IABC
        {
            get { return _iabc; }
            set { _iabc = value; }
        }
        private string _freq = "";
        /// <summary>
        /// 频率
        /// </summary>
        public string FREQ
        {
            get { return _freq; }
            set { _freq = value; }
        }
        private string _pf = "";
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF
        {
            get { return _pf; }
            set { _pf = value; }
        }

        private string _meter_const_code = "";
        /// <summary>
        /// 电能表常数
        /// </summary>
        public string METER_CONST_CODE
        {
            get { return _meter_const_code; }
            set { _meter_const_code = value; }
        }

        private string _actuer_value = "";
        /// <summary>
        /// 由电源供电的时钟试验值
        /// </summary>
        public string ACTUER_VALUE
        {
            get { return _actuer_value; }
            set { _actuer_value = value; }
        }

        private string _avg_value = "";
        /// <summary>
        /// 平均值
        /// </summary>
        public string AVG_VALUE
        {
            get { return _avg_value; }
            set { _avg_value = value; }
        }
        private string _int_convert_err = "";
        /// <summary>
        /// 误差化整值
        /// </summary>
        public string INT_CONVERT_ERR
        {
            get { return _int_convert_err; }
            set { _int_convert_err = value; }
        }

        private string _value_err_abs = "";
        /// <summary>
        /// 由电源供电的时钟试验误差限
        /// </summary>
        public string VALUE_ERR_ABS
        {
            get { return _value_err_abs; }
            set { _value_err_abs = value; }
        }
        private string _tempera_value = "";
        /// <summary>
        /// 温度系数值
        /// </summary>
        public string TEMPERA_VALUE
        {
            get { return _tempera_value; }
            set { _tempera_value = value; }
        }

        private string _avg_tempera_value = "";
        /// <summary>
        /// 温度系数平均值
        /// </summary>
        public string AVG_TEMPERA_VALUE
        {
            get { return _avg_tempera_value; }
            set { _avg_tempera_value = value; }
        }
        private string _int_tempera_value = "";
        /// <summary>
        /// 温度系数化整值
        /// </summary>
        public string INT_TEMPERA_VALUE
        {
            get { return _int_tempera_value; }
            set { _int_tempera_value = value; }
        }

        private string _tempera_value_abs = "";
        /// <summary>
        /// 温度系数限值
        /// </summary>
        public string TEMPERA_VALUE_ABS
        {
            get { return _tempera_value_abs; }
            set { _tempera_value_abs = value; }
        }

        private string _detect_result = "";
        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT
        {
            get { return _detect_result; }
            set { _detect_result = value; }
        }
    }
}