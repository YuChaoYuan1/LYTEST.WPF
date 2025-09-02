using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aspose.Cells;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.InputPara;
using LYTest.ViewModel.Meters;
using LYTest.WPF.Converter;
using LYTest.WPF.Model;

namespace LYTest.WPF.View
{
    /// <summary>
    /// ExportData.xaml 的交互逻辑
    /// </summary>
    public partial class ExportData
    {
        public ExportData()
        {
            InitializeComponent();
            Name = "数据管理";
            DockStyle.IsFloating = true; //是否开始是全屏  
            DockStyle.FloatingSize = SystemParameters.WorkArea.Size;
            LoadColumns();
            //LoadTemplates();
            viewModel.SearchMeters();
            //checkBoxPreview.IsChecked = Properties.Settings.Default.IsPreview;
            datagridMeter.ColumnReordered += datagridMeter_ColumnReordered;

        }
        private MetersViewModel meterResults = new MetersViewModel(null);
        /// <summary>
        /// 表信息模型
        /// </summary>
        public MetersViewModel viewModel
        {
            get
            {
                return Resources["MetersViewModel"] as MetersViewModel;
            }
        }
        private ContextMenu menuTemp
        {
            get { return Resources["contextMenu"] as ContextMenu; }
        }
        void datagridMeter_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            var columnsTemp = datagridMeter.Columns.OrderBy(item => item.DisplayIndex);
            var columnNames = from item in columnsTemp select item.Header.ToString();
            Properties.Settings.Default.ColumnNames = string.Join(",", columnNames);
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// 加载列
        /// </summary>
        private void LoadColumns()
        {
            if (Properties.Settings.Default.ColumnNames == null)
            {
                Properties.Settings.Default.ColumnNames = "";
            }
            string[] columnNames = Properties.Settings.Default.ColumnNames.Split(',');
            if (columnNames.Length != MetersViewModel.ParasModel.AllUnits.Count)
            {
                columnNames = new string[MetersViewModel.ParasModel.AllUnits.Count];
                for (int i = 0; i < MetersViewModel.ParasModel.AllUnits.Count; i++)
                {
                    columnNames[i] = MetersViewModel.ParasModel.AllUnits[i].DisplayName;
                }

                Properties.Settings.Default.ColumnNames = string.Join(",", columnNames);
                Properties.Settings.Default.Save();
            }
            for (int i = 0; i < columnNames.Length; i++)
            {
                InputParaUnit paraUnit = MetersViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == columnNames[i]);
                #region 要检
                if (paraUnit.FieldName == "CHR_CHECKED")
                {
                    Binding cellBinding = new Binding("CHR_CHECKED");
                    cellBinding.Mode = BindingMode.TwoWay;
                    cellBinding.Converter = new BoolBitConverter();
                    DataGridColumn columnYaojian = new DataGridCheckBoxColumn
                    {
                        Header = paraUnit.DisplayName,
                        Binding = cellBinding,
                        IsReadOnly = true
                    };
                    datagridMeter.Columns.Add(columnYaojian);
                }
                #endregion
                else
                {
                    DataGridColumn column = new DataGridTextColumn
                    {
                        Header = paraUnit.DisplayName,
                        Binding = new Binding(paraUnit.FieldName),
                        IsReadOnly = true
                    };
                    datagridMeter.Columns.Add(column);
                }
            }
        }


        private void datagridMeter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DynamicViewModel dynamic = datagridMeter.SelectedItem as DynamicViewModel;
            //OneMeterResult meterResult = viewModel.ResultCollection[datagridMeter.SelectedIndex ];
            if (dynamic == null)
            {
                return;
            }

