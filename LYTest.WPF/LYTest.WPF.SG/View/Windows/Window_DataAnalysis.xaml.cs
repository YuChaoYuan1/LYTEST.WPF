using LYTest.Utility.Log;
using LYTest.ViewModel;
using LYTest.ViewModel.Meters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_DataAnalysis.xaml 的交互逻辑
    /// </summary>
    public partial class Window_DataAnalysis : Window
    {
        private readonly DataAnalysisModel viewModel = new DataAnalysisModel();
        public Window_DataAnalysis()
        {
            InitializeComponent();
            viewModel.CheckResults = EquipmentData.CheckResults.CheckNodeCurrent.CheckResults;
            this.DataContext = viewModel;

            if (EquipmentData.CheckResults.CheckNodeCurrent.ParaNo == Core.Enum.ProjectID.高次谐波)
            {
                viewModel.NameList.Add($"正常状态");
                for (int i = 15; i < 41; i++)    //将就先用
                {
                    viewModel.NameList.Add($"第{i}次谐波_低到高");
                }
                for (int i = 40; i >= 15; i--)
                {
                    viewModel.NameList.Add($"第{i}次谐波_高到低");
                }
                viewModel.SelectIndex = 0;
            }
            else
            {
                viewModel.NameList.Add(EquipmentData.CheckResults.CheckNodeCurrent.Name);
                viewModel.SelectIndex = 0;
            }
            ReloadColumn();

        }
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

                    for (int i = 0; i < columnNames.Count; i++)
                    {
                        if (columnNames[i] == "要检" || columnNames[i] == "项目名") continue;
                        if (columnNames[i] == "结论")
                        {
                            DataGridComboBoxColumn column1 = new DataGridComboBoxColumn()
                            {
                                ItemsSource = viewModel.Results,
                                Header = columnNames[i],
                                SelectedItemBinding = new Binding(columnNames[i]),
                                MaxWidth = 300,
                                MinWidth = 50
                            };
                            dataGrid.Columns.Add(column1);
                            continue;
                        }

                        DataGridTextColumn column = new DataGridTextColumn
                        {
                            Header = columnNames[i],
                            Binding = new Binding(columnNames[i]),
                            MinWidth = 50
                        };
                        dataGrid.Columns.Add(column);
                    };
                }));
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(string.Format("解析界面异常:{0}", ex.Message), EnumLogSource.用户操作日志, EnumLevel.Warning, ex);
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


    }
}
