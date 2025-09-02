using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表记度器示值组合误差
    /// </summary>
    class VHutCombinaMeterConc : CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string bothWayPowerFlag { get; set; }    //	功率方向
		public string loadCurrent { get; set; } //	电流负载
		public string pf { get; set; }  //	功率因数
		public string feeRatio { get; set; }    //	费率
		public string controlWay { get; set; }  //	控制方式
		public string irTime { get; set; }  //	走字时间
		public string irReading { get; set; }   //	走字度数
		public string errUp { get; set; }   //	误差上限
		public string errDown { get; set; } //	误差下限
		public string voltage { get; set; } //	承压(1为1%)
		public string totalReadingErr { get; set; } //	总分电量值差(kWh)
		public string totalIncrement { get; set; }  //	总示值增量
		public string sumerAllIncrement { get; set; }   //	各费率示值增量和
		public string sharpIncrement { get; set; }  //	尖示值增量
		public string peakIncrement { get; set; }   //	峰示值增量
		public string flatIncrement { get; set; }   //	平示值增量
		public string valleyIncrement { get; set; } //	谷示值增量
		public string valueConcCode { get; set; }   //	示值组合误差
		public string feeValue { get; set; }    //	费率示值
		public string startValue { get; set; }  //	起始示值
		public string endValue { get; set; }    //	结束示值
		public string eleIncrement { get; set; }    //	电能增量
		public string checkConc { get; set; }   //	结论
		public string otherIncrementOne { get; set; }    //	其他示值增量1
		public string otherIncrementTwo { get; set; }    //	其他示值增量2


	}
}
