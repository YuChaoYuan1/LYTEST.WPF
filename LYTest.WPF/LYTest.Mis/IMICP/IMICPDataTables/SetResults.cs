using System.Collections.Generic;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 6.12  检定综合结论
    /// </summary>
    public class SetResults : CommonDatas
    {
        /// <summary>
        /// 检定明细单
        /// </summary>
        public List<VeriDtlFormList> veriDtlFormList { get; set; } = new List<VeriDtlFormList>();

        /// <summary>
        /// 不合格原因
        /// </summary>
        public List<VeriDisqualReasonList> veriDisqualReasonList { get; set; } = new List<VeriDisqualReasonList>();

        /// <summary>
        /// 综合结论
        /// </summary>
        public List<MeterVeriCompConc> meterVeriCompConc { get; set; } = new List<MeterVeriCompConc>();
    }
}
