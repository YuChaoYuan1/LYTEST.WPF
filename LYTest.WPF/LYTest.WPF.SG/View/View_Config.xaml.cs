using LYTest.ViewModel.Config;
using LYTest.ViewModel.User;
using LYTest.WPF.SG.Converter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_Config.xaml 的交互逻辑
    /// </summary>
    public partial class View_Config
    {
        public View_Config()
        {
            InitializeComponent();
            Name = "软件配置";
            DockStyle.IsFloating = true;
            //超级用户才可见
            Binding bindingUser = new Binding("USER_POWER")
            {
                Source = UserViewModel.Instance.CurrentUser,
                Converter = Application.Current.Resources["UserVisibilityConverter"] as UserVisibilityConverter,
                ConverterParameter = "2"
            };
            tabitemConfig1.SetBinding(TabItem.VisibilityProperty, bindingUser);
        }
        private ConfigViewModel ViewModel
        {
            get
            {
                return Resources["ConfigViewModel"] as ConfigViewModel;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == "DeleteConfigInfo")
                {
                    if (button.DataContext is ConfigInfo configInfo)
                    {
                        ViewModel.DeleteConfigInfo(configInfo);
                    }
                }
                else if (button.Name == "DeleteGroup")
                {
                    if (button.DataContext is ConfigGroup group)
                    {
                        ViewModel.DeleteGroup(group);
                    }
                }
                else if (button.Name == "SaveGroup")
                {
                    if (button.DataContext is ConfigGroup group)
                    {
                        ViewModel.SaveGroup(group);
                    }
                }
                else
                {
                    ViewModel.CommandFactoryMethod(button.Name);
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ViewModel.Groups.Count; i++)
            {
                ConfigGroup group = ViewModel.Groups[i];
                if (group != null)
                {
                    ViewModel.SaveGroup(group);
                }
            }
        }
        private void AdvTree_ActiveItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (advTree.SelectedItem is ConfigTreeNode node && node.ConfigNo.Length == 5)
            {
                ViewModel.CurrentNode = node;
            }
        }
    }
}
