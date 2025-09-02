using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    /// <summary>
    /// 外观检查试验结论
    /// </summary>
    public class MT_INTUIT_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 进行外观检查试验的检修内容，01表盘是否破损、02表内有异物【需定】
        /// </summary>
        public string DETECT_CONTENT { get; set; }
    }
}
