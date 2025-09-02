using System;

namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 剩余电量递减试验结论
    /// </summary>
    public class MT_EQ_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 设备的唯一标识
        /// </summary>
        public string EQUIP_ID { get; set; }

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        private string _TOTAL_EQ;
        /// <summary>
        /// 总电量
        /// </summary>
        public string TOTAL_EQ
        {
            get { return _TOTAL_EQ; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "";
                else
                    value = Convert.ToSingle(value).ToString();

                if (value.Length > 8)
                    _TOTAL_EQ = value.Substring(0, 8);
                else
                    _TOTAL_EQ = value;
            }
        }

        private string _SURPLUS_EQ;

        /// <summary>
        /// 剩余电量
        /// </summary>
        public string SURPLUS_EQ
        {
            get { return _SURPLUS_EQ; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "";
                else
                    value = Convert.ToSingle(value).ToString();

                if (value.Length > 8)
                    _SURPLUS_EQ = value.Substring(0, 8);
                else
                    _SURPLUS_EQ = value;
            }
        }

        private string _CURR_ELEC_PRICE;
        /// <summary>
        /// 当前电价
        /// </summary>
        public string CURR_ELEC_PRICE
        {
            get { return _CURR_ELEC_PRICE; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "";
                else
                    value = Convert.ToSingle(value).ToString();

                if (value.Length > 8)
                    _CURR_ELEC_PRICE = value.Substring(0, 8);
                else
                    _CURR_ELEC_PRICE = value;
            }
        }

        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
    }
}