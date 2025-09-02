using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core;
using LYTest.Core.Model;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.Mis.Common;
using LYTest.Mis.MisData;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LYTest.Mis.ShangHai20.Tables;

namespace LYTest.Mis.ShangHai20
{
    public class ShangHai20 : OracleHelper, IMis
    {
        public string SysNo { get; private set; } = "450";
        public ShangHai20(string ip, int port, string dataSource, string userId, string pwd, string url, string sysno)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;
            SysNo = sysno;
        }

        private int SusessCount = 0;//成功上传数量
        /// <summary>
        /// 任务单号
        /// </summary>
        private string TaskNO = "";

        public readonly static Dictionary<string, Dictionary<string, string>> PCodeTable = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// 时区时段表数据
        /// </summary>
        private RatePeriodTable ratePeriodTable = null;


        /// <summary>
        /// 下载表信息
        /// </summary>
        /// <param name="MD_BarCode"></param>
        /// <param name="meter"></param>
        /// <returns></returns>
        public bool Down(string MD_BarCode, ref TestMeterInfo meter)
        {

            if (string.IsNullOrEmpty(MD_BarCode)) return false;

            if (PCodeTable.Count <= 2)
                GetDicPCodeTable();

            string sql = string.Format(@"SELECT * FROM mt_detect_out_equip t1 INNER JOIN mt_meter t2 ON t1.bar_code=t2.bar_code WHERE t2.bar_code='{0}' ORDER BY t1.write_date DESC", MD_BarCode.Trim());

            DataTable dt = ExecuteReader(sql);
            if (dt.Rows.Count <= 0) return false;

            DataRow row = dt.Rows[0];

            meter.Meter_ID = row["METER_ID"].ToString().Trim();        //表唯一编号
            meter.MD_BarCode = row["BAR_CODE"].ToString().Trim();      //条形码
            meter.MD_AssetNo = row["ASSET_NO"].ToString().Trim();      //申请编号 --资产编号
            meter.MD_MadeNo = row["MADE_NO"].ToString().Trim();        //出厂编号
            meter.MD_Sort = GetPName("meterSort", row["SORT_CODE"]);//类别
            meter.MD_TaskNo = row["DETECT_TASK_NO"].ToString().Trim(); //检定任务单，申请编号
            meter.MD_PostalAddress = row["COMM_ADDR"].ToString().Trim();//通信地址
            meter.MD_Frequency = 50;

            //表类型  
            meter.MD_MeterType = GetPName("meterTypeCode", row["TYPE_CODE"]);                   //表类型
            #region 本地或远程表
            if (!string.IsNullOrEmpty(meter.MD_MeterType) && meter.MD_MeterType.IndexOf("本地") != -1)
                meter.FKType = 1;
            else
                meter.FKType = 0;
            #endregion

            #region 表常数
            string cons = GetPName("meterConstCode", row["CONST_CODE"]);
            string consQ = GetPName("meterConstCode", row["RP_CONSTANT"]);

            if (consQ != "")
                cons = cons + "(" + consQ + ")";
            meter.MD_Constant = cons;               //表常数
            #endregion

            #region 接线方法
            string wMode = GetPName("wiringMode", row["WIRING_MODE"]);

            switch (wMode)
            {
                case "三相四线":
                    meter.MD_WiringMode = "三相四线";
                    break;
                case "三相三线":
                    meter.MD_WiringMode = "三相三线";
                    break;
                case "单相":
                    meter.MD_WiringMode = "单相";
                    break;
                default:
                    meter.MD_WiringMode = "单相";
                    break;
            }
            #endregion

            #region 表等级

            string accurP = GetPName("meterAccuracy", row["AP_PRE_LEVEL_CODE"]);
            string accurQ = GetPName("meterAccuracy", row["RP_PRE_LEVEL_CODE"]);
            string accurDJ = accurP;
            if (accurQ != "")     //假如有功表等级和无功表等级不一致
                accurDJ = accurP + "(" + accurQ + ")";
            meter.MD_Grane = accurDJ;               //表等级
            #endregion

            #region 互感器
            meter.MD_ConnectionFlag = "直接式";

            string hgq = GetPName("conMode", row["CON_MODE"]);
            if (hgq == "经互感器接入")
                meter.MD_ConnectionFlag = "互感式";
            #endregion

            meter.MD_MeterModel = GetPName("meterModelNo", row["MODEL_CODE"]);                   //表型号
            meter.MD_Customer = row["ORG_NO"].ToString().Trim();
            if (meter.MD_Customer == "23101")
                meter.MD_Customer = "黑龙江省计量中心";
            else if (meter.MD_Customer == "22101")
                meter.MD_Customer = "吉林省计量中心";
            else if (meter.MD_Customer == "51101")
                meter.MD_Customer = "四川省计量中心";
            else if (meter.MD_Customer == "65101")
                meter.MD_Customer = "新疆电力公司";

            #region 电能表通讯协议
            meter.MD_ProtocolName = "CDLT698";
            string commProtocol = GetPName("commProtocol", row["comm_prot_Code"]);
            if (commProtocol.IndexOf("698") >= 0)
            {
                meter.MD_ProtocolName = "CDLT698";
            }
            //else if (commProtocol.IndexOf("DL/T645-2007") >= 0 || commProtocol.IndexOf("DL/T645-2013") >= 0)
            //{
            //    meter.MD_ProtocolName = "CDLT6452007";
            //}
            else if (commProtocol.IndexOf("T645") >= 0)
            {
                meter.MD_ProtocolName = "CDLT6452007";
            }
            else if (commProtocol.IndexOf("面向对象") != -1)
            {
                meter.MD_ProtocolName = "CDLT698";
            }
            #endregion

            #region 电能表规程
            meter.MD_JJGC = "JJG596-2012";
            string djIR46 = "ABCDE";

            if (djIR46.IndexOf(accurP) != -1)
            {
                meter.MD_JJGC = "IR46";
            }
            #endregion

            meter.DgnProtocol = null;

            #region 载波
            meter.MD_CarrName = "标准载波";
            string carrtmp = GetPName("LocalChipManu", row["chip_manufacturer"]);
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
            string ubtmp = GetPName("meterVolt", row["VOLT_CODE"]);
            if (ubtmp.IndexOf("57.7") >= 0)
            {
                meter.MD_UB = 57.7f;
            }
            else if (ubtmp.IndexOf("100") >= 0)
            {
                meter.MD_UB = 100;
            }
            else if (ubtmp.IndexOf("220") >= 0)
            {
                meter.MD_UB = 220;
            }
            else
                meter.MD_UB = meter.MD_WiringMode == "单相" ? 220f : 57.7f;
            #endregion

            #region 额定电流
            string ibtmp = GetPName("meterRcSort", row["RATED_CURRENT"]);
            meter.MD_UA = ibtmp.Trim('A');
            #endregion

            //meter.IsTest = true;

            meter.MD_Factory = GetPName("meterFacturer", row["MANUFACTURER"]);    //生产厂家
            meter.Seal1 = "";                       //铅封1,暂时置空
            meter.Seal2 = "";                       //铅封2,暂时置空
            meter.Seal3 = "";                       //铅封3,暂时置空
            meter.Meter_SysNo = row["SYS_NO"].ToString(); //系统编号

            return true;
        }

        public void ShowPanel(Control panel)
        {

        }

