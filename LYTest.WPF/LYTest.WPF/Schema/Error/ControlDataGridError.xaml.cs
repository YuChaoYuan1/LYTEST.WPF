using LYTest.DAL;
using LYTest.ViewModel;
using LYTest.ViewModel.Model;
using LYTest.ViewModel.Schema.Error;
using LYTest.WPF.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LYTest.WPF.Schema.Error
{
    /// <summary>
    /// ControlDataGridError.xaml 的交互逻辑
    /// </summary>
    public partial class ControlDataGridError : UserControl
    {
        public ControlDataGridError()
        {
            InitializeComponent();
            InitializeDataGrid();
        }

        private void InitializeDataGrid()
        {
            Dictionary<string, string> currentDictionary = CodeDictionary.GetLayer2("CurrentTimes");
            foreach (KeyValuePair<string, string> pair in currentDictionary)
            {
                string propertyName = pair.Key.Replace('.', '_').Replace('(', '_').Replace(')', '_').Replace('/', '_');
                columnNameDictionary.Add(propertyName, pair.Key);
                DataGridColumn column = new DataGridTextColumn
                {
                    Header = pair.Key,
                    Width = 50,
                    Binding = new Binding(propertyName)
                };
                dataGrid.Columns.Add(column);
            }
            Dictionary<string, string> factorDictionary = CodeDictionary.GetLayer2("PowerFactor");
            for (int i = 0; i < factorDictionary.Keys.Count; i++)
            {
                DynamicViewModel viewModel = new DynamicViewModel(i);
                viewModel.SetProperty("PowerFactor", factorDictionary.Keys.ElementAt(i));
                foreach (string currentString in columnNameDictionary.Keys)
                {
                    viewModel.SetProperty(currentString, "");
                }
                models.Add(viewModel);
            }
            dataGrid.ItemsSource = models;
        }

        private void ClearErrorPoints()
        {
            for (int i = 0; i < models.Count; i++)
            {
                foreach (string currentString in columnNameDictionary.Keys)
                {
                    models[i].SetProperty(currentString, "");
                }
            }
        }

        private Dictionary<string, string> columnNameDictionary = new Dictionary<string, string>();
        private AsyncObservableCollection<DynamicViewModel> models = new AsyncObservableCollection<DynamicViewModel>();

        public void Load()
        {
            ClearErrorPoints();
            if (!(DataContext is ErrorCategory category))
            {
                return;
            }
            for (int i = 0; i < category.ErrorPoints.Count; i++)
            {
                string current = category.ErrorPoints[i].Current.Replace('.', '_').Replace('(', '_').Replace(')', '_').Replace('/', '_');
                string factor = category.ErrorPoints[i].Factor;
                DynamicViewModel model = models.ToList().Find(item => item.GetProperty("PowerFactor") as string == factor);
                if (model != null)
                {
                    model.SetProperty(current, "√");
                }
            }
        }

        public bool FlagLoad
        {
            get { return (bool)GetValue(FlagLoadProperty); }
            set { SetValue(FlagLoadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlagLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlagLoadProperty =
            DependencyProperty.Register("FlagLoad", typeof(bool), typeof(ControlDataGridError), new PropertyMetadata(false));



        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "FlagLoad")
            {
                if (FlagLoad)
                {
                    if (DataContext as ErrorCategory != null)
                    {
                        Load();
                    }
                    FlagLoad = false;
                }
            }
            base.OnPropertyChanged(e);
        }

        private void DataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dpo = e.OriginalSource as DependencyObject;
            while (dpo != null)
            {
                if (dpo is DataGridCell)
                {
                    DataGridCell cell = dpo as DataGridCell;
                    TextBlock textBlock = Utils.FindVisualChild<TextBlock>(cell);
                    BindingExpression expression = textBlock.GetBindingExpression(TextBlock.TextProperty);
                    DynamicViewModel model = textBlock.DataContext as DynamicViewModel;
                    Binding binding = expression.ParentBinding;
                    if (model != null && binding != null)
                    {
                        string temp = "";
                        if (textBlock.Text == "")
                        {
                            temp = "√";
                        }
                        else if (textBlock.Text == "√")
                        {
                            temp = "";
                        }
                        model.SetProperty(binding.Path.Path, temp);
                        #region 修改集合中的检定点列表
                        string current = columnNameDictionary[binding.Path.Path];
                        string factor = model.GetProperty("PowerFactor") as string;
                        if (DataContext is ErrorCategory category)
                        {
                            ErrorModel errormodel = category.ErrorPoints.FirstOrDefault(item => item.Current == current && item.Factor == factor);
                            if (temp == "" && errormodel != null)
                            {
                                errormodel.FlagRemove = true;
                                category.OnPointsChanged(errormodel);
                                category.ErrorPoints.Remove(errormodel);
                            }
                            else if (temp == "√" && errormodel == null)
                            {
                                errormodel = new ErrorModel
                                {
                                    Current = current,
                                    Factor = factor,
                                    ErrorType = category.ErrorType,
                                    FangXiang = category.Fangxiang,
                                    Component = category.Component,
                                    GuichengMulti = category.GuichengMulti,
                                    LapCountIb = category.LapCountIb
                                };
                                category.OnPointsChanged(errormodel);
                                category.ErrorPoints.Add(errormodel);
                            }
                        }
                        #endregion
                    }
                    return;
                }
                else
                {
                    dpo = VisualTreeHelper.GetParent(dpo);
                }
            }
        }
    }
}
