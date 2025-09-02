using System;
using System.Runtime.InteropServices;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH.Struct;

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
        /// 脱机指令没有
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        [DispId(4)]
        int DisConnect(out string[] FrameAry);

        /// <summary>
        /// 连机
        /// </summary>
        /// <returns></returns>
        [DispId(5)]
        int Connect(out string[] FrameAry);

    }

    public class ZH1104 : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 误差板控制端口
        /// </summary>
        private readonly StPortInfo m_ErrorZh1104Port = null;

        private readonly DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZH1104()
        {
            m_ErrorZh1104Port = new StPortInfo();

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
            m_ErrorZh1104Port.m_Exist = 1;
            m_ErrorZh1104Port.m_IP = IP;
            m_ErrorZh1104Port.m_Port = ComNumber;
            m_ErrorZh1104Port.m_Port_IsUDPorCom = true;
            m_ErrorZh1104Port.m_Port_Setting = "2400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_ErrorZh1104Port.m_Port_Setting, m_ErrorZh1104Port.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
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
            m_ErrorZh1104Port.m_Exist = 1;
            m_ErrorZh1104Port.m_IP = "";
            m_ErrorZh1104Port.m_Port = ComNumber;
            m_ErrorZh1104Port.m_Port_IsUDPorCom = false;
            //m_ErrorZh1104Port.m_Port_Setting = "19200,n,8,1";
            m_ErrorZh1104Port.m_Port_Setting = "38400,n,8,1";
            try
            {
                //修改波特率
                //driverBase.RegisterPort(ComNumber, "19200,n,8,1", MaxWaitTime, WaitSencondsPerByte);
                driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// 读取功耗，功耗板 ZH1104
        /// </summary>
        /// <param name="bwIndex"></param>
        /// <param name="pd"></param>
        /// <returns></returns>
        public bool Read_GH_Dissipation(int bwIndex, out float[] pd)
        {
            pd = new float[15];

            for (int i = 0; i < pd.Length; i++)
            {
                pd[i] = 99.99F;
            }
            ZH1104_ReadGHDataRelayPacket sgh = new ZH1104_ReadGHDataRelayPacket(bwIndex);
            ZH1104_ReadGHDataReplyPacket rgh = new ZH1104_ReadGHDataReplyPacket();
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    return false;
                }
                pd = rgh.Fldata;
            }
            else
            {
                return false;
            }
            return true;
        }

        //add yjt 20230103 新增功耗板联机
        /// <summary>
        /// 连机
        /// </summary>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH1104_RequestLinkPacket rlp = new ZH1104_RequestLinkPacket();
            ZH1104_RequestLinkReplyPacket rlrp = new ZH1104_RequestLinkReplyPacket();
            //合成的报文
            try
            {
                FrameAry[0] = BytesToString(rlp.GetPacketData());
                if (!sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1104Port, rlp, rlrp))
                    {
                        bool linkClockOk = rlrp.ReciveResult == RecvResult.OK;
                        string Clockmessage = string.Format("表联机{0}。", linkClockOk ? "成功" : "失败");
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

        //add yjt 20230103 新增零线电流联机
        /// <summary>
        /// 零线电流联机
        /// </summary>
        /// <returns></returns>
        public int ZeroCurveIbControl()
        {
            //FrameAry = new string[1];
            ZH1104_RequestLinkPacket rlp = new ZH1104_RequestLinkPacket();
            ZH1104_RequestLinkReplyPacket rlrp = new ZH1104_RequestLinkReplyPacket();
            //合成的报文
            try
            {
                //FrameAry[0] = BytesToString(rlp.GetPacketData());
                if (!sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1104Port, rlp, rlrp))
                    {
                        bool linkClockOk = rlrp.ReciveResult == RecvResult.OK;
                        string Clockmessage = string.Format("表联机{0}。", linkClockOk ? "成功" : "失败");
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

        //add yjt 20230103 新增零线电流控制启停
        /// <summary>
        /// 零线电流控制启停
        /// </summary>
        /// <param name="Control"></param>
        /// <returns></returns>
        public int StartZeroCurrent(int A_kz, int BC_kz)
        {
            string[] FrameAry = new string[1];
            ZH1104_StartZeroCurrentLinkPacket rlp = new ZH1104_StartZeroCurrentLinkPacket();
            ZH1104_StartZeroCurrentLinkReplyPacket rlrp = new ZH1104_StartZeroCurrentLinkReplyPacket();
            //合成的报文
            try
            {
                //6801FE0A1340000001A7 启动
                //6801FE0A1340000100A7 关闭
                rlp.SetPara(A_kz, BC_kz);
                FrameAry[0] = BytesToString(rlp.GetPacketData());

                m_ErrorZh1104Port.m_Port_Setting = "38400,n,8,1";
                if (SendPacketWithRetry(m_ErrorZh1104Port, rlp, rlrp))
                {
                    bool linkClockOk = rlrp.ReciveResult == RecvResult.OK;
                    return linkClockOk ? 0 : 1;
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

        /// <summary>
        /// 接地故障试验控制继电器
        /// </summary>
        /// <param name="bwIndex"> 控制板ID号</param>
        /// <param name="Ua">A相电压接地 00-断开  01-闭合</param>
        /// <param name="Ub">B相电压接地 00-断开  01-闭合</param>
        /// <param name="Uc">C相电压接地 00-断开  01-闭合</param>
        /// <param name="Un">N相电压接地 00-断开  01-闭合</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetJDGZContrnl(int bwIndex, int Ua, int Ub, int Uc, int Un, out string[] FrameAry)
        {
            ZH1104_RequestJDGZPacket rc2 = new ZH1104_RequestJDGZPacket(bwIndex, Un);
            ZH1104_RequestJDGZReplyPacket recv2 = new ZH1104_RequestJDGZReplyPacket();

            rc2.SetPara(Ua, Ub, Uc, Un);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1104Port, rc2, recv2))
                    {
                        return recv2.ReciveResult == RecvResult.OK ? 0 : 2;
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
                Thread.Sleep(100);
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
