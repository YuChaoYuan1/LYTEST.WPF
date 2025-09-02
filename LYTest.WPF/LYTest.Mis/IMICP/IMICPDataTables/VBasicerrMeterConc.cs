using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    ///  电能表基本误差试验结论
    /// </summary>
    public class VBasicerrMeterConc : CommonDatas_DETedTestData
	{


		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string bothWayPowerFlag { get; set; }    //	功率方向
		public string curPhaseCode { get; set; }    //	电流相别
		public string loadCurrent { get; set; } //	电流负载
		public string loadVoltage { get; set; } //	电压比值
		public string trialFreq { get; set; }   //	频率
		public string pf { get; set; }  //	功率因数
		public string detectCircle { get; set; }    //	检验脉冲数/圈数
		public string simpling { get; set; }    //	采样次数
		public string actlError { get; set; }   //	实际误差
		public string aveErr { get; set; }  //	误差平均值
		public string intConvertErr { get; set; }   //	误差化整值
		public string errUp { get; set; }   //	误差上限
		public string errDown { get; set; } //	误差下限
		public string checkConc { get; set; }   //	结论

	}
}
