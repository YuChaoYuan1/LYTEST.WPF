using System.Windows;
using LYTest.WPF.SG.Model;
using System.Windows.Media;
using System.Windows.Controls;

namespace LYTest.WPF.SG
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
                DockWindowDisposable dockWindow = new DockWindowDisposable
                {
                    Name = dockControl.Name,
                    Content = dockControl,
                };
                if (Application.Current.Resources.Contains(dockControl.Name))
                {
                    dockWindow.SetResourceReference(HeaderedContentControl.HeaderProperty, dockControl.Name);
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
