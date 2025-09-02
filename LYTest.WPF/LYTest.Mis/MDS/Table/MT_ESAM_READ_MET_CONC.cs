using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    /// <summary>
    /// 费控试验
    /// </summary>
    public class MT_ESAM_READ_MET_CONC : MT_MET_CONC_Base
    {
        ///// <summary>
        ///// 有效标注位     0：无效 1：有效
        ///// </summary>
        //public string IS_VALID { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }
        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 试验项目
        /// </summary>
        public string TEST_ITEM { get; set; }
        /// <summary>
        /// 试验方法    
        /// </summary>
        public string TEST_METHOD_DIFFER { get; set; }
        /// <summary>
        /// 跳闸延时时间秒
        /// </summary>
        public string TRIP_DELAY_TIME { get; set; }

    }

}
