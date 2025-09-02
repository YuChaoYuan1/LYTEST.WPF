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

        /// <summary>
        /// 偶次谐波设置
        /// </summary>
        /// <param name="HarmonicA">A相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="HarmonicB">B相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="HarmonicC">C相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        int SetZH1106Controlboard(string HarmonicA, string HarmonicB, string HarmonicC, out string[] FrameAry);

        /// <summary>
        ///  读取 偶次谐波电流采样值
        /// </summary>
        /// <param name="floatCurrent"> A相正半波电流值 + A相负半波电流值 + B相正半波电流值 + B相负半波电流值 + C相正半波电流值 + C相负半波电流值</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        int ReadZH1106Data(out float[] floatCurrent, out string[] FrameAry);
    }

    public class ZH1106 : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private StPortInfo m_ErrorZH1106Port = null;

        private DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZH1106()
        {
            m_ErrorZH1106Port = new StPortInfo();
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
            m_ErrorZH1106Port.m_Exist = 1;
            m_ErrorZH1106Port.m_IP = IP;
            m_ErrorZH1106Port.m_Port = ComNumber;
            m_ErrorZH1106Port.m_Port_IsUDPorCom = true;
            m_ErrorZH1106Port.m_Port_Setting = "19200,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_ErrorZH1106Port.m_Port_Setting, m_ErrorZH1106Port.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
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
            m_ErrorZH1106Port.m_Exist = 1;
            m_ErrorZH1106Port.m_IP = "";
            m_ErrorZH1106Port.m_Port = ComNumber;
            m_ErrorZH1106Port.m_Port_IsUDPorCom = false;
            m_ErrorZH1106Port.m_Port_Setting = "38400,n,8,1";
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
        /// 连接
        /// </summary>
        /// <param name="FrameAry">出参</param>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            ZH1106ReadSwitchPacket rc2 = new ZH1106ReadSwitchPacket();
            ZH1106ReadSwitchReplayPacket recv2 = new ZH1106ReadSwitchReplayPacket();
            FrameAry = new string[1];
            int ReValue = 0;
            try
            {

                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZH1106Port, rc2, recv2))
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

        //SetZH1106Controlboard
        /// <summary>
        /// 偶次谐波设置
        /// </summary>
        /// <param name="HarmonicA">A相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="HarmonicB">B相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="HarmonicC">C相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetZH1106Controlboard(string HarmonicA, string HarmonicB, string HarmonicC, out string[] FrameAry)
        {
            ZH1106SwitchPacket rc2 = new ZH1106SwitchPacket();
            ZH1106SwitchReplayPacket recv2 = new ZH1106SwitchReplayPacket();
            FrameAry = new string[1];
            int ReValue = 0;
            try
            {
                rc2.SetPara(HarmonicA, HarmonicB, HarmonicC);
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZH1106Port, rc2, recv2))
                    {
                        ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
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
        ///  读取 偶次谐波电流采样值
        /// </summary>
        /// <param name="floatCurrent"> A相正半波电流值 + A相负半波电流值 + B相正半波电流值 + B相负半波电流值 + C相正半波电流值 + C相负半波电流值</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadZH1106Data(out float[] floatCurrent, out string[] FrameAry)
        {
            floatCurrent = new float[6];

            ZH1106ReadSwitchPacket rc = new ZH1106ReadSwitchPacket();
            ZH1106ReadSwitchReplayPacket recv = new ZH1106ReadSwitchReplayPacket();

            FrameAry = new string[1];
            int ReValue = 0;
            try
            {

                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZH1106Port, rc, recv))
                    {
                        ReValue = recv.ReciveResult == RecvResult.OK ? 0 : 1;
                        floatCurrent = recv.floatcurrent;
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
