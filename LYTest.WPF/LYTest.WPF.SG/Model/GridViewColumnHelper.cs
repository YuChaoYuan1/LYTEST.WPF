using System.Reflection;
using System.Windows.Controls;

namespace LYTest.WPF.SG.Model
{
    internal static class GridViewColumnHelper
    {
        private static readonly PropertyInfo DesiredWidthProperty =
            typeof(GridViewColumn).GetProperty("DesiredWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        public static double GetColumnWidth(this GridViewColumn column)
        {
            return (double.IsNaN(column.Width)) ? (double)DesiredWidthProperty.GetValue(column, null) : column.Width;
        }
    }
}
