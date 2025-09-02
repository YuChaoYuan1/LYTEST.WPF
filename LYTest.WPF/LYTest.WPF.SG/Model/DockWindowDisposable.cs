using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LYTest.WPF.SG.Model
{
    public class DockWindowDisposable : UserControl
    {
        public int Index { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public BitmapImage ImageControl { get; set; }



        public bool IsSelected { get; set; }

        /// <summary>
        /// 在创建窗体时对此控件赋值
        /// </summary>
        public DockControlDisposable CurrentControl
        {
            get { return Content as DockControlDisposable; }
        }
        /// <summary>
        /// 关闭窗体时调用控件注销方法
        /// </summary>
        public void OnClosed()
        {
            CurrentControl?.Dispose();
            Content = null;
            MainViewModel.Instance.WindowsAll.Remove(this);

            for (int i = this.Index + 1; i < MainViewModel.Instance.WindowsAll.Count; i++)
            {
                if (i >= 0)
                    MainViewModel.Instance.WindowsAll[i].Index--;
            }

            GC.Collect();
            GC.SuppressFinalize(this);
        }


        public void OnClosed2()
        {
            CurrentControl?.Dispose();
            Content = null;
            MainViewModel.Instance.WindowsAll.Remove(this);
            Window window = Window.GetWindow(this);
            window?.Close();
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
