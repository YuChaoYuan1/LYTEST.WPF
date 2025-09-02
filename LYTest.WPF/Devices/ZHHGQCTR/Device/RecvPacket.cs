using System;
using System.Collections.Generic;
using System.Text;
using ZHHGQCTR.SocketModel.Packet;

namespace ZHHGQCTR.Device
{
    class HgqRecvPacket : RecvPacket
    {
        public override bool ParsePacket(byte[] buf)
        {
            return true;
            throw new NotImplementedException();
        }
    }
}
