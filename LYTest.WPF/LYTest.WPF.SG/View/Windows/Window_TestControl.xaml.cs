using LYTest.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
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
