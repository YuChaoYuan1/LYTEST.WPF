using LYTest.DAL;
using LYTest.ViewModel;
using LYTest.ViewModel.InputPara;
using LYTest.ViewModel.Menu;
using LYTest.ViewModel.User;
using LYTest.WPF.Converter;
using LYTest.WPF.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LYTest.WPF.UiGeneral
{
    /// <summary>
    /// 目录生成器
    /// </summary>
    public class ControlFactory
    {
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="isMainMenu">是否为主界面目录</param>
        /// <returns></returns>
        public static Button CreateButton(MenuConfigItem menuItem, bool isMainMenu)
        {
            if (menuItem == null || !menuItem.IsValid)
            {
                return null;
            }
            if (isMainMenu && menuItem.IsMainMenu == EnumMainMenu.否)
            {
                return null;
            }
            string buttonHeader = menuItem.MenuName;
            #region 构造控件
            if (Application.Current.Resources.Contains(menuItem.MenuName))
            {
                buttonHeader = Application.Current.Resources[menuItem.MenuName] as string;
            }
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Button button = new Button()
            {
                Content = stackPanel,
                Margin = new Thickness(3, 0, 3, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                Focusable = false,
                Style = Application.Current.Resources["StyleButtonMenu"] as Style
                //Style = Application.Current.Resources["GlassButton"] as Style

            };
            button.SetBinding(Button.CommandProperty, new Binding("LocalCommand"));
            Image imageCurrent = new Image()
            {
                //Margin = new Thickness(8, 0, 8, 0),  //===========
                Source = menuItem.MenuImage?.ImageControl,
                Width = (menuItem.IsMainMenu == EnumMainMenu.否) ? 32 : 32,
                Height = (menuItem.IsMainMenu == EnumMainMenu.否) ? 32 : 32,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            TextBlock textBlockButtonText = new TextBlock()
            {
                //Foreground = System.Windows.Media.Brushes.White, //===========
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = buttonHeader
            };
            stackPanel.Children.Add(imageCurrent);
            stackPanel.Children.Add(textBlockButtonText);
            #endregion
            #region 设置命令绑定
            if (menuItem.MenuType == EnumMenuType.设备控制 || menuItem.MenuType == EnumMenuType.检定控制)
            {
                button.DataContext = EquipmentData.DeviceManager;
                button.CommandParameter = menuItem.MenuClass;
            }
            else
            {
                button.CommandParameter = string.Format("{0}|{1}", menuItem.MenuName, menuItem.MenuClass);
            }
            #endregion
            #region 用户权限
            //控件仅有超级用户可见
            Binding bindingUserVisible = new Binding("USER_POWER");
            bindingUserVisible.Source = UserViewModel.Instance.CurrentUser;
            bindingUserVisible.Converter = Application.Current.Resources["UserVisibilityConverter"] as UserVisibilityConverter;
            bindingUserVisible.ConverterParameter = ((int)menuItem.UserCode).ToString();
            button.SetBinding(Button.VisibilityProperty, bindingUserVisible);
            //if (menuItem.UserCode == EnumUserVisible.普通用户不可见)
            //{
            //    //控件仅有管理员可见
            //    Binding bindingUserVisible = new Binding("chrQx");
            //    bindingUserVisible.Source = UserViewModel.Instance.CurrentUser;
            //    bindingUserVisible.Converter = new AdminUserVisiblityConverter();
            //    button.SetBinding(Button.VisibilityProperty, bindingUserVisible);
            //}
            //else if (menuItem.UserCode == EnumUserVisible.超级用户可见)
            //{
            //    //控件仅有超级用户可见
            //    Binding bindingUserVisible = new Binding("chrQx");
            //    bindingUserVisible.Source = UserViewModel.Instance.CurrentUser;
            //    bindingUserVisible.Converter = new UserVisibilityConverter();
            //    bindingUserVisible.ConverterParameter = ((int)menuItem.UserCode).ToString();
            //    button.SetBinding(Button.VisibilityProperty, bindingUserVisible);
            //}
            #endregion
            #region 检定时不可用
            if (menuItem.CheckingEnable == EnumCheckingEnable.不可用)
            {
                Binding bindingCheckEnable = new Binding("IsChecking");
                bindingCheckEnable.Source = EquipmentData.Controller;
                bindingCheckEnable.Mode = BindingMode.OneWay;
                bindingCheckEnable.Converter = new NotBoolConverter();
                button.SetBinding(Button.IsEnabledProperty, bindingCheckEnable);
            }
            #endregion
            return button;
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnHeader">列头名称</param>
        /// <param name="codeType">数据项编码</param>
        /// <param name="allowAdd">是否允许添加新项</param>
        /// <returns></returns>
        public static DataGridColumn CreateColumn(string columnHeader, string codeType, string bindingPath, bool allowAdd = false)
        {
            if (string.IsNullOrEmpty(codeType))
            {
                DataGridColumn column = new DataGridTextColumn
                {
                    Header = columnHeader,
                    Binding = new Binding(bindingPath) { Mode = BindingMode.TwoWay },
                    Width = 120,
                    IsReadOnly = false,
                    EditingElementStyle = Application.Current.Resources["StyleEditTextBox"] as Style
                };
                return column;
            }
            else
            {

                if (codeType == "ConnProtocolDataName")
                {
                    DataGridColumn columnYaojian = GenerateCustomColumn(columnHeader);
                    columnYaojian.Header = columnHeader;
                    columnYaojian.Width = 150;
                    return columnYaojian;
                }
                else
                {
                    Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(codeType);
                    List<string> datasource = dictionary.Keys.ToList();
                    if (allowAdd)
                    {
                        datasource.Add("添加新项");
                    }
                    DataGridColumn column = new DataGridComboBoxColumn
                    {
                        Header = columnHeader,
                        Width = 80,
                        IsReadOnly = false,
                        ItemsSource = datasource,
                        SelectedItemBinding = new Binding(bindingPath) { Mode = BindingMode.TwoWay },
                        EditingElementStyle = Application.Current.Resources["StyleComboBox"] as Style,
                    };
                    return column;
                }
            }
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnHeader">列头名称</param>
        /// <param name="codeType">数据项编码</param>
        /// <param name="allowAdd">是否允许添加新项</param>
        /// <returns></returns>
        public static DataGridColumn CreateColumn(string columnHeader, string codeType, string bindingPath, EnumValueType ValueType, bool allowAdd = false, IValueConverter converter = null, object converterParameter = null)
        {
            if (ValueType == EnumValueType.日期 || ValueType == EnumValueType.时间 || ValueType == EnumValueType.日期时间)
            {
                switch (ValueType)
                {
                    case EnumValueType.日期:
                        break;
                    case EnumValueType.时间:
                        break;
                    case EnumValueType.日期时间:
                        break;
                    default:
                        break;
                }
                DataGridColumn column = GenerateDateTimeColumn(columnHeader, bindingPath);
                column.Header = columnHeader;
                column.ClipboardContentBinding = new Binding(bindingPath)
                {
                    Mode = BindingMode.TwoWay
                };
                column.Width = 120;
                column.IsReadOnly = false;
                return column;

            }
            else
            {
                if (string.IsNullOrEmpty(codeType))
                {
                    DataGridColumn column = new DataGridTextColumn
                    {
                        Header = columnHeader,
                        Binding = new Binding(bindingPath) { Mode = BindingMode.TwoWay },
                        Width = 120,
                        IsReadOnly = false,
                        EditingElementStyle = Application.Current.Resources["StyleEditTextBox"] as Style
                    };
                    return column;
                }
                else
                {

                    if (codeType == "ConnProtocolDataName")
                    {
                        DataGridColumn columnYaojian = GenerateCustomColumn(columnHeader);
                        columnYaojian.Header = columnHeader;
                        columnYaojian.Width = 150;
                        return columnYaojian;
                    }
                    else
                    {
                        Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(codeType);
                        List<string> datasource = dictionary.Keys.ToList();
                        if (allowAdd)
                        {
                            datasource.Add("添加新项");
                        }
                        DataGridColumn column = new DataGridComboBoxColumn
                        {
                            Header = columnHeader,
                            Width = 80,
                            IsReadOnly = false,
                            ItemsSource = datasource,
                            SelectedItemBinding = new Binding(bindingPath)
                            {
                                Mode = BindingMode.TwoWay,
                                Converter = converter,
                                ConverterParameter = converterParameter
                            },
                            EditingElementStyle = Application.Current.Resources["StyleComboBox"] as Style,
                        };
                        return column;
                    }
                }
            }
        }

        private static DataGridColumn GenerateDateTimeColumn(string Header, string BindSource)
        {
            FrameworkElementFactory datePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
            datePickerFactory.SetBinding(DatePicker.SelectedDateProperty, new Binding(BindSource) { Mode = BindingMode.TwoWay, Converter = new DateOnlyConverter() });


            Binding textBinding = new Binding(BindSource)
            {
                Mode = BindingMode.OneWay,
                StringFormat = "yyyy/MM/dd"
            };

            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, textBinding);

            DataGridTemplateColumn column = new DataGridTemplateColumn
            {
                Header = Header,
                CellEditingTemplate = new DataTemplate
                {
                    VisualTree = datePickerFactory
                },
                CellTemplate = new DataTemplate
                {
                    VisualTree = textBlockFactory
                }
            };

            return column;

        }

        private static DataGridColumn GenerateCustomColumn(string Header)
        {
            // 创建一个自定义的DataGridTemplateColumn
            DataGridTemplateColumn customColumn = new DataGridTemplateColumn();

            // 创建一个DataTemplate来定义列的外观
            DataTemplate columnTemplate = new DataTemplate();

            // 创建一个Grid作为DataTemplate的根元素
            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            // 创建第一个部分的TextBlock
            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding(Header)); // 设置TextBlock的绑定属性
            gridFactory.AppendChild(textBlockFactory);

            // 创建第二个部分的Button
            FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(Button));
            //buttonFactory.SetValue(Button.ContentProperty, "按钮"); // 设置Button的显示内容
            buttonFactory.SetValue(Button.WidthProperty, 20.0); ;
            buttonFactory.SetValue(Button.StyleProperty, Application.Current.Resources["DataGridToggleButton"] as Style);
            //var s1 = Application.Current.Resources["DataGridToggleButton"] as Style;
            //ToggleButton
            buttonFactory.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Right); // 设置Button靠右对齐
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click)); // 为Button的点击事件添加逻辑
            gridFactory.AppendChild(buttonFactory);

            // 将Grid设置为DataTemplate的VisualTree
            columnTemplate.VisualTree = gridFactory;

            // 将DataTemplate设置为自定义列的CellTemplate
            customColumn.CellTemplate = columnTemplate;

            return customColumn;
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            //// 按钮点击事件的逻辑
            //// 在此处添加你想要执行的代码
            Button button = (Button)sender;
            // 获取按钮在屏幕上的位置
            Point buttonPosition = button.PointToScreen(new Point(0, 0));

            // 设置窗体的位置
            double windowLeft = buttonPosition.X + button.ActualWidth;
            double windowTop = buttonPosition.Y;

            // 调整窗体位置，避免超出屏幕范围
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            //计算屏幕缩放比例
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            var ratio = (int)(graphics.DpiX * 1.041666667);
            windowLeft = windowLeft / (ratio / 100f) + 2;
            windowTop = windowTop / (ratio / 100f);

            if (windowLeft + Window_ProtocolSelect.Instance.Width > screenWidth)
            {
                windowLeft = screenWidth - Window_ProtocolSelect.Instance.Width;
            }

            if (windowTop + Window_ProtocolSelect.Instance.Height > screenHeight)
            {
                windowTop = screenHeight - Window_ProtocolSelect.Instance.Height;

            }


            // 显示窗体
            Window_ProtocolSelect.Instance.ShowProtocol(windowLeft, windowTop, button.DataContext as DynamicViewModel);
        }
    }
}
