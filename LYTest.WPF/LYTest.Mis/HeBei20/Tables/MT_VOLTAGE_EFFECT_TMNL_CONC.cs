namespace LYTest.Mis.HeBei20.Tables
{
    //电源电压变化影响试验结论
    public class MT_VOLTAGE_EFFECT_TMNL_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// A13
        /// </summary>
        public string DETECT_NUM { get; set; }
        /// <summary>
        /// 0：不成功 1：成功
        /// </summary>
        public string IS_CONNECT { get; set; }

        /// <summary>
        /// 检验类别,见附录D：电源影响量
        /// </summary>
        public string DETECT_TYPE { get; set; }
        /// <summary>
        /// 电压A,
        /// </summary>
        public string VOLTAGE_A { get; set; }
        /// <summary>
        /// 电压B,
        /// </summary>
        public string VOLTAGE_B { get; set; }
        /// <summary>
        /// 电压C,
        /// </summary>
        public string VOLTAGE_C { get; set; }
        /// <summary>
        /// 电流A,
        /// </summary>
        public string CURRENT_A { get; set; }
        /// <summary>
        /// 电流B,
        /// </summary>
        public string CURRENT_B { get; set; }
        /// <summary>
        /// 电流C,
        /// </summary>
        public string CURRENT_C { get; set; }
    }
}