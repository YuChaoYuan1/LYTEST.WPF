using LYTest.ViewModel.CodeTree;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LYTest.WPF.SG.Schema
{
    /// <summary>
    /// ControlComboParaCode.xaml 的交互逻辑
    /// </summary>
    public partial class ControlComboParaCode : ComboBox
    {
        public ControlComboParaCode()
        {
            LoadComboSource();
            Binding binding = new Binding("CodeName")
            {
                Source = this
            };
            SetBinding(ComboBox.SelectedItemProperty, binding);
        }

        private void LoadComboSource()
        {
            CodeTreeNode nodeTemp = CodeTreeViewModel.Instance.GetCodeByEnName("CheckParamSource", 1);
            IEnumerable<string> enumCodes = from nodeChild in nodeTemp.Children select nodeChild.CODE_NAME;
            List<string> codeList = enumCodes.ToList();
            codeList.Insert(0, "");
            codeList.Add("数据标识列表");
            ItemsSource = codeList;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "CodeName")
            {
                LoadComboSource();
            }
            base.OnPropertyChanged(e);
        }

        public string CodeName
        {
            get { return (string)GetValue(CodeNameProperty); }
            set { SetValue(CodeNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnumCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeNameProperty =
            DependencyProperty.Register("CodeName", typeof(string), typeof(ControlComboParaCode), new PropertyMetadata(""));
    }
}
