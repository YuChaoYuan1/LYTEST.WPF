using LYTest.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_ShemaSet.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ShemaSet : Window
    {
        public Window_ShemaSet()
        {
            InitializeComponent();
            DataContext = EquipmentData.SchemaModels;//方案列表

            //dataGrid_ShcemaInfo.ItemsSource = EquipmentData.SchemaModels.Schemas;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

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
    }
}
