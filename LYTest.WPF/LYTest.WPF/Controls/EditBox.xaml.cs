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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.Controls
{
    /// <summary>
    /// EditBox.xaml 的交互逻辑
    /// </summary>
    public partial class EditBox : UserControl
    {
        public EditBox()
        {
            InitializeComponent();
            gridRoot.DataContext = this;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditBox), new PropertyMetadata(""));

        private void textBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            textBlock.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBlock.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Collapsed;
        }
    }
}
