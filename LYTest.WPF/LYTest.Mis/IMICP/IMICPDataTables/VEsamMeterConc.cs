using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能表密钥下装结论
    /// </summary>
    class VEsamMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string rv { get; set; }  //	电压
        public string keyType { get; set; } //	密钥类型
        public string keyStatus { get; set; }   //	密钥状态
        public string keyNum { get; set; }  //	密钥条数
        public string keyVer { get; set; }  //	密钥版本
        public string checkConc { get; set; }	//	结论

    }
}
