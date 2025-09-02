namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    public class MeterRegister : MeterBase
    {
        /// <summary>
        /// 费率
        /// </summary>
        public string FL { get; set; }
        /// <summary>
        /// 试验前总电量
        /// </summary>
        public string TopTotalPower { get; set; }

        /// <summary>
        /// 试验后总电量
        /// </summary>
        public string EndTotalPower { get; set; }
        /// <summary>
        /// 总电量差值
        /// </summary>
        public string TotalPowerD { get; set; }

        /// <summary>
        /// 试验前费率电量
        /// </summary>
        public string TopFLPower { get; set; }

        /// <summary>
        /// 试验后费率电量
        /// </summary>
        public string EndFLPower { get; set; }

        /// <summary>
        /// 费率电量差值
        /// </summary>
        public string FLPowerD { get; set; }

        /// <summary>
        /// 组合误差
        /// </summary>
        public string CombError { get; set; }


        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }

    }
}
