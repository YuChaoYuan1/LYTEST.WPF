using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表误差变差试验结论
    /// </summary>
    public class VErrorMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string bothWayPowerFlag { get; set; }    //	功率方向
        public string loadCurrent { get; set; } //	电流负载
        public string pf { get; set; }  //	功率因数
        public string pules { get; set; }   //	圈数
        public string simpling { get; set; }    //	采样次数
        public string onceErr { get; set; } //	一次误差
        public string avgOnceErr { get; set; }  //	一次平均误差
        public string intOnceErr { get; set; }  //	一次化整误差
        public string twiceErr { get; set; }    //	二次误差
        public string avgTwiceErr { get; set; } //	二次平均误差
        public string intTwiceErr { get; set; } //	二次化整误差
        public string actlError { get; set; }   //	误差变差
        public string errUp { get; set; }   //	误差上限
        public string errDown { get; set; } //	误差下限
        public string checkConc { get; set; }	//	结论

    }
}
