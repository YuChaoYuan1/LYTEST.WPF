using LYTest.WPF.Schema;
using System.Windows;

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_SchemaOperation.xaml 的交互逻辑
    /// </summary>
    public partial class View_SchemaOperation
    {
        /// 方案操作视图
        /// <summary>
        /// 方案操作视图
        /// </summary>
        /// <param name="operationType"></param>
        public View_SchemaOperation(string operationType)
        {
            InitializeComponent();
            switch (operationType)
            {
                case "新建方案":
                    Content = new View_AddSchema();
                    break;
                case "复制方案":
                    Content = new View_CopySchema();
                    break;
                case "重命名方案":
                    Content = new View_RenameSchema();
                    break;
                case "删除方案":
                    Content = new View_DeleteSchema();
                    break;
                case "导入方案":
                    Content = new View_ImportSchema();
                    break;
            }
            Name = operationType;
            DockStyle.IsFloating = true;
            DockStyle.FloatingSize = new Size(1000, 600);
        }
    }
}
