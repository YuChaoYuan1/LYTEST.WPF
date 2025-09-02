using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.User;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LYTest.WPF.SG.Converter.CodePermission
{
    /// <summary>
    /// 编码允许选中转换器
    /// </summary>
    public class CodeSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumPermission p)
            {
                string currentPermission = UserViewModel.Instance.CurrentUser.GetProperty("USER_POWER") as string;

                if (currentPermission == "2")  //超级用户
                {
                    return true;
                }
                else if (currentPermission == "1")          //管理员
                {
                    if ((int)p < 20)
                    {
                        return true;
                    }
                }
                else if (currentPermission == "0")          //普通用户
                {
                    if ((int)p < 10)
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
