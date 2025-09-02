using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表潜动试验结论
    /// </summary>
    class VCreepingMeterConc : CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string constConcCode { get; set; }   //	常数试验
		public string loadVoltage { get; set; } //	电压比值
		public string volt { get; set; }    //	电压
		public string pules { get; set; }   //	脉冲数
		public string bothWayPowerFlag { get; set; }    //	功率方向
		public string loadCurrent { get; set; } //	电流负载
		public string testTime { get; set; }    //	检测时间
		public string testCircleNum { get; set; }   //	测试圈数
		public string realTestTime { get; set; }    //	实际时间
		public string checkConc { get; set; }   //	结论

	}
}
