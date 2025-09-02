using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 远程控制
    /// </summary>
    [Serializable()]
    public class MeterEncryptionControl : MeterBase
    {
        /// <summary>
        /// 合闸动作状态字
        /// </summary>
        public string SwitchOffState { get; set; }
        /// <summary>
        /// 拉着动作状态字
        /// </summary>
        public string SwitchOnState { get; set; }
        /// <summary>
        /// 是否合格
        /// </summary>
        public string Result { get; set; }
    }
}
