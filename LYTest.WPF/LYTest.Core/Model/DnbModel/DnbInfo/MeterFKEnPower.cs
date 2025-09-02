using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 远程保电
    /// </summary>
    [Serializable()]
    public class MeterFKEnPower : MeterBase
    {
        /// <summary>
        /// 保电解除后状态字
        /// </summary>
        public string EnPowerOffState { get; set; }
        /// <summary>
        /// 保电开启后状态字
        /// </summary>
        public string EnPowerOnState { get; set; }
        /// <summary>
        /// 是否合格
        /// </summary>
        public string Result { get; set; }
    }
}
