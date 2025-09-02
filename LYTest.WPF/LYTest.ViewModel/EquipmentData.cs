using LYTest.Core.Enum;
using LYTest.Core.Model.Meter;
using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.Mis;
using LYTest.Mis.Common;
using LYTest.Mis.HEB;
using LYTest.Mis.Houda;
using LYTest.Mis.XiAn;
using LYTest.Utility;
using LYTest.Utility.Log;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.Device;
using LYTest.ViewModel.InputPara;
using LYTest.ViewModel.Monitor;
using LYTest.ViewModel.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LYTest.ViewModel
{
    /// <summary>
    /// 检定数据中心
    /// </summary>
    public class EquipmentData
    {
        /// <summary>
        /// 测试版标记
        /// </summary>
        public static bool Beta = false;

        /// <summary>
        /// 指示当前批次表是否已经组网。参数录入和登录后此值为False,组网后为True
        /// </summary>
        public static bool IsHplcNet { get; set; }

        public static HeiLongJian heiLongJian = null;
        public static IMis HeiLongJIanMis = null;

        public static XiAnProject xiAnProject = null;

        internal static void IMICPKz(string type)
        {
            switch (type)
            {
                case "01":
                    Controller.StopVerify();
                    break;
                case "02":
                    Controller.RunningVerify();
                    break;
                case "03":
                    Controller.StopVerify();
                    break;
                default:
                    break;
                  
            }
        }

        /// <summary>
        /// 参数录入是否读取表号等级常数地址
        /// </summary>
        public static int IsReadMeterInfo = 1;


        /// <summary>
        /// 台体信息
        /// </summary>
        public static EquipmentViewModel Equipment { get; } = new EquipmentViewModel();


        #region 营销系统部分
        //private static ServiceHost host = null;
        /// <summary>
        /// 通知消息事件
        /// </summary>
        public static event EventHandler<EventArgs> CallMsgEvent = null;

        public static void CallMsg(string cmd)
        {
            CallMsgEvent?.Invoke(cmd, new EventArgs());
        }
        /// <summary>
        /// 应用程序是否已经退出.用于结束线程
        /// </summary>
        public static bool ApplicationIsOver { get; set; }

        /// <summary>
        /// 与厚达通讯进行Tcp通讯接口业务过程
        /// </summary>
        public static HoudaInteraction HouTcpCommunication = null;
        #endregion

        /// <summary>
        /// 表信息数据集合
        /// </summary>
        public static MeterInputParaViewModel MeterGroupInfo { get; } = new MeterInputParaViewModel();
        #region 方案


        private static SchemaOperationViewModel schemaModels;
        /// <summary>
        /// 方案列表
        /// </summary>
        public static SchemaOperationViewModel SchemaModels
        {
            get
            {
                if (schemaModels == null)
                {
                    schemaModels = new SchemaOperationViewModel();
                    SchemaModels.PropertyChanged += SchemaModels_PropertyChanged;
                }
                return schemaModels;
            }
        }

        // 更改检定方案时的事件
        private static void SchemaModels_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "SelectedSchema")
            {
                if (SchemaModels.SelectedSchema == null)
                {
                    return;
                }
                TaskManager.AddDataBaseAction(() =>
                {
                    int schemaId = (int)SchemaModels.SelectedSchema.GetProperty("ID");

                    Schema.LoadSchema(schemaId);
                    Equipment.TextLogin = "初始化检定结论信息...";
                    CheckResults.InitialResult();

                    SchemaModels.EndWaitRefreshSchema();
                });
            }
        }


        /// <summary>
        /// 检定方案
        /// </summary>
        public static SchemaViewModel Schema { get; } = new SchemaViewModel();



        #endregion



        /// <summary>
        /// 检定结论
        /// </summary>
        public static CheckResultViewModel CheckResults { get; } = new CheckResultViewModel();


        /// <summary>
        /// 检定软件退出时的软件信息
        /// </summary>
        public static LastCheckInfoViewModel LastCheckInfo { get; } = new LastCheckInfoViewModel();


        /// <summary>
        /// 数据标识信息
        /// </summary>
        public static DISetsModel.DISetsModel DISetsInfo { get; } = new DISetsModel.DISetsModel();

        /// <summary>
        /// 标准表信息
        /// </summary>
        public static StdInfoViewModel StdInfo { get; } = new StdInfoViewModel();


        /// <summary>
        /// 初始化检定数据
        /// </summary>
        public static void Initialize()
        {
            Equipment.TextLogin = "正在加载表信息...";
            //MeterGroupInfo.Initialize();
            Equipment.TextLogin = "正在加载设备信息...";
            DeviceManager.LoadDevices();  // 【标注002】
            //加载检定初始数据
            LastCheckInfo.LoadLastCheckInfo();
            Equipment.TextLogin = "正在加载方案信息...";
            //加载检定方案
            SchemaModels.SelectedSchema = SchemaModels.Schemas.FirstOrDefault(item => (int)item.GetProperty("ID") == LastCheckInfo.SchemaId);
            Controller.Index = LastCheckInfo.CheckIndex;

            LoadInfoFormMeter();

            if (!Equipment.IsDemo)
            {

                switch (ConfigHelper.Instance.Marketing_Type)
                {
                    case "厚达":
                        if (ConfigHelper.Instance.VerifyModel == "自动模式")
                        {
                            //初始化与厚达通讯 模块。
                            string HouDaIP = ConfigHelper.Instance.SetControl_Ip;
                            int HouDaPort = int.Parse(ConfigHelper.Instance.SetControl_Port);
                            string XianTiCode = Equipment.ID;
                            int iHdPort = Convert.ToInt32(HouDaPort);
                            HouTcpCommunication = new HoudaInteraction(HouDaIP, iHdPort, XianTiCode);
                            HouTcpCommunication.UpdataStatusLabel += new EventHandler<EventArgs>(HouTcpCommunication_UpdataStatusLabel);
                            HouTcpCommunication.RevcMessageEvent += new EventHandler<XmlMsg>(HouTcpCommunication_RevcMessageEvent);
                        }
                        break;
                    case "黑龙江调度平台":
                        if (ConfigHelper.Instance.VerifyModel == "自动模式")
                        {
                            string ServiceIP = ConfigHelper.Instance.SetControl_Ip;
                            int ServicePort = int.Parse(ConfigHelper.Instance.SetControl_Port);
                            string StringsbUri = $"net.tcp://{ServiceIP}:{ServicePort}";
                            heiLongJian = new HeiLongJian(ServiceIP, ServicePort, null, null, null, StringsbUri);
                            heiLongJian.RunningVerifyEvent += HeiLongJian_RunningVerify;
                        }

                        break;
                    case "西安调度平台":
                        if (ConfigHelper.Instance.VerifyModel == "自动模式")
                        {
                            string XiAnServiceIP = ConfigHelper.Instance.SetControl_Ip;
                            int XiAnServicePort = int.Parse(ConfigHelper.Instance.SetControl_Port);
                            string XiAnStringsbUri = $"net.tcp://{XiAnServiceIP}:{XiAnServicePort}";
                            xiAnProject = new XiAnProject(XiAnServiceIP, XiAnServicePort, null, null, null, XiAnStringsbUri);
                            xiAnProject.RunningVerifyEvent += XiAnProject_RunningVerifyEvent; ;
                        }
                        break;
                }
            }

            CallMsgEvent += new EventHandler<EventArgs>(GlobalUnit_CallMsgEvent);
            Thread.Sleep(100);


            Controller.DeviceStart = -1;  //开始台体状态-1
            VerifyBase.MeterInfo = MeterGroupInfo.GetVerifyMeterInfo();  //载入表信息
                                                                         //载入系统设置

            //初始化所有设备
            DeviceManager.InitializeDevice();

            if (ConfigHelper.Instance.VerifyModel == "自动模式")
            {
                //string centerIp = ConfigHelper.Instance.SetControl_Ip;
                if (!int.TryParse(ConfigHelper.Instance.SetControl_Port, out int centerPort))
                {
                    centerPort = 9999;
                }

                Task.Run(() =>
                {
                    while (true)
                    {
                        if (DeviceManager.IsConnected != true)
                        {
                            DeviceManager.Link();
                            DeviceManager.LinkDog();
                        }
                        Task wait = Task.Delay(60000);
                        wait.Wait();
                    }
                }).Wait(1000);
            }
            else
            {
                Utility.TaskManager.AddWcfAction(() =>
                {

                    //连接设备
                    DeviceManager.Link();
                    ////连接加密机
                    DeviceManager.LinkDog();

                });
            }


            #region 智慧工控平台
            if (ConfigHelper.Instance.OperatingConditionsYesNo == "是")
            {
                IMICPGK(true);
            }
            #endregion
        }
        public static void IMICPGK(bool v)
        {
            ViewModel.IMICP.OpenPortIMICP open = new ViewModel.IMICP.OpenPortIMICP();

            if (v)
            {

                string ip = ConfigHelper.Instance.OperatingConditionsIp.Trim();
                string port = ConfigHelper.Instance.OperatingConditionsProt.Trim();
                string pl = ConfigHelper.Instance.OperatingConditionsUpdataF.Trim();
                open.openFWQ();
                //open.button13_Click();
                open.Updata(ip, port, pl);
            }
            else
            {
                open.EndApi();
            }
        }
        //modify yjt 20230131 合并蒋工西安代码
        /// <summary>
        /// 西安流水线
        /// </summary>
        private static void XiAnProject_RunningVerifyEvent()
        {
            MeterGroupInfo.NewMetersAuto();   //换新表
            Thread.Sleep(1000);
            MeterGroupInfo.Frame_DownMeterInfoFromMis();
            Thread.Sleep(500);
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new
                    DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl =>
                {
                    MeterGroupInfo.Frame_DownSchemeMis_XiAn();   //下载方案
                    Thread.Sleep(500);
                    MeterGroupInfo.UpdateMeterInfoAuto(out string msg); //更新电表数据，跳转到检定界面
                    Thread.Sleep(200);
                    //开始执行检定方案
                    Controller.Index = 0; //设置从第一项开始检定
                    Controller.RunningVerify();

                }, null);
            });
        }

        /// <summary>
        /// 黑龙江流水线
        /// </summary>
        static void HeiLongJian_RunningVerify()
        {

            MeterGroupInfo.NewMetersAuto();   //换新表
            Thread.Sleep(1000);
            MeterGroupInfo.Frame_DownMeterInfoFromMis();
            Thread.Sleep(500);
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new
                    DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl =>
                {
                    MeterGroupInfo.Frame_DownSchemeMis_HLJ();   //下载方案
                    Thread.Sleep(500);
                    MeterGroupInfo.UpdateMeterInfoAuto(out string msg); //更新电表数据，跳转到检定界面
                    Thread.Sleep(200);
                    //开始执行检定方案
                    Controller.Index = 0; //设置从第一项开始检定
                    Controller.RunningVerify();

                }, null);
            });

        }



        ///// <summary>
        ///// 连接设备
        ///// </summary>
        //private static void LinkDevice()
        //{
        //    //初始化所有设备
        //    DeviceManager.InitializeDevice();
        //    //连接设备
        //    DeviceManager.Link();
        //    ////连接加密机
        //    DeviceManager.LinkDog();
        //}

        /// <summary>
        /// 导航到当前的默认界面
        /// </summary>
        public static void NavigateCurrentUi()
        {
            //【标注】先用着测试
            UiInterface.ChangeUi("运行日志", "View_Log");
            //UiInterface.ChangeUi("标准表信息", "View_MeterMessage");
            UiInterface.ChangeUi("标准表信息", "View_StdMessage");
            //UiInterface.ChangeUi("检定", "View_Test");
            //return;
            //根据检定点序号来加载不同检定界面
            if (LastCheckInfo.CheckIndex == -1)     //参数录入 
            {
                UiInterface.ChangeUi("参数录入", "View_Input");
            }
            else if (LastCheckInfo.CheckIndex == -3)        //审核存盘
            {
            }
            else                    //检定界面
            {
                if (MeterGroupInfo.CheckInfoCompleted(out _))
                {
                    UiInterface.ChangeUi("检定", "View_Test");
                }
                else
                {
                    UiInterface.ChangeUi("参数录入", "View_Input");
                    LogManager.AddMessage("", EnumLogSource.用户操作日志, EnumLevel.Warning);
                }
            }
        }



        public static ControllerViewModel Controller { get; } = new ControllerViewModel();


        public static DeviceViewModel DeviceManager { get; } = new DeviceViewModel();

        #region Socket
        //处理连接指示工作
        static void HouTcpCommunication_UpdataStatusLabel(object sender, EventArgs e)
        {
            string strMsg = sender as string;
            switch (strMsg)
            {
                case "connect":
                    {
                        //m_Frame.NetState = Cus_NetState.DisConnected;
                        LogManager.AddMessage("开始连接服务器...");
                    }
                    break;
                case "connectSuccess":
                    {
                        //m_Frame.NetState = Cus_NetState.Connected;
                        LogManager.AddMessage("连接服务器成功");
                        //m_Frame.NetState = Cus_NetState.Connected;
                    }
                    break;
                case "disconNexion":
                    {
                        //m_Frame.NetState = Cus_NetState.DisConnected;
                        //断开连接
                        LogManager.AddMessage("断开后重连服务器");
                    }
                    break;
                default:
                    break;
            }

        }


        /// <summary>
        /// 处理返回的服务端接口数据
        /// </summary>
        static void HouTcpCommunication_RevcMessageEvent(object sender, Mis.Houda.XmlMsg serverMsg)
        {
            //添加服务器指示 数据操作
            switch (serverMsg.MessageAttribute)
            {


                case Mis.Houda.NetworkMessageAttribute.应答指令9999:
                    {
                        //不做处理
                    }
                    break;
                case Mis.Houda.NetworkMessageAttribute.测试通知1004:
                    {
                        Mis.Houda.XmlMsg xmlmsg = new Mis.Houda.XmlMsg();
                        xmlmsg.headMsg.ToRecive = "Main";
                        xmlmsg.headMsg.CmdType = "1";
                        xmlmsg.headMsg.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        xmlmsg.headMsg.Command = "9999";
                        xmlmsg.headMsg.Seq = serverMsg.headMsg.Seq;
                        HouTcpCommunication.SendNetworkMessage(xmlmsg);
                        MeterGroupInfo.NewMetersAuto();   //换新表
                        Thread.Sleep(1000);
                        MeterGroupInfo.Frame_DownMeterInfoFromMis();
                        Thread.Sleep(500);
                        MeterGroupInfo.Frame_DownSchemeMis();   //下载方案
                        Thread.Sleep(500);
                        MeterGroupInfo.UpdateMeterInfoAuto(out _); //更新电表数据，跳转到检定界面
                        Thread.Sleep(200);
                        //开始执行检定方案
                        Controller.Index = 0; //设置从第一项开始检定
                        Controller.RunningVerify();
                    }
                    break;
                case Mis.Houda.NetworkMessageAttribute.台体控制指令1006:
                    {
                        Mis.Houda.XmlMsg xmlmsg = new Mis.Houda.XmlMsg();
                        xmlmsg.headMsg.ToRecive = "Main";
                        xmlmsg.headMsg.CmdType = "1";
                        xmlmsg.headMsg.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        xmlmsg.headMsg.Command = "9999";
                        xmlmsg.headMsg.Seq = serverMsg.headMsg.Seq;

                        HouTcpCommunication.SendNetworkMessage(xmlmsg);

                        #region 服务端发送控制指令
                        switch (serverMsg.OP)
                        {
                            //操作类型： 0-不执行； 1-暂停检定； 2-停止检定；3-继续检定； 9-关闭计算机
                            case "0":
                                {
                                    //    Frame_StopAdpater();
                                    Controller.StopVerify();
                                }
                                break;
                            case "1":
                                {
                                    //Frame_StopAdpater();
                                    Controller.StopVerify();
                                }
                                break;
                            case "2":
                                {
                                    //Frame_StopAdpater();
                                    Controller.StopVerify();

                                }
                                break;
                            case "3":
                                {
                                    //ActiveItemID
                                    //App.CUS.CheckState = Cus_CheckStaute.检定;
                                    //IsChecking = true;
                                    //Frame_StartAdpater(App.CUS.ActiveItemID);
                                    Controller.RunningVerify();
                                }
                                break;
                            case "9":
                                {
                                    //Controller.DeviceStart = 9;
                                    //关闭计算机
                                    Process process1 = new Process();
                                    process1.StartInfo.FileName = "shutdown";
                                    process1.StartInfo.Arguments = "-s -t 5";
                                    process1.Start();
                                }
                                break;
                            case "11":
                                {
                                    //TODO检定完成后开始自动保存数据
                                    // m_Frame.StartSaveData();
                                }
                                break;
                            default:
                                break;
                        }
                        #endregion
                    }
                    break;
                #region 丢弃


                //case Mis.Houda.NetworkMessageAttribute.测试通知1004:
                //    {
                //        Mis.Houda.XmlMsg xmlmsg = new Mis.Houda.XmlMsg();
                //        xmlmsg.headMsg.ToRecive = "Main";
                //        xmlmsg.headMsg.CmdType = "1";
                //        xmlmsg.headMsg.CurrentTime = DateTime.Now.ToString("YYYY.MM.DD HH:mm:ss");
                //        xmlmsg.headMsg.Command = "9999";
                //        xmlmsg.headMsg.Seq = serverMsg.headMsg.Seq;

                //        HouTcpCommunication.SendNetworkMessage(xmlmsg);
                //        //开始检定
                //        // TODO先下载信息
                //        MeterGroupInfo.Frame_DownMeterInfoFromMis();
                //        Thread.Sleep(200);

                //        //开始执行检定方案
                //        controller.Index = 1; //设置从第一项开始检定
                //        controller.RunningVerify();
                //        //m_Frame.StartSolutionVerify();
                //    }
                //    break;
                //case Mis.Houda.NetworkMessageAttribute.查找表位1109:
                //    {
                //        //先回复 收到命令指令
                //        Mis.Houda.XmlMsg xmlmsgHf = new Mis.Houda.XmlMsg();
                //        xmlmsgHf.headMsg.ToRecive = "Main";
                //        xmlmsgHf.headMsg.CmdType = "1";
                //        xmlmsgHf.headMsg.CurrentTime = string.Empty;
                //        xmlmsgHf.headMsg.Command = "9999";
                //        xmlmsgHf.headMsg.Seq = serverMsg.headMsg.Seq;
                //        HouTcpCommunication.SendNetworkMessage(xmlmsgHf);

                //        string[] bwHaveMeter = GetRepeatPressMeters();
                //        Mis.Houda.XmlMsg xmlmsg = new Mis.Houda.XmlMsg();
                //        xmlmsg.headMsg.ToRecive = "Main";
                //        xmlmsg.headMsg.Seq = serverMsg.headMsg.Seq;
                //        xmlmsg.headMsg.CmdType = "1";
                //        xmlmsg.headMsg.CurrentTime = string.Empty;
                //        xmlmsg.headMsg.Command = "2109";
                //        xmlmsg.Positionns = serverMsg.Positionns;

                //        xmlmsg.RobotId = serverMsg.RobotId;// 机器人编号
                //        xmlmsg.BwHaveMeterStatus = bwHaveMeter;//表位状态
                //        HouTcpCommunication.SendNetworkMessage(xmlmsg);
                //    }
                //    break;
                #endregion

                default:
                    break;
            }

        }

        /// <summary>
        /// 全局通知消息事件
        /// </summary>
        static void GlobalUnit_CallMsgEvent(object sender, EventArgs e)
        {

            //通知检定事件已经完成
            string strMsg = sender as string;
            switch (strMsg)
            {
                case "VerifyCompelate":
                    {
                        //检定完成后开始自动保存数据 延时等待检定线程结束
                        LogManager.AddMessage("检定完成开始保存数据!");
                        Thread.Sleep(500);

                        switch (ConfigHelper.Instance.Marketing_Type)
                        {
                            case "厚达":
                            case "黑龙江调度平台":
                            case "西安调度平台":
                            case "新疆生产调度平台":
                                if (ConfigHelper.Instance.VerifyModel == "自动模式")
                                {
                                    UpMeterData();
                                }
                                break;
                            case "天津MIS接口":
                                UpMeterData();
                                break;
                        }
                    }
                    break;
                case "CompelateOneBatch":  //检定完成
                    {
                        if (ConfigHelper.Instance.Marketing_Type == "新疆生产调度平台" && ConfigHelper.Instance.VerifyModel == "自动模式")
                        {
                        }
                        else if (ConfigHelper.Instance.Marketing_Type == "厚达" && ConfigHelper.Instance.VerifyModel == "自动模式")
                        {
                            //检定完成
                            Mis.Houda.XmlMsg xmlmsg = new Mis.Houda.XmlMsg
                            {
                                Err = "0",
                                Des = ""
                            };
                            //xmlmsg.headMsg.FromStart = EquipmentData.equipment.ID;
                            xmlmsg.headMsg.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            xmlmsg.headMsg.ToRecive = "Main";
                            xmlmsg.headMsg.CmdType = "1";
                            //xmlmsg.headMsg.CurrentTime = string.Empty;
                            xmlmsg.headMsg.Command = "2005";
                            //Controller.DeviceStart = 4;
                            LogManager.AddMessage("发送检定完成消息给服务器!");
                            if (HouTcpCommunication != null)
                                HouTcpCommunication.SendNetworkMessage(xmlmsg);
                            RestareWindow();
                        }
                    }
                    break;
                case "DeviceState":     //台体状态
                    if (ConfigHelper.Instance.Marketing_Type == "新疆生产调度平台" && ConfigHelper.Instance.VerifyModel == "自动模式")
                    {
                    }
                    else if (ConfigHelper.Instance.Marketing_Type == "厚达" && ConfigHelper.Instance.VerifyModel == "自动模式")
                    {
                        //检定完成
                        Mis.Houda.XmlMsg xmlmsg = new Mis.Houda.XmlMsg
                        {
                            Err = "0",
                            Des = ""
                        };
                        xmlmsg.headMsg.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        xmlmsg.headMsg.ToRecive = "Main";
                        xmlmsg.headMsg.CmdType = "1";
                        xmlmsg.headMsg.Command = "2008";
                        xmlmsg.DeviceStart = Controller.DeviceStart.ToString();
                        // < BODY >
                        //< DESKID > 台体编号 </ DESKID >
                        //   < STATUS > 台体状态 </ STATUS >
                        //</ BODY >

                        //台体状态 ： 0-空闲； 1-检测中； 2-暂停；3-停止；4-完成； 5-关机中；8-不合格率报警 9-异常
                        LogManager.AddMessage("发送台体状态消息给服务器!");
                        if (HouTcpCommunication != null)
                            HouTcpCommunication.SendNetworkMessage(xmlmsg);
                    }
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// 重启电脑
        /// </summary>
        public static void RestareWindow()
        {
            Thread.Sleep(1000);
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                p.StandardInput.WriteLine("shutdown -r -t 1");
                p.StandardInput.WriteLine("exit");
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 上传数据到中间库，先从本地数据库读取表数据，然后转换上传
        /// </summary>
        /// <param name="meterInfos"></param>
        public static void UpMeterData()
        {
            LogManager.AddMessage("开始上传中间库...");
            #region 上传到中间库
            bool blnTotalUpRst = true;
            int iUpdateOkSum = 0;

            //20221018jx黑龙江添加
            float FailureResultRatio = 0;

            IMis mis = MISFactory.Create();
            mis.UpdateInit();


            // TODO新版本数据上传，速度比原版本提高1000倍以上，还需要测试才能投入使用
            TestMeterInfo[] testMeters;

            switch (ConfigHelper.Instance.Marketing_Type)
            {
                case "黑龙江调度平台":
                    if (ConfigHelper.Instance.VerifyModel == "自动模式")
                    {
                        testMeters = MeterResoultModel.MeterDataHelper_HLJ.GetDnbInfoNew();
                        bool bUpdateOk = heiLongJian.UpdateALL(testMeters, ref FailureResultRatio);
                        if (!bUpdateOk)
                        {
                            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 1);
                            LogManager.AddMessage(string.Format("上传到生产调度中间库失败，条形码"), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                            return;
                        }
                        else
                        {
                            if (FailureResultRatio > ConfigHelper.Instance.FailureRate)
                            {
                                if (MessageBox.Show($@" 合格率标准值为{ConfigHelper.Instance.FailureRate}，此次检定的不合格率为{FailureResultRatio}，不达标，是否确认要上传检定完成信息？", "检定完成", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.No)
                                {
                                    LogManager.AddMessage(string.Format("上传检定完成信息失败!"), EnumLogSource.数据库存取日志, EnumLevel.Information);
                                    return;
                                }
                                else
                                {
                                    heiLongJian.UpdateCompleted();
                                    LogManager.AddMessage(string.Format("上传检定完成信息成功!"), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                                }
                            }
                            else
                            {
                                heiLongJian.UpdateCompleted();
                                LogManager.AddMessage(string.Format("上传检定完成信息上传成功!"), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                            }
                        }
                    }

                    break;
                case "西安调度平台":
                    if (ConfigHelper.Instance.VerifyModel == "自动模式")
                    {
                        testMeters = MeterResoultModel.MeterDataHelper_XiAn.GetDnbInfoNew();
                        bool bUpdateOk = xiAnProject.UpdateALL(testMeters, ref FailureResultRatio);
                        if (!bUpdateOk)
                        {
                            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 1);
                            LogManager.AddMessage(string.Format("上传到生产调度中间库失败，条形码"), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                            return;
                        }
                        else
                        {

                            if (FailureResultRatio > ConfigHelper.Instance.FailureRate)
                            {
                                if (MessageBox.Show($@" 合格率标准值为{ConfigHelper.Instance.FailureRate}，此次检定的不合格率为{FailureResultRatio}，不达标，是否确认要上传检定完成信息？", "检定完成", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.No)
                                {
                                    LogManager.AddMessage(string.Format("上传检定完成信息失败!"), EnumLogSource.数据库存取日志, EnumLevel.Information);
                                    return;
                                }
                                else
                                {
                                    xiAnProject.UpdateCompleted();
                                    LogManager.AddMessage(string.Format("上传检定完成信息成功!"), EnumLogSource.数据库存取日志, EnumLevel.Warning);

                                }
                            }
                            else
                            {
                                xiAnProject.UpdateCompleted();
                                LogManager.AddMessage(string.Format("上传检定完成信息上传成功!"), EnumLogSource.数据库存取日志, EnumLevel.Warning);

                            }

                        }
                    }
                    break;
                default:
                    #region 通用上传
                    testMeters = MeterResoultModel.MeterDataHelper.GetDnbInfoNew();

                    for (int i = 0; i < testMeters.Length; i++)
                    {
                        TestMeterInfo temmeter = testMeters[i];
                        if (temmeter.Other1 != "1")
                        {
                            continue;//不需要上传的话就跳过
                        }
                        bool bUpdateOk = mis.Update(temmeter);
                        if (!bUpdateOk)
                        {
                            blnTotalUpRst = false;
                            LogManager.AddMessage(string.Format("上传到生产调度中间库失败，条形码{0}", temmeter.MD_BarCode), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                            continue;
                        }
                        else
                        {
                            iUpdateOkSum++;
                        }
                        LogManager.AddMessage(string.Format("电能表[{0}]检定记录上传成功!", temmeter.MD_BarCode), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                        SetMeterData(temmeter, i);
                    }
                    LogManager.AddMessage($"{iUpdateOkSum}块电表信息上传中间库完成,通知平台...");
                    if (blnTotalUpRst)        //TODO这里写抬起电机，然后发送检定完成
                    {
                        LogManager.AddMessage("电表信息全部上传完成,开始抬起电机...");
                        DeviceManager.E_M_Up();  //取出表

                        int time = 15;
                        while (time > 0)
                        {
                            Controller.MessageTips = $"松开压接电机等待{time}秒";
                            Thread.Sleep(1000);
                            time--;
                        }
                        Controller.MessageTips = $"开始读取表位状态";
                        //读取表位状态
                        bool allup = DeviceManager.Read_Status(false, out string errmsg);  //读取表位的状态
                        if (!allup) allup = DeviceManager.Read_Status(false, out errmsg);

                        if (!allup)
                        {
                            LogManager.AddMessage($"有表位未松开:{errmsg}", EnumLogSource.设备操作日志, EnumLevel.Warning);
                            Controller.DeviceStart = 9; //状态异常
                        }
                        else
                        {
                            LogManager.AddMessage("表位状态正常,检定完成。");
                            Controller.MessageTips = "表位状态正常,检定完成";
                            //通知一批表检定完成
                            CallMsg("CompelateOneBatch");
                        }
                    }
                    else
                    {
                        LogManager.AddMessage("上传失败！手动上传成功后，再抬表、摘表！", EnumLogSource.用户操作日志, EnumLevel.Error);
                    }
                    #endregion
                    break;
            }

            #region 老版本从数据库中取数据，效率太低
            //老版本从数据库中取数据，效率太低
            //foreach (TestMeterInfo meter in meterInfos)
            //{
            //    if (!meter.YaoJianYn && ((meter.Meter_ID == null || meter.Meter_ID.Trim() == "") || (meter.MD_BarCode == null || meter.MD_BarCode.Trim() == ""))) continue;
            //    try
            //    {
            //        LogManager.AddMessage(string.Format($"开始上传表ID【{meter.Meter_ID}】"), EnumLogSource.数据库存取日志, EnumLevel.Information);
            //    }
            //    catch (Exception)
            //    {
            //    }
            //    TestMeterInfo temmeter;
            //    try
            //    {
            //        temmeter = Mis.DataHelper.DataManage.GetDnbInfoNew(meter, false);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogManager.AddMessage(string.Format("上传到生产调度中间库失败1，条形码{0}" + ex, meter.MD_BarCode), EnumLogSource.数据库存取日志, EnumLevel.Error);
            //        continue;
            //    }
            //    LogManager.AddMessage("电表数据获取成功,开始上传", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Information);
            //    bool bUpdateOk = false;
            //    try
            //    {
            //        bUpdateOk = mis.Update(temmeter);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogManager.AddMessage(string.Format("上传到生产调度中间库失败2，条形码{0}" + ex, meter.MD_BarCode), EnumLogSource.数据库存取日志, EnumLevel.Error);
            //        continue;
            //    }

            //    if (!bUpdateOk)
            //    {
            //        blnTotalUpRst = false;
            //        LogManager.AddMessage(string.Format("上传到生产调度中间库失败3，条形码{0}", temmeter.MD_BarCode), EnumLogSource.数据库存取日志, EnumLevel.Error);
            //        continue;
            //    }
            //    else
            //    {
            //        iUpdateOkSum++;
            //    }
            //    LogManager.AddMessage(string.Format("电能表[{0}]检定记录上传成功!", temmeter.MD_BarCode), EnumLogSource.数据库存取日志, EnumLevel.Warning);
            //}

            //mis.UpdateCompleted();
            #endregion

            #endregion
        }

        /// <summary>
        /// 修改数据库里面的上传标识
        /// </summary>
        /// <param name="meter"></param>
        private static void SetMeterData(TestMeterInfo temmeter, int index)
        {
            try
            {
                temmeter.Other2 = "已上传";
                List<string> fieldNames = new List<string>() { "MD_OTHER_2" };//更新上传的状态
                List<DynamicModel> models = new List<DynamicModel>();
                DynamicModel model = new DynamicModel();
                model.SetProperty("METER_ID", temmeter.Meter_ID);
                model.SetProperty("MD_OTHER_2", temmeter.Other2);
                models.Add(model);
                MeterGroupInfo.Meters[index].SetProperty("MD_OTHER_2", "已上传");
                int updateCount = DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", "METER_ID", models, fieldNames);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("修改上传状态出错!\r\n" + ex.ToString(), EnumLogSource.数据库存取日志, EnumLevel.TipsError);
            }


        }
        #endregion


        private static void LoadInfoFormMeter()
        {
            try
            {
                string temp = Core.OperateFile.GetINI("Config", "IsReadMeterInfo", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
                if (string.IsNullOrWhiteSpace(temp))
                {
                    IsReadMeterInfo = 1;
                }
                else
                {
                    IsReadMeterInfo = int.Parse(temp);
                }
            }
            catch
            {
                IsReadMeterInfo = 1;

            }
        }
    }
}