            OneMeterResult meterResult = new OneMeterResult(dynamic.GetProperty("METER_ID").ToString(), false);
            if (meterResult == null)
            {
                return;
            }
            LoadMeterDataGrids(meterResult);

        }

        /// <summary>
        /// 加载结论对应的表格
        /// </summary>
        private void LoadMeterDataGrids(OneMeterResult meterResult)
        {
            if (meterResult == null)
            {
                return;
            }
            resultContainer.Children.Clear();
            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                DataGrid dataGrid = new DataGrid()
                {
                    Margin = new Thickness(3),
                    HeadersVisibility = DataGridHeadersVisibility.All,
                    IsReadOnly = true,
                    Style = Application.Current.Resources["dataGridStyleMeterDetailResult"] as System.Windows.Style,
                };
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
                resultContainer.Children.Add(dataGrid);
            }
        }
        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBoxTemp = sender as CheckBox;
            if (checkBoxTemp != null)
            {
                DynamicViewModel modelTemp = checkBoxTemp.DataContext as DynamicViewModel;
                if (modelTemp != null)
                {
                    if (checkBoxTemp.IsChecked.HasValue && checkBoxTemp.IsChecked.Value)
                    {
                        modelTemp.SetProperty("IsSelected", true);
                    }
                    else
                    {
                        modelTemp.SetProperty("IsSelected", false);
                    }
                }
            }
        }

        private void datagridMeter_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
                menuTemp.Visibility = Visibility.Visible;
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
            viewModel.LoadFilterCollection(fieldName, temp);
        }

        private void btn_SaveData(object sender, RoutedEventArgs e)
        {
           

            var meters = viewModel.Meters.Where(item => ((bool)item.GetProperty("IsSelected")));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                return;
            }
            foreach (DynamicViewModel meter in meters)
            {
                string barcode = meter.GetProperty("AVR_BAR_CODE") as string;
                OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);
                try
                {
                    Save_Excel(meterResult);
                }
                catch (Exception)
                {
                    Utility.Log.LogManager.AddMessage($"文件【{barcode}】保存失败", Utility.Log.EnumLogSource.用户操作日志, Utility.Log.EnumLevel.Tip);
                }

            }

        }
        private void btn_UpData(object sender, RoutedEventArgs e)
        {

            var meters = viewModel.Meters.Where(item => ((bool)item.GetProperty("IsSelected")));
            int s = meters.Count();
            if (meters == null || meters.Count() == 0)
            {
                MessageBox.Show("请至少选择一块表!");
                return;
            }
            foreach (DynamicViewModel meter in meters)
            {
                string barcode = meter.GetProperty("METER_ID") as string;
                Mis.Common.IMis mis = Mis.MISFactory.Create();
                mis.UpdateInit();
                Core.Model.Meter.TestMeterInfo m = new Core.Model.Meter.TestMeterInfo();
                m.Meter_ID = barcode;
                Core.Model.Meter.TestMeterInfo temmeter = Mis.DataHelper.DataManage.GetDnbInfoNew(m, true);
                if (mis.Update(temmeter))
                {
                    Utility.Log.LogManager.AddMessage(string.Format("电能表[{0}]检定记录上传成功!", temmeter.MD_BarCode), Utility.Log.EnumLogSource.数据库存取日志, Utility.Log.EnumLevel.Warning);
                }
                else
                {
                    Utility.Log.LogManager.AddMessage(string.Format("电能表[{0}]检定记录上传失败!", temmeter.MD_BarCode), Utility.Log.EnumLogSource.数据库存取日志, Utility.Log.EnumLevel.TipsError);
                }

            }

        }
        

        private void Save_Excel(OneMeterResult meterResult)
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
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框  
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
            //style = sheet.Cells["A1"].GetStyle();


            style2.Pattern = Aspose.Cells.BackgroundType.Solid;
            style2.Font.IsBold = false;
            style2.Font.Name = "宋体";//文字字体 
            style2.Font.Size = 15;//文字大小
            style2.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //左边框 
            style2.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin; //右边框  
            style2.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin; //上边框  
            style2.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //下边框
            //style2 = sheet.Cells["A1"].GetStyle();

            int Start_A = 0;
            int Start_B = 0;
            int End_A = 0;
            int End_B = 0;
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
            string file = System.IO.Directory.GetCurrentDirectory() + @"\Res\Excel\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            wbook.Save(file, SaveFormat.Excel97To2003);
            System.Threading.Thread.Sleep(1000);
            Utility.Log.LogManager.AddMessage($"文件路径:【{file}】保存成功", Utility.Log.EnumLogSource.用户操作日志, Utility.Log.EnumLevel.Tip);
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
            DataTable[] dataTables = new DataTable[meterResult.Categories.Count+1];

            DataTable dataTable = new DataTable();
            //先添加表数据
            for (int i = 0; i < datagridMeter.Columns.Count; i++)
            {
                dataTable.Columns.Add(datagridMeter.Columns[i].Header.ToString());//构建表头 
            }
            DataRow row2 = dataTable.NewRow();

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                row2[i] = meterResult.MeterInfo.GetProperty(datagridMeter.Columns[i].SortMemberPath);
            }
            dataTable.Rows.Add(row2);
            dataTables[0] = dataTable;


            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                dataTable = new DataTable();

              
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

                ViewModel.Model.AsyncObservableCollection<DynamicViewModel> s = meterResult.Categories[i].ResultUnits;
                for (int q = 0; q < s.Count; q++)
                {
                    DataRow row = dataTable.NewRow();
                    for (int j = 0; j < dataTable.Columns.Count; j++)   //循环所有列
                    {
                        row[j] = s[q].GetProperty(dataTable.Columns[j].ColumnName) as string;
                    }
                    dataTable.Rows.Add(row);
                }
                dataTables[i+1] = dataTable;
            }
            return dataTables;

        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_SearchMeters(object sender, RoutedEventArgs e)
        {
            viewModel.SearchMeters1();
        }
    }
}
