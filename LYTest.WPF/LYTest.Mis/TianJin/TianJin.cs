using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.Core.Struct;
using LYTest.DAL.Config;
using LYTest.Mis.Common;
using LYTest.Mis.MisData;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace LYTest.Mis.TianJin
{
    /// <summary>
    /// PCODE对应表
    /// </summary>
    [Serializable()]
    public class StPCodeDicForMis
    {

        public Dictionary<string, string> DicPCode = new Dictionary<string, string>();

        /// <summary>
        /// 获取名字
        /// </summary>
        public string GetPName(string code)
        {
            if (DicPCode.ContainsKey(code))
                return DicPCode[code];

            return "";
        }
    }
    public class TianJin : IMis
    {
        public TianJin(string ip, int port, string dataSource, string userId, string pwd, string webUrl)
        {
            SqlHelper.SetDataConfig(ip, port, dataSource, userId, pwd, webUrl);
        }

        public static Dictionary<string, StPCodeDicForMis> g_DicPCodeTable;
        public static void GetDicPCodeTable()
        {
            //获取MIS字典表信息
            g_DicPCodeTable = new Dictionary<string, StPCodeDicForMis>
            {
                //功率方向
                { "meterBwpf", GetPCodeDicFromPrductionControlSystem("meterBwpf") },
                //类别
                { "meterSort", GetPCodeDicFromPrductionControlSystem("meterSort") },
                //结构
                { "meterTheory", GetPCodeDicFromPrductionControlSystem("meterTheory") },
                //电流
                { "meterRcSort", GetPCodeDicFromPrductionControlSystem("meterRcSort") },
                //功率因数
                { "detectErrPF", GetPCodeDicFromPrductionControlSystem("detectErrPF") },
                //试验电压
                { "meterVolt", GetPCodeDicFromPrductionControlSystem("meterVolt") },
                //检定点
                { "meterLdCt", GetPCodeDicFromPrductionControlSystem("meterLdCt") },
                //等级
                { "meterAccuracy", GetPCodeDicFromPrductionControlSystem("meterAccuracy") },
                //电表类型
                { "meterTypeCode", GetPCodeDicFromPrductionControlSystem("meterTypeCode") },
                //电表常数
                { "meterConstCode", GetPCodeDicFromPrductionControlSystem("meterConstCode") },
                //接线方式
                { "wiringMode", GetPCodeDicFromPrductionControlSystem("wiringMode") },
                //通信方式
                { "commMode", GetPCodeDicFromPrductionControlSystem("commMode") },
                //接入方式
                { "conMode", GetPCodeDicFromPrductionControlSystem("conMode") },
                //电表型号
                { "meterModelNo", GetPCodeDicFromPrductionControlSystem("meterModelNo") },
                //厂家
                { "meterFacturer", GetPCodeDicFromPrductionControlSystem("meterFacturer") },
                //频率
                { "meterFreq", GetPCodeDicFromPrductionControlSystem("meterFreq") },
                //通信规约
                { "commProtocol", GetPCodeDicFromPrductionControlSystem("commProtocol") },
                //通信规约
                { "svCommProtocol", GetPCodeDicFromPrductionControlSystem("svCommProtocol") },
                //通信速率
                { "svBaudRate", GetPCodeDicFromPrductionControlSystem("svBaudRate") }
            };

        }

        /// <summary>
        /// 从生产控制系统数据库根据代码获取代码值
        /// </summary>
        /// <param name="codeType"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private static StPCodeDicForMis GetPCodeDicFromPrductionControlSystem(string codeType)
        {
            StPCodeDicForMis PCodeDicForMis = new StPCodeDicForMis();
            string sql = string.Format(@"select * from m_p_code where code_type ='{0}'", codeType);
            DataTable ds = SqlHelper.Query(sql);
            foreach (DataRow dr in ds.Rows)
            {
                string value = dr["value"].ToString().Trim();
                string name = dr["name"].ToString().Trim();
                if (value.Length > 0)
                {
                    if (!PCodeDicForMis.DicPCode.ContainsKey(value))
                        PCodeDicForMis.DicPCode.Add(value, name);
                }
            }
            return PCodeDicForMis;
        }

        //public TianJin(string ip, int port, string dataSource, string userId, string pwd, string url)
        //{
        //    this.Ip = ip;
        //    this.Port = port;
        //    this.DataSource = dataSource;
        //    this.UserId = userId;
        //    this.Password = pwd;
        //    this.WebServiceURL = url;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="barCode">条形码</param>
        /// <param name="meter">一块表的基本数据</param>
        /// <returns></returns>
        public bool Down(string barCode, ref TestMeterInfo meter)
        {
            Dictionary<int, TestMeterInfo> meterDic = new Dictionary<int, TestMeterInfo>();
            if (g_DicPCodeTable == null)
                GetDicPCodeTable();
            string bodyNo = ConfigHelper.Instance.EquipmentNo;
            string sqld = $@"select * from M_VERIFY_MAIN where DEV_ID ='{bodyNo}' order by SEND_TIME DESC";
            DataTable ds = SqlHelper.Query(sqld);
            string batchno = ds.Rows[0][0].ToString();

            //string sql = "select * from M_METER_PARAMS where BAR_CODE='" + meter.MD_BarCode + "' and  BATCH_NO='" + BATCH_NO + "' order by POSITION";
            //LogHelper.WriteLog($"Down_A");
            DataRow row;
            if (barCode.Trim().Length < 3)
            {
                // 按表位号查询
                string sql = $"select * from M_METER_PARAMS where BATCH_NO='{batchno}' AND POSITION={barCode} order by POSITION";
                DataTable dt = SqlHelper.Query(sql);
                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show($"不存在批次号号为({batchno})的记录");
                    return false;
                }
                //int rid = int.Parse(barCode) - 1;
                //row = dt.Rows[rid];
                row = dt.Rows[0];

            }
            else
            {
                // 按条码查询
                string sql = $"select * from M_METER_PARAMS where BATCH_NO='{batchno}' AND BAR_CODE='{barCode}' order by POSITION";
                DataTable dt = SqlHelper.Query(sql);
                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show($"不存在批次号号为({batchno})的记录");
                    return false;
                }

                row = dt.Rows[0];

            }
            if (row == null) return false;

            // 查询整张表中等于当前条形码的一条数据
            string barcodes = row["BAR_CODE"].ToString().Trim();              //条码号
            int pos = Convert.ToInt32(row["POSITION"].ToString().Trim());        //表位号
            if (string.IsNullOrEmpty(barcodes) || pos <= 0)
            {
                MessageBox.Show($"不存在条码号为({barcodes})的表条码为空或位置号不对");
            }
            string inifile = "Ini\\TianJinData.ini";
            File.WriteInIString(inifile, "Data", "BatchNo", batchno);

            meter.BatchNo = batchno;
            meter.Other5 = row["METER_ID"].ToString().Trim();
            meter.MD_BarCode = row["BAR_CODE"].ToString().Trim();              //条形码
            meter.MD_AssetNo = row["ASSET_NO"].ToString().Trim();              //申请编号
            meter.MD_MadeNo = row["MADE_NO"].ToString().Trim();              //出厂编号
            meter.MD_Epitope = pos;                  //表位号
            meter.BatchNo = row["BATCH_NO"].ToString().Trim();  //到货批次号
            meter.MD_TaskNo = row["BATCH_NO"].ToString().Trim();
            meter.MD_PostalAddress = row["COMM_ADDR"].ToString().Trim();
            meter.WorkNo = row["APP_NO"].ToString().Trim();
            meter.MD_ProtocolName = g_DicPCodeTable["svCommProtocol"].GetPName(row["CONST_CODE"].ToString().Trim());
            meter.MD_ConnectionFlag = row["CON_MODE"].ToString().Trim() == "01" ? "直接式" : "互感式";

            //表类型                
            meter.MD_MeterType = g_DicPCodeTable["meterTypeCode"].GetPName(row["TYPE_CODE"].ToString().Trim());

            //表常数 
            #region
            string strBcs = g_DicPCodeTable["meterConstCode"].GetPName(row["CONST_CODE"].ToString().Trim());
            string strBcswg = g_DicPCodeTable["meterConstCode"].GetPName(row["RP_CONSTANT"].ToString().Trim());
            if (strBcswg != "")
                meter.MD_Constant = strBcs + "(" + strBcswg + ")";
            else
                meter.MD_Constant = strBcs;               //表常数
            #endregion
            #region 接线方式1
            string strWiringMode = g_DicPCodeTable["wiringMode"].GetPName(row["WIRNG_MODE"].ToString().Trim());
            switch (strWiringMode)
            {
                case "三相四线":
                    meter.MD_WiringMode = WireMode.三相四线.ToString();
                    break;
                case "三相三线":
                    meter.MD_WiringMode = WireMode.三相三线.ToString();
                    break;
                case "单相":
                    meter.MD_WiringMode = WireMode.单相.ToString();
                    break;
                default:
                    meter.MD_WiringMode = WireMode.单相.ToString();
                    break;
            }
            #endregion
            //表等级    
            #region 电表等级
            string strBdj = g_DicPCodeTable["meterAccuracy"].GetPName(row["AP_PRE_LEVEL_CODE"].ToString().Trim());

            string strBdjwg = g_DicPCodeTable["meterAccuracy"].GetPName(row["RP_PRE_LEVEL_CODE"].ToString().Trim());
            if (strBdjwg != "")     //假如有功表等级和无功表等级不一致
                meter.MD_Grane = strBdj + "(" + strBdjwg + ")";
            else
                meter.MD_Grane = strBdj;               //表等级
            #endregion

            meter.MD_MeterModel = g_DicPCodeTable["meterModelNo"].GetPName(row["MODEL_CODE"].ToString().Trim());           //表型号
            meter.MD_Customer = row["ORG_NO"].ToString().Trim();
            if (meter.MD_Customer == "23101")
                meter.MD_Customer = "黑龙江省计量中心";
            //通讯协议置空,由客户自已输入 
            meter.MD_ProtocolName = "CDLT698";//CDLT6452007
            meter.MD_CarrName = "标准载波";

            //meter.DgnProtocol = new LyTest.Core.Model.DgnProtocol.DgnProtocolInfo();
            //meter.DgnProtocol.Load(meter.MD_ProtocolName);

            #region 额定电压
            string ubtmp = g_DicPCodeTable["meterVolt"].GetPName(row["VOLT_CODE"].ToString().Trim());
            if (ubtmp.IndexOf("57.7") >= 0)
            {
                meter.MD_UB = 57.7F;
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
            {
                if (meter.MD_WiringMode == WireMode.单相.ToString())
                {
                    meter.MD_UB = 220;
                }
                else
                {
                    meter.MD_UB = 57.7F;
                }
            }
            #endregion
            #region 额定电流
            string ibtmp = g_DicPCodeTable["meterRcSort"].GetPName(row["RATED_CURRENT"].ToString().Trim());
            meter.MD_UA = ibtmp.Trim('A').Replace("（", "(").Replace("）", ")");
            #endregion
            meter.YaoJianYn = true;
            meter.MD_Factory = g_DicPCodeTable["meterFacturer"].GetPName(row["MANUFACTURER"].ToString().Trim());            //生产厂家
            meter.Seal1 = "";                                                 //铅封1,暂时置空
            meter.Seal2 = "";                                                 //铅封2,暂时置空
            meter.Seal3 = "";                                                 //铅封3,暂时置空       
            meter.Meter_SysNo = "480";
            if (meterDic.ContainsKey(pos))
                meterDic.Remove(pos);
            meterDic.Add(pos, meter);


            return true;
        }
        private string JoinValue(params string[] values)
        {
            return string.Join("|", values);
        }
        private static readonly string inifile = "Ini\\TianJinData.ini";

        /// <summary>
        /// 下载方案
        /// </summary>
        /// <param name="batchNo">批次号</param>
        /// <param name="schemeName">方案名称</param>
        /// <param name="DownGroup"></param>
        /// <returns></returns>
        public bool SchemeDown(TestMeterInfo meterInfo, out string schemeName, out Dictionary<string, SchemaNode> DownGroup)
        {
            try
            {

                DownGroup = new Dictionary<string, SchemaNode>();
                schemeName = meterInfo.BatchNo;
                string strPulse = "";
                string strGetNum = "";
                string strRatePeriod = "";
                string strRatePeriodF = "";
                int TmpFzdl = 0;//HQ20250408
                string TmpFzdl1 = "0.1Ib";//HQ20250408
                string TmpFzdl2 = "Ib";//HQ20250408
                string TmpFzdl3 = "Imax";//HQ20250408
                if (g_DicPCodeTable == null)
                    GetDicPCodeTable();

                int bphNum = 0; //记录不平衡点个数

                string inifile = "Ini\\TianJinData.ini";
                string batchNo = File.ReadInIString(inifile, "Data", "BatchNo", "");
                meterInfo.BatchNo = batchNo;
                string sql = $@"select schema_id from M_VERIFY_MAIN where BATCH_NO='{batchNo}'";
                DataTable dt = SqlHelper.Query(sql);


                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show($"M_VERIFY_MAIN表中不存在批次号为({batchNo})的方案信息");
                    return false;
                }
                string schemaId = dt.Rows[0]["SCHEMA_ID"].ToString().Trim();
                schemaId = schemaId.Replace('/', '_');
                if (string.IsNullOrEmpty(schemaId)) return true;
                schemeName = schemaId;

                //LogHelper.WriteLog($"方案名称:{schemeName}");

                sql = string.Format(@"select * from M_METER_VERIFY_SCHEMA where SCHEMA_ID = '{0}' order by ORDER_ID,ORDER_SUBID", schemaId);
                dt = SqlHelper.Query(sql);


                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show("M_METER_VERIFY_SCHEMA表中不存在方案编号为(" + schemaId + ")的方案信息");
                    LogManager.AddMessage("M_METER_VERIFY_SCHEMA表中不存在方案编号为(" + schemaId + "的方案信息", level: EnumLevel.Error);
                    return false;
                }
                int errIndex = 0;
                List<string> listStr = new List<string>();

                List<string> aaa = new List<string>();
                for (int a = 0; a < dt.Rows.Count; a++)
                {
                    aaa.Add(dt.Rows[a]["ITEM_NO"].ToString().Trim());
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // 大项编号
                    string itemNo = dt.Rows[i]["ITEM_NO"].ToString().Trim();
                    // 小项编号
                    string subItemNo = dt.Rows[i]["SUBITEM_NO"].ToString().Trim();
                    string paramList = dt.Rows[i]["PARAMS_LIST"].ToString().Trim();

                    //LogHelper.WriteLog($"{itemNo}-{subItemNo}:[{paramList}]");

                    string[] paraArr = paramList.Split('[');
                    if (itemNo == "02" && paraArr.Length <= 2) continue;
                    if (itemNo == "03" && paraArr.Length <= 2) continue;
                    if (itemNo == "04" && paraArr.Length <= 2) continue;
                    if (itemNo == "05" && paraArr.Length <= 2) continue;
                    if (itemNo == "06" && paraArr.Length <= 2) continue;
                    if (itemNo == "0812" && paraArr.Length <= 2) continue;
                    if (itemNo == "03" && paraArr.Length <= 2) continue;
                    if (itemNo == "18" && paraArr.Length <= 2) continue;//HQ20250408
                    if (itemNo == "19" && paraArr.Length <= 2) continue;//HQ20250408
                    if (itemNo == "20" && paraArr.Length <= 2) continue;//HQ20250408
                    string strXIb;
                    string strLimitDown;
                    string strLimitUp;
                    string strVoltage;
                    string strGlys;
                    string strYj;
                    string strGlfx;
                    string strWcx;//误差限//HQ20250408


                    PowerWay _FangXiang;
                    Cus_PowerYuanJian _YJ;
                    switch (itemNo)
                    {
                        case "0802"://"通讯模块测试":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.通讯测试))
                                    DownGroup.Add(ProjectID.通讯测试, new SchemaNode());
                                string values = JoinValue();
                                DownGroup[ProjectID.通讯测试].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.通讯测试, subItemNo);
                            }
                            break;
                        case "0803"://"通讯模块测试":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.通讯测试))
                                    DownGroup.Add(ProjectID.通讯测试, new SchemaNode());
                                string values = JoinValue("001", "通讯测试", "1|1|1|0Ib|1.0", "1", subItemNo, itemNo);
                                DownGroup[ProjectID.通讯测试].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.通讯测试, subItemNo);
                            }
                            break;
                        case "外观、标志检查":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.外观检查))
                                    DownGroup.Add(ProjectID.外观检查, new SchemaNode());
                                DownGroup[ProjectID.外观检查].SchemaNodeValue.Add("0");
                                File.WriteInIString(inifile, "Data", ProjectID.外观检查, subItemNo);
                            }
                            break;
                        case "03":// "起动试验":
                            if (!DownGroup.ContainsKey(ProjectID.起动试验))
                                DownGroup.Add(ProjectID.起动试验, new SchemaNode());
                            {
                                _FangXiang = PowerWay.正向有功;
                                float _fIb = 0, _fTime = 0;//,_fVolt = 0

                                if (paraArr.Length > 3)
                                {

                                    _fIb = float.Parse(paraArr[2].Substring(0, 5));
                                    string[] _sTimeTemp = paraArr[3].Substring(0, 8).Split(':');
                                    _fTime = float.Parse(_sTimeTemp[0]) * 60 + float.Parse(_sTimeTemp[1]) + float.Parse(_sTimeTemp[2]) / 60;

                                }
                                string values = JoinValue(_FangXiang.ToString(), _fIb.ToString(), "是", "是", "否", _fTime.ToString());
                                DownGroup[ProjectID.起动试验].SchemaNodeValue.Add(values);
                                File.WriteInIString(inifile, "Data", ProjectID.起动试验, subItemNo);

                            }
                            break;
                        case "02"://"潜动试验":
                            if (!DownGroup.ContainsKey(ProjectID.潜动试验))
                                DownGroup.Add(ProjectID.潜动试验, new SchemaNode());
                            {
                                _FangXiang = PowerWay.正向有功;
                                string _fVolt = "100%";
                                float _fTime = 0;
                                if (paraArr.Length >= 2)
                                {
                                    if (paraArr[1].IndexOf("%") != -1)
                                    {
                                        strVoltage = paraArr[1].Substring(0, 3);
                                        _fVolt = strVoltage + "%";
                                    }
                                    string[] _sTimeTemp = paraArr[3].Substring(0, 8).Split(':');
                                    _fTime = float.Parse(_sTimeTemp[0]) * 60 + float.Parse(_sTimeTemp[1]) + float.Parse(_sTimeTemp[2]) / 60;

                                    _fTime = (float)(Math.Truncate(_fTime * 100) / 100);
                                }
                                string values = JoinValue(_FangXiang.ToString(), _fVolt, "默认电流开路".ToString(), "是", "否", _fTime.ToString());
                                DownGroup[ProjectID.潜动试验].SchemaNodeValue.Add(values);
                                File.WriteInIString(inifile, "Data", ProjectID.潜动试验, subItemNo);

                            }
                            break;
                        case "04":// "基本误差":
                            if (paraArr[9].Substring(0, 1) == "1")
                            {

                                if (!DownGroup.ContainsKey(ProjectID.标准偏差试验))
                                    DownGroup.Add(ProjectID.标准偏差试验, new SchemaNode());
                                {
                                    if (paraArr == null || paraArr.Length < 1) continue;

                                    strGlfx = g_DicPCodeTable["meterBwpf"].GetPName(paraArr[1].Substring(0, 2));
                                    _FangXiang = GetGLFXFromString(strGlfx);
                                    if (paraArr[2].IndexOf("分") != -1)
                                    {
                                        strYj = paraArr[2].Substring(1, 1);
                                    }
                                    else
                                    {
                                        strYj = " ";
                                    }

                                    _YJ = GetYuanJianFromString(strYj);
                                    strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[4].Substring(0, 2));
                                    strXIb = strXIb.Replace("In", "Ib");

                                    if (strXIb == "Ib")
                                        strXIb = "1.0Ib";

                                    strGlys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));
                                    //string strQs = strParaList[7].Substring(0, 2).Replace(",", "");

                                    strLimitUp = paraArr[5].Substring(0, 4);
                                    strLimitDown = paraArr[6].Substring(0, 4);
                                    string strLimit = strLimitUp + "|" + strLimitDown;

                                    string values = JoinValue("标准偏差", _FangXiang.ToString(), _YJ.ToString(), strGlys, strXIb, "否", "否", "2", "100" /*subItemNo, itemNo*/);
                                    DownGroup[ProjectID.标准偏差试验].SchemaNodeValue.Add(values);
                                    Core.Function.File.WriteInIString(inifile, "Data", ProjectID.标准偏差试验 + _FangXiang.ToString() + _YJ.ToString() + strGlys + strXIb, subItemNo);
                                    errIndex++;


                                }

                            }
                            else
                            {

                                if (!DownGroup.ContainsKey(ProjectID.基本误差试验))
                                    DownGroup.Add(ProjectID.基本误差试验, new SchemaNode());
                                {
                                    strGlfx = g_DicPCodeTable["meterBwpf"].GetPName(paraArr[1].Substring(0, 2));
                                    _FangXiang = GetGLFXFromString(strGlfx);



                                    strYj = " ";
                                    //LogHelper.WriteLog($"误差方案点：{paramList}");

                                    if (paraArr[2].IndexOf("分") != -1)
                                    {
                                        strYj = paraArr[2].Substring(1, 1);
                                    }
                                    else if (paraArr[2].IndexOf("元不平衡") > 0)
                                    {
                                        strYj = paraArr[2].Substring(0, 1);
                                    }



                                    _YJ = GetYuanJianFromString(strYj);
                                    strGlys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));
                                    strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[4].Substring(0, 2));

                                    strXIb = strXIb.Replace("In", "Ib");

                                    if (strXIb == "Ib")
                                        strXIb = "1.0Ib";

                                    // 记录不平衡
                                    if (paraArr[2].IndexOf("元不平衡") > 0)
                                    {
                                        string str1 = $"{ProjectID.基本误差试验}{_FangXiang}{Cus_PowerYuanJian.H}{strGlys}{strXIb}";
                                        string str = $"{ProjectID.基本误差试验}{_FangXiang}{_YJ}{strGlys}{strXIb}";
                                        File.WriteInIString(inifile, "DataBPH", $"Point{bphNum}", $"{str1}|{str}|{subItemNo}"); // H元_分元_Key
                                        bphNum++;
                                        continue;

                                    }





                                    strLimitUp = paraArr[5].Substring(0, 4);
                                    strLimitDown = paraArr[6].Substring(0, 4);
                                    string strLimit = strLimitUp + "|" + strLimitDown;


                                    if (string.IsNullOrEmpty(strLimitUp) || string.IsNullOrEmpty(strLimitDown))
                                    {

                                        string values = JoinValue("基本误差", _FangXiang.ToString(), _YJ.ToString(), strGlys, strXIb, "否", "否", "2", "100" /*subItemNo, itemNo*/);
                                        DownGroup[ProjectID.基本误差试验].SchemaNodeValue.Add(values);
                                    }
                                    else if (paraArr[9].Substring(0, 1) == "1")
                                    {

                                        string values = JoinValue("标准偏差", _FangXiang.ToString(), _YJ.ToString(), strGlys, strXIb, "否", "否", "2", "100" /*subItemNo, itemNo*/);
                                        DownGroup[ProjectID.基本误差试验].SchemaNodeValue.Add(values);
                                    }
                                    else
                                    {
                                        string values = JoinValue("基本误差", _FangXiang.ToString(), _YJ.ToString(), strGlys, strXIb, "否", "否", "2", "100" /*subItemNo, itemNo*/);
                                        DownGroup[ProjectID.基本误差试验].SchemaNodeValue.Add(values);
                                    }
                                    Core.Function.File.WriteInIString(inifile, "Data", ProjectID.基本误差试验 + _FangXiang.ToString() + _YJ.ToString() + strGlys + strXIb, subItemNo);

                                    errIndex++;
                                }
                                break;

                            }

                            break;
                        case "11":// "常数试验":
                            if (!DownGroup.ContainsKey(ProjectID.电能表常数试验))
                                DownGroup.Add(ProjectID.电能表常数试验, new SchemaNode());
                            {
                                //strXIb = strParaList[4].Substring(0, 1) + ".0Ib";
                                strXIb = "2.0Ib";

                                strGlfx = g_DicPCodeTable["meterBwpf"].GetPName(paraArr[1].Substring(0, 2));
                                _FangXiang = GetGLFXFromString(strGlfx);


                                strGlys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[2].Substring(0, 2));
                                List<StPlan_ZouZi.StPrjFellv> _ListFeLv = new List<StPlan_ZouZi.StPrjFellv>();
                                StPlan_ZouZi.StPrjFellv stFeilv = new StPlan_ZouZi.StPrjFellv
                                {
                                    FeiLv = Cus_FeiLv.总,
                                    ZouZiTime = paraArr[5].Substring(0, 3),//strParaList[8].Substring(0, 3);
                                    StartTime = ""
                                };
                                _ListFeLv.Add(stFeilv);
                                string values = JoinValue(_FangXiang.ToString(), Cus_PowerYuanJian.H.ToString(), strGlys, strXIb, Cus_ZouZiMethod.标准表法.ToString(), "总", "0.5", stFeilv.ZouZiTime.ToString(), "");
                                DownGroup[ProjectID.电能表常数试验].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.电能表常数试验 + _FangXiang.ToString() + Cus_PowerYuanJian.H.ToString() + strGlys + strXIb, subItemNo);

                            }
                            break;
                        case "影响量试验":

                            break;
                        case "10":// "载波通信性能试验":
                            if (!DownGroup.ContainsKey(ProjectID.载波通信测试))
                                DownGroup.Add(ProjectID.载波通信测试, new SchemaNode());
                            {
                                string values = JoinValue("(当前)正向有功总电能", "00010000", itemNo);
                                DownGroup[ProjectID.载波通信测试].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.载波通信测试, subItemNo);

                            }
                            break;
                        case "0812"://"由电源供电的时钟试验":
                            if (!DownGroup.ContainsKey(ProjectID.日计时误差))
                                DownGroup.Add(ProjectID.日计时误差, new SchemaNode());
                            {
                                if (paraArr.Length > 7)
                                {
                                    strPulse = paraArr[2];
                                    strGetNum = paraArr[3];
                                }
                                if (strPulse.Length < 1)
                                    strPulse = "10";
                                if (strGetNum.Length < 1)
                                    strGetNum = "10";
                                //add lsj 2023 02 22 有问题 默认先传5个误差
                                string values = JoinValue("0.5", "5", strGetNum);
                                DownGroup[ProjectID.日计时误差].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.日计时误差, subItemNo);

                            }
                            break;
                        case "15":// "时钟示值误差":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.时钟示值误差))
                                    DownGroup.Add(ProjectID.时钟示值误差, new SchemaNode());
                                string values = JoinValue("5");
                                DownGroup[ProjectID.时钟示值误差].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.时钟示值误差, subItemNo);
                            }
                            break;
                        case "05":// "电子指示显示器电能示值组合误差":
                            if (paraArr[3].Trim() == "" || paraArr[3].IndexOf("有") == -1 || paraArr[3].IndexOf("总") != -1)
                            {
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.电能示值组合误差 + "总", subItemNo);
                                break;
                            }
                            if (!DownGroup.ContainsKey(ProjectID.电能示值组合误差))
                                DownGroup.Add(ProjectID.电能示值组合误差, new SchemaNode());
                            strRatePeriod = strRatePeriod + paraArr[4].Substring(0, 2) + ":" + paraArr[4].Substring(2, 2) + "(" + paraArr[3].Substring(4, 1) + ")" + ",";

                            Core.Function.File.WriteInIString(inifile, "Data", ProjectID.电能示值组合误差 + paraArr[4].Substring(0, 2) + ":" + paraArr[4].Substring(2, 2) + "(" + paraArr[3].Substring(4, 1) + ")", subItemNo);
                            break;
                        case "0811"://"校时功能":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.GPS对时))
                                    DownGroup.Add(ProjectID.GPS对时, new SchemaNode());
                                string values = JoinValue();
                                DownGroup[ProjectID.GPS对时].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.GPS对时, subItemNo);
                            }
                            break;
                        case "0805"://"需量示值误差":
                            if (!DownGroup.ContainsKey(ProjectID.需量示值误差))
                                DownGroup.Add(ProjectID.需量示值误差, new SchemaNode());
                            {
                                strXIb = "Imax";
                                strWcx = "2";//HQ20250408
                                if (paraArr.Length > 8)
                                {
                                    strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[6].Substring(0, 2));
                                    if (strXIb == "Ib")//HQ20250408
                                        strXIb = "1.0Ib";//HQ20250408                                    
                                    strWcx = paraArr[7].Split(',')[0].Replace('+', ' ').Trim();//HQ20250408
                                }

                                string values = JoinValue(strXIb, "正向有功", "15", "1", "1", strWcx);
                                DownGroup[ProjectID.需量示值误差].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.需量示值误差, subItemNo);

                            }
                            break;

                        case "9":// "安全认证试验":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.身份认证))
                                    DownGroup.Add(ProjectID.身份认证, new SchemaNode());

                                string values = JoinValue();
                                DownGroup[ProjectID.身份认证].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.身份认证, subItemNo);
                            }
                            break;
                        case "13":// "控制功能":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.远程控制))
                                    DownGroup.Add(ProjectID.远程控制, new SchemaNode());

                                //string values = JoinValue();
                                DownGroup[ProjectID.远程控制].SchemaNodeValue.Add("");
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.远程控制, subItemNo);
                            }
                            break;

                        case "12":// "密钥更新试验":
                            {
                                if (!DownGroup.ContainsKey(ProjectID.密钥更新))
                                    DownGroup.Add(ProjectID.密钥更新, new SchemaNode());

                                string values = JoinValue();
                                DownGroup[ProjectID.密钥更新].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.密钥更新, subItemNo);
                            }
                            break;
                        case "14":// "剩余电量递减准确度试验":

                            if (meterInfo.FKType != 0)
                            {
                                //远程表不需要这个试验
                                if (!DownGroup.ContainsKey(ProjectID.剩余电量递减准确度))
                                    DownGroup.Add(ProjectID.剩余电量递减准确度, new SchemaNode());

                                string values = JoinValue();
                                DownGroup[ProjectID.剩余电量递减准确度].SchemaNodeValue.Add(values);
                                File.WriteInIString(inifile, "Data", ProjectID.剩余电量递减准确度, subItemNo);

                            }
                            break;
                        case "18":// "误差变差"//HQ20250409
                            {
                                strGlys = "1.0";
                                if (!DownGroup.ContainsKey(ProjectID.误差变差))
                                    DownGroup.Add(ProjectID.误差变差, new SchemaNode());
                                if (paraArr.Length > 6)
                                {
                                    strGlys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));
                                }

                                string values = JoinValue("5", strGlys);
                                DownGroup[ProjectID.误差变差].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.误差变差, subItemNo);
                            }
                            break;
                        case "19":// "误差一致性"//HQ20250409
                            {
                                strGlys = "1.0";
                                strXIb = "Ib";
                                if (!DownGroup.ContainsKey(ProjectID.误差一致性))
                                    DownGroup.Add(ProjectID.误差一致性, new SchemaNode());
                                if (paraArr.Length > 6)
                                {
                                    strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[2].Substring(0, 2));
                                    strGlys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));
                                }
                                string values = JoinValue(strXIb, strGlys);
                                DownGroup[ProjectID.误差一致性].SchemaNodeValue.Add(values);
                                Core.Function.File.WriteInIString(inifile, "Data", ProjectID.误差一致性, subItemNo);
                            }
                            break;
                        case "20":// "负载电流升将变差"//HQ20250409
                            {

                                strXIb = "Ib";
                                if (!DownGroup.ContainsKey(ProjectID.负载电流升将变差))
                                    DownGroup.Add(ProjectID.负载电流升将变差, new SchemaNode());
                                if (TmpFzdl < 3)
                                {
                                    if (paraArr.Length > 6)
                                    {
                                        strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[2].Substring(0, 2));
                                        TmpFzdl++;
                                    }
                                    switch (TmpFzdl)
                                    {
                                        case 1:
                                            {
                                                TmpFzdl1 = strXIb;
                                                break;
                                            }
                                        case 2:
                                            {
                                                TmpFzdl2 = strXIb;
                                                break;
                                            }
                                        case 3:
                                            {
                                                TmpFzdl3 = strXIb;
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    TmpFzdl = 6;
                                }
                                if (TmpFzdl == 3)
                                {
                                    string values = JoinValue(TmpFzdl1, TmpFzdl2, TmpFzdl3);
                                    DownGroup[ProjectID.负载电流升将变差].SchemaNodeValue.Add(values);
                                    Core.Function.File.WriteInIString(inifile, "Data", ProjectID.负载电流升将变差, subItemNo);
                                }

                            }
                            break;

                        case "28":// "读取电量1":
                            {

                                if (!DownGroup.ContainsKey(ProjectID.通讯协议检查试验2))
                                    DownGroup.Add(ProjectID.通讯协议检查试验2, new SchemaNode());

                                string values = JoinValue("17001", "通讯协议检查试验", "(当前)正向有功电能数据块|00100200|20|2|XXXXXX.XX|读|0000", "17001_02791", "(当前)正向有功电能数据块_读", subItemNo, "1");
                                DownGroup[ProjectID.通讯协议检查试验2].SchemaNodeValue.Add(values);
                                File.WriteInIString(inifile, "Data", ProjectID.通讯协议检查试验2, subItemNo);
                            }
                            break;
                        case "21": //芯片ID认证
                            {
                                if (!DownGroup.ContainsKey(ProjectID.载波芯片ID测试))
                                    DownGroup.Add(ProjectID.载波芯片ID测试, new SchemaNode());

                                //string values = JoinValue(itemNo + "_" + subItemNo);
                                string values = JoinValue("19004", "芯片ID认证", "", "19004", "芯片ID认证", subItemNo, "1");

                                DownGroup[ProjectID.载波芯片ID测试].SchemaNodeValue.Add(values);
                                File.WriteInIString(inifile, "Data", ProjectID.载波芯片ID测试, subItemNo);

                                break;
                            }

                        //case "可靠性验证试验":
                        //    break;
                        //case "自热试验":
                        //    break;
                        //case "温升试验":
                        //    break;
                        //case "接地故障抑制试验":
                        //    break;
                        //case "短时过电流影响试验":
                        //    break;
                        //case "电源电压影响":
                        //    break;
                        default:
                            break;
                    }


                }
                if (strRatePeriod.Length > 0)
                {

                    strRatePeriod = strRatePeriod.TrimEnd(',');
                    string values = JoinValue(strRatePeriod, "1", "0.5Imax", "否", "是");
                    DownGroup[ProjectID.电能示值组合误差].SchemaNodeValue.Add(values);
                }
                if (strRatePeriodF.Length > 0)
                {

                    strRatePeriod = strRatePeriod.TrimEnd(',');
                    string values = JoinValue("费率时段示值误差", "正向有功", "1");
                    DownGroup[ProjectID.费率时段示值误差].SchemaNodeValue.Add(values);
                }
                //TODO远程表做电量清零，本地表不需要
                if (meterInfo.FKType == 0)
                {
                    if (!DownGroup.ContainsKey(ProjectID.电量清零))
                    {
                        DownGroup.Add(ProjectID.电量清零, new SchemaNode());
                        string values = JoinValue();
                        DownGroup[ProjectID.电量清零].SchemaNodeValue.Add(values);
                    }
                }
                sql = string.Format(@"select * from M_SET_METER_PARAMS where BATCH_NO = '{0}' order by ORDER_ID", schemaId);
                DataTable dt2 = SqlHelper.Query(sql);
                if (dt2.Rows.Count <= 0)
                {
                    MessageBox.Show($"M_SET_METER_PARAMS表中不存在方案编号为({schemaId})的方案信息");
                    LogManager.AddMessage($"M_SET_METER_PARAMS表中不存在方案编号为({schemaId})的方案信息", level: EnumLevel.Error);
                }
                else
                {
                }
                //对添加的方案进行排序
                if (DownGroup.Keys.Count > 0) //需要排序一下
                {
                    DownGroup = SortScheme(DownGroup);

                }

                // 记录不平衡负载个数
                File.WriteInIString(inifile, "DataBPH", $"PointNum", bphNum.ToString());

                return true;
            }
            catch (Exception ex)
            {
                schemeName = "";
                LogManager.AddMessage(ex.ToString(), EnumLogSource.检定业务日志, EnumLevel.Warning);
                DownGroup = null;
                return false;
            }
        }

        public Dictionary<string, SchemaNode> SortScheme(Dictionary<string, SchemaNode> Schema)
        {
            Dictionary<string, SchemaNode> TemScheme = new Dictionary<string, SchemaNode>();

            if (Schema.ContainsKey(ProjectID.密钥更新_预先调试)) TemScheme.Add(ProjectID.密钥更新_预先调试, Schema[ProjectID.密钥更新_预先调试]);
            if (Schema.ContainsKey(ProjectID.密钥恢复_预先调试)) TemScheme.Add(ProjectID.密钥恢复_预先调试, Schema[ProjectID.密钥恢复_预先调试]);
            if (Schema.ContainsKey(ProjectID.起动试验)) TemScheme.Add(ProjectID.起动试验, Schema[ProjectID.起动试验]);
            if (Schema.ContainsKey(ProjectID.潜动试验)) TemScheme.Add(ProjectID.潜动试验, Schema[ProjectID.潜动试验]);
            if (Schema.ContainsKey(ProjectID.基本误差试验)) TemScheme.Add(ProjectID.基本误差试验, Schema[ProjectID.基本误差试验]);
            if (Schema.ContainsKey(ProjectID.标准偏差试验)) TemScheme.Add(ProjectID.标准偏差试验, Schema[ProjectID.标准偏差试验]);
            if (Schema.ContainsKey(ProjectID.载波通信测试)) TemScheme.Add(ProjectID.载波通信测试, Schema[ProjectID.载波通信测试]);
            if (Schema.ContainsKey(ProjectID.载波芯片ID测试)) TemScheme.Add(ProjectID.载波芯片ID测试, Schema[ProjectID.载波芯片ID测试]);
            if (Schema.ContainsKey(ProjectID.电能表常数试验)) TemScheme.Add(ProjectID.电能表常数试验, Schema[ProjectID.电能表常数试验]);
            if (Schema.ContainsKey(ProjectID.通讯测试)) TemScheme.Add(ProjectID.通讯测试, Schema[ProjectID.通讯测试]);
            if (Schema.ContainsKey(ProjectID.GPS对时)) TemScheme.Add(ProjectID.GPS对时, Schema[ProjectID.GPS对时]);
            if (Schema.ContainsKey(ProjectID.时钟示值误差)) TemScheme.Add(ProjectID.时钟示值误差, Schema[ProjectID.时钟示值误差]);
            if (Schema.ContainsKey(ProjectID.日计时误差)) TemScheme.Add(ProjectID.日计时误差, Schema[ProjectID.日计时误差]);
            if (Schema.ContainsKey(ProjectID.需量示值误差)) TemScheme.Add(ProjectID.需量示值误差, Schema[ProjectID.需量示值误差]);
            //if (Schema.ContainsKey(ProjectID.需量示值误差)) TemScheme.Add(ProjectID.需量示值误差, Schema[ProjectID.需量示值误差]);
            if (Schema.ContainsKey(ProjectID.电能示值组合误差)) TemScheme.Add(ProjectID.电能示值组合误差, Schema[ProjectID.电能示值组合误差]);
            if (Schema.ContainsKey(ProjectID.费率时段示值误差)) TemScheme.Add(ProjectID.费率时段示值误差, Schema[ProjectID.费率时段示值误差]);
            if (Schema.ContainsKey(ProjectID.误差一致性)) TemScheme.Add(ProjectID.误差一致性, Schema[ProjectID.误差一致性]);
            if (Schema.ContainsKey(ProjectID.误差变差)) TemScheme.Add(ProjectID.误差变差, Schema[ProjectID.误差变差]);
            if (Schema.ContainsKey(ProjectID.负载电流升将变差)) TemScheme.Add(ProjectID.负载电流升将变差, Schema[ProjectID.负载电流升将变差]);
            if (Schema.ContainsKey(ProjectID.通讯协议检查试验)) TemScheme.Add(ProjectID.通讯协议检查试验, Schema[ProjectID.通讯协议检查试验]);
            if (Schema.ContainsKey(ProjectID.通讯协议检查试验2)) TemScheme.Add(ProjectID.通讯协议检查试验2, Schema[ProjectID.通讯协议检查试验2]);
            if (Schema.ContainsKey(ProjectID.身份认证)) TemScheme.Add(ProjectID.身份认证, Schema[ProjectID.身份认证]);
            if (Schema.ContainsKey(ProjectID.远程控制)) TemScheme.Add(ProjectID.远程控制, Schema[ProjectID.远程控制]);
            if (Schema.ContainsKey(ProjectID.控制功能)) TemScheme.Add(ProjectID.控制功能, Schema[ProjectID.控制功能]);
            if (Schema.ContainsKey(ProjectID.报警功能)) TemScheme.Add(ProjectID.报警功能, Schema[ProjectID.报警功能]);
            if (Schema.ContainsKey(ProjectID.远程保电)) TemScheme.Add(ProjectID.远程保电, Schema[ProjectID.远程保电]);
            if (Schema.ContainsKey(ProjectID.保电解除)) TemScheme.Add(ProjectID.保电解除, Schema[ProjectID.保电解除]);
            if (Schema.ContainsKey(ProjectID.数据回抄)) TemScheme.Add(ProjectID.数据回抄, Schema[ProjectID.数据回抄]);
            if (Schema.ContainsKey(ProjectID.电量清零)) TemScheme.Add(ProjectID.电量清零, Schema[ProjectID.电量清零]);
            if (Schema.ContainsKey(ProjectID.钱包初始化)) TemScheme.Add(ProjectID.钱包初始化, Schema[ProjectID.钱包初始化]);
            if (Schema.ContainsKey(ProjectID.密钥更新)) TemScheme.Add(ProjectID.密钥更新, Schema[ProjectID.密钥更新]);

            //  身份认证 = "25001";
            //  远程控制 = "25002";
            //  报警功能 = "25003";
            //  远程保电 = "25004";
            //  保电解除 = "25005";
            //  数据回抄 = "25006";
            //  钱包初始化 = "25007";
            //  密钥更新 = "25008";
            //  密钥恢复 = "25009";

            return TemScheme;
        }

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }


        public static Dictionary<string, Dictionary<string, string>> PCodeTable = new Dictionary<string, Dictionary<string, string>>();
        public bool Update(TestMeterInfo meterInfo)
        {

            batchNoUp = meterInfo.MD_TaskNo;

            if (!GetSchemeDetail(batchNoUp)) return false;

            Dictionary<string, string> schemeKey = schemeDic[batchDic[batchNoUp]];

            LogHelper.WriteLog($"=================================================================================================");

            LogHelper.WriteLog($"Update 【barcode:{meterInfo.MD_BarCode}, Epitope:{meterInfo.MD_Epitope}】 ");

            // 记录不平衡负载个数
            int bphNum = int.Parse(File.ReadInIString(inifile, "DataBPH", $"PointNum", "0"));
            List<BphItem> bphList = new List<BphItem>();
            for (int i = 0; i < bphNum; i++)
            {
                //string str = $"{ProjectID.基本误差试验}{_FangXiang}{_YJ}{strGlys}{strXIb}_{subItemNo}";
                //string str = File.ReadInIString(inifile, "DataBPH", $"Point{i}", "");
                string str = schemeKey[$"DataBPH_Point{i}"];
                if (string.IsNullOrEmpty(str.Trim())) break;
                string[] arr = str.Trim().Split('|');
                bphList.Add(new BphItem()
                {
                    KeyH = arr[0].Trim(),
                    KeyF = arr[1].Trim(),
                    SubItem = Trim(arr[2].Trim()),
                });

            }

            LogHelper.WriteLog($"开始上传:表号:{meterInfo.MD_BarCode}");
            //MessageBox.Show($"天津上传\nPointNum:{bphNum};Len:{bphList.Count}");

            try
            {
                //启动
                GetMT_STARTING_MET_CONCByMt(meterInfo, schemeKey);
                //潜动
                GetMT_CREEPING_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("起动潜动插入数据出现问题" + ex, EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            try
            {
                //基本误差
                GetMT_BASICERR_MET_CONCByMt(meterInfo, bphList, schemeKey);

                // 不平衡
                //LogHelper.WriteLog($"开始上传不平衡!");
                GetMT_BASICERR_MET_CONC_BPH_ByMt(meterInfo, bphList);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("基本误差插入数据出现问题" + ex, EnumLogSource.检定业务日志, EnumLevel.Error);
            }
            try
            {
                //常数试验
                GetMT_WalkToPrduction_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("常数试验插入数据出现问题" + ex, EnumLogSource.检定业务日志, EnumLevel.Error);
            }

            try
            {
                //通讯测试
                GetMT_TXCS_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("通讯测试插入数据出现问题" + ex, EnumLogSource.检定业务日志, EnumLevel.Error);
            }

            try
            {
                //日计时
                GetMT_DAYERR_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("日计时插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //时钟示值误差
                GetMT_CLOCK_VALUE_MET_CONC(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("时钟示值误差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            try
            {
                //电子指示显示器电能示值组合误差
                GetMT_HUTCHISON_COMBINA_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("电子指示显示器电能示值组合误差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            try
            {
                //费率时段电能示值误差
                GetMT_RATE_PERIOD_MET_CONC(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("费率时段电能示值误差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //gps对时
                GetMT_GPS_MET_CONC(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("gps对时插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            try//HQ20250409
            {
                //需量示值误差
                GetMT_DEMANDVALUE_MET_CONC(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("需量示值误差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            try
            {
                //时间误差
                GetMT_CLOCK_VALUE_MET_CONC(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("需量示值误差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            try
            {
                //密钥更新
                GetMT_ESAM_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("密钥更新插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //身份认证
                GetMT_ESAM_SECURITY_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("身份认证插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //剩余电量递减准确度
                GetMT_EQ_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("剩余电量递减准确度插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }


            try
            {
                //控制功能
                GetMT_CONTROL_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("控制功能插入数据出现问题" + ex, EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //载波通讯
                GetMT_CarrierToPrduction_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("载波通讯插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try
            {
                //通讯协议检查试验2
                GetMT_METER_ENERGY_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("通讯协议检查试验2插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try//HQ20250409
            {
                //误差变差
                GetMT_ERROR_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("误差变差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            try//HQ20250409
            {
                //误差一致性
                GetMT_CONSIST_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("误差一致性插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }
            try//HQ20250409
            {
                //负载电流升降变差
                GetMT_VARIATION_MET_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("负载电流升降变差插入数据出现问题" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
            }

            //载波芯片ID测试
            try
            {
                GetMT_HPLC_ID_CONCByMt(meterInfo, schemeKey);
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("载波芯片ID测试插入数据出现问题" + ex, EnumLogSource.检定业务日志, EnumLevel.Error);
            }

            LogHelper.WriteLog($"Update 【barcode:{meterInfo.MD_BarCode}, Epitope:{meterInfo.MD_Epitope}】 End");
            LogHelper.WriteLog($"=================================================================================================");
            return true;
        }

        string batchNoUp = Core.Function.File.ReadInIString(inifile, "Data", "BatchNo", "");

        private string Trim(string str)
        {
            string str1 = "";
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || c == '_')
                    str1 += c;
            }
            return str1;
        }

        /// <summary>
        /// 启动
        /// </summary>
        private bool GetMT_STARTING_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("起动试验");
            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;
            //string ItemKey = ProjectID.起动试验;
            foreach (string key in meterQdQids.Keys)
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.起动试验, "");
                if (schemeKey.TryGetValue(ProjectID.起动试验, out string subItemNo))
                {
                    if (key.Split('_')[0] != "12002") continue;    // 排除潜动的
                    string strConclusion = meter.MeterQdQids[key].Result.Trim() == "合格" ? "01" : "02";
                    UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);

                }

                //string subItemNo = schemeKey[ProjectID.起动试验];
                //string current = Convert.ToSingle(meterQdQids[key].Current) + "Ib";
                //if (key.Split('_')[0] != "12002") continue;    // 排除潜动的
                //string strConclusion = meter.MeterQdQids[key].Result.Trim() == "合格" ? "01" : "02";
                //UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;

        }
        /// <summary>
        /// 潜动
        /// </summary>
        private bool GetMT_CREEPING_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {

            LogHelper.WriteLog("潜动试验");
            Dictionary<string, MeterQdQid> meterQdQids = meter.MeterQdQids;
            //string ItemKey = ProjectID.潜动试验;
            foreach (string key in meterQdQids.Keys)
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.潜动试验, "");
                string subItemNo = schemeKey[ProjectID.潜动试验];

                if (key.Split('_')[0] != "12003") continue;    // 排除启动的
                string strConclusion = meter.MeterQdQids[key].Result.Trim() == "合格" ? "01" : "02"; ;
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;
        }

        /// <summary>
        /// 基本误差数据
        /// </summary>
        private bool GetMT_BASICERR_MET_CONCByMt(TestMeterInfo meter, List<BphItem> bphs, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("基本误差数据");

            string[] keys = new string[meter.MeterErrors.Keys.Count];
            //string ItemKey = ProjectID.基本误差试验;
            meter.MeterErrors.Keys.CopyTo(keys, 0);


            for (int i = 0; i < keys.Length; i++)
            {
                string Para = keys[i].Split('_')[0];

                string key = keys[i];
                MeterError meterErr = meter.MeterErrors[key];
                string[] wc = meterErr.WCData.Split('|');
                if (wc.Length < 2) continue;

                string strPJZ = meter.MeterErrors[key].WCValue; //平均值
                string strPCZ = meter.MeterErrors[key].WCPC;    //偏差值
                string strHZZ = meter.MeterErrors[key].WCHZ;    //化整值
                string subItemNo;
                if (Para == ProjectID.标准偏差试验)
                    //subItemNo = File.ReadInIString(inifile, "Data", ProjectID.标准偏差试验 + meter.MeterErrors[key].GLFX + meter.MeterErrors[key].YJ + meter.MeterErrors[key].GLYS + meter.MeterErrors[key].IbX, "");
                    subItemNo = schemeKey[ProjectID.标准偏差试验 + meter.MeterErrors[key].GLFX + meter.MeterErrors[key].YJ + meter.MeterErrors[key].GLYS + meter.MeterErrors[key].IbX];
                else
                    subItemNo = schemeKey[ProjectID.基本误差试验 + meter.MeterErrors[key].GLFX + meter.MeterErrors[key].YJ + meter.MeterErrors[key].GLYS + meter.MeterErrors[key].IbX];

                string strConclusion = meter.MeterErrors[key].Result.Trim() == "合格" ? "01" : "02";
                string strdata = "";

                for (int ii = wc.Length - 1; ii >= 0; ii--)
                {
                    strdata += wc[ii];
                    if (ii > 0)
                    {
                        strdata += ",";
                    }
                }



                if (Para == ProjectID.标准偏差试验)
                    strdata = $"{strPCZ},{strHZZ},,,{strdata.TrimEnd()}";
                else
                {
                    strdata = $"{strPJZ},{strHZZ},,,{strdata.TrimEnd()}";

                    // 记录不平衡
                    string key1 = $"{ProjectID.基本误差试验}{meter.MeterErrors[key].GLFX}{meter.MeterErrors[key].YJ}{meter.MeterErrors[key].GLYS}{meter.MeterErrors[key].IbX}";
                    foreach (BphItem item in bphs)
                    {
                        //LogHelper.WriteLog($"record_bph_Key:{item.KeyH}|{item.KeyF}|{key1}");

                        if (item.KeyH == key1)
                        {
                            item.ValueH = $"{strPJZ}";
                            //LogHelper.WriteLog($"record_bph_H:{item.KeyH}|{item.SubItem}|{item.ValueH}");
                            //break;
                        }
                        else if (item.KeyF == key1)
                        {
                            item.ValueF = $"{strPJZ}";
                            //LogHelper.WriteLog($"record_bph_F:{item.KeyF}|{item.SubItem}|{item.ValueF}");
                            break;
                        }
                    }
                }
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strdata, strConclusion, "", meter.Other5, batchNoUp);

            }
            return true; ;
        }

        /// <summary>
        /// 基本误差数据不平衡
        /// </summary>
        private bool GetMT_BASICERR_MET_CONC_BPH_ByMt(TestMeterInfo meter, List<BphItem> bphs)
        {
            LogHelper.WriteLog("基本误差数据不平衡");

            //LogHelper.WriteLog($"updateBPH:{bphs.Count};");
            foreach (BphItem item in bphs)
            {
                //LogHelper.WriteLog($"updateBPH:ValueF[{item.ValueF}],ValueH:[{item.ValueH}];");
                if (string.IsNullOrEmpty(item.ValueF) || string.IsNullOrEmpty(item.ValueH)) continue;
                if (!(float.TryParse(item.ValueH, out float fH) && float.TryParse(item.ValueF, out float fF))) continue;

                float err = fF - fH; // 差值
                float avg = (fH + fF) / 2f; // 平均值


                // 误差限
                float limit = 1.0f;
                string[] levs = Number.GetDj(meter.MD_Grane);

                if (levs[0] == "A" || levs[0] == "B")
                    limit = 1.0f;
                else if (levs[0] == "C")
                    limit = 0.5f;
                else if (levs[0] == "D")
                    limit = 0.2f;

                string result;
                if (Math.Abs(err) < limit)
                    result = "01"; // 合格
                else
                    result = "02"; // 不合格

                string avgS = avg.ToString("f4");
                string errS = err.ToString("f2");
                if (avg >= 0)
                {
                    avgS = "+" + avgS;
                    errS = "+" + errS;

                }
                else
                {
                    if (!errS.StartsWith("-"))
                    {
                        errS = "-" + errS;
                    }
                }

                string strdata = $"{avgS},{errS},,,,,,,,";


                //LogHelper.WriteLog($"update_bph_H:{item.SubItem.Trim()}|{strdata}|{result}[{limit}]");


                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, item.SubItem.Trim(), strdata, result, "", meter.Other5, batchNoUp);

            }
            return true; ;
        }


        /// <summary>
        /// 常数试验
        /// </summary>
        private bool GetMT_WalkToPrduction_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("常数试验");

            //string ItemKey = ProjectID.电能表常数试验;
            foreach (string key in meter.MeterZZErrors.Keys)
            {
                var item = meter.MeterZZErrors[key];
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.电能表常数试验 + meter.MeterZZErrors[key].PowerWay + meter.MeterZZErrors[key].YJ + meter.MeterZZErrors[key].GLYS + meter.MeterZZErrors[key].IbX, "");
                string subItemNo = schemeKey[ProjectID.电能表常数试验 + meter.MeterZZErrors[key].PowerWay + meter.MeterZZErrors[key].YJ + meter.MeterZZErrors[key].GLYS + meter.MeterZZErrors[key].IbX];
                string strConclusion = meter.MeterZZErrors[key].Result.Trim() == "合格" ? "01" : "02";
                string zuhe = "0.00";
                string strvalue = $"{item.PowerStart},{item.PowerEnd},{item.ErrorValue},{item.PowerSumStart},{item.PowerSumEnd},{zuhe}";
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strvalue, strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;

        }

        /// <summary>
        /// 通讯测试
        /// </summary>
        private bool GetMT_TXCS_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("通讯测试");

            string ItemKey = ProjectID.通讯测试;
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.通讯测试, "");
                string subItemNo = schemeKey[ProjectID.通讯测试];

                string strConclusion = meter.MeterDgns[ItemKey].Result.Trim() == "合格" ? "01" : "02";
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;

        }

        /// <summary>
        /// 日计时
        /// </summary>
        private bool GetMT_DAYERR_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("日计时");

            ////平均值
            string key = ProjectID.日计时误差;
            if (meter.MeterDgns.ContainsKey(key))
            {
                //string subItemNo = File.ReadInIString(inifile, "Data", ProjectID.日计时误差, "");
                string subItemNo = schemeKey[ProjectID.日计时误差];
                string[] v = meter.MeterDgns[key].Value.Split('|');
                string error = v[0] + "," + v[1] + "," + v[2] + "," + v[3] + "," + v[4] + ",,,,,";
                string ave = v[5];
                string hz = v[6];
                string strRjsValue = error + "," + ave + "," + hz;
                string strConclusion = meter.MeterDgns[key].Result == "合格" ? "01" : "02";
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strRjsValue, strConclusion, "", meter.Other5, batchNoUp);

            }
            return true;
        }

        /// <summary>
        /// 时间误差
        /// </summary>
        protected bool GetMT_CLOCK_VALUE_MET_CONC(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("时间误差");

            string meterTime = "", stdTime = "", wc = "";
            string ItemKey = ProjectID.时钟示值误差;

            if (!meter.MeterDgns.ContainsKey(ItemKey))
                return false;

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
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.时钟示值误差, "");
                string subItemNo = schemeKey[ProjectID.时钟示值误差];

                string strValue = meterTime + "," + stdTime + "," + wc;
                string strConclusion = meter.MeterDgns[ItemKey].Result == "合格" ? "01" : "02";
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strValue, strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;
        }
        /// <summary>
        /// 计度器总电能示值组合误差
        /// </summary>
        protected bool GetMT_HUTCHISON_COMBINA_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("计度器总电能示值组合误差");

            string[] keys = new string[meter.MeterRegs.Keys.Count];

            meter.MeterRegs.Keys.CopyTo(keys, 0);

            string key = ProjectID.电能示值组合误差;
            if (!meter.MeterRegs.ContainsKey(key)) return false;

            //int times = 0; //时间
            string subItemNo;
            string strValue;
            string strConclusion;
            string[] topvalue = meter.MeterRegs[key].TopFLPower.Split(',');
            string[] endvalue = meter.MeterRegs[key].EndFLPower.Split(',');
            string[] FlCz = meter.MeterRegs[key].FLPowerD.Split(',');
            string alltop = meter.MeterRegs[key].TopTotalPower;
            string allend = meter.MeterRegs[key].EndTotalPower;
            string allCz = meter.MeterRegs[key].TotalPowerD;
            string[] testvalueone = meter.MeterRegs[key].FL.Split(',');//费率时段
            for (int i = 0; i < testvalueone.Length; i++)
            {
                //subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.电能示值组合误差 + testvalueone[i], "");
                subItemNo = schemeKey[ProjectID.电能示值组合误差 + testvalueone[i]];

                if (topvalue.Length != testvalueone.Length || endvalue.Length != testvalueone.Length || FlCz.Length != testvalueone.Length)
                {
                    strValue = "0" + "," + "0" + "," + "0" + "," + alltop + "," + allend + "," + allCz;
                    strConclusion = meter.MeterRegs[key].Result == "合格" ? "01" : "02";
                    UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strValue, strConclusion, "", meter.Other5, batchNoUp);
                }
                else
                {
                    strValue = topvalue[i] + "," + endvalue[i] + "," + FlCz[i] + "," + alltop + "," + allend + "," + allCz;
                    strConclusion = meter.MeterRegs[key].Result == "合格" ? "01" : "02";
                    UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strValue, strConclusion, "", meter.Other5, batchNoUp);
                }


            }
            subItemNo = schemeKey[ProjectID.电能示值组合误差 + "总"];

            //subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.电能示值组合误差 + "总", "");
            strValue = alltop + "," + alltop + "," + "" + "," + alltop + "," + allend + "," + allCz;
            strConclusion = meter.MeterRegs[key].Result == "合格" ? "01" : "02";
            UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strValue, strConclusion, "", meter.Other5, batchNoUp);
            return true;
        }
        /// <summary>
        ///需量示值误差//HQ20250409
        /// </summary>
        private bool GetMT_DEMANDVALUE_MET_CONC(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            string strValue;
            LogHelper.WriteLog("需量示值误差");

            for (int i = 1; i <= 3; i++)
            {
                string key = ProjectID.需量示值误差 + "_" + i.ToString();
                if (!meter.MeterDgns.ContainsKey(key)) continue;
                string[] value = meter.MeterDgns[key].Value.Split('|');
                string[] Testvalue = meter.MeterDgns[key].TestValue.Split('|');
                if (Testvalue[0] == "1.0Ib")
                    Testvalue[0] = "Ib";
                string subItemNo = schemeKey[ProjectID.需量示值误差 + Testvalue[0]];
                string strConclusion = meter.MeterDgns[key].Result.Trim() == "合格" ? "01" : "02";
                strValue = value[3] + "," + value[4] + "," + value[5] + "," + value[5];
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strValue, strConclusion, "", meter.Other5, batchNoUp);

            }

            return true;

        }


        /// <summary>
        /// 费率时段电能示值误差
        /// </summary>
        private bool GetMT_RATE_PERIOD_MET_CONC(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("费率时段电能示值误差");

            string key = ProjectID.费率时段示值误差;
            if (!meter.MeterDgns.ContainsKey(key)) return false;
            string strStartEnergy = "";
            string strEndEnergy = "";
            float fIncrementSharp = 0.0f;   //尖示值增量
            float fIncrementPeak = 0.0f;    //峰示值增量
            float fIncrementFlat = 0.0f;    //平示值增量
            float fIncrementValley = 0.0f;  //谷示值增量
            float fIncrementTotal = 0.0f;   //总示值增量
            if (meter.MeterDgns.ContainsKey(key))
            {
                for (int i = 0; i < 20; i++)
                {
                    if (!meter.MeterDgns.ContainsKey(key)) continue;
                    string value = meter.MeterDgns[key].Value;
                    if (string.IsNullOrEmpty(value)) continue;
                    if (value.IndexOf('|') == -1) continue;
                    string[] arr = value.Split('|');
                    if (arr.Length < 4) continue;

                    if (arr[3] == "尖")
                    {
                        fIncrementSharp += float.Parse(arr[2]);
                    }
                    else if (arr[3] == "峰")
                    {
                        fIncrementPeak += float.Parse(arr[2]);
                    }
                    else if (arr[3] == "平")
                    {
                        fIncrementFlat += float.Parse(arr[2]);
                    }
                    else if (arr[3] == "谷")
                    {
                        fIncrementValley += float.Parse(arr[2]);
                    }
                    else if (arr[3] == "总")
                    {
                        strStartEnergy = arr[0];
                        strEndEnergy = arr[1];
                        fIncrementTotal += float.Parse(arr[2]);
                    }
                }
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.费率时段示值误差, "");
                string subItemNo = schemeKey[ProjectID.费率时段示值误差];

                float fIncrementSumerAll = fIncrementSharp + fIncrementPeak + fIncrementFlat + fIncrementValley;
                float fReadingErrTotal = fIncrementTotal - fIncrementSumerAll;
                string data = strStartEnergy + "," + strEndEnergy + "," + fIncrementTotal.ToString("F2") + "," + strStartEnergy + "," + strEndEnergy + "," + fReadingErrTotal.ToString("F2");
                string strConclusion = meter.MeterQdQids[key].Result.Trim() == "合格" ? "01" : "02"; ;
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, data, strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;
        }

        /// <summary>
        /// GPS对时
        /// </summary>
        private bool GetMT_GPS_MET_CONC(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("GPS对时");

            string ItemKey = ProjectID.GPS对时;
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.GPS对时, "");
                string subItemNo = schemeKey[ProjectID.GPS对时];

                string strConclusion = meter.MeterDgns[ItemKey].Result.Trim() == "合格" ? "01" : "02";
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
            }
            return true;
        }


        /// <summary>
        /// 密钥更新
        /// </summary>
        private bool GetMT_ESAM_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("密钥更新");

            string ItemKey = ProjectID.密钥更新;
            if (meter.MeterCostControls.ContainsKey(ItemKey))
            {
                //string subItemNo = File.ReadInIString(inifile, "Data", ProjectID.密钥更新, "");
                string subItemNo = schemeKey[ProjectID.密钥更新];

                string result = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.密钥更新);
                string value;
                if (result == "01" && meter.MeterCostControls[ItemKey].Data.Length > 16)
                {
                    string[] mkeys = meter.MeterCostControls[ItemKey].Data.Split('|');
                    string mkey = mkeys.Length > 1 ? mkeys[1] : meter.MeterCostControls[ItemKey].Data;
                    value = mkey.PadRight(16, '0').Substring(0, 16) + "," + result + "," + "16";//"7FFFFFFFF07F80FF"
                }
                else
                {
                    value = "0000000000000000" + "," + "0" + "," + result;//"7FFFFFFFF07F80FF"
                }
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, value, result, "", meter.Other5, batchNoUp);
            }
            return true;
        }

        /// <summary>
        ///身份认证
        /// </summary>
        private bool GetMT_ESAM_SECURITY_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("身份认证");

            string ItemKey = ProjectID.身份认证;
            if (meter.MeterCostControls.ContainsKey(ItemKey))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.身份认证, "");
                string subItemNo = schemeKey[ProjectID.身份认证];

                string result = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.身份认证);
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, meter.MeterCostControls[ItemKey].Data.ToString(), result, "", meter.Other5, batchNoUp);
            }
            return true;
        }

        /// <summary>
        ///误差变差//HQ20250409
        /// </summary>
        private bool GetMT_ERROR_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("误差变差");
            foreach (string key in meter.MeterErrAccords.Keys)
            {
                if (key == "2")
                {
                    foreach (string keyT in meter.MeterErrAccords[key].PointList.Keys)
                    {
                        string subItemNo = schemeKey[ProjectID.误差变差 + meter.MeterErrAccords[key].PointList[keyT].PF];
                        string strConclusion = meter.MeterErrAccords[key].Result.Trim() == "合格" ? "01" : "02";
                        UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
                    }
                }
            }
            return true;

        }
        /// <summary>
        ///误差一致性//HQ20250409
        /// </summary>
        private bool GetMT_CONSIST_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("误差一致性");
            foreach (string key in meter.MeterErrAccords.Keys)
            {
                if (key == "1")
                {
                    foreach (string keyT in meter.MeterErrAccords[key].PointList.Keys)
                    {
                        string Ib = meter.MeterErrAccords[key].PointList[keyT].IbX;
                        if (Ib == "1.0Ib")
                            Ib = "Ib";
                        try
                        {
                            string subItemNo = schemeKey[ProjectID.误差一致性 + Ib + meter.MeterErrAccords[key].PointList[keyT].PF];
                            string strConclusion = meter.MeterErrAccords[key].Result.Trim() == "合格" ? "01" : "02";
                            UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
                        }
                        catch (Exception)
                        { }

                    }
                }
            }
            return true;

        }
        /// <summary>
        ///负载电流升降变差//HQ20250409
        /// </summary>
        private bool GetMT_VARIATION_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("负载电流升将变差");
            foreach (string key in meter.MeterErrAccords.Keys)
            {
                if (key == "3")
                {
                    foreach (string keyT in meter.MeterErrAccords[key].PointList.Keys)
                    {
                        string Ib = meter.MeterErrAccords[key].PointList[keyT].IbX;
                        if (Ib == "1.0Ib")
                            Ib = "Ib";
                        try
                        {
                            string subItemNo = schemeKey[ProjectID.负载电流升将变差 + Ib];
                            string strConclusion = meter.MeterErrAccords[key].Result.Trim() == "合格" ? "01" : "02";
                            UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            return true;
        }


        /// <summary>
        ///剩余电量递减准确度
        /// </summary>
        private bool GetMT_EQ_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("剩余电量递减准确度");

            string ItemKey = ProjectID.剩余电量递减准确度;
            if (meter.MeterCostControls.ContainsKey(ItemKey))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.剩余电量递减准确度, "");
                string subItemNo = schemeKey[ProjectID.剩余电量递减准确度];

                string result = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.剩余电量递减准确度);
                string[] arr = meter.MeterCostControls[ItemKey].Data.ToString().Split('|');
                string strValue = "";
                if (arr.Length > 5)
                    strValue = string.Format("{0},{1},{2},{3},{4},{5}", arr[2], arr[1], arr[4], arr[3], arr[0], arr[5]);
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, strValue, result, "", meter.Other5, batchNoUp);
            }
            return true;
        }


        /// <summary>
        ///控制功能
        /// </summary>
        private bool GetMT_CONTROL_MET_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("控制功能");

            if (meter.MeterCostControls.ContainsKey(ProjectID.控制功能))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.控制功能, "");
                string subItemNo = schemeKey[ProjectID.控制功能];

                string strConclusion = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.控制功能);
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", strConclusion, "", meter.Other5, batchNoUp);
            }
            else if (meter.MeterCostControls.ContainsKey(ProjectID.远程控制))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.远程控制, "");
                string subItemNo = schemeKey[ProjectID.远程控制];

                string result = MisDataHelper.GetFkConclusion(meter, Cus_CostControlItem.远程控制);
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", result, "", meter.Other5, batchNoUp);
            }
            return true;
        }


        /// <summary>
        ///载波试验
        /// </summary>
        private bool GetMT_CarrierToPrduction_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("载波试验");

            string ItemKey = ProjectID.载波通信测试;
            KeyValuePair<string, Core.Model.DnbModel.MeterTestData> data = meter.MeterTestDatas.FirstOrDefault(item => { return item.Key.StartsWith(ItemKey); });
            if (!string.IsNullOrWhiteSpace(data.Key))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.载波通信测试, "");
                string subItemNo = schemeKey[ProjectID.载波通信测试];

                string result = data.Value?.Result.Trim() == "合格" ? "01" : "02";
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", result, "", meter.Other5, batchNoUp);
            }
            return true;
        }

        /// <summary>
        /// 误差前读取电量
        /// </summary>
        private bool GetMT_METER_ENERGY_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            LogHelper.WriteLog("误差前读取电量");

            string ItemKey = ProjectID.通讯协议检查试验;
            KeyValuePair<string, MeterDLTData> data = meter.MeterDLTDatas.FirstOrDefault(item => { return item.Key.StartsWith(ItemKey); });
            if (!string.IsNullOrWhiteSpace(data.Key))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.通讯协议检查试验2, "");
                string subItemNo = schemeKey[ProjectID.通讯协议检查试验2];

                string result = data.Value?.Result.Trim() == "合格" ? "01" : "02";
                string value = data.Value.Value;
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, value, result, "", meter.Other5, batchNoUp);
            }
            return true;
        }


        /// <summary>
        /// 载波芯片ID测试
        /// </summary>
        private bool GetMT_HPLC_ID_CONCByMt(TestMeterInfo meter, Dictionary<string, string> schemeKey)
        {
            //LogHelper.WriteLog("载波芯片ID测试");

            //string ItemKey = ProjectID.载波芯片ID测试;
            //KeyValuePair<string, MeterDLTData> data = meter.MeterDLTDatas.FirstOrDefault(item => { return item.Key.StartsWith(ItemKey); });
            //if (!string.IsNullOrWhiteSpace(data.Key))
            //{
            //    //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.通讯协议检查试验2, "");
            //    string subItemNo = schemeKey[ProjectID.载波芯片ID测试];

            //    string result = data.Value?.Result.Trim() == "合格" ? "01" : "02";
            //    string value = data.Value;
            //    UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, value, result, "", meter.Other5, batchNoUp);
            //}
            //return true;


            LogHelper.WriteLog("载波芯片ID测试");

            string ItemKey = ProjectID.载波芯片ID测试;
            KeyValuePair<string, Core.Model.DnbModel.MeterTestData> data = meter.MeterTestDatas.FirstOrDefault(item => { return item.Key.StartsWith(ItemKey); });
            if (!string.IsNullOrWhiteSpace(data.Key))
            {
                //string subItemNo = Core.Function.File.ReadInIString(inifile, "Data", ProjectID.载波通信测试, "");
                string subItemNo = schemeKey[ProjectID.载波芯片ID测试];

                string result = data.Value?.Result.Trim() == "合格" ? "01" : "02";
                string value = data.Value.ResultValues.Split('^')[1];
                UpdateMeterInfoToPrductionControlSystem(meter.MD_Epitope.ToString(), meter.MD_BarCode, subItemNo, "", result, "", meter.Other5, batchNoUp);
            }
            return true;
        }


        /// <summary>
        /// <summary>
        /// 上传检定数据到天津数据库  先删除再插入
        /// </summary>
        /// <param name="pos">表的位置号</param>
        /// <param name="barCode">条形码</param>
        /// <param name="subNo">子项目编号</param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <param name="errInfo"></param>
        /// <param name="meterId"></param>
        /// <param name="batchNo">批次号</param>
        /// <returns></returns>
        public static bool UpdateMeterInfoToPrductionControlSystem(string pos, string barCode, string subNo, string data, string result, string errInfo, string meterId, string batchNo)
        {
            string Time = DateTime.Now.ToString();

            string sql = $"delete from M_VERIFY_RESULT where BATCH_NO = '{batchNo}' and BAR_CODE='{barCode}' and SUBITEM_NO='{subNo}'";
            SqlHelper.ExecuteNonQuery(sql);


            sql = $"insert into M_VERIFY_RESULT(BATCH_NO,POSITION,BAR_CODE,SUBITEM_NO,VERIFY_TIME,DATA_RESULT,CONCLUSION,ERROR_INFO,METER_ID) values('{batchNo.Trim()}','{pos.Trim()}','{barCode.Trim()}','{subNo.Trim()}','{Time.Trim()}','{data.Trim()}','{result.Trim()}','{errInfo.Trim()}','{meterId.Trim()}')";

            //LogHelper.WriteLog(sql);
            if (SqlHelper.ExecuteNonQuery(sql) <= 0)
            {
                return false;
            }
            return true;
        }

        //private MT_METER GetMt_meter(string barcode)
        //{
        //    string strSysNo;
        //    string strDetectTaskNo;

        //    //根据条码号取出工单号 根据条码号取出工单号
        //    string sql = "SELECT DETECT_TASK_NO FROM MT_DETECT_OUT_EQUIP WHERE BAR_CODE='" + barcode + "'order by  write_date desc";
        //    object o = ExecuteScalar(sql);
        //    if (o == null)
        //    {
        //        LogManager.AddMessage("不存在条码号为(" + barcode + ")的工单记录", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
        //        //("不存在条码号为(" + barcode + ")的工单记录");
        //        return null;
        //    }
        //    string detetTaskNo = o.ToString().Trim();

        //    //根据任务号查询 系统编号 和设备类型
        //    sql = string.Format(@"SELECT * FROM MT_DETECT_TASK T WHERE T.TASK_STATUS='21' AND T.DETECT_TASK_NO ='{0}'", detetTaskNo);

        //    DataTable dr = ExecuteReader(sql);
        //    if (dr.Rows.Count > 0)
        //    {
        //        strSysNo = dr.Rows[0]["SYS_NO"].ToString().Trim();
        //        //strSysEquipType = dr.Rows[0]["EQUIP_TYPE"].ToString().Trim();
        //        strDetectTaskNo = dr.Rows[0]["DETECT_TASK_NO"].ToString().Trim();
        //    }
        //    else
        //    {
        //        LogManager.AddMessage("不存在任务号为(" + detetTaskNo + ")的工单记录", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
        //        return null;
        //    }

        //    sql = "select * from mt_meter where bar_code='" + barcode.Trim() + "'";

        //    DataTable dr1 = ExecuteReader(sql);
        //    //if (dr1.Rows.Count > 0) return null;
        //    DataRow row = dr1.Rows[0];

        //    MT_METER model = new MT_METER();

        //    if (row["METER_ID"].ToString() != "")
        //    {
        //        model.METER_ID = row["METER_ID"].ToString();
        //    }
        //    model.BAR_CODE = row["BAR_CODE"].ToString();
        //    model.LOT_NO = row["LOT_NO"].ToString();
        //    model.ASSET_NO = row["ASSET_NO"].ToString();
        //    model.MADE_NO = row["MADE_NO"].ToString();
        //    model.SORT_CODE = row["SORT_CODE"].ToString();
        //    model.TYPE_CODE = row["TYPE_CODE"].ToString();
        //    model.MODEL_CODE = row["MODEL_CODE"].ToString();
        //    model.WIRING_MODE = row["WIRING_MODE"].ToString();
        //    model.VOLT_CODE = row["VOLT_CODE"].ToString();
        //    model.OVERLOAD_FACTOR = row["OVERLOAD_FACTOR"].ToString();
        //    model.AP_PRE_LEVEL_CODE = row["AP_PRE_LEVEL_CODE"].ToString();
        //    model.CONST_CODE = row["CONST_CODE"].ToString();
        //    model.RP_CONSTANT = row["RP_CONSTANT"].ToString();
        //    model.PULSE_CONSTANT_CODE = row["PULSE_CONSTANT_CODE"].ToString();
        //    model.FREQ_CODE = row["FREQ_CODE"].ToString();
        //    model.RATED_CURRENT = row["RATED_CURRENT"].ToString();
        //    model.CON_MODE = row["CON_MODE"].ToString();
        //    model.SOFT_VER = row["SOFT_VER"].ToString();
        //    model.HARD_VER = row["HARD_VER"].ToString();
        //    model.MT_DATECT_OUT_EQUIP.SYS_NO = strSysNo;
        //    model.MT_DATECT_OUT_EQUIP.DETECT_TASK_NO = strDetectTaskNo;

        //    return model;

        //}
        public bool UpdateCompleted()
        {
            return true;
        }

        public void UpdateInit()
        {

        }

        private static PowerWay GetGLFXFromString(string strValue)
        {
            PowerWay pw;
            switch (strValue)
            {
                case "正向有功":
                    pw = PowerWay.正向有功;
                    break;
                case "正向无功":
                    pw = PowerWay.正向无功;
                    break;
                case "反向有功":
                    pw = PowerWay.反向有功;
                    break;
                case "反向无功":
                    pw = PowerWay.反向无功;
                    break;
                case "第一象限无功":
                    pw = PowerWay.第一象限无功;
                    break;
                case "第二象限无功":
                    pw = PowerWay.第二象限无功;
                    break;
                case "第三象限无功":
                    pw = PowerWay.第三象限无功;
                    break;
                case "第四象限无功":
                    pw = PowerWay.第四象限无功;
                    break;
                default:
                    pw = PowerWay.正向有功;
                    break;
            }
            return pw;
        }
        private static Cus_PowerYuanJian GetYuanJianFromString(string value)
        {
            Cus_PowerYuanJian jy = Cus_PowerYuanJian.H;
            switch (value)
            {
                case "ABC":
                case "AC":
                    jy = Cus_PowerYuanJian.H;
                    break;
                case "A":
                    jy = Cus_PowerYuanJian.A;
                    break;
                case "B":
                    jy = Cus_PowerYuanJian.B;
                    break;
                case "C":
                    jy = Cus_PowerYuanJian.C;
                    break;
            }
            return jy;
        }

        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Key:schemeId, value[Key:itemKey, value:subItemNo]
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> schemeDic = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Key:BatchNo,Value:SchemeId
        /// </summary>
        private Dictionary<string, string> batchDic = new Dictionary<string, string>();


        private bool GetSchemeDetail(string batchNo)
        {
            if (batchDic.ContainsKey(batchNo))
                return true;

            if (g_DicPCodeTable == null)
                GetDicPCodeTable();


            string sql = $@"select schema_id from M_VERIFY_MAIN where BATCH_NO='{batchNo}'";
            DataTable dt = SqlHelper.Query(sql);

            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show($"M_VERIFY_MAIN表中不存在批次号为({batchNo})的方案信息");
                return false;
            }
            string schemaId = dt.Rows[0]["SCHEMA_ID"].ToString().Trim();
            schemaId = schemaId.Replace('/', '_');
            if (string.IsNullOrEmpty(schemaId)) return false;

            if (schemeDic.ContainsKey(schemaId)) return true;

            sql = $"select * from M_METER_VERIFY_SCHEMA where SCHEMA_ID = '{schemaId}' order by ORDER_ID,ORDER_SUBID";
            dt = SqlHelper.Query(sql);

            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show($"M_METER_VERIFY_SCHEMA表中不存在方案编号为({schemaId})的方案信息");
                LogManager.AddMessage($"M_METER_VERIFY_SCHEMA表中不存在方案编号为({schemaId})的方案信息", level: EnumLevel.Error);
                return false;
            }

            int bphNum = 0; //记录不平衡点个数
            int TmpFzdl = 0;//HQ20250408
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // 大项编号
                string itemNo = dt.Rows[i]["ITEM_NO"].ToString().Trim();
                // 小项编号
                string subItemNo = dt.Rows[i]["SUBITEM_NO"].ToString().Trim();
                string paramList = dt.Rows[i]["PARAMS_LIST"].ToString().Trim();

                string[] paraArr = paramList.Split('[');
                if (itemNo == "02" && paraArr.Length <= 2) continue;
                if (itemNo == "03" && paraArr.Length <= 2) continue;
                if (itemNo == "04" && paraArr.Length <= 2) continue;
                if (itemNo == "05" && paraArr.Length <= 2) continue;
                if (itemNo == "06" && paraArr.Length <= 2) continue;
                if (itemNo == "0812" && paraArr.Length <= 2) continue;
                if (itemNo == "03" && paraArr.Length <= 2) continue;
                if (itemNo == "18" && paraArr.Length <= 2) continue;//HQ20250408
                if (itemNo == "19" && paraArr.Length <= 2) continue;//HQ20250408
                if (itemNo == "20" && paraArr.Length <= 2) continue;//HQ20250408
                switch (itemNo)
                {
                    case "0802"://"通讯模块测试":
                        dic.Add(ProjectID.通讯测试, subItemNo);
                        //File.WriteInIString(inifile, "Data", ProjectID.通讯测试, subItemNo);
                        break;
                    case "0803"://"通讯模块测试":
                        dic.Add(ProjectID.通讯测试, subItemNo);
                        //File.WriteInIString(inifile, "Data", ProjectID.通讯测试, subItemNo);
                        break;
                    case "外观、标志检查":
                        dic.Add(ProjectID.外观检查, subItemNo);
                        //File.WriteInIString(inifile, "Data", ProjectID.外观检查, subItemNo);
                        break;
                    case "03":// "起动试验":
                        dic.Add(ProjectID.起动试验, subItemNo);
                        //File.WriteInIString(inifile, "Data", ProjectID.起动试验, subItemNo);
                        break;
                    case "02"://"潜动试验":
                        dic.Add(ProjectID.潜动试验, subItemNo);
                        //File.WriteInIString(inifile, "Data", ProjectID.潜动试验, subItemNo);
                        break;
                    case "04":// "基本误差":
                        if (paraArr[9].Substring(0, 1) == "1")
                        {

                            if (paraArr == null || paraArr.Length < 1) continue;

                            string strGlfx = g_DicPCodeTable["meterBwpf"].GetPName(paraArr[1].Substring(0, 2));
                            PowerWay glfx = GetGLFXFromString(strGlfx);

                            string strYj;
                            if (paraArr[2].IndexOf("分") != -1)
                                strYj = paraArr[2].Substring(1, 1);
                            else
                                strYj = " ";
                            Cus_PowerYuanJian yj = GetYuanJianFromString(strYj);

                            string ibX = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[4].Substring(0, 2));
                            ibX = ibX.Replace("In", "Ib");

                            if (ibX == "Ib")
                                ibX = "1.0Ib";

                            string glys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));

                            //File.WriteInIString(inifile, "Data", $"{ProjectID.标准偏差试验}{glfx}{yj}{glys}{ibX}", subItemNo);
                            dic.Add($"{ProjectID.标准偏差试验}{glfx}{yj}{glys}{ibX}", subItemNo);
                        }
                        else
                        {

                            string fx = g_DicPCodeTable["meterBwpf"].GetPName(paraArr[1].Substring(0, 2));
                            PowerWay glfx = GetGLFXFromString(fx);

                            string str = "";
                            if (paraArr[2].IndexOf("分") != -1)
                                str = paraArr[2].Substring(1, 1);
                            else if (paraArr[2].IndexOf("元不平衡") > 0)
                                str = paraArr[2].Substring(0, 1);
                            Cus_PowerYuanJian yj = GetYuanJianFromString(str);

                            string glys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));
                            string ibX = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[4].Substring(0, 2));

                            ibX = ibX.Replace("In", "Ib");

                            if (ibX == "Ib")
                                ibX = "1.0Ib";

                            // 记录不平衡
                            if (paraArr[2].IndexOf("元不平衡") > 0)
                            {
                                string str1 = $"{ProjectID.基本误差试验}{glfx}{Cus_PowerYuanJian.H}{glys}{ibX}";
                                string str2 = $"{ProjectID.基本误差试验}{glfx}{yj}{glys}{ibX}";
                                //File.WriteInIString(inifile, "DataBPH", $"DataBPH_Point{bphNum}", $"{str1}|{str2}|{subItemNo}"); // H元_分元_Key
                                dic.Add($"DataBPH_Point{bphNum}", $"{str1}|{str2}|{subItemNo}");

                                bphNum++;
                                continue;

                            }

                            //File.WriteInIString(inifile, "Data", $"{ProjectID.基本误差试验}{glfx}{yj}{glys}{ibX}", subItemNo);
                            dic.Add($"{ProjectID.基本误差试验}{glfx}{yj}{glys}{ibX}", subItemNo);
                            break;

                        }

                        break;
                    case "11":// "常数试验":
                        {
                            string strXIb = "2.0Ib";

                            string str = g_DicPCodeTable["meterBwpf"].GetPName(paraArr[1].Substring(0, 2));
                            PowerWay glfx = GetGLFXFromString(str);

                            string glys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[2].Substring(0, 2));
                            //File.WriteInIString(inifile, "Data", $"{ProjectID.电能表常数试验}{glfx}{Cus_PowerYuanJian.H}{glys}{strXIb}", subItemNo);
                            dic.Add($"{ProjectID.电能表常数试验}{glfx}{Cus_PowerYuanJian.H}{glys}{strXIb}", subItemNo);
                        }
                        break;
                    case "影响量试验":

                        break;
                    case "10":// "载波通信性能试验":
                        //File.WriteInIString(inifile, "Data", ProjectID.载波通信测试, subItemNo);
                        dic.Add(ProjectID.载波通信测试, subItemNo);
                        break;
                    case "0812"://"由电源供电的时钟试验":
                        //File.WriteInIString(inifile, "Data", ProjectID.日计时误差, subItemNo);
                        dic.Add(ProjectID.日计时误差, subItemNo);
                        break;
                    case "15":// "时钟示值误差":
                        //File.WriteInIString(inifile, "Data", ProjectID.时钟示值误差, subItemNo);
                        dic.Add(ProjectID.时钟示值误差, subItemNo);
                        break;
                    case "05":// "电子指示显示器电能示值组合误差":
                        //File.WriteInIString(inifile, "Data", $"{ProjectID.电能示值组合误差}{paraArr[4].Substring(0, 2)}:{paraArr[4].Substring(2, 2)}({paraArr[3].Substring(4, 1)})", subItemNo);
                        dic.Add($"{ProjectID.电能示值组合误差}{paraArr[4].Substring(0, 2)}:{paraArr[4].Substring(2, 2)}({paraArr[3].Substring(4, 1)})", subItemNo);
                        break;
                    case "0811"://"校时功能":
                        //File.WriteInIString(inifile, "Data", ProjectID.GPS对时, subItemNo);
                        dic.Add(ProjectID.GPS对时, subItemNo);
                        break;
                    case "0805"://"需量示值误差":
                        //File.WriteInIString(inifile, "Data", ProjectID.需量示值误差, subItemNo);
                        {
                            // string strXIb = "1.0Ib";
                            string strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[6].Substring(0, 2));
                            //File.WriteInIString(inifile, "Data", $"{ProjectID.电能表常数试验}{glfx}{Cus_PowerYuanJian.H}{glys}{strXIb}", subItemNo);
                            dic.Add($"{ProjectID.需量示值误差}{strXIb}", subItemNo);
                        }

                        break;
                    case "18"://"误差变差"//HQ20250408

                        {
                            string glys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));
                            dic.Add($"{ProjectID.误差变差}{glys}", subItemNo);
                        }

                        break;
                    case "19"://"误差一致性"//HQ20250408

                        {
                            string strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[2].Substring(0, 2));
                            string glys = g_DicPCodeTable["detectErrPF"].GetPName(paraArr[3].Substring(0, 2));

                            dic.Add($"{ProjectID.误差一致性}{strXIb}{glys}", subItemNo);
                        }

                        break;
                    case "20"://"负载电流升降变差"//HQ20250408

                        {
                            string strXIb = g_DicPCodeTable["meterLdCt"].GetPName(paraArr[2].Substring(0, 2));
                            if (TmpFzdl < 3)
                            {
                                dic.Add($"{ProjectID.负载电流升将变差}{strXIb}", subItemNo);
                                TmpFzdl++;
                            }

                        }

                        break;

                    case "9":// "安全认证试验":
                        //File.WriteInIString(inifile, "Data", ProjectID.身份认证, subItemNo);
                        dic.Add(ProjectID.身份认证, subItemNo);
                        break;
                    case "13":// "控制功能":
                        //File.WriteInIString(inifile, "Data", ProjectID.远程控制, subItemNo);
                        dic.Add(ProjectID.远程控制, subItemNo);
                        break;
                    case "12":// "密钥更新试验":
                        //File.WriteInIString(inifile, "Data", ProjectID.密钥更新, subItemNo);
                        dic.Add(ProjectID.密钥更新, subItemNo);
                        break;
                    case "14":// "剩余电量递减准确度试验":
                        //File.WriteInIString(inifile, "Data", ProjectID.剩余电量递减准确度, subItemNo);
                        dic.Add(ProjectID.剩余电量递减准确度, subItemNo);
                        break;
                    case "21":// 芯片ID认证
                        dic.Add(ProjectID.载波芯片ID测试, subItemNo);
                        break;


                    case "28":// "读取电量1":
                        //File.WriteInIString(inifile, "Data", ProjectID.通讯协议检查试验2, subItemNo);
                        dic.Add(ProjectID.通讯协议检查试验2, subItemNo);
                        break;
                    default:
                        break;
                }

            }

            batchDic.Add(batchNo, schemaId);
            schemeDic.Add(schemaId, dic);

            return true;
        }


    }


    /// <summary>
    /// 不平衡结构体
    /// </summary>
    public class BphItem
    {
        /// <summary>
        /// 合元Key
        /// </summary>
        public string KeyH { get; set; }

        /// <summary>
        /// 分元Key
        /// </summary>
        public string KeyF { get; set; }

        /// <summary>
        /// 方案Key
        /// </summary>
        public string SubItem { get; set; }

        /// <summary>
        /// 合元误差：平均值,化整值,误差集合
        /// </summary>
        public string ValueH { get; set; }

        /// <summary>
        /// 分元误差：平均值,化整值,误差集合
        /// </summary>
        public string ValueF { get; set; }

        /// <summary>
        /// 不平衡差值
        /// </summary>
        public string Err { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }

    }
}
