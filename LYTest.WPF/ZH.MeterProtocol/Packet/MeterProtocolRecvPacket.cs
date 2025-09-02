

namespace LYTest.MeterProtocol.Packet
{
    /// <summary>
    /// 电能表多功能通讯数据包接收类
    /// </summary>
    public class MeterProtocolRecvPacket : SocketModule.Packet.RecvPacket
    {
        public byte[] RecvData { get; set; }
        public override bool ParsePacket(byte[] buf)
        {
            RecvData = buf;
            return true;
        }

        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
}
