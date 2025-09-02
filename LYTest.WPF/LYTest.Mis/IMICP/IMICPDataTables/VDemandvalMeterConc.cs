using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表需量示值误差结论
    /// </summary>
    public class VDemandvalMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string demandPeriod { get; set; }    //	需量周期时间
        public string demandTime { get; set; }  //	滑差时间
        public string demandInterval { get; set; }  //	滑差次数
        public string realDemand { get; set; }  //	实际需量
        public string realPeriod { get; set; }  //	实际周期
        public string demandValueErr { get; set; }  //	需量示值误差
        public string demandStandard { get; set; }  //	标准表需量值
        public string demandValueErrAbs { get; set; }   //	需量示值误差限
        public string clearDataRst { get; set; }    //	需量清零结果
        public string valueConcCode { get; set; }   //	示值误差结论
        public string bothWayPowerFlag { get; set; }    //	功率方向
        public string rv { get; set; }  //	电压
        public string loadCurrent { get; set; } //	电流负载
        public string pf { get; set; }  //	功率因数
        public string controlMethod { get; set; }   //	控制方法
        public string errUp { get; set; }   //	误差上限
        public string errDowm { get; set; } //	误差下限
        public string intConvertErr { get; set; }   //	需量示值误差化整值
        public string demandMeter { get; set; } //	被检表需量值
        public string checkConc { get; set; }   //	结论
    }
}
