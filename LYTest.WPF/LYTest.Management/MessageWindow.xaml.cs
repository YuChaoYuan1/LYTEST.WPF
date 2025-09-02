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

namespace LYTest.DataManager
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : Window
    {
        private static MessageWindow instance = null;

        public static MessageWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MessageWindow();
                }
                return instance;
            }
        }

        public MessageWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            ViewModel.MessageDisplay.Instance.PropertyChanged += Instance_PropertyChanged;
            Topmost = true;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FinishedInfo")
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        Visibility = Visibility.Visible;
                        TextMsg.Text = ViewModel.MessageDisplay.Instance.FinishedInfo;
                        Show();
                    }
                    catch
                    { }
                }));

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
