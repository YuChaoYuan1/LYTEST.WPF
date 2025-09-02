using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 设备码参数信息
    /// </summary>
    public class DevCodePara
    {
		public string metCateg { get; set; }    //	电能表类别
		public string metType { get; set; } //	电能表类型
		public string metSpec { get; set; } //	电能表规格
		public string metWireMode { get; set; } //	接线方式
		public string metRv { get; set; }   //	电能表电压
		public string metRc { get; set; }   //	电能表标定电流
		public string metApAccuLv { get; set; } //	有功准确度等级
		public string metRpAccuLv { get; set; } //	无功准确度等级
		public string metCommProt { get; set; } //	通讯规约
		public string metBidiMeterFlag { get; set; }    //	是否双向计量
		public string metTripMode { get; set; } //	卡表跳闸方式

	}
}
