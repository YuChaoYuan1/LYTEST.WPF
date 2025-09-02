using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
	/// <summary>
	/// 电能表485密码修改
	/// </summary>
	public class VPwdChangeMeterConc :CommonDatas_DETedTestData
    {
		public string sn { get; set; }  //	小项参数顺序号
		public string veriPointSn { get; set; } //	检定点的序号
		public string validFlag { get; set; }   //	有效标志
		public string passwordLevel { get; set; }   //	密码级别
		public string oldPassword { get; set; } //	原密码
		public string newPassword { get; set; } //	新密码
		public string checkConc { get; set; }   //	结论

	}
}
