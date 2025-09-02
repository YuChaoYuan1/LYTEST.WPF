using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Model;
using LYTest.ViewModel.Schema;
using LYTest.ViewModel.Time;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LYTest.ViewModel.CheckInfo
{
    /// <summary>
    /// 检定结论视图模型
    /// </summary>
    public class CheckResultViewModel : ViewModelBase
    {
        public CheckResultViewModel()
        {
            DetailResultView.Clear();
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                DetailResultView.Add(new DynamicViewModel(i + 1));
            }
        }

        private CheckNodeViewModel checkNodeCurrent;
        /// <summary>
        /// 当前选中的检定点
        /// </summary>
        public CheckNodeViewModel CheckNodeCurrent
        {
            get { return checkNodeCurrent; }
            set
            {
                bool flagTemp = false;
                if (checkNodeCurrent == null || checkNodeCurrent.ParaNo != value.ParaNo)
                {
                    flagTemp = true;
                }
                FlagLoadColumn = true;
                SetPropertyValue(value, ref checkNodeCurrent, "CheckNodeCurrent");
                if (flagTemp)
                {
                    LoadViewColumn();
                }
                else
                {
                    RefreshDetailResult();
                }
            }
        }

        /// <summary>
        /// 检定结论集合
        /// </summary>
        public AsyncObservableCollection<CheckNodeViewModel> ResultCollection { get; set; } = new AsyncObservableCollection<CheckNodeViewModel>();


        #region 初始化检定结论
        /// 初始化检定结论
        /// <summary>
        /// 初始化检定结论
        /// </summary>
        /// <param name="schemaId">方案编号</param>
        public void InitialResult()
        {

            ResultCollection.Clear();
            Categories.Clear();

            for (int i = 0; i < EquipmentData.Schema.Children.Count; i++)
            {
                Categories.Add(GetResultNode(EquipmentData.Schema.Children[i]));
                Categories[Categories.Count - 1].NameFontSize = 12;

            }
            for (int i = 0; i < Categories.Count; i++)
            {
                for (int j = 0; j < Categories[i].Children.Count; j++)
                {
                    Categories[i].Children[j].CompressNode();
                }
            }

            // 加载检定结论
            try
            {
                for (int i = 0; i < ResultCollection.Count; i++)
                {
                    CheckResultBll.Instance.LoadCheckResult(ResultCollection[i]);
                    ResultCollection[i].RefreshResultSummary();
                }
                for (int i = 0; i < Categories.Count; i++)
                {
                    UpdateResultSummaryDown(Categories[i]);
                }
                //初始化时间统计
                TimeMonitor.Instance.Initialize();
            }
            catch (Exception) { }

        }

        /// <summary>
        /// 初始化方案节点对应的结论节点
        /// </summary>
        /// <param name="schemaNode"></param>
        /// <returns></returns>
        public CheckNodeViewModel GetResultNode(SchemaNodeViewModel schemaNode)
        {
            #region 方案相关信息
            CheckNodeViewModel categoryModel = new CheckNodeViewModel
            {
                IsSelected = schemaNode.IsSelected,
                Name = schemaNode.Name,
                ParaNo = schemaNode.ParaNo,
                Level = schemaNode.Level
            };
            #endregion
            #region 如果为根节点则加载所有表位的详细信息
            for (int i = 0; i < schemaNode.ParaValuesCurrent.Count; i++)
            {
                CheckNodeViewModel itemModel = new CheckNodeViewModel
                {
                    IsSelected = schemaNode.IsSelected,
                    Name = schemaNode.ParaValuesCurrent[i].GetProperty("PARA_NAME") as string,
                    ParaNo = schemaNode.ParaValuesCurrent[i].GetProperty("PARA_NO") as string,
                    ItemKey = schemaNode.ParaValuesCurrent[i].GetProperty("PARA_KEY") as string,
                    Level = schemaNode.Level + 1
                };
                //初始化详细结论
                itemModel.InitializeCheckResults();
                //设置父节点
                itemModel.Parent = categoryModel;
                //添加到总结论集合,方便使用
                ResultCollection.Add(itemModel);
                categoryModel.Children.Add(itemModel);
            }
            #endregion
            #region 对子节点递归
            for (int i = 0; i < schemaNode.Children.Count; i++)
            {
                CheckNodeViewModel nodeChild = GetResultNode(schemaNode.Children[i]);
                nodeChild.Parent = categoryModel;
                categoryModel.Children.Add(nodeChild);
            }
            #endregion
            return categoryModel;
        }
        #endregion
        /// <summary>
        /// 清除当前点的检定结论
        /// </summary>
        public void ResetCurrentResult()
        {
            if (ResultCollection.Count <= EquipmentData.Controller.Index || EquipmentData.Controller.Index < 0)
            {
                return;
            }

            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            List<string> listNames = ResultCollection[EquipmentData.Controller.Index].CheckResults[0].GetAllProperyName();
            #region 更新详细结论
            ////modify yjt 20220822 合并蒋工的代码
            //for (int i = 0; i < CheckNodeCurrent.CheckResults.Count; i++)
            //{
            //    //只更新要检表的结论
            //    if (yaojianTemp[i])
            //    {
            //        for (int j = 0; j < listNames.Count; j++)
            //        {
            //            if (listNames[j] != "要检")
            //            {
            //                CheckNodeCurrent.CheckResults[i].SetProperty(listNames[j], "");
            //            }
            //        }
            //    }
            //}

            if (EquipmentData.Schema.ParaNo == Core.Enum.ProjectID.初始固有误差)//初始固有误差清除结论--清除当前方向的数据，将结论改成待完成
            {
                DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[EquipmentData.Controller.Index];
                string name = viewModel.GetProperty("PARA_NAME") as string;//获取初始固有误差是那个方向
                name = name.EndsWith("_低到高") ? "上升" : "下降";
                List<string> setName = new List<string>() { "误差1", "误差2", "平均值", "化整值" };
                for (int i = 0; i < CheckNodeCurrent.CheckResults.Count; i++)
                {
                    //只更新要检表的结论
                    if (yaojianTemp[i])
                    {
                        for (int j = 0; j < setName.Count; j++)
                        {
                            CheckNodeCurrent.CheckResults[i].SetProperty(name + setName[j], "");
                        }
                        CheckNodeCurrent.CheckResults[i].SetProperty("差值", "");
                        string r = CheckNodeCurrent.CheckResults[i].GetProperty("结论") as string;
                        if (r != "")
                        {
                            CheckNodeCurrent.CheckResults[i].SetProperty("结论", "待完成");
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < CheckNodeCurrent.CheckResults.Count; i++)
                {
                    //只更新要检表的结论
                    if (yaojianTemp[i])
                    {
                        for (int j = 0; j < listNames.Count; j++)
                        {
                            if (listNames[j] != "要检")
                            {
                                CheckNodeCurrent.CheckResults[i].SetProperty(listNames[j], "");
                            }
                        }
                    }
                }
            }
            #endregion
            RefreshDetailResult();
            CheckNodeCurrent.RefreshResultSummary();
            UpdateResultSummaryUp(CheckNodeCurrent);
        }

        /// <summary>
        /// 清除当前点的检定结论
        /// </summary>
        public void ResetCurrentResult2(int Index)
        {
            if (ResultCollection.Count <= Index || Index < 0)
            {
                return;
            }

            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            List<string> listNames = ResultCollection[Index].CheckResults[0].GetAllProperyName();
            #region 更新详细结论
            for (int i = 0; i < CheckNodeCurrent.CheckResults.Count; i++)
            {
                //只更新要检表的结论
                if (yaojianTemp[i])
                {
                    for (int j = 0; j < listNames.Count; j++)
                    {
                        if (listNames[j] != "要检")
                        {
                            EquipmentData.CheckResults.ResultCollection[Index].CheckResults[i].SetProperty(listNames[j], "");
                        }
                    }
                }
            }
            #endregion
            RefreshDetailResult();
            EquipmentData.CheckResults.ResultCollection[Index].RefreshResultSummary();
            UpdateResultSummaryUp(EquipmentData.CheckResults.ResultCollection[Index]);
        }

        /// <summary>
        /// 清除所有的结论
        /// </summary>
        public void ClearAllResult()
        {
            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            for (int j = 0; j < ResultCollection.Count; j++)
            {
                List<string> listNames = ResultCollection[j].CheckResults[0].GetAllProperyName();
                #region 更新详细结论
                for (int i = 0; i < ResultCollection[j].CheckResults.Count; i++)
                {
                    try
                    {
                        ResultCollection[j].IsChecked = false;
                    }
                    catch (Exception) { }
                    //只更新要检表的结论
                    if (yaojianTemp[i])
                    {
                        for (int k = 0; k < listNames.Count; k++)
                        {
                            if (listNames[k] != "要检")
                            {
                                ResultCollection[j].CheckResults[i].SetProperty(listNames[k], "");
                            }
                        }
                    }
                }
                #endregion
                ResultCollection[j].RefreshResultSummary();
            }
            for (int i = 0; i < Categories.Count; i++)
            {
                UpdateResultSummaryDown(Categories[i]);
            }
        }
        public object Lock = new object();

        /// <summary>
        /// 更新检定结论
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="columnName"></param>
        /// <param name="arrayResult"></param>
        public void UpdateCheckResult(string itemKey, string columnName, string[] arrayResult) //,string[] ErrorReason=null
        {
            bool[] yaoJianTemp = EquipmentData.MeterGroupInfo.YaoJian;

            #region 判断编号和结论


            CheckNodeViewModel nodeTemp = GetResultNode(itemKey);


            if (nodeTemp == null || nodeTemp.CheckResults.Count < 0)
            {
                LogManager.AddMessage($"未找到检定点编号{itemKey}对应的检定点编号", EnumLogSource.检定业务日志, EnumLevel.Warning);
                return;
            }

            if (ConstHelper.检定界面显示实时误差.Equals(columnName))
            {
                nodeTemp.RefreshResultValueRealtime();
                return;
            }

            List<string> listNames = nodeTemp.CheckResults[0].GetAllProperyName();
            if (!listNames.Contains(columnName))
            {
                LogManager.AddMessage($"不识别的检定结论:{columnName}", EnumLogSource.检定业务日志, EnumLevel.Warning);
                return;
            }
            #endregion


            CheckNodeViewModel nodeTempTow = null;
            if (itemKey.StartsWith(ProjectID.初始固有误差))
            {
                nodeTempTow = GetResultNodeTow(itemKey); //找到对应的第二个
            }

            #region 更新详细结论

            //更新列数据
            for (int i = 0; i < nodeTemp.CheckResults.Count; i++)
            {
                if (arrayResult.Length > i)
                {
                    if (yaoJianTemp[i])   //只更新要检表的结论
                    {
                        if (!(arrayResult[i] == null))
                        {
                            nodeTemp.CheckResults[i].SetProperty(columnName, arrayResult[i]);
                            nodeTempTow?.CheckResults[i].SetProperty(columnName, arrayResult[i]);
                        }
                    }
                }
            }

            lock (Lock)
            {
                if (columnName == "结论")
                {
                    nodeTemp.RefreshResultSummary();
                    UpdateResultSummaryUp(nodeTemp);
                    if (nodeTempTow != null)
                    {
                        nodeTempTow.RefreshResultSummary();
                        UpdateResultSummaryUp(nodeTempTow);
                    }
                    CheckResultBll.Instance.SaveCheckResult(nodeTemp);

                    int indexTemp = ResultCollection.IndexOf(nodeTemp);
                    TimeMonitor.Instance.ActiveCurrentItem(indexTemp, true);
                    //TimeMonitor.Instance.FinishCurrentItem(indexTemp);

                    #region 更新总结论

                    List<string> sqlSumRetList = new List<string>();
                    string[] strResult = new string[] { "合格" };

                    int IsQualifiedNum = 0;
                    for (int i = 0; i < nodeTemp.CheckResults.Count; i++)
                    {
                        if (arrayResult.Length > i)
                        {
                            //只更新要检表的结论
                            if (yaoJianTemp[i])
                            {
                                if (!string.IsNullOrEmpty(arrayResult[i]))
                                {
                                    //获得表iD
                                    string meterId = EquipmentData.MeterGroupInfo.Meters[i].GetProperty("METER_ID") as string;
                                    //查询数据库中id等于这个表的数据
                                    string strSQL = string.Format("METER_ID  = '{0}'", meterId);
                                    //大项目结论表
                                    List<DynamicModel> models = DALManager.MeterTempDbDal.GetList("T_TMP_METER_COMMUNICATION", strSQL);

                                    if (models.Count > 0)
                                    {
                                        strResult = new string[models.Count];
                                        for (int j = 0; j < models.Count; j++)
                                        {
                                            //大项目结论
                                            strResult[j] = models[j].GetProperty("MD_RESULT") as string;
                                        }
                                    }
                                    //MD_RESULE

                                    //设置整个表的总结论
                                    if (Array.IndexOf(strResult, "不合格") > -1)  //有一个数据不合格，总结论不合格
                                    {
                                        IsQualifiedNum += 1;
                                        sqlSumRetList.Add(string.Format("update T_TMP_METER_INFO set MD_RESULT ='{0}' where METER_ID  = '{1}'", "不合格", meterId));
                                        EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_RESULT", "不合格");
                                    }
                                    else
                                    {
                                        sqlSumRetList.Add(string.Format("update T_TMP_METER_INFO set MD_RESULT ='{0}' where METER_ID  = '{1}'", "合格", meterId));
                                        EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_RESULT", "合格");
                                    }
                                }
                            }
                        }
                    }
                    int countTemp = DALManager.MeterTempDbDal.ExecuteOperation(sqlSumRetList);
                    LogManager.AddMessage(string.Format("更新数据库中总结论信息完成,共更新{0}条", countTemp), EnumLogSource.数据库存取日志);

                    // modify yjt jx 20230205 修改
                    #region ADD WKW 20220621

                    //if (IsQualifiedNum >= VerifyConfig.MaxErrorNumber)
                    //{
                    //    //LogManager.AddMessage("要检表不合格，请重新操作！", EnumLogSource.检定业务日志, EnumLevel.Tip);
                    //    //EquipmentData.Controller.TryStopVerify();
                    //    //return;
                    //}
                    //else
                    //{
                    //    #region ADD WKW 20220609
                    //    if (EquipmentData.DeviceManager.Devices.ContainsKey(Device.DeviceName.多功能板))
                    //    {
                    //        EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.绿, 1);
                    //    }
                    //    #endregion
                    //}

                    #endregion
                    // modify yjt jx 20230205 修改
                    #region ADD WKW 20220621

                    if (IsQualifiedNum >= VerifyConfig.MaxErrorNumber)
                    {
                        LogManager.AddMessage("要检表不合格数量超标", EnumLogSource.检定业务日志, EnumLevel.Tip);
                        EquipmentData.Controller.TryStopVerify();
                    }

                    #endregion

                    #endregion

                }
            }


            #endregion

            RefreshDetailResult(); //刷新

        }
        /// <summary>
        /// 更新修约值
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="meterClass"></param>
        /// <param name="regulation"></param>
        internal void UpdateRoundingValueAndResult(int pos, string meterClass, string regulation)
        {
            if (pos < 0 || pos >= EquipmentData.Equipment.MeterCount) return;

            for (int i = 0; i < ResultCollection.Count; i++)
            {
                CheckNodeViewModel cknode = ResultCollection[i];
                if (cknode.ParaNo == ProjectID.基本误差试验 && !string.IsNullOrWhiteSpace(cknode.ItemKey))
                {
                    DynamicViewModel dymodel = cknode.CheckResults[pos];
                    if (dymodel == null)
                    {
                        continue;
                    }
                    string element = dymodel.GetProperty("功率元件") as string;
                    string direction = dymodel.GetProperty("功率方向") as string;
                    string currentIx = dymodel.GetProperty("电流倍数") as string;
                    string factor = dymodel.GetProperty("功率因数") as string;
                    string average = dymodel.GetProperty("平均值") as string;
                    string round = dymodel.GetProperty("化整值") as string;
                    string result = dymodel.GetProperty("结论") as string;
                    if (!string.IsNullOrWhiteSpace(round))
                    {
                        //update 修约值
                        bool active = direction.IndexOf("无功") < 0;
                        float mclass = Core.Function.Number.GetMeterPdClass(meterClass, active, out string pdClass);
                        float space = Core.Function.Number.GetRoundingSpace(false, mclass);
                        string roundValue = Core.Function.Number.GetRounding(float.Parse(average), space);
                        dymodel.SetProperty("化整值", roundValue);
                        //update 误差上限、下
                        string limit = ErrLimitHelper.Wcx(currentIx, regulation, pdClass, (Cus_PowerYuanJian)Enum.Parse(typeof(Cus_PowerYuanJian), element), factor, false, active);
                        dymodel.SetProperty("误差上限", limit);
                        dymodel.SetProperty("误差下限", $"-{limit}");
                        //update 结论
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            string resultNew = ConstHelper.不合格;
                            if (Math.Abs(float.Parse(roundValue)) <= Math.Abs(float.Parse(limit)))
                                resultNew = ConstHelper.合格;

                            if (result != resultNew)
                                dymodel.SetProperty("结论", resultNew);
                        }
                    }

                    cknode.RefreshResultSummary();
                    CheckResultBll.Instance.SaveCheckResult(cknode);
                }

            }

        }

        //modify yjt 20220822 合并蒋工的代码
        /// <summary>
        /// 获得检定项目的详细数据
        /// </summary>
        public Dictionary<string, string[]> GetCheckResult()
        {
            Dictionary<string, string[]> values = new Dictionary<string, string[]>();
            if (ResultCollection.Count <= EquipmentData.Controller.Index || EquipmentData.Controller.Index < 0)
            {
                return null;
            }

            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            List<string> listNames = ResultCollection[EquipmentData.Controller.Index].CheckResults[0].GetAllProperyName();
            for (int i = 0; i < listNames.Count; i++)
            {
                if (!values.ContainsKey(listNames[i]))
                {
                    values.Add(listNames[i], new string[yaojianTemp.Length]);
                }
            }
            for (int i = 0; i < CheckNodeCurrent.CheckResults.Count; i++)
            {
                //只更新要检表的结论
                if (yaojianTemp[i])
                {
                    for (int j = 0; j < listNames.Count; j++)
                    {
                        if (listNames[j] != "要检")
                        {
                            values[listNames[j]][i] = CheckNodeCurrent.CheckResults[i].GetProperty(listNames[j]) as string;
                        }
                    }
                }
            }
            return values;

        }
        /// <summary>
        /// 合格率n%
        /// </summary>
        /// <returns></returns>
        public float GetPassRate()
        {
            int checkCount = 0;
            int passCount = 0;
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;
                checkCount++;
                if (GetMeterResult(i))
                {
                    passCount++;
                }
            }
            if (checkCount == 0) return 0;
            return passCount / (float)checkCount * 100;
        }
        public bool GetMeterResult(int meterNo, string exceptId = "")
        {
            if (meterNo >= EquipmentData.MeterGroupInfo.Meters.Count)
            {
                return false;
            }
            string str = EquipmentData.MeterGroupInfo.Meters[meterNo].GetProperty("MD_RESULT").ToString();
            if (str == "合格")
            {
                return true;
            }
            else
            {
                //获得表iD
                string meterId = EquipmentData.MeterGroupInfo.Meters[meterNo].GetProperty("METER_ID") as string;
                //查询数据库中id等于这个表的数据
                string strSQL = string.Format("METER_ID  = '{0}'", meterId);
                //大项目结论表
                List<DynamicModel> models = DALManager.MeterTempDbDal.GetList("T_TMP_METER_COMMUNICATION", strSQL);
                string[] strResult = new string[] { "合格" };
                if (models.Count > 0)
                {
                    strResult = new string[models.Count];
                    for (int j = 0; j < models.Count; j++)
                    {
                        //大项目结论
                        string s = models[j].GetProperty("MD_PROJECT_NO") as string;
                        if (s == exceptId) continue;
                        strResult[j] = models[j].GetProperty("MD_RESULT") as string;
                    }
                }

                //设置整个表的总结论
                if (Array.IndexOf(strResult, "不合格") > -1)  //有一个数据不合格，总结论不合格
                {
                    return false;
                }
                return true;
            }
        }
        public bool GetMeterResultExceptMyself(int meterNo, string exceptId = "")
        {
            if (meterNo >= EquipmentData.MeterGroupInfo.Meters.Count)
            {
                return false;
            }

            //获得表iD
            string meterId = EquipmentData.MeterGroupInfo.Meters[meterNo].GetProperty("METER_ID") as string;
            //查询数据库中id等于这个表的数据
            string strSQL = string.Format("METER_ID  = '{0}'", meterId);
            //大项目结论表
            List<DynamicModel> models = DALManager.MeterTempDbDal.GetList("T_TMP_METER_COMMUNICATION", strSQL);
            string MD_SCHEME_ID = EquipmentData.MeterGroupInfo.Meters[meterNo].GetProperty("MD_SCHEME_ID") as string;

            List<DynamicModel> schmeModels = DALManager.SchemaDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), $@"SCHEMA_ID  = {MD_SCHEME_ID}");

            string[] strResult = new string[] { "合格" };
            if (models.Count != schmeModels.Count - 1)
            {
                return false;
            }


            if (models.Count > 0)
            {
                strResult = new string[models.Count];
                for (int j = 0; j < models.Count; j++)
                {
                    //大项目结论
                    string s = models[j].GetProperty("MD_PROJECT_NO") as string;
                    if (s == exceptId)
                    {
                        strResult[j] = "合格";
                        continue;
                    }
                    strResult[j] = models[j].GetProperty("MD_RESULT") as string;
                }
            }



            //设置整个表的总结论
            if (Array.IndexOf(strResult, "不合格") > -1 || Array.IndexOf(strResult, "") > -1)  //有一个数据不合格，总结论不合格
            {
                return false;
            }
            return true;
        }

        #region 刷新结论总览,从上往下和从下往上两个方法
        public void UpdateResultSummaryDown(CheckNodeViewModel nodeTop)
        {
            if (nodeTop.CheckResults.Count > 0)
            {
                return;
            }
            else
            {
                nodeTop.RefreshResultSummary();
                for (int i = 0; i < nodeTop.Children.Count; i++)
                {
                    nodeTop.Children[i].RefreshResultSummary();
                }
            }
        }
        public void UpdateResultSummaryUp(CheckNodeViewModel nodeBottom)
        {
            CheckNodeViewModel nodeParent = nodeBottom.Parent;
            while (nodeParent != null)
            {
                //if (nodeParent != null)
                //{
                nodeParent?.RefreshResultSummary();
                //}
                nodeParent = nodeParent.Parent;
            }
        }
        #endregion


        private CheckNodeViewModel GetResultNode(string itemKey)
        {
            int indexTemp = EquipmentData.Controller.Index;
            CheckNodeViewModel nodeResult = null;
            for (; indexTemp >= 0; indexTemp--)
                if (indexTemp < ResultCollection.Count)
                {
                    CheckNodeViewModel nodeTemp = ResultCollection[indexTemp];
                    if (nodeTemp.ItemKey == itemKey && nodeTemp.IsSelected)
                    {
                        nodeResult = nodeTemp;
                        break;
                    }
                }
            if (nodeResult == null)
            {
                indexTemp = EquipmentData.Controller.Index;
                for (int i = indexTemp; i < ResultCollection.Count; i++)
                {
                    CheckNodeViewModel nodeTemp = ResultCollection[i];
                    if (nodeTemp.ItemKey == itemKey && nodeTemp.IsSelected)
                    {
                        nodeResult = nodeTemp;
                        break;
                    }
                }
            }
            return nodeResult;
        }
        private CheckNodeViewModel GetResultNodeTow(string itemKey)
        {
            int indexTemp = EquipmentData.Controller.Index;
            CheckNodeViewModel nodeResult = null;
            for (int i = 0; i < ResultCollection.Count; i++)
            {
                if (i == indexTemp) continue;//排除自身外的另一个
                CheckNodeViewModel nodeTemp = ResultCollection[i];
                if (nodeTemp.ItemKey == itemKey && nodeTemp.IsSelected)
                {
                    nodeResult = nodeTemp;
                    break;
                }
            }
            return nodeResult;
        }
        /// <summary>
        /// 更新表位要检状态
        /// </summary>
        public void UpdateYaoJian()
        {
            for (int i = 0; i < ResultCollection.Count; i++)
            {
                UpdateYaoJian(i);
            }
        }

        /// <summary>
        /// 更新要检标记,序号从0开始
        /// </summary>
        /// <param name="meterIndex"></param>
        public void UpdateYaoJian(int meterIndex)
        {
            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            for (int j = 0; j < ResultCollection.Count; j++)
            {
                if (ResultCollection.Count > j && ResultCollection[j].CheckResults.Count > meterIndex)
                {
                    ResultCollection[j].CheckResults[meterIndex].SetProperty("要检", yaojianTemp[meterIndex]);
                }
            }
        }

        //private AsyncObservableCollection<CheckNodeViewModel> categories = new AsyncObservableCollection<CheckNodeViewModel>();

        //private AsyncObservableCollection<CheckNodeViewModel> categories = new AsyncObservableCollection<CheckNodeViewModel>();

        /// <summary>
        /// 检定大类列表
        /// </summary>
        public AsyncObservableCollection<CheckNodeViewModel> Categories { get; } = new AsyncObservableCollection<CheckNodeViewModel>();
        //{
        //    get { return categories; }
        //    set { SetPropertyValue(value, ref categories, "Categories"); }
        //}

        #region 更新主界面检定数据
        //private AsyncObservableCollection<DynamicViewModel> detailResultView = new AsyncObservableCollection<DynamicViewModel>();
        /// <summary>
        /// 当前显示的检定点
        /// 将界面显示的检定点结论模型固定,不重新设置值,这样可以大大提高界面绑定数据的速度
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> DetailResultView { get; } = new AsyncObservableCollection<DynamicViewModel>();
        //{
        //    get { return detailResultView; }
        //    set { SetPropertyValue(value, ref detailResultView, "DetailResultView"); }
        //}
        /// <summary>
        /// 更改显示结论的列
        /// </summary>
        private void LoadViewColumn()
        {
            for (int i = 0; i < DetailResultView.Count; i++)
            {
                List<string> propertyNames = DetailResultView[i].GetAllProperyName();
                for (int j = 0; j < propertyNames.Count; j++)
                {
                    if (propertyNames[j] != "要检")
                    {
                        DetailResultView[i].RemoveProperty(propertyNames[j]);
                    }
                }
                if (CheckNodeCurrent.CheckResults.Count <= i)
                {
                    continue;
                }
                propertyNames = CheckNodeCurrent.CheckResults[i].GetAllProperyName();
                for (int j = 0; j < propertyNames.Count; j++)
                {
                    DetailResultView[i].SetProperty(propertyNames[j], CheckNodeCurrent.CheckResults[i].GetProperty(propertyNames[j]));
                }
            }
            FlagLoadColumn = false;
        }
        /// <summary>
        /// 更新当前显示的数据
        /// </summary>
        private void RefreshDetailResult()
        {
            Parallel.For(0, DetailResultView.Count, (i) =>
            {
                try
                {
                    List<string> propertyNames = CheckNodeCurrent.CheckResults[i].GetAllProperyName();
                    for (int j = 0; j < propertyNames.Count; j++)
                    {
                        DetailResultView[i].SetProperty(propertyNames[j], CheckNodeCurrent.CheckResults[i].GetProperty(propertyNames[j]));
                    }
                }
                catch
                {
                }
            });
        }
        private bool flagLoadColumn;
        /// <summary>
        /// 列加载完毕标记
        /// 如果检定结论视图发生了变化,值会变成true,界面加载完毕以后值会变为false,该标记用于防止界面加载过于频繁影响速度
        /// </summary>
        public bool FlagLoadColumn
        {
            get { return flagLoadColumn; }
            set
            {
                SetPropertyValue(value, ref flagLoadColumn, "FlagLoadColumn");
            }
        }
        #endregion
        /// <summary>
        /// 更新详细结论中的要检标记
        /// </summary>
        public void RefreshYaojian()
        {
            bool[] yaojian = EquipmentData.MeterGroupInfo.YaoJian;
            for (int i = 0; i < ResultCollection.Count; i++)
            {
                for (int j = 0; j < EquipmentData.Equipment.MeterCount; j++)
                {
                    if (yaojian.Length > j)
                    {
                        ResultCollection[i].CheckResults[j].SetProperty("要检", yaojian[j]);
                    }
                }
            }
            //更新界面显示
            for (int j = 0; j < EquipmentData.Equipment.MeterCount; j++)
            {
                if (yaojian.Length > j)
                {
                    DetailResultView[j].SetProperty("要检", yaojian[j]);
                }
            }
        }
    }
}
