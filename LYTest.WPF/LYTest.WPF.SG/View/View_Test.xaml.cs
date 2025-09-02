using LYTest.Core;
using LYTest.Utility;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckInfo;
using LYTest.WPF.SG.Converter;
using LYTest.WPF.SG.Model;
using LYTest.WPF.SG.View.Windows;
using LYTest.WPF.SG.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_Test.xaml 的交互逻辑
    /// </summary>
    public partial class View_Test
    {
        TestVM vm = new TestVM();
        public View_Test()
        {
            InitializeComponent();
            DataContext = vm;
            Name = "检定";
            InitializeColumns();
            comboBoxSchema.DataContext = EquipmentData.SchemaModels;
            treeSchemaMeters.DataContext = EquipmentData.CheckResults;
            treeSchemaLeft.DataContext = EquipmentData.CheckResults;
            EquipmentData.CheckResults.PropertyChanged += CheckResults_PropertyChanged;
            DockStyle.Position = DockSideE.Tab;
            DockStyle.FloatingSize = SystemParameters.WorkArea.Size;
            textBlockCheckPara.DataContext = EquipmentData.Controller;

            Binding bindingRefresh = new Binding("IsChecking")
            {
                Source = EquipmentData.Controller,
                Converter = new NotBoolConverter()
            };
            buttonRefresh.SetBinding(IsEnabledProperty, bindingRefresh);
            comboBoxSchema.SetBinding(IsEnabledProperty, bindingRefresh);

            treeSchemaMeters.Loaded += TreeScheme_Loaded;
            EquipmentData.SchemaModels.PropertyChanged += SchemaModels_PropertyChanged;
        }

        private void SchemaModels_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedSchema")
            {
                Window_Wait.Instance.StateWait("正在刷新方案...", 25000);
            }
            else if (e.PropertyName == "EndWait")
            {
                Window_Wait.Instance.EndWait();
                Dispatcher.Invoke(() => buttonRefresh.IsEnabled = true);
            }
        }

        /// <summary>
        /// 初始化表位消息
        /// </summary>
        private void InitializeColumns()
        {
            //GridViewColumnCollection columnsPos = Application.Current.Resources["ColumnCollectionMeterPos"] as GridViewColumnCollection;
            //删除所有表位列
            GridViewColumnCollection columns = Application.Current.Resources["ColumnCollection"] as GridViewColumnCollection;
            while (columns.Count > 2)
            {
                columns.RemoveAt(2);
            }
            //载入表位
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {

                Binding binding = new Binding("MD_CHECKED")
                {
                    Source = EquipmentData.MeterGroupInfo.Meters[i],
                    Converter = new BoolBitConverter()
                };

                CheckBox chk = new CheckBox
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = string.Format("#{0:D2}", i + 1),
                };
                chk.PreviewMouseLeftButtonDown += CheckBoxTemp_PreviewMouseLeftButtonDown;
                chk.SetBinding(ToggleButton.IsCheckedProperty, binding);


                #region 动态模板
                Binding bindingText = new Binding($"ResultSummary.表位{i + 1}.ResultValue");

                Binding bindingColor = new Binding($"ResultSummary.表位{i + 1}.Result")
                {
                    Converter = new ResultColorConverter()
                };

                StackPanel tip = new StackPanel();
                tip.Children.Add(new TextBlock() { Text = $"{i + 1}表位" });

                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock), "textBlock");
                factory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                factory.SetBinding(TextBlock.TextProperty, bindingText);
                factory.SetBinding(TextBlock.ForegroundProperty, bindingColor);
                factory.SetBinding(TextBlock.FontSizeProperty, new Binding("NameFontSize"));
                factory.SetValue(ToolTipProperty, tip);

                //columnsPos.Add(new GridViewColumn
                //{
                //    Header = chk,
                //    Width = 60,
                //    //CellTemplate = new DataTemplate() { VisualTree = factory },
                //});
                GridViewColumn column = new GridViewColumn
                {
                    Header = chk,
                    Width = 60,
                    CellTemplate = new DataTemplate() { VisualTree = factory, },
                };
                #endregion

                columns.Add(column);

            }

        }


        /// <summary>
        /// 表位复选框改变事件
        /// </summary>
        private void CheckBoxTemp_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CheckBox temp = sender as CheckBox;
            if (temp.IsChecked.HasValue)
            {
                DynamicViewModel modelTemp = temp.GetBindingExpression(CheckBox.IsCheckedProperty).DataItem as DynamicViewModel;
                DAL.DynamicModel Model2 = new DAL.DynamicModel();
                if (modelTemp != null)
                {
                    object objTemp = modelTemp.GetProperty("MD_CHECKED");
                    if (objTemp.ToString() == "1")
                    {
                        modelTemp.SetProperty("MD_CHECKED", "0");
                        Model2.SetProperty("MD_CHECKED", "0");
                    }
                    else
                    {
                        modelTemp.SetProperty("MD_CHECKED", "1");
                        Model2.SetProperty("MD_CHECKED", "1");

                    }
                    TaskManager.AddWcfAction(() =>
                    {
                        EquipmentData.MeterGroupInfo.UpdateCheckFlag();
                    });
                    EquipmentData.CheckResults.RefreshYaojian();
                }
                string id = modelTemp.GetProperty("METER_ID") as string;
                string where1 = $"METER_ID = '{id}'";
                DAL.DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_CHECKED" });
            }
            e.Handled = true;

        }

        void TreeScheme_Loaded(object sender, RoutedEventArgs e)
        {
            if (EquipmentData.CheckResults.CheckNodeCurrent == null)
            {
                return;
            }
            if (EquipmentData.CheckResults.CheckNodeCurrent.Level == 1)
            {
                if (treeSchemaMeters.ItemContainerGenerator.ContainerFromItem(EquipmentData.CheckResults.CheckNodeCurrent.Parent) is TreeViewItem treeItem)
                {
                    treeItem.IsExpanded = true;
                }
            }
        }



        private void CheckResults_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CheckNodeCurrent")
            {
                return;
            }
            Dispatcher.Invoke(new Action(() =>
            {
                CheckNodeViewModel nodeSelected = treeSchemaMeters.SelectedItem as CheckNodeViewModel;
                if (nodeSelected == EquipmentData.CheckResults.CheckNodeCurrent)
                {
                    return;
                }
                if (EquipmentData.CheckResults.CheckNodeCurrent != null)
                {
                    List<CheckNodeViewModel> nodeList = new List<CheckNodeViewModel>() { EquipmentData.CheckResults.CheckNodeCurrent };
                    CheckNodeViewModel nodeParent = EquipmentData.CheckResults.CheckNodeCurrent.Parent;
                    while (nodeParent != null)
                    {
                        nodeList.Add(nodeParent);
                        nodeParent = nodeParent.Parent;
                    }
                    if (treeSchemaMeters.ItemContainerGenerator.ContainerFromItem(nodeList[nodeList.Count - 1]) is TreeViewItem treeItem)
                    {
                        treeItem.IsExpanded = true;
                    }
                    else
                    {
                        return;
                    }
                    for (int i = nodeList.Count - 2; i >= 0; i--)
                    {
                        treeItem = treeItem.ItemContainerGenerator.ContainerFromItem(nodeList[i]) as TreeViewItem;
                        if (treeItem == null)
                        {
                            break;
                        }
                        else
                        {
                            treeItem.IsExpanded = true;
                        }
                    }
                    if (treeItem != null)
                    {
                        treeItem.IsSelected = true;
                        treeItem.BringIntoView();
                    }
                }
            }));
        }

        public override void Dispose()
        {
            treeSchemaMeters.Loaded -= TreeScheme_Loaded;
            treeSchemaLeft.Loaded -= TreeScheme_Loaded;
            BindingOperations.ClearAllBindings(this);
            comboBoxSchema.DataContext = null;
            treeSchemaMeters.DataContext = null;
            treeSchemaLeft.DataContext = null;
            treeViewDecorator.Target = null;
            EquipmentData.CheckResults.PropertyChanged -= CheckResults_PropertyChanged;
            treeSchemaMeters.SelectedItemChanged -= TreeSchema1_SelectedItemChanged;
            treeSchemaLeft.SelectedItemChanged -= TreeSchema1_SelectedItemChanged;
            base.Dispose();
        }



        /// <summary>
        /// 展开
        /// </summary>
        private void Click_SchemaExpand(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuTemp)
            {
                if (menuTemp.Name == "clearData")  //清除数据
                {
                    string[] arrayResult = new string[EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count];
                    List<string> list = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults[0].GetAllProperyName();

                    for (int i = 0; i < list.Count; i++)     //修改所有属性
                    {
                        if (list[i] == "要检") continue;
                        for (int k = 0; k < EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count; k++)
                        {
                            arrayResult[k] = "";
                        }
                        EquipmentData.CheckResults.UpdateCheckResult(EquipmentData.CheckResults.CheckNodeCurrent.ItemKey, list[i], arrayResult);
                    }

                }

                else if (menuTemp.Name == "setResoultOK")//手动合格
                {
                    string[] arrayResult = new string[EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count];
                    for (int k = 0; k < EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count; k++)
                    {
                        arrayResult[k] = ConstHelper.合格;
                    }
                    EquipmentData.CheckResults.UpdateCheckResult(EquipmentData.CheckResults.CheckNodeCurrent.ItemKey, "结论", arrayResult);
                }
                else if (menuTemp.Name == "setResoultNO")  //手动不合格
                {
                    string[] arrayResult = new string[EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count];
                    for (int k = 0; k < EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count; k++)
                    {
                        arrayResult[k] = ConstHelper.不合格;
                    }
                    EquipmentData.CheckResults.UpdateCheckResult(EquipmentData.CheckResults.CheckNodeCurrent.ItemKey, "结论", arrayResult);
                }
                else
                {
                    for (int i = 0; i < EquipmentData.CheckResults.Categories.Count; i++)
                    {
                        SetNodeExpanded(EquipmentData.CheckResults.Categories[i], menuTemp.Name == "menuExpand");
                    }
                }

            }
        }
        private void SetNodeExpanded(CheckNodeViewModel nodeTemp, bool isExpanded)
        {
            if (nodeTemp != null)
            {
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {

                    nodeTemp.IsExpanded = isExpanded;
                    for (int j = 0; j < nodeTemp.Children.Count; j++)
                    {
                        SetNodeExpanded(nodeTemp.Children[i], isExpanded);
                    }
                }
            }
        }

        /// <summary>
        /// 选中项发生改变事件
        /// </summary>
        private void TreeSchema1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            Dispatcher.Invoke(new Action(() =>
            {
                object obj = (sender as TreeView)?.SelectedItem;
                if (EquipmentData.Controller.IsChecking)
                {
                    if (obj is CheckNodeViewModel model)
                    {
                        if (model.CheckResults.Count > 0)
                        {
                            EquipmentData.CheckResults.CheckNodeCurrent = model;
                        }
                    }
                }
                else
                {
                    if (obj is CheckNodeViewModel model)
                    {
                        if (model.CheckResults.Count > 0)
                        {
                            EquipmentData.Controller.Index = EquipmentData.CheckResults.ResultCollection.IndexOf(model);
                        }
                    }
                }
            }));

        }
        // 更新当前方案
        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            buttonRefresh.IsEnabled = false;
            EquipmentData.SchemaModels.RefreshCurrrentSchema();
        }

        private void Columnscroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            treescroller.ScrollToVerticalOffset(e.VerticalOffset);
            //treeViewDecorator.p

            //meterscroller.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void OnTreeScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //treeDetail.ScrollToVerticalOffset(e.VerticalOffset);
        }
    }
}

