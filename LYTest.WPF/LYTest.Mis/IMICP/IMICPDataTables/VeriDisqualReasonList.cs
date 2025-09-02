using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 检定综合结论 - 不合格原因
    /// </summary>
    public class VeriDisqualReasonList
    {
        /// <summary>
        /// 检定任务号
        /// </summary>
        public string veriTaskNo { get; set; }

        /// <summary>
        /// 检定系统编号
        /// </summary>
        public string sysNo { get; set; }

        /// <summary>
        /// 检定单元编号
        /// </summary>
        public string veriUnitNo { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string barCode { get; set; }

        /// <summary>
        /// 不合格原因编码
        /// </summary>
        public string disqualReason { get; set; }

    }
}
