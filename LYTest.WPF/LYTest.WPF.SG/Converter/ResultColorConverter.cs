using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LYTest.WPF.SG.Converter
{
    /// <summary>
    /// 结论颜色转换器
    /// </summary>
    public class ResultColorConverter : IValueConverter
    {
        /// <summary>
        /// 结论颜色转换器
        /// </summary>
        /// <param name="value">合格:绿色,不合格:红色,默认:黑色</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string temp = value as string;
            switch (temp)
            {
                case "合格":
                    return new SolidColorBrush(Colors.Green);
                case "不合格":
                    return new SolidColorBrush(Colors.Red);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }


        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
