using LYTest.ViewModel;
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
using System.Windows.Shapes;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_TestControl.xaml 的交互逻辑
    /// </summary>
    public partial class Window_TestControl : Window
    {
        public Window_TestControl()
        {
            InitializeComponent(); 
            DataContext = EquipmentData.Controller;
            ResizeMode = ResizeMode.NoResize;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = SystemParameters.WorkArea.Width - Width - 80;
            Top = 33;
            Topmost = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
