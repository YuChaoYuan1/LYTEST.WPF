using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using ZH.Enum;
using ZH.Struct;
using ZH;
using ZH.SocketModule.Packet;
using ZH3501;

namespace ZH
{
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
     ComVisible(true)]
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
    
    public class ZH3501 : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private StPortInfo m_ErrorZH3501Port = null;

        private DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZH3501()
        {
            m_ErrorZH3501Port = new StPortInfo();
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
            m_ErrorZH3501Port.m_Exist = 1;
            m_ErrorZH3501Port.m_IP = IP;
            m_ErrorZH3501Port.m_Port = ComNumber;
            m_ErrorZH3501Port.m_Port_IsUDPorCom = true;
            m_ErrorZH3501Port.m_Port_Setting = "19200,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_ErrorZH3501Port.m_Port_Setting, m_ErrorZH3501Port.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
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
            m_ErrorZH3501Port.m_Exist = 1;
            m_ErrorZH3501Port.m_IP = "";
            m_ErrorZH3501Port.m_Port = ComNumber;
            m_ErrorZH3501Port.m_Port_IsUDPorCom = false;
            m_ErrorZH3501Port.m_Port_Setting = "19200,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "19200,n,8,1", MaxWaitTime, WaitSencondsPerByte);
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
            ZH3501_SendLinkRelayPacket rc2 = new ZH3501_SendLinkRelayPacket();
            ZH3501_RequesttLinkRelayReplyPacket recv2 = new ZH3501_RequesttLinkRelayReplyPacket();
            FrameAry = new string[1];
            int ReValue = 0;
            try
            {
               
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (!sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZH3501Port, rc2, recv2))
                    {
                        ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 2;
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
        /// ZH3501多功能板控制载波切换通道
        /// </summary>
        /// <param name="ControlboardID">载波通道号(FF广播)</param>
        /// <param name="ControlboardType">01=闭合，00=断开</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int  SetZH3501Controlboard(int ControlboardID,int ControlboardType,   out string[] FrameAry)
        {
            ZH3501_SendContonlRelayPacket rc2 = new ZH3501_SendContonlRelayPacket();
            ZH3501_RequesttContonlRelayReplyPacket recv2 = new ZH3501_RequesttContonlRelayReplyPacket();
            FrameAry = new string[1];
            int ReValue = 0;
            try
            {
                //       发送：68H+RID+SID+LEN+13H+8800H+DATA+CS  （DATA=2bytes）
                //LEN：0x0A
                //DATA内容：01 00—2bytes， 第1路载波模块继电器断开
                //01 01---2bytes， 第1路载波模块继电器闭合
                //02 00—2bytes， 第2路载波模块继电器断开
                //02 01---2bytes， 第2路载波模块继电器闭合
                //03 00—2bytes， 第3路载波模块继电器断开
                //03 01---2bytes， 第3路载波模块继电器闭合
                //04 00—2bytes， 第4路载波模块继电器断开
                //04 01---2bytes， 第4路载波模块继电器闭合
                //FF 00---2bytes,  所有4路继电器都断开         //新增
                //FF 01---2bytes,  所有4路继电器都闭合         //新增

                rc2.SetPara(ControlboardID, ControlboardType);
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZH3501Port, rc2, recv2))
                    {
                        ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 2;
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
