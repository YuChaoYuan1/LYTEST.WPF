using LYTest.ViewModel.User;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LYTest.WPF.User
{
    /// <summary>
    /// Page_PassWordChange.xaml 的交互逻辑
    /// </summary>
    public partial class Page_PassWordChange : IDisposable
    {
        private DispatcherTimer timer = new DispatcherTimer();
        public Page_PassWordChange()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 8);
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            textBlockLog.Text = "";
            timer.Stop();
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            string userName = UserViewModel.Instance.CurrentUser.GetProperty("USER_NAME") as string;
            if (string.IsNullOrEmpty(userName))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "当前用户名不能为空!!!";
                return;
            }
            if (UserViewModel.Instance.UpdatePassword(UserViewModel.Instance.CurrentUser.GetProperty("USER_NAME") as string, "12345"))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Black);
                textBlockLog.Text = "重置用户名和密码成功!!!";
            }
            else
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "重置用户名和密码失败!!!";
            }
        }

        private void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            string userName = UserViewModel.Instance.CurrentUser.GetProperty("USER_NAME") as string;
            if (string.IsNullOrEmpty(userName))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "当前用户名不能为空!!!";
                return;
            }
            if (passwordCurrent.Password != UserViewModel.Instance.CurrentUser.GetProperty("USER_PASSWORD") as string)
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "密码输入错误!!!";
                return;
            }
            if (string.IsNullOrEmpty(passwordFirst.Password) || string.IsNullOrEmpty(passwordSecond.Password))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "新密码不能为空!!!";
                return;
            }
            if (passwordFirst.Password != passwordSecond.Password)
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "两次输入的密码不一致!!!";
                return;
            }
            if (UserViewModel.Instance.UpdatePassword(UserViewModel.Instance.CurrentUser.GetProperty("USER_NAME") as string, passwordFirst.Password))
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Black);
                textBlockLog.Text = "修改用户名和密码成功!!!";
            }
            else
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "修改用户名和密码失败!!!";
            }
        }

        public void Dispose()
        {
            timer.Stop();
            buttonResetPassword.Click -= ButtonReset_Click;
            buttonChangePassword.Click -= ButtonChange_Click;
            timer.Tick -= timer_Tick;
        }
    }
}
