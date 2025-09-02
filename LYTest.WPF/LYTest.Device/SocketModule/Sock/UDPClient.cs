using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;
using LY.SocketModule.Packet;
using LYTest.Device.SocketModule;


namespace LY.SocketModule.Sock
{
    /// <summary>
    /// UDP端口
    /// </summary>
    internal class UDPClient : IConnection
    {
        private readonly int UdpBindPort;
        private UdpClient Client;
        private UdpClient settingClient;
        //private string szBlt = "1200,e,8,1";
        private IPEndPoint Point = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 10003);
        private readonly IPEndPoint localPoint = null;
        private readonly string m_2018IpAddress = string.Empty;

        private readonly List<byte> recvFrame = new List<byte>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="BindPort">com</param>
        /// <param name="RemotePort">10003,10004</param>
        /// <param name="BasePort">本地起始端口</param>
        public UDPClient(string Ip, int BindPort, int RemotePort, int BasePort)
        {
            m_2018IpAddress = Ip;
            Point.Address = IPAddress.Parse(Ip);
            Point.Port = RemotePort;
            UdpBindPort = LocalPortTo2011Port(BindPort, BasePort);//转换端口成2018端口
            localPoint = new IPEndPoint(IPAddress.Parse(GetHostIp()), UdpBindPort);

        }

        private string GetHostIp()
        {
            string[] ip2018s = m_2018IpAddress.Split('.');

            IPHostEntry Tipentry = Dns.GetHostEntry(Dns.GetHostName());
            string strResult = string.Empty;
            for (int i = 0; i < Tipentry.AddressList.Length; i++)
            {
                string ip = Tipentry.AddressList[i].ToString();
                string[] ipArr = ip.Split('.');
                if (ipArr.Length == 4)
                {
                    if (ip2018s.Length == 4)
                    {
                        if (ipArr[0] == ip2018s[0] && ipArr[1] == ip2018s[1] && ipArr[2] == ip2018s[2])
                        {
                            strResult = ip;
                            break;
                        }
                    }
                    else
                    {
                        strResult = ip;
                        break;
                    }

                }
            }
            return strResult;

        }

        public static uint IPToUint(string ipAddress)
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

        public static string GetSubnetworkIP(string targetIP)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\Tcpip\Parameters\Interfaces", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            uint iTarget = IPToUint(targetIP);
            foreach (string keyName in key.GetSubKeyNames())
            {
                try
                {
                    RegistryKey tmpKey = key.OpenSubKey(keyName);
                    if (!(tmpKey.GetValue("IPAddress") is string[] ip))
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
                catch
                {
                }
            }
            return "127.0.0.1";

        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="vData"></param>
        /// <param name="IsReturn"></param>
        /// <param name="WaiteTime"></param>
        /// <returns></returns>
        public bool SendData(SendPacket sPacke, RecvPacket rPacket)
        {
            recvFrame.Clear();

            try
            {
                lock (this)
                {

                    {
                        Client = new UdpClient();
                        Client.Client.Bind(this.localPoint);
                    }

                }
                Client.Connect(Point);
            }
            catch
            {
                LogHelper.Write(true, PortName, Array.Empty<byte>(), "端口连接失败");
                return false;
            }

            byte[] sendFrame = sPacke.GetPacketData();
            LogHelper.Write(true, PortName, sendFrame);


            Client.Send(sendFrame, sendFrame.Length);

            if (!sPacke.IsNeedReturn)
            {
                LogHelper.Write(false, PortName, recvFrame, "无需回复");
                Client.Close();
                return true;
            }
            Thread.Sleep(100);

            bool ret = false;
            DateTime endT = DateTime.Now.AddSeconds(Timeout);
            while (DateTime.Now.Subtract(endT).Seconds < 0)          //1秒超时器，如果超过表示收不到任何数据，直接退出
            {
                try
                {
                    if (Client.Available > 0)
                    {
                        byte[] bytes = Client.Receive(ref Point);
                        recvFrame.AddRange(bytes);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Write(false, PortName, recvFrame, $"接收数据异常:{ex.Message}");
                    Client.Close();
                    break;
                }


                if (rPacket.ParsePacket(recvFrame))
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(100);
            }
            LogHelper.Write(false, PortName, recvFrame);

            Client.Close();

            return ret;
        }


        /// <summary>
        /// 本地通道转换成2018端口:20000 + 2 * (port - 1);
        /// 数据端口，设置端口在数据端口的基础上+1
        /// </summary>
        /// <param name="port"></param>
        /// <param name="BasePort"></param>
        /// <returns></returns>
        private int LocalPortTo2011Port(int port, int BasePort)
        {
            return BasePort + 2 * (port - 1);
        }


        public string PortName
        {
            get { return Point.ToString(); }
        }

        public int Timeout { get; set; }


        public bool Open()
        {
            return true;
        }

        public bool Close()
        {
            return true;
        }

        /// <summary>
        /// 更新232串口波特率
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool UpdateBaudrate(string setting)
        {
            //szBlt = setting;
            int settingPort = UdpBindPort + 1;

            try
            {
                try
                {
                    settingClient = new UdpClient(settingPort);
                    settingClient.Connect(Point);
                }
                catch { }

                byte[] bytes = Encoding.ASCII.GetBytes("reset");
                LogHelper.Write(true, settingClient.ToString(), bytes, "reset");
                int sendlen = settingClient.Send(bytes, bytes.Length);

                Thread.Sleep(10);
                string tmp = $"init {setting.Replace(',', ' ')}";
                bytes = Encoding.ASCII.GetBytes(tmp);

                LogHelper.Write(true, settingClient.ToString(), bytes, tmp);
                sendlen = settingClient.Send(bytes, bytes.Length);
                settingClient.Close();
                return sendlen == bytes.Length;
            }
            catch { }
            finally
            {
            }
            return false;
        }

    }
}
