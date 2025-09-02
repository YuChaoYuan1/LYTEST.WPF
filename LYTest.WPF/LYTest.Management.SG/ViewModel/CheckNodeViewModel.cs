using LYTest.DAL.Config;
using LYTest.DAL.DataBaseView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager.SG
{
    public class CheckNodeViewModel : ViewModelBase
    {
        private int level = 1;
        public int Level
        {
            get { return level; }
            set { SetPropertyValue(value, ref level, "Level"); }
        }
        public CheckNodeViewModel()
        {
        }
        private string name = "";
        /// 名称
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        private int nameFontSize = 12;
        /// <summary>
        /// 字体大小 
        /// </summary>
        public int NameFontSize
        {
            get { return nameFontSize; }
            set { SetPropertyValue(value, ref nameFontSize, "NameFontSize"); }
        }

        private string itemTime;
        /// <summary>
        /// 测试项用时，分
        /// </summary>
        public string ItemTime
        {
            get { return itemTime; }
            set { SetPropertyValue(value, ref itemTime, "ItemTime"); }
        }



        private string paraNo;
        /// 检定大项编号
        /// <summary>
        /// 检定大项编号
        /// </summary>
        public string ParaNo
        {
            get { return paraNo; }
            set { SetPropertyValue(value, ref paraNo, "ParaNo"); }
        }
        private string itemKey;
        /// 检定点编号
        /// <summary>
        /// 检定点编号
        /// </summary>
        public string ItemKey
        {
            get { return itemKey; }
            set { SetPropertyValue(value, ref itemKey, "ItemKey"); }
        }

        private int schemarId;
        /// 方案编号
        /// <summary>
        /// 方案编号
        /// </summary>
        public int SchemaId
        {
            get { return schemarId; }
            set { SetPropertyValue(value, ref schemarId, "SchemaId"); }
        }

        private bool isSelected = true;
        /// 是否选中
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {

            get { return isSelected; }
            set
            {
                SetPropertyValue(value, ref isSelected, "IsSelected");
                CheckSelectedStatus();
            }

        }

        public void CheckSelectedStatus()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].IsSelected = IsSelected;
                if (Children[i].Children.Count > 0)
                {
                    for (int k = 0; k < Children[i].Children.Count; k++)
                    {
                        Children[i].Children[k].IsSelected = IsSelected;
                    }
                }
            }
            //OnPropertyChanged("DescNodeResult");

        }


        /// 60块表的检定结论
        /// <summary>
        /// 60块表的检定结论
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> CheckResults { get; } = new AsyncObservableCollection<DynamicViewModel>();


        /// 数据显示模型
        /// <summary>
        /// 数据显示模型
        /// </summary>
        public TableDisplayModel DisplayModel { get; set; }


        /// 初始化检定结论
        /// <summary>
        /// 初始化检定结论
        /// </summary>
        /// <returns>是否能找到检定项对应的视图模型</returns>
        public bool InitializeCheckResults()
        {
            bool result = true;
            CheckResults.Clear();
            int meterCount = ConfigHelper.Instance.MeterCount;

            List<string> columnList = new List<string>();

            #region 加载所有列名称
            DisplayModel = ResultViewHelper.GetParaNoDisplayModel(paraNo);
            if (DisplayModel == null)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < DisplayModel.FKDisplayModelList.Count; i++)
                {
                    columnList.AddRange(DisplayModel.FKDisplayModelList[i].DisplayNames);
                }
                var displayNames = from item in DisplayModel.ColumnModelList select item.DisplayName;
                for (int i = 0; i < displayNames.Count(); i++)
                {
                    columnList.AddRange(displayNames.ElementAt(i).Split('|'));
                }
            }
            #endregion

            #region 初始化详细结论
            for (int i = 0; i < meterCount; i++)
            {
                DynamicViewModel resultModel = new DynamicViewModel(i + 1);
                resultModel.SetProperty("要检", false);
                for (int j = 0; j < columnList.Count; j++)
                {
                    resultModel.SetProperty(columnList[j], "");
                }
                CheckResults.Add(resultModel);
            }
            #endregion

            return result;
        }

        private bool isCurrent;
        /// 当前检定点
        /// <summary>
        /// 当前检定点
        /// </summary>
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetPropertyValue(value, ref isCurrent, "IsCurrent"); }
        }

        private bool isChecking;

        public bool IsChecking
        {
            get { return isChecking; }
            set { SetPropertyValue(value, ref isChecking, "IsChecking"); }
        }

        /// <summary>
        /// 是否已经检定过了,在开始检定全部置为false,检定完一项置为true，仅对多个项目同步检定有效
        /// </summary>
        public bool IsChecked { get; set; }

        private DynamicViewModel resultSummary = new DynamicViewModel(0);
        /// 表的结论总览
        /// <summary>
        /// 表的结论总览
        /// </summary>
        public DynamicViewModel ResultSummary
        {
            get { return resultSummary; }
            set { SetPropertyValue(value, ref resultSummary, "ResultSummary"); }
        }
        /// <summary>
        /// 父节点
        /// </summary>
        public CheckNodeViewModel Parent { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public AsyncObservableCollection<CheckNodeViewModel> Children { get; } = new AsyncObservableCollection<CheckNodeViewModel>();

        
        /// <summary>
        /// 获取所有的根节点
        /// </summary>
        /// <param name="categoryNode"></param>
        /// <returns></returns>
        private List<CheckNodeViewModel> GetRootNodes(CheckNodeViewModel categoryNode)
        {
            List<CheckNodeViewModel> nodeList = new List<CheckNodeViewModel>();
            if (categoryNode.CheckResults.Count > 0)
            {
                nodeList.Add(categoryNode);
            }
            for (int i = 0; i < categoryNode.Children.Count; i++)
            {
                nodeList.AddRange(GetRootNodes(categoryNode.Children[i]));
            }
            return nodeList;
        }
        /// <summary>
        /// 压缩节点:第二层节点如果只有一个子节点,则将节点上移
        /// </summary>
        public void CompressNode()
        {
            CompressNode(this);
        }
        /// <summary>
        /// 压缩节点
        /// </summary>
        /// <param name="nodeTemp"></param>
        private void CompressNode(CheckNodeViewModel nodeTemp)
        {
            //第一层的点不压缩
            if (Level < 2)
            {
                return;
            }
            List<CheckNodeViewModel> nodesChild = GetRootNodes(nodeTemp);
            if (nodesChild.Count == 1)
            {
                CheckNodeViewModel nodeChild = nodesChild[0];
                nodeChild.Parent = nodeTemp.Parent;
                nodeChild.Parent.NameFontSize = 14;
                nodeChild.Level = nodeTemp.Level;
                int index = nodeTemp.Parent.Children.IndexOf(nodeTemp);
                if (index >= 0)
                {
                    nodeTemp.Parent.Children.Remove(nodeTemp);
                    nodeTemp.Parent.Children.Insert(index, nodeChild);
                }
            }
            else
            {
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {
                    CompressNode(nodeTemp.Children[i]);
                }
            }
        }
        /// <summary>
        /// 节点结论描述:有多少表位不合格,多少表位合格
        /// </summary>
        public string DescNodeResult { get; private set; }

        private bool isExpanded = true;
        /// <summary>
        /// 是否折叠
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetPropertyValue(value, ref isExpanded, "IsExpanded"); }
        }

    }
}
