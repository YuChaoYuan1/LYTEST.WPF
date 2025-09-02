using System;
using System.Globalization;
using System.Windows.Data;

namespace LYTest.WPF.SG.Converter
{
    /// <summary>
    /// A(B)转换器
    /// </summary>
    public class AbracketBConverter : IValueConverter
    {
        private string tt;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = value as string;
            tt = input;
            if (input != null)
            {
                string[] parts = input.Split('(', ')');
                if (parts.Length >= 2)
                {
                    // 返回需要显示的部分
                    // 这里假设第一个部分显示在第一列，第二个部分显示在第二列
                    if (parameter == null || parameter.ToString() == "A" || parameter.ToString() == "FirstPart")
                    {
                        return parts[0];
                    }
                    else if (parameter.ToString() == "B" || parameter.ToString() == "SecondPart")
                    {
                        return parts[1];
                    }
                    else
                        return value;
                }
                else
                    return parts[0];

            }

            // 如果输入无效，则返回空值
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string back)
            {
                string part;
                string[] parts = back.Split('(', ')');
                if (parts.Length >= 2)
                {
                    if (parameter == null || parameter.ToString() == "A" || parameter.ToString() == "FirstPart")
                        part = parts[0];
                    else if (parameter.ToString() == "B" || parameter.ToString() == "SecondPart")
                        part = parts[1];
                    else
                        return tt;
                }
                else
                    part = parts[0];

                if (tt == null) tt = "";
                string[] ttt = tt.Split('(', ')');
                if (parameter == null || parameter.ToString() == "A" || parameter.ToString() == "FirstPart")
                {
                    if (ttt.Length >= 2)
                        return part + "(" + ttt[1] + ")";
                    else
                        return part;
                }
                else if (parameter.ToString() == "B" || parameter.ToString() == "SecondPart")
                    return ttt[0] + "(" + part + ")";
            }
            return tt;
        }
    }
}
