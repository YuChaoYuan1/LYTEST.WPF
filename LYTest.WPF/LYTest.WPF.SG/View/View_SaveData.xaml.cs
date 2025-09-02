using Aspose.Cells;
using LYTest.Core.Enum;
using LYTest.DAL.Config;
using LYTest.DAL.DataBaseView;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.User;
using LYTest.WPF.SG.Converter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// 审核存盘界面
    /// </summary>
    public partial class View_SaveData
    {
        public View_SaveData()
        {
            InitializeComponent();
            Name = "审核存盘";
            DockStyle.IsFloating = true; //是否开始是全屏  
            DockStyle.IsMaximized = true;

            DockStyle.FloatingSize = SystemParameters.WorkArea.Size;
            LoadColumns();
            textBoxTemperature.Text = ConfigHelper.Instance.Temperature.ToString();// Properties.Settings.Default.Temperature;
            textBoxHumidy.Text = ConfigHelper.Instance.Humidity.ToString();// = Properties.Settings.Default.Humidy;
            DynamicViewModel meter = EquipmentData.MeterGroupInfo.GetFirstMeter();
            string clas = meter.GetProperty("MD_GRADE") as string;
            if (!string.IsNullOrWhiteSpace(clas))
            {
                int year = 8;
                if (clas.ToUpper().IndexOf("0.2S") >= 0 || clas.ToUpper().IndexOf("0.5S") >= 0
                    || clas.ToUpper().IndexOf("D") >= 0 || clas.ToUpper().IndexOf("C") >= 0)
                    year = 6;

                textBoxValidate.Text = (year * 12).ToString();
            }
            else
                textBoxValidate.Text = ConfigHelper.Instance.TestEffectiveTime.ToString();
            // Properties.Settings.Default.ValidateTime.ToString();
            treeSchema1.DataContext = EquipmentData.CheckResults;
            datagridMeters.SelectedIndex = 0;
            LoadMeterInfo();
            LoadUsers();
        }

        /// <summary>
        /// 所有表的结论
        /// </summary>
        private AllMeterResult ViewModel
        {
            get { return Resources["AllMeterResult"] as AllMeterResult; }
        }
        /// <summary>
        /// 结论总览加载表信息列
        /// </summary>
        private void LoadColumns()
        {
            GridViewColumnCollection columns = Application.Current.Resources["ColumnCollectionSave"] as GridViewColumnCollection;
            while (columns.Count > 2)
            {
                columns.RemoveAt(2);
            }
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                GridViewColumn column = new GridViewColumn
                {
                    Header = string.Format("表位{0}", i + 1),
                    //DisplayMemberBinding = new Binding(string.Format("ResultSummary.表位{0}.ResultValue", i + 1)),
                    Width = 58,
                };
                #region 动态模板
                DataTemplate dataTemplateTemp = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock), "textBlock");
                //上下文
                Binding bindingDataContext = new Binding(string.Format("ResultSummary.表位{0}", i + 1));
                factory.SetBinding(TextBlock.DataContextProperty, bindingDataContext);
                //文本
                Binding bindingText = new Binding("ResultValue");
                factory.SetBinding(TextBlock.TextProperty, bindingText);
                dataTemplateTemp.VisualTree = factory;
                Binding bindingColor = new Binding("Result")
                {
                    Converter = new ResultColorConverter()
                };
                factory.SetBinding(TextBlock.ForegroundProperty, bindingColor);
                column.CellTemplate = dataTemplateTemp;
                #endregion
                columns.Add(column);
            }
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
                    HeadersVisibility = DataGridHeadersVisibility.Column,
                    IsReadOnly = true,
                    SelectionUnit=DataGridSelectionUnit.FullRow,
                    
                    Style = Application.Current.Resources["dataGridStyleMeterDetailResult"] as System.Windows.Style,
                };

                //System.Windows.Style style = Application.Current.Resources["dataGridStyleMeterDetailResult"] as System.Windows.Style;
                meterResult.Categories[i].ResultUnits.Sort(item => GetSortString(item));
                dataGrid.ItemsSource = meterResult.Categories[i].ResultUnits;

                Binding textBinding = new Binding("项目名");

                FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                textBlockFactory.SetBinding(TextBlock.TextProperty, textBinding);

                DataGridTemplateColumn col = new DataGridTemplateColumn
                {
                    Header = "项目名",
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTemplate = new DataTemplate()
                    {
                        VisualTree= textBlockFactory
                    },
                };
                dataGrid.Columns.Add(col);

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
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = sender
                    };
                    dataGrid.RaiseEvent(eventArg);
                };
                resultContainer.Children.Add(dataGrid);
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

        /// <summary>
        /// 选中表发生变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OneMeterResult meterResult = datagridMeters.SelectedItem as OneMeterResult;
            LoadMeterDataGrids(meterResult);
        }
        /// <summary>
        /// 加载表基本信息
        /// </summary>
        private void LoadMeterInfo()
        {
            //800是参数录入对应的列
            Dictionary<string, string> dictionaryColumn = ResultViewHelper.GetPkDisplayDictionary("800");
            foreach (string fieldName in dictionaryColumn.Keys)
            {
                if (fieldName == "MD_CHECKED")
                {
                    continue;
                }
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
                gridTemp.ColumnDefinitions[0].Width = new GridLength(70);
                TextBlock textBlockName = new TextBlock()
                {
                    Text = dictionaryColumn[fieldName],
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0)
                };
                TextBlock textBlockValue = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(3, 0, 3, 0)
                };
                textBlockValue.SetBinding(TextBlock.TextProperty, new Binding(string.Format("MeterInfo.{0}", fieldName)));
                Grid.SetColumn(textBlockValue, 1);
                gridTemp.Children.Add(textBlockName);
                gridTemp.Children.Add(textBlockValue);
                stackPanelMeterInfo.Children.Add(gridTemp);
            }
        }
        /// <summary>
        /// 加载用户
        /// </summary>
        private void LoadUsers()
        {
            List<string> userNames = UserViewModel.Instance.GetList("");
            comboBoxAuditor.ItemsSource = userNames;
            comboBoxAuditor.SelectedItem = EquipmentData.LastCheckInfo.AuditPerson;

            comboBoxBoss.ItemsSource = userNames;

            comboBoxTester.ItemsSource = userNames;
            comboBoxTester.SelectedItem = EquipmentData.LastCheckInfo.TestPerson;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //温度,湿度,批准人,检验员,核验员,有效期,检定日期
            string[] arrayField = new string[]
                {
                    "MD_TEMPERATURE",
                    "MD_HUMIDITY",
                    "MD_SUPERVISOR",
                    "MD_TEST_PERSON",
                    "MD_AUDIT_PERSON",
                    "MD_VALID_DATE",
                    "MD_TEST_DATE"
                };

            int.TryParse(textBoxValidate.Text, out int intTemp);

            //有效期
            string stringValidDate = DateTime.Now.AddMonths(intTemp).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            string strTestDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string[] arrayValue = new string[]
                {
                    textBoxTemperature.Text,
                    textBoxHumidy.Text,
                    comboBoxBoss.SelectedItem as string,
                    comboBoxTester.Text,
                    comboBoxAuditor.SelectedItem as string,
                    stringValidDate,
                    strTestDate,
                };
            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            for (int i = 0; i < EquipmentData.MeterGroupInfo.Meters.Count; i++)
            {
                if (yaojianTemp[i])
                {
                    for (int j = 0; j < arrayField.Length; j++)
                    {
                        EquipmentData.MeterGroupInfo.Meters[i].SetProperty(arrayField[j], arrayValue[j]);
                    }
                }
            }

            ViewModel.SaveAllInfo();

            Utility.WindowProcess.LYBackup();
        }


        #region 导出excel测试

        private void Button_Click2(object sender, RoutedEventArgs e)
        {

            OneMeterResult meterResult = datagridMeters.SelectedItem as OneMeterResult;
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
                                                                                     //cells.GetColumnWidthPixel
            for (int t = 0; t < dataTables.Length; t++)
            {
                DataTable table = dataTables[t];
                //style2 = sheet.Cells["A1"].GetStyle();

                int Start_A = row;
                int Start_B = col;

                for (int i = 0; i < table.Columns.Count; i++) //列名
                {
                    sheet.Cells[row, col].Value = table.Columns[i].ColumnName.ToString().Trim();
                    sheet.Cells[row, col].PutValue(table.Columns[i].ColumnName.ToString().Trim());
                    sheet.Cells[row, col].SetStyle(style);
                    //sheet.Cells[row, col].
                    col++;
                }
                int End_B = col;
                col = 0;
                row++;
                int index = 1;
                int End_A = 1;
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

            string file = @"C:\Users\zhang\Desktop\excel\1.xls";
            wbook.Save(file, SaveFormat.Excel97To2003);
            System.Threading.Thread.Sleep(1500);
            //string file = @"C:\Users\zhang\Desktop\excel\1.xls";
            //FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
            //StreamWriter sw = new StreamWriter(new BufferedStream(fs), System.Text.Encoding.Default);
            //for (int t = 0; t < dataTables.Length; t++)
            //{
            //    //dataTableToCsv(dataTables[i], @"C:\Users\zhang\Desktop\excel\1.xls");
            //    DataTable table = dataTables[t];
            //    string line2 = "";
            //    for (int i = 0; i < table.Columns.Count; i++)
            //    {
            //        line2 += table.Columns[i].ColumnName.ToString().Trim() + "\t"; //内容：自动跳到下一单元格
            //    }
            //    line2 = line2.Substring(0, line2.Length - 1) + "\n";
            //    sw.Write(line2);
            //    foreach (DataRow row in table.Rows)
            //    {
            //        string line = "";

            //        for (int i = 0; i < table.Columns.Count; i++)
            //        {
            //            line += row[i].ToString().Trim() + "\t"; //内容：自动跳到下一单元格
            //        }
            //        line = line.Substring(0, line.Length - 1) + "\n";
            //        sw.Write(line);
            //    }
            //    sw.Write("\n");
            //}

            //sw.Close();
            //fs.Close();
            ////dataTableToCsv(dataTables[0], @"C:\Users\zhang\Desktop\excel\1.xls");
            ////dataTableToCsv(dataTables[1], @"C:\Users\zhang\Desktop\excel\1.xls");

            System.Diagnostics.Process.Start(file); //打开excel文件



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
            DataTable[] dataTables = new DataTable[meterResult.Categories.Count];
            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                DataTable dataTable = new DataTable();
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
                dataTables[i] = dataTable;
            }
            return dataTables;

        }



        //private void dataTableToCsv(DataTable table, string file)
        //{
        //    FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
        //    StreamWriter sw = new StreamWriter(new BufferedStream(fs), System.Text.Encoding.Default);

        //    string line2 = "";
        //    for (int i = 0; i < table.Columns.Count; i++)
        //    {
        //        line2 += table.Columns[i].ColumnName.ToString().Trim() + "\t"; //内容：自动跳到下一单元格
        //    }

        //    line2 = line2.Substring(0, line2.Length - 1) + "\n";
        //    sw.Write(line2);
        //    foreach (DataRow row in table.Rows)
        //    {
        //        string line = "";

        //        for (int i = 0; i < table.Columns.Count; i++)
        //        {
        //            line += row[i].ToString().Trim() + "\t"; //内容：自动跳到下一单元格
        //        }
        //        line = line.Substring(0, line.Length - 1) + "\n";
        //        sw.Write(line);
        //    }
        //    sw.Write("\n");
        //    sw.Close();
        //    fs.Close();
        //}


        #endregion


    }
}
