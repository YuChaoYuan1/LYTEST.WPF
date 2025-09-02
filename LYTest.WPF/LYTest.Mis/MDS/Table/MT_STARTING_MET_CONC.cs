namespace LYTest.Mis.MDS.Table
{
    /// <summary>
    ///  起动试验结论
    /// </summary>
    public class MT_STARTING_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 电压负载
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }

        /// <summary>
        /// 圈数
        /// </summary>
        public string PULES { get; set; }

        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }

        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }

        /// <summary>
        /// 测试时长：秒
        /// </summary>
        public string TEST_TIME { get; set; }

        /// <summary>
        /// 实际时间
        /// </summary>
        public string REAL_TEST_TIME { get; set; }


        /// <summary>
        /// 起动电流
        /// </summary>
        public string START_CURRENT { get; set; }

    }
}
