namespace LYTest.Mis.NanRui.Tables
{
    //电流过载试验结论
    public class MT_OVERLOAD_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 秒
        /// </summary>
        public string OVERLOAD_TIME { get; set; }
        /// <summary>
        /// 秒
        /// </summary>
        public string WAIT_TIME { get; set; }
        /// <summary>
        /// 圈数
        /// </summary>
        public string PULES { get; set; }
        /// <summary>
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }
        /// <summary>
        /// 误差
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        /// 平均误差
        /// </summary>
        public string AVG_ERR { get; set; }
        /// <summary>
        /// 化整误差
        /// </summary>
        public string INT_CONVERTER_ERR { get; set; }
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