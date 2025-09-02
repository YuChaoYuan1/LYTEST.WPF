using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    /// <summary>
    /// 费控安全认证试验结论(身份认证)
    /// </summary>
    public class MT_ESAM_SECURITY_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// ESAM序列号
        /// </summary>
        public string ESAM_ID { get; set; }
    }
}
