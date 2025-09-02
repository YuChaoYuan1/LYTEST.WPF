using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 芯片ID认证
    /// </summary>
    [Serializable()]
    public class MeterHPLCIDAuthen : MeterBase
    {
        /// <summary>
        /// 芯片ID
        /// </summary>
        public string ChipID { get; set; }

        /// <summary>
        /// 19结论：[合格,不合格]
        /// </summary>
        public string Result { get; set; }
    }
}
