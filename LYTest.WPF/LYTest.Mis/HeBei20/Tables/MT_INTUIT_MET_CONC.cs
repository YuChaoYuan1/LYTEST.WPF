namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 外观检查试验结论
    /// </summary>
    public class MT_INTUIT_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 进行外观检查试验的检修内容，01表盘是否破损、02表内有异物【需定】
        /// </summary>
        public string DETECT_CONTENT { get; set; }
    }
}