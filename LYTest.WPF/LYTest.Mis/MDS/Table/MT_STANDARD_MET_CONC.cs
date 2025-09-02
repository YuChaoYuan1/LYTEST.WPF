using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    //规约一致性检查结论
    public class MT_STANDARD_MET_CONC : MT_MET_CONC_Base
    {
        ///// <summary>
        ///// A10       0：无效 1：有效
        ///// </summary>
        //public string IS_VALID { get; set; }

        /// <summary>
        /// 数据标识码
        /// </summary>
        public string DATA_FLAG { get; set; }

        /// <summary>
        /// 设定值
        /// </summary>
        public string SETTING_VALUE { get; set; }

        /// <summary>
        /// 读取值
        /// </summary>
        public string READ_VALUE { get; set; }


        /// <summary>
        /// 判定依据
        /// </summary>
        public string DETECT_BASIS { get; set; }
    }
}
