using Aspose.Cells;
using Aspose.Words;
using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.DAL;
using LYTest.DataManager.SG.Controls;
using LYTest.DataManager.SG.Exports;
using LYTest.DataManager.SG.Mark.ViewModel;
using LYTest.DataManager.SG.ViewModel;
using LYTest.DataManager.SG.ViewModel.Meters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LYTest.DataManager.SG
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// 主窗体
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            LoadColumns();
            LoadTemplates();
            MetersVM.SearchMeters();
            //checkBoxPreview.IsChecked = Properties.Settings.Default.IsPreview;
            datagridMeter.ColumnReordered += DatagridMeter_ColumnReordered;

            textBlockMessage.DataContext = MessageDisplay.Instance;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessageWindow.Instance.Owner = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DetailedData.Instance != null)
            {
                DetailedData.Instance.IsClose = true;
                DetailedData.Instance.Close();
            }
            base.OnClosing(e);
        }


        void DatagridMeter_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            var columnsTemp = datagridMeter.Columns.OrderBy(item => item.DisplayIndex);
            var columnNames = from item in columnsTemp select item.Header.ToString();
            Properties.Settings.Default.ColumnNames = string.Join(",", columnNames);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 表信息模型
        /// </summary>
        private MetersViewModel MetersVM
        {
            get
            {
                return Resources["MetersViewModel"] as MetersViewModel;
            }
        }
        /// <summary>
        /// 窗体的拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private List<Page> pages = new List<Page>();
        /// <summary>
        /// 页面列表
        /// </summary>
        public List<Page> Pages { get { return pages; } set { pages = value; } }


        #region 数据
        /// <summary>
        /// 加载列
        /// </summary>
        private void LoadColumns()
        {
            //string InfoNames = "台体编号,表位,条形码,任务编号,证书编号,检验员,核验员,检定日期,计检日期,结论,上传标识,制造厂家,测量方式,电压(V),电流(A),频率(Hz),首检抽检,检定类型,互感器,费控类型,检定规程,表类型,常数,等级,表型号,通讯协议,载波协议,通讯地址,送检单位,资产编号,出厂编号,脉冲类型,样品单号,是否要检,温度,湿度,是否需要上传,备用5,方案编号,铅封1,铅封2,铅封3,铅封4,铅封5,主管,表唯一编号,表类别";
            string InfoNames = "条形码,出厂编号,表位,任务编号,证书编号,检验员,核验员,检定日期,计检日期,结论,上传标识,制造厂家,测量方式,电压(V),电流(A),频率(Hz),首检抽检,检定类型,互感器,费控类型,检定规程,表类型,常数,等级,表型号,通讯协议,载波协议,通讯地址,送检单位,资产编号,脉冲类型,样品单号,是否要检,温度,湿度,是否需要上传,备用5,方案编号,铅封1,铅封2,铅封3,铅封4,铅封5,主管,表唯一编号,表类别,台体编号";
            string ResultNames = "外观检查,准确度试验,预先调试,多功能试验,通讯协议检查试验,一致性试验,电气性能试验,智能表功能试验,事件记录试验,冻结功能试验,红外通信试验,费控功能试验,自热试验";
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ColumnNames)
                || Properties.Settings.Default.ColumnNames.IndexOf("外观检查") == 0
                || Properties.Settings.Default.ColumnNames.IndexOf("电压(V)") == 0
                )
            {
                Properties.Settings.Default.ColumnNames = $"{InfoNames},{ResultNames}";
            }
            string[] columnNames = Properties.Settings.Default.ColumnNames.Split(',');
            //这里添加分项总结论
            //1：先获得大项目的名字，来创建列
            //2: 在获取每个大项目下所有结论的集合


            //columnNames = SortColunms(columnNames);//对列进行排序
            List<string> resoultS = GetResoultName();
            if (columnNames.Length != MetersViewModel.ParasModel.AllUnits.Count + resoultS.Count)
            {
                List<string> list = new List<string>();
                list.AddRange(InfoNames.Split(','));
                for (int i = 0; i < MetersViewModel.ParasModel.AllUnits.Count; i++)
                {
                    if (!list.Contains(MetersViewModel.ParasModel.AllUnits[i].DisplayName))
                    {
                        list.Add(MetersViewModel.ParasModel.AllUnits[i].DisplayName);
                    }
                }
                list.AddRange(resoultS);

                columnNames = list.ToArray();
                Properties.Settings.Default.ColumnNames = string.Join(",", columnNames);
                Properties.Settings.Default.Save();
            }

            for (int i = 0; i < columnNames.Length; i++)
            {
                InputParaUnit paraUnit = MetersViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == columnNames[i]);
                if (paraUnit != null)
                {
                    DataGridColumn column = new DataGridTextColumn
                    {
                        Header = paraUnit.DisplayName,
                        Binding = new Binding(paraUnit.FieldName),
                        IsReadOnly = true,
                        CellStyle = new System.Windows.Style(typeof(DataGridCell))
                        {
                            Setters = { new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center) }
                        }
                    };
                    datagridMeter.Columns.Add(column);
                }
                else
                {
                    if (resoultS.Contains(columnNames[i]))
                    {
                        DataGridColumn column = new DataGridTextColumn
                        {
                            Header = columnNames[i],
                            Binding = new Binding(columnNames[i]),
                            IsReadOnly = true,
                            CellStyle = new System.Windows.Style(typeof(DataGridCell))
                            {
                                Setters = { new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center) }
                            }
                        };
                        datagridMeter.Columns.Add(column);
                    }
                }
            }
        }
        private List<string> GetResoultName()
        {
            List<string> ProjectName = new List<string>();
            //获取所有ID的名称
            try
            {
                FieldInfo[] f_key = typeof(UniversityMeterID).GetFields();
                for (int i = 0; i < f_key.Length; i++)
                {
                    ProjectName.Add(f_key[i].Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return ProjectName;
        }

        //private string[] SortColunms(string[] columnNames)
        //{
        //    List<string> list = new List<string>() {"表位", "结论", "条形码","资产编号", "任务编号", "通讯地址", "检定规程", "台体编号", "检定日期", "计检日期", "检验员", "核验员", "主管",
        //        "证书编号", "出厂编号", "温度", "湿度" };
        //    List<string> value = columnNames.ToList();

        //    List<string> resoult = new List<string>();
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        if (value.Contains(list[i]))  //找到这个值
        //        {
        //            resoult.Add(list[i]); //按照顺序添加指定的排序
        //            value.Remove(list[i]); //删除旧的
        //        }
        //    }

        //    for (int i = 0; i < value.Count; i++)
        //    {
        //        resoult.Add(value[i]); //按照顺序添加指定的排序
        //    }
        //    return resoult.ToArray();
        //}

        /// <summary>
        /// 加载报表模板列表
        /// </summary>
        private void LoadTemplates()
        {
            string path = string.Format(@"{0}\Res\Word", Directory.GetCurrentDirectory());
            if (!Directory.Exists(path))
            {
                MessageBox.Show("\r\n\r\n未找到报表模板目录\\Res\\Word\r\n\r\n\r\n\r\n\r\n");
                return;
            }
            string[] fileNames = Directory.GetFiles(path);
            List<string> listNames = new List<string>();
            foreach (string fileName in fileNames)
            {
                string[] arrayName = fileName.Split('\\');
                string nameTemp = arrayName[arrayName.Length - 1];
                if (nameTemp.EndsWith(".doc") || nameTemp.EndsWith(".docx"))
                {
                    listNames.Add(nameTemp);
                }
            }
            comboBoxTemplates.ItemsSource = listNames;
            comboBoxTemplates.SelectedItem = Properties.Settings.Default.ReportPath;
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                ReportHelper.WordApp.Quit(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges);
            }
            catch
            { }
            try
            {
                DocumentViewModel.WordApp.Quit(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges);
            }
            catch
            { }
            base.OnClosed(e);
        }
        private ContextMenu MenuTemp
        {
            get { return Resources["contextMenu"] as ContextMenu; }
        }
        private void DatagridMeter_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            #region 寻找鼠标点到的单元格
            DataGridCell cellTemp = null;
            Point pointTemp = e.GetPosition(datagridMeter);
            HitTestResult hitResult = VisualTreeHelper.HitTest(datagridMeter, pointTemp);
            if (hitResult != null)
            {
                cellTemp = Utils.FindVisualParent<DataGridCell>(hitResult.VisualHit);
            }
            if (cellTemp == null)
            {
                //menuTemp.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                MenuTemp.Visibility = Visibility.Visible;
            }
            #endregion
            DynamicViewModel modelTemp = cellTemp.DataContext as DynamicViewModel;
            string fieldName = "";
            #region 获取列对应的字段名称
            if (cellTemp.Column is DataGridTextColumn)
            {
                DataGridTextColumn columnTemp = cellTemp.Column as DataGridTextColumn;
                Binding bindingTemp = columnTemp.Binding as Binding;
                fieldName = bindingTemp.Path.Path;
            }
            else
            {
                if (cellTemp.Column is DataGridCheckBoxColumn)
                {
                    DataGridCheckBoxColumn columnTemp = cellTemp.Column as DataGridCheckBoxColumn;
                    Binding bindingTemp = columnTemp.Binding as Binding;
                    fieldName = bindingTemp.Path.Path;
                }
            }
            #endregion
            if (string.IsNullOrEmpty(fieldName))
            {
                //menuTemp.Visibility = Visibility.Collapsed;
                return;
            }
            object cellValue = modelTemp.GetProperty(fieldName);
            string temp = "";
            if (cellValue != null)
            {
                temp = cellValue.ToString();
            }
            MetersVM.LoadFilterCollection(fieldName, temp);
        }

        private void DatagridMeter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        ///// <summary>
        ///// 预览按钮
        ///// </summary>
        //private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    Properties.Settings.Default.IsPreview = checkBoxPreview.IsChecked.HasValue && checkBoxPreview.IsChecked.Value;
        //    Properties.Settings.Default.Save();
        //}
        /// <summary>
        /// 查询按钮
        /// </summary>
        private void Click_SearchMeters(object sender, System.Windows.RoutedEventArgs e)
        {
            MetersVM.SearchMeters1();
        }

        /// <summary>
        /// 选中模板改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxTemplates.SelectedItem != null)
            {
                Properties.Settings.Default.ReportPath = comboBoxTemplates.SelectedItem.ToString();
                Properties.Settings.Default.Save();

                //MainWindow windowTemp = Application.Current.MainWindow as MainWindow;
                //if (windowTemp != null)
                //{
                //    PageInsertBookmark pageWordTemplate = windowTemp.Pages.FirstOrDefault(item => item is PageInsertBookmark) as PageInsertBookmark;
                //    if (pageWordTemplate != null)
                //    {
                //        Dispatcher.BeginInvoke(new Action(() =>
                //        {
                //            pageWordTemplate.OpenTemplate();
                //        }));
                //    }
                //}
            }
        }
        /// <summary>
        /// 详细数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!(datagridMeter.SelectedItem is DynamicViewModel dynamic))
            {
                return;
            }

            OneMeterResult meterResult = null;
            if (datagridMeter.SelectedIndex >= 0 && datagridMeter.SelectedIndex <= MetersVM.ResultCollection.ItemsSource.Count)
            {
                meterResult = MetersVM.ResultCollection.ItemsSource[datagridMeter.SelectedIndex];
            }
            if (meterResult == null)
            {
                return;
            }

            LoadMeterDataGrids(meterResult);
            try
            {
                string bar = dynamic.GetProperty("MD_BAR_CODE").ToString();
                string ID = dynamic.GetProperty("MD_EPITOPE").ToString();
                string res = dynamic.GetProperty("MD_RESULT").ToString();
                DetailedData.Instance.Title = $"详细数据--条形码【{bar}】--表位号【{ID}】--{res}";
            }
            catch (Exception)
            {
            }

            DetailedData.Instance.Show();
            if (DetailedData.Instance.WindowState == WindowState.Minimized)
            {
                DetailedData.Instance.WindowState = WindowState.Maximized;
            }


        }
        #region 导出excel文件


        /// <summary>
        /// 加载结论对应的表格
        /// </summary>
        private void LoadMeterDataGrids(OneMeterResult meterResult)
        {
            if (meterResult == null)
            {
                return;
            }
            DetailedData.Instance.pnlMeterResult.Children.Clear();
            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                DataGrid dataGrid = new DataGrid()
                {
                    Margin = new Thickness(3),
                    HeadersVisibility = DataGridHeadersVisibility.All,
                    IsReadOnly = true,
                    Style = Application.Current.Resources["dataGridStyleMeterDetailResult"] as System.Windows.Style,
                };
                meterResult.Categories[i].ResultUnits.Sort(item => GetSortString(item));
                dataGrid.ItemsSource = meterResult.Categories[i].ResultUnits;
                for (int j = 0; j < meterResult.Categories[i].Names.Count; j++)
                {
                    string columnName = meterResult.Categories[i].Names[j];
                    if (columnName == "要检" || columnName == "项目名" || columnName == "项目号")
                    {
                        continue;
                    }
                    DataGridTextColumn column = new DataGridTextColumn()
                    {
                        Header = columnName,
                        Binding = new Binding(columnName),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                    };
                    dataGrid.Columns.Add(column);
                }
                //add yjt zxg 20221207 新增合并张工代码
                dataGrid.PreviewMouseWheel += (sender, e) =>
                {
                    MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = MouseWheelEvent,
                        Source = sender
                    };
                    dataGrid.RaiseEvent(eventArg);
                };
                DetailedData.Instance.pnlMeterResult.Children.Add(dataGrid);
            }
        }
        private string GetSortString(DynamicViewModel dvm)
        {
            if (dvm == null) return "";
            if (!(dvm.GetProperty("项目号") is string sortKeyString)) return "";
            string keyString = sortKeyString.Split('_')[0];
            if (keyString.StartsWith(ProjectID.基本误差试验) || keyString.StartsWith(ProjectID.初始固有误差) || keyString.StartsWith(ProjectID.初始固有误差试验))
            {
                return GetErrorSortString(sortKeyString);
            }
            return sortKeyString;
        }
        private string GetErrorSortString(string keyString)
        {
            if (keyString == null)
            {
                return "";
            }
            string[] arrayTemp = keyString.Split('_');
            if (arrayTemp.Length == 3)
            {
                //数据格式:排序号|误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                string strPara = arrayTemp[2];
                string currentString = strPara.Substring(4, 2);
                strPara = strPara.Remove(4, 2);
                strPara = strPara.Insert(3, currentString);
                return arrayTemp[0] + "_" + arrayTemp[1] + "_" + strPara;
            }
            else if (arrayTemp.Length == 2)
            {
                if (arrayTemp[0] == ProjectID.初始固有误差)
                {
                    //数据格式:功率方向|功率元件|功率因数|电流倍数     11106
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(3, 2);
                    strPara = strPara.Remove(3, 2);
                    currentString = (99 - int.Parse(currentString)).ToString();//电流从小到大
                    strPara = strPara.Insert(2, currentString);
                    return arrayTemp[0] + "_" + strPara;
                }
                else if (arrayTemp[0] == ProjectID.初始固有误差试验 || arrayTemp[0] == ProjectID.基本误差试验)
                {
                    //数据格式:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(4, 2);
                    strPara = strPara.Remove(4, 2);
                    strPara = strPara.Insert(3, currentString);
                    if (strPara[1] == '2')
                        strPara = strPara.Remove(1, 1).Insert(1, "3");
                    else if (strPara[1] == '3')
                        strPara = strPara.Remove(1, 1).Insert(1, "2");
                    return arrayTemp[0] + "_" + strPara;
                }
                else
                {
                    //数据格式:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(4, 2);
                    strPara = strPara.Remove(4, 2);
                    strPara = strPara.Insert(3, currentString);
                    return arrayTemp[0] + "_" + strPara;
                }

            }
            return keyString;
        }

        // 导出明细
        private void OnExportDetail(object sender, RoutedEventArgs e)
        {
            Export(false);

        }
        // 导出汇总
        private void OnExportSummary(object sender, RoutedEventArgs e)
        {
            Export_Summary_Excel_Click();
        }


        // 导出excel文件
        private void Export(bool isSummary)
        {
            var meters = MetersVM.Meters.Where(item => (bool)item.GetProperty("IsSelected"));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                return;
            }
            string pathExcel = System.IO.Directory.GetCurrentDirectory() + @"\Res\Excel";
            using (System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = f.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(f.SelectedPath))
                {
                    pathExcel = f.SelectedPath;
                }
                else
                {
                    return;
                }
            }
            // 汇总
            if (isSummary)
            {
                try
                {
                    Aspose.Cells.Workbook wbook = new Aspose.Cells.Workbook();
                    Aspose.Cells.Worksheet sheet = wbook.Worksheets[0];
                    int row = 0;
                    foreach (DynamicViewModel meter in meters)
                    {
                        string barcode = meter.GetProperty("AVR_BAR_CODE") as string;
                        OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);

                        Save_ExcelSummary(meterResult, wbook, sheet, ref row);

                        //++row;
                    }
                    if (!System.IO.Directory.Exists(pathExcel))
                    {
                        System.IO.Directory.CreateDirectory(pathExcel);
                        System.Threading.Thread.Sleep(500);
                    }
                    string file = Path.Combine(pathExcel, $"汇总_{DateTime.Now:yyyy-MM-ddTHHmmss}.xls");
                    wbook.Save(file, Aspose.Cells.SaveFormat.Excel97To2003);
                    System.Threading.Thread.Sleep(100);
                    MessageBox.Show($"汇总文件导出成功\r\n路径:【{file}】\r\n保存成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"汇总文件导出失败\r\n{ex}");
                }
            }
            else // 详细
            {
                int count = 0;
                foreach (DynamicViewModel meter in meters)
                {
                    string barcode = meter.GetProperty("AVR_BAR_CODE") as string;
                    //OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);
                    OneMeterResult meterResult = MetersVM.ResultCollection.ItemsSource[meter.Index - 1];

                    try
                    {
                        Save_Excel(meterResult, pathExcel);
                        ++count;
                    }
                    catch (Exception ex)
                    {
                        MessageDisplay.Instance.Message = $"文件【{barcode}】保存失败\r\n{ex}";
                    }

                }
                if (count > 0)
                {
                    MessageBox.Show($"共{count}个文件保存成功\r\n路径:{pathExcel}");
                }
            }
        }

        /// <summary>
        /// 多个表导出一个汇总文件
        /// </summary>
        /// <param name="meterResult"></param>
        private void Save_ExcelSummary(OneMeterResult meterResult, Aspose.Cells.Workbook wbook, Aspose.Cells.Worksheet sheet, ref int row)
        {
            DataTable[] dataTables = GetDataTabelS(meterResult);   //获取到这个表的所有项目的datatable

            sheet.Name = "汇总数据";
            Aspose.Cells.Style style = wbook.Styles[wbook.Styles.Add()];
            Aspose.Cells.Style style2 = wbook.Styles[wbook.Styles.Add()];
            style.ForegroundColor = System.Drawing.Color.FromArgb(226, 239, 218);//31, 78, 120
            style.Pattern = Aspose.Cells.BackgroundType.Solid;
            style.Font.IsBold = true;
            style.Font.Color = System.Drawing.Color.FromArgb(0, 0, 0);//255, 255, 255
            style.Font.Name = "宋体";//文字字体 
            style.Font.Size = 15;//文字大小
            style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
            style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框  
            style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
            style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
            //style = sheet.Cells["A1"].GetStyle();


            style2.Pattern = Aspose.Cells.BackgroundType.Solid;
            style2.Font.IsBold = false;
            style2.Font.Name = "宋体";//文字字体 
            style2.Font.Size = 15;//文字大小
            style2.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
            style2.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框  
            style2.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
            style2.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
            //style2 = sheet.Cells["A1"].GetStyle();

            int Start_A;
            int Start_B = 0;
            int End_A;
            int End_B;
            int col = 0;
            //列名
            for (int t = 0; t < dataTables.Length; t++)
            {
                DataTable table = dataTables[t];
                if (table.TableName == "MeterInfo")
                {
                    for (int i = 0; i < table.Columns.Count; i++) //列名
                    {
                        if (row == 0)
                        {
                            sheet.Cells[row, col].Value = table.Columns[i].ColumnName.ToString().Trim();
                            sheet.Cells[row, col].PutValue(table.Columns[i].ColumnName.ToString().Trim());
                            sheet.Cells[row, col].SetStyle(style);
                        }
                        col++;
                    }
                }
                else if (table.TableName == "012")
                {
                    foreach (DataRow Row in table.Rows)
                    {
                        if (row == 0)
                        {
                            sheet.Cells[row, col].Value = Row["项目名"].ToString().Trim();
                            sheet.Cells[row, col].PutValue(Row["项目名"].ToString().Trim());
                            sheet.Cells[row, col].SetStyle(style);
                        }
                        col++;
                    }
                }
                else if (table.TableName == "156")
                {
                    foreach (DataRow Row in table.Rows)
                    {
                        if (row == 0)
                        {
                            sheet.Cells[row, col].Value = Row["项目参数"].ToString().Trim();
                            sheet.Cells[row, col].PutValue(Row["项目参数"].ToString().Trim());
                            sheet.Cells[row, col].SetStyle(style);
                        }
                        col++;
                    }
                }
            }
            End_B = col;

            col = 0;
            Start_A = row;
            if (row == 0)
            {
                row++;
            }
            //End_A = 0;
            for (int t = 0; t < dataTables.Length; t++)
            {
                DataTable table = dataTables[t];

                if (table.TableName == "MeterInfo")
                {
                    foreach (DataRow Row in table.Rows)
                    {
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            sheet.Cells[row, col].Value = Row[i].ToString().Trim();
                            sheet.Cells[row, col].PutValue(Row[i].ToString().Trim());
                            sheet.Cells[row, col].SetStyle(style2);
                            col++;
                        }
                        break;

                    }
                }
                else if (table.TableName == "012")
                {
                    foreach (DataRow Row in table.Rows)
                    {
                        sheet.Cells[row, col].Value = Row["平均值"].ToString().Trim();
                        sheet.Cells[row, col].PutValue(Row["平均值"].ToString().Trim());
                        sheet.Cells[row, col].SetStyle(style2);

                        col++;
                    }
                }
                else if (table.TableName == "156")
                {
                    foreach (DataRow Row in table.Rows)
                    {
                        sheet.Cells[row, col].Value = Row["电量"].ToString().Trim();
                        sheet.Cells[row, col].PutValue(Row["电量"].ToString().Trim());
                        sheet.Cells[row, col].SetStyle(style2);

                        col++;
                    }
                }
            }
            row++;
            End_A = row - Start_A;


            var range = sheet.Cells.CreateRange(Start_A, Start_B, End_A, End_B);
            range.SetOutlineBorder(Aspose.Cells.BorderType.TopBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Black);
            range.SetOutlineBorder(Aspose.Cells.BorderType.BottomBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Black);
            range.SetOutlineBorder(Aspose.Cells.BorderType.LeftBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Black);
            range.SetOutlineBorder(Aspose.Cells.BorderType.RightBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Black);


            sheet.AutoFitColumns();


        }

        private void Save_Excel(OneMeterResult meterResult, string pathExcel)
        {
            DataTable[] dataTables = GetDataTabelS(meterResult);   //获取到这个表的所有项目的datatable

            Aspose.Cells.Workbook wbook = new Aspose.Cells.Workbook();
            Aspose.Cells.Worksheet sheet = wbook.Worksheets[0];
            sheet.Name = "数据";
            int row = 0;
            int col = 0;
            Aspose.Cells.Style style = wbook.Styles[wbook.Styles.Add()];
            Aspose.Cells.Style style2 = wbook.Styles[wbook.Styles.Add()];
            style.ForegroundColor = System.Drawing.Color.FromArgb(31, 78, 120);
            style.Pattern = Aspose.Cells.BackgroundType.Solid;
            style.Font.IsBold = true;
            style.Font.Color = System.Drawing.Color.FromArgb(255, 255, 255);
            style.Font.Name = "宋体";//文字字体 
            style.Font.Size = 15;//文字大小
            style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
            style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框  
            style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
            style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
                                                                                                 //style = sheet.Cells["A1"].GetStyle();


            style2.Pattern = Aspose.Cells.BackgroundType.Solid;
            style2.Font.IsBold = false;
            style2.Font.Name = "宋体";//文字字体 
            style2.Font.Size = 15;//文字大小
            style2.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
            style2.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框  
            style2.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
            style2.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
                                                                                                  //style2 = sheet.Cells["A1"].GetStyle();

            int Start_A;
            int Start_B;
            int End_A;
            int End_B;
            //cells.GetColumnWidthPixel
            for (int t = 0; t < dataTables.Length; t++)
            {
                DataTable table = dataTables[t];
                Start_A = row;
                Start_B = col;

                for (int i = 0; i < table.Columns.Count; i++) //列名
                {
                    sheet.Cells[row, col].Value = table.Columns[i].ColumnName.ToString().Trim();
                    sheet.Cells[row, col].PutValue(table.Columns[i].ColumnName.ToString().Trim());
                    sheet.Cells[row, col].SetStyle(style);
                    //sheet.Cells[row, col].
                    col++;
                }
                End_B = col;
                col = 0;
                row++;
                int index = 1;
                End_A = 1;
                foreach (DataRow Row in table.Rows)
                {

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        sheet.Cells[row, col].Value = Row[i].ToString().Trim();
                        sheet.Cells[row, col].PutValue(Row[i].ToString().Trim());
                        if (index % 2 == 0)
                        {
                            style2.ForegroundColor = System.Drawing.Color.FromArgb(254, 255, 255);
                        }
                        else
                        {
                            style2.ForegroundColor = System.Drawing.Color.FromArgb(216, 230, 243);
                        }
                        sheet.Cells[row, col].SetStyle(style2);
                        //System.Threading.Thread.Sleep(2);
                        col++;
                    }
                    col = 0;
                    row++;
                    index++;
                    End_A++; ;
                }



                var range = sheet.Cells.CreateRange(Start_A, Start_B, End_A, End_B);
                range.SetOutlineBorder(Aspose.Cells.BorderType.TopBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Blue);
                range.SetOutlineBorder(Aspose.Cells.BorderType.BottomBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Blue);
                range.SetOutlineBorder(Aspose.Cells.BorderType.LeftBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Blue);
                range.SetOutlineBorder(Aspose.Cells.BorderType.RightBorder, Aspose.Cells.CellBorderType.Thick, System.Drawing.Color.Blue);
                row++;
            }


            sheet.AutoFitColumns();


            //限制最大列宽
            for (int i = 0; i < 10; i++)
            {
                //当前列宽
                int pixel = sheet.Cells.GetColumnWidthPixel(i);
                //设置最大列宽
                if (pixel > 180)
                {
                    sheet.Cells.SetColumnWidthPixel(i, 180);
                }
                else if (pixel < 80)
                    sheet.Cells.SetColumnWidthPixel(i, 100);
                else
                {
                    sheet.Cells.SetColumnWidthPixel(i, pixel);
                }
                //if (i == 0)
                //{
                //    sheet.Cells.SetColumnWidthPixel(i, 150);

                //}
                //else
                //{ 
                //sheet.Cells.SetColumnWidthPixel(i, 120);

                //}

            }
            //System.IO.MemoryStream ms = wbook.SaveToStream();

            //string file = @"C:\Users\zhang\Desktop\excel\1.xls";
            if (!System.IO.Directory.Exists(pathExcel))
            {
                System.IO.Directory.CreateDirectory(pathExcel);
                System.Threading.Thread.Sleep(500);
            }
            string barcode = meterResult.MeterInfo.GetProperty("MD_BAR_CODE")?.ToString();
            string file = Path.Combine(pathExcel, $"{barcode}_{DateTime.Now:yyyy-MM-ddTHHmmss.fff}.xls");

            wbook.Save(file, Aspose.Cells.SaveFormat.Excel97To2003);
            System.Threading.Thread.Sleep(100);
            //MessageBox.Show($"文件路径:【{file}】保存成功");
            //System.Diagnostics.Process.Start(file); //打开excel文件

        }

        /// <summary>
        /// 获取表
        /// </summary>
        private DataTable[] GetDataTabelS(OneMeterResult meterResult)
        {
            if (meterResult == null)
            {
                return null;
            }
            DataTable[] dataTables = new DataTable[meterResult.Categories.Count + 1];

            DataTable dataTable = new DataTable
            {
                TableName = "MeterInfo"
            };
            //先添加表数据
            for (int i = 0; i < datagridMeter.Columns.Count; i++)
            {
                dataTable.Columns.Add(datagridMeter.Columns[i].Header.ToString());//构建表头 
            }
            DataRow row2 = dataTable.NewRow();
            Dictionary<string, string> result = MetersVM.GetResult(meterResult);
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                row2[i] = meterResult.MeterInfo.GetProperty(datagridMeter.Columns[i].SortMemberPath);
                if (result.ContainsKey(datagridMeter.Columns[i].SortMemberPath))
                {
                    row2[i] = result[datagridMeter.Columns[i].SortMemberPath];
                }
            }
            dataTable.Rows.Add(row2);
            dataTables[0] = dataTable;


            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                dataTable = new DataTable
                {
                    TableName = meterResult.Categories[i].ViewName
                };

                dataTable.Columns.Add("项目名");//构建表头 
                for (int j = 0; j < meterResult.Categories[i].Names.Count; j++)
                {
                    string columnName = meterResult.Categories[i].Names[j];
                    if (columnName == "要检" || columnName == "项目号" || columnName == "项目名")
                    {
                        continue;
                    }
                    dataTable.Columns.Add(columnName);//构建表头 
                }
                meterResult.Categories[i].ResultUnits.Sort(item => GetSortString(item));
                AsyncObservableCollection<DynamicViewModel> s = meterResult.Categories[i].ResultUnits;
                for (int q = 0; q < s.Count; q++)
                {
                    DataRow row = dataTable.NewRow();
                    for (int j = 0; j < dataTable.Columns.Count; j++)   //循环所有列
                    {
                        row[j] = s[q].GetProperty(dataTable.Columns[j].ColumnName) as string;
                    }
                    dataTable.Rows.Add(row);
                }
                dataTables[i + 1] = dataTable;
            }
            return dataTables;

        }
        #endregion

        private void IsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox ck)
            {
                if (ck.DataContext is DynamicViewModel vm)
                {
                    if (ck.IsChecked.HasValue && ck.IsChecked.Value)
                    {
                        vm.SetProperty("IsSelected", true);
                    }
                    else
                    {
                        vm.SetProperty("IsSelected", false);
                    }
                }
            }
        }
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //DetailedData.Instance.Show();
        }

        /// <summary>
        /// 是否正在上传
        /// </summary>
        bool IsSendTask = false;
        private string UpDownUri = "MDS";

        // 上传数据
        private void OnUpMds(object sender, RoutedEventArgs e)
        {
            UpDownUri = "MDS";
            if (!IsSendTask)
            {
                IsSendTask = true;
                System.Threading.Tasks.Task.Factory.StartNew(() => SendTestData());
            }
            else
            {
                MessageBox.Show("正在上传数据请稍后");
            }
        }

        // 上传数据
        private void OnUpMis(object sender, RoutedEventArgs e)
        {
            UpDownUri = "营销";
            if (!IsSendTask)
            {
                IsSendTask = true;
                System.Threading.Tasks.Task.Factory.StartNew(() => SendTestData());
            }
            else
            {
                MessageBox.Show("正在上传数据请稍后");
            }
        }


        /// <summary>
        /// 上传检定数据
        /// </summary>
        private void SendTestData()
        {
            //add yjt 20220422 提示放在循环外面 新增上传数量
            bool bUpdateOk = false;
            int iUpdateOkSum = 0;
            int iUpdateSum = 0;
            string msg = "";
            //add yjt 20220428 新增每个表是否上传成功上传提示
            string msgs = "";

            var meters = MetersVM.Meters.Where(item => (bool)item.GetProperty("IsSelected"));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                IsSendTask = false;
                return;
            }
            string Tips = "";
            //add yjt 20220805 新增获取上传的总表位
            int UpMeterCount = 0;
            Dictionary<string, List<DynamicViewModel>> taskMeters = new Dictionary<string, List<DynamicViewModel>>();//多任务集合
            foreach (DynamicViewModel meter in meters)
            {
                string barcode = meter.GetProperty("MD_BAR_CODE") as string;
                Tips += $"【{barcode}】\r\n";
                string taskno = meter.GetProperty("MD_TASK_NO") as string;
                if (string.IsNullOrWhiteSpace(taskno)) taskno = "0";
                if (!taskMeters.ContainsKey(taskno))
                {
                    taskMeters.Add(taskno, new List<DynamicViewModel>());
                }
                taskMeters[taskno].Add(meter);

                UpMeterCount++;
            }

            if (MessageBox.Show($"是否确定上传 {UpMeterCount} 条数据,  共 {taskMeters.Count} 个任务", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                IsSendTask = false;
                return;
            }

            Mis.Common.IMis mis = Mis.MISFactory.Create(UpDownUri);
            if (mis == null)
            {
                LogHelper.WriteLog($"上传错误【mis:null】");

                IsSendTask = false;
                return;
            }
            try
            {
                Window_Wait.Instance.StateWait("正在上传数据,请等待", UpMeterCount * 65000);
                int[] okcount = new int[taskMeters.Count];

                LogHelper.WriteLog($"上传表数据【taskMeters.count :{taskMeters.Count}】");

                for (int i = 0; i < taskMeters.Count; i++)
                {
                    var item = taskMeters.ElementAt(i);
                    foreach (DynamicViewModel meter in item.Value)
                    {
                        OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);
                        Core.Model.Meter.TestMeterInfo temmeter2 = MeterDataHelper.GetDnbInfoNew(meterResult);
                        MessageDisplay.Instance.Message = $"正在上传【{temmeter2.MD_BarCode}】检定数据，请稍后...";

                        iUpdateSum++;

                        bUpdateOk = mis.Update(temmeter2);

                        LogHelper.WriteLog($"上传结论【barcode:{temmeter2.MD_BarCode}, 结论:{bUpdateOk}】");


                        if (bUpdateOk)
                        {
                            iUpdateOkSum++;
                            okcount[i]++;
                            //add yjt 20220428 新增每个表是否上传成功上传提示
                            Utility.Log.LogManager.AddMessage($"电能表[{temmeter2.MD_BarCode}]检定记录上传成功!", Utility.Log.EnumLogSource.数据库存取日志, Utility.Log.EnumLevel.Warning);
                            MessageDisplay.Instance.Message = $"电能表[{temmeter2.MD_BarCode}]检定记录上传成功!";
                            meter.SetProperty("MD_OTHER_2", "已上传");
                            SetMeterData(meter);
                            msgs += $"电能表[{temmeter2.MD_BarCode}]上传成功!\r\n";

                        }
                        else
                        {
                            Utility.Log.LogManager.AddMessage($"电能表[{temmeter2.MD_BarCode}]检定记录上传失败!", Utility.Log.EnumLogSource.数据库存取日志, Utility.Log.EnumLevel.TipsError);
                            MessageDisplay.Instance.Message = $"电能表[{temmeter2.MD_BarCode}]检定记录上传失败!";
                            msgs += $"电能表[{temmeter2.MD_BarCode}]上传失败!\r\n";
                        }
                    }
                    if (!mis.UpdateCompleted())
                    {
                        okcount[i] = 0;
                    }
                }
                int iUpdateFalseSum = iUpdateSum - iUpdateOkSum;
                iUpdateFalseSum = iUpdateSum - okcount.Sum();
                Window_Wait.Instance.EndWait();
                msg = $"共上传【{iUpdateSum}】块数据，上传成功【{iUpdateOkSum}】块，上传失败【{iUpdateFalseSum}】块。";
                MessageDisplay.Instance.FinishedInfo = msg;
            }
            catch (Exception ex)
            {
                Window_Wait.Instance.EndWait();
                MessageBox.Show($"上传数据失败,{ex}");
                IsSendTask = false;
            }
            IsSendTask = false;

        }


        /// <summary>
        /// 修改数据库里面的上传标识
        /// </summary>
        /// <param name="meter"></param>
        private void SetMeterData(DynamicViewModel meter)
        {
            List<string> fieldNames = new List<string>() { "MD_OTHER_2" };//更新上传的状态
            List<DynamicModel> models = new List<DynamicModel>();
            DynamicModel model = new DynamicModel();
            model.SetProperty("METER_ID", meter.GetProperty("METER_ID"));
            model.SetProperty("MD_OTHER_2", meter.GetProperty("MD_OTHER_2"));
            models.Add(model);
            int updateCount = DALManager.MeterDbDal.Update("METER_INFO", "METER_ID", models, fieldNames);
            Console.WriteLine($"更新表METER_INFO影响行数：{updateCount}");
        }

        // 模板制作
        private void Btn_Model_Click(object sender, RoutedEventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            Process processTemp = processes.FirstOrDefault(item => item.ProcessName == "LYTest.WordTemplate");
            if (processTemp == null)
            {
                try
                {
                    Process.Start(string.Format(@"{0}\{1}", Directory.GetCurrentDirectory(), "LYTest.WordTemplate.exe"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("模板编辑工具启动失败" + ex);
                }
            }
            else
            {
                return;
            }
        }

        private Dictionary<string, string> DeviceValue;
        private string EmptyMark = "/";
        private bool PlusSignMark = false;//加+号

        // 打印
        private void OnReportPrint(object sender, RoutedEventArgs e)
        {
            ReportPrint(false);
        }

        // 预览
        private void OnReportPreview(object sender, RoutedEventArgs e)
        {
            ReportPrint(true);
        }
        /// <summary>
        /// 打印报告
        /// </summary>
        private void ReportPrint(bool isPreview)
        {
            var meters = MetersVM.Meters.Where(item => (bool)item.GetProperty("IsSelected"));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                return;
            }
            string Tips = "";
            int aa = 0;
            foreach (DynamicViewModel meter in meters)
            {
                string barcode = meter.GetProperty("MD_BAR_CODE") as string;
                Tips += $"【{barcode}】\r\n";
                ++aa;
            }
            string msg = "是否确定打印表:";
            if (isPreview)
            {
                msg = "是否确定预览表:";
            }
            if (aa > 1 && isPreview)
            {
                msg = $@"勾选了多块表，同时只能预览一块表；要打印多块表，请不勾选预览。{Environment.NewLine}批量保存在：{Directory.GetCurrentDirectory()}\Res\Word\Report\";
                MessageBox.Show(msg, "", MessageBoxButton.OK);
                return;
            }
            if (MessageBox.Show(msg + " " + aa + " 条数据", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            //查找对应模板的所有书签M_是表信息,D_是台体信息，R_是结论数据
            string path = string.Format(@"{0}\Res\Word\{1}", Directory.GetCurrentDirectory(), comboBoxTemplates.Text);
            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("未找到模板文件:" + path);
            }
            Window_Wait.Instance.StateWait("正在打印报表,请等待", 65000);


            string DeviceDataPath = System.IO.Directory.GetCurrentDirectory() + "\\Ini\\DeviceData.ini";
            string[] NameS = Core.OperateFile.GetINI("Data", "名称", DeviceDataPath).Split('|');
            string[] Values = Core.OperateFile.GetINI("Data", "值", DeviceDataPath).Split('|');

            DeviceValue = new Dictionary<string, string>();

            for (int i = 0; i < NameS.Length; i++)
            {
                if (!DeviceValue.ContainsKey(NameS[i]))
                {
                    DeviceValue.Add(NameS[i], Values[i]);
                }
            }
            //检定依据
            for (int i = 0; i < Equipments.Instance.ReguNames.Count; i++)
            {
                if (!DeviceValue.ContainsKey(Equipments.Instance.ReguNames[i]))
                {
                    string avalue = Core.OperateFile.GetINI("Regulation", Equipments.Instance.ReguNames[i], DeviceDataPath);
                    DeviceValue.Add(Equipments.Instance.ReguNames[i], avalue);
                }
            }
            string[] AddNames = Core.OperateFile.GetINI("Regulation", "名称", DeviceDataPath).Split('|'); ;
            for (int i = 0; i < AddNames.Length; i++)
            {
                if (!DeviceValue.ContainsKey(AddNames[i]))
                {
                    string avalue = Core.OperateFile.GetINI("Regulation", AddNames[i], DeviceDataPath);
                    DeviceValue.Add(AddNames[i], avalue);
                }
            }

            //替换符
            if (!Core.OperateFile.ExistINIKey("MarkFlag", "EmptyMark", DeviceDataPath))
            {
                EmptyMark = "/";
            }
            else
            {
                EmptyMark = Core.OperateFile.GetINI("MarkFlag", "EmptyMark", DeviceDataPath);
            }
            //+-0
            if (!Core.OperateFile.ExistINIKey("MarkFlag", "PlusSignMark", DeviceDataPath))
            {
                PlusSignMark = false;
            }
            else
            {
                PlusSignMark = Core.OperateFile.GetINI("MarkFlag", "PlusSignMark", DeviceDataPath).ToUpper() == "YES";
            }


            var dat = meters.ToList();
            for (int i = 0; i < dat.Count; i++)
            {
                OneMeterResult meterResult = MetersVM.ResultCollection.ItemsSource[dat[i].Index - 1];
                if (meterResult != null)
                {
                    Print(path, meterResult, isPreview);
                }
            }
            Window_Wait.Instance.EndWait();
            if (!isPreview)
            {
                MessageBox.Show($@"保存成功。{Environment.NewLine}在：{Directory.GetCurrentDirectory()}\Res\Word\Report\");
            }
        }

        private void Print(string path, OneMeterResult meterResult, bool preview = false)
        {
            try
            {
                //add yjt 20220315 新增报表文件名称字段
                string barcode = meterResult.MeterInfo.GetProperty("MD_BAR_CODE") as string;
                DateTime time = DateTime.Parse(meterResult.MeterInfo.GetProperty("MD_TEST_DATE") as string);
                string bb = "报表";
                if (path.IndexOf("证书") != -1)
                {
                    bb = "检定证书";
                }
                else if (path.IndexOf("记录") != -1)
                {
                    bb = "原始记录";
                }
                else if (path.IndexOf("通知书") != -1)
                {
                    bb = "通知书";
                }

                Document doc = new Document(path);
                if (doc == null)
                {
                    MessageBox.Show(string.Format(@"未能找到对应 {0}的模板文件...", path), "打印失败");
                    return;
                }

                foreach (Bookmark mark in doc.Range.Bookmarks)
                {
                    string name = mark.Text; //书签的关键字
                    if (name.StartsWith("M_")) //表信息
                    {
                        //这里需要传入Mark和表的信息,替换Mark的text就可以
                        ReplaceMeterInfo(mark, meterResult);
                    }
                    else if (name.StartsWith("D_")) //台体信息
                    {
                        ReplaceDeviceInfo(mark, DeviceValue, meterResult);
                    }
                    else if (name.StartsWith("R_")) //结论数据
                    {
                        ReplaceResult(mark, meterResult);
                    }
                    //add yjt 20220630 新增分合元差的计算
                    else if (name.StartsWith("C_")) //分合元差
                    {
                        ReplaceResultCha(mark, meterResult);
                    }
                }
                //doc.AppendDocument(doc, ImportFormatMode.KeepSourceFormatting);

                //这里进行公式计算
                FormulaCalculation(doc);

                Document docPrint = doc;

                //add
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Res\Word\Tem"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Res\Word\Tem");
                    System.Threading.Thread.Sleep(500);
                }

                //是否预览 --预览同一时间只能支持一个,而打印可以多个。所以临时文件分为俩个
                if (preview)
                {
                    //临时文件                
                    string tmpPath = string.Format(@"{0}\Res\Word\Tem\{1}", Directory.GetCurrentDirectory(), "tmp.doc");
                    if (!File.Exists(tmpPath))
                    {
                        File.Create(path);
                    }
                    if (!IsFileReady(tmpPath)) //预览的话判断一下文件是不是被占用了
                    {
                        MessageBox.Show("预览文件同一时间只能打开一个,如需预览请关闭之前打开的预览文件,重新预览");
                        return;
                    }
                    doc.Save(tmpPath, Aspose.Words.SaveFormat.Doc);
                    Process.Start(tmpPath);  //预览文件               
                }
                else
                {
                    //临时文件
                    string tmpPath = string.Format(@"{0}\Res\Word\Tem\{1}", Directory.GetCurrentDirectory(), "printTmp.doc");
                    if (!File.Exists(tmpPath))
                    {
                        File.Create(path);
                    }
                    doc.Save(tmpPath, Aspose.Words.SaveFormat.Doc);
                    //docPrint.Print();  //delete yjt 20220326 注释打印机直接打印
                    docPrint.Clone();
                }

                //add yjt 20220315 新增报表保存路径
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\Res\Word\Report\" + DateTime.Now.ToString("yyyyMMdd")))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Res\Word\Report\" + DateTime.Now.ToString("yyyyMMdd"));
                    System.Threading.Thread.Sleep(500);
                }
                string file = string.Format(@"{0}\Res\Word\Report\{1}\{2}_{3}_{4}.doc", Directory.GetCurrentDirectory(), DateTime.Now.ToString("yyyyMMdd"), barcode, bb, time.ToString("yyyyMMdd"));
                doc.Save(file, Aspose.Words.SaveFormat.Doc);
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"请检查文件模板，路径：{path}\r\n打印异常:{ex}");
            }
        }

        /// <summary>
        /// 判断文件是否占用
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private bool IsFileReady(string filepath)
        {
            if (System.IO.File.Exists(filepath) == false)
            {
                return true;
            }
            try
            {
                System.IO.File.Open(filepath, FileMode.Open).Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region 替换数据
        /// <summary>
        /// 替换表信息
        /// </summary>
        /// <param name="mark"></param>
        /// <param name="meterResult"></param>
        private void ReplaceMeterInfo(Bookmark mark, OneMeterResult meterResult)
        {
            DynamicViewModel meterInfo = meterResult.MeterInfo;
            string name = mark.Text.Substring(2);
            string HandelString = "";
            if (name.Substring(name.LastIndexOf('_') + 1).IndexOf("$") != -1) //判断是否需要代码解析
            {
                HandelString = name.Substring(name.LastIndexOf('_') + 1);
                name = name.Substring(0, name.LastIndexOf('_'));//排除代码那部分
            }
            var s = meterInfo.GetProperty(name);
            string value = EmptyMark;
            if (s != null)
            {
                value = s.ToString();

                var jxfs = meterInfo.GetProperty("MD_WIRING_MODE");
                //电压
                if (name == "MD_UB")
                {
                    if (jxfs.ToString() != "单相")
                    {
                        if (value == "57.7")
                        {
                            value = "3×57.7/100";
                        }
                        else if (value == "100")
                        {
                            value = "3×100";
                        }
                        else if (value == "220")
                        {
                            value = "3×220/380";
                        }
                    }
                }

                //电流
                if (name == "MD_UA")
                {
                    var gc = meterInfo.GetProperty("MD_JJGC");
                    if (jxfs.ToString() != "单相")
                    {
                        if (gc.ToString() != "IR46")
                        {
                            value = "3×" + value;
                        }
                        else
                        {
                            value = "3×" + value;
                        }
                    }
                }

                //常数
                if (name == "MD_CONSTANT")
                {
                    if (jxfs.ToString() != "单相")
                    {
                        value = value.Split('(')[0] + "imp/kWh" + " " + value.Split('(')[1].TrimEnd(')') + "imp/kvarh";
                    }
                    else
                    {
                        value += "imp/kWh";
                    }
                }
                //是否经互感器
                if (name == "MD_CONNECTION_FLAG")
                {
                    if (value == "直接式")
                    {
                        value = "直接接入";
                    }
                    else if (value == "互感式")
                    {
                        value = "经互感器接入";
                    }
                }
                //if (name == "MD_TEST_DATE")
                //{
                //    string r = s.ToString().Substring(0, 9);
                //    value = r.ToString();
                //}
                if (name == "MD_GRADE")
                {
                    if (value == "1.0(2.0)")
                    {
                        value = "1.0/2.0";
                    }
                }
            }
            if (HandelString != "")
            {
                try
                {
                    value = StringHandel(value, HandelString);
                }
                catch (Exception ex)
                {
                    MessageDisplay.Instance.Message = string.Format($"书签{mark.Text}字符串处理时失败  " + ex.ToString());
                }

            }
            try
            {
                mark.Text = value ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// 替换台体信息
        /// </summary>
        /// <param name="mark"></param>
        /// <param name="meterResult"></param>
        private void ReplaceDeviceInfo(Bookmark mark, Dictionary<string, string> DeviceValue, OneMeterResult meterResult)
        {
            string name = mark.Text.Substring(2);
            string HandelString = "";
            if (name.Substring(name.LastIndexOf('_') + 1).IndexOf("$") != -1) //判断是否需要代码解析
            {
                HandelString = name.Substring(name.LastIndexOf('_') + 1);
                name = name.Substring(0, name.LastIndexOf('_'));//排除代码那部分
            }
            string newName = name;
            if (Equipments.Instance.ReguNames.Contains(name))
            {
                if (meterResult.MeterInfo.GetProperty("MD_WIRING_MODE") as string == "单相")
                {
                    if (meterResult.MeterInfo.GetProperty("MD_FKTYPE") as string == "本地费控")
                        newName = Equipments.Instance.ReguNames[0];
                    else
                        newName = Equipments.Instance.ReguNames[1];
                }
                else
                {
                    if (meterResult.MeterInfo.GetProperty("MD_FKTYPE") as string == "本地费控")
                        newName = Equipments.Instance.ReguNames[2];
                    else
                        newName = Equipments.Instance.ReguNames[3];
                }
            }
            string s = null;
            if (DeviceValue.ContainsKey(newName))
            {
                s = DeviceValue[newName];
            }
            string value = EmptyMark;
            if (s != null)
            {
                value = s.ToString();
            }
            if (HandelString != "")
            {
                try
                {
                    value = StringHandel(value, HandelString);
                }
                catch (Exception ex)
                {
                    MessageDisplay.Instance.Message = string.Format($"书签{mark.Text}字符串处理时失败  " + ex.ToString());
                }

            }
            mark.Text = value ?? "";
        }
        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="mark"></param>
        /// <param name="meterResult"></param>
        private void ReplaceResult(Bookmark mark, OneMeterResult meterResult)
        {
            string value = EmptyMark;
            string tem = mark.Text.Substring(2);
            string HandelString = "";
            if (tem.Substring(tem.LastIndexOf('_') + 1).IndexOf("$") != -1) //判断是否需要代码解析
            {
                HandelString = tem.Substring(tem.LastIndexOf('_') + 1);
                tem = tem.Substring(0, tem.LastIndexOf('_'));//排除代码那部分
            }

            string ID = tem.Substring(0, tem.LastIndexOf('_'));
            string Name = tem.Substring(tem.LastIndexOf('_') + 1);
            bool T = false;
            if (Name == "总结论")
            {
                string ZID = tem.Split('_')[0];
                //就需要获取到项目号是这个的所有项目
                string Resoult = ConstHelper.合格;
                bool isOk = false;
                for (int i = 0; i < meterResult.Categories.Count; i++)
                {
                    OneMeterResult.ViewCategory view = meterResult.Categories[i];
                    for (int j = 0; j < view.ResultUnits.Count; j++)
                    {
                        var s = view.ResultUnits[j].GetProperty("项目号");
                        string id = "";
                        if (s != null)
                        {
                            id = s.ToString().Split('_')[0];   //这个就是大项目的编号
                        }
                        if (id == ZID)
                        {
                            isOk = true;
                            s = view.ResultUnits[j].GetProperty("结论");
                            if (s != null)
                            {
                                if (s.ToString() == "不合格")
                                {
                                    Resoult = "不合格";
                                }
                            }
                        }
                    }
                    if (isOk)
                    {
                        mark.Text = Resoult ?? "";
                        break;
                    }
                }
                //add yjt 20220630 没做的功能默认为/
                if (!isOk)
                {
                    mark.Text = EmptyMark;
                }
                return;
            }
            //string levels = meterResult.MeterInfo.GetProperty("MD_GRADE") as string;
            //float qlev = 2;
            //if (!string.IsNullOrWhiteSpace(levels) && levels.IndexOf("(") > 0)
            //{
            //    if (!float.TryParse(levels.Split('(')[1].Replace(")", ""), out qlev))
            //    {
            //        qlev = 2;
            //    }
            //}
            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                OneMeterResult.ViewCategory view = meterResult.Categories[i];
                for (int j = 0; j < view.ResultUnits.Count; j++)
                {
                    try
                    {
                        var s = view.ResultUnits[j].GetProperty("项目号");
                        string id = "";
                        if (s != null)
                        {
                            id = s.ToString();
                        }
                        if (id == ID)
                        {
                            s = view.ResultUnits[j].GetProperty(Name);
                            object avgTmp = view.ResultUnits[j].GetProperty("平均值");

                            if (s == null) s = "";

                            if (Name == "误差1" || Name == "误差2" || Name == "误差3" || Name == "误差4" || Name == "误差5")
                            {
                                value = ProcessPlusSign(s, avgTmp);
                            }
                            else if (Name == "化整值")
                            {
                                if (view.ResultUnits[j].GetProperty("功率方向") != null)
                                {
                                    value = ProcessPlusSign(s, avgTmp);
                                }
                                else
                                {
                                    value = ProcessPlusSign(s, avgTmp);
                                }
                            }
                            else if (Name == "平均值")
                            {
                                value = ProcessPlusSign(s, avgTmp);
                            }
                            else if (s != null)
                            {
                                value = s.ToString();
                            }
                            T = true;
                            break;
                        }
                    }
                    catch
                    {
                        value = "";
                    }
                }
                if (T) break;
            }
            if (HandelString != "")
            {
                try
                {
                    value = StringHandel(value, HandelString);
                }
                catch (Exception ex)
                {
                    MessageDisplay.Instance.Message = string.Format($"书签{mark.Text}字符串处理时失败  " + ex.ToString());
                }

            }
            mark.Text = value ?? "";
        }
        /// <summary>
        /// 处理加符号，全加；不加符号，只去0的
        /// </summary>
        /// <param name="s"></param>
        /// <param name="average"></param>
        /// <returns></returns>
        private string ProcessPlusSign(object s, object average)
        {
            bool positive;
            if (average == null) positive = true;
            else
            {
                if (float.TryParse(average as string, out float atmp))
                    positive = atmp >= 0;
                else
                    positive = true;
            }
            string value;
            if (s == null || string.IsNullOrWhiteSpace(s.ToString()))
            {
                value = "";
            }
            else
            {
                if (PlusSignMark)//加符号，全加
                {
                    if (s.ToString().Contains("+") || s.ToString().Contains("-"))
                        value = s.ToString();
                    else
                    {
                        if (positive)
                            value = "+" + s.ToString();
                        else
                            value = "-" + s.ToString();
                    }
                }
                else//不加符号，只去0的
                {
                    if (string.IsNullOrWhiteSpace(s.ToString().Replace("0", "").Replace(".", "").Replace("+", "").Replace("-", "")))
                        value = s.ToString().Replace("+", "").Replace("-", "");
                    else
                        value = s.ToString();
                }
            }

            return value;
        }

        //add yjt 20220630 新增分合元差的计算
        private void ReplaceResultCha(Bookmark mark, OneMeterResult meterResult)
        {
            string value = EmptyMark;
            string tem = mark.Text.Substring(2);
            string HandelString = "";
            if (tem.Substring(tem.LastIndexOf('_') + 1).IndexOf("$") != -1) //判断是否需要代码解析
            {
                HandelString = tem.Substring(tem.LastIndexOf('_') + 1);
                tem = tem.Substring(0, tem.LastIndexOf('_'));//排除代码那部分
            }

            string ID = tem.Substring(0, tem.LastIndexOf('_'));
            string Name = tem.Substring(tem.LastIndexOf('_') + 1);


            for (int p = 0; p < meterResult.Categories.Count; p++)
            {
                if (meterResult.Categories[p].ViewName == "012")
                {
                    OneMeterResult.ViewCategory view1 = meterResult.Categories[p];
                    for (int q = 0; q < view1.ResultUnits.Count; q++)
                    {
                        try
                        {
                            var s = view1.ResultUnits[q].GetProperty(Name);
                            var name = view1.ResultUnits[q].GetProperty("项目名");

                            string[] glfx = ID.Split('_');

                            if (name.ToString().IndexOf(glfx[0] + "_H_1.0_Ib") != -1)
                            {
                                var h = view1.ResultUnits[q].GetProperty(Name);
                                var pjz = view1.ResultUnits[q].GetProperty("平均值");

                                for (int m = 0; m < view1.ResultUnits.Count; m++)
                                {
                                    var name1 = view1.ResultUnits[m].GetProperty("项目名");
                                    int xsw = 2; //小数位
                                    if (name1.ToString().IndexOf(ID + "_1.0_Ib") != -1 && ID.IndexOf(ID) != -1)
                                    {
                                        s = view1.ResultUnits[m].GetProperty(Name);
                                        var pjz2 = view1.ResultUnits[m].GetProperty("平均值");
                                        xsw = s.ToString().Split('.')[1].Length;
                                        value = (float.Parse(s.ToString()) - float.Parse(h.ToString())).ToString("f" + xsw);
                                        float jzjfh = float.Parse(pjz2.ToString()) - float.Parse(pjz.ToString());
                                        if (jzjfh > 0 && value.IndexOf("+") < 0 && value.IndexOf("-") < 0)
                                        {
                                            value = $"+{value}";
                                        }
                                        if (Name == "平均值")
                                        {
                                            value = ProcessPlusSign(value, value);
                                        }
                                        else if (Name == "化整值")
                                        {
                                            value = ProcessPlusSign(value, jzjfh);
                                        }
                                        else
                                        {
                                            value = ProcessPlusSign(value, value);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            value = "";
                        }
                    }
                }
            }
            if (HandelString != "")
            {
                try
                {
                    value = StringHandel(value, HandelString);
                }
                catch (Exception ex)
                {
                    MessageDisplay.Instance.Message = string.Format($"书签{mark.Text}字符串处理时失败  " + ex.ToString());
                }

            }
            mark.Text = value ?? "";
        }


        /// <summary>
        /// 字符串处理
        /// </summary>
        /// <param name="value">现在的值</param>
        /// <param name="str">处理方式</param>
        /// <returns></returns>
        private string StringHandel(string value, string handelStr)
        {
            string str = handelStr.Substring(1, 1);  //&Tyyyy年MM月dd日  &V(|)    &S(|,,1)
            string format = handelStr.Substring(2);//字符串格式，根据需要在分割
            string temValue = value;
            if (string.IsNullOrWhiteSpace(value) || value == EmptyMark) return temValue;
            switch (str)
            {
                case "T"://日期格式化  tostring  //M_MD_TEST_DATE_$THH:mm:ss=AddDay1
                    if (format.IndexOf("=") >= 0)//                 HH:mm:ss=AddDay1
                    {
                        string function = format.Split('=')[1];
                        format = format.Split('=')[0];
                        if (function.ToLower().IndexOf("addday") >= 0)//AddDay1
                        {
                            int day = 0;
                            if (function.Length > 6)
                            {
                                int.TryParse(function.Substring(6), out day);
                            }
                            temValue = Convert.ToDateTime(value).AddDays(day).ToString(format);
                        }
                        else if (function.ToLower().IndexOf("addmonth") >= 0)//AddDay1
                        {
                            int month = 0;
                            if (function.Length > 6)
                            {
                                int.TryParse(function.Substring(6), out month);
                            }
                            temValue = Convert.ToDateTime(value).AddMonths(month).ToString(format);
                        }
                        else if (function.ToLower().IndexOf("addyear") >= 0)//AddDay1
                        {
                            int year = 0;
                            if (function.Length > 6)
                            {
                                int.TryParse(function.Substring(6), out year);
                            }
                            temValue = Convert.ToDateTime(value).AddYears(year).ToString(format);
                        }
                        else
                            temValue = Convert.ToDateTime(value).ToString(format);
                    }
                    else
                        temValue = Convert.ToDateTime(value).ToString(format);
                    break;
                case "S"://字符串分割  splite('/')[0]
                         //以最后一个符号作为分割，前面的是分割的字符串，后面的是下标
                    int tem1 = int.Parse(format.Substring(format.LastIndexOf('|') + 1));
                    string tem2 = format.Substring(0, format.LastIndexOf('|'));
                    string[] data = value.Split(tem2.ToCharArray());
                    if (tem1 >= data.Length)
                    {
                        temValue = EmptyMark;
                        break;
                    }
                    temValue = data[tem1];
                    break;
                default:
                    break;
            }
            return temValue;
        }
        #endregion
        #region 公式计算

        /// <summary>
        /// 计算公式 
        /// </summary>
        private void FormulaCalculation(Document doc)
        {

            NodeCollection node = doc.GetChildNodes(NodeType.Table, true);
            for (int i = 0; i < node.Count; i++)   //便利文档中所有的表格
            {
                Aspose.Words.Tables.Table table = (Aspose.Words.Tables.Table)node[i];
                foreach (Bookmark mark in table.Range.Bookmarks)
                {
                    string name = mark.Text; //书签的关键字
                    if (name.StartsWith("=AVERAGE")) //计算平均值
                    {
                        //这里需要传入Mark和表的信息,替换Mark的text就可以
                        CalculationAVERAGE(table, mark);
                    }
                    else if (name.StartsWith("=SUM"))
                    {
                        CalculationSUM(table, mark);
                    }
                    else if (name.StartsWith("=MINUS"))
                    {
                        CalculationMINUS(table, mark);
                    }

                }
            }
        }

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="mark"></param>
        private void CalculationAVERAGE(Aspose.Words.Tables.Table table, Bookmark mark)
        {
            //=AVERAGE(a1,a2)
            //mark.Text = "=AVERAGE(A1:a3,b2)";
            string tem = mark.Text.Substring(mark.Text.IndexOf("(") + 1).TrimEnd(')');    //中间的值

            //逗号代表单个的单元格，冒号代表连续的
            List<string> values = GetCellValueList(table, tem);

            double sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (double.TryParse(values[i], out double value))
                {
                    sum += value;
                }
            }
            mark.Text = (sum / values.Count).ToString("f2");
        }
        /// <summary>
        /// 计算和
        /// </summary>
        /// <param name="mark"></param>
        private void CalculationSUM(Aspose.Words.Tables.Table table, Bookmark mark)
        {
            string tem = mark.Text.Substring(mark.Text.IndexOf("(") + 1).TrimEnd(')');    //中间的值
            //逗号代表单个的单元格，冒号代表连续的
            List<string> values = GetCellValueList(table, tem);
            double sum = 0;
            //add yjt 20220624 新增小数位
            int xsw = 2; //小数位
            for (int i = 0; i < values.Count; i++)
            {
                if (double.TryParse(values[i], out double value))
                {
                    sum += value;
                    //add yjt 20220624 新增小数位
                    if (values[i].IndexOf(".") != -1)
                    {
                        xsw = values[i].Split('.')[1].Length;
                    }
                }
            }
            mark.Text = sum.ToString("f" + xsw);
        }
        /// <summary>
        /// 计算差,mark1(原书签)-mark2(原书签)
        /// </summary>
        /// <param name="mark"></param>
        private void CalculationMINUS(Aspose.Words.Tables.Table table, Bookmark mark)
        {
            string tem = mark.Text.Substring(mark.Text.IndexOf("(") + 1).TrimEnd(')');    //中间的值
            //逗号代表单个的单元格，冒号代表连续的
            string[] rang = tem.ToLower().Split(',');
            List<string> values = GetCellValueList(table, rang[0]);
            List<string> values2 = new List<string>();
            if (rang.Length > 1) values2 = GetCellValueList(table, rang[1]);
            int xsw = 2; //小数位
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (double.TryParse(values[i], out double value))
                {
                    sum += value;
                    if (values[i].IndexOf(".") != -1)
                    {
                        xsw = values[i].Split('.')[1].Length;
                    }
                }
            }
            double sum2 = 0;
            for (int i = 0; i < values2.Count; i++)
            {
                if (double.TryParse(values2[i], out double value))
                {
                    sum2 += value;
                    if (values2[i].IndexOf(".") != -1)
                    {
                        xsw = values2[i].Split('.')[1].Length;
                    }
                }
            }
            mark.Text = (sum - sum2).ToString("f" + xsw);
        }

        #region 单元格的方法

        private string GetCellValue(Aspose.Words.Tables.Table table, int row, int cell)
        {
            string b = "0";
            try
            {
                b = table.Rows[row - 1].Cells[cell - 1].ToTxt().ToString().Trim('\n').Trim('\r').Trim();
            }
            catch (Exception)
            {

            }
            return b;

        }

        //add yjt 20220624 判断-号获取值
        private string GetCellValueJian(string str, Aspose.Words.Tables.Table table, int row, int cell)
        {
            string b = "0";
            try
            {
                b = table.Rows[row - 1].Cells[cell - 1].ToTxt().ToString().Trim('\n').Trim('\r').Trim();

                if (str.IndexOf('-') != -1)
                {
                    if (b.IndexOf('+') != -1)
                    {
                        b = b.TrimStart('+');
                    }
                    b = "-" + b;
                }
            }
            catch (Exception)
            {

            }
            return b;
        }

        /// <summary>
        ///获取单元格坐标
        /// </summary>
        /// <returns></returns>
        private bool GetCellIndex(string str, ref int rowIndex, ref int cellIndex)
        {
            if (str.Length < 2)
            {
                return false;
            }
            //string cell = str.Substring(0, 1);//第几列
            char cell = str[0];

            //modify yjt 20220624 判断-号获取值
            string row;
            if (str.IndexOf('-') != -1)
            {
                cell = str.TrimStart('-')[0];
                row = str.Substring(2); //第几行
            }
            else
            {
                row = str.Substring(1); //第几行
            }
            //modify yjt 20220624 判断-号获取值
            //string row = str.Substring(1); //第几行

            try
            {
                cellIndex = Convert.ToInt32(cell) - 96;  //计算是第几列
                rowIndex = int.Parse(row);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 传入一个字符串，返回 字符串代表的所有单元格的值
        /// </summary>
        /// <returns></returns>
        private List<string> GetCellValueList(Aspose.Words.Tables.Table table, string str)
        {
            List<string> value = new List<string>();
            string[] CellData = str.ToLower().Split(',');

            for (int i = 0; i < CellData.Length; i++)
            {

                if (CellData[i].Trim() == "") continue;
                string[] data = CellData[i].Split(':'); //开始区域和结束区域，是一个矩形区域 a1:a2

                if (data.Length == 1)//这个代表一个单元格
                {
                    int row1 = 0;
                    int cell1 = 0;
                    if (GetCellIndex(data[0], ref row1, ref cell1))
                    {
                        //modify yjt 20220624 判断-号获取值
                        //value.Add(GetCellValue(table, row1, cell1));
                        //modify yjt 20220624 判断-号获取值
                        value.Add(GetCellValueJian(data[0], table, row1, cell1));
                    }
                }
                else if (data.Length == 2)//这个代表一个矩形区域
                {
                    int row1 = 0;
                    int cell1 = 0;
                    int row2 = 0;
                    int cell2 = 0;
                    //开始和结束区域
                    if (GetCellIndex(data[0], ref row1, ref cell1) && GetCellIndex(data[1], ref row2, ref cell2))
                    {

                        for (int r = row1; r <= row2; r++)
                        {
                            for (int c = cell1; c <= cell2; c++)
                            {
                                value.Add(GetCellValue(table, r, c));
                            }
                        }
                    }
                }
                //根据列和行取出单元格的值，转数字--默认0
                //求和-求平均值-
                //应该先分割字符串-在判断公式-因为还有求和-求减的公式
            }
            return value;
        }

        #endregion

        #endregion

        private void OnPopSelected(object sender, SelectionChangedEventArgs e)
        {


        }


        /// <summary>
        /// 文件存储路径
        /// </summary>
        string PathExcel { get; set; } = System.IO.Directory.GetCurrentDirectory() + @"\Res\Excel";
        /// <summary>
        /// 导出汇总excel文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Export_Summary_Excel_Click()
        {
            IEnumerable<DynamicViewModel> meters = MetersVM.IsAllMeter ? MetersVM.Meters : MetersVM.Meters.Where(item => (bool)item.GetProperty("IsSelected"));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                return;
            }

            using (System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = f.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(f.SelectedPath))
                {
                    PathExcel = f.SelectedPath;
                }
                else
                {
                    return;
                }
            }
            // 汇总
            try
            {
                Aspose.Cells.Workbook wbook = new Aspose.Cells.Workbook();
                Aspose.Cells.Worksheet sheet = wbook.Worksheets[0];
                sheet.Name = "数据汇总";
                Aspose.Cells.Style titleStyle = wbook.Styles[wbook.Styles.Add()];
                Aspose.Cells.Style dataStyle = wbook.Styles[wbook.Styles.Add()];
                titleStyle.Name = "表头";
                titleStyle.ForegroundColor = System.Drawing.Color.FromArgb(200, 200, 200);
                titleStyle.Pattern = BackgroundType.Solid;
                titleStyle.Font.IsBold = true;
                titleStyle.Font.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                titleStyle.Font.Size = 9;//文字大小
                titleStyle.Font.Name = "幼圆";
                titleStyle.HorizontalAlignment = TextAlignmentType.Center;
                titleStyle.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
                titleStyle.Borders[Aspose.Cells.BorderType.LeftBorder].Color = System.Drawing.Color.WhiteSmoke;
                titleStyle.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框
                titleStyle.Borders[Aspose.Cells.BorderType.RightBorder].Color = System.Drawing.Color.WhiteSmoke;
                titleStyle.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
                titleStyle.Borders[Aspose.Cells.BorderType.TopBorder].Color = System.Drawing.Color.WhiteSmoke;
                titleStyle.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
                titleStyle.Borders[Aspose.Cells.BorderType.BottomBorder].Color = System.Drawing.Color.WhiteSmoke;

                dataStyle.Name = "数据";
                dataStyle.Pattern = BackgroundType.Solid;
                dataStyle.Font.IsBold = false;
                dataStyle.Font.Size = 9;//文字大小
                dataStyle.HorizontalAlignment = TextAlignmentType.Center;
                dataStyle.ForegroundColor = System.Drawing.Color.White;
                dataStyle.Font.Color = System.Drawing.Color.Black;
                dataStyle.Font.Name = "幼圆";
                dataStyle.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
                dataStyle.Borders[Aspose.Cells.BorderType.LeftBorder].Color = System.Drawing.Color.WhiteSmoke;
                dataStyle.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框
                dataStyle.Borders[Aspose.Cells.BorderType.RightBorder].Color = System.Drawing.Color.WhiteSmoke;
                dataStyle.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
                dataStyle.Borders[Aspose.Cells.BorderType.TopBorder].Color = System.Drawing.Color.WhiteSmoke;
                dataStyle.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
                dataStyle.Borders[Aspose.Cells.BorderType.BottomBorder].Color = System.Drawing.Color.WhiteSmoke;

                Save_ExcelSummary_StateGrid(meters, wbook, sheet);

                if (!System.IO.Directory.Exists(PathExcel))
                {
                    System.IO.Directory.CreateDirectory(PathExcel);
                    System.Threading.Thread.Sleep(500);
                }
                string file = Path.Combine(PathExcel, $"汇总_{DateTime.Now:yyyy-MM-ddTHHmmss}.xls");
                wbook.Save(file, Aspose.Cells.SaveFormat.Excel97To2003);
                System.Threading.Thread.Sleep(100);
                MessageBox.Show($"汇总文件导出成功\r\n路径:【{file}】\r\n保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"汇总文件导出失败\r\n{ex}");
            }
        }

        /// <summary>
        /// 国网表头对照属性字段表
        /// </summary>
        private Dictionary<string, string> Reference_StateGrid { get; } = new Dictionary<string, string> {
      { "条码号段","MD_BAR_CODE" },
      { "出厂编号","MD_MADE_NO" },
      { "表位号","MD_EPITOPE" },
      { "批次","MD_SEAL_2" },
      { "型号","MD_METER_MODEL" },
      { "测试时间","MD_TEST_DATE" },
      { "电流","MD_UA" },
      { "电压","MD_UB" },
      { "常数","MD_CONSTANT" },
      { "温度","MD_TEMPERATURE" },
      { "湿度","MD_HUMIDITY" },
      { "结论","MD_RESULT" },
      { "检验工号","MD_TEST_PERSON" },
      { "检验台体","MD_DEVICE_ID" },
  };

        /// <summary>
        /// 国网电力公司数据表
        /// </summary>
        /// <param name="meterResult"></param>
        /// <param name="columnHeaders"></param>
        /// <returns></returns>
        private DataTable GetDataTables_StateGrid(IEnumerable<DynamicViewModel> meters)
        {
            if (meters.Count() <= 0) return null;
            DataTable dataTable = new DataTable
            {
                TableName = "Summary"
            };
            List<string> ref_keys = Reference_StateGrid.Keys.ToList();
            List<string> all_keys = new List<string>(ref_keys);
            foreach (DynamicViewModel meter in meters)
            {
                OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);
                _ = meterResult.MeterInfo.Sort();
                meterResult.Categories.Sort(x => x.ViewName);
                for (int i = 0; i < meterResult.Categories.Count; i++)
                {
                    string headerName = string.Empty;
                    foreach (DynamicViewModel resultUnit in meterResult.Categories[i].ResultUnits)
                    {
                        headerName = resultUnit.GetProperty("项目名").ToString();
                        // 重名的项目直接跳过
                        if (all_keys.Contains(headerName))
                            continue;
                        // 导入项目名作为表头
                        all_keys.Add(headerName);
                    }
                }
            }
            foreach (string header in all_keys)
            {
                // 构建表头
                _ = dataTable.Columns.Add(header);
            }
            // 数据导入
            foreach (DynamicViewModel meter in meters)
            {
                DataRow dataRow = dataTable.NewRow();
                int index = 0;
                OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);
                _ = meterResult.MeterInfo.Sort();
                meterResult.Categories.Sort(x => x.ViewName);
                // 导入meterInfo
                while (index < ref_keys.Count)
                {
                    dataRow[index] = meterResult.MeterInfo.GetProperty(Reference_StateGrid[ref_keys[index]]);
                    index++;
                }
                foreach (OneMeterResult.ViewCategory category in meterResult.Categories)
                {
                    foreach (DynamicViewModel result in category.ResultUnits)
                    {
                        dataRow[result.GetProperty("项目名").ToString()] = result.GetProperty("平均值") == null ? result.GetProperty("结论").ToString() : result.GetProperty("平均值").ToString();
                    }
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
        /// <summary>
        /// 打印国网山东省电力公司样式汇总Excel
        /// </summary>
        /// <param name="meterResult"></param>
        /// <param name="wbook"></param>
        /// <param name="sheet"></param>
        /// <param name="nextRow"></param>
        private void Save_ExcelSummary_StateGrid(IEnumerable<DynamicViewModel> meters, Workbook wbook, Worksheet sheet)
        {
            DataTable dataTable = GetDataTables_StateGrid(meters);
            int StartRow = 0;
            int StartColumn = 0;
            int TotalRows = dataTable.Rows.Count + 1;
            int TotalColumns = dataTable.Columns.Count;
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    if (row == 0)
                    {
                        sheet.Cells[row, col].PutValue(dataTable.Columns[col].ColumnName.ToString().Trim());
                        Aspose.Cells.Style titleStyle = wbook.Styles[wbook.Styles.Count - 2];
                        sheet.Cells[row, col].SetStyle(titleStyle);
                    }
                    sheet.Cells[row + 1, col].PutValue(dataTable.Rows[row][col].ToString().Trim());
                    Aspose.Cells.Style valueStyle = wbook.Styles[wbook.Styles.Count - 1];
                    sheet.Cells[row + 1, col].SetStyle(valueStyle);
                }
            }
            Aspose.Cells.Range range = sheet.Cells.CreateRange(StartRow, StartColumn, TotalRows, TotalColumns);
            range.SetOutlineBorder(Aspose.Cells.BorderType.TopBorder, Aspose.Cells.CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(Aspose.Cells.BorderType.BottomBorder, Aspose.Cells.CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(Aspose.Cells.BorderType.LeftBorder, Aspose.Cells.CellBorderType.Thin, System.Drawing.Color.Black);
            range.SetOutlineBorder(Aspose.Cells.BorderType.RightBorder, Aspose.Cells.CellBorderType.Thin, System.Drawing.Color.Black);
            sheet.AutoFitRows();
            sheet.AutoFitColumns();
        }

        private void OnPopSelected(object sender, RoutedEventArgs e)
        {
            pop.IsOpen = false;
            //if (e.AddedItems.Count <= 0) return;
            Button   item = e.OriginalSource as Button;
            if (item == null) return;

            var meters = MetersVM.Meters.Where(m => (bool)m.GetProperty("IsSelected"));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                return;
            }

            if (item.Tag.ToString() == "summary1")
            {
                Export(true);
            }
            else if (item.Tag.ToString() == "summary2")
            {
                SummaryExport2.Export(meters);
            }
            else if (item.Tag.ToString() == "summary3")
            {
                Export_Summary_Excel_Click();
            }
        }
    }
}

