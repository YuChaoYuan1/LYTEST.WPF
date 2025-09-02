using LYTest.ViewModel;
using LYTest.ViewModel.PowerControl;
using System.Windows;

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_PowerControl.xaml 的交互逻辑
    /// </summary>
    public partial class View_PowerControl
    {
        public View_PowerControl()
        {
            InitializeComponent();
            DockStyle.FloatingSize = new Size(850, 550);
            Name = "功率源控制";
            DockStyle.IsFloating = true;
            StdData.DataContext = EquipmentData.StdInfo;
            EquipmentData.MeterGroupInfo.FirstMeter.PropertyChanged -= FirstMeter_PropertyChanged;
            EquipmentData.MeterGroupInfo.FirstMeter.PropertyChanged += FirstMeter_PropertyChanged;
        }


        private PowerControlModel ViewModel
        {
            get { return Resources["PowerControlViewModel"] as PowerControlModel; }
        }
        //private void MainWindow_Closed(object sender, System.ComponentModel.CancelEventArgs e)
        //{ 
        //    viewModel.SaveValue();

        //}
        private void FirstMeter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MD_UB")
            {
                if (double.TryParse(EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_UB")?.ToString(), out double ub))
                {
                    ViewModel.Ua = ub;
                    ViewModel.Ub = ub;
                    ViewModel.Uc = ub;
                }
                else
                {
                    ViewModel.Ua = 0;
                    ViewModel.Ub = 0;
                    ViewModel.Uc = 0;
                }
                ViewModel.Ia = 0;
                ViewModel.Ib = 0;
                ViewModel.Ic = 0;
            }
        }

        private void ALLU_Click(object sender, RoutedEventArgs e)
        {
            if (ALLU.IsChecked == true)
            {
                ALLUa.IsChecked = true;
                ALLUb.IsChecked = true;
                ALLUc.IsChecked = true;
            }
            else
            {
                ALLUa.IsChecked = false;
                ALLUb.IsChecked = false;
                ALLUc.IsChecked = false;
            }
        }

        private void ALLI_Click(object sender, RoutedEventArgs e)
        {
            if (ALLI.IsChecked == true)
            {
                ALLIa.IsChecked = true;
                ALLIb.IsChecked = true;
                ALLIc.IsChecked = true;
            }
            else
            {
                ALLIa.IsChecked = false;
                ALLIb.IsChecked = false;
                ALLIc.IsChecked = false;
            }

        }

    }
}
