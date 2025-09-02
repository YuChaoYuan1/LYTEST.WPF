using System.Collections.Generic;
using System.Linq;

namespace LYTest.DAL.DataBaseView
{
    /// <summary>
    /// 表显示帮助类
    /// </summary>
    public class ResultViewHelper
    {
        private static Dictionary<string, TableDisplayModel> dictionaryView = new Dictionary<string, TableDisplayModel>();
        private static Dictionary<string, string> dictionaryParaNoView = new Dictionary<string, string>();

        private static List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString());
        static ResultViewHelper()
        {
            for (int i = 0; i < models.Count; i++)
            {
                string key = models[i].GetProperty("PARA_NO") as string;
                string viewString = models[i].GetProperty("RESULT_VIEW_ID") as string;
                if (!string.IsNullOrEmpty(key) && !dictionaryParaNoView.ContainsKey(key))
                {
                    dictionaryParaNoView.Add(key, viewString);
                }
            }
        }
        /// <summary>
        /// 空方法,用来初始化
        /// </summary>
        public static void Initialize()
        {
            #region 加载所有视图
            Dictionary<string, string> viewIdDictionary = CodeDictionary.GetLayer2("MeterResultViewId");
            foreach (string viewId in viewIdDictionary.Values)
            {
                GetTableDisplayModel(viewId, true);
            }
            #endregion
        }
        /// <summary>
        /// 根据显示ID获取表的显示模型
        /// </summary>
        /// <param name="viewID">显示ID</param>
        /// <returns>表的显示模型</returns>
        public static TableDisplayModel GetTableDisplayModel(string viewID, bool reloadFlag = false)
        {
            #region 判断是否有当前视图
            if (string.IsNullOrEmpty(viewID))
            {
                return null;
            }
            if (dictionaryView.ContainsKey(viewID))
            {
                if (!reloadFlag)
                {
                    return dictionaryView[viewID];
                }
                else
                {
                    dictionaryView.Remove(viewID);
                }
            }
            else if (!reloadFlag)
            {
                return null;
            }
            #endregion

            #region 加载数据显示视图
            TableDisplayModel tableDisplayModel = new TableDisplayModel();
            tableDisplayModel.ViewID = viewID;
            DynamicModel viewModel = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_VIEW_CONFIG.ToString(), $"AVR_VIEW_ID='{viewID}'");
            if (viewModel != null)
            {
                tableDisplayModel.TableName = viewModel.GetProperty("AVR_TABLE_NAME") as string;
                if (viewModel.GetProperty("AVR_COL_NAME") != null && viewModel.GetProperty("AVR_COL_SHOW_NAME") != null)
                {
                    string[] fields = viewModel.GetProperty("AVR_COL_NAME").ToString().Split(',');
                    string[] keysDisplayArray = viewModel.GetProperty("AVR_COL_SHOW_NAME").ToString().Split('#');
                    string stringParaNos = viewModel.GetProperty("PARA_NO") as string;
                    if (!string.IsNullOrEmpty(stringParaNos))
                    {
                        tableDisplayModel.ParaNoList = stringParaNos.Split(',').ToList();
                    }
                    string pkDisplayString = keysDisplayArray[0];
                    #region 加载主结论列表
                    if (pkDisplayString.Trim() != string.Empty)
                    {
                        string[] displayNames = pkDisplayString.Split(',');
                        for (int i = 0; i < fields.Length; i++)
                        {
                            ColumnDisplayModel displayModel = new ColumnDisplayModel();
                            displayModel.IsSelected = true;
                            displayModel.Field = fields[i].Trim();
                            if (i < displayNames.Length)
                            {
                                displayModel.DisplayName = displayNames[i].Replace(" ", "");
                            }
                            tableDisplayModel.ColumnModelList.Add(displayModel);
                        }
                    }
                    #endregion
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            if (dictionaryView.ContainsKey(viewID))
            {
                dictionaryView[viewID] = tableDisplayModel;
            }
            else
            {
                dictionaryView.Add(viewID, tableDisplayModel);
            }
            #endregion
            return tableDisplayModel;
        }
        /// <summary>
        /// 加载副结论项检定模型
        /// </summary>
        /// <param name="fkDisplayString"></param>
        /// <returns></returns>
        private static FKDisplayConfigModel GetFKModel(string fkDisplayString)
        {
            string[] modelArray = fkDisplayString.Split(',');
            if (modelArray.Length == 3)
            {
                FKDisplayConfigModel fkModel = new FKDisplayConfigModel();
                fkModel.Key = modelArray[0];
                fkModel.Field = modelArray[1];
                fkModel.DisplayNames = modelArray[2].Split('|').ToList();
                return fkModel;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 存储表的显示模型到数据库
        /// </summary>
        /// <param name="tableDisplayModel"></param>
        public static void SaveTableDisplayModel(TableDisplayModel tableDisplayModel)
        {
            DynamicModel viewModel = new DynamicModel();
            viewModel.SetProperty("PK_VIEW_NO", tableDisplayModel.ViewID);
            viewModel.SetProperty("PARA_NO", string.Join(",", tableDisplayModel.ParaNoList));
            viewModel.SetProperty("AVR_CHECK_NAME", tableDisplayModel.ViewID);
            viewModel.SetProperty("AVR_TABLE_NAME", tableDisplayModel.TableName);
            string fieldsString = string.Empty;
            string displayString = string.Empty;
            //主结论项显示名称
            for (int i = 0; i < tableDisplayModel.ColumnModelList.Count; i++)
            {
                if (tableDisplayModel.ColumnModelList[i].IsSelected)
                {
                    fieldsString += string.Format("{0},", tableDisplayModel.ColumnModelList[i].Field);
                    displayString += string.Format("{0},", tableDisplayModel.ColumnModelList[i].DisplayName);
                }
            }
            displayString = displayString.TrimEnd(',');
            //加载副检定项显示
            if (tableDisplayModel.FKDisplayModelList != null && tableDisplayModel.FKDisplayModelList.Count > 0)
            {
                for (int i = 0; i < tableDisplayModel.FKDisplayModelList.Count; i++)
                {
                    displayString += string.Format("#{0}", tableDisplayModel.FKDisplayModelList[i]);
                }
            }
            fieldsString = fieldsString.TrimEnd(',');
            viewModel.SetProperty("AVR_COL_NAME", fieldsString);
            viewModel.SetProperty("AVR_COL_SHOW_NAME", displayString);
            string where = string.Format("PK_VIEW_NO = {0}", tableDisplayModel.ViewID);
            if (DALManager.ApplicationDbDal.GetCount("DSPTCH_DIC_VIEW", where) > 0)
            {
                DALManager.ApplicationDbDal.Update("DSPTCH_DIC_VIEW", where, viewModel, new List<string> { "AVR_TABLE_NAME", "AVR_COL_NAME", "AVR_COL_SHOW_NAME" });
            }
            else
            {
                DALManager.ApplicationDbDal.Insert("DSPTCH_DIC_VIEW", viewModel);
            }
        }
        /// <summary>
        /// 获取主结论显示
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPkDisplayDictionary(string viewId)
        {
            TableDisplayModel model = GetTableDisplayModel(viewId);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (model != null)
            {
                if (dictionaryView.ContainsKey(viewId))
                {
                    for (int i = 0; i < model.ColumnModelList.Count; i++)
                    {
                        ColumnDisplayModel columnModel = model.ColumnModelList[i];
                        dictionary.Add(columnModel.Field, columnModel.DisplayName);
                    }
                }
            }
            return dictionary;
        }

        public static string GetParaNoView(string paraNo, bool reload = false)
        {
            if (reload)
            {
                if (dictionaryParaNoView.ContainsKey(paraNo))
                {
                    DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), string.Format("PARA_NO = '{0}'", paraNo));
                    if (model != null)
                    {
                        return model.GetProperty("RESULT_VIEW_ID") as string;
                    }
                }
            }
            if (dictionaryParaNoView.ContainsKey(paraNo))
            {
                return dictionaryParaNoView[paraNo];
            }
            else
            {
                //#region 将检定点参数配置加入到数据库
                //string paraName = SchemaFramework.GetItemName(paraNo);
                //DynamicModel temp = new DynamicModel();
                //temp.SetProperty("PARA_NO", paraNo);
                //temp.SetProperty("PARA_NAME", paraName);
                //DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), temp);
                //#endregion
                return "";
            }
        }
        /// <summary>
        /// 根据检定点编号获取显示视图
        /// </summary>
        /// <param name="paraNo"></param>
        /// <returns></returns>
        public static TableDisplayModel GetParaNoDisplayModel(string paraNo)
        {
            string viewID = GetParaNoView(paraNo);
            return GetTableDisplayModel(viewID);
        }
    }
}