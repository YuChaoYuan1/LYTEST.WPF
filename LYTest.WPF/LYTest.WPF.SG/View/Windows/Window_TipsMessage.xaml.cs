using LYTest.ViewModel.Log;
using System;
using System.Windows;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_TipsMessage.xaml 的交互逻辑
    /// </summary>
    public partial class Window_TipsMessage : Window
    {
        private static Window_TipsMessage instance = null;

        public static Window_TipsMessage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Window_TipsMessage();
                }
                return instance;
            }
        }


        public Window_TipsMessage()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            LogViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
            Topmost = true;
        }
        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TipMessage")
            {
                if (LogViewModel.Instance.IsShowWinForm)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            Visibility = Visibility.Visible;
                            textBlock.Text = LogViewModel.Instance.TipMessage;
                            Show();
                        }
                        catch
                        { }
                    }));
                }
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
