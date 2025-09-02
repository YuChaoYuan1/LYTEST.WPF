using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.AccessControl;
using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.InputPara;
using LYTest.WPF.Controls;
using LYTest.WPF.Converter;
using LYTest.WPF.Model;
using LYTest.WPF.UiGeneral;
using LYTest.WPF.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace LYTest.WPF.View
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
            get
            {
                return Resources["MeterInputParaViewModel"] as MeterInputParaViewModel;
            }
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
            if ((paraUnit.CodeType.Contains("CurrentValue") && paraUnit.FieldName.Contains("MD_UA")) ||
                (paraUnit.CodeType.Contains("VoltageValue") && paraUnit.FieldName.Contains("MD_UB")))
            {
                Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(paraUnit.CodeType);
                List<string> dataSource = dictionary.Keys.ToList();
                dataSource.Add("添加新项");
                comboBox = new ControlEnumComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = Application.Current.Resources["StyleComboBox"] as Style,
                    Width = 100,
                    Tag = paraUnit.FieldName,
                    FontSize = 16,
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
                    Style = Application.Current.Resources["StyleComboBox"] as Style,
                    Width = 100,
                    Tag = paraUnit.FieldName,
                    FontSize = 16,
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
                    Style = Application.Current.Resources["StyleComboBox"] as Style,
                    Width = 100,
                    Tag = paraUnit.FieldName,
                    FontSize = 16,
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
                    Style = Application.Current.Resources["StyleComboBox"] as Style,
                    Width = 100,
                    Tag = paraUnit.FieldName,
                    FontSize = 16
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
                    if (((string)comboBox.SelectedItem).Contains("添加新项"))
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

                    if (((string)comboBox.SelectedItem).Contains("添加新项"))
                        return;
                    for (int i = 0; i < ViewModel.Meters.Count; i++)
                    {
                        ViewModel.Meters[i].SetProperty(fieldName, comboBox.SelectedItem);
                    }
                }
            }
        }



        public bool IsAllSelected
        {
            get
            {
                return (bool)GetValue(IsAllSelectedProperty);
            }
            set
            {
                SetValue(IsAllSelectedProperty, value);
            }
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
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "表要检标记,全选"
            };
            Binding binding = new Binding("IsAllSelected")
            {
                Source = this
            };
            checkbox.SetBinding(ToggleButton.IsCheckedProperty, binding);
            //Binding cellBinding = new Binding("MD_CHECKED")
            //{
            //    Mode = BindingMode.TwoWay,
            //    Converter = new BoolBitConverter()
            //};
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
                    string bindingPath = (colunm.Binding as Binding).Path.Path;
                    #region --处理条码格输入逻辑--
                    if (bindingPath == "MD_BAR_CODE")
                    {
                        int currentCellRowIndex = e.Row.GetIndex();
                        if (dgv_MeterData.CurrentCell.Column == null) return;
                        int colunIndex = dgv_MeterData.CurrentCell.Column.DisplayIndex;
                        string MD_BAR_CODE = (dgv_MeterData.Columns[colunIndex].GetCellContent(dgv_MeterData.Items[currentCellRowIndex]) as TextBox)?.Text;
                        if (string.IsNullOrWhiteSpace(MD_BAR_CODE)) return;

                        #region -- 处理从条码获取地址的逻辑 -- 
                        if (ConfigHelper.Instance.Address_IsIntercept)
                        {
                            string MD_POSTAL_ADDRESS = MD_BAR_CODE;
                            int addressSubStartIndex = Math.Min(Math.Max(ConfigHelper.Instance.Address_StartIndex - 1, 0), Math.Max(0, MD_POSTAL_ADDRESS.Length - 1));
                            int subLength = Math.Min(Math.Max(ConfigHelper.Instance.Address_Len, 0), Math.Max(0, MD_POSTAL_ADDRESS.Length - addressSubStartIndex));

                            if (ConfigHelper.Instance.Address_LeftToRight)
                            {
                                MD_POSTAL_ADDRESS = MD_POSTAL_ADDRESS.Substring(addressSubStartIndex, subLength).PadLeft(12, '0');
                            }
                            else
                            {
                                MD_POSTAL_ADDRESS = MD_POSTAL_ADDRESS.Substring(MD_POSTAL_ADDRESS.Length - addressSubStartIndex - subLength, subLength).PadLeft(12, '0');
                            }

                            ViewModel.Meters[currentCellRowIndex].SetProperty("MD_POSTAL_ADDRESS", MD_POSTAL_ADDRESS);

                            if (checkBoxDownload.IsChecked.HasValue && checkBoxDownload.IsChecked.Value)
                            {
                                string upDownUri = UpDownMDS.IsChecked == true ? "MDS" : "营销";
                                ViewModel.Frame_DownMeterInfoFromMisByPos(e.Row.GetIndex(), upDownUri);
                            }
                        }
                        #endregion

                        #region -- 出厂编码设计 -- 
                        string MD_MADE_NO = string.Empty;

                        #region --基于通讯地址生产出厂编码--
                        // 是否通过通讯地址生产出厂编码
                        if (ConfigHelper.Instance.Factory_GenerateSource.Contains("地址"))
                        {
                            string MD_POSTAL_ADDRESS = ViewModel.Meters[currentCellRowIndex].GetProperty("MD_POSTAL_ADDRESS").ToString();
                            // 通讯地址可能为空，但是不处理
                            MD_MADE_NO = MD_POSTAL_ADDRESS;
                        }
                        #endregion

                        #region --通过条码生成出厂编码--
                        else if (ConfigHelper.Instance.Factory_GenerateSource.Contains("条形码"))
                        {
                            MD_MADE_NO = MD_BAR_CODE;
                        }
                        #endregion

                        #region --统一格式加工出厂编码长度和读取方向--
                        // 确保加工数据源不是空
                        if (!string.IsNullOrEmpty(MD_MADE_NO))
                        {
                            // 获取起始索引，范围[0,MD_MADE_NO.Length - 1]
                            int subStartIndex = Math.Min(Math.Max(ConfigHelper.Instance.Factory_StartIndex - 1, 0), Math.Max(0, MD_MADE_NO.Length - 1));
                            // 限制截取长度，使其范围不出最大合法范围
                            int subLength = Math.Min(Math.Max(ConfigHelper.Instance.Factory_Length, 0), Math.Max(MD_MADE_NO.Length - subStartIndex, 0));
                            // 如果截取长度大于0
                            if (subLength > 0)
                            {
                                if (ConfigHelper.Instance.Factory_LeftToRight)
                                {
                                    MD_MADE_NO = MD_MADE_NO.Substring(subStartIndex, subLength).PadLeft(ConfigHelper.Instance.Factory_Length, '0');
                                }
                                else
                                {
                                    MD_MADE_NO = MD_MADE_NO.Substring(MD_MADE_NO.Length - subStartIndex - subLength, subLength).PadLeft(ConfigHelper.Instance.Factory_Length, '0');
                                }
                            }
                            MD_MADE_NO = ConfigHelper.Instance.Factory_Prefix + MD_MADE_NO + ConfigHelper.Instance.Factory_Suffix;
                        }
                        #endregion

                        // 不管是否为空值，都要将最终结果赋值给MD_MADE_NO
                        ViewModel.Meters[currentCellRowIndex].SetProperty("MD_MADE_NO", MD_MADE_NO);
                        #endregion

                        #region -- 从条码中生成资产编号 --
                        if (ConfigHelper.Instance.Asset_GenerateSource.Contains("条形码"))
                        {
                            string MD_ASSET_NO = MD_BAR_CODE;

                            ViewModel.Meters[currentCellRowIndex].SetProperty("MD_ASSET_NO", MD_ASSET_NO);
                        }
                        #endregion

                        #region -- 证书编号设计 --
                        // 是否从条码中截取证书编号
                        if (ConfigHelper.Instance.Certificate_GenerateSource.Contains("条形码"))
                        {
                            string barcode = MD_BAR_CODE;
                            // 限制起始索引位置在（0，MD_BAR_CODE.Length-1）
                            int certificateSubStartIndex = Math.Min(Math.Max(ConfigHelper.Instance.Certificate_StartIndex - 1, 0), barcode.Length - 1);
                            int subLength = 0;
                            string certificateString = string.Empty;
                            if (ConfigHelper.Instance.Certificate_LeftToRight)
                            {
                                // 限制截取长度
                                subLength = Math.Min(ConfigHelper.Instance.Certificate_Length, barcode.Length - certificateSubStartIndex);
                                // 截取证书号并补齐长度
                                certificateString = barcode.Substring(certificateSubStartIndex, subLength)
                                    .PadLeft(ConfigHelper.Instance.Certificate_Length, '0');
                            }
                            else
                            {
                                certificateSubStartIndex = barcode.Length - 1 - certificateSubStartIndex;
                                subLength = Math.Min(ConfigHelper.Instance.Certificate_Length, certificateSubStartIndex + 1);
                                certificateString = barcode.Substring(certificateSubStartIndex - subLength + 1, subLength)
                                    .PadLeft(ConfigHelper.Instance.Certificate_Length, '0');
                            }

                            ViewModel.Meters[currentCellRowIndex].SetProperty("MD_CERTIFICATE_NO", certificateString);
                        }
                        else if (ConfigHelper.Instance.Certificate_GenerateSource.Contains("时间"))
                        {
                            // 不是从条码中截取证书编号，按照时间戳生成证书编号
                            ViewModel.Meters[currentCellRowIndex].SetProperty("MD_CERTIFICATE_NO", DateTime.Now.ToString("yyyyMMddHH") + "001");
                        }


                        #endregion

                        #region -- 处理扫码重复逻辑 --
                        bool repeat = false;
                        for (int i = 0; i < ViewModel.Meters.Count; i++)
                        {
                            if (i == currentCellRowIndex) continue;
                            if (MD_BAR_CODE.Equals(ViewModel.Meters[i].GetProperty("MD_BAR_CODE")?.ToString()))
                            {
                                repeat = true;
                                break;
                            }
                        }
                        if (repeat)
                        {
                            var curCell = GetCell(dgv_MeterData, currentCellRowIndex, colunIndex);
                            if (curCell != null)
                            {
                                DynamicViewModel modelTemp1 = ViewModel.Meters[currentCellRowIndex];
                                modelTemp1.SetProperty("MD_BAR_CODE", "扫码重复");
                                curCell.IsSelected = true;
                                curCell.Focus();
                                return;
                            }
                        }
                        else
                        {
                            //计量中心要求扫码勾选
                            ViewModel.Meters[currentCellRowIndex].SetProperty("MD_CHECKED", "1");
                        }
                        #endregion



                        #region -- 完成编辑，跳转到下一行逻辑 --
                        if (currentCellRowIndex >= ViewModel.Meters.Count - 1)
                            return;

                        currentCellRowIndex += 1;

                        var cell = GetCell(dgv_MeterData, currentCellRowIndex, colunIndex);
                        if (cell != null)
                        {
                            cell.IsSelected = true;
                            cell.Focus();
                            dgv_MeterData.BeginEdit();
                        }
                        #endregion
                    }
                    #endregion
                }



                #region -- 校验单元格 -- 
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
                string tempFieldName = paraUnit.FieldName;

                if (!(e.Row.DataContext is DynamicViewModel currentMeter))
                {
                    return;
                }

                int currentMeterIndex = ViewModel.Meters.IndexOf(currentMeter);
                #endregion

                object currentCellValue = e.EditingElement;
                if (e.EditingElement is ComboBox comboBoxElement)
                {
                    currentCellValue = comboBoxElement.SelectedItem;
                }
                else if (e.EditingElement is TextBox textBoxElement)
                {
                    currentCellValue = textBoxElement.Text;
                }

                if (tempFieldName == "MD_DATERECEIVED")
                {
                    if (e.EditingElement is ContentPresenter contentPresenterElement)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(contentPresenterElement, 0);
                        if (child is DatePicker Date)
                        {
                            currentCellValue = Date.SelectedDate?.ToString("yyyy/MM/dd") ?? "";
                        }
                    }
                }


                if (currentCellValue == null || currentCellValue.ToString().Contains("重复"))
                {
                    return;
                }
                if ("添加新项".Equals(currentCellValue))
                {
                    return;
                }


                // 检查是否勾选了向下填充快速录入
                if (!(checkBoxQuickInput.IsChecked.HasValue && checkBoxQuickInput.IsChecked.Value))
                {
                    #region -- 处理常数和等级 -- 
                    if (tempFieldName == "MD_GRADE" || tempFieldName == "MD_CONSTANT")
                    {
                        if (!(ViewModel.Meters[currentMeterIndex].GetProperty(tempFieldName) is string ABtmp))
                            ABtmp = "";
                        string[] abtmps = ABtmp.Split('(', ')');
                        string[] newab = currentCellValue.ToString().Split('(', ')');
                        if (columnHeader == "等级" || columnHeader == "常数")
                        {
                            if (ConfigHelper.Instance.InputUIShowReactive)
                            {
                                if (newab.Length >= 2) ABtmp = currentCellValue.ToString();
                                else if (abtmps.Length >= 2) ABtmp = currentCellValue + "(" + abtmps[1] + ")";
                                else ABtmp = currentCellValue.ToString();
                            }
                            else
                                ABtmp = currentCellValue.ToString();
                        }
                        else if (columnHeader == "无功等级" || columnHeader == "无功常数")
                        {
                            if (newab.Length >= 2)
                                ABtmp = currentCellValue.ToString();
                            else
                                ABtmp = abtmps[0] + "(" + currentCellValue + ")";
                        }
                        ViewModel.Meters[currentMeterIndex].SetProperty(tempFieldName, ABtmp);
                    }
                    return;
                    #endregion
                }
                List<string> NoQuickInput = new List<string>() { "MD_MADE_NO", "MD_POSTAL_ADDRESS" };
                if (NoQuickInput.Contains(tempFieldName))
                {
                    return;
                }
                int encodingLength = int.MaxValue;
                int certificateSubLength = 3;
                #region -- 处理自动录入逻辑 --
                for (int offset = 0; offset < ViewModel.Meters.Count - currentMeterIndex; offset++)
                {
                    #region --处理自动证书编号--
                    if (tempFieldName == "MD_CERTIFICATE_NO")//证书编号
                    {
                        //delete yjt 20220707 证书编号不加结尾编号
                        if (!ConfigHelper.Instance.Certificate_GenerateSource.Contains("无") &&
                            !string.IsNullOrWhiteSpace(currentCellValue as string) &&
                            currentCellValue.ToString().Length >= ViewModel.Meters.Count.ToString().Length &&
                            int.TryParse(currentCellValue.ToString().Substring(Math.Max(0, currentCellValue.ToString().Length - certificateSubLength), Math.Min(currentCellValue.ToString().Length, certificateSubLength)), out int cid))
                        {
                            ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, currentCellValue.ToString().Substring(0, Math.Max(0, currentCellValue.ToString().Length - certificateSubLength)) + (cid + offset).ToString().PadLeft(certificateSubLength, '0').Substring(0, certificateSubLength));
                        }
                        else if (string.IsNullOrWhiteSpace(currentCellValue as string))
                        {
                            ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, ViewModel.Meters[currentMeterIndex + offset].GetProperty("MD_BAR_CODE").ToString());
                        }
                        continue;
                    }
                    #endregion
                    #region --处理自动条码，资产编号--
                    if (tempFieldName == "MD_BAR_CODE" || tempFieldName == "MD_ASSET_NO")    //资产编号，条形码
                    {
                        // 如果这个单元格内已经有非空的值了，那么它就不需要自动填充
                        if (!string.IsNullOrWhiteSpace(ViewModel.Meters[currentMeterIndex + offset].GetProperty(tempFieldName) as string)) continue;

                        // 如果参照单元格内的值为空
                        if (string.IsNullOrEmpty(currentCellValue.ToString())) continue;

                        Match digitMatch = Regex.Match(currentCellValue.ToString(), @"\d+$");
                        if (digitMatch.Success)
                        {
                            // 如果匹配成功，解析出数字部分,
                            // 防止超出最大的ulong长度，截取最后16位数字
                            if (ulong.TryParse(digitMatch.Value.Substring(Math.Max(0, digitMatch.Value.Length - 16), Math.Min(digitMatch.Value.Length, 16)), out ulong digitValue))
                            {
                                string noDigitPart = currentCellValue.ToString().Substring(0, Math.Max(0, currentCellValue.ToString().Length - 16));
                                encodingLength = Math.Min(encodingLength, digitMatch.Value.Length);
                                string digitPart = (digitValue + (ulong)offset).ToString();
                                //digitPart = digitPart.Substring(digitPart.Length - encodingLength, encodingLength);
                                string newValue = noDigitPart + digitPart;
                                ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, newValue);
                            }
                        }
                        else
                        {
                            // 非数字编码结尾,或者超了ulong的最大转化长度，尝试使用Hex值
                            Match hexMatch = Regex.Match(currentCellValue.ToString(), @"[0-9A-F]+$", RegexOptions.IgnoreCase);
                            if (hexMatch.Success)
                            {
                                ulong hexValue = Convert.ToUInt64(hexMatch.Value.Substring(Math.Max(0, hexMatch.Value.Length - 16), Math.Min(hexMatch.Value.Length, 16)), 16);
                                encodingLength = Math.Min(encodingLength, hexMatch.Value.Length);
                                // 生成新的值，Hex部分加上偏移量
                                string noHexPart = currentCellValue.ToString().Substring(0, currentCellValue.ToString().Length - hexMatch.Value.Length);
                                string hexPart = (hexValue + (ulong)offset).ToString($"X{encodingLength}");
                                hexPart = hexPart.Substring(hexPart.Length - encodingLength, encodingLength);
                                string newValue = noHexPart + hexPart;
                                ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, newValue);
                            }
                            else
                            {
                                encodingLength = Math.Min(encodingLength, ViewModel.Meters.Count.ToString().Length);
                                // 如果没有数字或Hex结尾，直接使用原值+偏移量
                                ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, currentCellValue.ToString() + offset.ToString($"D{encodingLength}"));
                            }
                        }
                        if (tempFieldName == "MD_BAR_CODE" && ConfigHelper.Instance.Factory_GenerateSource.Contains("条形码"))
                        {
                            string MD_MADE_NO = ViewModel.Meters[currentMeterIndex + offset].GetProperty(tempFieldName).ToString();
                            // 约束起始索引位置以及截取长度
                            int subStartIndex = ConfigHelper.Instance.Factory_StartIndex;
                            int subLength = ConfigHelper.Instance.Factory_Length;
                            subLength = Math.Min(Math.Max(subLength, 0), MD_MADE_NO.Length);
                            subStartIndex = Math.Min(Math.Max(subStartIndex - 1, 0), MD_MADE_NO.Length - 1);
                            subLength = Math.Min(subLength, MD_MADE_NO.Length - subStartIndex);
                            if (subLength > 0)
                            {
                                MD_MADE_NO = MD_MADE_NO.Substring(subStartIndex, subLength);
                                MD_MADE_NO = ConfigHelper.Instance.Factory_Prefix + MD_MADE_NO + ConfigHelper.Instance.Factory_Suffix;
                            }
                            ViewModel.Meters[currentMeterIndex + offset].SetProperty("MD_MADE_NO", MD_MADE_NO);
                        }
                        continue;
                    }
                    #endregion
                    #region -- 处理自动样品单号 -- 
                    if (tempFieldName == "MD_OTHER_4")//样品单号
                    {
                        if (!string.IsNullOrWhiteSpace(currentCellValue as string))
                        {
                            if (int.TryParse(currentCellValue.ToString().Substring(Math.Max(0, currentCellValue.ToString().Length - 2), Math.Min(currentCellValue.ToString().Length, 2)), out int cid))
                            {
                                ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, currentCellValue.ToString().Substring(0, Math.Max(0, currentCellValue.ToString().Length - 2)) + (cid + offset).ToString("D2"));
                            }
                        }
                        continue;
                    }
                    #endregion
                    #region -- 处理自动等级和常数 --
                    if (tempFieldName == "MD_GRADE" || tempFieldName == "MD_CONSTANT")
                    {
                        if (!(ViewModel.Meters[currentMeterIndex + offset].GetProperty(tempFieldName) is string ABtmp))
                            ABtmp = "";
                        string[] abtmps = ABtmp.Split('(', ')');
                        string[] newab = currentCellValue.ToString().Split('(', ')');
                        if (columnHeader == "等级" || columnHeader == "常数")
                        {
                            if (ConfigHelper.Instance.InputUIShowReactive)
                            {
                                if (newab.Length >= 2) ABtmp = currentCellValue.ToString();
                                else if (abtmps.Length >= 2) ABtmp = currentCellValue + "(" + abtmps[1] + ")";
                                else ABtmp = currentCellValue.ToString();
                            }
                            else
                                ABtmp = currentCellValue.ToString();
                        }
                        else if (columnHeader == "无功等级" || columnHeader == "无功常数")
                        {
                            if (newab.Length >= 2)
                                ABtmp = currentCellValue.ToString();
                            else
                                ABtmp = abtmps[0] + "(" + currentCellValue + ")";
                        }
                        ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, ABtmp);
                        continue;
                    }
                    #endregion
                    #region -- 处理无特未分配柜子的字段 --
                    ViewModel.Meters[currentMeterIndex + offset].SetProperty(tempFieldName, currentCellValue);
                    #endregion
                }
                #endregion
            }
            else
            {

            }
        }

        public void AutoGenerateFactory()
        {
        }

        /// <summary>
        /// 是否需要检定复选框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_AddNew(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.OriginalSource is ComboBox comboBox) || comboBox.SelectedItem as string != "添加新项")
            {
                return;
            }
            #region 添加新的编码
            BindingExpression expression = comboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
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
                    Window_AddNewCode windowTemp = new Window_AddNewCode(nodeTemp);
                    bool? boolTemp = windowTemp.ShowDialog();
                    //if (boolTemp.HasValue && boolTemp.Value)//取消也刷新
                    {
                        DataGridCell cellTemp = Utils.FindVisualParent<DataGridCell>(comboBox);
                        if (cellTemp != null)
                        {
                            if (cellTemp.Column is DataGridComboBoxColumn columnTemp && nodeTemp.Children.Count > 0)
                            {
                                Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(nodeTemp.CODE_TYPE);
                                List<string> dataSource = dictionary.Keys.ToList();
                                dataSource.Add("添加新项");
                                columnTemp.ItemsSource = dataSource;
                                comboBox.SelectedItem = nodeTemp.Children[nodeTemp.Children.Count - 1].CODE_NAME;
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
    }
}
