using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 电能计量功能
    /// </summary>
    public class VPowerMeasMeterConc :CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string checkConc { get; set; }   //	结论
        public string curPhaseCode { get; set; }    //	电流相别
        public string consistDown { get; set; } //	误差一致性下限
        public string consistUp { get; set; }   //	误差一致性上限
        public string voltRatio { get; set; }   //	电压比例
        public string errDown { get; set; } //	误差下限
        public string errUp { get; set; }   //	误差上限
        public string simplingSpace { get; set; }   //	采样间隔
        public string simpling { get; set; }    //	采样次数
        public string pf { get; set; }  //	功率因数
        public string loadCurrent { get; set; } //	电流负载
        public string bothWayPowerFlag { get; set; }	//	功率方向

    }


    /// <summary>
    /// 电能表功率消耗结论
    /// </summary>
    public class VPowerMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string volActPower { get; set; } //	电压回路有功功率
        public string volInsPower { get; set; } //	电压回路视在功率
        public string curActPower { get; set; } //	电流回路有功功率
        public string curInsPower { get; set; } //	电流回路视在功率
        public string volActPowerErr { get; set; }  //	电压回路有功功率误差限
        public string volInsPowerErr { get; set; }  //	电压回路视在功率误差限
        public string curActPowerErr { get; set; }  //	电流回路有功功率误差限
        public string curInsPowerErr { get; set; }  //	电流回路视在功率误差限
        public string volActPowerResult { get; set; }   //	电压回路有功功率结论
        public string volInsPowerResult { get; set; }   //	电压回路视在功率结论
        public string curActPowerResult { get; set; }   //	电流回路有功功率结论
        public string curInsPowerResult { get; set; }   //	电流回路视在功率结论
        public string trialProj { get; set; }   //	试验项目
        public string errAbs { get; set; }  //	误差限
        public string powerConsumType { get; set; } //	电能表功耗试验项目
        public string checkConc { get; set; }	//	结论

    }


    /// <summary>
    /// 电能表预置参数检查
    /// </summary>
    public class VPreCheckMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string dataName { get; set; }    //	数据项名称
        public string dataId { get; set; }  //	数据标识
        public string controlCode { get; set; } //	控制码
        public string dataFormat { get; set; }  //	数据格式
        public string dataBlockFlag { get; set; }   //	数据块标志
        public string standardValue { get; set; }   //	标准值
        public string deterUpperLimit { get; set; } //	上限
        public string deterLowerLimit { get; set; } //	下限
        public string realValue { get; set; }   //	实际值
        public string checkConc { get; set; }	//	结论


    }

    /// <summary>
    /// 电能表预置参数设置
    /// </summary>
    public class VPreParamMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; }  //	小项参数顺序号
        public string veriPointSn { get; set; } //	检定点的序号
        public string validFlag { get; set; }   //	有效标志
        public string dataName { get; set; }    //	数据项名称
        public string dataId { get; set; }  //	数据标识
        public string controlCode { get; set; } //	控制码
        public string dataFormat { get; set; }  //	数据格式
        public string dataBlockFlag { get; set; }   //	数据块标志
        public string standardValue { get; set; }   //	标准值
        public string delayWaitTime { get; set; }   //	延时等待时间
        public string loginPwd { get; set; }    //	密码
        public string checkConc { get; set; }	//	结论

    }




    /// <summary>

    /// 电能表规约一致性检查结论

    /// <summary>

    public class vStandardMeterConc : CommonDatas_DETedTestData
    {
        public string veriMeterRsltId { get; set; } //电能表综合结论记录标识

        public string areaCode { get; set; } //地区代码

        public string psOrgNo { get; set; } //供电单位编号

        public string devId { get; set; } //设备标识

        public string assetNo { get; set; } //资产编号

        public string barCode { get; set; } //条形码

        public string veriTaskNo { get; set; } //检定任务单编号

        public string devCls { get; set; } //设备分类

        public string sysNo { get; set; } //系统编号

        public string plantNo { get; set; } //设备档案编号

        public string plantElementNo { get; set; } //设备单元编号

        public string machNo { get; set; } //专机编号

        public string devSeatNo { get; set; } //表位编号

        public string veriCaliDate { get; set; } //检定/校准日期

        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string dataFlag { get; set; } //数据标识码

        public string settingValue { get; set; } //设定值

        public string readValue { get; set; } //读取值

        public string checkConc { get; set; } //结论

        public string chkBasis { get; set; } //判定依据

    }

    /// <summary>
    /// 电能表起动试验结论
    /// <summary>
    public class vStartingMeterConc : CommonDatas_DETedTestData
    {       
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string current { get; set; } //起动电流

        public string testTime { get; set; } //测试时长

        public string theorTime { get; set; } //理论时间

        public string checkConc { get; set; } //结论

        public string esamNo { get; set; } //ESAM序列号

    }

    /// <summary>

    /// 电能表时段投切误差结论

    /// <summary>

    public class vTsMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string rate { get; set; } //费率

        public string tsStartTime { get; set; } //测试起始时间

        public string tsRealTime { get; set; } //实际投切时间

        public string tsWay { get; set; } //实现方式

        public string tsErrConcCode { get; set; } //投切误差

        public string errAbs { get; set; } //误差限

        public string rv { get; set; } //电压

        public string startTime { get; set; } //时段起始时间

        public string errUp { get; set; } //误差上限

        public string errDowm { get; set; } //误差下限

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表负载电流升降变差试验结论
    /// <summary>
    public class vVariationMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string bothWayPowerFlag { get; set; } //功率方向

        public string loadCurrent { get; set; } //电流负载

        public string curPhaseCode { get; set; } //电流相别

        public string pf { get; set; } //功率因数

        public string detectCircle { get; set; } //检验圈数

        public string simpling { get; set; } //采样次数

        public string waitTime { get; set; } //升降电流等待时间

        public string upErr1 { get; set; } //升流误差1

        public string upErr2 { get; set; } //升流误差2

        public string avgUpErr { get; set; } //升流误差平均

        public string intUpErr { get; set; } //升流化整误差

        public string downErr1 { get; set; } //降流误差1

        public string downErr2 { get; set; } //降流误差2

        public string avgDownErr { get; set; } //降流误差平均

        public string intDownErr { get; set; } //降流化整误差

        public string variationErr { get; set; } //升降变差

        public string intVariationErr { get; set; } //升降化整变差

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表交流电压试验结论
    /// <summary>

    public class vVoltMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string testVoltValue { get; set; } //测试耐压值

        public string voltObj { get; set; } //耐压对象

        public string testDate { get; set; } //试验日期

        public string testTime { get; set; } //测试时长

        public string leakCurrentLimit { get; set; } //漏电流阀值(毫安)

        public string positionLeakLimit { get; set; } //表位漏电流阀值(毫安)

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表最大需量功能
    /// <summary>
    public class vMaxDemandMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string curPhaseCode { get; set; } //电流相别

        public string loadCurrent { get; set; } //电流负载

        public string bothWayPowerFlag { get; set; } //功率方向

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表载波通信性能试验结论
    /// <summary>
    public class vWaveMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表最大需量清零
    /// <summary>

    public class vResetDemandMeterConc : CommonDatas_DETedTestData
    {

      
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string resetDemand { get; set; } //清零后需量

        public string demand { get; set; } //清零前需量

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表走字试验结论
    /// <summary>
    public class vRunningMeterConc : CommonDatas_DETedTestData
    {

        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string bothWayPowerFlag { get; set; } //功率方向

        public string startReading { get; set; } //起始度数

        public string endReading { get; set; } //结束度数

        public string irReading { get; set; } //走字度数

        public string rate { get; set; } //费率

        public string tsWay { get; set; } //实现方式

        public string feeStartTime { get; set; } //起始费率时间

        public string irPules { get; set; } //走字脉冲数

        public string registerRead { get; set; } //标准表度数

        public string arTsReadingErr { get; set; } //电量误差

        public string errUp { get; set; } //误差上限

        public string errDown { get; set; } //误差下限

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表费控拉闸试验结论
    /// <summary>

    public class vSwitchoffMeterConc : CommonDatas_DETedTestData
    {


        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表费控合闸试验结论
    /// <summary>

    public class vSwitchonMeterConc : CommonDatas_DETedTestData
    {

       

        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表广播校时试验
    /// <summary>

    public class vTimingMeterConc : CommonDatas_DETedTestData
    {   
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 采标信息
    /// <summary>
    public class vCpInfo : CommonDatas_DETedTestData
    {       
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string cpInfo { get; set; } //彩标标识

        public string cpPosition { get; set; } //采标位置

        public string passFlag { get; set; } //合格标志

        public string detectDate { get; set; } //检定时间

        public string detecterNo { get; set; } //检定人员

        public string recheckerNo { get; set; } //核验人员

    }

    /// <summary>
    /// 剩余电能量递减准确度表
    /// <summary>
    public class vSurplusMeterConc : CommonDatas_DETedTestData
    {    
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string sumIncrease { get; set; } //Eo累计用电能量增加数

        public string surplusReduce { get; set; } //ΔE 剩余电能量减少数

        public string surplusValue { get; set; } //|E0-ΔE|误差值

        public string checkConc { get; set; } //检定结论

    }

    /// <summary>
    /// 控制功能结论表
    /// <summary>
    public class vControlMeterConc : CommonDatas_DETedTestData
    { 
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string settingBreakValue1 { get; set; } //预置断电金额1

        public string settingWarnValue1 { get; set; } //预置报警金额1

        public string settingWarnValue2 { get; set; } //预置报警金额2

        public string realBreakValue1 { get; set; } //实际断电金额1

        public string realWarnValue1 { get; set; } //实际报警金额1

        public string realWarnValue2 { get; set; } //实际报警金额2

        public string veriConc { get; set; } //检定结论

    }

    /// <summary>
    /// 电能表清零
    /// <summary>

    public class vResetEqMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string eq { get; set; } //清零前电量

        public string resetEq { get; set; } //清零后电量

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 电能表标准偏差
    /// <summary>
    public class vDeviationMeterConc : CommonDatas_DETedTestData
    {

   
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string bothWayPowerFlag { get; set; } //功率方向

        public string curPhaseCode { get; set; } //电流相别

        public string loadCurrent { get; set; } //负载电流

        public string loadVoltage { get; set; } //电压比值

        public string trialFreq { get; set; } //频率

        public string pf { get; set; } //功率因数

        public string detectCircle { get; set; } //检验脉冲数

        public string simpling { get; set; } //采样次数

        public string allError { get; set; } //实际误差

        public string aveErr { get; set; } //误差平均值

        public string intConvertErr { get; set; } //误差化整值

        public string checkConc { get; set; } //结论

    }

    /// <summary>
    /// 费率时段电能示值误差
    /// <summary>
    public class vTimeErrorMeterConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string bothWayPowerFlag { get; set; } //功率方向

        public string loadCurrent { get; set; } //电流负载

        public string pf { get; set; } //功率因数

        public string rate { get; set; } //费率

        public string controlWay { get; set; } //控制方式

        public string feeStartTime { get; set; } //费率起始时间

        public string irTime { get; set; } //走字时间(分钟)

        public string irReading { get; set; } //走字度数(千瓦时)

        public string errUp { get; set; } //误差上限（s/d）

        public string errDown { get; set; } //误差下限（s/d）

        public string voltage { get; set; } //电压(1为1%)

        public string valueConcCode { get; set; } //示值误差

        public string qualifiedPules { get; set; } //合格脉冲数

        public string totalReadingErr { get; set; } //总分电量值差(kWh)

        public string concCode { get; set; } //结论

    }

    /// <summary>
    /// 远程控制试验结论
    /// <summary>
    public class vRemoteCtrlTestConc : CommonDatas_DETedTestData
    {
        public string sn { get; set; } //小项参数顺序号

        public string veriPointSn { get; set; } //检定点的序号

        public string validFlag { get; set; } //有效标志

        public string checkConc { get; set; } //检定结论

        public string testTime { get; set; } //检测时间

        public string volt { get; set; } //电压

        public string cur { get; set; } //电流

        public string powerFactor { get; set; } //功率因数

        public string trialProj { get; set; } //试验项目

        public string testMethod { get; set; } //测试方法
        public string tripDelayTime { get; set; } //跳闸延时时间
    }
}
