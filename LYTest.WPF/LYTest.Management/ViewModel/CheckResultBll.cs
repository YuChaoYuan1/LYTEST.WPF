using LYTest.DAL;
using LYTest.DAL.DataBaseView;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager
{
    public class CheckResultBll
    {
        #region 单例
        private static CheckResultBll instance = null;
        public static CheckResultBll Instance
        {
            get
            {
                if (instance == null)
                { instance = new CheckResultBll(); }
                return instance;
            }
        }
        #endregion

        #region 私有成员

        /// 获取主结论视图
        /// <summary>
        /// 获取主结论视图
        /// </summary>
        /// <param name="displayModel"></param>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, List<string>>> GetResultView(TableDisplayModel displayModel)
        {
            Dictionary<string, Dictionary<string, List<string>>> dictionaryAll = new Dictionary<string, Dictionary<string, List<string>>>();
            #region 主结论视图字典
            Dictionary<string, List<string>> pkDictionary = new Dictionary<string, List<string>>();
            if (displayModel.ColumnModelList.Count > 0)
            {
                for (int i = 0; i < displayModel.ColumnModelList.Count; i++)
                {
                    string fieldName = displayModel.ColumnModelList[i].Field;
                    string[] nameArray = displayModel.ColumnModelList[i].DisplayName.Split('|');
                    if (pkDictionary.ContainsKey(fieldName))
                    {
                        pkDictionary[fieldName].AddRange(nameArray);
                    }
                    else
                    {
                        pkDictionary.Add(fieldName, new List<string>(nameArray));
                    }
                }
                dictionaryAll.Add("", pkDictionary);
            }
            #endregion
            #region 副结论视图字典
            for (int i = 0; i < displayModel.FKDisplayModelList.Count; i++)
            {
                string fkKey = "_" + displayModel.FKDisplayModelList[i].Key;
                string fkField = displayModel.FKDisplayModelList[i].Field;
                List<string> nameList = displayModel.FKDisplayModelList[i].DisplayNames;
                if (!dictionaryAll.ContainsKey(fkKey))
                {
                    dictionaryAll.Add(fkKey, new Dictionary<string, List<string>>());
                }
                Dictionary<string, List<string>> fieldDictionary = dictionaryAll[fkKey];
                if (fieldDictionary.ContainsKey(fkField))
                {
                    fieldDictionary[fkField].AddRange(nameList);
                }
                else
                {
                    fieldDictionary.Add(fkField, nameList);
                }
            }
            #endregion

            return dictionaryAll;
        }
        #endregion

        #region 单个表的检定结论
        /// <summary>
        /// 从数据库加载一块表的所有结论
        /// </summary>
        /// <param name="isTemp">true:临时库;false:正式库</param>
        /// <param name="meterPK">单个表检定结论的唯一编号</param>
        /// <returns>未被提取的结论</returns>
        private List<DynamicModel> LoadOneModels(bool isTemp, string meterPK)
        {
            GeneralDal dal = isTemp ? DALManager.MeterTempDbDal : DALManager.MeterDbDal;
            List<string> tableNames = dal.GetTableNames();
            if (isTemp)
            {
                tableNames.Remove("T_TMP_METER_INFO");
            }
            else
            {
                tableNames.Remove("METER_INFO");
            }
            List<string> sqlList = new List<string>();
            for (int i = 0; i < tableNames.Count; i++)
            {
                sqlList.Add(string.Format("select * from {0} where METER_ID='{1}'", tableNames[i], meterPK));
            }

            return dal.GetList(tableNames, sqlList);
        }
        /// <summary>
        /// 加载单个表位单个检定项的检定结论
        /// </summary>
        /// <param name="isTemp">true:临时库;false:正式库</param>
        /// <param name="meterPK">单个表检定结论的唯一编号</param>
        /// <param name="checkKey">检定点编号</param>
        /// <param name="models">数据库中的行</param>
        /// <returns>编号对应的详细结论</returns>
        private Dictionary<string, string> LoadOneResult( string checkKey, List<DynamicModel> models)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string paraNo = checkKey.Split('_')[0];
            TableDisplayModel displayModel = ResultViewHelper.GetParaNoDisplayModel(paraNo);
            if (displayModel == null)
            {
                return dictionary;
            }
            List<string> keyList = new List<string>();
            Dictionary<string, Dictionary<string, List<string>>> dictionaryAll = GetResultView(displayModel);
            for (int i = 0; i < dictionaryAll.Count; i++)
            {
                keyList.Add(string.Format("{0}{1}", checkKey, dictionaryAll.Keys.ElementAt(i)));
            }

            for (int i = 0; i < keyList.Count; i++)
            {
                DynamicModel resultRow = models.FirstOrDefault(item => (item.GetProperty("MD_PROJECT_NO") as string) == keyList[i]);
                if (resultRow == null)
                { continue; }
                if (i == 0)
                {
                    if (dictionary.ContainsKey("项目号"))
                    {
                        dictionary["项目号"] = keyList[0];
                    }
                    else
                    {
                        dictionary.Add("项目号", keyList[0]);
                    }
                    if (dictionary.ContainsKey("项目名"))
                    {
                        dictionary["项目名"] = resultRow.GetProperty("MD_PROJECT_NAME") as string;
                    }
                    else
                    {
                        dictionary.Add("项目名", resultRow.GetProperty("MD_PROJECT_NAME") as string);
                    }
                    if (dictionary.ContainsKey("项目参数"))
                    {
                        dictionary["项目参数"] = resultRow.GetProperty("MD_PARAMETER") as string;
                    }
                    else
                    {
                        dictionary.Add("项目参数", resultRow.GetProperty("MD_PARAMETER") as string);
                    }
                }
                #region 提取结论视图
                string itemKeyTemp = resultRow.GetProperty("MD_PROJECT_NO") as string;
                int keyIndex = keyList.IndexOf(itemKeyTemp);
                if (keyIndex < 0)
                {
                    continue;
                }
                Dictionary<string, List<string>> fieldDictionary = dictionaryAll.Values.ElementAt(keyIndex);
                #endregion
                #region 解析结论
                for (int j = 0; j < fieldDictionary.Count; j++)
                {
                    string fieldName = fieldDictionary.Keys.ElementAt(j);
                    string fieldValue = resultRow.GetProperty(fieldName) as string;
                    if (string.IsNullOrEmpty(fieldValue))
                    { continue; }
                    string[] valueArray = fieldValue.Split('^');
                    //显示名称列表
                    List<string> displayNameList = fieldDictionary.Values.ElementAt(j);
                    for (int k = 0; k < displayNameList.Count; k++)
                    {
                        if (valueArray.Length > k)
                        {
                            if (dictionary.ContainsKey(displayNameList[k]))
                            {
                                dictionary[displayNameList[k]] = valueArray[k];
                            }
                            else
                            {
                                dictionary.Add(displayNameList[k], valueArray[k]);
                            }
                        }
                    }
                }
                #endregion
            }

            return dictionary;
        }
        /// <summary>
        /// 一块表的结论字典
        /// </summary>
        /// <param name="isTemp">临时库还是正式库</param>
        /// <param name="meterPk">表结论的唯一编号</param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, string>> LoadOneResult(bool isTemp, string meterPk)
        {
            Dictionary<string, Dictionary<string, string>> dictionaryAll = new Dictionary<string, Dictionary<string, string>>();
            List<DynamicModel> models = LoadOneModels(isTemp, meterPk);
            for (int i = 0; i < models.Count; i++)
            {
                string keyTemp = models[i].GetProperty("MD_PROJECT_NO") as string;
                if (IsPrimaryKey(keyTemp))
                {
                    Dictionary<string, string> dictionaryTemp = LoadOneResult(keyTemp, models);
                    if (dictionaryAll.ContainsKey(keyTemp))
                    {
                        dictionaryAll[keyTemp] = dictionaryTemp;
                    }
                    else
                    {
                        dictionaryAll.Add(keyTemp, dictionaryTemp);
                    }
                }
            }
            return dictionaryAll;
        }

        /// <summary>
        /// 检定点编号是否为主结论编号
        /// </summary>
        /// <param name="checkKey"></param>
        /// <returns></returns>
        private bool IsPrimaryKey(string checkKey)
        {
            if (string.IsNullOrEmpty(checkKey))
            {
                return false;
            }
            string[] arrayTemp = checkKey.Split('_');
            if (arrayTemp.Length == 1) return true;
            string paraNo = arrayTemp[0];
            DynamicModel model = SchemaFramework.GetParaFormat(paraNo);
            if (model == null)
            {
                return false;
            }
            else
            {
                string keyFormat = model.GetProperty("PARA_KEY_RULE") as string;

                if (string.IsNullOrEmpty(keyFormat))
                {
                    return arrayTemp.Length == 1;
                }
                else
                {
                    string[] arrayKeyRule = keyFormat.Split('|');
                    for (int i = 0; i < arrayKeyRule.Length; i++)
                    {
                        if (bool.TryParse(arrayKeyRule[i], out bool isParaKey) && isParaKey)
                        {
                            return arrayTemp.Length == 2;
                        }
                    }
                    return arrayTemp.Length == 1;
                }
            }
        }
        #endregion

    }
}
