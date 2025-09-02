using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    public class MT_PRESETPARAM_CHECK_MET_CONC : MT_MET_CONC_Base
    {

        /// <summary>
        /// 数据项名称
        /// </summary>
        public string DATA_ITEM_NAME { get; set; }
        /// <summary>
        /// 数据标识    
        /// </summary>
        public string DATA_IDENTION { get; set; }
        /// <summary>
        /// 控制码
        /// </summary>
        public string CONTROL_CODE { get; set; }
        /// <summary>
        /// 数据格式
        /// </summary>
        public string DATA_FORMAT { get; set; }
        /// <summary>
        /// 是否数据块
        /// </summary>
        public string IS_DATA_BLOCK { get; set; }
        /// <summary>
        /// 标准值
        /// </summary>
        public string STANDARD_VALUE { get; set; }
        /// <summary>
        /// 实际值
        /// </summary>
        public string REAL_VALUE { get; set; }
        /// <summary>
        /// 判定上限
        /// </summary>
        public string DETER_UPPER_LIMIT { get; set; }
        /// <summary>
        /// 判定下限
        /// </summary>
        public string DETER_LOWER_LIMIT { get; set; }
  

    }
}
