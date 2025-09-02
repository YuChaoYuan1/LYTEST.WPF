using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.InputPara;
using System;
using System.Collections.Generic;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_InputConfig.xaml 的交互逻辑
    /// </summary>
    public partial class View_InputConfig
    {
        public View_InputConfig()
        {
            InitializeComponent();
            Name = "录入配置";
            //DockStyle.IsFloating = true;
            DockStyle.IsFloating = false;

            LoadDropDownList();
        }
        private void LoadDropDownList()
        {
            columnValueType.ItemsSource = Enum.GetValues(typeof(EnumValueType));
            List<string> listTemp = new List<string>() { "" };
            foreach (CodeTreeNode node in CodeTreeViewModel.Instance.CodeNodes)
            {
                foreach (CodeTreeNode nodeChild in node.Children)
                {
                    if (nodeChild.CODE_PARENT == "CheckParamSource" || nodeChild.CODE_PARENT == "ConfigSource")
                    {
                        listTemp.Add(nodeChild.CODE_NAME);
                    }
                }
            }
            columnCodeType.ItemsSource = listTemp;
        }
    }
}
