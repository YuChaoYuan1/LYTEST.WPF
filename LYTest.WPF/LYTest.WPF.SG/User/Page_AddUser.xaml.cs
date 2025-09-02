using LYTest.ViewModel.User;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LYTest.WPF.SG.User
{
    /// <summary>
    /// Page_AddUser.xaml 的交互逻辑
    /// </summary>
    public partial class Page_AddUser : IDisposable
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public Page_AddUser()
        {
            InitializeComponent();
            DataContext = UserViewModel.Instance;
            timer.Interval = new TimeSpan(0, 0, 8);
            timer.Tick += Timer_Tick;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            textBlockLog.Text = "";
            timer.Stop();
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Tick -= Timer_Tick;
        }

        private void OnUserAdd(object sender, RoutedEventArgs e)
        {
            timer.Start();
            string userName = textBoxUserName.Text;
            string userCode = textBoxUserCode.Text;
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userCode))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "用户名和用户编号不能为空!!!";
                return;
            }
            if (UserViewModel.Instance.IsExist(userName))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "当前用户名已存在,请重新输入用户名!!!";
                return;
            }
            else if (UserViewModel.Instance.IsExist(userCode))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "当前用户编号已存在,请重新输入用户编号!!!";
                return;
            }
            if (string.IsNullOrEmpty(passwordBox1.Password) || string.IsNullOrEmpty(passwordBox2.Password))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "密码不能为空!!!";
                return;
            }
            if (passwordBox2.Password != passwordBox2.Password)
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "两次输入的密码不一致!!!";
                return;
            }
            string permission = "0";
            if (UserViewModel.Instance.CurrentUser.GetProperty("USER_POWER") as string == "2")
            {
                if (comboBoxPermission.SelectedIndex == 1)
                {
                    permission = "1";
                }
            }
            if (UserViewModel.Instance.AddUser(userName, userCode, passwordBox1.Password, permission))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Black);
                textBlockLog.Text = "添加用户成功!!!";
            }
            else
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "添加用户失败!!!";
            }
        }
    }
}
