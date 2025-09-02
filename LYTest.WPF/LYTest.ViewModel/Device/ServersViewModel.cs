
using LYTest.ViewModel.Model;

namespace LYTest.ViewModel.Device
{
    /// 串口服务器视图模型
    /// <summary>
    /// 串口服务器视图模型
    /// </summary>
    public class ServersViewModel : ViewModelBase
    {
        private static ServersViewModel instance = null;
        public static ServersViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServersViewModel();
                }
                return instance;
            }
        }

        private AsyncObservableCollection<ComServerViewModel> servers = new AsyncObservableCollection<ComServerViewModel>();

        public AsyncObservableCollection<ComServerViewModel> Servers
        {
            get { return servers; }
            set { SetPropertyValue(value, ref servers, "Servers"); }
        }

        /// <summary>
        /// 注册地址为comAddress的服务器
        /// </summary>
        /// <param name="comAddress"></param>
        public ComServerViewModel Register(string comAddress, string remotePort, string startPort)
        {
            string addressTemp = string.Format("{0}_{1}_{2}", comAddress, remotePort, startPort);
            if (comAddress == "COM")
            {
                addressTemp = "COM";
            }
            //由于多线程往AsyncObservableCollection<T>里面添加数据不能立即执行,所以引入了一个中间量
            for (int i = 0; i < Servers.Count; i++)
            {
                if (Servers[i].ServerAddress == addressTemp)
                {
                    return Servers[i];
                }
            }
            ComServerViewModel server = new ComServerViewModel
            {
                ServerAddress = addressTemp,
                Index = Servers.Count
            };
            Servers.Add(server);
            return server;
        }

        /// <summary>
        /// 注册地址为comAddress的服务器
        /// </summary>
        /// <param name="fullName"></param>
        public PortUnit FindPort(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }
            string[] arrayInfor = fullName.Split('_');
            if (arrayInfor.Length == 2)
            {
                for (int i = 0; i < Servers.Count; i++)
                {
                    if (Servers[i].ServerAddress == arrayInfor[0])
                    {
                        for (int j = 0; j < Servers[i].Units.Count; j++)
                        {
                            if (Servers[i].Units[j].Index.ToString() == arrayInfor[1])
                            {
                                return Servers[i].Units[j];
                            }
                        }
                    }
                }
            }
            else
            {
                if (arrayInfor.Length != 4)
                {
                    return null;
                }
                string addressTemp = string.Format("{0}_{1}_{2}", arrayInfor[0], arrayInfor[1], arrayInfor[2]);
                for (int i = 0; i < Servers.Count; i++)
                {
                    if (Servers[i].ServerAddress == addressTemp)
                    {
                        for (int j = 0; j < Servers[i].Units.Count; j++)
                        {
                            if (Servers[i].Units[j].Index.ToString() == arrayInfor[3])
                            {
                                return Servers[i].Units[j];
                            }
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 查找表端口
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="portNo"></param>
        /// <returns></returns>
        public PortUnit FindMeterPort(string ipAddress, string portNo)
        {
            for (int i = 0; i < Servers.Count; i++)
            {
                string[] arrayAddress = Servers[i].ServerAddress.Split('_');
                if (arrayAddress.Length == 3 && arrayAddress[0] == ipAddress)
                {
                    for (int j = 0; j < Servers[i].Units.Count; j++)
                    {
                        if (Servers[i].Units[j].Index.ToString() == portNo)
                        {
                            return Servers[i].Units[j];
                        }
                    }
                }
            }
            return null;
        }

        ///// 加载表位端口
        ///// <summary>
        ///// 加载表位端口
        ///// </summary>
        //public void LoadMeterPort()
        //{
        //    int meterIndex = 0;
        //    for (int i = 0; i < EquipmentData.DeviceManager.MeterPorts.Count; i++)
        //    {
        //        string meterConfig = EquipmentData.DeviceManager.MeterPorts[i];
        //        #region 合法性校验
        //        string[] arrayPort = meterConfig.Split('|');
        //        if (arrayPort.Length != 8)
        //        {
        //            continue;
        //        }
        //        int meterCount = 0;
        //        int startComno = 0;
        //        int comnoInterval = 0;
        //        int maxTimePerFrame = 0;
        //        int maxTimePerByte = 0;
        //        if (!int.TryParse(arrayPort[1], out meterCount) || !int.TryParse(arrayPort[2], out startComno) || !int.TryParse(arrayPort[3], out comnoInterval) || !int.TryParse(arrayPort[6], out maxTimePerFrame) || !int.TryParse(arrayPort[7], out maxTimePerByte))
        //        {
        //            continue;
        //        }
        //        string baudrate = arrayPort[4];
        //        string[] serverArray = arrayPort[5].Split('_');
        //        if (serverArray.Length != 3)
        //        {
        //            continue;
        //        }
        //        string address = serverArray[0];
        //        int startPort = 0;
        //        int remotePort = 0;
        //        if (!int.TryParse(serverArray[2], out startPort) || !int.TryParse(serverArray[1], out remotePort))
        //        {
        //            continue;
        //        }
        //        #endregion

        //        ComServerViewModel server = Register(address, serverArray[1], serverArray[2]);

        //        #region 构造表端口
        //        if (server != null)
        //        {
        //            for (int j = 0; j < meterCount; j++)
        //            {
        //                int noTemp = startComno + j * comnoInterval;
        //                if (server.Units.Count >= noTemp && noTemp > 0)
        //                {
        //                    server.Units[noTemp - 1].Name = arrayPort[0];
        //                    server.Units[noTemp - 1].DeviceIndex = meterIndex;
        //                    meterIndex++;
        //                }
        //            }
        //        }
        //        #endregion

        //    }
        //}
    }
}
