using System;
using System.Collections.Generic;
using System.Text;
using E_CLSocketModule.Struct;
using E_CLSocketModule.SocketModule.Packet;
using E_CLSocketModule.SocketModule;
using System.Runtime.InteropServices;
using E_CLSocketModule.Enum;

namespace E_CLSocketModule
{
    public class DriverBase
    {
        ///// <summary>
        ///// 配置信息类
        ///// </summary>
        //public CLDC_DeviceDriver.Setting.SettingBase Setting;

        ///// <summary>
        ///// 消息回调函数
        ///// </summary>
        //public MsgCallBack msgCallBack;

        /// <summary>
        /// 当前工作流状态
        /// </summary>
        public WorkFlow currentWorkFlow = WorkFlow.None;

        public DriverBase()
        {
            
        }

        #region ---------端口操作&数据包发送----------
        
        /// <summary>
        /// 根据端口号、IP地址获取唯一端口名称
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="UDPorCOM">true UDP,false COM</param>
        /// <param name="ip">IP isNullOrEmpty COM ,other wise UDP/TCP</param>
        /// <returns></returns>
        public string GetPortNameByPortNumber(int port, Cus_EmComType ComType, string ip)
        {
            switch(ComType)
            {
                case Cus_EmComType.COM:
                    return string.Format("Port_COM_{0}", port);
                case Cus_EmComType.UDP:
                    return string.Format("Port_UDP_{0}_{1}", ip, port);
                case Cus_EmComType.TCP:
                    return string.Format("Port_TCP_{0}_{1}", ip, port);
                default:
                    return string.Format("Port_UDP_{0}_{1}", ip, port);
            }
        }

        /// <summary>
        /// 注册端口[UDP端口]
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="strSetting">端口参数</param>
        /// <param name="IP">串口服务器IP</param>
        public void RegisterPort(int port, string strSetting, string IP, int maxWaittime, int waitSencondsPerByte)
        {
            RegisterPort(port, strSetting, IP, 10004, 30000, "2018-负控", maxWaittime, waitSencondsPerByte);
        }

        public void RegisterPort(int port, string strSetting, string IP, int RemotePort, int LocalStartPoet,string HaveProtocol, int maxWaittime, int waitSencondsPerByte)
        {
            System.Net.IPAddress ipa = System.Net.IPAddress.Parse(IP);
            string portName = GetPortNameByPortNumber(port,Cus_EmComType.UDP, IP);
            //注册数据端口
            SockPool.Instance.AddUdpSock(portName, ipa, RemotePort, port, LocalStartPoet,HaveProtocol, maxWaittime, waitSencondsPerByte);
            
            SockPool.Instance.UpdatePortSetting(portName, strSetting);
            SockPool.Instance.UpdatePortSetting(portName, strSetting);
        }

        /// <summary>
        /// 注册端口[串口]
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="strSetting">串口设置</param>
        public void RegisterPort(int port, string strSetting, int maxWaittime, int waitSencondsPerByte)
        {
            string portName = GetPortNameByPortNumber(port,Cus_EmComType.COM, "");
            //注册设置端口
            SockPool.Instance.AddComSock(portName, port, strSetting, maxWaittime, waitSencondsPerByte);
            SockPool.Instance.UpdatePortSetting(portName, strSetting);
        }

        
        /// <summary>
        /// UDP发送,重新初始化波特率
        /// </summary>
        ///<param name="stPort"></param>
        /// <param name="sendPacket"></param>
        /// <param name="recvPacket"></param>
        /// <returns></returns>
        public bool SendData(StPortInfo stPort, SendPacket sendPacket, RecvPacket recvPacket)
        {
            if (stPort.m_Exist == 0)
            {
                return true;
            }
            return SendData(stPort.m_Port, stPort.m_Port_Type, stPort.m_IP, stPort.m_Port_Setting, sendPacket, recvPacket);
        }

        /// <summary>
        /// UDP发送,重新初始化波特率
        /// </summary>
        ///<param name="stPort"></param>
        /// <param name="sendPacket"></param>
        /// <param name="recvPacket"></param>
        /// <returns></returns>
        public bool SendData(StPortInfo stPort, SendPacket sendPacket, RecvPacket recvPacket, int MaxWaitSeconds)
        {
            if (stPort.m_Exist == 0)
            {
                return true;
            }
            return SendData(stPort.m_Port, stPort.m_Port_Type, stPort.m_IP, stPort.m_Port_Setting, sendPacket, recvPacket, MaxWaitSeconds);
        }

        /// <summary>
        /// true UDP,false COM。重新初始化波特率
        /// </summary>
        /// <param name="port"></param>
        /// <param name="UDPorCOM">true UDP,false COM</param>
        /// <param name="IP">IP</param>
        /// <param name="sendPacket"></param>
        /// <param name="recvPacket"></param>
        /// <returns></returns>
        private bool SendData(int port, Cus_EmComType ComType, string IP, string strSetting, SendPacket sendPacket, RecvPacket recvPacket)
        {
            string portName = GetPortNameByPortNumber(port, ComType, IP);
            if (strSetting.Trim() != "")
            {
                SockPool.Instance.UpdatePortSetting(portName, strSetting);
            }
            return SockPool.Instance.Send(portName, sendPacket, recvPacket);
        }

        /// <summary>
        /// true UDP,false COM。重新初始化波特率
        /// </summary>
        /// <param name="port"></param>
        /// <param name="UDPorCOM">true UDP,false COM</param>
        /// <param name="IP">IP</param>
        /// <param name="sendPacket"></param>
        /// <param name="recvPacket"></param>
        /// <returns></returns>
        private bool SendData(int port, Cus_EmComType ComType, string IP, string strSetting, SendPacket sendPacket, RecvPacket recvPacket, int MaxWaitSeconds)
        {
            string portName = GetPortNameByPortNumber(port, ComType, IP);
            if (strSetting.Trim() != "")
            {
                SockPool.Instance.UpdatePortSetting(portName, strSetting);
            }
            return SockPool.Instance.Send(portName, sendPacket, recvPacket, MaxWaitSeconds);
        }
        
        /// <summary>
        /// true UDP,false COM。主动初始化波特率。strSetting空字符串则不初始化
        /// </summary>
        /// <param name="port"></param>
        /// <param name="strSetting">波特率（格式：38400,n,8,1）</param>
        /// <param name="UDPorCOM">true UDP,false COM</param>
        /// <param name="IP">IP</param>
        /// <param name="sendPacket"></param>
        /// <param name="recvPacket"></param>
        /// <returns></returns>
        public bool SendData(int port, string strSetting, Cus_EmComType ComType, string IP, SendPacket sendPacket, RecvPacket recvPacket)
        {
            string portName = GetPortNameByPortNumber(port, ComType, IP);
            if (strSetting.Trim() != "")
            {
                SockPool.Instance.UpdatePortSetting(portName, strSetting);
            }
            return SockPool.Instance.Send(portName, sendPacket, recvPacket);
        }

        #endregion

    }
}
