using LYTest.ViewModel;
using LYTest.ViewModel.Schema;
using LYTest.ViewModel.Schema.Error;
using System.Windows;
using System.Windows.Controls;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_SchemaItemConfig.xaml 的交互逻辑
    /// </summary>
    public partial class View_SchemaItemConfig
    {

        private SchemaOperationViewModel viewModelSchemas
        {
            get { return Resources["SchemasViewModel"] as SchemaOperationViewModel; }
        }
        private SchemaViewModel viewModel
        {
            get { return Resources["SchemaViewModel"] as SchemaViewModel; }
        }


        public View_SchemaItemConfig()
        {
            InitializeComponent();
            Name = "检定项配置";
            DockStyle.IsFloating = true;
            //DataContext = new SchemaViewModel();
            treeSchema.ItemsSource = FullTree.Instance.Children;
            for (int i = 0; i < viewModelSchemas.Schemas.Count; i++)
            {
                if ((int)viewModelSchemas.Schemas[i].GetProperty("ID") == EquipmentData.Schema.SchemaId)
                {
                    viewModelSchemas.SelectedSchema = viewModelSchemas.Schemas[i];  //获得当前方案
                    break;
                }
            }
        }

        private void ButtonParaInfo_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;
            if (button.DataContext is CheckParaViewModel)
            {
                viewModel.ParaInfo.CheckParaCurrent = button.DataContext as CheckParaViewModel;
            }
            viewModel.ParaInfo.CommandFactoryMethod(button.Name);
            listBoxParaConfig.Items.Refresh();
        }

        private void AdvTree_ActiveItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SchemaNodeViewModel currentNode = treeSchema.SelectedItem as SchemaNodeViewModel;
            if (currentNode == null)
            {
                return;
            }
            viewModel.SchemaId = currentNode.SchemaId;
            if (currentNode.Children.Count == 0)
            {
                viewModel.ParaNo = currentNode.ParaNo;
            }
        }


        public override void Dispose()
        {
            treeSchema.SelectedItemChanged -= AdvTree_ActiveItemChanged;
            base.Dispose();
        }


        private void ComboBoxSchemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModelSchemas.SelectedSchema != null)
            {
                viewModel.LoadSchema((int)viewModelSchemas.SelectedSchema.GetProperty("ID")); //根据方案ID载入方案
            }
        }

        void controlEror_PointsChanged(object sender, System.EventArgs e)
        {
            ErrorModel model = sender as ErrorModel;
            if (model is ErrorModel)
            {
                viewModel.UpdateErrorPoint(model);
            }
        }

    }
}
