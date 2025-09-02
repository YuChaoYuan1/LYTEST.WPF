using LYTest.ViewModel.Log;
using System;
using System.Windows;
using System.Windows.Input;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_TipsStepFinished.xaml 的交互逻辑
    /// </summary>
    public partial class Window_TipsStepFinished : Window
    {
        private static Window_TipsStepFinished instance = null;

        public static Window_TipsStepFinished Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Window_TipsStepFinished();
                }
                return instance;
            }
        }


        public Window_TipsStepFinished()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            LogViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
            Topmost = true;
        }
        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TipStepFinished")
            {
                if (LogViewModel.Instance.IsShowWinForm)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            Visibility = Visibility.Visible;

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

        private void Button_Off_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            ViewModel.EquipmentData.DeviceManager.PowerOff();
        }
    }
}
