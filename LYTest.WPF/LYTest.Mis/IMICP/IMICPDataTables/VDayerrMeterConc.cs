using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表日计时误差结论
    /// </summary>
    public class VDayerrMeterConc :CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string secPiles { get; set; }    //	秒脉冲频率
		public string testTime { get; set; }    //	测试时长
		public string simpling { get; set; }    //	采样次数
		public string actlError { get; set; }   //	误差
		public string testAvgErr { get; set; }  //	平均误差
		public string intConvertErr { get; set; }   //	化整误差
		public string errAbs { get; set; }  //	误差限
		public string checkConc { get; set; }   //	结论

	}
}
