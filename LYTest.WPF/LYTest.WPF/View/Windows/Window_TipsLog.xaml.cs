using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Image imageTemp = e.OriginalSource as Image;
            if (imageTemp != null)
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
