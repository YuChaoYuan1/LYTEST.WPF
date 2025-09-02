using LYTest.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.User
{
    /// <summary>
    /// Page_MainUser.xaml 的交互逻辑
    /// </summary>
    public partial class Page_MainUser : IDisposable
    {
        public Page_MainUser()
        {
            InitializeComponent();
            DataContext = UserViewModel.Instance;
        }

        private void StackPanel_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            if (button == null)
            {
                return;
            }
            switch (button.Name)
            {
                case "buttonChangeUser":
                    UserViewModel.Instance.CurrentUser = null;
                    UserViewModel.Instance.Step = 0;
                    break;
                case "buttonChangePassword":
                    UserViewModel.Instance.Step = 2;
                    break;
                case "buttonAddUser":
                    UserViewModel.Instance.Step = 4;
                    break;
                case "buttonDeleteUser":
                    UserViewModel.Instance.Step = 3;
                    break;
            }
        }

        public void Dispose()
        {
            buttonChangeUser.Click -= StackPanel_Click;
            buttonChangePassword.Click -= StackPanel_Click;
            buttonAddUser.Click -= StackPanel_Click;
            buttonDeleteUser.Click -= StackPanel_Click;
        }
    }
}
