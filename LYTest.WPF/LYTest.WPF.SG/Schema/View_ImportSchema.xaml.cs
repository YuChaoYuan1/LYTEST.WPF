using LYTest.ViewModel;
using System.Windows.Controls;

namespace LYTest.WPF.SG.Schema
{
    /// <summary>
    /// View_ImportSchema.xaml 的交互逻辑
    /// </summary>
    public partial class View_ImportSchema : UserControl
    {
        public View_ImportSchema()
        {
            InitializeComponent();
            this.DataContext = EquipmentData.SchemaModels;
        }
    }
}
