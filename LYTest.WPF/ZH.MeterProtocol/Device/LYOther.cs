using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Device
{
    /// <summary>
    /// 结论返回
    /// 0x4b:成功
    /// </summary>
    internal class CLNormalRequestResultReplayPacket : ZHRecvPacket_NotCltTwo
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

    /// <summary>
    /// CLT11结果返回
    /// </summary>
    internal class Clt11RequestResultReplayPacket : ZHRecvPacket_CLT11
    {
        public Clt11RequestResultReplayPacket() : base(0xFF, 0xFF) { }
        public CLNormalRequestResultReplayPacket.ReplayCode ReplayResult
        {
            get;
            set;
        }

        protected override void ParseBody(byte[] data)
        {
            if (data.Length != 1)
                throw new ArgumentOutOfRangeException("parsebody data");
            else
                ReplayResult = (CLNormalRequestResultReplayPacket.ReplayCode)data[0];
        }
    }


    internal class Comm_SendPacket : SendPacket_Nothing
    {
        public Comm_SendPacket()
        {
            IsNeedReturn = true;
        }
        public Comm_SendPacket(bool isNeedReturn)
        {
            IsNeedReturn = isNeedReturn;
        }
        /// <summary>
        /// 已重写
        /// </summary>
        /// <returns></returns>
        public override byte[] GetPacketData()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();

            byte[] body = GetBody();

            buf.Put(body);

            return buf.ToByteArray();
        }
        public byte[] bytSendByte = null;

        protected override byte[] GetBody()
        {
            return bytSendByte;
        }

        /// <summary>
        /// 发送后等待返回时间（ms）
        /// </summary>
        /// <returns></returns>
        public override int WaiteTime()
        {
            return 3000;
        }
    }


    /// <summary>
    /// 接收基类
    /// </summary>
    internal class Comm_RecvPacket : RecvPacket_Nothing
    {
        public byte[] RecvData { get; private set; }

        public override bool ParsePacket(byte[] data)
        {
            ByteBuffer buf = new ByteBuffer(data);

            RecvData = buf.GetByteArray(data.Length);

            return true;
        }
        protected override void ParseBody(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
