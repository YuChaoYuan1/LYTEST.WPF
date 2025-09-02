using System;
using System.Globalization;
using System.Windows.Data;

namespace LYTest.WPF.SG.Converter
{
    /// <summary>
    /// 将表位序号格式化为D2
    /// </summary>
    public class IndexFormatConverter : IValueConverter
    {
        /// <summary>
        /// 将表位序号格式化为D2
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value.ToString();
            if (string.IsNullOrEmpty(str)) return "";

            return str.PadLeft(2, '0');
        }


        /// <returns></returns>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
