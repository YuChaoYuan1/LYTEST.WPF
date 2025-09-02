using LY.SocketModule.Packet;
using System.Net;
using System.Threading;
namespace LY.SocketModule.Sock
{
    /// <summary>
    /// 与台体的通讯连接
    /// </summary>
    internal class Connection
    {
        private readonly object objLock = new object();

        /// <summary>
        /// 连接对象
        /// </summary>
        readonly IConnection connection = null;

        /// <summary>
        /// 初始化为UDP连接，并打开连接
        /// </summary>
        /// <param name="remoteIp">远程服务器IP</param>
        /// <param name="remotePort">远程服务器端口</param>
        /// <param name="localPort">本地监听端口</param>
        /// <param name="basePort">本地监听端口</param>
        /// <param name="timeout">指示最大等待时间</param>
        public Connection(IPAddress remoteIp, int remotePort, int localPort, int basePort, int timeout)
        {
            connection = new UDPClient(remoteIp.ToString().Split(':')[0], localPort, remotePort, basePort)
            {
                Timeout = timeout
            };
        }

        /// <summary>
        /// 初始化为COM连接，并打开连接
        /// </summary>
        /// <param name="port">COM端口</param>
        /// <param name="baudrate">波特率字符串，如：1200,e,8,1</param>
        /// <param name="timeout">指示最大等待时间</param>
        public Connection(int port, string baudrate, int timeout)
        {
            connection = new COM32(baudrate, port)
            {
                Timeout = timeout
            };
        }

        /// <summary>
        /// 更新端口对应的COMM口波特率参数
        /// </summary>
        /// <param name="setting">要更新的波特率</param>
        /// <returns>更新是否成功</returns>
        public bool UpdatePortSetting(string setting)
        {
            connection?.UpdateBaudrate(setting);
            return true;
        }

        /// <summary>
        /// 发送并且接收返回数据
        /// </summary>
        /// <param name="sendPack">发送数据包</param>
        /// <param name="recvPack">接收数据包</param>
        /// <returns></returns>
        public bool Send(SendPacket sendPack, RecvPacket recvPack)
        {
            if (connection == null) return false;

            lock (objLock)
            {
                //connection.Timeout = sendPack.WaiteTime();

                bool ret = false;
                for (int i = 0; i < 3; i++)
                {
                    ret = connection.SendData(sendPack, recvPack);
                    if (ret) break;
                    Thread.Sleep(1000);
                }

                return ret;
            }
        }


    }
}
