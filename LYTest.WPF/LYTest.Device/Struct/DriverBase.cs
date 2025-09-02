using LY.SocketModule;
using LY.SocketModule.Packet;

namespace LYTest.Device.Struct
{
    public class DriverBase
    {
        /// <summary>
        /// 端口信息类
        /// </summary>
        private readonly StPortInfo port = new StPortInfo();

        public DriverBase()
        {

        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        public bool SendPacketWithRetry(SendPacket sp, RecvPacket rp)
        {
            sp.IsNeedReturn = true;
            return SendData(port, sp, rp);

        }


        /// <summary>
        /// 发送命令 不带返回
        /// </summary>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        public bool SendPacketNotRevWithRetry(SendPacket sp, RecvPacket rp)
        {
            sp.IsNeedReturn = false;
            return SendData(port, sp, rp);
        }


        ///// <summary>
        ///// 根据端口号、IP地址获取唯一端口名称
        ///// </summary>
        ///// <param name="port">端口号</param>
        ///// <param name="UDPorCOM">true UDP,false COM</param>
        ///// <param name="ip">IP isNullOrEmpty COM ,other wise UDP/TCP</param>
        ///// <returns></returns>
        //private string GetPortNameByPortNumber(int port, bool UDPorCOM, string ip)
        //{
        //    if (false == UDPorCOM)
        //    {
        //        return $"COM_{port}";
        //    }
        //    else
        //    {
        //        return $"Port_{ip}_{port}";
        //    }
        //}

        /// <summary>
        /// 初始化2018端口
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="setting">波特率</param>
        /// <param name="IP">Ip地址</param>
        /// <param name="remotePort">远程端口</param>
        /// <param name="localStartPoet">本地端口</param>
        /// <param name="timeout">帧命令最长等待时间</param>
        public void RegisterPort(int port, string setting, string IP, int remotePort, int localStartPoet, int timeout)
        {
            //this.port.IsExist = 1;
            this.port.IP = IP;
            this.port.Port = port;
            this.port.IsUdp = true;
            this.port.Setting = setting;

            System.Net.IPAddress ipa = System.Net.IPAddress.Parse(IP);
            string portName = this.port.ToString();
            //注册数据端口
            SockPool.Instance.AddUdpSock(portName, ipa, remotePort, port, localStartPoet, timeout);

            SockPool.Instance.UpdatePortSetting(portName, setting);
        }


        /// <summary>
        /// 注册端口[串口]
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="setting">串口设置</param>
        public void RegisterPort(int port, string setting, int timeout)
        {
            this.port.IP = "";
            this.port.Port = port;
            this.port.IsUdp = false;
            this.port.Setting = setting;

            string portName = this.port.ToString();
            //注册设置端口
            SockPool.Instance.AddComSock(portName, port, setting, timeout);
            SockPool.Instance.UpdatePortSetting(portName, setting);
        }


        /// <summary>
        /// UDP发送,重新初始化波特率
        /// </summary>
        ///<param name="port"></param>
        /// <param name="sendPacket"></param>
        /// <param name="recvPacket"></param>
        /// <returns></returns>
        private bool SendData(StPortInfo port, SendPacket sendPacket, RecvPacket recvPacket)
        {
            string portName = port.ToString();
            if (!string.IsNullOrEmpty(port.Setting.Trim()))
            {
                SockPool.Instance.UpdatePortSetting(portName, port.Setting);
            }
            return SockPool.Instance.Send(portName, sendPacket, recvPacket);
        }


    }
}
