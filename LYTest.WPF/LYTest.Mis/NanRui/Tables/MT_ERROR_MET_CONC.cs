namespace LYTest.Mis.NanRui.Tables
{
    /// <summary>
    /// 变差要求试验结论
    /// </summary>
    public class MT_ERROR_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }
        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 圈数
        /// </summary>
        public string PULES { get; set; }
        /// <summary>
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }
        /// <summary>
        /// 一次误差
        /// </summary>
        public string ONCE_ERR { get; set; }
        /// <summary>
        /// 一次平均误差
        /// </summary>
        public string AVG_ONCE_ERR { get; set; }
        /// <summary>
        /// 一次化整误差
        /// </summary>
        public string INT_ONCE_ERR { get; set; }
        /// <summary>
        /// 二次误差
        /// </summary>
        public string TWICE_ERR { get; set; }
        /// <summary>
        /// 二次平均误差
        /// </summary>
        public string AVG_TWICE_ERR { get; set; }
        /// <summary>
        /// 二次化整误差
        /// </summary>
        public string INT_TWICE_ERR { get; set; }
        /// <summary>
        /// 误差变差
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }
        /// <summary>
        /// 误差下限
        /// </summary>
        public string ERR_DOWN { get; set; }


        //以下为新加入

        /// <summary>
        /// 电压负载
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }

    }
}