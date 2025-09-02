using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 6.13 施封信息集合
    /// </summary>
    public class SealInst
    {
        public string barCode { get; set; } //	条形码
        public string sealPosition { get; set; }    //	施封位置
        public string sealBarCode { get; set; } //	施封条码
        public string sealDate { get; set; }    //	施封日期
        public string sealerNo { get; set; }    //	施封人员
        public string validFlag { get; set; }   //	是否有效
        public string releaseInfo { get; set; }	//	发行信息

    }
}
