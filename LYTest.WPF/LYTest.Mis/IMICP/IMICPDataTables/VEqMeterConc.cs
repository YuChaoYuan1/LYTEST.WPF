using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
	/// <summary>
	/// 电能表剩余电量递减试验结论
	/// </summary>
	public class VEqMeterConc : CommonDatas_DETedTestData
	{
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string totalEq { get; set; } //	总电量
		public string surplusEq { get; set; }   //	剩余电量
		public string currElecPrice { get; set; }   //	当前电价
		public string loadCurrent { get; set; } //	电流负载
		public string pf { get; set; }  //	功率因数
		public string checkConc { get; set; }   //	结论

	}
}
