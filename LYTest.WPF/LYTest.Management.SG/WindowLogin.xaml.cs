using LYTest.DAL.Config;
using LYTest.DAL.DataBaseView;
using LYTest.DataManager.SG.ViewModel;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LYTest.DataManager.SG
{
    /// <summary>
    /// Interaction logic for WindowLogin.xaml
    /// </summary>
    public partial class WindowLogin
    {
        public WindowLogin()
        {
            InitializeComponent();
            UiInterface.UiDispatcher = SynchronizationContext.Current;
            if (loginModel==null)
            {
                loginModel = new LoginModel();
            }
            timeBar.DataContext = loginModel;
            timeBar.SetBinding(ProgressBar.ValueProperty, new Binding("ProgressBarValue"));
            textBlockLogin.DataContext = loginModel;
            textBlockLogin.SetBinding(TextBlock.TextProperty, new Binding("TextLogin"));
            loginModel.IsClose = false;
            Login();
        }
        LoginModel loginModel;

        private void Login()
        {
            MessageDisplay.Instance.Initialize();
            loginModel.TextLogin = "正在加载数据...";
            new Thread(() =>
            {
                loginModel.ProgressBarValue = 30;
                loginModel.TextLogin = "正在加载配置数据...";
                CodeTreeViewModel.Instance.InitializeTree();
                if (SetClose(10)) return;
                loginModel.TextLogin = "正在初始化表信息...";
                loginModel.ProgressBarValue = 40;
                FullTree.Instance.Initialize();
                if (SetClose(20)) return;
                ResultViewHelper.Initialize();
                if (SetClose(30)) return;
                loginModel.TextLogin = "正在加载台体信息...";
                loginModel.ProgressBarValue = 70;
                Equipments.Instance.Initialize();
                ConfigHelper.Instance.LoadAllConfig(); //add yjt 20220221 新增初始化台体设置信息如营销接口
                if (SetClose(50)) return;
                loginModel.TextLogin = "正在初始化界面...";
                loginModel.ProgressBarValue = 100;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow mainWindow = new MainWindow();
                    Application.Current.MainWindow = mainWindow;
                    if (SetClose(70)) return;
                    mainWindow.Show();
                    if (SetClose(80)) return;
                    Close();
                }));
            }).Start();
        }

        private bool SetClose(int value)
        {
            if (!loginModel.IsClose) return false;
            loginModel.ProgressBarValue = value;
            return true;
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            loginModel.IsClose = true ;
            loginModel.TextLogin = "正在退出请稍后...";
            Close();
        }
    }
}
