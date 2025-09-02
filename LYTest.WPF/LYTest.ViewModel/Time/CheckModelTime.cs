using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Time
{
    /// <summary>
    /// 时间计算需要的参数
    /// </summary>
    public class CheckModelTime
    {
        /// <summary>
        /// 检定项编号
        /// </summary>
        public string ParaNo { get; set; }
        /// <summary>
        /// 检定参数
        /// </summary>
        public string ParaValue { get; set; }
        /// <summary>
        /// 电表信息
        /// </summary>
        public string MeterInfo { get; set; }
    }
}
