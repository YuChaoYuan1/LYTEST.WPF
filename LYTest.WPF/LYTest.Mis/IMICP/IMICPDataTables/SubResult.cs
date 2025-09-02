using System.Collections.Generic;

namespace LYTest.Mis.IMICP.IMICPDataTables
{

    /// <summary>
    /// 6.106.13 检定分项结论接口
    /// </summary>
    public class SubResult
    {

        public string sysNo { get; set; }//系统编号
        public string veriTaskNoo { get; set; }//检定任务编号
        public string devClso { get; set; }//设备分类

        /// <summary>
        /// 电能表基本误差试验结论
        /// </summary>
        public List<VBasicerrMeterConc> vBasicerrMeterConc { get; set; } = new List<VBasicerrMeterConc>();

        /// <summary>
        /// 电能表时钟示值误差
        /// </summary>
        public List<VClockValueMeterConc> vClockValueMeterConc { get; set; } = new List<VClockValueMeterConc>();

    }
}
