using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IConnection;

namespace ZH
{
    public class CT20303C
    {
        private PortInfo Port = null;
        private IConnection.IConnection connection = null;
        public CT20303C()
        {
            Port = new PortInfo();
        }
        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int remotePort, int basePort)
        {
            try
            {
                Port.Exist = 1;
                Port.IP = IP;
                Port.Port = ComNumber;
                Port.IsUDP = true;
                Port.Setting = "38400,n,8,1";
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
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            return 5;
        }
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            bool result = false;
            try
            {
                byte[] data = { 0x81, 0x30, 0x01, 0x08, 0xC1, 0x01, 0x00, 0xF9 };
                if (connection != null)
                {
                    connection.UpdateSetting(Port.Setting);
                    result = connection.SendData(ref data, true, 200);
                    if (result)
                    {
                        if (data == null || data.Length < 6)
                        {
                            result = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return result ? 0 : 1;
        }
        public int SetRange(int wiring, double current)
        {
            try
            {
                byte[] data = { 0x81, 0x30, 0x01, 0x08, 0xC3, 0x01, 0x00, 0xFB };

                if (wiring == 1)//三相三线
                {
                    data[4] = 0xC5;
                    data[5] = 0x01;
                    data[6] = 0x33;
                    data[7] = 0xCE;
                }
                else
                {
                    data[4] = 0xC5;
                    data[5] = 0x01;
                    data[6] = 0x34;
                    data[7] = 0xC9;
                }
                if (connection != null)
                {
                    connection.UpdateSetting(Port.Setting);
                    connection.SendData(ref data, true, 200);
                }
                System.Threading.Thread.Sleep(10);

                data = new byte[] { 0x81, 0x30, 0x01, 0x08, 0xC3, 0x01, 0x00, 0xFB };
                if (current <= 2)
                {
                    data[4] = 0xC3;
                    data[5] = 0x01;
                    data[6] = 0x01;
                    data[7] = 0xFA;
                }
                else
                {
                    data[4] = 0xC3;
                    data[5] = 0x01;
                    data[6] = 0x00;
                    data[7] = 0xFB;
                }
                if (connection != null)
                {
                    connection.UpdateSetting(Port.Setting);
                    connection.SendData(ref data, true, 200);
                }
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception)
            {
                return -1;
            }
            return 0;
        }
    }
}
