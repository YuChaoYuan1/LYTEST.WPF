using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.Schema;
using LYTest.WPF.SG.Schema;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_TestConfig.xaml 的交互逻辑
    /// </summary>
    public partial class View_TestConfig 
    {
        public View_TestConfig()
        {
            InitializeComponent();
            Name = "结论配置";
            DockStyle.IsMaximized = true;
            treeSchema.DataContext = FullTree.Instance;
            DockStyle.IsFloating = true;
            DockStyle.FloatingSize = SystemParameters.WorkArea.Size;
        }

        private DataBaseDisplayViewModel viewModel
        {
            get
            {
                try
                {
                    return Resources["dataDisplayViewModel"] as DataBaseDisplayViewModel;
                }
                catch
                {
                    return null;
                }
            }
        }


        private void AdvTree_ActiveItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SchemaNodeViewModel nodeTemp = treeSchema.SelectedItem as SchemaNodeViewModel;
            if (nodeTemp != null && nodeTemp.IsTerminal)
            {
                viewModel.ParaNo = nodeTemp.ParaNo;
                viewModel.ViewIds.SelectedUnit = viewModel.ViewIds.ViewUnits.FirstOrDefault(item => item.ViewId == nodeTemp.ViewNo);
                if (viewModel.ViewIds.SelectedUnit != null)
                {
                    FrameworkElement itemTemp = listboxViewId.ItemContainerGenerator.ContainerFromItem(viewModel.ViewIds.SelectedUnit) as FrameworkElement;
                    if (itemTemp != null)
                    {
                        itemTemp.BringIntoView();
                    }
                }
            }
        }

        private void ClickItemMove(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button.Name.ToLower().Contains("fk"))
                {
                    viewModel.FKField = button.DataContext as FieldModelView;
                }
                else
                {
                    viewModel.PKField = button.DataContext as FieldModelView;
                }
                viewModel.CommandFactoryMethod(button.Name);
            }
        }

        private void tableNameChanged(object sender, SelectionChangedEventArgs e)
        {
            columnFieldPk.ItemsSource = viewModel.FieldNames;
        }    


        //保存
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveFieldView();
            SchemaNodeViewModel nodeTemp = treeSchema.SelectedItem as SchemaNodeViewModel;
            if (nodeTemp != null && nodeTemp.IsTerminal && viewModel.ViewIds.SelectedUnit != null)
            {
                nodeTemp.ViewNo = viewModel.ViewIds.SelectedUnit.ViewId;
            }
        }


        public override void Dispose()
        {
            BindingOperations.ClearAllBindings(this);
            Resources.Clear();
            listboxViewId.SelectionChanged -= ViewItemChanged;
            comboBoxTable.SelectionChanged -= tableNameChanged;
            base.Dispose();
        }

        private void ViewItemChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.LoadFieldView();
        }
    }
}
