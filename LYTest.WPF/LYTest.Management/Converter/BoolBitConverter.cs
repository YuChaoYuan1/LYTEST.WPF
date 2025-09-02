using System;
using System.Globalization;
using System.Windows.Data;

namespace LYTest.DataManager.Converter
{
    /// bool和1,0转换器
    /// <summary>
    /// bool和1,0转换器
    /// </summary>
    public class BoolBitConverter : IValueConverter
    {
        /// bool和1,0转换器
        /// <summary>
        /// bool和1,0转换器
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((value as string) == "1")
                {
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        /// 未定义
        /// <summary>
        /// 未定义
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool)
                {
                    bool currentValue = (bool)value;
                    if (currentValue)
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "0";
            }
        }
    }
}
