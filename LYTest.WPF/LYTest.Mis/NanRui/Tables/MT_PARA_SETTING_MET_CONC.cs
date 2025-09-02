namespace LYTest.Mis.NanRui.Tables
{
    //参数设置结论
    public class MT_PARA_SETTING_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 数据标识
        /// </summary>
        public string DATA_FLAG { get; set; }
        /// <summary>
        /// 设置值
        /// </summary>
        public string SETTING_VALUE { get; set; }
        /// <summary>
        /// 读取值
        /// </summary>
        public string READ_VALUE { get; set; }
    }
}