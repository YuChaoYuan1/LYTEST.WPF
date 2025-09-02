using LYTest.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LYTest.WPF.SG.User
{
    /// <summary>
    /// Page_DelUser.xaml 的交互逻辑
    /// </summary>
    public partial class Page_DelUser : IDisposable
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public Page_DelUser()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 8);
            timer.Tick += Timer_Tick;
            listBoxUsers.ItemsSource = users;
            LoadUserList();
        }


        private readonly ObservableCollection<ModelTemp> users = new ObservableCollection<ModelTemp>();
        private void LoadUserList()
        {
            users.Clear();
            List<string> list = UserViewModel.Instance.GetList("");
            for (int i = 0; i < list.Count; i++)
            {
                users.Add(new ModelTemp { UserName = list[i], IsSelected = false });
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
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
                    textBlockLog.Text = $"用户:{string.Join(",", deleteNames)} 删除成功,用户:{string.Join(",", failedNames)} 删除失败";
                }
                else
                {
                    textBlockLog.Text = $"用户:{string.Join(",", failedNames)} 删除失败";
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


        void Timer_Tick(object sender, EventArgs e)
        {
            textBlockLog.Text = "";
            timer.Stop();
        }
        public void Dispose()
        {
            timer.Stop();
            timer.Tick -= Timer_Tick;
            buttonDelete.Click -= ButtonDelete_Click;
        }

        public class ModelTemp : DependencyObject
        {
            public string UserName
            {
                get { return (string)GetValue(UserNameProperty); }
                set { SetValue(UserNameProperty, value); }
            }

            public static readonly DependencyProperty UserNameProperty =
                DependencyProperty.Register("UserName", typeof(string), typeof(ModelTemp), new PropertyMetadata(""));

            public bool IsSelected
            {
                get { return (bool)GetValue(IsSelectedProperty); }
                set { SetValue(IsSelectedProperty, value); }
            }

            public static readonly DependencyProperty IsSelectedProperty =
                DependencyProperty.Register("IsSelected", typeof(bool), typeof(ModelTemp), new PropertyMetadata(false));


        }
    }
}
