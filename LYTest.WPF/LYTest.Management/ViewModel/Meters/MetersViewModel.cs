using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.DAL;
using LYTest.DataManager.ViewModel.Meters;
using LYTest.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LYTest.DataManager.ViewModel.Meters
{
    /// <summary>
    /// 表信息模型
    /// </summary>
    public class MetersViewModel : ViewModelBase
    {
        private static int CanUpdate = 0;
        private SelectCollection<OneMeterResult> resultCollection = new SelectCollection<OneMeterResult>();

        /// <summary>
        /// 所有检定结论
        /// </summary>
        public SelectCollection<OneMeterResult> ResultCollection
        {
            get { return resultCollection; }
            set { SetPropertyValue(value, ref resultCollection, "ResultCollection"); }
        }

        public MetersViewModel()
        {
            PagerModel.PageSize = 60;
            PagerModel.EventUpdateData += PagerModel_EventUpdateData;
            Array arrayTemp = Enum.GetValues(typeof(EnumCompare));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                EnumCompare enumTemp = (EnumCompare)arrayTemp.GetValue(i);
                if (enumTemp != EnumCompare.自定义筛选条件)
                {
                    SearchMenuItem searchItem = new SearchMenuItem()
                    {
                        CompareExpression = enumTemp
                    };
                    FilterCollection.Add(searchItem);
                }
                else
                {
                    SearchMenuItem searchItem = new SearchMenuItem()
                    {
                        CompareExpression = enumTemp,
                        SearchItemChild = new SearchMenuItem()
                        {
                            CompareExpression = EnumCompare.等于
                        }
                    };
                    FilterCollection.Add(searchItem);
                }
            }
            #region 直接选择查询条件
            var names = from item in ParasModel.AllUnits select item.DisplayName;
            ObservableCollection<string> nameCollection = new ObservableCollection<string>(names);
            nameCollection.Add("检定日期(期间)");
            Array arrayTemp1 = Enum.GetValues(typeof(EnumCompare));
            //modify yjt 20220816 修改查询条件的默认值
            //string[] columnNamesTemp = new string[] { "条形码" };
            string[] columnNamesTemp = new string[] { "检定日期" };
            for (int i = 0; i < columnNamesTemp.Length; i++)
            {
                SearchItemModel modelTemp = new SearchItemModel();
                modelTemp.PropertyChanged += ModelTemp_PropertyChanged;
                modelTemp.ColumnNames = nameCollection;
                if (nameCollection.Count > i)
                {
                    modelTemp.ColumnName = columnNamesTemp[i];
                }
                foreach (EnumCompare valueTemp in arrayTemp1)
                {
                    if (valueTemp != EnumCompare.清空筛选条件 && valueTemp != EnumCompare.自定义筛选条件)
                    {
                        modelTemp.Conditions.Add(valueTemp);
                    }
                }
                SearchItems.Add(modelTemp);
            }
            #endregion
        }
        /// <summary>
        /// 从正式库加载表信息
        /// </summary>
        /// <param name="meters"></param>
        public MetersViewModel(IEnumerable<DynamicViewModel> meters)
        {
            LoadMeters(meters);
        }
        public void LoadMeters(IEnumerable<DynamicViewModel> meters)
        {
            ResultCollection.ItemsSource.Clear();
            if (meters == null)
            {
                return;
            }
            //for (int i = 0; i < meters.Count(); i++)
            //{
            //    string meterPk = meters.ElementAt(i).GetProperty("METER_ID") as string;
            //    ResultCollection.ItemsSource.Add(new OneMeterResult(meterPk, false));
            //}
        }
        #region 表信息
        private static InputParaViewModel parasModel = new InputParaViewModel();
        /// <summary>
        /// 表信息录入相关的数据模型
        /// </summary>
        public static InputParaViewModel ParasModel
        {
            get { return parasModel; }
            set { parasModel = value; }
        }

        //add zxg 20220327 新增数据管理的是否上传
        private bool isAllMeter = true;
        /// <summary>
        ///  是否全选表
        /// </summary>
        public bool IsAllMeter
        {
            get
            {
                return isAllMeter;
            }
            set
            {
                SetPropertyValue(value, ref isAllMeter, "IsAllMeter");
                for (int i = 0; i < Meters.Count; i++)
                {
                    Meters[i].SetProperty("IsSelected", value);
                }

                //Meters[i].SetProperty("IsSelected", IsAllMeter ? true : false);
            }
        }

        private AsyncObservableCollection<DynamicViewModel> meters = new AsyncObservableCollection<DynamicViewModel>();
        /// <summary>
        /// 表信息
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> Meters
        {
            get { return meters; }
            set { SetPropertyValue(value, ref meters, "Meters"); }
        }
        #endregion
        #region 分页控件
        /// <summary>
        /// 分页控件查找事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PagerModel_EventUpdateData(object sender, EventArgs e)
        {
            if (CanUpdate > 0)
                UpdateMeters();

            ++CanUpdate;
        }

        private DataPagerViewModel pagerModel = new DataPagerViewModel();
        /// <summary>
        /// 分页控件数据模型
        /// </summary>
        public DataPagerViewModel PagerModel
        {
            get { return pagerModel; }
            set { SetPropertyValue(value, ref pagerModel, "PagerModel"); }
        }
        #endregion
        #region 查询条件
        public void LoadFilterCollection(string fieldName, string fieldValue)
        {
            InputParaUnit paraUnitTemp = ParasModel.AllUnits.FirstOrDefault(item => item.FieldName == fieldName);
            if (paraUnitTemp != null)
            {
                for (int i = 0; i < FilterCollection.Count; i++)
                {
                    #region 更多筛选条件
                    if (FilterCollection[i].CompareExpression == EnumCompare.自定义筛选条件)
                    {
                        FilterCollection[i].SearchItemChild.FieldName = fieldName;
                        FilterCollection[i].SearchItemChild.ColumnName = paraUnitTemp.DisplayName;
                        FilterCollection[i].SearchItemChild.ValueDisplay = fieldValue;
                        FilterCollection[i].SearchItemChild.CodeType = paraUnitTemp.CodeType;
                        FilterCollection[i].SearchItemChild.ValueType = paraUnitTemp.ValueType;
                        continue;
                    }
                    #endregion
                    if (FilterCollection[i].CompareExpression == EnumCompare.清空筛选条件)
                    {
                        continue;
                    }
                    FilterCollection[i].FieldName = fieldName;
                    FilterCollection[i].ColumnName = paraUnitTemp.DisplayName;
                    FilterCollection[i].ValueDisplay = fieldValue;
                    FilterCollection[i].CodeType = paraUnitTemp.CodeType;
                    FilterCollection[i].ValueType = paraUnitTemp.ValueType;
                }
            }
        }

        private AsyncObservableCollection<SearchMenuItem> filterCollection = new AsyncObservableCollection<SearchMenuItem>();
        /// <summary>
        /// 过滤条件集合
        /// </summary>
        public AsyncObservableCollection<SearchMenuItem> FilterCollection
        {
            get { return filterCollection; }
            set { SetPropertyValue(value, ref filterCollection, "FilterCollection"); }
        }

        private List<string> searchList = new List<string>();
        /// <summary>
        /// 查询条件集合
        /// </summary>
        public List<string> SearchList { get { return searchList; } set { searchList = value; } }

        /// <summary>
        /// 查询条件列表
        /// </summary>
        public string Where
        {
            get
            {
                if (searchList.Count > 0)
                {
                    return string.Join(" and ", searchList);
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 更新表信息
        /// </summary>
        private void UpdateMeters()
        {
            TaskManager.AddCollectionQueue(() =>
            {
                ////add zxg 20220327 新增数据管理的是否上传
                IsAllMeter = false;

                Meters.Clear();
                string whereTemp = Where;
                //SearchItemModel serchItem = SearchItems.Last();
                if (searchType == 1)
                {
                    whereTemp = Where1;
                    if (SearchItems.FirstOrDefault(item => item.ColumnName.Contains("检定日期(期间)")) != null)
                    {
                        whereTemp = WhereDate; //如果是检定日期，则使用日期查询
                    }
                }


                ResultCollection.ItemsSource.Clear();
                //PagerModel.PageIndex = 21;
                List<DynamicModel> models = DALManager.MeterDbDal.GetPage("METER_INFO", "METER_ID", PagerModel.PageSize, PagerModel.PageIndex, whereTemp, true);
                for (int j = 0; j < models.Count; j++)
                {
                    OneMeterResult oneMeter = new OneMeterResult(models[j].GetProperty("METER_ID").ToString(), false);
                    ResultCollection.ItemsSource.Add(oneMeter);
                    Dictionary<string, string> resoult = GetResoult(oneMeter);
                    foreach (var key in resoult.Keys)
                    {
                        models[j].SetProperty(key, resoult[key]);
                    }
                    //MD_BAR_CODE
                    Meters.Add(new DynamicViewModel(models[j], j + 1));
                    Meters[j].SetProperty("IsSelected", false);
                    for (int i = 0; i < ParasModel.AllUnits.Count; i++)
                    {
                        if (ParasModel.AllUnits[i].ValueType == EnumValueType.编码值)
                        {
                            InputParaUnit paraUnitTemp = ParasModel.AllUnits[i];
                            Meters[j].SetProperty(paraUnitTemp.FieldName, CodeDictionary.GetNameLayer2(paraUnitTemp.CodeType, Meters[j].GetProperty(paraUnitTemp.FieldName) as string));
                        }
                    }
                }
            });
        }

        private static Dictionary<string, string> ProjectName = null;
        /// <summary>
        /// 获取一个大项目的结论，比如基本误差，启动，潜动
        /// </summary>
        public Dictionary<string, string> GetResoult(OneMeterResult meterResult)
        {
            //string id = result.GetProperty("项目号").ToString(); //这个项目号的详细的项目号，需要分割找到大类编号
            //string DID = id.Split('_')[0];
            Dictionary<string, string> ResultDictionary = new Dictionary<string, string>();

            //获取所有ID的名称
            if (ProjectName == null)
            {
                ProjectName = new Dictionary<string, string>();
                try
                {
                    FieldInfo[] f_key = typeof(UniversityMeterID).GetFields();
                    for (int i = 0; i < f_key.Length; i++)
                    {
                        string name = f_key[i].Name;
                        string ID = f_key[i].GetValue(new UniversityMeterID()).ToString();
                        if (!ProjectName.ContainsKey(ID))
                        {
                            ProjectName.Add(ID, name);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            //默认合格的
            List<string> DefalutResoult = new List<string>() { UniversityMeterID.外观检查 };
            foreach (var key in ProjectName.Keys)
            {
                if (DefalutResoult.Contains(key))
                {
                    ResultDictionary.Add(ProjectName[key], ConstHelper.合格);
                }
            }


            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                for (int j = 0; j < meterResult.Categories[i].ResultUnits.Count; j++)
                {
                    DynamicViewModel result = meterResult.Categories[i].ResultUnits[j];
                    if (result.GetProperty("项目号") == null) continue;
                    string id = result.GetProperty("项目号").ToString(); //这个项目号的详细的项目号，需要分割找到大类编号
                    string DID = id.Substring(0, 2);
                    string Name;
                    if (ProjectName.ContainsKey(DID))
                    {
                        Name = ProjectName[DID];
                    }
                    else
                    {
                        MessageDisplay.Instance.Message = $"没有找到ID为{DID}所对应的枚举名称";
                        continue;
                    }

                    if (!ResultDictionary.ContainsKey(Name)) ResultDictionary.Add(Name, ConstHelper.合格);  //是否存在

                    string objr = result.GetProperty("结论") as string;
                    if (objr == ConstHelper.不合格)
                    {
                        ResultDictionary[Name] = ConstHelper.不合格;
                    }

                }
            }


            return ResultDictionary;
        }

        public void SearchMeters()
        {
            searchType = 0;
            PagerModel.Total = DALManager.MeterDbDal.GetCount("METER_INFO", Where);
        }
        #endregion
        #region 用户选择查询条件
        private AsyncObservableCollection<SearchItemModel> searchItems = new AsyncObservableCollection<SearchItemModel>();
        /// <summary>
        /// 查询条件集合
        /// </summary>
        public AsyncObservableCollection<SearchItemModel> SearchItems
        {
            get { return searchItems; }
            set { SetPropertyValue(value, ref searchItems, "SearchItems"); }
        }
        /// <summary>
        /// 直接选择的查询条件
        /// </summary>
        private string Where1
        {
            get
            {
                List<string> whereList = new List<string>();
                for (int i = 0; i < SearchItems.Count; i++)
                {
                    if (true) //不需要选择

                    {
                        InputParaUnit paraUnitTemp = ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == SearchItems[i].ColumnName);
                        if (paraUnitTemp == null)
                        {
                            continue;
                        }
                        SearchMenuItem itemTemp = new SearchMenuItem()
                        {
                            ColumnName = SearchItems[i].ColumnName,
                            FieldName = paraUnitTemp.FieldName,
                            CodeType = paraUnitTemp.CodeType,
                            ValueType = paraUnitTemp.ValueType,
                            ValueDisplay = SearchItems[i].SearchValue,
                            CompareExpression = SearchItems[i].CompareCondition
                        };
                        whereList.Add(itemTemp.ToString());
                    }
                }
                if (whereList.Count > 0)
                {
                    return string.Join(" and ", whereList);
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 根据时间查询条件生成查询字符串
        /// </summary>
        private string WhereDate
        {
            get
            {
                InputParaUnit paraUnitTemp = ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == SearchItems.First().ColumnName);
                if (paraUnitTemp == null)
                {
                    return "";
                }
                List<string> whereList = new List<string>();
                for (int i = 0; i < SearchItems.Count; i++)
                {
                    if (SearchItems[i].ColumnName.Contains("检定日期(期间)"))
                    {
                        if (SearchItems[i].SearchStartDate != null && SearchItems[i].SearchEndDate != null)
                        {
                            string sql = $"CDate(MD_TEST_DATE) BETWEEN #{SearchItems[i].SearchStartDate:yyyy-MM-dd} 00:00:00# AND #{SearchItems[i].SearchEndDate:yyyy-MM-dd} 23:59:59#";
                            whereList.Add(sql);
                        }
                    }
                 }
                if (whereList.Count > 0)
                {
                    return string.Join(" and ", whereList);
                }
                else
                {
                    return "";
                }
            }
        }
        private int searchType = 0;
        /// <summary>
        /// 直接选择查询方法
        /// </summary>
        public void SearchMeters1()
        {
            searchType = 1;
            if (SearchItems.FirstOrDefault(item => item.ColumnName.Contains("检定日期(期间)")) == null)
            {
                PagerModel.Total = DALManager.MeterDbDal.GetCount("METER_INFO", Where1);
            }
            else
            {
                PagerModel.Total = DALManager.MeterDbDal.GetCount("METER_INFO", WhereDate);
            }
        }

        // 值的选择范围
        void ModelTemp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ColumnName")
            {
                return;
            }
            SearchItemModel modelTemp = sender as SearchItemModel;
            if (string.IsNullOrEmpty(modelTemp.ColumnName))
            {
                return;
            }
            if (modelTemp.ColumnName.Contains("检定日期(期间)"))
            {
                modelTemp.VisibilityText = System.Windows.Visibility.Collapsed;
                modelTemp.VisibilityDate = System.Windows.Visibility.Visible;
                // 通过列名获取对应的字段名称
                InputParaUnit paraUnitTemp = ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == modelTemp.ColumnName);
                if (paraUnitTemp != null)
                {
                    List<string> valuesTemp = DALManager.MeterDbDal.GetDistinct("meter_info", paraUnitTemp.FieldName, "", 500, true);
                    modelTemp.SeachValues = new AsyncObservableCollection<string>(valuesTemp);
                    if (modelTemp.SeachValues.Count() > 0)
                    {
                        modelTemp.SearchStartDate = DateTime.Parse(modelTemp.SeachValues.First());
                        modelTemp.SearchEndDate = DateTime.Parse(modelTemp.SeachValues.Last());
                    }
                }
            }
            else
            {
                modelTemp.VisibilityText = System.Windows.Visibility.Visible;
                modelTemp.VisibilityDate = System.Windows.Visibility.Collapsed;
                InputParaUnit paraUnitTemp = ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == modelTemp.ColumnName);
                if (paraUnitTemp != null)
                {
                    List<string> valuesTemp = DALManager.MeterDbDal.GetDistinct("meter_info", paraUnitTemp.FieldName, "", 500, false);
                    modelTemp.SeachValues = new AsyncObservableCollection<string>(valuesTemp);
                }
            }

        }
        #endregion
    }
}
