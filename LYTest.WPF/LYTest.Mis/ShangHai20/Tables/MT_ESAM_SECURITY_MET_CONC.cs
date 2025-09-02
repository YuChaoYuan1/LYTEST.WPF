namespace LYTest.Mis.ShangHai20.Tables
{
    /// <summary>
    /// 费控安全认证试验结论
    /// </summary>
    public class MT_ESAM_SECURITY_MET_CONC : MT_MET_CONC_Base
    {

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// ESAM序列号
        /// </summary>
        public string ESAM_ID { get; set; }
    }
}