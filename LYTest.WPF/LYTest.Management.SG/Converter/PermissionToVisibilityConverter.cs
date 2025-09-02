using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LYTest.DataManager.SG.Converter
{
    /// 仅超级用户可见
    /// <summary>
    /// 仅超级用户可见
    /// </summary>
    public class SuperUserVisibilityConverter : IValueConverter
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
            string temp = value as string;
            if (temp == "2")
            {
                return Visibility.Visible;
            }
            else
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
            return "";
        }
    }
    /// 超级用户和管理员才可见
    /// <summary>
    /// 超级用户和管理员才可见
    /// </summary>
    public class AdminUserVisiblityConverter : IValueConverter
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
            string temp = value as string;
            if (temp == "2" || temp =="1")
            {
                return Visibility.Visible;
            }
            else
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
            return "";
        }
    }
}
