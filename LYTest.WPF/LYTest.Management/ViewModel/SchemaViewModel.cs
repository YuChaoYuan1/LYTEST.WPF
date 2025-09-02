using LYTest.Core.Enum;
using LYTest.DAL;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager
{
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
                LogManager.AddMessage(string.Format("未能加载编号为 {0} 的方案.", idSchema), EnumLogSource.用户操作日志, EnumLevel.Warning);
                return;
            }
            Name = model.GetProperty("SCHEMA_NAME") as string;
            LoadParaValues();
            LogManager.AddMessage(string.Format("当前方案:{0}", Name), EnumLogSource.用户操作日志, EnumLevel.Information);
        }
        //private AsyncObservableCollection<DynamicViewModel> paraValues = new AsyncObservableCollection<DynamicViewModel>();
        /// <summary>
        /// 参数值
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> ParaValues { get; } = new AsyncObservableCollection<DynamicViewModel>();
        //{
        //    get { return paraValues; }
        //    set { SetPropertyValue(value, ref paraValues, "ParaValues"); }
        //}

        private ParaInfoViewModel paraInfo = new ParaInfoViewModel();
        /// 检定点参数信息
        /// <summary>
        /// 检定点参数信息
        /// </summary>
        public ParaInfoViewModel ParaInfo
        {
            get { return paraInfo; }
            set { SetPropertyValue(value, ref paraInfo, "ParaInfo"); }
        }

        #region 参数值的配置
        private AsyncObservableCollection<DynamicViewModel> paraValuesView = new AsyncObservableCollection<DynamicViewModel>();
        /// 参数值
        /// <summary>
        /// 参数值
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> ParaValuesView
        {
            get { return paraValuesView; }
            set { SetPropertyValue(value, ref paraValuesView, "ParaValuesView"); }
        }
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
                level++;
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
                if (b)
                {
                    validFlag = "1";
                }
            }
            List<string> propertyNames = viewModelTemp.GetAllProperyName();
            propertyNames.Remove("IsSelected");
            List<string> valueList = new List<string>();
            List<string> codeList = new List<string>();
            string paraName = ParaInfo.ItemName;
            if (paraName == "")
            {
                paraName = ParaInfo.CategoryName;
            }
            string basicName = paraName;
            if (!ParaInfo.ContainProjectName)
            {
                paraName = "";
            }
            string paraKey = ParaInfo.ParaNo;
            string paraCodeString = "";
            #region 参数
            for (int j = 0; j < propertyNames.Count; j++)
            {
                string temp = viewModelTemp.GetProperty(propertyNames[j]) as string;
                string codeTemp = "";
                valueList.Add(temp);
                if (ParaInfo.CheckParas.Count > j)
                {
                    codeTemp = CodeDictionary.GetValueLayer2(ParaInfo.CheckParas[j].ParaEnumType, temp);
                    if (ParaInfo.CheckParas[j].IsNameMember)
                    {
                        paraName = paraName + "_" + temp;
                    }
                    if (ParaInfo.CheckParas[j].IsKeyMember)
                    {
                        paraCodeString += codeTemp;
                    }
                }
                codeList.Add(codeTemp);
            }
            #endregion
            if (!string.IsNullOrEmpty(paraCodeString))
            {
                paraKey = paraKey + "_" + paraCodeString;
            }
            #endregion
            string paraValue = string.Join("|", valueList);
            //string paraValueNo = string.Join("|", codeList);

            DynamicModel modelTemp = new DynamicModel();
            modelTemp.SetProperty("PARA_NO", ParaNo);
            modelTemp.SetProperty("PARA_VALUE", paraValue);
            modelTemp.SetProperty("PARA_VALUE_NO", paraKey);
            modelTemp.SetProperty("PARA_KEY", paraKey);
            if (string.IsNullOrEmpty(paraName))
            {
                paraName = basicName;
            }
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
            {
                return;
            }
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
            {
                return "";
            }
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
                    //数据格式:功率方向|功率元件|功率因数|电流倍数     11106
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(3, 2);
                    strPara = strPara.Remove(3, 2);
                    currentString = (99 - int.Parse(currentString)).ToString();//电流从小到大
                    strPara = strPara.Insert(2, currentString);
                    return arrayTemp[0] + "_" + strPara;
                }
                else if (arrayTemp[0] == ProjectID.初始固有误差试验 || arrayTemp[0] == ProjectID.基本误差试验)
                {
                    //数据格式:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
                    string strPara = arrayTemp[1];
                    string currentString = strPara.Substring(4, 2);
                    strPara = strPara.Remove(4, 2);
                    strPara = strPara.Insert(3, currentString);
                    if (strPara[1] == '2')
                        strPara = strPara.Remove(1, 1).Insert(1, "3");
                    else if (strPara[1] == '3')
                        strPara = strPara.Remove(1, 1).Insert(1, "2");
                    return arrayTemp[0] + "_" + strPara;
                }
                else
                {
                    //数据格式:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
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


    }
}
