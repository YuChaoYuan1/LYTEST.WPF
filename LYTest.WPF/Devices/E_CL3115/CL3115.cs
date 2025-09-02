using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using CLOU;
using CLOU.Enum;
using CLOU.LogModel;
using CLOU.SocketModule.Packet;
using CLOU.Struct;

namespace E_CL3115
{

    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
     ComVisible(true)]
    public interface IClass_Interface
    {
        /// <summary>
        /// 初始化设备通讯参数
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTme">最长等待时间</param>
        /// <param name="WaitSencondsPerByte">帧字节间隔时间</param>
        /// <param name="IP">Ip地址</param>
        /// <param name="RemotePort">远程端口</param>
        /// <param name="LocalStartPort">本地端口</param>
        /// <returns>是否注册成功</returns>
        [DispId(1)]
        int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort);
        /// <summary>
        /// 注册Com 口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="strSetting"></param>
        /// <param name="maxWaittime"></param>
        /// <returns></returns>
        [DispId(2)]
        int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte);
        /// <summary>
        /// 连机
        /// </summary>
        /// <param name="FrameAry">输出连机报文</param>
        /// <returns></returns>
        [DispId(3)]
        int Connect(out string[] FrameAry);
        /// <summary>
        /// 断开连机
        /// </summary>
        /// <param name="FrameAry">输出断开连机报文</param>
        /// <returns></returns>
        [DispId(4)]
        int DisConnect(out string[] FrameAry);
        /// <summary>
        /// 读实时测量数据
        /// </summary>
        /// <param name="instValue">输出测量数据</param>
        /// <param name="FrameAry">读实时测量数据报文</param>
        /// <returns></returns>
        [DispId(5)]
        int ReadInstMetricAll(out float[] instValue, out string[] FrameAry);
        /// <summary>
        /// 读标准表常数
        /// </summary>
        /// <param name="pulseConst"></param>
        /// <param name="FrameAry">读标准表常数</param>
        /// <returns></returns>
        [DispId(6)]
        int ReadStdPulseConst(out int pulseConst, out string[] FrameAry);
        /// <summary>
        /// 读取电能量
        /// </summary>
        /// <param name="energy">返回电能量</param>        
        /// <param name="FrameAry">读取电能量输出报文</param>
        /// <returns></returns>
        [DispId(7)]
        int ReadEnergy(out float energy, out string[] FrameAry);
        /// <summary>
        /// 读电能量累计脉冲数
        /// </summary>
        /// <param name="pulses">读电能量累计脉冲数</param>
        /// <param name="FrameAry">读电能量累计脉冲数输出报文</param>
        /// <returns></returns>
        [DispId(8)]
        int ReadTotalPulses(out long pulses, out string[] FrameAry);
        /// <summary>
        /// 读电能走字数据
        /// </summary>
        /// <param name="testEnergy">返回走字电能量</param>
        /// <param name="pulses">读电能量累计脉冲数</param>
        /// <param name="FrameAry">读电能走字数据输出报文</param>
        /// <returns></returns>
        [DispId(9)]
        int ReadTestEnergy(out float testEnergy, out long pulses, out string[] FrameAry);
        /// <summary>
        /// 读仪器版本号
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="FrameAry">读仪器版本号输出报文</param>
        /// <returns></returns>
        [DispId(10)]
        int ReadVersion(out string version, out string[] FrameAry);
        /// <summary>
        /// 读各项电压电流谐波幅值
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="harmonicArry">65个长度 </param>
        /// <param name="FrameAry">读各项电压电流谐波幅值输出报文</param>
        /// <returns></returns>
        [DispId(11)]
        int ReadHarmonicArry(int phase, out float[] harmonicArry, out string[] FrameAry);
        /// <summary>
        /// 读各项电压电流波形数据
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="waveformArry"> 256个长度</param>
        /// <param name="FrameAry">读各项电压电流波形数据输出报文</param>
        /// <returns></returns>
        [DispId(12)]
        int ReadWaveformArry(int phase, out float[] waveformArry, out string[] FrameAry);
        /// <summary>
        /// 设置接线方式
        /// </summary>
        /// <param name="rangeMode">量程类型，0-自动，1-手动</param>
        /// <param name="wiringMode">接线类型</param>
        /// <param name="FrameAry">设置接线方式输出报文</param>
        /// <returns></returns>
        [DispId(13)]
        int SetWiringMode(int rangeMode, int wiringMode, out string[] FrameAry);
        /// <summary>
        /// 设置标准表常数
        /// </summary>
        /// <param name="pulseConst">标准表常数</param>
        /// <param name="FrameAry">设置标准表常数输出报文</param>
        /// <returns></returns>
        [DispId(14)]
        int SetStdPulseConst(int pulseConst, out string[] FrameAry);
        /// <summary>
        /// 设置电能指示
        /// </summary>
        /// <param name="powerMode">1：总有功电能 2：总无功电能</param>
        /// <param name="FrameAry">设置电能指示输出报文</param>
        /// <returns></returns>
        [DispId(15)]
        int SetPowerMode(int powerMode, out string[] FrameAry);
        /// <summary>
        /// 设置电能误差计算启动开关
        /// </summary>
        /// <param name="calcType">0 停止计算  </param>  
        ///                        1 开始计算电能误差  
        ///                        2 开始计算电能走字
        /// <param name="FrameAry">设置电能误差计算启动开关输出报文</param>
        /// <returns></returns>
        [DispId(16)]
        int SetErrCalcType(int calcType, out string[] FrameAry);
        /// <summary>
        /// 设置电能参数
        /// </summary>
        /// <param name="rangeMode">量程方式 0-自动，1-手动</param>
        /// <param name="wiringMode">接线方式</param>
        /// <param name="powerMode">电能方式</param>
        /// <param name="calcType">电能误差计算开关</param>
        /// <param name="FrameAry">设置电能参数输出报文</param>
        /// <returns></returns>
        [DispId(17)]
        int SetStdParams(int rangeMode, int wiringMode, int powerMode, int calcType, out string[] FrameAry);
        /// <summary>
        /// 设置档位
        /// </summary>
        /// <param name="UaRange">Ua档位</param>
        /// <param name="UbRange">Ub档位</param>
        /// <param name="UcRange">Uc档位</param>
        /// <param name="IaRange">Ia档位</param>
        /// <param name="IbRange">Ib档位</param>
        /// <param name="IcRange">Ic档位</param>
        /// <param name="FrameAry">设置档位输出报文</param>
        /// <returns></returns>
        [DispId(18)]
        int SetRange(int UaRange, int UbRange, int UcRange, int IaRange, int IbRange, int IcRange, out string[] FrameAry);
        /// <summary>
        /// 设置显示界面
        /// </summary>
        /// <param name="formType">界面类型</param>
        /// <param name="FrameAry">设置显示界面输出报文</param>
        /// <returns></returns>
        [DispId(19)]
        int SetDisplayForm(int formType, out string[] FrameAry);
        /// <summary>
        /// 设置电能误差检定参数
        /// </summary>
        /// <param name="pulseNum"></param>
        /// <param name="testConst"></param>
        /// <param name="FrameAry">设置电能误差检定参数输出报文</param>
        /// <returns></returns>
        [DispId(20)]
        int SetCalcParams(int pulseNum, int testConst, out string[] FrameAry);
        /// <summary>
        /// 读取电能误差
        /// </summary>
        /// <param name="error"></param>
        /// <param name="FrameAry">读取电能误差输出报文</param>
        /// <returns></returns>
        [DispId(21)]
        int ReadError(out float error, out string[] FrameAry);
        /// <summary>
        /// 读取最近一次误差及误差次数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="error"></param>
        /// <param name="FrameAry">读取最近一次误差及误差次数输出报文</param>
        /// <returns></returns>
        [DispId(22)]
        int ReadLastError(out int num, out float error, out string[] FrameAry);
        /// <summary>
        /// 设置获取请求报文标志
        /// </summary>
        /// <param name="Flag">True:发送报文,并传出报文,false:不发送,只传出报文</param>
        /// <returns></returns>
        [DispId(23)]
        int SetSendFlag(bool Flag);
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName">函数名(有出参FrameAry的函数的名称)</param>
        /// <param name="ReFrameAry">下行报文</param>
        /// <param name="ReAry">解析后的数据</param>
        /// <returns></returns>
        [DispId(24)]
        int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry);
        /// <summary>
        /// 设置电压电流幅值和相位 @C_B
        /// </summary>
        /// <param name="c_u_v">C相电压幅值</param>
        /// <param name="b_u_v">B相电压幅值</param>
        /// <param name="a_u_v">A相电压幅值</param>
        /// <param name="c_i_v">C相电流幅值</param>
        /// <param name="b_i_v">B相电流幅值</param>
        /// <param name="a_i_v">A相电流幅值</param>
        /// <param name="c_u_p">C相电压相位</param>
        /// <param name="b_u_p">B相电压相位</param>
        /// <param name="a_u_p">A相电压相位</param>
        /// <param name="c_i_p">C相电流相位</param>
        /// <param name="b_i_p">B相电流相位</param>
        /// <param name="a_i_p">A相电流相位</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(25)]
        int SetAmplitudeAndPhase(float c_u_v, float b_u_v, float a_u_v, float c_i_v, float b_i_v, float a_i_v, float c_u_p, float b_u_p, float a_u_p, float c_i_p, float b_i_p, float a_i_p, out string[] FrameAry);
        /// <summary>
        /// 设置通讯地址 @C_B
        /// </summary>
        /// <param name="deviceAddress"></param>
        /// <returns></returns>
        [DispId(26)]
        int SetDeviceAddress(int deviceAddress);
        /// <summary>
        /// 读取测试走字数据 @C_B
        /// </summary>
        /// <param name="totalEnergy">累计电能量</param>
        /// <param name="pulseNum">累计脉冲数</param>
        /// <returns></returns>
        [DispId(27)]
        int ReadTestRegister(out float totalEnergy, out long pulseNum, out string[] FrameAry);
        /// <summary>
        /// 在线监测 @C_B
        /// </summary>
        /// <param name="isConnect">输出联机状态，true-联机状态的，false-脱机状态</param>
        /// <returns></returns>
        [DispId(28)]
        int ReadOnline(out bool isConnect);
        /// <summary>
        /// 设置报文记录打印 @C_B
        /// </summary>
        /// <param name="deviceID">设备ID，主要区分是否为-1</param>
        /// <param name="bStart">true-开始记录，false-暂停记录</param>
        /// <returns></returns>
        [DispId(29)]
        int SetAutoRecord(string deviceID, bool bStart);
        /// <summary>
        /// 解析报文 @C_B
        /// </summary>
        /// <param name="frames">报文</param>
        /// <param name="frameValue">报文信息</param>
        /// <returns></returns>
        [DispId(30)]
        int AnalyzeFrames(string[] frames, out string[] frameValue);
        /// <summary>
        /// 设置校正值 @C_B
        /// </summary>
        /// <param name="amplitudeOrAngle">幅值或者相位，0=相位，1=幅值</param>
        /// <param name="referenceValue">标准值</param>
        /// <param name="cl3115Value">3115的值</param>
        /// <param name="FrameAry">报文信息</param>
        /// <returns></returns>
        [DispId(31)]
        int SetCorrectionValue(int amplitudeOrAngle, float referenceValue, float cl3115Value, out string[] FrameAry);
        /// <summary>
        /// 设置校正相别标识 @C_B
        /// </summary>
        /// <param name="phase">相别，0=A相电压，1=B相电压，2=C相电压，3=A相电流，4=B相电流，5=C相电流</param>
        /// <param name="amplitudeOrAngle">幅值或者相位，0=相位，1=幅值</param>
        /// <param name="FrameAry">报文信息</param>
        /// <returns></returns>
        [DispId(32)]
        int SetPhaseInformation(int phase, int amplitudeOrAngle, out string[] FrameAry);
        /// <summary>
        /// 设置标准表常数
        /// </summary>
        /// <param name="auto">是否自动常数</param>
        /// <param name="pulseConst">标准表常数</param>
        /// <param name="U">电压值</param>
        /// <param name="I">电流值</param>
        /// <param name="outPulseConst">输出标准表常数</param>
        /// <param name="FrameAry">报文信息</param>
        /// <returns></returns>
        [DispId(33)]
        int SetStdPulseConstValue(bool auto, int pulseConst, float U, float I, out int outPulseConst, out string[] FrameAry);
        /// <summary>
        /// 重置走字
        /// </summary>
        /// <returns></returns>
        [DispId(34)]
        int ResetRegister();
        /// <summary>
        /// 设置档位by值
        /// </summary>
        /// <param name="Ua">Ua</param>
        /// <param name="Ub">Ub</param>
        /// <param name="Uc">Uc</param>
        /// <param name="Ia">Ia</param>
        /// <param name="Ib">Ib</param>
        /// <param name="Ic">Ic</param>
        /// <param name="FrameAry">设置档位输出报文</param>
        /// <returns></returns>
        [DispId(35)]
        int SetRangeByValue(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, out string[] FrameAry);
    }

    [Guid("B5085DB3-E6F6-405A-8D68-8123F39176E0"),
    InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
    ComVisible(true)]
    public interface IClass_Events
    {
        [DispId(80)]
        void RecordMsgCallBack(string logFrameInfo);
    }

    [Guid("D09C2799-FEC8-4e35-BB2E-A59255875188"),
    ProgId("CLOU.CL3115"),
    ClassInterface(ClassInterfaceType.None),
    ComDefaultInterface(typeof(IClass_Interface)),
    ComSourceInterfaces(typeof(IClass_Events)),
    ComVisible(true)]
    public class CL3115 : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 控制端口
        /// </summary>
        private readonly StPortInfo m_MeterStd = new StPortInfo();

        /// <summary>
        /// 
        /// </summary>
        private readonly DriverBase driverBase = new DriverBase();
        /// <summary>
        /// 是否发送报文标志位
        /// </summary>
        private bool sendFlag = true;

        public CL3115()
        {
            //m_MeterStd = new StPortInfo();
            DataLoger.DeleteAllLog();
            IntStdMeterConst();
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
            m_MeterStd.m_Exist = 1;
            m_MeterStd.m_IP = IP;
            m_MeterStd.m_Port = ComNumber;
            m_MeterStd.m_Port_IsUDPorCom = true;
            m_MeterStd.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_MeterStd.m_Port_Setting, m_MeterStd.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return 1;
            }
            return 0;
        }
        /// <summary>
        /// 初始化COM口
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">最长等待回复时间</param>
        /// <param name="WaitSencondsPerByte">单帧字节等待时间</param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_MeterStd.m_Exist = 1;
            m_MeterStd.m_IP = "";
            m_MeterStd.m_Port = ComNumber;
            m_MeterStd.m_Port_IsUDPorCom = false;
            m_MeterStd.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_MeterStd.m_Port_Setting, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return 1;
            }

            return 0;
        }
        /// <summary>
        /// 连机
        /// </summary>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            CL3115_RequestLinkPacket rc = new CL3115_RequestLinkPacket();
            CL3115_RequestLinkReplyPacket recv = new CL3115_RequestLinkReplyPacket();
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        bool linkOk = recv.ReciveResult == RecvResult.OK;
                        if (linkOk) StartOnlineMonitoring();
                        return linkOk ? 0 : 1006;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

        }
        /// <summary>
        /// 脱机
        /// </summary>
        /// <returns></returns>
        public int DisConnect(out string[] FrameAry)
        {
            CL3115_RequestLinkPacket rc = new CL3115_RequestLinkPacket();
            CL3115_RequestLinkReplyPacket recv = new CL3115_RequestLinkReplyPacket();
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        bool linkOk = recv.ReciveResult == RecvResult.OK;
                        if (linkOk) EndOnlineMonitoring();
                        return linkOk ? 0 : 1006;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

        }
        /// <summary>
        /// 读取瞬时测量数据
        /// </summary>
        /// <param name="instValue"></param>
        /// <returns></returns>
        public int ReadInstMetricAll(out float[] instValue, out string[] FrameAry)
        {
            CL3115_RequestReadStdInfoPacket rc = new CL3115_RequestReadStdInfoPacket();
            CL3115_RequestReadStdInfoReplayPacket rcback = new CL3115_RequestReadStdInfoReplayPacket();
            instValue = new float[40];
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, rcback))
                    {

                        CLOU.Struct.stStdInfo stdInfo = rcback.PowerInfo;
                        //电压
                        instValue[0] = stdInfo.Ua;
                        instValue[1] = stdInfo.Ub;
                        instValue[2] = stdInfo.Uc;
                        //电流
                        instValue[3] = stdInfo.Ia;
                        instValue[4] = stdInfo.Ib;
                        instValue[5] = stdInfo.Ic;

                        //电压相位
                        instValue[6] = stdInfo.Phi_Ua;
                        instValue[7] = stdInfo.Phi_Ub;
                        instValue[8] = stdInfo.Phi_Uc;
                        //电流相位
                        instValue[9] = stdInfo.Phi_Ia;
                        instValue[10] = stdInfo.Phi_Ib;
                        instValue[11] = stdInfo.Phi_Ic;

                        instValue[12] = stdInfo.PhiAngle_A;
                        instValue[13] = stdInfo.PhiAngle_B;
                        instValue[14] = stdInfo.PhiAngle_C;
                        instValue[15] = stdInfo.SAngle;

                        //有功功率
                        instValue[16] = stdInfo.Pa;
                        instValue[17] = stdInfo.Pb;
                        instValue[18] = stdInfo.Pc;
                        instValue[19] = stdInfo.P;
                        //无功功率
                        instValue[20] = stdInfo.Qa;
                        instValue[21] = stdInfo.Qb;
                        instValue[22] = stdInfo.Qc;
                        instValue[23] = stdInfo.Q;
                        //视在功率
                        instValue[24] = stdInfo.Sa;
                        instValue[25] = stdInfo.Sb;
                        instValue[26] = stdInfo.Sc;
                        instValue[27] = stdInfo.S;

                        instValue[28] = stdInfo.PowerFactor_A;
                        instValue[29] = stdInfo.PowerFactor_B;
                        instValue[30] = stdInfo.PowerFactor_C;

                        instValue[31] = stdInfo.COS;
                        instValue[32] = stdInfo.SIN;
                        //频率
                        instValue[33] = stdInfo.Freq;
                        //档位
                        instValue[34] = Convert.ToInt32(stdInfo.Scale_Ua);
                        instValue[35] = Convert.ToInt32(stdInfo.Scale_Ub);
                        instValue[36] = Convert.ToInt32(stdInfo.Scale_Uc);
                        instValue[37] = Convert.ToInt32(stdInfo.Scale_Ia);
                        instValue[38] = Convert.ToInt32(stdInfo.Scale_Ib);
                        instValue[39] = Convert.ToInt32(stdInfo.Scale_Ic);
                        string tmp = string.Format(";{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26}", stdInfo.Uc, stdInfo.Ub, stdInfo.Ua, stdInfo.Ic, stdInfo.Ib, stdInfo.Ia, stdInfo.Freq, stdInfo.Phi_Uc, stdInfo.Phi_Ub, stdInfo.Phi_Ua, stdInfo.Phi_Ic, stdInfo.Phi_Ib, stdInfo.Phi_Ia, stdInfo.COS, stdInfo.SIN, stdInfo.Pc, stdInfo.Pb, stdInfo.Pa, stdInfo.P, stdInfo.Qc, stdInfo.Qb, stdInfo.Qa, stdInfo.Q, stdInfo.Sc, stdInfo.Sb, stdInfo.Sa, stdInfo.S);
                        DataLoger.WriteValue(tmp);
                        return 0;
                    }
                    else
                    {
                        DataLoger.WriteValue(";");
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

        }
        //private static string GetUaRange(int uaNum)
        //{
        //    string[] UaList = { "600", "480", "240", "120", "60" };
        //    int len = UaList.Length;
        //    if (uaNum >= len || uaNum < 0) return "0";
        //    return UaList[uaNum];
        //}
        ///// <summary>
        ///// 返回电流档位值
        ///// </summary>
        ///// <param name="iaNum">电流档位序号</param>
        ///// <returns></returns>
        //private static string GetIaRange(int iaNum)
        //{
        //    string[] iaList = { "100", "50", "20", "10", "5", "2", "1", "0.5", "0.2", "0.1", "0.05", "0.02", "0.01" };
        //    int len = iaList.Length;
        //    if (iaNum >= len || iaNum < 0) return "0";
        //    return iaList[iaNum];
        //}
        /// <summary>
        /// 读取标准表脉冲常数
        /// </summary>
        /// <param name="pulseConst"></param>
        /// <returns></returns>
        public int ReadStdPulseConst(out int pulseConst, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterConstPacket CL3115packet = new CL3115_RequestReadStdMeterConstPacket();
            CL3115_RequestReadStdMeterConstReplayPacket CL3115recv = new CL3115_RequestReadStdMeterConstReplayPacket();
            pulseConst = 0;
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(CL3115packet.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, CL3115packet, CL3115recv))
                    {
                        if (CL3115recv.ReciveResult == RecvResult.OK)
                        {
                            pulseConst = CL3115recv.meterConst;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 读取标准表电能
        /// </summary>
        /// <param name="energy">标准表电能</param>
        /// <returns></returns>
        public int ReadEnergy(out float energy, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterTotalNumPacket rc = new CL3115_RequestReadStdMeterTotalNumPacket();
            CL3115_RequestReadStdMeterTotalNumReplayPacket resv = new CL3115_RequestReadStdMeterTotalNumReplayPacket();
            energy = 0.0f;
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, resv))
                    {
                        if (resv.ReciveResult == RecvResult.OK)
                        {
                            energy = resv.MeterTotalNum;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 读取电能累计脉冲数
        /// </summary>
        /// <param name="pulses"></param>
        /// <returns></returns>
        public int ReadTotalPulses(out long pulses, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterTotalPulseNumPacket rc = new CL3115_RequestReadStdMeterTotalPulseNumPacket();
            CL3115_RequestReadStdMeterTotalPulseNumReplayPacket resv = new CL3115_RequestReadStdMeterTotalPulseNumReplayPacket();

            pulses = 0;
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, resv))
                    {
                        if (resv.ReciveResult == RecvResult.OK)
                        {
                            pulses = resv.Pulsenum;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }

                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

        }
        /// <summary>
        /// 读取电能走字数据
        /// </summary>
        /// <param name="testEnergy"></param>
        /// <returns></returns>
        public int ReadTestEnergy(out float testEnergy, out long PulseNum, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterZZDataPacket rc = new CL3115_RequestReadStdMeterZZDataPacket();
            CL3115_RequestReadStdMeterZZDataReplayPacket resv = new CL3115_RequestReadStdMeterZZDataReplayPacket();
            testEnergy = 0.0F;
            PulseNum = 0L;
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, resv))
                    {
                        if (resv.ReciveResult == RecvResult.OK)
                        {
                            testEnergy = resv.meterTotalNum;
                            PulseNum = resv.meterPulseNum;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 读取版本号
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public int ReadVersion(out string version, out string[] FrameAry)
        {
            version = "null";
            FrameAry = new string[1];
            try
            {
                var send = new CL3115_RequestReadVersionPacket();
                var recv = new CL3115_RequestReadVersionReplyPacket();
                FrameAry[0] = BytesToString(send.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, send, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            version = string.Format("{0}{1}{2}{3}", recv.CLTVer, recv.DeviceType, recv.VerNo, recv.SerialNo);
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// 读取各相电压电流谐波幅值（分两帧读取数据）
        /// </summary>
        /// <param name="phase">相别</param>
        /// <param name="harmonicArry"></param>
        /// <returns></returns>
        public int ReadHarmonicArry(int phase, out float[] harmonicArry, out string[] FrameAry)
        {
            harmonicArry = new float[65];
            CL3115_RequestReadStdMeterHarmonicArryPacket rc = new CL3115_RequestReadStdMeterHarmonicArryPacket();
            CL3115_RequestReadStdMeterHarmonicArryReplayPacket recv = new CL3115_RequestReadStdMeterHarmonicArryReplayPacket();
            int start = 0;
            FrameAry = new string[2];
            try
            {
                int len = 32;
                rc.SetPara(phase, start, len);
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            for (int i = 1; i < 33; i++)
                            {
                                harmonicArry[i] = recv.fHarmonicArryData[i - 1];
                            }
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                    //读取后32次谐波数据
                    start = 32;
                    len = 32;
                    rc.SetPara(phase, start, len);
                    FrameAry[1] = BytesToString(rc.GetPacketData());
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            for (int i = 33; i < 65; i++)
                            {
                                harmonicArry[i] = recv.fHarmonicArryData[i - 33];
                            }
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                    if (harmonicArry[1] == 0)
                    {
                        harmonicArry[0] = 0f;
                    }
                    else
                    {
                        for (int j = 2; j < 65; j++)
                        {
                            harmonicArry[0] += (float)Math.Pow(harmonicArry[j] / harmonicArry[1], 2);
                        }
                        harmonicArry[0] = (float)Math.Sqrt(harmonicArry[0]);
                    }
                    return 0;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 读取各项电压电流波形数据（分三帧读取数据）
        /// </summary>
        /// <param name="phase">相别</param>
        /// <param name="waveformArry">256个点的数据</param>
        /// <returns></returns>
        public int ReadWaveformArry(int phase, out float[] waveformArry, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterWaveformArryPacket rc = new CL3115_RequestReadStdMeterWaveformArryPacket();
            CL3115_RequestReadStdMeterWaveformArryReplayPacket recv = new CL3115_RequestReadStdMeterWaveformArryReplayPacket();
            waveformArry = new float[256];
            int iCountNum = 256;
            FrameAry = new string[3];
            try
            {
                for (int i = 1; i < 4; i++)
                {
                    ushort iStart;
                    int iOneFrame;
                    if (iCountNum > 100)
                    {
                        iCountNum -= 100;
                        iOneFrame = 100;
                        iStart = (ushort)((i - 1) * 200);
                        rc.SetPara(Convert.ToByte(phase), iStart, 200);
                    }
                    else
                    {
                        iOneFrame = iCountNum;
                        iStart = (ushort)((i - 1) * 200);
                        rc.SetPara(Convert.ToByte(phase), iStart, Convert.ToByte(iCountNum * 2));
                    }

                    FrameAry[i - 1] = BytesToString(rc.GetPacketData());
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            if (recv.fWaveformData.Length == iOneFrame)
                            {
                                for (int j = 0; j < iOneFrame; j++)
                                {
                                    waveformArry[(i - 1) * 100 + j] = recv.fWaveformData[j];
                                }
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        else
                        {
                            return 1006;//返回数据格式错误
                        }
                    }
                    else
                    {
                        return 1;//发送数据失败
                    }
                }
                return 0;//成功
            }
            catch (Exception)
            {

                return -1;
            }



        }
        /// <summary>
        /// 设置接线方式
        /// </summary>
        /// <param name="wiringMode"></param>
        /// <returns></returns>
        public int SetWiringMode(int rangeMode, int wiringMode, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterLinkTypePacket rc = new CL3115_RequestSetStdMeterLinkTypePacket();
            CL3115_RequestSetStdMeterLinkTypeReplayPacket recv = new CL3115_RequestSetStdMeterLinkTypeReplayPacket();
            //
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    Cus_Clfs ccl = Cus_Clfs.PT4;
                    ccl = wiringMode >= 6 ? Cus_Clfs.PT4 : (Cus_Clfs)(wiringMode % 5);
                    rc.SetPara(ccl, rangeMode == 0);
                    //rc.SetPara(ccl, wiringMode > 4 ? false : true);
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 设置标准表常数
        /// </summary>
        /// <param name="pulseConst">标准表常数</param>
        /// <returns></returns>
        public int SetStdPulseConst(int pulseConst, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterConstPacket rc = new CL3115_RequestSetStdMeterConstPacket();
            CL3115_RequestSetStdMeterConstReplayPacket recv = new CL3115_RequestSetStdMeterConstReplayPacket();
            FrameAry = new string[1];

            try
            {
                rc.SetPara(pulseConst);
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        return recv.ReciveResult == RecvResult.OK ? 0 : 1006;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


        }
        public int SetStdPulseConstValue(bool auto, int pulseConst, float U, float I, out int outPulseConst, out string[] FrameAry)
        {
            outPulseConst = 0;
            CL3115_RequestSetStdMeterConstPacket rc = new CL3115_RequestSetStdMeterConstPacket();
            CL3115_RequestSetStdMeterConstReplayPacket recv = new CL3115_RequestSetStdMeterConstReplayPacket();
            FrameAry = new string[1];

            try
            {
                if (auto)
                {
                    pulseConst = SearchStdMeterConst(FetUSwByU(U), FetISwByI(I));
                }
                outPulseConst = pulseConst;
                rc.SetPara(pulseConst);
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 设置电能指示
        /// </summary>
        /// <param name="powerMode">1:总有功|2：总无功3</param>
        /// <returns></returns>
        public int SetPowerMode(int powerMode, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterUsE1typePacket rc = new CL3115_RequestSetStdMeterUsE1typePacket();
            CL3115_RequestSetStdMeterUsE1typeReplayPacket recv = new CL3115_RequestSetStdMeterUsE1typeReplayPacket();
            FrameAry = new string[1];

            try
            {
                //rc.SetPara(powerMode == 1 ? Cus_PowerFangXiang.ZXP : Cus_PowerFangXiang.ZXQ);
                rc.SetPara(powerMode);
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 设置电能计算误差启动开关
        /// </summary>
        /// <param name="calcType"> 0，停止；1，开始计算电能误差；2，开始计算电能走字</param>
        /// <returns></returns>
        public int SetErrCalcType(int calcType, out string[] FrameAry)
        {
            CL3115_RequestStartTaskPacket rc = new CL3115_RequestStartTaskPacket();
            CL3115_RequestStartTaskReplyPacket recv = new CL3115_RequestStartTaskReplyPacket();
            rc.SetPara(calcType);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }



        }
        /// <summary>
        /// 设置标准表参数
        /// </summary>
        /// <param name="rangeMode"></param>
        /// <param name="wiringMode">接线方式</param>
        /// <param name="powerMode">供电模式</param>
        /// <param name="calcType">计算误差开关</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetStdParams(int rangeMode, int wiringMode, int powerMode, int calcType, out string[] FrameAry)
        {
            CL3115_RequestSetParaPacket rc = new CL3115_RequestSetParaPacket();
            CL3115_RequestSetParaReplayPacket recv = new CL3115_RequestSetParaReplayPacket();
            FrameAry = new string[1];

            try
            {
                Cus_Clfs ccl = Cus_Clfs.PT4;
                if (wiringMode >= 6)
                {
                    ccl = Cus_Clfs.PT4;
                }
                else
                {
                    ccl = (Cus_Clfs)(wiringMode % 5);
                }
                rc.SetPara(ccl, powerMode == 1 ? Cus_PowerFangXiang.ZXP : Cus_PowerFangXiang.ZXQ, calcType, rangeMode == 0);
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }

        }
        /// <summary>
        /// 设置档位
        /// </summary>
        /// <param name="UaRange">UA档位</param>
        /// <param name="UbRange">UB档位</param>
        /// <param name="UcRange">UC档位</param>
        /// <param name="IaRange">IA档位</param>
        /// <param name="IbRange">IB档位</param>
        /// <param name="IcRange">IC档位</param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        public int SetRange(int UaRange, int UbRange, int UcRange, int IaRange, int IbRange, int IcRange, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterDangWeiPacket rc = new CL3115_RequestSetStdMeterDangWeiPacket
                    ((Cus_StdMeterURange)UaRange, (Cus_StdMeterURange)UbRange, (Cus_StdMeterURange)UcRange,
                    (Cus_StdMeterIRange)IaRange, (Cus_StdMeterIRange)IbRange, (Cus_StdMeterIRange)IcRange, true);
            CL3115_RequestSetStdMeterDangWeiReplayPacket recv = new CL3115_RequestSetStdMeterDangWeiReplayPacket();
            FrameAry = new string[1];

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                //
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int SetRangeByValue(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterDangWeiPacket rc = new CL3115_RequestSetStdMeterDangWeiPacket
                    (FetUSwByU(Ua), FetUSwByU(Ub), FetUSwByU(Uc),
                    FetISwByI(Ia), FetISwByI(Ib), FetISwByI(Ic), true);
            CL3115_RequestSetStdMeterDangWeiReplayPacket recv = new CL3115_RequestSetStdMeterDangWeiReplayPacket();
            FrameAry = new string[1];

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// 设置标准表界面 1：谐波柱图界面2：谐波列表界面3：波形界面4：清除设置界面
        /// </summary>
        /// <param name="formType"></param>
        /// <param name="FrameAry">输出报文</param>
        /// <returns></returns>
        public int SetDisplayForm(int formType, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterScreenPacket rc =
                new CL3115_RequestSetStdMeterScreenPacket();
            CL3115_RequestSetStdMeterScreenReplayPacket recv = new CL3115_RequestSetStdMeterScreenReplayPacket();
            FrameAry = new string[1];
            rc.SetPara(formType);
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }


        }
        /// <summary>
        /// 5.1.16	设置电能误差检定参数（CL1115副表检定控制前）
        /// </summary>
        /// <param name="pulseNum">校验圈数</param>
        /// <param name="testConst">被检表常数</param>
        /// <returns></returns>
        public int SetCalcParams(int pulseNum, int testConst, out string[] FrameAry)
        {
            CL3115_RequestSetStdMeterCalcParamsPacket rc = new CL3115_RequestSetStdMeterCalcParamsPacket();
            CL3115_RequestSetStdMeterCalcParamsReplayPacket recv = new CL3115_RequestSetStdMeterCalcParamsReplayPacket();
            rc.SetPara(pulseNum, testConst);

            FrameAry = new string[1];

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }

                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }



        }
        /// <summary>
        /// 读取电能误差（仅CL1115主副表版本）
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public int ReadError(out float error, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterErrorPacket rc = new CL3115_RequestReadStdMeterErrorPacket();
            CL3115_RequestReadStdMeterErrorReplayPacket recv = new CL3115_RequestReadStdMeterErrorReplayPacket();
            error = -1f;
            FrameAry = new string[1];
            try
            {
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            error = recv.fError;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }



        }
        /// <summary>
        /// 读取最近一次电能误差及误差计算次数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public int ReadLastError(out int num, out float error, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterLastErrorPacket rc = new CL3115_RequestReadStdMeterLastErrorPacket();
            CL3115_RequestReadStdMeterLastErrorReplayPacket recv = new CL3115_RequestReadStdMeterLastErrorReplayPacket();
            num = -1;
            error = -1f;
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                    {
                        if (recv.ReciveResult == RecvResult.OK)
                        {
                            num = recv.iNumber;
                            error = recv.fError;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    return 1;
                }
                else
                {
                    return 0;
                }


            }
            catch (Exception)
            {

                return -1;
            }



        }
        /// <summary>
        /// 重置走字
        /// </summary>
        /// <returns></returns>
        public int ResetRegister()
        {
            return 0;
        }
        #endregion

        #region IClass_Interface 成员


        public int SetSendFlag(bool Flag)
        {
            this.sendFlag = Flag;
            return 0;
        }
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName">对应方法名称</param>
        /// <param name="ReFrameAry">下行报文</param>
        /// <param name="ReAry">解析相应的数据</param>
        /// <returns></returns>
        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            MothedName = MothedName.Replace(" ", "").ToUpper();
            int iRsValue = 0;
            ReAry = new string[1];
            switch (MothedName)
            {
                case "CONNECT":
                    {
                        //连机
                        try
                        {
                            CL3115_RequestLinkReplyPacket recv = new CL3115_RequestLinkReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "DISCONNECT":
                    {
                        //断开连机 int DisConnect(out string[] FrameAry);
                        try
                        {
                            CL3115_RequestLinkReplyPacket recv = new CL3115_RequestLinkReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                        }
                        catch (Exception)
                        {
                            return -1;
                        }

                    }
                    break;
                case "READINSTMETRICALL":
                    {
                        //读实时测量数据 int ReadInstMetricAll(out float[] instValue, out string[] FrameAry);
                        try
                        {
                            CL3115_RequestReadStdInfoReplayPacket rcback = new CL3115_RequestReadStdInfoReplayPacket();
                            rcback.ParsePacket(ReFrameAry);
                            if (rcback.ReciveResult == RecvResult.OK)
                            {
                                CLOU.Struct.stStdInfo stdInfo = rcback.PowerInfo;
                                ReAry = new string[40];
                                //电压
                                ReAry[0] = stdInfo.Ua.ToString();
                                ReAry[1] = stdInfo.Ub.ToString();
                                ReAry[2] = stdInfo.Uc.ToString();
                                //电流
                                ReAry[3] = stdInfo.Ia.ToString();
                                ReAry[4] = stdInfo.Ib.ToString();
                                ReAry[5] = stdInfo.Ic.ToString();
                                //电压相位
                                ReAry[6] = stdInfo.Phi_Ua.ToString();
                                ReAry[7] = stdInfo.Phi_Ub.ToString();
                                ReAry[8] = stdInfo.Phi_Uc.ToString();
                                //电流相位
                                ReAry[9] = stdInfo.Phi_Ia.ToString();
                                ReAry[10] = stdInfo.Phi_Ib.ToString();
                                ReAry[11] = stdInfo.Phi_Ic.ToString();
                                //相角
                                ReAry[12] = stdInfo.PhiAngle_A.ToString();
                                ReAry[13] = stdInfo.PhiAngle_B.ToString();
                                ReAry[14] = stdInfo.PhiAngle_C.ToString();
                                //功率相角
                                ReAry[15] = stdInfo.SAngle.ToString();
                                //有功功率
                                ReAry[16] = stdInfo.Pa.ToString();
                                ReAry[17] = stdInfo.Pb.ToString();
                                ReAry[18] = stdInfo.Pc.ToString();
                                ReAry[19] = stdInfo.P.ToString();
                                //无功功率
                                ReAry[20] = stdInfo.Qa.ToString();
                                ReAry[21] = stdInfo.Qb.ToString();
                                ReAry[22] = stdInfo.Qc.ToString();
                                ReAry[23] = stdInfo.Q.ToString();
                                //视在功率
                                ReAry[24] = stdInfo.Sa.ToString();
                                ReAry[25] = stdInfo.Sb.ToString();
                                ReAry[26] = stdInfo.Sc.ToString();
                                ReAry[27] = stdInfo.S.ToString();
                                //功率因数
                                ReAry[28] = stdInfo.PowerFactor_A.ToString();
                                ReAry[29] = stdInfo.PowerFactor_B.ToString();
                                ReAry[30] = stdInfo.PowerFactor_C.ToString();
                                //总有功功率因数
                                ReAry[31] = stdInfo.COS.ToString();
                                //总无功功率因数
                                ReAry[32] = stdInfo.SIN.ToString();
                                //频率
                                ReAry[33] = stdInfo.Freq.ToString();
                                //ABC电压档位
                                ReAry[34] = stdInfo.Scale_Ua.ToString();
                                ReAry[35] = stdInfo.Scale_Ub.ToString();
                                ReAry[36] = stdInfo.Scale_Uc.ToString();
                                //ABC电流档位
                                ReAry[37] = stdInfo.Scale_Ia.ToString();
                                ReAry[38] = stdInfo.Scale_Ib.ToString();
                                ReAry[39] = stdInfo.Scale_Ic.ToString();
                            }
                            else
                            {
                                return 1006;
                            }

                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "READSTDPULSECONST":
                    {
                        //读标准表常数 int ReadStdPulseConst(out int pulseConst,out string[] FrameAry);
                        try
                        {
                            CL3115_RequestReadStdMeterConstReplayPacket recv = new CL3115_RequestReadStdMeterConstReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.meterConst.ToString();
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                case "READENERGY":
                    {
                        //读取电能量       int ReadEnergy(out float energy, out string[] FrameAry);
                        CL3115_RequestReadStdMeterTotalNumReplayPacket recv = new CL3115_RequestReadStdMeterTotalNumReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.MeterTotalNum.ToString();
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "READTOTALPULSES":
                    {
                        //读电能量累计脉冲数 int ReadTotalPulses(out long pulses, out string[] FrameAry);
                        CL3115_RequestReadStdMeterTotalPulseNumReplayPacket resv = new CL3115_RequestReadStdMeterTotalPulseNumReplayPacket();
                        try
                        {
                            resv.ParsePacket(ReFrameAry);
                            if (resv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = resv.Pulsenum.ToString();
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "READTESTENERGY":
                    {
                        //读电能走字数据 int ReadTestEnergy(out float testEnergy, out string[] FrameAry);
                        CL3115_RequestReadStdMeterZZDataReplayPacket resv = new CL3115_RequestReadStdMeterZZDataReplayPacket();
                        try
                        {
                            resv.ParsePacket(ReFrameAry);

                            if (resv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = resv.meterPulseNum.ToString();
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "READVERSION":
                    {
                        //读仪器版本号 int ReadVersion(out string version, out string[] FrameAry);
                        return 3;
                    }
                case "READHARMONICARRY":
                    {
                        //读各项电压电流谐波幅值 int ReadHarmonicArry(int phase, out float[] harmonicArry, out string[] FrameAry);
                        CL3115_RequestReadStdMeterHarmonicArryReplayPacket recv = new CL3115_RequestReadStdMeterHarmonicArryReplayPacket();

                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                if (recv.fHarmonicArryData.Length > 0)
                                {
                                    ReAry = new string[recv.fHarmonicArryData.Length];
                                    for (int i = 0; i < recv.fHarmonicArryData.Length; i++)
                                    {
                                        ReAry[i] = recv.fHarmonicArryData[i].ToString();
                                    }
                                }
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "READWAVEFORMARRY":
                    {
                        //读各项电压电流波形数据 int ReadWaveformArry(int phase, out float[] waveformArry, out string[] FrameAry);
                        CL3115_RequestReadStdMeterWaveformArryReplayPacket recv = new CL3115_RequestReadStdMeterWaveformArryReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[recv.fWaveformData.Length];
                                for (int j = 0; j < recv.fWaveformData.Length; j++)
                                {
                                    ReAry[j] = recv.fWaveformData[j].ToString();
                                }
                            }
                            else
                            {
                                return 1006;//返回数据格式错误
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SETWIRINGMODE":
                    {
                        //设置接线方式 int SetWiringMode(int wiringMode, out string[] FrameAry);
                        CL3115_RequestSetStdMeterLinkTypeReplayPacket recv = new CL3115_RequestSetStdMeterLinkTypeReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "SETSTDPULSECONST":
                    {
                        //设置标准表常数 int SetStdPulseConst(int pulseConst, out string[] FrameAry);
                        CL3115_RequestSetStdMeterConstReplayPacket recv = new CL3115_RequestSetStdMeterConstReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "SETPOWERMODE":
                    {
                        //设置电能指示int SetPowerMode(int powerMode, out string[] FrameAry);
                        CL3115_RequestSetStdMeterUsE1typeReplayPacket recv = new CL3115_RequestSetStdMeterUsE1typeReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }

                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                case "SETERRCALCTYPE":
                    {
                        //设置电能误差计算启动开关        int SetErrCalcType(int calcType, out string[] FrameAry);
                        CL3115_RequestStartTaskReplyPacket recv = new CL3115_RequestStartTaskReplyPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }

                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "SETSTDPARAMS":
                    {
                        //设置电能参数 int SetStdParams(int wiringMode, int powerMode, int calcType, out string[] FrameAry);
                        CL3115_RequestSetParaReplayPacket recv = new CL3115_RequestSetParaReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }

                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "SETRANGE":
                    {
                        //设置档位        int SetRange(int UaRange, int UbRange, int UcRange, int IaRange, int IbRange, int IcRange, out string[] FrameAry);
                        CL3115_RequestSetStdMeterDangWeiReplayPacket recv = new CL3115_RequestSetStdMeterDangWeiReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }

                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                case "SETDISPLAYFORM":
                    {
                        //设置显示界面        int SetDisplayForm(int formType, out string[] FrameAry);
                        CL3115_RequestSetStdMeterScreenReplayPacket recv = new CL3115_RequestSetStdMeterScreenReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "SETCALCPARAMS":
                    {
                        //设置电能误差检定参数 int SetCalcParams(int pulseNum, int testConst, out string[] FrameAry);
                        CL3115_RequestSetStdMeterCalcParamsReplayPacket recv = new CL3115_RequestSetStdMeterCalcParamsReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "READERROR":
                    {
                        //读取电能误差 int ReadError(out float error, out string[] FrameAry);
                        CL3115_RequestReadStdMeterErrorReplayPacket recv = new CL3115_RequestReadStdMeterErrorReplayPacket();
                        try
                        {
                            recv.ParsePacket(ReFrameAry);
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry[0] = recv.fError.ToString();
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                case "READLASTERROR":
                    {
                        //读取最近一次误差及误差次数  int ReadLastError(out int num, out float error, out string[] FrameAry);
                        CL3115_RequestReadStdMeterLastErrorReplayPacket recv = new CL3115_RequestReadStdMeterLastErrorReplayPacket();

                        try
                        {
                            if (recv.ReciveResult == RecvResult.OK)
                            {
                                ReAry = new string[2];
                                ReAry[0] = recv.iNumber.ToString();
                                ReAry[1] = recv.fError.ToString();
                                return 0;
                            }
                            else
                            {
                                return 1006;
                            }
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                default:
                    break;

            }

            return iRsValue;

        }
        #region 设置电压电流幅值和相位 @C_B
        /// <summary>
        /// 设置电压电流幅值和相位 @C_B
        /// </summary>
        /// <param name="c_u_v">C相电压幅值</param>
        /// <param name="b_u_v">B相电压幅值</param>
        /// <param name="a_u_v">A相电压幅值</param>
        /// <param name="c_i_v">C相电流幅值</param>
        /// <param name="b_i_v">B相电流幅值</param>
        /// <param name="a_i_v">A相电流幅值</param>
        /// <param name="c_u_p">C相电压相位</param>
        /// <param name="b_u_p">B相电压相位</param>
        /// <param name="a_u_p">A相电压相位</param>
        /// <param name="c_i_p">C相电流相位</param>
        /// <param name="b_i_p">B相电流相位</param>
        /// <param name="a_i_p">A相电流相位</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetAmplitudeAndPhase(float c_u_v, float b_u_v, float a_u_v, float c_i_v, float b_i_v, float a_i_v, float c_u_p, float b_u_p, float a_u_p, float c_i_p, float b_i_p, float a_i_p, out string[] FrameAry)
        {
            CL3115_ReplyAmplitudeAndPhasePacket rc = new CL3115_ReplyAmplitudeAndPhasePacket(GetLong(c_u_v, 5), GetLong(b_u_v, 5), GetLong(a_u_v, 5), GetLong(c_i_v, 7), GetLong(b_i_v, 7), GetLong(a_i_v, 7), GetLong(c_u_p, 4), GetLong(b_u_p, 4), GetLong(a_u_p, 4), GetLong(c_i_p, 4), GetLong(b_i_p, 4), GetLong(a_i_p, 4), 0xFB, 0xF9);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                return 0;
                //if (sendFlag)
                //{
                //    if (SendPacketWithRetry(m_MeterStd, rc, recv))
                //    {
                //        bool linkOk = recv.ReciveResult == RecvResult.OK;
                //        return linkOk ? 0 : 1;
                //    }
                //    else
                //    {
                //        return 1;
                //    }
                //}
                //else
                //{
                //    return 0;
                //}
            }
            catch (Exception)
            {

                return -1;
            }
        }
        private long GetLong(float value, int e)
        {
            double tmp = value * Math.Pow(10, e);
            long num = Convert.ToInt64(tmp);
            return num;
        }
        #endregion
        #region 设置通讯地址 @C_B
        /// <summary>
        /// 设置通讯地址 @C_B
        /// </summary>
        /// <param name="newsletterAddress"></param>
        /// <returns></returns>
        public int SetDeviceAddress(int deviceAddress)
        {
            //if (deviceAddress < 1 || deviceAddress > 254) deviceAddress = 0;
            //CL3115SendPacket cL3115SendPackets = new CL3115SendPacket();
            //byte toID = Convert.ToByte(deviceAddress);
            //cL3115SendPackets.SetDeviceAddress(toID);
            return 0;
        }
        #endregion
        #region 读取走字数据 @C_B
        /// <summary>
        /// 读取走字数据 @C_B
        /// </summary>
        /// <param name="totalEnergy"></param>
        /// <param name="pulseNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadTestRegister(out float totalEnergy, out long pulseNum, out string[] FrameAry)
        {
            CL3115_RequestReadStdMeterZZDataPacket rc = new CL3115_RequestReadStdMeterZZDataPacket();
            CL3115_RequestReadStdMeterZZDataReplayPacket resv = new CL3115_RequestReadStdMeterZZDataReplayPacket();
            totalEnergy = 0.0F;
            pulseNum = 0;
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, resv))
                    {
                        if (resv.ReciveResult == RecvResult.OK)
                        {
                            totalEnergy = resv.meterTotalNum;
                            pulseNum = resv.meterPulseNum;
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }
        #endregion
        #region 设置记录打印 @C_B
        public int SetAutoRecord(string deviceID, bool bStart)
        {
            return DataLoger.Set(deviceID, bStart);
        }
        #endregion
        #region 解析报文 @C_B
        public int AnalyzeFrames(string[] frames, out string[] frameValue)
        {
            return AnalyzeFrame.AnalyzeFrames(frames, out frameValue);
        }
        #endregion
        #region 设置校正值 @C_B
        public int SetCorrectionValue(int amplitudeOrAngle, float referenceValue, float cl3115Value, out string[] FrameAry)
        {
            CL3115_RequestSetCorrectionValuePacket rc = new CL3115_RequestSetCorrectionValuePacket();
            CL3115_RequestSetCorrectionValueReplyPacket resv = new CL3115_RequestSetCorrectionValueReplyPacket();
            rc.SetPara(amplitudeOrAngle, referenceValue, cl3115Value);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, resv))
                    {
                        if (resv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }
        #endregion
        #region 设置校正标识 @C_B
        public int SetPhaseInformation(int phase, int amplitudeOrAngle, out string[] FrameAry)
        {
            CL3115_RequestSetPhaseInformationPacket rc = new CL3115_RequestSetPhaseInformationPacket();
            CL3115_RequestSetPhaseInformationReplyPacket resv = new CL3115_RequestSetPhaseInformationReplyPacket();
            rc.SetPara(phase, amplitudeOrAngle);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_MeterStd, rc, resv))
                    {
                        if (resv.ReciveResult == RecvResult.OK)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1006;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }
        #endregion
        #region 在线监测 @C_B
        private readonly bool IsConnect = false;
        private readonly Thread onlineMonitoringThread = null;
        public int ReadOnline(out bool isConnect)
        {
            isConnect = this.IsConnect;
            return 0;
        }
        private void StartOnlineMonitoring()
        {
            //if (onlineMonitoringThread == null || onlineMonitoringThread.ThreadState == System.Threading.ThreadState.Aborted)
            //{
            //    CL3115_RequestLinkPacket rc = new CL3115_RequestLinkPacket();
            //    CL3115_RequestLinkReplyPacket recv = new CL3115_RequestLinkReplyPacket();
            //    onlineMonitoringThread = new System.Threading.Thread(() =>
            //    {
            //        while (true)
            //        {
            //            try
            //            {
            //                if (sendFlag)
            //                {
            //                    if (SendPacketWithRetry(m_MeterStd, rc, recv))
            //                    {
            //                        if (recv.ReciveResult == RecvResult.OK)
            //                        {
            //                            IsConnect = true;
            //                        }
            //                        else
            //                        {
            //                            IsConnect = false;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        IsConnect = false;
            //                    }
            //                }
            //            }
            //            catch
            //            {
            //                IsConnect = false;
            //            }
            //            System.Threading.Thread.Sleep(2000);
            //        }
            //    });
            //    onlineMonitoringThread.Start();
            //}
        }
        private void EndOnlineMonitoring()
        {
            if (onlineMonitoringThread != null) onlineMonitoringThread.Abort();
        }
        #endregion
        #endregion

        #region IClass_Events 成员
        public delegate void MsgCallBackDelegate(string logFrameInfo);
        public event MsgCallBackDelegate RecordMsgCallBack
        {
            add
            {
                if (RecordMsgCallBackEvent != null)
                {
                    RecordMsgCallBackEvent -= value;
                }

                RecordMsgCallBackEvent += value;
            }
            remove { RecordMsgCallBackEvent -= value; }
        }
        private static event MsgCallBackDelegate RecordMsgCallBackEvent;
        public static void RecordMsg(LogFrameInfo frameInfo)
        {
            if (RecordMsgCallBackEvent != null)
            {

                string frameString = string.Format("{0}^{1}^{2}^{3}^{4}^{5}^{6}^{7}^{8}^{9}^{10}", frameInfo.strPortNo, frameInfo.strEquipName, frameInfo.strItemName, frameInfo.strMessage, frameInfo.sendFrm.getStrFrame, frameInfo.sendFrm.FrameMeaning, frameInfo.sendFrm.FrameTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), frameInfo.ResvFrm.getStrFrame, frameInfo.ResvFrm.FrameMeaning, frameInfo.ResvFrm.FrameTime > DateTime.MinValue ? frameInfo.ResvFrm.FrameTime.ToString("yyyy-MM-dd HH:mm:ss.fff") : "", frameInfo.strOther);
                ThreadPool.QueueUserWorkItem((obj) => RecordMsgCallBackEvent(frameString));
            }
        }
        #endregion

        #region
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="stPort">端口号</param>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {
            for (int i = 0; i < RETRYTIEMS; i++)
            {
                if (driverBase.SendData(stPort, sp, rp) == true)
                {
                    return true;
                }
                if (RETRYTIEMS > 1) Thread.Sleep(300);
            }
            return false;
        }
        /// <summary>
        /// 字节数组转字符串
        /// </summary>
        /// <param name="bytesData"></param>
        /// <returns></returns>
        private string BytesToString(byte[] bytesData)
        {
            string strRevalue = string.Empty;
            if (bytesData == null || bytesData.Length < 1)
                return strRevalue;

            strRevalue = BitConverter.ToString(bytesData).Replace("-", "");

            return strRevalue;
        }
        #endregion

        #region private
        ///// <summary>
        ///// 第一维为电压电流，第二维为常数
        ///// </summary>
        //private readonly Dictionary<string, int> dicStdConstSheet = new Dictionary<string, int>();
        #region 3115
        /// <summary>
        /// 第一维为电压电流，第二维为常数
        /// </summary>
        private static readonly Dictionary<string, int> dicStd3115ConstSheet = new Dictionary<string, int>();
        private static readonly Cus_StdMeterURange[] arr3115U = new Cus_StdMeterURange[4] { Cus_StdMeterURange.档位60V,
            Cus_StdMeterURange.档位120V,
            Cus_StdMeterURange.档位240V,
            Cus_StdMeterURange.档位480V };

        private static readonly Cus_StdMeterIRange[] arr3115I = new Cus_StdMeterIRange[13] { Cus_StdMeterIRange.档位001A,
            Cus_StdMeterIRange.档位002A,
            Cus_StdMeterIRange.档位005A,
            Cus_StdMeterIRange.档位01A,
            Cus_StdMeterIRange.档位02A,
            Cus_StdMeterIRange.档位05A,
            Cus_StdMeterIRange.档位1A,
            Cus_StdMeterIRange.档位2A,
            Cus_StdMeterIRange.档位5A,
            Cus_StdMeterIRange.档位10A,
            Cus_StdMeterIRange.档位20A,
            Cus_StdMeterIRange.档位50A,
            Cus_StdMeterIRange.档位100A };
        #endregion
        /// <summary>
        /// 查表3115
        /// </summary>
        /// <param name="sngU"></param>
        /// <param name="sngI"></param>
        /// <returns></returns>
        private static int SearchStdMeterConst(Cus_StdMeterURange sngU, Cus_StdMeterIRange sngI)
        {
            string strKey = string.Format("{0}{1}", sngU, sngI);
            int meterconst;
            if (dicStd3115ConstSheet.ContainsKey(strKey))
            {
                meterconst = dicStd3115ConstSheet[strKey];
            }
            else
            {
                throw new Exception("the key is not found");
            }

            return meterconst;
        }
        private static void IntStdMeterConst()
        {
            #region 3115
            int[] consts3115 = new int[] { 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 16 * 100000000, 64 * 10000000, 32 * 10000000, 16 * 10000000, 64 * 1000000, 32 * 1000000 ,
            2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 16 * 100000000, 8 * 100000000, 32 * 10000000, 16 * 10000000, 8 * 10000000, 32 * 1000000, 16 * 1000000 ,
            2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 16 * 100000000, 8 * 100000000, 4 * 100000000, 16 * 10000000, 8 * 10000000, 4 * 10000000, 16 * 1000000, 8 * 1000000 ,
            2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 2 * 1000000000, 8 * 100000000, 4 * 100000000, 2 * 100000000, 8 * 10000000, 4 * 10000000, 2 * 10000000, 8 * 1000000, 4 * 1000000 ,};
            for (int i = 0; i < arr3115U.Length; i++)
            {
                for (int j = 0; j < arr3115I.Length; j++)
                {
                    string strKey3115 = string.Format("{0}{1}", arr3115U[i], arr3115I[j]);
                    if (!dicStd3115ConstSheet.ContainsKey(strKey3115)) dicStd3115ConstSheet.Add(strKey3115, consts3115[i * 13 + j]);
                }
            }
            #endregion
        }
        private static Cus_StdMeterURange FetUSwByU(float sng_xUb_A)
        {
            Cus_StdMeterURange USw;
            if (sng_xUb_A <= 66)
            {
                USw = Cus_StdMeterURange.档位60V;
            }
            else if (sng_xUb_A <= 132)
            {
                USw = Cus_StdMeterURange.档位120V;
            }
            else if (sng_xUb_A <= 264)
            {
                USw = Cus_StdMeterURange.档位240V;
            }
            else if (sng_xUb_A <= 528)
            {
                USw = Cus_StdMeterURange.档位480V;
            }
            else
            {
                USw = Cus_StdMeterURange.档位480V;
            }
            return USw;
        }
        private static Cus_StdMeterIRange FetISwByI(float sng_xUb_A)
        {
            Cus_StdMeterIRange USw;
            if (sng_xUb_A <= 0.011)
            {
                USw = Cus_StdMeterIRange.档位01A;//01A
            }
            else if (sng_xUb_A <= 0.022)
            {
                USw = Cus_StdMeterIRange.档位01A;//02
            }
            else if (sng_xUb_A <= 0.055)
            {
                USw = Cus_StdMeterIRange.档位01A;//05
            }
            else if (sng_xUb_A <= 0.11)
            {
                USw = Cus_StdMeterIRange.档位01A;
            }
            else if (sng_xUb_A <= 0.22)
            {
                USw = Cus_StdMeterIRange.档位02A;
            }
            else if (sng_xUb_A <= 0.55)
            {
                USw = Cus_StdMeterIRange.档位05A;

            }
            else if (sng_xUb_A <= 1.1)
            {
                USw = Cus_StdMeterIRange.档位1A;
            }
            else if (sng_xUb_A <= 2.2)
            {
                USw = Cus_StdMeterIRange.档位2A;
            }
            else if (sng_xUb_A <= 5.5)
            {
                USw = Cus_StdMeterIRange.档位5A;
            }
            else if (sng_xUb_A <= 11)
            {
                USw = Cus_StdMeterIRange.档位10A;
            }
            else if (sng_xUb_A <= 22)
            {
                USw = Cus_StdMeterIRange.档位20A;
            }
            else if (sng_xUb_A <= 55)
            {
                USw = Cus_StdMeterIRange.档位50A;
            }
            else
            {
                USw = Cus_StdMeterIRange.档位100A;
            }
            return USw;
        }

        #endregion
    }
}
