using LYTest.DataManager.ViewModel.Meters;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LYTest.DataManager
{
    /// <summary>
    /// Interaction logic for WindowSearchItem.xaml
    /// </summary>
    public partial class WindowSearchItem
    {
        public WindowSearchItem()
        {
            InitializeComponent();
            LoadComboBox();
        }

        private void LoadComboBox()
        {
            List<EnumCompare> compareList = new List<EnumCompare>();
            Array arrayTemp = Enum.GetValues(typeof(EnumCompare));
            for(int i=0;i<arrayTemp.Length;i++)
            {
                EnumCompare enumTemp = (EnumCompare)arrayTemp.GetValue(i);
                if(enumTemp!=EnumCompare.清空筛选条件 && enumTemp!=EnumCompare.自定义筛选条件)
                {
                    compareList.Add(enumTemp);
                }
            }
            comboBox.ItemsSource = compareList;
        }

        private void Click_Enter(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Click_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
