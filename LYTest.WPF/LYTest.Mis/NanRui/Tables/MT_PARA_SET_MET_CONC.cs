namespace LYTest.Mis.NanRui.Tables
{
    //任意数据写入
    public class MT_PARA_SET_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 以|分割与读取值一一对应
        /// </summary>
        public string DATA_FLAG { get; set; }
        /// <summary>
        /// 以|分割
        /// </summary>
        public string SETTING_VALUE { get; set; }
    }
}