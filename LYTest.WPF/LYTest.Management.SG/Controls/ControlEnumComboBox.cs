using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LYTest.DAL;

namespace LYTest.DataManager.SG.Controls
{
    /// <summary>
    /// 根据PCode加载的下拉控件
    /// </summary>
    public class ControlEnumComboBox : ComboBox
    {
        public override void EndInit()
        {
            LoadDropDownList();
            base.EndInit();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "EnumName")
            {
                LoadDropDownList();
            }
            base.OnPropertyChanged(e);
        }

        public string EnumName
        {
            get { return (string)GetValue(EnumNameProperty); }
            set
            {
                SetValue(EnumNameProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for EnumName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnumNameProperty =
            DependencyProperty.Register("EnumName", typeof(string), typeof(ControlEnumComboBox), new PropertyMetadata(""));


        private void LoadDropDownList()
        {
            if (string.IsNullOrEmpty(EnumName))
            {
                IsEditable = true;
                return;
            }
            Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(EnumName);
            ItemsSource = dictionary.Keys;
        }
    }
}
