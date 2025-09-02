using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表常数试验结论
    /// </summary>
    public class VConstMeterConc : CommonDatas_DETedTestData
	{
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string loadCurrent { get; set; } //	电流负载
		public string bothWayPowerFlag { get; set; }    //	功率方向
		public string realPules { get; set; }   //	实测脉冲数
		public string startReading { get; set; }    //	起始度数
		public string endReading { get; set; }  //	结束度数
		public string diffReading { get; set; } //	走字差值
		public string standardReading { get; set; } //	标准表度数
		public string actlError { get; set; }   //	实际误差
		public string errDown { get; set; } //	误差下限
		public string errUp { get; set; }   //	误差上限
		public string constConcCode { get; set; }   //	常数试验
		public string constErr { get; set; }    //	常数误差
		public string rv { get; set; }  //	电压
		public string pf { get; set; }  //	功率因数
		public string feeStartTime { get; set; }    //	费率起始时间
		public string rate { get; set; }    //	费率
		public string controlMethod { get; set; }   //	控制方式
		public string qualifiedPules { get; set; }  //	理论脉冲数
		public string divideElectricQuantity { get; set; }  //	总分电量值
		public string valueConcCode { get; set; }   //	示值误差
		public string irLastReading { get; set; }   //	装拆起始度数
		public string readType { get; set; }    //	示数类型
		public string checkConc { get; set; }   //	结论

	}
}
