using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表最大需量周期误差结论
    /// </summary>
    class VMaxDmdErrMeterConc :CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string loadCurrent { get; set; } //	电流负载
		public string loadVoltage { get; set; } //	电压负载
		public string trialFreq { get; set; }   //	频率
		public string pf { get; set; }  //	功率因数
		public string dmdError { get; set; }    //	误差
		public string testAvgErr { get; set; }  //	平均误差
		public string intConvertErr { get; set; }   //	化整误差
		public string checkConc { get; set; }   //	结论
		public string writeDate { get; set; }   //	写入时间
		public string handleFlag { get; set; }  //	处理标志
		public string handleDate { get; set; }  //	处理时间

	}
}
