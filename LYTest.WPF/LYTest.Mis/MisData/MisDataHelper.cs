using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.Mis.MisData
{
    public class MisDataHelper
    {

        /// <summary>
        ///  获取启动结论
        /// </summary>
        public static string GetQiQianDongConclusion(TestMeterInfo meter, string key)
        {
            string conc = "";

            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;
            foreach (string k in meterQdQids.Keys)
            {
                if (k.IndexOf(key) == -1) continue;

                if (meterQdQids.ContainsKey(k))
                {
                    meterQdQids[k].Result = meterQdQids[k].Result.Trim();
                    if (meterQdQids[k].Result == ConstHelper.不合格)
                        conc = "02";     //如果不合格
                    else
                        conc = "01";
                }
                else
                {
                    conc = "";
                }
            }
            return conc;
        }

        /// <summary>
        ///  获取多功能结论
        /// </summary>
        public static string GetBasicConclusion(TestMeterInfo meter, string key)
        {
            Dictionary<string, MeterResult> results = meter.MeterResults;
            if (results.ContainsKey(key))
            {
                if (results[key].Result == ConstHelper.不合格)
                    return "02";     //如果不合格
                else
                    return "01";
            }
            return "03";
        }

        /// <summary>
        ///  获取多功能结论
        /// </summary>
        public static string GetDgnConclusion(TestMeterInfo meter, string itemKey)
        {
            //modify yjt 20220324 修改获取的功能是否合格
            //string conc = "";
            //if (meter.MeterDgns.ContainsKey(itemKey))
            //{
            //    conc = meter.MeterDgns[itemKey.Split('_')[0]].Result == ConstHelper.合格 ? "01" : "02";
            //}
            //return conc;

            //modify yjt 20220324 修改获取的功能是否合格
            foreach (string keyitem in meter.MeterDgns.Keys)
            {
                if (keyitem.Split('_')[0] != itemKey) continue;
                return meter.MeterDgns[keyitem].Result == ConstHelper.合格 ? "01" : "02";
            }
            return "";
        }
        /// <summary>
        ///  获取需量示值误差结论
        /// </summary>
        public static string GetDemandValueConclusion(TestMeterInfo meter )
        {
            string[] xlData = new string[19];

            string ItemKey = ProjectID.需量示值误差;

            //最大需量数据MaxIb
            string key = ItemKey + "_1";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[0] = maxdata1[3];
                xlData[1] = maxdata1[4];
                xlData[2] = maxdata1[5];

                if (meter.MeterDgns.ContainsKey(key))
                {
                    xlData[4] = meter.MeterDgns[key].Result;
                }
            }
            else
            {
                xlData[0] = "";
                xlData[1] = "";
                xlData[2] = "";
                xlData[3] = "";
                xlData[4] = "";
            }
            //最大需量数据1.0Ib
            key = ItemKey + "_2";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[5] = maxdata1[3];
                xlData[6] = maxdata1[4];
                xlData[7] = maxdata1[5];

                if (meter.MeterDgns.ContainsKey(key))
                {
                    xlData[9] = meter.MeterDgns[key].Result;
                }
            }
            else if (meter.MeterDgns.ContainsKey(ItemKey + "_5"))//10Itr
            {
                key = ItemKey + "_5";
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[5] = maxdata1[3];
                xlData[6] = maxdata1[4];
                xlData[7] = maxdata1[5];

                if (meter.MeterDgns.ContainsKey(key))
                {
                    xlData[9] = meter.MeterDgns[key].Result;
                }
            }
            else
            {
                xlData[5] = "";
                xlData[6] = "";
                xlData[7] = "";
                xlData[8] = "";
                xlData[9] = "";
            }

            //最大需量数据0.1Ib
            key = ItemKey + "_3";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[10] = maxdata1[3];
                xlData[11] = maxdata1[4];
                xlData[12] = maxdata1[5];

                if (meter.MeterDgns.ContainsKey(key))
                {
                    xlData[14] = meter.MeterDgns[key].Result;
                }
            }
            else if (meter.MeterDgns.ContainsKey(ItemKey + "_4"))//Itr
            {
                key = ItemKey + "_4";
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[10] = maxdata1[3];
                xlData[11] = maxdata1[4];
                xlData[12] = maxdata1[5];

                if (meter.MeterDgns.ContainsKey(key))
                {
                    xlData[14] = meter.MeterDgns[key].Result;
                }
            }
            else
            {
                xlData[10] = "";
                xlData[11] = "";
                xlData[12] = "";
                xlData[13] = "";
                xlData[14] = "";
            }

            string[] results = new string[3];
            if ((xlData[0] != null && xlData[0] != "") || (xlData[1] != null && xlData[1] != ""))
            {
                results[0] = xlData[4] == ConstHelper.合格 ? "01" : "02";
            }
            if ((xlData[5] != null && xlData[5] != "") || (xlData[6] != null && xlData[6] != ""))
            {
                results[1] = xlData[9] == ConstHelper.合格 ? "01" : "02";
            }
            if ((xlData[10] != null && xlData[10] != "") || (xlData[11] != null && xlData[11] != ""))
            {
                results[2] = xlData[14] == ConstHelper.合格 ? "01" : "02";
            }

            if (results.Contains("02")) return "02";
            else if (results.Contains("01")) return "01";

            return "";
        }

        //add yjt 20220314 新增显示功能判断合格
        /// <summary>
        /// 获取智能表功能试验 显示功能
        /// </summary>
        public static string GetFunctionShowConclusion(TestMeterInfo meter, string key)
        {
            foreach (string keyitem in meter.MeterShows.Keys)
            {
                if (keyitem.Split('_')[0] != key) continue;
                return meter.MeterShows[keyitem].Result == ConstHelper.合格 ? "01" : "02";
            }
            return "";
        }

        ///// <summary>
        /////  获取多功能结论
        ///// </summary>
        //public static string GetCarrierConclusion(TestMeterInfo meter)
        //{
        //    string conc = "";

        //    foreach (KeyValuePair<string, MeterCarrierData> cd in meter.MeterCarrierDatas)
        //    {
        //        conc = cd.Value.Result == ConstHelper.合格 ? "01" : "02";
        //        if (conc == "02") break;
        //    }

        //    return conc;
        //}

        /// <summary>
        /// 获取费控试验
        /// </summary>
        public static string GetFkValue(TestMeterInfo meter, string key)
        {
            if (meter.MeterCostControls.ContainsKey(key))
            {
                return meter.MeterCostControls[key].Result;
            }

            return "";
        }

        /// <summary>
        /// 获取费控试验
        /// </summary>
        public static string GetFkConclusion(TestMeterInfo meter, string key)
        {
            string conc = "";

            if (meter.MeterCostControls.ContainsKey(key))
            {
                conc = meter.MeterCostControls[key].Result.Trim() == ConstHelper.合格 ? "01" : "02";
                if (meter.MeterCostControls[key].Result == ConstHelper.合格 || meter.MeterCostControls[key].Result == "公钥合格" || meter.MeterCostControls[key].Result == "私钥合格")
                    conc = "01";
            }

            return conc;
        }

        //    /// <summary>
        //    /// 获取智能表功能试验
        //    /// </summary>
        //    /// <param name="meter"></param>
        //    /// <param name="costItem"></param>
        //    /// <returns></returns>
        //    public static string GetFunctionConclusion(TestMeterInfo meter, string key)
        //    {
        //        if (meter.MeterFunctions.ContainsKey(key))
        //        {
        //            return meter.MeterFunctions[key].Result == ConstHelper.合格 ? "01" : "02";
        //        }

        //        return "";
        //    }
        //    /// <summary>
        //    /// 获取智能表功能试验
        //    /// </summary>
        //    public static string GetFunctionShowConclusion(TestMeterInfo meter, string key)
        //    {
        //        if (meter.MeterShows.ContainsKey(key))
        //        {
        //            return meter.MeterShows[key].Result == ConstHelper.合格 ? "01" : "02";
        //        }

        //        return "";
        //    }
        /// <summary>
        /// 获取误差一致性试验结论
        /// </summary>
        public static string GetErrorRecordConclusion(TestMeterInfo meter, string key)
        {
            string innerkey = "";
            if (ProjectID.误差一致性 == key)
            {
                innerkey = "1";
            }
            else if (ProjectID.误差变差 == key)
            {
                innerkey = "2";
            }
            else if (ProjectID.负载电流升将变差 == key)
            {
                innerkey = "3";
            }

            if (meter.MeterErrAccords.ContainsKey(innerkey))
            {
                if (meter.MeterErrAccords[innerkey].PointList == null || meter.MeterErrAccords[innerkey].PointList.Count <= 0)
                    return "";

                if (meter.MeterErrAccords[innerkey].Result == ConstHelper.合格)
                    return "01";
                else if (meter.MeterErrAccords[innerkey].Result == ConstHelper.不合格)
                    return "02";
            }

            return "";
        }

        //    /// <summary>
        //    /// 获取功耗试验结论
        //    /// </summary>
        //    public static string GetPowerConsumeConclusion(TestMeterInfo meter)
        //    {
        //        string conc = "";
        //        string powerConsumeKey = string.Format("{0}{1}", ProjectID.功耗试验, "11");
        //        if (meter.MeterPowers.ContainsKey(powerConsumeKey))
        //        {
        //            conc = meter.MeterPowers[powerConsumeKey].Value == ConstHelper.合格 ? "01" : "02";
        //        }
        //        return conc;
        //    }

        //    /// <summary>
        //    /// 耐压试验结论
        //    /// </summary>
        //    public static string GetInsulationConclusion(TestMeterInfo meter)
        //    {
        //        string conc = "";
        //        if (meter.MeterInsulations.Count > 0)
        //        {
        //            foreach (MeterInsulation meterResult in meter.MeterInsulations.Values)
        //            {
        //                if (meterResult.Result != ConstHelper.合格)
        //                    conc = "02";
        //            }
        //            if (conc == "03")
        //                conc = "01";
        //        }
        //        return conc;
        //    }
    }
}
