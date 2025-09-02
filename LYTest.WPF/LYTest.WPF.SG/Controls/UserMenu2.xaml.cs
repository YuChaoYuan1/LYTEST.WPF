using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.Menu;
using LYTest.WPF.SG.Skin;
using LYTest.WPF.SG.UiGeneral;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Shapes = System.Windows.Shapes;

namespace LYTest.WPF.SG.Controls
{
    /// <summary>
    /// UserMenu.xaml 的交互逻辑
    /// </summary>
    public partial class UserMenu2
    {
        public UserMenu2()
        {
            InitializeComponent();
            LoadMenu();
            MouseLeftButtonDown += UserMenu_MouseLeftButtonDown;


            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string name = "深圳市隆元科技有限公司";
            TestName.Text = "隆元科技-LY8001";
            //MainLogo.Source = ((ImageBrush)Application.Current.Resources["MainLogo"]).ImageSource;
            //MainLogo.Source= new BitmapImage(new Uri(".\\Images\\隆元.png", UriKind.Relative));
            if (ConfigHelper.Instance.IsVersionNumber)
            {
                name += $"({version})";
            }
            if (ConfigHelper.Instance.IsDeviceNumber)
            {
                name += $" - {EquipmentData.Equipment.ID}号";
            }
            TestName.ToolTip = name;
            Version.Text = $"V{version}({EquipmentData.Equipment.ID})";
            if (EquipmentData.Beta)
            {
                Version.Text += "内测版";
            }
            EquipmentData.Equipment.Version = version.ToString();

            if (!ConfigHelper.Instance.IsTestButtonSuspension)
            {
                VerifyGrid.DataContext = EquipmentData.Controller;
                VerifyGrid.Visibility = Visibility.Visible;
            }
            else
            {
                VerifyGrid.Visibility = Visibility.Collapsed;
            }
            //ResizeMode = ResizeMode.NoResize;
        }


        private void LoadMenu()
        {
            MenuViewModel menuViewModel = new MenuViewModel();
            for (int i = 0; i < menuViewModel.Menus.Count; i++)
            {
                Button button = ControlFactory.CreateButton(menuViewModel.Menus[i], true);
                if (button != null)
                {
                    stackPannelMenu.Children.Add(button);
                }
            }
        }


        #region 基础功能

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Shapes.Path path)
            {
                if (path.DataContext is ThemeItem theme)
                {
                    theme.Load();
                    Properties.Settings.Default.Save();
                }
            }
        }

        //双击改变大小
        private void UserMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Application.Current.MainWindow == null) return;
                if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                    imageMax.Source = new BitmapImage(new Uri(@"../Images/最大化窗口.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    imageMax.Source = new BitmapImage(new Uri(@"../Images/正常窗口.png", UriKind.RelativeOrAbsolute));
                }
            }
        }


        //功能按钮
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image image)
            {
                switch (image.Name)
                {
                    case "imageMin":
                        Application.Current.MainWindow.WindowState = WindowState.Minimized;
                        break;
                    case "imageMax":
                        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                            imageMax.Source = new BitmapImage(new Uri(@"../Images/最大化窗口.png", UriKind.RelativeOrAbsolute));
                        }
                        else
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Maximized;
                            imageMax.Source = new BitmapImage(new Uri(@"../Images/正常窗口.png", UriKind.RelativeOrAbsolute));
                        }
                        break;
                    case "imageClose":
                        //ViewModel.EquipmentData.ApplicationIsOver = true;
                        try
                        {
                            Application.Current.MainWindow.Close();
                        }
                        catch
                        {
                            Application.Current.Shutdown();
                        }
                        break;
                    case "ThemeChoice":  //切换主题
                                         //Window_ThemeChoice window_Theme =  Window_ThemeChoice.Instance;
                                         //window_Theme.Show();
                                         //Window_ColorSet.Instance.Show();
                                         //Window_ColorSet colorSet = new Window_ColorSet();
                                         //  colorSet.Show();
                                         //window_Theme.
                        break;
                }
            }

        }


        /// 拖动
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                Application.Current.MainWindow.DragMove();
            }
        }
        #endregion

    }
}
