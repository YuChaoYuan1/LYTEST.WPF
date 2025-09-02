using ICInterface.ICApiStructure;
using LYTest.Mis.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.ZHGK_XinJiang
{
    /// <summary>
    /// 标准代码
    /// </summary>
    public class StdDicCode
    {

        private Dictionary<string, Dictionary<string, string>> DicCode;
        private string SystNo;
        private List<string> CodeNameList;
        private GK_BaseApi GK_Base;

        public StdDicCode(string sysNo, GK_BaseApi gK_Base, List<string> CodeNames)
        {
            SystNo = sysNo;
            CodeNameList = CodeNames;
            GK_Base = gK_Base;
        }

        #region MyRegion
        static object InitLock = new object();
        public bool InitCode(out string Error, bool IsInit = false)
        {
            lock (InitLock)
            {
                Error = "";
                if (DicCode != null && !IsInit) return true;
                try
                {
                    DicCode = new Dictionary<string, Dictionary<string, string>>();
                    int page = 0;
                    int PageCount = 1000;
                    while (true)
                    {
                        getStdCodesCell data = new getStdCodesCell()
                        {
                            pageNo = page,
                            pageSize = PageCount,
                            sysNo = SystNo,
                            type = "02",
                            code = string.Join(",", CodeNameList),
                        };
                        var Result = GK_Base.标准代码获取接口(data, out string Msg);
                        if (Result == null)
                        {
                            DicCode = null;
                            Error = "初始化标准代码字典错误,没有收到返回值";
                            return false;
                        }
                        if (Result.resultFlag != "1")
                        {
                            DicCode = null;
                            Error = "初始化标准代码字典失败:" + Result.errorInfo;
                            return false;
                        }
                        foreach (var item in Result.stdCodeList)
                        {
                            if (!DicCode.ContainsKey(item.code))
                            {
                                DicCode.Add(item.code, new Dictionary<string, string>());
                            }
                            if (string.IsNullOrEmpty(item.codeValueName)) continue;
                            if (!DicCode[item.code].ContainsKey(item.codeValueName))
                            {
                                DicCode[item.code].Add(item.codeValueName, item.codeValue);
                            }
                        }
                        if (Result.stdCodeList.Count < PageCount)
                        {
                            return true;
                        }
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    Error = "初始化标准代码字典异常:" + ex;
                    DicCode = null;
                    return false;
                }
            }
        }
        #endregion
        /// <summary>
        /// 根据名称或者标准代码值
        /// </summary>
        /// <param name="CoreType">标准代码类型</param>
        /// <param name="Name">名称</param>
        /// <returns></returns>
        public string GetCodeValue(string CoreType, string Name)
        {
            //那个标准代码，他的名称-返回值
            try
            {
                if (!DicCode.ContainsKey(CoreType)) return "";

                if (!DicCode[CoreType].ContainsKey(Name)) return "";
                return DicCode[CoreType][Name];
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        /// 根据标准代码值获取名称
        /// </summary>
        /// <param name="CoreType">标准代码类型</param>
        /// <param name="Value">值</param>
        /// <returns></returns>
        public string GetCodeName(string CoreType, string Value)
        {
            //那个标准代码，他的名称-返回值
            try
            {
                if (!DicCode.ContainsKey(CoreType)) return "";
                var d = DicCode[CoreType];
                var value = DicCode[CoreType].FirstOrDefault(x => x.Value == Value);
                if (value.Key == null) return "";
                if (value.Key == "ABC") return "H";
                return value.Key;
            }
            catch (Exception)
            {
                return "";
            }
        }

        #region 标准代码部分
        //private readonly string mConnectdbPath = "Data Source=" + Directory.GetCurrentDirectory() + "\\Configs\\新疆\\标准码表.db";
        //private Dictionary<string, Dictionary<string, string>> 标准代码字典;
        //private void 初始化标准代码库()
        //{
        //    if (标准代码字典 != null) return;
        //    List<string> EnNames = new List<string>() { "commModuleType", "commAdaptType" };
        //    var Names = EnNames.Select(x => $"'{x}'");
        //    string sql = $"SELECT *  from MT_CODE  where 属性英文名 IN ({string.Join(",", Names)})";
        //    var 标准代码信息 = SQLiteHelper.ExecuteQuery(mConnectdbPath, sql);
        //    if (标准代码信息 != null)
        //    {
        //        标准代码字典 = new Dictionary<string, Dictionary<string, string>>();
        //        string 代码名称 = "";
        //        string 代码值 = "";
        //        string 英文名称 = "";
        //        foreach (DataRow row in 标准代码信息.Rows)
        //        {
        //            代码名称 = row["枚举值名称"] as string;
        //            代码值 = row["枚举值编码"] as string;
        //            英文名称 = row["属性英文名"] as string;

        //            if (string.IsNullOrWhiteSpace(英文名称)) continue;
        //            if (!标准代码字典.ContainsKey(英文名称))
        //            {
        //                标准代码字典.Add(英文名称, new Dictionary<string, string>());
        //            }
        //            if (标准代码字典[英文名称].ContainsKey(代码名称))
        //            {
        //                标准代码字典[英文名称].Add(代码名称, 代码值);
        //            }

        //        }

        //    }
        //}

        //private string 根据名称获取标准代码值(string 代码类型, string 名称)
        //{
        //    初始化标准代码库();
        //    //那个标准代码，他的名称-返回值
        //    try
        //    {
        //        if (!标准代码字典.ContainsKey(代码类型)) return "";
        //        if (!标准代码字典[代码类型].ContainsKey(名称)) return "";
        //        return 标准代码字典[代码类型][名称];
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }
        //}
        //private string 根据值获取标准代码名称(string 代码类型, string 值)
        //{
        //    初始化标准代码库();
        //    //那个标准代码，他的名称-返回值
        //    try
        //    {
        //        if (!标准代码字典.ContainsKey(代码类型)) return "";
        //        var value = 标准代码字典[代码类型].FirstOrDefault(x => x.Value == 值);
        //        return value.Key;
        //    }
        //    catch (Exception)
        //    {
        //        return "";
        //    }
        //}
        #endregion
    }
}
