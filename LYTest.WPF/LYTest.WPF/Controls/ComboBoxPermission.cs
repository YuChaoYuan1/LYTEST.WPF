using LYTest.ViewModel.CodeTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LYTest.WPF.Controls
{
    /// <summary>
    /// 用户权限下拉框
    /// </summary>
    public class ComboBoxPermission : ComboBox
    {
        public ComboBoxPermission() : base()
        {
            ItemsSource = Enum.GetValues(typeof(EnumPermission));
        }
    }

}
