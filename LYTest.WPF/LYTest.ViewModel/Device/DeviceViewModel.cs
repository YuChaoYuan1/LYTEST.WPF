using LYTest.Core.Enum;
using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.MeterProtocol.Encryption;
using LYTest.Utility;
using LYTest.Utility.Log;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.ViewModel.Device
{
    public class DeviceViewModel : ViewModelBase
    {
        private bool isBusy;
        /// <summary>
        /// 正在忙碌
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetPropertyValue(value, ref isBusy); }
        }



        #region 设备状态
        private bool isReady;
        /// <summary>
        /// 设备就绪,连接以后就可以下发开始检定命令了
        /// </summary>
        public bool IsReady
        {
            get { return isReady; }
            set
            {
                SetPropertyValue(value, ref isReady);
                EquipmentData.Controller.OnPropertyChanged("IsEnable");
            }
        }

        private bool? isConnected = null;
        /// <summary>
        /// 台体设备连接正常
        /// </summary>
        public bool? IsConnected
        {
            get { return isConnected; }
            set
            {
                SetPropertyValue(value, ref isConnected);
            }
        }
        #endregion

        #region 表位信息
        //private bool selectAll;
        //public bool SelectAll
        //{
        //    get { return selectAll; }
        //    set
        //    {
        //        SetPropertyValue(value, ref selectAll);
        //        //for (int i = 0; i < MeterUnits.Count; i++)
        //        //{
        //        //    //MeterUnits[i].IsSelected = value;
        //        //}
        //    }
        //}

        /// <summary>
        /// 电能表端口数据
        /// </summary>
        public AsyncObservableCollection<MeterUnitViewModel> MeterUnits { get; } = new AsyncObservableCollection<MeterUnitViewModel>();

        #endregion

        #region 配置相关


        //private AsyncObservableCollection<string> meterPorts = new AsyncObservableCollection<string>();
        ///// <summary>
        ///// 表位端口配置字符串
        ///// </summary>
        //public AsyncObservableCollection<string> MeterPorts { get; } = new AsyncObservableCollection<string>();
        //{
        //    get { return meterPorts; }
        //    set { meterPorts = value; }
        //}
        #endregion


        /// <summary>
        /// 解析设备操作命令
        /// </summary>
        /// <param name="deviceCommand">格式:{方法名}|{参数1}|{参数2}|{参数3}...</param>
        public override void CommandFactoryMethod(string deviceCommand)
        {
            TaskManager.AddWcfAction(() =>
            {
                string[] arrayCommand = deviceCommand.Split('|');
                #region 合法性校验
                if (string.IsNullOrEmpty(arrayCommand[0]))
                {
                    LogManager.AddMessage($"命令[{deviceCommand}]不是有效调用方式", EnumLogSource.用户操作日志, EnumLevel.Warning);
                    return;
                }
                #endregion

                #region 获取方法
                try
                {
                    Type[] typeArray = Type.EmptyTypes;
                    if (arrayCommand.Length > 1)
                    {
                        typeArray = new Type[arrayCommand.Length - 1];
                        for (int i = 1; i < arrayCommand.Length; i++)
                        {
                            typeArray[i - 1] = typeof(string);
                        }
                    }
                    MethodInfo method = GetType().GetMethod(arrayCommand[0], typeArray);
                    if (method == null)
                    {
                        LogManager.AddMessage($"没有找到方法:{deviceCommand}", EnumLogSource.设备操作日志, EnumLevel.Warning);
                        return;
                    }
                    try
                    {
                        object[] arrayParams = null;
                        if (arrayCommand.Length > 1)
                        {
                            arrayParams = new object[arrayCommand.Length - 1];
                            for (int j = 0; j < arrayParams.Length; j++)
                            {
                                arrayParams[j] = arrayCommand[1 + j];
                            }
                        }
                        IsBusy = true;
                        object objReturn = method.Invoke(this, arrayParams);
                        IsBusy = false;
                        if (objReturn is int resultTemp)
                        {
                            if (resultTemp == 0)
                            {
                                LogManager.AddMessage($"调用台体操作方法{deviceCommand}成功", EnumLogSource.设备操作日志);
                            }
                            else
                            {
                                LogManager.AddMessage($"调用台体操作方法{deviceCommand}失败,返回值:{objReturn}", EnumLogSource.设备操作日志, EnumLevel.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IsBusy = false;
                        LogManager.AddMessage($"调用方法出现异常:{deviceCommand}:{ex.Message}", EnumLogSource.设备操作日志, EnumLevel.Warning, ex);
                    }
                }
                catch (AmbiguousMatchException e)
                {
                    IsBusy = false;
                    LogManager.AddMessage($"找到不止一个具有指定名称的方法:{deviceCommand}", EnumLogSource.设备操作日志, EnumLevel.Warning, e);
                }
                #endregion
            });
        }

        private bool isOpenExportData = false;
        /// <summary>
        /// 打开数据管理
        /// </summary>
        public void ExportData()
        {
            DataManagerOpen("LYTest.DataManager");
        }

        public void ExportDataSG()
        {
            DataManagerOpen("LYTest.DataManager.SG");
        }

        private void DataManagerOpen(string fileName)
        {
            if (isOpenExportData)
            {
                return;
            }
            isOpenExportData = true;
            Process[] processes = Process.GetProcesses();
            Process processTemp = processes.FirstOrDefault(item => item.ProcessName == fileName);

            if (processTemp == null)
            {
                try
                {
                    DateTime startTime = DateTime.Now;
                    Process openManager = Process.Start($"{Directory.GetCurrentDirectory()}\\{fileName}.exe");
                    DateTime endTime = DateTime.Now;
                    //等待一会儿，让程序彻底启动了，免得同时打开多个了
                    Task task = new Task(() =>
                    {
                        while (endTime.Subtract(startTime).TotalSeconds < 5)
                        {
                            endTime = DateTime.Now;
                            Thread.Sleep(1000);
                        }
                        isOpenExportData = false;
                    });
                    task.Start();
                }
                catch (Exception e)
                {
                    LogManager.AddMessage($"数据管理程序启动失败:{e.Message}", EnumLogSource.用户操作日志, EnumLevel.Error);
                    isOpenExportData = false;
                    MessageBox.Show($"数据管理程序启动失败:{e.Message}");
                }
            }
            else
            {
                isOpenExportData = false;
                WindowProcess.FocusProcess(processTemp.ProcessName);
            }
        }




        /// 连接设备
        /// <summary>
        /// 连接设备
        /// </summary>
        public void Link()
        {
            if (EquipmentData.Equipment.IsDemo)
            {
                EquipmentData.DeviceManager.IsConnected = true;
                IsReady = true;
                LogManager.AddMessage("台体联机成功", EnumLogSource.设备操作日志, EnumLevel.Information);
                return;
            }

            //【标注005】
            if (ConnectDeviceAll())
            {
                EquipmentData.DeviceManager.IsConnected = true;
                IsReady = true;
                InnerCommand.VerifyControl.SendMsg("设备联机成功。");
                //EquipmentData.Controller.DeviceStart =0;  //开始台体状态0
            }
            else
            {
                IsReady = false;
                EquipmentData.DeviceManager.IsConnected = false;
                InnerCommand.VerifyControl.SendMsg("设备联机失败。");

                if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                {
                    IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                    open.AlarmData("联机失败", "检查网络信息");
                }
                //EquipmentData.DeviceManager.IsConnected = true;
                //IsReady = true;
            }
        }


        /// <summary>
        /// 连接加密机
        /// </summary>
        public bool LinkDog()
        {
            if (ConfigHelper.Instance.Dog_Type == "无" || EquipmentData.Equipment.IsDemo)
            {
                return true;
            }
            try
            {
                if (!EncrypGW.Link())
                {
                    LogManager.AddMessage("加密机连接失败!", EnumLogSource.设备操作日志, EnumLevel.Warning);
                    InnerCommand.VerifyControl.SendMsg("加密机连接失败！");
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckEncrypLink()
        {
            try
            {
                return EncrypGW.CheckIsLink();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 断开所有设备
        /// </summary>
        public void UnLink()
        {
            UnLinkDeviceAll();

            IsReady = false;
        }
        /// <summary>
        /// 设备集合字典
        /// </summary>
        public Dictionary<string, List<DeviceData>> Devices { get; } = new Dictionary<string, List<DeviceData>>();

        /// <summary>
        /// 误差板设备总线数量
        /// </summary>
        public int DeviceCount
        {
            get
            {
                if (Devices == null || !Devices.ContainsKey(DeviceName.误差板))
                    return 1;
                return Devices[DeviceName.误差板].Count;
            }
        }
        /// <summary>
        ///载入设备
        /// </summary>
        public void LoadDevices()
        {
            #region 加载设备列表

            Devices.Clear();  //设备列表
            //DeviceCount.Clear();
            //DevicesModel.Clear();
            Dictionary<string, string> EquipmentList = CodeDictionary.GetLayer2("EquipmentType");
            string[] name = EquipmentList.Keys.ToArray();
            for (int i = 0; i < EquipmentList.Count; i++)
            {
                //在数据库中找到对应的数据
                DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), string.Format("DEVICE_NAME = '{0}'", name[i]));

                if (model != null)
                {
                    bool IsExist = model.GetProperty("DEVICE_ENABLED").ToString() == "1"; //是否可以用--就是掐前面的勾打不打上-
                    string[] DeviceItemData = model.GetProperty("DEVICE_DATA").ToString().Split('$');///根据$分割，获得该设备配置集合
                    if (!IsExist) continue;
                    //循环获得该设备名称下的所有设备，比如有俩个误差板
                    for (int j = 0; j < DeviceItemData.Length; j++)
                    {
                        DeviceData device = new DeviceData(); //创建一个设备
                        string[] data = DeviceItemData[j].Split('|');//获得串口数据
                        if (data.Length > 5)
                        {
                            string comAddress = data[0];
                            string[] t = comAddress.Split('_');//开始端口远程端口ip
                            if (t.Length >= 3)
                            {
                                device.Address = t[0];
                                if (device.Address == "127.0.0.1")
                                {
                                    device.Conn_Type = CommMode.COM;
                                }
                                else
                                {
                                    device.Conn_Type = CommMode.远程服务器;
                                }
                                device.StartPort = t[1];
                                device.RemotePort = t[2];
                            }
                            device.IsExist = IsExist;
                            device.Model = data[1];
                            device.ComParam = data[2];
                            device.PortNum = data[3];
                            device.MaxTimePerFrame = data[4];
                            device.MaxTimePerByte = data[5];
                            //先把该设备加入字典
                            if (!Devices.ContainsKey(name[i]))
                            {
                                Devices.Add(name[i], new List<DeviceData>());
                            }
                            Devices[name[i]].Add(device);   //添加该设备
                        }
                    }
                }
            }

            #endregion

            #region 电能表

            MeterUnits.Clear();  //表列表
            DynamicModel meter_Model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_DEVICE_CONFIG.ToString(), "DEVICE_NAME ='电能表'");
            if (meter_Model != null)
            {
                //192.168.0.1_10003_20000|38400,n,8,1|1|1|24|3000|10
                string[] MeterItemData = meter_Model.GetProperty("DEVICE_DATA").ToString().Split('$');///根据$分割
                for (int j = 0; j < MeterItemData.Length; j++)
                {
                    string[] data = MeterItemData[j].Split('|');//获得串口数据

                    if (data.Length > 6)
                    {
                        int start = int.Parse(data[2]);
                        int interval = int.Parse(data[3]);
                        int number = int.Parse(data[4]);
                        int value = 1;
                        while (value <= number)
                        {
                            MeterUnitViewModel meterItems = new MeterUnitViewModel
                            {
                                ComParam = data[1],
                                PortNum = (start + (value - 1) * interval).ToString(),
                                MaxTimePerFrame = data[5],
                                MaxTimePerByte = data[6]
                            };

                            string[] t = data[0].Split('_');//开始端口远程端口ip
                            if (t.Length >= 3)
                            {
                                meterItems.Address = t[0];
                                if (meterItems.Address == "127.0.0.1")
                                {
                                    meterItems.Conn_Type = CommMode.COM;
                                }
                                else
                                {
                                    meterItems.Conn_Type = CommMode.远程服务器;
                                }
                                meterItems.StartPort = t[1];
                                meterItems.RemotePort = t[2];
                            }
                            MeterUnits.Add(meterItems);
                            value++;
                        }
                    }
                    //System.IO.Ports.SerialPort serial = new System.IO.Ports.SerialPort();
                }

                LogManager.AddMessage("设备端口数据加载完成", EnumLogSource.设备操作日志);
                CheckController.MeterProtocolAdapter.Instance.SetBwCount(MeterUnits.Count);

            }
            #endregion

        }


        #region 设备控制部分
        /// <summary>
        /// 初始化所有设备
        /// </summary>
        /// <returns></returns>
        public void InitializeDevice()
        {
            if (EquipmentData.Equipment.IsDemo)
                return;

            bool IsOk = true;

            foreach (string key in Devices.Keys)
            {
                List<DeviceData> device = Devices[key]; //获得这个名称下的所有设备
                for (int i = 0; i < device.Count; i++)    //循环所有设备，进行初始化
                {
                    Type type = GetReflexObject(device[i].Model);//获得设备的实例
                    if (type != null)
                    {
                        device[i].Type = type;   //保存设备的实例
                        device[i].Obj = Activator.CreateInstance(type);// 创建此类型实例
                        string Conn_Type = "InitSetting";
                        if (device[i].Conn_Type == CommMode.COM)
                        {
                            Conn_Type = "InitSettingCom";
                        }
                        MethodInfo mInfo = type.GetMethod(Conn_Type); //获取当前方法
                        bool rst = int.TryParse(device[i].PortNum, out int port);
                        if (!rst || port <= 0)
                        {
                            LogManager.AddMessage($"输入的第{i + 1}路{key}的端口号不正确!", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                        }
                        int MaxWaitTime = Convert.ToInt32(device[i].MaxTimePerFrame);
                        int WaitSencondsPerByte = Convert.ToInt32(device[i].MaxTimePerByte);
                        object[] s;
                        if (device[i].Conn_Type == CommMode.COM)
                        {
                            s = new object[3] { port, MaxWaitTime, WaitSencondsPerByte };
                        }
                        else
                        {
                            string Ip = device[i].Address;
                            int RemotePort = Convert.ToInt32(device[i].RemotePort);
                            int LocalStartPort = Convert.ToInt32(device[i].StartPort);
                            s = new object[6] { port, MaxWaitTime, WaitSencondsPerByte, Ip, RemotePort, LocalStartPort };
                        }
                        try
                        {
                            mInfo.Invoke(device[i].Obj, s);  //接收调用返回值，判断调用是否成功  new object[1] {5}
                            device[i].InitialStatus = true;
                        }
                        catch
                        {
                            if (device.Count > 1)  //设备数量大于1的化用第几路设备进行标识
                                LogManager.AddMessage($"第{i + 1}路{key}初始化失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                            else
                                LogManager.AddMessage(key + "初始化失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                        }

                    }
                    else
                    {
                        IsOk = false;
                        device[i].InitialStatus = false;
                        if (device.Count > 1)  //设备数量大于1的化用第几路设备进行标识
                            LogManager.AddMessage($"第{i + 1}路{key}初始化失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                        else
                            LogManager.AddMessage(key + "初始化失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                    }
                }
            }

            if (IsOk)
            {
                LogManager.AddMessage("设备初始化成功", EnumLogSource.设备操作日志, EnumLevel.Information);
            }
        }

        /// <summary>
        /// 连接所有设备
        /// </summary>
        /// <returns></returns>
        private bool ConnectDeviceAll()
        {
            int RetryCount = 2;//重试次数
            bool IsOk = true;
            foreach (string key in Devices.Keys)
            {
                List<DeviceData> device = Devices[key]; //获得这个名称下的所有设备
                if (DeviceName.误差板 == key)
                {
                    int dcount = device.Count;
                    int num = MeterUnits.Count / dcount;
                    List<Task> tasks = new List<Task>();
                    for (int ic = 0; ic < dcount; ic++)    //循环所有设备，进行开始联机
                    {
                        int i = ic;
                        tasks.Add(Task.Run(() =>
                        {
                            for (int t = 1; t <= RetryCount; t++)//重试次数
                            {
                                if (!device[i].Status && device[i].InitialStatus)  //没有联机成功，并且初始化成功
                                {

                                    if (dcount > 1)  //设备数量大于1的化用第几路设备进行标识
                                        LogManager.AddMessage($"正在联机第{i + 1}路{key}...", EnumLogSource.设备操作日志, EnumLevel.Information);
                                    else
                                        LogManager.AddMessage("正在联机" + key + "...", EnumLogSource.设备操作日志, EnumLevel.Information);
                                    //MeterUnits.Count / device.Count;//电表数量除以误差板数量

                                    string err = "";
                                    for (int m = i * num; m < num * (i + 1); m++) //循环该路误差板上所有表位
                                    {
                                        MethodInfo mInfo = device[i].Type.GetMethod("Connect"); //获取当前方法
                                        string[] FrameAry = { };
                                        object[] parameters = new object[2] { (byte)(m + 1), FrameAry };
                                        int connOK = (int)mInfo.Invoke(device[i].Obj, parameters);  //接收调用返回值，判断调用是否成功  new object[1] {5}
                                        if (connOK != 0)
                                        {
                                            err += $"【{m + 1}】";
                                        }
                                    }
                                    if (err != "")
                                    {
                                        if (t == RetryCount)
                                        {
                                            if (dcount > 1)  //设备数量大于1的化用第几路设备进行标识
                                                LogManager.AddMessage($"第{i + 1}路{key}联机失败,错误表位:{err}", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                                            else
                                            {
                                                LogManager.AddMessage(key + "联机失败,错误表位" + err, EnumLogSource.设备操作日志, EnumLevel.TipsError);
                                                if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                                                {
                                                    IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                                                    open.AlarmData($"第{i + 1}路{key}联机失败，错误表位", "请检查网络信息");
                                                }
                                            }

                                            device[i].Status = false;
                                            IsOk = false; //有一个设备联机不成功，就是不成功
                                        }
                                        else
                                        {
                                            LogManager.AddMessage(key + $"第{t}次联机失败,错误表位:{err}", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                                        }
                                    }
                                    else
                                    {
                                        if (dcount > 1)  //设备数量大于1的化用第几路设备进行标识
                                            LogManager.AddMessage($"第{i + 1}路{key}联机成功", EnumLogSource.设备操作日志, EnumLevel.Information);
                                        else
                                            LogManager.AddMessage(key + "联机成功", EnumLogSource.设备操作日志, EnumLevel.Information);
                                        device[i].Status = true;
                                        break;
                                    }


                                }
                            }
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
                else
                {
                    for (int i = 0; i < device.Count; i++)    //循环所有设备，进行开始联机
                    {
                        for (int t = 1; t <= RetryCount; t++)//重试次数
                        {
                            if (!device[i].Status && device[i].InitialStatus)  //没有联机成功，并且初始化成功
                            {
                                if (device.Count > 1)  //设备数量大于1的化用第几路设备进行标识
                                    LogManager.AddMessage($"正在联机第{i + 1}路{key}...", EnumLogSource.设备操作日志, EnumLevel.Information);
                                else
                                    LogManager.AddMessage("正在联机" + key + "...", EnumLogSource.设备操作日志, EnumLevel.Information);
                                try
                                {
                                    MethodInfo mInfo = device[i].Type.GetMethod("Connect"); //获取当前方法
                                    string[] FrameAry = { };
                                    object[] parameters = new object[1] { FrameAry };
                                    int connOK = (int)mInfo.Invoke(device[i].Obj, parameters);  //接收调用返回值，判断调用是否成功  new object[1] {5}
                                    if (connOK == 0)
                                    {
                                        if (device.Count > 1)  //设备数量大于1的化用第几路设备进行标识
                                            LogManager.AddMessage($"第{i + 1}路{key}联机成功", EnumLogSource.设备操作日志, EnumLevel.Information);
                                        else
                                            LogManager.AddMessage(key + "联机成功", EnumLogSource.设备操作日志, EnumLevel.Information);
                                        device[i].Status = true;
                                        break;
                                    }
                                    else
                                    {
                                        if (t == RetryCount)
                                        {
                                            if (device.Count > 1)  //设备数量大于1的化用第几路设备进行标识
                                                LogManager.AddMessage($"第{i + 1}路{key}联机失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                                            else
                                            {
                                                LogManager.AddMessage(key + "联机失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                                                if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                                                {
                                                    IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                                                    open.AlarmData($"第{i + 1}路{key}联机失败，错误表位", "请检查网络信息");
                                                }
                                            }

                                            device[i].Status = false;
                                            IsOk = false; //有一个设备联机不成功，就是不成功
                                        }
                                        else
                                        {
#if DEBUG
                                            LogManager.AddMessage(key + $"第{t}次联机失败", EnumLogSource.设备操作日志, EnumLevel.TipsError);
#endif
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogManager.AddMessage($"联机{key}异常：" + ex, EnumLogSource.设备操作日志, EnumLevel.Information);
                                }
                            }


                        }
                    }
                }
            }

            GetStdData();//启动时时读取标准表线程
            if (IsOk)
            {
                LogManager.AddMessage("台体联机成功", EnumLogSource.设备操作日志, EnumLevel.Information);
            }
            return IsOk;
        }


        private void UnLinkDeviceAll()
        {
            try
            {
                if (Devices.ContainsKey(DeviceName.功率源))
                {
                    PowerOff();
                    PowerOff();
                }
                if (Devices.ContainsKey(DeviceName.多功能板))
                {
                    EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.灭, 1);
                }
            }
            catch
            {
            }
        }



        /// <summary>
        /// 获得一个设备类的类型
        /// </summary>
        /// <returns></returns>
        public Type GetReflexObject(string deviceName)
        {
            try
            {
                //string filaName = $"LYTest.{deviceName}";
                string filaName = $"{deviceName}";
                string filePath = "Devices\\" + filaName + ".dll";
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] bFile = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();

                Assembly assembly = Assembly.Load(bFile);
                //string className = filaName+ "." + deviceName;
                string className = "ZH." + deviceName;    //ZH.3001
                Type type = assembly.GetType(className); //获取当前类的类型
                return type;

            }
            catch (Exception e)
            {
                LogManager.AddMessage("调用设备方法失败!错误代码20001-002:\r\n" + e, EnumLogSource.检定业务日志, EnumLevel.Error);
                return null;
            }

        }

        /// <summary>
        /// 反射设备方法
        /// </summary>
        /// <param name="deviceName">设备名称</param>
        /// <param name="functionName">方法名称</param>
        /// <param name="ID">设备ID,从0开始</param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object DeviceControl(string deviceName, string functionName, int ID, object[] value, bool checkType = false)
        {
            try
            {
                //这里需要修改，根据名称从设备列表中找到他的类型
                if (!Devices.ContainsKey(deviceName))
                {
                    LogManager.AddMessage($"没有添加设备【{deviceName}】", EnumLogSource.设备操作日志, EnumLevel.Warning);
                    return false;
                }
                List<DeviceData> deviceList = Devices[deviceName];
                if (ID >= deviceList.Count)
                {
                    LogManager.AddMessage($"没有添加第{ID}路【{deviceName}】", EnumLogSource.设备操作日志, EnumLevel.TipsError);
                    return false;
                }
                DeviceData device = deviceList[ID];

                //联机出现问题，没有实例化
                if (device.Type == null) return false;

                if (checkType && value != null)
                {
                    Type[] types = new Type[value.Length];
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (value[i] == null)
                        {
                            types[i] = typeof(string[]).MakeByRefType();
                        }
                        else
                        {
                            types[i] = value[i].GetType();
                        }
                    }
                    MethodInfo mInfo = device.Type.GetMethod(functionName, types);
                    return mInfo.Invoke(device.Obj, value);
                }
                else
                {
                    MethodInfo mInfo = device.Type.GetMethod(functionName);
                    return mInfo.Invoke(device.Obj, value);
                }
            }
            catch (Exception e)
            {
                LogManager.AddMessage($"deviceName:{deviceName}, functionName:{functionName}, Id:{ID}, value:{value}, checkType:{checkType}", EnumLogSource.设备操作日志, EnumLevel.Error);

                LogManager.AddMessage("调用设备方法失败!错误代码20001-001:" + e, EnumLogSource.设备操作日志, EnumLevel.Error);
                return false;
            }
        }


        #endregion


        #region 设备

        #region 功率源
        //谐波--特殊谐波
        public void OnVoltage()
        {
            if (!double.TryParse(EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_UB")?.ToString(), out double ub))
            {
                ub = 0;
            }
            double Ua = ub, Ub = ub, Uc = ub;
            int jxfs = 5;
            if (EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_WIRING_MODE")?.ToString().IndexOf("三相四线") >= 0)
            {
                jxfs = 0;
            }
            else if (EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_WIRING_MODE")?.ToString().IndexOf("三相三线") >= 0)
            {
                jxfs = 1;
                Ub = 0;
            }

            PowerOn(jxfs, Ua, Ub, Uc, 0, 0, 0, 0, 240, 120, 0, 240, 120, 50, 1);
        }
        public void OffPower()
        {
            PowerOff();
        }

        private readonly double[] TargetUabc = new double[3];
        private readonly double[] TargetIabc = new double[3];
        private int _jxfx = 0;
        private bool TargetNew = false;
        private DateTime TargetStartTime;
        private int targetOverLoad;

        public int TargetOverLoad
        {
            get { return targetOverLoad; }
            set { SetPropertyValue(value, ref targetOverLoad, "TargetOverLoad"); }
        }


        public bool PowerOn(int jxfs, double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, double PhiUa, double PhiUb,
                            double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq, int Mode, int ID = 0)
        {
            if (jxfs == 1 && VerifyConfig.VbCompensation > 0)
            {
                Ub = VerifyConfig.VbCompensation;
            }
            if (jxfs == 1 && !ConfigHelper.Instance.NewSource)
            {
                jxfs = 0;
            }
            if (Math.Max(Ua, Math.Max(Ub, Uc)) > EquipmentData.LastCheckInfo.ProtectedMaxVoltage)
            {
                LogManager.AddMessage("超过保护电压，停止输出", EnumLogSource.设备操作日志, EnumLevel.Warning);
                return false;
            }
            if (Math.Max(Ia, Math.Max(Ib, Ic)) > EquipmentData.LastCheckInfo.ProtectedMaxCurrent)
            {
                LogManager.AddMessage("超过保护电流，停止输出", EnumLogSource.设备操作日志, EnumLevel.Warning);
                return false;
            }
            object[] paras;
            string value;
            if (Devices.ContainsKey(DeviceName.隔离互感器))
            {
                double maxI = Math.Max(Ia, Math.Max(Ib, Ic));
                if (maxI != 0)
                {
                    paras = new object[] { jxfs, maxI };
                    value = DeviceControl(DeviceName.隔离互感器, "SetRange", ID, paras, true).ToString();
                    if (value == "0")
                    {

                    }
                }
            }
            if (MeterWiringMode == "单相")
            {
                Ub = 0;
                Uc = 0;
                Ib = 0;
                Ic = 0;
            }

            _jxfx = jxfs;
            TargetUabc[0] = Ua;
            TargetUabc[1] = Ub;
            TargetUabc[2] = Uc;

            TargetIabc[0] = Ia;
            TargetIabc[1] = Ib;
            TargetIabc[2] = Ic;
            TargetNew = true;
            TargetStartTime = DateTime.Now;
            string[] FrameAry = null;
            paras = new object[] { jxfs, Ua, Ub, Uc, Ia, Ib, Ic, PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Freq, Mode, FrameAry };
            value = DeviceControl(DeviceName.功率源, "PowerOn", ID, paras, true).ToString();
            return value == "0";
        }


        public bool PowerCurrentOff()
        {

            TargetIabc[0] = 0;
            TargetIabc[1] = 0;
            TargetIabc[2] = 0;
            if (TargetUabc[0] == 0 && TargetUabc[1] == 0 && TargetUabc[2] == 0 && TargetIabc[0] == 0 && TargetIabc[1] == 0 && TargetIabc[2] == 0)
                return true;

            TargetNew = true;
            TargetStartTime = DateTime.Now;
            string[] FrameAry = null;
            object[] paras = new object[] { _jxfx, TargetUabc[0], TargetUabc[1], TargetUabc[2], 0, 0, 0, 0, 240, 120, 0, 240, 120, 50, 1, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "PowerOn", 0, paras, true).ToString();
            return value == "0";

        }

        /// <summary>
        /// 2号源输出
        /// 2号源为个别特殊台子才有
        /// </summary>
        /// <returns></returns>
        public bool PowerOn2(int jxfs, double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, double PhiUa, double PhiUb,
                    double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq, int Mode, int ID = 1)
        {

            string[] FrameAry = null;
            object[] paras = new object[] { jxfs, Ua, Ub, Uc, Ia, Ib, Ic, PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Freq, Mode, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "PowerOn", ID, paras, true).ToString();
            return value == "0";
        }

        /// <summary>
        /// 关源
        /// </summary>
        /// <returns></returns>
        public bool PowerOff(int ID = 0)
        {
            TargetUabc[0] = 0;
            TargetUabc[1] = 0;
            TargetUabc[2] = 0;
            TargetIabc[0] = 0;
            TargetIabc[1] = 0;
            TargetIabc[2] = 0;
            TargetNew = true;
            TargetStartTime = DateTime.Now;
            string[] FrameAry = { };
            object[] paras = new object[2] { 0, FrameAry };
            object value = DeviceControl(DeviceName.功率源, "PowerOn_Off", ID, paras);
            if (value != null && value.ToString() == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 设置相位角度
        /// </summary>
        /// <returns></returns>
        public bool PowerAngle(double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Freq, int ID = 0)
        {
            string[] FrameAry = { };
            object[] paras = new object[8] { PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Freq, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "PowerAngle", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        //add yjt 20230131 新增负载电流快速变化实验的时间与启动标志
        /// <summary>
        /// 设置负载电流快速变化实验的时间与启动标志 测试没问题
        /// </summary>
        /// <param name="TonTime">开启时间</param>
        /// <param name="ToffTime">关断时间</param>
        /// <param name="strA">标志A</param>
        /// <param name="strB">标志B</param>
        /// <param name="strC">标志C</param>
        /// <returns></returns>
        public bool SetCurrentChangeByPower(int TonTime, int ToffTime, string strA, string strB, string strC)
        {
            string[] FrameAry = { };
            object[] paras = new object[6] { TonTime, ToffTime, strA, strB, strC, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "SetCurrentChangeByPower", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        public bool AC_VoltageSagSndInterruption(int TonTime, int ToffTime, int count, int proportion, string strA, string strB, string strC)
        {
            string[] FrameAry = { };
            object[] paras = new object[8] { TonTime, ToffTime, count, proportion, strA, strB, strC, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "AC_VoltageSagSndInterruption", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        //add yjt 20230131 新增负载电流快速变化实验的时间与启动标志
        /// <summary>
        /// 设置负载电流快速变化实验的时间与启动标志 测试没问题
        /// </summary>
        /// <param name="Time1">开启时间</param>
        /// <param name="Time2">关断时间</param>
        /// <param name="Mode">标志 111-ABC相开启</param>
        /// <returns></returns>
        public bool SetCurrentChangeByPower2(double Time1, double Time2, int Mode)
        {
            string[] FrameAry = { };
            object[] paras = new object[4] { Time1, Time2, Mode, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "LoadCurrent", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        #endregion

        //add yjt 20220805 谐波控制
        #region add yjt 20220805 谐波控制



        //谐波-常规谐波
        public bool ZH3001SetPowerGetHarmonic(string ua, string ub, string uc, string ia, string ib, string ic, float[] HarmonicContent,
            float[] HarmonicPhase, bool OnOff, int ID = 0)
        {
            int v = OnOff ? 1 : 0;
            string[] FrameAry = { };
            object[] paras = new object[] { ua, ub, uc, ia, ib, ic, v, HarmonicContent, HarmonicPhase, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "SetZH3001PowerGetHarmonic", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        //谐波-特殊谐波
        public bool ZH3001SetPowerHarmonic(string ua, string ub, string uc, string ia, string ib, string ic, int HarmonicType, int ID = 0)
        {
            string[] FrameAry = { };
            object[] paras = new object[] { ua, ub, uc, ia, ib, ic, HarmonicType, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "SetZH3001PowerHarmonic", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 设置谐波类型
        /// </summary>
        /// <param name="HarmonicType">00普通谐波，01间谐波</param>
        /// <returns></returns>
        public bool SetHarmonicType(byte HarmonicType, int ID = 0)
        {
            string[] FrameAry = { };
            object[] paras = new object[] { HarmonicType, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "SetHarmonicType", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        /// <summary>
        /// 设置电能质量项目
        /// </summary>
        /// <param name="QualityType">1个字节表示，00H是电压波动，01H是电压闪变</param>
        /// <param name="Model">1个字节表示，试验类型为电压波动时：参数为1、2、3（非这三个数值无效）分别对应电压波动试验项目中的检测点（1）、（2）、（3）试验项目为电压闪变时：参数为1、3（非这两个数值无效）分别对应闪变试验中的短时闪变值为1和闪变值为3的试验点。</param>
        /// <param name="State">使能状态：1个字节表示，01H表示启动，其他无效，停止时只需要下发关源指令即可</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool SetQualityItem(byte QualityType, byte Model, byte State = 0x01, int ID = 0)
        {
            string[] FrameAry = { };
            object[] paras = new object[] { QualityType, Model, State, FrameAry };
            string value = DeviceControl(DeviceName.功率源, "SetQualityItem", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        //谐波-设置偶次谐波
        public bool ZH1106SetControlboard(string HarmonicA, string HarmonicB, string HarmonicC, int ID = 0)
        {
            string[] FrameAry = { };
            object[] paras = new object[] { HarmonicA, HarmonicB, HarmonicC, FrameAry };
            string value = DeviceControl(DeviceName.偶次谐波, "SetZH1106Controlboard", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        // 读取 偶次谐波电流采样值
        public int ZH1106ReadData(out float[] floatCurrent, int ID = 0)
        {
            string[] FrameAry = { };
            floatCurrent = new float[6];
            object[] paras = new object[] { floatCurrent, FrameAry };
            int value = int.Parse(DeviceControl(DeviceName.偶次谐波, "ReadZH1106Data", ID, paras).ToString());
            floatCurrent = (float[])paras[0];
            return value;
        }
        #endregion

        #region 标准表


        #region 时时读取标准表

        //设置常数，读取测量值，启动走字，读电能
        //ReadstdZH311Param

        /// <summary>
        /// 是否以及开启了读取线程
        /// </summary>
        private static bool IsRunStdData = false;
        private Task task;
        private void GetStdData()
        {
            if (IsRunStdData) return;
            if (Devices.ContainsKey(DeviceName.标准表))
            {
                List<DeviceData> device = Devices[DeviceName.标准表]; //获得这个名称下的所有设备
                for (int i = 0; i < device.Count; i++)
                {
                    if (!device[i].Status)
                    {
                        return;
                    }
                }
            }

            if (Devices.ContainsKey(DeviceName.标准表) && ConfigHelper.Instance.IsReadStd)   //自动读取标准表数据
            {
                DeviceData device = Devices[DeviceName.标准表][0];
                int value = ConfigHelper.Instance.Std_RedInterval;
                string name = device.Model;
                IsRunStdData = true;
                task = new Task(() =>
                {
                    if (device.Model == "ZH311")
                    {
                        RefStd();//311标准表
                    }
                    else
                    {
                        RefStdLd(); //雷电标准表
                    }
                    //while (true)
                    //{
                    //    if (EquipmentData.ApplicationIsOver == true) break;
                    //    Thread.Sleep(value); //标准表读取间隔
                    //    if (device.Model == "ZH311")
                    //    {
                    //        RefStd();//311标准表
                    //    }
                    //    else
                    //    {
                    //        RefStdLd(); //雷电标准表
                    //    }
                    //    if (EquipmentData.ApplicationIsOver == true) break;
                    //}
                });
                task.Start();
            }
            else if ((ConfigHelper.Instance.BenthFun == "工频磁场台体" ||
        ConfigHelper.Instance.BenthFun == "辐射电磁场传导抗扰度台体")
        && ConfigHelper.Instance.IsReadStd)
            {
                IsRunStdData = true;
                task = new Task(() =>
                {
                    RefStd1();

                });
                task.Start();
            }

        }

        /// <summary>
        /// 读取ZH311瞬时测量数据,返回1成功
        /// </summary>
        /// <returns></returns>
        public int Readstd(out float[] values, out string err)
        {
            float[] instValue = new float[0];
            string errmsg = "";
            object[] paras = new object[] { instValue, errmsg };
            object data = DeviceControl(DeviceName.标准表, "ReadstdZH311Param", 0, paras);
            int r = 0;
            if (data is int @int) r = @int;
            if (paras[0] is float[] v) instValue = v;
            if (paras[1] is string s) errmsg = s;

            values = instValue;
            err = errmsg;
            return r;
        }

        private static int FailTimes = 0;
        /// <summary>
        /// 刷新标准表数据
        /// </summary>
        private void RefStd()
        {
            int value = ConfigHelper.Instance.Std_RedInterval;
            if (value < 100) value = 100;
            while (true)
            {
                if (EquipmentData.ApplicationIsOver) break;
                try
                {
                    //return;
                    int r = Readstd(out float[] floatArray, out string err);
                    if (r != 1 || floatArray == null || floatArray.Count(a => a == 0) == floatArray.Length)  //全部都为0
                    {
                        ++FailTimes;
                        if (FailTimes > 3)
                        {
                            LogManager.AddMessage($"标准表数据读取失败-{err}", EnumLogSource.设备操作日志, EnumLevel.Warning);
                        }
                    }
                    else if (floatArray != null && floatArray.Length > 28)
                    {
                        FailTimes = 0;
                        EquipmentData.StdInfo.Ua = floatArray[0];
                        EquipmentData.StdInfo.Ub = floatArray[2];
                        EquipmentData.StdInfo.Uc = floatArray[4];
                        EquipmentData.StdInfo.Ia = floatArray[1];
                        EquipmentData.StdInfo.Ib = floatArray[3];
                        EquipmentData.StdInfo.Ic = floatArray[5];

                        EquipmentData.StdInfo.PhaseUa = floatArray[6];
                        EquipmentData.StdInfo.PhaseUb = floatArray[8];
                        EquipmentData.StdInfo.PhaseUc = floatArray[10];

                        EquipmentData.StdInfo.PhaseIa = floatArray[7];
                        EquipmentData.StdInfo.PhaseIb = floatArray[9];
                        EquipmentData.StdInfo.PhaseIc = floatArray[11];

                        EquipmentData.StdInfo.PhaseA = ConvertPhase(floatArray[6] - floatArray[7]);
                        EquipmentData.StdInfo.PhaseB = ConvertPhase(floatArray[8] - floatArray[9]);
                        EquipmentData.StdInfo.PhaseC = ConvertPhase(floatArray[10] - floatArray[11]);
                        EquipmentData.StdInfo.PF = floatArray[16];

                        EquipmentData.StdInfo.Pa = floatArray[17];
                        EquipmentData.StdInfo.Pb = floatArray[20];
                        EquipmentData.StdInfo.Pc = floatArray[23];

                        EquipmentData.StdInfo.Qa = floatArray[18];
                        EquipmentData.StdInfo.Qb = floatArray[21];
                        EquipmentData.StdInfo.Qc = floatArray[24];

                        EquipmentData.StdInfo.Sa = floatArray[19];
                        EquipmentData.StdInfo.Sb = floatArray[22];
                        EquipmentData.StdInfo.Sc = floatArray[25];


                        EquipmentData.StdInfo.Freq = floatArray[12];
                        EquipmentData.StdInfo.P = floatArray[26];
                        EquipmentData.StdInfo.Q = floatArray[27];
                        EquipmentData.StdInfo.S = floatArray[28];
                    }
                    else
                    {
                        if (floatArray != null)
                        {
                            LogManager.AddMessage($"标准表数据解析失败,{err},值：" + string.Join(",", floatArray), EnumLogSource.设备操作日志, EnumLevel.Warning);
                        }
                        else
                        {
                            LogManager.AddMessage($"标准表数据解析失败,{err},读回来值为空", EnumLogSource.设备操作日志, EnumLevel.Warning);
                        }
                    }

                    if (TargetNew)
                    {
                        if ((DateTime.Now - TargetStartTime).TotalSeconds > 30)
                        {
                            TargetNew = false;
                            if (EquipmentData.Equipment.EquipmentType.Equals("单相台") || MeterWiringMode == "单相")
                            {
                                if (EquipmentData.StdInfo.Ia < TargetIabc[0] * 0.5)
                                {
                                    ++TargetOverLoad;
                                }
                            }
                            else if (MeterWiringMode == "三相三线")
                            {
                                if (EquipmentData.StdInfo.Ia < TargetIabc[0] * 0.5
                                || EquipmentData.StdInfo.Ic < TargetIabc[2] * 0.5)
                                {
                                    ++TargetOverLoad;
                                }
                            }
                            else if (EquipmentData.StdInfo.Ia < TargetIabc[0] * 0.5
                                || EquipmentData.StdInfo.Ib < TargetIabc[1] * 0.5
                                || EquipmentData.StdInfo.Ic < TargetIabc[2] * 0.5)
                            {
                                ++TargetOverLoad;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddMessage("标准表数据读取失败:" + ex, EnumLogSource.设备操作日志, EnumLevel.Warning);
                }
                if (EquipmentData.ApplicationIsOver) break;
                Thread.Sleep(value); //标准表读取间隔
            }


        }

        /// <summary>
        /// 读取标准源
        /// </summary>
        private void RefStd1()
        {
            int value = ConfigHelper.Instance.Std_RedInterval;
            if (value < 100) value = 100;
            while (true)
            {
                if (EquipmentData.ApplicationIsOver) break;
                try
                {
                    int r = Readstd1(out float[] floatArray, out string err);

                    if (r != 0 || floatArray == null || floatArray.Count(a => a == 0) == floatArray.Length)  //全部都为0
                    {
                        ++FailTimes;
                        if (FailTimes > 3)
                        {
                            LogManager.AddMessage($"标准表数据读取失败-{err}", EnumLogSource.设备操作日志, EnumLevel.Warning);
                        }
                    }
                    else if (floatArray != null && floatArray.Length > 28)
                    {
                        FailTimes = 0;
                        EquipmentData.StdInfo.Ua = floatArray[0];
                        EquipmentData.StdInfo.Ub = floatArray[1];
                        EquipmentData.StdInfo.Uc = floatArray[2];
                        EquipmentData.StdInfo.Ia = floatArray[3];
                        EquipmentData.StdInfo.Ib = floatArray[4];
                        EquipmentData.StdInfo.Ic = floatArray[5];

                        EquipmentData.StdInfo.PhaseUa = floatArray[6];
                        EquipmentData.StdInfo.PhaseUb = floatArray[7];
                        EquipmentData.StdInfo.PhaseUc = floatArray[8];

                        EquipmentData.StdInfo.PhaseIa = floatArray[9];
                        EquipmentData.StdInfo.PhaseIb = floatArray[10];
                        EquipmentData.StdInfo.PhaseIc = floatArray[11];

                        EquipmentData.StdInfo.PhaseA = ConvertPhase(floatArray[6] - floatArray[7]);
                        EquipmentData.StdInfo.PhaseB = ConvertPhase(floatArray[8] - floatArray[9]);
                        EquipmentData.StdInfo.PhaseC = ConvertPhase(floatArray[10] - floatArray[11]);
                        EquipmentData.StdInfo.PF = floatArray[33];

                        EquipmentData.StdInfo.Pa = floatArray[16];
                        EquipmentData.StdInfo.Pb = floatArray[17];
                        EquipmentData.StdInfo.Pc = floatArray[18];

                        EquipmentData.StdInfo.Qa = floatArray[20];
                        EquipmentData.StdInfo.Qb = floatArray[21];
                        EquipmentData.StdInfo.Qc = floatArray[22];

                        EquipmentData.StdInfo.Sa = floatArray[24];
                        EquipmentData.StdInfo.Sb = floatArray[25];
                        EquipmentData.StdInfo.Sc = floatArray[26];


                        EquipmentData.StdInfo.Freq = floatArray[12];
                        EquipmentData.StdInfo.P = floatArray[19];
                        EquipmentData.StdInfo.Q = floatArray[23];
                        EquipmentData.StdInfo.S = floatArray[27];
                    }
                    else
                    {
                        if (floatArray != null)
                        {
                            LogManager.AddMessage($"标准表数据解析失败,{err},值：" + string.Join(",", floatArray), EnumLogSource.设备操作日志, EnumLevel.Warning);
                        }
                        else
                        {
                            LogManager.AddMessage($"标准表数据解析失败,{err},读回来值为空", EnumLogSource.设备操作日志, EnumLevel.Warning);
                        }
                    }

                    if (TargetNew)
                    {
                        if ((DateTime.Now - TargetStartTime).TotalSeconds > 30)
                        {
                            TargetNew = false;
                            if (EquipmentData.Equipment.EquipmentType.Equals("单相台") || MeterWiringMode == "单相")
                            {
                                if (EquipmentData.StdInfo.Ia < TargetIabc[0] * 0.5)
                                {
                                    ++TargetOverLoad;
                                }
                            }
                            else if (MeterWiringMode == "三相三线")
                            {
                                if (EquipmentData.StdInfo.Ia < TargetIabc[0] * 0.5
                                || EquipmentData.StdInfo.Ic < TargetIabc[2] * 0.5)
                                {
                                    ++TargetOverLoad;
                                }
                            }
                            else if (EquipmentData.StdInfo.Ia < TargetIabc[0] * 0.5
                                || EquipmentData.StdInfo.Ib < TargetIabc[1] * 0.5
                                || EquipmentData.StdInfo.Ic < TargetIabc[2] * 0.5)
                            {
                                ++TargetOverLoad;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddMessage("标准表数据读取失败:" + ex, EnumLogSource.设备操作日志, EnumLevel.Warning);
                }
                if (EquipmentData.ApplicationIsOver) break;
                Thread.Sleep(value); //标准表读取间隔
            }


        }

        /// <summary>
        /// 读取标准源测量数据,返回1成功
        /// </summary>
        /// <returns></returns>
        public int Readstd1(out float[] values, out string err)
        {
            float[] instValue = new float[0];
            string frame = "";
            string[] frames = new string[1];
            object[] paras = new object[] { instValue, frames };
            object data = DeviceControl(DeviceName.功率源, "ReadInstMetricAll", 0, paras);
            int r = 0;
            if (data is int r1) r = r1;
            if (paras[0] is float[] v) instValue = v;
            if (paras[1] is string[] s && s.Length > 0) frame = s[0];

            values = instValue;
            err = frame;
            return r;
        }

        private float ConvertPhase(float v)
        {
            if (v < 0) v += 360;
            return v;
        }
        #endregion


        /// <summary>
        ///读取累积电量
        /// </summary>
        /// <returns >返回值float数组</returns>
        public float[] ReadEnergy()
        {
            float[] flaEnergy = null;
            string[] FrameAry = null;
            object[] paras = new object[] { flaEnergy, FrameAry };

            StdControlBase("ReadEnergy", paras).ToString();
            return paras[0] as float[];
        }

        //读标准表电量
        /// <summary>
        /// 读标准表电量
        /// </summary>
        /// <param name="TotalEnergy"></param>
        /// <param name="PulseNum"></param>
        /// <returns></returns>
        public bool ReadStdZZData(out float TotalEnergy, out long PulseNum)
        {
            TotalEnergy = 0;
            PulseNum = 0;
            object[] paras = new object[] { TotalEnergy, PulseNum };
            //DeviceControl(DevicesModel["标准表"], "readEnergy", paras);
            //TODO不知道有没问题，需要测试
            StdControlBase("ReadEnergy", paras).ToString();  //新--加入了标准表状态判断
            return true;
        }

        //add yjt 20230103 新增旧版本标准表的读取标准表电量
        /// <summary>
        /// 读取标准表电量，旧版本的标准表
        /// </summary>
        /// <returns></returns>
        public string[] ReadEnrgyZh311()
        {
            string[] flaEnergy = null;
            string[] FrameAry = null;
            object[] paras = new object[] { flaEnergy, FrameAry };
            //DeviceControl(DevicesModel["标准表"], "readEnergy", paras);
            //TODO不知道有没问题，需要测试
            StdControlBase("ReadEnrgyZh311", paras).ToString();  //新--加入了标准表状态判断
            return (string[])paras[0];
        }

        /// <summary>
        /// 固定常数下获取设置的常数值
        /// </summary>
        ///<param name="u">电压</param>
        /// <param name="i">电流</param>
        /// <returns></returns>
        public ulong GetStdConst(float i)
        {
            ulong constants = VerifyConfig.StdConst;
            if (constants == 0)//0的情况就用这里设置的，否则用设置里面设置的
            {
                #region 新标准表要求的常数
                if (i < 0.1) constants = (ulong)(2 * Math.Pow(10, 9));
                else if (i < 1) constants = (ulong)(2 * Math.Pow(10, 8));
                else if (i < 10) constants = (ulong)(2 * Math.Pow(10, 7));
                else constants = (ulong)(2 * Math.Pow(10, 6));
                #endregion

                #region 一直正常用的，标准表升级后不能用了,注释掉

                #region //获取电压电流的挡位
                /*
                //电压挡位
                int Gear_U = 2;  // 预设220V挡
                int Gear_I = 6;  // 预设220V挡
                if (u > 350)             // 380V
                    Gear_U = 3;
                else if (u > 144)         // 220V
                    Gear_U = 2;
                else if (u > 72)            // 100V
                    Gear_U = 1;
                else if (u > 0)               // 57.7V
                    Gear_U = 0;

                if (i > 60)                     // 100A
                    Gear_I = 6;
                else if (i > 30)               // 50A
                    Gear_I = 5;
                else if (i > 15)                // 20A
                    Gear_I = 5;
                else if (i > 7)                // 10A
                    Gear_I = 3;
                else if (i > 1.5)                   // 2.5A
                    Gear_I = 2;
                else if (i > 0.3)             // 0.5A
                    Gear_I = 1;
                else if (i > 0.03)          // 0.1A
                    Gear_I = 0;
                else            // 0.1A
                    Gear_I = 0;
                */
                #endregion

                #region //根据电压电流挡位获取常数 --采用降一档
                /*
                switch (Gear_U)
                {
                    case 0:            // 57.7V
                        switch (Gear_I)
                        {
                            case 0: // 0.1A及以下
                                constants = 1600000000;
                                break;
                            case 1: // 0.5A
                                constants = 400000000;
                                break;
                            case 2: // 2.5A
                                constants = 100000000;
                                break;
                            case 3: // 10A
                                constants = 25000000;
                                break;
                            case 4: // 20A
                                constants = 8000000;
                                break;
                            case 5: // 50A
                                constants = 4000000;
                                break;
                            case 6: // 100A
                                constants = 4000000;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 1:          // 100V
                        switch (Gear_I)
                        {
                            case 0: // 0.1A及以下
                                constants = 800000000;
                                break;
                            case 1: // 0.5A
                                constants = 200000000;
                                break;
                            case 2: // 2.5A
                                constants = 50000000;
                                break;
                            case 3: // 10A
                                constants = 12500000;
                                break;
                            case 4: // 20A
                                constants = 4000000;
                                break;
                            case 5: // 50A
                                constants = 2000000;
                                break;
                            case 6: // 100A
                                constants = 2000000;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 2:       // 220V
                        switch (Gear_I)
                        {
                            case 0: // 0.1A及以下
                                constants = 400000000;
                                break;
                            case 1: // 0.5A
                                constants = 100000000;
                                break;
                            case 2: // 2.5A
                                constants = 25000000;
                                break;
                            case 3: // 10A
                                constants = 6000000;
                                break;
                            case 4: // 20A
                                constants = 2000000;
                                break;
                            case 5: // 50A
                                constants = 1000000;
                                break;
                            case 6: // 100A
                                constants = 1000000;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 3:     // 380V
                        switch (Gear_I)
                        {
                            case 0: // 0.1A及以下
                                constants = 200000000;
                                break;
                            case 1: // 0.5A
                                constants = 50000000;
                                break;
                            case 2: // 2.5A
                                constants = 12000000;
                                break;
                            case 3: // 10A
                                constants = 3000000;
                                break;
                            case 4: // 20A
                                constants = 1000000;
                                break;
                            case 5: // 50A
                                constants = 500000;
                                break;
                            case 6: // 100A
                                constants = 500000;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                */
                #endregion //

                #endregion
            }
            return constants;
        }

        /// <summary>
        /// 1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="stdCmd">>0x10 读 ，0x13写</param>
        /// <param name="stdConst">常数</param>
        /// <param name="stdUIGear">电压电流挡位UA，ub，uc，ia，ib，ic</param>
        /// <returns></returns>
        public bool StdGear(byte stdCmd, ref ulong stdConst, ref double[] stdUIGear)
        {
            string[] FrameAry = null;

            if (ConfigHelper.Instance.AutoGearTemp && ConfigHelper.Instance.AutoGear)   //判断是不是自动挡位
            {
                for (int i = 0; i < stdUIGear.Length; i++)
                {
                    stdUIGear[i] = 0;
                }
            }
            else
            {
                double ia = Math.Max(Math.Max(stdUIGear[3], stdUIGear[4]), stdUIGear[5]);
                double isa;
                if (ia > 25) isa = 14;
                else if (ia > 10) isa = 13;
                else if (ia > 5) isa = 12;
                else if (ia > 2.5) isa = 11;
                else if (ia > 1) isa = 10;
                else if (ia > 0.5) isa = 9;
                else if (ia > 0.25) isa = 8;
                else if (ia > 0.1) isa = 7;
                else if (ia > 0.05) isa = 6;
                else if (ia > 0.025) isa = 5;
                else if (ia > 0.01) isa = 4;
                else isa = 3;

                stdUIGear[0] = 0;
                stdUIGear[1] = isa;
                stdUIGear[2] = 0;
                stdUIGear[3] = isa;
                stdUIGear[4] = 0;
                stdUIGear[5] = isa;
            }
            //if (stdCmd == 0x13)
            //{
            //    LogManager.AddMessage($"设置常数:{stdConst},档位：{string.Join(",", stdUIGear)}", EnumLogSource.设备操作日志);
            //}
            object[] paras = new object[] { stdCmd, stdConst, stdUIGear, FrameAry };
            string value = StdControlBase("StdGear2", paras).ToString();  //新--加入了标准表状态判断
            ulong.TryParse(paras[1].ToString(), out stdConst);
            //LogManager.AddMessage($"返回常数:{stdConst}", EnumLogSource.设备操作日志);
            if (value == "0")
                return true;
            return false;
        }

        // add yjt jx 20230205 新增设置标准表标准常数
        /// <summary>
        /// 1008H- 档位常数 读取与设置_带标准表设备ID
        /// </summary>
        /// <param name="stdCmd">>0x10 读 ，0x13写</param>
        /// <param name="stdConst">常数</param>
        /// <param name="stdUIGear">电压电流挡位UA，ub，uc，ia，ib，ic</param>
        /// <param name="DeviceId">标准表设备ID</param>
        /// <returns></returns>
        public bool StdGearDeviceID(byte stdCmd, ref ulong stdConst, ref double[] stdUIGear, int DeviceId)
        {
            string[] FrameAry = null;

            if (ConfigHelper.Instance.AutoGearTemp && ConfigHelper.Instance.AutoGear)   //判断是不是自动挡位
            {
                for (int i = 0; i < stdUIGear.Length; i++)
                {
                    stdUIGear[i] = 0;
                }
            }
            else
            {
                double ia = Math.Max(Math.Max(stdUIGear[3], stdUIGear[4]), stdUIGear[5]);
                double isa;
                if (ia >= 10)
                    isa = 14;
                else if (ia >= 2.5)
                    isa = 11;
                else if (ia >= 0.5)
                    isa = 9;
                else if (ia >= 0.1)
                    isa = 7;
                else
                    isa = 5;
                stdUIGear[0] = 0;
                stdUIGear[1] = isa;
                stdUIGear[2] = 0;
                stdUIGear[3] = isa;
                stdUIGear[4] = 0;
                stdUIGear[5] = isa;
            }

            object[] paras = new object[] { stdCmd, stdConst, stdUIGear, FrameAry };
            string value = StdControlBaseDeviceID("StdGear2", paras, DeviceId).ToString();  //新--加入了标准表状态判断
            ulong.TryParse(paras[1].ToString(), out stdConst);
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 初始化标准表
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">最大等待时间</param>
        /// <param name="WaitSencondsPerByte">帧最大等待时间</param>
        /// <returns></returns>
        public bool InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            object[] paras = new object[] { ComNumber, MaxWaitTime, WaitSencondsPerByte };
            string value = DeviceControl(DeviceName.标准表, "InitSettingCom", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        ///  100bH-模式设置--底层有问题
        /// </summary>
        /// <param name="stdCmd"></param>
        /// <param name="strModeJxfs"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool StdMode()
        {
            //            明：自动手动模式标识字符’1’表示手动模式，字符’0’表示自动模式（ascii 码读取）
            //     接线方式模式标识字符’1’表示3相4线制，字符’2’表示3相3线制（ascii 码读取）
            //标准表模式标识字符’1’表示单相表，字符’3’表示三相表，写操作此位置填0（ascii 码读取）

            //string[] FrameAry = null;
            object[] paras = new object[1];// { ComNumber, MaxWaitTime, WaitSencondsPerByte , FrameAry };
            //string value = DeviceControl(DevicesModel["标准表"], "stdMode", paras).ToString();   、
            string value = StdControlBase("StdMode", paras).ToString();  //新--加入了标准表状态判断
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 100cH-启停标准表累积电能
        /// </summary>
        /// <param name="startOrStopStd">字符’1’表示清零当前电能并开始累计（ascii 码读取）</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool StartStdEnergy(int startOrStopStd)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { startOrStopStd, FrameAry };
            //string value = DeviceControl(DevicesModel["标准表"], "startStdEnergy", paras).ToString();
            string value = StdControlBase("StartStdEnergy", paras).ToString();  //新--加入了标准表状态判断
            if (value == "0")
                return true;
            return false;
        }


        /// <summary>
        /// 脉冲校准误差 字符串形式(10字节)W， 单位 分数值。
        /// 误差板的值 × 1000000 下发，譬如误差板计算的误差0.01525%，则0.01525% ×1000000 = 152.5，
        /// 则下发字符串 ”152.5”。
        /// </summary>
        ///  <param name="Error">误差板的值</param>
        /// <returns></returns>
        public bool SetPulseCalibration(string Error)
        {
            object[] paras = new object[] { Error };
            string value = StdControlBase("SetPulseCalibration", paras).ToString();  //新--加入了标准表状态判断
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        ///   设置脉冲
        /// </summary>
        /// <param name="pulseType">
        ///有功脉冲 设置字符’1’
        ///无功脉冲 设置字符’2’
        ///UA脉冲 设置字符’3’
        ///UB脉冲 设置字符’4’
        ///UC脉冲 设置字符’5’
        ///IA脉冲 设置字符’6’
        ///IB脉冲 设置字符’7’
        ///IC脉冲 设置字符’8’
        ///PA脉冲 设置字符’9’
        ///PB脉冲 设置字符’10’
        ///PC脉冲 设置字符’11’
        ///</param>
        /// <returns></returns>
        public bool SetPulseType(string pulseType)
        {
            object[] paras = new object[] { pulseType };
            //string value = DeviceControl(DevicesModel["标准表"], "SetPulseType", paras).ToString();
            string value = StdControlBase("SetPulseType", paras).ToString();  //新--加入了标准表状态判断
            if (value == "0")
                return true;
            return false;
        }

        private object StdControlBase(string FunName, object[] paras)
        {
            object obj = DeviceControl(DeviceName.标准表, FunName, 0, paras);     //操作过程
            return obj;
        }

        // add yjt jx 20230205 新增设置标准表标准常数
        private object StdControlBaseDeviceID(string FunName, object[] paras, int DeviceId)
        {
            object obj = DeviceControl(DeviceName.标准表, FunName, DeviceId, paras);     //操作过程
            return obj;
        }

        /// <summary>
        /// 1006H-谐波有功功率
        /// </summary>
        /// <returns></returns>
        public float[] ReadHarmonicActivePower()
        {
            string[] FrameAry = null;
            float[] value = null;
            object[] paras = new object[] { value, FrameAry };
            StdControlBase("ReadHarmonicActivePower", paras);
            return (float[])paras[0];
            //返回值--0-6 a相U基波有功功率-a相I。。。
            //7-12 a相u的第二次谐波有功功率--a相I的第二次有功功率。。。最多64次
        }

        //public float[] ReadHarmonicActiveEnergy()//标准表问题暂时不用
        //{
        //    string[] FrameAry = null;
        //    float[] value = null;
        //    object[] paras = new object[] { value, FrameAry };
        //    StdControlBase("readHarmonicActiveEnergy", paras);
        //    return (float[])paras[0];

        //}
        //
        /// <summary>
        /// 1004H-谐波含量
        /// </summary>
        /// <returns></returns>
        public float[] ReadHarmonicEnergy()
        {
            string[] FrameAry = null;
            float[] value = null;
            object[] paras = new object[] { value, FrameAry };
            StdControlBase("ReadHarmonicEnergy", paras);
            return (float[])paras[0];
            //返回值--0-6 a相U基波幅值-a相I。。。
            //7-12 a相u的第二次谐波含量--a相I的第二次谐波含量。。。最多64次

        }
        /// <summary>
        /// 1005H-谐波相位
        /// </summary>
        /// <returns></returns>
        public float[] ReadHarmonicAngle()
        {
            string[] FrameAry = null;
            float[] value = null;
            object[] paras = new object[] { value, FrameAry };
            StdControlBase("ReadHarmonicAngle", paras);
            return (float[])paras[0];
        }

        /// <summary>
        /// 1028H-间谐波相位
        /// </summary>
        /// <returns></returns>
        public float[] ReadInterHarmonicActivePower()
        {
            string[] FrameAry = null;
            float[] value = null;
            object[] paras = new object[] { value, FrameAry };
            StdControlBase("ReadInterHarmonicActivePower", paras);
            return (float[])paras[0];
        }

        /// <summary>
        /// 1029H-设置谐波采样模式
        /// </summary>
        /// <param name="ModelType">0/1 普通采样模式/谐波采样模式</param>
        /// <returns></returns>
        public bool ChangeHarmonicModel(int ModelType)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { ModelType, FrameAry };
            //string value = DeviceControl(DevicesModel["标准表"], "startStdEnergy", paras).ToString();
            string value = StdControlBase("ChangeHarmonicModel", paras).ToString();  //新--加入了标准表状态判断
            if (value == "0")
                return true;
            return false;
        }
        #endregion

        #region 误差板   控制继电器，设置标准参数，设置被检常数，读取计算值，启动，停止
        /// <summary>
        /// 控制互感继电器
        /// </summary>
        /// <param name="hgq">true互感器</param>
        public void ControlHGQRelay(bool hgq)
        {
            if (Devices.ContainsKey(DeviceName.多功能板) && Devices[DeviceName.多功能板].Count > 0)
            {
                if (Devices[DeviceName.多功能板][0].Model == "LY2001")
                {
                    int count = Devices[DeviceName.多功能板].Count;
                    byte[] chnls = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 16, 17, 19 };
                    byte open;
                    if (hgq)
                    {
                        for (int ID = 0; ID < count; ID++)
                        {
                            for (int i = 0; i < chnls.Length; i++)
                            {
                                byte number = chnls[i];
                                open = 1;
                                object[] paras = new object[] { number, open };
                                DeviceControl(DeviceName.多功能板, "MultiControl", ID, paras);
                            }
                        }
                    }
                    else
                    {
                        for (int ID = 0; ID < count; ID++)
                        {
                            for (int i = 0; i < chnls.Length; i++)
                            {
                                byte number = chnls[i];
                                open = 0;
                                object[] paras = new object[] { number, open };
                                DeviceControl(DeviceName.多功能板, "MultiControl", ID, paras);
                            }
                        }
                    }
                }
                else
                {
                    if (EquipmentData.Equipment.EquipmentType.Equals("单相台"))
                    {

                    }
                    else
                    {
                        string[] Frames = null;
                        int iType = hgq ? 1 : 0;
                        int iState = 0;
                        object[] paras = new object[] { iType, iState, Frames };
                        DeviceControl(DeviceName.多功能板, "SetSwitchDirectTransformer", 0, paras);
                    }
                }
            }
        }

        public void ControlMeterRelay(int contrnlType, byte EpitopeNo, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { contrnlType, EpitopeNo, (byte)0x01, (byte)0x01, FrameAry };
            DeviceControl(DeviceName.误差板, "ContnrlBw", ID, paras);
        }
        /// <summary>
        /// 控制表位耐压继电器 
        /// </summary>
        /// <param name="EpitopeNo">表位编号0xFF为广播</param>
        /// <param name="ctr">1 闭合,0 恢复</param>
        public bool ControlMeterInsulationRelay(byte EpitopeNo, int ctr, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { 7, EpitopeNo, (byte)ctr, (byte)0x01, FrameAry };
            object obj = DeviceControl(DeviceName.误差板, "ContnrlBw", ID, paras);
            if (obj is int rst)
            {
                if (rst == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 控制表位电压回路继电器 
        /// </summary>
        /// <param name="EpitopeNo">表位编号0xFF为广播</param>
        /// <param name="state">1 闭合,0 恢复</param>
        public bool ControlULoadRelay(byte EpitopeNo, byte state, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { 3, EpitopeNo, state, (byte)0x01, FrameAry };
            object obj = DeviceControl(DeviceName.误差板, "ContnrlBw", ID, paras);
            if (obj is int rst)
            {
                if (rst == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置标准常数
        /// </summary>
        /// <param name="ControlType">控制类型（00-标准电能误差相关； 01-标准时钟日计时需量）</param>
        /// <param name="constant">常数</param>
        /// <param name="magnification">放大倍数-2就是缩小100倍</param>
        /// <param name="EpitopeNo">表位编号</param>
        public bool SetStandardConst(int ControlType, int constant, int magnification, byte EpitopeNo, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { ControlType, constant, magnification, EpitopeNo, FrameAry };
            string value = DeviceControl(DeviceName.误差板, "SetStandardConstantQs", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 设置被检常数
        /// </summary>
        /// <param name="ControlType">控制类型(6组被检:00-有功(含正向、反向，以下同,01-无功(正向、反向，以下同),04-日计时,05-需量）</param>
        /// <param name="constant">常数</param>
        /// <param name="magnification">放大倍数-2就是缩小100倍</param>
        /// <param name="qs">圈数</param>
        /// <param name="EpitopeNo">表位编号</param>
        public bool SetTestedConst(int ControlType, int constant, int magnification, int qs, byte EpitopeNo, int ID = 0)
        {

            //(int enlarge, int Constant, int fads, int qs, byte bwNum, out string[] FrameAry)
            string[] FrameAry = null;
            object[] paras = new object[] { ControlType, constant, magnification, qs, EpitopeNo, FrameAry };
            string value = DeviceControl(DeviceName.误差板, "SetBJConstantQs", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }


        /// <summary>
        /// 启动误差版
        /// </summary>
        /// <param name="ControlType">控制类型（00：正向有功，01：正向无功，02：反向有功，03：反向无功，04：日计时，05：需量， 06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）</param>
        /// <param name="EpitopeNo">表位号--FF广播</param>
        public bool StartWcb(int ControlType, byte EpitopeNo, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { ControlType, EpitopeNo, FrameAry };
            string value = DeviceControl(DeviceName.误差板, "Start", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 停止误差版
        /// </summary>
        /// <param name="ControlType">控制类型（00：正向有功，01：正向无功，02：反向有功，03：反向无功，04：日计时，05：需量， 06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）</param>
        /// <param name="EpitopeNo">表位号--FF广播</param>
        public bool StopWcb(int ControlType, byte EpitopeNo, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { ControlType, EpitopeNo, FrameAry };
            string value = DeviceControl(DeviceName.误差板, "Stop", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        ///   读取误差计算值
        /// </summary>
        /// <param name="readType">读取类型(00--正向有功，01--正向无功，02--反向有功，03--反向无功，04--日计时误差</param>
        /// <param name="EpitopeNo">表位编号</param>
        /// <param name="OutWcData">误差值</param>
        /// <param name="OutBwNul">表位号</param>
        /// <param name="OutGroup">组别: 1个字节(00--有功，01--无功，04--日计时误差，05--需量，06--有功脉冲计数，07--无功</param>
        /// <param name="OutWcNul">第几次误差</param>
        public void ReadWcbData(int readType, byte EpitopeNo, out string[] OutWcData, out int OutBwNul, out int OutGroup, out int OutWcNul, int ID = 0)
        {
            string[] FrameAry = null;
            OutWcData = new string[1];
            OutBwNul = 0;
            OutGroup = 0;
            OutWcNul = 0;
            object[] paras = new object[] { readType, EpitopeNo, OutWcData, OutBwNul, OutGroup, OutWcNul, FrameAry };
            DeviceControl(DeviceName.误差板, "ReadData", ID, paras);
            OutWcData = (string[])paras[2];
            if (OutWcData.Length <= 0)
            {
                OutWcData = new string[1] { "0" };
            }
            OutBwNul = (int)paras[3];
            OutGroup = (int)paras[4];
            OutWcNul = (int)paras[5];

        }

        /// <summary>
        /// 电机控制，0下1上
        /// </summary>
        /// <param name="ControlType">控制类型0压入表  1取出表</param>
        /// <param name="EpitopeNo">表位号--FF广播</param>
        public bool ElectricmachineryContrnl(int ControlType, byte EpitopeNo, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { ControlType, EpitopeNo, FrameAry };
            string value = DeviceControl(DeviceName.误差板, "ElectricmachineryContrnl", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        /// <summary>
        /// 读取表位电压短路和电流开路标志 
        /// </summary>
        /// <param name="reg">命令03读取表位电压短路和电流开路标志</param>
        /// <param name="bwNum">表位</param>
        /// <param name="OutResult">返回状态0:电压短路标志，00表示没短路，01表示短路；DATA1-电流开路标志，1:00表示没开路，01表示开路。</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public bool Read_Fault(byte reg, byte bwNum, out byte[] OutResult, int ID = 0)
        {
            string[] FrameAry = null;
            OutResult = new byte[7];
            object[] paras = new object[] { reg, bwNum, OutResult, FrameAry };
            string value = DeviceControl(DeviceName.误差板, "Read_Fault", ID, paras).ToString();
            OutResult = (byte[])paras[2];
            if (value == "0")
                return true;
            return false;

        }


        #endregion

        #region 载波模块



        /// <summary>
        ///   多功能板控制载波切换通道
        /// </summary>
        /// <param name="ControlboardID">载波通道号(FF广播)</param>
        /// <param name="controlboardType">01=闭合，00=断开</param>
        public void CarrierModuleControl(int ControlboardID, int controlboardType, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { ControlboardID, controlboardType, FrameAry };
            DeviceControl(DeviceName.载波, "SetZH3501Controlboard", ID, paras);
        }

        /// <summary>
        /// 载波供电
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="id"></param>
        /// <param name="ID"></param>
        public bool ZBPowerSupplyType(int zbid)
        {
            if (!Devices.ContainsKey(DeviceName.老载波)) return true;
            string[] FrameAry = null;
            object[] paras = new object[] { zbid, FrameAry };
            string value = DeviceControl(DeviceName.老载波, "ZBPowerSupplyType", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        #endregion

        #region 偶次谐波

        /// <summary>
        ///  偶次谐波设置
        /// </summary>
        /// <param name="HarmonicA">A相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="HarmonicB">B相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        /// <param name="HarmonicC">C相偶次谐波 置1为启动偶次谐波发生，置0为停止偶次谐波发生</param>
        public void SetEvenHarmonic(string HarmonicA, string HarmonicB, string HarmonicC, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { HarmonicA, HarmonicB, HarmonicC, FrameAry };
            DeviceControl(DeviceName.偶次谐波, "SetZH1106Controlboard", ID, paras);
        }


        /// <summary>
        ///  读取 偶次谐波电流采样值
        /// </summary>
        /// <param name="floatCurrent"> A相正半波电流值 + A相负半波电流值 + B相正半波电流值 + B相负半波电流值 + C相正半波电流值 + C相负半波电流值</param>
        public void GetEvenHarmonicValue(out float[] floatCurrent, int ID = 0)
        {
            string[] FrameAry = null;
            floatCurrent = new float[0];
            object[] paras = new object[] { floatCurrent, FrameAry };
            DeviceControl(DeviceName.偶次谐波, "ReadZH1106Data", ID, paras);
        }

        #endregion

        #region 雷电标准表


        /// <summary>
        ///  设置接线方式 
        /// </summary>
        /// <param name="MetricIndex"></param>
        public void SetPuase(string BncCode, string MetricIndex, string aseCode)
        {
            object[] paras = new object[] { BncCode, MetricIndex, aseCode };
            DeviceControl(DeviceName.标准表, "SetPuase", 0, paras);
        }
        /// <summary>
        ///  设置常数
        /// </summary>
        /// <param name="MetricIndex"></param>
        /// <param name="Constant"></param>
        /// 
        public void SetConstant(string MetricIndex, float Constant)
        {
            object[] paras = new object[] { MetricIndex, Constant };
            DeviceControl(DeviceName.标准表, "SetConstant", 0, paras);
        }

        /// <summary>
        ///   读取标准表瞬时值
        /// </summary>
        /// 
        public float[] ReadStMeterInfo()
        {
            object[] paras = null;
            object obj = DeviceControl(DeviceName.标准表, "ReadStMeterInfo", 0, paras);
            float[] value = (float[])obj;

            //StandarMeterInfo info= (StandarMeterInfo)obj;
            return value;
        }

        /// <summary>
        /// 刷新标准表数据
        /// </summary>
        private void RefStdLd()
        {
            int value = ConfigHelper.Instance.Std_RedInterval;
            while (true)
            {
                if (EquipmentData.ApplicationIsOver == true) break;
                Thread.Sleep(value); //标准表读取间隔
                try
                {
                    //return;
                    float[] floatArray = ReadStMeterInfo();
                    if (floatArray != null)
                    {
                        EquipmentData.StdInfo.Ua = floatArray[0];
                        EquipmentData.StdInfo.Ub = floatArray[1];
                        EquipmentData.StdInfo.Uc = floatArray[2];
                        EquipmentData.StdInfo.Ia = floatArray[3];
                        EquipmentData.StdInfo.Ib = floatArray[4];
                        EquipmentData.StdInfo.Ic = floatArray[5];

                        EquipmentData.StdInfo.PhaseUa = floatArray[17];
                        EquipmentData.StdInfo.PhaseUb = floatArray[18];
                        EquipmentData.StdInfo.PhaseUc = floatArray[19];

                        EquipmentData.StdInfo.PhaseIa = floatArray[20];
                        EquipmentData.StdInfo.PhaseIb = floatArray[21];
                        EquipmentData.StdInfo.PhaseIc = floatArray[22];

                        //EquipmentData.StdInfo.PhaseA = floatArray.;
                        //EquipmentData.StdInfo.PhaseB = floatArray[13];
                        //EquipmentData.StdInfo.PhaseC = floatArray[14];

                        EquipmentData.StdInfo.Pa = floatArray[6];
                        EquipmentData.StdInfo.Pb = floatArray[7];
                        EquipmentData.StdInfo.Pc = floatArray[8];

                        EquipmentData.StdInfo.Qa = floatArray[9];
                        EquipmentData.StdInfo.Qb = floatArray[10];
                        EquipmentData.StdInfo.Qc = floatArray[11];

                        EquipmentData.StdInfo.Sa = floatArray[12];
                        EquipmentData.StdInfo.Sb = floatArray[13];
                        EquipmentData.StdInfo.Sc = floatArray[14];


                        EquipmentData.StdInfo.Freq = floatArray[15];
                        //EquipmentData.StdInfo.P = floatArray.P;
                        //EquipmentData.StdInfo.Q = floatArray.Q;
                        //EquipmentData.StdInfo.S = floatArray.S;
                    }
                }
                catch
                {
                }
                if (EquipmentData.ApplicationIsOver == true) break;
            }


        }

        //ReadStMeterInfo


        //StandarMeterInfo
        #endregion

        #region 互感器电机控制
        /// <summary>
        ///  表位互感器探针电机控制
        /// </summary>
        /// <param name="ControlboardID">0切换到互感器，1切换到直接</param>
        public void Hgq_Set(int controlId, int ID = 0)
        {
            object[] paras = new object[] { };
            if (controlId == 0)
            {
                DeviceControl(DeviceName.互感器电机, "Set_HGQ", ID, paras);
            }
            else
            {
                DeviceControl(DeviceName.互感器电机, "Set_ZJ", ID, paras);
            }
            Hgq_Off();//过60秒之后关闭电机
        }

        bool Hgq_Is_Off = false;
        /// <summary>
        /// 关闭
        /// </summary>
        public void Hgq_Off(int ID = 0)
        {

            int index = 60;
            if (!Hgq_Is_Off)
            {
                Hgq_Is_Off = true;
                Task task = new Task(() =>
                {
                    while (true)
                    {
                        if (EquipmentData.ApplicationIsOver == true) break;
                        Thread.Sleep(1000);
                        index--;
                        if (index <= 0)
                        {
                            break;
                        }

                        if (EquipmentData.ApplicationIsOver == true) break;
                    }
                    object[] paras = new object[] { };
                    DeviceControl(DeviceName.互感器电机, "Set_Off", ID, paras);
                    Hgq_Is_Off = false;
                });
                task.Start();
            }
            else
            {
                index = 60;
            }



        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Hgq_Off2(int ID = 0)
        {
            object[] paras = new object[] { };
            DeviceControl(DeviceName.互感器电机, "Set_Off", ID, paras);
            Hgq_Is_Off = false;
        }
        #endregion

        #region 多功能板

        public void SetEquipmentThreeColor(EmLightColor color, int iType, int ID = 0)
        {
            VerifyBase.LightColorType = color;
            if (Devices == null || !Devices.ContainsKey(DeviceName.多功能板)) return;
            string[] FrameAry = null;
            int ColorNum = 0;
            switch (Devices[DeviceName.多功能板][0].Model)
            {
                case "ZH2029B":
                case "ZH2029D":
                    {
                        switch (color)
                        {
                            case EmLightColor.黄:
                                ColorNum = 19;
                                break;
                            case EmLightColor.绿:
                                ColorNum = 20;
                                break;
                            case EmLightColor.红:
                                ColorNum = 18;
                                break;
                            case EmLightColor.灭:
                                iType = 0;
                                break;
                        }
                        object[] paras = new object[] { ColorNum, iType, FrameAry };
                        DeviceControl(DeviceName.多功能板, "SetEquipmentThreeColor", ID, paras);
                    }
                    break;
                case "LY2001":
                    {
                        switch (color)
                        {
                            case EmLightColor.黄:
                                ColorNum = 0;
                                break;
                            case EmLightColor.绿:
                                ColorNum = 1;
                                break;
                            case EmLightColor.红:
                                ColorNum = 2;
                                break;
                            case EmLightColor.灭:
                                ColorNum = 0;
                                break;
                            default:
                                ColorNum = 0;
                                break;
                        }
                        object[] paras = new object[] { ColorNum };
                        DeviceControl(DeviceName.多功能板, "SetTricolorLamp", 0, paras);
                    }
                    break;
            }

        }

        /// <summary>
        /// 设置工频磁场装置磁场线圈电机旋转
        /// </summary>
        /// <param name="dev">选择电机控制对象，0表示磁场线圈电机，1表示挂表座旋转电机。</param>
        /// <param name="angle"></param>
        public void LY2001_7000HSet(byte dev, int angle)
        {
            if (Devices == null || !Devices.ContainsKey(DeviceName.多功能板)) return;
            if (Devices[DeviceName.多功能板][0].Model != "LY2001") return;
            object[] paras = new object[] { dev, angle };
            DeviceControl(DeviceName.多功能板, "Fun7000H", 0, paras);

        }

        /// <summary>
        /// 读取工频磁场装置磁场线圈电机角度
        /// </summary>
        /// <param name="angle1">磁场电机角度</param>
        /// <param name="angle2">挂表电机角度</param>
        public void LY2001_7001HGet(out float angle1, out float angle2)
        {
            angle1 = 0;
            angle2 = 0;
            if (Devices == null || !Devices.ContainsKey(DeviceName.多功能板)) return;
            if (Devices[DeviceName.多功能板][0].Model != "LY2001") return;
            object[] paras = new object[] { angle1, angle2 };
            DeviceControl(DeviceName.多功能板, "Fun7001H", 0, paras);
            angle1 = Convert.ToSingle(paras[0]);
            angle2 = Convert.ToSingle(paras[1]);

        }

        /// <summary>
        /// 普通供电=0、耐压供电=1、载波供电=2、二回路=5、 耐压保护=6
        /// </summary>
        /// <param name="supplyType"></param>
        /// <param name="iType">false直接式，true互感式</param>
        /// <param name="ID"></param>
        public void SetEquipmentPowerSupply(int supplyType, bool iType, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras;

            switch (EquipmentData.DeviceManager.Devices[Device.DeviceName.多功能板][0].Model)
            {
                case "ZH2029B":
                case "ZH2029D":
                    int elementType = supplyType;
                    bool isMeterTypeHGQ = iType;
                    int[] switchOpen = new int[] { 0, 1, 2, 6 };
                    int[] switchClose = new int[] { 3, 7, 9 };
                    switch (supplyType)
                    {
                        case 0:
                            if (EquipmentData.Equipment.EquipmentType.Equals("单相台"))
                            {
                                switchOpen = new int[] { 0, 1, 2, 6 };
                                switchClose = new int[] { 3, 7, 9 };
                            }
                            else
                            {
                                if (iType)//互感,三相
                                {
                                    switchOpen = new int[] { 0, 1, 2, 3, 4, 5, 6 };
                                    switchClose = new int[] { 7, 9 };
                                }
                                else
                                {
                                    switchOpen = new int[] { 0, 1, 2, 3, 4, 5, 6 };
                                    switchClose = new int[] { 7, 9 };
                                }
                            }
                            break;
                        case 1:
                            if (EquipmentData.Equipment.EquipmentType.Equals("单相台"))
                            {
                                switchOpen = new int[] { 7 };
                                switchClose = new int[] { 0, 1, 2, 3, 6, 9 };
                            }
                            else
                            {
                                if (iType)//互感
                                {
                                    switchOpen = new int[] { 7 };
                                    switchClose = new int[] { 0, 1, 2, 3, 4, 5, 6, 9 };
                                }
                                else
                                {
                                    switchOpen = new int[] { 7 };
                                    switchClose = new int[] { 0, 1, 2, 3, 4, 5, 6, 9 };
                                }
                            }
                            break;
                        case 2:
                            if (EquipmentData.Equipment.EquipmentType.Equals("单相台"))
                            {
                                switchOpen = new int[] { 0, 1, 2, 6, 9 };
                                switchClose = new int[] { 3, 7 };
                            }
                            else
                            {
                                if (iType)//互感
                                {
                                    switchOpen = new int[] { 0, 1, 2, 3, 4, 5, 6, 9 };
                                    switchClose = new int[] { 7, 8 };
                                }
                                else
                                {
                                    switchOpen = new int[] { 0, 1, 2, 3, 4, 5, 6, 9 };
                                    switchClose = new int[] { 7, 8 };
                                }
                            }
                            break;

                        case 5:

                            break;
                        case 6:

                            break;
                    }
                    paras = new object[] { elementType, isMeterTypeHGQ, switchOpen, switchClose, FrameAry };
                    DeviceControl(DeviceName.多功能板, "SetPowerSupplyType", ID, paras);
                    break;
            }

        }
        public void Remote_Supply(bool On)
        {
            object[] paras = new object[] { On };
            DeviceControl(DeviceName.供断电控制板, "Remote_Supply", 0, paras);
        }
        #endregion

        #region 功耗板
        /// <summary>
        ///    控制表位继电器 
        /// </summary>
        /// <param name="contrnlType">控制类型--1开启-2关闭</param>
        /// <param name="EpitopeNo">表位编号0xFF为广播</param>
        public bool Read_GH_Dissipation(int bwIndex, out float[] pd, int ID = 0)
        {
            pd = new float[1];
            object[] paras = new object[] { bwIndex, pd };
            bool t = (bool)DeviceControl(DeviceName.功耗板, "Read_GH_Dissipation", ID, paras);
            pd = (float[])paras[1];
            return t;
        }

        #endregion

        #region 时基源

        /// <summary>
        ///设置时基源输出脉冲
        /// </summary>
        /// <param name="iID">是否切换为时钟脉冲,true切换到时钟脉冲,false切换到电能脉冲</param>
        public void SetTimePulse(bool isTime, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { isTime, FrameAry };
            DeviceControl(DeviceName.时基源, "SetTimePulse", ID, paras);
        }
        #endregion

        #region 耐压仪及耐压测试板
        /// <summary>
        /// 启动，停止测试
        /// </summary>
        /// <param name="ValueType">01开始，02结束<param>
        /// <returns></returns>
        public bool Ainuo_Start(byte ValueType, int ID = 0)
        {
            object[] paras = new object[] { ValueType };
            DeviceControl(DeviceName.耐压仪, "Start", ID, paras);
            return true;
        }

        /// <summary>
        ///  设置当前测试方式下的参数
        /// </summary>
        /// <param name="U">电压</param>
        /// <param name="UpI">电流上限(毫安)</param>
        /// <param name="DownI">电流下限(毫安)</param>
        /// <param name="Time">测试时间(秒)</param>
        /// <param name="Pl">频率</param>
        /// <param name="UpTime">缓升时间</param>
        /// <param name="DownTime">缓降时间</param>
        /// <returns></returns>
        public bool Ainuo_SetModelValue(float U, float UpI, float DownI, int Time, int Pl, int UpTime, int DownTime, int ID = 0)
        {
            object[] paras = new object[] { U, UpI, DownI, Time, Pl, UpTime, DownTime };
            DeviceControl(DeviceName.耐压仪, "SetModelValue", ID, paras);
            return true;
        }
        #endregion

        #region 温控板
        /// <summary>
        /// 获取温度
        /// </summary>
        /// <param name="data">返回的温度</param>
        /// <param name="BwNum">端子号-1端子1，2端子2，3端子4，4端子8，1和2端子3，ff广播</param>
        /// <returns></returns>
        public bool GetTemperature(out float[] data, byte BwNum = 0xff, int ID = 0)
        {
            data = new float[1];
            object[] paras = new object[] { data, BwNum };
            string value = DeviceControl(DeviceName.温控板, "GetTemperature", ID, paras).ToString();
            if (value == "0")
            {
                data = (float[])paras[0];
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置温度
        /// </summary>
        /// <param name="data">设置的温度</param>
        /// <param name="BwNum">端子号-1端子1，2端子2，3端子4，4端子8，1和2端子3，ff广播</param>
        /// <returns></returns>
        public bool SetTemperature(float[] data, byte BwNum = 0xff, int ID = 0)
        {
            object[] paras = new object[] { data, BwNum };
            string value = DeviceControl(DeviceName.温控板, "SetTemperature", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        /// <summary>
        /// 设置其他设备开关 -05风扇，06电磁锁，07指示灯
        /// </summary>
        /// <param name="deviceNum">设备码 -05风扇，06电磁锁，07设置电压继电器板</param>
        /// <param name="YesNo">开关</param>
        /// <returns></returns>
        public bool SetDevice(byte deviceNum, bool YesNo, int ID = 0)
        {
            object[] paras = new object[] { deviceNum, YesNo };
            string value = DeviceControl(DeviceName.温控板, "SetDevice", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        /// <summary>
        ///  温度台控制温控板及电压
        /// </summary>
        /// <param name="meterType">温控板类型-1单相-2三相直接-3三相互感</param>
        /// <param name="ub">电压1=57.7--2=100--3=200</param>
        /// <returns></returns>
        public bool TemperaturePowerOn(int meterType, float ub, int ID = 0)
        {
            object[] paras = new object[] { meterType, ub };
            string value = DeviceControl(DeviceName.多功能板, "TemperaturePowerOn", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        #endregion

        #region 零线电流板
        //add yjt 20230103 新增零线电流控制启停
        /// <summary>
        /// 启停零线电流板
        /// </summary>
        /// <param name="StartZeroCurrent"></param>
        /// <returns></returns>
        public bool StartZeroCurrent(int A_kz, int BC_kz)
        {
            object[] paras = new object[] { A_kz, BC_kz };
            string value = DeviceControl(DeviceName.零线电流板, "StartZeroCurrent", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        #endregion

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctr">1 闭合,0 恢复</param>
        /// <returns></returns>
        public bool[] ControlMeterInsulationRelay(int ctr)
        {
            int DeviceCount = Devices[DeviceName.误差板].Count;
            int indexBwcount = EquipmentData.Equipment.MeterCount / DeviceCount;
            List<Task<bool[]>> tasks = new List<Task<bool[]>>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    bool[] rst = new bool[indexBwcount];
                    for (int n = 0; n < indexBwcount; n++)
                    {
                        int pos = (line * indexBwcount) + n + 1;
                        rst[n] = EquipmentData.DeviceManager.ControlMeterInsulationRelay((byte)pos, ctr, line);
                        Task.Delay(80).Wait(1000);
                    }
                    return rst;
                }));
            }
            Task.WaitAll(tasks.ToArray());
            List<bool> rsts = new List<bool>();
            foreach (var t in tasks)
            {
                rsts.AddRange(t.Result);
            }
            return rsts.ToArray();
        }

        public bool Set_InsulationMeterCurrent(float data)
        {
            int DeviceCount = Devices[DeviceName.误差板].Count;
            int indexBwcount = EquipmentData.Equipment.MeterCount / DeviceCount;
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    bool rst = true;
                    for (int n = 0; n < indexBwcount; n++)
                    {
                        int pos = (line * indexBwcount) + n + 1;

                        object[] paras = new object[] { pos, data };
                        int value = (int)DeviceControl(DeviceName.误差板, "SetInsulationLimit", line, paras);

                        rst &= value == 0;
                        Task.Delay(80).Wait(1000);
                    }
                    return rst;
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return tasks.All(t => t.Result == true);

        }

        public bool Read_InsulationMeterCurrent(out List<float[]> currents)
        {
            currents = new List<float[]>();
            int DeviceCount = Devices[DeviceName.误差板].Count;
            int indexBwcount = EquipmentData.Equipment.MeterCount / DeviceCount;
            List<Task<List<float[]>>> tasks = new List<Task<List<float[]>>>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    bool rst = true;
                    List<float[]> c = new List<float[]>();
                    for (int n = 0; n < indexBwcount; n++)
                    {
                        int pos = (line * indexBwcount) + n + 1;
                        float[] data = new float[2];
                        object[] paras = new object[] { pos, data };
                        int value = (int)DeviceControl(DeviceName.误差板, "ReadInsulationCurrent", line, paras);
                        data = paras[1] as float[];
                        c.Add(data);
                        rst &= value == 0;
                        Task.Delay(80).Wait(1000);
                    }
                    return c;
                }));
            }
            Task.WaitAll(tasks.ToArray());
            foreach (var t in tasks)
            {
                currents.AddRange(t.Result);
            }
            return true;

        }
        /// <summary>
        /// 每路表位数
        /// </summary>
        public int BWCountPerLine
        {
            get
            {
                return EquipmentData.Equipment.MeterCount / EquipmentData.DeviceManager.DeviceCount;
            }
        }

        public string MeterWiringMode
        {
            get
            {
                try
                {
                    if (EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_WIRING_MODE")?.ToString().IndexOf("三相四线") >= 0)
                    {
                        return "三相四线";
                    }
                    else if (EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_WIRING_MODE")?.ToString().IndexOf("三相三线") >= 0)
                    {
                        return "三相三线";
                    }
                    else
                    {
                        return "单相";
                    }
                }
                catch
                {
                    return "三相四线";
                }
            }
        }

        /// <summary>
        /// 压表
        /// </summary>
        /// <returns></returns>
        public bool E_M_Down()
        {
            LogManager.AddMessage("电机下行...", EnumLogSource.设备操作日志);

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < EquipmentData.DeviceManager.DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < BWCountPerLine; n++)
                    {
                        int pos = (line * BWCountPerLine) + n + 1;
                        EquipmentData.DeviceManager.ElectricmachineryContrnl(0, (byte)pos, line);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return true;//以下限状态位为准
        }
        ///<summary>
        /// 抬表
        /// </summary>
        /// <returns></returns>
        public bool E_M_Up()
        {
            LogManager.AddMessage("电机上行...", EnumLogSource.设备操作日志);

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < EquipmentData.DeviceManager.DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < BWCountPerLine; n++)
                    {
                        int pos = (line * BWCountPerLine) + n + 1;
                        EquipmentData.DeviceManager.ElectricmachineryContrnl(1, (byte)pos, line);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return true;//以下限状态位为准
        }
        /// <summary>
        /// 读并判断下（ture）上（false）限位
        /// </summary>
        /// <param name="down"></param>
        /// <param name="errmsg">pos,</param>
        /// <returns></returns>
        public bool Read_Status(bool down, out string errmsg)
        {
            LogManager.AddMessage("检查电机位置...", EnumLogSource.设备操作日志);
            errmsg = string.Empty;
            int flag = down ? 2 : 1;
            List<Task<string>> status = new List<Task<string>>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                status.Add(Task.Run(() =>
                {
                    string notdown = "";
                    for (int n = 0; n < BWCountPerLine; n++)
                    {
                        int pos = (line * BWCountPerLine) + n + 1;
                        EquipmentData.DeviceManager.Read_Fault(3, (byte)pos, out byte[] outResult, line);
                        if (outResult[2] != flag && outResult[3] == 1)
                        {
                            notdown += $"{pos},";
                        }
                    }
                    return notdown;
                }));
            }
            Task.WaitAll(status.ToArray());

            foreach (var t in status)
            {
                errmsg += t.Result;
            }
            return status.All(t => string.IsNullOrWhiteSpace(t.Result));
        }


        public bool ControlYaoJianPositions()
        {
            LogManager.AddMessage($"设置表位电流继电器...", EnumLogSource.设备操作日志);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < BWCountPerLine; n++)
                    {
                        int pos = (line * BWCountPerLine) + n + 1;

                        if (EquipmentData.MeterGroupInfo.YaoJian[pos - 1])
                        {
                            EquipmentData.DeviceManager.ControlMeterRelay(2, (byte)pos, line);  //断开继电器
                        }
                        else
                        {
                            EquipmentData.DeviceManager.ControlMeterRelay(1, (byte)pos, line); //闭合继电器
                        }
                        Task.Delay(100).Wait(1000);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return true;
        }


        public bool ControlULoadPositions()
        {
           return ControlULoadPositions(EquipmentData.MeterGroupInfo.YaoJian);
        }

        public bool ControlULoadPositions(bool[] bws)
        {
            LogManager.AddMessage($"设置表位电压继电器...", EnumLogSource.设备操作日志);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < BWCountPerLine; n++)
                    {
                        int pos = (line * BWCountPerLine) + n + 1;

                        byte state = bws[pos - 1] ? (byte)0x00 : (byte)0x01;
                        ControlULoadRelay((byte)pos, state, line); // 接入电压

                        Task.Delay(100).Wait(1000);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return true;
        }

        public bool ClearOL()
        {
            string[] FrameAry = { };
            object[] paras = new object[1] { FrameAry };
            string value = DeviceControl(DeviceName.功率源, "ClearOL", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }
        /// <summary>
        /// 设置大电流长时间工作保护
        /// </summary>
        /// <param name="safe"></param>
        /// <returns></returns>
        public bool SetPowerSafe(bool safe)
        {
            bool mode = safe;
            object[] paras = new object[1] { mode };
            string value = DeviceControl(DeviceName.功率源, "SetPowerSafe", 0, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        #region 接地故障
        /// <summary>
        /// 接地故障
        /// </summary>
        /// <param name="bwIndex"></param>
        /// <param name="typeA"></param>
        /// <param name="typeB"></param>
        /// <param name="typeC"></param>
        /// <param name="typeN"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool SetJDGZContrnl(int bwIndex, int typeA, int typeB, int typeC, int typeN, int ID = 0)
        {
            string[] FrameAry = null;
            object[] paras = new object[] { bwIndex, typeA, typeB, typeC, typeN, FrameAry };
            string value = DeviceControl(DeviceName.接地故障抑制器, "SetJDGZContrnl", ID, paras).ToString();
            if (value == "0")
                return true;
            return false;
        }

        internal bool SetErrorData(int stopOrstart, int pushType, int meterConst, int qs)
        {
            object[] paras = new object[] { stopOrstart, pushType, meterConst, qs };
            object value = DeviceControl(DeviceName.功率源, "SetErrorData", 0, paras);
            if (value != null && value.ToString() == "0")
                return true;
            return false;
        }

        internal bool ReadErrorData(out float[] data)
        {
            data = new float[12];
            object[] paras = new object[] { data };
            object value = DeviceControl(DeviceName.功率源, "ReadErrorData", 0, paras);
            if (value != null && value.ToString() == "0")
            {
                data = (float[])paras[0];
                return true;
            }
            return false;
        }
        #endregion
    }

    public class DeviceName
    {
        public const string 误差板 = "误差板";
        public const string 标准表 = "标准表";
        public const string 功率源 = "功率源";
        public const string 时基源 = "时基源";
        public const string 多功能板 = "多功能板";
        public const string 互感器电机 = "互感器电机";
        public const string 载波 = "载波";
        public const string 偶次谐波 = "偶次谐波";
        public const string 功耗板 = "功耗板";
        public const string 直流功耗板 = "直流功耗板";
        public const string 载波信号控制板 = "载波信号控制板";
        public const string 耐压仪 = "耐压仪";
        public const string 测试板 = "测试板";
        public const string 温控板 = "温控板";
        //add yjt 20230103 新增零线电流板
        public const string 零线电流板 = "零线电流板";
        public const string 老载波 = "老载波";
        public const string 供断电控制板 = "供断电控制板";
        public const string 隔离互感器 = "隔离互感器";

        public const string 接地故障抑制器 = "接地故障抑制器";
        public const string 红外通信 = "红外通信";

    }
}
