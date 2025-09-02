using LYTest.Core.Enum;
using LYTest.Utility;
using LYTest.Utility.Log;
using LYTest.ViewModel;
using LYTest.ViewModel.Time;
using LYTest.WPF.SG.Model;
using LYTest.WPF.SG.View.Windows;
using LYTest.WPF.WanTe;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.WPF.SG
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly Window_TestControl verifyWindow = new Window_TestControl();

        /// <summary>
        /// 考试系统客户端
        /// </summary>
        //private TestSystemTCPClient Client { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = MainViewModel.Instance;
            MainViewModel.Instance.WindowsAll.CollectionChanged += WindowsAll_CollectionChanged;
            //MainViewModel.Instance.WindowsAll.change
            Loaded += MainWindow_Loaded;


            EquipmentData.DeviceManager.PropertyChanged += DeviceManager_PropertyChanged;
        }
        //MessageBoxResult boxResult = MessageBoxResult.None;
        private void DeviceManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TargetOverLoad")
            {
                if (EquipmentData.Schema.ParaNo == ProjectID.负载电流快速改变) return;

                Task.Run(() =>
                {
                    EquipmentData.Controller.MessageAdd($"输出电流异常({DateTime.Now:yyyy-MM-dd HH:mm:ss})", ViewModel.CheckController.EnumLogType.详细信息);
                });
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            ////检定窗体【开始检定，连续检定，停止检定三个按钮】
            if (DAL.Config.ConfigHelper.Instance.IsTestButtonSuspension)
            {
                verifyWindow.Show();
                verifyWindow.Owner = this;
            }
            if (Properties.Settings.Default.ShowTip)
            {
                Window_TipsMessage.Instance.Owner = this;
                Window_ThemeChoice.Instance.Owner = this;
            }
            Window_TipsStepFinished.Instance.Owner = this;

            if (EquipmentData.Equipment.VerifyModel == "自动模式")
            {

            }
            else
            {
                UiInterface.ChangeUi("参数录入", "View_Input");
            }


            ////新线程创建客户端
            //Thread clientThread = new Thread(() =>
            //{
            //    Client = new TestSystemTCPClient();
            //    if (Client != null && Client.Connected)
            //    {
            //        if (Client.SendLogin().GetAwaiter().GetResult())
            //        {
            //            LogManager.AddMessage("客户端登录成功。", EnumLogSource.服务器日志, EnumLevel.Information);
            //            // 注册收到考试开始帧事件
            //            Client.OnReceiveTestStart += () =>
            //            {
            //                MessageBox.Show("考试开始", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //            };
            //            Client.StartBeat();
            //            LogManager.AddMessage("启动心跳功能。", EnumLogSource.服务器日志, EnumLevel.Information);
            //        }
            //    }
            //    else
            //    {
            //        LogManager.AddMessage("客户端创建失败，请检查参数是否设置正确。", EnumLogSource.服务器日志, EnumLevel.Error);
            //    }
            //})
            //{ IsBackground = true };
            //clientThread.Start();

        }

        protected override void OnClosed(System.EventArgs e)
        {
            MainViewModel.Instance.WindowsAll.CollectionChanged -= WindowsAll_CollectionChanged;
            base.OnClosed(e);
            System.Environment.Exit(0);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (EquipmentData.Controller.IsChecking)
            {
                if (MessageBox.Show("程序正在检定,确认要退出吗?", "退出程序", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (MessageBox.Show("确认要退出检定吗?", "退出程序", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            //// 关闭客户端并发送登出帧
            //Thread clientThread = new Thread(() =>
            //{
            //    if (Client != null && Client.Connected)
            //    {
            //        Client.StopBeat();
            //        Client.SendLogout(false).GetAwaiter().GetResult();
            //        Client.Close();
            //        Client.Dispose();
            //    }
            //})
            //{ IsBackground = true };
            //clientThread.Start();

            ViewModel.EquipmentData.ApplicationIsOver = true;
            EquipmentData.DeviceManager.UnLink(); //退出时候关闭设备连接--关源

            verifyWindow.Close();
            WindowProcess.LYBackup();

            base.OnClosing(e);
        }

        #region 添加窗体


        /// <summary>
        /// 新添窗体时
        void WindowsAll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList list = e.NewItems;
            if (list == null)
                return;
            for (int i = 0; i < e.NewItems.Count; i++)
            {
                if (list[i] is DockWindowDisposable dockWindow && dockWindow.CurrentControl != null)
                {
                    AddDockWindow(dockWindow);
                }
            }
        }

        private void AddDockWindow(DockWindowDisposable dockWindow)
        {
            DockSideE dockSide = dockWindow.CurrentControl.DockStyle.Position;
            if (dockSide == DockSideE.Tab)
            {
                if (dockWindow.CurrentControl.DockStyle.IsFloating) //是否悬浮
                {
                    Window_Show show = new Window_Show();
                    show.LoaiUserWindow(dockWindow);
                    show.Closing += Show_Closing;
                    show.Show();

                }
                else
                {
                    MainTabContro.TabItemClose tabItem = new MainTabContro.TabItemClose
                    {
                        MinHeight = 25,
                        MinWidth = 80,
                        Margin = new Thickness(0),
                        LogoPadding = new Thickness(10, 0, 3, 0)
                    };

                    if (dockWindow.ImageControl != null)
                    {
                        tabItem.LogoIcon = dockWindow.ImageControl;
                        tabItem.LogoIconWidth = 16;
                        tabItem.LogoIconHeigth = 16;
                    }
                    tabItem.Content = dockWindow;
                    tabItem.Header = dockWindow.Name;

                    AppDock2.Items.Add(tabItem);
                    AppDock2.SelectedIndex = AppDock2.Items.Count - 1;
                    dockWindow.Index = AppDock2.Items.Count - 1;
                }
            }

        }
        private void Show_Closing(object sender, CancelEventArgs e)
        {
            Activate();
        }
        #endregion

        #region 状态栏数据  

        private bool initialFlag = false;
        protected override void OnActivated(System.EventArgs e)
        {
            if (!initialFlag)
            {
                BindingStatusBar();
                initialFlag = true;
            }
            base.OnActivated(e);
        }

        /// <summary>
        /// 状态栏数据源绑定
        /// </summary>
        private void BindingStatusBar()
        {
            //右边检验员进度条等数据
            stackPanel_CheckerUser.DataContext = EquipmentData.LastCheckInfo;
            //联机图标数据源
            imageEqupmentStatus.DataContext = EquipmentData.DeviceManager;
            textEqupmentStatus.DataContext = EquipmentData.DeviceManager;
            //检定模式
            textBlockTestMode.DataContext = EquipmentData.Controller;
            //检定方案名称
            //textBlockSchemaName.DataContext = EquipmentData.SchemaModels;
            //当前步骤
            textBlockCheckIndex.DataContext = EquipmentData.Controller;
            //当前检定项目
            textBlockCheckName.DataContext = EquipmentData.Controller;

            lightCheckStatus.DataContext = EquipmentData.Controller;

            TipsStr.DataContext = EquipmentData.Controller;

            stackPanelTime.DataContext = TimeMonitor.Instance;
            _loading.DataContext = EquipmentData.Controller;

            //监视窗口
            monitorGrid.DataContext = EquipmentData.Controller;
        }

        #endregion
    }
}
