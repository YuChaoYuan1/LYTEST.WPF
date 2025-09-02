using LYTest.ViewModel.CodeTree;
using System;
using System.Windows.Controls;

namespace LYTest.WPF.SG.Controls
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
