using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_Show.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Show : Window
    {
        public Window_Show()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            MouseLeftButtonDown += UserMenu_MouseLeftButtonDown;
        }
        //双击改变大小
        private void UserMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowStateSet(this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
              }
        }
        public void LoaiUserWindow(Model.DockWindowDisposable control)
        {

            if (control.ImageControl != null)
            {
                HeadIcon.Source = control.ImageControl;
            }

            if (control.CurrentControl.DockStyle.IsMaximized)
            {
                WindowStateSet(WindowState.Maximized);
            }
            else
            {
                this.Width = control.CurrentControl.DockStyle.FloatingSize.Width;
                this.Height = control.CurrentControl.DockStyle.FloatingSize.Height;
            }

            txt.Text = control.Name;
            grid.Children.Add(control);
        }
        protected override void OnClosed(System.EventArgs e)
        {
            ((Model.DockWindowDisposable)grid.Children[0]).OnClosed2();
            base.OnClosed(e);
        }
        /// 拖动
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image imageTemp)
            {
                switch (imageTemp.Name)
                {
                    case "imageMax":
                        WindowStateSet(this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
                        break;
                    case "imageClose":
                        this.Close();
                        break;
                }
            }

        }


        private void Click_Max(object sender, RoutedEventArgs e)
        {
            WindowStateSet(this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        }

        private void WindowStateSet(WindowState state)
        {
            this.WindowState = state;
            if (state == WindowState.Normal)
            {
                imageMax.Source = new BitmapImage(new Uri(@"../../Images/最大化窗口.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                imageMax.Source = new BitmapImage(new Uri(@"../../Images/正常窗口.png", UriKind.RelativeOrAbsolute));
            }
        }


        private void Click_close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
