using System.Windows;
using LYTest.WPF.Model;
using System.Windows.Media;
using System.Windows.Controls;

namespace LYTest.WPF
{
    public static class UIGeneralClass
    {
        /// <summary>
        /// 新建一个停靠窗体装载要显示的控件
        /// </summary>
        /// <returns></returns>
        public static DockWindowDisposable CreateDockWindow(DockControlDisposable dockControl)
        {
            if (dockControl != null)
            {
                dockControl.Foreground = Application.Current.Resources["字体颜色标准"] as Brush;
                Point location = new Point((SystemParameters.WorkArea.Width - dockControl.DockStyle.FloatingSize.Width) / 2, (SystemParameters.WorkArea.Height - dockControl.DockStyle.FloatingSize.Height) / 2);
                DockWindowDisposable dockWindow = new DockWindowDisposable
                {
                    Name = dockControl.Name,
                    Content = dockControl,
                    //CanClose = dockControl.DockStyle.CanClose,
                    //CanDockAsDocument = dockControl.DockStyle.CanDockAsDocument,
                    //CanDockBottom = dockControl.DockStyle.CanDockBottom,
                    //CanDockLeft = dockControl.DockStyle.CanDockLeft,
                    //CanDockRight = dockControl.DockStyle.CanDockRight,
                    //CanDockTop = dockControl.DockStyle.CanDockTop,
                    //CanFloat = dockControl.DockStyle.CanFloat,
                    //CloseButtonVisibility = dockControl.DockStyle.CloseButtonVisibility,
                    //FloatingRect = new Rect(location, dockControl.DockStyle.FloatingSize),
                };
                if (Application.Current.Resources.Contains(dockControl.Name))
                {
                    dockWindow.SetResourceReference(HeaderedContentControl.HeaderProperty, dockControl.Name);
                }
                else
                {
                    //dockWindow.Header = dockControl.Name;
                }

                return dockWindow;
            }
            else
            {
                return null;
            }
        }
    }
}
