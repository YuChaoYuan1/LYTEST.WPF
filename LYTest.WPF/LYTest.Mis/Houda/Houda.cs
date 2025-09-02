using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.DAL.Config;
using LYTest.Mis.Common;
using LYTest.Mis.MDS.Table;
using LYTest.Mis.MisData;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace LYTest.Mis.Houda
{
    public class Houda : OracleHelper, IMis
    {
        /// <summary>
        /// 任务单号
        /// </summary>
        private readonly string TaskNO = "";
        /// <summary>
        /// 成功上传数量
        /// </summary>
        private int SusessCount = 0;//
        public static Dictionary<string, Dictionary<string, string>> PCodeTable = new Dictionary<string, Dictionary<string, string>>();
        public Houda(string ip, int port, string dataSource, string userId, string pwd, string url)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;
        }
        public bool Down(string MD_BarCode, ref TestMeterInfo meter)
        {
            if (string.IsNullOrEmpty(MD_BarCode)) return false;

            if (PCodeTable.Count <= 0)
                GetDicPCodeTable();
            string sql = string.Format(@"SELECT * FROM mt_detect_out_equip t1 INNER JOIN mt_meter t2 ON t1.bar_code=t2.bar_code WHERE t2.bar_code='{0}' ORDER BY t1.write_date DESC", MD_BarCode.Trim());
            DataTable dt = ExecuteReader(sql);
            if (dt.Rows.Count <= 0) return false;
            DataRow row = dt.Rows[0];
            meter.Meter_ID = row["METER_ID"].ToString().Trim();
            meter.MD_BarCode = row["BAR_CODE"].ToString().Trim();              //条形码
            meter.MD_AssetNo = row["ASSET_NO"].ToString().Trim();              //申请编号 --资产编号
            meter.MD_MadeNo = row["MADE_NO"].ToString().Trim();                //出厂编号
            meter.MD_TaskNo = row["DETECT_TASK_NO"].ToString().Trim();         //检定任务单，申请编号

            //meter.Type = GetPName("meterTypeCode", row["TYPE_CODE"]);
            return true;
        }

        //TODO下载方案
        public bool SchemeDown(string MD_BarCode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }

        //TODO上传检定记录

        public bool Update(TestMeterInfo meterInfo)
        {

            MT_METER meter = GetMt_meter(meterInfo.MD_BarCode);

            if (meter == null)
            {
                LogManager.AddMessage("没有查查到表信息", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                return false;
            }
            string task_no = meter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO;
            meterInfo.Other5 = task_no;
            meterInfo.MD_BarCode = meterInfo.MD_BarCode.Trim();

            if (PCodeTable.Count <= 0)
                GetDicPCodeTable();

            //GetInsulationConclusionFromHouda(meterInfo,task_no);
            //sqlList.Add("delete from bjxm3_result.MT_CONST_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ");

            ////常数--走字
            //string[] strSql2 = GetMT_CONST_MET_CONCByMt(meter, meterInfo);
            //if (strSql2 != null)
            //{
            //    foreach (string strQuest in strSql2)
            //    {
            //        sqlList.Add(strQuest);
            //    }
            //}
            //Execute(sqlList);
            List<string> sqlList = new List<string>
            {
                //基本误差
                "delete from bjxm3_result.MT_BASICERR_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //启动试验
                "delete from bjxm3_result.MT_STARTING_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //潜动试验
                "delete from bjxm3_result.MT_CREEPING_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //电能表常数试验
                "delete from bjxm3_result.MT_CONST_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //日计时试验
                "delete from bjxm3_result.MT_DAYERR_MET_CONC  where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //变差要求试验 表A.11　误差变差试验
                "delete from bjxm3_result.MT_ERROR_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //误差一致性试验
                "delete from bjxm3_result.MT_CONSIST_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //负载电流升降变差试验 
                "delete from bjxm3_result.MT_VARIATION_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //安全认证试验 --身份认证
                "delete from bjxm3_result.MT_ESAM_SECURITY_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //密钥下装-密钥更新
                "delete from bjxm3_result.MT_ESAM_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //规约一致性
                "delete from bjxm3_result.MT_STANDARD_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                ////预置参数检查
                "delete from bjxm3_result.MT_PRESETPARAM_CHECK_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //预置参数设置
                //sqlList.Add("delete from bjxm3_result.MT_PRESETPARAM_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ");
                // sqlList.Add("delete from bjxm3_result.MT_PASSWORD_CHANGE_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ");
                //需量示值误差
                "delete from bjxm3_result.MT_DEMANDVALUE_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //费控试验
                //sqlList.Add("delete from bjxm3_result.MT_ESAM_READ_MET_CONC where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ");
                //检定 校准 综合结论
                "delete from bjxm3_result.MT_DETECT_MET_RSLT where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ",
                //外观检查
                // sqlList.Add("delete from bjxm3_result.MT_INTUIT_MET_CONC where detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' ");

                //不合格原因
                "delete from bjxm3_result.MT_EQUIP_UNPASS_REASON where  detect_task_no = '" + task_no + "' and bar_code='" + meterInfo.MD_BarCode + "' "
            };

            try
            {
                //外观检查试验
                sqlList.Add(GetMT_INTUIT_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("外观检查试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            string[] strSql;
            try
            {

                //启动
                strSql = GetMT_STARTING_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }

                //潜动
                strSql = GetMT_CREEPING_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("起动潜动插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            ////预置参数结论（抄读项）
            //strSql = GetMT_PRESETPARAM_CHECK_MET_CONCByMtFK(meter, meterInfo);
            //if (strSql != null)
            //{
            //    foreach (string strQuest in strSql)
            //    {
            //        sqlList.Add(strQuest);
            //    }
            //}

            try
            {
                //基本误差试验
                strSql = GetMT_BASICERR_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("基本误差试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //常数--走字
                strSql = GetMT_CONST_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {

                LogManager.AddMessage("常数试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //日计时
                sqlList.Add(GetMT_DAYERR_MET_CONCByMt(meter, meterInfo));

                //密钥更新
                sqlList.Add(GetMT_ESAM_MET_CONCByMt(meter, meterInfo));

                //身份认证
                sqlList.Add(GetMT_ESAM_SECURITY_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {

                LogManager.AddMessage("试验插入出现问题22" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //规约一致性
                strSql = GetMT_STANDARD_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("规约一致性检查出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            try
            {

                //需量示值误差
                strSql = GetMT_DEMANDVALUE_MET_CONCMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //一致性误差
                strSql = GetMT_CONSIST_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //变差要求试验
                strSql = GetMT_ERROR_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //负载电流升降变差
                strSql = GetMT_VARIATION_MET_CONCByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("最大需量或误差一致性检查出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            try
            {
                //参数预设检查
                string[] array = GetMT_PRESETPARAM_CHECK_MET_CONCByMt(meter, meterInfo);
                if (array != null)
                {
                    foreach (string value in array)
                    {
                        sqlList.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("参数预设检查出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            //string[] array = GetMT_INTUIT_MET_CONCByMt(meter, meterInfo);
            //if (array != null)
            //{
            //    foreach (string value in array)
            //    {
            //        sqlList.Add(value);
            //    }
            //}

            //费控试验
            //strSql = GetMT_ESAM_READ_MET_CONCMt(meter, meterInfo);
            //if (strSql != null)
            //{
            //    foreach (string strQuest in strSql)
            //    {
            //        sqlList.Add(strQuest);
            //    }
            //}
            //补充规约一致性(本地部分已经好了，只需要上传部分)，
            //费控试验，报警，保电等都需要，具体看文档
            //密钥更新需要确认一下上传的数据有没有问题
            //潜动电流不对
            //





            try
            {
                //总结论
                strSql = GetMT_DETECT_RSLTByMt(meter, meterInfo);
                if (strSql != null)
                {
                    foreach (string strQuest in strSql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("总结论插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            //  sqlList.Add(GetMT_DETECT_RSLTByMt(meter, meterInfo));

            //if (meterInfo.Result != "合格")  //不合格上传不合格原因
            //{

            //}


            try
            {
                sqlList = RemoveNull(sqlList);
                Execute(sqlList);
            }
            catch (Exception ex)
            {

                LogManager.AddMessage("错误：" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                return false;
            }

            return true;
        }

        private List<string> RemoveNull(List<string> sqlList)
        {

            List<string> str = new List<string>();
            for (int i = 0; i < sqlList.Count; i++)
            {
                if (sqlList[i] != null && sqlList[i] != "")
                {
                    str.Add(sqlList[i]);
                }
            }
            return str;
        }
        public bool UpdateCompleted()
        {

            string sys_no = "450";

            string xml = "<PARA>";
            xml += "<SYS_NO>" + sys_no + "</SYS_NO>";
            xml += "<DETECT_TASK_NO>" + TaskNO + "</DETECT_TASK_NO>";
            xml += "</PARA>";
            string[] args = new string[1];
            args[0] = xml;
            object result = WebServiceHelper.InvokeWebService(WebServiceURL, "getDETedTestData", args);
            if (result.ToString().ToUpper() == "FALSE")
                result = WebServiceHelper.InvokeWebService(WebServiceURL, "getDETedTestData", args);


            if (!WebServiceHelper.GetResultByXml(result.ToString()))
            {
                MessageBox.Show("数据上传失败，调用分项结论接口错误信息：" + WebServiceHelper.GetMessageByXml(result.ToString()));
                return false;
            }

            xml = "<PARA>";
            xml += "<SYS_NO>" + sys_no + "</SYS_NO>";
            xml += "<DETECT_TASK_NO>" + TaskNO + "</DETECT_TASK_NO>";
            xml += "<VALID_QTY>" + SusessCount + "</VALID_QTY>";
            xml += "</PARA>";
            args[0] = xml;
            result = WebServiceHelper.InvokeWebService(WebServiceURL, "setResults", args);
            if (result.ToString().ToUpper() == "FALSE")
                result = WebServiceHelper.InvokeWebService(WebServiceURL, "setResults", args);

            if (!WebServiceHelper.GetResultByXml(result.ToString()))
            {
                MessageBox.Show("数据上传失败，调用综合结论接口错误信息：" + WebServiceHelper.GetMessageByXml(result.ToString()));
                return false;
            }
            return true;
        }

        public void UpdateInit()
        {
            SusessCount = 0;

        }


        #region 检定项目数据
        ///// <summary>
        ///// 不合格原因
        ///// </summary>
        //private string[] GetMT_EQUIP_UNPASS_REASON(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    List<string> sql = new List<string>();


        //    return sql.ToArray();
        //}

        /// <summary>
        /// 外观检查试验数据上传
        /// </summary>
        protected string GetMT_INTUIT_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            MT_INTUIT_MET_CONC entity = new MT_INTUIT_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = meter.VerifyDate,
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                DETECT_CONTENT = "01",
                CONC_CODE = "01",
                //CONC_CODE = meter.MeterResults[ProjectID.外观检查试验].Result == Const .合格 ? "01" : "02",
                WRITE_DATE = DateTime.Now.ToString(),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",
            };
            DataTable dataSet = GetInsulationConclusionFromHouda2(meter, mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO);
            if (dataSet.Rows.Count > 0)
            {
                entity.DETECT_CONTENT = dataSet.Rows[0]["DETECT_CONTENT"].ToString().Trim();
                entity.CONC_CODE = dataSet.Rows[0]["CONC_CODE"].ToString().Trim();
                entity.DETECT_DATE = dataSet.Rows[0]["DETECT_DATE"].ToString().Trim();
                entity.IS_VALID = dataSet.Rows[0]["IS_VALID"].ToString().Trim();
            }

            return entity.ToInsertString();
        }

        protected string GetPCode(string type, string name)
        {
            string code = "";
            Dictionary<string, string> dic = PCodeTable[type];
            foreach (string k in dic.Keys)
            {
                if (dic[k] == name)
                {
                    code = k;
                    break;
                }
            }
            return code;
        }

        /// <summary>
        /// 基本误差数据
        /// </summary>
        private string[] GetMT_BASICERR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            string[] keys = new string[meter.MeterErrors.Keys.Count];
            meter.MeterErrors.Keys.CopyTo(keys, 0);
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                MeterError meterErr = meter.MeterErrors[key];

                string[] wc = meterErr.WCData.Split('|');
                if (wc.Length < 2) continue;


                MT_BASICERR_MET_CONC entity = new MT_BASICERR_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = "1",
                    DETECT_ITEM_POINT = (i + 1).ToString(),
                    IS_VALID = "1",
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = ""
                };
                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meterErr.GLFX);
                string strYj;
                switch (meterErr.YJ)
                {
                    case "A":
                        strYj = "A元";
                        break;
                    case "B":
                        strYj = "B元";
                        break;
                    case "C":
                        strYj = "C元";
                        break;
                    case "H":
                        strYj = "合元";
                        break;
                    default:
                        strYj = "合元";
                        break;
                }
                string abc;
                if (strYj == "合元")
                {
                    if (meter.MD_WiringMode == "三相三线")
                        abc = "AC";
                    else
                        abc = "ABC";
                }
                else
                    abc = strYj.Trim('元');

                entity.IABC = GetPCode("currentPhaseCode", abc);
                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");
                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", meterErr.IbX);
                entity.FREQ = GetPCode("meterFreq", "50");
                entity.PF = GetPCode("meterTestPowerFactor", meterErr.GLYS.Trim());

                entity.DETECT_CIRCLE = meterErr.Circle;// "2";          //Circle
                entity.SIMPLING = "2";


                entity.ERROR = meterErr.WCData;
                entity.AVE_ERR = meterErr.WCValue;    //平均值
                entity.INT_CONVERT_ERR = meterErr.WCHZ;  //化整值
                try
                {
                    if (meterErr.WCData == null || meterErr.WCData.Replace("|", "").Trim() == "")
                    {
                        entity.ERROR = "999.99|999.99";
                        entity.AVE_ERR = "999.99";  //平均值
                        entity.INT_CONVERT_ERR = "999.99";  //化整值
                    }
                    if (entity.AVE_ERR == null || entity.AVE_ERR.Trim() == "")
                    {
                        entity.AVE_ERR = "999.99";  //平均值
                        entity.INT_CONVERT_ERR = "999.99";  //化整值
                    }
                }
                catch (Exception)
                {
                }



                if (meterErr.Limit != null)
                {
                    entity.ERR_UP = meterErr.Limit.Trim().Split('|')[0];   //误差上限
                    entity.ERR_DOWN = meterErr.Limit.Trim().Split('|')[1];
                }

                entity.CONC_CODE = meterErr.Result == ConstHelper.合格 ? "01" : "02";
                entity.WRITE_DATE = DateTime.Now.ToString();
                entity.HANDLE_FLAG = "0";
                if (entity.AVE_ERR.Length > 8)
                {
                    entity.AVE_ERR = entity.AVE_ERR.Substring(0, 8);
                }
                sql.Add(entity.ToInsertString());
            }
            return sql.ToArray(); ;
        }
        /// <summary>
        /// 启动
        /// </summary>
        private string[] GetMT_STARTING_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;
            List<string> sql = new List<string>();
            int index = 0;
            foreach (string key in meterQdQids.Keys)
            {
                string current = Convert.ToSingle(meterQdQids[key].Current) + "Ib";

                if (key.Split('_')[0] != "12002") continue;    // 排除潜动的

                MT_STARTING_MET_CONC entity = new MT_STARTING_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = (index + 1).ToString(),
                    DETECT_ITEM_POINT = (index + 1).ToString(),
                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    CONC_CODE = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",

                    IS_VALID = "1",
                    LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//潜动默认115%
                    PULES = "1",
                    BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meterQdQids[key].PowerWay),
                    //LOAD_CURRENT = g_DicPCodeTable["meter_Test_CurLoad",current),
                    LOAD_CURRENT = meterQdQids[key].Current.ToString(),

                    START_CURRENT = Convert.ToSingle(meterQdQids[key].Current).ToString("F5"),
                };
                // entity.WRITE_DATE = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                //entity.DETECT_DATE = meter.VerifyDate.ToString("yyyy/MM/dd HH:mm");

                if (!string.IsNullOrEmpty(meterQdQids[key].StandartTime))
                    entity.TEST_TIME = (Convert.ToSingle(meterQdQids[key].StandartTime) * 60).ToString("F0");
                if (!string.IsNullOrEmpty(meterQdQids[key].ActiveTime))
                    entity.REAL_TEST_TIME = (Convert.ToSingle(meterQdQids[key].ActiveTime) * 60).ToString("F0");

                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", current);
                sql.Add(entity.ToInsertString());
                index++;
            }
            return sql.ToArray();
        }

        /// <summary>
        /// 潜动
        /// </summary>
        private string[] GetMT_CREEPING_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            int index = 0;
            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;

            List<string> sql = new List<string>();
            foreach (string key in meterQdQids.Keys)
            {
                if (key.Split('_')[0] != "12003") continue;    // 排除启动的
                MT_CREEPING_MET_CONC entity = new MT_CREEPING_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = index.ToString(),
                    DETECT_ITEM_POINT = index.ToString(),
                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    CONC_CODE = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",
                    IS_VALID = "1",
                    LOAD_VOLTAGE = GetPCode("meterTestVolt", "115%Un"),//潜动默认115%
                    PULES = "0",
                    BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meter.MeterQdQids[key].PowerWay),
                    LOAD_CURRENT = GetPCode("meterTestCurLoad", "0"),
                    TEST_TIME = (Convert.ToSingle(meterQdQids[key].ActiveTime) * 60).ToString("F0"),
                    REAL_TEST_TIME = (Convert.ToSingle(meterQdQids[key].StandartTime) * 60).ToString("F0"),
                };
                if (!string.IsNullOrEmpty(meterQdQids[key].ActiveTime))
                    entity.TEST_TIME = (Convert.ToSingle(meterQdQids[key].ActiveTime) * 60).ToString("F0");
                if (!string.IsNullOrEmpty(meterQdQids[key].StandartTime))
                    entity.REAL_TEST_TIME = (Convert.ToSingle(meterQdQids[key].StandartTime) * 60).ToString("F0");
                //entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", meterQdQids[key].Current);
                sql.Add(entity.ToInsertString());
                index++;
            }
            return sql.ToArray();
        }

        /// <summary>
        /// 校核常数(走字)
        /// </summary>
        private string[] GetMT_CONST_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            int index = 0;
            foreach (string key in meter.MeterZZErrors.Keys)
            {
                MeterZZError meterError = meter.MeterZZErrors[key];
                string current = GetPCode("meterTestCurLoad", meterError.IbX);
                string[] levs = Core.Function.Number.GetDj(meter.MD_Grane);


                string str = meterError.WarkPower.ToString().Trim();
                MT_CONST_MET_CONC entity = new MT_CONST_MET_CONC
                {
                    //DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    ////EQUIP_CATEG = "01",
                    //SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    //DETECT_EQUIP_NO = meter.BenthNo,
                    ////DETECT_UNIT_NO = string.Empty,
                    ////POSITION_NO = meter.MD_Epitope.ToString(),
                    //BAR_CODE = mtMeter.BAR_CODE,
                    ////DETECT_DATE = meter.VerifyDate.ToString(),
                    ////DETECT_ITEM_POINT = (index + 1).ToString(),
                    //PARA_INDEX = (index + 1).ToString(),
                    ////CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02",
                    ////WRITE_DATE = DateTime.Now.ToString(),
                    ////HANDLE_FLAG = "0",

                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = (index + 1).ToString(),
                    DETECT_ITEM_POINT = (index + 1).ToString(),
                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02",

                    IS_VALID = "1",
                    VOLT = GetPCode("meterTestVolt", "100%Un"),
                    LOAD_CURRENT = current,
                    DIFF_READING = meterError.WarkPower.ToString().Trim(),
                    PF = GetPCode("meterTestPowerFactor", meterError.GLYS.Trim()),
                    START_READING = meterError.PowerSumStart.ToString(),
                    END_READING = meterError.PowerSumEnd.ToString(),
                    ERROR = meterError.PowerError.Trim(),
                    STANDARD_READING = (Convert.ToSingle(meterError.STMEnergy) / meter.GetBcs()[0]).ToString("F4"),
                    REAL_PULES = meterError.Pules.Trim(),
                    QUALIFIED_PULES = meterError.STMEnergy.Trim(),
                    ERR_UP = (Convert.ToDouble(levs[0]) * 1.0).ToString("0.0"),
                    ERR_DOWN = (Convert.ToDouble(levs[0]) * (-1.0)).ToString("0.0"),
                    CONST_ERR = meterError.PowerError.Trim(),
                    READ_TYPE_CODE = "",
                };


                //entity.START_READING = meterError.PowerSumEnd.ToString();
                // entity.END_READING = meterError.PowerSumStart.ToString();
                //PowerError STMEnergy
                //sql.Add(entity.ToInsertString());
                entity.FEE_RATIO = meterError.Fl.Trim();      //1：尖，2：峰，3：平，4：谷，5：总
                //switch (meterError.Fl.Trim())
                //{
                //    case "尖":
                //        entity.FEE_RATIO = "1";
                //        break;
                //    case "峰":
                //        entity.FEE_RATIO = "2";
                //        break;
                //    case "平":
                //        entity.FEE_RATIO = "3";
                //        break;
                //    case "谷":
                //        entity.FEE_RATIO = "4";
                //        break;
                //    case "总":
                //        entity.FEE_RATIO = "5";
                //        break;
                //    default:
                //        break;
                //}

                //entity.CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02";
                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meterError.PowerWay);
                //entity.ERROR = meterError.PowerError.Trim();
                //entity.STANDARD_READING = (Convert.ToSingle(meterError.STMEnergy) / meter.GetBcs()[0]).ToString("F4");
                //entity.REAL_PULES = meterError.Pules.Trim();
                //entity.QUALIFIED_PULES = meterError.STMEnergy.Trim();
                //entity.ERR_UP = "2";
                //entity.ERR_DOWN = "-2";
                //entity.CONST_ERR = meterError.PowerError.Trim();
                entity.CONTROL_METHOD = GetPCode("meterTestCtrlMode", meterError.TestWay);
                sql.Add(entity.ToInsertString());
                index++;
            }
            return sql.ToArray();
        }
        /// <summary>
        /// 日计时
        /// </summary>
        private string GetMT_DAYERR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string sql = "";
            //平均值
            string key = "15002";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] v = meter.MeterDgns[key].Value.Split('|');
                string error = v[0] + "|" + v[1] + "|" + v[2] + "|" + v[3] + "|" + v[4];
                string ave = v[5];
                string hz = v[6];
                string result = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : "02";

                MT_DAYERR_MET_CONC entity = new MT_DAYERR_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = "1",
                    DETECT_ITEM_POINT = "01",
                    IS_VALID = "1",
                    SEC_PILES = "1",      //ProtocolInfo额外。
                    TEST_TIME = "300",
                    SIMPLING = "5",
                    ERROR = error,
                    AVG_ERR = ave,
                    INT_CONVERT_ERR = hz,
                    ERR_ABS = "0.5",
                    CONC_CODE = result,
                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un")
                };
                sql = entity.ToInsertString();
            }
            return sql;
        }


        // <summary>
        /// 密钥更新
        /// </summary>
        private string GetMT_ESAM_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {

            string s = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.密钥更新);
            if (s == "" || s == "03")
            {
                return "";
            }
            MT_ESAM_MET_CONC entity = new MT_ESAM_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = meter.VerifyDate.ToString(),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//
                KEY_NUM = "3",//密钥条数
                KEY_VER = "04",//密钥版本
                KEY_STATUS = GetPCode("secretKeyStatus", "正式密钥"), //密钥状态
                KEY_TYPE = GetPCode("secretKeyType", "身份认证密钥"), //密钥类型
                CONC_CODE = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.密钥更新),
                WRITE_DATE = DateTime.Now.ToString(),
                HANDLE_FLAG = "0"
            };
            return entity.ToInsertString();
        }

        /// <summary>
        /// 身份认证
        /// </summary>
        private string GetMT_ESAM_SECURITY_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {

            if (MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证) == "03" || MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证) == "")
            {
                return "";
            }
            MT_ESAM_SECURITY_MET_CONC entity = new MT_ESAM_SECURITY_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = meter.VerifyDate.ToString(),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                ESAM_ID = MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证) == "01" ? ConstHelper.合格 : ConstHelper.不合格,//
                CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证),
                WRITE_DATE = DateTime.Now.ToString(),
                HANDLE_FLAG = "0"
            };

            return entity.ToInsertString();
        }


        /// <summary>
        /// 需量示值误差
        /// </summary>
        /// <param name="mtMeter"></param>
        /// <param name="meter"></param>
        /// <returns></returns>
        protected string[] GetMT_DEMANDVALUE_MET_CONCMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            int Index = 1;
            for (int i = 1; i <= 3; i++)
            {
                string key = ProjectID.需量示值误差 + "_" + i.ToString();
                if (!meter.MeterDgns.ContainsKey(key)) continue;
                string[] value = meter.MeterDgns[key].Value.Split('|');
                string[] Testvalue = meter.MeterDgns[key].TestValue.Split('|');

                MeterDgn meterDgn = meter.MeterDgns[key];
                MT_DEMANDVALUE_MET_CONC entity = new MT_DEMANDVALUE_MET_CONC
                {

                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = Index.ToString(),
                    DETECT_ITEM_POINT = Index.ToString(),
                    IS_VALID = "1",

                    LOAD_CURRENT = GetPCode("meterTestCurLoad", Testvalue[0]),
                    BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", Testvalue[1]),  //功率方向
                    DEMAND_PERIOD = Testvalue[2],   //周期
                    DEMAND_TIME = Testvalue[3],   //滑差时间
                    DEMAND_INTERVAL = Testvalue[4],  //滑差次数

                    REAL_DEMAND = value[4],   //实际需量
                    REAL_PERIOD = Testvalue[2],    //实际周期
                    DEMAND_VALUE_ERR = value[5],    //需量示值误差
                    DEMAND_STANDARD = value[3],  //标准表需量值
                    DEMAND_VALUE_ERR_ABS = "1",   //需量示值误差限
                    CLEAR_DATA_RST = meterDgn.Result == ConstHelper.合格 ? "01" : "02",   //需量清零结果
                    VALUE_CONC_CODE = meterDgn.Result == ConstHelper.合格 ? "01" : "02",  //示值误差结论

                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",
                    VOLTAGE = "100%Un",
                    PF = GetPCode("meterTestPowerFactor", "1.0"),
                    CONTROL_METHOD = "01",
                    ERR_UP = value[1],
                    ERR_DOWM = value[2],
                    CHK_CONC_CODE = meterDgn.Result == ConstHelper.合格 ? "01" : "02",
                };
                sql.Add(entity.ToInsertString());

                Index++;
            }

            return sql.ToArray();
        }

        /// <summary>
        /// 规约一致性
        /// </summary>
        protected string[] GetMT_STANDARD_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            int index = 0;
            Dictionary<string, MeterDLTData> meterDLTDatas = meter.MeterDLTDatas;

            List<string> sql = new List<string>();
            foreach (string key in meterDLTDatas.Keys)
            {

                MT_STANDARD_MET_CONC entity = new MT_STANDARD_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = index.ToString(),
                    DETECT_ITEM_POINT = index.ToString(),
                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",
                    CONC_CODE = meterDLTDatas[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",

                    IS_VALID = "1",
                    DATA_FLAG = meterDLTDatas[key].DataFlag,
                    DETECT_BASIS = "",
                    SETTING_VALUE = "",
                };

                string str = meterDLTDatas[key].Value.Trim();
                if (str.Length > 120)
                    entity.READ_VALUE = str.Substring(0, 120);
                else
                    entity.READ_VALUE = str;

                index++;

                sql.Add(entity.ToInsertString());
            }
            return sql.ToArray();
        }
        /// <summary>
        /// 参数预设检查
        /// </summary>
        protected string[] GetMT_PRESETPARAM_CHECK_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            int index = 0;
            Dictionary<string, MeterDLTData> meterDLTDatas = meter.MeterDLTDatas;

            List<string> sql = new List<string>();
            foreach (string key in meterDLTDatas.Keys)
            {

                if (meterDLTDatas[key].DataFlag.IndexOf("040401") != -1 || meterDLTDatas[key].DataFlag.IndexOf("040402") != -1)
                {
                    continue;
                }
                MT_PRESETPARAM_CHECK_MET_CONC entity = new MT_PRESETPARAM_CHECK_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meter.VerifyDate.ToString(),
                    PARA_INDEX = index.ToString(),
                    DETECT_ITEM_POINT = index.ToString(),
                    WRITE_DATE = DateTime.Now.ToString(),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",
                    CONC_CODE = meterDLTDatas[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",

                    DETER_UPPER_LIMIT = "",
                    DETER_LOWER_LIMIT = "",
                    REAL_VALUE = "",
                    IS_DATA_BLOCK = "否",
                    DATA_FORMAT = meterDLTDatas[key].DataFormat,
                    CONTROL_CODE = "",
                    DATA_IDENTION = meterDLTDatas[key].DataFlag,

                    DATA_ITEM_NAME = meterDLTDatas[key].FlagMsg,
                };

                if (meterDLTDatas[key].StandardValue.Length > 120)
                {
                    entity.STANDARD_VALUE = meterDLTDatas[key].StandardValue.Substring(0, 120);
                }
                else
                {
                    entity.STANDARD_VALUE = meterDLTDatas[key].StandardValue;
                }


                string str = meterDLTDatas[key].Value.Trim();
                if (str.Length > 120)
                    entity.REAL_VALUE = str.Substring(0, 120);
                else
                    entity.REAL_VALUE = str;
                index++;
                sql.Add(entity.ToInsertString());
            }
            return sql.ToArray();
        }


        #region 误差一致性
        /// <summary>
        /// 误差一致性
        /// </summary>
        private string[] GetMT_CONSIST_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meterInfo)
        {
            string key = "1";
            if (!meterInfo.MeterErrAccords.ContainsKey(key)) return null;
            if (meterInfo.MeterErrAccords[key].PointList.Count < 0) return null;
            string[] sql = new string[meterInfo.MeterErrAccords[key].PointList.Keys.Count];
            if (meterInfo.MeterErrAccords.ContainsKey(key))          //如果数据模型中已经存在该点的数据
            {
                int iIndex = 0;
                MT_CONSIST_MET_CONC entity = new MT_CONSIST_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    DETECT_EQUIP_NO = meterInfo.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meterInfo.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = meterInfo.VerifyDate.ToString(),
                    IS_VALID = "1"
                };

                foreach (string k in meterInfo.MeterErrAccords[key].PointList.Keys)
                {
                    MeterErrAccordBase errAccord = meterInfo.MeterErrAccords[key].PointList[k];
                    entity.SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO;
                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");
                    string[] limit = errAccord.Limit.Split('|');
                    entity.ERR_DOWN = limit[1];
                    entity.ERR_UP = limit[1];
                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", errAccord.IbX);

                    entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);
                    entity.SIMPLING = "2";
                    entity.PARA_INDEX = iIndex.ToString();
                    entity.DETECT_ITEM_POINT = iIndex.ToString();
                    entity.PULES = errAccord.PulseCount.ToString();
                    entity.WRITE_DATE = DateTime.Now.ToString();
                    entity.HANDLE_FLAG = "0";
                    entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";
                    entity.ALL_AVG_ERROR = errAccord.ErrAver.Trim();
                    entity.ALL_INT_ERROR = errAccord.Error.Trim();
                    string[] Arr_Err = errAccord.Data1.Split('|');           //分解误差
                    entity.ALL_ERROR = Arr_Err[0] + "|" + Arr_Err[1];
                    entity.ERROR = errAccord.Error.Trim();
                    entity.AVG_ERROR = Arr_Err[2];
                    entity.INT_CONVERT_ERR = Arr_Err[3];

                    sql[iIndex++] = entity.ToInsertString();
                }

            }
            return sql;
        }

        /// <summary>
        /// 误差变差
        /// </summary>
        private string[] GetMT_ERROR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meterInfo)
        {

            string key = "2";
            if (!meterInfo.MeterErrAccords.ContainsKey(key)) return null;
            if (meterInfo.MeterErrAccords[key].PointList.Count < 0) return null;
            string[] sql = new string[meterInfo.MeterErrAccords[key].PointList.Keys.Count];

            MT_ERROR_MET_CONC entity = new MT_ERROR_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                DETECT_EQUIP_NO = meterInfo.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meterInfo.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = meterInfo.VerifyDate.ToString(),
                IS_VALID = "1"
            };

            int index = 0;
            foreach (string k in meterInfo.MeterErrAccords[key].PointList.Keys)
            {
                MeterErrAccordBase errAccord = meterInfo.MeterErrAccords[key].PointList[k];
                entity.SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO;
                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");
                string[] arrlimit = errAccord.Limit.Split('|');
                entity.ERR_DOWN = arrlimit[1];
                entity.ERR_UP = arrlimit[0];
                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", errAccord.IbX);

                entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);
                entity.SIMPLING = "2";
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();

                entity.WRITE_DATE = DateTime.Now.ToString();
                entity.HANDLE_FLAG = "0";
                string[] err0 = errAccord.Data1.Split('|');           //分解误差
                if (err0.Length > 1) entity.ONCE_ERR = err0[0] + "|" + err0[1];
                if (err0.Length > 2) entity.AVG_ONCE_ERR = err0[2];
                if (err0.Length > 3) entity.INT_ONCE_ERR = err0[3];
                entity.ERROR = errAccord.Error.Trim();

                entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";
                //分解误差
                //表位|校验点|误差1|误差2|平均值|化整值|误差1|误差2|平均值|化整值|差值|结论
                string[] err1 = errAccord.Data2.Split('|');           //分解误差                        
                if (err1.Length > 1) entity.TWICE_ERR = err1[0] + "|" + err1[1];
                if (err1.Length > 2) entity.AVG_TWICE_ERR = err1[2];
                if (err1.Length > 3) entity.INT_TWICE_ERR = err1[3];
                entity.ERROR = errAccord.Error.Trim();

                entity.PULES = errAccord.PulseCount.ToString();
                entity.SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO;
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();
                entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";
                sql[index++] = entity.ToInsertString();
            }

            return sql;
        }

        /// <summary>
        /// 负载电流升降变差 
        /// </summary>
        private string[] GetMT_VARIATION_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string key = "3";
            if (!meter.MeterErrAccords.ContainsKey(key)) return null;
            if (meter.MeterErrAccords[key].PointList.Count < 0) return null;
            string[] sql = new string[meter.MeterErrAccords[key].PointList.Keys.Count];
            MT_VARIATION_MET_CONC entity = new MT_VARIATION_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = meter.VerifyDate.ToString(),

                IS_VALID = "1",
                HANDLE_FLAG = "0"
            };


            int index = 0;
            foreach (string k in meter.MeterErrAccords[key].PointList.Keys)
            {
                MeterErrAccordBase errAccord = meter.MeterErrAccords[key].PointList[k];
                entity.SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO;
                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");
                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", errAccord.IbX);

                entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);
                entity.IABC = GetPCode("currentPhaseCode", "ABC");
                entity.DETECT_CIRCLE = errAccord.PulseCount.ToString();
                entity.SIMPLING = "2";
                entity.WAIT_TIME = "30";
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();

                entity.WRITE_DATE = DateTime.Now.ToString();
                entity.HANDLE_FLAG = "0";

                string[] err0 = errAccord.Data1.Split('|');           //分解误差
                entity.UP_ERR1 = err0[0];
                entity.UP_ERR2 = err0[1];
                entity.AVG_UP_ERR = err0[2];
                entity.INT_UP_ERR = err0[3];
                entity.VARIATION_ERR = errAccord.Error.Trim();
                entity.INT_VARIATION_ERR = errAccord.Error.Trim();

                entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";
                //分解误差
                //表位|校验点|误差1|误差2|平均值|化整值|误差1|误差2|平均值|化整值|差值|结论
                string[] err1 = errAccord.Data2.Split('|');           //分解误差                        
                entity.DOWN_ERR1 = err1[0];
                entity.DOWN_ERR2 = err1[1];
                entity.AVG_DOWN_ERR = err1[2];
                entity.INT_DOWN_ERR = err1[3];
                entity.VARIATION_ERR = errAccord.Error.Trim();
                entity.INT_VARIATION_ERR = errAccord.Error.Trim();

                entity.SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO;
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();
                entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";

                sql[index++] = entity.ToInsertString();
            }
            return sql;
        }

        #endregion
        /// <summary>
        /// 总结论
        /// </summary>
        private string[] GetMT_DETECT_RSLTByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            MT_EQUIP_UNPASS_REASON mt_EQUIP_UNPASS_REASON = new MT_EQUIP_UNPASS_REASON();  //不合格原因
            MT_DETECT_RSLT e = new MT_DETECT_RSLT
            {
                DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = meter.VerifyDate.ToString(),
                CONC_CODE = meter.Result == ConstHelper.合格 ? "01" : "02",
                UNPASS_REASON = "",

                INTUIT_CONC_CODE = GetInsulationConclusionFromHouda(meter, mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO),
                //INTUIT_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.外观检查试验),
                BASICERR_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.基本误差试验),
                CONST_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电能表常数试验),
                STARTING_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.起动试验),
                CREEPING_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.潜动试验),
                DAYERR_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.日计时误差),
                POWER_CONC_CODE = "01", //功率消耗结论 
                VOLT_CONC_CODE = GetVoltConcFromHouda(meter, mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO),//交流电压试验结论    
                STANDARD_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.通讯协议检查试验),   //规约一致性检查结论
                PRESET_PARAMET_CHECK_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.通讯协议检查试验),   //规约一致性检查结论
                WAVE_CONC_CODE = string.Empty,    // 载波通信性能试验结论
                ERROR_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.误差变差),   //误差变差试验结论
                CONSIST_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.误差一致性),   //误差一致性试验结论
                VARIATION_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.负载电流升将变差),   //负载电流升降变差试验结论
                OVERLOAD_CONC_CODE = string.Empty,    //电流过载试验结论
                TS_CONC_CODE = string.Empty,   //时段投切误差结论
                RUNING_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电能表常数试验),   //走字试验结论--不需要
                PERIOD_CONC_CODE = string.Empty,   //需量周期误差结论
                VALUE_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.需量示值误差),   //需量示值误差结论
                KEY_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.密钥更新),   //密钥下装结论

                ESAM_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证),   //费控安全认证试验结论
                REMOTE_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.数据回抄),   //费控远程数据回抄试验结论
                EH_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.远程保电),   //费控保电试验结论
                EC_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.保电解除),   //费控取消保电结论
                SWITCH_ON_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.远程控制),   //费控合闸试验结论
                SWITCH_OUT_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.远程控制),   //费控拉闸结论
                WARN_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.报警功能),   //费控告警试验结论
                WARN_CANCEL_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.报警功能),   //费控取消告警结论
                SURPLUS_CONC_CODE = string.Empty,    //剩余电量递减试验结论
                RESET_EQ_MET_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电量清零),   //电能表清零结论
                INFRARED_TEST_CONC_CODE = string.Empty,     //红外通信测试结论
                RESET_DEMAND_MET_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.需量示值误差),    //最大需量清零
                TIMING_MET_CONC_CODE = string.Empty,   //广播校时试验
                COMMINICATE_MET_CONC_CODE = "01",   //通讯测试
                ADDRESS_MET_CONC_CODE = "01",   //探测表地址--可以考虑加个通讯测试
                MULTI_INTERFACE_MET_CONC_CODE = "",   //多功能口测试
                LEAP_YEAR_MET_CONC = string.Empty,   //闰年切换
                PARA_READ_MET_CONC_CODE = string.Empty, //任意数据读取
                PARA_SET_MET_CONC = string.Empty,   //任意数据写入
                SETTING_CONC_CODE = string.Empty,   //参数设置结论
                DEVIATION_MET_CONC = "01",   //标准偏差
                GPS_CONC = MisDataHelper.GetBasicConclusion(meter, ProjectID.GPS对时),   //GPS校时
                DETECT_PERSON = "18",   //检定人员
                AUDIT_PERSON = "18",   //审核人员
                WRITE_DATE = DateTime.Now.ToString(),   //检定线写入时间
                HANDLE_FLAG = "0",   //平台处理标记
                HANDLE_DATE = string.Empty,    //平台处理时间
                TEMP = meter.Temperature,   //温度
                HUMIDITY = meter.Humidity,   //湿度

                PASSWORD_CHANGE_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电量清零),
                //INFLUENCE_QTY_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.报警功能),
                //施封线处理标记 SEAL_HANDLE_FLAG
                //施封线处理时间 SEAL_HANDLE_DATE
            };
            if (e.GPS_CONC == "03")
            {
                e.GPS_CONC = MisDataHelper.GetBasicConclusion(meter, ProjectID.GPS对时);   //GPS校时
            }
            e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.钱包初始化);
            if (e.RESET_EQ_MET_CONC_CODE == "03")
            {
                e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电量清零);
            }
            e.RESET_DEMAND_MET_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.钱包初始化);
            if (e.RESET_DEMAND_MET_CONC_CODE == "03")
            {
                e.RESET_DEMAND_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电量清零);
            }
            e.COMMINICATE_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, Cus_DgnItem.GPS对时);
            List<string> sql = new List<string>();
            if (!meter.YaoJianYn)
            {
                if (meter.MD_BarCode == "")
                {
                    e.CONC_CODE = string.Empty;
                }
                else
                {
                    e.CONC_CODE = "02";
                    e.FAULT_TYPE = "00120";
                    e.INTUIT_CONC_CODE = "02";
                    e.UNPASS_REASON = "00120";
                }
            }
            else
            {
                e.CONC_CODE = meter.Result.Trim() == "合格" ? "01" : "02";
                if (e.INTUIT_CONC_CODE == "03" || e.INTUIT_CONC_CODE == "02")
                {
                    e.CONC_CODE = "02";
                    e.FAULT_TYPE = "001";
                    e.UNPASS_REASON = "001";
                }
            }
            e.VOLT_CONC_CODE = GetVoltConcFromHouda(meter, TaskNO);
            if (e.VOLT_CONC_CODE == "03" || e.VOLT_CONC_CODE == "02")
            {
                e.CONC_CODE = "02";
                e.FAULT_TYPE = "027";
                mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "027";
            }
            if (e.BASICERR_CONC_CODE == "03")
            {
                e.CONC_CODE = "02";
                e.FAULT_TYPE = "00120";
                mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "00120";
            }
            //string s = e.SWITCH_ON_CONC_CODE;
            //s = e.SWITCH_OUT_CONC_CODE;
            //s = MisDataHelper.GetBasicConclusion(meter, ProjectID.GPS对时);

            //不合格原因表格上传
            if (e.CONC_CODE == "02")
            {
                mt_EQUIP_UNPASS_REASON.DETECT_TASK_NO = mtMeter.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO;
                mt_EQUIP_UNPASS_REASON.SYS_NO = mtMeter.MT_DATECT_OUT_EQUIP.SYS_NO;
                mt_EQUIP_UNPASS_REASON.EQUIP_CATEG = "01";
                mt_EQUIP_UNPASS_REASON.BAR_CODE = mtMeter.BAR_CODE;
                if (!meter.YaoJianYn)
                {
                    if (mtMeter.BAR_CODE != "")
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "00120";
                    else
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "001";
                }
                else if (e.INTUIT_CONC_CODE == "03" || e.INTUIT_CONC_CODE == "02")
                {
                    mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "001";
                    e.UNPASS_REASON = "001";
                }


                if (e.VOLT_CONC_CODE == "03" || e.VOLT_CONC_CODE == "02")
                {
                    mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "027";
                    e.UNPASS_REASON = "027";
                }
                if (e.BASICERR_CONC_CODE == "03")
                {
                    mt_EQUIP_UNPASS_REASON.UNPASS_REASON = "00120";
                    e.UNPASS_REASON = "00120";
                }

                if (e.CONC_CODE == "02")  //不合格，上传不合格原因
                {
                    if (e.BASICERR_CONC_CODE == "02")
                    {
                        string strName = "电流变化引起的百分误差（基本误差）";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.CONST_CONC_CODE == "02")
                    {
                        string strName = "电能表常数试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.STARTING_CONC_CODE == "02")
                    {
                        string strName = "起动试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.CREEPING_CONC_CODE == "02")
                    {
                        string strName = "潜动试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.DAYERR_CONC_CODE == "02")
                    {
                        string strName = "日计时误差";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.GPS_CONC == "02")
                    {
                        string strName = "校时失败";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.VALUE_CONC_CODE == "02")
                    {
                        string strName = "需量示值误差";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.PRESET_PARAMET_CHECK_CONC_CODE == "02")
                    {
                        string strName = "预置参数错误";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.STANDARD_CONC_CODE == "02")
                    {
                        string strName = "预置参数错误";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }

                    else if (e.ESAM_CONC_CODE == "02")
                    {
                        string strName = "安全认证试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.KEY_CONC_CODE == "02")
                    {
                        string strName = "密钥更新试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.SWITCH_ON_CONC_CODE == "02" || e.SWITCH_OUT_CONC_CODE == "02")
                    {
                        string strName = "远程控制试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.WARN_CANCEL_CONC_CODE == "02" || e.WARN_CONC_CODE == "02")
                    {
                        string strName = "报警功能";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.EH_CONC_CODE == "02" || e.EC_CONC_CODE == "02")
                    {
                        string strName = "费控功能";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.RESET_EQ_MET_CONC_CODE == "02" || e.RESET_DEMAND_MET_CONC_CODE == "02" || e.REMOTE_CONC_CODE == "02")
                    {
                        string strName = "费控功能";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.ERROR_CONC_CODE == "02")
                    {
                        string strName = "误差变差试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.CONSIST_CONC_CODE == "02")
                    {
                        string strName = "误差一致性试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    else if (e.VARIATION_CONC_CODE == "02")
                    {
                        string strName = "负载电流升降变差试验";
                        mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                        e.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    }
                    //else if (e.METER_ERROR_CONC_CODE == "02")
                    //{
                    //    string strName = "计度器总电能示值误差";
                    //    mt_EQUIP_UNPASS_REASON.UNPASS_REASON = GetPCode("metDetectUnqualified", strName);
                    //}
                }
                mt_EQUIP_UNPASS_REASON.WRITE_DATE = DateTime.Now.ToString();
                mt_EQUIP_UNPASS_REASON.HANDLE_FLAG = "0";
                sql.Add(mt_EQUIP_UNPASS_REASON.ToInsertString());
            }

            sql.Add(e.ToInsertString());

            return sql.ToArray(); ;
            //return e.ToInsertString();
        }
        #endregion

        #region 方法
        public string GetInsulationConclusionFromHouda(TestMeterInfo meterInfo, string task_no)
        {
            string text = "";
            string text2 = meterInfo.MD_BarCode.ToString();
            string text3 = task_no;
            if (text3 == "")
            {
                text3 = meterInfo.Other5.ToString();
            }
            string sqlstring = string.Concat(new string[]
            {
                "select * from MT_INTUIT_MET_CONC where BAR_CODE='",
                text2,
                "' and DETECT_TASK_NO='",
                text3,
                "'"
            });

            DataTable dataSet = Query(sqlstring);
            if (dataSet.Rows.Count <= 0)
            {
                text = "03";
            }
            else
            {
                for (int i = 0; i < dataSet.Rows.Count; i++)
                {
                    if (dataSet.Rows[0]["CONC_CODE"].ToString().Trim().IndexOf("02") != -1)
                    {
                        text = "02";
                    }
                }


                if (text == "02")
                {
                    text = "02";
                }
                else
                {
                    text = "01";
                }
            }
            return text;
        }
        public DataTable GetInsulationConclusionFromHouda2(TestMeterInfo meter, string task_no)
        {
            string text2 = meter.MD_BarCode.ToString();
            string text3 = task_no;
            if (text3 == "")
            {
                text3 = meter.Other5.ToString();
            }
            string sql = $"select * from MT_INTUIT_MET_CONC where BAR_CODE='{text2}' and DETECT_TASK_NO='{text3}'";

            DataTable dataSet = Query(sql);
            if (dataSet.Rows.Count <= 0)
            {
                return null;
            }
            else
            {
                return dataSet;
            }
        }

        /// <summary>
        /// 交流电压试验结论    
        /// </summary>
        /// <param name="meterInfo"></param>
        /// <param name="task_no"></param>
        /// <returns></returns>
        public string GetVoltConcFromHouda(TestMeterInfo meterInfo, string task_no)
        {
            string text = "";
            string text2 = meterInfo.MD_BarCode.ToString();
            string text3 = task_no;
            if (text3 == "")
            {
                text3 = meterInfo.Other5.ToString();
            }
            string sqlstring = string.Concat(new string[]
            {
                "select * from MT_VOLT_MET_CONC where BAR_CODE='",
                text2,
                "' and DETECT_TASK_NO='",
                text3,
                "'"
            });
            DataTable dataSet = Query(sqlstring);
            if (dataSet.Rows.Count <= 0)
            {
                text = "03";
            }
            else
            {
                for (int i = 0; i < dataSet.Rows.Count; i++)
                {
                    if (dataSet.Rows[0]["CONC_CODE"].ToString().Trim().IndexOf("02") != -1)
                    {
                        text = "02";
                    }
                }
                if (text == "02")
                {
                    text = "02";
                }
                else
                {
                    text = "01";
                }
            }
            return text;
        }


        /// <summary>
        /// 从厚达数据库获取数据
        /// </summary>
        public Dictionary<int, TestMeterInfo> GetMeterModel()
        {
            string msg = "";
            Dictionary<int, TestMeterInfo> meterList = new Dictionary<int, TestMeterInfo>();

            string bodyNo = ConfigHelper.Instance.SetControl_BenthNo;

            if (PCodeTable.Count <= 0)
                GetDicPCodeTable();

            string sql = "select COUNT(METER_CODE) AS METERCOUNTS,METER_CODE,MAX(to_number(METER_POS)) AS METER_POS from meter_pos_info where check_body_no='" + bodyNo + "' and Meter_Code IS NOT NULL GROUP BY METER_CODE";
            DataTable dt = ExecuteReader(sql);      //根据台体编号，从数据库拿去条形码

            if (dt.Rows.Count <= 0)
            {
                //不存在台体号为(" + bodyNo + ")的记录;
                return meterList;
            }

            foreach (DataRow row in dt.Rows)
            {
                TestMeterInfo meter = new TestMeterInfo();

                string barcode = row["METER_CODE"].ToString().Trim();              //条码号
                int pos = Convert.ToInt32(row["METER_POS"].ToString().Trim());        //表位号
                if (string.IsNullOrEmpty(barcode) || pos <= 0)
                {
                    //不存在台体号为(" + bodyNo + ")的表条码为空或位置号不对;
                    continue;
                }

                //根据条码号取出工单号
                sql = string.Format(@"SELECT DETECT_TASK_NO FROM MT_DETECT_OUT_EQUIP WHERE BAR_CODE= '{0}' order by write_date desc", barcode);
                object o = ExecuteScalar(sql);
                if (o == null)
                {
                    //不存在条码号为(" + barcode + ")的工单记录;
                    continue;
                }

                string strDetetTaskNo = o.ToString().Trim();

                //根据任务号查询 系统编号 和设备类型
                sql = string.Format(@"SELECT * FROM MT_DETECT_TASK T WHERE T.TASK_STATUS='21' AND T.DETECT_TASK_NO ='{0}'", strDetetTaskNo);
                DataTable dt1 = ExecuteReader(sql);


                if (dt1.Rows.Count <= 0)
                {
                    LogManager.AddMessage("不存在任务号为(" + strDetetTaskNo + ")的工单记录");
                    continue;
                }
                string strSysNo = dt1.Rows[0]["SYS_NO"].ToString().Trim();
                string strSysEquipType = dt1.Rows[0]["EQUIP_TYPE"].ToString().Trim();




                sql = string.Format(@"select * from mt_meter where bar_code='{0}'", barcode.Trim());
                DataTable dt2 = ExecuteReader(sql);

                if (dt2.Rows.Count <= 0)
                {
                    LogManager.AddMessage("不存在条形码为(" + barcode + ")的记录");
                    return null;
                }
                DataRow r1 = dt2.Rows[0];

                meter.MD_TaskNo = strDetetTaskNo;    //任务编号
                meter.Meter_ID = r1["METER_ID"].ToString().Trim();
                meter.MD_BarCode = r1["BAR_CODE"].ToString().Trim();              //条形码
                meter.MD_AssetNo = r1["ASSET_NO"].ToString().Trim();              //申请编号
                meter.MD_MadeNo = r1["MADE_NO"].ToString().Trim();              //出厂编号

                #region 表常数
                string cons = GetPName("meterConstCode", r1["CONST_CODE"]);
                string consQ = GetPName("meterConstCode", r1["RP_CONSTANT"]);

                msg += $"有功常数：【{r1["CONST_CODE"]} 值：】{cons}\r\n";

                if (consQ != "")
                    cons = cons + "(" + consQ + ")";

                meter.MD_Constant = cons;               //表常数
                #endregion

                #region 接线方式
                string wiringMode = GetPName("wiringMode", r1["WIRING_MODE"]);
                meter.MD_WiringMode = "单相";

                switch (wiringMode)
                {
                    case "三相四线":
                        meter.MD_WiringMode = "三相四线";
                        break;
                    case "三相三线":
                        meter.MD_WiringMode = "三相三线";
                        break;
                }
                #endregion

                #region 表等级
                string accu = GetPName("meterAccuracy", r1["AP_PRE_LEVEL_CODE"]);
                string accuQ = GetPName("meterAccuracy", r1["RP_PRE_LEVEL_CODE"]);
                msg += $"有功等级：【{r1["AP_PRE_LEVEL_CODE"]} 值：】{accu}\r\n";
                msg += $"无功等级：【{ r1["RP_PRE_LEVEL_CODE"]} 值：】{accuQ}\r\n";

                string s = "ABCD";
                meter.MD_JJGC = "JJG596-2012";                //表类型
                if (s.Contains(accu))
                {
                    meter.MD_JJGC = "IR46";                //表类型
                }

                if (!string.IsNullOrEmpty(accuQ))     //假如有功表等级和无功表等级不一致
                    accu = accu + "(" + accuQ + ")";

                meter.MD_Grane = accu;               //表等级

                #endregion

                #region 互感器
                meter.MD_ConnectionFlag = "直接式";

                string hgq = GetPName("conMode", r1["CON_MODE"]);
                if (hgq == "经互感器接入")
                    meter.MD_ConnectionFlag = "互感式";
                #endregion

                #region 本地或远程表
                string typeCode = GetPName("meterTypeCode", r1["TYPE_CODE"]);
                if (typeCode.IndexOf("本地") != -1)
                    meter.FKType = 1;
                else
                    meter.FKType = 0;
                #endregion

                meter.MD_MeterModel = GetPName("meterModelNo", r1["MODEL_CODE"]);                   //表型号
                meter.MD_MeterType = GetPName("meterTypeCode", r1["TYPE_CODE"]);                   //表类型

                meter.MD_Customer = r1["ORG_NO"].ToString().Trim();
                if (meter.MD_Customer == "23101")
                    meter.MD_Customer = "黑龙江省计量中心";
                //通讯协议置空,由客户自已输入 
                meter.MD_ProtocolName = "DL/T645-2013";

                #region 载波
                string carrtmp = GetPName("LocalChipManu", r1["chip_manufacturer"]);
                if (carrtmp.IndexOf("鼎信") >= 0)
                {
                    meter.MD_CarrName = "鼎信";
                }
                else if (carrtmp.IndexOf("东软") >= 0)
                {
                    meter.MD_CarrName = "东软";
                }
                else if (carrtmp.IndexOf("晓程") >= 0)
                {
                    meter.MD_CarrName = "晓程";
                }
                else if (carrtmp.IndexOf("瑞斯康") >= 0)
                {
                    meter.MD_CarrName = "瑞斯康";
                }
                else if (carrtmp.IndexOf("力合微") >= 0)
                {
                    meter.MD_CarrName = "力合微";
                }
                else
                {
                    meter.MD_CarrName = "标准载波";
                }

                #endregion

                #region 额定电压
                string ubtmp = GetPName("meterVolt", r1["VOLT_CODE"]);
                if (ubtmp.IndexOf("57.7") >= 0)
                    meter.MD_UB = 57.7f;
                else if (ubtmp.IndexOf("100") >= 0)
                    meter.MD_UB = 100;
                else if (ubtmp.IndexOf("220") >= 0)
                    meter.MD_UB = 220;
                else
                    meter.MD_UB = meter.MD_WiringMode == "单相" ? 220f : 57.7f;
                #endregion

                #region 额定电流
                string ibtmp = GetPName("meterRcSort", r1["RATED_CURRENT"]);
                msg += $"电流：【{ r1["RATED_CURRENT"]} 值：】{ibtmp}";
                meter.MD_UA = ibtmp.Trim('A');
                #endregion


                meter.MD_Factory = GetPName("meterFacturer", r1["MANUFACTURER"]);            //生产厂家
                meter.Seal1 = "";                                                 //铅封1,暂时置空
                meter.Seal2 = "";                                                 //铅封2,暂时置空
                meter.Seal3 = "";                                                 //铅封3,暂时置空       
                meter.MD_TestType = "质量抽检";

                msg = "";
                meterList.Add(pos, meter);
            }
            return meterList;
        }
        /// 获取名字
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected string GetPName(string type, object code)
        {
            string s = code.ToString().Trim();
            if (PCodeTable[type].ContainsKey(s))
                return PCodeTable[type][s];

            if (s.Length < 3)
            {
                s = s.PadLeft(4, '0');
                if (PCodeTable[type].ContainsKey(s))
                    return PCodeTable[type][s];
            }
            return "";
        }
        private void GetDicPCodeTable()
        {
            PCodeTable.Add("powerFlag", GetPCodeDic("powerFlag"));
            PCodeTable.Add("currentPhaseCode", GetPCodeDic("currentPhaseCode"));
            PCodeTable.Add("meterTestCurLoad", GetPCodeDic("meterTestCurLoad"));
            PCodeTable.Add("itRatedLoadPf", GetPCodeDic("itRatedLoadPf"));
            PCodeTable.Add("meterTestPowerFactor", GetPCodeDic("meterTestPowerFactor"));
            PCodeTable.Add("meter_Test_Volt", GetPCodeDic("meter_Test_Volt"));
            PCodeTable.Add("meterTestVolt", GetPCodeDic("meterTestVolt"));
            PCodeTable.Add("meterVolt", GetPCodeDic("meterVolt"));
            PCodeTable.Add("meterRcSort", GetPCodeDic("meterRcSort"));
            PCodeTable.Add("equip_fee_rate", GetPCodeDic("equip_fee_rate"));
            PCodeTable.Add("meterAccuracy", GetPCodeDic("meterAccuracy"));
            PCodeTable.Add("meterTypeCode", GetPCodeDic("meterTypeCode"));
            PCodeTable.Add("meterConstCode", GetPCodeDic("meterConstCode"));
            PCodeTable.Add("wiringMode", GetPCodeDic("wiringMode"));
            PCodeTable.Add("meterModelNo", GetPCodeDic("meterModelNo"));
            PCodeTable.Add("meterFacturer", GetPCodeDic("meterFacturer"));
            PCodeTable.Add("meterFreq", GetPCodeDic("meterFreq"));
            PCodeTable.Add("secretKeyStatus", GetPCodeDic("secretKeyStatus"));
            PCodeTable.Add("secretKeyType", GetPCodeDic("secretKeyType"));
            PCodeTable.Add("meterTestCtrlMode", GetPCodeDic("meterTestCtrlMode"));
            PCodeTable.Add("LocalChipManu", GetPCodeDic("LocalChipManu"));
            PCodeTable.Add("conMode", GetPCodeDic("conMode"));
            PCodeTable.Add("DET_TYPE", GetPCodeDic("DET_TYPE"));
            PCodeTable.Add("tripMode", GetPCodeDic("tripMode"));
            PCodeTable.Add("metDetectUnqualified", GetPCodeDic("metDetectUnqualified"));
            PCodeTable.Add("tari_ff", GetPCodeDic("tari_ff"));
            PCodeTable.Add("meterRateCtrlItem", GetPCodeDic("meterRateCtrlItem"));     //费控试验项目
        }
        /// <summary>
        /// 从生产调度系统数据库根据代码获取代码值
        /// </summary>
        /// <param name="codeType"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetPCodeDic(string codeType)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string sql = string.Format(@"select * from mt_p_code where code_type ='{0}'", codeType);
            if (ConfigHelper.Instance.Marketing_Type == "厚达")
            {
                sql = string.Format("select * from mt_p_code@mid where code_type ='{0}'", codeType);
            }

            DataTable dr = ExecuteReader(sql);
            string msg = "查询到电流规格：";
            foreach (DataRow r in dr.Rows)
            {
                string value = r["value"].ToString().Trim();
                string name = r["name"].ToString().Trim();
                if (codeType == "meterRcSort")
                {
                    msg += $"编码：{value}  值：【{name}】\r\n";
                }
                if (value.Length > 0)
                {
                    if (!dic.ContainsKey(value))
                        dic.Add(value, name);
                }
            }
            return dic;
        }
        private MT_METER GetMt_meter(string barcode)
        {
            string strSysNo;
            string strDetectTaskNo;

            //根据条码号取出工单号 根据条码号取出工单号
            string sql = "SELECT DETECT_TASK_NO FROM MT_DETECT_OUT_EQUIP WHERE BAR_CODE='" + barcode + "'order by  write_date desc";
            object o = ExecuteScalar(sql);
            if (o == null)
            {
                LogManager.AddMessage("不存在条码号为(" + barcode + ")的工单记录", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                //("不存在条码号为(" + barcode + ")的工单记录");
                return null;
            }
            string detetTaskNo = o.ToString().Trim();

            //根据任务号查询 系统编号 和设备类型
            sql = string.Format(@"SELECT * FROM MT_DETECT_TASK T WHERE T.TASK_STATUS='21' AND T.DETECT_TASK_NO ='{0}'", detetTaskNo);

            DataTable dr = ExecuteReader(sql);
            if (dr.Rows.Count > 0)
            {
                strSysNo = dr.Rows[0]["SYS_NO"].ToString().Trim();
                strDetectTaskNo = dr.Rows[0]["DETECT_TASK_NO"].ToString().Trim();
            }
            else
            {
                LogManager.AddMessage("不存在任务号为(" + detetTaskNo + ")的工单记录", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                return null;
            }

            sql = "select * from mt_meter where bar_code='" + barcode.Trim() + "'";

            DataTable dr1 = ExecuteReader(sql);
            DataRow row = dr1.Rows[0];

            MT_METER model = new MT_METER();

            if (row["METER_ID"].ToString() != "")
            {
                model.METER_ID = row["METER_ID"].ToString();
            }
            model.BAR_CODE = row["BAR_CODE"].ToString();
            model.LOT_NO = row["LOT_NO"].ToString();
            model.ASSET_NO = row["ASSET_NO"].ToString();
            model.MADE_NO = row["MADE_NO"].ToString();
            model.SORT_CODE = row["SORT_CODE"].ToString();
            model.TYPE_CODE = row["TYPE_CODE"].ToString();
            model.MODEL_CODE = row["MODEL_CODE"].ToString();
            model.WIRING_MODE = row["WIRING_MODE"].ToString();
            model.VOLT_CODE = row["VOLT_CODE"].ToString();
            model.OVERLOAD_FACTOR = row["OVERLOAD_FACTOR"].ToString();
            model.AP_PRE_LEVEL_CODE = row["AP_PRE_LEVEL_CODE"].ToString();
            model.CONST_CODE = row["CONST_CODE"].ToString();
            model.RP_CONSTANT = row["RP_CONSTANT"].ToString();
            model.PULSE_CONSTANT_CODE = row["PULSE_CONSTANT_CODE"].ToString();
            model.FREQ_CODE = row["FREQ_CODE"].ToString();
            model.RATED_CURRENT = row["RATED_CURRENT"].ToString();
            model.CON_MODE = row["CON_MODE"].ToString();
            model.SOFT_VER = row["SOFT_VER"].ToString();
            model.HARD_VER = row["HARD_VER"].ToString();
            model.MT_DATECT_OUT_EQUIP.SYS_NO = strSysNo;
            model.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO = strDetectTaskNo;

            return model;
        }

        ///// <summary>
        ///// 创建CSV文件并写入内容
        ///// </summary>
        ///// <param name="dt">DataTable</param>
        ///// <param name="fileName">文件全名</param>
        ///// <returns>是否写入成功</returns>
        //public static bool SaveCSV(DataTable dt, string fullFileName)
        //{
        //    System.IO.FileStream fs = new System.IO.FileStream(fullFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        //    System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.Default);
        //    string data = "";

        //    //写出列名称
        //    for (int i = 0; i < dt.Columns.Count; i++)
        //    {
        //        data += dt.Columns[i].ColumnName.ToString();
        //        if (i < dt.Columns.Count - 1)
        //        {
        //            data += ",";
        //        }
        //    }
        //    sw.WriteLine(data);

        //    //写出各行数据
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        data = "";
        //        for (int j = 0; j < dt.Columns.Count; j++)
        //        {
        //            data += dt.Rows[i][j].ToString();
        //            if (j < dt.Columns.Count - 1)
        //            {
        //                data += ",";
        //            }
        //        }
        //        sw.WriteLine(data);
        //    }

        //    sw.Close();
        //    fs.Close();

        //    bool r = true;
        //    return r;
        //}

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