        public bool Update(TestMeterInfo meterInfo)
        {
            bool exeRst;
            bool sqlrst = true;
            List<string> sqlList = new List<string>();

            MT_METER meter = GetMeter(meterInfo.MD_BarCode);
            if (meter == null)
            {
                meter = GetMeter(meterInfo.MD_BarCode);
                if (meter == null)
                {
                    LogManager.AddMessage("没有查查到表信息", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            TaskNO = meter.DETECT_OUT_EQUIP.DETECT_TASK_NO;
            meterInfo.Other5 = TaskNO;
            meterInfo.MD_BarCode = meterInfo.MD_BarCode.Trim();
            if (string.IsNullOrWhiteSpace(meterInfo.VerifyDate))
            {
                meterInfo.VerifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (PCodeTable.Count <= 0)
                GetDicPCodeTable();

            #region 删除旧数据
            sqlList.Add("delete from MT_BASICERR_MET_CONC where detect_task_no = '" + TaskNO + "' and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_INTUIT_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_MEASURE_REPEAT_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_DEVIATION_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_STARTING_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_CREEPING_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_CONST_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_DAYERR_MET_CONC  where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_ESAM_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_DETECT_RSLT where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_PRESETPARAM_CHECK_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_PRESETPARAM_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_PASSWORD_CHANGE_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_CONSIST_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_DETECT_RSLT where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from Mt_OVERLOAD_MET_CONC  where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_TS_MET_CONC  where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_ERROR_MET_CONC where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_VOLT_MET_CONC  where  detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_VARIATION_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_DEMANDVALUE_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_ESAM_SECURITY_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");

            sqlList.Add("delete from MT_HUTCHISON_COMBINA_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_TIME_ERROR_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_INFLUENCE_QTY_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_GPS_TIMING where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_INFLUENCE_QTY_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_WAVE_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_WARNNING_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_EP_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_EC_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_ESAM_READ_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_PARA_SETTING_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_FEE_TMNL_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_POWER_MEASURE_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_POWER_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_STANDARD_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_MAX_DEMANDVALUE_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_FEE_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");

            sqlList.Add("delete from MT_CONTROL_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_EQ_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_SURPLUS_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");
            sqlList.Add("delete from MT_CLOCK_VALUE_MET_CONC where detect_task_no = '" + TaskNO + "'and bar_code='" + meterInfo.MD_BarCode + "' ");

            #endregion

            exeRst = Execute(sqlList);
            if (!exeRst)
            {
                exeRst = Execute(sqlList);
            }
            sqlList.Clear();

            string[] sql;

            try
            {
                //外观检查试验
                sqlList.Add(GetMT_INTUIT_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("外观检查试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //启动
                sql = GetMT_STARTING_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //潜动
                sql = GetMT_CREEPING_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("启动潜动试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //常数
                sql = GetMT_CONST_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("常数试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //基本误差试验
                sql = GetMT_BASICERR_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //（初始固有误差表）基本误差试验
                //sql = GetMT_INIERR_MET_CONCByMt(meter, meterInfo);
                //if (sql != null)
                //{
                //    foreach (string strQuest in sql)
                //    {
                //        sqlList.Add(strQuest);
                //    }
                //}

                //暂时不需要
                //标准偏差（测量重复性检测）
                //sql = GetMT_DEVIATION_MET_CONCByMt(meter, meterInfo);
                //if (sql != null)
                //{
                //    foreach (string strQuest in sql)
                //    {
                //        sqlList.Add(strQuest);
                //    }
                //}
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("基本误差和标准偏差（重复性）实验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //日计时
                sqlList.Add(GetMT_DAYERR_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("日计时试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //时间误差
                sqlList.Add(GetMT_CLOCK_VALUE_MET_CONC(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("时间误差试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //计度器总电能示值组合误差
                sql = GetMT_HUTCHISON_COMBINA_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("计度器总电能示值组合误差试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            //add yjt 20220513 新增时段投切的数据上传
            try
            {
                //时段投切
                sql = GetMT_TS_MET_CONC(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("时段投切试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //需量示值误差
                sql = GetMT_DEMANDVALUE_MET_CONC(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("需量示值误差试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //规约一致性   通讯协议检查
                sql = GetMT_STANDARD_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("规约一致性试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //交流电压试验(交流耐压试验)
                sqlList.Add(GetMT_VOLT_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("耐压试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //密钥更新
                sqlList.Add(GetMT_ESAM_MET_CONCByMt(meter, meterInfo));

                //身份认证
                sqlList.Add(GetMT_ESAM_SECURITY_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("密钥试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //剩余电量递减
                sqlList.Add(GetMT_EQ_MET_CONCByMt(meter, meterInfo));

                //剩余电量递减
                sqlList.Add(MT_SURPLUS_MET_CONCByMt(meter, meterInfo));//不一样，需调试看看
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("剩余电量递减准确度试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //控制功能
                sqlList.Add(GetMT_CONTROL_MET_CONCByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("控制功能试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //一致性误差
                sql = GetMT_CONSIST_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //变差要求试验(误差变差试验)
                sql = GetMT_ERROR_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
                //负载电流升降变差
                sql = GetMT_VARIATION_MET_CONCByMt(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("误差一致性试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            try
            {
                //预置内容检查实验
                sql = GetMT_PRESETPARAM_CHECK_MET_CONCByMtFK(meter, meterInfo);
                if (sql != null)
                {
                    foreach (string strQuest in sql)
                    {
                        sqlList.Add(strQuest);
                    }
                }
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("预置内容检查插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            try
            {
                //总结论
                sqlList.Add(GetMT_DETECT_RSLTByMt(meter, meterInfo));
            }
            catch (Exception ex)
            {
                sqlrst = false;
                LogManager.AddMessage("总结论插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            #region 非全检项目
            //#region
            //try
            //{
            //    //载波
            //    sql = GetMT_WAVE_MET_CONCByMt(meter, meterInfo);
            //    if (sql != null)
            //    {
            //        foreach (string strQuest in sql)
            //        {
            //            sqlList.Add(strQuest);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogManager.AddMessage("载波试验插入出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            //}



            ////电流过载过载
            //sqlList.Add(GetMT_OVERLOAD_MET_CONCByMt(meter, meterInfo));



            ////费率时段电能示值误差
            //sql = GetMT_TIME_ERROR_MET_CONCByMt(meter, meterInfo);
            //if (sql != null)
            //{
            //    foreach (string strQuest in sql)
            //    {
            //        sqlList.Add(strQuest);
            //    }
            //}

            ////影响量
            //sql = GetMT_INFLUENCE_QTY_MET_CONCByMt(meter, meterInfo);
            //if (sql != null)
            //{
            //    foreach (string strQuest in sql)
            //    {
            //        sqlList.Add(strQuest);
            //    }
            //}

            ////费率和时段功能
            //sqlList.Add(GetMT_FEE_MET_CONCByMt(meter, meterInfo));    //MT_FEE_TMNL_CONC改为MT_FEE_MET_CONC
            ////计量功能
            //sqlList.Add(GetMT_POWER_MEASURE_MET_CONCByMt(meter, meterInfo));
            ////最大需量功能
            //sqlList.Add(GetMT_MAX_DEMANDVALUE_MET_CONCByMt(meter, meterInfo));
            ////功耗
            //sqlList.Add(GetMT_POWER_MET_CONCByMt(meter, meterInfo));

            //#endregion           
            #endregion


            if (exeRst)
            {
                exeRst = Execute(sqlList);
                if (!exeRst)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        exeRst = Execute(sqlList);
                        if (exeRst) break;
                    }
                }
                if (exeRst)
                {
                    SusessCount++;
                }
            }
            return sqlrst && exeRst;
        }




        public bool UpdateCompleted()
        {
            string xml = "<PARA>";
            xml += "<SYS_NO>" + SysNo + "</SYS_NO>";
            xml += "<DETECT_TASK_NO>" + TaskNO + "</DETECT_TASK_NO>";
            xml += "</PARA>";
            string[] args = new string[1];
            args[0] = xml;
            object result = WebServiceHelper.InvokeWebService(WebServiceURL, "getDETedTestData", args);
            if (result.ToString().ToUpper() == "FALSE")
                //if (result.ToString().IndexOf("成功") == -1)
                result = WebServiceHelper.InvokeWebService(WebServiceURL, "getDETedTestData", args);


            if (!WebServiceHelper.GetResultByXml(result.ToString()))
            {
                MessageBox.Show("数据上传失败，调用分项结论接口错误信息：" + WebServiceHelper.GetMessageByXml(result.ToString()));
                return false;
            }

            xml = "<PARA>";
            xml += "<SYS_NO>" + SysNo + "</SYS_NO>";
            xml += "<DETECT_TASK_NO>" + TaskNO + "</DETECT_TASK_NO>";
            xml += "<VALID_QTY>" + SusessCount + "</VALID_QTY>";
            xml += "</PARA>";
            args[0] = xml;
            result = WebServiceHelper.InvokeWebService(WebServiceURL, "setResults", args);
            if (result.ToString().ToUpper() == "FALSE")
                //if (result.ToString().IndexOf("成功") == -1)
                result = WebServiceHelper.InvokeWebService(WebServiceURL, "setResults", args);

            SusessCount = 0;
            if (!WebServiceHelper.GetResultByXml(result.ToString()))
            {
                MessageBox.Show("数据上传失败，调用综合结论接口错误信息：" + WebServiceHelper.GetMessageByXml(result.ToString()));
                return false;
            }
            return true;
        }

        readonly Dictionary<string, string> nameList = new Dictionary<string, string>(); //用来判断是否已经添加过了该项，用于需要同时698和645协议检查的
        private readonly bool IoT_Meter = false;
        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            nameList.Clear();
            Schema = new Dictionary<string, SchemaNode>();
            schemeName = "";

            try
            {
                string sql = $"select detect_task_no from mt_detect_out_equip where bar_code='{barcode}' order by write_date desc";
                object obj = ExecuteScalar(sql);
                if (obj != null)
                {
                    string taskno = obj.ToString().Trim();

                    sql = $"select schema_id,detect_task_no from MT_DETECT_TASK where detect_task_no='{taskno}'";
                    var table = ExecuteReader(sql);
                    if (table.Rows.Count <= 0) return false; //无对应检定单

                    string SchemeID = table.Rows[0]["schema_id"].ToString().Trim();
                    TaskNO = table.Rows[0]["detect_task_no"].ToString().Trim();
                    if (SchemeID.Length <= 0) return false; //检定任务单号无效

                    return SchemeDownBySchemeID(SchemeID, out schemeName, out Schema);
                }
                return false;
            }
            catch
            {

            }
            return false;
        }

        public bool SchemeDownBySchemeID(string schemeID, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            nameList.Clear();
            Schema = new Dictionary<string, SchemaNode>();
            schemeName = "";

            string SchemeID = schemeID;
            if (SchemeID.Length <= 0) return false; //检定任务单号无效

            string sql = string.Format(@"select * from MT_DETECT_SCHEME where SCHEMA_ID = '{0}'", SchemeID);
            DataTable dt = ExecuteReader(sql);

            if (dt.Rows.Count <= 0) return false;//检定任务无明细

            //string schemeNo = dt.Rows[0]["SCHEMA_NO"].ToString().Trim();
            schemeName = dt.Rows[0]["SCHEMA_NAME"].ToString().Trim();
            //string detectType = dt.Rows[0]["DETECT_TYPE"].ToString().Trim();
            //string detectMode = dt.Rows[0]["DETECT_MODE"].ToString().Trim();
            //string equipCateg = dt.Rows[0]["EQUIP_CATEG"].ToString().Trim();
            //string handFlag = dt.Rows[0]["HANDLE_FLAG"].ToString().Trim();

            schemeName = schemeName.Replace('/', '_');






            bool rst = SchemeDownDetPara($"SCHEMA_ID='{SchemeID}' order by to_number(PARA_INDEX)", ref Schema);



            return rst;
        }
        //调试后改为private
        //调试后改为private
        public bool SchemeDownDetPara(string where, ref Dictionary<string, SchemaNode> Schema)
        {
            if (PCodeTable.Count <= 0)
            {
                GetDicPCodeTable();
            }

            #region 方案详细及参数

            string sql = $"select * from MT_DETECT_SCHEME_DET where 1=1 and {where}";

            DataTable det = Query(sql);

            if (det.Rows.Count <= 0 )
            {
                LogManager.AddMessage($"MT_DETECT_SCHEME_DET表中不存在{where}的方案信息", Utility.Log.EnumLogSource.数据库存取日志);
                return false;
            }
            string values;
            string ID;


            //DataTable dataTable4 = det.Tables[0];
            for (int j = 0; j < det.Rows.Count; j++)
            {
                string PARA_ID = det.Rows[j]["PARA_NO"].ToString().Trim();
                string PARA_VALUE = det.Rows[j]["PARA_VALUE"].ToString().Trim();
                sql = string.Format("select * from MT_DETECT_PARA_INFO where para_no = '{0}'", PARA_ID);
                DataTable detpara = Query(sql);

                if (detpara.Rows.Count <= 0 )
                {
                    Utility.Log.LogManager.AddMessage($"MT_DETECT_PARA_INFO表中不存在{where}  para_id='{PARA_ID}')的方案信息", Utility.Log.EnumLogSource.数据库存取日志);
                    return false;
                }
                //DataTable dataTable2 = detpara.Tables[0];
                for (int k = 0; k < detpara.Rows.Count; k++)
                {
                    string paraName = detpara.Rows[k]["PARA_NAME"].ToString().Trim();
                    string[] paras = PARA_VALUE.Split(new char[] { ',' });
                    switch (paraName)
                    {
                        case "交流电压试验":
                            ID = ProjectID.工频耐压试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string para in paras)
                            {
                                string[] par = para.Split('|');
                                string[] mparas = new string[7];
                                mparas[0] = "UI对地";
                                mparas[1] = par[0];
                                mparas[2] = par[3];
                                mparas[3] = par[2];
                                mparas[4] = "10";
                                mparas[5] = "10";
                                mparas[6] = par[4];
                                Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue(mparas));
                            }
                            break;
                        case "外观、标志检查":
                            //外观工位做
                            break;
                        case "通电检查":
                            ID = ProjectID.通讯测试;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            break;
                        case "起动试验":
                            #region 起动试验
                            ID = ProjectID.起动试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string para in paras)
                            {
                                string[] ps = para.Split('|'); //功率方向|电流倍数|起动时间
                                PowerWay fx = PowerWay.正向有功;
                                float ib = 0;
                                if (ps.Length > 3)
                                {
                                    fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));

                                    string starti = GetPName("meter_Test_CurLoad", ps[1]);

                                    ib = float.Parse(starti.Trim('I', 'b', 'n', 't', 'r', 'i'));

                                    float time = float.Parse(ps[2]);  //起动时间    
                                    values = MisScheme.JoinValue(fx.ToString(), ib.ToString(), "是", "是", "否", time.ToString());
                                    Schema[ID].SchemaNodeValue.Add(values);
                                }
                            }
                            #endregion
                            break;
                        case "潜动试验":
                            #region 潜动试验
                            ID = ProjectID.潜动试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string p in paras)
                            {
                                string[] ps = p.Split('|'); //功率方向|电压值|电流值|潜动时间
                                PowerWay fx = PowerWay.正向有功;
                                string volt = "";
                                float ib = 0;
                                if (ps.Length > 3)
                                {
                                    fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                    string u = GetPName("meterTestVolt", ps[1]).Trim('%', 'U', 'n');
                                    volt = u + "%";

                                    if (ps[2].IndexOf("Ib") != -1)
                                        ib = float.Parse(GetPName("meter_Test_CurLoad", ps[2]).Trim('b').Trim('I'));
                                    else
                                        ib = float.Parse(GetPName("meter_Test_CurLoad", ps[2]).Trim('n').Trim('I'));

                                    float time = float.Parse(ps[2]);
                                    values = MisScheme.JoinValue(fx.ToString(), volt, "默认电流开路".ToString(), "是", "否", time.ToString());
                                    Schema[ID].SchemaNodeValue.Add(values);
                                }
                            }
                            #endregion
                            break;
                        case "初始固有误差":
                        case "初始固有误差试验":
                            #region 初始固有误差试验
                            ID = ProjectID.初始固有误差试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string p in paras)
                            {
                                string[] ps = p.Split('|'); //功率方向|元件|电流值|功率因数|圈数|频率|电压|上限|下限

                                PowerWay fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                Cus_PowerYuanJian yj = GetYuanJianFromString(GetPName("currentPhaseCode", ps[1]));
                                string xIb = GetPName("meter_Test_CurLoad", ps[2]).Replace("In", "Ib");
                                if (xIb == "Ib") xIb = "1.0Ib";
                                if (xIb == "Itr") xIb = "1Itr";
                                if (xIb == "10.0Itr") xIb = "10Itr";
                                string glys = GetPName("meterTestPowerFactor", ps[3]);
                                string qs = ps[4];
                                string hz = GetPName("meterTestFreq", ps[5]);
                                string voltage = GetPName("meterTestVolt", ps[6]);

                                string limitUp = ps[7];
                                string limitDown = ps[8];
                                string limit = limitUp + "|" + limitDown;
                                values = MisScheme.JoinValue("基本误差", fx.ToString(), yj.ToString(), glys, xIb, "否", "否", "2", "100");
                                Schema[ID].SchemaNodeValue.Add(values);
                            }
                            #endregion
                            break;
                        case "基本误差":
                        case "基本误差试验":
                            #region 基本误差
                            ID = ProjectID.基本误差试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string p in paras)
                            {
                                string[] ps = p.Split('|'); //功率方向|元件|电流值|功率因数|圈数|频率|电压|上限|下限

                                PowerWay fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                Cus_PowerYuanJian yj = GetYuanJianFromString(GetPName("currentPhaseCode", ps[1]));
                                string xIb = GetPName("meter_Test_CurLoad", ps[2]).Replace("In", "Ib");
                                if (xIb == "Ib") xIb = "1.0Ib";
                                if (xIb == "Itr") xIb = "1Itr";
                                if (xIb == "10.0Itr") xIb = "10Itr";
                                string glys = GetPName("meterTestPowerFactor", ps[3]);
                                string qs = ps[4];
                                string hz = GetPName("meterTestFreq", ps[5]);
                                string voltage = GetPName("meterTestVolt", ps[6]);

                                string limitUp = ps[7];
                                string limitDown = ps[8];
                                string limit = limitUp + "|" + limitDown;
                                values = MisScheme.JoinValue("基本误差", fx.ToString(), yj.ToString(), glys, xIb, "否", "否", "2", "100");
                                Schema[ID].SchemaNodeValue.Add(values);
                            }
                            #endregion
                            break;
                        case "测量重复性试验(新规)":
                            break;
                        case "常数试验":
                        case "仪表常数试验":
                            #region 常数试验
                            ID = ProjectID.电能表常数试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            {
                                foreach (string strWcPara in paras)
                                {
                                    string[] ps = strWcPara.Split('|');
                                    PowerWay fx = PowerWay.正向有功;
                                    string xIb = "1.0Ib";
                                    string glys = "1.0";
                                    string fl = "总";
                                    string ctrl = "计读脉冲法";
                                    if (ps.Length > 3)
                                    {
                                        fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                        xIb = GetPName("meter_Test_CurLoad", ps[1]);
                                        glys = GetPName("meterTestPowerFactor", ps[2]);
                                        //费率 总 5
                                        fl = GetPName("tari_ff", ps[3]);
                                        //试验方法 计读脉冲法 02
                                        ctrl = GetPName("meterTestCtrlMode", ps[4]);

                                        string StartTime = ps[5];
                                        string ZouZiTime = ps[7];//kWh
                                        values = MisScheme.JoinValue(fx.ToString(), "H", glys, xIb, ctrl, fl, ZouZiTime, "0");
                                        Schema[ID].SchemaNodeValue.Add(values);
                                    }
                                }
                            }
                            #endregion
                            break;
                        case "由电源供电的时钟试验":
                        case "日计时误差":
                            #region 由电源供电的时钟试验
                            ID = ProjectID.日计时误差;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            {
                                string[] ps = paras[0].Split('|');
                                string pulse = "10";
                                string getNum = "5";
                                if (ps.Length > 7)
                                {
                                    int.TryParse(ps[2], out int n);
                                    pulse = (n * 60).ToString();//
                                    getNum = ps[3];  //误差个数
                                }
                                if (int.Parse(pulse) < 10)
                                    pulse = "10";
                                if (getNum.Length < 1)
                                    getNum = "5";
                                values = MisScheme.JoinValue("0.5", getNum, pulse, "50000", "1", "");
                                Schema[ID].SchemaNodeValue.Add(values);
                            }
                            #endregion
                            break;
                        case "电能示值组合误差":
                        case "总电能示值组合误差":
                        case "计度器示值组合误差":
                        case "计度器总电能示值组合误差":
                            {
                                ID = ProjectID.电能示值组合误差;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                string sd = "";
                                string current = "Imax";
                                string kwh = "0.5";
                                string limit = "0.02";
                                string read = "";
                                foreach (string para in paras)
                                {
                                    string[] par = para.Split('|');
                                    if (par[11] == "1" && string.IsNullOrWhiteSpace(sd))
                                    {
                                        read = "读";
                                    }
                                    sd += $"{par[7]}({GetPName("fee", par[7])}),";//tari_ff
                                    current = GetPName("meter_Test_CurLoad", par[2]);
                                    kwh = par[4];
                                    limit = par[8];
                                }
                                string[] mparas = new string[7];
                                mparas[0] = sd;
                                mparas[1] = "0";
                                mparas[2] = current;
                                mparas[3] = "否";
                                mparas[4] = read == "读" ? "是" : "否";
                                mparas[5] = kwh;
                                mparas[6] = limit;
                                Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue(mparas));
                            }
                            break;
                        case "费率时段电能示值误差":
                            break;
                        case "时段投切误差试验":
                            break;
                        case "钱包初始化":
                            ID = ProjectID.钱包初始化;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("100");      //TODO需要改变初始化金额
                            break;
                        case "电能表清零":
                            ID = ProjectID.电量清零;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("0");
                            break;
                        case "校时功能":
                            ID = ProjectID.GPS对时;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string para in paras)//参数现在不用
                            {
                                string[] par = para.Split('|');
                                string[] mparas = new string[2];
                                mparas[0] = par[0];//校时前时间
                                mparas[1] = par[1];//校时后时间
                            }
                            Schema[ID].SchemaNodeValue.Add("");
                            break;
                        case "485密码修改":
                            break;
                        case "电能表预置内容检查":
                            break;
                        case "需量示值误差":
                            #region 需量示值误差
                            ID = ProjectID.需量示值误差;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            {
                                foreach (string para in paras)
                                {
                                    string[] ps = para.Split('|');
                                    PowerWay fx = PowerWay.正向有功;
                                    Cus_PowerYuanJian yj = Cus_PowerYuanJian.H;
                                    string xIb = "Imax";
                                    string glys = "1.0";
                                    string demandTime = "15";   //需量周期
                                    string slipeTime = "1";     //滑差时间
                                    if (ps.Length > 8)
                                    {
                                        fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                        yj = GetYuanJianFromString(GetPName("currentPhaseCode", ps[1]));
                                        xIb = GetPName("meter_Test_CurLoad", ps[2]);
                                        glys = GetPName("meterTestPowerFactor", ps[3]);
                                        demandTime = ps[4];
                                        slipeTime = ps[5];
                                    }

                                    xIb = xIb.Replace("In", "Ib");
                                    if (xIb == "Ib") xIb = "1.0Ib";

                                    values = MisScheme.JoinValue(xIb, fx.ToString(), demandTime, slipeTime, "1");
                                    Schema[ID].SchemaNodeValue.Add(values);
                                    //values = MisScheme.JoinValue("1.0Ib", fx.ToString(), demandTime, slipeTime, "1");
                                    //Schema[ID].SchemaNodeValue.Add(values);
                                    //values = MisScheme.JoinValue("0.1Ib", fx.ToString(), demandTime, slipeTime, "1");
                                    //Schema[ID].SchemaNodeValue.Add(values);
                                }
                            }
                            break;
                        #endregion
                        case "误差一致性试验":
                            #region 误差一致性试验
                            ID = ProjectID.误差一致性;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string p in paras)
                            {
                                string[] ps = p.Split('|');  //功率方向|电流倍数|功率因数|下限|电压|误差个数|上限
                                PowerWay fx = PowerWay.正向有功;
                                string glys = "1.0";
                                string xIb = "1.0Ib";
                                if (ps.Length >= 7)
                                {
                                    fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                    xIb = GetPName("meter_Test_CurLoad", ps[1]);
                                    if (xIb == "Ib") xIb = "1.0Ib";
                                    if (xIb == "Itr") xIb = "1Itr";
                                    if (xIb == "10.0Itr") xIb = "10Itr";
                                    glys = GetPName("meterTestPowerFactor", ps[2]);
                                    values = MisScheme.JoinValue(xIb, glys);
                                    Schema[ID].SchemaNodeValue.Add(values);
                                }
                            }
                            #endregion
                            break;
                        case "误差变差试验":
                            #region 变差要求试验
                            ID = ProjectID.误差变差;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string para in paras)
                            {
                                string[] ps = para.Split('|'); //功率方向|电流倍数|功率因数|下限|电压|误差个数|上限
                                PowerWay fx = PowerWay.正向有功;
                                string glys = "1.0";
                                string xIb = "1.0Ib";
                                string time = "";
                                if (ps.Length >= 7)
                                {
                                    fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                    xIb = GetPName("meter_Test_CurLoad", ps[1]);
                                    if (xIb == "Ib") xIb = "1.0Ib";
                                    if (xIb == "Itr") xIb = "1Itr";
                                    if (xIb == "10.0Itr") xIb = "10Itr";
                                    glys = GetPName("meterTestPowerFactor", ps[2]);

                                    time = ps[3];

                                    if (string.IsNullOrWhiteSpace(time))
                                    {
                                        time = "5";
                                    }
                                    else
                                    {
                                        int.TryParse(time, out int min);
                                        time = ((int)(min / 60F)).ToString();
                                    }
                                    values = MisScheme.JoinValue(time, glys);
                                    if (Schema[ID].SchemaNodeValue.Count >= 2)
                                    {
                                        break;
                                    }
                                    Schema[ID].SchemaNodeValue.Add(values);
                                }
                            }
                            #endregion
                            break;
                        case "负载电流升降变差试验":
                            #region 负载电流升降变差试验
                            ID = ProjectID.负载电流升将变差;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            string[] str = new string[paras.Length];
                            if (str.Length < 3) str = new string[3];
                            string tsp = "";
                            int index = 0;
                            foreach (string strWcPara in paras)
                            {
                                string[] ps = strWcPara.Split('|'); //功率方向|电流倍数|电压|功率因数|等待时间|误差个数|上限
                                PowerWay fx = PowerWay.正向有功;
                                string glys = "1.0";
                                string xIb = "1.0Ib";
                                if (ps.Length >= 7)
                                {
                                    fx = GetGLFXFromString(GetPName("powerFlag", ps[0]));
                                    xIb = GetPName("meter_Test_CurLoad", ps[1]);
                                    if (xIb == "Ib") xIb = "1.0Ib";
                                    if (xIb == "Itr") xIb = "1Itr";
                                    if (xIb == "10.0Itr") xIb = "10Itr";
                                    glys = GetPName("meterTestPowerFactor", ps[3]);
                                    tsp = ps[4];
                                    if (string.IsNullOrWhiteSpace(tsp))
                                    {
                                        tsp = "2";
                                    }
                                    else
                                    {
                                        int.TryParse(tsp, out int min);
                                        tsp = ((int)(min / 60F)).ToString();
                                    }
                                    str[index++] = xIb;
                                }
                            }
                            values = MisScheme.JoinValue(str[0], str[1], str[2], tsp);
                            Schema[ID].SchemaNodeValue.Add(values);
                            #endregion
                            break;
                        case "电流过载试验":
                            break;
                        case "安全认证试验":
                            ID = ProjectID.身份认证;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            break;
                        case "远程控制试验":
                            ID = ProjectID.远程控制;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            ID = ProjectID.报警功能;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            ID = ProjectID.远程保电;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            ID = ProjectID.保电解除;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            break;
                        case "报警功能":
                            ID = ProjectID.报警功能;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            Schema[ID].SchemaNodeValue.Add("");
                            break;
                        case "密钥更新试验":
                            ID = ProjectID.密钥更新;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string para in paras)//参数现在不用
                            {
                                string[] par = para.Split('|');
                                string[] mparas = new string[5];
                                mparas[0] = par[0];//密钥条数
                                mparas[1] = par[1];//密钥版本
                                mparas[2] = par[2];//电压
                                mparas[3] = par[3];//密钥类型 01 主控密钥
                                mparas[4] = par[4];//密钥状态 02 正式密钥
                            }
                            Schema[ID].SchemaNodeValue.Add("");
                            break;
                        case "预置参数检查1":
                        case "预置参数检查2":
                            ID = ProjectID.通讯协议检查试验;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());
                            foreach (string para in paras)
                            {
                                string[] ps = para.Split('|');
                                if (ps.Length > 5)
                                {
                                    string name = MisScheme.GetProtocolName(ps[0]);
                                    string opertype = "读";
                                    if (ps[2] != "11")
                                    {
                                        opertype = "写";
                                    }
                                    //int TestIndex = GetTestIndex(name, opertype);   //判断获取他的编号
                                    string datalen = (ps[5].Replace(".", "").Length / 2).ToString();
                                    values = MisScheme.JoinValue(name, ps[1], datalen, "0", ps[3], opertype, ps[5]);
                                    Schema[ID].SchemaNodeValue.Add(values);
                                }
                            }
                            break;
                        case "剩余电量递减准确度":
                            ID = ProjectID.剩余电量递减准确度;
                            if (!Schema.ContainsKey(ID))
                                Schema.Add(ID, new SchemaNode());

                            break;
                        case "时钟示值误差":
                            {
                                ID = ProjectID.时钟示值误差;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                string[] mparas = new string[1];
                                foreach (string para in paras)
                                {
                                    string[] par = para.Split('|');
                                    mparas[0] = par[1];
                                }
                                Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue(mparas));
                            }
                            break;
                        case "预置内容设置":
                            {
                                ID = ProjectID.预置内容设置;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                string[] mparas = new string[5];
                                foreach (string para in paras)
                                {
                                    string[] par = para.Split('|');
                                    mparas[0] = par[0];//预置金额
                                    mparas[1] = par[1];//报警金额1
                                    mparas[2] = par[2];//报警金额2
                                    mparas[3] = par[3] == "1" ? "是" : "否";//电价设置
                                    mparas[4] = par[4] == "1" ? "是" : "否";//时段设置
                                    if (mparas[4] == "是")
                                    {
                                        //查询时段
                                        if (GetRatePeriod(TaskNO, out ratePeriodTable))
                                        {
                                            Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("年时区数", "04000201", "1", "0", "NN", "写", ratePeriodTable.time_zone_num));
                                            Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("日时段表数", "04000202", "1", "0", "NN", "写", ratePeriodTable.daytime_periodtab_num));
                                            Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("日时段数", "04000203", "1", "0", "NN", "写", ratePeriodTable.day_period_num));
                                            //费率数
                                            int rateCount = 4;
                                            bool find = false;
                                            foreach (var item in ratePeriodTable.PeriodTable)
                                            {
                                                foreach (KeyValuePair<string, List<string>> period in item.Value)
                                                {
                                                    foreach (string aPeriod in period.Value)
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(aPeriod)
                                                            && aPeriod.Length == 6
                                                            && aPeriod[4] == '0' && aPeriod[5] == '5')
                                                        {
                                                            rateCount = 5;
                                                            find = true;
                                                            break;
                                                        }
                                                    }
                                                    if (find) break;
                                                }
                                                if (find) break;
                                            }
                                            Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("费率数", "04000204", "1", "0", "NN", "写", rateCount.ToString()));
                                            //时区表
                                            foreach (KeyValuePair<string, List<string>> item in ratePeriodTable.TimeZoneTable)
                                            {
                                                string no = "一";
                                                string flag645 = "";
                                                string writeValue = "";
                                                if (item.Key == "1")
                                                {
                                                    no = "一";
                                                    flag645 = "04010000";
                                                }
                                                else if (item.Key == "2")
                                                {
                                                    no = "二";
                                                    flag645 = "04020000";
                                                }

                                                writeValue = string.Join("", item.Value);

                                                Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue($"第{no}套时区表数据", flag645, (writeValue.Length / 2).ToString(), "0", "MMDDNN...MMDDNN", "写", writeValue));
                                            }
                                            //时段表
                                            foreach (var item in ratePeriodTable.PeriodTable)
                                            {
                                                string no = "一";
                                                string flag645 = "";
                                                string writeValue = "";
                                                if (item.Key == "1")
                                                {
                                                    no = "一";
                                                    flag645 = "0401000";
                                                }
                                                else if (item.Key == "2")
                                                {
                                                    no = "二";
                                                    flag645 = "0402000";
                                                }


                                                foreach (KeyValuePair<string, List<string>> period in item.Value)
                                                {
                                                    writeValue = string.Join("", period.Value);
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue($"第{no}套第{period.Key}日时段数据", $"{flag645}{period.Key}", (writeValue.Length / 2).ToString(), "0", "hhmmNN...hhmmNN", "写", writeValue));
                                                }
                                            }

                                            //写屏显
                                            //单相屏显5费率
                                            if (rateCount == 5)
                                            {
                                                if (IoT_Meter)
                                                {
                                                    #region 2023-09-21 物联版
                                                    //Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示屏数", "04000301", "1", "0", "NN", "写", "1"));
                                                    //Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第1屏", "04040101", "5", "0", "NNNNNNNNNN", "写", "00000201,00"));

                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键显示屏数", "04000305", "5", "0", "NN", "写", "10"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第1屏", "04040201", "5", "0", "NNNNNNNNNN", "写", "00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第2屏", "04040202", "5", "0", "NNNNNNNNNN", "写", "50050201,00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第3屏", "04040203", "5", "0", "NNNNNNNNNN", "写", "40020200,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第4屏", "04040204", "5", "0", "NNNNNNNNNN", "写", "40000200,01"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第5屏", "04040205", "5", "0", "NNNNNNNNNN", "写", "40000200,02"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第6屏", "04040206", "5", "0", "NNNNNNNNNN", "写", "20000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第7屏", "04040207", "5", "0", "NNNNNNNNNN", "写", "20010201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第8屏", "04040208", "5", "0", "NNNNNNNNNN", "写", "20040201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第9屏", "04040209", "5", "0", "NNNNNNNNNN", "写", "200A0201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第10屏", "0404020A", "5", "0", "NNNNNNNNNN", "写", "F20B0500,00"));

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region 2023-9-7第1版
                                                    /*
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示屏数", "04000301", "1", "0", "NN", "写", "6"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第1屏", "04040101", "5", "0", "NNNNNNNNNN", "写", "00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第2屏", "04040102", "5", "0", "NNNNNNNNNN", "写", "00000202,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第3屏", "04040103", "5", "0", "NNNNNNNNNN", "写", "00000203,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第4屏", "04040104", "5", "0", "NNNNNNNNNN", "写", "00000204,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第5屏", "04040105", "5", "0", "NNNNNNNNNN", "写", "00000205,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第6屏", "04040106", "5", "0", "NNNNNNNNNN", "写", "00000206,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键显示屏数", "04000305", "5", "0", "NN", "写", "26"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第1屏", "04040201", "5", "0", "NNNNNNNNNN", "写", "00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第2屏", "04040202", "5", "0", "NNNNNNNNNN", "写", "00000202,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第3屏", "04040203", "5", "0", "NNNNNNNNNN", "写", "00000203,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第4屏", "04040204", "5", "0", "NNNNNNNNNN", "写", "00000204,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第5屏", "04040205", "5", "0", "NNNNNNNNNN", "写", "00000205,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第6屏", "04040206", "5", "0", "NNNNNNNNNN", "写", "00000206,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第7屏", "04040207", "5", "0", "NNNNNNNNNN", "写", "50050201,00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第8屏", "04040208", "5", "0", "NNNNNNNNNN", "写", "50050201,00000202,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第9屏", "04040209", "5", "0", "NNNNNNNNNN", "写", "50050201,00000203,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第10屏", "0404020A", "5", "0", "NNNNNNNNNN", "写", "50050201,00000204,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第11屏", "0404020B", "5", "0", "NNNNNNNNNN", "写", "50050201,00000205,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第12屏", "0404020C", "5", "0", "NNNNNNNNNN", "写", "50050201,00000206,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第13屏", "0404020D", "5", "0", "NNNNNNNNNN", "写", "50050202,00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第14屏", "0404020E", "5", "0", "NNNNNNNNNN", "写", "50050202,00000202,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第15屏", "0404020F", "5", "0", "NNNNNNNNNN", "写", "50050202,00000203,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第16屏", "04040210", "5", "0", "NNNNNNNNNN", "写", "50050202,00000204,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第17屏", "04040211", "5", "0", "NNNNNNNNNN", "写", "50050202,00000205,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第18屏", "04040212", "5", "0", "NNNNNNNNNN", "写", "50050202,00000206,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第19屏", "04040213", "5", "0", "NNNNNNNNNN", "写", "40020200,02"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第20屏", "04040214", "5", "0", "NNNNNNNNNN", "写", "40020200,01"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第21屏", "04040215", "5", "0", "NNNNNNNNNN", "写", "40000200,01"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第22屏", "04040216", "5", "0", "NNNNNNNNNN", "写", "40000200,02"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第23屏", "04040217", "5", "0", "NNNNNNNNNN", "写", "20000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第24屏", "04040218", "5", "0", "NNNNNNNNNN", "写", "20010201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第25屏", "04040219", "5", "0", "NNNNNNNNNN", "写", "20040201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第26屏", "0404021A", "5", "0", "NNNNNNNNNN", "写", "200A0201,00"));
                                                    */
                                                    #endregion 2023-9-7第1版

                                                    #region 2023-9-15 第2版

                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示屏数", "04000301", "1", "0", "NN", "写", "1"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("自动循环显示第1屏", "04040101", "5", "0", "NNNNNNNNNN", "写", "00000201,00"));

                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键显示屏数", "04000305", "5", "0", "NN", "写", "11"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第1屏", "04040201", "5", "0", "NNNNNNNNNN", "写", "00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第2屏", "04040202", "5", "0", "NNNNNNNNNN", "写", "50050201,00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第3屏", "04040203", "5", "0", "NNNNNNNNNN", "写", "50050202,00000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第4屏", "04040204", "5", "0", "NNNNNNNNNN", "写", "40020200,02"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第5屏", "04040205", "5", "0", "NNNNNNNNNN", "写", "40020200,01"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第6屏", "04040206", "5", "0", "NNNNNNNNNN", "写", "40000200,01"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第7屏", "04040207", "5", "0", "NNNNNNNNNN", "写", "40000200,02"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第8屏", "04040208", "5", "0", "NNNNNNNNNN", "写", "20000201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第9屏", "04040209", "5", "0", "NNNNNNNNNN", "写", "20010201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第10屏", "0404020A", "5", "0", "NNNNNNNNNN", "写", "20040201,00"));
                                                    Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("按键循环显示第11屏", "0404020B", "5", "0", "NNNNNNNNNN", "写", "200A0201,00"));

                                                    #endregion 2023-9-15 第2版
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            break;
                        case "预置内容检查":
                            {
                                ID = ProjectID.预置内容检查;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                string[] mparas = new string[5];
                                foreach (string para in paras)
                                {
                                    string[] par = para.Split('|');
                                    mparas[0] = par[0] == "1" ? "是" : "否";//实际金额
                                    mparas[1] = par[1] == "1" ? "是" : "否";//预置报警金额1
                                    mparas[2] = par[2] == "1" ? "是" : "否";//预置报警金额2
                                    mparas[3] = par[3] == "1" ? "是" : "否";//电价读取
                                    mparas[4] = par[4] == "1" ? "是" : "否";//时段读取
                                    if (mparas[4] == "是")
                                    {
                                        if (ratePeriodTable == null)
                                        {
                                            if (GetRatePeriod(TaskNO, out ratePeriodTable))
                                            {

                                            }
                                        }
                                        Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("年时区数", "04000201", "1", "0", "NN", "读", ratePeriodTable.time_zone_num));
                                        Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("日时段表数", "04000202", "1", "0", "NN", "读", ratePeriodTable.daytime_periodtab_num));
                                        Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue("日时段数", "04000203", "1", "0", "NN", "读", ratePeriodTable.day_period_num));
                                        foreach (KeyValuePair<string, List<string>> item in ratePeriodTable.TimeZoneTable)
                                        {
                                            string no = "一";
                                            string flag645 = "";
                                            string writeValue = "";
                                            if (item.Key == "1")
                                            {
                                                no = "一";
                                                flag645 = "04010000";
                                            }
                                            else if (item.Key == "2")
                                            {
                                                no = "二";
                                                flag645 = "04020000";
                                            }

                                            writeValue = string.Join("", item.Value);

                                            Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue($"第{no}套时区表数据", flag645, (writeValue.Length / 2).ToString(), "0", "MMDDNN...MMDDNN", "读", writeValue));
                                        }

                                        foreach (var item in ratePeriodTable.PeriodTable)
                                        {
                                            string no = "一";
                                            string flag645 = "";
                                            string writeValue = "";
                                            if (item.Key == "1")
                                            {
                                                no = "一";
                                                flag645 = "0401000";
                                            }
                                            else if (item.Key == "2")
                                            {
                                                no = "二";
                                                flag645 = "0402000";
                                            }

                                            foreach (KeyValuePair<string, List<string>> period in item.Value)
                                            {
                                                writeValue = string.Join("", period.Value);
                                                Schema[ID].SchemaNodeValue.Add(MisScheme.JoinValue($"第{no}套第{period.Key}日时段数据", $"{flag645}{period.Key}", (writeValue.Length / 2).ToString(), "0", "hhmmNN...hhmmNN", "读", writeValue));
                                            }
                                        }

                                    }
                                }
                            }
                            break;
                        case "显示功能":
                            {
                                ID = ProjectID.显示功能;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                List<string> allparas = new List<string>();
                                foreach (string para in paras)
                                {
                                    string[] par = para.Split('|');
                                    string[] mparas = new string[6];

                                    mparas[0] = par[0];
                                    mparas[1] = par[1];
                                    mparas[2] = par[1];
                                    allparas.Add(MisScheme.JoinValue(mparas));
                                }
                                Schema[ID].SchemaNodeValue.Add(string.Join(",", allparas));
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            #endregion 方案详细及参数

            return true;
        }

        private bool GetRatePeriod(string taskno, out RatePeriodTable ratePeriodTable)
        {
            ratePeriodTable = new RatePeriodTable();

            string sql = $"select t.equip_code_new from MT_DETECT_TASK t where t.detect_task_no = '{taskno}' order by t.write_date desc";
            object obj = ExecuteScalar(sql);
            if (obj != null)
            {
                string equip_code_new = obj.ToString().Trim();
                sql = $"select b.time_zone_code,b.creator_no,b.create_date,b.flag from B_Elec_Price_Time_Zone b where b.extend_elec_code='{equip_code_new}'";//89999993
                obj = ExecuteScalar(sql);
                if (obj != null)
                {
                    string time_zone_code = obj.ToString().Trim();
                    sql = $"select b.day_period_num,b.daytime_periodtab_num,b.time_zone_num from B_Time_Zone b where b.time_zone_code='{time_zone_code}'";//654202111110001
                    DataTable table = ExecuteReader(sql);
                    if (table.Rows.Count > 0)
                    {
                        ratePeriodTable.day_period_num = table.Rows[0]["day_period_num"].ToString().Trim();
                        ratePeriodTable.daytime_periodtab_num = table.Rows[0]["daytime_periodtab_num"].ToString().Trim();
                        ratePeriodTable.time_zone_num = table.Rows[0]["time_zone_num"].ToString().Trim();
                    }

                    sql = $"select * from b_time_zone_detail t where t.time_zone_code='{time_zone_code}' order by t.time_zone_formula_id ,t.time_zone_order asc";//654202111110001
                    table = ExecuteReader(sql);
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        string key = table.Rows[i]["Time_Zone_Formula_ID"].ToString().Trim();
                        string start_time = table.Rows[i]["Start_Time"].ToString().Trim();
                        string period_id = table.Rows[i]["Dperiod_ID"].ToString().Trim();

                        if (!ratePeriodTable.TimeZoneTable.ContainsKey(key))
                        {
                            ratePeriodTable.TimeZoneTable.Add(key, new List<string>());
                        }

                        ratePeriodTable.TimeZoneTable[key].Add($"{start_time}{int.Parse(period_id):D2}");
                    }


                    sql = $"select * from B_Day_Time_Period_Detail t where t.time_zone_code='{time_zone_code}' order by t.time_zone_formula_id,t.dperiod_id,t.period_order";//654202111110001
                    table = ExecuteReader(sql);
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        string key = table.Rows[i]["Time_Zone_Formula_ID"].ToString().Trim();
                        string start_time = table.Rows[i]["Start_Time"].ToString().Trim();
                        string period_id = table.Rows[i]["Dperiod_ID"].ToString().Trim();
                        string period_code = table.Rows[i]["Period_Code"].ToString().Trim();

                        if (!ratePeriodTable.PeriodTable.ContainsKey(key))
                        {
                            ratePeriodTable.PeriodTable.Add(key, new Dictionary<string, List<string>>());
                        }

                        if (!ratePeriodTable.PeriodTable[key].ContainsKey(period_id))
                        {
                            ratePeriodTable.PeriodTable[key].Add(period_id, new List<string>());
                        }

                        ratePeriodTable.PeriodTable[key][period_id].Add($"{start_time}{int.Parse(period_code):D2}");

                    }
                    return true;
                }
            }
            return false;
        }

        public void UpdateInit()
        {

        }

        private MT_METER GetMeter(string barcode)
        {
            string sql = string.Format(@"SELECT * FROM mt_detect_out_equip t1 INNER JOIN mt_meter t2 ON t1.bar_code=t2.bar_code WHERE t2.bar_code='{0}' ORDER BY t1.write_date DESC", barcode.Trim());
            DataTable dr = ExecuteReader(sql);
            if (dr.Rows.Count <= 0) return null;

            DataRow row = dr.Rows[0];

            MT_METER meter = new MT_METER();
            meter.METER_ID = (row["METER_ID"].ToString() != "") ? row["METER_ID"].ToString() : meter.METER_ID;
            meter.BAR_CODE = row["BAR_CODE"].ToString();
            meter.LOT_NO = row["LOT_NO"].ToString();
            meter.ASSET_NO = row["ASSET_NO"].ToString();
            meter.MADE_NO = row["MADE_NO"].ToString();
            meter.SORT_CODE = row["SORT_CODE"].ToString();
            meter.TYPE_CODE = row["TYPE_CODE"].ToString();
            meter.MODEL_CODE = row["MODEL_CODE"].ToString();
            meter.WIRING_MODE = row["WIRING_MODE"].ToString();
            meter.VOLT_CODE = row["VOLT_CODE"].ToString();
            meter.OVERLOAD_FACTOR = row["OVERLOAD_FACTOR"].ToString();
            meter.AP_PRE_LEVEL_CODE = row["AP_PRE_LEVEL_CODE"].ToString();
            meter.CONST_CODE = row["CONST_CODE"].ToString();
            meter.RP_CONSTANT = row["RP_CONSTANT"].ToString();
            meter.PULSE_CONSTANT_CODE = row["PULSE_CONSTANT_CODE"].ToString();
            meter.FREQ_CODE = row["FREQ_CODE"].ToString();
            meter.RATED_CURRENT = row["RATED_CURRENT"].ToString();
            meter.CON_MODE = row["CON_MODE"].ToString();
            meter.SOFT_VER = row["SOFT_VER"].ToString();
            meter.HARD_VER = row["HARD_VER"].ToString();

            meter.DETECT_OUT_EQUIP.SYS_NO = row["SYS_NO"].ToString();
            meter.DETECT_OUT_EQUIP.DETECT_TASK_NO = row["DETECT_TASK_NO"].ToString();
            return meter;
        }


        #region 各检测顶数据上传

        /// <summary>
        /// 外观检查试验数据上传
        /// </summary>
        protected string GetMT_INTUIT_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            MT_INTUIT_MET_CONC entity = new MT_INTUIT_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                DETECT_CONTENT = "01",
                CONC_CODE = "01",
                //CONC_CODE = meter.MeterResults[ProjectID.外观检查试验].Result == ConstHelper.合格 ? "01" : "02",
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",
            };

            return entity.ToInsertString();
        }

        /// <summary>
        /// 启动
        /// </summary>
        protected string[] GetMT_STARTING_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            int iRecordCount = 0;
            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;
            foreach (string key in meterQdQids.Keys)
            {
                if (key.Split('_')[0] != ProjectID.起动试验) continue;
                iRecordCount++;
            }
            string[] sql = new string[iRecordCount];

            int index = 0;
            foreach (string key in meterQdQids.Keys)
            {
                if (key.Split('_')[0] == ProjectID.起动试验)           //只有大于3才可能是小项目,并且当中要包含启动ID和潜动ID
                {
                    float.TryParse(meterQdQids[key].Current, out float curtmp);
                    string current = curtmp.ToString("F3");// + "Ib";
                    string Ibstr = Number.GetCurrentBase(meter.MD_UA, out float IbItr);
                    float loadc;
                    if (IbItr == 0)
                        loadc = 0.05F;
                    else
                        loadc = curtmp / IbItr;
                    string loadcstr = "";
                    if (meter.MD_JJGC == "IR46")
                    {
                        if (Ibstr == "Itr")
                            loadcstr = loadc + Ibstr;
                        else
                            loadcstr = loadc * 10 + "Itr";
                    }
                    else
                    {
                        if (Ibstr == "Ib")
                            loadcstr = loadc + Ibstr;
                        else
                            loadcstr = loadc / 10 + "Ib";
                    }

                    MT_STARTING_MET_CONC entity = new MT_STARTING_MET_CONC
                    {
                        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                        EQUIP_CATEG = "01",
                        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                        DETECT_EQUIP_NO = meter.BenthNo,
                        DETECT_UNIT_NO = string.Empty,
                        POSITION_NO = meter.MD_Epitope.ToString(),
                        BAR_CODE = mtMeter.BAR_CODE,
                        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                        PARA_INDEX = (index + 1).ToString(),
                        DETECT_ITEM_POINT = (index + 1).ToString(),
                        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        HANDLE_FLAG = "0",
                        HANDLE_DATE = "",
                        CONC_CODE = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",

                        IS_VALID = "1",
                        LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),
                        PULES = "1",
                        BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meter.MeterQdQids[key].PowerWay),
                        //LOAD_CURRENT = GetPCode("meter_Test_CurLoad",current),
                        LOAD_CURRENT = GetPCode("meter_Test_CurLoad", loadcstr),

                        START_CURRENT = GetPCode("meter_Test_CurLoad", loadcstr),//current,
                    };

                    if (!string.IsNullOrEmpty(meterQdQids[key].StandartTime))
                    {
                        float.TryParse(meterQdQids[key].StandartTime, out float timetmp);
                        entity.TEST_TIME = (timetmp * 60).ToString("F0");
                    }
                    if (!string.IsNullOrEmpty(meterQdQids[key].ActiveTime))
                    {
                        float.TryParse(meterQdQids[key].ActiveTime, out float actmtmp);
                        entity.REAL_TEST_TIME = (actmtmp * 60).ToString("F0");
                    }
                    //entity.START_CURRENT = current;
                    //entity.LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "1.0Ib");

                    sql[index++] = entity.ToInsertString();
                }
            }
            return sql;
        }

        /// <summary>
        /// 潜动
        /// </summary>
        protected string[] GetMT_CREEPING_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            int index = 1;
            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;

            List<string> sql = new List<string>();
            foreach (string key in meterQdQids.Keys)
            {
                if (key.Split('_')[0] == ProjectID.潜动试验)
                {
                    MT_CREEPING_MET_CONC entity = new MT_CREEPING_MET_CONC
                    {
                        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                        EQUIP_CATEG = "01",
                        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                        DETECT_EQUIP_NO = meter.BenthNo,
                        DETECT_UNIT_NO = string.Empty,
                        POSITION_NO = meter.MD_Epitope.ToString(),
                        BAR_CODE = mtMeter.BAR_CODE,
                        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                        PARA_INDEX = index.ToString(),
                        DETECT_ITEM_POINT = index.ToString(),
                        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        HANDLE_FLAG = "0",
                        HANDLE_DATE = "",
                        CONC_CODE = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",

                        IS_VALID = "1",
                        //LOAD_VOLTAGE = GetPCode("meterTestVolt", "115%Un"),//潜动默认115%
                        PULES = "0",
                        BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meter.MeterQdQids[key].PowerWay),
                        LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "0"),
                        //TEST_TIME = (Convert.ToSingle(meterQdQids[key].ActiveTime) * 60).ToString("F0"),
                        //REAL_TEST_TIME = (Convert.ToSingle(meterQdQids[key].StandartTime) * 60).ToString("F0"),
                    };

                    float.TryParse(meterQdQids[key].Voltage, out float vtmp);
                    string vRate = (vtmp / meter.MD_UB * 100).ToString("F0");
                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", $"{vRate}%Un");

                    if (!string.IsNullOrEmpty(meterQdQids[key].ActiveTime))
                    {
                        float.TryParse(meterQdQids[key].ActiveTime, out float actimetmp);
                        entity.TEST_TIME = (actimetmp * 60).ToString("F0");
                    }
                    if (!string.IsNullOrEmpty(meterQdQids[key].StandartTime))
                    {
                        float.TryParse(meterQdQids[key].StandartTime, out float timetmp);
                        entity.REAL_TEST_TIME = (timetmp * 60).ToString("F0");
                    }
                    sql.Add(entity.ToInsertString());
                    index++;
                }
            }
            return sql.ToArray();
        }

