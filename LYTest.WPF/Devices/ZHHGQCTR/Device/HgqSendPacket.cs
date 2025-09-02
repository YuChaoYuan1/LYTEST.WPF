using System;
using System.Collections.Generic;
using System.Text;
using ZHHGQCTR.SocketModel.Packet;

namespace ZHHGQCTR.Device
{
    class HgqSendPacket : SendPacket
    {

        public byte[] PacketDate ;
        public override byte[] GetPacketData()
        {
            return PacketDate;
        }
    }
}
