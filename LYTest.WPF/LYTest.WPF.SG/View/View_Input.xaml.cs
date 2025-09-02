using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.AccessControl;
using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.InputPara;
using LYTest.WPF.SG.Controls;
using LYTest.WPF.SG.Converter;
using LYTest.WPF.SG.Model;
using LYTest.WPF.SG.UiGeneral;
using LYTest.WPF.SG.View.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_Input.xaml 的交互逻辑
    /// </summary>
    public partial class View_Input
    {
        public View_Input()
        {
            InitializeComponent();
            Name = "参数录入";

            DockStyle.IsMaximized = true;

            DockStyle.IsFloating = true;
            DockStyle.FloatingSize = SystemParameters.WorkArea.Size;
            for (int i = 0; i < ViewModel.ParasModel.AllUnits.Count; i++)
            {
                if (ViewModel.ParasModel.AllUnits[i].IsSame && ViewModel.ParasModel.AllUnits[i].IsNecessary)
                {
                    AddBasicPara(ViewModel.ParasModel.AllUnits[i]);
                }
            }
            comboBoxSchema.DataContext = EquipmentData.SchemaModels;
            GenerateColumns();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            string ckqi = Core.OperateFile.GetINI("Config", "checkBoxQuickInput", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini").Trim();
            if (ckqi.ToLower() == "true")
            {
                checkBoxQuickInput.IsChecked = true;
            }
            string ckdl = Core.OperateFile.GetINI("Config", "checkBoxDownload", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini").Trim();
            if (ckdl.ToLower() == "true")
            {
                checkBoxDownload.IsChecked = true;
            }

            if (CheckBoxUseJJGPoints.Visibility == Visibility.Visible)
            {
                string ckusejjg = Core.OperateFile.GetINI("Config", "CheckBoxUseJJGPoints", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini").Trim();
                if (ckusejjg.ToLower() == "true")
                {
                    CheckBoxUseJJGPoints.IsChecked = true;
                }
            }

            //加载是否从电表读取表号等级常数
            RadioBtnSoure(EquipmentData.IsReadMeterInfo);

            if (ChkColumnJump.Visibility == Visibility.Visible)
            {
                string jumpCount = Core.OperateFile.GetINI("Config", "TxtColJumpCount", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini").Trim();
                if (string.IsNullOrWhiteSpace(jumpCount))
                    TxtColJumpCount.Text = "0";
                else
                    TxtColJumpCount.Text = jumpCount;

                string coljump = Core.OperateFile.GetINI("Config", "ChkColumnJump", System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini").Trim();
                if (coljump.ToLower() == "true")
                {
                    ChkColumnJump.IsChecked = true;
                }
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InputParameterChanged")
            {
                Window_Wait.Instance.StateWait("检测到检定参数变化，正在处理检定数据...", 25000);
            }
            else if (e.PropertyName == "ParameterHandled")
            {
                Window_Wait.Instance.EndWait();
            }
        }

        private MeterInputParaViewModel ViewModel
        {
            get { return Resources["MeterInputParaViewModel"] as MeterInputParaViewModel; }
        }
        private void AddBasicPara(InputParaUnit paraUnit)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(5, 3, 5, 3),
                Orientation = Orientation.Horizontal,
            };
            TextBlock textBlock = new TextBlock
            {
                Text = paraUnit.DisplayName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 50
            };
            ControlEnumComboBox comboBox;
            if (paraUnit.CodeType == "CurrentValue" && paraUnit.FieldName == "MD_UA")
            {
                Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(paraUnit.CodeType);
                List<string> dataSource = dictionary.Keys.ToList();
                dataSource.Add("添加新项");
                comboBox = new ControlEnumComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    //Style = Application.Current.Resources["StyleComboBox"] as Style,
                    MinWidth = 100,
                    Tag = paraUnit.FieldName,
                    //FontSize = 16,
                    ItemsSource = dataSource
                };
            }
            else if (paraUnit.CodeType == "JJGC" && paraUnit.FieldName == "MD_JJGC")
            {
                Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(paraUnit.CodeType);
                var dataSource = dictionary.Keys.ToList().Where(a => RightsJJGC.JJGC.Contains(a));

                comboBox = new ControlEnumComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    //Style = Application.Current.Resources["StyleComboBox"] as Style,
                    MinWidth = 100,
                    Tag = paraUnit.FieldName,
                    //FontSize = 16,
                    ItemsSource = dataSource
                };
            }
            else if (paraUnit.CodeType == "TestMeterSort" && paraUnit.FieldName == "MD_SORT")
            {
                Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(paraUnit.CodeType);
                var dataSource = dictionary.Keys.ToList().Where(a => RightsIoT.MeterSort.Contains(a));

                comboBox = new ControlEnumComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    //Style = Application.Current.Resources["StyleComboBox"] as Style,
                    MinWidth = 100,
                    Tag = paraUnit.FieldName,
                    //FontSize = 16,
                    ItemsSource = dataSource
                };
            }
            else
            {
                comboBox = new ControlEnumComboBox()
                {
                    EnumName = paraUnit.CodeType,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    //Style = Application.Current.Resources["StyleComboBox"] as Style,
                    MinWidth = 100,
                    Tag = paraUnit.FieldName,
                    //FontSize = 16
                };
            }

            comboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding(string.Format("FirstMeter.{0}", paraUnit.FieldName)) { Mode = BindingMode.TwoWay });
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(comboBox);
            wrapPanelParas.Children.Add(stackPanel);
            comboBox.SelectionChanged += ComboBox_SelectionChanged;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string fieldName = comboBox.Tag as string;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    if (comboBox.SelectedItem as string == "添加新项")
                    {
                        #region 添加新的编码
                        BindingExpression expression = comboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                        if (expression == null)
                        {
                            return;
                        }
                        //解析编码路径
                        string strPath = expression.ParentBinding.Path.Path.Replace("FirstMeter.", "");
                        //如果没有创建新的值,就恢复原来的值
                        string oldValue = (expression.DataItem as MeterInputParaViewModel)?.FirstMeter.GetProperty(strPath) as string;
                        InputParaUnit unitTemp = ViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.FieldName == strPath);
                        if (unitTemp != null)
                        {
                            //获取节点
                            CodeTreeNode nodeTemp = CodeTreeViewModel.Instance.GetCodeByEnName(unitTemp.CodeType, 2);
                            if (nodeTemp != null)
                            {
                                Window_AddNewCode windowTemp = new Window_AddNewCode(nodeTemp);
                                bool? boolTemp = windowTemp.ShowDialog();
                                //if (boolTemp.HasValue && boolTemp.Value)//取消也刷新
                                {
                                    if (comboBox != null && nodeTemp.Children.Count > 0)
                                    {
                                        Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(nodeTemp.CODE_TYPE);
                                        List<string> dataSource = dictionary.Keys.ToList();
                                        dataSource.Add("添加新项");
                                        comboBox.ItemsSource = dataSource;
                                        comboBox.SelectedItem = nodeTemp.Children[nodeTemp.Children.Count - 1].CODE_NAME;
                                        return;
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    if (comboBox.SelectedItem as string == "添加新项") return;

                    for (int i = 0; i < ViewModel.Meters.Count; i++)
                    {
                        ViewModel.Meters[i].SetProperty(fieldName, comboBox.SelectedItem);
                    }
                }
            }
        }



        public bool IsAllSelected
        {
            get { return (bool)GetValue(IsAllSelectedProperty); }
            set { SetValue(IsAllSelectedProperty, value); }
        }

        // 使用DependencyProperty作为IsAllSelected的备份存储。这将启用动画、样式设置、绑定等。。
        public static readonly DependencyProperty IsAllSelectedProperty =
            DependencyProperty.Register("IsAllSelected", typeof(bool), typeof(View_Input), new PropertyMetadata(false));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "IsAllSelected")
            {
                for (int i = 0; i < ViewModel.Meters.Count; i++)
                {
                    ViewModel.Meters[i].SetProperty("MD_CHECKED", IsAllSelected ? "1" : "0");
                }
            }
            base.OnPropertyChanged(e);
        }


        /// <summary>
        /// 生成表格列
        /// </summary>
        private void GenerateColumns()
        {
            #region 要检
            CheckBox checkbox = new CheckBox
            {
                Content = "表位",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "表要检标记,全选"
            };
            Binding binding = new Binding("IsAllSelected")
            {
                Source = this
            };
            checkbox.SetBinding(ToggleButton.IsCheckedProperty, binding);
            DataGridColumn columnYaojian = Resources["KeyYaojianColumn"] as DataGridColumn;
            columnYaojian.Header = checkbox;
            dgv_MeterData.Columns.Add(columnYaojian);

            #endregion
            for (int i = 0; i < ViewModel.ParasModel.AllUnits.Count; i++)
            {
                InputParaUnit paraUnit = ViewModel.ParasModel.AllUnits[i];
                if (paraUnit.IsDisplayMember && (!paraUnit.IsSame) && (paraUnit.FieldName != "MD_CHECKED"))
                {
                    DataGridColumn column;
                    if (paraUnit.FieldName == "MD_CONSTANT" || paraUnit.FieldName == "MD_GRADE")
                    {
                        if (ConfigHelper.Instance.InputUIShowReactive)
                        {
                            column = ControlFactory.CreateColumn(paraUnit.DisplayName, paraUnit.CodeType, paraUnit.FieldName, paraUnit.ValueType, true, new AbracketBConverter(), "A");
                            dgv_MeterData.Columns.Add(column);
                            column = ControlFactory.CreateColumn("无功" + paraUnit.DisplayName, paraUnit.CodeType, paraUnit.FieldName, paraUnit.ValueType, true, new AbracketBConverter(), "B");
                            dgv_MeterData.Columns.Add(column);
                        }
                        else
                        {
                            column = ControlFactory.CreateColumn(paraUnit.DisplayName, paraUnit.CodeType, paraUnit.FieldName, paraUnit.ValueType, true);
                            dgv_MeterData.Columns.Add(column);
                        }
                    }
                    else
                    {
                        column = ControlFactory.CreateColumn(paraUnit.DisplayName, paraUnit.CodeType, paraUnit.FieldName, paraUnit.ValueType, true);

                        if (paraUnit.FieldName == "MD_BAR_CODE")
                        {
                            column.Width = 188;
                            //column.CellStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty,)
                        }
                        else if (paraUnit.FieldName == "MD_POSTAL_ADDRESS")
                        {
                            //column.IsReadOnly = true;
                            column.Width = 100;
                        }
                        else if (paraUnit.FieldName == "MD_EPITOPE")
                        {
                            column.IsReadOnly = true;
                            column.Width = 40;
                        }
                        else if (paraUnit.FieldName == "MD_DATERECEIVED")
                        {
                            column.Width = 130;
                        }
                        else
                        {
                            column.MinWidth = 40;
                            column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                        }
                        dgv_MeterData.Columns.Add(column);
                    }
                }
            }
        }


        #region 单元格
        public DataGridCell GetCell(DataGrid grid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(grid, row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    grid.ScrollIntoView(rowContainer, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;

        }
        /// <summary>
        /// 获取行索引取的行对象
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataGridRow GetRow(DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;

        }

        private T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default;
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual visual = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = visual as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(visual);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        #endregion
        private void Dgv_MeterData_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //跳转行
            if (e.EditAction == DataGridEditAction.Commit)
            {
                bool columnJump = ChkColumnJump.IsChecked == true;
                int.TryParse(TxtColJumpCount.Text, out int jumpCount);
                if (e.Column is DataGridBoundColumn colunm)
                {
                    var bindingPath = (colunm.Binding as Binding).Path.Path;
                    if (bindingPath == "MD_BAR_CODE")
                    {
                        int rowIndex = e.Row.GetIndex();
                        if (dgv_MeterData.CurrentCell.Column == null) return;
                        int colunIndex = dgv_MeterData.CurrentCell.Column.DisplayIndex;
                        string MD_BAR_CODE = (dgv_MeterData.Columns[colunIndex].GetCellContent(dgv_MeterData.Items[rowIndex]) as TextBox)?.Text;
                        if (string.IsNullOrWhiteSpace(MD_BAR_CODE)) return;
                        //if (ConfigHelper.Instance.Factory_ISIntercept)  //从条形码截取长度
                        //{
                        //    DynamicViewModel modelTemp = ViewModel.Meters[rowIndex];
                        //    int startIndex = ConfigHelper.Instance.Factory_StartIndex;
                        //    int len = ConfigHelper.Instance.Factory_Len;
                        //    len = (len < 0 || len > MD_BAR_CODE.Length) ? 0 : len;
                        //    startIndex = (startIndex < 1 || startIndex > MD_BAR_CODE.Length) ? 0 : startIndex;
                        //    len = (startIndex + len > MD_BAR_CODE.Length) ? MD_BAR_CODE.Length - startIndex : len;
                        //    if (ConfigHelper.Instance.Factory_Len != 0)
                        //    {
                        //        MD_BAR_CODE = MD_BAR_CODE.Substring(startIndex, len);
                        //        MD_BAR_CODE = ConfigHelper.Instance.Factory_Prefix + MD_BAR_CODE + ConfigHelper.Instance.Factory_Suffix;
                        //    }
                        //    ViewModel.Meters[rowIndex].SetProperty("AVR_MADE_NO", MD_BAR_CODE);
                        //    //MD_ASSET_NO//资产编号

                        //}

                        bool repeat = false;
                        for (int i = 0; i < ViewModel.Meters.Count; i++)
                        {
                            if (i == rowIndex) continue;
                            DynamicViewModel modelTemp1 = ViewModel.Meters[i];
                            if (MD_BAR_CODE.Equals(modelTemp1.GetProperty("MD_BAR_CODE")?.ToString()))
                            {
                                repeat = true;
                                break;
                            }
                        }
                        if (repeat)
                        {
                            var curCell = GetCell(dgv_MeterData, rowIndex, colunIndex);
                            if (curCell != null)
                            {
                                DynamicViewModel modelTemp1 = ViewModel.Meters[rowIndex];
                                modelTemp1.SetProperty("MD_BAR_CODE", "扫码重复");
                                curCell.IsSelected = true;
                                curCell.Focus();
                                return;
                            }
                        }
                        else
                        {
                            //计量中心要求扫码勾选
                            ViewModel.Meters[rowIndex].SetProperty("MD_CHECKED", "1");
                        }

                        if (rowIndex >= ViewModel.Meters.Count - 1)
                            return;

                        rowIndex += 1;
                        var cell = GetCell(dgv_MeterData, rowIndex, colunIndex);
                        if (cell != null)
                        {
                            cell.IsSelected = true;
                            cell.Focus();
                            dgv_MeterData.BeginEdit();

                        }
                    }
                }



                #region 校验单元格
                string columnHeader = e.Column.Header as string;
                InputParaUnit paraUnit = ViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == columnHeader);
                if (paraUnit == null)
                {
                    if (columnHeader == "无功等级")
                    {
                        paraUnit = ViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == "等级");
                    }
                    else if (columnHeader == "无功常数")
                    {
                        paraUnit = ViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == "常数");
                    }

                    if (paraUnit == null)
                        return;
                }
                string fieldTemp = paraUnit.FieldName;

                if (!(e.Row.DataContext is DynamicViewModel meterCurrent))
                {
                    return;
                }

                int indexTemp = ViewModel.Meters.IndexOf(meterCurrent);
                #endregion

                object cellValue = "";
                if (e.Column is DataGridTemplateColumn col)
                {
                    DynamicViewModel rowData = e.Row.Item as DynamicViewModel;
                    object value = rowData.GetProperty(fieldTemp);
                    cellValue = value;
                }

                //if (e.EditingElement is ComboBox currentElement1)
                //{
                //    cellValue = currentElement1.SelectedItem;
                //}

                //else if (e.EditingElement is TextBox currentElement2)
                //{
                //    cellValue = currentElement2.Text;
                //}

                if (fieldTemp == "MD_DATERECEIVED")
                {
                    if (e.EditingElement is ContentPresenter currentElement3)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(currentElement3, 0);
                        if (child is DatePicker Date)
                        {
                            cellValue = Date.SelectedDate?.ToString("yyyy/MM/dd") ?? "";
                        }
                    }
                }



                if (cellValue == null || cellValue.ToString().IndexOf("重复") >= 0)
                {
                    return;
                }
                if ("添加新项".Equals(cellValue))
                {
                    return;
                }


                if (fieldTemp == "MD_BAR_CODE")
                {
                    //if (ConfigHelper.Instance.Address_ISIntercept)
                    //{
                    //    int starti = ConfigHelper.Instance.Address_StartIndex - 1;
                    //    if (starti < 0) starti = 0;
                    //    int needLen = starti + ConfigHelper.Instance.Address_Len;
                    //    string bar = cellValue.ToString();
                    //    if (bar.Length >= needLen)
                    //    {
                    //        string addr;
                    //        if (ConfigHelper.Instance.Address_LeftToRight)
                    //        {
                    //            addr = bar.Substring(starti, ConfigHelper.Instance.Address_Len).PadLeft(12, '0');
                    //        }
                    //        else
                    //        {
                    //            addr = bar.Substring(bar.Length - needLen, ConfigHelper.Instance.Address_Len).PadLeft(12, '0');
                    //        }

                    //        ViewModel.Meters[indexTemp].SetProperty("MD_POSTAL_ADDRESS", addr);

                    //        if (checkBoxDownload.IsChecked.HasValue && checkBoxDownload.IsChecked.Value)
                    //        {
                    //            string upDownUri = UpDownMDS.IsChecked == true ? "MDS" : "营销";
                    //            ViewModel.Frame_DownMeterInfoFromMisByPos(e.Row.GetIndex(), upDownUri);
                    //        }
                    //        return;
                    //    }
                    //}
                }


                if (!(checkBoxQuickInput.IsChecked.HasValue && checkBoxQuickInput.IsChecked.Value))
                {
                    if (fieldTemp == "MD_GRADE" || fieldTemp == "MD_CONSTANT")
                    {
                        if (!(ViewModel.Meters[indexTemp].GetProperty(fieldTemp) is string ABtmp))
                            ABtmp = "";
                        string[] abtmps = ABtmp.Split('(', ')');
                        string[] newab = cellValue.ToString().Split('(', ')');
                        if (columnHeader == "等级" || columnHeader == "常数")
                        {
                            if (ConfigHelper.Instance.InputUIShowReactive)
                            {
                                if (newab.Length >= 2) ABtmp = cellValue.ToString();
                                else if (abtmps.Length >= 2) ABtmp = cellValue + "(" + abtmps[1] + ")";
                                else ABtmp = cellValue.ToString();
                            }
                            else
                                ABtmp = cellValue.ToString();
                        }
                        else if (columnHeader == "无功等级" || columnHeader == "无功常数")
                        {
                            if (newab.Length >= 2)
                                ABtmp = cellValue.ToString();
                            else
                                ABtmp = abtmps[0] + "(" + cellValue + ")";
                        }
                        ViewModel.Meters[indexTemp].SetProperty(fieldTemp, ABtmp);
                    }
                    return;
                }
                List<string> NoQuickInput = new List<string>() { "MD_MADE_NO", "MD_POSTAL_ADDRESS" };
                if (NoQuickInput.Contains(fieldTemp))
                {
                    return;
                }

                //证书编号自动录入部分
                //string CertificateIndex = "";
                for (int i = indexTemp; i < ViewModel.Meters.Count; i++)
                {
                    if (fieldTemp == "MD_CERTIFICATE_NO")//证书编号
                    {
                        //delete yjt 20220707 证书编号不加结尾编号
                        //CertificateIndex = (i + 1).ToString().PadLeft(3, '0');
                        //viewModel.Meters[i].SetProperty(fieldTemp, cellValue + CertificateIndex);
                        if (!string.IsNullOrWhiteSpace(cellValue as string) && cellValue.ToString().Length > 1)
                        {
                            if (!string.IsNullOrWhiteSpace(cellValue.ToString()) && int.TryParse(cellValue.ToString().Substring(cellValue.ToString().Length - 2, 2), out int cid))
                            {
                                ViewModel.Meters[i].SetProperty(fieldTemp, cellValue.ToString().Substring(0, cellValue.ToString().Length - 2) + (cid + i).ToString("D2"));
                            }
                        }
                    }
                    else if (fieldTemp == "MD_BAR_CODE" || fieldTemp == "MD_ASSET_NO")    //资产编号，条形码
                    {
                        if (string.IsNullOrWhiteSpace(ViewModel.Meters[i].GetProperty(fieldTemp) as string))
                        {
                            if (cellValue != null && cellValue.ToString().Length < 22)
                            {
                                int len = cellValue.ToString().Length;
                                if (long.TryParse(cellValue.ToString(), out long value))
                                {
                                    value += (i - indexTemp);
                                    ViewModel.Meters[i].SetProperty(fieldTemp, value.ToString($"D{len}"));
                                }

                            }
                        }
                    }
                    else if (fieldTemp == "MD_OTHER_4")//样品单号
                    {
                        if (!string.IsNullOrWhiteSpace(cellValue as string) && cellValue.ToString().Length > 1)
                        {
                            if (int.TryParse(cellValue.ToString().Substring(cellValue.ToString().Length - 2, 2), out int cid))
                            {
                                ViewModel.Meters[i].SetProperty(fieldTemp, cellValue.ToString().Substring(0, cellValue.ToString().Length - 2) + (cid + i).ToString("D2"));
                            }
                        }

                    }
                    else if (fieldTemp == "MD_GRADE" || fieldTemp == "MD_CONSTANT")
                    {
                        if (!(ViewModel.Meters[i].GetProperty(fieldTemp) is string ABtmp))
                            ABtmp = "";
                        string[] abtmps = ABtmp.Split('(', ')');
                        string[] newab = cellValue.ToString().Split('(', ')');
                        if (columnHeader == "等级" || columnHeader == "常数")
                        {
                            if (ConfigHelper.Instance.InputUIShowReactive)
                            {
                                if (newab.Length >= 2) ABtmp = cellValue.ToString();
                                else if (abtmps.Length >= 2) ABtmp = cellValue + "(" + abtmps[1] + ")";
                                else ABtmp = cellValue.ToString();
                            }
                            else
                                ABtmp = cellValue.ToString();
                        }
                        else if (columnHeader == "无功等级" || columnHeader == "无功常数")
                        {
                            if (newab.Length >= 2)
                                ABtmp = cellValue.ToString();
                            else
                                ABtmp = abtmps[0] + "(" + cellValue + ")";
                        }
                        ViewModel.Meters[i].SetProperty(fieldTemp, ABtmp);
                    }
                    else
                    {
                        ViewModel.Meters[i].SetProperty(fieldTemp, cellValue);
                    }
                }
            }
            else
            {

            }
        }

        /// <summary>
        /// 是否需要检定复选框
        /// </summary>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox boxYaojian)
            {
                string ischeck = boxYaojian.IsChecked == true ? "1" : "0";
                if (boxYaojian.DataContext is DynamicViewModel modelTemp)
                {
                    modelTemp.SetProperty("MD_CHECKED", ischeck);
                }
            }

        }

        /// <summary>
        /// 添加新的编码
        /// </summary>
        private void Event_AddNew(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.OriginalSource is ComboBox comboBox) || comboBox.SelectedItem as string != "添加新项")
            {
                return;
            }
            #region 添加新的编码
            BindingExpression expression = comboBox.GetBindingExpression(ComboBox.TextProperty);
            if (expression == null)
            {
                return;
            }
            //解析编码路径
            string strPath = expression.ParentBinding.Path.Path;
            //如果没有创建新的值,就恢复原来的值
            string oldValue = (expression.DataItem as DynamicViewModel)?.GetProperty(strPath) as string;
            InputParaUnit unitTemp = ViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.FieldName == strPath);
            if (unitTemp != null)
            {
                //获取节点
                CodeTreeNode nodeTemp = CodeTreeViewModel.Instance.GetCodeByEnName(unitTemp.CodeType, 2);
                if (nodeTemp != null)
                {
                    Window_AddNewCode dlg = new Window_AddNewCode(nodeTemp);
                    bool? b = dlg.ShowDialog();
                    if (b.HasValue && b.Value)//取消也刷新
                    {
                        DataGridCell cellTemp = Utils.FindVisualParent<DataGridCell>(comboBox);
                        if (cellTemp != null)
                        {
                            if (cellTemp.Column is DataGridTemplateColumn col && nodeTemp.Children.Count > 0)
                            {
                                //Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(nodeTemp.CODE_TYPE);
                                //List<string> dataSource = dictionary.Keys.ToList();
                                //dataSource.Add("添加新项");
                                //list.Clear();
                                //list.AddRange(dataSource);

                                //fac.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = dataSource });

                                ((List<string>)comboBox.ItemsSource).Insert(0, dlg.NameText);

                                comboBox.Text = nodeTemp.Children[nodeTemp.Children.Count - 1].CODE_NAME;
                                comboBox.Text = dlg.NameText;
                                return;
                            }
                        }
                    }
                }
            }
            #endregion
            comboBox.SelectedItem = oldValue;
        }
        private void Click_IsChecked(object sender, RoutedEventArgs e)
        {
            int index = -1;
            if (e.OriginalSource is MenuItem menuTemp)
            {
                string ischek = "-1";
                bool UpOrDown = true; //true是下，false向上
                switch (menuTemp.Name)
                {
                    case "UpCheck": //向上选表
                        ischek = "1";
                        UpOrDown = false;
                        break;
                    case "DownCheck":  //向下选表
                        ischek = "1";
                        UpOrDown = true;
                        break;
                    case "NoUpCheck"://向上取消选表
                        ischek = "0";
                        UpOrDown = false;
                        break;
                    case "NoDownCheck": //向下取消选表
                        ischek = "0";
                        UpOrDown = true;
                        break;
                    default:
                        break;
                }
                var _cells = dgv_MeterData.SelectedCells; //获取选中单元格的列表
                if (_cells.Any())
                {
                    index = dgv_MeterData.Items.IndexOf(_cells.First().Item);
                }

                if (index >= 0 && ischek != "-1")
                {
                    if (UpOrDown)  //向下
                    {
                        for (int i = index; i < ViewModel.Meters.Count; i++)
                        {
                            DynamicViewModel modelTemp = ViewModel.Meters[i];
                            modelTemp.SetProperty("MD_CHECKED", ischek);
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= index; i++)
                        {
                            DynamicViewModel modelTemp = ViewModel.Meters[i];
                            modelTemp.SetProperty("MD_CHECKED", ischek);
                        }
                    }
                }
            }
        }

        private async void DownInfo_Click(object sender, RoutedEventArgs e)
        {
            Window_Wait.Instance.StateWait("正在下载数据,请等待", 180000);
            string upDownUri = UpDownMDS.IsChecked == true ? "MDS" : "营销";
            await Task.Run(() => ViewModel.Frame_DownMeterInfoFromMis(upDownUri));
            Window_Wait.Instance.EndWait();
        }

        private void CheckBoxQuickInput_Click(object sender, RoutedEventArgs e)
        {
            Core.OperateFile.WriteINI("Config", "checkBoxQuickInput", checkBoxQuickInput.IsChecked.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
        }

        private void CheckBoxDownload_Click(object sender, RoutedEventArgs e)
        {
            Core.OperateFile.WriteINI("Config", "checkBoxDownload", checkBoxDownload.IsChecked.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
        }

        // 单选按钮事件
        private void UpDown_Checked(object sender, RoutedEventArgs e)
        {

            switch (((RadioButton)sender).Name)
            {
                case "UpDownMDS":
                    EquipmentData.IsReadMeterInfo = 1;
                    break;
                case "UpDownYX":
                    EquipmentData.IsReadMeterInfo = 2;
                    break;
                case "UpDownRead":
                    EquipmentData.IsReadMeterInfo = 3;
                    break;
                default:
                    EquipmentData.IsReadMeterInfo = 3;
                    break;
            }
        }

        private void RadioBtnSoure(int IsReadMeterInfo)
        {
            switch (IsReadMeterInfo)
            {
                case 1:
                    this.UpDownMDS.IsChecked = true;
                    break;
                case 2:
                    this.UpDownYX.IsChecked = true;
                    break;
                case 3:
                    this.UpDownRead.IsChecked = true;
                    break;
            }

        }

        private async void ReadAddress_Click(object sender, RoutedEventArgs e)
        {
            Window_Wait.Instance.StateWait("正在读数据,请等待", 210000);
            await ViewModel.Read_Meter_Addres();
            Window_Wait.Instance.EndWait();
        }

        private void CheckBoxUseJJGPoints_Click(object sender, RoutedEventArgs e)
        {
            Core.OperateFile.WriteINI("Config", "CheckBoxUseJJGPoints", checkBoxDownload.IsChecked.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
        }

        private void ChkColumnJump_Click(object sender, RoutedEventArgs e)
        {
            Core.OperateFile.WriteINI("Config", "ChkColumnJump", ChkColumnJump.IsChecked.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
            Core.OperateFile.WriteINI("Config", "TxtColJumpCount", TxtColJumpCount.Text.Trim(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");
        }

        // 录入完成
        private void OnOK(object sender, RoutedEventArgs e)
        {

            //if (ConfigHelper.Instance.InputCheckAddress)
            //{

            //    List<int> list = ViewModel.CheckAddressCompleted();
            //    if (list.Count > 0)
            //    {
            //        StringBuilder msg = new StringBuilder();
            //        foreach (int no in list)
            //        {
            //            msg.Append($"表位{no + 1}");
            //            msg.AppendLine();
            //        }
            //        msg.Insert(0, $"以下表位地址比对失败,数量{list.Count}:\n\r");
            //        msg.AppendLine();
            //        msg.Append("继续录入完成按[是]，否则按[否]");

            //        if (MessageBoxResult.No == MessageBox.Show(msg.ToString(), "提示", MessageBoxButton.YesNo, MessageBoxImage.Question))
            //            return;
            //    }

            //}

            ViewModel.UpdateMeterInfo();


        }

        private void Dgv_MeterData_SelectedCellChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            dgv_MeterData.Focus();
            dgv_MeterData.BeginEdit();
            //DataGridCell cell = e.AddedCells[0];

        }
    }
}
