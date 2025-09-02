using LYTest.Utility.Log;
using LYTest.ViewModel.CodeTree;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_CodeTree.xaml 的交互逻辑
    /// </summary>
    public partial class View_CodeTree
    {
        public View_CodeTree()
        {
            InitializeComponent();
            Name = "数据配置";
            DockStyle.IsFloating = true;
            DataContext = CodeTreeViewModel.Instance;
        }

        // 保存
        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.DataContext is CodeTreeNode codeNode)) return;
            if (codeNode.SaveCode())
            {
                LogManager.AddMessage("编码信息已保存", EnumLogSource.用户操作日志, EnumLevel.Tip);
            }
        }
        // 添加
        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.DataContext is CodeTreeNode codeNode)) return;
            codeNode.AddCode();
        }
        // 删除
        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.DataContext is CodeTreeNode codeNode)) return;
            if (MessageBox.Show("确认要删除选中的编码及所有子节点吗?", "删除节点", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                codeNode.DeleteCode();
            };
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!(sender is Button btn))return;
        //    if (!(btn.DataContext is CodeTreeNode codeNode))return;

        //    switch (btn.Name)
        //    {
        //        case "buttonSave":
        //            if (codeNode.SaveCode())
        //            {
        //                LogManager.AddMessage("编码信息已保存", EnumLogSource.用户操作日志, EnumLevel.Tip);
        //            }
        //            break;
        //        case "buttonAdd":
        //            codeNode.AddCode();
        //            break;
        //        case "buttonDelete":
        //            if (MessageBox.Show("确认要删除选中的编码及所有子节点吗?", "删除节点", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
        //            {
        //                codeNode.DeleteCode();
        //            }
        //            break;
        //    }
        //}

        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            CodeTreeViewModel.Instance.SearchNodes();
        }
        private void Event_NodeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement elementTemp))
            {
                return;
            }
            if (elementTemp.DataContext is CodeTreeNode nodeTemp)
            {
                List<CodeTreeNode> nodesList = new List<CodeTreeNode>();
                #region 获取链
                nodesList.Add(nodeTemp);
                CodeTreeNode nodeParentTemp = nodeTemp.Parent;
                while (nodeParentTemp != null && nodeParentTemp.CODE_LEVEL >= 1)
                {
                    if (nodeParentTemp != null)
                    {
                        nodesList.Add(nodeParentTemp);
                    }
                    nodeParentTemp = nodeParentTemp.Parent;
                    if (nodeParentTemp == null)
                    {
                        break;
                    }
                }
                #endregion

                if (!(treeViewCode.ItemContainerGenerator.ContainerFromItem(nodesList[nodesList.Count - 1]) is TreeViewItem treeItem))
                {
                    return;
                }
                else
                {
                    treeItem.IsExpanded = true;
                    treeItem.IsSelected = true;
                    treeItem.BringIntoView();
                }
                for (int i = nodesList.Count - 2; i >= 0; i--)
                {
                    treeItem = treeItem.ItemContainerGenerator.ContainerFromItem(nodesList[i]) as TreeViewItem;
                    if (treeItem == null)
                    {
                        return;
                    }
                    else
                    {
                        treeItem.IsExpanded = true;
                        treeItem.IsSelected = true;
                        treeItem.BringIntoView();
                    }
                }
            }
        }
        public override void Dispose()
        {
            //重新加载编码字典
            //最简单野蛮的方法
            CodeTreeViewModel.Instance.InitializeTree();
            base.Dispose();
        }
    }
}
