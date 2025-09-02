using LYTest.ViewModel.User;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LYTest.WPF.User
{
    /// <summary>
    /// Page_LoginUser.xaml 的交互逻辑
    /// </summary>
    public partial class Page_LoginUser : IDisposable
    {
        private DispatcherTimer timer = new DispatcherTimer();
        public Page_LoginUser()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 8);
            timer.Tick += timer_Tick;
            GetUserName();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            textBlockLog.Text = "";
            timer.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            string userName = comboBoxUserName.Text;
            string password = passwordBox.Password;
            if (UserViewModel.Instance.Login(userName, password))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Black);
                textBlockLog.Text = "登录成功";
            }
            else
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "登录失败,请确认用户名和密码!!";
            }
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Tick -= timer_Tick;
            buttonLogIn.Click -= Button_Click;
            comboBoxUserName.KeyUp -= comboBoxUserName_KeyUp;
        }

        void comboBoxUserName_KeyUp(object sender, KeyEventArgs e)
        {
            string inputText = comboBoxUserName.Text;
            comboBoxUserName.ItemsSource = UserViewModel.Instance.GetList(inputText);
        }

        void GetUserName()
        {
            comboBoxUserName.ItemsSource = UserViewModel.Instance.GetList("");
        }
    }
}
