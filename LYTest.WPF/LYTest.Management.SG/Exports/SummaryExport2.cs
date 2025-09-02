using Aspose.Cells;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace LYTest.DataManager.SG.Exports
{
    /// <summary>
    /// 汇总导出2
    /// </summary>
    class SummaryExport2
    {

        // 导出excel文件
        public static void Export(IEnumerable<DynamicViewModel> meters)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "导出文件存放路径",
                Filter = "excel|*.xls"
            };
            if (dlg.ShowDialog() == DialogResult.Cancel) return;

            string pathExcel = dlg.FileName;


            DataTable table = new DataTable();
            table.Columns.Add("条码");
            table.Columns.Add("出厂编号");
            table.Columns.Add("箱号");
            table.Columns.Add("批次");
            table.Columns.Add("型号");
            table.Columns.Add("测试时间");
            table.Columns.Add("电流");
            table.Columns.Add("电压");
            table.Columns.Add("常数");
            table.Columns.Add("温度");
            table.Columns.Add("湿度");

            foreach (DynamicViewModel meter in meters)
            {
                OneMeterResult meterResult = new OneMeterResult(meter.GetProperty("METER_ID").ToString(), false);
                TableFill(table, meterResult);
            }

            Workbook wbook = new Workbook();
            Worksheet sheet = wbook.Worksheets[0];
            sheet.Name = "汇总";

            Style style = wbook.Styles[wbook.Styles.Add()];
            style.Pattern = BackgroundType.Solid;
            style.ForegroundColor = System.Drawing.Color.FromArgb(0xF0, 0xF0, 0xF0);
            style.Font.Name = "微软雅黑";
            style.Font.Size = 9;

            Style style1 = wbook.Styles[wbook.Styles.Add()];
            style1.Pattern = BackgroundType.Solid;
            style1.Font.Name = "微软雅黑";
            style1.Font.Size = 9;


            TableToSheet(table, sheet, style, style1);

            string fold = Path.GetDirectoryName(pathExcel);
            if (!Directory.Exists(fold))
            {
                System.IO.Directory.CreateDirectory(fold);
                System.Threading.Thread.Sleep(500);
            }
            wbook.Save(pathExcel, SaveFormat.Excel97To2003);

            System.Threading.Thread.Sleep(100);
            MessageBox.Show("导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"汇总文件导出失败\r\n{ex}");
            //}

        }

        /// <summary>
        /// 多个表导出一个汇总文件
        /// </summary>
        private static void TableFill(DataTable dt, OneMeterResult meter)
        {
            DataRow row = dt.NewRow();
            row["条码"] = meter.MeterInfo.GetProperty("MD_BAR_CODE");     // 条码号段
            row["出厂编号"] = meter.MeterInfo.GetProperty("MD_MADE_NO");      // 出厂编号
            row["箱号"] = meter.MeterInfo.GetProperty("MD_SEAL_1");       // 箱号
            row["批次"] = meter.MeterInfo.GetProperty("MD_SEAL_2");       // 批次
            row["型号"] = meter.MeterInfo.GetProperty("MD_METER_MODEL");  // 型号
            row["测试时间"] = meter.MeterInfo.GetProperty("MD_TEST_DATE");    // 测试时间
            row["电流"] = meter.MeterInfo.GetProperty("MD_UA");           // 电流
            row["电压"] = meter.MeterInfo.GetProperty("MD_UB");           // 电压
            row["常数"] = meter.MeterInfo.GetProperty("MD_CONSTANT");     // 常数
            row["温度"] = meter.MeterInfo.GetProperty("MD_TEMPERATURE"); // 温度
            row["湿度"] = meter.MeterInfo.GetProperty("MD_HUMIDITY");    // 湿度

            foreach (var c in meter.Categories)
            {
                foreach (DynamicViewModel vm in c.ResultUnits)
                {
                    string proId = vm.GetProperty("项目号").ToString();
                    string proName = vm.GetProperty("项目名").ToString();

                    string[] arr = proName.Split('_');
                    if (proId.StartsWith("12002_")) // 起动 | 潜动 | 日计时
                    {
                        string colName = $"起动|{arr[1]}";
                        if (!dt.Columns.Contains(colName))
                            dt.Columns.Add(colName);
                        row[colName] = vm.GetProperty("结论");
                    }
                    else if (proId.StartsWith("12003_"))
                    {
                        string colName = $"潜动|{arr[1]}";
                        if (!dt.Columns.Contains(colName))
                            dt.Columns.Add(colName);
                        row[colName] = vm.GetProperty("结论");
                    }
                    else if (proId.StartsWith("15002")) // 日计时
                    {
                        string colName = "日计时";
                        if (!dt.Columns.Contains(colName))
                            dt.Columns.Add(colName);
                        row[colName] = vm.GetProperty("结论");

                    }
                    else if (proId.StartsWith("12001_"))  // 基本误差
                    {
                        if (arr[1] == "H")
                            arr[1] = "ABC";
                        string colName = $"回路1|{arr[0]}|{arr[1]}|{arr[2]}|{arr[3]}";
                        if (!dt.Columns.Contains(colName))
                            dt.Columns.Add(colName);
                        row[colName] = vm.GetProperty("平均值");
                    }

                }
            }

            string colr = "结论";
            if (!dt.Columns.Contains(colr))
                dt.Columns.Add(colr);
            row[colr] = meter.MeterInfo.GetProperty("MD_RESULT");

            string colMan = "检验工号";
            if (!dt.Columns.Contains(colMan))
                dt.Columns.Add(colMan);
            row[colMan] = meter.MeterInfo.GetProperty("MD_TEST_PERSON");

            string colTT = "检验台体";
            if (!dt.Columns.Contains(colTT))
                dt.Columns.Add(colTT);
            row[colTT] = meter.MeterInfo.GetProperty("MD_DEVICE_ID");

            dt.Rows.Add(row);
        }

        private static void TableToSheet(DataTable dt, Worksheet sheet, Style headStyle, Style style)
        {

            int col = 0;
            sheet.Cells.SetRowHeightPixel(0, 25);

            foreach (DataColumn column in dt.Columns)
            {
                sheet.Cells[0, col].Value = column.Caption;
                sheet.Cells.SetColumnWidth(col, 20);
                sheet.Cells[0, col].SetStyle(headStyle);
                col++;
            }
            //sheet.Cells.CreateRange(0, 0, 1, col).ApplyStyle(headStyle, new StyleFlag() { All = true });


            int row = 1;
            int columnCount = dt.Columns.Count;
            foreach (DataRow r in dt.Rows)
            {
                sheet.Cells.SetRowHeightPixel(row, 25);
                col = 0;
                for (int i = 0; i < columnCount; i++)
                {
                    sheet.Cells[row, col].Value = r[col];
                    sheet.Cells[row, col].SetStyle(style);
                    col++;

                }
                row++;

            }

            //sheet.AutoFitColumns();


        }




    }
}
