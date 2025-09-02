namespace LYTest.Mis.ShangHai20.Tables
{
    //交流电压试验结论
    public class MT_VOLT_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 测试耐压值 （伏）
        /// </summary>
        public string VOLT_TEST_VALUE { get; set; }
        /// <summary>
        /// 耐压对象
        /// </summary>
        public string VOLT_OBJ { get; set; }

        /// <summary>
        /// 漏电流阀值
        /// </summary>
        public string LEAK_CURRENT_LIMIT { get; set; }
        /// <summary>
        /// 表位漏电流阀值
        /// </summary>
        public string POSITION_LEAK_LIMIT { get; set; }


        /// <summary>
        /// 测试时长（秒）
        /// </summary>
        public string TEST_TIME { get; set; }
        /// <summary>
        /// 测试日期
        /// </summary>
        public string VOLT_DATE { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
    }
}