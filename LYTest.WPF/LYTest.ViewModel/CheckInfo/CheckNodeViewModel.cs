using LYTest.Core;
using LYTest.DAL.DataBaseView;
using LYTest.ViewModel.FrameLog;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.CheckInfo
{
    /// <summary>
    /// 检定信息节点视图
    /// </summary>
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

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 字体大小 
        /// </summary>
        public int NameFontSize
        {
            get => GetProperty(12);
            set => SetProperty(value);
        }

        /// <summary>
        /// 测试项用时，分
        /// </summary>
        public string ItemTime
        {
            get => GetProperty("");
            set => SetProperty(value);
        }



        /// <summary>
        /// 检定大项编号
        /// </summary>
        public string ParaNo
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 检定点编号
        /// </summary>
        public string ItemKey
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 方案编号
        /// </summary>
        public int SchemaId
        {
            get => GetProperty(0);
            set => SetProperty(value);
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


        /// 不合格数量
        /// <summary>
        /// 不合格数量
        /// </summary>
        public int FailCount
        {
            get
            {
                List<int> notPassList = new List<int>();
                int failCount = 0;
                bool[] yaoJian = EquipmentData.MeterGroupInfo.YaoJian;
                for (int i = 0; i < yaoJian.Length; i++)
                {
                    string result = "";
                    if (ResultSummary.GetProperty($"表位{i + 1}") is MeterResultUnit resultUnit)
                    {
                        result = resultUnit.Result;
                    }
                    if (yaoJian[i] && result == ConstHelper.不合格)
                    {
                        failCount++;
                        notPassList.Add(i + 1);
                    }
                }
                if (notPassList.Count > 0)
                {
                    DescNodeResult = "不合格表位:" + string.Join(",", notPassList);
                }
                else if (DescNodeResult != null && !DescNodeResult.Contains("均合格"))
                {
                    DescNodeResult = "";
                }
                OnPropertyChanged("DescNodeResult");
                return failCount;
            }
        }

        /// 测试通过
        /// <summary>
        /// 测试通过
        /// </summary>
        public bool TestPass
        {
            get
            {
                //int meterCount = EquipmentData.Equipment.MeterCount;
                bool[] yaoJian = EquipmentData.MeterGroupInfo.YaoJian;
                int meterCountToCheck = 0;
                for (int i = 0; i < yaoJian.Length; i++)
                {
                    string result = "";
                    if (ResultSummary.GetProperty($"表位{i + 1}") is MeterResultUnit resultUnit)
                    {
                        result = resultUnit.Result;
                    }
                    if (yaoJian[i])
                    {
                        if (result != ConstHelper.合格)
                            return false;

                        meterCountToCheck++;
                    }
                }
                //如果要检表数量为0,不设置测试通过
                if (meterCountToCheck == 0)
                    return false;

                DescNodeResult = "此项目要检表位均合格!";
                OnPropertyChanged("DescNodeResult");
                return true;
            }
        }

        private AsyncObservableCollection<DynamicViewModel> checkResults = new AsyncObservableCollection<DynamicViewModel>();
        /// 60块表的检定结论
        /// <summary>
        /// 60块表的检定结论
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> CheckResults
        {
            get { return checkResults; }
            set { SetPropertyValue(value, ref checkResults, "CheckResults"); }
        }

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
            int meterCount = EquipmentData.Equipment.MeterCount;
            bool[] yaoJian = EquipmentData.MeterGroupInfo.YaoJian;

            List<string> columnList = new List<string>();

            #region 加载所有列名称
            DisplayModel = ResultViewHelper.GetParaNoDisplayModel(ParaNo);
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
                resultModel.SetProperty("要检", yaoJian[i]);
                for (int j = 0; j < columnList.Count; j++)
                {
                    resultModel.SetProperty(columnList[j], "");
                }
                CheckResults.Add(resultModel);
            }
            #endregion

            return result;
        }

        /// <summary>
        /// 当前检定点
        /// </summary>
        public bool IsCurrent
        {
            get => GetProperty(false);
            set => SetProperty(value);
        }

        public bool IsChecking
        {
            get => GetProperty(false);
            set => SetProperty(value);
        }

        /// <summary>
        /// 是否已经检定过了,在开始检定全部置为false,检定完一项置为true，仅对多个项目同步检定有效
        /// </summary>
        public bool IsChecked { get; set; }

        private DynamicViewModel resultSummary = new DynamicViewModel(0);

        /// <summary>
        /// 表的结论总览
        /// </summary>
        public DynamicViewModel ResultSummary
        {
            get { return resultSummary; }
            set { SetPropertyValue(value, ref resultSummary); }
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
        /// 更新结论总览
        /// </summary>
        public void RefreshResultSummary()
        {
            int meterCount = EquipmentData.Equipment.MeterCount;
            bool[] yaoJian = EquipmentData.MeterGroupInfo.YaoJian;
            if (CheckResults.Count > 0)
            {
                //NameFontSize = 10;
                #region 根节点
                for (int i = 0; i < meterCount; i++)
                {
                    if (!(ResultSummary.GetProperty($"表位{i + 1}") is MeterResultUnit unit))
                    {
                        unit = new MeterResultUnit();
                        ResultSummary.SetProperty($"表位{i + 1}", unit);
                    }
                    #region 如果是根节点,从详细信息中取检定结论
                    unit.Result = CheckResults[i].GetProperty("结论") as string;
                    if (ParaNo == Core.Enum.ProjectID.基本误差试验)//误差的情况 显示他的平均值
                    {
                        if (DAL.Config.ConfigHelper.Instance.VerifyUIErrorShow == "化整值")
                            unit.ResultValue = CheckResults[i].GetProperty("化整值") as string;
                        else
                            unit.ResultValue = CheckResults[i].GetProperty("平均值") as string;
                    }
                    else if (ParaNo == Core.Enum.ProjectID.初始固有误差)
                    {
                        //unit.ResultValue = CheckResults[i].GetProperty("差值") as string;
                        unit.ResultValue = CheckResults[i].GetProperty("平均值") as string;
                    }
                    else
                    {
                        unit.ResultValue = CheckResults[i].GetProperty("结论") as string;
                    }
                    #endregion
                }
                #endregion
            }
            else
            {
                //NameFontSize = 12;
                #region 非根节点
                List<CheckNodeViewModel> nodesChild = GetRootNodes(this);
                for (int i = 0; i < yaoJian.Length; i++)
                {
                    string temp = "";
                    for (int j = 0; j < nodesChild.Count; j++)
                    {
                        string temp1 = "";
                        if (nodesChild[j].ResultSummary.GetProperty($"表位{i + 1}") is MeterResultUnit unit1)
                            temp1 = unit1.Result;

                        if (temp1 == ConstHelper.不合格)
                        {
                            temp = ConstHelper.不合格;
                            break;
                        }

                        if (temp1 == ConstHelper.合格)
                            temp = ConstHelper.合格;
                    }
                    if (!(ResultSummary.GetProperty($"表位{i + 1}") is MeterResultUnit unit2))
                    {
                        unit2 = new MeterResultUnit();
                        ResultSummary.SetProperty($"表位{i + 1}", unit2);
                    }
                    unit2.Result = temp;
                    unit2.ResultValue = temp;
                }
                #endregion
            }

            OnPropertyChanged("FailCount");
            OnPropertyChanged("TestPass");
        }
        /// <summary>
        /// 更新结论总览实时数据，例如实时误差
        /// </summary>
        public void RefreshResultValueRealtime()
        {
            if (CheckResults.Count > 0)
            {
                for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
                {
                    if (!(ResultSummary.GetProperty($"表位{i + 1}") is MeterResultUnit unit))
                    {
                        unit = new MeterResultUnit();
                        ResultSummary.SetProperty($"表位{i + 1}", unit);
                    }
                    else
                    {
                        if (ParaNo == Core.Enum.ProjectID.基本误差试验)//误差实时 显示误差值1
                        {
                            unit.ResultValue = CheckResults[i].GetProperty("误差1") as string;
                        }
                    }
                }
            }

        }
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

        private LiveMeterFrame liveFrames = new LiveMeterFrame();
        /// <summary>
        /// 实时报文记录
        /// </summary>
        public LiveMeterFrame LiveFrames
        {
            get { return liveFrames; }
            set { liveFrames = value; }
        }
    }
}
