using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using LYTest.ViewModel;
namespace LYTest.WPF.Skin
{
    /// <summary>
    /// 颜色主题
    /// </summary>
    public class ThemeItem : ViewModelBase
    {
        public ThemeItem()
        {
            Name = "默认";
            dictionary = new ResourceDictionary()
            {
                Source = new Uri(@"../Resources/ColorResource.xaml", UriKind.RelativeOrAbsolute)
            };
        }

        public ThemeItem(string resourceName)
        {
            Name = resourceName;
            dictionary = new ResourceDictionary()
            {
                Source = new Uri(string.Format(@"{0}/Skin/{1}.xaml", Directory.GetCurrentDirectory(), resourceName), UriKind.Absolute)
            };
        }

        private ResourceDictionary dictionary = new ResourceDictionary();
        private string name;

        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        public Brush ThemeBrush
        {
            get
            {
                if (dictionary.Contains("窗口背景深色"))
                {
                    return dictionary["窗口背景深色"] as Brush;
                }
                return null;
            }
        }
        /// <summary>
        /// 加载主题颜色
        /// </summary>
        public void Load()
        {
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            SkinManager.SetAppColor();
        }
    }
}
