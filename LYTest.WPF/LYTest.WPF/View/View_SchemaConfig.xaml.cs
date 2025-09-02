using LYTest.Core.Enum;
using LYTest.Utility.Log;
using LYTest.ViewModel;
using LYTest.ViewModel.Schema;
using LYTest.ViewModel.Schema.Error;
using LYTest.WPF.Model;
using LYTest.WPF.View.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_SchemaConfig.xaml 的交互逻辑
    /// </summary>
    public partial class View_SchemaConfig
    {
        public View_SchemaConfig()
        {
            InitializeComponent();
            Name = "方案管理";
            DockStyle.IsMaximized = true;
            DockStyle.IsFloating = true; //是否开始是全屏  
            DockStyle.FloatingSize = new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            treeFramework.ItemsSource = FullTree.Instance.FilterAllCheck();
            gridSchemas.DataContext = EquipmentData.SchemaModels;//方案列表
            comboBoxSchemas.SelectionChanged += ComboBoxSchemas_SelectionChanged;//选中方案改变触发事件

            if (EquipmentData.SchemaModels.SelectedSchema != null)
            {
                comboBoxSchemas.SelectedItem = EquipmentData.SchemaModels.SelectedSchema;
                //viewModel.LoadSchema((int)EquipmentData.SchemaModels.SelectedSchema.GetProperty("ID"));
            }

            controlError.PointsChanged += ControlEror_PointsChanged;
            controlError.AllPoints.PropertyChanged += AllPoints_PropertyChanged;

            checkBoxErrorView.Checked += CheckBoxErrorView_Checked;
            checkBoxErrorView.Unchecked += CheckBoxErrorView_Checked;

        }

        private SchemaViewModel ViewModel
        {
            get { return Resources["SchemaViewModel"] as SchemaViewModel; }
        }

        private DynamicViewModel CurrentSchema
        {
            get { return comboBoxSchemas.SelectedItem as DynamicViewModel; }
        }

        void ControlEror_PointsChanged(object sender, System.EventArgs e)
        {
            ErrorModel model = sender as ErrorModel;
            if (model is ErrorModel)
            {
                ViewModel.UpdateErrorPoint(model);
            }
        }

        #region 用户事件
        //private void ButtonParaInfo_Click(object sender, RoutedEventArgs e)
        //{
        //    Button button = sender as Button;
        //    if (button == null) return;
        //    viewModel.ParaInfo.CommandFactoryMethod(button.Name);
        //}

        public override void Dispose()
        {
            controlError.PointsChanged -= ControlEror_PointsChanged;
            controlError.AllPoints.PropertyChanged -= AllPoints_PropertyChanged;
            checkBoxErrorView.Checked -= CheckBoxErrorView_Checked;
            checkBoxErrorView.Unchecked -= CheckBoxErrorView_Checked;
            comboBoxSchemas.SelectionChanged -= ComboBoxSchemas_SelectionChanged;
            dataGridGeneral.DataContext = null;
            dataGridGeneral.Columns.Clear();
            dataGridGeneral.ItemsSource = null;
            base.Dispose();
        }

        private void Button_Click_RemoveNode(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SchemaNodeViewModel node = btn.DataContext as SchemaNodeViewModel;
                if (node.Level == 1)
                {
                    ViewModel.Children.Remove(node);
                }
                else
                {
                    node.ParentNode.Children.Remove(node);
                }
                ViewModel.RefreshPointCount();
            }
        }

        //  object temCurrentNode2 = null;

        private void TreeSchema_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(treeSchema.SelectedItem is SchemaNodeViewModel currentNode))
            {
                checkBoxErrorView.Visibility = Visibility.Collapsed;
                gridGeneral.Visibility = Visibility.Collapsed;
                scrollViewError.Visibility = Visibility.Collapsed;
                ProtocolPanel.Visibility = Visibility.Collapsed;
                return;
            }

            // temCurrentNode2 =  treeSchema.SelectedItem;

            if (currentNode.Children.Count == 0)
            {
                //切换检定点时保存(不保存到数据库)
                if (ViewModel.SelectedNode != null)
                {
                    ViewModel.SelectedNode.ParaValuesCurrent.Clear();
                    ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
                }
                ViewModel.SelectedNode = currentNode;
                if (currentNode.ParaNo.Equals(ViewModel.ParaNo))
                {
                    ViewModel.ParaValuesConvert();
                }
                else
                    ViewModel.ParaNo = currentNode.ParaNo;
            }

            //12001:基本误差
            if (ViewModel.ParaNo == ProjectID.基本误差试验 || ViewModel.ParaNo == ProjectID.初始固有误差 || ViewModel.ParaNo == ProjectID.初始固有误差试验)
            {
                checkBoxErrorView.IsChecked = true;
                checkBoxErrorView.Visibility = Visibility.Visible;
                gridGeneral.Visibility = Visibility.Collapsed;
                scrollViewError.Visibility = Visibility.Visible;
                controlError.AllPoints.Load(ViewModel.SelectedNode.ParaValuesCurrent);
            }
            else
            {
                checkBoxErrorView.IsChecked = true;
                checkBoxErrorView.Visibility = Visibility.Collapsed;
                gridGeneral.Visibility = Visibility.Visible;
                scrollViewError.Visibility = Visibility.Collapsed;
            }
            if (ViewModel.ParaNo == ProjectID.通讯协议检查试验 || ViewModel.ParaNo == ProjectID.通讯协议检查试验2)
            {
                ProtocolPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ProtocolPanel.Visibility = Visibility.Collapsed;
            }

        }



        private void Button_Click_RemoveItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.DataContext is DynamicViewModel modelTemp)
                {
                    ViewModel.ParaValuesView.Remove(modelTemp);
                    if (ViewModel.ParaValuesView.Count == 0)
                    {
                        if (ViewModel.SelectedNode.Level == 1)
                        {
                            ViewModel.Children.Remove(ViewModel.SelectedNode);
                        }
                        else
                        {
                            ViewModel.SelectedNode.ParentNode.Children.Remove(ViewModel.SelectedNode);
                        }
                        ViewModel.RefreshPointCount();
                        return;
                    }
                    else
                    {
                        ViewModel.SelectedNode.ParaValuesCurrent.Clear();
                        ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
                        ViewModel.RefreshPointCount();
                    }
                }
            }
        }

        private void Button_Click_AddItem(object sender, RoutedEventArgs e)
        {
            ViewModel.AddNewParaValue();
            ViewModel.SelectedNode.ParaValuesCurrent.Clear();
            ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
            ViewModel.RefreshPointCount();
        }

        private void ButtonClick_AddNode(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is Button btn)) return;
            if (btn.Name != "buttonAdd") return;
            if (!(btn.DataContext is SchemaNodeViewModel nodeTemp)) return;
            List<SchemaNodeViewModel> nodeList = new List<SchemaNodeViewModel>();
            if (nodeTemp.IsTerminal)
            {
                nodeList.Add(nodeTemp);
            }
            else
            {
                nodeList = nodeTemp.GetTerminalNodes();
            }
            List<string> namesList = new List<string>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                string noTemp = nodeList[i].ParaNo;
                if (ViewModel.ExistNode(noTemp))
                {
                    namesList.Add(nodeList[i].Name);
                    continue;
                }
                SchemaNodeViewModel nodeNew = ViewModel.AddParaNode(noTemp);

                if (i == nodeList.Count - 1)
                {
                    SelectNode(nodeNew);
                }
            }
            if (namesList.Count > 0)
            {
                LogManager.AddMessage(string.Format("检定点:{0}已存在,将不会重复添加!", string.Join(",", namesList)), EnumLogSource.用户操作日志, EnumLevel.Tip);
            }
        }

        private void SelectNode(SchemaNodeViewModel nodeTemp)
        {
            if (nodeTemp != null)
            {
                List<SchemaNodeViewModel> nodesList = new List<SchemaNodeViewModel>();
                #region 获取链
                nodesList.Add(nodeTemp);
                SchemaNodeViewModel nodeParentTemp = nodeTemp.ParentNode;
                while (nodeParentTemp != null && nodeParentTemp.Level >= 1)
                {
                    if (nodeParentTemp != null)
                    {
                        nodesList.Add(nodeParentTemp);
                    }
                    nodeParentTemp = nodeParentTemp.ParentNode;
                    if (nodeParentTemp == null)
                    {
                        break;
                    }
                }
                #endregion

                nodesList = OrderByListChildren(nodesList);

                if (!(treeSchema.ItemContainerGenerator.ContainerFromItem(nodesList[nodesList.Count - 1]) is TreeViewItem treeItem))
                {
                    return;
                }
                else
                {
                    treeItem.IsExpanded = true;
                    treeItem.IsSelected = true;
                    treeItem.BringIntoView();
                }
                for (int i = nodesList.Count - 2; i >= 0; i--)
                {
                    treeItem = treeItem.ItemContainerGenerator.ContainerFromItem(nodesList[i]) as TreeViewItem;
                    if (treeItem == null)
                    {
                        return;
                    }
                    else
                    {
                        treeItem.IsExpanded = true;
                        treeItem.IsSelected = true;
                        treeItem.BringIntoView();
                    }
                }
            }
        }

        /// <summary>
        /// 方案排序
        /// </summary>
        /// <param name="ViewModel"></param>
        /// <returns></returns>
        public List<SchemaNodeViewModel> OrderByListChildren(List<SchemaNodeViewModel> ViewModel)
        {
            for (int i = 0; i < ViewModel.Count; i++)
            {
                if (ViewModel[i].ParaNo == null)
                {
                    ViewModel[i].ParaNo = DAL.SchemaFramework.GetSortNo(ViewModel[i].ParaNo);
                }
                for (int j = 0; j < ViewModel[i].Children.Count; j++)
                {
                    if (ViewModel[i].Children[j].SortNo == null)
                    {
                        ViewModel[i].Children[j].SortNo = DAL.SchemaFramework.GetSortNo(ViewModel[i].Children[j].ParaNo);
                    }
                }
            }



            for (int k = 0; k < ViewModel.Count; k++)
            {
                for (int i = ViewModel[k].Children.Count; i > 0; i--)
                {
                    for (int j = 0; j < i - 1; j++)
                    {
                        if (string.IsNullOrWhiteSpace(ViewModel[k].Children[j].SortNo) || string.IsNullOrWhiteSpace(ViewModel[k].Children[j + 1].SortNo))
                        {
                            continue;
                        }

                        SchemaNodeViewModel ViewModeTmp;
                        if (int.Parse(ViewModel[k].Children[j].SortNo) > int.Parse(ViewModel[k].Children[j + 1].SortNo))
                        {
                            ViewModeTmp = ViewModel[k].Children[j];
                            ViewModel[k].Children[j] = ViewModel[k].Children[j + 1];
                            ViewModel[k].Children[j + 1] = ViewModeTmp;
                        }
                        if (ViewModel[k].Children[j].Children.Count > 0)
                        {
                            for (int p = 0; p < ViewModel[k].Children[j].Children.Count - 1; p++)
                            {
                                if (int.Parse(ViewModel[k].Children[j].Children[p].SortNo) > int.Parse(ViewModel[k].Children[j].Children[p + 1].SortNo))
                                {
                                    ViewModeTmp = ViewModel[k].Children[j].Children[p];
                                    ViewModel[k].Children[j].Children[p] = ViewModel[k].Children[j].Children[p + 1];
                                    ViewModel[k].Children[j].Children[p + 1] = ViewModeTmp;
                                }
                            }
                        }

                    }
                }
            }

            //List<SchemaNodeViewModel> ViewModelList;

            return ViewModel;

        }

        private void ButtonClick_Save(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveParaValue();
        }


        //private void SchemaDown(object sender, RoutedEventArgs e)
        //{
        //    //model = new EquipmentData.Schema();
        //    //传入方案编号，及参数值，创建方案
        //    test();
        //    return;

        //    EquipmentData.SchemaModels.NewName = "创建方案名称测试";
        //    EquipmentData.SchemaModels.AddSchema(); //这个需要重写一个方法，刷新方案放在背后，并且把选中等方法添加到里面
        //    DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == EquipmentData.SchemaModels.NewName);
        //    EquipmentData.SchemaModels.SelectedSchema = modelTemp;
        //    System.Threading.Thread.Sleep(1000);
        //    if (!EquipmentData.Schema.ExistNode("18001"))
        //    {
        //        // SchemaNodeViewModel nodeNew = 
        //        EquipmentData.Schema.AddParaNode("18001");//根据方案的编号，添加进了节点
        //        EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
        //    }
        //    //SchemaNodeViewModel nodeNew =
        //    //EquipmentData.Schema.ParaValuesView.Clear();

        //    List<string> propertyNames = new List<string>();
        //    for (int i = 0; i < EquipmentData.Schema.ParaInfo.CheckParas.Count; i++)
        //    {
        //        propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[i].ParaDisplayName);
        //    }

        //    for (int j = 0; j < 2; j++)
        //    {
        //        DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
        //        viewModel2.SetProperty("IsSelected", true);

        //        for (int i = 0; i < propertyNames.Count; i++)
        //        {
        //            viewModel2.SetProperty(propertyNames[i], EquipmentData.Schema.ParaInfo.CheckParas[i].DefaultValue); //这里改成参数的值
        //        }
        //        viewModel2.SetProperty("功率因数", "0.5L");

        //        EquipmentData.Schema.ParaValuesView.Add(viewModel2);
        //    }
        //    viewModel.RefreshPointCount();
        //    EquipmentData.Schema.SaveParaValue();    //保存方案
        //    EquipmentData.SchemaModels.RefreshCurrrentSchema();

        //}


        //private void test()
        //{
        //    EquipmentData.SchemaModels.NewName = "创建方案名称测试";
        //    EquipmentData.SchemaModels.AddDownSchema(); //这个需要重写一个方法，刷新方案放在背后，并且把选中等方法添加到里面
        //    DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == EquipmentData.SchemaModels.NewName);
        //    EquipmentData.SchemaModels.SelectedSchema = modelTemp;
        //    //System.Threading.Thread.Sleep(1000);
        //    string[] keys = new string[] { "12001", "12002" };
        //    string[] str = new string[] { "正向有功", "正向无功", "反向有功", "反向无功" };
        //    foreach (var key in keys)
        //    {
        //        if (!EquipmentData.Schema.ExistNode(key))
        //        {
        //            SchemaNodeViewModel nodeNew = EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
        //            EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
        //        }
        //        List<string> propertyNames = new List<string>();

        //        for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
        //        {
        //            propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
        //        }

        //        for (int i = 0; i < 4; i++)
        //        {
        //            DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
        //            viewModel2.SetProperty("IsSelected", true);
        //            for (int j = 0; j < propertyNames.Count; j++)
        //            {
        //                viewModel2.SetProperty(propertyNames[j], EquipmentData.Schema.ParaInfo.CheckParas[j].DefaultValue); //这里改成参数的值
        //            }
        //            if (key == "12001")
        //            {
        //                viewModel2.SetProperty(propertyNames[1], str[i]); //这里改成参数的值
        //            }
        //            EquipmentData.Schema.ParaValuesView.Add(viewModel2);
        //        }
        //        EquipmentData.Schema.SaveParaValue();    //保存方案
        //        EquipmentData.Schema.RefreshPointCount();
        //    }

        //    EquipmentData.SchemaModels.RefreshCurrrentSchema();
        //}
        private void ButtonClick_SortDefault(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要默认排序吗?", "默认排序", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                ViewModel.SortDefault();
            }
        }

        private void Event_NodeMove(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(SchemaNodeViewModel)) is SchemaNodeViewModel nodeSource))
            { return; }
            Point pos = e.GetPosition(treeSchema);
            HitTestResult result = VisualTreeHelper.HitTest(treeSchema, pos);
            if (result == null)
                return;

            TreeViewItem selectedItem = Utils.FindVisualParent<TreeViewItem>(result.VisualHit);
            if (selectedItem == null)
            {
                return;
            }
            if (!(selectedItem.DataContext is SchemaNodeViewModel nodeDest))
            {
                return;
            }
            if (nodeDest == nodeSource)
            {
                return;
            }
            ViewModel.MoveNode(nodeSource, nodeDest);
        }

        private void Button_Click_ItemUp(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.DataContext is DynamicViewModel modelTemp))
            {
                return;
            }
            int index = ViewModel.ParaValuesView.IndexOf(modelTemp);
            if (index > 0)
            {
                ViewModel.ParaValuesView.Move(index, index - 1);
            }
            ViewModel.SelectedNode.ParaValuesCurrent.Clear();
            ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
        }

        private void Button_Click_ItemDown(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is DynamicViewModel modelTemp))
            {
                return;
            }
            int index = ViewModel.ParaValuesView.IndexOf(modelTemp);
            if (index < ViewModel.ParaValuesView.Count - 1)
            {
                ViewModel.ParaValuesView.Move(index, index + 1);
            }
            ViewModel.SelectedNode.ParaValuesCurrent.Clear();
            ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
        }

        private void CheckBoxErrorView_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ParaNo != ProjectID.基本误差试验 && ViewModel.ParaNo != ProjectID.初始固有误差 && ViewModel.ParaNo != ProjectID.初始固有误差试验)
            {
                return;
            }
            if (checkBoxErrorView.IsChecked.HasValue && checkBoxErrorView.IsChecked.Value)
            {
                gridGeneral.Visibility = Visibility.Collapsed;
                scrollViewError.Visibility = Visibility.Visible;
            }
            else
            {
                gridGeneral.Visibility = Visibility.Visible;
                scrollViewError.Visibility = Visibility.Collapsed;
            }
        }

        private void ComboBoxSchemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentSchema != null)
            {
                ViewModel.LoadSchema((int)CurrentSchema.GetProperty("ID"));
                //tipsSchemeName.Text = currentSchema.GetProperty("SCHEMA_NAME").ToString();
            }


        }
        #endregion

        private void Click_SchemaOperation(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn)
            {
                try
                {
                    switch (btn.Name)
                    {
                        case "buttonNew":
                            MainViewModel.Instance.CommandFactoryMethod("新建方案|View_SchemaOperation|新建方案");
                            break;
                        case "buttonDelete":
                            MainViewModel.Instance.CommandFactoryMethod("删除方案|View_SchemaOperation|删除方案");
                            break;
                        case "buttonRename":
                            MainViewModel.Instance.CommandFactoryMethod("重命名方案|View_SchemaOperation|重命名方案");
                            break;
                        case "buttonCopy":
                            MainViewModel.Instance.CommandFactoryMethod("复制方案|View_SchemaOperation|复制方案");
                            break;
                    }
                }
                catch
                { }
            }
        }


        private XmlNode nodeDataFlags = null;

        // 通信协议检查加载数据标识
        private void DataGridGeneral_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            if (ViewModel.ParaInfo != null && ViewModel.ParaInfo.ParaNo == "17001")
            {
                if (!(e.Column is DataGridComboBoxColumn column))
                {
                    return;
                }
                if (!(column.SelectedItemBinding is Binding bindingColumn))
                {
                    return;
                }
                string pathTemp = bindingColumn.Path.Path;
                if (pathTemp == "数据项名称")
                {
                    BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(ComboBox.SelectedItemProperty);
                    //加载数据标识内容
                    if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                    {
                        if (nodeDataFlags == null)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                            nodeDataFlags = doc.DocumentElement;
                        }
                        foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                        {
                            try
                            {
                                ComboBox comboBox = e.EditingElement as ComboBox;
                                if (nodeTemp.Attributes["DataFlagDiName"].Value == comboBox.SelectedItem?.ToString())
                                {
                                    modelTemp.SetProperty("标识编码", nodeTemp.Attributes["DataFlag"].Value);
                                    //modelTemp.SetProperty("标识编码698", nodeTemp.Attributes["DataFlag698"].Value);
                                    modelTemp.SetProperty("长度", nodeTemp.Attributes["DataLength"].Value);
                                    modelTemp.SetProperty("小数位", nodeTemp.Attributes["DataSmallNumber"].Value);
                                    modelTemp.SetProperty("数据格式", nodeTemp.Attributes["DataFormat"].Value);
                                    //modelTemp.SetProperty("写入内容", nodeTemp.Attributes["ReadData"].Value);
                                    //modelTemp.SetProperty("写入数据示例", nodeTemp.Attributes["Default"].Value);
                                    if (nodeTemp.Attributes["ReadData"] != null)
                                    {
                                        modelTemp.SetProperty("写入内容", nodeTemp.Attributes["ReadData"].Value);
                                    }
                                    return;
                                }
                            }
                            catch
                            { }
                        }
                    }
                }
            }
            else if (ViewModel.ParaInfo != null && ViewModel.ParaInfo.ParaNo == "17002") //载波
            {
                if (!(e.Column is DataGridComboBoxColumn column))
                    return;
                if (column.SelectedItemBinding is Binding bindingColumn)
                {
                    string pathTemp = bindingColumn.Path.Path;
                    if (pathTemp == "项目名称")
                    {
                        BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(ComboBox.SelectedItemProperty);
                        //加载数据标识内容
                        if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                        {
                            if (nodeDataFlags == null)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                                nodeDataFlags = doc.DocumentElement;
                            }
                            foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                            {
                                try
                                {
                                    ComboBox comboBox = e.EditingElement as ComboBox;
                                    if (nodeTemp.Attributes["DataFlagDiName"].Value == comboBox.SelectedItem.ToString())
                                    {
                                        modelTemp.SetProperty("标识符", nodeTemp.Attributes["DataFlag"].Value);
                                        return;
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }

            }
            else if (ViewModel.ParaInfo != null && ViewModel.ParaInfo.ParaNo == "17003")
            {
                if (!(e.Column is DataGridComboBoxColumn column))
                    return;
                if (column.SelectedItemBinding is Binding bindingColumn)
                {
                    string pathTemp = bindingColumn.Path.Path;
                    if (pathTemp == "数据项名称")
                    {
                        BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(ComboBox.SelectedItemProperty);
                        //加载数据标识内容
                        if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                        {
                            if (nodeDataFlags == null)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                                nodeDataFlags = doc.DocumentElement;
                            }


                            foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                            {
                                try
                                {
                                    ComboBox comboBox = e.EditingElement as ComboBox;
                                    if (comboBox.SelectedItem == null)
                                    {
                                        return;
                                    }
                                    if (nodeTemp.Attributes["DataFlagDiName"].Value == comboBox.SelectedItem.ToString())
                                    {

                                        //SchemaNodeViewModel currentNode = treeSchema.SelectedItem as SchemaNodeViewModel;
                                        //int Index = 1;
                                        //if (currentNode != null)
                                        //{
                                        //    foreach (var item in currentNode.ParaValuesCurrent)
                                        //    {
                                        //        if (item.GetProperty("PARA_VALUE").ToString().Split('|')[0] == comboBox.SelectedItem.ToString())
                                        //        {
                                        //            Index++;
                                        //        }
                                        //    }
                                        //}


                                        modelTemp.SetProperty("标识编码", nodeTemp.Attributes["DataFlag"].Value);
                                        modelTemp.SetProperty("标识编码698", nodeTemp.Attributes["DataFlag698"].Value);
                                        modelTemp.SetProperty("长度", nodeTemp.Attributes["DataLength"].Value);
                                        modelTemp.SetProperty("小数位", nodeTemp.Attributes["DataSmallNumber"].Value);
                                        modelTemp.SetProperty("数据格式", nodeTemp.Attributes["DataFormat"].Value);
                                        if (nodeTemp.Attributes["ReadData"] != null)
                                        {
                                            modelTemp.SetProperty("写入内容", nodeTemp.Attributes["ReadData"].Value);
                                        }
                                        //modelTemp.SetProperty("检定编号", Index.ToString());

                                        //modelTemp.SetProperty("写入数据示例", nodeTemp.Attributes["Default"].Value);
                                        return;
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
            }
            else if (ViewModel.ParaInfo != null && (ViewModel.ParaInfo.ParaNo == "19003" || ViewModel.ParaInfo.ParaNo == "19002"))
            {
                if (!(e.Column is DataGridComboBoxColumn column))
                {
                    return;
                }
                if (column.SelectedItemBinding is Binding bindingColumn)
                {
                    string pathTemp = bindingColumn.Path.Path;
                    if (pathTemp == "项目名称")
                    {
                        BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(ComboBox.SelectedItemProperty);
                        //加载数据标识内容
                        if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                        {
                            if (nodeDataFlags == null)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                                nodeDataFlags = doc.DocumentElement;
                            }


                            foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                            {
                                try
                                {
                                    ComboBox comboBox = e.EditingElement as ComboBox;
                                    if (nodeTemp.Attributes["DataFlagDiName"].Value == comboBox.SelectedItem.ToString())
                                    {
                                        modelTemp.SetProperty("数据标识", nodeTemp.Attributes["DataFlag"].Value);
                                        return;
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
            }

            else if (ViewModel.ParaInfo != null && ViewModel.ParaInfo.ParaNo == "21007")
            {
                if (!(e.Column is DataGridComboBoxColumn column))
                {
                    return;
                }
                if (column.SelectedItemBinding is Binding bindingColumn)
                {
                    string pathTemp = bindingColumn.Path.Path;
                    if (pathTemp == "数据项名称")
                    {
                        BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(ComboBox.SelectedItemProperty);
                        //加载数据标识内容
                        if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                        {
                            if (nodeDataFlags == null)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                                nodeDataFlags = doc.DocumentElement;
                            }
                            foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                            {
                                try
                                {
                                    ComboBox comboBox = e.EditingElement as ComboBox;
                                    if (comboBox.SelectedItem == null)
                                    {
                                        return;
                                    }
                                    if (nodeTemp.Attributes["DataFlagDiName"].Value == comboBox.SelectedItem.ToString())
                                    {
                                        modelTemp.SetProperty("标识编码645", nodeTemp.Attributes["DataFlag"].Value);
                                        modelTemp.SetProperty("标识编码698", nodeTemp.Attributes["DataFlag698"].Value);
                                        modelTemp.SetProperty("长度", nodeTemp.Attributes["DataLength"].Value);
                                        modelTemp.SetProperty("小数位", nodeTemp.Attributes["DataSmallNumber"].Value);
                                        modelTemp.SetProperty("数据格式", nodeTemp.Attributes["DataFormat"].Value);
                                        return;
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
            }

            else if (ViewModel.ParaInfo != null && ViewModel.ParaInfo.ParaNo == "24001")
            {
                if (!(e.Column is DataGridComboBoxColumn column))
                    return;

                if (column.SelectedItemBinding is Binding bindingColumn)
                {
                    string pathTemp = bindingColumn.Path.Path;
                    if (pathTemp == "数据项名称")
                    {
                        BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(ComboBox.SelectedItemProperty);
                        //加载数据标识内容
                        if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                        {
                            if (nodeDataFlags == null)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                                nodeDataFlags = doc.DocumentElement;
                            }
                            foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                            {
                                try
                                {
                                    ComboBox comboBox = e.EditingElement as ComboBox;
                                    if (comboBox.SelectedItem == null)
                                    {
                                        return;
                                    }
                                    if (nodeTemp.Attributes["DataFlagDiName"].Value == comboBox.SelectedItem.ToString())
                                    {
                                        modelTemp.SetProperty("标识符", nodeTemp.Attributes["DataFlag"].Value);

                                        //modelTemp.SetProperty("标识编码645", nodeTemp.Attributes["DataFlag"].Value);
                                        //modelTemp.SetProperty("标识编码698", nodeTemp.Attributes["DataFlag698"].Value);
                                        return;
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
            }
        }



        private void AllPoints_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModel.SelectedNode != null && (ViewModel.SelectedNode.ParaNo == ProjectID.基本误差试验))
            {
                ViewModel.Model.AsyncObservableCollection<DynamicViewModel> viewModelsTemp = ViewModel.ParaValuesView;
                foreach (DynamicViewModel modelTemp in viewModelsTemp)
                {
                    if (e.PropertyName == "LapCountIb")     //相对于Ib圈数
                    {
                        try
                        {
                            ErrorCategory catg = controlError.AllPoints.Categories.First(c =>
                                     c.ErrorType.Equals(modelTemp.GetProperty("误差试验类型") as string)
                                  && c.Fangxiang.Equals(modelTemp.GetProperty("功率方向") as string)
                                  && c.Component.Equals(modelTemp.GetProperty("功率元件") as string)
                            );
                            ErrorModel errorModel = catg.ErrorPoints.First(v =>
                                 v.Component.Equals(modelTemp.GetProperty("功率元件") as string)
                              && v.Factor.Equals(modelTemp.GetProperty("功率因数") as string)
                              && v.Current.Equals(modelTemp.GetProperty("电流倍数") as string)
                            );
                            modelTemp.SetProperty("误差圈数(Ib)", errorModel.LapCountIb);
                        }
                        catch
                        {
                            modelTemp.SetProperty("误差圈数(Ib)", controlError.AllPoints.LapCountIb);
                        }
                    }
                    else if (e.PropertyName == "GuichengMulti")      //规程误差限倍数
                    {
                        modelTemp.SetProperty("误差限倍数(%)", controlError.AllPoints.GuichengMulti);
                    }
                }
            }
        }

        private void ComboBoxSchemas_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(comboBoxSchemas.Items[0].ToString());
        }

        /// <summary>
        /// 添加通讯协议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AddProtocol(object sender, RoutedEventArgs e)
        {

            if (Protocol_Name.Text.Trim() != "") //
            {
                if (nodeDataFlags == null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
                    nodeDataFlags = doc.DocumentElement;
                }
                foreach (XmlNode nodeTemp in nodeDataFlags.ChildNodes)
                {
                    try
                    {
                        if (nodeTemp.Attributes["DataFlagDiName"].Value == Protocol_Name.Text.Trim())
                        {
                            ViewModel.AddNewParaValue();
                            ViewModel.SelectedNode.ParaValuesCurrent.Clear();
                            ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
                            ViewModel.RefreshPointCount();

                            int Index = 1;
                            if (treeSchema.SelectedItem is SchemaNodeViewModel currentNode)
                            {
                                foreach (var item in currentNode.ParaValuesCurrent)
                                {
                                    if (item.GetProperty("PARA_VALUE").ToString().Split('|')[0] == Protocol_Name.Text.Trim())
                                    {
                                        Index++;
                                    }
                                }
                            }
                            DynamicViewModel modelTemp = ViewModel.ParaValuesView[ViewModel.ParaValuesView.Count - 1];
                            modelTemp.SetProperty("数据项名称", nodeTemp.Attributes["DataFlagDiName"].Value);
                            modelTemp.SetProperty("标识编码", nodeTemp.Attributes["DataFlag"].Value);
                            if (ViewModel.ParaInfo.ParaNo == "17003")
                            {
                                modelTemp.SetProperty("标识编码698", nodeTemp.Attributes["DataFlag698"].Value);

                            }
                            modelTemp.SetProperty("长度", nodeTemp.Attributes["DataLength"].Value);
                            modelTemp.SetProperty("小数位", nodeTemp.Attributes["DataSmallNumber"].Value);
                            modelTemp.SetProperty("数据格式", nodeTemp.Attributes["DataFormat"].Value);
                            if (nodeTemp.Attributes["ReadData"] != null)
                            {
                                modelTemp.SetProperty("写入内容", nodeTemp.Attributes["ReadData"].Value);
                            }
                            modelTemp.SetProperty("检定编号", Index.ToString());
                            return;
                        }
                    }
                    catch
                    { }
                }
            }
        }


        private void Btn_RemoveCF(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < ViewModel.ParaValuesView.Count; i++)
            {
                DynamicViewModel modelTemp = ViewModel.ParaValuesView[i];
                string s1 = modelTemp.GetProperty("功率方向").ToString();
                string s2 = modelTemp.GetProperty("功率元件").ToString();
                string s3 = modelTemp.GetProperty("功率因数").ToString();
                string s4 = modelTemp.GetProperty("电流倍数").ToString();
                for (int j = i + 1; j < ViewModel.ParaValuesView.Count; j++)
                {
                    DynamicViewModel item = ViewModel.ParaValuesView[j];
                    string q1 = item.GetProperty("功率方向").ToString();
                    string q2 = item.GetProperty("功率元件").ToString();
                    string q3 = item.GetProperty("功率因数").ToString();
                    string q4 = item.GetProperty("电流倍数").ToString();

                    if (item != null)
                    {
                        if (s1 == q1 && s2 == q2 && s3 == q3 && s4 == q4)
                        {
                            ViewModel.ParaValuesView.Remove(modelTemp);
                            i--;
                            break;
                        }
                    }
                }
            }
            //viewModel.AddNewParaValue();
            //viewModel.SelectedNode.ParaValuesCurrent = viewModel.ParaValuesConvertBack();
            //viewModel.RefreshPointCount();
            //for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
            //{
            //    DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
            //    viewModel2.SetProperty("IsSelected", true);
            //    string[] value = Schema[key].SchemaNodeValue[i].Split('|');
            //    for (int j = 0; j < propertyNames.Count; j++)
            //    {
            //        viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
            //    }
            //    EquipmentData.Schema.ParaValuesView.Add(viewModel2);
            //}
            //errormodel = new ErrorModel
            //{
            //    Current = current,
            //    Factor = factor,
            //    FangXiang = category.Fangxiang,
            //    Component = category.Component,
            //    GuichengMulti = category.GuichengMulti,
            //    LapCountIb = category.LapCountIb
            //};
            //category.OnPointsChanged(errormodel);
            ViewModel.SelectedNode.ParaValuesCurrent.Clear();
            ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
            ViewModel.RefreshPointCount();

        }

        private void Click_SchemaExpand(object sender, RoutedEventArgs e)
        {
            Window_ShemaSet shemaSet = new Window_ShemaSet();
            shemaSet.ShowDialog();
        }

        private void Btn_BatchAddProtocol(object sender, RoutedEventArgs e)
        {
            //当前有的项目了
            Windows.Window_ProtocolAdd window_Protocol = new Windows.Window_ProtocolAdd(ViewModel.SelectedNode.ParaValuesCurrent);
            if (window_Protocol.ShowDialog() == true)
            {
                ViewModel.ParaValuesView.Clear();
                foreach (var item in window_Protocol.selectItems)
                {
                    ViewModel.AddNewParaValue();
                    ViewModel.SelectedNode.ParaValuesCurrent.Clear();
                    ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
                    ViewModel.RefreshPointCount();
                    DynamicViewModel modelTemp = ViewModel.ParaValuesView[ViewModel.ParaValuesView.Count - 1];
                    modelTemp.SetProperty("数据项名称", item.Name);
                    modelTemp.SetProperty("标识编码", item.DataFlag);
                    if (ViewModel.ParaInfo.ParaNo == "17003")
                    {
                        modelTemp.SetProperty("标识编码698", item.DataFlag698);
                    }
                    modelTemp.SetProperty("长度", item.Length);
                    modelTemp.SetProperty("小数位", item.DotLength);
                    modelTemp.SetProperty("数据格式", item.DataFormat);
                    modelTemp.SetProperty("功能", item.Function);
                    modelTemp.SetProperty("标准数据", item.ReadData);
                }
                ViewModel.SelectedNode.ParaValuesCurrent.Clear();
                ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
            }
        }

        //add yjt zxg 20220202 合并张工导入其他数据库方案
        /// <summary>
        /// 导入方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClick_Import(object sender, RoutedEventArgs e)
        {
            //先选择文件
            using (System.Windows.Forms.OpenFileDialog f = new System.Windows.Forms.OpenFileDialog())
            {
                f.Filter = "数据库文件|*.mdb";
                f.Title = "请选择导入的方案文件";
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //f.FileName
                    EquipmentData.SchemaModels.GetImportShema(f.FileName);
                    //弹出对话框，获取方案的列表
                    //然后选择需要复制的方案
                    MainViewModel.Instance.CommandFactoryMethod("导入方案|View_SchemaOperation|导入方案");
                }
            }
        }

        /// <summary>
        /// 插入一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_InsertRow(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.DataContext is DynamicViewModel modelTemp)) return;
                var Index = ViewModel.ParaValuesView.IndexOf(modelTemp) + 1;
                if (Index > 0)
                {
                    ViewModel.InsertNewParaValue(Index);
                    ViewModel.SelectedNode.ParaValuesCurrent.Clear();
                    ViewModel.SelectedNode.ParaValuesCurrent.AddRange(ViewModel.ParaValuesConvertBack());
                    ViewModel.RefreshPointCount();
                }
            }
        }
        private byte optionId = 0;
        // 全检
        private void AllCheck_Click(object sender, RoutedEventArgs e)
        {
            if (optionId == 1) return;
            optionId = 1;
            //treeFramework.ItemsSource = null;
            treeFramework.ItemsSource = FullTree.Instance.FilterAllCheck();
        }
        // 抽检
        private void SampleCheck_Click(object sender, RoutedEventArgs e)
        {
            if (optionId == 3) return;
            optionId = 3;
            //treeFramework.ItemsSource = null;
            treeFramework.ItemsSource = FullTree.Instance.FilterSampleCheck();
        }
        // 全性能
        private void FullCheck_Click(object sender, RoutedEventArgs e)
        {
            if (optionId == 7) return;
            optionId = 7;
            //treeFramework.ItemsSource = null;
            treeFramework.ItemsSource = FullTree.Instance.FilterFullCheck();
        }

        private void JJGPlan_Click(object sender, RoutedEventArgs e)
        {
            Window_SetPlanPoint dlg = new Window_SetPlanPoint
            {
                Top = 65
            };
            dlg.Left = (this.ActualWidth - dlg.Width) * 0.5;
            if (dlg.ShowDialog() == true)
            {
                ViewModel.SetJJGPlanPoint(dlg.JJGParams);
            }
        }

        private void DataGridGeneral_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (ViewModel.ParaInfo != null
                && (ViewModel.ParaInfo.ParaNo == "17001"
                 || ViewModel.ParaInfo.ParaNo == "15011"
                 || ViewModel.ParaInfo.ParaNo == "15012"
                 || ViewModel.ParaInfo.ParaNo == "15013"
                 || ViewModel.ParaInfo.ParaNo == "15019"
                 || ViewModel.ParaInfo.ParaNo == "15027"
                 || ViewModel.ParaInfo.ParaNo == "12016"
                ))
            {
                if (!(e.Column is DataGridTextColumn column) || !(e.EditingElement is TextBox editingElement))
                {
                    return;
                }
                //bindingColumn.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                if (column.Binding is Binding bindingColumn)
                {
                    string pathTemp = bindingColumn.Path.Path;
                    if (ViewModel.ParaInfo.ParaNo == "17001")
                    {
                        if (pathTemp == "标准数据")
                        {
                            BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(TextBox.TextProperty);
                            if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                            {
                                string itemName = modelTemp.GetProperty("数据项名称") as string;
                                if (!string.IsNullOrWhiteSpace(itemName) && (itemName.IndexOf("第一套") >= 0 || itemName.IndexOf("第二套") >= 0) && itemName.IndexOf("日时段数据") > 0)
                                {
                                    Point buttonPosition = editingElement.PointToScreen(new Point(0, 0));
                                    // 设置窗体的位置
                                    double windowLeft = buttonPosition.X - editingElement.ActualWidth - Window_RatePeriodSelect.Instance.Width - 150;
                                    double windowTop = buttonPosition.Y;
                                    //计算屏幕缩放比例
                                    System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                                    var ratio = (int)(graphics.DpiX * 1.041666667);
                                    windowLeft = windowLeft / (ratio / 100f) + 2;
                                    windowTop /= ratio / 100f;
                                    // 调整窗体位置，避免超出屏幕范围
                                    double screenWidth = SystemParameters.PrimaryScreenWidth;
                                    double screenHeight = SystemParameters.PrimaryScreenHeight;
                                    if (windowLeft + Window_RatePeriodSelect.Instance.Width > screenWidth)
                                    {
                                        windowLeft = screenWidth - Window_RatePeriodSelect.Instance.Width;
                                    }

                                    if (windowTop + Window_RatePeriodSelect.Instance.Height > screenHeight)
                                    {
                                        windowTop = screenHeight - Window_RatePeriodSelect.Instance.Height;

                                    }
                                    modelTemp.SetProperty("标准数据", editingElement.Text);
                                    Window_RatePeriodSelect.Instance.ShowRatePeriod(windowLeft, windowTop, modelTemp, "标准数据", Window_RatePeriodSelect.FormatN.F1);
                                }

                            }
                        }
                    }
                    else if ((ViewModel.ParaInfo.ParaNo == "15011"
                 || ViewModel.ParaInfo.ParaNo == "15012"
                 || ViewModel.ParaInfo.ParaNo == "15013"
                 || ViewModel.ParaInfo.ParaNo == "15019"
                 || ViewModel.ParaInfo.ParaNo == "15027"
                 || ViewModel.ParaInfo.ParaNo == "12016")
                 && !string.IsNullOrWhiteSpace(pathTemp)
                 && pathTemp.IndexOf("费率") >= 0 && pathTemp.IndexOf("段") > 0
                 )
                    {
                        BindingExpression expressionTemp = e.EditingElement.GetBindingExpression(TextBox.TextProperty);
                        if (expressionTemp.DataItem is DynamicViewModel modelTemp)
                        {
                            Point buttonPosition = editingElement.PointToScreen(new Point(0, 0));
                            // 设置窗体的位置
                            double windowLeft = buttonPosition.X - editingElement.ActualWidth - Window_RatePeriodSelect.Instance.Width + 300;
                            double windowTop = buttonPosition.Y;
                            //计算屏幕缩放比例
                            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                            var ratio = (int)(graphics.DpiX * 1.041666667);
                            windowLeft = windowLeft / (ratio / 100f) + 2;
                            windowTop /= (ratio / 100f);
                            // 调整窗体位置，避免超出屏幕范围
                            double screenWidth = SystemParameters.PrimaryScreenWidth;
                            double screenHeight = SystemParameters.PrimaryScreenHeight;
                            if (windowLeft + Window_RatePeriodSelect.Instance.Width > screenWidth)
                            {
                                windowLeft = screenWidth - Window_RatePeriodSelect.Instance.Width;
                            }

                            if (windowTop + Window_RatePeriodSelect.Instance.Height > screenHeight)
                            {
                                windowTop = screenHeight - Window_RatePeriodSelect.Instance.Height;

                            }
                            modelTemp.SetProperty(pathTemp, editingElement.Text);
                            Window_RatePeriodSelect.Instance.ShowRatePeriod(windowLeft, windowTop, modelTemp, pathTemp, Window_RatePeriodSelect.FormatN.F2);
                        }
                    }
                }
            }

        }
    }
}

