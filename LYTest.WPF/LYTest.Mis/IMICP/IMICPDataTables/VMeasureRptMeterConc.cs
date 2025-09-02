using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
	/// <summary>
	/// 电能表测量重复性试验(新规)
	/// </summary>
	public class VMeasureRptMeterConc : CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string rv { get; set; }  //	电压
		public string bothWayPowerFlag { get; set; }    //	功率方向
		public string loadCurrent { get; set; } //	电流负载
		public string pf { get; set; }  //	功率因数
		public string simpling { get; set; }    //	采样次数
		public string deviationLimt { get; set; }   //	标准偏差限
		public string checkConc { get; set; }   //	结论

	}
}
