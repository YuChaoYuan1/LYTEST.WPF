using LYTest.ViewModel.Menu;
using LYTest.WPF.Model;
using LYTest.WPF.UiGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.View
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
                Border border = new Border()
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Application.Current.Resources["边框颜色"] as Brush,
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(5),
                };
                UniformGrid gridTemp = new UniformGrid();
                gridTemp.Rows = 4;
                foreach (MenuConfigItem menuItemTemp in menuCollection)
                {
                    Viewbox viewBox = new Viewbox();
                    Button button = ControlFactory.CreateButton(menuItemTemp, false);
                    if (button != null && button.Visibility == Visibility.Visible)
                    {
                        button.Margin = new Thickness(5);
                        viewBox.Child = button;
                        viewBox.Margin = new Thickness(5);
                        gridTemp.Children.Add(viewBox);
                    }
                }
                border.Child = gridTemp;
                panelMenu.Children.Add(border);
            }
        }
    }
}
