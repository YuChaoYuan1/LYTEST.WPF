using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.ViewModel.ProtConfig
{
    /// <summary>
    /// 服务器端口配置
    /// </summary>
    public class PortConfigViewModel : ViewModelBase
    {
        public PortConfigViewModel()
        {
            LoadConfig();  //载入配置
        }

        #region 串口服务器列表
        private AsyncObservableCollection<ServerItem> servers = new AsyncObservableCollection<ServerItem>();
        /// <summary>
        /// 串口服务器列表
        /// </summary>
        public AsyncObservableCollection<ServerItem> Servers
        {
            get { return servers; }
            set { SetPropertyValue(value, ref servers, "Servers"); }
        }
        #endregion


        #region 设备列表--

        /// <summary>
        /// 所有设备列表
        /// </summary>
        public AsyncObservableCollection<DeviceGroup> Groups { get; } = new AsyncObservableCollection<DeviceGroup>();

        /// <summary>
        /// 型号列表
        /// </summary>
        public AsyncObservableCollection<string> ModelItems { get; } = new AsyncObservableCollection<string>();


        /// <summary>
        /// 表位端口
        /// </summary>
        public AsyncObservableCollection<MeterItems> MeterItem { get; } = new AsyncObservableCollection<MeterItems>();
        #endregion

        /// <summary>
        /// 串口参数
        /// </summary>
        public AsyncObservableCollection<string> ComParams { get; } = new AsyncObservableCollection<string>();



        /// <summary>
        /// 载入配置
        /// </summary>
        public void LoadConfig()
        {
            #region 加载服务器列表

            Servers.Clear();//清空服务器列表
            Dictionary<string, string> dictionaryServers = CodeDictionary.GetLayer2("PortServerType");
            int index = 1;
            for (int i = 0; i < dictionaryServers.Count; i++)
            {
                ServerItem server = new ServerItem();
                if (dictionaryServers.Keys.ElementAt(i) != "COM")
                {
                    server.Name = string.Format("服务器端口{0}", index++);
                    server.Address = dictionaryServers.Keys.ElementAt(i);
                }
                else  //com口
                {
                    server.Name = string.Format("COM");
                    server.Address = "127.0.0.1_5001_5001";
                }
                server.FlagChanged = false;
                Servers.Add(server);
            }

            #endregion

            #region 初始化串口参数
            ComParams.Clear();
            Dictionary<string, string> dictionaryComPars = CodeDictionary.GetLayer2("SerialPortPara");
            for (int i = 0; i < dictionaryComPars.Count; i++)
            {
                ComParams.Add(dictionaryComPars.Keys.ElementAt(i));
            }
            #endregion

            //名称  服务器编号  设备型号   串口参数  开始端口 端口间隔  端口数量   帧最大间隔  字节最大间隔  是否编辑  删除
            //误差版   服务器1  ZH311     192.168.0.1  1 12 1 100 100 否  删除
            #region 加载设备--误差版、功率源、标准表、485通道(485是连续的，所以只需要开始结束就好)

            //数据源应该是设备--误差板-功率源-标准表----每个设备的型号
            //先获得树结构中设备列表，创建
            //每个设备的型号列表是数结构中对应的项


            Groups.Clear();
            //先获得了设备列表--这里只会读取设备列表里面打勾的
            Dictionary<string, string> EquipmentList = CodeDictionary.GetLayer2("EquipmentType");

            //先判断数据库中是否有这个

            //string devideId = dictionaryConfig["设备类型"];
            string[] name = EquipmentList.Keys.ToArray();
            for (int i = 0; i < EquipmentList.Count; i++)
            {
                //添加设备列表
                List<string> ModelTem = CodeDictionary.GetLayer2(CodeDictionary.GetCodeLayer1(name[i])).Keys.ToList();
                for (int x = 0; x < ModelTem.Count; x++)
                {
                    if (!ModelItems.Contains(ModelTem[x]))
                    {
                        ModelItems.Add(ModelTem[x]);
                    }
                }


                //在数据库中找到对应的数据
                DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), string.Format("DEVICE_NAME = '{0}'", name[i]));
                //【标注：这里应该加上没有找到的情况向数据库中插入一条数据】
                if (model == null)
                {
                    model = new DynamicModel();
                    model.SetProperty("DEVICE_NAME", name[i]);
                    model.SetProperty("DEVICE_ENABLED", "0");
                    model.SetProperty("DEVICE_DATA", "192.168.0.1_10005_20000|ZH311|38400,n,8,1|32|3000|10");
                    DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), model);
                    model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), string.Format("DEVICE_NAME = '{0}'", name[i]));
                }

                if (model == null) continue;

                DeviceGroup device = new DeviceGroup
                {
                    Name = name[i],
                    IsExist = model.GetProperty("DEVICE_ENABLED").ToString() == "1" //是否可以用--就是掐前面的勾打不打上-
                };
                string[] DeviceItemData = model.GetProperty("DEVICE_DATA").ToString().Split('$');//根据$分割，获得该设备配置集合

                for (int j = 0; j < DeviceItemData.Length; j++)
                {
                    string[] data = DeviceItemData[j].Split('|');//获得串口数据
                    if (data.Length < 6) continue;

                    string comAddress = data[0];
                    ServerItem serverTemp = Servers.FirstOrDefault(item => item.Address == comAddress);
                    if (serverTemp == null)
                        serverTemp = Servers[0];

                    DeviceItem deviceTemp = new DeviceItem
                    {
                        Server = serverTemp,
                        Model = data[1],
                        ComParam = data[2],
                        PortNum = data[3],
                        MaxTimePerFrame = data[4],
                        MaxTimePerByte = data[5],
                        IsLink = true,
                        FlagChanged = false
                    };

                    device.DeviceItems.Add(deviceTemp);
                }

                Groups.Add(device);
            }


            #endregion

            #region 加载表位端口--485
            MeterItem.Clear();

            DynamicModel Meter_model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), string.Format("DEVICE_NAME = '{0}'", "电能表"));
            if (Meter_model != null)
            {
                string[] MeterItemData = Meter_model.GetProperty("DEVICE_DATA").ToString().Split('$');///根据$分割
                for (int j = 0; j < MeterItemData.Length; j++)
                {
                    string[] data = MeterItemData[j].Split('|');//获得串口数据
                    MeterItems meterItems = new MeterItems();
                    if (data.Length > 6)
                    {
                        //服务器名
                        string comAddress = data[0];
                        ServerItem serverTemp = Servers.FirstOrDefault(item => item.Address == comAddress);
                        if (serverTemp == null)
                            meterItems.Server = Servers[0];
                        else
                            meterItems.Server = serverTemp;
                        //   < !--服务器编号   串口参数 开始端口 端口间隔 端口数量   帧最大间隔 字节最大间隔  是否编辑 删除-->
                        meterItems.ComParam = data[1];
                        meterItems.StartPort = data[2];
                        meterItems.IntervalValue = data[3];
                        meterItems.PortCount = data[4];
                        meterItems.MaxTimePerFrame = data[5];
                        meterItems.MaxTimePerByte = data[6];
                        meterItems.FlagChanged = false;
                        MeterItem.Add(meterItems);
                    }
                }
            }

            #endregion
        }


        #region 保存配置
        private static byte doing = 0;
        public void SavePortConfig()
        {
            System.Threading.Thread.Sleep(1);
            ++doing;
            if (doing > 1) return;

            //保存设备列表
            Device_Save();
            //保存电能表
            Meter_Save();

            //LogManager.AddMessage(string.Format("保存端口配置成功"), EnumLogSource.用户操作日志, EnumLevel.Tip);
            System.Threading.Tasks.Task.Delay(250).ContinueWith(t => System.Threading.Tasks.Task.Run(() => { doing = 0; }));
        }
        /// <summary>
        /// 保存设备信息
        /// </summary>
        private void Device_Save()
        {
            for (int i = 0; i < Groups.Count; i++)   //所有设备
            {
                DynamicModel modelTemp = new DynamicModel();
                int IsEnabled = Groups[i].IsExist ? 1 : 0;
                modelTemp.SetProperty("DEVICE_ENABLED", IsEnabled);  //可不可以用
                modelTemp.SetProperty("DEVICE_DATA", Groups[i].ToString());  //可不可以用
                int updateTemp = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), string.Format("DEVICE_NAME = '{0}'", Groups[i].Name), modelTemp, new List<string>() { "DEVICE_ENABLED", "DEVICE_DATA" });
                if (updateTemp == 1)
                {
                    for (int j = 0; j < Groups[i].DeviceItems.Count; j++)
                    {
                        Groups[i].DeviceItems[j].FlagChanged = false;
                    }
                    LogManager.AddMessage(string.Format("更新{0}端口信息:{1} 成功", Groups[i].Name, modelTemp.GetProperty("DEVICE_DATA")), EnumLogSource.数据库存取日志);
                }
                else
                {
                    LogManager.AddMessage(string.Format("更新{0}端口信息失败", Groups[i].Name), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                }
            }
        }

        /// <summary>
        /// 保存表位端口信息
        /// </summary>
        private void Meter_Save()
        {
            DynamicModel modelTemp = new DynamicModel();

            string UpdateString = "";


            for (int i = 0; i < MeterItem.Count; i++)
            {
                UpdateString += MeterItem[i].ToString() + "$";
            }
            UpdateString = UpdateString.TrimEnd('$');

            modelTemp.SetProperty("DEVICE_DATA", UpdateString);
            int updateTemp = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), string.Format("DEVICE_NAME = '{0}'", "电能表"), modelTemp, new List<string>() { "DEVICE_DATA" });
            if (updateTemp == 1)
            {
                for (int i = 0; i < MeterItem.Count; i++)
                {
                    MeterItem[i].FlagChanged = false;
                }
                LogManager.AddMessage(string.Format("更新表位端口信息:{0} 成功", modelTemp.GetProperty("DEVICE_DATA")), EnumLogSource.数据库存取日志);
            }
            else
            {
                LogManager.AddMessage("更新表位端口信息失败", EnumLogSource.数据库存取日志, EnumLevel.Warning);
            }
        }


        #endregion

    }
}

