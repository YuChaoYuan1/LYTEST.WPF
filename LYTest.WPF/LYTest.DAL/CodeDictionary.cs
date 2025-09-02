using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace LYTest.DAL
{
    /// <summary>
    /// 编码字典类
    /// </summary>
    public static class CodeDictionary
    {
        /// <summary>
        /// 数据存放的地方
        /// </summary>
        private static readonly Dictionary<string, ClassCodeLayer2> dictionaryLayer1 = new Dictionary<string, ClassCodeLayer2>();
        /// <summary>
        /// 根据codetype获取chinesename
        /// </summary>
        /// <param name="codeLayer1">英文名称关键字</param>
        /// <returns>要返回的中文名称</returns>
        public static string GetNameLayer1(string codeLayer1)
        {
            if (dictionaryLayer1.ContainsKey(codeLayer1))
            {
                return dictionaryLayer1[codeLayer1].CodeName;
            }
            return "";
        }
        /// <summary>
        /// 根据nameLayer1获取codetype
        /// </summary>
        /// <param name="nameLayer1">中文名称关键字</param>
        /// <returns>要返回的英文名称</returns>
        public static string GetCodeLayer1(string nameLayer1)
        {
            foreach (KeyValuePair<string, ClassCodeLayer2> pair in dictionaryLayer1)
            {
                if (pair.Value.CodeName == nameLayer1)
                {
                    return pair.Key;
                }
            }
            return "";
        }
        /// <summary>
        /// 根据codetype获取Code_detail表的数据字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetLayer2(string codeLayer1)
        {
            if (dictionaryLayer1.ContainsKey(codeLayer1))
            {
                return dictionaryLayer1[codeLayer1].DictionaryLayer2;
            }
            return new Dictionary<string, string>();
        }
        /// <summary>
        /// 根据1级代码找到2级名称对应的值
        /// </summary>
        /// <param name="codeLayer1">1级代码</param>
        /// <param name="nameLayer2">2级名称</param>
        /// <returns>2级参数值</returns>
        public static string GetValueLayer2(string codeLayer1, string nameLayer2)
        {
            if (string.IsNullOrEmpty(nameLayer2) || string.IsNullOrEmpty(codeLayer1))
            {
                return "";
            }
            if (dictionaryLayer1.ContainsKey(codeLayer1))
            {
                if (dictionaryLayer1[codeLayer1].DictionaryLayer2.ContainsKey(nameLayer2))
                {
                    return dictionaryLayer1[codeLayer1].DictionaryLayer2[nameLayer2];
                }
            }
            return "";
        }
        /// <summary>
        /// 根据1级代码找到2级值对应的名称
        /// </summary>
        /// <param name="codeLayer1">1级代码</param>
        /// <param name="valueLayer2">2级参数值</param>
        /// <returns>2级名称</returns>
        public static string GetNameLayer2(string codeLayer1, string valueLayer2)
        {
            if (dictionaryLayer1.ContainsKey(codeLayer1))
            {
                foreach (KeyValuePair<string, string> pair in dictionaryLayer1[codeLayer1].DictionaryLayer2)
                {
                    if (pair.Value == valueLayer2)
                    {
                        return pair.Key;
                    }
                }
            }
            return "";
        }
        private class ClassCodeLayer2
        {
            public string CodeName { get; set; }
            private Dictionary<string, string> dictionaryLayer2 = new Dictionary<string, string>();

            public Dictionary<string, string> DictionaryLayer2
            {
                get { return dictionaryLayer2; }
                set { dictionaryLayer2 = value; }
            }

        }

        public static void AddItem(string codeLayer1, string nameLayer1, Dictionary<string, string> dictionaryLayer2)
        {
            if (string.IsNullOrEmpty(codeLayer1))
            {
                return;
            }
            if (dictionaryLayer1.ContainsKey(codeLayer1))
            {
                dictionaryLayer1.Remove(codeLayer1);
            }
            dictionaryLayer1.Add(codeLayer1, new ClassCodeLayer2()
            {
                CodeName= nameLayer1,
                DictionaryLayer2=dictionaryLayer2
            });
        }
        public static List<string> GetNamesLayer1()
        {
            var listNames = from item in dictionaryLayer1.Values select item.CodeName;
            if(listNames==null)
            {
                return null;
            }
            else
            {
                return listNames.ToList();
            }
        }
        /// <summary>
        /// 加载通信协议检查标识
        /// </summary>
        public static void LoadDataFlagNames()
        {
            if(dictionaryLayer1.ContainsKey("DataFlagNames"))
            {
                dictionaryLayer1.Remove("DataFlagNames");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory()));
            ClassCodeLayer2 temp1 = new ClassCodeLayer2() { CodeName = "数据标识列表" };
            foreach (XmlNode nodeTemp in doc.DocumentElement.ChildNodes)
            {
                temp1.DictionaryLayer2.Add(nodeTemp.Attributes["DataFlagDiName"].Value, nodeTemp.Attributes["DataFlagDi"].Value);
            }
            dictionaryLayer1.Add("DataFlagNames", temp1);
        }
        /// <summary>
        /// 清空所有编码
        /// </summary>
        public static void Clear()
        {
            dictionaryLayer1.Clear();
        }
    }
}
