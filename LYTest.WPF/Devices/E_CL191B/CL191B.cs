using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using CLOU.Enum;
using CLOU.Struct;
using CLOU;
using CLOU.SocketModule.Packet;
using E_CL191B;

namespace ZH
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
        /// <returns></returns>
        [DispId(3)]
        int Connect(out string[] FrameAry);
        /// <summary>
        /// 断开连机
        /// </summary>
        /// <returns></returns>
        [DispId(4)]
        int DisConnect(out string[] FrameAry);
        /// <summary>
        /// 读取Gps时间
        /// </summary>
        /// <param name="GpsTime"></param>
        /// <returns></returns>
        [DispId(5)]
        int ReadGPSTime(out DateTime GpsTime, out string[] FrameAry);
        /// <summary>
        /// 读取温度、湿度
        /// </summary>
        /// <param name="sng_temp">温度</param>
        /// <param name="sng_huim">湿度</param>
        /// <returns></returns>
        [DispId(6)]
        int ReadTempHuim(out float sng_temp, out float sng_huim, out string[] FrameAry);
        /// 切换脉冲方式
        /// </summary>
        /// <param name="isTime">true 为时钟脉冲 false 为电能脉冲</param>
        /// <returns></returns>
        [DispId(7)]
        int SetTimePulse(bool isTime, out string[] FrameAry);
        /// <summary>
        /// 设置获取请求报文标志
        /// </summary>
        /// <param name="Flag">True:发送报文,并传出报文,false:不发送,只传出报文</param>
        /// <returns></returns>
        [DispId(8)]
        int SetSendFlag(bool Flag);
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName"></param>
        /// <param name="ReFrameAry"></param>
        /// <param name="ReAry"></param>
        /// <returns></returns>
        [DispId(9)]
        int UnPacket(string MothedName,byte[]ReFrameAry, out string[] ReAry);

    }
   
    public class CL191B : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private StPortInfo m_TimeSourcePort = null;

        private DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public CL191B()
        {
            m_TimeSourcePort = new StPortInfo();
            driverBase = new DriverBase();
        }

        #region IClass_Interface 成员
        /// <summary>
        /// 初始化2018端口
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
            m_TimeSourcePort.m_Exist = 1;
            m_TimeSourcePort.m_IP = IP;
            m_TimeSourcePort.m_Port = ComNumber;
            m_TimeSourcePort.m_Port_IsUDPorCom = true;
            m_TimeSourcePort.m_Port_Setting = "2400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_TimeSourcePort.m_Port_Setting, m_TimeSourcePort.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {
                return 1;
            }

            return 0;
        }
        /// <summary>
        /// 初始化Com 口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_TimeSourcePort.m_Exist = 1;
            m_TimeSourcePort.m_IP = "";
            m_TimeSourcePort.m_Port = ComNumber;
            m_TimeSourcePort.m_Port_IsUDPorCom = false;
            m_TimeSourcePort.m_Port_Setting = "2400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "2400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
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
            CL191B_RequestLinkPacket rc3 = new CL191B_RequestLinkPacket();
            CL191B_RequestLinkReplyPacket recv3 = new CL191B_RequestLinkReplyPacket();
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_TimeSourcePort, rc3, recv3))
                    {
                        bool linkClockOk = recv3.ReciveResult == RecvResult.OK;
                        string Clockmessage = string.Format("时基源联机{0}。", linkClockOk ? "成功" : "失败");
                        return linkClockOk ? 0 : 1;
                    }
                    else
                    {
                        return 2;
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
            FrameAry = new string[1];
            return 0;
        }

        /// <summary>
        /// 读取GPS时间，如果失败会返回当前时间
        /// </summary>
        /// <returns></returns>
        public int ReadGPSTime(out DateTime GpsTime, out string[] FrameAry)
        {
            CL191B_RequestReadGPSTimePacket rc = new CL191B_RequestReadGPSTimePacket();
            RecvPacket rc2 = new CL191B_RequestReadGPSTimeReplayPacket();
            DateTime dt = new DateTime();
            FrameAry = new string[1];
            GpsTime = DateTime.Now;

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_TimeSourcePort, rc, rc2))
                    {
                        dt = ((CL191B_RequestReadGPSTimeReplayPacket)rc2).GPSTime;
                        if (dt == (new DateTime(1, 1, 1, 0, 0, 0)))
                        {
                            dt = DateTime.Now;
                            GpsTime = dt;
                            return 1;
                        }
                        GpsTime = dt;
                        return 0;

                    }
                    GpsTime = DateTime.Now;
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
        /// 读取温度湿度191
        /// </summary>
        /// <param name="sng_temp">温度返回</param>
        /// <param name="sng_huim">湿度返回</param>
        /// <returns></returns>
        public int ReadTempHuim(out float sng_temp, out float sng_huim, out string[] FrameAry)
        {
            sng_temp = -1f;
            sng_huim = -1f;
            FrameAry = new string[1];

            CL191B_RequestReadTemperatureAndHumidityPacket rc = new CL191B_RequestReadTemperatureAndHumidityPacket();
            CL191B_RequestReadTemperatureAndHumidityReplayPacket backpc = new CL191B_RequestReadTemperatureAndHumidityReplayPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {

                    if (SendPacketWithRetry(m_TimeSourcePort, rc, backpc))
                    {
                        sng_temp = backpc.Tempututer;
                        sng_huim = backpc.Humidity;
                        return 0;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception)
            {
                
                return -1;
            }

        }

        /// <summary>
        /// 切换时钟脉冲
        /// </summary>
        /// <param name="isTime">是否切换为时钟脉冲</param>
        public int SetTimePulse(bool isTime, out string[] FrameAry)
        {
            FrameAry = new string[1];
            CL191B_RequestSetChannelPacket rc191 = new CL191B_RequestSetChannelPacket();
            if (isTime)
            {
                rc191.channelType = enmStdPulseType.标准时钟脉冲;
            }
            else
            {
                rc191.channelType = enmStdPulseType.标准电能脉冲;
            }
            CL191B_RequestSetChannelReplyPacket rc191rec = new CL191B_RequestSetChannelReplyPacket();

            try
            {
                FrameAry[0] = BytesToString(rc191.GetPacketData());

                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_TimeSourcePort, rc191, rc191rec))
                        return 0;
                    return 1;
                }
                return 0;
            }
            catch (Exception)
            {
                
                return -1;
            }


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
                    {
                        try
                        {
                            CL191B_RequestLinkReplyPacket recv = new CL191B_RequestLinkReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                        }
                        catch (Exception)
                        {
                            
                            return -1;
                        }

                    }
                    break;
                case "DisConnect":
                    {
                        ReAry[0] = string.Empty;
                    }
                    break;
                case "ReadGPSTime":
                    {
                        try
                        {
                            CL191B_RequestReadGPSTimeReplayPacket recv = new CL191B_RequestReadGPSTimeReplayPacket();
                            if (recv.ParsePacket(ReFrameAry))
                            {
                                ReAry[0] = recv.GPSTime.ToString();
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        catch (Exception)
                        {
                            
                            return -1;
                        }

                    }
                    break;
                case "ReadTempHuim":
                    {
                        try
                        {
                            CL191B_RequestReadTemperatureAndHumidityReplayPacket recv = new CL191B_RequestReadTemperatureAndHumidityReplayPacket();
                            if (recv.ParsePacket(ReFrameAry))
                            {
                                ReAry = new string[2];
                                ReAry[0] = recv.Tempututer.ToString();
                                ReAry[1] = recv.Humidity.ToString();
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        catch (Exception)
                        {
                            
                            return -1;
                        }

                    }
                    break;
                case "SetTimePulse":
                    {
                        try
                        {
                            CL191B_RequestSetChannelReplyPacket rc191rec = new CL191B_RequestSetChannelReplyPacket();
                            if (rc191rec.ParsePacket(ReFrameAry))
                            {
                                ReAry[0] = rc191rec.ReciveResult.ToString();
                            }
                            else
                            {
                                return 1;
                            }

                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                default:
                    {
                        ReAry[0] = "Null this Data";
                    }
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
            string strRevalue =string.Empty;
            if (bytesData == null || bytesData.Length < 1)
                return strRevalue;

            for (int i = 0; i < bytesData.Length; i++)
            {
                strRevalue += Convert.ToString(bytesData[i], 16);
            }
            return strRevalue;
        }
        #endregion

    }
}
