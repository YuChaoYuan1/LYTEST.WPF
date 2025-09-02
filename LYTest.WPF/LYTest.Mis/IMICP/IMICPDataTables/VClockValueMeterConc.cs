using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
	/// <summary>
	/// 电能表时钟示值误差
	/// </summary>
	public class VClockValueMeterConc : CommonDatas_DETedTestData
	{
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string stdDate { get; set; } //	标准日期
		public string meterDate { get; set; }   //	电表日期
		public string timeErr { get; set; } //	时间差值
		public string meterValue { get; set; }  //	显示的时刻
		public string stdValue { get; set; }    //	标准时刻
		public string checkConc { get; set; }   //	结论
		public string writeDate { get; set; }   //	写入时间

	}
}
