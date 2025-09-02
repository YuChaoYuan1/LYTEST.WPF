using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 7.3 设备参数信息获取接口(通用) 电能表返回数据--根据条形码返回数据
    /// </summary>
    public class MeterDet
    {
        public string barCode { get; set; } //	条形码
        public string metCateg { get; set; }    //	电能表类别
        public string metType { get; set; } //	电能表类型
        public string metModel { get; set; }    //	电能表型号
        public string metSpec { get; set; } //	电能表规格
        public string metWireMode { get; set; } //	接线方式
        public string metRv { get; set; }   //	电能表电压
        public string metRc { get; set; }   //	电能表标定电流
        public string metOverloadTimes { get; set; }    //	电能表过载倍数
        public string metApAccuLv { get; set; } //	有功准确度等级
        public string metRpAccuLv { get; set; } //	无功准确度等级
        public string metDigits { get; set; }   //	电能表示数位数
        public string metTimeSecDigit { get; set; } //	电能表分时位数
        public string metApConst { get; set; }  //	电能表有功常数
        public string metRpConst { get; set; }  //	电能表无功常数
        public string metBidiMeterFlag { get; set; }    //	是否双向计量
        public string metPrepayFlag { get; set; }   //	是否预付费
        public string metDmdMeterFlag { get; set; } //	是否需量表
        public string metPulseCateg { get; set; }   //	脉冲类别
        public string metFreq { get; set; } //	额定频率
        public string metAccsMode { get; set; } //	接入方式
        public string metUsage { get; set; }    //	使用用途
        public string metMeasTheory { get; set; }   //	测量原理
        public string metBrgStruc { get; set; } //	轴承结构
        public string metCommIntfcType { get; set; }    //	通讯接口类型
        public string metRelayContact { get; set; } //	继电器接点
        public string metVoltLossJudge { get; set; }    //	失压判断
        public string metCurLossJudge { get; set; } //	失流判断
        public string metRevPhaseSeqJudge { get; set; } //	逆相序判断
        public string metPwrOverLmtFlag { get; set; }   //	超功率
        public string metLoadCurve { get; set; }    //	负荷曲线
        public string metPwrOffMRFlag { get; set; } //	停电抄表
        public string metIrMR { get; set; } //	红外抄表
        public string metTripMode { get; set; } //	卡表跳闸方式
        public string metReadingMode { get; set; }  //	计度器方式
        public string metDispMode { get; set; } //	显示方式
        public string metCommHardVer { get; set; }  //	硬件版本
        public string metCommProt { get; set; } //	通讯规约
        public string metCommMode { get; set; } //	通讯方式
        public string metRate { get; set; } //	费率
        public string metRpRc { get; set; } //	无功电流
        public string metBatteryReplaceFlag { get; set; }   //	电池模块可更换标志
        public string metBaudRate { get; set; } //	波特率
        public string estabArchDate { get; set; }   //	建档日期
        public string lastChkDate { get; set; } //	上次检定时间
        public string devCodeNo { get; set; }   //	设备码
        public string devStat { get; set; } //	设备状态
        public string ftyNo { get; set; }   //	出厂编号
        public string arrBatchNo { get; set; }  //	到货批次号
        public string commAddr { get; set; }    //	通讯地址
        public string bkltOn { get; set; }  //	上电点亮背光
        public string fullDispSec { get; set; } //	全屏时间
        public string totalScreenNo { get; set; }   //	总底度屏显顺序
        public string sharpScreenNo { get; set; }   //	尖底度屏显顺序
        public string peakScreenNo { get; set; }    //	峰底度屏显顺序
        public string flatScreenNo { get; set; }    //	平底度屏显顺序
        public string valleyScreenNo { get; set; }  //	谷底度屏显顺序
        public string overdraftAmountLmt { get; set; }  //	透支金额限值
        public string hoardAmountLmt { get; set; }  //	囤积金额限值
        public string closingAmountLmt { get; set; }    //	合闸允许金额限值
        public string rmReason { get; set; }    //	拆回原因
        public string faultReason { get; set; } //	故障现象
        public string assetNo { get; set; }	//	资产编号

    }
}
