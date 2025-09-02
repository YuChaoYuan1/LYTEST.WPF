using LYTest.DAL;
using LYTest.DAL.DataBaseView;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.CheckInfo
{
    /// <summary>
    /// 一块表的详细结论
    /// </summary>
    public class OneMeterResult : ViewModelBase
    {
        /// <summary>
        /// 构造方法,获取一块表的结论
        /// </summary>
        /// <param name="meterPk">表的唯一编号</param>
        /// <param name="isTemp">临时库还是正式库</param>
        /// <param name="isFromDb">是否从数据库加载表信息</param>
        public OneMeterResult(string meterPk, bool isTemp)
        {
            if (isTemp)
            {
                LoadResultFromEquipmentData(meterPk);
            }
            else
            {
                LoadResultFromDb(meterPk, isTemp);
            }
        }
        /// <summary>
        /// 从数据库加载数据
        /// </summary>
        /// <param name="meterPk">表唯一编号</param>
        private void LoadResultFromDb(string meterPk, bool isTemp)
        {
            Categories.Clear();
            Dictionary<string, Dictionary<string, string>> resultDictionary = CheckResultBll.Instance.LoadOneResult(isTemp, meterPk);
            #region 条码号及是否要检
            GeneralDal dalClass = DALManager.MeterDbDal;
            DynamicModel modelTemp = DALManager.MeterDbDal.GetByID("METER_INFO", string.Format("METER_ID='{0}'", meterPk));
            if (modelTemp != null)
            {
                YaoJian = (modelTemp.GetProperty("MD_CHECKED") as string == "1") ? true : false;
                MeterInfo = new DynamicViewModel(modelTemp, 0);
            }
            else
            {
                return;
            }
            #endregion
            #region 提取ViewId
            Dictionary<string, List<string>> dictionaryViewTemp = new Dictionary<string, List<string>>();
            List<string> keyList = resultDictionary.Keys.ToList();
            for (int i = 0; i < keyList.Count; i++)
            {
                //arrayTemp[0]为检定点编号
                string[] arrayTemp = keyList[i].Split('_');
                if (arrayTemp.Length == 0)
                {
                    continue;
                }
                string viewId = ResultViewHelper.GetParaNoView(arrayTemp[0]);
                if (string.IsNullOrEmpty(viewId))
                {
                    continue;
                }
                if (dictionaryViewTemp.ContainsKey(viewId))
                {
                    dictionaryViewTemp[viewId].Add(keyList[i]);
                }
                else
                {
                    dictionaryViewTemp.Add(viewId, new List<string>() { keyList[i] });
                }
            }
            #endregion
            #region 结论分组
            for (int i = 0; i < dictionaryViewTemp.Count; i++)
            {
                ViewCategory category = new ViewCategory();
                category.ViewName = dictionaryViewTemp.Keys.ElementAt(i);
                List<string> keyCategory = dictionaryViewTemp.ElementAt(i).Value;
                for (int j = 0; j < keyCategory.Count; j++)
                {
                    DynamicViewModel modelTemp1 = new DynamicViewModel(j);
                    foreach (string columnHeader in resultDictionary[keyCategory[j]].Keys)
                    {
                        modelTemp1.SetProperty(columnHeader, resultDictionary[keyCategory[j]][columnHeader]);
                        if (!category.Names.Contains(columnHeader))
                        {
                            category.Names.Add(columnHeader);
                        }
                    }
                    category.ResultUnits.Add(modelTemp1);
                }
                Categories.Add(category);
            }
            #endregion
        }
        /// <summary>
        /// 从当前检定结论中加载检定数据
        /// </summary>
        /// <param name="meterPk">表唯一编号</param>
        private void LoadResultFromEquipmentData(string meterPk)
        {
            DynamicViewModel meterCurrent = EquipmentData.MeterGroupInfo.Meters.FirstOrDefault(item => item.GetProperty("METER_ID") as string == meterPk);
            if (meterCurrent == null)
            {
                return;
            }
            MeterInfo = meterCurrent;
            int meterIndex = EquipmentData.MeterGroupInfo.Meters.IndexOf(meterCurrent);
            YaoJian = EquipmentData.MeterGroupInfo.YaoJian[meterIndex];
            //string resultTotalTemp = MeterInfo.GetProperty("MD_RESULE") as string;
            for (int i = 0; i < EquipmentData.CheckResults.ResultCollection.Count; i++)
            {
                TableDisplayModel displayModel = EquipmentData.CheckResults.ResultCollection[i].DisplayModel;
                if (displayModel == null)
                {
                    continue;
                }
                string viewId = displayModel.ViewID;
                DynamicViewModel resultTemp = EquipmentData.CheckResults.ResultCollection[i].CheckResults[meterIndex];
                ViewCategory viewCategory = Categories.FirstOrDefault(item => item.ViewName == viewId);
                resultTemp.SetProperty("项目名", EquipmentData.CheckResults.ResultCollection[i].Name);
                resultTemp.SetProperty("项目号", EquipmentData.CheckResults.ResultCollection[i].ItemKey);
                if (viewCategory == null)
                {
                    viewCategory = new ViewCategory() { ViewName = viewId };
                    viewCategory.Names = new AsyncObservableCollection<string>(resultTemp.GetAllProperyName());
                    viewCategory.ResultUnits.Add(resultTemp);
                    Categories.Add(viewCategory);
                }
                else
                {
                    viewCategory.ResultUnits.Add(resultTemp);
                }
                //string itemResultTemp = resultTemp.GetProperty("结论") as string;
                //if (itemResultTemp == "不合格")
                //{
                //    resultTotalTemp = "不合格";
                //}
                //else if (itemResultTemp != "合格")
                //{
                //    if (resultTotalTemp == "合格")
                //    {
                //        resultTotalTemp = "";
                //    }
                //}
            }
            //MeterInfo.SetProperty("MD_RESULE", resultTotalTemp);
        }

        private bool yaoJian;
        /// <summary>
        /// 要检标记
        /// </summary>
        public bool YaoJian
        {
            get { return yaoJian; }
            set { SetPropertyValue(value, ref yaoJian, "YaoJian"); }
        }

        private AsyncObservableCollection<ViewCategory> categories = new AsyncObservableCollection<ViewCategory>();
        /// <summary>
        /// 结论列表
        /// </summary>
        public AsyncObservableCollection<ViewCategory> Categories
        {
            get { return categories; }
            set { SetPropertyValue(value, ref categories, "Categories"); }
        }

        /// <summary>
        /// 结论分类视图
        /// </summary>
        public class ViewCategory : ViewModelBase
        {
            private string viewName;
            /// <summary>
            /// 显示的名称
            /// </summary>
            public string ViewName
            {
                get { return viewName; }
                set { SetPropertyValue(value, ref viewName, "ViewName"); }
            }
            private AsyncObservableCollection<string> names = new AsyncObservableCollection<string>();
            /// <summary>
            /// 结论项的名称集合
            /// </summary>
            public AsyncObservableCollection<string> Names
            {
                get { return names; }
                set { SetPropertyValue(value, ref names, "Names"); }
            }

            private AsyncObservableCollection<DynamicViewModel> resultUnits = new AsyncObservableCollection<DynamicViewModel>();
            /// <summary>
            /// 结论列表
            /// </summary>
            public AsyncObservableCollection<DynamicViewModel> ResultUnits
            {
                get { return resultUnits; }
                set { SetPropertyValue(value, ref resultUnits, "ResultUnits"); }
            }

        }

        private DynamicViewModel meterInfo;
        /// <summary>
        /// 表信息
        /// </summary>
        public DynamicViewModel MeterInfo
        {
            get { return meterInfo; }
            set { SetPropertyValue(value, ref meterInfo, "MeterInfo"); }
        }
    }
}
