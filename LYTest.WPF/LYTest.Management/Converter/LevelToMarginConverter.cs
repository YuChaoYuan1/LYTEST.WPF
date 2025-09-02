using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LYTest.DataManager.Converter
{
    public class LevelToMarginConverter : IValueConverter
    {
        public object Convert(object o, Type type, object parameter,
                              CultureInfo culture)
        {
            return new Thickness(3+((int)o-1) * c_IndentSize, 1, 1, 1);
        }

        public object ConvertBack(object o, Type type, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private const double c_IndentSize = 20;
    }
}
