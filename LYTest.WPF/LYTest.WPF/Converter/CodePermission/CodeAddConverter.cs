using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.User;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LYTest.WPF.Converter.CodePermission
{
    /// <summary>
    /// 编码允许添加转换器
    /// </summary>
    public class CodeAddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumPermission)
            {
                string currentPermission = UserViewModel.Instance.CurrentUser.GetProperty("USER_POWER") as string;
                EnumPermission codePermission = (EnumPermission)value;
                int intTemp = (int)codePermission;

                if (currentPermission == "2")  //超级用户
                {
                    return Visibility.Visible;
                }
                else if (currentPermission == "1")          //管理员
                {
                    if ((intTemp > 1 && intTemp <= 3) || (intTemp > 11 && intTemp <= 13))
                    {
                        return Visibility.Visible;
                    }
                }
                else if (currentPermission == "0")          //普通用户
                {
                    if (intTemp > 1 && intTemp <= 3)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
