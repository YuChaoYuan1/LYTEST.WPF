namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 时钟示值误差
    /// </summary>
    public class MT_CLOCK_VALUE_MET_CONC : MT_MET_CONC_Base
    {


        //IS_VALID	VARCHAR2(8)	Y有效标志
        public string IS_VALID { get; set; }


        //STD_DATE	VARCHAR2(32)	Y标准时间
        public string STD_DATE { get; set; }

        //MET_DATE	VARCHAR2(32)	Y电表时间
        public string MET_DATE { get; set; }

        //TIME_ERR	VARCHAR2(32)	Y时间差值
        public string TIME_ERR { get; set; }

        //MET_VALUE	NUMBER(16)	Y电能表显示的时刻T/s
        public string MET_VALUE { get; set; }

        //STD_VALUE	NUMBER(16)	Y标准时刻T/s
        public string STD_VALUE { get; set; }


        //新加

        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_DOWM { get; set; }

    }

}
