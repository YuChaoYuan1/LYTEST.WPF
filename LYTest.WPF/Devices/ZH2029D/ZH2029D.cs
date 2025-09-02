using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using CLOU;
using CLOU.Struct;
using CLOU.SocketModule.Packet;
using System.Threading;

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

    public class ZH2029D : IClass_Interface
    {
        /// <summary>
        /// 载波端口
        /// </summary>
        private StPortInfo[] m_2029DPort = null;
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

        public ZH2029D()
        {
            m_2029DPort = new StPortInfo[1];
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
                m_2029DPort[0] = new StPortInfo();
                m_2029DPort[0].m_Exist = 1;
                m_2029DPort[0].m_IP = IP;
                m_2029DPort[0].m_Port = ComNumber;
                m_2029DPort[0].m_Port_IsUDPorCom = true;
                m_2029DPort[0].m_Port_Setting = "38400,n,8,1";
                driverBase.RegisterPort(ComNumber, m_2029DPort[0].m_Port_Setting, m_2029DPort[0].m_IP, RemotePort, LocalStartPort, MaxWaitTime, 100);
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
            m_2029DPort[0] = new StPortInfo();
            m_2029DPort[0].m_Exist = 1;
            m_2029DPort[0].m_IP = "";
            m_2029DPort[0].m_Port = ComNumber;
            m_2029DPort[0].m_Port_IsUDPorCom = false;
            m_2029DPort[0].m_Port_Setting = "38400,n,8,1";
            driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
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


            try
            {
                CL2029D_RequestSetSwitchPacket cl2029 = new CL2029D_RequestSetSwitchPacket();
                cl2029.SetPara(19, 1);
                cl2029.IsNeedReturn = true;
                CL2029D_RequestSetSwitchReplyPacket rcl2029 = new CL2029D_RequestSetSwitchReplyPacket();

                FrameAry[0] = BytesToString(cl2029.GetPacketData());
                if (sendFlag)
                {

                    if (SendPacketWithRetry(m_2029DPort[0], cl2029, rcl2029))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        return 1;
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
        /// 切换载波供电
        /// </summary>
        /// <param name="bType">true,供电；false，停电</param>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int SetSwitchTypeForCarrier(bool bType, out string[] FrameAry)
        {
            int reValue = -1;
            FrameAry = new string[1];
            CL2029D_RequestSetSwitchPacket rc = new CL2029D_RequestSetSwitchPacket();
            CL2029D_RequestSetSwitchReplyPacket recv = new CL2029D_RequestSetSwitchReplyPacket();
            rc.SetPara(19, bType ? 1 : 0);

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029DPort[0], rc, recv))
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
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int SetEquipmentThreeColor(int iID, int iType, out string[] FrameAry)
        {
            int reValue = -1;
            FrameAry = new string[1];
            CL2029D_RequestSetSwitchPacket rc = new CL2029D_RequestSetSwitchPacket();
            CL2029D_RequestSetSwitchReplyPacket recv = new CL2029D_RequestSetSwitchReplyPacket();
            rc.SetPara(iID, iType);
            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_2029DPort[0], rc, recv))
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
        /// 供电类型
        /// </summary>
        /// <param name="elementType">耐压供电=1、载波供电=2、普通供电=3、二回路=5、 耐压保护=6</param>
        /// <param name="isMeterTypeHGQ">false直接式，true互感式</param>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int SetPowerSupplyType(int elementType, bool isMeterTypeHGQ, int[] switchOpen, int[] switchClose, out string[] FrameAry)
        {
            int reValue = -1;
            CL2029D_RequestSetSwitchPacket cl2029d = new CL2029D_RequestSetSwitchPacket();
            CL2029D_RequestSetSwitchReplyPacket cl2029drec = new CL2029D_RequestSetSwitchReplyPacket();
            int iCount = switchOpen.Length + switchClose.Length;
            FrameAry = new string[iCount];
            List<string> listFrames = new List<string>();
            string strSingle = string.Empty;
            //0断开，1闭合
            //耐压供电 电压电流对地
            if (elementType == 1)
            {
                #region
                //初始化第一块多功能板 闭合
                for (int i = 0; i < switchOpen.Length; i++)
                {
                    cl2029d.SetPara(switchOpen[i], 1);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }
                    Thread.Sleep(100);
                }
                //第一块多功能板 断开
                for (int i = 0; i < switchClose.Length; i++)
                {
                    cl2029d.SetPara(switchClose[i], 0);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }
                    Thread.Sleep(100);
                }

                #endregion
            }
            //载波供电
            else if (elementType == 2)
            {
                #region
                for (int i = 0; i < switchOpen.Length; i++)
                {
                    cl2029d.SetPara(switchOpen[i], 1);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }

                    Thread.Sleep(100);
                }
                for (int i = 0; i < switchClose.Length; i++)
                {
                    cl2029d.SetPara(switchClose[i], 0);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }
                    Thread.Sleep(100);
                }
                #endregion
            }
            else if (elementType == 5)//二回路
            {
                cl2029d.SetPara(3, 1);
                strSingle = BytesToString(cl2029d.GetPacketData());
                listFrames.Add(strSingle);
                if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                {
                    reValue = 0;
                }
                else
                {
                    reValue = 1;
                }
                Thread.Sleep(100);
                cl2029d.SetPara(2, 0);//
                strSingle = BytesToString(cl2029d.GetPacketData());
                listFrames.Add(strSingle);
                if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                {
                    reValue = 0;
                }
                else
                {
                    reValue = 1;
                }
                Thread.Sleep(100);
            }
            else if (elementType == 6)//耐压保护
            {
                if (switchOpen.Length > 0)
                {
                    cl2029d.SetPara(switchOpen[0], 0);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }
                }
            }
            else if (elementType == 7) //耐压供电电压对电流
            {
                //第一块多功能板 闭合
                for (int i = 0; i < switchOpen.Length; i++)
                {
                    cl2029d.SetPara(switchOpen[i], 1);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }
                    Thread.Sleep(100);
                }
                //第一块多功能表 断开
                for (int i = 0; i < switchClose.Length; i++)
                {
                    cl2029d.SetPara(switchClose[i], 0);
                    strSingle = BytesToString(cl2029d.GetPacketData());
                    listFrames.Add(strSingle);
                    if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                    {
                        reValue = 0;
                    }
                    else
                    {
                        reValue = 1;
                    }
                    Thread.Sleep(100);
                }

            }
            else //普通供电
            {
                #region 0开 1闭
                if (isMeterTypeHGQ == false)//直接式
                {
                    for (int i = 0; i < switchOpen.Length; i++)
                    {
                        cl2029d.SetPara(switchOpen[i], 1);

                        strSingle = BytesToString(cl2029d.GetPacketData());
                        listFrames.Add(strSingle);
                        if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                        {
                            reValue = 0;
                        }
                        else
                        {
                            reValue = 1;
                        }
                        Thread.Sleep(100);
                    }
                    for (int i = 0; i < switchClose.Length; i++)
                    {
                        cl2029d.SetPara(switchClose[i], 0);

                        strSingle = BytesToString(cl2029d.GetPacketData());
                        listFrames.Add(strSingle);
                        if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                        {
                            reValue = 0;
                        }
                        else
                        {
                            reValue = 1;
                        }
                        Thread.Sleep(100);
                    }
                }
                else//互感式
                {
                    for (int i = 0; i < switchOpen.Length; i++)
                    {
                        cl2029d.SetPara(switchOpen[i], 1);

                        strSingle = BytesToString(cl2029d.GetPacketData());
                        listFrames.Add(strSingle);

                        if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                        {
                            reValue = 0;
                        }
                        else
                        {
                            reValue = 1;
                        }
                        Thread.Sleep(100);
                    }

                    for (int i = 0; i < switchClose.Length; i++)
                    {
                        cl2029d.SetPara(switchClose[i], 0);

                        strSingle = BytesToString(cl2029d.GetPacketData());
                        listFrames.Add(strSingle);

                        if (SendPacketWithRetry(m_2029DPort[0], cl2029d, cl2029drec))
                        {
                            reValue = 0;
                        }
                        else
                        {
                            reValue = 1;
                        }
                        Thread.Sleep(100);
                    }


                }
                #endregion


            }

            FrameAry = listFrames.ToArray();

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
                            CL2029D_RequestSetSwitchReplyPacket recv = new CL2029D_RequestSetSwitchReplyPacket();
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
                case "SetSwitchTypeForCarrier":
                    {
                        try
                        {
                            CL2029D_RequestSetSwitchReplyPacket recv = new CL2029D_RequestSetSwitchReplyPacket();
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
                            CL2029D_RequestSetSwitchReplyPacket recv = new CL2029D_RequestSetSwitchReplyPacket();
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
                case "SetPowerSupplyType":
                    {
                        try
                        {
                            CL2029D_RequestSetSwitchReplyPacket recv = new CL2029D_RequestSetSwitchReplyPacket();
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

        public int MultiControl(byte number, byte open)
        {
            return 5;
        }
    }
}
