using LYTest.Core.Enum;
using LYTest.Core.Model.Schema;
using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using LYTest.ViewModel.Schema.Error;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.ViewModel.Schema
{

    /// <summary>
    /// 检定方案视图模型
    /// </summary>
    public class SchemaViewModel : SchemaNodeViewModel
    {
        public SchemaViewModel()
        {
            PropertyChanged += (obj, e) =>
            {
                if (e.PropertyName == "ParaNo")
                {
                    ParaInfo.ParaNo = ParaNo;
                    ParaValuesConvert();
                }
            };
        }
        public SchemaViewModel(int idSchema)
        {
            LoadSchema(idSchema);
            PropertyChanged += (obj, e) =>
            {
                if (e.PropertyName == "ParaNo")
                {
                    ParaInfo.ParaNo = ParaNo;
                    ParaValuesConvert();
                }
            };
        }

        public void LoadSchema(int idSchema)
        {
            Children.Clear();
            if (SchemaId != idSchema)
            {
                SchemaId = idSchema;
            }
            DynamicModel model = DALManager.SchemaDal.GetByID(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("id={0}", idSchema));
            if (model == null)
            {
                LogManager.AddMessage($"未能加载编号为 {idSchema} 的方案.", EnumLogSource.用户操作日志, EnumLevel.Warning);
                return;
            }
            Name = model.GetProperty("SCHEMA_NAME") as string;
            LoadParaValues();
            LogManager.AddMessage($"当前方案:{Name}", EnumLogSource.用户操作日志, EnumLevel.Information);
        }

        /// <summary>
        /// 参数值
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> ParaValues { get; } = new AsyncObservableCollection<DynamicViewModel>();


        private ParaInfoViewModel paraInfoViewModel;
        /// <summary>
        /// 检定点参数信息
        /// </summary>
        public ParaInfoViewModel ParaInfo
        {
            get
            {
                if (paraInfoViewModel == null)
                {
                    paraInfoViewModel = new ParaInfoViewModel();
                }

                return paraInfoViewModel;
            }
            set { paraInfoViewModel = value; }
        }

        #region 参数值的配置
        /// <summary>
        /// 参数值
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> ParaValuesView { get; set; } = new AsyncObservableCollection<DynamicViewModel>();

        /// 加载方案
        /// <summary>
        /// 加载方案
        /// </summary>
        public void LoadParaValues()
        {
            ParaValues.Clear();
            List<DynamicModel> models = DALManager.SchemaDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), string.Format("SCHEMA_ID={0} and (CODE_ENABLED <> '0' or CODE_ENABLED is null) order by para_index", SchemaId));

            for (int i = 0; i < models.Count; i++)
            {
                ParaValues.Add(new DynamicViewModel(models[i], 0));
            }
            GetInitialData();
            InitialSchemaTree();
        }
        /// <summary>
        /// 获取初始固有误差的方案
        /// </summary>
        /// <returns></returns>
        private void GetInitialData()
        {

            if (ParaValues.Count(item => item.GetProperty("PARA_NO") as string == ProjectID.初始固有误差) < 1) return;

            Dictionary<string, List<DynamicViewModel>> list = new Dictionary<string, List<DynamicViewModel>>();

            var a = from item in ParaValues
                    where item.GetProperty("PARA_NO") as string == ProjectID.初始固有误差
                    select item;
            foreach (var item in a)
            {
                string startStr = item.GetProperty("PARA_KEY").ToString().Substring(0, 8);  // 取到开始发字符
                if (!list.ContainsKey(startStr))
                {
                    list.Add(startStr, new List<DynamicViewModel>());
                }
                DynamicViewModel tem = new DynamicViewModel(0);
                List<string> propertyNames = item.GetAllProperyName();
                for (int i = 0; i < propertyNames.Count; i++)
                {
                    tem.SetProperty(propertyNames[i], item.GetProperty(propertyNames[i]));
                }
                string name = item.GetProperty("PARA_NAME") as string;
                tem.SetProperty("PARA_NAME", name + "_低到高");
                list[startStr].Add(tem);
            }


            var b = from item in ParaValues
                    where item.GetProperty("PARA_NO") as string == ProjectID.初始固有误差
                    orderby GetErrorSortString_Reversal(item.GetProperty("PARA_KEY") as string) descending
                    select item;

            foreach (var item in b)
            {
                string startStr = item.GetProperty("PARA_KEY").ToString().Substring(0, 8);  // 取到开始发字符
                DynamicViewModel tem = new DynamicViewModel(0);
                List<string> propertyNames = item.GetAllProperyName();
                for (int i = 0; i < propertyNames.Count; i++)
                {
                    tem.SetProperty(propertyNames[i], item.GetProperty(propertyNames[i]));
                }
                string name = tem.GetProperty("PARA_NAME") as string;
                tem.SetProperty("PARA_NAME", name + "_高到低");
                list[startStr].Add(tem);
            }

            //这里将方案里面的初始固有误差替换了
            //找到初始固有误差-然后全部删除全部替换
            //ParaValues.re
            int index = -1;
            for (int i = ParaValues.Count - 1; i >= 0; i--)
            {
                if (ParaValues[i].GetProperty("PARA_NO") as string == ProjectID.初始固有误差)
                {
                    index = i;
                    ParaValues.RemoveAt(i);
                }
            }
            if (index != -1)
            {
                //paraValues.RemoveAt
                foreach (var key in list.Keys)
                {
                    for (int i = 0; i < list[key].Count; i++)
                    {
                        ParaValues.Insert(index, list[key][i]);
                        index++;
                    }
                }
            }
            //return list;
        }
        #region 增删改查操作
        /// <summary>
        /// 保存参数配置--针对于下载方案
        /// </summary>
        public void SaveDownParaValue()
        {
            RefreshPointCount();
            if (SelectedNode != null && ParaNo == SelectedNode.ParaNo)
            {
                List<DynamicModel> list = ParaValuesConvertBack();
                SelectedNode.ParaValuesCurrent.Clear();
                SelectedNode.ParaValuesCurrent.AddRange(list);
            }
            List<SchemaNodeViewModel> nodesTerminal = GetTerminalNodes();
            List<DynamicModel> models = new List<DynamicModel>();
            for (int i = 0; i < nodesTerminal.Count; i++)
            {
                models.AddRange(nodesTerminal[i].ParaValuesCurrent);
            }
            for (int i = 0; i < models.Count; i++)
            {
                string PARA_NO = models[i].GetProperty("PARA_NO") as string;
                string PARA_INDEX = (i + 1).ToString("D3");
                models[i].SetProperty("PARA_INDEX", PARA_INDEX);
                models[i].SetProperty("SCHEMA_ID", SchemaId);
                if (PARA_NO == "14002" || PARA_NO == "04023")//|| PARA_NO == "15003"
                {
                    models[i].SetProperty("PARA_KEY", PARA_NO + "_" + PARA_INDEX);
                }
            }

            DALManager.SchemaDal.Delete(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), string.Format("schema_id={0}", SchemaId));
            DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), models);
            LoadParaValues();
        }

        /// <summary>
        /// 保存参数配置
        /// </summary>
        public void SaveParaValue()
        {
            RefreshPointCount();
            if (SelectedNode != null && ParaNo == SelectedNode.ParaNo)
            {
                List<DynamicModel> list = ParaValuesConvertBack();
                SelectedNode.ParaValuesCurrent.Clear();
                SelectedNode.ParaValuesCurrent.AddRange(list);
            }
            List<SchemaNodeViewModel> nodesTerminal = GetTerminalNodes();
            List<DynamicModel> models = new List<DynamicModel>();
            for (int i = 0; i < nodesTerminal.Count; i++)
            {
                if (nodesTerminal[i].ParaNo == ProjectID.初始固有误差)
                {
                    //Dictionary<string, List<DynamicModel>> keyValues = new Dictionary<string, List<DynamicModel>>();
                    List<string> list = new List<string>();
                    //nodesTerminal[i].ParaValuesCurrent
                    for (int index = 0; index < nodesTerminal[i].ParaValuesCurrent.Count; index++)
                    {
                        //nodesTerminal[i].ParaValuesCurrent[index].
                        string key = nodesTerminal[i].ParaValuesCurrent[index].GetProperty("PARA_KEY") as string;
                        if (!list.Contains(key))
                        {
                            list.Add(key);
                            models.Add(nodesTerminal[i].ParaValuesCurrent[index]);
                        }
                    }

                }
                else
                {
                    models.AddRange(nodesTerminal[i].ParaValuesCurrent);
                }
            }
            //对初始固有误差重复的部分删除



            for (int i = 0; i < models.Count; i++)
            {
                string PARA_NO = models[i].GetProperty("PARA_NO") as string;
                string PARA_INDEX = (i + 1).ToString("D3");
                models[i].SetProperty("PARA_INDEX", PARA_INDEX);
                models[i].SetProperty("SCHEMA_ID", SchemaId);
                //modify yjt 20220822 合并蒋工的初始固有误差
                if (PARA_NO == ProjectID.初始固有误差)
                {
                    string name = models[i].GetProperty("PARA_NAME") as string;
                    name = name.Replace("_低到高", "").Replace("_高到低", "");
                    models[i].SetProperty("PARA_NAME", name);
                }

            }

            int deleteCount = DALManager.SchemaDal.Delete(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), string.Format("schema_id={0}", SchemaId));
            LogManager.AddMessage(string.Format("删除方案{0}的所有检定点,共删除{1}条记录", Name, deleteCount));
            int insertCount = DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), models);
            LoadParaValues();
            LogManager.AddMessage(string.Format("插入方案{0}的所有检定点,共插入{1}条记录", Name, insertCount), EnumLogSource.用户操作日志, EnumLevel.Tip);
        }

        public void AddNewParaValue()
        {
            List<string> propertyNames = new List<string>();
            for (int i = 0; i < ParaInfo.CheckParas.Count; i++)
            {
                propertyNames.Add(ParaInfo.CheckParas[i].ParaDisplayName);
            }
            DynamicViewModel viewModel = new DynamicViewModel(propertyNames, 0);
            viewModel.SetProperty("IsSelected", true);
            for (int i = 0; i < propertyNames.Count; i++)
            {
                viewModel.SetProperty(propertyNames[i], ParaInfo.CheckParas[i].DefaultValue);
            }
            ParaValuesView.Add(viewModel);
        }
        /// <summary>
        /// 插入一行数据
        /// </summary>
        public void InsertNewParaValue(int Index)
        {
            List<string> propertyNames = new List<string>();
            for (int i = 0; i < ParaInfo.CheckParas.Count; i++)
            {
                propertyNames.Add(ParaInfo.CheckParas[i].ParaDisplayName);
            }
            DynamicViewModel viewModel = new DynamicViewModel(propertyNames, 0);
            viewModel.SetProperty("IsSelected", true);
            for (int i = 0; i < propertyNames.Count; i++)
            {
                viewModel.SetProperty(propertyNames[i], ParaInfo.CheckParas[i].DefaultValue);
            }
            ParaValuesView.Insert(Index, viewModel);
        }
        #endregion
        #endregion

        #region 加载方案树
        /// <summary>
        /// 初始化方案树
        /// </summary>
        /// <param name="listParaNo"></param>
        public void InitialSchemaTree()
        {
            Children.Clear();
            for (int i = 0; i < ParaValues.Count; i++)
            {
                string noTemp = ParaValues[i].GetProperty("PARA_NO") as string;
                if (string.IsNullOrEmpty(noTemp))
                {
                    continue;
                }

                SchemaNodeViewModel nodeParent = GetLastNode(noTemp);
                //如果方案编号不符合规则:长度为2+3*i,丢弃这个点
                if (nodeParent == null)
                {
                    continue;
                }
                nodeParent.IsTerminal = true;
                nodeParent.ParaValuesCurrent.Add(ParaValues[i].GetDataSource());
            }
            RefreshPointCount();
        }
        private SchemaNodeViewModel GetLastNode(string noTemp)
        {
            //长度规则:2+3*i
            if (noTemp == null || noTemp.Length < 2 || ((noTemp.Length - 2) % 3 > 0))
            {
                return null;
            }
            List<string> noList = GetNoList(noTemp);
            #region 添加到现有节点或者获取现有节点
            SchemaNodeViewModel nodeCurrent = GetExistLastNode();
            int indexTemp = -1;
            #region 遍历每一层的编号
            while (nodeCurrent != null)
            {
                indexTemp = noList.IndexOf(nodeCurrent.ParaNo);
                if (indexTemp >= 0)
                {
                    break;
                }
                else
                {
                    nodeCurrent = nodeCurrent.ParentNode;
                }
            }
            #endregion
            #region 如果最后一个节点不存在当前的任何一个编号则创建一个新的顶层节点
            if (nodeCurrent == null)
            {
                nodeCurrent = new SchemaNodeViewModel
                {
                    ParaNo = noTemp.Substring(0, 2),
                    Name = SchemaFramework.GetItemName(noTemp.Substring(0, 2)),
                    Level = 1,
                };
                Children.Add(nodeCurrent);
                indexTemp = 0;
            }
            #endregion
            #region 在要插入的节点循环插入各层节点
            for (; indexTemp + 1 < noList.Count; indexTemp++)
            {
                SchemaNodeViewModel nodeNew = new SchemaNodeViewModel
                {
                    ParaNo = noList[indexTemp + 1],
                    Name = SchemaFramework.GetItemName(noList[indexTemp + 1]),
                    Level = indexTemp + 2,
                };
                nodeCurrent.Children.Add(nodeNew);
                nodeNew.ParentNode = nodeCurrent;
                nodeCurrent = nodeNew;
            }
            #endregion
            #endregion
            return nodeCurrent;
        }
        /// <summary>
        /// 获取当前的最后一个节点
        /// </summary>
        /// <returns></returns>
        private SchemaNodeViewModel GetExistLastNode()
        {
            if (Children.Count == 0)
            {
                return null;
            }
            SchemaNodeViewModel nodeRoot = Children[Children.Count - 1];
            while (nodeRoot.Children.Count > 0)
            {
                nodeRoot = nodeRoot.Children[nodeRoot.Children.Count - 1];
            }
            return nodeRoot;
        }
        /// <summary>
        /// 更新方案概览,每个节点检定点的数量
        /// </summary>
        public void RefreshPointCount()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                GetPointCountPerNode(Children[i]);
            }

            #region 刷新获取每一层的节点列表
            levelDictinary.Clear();
            int levelTemp = 1;
            while (true)
            {
                List<SchemaNodeViewModel> levelNodes = GetAllNodes(levelTemp);
                if (levelNodes.Count > 0)
                {
                    levelDictinary.Add(levelTemp, levelNodes);
                }
                else
                {
                    break;
                }
                levelTemp++;
            }
            #endregion
        }
        /// <summary>
        /// 递归获取每个分支上的子节点数量
        /// </summary>
        /// <param name="nodeTemp"></param>
        private int GetPointCountPerNode(SchemaNodeViewModel nodeTemp)
        {
            if (nodeTemp.IsTerminal)
            {
                nodeTemp.PointCount = nodeTemp.ParaValuesCurrent.Count;
            }
            else
            {
                int sumTemp = 0;
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {
                    sumTemp += GetPointCountPerNode(nodeTemp.Children[i]);
                }
                nodeTemp.PointCount = sumTemp;
            }

            //如果检定点数量为0,则将该节点删除
            if (nodeTemp.PointCount == 0)
            {
                if (nodeTemp.ParentNode == null)
                {
                    Children.Remove(nodeTemp);
                }
                else
                {
                    nodeTemp.ParentNode.Children.Remove(nodeTemp);
                }
            }

            return nodeTemp.PointCount;
        }
        /// <summary>
        /// 根据获取已经存在的对应的节点,如果不存在则创建一个节点并返回
        /// </summary>
        /// <param name="noTemp"></param>
        /// <returns></returns>
        private SchemaNodeViewModel GetFirstNode(string noTemp)
        {
            //长度规则:2+3*i
            if (noTemp == null || noTemp.Length < 2 || ((noTemp.Length - 2) % 3 > 0))
            {
                return null;
            }
            List<string> noList = GetNoList(noTemp);
            int i = noList.Count - 1;
            SchemaNodeViewModel nodeCurrent = null;
            #region 注释归类：不往上查找根节点，按照下载顺序创建节点
            //List<SchemaNodeViewModel> nodes = null;
            //int levelTemp = 1;
            //for (; i >= 0; i--)
            //{
            //levelTemp = i + 1;
            //if (levelDictinary.ContainsKey(levelTemp))
            //{
            //    nodes = levelDictinary[levelTemp];
            //    int indexTemp = nodes.FindIndex(item => item.ParaNo == noList[i]);
            //    if (indexTemp >= 0)
            //    {
            //        nodeCurrent = nodes[indexTemp];
            //        break;
            //    }
            //}
            //}
            #endregion
            #region 找到的顶层节点一步步往下插入
            if (nodeCurrent == null)
            {
                i = 0;
            }
            else
            {
                i += 1;
            }
            for (; i < noList.Count; i++)
            {
                nodeCurrent = InsertNodeInLevel(nodeCurrent, noList[i]);
            }
            #endregion
            if (nodeCurrent != null)
            {
                nodeCurrent.IsTerminal = true;
            }
            return nodeCurrent;
        }
        /// <summary>
        /// 是否存在检定点编号
        /// </summary>
        /// <param name="noTemp"></param>
        /// <returns></returns>
        public bool ExistNode(string noTemp)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (ExistNode(noTemp, Children[i]))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 是否存在编号
        /// </summary>
        /// <param name="noTemp"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private bool ExistNode(string noTemp, SchemaNodeViewModel nodeTemp)
        {
            if (nodeTemp.ParaNo == noTemp)
            {
                return true;
            }
            for (int i = 0; i < nodeTemp.Children.Count; i++)
            {
                if (ExistNode(noTemp, nodeTemp.Children[i]))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 编号必须为数字
        /// </summary>
        /// <param name="levelTemp"></param>
        /// <param name="noTemp"></param>
        private SchemaNodeViewModel InsertNodeInLevel(SchemaNodeViewModel nodeParent, string noTemp)
        {
            SchemaNodeViewModel nodeNew = new SchemaNodeViewModel
            {
                ParaNo = noTemp,
                Name = SchemaFramework.GetItemName(noTemp)
            };
            string intNo = SchemaFramework.GetSortNo(noTemp);//int.Parse(noTemp);
            AsyncObservableCollection<SchemaNodeViewModel> nodes = Children;
            nodeNew.Level = 1;
            nodeNew.ParentNode = null;
            if (nodeParent != null)
            {
                nodes = nodeParent.Children;
                nodeNew.Level = nodeParent.Level + 1;
                nodeNew.ParentNode = nodeParent;
            }
            bool flagAdd = false;
            for (int i = 0; i < nodes.Count; i++)
            {//TODO:排序
                string intTemp = SchemaFramework.GetSortNo(nodes[i].ParaNo);
                if (string.IsNullOrWhiteSpace(intTemp) || intTemp == "0") continue;
                if (intTemp.CompareTo(intNo) > 0)
                {
                    nodes.Insert(i, nodeNew);
                    flagAdd = true;
                    break;
                }
            }
            if (!flagAdd)
            {
                nodes.Add(nodeNew);
            }
            return nodeNew;
        }
        /// <summary>
        /// 每一层节点的列表
        /// </summary>
        private readonly Dictionary<int, List<SchemaNodeViewModel>> levelDictinary = new Dictionary<int, List<SchemaNodeViewModel>>();
        /// <summary>
        /// 获取各层的编号列表
        /// </summary>
        /// <param name="noTemp"></param>
        /// <returns></returns>
        private List<string> GetNoList(string noTemp)
        {
            List<string> noList = new List<string>();
            int stringLength = noTemp.Length;
            noList.Add(noTemp.Substring(0, 2));
            int level = 0;
            while (2 + 3 * level < stringLength)
            {
                level += 1;
                noList.Add(noTemp.Substring(0, 2 + 3 * level));
            }
            return noList;
        }
        /// <summary>
        /// 获取某一层所有的子节点
        /// </summary>
        /// <param name="levelTemp"></param>
        /// <returns></returns>
        private List<SchemaNodeViewModel> GetAllNodes(int levelTemp)
        {
            List<SchemaNodeViewModel> nodes = new List<SchemaNodeViewModel>();
            for (int i = 0; i < Children.Count; i++)
            {
                nodes.AddRange(GetNodesPerLevel(levelTemp, Children[i]));
            }
            return nodes;
        }

        /// <summary>
        /// 获取每一层的所有节点
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private List<SchemaNodeViewModel> GetNodesPerLevel(int levelTemp, SchemaNodeViewModel nodeTemp)
        {
            List<SchemaNodeViewModel> nodeList = new List<SchemaNodeViewModel>();
            if (nodeTemp.Level == levelTemp)
            {
                nodeList.Add(nodeTemp);
            }
            else if (nodeTemp.Level < levelTemp)
            {
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {
                    nodeList.AddRange(GetNodesPerLevel(levelTemp, nodeTemp.Children[i]));
                }
            }
            return nodeList;
        }
        #endregion

        /// <summary>
        /// 更新误差点
        /// </summary>
        /// <param name="model">要更新的误差点</param>
        public void UpdateErrorPoint(ErrorModel model)
        {
            if (model.FlagRemove)//删除
            {
                string key = string.Format("{0}|{1}|{2}|{3}|{4}|", model.ErrorType, model.FangXiang, model.Component, model.Factor, model.Current);
                if (SelectedNode.ParaNo == ProjectID.初始固有误差)
                    key = string.Format("{0}|{1}|{2}|{3}", model.FangXiang, model.Component, model.Factor, model.Current);
                for (int i = 0; i < SelectedNode.ParaValuesCurrent.Count;)
                {
                    string keyT = SelectedNode.ParaValuesCurrent[i].GetProperty("PARA_VALUE") as string;
                    if ((!string.IsNullOrEmpty(keyT)) && keyT.Contains(key))
                    {
                        SelectedNode.ParaValuesCurrent.RemoveAt(i);
                        ParaValuesConvert();
                        continue;
                    }
                    i++;
                }
            }
            else
            {
                //误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序|相对Ib误差圈数
                DynamicViewModel dynamicViewModel = new DynamicViewModel(0);
                if (SelectedNode.ParaNo == ProjectID.初始固有误差) // 12007
                {
                    dynamicViewModel.SetProperty("功率方向", model.FangXiang);
                    dynamicViewModel.SetProperty("功率元件", model.Component);
                    dynamicViewModel.SetProperty("功率因数", model.Factor);
                    dynamicViewModel.SetProperty("电流倍数", model.Current);
                    dynamicViewModel.SetProperty("IsSelected", true);
                }
                else
                {
                    dynamicViewModel.SetProperty("误差试验类型", model.ErrorType);
                    dynamicViewModel.SetProperty("功率方向", model.FangXiang);
                    dynamicViewModel.SetProperty("功率元件", model.Component);
                    dynamicViewModel.SetProperty("功率因数", model.Factor);
                    dynamicViewModel.SetProperty("电流倍数", model.Current);
                    dynamicViewModel.SetProperty("添加谐波", "否");
                    dynamicViewModel.SetProperty("逆相序", "否");
                    dynamicViewModel.SetProperty("误差圈数(Ib)", model.LapCountIb);
                    dynamicViewModel.SetProperty("误差限倍数(%)", model.GuichengMulti);
                    dynamicViewModel.SetProperty("IsSelected", true);
                }
                DynamicModel modelTemp = ParaValueConvertBack(dynamicViewModel);
                if (modelTemp == null) return;

                #region 排序
                IEnumerable<string> valuesEnumrable = from item
                                                      in SelectedNode.ParaValuesCurrent
                                                      select GetErrorSortString(item.GetProperty("PARA_KEY") as string);
                string valueCurrent = modelTemp.GetProperty("PARA_KEY") as string;
                string valueSort = GetErrorSortString(valueCurrent);
                if (valuesEnumrable != null && valuesEnumrable.Count() > 0)
                {
                    for (int i = 0; i < valuesEnumrable.Count(); i++)
                    {
                        // Key由小到大排序
                        if (string.Compare(valueSort, valuesEnumrable.ElementAt(i)) < 0)
                        {
                            ParaValuesView.Insert(i, dynamicViewModel);
                            SelectedNode.ParaValuesCurrent.Insert(i, modelTemp);
                            SelectedNode.PointCount = SelectedNode.ParaValuesCurrent.Count;
                            return;
                        }
                    }
                }
                ParaValuesView.Add(dynamicViewModel);
                SelectedNode.ParaValuesCurrent.Add(modelTemp);
                SelectedNode.PointCount = SelectedNode.ParaValuesCurrent.Count;
                #endregion
            }
            RefreshPointCount();
        }
        public SchemaNodeViewModel SelectedNode { get; set; }

        /// <summary>
        /// 将当前的视图转换成方案配置信息
        /// </summary>
        /// <returns></returns>
        public List<DynamicModel> ParaValuesConvertBack()
        {
            List<DynamicModel> models = new List<DynamicModel>();
            if (SelectedNode == null)
            {
                return new List<DynamicModel>();
            }
            SelectedNode.ParaValuesCurrent.Clear();
            for (int i = 0; i < ParaValuesView.Count; i++)
            {
                DynamicModel modelTemp = ParaValueConvertBack(ParaValuesView[i]);
                models.Add(modelTemp);
            }
            return models;
        }
        /// <summary>
        /// 将视图模型转换成结论模型
        /// </summary>
        /// <param name="viewModelTemp"></param>
        /// <returns></returns>
        private DynamicModel ParaValueConvertBack(DynamicViewModel viewModelTemp)
        {
            #region 获取para_value和para_value_no
            string validFlag = "0";
            if (viewModelTemp.GetProperty("IsSelected") is bool b)
            {
                validFlag = b ? "1" : "0";
            }
            List<string> propertyNames = viewModelTemp.GetAllProperyName();
            propertyNames.Remove("IsSelected");

            string paraName = ParaInfo.ItemName;
            if (paraName == "")
                paraName = ParaInfo.CategoryName;

            string basicName = paraName;
            if (!ParaInfo.ContainProjectName)
            {
                paraName = "";
            }
            string paraCodeString = "";
            #region 参数
            List<string> valueList = new List<string>();
            List<string> codeList = new List<string>();
            for (int j = 0; j < propertyNames.Count; j++)
            {
                string value = viewModelTemp.GetProperty(propertyNames[j]) as string;
                string codeTemp = "";
                valueList.Add(value);
                if (ParaInfo.CheckParas.Count > j)
                {
                    codeTemp = CodeDictionary.GetValueLayer2(ParaInfo.CheckParas[j].ParaEnumType, value);
                    if (ParaInfo.CheckParas[j].IsNameMember)
                    {
                        paraName = paraName + "_" + value;
                    }
                    if (ParaInfo.CheckParas[j].IsKeyMember)
                    {
                        paraCodeString += codeTemp;
                    }
                }
                codeList.Add(codeTemp);
            }
            #endregion
            string paraKey = ParaInfo.ParaNo;
            if (!string.IsNullOrEmpty(paraCodeString))
            {
                paraKey = paraKey + "_" + paraCodeString;
            }
            #endregion
            string paraValue = string.Join("|", valueList);
            //string paraValueNo = string.Join("|", codeList);
            if (string.IsNullOrEmpty(paraName))
                paraName = basicName;

            DynamicModel modelTemp = new DynamicModel();
            modelTemp.SetProperty("PARA_NO", ParaNo);
            modelTemp.SetProperty("PARA_VALUE", paraValue);
            modelTemp.SetProperty("PARA_VALUE_NO", paraKey);
            modelTemp.SetProperty("PARA_KEY", paraKey);
            modelTemp.SetProperty("PARA_NAME", paraName.TrimStart('_'));
            modelTemp.SetProperty("CODE_ENABLED", validFlag);
            modelTemp.SetProperty("SCHEMA_ID", SchemaId);
            return modelTemp;
        }

        /// <summary>
        /// 加载当前选定检定点参数视图
        /// </summary>
        public void ParaValuesConvert()
        {
            if (SelectedNode == null)
                return;

            ParaValuesView.Clear();
            List<DynamicModel> models = SelectedNode.ParaValuesCurrent;
            IEnumerable<string> displayNames = from item in ParaInfo.CheckParas select item.ParaDisplayName;
            IEnumerable<string> enumCodes = from item in ParaInfo.CheckParas select item.ParaEnumType;
            IEnumerable<bool> keyRules = from item in ParaInfo.CheckParas select item.IsKeyMember;
            for (int i = 0; i < models.Count; i++)
            {
                DynamicViewModel dynamicViewModel = new DynamicViewModel(displayNames.ToList(), i);
                if (!(models.ElementAt(i).GetProperty("PARA_VALUE") is string stringParaValue))
                {
                    stringParaValue = "";
                }
                string[] arrayParaValue = stringParaValue.Split('|');
                int displayNamesCount = displayNames.Count();
                if (displayNamesCount == 1)
                {
                    dynamicViewModel.SetProperty(displayNames.ElementAt(0), stringParaValue);
                }
                else
                {
                    for (int j = 0; j < displayNamesCount; j++)
                    {
                        if (arrayParaValue.Length > j)
                        {
                            dynamicViewModel.SetProperty(displayNames.ElementAt(j), arrayParaValue[j]);
                        }
                        else
                        {
                            dynamicViewModel.SetProperty(displayNames.ElementAt(j), "");
                        }
                    }
                }
                bool isChecked = false;
                if (models[i].GetProperty("CODE_ENABLED") as string == "1")
                {
                    isChecked = true;
                }
                dynamicViewModel.SetProperty("IsSelected", isChecked);
                ParaValuesView.Add(dynamicViewModel);
            }
        }
        /// <summary>
        /// 添加检定点
        /// </summary>
        /// <param name="noTemp"></param>
        public SchemaNodeViewModel AddParaNode(string noTemp)
        {
            string itemName = SchemaFramework.GetItemName(noTemp);
            if (string.IsNullOrEmpty(itemName))
            {
                return null;
            }
            SchemaNodeViewModel nodeTemp = GetFirstNode(noTemp);
            ParaNo = noTemp;
            SelectedNode = nodeTemp;
            ParaValuesConvert();
            AddNewParaValue();
            List<DynamicModel> list = ParaValuesConvertBack();
            SelectedNode.ParaValuesCurrent.Clear();
            SelectedNode.ParaValuesCurrent.AddRange(list);
            RefreshPointCount();
            AddSpecialNode(nodeTemp);
            return nodeTemp;
        }

        #region 特殊检定点

        public void AddSpecialNode(SchemaNodeViewModel noTemp)
        {
            if (noTemp.PointCount != 1) //第一次添加
            {
                return;
            }
            if (noTemp.ParaNo == "18001")//误差一致性
            {
                AddSpecialNewParaValue(new string[] { "Ib", "0.5L" });
                AddSpecialNewParaValue(new string[] { "0.1Ib", "1.0" });

                List<DynamicModel> list = ParaValuesConvertBack();
                SelectedNode.ParaValuesCurrent.Clear();
                SelectedNode.ParaValuesCurrent.AddRange(list);
                RefreshPointCount();
            }
            else if (noTemp.ParaNo == "18002") //误差变差
            {
                //AddSpecialNewParaValue(new string[] { "5", "0.5L","10Itr" });
                List<DynamicModel> list = ParaValuesConvertBack();
                SelectedNode.ParaValuesCurrent.Clear();
                SelectedNode.ParaValuesCurrent.AddRange(list);
                //SelectedNode.ParaValuesCurrent = ParaValuesConvertBack();
                RefreshPointCount();
            }
        }

        public void AddSpecialNewParaValue(string[] value)
        {
            List<string> propertyNames = new List<string>();
            for (int i = 0; i < ParaInfo.CheckParas.Count; i++)
            {
                propertyNames.Add(ParaInfo.CheckParas[i].ParaDisplayName);
            }
            DynamicViewModel viewModel = new DynamicViewModel(propertyNames, 0);
            viewModel.SetProperty("IsSelected", true);
            for (int i = 0; i < propertyNames.Count; i++)
            {
                viewModel.SetProperty(propertyNames[i], value[i]);
            }
            ParaValuesView.Add(viewModel);

        }

        #endregion


        /// <summary>
        /// 恢复默认排序
        /// </summary>
        public void SortDefault()
        {
            ParaValues.Clear();
            List<DynamicModel> models = new List<DynamicModel>();
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].ParaValuesCurrent.Count > 0)
                {
                    models.AddRange(Children[i].ParaValuesCurrent);
                }
                else
                {
                    for (int j = 0; j < Children[i].Children.Count; j++)
                    {
                        for (int k = 0; k < Children[i].Children[j].PointCount; k++)
                        {
                            if (Children[i].Children[j].PointCount < 2)
                            {
                                models.Add(Children[i].Children[j].ParaValuesCurrent[k]);
                            }
                            else
                            {
                                models.Add(Children[i].Children[j].ParaValuesCurrent[k]);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < models.Count; i++)
            {
                ParaValues.Add(new DynamicViewModel(models[i], i));
            }
            ParaValues.Sort(item => GetSortString(item));
            InitialSchemaTree();
        }
        /// <summary>
        /// 获取排序字符串
        /// </summary>
        /// <param name="viewModelTemp">要排序的模型</param>
        private string GetSortString(DynamicViewModel viewModelTemp)
        {
            string keyString = viewModelTemp.GetProperty("PARA_KEY") as string;
            string para_no = viewModelTemp.GetProperty("PARA_NO") as string;
            DynamicModel modelFormat = SchemaFramework.GetParaFormat(para_no);
            string sort_no = "999";
            if (modelFormat != null)
            {
                sort_no = modelFormat.GetProperty("DEFAULT_SORT_NO") as string;
            }

            string sortKeyString = sort_no + "_" + keyString;
            //如果是基本误差
            if (keyString != null && (keyString.StartsWith(ProjectID.基本误差试验) || keyString.StartsWith(ProjectID.初始固有误差) || keyString.StartsWith(ProjectID.初始固有误差试验)))
            {
                return GetErrorSortString(sortKeyString);
            }
            if (keyString != null && (keyString.StartsWith(ProjectID.通讯协议检查试验) || keyString.StartsWith(ProjectID.通讯协议检查试验2)))
            {
                return sortKeyString.Substring(0, sortKeyString.LastIndexOf("_"));
            }
            return sortKeyString;
        }
        private string GetErrorSortString(string keyString)
        {
            if (keyString == null)
                return "";

            string[] arrayTemp = keyString.Split('_');
            if (arrayTemp.Length == 3)
            {
                //数据格式:排序号|误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                string strPara = arrayTemp[2];
                string currentString = strPara.Substring(4, 2);
                strPara = strPara.Remove(4, 2);
                strPara = strPara.Insert(3, currentString);
                return arrayTemp[0] + "_" + arrayTemp[1] + "_" + strPara;
            }
            else if (arrayTemp.Length == 2)
            {
                if (arrayTemp[0] == ProjectID.初始固有误差)
                {
                    // 电流编号为2位
                    // 输入Key:功率方向|功率元件|功率因数|电流倍数     11106
                    // 输出Key:功率方向|功率元件|99-电流倍数|功率因数
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(3, 2);
                    strPara = strPara.Remove(3, 2);
                    currentString = (99 - int.Parse(currentString)).ToString();
                    strPara = strPara.Insert(2, currentString);
                    return arrayTemp[0] + "_" + strPara;
                }
                else if (arrayTemp[0] == ProjectID.初始固有误差试验 || arrayTemp[0] == ProjectID.基本误差试验)
                {
                    //输入Key:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                    //输出Key:误差试验类型|功率方向|功率元件|电流倍数|功率因数|添加谐波|逆相序
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(4, 2); //电流
                    strPara = strPara.Remove(4, 2);
                    strPara = strPara.Insert(3, currentString);
                    // 功率方向
                    if (strPara[1] == '2')
                        strPara = strPara.Remove(1, 1).Insert(1, "3");
                    else if (strPara[1] == '3')
                        strPara = strPara.Remove(1, 1).Insert(1, "2");
                    return arrayTemp[0] + "_" + strPara;
                }
                else
                {
                    //输入Key:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                    //输出Key:误差试验类型|功率方向|功率元件|电流倍数|功率因数|添加谐波|逆相序
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(4, 2);
                    strPara = strPara.Remove(4, 2);
                    strPara = strPara.Insert(3, currentString);
                    return arrayTemp[0] + "_" + strPara;
                }

            }
            return keyString;
        }

        /// <summary>
        /// 电流大到小的初始固有误差
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        private string GetErrorSortString_Reversal(string keyString)
        {
            if (keyString == null)
            {
                return "";
            }
            string[] arrayTemp = keyString.Split('_');

            //数据格式:功率方向|功率元件|功率因数|电流倍数     11106      11061  11062
            string strPara = arrayTemp[1];
            string currentString = strPara.Substring(3, 2);
            string glysString = strPara.Substring(2, 1);
            strPara = strPara.Remove(2, 3);
            currentString = (99 - int.Parse(currentString)).ToString();//电流从大到晓

            glysString = (99 - int.Parse(glysString)).ToString(); //功率因数还是大到小
            strPara = strPara.Insert(2, currentString + glysString);
            return arrayTemp[0] + "_" + strPara;
        }

        /// <summary>
        /// 移动检定点
        /// </summary>
        /// <param name="nodeSource"></param>
        /// <param name="nodeDest"></param>
        public void MoveNode(SchemaNodeViewModel nodeSource, SchemaNodeViewModel nodeDest)
        {
            if (nodeDest == null || nodeSource == null || nodeSource.Equals(nodeDest))
            {
                return;
            }
            //终端节点,及包含了检定点的节点
            List<SchemaNodeViewModel> nodesTerminalSource = nodeSource.GetTerminalNodes();
            List<SchemaNodeViewModel> nodesTerminalDest = nodeDest.GetTerminalNodes();
            List<SchemaNodeViewModel> nodesTerminalAll = GetTerminalNodes();
            if (nodesTerminalDest.Count == 0 || nodesTerminalSource.Count == 0)
            {
                return;
            }
            for (int i = 0; i < nodesTerminalSource.Count; i++)
            {
                nodesTerminalAll.Remove(nodesTerminalSource[i]);
            }
            int insertIndex = nodesTerminalAll.IndexOf(nodesTerminalDest[0]);
            if (insertIndex >= 0)
            {
                for (int i = 0; i < nodesTerminalSource.Count; i++)
                {
                    nodesTerminalAll.Insert(insertIndex, nodesTerminalSource[i]);
                    insertIndex++;
                }
            }

            ParaValues.Clear();
            List<DynamicModel> models = new List<DynamicModel>();
            for (int i = 0; i < nodesTerminalAll.Count; i++)
            {
                if (nodesTerminalAll[i].ParaValuesCurrent.Count > 0)
                {
                    models.AddRange(nodesTerminalAll[i].ParaValuesCurrent);
                }
            }
            for (int i = 0; i < models.Count; i++)
            {
                ParaValues.Add(new DynamicViewModel(models[i], i));
            }
            InitialSchemaTree();
        }

        ///// <summary>
        ///// 获取最底层的检定点
        ///// </summary>
        ///// <param name="nodeTemp"></param>
        ///// <returns></returns>
        //private List<DynamicModel> GetParaValues(SchemaNodeViewModel nodeTemp)
        //{
        //    List<DynamicModel> modelsTemp = new List<DynamicModel>();
        //    List<SchemaNodeViewModel> nodesTerminal = nodeTemp.GetTerminalNodes();
        //    for (int i = 0; i < nodesTerminal.Count; i++)
        //    {
        //        modelsTemp.AddRange(nodesTerminal[i].ParaValuesCurrent);
        //    }
        //    return modelsTemp;
        //}

        #region 规程选点
        /// <summary>
        /// 设置当前方案规程误差点
        /// </summary>
        /// <param name="jjgParams"></param>
        public void SetJJGPlanPoint(JJGParams jjgParams)
        {
            if (jjgParams == null) return;
            if (string.IsNullOrWhiteSpace(jjgParams.JJGName)) return;
            if (!jjgParams.Forward && !jjgParams.Reverse) return;
            if (!jjgParams.Active && !jjgParams.Reactive) return;
            if (string.IsNullOrWhiteSpace(jjgParams.Wiring)) return;
            if (jjgParams.Active && string.IsNullOrWhiteSpace(jjgParams.ActiveClass)) return;
            if (jjgParams.Reactive && string.IsNullOrWhiteSpace(jjgParams.ReactiveClass)) return;

            Dictionary<string, SchemaNode> plan = new Dictionary<string, SchemaNode>();
            if (!GetJJGPoints(jjgParams, plan))
            {
                return;
            }

            SchemaNodeViewModel curErrNode = null;
            foreach (var item in plan)
            {
                var nodes = Children.Where(v => v.ParaNo == item.Key.Substring(0, 2));
                foreach (var sub in nodes)
                {
                    var subnodes = sub.Children.Where(x => x.ParaNo == item.Key);
                    if (subnodes.Any())
                    {
                        curErrNode = subnodes.First();
                    }
                    break;
                }
                break;
            }
            if (curErrNode == null)
            {
                curErrNode = GetLastNode(ProjectID.基本误差试验);
            }
            ParaValuesView.Clear();
            curErrNode.ParaValuesCurrent.Clear();
            curErrNode.PointCount = 0;
            SelectedNode = curErrNode;

            string displayString = "误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序|误差圈数(Ib)|误差限倍数(%)";
            string[] displayNames = displayString.Split('|');
            foreach (var item in plan)
            {
                ParaNo = item.Key;
                foreach (string points in item.Value.SchemaNodeValue)
                {

                    string[] nameValues = points.Split('|');
                    DynamicViewModel dynamicViewModel = new DynamicViewModel(0);
                    for (int i = 0; i < displayNames.Length; i++)
                    {
                        dynamicViewModel.SetProperty(displayNames[i], nameValues[i]);
                    }
                    dynamicViewModel.SetProperty("IsSelected", true);
                    DynamicModel modelTemp = ParaValueConvertBack(dynamicViewModel);
                    if (modelTemp == null) return;
                    ParaValuesView.Add(dynamicViewModel);
                    curErrNode.ParaValuesCurrent.Add(modelTemp);
                    curErrNode.PointCount = curErrNode.ParaValuesCurrent.Count;
                }
            }
            RefreshPointCount();
        }

        private bool GetJJGPoints(JJGParams jjgParams, Dictionary<string, SchemaNode> plan)
        {
            JJGPoints jjgPoints = null;
            if (jjgParams.JJGName == "JJG596-2012")
            {
                jjgPoints = new JJGPoints();
            }
            else if (jjgParams.JJGName == "JJG596-202X")
            {
                jjgPoints = new JJG596Points202X();
            }

            if (jjgPoints == null) return false;

            #region 误差点
            var errPoint = new SchemaNode();
            if (jjgParams.Transformer)//互感式
            {
                if (jjgParams.Wiring == "单相")
                {
                    if (jjgParams.Active)
                    {
                        if (jjgParams.ActiveClass == "2" || jjgParams.ActiveClass == "1" || jjgParams.ActiveClass == "A" || jjgParams.ActiveClass == "B")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass2(jjgParams.Imax4Ib, "正向有功"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass2(jjgParams.Imax4Ib, "反向有功"));
                            }
                        }
                        else if (jjgParams.ActiveClass == "0.5S" || jjgParams.ActiveClass == "0.2S" || jjgParams.ActiveClass == "C" || jjgParams.ActiveClass == "D")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass02S(jjgParams.Imax4Ib, "正向有功"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass02S(jjgParams.Imax4Ib, "反向有功"));
                            }
                        }
                    }

                    if (jjgParams.Reactive)
                    {
                        if (jjgParams.ReactiveClass == "3" || jjgParams.ReactiveClass == "2")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功"));
                            }
                        }
                        else
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass1(jjgParams.Imax4Ib, "正向无功"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass1(jjgParams.Imax4Ib, "反向无功"));
                            }
                        }
                    }
                }
                else if (jjgParams.Wiring == "三相四线" || jjgParams.Wiring == "三相三线")
                {
                    if (jjgParams.Active)
                    {
                        if (jjgParams.ActiveClass == "2" || jjgParams.ActiveClass == "A")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass2(jjgParams.Imax4Ib, "正向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass2(jjgParams.Imax4Ib, "正向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass2(jjgParams.Imax4Ib, "正向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass2(jjgParams.Imax4Ib, "正向有功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass2(jjgParams.Imax4Ib, "反向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass2(jjgParams.Imax4Ib, "反向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass2(jjgParams.Imax4Ib, "反向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass2(jjgParams.Imax4Ib, "反向有功", "C"));
                            }
                        }
                        else if (jjgParams.ActiveClass == "1" || jjgParams.ActiveClass == "B")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass1(jjgParams.Imax4Ib, "正向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass1(jjgParams.Imax4Ib, "正向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass1(jjgParams.Imax4Ib, "正向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass1(jjgParams.Imax4Ib, "正向有功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass1(jjgParams.Imax4Ib, "反向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass1(jjgParams.Imax4Ib, "反向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass1(jjgParams.Imax4Ib, "反向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass1(jjgParams.Imax4Ib, "反向有功", "C"));
                            }
                        }
                        else if (jjgParams.ActiveClass == "0.5S" || jjgParams.ActiveClass == "0.2S" || jjgParams.ActiveClass == "C" || jjgParams.ActiveClass == "D")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass02S(jjgParams.Imax4Ib, "正向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass02S(jjgParams.Imax4Ib, "正向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass02S(jjgParams.Imax4Ib, "正向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass02S(jjgParams.Imax4Ib, "正向有功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceActiveClass02S(jjgParams.Imax4Ib, "反向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass02S(jjgParams.Imax4Ib, "反向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass02S(jjgParams.Imax4Ib, "反向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceActiveClass02S(jjgParams.Imax4Ib, "反向有功", "C"));
                            }
                        }
                    }

                    if (jjgParams.Reactive)
                    {
                        if (jjgParams.ReactiveClass == "3" || jjgParams.ReactiveClass == "2")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功", "C"));
                            }
                        }
                        else
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass1(jjgParams.Imax4Ib, "正向无功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass1(jjgParams.Imax4Ib, "正向无功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass1(jjgParams.Imax4Ib, "正向无功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass1(jjgParams.Imax4Ib, "正向无功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerBalanceReactiveClass1(jjgParams.Imax4Ib, "反向无功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass1(jjgParams.Imax4Ib, "反向无功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass1(jjgParams.Imax4Ib, "反向无功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.TransformerImbalanceReactiveClass1(jjgParams.Imax4Ib, "反向无功", "C"));
                            }
                        }
                    }
                }
            }
            else //直接式
            {
                if (jjgParams.Wiring == "单相")
                {
                    if (jjgParams.Active)
                    {
                        if (jjgParams.ActiveClass == "2" || jjgParams.ActiveClass == "1" || jjgParams.ActiveClass == "A" || jjgParams.ActiveClass == "B")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceActiveClass2(jjgParams.Imax4Ib, "正向有功"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceActiveClass2(jjgParams.Imax4Ib, "反向有功"));
                            }
                        }
                    }

                    if (jjgParams.Reactive)
                    {
                        //if (jjgParams.ReactiveClass == "3" || jjgParams.ReactiveClass == "2")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功"));
                            }
                        }
                    }
                }
                else if (jjgParams.Wiring == "三相四线" || jjgParams.Wiring == "三相三线")
                {
                    if (jjgParams.Active)
                    {
                        if (jjgParams.ActiveClass == "2" || jjgParams.ActiveClass == "A")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceActiveClass2(jjgParams.Imax4Ib, "正向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass2(jjgParams.Imax4Ib, "正向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass2(jjgParams.Imax4Ib, "正向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass2(jjgParams.Imax4Ib, "正向有功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceActiveClass2(jjgParams.Imax4Ib, "反向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass2(jjgParams.Imax4Ib, "反向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass2(jjgParams.Imax4Ib, "反向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass2(jjgParams.Imax4Ib, "反向有功", "C"));
                            }
                        }
                        else if (jjgParams.ActiveClass == "1" || jjgParams.ActiveClass == "B")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceActiveClass1(jjgParams.Imax4Ib, "正向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass1(jjgParams.Imax4Ib, "正向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass1(jjgParams.Imax4Ib, "正向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass1(jjgParams.Imax4Ib, "正向有功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceActiveClass1(jjgParams.Imax4Ib, "反向有功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass1(jjgParams.Imax4Ib, "反向有功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass1(jjgParams.Imax4Ib, "反向有功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceActiveClass1(jjgParams.Imax4Ib, "反向有功", "C"));
                            }
                        }
                    }

                    if (jjgParams.Reactive)
                    {
                        //if (jjgParams.ReactiveClass == "3" || jjgParams.ReactiveClass == "2")
                        {
                            if (jjgParams.Forward)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceReactiveClass2(jjgParams.Imax4Ib, "正向无功", "C"));
                            }

                            if (jjgParams.Reverse)
                            {
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.BalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功"));
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功", "A"));
                                if (jjgParams.Wiring == "三相四线")
                                {
                                    errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功", "B"));
                                }
                                errPoint.SchemaNodeValue.AddRange(jjgPoints.ImbalanceReactiveClass2(jjgParams.Imax4Ib, "反向无功", "C"));
                            }

                        }
                    }
                }
            }

            if (!plan.ContainsKey(ProjectID.基本误差试验) && errPoint.SchemaNodeValue.Count > 0)
            {
                plan.Add(ProjectID.基本误差试验, errPoint);
            }
            #endregion 误差点

            return plan.Count > 0;
        }

        #endregion 规程选点
    }
}
