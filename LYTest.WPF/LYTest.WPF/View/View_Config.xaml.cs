using LYTest.ViewModel.Config;
using LYTest.ViewModel.User;
using LYTest.WPF.Converter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LYTest.WPF.View
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
            Binding bindingUser = new Binding("USER_POWER");
            bindingUser.Source = UserViewModel.Instance.CurrentUser;
            bindingUser.Converter = Application.Current.Resources["UserVisibilityConverter"] as UserVisibilityConverter;
            bindingUser.ConverterParameter = "2";
            tabitemConfig1.SetBinding(TabItem.VisibilityProperty, bindingUser);
        }
        private ConfigViewModel viewModel
        {
            get
            {
                return Resources["ConfigViewModel"] as ConfigViewModel;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button.Name == "DeleteConfigInfo")
                {
                    ConfigInfo configInfo = button.DataContext as ConfigInfo;
                    if (configInfo != null)
                    {
                        viewModel.DeleteConfigInfo(configInfo);
                    }
                }
                else if (button.Name == "DeleteGroup")
                {
                    ConfigGroup group = button.DataContext as ConfigGroup;
                    if (group != null)
                    {
                        viewModel.DeleteGroup(group);
                    }
                }
                else if (button.Name == "SaveGroup")
                {
                    ConfigGroup group = button.DataContext as ConfigGroup;
                    if (group != null)
                    {
                        viewModel.SaveGroup(group);
                    }
                }
                else
                {
                    viewModel.CommandFactoryMethod(button.Name);
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < viewModel.Groups.Count; i++)
            {
                ConfigGroup group = viewModel.Groups[i];
                if (group != null)
                {
                    viewModel.SaveGroup(group);
                }
            }
            //viewModel.Groups[0].Units;
            //ConfigGroup group = viewModel.CurrentNode[0];
            //Button button = sender as Button;
            //ConfigGroup group = button.DataContext as ConfigGroup;
            //if (group != null)
            //{
            //    viewModel.SaveGroup(group);
            //}
        }
        private void AdvTree_ActiveItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ConfigTreeNode node = advTree.SelectedItem as ConfigTreeNode;
            if (node != null && node.ConfigNo.Length == 5)
            {
                viewModel.CurrentNode = node;
            }
        }
    }
}
