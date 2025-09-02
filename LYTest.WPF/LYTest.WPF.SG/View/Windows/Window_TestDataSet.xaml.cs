using LYTest.ViewModel;
using LYTest.ViewModel.Meters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_TestDataSet.xaml 的交互逻辑
    /// </summary>
    public partial class Window_TestDataSet : Window
    {
        readonly SetMeterResoultModel viewModel = new SetMeterResoultModel();
        readonly string SetPath = System.IO.Directory.GetCurrentDirectory() + "\\Ini\\SetResoultConfig.ini";
        readonly Dictionary<string, DataFormat> dataFormat = new Dictionary<string, DataFormat>();


        public Window_TestDataSet()
        {
            InitializeComponent();

            //这里需要读取配置文件
            //1：获取列名的格式
            string str = Core.OperateFile.GetINI("data", EquipmentData.CheckResults.CheckNodeCurrent.ParaNo.ToString(), SetPath).Trim();
            if (str != "")
            {
                string[] tem = str.Split('#');
                for (int i = 0; i < tem.Length; i++)
                {
                    string[] data = tem[i].Split('|');
                    if (data.Length > 2)
                    {
                        if (!dataFormat.ContainsKey(data[0]))
                        {
                            DataFormat data1 = new DataFormat
                            {
                                ControType = data[1]
                            };
                            string[] v = data[2].Split('^');
                            for (int z = 0; z < v.Length; z++)
                            {
                                data1.parameterList.Add(v[z]);
                            }
                            dataFormat.Add(data[0], data1);
                        }
                    }
                }
            }

            DataContext = viewModel;
            //viewModel.CheckResults = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults;


            //这里本来需要采用序列化方式进行深拷贝--暂时先用这种方式-性能要求不高
            for (int i = 0; i < EquipmentData.CheckResults.CheckNodeCurrent.CheckResults.Count; i++)
            {
                List<string> list = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults[i].GetAllProperyName();
                DynamicViewModel model = new DynamicViewModel(i + 1);
                for (int j = 0; j < list.Count; j++)
                {
                    model.SetProperty(list[j], EquipmentData.CheckResults.CheckNodeCurrent.CheckResults[i].GetProperty(list[j]));
                }
                viewModel.CheckResults.Add(model);

            }
            ReloadColumn();

        }


        /// 加载检定点列
        /// <summary>
        /// 加载检定点列
        /// </summary>
        public void ReloadColumn()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    while (dataGrid.Columns.Count > 1)
                    {
                        BindingOperations.ClearAllBindings(dataGrid.Columns[1]);
                        dataGrid.Columns.Remove(dataGrid.Columns[1]);
                    }
                    List<string> columnNames = viewModel.CheckResults[0].GetAllProperyName();
                    //double widthTemp =1000  ;
                    //double columnWidth = 100;
                    //if (columnNames.Count > 1)
                    //{
                    //    columnWidth = (widthTemp - 100) / (columnNames.Count - 1);
                    //}
                    //if (columnWidth < 70)
                    //{
                    //    columnWidth = 70;
                    //}
                    for (int i = 0; i < columnNames.Count; i++)
                    {
                        if (columnNames[i] == "要检" || columnNames[i] == "项目名") continue;
                        if (columnNames[i] == "结论")
                        {
                            DataGridComboBoxColumn column1 = new DataGridComboBoxColumn()
                            {
                                ItemsSource = viewModel.ResoultType,
                                Header = columnNames[i],
                                SelectedItemBinding = new Binding(columnNames[i]),
                                MaxWidth = 300,
                                MinWidth = 50
                            };
                            dataGrid.Columns.Add(column1);
                            continue;
                        }

                        if (dataFormat.ContainsKey(columnNames[i])) //需要特殊操作的列
                        {
                            DataFormat data = dataFormat[columnNames[i]];
                            switch (data.ControType)
                            {
                                case "cmb":  //下拉框
                                    DataGridComboBoxColumn column = new DataGridComboBoxColumn()
                                    {
                                        ItemsSource = data.parameterList,
                                        Header = columnNames[i],
                                        SelectedItemBinding = new Binding(columnNames[i]),
                                        MaxWidth = 300,
                                        MinWidth = 50

                                    };
                                    dataGrid.Columns.Add(column);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {

                            DataGridTextColumn column = new DataGridTextColumn
                            {
                                Header = columnNames[i],
                                Binding = new Binding(columnNames[i]),
                                MinWidth = 50
                            };
                            dataGrid.Columns.Add(column);
                        }


                    };
                }));
            }
            catch
            {

            }
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image imageTemp)
            {
                switch (imageTemp.Name)
                {
                    case "imageClose":
                        this.Close();
                        break;
                    default:
                        break; ;
                }
            }
        }

        class DataFormat
        {
            /// <summary>
            /// 控件类型
            /// </summary>
            public string ControType { get; set; }
            /// <summary>
            /// 参数列表
            /// </summary>

            public ObservableCollection<string> parameterList = new ObservableCollection<string>();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (AllSet.IsChecked != true) return;  //统一修改    、
            if (!(e.Row.DataContext is DynamicViewModel)) return;

            object cellValue = "";
            if (e.EditingElement is ComboBox cbo)
            {
                cellValue = cbo.SelectedItem;
            }
            if (e.EditingElement is TextBox txt)
            {
                cellValue = txt.Text;
            }
            if (cellValue == null) return;


            //循环所有行--要检勾上的才做修改
            string columnHeader = e.Column.Header as string;
            for (int i = 0; i < viewModel.CheckResults.Count; i++)
            {
                var a = viewModel.CheckResults[i].GetProperty("要检");
                if ((bool)a != true) continue;
                viewModel.CheckResults[i].SetProperty(columnHeader, cellValue);
            }

        }
    }
}
