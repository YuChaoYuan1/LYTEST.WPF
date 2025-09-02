using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using CLOU;
using CLOU.Struct;
using CLOU.SocketModule.Packet;
using CLOU.Enum;
using E_CL2029B;

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
        /// <param name="WaitSencondsPerByte"></param>
        /// <param name="IP">2018IP地址</param>
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
        /// 设置标志位
        /// </summary>
        /// <param name="Flag">设置标志位</param>
        /// <returns></returns>
        [DispId(8)]
        int SetSendFlag(bool Flag);
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName">方法名称</param>
        /// <param name="ReFrameAry">下行报文</param>
        /// <param name="ReAry">解析下行报文的数据</param>
        /// <returns></returns>
        [DispId(9)]
        int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry);
    }

    public class ZH2029B : IClass_Interface
    {
        /// <summary>
        /// 载波端口
        /// </summary>
        private StPortInfo[] m_2029BPort = null;
        /// <summary>
        /// 通讯基类
        /// </summary>
        private DriverBase driverBase = null;
        //发送标志
        private bool sendFlag = true;

        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;

        public ZH2029B()
        {
            m_2029BPort = new StPortInfo[1];
            driverBase = new DriverBase();
        }
        /// <summary>
        /// 注册设备端口信息
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="Parameter"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <param name="IP"></param>
        /// <param name="RemotePort"></param>
        /// <param name="LocalStartPort"></param>
        /// <returns></returns>
        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort)
        {
            try
            {
                m_2029BPort[0] = new StPortInfo();
                m_2029BPort[0].m_Exist = 1;
                m_2029BPort[0].m_IP = IP;
                m_2029BPort[0].m_Port = ComNumber;
                m_2029BPort[0].m_Port_IsUDPorCom = true;
                //m_2029BPort[0].m_Port_Setting = "38400,n,8,1";
                m_2029BPort[0].m_Port_Setting = "9600,n,8,1";
                driverBase.RegisterPort(ComNumber, m_2029BPort[0].m_Port_Setting, m_2029BPort[0].m_IP, RemotePort, LocalStartPort, MaxWaitTime, 100);
                return 0;
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// 注册Com 口
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">最大等待时间</param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_2029BPort[0] = new StPortInfo();
            m_2029BPort[0].m_Exist = 1;
            m_2029BPort[0].m_IP = "";
            m_2029BPort[0].m_Port = ComNumber;
            m_2029BPort[0].m_Port_IsUDPorCom = false;
            //m_2029BPort[0].m_Port_Setting = "38400,n,8,1";
            //driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            m_2029BPort[0].m_Port_Setting = "9600,n,8,1";
            driverBase.RegisterPort(ComNumber, "9600,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            return 0;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            int reValue = -1;
            CL2029B_RequestLinkPacket rc = new CL2029B_RequestLinkPacket();
            Cl2029B_RequestLinkReplyPacket recv = new Cl2029B_RequestLinkReplyPacket();

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029BPort[0], rc, recv))
                    {
                        reValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue = 1;
                    }
                }
                else
                {
                    reValue = 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }



            return reValue;
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int DisConnect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }
        /// <summary>
        /// 设置警示灯                                           
        /// </summary>
        /// <param name="lightType">关灯=0,红灯=1,黄灯=2,绿灯=4</param>
        /// /// <param name="lightType">输出上行报文</param>
        /// <returns></returns>
        public int SetLightType(int lightType, out string[] FrameAry)
        {
            FrameAry = new string[1];
            int reValue = -1;
            CL2029B_RequestSetLightPacket rc = new CL2029B_RequestSetLightPacket();
            Cl2029B_RequestSetLightReplyPacket recv = new Cl2029B_RequestSetLightReplyPacket();
            try
            {
                rc.SetPara((int)lightType);
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029BPort[0], rc, recv))
                    {
                        reValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue = 1;
                    }
                }
                else
                {
                    reValue = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return reValue;
        }
        /// <summary>
        /// 切换载波供电
        /// </summary>
        /// <param name="bType">切换载波供电</param>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int SetSwitchTypeForCarrier(bool bType, out string[] FrameAry)
        {
            int reValue = -1;
            FrameAry = new string[1];
            CL2029B_RequestSetSwitchPacket rc = new CL2029B_RequestSetSwitchPacket();

            CL2029B_RequestSetSwitchReplayPacket recv = new CL2029B_RequestSetSwitchReplayPacket();
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029BPort[0], rc, recv))
                    {
                        reValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue = 1;
                    }
                }
                else
                {
                    reValue = 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return reValue;

        }
        /// <summary>
        /// 设置三色灯
        /// </summary>
        /// <param name="iID">灯类型 18红、19黄、20绿</param>
        /// <param name="iType">等于0时灭、1时正常、2时闪烁</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetEquipmentThreeColor(int iID, int iType, out string[] FrameAry)
        {
            FrameAry = new string[1];
            Cus_LightType lightType = Cus_LightType.OFF;
            CL2029B_RequestSetLightPacket rc = new CL2029B_RequestSetLightPacket();
            Cl2029B_RequestSetLightReplyPacket recv = new Cl2029B_RequestSetLightReplyPacket();
            if (iID == 18)
                lightType = Cus_LightType.RED;
            else if (iID == 19)
                lightType = Cus_LightType.YELLOW;
            else if (iID == 20)
                lightType = Cus_LightType.GREE;
            if (iType == 0)
                lightType = Cus_LightType.OFF;
            rc.SetPara((int)lightType);
            int reValue;
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029BPort[0], rc, recv))
                    {
                        reValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue = 1;
                    }
                }
                else
                {
                    reValue = 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return reValue;
        }
        /// <summary>
        /// 设置发送标志
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public int SetSendFlag(bool Flag)
        {
            sendFlag = Flag;
            return 0;
        }
        /// <summary>
        /// 解析下行报文 
        /// </summary>
        /// <param name="MothedName">方法名称</param>
        /// <param name="ReFrameAry">下行报文</param>
        /// <param name="ReAry">解析后的数据</param>
        /// <returns></returns>
        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            int reValue = 3;
            ReAry = new string[1];
            MothedName = MothedName.Replace(" ", "");
            switch (MothedName)
            {
                case "Connect":
                    {
                        try
                        {
                            Cl2029B_RequestLinkReplyPacket recv = new Cl2029B_RequestLinkReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }

                    }
                    break;
                case "DisConnect":
                    {
                        reValue = 3;
                    }
                    break;
                case "SetLightType":
                    {
                        try
                        {
                            Cl2029B_RequestSetLightReplyPacket recv = new Cl2029B_RequestSetLightReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetSwitchTypeForCarrier":
                    {
                        try
                        {
                            CL2029B_RequestSetSwitchReplayPacket recv = new CL2029B_RequestSetSwitchReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        catch (Exception)
                        {

                            return -1;
                        }
                    }
                    break;
                case "SetEquipmentThreeColor":
                    {
                        try
                        {
                            Cl2029B_RequestSetLightReplyPacket recv = new Cl2029B_RequestSetLightReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
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

            return reValue;
        }


        /// <summary>
        /// 发送数据到相应的端口
        /// </summary>
        /// <param name="stPort"></param>
        /// <param name="sp"></param>
        /// <param name="rp"></param>
        /// <returns></returns>
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {
            for (int i = 0; i < RETRYTIEMS; i++)
            {
                if (driverBase.SendData(stPort, sp, rp) == true)
                {
                    return true;
                }
                System.Threading.Thread.Sleep(300);
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

        public int SetPowerSupplyType(int elementType, bool isMeterTypeHGQ, int[] switchOpen, int[] switchClose, out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 5;
        }

        public int MultiControl(byte number, byte open)
        {
            return 5;
        }

        /// <summary>
        /// 切换直接电流，CT供电；互感电流，CT断电
        /// </summary>
        /// <param name="iType">0直接电流，CT供电；1互感电流，CT断电</param>
        /// <param name="iState">备用</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetSwitchDirectTransformer(int iType, int iState, out string[] FrameAry)
        {
            int reValue = -1;
            FrameAry = new string[2];
            try
            {
                CL2029B_SetSwitchCurrentPacket rc = new CL2029B_SetSwitchCurrentPacket();
                CL2029B_SetSwitchCurrentReplayPacket recv = new CL2029B_SetSwitchCurrentReplayPacket();

                CL2029B_SetSwitchCTPacket rc2 = new CL2029B_SetSwitchCTPacket();
                CL2029B_SetSwitchCTReplayPacket recv2 = new CL2029B_SetSwitchCTReplayPacket();
                if (iType == 1)
                {
                    rc.SetPara(true);
                    rc2.SetPara(false);
                }
                else
                {
                    rc.SetPara(false);
                    rc2.SetPara(true);
                }
                FrameAry[0] = BytesToString(rc.GetPacketData());
                FrameAry[1] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029BPort[0], rc, recv))
                    {
                        reValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue = 1;
                    }

                    if (SendPacketWithRetry(m_2029BPort[0], rc2, recv2))
                    {
                        reValue += recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue += 1;
                    }
                }
                else
                {
                    reValue = 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return reValue;
        }

    }
}
