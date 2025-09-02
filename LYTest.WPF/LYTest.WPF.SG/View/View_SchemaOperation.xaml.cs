using LYTest.WPF.SG.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_SchemaOperation.xaml 的交互逻辑
    /// </summary>
    public partial class View_SchemaOperation 
    {
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
