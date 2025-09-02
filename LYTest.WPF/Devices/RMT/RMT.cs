using RMT;
using System;

namespace ZH
{
    public class RMT
    {
        private PortInfo Port = null;
        IConnection connection = null;

        public RMT()
        {
            Port = new PortInfo();
        }

        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int remotePort, int basePort)
        {
            Port.Exist = 1;
            Port.IP = IP;
            Port.Port = ComNumber;
            Port.IsUDP = true;
            Port.Setting = "38400,n,8,1";
            try
            {
                connection = new UDPClient(Port.IP, ComNumber, remotePort, basePort);
                connection.MaxWaitSeconds = MaxWaitTime;
                connection.WaitSecondsPerByte = WaitSencondsPerByte;
                connection.UpdateSetting(Port.Setting);
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }
        public void Remote_Supply(bool On)
        {
            byte[] data = { 0x81, 0x21, 0x01, 0x08, 0x08, 0xFF, 0x00, 0xDF };
            if (On)
            {
                data[6] = 0x01;
                data[7] = 0xDE;
            }
            if (connection != null)
            {
                connection.UpdateSetting(Port.Setting);
                connection.SendData(ref data, false, 200);
            }
        }
    }
}
