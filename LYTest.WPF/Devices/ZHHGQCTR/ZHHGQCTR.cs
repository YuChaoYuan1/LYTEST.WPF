using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ZHHGQCTR.Device;
using ZHHGQCTR.SocketModel.Packet;
using ZHHGQCTR.Struct;

namespace ZH
{
    public interface IClass_Interface
    {
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
        /// 设置获取请求报文标志
        /// </summary>
        /// <param name="Flag">True:发送报文,并传出报文,false:不发送,只传出报文</param>
        /// <returns></returns>
        [DispId(16)]
        int SetSendFlag(bool Flag);
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName"></param>
        /// <param name="ReFrameAry"></param>
        /// <param name="ReAry"></param>
        /// <returns></returns>
        [DispId(17)]
        int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry);

    }
    public class ZHHGQCTR :IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private StPortInfo m_PowerSourcePort = null;

        private DriverBase driverBase = null;


        /// <summary>
        /// 构造方法
        /// </summary>
        public ZHHGQCTR()
        {
            m_PowerSourcePort = new StPortInfo();
            driverBase = new DriverBase();
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
            m_PowerSourcePort.m_Exist = 1;
            m_PowerSourcePort.m_IP = "";
            m_PowerSourcePort.m_Port = ComNumber;
            m_PowerSourcePort.m_Port_IsUDPorCom = false;
            m_PowerSourcePort.m_Port_Setting = "19200,n,8,1";
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


        public int Connect(out string[] FrameAry)
        {

            FrameAry = new string[1];
            return 0;
        }

        public int DisConnect(out string[] FrameAry)
        {
            throw new NotImplementedException();
        }



        public int readHarmonicEnergy(out float[] flaHarmonic, out string[] FrameAry)
        {
            throw new NotImplementedException();
        }

        public int SetSendFlag(bool Flag)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 切到互感式
        /// </summary>
        /// <returns></returns>
        public int Set_HGQ()
        {
            int[] resoult = new int[4];
            byte[] data;
            data = new byte[] { 0x01, 0x05, 0x00, 0x01, 0x00, 0x00, 0x9c, 0x0a };
            resoult[0]=sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x00, 0xff, 0x00, 0x8c, 0x3a };
            resoult[1] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x03, 0x00, 0x00, 0x3d, 0xca };
            resoult[2] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x02, 0xff, 0x00, 0x2d, 0xfa };
            resoult[3] = sendData(data);

            if (Array.IndexOf(resoult,1)>=0)
            {
                return 1;
            }
            return 0;

            //data = new byte[] { 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0xcd, 0xca };
            //sendData(data);

            //data = new byte[] { 0x01, 0x05, 0x00, 0x02, 0x00, 0x00, 0x6c, 0x0a };
            //sendData(data);
        }

        /// <summary>
        /// 切到直接式
        /// </summary>
        /// <returns></returns>
        public int Set_ZJ()
        {
            int[] resoult = new int[4];
            byte[] data;
            data = new byte[] { 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0xcd, 0xca };
            resoult[0] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x01, 0xff, 0x00, 0xdd, 0xfa };
            resoult[1] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x02, 0x00, 0x00, 0x6c, 0x0a };
            resoult[2] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x03, 0xff, 0x00, 0x7c, 0x3a };
            resoult[3] = sendData(data);
            if (Array.IndexOf(resoult, 1) >= 0)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 全部关闭，每次开启60秒左右发送个关闭命令
        /// </summary>
        /// <returns></returns>
        public int Set_Off()
        {
            int[] resoult = new int[4];
            byte[] data;
            data = new byte[] { 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0xcd, 0xca };
            resoult[0] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x02, 0x00, 0x00, 0x6c, 0x0a };
            resoult[1] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x01, 0x00, 0x00, 0x9c, 0x0a };
            resoult[2] = sendData(data);
            data = new byte[] { 0x01, 0x05, 0x00, 0x03, 0x00, 0x00, 0x3d, 0xca };
            resoult[3] = sendData(data);
            if (Array.IndexOf(resoult, 1) >= 0)
            {
                return 1;
            }
            return 0;
        }

        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 处理指令类
        /// </summary>
        /// <param name="cmd">命令码</param>
        /// <param name="RevbyteData"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        private int sendData(byte[] senData)
        {
            //SendPacket sendPacket = new SendPacket();

            HgqSendPacket rc3 = new HgqSendPacket();
            HgqRecvPacket recv3 = new HgqRecvPacket();
            rc3.IsNeedReturn = true;
            rc3.PacketDate = senData;
            if (SendPacketWithRetry(m_PowerSourcePort, rc3, recv3))
            {
                //RevbyteData = recv3.revbyteData;
            }
            else
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
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket   sp, RecvPacket rp)
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
    }
}
