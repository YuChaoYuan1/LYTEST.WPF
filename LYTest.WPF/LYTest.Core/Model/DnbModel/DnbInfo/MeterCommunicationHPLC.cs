using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 通信性能检测(HPCL)
    /// </summary>
    [Serializable()]
    public class MeterCommunicationHPLC : MeterBase
    {
        /// <summary>
        /// 电量
        /// </summary>
        public string Electricity { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }

    }
}
