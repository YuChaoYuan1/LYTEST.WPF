using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.User;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LYTest.WPF.Converter.CodePermission
{
    /// <summary>
    /// 编码允许修改转换器
    /// </summary>
    public class CodeModifyConverter : IValueConverter
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
                    return true;
                }
                else if (currentPermission == "1")          //管理员
                {
                    if ((intTemp > 0 && intTemp <= 3) || (intTemp > 10 && intTemp <= 13))
                    {
                        return true;
                    }
                }
                else if (currentPermission == "0")          //普通用户
                {
                    if (intTemp > 0 && intTemp <= 3)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
