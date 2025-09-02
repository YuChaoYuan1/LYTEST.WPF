namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 闰年切换
    /// </summary>
    public class MT_LEAP_YEAR_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 设置的闰年日期时间
        /// </summary>
        public string SETTING_DATE { get; set; }
        /// <summary>
        /// 读取的日期时间
        /// </summary>
        public string READ_DATE { get; set; }
    }
}