using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IConnection
{
    public class UDPClient : IConnection
    {
        private int udpBindPort;
        private UdpClient client;
        private UdpClient settingClient;
        private string szBlt = "9600,e,8,1";
        private IPEndPoint point = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 10003);
        private IPEndPoint localPoint = null;

        public string ConnectName { get => point.ToString(); set { } }
        public int MaxWaitSeconds { get; set; }
        public int WaitSecondsPerByte { get; set; }

        public UDPClient(string Ip, int BindPort, int RemotePort, int BasePort)
        {
            point.Address = IPAddress.Parse(Ip);
            point.Port = RemotePort;
            udpBindPort = BasePort + 2 * (BindPort - 1);
            localPoint = new IPEndPoint(IPAddress.Parse(GetSubnetworkIP(Ip)), udpBindPort);
        }

        public bool Close()
        {
            return true;
        }

        public bool Open()
        {
            return true;
        }

        public bool SendData(ref byte[] vData, bool IsReturn, int WaiteTime)
        {
            try
            {
                lock (this)
                {
                    {
                        client = new UdpClient();
                        client.Client.Bind(localPoint);
                    }
                }
                client.Connect(point);
            }
            catch { return false; }

            client.Send(vData, vData.Length);

            if (!IsReturn)
            {
                client.Close();
                return true;
            }
            Thread.Sleep(WaiteTime);

            List<byte> RevItems = new List<byte>();
            byte[] BytReceived = new byte[0];
            bool IsReveive = false;
            DateTime Dt = DateTime.Now;
            while (DateTime.Now.Subtract(Dt).TotalSeconds < MaxWaitSeconds)
            {
                Thread.Sleep(1);
                try
                {
                    if (client.Available > 0)
                    {
                        BytReceived = client.Receive(ref point);
                        IsReveive = true;
                        break;
                    }
                }
                catch
                {
                    client.Close();
                    return false;
                }
            }

            if (!IsReveive)
            {
                vData = new byte[0];
            }
            else
            {
                RevItems.AddRange(BytReceived);
                Dt = DateTime.Now;
                while (DateTime.Now.Subtract(Dt).TotalMilliseconds < WaitSecondsPerByte)
                {
                    if (client.Available > 0)
                    {
                        BytReceived = client.Receive(ref point);
                        RevItems.AddRange(BytReceived);
                        Dt = DateTime.Now;
                    }
                }
                vData = RevItems.ToArray();
            }

            client.Close();
            return true;
        }

        public bool UpdateSetting(string szSetting)
        {
            szBlt = szSetting;

            try
            {
                try
                {
                    settingClient = new UdpClient(udpBindPort + 1);
                    settingClient.Connect(point);
                }
                catch { }

                string str_Data = "reset";
                byte[] byt_Data = ASCIIEncoding.ASCII.GetBytes(str_Data);
                int sendlen = settingClient.Send(byt_Data, byt_Data.Length);

                System.Threading.Thread.Sleep(10);
                str_Data = "init " + szBlt.Replace(',', ' ');

                byt_Data = ASCIIEncoding.ASCII.GetBytes(str_Data);
                sendlen = settingClient.Send(byt_Data, byt_Data.Length);
                settingClient.Close();
                return sendlen == byt_Data.Length;
            }
            catch { }

            return false;
        }

        public string GetSubnetworkIP(string targetIP)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\Tcpip\Parameters\Interfaces", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            uint iTarget = IPToUint(targetIP);
            foreach (string keyName in key.GetSubKeyNames())
            {
                RegistryKey tmpKey = key.OpenSubKey(keyName);
                string[] ip = tmpKey.GetValue("IPAddress") as string[];
                if (ip == null)
                {
                    continue;
                }
                string[] subnet = tmpKey.GetValue("SubnetMask") as string[];
                for (int i = 0; i < ip.Length; i++)
                {
                    IPAddress local = IPAddress.Parse(ip[i]);
                    if (local.IsIPv6SiteLocal)
                        continue;

                    uint iIP = IPToUint(ip[i]);
                    uint iSub = IPToUint(subnet[i]);

                    if ((iIP & iSub) == (iTarget & iSub))
                    {
                        return ip[i];
                    }
                }
            }
            IPHostEntry IPEntyy = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < IPEntyy.AddressList.Length; i++)
            {
                if (IPEntyy.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    uint iIP = IPToUint(IPEntyy.AddressList[i].ToString());
                    uint iSub = IPToUint("255.255.255.0");

                    if ((iIP & iSub) == (iTarget & iSub))
                    {
                        return IPEntyy.AddressList[i].ToString();
                    }
                }
            }
            return "127.0.0.1";

        }
        public uint IPToUint(string ipAddress)
        {
            string[] strs = ipAddress.Trim().Split('.');
            byte[] buf = new byte[4];

            for (int i = 0; i < strs.Length; i++)
            {
                buf[i] = byte.Parse(strs[i]);
            }
            Array.Reverse(buf);

            return BitConverter.ToUInt32(buf, 0);
        }
    }
}
