namespace LYTest.Mis.HeBei20.Tables
{
    //功率消耗结论
    public class MT_POWER_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// 电压回路有功功率
        /// </summary>
        public string VOL_ACT_POWER { get; set; }

        /// <summary>
        /// 电压回路视在功率
        /// </summary>
        public string VOL_INS_POWER { get; set; }

        /// <summary>
        /// 电流回路有功功率
        /// </summary>
        public string CUR_ACT_POWER { get; set; }

        /// <summary>
        /// 电流回路视在功率
        /// </summary>
        public string CUR_INS_POWER { get; set; }

        /// <summary>
        /// 电压回路有功功率误差限
        /// </summary>
        public string VOL_ACT_POWER_ERR { get; set; }

        /// <summary>
        /// 电压回路视在功率误差限
        /// </summary>
        public string VOL_INS_POWER_ERR { get; set; }

        /// <summary>
        /// 电流回路有功功率误差限
        /// </summary>
        public string CUR_ACT_POWER_ERR { get; set; }

        /// <summary>
        /// 电流回路视在功率误差限
        /// </summary>
        public string CUR_INS_POWER_ERR { get; set; }

        /// <summary>
        /// 电压回路有功功率结论
        /// </summary>
        public string VOL_ACT_POWER_RESULT { get; set; }

        /// <summary>
        /// 电压回路视在功率结论
        /// </summary>
        public string VOL_INS_POWER_RESULT { get; set; }

        /// <summary>
        /// 电流回路有功功率结论
        /// </summary>
        public string CUR_ACT_POWER_RESULT { get; set; }

        /// <summary>
        /// 电流回路视在功率结论
        /// </summary>
        public string CUR_INS_POWER_RESULT { get; set; }

        /// <summary>
        /// 试验项目
        /// </summary>
        public string TEST_ITEM { get; set; }

        /// <summary>
        /// 误差限
        /// </summary>
        public string ERR_ABS { get; set; }


        private string _POWER_CONSUM_TYPE;
        /// <summary>
        /// 电能表功耗试验项目
        /// </summary>
        public string POWER_CONSUM_TYPE
        {
            get { return _POWER_CONSUM_TYPE; }
            set
            {
                if (value.Length > 2)
                    _POWER_CONSUM_TYPE = value.Substring(0, 2);
                else
                    _POWER_CONSUM_TYPE = value;
            }
        }
    }
}