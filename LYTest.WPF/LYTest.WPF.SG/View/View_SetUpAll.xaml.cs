using LYTest.ViewModel.Menu;
using LYTest.WPF.SG.Model;
using LYTest.WPF.SG.UiGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_SetUpAll.xaml 的交互逻辑
    /// </summary>
    public partial class View_SetUpAll 
    {
        public View_SetUpAll()
        {
            InitializeComponent();
            DockStyle.FloatingSize = new Size(1000, 650);
            DockStyle.ResizeMode = ResizeMode.NoResize;
            DockStyle.ResizeMode = ResizeMode.NoResize;
            Name = "设置";
            DataContext = MainViewModel.Instance;
            LoadMenu();
        }

        private void LoadMenu()
        {
            MenuViewModel menuModel = new MenuViewModel();
            Array arrayTemp = Enum.GetValues(typeof(EnumMenuCategory));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                EnumMenuCategory category = (EnumMenuCategory)(arrayTemp.GetValue(i));

                if (category == EnumMenuCategory.常用)
                    continue;

                var menuCollection = menuModel.Menus.Where(item => item.MenuCategory == category);
                if (menuCollection == null || menuCollection.Count() == 0)
                {
                    continue;
                }

                WrapPanel gridTemp = new WrapPanel();
                //gridTemp.Rows = 4;
                foreach (MenuConfigItem menuItemTemp in menuCollection)
                {
                    //Viewbox viewBox = new Viewbox();
                    Button button = ControlFactory.CreateButton(menuItemTemp, false);
                    if (button != null && button.Visibility == Visibility.Visible)
                    {

                        button.Margin = new Thickness(10);
                        button.Width = 120;
                        button.Height = 180;
                        //viewBox.Child = button;
                        //viewBox.Margin = new Thickness(5);
                        gridTemp.Children.Add(button);
                    }
                }
                svPanel.Content = gridTemp;
            }
        }
    }
}
