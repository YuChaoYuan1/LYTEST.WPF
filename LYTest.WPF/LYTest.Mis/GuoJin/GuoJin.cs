using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.DAL.Config;
using LYTest.Mis.Common;
using LYTest.Mis.GuoJin.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LYTest.Mis.GuoJin
{
    public class GuoJin : MySQLHelper, IMis
    {
        public GuoJin(string ip, int port, string dataSource, string userId, string pwd, string url)
        {
            this.Ip = ip;
            this.Port = port;
            this.Database = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;
        }

        private static string TaskId = "";//任务单ID号
        private static string[] MeterInfo = new string[20];//试品基本信息
        //private static string[] DetectRslt = new string[1000];//中间库信息
        private static readonly Dictionary<string, Dictionary<string, string>> PCodeTable = new Dictionary<string, Dictionary<string, string>>();
        private static string taskNo = "";

        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            if (string.IsNullOrEmpty(barcode)) return false;

            if (PCodeTable.Count <= 0)
                GetPCodeTable();

            string sql = $"select * from m_qt_meter_info where m_qt_meter_info.bar_code='{barcode}' order by made_date ";
            DataTable dt = ExecuteReader(sql);

            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show($"不存在条形码为({barcode})的记录");
                return false;
            }

            DataRow dr = dt.Rows[dt.Rows.Count - 1];

            meter = new TestMeterInfo
            {
                MD_TaskNo = dr["DETECT_TASK_NO"].ToString().Trim(),//任务编号
                MD_BarCode = dr["BAR_CODE"].ToString().Trim(),//条形码

                MD_MeterType = GetPName("testObjType", dr["TYPE_CODE"].ToString().Trim()),//表类型
                MD_MeterModel = GetPName("meterModelNo", dr["MODEL_CODE"].ToString().Trim())//表型号
            };

            #region 接线方式
            string wireMode = GetPName("wiringMode", dr["WIRING_CODE"].ToString().Trim()); // 接线方式
            switch (wireMode)
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

            #region 电压
            string ubtmp = GetPName("meterVolt", dr["VOLT_CODE"].ToString().Trim()); //电压
            if (ubtmp.IndexOf("57.7") >= 0)
                meter.MD_UB = 57.7f;
            else if (ubtmp.IndexOf("100") >= 0)
                meter.MD_UB = 100;
            else if (ubtmp.IndexOf("220") >= 0)
                meter.MD_UB = 220;
            else
            {
                if (meter.MD_WiringMode == "单相")
                    meter.MD_UB = 220;
                else
                    meter.MD_UB = 57.7f;
            }
            #endregion

            #region 电流
            string ibtmp = GetPName("meterRcSort", dr["RATED_CURRENT"].ToString().Trim()); //电流
            meter.MD_UA = ibtmp.Trim('A');
            if (meter.MD_UA == "")
            {
                meter.MD_UA = "1.5(6)";
            }
            #endregion

            #region 准确度等级
            string djP = GetPName("meterAccuracy", dr["AP_PRE_LEVEL_CODE"].ToString().Trim()); //有功准确度等级
            string djQ = GetPName("meterAccuracy", dr["RP_PRE_LEVEL_CODE"].ToString().Trim()); //无功准确度等级
            if (djP == "") djP = "1.0";
            if (meter.MD_WiringMode == "单相")     //假如有功表等级和无功表等级不一致
            {
                meter.MD_Grane = djP;
            }
            else
            {
                if (djQ == "")
                    meter.MD_Grane = djP + "(" + djP + ")";
                else
                    meter.MD_Grane = djP + "(" + djQ + ")";
            }
            #endregion

            #region 表等级
            //string accurP = GetPName("meterAccuracy", dr["AP_PRE_LEVEL_CODE"].ToString().Trim());//有功准确度等级
            //string accurQ = GetPName("meterAccuracy", dr["RP_PRE_LEVEL_CODE"].ToString().Trim());//无功准确度等级
            //string accurDJ = accurP;
            //if (accurQ != "")     //假如有功表等级和无功表等级不一致
            //    accurDJ = accurP + "(" + accurQ + ")";
            //meter.MD_Grane = accurDJ;
            #endregion

            #region 常数
            //string ConstP = GetPName("meterConstCode", dr["CONST_CODE"].ToString().Trim());//有功常数
            //string ConstQ = GetPName("meterConstCode", dr["RP_CONSTANT"].ToString().Trim());//无功常数
            //string ConstCS = ConstP;
            //if (ConstQ != "")     //假如有功表等级和无功表等级不一致
            //    ConstCS = ConstP + "(" + ConstQ + ")";
            //meter.MD_Constant = ConstCS;
            #endregion

            #region 常数
            string csP = GetPName("meterConstCode", dr["CONST_CODE"].ToString().Trim());//有功常数
            string csQ = GetPName("meterConstCode", dr["RP_CONSTANT"].ToString().Trim());//无功常数
            if (csP == "") csP = "400";
            if (meter.MD_WiringMode == "单相")     //假如有功表等级和无功表等级不一致
            {
                meter.MD_Constant = csP;
            }
            else
            {
                if (csQ == "")
                    meter.MD_Constant = csP + "(" + csP + ")";
                else
                    meter.MD_Constant = csP + "(" + csQ + ")";
            }
            #endregion

            meter.MD_Factory = GetPName("MadeSupp", dr["MANUFACTURER"].ToString().Trim());//厂家


            #region 频率
            string pl = GetPName("meterFreq", dr["FREQ_CODE"].ToString().Trim());//频率
            if (pl == "") pl = "0";
            if (pl.IndexOf("Hz") != -1)
                meter.MD_Frequency = int.Parse(pl.Replace("Hz", ""));
            else
                meter.MD_Frequency = int.Parse(pl.Trim());
            #endregion

            #region 接入方式
            string Hgq = GetPName("conMode", dr["CON_MODE"].ToString().Trim());
            if (Hgq == "") Hgq = "直接接入";
            switch (Hgq)
            {
                case "直接接入":
                    meter.MD_ConnectionFlag = "直接式";
                    break;
                case "经互感器接入":
                    meter.MD_ConnectionFlag = "互感式";
                    break;
                default:
                    meter.MD_ConnectionFlag = "直接式";
                    break;
            }

            //meter.MD_ConnectionFlag = "直接式";
            //string hgq = GetPName("conMode", dr["CON_MODE"].ToString().Trim());
            //if (hgq == "经互感器接入")
            //    meter.MD_ConnectionFlag = "互感式";
            #endregion

            #region 规程和协议
            meter.MD_ProtocolName = "CDLT698";      //协议 默认 CDLT698
            meter.DgnProtocol = null;

            meter.MD_JJGC = "JJG596-2012";         //规程 默认 JJG596-2012 
            string djIR46 = "ABCDE";

            if (djIR46.IndexOf(djP) != -1)
            {
                meter.MD_ProtocolName = "CDLT698";    //协议   
                meter.MD_JJGC = "IR46";                    //规程
            }
            #endregion

            meter.Seal1 = "";                                                 //铅封1,暂时置空
            meter.Seal2 = "";                                                 //铅封2,暂时置空
            meter.Seal3 = "";                                                 //铅封3,暂时置空

            return true;
        }

        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }

        public bool Update(TestMeterInfo meter)
        {
            List<string> sqlList = new List<string>();
            List<string> delList = new List<string>();
            M_DETECT_RSLT m_DETECT_RSLT = new M_DETECT_RSLT();

            int WCBC = 0;//判断误差变差和负载电流升降变差是否有数据
            taskNo = meter.MD_TaskNo;
            meter.Other5 = taskNo;
            meter.MD_BarCode = meter.MD_BarCode.Trim();

            if (PCodeTable.Count <= 0)
                GetPCodeTable();

            TaskId = GetTestbarcode(meter.MD_BarCode, meter.MD_TaskNo, out _);

            MeterInfo = GetMeterInfo(meter.MD_BarCode);

            #region 公共值-国金
            //--------------------------------公共值---------------------------------
            m_DETECT_RSLT.EQUIP_ID = MeterInfo[0]; //设备ID
            m_DETECT_RSLT.BAR_CODE = meter.MD_BarCode;//条形码
            m_DETECT_RSLT.EQUIP_NAME = ""; //设备名称
            m_DETECT_RSLT.DETECT_TASK_NO = meter.MD_TaskNo; //检定单编号
            m_DETECT_RSLT.EQUIP_CATEG = MeterInfo[3]; //设备大类
            m_DETECT_RSLT.EQUIP_TYPE = MeterInfo[4]; //设备小类

            m_DETECT_RSLT.SYS_NO = ""; //系统编号
            m_DETECT_RSLT.DETECT_EQUIP_NO = meter.BenthNo.Trim(); // 检定线/台体编号
            m_DETECT_RSLT.DETECT_UNIT_NO = "";                    //检定单元编号
            m_DETECT_RSLT.POSITION_NO = meter.MD_Epitope.ToString();  //表位编号
            m_DETECT_RSLT.VOLT_DATE = meter.VerifyDate; // 检定/校准日期

            m_DETECT_RSLT.DATA_SOURCE = "02"; //数据来源
            m_DETECT_RSLT.DATA_TYPE = "02"; //数据类型
            m_DETECT_RSLT.ENVIRON_TEMPER = meter.Temperature; //温度
            m_DETECT_RSLT.RELATIVE_HUM = meter.Humidity; //相对湿度

            m_DETECT_RSLT.CON_MODE = MeterInfo[15]; //接入方式 
            m_DETECT_RSLT.AP_LEVEL = MeterInfo[9]; //有功等级
            m_DETECT_RSLT.RP_LEVEL = MeterInfo[10]; //无功等级

            m_DETECT_RSLT.TEST_USER_ID = meter.CheckerNo1; //检定员ID
            m_DETECT_RSLT.TEST_USER_NAME = meter.Checker1; //检定员

            m_DETECT_RSLT.BASIC_ID = meter.Meter_ID.ToString(); //试验基础信息ID

            m_DETECT_RSLT.WRITE_DATE = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); //写入时间
            m_DETECT_RSLT.HANDLE_FLAG = "0"; //处理标记
            m_DETECT_RSLT.HANDLE_DATE = DateTime.Now.ToString(); //处理时间

            string zjxfs = meter.MD_WiringMode == "三相三线" ? "AC" : "ABC";
            int dd = 100000;

            //电压规格
            string Un = "Un";
            if (meter.MD_JJGC == "IR46")
            {
                Un = "Unom";
            }
            //-----------------------------------------------------------------------
            #endregion

            #region 全部试验-国金

            int iIndex = 1;
            string itemId = "";
            List<string> sqls;
            #region 初始固有误差试验-国金 *20
            if (meter.MeterInitialError.Count > 0)
            {
                int indexcs = 1;

                foreach (string key in meter.MeterInitialError.Keys)
                {
                    MeterInitialError data = meter.MeterInitialError[key];

                    itemId = "0101";
                    if (data.GLFX == "反向有功")
                    {
                        itemId = "0102";
                    }
                    else if (data.GLFX == "正向无功" || data.GLFX == "反向无功")
                    {
                        itemId = "0103";
                    }

                    if (TaskId.IndexOf(itemId) != -1)
                    {

                        #region 初始固有误差试验 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                        if (itemId == "0101")
                        {
                            m_DETECT_RSLT.ITEM_NAME = "初始固有误差-有功正潮流误差试验"; //试验项名称
                            //IsYouGong = true;
                        }
                        else if (itemId == "0102")
                        {
                            m_DETECT_RSLT.ITEM_NAME = "初始固有误差-有功负潮流误差试验"; //试验项名称
                            //IsYouGong = true;
                        }
                        else if (itemId == "0103")
                        {
                            m_DETECT_RSLT.ITEM_NAME = "初始固有误差-无功误差试验"; //试验项名称  
                            //IsYouGong = false;
                        }

                        m_DETECT_RSLT.DETECT_ITEM_POINT = indexcs.ToString(); //检定点的序号

                        if (meter.MD_WiringMode == "单相")
                        {
                            m_DETECT_RSLT.TEST_GROUP = "平衡负载时的初始固有误差(有功正潮流)"; //试验分组 *20
                            m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载时的初始固有误差(有功正潮流)";//试验分项
                        }
                        else
                        {
                            if (data.YJ == "H")
                            {
                                if (data.GLFX == "正向有功")
                                {
                                    m_DETECT_RSLT.TEST_GROUP = "平衡负载时的初始固有误差(有功正潮流)"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载时的初始固有误差(有功正潮流)";//试验分项
                                }
                                else if (data.GLFX == "反向有功")
                                {
                                    m_DETECT_RSLT.TEST_GROUP = "平衡负载时的初始固有误差(有功负潮流)"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载时的初始固有误差(有功负潮流)";//试验分项
                                }
                                else if (data.GLFX == "正向无功" || data.GLFX == "反向无功")
                                {
                                    m_DETECT_RSLT.TEST_GROUP = "平衡负载时的初始固有误差(无功)"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载时的初始固有误差(无功)";//试验分项
                                }
                            }
                            else
                            {
                                if (data.GLFX == "正向有功")
                                {
                                    m_DETECT_RSLT.TEST_GROUP = "不平衡负载时的初始固有误差(有功正潮流)"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载时的初始固有误差(有功正潮流)";//试验分项
                                }
                                else if (data.GLFX == "反向有功")
                                {
                                    m_DETECT_RSLT.TEST_GROUP = "不平衡负载时的初始固有误差(有功负潮流)"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载时的初始固有误差(有功负潮流)";//试验分项
                                }
                                else if (data.GLFX == "正向无功" || data.GLFX == "反向无功")
                                {
                                    m_DETECT_RSLT.TEST_GROUP = "不平衡负载时的初始固有误差(无功)"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载时的初始固有误差(无功)";//试验分项
                                }
                            }
                        }

                        if (data.YJ == "H") data.YJ = zjxfs;

                        m_DETECT_RSLT.IABC = data.YJ; //相别 *20
                        m_DETECT_RSLT.PF = data.GLYS; //功率因数 *20
                        m_DETECT_RSLT.LOAD_CURRENT = data.IbX; //负载电流 *20
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = data.GLFX; //功率方向

                        string[] wc = data.WcMore.Split('|');


                        string[] level = Number.GetDj(meter.MD_Grane);
                        float xsws = 1;
                        switch (level[0])
                        {
                            case "A":
                            case "B":
                                xsws = 1;
                                break;
                            case "C":
                            case "D":
                                xsws = 2;
                                break;
                        }

                        //if (wc[8].Split('.')[1].Length == xsws)

                        if (data.UpWCPJ.Length > 8) data.UpWCPJ = data.UpWCPJ.Substring(0, 8);//误差上升平均值
                        if (data.DownWCPJ.Length > 8) data.DownWCPJ = data.DownWCPJ.Substring(0, 8);//误差下降平均值

                        if (data.UpWCHZ.Length > 8) data.UpWCHZ = data.UpWCHZ.Substring(0, 8);//误差上升化整值
                        if (data.DownWCHZ.Length > 8) data.DownWCHZ = data.DownWCHZ.Substring(0, 8);//误差下降化整值

                        m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(wc[8]).ToString("f" + xsws); //误差化整值 *20
                        //m_DETECT_RSLT.INT_CONVERT_ERR = wc[8]; //误差化整值 *20

                        //if (data.GLYS.Trim() == "0.25L")
                        //    m_DETECT_RSLT.ERR_ABS = "±" + (2.5 * 0.6);//误差限值 *20
                        //else
                        m_DETECT_RSLT.ERR_ABS = "±" + data.UpLimit.Trim().Split('|')[0];//误差限值 *20

                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论 

                        m_DETECT_RSLT.DATA_ITEM1 = wc[0]; //误差1
                        m_DETECT_RSLT.DATA_ITEM2 = wc[1]; //误差2
                        m_DETECT_RSLT.DATA_ITEM3 = wc[4]; //误差3
                        m_DETECT_RSLT.DATA_ITEM4 = wc[5]; //误差4
                        m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                        m_DETECT_RSLT.DATA_ITEM6 = data.UpWCPJ; //误差平均值
                                                                //m_DETECT_RSLT.DATA_ITEM7 = wc[8]; //变差原始值，没有可不填
                        m_DETECT_RSLT.DATA_ITEM7 = ""; //变差原始值，没有可不填
                        m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                        m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                        m_DETECT_RSLT.DATA_ITEM10 = data.GLFX; //试验条件
                        m_DETECT_RSLT.DATA_ITEM11 = ""; //试验要求
                        m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后

                        #region 初始固有误差试验 指标项 空值-国金
                        #region 初始固有误差试验 旧数据
                        //if (qh == 0)
                        //{
                        //    m_DETECT_RSLT.DATA_ITEM1 = wc[0]; //误差1
                        //    m_DETECT_RSLT.DATA_ITEM2 = wc[1]; //误差2
                        //    m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                        //    m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                        //    m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                        //    m_DETECT_RSLT.DATA_ITEM6 = data.UpWCPJ; //误差平均值
                        //    //m_DETECT_RSLT.DATA_ITEM7 = wc[8]; //变差原始值，没有可不填
                        //    m_DETECT_RSLT.DATA_ITEM7 = ""; //变差原始值，没有可不填
                        //    m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                        //    m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                        //    m_DETECT_RSLT.DATA_ITEM10 = data.GLFX; //试验条件
                        //    m_DETECT_RSLT.DATA_ITEM11 = m_DETECT_RSLT.TEST_CATEGORIES; //试验要求
                        //    m_DETECT_RSLT.DATA_ITEM12 = "影响前"; //影响量前后
                        //}
                        //else
                        //{
                        //    m_DETECT_RSLT.DATA_ITEM1 = wc[4]; //误差1
                        //    m_DETECT_RSLT.DATA_ITEM2 = wc[5]; //误差2
                        //    m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                        //    m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                        //    m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                        //    m_DETECT_RSLT.DATA_ITEM6 = data.DownWCPJ; //误差平均值
                        //    //m_DETECT_RSLT.DATA_ITEM7 = wc[8]; //变差原始值，没有可不填
                        //    m_DETECT_RSLT.DATA_ITEM7 = ""; //变差原始值，没有可不填
                        //    m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                        //    m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                        //    m_DETECT_RSLT.DATA_ITEM10 = data.GLFX; //试验条件 //方向
                        //    m_DETECT_RSLT.DATA_ITEM11 = m_DETECT_RSLT.TEST_CATEGORIES; //试验要求
                        //    m_DETECT_RSLT.DATA_ITEM12 = "影响后"; //影响量前后
                        //}

                        //m_DETECT_RSLT.DATA_ITEM1 = wc[0]; //误差1，根据实测个数传递（升1）
                        //m_DETECT_RSLT.DATA_ITEM2 = wc[1]; //误差2，根据实测个数传递（升2）
                        //m_DETECT_RSLT.DATA_ITEM3 = wc[4]; //误差3，根据实测个数传递（降1）
                        //m_DETECT_RSLT.DATA_ITEM4 = wc[5]; //误差4，根据实测个数传递（降2）
                        //m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5，根据实测个数传递
                        //m_DETECT_RSLT.DATA_ITEM6 = data.UpWCPJ + "|" + data.DownWCPJ; //误差平均值
                        //m_DETECT_RSLT.DATA_ITEM7 = wc[8]; //变差原始值，没有可不填
                        //m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                        //m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                        //m_DETECT_RSLT.DATA_ITEM10 = data.GLFX; //试验项目 //方向
                        //m_DETECT_RSLT.DATA_ITEM11 = m_DETECT_RSLT.TEST_CATEGORIES; //技术要求说明
                        //m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后
                        #endregion
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = key;
                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                        indexcs++;

                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }

                    }
                }
            }
            #endregion

            #region 启动潜动试验-国金 *13 *20
            if (meter.MeterQdQids.Count > 0)
            {
                iIndex = 1;
                itemId = "";
                //IsYouGong = true;

                foreach (string key in meter.MeterQdQids.Keys)
                {
                    Dictionary<string, MeterQdQid> data = meter.MeterQdQids;

                    #region 启动-国金 *13 *20
                    if (key.Split('_')[0] == ProjectID.起动试验)
                    {
                        //itemId = Item_ID["起动试验"];//008

                        itemId = "008";
                        if (meter.MD_JJGC == "IR46")
                        {
                            itemId = "0104";
                        }
                        double FlIb = 0;
                        Regex reg = new Regex("(?<ib>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
                        Match match = reg.Match(meter.MD_UA);
                        if (match.Groups["ib"].Value.Length < 1)
                            FlIb = 0;

                        FlIb = double.Parse(match.Groups["ib"].Value);
                        FlIb = double.Parse(data[key].Current) / FlIb;

                        string ibstr = FlIb + "Ib";

                        List<string> splitChar = new List<string>(new string[] { "A", "B", "C", "D", "E" });
                        if (splitChar.Contains(meter.MD_Grane))
                        {
                            ibstr = FlIb + "Itr";
                        }

                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 启动私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID                            
                            m_DETECT_RSLT.ITEM_NAME = "起动试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "起动试验"; //试验分组 *13 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = data[key].Name; //试验分项
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数 *13 *20
                            m_DETECT_RSLT.LOAD_CURRENT = ibstr; //负载电流 *13 *20
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = data[key].PowerWay; //功率方向

                            //float meterLevel = MeterLevel(meter, IsYouGong);

                            //int xsws = 1;
                            //if (meterLevel <= 1)
                            //{
                            //    xsws = 2;
                            //}

                            //m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(data[key].ErrorValue).ToString("f" + xsws); //误差化整值 *20
                            m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(data[key].ErrorValue).ToString("f2"); //误差化整值 *20
                            m_DETECT_RSLT.ERR_ABS = "±" + data[key].QDLimit; //误差限值 *20
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论 *13

                            string fx = "正潮流";
                            if (data[key].PowerWay.IndexOf("正向") != -1)
                            {
                                fx = "正潮流";
                            }
                            else
                            {
                                fx = "负潮流";
                            }

                            m_DETECT_RSLT.DATA_ITEM1 = fx; //方向 *13 *20
                            m_DETECT_RSLT.DATA_ITEM2 = "能起动并连续记录"; //试验要求 *13 *20

                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.DATA_ITEM3 = data[key].ErrorValue; //误差1 
                                m_DETECT_RSLT.DATA_ITEM4 = data[key].ErrorValue; //误差2 
                                m_DETECT_RSLT.DATA_ITEM5 = data[key].ErrorValue; //平均值
                                m_DETECT_RSLT.DATA_ITEM6 = data[key].PushTime1; //脉冲间隔1
                                m_DETECT_RSLT.DATA_ITEM7 = data[key].PushTime2; //脉冲间隔2
                            }
                            else
                            {
                                m_DETECT_RSLT.DATA_ITEM3 = "1"; //实测脉冲个数
                                m_DETECT_RSLT.DATA_ITEM4 = data[key].ActiveTime; //实测脉冲间隔(s)
                                m_DETECT_RSLT.DATA_ITEM5 = data[key].StandartTime; //允许测脉冲间隔(s)
                                m_DETECT_RSLT.DATA_ITEM6 = data[key].ErrorValue; //启动误差(%)
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                            }

                            #region 启动 指标项 空值-国金

                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = key;

                            #endregion                         

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : meter.MeterQdQids[key].Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                    #endregion

                    #region 潜动-国金 *13 *20
                    if (key.Split('_')[0] == ProjectID.潜动试验)
                    {
                        //itemId = Item_ID["潜动试验"]; //009

                        itemId = "009";
                        if (meter.MD_JJGC == "IR46")
                        {
                            itemId = "0105";
                        }
                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 潜动私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "潜动试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "潜动试验"; //试验分组 *13 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = data[key].Name; //试验分项
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = ""; //负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = data[key].PowerWay; //功率方向

                            m_DETECT_RSLT.INT_CONVERT_ERR = ""; //误差化整值
                            m_DETECT_RSLT.ERR_ABS = ""; //误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                            m_DETECT_RSLT.DATA_ITEM1 = "110%" + Un; //电压 *13 *20
                            m_DETECT_RSLT.DATA_ITEM2 = "在规定时间内不产生多于一个脉冲"; //试验要求 *13 *20
                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.DATA_ITEM3 = "0"; //判定脉冲个数
                            }
                            else
                            {
                                m_DETECT_RSLT.DATA_ITEM3 = data[key].StandartTime; //理论试验时间
                            }

                            m_DETECT_RSLT.DATA_ITEM4 = data[key].ActiveTime; //实际试验时间
                            m_DETECT_RSLT.DATA_ITEM5 = "0"; //判定脉冲个数
                            m_DETECT_RSLT.DATA_ITEM6 = data[key].Pulse; //实测脉冲个数


                            #region 潜动 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = key;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ?
                                "01" : meter.MeterQdQids[key].Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region 基本误差试验-国金 *13
            if (meter.MeterErrors.Count > 0)
            {
                iIndex = 0;

                foreach (string key in meter.MeterErrors.Keys)
                {
                    if (key.Split('_')[0] == ProjectID.基本误差试验)
                    {
                        MeterError data = meter.MeterErrors[key];

                        string[] wc = data.WCData.Split('|');

                        itemId = "004";
                        if (data.GLFX == "正向无功" || data.GLFX == "反向无功")
                        {
                            itemId = "005";
                        }

                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 基本误差试验 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                            if (itemId == "004")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "有功基本误差试验"; //试验项名称  
                            }
                            else if (itemId == "005")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "无功基本误差试验"; //试验项名称  
                            }

                            if (meter.MD_WiringMode == "单相")
                            {
                                m_DETECT_RSLT.TEST_GROUP = "平衡负载基本误差(正向)"; //试验分组
                                m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载基本误差(正向)";//试验分项
                            }
                            else
                            {
                                if (data.YJ == "H")
                                {
                                    if (data.GLFX == "正向有功")
                                    {
                                        m_DETECT_RSLT.TEST_GROUP = "平衡负载基本误差(正向)"; //试验分组
                                        m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载基本误差(正向)";//试验分项

                                    }
                                    else if (data.GLFX == "反向有功")
                                    {
                                        m_DETECT_RSLT.TEST_GROUP = "平衡负载基本误差(反向)"; //试验分组
                                        m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载基本误差(反向)";//试验分项
                                    }
                                    else if (data.GLFX == "正向无功" || data.GLFX == "反向无功")
                                    {
                                        m_DETECT_RSLT.TEST_GROUP = "三相表：平衡负载时的基本误差试验(无功）"; //试验分组
                                        m_DETECT_RSLT.TEST_CATEGORIES = "三相表：平衡负载时的基本误差试验(无功）";//试验分项
                                    }
                                }
                                else
                                {
                                    if (data.GLFX == "正向有功")
                                    {
                                        m_DETECT_RSLT.TEST_GROUP = "不平衡负载基本误差(正向)"; //试验分组
                                        m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载基本误差(正向)";//试验分项

                                    }
                                    else if (data.GLFX == "反向有功")
                                    {
                                        m_DETECT_RSLT.TEST_GROUP = "不平衡负载基本误差(反向)"; //试验分组
                                        m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载基本误差(反向)";//试验分项
                                    }
                                    else if (data.GLFX == "正向无功" || data.GLFX == "反向无功")
                                    {
                                        m_DETECT_RSLT.TEST_GROUP = "三相表：不平衡负载时的基本误差试验(无功)"; //试验分组
                                        m_DETECT_RSLT.TEST_CATEGORIES = "三相表：不平衡负载时的基本误差试验(无功）";//试验分项
                                    }
                                }
                            }

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            if (data.YJ == "H") data.YJ = zjxfs;

                            m_DETECT_RSLT.IABC = zjxfs; //相别 *13
                            m_DETECT_RSLT.PF = data.GLYS.Trim(); //功率因数 *13
                            m_DETECT_RSLT.LOAD_CURRENT = data.IbX; //负载电流 *13
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = data.GLFX; //功率方向

                            if (data.WCHZ.Length > 8) data.WCHZ = data.WCHZ.Substring(0, 8);

                            m_DETECT_RSLT.INT_CONVERT_ERR = data.WCHZ;//误差化整值 *13
                            m_DETECT_RSLT.ERR_ABS = "±" + data.Limit; //误差限值 *13
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = data.Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                            if (data.WCPC.Length > 8) data.WCPC = data.WCPC.Substring(0, 8);

                            m_DETECT_RSLT.DATA_ITEM1 = data.WCValue; //误差平均值
                            m_DETECT_RSLT.DATA_ITEM2 = wc[0]; //误差1，根据实测个数传递
                            m_DETECT_RSLT.DATA_ITEM3 = wc[1]; //误差2，根据实测个数传递
                            m_DETECT_RSLT.DATA_ITEM4 = "50"; //频率值
                            m_DETECT_RSLT.DATA_ITEM5 = Un; //电压值
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = data.WCPC; //化整值

                            #region 基本误差试验 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = key;
                            #endregion


                            m_DETECT_RSLT.CHK_CONC_CODE = data.Result == ConstHelper.合格 ? "01" : data.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            iIndex++;

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 电能表常试验(走字试验)-国金 *13 *20
            if (meter.MeterZZErrors.Count > 0)
            {
                iIndex = 1;
                DataTable dtKeys = new DataTable();
                dtKeys.Columns.Add("Keys", typeof(string));
                dtKeys.Columns.Add("PrjId", typeof(string));

                //itemId = Item_ID["常数试验"];

                itemId = "007";
                if (meter.MD_JJGC == "IR46")
                {
                    itemId = "0106";
                }

                if (TaskId.IndexOf(itemId) != -1)
                {
                    foreach (string key in meter.MeterZZErrors.Keys)
                    {
                        MeterZZError MeterError = meter.MeterZZErrors[key];
                        dtKeys.Rows.Add(key, MeterError.PrjID);
                    }

                    DataRow[] Rows = dtKeys.Select("Keys <>'' and PrjId <> ''", "PrjId asc");

                    for (int i = 0; i < Rows.Length; i++)
                    {
                        MeterZZError data = meter.MeterZZErrors[Rows[i][0].ToString()];

                        //string[] dj = Number.GetDj(meter.MD_Grane);

                        #region 常数（走字）私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_NAME = "常数试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "常数试验"; //试验分组 *20
                            m_DETECT_RSLT.TEST_CATEGORIES = "常数试验"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_NAME = "电能表常数试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电能表常数试验"; //试验分组 *13
                            m_DETECT_RSLT.TEST_CATEGORIES = "电能表常数试验"; //试验分项
                        }

                        m_DETECT_RSLT.DETECT_ITEM_POINT = (i + iIndex).ToString(); //检定点的序号

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = data.GLYS; //功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = data.IbXString; //负载电流 *13
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = data.PowerWay; //功率方向

                        m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(data.ErrorValue).ToString("f2"); //误差化整值
                        m_DETECT_RSLT.ERR_ABS = ""; //误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        string[] Constant = meter.MD_Constant.Split('(');
                        string countStr;
                        if (Constant.Length < 2)
                        {
                            countStr = Constant[0] + "imp/kWh";
                        }
                        else
                        {
                            countStr = Constant[0] + "imp/kWh，" + Constant[0] + "imp/kvarh";
                        }
                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = countStr; //表常数
                        }
                        else
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "测试输出与显示器指示之间的关系，应与铭牌标志一致"; //试验要求 *13 
                        }

                        m_DETECT_RSLT.DATA_ITEM2 = data.Fl; //费率fee;
                        m_DETECT_RSLT.DATA_ITEM3 = data.PowerStart.ToString(); //起码
                        m_DETECT_RSLT.DATA_ITEM4 = data.PowerEnd.ToString(); //止码
                        m_DETECT_RSLT.DATA_ITEM5 = data.WarkPower.ToString().Trim(); //电量差
                        m_DETECT_RSLT.DATA_ITEM6 = float.Parse(data.ErrorValue).ToString("f2"); //误差
                        m_DETECT_RSLT.DATA_ITEM10 = "测试输出与显示器指示之间的关系，应与铭牌标志一致"; //试验要求 *20
                        m_DETECT_RSLT.DATA_ITEM11 = "测试输出与显示器指示之间的关系，应与铭牌标志一致"; //试验要求 *20
                        #region 常数（走字） 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";

                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = data.PrjID;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
            }
            #endregion

            #region 电子指示显示器电能示值组合误差试验-国金 *13 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.电能示值组合误差;

                if (meter.MeterDgns.ContainsKey(ItemKey))
                {
                    iIndex = 1;

                    itemId = "012";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0107";
                    }
                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        string[] value = meter.MeterDgns[ItemKey].Value.Split('|');
                        string testValue = meter.MeterDgns[ItemKey].TestValue;
                        string[] flQM = value[3].Split(',');
                        string[] flZM = value[4].Split(',');
                        string[] flvslue = value[5].Split(',');
                        float fIncrementSumerAll = float.Parse(flvslue[0]) + float.Parse(flvslue[1]) + float.Parse(flvslue[2]) + float.Parse(flvslue[3]);
                        int iIndexasd = 0;

                        for (int i = 0; i < 5; i++)
                        {
                            #region 电子指示显示器电能示值组合误差试验 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            //ff = (dd+ 10).ToString()+ System.Guid.NewGuid().ToString().Replace("-", "").Substring(6);

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "电子指示显示器电能示值组合误差"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "电子指示显示器电能示值组合误差"; //试验分组 *20
                                m_DETECT_RSLT.TEST_CATEGORIES = "电子指示显示器电能示值组合误差"; //试验分项
                            }
                            else
                            {
                                m_DETECT_RSLT.ITEM_NAME = "计度器总电能示值误差试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "计度器总电能示值误差试验"; //试验分组 *13
                                m_DETECT_RSLT.TEST_CATEGORIES = "计度器总电能示值误差试验"; //试验分项
                            }

                            iIndexasd++;
                            m_DETECT_RSLT.DETECT_ITEM_POINT = iIndexasd.ToString(); //检定点的序号

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = testValue.Split('|')[2]; //负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                            //m_DETECT_RSLT.INT_CONVERT_ERR = flvslue[i];//误差化整值 *20
                            m_DETECT_RSLT.ERR_ABS = "±0.003"; //误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey].Result.Trim() == ConstHelper.合格 ? "01" : "02"; ; //分项结论                                
                            string jfpg = "尖电量|峰电量|平电量|谷电量";
                            if (i < 4)//分
                            {
                                m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(flvslue[i]).ToString("f2");//误差化整值 *20
                                m_DETECT_RSLT.DATA_ITEM1 = jfpg.Split('|')[i];//费率 *13 *20
                                m_DETECT_RSLT.DATA_ITEM2 = flvslue[i]; //走度 *13
                                m_DETECT_RSLT.DATA_ITEM3 = jfpg.Split('|')[i]; //类别
                                m_DETECT_RSLT.DATA_ITEM4 = flQM[i]; //起度
                                m_DETECT_RSLT.DATA_ITEM5 = flZM[i]; //止度
                                m_DETECT_RSLT.DATA_ITEM6 = flvslue[i];//电量差
                            }
                            else//总
                            {
                                m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(value[2]).ToString("f2");//误差化整值 *20
                                m_DETECT_RSLT.DATA_ITEM1 = "总电量";//费率 *13 *20
                                m_DETECT_RSLT.DATA_ITEM2 = value[2]; //走度 *13
                                m_DETECT_RSLT.DATA_ITEM3 = "总电量"; //类别
                                m_DETECT_RSLT.DATA_ITEM4 = value[0]; //起度
                                m_DETECT_RSLT.DATA_ITEM5 = value[1]; //止度
                                m_DETECT_RSLT.DATA_ITEM6 = value[2];//电量差
                            }

                            #region 电子指示显示器电能示值组合误差 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result.Trim() == ConstHelper.合格 ? "01" : meter.MeterDgns["005"].Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            #region 电子指示显示器电能示值组合误差 私有值-国金

                            iIndexasd++;

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "电子指示显示器电能示值组合误差"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "电子指示显示器电能示值组合误差"; //试验分组 *20
                                m_DETECT_RSLT.TEST_CATEGORIES = "电子指示显示器电能示值组合误差"; //试验分项
                            }
                            else
                            {
                                m_DETECT_RSLT.ITEM_NAME = "计度器总电能示值误差试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "计度器总电能示值误差试验"; //试验分组 *13
                                m_DETECT_RSLT.TEST_CATEGORIES = "计度器总电能示值误差试验"; //试验分项
                            }
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = testValue.Split('|')[2]; //负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "±0.003"; //误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey].Result.Trim() == ConstHelper.合格 ? "01" : "02"; ; //分项结论

                            if (i == 0)
                            {
                                m_DETECT_RSLT.INT_CONVERT_ERR = "±0.03"; //项目的试验结果值 *20
                                m_DETECT_RSLT.DATA_ITEM1 = "允许误差";//项目 *13 *20
                                m_DETECT_RSLT.DATA_ITEM2 = "±0.03"; //项目的试验结果值 *13
                            }
                            else if (i == 1)
                            {
                                m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(value[value.Length - 1]).ToString("f2"); //项目的试验结果值 *20
                                m_DETECT_RSLT.DATA_ITEM1 = "实际误差";//项目 *13 *20
                                m_DETECT_RSLT.DATA_ITEM2 = value[value.Length - 1]; //项目的试验结果值 *13
                            }
                            else if (i == 2)
                            {
                                m_DETECT_RSLT.INT_CONVERT_ERR = fIncrementSumerAll.ToString("f2"); //项目的试验结果值 *20
                                m_DETECT_RSLT.DATA_ITEM1 = "各分时电量之和";//项目 *13 *20
                                m_DETECT_RSLT.DATA_ITEM2 = fIncrementSumerAll.ToString(); //项目的试验结果值     *13
                            }

                            m_DETECT_RSLT.DATA_ITEM3 = m_DETECT_RSLT.DATA_ITEM1;
                            m_DETECT_RSLT.DATA_ITEM4 = "0.00";
                            m_DETECT_RSLT.DATA_ITEM5 = "0.00";
                            m_DETECT_RSLT.DATA_ITEM6 = "0.00";

                            #region 常数（走字） 指标项 空值-国金

                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result.Trim() == ConstHelper.合格 ? "01" : meter.MeterDgns["005"].Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 需量示值误差试验-国金  *13 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.需量示值误差;

                List<string> LstKeys = new List<string>();

                //string[] xlwc = GetXLData(meter);

                foreach (string item in meter.MeterDgns.Keys)
                {
                    if (item.Substring(0, 5) == ItemKey)
                        LstKeys.Add(item);
                }

                for (int i = 0; i < LstKeys.Count; i++)
                {
                    itemId = "010";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0108";
                    }
                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        if (meter.MeterDgns.ContainsKey(LstKeys[i]))
                        {
                            string[] maxdata1 = meter.MeterDgns[LstKeys[i]].Value.Split('|');
                            string testValue = meter.MeterDgns[LstKeys[i]].TestValue;

                            #region 需量示值误差私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "需量示值误差试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "需量示值误差试验"; //试验分组 *13 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "需量示值误差试验"; //试验分项

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数 *13 *20
                            m_DETECT_RSLT.LOAD_CURRENT = maxdata1[0]; //负载电流 *13 *20
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = testValue.Split('|')[1]; //功率方向

                            m_DETECT_RSLT.INT_CONVERT_ERR = maxdata1[5]; //误差化整值 *20
                            m_DETECT_RSLT.ERR_ABS = "±" + maxdata1[1]; //误差限值 *13 *20
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[LstKeys[i]].Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                            //m_DETECT_RSLT.DATA_ITEM1 = GetPCode("meterTestVolt", "100%Un");//电压
                            m_DETECT_RSLT.DATA_ITEM1 = Un;//电压 *13 *20
                            m_DETECT_RSLT.DATA_ITEM2 = maxdata1[5];//需量示值误差 *13
                            m_DETECT_RSLT.DATA_ITEM3 = maxdata1[3];//需量示值标准值
                            m_DETECT_RSLT.DATA_ITEM4 = maxdata1[4]; //需量示值实测值，指示值
                                                                    //Double wcjdz = Math.Abs(Convert.ToDouble(maxdata1[3]));//绝对值
                            m_DETECT_RSLT.DATA_ITEM5 = maxdata1[5];//误差
                            m_DETECT_RSLT.DATA_ITEM6 = maxdata1[1] + "|" + maxdata1[2];//需量误差限
                                                                                       //m_DETECT_RSLT.DATA_ITEM6 = "?P=X+0.05Pn/P(X=1.0,Pn为额定功率；P为测量负载点功率";//需量误差
                            #region 需量示值误差 指标项 空值-国金

                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = LstKeys[i];

                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[LstKeys[i]].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[LstKeys[i]].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion           

            #region 由电源供电的时钟试验（日计时试验）-国金 *13 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.日计时误差;

                if (meter.MeterDgns.ContainsKey(ItemKey))
                {
                    string[] Value = meter.MeterDgns[ItemKey].Value.Split('|');
                    //string[] testValue = meter.MeterDgns[ItemKey].TestValue.Split('|');
                    //itemId = "005";
                    itemId = "011";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0109";
                    }
                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        #region 由电源供电的时钟试验 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_NAME = "由电源供电的时钟试验试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "由电源供电的时钟试验"; //试验分组 *20
                            m_DETECT_RSLT.TEST_CATEGORIES = "由电源供电的时钟试验"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_NAME = "日计时误差实验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "日计时误差实验"; //试验分组 *13
                            m_DETECT_RSLT.TEST_CATEGORIES = "日计时误差实验"; //试验分项
                        }

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0"; //功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = ""; //负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                        m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(Value[6]).ToString("f2");//误差化整值 *13 *20
                        m_DETECT_RSLT.ERR_ABS = "±0.5s/24h"; //误差限值 *13 *20
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; ; //分项结论

                        m_DETECT_RSLT.DATA_ITEM1 = "5"; //次数 *13
                        m_DETECT_RSLT.DATA_ITEM2 = "1"; //时基频率（Hz） *13
                        m_DETECT_RSLT.DATA_ITEM3 = float.Parse(Value[6]).ToString("f2");//误差化整值
                        m_DETECT_RSLT.DATA_ITEM4 = Value[0];
                        m_DETECT_RSLT.DATA_ITEM5 = Value[1];
                        m_DETECT_RSLT.DATA_ITEM6 = Value[2];
                        m_DETECT_RSLT.DATA_ITEM7 = Value[3];
                        m_DETECT_RSLT.DATA_ITEM8 = Value[4];

                        #region 由电源供电的时钟试验 指标项 空值-国金

                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;
                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
            }
            #endregion

            #region 采用备用电源工作的时钟试验-国金 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.采用备用电源工作的时钟试验;
                if (meter.MeterDgns.ContainsKey(ItemKey))
                {
                    //itemId = "0";
                    itemId = "0110";

                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        #region 采用备用电源工作的时钟试验 私有值-国金

                        string[] Value = meter.MeterDgns[ItemKey].Value.Split('|');
                        string[] testValue = meter.MeterDgns[ItemKey].TestValue.Split('|');
                        DateTime qian = Convert.ToDateTime(Value[0]);
                        DateTime hou = Convert.ToDateTime(Value[1]);
                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "采用备用电源工作的时钟试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "采用备用电源工作的时钟试验"; //试验分组 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "采用备用电源工作的时钟试验"; //试验分项
                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0"; //功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = ""; //负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                        //if (Value[2].IndexOf("-") != -1)
                        //    m_DETECT_RSLT.INT_CONVERT_ERR = Value[2];//误差化整值
                        //else
                        //    m_DETECT_RSLT.INT_CONVERT_ERR = "-" + Value[2];//误差化整值
                        m_DETECT_RSLT.INT_CONVERT_ERR = Convert.ToInt32(float.Parse(Value[2])).ToString();//误差化整值 *20

                        m_DETECT_RSLT.ERR_ABS = "±" + testValue[0] + "s/72h"; //误差限值 *20
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                        m_DETECT_RSLT.DATA_ITEM1 = qian.ToShortDateString(); //试验前日期
                        m_DETECT_RSLT.DATA_ITEM2 = qian.ToLongTimeString(); //试验前时间
                        m_DETECT_RSLT.DATA_ITEM3 = hou.ToShortDateString(); //试验后日期
                        m_DETECT_RSLT.DATA_ITEM4 = hou.ToLongTimeString(); //试验后时间

                        #region 采用备用电源工作的时钟试验 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;
                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
            }
            #endregion

            #region 误差一致性试验-国金 *13 *20
            if (meter.MeterErrAccords.Count > 0)
            {
                string strKey = "1";
                if (meter.MeterErrAccords.ContainsKey(strKey))
                {
                    if (meter.MeterErrAccords[strKey].PointList.Count >= 0)
                    {
                        //string[] strSubKey = new string[meter.MeterErrAccords[strKey].PointList.Keys.Count];
                        if (meter.MeterErrAccords.ContainsKey(strKey))          //如果数据模型中已经存在该点的数据
                        {
                            iIndex = 0;

                            foreach (string _subKey in meter.MeterErrAccords[strKey].PointList.Keys)
                            {
                                MeterErrAccordBase errAccord = meter.MeterErrAccords[strKey].PointList[_subKey];

                                //itemId = "003";
                                itemId = "014";
                                if (meter.MD_JJGC == "IR46")
                                {
                                    itemId = "0112";
                                }
                                if (TaskId.IndexOf(itemId) != -1)
                                {
                                    if (errAccord.IbX == "0.1Ib" && errAccord.PF == "0.5L") continue;

                                    string[] Arr_Err = errAccord.Data1.Split('|');  //分解误差

                                    string strName = errAccord.Name;
                                    string[] Stryzx = strName.Split(' ');
                                    string glfxyzx = "";
                                    switch (Stryzx[0])
                                    {
                                        case "P+":
                                            glfxyzx = "正向有功";
                                            break;
                                        case "P-":
                                            glfxyzx = "反向有功";
                                            break;
                                        case "Q+":
                                            glfxyzx = "正向无功";
                                            break;
                                        case "Q-":
                                            glfxyzx = "反向无功";
                                            break;
                                        default:
                                            glfxyzx = "正向有功";
                                            break;
                                    }

                                    #region 误差一致性 私有值-国金
                                    string aa = System.Guid.NewGuid().ToString();
                                    string bb = aa.Replace("-", "");
                                    dd += 10;
                                    string cc = bb.Substring(6);
                                    string ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                    m_DETECT_RSLT.ITEM_NAME = "误差一致性试验"; //试验项名称
                                    m_DETECT_RSLT.TEST_GROUP = "误差一致性试验误差改变量"; //试验分组 *13 *20

                                    m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                                    m_DETECT_RSLT.TEST_CATEGORIES = "误差一致性试验误差改变量"; //试验分项

                                    m_DETECT_RSLT.IABC = zjxfs; //相别
                                    m_DETECT_RSLT.PF = errAccord.PF;//code 功率因数 *13 *20
                                    m_DETECT_RSLT.LOAD_CURRENT = errAccord.IbX;//负载电流 *13 *20
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = glfxyzx; //功率方向

                                    m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(errAccord.Error).ToString("f2");//误差化整值 *20
                                    m_DETECT_RSLT.ERR_ABS = errAccord.Limit; //误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = errAccord.Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                                    if (meter.MD_JJGC == "IR46")
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = Arr_Err[0]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM2 = Arr_Err[1]; //误差2
                                        m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                        m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                        m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                        m_DETECT_RSLT.DATA_ITEM6 = Arr_Err[2]; //误差平均值
                                        m_DETECT_RSLT.DATA_ITEM7 = (float.Parse(Arr_Err[2]) - float.Parse(errAccord.ErrAver)).ToString("f4"); //变差原始值，没有可不填
                                        m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                                        m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                                        m_DETECT_RSLT.DATA_ITEM10 = ""; //试验条件
                                        m_DETECT_RSLT.DATA_ITEM11 = ""; //试验要求
                                        m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = errAccord.Error; //误差1，根据实测个数传递 *13
                                        m_DETECT_RSLT.DATA_ITEM2 = Arr_Err[0]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM3 = Arr_Err[1]; //误差2
                                        m_DETECT_RSLT.DATA_ITEM4 = ""; //误差3
                                        m_DETECT_RSLT.DATA_ITEM5 = ""; //误差4
                                        m_DETECT_RSLT.DATA_ITEM6 = ""; //误差5
                                        m_DETECT_RSLT.DATA_ITEM7 = Arr_Err[2]; //平均值
                                        m_DETECT_RSLT.DATA_ITEM8 = errAccord.Error.Trim(); //变差
                                        m_DETECT_RSLT.DATA_ITEM9 = float.Parse(errAccord.Error).ToString("f2"); //变差化整
                                        m_DETECT_RSLT.DATA_ITEM10 = errAccord.ErrAver.Trim(); //参考平均值
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                    }

                                    #region 误差一致性 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM13 = "";
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = errAccord.PrjID;
                                    #endregion

                                    m_DETECT_RSLT.CHK_CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : errAccord.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion

                                    iIndex++;

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        foreach (string sql in sqls)
                                        {
                                            sqlList.Add(sql);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 变差要求试验-国金 *13 *20
            if (meter.MeterErrAccords.Count > 0)
            {
                iIndex = 0;
                string strKey = "2";

                if (meter.MeterErrAccords.ContainsKey(strKey))
                {
                    if (meter.MeterErrAccords[strKey].PointList.Count >= 0)
                    {
                        //string[] strSubKey = new string[meter.MeterErrAccords[strKey].PointList.Keys.Count];

                        foreach (string _subKey in meter.MeterErrAccords[strKey].PointList.Keys)
                        {
                            //itemId = "003";
                            itemId = "013";
                            if (meter.MD_JJGC == "IR46")
                            {
                                itemId = "0113";
                            }
                            if (TaskId.IndexOf(itemId) != -1)
                            {
                                MeterErrAccordBase errAccord = meter.MeterErrAccords[strKey].PointList[_subKey];

                                string[] Arr_Err = errAccord.Data1.Split('|');           //分解误差
                                string[] err0 = errAccord.Data2.Split('|');           //分解误差                        

                                string strName = errAccord.Name;
                                string[] Stryzx = strName.Split(' ');
                                string glfxyzx = "";
                                switch (Stryzx[0])
                                {
                                    case "P+":
                                        glfxyzx = "正向有功";
                                        break;
                                    case "P-":
                                        glfxyzx = "反向有功";
                                        break;
                                    case "Q+":
                                        glfxyzx = "正向无功";
                                        break;
                                    case "Q-":
                                        glfxyzx = "反向无功";
                                        break;
                                    default:
                                        glfxyzx = "正向有功";
                                        break;
                                }

                                for (int qh = 0; qh < 2; qh++)
                                {
                                    #region 变差要求试验 私有值-国金

                                    string aa = System.Guid.NewGuid().ToString();
                                    string bb = aa.Replace("-", "");
                                    dd += 10;
                                    string cc = bb.Substring(6);
                                    string ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                    if (meter.MD_JJGC == "IR46")
                                    {
                                        m_DETECT_RSLT.ITEM_NAME = "变差要求试验"; //试验项名称
                                        m_DETECT_RSLT.TEST_GROUP = "变差要求试验误差改变量"; //试验分组 *20
                                        m_DETECT_RSLT.TEST_CATEGORIES = "变差要求试验误差改变量"; //试验分项
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.ITEM_NAME = "误差变差试验"; //试验项名称
                                        m_DETECT_RSLT.TEST_GROUP = "误差变差试验误差改变量"; //试验分组 *13
                                        m_DETECT_RSLT.TEST_CATEGORIES = "误差变差试验误差改变量"; //试验分项
                                    }
                                    m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                                    m_DETECT_RSLT.IABC = zjxfs; //相别
                                    m_DETECT_RSLT.PF = errAccord.PF;//code 功率因数 *13 *20
                                    m_DETECT_RSLT.LOAD_CURRENT = errAccord.IbX;//负载电流 *13 *20
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = glfxyzx; //功率方向

                                    m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(errAccord.Error).ToString("f2");//误差化整值 *20
                                    m_DETECT_RSLT.ERR_ABS = errAccord.Limit; //误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = errAccord.Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                                    if (qh == 0)
                                    {
                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = Arr_Err[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM2 = Arr_Err[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM6 = Arr_Err[2]; //误差平均值
                                            m_DETECT_RSLT.DATA_ITEM7 = (float.Parse(err0[2]) - float.Parse(Arr_Err[2])).ToString("f4"); //变差原始值，没有可不填
                                            m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                                            m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                                            m_DETECT_RSLT.DATA_ITEM10 = ""; //试验条件
                                            m_DETECT_RSLT.DATA_ITEM11 = ""; //试验要求
                                            m_DETECT_RSLT.DATA_ITEM12 = "影响前"; //影响量前后
                                            m_DETECT_RSLT.DATA_ITEM13 = "第一次"; //第一次第二次
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = errAccord.Error.Trim(); //误差1，根据实测个数传递 *13
                                            m_DETECT_RSLT.DATA_ITEM2 = Arr_Err[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM3 = Arr_Err[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM4 = Arr_Err[2]; //平均值
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM6 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM7 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM8 = errAccord.Error.Trim(); //差值
                                            m_DETECT_RSLT.DATA_ITEM9 = "影响前"; //序列值为：第一次，第二次
                                            m_DETECT_RSLT.DATA_ITEM10 = float.Parse(errAccord.Error).ToString("f2"); ; //变差化整
                                            m_DETECT_RSLT.DATA_ITEM11 = "";
                                            m_DETECT_RSLT.DATA_ITEM12 = "";
                                            m_DETECT_RSLT.DATA_ITEM13 = "";
                                        }
                                    }
                                    else
                                    {
                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = err0[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM2 = err0[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM6 = err0[2]; //误差平均值
                                            m_DETECT_RSLT.DATA_ITEM7 = (float.Parse(err0[2]) - float.Parse(Arr_Err[2])).ToString("f4"); //变差原始值，没有可不填
                                            m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                                            m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                                            m_DETECT_RSLT.DATA_ITEM10 = ""; //试验条件
                                            m_DETECT_RSLT.DATA_ITEM11 = ""; //试验要求
                                            m_DETECT_RSLT.DATA_ITEM12 = "影响后"; //影响量前后
                                            m_DETECT_RSLT.DATA_ITEM13 = "第二次"; //第一次第二次
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = errAccord.Error.Trim(); //误差1，根据实测个数传递 *13
                                            m_DETECT_RSLT.DATA_ITEM2 = err0[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM3 = err0[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM4 = err0[2]; //平均值
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM6 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM7 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM8 = errAccord.Error.Trim(); //差值
                                            m_DETECT_RSLT.DATA_ITEM9 = "影响后"; //序列值为：第一次，第二次
                                            m_DETECT_RSLT.DATA_ITEM10 = float.Parse(errAccord.Error).ToString("f2"); ; //变差化整
                                            m_DETECT_RSLT.DATA_ITEM11 = "";
                                            m_DETECT_RSLT.DATA_ITEM12 = "";
                                            m_DETECT_RSLT.DATA_ITEM13 = "";
                                        }
                                    }

                                    #region 变差要求试验 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = errAccord.PrjID;
                                    #endregion

                                    m_DETECT_RSLT.CHK_CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : errAccord.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion

                                    iIndex++;

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        if (qh == 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        }
                                        foreach (string sql in sqls)
                                        {
                                            WCBC += 1;
                                            sqlList.Add(sql);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            #endregion

            #region 负载电流升降变差试验-国金 *13 *20
            if (meter.MeterErrAccords.Count > 0)
            {
                int iTnt = 0;
                string strKey = "3";
                if (meter.MeterErrAccords.ContainsKey(strKey))
                {
                    if (meter.MeterErrAccords[strKey].PointList.Count >= 0)
                    {
                        //string[] strSubKey = new string[meter.MeterErrAccords[strKey].PointList.Keys.Count];

                        foreach (string _subKey in meter.MeterErrAccords[strKey].PointList.Keys)
                        {
                            //itemId = "003";
                            itemId = "015";
                            if (meter.MD_JJGC == "IR46")
                            {
                                itemId = "0114";
                            }
                            if (TaskId.IndexOf(itemId) != -1)
                            {
                                MeterErrAccordBase errAccord = meter.MeterErrAccords[strKey].PointList[_subKey];

                                string[] err0 = errAccord.Data1.Split('|');           //分解误差
                                string[] err1 = errAccord.Data2.Split('|');           //分解误差                        

                                string strName = errAccord.Name;
                                string[] Stryzx = strName.Split(' ');
                                string glfxyzx = "";
                                switch (Stryzx[0])
                                {
                                    case "P+":
                                        glfxyzx = "正向有功";
                                        break;
                                    case "P-":
                                        glfxyzx = "反向有功";
                                        break;
                                    case "Q+":
                                        glfxyzx = "正向无功";
                                        break;
                                    case "Q-":
                                        glfxyzx = "反向无功";
                                        break;
                                    default:
                                        glfxyzx = "正向有功";
                                        break;
                                }

                                for (int qh = 0; qh < 2; qh++)
                                {
                                    #region 负载电流升降变差试验 私有值-国金

                                    string aa = System.Guid.NewGuid().ToString();
                                    string bb = aa.Replace("-", "");
                                    dd += 10;
                                    string cc = bb.Substring(6);
                                    string ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                    m_DETECT_RSLT.ITEM_NAME = "负载电流升降变差试验"; //试验项名称
                                    m_DETECT_RSLT.TEST_GROUP = "负载电流升降变差试验误差改变量"; //试验分组 *13 *20

                                    m_DETECT_RSLT.DETECT_ITEM_POINT = (iTnt + 1).ToString(); //检定点的序号

                                    m_DETECT_RSLT.TEST_CATEGORIES = "负载电流升降变差试验误差改变量"; //试验分项

                                    m_DETECT_RSLT.IABC = zjxfs; //相别
                                    m_DETECT_RSLT.PF = errAccord.PF;//code 功率因数 *13 *20
                                    m_DETECT_RSLT.LOAD_CURRENT = errAccord.IbX;//负载电流 *13 *20
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = glfxyzx; //功率方向                               

                                    m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(errAccord.Error).ToString("f2");//误差化整值 *20
                                    m_DETECT_RSLT.ERR_ABS = errAccord.Limit;//误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = errAccord.Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                                    if (qh == 0)
                                    {
                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = err0[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM2 = err0[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM6 = err0[2]; //误差平均值
                                            m_DETECT_RSLT.DATA_ITEM7 = (float.Parse(err1[2]) - float.Parse(err0[2])).ToString("f4"); //变差原始值，没有可不填
                                            m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                                            m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                                            m_DETECT_RSLT.DATA_ITEM10 = ""; //试验条件
                                            m_DETECT_RSLT.DATA_ITEM11 = ""; //试验要求
                                            m_DETECT_RSLT.DATA_ITEM12 = "影响前"; //影响量前后
                                            m_DETECT_RSLT.DATA_ITEM13 = "上升";//上升或下降
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = errAccord.Error.Trim(); //误差1，根据实测个数传递 *13
                                            m_DETECT_RSLT.DATA_ITEM2 = err0[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM3 = err0[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM4 = err0[2]; //平均值
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM6 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM7 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM8 = "影响前"; //上升或下降
                                            m_DETECT_RSLT.DATA_ITEM9 = "影响前"; //序列值为：第一次，第二次
                                            m_DETECT_RSLT.DATA_ITEM10 = float.Parse(errAccord.Error).ToString("f2"); ; //变差化整
                                            m_DETECT_RSLT.DATA_ITEM11 = "";
                                            m_DETECT_RSLT.DATA_ITEM12 = "";
                                            m_DETECT_RSLT.DATA_ITEM13 = "";
                                        }
                                    }
                                    else
                                    {
                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = err1[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM2 = err1[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM6 = err1[2]; //误差平均值
                                            m_DETECT_RSLT.DATA_ITEM7 = (float.Parse(err1[2]) - float.Parse(err0[2])).ToString("f4"); //变差原始值，没有可不填
                                            m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                                            m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                                            m_DETECT_RSLT.DATA_ITEM10 = ""; //试验条件
                                            m_DETECT_RSLT.DATA_ITEM11 = ""; //试验要求
                                            m_DETECT_RSLT.DATA_ITEM12 = "影响后"; //影响量前后
                                            m_DETECT_RSLT.DATA_ITEM13 = "下降";//上升或下降
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.DATA_ITEM1 = errAccord.Error.Trim(); //误差1，根据实测个数传递 *13
                                            m_DETECT_RSLT.DATA_ITEM2 = err1[0]; //误差1
                                            m_DETECT_RSLT.DATA_ITEM3 = err1[1]; //误差2
                                            m_DETECT_RSLT.DATA_ITEM4 = err1[2]; //平均值
                                            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差3
                                            m_DETECT_RSLT.DATA_ITEM6 = ""; //误差4
                                            m_DETECT_RSLT.DATA_ITEM7 = ""; //误差5
                                            m_DETECT_RSLT.DATA_ITEM8 = "影响后"; //上升或下降
                                            m_DETECT_RSLT.DATA_ITEM9 = "影响后"; //序列值为：第一次，第二次
                                            m_DETECT_RSLT.DATA_ITEM10 = float.Parse(errAccord.Error).ToString("f2"); ; //变差化整
                                            m_DETECT_RSLT.DATA_ITEM11 = "";
                                            m_DETECT_RSLT.DATA_ITEM12 = "";
                                            m_DETECT_RSLT.DATA_ITEM13 = "";
                                        }
                                    }

                                    #region 负载电流升降变差试验 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = errAccord.PrjID;
                                    #endregion

                                    m_DETECT_RSLT.CHK_CONC_CODE = errAccord.Result == ConstHelper.合格 ? "01" : errAccord.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion
                                    iTnt++;

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        if (qh == 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        }
                                        foreach (string sql in sqls)
                                        {
                                            sqlList.Add(sql);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 重复性试验-国金 *13 *20
            if (meter.MeterErrors.Count > 0)
            {
                iIndex = 0;

                foreach (string key in meter.MeterErrors.Keys)
                {
                    if (key.Split('_')[0] == ProjectID.标准偏差试验)
                    {
                        MeterError data = meter.MeterErrors[key];

                        string[] wc = data.WCData.Split('|');

                        itemId = "006";
                        if (meter.MD_JJGC == "IR46")
                        {
                            itemId = "0115";
                        }
                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 重复性试验 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "重复性试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "重复性试验"; //试验分组 *20
                                m_DETECT_RSLT.TEST_CATEGORIES = "重复性试验"; //试验分项
                            }
                            else
                            {
                                m_DETECT_RSLT.ITEM_NAME = "测量重复性试验标准偏差"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "测量重复性试验标准偏差"; //试验分组 *13
                                m_DETECT_RSLT.TEST_CATEGORIES = "测量重复性试验标准偏差"; //试验分项
                            }

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            if (data.YJ == "H") data.YJ = zjxfs;

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = data.GLYS.Trim(); //功率因数 *13 *20
                            m_DETECT_RSLT.LOAD_CURRENT = data.IbX; //负载电流 *13 *20
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = data.GLFX; //功率方向

                            if (data.WCHZ.Length > 8) data.WCHZ = data.WCHZ.Substring(0, 8);

                            m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(data.WCHZ).ToString("f2");//误差化整值 *20
                            m_DETECT_RSLT.ERR_ABS = data.Limit; //误差限值 *13 *20
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = data.Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                            if (data.WCPC.Length > 8) data.WCPC = data.WCPC.Substring(0, 8);

                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = wc[0]; //误差1，根据实测个数传递
                                m_DETECT_RSLT.DATA_ITEM2 = wc[1]; //误差2，根据实测个数传递
                                m_DETECT_RSLT.DATA_ITEM3 = wc[2]; //误差3，根据实测个数传递
                                m_DETECT_RSLT.DATA_ITEM4 = wc[3]; //误差4，根据实测个数传递
                                m_DETECT_RSLT.DATA_ITEM5 = wc[4]; //误差5，根据实测个数传递
                                m_DETECT_RSLT.DATA_ITEM6 = data.WCPC; //误差偏差值
                                m_DETECT_RSLT.DATA_ITEM7 = data.WCPC; //偏差值 *13
                                m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值
                                m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值
                                m_DETECT_RSLT.DATA_ITEM10 = ""; //试验项目
                                m_DETECT_RSLT.DATA_ITEM11 = ""; //技术要求说明
                                m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后
                            }
                            else
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = data.WCPC;//平均值
                                m_DETECT_RSLT.DATA_ITEM2 = wc[0]; //误差1
                                m_DETECT_RSLT.DATA_ITEM3 = wc[1]; //误差2
                                m_DETECT_RSLT.DATA_ITEM4 = wc[2]; //误差3
                                m_DETECT_RSLT.DATA_ITEM5 = wc[3]; //误差4
                                m_DETECT_RSLT.DATA_ITEM6 = wc[4]; //误差5
                                m_DETECT_RSLT.DATA_ITEM7 = data.WCHZ; ; //偏差值
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                            }

                            #region 重复性试验 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = key;
                            #endregion


                            m_DETECT_RSLT.CHK_CONC_CODE = data.Result == ConstHelper.合格 ? "01" : data.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            iIndex++;

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 测量及监测误差试验-国金 *13 *20
            if (meter.MeterDgns.Count > 0)
            {
                string strResult = "";
                string ItemKey = ProjectID.测量及监测误差;
                iIndex = 1;
                if (meter.MeterDgns.ContainsKey(ItemKey))
                {
                    string[] strDnb = null;
                    string[] strBzb = null;
                    string[] strWc = null;

                    //int intj = 0;

                    itemId = "063";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0116";
                    }
                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        string fzIB;
                        if (meter.MD_JJGC == "IR46")
                        {
                            fzIB = "10Itr";
                        }
                        else
                        {
                            fzIB = "Ib";
                        }

                        string[] Value = meter.MeterDgns[ItemKey].Value.Split('|');
                        for (int i = 0; i < Value.Length; i++)
                        {
                            if (meter.MeterDgns.ContainsKey(ItemKey))
                            {
                                strResult = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";
                            }

                            string[] arr = Value[i].Split('~');
                            if (arr.Length > 2)
                            {
                                strDnb = arr[0].Split(',');
                                strBzb = arr[1].Split(',');
                                strWc = arr[2].Split(',');
                            }

                            m_DETECT_RSLT.ITEM_NAME = "测量及监测误差试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "测量及监测误差试验"; //试验分组 *13 *20
                            m_DETECT_RSLT.TEST_CATEGORIES = "测量及监测误差试验";//检定项 试验分项

                            string StrBJ;

                            string StrYj;
                            string bb;
                            string cc;
                            string ff;

                            string aa;
                            string StrBZ;
                            string StrBWC;
                            //string key = "";

                            switch (i)
                            {
                                case 0: //120%Un
                                    #region 电压120%Un
                                    for (int j = 0; j <= 2; j++)
                                    {
                                        if (strDnb == null) continue;
                                        switch (j.ToString())
                                        {
                                            case "0":
                                                StrYj = "A";
                                                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                                StrBWC = strWc.Length > 0 ? strWc[0] : "";
                                                break;
                                            case "1":
                                                if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                StrYj = "B";
                                                StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
                                                StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
                                                StrBWC = strWc.Length > 1 ? strWc[1] : "";
                                                break;
                                            case "2":
                                                if (meter.MD_WiringMode == "单相") continue;
                                                StrYj = "C";
                                                StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
                                                StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
                                                StrBWC = strWc.Length > 2 ? strWc[2] : "";
                                                break;
                                            default:
                                                StrYj = "A";
                                                StrBJ = "";
                                                StrBZ = "";
                                                StrBWC = "";
                                                break;
                                        }

                                        if (meter.MD_WiringMode == "单相")
                                        {
                                            StrYj = "ABC";
                                        }

                                        #region 测量及监测误差试验 私有值-国金

                                        aa = System.Guid.NewGuid().ToString();
                                        bb = aa.Replace("-", "");
                                        dd += 10;
                                        cc = bb.Substring(6);
                                        ff = dd.ToString() + cc;
                                        m_DETECT_RSLT.READ_ID = ff; //主键

                                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                        m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                        m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                        m_DETECT_RSLT.LOAD_CURRENT = fzIB;//负载电流
                                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                        m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                        m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                        m_DETECT_RSLT.DATA_ITEM1 = "电压";//国金试验要求项目 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM2 = "120%" + Un; //试验要求分项 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                        m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                        m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                        #region 测量及监测误差试验 指标项 空值-国金
                                        m_DETECT_RSLT.DATA_ITEM7 = "";
                                        m_DETECT_RSLT.DATA_ITEM8 = "";
                                        m_DETECT_RSLT.DATA_ITEM9 = "";
                                        m_DETECT_RSLT.DATA_ITEM10 = "";
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                        m_DETECT_RSLT.DATA_ITEM13 = "";
                                        m_DETECT_RSLT.DATA_ITEM14 = "";
                                        m_DETECT_RSLT.DATA_ITEM15 = "";
                                        m_DETECT_RSLT.DATA_ITEM16 = "";
                                        m_DETECT_RSLT.DATA_ITEM17 = "";
                                        m_DETECT_RSLT.DATA_ITEM18 = "";
                                        m_DETECT_RSLT.DATA_ITEM19 = "";
                                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                        #endregion
                                        iIndex++;
                                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                        #endregion

                                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                        if (sqls.Count > 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                            foreach (string sql in sqls)
                                            {
                                                sqlList.Add(sql);
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                case 1: //100%Un
                                    #region 100%Un
                                    for (int j = 0; j <= 2; j++)
                                    {
                                        if (strDnb == null) continue;

                                        switch (j.ToString())
                                        {
                                            case "0":
                                                StrYj = "A";
                                                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                                StrBWC = strWc.Length > 0 ? strWc[0] : "";
                                                break;
                                            case "1":
                                                if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                StrYj = "B";
                                                StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
                                                StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
                                                StrBWC = strWc.Length > 1 ? strWc[1] : "";
                                                break;
                                            case "2":
                                                if (meter.MD_WiringMode == "单相") continue;
                                                StrYj = "C";
                                                StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
                                                StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
                                                StrBWC = strWc.Length > 2 ? strWc[2] : "";
                                                break;
                                            default:
                                                StrYj = "A";
                                                StrBJ = "";
                                                StrBZ = "";
                                                StrBWC = "";
                                                break;
                                        }

                                        if (meter.MD_WiringMode == "单相")
                                        {
                                            StrYj = "ABC";
                                        }

                                        #region 测量及监测误差试验 私有值-国金

                                        aa = System.Guid.NewGuid().ToString();
                                        bb = aa.Replace("-", "");
                                        dd += 10;
                                        cc = bb.Substring(6);
                                        ff = dd.ToString() + cc;
                                        m_DETECT_RSLT.READ_ID = ff; //主键

                                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                        m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                        m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                        m_DETECT_RSLT.LOAD_CURRENT = fzIB;//负载电流
                                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                        m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                        m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                        m_DETECT_RSLT.DATA_ITEM1 = "电压";//国金试验要求项目 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM2 = Un; //试验要求分项 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                        m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                        m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                        #region 测量及监测误差试验 指标项 空值-国金
                                        m_DETECT_RSLT.DATA_ITEM7 = "";
                                        m_DETECT_RSLT.DATA_ITEM8 = "";
                                        m_DETECT_RSLT.DATA_ITEM9 = "";
                                        m_DETECT_RSLT.DATA_ITEM10 = "";
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                        m_DETECT_RSLT.DATA_ITEM13 = "";
                                        m_DETECT_RSLT.DATA_ITEM14 = "";
                                        m_DETECT_RSLT.DATA_ITEM15 = "";
                                        m_DETECT_RSLT.DATA_ITEM16 = "";
                                        m_DETECT_RSLT.DATA_ITEM17 = "";
                                        m_DETECT_RSLT.DATA_ITEM18 = "";
                                        m_DETECT_RSLT.DATA_ITEM19 = "";
                                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                        #endregion
                                        iIndex++;
                                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                        #endregion

                                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                        if (sqls.Count > 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                            foreach (string sql in sqls)
                                            {
                                                sqlList.Add(sql);
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                case 2:  //60%Un
                                    #region 60%Un
                                    for (int j = 0; j <= 2; j++)
                                    {
                                        if (strDnb == null) continue;

                                        switch (j.ToString())
                                        {
                                            case "0":
                                                StrYj = "A";
                                                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                                StrBWC = strWc.Length > 0 ? strWc[0] : "";
                                                break;
                                            case "1":
                                                if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                StrYj = "B";
                                                StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
                                                StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
                                                StrBWC = strWc.Length > 1 ? strWc[1] : "";
                                                break;
                                            case "2":
                                                if (meter.MD_WiringMode == "单相") continue;
                                                StrYj = "C";
                                                StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
                                                StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
                                                StrBWC = strWc.Length > 2 ? strWc[2] : "";
                                                break;
                                            default:
                                                StrYj = "A";
                                                StrBJ = "";
                                                StrBZ = "";
                                                StrBWC = "";
                                                break;
                                        }

                                        if (meter.MD_WiringMode == "单相")
                                        {
                                            StrYj = "ABC";
                                        }

                                        #region 测量及监测误差试验 私有值-国金

                                        aa = System.Guid.NewGuid().ToString();
                                        bb = aa.Replace("-", "");
                                        dd += 10;
                                        cc = bb.Substring(6);
                                        ff = dd.ToString() + cc;
                                        m_DETECT_RSLT.READ_ID = ff; //主键

                                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                        m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                        m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                        m_DETECT_RSLT.LOAD_CURRENT = fzIB;//负载电流
                                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                        m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                        m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                        m_DETECT_RSLT.DATA_ITEM1 = "电压";//国金试验要求项目 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM2 = "60%" + Un; //试验要求分项 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                        m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                        m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                        #region 测量及监测误差试验 指标项 空值-国金
                                        m_DETECT_RSLT.DATA_ITEM7 = "";
                                        m_DETECT_RSLT.DATA_ITEM8 = "";
                                        m_DETECT_RSLT.DATA_ITEM9 = "";
                                        m_DETECT_RSLT.DATA_ITEM10 = "";
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                        m_DETECT_RSLT.DATA_ITEM13 = "";
                                        m_DETECT_RSLT.DATA_ITEM14 = "";
                                        m_DETECT_RSLT.DATA_ITEM15 = "";
                                        m_DETECT_RSLT.DATA_ITEM16 = "";
                                        m_DETECT_RSLT.DATA_ITEM17 = "";
                                        m_DETECT_RSLT.DATA_ITEM18 = "";
                                        m_DETECT_RSLT.DATA_ITEM19 = "";
                                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                        #endregion
                                        iIndex++;
                                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                        #endregion

                                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                        if (sqls.Count > 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                            foreach (string sql in sqls)
                                            {
                                                sqlList.Add(sql);
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                case 3: //120%Imax
                                    #region 120%Imax
                                    for (int j = 0; j <= 2; j++)
                                    {
                                        if (strDnb == null) continue;

                                        switch (j.ToString())
                                        {
                                            case "0":
                                                StrYj = "A";
                                                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                                StrBWC = strWc.Length > 0 ? strWc[0] : "";
                                                break;
                                            case "1":
                                                if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                StrYj = "B";
                                                StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
                                                StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
                                                StrBWC = strWc.Length > 1 ? strWc[1] : "";
                                                break;
                                            case "2":
                                                if (meter.MD_WiringMode == "单相") continue;
                                                StrYj = "C";
                                                StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
                                                StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
                                                StrBWC = strWc.Length > 2 ? strWc[2] : "";
                                                break;
                                            default:
                                                StrYj = "A";
                                                StrBJ = "";
                                                StrBZ = "";
                                                StrBWC = "";
                                                break;
                                        }

                                        if (meter.MD_WiringMode == "单相")
                                        {
                                            StrYj = "ABC";
                                        }

                                        #region 测量及监测误差试验 私有值-国金

                                        aa = System.Guid.NewGuid().ToString();
                                        bb = aa.Replace("-", "");
                                        dd += 10;
                                        cc = bb.Substring(6);
                                        ff = dd.ToString() + cc;
                                        m_DETECT_RSLT.READ_ID = ff; //主键

                                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                        m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                        m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                        m_DETECT_RSLT.LOAD_CURRENT = "1.2Imax";//负载电流
                                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                        m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                        m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                        m_DETECT_RSLT.DATA_ITEM1 = "电流";//国金试验要求项目 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM2 = "120%Imax"; //试验要求分项 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                        m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                        m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                        #region 测量及监测误差试验 指标项 空值-国金
                                        m_DETECT_RSLT.DATA_ITEM7 = "";
                                        m_DETECT_RSLT.DATA_ITEM8 = "";
                                        m_DETECT_RSLT.DATA_ITEM9 = "";
                                        m_DETECT_RSLT.DATA_ITEM10 = "";
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                        m_DETECT_RSLT.DATA_ITEM13 = "";
                                        m_DETECT_RSLT.DATA_ITEM14 = "";
                                        m_DETECT_RSLT.DATA_ITEM15 = "";
                                        m_DETECT_RSLT.DATA_ITEM16 = "";
                                        m_DETECT_RSLT.DATA_ITEM17 = "";
                                        m_DETECT_RSLT.DATA_ITEM18 = "";
                                        m_DETECT_RSLT.DATA_ITEM19 = "";
                                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                        #endregion
                                        iIndex++;
                                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                        #endregion

                                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                        if (sqls.Count > 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                            foreach (string sql in sqls)
                                            {
                                                sqlList.Add(sql);
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                case 4: //100%Ib
                                    #region 100%Ib
                                    for (int j = 0; j <= 2; j++)
                                    {
                                        if (strDnb == null) continue;

                                        switch (j.ToString())
                                        {
                                            case "0":
                                                StrYj = "A";
                                                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                                StrBWC = strWc.Length > 0 ? strWc[0] : "";
                                                break;
                                            case "1":
                                                if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                StrYj = "B";
                                                StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
                                                StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
                                                StrBWC = strWc.Length > 1 ? strWc[1] : "";
                                                break;
                                            case "2":
                                                if (meter.MD_WiringMode == "单相") continue;
                                                StrYj = "C";
                                                StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
                                                StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
                                                StrBWC = strWc.Length > 2 ? strWc[2] : "";
                                                break;
                                            default:
                                                StrYj = "A";
                                                StrBJ = "";
                                                StrBZ = "";
                                                StrBWC = "";
                                                break;
                                        }

                                        if (meter.MD_WiringMode == "单相")
                                        {
                                            StrYj = "ABC";
                                        }

                                        #region 测量及监测误差试验 私有值-国金

                                        aa = System.Guid.NewGuid().ToString();
                                        bb = aa.Replace("-", "");
                                        dd += 10;
                                        cc = bb.Substring(6);
                                        ff = dd.ToString() + cc;
                                        m_DETECT_RSLT.READ_ID = ff; //主键

                                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                        m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                        m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                        m_DETECT_RSLT.LOAD_CURRENT = fzIB;//负载电流
                                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                        m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                        m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                        m_DETECT_RSLT.DATA_ITEM1 = "电流";//国金试验要求项目 *13 *20

                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM2 = "1000%Itr"; //试验要求分项 *20
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.DATA_ITEM2 = "100%Ib"; //试验要求分项 *13
                                        }

                                        m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                        m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                        m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据
                                        iIndex++;
                                        #region 测量及监测误差试验 指标项 空值-国金
                                        m_DETECT_RSLT.DATA_ITEM7 = "";
                                        m_DETECT_RSLT.DATA_ITEM8 = "";
                                        m_DETECT_RSLT.DATA_ITEM9 = "";
                                        m_DETECT_RSLT.DATA_ITEM10 = "";
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                        m_DETECT_RSLT.DATA_ITEM13 = "";
                                        m_DETECT_RSLT.DATA_ITEM14 = "";
                                        m_DETECT_RSLT.DATA_ITEM15 = "";
                                        m_DETECT_RSLT.DATA_ITEM16 = "";
                                        m_DETECT_RSLT.DATA_ITEM17 = "";
                                        m_DETECT_RSLT.DATA_ITEM18 = "";
                                        m_DETECT_RSLT.DATA_ITEM19 = "";
                                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                        #endregion

                                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                        #endregion

                                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                        if (sqls.Count > 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                            foreach (string sql in sqls)
                                            {
                                                sqlList.Add(sql);
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                case 5: //5%Ib
                                    #region 5%Ib
                                    for (int j = 0; j <= 2; j++)
                                    {
                                        if (strDnb == null) continue;

                                        switch (j.ToString())
                                        {
                                            case "0":
                                                StrYj = "A";
                                                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                                StrBWC = strWc.Length > 0 ? strWc[0] : "";
                                                break;
                                            case "1":
                                                if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                StrYj = "B";
                                                StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
                                                StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
                                                StrBWC = strWc.Length > 1 ? strWc[1] : "";
                                                break;
                                            case "2":
                                                if (meter.MD_WiringMode == "单相") continue;
                                                StrYj = "C";
                                                StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
                                                StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
                                                StrBWC = strWc.Length > 2 ? strWc[2] : "";
                                                break;
                                            default:
                                                StrYj = "A";
                                                StrBJ = "";
                                                StrBZ = "";
                                                StrBWC = "";
                                                break;
                                        }

                                        if (meter.MD_WiringMode == "单相")
                                        {
                                            StrYj = "ABC";
                                        }

                                        #region 测量及监测误差试验 私有值-国金

                                        aa = System.Guid.NewGuid().ToString();
                                        bb = aa.Replace("-", "");
                                        dd += 10;
                                        cc = bb.Substring(6);
                                        ff = dd.ToString() + cc;
                                        m_DETECT_RSLT.READ_ID = ff; //主键

                                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                        m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                        m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                        m_DETECT_RSLT.PF = "1.0";//code 功率因数

                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.LOAD_CURRENT = "Imin";//负载电流
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.LOAD_CURRENT = "0.05Ib";//负载电流
                                        }

                                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                        m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                        m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                        m_DETECT_RSLT.DATA_ITEM1 = "电流";//国金试验要求项目 *13 *20

                                        if (meter.MD_JJGC == "IR46")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM2 = "100%Imin"; //试验要求分项 *20
                                        }
                                        else
                                        {
                                            m_DETECT_RSLT.DATA_ITEM2 = "5%Ib"; //试验要求分项 *13
                                        }

                                        m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                        m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                        m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                        #region 测量及监测误差试验 指标项 空值-国金
                                        m_DETECT_RSLT.DATA_ITEM7 = "";
                                        m_DETECT_RSLT.DATA_ITEM8 = "";
                                        m_DETECT_RSLT.DATA_ITEM9 = "";
                                        m_DETECT_RSLT.DATA_ITEM10 = "";
                                        m_DETECT_RSLT.DATA_ITEM11 = "";
                                        m_DETECT_RSLT.DATA_ITEM12 = "";
                                        m_DETECT_RSLT.DATA_ITEM13 = "";
                                        m_DETECT_RSLT.DATA_ITEM14 = "";
                                        m_DETECT_RSLT.DATA_ITEM15 = "";
                                        m_DETECT_RSLT.DATA_ITEM16 = "";
                                        m_DETECT_RSLT.DATA_ITEM17 = "";
                                        m_DETECT_RSLT.DATA_ITEM18 = "";
                                        m_DETECT_RSLT.DATA_ITEM19 = "";
                                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                        #endregion
                                        iIndex++;
                                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                        #endregion

                                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                        if (sqls.Count > 0)
                                        {
                                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                            foreach (string sql in sqls)
                                            {
                                                sqlList.Add(sql);
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                case 6: //120%Un/120%Imax/1.0
                                    #region 120%Un/120%Imax/1.0
                                    if (strDnb == null) continue;

                                    if (meter.MD_WiringMode == "三相三线")
                                    {
                                        StrYj = "AC";
                                    }
                                    else
                                    {
                                        StrYj = "ABC";
                                    }

                                    StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                    StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                    StrBWC = strWc.Length > 0 ? strWc[0] : "";

                                    #region 测量及监测误差试验 私有值-国金

                                    aa = System.Guid.NewGuid().ToString();
                                    bb = aa.Replace("-", "");
                                    dd += 10;
                                    cc = bb.Substring(6);
                                    ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                    m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                    m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                    m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                    m_DETECT_RSLT.LOAD_CURRENT = "1.2Imax";//负载电流
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                    m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                    m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                    m_DETECT_RSLT.DATA_ITEM1 = "功率";//国金试验要求项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "120%Un/120%Imax/1.0"; //试验要求分项 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                    m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                    m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                    #region 测量及监测误差试验 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM7 = "";
                                    m_DETECT_RSLT.DATA_ITEM8 = "";
                                    m_DETECT_RSLT.DATA_ITEM9 = "";
                                    m_DETECT_RSLT.DATA_ITEM10 = "";
                                    m_DETECT_RSLT.DATA_ITEM11 = "";
                                    m_DETECT_RSLT.DATA_ITEM12 = "";
                                    m_DETECT_RSLT.DATA_ITEM13 = "";
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                    #endregion
                                    iIndex++;
                                    m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        foreach (string sql in sqls)
                                        {
                                            sqlList.Add(sql);
                                        }
                                    }
                                    break;
                                #endregion
                                case 7: //100%Un/100%Ib/1.0
                                    #region 100%Un/100%Ib/1.0
                                    if (strDnb == null) continue;

                                    if (meter.MD_WiringMode == "三相三线")
                                    {
                                        StrYj = "AC";
                                    }
                                    else
                                    {
                                        StrYj = "ABC";
                                    }

                                    StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                    StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                    StrBWC = strWc.Length > 0 ? strWc[0] : "";

                                    #region 测量及监测误差试验 私有值-国金

                                    aa = System.Guid.NewGuid().ToString();
                                    bb = aa.Replace("-", "");
                                    dd += 10;
                                    cc = bb.Substring(6);
                                    ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                    m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                    m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                    m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                    m_DETECT_RSLT.LOAD_CURRENT = "Imax";//负载电流
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                    m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                    m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                    m_DETECT_RSLT.DATA_ITEM1 = "功率";//国金试验要求项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "100%Un/100%Imax/1.0"; //试验要求分项 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                    m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                    m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                    #region 测量及监测误差试验 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM7 = "";
                                    m_DETECT_RSLT.DATA_ITEM8 = "";
                                    m_DETECT_RSLT.DATA_ITEM9 = "";
                                    m_DETECT_RSLT.DATA_ITEM10 = "";
                                    m_DETECT_RSLT.DATA_ITEM11 = "";
                                    m_DETECT_RSLT.DATA_ITEM12 = "";
                                    m_DETECT_RSLT.DATA_ITEM13 = "";
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                    #endregion
                                    iIndex++;
                                    m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        foreach (string sql in sqls)
                                        {
                                            sqlList.Add(sql);
                                        }
                                    }
                                    break;
                                #endregion
                                case 8: //100%Un/0.4%Ib/1.0
                                    #region 100%Un/0.4%Ib/1.0
                                    if (strDnb == null) continue;

                                    if (meter.MD_WiringMode == "三相三线")
                                    {
                                        StrYj = "AC";
                                    }
                                    else
                                    {
                                        StrYj = "ABC";
                                    }

                                    string Bdj;
                                    string Qddl = "0.4";
                                    Bdj = meter.MD_Grane;


                                    if (meter.MD_ConnectionFlag == "互感式")
                                    {
                                        Bdj = Bdj.Split('(')[0];

                                        if (Bdj.IndexOf("0.2") != -1 || Bdj.IndexOf("0.5") != -1)
                                        {
                                            Qddl = "0.1";
                                        }

                                        else if (Bdj.IndexOf("1.0") != -1)
                                        {
                                            Qddl = "0.2";
                                        }
                                        else
                                        {
                                            Qddl = "0.4";
                                        }
                                    }

                                    string dl;

                                    string tj;
                                    if (meter.MD_JJGC == "IR46")
                                    {
                                        dl = "0.04Itr";
                                        tj = "100%Un×4%Itr×1.0";
                                    }
                                    else
                                    {
                                        dl = Qddl + "Ib";
                                        tj = "100%Un×" + (float.Parse(Qddl) * 100) + "%Ib×1.0";
                                    }

                                    //m_DETECT_RSLT.TEST_CATEGORIES = "功率100%Un×" + Qddl + "%Ib×1.0";//检定项 试验分项

                                    StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                    StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                    StrBWC = strWc.Length > 0 ? strWc[0] : "";

                                    #region 测量及监测误差试验 私有值-国金

                                    aa = System.Guid.NewGuid().ToString();
                                    bb = aa.Replace("-", "");
                                    dd += 10;
                                    cc = bb.Substring(6);
                                    ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                    m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                    m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                    m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                    m_DETECT_RSLT.LOAD_CURRENT = dl;//负载电流
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                    m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                    m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                    m_DETECT_RSLT.DATA_ITEM1 = "功率";//国金试验要求项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = tj; //试验要求分项 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                    m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                    m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据
                                    iIndex++;
                                    #region 测量及监测误差试验 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM7 = "";
                                    m_DETECT_RSLT.DATA_ITEM8 = "";
                                    m_DETECT_RSLT.DATA_ITEM9 = "";
                                    m_DETECT_RSLT.DATA_ITEM10 = "";
                                    m_DETECT_RSLT.DATA_ITEM11 = "";
                                    m_DETECT_RSLT.DATA_ITEM12 = "";
                                    m_DETECT_RSLT.DATA_ITEM13 = "";
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                    #endregion

                                    m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        foreach (string sql in sqls)
                                        {
                                            sqlList.Add(sql);
                                        }
                                    }
                                    break;

                                #endregion
                                case 9: //0.5L
                                    #region 0.5L
                                    if (strDnb == null) continue;

                                    if (meter.MD_WiringMode == "三相三线")
                                    {
                                        StrYj = "AC";
                                    }
                                    else
                                    {
                                        StrYj = "ABC";
                                    }

                                    StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
                                    StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
                                    StrBWC = strWc.Length > 0 ? strWc[0] : "";

                                    #region 测量及监测误差试验 私有值-国金

                                    aa = System.Guid.NewGuid().ToString();
                                    bb = aa.Replace("-", "");
                                    dd += 10;
                                    cc = bb.Substring(6);
                                    ff = dd.ToString() + cc;
                                    m_DETECT_RSLT.READ_ID = ff; //主键

                                    m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                    m_DETECT_RSLT.DETECT_ITEM_POINT = iIndex.ToString(); //检定点的序号

                                    m_DETECT_RSLT.IABC = StrYj; //相别 *13 *20
                                    m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                    m_DETECT_RSLT.LOAD_CURRENT = fzIB;//负载电流
                                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                    m_DETECT_RSLT.INT_CONVERT_ERR = StrBWC;//误差化整值
                                    m_DETECT_RSLT.ERR_ABS = "±1.0"; //误差限值 *13 *20
                                    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                    m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                                    m_DETECT_RSLT.DATA_ITEM1 = "功率因数";//国金试验要求项目
                                    m_DETECT_RSLT.DATA_ITEM2 = "0.5L"; //试验要求分项 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM3 = StrBWC; //试验结果实测值 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM4 = Value[i]; //试验要求值类别，标准值、指示值、引用误差
                                    m_DETECT_RSLT.DATA_ITEM5 = StrBJ; //电表数据
                                    m_DETECT_RSLT.DATA_ITEM6 = StrBZ; //标准表数据

                                    #region 测量及监测误差试验 指标项 空值-国金
                                    m_DETECT_RSLT.DATA_ITEM7 = "";
                                    m_DETECT_RSLT.DATA_ITEM8 = "";
                                    m_DETECT_RSLT.DATA_ITEM9 = "";
                                    m_DETECT_RSLT.DATA_ITEM10 = "";
                                    m_DETECT_RSLT.DATA_ITEM11 = "";
                                    m_DETECT_RSLT.DATA_ITEM12 = "";
                                    m_DETECT_RSLT.DATA_ITEM13 = "";
                                    m_DETECT_RSLT.DATA_ITEM14 = "";
                                    m_DETECT_RSLT.DATA_ITEM15 = "";
                                    m_DETECT_RSLT.DATA_ITEM16 = "";
                                    m_DETECT_RSLT.DATA_ITEM17 = "";
                                    m_DETECT_RSLT.DATA_ITEM18 = "";
                                    m_DETECT_RSLT.DATA_ITEM19 = "";
                                    m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                                    #endregion
                                    iIndex++;
                                    m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                    #endregion

                                    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                    if (sqls.Count > 0)
                                    {
                                        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        foreach (string sql in sqls)
                                        {
                                            sqlList.Add(sql);
                                        }
                                    }
                                    break;
                                    #endregion
                            }
                        }
                    }
                }
            }
            #endregion

            #region 测量及监测零线电流误差-国金 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.零线电流检测;
                foreach (string item in meter.MeterDgns.Keys)
                {
                    if (item.Substring(0, 5) == ItemKey)
                    {
                        itemId = "0117";

                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 测量及监测零线电流误差 私有值-国金

                            string[] Value = meter.MeterDgns[item].Value.Split('|');
                            string[] testValue = meter.MeterDgns[item].TestValue.Split('|');

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "测量及监测零线电流误差试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "测量及监测零线电流误差试验"; //试验分组 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "测量及监测零线电流误差试验"; //试验分项
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = testValue[0]; //负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                            m_DETECT_RSLT.INT_CONVERT_ERR = Value[1];//误差化整值 *20

                            m_DETECT_RSLT.ERR_ABS = "±" + testValue[1]; //误差限值 *20
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[item].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                            m_DETECT_RSLT.DATA_ITEM1 = "零线电流"; ;
                            m_DETECT_RSLT.DATA_ITEM2 = testValue[0];
                            m_DETECT_RSLT.DATA_ITEM3 = Value[1];

                            m_DETECT_RSLT.DATA_ITEM10 = "零线电流"; // *20
                            m_DETECT_RSLT.DATA_ITEM11 = testValue[0]; // *20

                            #region 测量及监测零线电流误差 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";

                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = item;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[item].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[item].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }

                #region 旧
                //if (meter.MeterDgns.ContainsKey(ItemKey+"_20"))
                //{
                //    //itemId = "0";
                //    itemId = "0117";

                //    if (TaskId.IndexOf(itemId) != -1)
                //    {
                //        #region 测量及监测零线电流误差 私有值-国金

                //        string[] Value = meter.MeterDgns[ItemKey + "_20"].Value.Split('|');
                //        string[] testValue = meter.MeterDgns[ItemKey + "_20"].TestValue.Split('|');

                //        string aa = System.Guid.NewGuid().ToString();
                //        string bb = aa.Replace("-", "");
                //        dd = dd + 10;
                //        string cc = bb.Substring(6);
                //        string ff = dd.ToString() + cc;
                //        m_DETECT_RSLT.READ_ID = ff; //主键

                //        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                //        m_DETECT_RSLT.ITEM_NAME = "测量及监测零线电流误差试验"; //试验项名称
                //        m_DETECT_RSLT.TEST_GROUP = "测量及监测零线电流误差试验"; //试验分组 *20

                //        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                //        m_DETECT_RSLT.TEST_CATEGORIES = "测量及监测零线电流误差试验"; //试验分项
                //        m_DETECT_RSLT.IABC = zjxfs; //相别
                //        m_DETECT_RSLT.PF = "1.0"; //功率因数
                //        m_DETECT_RSLT.LOAD_CURRENT = testValue[0]; //负载电流
                //        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                //        m_DETECT_RSLT.INT_CONVERT_ERR = Value[1];//误差化整值 *20

                //        m_DETECT_RSLT.ERR_ABS = "±" + testValue[1]; //误差限值 *20
                //        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //        m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey + "_20"].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                //        m_DETECT_RSLT.DATA_ITEM10 = "零线电流";
                //        m_DETECT_RSLT.DATA_ITEM11 = testValue[0];

                //        #region 测量及监测零线电流误差 指标项 空值-国金
                //        m_DETECT_RSLT.DATA_ITEM1 = "";
                //        m_DETECT_RSLT.DATA_ITEM2 = "";
                //        m_DETECT_RSLT.DATA_ITEM3 = "";
                //        m_DETECT_RSLT.DATA_ITEM4 = "";
                //        m_DETECT_RSLT.DATA_ITEM5 = "";
                //        m_DETECT_RSLT.DATA_ITEM6 = "";
                //        m_DETECT_RSLT.DATA_ITEM7 = "";
                //        m_DETECT_RSLT.DATA_ITEM8 = "";
                //        m_DETECT_RSLT.DATA_ITEM9 = "";

                //        m_DETECT_RSLT.DATA_ITEM12 = "";
                //        m_DETECT_RSLT.DATA_ITEM13 = "";
                //        m_DETECT_RSLT.DATA_ITEM14 = "";
                //        m_DETECT_RSLT.DATA_ITEM15 = "";
                //        m_DETECT_RSLT.DATA_ITEM16 = "";
                //        m_DETECT_RSLT.DATA_ITEM17 = "";
                //        m_DETECT_RSLT.DATA_ITEM18 = "";
                //        m_DETECT_RSLT.DATA_ITEM19 = "";
                //        m_DETECT_RSLT.DATA_ITEM20 = "";
                //        #endregion

                //        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey + "_20"].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey + "_20"].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                //        #endregion

                //        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //        if (sqls.Count > 0)
                //        {
                //            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //            foreach (string sql in sqls)
                //            {
                //                sqlList.Add(sql);
                //            }
                //        }
                //    }
                //}

                //if (meter.MeterDgns.ContainsKey(ItemKey + "_14"))
                //{
                //    //itemId = "0";
                //    itemId = "0117";

                //    if (TaskId.IndexOf(itemId) != -1)
                //    {
                //        #region 测量及监测零线电流误差 私有值-国金

                //        string[] Value = meter.MeterDgns[ItemKey + "_14"].Value.Split('|');
                //        string[] testValue = meter.MeterDgns[ItemKey + "_14"].TestValue.Split('|');

                //        string aa = System.Guid.NewGuid().ToString();
                //        string bb = aa.Replace("-", "");
                //        dd = dd + 10;
                //        string cc = bb.Substring(6);
                //        string ff = dd.ToString() + cc;
                //        m_DETECT_RSLT.READ_ID = ff; //主键

                //        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                //        m_DETECT_RSLT.ITEM_NAME = "测量及监测零线电流误差试验"; //试验项名称
                //        m_DETECT_RSLT.TEST_GROUP = "测量及监测零线电流误差试验"; //试验分组 *20

                //        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                //        m_DETECT_RSLT.TEST_CATEGORIES = "测量及监测零线电流误差试验"; //试验分项
                //        m_DETECT_RSLT.IABC = zjxfs; //相别
                //        m_DETECT_RSLT.PF = "1.0"; //功率因数
                //        m_DETECT_RSLT.LOAD_CURRENT = testValue[0]; //负载电流
                //        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                //        m_DETECT_RSLT.INT_CONVERT_ERR = Value[1];//误差化整值 *20

                //        m_DETECT_RSLT.ERR_ABS = "±" + testValue[1]; //误差限值 *20
                //        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //        m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey + "_14"].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                //        m_DETECT_RSLT.DATA_ITEM10 = "零线电流";
                //        m_DETECT_RSLT.DATA_ITEM11 = testValue[0];

                //        #region 测量及监测零线电流误差 指标项 空值-国金
                //        m_DETECT_RSLT.DATA_ITEM1 = "";
                //        m_DETECT_RSLT.DATA_ITEM2 = "";
                //        m_DETECT_RSLT.DATA_ITEM3 = "";
                //        m_DETECT_RSLT.DATA_ITEM4 = "";
                //        m_DETECT_RSLT.DATA_ITEM5 = "";
                //        m_DETECT_RSLT.DATA_ITEM6 = "";
                //        m_DETECT_RSLT.DATA_ITEM7 = "";
                //        m_DETECT_RSLT.DATA_ITEM8 = "";
                //        m_DETECT_RSLT.DATA_ITEM9 = "";

                //        m_DETECT_RSLT.DATA_ITEM12 = "";
                //        m_DETECT_RSLT.DATA_ITEM13 = "";
                //        m_DETECT_RSLT.DATA_ITEM14 = "";
                //        m_DETECT_RSLT.DATA_ITEM15 = "";
                //        m_DETECT_RSLT.DATA_ITEM16 = "";
                //        m_DETECT_RSLT.DATA_ITEM17 = "";
                //        m_DETECT_RSLT.DATA_ITEM18 = "";
                //        m_DETECT_RSLT.DATA_ITEM19 = "";
                //        m_DETECT_RSLT.DATA_ITEM20 = "";
                //        #endregion

                //        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey + "_14"].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey + "_20"].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                //        #endregion

                //        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //        if (sqls.Count > 0)
                //        {
                //            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //            foreach (string sql in sqls)
                //            {
                //                sqlList.Add(sql);
                //            }
                //        }
                //    }
                //}

                //if (meter.MeterDgns.ContainsKey(ItemKey + "_19"))
                //{
                //    //itemId = "0";
                //    itemId = "0117";

                //    if (TaskId.IndexOf(itemId) != -1)
                //    {
                //        #region 测量及监测零线电流误差 私有值-国金

                //        string[] Value = meter.MeterDgns[ItemKey + "_19"].Value.Split('|');
                //        string[] testValue = meter.MeterDgns[ItemKey + "_19"].TestValue.Split('|');

                //        string aa = System.Guid.NewGuid().ToString();
                //        string bb = aa.Replace("-", "");
                //        dd = dd + 10;
                //        string cc = bb.Substring(6);
                //        string ff = dd.ToString() + cc;
                //        m_DETECT_RSLT.READ_ID = ff; //主键

                //        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                //        m_DETECT_RSLT.ITEM_NAME = "测量及监测零线电流误差"; //试验项名称
                //        m_DETECT_RSLT.TEST_GROUP = "测量及监测零线电流误差"; //试验分组

                //        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                //        m_DETECT_RSLT.TEST_CATEGORIES = "测量及监测零线电流误差"; //试验分项
                //        m_DETECT_RSLT.IABC = zjxfs; //相别
                //        m_DETECT_RSLT.PF = "1.0"; //功率因数
                //        m_DETECT_RSLT.LOAD_CURRENT = testValue[0]; //负载电流
                //        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                //        if (Value[1].IndexOf("-") != -1)
                //            m_DETECT_RSLT.INT_CONVERT_ERR = Value[1];//误差化整值
                //        else
                //            m_DETECT_RSLT.INT_CONVERT_ERR = "-" + Value[1];//误差化整值

                //        m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey + "_19"].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                //        m_DETECT_RSLT.DATA_ITEM1 = Value[0];
                //        m_DETECT_RSLT.DATA_ITEM2 = ""; //试验前时间
                //        m_DETECT_RSLT.DATA_ITEM3 = ""; //试验后日期
                //        m_DETECT_RSLT.DATA_ITEM4 = ""; //试验后时间

                //        #region 测量及监测零线电流误差 指标项 空值-国金
                //        m_DETECT_RSLT.DATA_ITEM5 = "";
                //        m_DETECT_RSLT.DATA_ITEM6 = "";
                //        m_DETECT_RSLT.DATA_ITEM7 = "";
                //        m_DETECT_RSLT.DATA_ITEM8 = "";
                //        m_DETECT_RSLT.DATA_ITEM9 = "";
                //        m_DETECT_RSLT.DATA_ITEM10 = "";
                //        m_DETECT_RSLT.DATA_ITEM11 = "";
                //        m_DETECT_RSLT.DATA_ITEM12 = "";
                //        m_DETECT_RSLT.DATA_ITEM13 = "";
                //        m_DETECT_RSLT.DATA_ITEM14 = "";
                //        m_DETECT_RSLT.DATA_ITEM15 = "";
                //        m_DETECT_RSLT.DATA_ITEM16 = "";
                //        m_DETECT_RSLT.DATA_ITEM17 = "";
                //        m_DETECT_RSLT.DATA_ITEM18 = "";
                //        m_DETECT_RSLT.DATA_ITEM19 = "";
                //        m_DETECT_RSLT.DATA_ITEM20 = "";
                //        #endregion

                //        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey + "_19"].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey + "_19"].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                //        #endregion

                //        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //        if (sqls.Count > 0)
                //        {
                //            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //            foreach (string sql in sqls)
                //            {
                //                sqlList.Add(sql);
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            #endregion

            #region 交流电压暂降和短时中断实验-国金 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.交流电压暂降和短时中断;
                if (meter.MeterDgns.ContainsKey(ItemKey))
                {
                    itemId = "0118";

                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            #region 交流电压暂降和短时中断实验 私有值-国金

                            //string[] Value = meter.MeterDgns[ItemKey].Value.Split('|');
                            string[] testValue = meter.MeterDgns[ItemKey].TestValue.Split('|');

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "交流电压暂降和短时中断试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "交流电压暂降和短时中断试验"; //试验分组 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "交流电压暂降和短时中断试验"; //试验分项
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = ""; //负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值

                            m_DETECT_RSLT.ERR_ABS = ""; //误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";  //分项结论 *20

                            if (i == 0)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = testValue[0] + "%";
                                m_DETECT_RSLT.DATA_ITEM2 = testValue[2] + "s";
                                m_DETECT_RSLT.DATA_ITEM3 = testValue[3];
                                m_DETECT_RSLT.DATA_ITEM4 = testValue[1] + "s";
                            }
                            else if (i == 1)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = testValue[4] + "%";
                                m_DETECT_RSLT.DATA_ITEM2 = testValue[6] + "s";
                                m_DETECT_RSLT.DATA_ITEM3 = testValue[7];
                                m_DETECT_RSLT.DATA_ITEM4 = testValue[5] + "s";
                            }
                            else if (i == 2)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = testValue[8] + "%";
                                m_DETECT_RSLT.DATA_ITEM2 = testValue[10] + "s";
                                m_DETECT_RSLT.DATA_ITEM3 = testValue[11];
                                m_DETECT_RSLT.DATA_ITEM4 = testValue[9] + "s";
                            }
                            else if (i == 3)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = testValue[12] + "%";
                                m_DETECT_RSLT.DATA_ITEM2 = testValue[14] + "s";
                                m_DETECT_RSLT.DATA_ITEM3 = testValue[15];
                                m_DETECT_RSLT.DATA_ITEM4 = testValue[13] + "s";
                            }

                            m_DETECT_RSLT.DATA_ITEM10 = "工作正常，信息无变化；寄存器值得改变不大于0.040kWh;符合基本最大允许误差极限的要求";//试验要求 *20

                            #region 交流电压暂降和短时中断实验 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";

                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = ItemKey;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 影响量试验-国金
            if (meter.MeterSpecialErrs.Count > 0)
            {
                List<string> LstKey = new List<string>();

                int Indexyxl = 0;
                //int XBJD = 1;//先做个JD放谐波角度

                foreach (string item in meter.MeterSpecialErrs.Keys)
                {
                    if (item.Substring(0, 5) == "12005" || item.Substring(0, 5) == "26001")
                        LstKey.Add(item);
                }

                for (int i = 0; i < LstKey.Count; i++)
                {
                    MeterSpecialErr data = meter.MeterSpecialErrs[LstKey[i]];
                    //string asdfghjk = data.Name;
                    string[] testvalue = data.TestValue.Split('|');
                    string prjID = data.PrjNo.Split('_')[0];
                    #region 影响量-国金

                    M_QT_IMPACT_MET_CONC entity = new M_QT_IMPACT_MET_CONC();

                    string strName = data.Name;
                    string[] prjSte = data.Name.Split('_');
                    //string StrA = data.AVR_VOT_A_MULTIPLE;
                    //string StrB = data.AVR_VOT_B_MULTIPLE;
                    //string StrC = data.AVR_VOT_C_MULTIPLE;
                    //string[] StrTmp = strName.Split('_');
                    //string[] StrT = StrTmp[1].Split(',');
                    string StrGlfx;
                    string StrYj;
                    string StrGlys;
                    string StrFzdl;
                    if (data.YJ == "H") data.YJ = zjxfs;
                    StrYj = data.YJ;
                    StrGlys = data.GLYS;
                    StrFzdl = data.IbX;
                    StrGlfx = data.GLFX;

                    #region 已注释
                    //switch (StrT[0])
                    //{
                    //    case "P+":
                    //        StrGlfx = "正向有功";
                    //        break;
                    //    case "P-":
                    //        StrGlfx = "反向有功";
                    //        break;
                    //    case "Q+":
                    //        StrGlfx = "正向无功";
                    //        break;
                    //    case "Q-":
                    //        StrGlfx = "反向无功";
                    //        break;
                    //    case "Q1":
                    //        StrGlfx = "第一象限无功";
                    //        break;
                    //    case "Q2":
                    //        StrGlfx = "第二象限无功";
                    //        break;
                    //    case "Q3":
                    //        StrGlfx = "第三象限无功";
                    //        break;
                    //    case "Q4":
                    //        StrGlfx = "第四象限无功";
                    //        break;
                    //    default:
                    //        StrGlfx = "正向有功";
                    //        break;
                    //}
                    //if (StrT[1] == "合元")
                    //{
                    //    StrGlys = StrT[2];
                    //    StrFzdl = StrT[3];
                    //}
                    //else
                    //{

                    //    StrGlys = StrT[1];
                    //    StrFzdl = StrT[2];
                    //}
                    //if (meter.MD_WiringMode == "三相三线")
                    //{
                    //    if (StrA == "0" & StrB == "0" & StrC == "1")
                    //    {
                    //        StrYj = "C";
                    //    }
                    //    else if (StrA == "1" & StrB == "0" & StrC == "0")
                    //    {
                    //        StrYj = "A";
                    //    }
                    //    else
                    //    {
                    //        StrYj = "AC";
                    //    }
                    //}
                    //else
                    //{
                    //    if (StrA == "0" & StrB == "1" & StrC == "1")
                    //    {
                    //        StrYj = "A";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
                    //    }
                    //    else if (StrA == "1" & StrB == "0" & StrC == "1")
                    //    {
                    //        StrYj = "B";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
                    //    }
                    //    else if (StrA == "1" & StrB == "1" & StrC == "0")
                    //    {
                    //        StrYj = "C";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
                    //    }
                    //    else
                    //    {
                    //        StrYj = "ABC";
                    //    }
                    //}
                    #endregion

                    m_DETECT_RSLT.IABC = StrYj; //相别
                    m_DETECT_RSLT.PF = StrGlys; //功率因数 *20
                    m_DETECT_RSLT.LOAD_CURRENT = StrFzdl; //负载电流 *20
                    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = StrGlfx; //功率方向

                    m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13
                    m_DETECT_RSLT.DATA_ITEM2 = ""; //功率要求 *13
                    m_DETECT_RSLT.DATA_ITEM3 = ""; //实验要求 *13
                    m_DETECT_RSLT.DATA_ITEM4 = ""; //实验角度 *13
                    m_DETECT_RSLT.DATA_ITEM5 = ""; //误差1 *13
                    m_DETECT_RSLT.DATA_ITEM6 = ""; //误差2 *13
                    m_DETECT_RSLT.DATA_ITEM7 = ""; //平均值 *13

                    m_DETECT_RSLT.DATA_ITEM8 = Un; //电压值 *20
                    m_DETECT_RSLT.DATA_ITEM9 = "50"; //频率值 *20
                    m_DETECT_RSLT.DATA_ITEM10 = ""; //实验条件 *20
                    m_DETECT_RSLT.DATA_ITEM11 = ""; //实验要求 *20
                    m_DETECT_RSLT.DATA_ITEM13 = ""; //功率要求 *20
                    m_DETECT_RSLT.DATA_ITEM14 = ""; //实验角度 *20

                    bool isQH = false; //是否上传前后两次
                    string sytj = "";
                    #region 各种影响量的不同设置
                    if (prjID == ProjectID.第N次谐波试验)
                    {
                        isQH = true;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0131";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电流和电压电路中谐波-第5次谐波试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电流和电压电路中谐波-第5次谐波试验"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "电流和电压电路中谐波-第5次谐波试验"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "021";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电流线路和电压线路中谐波分量误差改变量"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电流线路和电压线路中谐波分量误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "电流线路和电压线路中谐波分量误差改变量"; //试验分项
                        }

                        string xw = testvalue[testvalue.Length - 1];


                        m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13
                        m_DETECT_RSLT.DATA_ITEM2 = "P5=0.04P0"; //功率要求 *13
                        m_DETECT_RSLT.DATA_ITEM3 = "谐波与基波相位"; //实验要求 *13
                        m_DETECT_RSLT.DATA_ITEM4 = xw + "°"; //实验角度 *13
                        m_DETECT_RSLT.DATA_ITEM10 = "5次谐波"; //实验条件
                        //m_DETECT_RSLT.DATA_ITEM11 = "谐波与基波相位"; //实验要求 *20
                        //m_DETECT_RSLT.DATA_ITEM13 = "P5=0.04P1"; //功率要求 *20
                        m_DETECT_RSLT.DATA_ITEM11 = "P5=0.04P1"; //实验要求 *20
                        m_DETECT_RSLT.DATA_ITEM13 = "谐波与基波相位"; //功率要求 *20
                        m_DETECT_RSLT.DATA_ITEM14 = xw + "°"; //实验角度 *20
                    }
                    else if (prjID == ProjectID.方顶波波形试验)
                    {
                        isQH = true;

                        m_DETECT_RSLT.ITEM_ID = "0132";//质检编码
                        m_DETECT_RSLT.ITEM_NAME = "电流和电压电路中谐波-方顶波波形试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "电流和电压电路中谐波-方顶波波形试验"; //试验分组
                        m_DETECT_RSLT.TEST_CATEGORIES = "电流和电压电路中谐波-方顶波波形试验"; //试验分项
                        m_DETECT_RSLT.DATA_ITEM3 = "方顶波"; //实验要求 *13
                        m_DETECT_RSLT.DATA_ITEM10 = "方顶波"; //实验条件
                    }
                    else if (prjID == ProjectID.尖顶波波形改变)
                    {
                        isQH = true;

                        m_DETECT_RSLT.ITEM_ID = "0133";//质检编码
                        m_DETECT_RSLT.ITEM_NAME = "电流和电压电路中谐波-尖顶波波形试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "电流和电压电路中谐波-尖顶波波形试验"; //试验分组
                        m_DETECT_RSLT.TEST_CATEGORIES = "电流和电压电路中谐波-尖顶波波形试验"; //试验分项  

                        m_DETECT_RSLT.DATA_ITEM3 = "尖顶波"; //试验条件
                        m_DETECT_RSLT.DATA_ITEM10 = "尖顶波"; //实验条件
                    }
                    else if (prjID == ProjectID.脉冲群触发波形试验) //间谐波
                    {
                        isQH = true;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0134";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电流电路中的间谐波-脉冲串触发波形试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电流电路中的间谐波-脉冲串触发波形试验"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "电流电路中的间谐波-脉冲串触发波形试验"; //试验分项 
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "023";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "交流电流线路中次谐波试验误差改变量"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "交流电流线路中次谐波试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "交流电流线路中次谐波试验误差改变量"; //试验分项
                        }

                        m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13
                        m_DETECT_RSLT.DATA_ITEM3 = "间谐波"; //试验条件

                        m_DETECT_RSLT.DATA_ITEM10 = "间谐波"; //实验条件
                    }
                    else if (prjID == ProjectID.九十度相位触发波形试验) //奇次谐波
                    {
                        isQH = true;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0135";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电流电路中奇次谐波-90度相位触发波形试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电流电路中奇次谐波-90度相位触发波形试验"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "电流电路中奇次谐波-90度相位触发波形试验"; //试验分项 
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "022";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "交流电流线路中奇次谐波试验误差改变量"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "交流电流线路中奇次谐波试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "交流电流线路中奇次谐波试验误差改变量"; //试验分项
                        }
                        m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13
                        m_DETECT_RSLT.DATA_ITEM3 = "奇次谐波"; //试验条件
                        m_DETECT_RSLT.DATA_ITEM10 = "奇次谐波"; //实验条件
                    }
                    else if (prjID == ProjectID.半波整流波形试验) //偶次谐波
                    {
                        isQH = true;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0136";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "直流和偶次谐波-半波整流波形试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "直流和偶次谐波-半波整流波形试验"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "直流和偶次谐波-半波整流波形试验"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "024";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "交流电流线路中直流和偶次谐波影响试验误差改变量"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "交流电流线路中直流和偶次谐波影响试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "交流电流线路中直流和偶次谐波影响试验误差改变量"; //试验分项
                        }
                        m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13
                        m_DETECT_RSLT.DATA_ITEM3 = "直流偶次谐波"; //试验条件
                        m_DETECT_RSLT.DATA_ITEM10 = "直流偶次谐波"; //实验条件
                        m_DETECT_RSLT.LOAD_CURRENT = "0.707Imax"; //负载电流 *20
                    }


                    else if (prjID == ProjectID.负载不平衡试验)
                    {
                        isQH = true;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0137";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "负载不平衡试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "负载不平衡试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "负载不平衡试验误差改变量"; //试验分项 
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "020";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电压不平衡影响试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电压不平衡影响试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "电压不平衡影响试验误差改变量"; //试验分项
                        }
                    }
                    else if (prjID == ProjectID.电压改变)
                    {
                        isQH = true;
                        string JJG;
                        string IR;
                        if (float.Parse(prjSte[5]) >= 80)
                        {
                            IR = "80%以上";
                            JJG = "80%以上电压误差改变量";
                        }
                        else
                        {
                            IR = "80%以下";
                            JJG = "70%以下电压误差改变量";
                        }

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0138";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电压改变实验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = IR; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = IR; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "033";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "电压改变实验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = JJG; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = JJG; //试验分项
                        }
                        m_DETECT_RSLT.DATA_ITEM1 = prjSte[5] + "%" + Un;//电压 *13
                        m_DETECT_RSLT.DATA_ITEM2 = double.Parse(data.ErrValue).ToString("F2");//结果 *13
                        m_DETECT_RSLT.DATA_ITEM8 = prjSte[5] + "%" + Un;//电压 *20
                    }
                    else if (prjID == ProjectID.一相或两相电压中断试验)
                    {
                        isQH = true;

                        m_DETECT_RSLT.ITEM_ID = "0140";//质检编码
                        m_DETECT_RSLT.ITEM_NAME = "一相或两相电压中断试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "一相或两相电压中断试验"; //试验分组
                        m_DETECT_RSLT.TEST_CATEGORIES = "一相或两相电压中断试验"; //试验分项 

                        m_DETECT_RSLT.DATA_ITEM1 = StrYj; //试验条件

                        string zdA = "";
                        string zdB = "";
                        string zdC = "";
                        if (float.Parse(testvalue[5]) == 0)
                        {
                            zdA = "A相、";
                        }
                        if (float.Parse(testvalue[6]) == 0)
                        {
                            zdB = "B相、";
                        }
                        if (float.Parse(testvalue[7]) == 0)
                        {
                            zdC = "C相、";
                        }

                        string syyq = (zdA + zdB + zdC).Trim('、');

                        m_DETECT_RSLT.DATA_ITEM11 = syyq + "电压中断"; //实验要求
                    }
                    else if (prjID == ProjectID.频率改变)
                    {
                        isQH = true;

                        m_DETECT_RSLT.ITEM_ID = "0141";//质检编码
                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0141";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "频率改变试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "频率改变试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "频率改变试验误差改变量"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "018";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "频率影响试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "频率影响试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "频率影响试验误差改变量"; //试验分项
                        }
                        m_DETECT_RSLT.DATA_ITEM5 = prjSte[5];
                        m_DETECT_RSLT.DATA_ITEM9 = prjSte[5];
                    }
                    else if (prjID == ProjectID.逆相序试验)
                    {
                        isQH = true;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0142";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "逆向序试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "逆向序试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "逆向序试验误差改变量"; //试验分项 
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "019";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "逆相序影响试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "逆相序影响试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "逆相序影响试验误差改变量"; //试验分项
                        }
                        m_DETECT_RSLT.DATA_ITEM1 = Un;//电压 *13
                        m_DETECT_RSLT.DATA_ITEM8 = Un;//电压 *20
                        m_DETECT_RSLT.DATA_ITEM10 = "平衡负载";//平衡负载 *20
                        m_DETECT_RSLT.DATA_ITEM11 = "逆相序";//相序 *20
                    }
                    else if (prjID == ProjectID.辅助装置试验)
                    {
                        isQH = true;

                        m_DETECT_RSLT.ITEM_ID = "0143";//质检编码
                        m_DETECT_RSLT.ITEM_NAME = "辅助装置工作试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "辅助装置工作试验"; //试验分组
                        m_DETECT_RSLT.TEST_CATEGORIES = "辅助装置工作试验"; //试验分项 

                        m_DETECT_RSLT.DATA_ITEM11 = "辅助装置工作"; //实验要求
                    }
                    else if (prjID == ProjectID.自热试验)
                    {
                        isQH = false;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0146";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "自热试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "自热试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "自热试验误差改变量"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "034";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "自热影响试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "自热影响试验误差改变量"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "自热影响试验误差改变量"; //试验分项
                        }
                    }
                    else if (prjID == ProjectID.高次谐波)
                    {
                        isQH = false;

                        m_DETECT_RSLT.ITEM_ID = "0147";//质检编码
                        m_DETECT_RSLT.ITEM_NAME = "高次谐波试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "高次谐波试验"; //试验分组
                        m_DETECT_RSLT.TEST_CATEGORIES = "高次谐波试验"; //试验分项
                    }
                    else if (prjID == ProjectID.冲击试验)
                    {
                        isQH = false;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0154";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "冲击试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "冲击试验"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "冲击试验"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "050";//质检编码

                            if (zjxfs == "ABC" || zjxfs == "AC")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "冲击试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "平衡负载时的基本误差"; //试验分组
                                m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载时的基本误差"; //试验分项
                            }
                            else
                            {
                                m_DETECT_RSLT.ITEM_NAME = "冲击试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "不平衡负载时的基本误差"; //试验分组
                                m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载时的基本误差"; //试验分项
                            }
                        }
                    }
                    else if (prjID == ProjectID.振动试验)
                    {
                        isQH = false;

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.ITEM_ID = "0155";//质检编码
                            m_DETECT_RSLT.ITEM_NAME = "振动试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "振动试验"; //试验分组
                            m_DETECT_RSLT.TEST_CATEGORIES = "振动试验"; //试验分项
                        }
                        else
                        {
                            m_DETECT_RSLT.ITEM_ID = "049";//质检编码
                            if (zjxfs == "ABC" || zjxfs == "AC")
                            {
                                m_DETECT_RSLT.ITEM_NAME = "振动试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "平衡负载时的基本误差"; //试验分组
                                m_DETECT_RSLT.TEST_CATEGORIES = "平衡负载时的基本误差"; //试验分项
                            }
                            else
                            {
                                m_DETECT_RSLT.ITEM_NAME = "振动试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "不平衡负载时的基本误差"; //试验分组
                                m_DETECT_RSLT.TEST_CATEGORIES = "不平衡负载时的基本误差"; //试验分项
                            }
                        }
                    }
                    #endregion

                    //_ = new string[50];
                    //_ = new string[50];
                    //_ = new string[50];
                    //_ = new string[50];

                    #region 修改前 已注释

                    //if (strName.IndexOf("自热试验") != -1)
                    //{
                    //    #region 自热试验

                    //    wc1 = data.Error1.Split('|');
                    //    wc2 = data.Error2.Split('|');

                    //    if (wc2.Length < 6 || wc1.Length <= 0) continue;

                    //    entity.INF_ERR1 = wc1[0];//影响量前误差
                    //    entity.AVER_ERR1 = wc1[0];//影响量前平均误差
                    //    entity.INT_ERR1 = wc1[0];//影响量前化整误差

                    //    entity.INF_ERR2 = wc1[0] + "|" + wc2[0] + "|" + wc2[1] + "|" + wc2[2] + "|" + wc2[3] + "|" + wc2[4];//影响量后误差，多个用‘|’分割AVER_ERR1
                    //    entity.AVER_ERR2 = wc2[wc2.Length - 2];//影响量后平均误差INT_ERR
                    //    entity.INT_ERR2 = wc2[wc2.Length - 1];//影响量后化整误差


                    //    Wctmp = double.Parse(wc2[wc2.Length - 2]) - double.Parse(wc1[0]);
                    //    if (Wctmp < 0)
                    //    {
                    //        if ((data.ErrValue).IndexOf("-") == -1)
                    //        {
                    //            entity.INT_VARIA_ERR = "-" + double.Parse(data.ErrValue).ToString("F2");
                    //            entity.VARIA_ERR = "-" + data.ErrValue;
                    //        }
                    //        else
                    //        {
                    //            entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if ((data.ErrValue).IndexOf("+") == -1)
                    //        {
                    //            entity.INT_VARIA_ERR = "+" + double.Parse(data.ErrValue).ToString("F2");
                    //            entity.VARIA_ERR = "+" + data.ErrValue;
                    //        }
                    //        else
                    //        {
                    //            entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
                    //        }
                    //    }
                    //    #endregion
                    //}
                    //else
                    //{
                    //    strWc = data.Error1.Split('|');// meterErr.Me_chrWcMore.Split('|');+
                    //    if (strWc.Length <= 3) continue;
                    //    strWc1 = data.Error2.Split('|');
                    //    if (strWc1.Length <= 3) continue;

                    //    entity.INF_ERR1 = strWc[0] + "|" + strWc[1];        //影响量前误差，多个用‘|’分割
                    //    entity.INF_ERR2 = strWc1[0] + "|" + strWc1[1];      //影响量后误差，多个用‘|’分割AVER_ERR1

                    //    entity.AVER_ERR1 = strWc[strWc.Length - 2];         //影响量前平均误差
                    //    entity.AVER_ERR2 = strWc1[strWc1.Length - 2];       //影响量后平均误差INT_ERR1 
                    //    entity.AVER_ERR = Math.Abs(Convert.ToSingle(entity.AVER_ERR1.Trim('+')) - Convert.ToSingle(entity.AVER_ERR2.Trim('+'))).ToString(); //平均值误差值
                    //    entity.AVER_ERR = data.ErrValue;

                    //    entity.INT_ERR1 = strWc[strWc.Length - 1];          //影响量前化整误差
                    //    entity.INT_ERR2 = strWc1[strWc1.Length - 1];        //影响量后化整误差
                    //    entity.INT_ERR = data.ErrInt;                   //平均值化整


                    //    Wctmp = double.Parse(strWc1[strWc1.Length - 2]) - double.Parse(strWc[strWc.Length - 2]);
                    //    if (Wctmp < 0)
                    //    {
                    //        if ((data.ErrValue).IndexOf("-") == -1)
                    //        {
                    //            entity.INT_VARIA_ERR = "-" + double.Parse(data.ErrValue).ToString("F2");
                    //            entity.VARIA_ERR = "-" + data.ErrValue;
                    //        }
                    //        else
                    //        {
                    //            entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if ((data.ErrValue).IndexOf("+") == -1)
                    //        {
                    //            entity.INT_VARIA_ERR = "+" + double.Parse(data.ErrValue).ToString("F2");
                    //            entity.VARIA_ERR = "+" + data.ErrValue;
                    //        }
                    //        else
                    //        {
                    //            entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
                    //        }
                    //    }
                    //}

                    //string[] strWcx = data.ErrLimit.Split('|');

                    //if (strName.IndexOf("电源电压影响") != -1)
                    //{
                    //    #region  电源电压影响
                    //    int IntVolt = int.Parse(StrDYTmp.Replace("%Un", ""));
                    //    if (IntVolt <= 70)
                    //    {
                    //        if (entity.AVER_ERR2 == "+100.0000")
                    //        {
                    //            entity.VARIA_ERR = "-100";//HP 电源电压影响低于80Un%不做改变量只做误差
                    //            entity.VALUE_ABS = "-100|+10";
                    //            entity.INT_VARIA_ERR = "-100";//HP 电源电压影响低于80Un%不做改变量只做误差
                    //            entity.INT_ERR2 = "-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
                    //            entity.AVER_ERR2 = "-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
                    //            entity.INF_ERR2 = "-100|-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
                    //        }
                    //        else
                    //        {
                    //            strWc1 = data.Error2.Split('|');

                    //            if ((strWc1[strWc1.Length - 2]).IndexOf("+") == -1)
                    //            {
                    //                entity.VARIA_ERR = "-" + double.Parse(strWc1[strWc1.Length - 2]).ToString();
                    //                entity.INT_VARIA_ERR = "-" + double.Parse(strWc1[strWc1.Length - 2]).ToString("F2");//HP 变差化整值重新计算
                    //            }
                    //            else
                    //            {
                    //                entity.VARIA_ERR = "+" + double.Parse(strWc1[strWc1.Length - 2]).ToString();
                    //                entity.INT_VARIA_ERR = "+" + double.Parse(strWc1[strWc1.Length - 2]).ToString("F2");//HP 变差化整值重新计算
                    //            }
                    //            entity.VALUE_ABS = "-100|+10";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        strWc = data.Error1.Split('|');
                    //        strWc1 = data.Error2.Split('|');
                    //        Wctmp = double.Parse(strWc1[strWc1.Length - 2]) - double.Parse(strWc[strWc.Length - 2]);
                    //        if (Wctmp > 0)
                    //        {
                    //            entity.INT_VARIA_ERR = "+" + Wctmp.ToString("F2");
                    //            entity.VARIA_ERR = "+" + Wctmp.ToString("F4");
                    //        }
                    //        else
                    //        {
                    //            entity.INT_VARIA_ERR = Wctmp.ToString("F2");
                    //            entity.VARIA_ERR = Wctmp.ToString("F4");
                    //        }
                    //        entity.VALUE_ABS = "±" + (data.BPHUpLimit).ToString(); //strWcx[0].Replace('+', '±');//变差限
                    //    }
                    //    #endregion
                    //}
                    //else
                    //{
                    //    entity.VALUE_ABS = "±" + Convert.ToSingle(data.BPHUpLimit).ToString("F1");  //strWcx[0].Replace('+', '±');//变差限
                    //}

                    //if (strName.IndexOf("第5次谐波") != -1)
                    //{
                    //    m_DETECT_RSLT.DATA_ITEM1 = GetPCode("meterTestVolt", "100%Un"); //电压
                    //    m_DETECT_RSLT.DATA_ITEM2 = ""; //试验要求公式    
                    //    m_DETECT_RSLT.DATA_ITEM3 = "第5次谐波"; //试验条件
                    //    m_DETECT_RSLT.DATA_ITEM4 = "0度"; //试验要求角度
                    //    m_DETECT_RSLT.DATA_ITEM5 = strWc1[0]; //试验结果实际相对误差改变实测误差值1
                    //    m_DETECT_RSLT.DATA_ITEM6 = strWc1[1]; //试验结果实际相对误差改变实测误差值2
                    //    m_DETECT_RSLT.DATA_ITEM7 = strWc1[strWc1.Length - 2]; //试验结果实际相对误差改变平均值
                    //    m_DETECT_RSLT.DATA_ITEM8 = entity.VARIA_ERR; //试验结果实际相对误差改变变差值
                    //    m_DETECT_RSLT.DATA_ITEM9 = entity.AVER_ERR1 + "|" + entity.AVER_ERR2; //影响量前/后
                    //    m_DETECT_RSLT.DATA_ITEM10 = "";
                    //    m_DETECT_RSLT.DATA_ITEM11 = "";
                    //}
                    //else if (strName.IndexOf("方顶波影响") != -1 || strName.IndexOf("方顶波影响") != -1)
                    //{
                    //    m_DETECT_RSLT.DATA_ITEM1 = GetPCode("meterTestVolt", "100%Un"); //电压
                    //    m_DETECT_RSLT.DATA_ITEM2 = ""; //试验要求公式    
                    //    m_DETECT_RSLT.DATA_ITEM3 = "方顶波"; //试验条件
                    //    m_DETECT_RSLT.DATA_ITEM4 = "0度"; //试验要求角度
                    //    m_DETECT_RSLT.DATA_ITEM5 = strWc1[0]; //试验结果实际相对误差改变实测误差值1
                    //    m_DETECT_RSLT.DATA_ITEM6 = strWc1[1]; //试验结果实际相对误差改变实测误差值2
                    //    m_DETECT_RSLT.DATA_ITEM7 = strWc1[strWc1.Length - 2]; //试验结果实际相对误差改变平均值
                    //    m_DETECT_RSLT.DATA_ITEM8 = entity.VARIA_ERR; //试验结果实际相对误差改变变差值
                    //    m_DETECT_RSLT.DATA_ITEM9 = entity.AVER_ERR1 + "|" + entity.AVER_ERR2; //影响量前/后
                    //    m_DETECT_RSLT.DATA_ITEM10 = "";
                    //    m_DETECT_RSLT.DATA_ITEM11 = "";
                    //}
                    //else if (strName.IndexOf("自热试验") != -1)
                    //{
                    //    m_DETECT_RSLT.DATA_ITEM1 = entity.VALUE_ABS; //允许误差偏移%                                     
                    //    m_DETECT_RSLT.DATA_ITEM2 = entity.VARIA_ERR; //实际误差偏移%
                    //    m_DETECT_RSLT.DATA_ITEM3 = wc2[0]; //误差1
                    //    m_DETECT_RSLT.DATA_ITEM4 = wc2[1]; //误差2
                    //    m_DETECT_RSLT.DATA_ITEM5 = wc2[2]; //误差3
                    //    m_DETECT_RSLT.DATA_ITEM6 = wc2[3]; //误差4
                    //    m_DETECT_RSLT.DATA_ITEM7 = wc2[4]; //误差5
                    //    m_DETECT_RSLT.DATA_ITEM8 = wc2[5]; //误差6
                    //    m_DETECT_RSLT.DATA_ITEM9 = wc2[6]; //误差7
                    //    m_DETECT_RSLT.DATA_ITEM10 = wc2[7]; //误差8
                    //    m_DETECT_RSLT.DATA_ITEM11 = wc2[8]; //误差9

                    //}
                    //else if (strName.IndexOf("高次谐波影响") != -1)
                    //{
                    //    m_DETECT_RSLT.DATA_ITEM1 = "电压电路"; //试验条件
                    //    m_DETECT_RSLT.DATA_ITEM1 = "电流电路"; //试验条件
                    //    m_DETECT_RSLT.DATA_ITEM2 = entity.VALUE_ABS; //允许误差偏移%   
                    //    m_DETECT_RSLT.DATA_ITEM3 = ""; //谐波次数
                    //    m_DETECT_RSLT.DATA_ITEM4 = entity.VARIA_ERR; //实际误差偏移%
                    //    m_DETECT_RSLT.DATA_ITEM5 = "";
                    //    m_DETECT_RSLT.DATA_ITEM6 = "";
                    //    m_DETECT_RSLT.DATA_ITEM7 = "";
                    //    m_DETECT_RSLT.DATA_ITEM8 = "";
                    //    m_DETECT_RSLT.DATA_ITEM9 = "";
                    //    m_DETECT_RSLT.DATA_ITEM10 = "";
                    //    m_DETECT_RSLT.DATA_ITEM11 = "";
                    //}
                    //else
                    //{
                    //    m_DETECT_RSLT.DATA_ITEM2 = entity.VALUE_ABS; //允许误差偏移%                                     
                    //    m_DETECT_RSLT.DATA_ITEM3 = entity.VARIA_ERR; //实际误差偏移%
                    //    m_DETECT_RSLT.DATA_ITEM4 = "";
                    //    m_DETECT_RSLT.DATA_ITEM5 = "";
                    //    m_DETECT_RSLT.DATA_ITEM6 = "";
                    //    m_DETECT_RSLT.DATA_ITEM7 = "";
                    //    m_DETECT_RSLT.DATA_ITEM8 = "";
                    //    m_DETECT_RSLT.DATA_ITEM9 = "";
                    //    m_DETECT_RSLT.DATA_ITEM10 = "";
                    //    m_DETECT_RSLT.DATA_ITEM11 = "";

                    ////if ((strName.IndexOf("电压影响") != -1 && strName.IndexOf("电源电压影响") == -1) || strName.IndexOf("频率影响") != -1 || strName.IndexOf("尖顶波波形影响") != -1
                    ////    || strName.IndexOf("间谐波影响") != -1 || strName.IndexOf("电源电压影响") != -1 || strName.IndexOf("直流和偶次谐波-半波整流波形影响") != -1
                    ////    || strName.IndexOf("交流电流线路中直流和偶次谐波分量影响") != -1 || strName.IndexOf("负载不平衡影响") != -1 || strName.IndexOf("负载不平衡影响") != -1
                    ////    || strName.IndexOf("逆相序影响") != -1)
                    ////{
                    ////    m_DETECT_RSLT.DATA_ITEM2 = entity.VALUE_ABS; //允许误差偏移%                                     
                    ////    m_DETECT_RSLT.DATA_ITEM3 = entity.VARIA_ERR; //实际误差偏移%
                    ////}
                    //}

                    //m_DETECT_RSLT.DATA_ITEM12 = "";
                    //m_DETECT_RSLT.DATA_ITEM13 = "";
                    //m_DETECT_RSLT.DATA_ITEM14 = "";
                    //m_DETECT_RSLT.DATA_ITEM15 = "";
                    //m_DETECT_RSLT.DATA_ITEM16 = "";
                    //m_DETECT_RSLT.DATA_ITEM17 = "";
                    //m_DETECT_RSLT.DATA_ITEM18 = "";
                    //m_DETECT_RSLT.DATA_ITEM19 = "";
                    //m_DETECT_RSLT.DATA_ITEM20 = "";

                    #endregion


                    if (data.ErrLimitUp.Split('.').Length <= 1 && float.Parse(data.ErrLimitUp) != 10f)
                    {
                        data.ErrLimitUp = float.Parse(data.ErrLimitUp).ToString("f1");
                    }

                    if (TaskId.IndexOf(m_DETECT_RSLT.ITEM_ID) >= 0)
                    {
                        string[] wc1;
                        string[] wc2;
                        if (prjID == ProjectID.自热试验)
                        {
                            #region 自热试验
                            for (int n = 0; n < 2; n++)
                            {
                                wc1 = data.Error1.Split('|');
                                wc2 = data.Error2.Split('|');
                                if (n == 0)
                                {
                                    m_DETECT_RSLT.INT_CONVERT_ERR = double.Parse(wc1[wc1.Length - 1]).ToString("F2"); //误差化整值 *20
                                    StrGlys = "1.0";
                                    m_DETECT_RSLT.DATA_ITEM1 = m_DETECT_RSLT.INT_CONVERT_ERR; //误差1
                                    m_DETECT_RSLT.DATA_ITEM2 = "";
                                    m_DETECT_RSLT.DATA_ITEM3 = wc1[0]; //误差1
                                    m_DETECT_RSLT.DATA_ITEM4 = wc1[1]; //误差2
                                    m_DETECT_RSLT.DATA_ITEM5 = wc1[2]; //误差3
                                    m_DETECT_RSLT.DATA_ITEM6 = wc1[3]; //误差4
                                    m_DETECT_RSLT.DATA_ITEM7 = wc1[4]; //误差5
                                    m_DETECT_RSLT.DATA_ITEM8 = wc1[5]; //误差6
                                    m_DETECT_RSLT.DATA_ITEM9 = wc1[6]; //误差7
                                }
                                else
                                {
                                    m_DETECT_RSLT.INT_CONVERT_ERR = double.Parse(wc2[wc2.Length - 1]).ToString("F2"); //误差化整值 *20
                                    StrGlys = "0.5L";
                                    m_DETECT_RSLT.DATA_ITEM1 = m_DETECT_RSLT.INT_CONVERT_ERR; //误差1
                                    m_DETECT_RSLT.DATA_ITEM2 = "";
                                    m_DETECT_RSLT.DATA_ITEM3 = wc2[0]; //误差1
                                    m_DETECT_RSLT.DATA_ITEM4 = wc2[1]; //误差2
                                    m_DETECT_RSLT.DATA_ITEM5 = wc2[2]; //误差3
                                    m_DETECT_RSLT.DATA_ITEM6 = wc2[3]; //误差4
                                    m_DETECT_RSLT.DATA_ITEM7 = wc2[4]; //误差5
                                    m_DETECT_RSLT.DATA_ITEM8 = wc2[5]; //误差6
                                    m_DETECT_RSLT.DATA_ITEM9 = wc2[6]; //误差7
                                }

                                m_DETECT_RSLT.ERR_ABS = "±" + data.ErrLimitUp; //变差限 *20

                                string aaa = System.Guid.NewGuid().ToString();
                                string bbb = aaa.Replace("-", "");
                                dd += 10;
                                string ccc = bbb.Substring(6);
                                string fff = dd.ToString() + ccc;
                                m_DETECT_RSLT.READ_ID = fff; //主键

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (1 + Indexyxl).ToString(); //检定点的序号

                                m_DETECT_RSLT.IABC = StrYj; //相别
                                m_DETECT_RSLT.PF = StrGlys; //功率因数 *20
                                m_DETECT_RSLT.LOAD_CURRENT = "Imax"; //负载电流 *20
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = StrGlfx; //功率方向

                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位
                                m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论
                                m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                                m_DETECT_RSLT.DATA_ITEM10 = ""; //试验项目
                                m_DETECT_RSLT.DATA_ITEM11 = ""; //技术要求说明
                                m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后

                                #region 影响量试验 指标项 空值-国金
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = LstKey[i];
                                #endregion

                                Indexyxl++;

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + LstKey[i] + "' ");
                                    //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                            continue;
                            #endregion
                        }
                        else if (prjID == ProjectID.高次谐波)
                        {
                            #region 高次谐波
                            string xm = "电压";
                            string name = data.Name.Split('_')[1];

                            wc1 = data.Error1.Split('~');

                            string[] gcwc1 = wc1[0].Split('|'); //误差1
                            string[] gcwc2 = wc1[1].Split('|'); //误差1
                            string[] gcwcpj = wc1[2].Split('|'); //平均
                            //string[] gcwchz = wc1[3].Split('|'); //化整
                            wc2 = data.ErrValue.Split('|'); //偏差


                            for (int n = 0; n < wc2.Length; n++)
                            {
                                m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(wc2[n]).ToString("F2"); //误差化整值 *20
                                m_DETECT_RSLT.ERR_ABS = "±" + data.ErrLimitUp; ////变差限 *20

                                if (name == "电流电路")
                                {
                                    xm = "电流";
                                }
                                else if (name == "电压电路")
                                {
                                    xm = "电压";
                                }

                                string sj;
                                int xbcs;
                                if (n < 27)
                                {
                                    xbcs = n + 14;
                                    sj = "升序";
                                }
                                else
                                {
                                    xbcs = 40 + 27 - n;
                                    sj = "降序";
                                }

                                string aaa = System.Guid.NewGuid().ToString();
                                string bbb = aaa.Replace("-", "");
                                dd += 10;
                                string ccc = bbb.Substring(6);
                                string fff = dd.ToString() + ccc;
                                m_DETECT_RSLT.READ_ID = fff; //主键

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (1 + Indexyxl).ToString(); //检定点的序号

                                m_DETECT_RSLT.IABC = StrYj; //相别
                                m_DETECT_RSLT.PF = "1.0"; //功率因数 *20
                                m_DETECT_RSLT.LOAD_CURRENT = "Itr"; //负载电流 *20
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = StrGlfx; //功率方向

                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位
                                m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论
                                m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                                m_DETECT_RSLT.DATA_ITEM1 = gcwc1[n]; //误差1
                                m_DETECT_RSLT.DATA_ITEM2 = gcwc2[n]; //误差2
                                m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                m_DETECT_RSLT.DATA_ITEM6 = gcwcpj[n]; //误差平均值
                                m_DETECT_RSLT.DATA_ITEM7 = ""; //变差原始值，没有可不填

                                if (n == 0)
                                {
                                    m_DETECT_RSLT.DATA_ITEM10 = "无";
                                    m_DETECT_RSLT.DATA_ITEM11 = "无"; //项目
                                    m_DETECT_RSLT.DATA_ITEM12 = "无"; //谐波次数
                                }
                                else
                                {
                                    m_DETECT_RSLT.DATA_ITEM10 = name;
                                    m_DETECT_RSLT.DATA_ITEM11 = xm + sj; //项目
                                    m_DETECT_RSLT.DATA_ITEM12 = xbcs.ToString(); //谐波次数
                                }

                                #region 影响量试验 指标项 空值-国金
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = LstKey[i];
                                #endregion

                                Indexyxl++;

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + LstKey[i] + "' ");
                                    //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                            continue;
                            #endregion
                        }
                        else
                        {
                            #region 其他影响量实验
                            for (int qh = 0; qh < 2; qh++)
                            {
                                string[] strWc = data.Error1.Split('|');
                                if (strWc.Length <= 3) continue;
                                string[] strWc1 = data.Error2.Split('|');
                                if (strWc1.Length <= 3) continue;

                                entity.AVER_ERR1 = strWc[strWc.Length - 2];         //影响量前平均误差
                                entity.AVER_ERR2 = strWc1[strWc1.Length - 2];       //影响量后平均误差INT_ERR1 
                                entity.AVER_ERR = Math.Abs(Convert.ToSingle(entity.AVER_ERR1.Trim('+')) - Convert.ToSingle(entity.AVER_ERR2.Trim('+'))).ToString(); //平均值误差值
                                entity.AVER_ERR = data.ErrValue;

                                entity.INT_ERR1 = strWc[strWc.Length - 1];          //影响量前化整误差
                                entity.INT_ERR2 = strWc1[strWc1.Length - 1];        //影响量后化整误差
                                entity.INT_ERR = data.ErrInt;                   //平均值化整

                                //string StrDYTmp = "";
                                //m_DETECT_RSLT.DATA_ITEM1 = strWc[0]; //误差1（前），根据实测个数传递
                                //m_DETECT_RSLT.DATA_ITEM2 = strWc[1]; //误差2（前），根据实测个数传递
                                //m_DETECT_RSLT.DATA_ITEM3 = strWc1[0]; //误差3（后），根据实测个数传递
                                //m_DETECT_RSLT.DATA_ITEM4 = strWc1[1]; //误差4（后），根据实测个数传递
                                //m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5，根据实测个数传递
                                //m_DETECT_RSLT.DATA_ITEM6 = strWc[strWc.Length - 2] + "|" + strWc1[strWc1.Length - 2]; //误差平均值 前后

                                double Wctmp = double.Parse(strWc1[strWc1.Length - 2]) - double.Parse(strWc[strWc.Length - 2]);
                                if (Wctmp < 0)
                                {
                                    if ((data.ErrValue).IndexOf("-") == -1)
                                    {
                                        m_DETECT_RSLT.INT_CONVERT_ERR = "-" + double.Parse(data.ErrValue).ToString("F2"); //误差化整值
                                        m_DETECT_RSLT.DATA_ITEM7 = "-" + data.ErrValue; //变差原始值，没有可不填
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.INT_CONVERT_ERR = double.Parse(data.ErrValue).ToString("F2"); //误差化整值
                                        m_DETECT_RSLT.DATA_ITEM7 = data.ErrValue; //变差原始值，没有可不填
                                    }
                                }
                                else
                                {
                                    if ((data.ErrValue).IndexOf("+") == -1)
                                    {
                                        m_DETECT_RSLT.INT_CONVERT_ERR = "+" + double.Parse(data.ErrValue).ToString("F2"); //误差化整值
                                        m_DETECT_RSLT.DATA_ITEM7 = "+" + data.ErrValue; //变差原始值，没有可不填
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.INT_CONVERT_ERR = double.Parse(data.ErrValue).ToString("F2"); //误差化整值
                                        m_DETECT_RSLT.DATA_ITEM7 = data.ErrValue; //变差原始值，没有可不填
                                    }
                                }

                                if (strName.IndexOf("电压改变") != -1)
                                {
                                    if (float.Parse(prjSte[5]) >= 80)
                                    {
                                        m_DETECT_RSLT.ERR_ABS = "±" + data.ErrLimitUp; //变差限
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.ERR_ABS = data.ErrLimitDown + "|+" + data.ErrLimitUp; //误差限值
                                    }
                                    #region  电源电压影响 已注释
                                    //int IntVolt = int.Parse(StrDYTmp.Replace("%Un", ""));
                                    //if (IntVolt <= 70)
                                    //{
                                    //    if (entity.AVER_ERR2 == "+100.0000")
                                    //    {
                                    //        m_DETECT_RSLT.DATA_ITEM7 = "-100";//HP 电源电压影响低于80Un%不做改变量只做误差    //变差原始值，没有可不填
                                    //        m_DETECT_RSLT.ERR_ABS = "-100|+10";
                                    //        m_DETECT_RSLT.INT_CONVERT_ERR = "-100";//HP 电源电压影响低于80Un%不做改变量只做误差 //误差化整值
                                    //        entity.INT_ERR2 = "-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
                                    //        entity.AVER_ERR2 = "-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
                                    //        entity.INF_ERR2 = "-100|-100";//HP 电源电压影响未启动误差值应为-100，接口处修改

                                    //        m_DETECT_RSLT.DATA_ITEM1 = "-100"; //误差1（前），根据实测个数传递
                                    //        m_DETECT_RSLT.DATA_ITEM2 = "-100"; //误差2（前），根据实测个数传递
                                    //        m_DETECT_RSLT.DATA_ITEM3 = "-100"; //误差3（后），根据实测个数传递
                                    //        m_DETECT_RSLT.DATA_ITEM4 = "-100"; //误差4（后），根据实测个数传递
                                    //    }
                                    //    else
                                    //    {
                                    //        strWc1 = data.Error2.Split('|');

                                    //        if ((strWc1[strWc1.Length - 2]).IndexOf("+") == -1)
                                    //        {
                                    //            m_DETECT_RSLT.DATA_ITEM7 = "-" + double.Parse(strWc1[strWc1.Length - 2]).ToString(); //变差原始值，没有可不填
                                    //            m_DETECT_RSLT.INT_CONVERT_ERR = "-" + double.Parse(strWc1[strWc1.Length - 2]).ToString("F2");//HP 变差化整值重新计算 //误差化整值
                                    //        }
                                    //        else
                                    //        {
                                    //            m_DETECT_RSLT.DATA_ITEM7 = "+" + double.Parse(strWc1[strWc1.Length - 2]).ToString(); //变差原始值，没有可不填
                                    //            m_DETECT_RSLT.INT_CONVERT_ERR = "+" + double.Parse(strWc1[strWc1.Length - 2]).ToString("F2");//HP 变差化整值重新计算 //误差化整值
                                    //        }
                                    //        m_DETECT_RSLT.ERR_ABS = "-100|+10";
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    strWc = data.Error1.Split('|');
                                    //    strWc1 = data.Error2.Split('|');
                                    //    Wctmp = double.Parse(strWc1[strWc1.Length - 2]) - double.Parse(strWc[strWc.Length - 2]);
                                    //    if (Wctmp > 0)
                                    //    {
                                    //        m_DETECT_RSLT.INT_CONVERT_ERR = "+" + Wctmp.ToString("F2"); //误差化整值
                                    //        m_DETECT_RSLT.DATA_ITEM7 = "+" + Wctmp.ToString("F4"); //变差原始值，没有可不填
                                    //    }
                                    //    else
                                    //    {
                                    //        m_DETECT_RSLT.INT_CONVERT_ERR = Wctmp.ToString("F2"); //误差化整值
                                    //        m_DETECT_RSLT.DATA_ITEM7 = Wctmp.ToString("F4"); //变差原始值，没有可不填
                                    //    }
                                    //    m_DETECT_RSLT.ERR_ABS = "±" + (data.BPHUpLimit).ToString(); //误差限值
                                    //}
                                    #endregion
                                }
                                else
                                {
                                    m_DETECT_RSLT.ERR_ABS = "±" + data.ErrLimitUp; //变差限
                                }

                                if (qh == 0)
                                {
                                    sytj = m_DETECT_RSLT.DATA_ITEM10;

                                    if (meter.MD_JJGC == "IR46")
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = strWc[0]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM2 = strWc[1]; //误差2
                                        m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                        m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                        m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                        m_DETECT_RSLT.DATA_ITEM6 = strWc[strWc.Length - 2]; //误差平均值
                                        m_DETECT_RSLT.DATA_ITEM7 = ""; //变差原始值，没有可不填

                                        if (m_DETECT_RSLT.DATA_ITEM10 != "")
                                        {
                                            m_DETECT_RSLT.DATA_ITEM10 = "正常"; //实验条件
                                        }

                                        m_DETECT_RSLT.DATA_ITEM12 = "影响前"; //影响量前后

                                        if (prjID == ProjectID.辅助装置试验)
                                        {
                                            m_DETECT_RSLT.DATA_ITEM10 = "辅助装置工作"; //实验条件
                                            m_DETECT_RSLT.DATA_ITEM11 = "正常"; //实验要求
                                            m_DETECT_RSLT.DATA_ITEM12 = "影响后"; //影响量前后
                                        }

                                        if (prjID == ProjectID.逆相序试验)
                                        {
                                            m_DETECT_RSLT.DATA_ITEM10 = "平衡负载";//平衡负载 *20
                                            m_DETECT_RSLT.DATA_ITEM11 = "正相序";//相序 *20
                                        }

                                        if (isQH == false)
                                        {
                                            m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后
                                        }
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.DATA_ITEM5 = strWc[0]; //误差1
                                        if (prjID == ProjectID.频率改变)
                                        {
                                            m_DETECT_RSLT.DATA_ITEM5 = prjSte[5]; //频率
                                        }

                                        m_DETECT_RSLT.DATA_ITEM6 = strWc[1]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM7 = strWc[2]; //误差平均值
                                        m_DETECT_RSLT.DATA_ITEM8 = strWc[3]; //化整值
                                        m_DETECT_RSLT.DATA_ITEM9 = "影响前"; //影响量前后
                                    }
                                }
                                else
                                {
                                    if (isQH == false) continue;

                                    if (meter.MD_JJGC == "IR46")
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = strWc1[0]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM2 = strWc1[1]; //误差2
                                        m_DETECT_RSLT.DATA_ITEM3 = ""; //误差3
                                        m_DETECT_RSLT.DATA_ITEM4 = ""; //误差4
                                        m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5
                                        m_DETECT_RSLT.DATA_ITEM6 = strWc1[strWc1.Length - 2]; //误差平均值
                                        m_DETECT_RSLT.DATA_ITEM7 = ""; //变差原始值，没有可不填
                                        m_DETECT_RSLT.DATA_ITEM10 = sytj; //实验条件
                                        if (prjID == ProjectID.辅助装置试验)
                                        {
                                            m_DETECT_RSLT.DATA_ITEM10 = "辅助装置工作"; //实验条件
                                            m_DETECT_RSLT.DATA_ITEM11 = "载波"; //实验要求
                                        }
                                        if (prjID == ProjectID.逆相序试验)
                                        {
                                            m_DETECT_RSLT.DATA_ITEM10 = "平衡负载";//平衡负载 *20
                                            m_DETECT_RSLT.DATA_ITEM11 = "逆相序";//相序 *20
                                        }

                                        m_DETECT_RSLT.DATA_ITEM12 = "影响后"; //影响量前后
                                    }
                                    else
                                    {
                                        m_DETECT_RSLT.DATA_ITEM5 = strWc1[0]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM6 = strWc1[1]; //误差1
                                        m_DETECT_RSLT.DATA_ITEM7 = strWc1[2]; //误差平均值
                                        m_DETECT_RSLT.DATA_ITEM8 = strWc1[3]; //化整值
                                        m_DETECT_RSLT.DATA_ITEM9 = "影响后"; //影响量前后
                                    }
                                }

                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (1 + Indexyxl).ToString(); //检定点的序号

                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位
                                m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论
                                m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                                #region 影响量试验 指标项 空值-国金
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = LstKey[i];
                                #endregion

                                Indexyxl++;

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + LstKey[i] + "' ");
                                    if (qh == 0)
                                    {
                                        //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + LstKey[i] + "' ");
                                    }
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }


                //短时过电压试验
                #region 短时过电压试验 已注释
                //if (ConfigHelper.Instance.EquipmentType == "单相台")
                //{
                //    #region 短时过电压试验 私有值-国金

                //    string aa = System.Guid.NewGuid().ToString();
                //    string bb = aa.Replace("-", "");
                //    dd = dd + 10;
                //    string cc = bb.Substring(6);
                //    string ff = dd.ToString() + cc;
                //    m_DETECT_RSLT.READ_ID = ff; //主键

                //    m_DETECT_RSLT.ITEM_ID = "072"; //试验项ID
                //    m_DETECT_RSLT.ITEM_NAME = "短时过电压试验"; //试验项名称
                //    m_DETECT_RSLT.TEST_GROUP = "短时过电压试验"; //试验分组

                //    m_DETECT_RSLT.DETECT_ITEM_POINT = (LstKey.Count + 1).ToString(); //检定点的序号

                //    m_DETECT_RSLT.TEST_CATEGORIES = "短时过电压试验"; //试验分项
                //    m_DETECT_RSLT.IABC = GetPCode("currentPhaseCode", zjxfs); //相别
                //    m_DETECT_RSLT.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
                //    m_DETECT_RSLT.LOAD_CURRENT = GetPCode("meterTestCurLoad", "5");//负载电流
                //    m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"); //功率方向                                

                //    m_DETECT_RSLT.INT_CONVERT_ERR = "0.0";//误差化整值
                //    m_DETECT_RSLT.ERR_ABS = "0|0";//误差限值
                //    m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //    m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //    m_DETECT_RSLT.DETECT_RESULT = "01"; //分项结论

                //    m_DETECT_RSLT.DATA_ITEM1 = "短时过电压"; //试验条件
                //    m_DETECT_RSLT.DATA_ITEM2 = "0|0"; //允许误差偏移%                                     
                //    m_DETECT_RSLT.DATA_ITEM3 = "0"; //实际误差偏移%

                //    #region 短时过电压试验 指标项 空值-国金
                //    m_DETECT_RSLT.DATA_ITEM4 = "";
                //    m_DETECT_RSLT.DATA_ITEM5 = "";
                //    m_DETECT_RSLT.DATA_ITEM6 = "";
                //    m_DETECT_RSLT.DATA_ITEM7 = "";
                //    m_DETECT_RSLT.DATA_ITEM8 = "";
                //    m_DETECT_RSLT.DATA_ITEM9 = "";
                //    m_DETECT_RSLT.DATA_ITEM10 = "";
                //    m_DETECT_RSLT.DATA_ITEM11 = "";
                //    m_DETECT_RSLT.DATA_ITEM12 = "";
                //    m_DETECT_RSLT.DATA_ITEM13 = "";
                //    m_DETECT_RSLT.DATA_ITEM14 = "";
                //    m_DETECT_RSLT.DATA_ITEM15 = "";
                //    m_DETECT_RSLT.DATA_ITEM16 = "";
                //    m_DETECT_RSLT.DATA_ITEM17 = "";
                //    m_DETECT_RSLT.DATA_ITEM18 = "";
                //    m_DETECT_RSLT.DATA_ITEM19 = "";
                //    m_DETECT_RSLT.DATA_ITEM20 = "";

                //    #endregion

                //    m_DETECT_RSLT.CHK_CONC_CODE = "01"; //检定总结论
                //    #endregion

                //    sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //    if (sqls.Count > 0)
                //    {
                //        delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //        foreach (string sql in sqls)
                //        {
                //            sqlList.Add(sql);
                //        }
                //    }
                //}
                #endregion
            }
            #endregion

            #region 负载电流快速改变试验-国金 *20
            if (meter.MeterDgns.Count > 0)
            {
                string strResult = "";
                string ItemKey = ProjectID.负载电流快速改变;
                if (meter.MeterDgns.ContainsKey(ItemKey))
                {

                    string[] arr = meter.MeterDgns[ItemKey].Value.Split('|');
                    string[] testvalue = meter.MeterDgns[ItemKey].TestValue.Split('|');

                    if (meter.MeterDgns.ContainsKey(ItemKey))
                    {
                        strResult = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";
                    }

                    itemId = "0145";

                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        #region 负载电流快速改变试验 私有值-国金

                        int sz = 6;
                        string sytj = "";
                        //for (int i = 0; i < testvalue.Length / 3; i++)
                        for (int i = 0; i < 3; i++)
                        {
                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "负载电流快速改变试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "负载电流快速改变试验"; //试验分组 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "负载电流快速改变试验"; //试验分项
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0"; //功率因数 *20
                            m_DETECT_RSLT.LOAD_CURRENT = "10Itr"; //负载电流 *20
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                            if (i == 0)
                            {
                                sz = 4;
                                sytj = "保持10s，中断10s，持续4h";
                                m_DETECT_RSLT.DATA_ITEM1 = arr[0];
                                m_DETECT_RSLT.DATA_ITEM2 = arr[1];
                                m_DETECT_RSLT.DATA_ITEM3 = arr[2];
                                m_DETECT_RSLT.DATA_ITEM4 = arr[3];
                                m_DETECT_RSLT.DATA_ITEM5 = testvalue[0] + "s";
                                m_DETECT_RSLT.DATA_ITEM6 = testvalue[1] + "s";
                            }
                            else if (i == 1)
                            {
                                sz = 9;
                                sytj = "保持5s，中断5s，持续4h";
                                m_DETECT_RSLT.DATA_ITEM1 = arr[5];
                                m_DETECT_RSLT.DATA_ITEM2 = arr[6];
                                m_DETECT_RSLT.DATA_ITEM3 = arr[7];
                                m_DETECT_RSLT.DATA_ITEM4 = arr[8];
                                m_DETECT_RSLT.DATA_ITEM5 = testvalue[3] + "s";
                                m_DETECT_RSLT.DATA_ITEM6 = testvalue[4] + "s";

                            }
                            else if (i == 2)
                            {
                                sz = 14;
                                sytj = "保持5s，中断0.5s，持续4h";
                                m_DETECT_RSLT.DATA_ITEM1 = arr[10];
                                m_DETECT_RSLT.DATA_ITEM2 = arr[11];
                                m_DETECT_RSLT.DATA_ITEM3 = arr[12];
                                m_DETECT_RSLT.DATA_ITEM4 = arr[13];
                                m_DETECT_RSLT.DATA_ITEM5 = testvalue[6] + "s";
                                m_DETECT_RSLT.DATA_ITEM6 = testvalue[7] + "s";
                            }
                            //else if (i == 3)
                            //{
                            //    sz = 21;
                            //    sytj = "保持0.5s，中断5s，持续4h";
                            //}

                            //if (arr[4].Length > 5) arr[6] = arr[6].Substring(0, 4);//误差上升平均值
                            m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(arr[sz]).ToString("f2");//误差化整值 *20


                            m_DETECT_RSLT.ERR_ABS = "±" + float.Parse(arr[0]).ToString("f1"); //误差限值 *20
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论

                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = sytj; //实验条件 *20
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";

                            #region 负载电流快速改变试验 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = ItemKey;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 安全认证试验-国金  *13 *20
            if (meter.MeterCostControls.Count > 0)
            {
                string ItemKey = ProjectID.身份认证;
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    //itemId = "067";
                    itemId = "056";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0167";
                    }
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            #region 安全认证试验 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "安全认证试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "安全认证试验"; //试验分组 *13 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "安全认证试验"; //试验分项

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                            if (i == 0)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = "身份认证时效性"; //试验项目 *13 *20
                                m_DETECT_RSLT.DATA_ITEM2 = "身份认证时效性测试"; //试验分项 *13 *20
                            }
                            else if (i == 1)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = "身份证认证失效性"; //试验项目 *20
                                m_DETECT_RSLT.DATA_ITEM2 = "身份证认证失效性测试"; //试验分项 *20

                            }
                            else if (i == 2)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = "防攻击能力"; //试验项目 *20
                                m_DETECT_RSLT.DATA_ITEM2 = "防攻击能力测试"; //试验分项 *20
                            }
                            else if (i == 3)
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = "红外认证能力"; //试验项目 *20
                                m_DETECT_RSLT.DATA_ITEM2 = "红外认证功能测试"; //试验分项 *20
                            }

                            #region 安全认证试验 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 密钥更新试验-国金  *13 *20
            if (meter.MeterCostControls.Count > 0)
            {
                string ItemKey = ProjectID.密钥更新;

                if (!meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    ItemKey = ProjectID.密钥更新_预先调试;
                }
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    //itemId = "068";
                    itemId = "057";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0168";
                    }
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        #region 密钥更新试验 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "密钥更新试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "密钥更新试验"; //试验分组 *13 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "密钥更新试验"; //试验分项

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        m_DETECT_RSLT.DATA_ITEM1 = "密钥更新功能"; //试验项目 *13 *20
                        m_DETECT_RSLT.DATA_ITEM2 = "正确参数的密钥下装"; //试验分项 *13 *20

                        #region 密钥更新试验 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }

                ItemKey = ProjectID.密钥恢复;
                if (!meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    ItemKey = ProjectID.密钥恢复_预先调试;
                }
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        #region 密钥恢复试验 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "密钥更新试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "密钥更新试验"; //试验分组 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "密钥更新试验"; //试验分项

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *20

                        m_DETECT_RSLT.DATA_ITEM1 = "密钥恢复功能"; //试验项目 *20
                        m_DETECT_RSLT.DATA_ITEM2 = "正确参数的密钥恢复"; //试验分项 *20

                        #region 密钥更新试验 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
            }
            #endregion

            #region 远程控制试验-国金 *13 *20
            if (meter.MeterCostControls.Count > 0)
            {
                //itemId = "069";
                itemId = "058";
                if (meter.MD_JJGC == "IR46")
                {
                    itemId = "0169";
                }
                //跳合闸测试
                #region 远程控制试验-跳合闸测试-国金
                string ItemKey = ProjectID.远程控制;
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        #region 远程控制试验-跳合闸测试 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "远程控制试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "远程控制试验"; //试验分组 *13 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "远程控制试验"; //试验分项                              

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "跳合闸"; //试验项目 *20
                            m_DETECT_RSLT.DATA_ITEM2 = "跳合闸测试"; //试验分项 *20
                        }
                        else
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "拉合闸"; //试验项目 *13
                            m_DETECT_RSLT.DATA_ITEM2 = ""; //试验分项 *13
                        }

                        #region 远程控制试验-跳合闸测试 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
                #endregion

                //保电功能测试
                #region 远程控制试验-保电功能测试-跳电闸-国金
                ItemKey = ProjectID.远程保电;
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        #region 远程控制试验-保电功能测试 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "远程控制试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "远程控制试验"; //试验分组 *13 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "远程控制试验"; //试验分项                              

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        m_DETECT_RSLT.DATA_ITEM1 = "保电"; //试验项目 *13 *20
                        m_DETECT_RSLT.DATA_ITEM2 = "保电功能测试"; //试验分项 *20

                        #region 远程控制试验-保电功能测试 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
                #endregion

                //报警测试
                #region 远程控制试验-报警测试-国金
                ItemKey = ProjectID.报警功能;
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        #region 远程控制试验-报警测试 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "远程控制试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "远程控制试验"; //试验分组 *13 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "远程控制试验"; //试验分项                             

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        m_DETECT_RSLT.DATA_ITEM1 = "报警"; //试验项目 *13 *20
                        m_DETECT_RSLT.DATA_ITEM2 = "报警测试"; //试验分项 *20

                        #region 远程控制试验-报警测试 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region 参数更新试验-国金 *13 *20
            if (meter.MeterCostControls.Count > 0)
            {
                //itemId = "070";
                itemId = "059";
                if (meter.MD_JJGC == "IR46")
                {
                    itemId = "0170";
                }
                //安全模式参数测试
                #region 远程控制试验-安全模式参数测试-国金
                string ItemKey = ProjectID.参数设置;
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            #region 参数更新试验-安全模式参数测试 私有值-国金
                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "参数更新试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "参数更新试验"; //试验分组 *13 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "参数更新试验"; //试验分项                              

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                            if (meter.MD_JJGC == "IR46")
                            {
                                m_DETECT_RSLT.DATA_ITEM1 = "安全模式参数测试"; //试验项目 *20

                                if (i == 0)
                                {
                                    m_DETECT_RSLT.DATA_ITEM2 = "公钥下安全模式参数测试"; //试验分项 *20
                                }
                                else if (i == 1)
                                {
                                    m_DETECT_RSLT.DATA_ITEM2 = "私钥下安全模式参数测试"; //试验分项 *20
                                }
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "电能表清零"; //试验项目 *13
                                    m_DETECT_RSLT.DATA_ITEM2 = "公钥下二类参数测试"; //试验分项 *13
                                }
                                else if (i == 1)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "二类参数更新"; //试验项目 *13
                                    m_DETECT_RSLT.DATA_ITEM2 = "私钥下二类参数测试"; //试验分项 *13
                                }
                            }

                            #region 参数更新试验-安全模式参数测试 指标项 空值-国金
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }

                    }
                }
                #endregion

                //数据回抄功能
                #region 远程控制试验-数据回抄功能-国金
                ItemKey = ProjectID.数据回抄;
                if (meter.MeterCostControls.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        #region 参数更新试验-数据回抄功能 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "参数更新试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "参数更新试验"; //试验分组 *13 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "参数更新试验"; //试验分项                              

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "数据回抄功能"; //试验项目 *20
                            m_DETECT_RSLT.DATA_ITEM2 = "数据回抄测试"; //试验分项 *20
                        }
                        else
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "电能表清零"; //试验项目 *13
                            m_DETECT_RSLT.DATA_ITEM2 = "数据回抄测试"; //试验分项 *13
                        }

                        #region 参数更新试验-数据回抄功能 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterCostControls[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterCostControls[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }

                #endregion
                #region 远程控制试验-电能表清零功能-国金
                ItemKey = ProjectID.电量清零;
                if (!meter.MeterDgns.ContainsKey(ItemKey))
                {
                    ItemKey = ProjectID.费控_电量清零;
                }
                if (meter.MeterCostControls.ContainsKey(ItemKey) || meter.MeterDgns.ContainsKey(ItemKey))
                {
                    if (TaskId.IndexOf(itemId) >= 0)
                    {
                        string strResult;
                        #region 参数更新试验-电能表清零功能 私有值-国金

                        try
                        {
                            MeterFK data = meter.MeterCostControls[ItemKey];
                            strResult = data.Result == ConstHelper.合格 ? "01" : data.Result == ConstHelper.不合格 ? "02" : "03";

                        }
                        catch
                        {
                            MeterDgn data = meter.MeterDgns[ItemKey];
                            strResult = data.Result == ConstHelper.合格 ? "01" : data.Result == ConstHelper.不合格 ? "02" : "03";
                        }
                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "参数更新试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "参数更新试验"; //试验分组 *13 *20

                        m_DETECT_RSLT.DETECT_ITEM_POINT = "1"; //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "参数更新试验"; //试验分项                                

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = strResult; //分项结论 *13 *20

                        if (meter.MD_JJGC == "IR46")
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "电能表清零功能"; //试验项目 *20
                            m_DETECT_RSLT.DATA_ITEM2 = "远程清零测试"; //试验分项 *20
                        }
                        else
                        {
                            m_DETECT_RSLT.DATA_ITEM1 = "二类参数更新"; //试验项目 *13
                            m_DETECT_RSLT.DATA_ITEM2 = "远程清零测试"; //试验分项 *13
                        }

                        #region 参数更新试验-电能表清零功能 指标项 空值-国金
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = ItemKey;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = strResult; //检定总结论
                        #endregion

                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region 电能量分项设置与累计存储试验-国金 *13
            if (meter.MeterFunctions.Count > 0)
            {
                iIndex = 0;
                string[] Arr_ID = new string[meter.MeterFunctions.Keys.Count];
                meter.MeterFunctions.Keys.CopyTo(Arr_ID, 0);

                for (int i = 0; i < Arr_ID.Length; i++)
                {
                    string _ID = Arr_ID[i];
                    //string strXmh;
                    string glfx = "正向有功";
                    string zong = "";
                    if (_ID == ProjectID.最大需量功能 || _ID == ProjectID.计量功能)
                    {
                        //MeterFunction MeterFunctions = meter.MeterFunctions[_ID];

                        string strJl = meter.MeterFunctions[_ID].Result;
                        //strXmh = MeterFunctions.Name;
                        int counts = 4;
                        if (_ID == ProjectID.最大需量功能)
                        {
                            counts = 4;
                        }
                        else if (_ID == ProjectID.计量功能)
                        {
                            if (meter.MD_WiringMode == "单相")
                            {
                                counts = 7;
                            }
                            else
                            {
                                counts = 23;
                            }
                        }

                        for (int j = 0; j < counts; j++)
                        {

                            if (_ID == ProjectID.最大需量功能)
                            {
                                switch (j)
                                {
                                    case 0:
                                        glfx = "正向有功";
                                        zong = "总";
                                        break;
                                    case 1:
                                        glfx = "正向有功";
                                        zong = "T1~T12";
                                        break;
                                    case 2:
                                        glfx = "反向有功";
                                        zong = "总";
                                        break;
                                    case 3:
                                        glfx = "反向有功";
                                        zong = "T1~T12";
                                        break;
                                }
                            }
                            else if (_ID == ProjectID.计量功能)
                            {
                                if (meter.MD_WiringMode == "单相")
                                {
                                    switch (j)
                                    {
                                        case 0:
                                            glfx = "组合有功";
                                            zong = "总";
                                            break;
                                        case 1:
                                            glfx = "组合有功";
                                            zong = "T1~T12";
                                            break;
                                        case 2:
                                            glfx = "正向有功";
                                            zong = "总";
                                            break;
                                        case 3:
                                            glfx = "正向有功";
                                            zong = "T1~T12";
                                            break;
                                        case 4:
                                            glfx = "反向有功";
                                            zong = "总";
                                            break;
                                        case 5:
                                            glfx = "反向有功";
                                            zong = "T1~T12";
                                            break;
                                        case 6:
                                            glfx = "组合有功";
                                            zong = "";
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (j)
                                    {
                                        case 0:
                                            glfx = "组合有功";
                                            zong = "总";
                                            break;
                                        case 1:
                                            glfx = "组合有功";
                                            zong = "T1~T12";
                                            break;
                                        case 2:
                                            glfx = "正向有功";
                                            zong = "总";
                                            break;
                                        case 3:
                                            glfx = "正向有功";
                                            zong = "T1~T12";
                                            break;
                                        case 4:
                                            glfx = "反向有功";
                                            zong = "总";
                                            break;
                                        case 5:
                                            glfx = "反向有功";
                                            zong = "T1~T12";
                                            break;
                                        case 6:
                                            glfx = "正向分项有功";
                                            zong = "总";
                                            break;
                                        case 7:
                                            glfx = "反向分项有功";
                                            zong = "总";
                                            break;
                                        case 8:
                                            glfx = "组合无功1";
                                            zong = "总";
                                            break;
                                        case 9:
                                            glfx = "组合无功1";
                                            zong = "T1~T12";
                                            break;
                                        case 10:
                                            glfx = "组合无功2";
                                            zong = "总";
                                            break;
                                        case 11:
                                            glfx = "组合无功2";
                                            zong = "T1~T12";
                                            break;
                                        case 12:
                                            glfx = "第一象限无功";
                                            zong = "总";
                                            break;
                                        case 13:
                                            glfx = "第一象限无功";
                                            zong = "T1~T12";
                                            break;
                                        case 14:
                                            glfx = "第二象限无功";
                                            zong = "总";
                                            break;
                                        case 15:
                                            glfx = "第二象限无功";
                                            zong = "T1~T12";
                                            break;
                                        case 16:
                                            glfx = "第三象限无功";
                                            zong = "总";
                                            break;
                                        case 17:
                                            glfx = "第三象限无功";
                                            zong = "T1~T12";
                                            break;
                                        case 18:
                                            glfx = "第四象限无功";
                                            zong = "总";
                                            break;
                                        case 19:
                                            glfx = "第四象限无功";
                                            zong = "T1~T12";
                                            break;
                                        case 20:
                                            glfx = "组合有功";
                                            zong = "";
                                            break;
                                        case 21:
                                            glfx = "组合无功1";
                                            zong = "";
                                            break;
                                        case 22:
                                            glfx = "组合无功2";
                                            zong = "";
                                            break;
                                    }
                                }
                            }

                            //itemId = "071";
                            itemId = "065";
                            if (meter.MD_JJGC == "IR46")
                            {
                                itemId = "0171";
                            }
                            if (TaskId.IndexOf(itemId) != -1)
                            {
                                #region 电能量分项设置与累计存储试验 私有值-国金

                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                m_DETECT_RSLT.ITEM_NAME = "电能量分项设置与累计存储试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "电能量分项设置与累计存储试验"; //试验分组

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + i).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "电能量分项设置与累计存储试验"; //试验分项                               

                                m_DETECT_RSLT.IABC = zjxfs; //相别
                                m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = glfx; //功率方向  

                                m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                                m_DETECT_RSLT.ERR_ABS = "";//误差限值
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论 *13

                                m_DETECT_RSLT.DATA_ITEM1 = glfx; //试验项目2 *13

                                if (_ID == ProjectID.最大需量功能)
                                {
                                    m_DETECT_RSLT.DATA_ITEM2 = "存储12个结算日最大需量"; //试验分项  项目类别（计量功能、最大需量功能） *13
                                }
                                else if (_ID == ProjectID.计量功能)
                                {
                                    m_DETECT_RSLT.DATA_ITEM2 = "存储12个结算日电能量"; //试验分项  项目类别（计量功能、最大需量功能） *13

                                    float js;
                                    if (meter.MD_WiringMode == "单相")
                                    {
                                        js = 6;
                                    }
                                    else
                                    {
                                        js = 20;
                                    }

                                    if (j >= js)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM2 = "可设置"; //glfx *13
                                    }
                                }
                                m_DETECT_RSLT.DATA_ITEM3 = zong; //*13

                                #region 电能量分项设置与累计存储试验 指标项 空值-国金
                                m_DETECT_RSLT.DATA_ITEM4 = "";
                                m_DETECT_RSLT.DATA_ITEM5 = "";
                                m_DETECT_RSLT.DATA_ITEM6 = "";
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = _ID;

                                #endregion

                                m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion
                                iIndex++;
                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            #endregion

            #region 费率和时段试验-国金 *13 *20
            if (meter.MeterFunctions.Count > 0)
            {
                iIndex = 0;

                string[] keys = new string[meter.MeterFunctions.Keys.Count];
                meter.MeterFunctions.Keys.CopyTo(keys, 0);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i] == ProjectID.费率时段功能)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            MeterFunction MeterFunctions = meter.MeterFunctions[keys[i]];

                            string strJl = MeterFunctions.Result;//结论

                            //itemId = "071";
                            itemId = "066";
                            if (meter.MD_JJGC == "IR46")
                            {
                                itemId = "0172";
                            }
                            if (TaskId.IndexOf(itemId) != -1)
                            {
                                #region 费率和时段试验 私有值-国金

                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                m_DETECT_RSLT.ITEM_NAME = "费率和时段试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "费率和时段试验"; //试验分组

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "费率和时段试验"; //试验分项                           

                                m_DETECT_RSLT.IABC = zjxfs; //相别
                                m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                                m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                                m_DETECT_RSLT.ERR_ABS = "";//误差限值
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                                //string[] strQhqCF = MeterFunctions5.Value.Split('|');

                                if (j == 0)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "两套时区、时段表"; //试验项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "自动切换"; //试验分项 *13 *20
                                }
                                else if (j == 1)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "日时段表"; //试验项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "可设置"; //试验分项 *13 *20
                                }
                                else if (j == 2)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "时区表"; //试验项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "可设置"; //试验分项 *13 *20
                                }

                                #region 费率和时段试验 指标项 空值-国金 
                                m_DETECT_RSLT.DATA_ITEM3 = "";
                                m_DETECT_RSLT.DATA_ITEM4 = "";
                                m_DETECT_RSLT.DATA_ITEM5 = "";
                                m_DETECT_RSLT.DATA_ITEM6 = "";
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = keys[i];
                                #endregion

                                m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 事件记录试验-国金 *13 *20
            if (meter.MeterSjJLgns.Count > 0)
            {
                iIndex = 0;

                string[] Arr_ID = new string[meter.MeterSjJLgns.Keys.Count];
                meter.MeterSjJLgns.Keys.CopyTo(Arr_ID, 0);


                for (int i = 0; i < Arr_ID.Length; i++)
                {
                    string _ID = Arr_ID[i];
                    //string strXmh;
                    string strXmBh = "";

                    if (!meter.MeterSjJLgns.ContainsKey(_ID)) continue;


                    MeterSjJLgn MeterSjJLgns = meter.MeterSjJLgns[_ID];
                    //string[] STRSj = MeterSjJLgns.Value.Split('|');//事件其他数据
                    string StrID = MeterSjJLgns.PrjID;
                    string strJl = MeterSjJLgns.Result;
                    //strXmh = MeterSjJLgns.Name;
                    switch (StrID)
                    {
                        case ProjectID.失压记录:
                            strXmBh = "各相失压";//各相失压
                            break;
                        case ProjectID.全失压记录:
                            strXmBh = "全失压";//全失压
                            break;
                        case ProjectID.欠压记录:
                            strXmBh = "各相欠压";//各相欠压
                            break;
                        case ProjectID.过压记录:
                            strXmBh = "各相过压";//各相过压
                            break;
                        case ProjectID.断相记录:
                            strXmBh = "各相断相";//各相断相
                            break;
                        case ProjectID.电压逆向序记录:
                            strXmBh = "电压逆相序";//电压逆相序
                            break;
                        case ProjectID.电压不平衡记录:
                            strXmBh = "电压不平衡";//电压不平衡
                            break;
                        case ProjectID.电流不平衡记录:
                            strXmBh = "电流不平衡";//电流不平衡
                            break;
                        case ProjectID.失流记录:
                            strXmBh = "各相失流";//各相失流
                            break;
                        case ProjectID.过流记录:
                            strXmBh = "各相过流";//各相过流
                            break;
                        case ProjectID.断流记录:
                            strXmBh = "各相断流";//各相断流
                            break;
                        case ProjectID.需量清零记录:
                            strXmBh = "需量清零";//需量清零
                            break;
                        case ProjectID.事件清零记录:
                            strXmBh = "事件清零";//事件清零
                            break;
                        case ProjectID.掉电记录:
                            strXmBh = "掉电";//掉电
                            break;
                        case ProjectID.校时记录:
                            strXmBh = "校时";//校时
                            break;
                        case ProjectID.编程记录:
                            strXmBh = "编程";//编程
                            break;
                        case ProjectID.跳闸合闸事件记录:
                            strXmBh = "拉闸事件";//拉闸事件
                            break;
                        case ProjectID.广播校时事件记录:
                            strXmBh = "广播校时";//广播校时
                            break;
                        case ProjectID.时钟故障事件记录:
                            strXmBh = "时钟故障";//时钟故障
                            break;
                        case ProjectID.零线电流异常事件记录:
                            strXmBh = "零线电流异常";//零线电流异常
                            break;
                        case ProjectID.事件跟随上报:
                            strXmBh = "事件跟随上报";//事件跟随上报
                            break;
                        case ProjectID.事件主动上报_载波:
                            strXmBh = "事件主动上报";//事件主动上报
                            break;
                        case ProjectID.电表清零记录:
                            strXmBh = "电能表清零";//电能表清零
                            break;
                        //case ProjectID.过载记录:
                        //    strXmBh = "过载记录";//过载记录
                        //    break;
                        //case ProjectID.电流逆向序记录:
                        //    strXmBh = "电流逆相序";//电流逆相序
                        //    break;
                        //case ProjectID.潮流反向记录:
                        //    strXmBh = "潮流反向记录";//潮流反向记录
                        //    break;
                        //case ProjectID.功率反向记录:
                        //    strXmBh = "功率反向记录";//功率反向记录
                        //    break;
                        //case ProjectID.需量超限记录:
                        //    strXmBh = "需量超限记录";//需量超限记录
                        //    break;
                        //case ProjectID.功率因数超下限记录:
                        //    strXmBh = "功率因数超下限记录";//功率因数超下限记录
                        //    break;
                        //case ProjectID.事件主动上报_载波:
                        //    strXmBh = "合闸事件";//合闸事件
                        //    break;
                        default:
                            break;
                    }

                    //_ = new M_QT_EVENTLOG_MET_CONC();


                    itemId = "067";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0173";
                    }

                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        #region 事件记录试验 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "事件记录试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "事件记录试验"; //试验分组

                        m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "事件记录试验"; //试验分项                              

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                        m_DETECT_RSLT.DATA_ITEM1 = strXmBh; //试验项目 *13 *20

                        if (strXmBh.IndexOf("掉电") != -1)
                        {
                            m_DETECT_RSLT.DATA_ITEM2 = "最近100次事件记录"; //试验分项 *13  掉电记录 
                        }
                        else if (strXmBh.IndexOf("广播校时") != -1)
                        {
                            m_DETECT_RSLT.DATA_ITEM2 = "最近100次事件记录"; //试验分项 *13  广播校时 
                        }
                        else if (strXmBh.IndexOf("电能表清零") != -1)
                        {
                            m_DETECT_RSLT.DATA_ITEM2 = "永久记录"; //试验分项 *20 电能表清零
                        }
                        else if (strXmBh.IndexOf("跟随上报") != -1)
                        {
                            m_DETECT_RSLT.DATA_ITEM2 = "按照模式字及属性配置要求实现跟随上报"; //试验分项 *20
                        }
                        else if (strXmBh.IndexOf("主动上报") != -1)
                        {
                            m_DETECT_RSLT.DATA_ITEM2 = "按照模式字及属性配置要求实现主动上报"; //试验分项 *20
                        }
                        else
                        {
                            m_DETECT_RSLT.DATA_ITEM2 = "最近10次事件记录"; //试验分项 *13 其他
                        }

                        #region 事件记录试验 指标项 空值-国金 
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = _ID;

                        #endregion

                        m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        iIndex++;
                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }

                        if (StrID == ProjectID.跳闸合闸事件记录)
                        {
                            #region 事件记录试验 私有值-国金

                            aa = System.Guid.NewGuid().ToString();
                            bb = aa.Replace("-", "");
                            dd += 10;
                            cc = bb.Substring(6);
                            ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "事件记录试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "事件记录试验"; //试验分组

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "事件记录试验"; //试验分项                              

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                            strXmBh = "合闸事件";
                            m_DETECT_RSLT.DATA_ITEM1 = strXmBh; //试验项目 *13 *20


                            m_DETECT_RSLT.DATA_ITEM2 = "最近10次事件记录"; //试验分项 *13 其他


                            #region 事件记录试验 指标项 空值-国金 
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = _ID;

                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            iIndex++;
                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 冻结功能试验-国金 *13 *20
            if (meter.MeterFreezes.Count > 0)
            {
                iIndex = 0;

                string[] keys = new string[meter.MeterFreezes.Keys.Count];
                meter.MeterFreezes.Keys.CopyTo(keys, 0);

                for (int i = 0; i < keys.Length; i++)
                {
                    string itemKey = keys[i];
                    if (!meter.MeterFreezes.ContainsKey(itemKey)) continue;

                    MeterFreeze meterFreeze = meter.MeterFreezes[itemKey];

                    string strDjFsfz = meterFreeze.Name;//冻结方式  试验分组
                    string strDjID = meterFreeze.PrjID;//冻结方式  试验分项
                    string strJl = meterFreeze.Result;
                    //string[] strXmBh = meterFreeze.Value.Split('|');

                    //itemId = "0174";
                    itemId = "068";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0174";
                    }
                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        #region 冻结功能试验 私有值-国金

                        string aa = System.Guid.NewGuid().ToString();
                        string bb = aa.Replace("-", "");
                        dd += 10;
                        string cc = bb.Substring(6);
                        string ff = dd.ToString() + cc;
                        m_DETECT_RSLT.READ_ID = ff; //主键

                        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                        m_DETECT_RSLT.ITEM_NAME = "冻结功能试验"; //试验项名称
                        m_DETECT_RSLT.TEST_GROUP = "冻结功能试验"; //试验分组 *13

                        m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                        m_DETECT_RSLT.TEST_CATEGORIES = "冻结功能试验"; //试验分项                              

                        m_DETECT_RSLT.IABC = zjxfs; //相别
                        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                        m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                        m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论 *13

                        string syyq = "保存最后3次";
                        switch (strDjID)
                        {
                            case ProjectID.瞬时冻结:
                                syyq = "保存最后3次";
                                break;
                            case ProjectID.整点冻结:
                                syyq = "存储254次";
                                break;
                            case ProjectID.日冻结:
                                syyq = "存储62次";
                                break;
                            case ProjectID.月冻结:
                                syyq = "存储24次";
                                break;
                            case ProjectID.结算日冻结:
                                syyq = "存储12次";
                                break;
                            case ProjectID.约定冻结:
                                strDjFsfz = "约定冻结时区转换";
                                if (meter.MD_JJGC != "IR46")
                                {
                                    strDjFsfz = "两套时区表切换冻结";
                                }
                                syyq = "保存最后2次";
                                break;
                            case ProjectID.分钟冻结:
                                syyq = "存储254次";
                                break;
                            default:
                                break;
                        }

                        if (meter.MD_JJGC != "IR46")
                        {
                            syyq = "冻结量存储正确";
                        }
                        m_DETECT_RSLT.DATA_ITEM1 = strDjFsfz; //试验项目1 *13
                        m_DETECT_RSLT.DATA_ITEM2 = syyq; //试验项目2 *13   

                        #region 事件记录试验 指标项 空值-国金 
                        m_DETECT_RSLT.DATA_ITEM3 = "";
                        m_DETECT_RSLT.DATA_ITEM4 = "";
                        m_DETECT_RSLT.DATA_ITEM5 = "";
                        m_DETECT_RSLT.DATA_ITEM6 = "";
                        m_DETECT_RSLT.DATA_ITEM7 = "";
                        m_DETECT_RSLT.DATA_ITEM8 = "";
                        m_DETECT_RSLT.DATA_ITEM9 = "";
                        m_DETECT_RSLT.DATA_ITEM10 = "";
                        m_DETECT_RSLT.DATA_ITEM11 = "";
                        m_DETECT_RSLT.DATA_ITEM12 = "";
                        m_DETECT_RSLT.DATA_ITEM13 = "";
                        m_DETECT_RSLT.DATA_ITEM14 = "";
                        m_DETECT_RSLT.DATA_ITEM15 = "";
                        m_DETECT_RSLT.DATA_ITEM16 = "";
                        m_DETECT_RSLT.DATA_ITEM17 = "";
                        m_DETECT_RSLT.DATA_ITEM18 = "";
                        m_DETECT_RSLT.DATA_ITEM19 = "";
                        m_DETECT_RSLT.DATA_ITEM20 = itemKey;

                        #endregion
                        m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                        #endregion

                        iIndex++;
                        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                        if (sqls.Count > 0)
                        {
                            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                            foreach (string sql in sqls)
                            {
                                sqlList.Add(sql);
                            }
                        }

                        if (strDjID == ProjectID.约定冻结)
                        {
                            #region 冻结功能试验 私有值-国金

                            aa = System.Guid.NewGuid().ToString();
                            bb = aa.Replace("-", "");
                            dd += 10;
                            cc = bb.Substring(6);
                            ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "冻结功能试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "冻结功能试验"; //试验分组 *13

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "冻结功能试验"; //试验分项                              

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论 *13

                            syyq = "保存最后2次";
                            strDjFsfz = "约定冻结时段转换";

                            if (meter.MD_JJGC != "IR46")
                            {
                                syyq = "冻结量存储正确";
                                strDjFsfz = "两套日时段表切换冻结";
                            }

                            m_DETECT_RSLT.DATA_ITEM1 = strDjFsfz; //试验项目1 *13
                            m_DETECT_RSLT.DATA_ITEM2 = syyq; //试验项目2 *13   

                            #region 事件记录试验 指标项 空值-国金 
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = itemKey;

                            #endregion
                            m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            iIndex++;
                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 负荷纪录试验-国金 *13 *20
            if (meter.MeterLoadRecords.Count > 0)
            {
                iIndex = 0;

                string[] Arr_ID = new string[meter.MeterLoadRecords.Keys.Count];
                meter.MeterLoadRecords.Keys.CopyTo(Arr_ID, 0);

                for (int i = 0; i < Arr_ID.Length; i++)
                {
                    string _ID = Arr_ID[i];
                    //string strJl = "";
                    if (_ID.Length > 3)
                    {
                        MeterLoadRecord MeterLoadRecords = meter.MeterLoadRecords[_ID];
                        //if (_ID.IndexOf("001") == 0)
                        string strJl = MeterLoadRecords.Result;

                        //string[] strXmh = MeterLoadRecords.SubName.Split('次');
                        //string StrFlx; //负荷记录类型
                        //string StrFcs; //负荷记录次数
                        //switch (strXmh[1])
                        //{
                        //    case "第【01】类负荷记录":
                        //        StrFlx = "第【01】类(电压、电流)负荷记录";
                        //        break;
                        //    case "第【02】类负荷记录":
                        //        StrFlx = "第【02】类(有、无功功率)负荷记录";
                        //        break;
                        //    case "第【03】类负荷记录":
                        //        StrFlx = "第【03】类(功率因数)负荷记录";
                        //        break;
                        //    case "第【04】类负荷记录":
                        //        StrFlx = "第【04】类(有、无功电能)负荷记录";
                        //        break;
                        //    case "第【05】类负荷记录":
                        //        StrFlx = "第【05】类(四象限电能)负荷记录";
                        //        break;
                        //    case "第【06】类负荷记录":
                        //        StrFlx = "第【06】类(当前需量)负荷记录";
                        //        break;
                        //    default:
                        //        StrFlx = "第【01】类(电压、电流)负荷记录";
                        //        break;
                        //}
                        //switch (strXmh[0])
                        //{
                        //    case "第【01】":
                        //        StrFcs = "第1次";
                        //        break;
                        //    case "第【02】":
                        //        StrFcs = "第2次";
                        //        break;
                        //    case "第【03】":
                        //        StrFcs = "第3次";
                        //        break;
                        //    case "第【04】":
                        //        StrFcs = "第4次";
                        //        break;
                        //    case "第【05】":
                        //        StrFcs = "第5次";
                        //        break;
                        //    case "第【06】":
                        //        StrFcs = "第6次";
                        //        break;
                        //    case "第【07】":
                        //        StrFcs = "第7次";
                        //        break;
                        //    case "第【08】":
                        //        StrFcs = "第8次";
                        //        break;
                        //    case "第【09】":
                        //        StrFcs = "第9次";
                        //        break;
                        //    case "第【10】":
                        //        StrFcs = "第10次";
                        //        break;
                        //    default:
                        //        StrFcs = "第1次";
                        //        break;
                        //}

                        //string[] arrEnergy = MeterLoadRecords.Value.Split(',');

                        //itemId = "069";//质检编码
                        itemId = "069";
                        if (meter.MD_JJGC == "IR46")
                        {
                            itemId = "0175";
                        }
                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 负荷纪录试验 私有值-国金
                            for (int n = 0; n < 3; n++)
                            {
                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                m_DETECT_RSLT.ITEM_NAME = "负荷记录试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "负荷记录试验"; //试验分组

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex++).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "负荷记录试验"; //试验分项                           

                                m_DETECT_RSLT.IABC = zjxfs; //相别
                                m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                                m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                                m_DETECT_RSLT.ERR_ABS = "";//误差限值
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //分项结论

                                if (meter.MD_JJGC == "IR46")
                                {
                                    if (n == 0)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = "负荷纪录数据类"; //试验项目
                                        m_DETECT_RSLT.DATA_ITEM2 = "正确存储"; //试验分项  
                                    }
                                    else if (n == 1)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = "每类负荷记录的时间间隔"; //试验项目
                                        m_DETECT_RSLT.DATA_ITEM2 = "可设置"; //试验分项 
                                    }
                                    else if (n == 2)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = "负荷记录抄读"; //试验项目
                                        m_DETECT_RSLT.DATA_ITEM2 = "支持不同负荷记录抄读方式"; //试验分项 
                                    }
                                }
                                else
                                {
                                    if (n == 0)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = "六类数据项"; //试验项目
                                        m_DETECT_RSLT.DATA_ITEM2 = "任意组合"; //试验分项  
                                    }
                                    else if (n == 1)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = "每类负荷记录的时间间隔"; //试验项目
                                        m_DETECT_RSLT.DATA_ITEM2 = "可以相同，也可以不同"; //试验分项 
                                    }
                                    else if (n == 2)
                                    {
                                        m_DETECT_RSLT.DATA_ITEM1 = "负荷记录抄读"; //试验项目
                                        m_DETECT_RSLT.DATA_ITEM2 = "支持两种负荷记录抄读方式"; //试验分项 
                                    }
                                }

                                #region 事件记录试验 指标项 空值-国金 
                                m_DETECT_RSLT.DATA_ITEM3 = "";
                                m_DETECT_RSLT.DATA_ITEM4 = "";
                                m_DETECT_RSLT.DATA_ITEM5 = "";
                                m_DETECT_RSLT.DATA_ITEM6 = "";
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = _ID;
                                #endregion

                                m_DETECT_RSLT.CHK_CONC_CODE = strJl == ConstHelper.合格 ? "01" : strJl == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            #endregion

            #region 功能检查-国金 *13 *20
            if (meter.MeterFunctions.Count > 0)
            {
                iIndex = 0;
                //itemId = "0180";//质检编码
                string jjg_IR = "基本功能试验";
                itemId = "064";
                if (meter.MD_JJGC == "IR46")
                {
                    itemId = "0180";
                    jjg_IR = "功能检查";
                }
                foreach (KeyValuePair<string, MeterFunction> kv in meter.MeterFunctions)
                {
                    #region
                    if (kv.Key.Length == 5 && kv.Key != ProjectID.费率时段功能)
                    {
                        string require = "";
                        string name = "";
                        //string require = "";//试验要求require
                        switch (kv.Key)
                        {
                            case ProjectID.计量功能:
                                require = "可计量正、反向总及各费率电量。";
                                name = "计量功能";
                                break;
                            //case ProjectID.显示功能:
                            //    require = "可显示电量、时间、报警、通信等信息，可上电全显，背光可自动关闭。";
                            //    name = "显示功能";
                            //    break;
                            case ProjectID.脉冲输出功能:
                                require = "具有光脉冲、电脉冲、时钟脉冲输出功能。";
                                name = "脉冲输出";
                                break;
                            //case ProjectID.费率时段功能:
                            //    break;
                            case ProjectID.最大需量功能:
                                require = "可测最大需量，并记录其出现的日期和时间。";
                                name = "需量测试";
                                break;
                            case ProjectID.停电抄表功能:
                                require = "停电状态下，能够通过按键唤醒电能表，并抄读数据。";
                                name = "停电抄表";
                                break;
                            case ProjectID.计时功能:
                                require = "具有日历、计时功能。";
                                name = "计时功能";
                                break;
                        }

                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 功能检查 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = jjg_IR; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = jjg_IR; //试验分组

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号
                            m_DETECT_RSLT.TEST_CATEGORIES = jjg_IR; //试验分项 
                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *20

                            m_DETECT_RSLT.DATA_ITEM1 = name; //试验条件/试验项目 *20
                            m_DETECT_RSLT.DATA_ITEM2 = require; //试验要求 *20      

                            #region 事件记录试验 指标项 空值-国金 
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = kv.Key;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = kv.Value.Result == ConstHelper.合格 ? "01" : kv.Value.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                    #endregion
                }

                //显示功能
                foreach (KeyValuePair<string, MeterShow> kv in meter.MeterShows)
                {
                    #region 显示功能
                    if (kv.Key.Split('_')[0] == ProjectID.显示功能)
                    {
                        string require = "";
                        string name = "";
                        switch (kv.Key.Split('_')[0])
                        {
                            case ProjectID.显示功能:

                                require = "可显示电量、时间、报警、通信等信息，可上电全显，背光可自动关闭。";
                                name = "显示功能";
                                break;
                        }

                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 显示功能 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = jjg_IR; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = jjg_IR; //试验分组

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = jjg_IR; //试验分项

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *20

                            m_DETECT_RSLT.DATA_ITEM1 = name; //试验条件/试验项目 *20
                            m_DETECT_RSLT.DATA_ITEM2 = require; //试验要求 *20         

                            #region 事件记录试验 指标项 空值-国金 
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = kv.Key;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = kv.Value.Result == ConstHelper.合格 ? "01" : kv.Value.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                    #endregion
                }

                //费控功能 
                foreach (KeyValuePair<string, MeterFK> kv in meter.MeterCostControls)
                {
                    #region 费控功能 
                    if (kv.Key == ProjectID.报警功能)
                    {
                        string require = "";
                        string name = "";
                        switch (kv.Key)
                        {
                            case ProjectID.报警功能:
                                require = "有错误代码或报警提示，背光持续点亮。";
                                name = "报警功能";
                                break;
                                //case ProjectID.控制功能:    //1 在费控功能
                                //    require = "表计能以声、光或其他方式提醒及控制符合开关";
                                //    break;
                        }

                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 费控功能 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = jjg_IR; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = jjg_IR; //试验分组

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = jjg_IR; //试验分项

                            m_DETECT_RSLT.IABC = zjxfs; //相别
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向 

                            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *20

                            m_DETECT_RSLT.DATA_ITEM1 = name; //试验条件/试验项目 *20
                            m_DETECT_RSLT.DATA_ITEM2 = require; //试验要求 *20            

                            #region 事件记录试验 指标项 空值-国金 
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";
                            m_DETECT_RSLT.DATA_ITEM8 = "";
                            m_DETECT_RSLT.DATA_ITEM9 = "";
                            m_DETECT_RSLT.DATA_ITEM10 = "";
                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = kv.Key;
                            #endregion

                            m_DETECT_RSLT.CHK_CONC_CODE = kv.Value.Result == ConstHelper.合格 ? "01" : kv.Value.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region 停电抄表功能
                //if (TaskId.IndexOf("064") >= 0)
                //{
                //    #region 停电抄表功能 私有值-国金

                //    string FItemKey = ProjectID.停电抄表功能;

                //    if (meter.MeterFunctions.ContainsKey(FItemKey))
                //    {
                //        string aa = System.Guid.NewGuid().ToString();
                //        string bb = aa.Replace("-", "");
                //        dd = dd + 10;
                //        string cc = bb.Substring(6);
                //        string ff = dd.ToString() + cc;
                //        m_DETECT_RSLT.READ_ID = ff; //主键

                //        m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                //        m_DETECT_RSLT.ITEM_NAME = "基本功能试验"; //试验项名称
                //        m_DETECT_RSLT.TEST_GROUP = "功能检查"; //试验分组

                //        m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                //        m_DETECT_RSLT.TEST_CATEGORIES = "停电抄表功能"; //试验分项
                //        m_DETECT_RSLT.IABC = GetPCode("currentPhaseCode", zjxfs); //相别
                //        m_DETECT_RSLT.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
                //        m_DETECT_RSLT.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
                //        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"); //功率方向                                

                //        m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                //        m_DETECT_RSLT.ERR_ABS = "";//误差限值
                //        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //        m_DETECT_RSLT.DATA_ITEM1 = "停电抄表功能"; //试验条件/试验项目
                //        m_DETECT_RSLT.DATA_ITEM2 = "停电状态下，能够通过按键唤醒电能表，并抄读数据。"; //试验要求            

                //        #region 事件记录试验 指标项 空值-国金 
                //        m_DETECT_RSLT.DATA_ITEM3 = "";
                //        m_DETECT_RSLT.DATA_ITEM4 = "";
                //        m_DETECT_RSLT.DATA_ITEM5 = "";
                //        m_DETECT_RSLT.DATA_ITEM6 = "";
                //        m_DETECT_RSLT.DATA_ITEM7 = "";
                //        m_DETECT_RSLT.DATA_ITEM8 = "";
                //        m_DETECT_RSLT.DATA_ITEM9 = "";
                //        m_DETECT_RSLT.DATA_ITEM10 = "";
                //        m_DETECT_RSLT.DATA_ITEM11 = "";
                //        m_DETECT_RSLT.DATA_ITEM12 = "";
                //        m_DETECT_RSLT.DATA_ITEM13 = "";
                //        m_DETECT_RSLT.DATA_ITEM14 = "";
                //        m_DETECT_RSLT.DATA_ITEM15 = "";
                //        m_DETECT_RSLT.DATA_ITEM16 = "";
                //        m_DETECT_RSLT.DATA_ITEM17 = "";
                //        m_DETECT_RSLT.DATA_ITEM18 = "";
                //        m_DETECT_RSLT.DATA_ITEM19 = "";
                //        m_DETECT_RSLT.DATA_ITEM20 = "";
                //        #endregion

                //        m_DETECT_RSLT.DETECT_RESULT = "01";//分项结论
                //        m_DETECT_RSLT.CHK_CONC_CODE = "01";//检定总结论
                //        #endregion

                //        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //        if (sqls.Count > 0)
                //        {
                //            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //            foreach (string sql in sqls)
                //            {
                //                sqlList.Add(sql);
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            #endregion 

            #region 时钟功能试验-国金 *20
            //时钟功能试验-国金
            foreach (KeyValuePair<string, MeterDgn> kv in meter.MeterDgns)
            {
                #region 时钟功能
                string ItemKey = ProjectID.闰年判断功能;
                if (kv.Key.Length == 5)
                {
                    if (ItemKey == kv.Key)
                    {
                        itemId = "0178";
                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 时钟功能 私有值-国金

                            for (int i = 0; i < 3; i++)
                            {
                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                m_DETECT_RSLT.ITEM_NAME = "时钟功能试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "时钟功能试验"; //试验分组

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "时钟功能试验"; //试验分项

                                m_DETECT_RSLT.IABC = zjxfs; //相别
                                m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "";//负载电流
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向  

                                m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                                m_DETECT_RSLT.ERR_ABS = "";//误差限值
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //分项结论          

                                if (i == 0)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "闰年测试"; //试验项目 *13 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "闰年自动转换功能"; //试验分项 *13 *20
                                }
                                else if (i == 1)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "日历测试"; //试验项目 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "日历自动转换功能"; //试验分项 *20

                                }
                                else if (i == 2)
                                {
                                    m_DETECT_RSLT.DATA_ITEM1 = "广播校时测试"; //试验项目 *20
                                    m_DETECT_RSLT.DATA_ITEM2 = "支持明文和密文的广播校时"; //试验分项 *20
                                }

                                #region 事件记录试验 指标项 空值-国金 
                                m_DETECT_RSLT.DATA_ITEM3 = "";
                                m_DETECT_RSLT.DATA_ITEM4 = "";
                                m_DETECT_RSLT.DATA_ITEM5 = "";
                                m_DETECT_RSLT.DATA_ITEM6 = "";
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = kv.Key;
                                #endregion

                                m_DETECT_RSLT.CHK_CONC_CODE = kv.Value.Result == ConstHelper.合格 ? "01" : kv.Value.Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region 电流回路阻抗试验-国金 *13 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.电流回路阻抗;
                foreach (string item in meter.MeterDgns.Keys)
                {
                    if (item.Substring(0, 5) == ItemKey)
                    {
                        iIndex = 0;
                        string[] errW = meter.MeterDgns[item].Value.Split('|');

                        itemId = "028";
                        if (meter.MD_JJGC == "IR46")
                        {
                            itemId = "0162";
                        }
                        if (TaskId.IndexOf(itemId) != -1)
                        {
                            #region 电流回路阻抗试验 私有值-国金

                            string aa = System.Guid.NewGuid().ToString();
                            string bb = aa.Replace("-", "");
                            dd += 10;
                            string cc = bb.Substring(6);
                            string ff = dd.ToString() + cc;
                            m_DETECT_RSLT.READ_ID = ff; //主键

                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                            m_DETECT_RSLT.ITEM_NAME = "电流回路阻抗试验"; //试验项名称
                            m_DETECT_RSLT.TEST_GROUP = "电流回路阻抗试验"; //试验分组 *13 *20

                            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                            m_DETECT_RSLT.TEST_CATEGORIES = "电流回路阻抗试验"; //试验分项
                            m_DETECT_RSLT.IABC = zjxfs; //相别 *13
                            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                            m_DETECT_RSLT.LOAD_CURRENT = "Imax";//负载电流 *13 *20
                            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向                                

                            m_DETECT_RSLT.INT_CONVERT_ERR = errW[8];//误差化整值 *20
                            m_DETECT_RSLT.ERR_ABS = errW[0] + "mΩ";//误差限值 *20
                            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[item].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                            m_DETECT_RSLT.DATA_ITEM8 = Un; //电压 *20
                            m_DETECT_RSLT.DATA_ITEM10 = "10次实负载拉合闸间隔20s通10s"; //实验要求 *20

                            m_DETECT_RSLT.DATA_ITEM1 = errW[8]; //电流回路阻抗平均值 *13

                            #region 电流回路阻抗试验 指标项 空值-国金

                            m_DETECT_RSLT.DATA_ITEM2 = "";
                            m_DETECT_RSLT.DATA_ITEM3 = "";
                            m_DETECT_RSLT.DATA_ITEM4 = "";
                            m_DETECT_RSLT.DATA_ITEM5 = "";
                            m_DETECT_RSLT.DATA_ITEM6 = "";
                            m_DETECT_RSLT.DATA_ITEM7 = "";

                            m_DETECT_RSLT.DATA_ITEM9 = "";

                            m_DETECT_RSLT.DATA_ITEM11 = "";
                            m_DETECT_RSLT.DATA_ITEM12 = "";
                            m_DETECT_RSLT.DATA_ITEM13 = "";
                            m_DETECT_RSLT.DATA_ITEM14 = "";
                            m_DETECT_RSLT.DATA_ITEM15 = "";
                            m_DETECT_RSLT.DATA_ITEM16 = "";
                            m_DETECT_RSLT.DATA_ITEM17 = "";
                            m_DETECT_RSLT.DATA_ITEM18 = "";
                            m_DETECT_RSLT.DATA_ITEM19 = "";
                            m_DETECT_RSLT.DATA_ITEM20 = item;

                            #endregion
                            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[item].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[item].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                            #endregion

                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                            if (sqls.Count > 0)
                            {
                                //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + item + "' ");
                                foreach (string sql in sqls)
                                {
                                    sqlList.Add(sql);
                                }
                            }
                        }
                    }
                }

                #region 旧
                //string key = ItemKey + "_01";
                //if (meter.MeterDgns.ContainsKey(key))
                //{
                //    iIndex = 0;
                //    string[] errW = meter.MeterDgns[key].Value.Split('|');

                //    string itemID = "028";
                //    if (meter.MD_JJGC == "IR46")
                //    {
                //        itemId = "0154";
                //    }
                //    if (TaskId.IndexOf(itemID) != -1)
                //    {
                //        #region 电流回路阻抗试验 私有值-国金

                //        string aa = System.Guid.NewGuid().ToString();
                //        string bb = aa.Replace("-", "");
                //        dd = dd + 10;
                //        string cc = bb.Substring(6);
                //        string ff = dd.ToString() + cc;
                //        m_DETECT_RSLT.READ_ID = ff; //主键

                //        m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID
                //        m_DETECT_RSLT.ITEM_NAME = "电流回路阻抗试验"; //试验项名称
                //        m_DETECT_RSLT.TEST_GROUP = "电流回路阻抗试验"; //试验分组 *13 *20

                //        m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                //        m_DETECT_RSLT.TEST_CATEGORIES = "电流回路阻抗试验"; //试验分项
                //        m_DETECT_RSLT.IABC = zjxfs; //相别 *13
                //        m_DETECT_RSLT.PF = "1.0";//code 功率因数
                //        m_DETECT_RSLT.LOAD_CURRENT = "Imax";//负载电流 *13 *20
                //        m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向                                

                //        m_DETECT_RSLT.INT_CONVERT_ERR = errW[8];//误差化整值 *20
                //        m_DETECT_RSLT.ERR_ABS = errW[0] + "mΩ";//误差限值 *20
                //        m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //        m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //        m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *13 *20

                //        m_DETECT_RSLT.DATA_ITEM8 = Un; //电压 *20
                //        m_DETECT_RSLT.DATA_ITEM10 = "10次实负载拉合闸间隔20s通10s"; //实验要求 *20

                //        m_DETECT_RSLT.DATA_ITEM1 = errW[8]; //电流回路阻抗平均值 *13

                //        #region 电流回路阻抗试验 指标项 空值-国金

                //        m_DETECT_RSLT.DATA_ITEM2 = "";
                //        m_DETECT_RSLT.DATA_ITEM3 = "";
                //        m_DETECT_RSLT.DATA_ITEM4 = "";
                //        m_DETECT_RSLT.DATA_ITEM5 = "";
                //        m_DETECT_RSLT.DATA_ITEM6 = "";
                //        m_DETECT_RSLT.DATA_ITEM7 = "";

                //        m_DETECT_RSLT.DATA_ITEM9 = "";

                //        m_DETECT_RSLT.DATA_ITEM11 = "";
                //        m_DETECT_RSLT.DATA_ITEM12 = "";
                //        m_DETECT_RSLT.DATA_ITEM13 = "";
                //        m_DETECT_RSLT.DATA_ITEM14 = "";
                //        m_DETECT_RSLT.DATA_ITEM15 = "";
                //        m_DETECT_RSLT.DATA_ITEM16 = "";
                //        m_DETECT_RSLT.DATA_ITEM17 = "";
                //        m_DETECT_RSLT.DATA_ITEM18 = "";
                //        m_DETECT_RSLT.DATA_ITEM19 = "";
                //        m_DETECT_RSLT.DATA_ITEM20 = "";

                //        #endregion
                //        m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[key].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                //        #endregion

                //        sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //        if (sqls.Count > 0)
                //        {
                //            delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //            foreach (string sql in sqls)
                //            {
                //                sqlList.Add(sql);
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            #endregion

            #region 接地故障试验-国金 *13 *20
            if (meter.MeterDgns.Count > 0)
            {
                string ItemKey = ProjectID.接地故障;


                //string itemID = "029";
                string itemID = "0185";

                foreach (string item in meter.MeterDgns.Keys)
                {
                    if (item.Substring(0, 5) == ItemKey)
                    {
                        iIndex = 0;
                        int i = 0;

                        foreach (string key in meter.MeterDgns.Keys)
                        {
                            if (key.IndexOf("15031") != 0) continue;
                            string[] errW = meter.MeterDgns[key].Value.Split('|');

                            if (TaskId.IndexOf(itemID) != -1)
                            {
                                #region 接地故障试验 私有值-国金

                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID
                                m_DETECT_RSLT.ITEM_NAME = "接地故障试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "接地故障试验"; //试验分组 *20

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "接地故障试验"; //试验分项
                                m_DETECT_RSLT.IABC = zjxfs; //相别
                                m_DETECT_RSLT.PF = "1.0";//code 功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "10Itr";//负载电流
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向                                

                                m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                                m_DETECT_RSLT.ERR_ABS = "";//误差限值
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *20

                                m_DETECT_RSLT.DATA_ITEM1 = "某一相模拟接地故障时，所有电压提高到标称电压的1.1倍，持续4h"; //实验条件 *20
                                m_DETECT_RSLT.DATA_ITEM2 = "电源控制开关和负荷控制开关不应意外动作；工作正常，功能未受影响。"; //实验要求 *20
                                m_DETECT_RSLT.DATA_ITEM3 = "";
                                m_DETECT_RSLT.DATA_ITEM4 = "";
                                m_DETECT_RSLT.DATA_ITEM5 = "";
                                m_DETECT_RSLT.DATA_ITEM6 = "";

                                #region 接地故障试验 指标项 空值-国金 
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = "";

                                #endregion
                                m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[key].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[key].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion
                                i++;
                                iIndex++;
                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + item + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            string[] errW = meter.MeterDgns[item].Value.Split('|');
                            string[] Test = meter.MeterDgns[item].TestValue.Split('|');
                            string Iabc = "A";
                            string jjg = "A相对地";
                            int xb = 0;
                            if (j == 0)
                            {
                                Iabc = "A";
                                xb = 9;
                                jjg = "A相对地";
                            }
                            else if (j == 1)
                            {
                                Iabc = "B";
                                xb = 13;
                                jjg = "B相对地";
                            }
                            else if (j == 2)
                            {
                                Iabc = "C";
                                xb = 17;
                                jjg = "C相对地";
                            }

                            if (TaskId.IndexOf(itemID) != -1)
                            {
                                #region 接地故障试验 私有值-国金

                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID

                                if (meter.MD_JJGC == "IR46")
                                {
                                    m_DETECT_RSLT.ITEM_NAME = "接地故障误差"; //试验项名称
                                    m_DETECT_RSLT.TEST_GROUP = "接地故障误差"; //试验分组 *20
                                    m_DETECT_RSLT.TEST_CATEGORIES = "接地故障误差"; //试验分项
                                }
                                else
                                {
                                    m_DETECT_RSLT.ITEM_NAME = "抗接地故障抑制能力试验误差改变量"; //试验项名称
                                    m_DETECT_RSLT.TEST_GROUP = "抗接地故障抑制能力试验误差改变量"; //试验分组 *13
                                    m_DETECT_RSLT.TEST_CATEGORIES = "抗接地故障抑制能力试验误差改变量"; //试验分项
                                }

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                                m_DETECT_RSLT.IABC = Iabc; //相别 *20
                                m_DETECT_RSLT.PF = "1.0";//code 功率因数 *13 *20
                                //m_DETECT_RSLT.LOAD_CURRENT = Test[1];//负载电流 *20
                                m_DETECT_RSLT.LOAD_CURRENT = "10Itr";//负载电流 *20
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向                                

                                m_DETECT_RSLT.INT_CONVERT_ERR = errW[xb];//误差化整值 *20
                                m_DETECT_RSLT.ERR_ABS = "±" + errW[0];//误差限值 *13 *20
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[item].Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                                m_DETECT_RSLT.DATA_ITEM1 = jjg; //误差要求 *13
                                m_DETECT_RSLT.DATA_ITEM2 = "";
                                m_DETECT_RSLT.DATA_ITEM3 = "";
                                m_DETECT_RSLT.DATA_ITEM4 = "";
                                m_DETECT_RSLT.DATA_ITEM5 = "";
                                m_DETECT_RSLT.DATA_ITEM6 = errW[xb]; //实验相对误差该变量 *13

                                #region 接地故障试验 指标项 空值-国金 
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = "";

                                #endregion
                                m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[item].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[item].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion
                                i++;
                                iIndex++;
                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    //delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "'and DATA_ITEM20='" + item + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                        }
                    }
                }

                #region 旧
                //string keys = ItemKey + "_14061";
                //if (meter.MeterDgns.ContainsKey(keys))
                //{
                //    iIndex = 0;
                //    int i = 0;

                //    foreach (string key in meter.MeterDgns.Keys)
                //    {
                //        if (key.IndexOf("15031") != 0) continue;
                //        string[] errW = meter.MeterDgns[key].Value.Split('|');

                //        if (TaskId.IndexOf(itemID) != -1)
                //        {
                //            #region 接地故障试验 私有值-国金

                //            string aa = System.Guid.NewGuid().ToString();
                //            string bb = aa.Replace("-", "");
                //            dd = dd + 10;
                //            string cc = bb.Substring(6);
                //            string ff = dd.ToString() + cc;
                //            m_DETECT_RSLT.READ_ID = ff; //主键

                //            m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID
                //            m_DETECT_RSLT.ITEM_NAME = "接地故障试验"; //试验项名称
                //            m_DETECT_RSLT.TEST_GROUP = "接地故障试验"; //试验分组 *20

                //            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                //            m_DETECT_RSLT.TEST_CATEGORIES = "接地故障试验"; //试验分项
                //            m_DETECT_RSLT.IABC = zjxfs; //相别
                //            m_DETECT_RSLT.PF = "1.0";//code 功率因数
                //            m_DETECT_RSLT.LOAD_CURRENT = "Imax";//负载电流
                //            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向                                

                //            m_DETECT_RSLT.INT_CONVERT_ERR = "";//误差化整值
                //            m_DETECT_RSLT.ERR_ABS = "";//误差限值
                //            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[keys].Result == ConstHelper.合格 ? "01" : "02"; //分项结论 *20

                //            m_DETECT_RSLT.DATA_ITEM1 = "某一相模拟接地故障时，所有电压提高到标称电压的1.1倍，持续4h"; //实验条件 *20
                //            m_DETECT_RSLT.DATA_ITEM2 = "电源控制开关和负荷控制开关不应意外动作；工作正常，功能未受影响。"; //实验要求 *20
                //            m_DETECT_RSLT.DATA_ITEM3 = "";
                //            m_DETECT_RSLT.DATA_ITEM4 = "";
                //            m_DETECT_RSLT.DATA_ITEM5 = "";
                //            m_DETECT_RSLT.DATA_ITEM6 = "";

                //            #region 接地故障试验 指标项 空值-国金 
                //            m_DETECT_RSLT.DATA_ITEM7 = "";
                //            m_DETECT_RSLT.DATA_ITEM8 = "";
                //            m_DETECT_RSLT.DATA_ITEM9 = "";
                //            m_DETECT_RSLT.DATA_ITEM10 = "";
                //            m_DETECT_RSLT.DATA_ITEM11 = "";
                //            m_DETECT_RSLT.DATA_ITEM12 = "";
                //            m_DETECT_RSLT.DATA_ITEM13 = "";
                //            m_DETECT_RSLT.DATA_ITEM14 = "";
                //            m_DETECT_RSLT.DATA_ITEM15 = "";
                //            m_DETECT_RSLT.DATA_ITEM16 = "";
                //            m_DETECT_RSLT.DATA_ITEM17 = "";
                //            m_DETECT_RSLT.DATA_ITEM18 = "";
                //            m_DETECT_RSLT.DATA_ITEM19 = "";
                //            m_DETECT_RSLT.DATA_ITEM20 = "";

                //            #endregion
                //            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[keys].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[keys].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                //            #endregion
                //            i++;
                //            iIndex++;
                //            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //            if (sqls.Count > 0)
                //            {
                //                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //                foreach (string sql in sqls)
                //                {
                //                    sqlList.Add(sql);
                //                }
                //            }
                //        }
                //    }

                //    for (int j = 0; j < 3; j++)
                //    {
                //        string[] errW = meter.MeterDgns[keys].Value.Split('|');
                //        string[] Test = meter.MeterDgns[keys].TestValue.Split('|');
                //        string Iabc = "A";
                //        string jjg = "A相对地";
                //        int xb = 0;
                //        if (j == 0)
                //        {
                //            Iabc = "A";
                //            xb = 9;
                //            jjg = "A相对地";
                //        }
                //        else if (j == 1)
                //        {
                //            Iabc = "B";
                //            xb = 13;
                //            jjg = "B相对地";
                //        }
                //        else if (j == 2)
                //        {
                //            Iabc = "C";
                //            xb = 17;
                //            jjg = "C相对地";
                //        }

                //        if (TaskId.IndexOf(itemID) != -1)
                //        {
                //            #region 接地故障试验 私有值-国金

                //            string aa = System.Guid.NewGuid().ToString();
                //            string bb = aa.Replace("-", "");
                //            dd = dd + 10;
                //            string cc = bb.Substring(6);
                //            string ff = dd.ToString() + cc;
                //            m_DETECT_RSLT.READ_ID = ff; //主键

                //            m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID

                //            if (meter.MD_JJGC == "IR46")
                //            {
                //                m_DETECT_RSLT.ITEM_NAME = "接地故障误差"; //试验项名称
                //                m_DETECT_RSLT.TEST_GROUP = "接地故障误差"; //试验分组 *20
                //                m_DETECT_RSLT.TEST_CATEGORIES = "接地故障误差"; //试验分项
                //            }
                //            else
                //            {
                //                m_DETECT_RSLT.ITEM_NAME = "抗接地故障抑制能力试验误差改变量"; //试验项名称
                //                m_DETECT_RSLT.TEST_GROUP = "抗接地故障抑制能力试验误差改变量"; //试验分组 *13
                //                m_DETECT_RSLT.TEST_CATEGORIES = "抗接地故障抑制能力试验误差改变量"; //试验分项
                //            }

                //            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

                //            m_DETECT_RSLT.IABC = Iabc; //相别 *20
                //            m_DETECT_RSLT.PF = "1.0";//code 功率因数 *13 *20
                //            m_DETECT_RSLT.LOAD_CURRENT = Test[1];//负载电流 *20
                //            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向                                

                //            m_DETECT_RSLT.INT_CONVERT_ERR = errW[xb];//误差化整值 *20
                //            m_DETECT_RSLT.ERR_ABS = "±" + errW[0];//误差限值 *13 *20
                //            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                //            m_DETECT_RSLT.UNIT_MARK = ""; //单位

                //            m_DETECT_RSLT.DETECT_RESULT = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02"; //分项结论

                //            m_DETECT_RSLT.DATA_ITEM1 = jjg; //误差要求 *13
                //            m_DETECT_RSLT.DATA_ITEM2 = "";
                //            m_DETECT_RSLT.DATA_ITEM3 = "";
                //            m_DETECT_RSLT.DATA_ITEM4 = "";
                //            m_DETECT_RSLT.DATA_ITEM5 = "";
                //            m_DETECT_RSLT.DATA_ITEM6 = errW[xb]; //实验相对误差该变量 *13

                //            #region 接地故障试验 指标项 空值-国金 
                //            m_DETECT_RSLT.DATA_ITEM7 = "";
                //            m_DETECT_RSLT.DATA_ITEM8 = "";
                //            m_DETECT_RSLT.DATA_ITEM9 = "";
                //            m_DETECT_RSLT.DATA_ITEM10 = "";
                //            m_DETECT_RSLT.DATA_ITEM11 = "";
                //            m_DETECT_RSLT.DATA_ITEM12 = "";
                //            m_DETECT_RSLT.DATA_ITEM13 = "";
                //            m_DETECT_RSLT.DATA_ITEM14 = "";
                //            m_DETECT_RSLT.DATA_ITEM15 = "";
                //            m_DETECT_RSLT.DATA_ITEM16 = "";
                //            m_DETECT_RSLT.DATA_ITEM17 = "";
                //            m_DETECT_RSLT.DATA_ITEM18 = "";
                //            m_DETECT_RSLT.DATA_ITEM19 = "";
                //            m_DETECT_RSLT.DATA_ITEM20 = "";

                //            #endregion
                //            m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : meter.MeterDgns[ItemKey].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                //            #endregion
                //            i++;
                //            iIndex++;
                //            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                //            if (sqls.Count > 0)
                //            {
                //                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                //                foreach (string sql in sqls)
                //                {
                //                    sqlList.Add(sql);
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            #endregion

            #region 振动试验-国金
            //if (meter.MeterSpecialErrs.Count > 0)
            //{
            //    List<string> LstKey = new List<string>();
            //    foreach (string item in meter.MeterSpecialErrs.Keys)
            //    {
            //        if (item.IndexOf("129") != -1)
            //            LstKey.Add(item);
            //    }

            //    for (int i = 0; i < LstKey.Count; i++)
            //    {
            //        MeterSpecialErr data = meter.MeterSpecialErrs[LstKey[i]];
            //        string strName = data.Name;
            //        string StrA = data.AVR_VOT_A_MULTIPLE;
            //        string StrB = data.AVR_VOT_B_MULTIPLE;
            //        string StrC = data.AVR_VOT_C_MULTIPLE;
            //        string[] StrTmp = strName.Split(' ');
            //        string[] StrT = StrTmp[1].Split(',');
            //        string StrYj;
            //        string StrGlys;
            //        string StrFzdl;
            //        double Wctmp = 0;//在接口计算一下误差，然后看正负

            //        string prjName = meter.MeterSpecialErrs[LstKey[i]].Name;
            //        string[] tmp = prjName.Split(' ');
            //        string[] arr = tmp[1].Split(',');

            //        StrGlys = arr[1];
            //        StrFzdl = arr[2];

            //        string glfx = "正向有功";
            //        if (arr[0] == "P+")
            //            glfx = "正向有功";
            //        if (arr[0] == "P-")
            //            glfx = "反向有功";
            //        else if (arr[0] == "Q+")
            //            glfx = "正向无功";
            //        else if (arr[0] == "Q-")
            //            glfx = "反向无功";
            //        else if (arr[0] == "Q1")
            //            glfx = "第一象限无功";
            //        else if (arr[0] == "Q2")
            //            glfx = "第二象限无功";
            //        else if (arr[0] == "Q3")
            //            glfx = "第三象限无功";
            //        else if (arr[0] == "Q4")
            //            glfx = "第四象限无功";

            //        if (meter.MD_WiringMode == "三相三线")
            //        {
            //            if (StrA == "0" & StrB == "0" & StrC == "1")
            //            {
            //                StrYj = "C";
            //            }
            //            else if (StrA == "1" & StrB == "0" & StrC == "0")
            //            {
            //                StrYj = "A";
            //            }
            //            else
            //            {
            //                StrYj = "AC";
            //            }
            //        }
            //        else
            //        {
            //            if (StrA == "0" & StrB == "1" & StrC == "1")
            //            {
            //                StrYj = "A";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
            //            }
            //            else if (StrA == "1" & StrB == "0" & StrC == "1")
            //            {
            //                StrYj = "B";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
            //            }
            //            else if (StrA == "1" & StrB == "1" & StrC == "0")
            //            {
            //                StrYj = "C";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
            //            }
            //            else
            //            {
            //                StrYj = "ABC";
            //            }
            //        }

            //        string[] wcArr = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');       //误差值
            //        if (wcArr.Length <= 3) continue;

            //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');    //误差限

            //        string itemID = "0154";

            //        if (TaskId.IndexOf(itemID) != -1)
            //        {
            //            #region 振动试验 私有值-国金

            //            string aa = System.Guid.NewGuid().ToString();
            //            string bb = aa.Replace("-", "");
            //            dd = dd + 10;
            //            string cc = bb.Substring(6);
            //            string ff = dd.ToString() + cc;
            //            m_DETECT_RSLT.READ_ID = ff; //主键

            //            m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID
            //            m_DETECT_RSLT.ITEM_NAME = "振动试验"; //试验项名称
            //            m_DETECT_RSLT.TEST_GROUP = "振动试验"; //试验分组

            //            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

            //            m_DETECT_RSLT.TEST_CATEGORIES = "振动试验"; //试验分项
            //            m_DETECT_RSLT.IABC = GetPCode("currentPhaseCode", StrYj); //相别
            //            m_DETECT_RSLT.PF = GetPCode("meterTestPowerFactor", StrGlys); //功率因数
            //            m_DETECT_RSLT.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl); //负载电流
            //            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", glfx); //功率方向

            //            m_DETECT_RSLT.INT_CONVERT_ERR = double.Parse(data.ErrValue).ToString("F2");//误差化整值
            //            m_DETECT_RSLT.ERR_ABS = "±" + (meter.MeterSpecialErrs[LstKey[i]].BPHUpLimit).ToString();//误差限值
            //            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
            //            m_DETECT_RSLT.UNIT_MARK = ""; //单位

            //            m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论

            //            string[] strWc1 = data.Error1.Split('|');
            //            if (strWc1.Length <= 3) continue;
            //            string[] strWc2 = data.Error2.Split('|');
            //            if (strWc2.Length <= 3) continue;

            //            m_DETECT_RSLT.DATA_ITEM1 = strWc1[0]; //误差1（前），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM2 = strWc1[1]; //误差2（前），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM3 = strWc2[0]; //误差3（后），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM4 = strWc2[1]; //误差4（后），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5，根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM6 = strWc1[strWc1.Length - 2] + "|" + strWc2[strWc2.Length - 2]; //误差平均值 前后
            //            m_DETECT_RSLT.DATA_ITEM7 = data.ErrValue; //变差原始值，没有可不填
            //            m_DETECT_RSLT.DATA_ITEM8 = GetPCode("meterTestVolt", "100%Un"); //电压值
            //            m_DETECT_RSLT.DATA_ITEM9 = GetPCode("meterTestFreq", "50"); //频率值
            //            m_DETECT_RSLT.DATA_ITEM10 = m_DETECT_RSLT.TEST_CATEGORIES; //试验项目
            //            m_DETECT_RSLT.DATA_ITEM11 = m_DETECT_RSLT.TEST_CATEGORIES; //技术要求说明
            //            m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后

            //            #region 振动试验 指标项 空值-国金
            //            m_DETECT_RSLT.DATA_ITEM13 = "";
            //            m_DETECT_RSLT.DATA_ITEM14 = "";
            //            m_DETECT_RSLT.DATA_ITEM15 = "";
            //            m_DETECT_RSLT.DATA_ITEM16 = "";
            //            m_DETECT_RSLT.DATA_ITEM17 = "";
            //            m_DETECT_RSLT.DATA_ITEM18 = "";
            //            m_DETECT_RSLT.DATA_ITEM19 = "";
            //            m_DETECT_RSLT.DATA_ITEM20 = "";
            //            #endregion

            //            m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论                  
            //            #endregion

            //            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
            //            if (sqls.Count > 0)
            //            {
            //                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
            //                foreach (string sql in sqls)
            //                {
            //                    sqlList.Add(sql);
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion

            #region 冲击试验-国金
            //if (meter.MeterSpecialErrs.Count > 0)
            //{
            //    List<string> LstKey = new List<string>();

            //    foreach (string item in meter.MeterSpecialErrs.Keys)
            //    {
            //        if (item.IndexOf("130") != -1)
            //            LstKey.Add(item);
            //    }

            //    for (int i = 0; i < LstKey.Count; i++)
            //    {
            //        MeterSpecialErr data = meter.MeterSpecialErrs[LstKey[i]];

            //        M_QT_IMPACTTEST_MET_CONC entity = new M_QT_IMPACTTEST_MET_CONC();

            //        string[] wc1 = new string[50];
            //        string[] wc2 = new string[50];

            //        string[] strWc1 = new string[50];

            //        string strName = meter.MeterSpecialErrs[LstKey[i]].Name;
            //        string[] StrTmp = strName.Split(' ');
            //        string[] StrT = StrTmp[1].Split(',');
            //        string StrGlfx;
            //        string StrYj;
            //        string StrGlys;
            //        string StrFzdl;
            //        string StrA = data.AVR_VOT_A_MULTIPLE;
            //        string StrB = data.AVR_VOT_B_MULTIPLE;
            //        string StrC = data.AVR_VOT_C_MULTIPLE;
            //        string StrDYTmp = "";
            //        double Wctmp = 0;//在接口计算一下误差，然后看正负

            //        switch (StrT[0])
            //        {
            //            case "P+":
            //                StrGlfx = "正向有功";
            //                break;
            //            case "P-":
            //                StrGlfx = "反向有功";
            //                break;
            //            case "Q+":
            //                StrGlfx = "正向无功";
            //                break;
            //            case "Q-":
            //                StrGlfx = "反向无功";
            //                break;
            //            case "Q1":
            //                StrGlfx = "第一象限无功";
            //                break;
            //            case "Q2":
            //                StrGlfx = "第二象限无功";
            //                break;
            //            case "Q3":
            //                StrGlfx = "第三象限无功";
            //                break;
            //            case "Q4":
            //                StrGlfx = "第四象限无功";
            //                break;
            //            default:
            //                StrGlfx = "正向有功";
            //                break;
            //        }
            //        if (StrT[1] == "合元")
            //        {


            //            switch (StrT[1])
            //            {

            //                case "合元":
            //                    if (meter.MD_WiringMode == "三相三线")
            //                    {
            //                        StrYj = "AC";
            //                    }
            //                    else
            //                    {
            //                        StrYj = "ABC";
            //                    }
            //                    break;
            //                case "A元":
            //                    StrYj = "A";
            //                    break;
            //                case "B元":
            //                    StrYj = "B";
            //                    break;
            //                case "C元":
            //                    StrYj = "C";
            //                    break;
            //                default:
            //                    StrYj = "ABC";
            //                    break;
            //            }
            //            StrGlys = StrT[2];
            //            StrFzdl = StrT[3];
            //        }
            //        else
            //        {
            //            if (meter.MD_WiringMode == "三相三线")
            //            {
            //                StrYj = "AC";
            //            }
            //            else
            //            {
            //                StrYj = "ABC";
            //            }
            //            StrGlys = StrT[2];
            //            StrFzdl = StrT[3];

            //        }
            //        if (StrYj == "ABC" || StrYj == "AC")
            //        {
            //            entity.TEST_CATEGORIES = "02";
            //        }
            //        else
            //        {
            //            entity.TEST_CATEGORIES = "03";
            //        }

            //        if (StrT[1] == "合元")
            //        {
            //            if (meter.MD_WiringMode == "三相三线")
            //            {
            //                StrYj = "AC";
            //            }
            //            else
            //            {
            //                StrYj = "ABC";
            //            }
            //        }
            //        if (StrT[1] == "A元")
            //        {
            //            StrYj = "A";
            //        }
            //        if (StrT[1] == "B元")
            //        {
            //            StrYj = "B";
            //        }
            //        if (StrT[1] == "C元")
            //        {
            //            StrYj = "C";
            //        }

            //        if (meter.MD_WiringMode == "三相三线")
            //        {
            //            if (StrA == "0" & StrB == "0" & StrC == "1")
            //            {
            //                StrYj = "C";
            //            }
            //            else if (StrA == "1" & StrB == "0" & StrC == "0")
            //            {
            //                StrYj = "A";
            //            }
            //            else
            //            {
            //                StrYj = "AC";
            //            }
            //        }
            //        else
            //        {
            //            if (StrA == "0" & StrB == "1" & StrC == "1")
            //            {
            //                StrYj = "A";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
            //            }
            //            else if (StrA == "1" & StrB == "0" & StrC == "1")
            //            {
            //                StrYj = "B";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
            //            }
            //            else if (StrA == "1" & StrB == "1" & StrC == "0")
            //            {
            //                StrYj = "C";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
            //            }
            //            else
            //            {
            //                StrYj = "ABC";
            //            }
            //        }

            //        string[] strWc = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');
            //        if (strWc.Length <= 3) continue;
            //        entity.ERROR = strWc[0] + "|" + strWc[1] + "|" + strWc[2];
            //        entity.ERROR1 = strWc[0];
            //        entity.ERROR2 = strWc[1];
            //        entity.AVE_ERR = strWc[strWc.Length - 2];
            //        entity.INT_CONVERT_ERR = strWc[strWc.Length - 1];
            //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');

            //        string itemID = "0154";

            //        if (TaskId.IndexOf(itemID) != -1)
            //        {
            //            #region 冲击试验 私有值-国金

            //            string aa = System.Guid.NewGuid().ToString();
            //            string bb = aa.Replace("-", "");
            //            dd = dd + 10;
            //            string cc = bb.Substring(6);
            //            string ff = dd.ToString() + cc;
            //            m_DETECT_RSLT.READ_ID = ff; //主键

            //            m_DETECT_RSLT.ITEM_ID = itemID; //试验项ID
            //            m_DETECT_RSLT.ITEM_NAME = "振动试验"; //试验项名称
            //            m_DETECT_RSLT.TEST_GROUP = "振动试验"; //试验分组

            //            m_DETECT_RSLT.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定点的序号

            //            m_DETECT_RSLT.TEST_CATEGORIES = "振动试验"; //试验分项
            //            m_DETECT_RSLT.IABC = GetPCode("currentPhaseCode", StrYj); //相别
            //            m_DETECT_RSLT.PF = GetPCode("meterTestPowerFactor", StrGlys); //功率因数
            //            m_DETECT_RSLT.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl); //负载电流
            //            m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", StrGlfx); //功率方向

            //            m_DETECT_RSLT.INT_CONVERT_ERR = double.Parse(data.ErrValue).ToString("F2");//误差化整值
            //            m_DETECT_RSLT.ERR_ABS = "±" + (meter.MeterSpecialErrs[LstKey[i]].BPHUpLimit).ToString();//误差限值
            //            m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
            //            m_DETECT_RSLT.UNIT_MARK = ""; //单位

            //            m_DETECT_RSLT.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02"; //分项结论

            //            strWc = data.Error1.Split('|');
            //            if (strWc.Length <= 3) continue;
            //            strWc1 = data.Error2.Split('|');
            //            if (strWc1.Length <= 3) continue;

            //            m_DETECT_RSLT.DATA_ITEM1 = strWc[0]; //误差1（前），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM2 = strWc[1]; //误差2（前），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM3 = strWc1[0]; //误差3（后），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM4 = strWc1[1]; //误差4（后），根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM5 = ""; //误差5，根据实测个数传递
            //            m_DETECT_RSLT.DATA_ITEM6 = strWc[strWc.Length - 2] + "|" + strWc1[strWc1.Length - 2]; //误差平均值 前后
            //            m_DETECT_RSLT.DATA_ITEM7 = data.ErrValue; //变差原始值，没有可不填
            //            m_DETECT_RSLT.DATA_ITEM8 = GetPCode("meterTestVolt", "100%Un"); //电压值
            //            m_DETECT_RSLT.DATA_ITEM9 = GetPCode("meterTestFreq", "50"); //频率值
            //            m_DETECT_RSLT.DATA_ITEM10 = m_DETECT_RSLT.TEST_CATEGORIES; //试验项目
            //            m_DETECT_RSLT.DATA_ITEM11 = m_DETECT_RSLT.TEST_CATEGORIES; //技术要求说明
            //            m_DETECT_RSLT.DATA_ITEM12 = ""; //影响量前后

            //            #region 冲击试验 指标项 空值-国金
            //            m_DETECT_RSLT.DATA_ITEM13 = "";
            //            m_DETECT_RSLT.DATA_ITEM14 = "";
            //            m_DETECT_RSLT.DATA_ITEM15 = "";
            //            m_DETECT_RSLT.DATA_ITEM16 = "";
            //            m_DETECT_RSLT.DATA_ITEM17 = "";
            //            m_DETECT_RSLT.DATA_ITEM18 = "";
            //            m_DETECT_RSLT.DATA_ITEM19 = "";
            //            m_DETECT_RSLT.DATA_ITEM20 = "";
            //            #endregion

            //            m_DETECT_RSLT.CHK_CONC_CODE = data.Result.Trim() == ConstHelper.合格 ? "01" : data.Result.Trim() == ConstHelper.不合格 ? "02" : "03"; //检定总结论                  
            //            #endregion

            //            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
            //            if (sqls.Count > 0)
            //            {
            //                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
            //                foreach (string sql in sqls)
            //                {
            //                    sqlList.Add(sql);
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion

            #region 功耗试验-国金 *13 *20
            if (meter.MeterPowers.Count > 0)
            {
                //string ItemKey = ProjectID.功耗试验;
                foreach (string item in meter.MeterPowers.Keys)
                {
                    itemId = "032";
                    if (meter.MD_JJGC == "IR46")
                    {
                        itemId = "0160";
                    }
                    if (TaskId.IndexOf(itemId) != -1)
                    {
                        MeterPower meterpower = meter.MeterPowers[item];
                        string[] Value = meter.MeterPowers[item].Value.Split('|');
                        string[] testValue = meter.MeterPowers[item].TestValue.Split('|');

                        if (testValue[0] == "正常") //非通讯状态下
                        {
                            if (meter.MD_JJGC == "IR46")
                            {
                                itemId = "0161";
                            }
                            for (int i = 0; i < 3; i++)
                            {
                                #region 功耗试验 私有值-国金
                                m_DETECT_RSLT.ITEM_NAME = "功耗试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "非通讯状态"; //试验分组 *13 *20

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (i + 1).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "非通讯状态"; //试验分项
                                m_DETECT_RSLT.IABC = zjxfs; //相别 *13
                                m_DETECT_RSLT.PF = "1.0"; //功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "1.0Ib"; //负载电流 *13
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(meterpower.UaPowerP).ToString("f2");//误差化整值 *20

                                m_DETECT_RSLT.ERR_ABS = "±" + testValue[1]; //误差限值 *20
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = meter.MeterPowers[item].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                                m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13

                                #region 功耗试验 指标项 空值-国金
                                m_DETECT_RSLT.DATA_ITEM6 = "";
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = item;
                                #endregion

                                m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterPowers[item].Result == ConstHelper.合格 ? "01" : meter.MeterPowers[item].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论

                                //有功功率,视在功率,视在功率,不带通信模块，背光关闭、
                                switch (i.ToString())
                                {
                                    case "0": //电压有功功率

                                        m_DETECT_RSLT.DATA_ITEM2 = "电压线路"; //实验路线 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = testValue[1] + "W"; //试验要求允许值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM5 = "有功功率"; //实验要求 *20

                                        for (int j = 0; j < 3; j++)
                                        {
                                            string aa = System.Guid.NewGuid().ToString();
                                            string bb = aa.Replace("-", "");
                                            dd += 10;
                                            string cc = bb.Substring(6);
                                            string ff = dd.ToString() + cc;
                                            m_DETECT_RSLT.READ_ID = ff; //主键

                                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                            switch (j.ToString())
                                            {
                                                case "0": //A
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UaPowerP).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                                case "1": //B
                                                    if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UbPowerP).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                                case "2": //C
                                                    if (meter.MD_WiringMode == "单相") continue;
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UcPowerP).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                            }

                                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                            if (sqls.Count > 0)
                                            {
                                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                                foreach (string sql in sqls)
                                                {
                                                    sqlList.Add(sql);
                                                }
                                            }
                                        }
                                        break;
                                    case "1": //电压视在功率
                                        m_DETECT_RSLT.DATA_ITEM2 = "电压线路"; //实验路线 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = testValue[2] + "VA"; //试验要求允许值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM5 = "视在功率"; //实验要求 *20

                                        for (int j = 0; j < 3; j++)
                                        {
                                            string aa = System.Guid.NewGuid().ToString();
                                            string bb = aa.Replace("-", "");
                                            dd += 10;
                                            string cc = bb.Substring(6);
                                            string ff = dd.ToString() + cc;
                                            m_DETECT_RSLT.READ_ID = ff; //主键

                                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                            switch (j.ToString())
                                            {
                                                case "0": //A
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UaPowerS).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                                case "1": //B
                                                    if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UbPowerS).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                                case "2": //C
                                                    if (meter.MD_WiringMode == "单相") continue;
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UcPowerS).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                            }

                                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                            if (sqls.Count > 0)
                                            {
                                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                                foreach (string sql in sqls)
                                                {
                                                    sqlList.Add(sql);
                                                }
                                            }
                                        }
                                        break;
                                    case "2": //电流视在功率
                                        m_DETECT_RSLT.DATA_ITEM2 = "电流线路"; //实验路线 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM3 = testValue[3] + "VA"; //试验要求允许值 *13 *20
                                        m_DETECT_RSLT.DATA_ITEM5 = "视在功率"; //实验要求 *20

                                        for (int j = 0; j < 3; j++)
                                        {
                                            string aa = System.Guid.NewGuid().ToString();
                                            string bb = aa.Replace("-", "");
                                            dd += 10;
                                            string cc = bb.Substring(6);
                                            string ff = dd.ToString() + cc;
                                            m_DETECT_RSLT.READ_ID = ff; //主键

                                            m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID

                                            switch (j.ToString())
                                            {
                                                case "0": //A
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.IaPowerS).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                                case "1": //B
                                                    if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.IbPowerS).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                                case "2": //C
                                                    if (meter.MD_WiringMode == "单相") continue;
                                                    m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.IcPowerS).ToString("f2"); //试验结果 *13 *20
                                                    break;
                                            }

                                            sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                            if (sqls.Count > 0)
                                            {
                                                delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                                foreach (string sql in sqls)
                                                {
                                                    sqlList.Add(sql);
                                                }
                                            }
                                        }
                                        break;
                                }


                                #endregion
                            }
                        }
                        else //通讯状态下
                        {
                            if (meter.MD_JJGC == "IR46")
                            {
                                itemId = "0160";
                            }
                            for (int i = 0; i < 3; i++)
                            {
                                #region 功耗试验 私有值-国金
                                string aa = System.Guid.NewGuid().ToString();
                                string bb = aa.Replace("-", "");
                                dd += 10;
                                string cc = bb.Substring(6);
                                string ff = dd.ToString() + cc;
                                m_DETECT_RSLT.READ_ID = ff; //主键

                                m_DETECT_RSLT.ITEM_ID = itemId; //试验项ID
                                m_DETECT_RSLT.ITEM_NAME = "功耗试验"; //试验项名称
                                m_DETECT_RSLT.TEST_GROUP = "通讯状态"; //试验分组 *13 *20

                                m_DETECT_RSLT.DETECT_ITEM_POINT = (i + 1).ToString(); //检定点的序号

                                m_DETECT_RSLT.TEST_CATEGORIES = "通讯状态"; //试验分项
                                m_DETECT_RSLT.IABC = zjxfs; //相别 *13
                                m_DETECT_RSLT.PF = "1.0"; //功率因数
                                m_DETECT_RSLT.LOAD_CURRENT = "1.0Ib"; //负载电流 *13
                                m_DETECT_RSLT.BOTH_WAY_POWER_FLAG = "正向有功"; //功率方向

                                m_DETECT_RSLT.INT_CONVERT_ERR = float.Parse(meterpower.UaPowerP).ToString("f2");//误差化整值 *20

                                m_DETECT_RSLT.ERR_ABS = "±" + testValue[1]; //误差限值 *20
                                m_DETECT_RSLT.RESULT_TYPE = "01"; //试验结果类型
                                m_DETECT_RSLT.UNIT_MARK = ""; //单位

                                m_DETECT_RSLT.DETECT_RESULT = meter.MeterPowers[item].Result == ConstHelper.合格 ? "01" : "02";  //分项结论

                                m_DETECT_RSLT.DATA_ITEM1 = Un; //电压值 *13
                                m_DETECT_RSLT.DATA_ITEM2 = "电压线路"; //实验路线 *13 *20
                                m_DETECT_RSLT.DATA_ITEM3 = testValue[1] + "W"; //试验要求允许值 *13 *20

                                switch (i.ToString())
                                {
                                    case "0": //A
                                        m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UaPowerP).ToString("f2"); //试验结果 *13 *20
                                        break;
                                    case "1": //B
                                        if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
                                        m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UbPowerP).ToString("f2"); //试验结果 *13 *20
                                        break;
                                    case "2": //C
                                        if (meter.MD_WiringMode == "单相") continue;
                                        m_DETECT_RSLT.DATA_ITEM4 = float.Parse(meterpower.UcPowerP).ToString("f2"); //试验结果 *13 *20
                                        break;
                                }

                                m_DETECT_RSLT.DATA_ITEM5 = "有功功率"; //实验要求 *20

                                #region 功耗试验 指标项 空值-国金
                                m_DETECT_RSLT.DATA_ITEM6 = "";
                                m_DETECT_RSLT.DATA_ITEM7 = "";
                                m_DETECT_RSLT.DATA_ITEM8 = "";
                                m_DETECT_RSLT.DATA_ITEM9 = "";
                                m_DETECT_RSLT.DATA_ITEM10 = "";
                                m_DETECT_RSLT.DATA_ITEM11 = "";
                                m_DETECT_RSLT.DATA_ITEM12 = "";
                                m_DETECT_RSLT.DATA_ITEM13 = "";
                                m_DETECT_RSLT.DATA_ITEM14 = "";
                                m_DETECT_RSLT.DATA_ITEM15 = "";
                                m_DETECT_RSLT.DATA_ITEM16 = "";
                                m_DETECT_RSLT.DATA_ITEM17 = "";
                                m_DETECT_RSLT.DATA_ITEM18 = "";
                                m_DETECT_RSLT.DATA_ITEM19 = "";
                                m_DETECT_RSLT.DATA_ITEM20 = item;
                                #endregion

                                m_DETECT_RSLT.CHK_CONC_CODE = meter.MeterPowers[item].Result == ConstHelper.合格 ? "01" : meter.MeterPowers[item].Result == ConstHelper.不合格 ? "02" : "03"; //检定总结论
                                #endregion

                                sqls = GetM_QT_Zong_MET_CONCByMt(m_DETECT_RSLT);
                                if (sqls.Count > 0)
                                {
                                    delList.Add("delete from M_DETECT_RSLT where DETECT_TASK_NO = '" + taskNo + "'and bar_code='" + meter.MD_BarCode + "' ");
                                    foreach (string sql in sqls)
                                    {
                                        sqlList.Add(sql);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 添加接口数据备用-国金
            if (meter.MeterDgns.Count > 0)
            {

            }
            #endregion

            #region 添加接口数据备用-国金
            if (meter.MeterDgns.Count > 0)
            {

            }
            #endregion

            #endregion

            Execute(delList);

            Execute(sqlList);

            return true;
        }


        public void UpdateInit()
        {
            return;
        }

        #region
        /// <summary>
        /// 新国金接口数据
        /// </summary>
        /// <param name="m_DETECT_RSLT"></param>
        /// <returns></returns>
        private List<string> GetM_QT_Zong_MET_CONCByMt(M_DETECT_RSLT m_DETECT_RSLT)
        {
            List<string> sqlList = new List<string>();

            M_DETECT_RSLT entity = new M_DETECT_RSLT
            {
                READ_ID = m_DETECT_RSLT.READ_ID,
                EQUIP_ID = m_DETECT_RSLT.EQUIP_ID,
                BAR_CODE = m_DETECT_RSLT.BAR_CODE,//条形码
                EQUIP_NAME = m_DETECT_RSLT.EQUIP_NAME,
                DETECT_TASK_NO = m_DETECT_RSLT.DETECT_TASK_NO,
                EQUIP_CATEG = m_DETECT_RSLT.EQUIP_CATEG,
                EQUIP_TYPE = m_DETECT_RSLT.EQUIP_TYPE,
                ITEM_ID = m_DETECT_RSLT.ITEM_ID,
                ITEM_NAME = m_DETECT_RSLT.ITEM_NAME,
                TEST_GROUP = m_DETECT_RSLT.TEST_GROUP,
                SYS_NO = m_DETECT_RSLT.SYS_NO,
                DETECT_EQUIP_NO = m_DETECT_RSLT.DETECT_EQUIP_NO,
                DETECT_UNIT_NO = m_DETECT_RSLT.DETECT_UNIT_NO,
                POSITION_NO = m_DETECT_RSLT.POSITION_NO,
                VOLT_DATE = m_DETECT_RSLT.VOLT_DATE,//测试时间
                DETECT_ITEM_POINT = m_DETECT_RSLT.DETECT_ITEM_POINT, //字符串
                DATA_SOURCE = m_DETECT_RSLT.DATA_SOURCE,
                DATA_TYPE = m_DETECT_RSLT.DATA_TYPE,
                ENVIRON_TEMPER = m_DETECT_RSLT.ENVIRON_TEMPER,//字符串 
                RELATIVE_HUM = m_DETECT_RSLT.RELATIVE_HUM,//字符串 
                CON_MODE = m_DETECT_RSLT.CON_MODE,
                AP_LEVEL = m_DETECT_RSLT.AP_LEVEL,
                RP_LEVEL = m_DETECT_RSLT.RP_LEVEL,
                TEST_CATEGORIES = m_DETECT_RSLT.TEST_CATEGORIES,               //code
                IABC = m_DETECT_RSLT.IABC,//code 
                PF = m_DETECT_RSLT.PF,//code meterTestPowerFactor
                LOAD_CURRENT = m_DETECT_RSLT.LOAD_CURRENT,
                BOTH_WAY_POWER_FLAG = m_DETECT_RSLT.BOTH_WAY_POWER_FLAG,//code
                TEST_USER_ID = m_DETECT_RSLT.TEST_USER_ID,
                TEST_USER_NAME = m_DETECT_RSLT.TEST_USER_NAME,
                INT_CONVERT_ERR = m_DETECT_RSLT.INT_CONVERT_ERR,
                ERR_ABS = m_DETECT_RSLT.ERR_ABS,
                RESULT_TYPE = m_DETECT_RSLT.RESULT_TYPE,
                UNIT_MARK = m_DETECT_RSLT.UNIT_MARK,
                DETECT_RESULT = m_DETECT_RSLT.DETECT_RESULT,
                DATA_ITEM1 = m_DETECT_RSLT.DATA_ITEM1,
                DATA_ITEM2 = m_DETECT_RSLT.DATA_ITEM2,
                DATA_ITEM3 = m_DETECT_RSLT.DATA_ITEM3,
                DATA_ITEM4 = m_DETECT_RSLT.DATA_ITEM4,
                DATA_ITEM5 = m_DETECT_RSLT.DATA_ITEM5,
                DATA_ITEM6 = m_DETECT_RSLT.DATA_ITEM6,
                DATA_ITEM7 = m_DETECT_RSLT.DATA_ITEM7,
                DATA_ITEM8 = m_DETECT_RSLT.DATA_ITEM8,
                DATA_ITEM9 = m_DETECT_RSLT.DATA_ITEM9,
                DATA_ITEM10 = m_DETECT_RSLT.DATA_ITEM10,
                DATA_ITEM11 = m_DETECT_RSLT.DATA_ITEM11,
                DATA_ITEM12 = m_DETECT_RSLT.DATA_ITEM12,
                DATA_ITEM13 = m_DETECT_RSLT.DATA_ITEM13,
                DATA_ITEM14 = m_DETECT_RSLT.DATA_ITEM14,
                DATA_ITEM15 = m_DETECT_RSLT.DATA_ITEM15,
                DATA_ITEM16 = m_DETECT_RSLT.DATA_ITEM16,
                DATA_ITEM17 = m_DETECT_RSLT.DATA_ITEM17,
                DATA_ITEM18 = m_DETECT_RSLT.DATA_ITEM18,
                DATA_ITEM19 = m_DETECT_RSLT.DATA_ITEM19,
                DATA_ITEM20 = m_DETECT_RSLT.DATA_ITEM20,
                BASIC_ID = m_DETECT_RSLT.BASIC_ID,
                CHK_CONC_CODE = m_DETECT_RSLT.CHK_CONC_CODE,
                WRITE_DATE = m_DETECT_RSLT.WRITE_DATE,//写入时间
                HANDLE_FLAG = m_DETECT_RSLT.HANDLE_FLAG,
                HANDLE_DATE = m_DETECT_RSLT.HANDLE_DATE,
            };
            sqlList.Add(entity.ToInsertString());
            return sqlList;
        }

        ///// <summary>
        ///// 基本误差数据-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_BASICERR_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 1;
        //    List<string> sqlList = new List<string>();
        //    foreach (string key in meter.MeterErrors.Keys)
        //    {
        //        if (key.Length == 3) continue;       //如果ID长度是3表示是大项目，则跳过

        //        MeterError data = meter.MeterErrors[key];

        //        string[] wc = data.WcMore.Split('|');

        //        if (wc.Length <= 2) continue;
        //        string aveValue = wc[wc.Length - 2];
        //        string intValue = wc[wc.Length - 1];
        //        string errValue = "";
        //        for (int i = 0; i < wc.Length - 2; i++)
        //        {
        //            errValue += wc[i] + "|";
        //        }
        //        errValue = errValue.TrimEnd('|');

        //        string firstID = key.Substring(0, 1);
        //        if (firstID == "2") continue;

        //        string tmpHi = key.Substring(0, 3);            //取出误差类别+功率方向+元件
        //        string tmpLo = key.Substring(7);               //取出谐波+相序
        //        string tmpGlys = key.Substring(3, 2);          //取出功率因素
        //        string tmpxIb = key.Substring(5, 2);           //取出电流倍数

        //        switch (int.Parse(tmpHi.Substring(1, 1)))
        //        {
        //            case 1:
        //                data.GLFX = "正向有功";
        //                break;
        //            case 2:
        //                data.GLFX = "反向有功";
        //                break;
        //            case 3:
        //                data.GLFX = "正向无功";
        //                break;
        //            case 4:
        //                data.GLFX = "反向无功";
        //                break;
        //            default:
        //                data.GLFX = "正向有功";
        //                break;
        //        }   //正反向有无功,正向有功/正向无功/反向有功/反向无功

        //        string itemID = "004";  //正向有功,反向有功
        //        if (data.GLFX == "正向无功" || data.GLFX == "反向无功") itemID = "005";//质检编码
        //        if (TaskId.IndexOf(itemID) == -1) continue;

        //        string yj = "合元";
        //        if (int.Parse(tmpHi.Substring(2, 1)) == 2)
        //            yj = "A元";
        //        else if (int.Parse(tmpHi.Substring(2, 1)) == 3)
        //            yj = "B元";
        //        else if (int.Parse(tmpHi.Substring(2, 1)) == 4)
        //            yj = "C元";

        //        string abc = meter.MD_WiringMode == "三相三线" ? "AC" : "ABC";
        //        if (yj != "合元") abc = yj.Trim('元');
        //        data.YJ = tmpHi.Substring(2, 1);

        //        M_QT_BASICERR_MET_CONC entity = new M_QT_BASICERR_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //            EXPET_CATEG = "01",//code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,//条形码
        //            PARA_INDEX = iIndex.ToString(),//序号
        //            TEST_CATEGORIES = "01",//检定项meterActiveBasicErrorTest 这个地方要国金看
        //            DETECT_ITEM_POINT = iIndex.ToString(),//检定项序号
        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", data.GLFX),//功率方向
        //            ITEM_ID = itemID,

        //            IABC = GetPCode("currentPhaseCode", abc),//电流相别  
        //            LOAD_CURRENT = GetPCode("meterTestCurLoad", data.IbX), //负载电流
        //            LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //            FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //            PF = GetPCode("meterTestPowerFactor", data.GLYS.Trim()),//code 功率因数
        //            ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //            RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //            DETECT_RESULT = data.Result == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_CONC = data.Result == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_USER_NAME = meter.Checker1,//检验员
        //            AUDIT_USER_NAME = meter.Checker2,//检验员
        //            VOLT_DATE = meter.VerifyDate,//测试时间
        //            TEST_REQUIRE = "",//国金
        //            TEST_CONDITION = "",//国金
        //            WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //            HANDLE_FLAG = "0",////字符串
        //            HANDLE_DATE = "",//国金
        //            //ERROR = wc[0] + "|" + wc[1]//实际误差
        //            ERROR = errValue//实际误差
        //        };

        //        if (meter.MD_WiringMode == "单相")
        //        {
        //            entity.TEST_CATEGORIES = "01";
        //        }
        //        else
        //        {
        //            if (data.GLFX == "正向有功" || data.GLFX == "正向无功")
        //            {
        //                if (yj == "合元")
        //                    entity.TEST_CATEGORIES = "03";
        //                else
        //                    entity.TEST_CATEGORIES = "04";
        //            }
        //            else if (data.GLFX == "反向有功" || data.GLFX == "反向无功")
        //            {
        //                if (yj == "合元")
        //                    entity.TEST_CATEGORIES = "05";
        //                else
        //                    entity.TEST_CATEGORIES = "06";
        //            }
        //        }

        //        if (data.GLYS.Trim() == "0.25L")
        //            entity.ERR_ABS = "±" + (2.5 * 0.6);//误差限
        //        else
        //            entity.ERR_ABS = "±" + (double.Parse(data.BPHUpLimit.Trim().Split('|')[0]) * 0.6);//误差限
        //        entity.ERR_ABS = "±0.6";
        //        entity.ERR_ABS = "±0.9";

        //        entity.AVE_ERR = aveValue;

        //        if (intValue.Length > 8)
        //            entity.INT_CONVERT_ERR = intValue.Substring(0, 8);
        //        else
        //            entity.INT_CONVERT_ERR = intValue;

        //        sqlList.Add(entity.ToInsertString());
        //        iIndex++;
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 启动_国金源富
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_START_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;
        //    Dictionary<string, MeterQdQid> data = meter.MeterQdQids;

        //    List<string> sqlList = new List<string>();
        //    foreach (string key in data.Keys)
        //    {
        //        if (key.Length > 3 && (key.Substring(0, 3) == ProjectID.起动试验))           //只有大于3才可能是小项目,并且当中要包含启动ID和潜动ID
        //        {
        //            double FlIb = double.Parse((meter.MD_UA).Split('(')[0]);
        //            FlIb = double.Parse(data[key].Current) / FlIb;

        //            string powerFlag = "正向有功";
        //            if (meter.MeterQdQids[key].Name.IndexOf("正向有功") != -1)
        //                powerFlag = "正向有功";
        //            else if (meter.MeterQdQids[key].Name.IndexOf("反向有功") != -1)
        //                powerFlag = "反向有功";
        //            else if (meter.MeterQdQids[key].Name.IndexOf("正向无功") != -1)
        //                powerFlag = "正向无功";
        //            else if (meter.MeterQdQids[key].Name.IndexOf("反向无功") != -1)
        //                powerFlag = "反向无功";

        //            string itemId = "008";
        //            if (TaskId.IndexOf(itemId) == -1) continue;

        //            M_QT_START_MET_CONC entity = new M_QT_START_MET_CONC
        //            {
        //                BASIC_ID = meter.Meter_ID.ToString(),//19位ID
        //                DETECT_TASK_NO = meter.MD_TaskNo,
        //                EXPET_CATEG = "01",              //code
        //                DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号
        //                BAR_CODE = meter.MD_BarCode,        //字符串
        //                PARA_INDEX = iIndex.ToString(),  //字符串
        //                TEST_CATEGORIES = "01",          //code
        //                ITEM_ID = itemId,

        //                DETECT_ITEM_POINT = iIndex.ToString(), //字符串

        //                BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", powerFlag),//code
        //                IABC = GetPCode("currentPhaseCode", "ABC"),//code  
        //                LOAD_CURRENT = GetPCode("meterTestCurLoad", "无"),
        //                LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),
        //                FREQ = GetPCode("meterTestFreq", "50"), //code
        //                PF = GetPCode("meterTestPowerFactor", "1.0"),//code meterTestPowerFactor
        //                ENVIRON_TEMPER = meter.Temperature,//字符串 
        //                RELATIVE_HUM = meter.Humidity,//字符串 
        //                DETECT_RESULT = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",
        //                TEST_CONC = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",
        //                TEST_USER_NAME = meter.Checker1,
        //                AUDIT_USER_NAME = meter.Checker2,
        //                VOLT_DATE = meter.VerifyDate,//测试时间
        //                TEST_REQUIRE = "",//国金
        //                TEST_CONDITION = "",//国金
        //                WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //                HANDLE_FLAG = "0",////字符串
        //                HANDLE_DATE = "",//国金

        //                START_CURRENT = FlIb.ToString("f3") + "Ib",//起动电流
        //                START_TIME = data[key].ActiveTime.ToString()//起动时间Mqd_chrTime
        //            };

        //            sqlList.Add(entity.ToInsertString());
        //        }
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 潜动-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_CREEPING_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;
        //    Dictionary<string, MeterQdQid> data = meter.MeterQdQids;

        //    List<string> sqlList = new List<string>();
        //    foreach (string key in data.Keys)
        //    {
        //        if (key.Length > 3 && key.Substring(0, 3) == ProjectID.潜动试验)
        //        {
        //            string itemId = "009";
        //            if (TaskId.IndexOf(itemId) == -1) continue;

        //            string powerFlag = "正向有功";
        //            if (meter.MeterQdQids[key].Name.IndexOf("反向有功") != -1)
        //                powerFlag = "反向有功";
        //            else if (meter.MeterQdQids[key].Name.IndexOf("正向无功") != -1)
        //                powerFlag = "正向无功";
        //            else if (meter.MeterQdQids[key].Name.IndexOf("反向无功") != -1)
        //                powerFlag = "反向无功";


        //            M_QT_CREEPING_MET_CONC entity = new M_QT_CREEPING_MET_CONC
        //            {
        //                BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //                DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //                EXPET_CATEG = "01",//code 试品类别 电能表
        //                DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //                BAR_CODE = meter.MD_BarCode,//条形码
        //                PARA_INDEX = iIndex.ToString(),//序号
        //                TEST_CATEGORIES = "01",//检定项
        //                ITEM_ID = itemId,//质检编码

        //                DETECT_ITEM_POINT = iIndex.ToString(), //检定项序号
        //                BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", powerFlag),//功率方向
        //                IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //                LOAD_CURRENT = GetPCode("meterTestCurLoad", "无"),//负载电流
        //                LOAD_VOLTAGE = GetPCode("meterTestVolt", "115%Un"),//电压
        //                FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //                PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //                ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //                RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //                DETECT_RESULT = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",//试验结果
        //                TEST_CONC = meter.MeterQdQids[key].Result.Trim() == ConstHelper.合格 ? "01" : "02",
        //                TEST_USER_NAME = meter.Checker1,
        //                AUDIT_USER_NAME = meter.Checker2,

        //                VOLT_DATE = meter.VerifyDate,//测试时间
        //                TEST_REQUIRE = "",//国金
        //                TEST_CONDITION = "",//国金
        //                WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //                HANDLE_FLAG = "0",////字符串
        //                HANDLE_DATE = "",//国金
        //                CREEP_TIME = data[key].ActiveTime.ToString()//潜动时间
        //            };
        //            sqlList.Add(entity.ToInsertString());
        //        }
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 标准偏差（测量重复性检测）-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_MEASREPE_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;
        //    List<string> sqlList = new List<string>();
        //    foreach (string key in meter.MeterErrors.Keys)
        //    {
        //        if (key.Length <= 3) continue;       //如果ID长度是3表示是大项目，则跳过

        //        string firstID = key.Substring(0, 1);
        //        if (firstID != "2") continue;
        //        MeterError data = meter.MeterErrors[key];

        //        string[] wc = data.WcMore.Split('|');
        //        if (wc.Length <= 3) continue;

        //        string itemId = "006";
        //        if (TaskId.IndexOf(itemId) == -1) continue;

        //        string tmpHi = key.Substring(0, 3);            //取出误差类别+功率方向+元件
        //        string tmpLo = key.Substring(7);               //取出谐波+相序
        //        string tmpGlys = key.Substring(3, 2);          //取出功率因素
        //        string tmpxIb = key.Substring(5, 2);           //取出电流倍数

        //        switch (int.Parse(tmpHi.Substring(1, 1)))
        //        {
        //            case 1:
        //                data.GLFX = "正向有功";
        //                break;
        //            case 2:
        //                data.GLFX = "正向无功";
        //                break;
        //            case 3:
        //                data.GLFX = "反向有功";
        //                break;
        //            case 4:
        //                data.GLFX = "反向无功";
        //                break;
        //            default:
        //                data.GLFX = "正向有功";
        //                break;
        //        }       //正反向有无功,正向有功/正向无功/反向有功/反向无功

        //        data.YJ = tmpHi.Substring(2, 1);

        //        string yj = "合元";
        //        switch (int.Parse(tmpHi.Substring(2, 1)))
        //        {
        //            case 2:
        //                yj = "A元";
        //                break;
        //            case 3:
        //                yj = "B元";
        //                break;
        //            case 4:
        //                yj = "C元";
        //                break;
        //        }

        //        string abc = meter.MD_WiringMode == "三相三线" ? "AC" : "ABC";
        //        if (yj != "合元") abc = yj.Trim('元');

        //        M_QT_MEASREPE_MET_CONC entity = new M_QT_MEASREPE_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //            EXPET_CATEG = "01",//code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,//条形码
        //            PARA_INDEX = (iIndex + 1).ToString(),//序号
        //            TEST_CATEGORIES = "01",//检定项meterActiveBasicErrorTest 这个地方要国金看
        //            DETECT_ITEM_POINT = (iIndex + 1).ToString(),//检定项序号
        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", data.GLFX),//功率方向
        //            ITEM_ID = itemId,//质检编码

        //            IABC = GetPCode("currentPhaseCode", abc),//电流相别  
        //            LOAD_CURRENT = GetPCode("meterTestCurLoad", data.IbX), //负载电流
        //            LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //            FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //            PF = GetPCode("meterTestPowerFactor", data.GLYS.Trim()),//code 功率因数
        //            ENVIRON_TEMPER = meter.Temperature,//试验温度 字段里没有
        //            RELATIVE_HUM = meter.Humidity,//试验相对湿度  字段里没有
        //            DETECT_RESULT = data.Result == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_CONC = data.Result == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_USER_NAME = meter.Checker1,
        //            AUDIT_USER_NAME = meter.Checker2,
        //            VOLT_DATE = meter.VerifyDate,//测试时间
        //            TEST_REQUIRE = data.BPHUpLimit.Trim().Split('|')[0],//国金
        //            TEST_CONDITION = data.BPHUpLimit.Trim().Split('|')[0],//国金
        //            WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //            HANDLE_FLAG = "0",////字符串
        //            HANDLE_DATE = "",//国金
        //            ERROR = wc[0] + "|" + wc[1] + "|" + wc[2] + "|" + wc[3] + "|" + wc[4],//实际误差 字段里没有
        //            VALUE_ABS = data.BPHUpLimit.Trim().Split('|')[0]//误差限
        //        };//MT_MEASURE_REPEAT_MET_CONC

        //        if (wc[5].Length > 8)
        //            entity.AVE_ERR = wc[5].Substring(0, 8);
        //        else
        //            entity.AVE_ERR = wc[5];

        //        if (wc[6].Length > 8)
        //            entity.INT_CONVERT_ERR = wc[6].Substring(0, 8);
        //        else
        //            entity.INT_CONVERT_ERR = wc[6];

        //        string[] arrValue = data.WcMore.Split('|');
        //        if (arrValue.Length >= 7)
        //        {
        //            entity.DEVIATION_LIMT = arrValue[5];
        //            entity.DEVIATION_ERR = arrValue[5];
        //        }

        //        entity.PERMISSIBLE_VALUE = "";//允许值 要看下是啥
        //        entity.INT_DEVIATION_ERR = arrValue[6];//偏差化整值 要看下是啥
        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 计度器总电能示值组合误差-国金
        ///// </summary>
        ///// <returns></returns>
        //private List<string> GetM_QT_WIDELYPROT_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();
        //    string ItemKey = Cus_DgnItem.电子指示显示器电能示值组合误差;
        //    if (!meter.MeterDgns.ContainsKey(ItemKey)) return sqlList;

        //    string itemId = "012";
        //    if (TaskId.IndexOf(itemId) == -1) return sqlList;

        //    M_QT_WIDELYPROT_MET_CONC entity = new M_QT_WIDELYPROT_MET_CONC
        //    {
        //        BASIC_ID = meter.Meter_ID.ToString(),     //19位ID  基本信息标识
        //        DETECT_TASK_NO = meter.MD_TaskNo,           //检定任务单编号
        //        EXPET_CATEG = "01",                          //code 试品类别 电能表
        //        DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号   
        //        BAR_CODE = meter.MD_BarCode,                //条形码
        //        PARA_INDEX = "1",                            //序号
        //        TEST_CATEGORIES = "01",                      //检定项
        //        ITEM_ID = itemId,                             //质检编码

        //        DETECT_ITEM_POINT = "1", //检定项序号
        //        BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向
        //        IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //        LOAD_CURRENT = GetPCode("meterTestCurLoad", "1.0Ib"),//负载电流
        //        LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //        FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //        PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //        ENVIRON_TEMPER = meter.Temperature, //试验温度 
        //        RELATIVE_HUM = meter.Humidity,      //试验相对湿度 
        //        DETECT_RESULT = meter.MeterDgns[Cus_DgnItem.电子指示显示器电能示值组合误差].Result == ConstHelper.合格 ? "01" : "02",
        //        VOLT_DATE = meter.VerifyDate,       //测试时间
        //        TEST_REQUIRE = "",                      //国金
        //        TEST_CONDITION = "",                    //国金
        //        WRITE_DATE = DateTime.Now.ToString(),   //写入时间
        //        HANDLE_FLAG = "0",                      //字符串
        //        HANDLE_DATE = ""                        //国金 
        //    };

        //    float fIncrementSharp = 0.0f;   //尖示值增量
        //    float fIncrementPeak = 0.0f;    //峰示值增量
        //    float fIncrementFlat = 0.0f;    //平示值增量
        //    float fIncrementValley = 0.0f;  //谷示值增量
        //    float fIncrementTotal = 0.0f;   //总示值增量

        //    string keyi = ItemKey + "07";

        //    int jCount = 0;
        //    int fCount = 0;
        //    int pCount = 0;
        //    int gCount = 0;
        //    string StrFee = "";//费率
        //    string StrQm = "";//起码
        //    string StrZm = "";//止码
        //    string StrMc = "";//码差

        //    for (int i = 0; i < 20; i++)
        //    {
        //        keyi = ItemKey + i.ToString("D2");

        //        if (meter.MeterDgns.ContainsKey(keyi))
        //        {
        //            string value = meter.MeterDgns[keyi].Value;
        //            if (!string.IsNullOrEmpty(value))
        //            {
        //                if (value.IndexOf('|') == -1)
        //                {
        //                    entity.VALUE_ERR = (double.Parse(value)).ToString("F2");
        //                }
        //                else
        //                {
        //                    string[] arr = value.Split('|');
        //                    string StrMcTmp = Math.Round(double.Parse(arr[2]), 2).ToString();//码差化整为0.00

        //                    if (arr.Length >= 4)
        //                    {
        //                        if (arr[3] == "尖")
        //                        {
        //                            jCount++;
        //                            fIncrementSharp += float.Parse(StrMcTmp);
        //                        }
        //                        else if (arr[3] == "峰")
        //                        {
        //                            fCount++;
        //                            fIncrementPeak += float.Parse(StrMcTmp);
        //                        }
        //                        else if (arr[3] == "平")
        //                        {
        //                            pCount++;
        //                            fIncrementFlat += float.Parse(StrMcTmp);
        //                        }
        //                        else if (arr[3] == "谷")
        //                        {
        //                            gCount++;
        //                            fIncrementValley += float.Parse(StrMcTmp);
        //                        }
        //                        else if (arr[3] == "总")
        //                            fIncrementTotal += float.Parse(StrMcTmp);

        //                        StrFee += "|" + arr[3] + "电量(kWh)";
        //                        StrQm += "|" + arr[0] + "kWh";
        //                        StrZm += "|" + arr[1] + "kWh";
        //                        StrMc += "|" + StrMcTmp + "kWh";

        //                    }
        //                }
        //            }
        //        }
        //    }

        //    float fIncrementSumerAll = fIncrementSharp + fIncrementPeak + fIncrementFlat + fIncrementValley;//各费率示值增量和
        //    float fReadingErrTotal = fIncrementTotal - fIncrementSumerAll;  //总分电量值差（千瓦时）

        //    //StrFee += "|各分时电量之和(kWh)|总电量|允许误差|实际误差";
        //    //StrMc += "|" + fIncrementSumerAll + "|" + fIncrementTotal + "|0.03|" + fReadingErrTotal;
        //    StrFee += "|各分时电量之和(kWh)|允许误差|实际误差";
        //    StrMc += "|" + fIncrementSumerAll + "kWh|0.03|" + fReadingErrTotal.ToString("f2");

        //    StrFee = StrFee.Trim('|');
        //    StrQm = StrQm.Trim('|');
        //    StrZm = StrZm.Trim('|');
        //    StrMc = StrMc.Trim('|');

        //    int zCount = jCount + fCount + pCount + gCount; //
        //    entity.SWIT_TIMES = zCount.ToString();
        //    entity.FEE1_TIMES = jCount.ToString();
        //    entity.FEE2_TIMES = fCount.ToString();
        //    entity.FEE3_TIMES = pCount.ToString();
        //    entity.FEE4_TIMES = gCount.ToString();
        //    entity.TEST_CONC = meter.MeterDgns["005"].Value.Trim() == ConstHelper.合格 ? "01" : "02";
        //    entity.TEST_USER_NAME = meter.Checker1;
        //    entity.AUDIT_USER_NAME = meter.Checker2;
        //    entity.TIME_ELECT_SUM = fIncrementSumerAll.ToString("F2");
        //    entity.FEE1_VALUE = fIncrementSharp.ToString("F2");
        //    entity.FEE2_VALUE = fIncrementPeak.ToString("F2");
        //    entity.FEE3_VALUE = fIncrementFlat.ToString("F2");
        //    entity.FEE4_VALUE = fIncrementValley.ToString("F2");
        //    entity.ELECT_SUM = fIncrementTotal.ToString("F2");
        //    entity.VALUE_ERR_ABS = "±" + (0.01 * zCount).ToString();

        //    entity.FEE = StrFee;//总费率
        //    entity.VALUE_BEG = StrQm;//起码
        //    entity.VALUE_END = StrZm;//止码
        //    entity.ELECT_VALUE = StrMc;//码差
        //    sqlList.Add(entity.ToInsertString());




        //    return sqlList;
        //}

        ///// <summary>
        ///// 校核常数-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_CONSTANT_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    DataTable dtKeys = new DataTable();
        //    dtKeys.Columns.Add("Keys", typeof(string));
        //    dtKeys.Columns.Add("PrjId", typeof(string));
        //    foreach (string key in meter.MeterZZErrors.Keys)
        //    {
        //        MeterZZError MeterError = meter.MeterZZErrors[key];
        //        dtKeys.Rows.Add(key, MeterError.PrjID);
        //    }
        //    DataRow[] Rows = dtKeys.Select("Keys <>'' and PrjId <> ''", "PrjId asc");
        //    List<string> sqlList = new List<string>();
        //    for (int i = 0; i < Rows.Length; i++)
        //    {
        //        MeterZZError data = meter.MeterZZErrors[Rows[i][0].ToString()];
        //        string prjId = data.PrjID;
        //        switch (int.Parse(prjId[0].ToString()))
        //        {
        //            case 1:
        //                data.PowerWay = "正向有功";
        //                break;
        //            case 2:
        //                data.PowerWay = "正向无功";
        //                break;
        //            case 3:
        //                data.PowerWay = "反向有功";
        //                break;
        //            case 4:
        //                data.PowerWay = "反向无功";
        //                break;
        //            default:
        //                data.PowerWay = "正向有功";
        //                break;
        //        }

        //        string itemID = "007";
        //        if (TaskId.IndexOf(itemID) == -1) continue;

        //        string[] dj = Number.GetDj(meter.MD_Grane);

        //        M_QT_CONSTANT_MET_CONC entity = new M_QT_CONSTANT_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //            EXPET_CATEG = "01",//code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,//条形码
        //            PARA_INDEX = (i + 1).ToString(), //序号
        //            TEST_CATEGORIES = "01",//检定项
        //            ITEM_ID = itemID,//质检编码

        //            DETECT_ITEM_POINT = (i + 1).ToString(),//检定项序号
        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", data.PowerWay),//功率方向
        //            IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //            LOAD_CURRENT = GetPCode("meterTestCurLoad", data.IbXString),//负载电流
        //            LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //            FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //            PF = GetPCode("meterTestPowerFactor", data.GLYS),//code 功率因数
        //            ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //            RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //            DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_CONC = data.Result.Trim() == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_USER_NAME = meter.Checker1,
        //            AUDIT_USER_NAME = meter.Checker2,
        //            VOLT_DATE = meter.VerifyDate,//测试时间
        //            //TEST_REQUIRE = meter.MD_Constant + " imp/kWh",//国金
        //            TEST_REQUIRE = "测试输出与显示器指示之间的关系，应与铭牌标志一致",
        //            TEST_CONDITION = "",//国金
        //            WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //            HANDLE_FLAG = "0",////字符串
        //            HANDLE_DATE = "",//国金
        //            IR_START_READING = data.PowerStart.ToString(),
        //            IR_END_READING = data.PowerEnd.ToString(),
        //            IR_READING = data.WarkPower.ToString().Trim(),
        //            METER_CONST_CODE = data.STMEnergy.Trim(),// meterInfo.Mb_chrBcs;
        //            METER_DIGITS = "6.2",
        //            IR_PULES = data.Pules.Trim(),
        //            AR_TS_READING_ERR = data.PowerError,
        //            ERR_UP = ((Convert.ToDouble(dj[0])) * 1.0).ToString("0.0"),
        //            ERR_DOWN = ((Convert.ToDouble(dj[0])) * (-1.0)).ToString("0.0"),
        //            FEE = GetPCode("fee", data.Fl), //费率fee
        //            CONTROL_METHOD = "标准表法"
        //        };

        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    return sqlList;
        //}

        ///// <summary>
        ///// 日计时 -国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_DAYTIME_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    string strRjsValue = "", strAverage = "", strHz = "";
        //    string strResult = "";
        //    string ItemKey = Cus_DgnItem.由电源供电的时钟试验;
        //    List<string> sqlList = new List<string>();

        //    if (!meter.MeterDgns.ContainsKey("002")) return sqlList;

        //    //平均值
        //    string key = ItemKey + "01";
        //    if (meter.MeterDgns.ContainsKey(key))
        //    {
        //        string[] arr = meter.MeterDgns[key].Value.Split('|');
        //        if (arr.Length > 1)
        //        {
        //            if (Convert.ToSingle(arr[0]) > 0)
        //                strAverage = "+" + arr[0].Trim('+');
        //            else
        //                strAverage = arr[0];
        //            strHz = arr[1];
        //        }
        //    }

        //    //前五次
        //    key = ItemKey + "02";
        //    if (meter.MeterDgns.ContainsKey(key))
        //    {
        //        strRjsValue = meter.MeterDgns[key].Value + "|";
        //    }

        //    ItemKey = Cus_DgnItem.由电源供电的时钟试验;
        //    if (meter.MeterDgns.ContainsKey(ItemKey))
        //    {
        //        strResult = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";
        //    }

        //    string itemId = "011";
        //    if (TaskId.IndexOf(itemId) == -1) return sqlList;

        //    M_QT_DAYTIME_MET_CONC entity = new M_QT_DAYTIME_MET_CONC
        //    {
        //        BASIC_ID = meter.Meter_ID.ToString(),     //19位ID  基本信息标识
        //        DETECT_TASK_NO = meter.MD_TaskNo,           //检定任务单编号
        //        EXPET_CATEG = "01",                      //code 试品类别 电能表
        //        DETECT_EQUIP_NO = meter.BenthNo.Trim(),  //检定设备编号 
        //        BAR_CODE = meter.MD_BarCode,                //条形码
        //        PARA_INDEX = "1",                        //序号
        //        TEST_CATEGORIES = "01",                  //检定项  p_code=1
        //        ITEM_ID = itemId,                        //质检编码

        //        METER_CONST_CODE = meter.MD_Constant,       //常数
        //        DETECT_ITEM_POINT = "1",                 //检定项序号
        //        BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向
        //        IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //        LOAD_CURRENT = GetPCode("meterTestCurLoad", "无"),//负载电流
        //        LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //        FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //        PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //        ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //        RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //        TEST_CONC = strResult,//试验结果
        //        VOLT_DATE = meter.VerifyDate,//测试时间
        //        TEST_REQUIRE = "",//国金
        //        TEST_CONDITION = "",//国金
        //        WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //        HANDLE_FLAG = "0",////字符串
        //        HANDLE_DATE = "",//国金
        //        TIME_FREQUENCY = "1",
        //        SIMPLING = "5",
        //        ACTUER_VALUE = strRjsValue,
        //        AVE_VALUE = strAverage
        //    };

        //    if (!string.IsNullOrEmpty(strAverage))
        //    {
        //        if (Convert.ToSingle(strAverage.Trim('+')) >= 0)
        //        {
        //            if (strAverage.IndexOf("+") != -1)
        //                entity.INT_CONVERT_ERR = strHz;
        //            else
        //                entity.INT_CONVERT_ERR = "+" + strHz;
        //        }
        //        else
        //        {
        //            if (strAverage.IndexOf("-") != -1)
        //                entity.INT_CONVERT_ERR = strHz;
        //            else
        //                entity.INT_CONVERT_ERR = "-" + strHz;
        //        }

        //    }

        //    entity.VALUE_ABS = "±0.5 s/d";
        //    entity.CLEAR_DATA_RST = "";
        //    entity.TEST_USER_NAME = meter.Checker1;
        //    entity.AUDIT_USER_NAME = meter.Checker2;

        //    sqlList.Add(entity.ToInsertString());

        //    return sqlList;


        //}

        ///// <summary>
        ///// 环境温度对由电源供电的时钟试验的影响
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_ETDAYTIME_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    string strAverage = "", strHz = "";
        //    string strResult = "";
        //    string wd = "23";

        //    List<string> sqlList = new List<string>();
        //    int iIndex = 0;
        //    string ItemKey = Cus_DgnItem.环境温度对由电源供电的时钟试验的影响_23度;

        //    for (int i = 0; i < 3; i++)
        //    {
        //        if (i == 1)
        //        {
        //            ItemKey = Cus_DgnItem.环境温度对由电源供电的时钟试验的影响_60度;
        //            wd = "60";
        //        }
        //        else if (i == 2)
        //        {
        //            ItemKey = Cus_DgnItem.环境温度对由电源供电的时钟试验的影响_负25度;
        //            wd = "-25";
        //        }

        //        //平均值
        //        string key = ItemKey + "01";
        //        if (!meter.MeterDgns.ContainsKey(key)) continue;
        //        string[] arr = meter.MeterDgns[key].Value.Split('|');
        //        if (arr.Length > 1)
        //        {
        //            strAverage = arr[0];
        //            strHz = arr[1];
        //        }

        //        //前五次
        //        key = ItemKey + "02";
        //        if (!meter.MeterDgns.ContainsKey(key)) continue;
        //        string rjsValue = meter.MeterDgns[key].Value;

        //        if (meter.MeterDgns.ContainsKey(ItemKey))
        //            strResult = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";

        //        string itemId = "017";
        //        if (TaskId.IndexOf(itemId) == -1) continue;

        //        M_QT_ETDAYTIME_MET_CONC entity = new M_QT_ETDAYTIME_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(),    //19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,          //检定任务单编号
        //            EXPET_CATEG = "01",                     //code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(), //检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,               //条形码
        //            PARA_INDEX = (iIndex + 1).ToString(),   //序号
        //            TEST_CATEGORIES = "01",                 //检定项  p_code=1
        //            ITEM_ID = itemId,                       //质检编码

        //            METER_CONST_CODE = meter.MD_Constant,//常数
        //            DETECT_ITEM_POINT = (iIndex + 1).ToString(),//检定项序号
        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向
        //            IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //            LOAD_CURRENT = GetPCode("meterTestCurLoad", "无"),//负载电流
        //            LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //            FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //            PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //            ENVIRON_TEMPER = wd,//试验温度 
        //            RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //            TEST_CONC = strResult,//试验结果
        //            TEST_USER_NAME = meter.Checker1,
        //            AUDIT_USER_NAME = meter.Checker2,
        //            VOLT_DATE = meter.VerifyDate,//测试时间
        //            TEST_REQUIRE = "",//国金
        //            TEST_CONDITION = "",//国金
        //            WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //            HANDLE_FLAG = "0",////字符串
        //            HANDLE_DATE = "",//国金
        //            ACTUER_VALUE = rjsValue,//
        //            AVG_VALUE = strAverage,//
        //            INT_CONVERT_ERR = strHz,//
        //            VALUE_ERR_ABS = "±1.0s/d"
        //        };
        //        if (wd == "23")
        //        {
        //            entity.TEMPERA_VALUE = "0.00";
        //            entity.AVG_TEMPERA_VALUE = "0.00";
        //            entity.INT_TEMPERA_VALUE = "0.00";
        //        }
        //        else
        //        {
        //            entity.TEMPERA_VALUE = (double.Parse(strAverage) / ((int.Parse(wd)) - 23)).ToString("0.00000");
        //            entity.AVG_TEMPERA_VALUE = (double.Parse(strAverage) / ((int.Parse(wd)) - 23)).ToString("0.00000");
        //            entity.INT_TEMPERA_VALUE = (double.Parse(strAverage) / ((int.Parse(wd)) - 23)).ToString("0.00");
        //        }
        //        entity.TEMPERA_VALUE_ABS = "±0.1s/d℃";
        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    return sqlList;


        //}

        ///// <summary>
        ///// 电能表电流回路阻抗试验 -国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_IMPCURRENT_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();
        //    string Iabc = "";
        //    string StrDy = "";//电压值
        //    string StrDl = "";//电流值

        //    string ItemKey = Cus_DgnItem.电流回路阻抗;
        //    if (meter.MeterDgns.ContainsKey(ItemKey))
        //    {
        //        int iIndex = 0;
        //        string key = ItemKey + "01";
        //        string[] errW = meter.MeterDgns[key].Value.Split('|');

        //        int j = 0;
        //        if (errW.Length == 5)
        //            j = 1;
        //        else if (errW.Length == 7)
        //            j = 2;
        //        else if (errW.Length == 9)
        //            j = 3;
        //        else
        //            j = 1;

        //        for (int i = 0; i < j; i++)
        //        {

        //            if (i == 0)
        //                Iabc = "A";
        //            else if (i == 1)
        //                Iabc = "C";
        //            else
        //                Iabc = "B";

        //            if (j == 3)
        //            {
        //                if (i == 1)
        //                {
        //                    StrDy = errW[4];
        //                    StrDl = errW[5];
        //                }
        //                else if (i == 2)
        //                {
        //                    StrDy = errW[2];
        //                    StrDl = errW[3];
        //                }
        //                else
        //                {
        //                    StrDy = errW[i * 2];
        //                    StrDl = errW[i * 2 + 1];
        //                }
        //            }
        //            else
        //            {
        //                StrDy = errW[i * 2];
        //                StrDl = errW[i * 2 + 1];
        //            }

        //            string itemID = "028";
        //            if (TaskId.IndexOf(itemID) == -1) continue;

        //            M_QT_IMPCURRENT_MET_CONC entity = new M_QT_IMPCURRENT_MET_CONC
        //            {
        //                IABC = GetPCode("currentPhaseCode", Iabc),//电流相别  
        //                VOLTAGE = StrDy,//电压值
        //                CURRENT = StrDl,//电流值
        //                IMPEDAN_VALUES_AVG = errW[errW.Length - 3],//回路阻抗平均值
        //                IMPEDAN_VALUES_ABS = errW[errW.Length - 2],//电流值
        //                BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //                DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //                EXPET_CATEG = "01",//code 试品类别 电能表
        //                DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //                BAR_CODE = meter.MD_BarCode,//条形码
        //                PARA_INDEX = (iIndex + 1).ToString(),//序号
        //                TEST_CATEGORIES = "01",//检定项  p_code=1
        //                ITEM_ID = itemID,//质检编码
        //                METER_CONST_CODE = meter.MD_Constant,//常数
        //                DETECT_ITEM_POINT = (iIndex + 1).ToString(),//检定项序号
        //                BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向

        //                LOAD_CURRENT = GetPCode("meterTestCurLoad", "Imax"),//负载电流
        //                LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //                FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //                PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //                ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //                RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //                VOLT_DATE = meter.VerifyDate,//测试时间
        //                TEST_REQUIRE = "",//国金
        //                TEST_CONDITION = "",//国金
        //                WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //                HANDLE_FLAG = "0",////字符串
        //                HANDLE_DATE = "",//国金
        //                DETECT_RESULT = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02",
        //                TEST_CONC = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02",//试验结果
        //                TEST_USER_NAME = meter.Checker1,
        //                AUDIT_USER_NAME = meter.Checker2,
        //                SIMPLING = ""//采样次数
        //            };
        //            sqlList.Add(entity.ToInsertString());
        //        }

        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 抗接地故障抑制能力试验 -国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_GROUNDFAULT_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    string ItemKey = Cus_DgnItem.抗接地故障抑制;

        //    string itemID = "029";

        //    List<string> sqlList = new List<string>();
        //    if (meter.MeterDgns.ContainsKey(ItemKey))
        //    {
        //        int iIndex = 0;
        //        int i = 0;
        //        int count039 = 0;
        //        foreach (string key in meter.MeterDgns.Keys)
        //        {
        //            if (key.IndexOf("039") != 0) continue;
        //            count039++;
        //        }

        //        foreach (string key in meter.MeterDgns.Keys)
        //        {
        //            if (key.IndexOf("039") != 0) continue;

        //            string[] errW = meter.MeterDgns[key].Value.Split('|');

        //            string Iabc = count039 >= 5 ? "C" : "B";
        //            if (i == 1)
        //                Iabc = "ABC";
        //            else if (i == 2)
        //                Iabc = "A";
        //            else if (i == 3)
        //                Iabc = count039 >= 5 ? "B" : "C";
        //            else
        //                Iabc = count039 >= 5 ? "C" : "B";

        //            if (TaskId.IndexOf(itemID) == -1) continue;

        //            M_QT_GROUNDFAULT_MET_CONC entity = new M_QT_GROUNDFAULT_MET_CONC
        //            {
        //                IABC = GetPCode("currentPhaseCode", Iabc),//电流相别  
        //                BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //                DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //                EXPET_CATEG = "01",//code 试品类别 电能表
        //                DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //                BAR_CODE = meter.MD_BarCode,//条形码
        //                PARA_INDEX = (iIndex + 1).ToString(),//序号
        //                TEST_CATEGORIES = "01",//检定项  p_code=1
        //                ITEM_ID = itemID,//质检编码
        //                DETECT_ITEM_POINT = (iIndex + 1).ToString(),//检定项序号
        //                BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向

        //                LOAD_CURRENT = GetPCode("meterTestCurLoad", "Imax"),//负载电流
        //                LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //                FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //                PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //                ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //                RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //                VOLT_DATE = meter.VerifyDate,//测试时间
        //                TEST_REQUIRE = "",//国金
        //                TEST_CONDITION = "",//国金
        //                WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //                HANDLE_FLAG = "0",////字符串
        //                HANDLE_DATE = "",//国金
        //                DETECT_RESULT = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02",
        //                CHK_CONC_CODE = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02",//试验结果
        //                TEST_USER_NAME = meter.Checker1,
        //                AUDIT_USER_NAME = meter.Checker2,
        //                ERROR = errW[0] + "|" + errW[1],
        //                INT_VARIA_ERR = errW[2],
        //                VARIA_ERR = errW[3],
        //                VALUE_ABS = "±0.7"
        //            };

        //            sqlList.Add(entity.ToInsertString());
        //            i++;
        //        }

        //    }

        //    return sqlList;

        //}

        ///// <summary>
        ///// 测量及监测误差-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_MEASONERR_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    string StrBJ = "", StrBZ = "", StrBWC = "";
        //    string strResult = "";
        //    string ItemKey = Cus_DgnItem.测量及监测误差;
        //    int iIndex = 0;
        //    List<string> sqlList = new List<string>();
        //    string[] strDnb = null;
        //    string[] strBzb = null;
        //    string[] strWc = null;

        //    string StrYj = "";
        //    int intj = 0;
        //    foreach (string key in meter.MeterDgns.Keys)
        //    {
        //        if (key.IndexOf("041") == 0) intj++;
        //    }

        //    string itemId = "063";

        //    M_QT_MEASONERR_MET_CONC entity = new M_QT_MEASONERR_MET_CONC();
        //    for (int i = 1; i < intj; i++)
        //    {
        //        iIndex++;

        //        if (TaskId.IndexOf(itemId) == -1) continue;

        //        string key = ItemKey + i.ToString("D2");
        //        if (meter.MeterDgns.ContainsKey(ItemKey))
        //        {
        //            strResult = meter.MeterDgns[ItemKey].Result == ConstHelper.合格 ? "01" : "02";
        //        }
        //        if (meter.MeterDgns.ContainsKey(key))
        //        {
        //            string[] arr = meter.MeterDgns[key].Value.Split('|');
        //            if (arr.Length > 1)
        //            {
        //                strDnb = arr[0].Split(',');
        //                strBzb = arr[1].Split(',');
        //                strWc = arr[2].Split(',');
        //            }
        //        }
        //        switch (key)
        //        {
        //            case "04101": //120%Un
        //                #region 04101
        //                for (int j = 0; j <= 2; j++)
        //                {
        //                    if (strDnb == null) continue;
        //                    switch (j.ToString())
        //                    {
        //                        case "0":
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "120%Un";//国金试验要求

        //                            StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                            StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                            StrBWC = strWc.Length > 0 ? strWc[0] : "";
        //                            break;
        //                        case "1":
        //                            if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "B";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "120%Un";//国金试验要求
        //                            StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
        //                            StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
        //                            StrBWC = strWc.Length > 1 ? strWc[1] : "";
        //                            break;
        //                        case "2":
        //                            if (meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "C";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "120%Un";//国金试验要求
        //                            StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
        //                            StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
        //                            StrBWC = strWc.Length > 2 ? strWc[2] : "";
        //                            break;
        //                        default:
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "120%Un";//国金试验要求
        //                            StrBJ = "";
        //                            StrBZ = "";
        //                            StrBWC = "";
        //                            break;
        //                    }
        //                    entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString();//序号
        //                    entity.ITEM_ID = "063";//质检编码

        //                    entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "120%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;   //试验相对湿度 
        //                    entity.TEST_CONC = strResult;           //试验结果
        //                    entity.VOLT_DATE = meter.VerifyDate;    //测试时间
        //                    entity.TEST_CONDITION = "";             //国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";           //字符串
        //                    entity.HANDLE_DATE = "";            //国金
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.TEST_VALUES = StrBZ;
        //                    entity.TEST_VALUES_AVG = StrBZ;
        //                    entity.TEST_VALUES_INT = StrBZ;
        //                    entity.TEST_VALUES_ABS = "±1%";     //限值
        //                    entity.TEST_VALUES1 = StrBJ;
        //                    entity.TEST_VALUES_AVG1 = StrBJ;
        //                    entity.TEST_VALUES_INT1 = StrBJ;
        //                    entity.ERROR = StrBWC;
        //                    entity.AVE_ERR = StrBWC;
        //                    entity.INT_CONV_ERR = StrBWC;
        //                    //entity.TEST_VALUES_ABS = "";//误差限
        //                    sqlList.Add(entity.ToInsertString());

        //                }
        //                break;
        //            #endregion
        //            case "04102": //100%Un
        //                #region 04102

        //                for (int j = 0; j <= 2; j++)
        //                {
        //                    if (strDnb == null) continue;
        //                    switch (j.ToString())
        //                    {
        //                        case "0":
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "100%Un";//国金试验要求

        //                            StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                            StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                            StrBWC = strWc.Length > 0 ? strWc[0] : "";
        //                            break;
        //                        case "1":
        //                            if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "B";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "100%Un";//国金试验要求

        //                            StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
        //                            StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
        //                            StrBWC = strWc.Length > 1 ? strWc[1] : "";
        //                            break;
        //                        case "2":
        //                            if (meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "C";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "100%Un";//国金试验要求

        //                            StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
        //                            StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
        //                            StrBWC = strWc.Length > 2 ? strWc[2] : "";
        //                            break;
        //                        default:
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "100%Un";//国金试验要求
        //                            StrBJ = "";
        //                            StrBZ = "";
        //                            StrBWC = "";
        //                            break;
        //                    }
        //                    entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString();//序号
        //                    entity.ITEM_ID = "063";//质检编码
        //                    if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                    entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                    entity.TEST_CONC = strResult;//试验结果
        //                    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                    entity.TEST_CONDITION = "";//国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";////字符串
        //                    entity.HANDLE_DATE = "";//国金
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.TEST_VALUES = StrBZ;
        //                    entity.TEST_VALUES_AVG = StrBZ;
        //                    entity.TEST_VALUES_INT = StrBZ;
        //                    entity.TEST_VALUES_ABS = "±1%";
        //                    entity.TEST_VALUES1 = StrBJ;
        //                    entity.TEST_VALUES_AVG1 = StrBJ;
        //                    entity.TEST_VALUES_INT1 = StrBJ;
        //                    entity.ERROR = StrBWC;
        //                    entity.AVE_ERR = StrBWC;
        //                    entity.INT_CONV_ERR = StrBWC;
        //                    sqlList.Add(entity.ToInsertString());

        //                }
        //                break;
        //            #endregion
        //            case "04103":  //60%Un
        //                #region 04103

        //                for (int j = 0; j <= 2; j++)
        //                {
        //                    if (strDnb == null) continue;
        //                    switch (j.ToString())
        //                    {
        //                        case "0":
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "60%Un";//国金试验要求

        //                            StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                            StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                            StrBWC = strWc.Length > 0 ? strWc[0] : "";
        //                            break;
        //                        case "1":
        //                            if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "B";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "60%Un";//国金试验要求
        //                            StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
        //                            StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
        //                            StrBWC = strWc.Length > 1 ? strWc[1] : "";

        //                            break;
        //                        case "2":
        //                            if (meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "C";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "60%Un";//国金试验要求

        //                            StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
        //                            StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
        //                            StrBWC = strWc.Length > 2 ? strWc[2] : "";

        //                            break;
        //                        default:
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "01";//检定项 
        //                            entity.TEST_REQUIRE = "60%Un";//国金试验要求
        //                            StrBJ = "";
        //                            StrBZ = "";
        //                            StrBWC = "";
        //                            break;
        //                    }
        //                    entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString();//序号
        //                    entity.ITEM_ID = "063";//质检编码
        //                    if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                    entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "60%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                    entity.TEST_CONC = strResult;//试验结果
        //                    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                    entity.TEST_CONDITION = "";//国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";////字符串
        //                    entity.HANDLE_DATE = "";//国金
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.TEST_VALUES = StrBZ;
        //                    entity.TEST_VALUES_AVG = StrBZ;
        //                    entity.TEST_VALUES_INT = StrBZ;
        //                    entity.TEST_VALUES_ABS = "±1%";
        //                    entity.TEST_VALUES1 = StrBJ;
        //                    entity.TEST_VALUES_AVG1 = StrBJ;
        //                    entity.TEST_VALUES_INT1 = StrBJ;
        //                    entity.ERROR = StrBWC;
        //                    entity.AVE_ERR = StrBWC;
        //                    entity.INT_CONV_ERR = StrBWC;
        //                    sqlList.Add(entity.ToInsertString());

        //                }
        //                break;
        //            #endregion
        //            case "04104": //120%Imax
        //                #region 04104
        //                for (int j = 0; j <= 2; j++)
        //                {
        //                    if (strDnb == null) continue;
        //                    switch (j.ToString())
        //                    {
        //                        case "0":
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "120%Imax";//国金试验要求

        //                            StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                            StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                            StrBWC = strWc.Length > 0 ? strWc[0] : "";
        //                            break;
        //                        case "1":
        //                            if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "B";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "120%Imax";//国金试验要求
        //                            StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
        //                            StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
        //                            StrBWC = strWc.Length > 1 ? strWc[1] : "";
        //                            break;
        //                        case "2":
        //                            if (meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "C";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "120%Imax";//国金试验要求
        //                            StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
        //                            StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
        //                            StrBWC = strWc.Length > 2 ? strWc[2] : "";
        //                            break;
        //                        default:
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "120%Imax";//国金试验要求
        //                            StrBJ = "";
        //                            StrBZ = "";
        //                            StrBWC = "";
        //                            break;
        //                    }
        //                    entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString();//序号
        //                    entity.ITEM_ID = "063";//质检编码
        //                    if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                    entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                    entity.TEST_CONC = strResult;//试验结果
        //                    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                    entity.TEST_CONDITION = "";//国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";////字符串
        //                    entity.HANDLE_DATE = "";//国金
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.TEST_VALUES = StrBZ;
        //                    entity.TEST_VALUES_AVG = StrBZ;
        //                    entity.TEST_VALUES_INT = StrBZ;
        //                    entity.TEST_VALUES_ABS = "±1%";
        //                    entity.TEST_VALUES1 = StrBJ;
        //                    entity.TEST_VALUES_AVG1 = StrBJ;
        //                    entity.TEST_VALUES_INT1 = StrBJ;
        //                    entity.ERROR = StrBWC;
        //                    entity.AVE_ERR = StrBWC;
        //                    entity.INT_CONV_ERR = StrBWC;
        //                    sqlList.Add(entity.ToInsertString());
        //                }
        //                break;
        //            #endregion
        //            case "04105": //100%Ib
        //                #region 04105
        //                for (int j = 0; j <= 2; j++)
        //                {
        //                    if (strDnb == null) continue;
        //                    switch (j.ToString())
        //                    {
        //                        case "0":
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "100%Ib";//国金试验要求

        //                            StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                            StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                            StrBWC = strWc.Length > 0 ? strWc[0] : "";

        //                            break;
        //                        case "1":
        //                            if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "B";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "100%Ib";//国金试验要求
        //                            StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
        //                            StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
        //                            StrBWC = strWc.Length > 1 ? strWc[1] : "";

        //                            break;
        //                        case "2":
        //                            if (meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "C";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "100%Ib";//国金试验要求
        //                            StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
        //                            StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
        //                            StrBWC = strWc.Length > 2 ? strWc[2] : "";

        //                            break;
        //                        default:
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "100%Ib";//国金试验要求
        //                            StrBJ = "";
        //                            StrBZ = "";
        //                            StrBWC = "";
        //                            break;
        //                    }
        //                    entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString();//序号
        //                    entity.ITEM_ID = "063";//质检编码
        //                    if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                    entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                    entity.TEST_CONC = strResult;//试验结果
        //                    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                    entity.TEST_CONDITION = "";//国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";////字符串
        //                    entity.HANDLE_DATE = "";//国金
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.TEST_VALUES = StrBZ;
        //                    entity.TEST_VALUES_AVG = StrBZ;
        //                    entity.TEST_VALUES_INT = StrBZ;
        //                    entity.TEST_VALUES_ABS = "±1%";
        //                    entity.TEST_VALUES1 = StrBJ;
        //                    entity.TEST_VALUES_AVG1 = StrBJ;
        //                    entity.TEST_VALUES_INT1 = StrBJ;
        //                    entity.ERROR = StrBWC;
        //                    entity.AVE_ERR = StrBWC;
        //                    entity.INT_CONV_ERR = StrBWC;
        //                    sqlList.Add(entity.ToInsertString());
        //                }
        //                break;
        //            #endregion
        //            case "04106": //5%Ib
        //                #region 04106
        //                for (int j = 0; j <= 2; j++)
        //                {
        //                    if (strDnb == null) continue;
        //                    switch (j.ToString())
        //                    {
        //                        case "0":
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "5%Ib";//国金试验要求

        //                            StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                            StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                            StrBWC = strWc.Length > 0 ? strWc[0] : "";
        //                            break;
        //                        case "1":
        //                            if (meter.MD_WiringMode == "三相三线" || meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "B";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "5%Ib";//国金试验要求

        //                            StrBJ = strDnb.Length > 1 ? strDnb[1] : "";
        //                            StrBZ = strBzb.Length > 1 ? strBzb[1] : "";
        //                            StrBWC = strWc.Length > 1 ? strWc[1] : "";
        //                            break;
        //                        case "2":
        //                            if (meter.MD_WiringMode == "单相") continue;
        //                            StrYj = "C";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "5%Ib";//国金试验要求
        //                            StrBJ = strDnb.Length > 2 ? strDnb[2] : "";
        //                            StrBZ = strBzb.Length > 2 ? strBzb[2] : "";
        //                            StrBWC = strWc.Length > 2 ? strWc[2] : "";
        //                            break;
        //                        default:
        //                            StrYj = "A";
        //                            entity.TEST_CATEGORIES = "02";//检定项 
        //                            entity.TEST_REQUIRE = "5%Ib";//国金试验要求
        //                            StrBJ = "";
        //                            StrBZ = "";
        //                            StrBWC = "";
        //                            break;
        //                    }
        //                    entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString();//序号
        //                    entity.ITEM_ID = "063";//质检编码
        //                    if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                    entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                    entity.TEST_CONC = strResult;//试验结果
        //                    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                    entity.TEST_CONDITION = "";//国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";////字符串
        //                    entity.HANDLE_DATE = "";//国金
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.TEST_VALUES = StrBZ;
        //                    entity.TEST_VALUES_AVG = StrBZ;
        //                    entity.TEST_VALUES_INT = StrBZ;
        //                    entity.TEST_VALUES_ABS = "±1%";
        //                    entity.TEST_VALUES1 = StrBJ;
        //                    entity.TEST_VALUES_AVG1 = StrBJ;
        //                    entity.TEST_VALUES_INT1 = StrBJ;
        //                    entity.ERROR = StrBWC;
        //                    entity.AVE_ERR = StrBWC;
        //                    entity.INT_CONV_ERR = StrBWC;
        //                    sqlList.Add(entity.ToInsertString());

        //                }
        //                break;
        //            #endregion
        //            case "04107": //120%Un/120%Imax/1.0
        //                #region 04107
        //                if (strDnb == null) continue;

        //                if (meter.MD_WiringMode == "三相三线")
        //                {
        //                    StrYj = "AC";
        //                }
        //                else
        //                {
        //                    StrYj = "ABC";
        //                }
        //                entity.TEST_CATEGORIES = "03";//检定项 
        //                entity.TEST_REQUIRE = "120%Un×120%Imax×1.0";//国金试验要求

        //                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                StrBWC = strWc.Length > 0 ? strWc[0] : "";

        //                entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                entity.PARA_INDEX = iIndex.ToString();//序号
        //                entity.ITEM_ID = "063";//质检编码
        //                if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                entity.TEST_CONC = strResult;//试验结果
        //                entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                entity.TEST_CONDITION = "";//国金
        //                entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                entity.HANDLE_FLAG = "0";////字符串
        //                entity.HANDLE_DATE = "";//国金
        //                entity.TEST_USER_NAME = meter.Checker1;
        //                entity.AUDIT_USER_NAME = meter.Checker2;
        //                entity.TEST_VALUES = StrBZ;
        //                entity.TEST_VALUES_AVG = StrBZ;
        //                entity.TEST_VALUES_INT = StrBZ;
        //                entity.TEST_VALUES_ABS = "±1%";
        //                entity.TEST_VALUES1 = StrBJ;
        //                entity.TEST_VALUES_AVG1 = StrBJ;
        //                entity.TEST_VALUES_INT1 = StrBJ;
        //                entity.ERROR = StrBWC;
        //                entity.AVE_ERR = StrBWC;
        //                entity.INT_CONV_ERR = StrBWC;
        //                sqlList.Add(entity.ToInsertString());


        //                break;
        //            #endregion
        //            case "04108": //100%Un/100%Ib/1.0
        //                #region 04108
        //                if (strDnb == null) continue;

        //                if (meter.MD_WiringMode == "三相三线")
        //                {
        //                    StrYj = "AC";
        //                }
        //                else
        //                {
        //                    StrYj = "ABC";
        //                }
        //                entity.TEST_CATEGORIES = "03";//检定项 
        //                entity.TEST_REQUIRE = "100%Un×100%Ib×1.0";//国金试验要求

        //                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                StrBWC = strWc.Length > 0 ? strWc[0] : "";

        //                entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                entity.PARA_INDEX = iIndex.ToString();//序号
        //                entity.ITEM_ID = "063";//质检编码
        //                if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                entity.TEST_CONC = strResult;//试验结果
        //                entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                entity.TEST_CONDITION = "";//国金
        //                entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                entity.HANDLE_FLAG = "0";////字符串
        //                entity.HANDLE_DATE = "";//国金
        //                entity.TEST_USER_NAME = meter.Checker1;
        //                entity.AUDIT_USER_NAME = meter.Checker2;
        //                entity.TEST_VALUES = StrBZ;
        //                entity.TEST_VALUES_AVG = StrBZ;
        //                entity.TEST_VALUES_INT = StrBZ;
        //                entity.TEST_VALUES_ABS = "±1%";
        //                entity.TEST_VALUES1 = StrBJ;
        //                entity.TEST_VALUES_AVG1 = StrBJ;
        //                entity.TEST_VALUES_INT1 = StrBJ;
        //                entity.ERROR = StrBWC;
        //                entity.AVE_ERR = StrBWC;
        //                entity.INT_CONV_ERR = StrBWC;
        //                sqlList.Add(entity.ToInsertString());


        //                break;
        //            #endregion
        //            case "04109": //100%Un/0.4%Ib/1.0
        //                #region 04109
        //                if (strDnb == null) continue;

        //                if (meter.MD_WiringMode == "三相三线")
        //                {
        //                    StrYj = "AC";
        //                }
        //                else
        //                {
        //                    StrYj = "ABC";
        //                }
        //                entity.TEST_CATEGORIES = "03";//检定项 

        //                string Bdj;
        //                string Qddl = "0.4";
        //                Bdj = meter.MD_Grane;


        //                if (meter.MD_ConnectionFlag == "互感式")
        //                {
        //                    Bdj = Bdj.Split('(')[0];

        //                    if (Bdj.IndexOf("0.2") != -1 || Bdj.IndexOf("0.5") != -1)
        //                    {
        //                        Qddl = "0.1";
        //                    }

        //                    else if (Bdj.IndexOf("1.0") != -1)
        //                    {
        //                        Qddl = "0.2";
        //                    }
        //                    else
        //                    {
        //                        Qddl = "0.4";
        //                    }
        //                }

        //                entity.TEST_REQUIRE = "100%Un×" + Qddl + "%Ib×1.0";//国金试验要求
        //                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                StrBWC = strWc.Length > 0 ? strWc[0] : "";

        //                entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                entity.PARA_INDEX = iIndex.ToString();//序号
        //                entity.ITEM_ID = "063";//质检编码
        //                if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                entity.TEST_CONC = strResult;//试验结果
        //                entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                entity.TEST_CONDITION = "";//国金
        //                entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                entity.HANDLE_FLAG = "0";////字符串
        //                entity.HANDLE_DATE = "";//国金
        //                entity.TEST_USER_NAME = meter.Checker1;
        //                entity.AUDIT_USER_NAME = meter.Checker2;
        //                entity.TEST_VALUES = StrBZ;
        //                entity.TEST_VALUES_AVG = StrBZ;
        //                entity.TEST_VALUES_INT = StrBZ;
        //                entity.TEST_VALUES_ABS = "±1%";
        //                entity.TEST_VALUES1 = StrBJ;
        //                entity.TEST_VALUES_AVG1 = StrBJ;
        //                entity.TEST_VALUES_INT1 = StrBJ;
        //                entity.ERROR = StrBWC;
        //                entity.AVE_ERR = StrBWC;
        //                entity.INT_CONV_ERR = StrBWC;
        //                entity.DETECT_RESULT = ""; //"01" : "02"
        //                sqlList.Add(entity.ToInsertString());

        //                break;

        //            #endregion
        //            case "04110": //0.5L
        //                #region 04110
        //                if (strDnb == null) continue;

        //                if (meter.MD_WiringMode == "三相三线")
        //                {
        //                    StrYj = "AC";
        //                }
        //                else
        //                {
        //                    StrYj = "ABC";
        //                }
        //                entity.TEST_CATEGORIES = "04";//检定项 
        //                entity.TEST_REQUIRE = "0.5L";//国金试验要求
        //                StrBJ = strDnb.Length > 0 ? strDnb[0] : "";
        //                StrBZ = strBzb.Length > 0 ? strBzb[0] : "";
        //                StrBWC = strWc.Length > 0 ? strWc[0] : "";

        //                entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别  
        //                entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //                entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                entity.PARA_INDEX = iIndex.ToString();//序号
        //                entity.ITEM_ID = "063";//质检编码
        //                if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                entity.METER_CONST_CODE = meter.MD_Constant;//常数
        //                entity.DETECT_ITEM_POINT = iIndex.ToString();//检定项序号
        //                entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                entity.TEST_CONC = strResult;//试验结果
        //                entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                entity.TEST_CONDITION = "";//国金
        //                entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                entity.HANDLE_FLAG = "0";////字符串
        //                entity.HANDLE_DATE = "";//国金
        //                entity.TEST_USER_NAME = meter.Checker1;
        //                entity.AUDIT_USER_NAME = meter.Checker2;
        //                entity.TEST_VALUES = StrBZ;
        //                entity.TEST_VALUES_AVG = StrBZ;
        //                entity.TEST_VALUES_INT = StrBZ;
        //                entity.TEST_VALUES_ABS = "±1%";
        //                entity.TEST_VALUES1 = StrBJ;
        //                entity.TEST_VALUES_AVG1 = StrBJ;
        //                entity.TEST_VALUES_INT1 = StrBJ;
        //                entity.ERROR = StrBWC;
        //                entity.AVE_ERR = StrBWC;
        //                entity.INT_CONV_ERR = StrBWC;
        //                sqlList.Add(entity.ToInsertString());

        //                break;
        //                #endregion
        //        }
        //    }

        //    return sqlList;

        //}

        ///// <summary>
        ///// 需量示值误差-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>　
        //private List<string> GetM_QT_DEMANDERR_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();

        //    string[] xlwc = GetXLData(meter);

        //    M_QT_DEMANDERR_MET_CONC entity = new M_QT_DEMANDERR_MET_CONC
        //    {
        //        BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //        DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //        EXPET_CATEG = "01",//code 试品类别 电能表
        //        DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //        BAR_CODE = meter.MD_BarCode,//条形码
        //        TEST_CATEGORIES = "01",//检定项
        //        ITEM_ID = "010",//质检编码

        //        ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //        RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //        TEST_USER_NAME = meter.Checker1,
        //        AUDIT_USER_NAME = meter.Checker2,
        //        VOLT_DATE = meter.VerifyDate,//测试时间
        //        WRITE_DATE = DateTime.Now.ToString(),//写入时间

        //        TEST_REQUIRE = "",//国金
        //        TEST_CONDITION = "",//国金
        //        HANDLE_FLAG = "0",////字符串
        //        HANDLE_DATE = "",//国金

        //        TEST_TIME = "15",//试验时间
        //        DEMAND_PERIOD = "15",//需量周期时间 按照分钟传
        //        DEMAND_TIME = "1",//滑差时间 
        //        DEMAND_INTERVAL = "1",//滑差次数
        //        REAL_PERIOD = "15"//实际周期
        //    };

        //    if (TaskId.IndexOf(entity.ITEM_ID) == -1) return sqlList;


        //    float FLDJ;

        //    string[] DlTmp = meter.MD_UA.Split('(');
        //    float IbTmp = float.Parse(DlTmp[0]);
        //    float ImaxTmp = float.Parse(DlTmp[1].Split(')')[0]);


        //    if (!string.IsNullOrEmpty(xlwc[0]) || !string.IsNullOrEmpty(xlwc[1]))
        //    {
        //        entity.PARA_INDEX = "01"; //序号


        //        entity.DETECT_ITEM_POINT = "01";//检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "Imax");//负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数

        //        entity.DETECT_RESULT = xlwc[4] == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = xlwc[4] == ConstHelper.合格 ? "01" : "02";//试验结果

        //        entity.REAL_DEMAND = xlwc[0]; //实际需量
        //        entity.DEMAND_STANDARD = xlwc[1];//标准需量
        //        entity.DEMAND_VALUE_ERR = xlwc[2];//需量示值误差
        //        string[] Strxl = Number.GetDj(meter.MD_Grane);//需量示值误差限
        //        //FLDJ = float.Parse(Strxl[0]);

        //        entity.DEMAND_ERR_ABS = "±1.0";
        //        //if (meter.MD_Grane == "1.0(2.0)")
        //        //{
        //        //    if (IbTmp == 5.0) entity.DEMAND_ERR_ABS = "±1.004";     //5倍Ib
        //        //    if (IbTmp == 10.0) entity.DEMAND_ERR_ABS = "±1.005";    //10倍Ib

        //        //}
        //        //if (meter.MD_Grane == "0.5S(2.0)") entity.DEMAND_ERR_ABS = "±0.5125";
        //        //if (meter.MD_Grane == "0.2S(2.0)") entity.DEMAND_ERR_ABS = "±0.2125";

        //        entity.CLEAR_DATA_RST = xlwc[4] == ConstHelper.合格 ? "01" : "02";//清零结论//

        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    if (!string.IsNullOrEmpty(xlwc[5]) || !string.IsNullOrEmpty(xlwc[6]))
        //    {
        //        entity.PARA_INDEX = "02"; //序号
        //        entity.DETECT_ITEM_POINT = "02";//检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "1.0Ib");//负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //        entity.DETECT_RESULT = xlwc[9] == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = xlwc[9] == ConstHelper.合格 ? "01" : "02";//试验结果

        //        entity.DEMAND_VALUE_ERR = xlwc[7];//需量示值误差
        //        entity.REAL_DEMAND = xlwc[5]; //实际需量
        //        entity.DEMAND_STANDARD = xlwc[6];//标准需量
        //        string[] Strxl = Number.GetDj(meter.MD_Grane);//需量示值误差限
        //        FLDJ = float.Parse(Strxl[0]);

        //        entity.DEMAND_ERR_ABS = "±1.0";
        //        //if (meter.MD_Grane == "1.0(2.0)") entity.DEMAND_ERR_ABS = "±1.05";
        //        //if (meter.MD_Grane == "0.5S(2.0)") entity.DEMAND_ERR_ABS = "±0.55";
        //        //if (meter.MD_Grane == "0.2S(2.0)") entity.DEMAND_ERR_ABS = "±0.25";

        //        entity.CLEAR_DATA_RST = xlwc[9] == ConstHelper.合格 ? "01" : "02";//清零结论//

        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    if (!string.IsNullOrEmpty(xlwc[10]) || !string.IsNullOrEmpty(xlwc[11]))
        //    {
        //        entity.PARA_INDEX = "03"; //序号
        //        entity.DETECT_ITEM_POINT = "03";//检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "0.1Ib");//负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数

        //        entity.DETECT_RESULT = xlwc[14] == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = xlwc[14] == ConstHelper.合格 ? "01" : "02";//试验结果

        //        entity.REAL_DEMAND = xlwc[10]; //实际需量
        //        entity.DEMAND_VALUE_ERR = xlwc[12];//需量示值误差
        //        entity.DEMAND_STANDARD = xlwc[11];//标准需量
        //        string[] Strxl = Number.GetDj(meter.MD_Grane);//需量示值误差限
        //        //FLDJ = float.Parse(Strxl[0]);

        //        entity.DEMAND_ERR_ABS = "±1.0";
        //        //if (meter.MD_Grane == "1.0(2.0)") entity.DEMAND_ERR_ABS = "±1.5";//HP 需量上传写死误差限
        //        //if (meter.MD_Grane == "0.5S(2.0)") entity.DEMAND_ERR_ABS = "±1.0";//HP 需量上传写死误差限
        //        //if (meter.MD_Grane == "0.2S(2.0)") entity.DEMAND_ERR_ABS = "±0.7";//HP 需量上传写死误差限

        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) return sqlList;
        //        entity.CLEAR_DATA_RST = xlwc[14] == ConstHelper.合格 ? "01" : "02";//清零结论

        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    return sqlList;
        //}

        ///// <summary>
        ///// 密钥更新-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_KEYUPDATE_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.密钥更新) == "") return sqlList;
        //    string itemId = "057";
        //    if (TaskId.IndexOf(itemId) == -1) return sqlList;

        //    M_QT_KEYUPDATE_MET_CONC entity = new M_QT_KEYUPDATE_MET_CONC
        //    {
        //        BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //        DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //        EXPET_CATEG = "01",//code 试品类别 电能表
        //        DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号   
        //        BAR_CODE = meter.MD_BarCode,//条形码
        //        PARA_INDEX = "01", //序号
        //        TEST_CATEGORIES = "02",//检定项
        //        ITEM_ID = itemId,//质检编码

        //        DETECT_ITEM_POINT = "01",//检定项序号
        //        BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向
        //        IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //        LOAD_CURRENT = GetPCode("meterTestCurLoad", "无"),//负载电流
        //        LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //        FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //        PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //        ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //        RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //        DETECT_RESULT = MisDataHelper.GetFkConclusion(meter, ProjectID.密钥更新),//试验结果
        //        TEST_CONC = MisDataHelper.GetFkConclusion(meter, ProjectID.密钥更新),//试验结果
        //        TEST_USER_NAME = meter.Checker1,
        //        AUDIT_USER_NAME = meter.Checker2,
        //        VOLT_DATE = meter.VerifyDate,//测试时间
        //        TEST_REQUIRE = "",//国金
        //        TEST_CONDITION = "",//国金
        //        WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //        HANDLE_FLAG = "0",////字符串
        //        HANDLE_DATE = "",//国金
        //        TEST_SUB = ""//检定分项
        //    };

        //    sqlList.Add(entity.ToInsertString());
        //    return sqlList;



        //}

        ///// <summary>
        ///// 安全认证-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_SAFETY_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.身份认证) == "") return sqlList;
        //    string itemId = "056";
        //    if (TaskId.IndexOf(itemId) < 0) return sqlList;

        //    M_QT_SAFETY_MET_CONC entity = new M_QT_SAFETY_MET_CONC
        //    {
        //        BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //        DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //        EXPET_CATEG = "01",//code 试品类别 电能表
        //        DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号   
        //        BAR_CODE = meter.MD_BarCode,//条形码
        //        PARA_INDEX = "01", //序号
        //        TEST_CATEGORIES = "01",//检定项
        //        ITEM_ID = itemId,//质检编码

        //        DETECT_ITEM_POINT = "01",//检定项序号
        //        BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"),//功率方向
        //        IABC = GetPCode("currentPhaseCode", "ABC"),//电流相别  
        //        LOAD_CURRENT = GetPCode("meterTestCurLoad", "无"),//负载电流
        //        LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),//电压
        //        FREQ = GetPCode("meterTestFreq", "50"), //code 频率
        //        PF = GetPCode("meterTestPowerFactor", "1.0"),//code 功率因数
        //        ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //        RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //        DETECT_RESULT = MisDataHelper.GetFkConclusion(meter, ProjectID.身份认证),//试验结果
        //        TEST_CONC = MisDataHelper.GetFkConclusion(meter, ProjectID.身份认证),//试验结果
        //        TEST_USER_NAME = meter.Checker1,
        //        AUDIT_USER_NAME = meter.Checker2,
        //        VOLT_DATE = meter.VerifyDate,//测试时间
        //        TEST_REQUIRE = "",//国金
        //        TEST_CONDITION = "",//国金
        //        WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //        HANDLE_FLAG = "0",////字符串
        //        HANDLE_DATE = "",//国金
        //        TEST_SUB = "011"//检定分项 meterSafetyCheckSubitem
        //    };

        //    sqlList.Add(entity.ToInsertString());
        //    return sqlList;

        //}

        ///// <summary>
        ///// 电能表远程控制-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_REMOTE_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();
        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.远程控制) == "") return sqlList;

        //    M_QT_REMOTE_MET_CONC entity = new M_QT_REMOTE_MET_CONC
        //    {
        //        BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //        DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //        EXPET_CATEG = "01",//code 试品类别 电能表
        //        DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号   
        //        BAR_CODE = meter.MD_BarCode,//条形码
        //        PARA_INDEX = "01", //序号
        //        TEST_CATEGORIES = "01",//检定项
        //        ITEM_ID = "058"//质检编码
        //    };


        //    if (TaskId.IndexOf(entity.ITEM_ID) == -1) return sqlList;
        //    entity.DETECT_ITEM_POINT = "01";//检定项序号
        //    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //    entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //    entity.DETECT_RESULT = MisDataHelper.GetFkConclusion(meter, ProjectID.远程控制);//试验结果
        //    entity.TEST_CONC = MisDataHelper.GetFkConclusion(meter, ProjectID.远程控制);//试验结果
        //    entity.TEST_USER_NAME = meter.Checker1;
        //    entity.AUDIT_USER_NAME = meter.Checker2;
        //    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //    entity.TEST_REQUIRE = "";//国金
        //    entity.TEST_CONDITION = "";//国金
        //    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //    entity.HANDLE_FLAG = "0";////字符串
        //    entity.HANDLE_DATE = "";//国金
        //    entity.TEST_SUB = "";//检定分项
        //    entity.TEST_ITEMS = "";


        //    sqlList.Add(entity.ToInsertString());
        //    return sqlList;
        //}

        ///// <summary>
        ///// 电能表参数更新表-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_PARAMEUPD_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();

        //    if (MisDataHelper.GetFkConclusion(meter, ProjectID.参数设置) == "") return sqlList;
        //    M_QT_PARAMEUPD_MET_CONC entity = new M_QT_PARAMEUPD_MET_CONC();
        //    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号   
        //    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //    entity.PARA_INDEX = "01"; //序号
        //    entity.TEST_CATEGORIES = "01";//检定项
        //    entity.ITEM_ID = "059";//质检编码


        //    if (TaskId.IndexOf(entity.ITEM_ID) == -1) return sqlList;
        //    entity.DETECT_ITEM_POINT = "01";//检定项序号
        //    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //    entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //    entity.DETECT_RESULT = MisDataHelper.GetFkConclusion(meter, ProjectID.参数设置);//试验结果
        //    entity.TEST_CONC = MisDataHelper.GetFkConclusion(meter, ProjectID.参数设置);//试验结果
        //    entity.TEST_USER_NAME = meter.Checker1;
        //    entity.AUDIT_USER_NAME = meter.Checker2;
        //    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //    entity.TEST_REQUIRE = "";//国金
        //    entity.TEST_CONDITION = "";//国金
        //    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //    entity.HANDLE_FLAG = "0";////字符串
        //    entity.HANDLE_DATE = "";//国金
        //    entity.TEST_SUB = "";//检定分项
        //    entity.TEST_ITEMS = "";


        //    sqlList.Add(entity.ToInsertString());
        //    return sqlList;
        //}

        ///// <summary>
        ///// 冻结-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_FREEZING_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;

        //    List<string> sqlList = new List<string>();
        //    string[] keys = new string[meter.MeterFreezes.Keys.Count];
        //    meter.MeterFreezes.Keys.CopyTo(keys, 0);

        //    for (int i = 0; i < keys.Length; i++)
        //    {
        //        if (keys[i].Length <= 3) continue;

        //        string itemKey = keys[i].Substring(0, 3);
        //        if (!meter.MeterFreezes.ContainsKey(itemKey)) continue;

        //        string strDjFs = "";//冻结方式
        //        string strJdx = "";//检定项编号

        //        MeterFreeze meterFreeze = meter.MeterFreezes[keys[i]];
        //        MeterFreeze meterFreeze1 = meter.MeterFreezes[itemKey];

        //        string strJl = meterFreeze1.Value;
        //        string[] strXmBh = meterFreeze.Value.Split('|');

        //        if (itemKey == "001") //定时冻结
        //        {
        //            switch (keys[i].Substring(3, 2))
        //            {

        //                case "01":
        //                    strDjFs = "月（周期）";
        //                    break;
        //                case "02":
        //                    strDjFs = "日（周期）";
        //                    break;
        //                case "03":
        //                    strDjFs = "小时（周期）";
        //                    break;
        //                default:
        //                    strDjFs = "日（周期）";
        //                    break;

        //            }
        //            strJdx = "01";

        //        }
        //        else if (itemKey == "002") //瞬时冻结
        //        {
        //            strJdx = "02";

        //        }
        //        else if (itemKey == "003") //日冻结
        //        {
        //            strJdx = "04";

        //        }
        //        else if (itemKey == "004") //约定冻结
        //        {
        //            switch (keys[i].Substring(3, 2))
        //            {

        //                case "01":
        //                    strDjFs = "两套时区表切换冻结";
        //                    break;
        //                case "02":
        //                    strDjFs = "两套日时段表切换冻结";
        //                    break;
        //                case "03":
        //                    strDjFs = "两套费率电价切换冻结";
        //                    break;
        //                case "04":
        //                    strDjFs = "两套阶梯切换冻结";
        //                    break;
        //                default:
        //                    strDjFs = "两套时区表切换冻结";
        //                    break;

        //            }
        //            strJdx = "03";//检定项

        //        }
        //        else if (itemKey == "005") //整点冻结
        //        {
        //            strJdx = "05";//检定项
        //        }
        //        M_QT_FREEZING_MET_CONC entity = new M_QT_FREEZING_MET_CONC();
        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (iIndex + 1).ToString(); //序号
        //        entity.ITEM_ID = "068";//质检编码


        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //        entity.TEST_CATEGORIES = strJdx;//检定项
        //        entity.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //strXmBh;//  GetPCode("meterLogOutTest",strXmh);//检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //        entity.TEST_CONC = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;

        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金
        //        entity.READ_VALUE = strXmBh[2];//冻结值
        //        entity.FREE_CATEG = strJdx;//冻结类别（原始记录）
        //        entity.FREE_MODE = strDjFs;//冻结方式（原始记录）
        //        entity.FREE_BEFORE_CURRENT = strXmBh[0];//冻结前电流（原始记录））
        //        entity.FREEING_CURRENT = strXmBh[1];//冻结时电流（原始记录）
        //        entity.FREE_AFTER_CURRENT = strXmBh[2];//冻结后电流（原始记录）
        //        if (keys[i].IndexOf("002") == 0)
        //        {
        //            if (strXmBh.Length == 5)
        //                entity.FREEING_CURRENT = strXmBh[1] + "|" + strXmBh[2] + "|" + strXmBh[3];//冻结时电流（原始记录）
        //            entity.FREE_AFTER_CURRENT = strXmBh[4];//冻结后电流（原始记录）
        //        }


        //        sqlList.Add(entity.ToInsertString());

        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 负荷记录试验-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_RECORD_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;
        //    List<string> sqlList = new List<string>();

        //    string[] Arr_ID = new string[meter.MeterLoadRecords.Keys.Count];
        //    meter.MeterLoadRecords.Keys.CopyTo(Arr_ID, 0);


        //    for (int i = 0; i < Arr_ID.Length; i++)
        //    {
        //        string _ID = Arr_ID[i];
        //        string strJl = "";
        //        if (_ID.Length > 3)
        //        {

        //            MeterLoadRecord MeterLoadRecords = meter.MeterLoadRecords[_ID];
        //            if (_ID.IndexOf("001") == 0)
        //                strJl = meter.MeterFunctions["001"].Value;

        //            string[] strXmh = MeterLoadRecords.SubName.Split('次');
        //            string StrFlx; //负荷记录类型
        //            string StrFcs; //负荷记录次数
        //            switch (strXmh[1])
        //            {
        //                case "第【01】类负荷记录":
        //                    StrFlx = "第【01】类(电压、电流)负荷记录";
        //                    break;
        //                case "第【02】类负荷记录":
        //                    StrFlx = "第【02】类(有、无功功率)负荷记录";
        //                    break;
        //                case "第【03】类负荷记录":
        //                    StrFlx = "第【03】类(功率因数)负荷记录";
        //                    break;
        //                case "第【04】类负荷记录":
        //                    StrFlx = "第【04】类(有、无功电能)负荷记录";
        //                    break;
        //                case "第【05】类负荷记录":
        //                    StrFlx = "第【05】类(四象限电能)负荷记录";
        //                    break;
        //                case "第【06】类负荷记录":
        //                    StrFlx = "第【06】类(当前需量)负荷记录";
        //                    break;
        //                default:
        //                    StrFlx = "第【01】类(电压、电流)负荷记录";
        //                    break;

        //            }
        //            switch (strXmh[0])
        //            {
        //                case "第【01】":
        //                    StrFcs = "第1次";
        //                    break;
        //                case "第【02】":
        //                    StrFcs = "第2次";
        //                    break;
        //                case "第【03】":
        //                    StrFcs = "第3次";
        //                    break;
        //                case "第【04】":
        //                    StrFcs = "第4次";
        //                    break;
        //                case "第【05】":
        //                    StrFcs = "第5次";
        //                    break;
        //                case "第【06】":
        //                    StrFcs = "第6次";
        //                    break;
        //                case "第【07】":
        //                    StrFcs = "第7次";
        //                    break;
        //                case "第【08】":
        //                    StrFcs = "第8次";
        //                    break;
        //                case "第【09】":
        //                    StrFcs = "第9次";
        //                    break;
        //                case "第【10】":
        //                    StrFcs = "第10次";
        //                    break;
        //                default:
        //                    StrFcs = "第1次";
        //                    break;

        //            }



        //            string[] arrEnergy = MeterLoadRecords.Value.Split(',');

        //            M_QT_RECORD_MET_CONC entity = new M_QT_RECORD_MET_CONC();
        //            entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //            entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //            entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //            entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //            entity.BAR_CODE = meter.MD_BarCode;//条形码
        //            entity.PARA_INDEX = iIndex.ToString(); //序号
        //            entity.DETECT_ITEM_POINT = (iIndex++).ToString(); //序号
        //            entity.ITEM_ID = "069";//质检编码


        //            if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //            entity.TEST_CATEGORIES = "01|02|03";
        //            entity.DETECT_ITEM_POINT = iIndex.ToString();//  GetPCode("meterStorageTest",strXmh);//检定项序号
        //            entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //            entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //            entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //            entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //            entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //            entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //            entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //            entity.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_CONC = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_USER_NAME = meter.Checker1;
        //            entity.AUDIT_USER_NAME = meter.Checker2;
        //            entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //            entity.TEST_REQUIRE = "";//国金
        //            entity.TEST_CONDITION = "";//国金
        //            entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //            entity.HANDLE_FLAG = "0";////字符串
        //            entity.HANDLE_DATE = "";//国金
        //            entity.RECORD_CATEG = StrFlx;//负荷记录类型（原始记录）
        //            entity.RECORD_INFO = arrEnergy[0];//负荷记录信息（原始记录）
        //            entity.RECORD_NO = StrFcs;//次数
        //            entity.RECORD_TIME = arrEnergy[1].ToString();//记录时间
        //            sqlList.Add(entity.ToInsertString());

        //        }

        //    }
        //    return sqlList;

        //}

        ///// <summary>
        ///// 基本功能-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_BASICFUN_MET_CONCByMt(TestMeterInfo meter)
        //{

        //    int iIndex = 0;

        //    List<string> sqlList = new List<string>();

        //    foreach (KeyValuePair<string, MeterFunction> kv in meter.MeterFunctions)
        //    {
        //        #region
        //        if (kv.Key.Length == 3)
        //        {
        //            string strXmBh = "";
        //            string require = "";
        //            //string require = "";//试验要求require
        //            switch (kv.Value.Name)
        //            {
        //                case "计量功能":
        //                    strXmBh = "01";//计量功能 1
        //                    require = "可计量正向及各费率电量。";
        //                    break;
        //                case "显示功能":
        //                    strXmBh = "02";//显示功能 1
        //                    require = "可显示电量、时间、报警、通信等信息，可上电全显，背光可自动关闭。";
        //                    break;
        //                case "脉冲输出功能":
        //                    strXmBh = "03";//脉冲输出功能 1
        //                    require = "具有光脉冲、电脉冲、时钟脉冲输出功能。";
        //                    break;
        //                case "费率时段功能":
        //                    strXmBh = "04"; //费率时段功能 1
        //                    break;
        //                case "事件记录功能":
        //                    strXmBh = "05"; //事件记录功能
        //                    break;
        //                case "最大需量功能":
        //                    strXmBh = "06"; //最大需量功能 1
        //                    break;
        //                case "报警功能":
        //                    strXmBh = "07"; //报警功能 1 在费控功能
        //                    require = "有错误代码或报警提示，背光持续点亮。";
        //                    break;
        //                case "停电抄表功能":
        //                    strXmBh = "08"; //停电抄表功能 0
        //                    require = "停电状态下，能够通过按键唤醒电能表，并抄读数据。";
        //                    break;
        //                case "计时功能":
        //                    strXmBh = "09"; //计时功能 1
        //                    require = "具有日历、计时功能。";
        //                    break;
        //            }

        //            if (string.IsNullOrEmpty(strXmBh)) continue;

        //            M_QT_BASICFUN_MET_CONC entity = new M_QT_BASICFUN_MET_CONC();
        //            entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //            entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //            entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //            entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //            entity.BAR_CODE = meter.MD_BarCode;//条形码
        //            entity.PARA_INDEX = iIndex.ToString(); //序号
        //            entity.TEST_CATEGORIES = strXmBh;//检定项
        //            entity.ITEM_ID = "064";//质检编码


        //            if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //            entity.DETECT_ITEM_POINT = iIndex.ToString();               //检定项序号
        //            entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"); //功率方向
        //            entity.IABC = GetPCode("currentPhaseCode", "ABC");              //电流相别  
        //            entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");       //负载电流
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");      //电压
        //            entity.FREQ = GetPCode("meterTestFreq", "50");                  //频率
        //            entity.PF = GetPCode("meterTestPowerFactor", "1.0");            //功率因数
        //            entity.ENVIRON_TEMPER = meter.Temperature;  //试验温度 
        //            entity.RELATIVE_HUM = meter.Humidity;       //相对湿度 

        //            entity.TEST_USER_NAME = meter.Checker1;
        //            entity.AUDIT_USER_NAME = meter.Checker2;
        //            entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //            entity.TEST_REQUIRE = require;           //国金
        //            entity.TEST_CONDITION = "";         //国金
        //            entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //            entity.HANDLE_FLAG = "0";           //字符串
        //            entity.HANDLE_DATE = "";            //国金


        //            entity.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_CONC = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //试验结果

        //            sqlList.Add(entity.ToInsertString());
        //        }
        //        #endregion

        //    }

        //    //显示功能
        //    foreach (KeyValuePair<string, MeterShow> kv in meter.MeterShows)
        //    {
        //        #region
        //        if (kv.Key.Length == 3)
        //        {
        //            string strXmBh = "";
        //            switch (kv.Value.Name)
        //            {

        //                case "显示功能":
        //                    strXmBh = "02";//显示功能 1
        //                    break;
        //            }

        //            if (string.IsNullOrEmpty(strXmBh)) continue;

        //            M_QT_BASICFUN_MET_CONC entity = new M_QT_BASICFUN_MET_CONC();
        //            entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //            entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //            entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //            entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //            entity.BAR_CODE = meter.MD_BarCode;//条形码
        //            entity.PARA_INDEX = iIndex.ToString(); //序号
        //            entity.TEST_CATEGORIES = strXmBh;//检定项
        //            entity.ITEM_ID = "064";//质检编码


        //            if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //            entity.DETECT_ITEM_POINT = iIndex.ToString();               //检定项序号
        //            entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"); //功率方向
        //            entity.IABC = GetPCode("currentPhaseCode", "ABC");              //电流相别  
        //            entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");       //负载电流
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");      //电压
        //            entity.FREQ = GetPCode("meterTestFreq", "50");                  //频率
        //            entity.PF = GetPCode("meterTestPowerFactor", "1.0");            //功率因数
        //            entity.ENVIRON_TEMPER = meter.Temperature;  //试验温度 
        //            entity.RELATIVE_HUM = meter.Humidity;       //相对湿度 

        //            entity.TEST_USER_NAME = meter.Checker1;
        //            entity.AUDIT_USER_NAME = meter.Checker2;
        //            entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //            entity.TEST_REQUIRE = "";           //国金
        //            entity.TEST_CONDITION = "";         //国金
        //            entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //            entity.HANDLE_FLAG = "0";           //字符串
        //            entity.HANDLE_DATE = "";            //国金


        //            entity.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_CONC = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //试验结果

        //            sqlList.Add(entity.ToInsertString());
        //        }
        //        #endregion

        //    }


        //    //费控功能 
        //    foreach (KeyValuePair<string, MeterFK> kv in meter.MeterCostControls)
        //    {
        //        #region
        //        if (kv.Key.Length == 3)
        //        {
        //            string strXmBh = "";
        //            string require = "";
        //            switch (kv.Value.Name)
        //            {
        //                case "报警功能":
        //                    strXmBh = "07"; //1 在费控功能
        //                    require = "有错误代码或报警提示，背光持续点亮。";
        //                    break;
        //                case "停电抄表功能":
        //                    strXmBh = "08"; //停电抄表功能 0
        //                    require = "停电状态下，能够通过按键唤醒电能表，并抄读数据。";
        //                    break;
        //                case "控制功能":    //1 在费控功能
        //                    strXmBh = "10";     //
        //                    require = "表计能以声、光或其他方式提醒及控制符合开关";
        //                    break;
        //            }

        //            if (string.IsNullOrEmpty(strXmBh)) continue;

        //            M_QT_BASICFUN_MET_CONC entity = new M_QT_BASICFUN_MET_CONC();
        //            entity.BASIC_ID = meter.Meter_ID.ToString();     //19位ID  基本信息标识
        //            entity.DETECT_TASK_NO = meter.MD_TaskNo;           //检定任务单编号
        //            entity.EXPET_CATEG = "01";                      //试品类别 电能表
        //            entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();  //检定设备编号 
        //            entity.BAR_CODE = meter.MD_BarCode;                //条形码
        //            entity.PARA_INDEX = iIndex.ToString();          //序号
        //            entity.TEST_CATEGORIES = strXmBh;               //检定项
        //            entity.ITEM_ID = "064";                         //质检编码


        //            if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //            entity.DETECT_ITEM_POINT = iIndex.ToString();               //检定项序号
        //            entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"); //功率方向
        //            entity.IABC = GetPCode("currentPhaseCode", "ABC");              //电流相别  
        //            entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");       //负载电流
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");      //电压
        //            entity.FREQ = GetPCode("meterTestFreq", "50");                  //频率
        //            entity.PF = GetPCode("meterTestPowerFactor", "1.0");            //功率因数
        //            entity.ENVIRON_TEMPER = meter.Temperature;  //试验温度 
        //            entity.RELATIVE_HUM = meter.Humidity;       //相对湿度 

        //            entity.TEST_USER_NAME = meter.Checker1;
        //            entity.AUDIT_USER_NAME = meter.Checker2;
        //            entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //            entity.TEST_REQUIRE = require;           //国金
        //            entity.TEST_CONDITION = "";         //国金
        //            entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //            entity.HANDLE_FLAG = "0";           //字符串
        //            entity.HANDLE_DATE = "";            //国金


        //            entity.DETECT_RESULT = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_CONC = kv.Value.Result == ConstHelper.合格 ? "01" : "02"; //试验结果

        //            sqlList.Add(entity.ToInsertString());
        //        }
        //        #endregion

        //    }


        //    #region 停电抄表功能
        //    if (TaskId.IndexOf("064") >= 0)
        //    {
        //        M_QT_BASICFUN_MET_CONC entity = new M_QT_BASICFUN_MET_CONC();
        //        entity.BASIC_ID = meter.Meter_ID.ToString();     //19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;           //检定任务单编号
        //        entity.EXPET_CATEG = "01";                      //试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();  //检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;                //条形码
        //        entity.PARA_INDEX = iIndex.ToString();          //序号
        //        entity.TEST_CATEGORIES = "08";                  //检定项 停电抄表功能
        //        entity.ITEM_ID = "064";                         //质检编码

        //        entity.DETECT_ITEM_POINT = iIndex.ToString();               //检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功"); //功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");              //电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");       //负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");      //电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50");                  //频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");            //功率因数
        //        entity.ENVIRON_TEMPER = meter.Temperature;  //试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;       //相对湿度 

        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";           //国金
        //        entity.TEST_CONDITION = "";         //国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";           //字符串
        //        entity.HANDLE_DATE = "";            //国金


        //        entity.DETECT_RESULT = "01"; //试验结果
        //        entity.TEST_CONC = "01"; //试验结果

        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    #endregion



        //    return sqlList;
        //}

        ///// <summary>
        ///// 误差一致性-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_CONSIST_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();

        //    string strKey = "1";
        //    if (!meter.MeterErrAccords.ContainsKey(strKey)) return sqlList;
        //    if (meter.MeterErrAccords[strKey].PointList.Count < 0) return sqlList;

        //    string[] strSubKey = new string[meter.MeterErrAccords[strKey].PointList.Keys.Count];
        //    if (meter.MeterErrAccords.ContainsKey(strKey))          //如果数据模型中已经存在该点的数据
        //    {
        //        int iIndex = 0;
        //        M_QT_CONSIST_MET_CONC entity = new M_QT_CONSIST_MET_CONC();

        //        foreach (string _subKey in meter.MeterErrAccords[strKey].PointList.Keys)
        //        {
        //            MeterErrAccordBase errAccord = meter.MeterErrAccords[strKey].PointList[_subKey];

        //            entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", errAccord.IbX);//负载电流
        //            entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);//code 功率因数
        //            entity.DETECT_RESULT = errAccord.Result == ConstHelper.合格 ? "01" : "02";//试验结果
        //            entity.TEST_CONC = errAccord.Result == ConstHelper.合格 ? "01" : "02";//试验结果
        //            entity.TEST_USER_NAME = meter.Checker1;
        //            entity.AUDIT_USER_NAME = meter.Checker2;
        //            entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //            entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //            entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //            entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //            entity.BAR_CODE = meter.MD_BarCode;//条形码
        //            entity.PARA_INDEX = (iIndex + 1).ToString();//序号
        //            entity.TEST_CATEGORIES = "01";//检定项
        //            entity.ITEM_ID = "014";//质检编码


        //            if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;

        //            if (errAccord.IbX == "0.1Ib" && errAccord.PF == "0.5L") continue;

        //            entity.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定项序号
        //            entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //            entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //            entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率

        //            entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 ENVIRON_TEMPER
        //            entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 

        //            entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //            entity.TEST_REQUIRE = "";//国金
        //            entity.TEST_CONDITION = "";//国金
        //            entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //            entity.HANDLE_FLAG = "0";////字符串
        //            entity.HANDLE_DATE = "";//国金

        //            string[] arrlimit = errAccord.Limit.Split('|');
        //            entity.VALUE_ABS = "±" + arrlimit[0];

        //            entity.ALL_AVG_ERROR = errAccord.ErrAver.Trim();
        //            entity.ALL_INT_ERROR = errAccord.Error.Trim();

        //            string[] Arr_Err = errAccord.Data1.Split('|');           //分解误差
        //            entity.ERROR = Arr_Err[0] + "|" + Arr_Err[1];
        //            entity.ERR_VALUE = errAccord.Error.Trim();
        //            entity.VARIA_ERR = errAccord.Error.Trim();
        //            entity.AVE_ERR = Arr_Err[2];
        //            entity.INT_VARIA_ERR = Arr_Err[3];
        //            entity.INT_CONVERT_ERR = Arr_Err[3];
        //            entity.SAMPLE_AVE = errAccord.ErrAver.Trim();//样品均值

        //            sqlList.Add(entity.ToInsertString());
        //        }
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 误差变差 -国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_ERROR_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;
        //    List<string> sqlList = new List<string>();

        //    string strKey = "2";
        //    if (!meter.MeterErrAccords.ContainsKey(strKey)) return sqlList;
        //    if (meter.MeterErrAccords[strKey].PointList.Count < 0) return sqlList;
        //    string[] strSubKey = new string[meter.MeterErrAccords[strKey].PointList.Keys.Count];

        //    //M_QT_ERROR_MET_CONCService Bo = new M_QT_ERROR_MET_CONCService();
        //    M_QT_ERROR_MET_CONC entity = new M_QT_ERROR_MET_CONC();


        //    foreach (string _subKey in meter.MeterErrAccords[strKey].PointList.Keys)
        //    {
        //        MeterErrAccordBase errAccord = meter.MeterErrAccords[strKey].PointList[_subKey];
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", errAccord.IbX);//负载电流
        //        entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);//code 功率因数
        //        entity.DETECT_RESULT = errAccord.Result == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = errAccord.Result == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;

        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (iIndex + 1).ToString();//序号
        //        entity.TEST_CATEGORIES = "01";//检定项
        //        entity.ITEM_ID = "013";//质检编码


        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //        entity.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  

        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率

        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 ENVIRON_TEMPER
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 

        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金


        //        entity.PULES = errAccord.PulseCount.ToString();


        //        string[] Arr_Err = errAccord.Data1.Split('|');           //分解误差
        //        entity.ONCE_ERR = Arr_Err[0] + "|" + Arr_Err[1];
        //        entity.AVG_ONCE_ERR = Arr_Err[2];
        //        entity.INT_ONCE_ERR = Arr_Err[3];
        //        entity.ERROR = errAccord.Error.Trim();

        //        string[] err0 = errAccord.Data2.Split('|');           //分解误差                        
        //        entity.SENC_ERR = err0[0] + "|" + Arr_Err[1];
        //        entity.AVG_SENC_ERR = err0[2];
        //        entity.INT_SENC_ERR = err0[3];
        //        entity.ERROR = errAccord.Error.Trim();



        //        entity.VARIA_ERR = errAccord.Error;//变差
        //        entity.INT_VARIA_ERR = errAccord.Error;//变差化整
        //        //entity.VALUE_ABS = "±" + errAccord.Limit.Substring(0, errAccord.Limit.IndexOf("|"));//变差限值
        //        entity.VALUE_ABS = errAccord.Limit.Substring(0, errAccord.Limit.IndexOf("|")) + "%";//变差限值
        //        entity.ERROR = errAccord.Error;//变差误差

        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    return sqlList;
        //}

        ///// <summary>
        ///// 负载电流升降变差 -国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_ERROR_MET_CONC_ByMt(TestMeterInfo meter)
        //{
        //    int iTnt = 10;
        //    string strKey = "3";
        //    List<string> sqlList = new List<string>();
        //    if (!meter.MeterErrAccords.ContainsKey(strKey)) return sqlList;
        //    if (meter.MeterErrAccords[strKey].PointList.Count < 0) return sqlList;

        //    string[] strSubKey = new string[meter.MeterErrAccords[strKey].PointList.Keys.Count];

        //    M_QT_ERROR_MET_CONC entity = new M_QT_ERROR_MET_CONC();


        //    foreach (string _subKey in meter.MeterErrAccords[strKey].PointList.Keys)
        //    {
        //        MeterErrAccordBase errAccord = meter.MeterErrAccords[strKey].PointList[_subKey];
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", errAccord.IbX);//负载电流
        //        entity.PF = GetPCode("meterTestPowerFactor", errAccord.PF);//code 功率因数
        //        entity.DETECT_RESULT = errAccord.Result == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = errAccord.Result == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;

        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (iTnt + 1).ToString();//序号
        //        entity.TEST_CATEGORIES = "01";//检定项
        //        entity.ITEM_ID = "015";//质检编码


        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //        entity.DETECT_ITEM_POINT = (iTnt + 1).ToString(); //检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  

        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率

        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 ENVIRON_TEMPER
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 

        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金

        //        entity.PULES = errAccord.PulseCount.ToString();


        //        string[] err0 = errAccord.Data1.Split('|');           //分解误差
        //        entity.ONCE_ERR = err0[0] + "|" + err0[1];
        //        entity.AVG_ONCE_ERR = err0[2];
        //        entity.INT_ONCE_ERR = err0[3];
        //        entity.ERROR = errAccord.Error.Trim();

        //        string[] err1 = errAccord.Data2.Split('|');           //分解误差                        
        //        entity.SENC_ERR = err1[0] + "|" + err1[1];
        //        entity.AVG_SENC_ERR = err1[2];
        //        entity.INT_SENC_ERR = err1[3];
        //        entity.ERROR = errAccord.Error.Trim();


        //        entity.VARIA_ERR = errAccord.Error;//变差
        //        entity.INT_VARIA_ERR = errAccord.Error;//变差化整
        //        entity.VALUE_ABS = Math.Abs(Convert.ToSingle(errAccord.Limit.Substring(0, errAccord.Limit.IndexOf("|")))).ToString() + "%";//变差限值
        //        entity.ERROR = errAccord.Error;//变差误差
        //        iTnt++;

        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    return sqlList;
        //}

        ///// <summary>
        ///// 事件记录-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_EVENTLOG_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;

        //    List<string> sqlList = new List<string>();
        //    string[] Arr_ID = new string[meter.MeterSjJLgns.Keys.Count];
        //    meter.MeterSjJLgns.Keys.CopyTo(Arr_ID, 0);

        //    string StrID = "";
        //    for (int i = 0; i < Arr_ID.Length; i++)
        //    {
        //        string _ID = Arr_ID[i];
        //        string strJl = "";
        //        string strXmh;
        //        string strXmBh = "";

        //        if (_ID.Length > 3)
        //        {
        //            StrID = _ID.Substring(0, _ID.Length - 1) + "2";
        //            if (!meter.MeterSjJLgns.ContainsKey(StrID)) continue;

        //            if (_ID != StrID)
        //            {
        //                if (meter.MeterSjJLgns.ContainsKey(_ID.Substring(0, 3)))
        //                {
        //                    MeterSjJLgn MeterSjJLgns = meter.MeterSjJLgns[_ID];
        //                    MeterSjJLgn MeterSjJLgns1 = meter.MeterSjJLgns[_ID.Substring(0, 3)];//这两个在数据库里没有005、012三位数的结论
        //                    MeterSjJLgn MeterSjJLgns2 = meter.MeterSjJLgns[StrID];
        //                    string[] STRSj = MeterSjJLgns.RecordOther.Split('|');//事件其他数据
        //                    string[] STRSj1 = MeterSjJLgns2.RecordOther.Split('|');//事件其他数据


        //                    strJl = MeterSjJLgns1.Result;
        //                    strXmh = MeterSjJLgns1.ItemName;
        //                    switch (strXmh)
        //                    {
        //                        case "编程记录":
        //                            strXmBh = "01";//编程 
        //                            break;
        //                        case "需量清零记录":
        //                            strXmBh = "02";//需量清零
        //                            break;
        //                        case "校时记录":
        //                            strXmBh = "03";//校时
        //                            break;
        //                        case "欠压记录":
        //                            strXmBh = "04";//各相欠压
        //                            break;
        //                        case "失压记录":
        //                            strXmBh = "05";//各相失压
        //                            break;
        //                        case "断相记录":
        //                            strXmBh = "06";//各相断相
        //                            break;
        //                        case "失流记录":
        //                            strXmBh = "07";//各相失流
        //                            break;
        //                        case "断流记录":
        //                            strXmBh = "08";//各相断流
        //                            break;
        //                        case "电流不平衡记录":
        //                            strXmBh = "09";//电流不平衡
        //                            break;
        //                        case "电压逆相序记录":
        //                            strXmBh = "10";//电压逆相序
        //                            break;
        //                        case "电表清零记录":
        //                            strXmBh = "11";//电能表清零
        //                            break;
        //                        case "事件清零记录":
        //                            strXmBh = "12";//事件清零
        //                            break;
        //                        case "掉电记录":
        //                            strXmBh = "13";//掉电
        //                            break;
        //                        case "全失压记录":
        //                            strXmBh = "14";//全失压
        //                            break;
        //                        case "事件主动上报":
        //                            strXmBh = "15";//事件主动上报
        //                            break;
        //                        case "电压不平衡":
        //                            strXmBh = "16";//电压不平衡
        //                            break;
        //                        case "过压记录":
        //                            strXmBh = "17";//过压记录
        //                            break;
        //                        case "过流记录":
        //                            strXmBh = "18";//过流记录
        //                            break;
        //                        case "过载记录":
        //                            strXmBh = "19";//过载记录
        //                            break;
        //                        case "电流逆相序":
        //                            strXmBh = "20";//电流逆相序
        //                            break;
        //                        case "潮流反向记录":
        //                            strXmBh = "21";//潮流反向记录
        //                            break;
        //                        case "功率反向记录":
        //                            strXmBh = "22";//功率反向记录
        //                            break;
        //                        case "需量超限记录":
        //                            strXmBh = "23";//需量超限记录
        //                            break;
        //                        case "功率因数超下限记录":
        //                            strXmBh = "24";//功率因数超下限记录
        //                            break;
        //                        default:
        //                            continue;


        //                    }

        //                    M_QT_EVENTLOG_MET_CONC entity = new M_QT_EVENTLOG_MET_CONC();
        //                    entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //                    entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //                    entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //                    entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号  
        //                    entity.BAR_CODE = meter.MD_BarCode;//条形码
        //                    entity.PARA_INDEX = iIndex.ToString(); //序号
        //                    entity.TEST_CATEGORIES = strXmBh;//检定项
        //                    entity.ITEM_ID = "067";//质检编码


        //                    if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //                    entity.DETECT_ITEM_POINT = iIndex.ToString();//  GetPCode("meterLogOutTest",strXmh);//检定项序号
        //                    entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //                    entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //                    entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //                    entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //                    entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //                    entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //                    entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //                    entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //                    entity.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //                    entity.TEST_CONC = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //                    entity.TEST_USER_NAME = meter.Checker1;
        //                    entity.AUDIT_USER_NAME = meter.Checker2;
        //                    entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //                    entity.TEST_REQUIRE = "";//国金
        //                    entity.TEST_CONDITION = "";//国金
        //                    entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //                    entity.HANDLE_FLAG = "0";////字符串
        //                    entity.HANDLE_DATE = "";//国金
        //                    if (STRSj.Length == 4)
        //                    {
        //                        entity.EVENT_ABOUT = "事件前" + "|" + "事件后";//事件前后（原始记录）
        //                        entity.EVENT_TYPE = STRSj[0] + "|" + STRSj1[0];//事件状态（原始记录）
        //                        entity.EVENT_START_TIME = STRSj[2] + "|" + STRSj1[2];//事件记录发生时刻（原始记录）
        //                        entity.TOTAL = STRSj[1] + "|" + STRSj1[1];//总次数（原始记录）
        //                        entity.EVENT_END_TIME = STRSj[3] + "|" + STRSj1[3];//事件记录结束时刻（原始记录）

        //                    }
        //                    else if (STRSj.Length == 3)
        //                    {
        //                        entity.EVENT_ABOUT = "事件前" + "|" + "事件后";//事件前后（原始记录）
        //                        entity.EVENT_TYPE = "" + "|" + "";//事件状态（原始记录）
        //                        entity.EVENT_START_TIME = STRSj[1] + "|" + STRSj1[1];//事件记录发生时刻（原始记录）
        //                        entity.TOTAL = STRSj[0] + "|" + STRSj1[0];//总次数（原始记录）
        //                        entity.EVENT_END_TIME = STRSj[2] + "|" + STRSj1[2];//事件记录结束时刻（原始记录）
        //                    }
        //                    else
        //                    {
        //                        entity.EVENT_ABOUT = "事件前" + "|" + "事件后";//事件前后（原始记录）
        //                        entity.EVENT_TYPE = "" + "|" + "";//事件状态（原始记录）
        //                        entity.EVENT_START_TIME = "" + "|" + "";//事件记录发生时刻（原始记录）
        //                        entity.TOTAL = "" + "|" + ""; //总次数（原始记录）
        //                        entity.EVENT_END_TIME = "" + "|" + ""; //事件记录结束时刻（原始记录）
        //                    }

        //                    sqlList.Add(entity.ToInsertString());
        //                }
        //            }
        //        }

        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 费率和时段功能 -国金
        ///// </summary>
        ///// <param name="meterInfo"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_RATETIME_MET_CONCByMt(TestMeterInfo meterInfo)
        //{

        //    int iIndex = 0;
        //    List<string> sqlList = new List<string>();

        //    string[] keys = new string[meterInfo.MeterFunctions.Keys.Count];
        //    meterInfo.MeterFunctions.Keys.CopyTo(keys, 0);

        //    int j = 0;
        //    for (int i = 0; i < keys.Length; i++)
        //    {
        //        if (keys[i].IndexOf("004") == 0)
        //            j++;

        //    }

        //    if (j == 8)//检测数据是否齐全，不齐全不上传。
        //    {
        //        //string strJl = "";
        //        //string strXmh;
        //        //string strLtsq;//两套时区表切换时间。
        //        //string strLtRsd;//两套日时段切换时间。
        //        //string strYddj;//约定冻结数据模式字。
        //        //string strDqyg;//当前正向有功电量。
        //        //string strQhq;//切换前信息。
        //        //string strQhh;//切换后信息。
        //        //string strHfh;//恢复后信息。
        //        MeterFunction MeterFunctions = meterInfo.MeterFunctions["004"];
        //        MeterFunction MeterFunctions1 = meterInfo.MeterFunctions["00401"];//两套时区表切换时间
        //        MeterFunction MeterFunctions2 = meterInfo.MeterFunctions["00402"];//两套日时段切换时间
        //        MeterFunction MeterFunctions3 = meterInfo.MeterFunctions["00403"];//约定冻结数据模式字
        //        MeterFunction MeterFunctions4 = meterInfo.MeterFunctions["00404"];//当前正向有功电量
        //        MeterFunction MeterFunctions5 = meterInfo.MeterFunctions["00405"];//切换前信息
        //        MeterFunction MeterFunctions6 = meterInfo.MeterFunctions["00406"];//切换后信息
        //        MeterFunction MeterFunctions7 = meterInfo.MeterFunctions["00407"];//恢复后信息

        //        string strJl = MeterFunctions.Value;//结论
        //        string strXmh = MeterFunctions.Name;//项目名
        //        string strLtsq = MeterFunctions1.Value;//两套时区表切换时间
        //        string strLtRsd = MeterFunctions2.Value;//两套日时段切换时间
        //        string strYddj = MeterFunctions3.Value;//约定冻结数据模式字
        //        string strDqyg = MeterFunctions4.Value;//当前正向有功电量
        //        string strQhq = MeterFunctions5.Value;//切换前信息
        //        string strQhh = MeterFunctions6.Value;//切换后信息
        //        string strHfh = MeterFunctions7.Value;//恢复后信息

        //        string itemId = "066";
        //        if (TaskId.IndexOf(itemId) == -1) return sqlList;

        //        M_QT_RATETIME_MET_CONC entity = new M_QT_RATETIME_MET_CONC();
        //        entity.BASIC_ID = meterInfo.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meterInfo.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meterInfo.BenthNo.Trim();
        //        entity.BAR_CODE = meterInfo.MD_BarCode;//条形码
        //        entity.PARA_INDEX = iIndex.ToString(); //序号
        //        entity.DETECT_ITEM_POINT = (iIndex + 1).ToString(); //序号
        //        entity.TEST_CATEGORIES = "01";//检定项:01-两套时区时段表，02-时区表，03-日时段表
        //        entity.ITEM_ID = itemId;//质检编码

        //        entity.DETECT_ITEM_POINT = (iIndex + 1).ToString();// 检定项序号
        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //        entity.ENVIRON_TEMPER = meterInfo.Temperature;//试验温度 
        //        entity.RELATIVE_HUM = meterInfo.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //        entity.TEST_CONC = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果

        //        entity.TEST_USER_NAME = meterInfo.Checker1;
        //        entity.AUDIT_USER_NAME = meterInfo.Checker2;
        //        entity.VOLT_DATE = meterInfo.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金

        //        //entity.CHANGE_DATE = strLtsq;//两套时区表切换时间
        //        //entity.CHANGE_DATE2 = strLtRsd;//两套日时段切换时间
        //        //entity.TIME_VALUE = strYddj;//约定冻结数据模式字
        //        //entity.FORWARD_POWER = strDqyg;//当前正向有功电量
        //        //entity.TIME_BEFORE = strQhq;//切换前
        //        //entity.TIME_AFTER = strQhh;//切换后
        //        //entity.REVERT_AFTER = strHfh;//恢复后
        //        sqlList.Add(entity.ToInsertString());

        //        entity.TEST_CATEGORIES = "02";//检定项:01-两套时区时段表，02-时区表，03-日时段表
        //        sqlList.Add(entity.ToInsertString());

        //        entity.TEST_CATEGORIES = "03";//检定项:01-两套时区时段表，02-时区表，03-日时段表
        //        sqlList.Add(entity.ToInsertString());

        //    }

        //    return sqlList;

        //    //return new M_QT_RATETIME_MET_CONCService().InsertSQL(entity);
        //}

        ///// <summary>
        ///// 电能量分项设置与累计存储试验-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_ELECTRIC_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    int iIndex = 0;
        //    List<string> sqlList = new List<string>();

        //    string[] Arr_ID = new string[meter.MeterFunctions.Keys.Count];
        //    meter.MeterFunctions.Keys.CopyTo(Arr_ID, 0);


        //    for (int i = 0; i < Arr_ID.Length; i++)
        //    {
        //        string _ID = Arr_ID[i];
        //        string strJl = "";
        //        string strXmh;
        //        if ((_ID.IndexOf("001") == 0 || _ID.IndexOf("006") == 0) && _ID.Length > 3)
        //        {
        //            MeterFunction MeterFunctions = meter.MeterFunctions[_ID];

        //            if (_ID.IndexOf("001") == 0)
        //                strJl = meter.MeterFunctions["001"].Value;
        //            else if (_ID.IndexOf("006") == 0)
        //                strJl = ConstHelper.合格;
        //            strXmh = MeterFunctions.Name;

        //            //string[] arrEnergy = MeterFunctions.Value.Split(',')[4].Split('|');

        //            M_QT_ELECTRIC_MET_CONC entity = new M_QT_ELECTRIC_MET_CONC();
        //            entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //            entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //            entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //            entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //            entity.BAR_CODE = meter.MD_BarCode;//条形码
        //            entity.PARA_INDEX = iIndex.ToString(); //序号
        //            entity.DETECT_ITEM_POINT = (iIndex++).ToString(); //序号
        //            int intFx = int.Parse(_ID.Substring(3, 2));
        //            PowerWay Glfx = (PowerWay)intFx;
        //            switch (Glfx)
        //            {
        //                case PowerWay.组合有功:
        //                    entity.TEST_CATEGORIES = "01";//检定项
        //                    break;
        //                case PowerWay.正向有功:
        //                    entity.TEST_CATEGORIES = "02";//检定项
        //                    break;
        //                case PowerWay.反向有功:
        //                    entity.TEST_CATEGORIES = "03";//检定项
        //                    break;
        //                case PowerWay.正向无功:
        //                    entity.TEST_CATEGORIES = "05";//检定项
        //                    break;
        //                case PowerWay.反向无功:
        //                    entity.TEST_CATEGORIES = "06";//检定项
        //                    break;
        //                case PowerWay.第一象限无功:
        //                    entity.TEST_CATEGORIES = "07";//检定项
        //                    break;
        //                case PowerWay.第二象限无功:
        //                    entity.TEST_CATEGORIES = "08";//检定项
        //                    break;
        //                case PowerWay.第三象限无功:
        //                    entity.TEST_CATEGORIES = "09";//检定项
        //                    break;
        //                case PowerWay.第四象限无功:
        //                    entity.TEST_CATEGORIES = "10";//检定项
        //                    break;
        //            }
        //            entity.ITEM_ID = "065";//质检编码


        //            if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //            entity.DETECT_ITEM_POINT = iIndex.ToString();//  GetPCode("meterStorageTest",strXmh);//检定项序号
        //            entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //            entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //            entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //            entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //            entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //            entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //            entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //            entity.DETECT_RESULT = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_CONC = strJl == ConstHelper.合格 ? "01" : "02"; //试验结果
        //            entity.TEST_USER_NAME = meter.Checker1;
        //            entity.AUDIT_USER_NAME = meter.Checker2;
        //            entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //            entity.TEST_REQUIRE = "";//国金
        //            entity.TEST_CONDITION = "";//国金
        //            entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //            entity.HANDLE_FLAG = "0";////字符串
        //            entity.HANDLE_DATE = "";//国金

        //            if (strXmh.IndexOf("读取【当前") != 0)
        //            {
        //                entity.STATE_POINT = "上一次";//结算次数
        //            }
        //            else
        //            {
        //                entity.STATE_POINT = "当前";//结算次数
        //            }

        //            string[] arrJlgn = MeterFunctions.Value.Split(',');
        //            entity.STATE_DATE = arrJlgn[0]; ;//第一结算日
        //            entity.FEAT_VALUE = arrJlgn[1];//组合有功特征字
        //            entity.FEAT_VALUE1 = arrJlgn[2];//组合有功1特征字
        //            entity.FEAT_VALUE2 = arrJlgn[3];//组合有功2特征字
        //            string[] StrType = strXmh.Split('】');
        //            if (StrType[1] == "需量")
        //            {
        //                entity.TEST_TYPE = "最大需量功能";//项目类别（计量功能、最大需量功能）
        //            }
        //            else
        //            {
        //                entity.TEST_TYPE = "计量功能";//项目类别（计量功能、最大需量功能）
        //                entity.TEST_REQUIRE = "可计量正向及各费率电量。";
        //            }

        //            entity.FEE_VALUE = arrJlgn[4];//费率电量（总|尖|峰|平|谷）


        //            sqlList.Add(entity.ToInsertString());
        //        }

        //    }
        //    return sqlList;



        //}

        ///// <summary>
        ///// 交流电压试验-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_ACVOLTAGE_MET_CONCByMt(TestMeterInfo meter)
        //{

        //    int iIndex = 0;
        //    string itemID = "003";
        //    List<string> sqlList = new List<string>();

        //    if (meter.MeterInsulations.Count == 0) return sqlList;

        //    foreach (string item in meter.MeterInsulations.Keys)
        //    {
        //        if (TaskId.IndexOf(itemID) == -1) continue;


        //        M_QT_ACVOLTAGE_MET_CONC entity = new M_QT_ACVOLTAGE_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //            EXPET_CATEG = "01",//code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,//条形码
        //            PARA_INDEX = iIndex.ToString(),//序号
        //            TEST_CATEGORIES = "01",//检定项
        //            ITEM_ID = itemID,//质检编码


        //            DETECT_ITEM_POINT = iIndex.ToString(),//检定项序号
        //            ENVIRON_TEMPER = meter.Temperature,//试验温度 
        //            RELATIVE_HUM = meter.Humidity,//试验相对湿度 
        //            DETECT_RESULT = "01",//试验结果
        //            VOLT_DATE = meter.VerifyDate,//测试时间
        //            TEST_REQUIRE = "",//国金
        //            TEST_CONDITION = "",//国金
        //            WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //            HANDLE_FLAG = "0",////字符串
        //            HANDLE_DATE = "",//国金

        //            TEST_CONC = meter.MeterInsulations[item].Result == ConstHelper.合格 ? "01" : "02", //结论

        //            TEST_USER_NAME = meter.Checker1,
        //            AUDIT_USER_NAME = meter.Checker2,
        //            VOLT_TEST_VALUE = meter.MeterInsulations[item].Voltage.ToString()//耐压值 
        //        };

        //        string strType = meter.MeterInsulations[item].Type;
        //        switch (strType.Substring(strType.Length - 1, 1))
        //        {
        //            case "0":
        //                entity.VOLT_OBJ = "01";
        //                break;
        //            case "1":
        //                entity.VOLT_OBJ = "05";
        //                break;
        //            case "2":
        //                entity.VOLT_OBJ = "03";
        //                break;
        //            default:
        //                entity.VOLT_OBJ = "01";
        //                break;

        //        }

        //        entity.LEAK_CURRENT_LIMIT = "5";//漏电流阀值 先做个假的
        //        entity.POSITION_LEAK_LIMIT = "1";//表位漏电流阀值 先做个假的
        //        entity.LEAK_CURRENT = meter.MeterInsulations[item].CurrentLost;//漏电流
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        sqlList.Add(entity.ToInsertString());

        //    }

        //    return sqlList;
        //}

        ///// <summary>
        ///// 影响量-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_IMPACT_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> LstKey = new List<string>();
        //    List<string> sqlList = new List<string>();
        //    int XBJD = 1;//先做个JD放谐波角度

        //    if (meter.MeterSpecialErrs.Count == 0) return sqlList;

        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        if (item.Substring(0, 1) == "1" || item.Substring(0, 1) == "4" || item.Substring(0, 1) == "6" ||
        //            item.IndexOf("301") == 0 || item.IndexOf("302") == 0)
        //            LstKey.Add(item);

        //    }

        //    for (int i = 0; i < LstKey.Count; i++)
        //    {
        //        MeterSpecialErr data = meter.MeterSpecialErrs[LstKey[i]];

        //        M_QT_IMPACT_MET_CONC entity = new M_QT_IMPACT_MET_CONC();
        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (i + 1).ToString();//序号
        //        entity.DETECT_ITEM_POINT = (i + 1).ToString();//序号

        //        entity.TEST_CATEGORIES = "01";//检定项
        //        string strName = data.Name;
        //        string StrA = data.AVR_VOT_A_MULTIPLE;
        //        string StrB = data.AVR_VOT_B_MULTIPLE;
        //        string StrC = data.AVR_VOT_C_MULTIPLE;
        //        string[] StrTmp = strName.Split(' ');
        //        string[] StrT = StrTmp[1].Split(',');
        //        string StrGlfx;
        //        string StrYj;
        //        string StrGlys;
        //        string StrFzdl;
        //        string StrDYTmp = "";
        //        double Wctmp = 0;//在接口计算一下误差，然后看正负

        //        switch (StrT[0])
        //        {
        //            case "P+":
        //                StrGlfx = "正向有功";
        //                break;
        //            case "P-":
        //                StrGlfx = "反向有功";
        //                break;
        //            case "Q+":
        //                StrGlfx = "正向无功";
        //                break;
        //            case "Q-":
        //                StrGlfx = "反向无功";
        //                break;
        //            case "Q1":
        //                StrGlfx = "第一象限无功";
        //                break;
        //            case "Q2":
        //                StrGlfx = "第二象限无功";
        //                break;
        //            case "Q3":
        //                StrGlfx = "第三象限无功";
        //                break;
        //            case "Q4":
        //                StrGlfx = "第四象限无功";
        //                break;
        //            default:
        //                StrGlfx = "正向有功";
        //                break;
        //        }
        //        if (StrT[1] == "合元")
        //        {
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];
        //        }
        //        else
        //        {

        //            StrGlys = StrT[1];
        //            StrFzdl = StrT[2];
        //        }
        //        if (meter.MD_WiringMode == "三相三线")
        //        {
        //            if (StrA == "0" & StrB == "0" & StrC == "1")
        //            {
        //                StrYj = "C";
        //            }
        //            else if (StrA == "1" & StrB == "0" & StrC == "0")
        //            {
        //                StrYj = "A";
        //            }
        //            else
        //            {
        //                StrYj = "AC";
        //            }
        //        }
        //        else
        //        {
        //            if (StrA == "0" & StrB == "1" & StrC == "1")
        //            {
        //                StrYj = "A";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
        //            }
        //            else if (StrA == "1" & StrB == "0" & StrC == "1")
        //            {
        //                StrYj = "B";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
        //            }
        //            else if (StrA == "1" & StrB == "1" & StrC == "0")
        //            {
        //                StrYj = "C";//HQ 2017-05-16 客户要求缺相的传缺相的相别。
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }
        //        }



        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", StrGlfx);//功率方向
        //        entity.PF = GetPCode("meterTestPowerFactor", StrGlys);//code 功率因数
        //        entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl);//负载电流 
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率

        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 

        //        if (strName.IndexOf("电压影响") != -1 && strName.IndexOf("电源电压影响") == -1)
        //        {
        //            entity.ITEM_ID = "071";//质检编码
        //            string strVolt = strName.Substring(strName.IndexOf('%') - 3, 4).Trim('%');
        //            strVolt = (100 + int.Parse(strVolt)).ToString() + "%Un";
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", strVolt);//电压

        //        }
        //        else if (strName.IndexOf("频率影响") != -1)
        //        {
        //            entity.ITEM_ID = "018";//质检编码
        //            string strFreq = strName.Substring(strName.IndexOf("Hz") - 5, 7).Trim('z').Trim('H');
        //            strFreq = (float.Parse(strFreq)).ToString();
        //            entity.FREQ = GetPCode("meterTestFreq", strFreq); //code 频率
        //        }
        //        else if (strName.IndexOf("电源电压影响") != -1)
        //        {
        //            entity.ITEM_ID = "033";//质检编码
        //            int intIndex = strName.IndexOf("响");
        //            string strVolt = strName.Substring(intIndex + 1, 4).Trim().Trim('P');

        //            strVolt = strVolt.ToString().Trim() + "Un";

        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", strVolt);//电压
        //            StrDYTmp = strVolt;

        //        }
        //        else if (strName.IndexOf("逆相序影响") != -1)
        //        {
        //            entity.ITEM_ID = "019";//质检编码

        //        }
        //        else if (strName.IndexOf("负载不平衡影响") != -1)
        //        {
        //            entity.ITEM_ID = "020";//质检编码
        //        }
        //        else if (strName.IndexOf("电压电流线路中的谐波分量影响") != -1)
        //        {

        //            if (XBJD == 1)
        //                entity.PHASE_TYPE = "0°";//相位角度
        //            else if (XBJD == 2)
        //                entity.PHASE_TYPE = "180°";//相位角度
        //            else
        //                entity.PHASE_TYPE = "0°";//相位角度

        //            entity.ITEM_ID = "021";//质检编码 
        //            entity.HARM_TYPE = "02";//谐波类型：标准代码：harmonicFype，谐波类型
        //            XBJD++;

        //        }
        //        else if (strName.IndexOf("交流电流线路中次谐波分量影响") != -1)
        //        {
        //            entity.ITEM_ID = "023";//质检编码

        //        }
        //        else if (strName.IndexOf("交流电流线路中奇次谐波分量影响") != -1)
        //        {
        //            entity.ITEM_ID = "022";//质检编码

        //        }
        //        else if (strName.IndexOf("交流电流线路中直流和偶次谐波分量影响") != -1)
        //        {
        //            entity.ITEM_ID = "024";//质检编码

        //        }
        //        else if (strName.IndexOf("自热试验") != -1)
        //        {
        //            entity.ITEM_ID = "034";//质检编码

        //        }
        //        else if (strName.IndexOf("环境温度") != -1)
        //        {

        //            entity.ITEM_ID = "016";//质检编码
        //            string[] arrPara = strName.Split(',');
        //            entity.ENVIRON_TEMPER = arrPara[4].Trim();//试验温度 
        //            entity.TEM_VARIA_ERR = data.ErrValue;
        //        }
        //        else if (strName.IndexOf("衰减震荡波抗扰度试验") != -1)
        //        {
        //            if (strName.IndexOf("共模") != -1)
        //            {
        //                entity.SIGNAL_MODE = "共模";
        //            }
        //            else
        //            {
        //                entity.SIGNAL_MODE = "差模";
        //            }

        //            entity.ITEM_ID = "042";//质检编码
        //            string[] arrPara = strName.Split(',');
        //            entity.DAMPED_RATE = arrPara[arrPara.Length - 2];//衰减振荡波速率

        //        }
        //        else if (strName.IndexOf("快速瞬变脉冲群抗扰度试验") != -1)
        //        {
        //            entity.ITEM_ID = "037";//质检编码

        //        }
        //        if (string.IsNullOrEmpty(entity.ITEM_ID)) continue;


        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;

        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = data.Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = data.Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金

        //        if (strName.IndexOf("自热试验") != -1)
        //        {
        //            #region 自热试验
        //            string[] wc1 = data.Error1.Split('|');
        //            string[] wc2 = data.Error2.Split('|');
        //            if (wc2.Length < 6 || wc1.Length <= 0) continue;

        //            entity.INF_ERR1 = wc1[0];//影响量前误差
        //            entity.AVER_ERR1 = wc1[0];//影响量前平均误差
        //            entity.INT_ERR1 = wc1[0];//影响量前化整误差

        //            entity.INF_ERR2 = wc1[0] + "|" + wc2[0] + "|" + wc2[1] + "|" + wc2[2] + "|" + wc2[3] + "|" + wc2[4];//影响量后误差，多个用‘|’分割AVER_ERR1
        //            entity.AVER_ERR2 = wc2[wc2.Length - 2];//影响量后平均误差INT_ERR
        //            entity.INT_ERR2 = wc2[wc2.Length - 1];//影响量后化整误差


        //            Wctmp = double.Parse(wc2[wc2.Length - 2]) - double.Parse(wc1[0]);
        //            if (Wctmp < 0)
        //            {
        //                if ((data.ErrValue).IndexOf("-") == -1)
        //                {
        //                    entity.INT_VARIA_ERR = "-" + double.Parse(data.ErrValue).ToString("F2");
        //                    entity.VARIA_ERR = "-" + data.ErrValue;
        //                }
        //                else
        //                {
        //                    entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
        //                }
        //            }
        //            else
        //            {
        //                if ((data.ErrValue).IndexOf("+") == -1)
        //                {
        //                    entity.INT_VARIA_ERR = "+" + double.Parse(data.ErrValue).ToString("F2");
        //                    entity.VARIA_ERR = "+" + data.ErrValue;
        //                }
        //                else
        //                {
        //                    entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
        //                }
        //            }
        //            #endregion
        //        }
        //        else
        //        {

        //            string[] strWc = data.Error1.Split('|');// meterErr.Me_chrWcMore.Split('|');+
        //            if (strWc.Length <= 3) continue;
        //            string[] strWc1 = data.Error2.Split('|');
        //            if (strWc1.Length <= 3) continue;

        //            entity.INF_ERR1 = strWc[0] + "|" + strWc[1];        //影响量前误差，多个用‘|’分割
        //            entity.INF_ERR2 = strWc1[0] + "|" + strWc1[1];      //影响量后误差，多个用‘|’分割AVER_ERR1

        //            entity.AVER_ERR1 = strWc[strWc.Length - 2];         //影响量前平均误差
        //            entity.AVER_ERR2 = strWc1[strWc1.Length - 2];       //影响量后平均误差INT_ERR1 
        //            //entity.AVER_ERR = Math.Abs(Convert.ToSingle(entity.AVER_ERR1.Trim('+')) - Convert.ToSingle(entity.AVER_ERR2.Trim('+'))).ToString(); //平均值误差值
        //            //entity.AVER_ERR = data.ErrValue;

        //            entity.INT_ERR1 = strWc[strWc.Length - 1];          //影响量前化整误差
        //            entity.INT_ERR2 = strWc1[strWc1.Length - 1];        //影响量后化整误差
        //            //entity.INT_ERR = data.ErrInt;                   //平均值化整


        //            Wctmp = double.Parse(strWc1[strWc1.Length - 2]) - double.Parse(strWc[strWc.Length - 2]);
        //            if (Wctmp < 0)
        //            {
        //                if ((data.ErrValue).IndexOf("-") == -1)
        //                {
        //                    entity.INT_VARIA_ERR = "-" + double.Parse(data.ErrValue).ToString("F2");
        //                    entity.VARIA_ERR = "-" + data.ErrValue;
        //                }
        //                else
        //                {
        //                    entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
        //                }
        //            }
        //            else
        //            {
        //                if ((data.ErrValue).IndexOf("+") == -1)
        //                {
        //                    entity.INT_VARIA_ERR = "+" + double.Parse(data.ErrValue).ToString("F2");
        //                    entity.VARIA_ERR = "+" + data.ErrValue;
        //                }
        //                else
        //                {
        //                    entity.INT_VARIA_ERR = double.Parse(data.ErrValue).ToString("F2");
        //                }
        //            }
        //        }

        //        string[] strWcx = data.ErrLimit.Split('|');

        //        if (strName.IndexOf("电源电压影响") != -1)
        //        {
        //            #region  电源电压影响
        //            int IntVolt = int.Parse(StrDYTmp.Replace("%Un", ""));
        //            if (IntVolt <= 70)
        //            {
        //                if (entity.AVER_ERR2 == "+100.0000")
        //                {
        //                    entity.VARIA_ERR = "-100";//HP 电源电压影响低于80Un%不做改变量只做误差
        //                    entity.VALUE_ABS = "-100|+10";
        //                    entity.INT_VARIA_ERR = "-100";//HP 电源电压影响低于80Un%不做改变量只做误差
        //                    entity.INT_ERR2 = "-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
        //                    entity.AVER_ERR2 = "-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
        //                    entity.INF_ERR2 = "-100|-100";//HP 电源电压影响未启动误差值应为-100，接口处修改
        //                }
        //                else
        //                {
        //                    string[] strWc1 = data.Error2.Split('|');

        //                    if ((strWc1[strWc1.Length - 2]).IndexOf("+") == -1)
        //                    {
        //                        entity.VARIA_ERR = "-" + double.Parse(strWc1[strWc1.Length - 2]).ToString();
        //                        entity.INT_VARIA_ERR = "-" + double.Parse(strWc1[strWc1.Length - 2]).ToString("F2");//HP 变差化整值重新计算
        //                    }
        //                    else
        //                    {
        //                        entity.VARIA_ERR = "+" + double.Parse(strWc1[strWc1.Length - 2]).ToString();
        //                        entity.INT_VARIA_ERR = "+" + double.Parse(strWc1[strWc1.Length - 2]).ToString("F2");//HP 变差化整值重新计算
        //                    }
        //                    entity.VALUE_ABS = "-100|+10";
        //                }
        //            }
        //            else
        //            {
        //                string[] strWc = data.Error1.Split('|');
        //                string[] strWc1 = data.Error2.Split('|');
        //                Wctmp = double.Parse(strWc1[strWc1.Length - 2]) - double.Parse(strWc[strWc.Length - 2]);
        //                if (Wctmp > 0)
        //                {
        //                    entity.INT_VARIA_ERR = "+" + Wctmp.ToString("F2");
        //                    entity.VARIA_ERR = "+" + Wctmp.ToString("F4");
        //                }
        //                else
        //                {
        //                    entity.INT_VARIA_ERR = Wctmp.ToString("F2");
        //                    entity.VARIA_ERR = Wctmp.ToString("F4");
        //                }
        //                entity.VALUE_ABS = "±" + (data.BPHUpLimit).ToString(); //strWcx[0].Replace('+', '±');//变差限
        //            }
        //            #endregion
        //        }
        //        else
        //        {
        //            entity.VALUE_ABS = "±" + Convert.ToSingle(data.BPHUpLimit).ToString("F1");  //strWcx[0].Replace('+', '±');//变差限
        //        }

        //        sqlList.Add(entity.ToInsertString());
        //    }

        //    //短时过电压试验
        //    if (ConfigHelper.Instance.EquipmentType == "单相台")
        //    {
        //        M_QT_IMPACT_MET_CONC entity = new M_QT_IMPACT_MET_CONC();
        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (LstKey.Count + 1).ToString();//序号
        //        entity.DETECT_ITEM_POINT = (LstKey.Count + 1).ToString();//序号

        //        entity.TEST_CATEGORIES = "01";//检定项

        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "5");//负载电流 
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率

        //        entity.INF_ERR1 = "0.000|0.000";    //影响量前误差，多个用‘|’分割
        //        entity.INF_ERR2 = "0.000|0.000";    //影响量后误差，多个用‘|’分割AVER_ERR1

        //        entity.AVER_ERR1 = "0.000";         //影响量前平均误差
        //        entity.AVER_ERR2 = "0.000";         //影响量后平均误差INT_ERR1 

        //        entity.INT_ERR1 = "0.00";           //影响量前化整误差
        //        entity.INT_ERR2 = "0.00";           //影响量后化整误差
        //        entity.VARIA_ERR = "0.00";
        //        entity.INT_VARIA_ERR = "0.0";

        //        entity.ENVIRON_TEMPER = meter.Temperature;  //试验温度 
        //        entity.ITEM_ID = "072";                     //质检编码
        //        entity.RELATIVE_HUM = meter.Humidity;       //试验相对湿度 
        //        entity.DETECT_RESULT = "01";                //试验结果
        //        entity.TEST_CONC = "01";                    //试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;        //测试时间
        //        entity.TEST_REQUIRE = "";                   //国金
        //        entity.TEST_CONDITION = "";                 //国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";                   //字符串
        //        entity.HANDLE_DATE = "";                    //国金

        //    }

        //    return sqlList;
        //}

        ///// <summary>
        ///// 气候影响-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>           
        //private List<string> GetM_QT_CLIMATEACT_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> LstKey = new List<string>();
        //    List<string> sqlList = new List<string>();

        //    if (meter.MeterSpecialErrs.Count == 0) return sqlList;

        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        if (item.Substring(0, 1) == "2")
        //            LstKey.Add(item);
        //    }

        //    for (int i = 0; i < LstKey.Count; i++)
        //    {
        //        M_QT_CLIMATEACT_MET_CONC entity = new M_QT_CLIMATEACT_MET_CONC();

        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = "01";//序号

        //        string strName = meter.MeterSpecialErrs[LstKey[i]].Name;
        //        string[] StrTmp = strName.Split(' ');
        //        string[] StrT = StrTmp[1].Split(',');
        //        string StrGlfx;
        //        string StrYj;
        //        string StrGlys;
        //        string StrFzdl;
        //        switch (StrT[0])
        //        {
        //            case "P+":
        //                StrGlfx = "正向有功";
        //                break;
        //            case "P-":
        //                StrGlfx = "反向有功";
        //                break;
        //            case "Q+":
        //                StrGlfx = "正向无功";
        //                break;
        //            case "Q-":
        //                StrGlfx = "反向无功";
        //                break;
        //            case "Q1":
        //                StrGlfx = "第一象限无功";
        //                break;
        //            case "Q2":
        //                StrGlfx = "第二象限无功";
        //                break;
        //            case "Q3":
        //                StrGlfx = "第三象限无功";
        //                break;
        //            case "Q4":
        //                StrGlfx = "第四象限无功";
        //                break;
        //            default:
        //                StrGlfx = "正向有功";
        //                break;
        //        }
        //        if (StrT[1] == "合元")
        //        {


        //            switch (StrT[1])
        //            {

        //                case "合元":
        //                    if (meter.MD_WiringMode == "三相三线")
        //                    {
        //                        StrYj = "AC";
        //                    }
        //                    else
        //                    {
        //                        StrYj = "ABC";
        //                    }
        //                    break;
        //                case "A元":
        //                    StrYj = "A";
        //                    break;
        //                case "B元":
        //                    StrYj = "B";
        //                    break;
        //                case "C元":
        //                    StrYj = "C";
        //                    break;
        //                default:
        //                    StrYj = "ABC";
        //                    break;
        //            }
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];
        //        }
        //        else
        //        {
        //            if (meter.MD_WiringMode == "三相三线")
        //            {
        //                StrYj = "AC";
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];

        //        }
        //        if (StrYj == "ABC" || StrYj == "AC")
        //        {
        //            entity.TEST_CATEGORIES = "02";
        //        }
        //        else
        //        {
        //            entity.TEST_CATEGORIES = "03";
        //        }

        //        if (StrT[1] == "合元")
        //        {
        //            if (meter.MD_WiringMode == "三相三线")
        //            {
        //                StrYj = "AC";
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }

        //        }
        //        if (StrT[1] == "A元")
        //        {
        //            StrYj = "A";
        //        }
        //        if (StrT[1] == "B元")
        //        {
        //            StrYj = "B";
        //        }
        //        if (StrT[1] == "C元")
        //        {
        //            StrYj = "C";
        //        }

        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", StrGlfx);//功率方向
        //        entity.PF = GetPCode("meterTestPowerFactor", StrGlys);//code 功率因数
        //        entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl);//负载电流 
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        if (strName.IndexOf("高温试验") != -1)
        //        {
        //            entity.ITEM_ID = "045";//质检编码
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "115%Un");//电压
        //        }
        //        if (strName.IndexOf("低温试验") != -1)
        //        {
        //            entity.ITEM_ID = "046";//质检编码
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "110%Un");//电压
        //        }
        //        if (strName.IndexOf("交变湿热试验") != -1)
        //        {
        //            entity.ITEM_ID = "047";//质检编码
        //            entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "90%Un");//电压
        //        }


        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;
        //        entity.ENVIRON_TEMPER = StrT[4].Trim();//试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金
        //        string[] strWc = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');// meterErr.Me_chrWcMore.Split('|');
        //        if (strWc.Length <= 3) continue;
        //        entity.ERROR = strWc[0] + "|" + strWc[1];//误差|
        //        entity.AVE_ERR = strWc[2];//平均值
        //        entity.INT_CONVERT_ERR = strWc[3];//化整值
        //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');
        //        entity.ERR_ABS = strWcx[0].Replace('+', '±');//变差限
        //        string[] strWc1 = meter.MeterSpecialErrs[LstKey[i]].ErrValue.Split('|');
        //        if (strWc1.Length == 2)
        //        {
        //            entity.INT_VARIA_ERR = strWc1[0];//相对误差（原始记录）TEM_VARIA_ERR
        //            entity.TEM_VARIA_ERR = strWc1[1];
        //        }
        //        else
        //        {
        //            entity.INT_VARIA_ERR = "0.00";//相对误差（原始记录）
        //            entity.TEM_VARIA_ERR = "0.00";
        //        }

        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 功耗-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_POWER_MET_CONCByMt(TestMeterInfo meter)
        //{

        //    List<string> sqlList = new List<string>();
        //    string[] Arr_ID = new string[meter.MeterPowers.Keys.Count];
        //    if (meter.MeterPowers.Count == 0) return sqlList;
        //    M_QT_POWER_MET_CONC entity = new M_QT_POWER_MET_CONC();
        //    foreach (string item in meter.MeterPowers.Keys)
        //    {
        //        if (TaskId.IndexOf("032") == -1) continue;
        //        MeterPower meterpower = meter.MeterPowers[item];

        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码

        //        if (item == "117110")
        //        {
        //            entity.TEST_CATEGORIES = "01";//非通讯状态
        //            entity.ITEM_ID = "032";//质检编码 
        //        }
        //        else
        //        {
        //            entity.TEST_CATEGORIES = "02";//通讯状态
        //            entity.ITEM_ID = "10008";//质检编码 
        //        }



        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", "正向有功");//功率方向
        //        entity.IABC = GetPCode("currentPhaseCode", "ABC");//电流相别  
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", "无");//负载电流
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.PF = GetPCode("meterTestPowerFactor", "1.0");//code 功率因数
        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.TEST_CONC = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金
        //        if (item == "117110")//非通讯状态
        //        {
        //            if (meter.MD_WiringMode == "单相")//单相
        //            {
        //                for (int i = 1; i < 4; i++)
        //                {
        //                    entity.PARA_INDEX = i.ToString();//序号
        //                    entity.DETECT_ITEM_POINT = i.ToString();//检定项序号

        //                    switch (i)
        //                    {

        //                        case 1:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.IaPowerS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                        case 2:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UaPowerS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                        case 3:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UaPowerP.ToString().Trim()).ToString("f2") + "W"; //功率，多个用‘|’分割
        //                            break;

        //                        default:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.IaPowerS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                    }
        //                    sqlList.Add(entity.ToInsertString());
        //                }
        //            }
        //            else if (meter.MD_WiringMode == "三相四线")//三相四线
        //            {
        //                for (int i = 1; i < 4; i++)
        //                {
        //                    entity.PARA_INDEX = i.ToString();//序号
        //                    entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                    switch (i)
        //                    {
        //                        case 1:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            double power1 = double.Parse(meterpower.IaPowerS.ToString().Trim());
        //                            power1 += double.Parse(meterpower.IbPowerS.ToString().Trim());
        //                            power1 += double.Parse(meterpower.IcPowerS.ToString().Trim());

        //                            entity.ACT_POWER = power1.ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                        case 2:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            double power2 = double.Parse(meterpower.UaPowerS.ToString().Trim());
        //                            power2 += double.Parse(meterpower.UbPowerS.ToString().Trim());
        //                            power2 += double.Parse(meterpower.UcPowerS.ToString().Trim());
        //                            entity.ACT_POWER = power2.ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                        case 3:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            double power3 = double.Parse(meterpower.UaPowerP.ToString().Trim());
        //                            power3 += double.Parse(meterpower.UbPowerP.ToString().Trim());
        //                            power3 += double.Parse(meterpower.UcPowerP.ToString().Trim());
        //                            entity.ACT_POWER = power3.ToString("f2") + "W"; //功率，多个用‘|’分割
        //                            break;
        //                            //case 4:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","B");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ib_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 5:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","B");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ub_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 6:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","B");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ub_ReactiveP.ToString().Trim()).ToString("f2") + "W"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 7:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","C");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ic_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 8:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","C");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Uc_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 9:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","C");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Uc_ReactiveP.ToString().Trim()).ToString("f2") + "W"; //功率，多个用‘|’分割
        //                            //    break;

        //                            //default:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","A");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ia_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                    }

        //                    sqlList.Add(entity.ToInsertString());


        //                }
        //            }
        //            else if (meter.MD_WiringMode == "三相三线")//三相三线
        //            {
        //                for (int i = 1; i < 4; i++)
        //                {
        //                    entity.PARA_INDEX = i.ToString();//序号
        //                    entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                    switch (i)
        //                    {
        //                        case 1:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            double power1 = double.Parse(meterpower.IaPowerS.ToString().Trim());
        //                            power1 += double.Parse(meterpower.IcPowerS.ToString().Trim());
        //                            entity.ACT_POWER = power1.ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                        case 2:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            double power2 = double.Parse(meterpower.UaPowerS.ToString().Trim());
        //                            power2 += double.Parse(meterpower.UcPowerS.ToString().Trim());
        //                            entity.ACT_POWER = power2.ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            break;
        //                        case 3:
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            double power3 = double.Parse(meterpower.UaPowerP.ToString().Trim());
        //                            power3 += double.Parse(meterpower.UcPowerP.ToString().Trim());
        //                            entity.ACT_POWER = power3.ToString("f2") + "W"; //功率，多个用‘|’分割
        //                            break;
        //                            //case 4:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","C");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ic_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 5:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","C");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Uc_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                            //case 6:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","C");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Uc_ReactiveP.ToString().Trim()).ToString("f2") + "W"; //功率，多个用‘|’分割
        //                            //    break;


        //                            //default:
        //                            //    entity.IABC =  GetPCode("currentPhaseCode","A");//电流相别 
        //                            //    entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == Const.CTG_HeGe ? "01" : "02";//试验结果
        //                            //    entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            //    entity.TEST_LINE = "02";//试验线路 01电压线路 02电流线路
        //                            //    entity.POWER_TYPE = "02";//功率类型01是有功功率  02是视在功率
        //                            //    entity.ACT_POWER = double.Parse(meterpower.Md_Ia_ReactiveS.ToString().Trim()).ToString("f2") + "VA"; //功率，多个用‘|’分割
        //                            //    break;
        //                    }

        //                    sqlList.Add(entity.ToInsertString());
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (meter.MD_WiringMode == "单相")//单相
        //            {
        //                entity.PARA_INDEX = "1";//序号
        //                entity.DETECT_ITEM_POINT = "1";//检定项序号
        //                entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                entity.ACT_POWER = double.Parse(meterpower.UaPowerS.ToString().Trim()).ToString("f2"); //功率，多个用‘|’分割
        //                sqlList.Add(entity.ToInsertString());

        //            }
        //            else if (meter.MD_WiringMode == "三相三线")//三相三线
        //            {
        //                for (int i = 1; i < 3; i++)
        //                {
        //                    switch (i)
        //                    {
        //                        case 1:
        //                            entity.PARA_INDEX = i.ToString();//序号
        //                            entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UaPowerS.ToString().Trim()).ToString("f2"); //功率，多个用‘|’分割
        //                            break;
        //                        case 2:
        //                            entity.PARA_INDEX = i.ToString();//序号
        //                            entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                            entity.IABC = GetPCode("currentPhaseCode", "C");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UaPowerS.ToString().Trim()).ToString("f2"); //功率，多个用‘|’分割
        //                            break;
        //                    }
        //                    sqlList.Add(entity.ToInsertString());
        //                }

        //            }
        //            else if (meter.MD_WiringMode == "三相四线")//三相四线
        //            {
        //                for (int i = 1; i < 4; i++)
        //                {
        //                    switch (i)
        //                    {
        //                        case 1:
        //                            entity.PARA_INDEX = i.ToString();//序号
        //                            entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                            entity.IABC = GetPCode("currentPhaseCode", "A");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UaPowerS.ToString().Trim()).ToString("f2"); //功率，多个用‘|’分割
        //                            break;
        //                        case 2:
        //                            entity.PARA_INDEX = i.ToString();//序号
        //                            entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                            entity.IABC = GetPCode("currentPhaseCode", "B");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UbPowerS.ToString().Trim()).ToString("f2"); //功率，多个用‘|’分割
        //                            break;
        //                        case 3:
        //                            entity.PARA_INDEX = i.ToString();//序号
        //                            entity.DETECT_ITEM_POINT = i.ToString();//检定项序号
        //                            entity.IABC = GetPCode("currentPhaseCode", "C");//电流相别 
        //                            entity.DETECT_RESULT = meterpower.Mgh_chrJL.ToString().Trim().Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //                            entity.ACT_POWER_ERR = meter.MD_Grane;//功率误差限
        //                            entity.TEST_LINE = "01";//试验线路 01电压线路 02电流线路
        //                            entity.POWER_TYPE = "01";//功率类型01是有功功率  02是视在功率
        //                            entity.ACT_POWER = double.Parse(meterpower.UcPowerS.ToString().Trim()).ToString("f2"); //功率，多个用‘|’分割
        //                            break;
        //                    }
        //                    sqlList.Add(entity.ToInsertString());
        //                }

        //            }

        //        }

        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 振动影响，盐雾-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_VIBRATION_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> sqlList = new List<string>();
        //    if (meter.MeterSpecialErrs.Count == 0) return sqlList;

        //    List<string> LstKey = new List<string>();
        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        if (item.IndexOf("500") == 0)
        //            LstKey.Add(item);
        //        else if (item.IndexOf("502") == 0)
        //            LstKey.Add(item);
        //    }

        //    for (int i = 0; i < LstKey.Count; i++)
        //    {
        //        string prjName = meter.MeterSpecialErrs[LstKey[i]].Name;
        //        string[] tmp = prjName.Split(' ');
        //        string[] arr = tmp[1].Split(',');

        //        string glfx = "正向有功";
        //        if (arr[0] == "P+")
        //            glfx = "正向有功";
        //        if (arr[0] == "P-")
        //            glfx = "反向有功";
        //        else if (arr[0] == "Q+")
        //            glfx = "正向无功";
        //        else if (arr[0] == "Q-")
        //            glfx = "反向无功";
        //        else if (arr[0] == "Q1")
        //            glfx = "第一象限无功";
        //        else if (arr[0] == "Q2")
        //            glfx = "第二象限无功";
        //        else if (arr[0] == "Q3")
        //            glfx = "第三象限无功";
        //        else if (arr[0] == "Q4")
        //            glfx = "第四象限无功";

        //        string yj = meter.MD_WiringMode == "三相三线" ? "AC" : "ABC";
        //        if (arr[1] == "A元")
        //            yj = "A";
        //        if (arr[1] == "B元")
        //            yj = "B";
        //        if (arr[1] == "C元")
        //            yj = "C";

        //        string[] wcArr = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');       //误差值
        //        if (wcArr.Length <= 3) continue;

        //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');    //误差限

        //        string itemID = "";
        //        if (LstKey[i].IndexOf("500") == 0)
        //            itemID = "049";//质检编码
        //        else if (LstKey[i].IndexOf("502") == 0)
        //            itemID = "062";//质检编码
        //        if (TaskId.IndexOf(itemID) == -1) continue;


        //        M_QT_VIBRATION_MET_CONC entity = new M_QT_VIBRATION_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(), //19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,       //检定任务单编号
        //            EXPET_CATEG = "01",                      //code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,            //条形码
        //            PARA_INDEX = (i + 1).ToString(),         //序号
        //            DETECT_ITEM_POINT = (i + 1).ToString(),  //序号
        //            ENVIRON_TEMPER = meter.Temperature,  //试验温度 
        //            TEST_CATEGORIES = "02",
        //            ITEM_ID = itemID,//质检编码

        //            BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", glfx),   //功率方向
        //            PF = GetPCode("meterTestPowerFactor", arr[2]),       //code 功率因数
        //            IABC = GetPCode("currentPhaseCode", yj),             //电流相别
        //            LOAD_CURRENT = GetPCode("meterTestCurLoad", arr[3]), //负载电流 
        //            FREQ = GetPCode("meterTestFreq", "50"),              //code 频率
        //            LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un"),  //电压
        //            RELATIVE_HUM = meter.Humidity,                                                               //试验相对湿度 
        //            DETECT_RESULT = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_CONC = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02",//试验结果
        //            TEST_USER_NAME = meter.Checker1,
        //            AUDIT_USER_NAME = meter.Checker2,
        //            VOLT_DATE = meter.VerifyDate,//测试时间
        //            TEST_REQUIRE = "",
        //            TEST_CONDITION = "",
        //            WRITE_DATE = DateTime.Now.ToString(),//写入时间
        //            HANDLE_FLAG = "0",//字符串
        //            HANDLE_DATE = "",

        //            ERROR = wcArr[0] + "|" + wcArr[1] + "|" + wcArr[2],
        //            ERROR1 = wcArr[0],
        //            ERROR2 = wcArr[1],
        //            AVE_ERR = wcArr[wcArr.Length - 2],
        //            INT_CONVERT_ERR = wcArr[wcArr.Length - 1],
        //            ERR_ABS = "±" + (meter.MeterSpecialErrs[LstKey[i]].BPHUpLimit).ToString()
        //        };

        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 冲击试验-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_IMPACTTEST_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> LstKey = new List<string>();
        //    List<string> sqlList = new List<string>();

        //    if (meter.MeterSpecialErrs.Count == 0) return sqlList;

        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        if (item.IndexOf("501") == 0)
        //            LstKey.Add(item);
        //    }

        //    for (int i = 0; i < LstKey.Count; i++)
        //    {
        //        if (TaskId.IndexOf("050") == -1) continue;
        //        M_QT_IMPACTTEST_MET_CONC entity = new M_QT_IMPACTTEST_MET_CONC();

        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (i + 1).ToString();//序号
        //        entity.DETECT_ITEM_POINT = (i + 1).ToString();//序号
        //        entity.TEST_CATEGORIES = "01";//检定项
        //        entity.ITEM_ID = "050";//质检编码


        //        if (TaskId.IndexOf(entity.ITEM_ID) == -1) continue;

        //        string strName = meter.MeterSpecialErrs[LstKey[i]].Name;
        //        string[] StrTmp = strName.Split(' ');
        //        string[] StrT = StrTmp[1].Split(',');
        //        string StrGlfx;
        //        string StrYj;
        //        string StrGlys;
        //        string StrFzdl;
        //        switch (StrT[0])
        //        {
        //            case "P+":
        //                StrGlfx = "正向有功";
        //                break;
        //            case "P-":
        //                StrGlfx = "反向有功";
        //                break;
        //            case "Q+":
        //                StrGlfx = "正向无功";
        //                break;
        //            case "Q-":
        //                StrGlfx = "反向无功";
        //                break;
        //            case "Q1":
        //                StrGlfx = "第一象限无功";
        //                break;
        //            case "Q2":
        //                StrGlfx = "第二象限无功";
        //                break;
        //            case "Q3":
        //                StrGlfx = "第三象限无功";
        //                break;
        //            case "Q4":
        //                StrGlfx = "第四象限无功";
        //                break;
        //            default:
        //                StrGlfx = "正向有功";
        //                break;
        //        }
        //        if (StrT[1] == "合元")
        //        {


        //            switch (StrT[1])
        //            {

        //                case "合元":
        //                    if (meter.MD_WiringMode == "三相三线")
        //                    {
        //                        StrYj = "AC";
        //                    }
        //                    else
        //                    {
        //                        StrYj = "ABC";
        //                    }
        //                    break;
        //                case "A元":
        //                    StrYj = "A";
        //                    break;
        //                case "B元":
        //                    StrYj = "B";
        //                    break;
        //                case "C元":
        //                    StrYj = "C";
        //                    break;
        //                default:
        //                    StrYj = "ABC";
        //                    break;
        //            }
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];
        //        }
        //        else
        //        {
        //            if (meter.MD_WiringMode == "三相三线")
        //            {
        //                StrYj = "AC";
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];

        //        }
        //        if (StrYj == "ABC" || StrYj == "AC")
        //        {
        //            entity.TEST_CATEGORIES = "02";
        //        }
        //        else
        //        {
        //            entity.TEST_CATEGORIES = "03";
        //        }

        //        if (StrT[1] == "合元")
        //        {
        //            if (meter.MD_WiringMode == "三相三线")
        //            {
        //                StrYj = "AC";
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }
        //        }
        //        if (StrT[1] == "A元")
        //        {
        //            StrYj = "A";
        //        }
        //        if (StrT[1] == "B元")
        //        {
        //            StrYj = "B";
        //        }
        //        if (StrT[1] == "C元")
        //        {
        //            StrYj = "C";
        //        }

        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", StrGlfx);//功率方向
        //        entity.PF = GetPCode("meterTestPowerFactor", StrGlys);//code 功率因数
        //        entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl);//负载电流 
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金
        //        string[] strWc = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');// meterErr.Me_chrWcMore.Split('|');
        //        if (strWc.Length <= 3) continue;
        //        entity.ERROR = strWc[0] + "|" + strWc[1] + "|" + strWc[2];
        //        entity.ERROR1 = strWc[0];
        //        entity.ERROR2 = strWc[1];
        //        entity.AVE_ERR = strWc[strWc.Length - 2];
        //        entity.INT_CONVERT_ERR = strWc[strWc.Length - 1];
        //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');
        //        entity.ERR_ABS = "±" + (meter.MeterSpecialErrs[LstKey[i]].BPHUpLimit).ToString();//变差限

        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 射频电磁场抗扰度试验-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_RFINDUCTION_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> LstKey = new List<string>();
        //    List<string> sqlList = new List<string>();

        //    if (meter.MeterSpecialErrs.Count == 0) return sqlList;

        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        if (item.IndexOf("300") == 0)
        //            LstKey.Add(item);
        //    }

        //    for (int i = 0; i < LstKey.Count; i++)
        //    {
        //        if (TaskId.IndexOf("038") == -1) continue;
        //        M_QT_RFINDUCTION_MET_CONC entity = new M_QT_RFINDUCTION_MET_CONC
        //        {
        //            BASIC_ID = meter.Meter_ID.ToString(),//19位ID  基本信息标识
        //            DETECT_TASK_NO = meter.MD_TaskNo,//检定任务单编号
        //            EXPET_CATEG = "01",//code 试品类别 电能表
        //            DETECT_EQUIP_NO = meter.BenthNo.Trim(),//检定设备编号 
        //            BAR_CODE = meter.MD_BarCode,//条形码
        //            PARA_INDEX = (i + 1).ToString(),//序号
        //            DETECT_ITEM_POINT = (i + 1).ToString(),//序号
        //            TEST_CATEGORIES = "01",//检定项
        //            ITEM_ID = "038"//质检编码
        //        };




        //        string strName = meter.MeterSpecialErrs[LstKey[i]].Name;
        //        string[] StrTmp = strName.Split(' ');
        //        string[] StrT = StrTmp[1].Split(',');
        //        string StrGlfx;
        //        string StrYj;
        //        string StrGlys;
        //        string StrFzdl;
        //        switch (StrT[0])
        //        {
        //            case "P+":
        //                StrGlfx = "正向有功";
        //                break;
        //            case "P-":
        //                StrGlfx = "反向有功";
        //                break;
        //            case "Q+":
        //                StrGlfx = "正向无功";
        //                break;
        //            case "Q-":
        //                StrGlfx = "反向无功";
        //                break;
        //            case "Q1":
        //                StrGlfx = "第一象限无功";
        //                break;
        //            case "Q2":
        //                StrGlfx = "第二象限无功";
        //                break;
        //            case "Q3":
        //                StrGlfx = "第三象限无功";
        //                break;
        //            case "Q4":
        //                StrGlfx = "第四象限无功";
        //                break;
        //            default:
        //                StrGlfx = "正向有功";
        //                break;
        //        }
        //        if (StrT[1] == "合元")
        //        {


        //            switch (StrT[1])
        //            {

        //                case "合元":
        //                    if (meter.MD_WiringMode == "三相三线")
        //                    {
        //                        StrYj = "AC";
        //                    }
        //                    else
        //                    {
        //                        StrYj = "ABC";
        //                    }
        //                    break;
        //                case "A元":
        //                    StrYj = "A";
        //                    break;
        //                case "B元":
        //                    StrYj = "B";
        //                    break;
        //                case "C元":
        //                    StrYj = "C";
        //                    break;
        //                default:
        //                    StrYj = "ABC";
        //                    break;
        //            }
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];
        //        }
        //        else
        //        {
        //            if (meter.MD_WiringMode == "三相三线")
        //            {
        //                StrYj = "AC";
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }
        //            StrGlys = StrT[1];
        //            StrFzdl = StrT[2];

        //        }

        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", StrGlfx);//功率方向
        //        entity.PF = GetPCode("meterTestPowerFactor", StrGlys);//code 功率因数
        //        entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl);//负载电流 
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金
        //        string[] strWc = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');// meterErr.Me_chrWcMore.Split('|');
        //        entity.RF_FIELD = StrT[StrT.Length - 1];//
        //        if (strWc.Length <= 3) continue;
        //        entity.ERROR1 = strWc[0];
        //        entity.ERROR2 = strWc[1];
        //        entity.AVE_ERR = strWc[2];
        //        entity.INT_CONVERT_ERR = strWc[3];
        //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');
        //        entity.ERR_ABS = strWcx[0].Replace('+', '±');//变差限
        //        string[] strWc1 = meter.MeterSpecialErrs[LstKey[i]].ErrValue.Split('|');
        //        entity.INT_VARIA_ERR = strWc1[0];//相对误差（原始记录）
        //        entity.ERROR = strWc[0] + "|" + strWc[1];//误差|
        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    return sqlList;
        //}

        ///// <summary>
        ///// 射频场感应传导骚扰抗扰度试验-国金
        ///// </summary>
        ///// <param name="meter"></param>
        ///// <returns></returns>
        //private List<string> GetM_QT_RFINDUCDIS_MET_CONCByMt(TestMeterInfo meter)
        //{
        //    List<string> LstKey = new List<string>();
        //    List<string> sqlList = new List<string>();

        //    if (meter.MeterSpecialErrs.Count == 0) return sqlList;

        //    foreach (string item in meter.MeterSpecialErrs.Keys)
        //    {
        //        if (item.IndexOf("100107") == 0)
        //            LstKey.Add(item);
        //    }

        //    for (int i = 0; i < LstKey.Count; i++)
        //    {
        //        if (TaskId.IndexOf("039") == -1) continue;
        //        M_QT_RFINDUCDIS_MET_CONC entity = new M_QT_RFINDUCDIS_MET_CONC();

        //        entity.BASIC_ID = meter.Meter_ID.ToString();//19位ID  基本信息标识
        //        entity.DETECT_TASK_NO = meter.MD_TaskNo;//检定任务单编号
        //        entity.EXPET_CATEG = "01";//code 试品类别 电能表
        //        entity.DETECT_EQUIP_NO = meter.BenthNo.Trim();//检定设备编号 
        //        entity.BAR_CODE = meter.MD_BarCode;//条形码
        //        entity.PARA_INDEX = (i + 1).ToString();//序号
        //        entity.DETECT_ITEM_POINT = (i + 1).ToString();//序号
        //        entity.TEST_CATEGORIES = "01";//检定项
        //        entity.ITEM_ID = "039";//质检编码




        //        string strName = meter.MeterSpecialErrs[LstKey[i]].Name;
        //        string[] StrTmp = strName.Split(' ');
        //        string[] StrT = StrTmp[1].Split(',');
        //        string StrGlfx;
        //        string StrYj;
        //        string StrGlys;
        //        string StrFzdl;
        //        switch (StrT[0])
        //        {
        //            case "P+":
        //                StrGlfx = "正向有功";
        //                break;
        //            case "P-":
        //                StrGlfx = "反向有功";
        //                break;
        //            case "Q+":
        //                StrGlfx = "正向无功";
        //                break;
        //            case "Q-":
        //                StrGlfx = "反向无功";
        //                break;
        //            case "Q1":
        //                StrGlfx = "第一象限无功";
        //                break;
        //            case "Q2":
        //                StrGlfx = "第二象限无功";
        //                break;
        //            case "Q3":
        //                StrGlfx = "第三象限无功";
        //                break;
        //            case "Q4":
        //                StrGlfx = "第四象限无功";
        //                break;
        //            default:
        //                StrGlfx = "正向有功";
        //                break;
        //        }
        //        if (StrT[1] == "合元")
        //        {


        //            switch (StrT[1])
        //            {

        //                case "合元":
        //                    if (meter.MD_WiringMode == "三相三线")
        //                    {
        //                        StrYj = "AC";
        //                    }
        //                    else
        //                    {
        //                        StrYj = "ABC";
        //                    }
        //                    break;
        //                case "A元":
        //                    StrYj = "A";
        //                    break;
        //                case "B元":
        //                    StrYj = "B";
        //                    break;
        //                case "C元":
        //                    StrYj = "C";
        //                    break;
        //                default:
        //                    StrYj = "ABC";
        //                    break;
        //            }
        //            StrGlys = StrT[2];
        //            StrFzdl = StrT[3];
        //        }
        //        else
        //        {
        //            if (meter.MD_WiringMode == "三相三线")
        //            {
        //                StrYj = "AC";
        //            }
        //            else
        //            {
        //                StrYj = "ABC";
        //            }
        //            StrGlys = StrT[1];
        //            StrFzdl = StrT[2];

        //        }

        //        entity.BOTH_WAY_POWER_FLAG = GetPCode("powerFlag", StrGlfx);//功率方向
        //        entity.PF = GetPCode("meterTestPowerFactor", StrGlys);//code 功率因数
        //        entity.IABC = GetPCode("currentPhaseCode", StrYj);//电流相别
        //        entity.LOAD_CURRENT = GetPCode("meterTestCurLoad", StrFzdl);//负载电流 
        //        entity.FREQ = GetPCode("meterTestFreq", "50"); //code 频率
        //        entity.LOAD_VOLTAGE = GetPCode("meterTestVolt", "100%Un");//电压
        //        entity.ENVIRON_TEMPER = meter.Temperature;//试验温度 
        //        entity.RELATIVE_HUM = meter.Humidity;//试验相对湿度 
        //        entity.DETECT_RESULT = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_CONC = meter.MeterSpecialErrs[LstKey[i]].Result.Trim() == ConstHelper.合格 ? "01" : "02";//试验结果
        //        entity.TEST_USER_NAME = meter.Checker1;
        //        entity.AUDIT_USER_NAME = meter.Checker2;
        //        entity.VOLT_DATE = meter.VerifyDate;//测试时间
        //        entity.TEST_REQUIRE = "";//国金
        //        entity.TEST_CONDITION = "";//国金
        //        entity.WRITE_DATE = DateTime.Now.ToString();//写入时间
        //        entity.HANDLE_FLAG = "0";////字符串
        //        entity.HANDLE_DATE = "";//国金
        //        entity.RF_FIELDFR = StrT[StrT.Length - 1];//
        //        string[] strWc = meter.MeterSpecialErrs[LstKey[i]].Error2.Split('|');// meterErr.Me_chrWcMore.Split('|');
        //        if (strWc.Length <= 3) continue;
        //        entity.ERROR1 = strWc[0];
        //        entity.ERROR2 = strWc[1];
        //        entity.AVE_ERR = strWc[2];
        //        entity.INT_CONVERT_ERR = strWc[3];
        //        string[] strWcx = meter.MeterSpecialErrs[LstKey[i]].ErrLimit.Split('|');
        //        entity.ERR_ABS = strWcx[0].Replace('+', '±');//变差限
        //        string[] strWc1 = meter.MeterSpecialErrs[LstKey[i]].ErrValue.Split('|');
        //        entity.INT_VARIA_ERR = strWc1[0];//相对误差（原始记录）
        //        entity.ERROR = strWc[0] + "|" + strWc[1];//误差|

        //        sqlList.Add(entity.ToInsertString());
        //    }
        //    return sqlList;
        //}
        #endregion

        private void GetPCodeTable()
        {
            //功率方向
            PCodeTable.Add("powerFlag", GetPCodeDic("powerFlag"));
            //电流相别
            PCodeTable.Add("currentPhaseCode", GetPCodeDic("currentPhaseCode"));
            //电表类型
            PCodeTable.Add("equipCateg", GetPCodeDic("equipCateg"));
            //频率
            PCodeTable.Add("meterTestFreq", GetPCodeDic("meterTestFreq"));
            //功率因数
            PCodeTable.Add("meterTestPowerFactor", GetPCodeDic("meterTestPowerFactor"));
            //试验电压（百分比）
            PCodeTable.Add("meterTestVolt", GetPCodeDic("meterTestVolt"));
            //电流 (Imax Ib) 
            PCodeTable.Add("meterTestCurLoad", GetPCodeDic("meterTestCurLoad"));

            //额定电压 
            PCodeTable.Add("meterVolt", GetPCodeDic("meterVolt"));
            //额定电流
            PCodeTable.Add("meterRcSort", GetPCodeDic("meterRcSort"));
            //电表型号
            PCodeTable.Add("meterModelNo", GetPCodeDic("meterModelNo"));
            //费率
            PCodeTable.Add("fee", GetPCodeDic("fee"));
            //经互感器
            PCodeTable.Add("conMode", GetPCodeDic("conMode"));

            //等级
            PCodeTable.Add("meterAccuracy", GetPCodeDic("meterAccuracy"));

            //电表常数
            PCodeTable.Add("meterConstCode", GetPCodeDic("meterConstCode"));
            //接线方式
            PCodeTable.Add("wiringMode", GetPCodeDic("wiringMode"));
            //厂家
            PCodeTable.Add("MadeSupp", GetPCodeDic("MadeSupp"));
            //电能表事件记录试验检查项目
            PCodeTable.Add("meterLogOutTest", GetPCodeDic("meterLogOutTest"));
            //电能表电能量分项累计存储试验检查项目
            PCodeTable.Add("meterStorageTest", GetPCodeDic("meterStorageTest"));

            //密钥状态
            PCodeTable.Add("secretKeyStatus", GetPCodeDic("secretKeyStatus"));//待定
            //密钥类型
            PCodeTable.Add("secretKeyType", GetPCodeDic("secretKeyType"));//待定
            //走字方法
            PCodeTable.Add("meterTestCtrlMode", GetPCodeDic("meterTestCtrlMode"));//待定
            //载波模块
            PCodeTable.Add("LocalChipManu", GetPCodeDic("LocalChipManu"));//待定

            //add 新增code
            ////试品类别
            //PCodeTable.Add("equipCateg", GetPCodeDic("equipCateg")); //已有
            //试品类型
            PCodeTable.Add("testObjType", GetPCodeDic("testObjType"));
            ////型号
            //PCodeTable.Add("meterModelNo", GetPCodeDic("meterModelNo")); //已有
            ////接线方式
            //PCodeTable.Add("wiringMode", GetPCodeDic("wiringMode")); //已有
            ////电压
            //PCodeTable.Add("meterVolt", GetPCodeDic("meterVolt")); //已有
            ////电流
            //PCodeTable.Add("meterRcSort", GetPCodeDic("meterRcSort")); //已有
            ////有功准确度等级
            //PCodeTable.Add("meterAccuracy", GetPCodeDic("meterAccuracy")); //已有
            ////无功准确度等级
            //PCodeTable.Add("meterAccuracy", GetPCodeDic("meterAccuracy")); //已有
            ////有功常数
            //PCodeTable.Add("meterConstCode", GetPCodeDic("meterConstCode")); //已有
            ////无功常数
            //PCodeTable.Add("meterConstCode", GetPCodeDic("meterConstCode")); //已有
            ////厂家
            //PCodeTable.Add("MadeSupp", GetPCodeDic("MadeSupp")); //已有
            //频率
            PCodeTable.Add("meterFreq", GetPCodeDic("meterFreq"));
            ////接入方式
            //PCodeTable.Add("conMode", GetPCodeDic("conMode")); //已有
        }

        private Dictionary<string, string> GetPCodeDic(string type)//从国金中间库取代码值
        {
            string sql = string.Format(@"select * from m_qt_p_code where code_type ='{0}'", type);
            DataTable dt = ExecuteReader(sql);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (DataRow dr in dt.Rows)
            {
                string value = dr["code"].ToString().Trim();
                string name = dr["name"].ToString().Trim();
                if (value.Length > 0)
                {
                    if (!dic.ContainsKey(value))
                        dic.Add(value, name);
                }
            }
            return dic;
        }

        /// <summary>
        /// 获取名字
        /// </summary>
        /// <param name="strCode"></param>
        /// <returns></returns>
        private string GetPName(string type, string code)
        {
            string name = "";
            Dictionary<string, string> dic = PCodeTable[type];
            if (dic.Count > 0)
            {
                foreach (string key in dic.Keys)
                {
                    if (key == code)
                    {
                        name = dic[key];
                        break;
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// 查找国金任务表Item_ID编号
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="strMisGroupInfo"></param>
        private string GetTestbarcode(string barcode, string taskNo, out Dictionary<string, string> Item_ID)
        {
            //string sql = string.Format(@"select * from m_qt_task_item where bar_code = '{0}'and task_no='{1}'and LAB_NO like '%{2}%' ", barcode.Trim(), taskNo.Trim(), benthNo.Trim());
            string sql = string.Format(@"select * from m_qt_task_item where bar_code = '{0}'and task_no='{1}'  ", barcode.Trim(), taskNo.Trim());
            //string sql = string.Format(@"select * from m_qt_task_item where task_no = '{0}'  ", taskNo.Trim());
            //string sql = string.Format(@"select * from m_qt_task_item where bar_code = '{0}'  ", barcode.Trim());
            DataTable dt = ExecuteReader(sql);
            //2130001230000001722864
            //2130001230000001722284
            Item_ID = new Dictionary<string, string>();
            string tmp = "";
            foreach (DataRow row in dt.Rows)
            {
                tmp = row["ITEM_ID"].ToString().Trim() + "|" + tmp;
                Item_ID.Add(row["ITEM_NAME"].ToString().Trim(), row["ITEM_ID"].ToString().Trim());
            }
            return tmp;
        }

        /// <summary>
        /// 查找国金试品基本信息
        /// </summary>
        /// <param name="barcode"></param>
        private string[] GetMeterInfo(string barcode)
        {

            string sql = $"select * from m_qt_meter_info where bar_code = '{barcode}'";
            DataTable dt = ExecuteReader(sql);

            string[] tmp = new string[20];
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    tmp[i] = row.ItemArray[i].ToString();
                }
            }
            return tmp;
        }


        bool IMis.UpdateCompleted()
        {
            string[] args = new string[1];

            string xml = "<PARA>";
            xml += "<DETECT_TASK_NO>" + taskNo + "</DETECT_TASK_NO>";
            xml += "<EQUIP_NO>" + ConfigHelper.Instance.EquipmentNo + "</EQUIP_NO>";
            xml += "</PARA>";

            args[0] = xml;
            object result = WebServiceHelper.InvokeWebService(WebServiceURL, "veriSendDetectRsltInfo", args);


            if (!WebServiceHelper.GetResultByXml(result.ToString()))
            {
                MessageBox.Show("数据上传失败，调用综合结论接口错误信息：" + WebServiceHelper.GetMessageByXml(result.ToString()));
                return false;
            }
            return true;
        }
    }
}