        /// <summary>
        /// 校核常数
        /// </summary>
        protected string[] GetMT_CONST_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            //新疆计量中心需求，4个方向*（总+4分）最多20条
            //深谷独立编号：96有功深谷，97无功深谷，98有功反向深谷，99无功反向深谷
            Dictionary<string, MeterZZError> needUpCode = new Dictionary<string, MeterZZError>();
            string[] read_type_power_codes = new string[] { "1", "4", "2", "5" };
            string[] read_type_power_Name = new string[] { "正向有功", "反向有功", "正向无功", "反向无功" };
            bool[] read_type_power_up = new bool[] { false, false, false, false };
            string[] read_type_fee_codes = new string[] { "1", "2", "3", "5", "4", "6" };//总，尖峰，峰，平，谷
            string[] read_type_fee_name = new string[] { "总", "尖", "峰", "平", "谷", "深谷" };
            for (int i_r_t = 0; i_r_t < read_type_power_codes.Length; i_r_t++)
            {
                KeyValuePair<string, MeterZZError> item = meter.MeterZZErrors.FirstOrDefault(v => v.Value != null && v.Value.PowerWay.Equals(read_type_power_Name[i_r_t])
                                                          );
                if (!string.IsNullOrWhiteSpace(item.Key))
                {
                    read_type_power_up[i_r_t] = true;
                    for (int i_f = 0; i_f < read_type_fee_codes.Length; i_f++)
                    {
                        int id = (i_r_t * 5) + i_f;
                        item = meter.MeterZZErrors.FirstOrDefault(v => v.Value != null && v.Value.PowerWay.Equals(read_type_power_Name[i_r_t]) && v.Value.Fl.Equals(read_type_fee_name[i_f])
                               );
                        if (!string.IsNullOrWhiteSpace(item.Key))
                        {
                            needUpCode.Add(read_type_power_codes[i_r_t] + read_type_fee_codes[i_f], item.Value);
                        }
                        else
                        {
                            needUpCode.Add(read_type_power_codes[i_r_t] + read_type_fee_codes[i_f], null);
                        }
                    }
                }
            }
            for (int i_r_t = 0; i_r_t < read_type_power_codes.Length; i_r_t++)
            {
                if (read_type_power_up[i_r_t])//该方向有数据
                {
                    if (needUpCode[read_type_power_codes[i_r_t] + "1"] == null)//没有总时
                    {
                        MeterZZError total = new MeterZZError()
                        {
                            PowerWay = read_type_power_Name[i_r_t],
                            Fl = read_type_fee_name[0],
                            GLYS = "1.0"
                        };
                        float _STMEnergy = 0;
                        float _ErrorValue = 0;
                        float _WarkPower = 0;
                        float _PowerError = 0;
                        float _Pules = 0;
                        float _NeedEnergy = 0;
                        bool Tresult = true;
                        for (int i_f = 1; i_f < read_type_fee_codes.Length; i_f++)
                        {
                            string id = read_type_power_codes[i_r_t] + read_type_fee_codes[i_f];
                            if (needUpCode[id] != null)
                            {
                                total.PowerStart += needUpCode[id].PowerStart;
                                total.PowerEnd += needUpCode[id].PowerEnd;
                                float.TryParse(needUpCode[id].STMEnergy, out float _sr1);
                                float.TryParse(needUpCode[id].NeedEnergy, out float _nd1);
                                float.TryParse(needUpCode[id].Pules, out float _pls);
                                float _sr1_e = _sr1 / meter.GetBcs()[0];
                                if (read_type_power_Name[i_r_t].IndexOf("无功") >= 0) _sr1_e = _sr1 / meter.GetBcs()[1];
                                if (_nd1 > 0 && _sr1_e < _nd1 && Math.Abs((_sr1_e - _nd1) / _nd1) > 0.4)
                                {
                                    _sr1 *= 2;
                                    _pls *= 2;
                                }
                                _STMEnergy += _sr1;
                                float.TryParse(needUpCode[id].ErrorValue, out float _tmp);
                                _ErrorValue += _tmp;
                                float.TryParse(needUpCode[id].WarkPower, out _tmp);
                                _WarkPower += _tmp;
                                float.TryParse(needUpCode[id].PowerError, out _tmp);
                                _PowerError += _tmp;
                                _Pules += _pls;

                                _NeedEnergy += _nd1;

                                if (needUpCode[id].Result.Equals("不合格")) Tresult = false;

                                total.IbX = needUpCode[id].IbX;
                                total.IbXString = needUpCode[id].IbXString;
                                total.TestWay = needUpCode[id].TestWay;
                            }
                            else//补全分费率
                            {
                                needUpCode[id] = new MeterZZError()
                                {
                                    PowerWay = read_type_power_Name[i_r_t],
                                    Fl = read_type_fee_name[i_f],
                                    GLYS = "1.0",
                                    PowerStart = 0,
                                    PowerEnd = 0,
                                    IbX = "Imax",
                                    IbXString = "Imax",
                                    TestWay = "标准表法",
                                    STMEnergy = "0",
                                    ErrorValue = "0",
                                    WarkPower = "0",
                                    PowerError = "0",
                                    Pules = "0",
                                    NeedEnergy = "0",
                                    Result = "合格"
                                };
                            }
                        }
                        total.PowerStart = Math.Round(total.PowerStart.Value, 4);
                        total.PowerEnd = Math.Round(total.PowerEnd.Value, 4);
                        total.STMEnergy = _STMEnergy.ToString();//用_PowerError//因为_STMEnergy计的脉冲
                        total.ErrorValue = _ErrorValue.ToString();
                        total.WarkPower = _WarkPower.ToString("F4");
                        total.PowerError = _PowerError.ToString();
                        total.Pules = _Pules.ToString();
                        total.NeedEnergy = _NeedEnergy.ToString();
                        total.Result = Tresult ? "合格" : "不合格";

                        needUpCode[read_type_power_codes[i_r_t] + "1"] = total;
                    }
                }
            }
            string[] sql = new string[needUpCode.Count];

