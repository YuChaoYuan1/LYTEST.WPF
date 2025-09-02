using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LYTest.WPF.SG.Converter
{
    /// <summary>
    /// bool量与可见转换器
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// bool量与可见转换器
        /// <summary>
        /// bool量与可见转换器
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
                if (((bool?)value).Value)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            catch
            {
                return Visibility.Collapsed;
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
                Visibility visibility = (Visibility)value;
                if (visibility == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }


    /// <summary>
    /// bool量取反与可见转换器
    /// </summary>
    public class NotBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// bool量取反与可见转换器
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
                if (((bool?)value).Value)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            catch
            {
                return Visibility.Collapsed;
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
                Visibility visibility = (Visibility)value;
                if (visibility == Visibility.Visible)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
