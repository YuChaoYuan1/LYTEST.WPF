using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    ///日志查询
    /// </summary>
    public partial class Window_TipsLog : Window
    {
        public Window_TipsLog()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }
        //功能按钮
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image imageTemp)
            {
                switch (imageTemp.Name)
                {
                    case "imageClose":
                        this.Close();
                        break;
                    default:
                        break; ;
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }


        //可以查询所有的日志，可以根据时间查询，可以导出

    }
}
