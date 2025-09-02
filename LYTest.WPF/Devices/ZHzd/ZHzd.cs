using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using ZH.Enum;
using ZH.Struct;
using ZH;
using ZH.SocketModule.Packet;
using ZHzd;

namespace ZH
{
    public interface IClass_Interface
    {
        /// <summary>
        /// 初始化设备通讯参数UDP
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
        /// 连接
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(3)]
        int Connect(out string[] FrameAry);

        /// <summary>
        /// 脱机指令没有
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        [DispId(4)]
        int DisConnect(out string[] FrameAry);

    }

    public class ZHzd : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private readonly StPortInfo m_ErrorZHzdPort = null;

        private readonly DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZHzd()
        {
            m_ErrorZHzdPort = new StPortInfo();
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
            m_ErrorZHzdPort.m_Exist = 1;
            m_ErrorZHzdPort.m_IP = IP;
            m_ErrorZHzdPort.m_Port = ComNumber;
            m_ErrorZHzdPort.m_Port_IsUDPorCom = true;
            m_ErrorZHzdPort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_ErrorZHzdPort.m_Port_Setting, m_ErrorZHzdPort.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
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
            m_ErrorZHzdPort.m_Exist = 1;
            m_ErrorZHzdPort.m_IP = "";
            m_ErrorZHzdPort.m_Port = ComNumber;
            m_ErrorZHzdPort.m_Port_IsUDPorCom = false;
            m_ErrorZHzdPort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_ErrorZHzdPort.m_Port_Setting, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="FrameAry">出参</param>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            ZHzd_RequestLinkPacket rc2 = new ZHzd_RequestLinkPacket();
            ZHzd_RequestLinkReplyPacket recv2 = new ZHzd_RequestLinkReplyPacket();
            FrameAry = new string[1];
            try
            {

                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (!sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZHzdPort, rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 2;
                        return ReValue;
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
        /// 脱机指令没有
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int DisConnect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }


        /// <summary>
        /// 载波供电
        /// </summary>
        /// <param name="zbid"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ZBPowerSupplyType(int zbid, out string[] FrameAry)
        {
            FrameAry = new string[1];

            int closetype = 1;

            //ZHzd_RequestContonlRelayPacket rc3 = new ZHzd_RequestContonlRelayPacket();
            //ZHzd_RequesttContonlRelayReplyPacket recv3 = new ZHzd_RequesttContonlRelayReplyPacket();

            ZH_Contonl12Packet rc3 = new ZH_Contonl12Packet
            {
                IsNeedReturn = false
            };

            zbid = 0xFF;
            ZH_Contonl12ReplyPacket recv3 = new ZH_Contonl12ReplyPacket();
            try
            {
                rc3.SetPara(zbid, closetype);
                FrameAry[0] = BytesToString(rc3.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZHzdPort, rc3, recv3))
                    {
                        return recv3.ReciveResult == RecvResult.OK ? 0 : 2;
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





    }
}
