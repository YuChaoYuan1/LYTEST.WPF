using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_Wait.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Wait : Window
    {

        private static Window_Wait instance = null;

        public static Window_Wait Instance
        {
            get
            {
                if (instance == null)
                {
                    // 在UI线程上调用实例化代码
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        instance = new Window_Wait();
                    });
                }
                return instance;
            }
        }
        public Window_Wait()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            Topmost = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private CancellationTokenSource _cancellationTokenSource;
        /// <summary>
        /// 开启等待
        /// </summary>
        /// <param name="Tips">提示文字</param>
        /// <param name="MaxTime">超时时间(毫秒),0无，大于0的话超出这个时间会自动关闭</param>
        public void StateWait(string Tips = "", int MaxTime = 0)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    Visibility = Visibility.Visible;
                    textBlock.Text = Tips;
                    Show();
                    if (MaxTime > 0)
                    {
                        _cancellationTokenSource = new CancellationTokenSource();
                        _cancellationTokenSource.CancelAfter(MaxTime); // 设置超时时间
                        _cancellationTokenSource.Token.Register(() =>
                        {
                            EndWait();
                        });
                    }
                }
                catch
                { }
            }));
        }
        /// <summary>
        /// 结束等待
        /// </summary>
        public void EndWait()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    Visibility = Visibility.Collapsed;
                }
                catch
                { }
            }));

        }
    }
}
