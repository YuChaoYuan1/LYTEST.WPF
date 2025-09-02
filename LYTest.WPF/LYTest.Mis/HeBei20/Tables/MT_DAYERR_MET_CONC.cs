namespace LYTest.Mis.HeBei20.Tables
{
    //由电源供电的时钟试验结论
    public class MT_DAYERR_MET_CONC : MT_MET_CONC_Base
    {

        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// 秒脉冲频率
        /// </summary>
        public string SEC_PILES { get; set; }

        /// <summary>
        /// 测试时长：秒
        /// </summary>
        public string TEST_TIME { get; set; }

        /// <summary>
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }

        /// <summary>
        /// 误差：以“|”分割(时测值)
        /// </summary>
        public string ERROR { get; set; }

        /// <summary>
        /// 平均误差
        /// </summary>
        public string AVG_ERR { get; set; }

        /// <summary>
        /// 化整误差
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }

        /// <summary>
        /// 秒/天，默认0.5
        /// </summary>
        public string ERR_ABS { get; set; }


        //以下为新加入

        /// <summary>
        /// 电压负载
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }

    }
}