using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表分项事件记录功能
    /// </summary>
    class VEventRcdMeterConc : CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string loadCurrent { get; set; } //	电流负载
		public string pf { get; set; }  //	功率因数
		public string simpling { get; set; }    //	采样次数
		public string overloadTime { get; set; }    //	过载时间
		public string recoverTime { get; set; } //	恢复等待时间
		public string errUp { get; set; }   //	误差上限
		public string errDown { get; set; } //	误差下限
		public string checkConc { get; set; }   //	结论
		public string pules { get; set; }   //	脉冲数

	}
}
