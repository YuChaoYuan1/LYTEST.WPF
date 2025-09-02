using E_ZH3001;
using System;
using System.Text;
using System.Threading;
using ZH.Enum;
using ZH.SocketModule.Packet;
using ZH.Struct;

namespace ZH
{
    public class ZH3001
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private readonly StPortInfo m_PowerSourcePort = null;

        private readonly DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZH3001()
        {
            m_PowerSourcePort = new StPortInfo();
            driverBase = new DriverBase();
        }

        #region IClass_Interface 成员
        /// <summary>
        /// 初始化Scoket端口
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">帧命令最长等待时间</param>
        /// <param name="WaitSencondsPerByte">帧字节等待时间</param>
        /// <param name="IP">Ip地址</param>
        /// <param name="RemotePort">远程端口</param>
        /// <param name="LocalStartPort">本地端口</param>
        /// <returns></returns>
        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort)
        {
            m_PowerSourcePort.m_Exist = 1;
            m_PowerSourcePort.m_IP = IP;
            m_PowerSourcePort.m_Port = ComNumber;
            m_PowerSourcePort.m_Port_IsUDPorCom = true;
            m_PowerSourcePort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_PowerSourcePort.m_Port_Setting, m_PowerSourcePort.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {
                return 1;
            }

            return 0;
        }
        /// <summary>
        /// 初始化Com 口  只能实例化一次
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">最大等待时间</param>
        /// <param name="WaitSencondsPerByte">字节接受时间</param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_PowerSourcePort.m_Exist = 1;
            m_PowerSourcePort.m_IP = "";
            m_PowerSourcePort.m_Port = ComNumber;
            m_PowerSourcePort.m_Port_IsUDPorCom = false;
            m_PowerSourcePort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
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
            FrameAry = new string[1];
            ZH3001_RequestLinkPacket rc3 = new ZH3001_RequestLinkPacket();
            ZH3001_RequestLinkReplyPacket recv3 = new ZH3001_RequestLinkReplyPacket();
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool linkClockOk = recv3.ReciveResult == RecvResult.OK;
                        string Clockmessage = string.Format("源联机{0}。", linkClockOk ? "成功" : "失败");
                        return linkClockOk ? 0 : 1;
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
        /// 设置电压电流幅值
        /// </summary>
        /// <param name="Ua">A相电压</param>
        /// <param name="Ub">B相电压</param>
        /// <param name="Uc">C相电压</param>
        /// <param name="Ia">A相电压</param>
        /// <param name="Ib">B相电压</param>
        /// <param name="Ic">C相电压</param>        
        /// <param name="FrameAry">返回的下发报文</param>
        /// <returns></returns>
        public int PowerAmplitude(double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerAmplitudePacket rc3 = new ZH3001_RequestPowerAmplitudePacket();
            ZH3001_RequestPowerAmplitudeReplyPacket recv3 = new ZH3001_RequestPowerAmplitudeReplyPacket();
            rc3.SetPara(Ua, Ub, Uc, Ia, Ib, Ic);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 升源（单条指令实现）
        /// </summary>
        /// <param name="jxfs">接线方式</param>
        /// <param name="Ua">A相电压</param>
        /// <param name="Ub">B相电压</param>
        /// <param name="Uc">C相电压</param>
        /// <param name="Ia">A相电压</param>
        /// <param name="Ib">B相电压</param>
        /// <param name="Ic">C相电压</param>
        /// <param name="PhiUa">A相电压角度</param>
        /// <param name="PhiUb">B相电压角度</param>
        /// <param name="PhiUc">C相电压角度</param>
        /// <param name="PhiIa">A相电流角度</param>
        /// <param name="PhiIb">B相电流角度</param>
        /// <param name="PhiIc">C相电流角度</param>
        /// <param name="Freq">频率</param>
        /// <param name="on">升源标志</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int PowerOn(int jxfs, double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq, int Mode, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerOnPacket rc3 = new ZH3001_RequestPowerOnPacket();
            ZH3001_RequestPowerOnReplyPacket recv3 = new ZH3001_RequestPowerOnReplyPacket();
            rc3.SetPara(jxfs, Ua, Ub, Uc, Ia, Ib, Ic, PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Freq, Mode);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置负载电流快速变化实验的时间与启动标志 测试没问题问题
        /// </summary>
        /// <param name="Time1">升源时间</param>
        /// <param name="Time2">降源时间</param>
        /// <param name="Mode"> 启动标志：1个字节表示，bit0-A相，bit1-B相，bit2-C相，其他bit位无效</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int LoadCurrent(double Time1, double Time2, int Mode, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestLoadCurrentPacket rc3 = new ZH3001_RequestLoadCurrentPacket();
            ZH3001_RequestLoadCurrentReplyPacket recv3 = new ZH3001_RequestLoadCurrentReplyPacket();
            rc3.SetPara(Time1, Time2, Mode);
            //合成的报文
            try
            {
                //68 01 01 11 13 21 00 00 00 13 88 00 00 27 10 07 88       00 00 13 88 -50s   00 00 27 10-100s  07-111
                //68 01 01 11 13 21 00 00 00 13 88 00 00 27 10 07 88
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置负载电流快速变化实验的时间与启动标志  测试成功没问题
        /// </summary>
        /// <param name="TonTime">开启时间</param>
        /// <param name="ToffTime">关断时间</param>
        /// <param name="strA">标志A</param>
        /// <param name="strB">标志B</param>
        /// <param name="strC">标志C</param>
        /// <returns></returns>
        public int SetCurrentChangeByPower(int TonTime, int ToffTime, string strA, string strB, string strC, out string[] FrameAry)
        {
            ZH3001_RequestSetCurrentChangePacket rcPowerHarmonic = new ZH3001_RequestSetCurrentChangePacket();
            ZH3001_RequestSetCurrentChangeReplyPacket rcevPowerHarmonic = new ZH3001_RequestSetCurrentChangeReplyPacket();
            rcPowerHarmonic.SetPara(TonTime, ToffTime, strA + strB + strC);

            FrameAry = new string[1];
            //68 01 01 11 13 21 00 00 00 13 88 00 00 27 10 07 88       00 00 13 88 -50s   00 00 27 10-100s  07-111
            //68 01 01 11 13 21 00 00 00 13 88 00 00 27 10 07 88

            try
            {
                FrameAry[0] = BytesToString(rcPowerHarmonic.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rcPowerHarmonic, rcevPowerHarmonic))
                    {
                        bool reuslt = rcevPowerHarmonic.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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

        public int AC_VoltageSagSndInterruption(int TonTime, int ToffTime, int count, int proportion, string strA, string strB, string strC, out string[] FrameAry)
        {
            ZH3001_RequestAC_VoltageCurrentPacket rc3 = new ZH3001_RequestAC_VoltageCurrentPacket();
            ZH3001_RequestAC_VoltageCurrentReplyPacket rcev3 = new ZH3001_RequestAC_VoltageCurrentReplyPacket();
            rc3.SetPara(TonTime, ToffTime, count, proportion, strA + strB + strC);

            FrameAry = new string[1];
            //68 01 01 16 13 21 01 00 00 01 F4 00 00 03 E8 00 00 00 0A 14 07 04      00 00 01 F4 -5s  00 00 03 E8-10s  00 00 00 0A-20  14-10  07-111 
            //68 01 01 16 13 21 01 00 00 01 f4 00 00 03 e8 00 00 00 0a 14 07 22

            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, rcev3))
                    {
                        bool reuslt = rcev3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置相位频率
        /// </summary>
        /// <param name="PhiUa">A相电压角度</param>
        /// <param name="PhiUb">B相电压角度</param>
        /// <param name="PhiUc">C相电压角度</param>
        /// <param name="PhiIa">A相电流角度</param>
        /// <param name="PhiIb">B相电流角度</param>
        /// <param name="PhiIc">C相电流角度</param>
        /// <param name="Freq">频率</param>
        /// <param name="FrameAry">返回的下发报文</param>
        /// <returns></returns>
        public int PowerAngle(double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerAnglePacket rc3 = new ZH3001_RequestPowerAnglePacket();
            ZH3001_RequestPowerAngleReplyPacket recv3 = new ZH3001_RequestPowerAngleReplyPacket();
            rc3.SetPara(PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Freq);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置接线模式
        /// </summary>
        /// <param name="Mode">三相四线/单相： 1  三相三线： 2 </param>
        /// <param name="FrameAry">返回的下发报文</param>
        /// <returns></returns>
        public int PowerMode(int Mode, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerModePacket rc3 = new ZH3001_RequestPowerModePacket();
            ZH3001_RequestPowerModeReplyPacket recv3 = new ZH3001_RequestPowerModeReplyPacket();
            rc3.SetPara(Mode);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 更新全部状态寄存器 (Bit0：A相电压幅度 Bit1：A相电流幅度 Bit2：B相电压幅度 Bit3：B相电流幅度 Bit4：C相电压幅度 Bit5：C相电流幅度 Bit6：A相电压相位 Bit7：A相电流相位 Bit8：B相电压相位 Bit9：B相电流相位 Bit10：C相电压相位 Bit11：C相电流相位 Bit15：频率更新)
        /// </summary>
        /// <param name="Mode">两个字节，源更新状态 </param>
        /// <returns></returns>
        public int PowerUpdate(byte[] Mode, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerUpdatePacket rc3 = new ZH3001_RequestPowerUpdatePacket();
            ZH3001_RequestPowerUpdateReplyPacket recv3 = new ZH3001_RequestPowerUpdateReplyPacket();
            rc3.SetPara(Mode);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 启动或者关闭交流源  
        /// </summary>
        /// <param name="Mode">输出：1 关闭：0 </param>
        /// <returns></returns>
        public int PowerOn_Off(int Mode, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerOn_OffPacket rc3 = new ZH3001_RequestPowerOn_OffPacket();
            ZH3001_RequestPowerOn_OffReplyPacket recv3 = new ZH3001_RequestPowerOn_OffReplyPacket();
            rc3.SetPara(Convert.ToByte(Mode));
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置数字闭环状态
        /// </summary>
        /// <param name="PowerType">128表示禁止闭环，0表示允许闭环</param>
        /// <returns></returns>
        public int SetPowerLoopType(int PowerType, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestSetPowerLoopTypePacket rc3 = new ZH3001_RequestSetPowerLoopTypePacket();
            ZH3001_RequestSetPowerLoopTypeReplyPacket recv3 = new ZH3001_RequestSetPowerLoopTypeReplyPacket();
            rc3.SetPara(Convert.ToByte(PowerType));
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 手动挡与自动挡的切换
        /// </summary>
        /// <param name="Ugear">Bit7:电压手动挡   1 手动挡 0 自动挡 </param>
        /// <param name="Igear">Bit6:电流手动挡   1 手动挡 0 自动挡 </param>
        /// <returns></returns>
        public int SetPowerGear(string Ugear, string Igear, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestSetPowerGearPacket rc3 = new ZH3001_RequestSetPowerGearPacket();
            ZH3001_RequestSetPowerGearReplyPacket recv3 = new ZH3001_RequestSetPowerGearReplyPacket();
            rc3.SetPara(Ugear, Igear);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置 特殊谐波
        /// </summary>
        /// <param name="ua"> 谐波设置对象 Ua ；1-设置 ；0-不设置</param>
        /// <param name="ub">谐波设置对象 Ub ；1-设置 ；0-不设置</param>
        /// <param name="uc">谐波设置对象 Uc ；1-设置 ；0-不设置</param>
        /// <param name="ia">谐波设置对象 ia ；1-设置 ；0-不设置</param>
        /// <param name="ib">谐波设置对象 ib ；1-设置 ；0-不设置</param>
        /// <param name="ic">谐波设置对象 ic ；1-设置 ；0-不设置</param>
        /// <param name="HarmonicType"> 特殊谐波：0表示正常谐波；1表示方形波，2表示尖顶波，3表示次谐波，4表示奇次谐波，5表示偶次谐波。</param>
        /// <returns></returns>
        public int SetZH3001PowerHarmonic(string ua, string ub, string uc, string ia, string ib, string ic, int HarmonicType, out string[] FrameAry)
        {
            FrameAry = new string[1];
            byte byteData = Convert.ToByte(Convert.ToInt32(ic + ib + ia + uc + ub + ua, 2));
            ZH3001_RequestPowerHarmonicPacket rcPowerHarmonic = new ZH3001_RequestPowerHarmonicPacket();
            ZH3001_RequestPowerHarmonicReplyPacket rcevPowerHarmonic = new ZH3001_RequestPowerHarmonicReplyPacket();
            rcPowerHarmonic.SetPara(byteData, HarmonicType);
            try
            {
                FrameAry[0] = BytesToString(rcPowerHarmonic.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rcPowerHarmonic, rcevPowerHarmonic))
                    {
                        bool reuslt = rcevPowerHarmonic.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置常规谐波
        /// </summary>
        /// <param name="ua"> 谐波设置对象 Ua ；1-设置 ；0-不设置</param>
        /// <param name="ub">谐波设置对象 Ub ；1-设置 ；0-不设置</param>
        /// <param name="uc">谐波设置对象 Uc ；1-设置 ；0-不设置</param>
        /// <param name="ia">谐波设置对象 ia ；1-设置 ；0-不设置</param>
        /// <param name="ib">谐波设置对象 ib ；1-设置 ；0-不设置</param>
        /// <param name="ic">谐波设置对象 ic ；1-设置 ；0-不设置</param>
        /// <param name="HarmonicOpenOrClose">谐波开关：  0关闭， 1打开</param>
        /// <param name="HarmonicContent">谐波含量 2---61</param>
        /// <param name="HarmonicPhase">谐波相位 2---61</param>
        /// <returns></returns>
        public int SetZH3001PowerGetHarmonic(string ua, string ub, string uc, string ia, string ib, string ic, int HarmonicOpenOrClose, float[] HarmonicContent, float[] HarmonicPhase, out string[] FrameAry)
        {
            FrameAry = new string[1];
            byte HarmonicType = Convert.ToByte(Convert.ToInt32(ic + ib + ia + uc + ub + ua, 2));
            ZH3001_RequestPowerGetHarmonicPacket rcPowerHarmonic = new ZH3001_RequestPowerGetHarmonicPacket();
            ZH3001_RequestGetHarmonicReplyPacket rcevPowerHarmonic = new ZH3001_RequestGetHarmonicReplyPacket();
            rcPowerHarmonic.SetPara(HarmonicContent, HarmonicPhase, HarmonicType, HarmonicOpenOrClose);
            try
            {

                FrameAry[0] = BytesToString(rcPowerHarmonic.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rcPowerHarmonic, rcevPowerHarmonic))
                    {
                        bool reuslt = rcevPowerHarmonic.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 交流源 进入校准模式
        /// </summary>
        /// <param name="CalibrationPower">0x81表示缺省，0x80表示进入校准模式，0x00退出校准模式</param>
        /// <returns></returns>
        public int CalibrationPower(byte CalibrationPower, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestCalibrationPowerPacket rc3 = new ZH3001_RequestCalibrationPowerPacket();
            ZH3001_RequestCalibrationPowerReplyPacket recv3 = new ZH3001_RequestCalibrationPowerReplyPacket();
            rc3.SetPara(CalibrationPower);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 启动校准
        /// </summary>
        /// <returns></returns>
        public int StartCalibrationPower(out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestStartCalibrationPowerPacket rc3 = new ZH3001_RequestStartCalibrationPowerPacket();
            ZH3001_RequestStartCalibrationPowerReplyPacket recv3 = new ZH3001_RequestStartCalibrationPowerReplyPacket();
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 校准标准幅值下发
        /// </summary>
        /// <param name="Ua">A相电压</param>
        /// <param name="Ub">B相电压</param>
        /// <param name="Uc">C相电压</param>
        /// <param name="Ia">A相电压</param>
        /// <param name="Ib">B相电压</param>
        /// <param name="Ic">C相电压</param>
        /// <returns></returns>
        public int CalibrationPowerAmplitude(double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestCalibrationPowerAmplitudePacket rc3 = new ZH3001_RequestCalibrationPowerAmplitudePacket();
            ZH3001_RequestCalibrationPowerAmplitudeReplyPacket recv3 = new ZH3001_RequestCalibrationPowerAmplitudeReplyPacket();
            rc3.SetPara(Ua, Ub, Uc, Ia, Ib, Ic);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 校准相位 下发
        /// </summary>
        /// <param name="PhiUa">A相电压角度</param>
        /// <param name="PhiUb">B相电压角度</param>
        /// <param name="PhiUc">C相电压角度</param>
        /// <param name="PhiIa">A相电流角度</param>
        /// <param name="PhiIb">B相电流角度</param>
        /// <param name="PhiIc">C相电流角度</param>
        /// <returns></returns>
        public int CalibrationPowerAngle(double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestCalibrationPowerAnglePacket rc3 = new ZH3001_RequestCalibrationPowerAnglePacket();
            ZH3001_RequestCalibrationPowerAngleReplyPacket recv3 = new ZH3001_RequestCalibrationPowerAngleReplyPacket();
            rc3.SetPara(PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 本型号源用不到
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ClearOL(out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 1;
        }

        /// <summary>
        /// 设置发送标志位
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public int SetSendFlag(bool Flag)
        {
            sendFlag = Flag;
            return 0;
        }
        /// <summary>
        /// 解析设备返回的的报文
        /// </summary>
        /// <param name="MothedName">方法名称</param>
        /// <param name="ReFrameAry">设备返回报文</param>
        /// <param name="ReAry">解析的数据</param>
        /// <returns></returns>
        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            int iRevalue = 0;
            ReAry = new string[1];

            switch (MothedName)
            {
                case "Connect":
                    try
                    {
                        ZH3001_RequestLinkReplyPacket recv = new ZH3001_RequestLinkReplyPacket();
                        recv.ParsePacket(ReFrameAry);
                        ReAry[0] = recv.ReciveResult.ToString();
                    }
                    catch (Exception)
                    {

                        return -1;
                    }

                    break;
                case "DisConnect":
                    ReAry[0] = string.Empty;
                    break;
                case "ReadGPSTime":
                    return -1;
                default:
                    ReAry[0] = "Null this Data";
                    break;
            }
            return iRevalue;
        }


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
                Thread.Sleep(300);
            }
            return false;
        }
        /// <summary>
        /// bytes转 string
        /// </summary>
        /// <param name="bytesData"></param>
        /// <returns></returns>
        private string BytesToString(byte[] bytesData)
        {
            string strRevalue = string.Empty;
            if (bytesData == null || bytesData.Length < 1)
                return strRevalue;

            for (int i = 0; i < bytesData.Length; i++)
            {
                strRevalue += Convert.ToString(bytesData[i], 16);
            }
            return strRevalue;
        }

        /// <summary>
        /// 读取瞬时测量数据
        /// </summary>
        /// <param name="instValue"></param>
        /// <returns></returns>
        public int ReadInstMetricAll(out float[] instValue, out string[] FrameAry)
        {
            instValue = new float[34];
            FrameAry = new string[1];
            stStdInfo stdInfo;

            try
            {

                if (sendFlag)
                {
                    #region 读取幅度
                    ZH3001_RequestReadAmplitudePacket rcReadAmplitude = new ZH3001_RequestReadAmplitudePacket();
                    ZH3001_RequestReadAmplitudeReplayPacket rcbackReadAmplitude = new ZH3001_RequestReadAmplitudeReplayPacket();
                    FrameAry[0] = BytesToString(rcReadAmplitude.GetPacketData());
                    if (SendPacketWithRetry(m_PowerSourcePort, rcReadAmplitude, rcbackReadAmplitude))
                    {

                        stdInfo = rcbackReadAmplitude.PowerInfo;
                        ////电压
                        instValue[0] = float.Parse(stdInfo.Ua.ToString("F1"));
                        instValue[1] = float.Parse(stdInfo.Ub.ToString("F1"));
                        instValue[2] = float.Parse(stdInfo.Uc.ToString("F1"));
                        //电流
                        instValue[3] = float.Parse(stdInfo.Ia.ToString("F2"));
                        instValue[4] = float.Parse(stdInfo.Ib.ToString("F2"));
                        instValue[5] = float.Parse(stdInfo.Ic.ToString("F2"));
                    }
                    else
                    {
                        return 1;
                    }
                    #endregion 读取幅度

                    #region 读取相频
                    ZH3001_RequestReadAnglePacket rcReadAngle = new ZH3001_RequestReadAnglePacket();
                    ZH3001_RequestReadAngleReplayPacket rcbackReadAngle = new ZH3001_RequestReadAngleReplayPacket();
                    FrameAry[0] = BytesToString(rcReadAmplitude.GetPacketData());
                    if (SendPacketWithRetry(m_PowerSourcePort, rcReadAngle, rcbackReadAngle))
                    {
                        stdInfo = rcbackReadAngle.PowerInfo;
                        //电压相位
                        instValue[6] = float.Parse(stdInfo.Phi_Ua.ToString("F1"));
                        instValue[7] = float.Parse(stdInfo.Phi_Ub.ToString("F1"));
                        instValue[8] = float.Parse(stdInfo.Phi_Uc.ToString("F1"));
                        //电流相位
                        instValue[9] = float.Parse(stdInfo.Phi_Ia.ToString("F1"));
                        instValue[10] = float.Parse(stdInfo.Phi_Ib.ToString("F1"));
                        instValue[11] = float.Parse(stdInfo.Phi_Ic.ToString("F1"));
                        //相角
                        instValue[12] = -1f;// stdInfo.PhiAngle_A;
                        instValue[13] = -1f;// stdInfo.PhiAngle_B;
                        instValue[14] = -1f;// stdInfo.PhiAngle_C;
                        //频率
                        instValue[33] = stdInfo.Freq;
                        //功率因数
                        instValue[28] = stdInfo.PowerFactor_A;//stdInfo.PowerFactor_A;
                        instValue[29] = stdInfo.PowerFactor_B;//stdInfo.PowerFactor_B;
                        instValue[30] = stdInfo.PowerFactor_C;//stdInfo.PowerFactor_C;
                    }
                    else
                    {
                        return 1;
                    }
                    #endregion 读取幅度

                    #region 读取功率
                    ZH3001_RequestReadPowerPacket rcReadPower = new ZH3001_RequestReadPowerPacket();
                    ZH3001_RequestReadPowerReplayPacket rcbackReadPower = new ZH3001_RequestReadPowerReplayPacket();
                    FrameAry[0] = BytesToString(rcReadAmplitude.GetPacketData());
                    if (SendPacketWithRetry(m_PowerSourcePort, rcReadPower, rcbackReadPower))
                    {
                        stdInfo = rcbackReadPower.PowerInfo;
                        instValue[15] = -1f; //stdInfo.SAngle;
                        //有功功率
                        instValue[16] = float.Parse(stdInfo.Pa.ToString("F1")) / 1000;
                        instValue[17] = float.Parse(stdInfo.Pb.ToString("F1")) / 1000;
                        instValue[18] = float.Parse(stdInfo.Pc.ToString("F1")) / 1000;
                        instValue[19] = float.Parse(stdInfo.P.ToString("F1")) / 1000;
                        //无功功率
                        instValue[20] = float.Parse(stdInfo.Qa.ToString("F1")) / 1000;
                        instValue[21] = float.Parse(stdInfo.Qb.ToString("F1")) / 1000;
                        instValue[22] = float.Parse(stdInfo.Qc.ToString("F1")) / 1000;
                        instValue[23] = float.Parse(stdInfo.Q.ToString("F1")) / 1000;
                        //视在功率
                        instValue[24] = float.Parse(stdInfo.Sa.ToString("F1")) / 1000;
                        instValue[25] = float.Parse(stdInfo.Sb.ToString("F1")) / 1000;
                        instValue[26] = float.Parse(stdInfo.Sc.ToString("F1")) / 1000; ;
                        instValue[27] = float.Parse(stdInfo.S.ToString("F1")) / 1000;

                        //总有功功率因数
                        instValue[31] = 1f;
                        //总无功功率因数
                        instValue[32] = 1f;
                    }
                    else
                    {
                        return 1;
                    }
                    #endregion 读取幅度

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
        /// 计算角度
        /// </summary>
        /// <param name="Glys">>功率因数如(0.5L,1.0,-1,-0,0.5C)</param>
        /// <param name="jxfs"> 0--三相四线有功表PT4; 1--三相三线有功表P32;  7--单相表 </param>
        /// <param name="cus_PowerYuanJiang">分合元。1=H，2=A，3=B,4=C</param>
        /// <param name="isNxx">逆向序。0=正相序 ，1=逆向序 2= 电压逆相序 3= 电流逆相序</param>
        /// <param name="phiPara">返回的电压电流角度，长度为6的double数组（UA，Ub，UC，Ia,Ib,Ic）</param>
        /// <returns></returns>
        public bool SetAcSourcePowerFactor(string Glys, int jxfs, int cus_PowerYuanJiang, int isNxx, out double[] phiPara)
        {
            phiPara = new double[6];
            bool PH = true;
            Cus_PowerYuanJiang yuanjian;
            Cus_PowerPhase _IsNXX;

            switch (cus_PowerYuanJiang)
            {
                case 1:
                    yuanjian = Cus_PowerYuanJiang.H;
                    break;
                case 2:
                    yuanjian = Cus_PowerYuanJiang.A;
                    break;
                case 3:
                    yuanjian = Cus_PowerYuanJiang.B;
                    break;
                case 4:
                    yuanjian = Cus_PowerYuanJiang.C;
                    break;
                default:
                    yuanjian = Cus_PowerYuanJiang.H;
                    break;


            }

            switch (isNxx)
            {
                case 0:
                    _IsNXX = Cus_PowerPhase.正相序;
                    break;
                case 1:
                    _IsNXX = Cus_PowerPhase.逆相序;
                    break;
                case 2:
                    _IsNXX = Cus_PowerPhase.电压逆相序;
                    break;
                case 3:
                    _IsNXX = Cus_PowerPhase.电流逆相序;
                    break;
                default:
                    _IsNXX = Cus_PowerPhase.正相序;
                    break;


            }

            PhiPara _PhiPara;
            //jxfs 0-三相四线有功表；1-三相三线有功表;2--三相四线真无功表(QT4);3--三相三线真无功表(Q32);
            //4--三元件跨相90无功表(Q33);5--二元件跨相90无功表(Q90);6--二元件人工中点(60)无功表(Q60);
            #region
            double XwUa = 0;
            double XwUb = 0;
            double XwUc = 0;
            double XwIa = 0;
            double XwIb = 0;
            double XwIc = 0;
            int n = 1;


            if (jxfs == 0)// 三相四线有功 = 0,
                jxfs = 0;
            else if (jxfs == 1)//三相四线无功 = 1,
                jxfs = 2;
            else if (jxfs == 2)//三相三线有功 = 2,
                jxfs = 1;
            else if (jxfs == 3)//三相三线无功 = 3,
                jxfs = 3;
            else if (jxfs == 4)//二元件跨相90 = 4,
                jxfs = 5;
            else if (jxfs == 5)//二元件跨相60 = 5,
                jxfs = 6;
            else if (jxfs == 6)//三元件跨相90 = 6,
                jxfs = 4;
            else if (jxfs == 7)//单相表
                jxfs = 7;


            string strGlys = Glys;
            if (Glys == "0") strGlys = "0L";
            if (Glys == "-0") strGlys = "0C";
            string LC = GetUnit(strGlys);
            double LcValue;
            if (LC.Length > 0)
            {
                LcValue = Convert.ToDouble(strGlys.Replace(LC, ""));
            }
            else
            {
                LcValue = Convert.ToDouble(strGlys);
            }

            double Phi;
            switch (jxfs)
            {
                case 0:  //三相四线有功表
                    #region
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
                    XwUa = 0;
                    XwUb = 240;
                    XwUc = 120;
                    if (LcValue > 0)
                    {
                        XwIa = 0;
                        XwIb = 240;
                        XwIc = 120;
                        Phi = 1 * Phi;
                    }
                    else if (LcValue < 0)
                    {
                        XwIa = 180;
                        XwIb = 60;
                        XwIc = 300;
                        Phi = -1 * Phi;
                    }
                    if (LC == "L") //感性
                    {
                        Phi = 1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;

                    }
                    if (LC == "C") //容性
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;

                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi;

                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                #endregion
                case 1:  //三相三线有功表
                    #region
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 90;
                    if (LcValue > 0)
                    {
                        if (yuanjian == Cus_PowerYuanJiang.H)
                        {
                            XwIa = 0;
                            XwIb = 0;
                            XwIc = 120;
                            Phi = 1 * Phi;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.A)
                        {
                            XwIa = 30;
                            XwIb = 0;
                            XwIc = 0;
                            Phi = 1 * Phi;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.C)
                        {
                            XwIa = 0;
                            XwIb = 0;
                            XwIc = 90;
                            Phi = 1 * Phi;
                        }
                    }
                    else if (LcValue < 0)
                    {
                        if (yuanjian == Cus_PowerYuanJiang.H)
                        {
                            XwIa = 180;
                            XwIb = 0;
                            XwIc = 300;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.A)
                        {
                            XwIa = 210;
                            XwIb = 0;
                            XwIc = 0;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.C)
                        {
                            XwIa = 0;
                            XwIb = 0;
                            XwIc = 270;
                        }
                        Phi = -1 * Phi;
                    }
                    if (LC == "L")
                    {
                        Phi = 1 * Phi;
                        XwIa -= Phi;
                        if (XwIa < 0) XwIa += (360);
                        if (XwIa >= 360) XwIa -= (360);
                        XwIb = 0;
                        if (XwIb < 0) XwIb += (360);
                        if (XwIb >= 360) XwIb -= (360);
                        XwIc -= Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa -= Phi;
                        if (XwIa < 0) XwIa += (360);
                        if (XwIa >= 360) XwIa -= (360);
                        XwIb = 0;
                        if (XwIb < 0) XwIb += (360);
                        if (XwIb >= 360) XwIb -= (360);
                        XwIc -= Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;

                    }
                    if (PH == false)
                    {
                        XwIa += 30;
                        XwIc += 30;
                    }
                    break;
                #endregion
                case 2: //三相四线真无功表(QT4)
                    #region
                    XwUa = 0;
                    XwUb = 240;
                    XwUc = 120;
                    if (LcValue > 0)
                    {
                        XwIa = 270 + 1;
                        XwIb = 150 + 1;
                        XwIc = 30 + 1;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90 + 1;
                        XwIb = 330 + 1;
                        XwIc = 210 + 1;
                        n = -1;
                    }
                    if (LC == "L")
                    {
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = XwUa - Phi + 1;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi + 1;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi + 1;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = XwUa - Phi + 1;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = XwUb - Phi + 1;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = XwUc - Phi + 1;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                #endregion
                case 3: //三相三线真无功表(Q32)
                    #region
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 90;
                    int p32tr = 2;
                    if (LcValue > 0)
                    {
                        if (yuanjian == Cus_PowerYuanJiang.H)
                        {
                            XwIa = 270 + p32tr;
                            XwIb = 0;
                            XwIc = 30 + p32tr;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.A)
                        {
                            XwIa = 300 + p32tr;
                            XwIb = 0;
                            XwIc = 0;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.C)
                        {
                            XwIa = 0;
                            XwIb = 0;
                            XwIc = 0;
                        }
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {
                        if (yuanjian == Cus_PowerYuanJiang.H)
                        {
                            XwIa = 90 + p32tr;
                            XwIb = 0;
                            XwIc = 210 + p32tr;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.A)
                        {
                            XwIa = 120 + p32tr;
                            XwIb = 0;
                            XwIc = 0;
                        }
                        else if (yuanjian == Cus_PowerYuanJiang.C)
                        {
                            XwIa = 0;
                            XwIb = 0;
                            XwIc = 180 + p32tr;
                        }
                        n = -1;
                    }

                    if (LC == "L")
                    {
                        switch (yuanjian)
                        {
                            case Cus_PowerYuanJiang.H:
                                if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = Phi;
                                XwIa = 0 - Phi + p32tr;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 120 - Phi + p32tr;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case Cus_PowerYuanJiang.A:
                                if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = Phi;
                                XwIa = 30 - Phi + p32tr;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 120 - Phi + p32tr;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case Cus_PowerYuanJiang.C:
                                if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = Phi;
                                XwIa = 0 - Phi + p32tr;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;


                                XwIc = 90 - Phi + p32tr;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                        }
                    }
                    if (LC == "C")
                    {
                        switch (yuanjian)
                        {
                            case Cus_PowerYuanJiang.H:
                                if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = n + Phi;
                                XwIa = 0 - Phi + p32tr;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;

                                XwIc = 120 - Phi + p32tr;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                            case Cus_PowerYuanJiang.A:
                                if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = n + Phi;
                                XwIa = 30 - Phi + p32tr;
                                if (XwIa < 0) XwIa += 360;
                                if (XwIa >= 360) XwIa -= 360;

                                XwIb = 0;
                                XwIc = 0;
                                break;
                            case Cus_PowerYuanJiang.C:
                                if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                                else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                                //Phi = n + Phi;
                                XwIa = 0;
                                XwIb = 0;
                                XwIc = 90 - Phi + p32tr;
                                if (XwIc < 0) XwIc += 360;
                                if (XwIc >= 360) XwIc -= 360;
                                break;
                        }
                    }
                    if (PH == false)
                    {
                        XwIa += 30;
                        XwIc -= 30;
                    }
                    break;
                #endregion
                case 4: //三元件跨相90无功表(Q33)
                    #region
                    XwUa = 30;
                    XwUb = 270;
                    XwUc = 150;
                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 150;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 330;
                        XwIc = 210;
                        n = -1;
                    }

                    if (LC == "L")
                    {
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 240 - Phi;
                        if (XwIb < 0) XwIb += 360;
                        if (XwIb >= 360) XwIb -= 360;

                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                #endregion
                case 5: //二元件跨相90无功表(Q90)
                    #region
                    XwUa = 30;
                    XwUb = 0;
                    XwUc = 270;

                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 0;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 0;
                        XwIc = 210;
                        n = -1;
                    }

                    if (LC == "L")
                    {
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                #endregion
                case 6: //二元件跨相60无功表(Q60)
                    #region
                    XwUa = 0;
                    XwUb = 0;
                    XwUc = 120;

                    if (LcValue > 0)
                    {
                        XwIa = 270;
                        XwIb = 0;
                        XwIc = 30;
                        n = 1;
                    }
                    else if (LcValue < 0)
                    {

                        XwIa = 90;
                        XwIb = 0;
                        XwIc = 210;
                        n = -1;
                    }
                    if (LC == "L")
                    {
                        if (n == -1) Phi = (-180) - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        else Phi = Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    if (LC == "C")
                    {
                        if (n == -1) Phi = (-180) - (180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5));
                        else Phi = 180 - Math.Round(Math.Asin(LcValue) * (180 / Math.PI), 5);
                        //Phi = n + Phi;
                        XwIa = 0 - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                        XwIb = 0;


                        XwIc = 120 - Phi;
                        if (XwIc < 0) XwIc += 360;
                        if (XwIc >= 360) XwIc -= 360;
                    }
                    break;
                #endregion
                case 7: //单相表
                    #region
                    Phi = Math.Round(Math.Acos(LcValue) * (180 / Math.PI), 5);
                    XwUa = 0;
                    if (LcValue > 0)
                    {
                        XwIa = 0;
                        Phi = 1 * Phi;
                    }
                    else if (LcValue < 0)
                    {
                        XwIa = 180;
                        Phi = -1 * Phi;
                    }
                    if (LC == "L")
                    {
                        Phi = 1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;

                    }
                    if (LC == "C")
                    {
                        Phi = -1 * Phi;
                        XwIa = XwUa - Phi;
                        if (XwIa < 0) XwIa += 360;
                        if (XwIa >= 360) XwIa -= 360;
                    }
                    XwUb = XwUa;
                    XwUc = XwUa;
                    XwIb = XwIa;
                    XwIc = XwIa;
                    break;
                    #endregion
            }
            #endregion

            _PhiPara.PhiUa = XwUa;
            _PhiPara.PhiUb = XwUb;
            _PhiPara.PhiUc = XwUc;
            _PhiPara.PhiIa = XwIa;
            _PhiPara.PhiIb = XwIb;
            _PhiPara.PhiIc = XwIc;
            if (_IsNXX == Cus_PowerPhase.电压逆相序)//电压电流逆相序
            {
                switch (jxfs)
                {
                    //三相三线有功表
                    case 1:
                        _PhiPara.PhiUb = 0;//XwUc;
                        _PhiPara.PhiUc = 330;//XwUb;
                        break;
                    default:
                        _PhiPara.PhiUb = XwUc;
                        _PhiPara.PhiUc = XwUb;

                        break;
                }

            }
            else if (_IsNXX == Cus_PowerPhase.电流逆相序)
            {
                _PhiPara.PhiIb = XwIc;
                _PhiPara.PhiIc = XwIb;
            }
            else if (_IsNXX == Cus_PowerPhase.逆相序)
            {
                switch (jxfs)
                {
                    //三相三线有功表
                    case 1:
                        _PhiPara.PhiUb = 0;//XwUc;
                        _PhiPara.PhiUc = 330;//XwUb;
                        break;
                    default:
                        _PhiPara.PhiUb = XwUc;
                        _PhiPara.PhiUc = XwUb;

                        break;
                }
                _PhiPara.PhiIb = XwIc;
                _PhiPara.PhiIc = XwIb;
            }
            else if (_IsNXX == Cus_PowerPhase.正相序)
            {
                _PhiPara.PhiUb = XwUb;
                _PhiPara.PhiUc = XwUc;

                _PhiPara.PhiIb = XwIb;
                _PhiPara.PhiIc = XwIc;
            }
            phiPara[0] = _PhiPara.PhiUa;
            phiPara[1] = _PhiPara.PhiUb;
            phiPara[2] = _PhiPara.PhiUc;
            phiPara[3] = _PhiPara.PhiIa;
            phiPara[4] = _PhiPara.PhiIb;
            phiPara[5] = _PhiPara.PhiIc;
            return true;
        }

        /// <summary>
        /// 获取单位，如15A 得A
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetUnit(string value)  //得到量程的单位 //value带单位的值如 15A
        {
            string unit = "";
            byte[] bs = Encoding.ASCII.GetBytes(value);
            for (int i = 0; i < bs.Length; ++i)
            {
                if (bs[i] > 57)
                {
                    unit = value.Substring(i);
                    break;
                }

            }
            return unit;
        }

        /// <summary>
        /// 设置大电流长时间工作保护
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public int SetPowerSafe(bool mode)
        {
            ZH3001_SetPowerSafePacket rc3 = new ZH3001_SetPowerSafePacket();
            ZH3001_SetPowerSafeReplyPacket recv3 = new ZH3001_SetPowerSafeReplyPacket();
            rc3.SetPara(mode ? 1 : 0);
            try
            {
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置启动或停止电能误差计算
        /// </summary>
        /// <param name="stopOrStart">启动或停止</param>
        /// <param name="typePush">脉冲类型</param>
        /// <param name="constQS">被检表常数</param>
        /// <param name="QS">误差圈数</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetErrorData(int StopOrstart, int pushType, int meterConst, int qs)
        {
            ZH3001_RequestSetErrorDataPacket rc3 = new ZH3001_RequestSetErrorDataPacket();
            ZH3001_RequestSetErrorDataReplyPacket recv3 = new ZH3001_RequestSetErrorDataReplyPacket();
            rc3.SetPara(StopOrstart, pushType, meterConst, qs);
            if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
            {
                bool reuslt = recv3.ReciveResult == RecvResult.OK;
                return reuslt ? 0 : 1;
            }
            else
            {
                return 1;
            }

        }

        /// <summary>
        /// 源做误差，读取误差值
        /// </summary>
        /// <param name="floatData">返回三组误差</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadErrorData(out float[] floatData)
        {
            ZH3001_RequestReadErrorDataPacket rc3 = new ZH3001_RequestReadErrorDataPacket();
            ZH3001_RequestReadErrorDataReplyPacket recv3 = new ZH3001_RequestReadErrorDataReplyPacket();
            floatData = new float[12];
            //合成的报文
            if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
            {
                floatData = recv3.Data;
                return recv3.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }
        }


        public int ReadContant(out long contant)
        {
            contant = 0;
            ZH3001_RequestReadContantPacket rc3 = new ZH3001_RequestReadContantPacket();
            ZH3001_RequestReadContantReplyPacket recv3 = new ZH3001_RequestReadContantReplyPacket();
            //合成的报文
            if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
            {
                contant = recv3.Data;
                return recv3.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }
        }



        #endregion
        #region 电能质量
        /// <summary>
        /// 设置谐波类型
        /// </summary>
        /// <param name="HarmonicType">00普通谐波，01间谐波</param>
        /// <returns></returns>
        public int SetHarmonicType(byte HarmonicType, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerInterharmonicsPacket rc3 = new ZH3001_RequestPowerInterharmonicsPacket();
            ZH3001_RequestPowerInterharmonicsReplyPacket recv3 = new ZH3001_RequestPowerInterharmonicsReplyPacket();
            rc3.SetPara(HarmonicType);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
        /// 设置电能质量项目
        /// </summary>
        /// <param name="QualityType">1个字节表示，00H是电压波动，01H是电压闪变</param>
        /// <param name="Model">1个字节表示，试验类型为电压波动时：参数为1、2、3（非这三个数值无效）分别对应电压波动试验项目中的检测点（1）、（2）、（3）试验项目为电压闪变时：参数为1、3（非这两个数值无效）分别对应闪变试验中的短时闪变值为1和闪变值为3的试验点。</param>
        /// <param name="State">使能状态：1个字节表示，01H表示启动，其他无效，停止时只需要下发关源指令即可</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetQualityItem(byte QualityType, byte Model, byte State, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH3001_RequestPowerQualityPacket rc3 = new ZH3001_RequestPowerQualityPacket();
            ZH3001_RequestPowerQualityReplyPacket recv3 = new ZH3001_RequestPowerQualityReplyPacket();
            rc3.SetPara(QualityType, Model, State);
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
                    {
                        bool reuslt = recv3.ReciveResult == RecvResult.OK;
                        return reuslt ? 0 : 1;
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
    }
}
