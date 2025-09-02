using LYTest.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LYTest.WPF.User
{
    /// <summary>
    /// Page_DelUser.xaml 的交互逻辑
    /// </summary>
    public partial class Page_DelUser :IDisposable
    {
        private DispatcherTimer timer = new DispatcherTimer();

        public Page_DelUser()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 8);
            timer.Tick += timer_Tick;
            listBoxUsers.ItemsSource = users;
            LoadUserList();
        }


        private ObservableCollection<ModelTemp> users = new ObservableCollection<ModelTemp>();
        private void LoadUserList()
        {
            users.Clear();
            List<string> userList = new List<string>();
            if (UserViewModel.Instance.CurrentUser.GetProperty("USER_POWER") as string == "2")   //超级管理员，获得所有用户
            {
                userList = UserViewModel.Instance.GetList("");
                //因为是删除，应该排除自己
            }
            else
            {
                userList = UserViewModel.Instance.GetNormalUsers();
            }
            for (int i = 0; i < userList.Count; i++)
            {
                users.Add(new ModelTemp { UserName = userList[i], IsSelected = false });
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            List<string> deleteNames = new List<string>();
            List<string> failedNames = new List<string>();
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].IsSelected)
                {
                    if (UserViewModel.Instance.DeleteUser(users[i].UserName))
                    {
                        deleteNames.Add(users[i].UserName);
                    }
                    else
                    {
                        failedNames.Add(users[i].UserName);
                    }
                }
            }
            if (failedNames.Count > 0)
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                if (deleteNames.Count > 0)
                {
                    textBlockLog.Text = string.Format("用户:{0} 删除成功,用户:{1} 删除失败", string.Join(",", deleteNames), string.Join(",", failedNames));
                }
                else
                {
                    textBlockLog.Text = string.Format("用户:{1} 删除失败", string.Join(",", failedNames));
                }
            }
            else if (deleteNames.Count > 0)
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Black);
                textBlockLog.Text = string.Format("用户:{0} 删除成功", string.Join(",", deleteNames));
            }
            else
            {
                textBlockLog.Foreground = new SolidColorBrush(Colors.Red);
                textBlockLog.Text = "请先选择要删除的用户!!";
            }
            LoadUserList();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            textBlockLog.Text = "";
            timer.Stop();
        }
        public void Dispose()
        {
            timer.Stop();
            timer.Tick -= timer_Tick;
            buttonDelete.Click -= buttonDelete_Click;
        }

        public class ModelTemp : DependencyObject
        {


            public string UserName
            {
                get { return (string)GetValue(UserNameProperty); }
                set { SetValue(UserNameProperty, value); }
            }

            // Using a DependencyProperty as the backing store for UserName.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty UserNameProperty =
                DependencyProperty.Register("UserName", typeof(string), typeof(ModelTemp), new PropertyMetadata(""));



            public bool IsSelected
            {
                get { return (bool)GetValue(IsSelectedProperty); }
                set { SetValue(IsSelectedProperty, value); }
            }

            // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty IsSelectedProperty =
                DependencyProperty.Register("IsSelected", typeof(bool), typeof(ModelTemp), new PropertyMetadata(false));


        }
    }
}
