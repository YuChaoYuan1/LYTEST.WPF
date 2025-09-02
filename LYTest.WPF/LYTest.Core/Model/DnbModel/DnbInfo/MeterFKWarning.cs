using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 报警功能
    /// </summary>
    [Serializable()]
    public class MeterFKWarning : MeterBase
    {
        /// <summary>
        /// 报警开启后状态
        /// </summary>
        public string WarningOnSata { get; set; }

        /// <summary>
        /// 报警解除后状态
        /// </summary>
        public string WarningOffSata { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }
    }
}
