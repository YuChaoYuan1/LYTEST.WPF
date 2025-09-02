using LYTest.Core.Enum;
using LYTest.Utility;
using LYTest.ViewModel;
using LYTest.ViewModel.Time;
using LYTest.WPF.Model;
using LYTest.WPF.Skin;
using LYTest.WPF.View.Windows;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LYTest.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly Window_TestControl verifyWindow = new Window_TestControl();
        public MainWindow()
        {
            InitializeComponent();

            Icon = new BitmapImage(new Uri($"./images/{AppInfo.Icon}", UriKind.Relative));
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

            //if (EquipmentData.DeviceManager.IsConnected == true)
            {
                if (EquipmentData.Equipment.VerifyModel == "自动模式")
                {

                }
                else
                {
                    UiInterface.ChangeUi("参数录入", "View_Input");
                }
            }
            //EquipmentData.NavigateCurrentUi(); //导航到默认界面


            //窗体主题颜色，通过name进行修改


            string ThemeName = Core.OperateFile.GetINI("SkinList", "SkinCurrent", System.IO.Directory.GetCurrentDirectory() + "\\Skin\\SkinData.ini", "blue").Trim();  //获得当前设计的颜色
            ThemeItem itemTemp;
            if (string.IsNullOrWhiteSpace(ThemeName))
            {
                itemTemp = SkinViewModel.Instance.Themes.FirstOrDefault(item => item.Name == "blue");
            }
            else
            {
                itemTemp = SkinViewModel.Instance.Themes.FirstOrDefault(item => item.Name == ThemeName);
            }


            itemTemp?.Load();

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

            ViewModel.EquipmentData.ApplicationIsOver = true;
            EquipmentData.DeviceManager.UnLink(); //退出时候关闭设备连接--关源

            verifyWindow.Close();
            WindowProcess.LYBackup();

            base.OnClosing(e);
        }

        #region 添加窗体


        /// 新添窗体时
        /// <summary>
        /// 新添窗体时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            eDockSide dockSide = dockWindow.CurrentControl.DockStyle.Position;
            if (dockSide == eDockSide.Tab)
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
                    MainTabContro.TabItemClose tabItem = new MainTabContro.TabItemClose();

                    if (dockWindow.ImageControl != null)
                    {
                        tabItem.LogoIcon = dockWindow.ImageControl;
                        //tabItem.LogoIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\images\\DockIcon\\" + dockWindow.Name + ".png", UriKind.Absolute));
                        tabItem.LogoIconWidth = 16;
                        tabItem.LogoIconHeigth = 16;
                    }
                    tabItem.Content = dockWindow;
                    tabItem.Header = dockWindow.Name;
                    //tabItem.Margin = new Thickness(2,0,0,0); 

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


        private void Std_visible_click(object sender, RoutedEventArgs e)
        {
            EquipmentData.Controller.IsVisibleStdMessage = !EquipmentData.Controller.IsVisibleStdMessage;
        }

        private void Log_visible_click(object sender, RoutedEventArgs e)
        {
            EquipmentData.Controller.IsVisibleLogMessage = !EquipmentData.Controller.IsVisibleLogMessage;
        }
        //private SplitPanel GetSplitPanel(eDockSide dockSide)
        //{
        //    SplitPanel splitPanel = null;
        //    string dockString = dockSide.ToString();
        //    for (int i = 0; i < AppDock.SplitPanels.Count; i++)
        //    {
        //        object obj = AppDock.SplitPanels[i].GetValue(DockSite.DockProperty);
        //        if (dockString == obj.ToString())
        //        {
        //            splitPanel = AppDock.SplitPanels[i];
        //            break;
        //        }
        //    }
        //    return splitPanel;
        //}

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
