using LY3522.Device;
using LY3522.SocketModule.Packet;
using LY3522.Struct;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ZH
{
    public class LY3522
    {

        #region 通用属性方法

        /// <summary>
        /// 控制端口
        /// </summary>
        private readonly StPortInfo m_SourcePort = null;

        private readonly DriverBase driverBase = null;
        //是否发送数据标志
        private readonly bool sendFlag = true;

        bool isInitSetting = false;
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        public LY3522()
        {
            m_SourcePort = new StPortInfo();
            driverBase = new DriverBase();
        }
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
            m_SourcePort.m_Exist = 1;
            m_SourcePort.m_IP = IP;
            m_SourcePort.m_Port = ComNumber;
            m_SourcePort.m_Port_IsUDPorCom = true;
            m_SourcePort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_SourcePort.m_Port_Setting, m_SourcePort.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
                isInitSetting = true;
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
            m_SourcePort.m_Exist = 1;
            m_SourcePort.m_IP = "";
            m_SourcePort.m_Port = ComNumber;
            m_SourcePort.m_Port_IsUDPorCom = false;
            m_SourcePort.m_Port_Setting = "38400,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
                isInitSetting = true;
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
        /// <param name="meterN0">表位号</param>
        /// <param name="FrameAry">出参</param>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            if (!isInitSetting)  //没有初始化，或初始话失败了，就返回失败
            {
                return 1;
            }
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
        #endregion

        //读取所有通道温度帧
        //设置所有加热通道温度帧
        //读取单个通道温度帧
        //设置单个加热通道温度帧
        //设置风扇帧
        //设置电磁锁帧
        //设置指示灯帧----调整为设置电压继电器板

        //命令码  1000，1010
        //读还是写0x10,0x13
        //


        /// <summary>
        ///  获取温度
        /// </summary>
        /// <param name="data">返回数据</param>
        /// <param name="BwNum">表位号--默认全部</param>
        /// <returns></returns>
        public int GetTemperature(out float[] data, byte BwNum = 0xff)
        {
            data = new float[0];
            try
            {
                SendContrnlTypePacket send = new SendContrnlTypePacket();
                RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
                byte[] tmp = new byte[1];
                tmp[0] = BwNum;
                send.SetPara(0x10, "1000", tmp);

                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_SourcePort, send, reslut))
                    {
                        int r = reslut.ReciveResult == RecvResult.OK ? 0 : 1;
                        if (r == 0)
                        {
                            data = (float[])reslut.OutData;
                        }
                        return r;
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
            catch 
            {

                return -1;
            }

        }


        /// <summary>
        ///  设置温度
        /// </summary>
        /// <param name="data">设置的温度</param>
        /// <param name="YesNo">开关</param>
        /// <param name="BwNum">端子号--默认全部--01就是1，02就是2，03就是1和2，0就是关闭全部/param>
        /// <returns></returns>
        public int SetTemperature(float[] data, bool[] YesNo)
        {
            try
            {
                SendContrnlTypePacket send = new SendContrnlTypePacket();
                RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
                List<byte> tmp = new List<byte>();
                for (int i = 0; i < data.Length; i++)
                {
                    tmp.AddRange(PutInt2((int)(data[i] * 100)));
                    tmp.Add((byte)(YesNo[i] ? 0x01 : 0x00));
                }
                send.SetPara(0x13, "1001", tmp.ToArray());
                //if (BwNum == 0xff) //所有的
                //{
                //    send.SetPara(0x02, tmp.ToArray());
                //}
                //else
                //{
                //    tmp[0] = BwNum;
                //    send.SetPara(0x04, tmp.ToArray());
                //}
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_SourcePort, send, reslut))
                    {
                        return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
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
        /// 压入一个2字节int,并将流中当前位置提升2
        /// </summary>
        /// <param name="value"></param>
        private byte[] PutInt2(int value)
        {
            byte[] buffer = new byte[4];
            // buffer[0] = (byte)(value >> 8);
            // buffer[1] = (byte)value;
            //return buffer;
            buffer[0] = (byte)(value >> 0x18);
            buffer[1] = (byte)(value >> 0x10);
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)value;
            return buffer;
        }

        /// <summary>
        /// 风扇
        /// </summary>
        /// <param name="YesNo">开关</param>
        /// <returns></returns>
        public int SetFan(bool YesNo)
        {
            try
            {
                SendContrnlTypePacket send = new SendContrnlTypePacket();
                RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
                byte[] tmp = new byte[1];

                tmp[0] = (byte)(YesNo ? 0x01 : 0x00);
                send.SetPara(0x13, "1002", tmp);
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_SourcePort, send, reslut))
                        return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
                    else
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

    }
}
