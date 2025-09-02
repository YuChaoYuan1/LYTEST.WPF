using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CLOU.SocketModule.Packet;
using CLOU;
using CLOU.Enum;
using CLOU.Struct;

namespace E_CL191B
{

    #region CL191B时基源

    #region CL191B联机指令
    /// <summary>
    /// 191联机操作请求包
    /// 回复:RequestResultReplayPacket
    /// </summary>
    internal class CL191B_RequestLinkPacket : CL191BSendPacket
    {

        public CL191B_RequestLinkPacket()
            : base()
        {
            ToID = 0xBF;
            MyID = 0x20;
        }

        public override string GetPacketName()
        {
            return "CL191B_RequestLinkPacket";
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            byte[] data = new byte[5] { 0xA3, 0x00, 0x00, 0x00, 0xFF };
            buf.Put(data);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 标准表，联机返回指令
    /// </summary>
    internal class CL191B_RequestLinkReplyPacket : CL191BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region CL191B读取GPS时间
    /// <summary>
    /// 191读取GPS时间请求包
    /// 回复:CL191B_RequestReadGPSTimePacket
    /// </summary>
    internal class CL191B_RequestReadGPSTimePacket : CL191BSendPacket
    {

        public CL191B_RequestReadGPSTimePacket()
            : base()
        {
            ToID = 0xBF;
            MyID = 0x20;
        }

        public override string GetPacketName()
        {
            return "CL191B_RequestLinkPacket";
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            byte[] data = new byte[4] { 0xA0, 00, 01, 00 };
            buf.Put(data);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 读取GPS时间回复处理
    /// </summary>
    internal class CL191B_RequestReadGPSTimeReplayPacket : ClouRecvPacket_CLT11
    {
        /// <summary>
        /// GPS时间
        /// </summary>
        public DateTime GPSTime
        {
            get;
            private set;
        }

        public override string GetPacketName()
        {
            return "CL191B_RequestReadGPSTimeReplayPacket";
        }

        protected override void ParseBody(byte[] data)
        {
            ByteBuffer buf = new ByteBuffer(data);
            buf.GetInt();//0x50, 00 01 00
            //后面9个字节是YYYY MMMM DDDD HH MM SS
            int YY = buf.GetUShort_S();
            int MM = buf.GetUShort_S();
            int DD = buf.GetUShort_S();
            int HH = buf.Get();
            int min = buf.Get();
            int SS = buf.Get();
            if (YY == 0 && MM == 0 && DD == 0)
            {
                GPSTime = new DateTime(1, 1, 1, 0, 0, 0);
            }
            else
            {
                GPSTime = new DateTime(YY, MM, DD, HH, min, SS);
            }
        }
    }
    #endregion

    #region CL191B读取GPS温度湿度请求包
    /// <summary>
    /// 读取GPS温度湿度请求包
    /// </summary>
    internal class CL191B_RequestReadTemperatureAndHumidityPacket : CL191BSendPacket
    {

        public CL191B_RequestReadTemperatureAndHumidityPacket()
            : base()
        {
            ToID = 0xBF;
            MyID = 0x20;

        }
        public override string GetPacketName()
        {
            return "CL191B_RequestReadTemperatureAndHumidityPacket";
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            byte[] data = new byte[4] { 0xA0, 00, 03, 00 };
            buf.Put(data);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 读取GPS温度湿度回复包
    /// </summary>
    internal class CL191B_RequestReadTemperatureAndHumidityReplayPacket : ClouRecvPacket_CLT11
    {
        /// <summary>
        /// 温度
        /// </summary>
        public float Tempututer { get; private set; }
        /// <summary>
        /// 湿度
        /// </summary>
        public float Humidity { get; private set; }

        protected override void ParseBody(byte[] data)
        {
            try
            {
                this.Tempututer = BitConverter.ToInt16(data, 0) / 100;
                this.Humidity = BitConverter.ToInt16(data, 2) / 100;
            }
            catch
            {
                this.ReciveResult = RecvResult.DataError;
            }
        }
    }
    #endregion

    #region CL191B设置通道命令
    /// <summary>
    /// 设置191通道请求包
    /// </summary>
    internal class CL191B_RequestSetChannelPacket : CL191BSendPacket
    {
        public CL191B_RequestSetChannelPacket()
            : base()
        {
            ToID = 0xBF;
            MyID = 0x20;

        }
        /// <summary>
        /// 通道类型[0xFF为标准电能脉冲，00为时间脉冲]
        /// </summary>
        public enmStdPulseType channelType = enmStdPulseType.标准电能脉冲;

        public void SetPara(enmStdPulseType Type)
        {
            this.channelType = Type;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xa3);
            buf.Put(0);
            buf.Put(0);
            buf.Put(0);
            if (channelType == enmStdPulseType.标准电能脉冲)
                buf.Put(0xFF);
            else if (channelType == enmStdPulseType.标准时钟脉冲)
                buf.Put(0x00);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// cl191b设置通道，联机返回指令
    /// </summary>
    internal class CL191B_RequestSetChannelReplyPacket : CL191BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 1)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x30)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #endregion

    #region 其它
    /// <summary>
    /// 结论返回
    /// 0x4b:成功
    /// </summary>
    internal class CLNormalRequestResultReplayPacket : ClouRecvPacket_NotCltTwo
    {
        public CLNormalRequestResultReplayPacket()
            : base()
        {
        }
        /// <summary>
        /// 结论
        /// </summary>
        public virtual ReplayCode ReplayResult
        {
            get;
            private set;
        }

        public override string GetPacketName()
        {
            return "CLNormalRequestResultReplayPacket";
        }
        protected override void ParseBody(byte[] data)
        {
            if (data.Length == 2)
                ReplayResult = (ReplayCode)data[1];
            else
                ReplayResult = (ReplayCode)data[0];
        }

        public enum ReplayCode
        {
            /// <summary>
            /// CLT11返回
            /// </summary>
            CLT11OK = 0x30,
            /// <summary>
            /// 响应命令，表示“OK”
            /// </summary>
            Ok = 0x4b,
            /// <summary>
            /// 响应命令，表示出错
            /// </summary>
            Error = 0x33,
            /// <summary>
            /// 响应命令，表示系统估计还要忙若干mS
            /// </summary>
            Busy = 0x35,
            /// <summary>
            /// 误差板联机成功
            /// </summary>
            CL188LinkOk = 0x36,
            /// <summary>
            /// 标准表脱机成功
            /// </summary>
            Cl311UnLinkOk = 0x37
        }
    }
    #endregion
}
