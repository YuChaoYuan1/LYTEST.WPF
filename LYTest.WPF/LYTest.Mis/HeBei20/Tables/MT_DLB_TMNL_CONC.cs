namespace LYTest.Mis.HeBei20.Tables
{
    //DLB2007抄表试验结论
    public class MT_DLB_TMNL_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 检验次数
        /// </summary>
        public string DETECT_NUM { get; set; }
        /// <summary>
        /// 检验类别
        /// </summary>
        public string DETECT_TYPE { get; set; }
        /// <summary>
        /// 冻结代码
        /// </summary>
        public string FROZEN_CODE { get; set; }
        /// <summary>
        /// 冻结项目
        /// </summary>
        public string FROZEN_ITEM { get; set; }

        /// <summary>
        /// 不合格原因
        /// </summary>
        public string FROZEN_CONTENT { get; set; }
        /// <summary>
        /// 不合格原因
        /// </summary>
        public string UNPASS_REASON { get; set; }
    }
}