            string[] levs = Core.Function.Number.GetDj(meter.MD_Grane);

            if (levs[0] == "A" || levs[0] == "B")
            {
                levs[0] = "1.0";
            }
            else if (levs[0] == "C")
            {
                levs[0] = "0.5";
            }
            else if (levs[0] == "D")
            {
                levs[0] = "0.2";
            }

            for (int i = 0; i < needUpCode.Count; i++)
            {
                MeterZZError meterError = needUpCode.ElementAt(i).Value;
                if (meterError == null) continue;

                string current = GetPCode("meter_Test_CurLoad", meterError.IbX);

                //字段长8
                string qualified_pules = meterError.STMEnergy.Trim();
                if (qualified_pules.Length > 8)
                {
                    qualified_pules = qualified_pules.Substring(0, 8);
                }

                MT_CONST_MET_CONC entity = new MT_CONST_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    DETECT_ITEM_POINT = (i + 1).ToString(),
                    PARA_INDEX = (i + 1).ToString(),
                    CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02",
                    WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",

                    IS_VALID = "1",
                    VOLT = GetPCode("meterTestVolt", "100%Un"),
                    LOAD_CURRENT = current,
                    DIFF_READING = meterError.WarkPower.ToString().Trim(),
                    PF = GetPCode("meterTestPowerFactor", meterError.GLYS.Trim()),
                    FEE_RATIO = meterError.Fl.Trim(),
                    START_READING = meterError.PowerStart.ToString(),
                    END_READING = meterError.PowerEnd.ToString(),
                    ERROR = meterError.ErrorValue.Length > 5 ? meterError.ErrorValue.Substring(0, 5) : meterError.ErrorValue,
                    STANDARD_READING = Convert.ToSingle(meterError.PowerError).ToString("F4"),//(Convert.ToSingle(meterError.STMEnergy) / meter.GetBcs()[0]).ToString("F4"),//用_PowerError//因为_STMEnergy记的脉冲
                    REAL_PULES = meterError.Pules.Trim(),

                    QUALIFIED_PULES = qualified_pules,
                    ERR_UP = (Convert.ToDouble(levs[0]) * 1.0).ToString("0.0"),
                    ERR_DOWN = (Convert.ToDouble(levs[0]) * (-1.0)).ToString("0.0"),
                    CONST_ERR = "0",
                    READ_TYPE_CODE = ConvertREAD_TYPE_CODE(needUpCode.ElementAt(i).Key),
                };

