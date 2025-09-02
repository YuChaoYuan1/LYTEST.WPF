using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH.Struct;

namespace ZH
{
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch),ComVisible(true)]
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
        [DispId(3)]
        int DisConnect(out string[] FrameAry);

    }

    /// <summary>
    /// 测试板
    /// </summary>
    public class Ainuo_WithstandVoltagePlate : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 误差板控制端口
        /// </summary>
        private StPortInfo m_ErrorZh1104Port =new StPortInfo();

        private DriverBase driverBase = new DriverBase();

        //是否发送数据标志
        //private bool sendFlag = true;
        public int DisConnect(out string[] FrameAry)
        {
            //throw new NotImplementedException();
            FrameAry = new string[0];
            return 0;
        }

        /// <summary>
        /// 初始化端口
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
            m_ErrorZh1104Port.m_Exist = 1;
            m_ErrorZh1104Port.m_IP = IP;
            m_ErrorZh1104Port.m_Port = ComNumber;
            m_ErrorZh1104Port.m_Port_IsUDPorCom = true;
            m_ErrorZh1104Port.m_Port_Setting = "9600,n,8,1";
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

        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_ErrorZh1104Port.m_Exist = 1;
            m_ErrorZh1104Port.m_IP = "";
            m_ErrorZh1104Port.m_Port = ComNumber;
            m_ErrorZh1104Port.m_Port_IsUDPorCom = false;
            m_ErrorZh1104Port.m_Port_Setting = "9600,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "9600,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return 1;
            }
            return 0;
        }
        /// <summary>
        /// 读取当前测试中漏电流采样值
        /// </summary>
        /// <param name="ValueType">30最小值，31最大值，32实时值</param>
        ///<param name="SamplingValue">读取到采样值</param>
        ///<param name="BwNum">表位0x00广播</param>
        /// <returns></returns>
        public int GetSamplingValue(byte ValueType, out float SamplingValue, byte BwNum = 0x00)
        {
            SamplingValue = new float();
            //组帧
            //发送数据给设备
            //解析值

            byte[] data = new byte[1];
            data[0] = ValueType;

            WithstandVoltageInstrument_RequestLinkPacket sgh = new WithstandVoltageInstrument_RequestLinkPacket(0x16, data, BwNum);
            WithstandVoltageInstrument_RequestLinkReplyPacket rgh = new WithstandVoltageInstrument_RequestLinkReplyPacket();

            //sgh.SetPara(ValueType);
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    return 1;
                }
                SamplingValue = float.Parse(rgh.OutData);
            }
            return 0;
        }

        /// <summary>
        /// 设置耐压仪功能状态
        /// </summary>
        /// <param name="ValueType">31开始测试,30停止测试，32复位(继电器释放),显示清零。该命令发送后，需延时2秒再发送后续命令，33加标准采样信号，执行AD校准功能。/param>
        /// <returns></returns>
        public int SetStrat(byte ValueType, byte BwNum = 0x00)
        {
            //组帧
            //发送数据给设备
            //解析值

            byte[] data = new byte[1];
            data[0] = ValueType;

            WithstandVoltageInstrument_RequestLinkPacket sgh = new WithstandVoltageInstrument_RequestLinkPacket(0x13, data, BwNum);
            WithstandVoltageInstrument_RequestLinkReplyPacket rgh = new WithstandVoltageInstrument_RequestLinkReplyPacket();

            //sgh.SetPara(ValueType);
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    return 1;
                }
            }
            return 0;
        }


        /// <summary>
        /// 设置耐压仪测试组合
        /// </summary>
        /// <param name="ValueType">30 对地，31电压对电流，32电流对电流<param>
        /// <returns></returns>
        public int SetTestType(byte ValueType)
        {
            //组帧
            //发送数据给设备
            //解析值

            byte[] data = new byte[1];
            data[0] = ValueType;

            WithstandVoltageInstrument_RequestLinkPacket sgh = new WithstandVoltageInstrument_RequestLinkPacket(0x20, data, 0xEA);
            WithstandVoltageInstrument_RequestLinkReplyPacket rgh = new WithstandVoltageInstrument_RequestLinkReplyPacket();

            //sgh.SetPara(ValueType);
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获得表位漏电流采样值
        /// </summary>
        /// <param name="BwNum">表位编号</param>
        ///<param name="valueType">值类型26最大，27最小，28实时值</param>
        /// <returns></returns>
        public float GetBwValue(byte BwNum, byte valueType)
        {
            byte[] data = new byte[2];
            WithstandVoltageInstrument_RequestLinkPacket sgh = new WithstandVoltageInstrument_RequestLinkPacket(valueType, data, BwNum);
            WithstandVoltageInstrument_RequestLinkReplyPacket rgh = new WithstandVoltageInstrument_RequestLinkReplyPacket();
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    float s = 0f;
                    float.TryParse(rgh.OutData, out s);
                    return s;
                }
            }
            return 0f;
        }
        /// <summary>
        /// 测试时间设置（范围：1-9999秒）0000: 连续测试
        /// </summary>
        /// <param name="time">测试时间,单位秒<param>
        /// <returns></returns>
        public int SetTestTime(int time, byte BwNum = 0x00)
        {
            //组帧
            //发送数据给设备
            //解析值

            //byte[] data = new byte[2];
            //data[0] = ValueType;
            byte[] data = StrToHexByte(time.ToString("x4"));
            WithstandVoltageInstrument_RequestLinkPacket sgh = new WithstandVoltageInstrument_RequestLinkPacket(0x11, data, BwNum);
            WithstandVoltageInstrument_RequestLinkReplyPacket rgh = new WithstandVoltageInstrument_RequestLinkReplyPacket();
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    return 1;
                }
            }
            return 0;
        }
        /// <summary>
        ///漏电流设置（范围：0.01-99.99mA数据扩大100倍）
        /// </summary>
        /// <param name="value"><param>
        /// <returns></returns>
        public int SetLeakageI(int value, byte BwNum = 0x00)
        {
            byte[] data = StrToHexByte(value.ToString("x4"));   //TODO可能需要×100在下发
            //byte[] data = StrToHexByte((value*100).ToString("x4"));   //TODO可能需要×100在下发

            WithstandVoltageInstrument_RequestLinkPacket sgh = new WithstandVoltageInstrument_RequestLinkPacket(0x12, data, BwNum);
            WithstandVoltageInstrument_RequestLinkReplyPacket rgh = new WithstandVoltageInstrument_RequestLinkReplyPacket();
            if (SendPacketWithRetry(m_ErrorZh1104Port, sgh, rgh))
            {
                if (rgh.ReciveResult != RecvResult.OK)
                {
                    return 1;
                }
            }
            return 0;
        }
        /// <summary>
        /// 将16进制的字符串转为byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
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
    }
}
