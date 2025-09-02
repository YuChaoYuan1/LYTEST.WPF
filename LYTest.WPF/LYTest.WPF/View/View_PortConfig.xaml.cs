using LYTest.DAL;
using LYTest.ViewModel.ProtConfig;
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

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_PortConfig.xaml 的交互逻辑
    /// </summary>
    public partial class View_PortConfig
    {
        public View_PortConfig()
        {
            InitializeComponent();
            Name = "端口配置";
         
           // 绑定数据源到目录树所配置的数据源
            //columnQuickPort.ItemsSource = CodeDictionary.GetLayer2("QuickPort").Keys;
        }
        private PortConfigViewModel ViewModel
        {
            get
            {
                return Resources["PortConfigViewModel"] as PortConfigViewModel;
            }
        }

        #region 表485


        /// <summary>
        /// 添加表端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Add_Meter(object sender, RoutedEventArgs e)
        {
            MeterItems meter = new MeterItems
            {
                Server = ViewModel.Servers[0],
                FlagChanged = true
            };
            ViewModel.MeterItem.Add(meter);
        }

        /// <summary>
        /// 删除表端口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Del_Meter(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                MeterItems meter = btn.DataContext as MeterItems;
                ViewModel.MeterItem.Remove(meter);
            }
        }
        #endregion


        #region 设备

        // 设备删除
        private void Btn_Del_Device(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn)
            {
                if (btn.DataContext is DeviceItem dev)
                {
                    for (int i = 0; i < ViewModel.Groups.Count; i++)
                    {
                        if (ViewModel.Groups[i].DeviceItems.Contains(dev))
                        {
                            ViewModel.Groups[i].DeviceItems.Remove(dev);
                            break;
                        }
                    }
                }
            }
        }

        // 设备添加
        private void Btn_Add_Device(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.DataContext is DeviceGroup goup)
                {
                    goup.DeviceItems.Add(new DeviceItem()
                    {
                        Server = ViewModel.Servers[0],
                        FlagChanged = false
                    });
                }
            }
        }
        #endregion

    }
}
