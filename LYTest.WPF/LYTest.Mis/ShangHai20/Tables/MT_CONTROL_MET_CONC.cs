namespace LYTest.Mis.ShangHai20.Tables
{
    /// <summary>
    /// 控制功能 @C_B
    /// </summary>
    public class MT_CONTROL_MET_CONC : MT_MET_CONC_Base
    {
        public string EQUIP_ID { get; set; }

        /// <summary>
        /// 预置报警金额1
        /// </summary>
        public string SETTING_WARN_VALUE1 { get; set; }

        /// <summary>
        /// 预置报警金额2
        /// </summary>
        public string SETTING_WARN_VALUE2 { get; set; }

        /// <summary>
        /// 实际报警金额1
        /// </summary>
        public string REAL_WARN_VALUE1 { get; set; }

        /// <summary>
        /// 实际报警金额2
        /// </summary>
        public string REAL_WARN_VALUE2 { get; set; }

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// A12          01合格、02不合格 
        /// </summary>
        public string CHK_CONC_CODE { get; set; }


    }
}
