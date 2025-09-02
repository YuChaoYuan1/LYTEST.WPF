using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 外观结论
    /// </summary>
    class VIntuitMeterConc : CommonDatas_DETedTestData
    {
        /// <summary>
        /// 小项参数顺序号
        /// </summary>
        public string sn { get; set; }  //	
        /// <summary>
        /// 检定点的序号
        /// </summary>
        public string veriPointSn { get; set; } //
        /// <summary>
        /// 	有效标志
        /// </summary>
        public string validFlag { get; set; }
        /// <summary>
        /// 	检修内容
        /// </summary>
        public string chkCont { get; set; }
        /// <summary>
        /// 	结论
        /// </summary>
        public string checkConc { get; set; }

    }
}
