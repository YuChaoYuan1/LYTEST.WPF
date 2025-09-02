using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LYTest.WPF.SG.Converter
{
    /// <summary>
    /// 用户可见转换器
    /// </summary>
    public class UserVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// bool量与可见转换器
        /// </summary>
        /// <param name="value">当前用户权限</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">控件权限</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currentPermission = value as string;
            string controlPermission = parameter as string;
            if (string.IsNullOrEmpty(currentPermission) || string.IsNullOrEmpty(controlPermission))
            {
                return Visibility.Collapsed;
            }
            else
            {
                if (currentPermission.CompareTo(controlPermission) >= 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
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
            return "";
        }
    }
}
