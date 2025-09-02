using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 电表清零
    /// </summary>
    [Serializable()]
    public class MeterClearEnerfy : MeterBase
    {
        /// <summary>
        /// 清零前电量
        /// </summary>
        public string ClearEnerfyBefore { get; set; }

        /// <summary>
        /// 清零后电量
        /// </summary>
        public string ClearEnerfyAfter { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public string AvgValue { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }
    }
}
