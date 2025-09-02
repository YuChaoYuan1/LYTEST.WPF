using LYTest.ViewModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using LYTest.ViewModel.CheckInfo;
using LYTest.Utility.Log;
using LYTest.WPF.SG.Converter;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_TestDetail.xaml 的交互逻辑
    /// </summary>
    public partial class View_TestDetail
    {
        private readonly string SetPath = System.IO.Directory.GetCurrentDirectory() + "\\Ini\\SetResoultConfig.ini";

        //bool isAllVisible = false;
        readonly List<string> visibleList = new List<string>();
        public View_TestDetail()
        {
            InitializeComponent();
            Name = "详细数据";
            GetSetResooltItemID();
            DataContext = EquipmentData.CheckResults;
            DockStyle.FloatingSize = SystemParameters.WorkArea.Size;
            EquipmentData.CheckResults.PropertyChanged += CheckResults_PropertyChanged;
            treeScheme.Loaded += TreeScheme_Loaded;

            comboBoxSchema.DataContext = EquipmentData.SchemaModels;
            Binding bindingRefresh = new Binding("IsChecking")
            {
                Source = EquipmentData.Controller,
                Converter = new NotBoolConverter()
            };
            buttonRefresh.SetBinding(IsEnabledProperty, bindingRefresh);
            comboBoxSchema.SetBinding(IsHitTestVisibleProperty, bindingRefresh);
        }
        /// <summary>
        /// 获取需要修改结论的项目id
        /// </summary>
        private void GetSetResooltItemID()
        {
            string str = Core.OperateFile.GetINI("visible", "all", SetPath).Trim();
            if (str.ToLower() == "0")
            {
                //isAllVisible = true;
                return;
            }
            for (int i = 0; i < 99; i++)
            {
                str = Core.OperateFile.GetINI("visible", i.ToString(), SetPath).Trim();
                if (str != "")
                {
                    visibleList.Add(str);
                }
            }

        }
        void TreeScheme_Loaded(object sender, RoutedEventArgs e)
        {
            if (EquipmentData.CheckResults.CheckNodeCurrent == null)
            {
                return;
            }
            if (EquipmentData.CheckResults.CheckNodeCurrent.Level == 1)
            {
                if (treeScheme.ItemContainerGenerator.ContainerFromItem(EquipmentData.CheckResults.CheckNodeCurrent.Parent) is TreeViewItem treeItem)
                {
                    treeItem.IsExpanded = true;
                }
            }
            ReloadColumn();
        }

        void CheckResults_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FlagLoadColumn" && !EquipmentData.CheckResults.FlagLoadColumn)
            {
                ReloadColumn();
            }
            if (e.PropertyName == "CheckNodeCurrent")
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    CheckNodeViewModel nodeSelected = treeScheme.SelectedItem as CheckNodeViewModel;
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
                        if (!(treeScheme.ItemContainerGenerator.ContainerFromItem(nodeList[nodeList.Count - 1]) is TreeViewItem treeItem))
                        {
                            return;
                        }
                        else
                        {
                            treeItem.IsExpanded = true;
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
        }

        /// <summary>
        /// 加载检定点列
        /// </summary>
        public void ReloadColumn()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    while (dataGridCheck.Columns.Count > 1)
                    {
                        BindingOperations.ClearAllBindings(dataGridCheck.Columns[1]);
                        dataGridCheck.Columns.Remove(dataGridCheck.Columns[1]);
                    }
                    List<string> columnNames = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults[0].GetAllProperyName();

                    MenuItem menuEdit = new MenuItem() { Header = "编辑" };
                    menuEdit.Click += MenuItem_Click;
                    MenuItem menuAnalyze = new MenuItem() { Header = "解析" };
                    menuAnalyze.Click += MenuItem_Click2;
                    MenuItem menuChart = new MenuItem() { Header = "查看曲线" };
                    menuChart.Click += MenuChart_Click;



                    setReoultMenu.Items.Clear();

                    //if (isAllVisible || visibleList.Contains(EquipmentData.CheckResults.CheckNodeCurrent.ParaNo))
                    //{
                    //}
                    //else 
                    if (EquipmentData.CheckResults.CheckNodeCurrent.ParaNo == Core.Enum.ProjectID.高次谐波)
                    {
                        setReoultMenu.Items.Add(menuEdit); // 编辑
                    }
                    else if (EquipmentData.CheckResults.CheckNodeCurrent.ParaNo == Core.Enum.ProjectID.自热试验)
                    {
                        setReoultMenu.Items.Add(menuChart); // 查看曲线
                    }
                    else
                    {
                        setReoultMenu.Items.Add(menuEdit);
                        setReoultMenu.Items.Add(menuAnalyze);
                    }

                    //else
                    //{
                    //    if (visibleList.Contains(EquipmentData.CheckResults.CheckNodeCurrent.ParaNo))
                    //    {
                    //        setReoultMenu.Visibility = Visibility.Visible;
                    //    }
                    //    if (EquipmentData.CheckResults.CheckNodeCurrent.ParaNo == Core.Enum.ProjectID.高次谐波)
                    //    {
                    //        setReoultMenu.Visibility = Visibility.Visible;
                    //        menuAnalyze.Visibility = Visibility.Collapsed; // 编辑
                    //    }
                    //    else if (EquipmentData.CheckResults.CheckNodeCurrent.ParaNo == Core.Enum.ProjectID.自热试验)
                    //    {
                    //        menuAnalyze.Visibility = Visibility.Collapsed;
                    //        this.menuAnalyze.Visibility = Visibility.Collapsed;

                    //    }
                    //    else
                    //    {
                    //        menuAnalyze.Visibility = Visibility.Visible;// 编辑
                    //    }
                    //}

                    double widthTemp = dataGridCheck.ActualWidth;
                    double columnWidth = 100;
                    if (columnNames.Count > 1)
                    {
                        columnWidth = (widthTemp - 100) / (columnNames.Count - 1);
                    }
                    if (columnWidth < 70)
                    {
                        columnWidth = 70;
                    }
                    for (int i = 0; i < columnNames.Count; i++)
                    {
                        if (columnNames[i] == "要检" || columnNames[i] == "项目名")
                        {
                            continue;
                        }
                        DataGridTextColumn column = new DataGridTextColumn
                        {
                            Header = columnNames[i],
                            Binding = new Binding(columnNames[i]),
                            Width = new DataGridLength(columnWidth),
                            MinWidth = 50
                        };
                        dataGridCheck.Columns.Add(column);
                    };
                }));
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(string.Format("控件加载异常:{0}", ex.Message), EnumLogSource.用户操作日志, EnumLevel.Warning, ex);
            }
        }

        // 查看曲线
        private void MenuChart_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridCheck.SelectedCells.Count <= 0) return;


            DataGridCellInfo cell = dataGridCheck.SelectedCells[0];
            DynamicViewModel vm = (DynamicViewModel)cell.Item;



            Windows.Window_SelfheatChart dlg = new Windows.Window_SelfheatChart
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Data = vm,
                Topmost = true
            };
            dlg.ShowDialog();
        }

        public sealed override void Dispose()
        {
            //清除绑定
            dataGridCheck.Columns.Clear();
            decorator1.Target = null;
            treeScheme.DataContext = null;
            treeScheme.SelectedItemChanged -= TreeScheme_SelectionChanged;
            EquipmentData.CheckResults.PropertyChanged -= CheckResults_PropertyChanged;
            DataContext = null;
            base.Dispose();
        }

        private void TreeScheme_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (EquipmentData.Controller.IsChecking)
                {
                    object obj = treeScheme.SelectedItem;
                    if (obj is CheckNodeViewModel vm)
                    {
                        if (vm.CheckResults.Count > 0)
                        {
                            EquipmentData.CheckResults.CheckNodeCurrent = vm;
                        }
                    }
                }
                else
                {
                    object obj = treeScheme.SelectedItem;
                    if (obj is CheckNodeViewModel vm)
                    {
                        if (vm.CheckResults.Count > 0)
                        {
                            EquipmentData.Controller.Index = EquipmentData.CheckResults.ResultCollection.IndexOf(vm);
                        }
                    }
                }
            }));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Windows.Window_TestDataSet dlg = new Windows.Window_TestDataSet();
            dlg.ShowDialog();
        }

        /// <summary>
        /// 展开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    //List<string> list = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults[0].GetAllProperyName();
                    string[] arrayResult = new string[EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count];
                    for (int k = 0; k < EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count; k++)
                    {
                        arrayResult[k] = "合格";
                    }
                    EquipmentData.CheckResults.UpdateCheckResult(EquipmentData.CheckResults.CheckNodeCurrent.ItemKey, "结论", arrayResult);
                }
                else if (menuTemp.Name == "setResoultNO")  //手动不合格
                {
                    //List<string> list = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults[0].GetAllProperyName();
                    string[] arrayResult = new string[EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count];
                    for (int k = 0; k < EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count; k++)
                    {
                        arrayResult[k] = "不合格";
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

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            EquipmentData.SchemaModels.RefreshCurrrentSchema();
        }

        private void MenuItem_Click2(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is MenuItem) ) return;
            Windows.Window_DataAnalysis dlg = new Windows.Window_DataAnalysis();
            dlg.ShowDialog();

        }
    }
}