                if (meterError.PowerWay.IndexOf("无功") >= 0)
                {
                    entity.ERR_UP = (Convert.ToDouble(levs[1]) * 1.0).ToString("0.0");
                    entity.ERR_DOWN = (Convert.ToDouble(levs[1]) * (-1.0)).ToString("0.0");
                }

                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meterError.PowerWay);
                entity.CONTROL_METHOD = GetPCode("meterTestCtrlMode", meterError.TestWay);
                entity.VOLT = "100%Un";

                //以下为新加入
                //entity.FEE_START_TIME = meterError.TimeStart.ToString();//费率起始时间
                entity.DIVIDE_ELECTRIC_QUANTITY = "";//总分电量值
                entity.IR_LAST_READING = "0.00";//装拆起始度数

                sql[i] = entity.ToInsertString();
            }

            return sql;
        }
        /// <summary>
        /// 深谷独立编号：96有功深谷，97无功深谷，98有功反向深谷，99无功反向深谷
        ///  {"1", "4", "2", "5" };
        ///  { "正向有功", "反向有功", "正向无功", "反向无功" };
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string ConvertREAD_TYPE_CODE(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length < 2) return key;

            if (key[1] == '6')
            {
                if (key[0] == '1') return "96";
                if (key[0] == '4') return "98";
                if (key[0] == '2') return "97";
                if (key[0] == '5') return "99";
            }
            return key;
        }

        ///// <summary>
        ///// 校核常数(走字) 新
        ///// </summary>
        //private string[] GetMT_CONST_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    List<string> sql = new List<string>();
        //    int index = 0;
        //    foreach (string key in meter.MeterZZErrors.Keys)
        //    {
        //        MeterZZError meterError = meter.MeterZZErrors[key];
        //        string current = GetPCode("meter_Test_CurLoad", meterError.IbX);
        //        string[] levs = Core.Function.Number.GetDj(meter.MD_Grane);


        //        string str = meterError.WarkPower.ToString().Trim();
        //        MT_CONST_MET_CONC entity = new MT_CONST_MET_CONC
        //        {
        //            //DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            ////EQUIP_CATEG = "01",
        //            //SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //            //DETECT_EQUIP_NO = meter.BenthNo,
        //            ////DETECT_UNIT_NO = string.Empty,
        //            ////POSITION_NO = meter.MD_Epitope.ToString(),
        //            //BAR_CODE = mtMeter.BAR_CODE,
        //            ////DETECT_DATE = meter.VerifyDate.ToString(),
        //            ////DETECT_ITEM_POINT = (index + 1).ToString(),
        //            //PARA_INDEX = (index + 1).ToString(),
        //            ////CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02",
        //            ////WRITE_DATE = DateTime.Now.ToString(),
        //            ////HANDLE_FLAG = "0",

        //            DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            EQUIP_CATEG = "01",
        //            SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //            DETECT_EQUIP_NO = meter.BenthNo,
        //            DETECT_UNIT_NO = string.Empty,
        //            POSITION_NO = meter.MD_Epitope.ToString(),
        //            BAR_CODE = mtMeter.BAR_CODE,
        //            DETECT_DATE = meter.VerifyDate.ToString(),
        //            PARA_INDEX = (index + 1).ToString(),
        //            DETECT_ITEM_POINT = (index + 1).ToString(),
        //            WRITE_DATE = DateTime.Now.ToString(),
        //            HANDLE_FLAG = "0",
        //            CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02",

        //            IS_VALID = "1",
        //            VOLT = GetPCode("meterTestVolt", "100%Un"),
        //            LOAD_CURRENT = current,
        //            DIFF_READING = meterError.WarkPower.ToString().Trim(),
        //            PF = GetPCode("meterTestPowerFactor", meterError.GLYS.Trim()),
        //            START_READING = meterError.PowerSumEnd.ToString(),
        //            END_READING = meterError.PowerSumStart.ToString(),
        //            ERROR = meterError.PowerError.Trim(),
        //            STANDARD_READING = (Convert.ToSingle(meterError.STMEnergy) / meter.GetBcs()[0]).ToString("F4"),
        //            REAL_PULES = meterError.Pules.Trim(),
        //            QUALIFIED_PULES = meterError.STMEnergy.Trim(),
        //            ERR_UP = "1.0",
        //            ERR_DOWN = "-1.0",
        //            CONST_ERR = meterError.PowerError.Trim(),
        //            READ_TYPE_CODE = "",
        //        };


        //        //entity.START_READING = meterError.PowerSumEnd.ToString();
        //        // entity.END_READING = meterError.PowerSumStart.ToString();
        //        //PowerError STMEnergy
        //        //sql.Add(entity.ToInsertString());
        //        entity.FEE_RATIO = meterError.Fl.Trim();      //1：尖，2：峰，3：平，4：谷，5：总
        //        //switch (meterError.Fl.Trim())
        //        //{
        //        //    case "尖":
        //        //        entity.FEE_RATIO = "1";
        //        //        break;
        //        //    case "峰":
        //        //        entity.FEE_RATIO = "2";
        //        //        break;
        //        //    case "平":
        //        //        entity.FEE_RATIO = "3";
        //        //        break;
        //        //    case "谷":
        //        //        entity.FEE_RATIO = "4";
        //        //        break;
        //        //    case "总":
        //        //        entity.FEE_RATIO = "5";
        //        //        break;
        //        //    default:
        //        //        break;
        //        //}

        //        //entity.CONC_CODE = meterError.Result.Trim() == ConstHelper.合格 ? "01" : "02";
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", meterError.PowerWay);
        //        //entity.ERROR = meterError.PowerError.Trim();
        //        //entity.STANDARD_READING = (Convert.ToSingle(meterError.STMEnergy) / meter.GetBcs()[0]).ToString("F4");
        //        //entity.REAL_PULES = meterError.Pules.Trim();
        //        //entity.QUALIFIED_PULES = meterError.STMEnergy.Trim();
        //        //entity.ERR_UP = "2";
        //        //entity.ERR_DOWN = "-2";
        //        //entity.CONST_ERR = meterError.PowerError.Trim();
        //        entity.CONTROL_METHOD = GetPCode("meterTestCtrlMode", meterError.TestWay);
        //        sql.Add(entity.ToInsertString());
        //        index++;
        //    }
        //    return sql.ToArray();
        //}

        /// <summary>
        /// 基本误差数据
        /// </summary>
        protected string[] GetMT_BASICERR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            string[] keys = new string[meter.MeterErrors.Keys.Count];
            meter.MeterErrors.Keys.CopyTo(keys, 0);
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (!key.StartsWith("12001")) continue;
                //if (key.Substring(0, 1) == "2") continue;

                MeterError meterErr = meter.MeterErrors[key];

                string[] wc = meterErr.WCData.Split('|');
                if (wc.Length < 2) continue;

                //string tmpHi = "";

                MT_BASICERR_MET_CONC entity = new MT_BASICERR_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    PARA_INDEX = (i + 1).ToString(),
                    DETECT_ITEM_POINT = (i + 1).ToString(),
                    IS_VALID = "1"
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

                entity.LOAD_CURRENT = GetPCode("meter_Test_CurLoad", meterErr.IbX);
                entity.FREQ = GetPCode("meter_Test_Freq", "50");
                entity.PF = GetPCode("meterTestPowerFactor", meterErr.GLYS.Trim());
                entity.DETECT_CIRCLE = "2";
                entity.SIMPLING = "2";

                //原始值
                entity.ERROR = meterErr.WCData;
                //if (string.IsNullOrWhiteSpace(entity.ERROR) || entity.ERROR == "|")
                //{
                //    entity.ERROR = "999.99|999.99";
                //}

                try
                {
                    if (meterErr.WCValue == null || meterErr.WCValue.Trim() == "" || meterErr.WCHZ == null || meterErr.WCHZ.Trim() == "")
                    {
                        entity.AVE_ERR = "999.99";  //平均值
                        entity.INT_CONVERT_ERR = "999.99";  //化整值
                    }
                    else
                    {
                        entity.AVE_ERR = meterErr.WCValue;    //平均值
                        entity.INT_CONVERT_ERR = meterErr.WCHZ;  //化整值
                    }

                    if (entity.AVE_ERR.Length > 8)
                    {
                        entity.AVE_ERR = entity.AVE_ERR.Substring(0, 8);
                    }
                }
                catch (Exception)
                {
                }

                if (meterErr.Limit != null)
                {
                    if (meterErr.Limit.IndexOf('|') > 0)
                    {
                        entity.ERR_UP = meterErr.Limit.Trim().Split('|')[0];   //误差上限
                        entity.ERR_DOWN = meterErr.Limit.Trim().Split('|')[1];

                    }
                    else if (meterErr.Limit.IndexOf('±') >= 0)
                    {
                        entity.ERR_UP = meterErr.Limit.Trim('±', ' ');
                        entity.ERR_DOWN = meterErr.Limit.Trim().Replace("±", "-");
                    }
                }

                entity.CONC_CODE = meterErr.Result == ConstHelper.合格 ? "01" : "02";
                entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.HANDLE_FLAG = "0";
                entity.HANDLE_DATE = "";
                sql.Add(entity.ToInsertString());
            }
            return sql.ToArray();

        }


        /// <summary>
        /// 标准偏差（测量重复性检测）
        /// </summary>
        protected string[] GetMT_DEVIATION_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            string[] keys = new string[meter.MeterErrors.Keys.Count];
            meter.MeterErrors.Keys.CopyTo(keys, 0);
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];

                if (key.Length <= 3) continue;       //如果ID长度是3表示是大项目，则跳过
                if (key.Substring(0, 1) != "2") continue;

                MeterError meterErr = meter.MeterErrors[key];

                string[] wc = meterErr.WcMore.Split('|');
                if (wc.Length <= 2) continue;

                //修改后
                string tmpHi;
                if (key.Length == 10)
                {
                    tmpHi = key.Substring(1, 3);            //取出误差类别+功率方向+元件
                }
                else
                {
                    tmpHi = key.Substring(0, 3);            //取出误差类别+功率方向+元件
                }

                //string tmpHi = key.Substring(0, 3);            //取出误差类别+功率方向+元件
                //string tmpLo = key.Substring(7);               //取出谐波+相序
                //string tmpGlys = key.Substring(3, 2);          //取出功率因数
                //string tmpxIb = key.Substring(5, 2);           //取出电流倍数

                MT_MEASURE_REPEAT_MET_CONC entity = new MT_MEASURE_REPEAT_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    PARA_INDEX = (i + 1).ToString(),
                    DETECT_ITEM_POINT = (i + 1).ToString(),
                    IS_VALID = "1",

                    CONC_CODE = meterErr.Result == ConstHelper.合格 ? "01" : "02",
                    WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",

                    BOTH_WAY_POWER_FLAG = tmpHi.Substring(1, 1),
                };

                //string abc;
                //string strYj;
                //switch (meterErr.YJ)
                //{
                //    case "A":
                //        strYj = "A元";
                //        break;
                //    case "B":
                //        strYj = "B元";
                //        break;
                //    case "C":
                //        strYj = "C元";
                //        break;
                //    case "H":
                //        strYj = "合元";
                //        break;
                //    default:
                //        strYj = "合元";
                //        break;
                //}
                //if (strYj == "合元")
                //{
                //    if (meter.MD_WiringMode == "三相三线")
                //        abc = "AC";
                //    else
                //        abc = "ABC";
                //}
                //else
                //    abc = strYj.Trim('元');
                //entity.IABC = GetPCode("currentPhaseCode", abc);

                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");
                entity.LOAD_CURRENT = GetPCode("meter_Test_CurLoad", meterErr.IbX);
                entity.FREQ = GetPCode("meter_Test_Freq", "50");
                entity.PF = GetPCode("meterTestPowerFactor", meterErr.GLYS.Trim());

                //新加
                entity.VOLT = "100%Un";

                string[] arrValue = meterErr.WcMore.Split('|');
                if (arrValue.Length >= 6)
                {
                    entity.SIMPLING = arrValue[0] + "|" + arrValue[1] + "|" + arrValue[2] + "|" + arrValue[3] + "|" + arrValue[4];
                    entity.DEVIATION_LIMT = arrValue[5];
                }

                sql.Add(entity.ToInsertString());
            }
            return sql.ToArray(); ;
        }

        /// <summary>
        /// 日计时
        /// </summary>
        protected string GetMT_DAYERR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string error = "", ave = "", hz = "";
            bool hasData = false;
            string ItemKey = ProjectID.日计时误差;
            if (!meter.MeterDgns.ContainsKey(ItemKey))
            {
                ItemKey = ProjectID.日计时误差_黑龙江;
                if (meter.MeterDgns.ContainsKey(ItemKey))
                    hasData = true;
            }
            else hasData = true;

            if (!hasData) return string.Empty;
            //_ = new string[7];
            string[] v;
            if (ItemKey == ProjectID.日计时误差)
            {
                v = meter.MeterDgns[ItemKey].Value.Split('|');
                if (v.Length > 5)
                    ave = v[5]; //平均值
                if (v.Length > 6)
                    hz = v[6]; //化整值
            }
            else
            {
                v = meter.MeterDgns[ItemKey].WCData.Split('|');
                ave = meter.MeterDgns[ItemKey].AvgValue; //平均值
                hz = meter.MeterDgns[ItemKey].HzValue; //化整值
            }
            if (v != null)
                error = string.Join("|", v.Take(5));//前面5次误差

           string result = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";

            MT_DAYERR_MET_CONC entity = new MT_DAYERR_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "1",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                SEC_PILES = "1",      //ProtocolInfo额外。
                TEST_TIME = "60",
                SIMPLING = "5",
                ERR_ABS = "0.5",
                ERROR = error,
                AVG_ERR = ave,
                INT_CONVERT_ERR = hz,

                CONC_CODE = result,
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",

                //新加
                //电压负载
                LOAD_VOLTAGE = "",
            };
            return entity.ToInsertString();
        }

        /// <summary>
        /// 时间误差
        /// </summary>
        protected string GetMT_CLOCK_VALUE_MET_CONC(MT_METER mtMeter, TestMeterInfo meter)
        {
            string meterTime = "", stdTime = "", wc = "";
            string ItemKey = ProjectID.时钟示值误差;

            if (!meter.MeterDgns.ContainsKey(ItemKey))
                return string.Empty;

            //获取时间值
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                string[] value = meter.MeterDgns[ItemKey].Value.Split('|');
                if (value.Length == 3)
                {
                    meterTime = value[0];
                    stdTime = value[1];
                    wc = value[2];
                }
            }

            MT_CLOCK_VALUE_MET_CONC entity = new MT_CLOCK_VALUE_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "1",
                DETECT_ITEM_POINT = "01",

                IS_VALID = "1",
                CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02",
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",//处理时间

                STD_DATE = stdTime.Split(' ')[0],
                MET_DATE = meterTime.Split(' ')[0],
                TIME_ERR = wc,
                MET_VALUE = meterTime.Split(' ')[1],
                STD_VALUE = stdTime.Split(' ')[1],

                //新加
                ERR_UP = "",
                ERR_DOWM = "",
            };

            return entity.ToInsertString();
        }

        /// <summary>
        /// 计度器总电能示值组合误差
        /// </summary>
        protected string[] GetMT_HUTCHISON_COMBINA_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            string key = ProjectID.电能示值组合误差;
            if (!meter.MeterDgns.ContainsKey(key)) return null;

            MT_HUTCHISON_COMBINA_MET_CONC entity = new MT_HUTCHISON_COMBINA_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = "01",
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),
                LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "1.0Ib"),
                PF = GetPCode("meterTestPowerFactor", "1.0"),

                CONTROL_METHOD = GetPCode("meterTestCtrlMode", "标准表法"),

                //新加
                IR_TIME = "",
                IR_READING = "",

                ERR_DOWN = "-0.01",
                ERR_UP = "0.01",
                VOLTAGE = "100",
                TOTAL_READING_ERR = "",
                TOTAL_INCREMENT = "",
                SUMER_ALL_INCREMENT = "",
                SHARP_INCREMENT = "",
                PEAK_INCREMENT = "",
                FLAT_INCREMENT = "",
                VALLEY_INCREMENT = "",
                VALUE_CONC_CODE = "",

                //CONC_CODE = MisDataHelper.GetDgnConclusion(meter, Cus_DgnItem.电子指示显示器电能示值组合误差),
                CONC_CODE = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : "02",
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",
                FEE_RATIO = "5"
            };
            float incSharp = 0.0f;   //尖示值增量
            float incPeak = 0.0f;    //峰示值增量
            float incFlat = 0.0f;    //平示值增量
            float incValley = 0.0f;  //谷示值增量
            float incTotal = 0.0f;   //总示值增量
            float incDeepValley = 0.0f; //深谷示值增量
            int times = 0; //时间

            string[] value = meter.MeterDgns[key].Value.Split('|');
            string[] testvalue = meter.MeterDgns[key].TestValue.Split('|');

            string[] testvalueone = testvalue[0].Split(',', '，');//费率时段,:
            if (testvalue[0].IndexOf(',') == -1 && testvalue[0].IndexOf('，') == -1)
            {
                testvalueone = testvalue[0].Split(':', '：');
            }

            if (meter.MeterDgns.ContainsKey(key))
            {
                if (value.Length > 5)
                {

                    //float.TryParse(value[0], out float incstart);
                    //float.TryParse(value[1], out float incend);
                    float.TryParse(value[2], out incTotal);//总示值增量

                    string[] valueErr = value[5].Split(',');//费率时段
                    if (valueErr.Length > 3)
                    {
                        float.TryParse(valueErr[0], out incSharp);
                        float.TryParse(valueErr[1], out incPeak);
                        float.TryParse(valueErr[2], out incFlat);
                        float.TryParse(valueErr[3], out incValley);
                    }
                    if (valueErr.Length > 4) float.TryParse(valueErr[4], out incDeepValley);

                    int.TryParse(testvalue.Length > 1 ? testvalue[1] : "1", out int timestmp);
                    times = testvalueone.Length * timestmp;

                    entity.FEE_VALUE = "尖|峰|平|谷|深谷|总";
                }
                else
                {
                    entity.VALUE_CONC_CODE = value[0];
                    entity.FEE_VALUE = value[0];
                }
            }
            entity.START_VALUE = "0.00" + "|" + "0.00" + "|" + "0.00" + "|" + "0.00" + "|" + "0.00" + "|" + "0.00";
            entity.END_VALUE = incSharp.ToString() + "|" + incPeak.ToString() + "|" + incFlat.ToString() + "|" + incValley.ToString() + "|" + incDeepValley + "|" + incTotal.ToString();
            entity.ELE_INCREMENT = incSharp.ToString() + "|" + incPeak.ToString() + "|" + incFlat.ToString() + "|" + incValley.ToString() + "|" + incDeepValley + "|" + incTotal.ToString();
            float incSumAll = incSharp + incPeak + incFlat + incValley + incDeepValley;

            float err = incTotal - incSumAll;//总分电量值差（千瓦时）

            entity.SUMER_ALL_INCREMENT = incSumAll.ToString("F4");//各费率示值增量和

            entity.SHARP_INCREMENT = incSharp.ToString("F4");//尖示值增量
            entity.PEAK_INCREMENT = incPeak.ToString("F4"); //峰示值增量
            entity.FLAT_INCREMENT = incFlat.ToString("F4"); //平示值增量
            entity.VALLEY_INCREMENT = incValley.ToString("F4");//谷示值增量
            entity.TOTAL_INCREMENT = incTotal.ToString("F4");//总示值增量
            entity.TOTAL_READING_ERR = err.ToString("F4");//总分电量差值
            entity.IR_TIME = times.ToString(); //走字时间(分钟)
            entity.IR_READING = incTotal.ToString("F4");//走字度数(千瓦时)
            entity.FEE_RATIO = "总";
            entity.VALUE_CONC_CODE = err.ToString("F4");

            sql.Add(entity.ToInsertString());

            return sql.ToArray();
        }

        //add yjt 20220513 新增时段投切
        /// <summary>
        /// 时段投切
        /// </summary>
        protected string[] GetMT_TS_MET_CONC(MT_METER mtMeter, TestMeterInfo meter)
        {
            List<string> sql = new List<string>();
            string ItemKey = ProjectID.时段投切;
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                string result = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";
                MT_TS_MET_CONC entity = new MT_TS_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),

                    IS_VALID = "1",
                    CONC_CODE = result,
                    WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",
                };

                string[] testvalue = meter.MeterDgns[ItemKey].TestValue.Split('|');
                string[] testvalueone = testvalue[0].Replace(" ", "").Replace("，", ",").Replace("（", "(").Replace("）", ")").Split(',');//费率时段

                string[] value = meter.MeterDgns[ItemKey].Value.Split('|');
                if (value.Length >= 3)
                {
                    string[] BZvalue = value[0].Split(',');//标准投切时间
                    string[] SJvalue = value[1].Split(',');//实际投切时间
                    string[] valueErr = value[2].Split(',');//投切误差
                    int index = 0;
                    for (int i = 0; i < testvalueone.Length; i++)
                    {
                        {
                            string curSD = testvalueone[i];
                            string[] para = curSD.Split('(');

                            if (i < BZvalue.Length && i < SJvalue.Length && i < valueErr.Length && para.Length > 1)
                            {
                                entity.FEE = GetPCode("tari_ff", para[1].TrimEnd(')'));
                                entity.START_TIME = BZvalue[i];
                                entity.TS_START_TIME = BZvalue[i];
                                entity.TS_REAL_TIME = SJvalue[i];
                                entity.TS_ERR_CONC_CODE = valueErr[i];
                                entity.TS_ERR = "300|-300";
                                entity.ERR_UP = "+300";
                                entity.ERR_DOWM = "-300";
                                entity.TS_WAY = "电量";
                                entity.PARA_INDEX = (index + 1).ToString();
                                entity.DETECT_ITEM_POINT = (index + 1).ToString();
                                entity.VOLT = meter.MD_UB.ToString();
                                sql.Add(entity.ToInsertString());
                                index++;
                            }
                        }
                    }
                }
            }
            return sql.ToArray();
        }

        #region 需量
        /// <summary>
        /// 需量示值误差 原
        /// </summary>
        protected string[] GetMT_DEMANDVALUE_MET_CONC(MT_METER mtMeter, TestMeterInfo meter)
        {


            string[] sql = new string[3];
            MT_DEMANDVALUE_MET_CONC entity = new MT_DEMANDVALUE_MET_CONC();

            string[] wc = GetXLData(meter);
            string asd = "";


            double aaa;
            if (mtMeter.AP_PRE_LEVEL_CODE == "01" || mtMeter.AP_PRE_LEVEL_CODE == "09")
            {
                aaa = 1.0;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "02")
            {
                aaa = 2.0;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "03")
            {
                aaa = 3.0;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "04" || mtMeter.AP_PRE_LEVEL_CODE == "05")
            {
                aaa = 0.5;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "06" || mtMeter.AP_PRE_LEVEL_CODE == "07")
            {
                aaa = 0.2;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "08")
            {
                aaa = 0.1;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "10")
            {
                aaa = 0.05;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "11")
            {
                aaa = 0.02;
            }
            else if (mtMeter.AP_PRE_LEVEL_CODE == "12" || mtMeter.AP_PRE_LEVEL_CODE == "13" || mtMeter.AP_PRE_LEVEL_CODE == "14"
                || mtMeter.AP_PRE_LEVEL_CODE == "15" || mtMeter.AP_PRE_LEVEL_CODE == "16")
            {
                aaa = 1.0;
                asd = "ABCDE";
            }
            else
            {
                aaa = 1.0;
            }

            if ((wc[0] != null && wc[0] != "") || (wc[1] != null && wc[1] != ""))
            {
                entity.DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO;
                entity.EQUIP_CATEG = "01";
                entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
                entity.DETECT_EQUIP_NO = meter.BenthNo;
                entity.DETECT_UNIT_NO = string.Empty;
                entity.POSITION_NO = meter.MD_Epitope.ToString();
                entity.BAR_CODE = mtMeter.BAR_CODE;
                entity.DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss");
                entity.PARA_INDEX = "1";
                entity.DETECT_ITEM_POINT = "1";
                entity.IS_VALID = "1";
                entity.DEMAND_PERIOD = "15";            // 需量周期时间 不知道这里是分钟还是秒，你到台子调注意下
                entity.DEMAND_TIME = "1";               // 需量滑差时间
                entity.DEMAND_INTERVAL = "1";                                       //需量滑差次数
                entity.REAL_DEMAND = wc[0];             //实际需量
                entity.REAL_PERIOD = "15";
                entity.DEMAND_VALUE_ERR = wc[2];        //需量示值误差
                entity.DEMAND_STANDARD = wc[1];         //标准需量

                if (asd == "ABCDE")
                {
                    aaa += 0.004;
                }

                entity.DEMAND_VALUE_ERR_ABS = "±" + aaa.ToString();            //需量示值误差限
                entity.CLEAR_DATA_RST = wc[4] == ConstHelper.合格 ? "01" : "02";   //清零结论
                entity.VALUE_CONC_CODE = wc[4] == ConstHelper.合格 ? "01" : "02";  //示值误差结论
                entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.HANDLE_FLAG = "0";
                entity.HANDLE_DATE = "";                //处理时间
                entity.BOTH_WAY_POWER_FLAG = "0";       //功率方向
                entity.VOLTAGE = "100%Un";              //电压
                entity.LOAD_CURRENT = "08";             //负载电流
                entity.PF = GetPCode("meterTestPowerFactor", "1.0");
                entity.CONTROL_METHOD = "01";
                entity.ERR_UP = aaa.ToString();
                entity.ERR_DOWM = "-" + aaa.ToString();
                entity.CHK_CONC_CODE = wc[4] == ConstHelper.合格 ? "01" : "02";    //结论

                sql[0] = entity.ToInsertString();
            }

            if ((wc[5] != null && wc[5] != "") || (wc[6] != null && wc[6] != ""))
            {
                entity.DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO;
                entity.EQUIP_CATEG = "01";
                entity.SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402";
                entity.DETECT_EQUIP_NO = meter.BenthNo;
                entity.DETECT_UNIT_NO = string.Empty;
                entity.POSITION_NO = meter.MD_Epitope.ToString();
                entity.BAR_CODE = mtMeter.BAR_CODE;
                entity.DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss");
                entity.PARA_INDEX = "2";
                entity.DETECT_ITEM_POINT = "2";
                entity.IS_VALID = "1";
                entity.DEMAND_PERIOD = "15";        // 需量周期时间 不知道这里是分钟还是秒，你到台子调注意下
                entity.DEMAND_TIME = "1";       // 需量滑差时间
                entity.DEMAND_INTERVAL = "1";                                       //需量滑差次数
                entity.REAL_DEMAND = wc[5];         //实际需量
                entity.REAL_PERIOD = "15";
                entity.DEMAND_VALUE_ERR = wc[7];    //需量示值误差   
                entity.DEMAND_STANDARD = wc[6];     //标准需量

                if (asd == "ABCDE")
                {
                    aaa += 0.05;
                }

                entity.DEMAND_VALUE_ERR_ABS = "±" + aaa.ToString();            //需量示值误差限         //需量示值误差限
                entity.CLEAR_DATA_RST = wc[9] == ConstHelper.合格 ? "01" : "02";   //清零结论
                entity.VALUE_CONC_CODE = wc[9] == ConstHelper.合格 ? "01" : "02";  //示值误差结论
                entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.HANDLE_FLAG = "0";
                entity.HANDLE_DATE = "";            //处理时间
                entity.BOTH_WAY_POWER_FLAG = "0";   //功率方向
                entity.VOLTAGE = "100%Un";          //电压
                entity.LOAD_CURRENT = "05";         //负载电流
                entity.PF = GetPCode("meterTestPowerFactor", "1.0");
                entity.CONTROL_METHOD = "01";
                entity.ERR_UP = aaa.ToString();
                entity.ERR_DOWM = "-" + aaa.ToString();
                entity.CHK_CONC_CODE = wc[9] == ConstHelper.合格 ? "01" : "02";//结论

                sql[1] = entity.ToInsertString();
            }
            if ((wc[10] != null && wc[10] != "") || (wc[11] != null && wc[11] != ""))
            {
                entity.DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO;
                entity.EQUIP_CATEG = "01";
                entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
                entity.DETECT_EQUIP_NO = meter.BenthNo;
                entity.DETECT_UNIT_NO = string.Empty;
                entity.POSITION_NO = meter.MD_Epitope.ToString();
                entity.BAR_CODE = mtMeter.BAR_CODE;
                entity.DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss");
                entity.PARA_INDEX = "3";
                entity.DETECT_ITEM_POINT = "3";
                entity.IS_VALID = "1";
                entity.DEMAND_PERIOD = "15";            //需量周期时间 不知道这里是分钟还是秒，你到台子调注意下
                entity.DEMAND_TIME = "1";               //需量滑差时间
                entity.DEMAND_INTERVAL = "1";           //需量滑差次数
                entity.REAL_DEMAND = wc[10];            //实际需量
                entity.REAL_PERIOD = "15";
                entity.DEMAND_VALUE_ERR = wc[12];       //需量示值误差
                entity.DEMAND_STANDARD = wc[11];        //标准需量

                if (asd == "ABCDE")
                {
                    aaa += 0.5;
                }

                entity.DEMAND_VALUE_ERR_ABS = "±" + aaa.ToString();            //需量示值误差限
                entity.CLEAR_DATA_RST = wc[14] == ConstHelper.合格 ? "01" : "02";  //清零结论
                entity.VALUE_CONC_CODE = wc[14] == ConstHelper.合格 ? "01" : "02"; //示值误差结论
                entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.HANDLE_FLAG = "0";
                entity.HANDLE_DATE = "";//处理时间
                entity.BOTH_WAY_POWER_FLAG = "0";       //功率方向
                entity.VOLTAGE = "100%Un";              //电压
                entity.LOAD_CURRENT = "00";             //负载电流
                entity.PF = GetPCode("meterTestPowerFactor", "1.0");
                entity.CONTROL_METHOD = "01";
                entity.ERR_UP = aaa.ToString();
                entity.ERR_DOWM = "-" + aaa.ToString();
                entity.CHK_CONC_CODE = wc[14] == ConstHelper.合格 ? "01" : "02";   //结论

                sql[2] = entity.ToInsertString();
            }
            return sql;

        }
        #endregion


        //protected string[] GetMT_DEMANDVALUE_MET_CONC(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    List<string> sql = new List<string>();
        //    //在多功能里面的
        //    Dictionary<string, MeterDgn> meterDgnS = meter.MeterDgns;

        //    //if (!meter.MeterDgns.ContainsKey(key)) return null;

        //    for (int i = 1; i <= 3; i++)
        //    {
        //        string key = ProjectID.需量示值误差 + "_" + i.ToString();
        //        if (!meter.MeterDgns.ContainsKey(key)) continue;
        //        string[] value = meter.MeterDgns[key].Value.Split('|');
        //        string[] Testvalue = meter.MeterDgns[key].TestValue.Split('|');

        //        MeterDgn meterDgn = meter.MeterDgns[key];
        //        int Index = 1;
        //        MT_DEMANDVALUE_MET_CONC entity = new MT_DEMANDVALUE_MET_CONC
        //        {

        //            DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            EQUIP_CATEG = "01",
        //            SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //            DETECT_EQUIP_NO = meter.BenthNo,
        //            DETECT_UNIT_NO = string.Empty,
        //            POSITION_NO = meter.MD_Epitope.ToString(),
        //            BAR_CODE = mtMeter.BAR_CODE,
        //            DETECT_DATE = meter.VerifyDate.ToString(),
        //            PARA_INDEX = Index.ToString(),
        //            DETECT_ITEM_POINT = Index.ToString(),
        //            IS_VALID = "1",

        //            LOAD_CURRENT = GetPCode("meter_Test_CurLoad", Testvalue[0]),
        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", Testvalue[1]),  //功率方向
        //            DEMAND_PERIOD = Testvalue[2],   //周期
        //            DEMAND_TIME = Testvalue[3],   //滑差时间
        //            DEMAND_INTERVAL = Testvalue[4],  //滑差次数

        //            REAL_DEMAND = value[4],   //实际需量
        //            REAL_PERIOD = Testvalue[2],    //实际周期
        //            DEMAND_VALUE_ERR = value[5],    //需量示值误差
        //            DEMAND_STANDARD = value[3],  //标准表需量值
        //            DEMAND_VALUE_ERR_ABS = "1",   //需量示值误差限
        //            CLEAR_DATA_RST = meterDgn.Result == ConstHelper.合格 ? "01" : "02",   //需量清零结果
        //            VALUE_CONC_CODE = meterDgn.Result == ConstHelper.合格 ? "01" : "02",  //示值误差结论

        //            WRITE_DATE = DateTime.Now.ToString(),
        //            HANDLE_FLAG = "0",
        //            HANDLE_DATE = "",
        //            VOLTAGE = "100%Un",
        //            PF = GetPCode("meterTestPowerFactor", "1.0"),
        //            CONTROL_METHOD = "01",
        //            ERR_UP = value[1],
        //            ERR_DOWM = value[2],
        //            CHK_CONC_CODE = meterDgn.Result == ConstHelper.合格 ? "01" : "02",
        //        };
        //        sql.Add(entity.ToInsertString());

        //        Index++;
        //    }

        //    return sql.ToArray();
        //}

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
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    PARA_INDEX = index.ToString(),
                    DETECT_ITEM_POINT = index.ToString(),
                    WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
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
        /// 交流电压试验
        /// </summary>
        protected string GetMT_VOLT_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            MeterInsulation firstItem = null;
            foreach (KeyValuePair<string, MeterInsulation> item in meter.MeterInsulations)
            {
                firstItem = item.Value;
                break;
            }
            string testVoltage = "4000";
            if (firstItem != null) testVoltage = firstItem.Voltage.ToString();

            MT_VOLT_MET_CONC entity = new MT_VOLT_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                VOLT_TEST_VALUE = testVoltage,
                VOLT_OBJ = "02",
                TEST_TIME = "60",
                CONC_CODE = "01",
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",
                LEAK_CURRENT_LIMIT = "5",
                POSITION_LEAK_LIMIT = "5",
            };
            return entity.ToInsertString();
        }

        /// <summary>
        /// 密钥更新
        /// </summary>
        protected string GetMT_ESAM_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            if (meter.MeterCostControls.ContainsKey(ProjectID.密钥更新))
            {
                MT_ESAM_MET_CONC entity = new MT_ESAM_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    PARA_INDEX = "01",
                    DETECT_ITEM_POINT = "01",
                    IS_VALID = "1",
                    LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//
                    KEY_NUM = "3",//密钥条数
                    KEY_VER = "04",//密钥版本
                    KEY_STATUS = GetPCode("secretKeyStatus", "正式密钥"), //密钥状态
                    KEY_TYPE = GetPCode("secretKeyType", "身份认证密钥"), //密钥类型
                                                                    //CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.密钥更新),
                    CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.密钥更新),
                    WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",
                };
                return entity.ToInsertString();
            }
            return "";
        }

        /// <summary>
        /// 身份认证
        /// </summary>
        protected string GetMT_ESAM_SECURITY_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            if (meter.MeterCostControls.ContainsKey(ProjectID.身份认证))
            {
                MT_ESAM_SECURITY_MET_CONC entity = new MT_ESAM_SECURITY_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                    DETECT_EQUIP_NO = meter.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meter.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    PARA_INDEX = "01",
                    DETECT_ITEM_POINT = "01",
                    IS_VALID = "1",
                    ESAM_ID = MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证) == "01" ? ConstHelper.合格 : ConstHelper.不合格,
                    CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.身份认证),
                    //ESAM_ID = meter.MeterCostControls[ProjectID.身份认证].Result == ConstHelper.合格 ? "01" : "02",
                    //CONC_CODE = meter.MeterCostControls[ProjectID.身份认证].Result == ConstHelper.合格 ? "01" : "02",
                    WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    HANDLE_FLAG = "0",
                    HANDLE_DATE = "",
                };

                return entity.ToInsertString();
            }
            return "";
        }

        /// <summary>
        /// 剩余电量递减准确度
        /// </summary>
        protected string GetMT_EQ_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string ItemKey = ProjectID.剩余电量递减准确度;
            if (!meter.MeterCostControls.ContainsKey(ItemKey)) return null;

            string data = meter.MeterCostControls[ItemKey].Data;

            MT_EQ_MET_CONC e = new MT_EQ_MET_CONC
            {
                //EQUIP_ID = mtMeter.METER_ID.Length == 0 ? "1234" : mtMeter.METER_ID,  //没有ID

                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",

                IS_VALID = "1",
                TOTAL_EQ = "03",
                SURPLUS_EQ = "0",
                CURR_ELEC_PRICE = "0",
                LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "Imax"),
                PF = GetPCode("meterTestPowerFactor", "1.0"),

                CONC_CODE = meter.MeterCostControls[ItemKey].Result.Trim() == ConstHelper.合格 ? "01" : "02",
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",
            };

            if (!string.IsNullOrEmpty(data))
            {
                string[] ds = data.Split('|');
                if (ds.Length >= 6)
                {
                    string[] str = ds[2].Split(',');
                    e.TOTAL_EQ = str[0];

                    e.SURPLUS_EQ = ds[4].Split(',')[0];
                    e.CURR_ELEC_PRICE = ds[0];
                }
            }

            return e.ToInsertString();
        }

        /// <summary>
        /// 剩余电量递减准确度
        /// </summary>
        protected string MT_SURPLUS_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string ItemKey = ProjectID.剩余电量递减准确度;
            if (!meter.MeterCostControls.ContainsKey(ItemKey)) return null;

            MT_SURPLUS_MET_CONC e = new MT_SURPLUS_MET_CONC
            {
                EQUIP_ID = mtMeter.METER_ID.Length == 0 ? "1234" : mtMeter.METER_ID,
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",

                SUM_INCREASE = "03",
                SURPLUS_REDUCE = "0",
                SURPLUS_VALUE = "0",
            };


            string data = meter.MeterCostControls[ItemKey].Data;
            if (!string.IsNullOrEmpty(data))
            {
                string[] ds = data.Split('|');
                if (ds.Length >= 6)
                {
                    //string[] str = ds[2].Split(',');
                    if (string.IsNullOrEmpty(ds[4]))
                        e.SURPLUS_REDUCE = "0";
                    else
                        e.SURPLUS_REDUCE = ds[5].ToString();
                    e.SUM_INCREASE = ds[2].ToString();
                    e.SURPLUS_VALUE = ds[7].ToString();
                }
            }

            e.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result.Trim() == ConstHelper.合格 ? "01" : "02";

            e.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            e.HANDLE_FLAG = "0";

            return e.ToInsertString();
        }

        /// <summary>
        /// 控制功能
        /// </summary>
        protected string GetMT_CONTROL_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string ItemKey = ProjectID.控制功能;
            if (!meter.MeterCostControls.ContainsKey(ItemKey)) return null;

            string data = meter.MeterCostControls[ItemKey].TestValue;

            MT_CONTROL_MET_CONC entity = new MT_CONTROL_MET_CONC
            {
                EQUIP_ID = mtMeter.METER_ID.Length == 0 ? "1234" : mtMeter.METER_ID,
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                DETECT_ITEM_POINT = "01",
                IS_VALID = "1",
                SETTING_WARN_VALUE1 = data.Substring(0, 3),//预置报警金额1
                SETTING_WARN_VALUE2 = data.Substring(4, 3),//预置报警金额2
                REAL_WARN_VALUE1 = data.Substring(0, 3),//实际报警金额1
                REAL_WARN_VALUE2 = data.Substring(4, 3),//实际报警金额2
                CHK_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.控制功能),
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                HANDLE_DATE = "",
            };
            return entity.ToInsertString();
        }

        /// <summary>
        /// 误差一致性
        /// </summary>
        protected string[] GetMT_CONSIST_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meterInfo)
        {
            string key = "1";
            if (!meterInfo.MeterErrAccords.ContainsKey(key)) return null;
            if (meterInfo.MeterErrAccords[key].PointList.Count <= 0) return null;
            string[] sql = new string[meterInfo.MeterErrAccords[key].PointList.Keys.Count];
            if (meterInfo.MeterErrAccords.ContainsKey(key))          //如果数据模型中已经存在该点的数据
            {
                int iIndex = 0;
                MT_CONSIST_MET_CONC entity = new MT_CONSIST_MET_CONC
                {
                    DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                    EQUIP_CATEG = "01",
                    DETECT_EQUIP_NO = meterInfo.BenthNo,
                    DETECT_UNIT_NO = string.Empty,
                    POSITION_NO = meterInfo.MD_Epitope.ToString(),
                    BAR_CODE = mtMeter.BAR_CODE,
                    DETECT_DATE = Convert.ToDateTime(meterInfo.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    IS_VALID = "1"
                };

                foreach (string k in meterInfo.MeterErrAccords[key].PointList.Keys)
                {
                    MeterErrAccordBase errAccord = meterInfo.MeterErrAccords[key].PointList[k];
                    entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");
                    string[] limit = errAccord.Limit.Split('|');
                    string slimit = errAccord.Limit;
                    if (errAccord.Limit.IndexOf('±') != -1)
                    {
                        slimit = errAccord.Limit.Replace("±", "");
                        limit = $"{slimit}|-{slimit}".Split('|');
                    }
                    entity.ERR_DOWN = limit[1];
                    entity.ERR_UP = limit[0];
                    string xib = errAccord.IbX;
                    if (xib == "Ib") xib = "1.0Ib";
                    else if (xib == "10Itr") xib = "10.0Itr";
                    else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
                    else if (xib == "1In" || xib == "1.0In") xib = "In";
                    entity.LOAD_CURRENT = GetPCode("meter_Test_CurLoad", xib);

                    //新加
                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");

                    entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);
                    entity.SIMPLING = "2";
                    entity.PARA_INDEX = (iIndex + 1).ToString();
                    entity.DETECT_ITEM_POINT = (iIndex + 1).ToString();
                    entity.PULES = errAccord.PulseCount.ToString();
                    entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    entity.HANDLE_FLAG = "0";
                    entity.HANDLE_DATE = "";
                    entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";
                    entity.ALL_AVG_ERROR = errAccord.ErrAver.Trim();
                    entity.ALL_INT_ERROR = errAccord.Error.Trim();
                    string[] Arr_Err = errAccord.Data1.Split('|');           //分解误差
                    string[] Err0 = errAccord.Data1.Split('|');           //分解误差

                    entity.ALL_ERROR = Arr_Err[0] + "|" + Arr_Err[1];
                    entity.ERROR = errAccord.Error.Trim();
                    entity.AVG_ERROR = Arr_Err[2];
                    entity.INT_CONVERT_ERR = Arr_Err[3];

                    entity.ONCE_ERR = Err0[0] + "|" + Err0[1];
                    entity.AVG_ONCE_ERR = Err0[2];
                    entity.INT_ONCE_ERR = Err0[3];

                    //新加，只测一次，去掉
                    //string[] Err1 = errAccord.Data2.Split('|');           //分解误差
                    //entity.TWICE_ERR = Err1[0] + "|" + Err1[1];
                    //entity.AVG_TWICE_ERR = Err1[2];
                    //entity.AVG_TWICE_ERR = Err1[3];

                    sql[iIndex++] = entity.ToInsertString();
                }

            }
            return sql;
        }

        /// <summary>
        /// 误差变差
        /// </summary>
        protected string[] GetMT_ERROR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meterInfo)
        {

            string key = "2";
            if (!meterInfo.MeterErrAccords.ContainsKey(key)) return null;
            if (meterInfo.MeterErrAccords[key].PointList.Count <= 0) return null;
            string[] sql = new string[meterInfo.MeterErrAccords[key].PointList.Keys.Count];

            MT_ERROR_MET_CONC entity = new MT_ERROR_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                DETECT_EQUIP_NO = meterInfo.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meterInfo.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meterInfo.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                IS_VALID = "1"
            };

            int index = 0;
            foreach (string k in meterInfo.MeterErrAccords[key].PointList.Keys)
            {
                MeterErrAccordBase errAccord = meterInfo.MeterErrAccords[key].PointList[k];
                entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");
                string[] arrlimit = errAccord.Limit.Split('|');
                string slimit = errAccord.Limit;
                if (errAccord.Limit.IndexOf('±') != -1)
                {
                    slimit = errAccord.Limit.Replace("±", "");
                    arrlimit = $"{slimit}|-{slimit}".Split('|');
                }
                entity.ERR_DOWN = arrlimit[1];
                entity.ERR_UP = arrlimit[0];
                string xib = errAccord.IbX;
                if (xib == "Ib") xib = "1.0Ib";
                else if (xib == "10Itr") xib = "10.0Itr";
                else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
                else if (xib == "1In" || xib == "1.0In") xib = "In";
                entity.LOAD_CURRENT = GetPCode("meter_Test_CurLoad", xib);

                //新加
                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");

                entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);
                entity.SIMPLING = "2";
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();

                entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.HANDLE_FLAG = "0";
                entity.HANDLE_DATE = "";
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
                entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
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
        protected string[] GetMT_VARIATION_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            string key = "3";
            if (!meter.MeterErrAccords.ContainsKey(key)) return null;
            if (meter.MeterErrAccords[key].PointList.Count <= 0) return null;
            string[] sql = new string[meter.MeterErrAccords[key].PointList.Keys.Count];
            MT_VARIATION_MET_CONC entity = new MT_VARIATION_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),

                IS_VALID = "1",
                HANDLE_FLAG = "0"
            };


            int index = 0;
            foreach (string k in meter.MeterErrAccords[key].PointList.Keys)
            {
                MeterErrAccordBase errAccord = meter.MeterErrAccords[key].PointList[k];
                entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");
                string xib = errAccord.IbX;
                if (xib == "Ib") xib = "1.0Ib";
                else if (xib == "10Itr") xib = "10.0Itr";
                else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
                else if (xib == "1In" || xib == "1.0In") xib = "In";
                entity.LOAD_CURRENT = GetPCode("meter_Test_CurLoad", xib);

                //新加
                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");

                entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);
                entity.IABC = GetPCode("currentPhaseCode", "ABC");
                entity.DETECT_CIRCLE = errAccord.PulseCount.ToString();
                entity.SIMPLING = "2";
                entity.WAIT_TIME = "30";
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();

                entity.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.HANDLE_FLAG = "0";
                entity.HANDLE_DATE = "";

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

                entity.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO;
                entity.PARA_INDEX = (index + 1).ToString();
                entity.DETECT_ITEM_POINT = (index + 1).ToString();
                entity.CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02";

                sql[index++] = entity.ToInsertString();
            }
            return sql;
        }


        #region 非全检项目
        //#region
        ///// <summary>
        ///// 载波
        ///// </summary>
        ///// <param name="mtMeter"></param>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //protected string[] GetMT_WAVE_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)      //shubo 2016-04-29 取消注释
        //{
        //    if (meter.MeterCarrierDatas.Count == 0) return null;

        //    List<MeterCarrierData> zblist = new List<MeterCarrierData>();
        //    foreach (string item in meter.MeterCarrierDatas.Keys)
        //    {
        //        zblist.Add(meter.MeterCarrierDatas[item]);
        //    }

        //    string[] sql = new string[zblist.Count];
        //    for (int i = 0; i < zblist.Count; i++)
        //    {
        //        MT_WAVE_MET_CONC entity = new MT_WAVE_MET_CONC
        //        {
        //            DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            EQUIP_CATEG = "01",
        //            SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //            DETECT_EQUIP_NO = meter.BenthNo.ToString().Trim(),
        //            DETECT_UNIT_NO = string.Empty,
        //            POSITION_NO = meter.MD_Epitope.ToString().Trim(),
        //            BAR_CODE = mtMeter.BAR_CODE,
        //            DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //            PARA_INDEX = (i + 1).ToString(),
        //            DETECT_ITEM_POINT = (i + 1).ToString(),
        //            IS_VALID = "1",
        //            CONC_CODE = zblist[i].Result == ConstHelper.合格 ? "01" : "02",
        //            WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //            HANDLE_FLAG = "0",
        //            HANDLE_DATE = "",
        //        };
        //        sql[i] = entity.ToInsertString();
        //    }
        //    return sql;
        //}

        ////报警
        //protected string GetMT_WARNNING_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.报警功能) == "") return "";
        //    MT_WARNNING_MET_CONC e = new MT_WARNNING_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        IS_VALID = "1",
        //        WARN_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.报警功能),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };
        //    return e.ToInsertString();
        //}

        ////费控保电试验
        //protected string GetMT_EP_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.保电功能) == "") return "";
        //    MT_EP_MET_CONC e = new MT_EP_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        IS_VALID = "1",
        //        EH_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.保电功能),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };
        //    return e.ToInsertString();
        //}

        ////费控取消保电试验 
        //protected string GetMT_EC_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.保电解除) == "") return "";
        //    MT_EC_MET_CONC entity = new MT_EC_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        IS_VALID = "1",
        //        EC_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.保电解除),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };
        //    return entity.ToInsertString();
        //}

        ////费控远程数据回抄试验
        //protected string GetMT_ESAM_READ_MET_CONCByMt(MT_METER mtMeter, MeterInfo meter)
        //{
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.数据回抄) == "") return "";
        //    MT_ESAM_READ_MET_CONC entity = new MT_ESAM_READ_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        IS_VALID = "1",
        //        CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.数据回抄),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };

        //    return entity.ToInsertString();
        //}

        ////参数设置
        //protected string GetMT_PARA_SETTING_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.参数设置) == "") return "";
        //    MT_PARA_SETTING_MET_CONC entity = new MT_PARA_SETTING_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        IS_VALID = "1",
        //        CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.参数设置),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };
        //    return entity.ToInsertString();
        //}

        ////费率和时段功能 
        //protected string GetMT_FEE_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{

        //    if (meter.MeterFunctions.Count == 0) return "";
        //    if (!meter.MeterFunctions.ContainsKey("004")) return "";
        //    MT_FEE_MET_CONC entity = new MT_FEE_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo.ToString().Trim(),
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",

        //        IS_VALID = "1",
        //        CONC_CODE = meter.MeterFunctions["004"].Result == ConstHelper.合格 ? "01" : "02",
        //        PF = GetPCode("meterTestPowerFactor", "1.0"),
        //        CUR_PHASE_CODE = GetPCode("currentPhaseCode", meter.WireMode == WireMode.三相三线 ? "AC" : "ABC"),
        //        LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "1.0Ib"),
        //        BOTH_WAY_POWER_FLAG = "0",

        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",
        //        HANDLE_DATE = "",
        //    };
        //    return entity.ToInsertString();
        //}

        ///// <summary>
        ///// 最大需量功能 
        ///// </summary>
        //protected string GetMT_MAX_DEMANDVALUE_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{

        //    if (meter.MeterFunctions.Count == 0) return "";
        //    if (!meter.MeterFunctions.ContainsKey("006")) return "";
        //    MT_MAX_DEMANDVALUE_MET_CONC entity = new MT_MAX_DEMANDVALUE_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",
        //        DETECT_EQUIP_NO = meter.BenthNo.ToString().Trim(),
        //        DETECT_UNIT_NO = string.Empty,

        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",

        //        IS_VALID = "1",
        //        CONC_CODE = meter.MeterFunctions["006"].Result == ConstHelper.合格 ? "01" : "02",

        //        CUR_PHASE_CODE = GetPCode("currentPhaseCode", meter.WireMode == WireMode.三相三线 ? "AC" : "ABC"),
        //        LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "1.0Ib"),
        //        BOTH_WAY_POWER_FLAG = "0",

        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",
        //        HANDLE_DATE = "",

        //        //PF = GetPCode("meterTestPowerFactor", "1.0"),
        //        //VOLT_RATIO = GetPCode("meterTestVolt", "100%Un"),
        //    };
        //    return entity.ToInsertString();
        //}

        ///// <summary>
        ///// 计量功能
        ///// </summary>
        //protected string GetMT_POWER_MEASURE_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{

        //    if (meter.MeterFunctions.Count == 0) return "";
        //    if (!meter.MeterFunctions.ContainsKey("001")) return "";
        //    MT_POWER_MEASURE_MET_CONC entity = new MT_POWER_MEASURE_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",
        //        DETECT_EQUIP_NO = meter.BenthNo.ToString().Trim(),
        //        DETECT_UNIT_NO = string.Empty,

        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        IS_VALID = "1",
        //        CONC_CODE = meter.MeterFunctions["001"].Result == ConstHelper.合格 ? "01" : "02",
        //        CUR_PHASE_CODE = GetPCode("currentPhaseCode", meter.WireMode == WireMode.三相三线 ? "AC" : "ABC"),
        //        VOLT_RATIO = GetPCode("meterTestVolt", "100%Un"),

        //        //新加
        //        ERR_DOWN = "",
        //        ERR_UP = "",
        //        SIMPLING_SPACE = "",
        //        SIMPLING = "",

        //        PF = GetPCode("meterTestPowerFactor", "1.0"),
        //        LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "1.0Ib"),
        //        BOTH_WAY_POWER_FLAG = "0",

        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",
        //        HANDLE_DATE = "",

        //        //新加
        //        //CONSIST_UP ="",
        //        //CONSIST_DOWN="",
        //    };

        //    return entity.ToInsertString();
        //}

        ///// <summary>
        ///// 功耗
        ///// </summary>
        //protected string GetMT_POWER_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    if (meter.MeterPowers.Count == 0) return "";
        //    MT_POWER_MET_CONC e = new MT_POWER_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",
        //        HANDLE_DATE = "",
        //        CONC_CODE = "01",

        //        IS_VALID = "1",

        //        //新加
        //        ERR_ABS = "",
        //        POWER_CONSUM_TYPE = "",

        //    };

        //    foreach (string item in meter.MeterPowers.Keys)
        //    {
        //        e.CUR_ACT_POWER = "";
        //        e.CUR_ACT_POWER_ERR = "";
        //        e.CUR_ACT_POWER_RESULT = "";
        //        e.CUR_INS_POWER = meter.MeterPowers[item].IaPowerS.ToString().Trim();
        //        e.CUR_INS_POWER_ERR = meter.MeterPowers[item].AVR_CUR_CIR_S_LIMIT.Trim();
        //        e.CUR_INS_POWER_RESULT = ConstHelper.合格;
        //        e.VOL_ACT_POWER = meter.MeterPowers[item].UaPowerP.ToString().Trim();
        //        e.VOL_ACT_POWER_ERR = meter.MeterPowers[item].AVR_VOT_CIR_P_LIMIT.Trim();
        //        e.VOL_ACT_POWER_RESULT = ConstHelper.合格;
        //        e.VOL_INS_POWER = meter.MeterPowers[item].UaPowerS.ToString().Trim();
        //        e.VOL_INS_POWER_ERR = meter.MeterPowers[item].AVR_VOT_CIR_S_LIMIT.Trim();
        //        e.VOL_INS_POWER_RESULT = ConstHelper.合格;
        //        e.TEST_ITEM = "功耗试验";
        //        e.POWER_CONSUM_TYPE = "功耗试验";
        //        e.ERR_ABS = meter.MeterPowers[item].AVR_CUR_CIR_S_LIMIT.Trim() + "|" + meter.MeterPowers[item].AVR_VOT_CIR_P_LIMIT.Trim() + "|" + meter.MeterPowers[item].AVR_VOT_CIR_S_LIMIT.Trim();
        //    }

        //    return e.ToInsertString();
        //}



        ///// <summary>
        ///// 设置项
        ///// </summary>
        //protected string[] GetMT_PRESETPARAM_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    string[] sql = new string[3];
        //    MT_PRESETPARAM_MET_CONC entity = new MT_PRESETPARAM_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        CONC_CODE = MisDataHelper.GetDgnConclusion(meter, Cus_DgnItem.电量清零),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",
        //        DATA_ITEM_NAME = "设置时间"
        //    };
        //    sql[0] = entity.ToInsertString();

        //    entity.DATA_ITEM_NAME = "清空电量";
        //    sql[1] = entity.ToInsertString();

        //    entity.DATA_ITEM_NAME = "清空事件";
        //    sql[2] = entity.ToInsertString();

        //    return sql;
        //}

        ///// <summary>
        ///// 02 04级密码
        ///// </summary>
        //protected string[] GetMT_PASSWORD_CHANGE_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    string[] sql = new string[2];
        //    MT_PASSWORD_CHANGE_MET_CONC entity = new MT_PASSWORD_CHANGE_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",
        //        CONC_CODE = "01",
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",

        //        PASSWORD_LEVEL = "02级"
        //    };

        //    sql[0] = entity.ToInsertString();
        //    entity.PASSWORD_LEVEL = "04级";
        //    sql[1] = entity.ToInsertString();
        //    return sql;
        //}

        ///// <summary>
        ///// 读表地址
        ///// </summary>
        //protected string GetMT_ADDRESS_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    MT_ADDRESS_MET_CONC entity = new MT_ADDRESS_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "1",

        //        MET_ADDRESS = meter.Address,
        //        CONC_CODE = MisDataHelper.GetDgnConclusion(meter, Cus_DgnItem.通信测试),
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };

        //    return entity.ToInsertString();
        //}

        ///// <summary>
        ///// 通信测试
        ///// </summary>
        //protected string GetMT_COMMINICATE_MET_CONC(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    MT_COMMINICATE_MET_CONC entity = new MT_COMMINICATE_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = meter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        DETECT_ITEM_POINT = "01",

        //        CONC_CODE = MisDataHelper.GetDgnConclusion(meter, Cus_DgnItem.通信测试),

        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0",
        //        HANDLE_DATE = "",
        //    };

        //    return entity.ToInsertString();
        //}


        ///// <summary>
        ///// 电流过载试验
        ///// </summary>
        //protected string GetMT_OVERLOAD_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    string key = "4";
        //    if (!meter.MeterErrAccords.ContainsKey(key)) return null;
        //    if (meter.MeterErrAccords[key].PointList.Count <= 0) return null;
        //    string sql = "";

        //    int index = 0;
        //    foreach (string k in meter.MeterErrAccords[key].PointList.Keys)
        //    {
        //        MeterErrAccordBase errAccord = meter.MeterErrAccords[key].PointList[k];
        //        string[] limit = errAccord.Limit.Split('|');
        //        string[] errs = errAccord.Data1.Split('|');           //分解误差

        //        string current = GetPCode("meter_Test_CurLoad", "10Ib");

        //        MT_OVERLOAD_MET_CONC entity = new MT_OVERLOAD_MET_CONC()
        //        {
        //            //DETECT_TASK_NO = meter.Other5,
        //            DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            EQUIP_CATEG = "01",
        //            SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //            DETECT_EQUIP_NO = meter.BenthNo,
        //            DETECT_UNIT_NO = string.Empty,
        //            POSITION_NO = meter.MD_Epitope.ToString(),
        //            BAR_CODE = meter.BAR_CODE,
        //            DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //            PARA_INDEX = index.ToString(),
        //            DETECT_ITEM_POINT = index.ToString(),
        //            IS_VALID = "1",

        //            //新加
        //            LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),

        //            LOAD_CURRENT = current,
        //            PF = GetPCode("meterTestPowerFactor", errAccord.PF),
        //            PULES = errAccord.PulseCount.ToString(),
        //            SIMPLING = "2",
        //            OVERLOAD_TIME = "60",
        //            WAIT_TIME = "30",
        //            ERROR = errs[2],
        //            ERR_DOWN = limit[1],
        //            ERR_UP = limit[0],
        //            AVG_ERR = errs[0],
        //            INT_CONVERTER_ERR = errs[2],
        //            CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : "02",
        //            WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //            HANDLE_FLAG = "0",
        //            HANDLE_DATE = "",
        //        };
        //        sql = entity.ToInsertString();
        //    }
        //    return sql;

        //}





        ///// <summary>
        ///// 预置内容设置
        ///// </summary>
        //protected string[] GetMT_CONTROL_MET_CONCByMt2(MT_METER mtMeter, TestMeterInfo meterInfo)
        //{
        //    List<string> sqlList = new List<string>();

        //    string key1 = ProjectID.预置内容设置;
        //    string key2 = ProjectID.预置内容检查;
        //    if (!meterInfo.MeterCostControls.ContainsKey(key1) || !meterInfo.MeterCostControls.ContainsKey(key2)) return null;

        //    MT_CONTROL_MET_CONC entity = new MT_CONTROL_MET_CONC
        //    {
        //        EQUIP_ID = mtMeter.METER_ID.Length == 0 ? "1234" : mtMeter.METER_ID,
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //        DETECT_EQUIP_NO = meterInfo.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meterInfo.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meterInfo.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        IS_VALID = "1",
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };

        //    int point = 1;
        //    foreach (string k in meterInfo.MeterCostControls.Keys)
        //    {
        //        string value = meterInfo.MeterCostControls[k].Data;
        //        if (k.Length <= 3) continue;
        //        if (k.Substring(0, 3) != key1 && k.Substring(0, 3) != key2) continue;

        //        switch (k.Substring(3))
        //        {
        //            case "01":
        //            case "02":
        //            case "04":
        //            case "06":
        //                point++;
        //                entity.CHK_CONC_CODE = meterInfo.MeterCostControls[k].Data == ConstHelper.合格 ? "01" : "02";
        //                entity.DETECT_ITEM_POINT = point.ToString("D2");
        //                break;
        //        }
        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    return sqlList.ToArray();
        //}

        ///// <summary>
        ///// 预置内容设置
        ///// </summary>
        //protected string[] GetMT_PRESETPARAM_MET_CONCByMtFK(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    List<string> sql = new List<string>(); ;
        //    int point = 1;
        //    string keyValue = ProjectID.预置内容设置;
        //    if (!meter.MeterCostControls.ContainsKey(keyValue)) return null;

        //    MT_PRESETPARAM_MET_CONC e = new MT_PRESETPARAM_MET_CONC
        //    {
        //        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //        EQUIP_CATEG = "01",
        //        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        //        DETECT_EQUIP_NO = meter.BenthNo,
        //        DETECT_UNIT_NO = string.Empty,
        //        POSITION_NO = meter.MD_Epitope.ToString(),
        //        BAR_CODE = mtMeter.BAR_CODE,
        //        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //        PARA_INDEX = "01",
        //        IS_VALID = "1",
        //        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        HANDLE_FLAG = "0"
        //    };
        //    foreach (string key in meter.MeterCostControls.Keys)
        //    {
        //        if (key.Length <= 3) continue;
        //        if (key.Substring(0, 3) != keyValue) continue;
        //        switch (key.Substring(3))
        //        {
        //            case "01":
        //            case "02":
        //            case "04":
        //            case "06":
        //                point++;
        //                e.CONC_CODE = meter.MeterCostControls[key].Result == ConstHelper.合格 ? "01" : "02";
        //                e.DETECT_ITEM_POINT = point.ToString("D2");
        //                break;
        //        }
        //        sql.Add(e.ToInsertString());
        //    }

        //    return sql.ToArray();
        //}

        ///// <summary>
        ///// 预置内容检查
        ///// </summary>
        protected string[] GetMT_PRESETPARAM_CHECK_MET_CONCByMtFK(MT_METER mtMeter, TestMeterInfo meter)
        {

            string[] sql = new string[9];
            string key = ProjectID.费率时段检查;
            if (!meter.MeterDgns.ContainsKey(key)) return null;

            MT_PRESETPARAM_CHECK_MET_CONC e = new MT_PRESETPARAM_CHECK_MET_CONC
            {
                DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
                EQUIP_CATEG = "01",
                SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
                DETECT_EQUIP_NO = meter.BenthNo,
                DETECT_UNIT_NO = string.Empty,
                POSITION_NO = meter.MD_Epitope.ToString(),
                BAR_CODE = mtMeter.BAR_CODE,
                DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
                PARA_INDEX = "01",
                WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                HANDLE_FLAG = "0",
                CONC_CODE = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : "02"
            };
            string[] testvalue = meter.MeterDgns[key].TestValue.Split('|');
            string[] testvalueone = testvalue[0].Split(',');//费率时段


            for (int i = 0; i < testvalueone.Length; i++)
            {
                string curSD = testvalueone[i];
                string[] para = curSD.Split('(');

                e.DATA_ITEM_NAME = "费率时段检查 - " + para[1].Substring(0, 1);
                e.REAL_VALUE = para[0];
                e.STANDARD_VALUE = para[0];
                e.DETECT_ITEM_POINT = i.ToString();

                sql[i] = e.ToInsertString();
            }



            return sql;
        }

        /////// <summary>
        /////// 预置内容检查
        /////// </summary>
        ////private static string[] GetMT_PRESETPARAM_CHECK_MET_CONCByMtFK(MT_METER mtMeter, MeterInfo meter)
        ////{
        ////    List<string> sql = new List<string>();
        ////    string key = ((int)ProjectID.预置内容检查).ToString("D3");
        ////    if (!meter.MeterCostControls.ContainsKey(key)) return null;

        ////    MT_PRESETPARAM_CHECK_MET_CONC e = new MT_PRESETPARAM_CHECK_MET_CONC
        ////    {
        ////        DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        ////        EQUIP_CATEG = "01",
        ////        SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,
        ////        DETECT_EQUIP_NO = meter.BenthNo,
        ////        DETECT_UNIT_NO = string.Empty,
        ////        POSITION_NO = meter.Index.ToString(),
        ////        BAR_CODE = mtMeter.BAR_CODE,
        ////        DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        ////        PARA_INDEX = "01",
        ////        WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ////        HANDLE_FLAG = "0"
        ////    };

        ////    int point = 1;
        ////    foreach (string k in meter.MeterCostControls.Keys)
        ////    {
        ////        if (k.Length <= 3) continue;
        ////        if (k.Substring(0, 3) != key) continue;
        ////        switch (k.Substring(3))
        ////        {
        ////            case "01":
        ////            case "02":
        ////            case "04":
        ////            case "06":
        ////                point++;
        ////                e.CONC_CODE = meter.MeterCostControls[k].Result == ConstHelper.CTG_HeGe ? "01" : "02"; ;
        ////                e.DETECT_ITEM_POINT = point.ToString("D2");
        ////                break;
        ////        }
        ////        sql.Add(e.ToInsertString());
        ////    }
        ////    return sql.ToArray();

        ////}


        ///// <summary>
        ///// 费率时段电能示值误差
        ///// </summary>
        //protected string[] GetMT_TIME_ERROR_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    List<string> sql = new List<string>();
        //    string key = Cus_DgnItem.费率时段示值误差;
        //    string[] keyNo = new string[] { "00601", "00602", "00603", "00604" };

        //    if (!meter.MeterDgns.ContainsKey(key))
        //        return sql.ToArray();

        //    for (int i = 0; i < 4; i++)
        //    {

        //        MT_TIME_ERROR_MET_CONC entity = new MT_TIME_ERROR_MET_CONC
        //        {
        //            DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            EQUIP_CATEG = "01",
        //            SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO,

        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),
        //            DETECT_UNIT_NO = string.Empty,
        //            POSITION_NO = meter.MD_Epitope.ToString().Trim(),
        //            BAR_CODE = meter.Barcode,
        //            DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),
        //            CONC_CODE = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : "02",
        //            WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //            HANDLE_FLAG = "0",
        //            PARA_INDEX = "1",
        //            DETECT_ITEM_POINT = "1",
        //            VOLTAGE = meter.Voltage.ToString().Trim(),

        //            PF = GetPCode("meterTestPowerFactor", "1.0"),
        //            ERR_UP = "0.01",
        //            ERR_DOWN = "-0.01",
        //            CONTROL_WAY = "电量",
        //            IR_TIME = "1",
        //            LOAD_CURRENT = GetPCode("meter_Test_CurLoad", "1.0Ib"),
        //        };

        //        string[] tmp = meter.MeterDgns[keyNo[i]].Value.Split('|');
        //        if (tmp.Length >= 6)
        //        {
        //            entity.VALUE_CONC_CODE = tmp[4];
        //            entity.TOTAL_READING_ERR = tmp[4];
        //            entity.IR_READING = (float.Parse(tmp[3]) - float.Parse(tmp[2])).ToString("F2");

        //            entity.FEE_RATIO = tmp[5];

        //        }
        //        sql.Add(entity.ToInsertString());
        //    }

        //    return sql.ToArray();

        //}

        ///// <summary>
        ///// 影响量
        ///// </summary>
        //protected string[] GetMT_INFLUENCE_QTY_MET_CONCByMt(MT_METER mtMeter, TestMeterInfo meter)
        //{
        //    if (meter.MeterSpecialErrs.Count == 0) return new string[0];

        //    List<MeterSpecialErr> spelist = new List<MeterSpecialErr>();
        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        spelist.Add(meter.MeterSpecialErrs[item]);
        //    }

        //    string[] sql = new string[spelist.Count];
        //    for (int i = 0; i < spelist.Count; i++)
        //    {
        //        MT_INFLUENCE_QTY_MET_CONC e = new MT_INFLUENCE_QTY_MET_CONC
        //        {
        //            DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO,
        //            EQUIP_CATEG = "01",
        //            SYS_NO = meter.MD_WiringMode == "单相" ? "401" : "402",

        //            DETECT_EQUIP_NO = meter.BenthNo.ToString().Trim(),
        //            DETECT_UNIT_NO = string.Empty,
        //            POSITION_NO = meter.MD_Epitope.ToString().Trim(),
        //            BAR_CODE = mtMeter.BAR_CODE,
        //            DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"),
        //            PARA_INDEX = (i + 1).ToString(),
        //            DETECT_ITEM_POINT = (i + 1).ToString(),
        //            IS_VALID = "1",
        //            CHK_CONC_CODE = spelist[i].Result.Trim() == ConstHelper.合格 ? "01" : "02",
        //            CONC_CODE = "",
        //            EFFECT_TEST_ITEM = spelist[i].Name.Trim(),

        //            WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //            HANDLE_FLAG = "0",
        //            HANDLE_DATE = "",

        //            //新加
        //            AREA_CODE = "",
        //            OPC_NO = "",
        //        };

        //        sql[i] = e.ToInsertString();
        //    }
        //    return sql;
        //}

        //#endregion
        #endregion


        /// <summary>
        /// 总结论
        /// </summary>
        protected string GetMT_DETECT_RSLTByMt(MT_METER mtMeter, TestMeterInfo meter)
        {
            MT_DETECT_RSLT e = new MT_DETECT_RSLT
            {
                //文档找不到字段
                DISPLAY_FUNC_CONC_CODE = MisDataHelper.GetFunctionShowConclusion(meter, ProjectID.显示功能),
                CONTROL_FUNC_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.控制功能),//控制功能
                METER_ERROR_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电能示值组合误差),    //电子指示显示器电能示值组合误差
                ELE_CHK_CONC_CODE = "01",//通电检查              
                PRESET_CONTENT_SET_CONC_CODE = "01",
                PRESET_CONTENT_CHECK_CONC_CODE = "01",

            };

            if (!meter.MD_MeterType.Contains("本地") || !meter.MD_MeterType.Contains("卡"))
            {
                //e.PURSE_INIT_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.电量清零);  //  更新钱包初始化字段
                //e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.电量清零);


                e.PURSE_INIT_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电量清零);  //远程
                e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电量清零); ////本地

                if (e.RESET_EQ_MET_CONC_CODE == "" || e.RESET_EQ_MET_CONC_CODE == null)
                {
                    e.PURSE_INIT_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.钱包初始化);  //远程
                    e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.钱包初始化); ////本地
                }
            }
            else
            {
                e.PURSE_INIT_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.钱包初始化);  //远程
                e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.钱包初始化);// 本地

                if (e.RESET_EQ_MET_CONC_CODE == "" || e.RESET_EQ_MET_CONC_CODE == null)
                {
                    e.PURSE_INIT_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电量清零); //远程
                    e.RESET_EQ_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电量清零); //本地
                }
            }

            ////载波通讯试验 暂时注释
            ////if (meter.MeterCarrierDatas.Count > 0)
            ////{
            ////    e.WAVE_CONC_CODE = meter.MeterCarrierDatas["P_1"].Result == ConstHelper.合格 ? "01" : "02";//载波通讯试验 id更改成为 P_1
            ////}
            ////else
            ////{
            ////    e.WAVE_CONC_CODE = "";
            ////}

            e.DETECT_TASK_NO = mtMeter.DETECT_OUT_EQUIP.DETECT_TASK_NO; //检定任务编号
            e.EQUIP_CATEG = "01"; //设备类别
            e.SYS_NO = mtMeter.DETECT_OUT_EQUIP.SYS_NO; //系统编号
            e.DETECT_EQUIP_NO = meter.BenthNo; //检定线台编号
            e.DETECT_UNIT_NO = string.Empty; //检定单元编号
            e.POSITION_NO = meter.MD_Epitope.ToString(); //表位编号
            e.BAR_CODE = mtMeter.BAR_CODE; //设备条形码
            e.DETECT_DATE = Convert.ToDateTime(meter.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss"); //检定时间
                                                                                                  //密钥更新;钱包初始化;gps对时通过的表才会认为总结论合格
            e.CONC_CODE = meter.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //检定总结论

            e.INTUIT_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.外观检查); //外观检查试验
            e.BASICERR_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.基本误差试验); //基本误差试验
            e.INIERR_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.初始固有误差试验);
            e.CONST_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电能表常数试验); //电能表常数试验
            e.STARTING_CONC_CODE = MisDataHelper.GetQiQianDongConclusion(meter, ProjectID.起动试验); //起动试验
            e.CREEPING_CONC_CODE = MisDataHelper.GetQiQianDongConclusion(meter, ProjectID.潜动试验); //潜动试验
            e.DAYERR_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.日计时误差); //日记时误差
            if (string.IsNullOrWhiteSpace(e.DAYERR_CONC_CODE))
                e.DAYERR_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.日计时误差_黑龙江);
            e.POWER_CONC_CODE = "01";//功耗
            e.VOLT_CONC_CODE = "01";        //交流电压试验 上海默认合格
            e.STANDARD_CONC_CODE = "01";    //规约一致性检查

            //e.WAVE_CONC_CODE = "01";        //载波通信测试
            //e.WAVE_CONC_CODE = MisDataHelper.GetCarrierConclusion(meter);
            //e.WAVE_CONC_CODE = meter.MeterCarrierDatas["P_1"].Result == ConstHelper.合格 ? "01" : "02";//载波通讯试验 id更改成为 P_1

            e.ERROR_CONC_CODE = MisDataHelper.GetErrorRecordConclusion(meter, ProjectID.误差变差); // 误差变差
            e.CONSIST_CONC_CODE = MisDataHelper.GetErrorRecordConclusion(meter, ProjectID.误差一致性); //误差一致性
            e.VARIATION_CONC_CODE = MisDataHelper.GetErrorRecordConclusion(meter, ProjectID.负载电流升将变差); // 升降变差

            ////电流过载 暂时注释
            ////e.OVERLOAD_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电流过载); //电流过载

            e.TS_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.时段投切); //时段投切
            e.RUNING_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.电能表常数试验); //走字 电能表常数试验
            e.PERIOD_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.需量示值误差); //需量周期误差
            e.VALUE_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.需量示值误差); //需量示值误差

            e.KEY_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.密钥更新); //密钥更新 密钥下装
            e.ESAM_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.身份认证); //费控安全认证试验
            if (!string.IsNullOrWhiteSpace(e.KEY_CONC_CODE) && string.IsNullOrWhiteSpace(e.ESAM_CONC_CODE))
            {
                e.ESAM_CONC_CODE = "01";
            }
            e.REMOTE_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.数据回抄); //费控远程数据回抄试验 645
            //e.EH_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.保电功能); //费控保电试验
            e.EH_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.远程保电); //费控保电试验
            e.EC_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.保电解除); //费控取消保电
            e.SWITCH_ON_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.远程控制); //费控合闸试验
            e.SWITCH_OUT_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.远程控制); //费控拉闸试验
            e.WARN_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.报警功能); //费控告警试验
            e.WARN_CANCEL_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.报警功能); //费控取消告警
            e.SURPLUS_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.剩余电量递减准确度); //剩余电量递减试验

            //新加
            e.INFRARED_TEST_CONC_CODE = ""; //红外通信测试结论

            //RESET_DEMAND_MET_CONC_CODE="",//需量清零
            e.RESET_DEMAND_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.需量清空);//需量清零

            //TIMING_MET_CONC_CODE="",//广播校时试验

            ////广播校时试验 暂时注释
            ////e.TIMING_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.计时功能); //广播校时试验

            e.COMMINICATE_MET_CONC_CODE = "01"; //通讯测试
            e.ADDRESS_MET_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.通讯测试); //探测表地址

            e.MULTI_INTERFACE_MET_CONC_CODE = ""; //多功能口测试
            e.LEAP_YEAR_MET_CONC = string.Empty; //闰年切换
            e.PARA_READ_MET_CONC_CODE = string.Empty; //任意数据读取
            e.PARA_SET_MET_CONC = string.Empty; // 任意数据写入

            //SETTING_CONC_CODE = string.Empty; //参数设置结论

            ////参数设置 暂时注释
            //e.SETTING_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.参数设置); //参数设置结论
            e.SETTING_CONC_CODE = "01";
            //DEVIATION_MET_CONC = "01";   //标准偏差

            ////标准偏差 暂时注释
            ////e.DEVIATION_MET_CONC = MisDataHelper.GetBasicConclusion(meter, ProjectID.标准偏差); //标准偏差

            e.GPS_CONC = MisDataHelper.GetDgnConclusion(meter, ProjectID.GPS对时); //GPS校时

            e.DETECT_PERSON = meter.Checker1.Trim(); //检定人员
            e.AUDIT_PERSON = meter.Checker2.Trim(); //审核人员
            e.WRITE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //检定线写入时间
            e.SEAL_HANDLE_FLAG = "0"; //施封线处理标记
            e.SEAL_HANDLE_DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //施封线处理时间  可能日期为空不填
            e.HANDLE_FLAG = "0"; //平台处理标记
            e.HANDLE_DATE = ""; //平台处理时间
            e.TEMP = meter.Temperature.Replace("℃", "");// + "℃"; //温度
            e.HUMIDITY = meter.Humidity.Replace("%", "");// + "%"; //湿度

            e.PRESET_PARAMET_CHECK_CONC_CODE = "01"; //预置参数检查1
            ////暂无ID
            ////e.PASSWORD_CHANGE_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.修改密码); //485密码修改
            e.PRESET_PARAMETER_SET_CONC_CODE = "01"; //预置参数设置1

            //新加
            e.RELIABILITY_CONC_CODE = ""; //可靠性验证试验
            e.MOVE_STABILITY_TEST_CONC_CODE = ""; // 动稳定试验
            e.ANTI_SEISMICTEST_CONC_CODE = ""; //抗震性能试验
            e.PRESET_PARAM2_CHECK_CONC_CODE = ""; //预置参数检查2
            e.PRESET_PARAM3_CHECK_CONC_CODE = ""; //预置参数检查3
            e.PRESET_PARAM2_SET_CONC_CODE = ""; //预置参数设置2
            e.PRESET_PARAM3_SET_CONC_CODE = ""; //预置参数设置3
            e.HARD_VERSION = ""; //硬件版本
            e.FAULT_TYPE = ""; //故障类型
            e.EVENT_REC_FUNC_CONC_CODE = ""; //事件记录功能
            e.INFLUENCE_QTY_CONC_CODE = ""; //影响量试验
            e.AUX_POW_CONC_CODE = ""; //辅助电源
            e.ALARM_FUNC_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.报警功能);//报警功能
            e.RATE_TIME_FUNC_CONC_CODE = ""; //费率和时段功能

            ////计量功能 暂时注释
            ////e.ELE_ENERGY_FUNC_CONC_CODE = MisDataHelper.GetFunctionConclusion(meter, Cus_FunctionItem.计量功能); //电能计量功能

            e.EXPIRATION_DATE = ""; //合格证有效截止日期

            //MEA_REP_CONC_CODE = "01";//测量重复性试验，偏差
            ////标准偏差 暂时注释
            ////e.MEA_REP_CONC_CODE = MisDataHelper.GetBasicConclusion(meter, ProjectID.标准偏差); //测量重复性试验(新规)

            e.CERT_NO = ""; //计量装置证书编号
            e.CERT_NO_PERIOD = ""; //计量装置证书编号有效期
            e.UNPASS_REASON = ""; //不合格原因
            e.COMM_MODE_CHG_CONC_CODE = ""; //通讯模块互换能力试验（新规）
            e.COMM_MODE_INT_CONC_CODE = ""; //通讯模块接口带载能力测试（新规）
            e.FROZEN_FUNC_CONC_CODE = ""; //冻结功能

            e.CLOCK_VALUE_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.时钟示值误差); //时钟示值误差
            ////暂无ID
            ////e.HPLC_CERT_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.HPLC芯片ID认证);
            //HPLC_CERT_CONC_CODE = "01";//HPLC芯片ID认证

            e.DISPLAY_FUNC_CONC_CODE = MisDataHelper.GetFunctionShowConclusion(meter, ProjectID.显示功能);
            e.CONTROL_FUNC_CONC_CODE = MisDataHelper.GetFkConclusion(meter, ProjectID.控制功能);//控制功能
            e.METER_ERROR_CONC_CODE = MisDataHelper.GetDgnConclusion(meter, ProjectID.电能示值组合误差);

            return e.ToInsertString();
        }

        #endregion

        /// <summary>
        /// 此函数可以添加字典，不可以修改或删除字典
        /// </summary>
        private void GetDicPCodeTable()
        {
            //获取MIS字典表信息
            //功率方向
            Dictionary<string, string> powerFlag = GetPCodeDic("powerFlag");
            PCodeTable.Add("powerFlag", powerFlag);
            //电流相别
            PCodeTable.Add("currentPhaseCode", GetPCodeDic("currentPhaseCode"));
            //电流 用后面的 meter_Test_CurLoad
            PCodeTable.Add("meterTestCurLoad", GetPCodeDic("meterTestCurLoad"));
            //功率因数
            PCodeTable.Add("itRatedLoadPf", GetPCodeDic("itRatedLoadPf"));
            //功率因数
            PCodeTable.Add("meterTestPowerFactor", GetPCodeDic("meterTestPowerFactor"));
            //试验电压
            PCodeTable.Add("meter_Test_Volt", GetPCodeDic("meter_Test_Volt"));
            //试验电压
            PCodeTable.Add("meterTestVolt", GetPCodeDic("meterTestVolt"));
            //额定电压
            PCodeTable.Add("meterVolt", GetPCodeDic("meterVolt"));
            //额定电流
            PCodeTable.Add("meterRcSort", GetPCodeDic("meterRcSort"));

            //经互感器
            PCodeTable.Add("conMode", GetPCodeDic("conMode"));

            //费率
            PCodeTable.Add("tari_ff", GetPCodeDic("tari_ff"));
            PCodeTable.Add("fee", GetPCodeDic("fee"));
            //等级
            PCodeTable.Add("meterAccuracy", GetPCodeDic("meterAccuracy"));
            //电表类型
            PCodeTable.Add("meterTypeCode", GetPCodeDic("meterTypeCode"));
            //电表常数
            PCodeTable.Add("meterConstCode", GetPCodeDic("meterConstCode"));
            //接线方式
            PCodeTable.Add("wiringMode", GetPCodeDic("wiringMode"));
            //电表型号
            PCodeTable.Add("meterModelNo", GetPCodeDic("meterModelNo"));
            //厂家
            PCodeTable.Add("meterFacturer", GetPCodeDic("meterFacturer"));
            //频率
            PCodeTable.Add("meter_Test_Freq", GetPCodeDic("meter_Test_Freq"));
            //密钥状态
            PCodeTable.Add("secretKeyStatus", GetPCodeDic("secretKeyStatus"));
            //密钥类型
            PCodeTable.Add("secretKeyType", GetPCodeDic("secretKeyType"));
            //走字方法
            PCodeTable.Add("meterTestCtrlMode", GetPCodeDic("meterTestCtrlMode"));
            //载波模块
            PCodeTable.Add("LocalChipManu", GetPCodeDic("LocalChipManu"));

            //电能表通讯规约
            PCodeTable.Add("commProtocol", GetPCodeDic("commProtocol"));

            PCodeTable.Add("meter_Test_CurLoad", GetPCodeDic("meter_Test_CurLoad"));//电流
            PCodeTable.Add("meterTestFreq", GetPCodeDic("meterTestFreq"));
            PCodeTable.Add("meterSort", GetPCodeDic("meterSort"));//类别
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
            DataTable dr = ExecuteReader(sql);
            foreach (DataRow r in dr.Rows)
            {
                string value = r["value"].ToString().Trim();
                string name = r["name"].ToString().Trim();
                if (value.Length > 0)
                {
                    if (!dic.ContainsKey(value))
                        dic.Add(value, name);
                }
            }
            return dic;
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

        private PowerWay GetGLFXFromString(string strValue)
        {
            PowerWay fx;
            switch (strValue)
            {
                case "正向有功":
                    fx = PowerWay.正向有功;
                    break;
                case "正向无功":
                    fx = PowerWay.正向无功;
                    break;
                case "反向有功":
                    fx = PowerWay.反向有功;
                    break;
                case "反向无功":
                    fx = PowerWay.反向无功;
                    break;
                case "第一象限无功":
                    fx = PowerWay.第一象限无功;
                    break;
                case "第二象限无功":
                    fx = PowerWay.第二象限无功;
                    break;
                case "第三象限无功":
                    fx = PowerWay.第三象限无功;
                    break;
                case "第四象限无功":
                    fx = PowerWay.第四象限无功;
                    break;
                default:
                    fx = PowerWay.正向有功;
                    break;
            }
            return fx;
        }

        private Cus_PowerYuanJian GetYuanJianFromString(string value)
        {
            Cus_PowerYuanJian yj = Cus_PowerYuanJian.H;
            switch (value)
            {
                case "ABC":
                case "AC":
                    yj = Cus_PowerYuanJian.H;
                    break;
                case "A":
                    yj = Cus_PowerYuanJian.A;
                    break;
                case "B":
                    yj = Cus_PowerYuanJian.B;
                    break;
                case "C":
                    yj = Cus_PowerYuanJian.C;
                    break;
            }
            return yj;
        }

        /// <summary>
        /// 获取最大需量数据
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        private string[] GetXLData(TestMeterInfo meter)
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
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[3] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[3] = "";
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
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[8] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[8] = "";
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
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[8] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[8] = "";
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
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[13] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[13] = "";
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
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[13] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[13] = "";
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

            //需量周期误差
            key = ItemKey;
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[15] = maxdata1[3];
                xlData[16] = maxdata1[4];
                xlData[17] = maxdata1[5];
                if (maxdata1.Length > 3)
                    xlData[18] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[18] = "";
            }
            else
            {
                xlData[15] = "";
                xlData[16] = "";
                xlData[17] = "";
                xlData[18] = "";
            }
            return xlData;
        }

        public bool SchemeDown(TestMeterInfo meter, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            schemeName = "";
            Schema = new Dictionary<string, SchemaNode>();
            if (meter == null) return false;
            return SchemeDown(meter.MD_BarCode, out schemeName, out Schema);
        }
    }
}
