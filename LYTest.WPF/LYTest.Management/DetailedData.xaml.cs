using LYTest.DAL.DataBaseView;
using LYTest.DataManager.ViewModel;
using LYTest.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace LYTest.DataManager
{
    /// <summary>
    /// DetailedData.xaml 的交互逻辑
    /// </summary>
    public partial class DetailedData : Window
    {
        private readonly AllMeterResult meterResults = new AllMeterResult(null);
        public DetailedData()
        {
            InitializeComponent();
            gridMeterResult.DataContext = meterResults;

            LoadEquipInfo();
        }

        private static DetailedData instance = null;

        public static DetailedData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DetailedData();
                }
                return instance;
            }
        }

        readonly string DeviceDataPath = System.IO.Directory.GetCurrentDirectory() + "\\Ini\\DeviceData.ini";
        /// <summary>
        /// 加载台体信息
        /// </summary>
        private void LoadEquipInfo()
        {
            string[] NameS = Core.OperateFile.GetINI("Data", "名称", DeviceDataPath).Split('|');
            string[] Values = new string[NameS.Length];
            for (int i = 0; i < NameS.Length; i++)
            {
                Values[i] = string.Empty;
            }
            string[] TempValues = Core.OperateFile.GetINI("Data", "值", DeviceDataPath).Split('|');
            Array.Copy(TempValues, Values, Math.Min(NameS.Length, TempValues.Length));
            for (int i = 0; i < NameS.Length; i++)
            {
                string nameTemp = NameS[i];
                string value = Values[i];
                Grid gridTemp = new Grid()
                {
                    Margin = new Thickness(2),
                };
                while (gridTemp.ColumnDefinitions.Count < 2)
                {
                    gridTemp.ColumnDefinitions.Add(
                        new ColumnDefinition()
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });
                }
                gridTemp.ColumnDefinitions[0].Width = new GridLength(90);
                TextBlock textBlockName = new TextBlock()
                {
                    Text = nameTemp,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0),
                    ToolTip = nameTemp
                };
                TextBox textBoxValue = new TextBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(3, 0, 3, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Text = value
                };
                Grid.SetColumn(textBoxValue, 1);
                gridTemp.Children.Add(textBlockName);
                gridTemp.Children.Add(textBoxValue);
                stackPanelEquipInfo.Children.Add(gridTemp);
            }
            //默认替换标签
            {
                string EmptyMark;
                if (!Core.OperateFile.ExistINIKey("MarkFlag", "EmptyMark", DeviceDataPath))
                {
                    EmptyMark = "/";
                }
                else
                {
                    EmptyMark = Core.OperateFile.GetINI("MarkFlag", "EmptyMark", DeviceDataPath);
                }

                Grid grid0 = new Grid()
                {
                    Margin = new Thickness(2),
                };
                grid0.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(90, GridUnitType.Pixel) });
                grid0.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                TextBlock textBlockName = new TextBlock()
                {
                    Text = "默认替换符",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0),
                    ToolTip = "报表数据默认替换符"
                };
                TextBox textBoxValue = new TextBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(3, 0, 3, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Text = EmptyMark
                };
                Grid.SetColumn(textBoxValue, 1);
                grid0.Children.Add(textBlockName);
                grid0.Children.Add(textBoxValue);
                stackPanelMark.Children.Add(grid0);
            }
            //0值加符号
            bool PlusSignMark;
            if (!Core.OperateFile.ExistINIKey("MarkFlag", "PlusSignMark", DeviceDataPath))
            {
                PlusSignMark = false;
            }
            else
            {
                PlusSignMark = Core.OperateFile.GetINI("MarkFlag", "PlusSignMark", DeviceDataPath).ToUpper() == "YES";
            }
            Grid grid1 = new Grid()
            {
                Margin = new Thickness(2),
            };
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(90, GridUnitType.Pixel) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            CheckBox checkBox = new CheckBox()
            {
                IsChecked = PlusSignMark,
                Content = "零值加正负号"
            };

            Grid.SetColumn(checkBox, 0);
            grid1.Children.Add(checkBox);
            stackPanelMark.Children.Add(grid1);

            //检定依据
            for (int i = 0; i < Equipments.Instance.ReguNames.Count; i++)
            {
                string aKey = Equipments.Instance.ReguNames[i];
                string aValue;
                if (!Core.OperateFile.ExistINIKey("Regulation", aKey, DeviceDataPath))
                {
                    aValue = Equipments.Instance.ReguDefaultValues[i];
                }
                else
                {
                    aValue = Core.OperateFile.GetINI("Regulation", aKey, DeviceDataPath);
                }
                Grid grid2 = new Grid()
                {
                    Margin = new Thickness(2),
                };
                grid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(90, GridUnitType.Pixel) });
                grid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                TextBlock textBlockName = new TextBlock()
                {
                    Text = aKey,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0),
                    ToolTip = "检定依据标签：根据单、三相和费控类型自动匹配"
                };
                TextBox textBoxValue = new TextBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(3, 0, 3, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Text = aValue
                };
                Grid.SetColumn(textBoxValue, 1);
                grid2.Children.Add(textBlockName);
                grid2.Children.Add(textBoxValue);
                stackPanelMark.Children.Add(grid2);
            }

            string[] addNameS = Core.OperateFile.GetINI("Regulation", "名称", DeviceDataPath).Split('|');
            for (int i = 0; i < addNameS.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(addNameS[i])) continue;
                if (Equipments.Instance.ReguNames.Contains(addNameS[i])) continue;
                string aValue = Core.OperateFile.GetINI("Regulation", addNameS[i], DeviceDataPath);
                Grid gridTemp = new Grid()
                {
                    Margin = new Thickness(2),
                };
                gridTemp.ColumnDefinitions.Add(
                        new ColumnDefinition()
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });
                gridTemp.ColumnDefinitions.Add(
                        new ColumnDefinition()
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });
                gridTemp.ColumnDefinitions[0].Width = new GridLength(90);
                TextBlock textBlockName = new TextBlock()
                {
                    Text = addNameS[i],
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0),
                    ToolTip = addNameS[i]
                };
                TextBox textBoxValue = new TextBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(3, 0, 3, 0),
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Text = aValue
                };
                Grid.SetColumn(textBoxValue, 1);
                gridTemp.Children.Add(textBlockName);
                gridTemp.Children.Add(textBoxValue);
                stackPanelMark.Children.Add(gridTemp);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = "";
                string value = "";
                for (int i = 0; i < stackPanelEquipInfo.Children.Count; i++)
                {
                    if (stackPanelEquipInfo.Children[i] is Grid gridTemp)
                    {
                        for (int j = 0; j < gridTemp.Children.Count; j++)
                        {

                            if (gridTemp.Children[j] is TextBlock) //名称
                            {
                                TextBlock textBlock = gridTemp.Children[j] as TextBlock;
                                string tempName = (textBlock.Text ?? " ") + "|";
                                //if (j == gridTemp.Children.Count - 1)
                                //{
                                //    tempName = tempName.TrimEnd('|');
                                //}
                                name += tempName;

                            }
                            else if (gridTemp.Children[j] is TextBox)   //值
                            {
                                TextBox textBlock = gridTemp.Children[j] as TextBox;
                                string tempValue = (textBlock.Text ?? " ") + "|";
                                //if (j == gridTemp.Children.Count - 1)
                                //{
                                //    tempValue = tempValue.TrimEnd('|');
                                //}
                                value += tempValue;
                            }

                        }
                    }
                }

                Core.OperateFile.WriteINI("Data", "名称", name, DeviceDataPath);
                Core.OperateFile.WriteINI("Data", "值", value, DeviceDataPath);

                List<string> emnameList = new List<string>();

                for (int i = 0; i < stackPanelMark.Children.Count; i++)
                {
                    if (stackPanelMark.Children[i] is Grid)
                    {
                        string aKey = "";
                        string aValue = "";
                        Grid gridTemp = stackPanelMark.Children[i] as Grid;
                        for (int j = 0; j < gridTemp.Children.Count; j++)
                        {
                            if (gridTemp.Children[j] is TextBlock) //名称
                            {
                                TextBlock textBlock = gridTemp.Children[j] as TextBlock;
                                aKey = textBlock.Text;
                            }
                            else if (gridTemp.Children[j] is TextBox)   //值
                            {
                                TextBox textBlock = gridTemp.Children[j] as TextBox;
                                aValue = textBlock.Text;
                            }
                            else if (gridTemp.Children[j] is CheckBox checkBox)
                            {
                                aKey = "PlusSignMark";
                                aValue = checkBox.IsChecked == true ? "yes" : "no";
                            }
                        }
                        if (aKey == "默认替换符")
                        {
                            Core.OperateFile.WriteINI("MarkFlag", "EmptyMark", aValue, DeviceDataPath);
                        }
                        else if (aKey == "零值加正负号" || aKey == "PlusSignMark")
                        {
                            Core.OperateFile.WriteINI("MarkFlag", aKey, aValue, DeviceDataPath);
                        }
                        else if (Equipments.Instance.ReguNames.Contains(aKey))
                        {
                            emnameList.Add(aKey);
                            Core.OperateFile.WriteINI("Regulation", aKey, aValue, DeviceDataPath);
                        }
                        else
                        {
                            emnameList.Add(aKey);
                            Core.OperateFile.WriteINI("Regulation", aKey, aValue, DeviceDataPath);
                        }
                    }
                }
                Core.OperateFile.WriteINI("Regulation", "名称", string.Join("|", emnameList), DeviceDataPath);

                MessageBox.Show("保存成功");
            }
            catch (System.Exception ex)
            {

                MessageBox.Show("保存失败" + ex);
            }
        }

        public bool IsClose = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!IsClose)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                base.OnClosing(e);
            }
        }
    }
}
