using System;
using System.Runtime.InteropServices;
using CLOU.Enum;
using CLOU.Struct;
using CLOU;
using CLOU.SocketModule.Packet;
using E_CL188M;
using System.Collections.Generic;

namespace ZH
{
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
    ComVisible(true)]
    public interface IClass_Interface
    {
        /// <summary>
        /// 初始化2018端口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTme"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <param name="IP"></param>
        /// <param name="RemotePort"></param>
        /// <param name="LocalStartPort"></param>
        /// <returns></returns>
        [DispId(1)]
        int InitSetting(int ComNumber, int MaxWaitTme, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort);
        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <returns></returns>
        [DispId(2)]
        int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte);
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(3)]
        int Connect(byte Id, out string[] FrameAry);
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(4)]
        int DisConnect(out string[] FrameAry);
        /// <summary>
        /// 设置发送标志
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        [DispId(5)]
        int SetSendFlag(bool Flag);
        /// <summary>
        /// 设置脉冲通道和类型
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="pram1"></param>
        /// <param name="pram2"></param>
        /// <param name="pram3"></param>
        /// <param name="pram4"></param>
        /// <param name="pram5"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(6)]
        int SetPulseChannelAndType(int Id, int pram1, int pram2, int pram3, int pram4, int pram5,
out string[] FrameAry);
        /// <summary>
        /// 升级登录指令
        /// </summary>
        /// <returns></returns>
        [DispId(7)]
        int UpdateLogin(int id, out string[] FrameAry);
        /// <summary>
        /// 重启误差板
        /// </summary>
        /// <param name="id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(8)]
        int ReBoot(int id, out string[] FrameAry);
        /// <summary>
        /// 升级数据
        /// </summary>
        /// <param name="DataSerial">数据序号</param>
        /// <param name="bytesData">要升级的数据</param>
        /// <param name="FrameAry">合成升级数据的报文</param>
        /// <returns></returns>
        [DispId(9)]
        int UpdateFirmware(int id, UInt16 DataSerial, byte[] bytesData, out string[] FrameAry);
        /// <summary>
        /// 升级登录2
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(10)]
        int UpdateLogin2(int id, out string[] FrameAry);
        /// <summary>
        /// 重启设备2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(11)]
        int ReBoot2(int id, out string[] FrameAry);
        /// <summary>
        /// 升级数据2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="DataSerial">数据序号</param>
        /// <param name="bytesData">要升级的数据</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(12)]
        int UpdateFirmware2(int id, UInt16 DataSerial, byte[] bytesData, out string[] FrameAry);
        /// <summary>
        /// 读取版本2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Version">版本号</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(13)]
        int ReadVersion2(int id, out string Version, out string[] FrameAry);
        /// <summary>
        /// 读取功耗参数
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="AU_Ia_or_I">三相A相电压回路电流值|单相电压回路电流值</param>
        /// <param name="BU_Ib_or_L1_U">三相B相电压回路电流值|电流1回路电压值</param>
        /// <param name="CU_Ic_or_L2_U">三相C相电压回路电流值|电流2回路电压值</param>
        /// <param name="AI_Ua">三相A相电流回路电压值|单相保留</param>
        /// <param name="BI_Ub">三相B相电流回路电压值|单相保留</param>
        /// <param name="CI_Uc">三相C相电流回路电压值|单相保留</param>
        /// <param name="AU_Phia_or_Phi">三相A相电压回路相位角|单相电压回路相位角</param>
        /// <param name="BU_Phib">三相B相电压回路相位角|单相保留</param>
        /// <param name="CU_Phic">三相C相电压回路相位角|单相保留</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(14)]
        int ReadPowerParams(int id, out float AU_Ia_or_I, out float BU_Ib_or_L1_U, out float CU_Ic_or_L2_U,
                                                 out float AI_Ua, out float BI_Ub, out float CI_Uc,
                                                 out float AU_Phia_or_Phi, out float BU_Phib, out float CU_Phic,
                                                 out string[] FrameAry);
        /// <summary>
        /// 启动要信输出
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="YXTestNo">遥信路数</param>
        /// <param name="YxTestType">遥信输出方式</param>
        /// <param name="YxTestPulseNum">脉冲个数</param>
        /// <param name="yxTestPulseOutHz">脉冲输出频率</param>
        /// <param name="yxTestOutmultiple">输出占空比的1000000倍（4Bytes</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(15)]
        int StartRemoteSignals(int id, int YXTestNo, int YxTestType,
                                                 int YxTestPulseNum, float yxTestPulseOutHz, float yxTestOutmultiple,
                                                out string[] FrameAry);
        /// <summary>
        /// 启动直流模拟量输出
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="Current">电流</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(16)]
        int StartDCAnalog(int id, int Current, out string[] FrameAry);
        /// <summary>
        /// 停止遥信、或直流模拟量输出
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="checkType">0：遥信实验1：直流模拟量采集实验</param>
        /// <param name="chennNo">表示第几路遥信,该字节只有当实验类型是遥信实验时候为有效值,其他值：无效参数</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(17)]
        int StopOutPut(int id, int checkType, int chennNo, out string[] FrameAry);
        /// <summary>
        /// 读取遥控信号
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="YkCount">遥控个数</param>
        /// <param name="PusleCount">脉冲个数</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(18)]
        int ReadSignals(int id, int YkCount, out int PusleCount, out string[] FrameAry);
        /// <summary>
        /// 设置跳闸继电器类型
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="SwitchCommand">0 停止跳闸|1启动跳闸试验</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(19)]
        int SetTripRelayType(int id, byte SwitchCommand, out string[] FrameAry);
        /// <summary>
        /// 负控继电器控制命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="bearRelayType">负控继电器状态</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(20)]
        int SetSecondaryRelayStatus(int id, byte bearRelayStatus, out string[] FrameAry);
        /// <summary>
        /// 直流输出校准
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(21)]
        int DirectCurrentOutPutCorrect(int id, out string[] FrameAry);
        /// <summary>
        /// 直流输出实际电流校准
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="uiData">电流值</param>
        /// <param name="correctParams">输出校准参数</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(22)]
        int DirectCurrentOutPutRealityValueCorrect(int id, UInt16 uiData, out short correctParams, out string[] FrameAry);
        /// <summary>
        /// 设置互斥电机和速度
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="UpOrDown">电机向上或向下</param>
        /// <param name="Option">递增或者递减</param>
        /// <param name="CalTime">延时时间</param>
        /// <returns></returns>
        [DispId(23)]
        int SetMotorMutexAndSpeed(int id, int UpOrDown, int Option, int CalTime, out string[] FrameAry);

        /// <summary>
        /// 读取电机速度
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="UpDelaytime">上限为延时时间</param>
        /// <param name="DownDelaytime">下限位延时时间</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(24)]
        int ReadMotorSpeed(int id, out int UpDelaytime, out int DownDelaytime, out string[] FrameAry);
        /// <summary>
        /// 读取温度
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="readType">0=A,1=B,2=C</param>
        /// <param name="Temperature">温度</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        [DispId(25)]
        int ReadTConnector(int id, int readType, out string[] Temperature, out string[] FrameAry);
        /// <summary>
        /// 设置继电器类型
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="RelayID">继电器ID</param>
        /// <param name="ControlType">继电器类型0，断开继电器|1，闭合继电器</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(26)]
        int SetSwitchType(int id, byte RelayID, byte ControlType, out string[] FrameAry);
        /// <summary>
        /// 读取继电器类型
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="RelayID">继电器ID</param>
        /// <param name="ContrlType">继电器控制类型 0,断开继电器|1，闭合继电器</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(27)]
        int ReadRelayStatus(int id, byte RelayID, out byte ContrlType, out string[] FrameAry);
        /// <summary>
        /// 设置设备信息
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="MeterType">电能表类型</param>
        /// <param name="CTtype">CT类型</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(28)]
        int SetEquipInformation(int id, byte MeterType, byte CTtype, out string[] FrameAry);
        /// <summary>
        /// 读取设备信息
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="MeterType">电表类型</param>
        /// <param name="CTbyte">CT类型</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(29)]
        int ReadEquipInformation(int id, out int MeterIndex, out int MeterType, out int CTbyte, out string[] FrameAry);
        /// <summary>
        /// 读取脉冲通道和类型
        /// </summary>
        /// <param name="Id">误差板编号</param>
        /// <param name="pram1"></param>
        /// <param name="pram2"></param>
        /// <param name="pram3"></param>
        /// <param name="pram4"></param>
        /// <param name="pram5"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(30)]
        int ReadPulseChannelAndType(int id, out int pram1, out int pram2, out int pram3, out int pram4, out int pram5, out string[] FrameAry);
        /// <summary>
        /// 设置光电头状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="selecttype">选择类型</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        [DispId(31)]
        int SetSelectLightStatus(int id, byte selecttype, out string[] FrameAry);
        /// <summary>
        /// 设置双回路命令
        /// </summary>
        /// <param name="id">表位状态</param>
        /// <param name="iroad">电流回路</param>
        /// <param name="vroad">电压回路</param>
        /// <param name="FrameAry">出参报文</param>
        /// <returns></returns>
        [DispId(32)]
        int SetSelectCheckRoad(int id, byte iroad, byte vroad, out string[] FrameAry);
        /// <summary>
        /// 开始检定试验
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">检定类型</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(33)]
        int StartTest(int id, byte verifyType, out string[] FrameAry);
        /// <summary>
        /// 停止检定试验
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">检定类型</param>
        /// <param name="FrameAry">出参</param>
        /// <returns></returns>
        [DispId(34)]
        int StopTest(int id, byte verifyType, out string[] FrameAry);
        /// <summary>
        /// 设置表位电压电流隔离命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="IsolationStatus">隔离状态</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(35)]
        int SetBwVolCutIsolation(int id, int IsolationStatus, out string[] FrameAry);
        /// <summary>
        /// 读取表位隔离状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="IsolationStatus">隔离状态</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(36)]
        int ReadBwVolCutsolation(int id, out int IsolationStatus, out string[] FrameAry);
        /// <summary>
        /// 设置耐压继电器状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="highVoltageType">耐压继电器状态</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(37)]
        int SetACTVRelay(int id, byte highVoltageType, out string[] FrameAry);
        /// <summary>
        /// 读取耐压继电器状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="highVoltageType">输出耐压状态</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(38)]
        int ReadACTVRelay(int id, out byte highVoltageType, out string[] FrameAry);
        /// <summary>
        /// CT 档位控制器
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="CtType">CT类型</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(39)]
        int SetCTPosition(int id, byte CtType, out string[] FrameAry);
        /// <summary>
        /// 查询误差板当前误差及其状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="wcbVerifyType">检定类型</param>
        /// <param name="verificationType">输出误差板检定类型</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="ErrorNum">误差次数</param>
        /// <param name="wcData">误差值</param>
        /// <param name="statusType">状态</param>
        /// <param name="CurrentContour"></param>
        /// <param name="VoltageContour"></param>
        /// <param name="CommunicationType"></param>
        /// <param name="workType"></param>
        /// <param name="expandStatus"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(40)]
        int ReadCurrentData(int id, byte wcbVerifyType, out byte verificationType,
            out int MeterIndex, out int ErrorNum, out string wcData, out bool[] statusType, out byte CurrentContour,
            out byte VoltageContour, out byte CommunicationType, out bool[] workType, out byte expandStatus, out string[] FrameAry);
        /// <summary>
        /// 读取误差板最新10次误差及其状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">检定类型</param>
        /// <param name="verificationType"></param>
        /// <param name="MeterIndex"></param>
        /// <param name="Errorcount"></param>
        /// <param name="wcDatas"></param>
        /// <param name="wcData"></param>
        /// <param name="statusType"></param>
        /// <param name="CurrentContour"></param>
        /// <param name="VoltageContour"></param>
        /// <param name="CommunicationType"></param>
        /// <param name="workType"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(41)]
        int ReadFirstTenTimesData(int id, byte wcbVerifyType, out byte verificationType,
            out int MeterIndex, out int ErrorNum, out string[] wcDatas, out string wcData, out bool[] statusType, out byte CurrentContour,
            out byte VoltageContour, out byte CommunicationType, out bool[] workType, out string[] FrameAry);
        /// <summary>
        /// 清除表位状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="statusType"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(42)]
        int ReSetStatus(int id, byte statusType, out string[] FrameAry);
        /// <summary>
        /// 读取软件版本号
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="strVersion">版本号</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(43)]
        int ReadVersion(int id, out int MeterIndex, out string strVersion, out string[] FrameAry);
        /// <summary>
        /// 读取打印信息
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        [DispId(44)]
        int ReadPrintInformation(int id, int modid, out byte dataSerial, out byte dataLen, out byte[] dataAry, out string[] FrameAry);
        /// <summary>
        /// 读取隔离继电器过载动作可靠性检测时间
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        [DispId(45)]
        int ReadIsolationrelayOverloadReliabilityTime(int id, out int meterIndex, out int detectionTime, out string[] FrameAry);
        /// <summary>
        /// 设置隔离继电器过载动作可靠性检测时间
        /// </summary>
        /// <param name="id">表位号</param>
        /// <param name="Time">时间</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(46)]
        int SetIsolationrelayOverloadReliabilityTime(int id, byte Time, out string[] FrameAry);

        /// <summary>
        /// 设置电能误差检定时脉冲参数
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterConst">标准表脉冲常数</param>
        /// <param name="stdPulseFreq">标准脉冲频率</param>
        /// <param name="stdMeterConstShorttime">标准脉冲缩放倍数</param>
        /// <param name="meterConst">被检表脉冲常数</param>
        /// <param name="circles">圈数</param>
        /// <param name="meterConstZooms">被检脉冲缩放倍数</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(47)]
        int SetEnergePulseParams(int id, int stdMeterConst, int stdPulseFreq, int stdMeterConstShorttime, int meterConst, int circles, int meterConstZooms, out string[] FrameAry);
        /// <summary>
        ///  读取电能差脉冲参数
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterConst">标准表常数</param>
        /// <param name="stdMeterConstShorttime">标准表脉冲常数缩放倍数</param>
        /// <param name="meterConst">被检表脉冲常数</param>
        /// <param name="circles">校验圈数</param>
        /// <param name="meterConstZooms">被检脉冲常数缩放倍数</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(48)]
        int ReadEnergePulseParams(int id, out int stdMeterConst, out int stdMeterConstShorttime, out int meterConst, out int circles, out int meterConstZooms, out string[] FrameAry);
        /// <summary>
        /// 设置日计时检定时钟频率及需量周期检定时间
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterTimeFreq">标准时钟频率</param>
        /// <param name="meterTimeFreq">需量时间周期</param>
        /// <param name="meterPulseNum">被检脉冲个数</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        [DispId(49)]
        int SetClockFrequency(int id, int stdMeterTimeFreq, int meterTimeFreq, int meterPulseNum, out string[] FrameAry);
        /// <summary>
        /// 读取日计时检定时钟频率及需量周期检定时间
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterTimeFreq">标准时钟频率</param>
        /// <param name="meterTimeFreq">被检表时钟频率</param>
        /// <param name="meterPulseNum">被检表脉冲数</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        [DispId(50)]
        int ReadClockFrequency(int id, out int stdMeterTimeFreq, out int meterTimeFreq, out int meterPulseNum, out string[] FrameAry);
        /// <summary>
        /// 设置耐压试验漏电流阀值
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="iCurrentLimit">电流阀值</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        [DispId(51)]
        int SetACTVLeakCurrentThresholdValue(int id, int iCurrentLimit, out string[] FrameAry);
        /// <summary>
        /// 读取漏电流阀值
        /// </summary>
        /// <param name="id">表位号</param>
        /// <param name="iCurrentLimit">漏电流阀值</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        [DispId(52)]
        int ReadACTVLeakCurrentThresholdValue(int id, out int iCurrentLimit, out string[] FrameAry);
        /// <summary>
        /// 设置电机模式
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="electricalMachineMode">电机控制方式</param>
        /// <returns></returns>
        [DispId(53)]
        int ControlMotor(int id, byte electricalMachineMode, out string[] FrameAry);
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName">解析报文的方法</param>
        /// <param name="ReFrameAry">报文帧</param>
        /// <param name="ReAry">返回解析数据</param>
        /// <returns></returns>
        [DispId(54)]
        int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry);

    }

    public class E_CL188M : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 控制端口
        /// </summary>
        private StPortInfo m_Port = null;

        DriverBase driverBase = null;
        /// <summary>
        /// 发送标志
        /// </summary>
        private bool SendFlag = true;

        public E_CL188M()
        {
            m_Port = new StPortInfo();
            driverBase = new DriverBase();
        }

        #region IClass_Interface 成员
        /// <summary>
        /// 初始化2018端口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <param name="IP"></param>
        /// <param name="RemotePort"></param>
        /// <param name="LocalStartPort"></param>
        /// <returns></returns>
        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort)
        {
            m_Port = new StPortInfo();
            m_Port.m_Exist = 1;
            m_Port.m_IP = IP;
            m_Port.m_Port = ComNumber;
            m_Port.m_Port_IsUDPorCom = true;
            m_Port.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_Port.m_Port_Setting, m_Port.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return -1;
            }
            return 0;
        }
        /// <summary>
        /// 初始化COM口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_Port.m_Exist = 1;
            m_Port.m_IP = "";
            m_Port.m_Port = ComNumber;
            m_Port.m_Port_IsUDPorCom = false;
            m_Port.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Connect(byte Id, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL188M_RequestLinkPacket rc2 = new CL188M_RequestLinkPacket();
            CL188M_RequestLinkReplayPacket recv2 = new CL188M_RequestLinkReplayPacket();
            try
            {
                rc2.Pos = Id;
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                int intRst = 1;
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc2, recv2))
                    {
                        intRst = recv2.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
                return intRst;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int DisConnect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            FrameAry[0] = "";
            return 0;
        }
        /// <summary>
        /// 控制表位继电器
        /// </summary>
        /// <param name="contrnlType"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ContnrlBw(int contrnlType, byte bwNum, byte data, byte cmd, out string[] FrameAry)
        {
            int to_contrnlType = contrnlType;
            if (contrnlType == 2) to_contrnlType = 0;
            int to_bwNum = bwNum;
            if (bwNum == 255) to_bwNum = 0;
            return SetBwVolCutIsolation(to_bwNum, to_contrnlType, out FrameAry);
        }
        /// <summary>
        /// 电机控制
        /// </summary>
        /// <param name="contrnlType">00下行 01上行</param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ElectricmachineryContrnl(int contrnlType, byte bwNum, out string[] FrameAry)
        {
            return ControlMotor(bwNum, (byte)contrnlType, out FrameAry);
        }
        private int stdMeterConst = 1;
        private int stdPulseFreq = 1;
        private int stdMeterConstShorttime = 1;
        private int meterConst = 1;
        private int circles = 1;
        private int meterConstZooms = 1;
        /// <summary>
        /// 设置标准常数
        /// </summary>
        /// <param name="enlarge">o电能类型（2个标准： 00-标准电能误差相关； 01-标准时钟日计时需量）</param>
        /// <param name="Constant">标准常数</param>
        /// <param name="sdbs">放大倍数-2缩小100倍</param>
        /// <param name="bwNum">表位FF</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetStandardConstantQs(int enlarge, int Constant, int sdbs, byte bwNum, out string[] FrameAry)
        {
            stdMeterConst = (int)(Constant * Math.Pow(10, sdbs * -1));
            stdMeterConstShorttime = 1;
            int to_bwNum = bwNum;
            if (bwNum == 255) to_bwNum = 0;
            if (enlarge == 1)
                return SetClockFrequency(to_bwNum, stdMeterConst, meterConst, circles, out FrameAry);
            else
                return SetEnergePulseParams(to_bwNum, stdMeterConst, stdPulseFreq, stdMeterConstShorttime, meterConst, circles, meterConstZooms, out FrameAry);
        }
        /// <summary>
        /// 设置被检常数
        /// </summary>
        /// <param name="enlarge">（6组被检 ：00-有功（含正向、反向，以下同）；01-无功（正向、反向，以下同）； 04-日计时；05-需量）</param>
        /// <param name="Constant"></param>
        /// <param name="fads"></param>
        /// <param name="qs"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetBJConstantQs(int enlarge, int Constant, int fads, int qs, byte bwNum, out string[] FrameAry)
        {
            int to_enlarge = enlarge;
            if (enlarge == 1) to_enlarge = 2;
            int to_muti = 0;
            if (enlarge == 4) to_muti = 1;
            else if (enlarge == 5) to_muti = 2;
            int to_type = 0;
            if (enlarge == 4) to_type = 2;
            else if (enlarge == 5) to_type = 1;
            int to_bwNum = bwNum;
            if (bwNum == 255) to_bwNum = 0;
            int rst = SetPulseChannelAndType(to_bwNum, to_enlarge, 0, 0, to_muti, to_type, out FrameAry);
            meterConst = (int)(Constant * Math.Pow(10, fads * -1));
            stdPulseFreq = 1;//TODO:
            circles = qs;
            meterConstZooms = 1;
            if (enlarge == 4)
                return SetClockFrequency(to_bwNum, stdMeterConst, meterConst, circles, out FrameAry);
            else
                return SetEnergePulseParams(to_bwNum, stdMeterConst, stdPulseFreq, stdMeterConstShorttime, meterConst, circles, meterConstZooms, out FrameAry);
        }
        /// <summary>
        /// 读取表位电压短路和电流开路标志 
        /// </summary>
        /// <param name="reg">命令03读取表位电压短路和电流开路标志</param>
        /// <param name="bwNum">表位</param>
        /// <param name="OutResult">返回状态0:电压短路标志，00表示没短路，01表示短路；DATA1-电流开路标志，1:00表示没开路，01表示开路。</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Read_Fault(byte reg, byte bwNum, out byte[] OutResult, out string[] FrameAry)
        {
            OutResult = new byte[0];
            int rst = ReadCurrentData(bwNum, 0, out byte verificationType, out int MeterIndex, out int ErrorNum, out string wcData, out bool[] statusType, out byte CurrentContour, out byte VoltageContour, out byte CommunicationType, out bool[] workType, out byte expandStatus, out FrameAry);
            if (rst == 0)
            {
                OutResult = new byte[] { VoltageContour, CurrentContour };
            }
            return rst;
        }

        /// <summary>
        /// 读取计算值
        /// </summary>
        /// <param name="readType">00--有功；01--无功；04--日计时05--需量06--有功脉冲计数07--无功脉冲计数
        /// 08-有功启动实验脉冲时长（必须先设置有功误差参数，因为会同时做启动电流误差实验）V3.1新增 09-无功启动实验脉冲时长（必须先设置无功误差参数，因为会同时做启动电流误差实验） V3.1新增
        ///</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadData(int readType, byte bwNum, out string[] OutWcData, out int OutBwNul, out int OutGroup, out int OutWcNul, out string[] FrameAry)
        {
            OutWcData = new string[0];
            OutBwNul = 0;
            OutGroup = 0;
            OutWcNul = 0;

            byte wcbVerifyType = (byte)readType;//TODO:convert
            if (readType == 4) wcbVerifyType = 2;
            else if (readType == 5) wcbVerifyType = 1;
            else if (readType == 6) wcbVerifyType = 3;
            else if (readType == 7) wcbVerifyType = 3;
            //int rst = ReadCurrentData(bwNum, wcbVerifyType, out byte verificationType, out int MeterIndex, out int ErrorNum, out string wcData, out bool[] statusType, out byte CurrentContour, out byte VoltageContour, out byte CommunicationType, out bool[] workType, out byte expandStatus, out FrameAry);
            int rst = ReadFirstTenTimesData(bwNum, wcbVerifyType, out byte verificationType, out int MeterIndex, out int ErrorNum, out string[] wcDatas, out string wcData, out bool[] statusType, out byte CurrentContour, out byte VoltageContour, out byte CommunicationType, out bool[] workType, out FrameAry);
            if (rst == 0)
            {
                OutWcData = new string[5];
                //OutWcData[0] = wcData;
                List<string> tmp = new List<string>();
                for (int i = 0; i < 5; i++)
                {
                    int n = wcDatas.Length - 1 - i;
                    if (wcDatas.Length > n)
                    {
                        if (wcDatas[n] != "0")
                            tmp.Insert(0, wcDatas[n]);
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    if (tmp.Count > i)
                        OutWcData[i] = tmp[i];
                }
                OutBwNul = MeterIndex;
                OutGroup = verificationType;
                OutWcNul = ErrorNum;
            }
            return rst;
        }
        /// <summary>
        /// 启动计算
        /// </summary>
        /// <param name="start"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Start(int start, byte bwNum, out string[] FrameAry)
        {
            int to_bwNum = bwNum;
            if (bwNum == 255) to_bwNum = 0;
            byte verifyType = (byte)start;
            if (start == 1) verifyType = 0;
            else if (start == 4) verifyType = 2;
            else if (start == 5) verifyType = 1;
            else if (start == 6) verifyType = 3;
            else if (start == 7) verifyType = 3;
            return StartTest(to_bwNum, verifyType, out FrameAry);
        }
        /// <summary>
        /// 停止计算
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Stop(int stop, byte bwNum, out string[] FrameAry)
        {
            int to_bwNum = bwNum;
            if (bwNum == 255) to_bwNum = 0;
            byte verifyType = (byte)stop;
            if (stop == 1) verifyType = 0;
            else if (stop == 4) verifyType = 2;
            else if (stop == 5) verifyType = 1;
            else if (stop == 6) verifyType = 3;
            else if (stop == 7) verifyType = 3;
            return StopTest(to_bwNum, verifyType, out FrameAry);
        }
        /// <summary>
        /// 对标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Benchmarking(int index, byte bwNum, out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }
        /// <summary>
        /// 取消队标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int RevBenchmarking(int index, byte bwNum, out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }
        /// <summary>
        /// 查询对标状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SelectBenchmarking(int index, byte bwNum, out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }

        /// <summary>
        /// 设置标志位
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public int SetSendFlag(bool Flag)
        {
            SendFlag = Flag;
            return SendFlag == Flag ? 0 : 1;
        }
        /// <summary>
        /// 设置脉冲通道和类型
        /// </summary>
        /// <param name="Id">误差板编号</param>
        /// <param name="pram1">电能误差通道号0正有|1反有|2正无|3反无|4需量|5时钟</param>
        /// <param name="pram2">光电头选择位0脉冲盒|1光电头</param>
        /// <param name="pram3">共因0|共阳1</param>
        /// <param name="pram4">多功能误差通道号0电能|1日计时|2需量</param>
        /// <param name="pram5">检定类型0电能|1需量|2日计时|3脉冲|4对标|5预付费|6耐压|7多功能脉冲计数</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetPulseChannelAndType(int Id, int pram1, int pram2, int pram3, int pram4, int pram5, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL188M_RequestSelectPulseChannelAndCheckTypePacket rc2 = new CL188M_RequestSelectPulseChannelAndCheckTypePacket();
            CL188M_RequestSelectPulseChannelAndCheckTypeReplayPacket recv2 = new CL188M_RequestSelectPulseChannelAndCheckTypeReplayPacket();
            try
            {
                rc2.Pos = Id;
                rc2.SetPara((Cus_MeterWcChannelNo)pram1, (Cus_PulseType)pram2, (Cus_GyGyType)pram3, (Cus_DgnWcChannelNo)pram4, (Cus_CheckType)pram5, 0);
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                int intRst = 1;
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc2, recv2))
                    {
                        intRst = recv2.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
                return intRst;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// 远程登录
        /// </summary>
        /// <param name="FrameAry">合成报文</param>
        /// <returns></returns>
        public int UpdateLogin(int id, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL188M_RequestUpdateLoginPacket rc = new CL188M_RequestUpdateLoginPacket();
            CL188M_RequestUpdateLoginReplayPacket recv = new CL188M_RequestUpdateLoginReplayPacket();
            int intRst = 1;
            try
            {
                rc.Pos = id;
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;

        }
        /// <summary>
        /// 重启误差板
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReBoot(int id, out string[] FrameAry)
        {
            FrameAry = new string[1];
            int intRst = 1;
            CL188M_RequestReBootPacket rc = new CL188M_RequestReBootPacket();
            CL188M_RequestReBootReplayPacket recv = new CL188M_RequestReBootReplayPacket();
            rc.Pos = id;

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());

                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 升级数据
        /// </summary>
        /// <param name="DataSerial">数据序号</param>
        /// <param name="bytesData">要升级的数据</param>
        /// <param name="FrameAry">要反会的组帧报文</param>
        /// <returns></returns>
        public int UpdateFirmware(int id, UInt16 DataSerial, byte[] bytesData, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL188M_RequestUpdateFirmwarePacket rc = new CL188M_RequestUpdateFirmwarePacket(DataSerial, bytesData);
            CL188M_RequestUpdateFirmwareReplayPacket recv = new CL188M_RequestUpdateFirmwareReplayPacket();
            rc.Pos = id;
            int intRst = 1;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;

        }

        /// <summary>
        /// 远程登录2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int UpdateLogin2(int id, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL188M_RequestUpdateLogin2Packet rc = new CL188M_RequestUpdateLogin2Packet();
            CL188M_RequestUpdateLogin2ReplayPacket recv = new CL188M_RequestUpdateLogin2ReplayPacket();
            int intRst = 1;
            try
            {
                rc.Pos = id;
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 重启误差板2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReBoot2(int id, out string[] FrameAry)
        {
            FrameAry = new string[1];
            int intRst = 1;
            CL188M_RequestReBoot2Packet rc = new CL188M_RequestReBoot2Packet();
            CL188M_RequestReBoot2ReplayPacket recv = new CL188M_RequestReBoot2ReplayPacket();
            rc.Pos = id;

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());

                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 升级数据2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="DataSerial"></param>
        /// <param name="bytesData"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int UpdateFirmware2(int id, ushort DataSerial, byte[] bytesData, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL188M_RequestUpdateFirm2warePacket rc = new CL188M_RequestUpdateFirm2warePacket(DataSerial, bytesData);
            CL188M_RequestUpdateFirmware2ReplayPacket recv = new CL188M_RequestUpdateFirmware2ReplayPacket();
            rc.Pos = id;
            int intRst = 1;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 读取版本号
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Version"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadVersion2(int id, out string Version, out string[] FrameAry)
        {
            FrameAry = new string[1];
            Version = string.Empty;
            int intRst = 1;

            CL1888M_RequestReadVersion2Packet rc = new CL1888M_RequestReadVersion2Packet();
            CL1888M_RequestReadVersion2ReplayPacket recv = new CL1888M_RequestReadVersion2ReplayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        Version = recv.strVersion;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 读取功耗
        /// </summary>
        /// <param name="id"></param>
        /// <param name="AU_Ia_or_I"></param>
        /// <param name="BU_Ib_or_L1_U"></param>
        /// <param name="CU_Ic_or_L2_U"></param>
        /// <param name="AI_Ua"></param>
        /// <param name="BI_Ub"></param>
        /// <param name="CI_Uc"></param>
        /// <param name="AU_Phia_or_Phi"></param>
        /// <param name="BU_Phib"></param>
        /// <param name="CU_Phic"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadPowerParams(int id, out float AU_Ia_or_I, out float BU_Ib_or_L1_U, out float CU_Ic_or_L2_U, out float AI_Ua, out float BI_Ub, out float CI_Uc, out float AU_Phia_or_Phi, out float BU_Phib, out float CU_Phic, out string[] FrameAry)
        {
            FrameAry = new string[1];
            int intRst = 1;
            AU_Ia_or_I = 0f;
            BU_Ib_or_L1_U = 0f;
            CU_Ic_or_L2_U = 0f;
            AI_Ua = 0f;
            BI_Ub = 0f;
            CI_Uc = 0f;
            AU_Phia_or_Phi = 0f;
            BU_Phib = 0f;
            CU_Phic = 0f;
            CL188M_RequestReadGHPramPacket rc = new CL188M_RequestReadGHPramPacket();
            CL188M_RequestReadBwGHPramReplyPacket recv = new CL188M_RequestReadBwGHPramReplyPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        AU_Ia_or_I = recv.AU_Ia_or_I;
                        BU_Ib_or_L1_U = recv.BU_Ib_or_L1_U;
                        CU_Ic_or_L2_U = recv.CU_Ic_or_L2_U;
                        AI_Ua = recv.AI_Ua;
                        BI_Ub = recv.BI_Ub;
                        CI_Uc = recv.CI_Uc;
                        AU_Phia_or_Phi = recv.AU_Phia_or_Phi;
                        BU_Phib = recv.BU_Phib;
                        CU_Phic = recv.CU_Phic;

                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {
                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 启动遥信输出命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="YXTestNo">要信路数</param>
        /// <param name="YxTestType">遥信输出方式</param>
        /// <param name="YxTestPulseNum">脉冲个数</param>
        /// <param name="yxTestPulseOutHz">脉冲输出频率</param>
        /// <param name="yxTestOutmultiple">输出占控比</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int StartRemoteSignals(int id, int YXTestNo, int YxTestType, int YxTestPulseNum, float yxTestPulseOutHz, float yxTestOutmultiple, out string[] FrameAry)
        {
            FrameAry = new string[1];
            int intRst = 1;
            CL188M_RequestStartYXOutPacket rc = new CL188M_RequestStartYXOutPacket();
            rc.SetPara(id, YXTestNo, YxTestType, YxTestPulseNum, yxTestPulseOutHz, yxTestOutmultiple);
            CL188M_RequestStartYXOutReplayPacket recv = new CL188M_RequestStartYXOutReplayPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;

        }
        /// <summary>
        /// 启动直流模拟量
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="Current">直流模拟量采集电流</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int StartDCAnalog(int id, int Current, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestStartZLMNTestFunctionPacket rc = new CL188M_RequestStartZLMNTestFunctionPacket();
            rc.SetParam(id, Current);
            CL188M_RequestStartZLMNTestFunctionReplayPacket recv = new CL188M_RequestStartZLMNTestFunctionReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 停止遥信试验、直流模拟量采集试验
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="checkType">试验类型</param>
        /// <param name="chennNo">遥信路数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int StopOutPut(int id, int checkType, int chennNo, out string[] FrameAry)
        {
            FrameAry = new string[1];
            int intRst = 1;
            CL188M_RequestStopPCYXZLTestFunctionPacket rc = new CL188M_RequestStopPCYXZLTestFunctionPacket();
            CL188M_RequestStopPCYXZLTestFunctionReplayPacket recv = new CL188M_RequestStopPCYXZLTestFunctionReplayPacket();

            rc.SetParam(id, checkType, chennNo);

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


            return intRst;
        }
        /// <summary>
        /// 读取遥控信号
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="YkCount">遥控路数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadSignals(int id, int YkCount, out int PusleCount, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            PusleCount = 0;
            CL188M_RequestYaoKongPacket rc = new CL188M_RequestYaoKongPacket();
            CL188M_RequestYaoKongReplayPacket recv = new CL188M_RequestYaoKongReplayPacket();
            rc.SetPara(YkCount);
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        PusleCount = recv.PusleCount;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 跳闸选择命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="SwitchCommand">跳闸方式</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetTripRelayType(int id, byte SwitchCommand, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestChoiceSwitchPacket rc = new CL188M_RequestChoiceSwitchPacket(SwitchCommand);
            rc.Pos = id;
            CL188M_RequestChoiceSwitchReplayPacket recv = new CL188M_RequestChoiceSwitchReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 负控继电器控制命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="bearRelayStatus">负控继电器状态</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetSecondaryRelayStatus(int id, byte bearRelayStatus, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];

            CL188M_RequestFuKJDQPacket rc = new CL188M_RequestFuKJDQPacket();
            CL188M_RequestFuKJDQReplayPacket recv = new CL188M_RequestFuKJDQReplayPacket();
            rc.FkTestStatus = bearRelayStatus;
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 直流输出校准命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int DirectCurrentOutPutCorrect(int id, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];

            CL188M_RequestDirectCurrentOutputPacket rc = new CL188M_RequestDirectCurrentOutputPacket();
            CL188M_RequestDirectCurrentOutputReplayPacket recv = new CL188M_RequestDirectCurrentOutputReplayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 直流输出校准传递实际值
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="uiData">直流模拟量实际电流值</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int DirectCurrentOutPutRealityValueCorrect(int id, ushort uiData, out short correctParams, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            correctParams = 0;
            CL188M_RequestDirectCurrentOutputCalibrationPacket rc = new CL188M_RequestDirectCurrentOutputCalibrationPacket(uiData);
            rc.Pos = id;
            CL188M_RequestDirectCurrentOutputCalibrationReplayPacket recv = new CL188M_RequestDirectCurrentOutputCalibrationReplayPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        correctParams = recv.correctParams;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 压表电机检测就位延时调试命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="UpOrDown">0上限位|1下限位</param>
        /// <param name="Option">0递增延时时间|1递减延时时间</param>
        /// <param name="CalTime">需要递增或递减的延时时间(ms)</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetMotorMutexAndSpeed(int id, int UpOrDown, int Option, int CalTime, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_SetElectromotorTimePacket rc = new CL188M_SetElectromotorTimePacket(id, UpOrDown, Option, CalTime);
            CL188M_SetElectromotorTimePacketReplayPacket recv = new CL188M_SetElectromotorTimePacketReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 压表电机检测就位延时时间
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="UpDelaytime">上限位延时时间</param>
        /// <param name="DownDelaytime">下限位延时时间</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        public int ReadMotorSpeed(int id, out int UpDelaytime, out int DownDelaytime, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            UpDelaytime = 0;
            DownDelaytime = 0;
            CL188L_ReadElectromotorTimePacket rc = new CL188L_ReadElectromotorTimePacket();
            rc.Pos = id;
            CL188L_ReadElectromotorTimePacketReplayPacket recv = new CL188L_ReadElectromotorTimePacketReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        UpDelaytime = recv.UpDelayTime;
                        DownDelaytime = recv.DownDelayTime;

                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 读取电流柱温度
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="readType">0=A,1=B,2=C</param>
        /// <param name="Temperature">温度</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        public int ReadTConnector(int id, int readType, out string[] Temperature, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            Temperature = new string[0];
            CL188L_RequestReadBwTemperaturePacket rc = new CL188L_RequestReadBwTemperaturePacket();
            CL188L_RequestReadBwTemperatureReplyPacket recv = new CL188L_RequestReadBwTemperatureReplyPacket();
            rc.Pos = id;
            rc.m_intReadType = readType;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        Temperature = recv.m_strTemperature;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        ///  继电器控制指令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="RelayID">继电器ID</param>
        /// <param name="ControlType">0断开继电器|1闭合继电器</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        public int SetSwitchType(int id, byte RelayID, byte ControlType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSetSwitchPacket rc = new CL188M_RequestSetSwitchPacket(RelayID, ControlType);
            CL188M_RequestSetSwitchReplayPacket recv = new CL188M_RequestSetSwitchReplayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }

        /// <summary>
        /// 查询继电器状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="RelayID">继电器ID</param>
        /// <param name="ContrlType">0断开|1闭合</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadRelayStatus(int id, byte RelayID, out byte ContrlType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            ContrlType = 13;
            CL188M_RequestQuerySwitchStatusPacket rc = new CL188M_RequestQuerySwitchStatusPacket(RelayID);
            rc.Pos = id;
            CL188M_RequestQuerySwitchStatusReplayPacket recv = new CL188M_RequestQuerySwitchStatusReplayPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        ContrlType = recv.SwitchStatus;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 设置检定设备信息
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="MeterType">电能表类型</param>
        /// <param name="CTtype">CT类型</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetEquipInformation(int id, byte MeterType, byte CTtype, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSetEquipInformationPacket rc = new CL188M_RequestSetEquipInformationPacket(MeterType, CTtype);
            rc.Pos = id;
            CL188M_RequestSetEquipInformationReplayPacket recv = new CL188M_RequestSetEquipInformationReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 读取设备信息
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="MeterType">电能表类型</param>
        /// <param name="CTbyte">CT类型</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadEquipInformation(int id, out int MeterIndex, out int MeterType, out int CTbyte, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            MeterIndex = 0;
            MeterType = 0;
            CTbyte = 0;
            CL188M_RequestReadEquipInformationPacket rc = new CL188M_RequestReadEquipInformationPacket();
            CL188M_RequestReadEquipInformationReplayPacket recv = new CL188M_RequestReadEquipInformationReplayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        MeterIndex = recv.MeterNumber;
                        MeterType = recv.MeterType;
                        CTbyte = recv.CtType;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 读取脉冲通道及检定类型
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="pram1">误差通道</param>
        /// <param name="pram2">脉冲类型</param>
        /// <param name="pram3">脉冲极性选择</param>
        /// <param name="pram4">多功能 0误差通道号,1为日计时脉冲、2为需量脉冲。</param>
        /// <param name="pram5">检定类型：0x00电能误差、0x01需量误差、0x02日计时误差、0x03走字计数、0x04对标、0x05预付费功能检定。0x06：耐压实验 0x07：多功能脉冲计数试验</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadPulseChannelAndType(int id, out int pram1, out int pram2, out int pram3, out int pram4, out int pram5, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            pram1 = -1;
            pram2 = -1;
            pram3 = -1;
            pram4 = -1;
            pram5 = -1;
            CL188M_RequestQueryPusleChannelAndCheckTypePacket rc = new CL188M_RequestQueryPusleChannelAndCheckTypePacket();
            CL188M_RequestQueryPusleChannelAndCheckTypeReplayPacket recv = new CL188M_RequestQueryPusleChannelAndCheckTypeReplayPacket();
            rc.Pos = id;

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        pram1 = (byte)recv.wcChannelNo;
                        pram2 = (byte)recv.pulseType;
                        pram3 = (byte)recv.GyGy;
                        pram4 = (byte)recv.dgnWcChannelNo;
                        pram5 = (byte)recv.checkType;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 设置光电头状态选择
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="selecttype">选择类型</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetSelectLightStatus(int id, byte selecttype, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSelectLightStatusPacket rc = new CL188M_RequestSelectLightStatusPacket();
            rc.SetPara(id, (Cus_LightSelect)selecttype);
            CL188M_RequestSelectLightStatusReplayPacket recv = new CL188M_RequestSelectLightStatusReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 设置双回路命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="iroad">电流回路路数</param>
        /// <param name="vroad">电压回路路数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetSelectCheckRoad(int id, byte iroad, byte vroad, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSelectCheckRoadPacket rc = new CL188M_RequestSelectCheckRoadPacket(id, (Cus_BothIRoadType)iroad, (Cus_BothVRoadType)vroad);
            CL188M_RequestSelectCheckRoadReplayPacket recv = new CL188M_RequestSelectCheckRoadReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }


            return intRst;
        }
        /// <summary>
        /// 启动检定
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">检定类型</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int StartTest(int id, byte verifyType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestStartPCFunctionPacket rc = new CL188M_RequestStartPCFunctionPacket();
            CL188M_RequestStartPCFunctionReplayPacket recv = new CL188M_RequestStartPCFunctionReplayPacket();
            rc.SetPara(id, (Cus_CheckType)verifyType);
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 停止检定
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">检定类型</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int StopTest(int id, byte verifyType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestStopPCFunctionPacket rc = new CL188M_RequestStopPCFunctionPacket();
            rc.SetParam(id, (Cus_CheckType)verifyType);
            CL188M_RequestStopPCFunctionReplayPacket recv = new CL188M_RequestStopPCFunctionReplayPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;


        }
        /// <summary>
        /// 设置表位电压电流隔离
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="IsolationStatus">继电器状态</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetBwVolCutIsolation(int id, int IsolationStatus, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSeperateBwControlPacket rc = new CL188M_RequestSeperateBwControlPacket();
            rc.SetPara(id, IsolationStatus);
            CL188M_RequestSeperateBwControlReplayPacket recv = new CL188M_RequestSeperateBwControlReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 读取表位电压电流隔离状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="IsolationStatus">继电器状态</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadBwVolCutsolation(int id, out int IsolationStatus, out string[] FrameAry)
        {
            int intRst = 1;
            IsolationStatus = 0;
            FrameAry = new string[1];
            CL188M_RequestQuerySeperateBwControlPacket rc = new CL188M_RequestQuerySeperateBwControlPacket();
            CL188M_RequestQuerySeperateBwControlReplayPacket recv = new CL188M_RequestQuerySeperateBwControlReplayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        IsolationStatus = recv.Seperate;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 设置耐压继电器指令控制状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="highVoltageType">耐压继电器指令控制状态</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetACTVRelay(int id, byte highVoltageType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_SendCMDToInsulationRelays rc = new CL188M_SendCMDToInsulationRelays();
            CL188M_SendCMDToInsulationRelaysReplayPacket recv = new CL188M_SendCMDToInsulationRelaysReplayPacket();

            rc.SetPara(id, highVoltageType);

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 查询耐压继电器状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="highVoltageType">耐压继电器指令控制状态</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadACTVRelay(int id, out byte highVoltageType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            highVoltageType = 0;
            CL188M_RequestQueryCMDToInsulationRelaysPacket rc = new CL188M_RequestQueryCMDToInsulationRelaysPacket();
            CL188M_RequestQueryCMDToInsulationRelaysRePlayPacket recv = new CL188M_RequestQueryCMDToInsulationRelaysRePlayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        highVoltageType = recv.InsulationStatus;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 设置CT档位指令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="CtType">CT 类型</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetCTPosition(int id, byte CtType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestCTPositionChannelControlPacket rc = new CL188M_RequestCTPositionChannelControlPacket(id, (Cus_IChannelType)CtType);
            CL188M_RequestCTPositionChannelControlReplayPacket recv = new CL188M_RequestCTPositionChannelControlReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 读取误差板当前误差
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">误差板当前状态</param>
        /// <param name="verificationType">输出当前检定误差类型</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="ErrorNum">误差次数</param>
        /// <param name="wcData">误差值</param>
        /// <param name="statusType">状态类型</param>
        /// <param name="CurrentContour">电流回路状态</param>
        /// <param name="VoltageContour">电压回路状态</param>
        /// <param name="CommunicationType">通讯口状态</param>
        /// <param name="workType">工作状态</param>
        /// <param name="expandStatus">扩展状态</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadCurrentData(int id, byte wcbVerifyType, out byte verificationType, out int MeterIndex, out int ErrorNum, out string wcData, out bool[] statusType, out byte CurrentContour, out byte VoltageContour, out byte CommunicationType, out bool[] workType, out byte expandStatus, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            verificationType = 0;
            MeterIndex = 0;
            ErrorNum = 0;
            wcData = string.Empty;
            statusType = new bool[8];
            CurrentContour = 0;
            VoltageContour = 0;
            CommunicationType = 0;
            workType = new bool[8];
            expandStatus = 0;
            CL188M_RequestReadBwWcAndStatusPacket rc = new CL188M_RequestReadBwWcAndStatusPacket((Cus_WuchaType)wcbVerifyType);
            CL188M_RequestReadBwWcAndStatusReplyPacket recv = new CL188M_RequestReadBwWcAndStatusReplyPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = 0;//recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        verificationType = (byte)recv.CheckType;
                        MeterIndex = recv.CurBwNum;
                        ErrorNum = recv.wcNum;
                        wcData = recv.wcData;
                        //接线故障状态
                        statusType[0] = recv.statusTypeIsOnErr_Jxgz;
                        //预付费状态
                        statusType[1] = recv.statusTypeIsOnErr_Yfftz;
                        //报警信号状态
                        statusType[2] = recv.statusTypeIsOnErr_Bjxh;
                        //对标状态
                        statusType[3] = recv.statusTypeIsOnOver_Db;
                        //温度过高状态
                        statusType[4] = recv.statusTypeIsOnErr_Temp;
                        //光电信号状态 有表还是没有表
                        statusType[5] = recv.statusTypeIsOn_HaveMeter;
                        //表位上限位状态
                        statusType[6] = recv.statusTypeIsOn_PressUpLimit;
                        //表位下限位状态
                        statusType[7] = recv.statusTypeIsOn_PressDownLimt;
                        CurrentContour = (byte)recv.iType;
                        VoltageContour = (byte)recv.vType;
                        CommunicationType = Convert.ToByte(recv.ConnType);
                        //电能误差
                        workType[0] = recv.workStatusIsOn_Dn;
                        //需量周期
                        workType[1] = recv.workStatusIsOn_Xlzq;
                        //日计时
                        workType[2] = recv.workStatusIsOn_Rjs;
                        //多功能走字
                        workType[3] = recv.workStatusIsOn_Dnzz;
                        //对标
                        workType[4] = recv.workStatusIsOn_Db;
                        //预付费
                        workType[5] = recv.workStatusIsOn_Yff;
                        //耐压
                        workType[6] = recv.workStatusIsOn_Ny;
                        //多功能脉冲计数
                        workType[7] = recv.workStatusIsOn_Dgnmcjs;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


            return intRst;
        }
        /// <summary>
        /// 读取最近10次误差
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="verifyType">检定误差类型</param>
        /// <param name="verificationType">输出检定误差类型</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="ErrorNum">误差次数</param>
        /// <param name="wcDatas">误差数据</param>
        /// <param name="wcData">当前误差值</param>
        /// <param name="statusType">状态类型</param>
        /// <param name="CurrentContour">电流回路</param>
        /// <param name="VoltageContour">电压回路</param>
        /// <param name="CommunicationType">通讯状态</param>
        /// <param name="workType">工作状态</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        public int ReadFirstTenTimesData(int id, byte wcbVerifyType, out byte verificationType, out int MeterIndex, out int ErrorNum, out string[] wcDatas, out string wcData, out bool[] statusType, out byte CurrentContour, out byte VoltageContour, out byte CommunicationType, out bool[] workType, out string[] FrameAry)
        {
            int intRst = 1;
            verificationType = 0;
            MeterIndex = 0;
            ErrorNum = 0;
            wcDatas = new string[9];
            wcData = string.Empty;
            statusType = new bool[8];
            CurrentContour = 0;
            VoltageContour = 0;
            CommunicationType = 0;
            workType = new bool[8];
            FrameAry = new string[1];
            CL188M_RequestReadBwLast10WcAndStatusPacket rc = new CL188M_RequestReadBwLast10WcAndStatusPacket(id, (Cus_WuchaType)wcbVerifyType);
            CL188M_RequestReadBwLast10WcAndStatusReplyPacket recv = new CL188M_RequestReadBwLast10WcAndStatusReplyPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = 0;//recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        verificationType = (byte)recv.CheckType;
                        MeterIndex = recv.CurBwNum;
                        ErrorNum = recv.wcNum;
                        wcDatas = recv.wcData;
                        wcData = recv.wcData[recv.wcData.Length - 1];
                        //接线故障状态
                        statusType[0] = recv.statusTypeIsOnErr_Jxgz;
                        //预付费状态
                        statusType[1] = recv.statusTypeIsOnErr_Yfftz;
                        //报警信号状态
                        statusType[2] = recv.statusTypeIsOnErr_Bjxh;
                        //对标状态
                        statusType[3] = recv.statusTypeIsOnOver_Db;
                        //温度过高状态
                        statusType[4] = recv.statusTypeIsOnErr_Temp;
                        //光电信号状态 有表还是没有表
                        statusType[5] = recv.statusTypeIsOn_HaveMeter;
                        //表位上限位状态
                        statusType[6] = recv.statusTypeIsOn_PressUpLimit;
                        //表位下限位状态
                        statusType[7] = recv.statusTypeIsOn_PressDownLimt;
                        CurrentContour = (byte)recv.iType;
                        VoltageContour = (byte)recv.vType;
                        CommunicationType = Convert.ToByte(recv.ConnType);
                        //电能误差
                        workType[0] = recv.workStatusIsOn_Dn;
                        //需量周期
                        workType[1] = recv.workStatusIsOn_Xlzq;
                        //日计时
                        workType[2] = recv.workStatusIsOn_Rjs;
                        //多功能走字
                        workType[3] = recv.workStatusIsOn_Dnzz;
                        //对标
                        workType[4] = recv.workStatusIsOn_Db;
                        //预付费
                        workType[5] = recv.workStatusIsOn_Yff;
                        //耐压
                        workType[6] = recv.workStatusIsOn_Ny;
                        //多功能脉冲计数
                        workType[7] = recv.workStatusIsOn_Dgnmcjs;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 清除表位状态
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="statusType">状态类型</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReSetStatus(int id, byte statusType, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestClearBwStatusPacket rc = new CL188M_RequestClearBwStatusPacket(statusType);
            rc.Pos = id;
            CL188M_RequestClearBwStatusReplayPacket recv = new CL188M_RequestClearBwStatusReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 读取表位版本信息
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="MeterIndex">表位号</param>
        /// <param name="strVersion">版本号</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadVersion(int id, out int MeterIndex, out string strVersion, out string[] FrameAry)
        {
            int intRst = 1;
            MeterIndex = 0;
            strVersion = string.Empty;
            FrameAry = new string[1];
            CL188M_RequestReadWcBoardVerPacket rc = new CL188M_RequestReadWcBoardVerPacket(id);
            CL188M_RequestReadWcBoardVerReplyPacket recv = new CL188M_RequestReadWcBoardVerReplyPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        MeterIndex = recv.wcbIndex;
                        strVersion = recv.softVer;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {
                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 读取打印信息
        /// </summary>
        /// <param name="id">指定模块表位号</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadPrintInformation(int id, int Modid, out byte dataSerial, out byte dataLen, out byte[] dataAry, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            dataSerial = 0;
            dataLen = 0;
            dataAry = new byte[0];
            CL188M_RequestReadPrintInformationPacket rc = new CL188M_RequestReadPrintInformationPacket(Convert.ToByte(Modid));
            rc.Pos = id;
            CL188M_RequestReadPrintInformationReplayPacket recv = new CL188M_RequestReadPrintInformationReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        dataSerial = recv.Serial;
                        dataLen = recv.DataNum;
                        dataAry = recv.listData.ToArray();
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 读取隔离继电器过载动作可靠性检测时间
        /// </summary>
        /// <param name="id"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadIsolationrelayOverloadReliabilityTime(int id, out int meterIndex, out int detectionTime, out string[] FrameAry)
        {
            int intRst = 1;
            meterIndex = 0;
            detectionTime = 0;
            FrameAry = new string[1];

            CL188M_RequestIsolationrelayOverloadReliabilityPacket rc = new CL188M_RequestIsolationrelayOverloadReliabilityPacket();
            CL188M_RequestIsolationrelayOverloadReliabilityReplayPacket recv = new CL188M_RequestIsolationrelayOverloadReliabilityReplayPacket();
            rc.Pos = id;

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        meterIndex = recv.MeterNumber;
                        detectionTime = recv.detectionTime;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 设置隔离继电器过载动作可靠性检测性时间
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="Time">设置时间</param>
        /// <param name="FrameAry">出参帧</param>
        /// <returns></returns>
        public int SetIsolationrelayOverloadReliabilityTime(int id, byte Time, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestIsolationrelayOverloadReliabilityTimePacket rc = new CL188M_RequestIsolationrelayOverloadReliabilityTimePacket(Time);
            CL188M_RequestIsolationrelayOverloadReliabilityTimeReplayPacket recv = new CL188M_RequestIsolationrelayOverloadReliabilityTimeReplayPacket();
            rc.Pos = id;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        ///  设置电能误差检定时脉冲参数命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterConst">标准表常数</param>
        /// <param name="stdPulseFreq">标准脉冲频率</param>
        /// <param name="stdMeterConstShorttime">标准缩放倍数</param>
        /// <param name="meterConst">被检表常数</param>
        /// <param name="circles">圈数</param>
        /// <param name="meterConstZooms">被检表常数缩放倍数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetEnergePulseParams(int id, int stdMeterConst, int stdPulseFreq, int stdMeterConstShorttime, int meterConst, int circles, int meterConstZooms, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSetPulseParaPacket rc = new CL188M_RequestSetPulseParaPacket();
            rc.SetPara(id, stdMeterConst, stdPulseFreq, stdMeterConstShorttime, meterConst, circles, meterConstZooms);
            CL188M_RequestSetPulseParaReplayPacket recv = new CL188M_RequestSetPulseParaReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }


            return intRst;
        }
        /// <summary>
        /// 4.50	查询电能误差检定时脉冲参数命令
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterConst">标准表脉冲</param>
        /// <param name="stdMeterConstShorttime">标准脉冲缩放倍数</param>
        /// <param name="meterConst">被检表常数</param>
        /// <param name="circles">圈数</param>
        /// <param name="meterConstZooms">被检脉冲常数缩放倍数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadEnergePulseParams(int id, out int stdMeterConst, out int stdMeterConstShorttime, out int meterConst, out int circles, out int meterConstZooms, out string[] FrameAry)
        {
            int intRst = 1;
            stdMeterConst = 0;
            stdMeterConstShorttime = 0;
            meterConst = 0;
            circles = 0;
            meterConstZooms = 0;
            FrameAry = new string[1];
            CL188M_RequestQueryPulseParamPacket rc = new CL188M_RequestQueryPulseParamPacket();
            rc.Pos = id;
            CL188M_RequestQueryPulseParamReplayPacket recv = new CL188M_RequestQueryPulseParamReplayPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        stdMeterConst = recv.stdMeterConst;
                        stdMeterConstShorttime = recv.stdMeterConstShortTime;
                        meterConst = recv.meterConst;
                        circles = recv.meterQuans;
                        meterConstZooms = recv.stdMeterConstShortTime;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 设置时钟频率
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterTimeFreq">标准表时钟频率</param>
        /// <param name="meterTimeFreq">被检表时钟频率</param>
        /// <param name="meterPulseNum">电表脉冲常数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int SetClockFrequency(int id, int stdMeterTimeFreq, int meterTimeFreq, int meterPulseNum, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSetTimePluseNumAndXLTimePacket rc = new CL188M_RequestSetTimePluseNumAndXLTimePacket();
            CL188M_RequestSetTimePluseNumAndXLTimeReplayPacket recv = new CL188M_RequestSetTimePluseNumAndXLTimeReplayPacket();
            rc.SetPara(id, stdMeterTimeFreq, meterTimeFreq, meterPulseNum);
            if (id != 0) rc.IsNeedReturn = true;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return intRst;
        }
        /// <summary>
        /// 读取时钟频率
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="stdMeterTimeFreq">标准表时钟频率</param>
        /// <param name="meterTimeFreq">被检表时钟频率</param>
        /// <param name="meterPulseNum">被检表脉冲常数</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadClockFrequency(int id, out int stdMeterTimeFreq, out int meterTimeFreq, out int meterPulseNum, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            stdMeterTimeFreq = 0;
            meterTimeFreq = 0;
            meterPulseNum = 0;
            CL188M_RequestQueryTimePluseNumAndXLTimePacket rc = new CL188M_RequestQueryTimePluseNumAndXLTimePacket();
            rc.Pos = id;
            CL188M_RequestQueryTimePusleNumAndXLTimeReplayPacket recv = new CL188M_RequestQueryTimePusleNumAndXLTimeReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        //
                        stdMeterTimeFreq = recv.stdMeterTimeFreq;
                        meterTimeFreq = recv.meterTimeFreq;
                        meterPulseNum = recv.meterPulseNum;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {
                return -1;
            }

            return intRst;
        }
        /// <summary>
        /// 设置误差板耐压漏电流阀值
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="iCurrentLimit">漏电流阀值</param>
        /// <param name="FrameAry">输出参数</param>
        /// <returns></returns>
        public int SetACTVLeakCurrentThresholdValue(int id, int iCurrentLimit, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSetWishStandCurrentLimitPacket rc = new CL188M_RequestSetWishStandCurrentLimitPacket();
            rc.SetPara(id, iCurrentLimit);
            CL188M_RequestSetWishStandCurrentLimitReplayPacket recv = new CL188M_RequestSetWishStandCurrentLimitReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        //
                    }
                }
                else
                {
                    intRst = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


            return intRst;

        }
        /// <summary>
        /// 查询耐压漏电流阀值
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="iCurrentLimit">漏电流阀值</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ReadACTVLeakCurrentThresholdValue(int id, out int iCurrentLimit, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            iCurrentLimit = 0;
            CL188M_RequestQueryWishStandCurrentLimitPacket rc = new CL188M_RequestQueryWishStandCurrentLimitPacket();
            rc.Pos = id;
            CL188M_RequestQueryWishStandCurrentLimitReplayPacket recv = new CL188M_RequestQueryWishStandCurrentLimitReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        iCurrentLimit = recv.CurrentLimit;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {
                return -1;
            }

            return intRst;
        }

        /// <summary>
        /// 电机控制
        /// </summary>
        /// <param name="id">误差板编号</param>
        /// <param name="electricalMachineMode">电机控制方式</param>
        /// <param name="FrameAry">输出帧</param>
        /// <returns></returns>
        public int ControlMotor(int id, byte electricalMachineMode, out string[] FrameAry)
        {
            int intRst = 1;
            FrameAry = new string[1];
            CL188M_RequestSetElectromotorPacket rc = new CL188M_RequestSetElectromotorPacket();
            rc.SetPara(id, electricalMachineMode);
            CL188M_RequestSetElectromotorReplayPacket recv = new CL188M_RequestSetElectromotorReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (SendFlag)
                {
                    if (SendPacketWithRetry(m_Port, rc, recv))
                    {
                        intRst = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    intRst = 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }


            return intRst;
        }


        #endregion

        #region private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stPort"></param>
        /// <param name="UDPorCOM">true UDP,false COM</param>
        /// <param name="sp"></param>
        /// <param name="rp"></param>
        /// <returns></returns>
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {

            {
                for (int i = 0; i < RETRYTIEMS; i++)
                {
                    if (driverBase.SendData(stPort, sp, rp) == true)
                    {
                        return true;
                    }
                    System.Threading.Thread.Sleep(300);
                }
            }

            return false;
        }

        /// <summary>
        /// 发送命令 不带返回
        /// </summary>
        /// <param name="stPort">端口号</param>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        private bool SendPacketNotRevWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {
            driverBase.SendData(stPort, sp, rp);
            return true;
        }


        private string BytesToString(byte[] bytesData)
        {
            string strRevalue = string.Empty;
            if (bytesData == null || bytesData.Length < 1)
                return strRevalue;

            strRevalue = BitConverter.ToString(bytesData).Replace("-", "");
            //for (int i = 0; i < bytesData.Length; i++)
            //{
            //    byte byteTmp = bytesData[i];
            //    strRevalue += Convert.ToString(byteTmp, 16);
            //}

            return strRevalue;
        }

        #endregion

        /// <summary>
        /// 表位漏电流
        /// </summary>
        /// <param name="pos">1-n</param>
        /// <param name="data">mA</param>
        /// <returns></returns>
        public int SetInsulationLimit(int pos, float data)
        {
            return 0;
        }

        public int ReadInsulationCurrent(int pos, out float[] data)
        {
            data = new float[2];
            return 0;
        }

        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName">相应的方法名称</param>
        /// <param name="ReFrameAry">下行报文</param>
        /// <param name="ReAry">解析后的数据</param>
        /// <returns></returns>
        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            int iRsValue = 3;
            MothedName = MothedName.Replace(" ", "");

            ReAry = new string[1];

            switch (MothedName)
            {
                case "Connect":
                    {
                        //连接设备         int Connect(int Id, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestLinkReplayPacket recv = new CL188M_RequestLinkReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }



                    }
                    break;
                case "DisConnect":
                    {
                        iRsValue = 3;
                    }
                    break;
                case "SetPulseChannelAndType":
                    {
                        try
                        {	       //设置脉冲通道和类型        int SetPulseChannelAndType(int Id, int pram1, int pram2, int pram3, int pram4, int pram5,out string[] FrameAry);
                            CL188M_RequestSelectPulseChannelAndCheckTypeReplayPacket recv = new CL188M_RequestSelectPulseChannelAndCheckTypeReplayPacket();

                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "UpdateLogin":
                    {
                        //升级登录指令 int UpdateLogin(int id,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestUpdateLoginReplayPacket recv = new CL188M_RequestUpdateLoginReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReBoot":
                    {
                        //重启误差板 int ReBoot(int id,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestReBootReplayPacket recv = new CL188M_RequestReBootReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "UpdateFirmware":
                    {
                        //升级数据 int UpdateFirmware(int id,UInt16 DataSerial,byte[] bytesData,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestUpdateFirmwareReplayPacket recv = new CL188M_RequestUpdateFirmwareReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "UpdateLogin2":
                    {
                        //升级登录2 int UpdateLogin2(int id,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestUpdateLogin2ReplayPacket recv = new CL188M_RequestUpdateLogin2ReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;

                case "ReBoot2":
                    {
                        //重启设备2 int ReBoot2(int id,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestReBoot2ReplayPacket recv = new CL188M_RequestReBoot2ReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "UpdateFirmware2":
                    {
                        //升级数据2 int UpdateFirmware2(int id,UInt16 DataSerial, byte[] bytesData, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestUpdateFirmware2ReplayPacket recv = new CL188M_RequestUpdateFirmware2ReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadVersion2":
                    {
                        //读取版本2 int ReadVersion2(int id,out string Version,out string []FrameAry);
                        try
                        {
                            CL1888M_RequestReadVersion2ReplayPacket recv = new CL1888M_RequestReadVersion2ReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadPowerParams":
                    {
                        //读取功耗参数
                        //int ReadPowerParams( int id,out float AU_Ia_or_I, out float BU_Ib_or_L1_U, out float CU_Ic_or_L2_U, 
                        //                 out float AI_Ua, out float BI_Ub,out float  CI_Uc,
                        //                 out float AU_Phia_or_Phi,out float BU_Phib, out float CU_Phic, 
                        //                 out string [] FrameAry);

                        try
                        {
                            CL188M_RequestReadBwGHPramReplyPacket recv = new CL188M_RequestReadBwGHPramReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry = new string[9];
                            ReAry[0] = recv.AU_Ia_or_I.ToString();
                            ReAry[1] = recv.BU_Ib_or_L1_U.ToString();
                            ReAry[2] = recv.CU_Ic_or_L2_U.ToString();
                            ReAry[3] = recv.AI_Ua.ToString();
                            ReAry[4] = recv.BI_Ub.ToString();
                            ReAry[5] = recv.CI_Uc.ToString();
                            ReAry[6] = recv.AU_Phia_or_Phi.ToString();
                            ReAry[8] = recv.BU_Phib.ToString();
                            ReAry[9] = recv.CU_Phic.ToString();
                            iRsValue = 0;

                        }
                        catch (Exception)
                        {

                            return -1;
                        }


                    }
                    break;
                case "StartRemoteSignals":
                    {
                        //启动要信输出
                        //int StartRemoteSignals(int id, int YXTestNo, int YxTestType,
                        //                 int YxTestPulseNum, float yxTestPulseOutHz, float yxTestOutmultiple,
                        //                out string[] FrameAry);
                        try
                        {
                            CL188M_RequestStartYXOutReplayPacket recv = new CL188M_RequestStartYXOutReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "StartDCAnalog":
                    {
                        //启动直流模拟量输出        int StartDCAnalog(int id, int Current,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestStartZLMNTestFunctionReplayPacket recv = new CL188M_RequestStartZLMNTestFunctionReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "StopOutPut":
                    {
                        // 停止遥信、或直流模拟量输出         int StopOutPut(int id, int checkType, int chennNo,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestStopPCYXZLTestFunctionReplayPacket recv = new CL188M_RequestStopPCYXZLTestFunctionReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadSignals":
                    {
                        //读取遥控信号 int ReadSignals(int id, int YkCount, out int PusleCount,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestYaoKongReplayPacket recv = new CL188M_RequestYaoKongReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.m_bError == false)
                            {
                                ReAry[0] = recv.PusleCount.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetTripRelayType":
                    {
                        //设置跳闸继电器类型         int SetTripRelayType(int id,byte SwitchCommand, out string [] FrameAry);
                        try
                        {
                            CL188M_RequestChoiceSwitchReplayPacket recv = new CL188M_RequestChoiceSwitchReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetSecondaryRelayStatus":
                    {
                        //负控继电器控制命令         int SetSecondaryRelayStatus(int id, byte bearRelayStatus, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestFuKJDQReplayPacket recv = new CL188M_RequestFuKJDQReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "DirectCurrentOutPutCorrect":
                    {
                        //直流输出校准         int DirectCurrentOutPutCorrect(int id,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestDirectCurrentOutputReplayPacket recv = new CL188M_RequestDirectCurrentOutputReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "DirectCurrentOutPutRealityValueCorrect":
                    {
                        //直流输出实际电流校准         int DirectCurrentOutPutRealityValueCorrect(int id,UInt16 uiData, out short correctParams, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestDirectCurrentOutputCalibrationReplayPacket recv = new CL188M_RequestDirectCurrentOutputCalibrationReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.correctParams.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetMotorMutexAndSpeed":
                    {
                        //设置互斥电机和速度  int SetMotorMutexAndSpeed(int id, int UpOrDown, int Option, int CalTime,out string[] FrameAry);
                        try
                        {
                            CL188M_SetElectromotorTimePacketReplayPacket recv = new CL188M_SetElectromotorTimePacketReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadMotorSpeed":
                    {
                        //读取电机速度         int ReadMotorSpeed(int id, out int UpDelaytime, out int DownDelaytime,out string[] FrameAry);
                        try
                        {
                            CL188L_ReadElectromotorTimePacketReplayPacket recv = new CL188L_ReadElectromotorTimePacketReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[2];
                                ReAry[0] = recv.UpDelayTime.ToString();
                                ReAry[1] = recv.DownDelayTime.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "ReadTConnector":
                    {
                        //读取温度         int ReadTConnector(int id, out string []Temperature,out string[] FrameAry);
                        try
                        {
                            CL188L_RequestReadBwTemperatureReplyPacket recv = new CL188L_RequestReadBwTemperatureReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    ReAry[i] = recv.m_strTemperature[i];
                                }
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetSwitchType":
                    {
                        //设置继电器类型         int SetSwitchType(int id, byte RelayID, byte ControlType,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestSetSwitchReplayPacket recv = new CL188M_RequestSetSwitchReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadRelayStatus":
                    {
                        //读取继电器类型        int ReadRelayStatus(int id, byte RelayID, out byte ContrlType,out string[] FrameAry);
                        CL188M_RequestQuerySwitchStatusReplayPacket recv = new CL188M_RequestQuerySwitchStatusReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.SwitchStatus.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetEquipInformation":
                    {
                        //设置设备信息  int SetEquipInformation(int id, byte MeterType, byte CTtype,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestSetEquipInformationReplayPacket recv = new CL188M_RequestSetEquipInformationReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadEquipInformation":
                    {
                        //读取设备信息 int ReadEquipInformation( int id, out int MeterIndex, out int MeterType, out int CTbyte,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestReadEquipInformationReplayPacket recv = new CL188M_RequestReadEquipInformationReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[3];
                                ReAry[0] = recv.MeterNumber.ToString();
                                ReAry[1] = recv.MeterType.ToString();
                                ReAry[2] = recv.CtType.ToString();

                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "ReadPulseChannelAndType":
                    {
                        //读取脉冲通道和类型  int ReadPulseChannelAndType(int id,out int pram1,out int pram2,out  int pram3,out int pram4,out int pram5,out string[] FrameAry);
                        try
                        {
                            CL188M_RequestQueryPusleChannelAndCheckTypeReplayPacket recv = new CL188M_RequestQueryPusleChannelAndCheckTypeReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[6];
                                ReAry[0] = recv.wcChannelNo.ToString();//误差通道
                                ReAry[1] = recv.pulseType.ToString();//脉冲类型
                                ReAry[2] = recv.GyGy.ToString();//共阴 共阳
                                ReAry[3] = recv.verificationPusleType.ToString();//被检脉冲
                                ReAry[4] = recv.dgnWcChannelNo.ToString();//多功能误差通达
                                ReAry[5] = recv.checkType.ToString();//检定类型
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetSelectLightStatus":
                    {
                        //设置光电头状态 int SetSelectLightStatus(int id, byte selecttype, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestSelectLightStatusReplayPacket recv = new CL188M_RequestSelectLightStatusReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetSelectCheckRoad":
                    {
                        //设置双回路命令         int SetSelectCheckRoad(int id, byte iroad, byte vroad, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestSelectCheckRoadReplayPacket recv = new CL188M_RequestSelectCheckRoadReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "StartTest":
                    {
                        //开始检定试验        int StartTest(int id, byte verifyType, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestStartPCFunctionReplayPacket recv = new CL188M_RequestStartPCFunctionReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "StopTest":
                    {
                        //停止检定试验        int StopTest(int id, byte verifyType, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestStopPCFunctionReplayPacket recv = new CL188M_RequestStopPCFunctionReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetBwVolCutIsolation":
                    {
                        //设置表位电压电流隔离命令int SetBwVolCutIsolation(int id, int IsolationStatus, out string[] FrameAry);
                        //CL188M_RequestSeperateBwControlReplayPacket
                        try
                        {
                            CL188M_RequestSeperateBwControlReplayPacket recv = new CL188M_RequestSeperateBwControlReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "ReadBwVolCutsolation":
                    {
                        // 读取表位隔离状态       int ReadBwVolCutsolation(int id, out int IsolationStatus, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestQuerySeperateBwControlReplayPacket recv = new CL188M_RequestQuerySeperateBwControlReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.Seperate.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetACTVRelay":
                    {
                        //设置耐压继电器状态        int SetACTVRelay(int id, byte highVoltageType, out string[] FrameAry);
                        try
                        {
                            CL188M_SendCMDToInsulationRelaysReplayPacket recv = new CL188M_SendCMDToInsulationRelaysReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadACTVRelay":
                    {
                        //读取耐压继电器状态        int ReadACTVRelay(int id, out byte highVoltageType, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestQueryCMDToInsulationRelaysRePlayPacket recv = new CL188M_RequestQueryCMDToInsulationRelaysRePlayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.InsulationStatus.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetCTPosition":
                    {
                        //CT 档位控制器         int SetCTPosition(int id, byte CtType, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestCTPositionChannelControlReplayPacket recv = new CL188M_RequestCTPositionChannelControlReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadCurrentData":
                    {
                        //查询误差板当前误差及其状态
                        //int ReadCurrentData(int id, byte wcbVerifyType, out byte verificationType,
                        //    out int MeterIndex,out int ErrorNum,out string wcData, out bool[] statusType,out byte CurrentContour,
                        //    out byte VoltageContour,out byte CommunicationType,out bool[] workType,out byte expandStatus, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestReadBwWcAndStatusReplyPacket recv = new CL188M_RequestReadBwWcAndStatusReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry = new string[23];
                            ReAry[0] = recv.CheckType.ToString();//检定类型
                            ReAry[1] = recv.CurBwNum.ToString();//表位号
                            ReAry[2] = recv.wcNum.ToString();
                            ReAry[3] = recv.wcData;
                            //接线故障状态
                            ReAry[4] = recv.statusTypeIsOnErr_Jxgz.ToString();
                            //预付费状态
                            ReAry[5] = recv.statusTypeIsOnErr_Yfftz.ToString();
                            //报警信号状态
                            ReAry[6] = recv.statusTypeIsOnErr_Bjxh.ToString();
                            //对标状态
                            ReAry[7] = recv.statusTypeIsOnOver_Db.ToString();
                            //温度过高状态
                            ReAry[8] = recv.statusTypeIsOnErr_Temp.ToString();
                            //光电信号状态 有表还是没有表
                            ReAry[9] = recv.statusTypeIsOn_HaveMeter.ToString();
                            //表位上限位状态
                            ReAry[10] = recv.statusTypeIsOn_PressUpLimit.ToString();
                            //表位下限位状态
                            ReAry[11] = recv.statusTypeIsOn_PressDownLimt.ToString();
                            ReAry[12] = recv.iType.ToString();
                            ReAry[13] = recv.vType.ToString();
                            ReAry[14] = Convert.ToByte(recv.ConnType).ToString();
                            //电能误差
                            ReAry[15] = recv.workStatusIsOn_Dn.ToString();
                            //需量周期
                            ReAry[16] = recv.workStatusIsOn_Xlzq.ToString();
                            //日计时
                            ReAry[17] = recv.workStatusIsOn_Rjs.ToString();
                            //多功能走字
                            ReAry[18] = recv.workStatusIsOn_Dnzz.ToString();
                            //对标
                            ReAry[19] = recv.workStatusIsOn_Db.ToString();
                            //预付费
                            ReAry[20] = recv.workStatusIsOn_Yff.ToString();
                            //耐压
                            ReAry[21] = recv.workStatusIsOn_Ny.ToString();
                            //多功能脉冲计数
                            ReAry[22] = recv.workStatusIsOn_Dgnmcjs.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadFirstTenTimesData":
                    {
                        //读取误差板最新10次误差及其状态
                        //int ReadFirstTenTimesData(int id, byte wcbVerifyType, out byte verificationType,
                        //    out int MeterIndex, out int ErrorNum, out string[] wcDatas, out string wcData, out bool[] statusType, out byte CurrentContour,
                        //    out byte VoltageContour, out byte CommunicationType, out bool [] workType, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestReadBwLast10WcAndStatusReplyPacket recv = new CL188M_RequestReadBwLast10WcAndStatusReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry = new string[33];
                            ReAry[0] = recv.CheckType.ToString();
                            ReAry[1] = recv.CurBwNum.ToString();
                            ReAry[2] = recv.wcNum.ToString();
                            for (int i = 0; i < 10; i++)
                            {
                                ReAry[3 + i] = recv.wcData[i];
                            }
                            ReAry[13] = recv.wcData[recv.wcData.Length - 1];
                            //接线故障状态
                            ReAry[14] = recv.statusTypeIsOnErr_Jxgz.ToString();
                            //预付费状态
                            ReAry[15] = recv.statusTypeIsOnErr_Yfftz.ToString();
                            //报警信号状态
                            ReAry[16] = recv.statusTypeIsOnErr_Bjxh.ToString();
                            //对标状态
                            ReAry[17] = recv.statusTypeIsOnOver_Db.ToString();
                            //温度过高状态
                            ReAry[18] = recv.statusTypeIsOnErr_Temp.ToString();
                            //光电信号状态 有表还是没有表
                            ReAry[19] = recv.statusTypeIsOn_HaveMeter.ToString();
                            //表位上限位状态
                            ReAry[20] = recv.statusTypeIsOn_PressUpLimit.ToString();
                            //表位下限位状态
                            ReAry[21] = recv.statusTypeIsOn_PressDownLimt.ToString();
                            ReAry[22] = recv.iType.ToString();
                            ReAry[23] = recv.vType.ToString();
                            ReAry[24] = Convert.ToByte(recv.ConnType).ToString();
                            //电能误差
                            ReAry[25] = recv.workStatusIsOn_Dn.ToString();
                            //需量周期
                            ReAry[26] = recv.workStatusIsOn_Xlzq.ToString();
                            //日计时
                            ReAry[27] = recv.workStatusIsOn_Rjs.ToString();
                            //多功能走字
                            ReAry[28] = recv.workStatusIsOn_Dnzz.ToString();
                            //对标
                            ReAry[29] = recv.workStatusIsOn_Db.ToString();
                            //预付费
                            ReAry[30] = recv.workStatusIsOn_Yff.ToString();
                            //耐压
                            ReAry[31] = recv.workStatusIsOn_Ny.ToString();
                            //多功能脉冲计数
                            ReAry[32] = recv.workStatusIsOn_Dgnmcjs.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReSetStatus":
                    {
                        //清除表位状态        int ReSetStatus(int id,byte statusType, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestClearBwStatusReplayPacket recv = new CL188M_RequestClearBwStatusReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadVersion":
                    {
                        //读取软件版本号         int ReadVersion(int id,out int MeterIndex, out string strVersion,out string [] FrameAry);
                        try
                        {
                            CL188M_RequestReadWcBoardVerReplyPacket recv = new CL188M_RequestReadWcBoardVerReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.softVer;
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "ReadPrintInformation":
                    {
                        //读取打印信息         int ReadPrintInformation(int id,int modid,out byte dataSerial,out byte dataLen,out byte []dataAry, out string[] FrameAry);
                        CL188M_RequestReadPrintInformationReplayPacket recv = new CL188M_RequestReadPrintInformationReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[3];
                                ReAry[0] = recv.Serial.ToString();
                                ReAry[1] = recv.DataNum.ToString();
                                ReAry[2] = BytesToString(recv.listData.ToArray());
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadIsolationrelayOverloadReliabilityTime":
                    {
                        //读取隔离继电器过载动作可靠性检测时间         int ReadIsolationrelayOverloadReliabilityTime(int id, out int meterIndex, out int detectionTime, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestIsolationrelayOverloadReliabilityReplayPacket recv = new CL188M_RequestIsolationrelayOverloadReliabilityReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[2];
                                ReAry[0] = recv.MeterNumber.ToString();
                                ReAry[1] = recv.detectionTime.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "SetIsolationrelayOverloadReliabilityTime":
                    {
                        //设置隔离继电器过载动作可靠性检测时间         int SetIsolationrelayOverloadReliabilityTime(int id, byte Time, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestIsolationrelayOverloadReliabilityTimeReplayPacket recv = new CL188M_RequestIsolationrelayOverloadReliabilityTimeReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "SetEnergePulseParams":
                    {
                        //设置电能误差检定时脉冲参数        int SetEnergePulseParams(int id,int stdMeterConst, int stdPulseFreq, int stdMeterConstShorttime, int meterConst, int circles, int meterConstZooms,out string []FrameAry);
                        try
                        {
                            CL188M_RequestSetPulseParaReplayPacket recv = new CL188M_RequestSetPulseParaReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "ReadEnergePulseParams":
                    {
                        //读取电能差脉冲参数        int ReadEnergePulseParams(int id, out int stdMeterConst, out int stdMeterConstShorttime, out  int meterConst,out  int circles, out int meterConstZooms, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestQueryPulseParamReplayPacket recv = new CL188M_RequestQueryPulseParamReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[5];
                                ReAry[0] = recv.stdMeterConst.ToString();
                                ReAry[1] = recv.stdMeterConstShortTime.ToString();
                                ReAry[2] = recv.meterConst.ToString();
                                ReAry[3] = recv.meterQuans.ToString();
                                ReAry[4] = recv.meterConstShortTime.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {
                            return -1;
                        }

                    }
                    break;
                case "SetClockFrequency":
                    {
                        //设置日计时检定时钟频率及需量周期检定时间         int SetClockFrequency(int id, int stdMeterTimeFreq, int meterTimeFreq, int meterPulseNum, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestSetTimePluseNumAndXLTimeReplayPacket recv = new CL188M_RequestSetTimePluseNumAndXLTimeReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "ReadClockFrequency":
                    {
                        //读取日计时检定时钟频率及需量周期检定时间 int ReadClockFrequency(int id, out int stdMeterTimeFreq, out int meterTimeFreq, out int meterPulseNum, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestQueryTimePusleNumAndXLTimeReplayPacket recv = new CL188M_RequestQueryTimePusleNumAndXLTimeReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry = new string[3];
                            ReAry[0] = recv.stdMeterTimeFreq.ToString();
                            ReAry[1] = recv.meterTimeFreq.ToString();
                            ReAry[2] = recv.meterPulseNum.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "SetACTVLeakCurrentThresholdValue":
                    {
                        //设置耐压试验漏电流阀值         int SetACTVLeakCurrentThresholdValue(int id,int iCurrentLimit, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestSetWishStandCurrentLimitReplayPacket recv = new CL188M_RequestSetWishStandCurrentLimitReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "ReadACTVLeakCurrentThresholdValue":
                    {
                        //读取漏电流阀值        int ReadACTVLeakCurrentThresholdValue(int id, out int iCurrentLimit, out string[] FrameAry);
                        try
                        {
                            CL188M_RequestQueryWishStandCurrentLimitReplayPacket recv = new CL188M_RequestQueryWishStandCurrentLimitReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[2];
                                ReAry[0] = recv.meterNumber.ToString();
                                ReAry[1] = recv.CurrentLimit.ToString();
                                iRsValue = 0;
                            }
                            else
                            {
                                iRsValue = 1;
                            }
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                    break;
                case "ControlMotor":
                    {
                        //设置电机模式         int ControlMotor(int id, byte electricalMachineMode,out string []FrameAry);
                        try
                        {
                            CL188M_RequestSetElectromotorReplayPacket recv = new CL188M_RequestSetElectromotorReplayPacket();
                            recv.ParsePacket(ReFrameAry);

                            ReAry[0] = recv.ReciveResult.ToString();
                            iRsValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;

                default:
                    break;
            }
            return iRsValue;
        }
    }
}
