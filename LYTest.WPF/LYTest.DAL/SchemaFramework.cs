using System.Collections.Generic;
using System.Linq;

namespace LYTest.DAL
{
    /// <summary>
    /// 方案的框架,由于该类要经常加载,所有做成静态的
    /// </summary>
    public static class SchemaFramework
    {
        private static Dictionary<string, string> itemDictionary = new Dictionary<string, string>();
        static SchemaFramework()
        {
            paraModels = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString());
        }
        public static void AddNewPair(string paraNo, string paraName)
        {
            if (!string.IsNullOrEmpty(paraNo))
            {
                if (!itemDictionary.ContainsKey(paraNo))
                {
                    itemDictionary.Add(paraNo, paraName);
                }
            }
        }
        /// 获取检定点名称
        /// <summary>
        /// 获取检定点名称
        /// </summary>
        /// <param name="itemNo"></param>
        /// <returns></returns>
        public static string GetItemName(string itemNo)
        {
            if (itemDictionary.ContainsKey(itemNo))
            { return itemDictionary[itemNo]; }
            else
            { return ""; }
        }

        private static List<DynamicModel> paraModels = new List<DynamicModel>();
        /// <summary>
        /// 获取检定点参数格式信息
        /// </summary>
        /// <param name="paraNo"></param>
        /// <returns></returns>
        public static DynamicModel GetParaFormat(string paraNo)
        {
            return paraModels.FirstOrDefault(item => (item.GetProperty("PARA_NO") as string) == paraNo);
        }
        //TODO:getSortNo()
        public static string GetSortNo(string paraNo)
        {
            DynamicModel dmTmp = GetParaFormat(paraNo);
            if (dmTmp == null)
            {
                return "999";
            }
            return dmTmp.GetProperty("DEFAULT_SORT_NO") as string;
        }
    }
}
