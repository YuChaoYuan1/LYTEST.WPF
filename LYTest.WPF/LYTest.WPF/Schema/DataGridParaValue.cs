using System;
using System.Windows;
using LYTest.ViewModel.Model;
using LYTest.ViewModel.Schema;
using System.Windows.Controls;
using LYTest.WPF.UiGeneral;

namespace LYTest.WPF.Schema
{
    /// 设置检定参数值的表格控件
    /// <summary>
    /// 设置检定参数值的表格控件
    /// </summary>
    public class DataGridParaValue : DataGrid, IDisposable
    {
        public override void EndInit()
        {
            base.EndInit();
            try
            {
                ParaInfo.PropertyChanged += ParaInfo_PropertyChanged;
            }
            catch
            { }
        }
        ParaInfoViewModel ParaInfo
        {
            get
            {
                if (DataContext as SchemaViewModel == null)
                {
                    return null;
                }
                else
                {
                    return (DataContext as SchemaViewModel).ParaInfo;
                }
            }
        }
        void ParaInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ParaInfo == null)
            { return; }
            if (e.PropertyName == "LoadFlag" && ParaInfo.LoadFlag)
            {
                AsyncObservableCollection<CheckParaViewModel> CheckParas = ParaInfo.CheckParas;

                while (Columns.Count > 4)
                {
                    Columns.RemoveAt(4);
                }


                if (CheckParas.Count == 0)
                {
                    DataGridTextColumn column = Application.Current.Resources["dataGridColumnNoParameter"] as DataGridTextColumn;
                    Columns.Add(column);
                }
                else
                {
                    for (int i = 0; i < CheckParas.Count; i++)
                    {
                        if (CheckParas[i] is CheckParaViewModel checkPara)
                        {
                            DataGridColumn column = ControlFactory.CreateColumn(checkPara.ParaDisplayName, checkPara.ParaEnumType, checkPara.ParaDisplayName);
                            if (column != null)
                            {
                                Columns.Add(column);
                            }
                        }
                    }
                }
                ParaInfo.LoadFlag = false;
            }
        }

        public void Dispose()
        {
            ParaInfo.PropertyChanged -= ParaInfo_PropertyChanged;
        }
    }
}
