using LYTest.DAL.DataBaseView;
using LYTest.MeterProtocol.DataFlag;
using LYTest.ViewModel;
using LYTest.ViewModel.Schema;
using LYTest.ViewModel.Time;
using LYTest.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        bool IsLogin = true;
        public Window2()
        {
            InitializeComponent();
            if (EquipmentData.Beta)
            {
                SoftModel.Text = "LY8001 内测版";
            }
//#if LOGO_MOEN
//            StyBorder.Margin = new Thickness(0, 20, 0, 0);
//            StyLogoPre.Margin = new Thickness(50, 0, 0, 0);
//            StyLogoBg.Points = new PointCollection(new Point[] { new Point(80, 0), new Point(240, 0), new Point(240, 100), new Point(80, 100) });
//            StyLogoFg.Points = new PointCollection(new Point[] { new Point(85, 5), new Point(235, 5), new Point(235, 95), new Point(85, 95) });
//            Logo.Stretch = Stretch.Fill;
//            Logo.ImageSource = ((ImageBrush)Application.Current.Resources["LoginLogo_moen"]).ImageSource;
//            BenchName.Text = "摩恩智能电能检定装置";
//#else
            this.Icon = new BitmapImage(new Uri($"./images/{AppInfo.Icon}", UriKind.Relative));
            StyBorder.Margin = new Thickness(0, 20, 0, 0);
            StyLogoPre.Margin = new Thickness(100, 0, 0, 0);
            StyLogoBg.Points = new PointCollection(new Point[] { new Point(130, 0), new Point(240, 0), new Point(240, 100), new Point(130, 100) });
            StyLogoFg.Points = new PointCollection(new Point[] { new Point(135, 0), new Point(235, 0), new Point(235, 100), new Point(135, 100) });
            //Logo.ImageSource = ((ImageBrush)Application.Current.Resources["LoginLogo"]).ImageSource;
            //ImageBrush brush = new ImageBrush();

            Logo.ImageSource = new BitmapImage(new Uri($"./images/{AppInfo.LogoImage}", UriKind.Relative));
            Logo.Stretch = Stretch.Fill;

            BenchName.Text = AppInfo.Title;
            SoftModel.Text =AppInfo.SoftModel;

//#endif
            LoadUsers(); //载入用户
            DataContext = EquipmentData.LastCheckInfo;
            Chk_IsDeom.DataContext = EquipmentData.Equipment;

            UiInterface.UiDispatcher = SynchronizationContext.Current;

            timeBar.DataContext = EquipmentData.Equipment;
            timeBar.SetBinding(ProgressBar.ValueProperty, new Binding("ProgressBarValue"));

            //timeBar.SetBinding(ProgressBar.ValueProperty, new Binding("PastTime") { Mode = BindingMode.OneWay });
            //timeBar.SetBinding(ProgressBar.MaximumProperty, new Binding("TotalTime"));

            textBlockLogin.DataContext = EquipmentData.Equipment;
            textBlockLogin.SetBinding(TextBlock.TextProperty, new Binding("TextLogin"));



            string str = Core.OperateFile.GetINI("Config", "IsDeom", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini").Trim();
            if (str.ToLower() == "true")
            {
                EquipmentData.Equipment.IsDemo = true;
            }
            Loaded += Window2_Loaded;

        }

        private void Window2_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Task.Delay(1000).Wait();
                if (EquipmentData.Equipment.AutoLogion)  //自动登入
                {
                    Dispatcher.Invoke(() => Timer_Tick(null, null));
                    //timer.Interval = new TimeSpan(0, 0, 3);
                    //timer.Tick += timer_Tick;
                    //timer.IsEnabled = true;
                    //timer.Start();
                }
            });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsLogin)
            {
                return;
            }

            if (CheckLogin())
            {
                gridContent.Visibility = Visibility.Collapsed;
                gridLogIn.Visibility = Visibility.Visible;
                TimeMonitor.Instance.LogIn();
                InitializeCheckInfo();
            }
        }

        /// <summary>
        /// 加载用户
        /// </summary>
        private void LoadUsers()
        {
            List<string> userNames = UserViewModel.Instance.GetList("");    //获得所有用户
            cmb_Checker.ItemsSource = userNames;
            cmb_Checker.SelectedItem = EquipmentData.LastCheckInfo.TestPerson;
            cmb_Auditor.ItemsSource = userNames;
            cmb_Auditor.SelectedItem = EquipmentData.LastCheckInfo.AuditPerson;
        }

        //拖动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsLogin = false;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        //按钮，登入和取消
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;

            IsLogin = false;
            switch (btn.Name)
            {
                case "buttonLogin":   //登入
                    if (EquipmentData.Equipment.IsDemo)
                    {
                        MessageBoxResult boxResult = MessageBox.Show(this, "确定进入演示模式？", "演示模式", MessageBoxButton.OKCancel);
                        if (boxResult != MessageBoxResult.OK)
                        {
                            return;
                        }
                    }
                    if (CheckLogin())
                    {
                        gridContent.Visibility = Visibility.Collapsed;
                        gridLogIn.Visibility = Visibility.Visible;
                        TimeMonitor.Instance.LogIn();
                        InitializeCheckInfo();

                    }
                    break;
                case "buttonQuit":
                    Close();
                    break;
            }
        }

        /// <summary>
        /// 登入判断
        /// </summary>
        /// <returns></returns>
        private bool CheckLogin()
        {
            bool flag = true;

            string checker = cmb_Checker.Text;
            string password1 = passwordBoxChecker.Password;
            string auditor = cmb_Auditor.Text;
            string password2 = passwordBoxAuditor.Password;
            if (!UserViewModel.Instance.Login(auditor, password2))
            {
                passwordBoxAuditor.BorderBrush = new SolidColorBrush(Colors.Red);
                flag = false;
            }
            else
            {
                passwordBoxAuditor.BorderBrush = SystemColors.ControlDarkDarkBrush;
            }
            if (!UserViewModel.Instance.Login(checker, password1))
            {
                passwordBoxChecker.BorderBrush = new SolidColorBrush(Colors.Red);
                flag = false;
            }
            else
            {
                passwordBoxChecker.BorderBrush = SystemColors.ControlDarkDarkBrush;
            }
            if (flag)
            {
                EquipmentData.LastCheckInfo.AuditPerson = auditor;
                EquipmentData.LastCheckInfo.ProtectedCurrent = comboBoxCurrent.SelectedItem as string;
                EquipmentData.LastCheckInfo.ProtectedVoltage = comboBoxVoltage.SelectedItem as string;
            }
            return flag;
        }

        private void InitializeCheckInfo()
        {
            new Thread(() =>
            {
                EquipmentData.Equipment.TextLogin = "开始加载配置信息...";
                EquipmentData.Equipment.ProgressBarValue = 15;
                EquipmentData.Equipment.TextLogin = "开始加载方案信息...";
                EquipmentData.Equipment.ProgressBarValue = 30; //进度条进度
                DataFlagS.LoadDataFlag();
                EquipmentData.DISetsInfo.DiRefresh();
                FullTree.Instance.Initialize();
                EquipmentData.Equipment.TextLogin = "正在加载结论视图...";
                EquipmentData.Equipment.ProgressBarValue = 60;
                ResultViewHelper.Initialize(); //初始化所有结论试图
                EquipmentData.Equipment.TextLogin = "开始初始化检定信息...";
                EquipmentData.Equipment.ProgressBarValue = 90;
                EquipmentData.Initialize();
                EquipmentData.Equipment.ProgressBarValue = 100;


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow mainWindow = new MainWindow();
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    Close();
                }));
            }).Start();
        }

        private void IsDeom_Click(object sender, RoutedEventArgs e)
        {
            //EquipmentData.Equipment.IsDemo = (bool)Chk_IsDeom.IsChecked;
            Core.OperateFile.WriteINI("Config", "IsDeom", Chk_IsDeom.IsChecked.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
        }

        private void IsDeom_Click2(object sender, RoutedEventArgs e)
        {
            //Chk_IsDeom.DataContext = EquipmentData.Equipment;
            //EquipmentData.Equipment.IsDemo = (bool)Chk_IsDeom.IsChecked;
            Core.OperateFile.WriteINI("Config", "IsDeom", Chk_IsDeom.IsChecked.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
        }
    }
}
