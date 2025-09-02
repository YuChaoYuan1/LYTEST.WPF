using LYTest.Core;
using System;
using LYTest.MeterProtocol.Comm;
using LYTest.MeterProtocol.Struct;

namespace LYTest.MeterProtocol.Settings
{
    public class DgnConfigManager : SingletonBase<DgnConfigManager>
    {
        private DgnPortInfo[] dicPortConfigs = new DgnPortInfo[0];
        //private readonly string configFilePath = Application.StartupPath + "\\System\\Rs485PortConfig.xml";

        private ComPortInfo[] _Rs485Port1s = new ComPortInfo[0];      //1路485通信串口信息
        private readonly ComPortInfo[] _Rs485Port2s = new ComPortInfo[0];      //2路485通信串口信息
        private ComPortInfo[] _CarrierPorts = new ComPortInfo[0];     //载波通信串口信息
        private ComPortInfo[] _InfraredPorts = new ComPortInfo[0];    //红外通信串口信息



        public int GetChannelCount()
        {
            return _Rs485Port1s.Length;
        }

        public ComPortInfo GetConfig(int index)
        {
            return _Rs485Port1s[index];
        }
        /// <summary>
        /// 获取当前载波端口
        /// </summary>
        /// <returns></returns>
        public ComPortInfo GetCarrierPort(int bwIndex)
        {
            int portNum = 0;

            if (null == App.CarrierInfos[bwIndex])
            {
                LYTest.Utility.Log.LogManager.AddMessage(DateTime.Now.ToString() + "载入载波端口：App.CarrierInfos[" + bwIndex + "]为空", LYTest.Utility.Log.EnumLogSource.检定业务日志, LYTest.Utility.Log.EnumLevel.Information);
            }

            if (null != App.CarrierInfos && App.CarrierInfos.Length > bwIndex && null != App.CarrierInfos[bwIndex])
            {
                int.TryParse(App.CarrierInfos[bwIndex].Port.Replace("COM", ""), out portNum);
            }
            ComPortInfo port = new ComPortInfo
            {
                LinkType = LinkType.COM,
                Port = portNum
            };
            return port;
        }

        /// <summary>
        /// 获取 第 2 路 485 通讯 端口 
        /// </summary>
        /// <returns></returns>
        public ComPortInfo GetTwo485CpPort(string name)
        {
            int count = 5;//App.CUS.Meters.Count;
            string[] arr = name.Split('_');
            int bwh = Convert.ToInt32(arr[arr.Length - 1]);
            if (bwh <= (count / 2))
            {
                return _Rs485Port2s[0];
            }
            else
            {
                if (_Rs485Port2s.Length > 1)
                    return _Rs485Port2s[1];
                else
                    return _Rs485Port2s[0];
            }
        }

        /// <summary>
        /// 获取红外端口信息
        /// </summary>
        /// <returns></returns>
        public ComPortInfo GetInfaredPort()
        {
            return _InfraredPorts[0];
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public bool Load(ComPortInfo[] comPort, ComPortInfo IrPort)
        {

            _Rs485Port1s = new ComPortInfo[comPort.Length];
            _Rs485Port1s = comPort;
            _CarrierPorts = new ComPortInfo[App.CarrierInfos.Length];
            for (int i = 0; i < App.CarrierInfos.Length; i++)
            {
                if (App.CarrierInfos[i] == null) continue;
                _CarrierPorts[i] = new ComPortInfo
                {
                    Port = int.Parse(App.CarrierInfos[i].Port),
                    Setting = App.CarrierInfos[i].Baudrate
                };
            }

            _InfraredPorts = new ComPortInfo[1];
            _InfraredPorts[0] = IrPort;


            return true;
        }




        #region 暂时没有用到
        ///// <summary>
        ///// 保存配置
        ///// </summary>
        ///// <returns></returns>
        //public bool Save()
        //{
        //    using (FileStream fs = new FileStream(configFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        //    {
        //        XmlSerializer serializer = new XmlSerializer(dicPortConfigs.GetType());
        //        serializer.Serialize(fs, dicPortConfigs);
        //        return true;
        //    }
        //}

        /// <summary>
        /// 设置通道数
        /// <paramref name="channelCount">通道数</paramref>
        /// </summary>
        public void SetChannelCount(int channelCount, int baseCom, string IP, string remotePort, int LocalBasePort)
        {
            dicPortConfigs = new DgnPortInfo[channelCount];
            for (int i = 0; i < dicPortConfigs.Length; i++)
            {
                dicPortConfigs[i] = new DgnPortInfo()
                {
                    PortNumber = baseCom + i,
                    PortType = PortType.ZHDevices1,
                    Setting = IP + "|" + remotePort + "|" + LocalBasePort,

                };
            }
        }
        #endregion

    }
}
