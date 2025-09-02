using LYTest.Core;
using System.Collections.Generic;
using LYTest.MeterProtocol.Comm;
using LYTest.MeterProtocol.Device;
using LYTest.MeterProtocol.Enum;
using LYTest.MeterProtocol.Settings;
using LYTest.MeterProtocol.SocketModule;
using LYTest.MeterProtocol.SocketModule.Packet;
using LYTest.MeterProtocol.Struct;

namespace LYTest.MeterProtocol
{

    /// <summary>
    /// 通讯端口类型
    /// </summary>
    public enum PortType
    {

        ZHDevices1 = 0,
        COMM = 1,
        CAN = 2
    }
    /// <summary>
    /// 多功能协议管理
    /// </summary>
    public class MeterProtocolManager : SingletonBase<MeterProtocolManager>
    {
        private readonly Dictionary<int, ComPortInfo> ChannelPortInfo = new Dictionary<int, ComPortInfo>();

        private readonly DriverBase driverBase = new DriverBase();
        /// <summary>
        /// 获取485通道数
        /// </summary>
        /// <returns></returns>
        public int GetChannelCount()
        {
            return DgnConfigManager.Instance.GetChannelCount();
        }

        /// <summary>
        /// 获取指定通道的的端口号
        /// </summary>
        /// <param name="channelId">通道号</param>
        /// <returns></returns>
        public string GetChannelPortName(int channelId)
        {
            if (ChannelPortInfo.ContainsKey(channelId))
            {
                ComPortInfo portInfo = ChannelPortInfo[channelId];

                return GetPortNameByPortNumber(portInfo);
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据端口号获取端口名
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="UDPorCOM">true：UDP false：COM</param>
        /// <returns>端口名</returns>
        private string GetPortNameByPortNumber(ComPortInfo port)
        {
            if (port.LinkType == LinkType.COM)
            {
                return string.Format("COM_{0}", port.Port);
            }
            else if (port.LinkType == LinkType.CAN)
            {
                return port.OtherParams;
            }
            else
            {
                return string.Format("Port_{0}_{1}", port.IP, port.Port);
            }

        }

        /// <summary>
        /// 查询指定通道端口名称
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private string GetPortName(ComPortInfo port)
        {
            if (port.LinkType == LinkType.COM)
            {
                return string.Format("COM_{0}", port.Port);
            }
            else if (port.LinkType == LinkType.CAN)
            {
                return port.OtherParams;
            }
            else if (port.LinkType == LinkType.UDP)
            {
                string strName = "Port_" + port.IP;
                return string.Format("{0}_{1}", strName, port.Port);
            }
            return "";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="irPort">红外头</param>
        public void Initialize(ComPortInfo[] ComPortInfo, ComPortInfo irPort)
        {
            DgnConfigManager.Instance.Load(ComPortInfo, irPort);
            for (int i = 0; i < DgnConfigManager.Instance.GetChannelCount(); i++)
            {
                ComPortInfo port = DgnConfigManager.Instance.GetConfig(i);
                if (ChannelPortInfo.ContainsKey(i + 1))
                {
                    ChannelPortInfo.Remove(i + 1);
                }
                ChannelPortInfo.Add(i + 1, port);

                if (!int.TryParse(port.MaxTimePerByte, out int MaxTimePerByte))
                {
                    MaxTimePerByte = 800;
                }

                if (!int.TryParse(port.MaxTimePerFrame, out int MaxTimePerFrame))
                {
                    MaxTimePerFrame = 3000;
                }

                if (port.LinkType == LinkType.COM)
                {
                    //modify yjt zxg 20220425 反了。调换一下
                    //driverBase.RegisterPort(port.Port, port.Setting, MaxTimePerByte, MaxTimePerFrame);
                    //modify yjt zxg 20220425 反了。调换一下
                    driverBase.RegisterPort(port.Port, port.Setting, MaxTimePerFrame, MaxTimePerByte);
                }
                else
                {
                    //driverBase.RegisterPort(port.Port, port.Setting,port.IP, port.RemotePort, port.StartPort , MaxTimePerByte, MaxTimePerFrame);
                    driverBase.RegisterPort(port.Port, port.Setting, port.IP, port.RemotePort, port.StartPort, MaxTimePerFrame, MaxTimePerByte);
                }
            }
        }

        /// <summary>
        /// 初始化载波的通讯端口
        /// </summary>
        public void InitializeCarrier()
        {
            List<CarrierWareInfo> list = new List<CarrierWareInfo>();
            for (int i = 0; i < App.CarrierInfos.Length; i++)
            {
                if (App.CarrierInfos[i] == null) continue;
                if (!list.Exists(item => item.Port == App.CarrierInfos[i].Port))//判断端口来初始化--这里为了预防以后有多个载波端口
                {
                    list.Add(App.CarrierInfos[i]);
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                int MaxTimePerByte = int.Parse(list[i].ByteTime);  //帧最大时间
                int MaxTimePerFrame = int.Parse(list[i].OutTime);//字节最大时间
                if (list[i].CarrierType == "COM")
                {
                    driverBase.RegisterPort(int.Parse(list[i].Port), list[i].Baudrate, MaxTimePerFrame, MaxTimePerByte);

                }
                else  //服务器的暂时不管他
                {
                    //  driverBase.RegisterPort(port.Port, port.Setting, port.IP, port.RemotePort, port.StartPort, MaxTimePerByte, MaxTimePerFrame);

                }

            }
            
            App.CarrierInfo.IsBroad = true;
            App.CarrierInfo.IsRoute = true;
            if (list.Count > 0)
            {
                App.CarrierInfo.CarrierType = list[0].CarrierType;
                App.CarrierInfo.Name = list[0].Name;
                App.CarrierInfo.RdType = list[0].RdType;
                App.CarrierInfo.CommuType = list[0].CommuType;
                App.CarrierInfo.Baudrate = list[0].Baudrate;
                App.CarrierInfo.Port = list[0].Port;
                App.CarrierInfo.OutTime = list[0].OutTime;
                App.CarrierInfo.ByteTime = list[0].ByteTime;

                App.CarrierInfo.IsBroad = list[0].IsBroad;
                App.CarrierInfo.IsRoute = list[0].IsRoute;
            }


        }
        /// <summary>
        /// 开启或关闭串口监听
        /// </summary>
        /// <param name="OpenAndClose">ture,开启，false,关闭</param>
        public void SetDataReceived(bool OpenAndClose)
        {
            //if (App.g_ChannelType == Cus_ChannelType.通讯载波 || App.g_ChannelType == Cus_ChannelType.通讯无线)
            //{
            string CarrPort = GetPortNameByPortNumber(DgnConfigManager.Instance.GetCarrierPort(App.Carrier_Cur_BwIndex));
            SockPool.Instance.SetDataReceived(CarrPort, OpenAndClose);
            //}
        }
        /// <summary>
        /// 开启或关闭串口监听
        /// </summary>
        /// <param name="OpenAndClose">ture,开启，false,关闭</param>
        public List<object> GetOADList()
        {
            string CarrPort = GetPortNameByPortNumber(DgnConfigManager.Instance.GetCarrierPort(App.Carrier_Cur_BwIndex));
            return SockPool.Instance.GetOADList(CarrPort);
        }

        /// <summary>
        /// 使用指定的端口发送数据包
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="sendPacket">发送包</param>
        /// <param name="recvPacket">回复包,如果不需要回复可以为null</param>
        /// <param name="setting">RS485波特率</param>
        /// <returns>发送是否成功</returns>
        internal bool SendData(string portName, SendPacket sendPacket, RecvPacket recvPacket, string setting)
        {
            bool ret = false;
            //TODO:表协议管理里发送载波报文，645to3762在
            if (App.g_ChannelType == Cus_ChannelType.通讯载波 || App.g_ChannelType == Cus_ChannelType.通讯无线)
            {
                //modify yjt jz 20220822 修改获取端口名
                string CarrPort = GetPortNameByPortNumber(DgnConfigManager.Instance.GetCarrierPort(App.Carrier_Cur_BwIndex));
                SockPool.Instance.UpdatePortSetting(CarrPort, App.CarrierInfo.Baudrate);
                if (sendPacket is Packet.MeterProtocolSendPacket sp && App.CarrierInfo.IsBroad)
                {
                    sp.PacketTo3762(out byte[] out_645F);//376包打完发送了，并直接返回645帧
                    ((Packet.MeterProtocolRecvPacket)recvPacket).RecvData = out_645F;
                    ret = true;
                }
                else
                {
                    //发送前先更新一下端口
                    ret = SockPool.Instance.Send(CarrPort, sendPacket, recvPacket);//这里发送的是初始化载波节点的。
                }
            }
            else if (App.g_ChannelType == Cus_ChannelType.通讯红外)
            {
                ComPortInfo infaredPort = DgnConfigManager.Instance.GetInfaredPort();
                if (infaredPort == null)
                    return ret;
                string strInfraredPort = GetPortName(infaredPort);
                //SockPool.Instance.UpdatePortSetting(portName, "1200,e,8,1");             //发送前先更新一下端口
                SockPool.Instance.UpdatePortSetting(strInfraredPort, infaredPort.Setting);             //发送前先更新一下端口
                ret = SockPool.Instance.Send(strInfraredPort, sendPacket, recvPacket);
            }
            else if (App.g_ChannelType == Cus_ChannelType.通讯485)
            {
                SockPool.Instance.UpdatePortSetting(portName, setting);             //发送前先更新一下端口
                ret = SockPool.Instance.Send(portName, sendPacket, recvPacket);
            }
            else if (App.g_ChannelType == Cus_ChannelType.第二路485)
            {
                ComPortInfo Two485Port = DgnConfigManager.Instance.GetTwo485CpPort(portName);
                string strTwo485Port = GetPortNameByPortNumber(Two485Port);
                SockPool.Instance.UpdatePortSetting(strTwo485Port, Two485Port.Setting);
                ret = SockPool.Instance.Send(strTwo485Port, sendPacket, recvPacket);
            }
            return ret;
        }

    }
}
