namespace LYTest.Mis.ShangHai20.Tables
{
    public class MT_SURPLUS_MET_CONC : MT_MET_CONC_Base
    {


        /// <summary>
        /// 设备的唯一标识
        /// </summary>
        public string EQUIP_ID { get; set; }


        /// <summary>
        /// Eo累计用电能量增加数
        /// </summary>
        public string SUM_INCREASE { get; set; }

        /// <summary>
        /// ΔE 剩余电能量减少数
        /// </summary>
        public string SURPLUS_REDUCE { get; set; }

        /// <summary>
        /// |E0-ΔE|误差值
        /// </summary>
        public string SURPLUS_VALUE { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string CHK_CONC_CODE { get; set; }
    }
}
