using LYTest.DAL.DataBaseView;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.User;
using LYTest.WPF.SG.Converter;
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

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// Form.xaml 的交互逻辑
    /// </summary>
    public partial class View_MeterInfoSimple
    {
        public View_MeterInfoSimple()
        {
            InitializeComponent();
            DataContext = EquipmentData.MeterGroupInfo;

            LoadMeterInfo();
        }
        private void LoadMeterInfo()
        {
            //800是参数录入对应的列
            string[] displayKey = new string[] {
                "MD_WIRING_MODE", "MD_UB", "MD_UA", "MD_FREQUENCY", "MD_CONSTANT",
                "MD_GRADE","MD_PROTOCOL_NAME" };
            Dictionary<string, string> dictionaryColumn = ResultViewHelper.GetPkDisplayDictionary("800");

            Brush brush = Application.Current.Resources["InfoColor"] as SolidColorBrush;

            foreach (string fieldName in displayKey)
            {
                //if (!displayKey.Contains(fieldName)) continue;
                //if (fieldName == "MD_CHECKED")
                //{
                //    continue;
                //}
                Grid gridTemp = new Grid() { Margin = new Thickness(2), };
                while (gridTemp.ColumnDefinitions.Count < 2)
                {
                    gridTemp.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                }
                gridTemp.ColumnDefinitions[0].Width = new GridLength(70);
                TextBlock textBlockName = new TextBlock()
                {
                    Text = dictionaryColumn[fieldName],
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0)
                };
                textBlockName.SetBinding(TextBlock.ForegroundProperty, new Binding { Source = brush });

                TextBlock textBlockValue = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0),
                };

                if (fieldName == "MD_UB")
                    textBlockValue.SetBinding(TextBlock.TextProperty, new Binding(fieldName) { StringFormat = "{0}V" });
                else if (fieldName == "MD_UA")
                    textBlockValue.SetBinding(TextBlock.TextProperty, new Binding(fieldName) { StringFormat = "{0}A" });
                else if (fieldName == "MD_FREQUENCY")
                    textBlockValue.SetBinding(TextBlock.TextProperty, new Binding(fieldName) { StringFormat = "{0}Hz" });
                else
                    textBlockValue.SetBinding(TextBlock.TextProperty, new Binding(fieldName));
                Grid.SetColumn(textBlockValue, 1);
                gridTemp.Children.Add(textBlockName);
                gridTemp.Children.Add(textBlockValue);
                stackPanelMeterInfo.Children.Add(gridTemp);
            }
        }

    }
}
