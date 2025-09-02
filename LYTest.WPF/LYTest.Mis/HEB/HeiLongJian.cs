using CLDC.CLAT.Comm.InternalCommClient;
using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.DAL.Config;
using LYTest.MeterProtocol;
using LYTest.Mis.Common;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LYTest.Mis.HEB
{
    public class HeiLongJian : OracleHelper, IMis
    {
        public IServiceProxy Client = null;
        public HeiLongJianDownHelper downHelpe = new HeiLongJianDownHelper();

        int StationId = 0;
        public bool isConnected = false;
        string xml = string.Empty;
        readonly Dictionary<string, string> XmlData = new Dictionary<string, string>();

        public int systenID = 0;
        public string TaskID = "";

        public delegate void RunningVerify();

        public event RunningVerify RunningVerifyEvent;
        public void RunningVerifyMethod()
        {
            RunningVerifyEvent?.Invoke();
        }




        public void StartThreadRead(ThreadStart StartFun)
        {
            Thread th = new Thread(StartFun)
            {
                IsBackground = true
            };
            th.Start();

        }

        public HeiLongJian()
        {

        }

        public HeiLongJian(string ip, int port, string dataSource, string userId, string pwd, string url)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;

            Client = ServiceProxyFactory.CreateServiceProxy();

            ConnectServer();

        }

        public void ConnectServer()
        {
            this.StationId = int.Parse(ConfigHelper.Instance.SetControl_BenthNo);
            isConnected = Client.Initialize(this.WebServiceURL, StationId, 20);
            Client.CheckStartReceived += Client_CheckStartReceived;


            if (!isConnected)
            {
                LogManager.AddMessage("初始化连接失败\r\n");
            }
            else
            {
                LogManager.AddMessage("初始化连接成功\r\n");

            }
            StartThreadRead(CheckConnected);//检查是否连接

        }

        private void Client_CheckStartReceived(object sender, CLDC.CLAT.Comm.InternalCommClient.ClientEventArgs.CheckStartEventArgs e)
        {
            downHelpe.TrayNumList = e.TrayNoList;
            downHelpe.systemID = this.StationId;
            downHelpe.codeType = 1;
            downHelpe.applyType = false;

            //应答
            if (e.TrayNoList != null && e.TrayNoList.Count > 0)
            {
                e.ReturnInfo = DataConvertHelperCls.CreateReturnXml(true, "");

            }

            RunningVerifyMethod();

        }




        private bool ApplyServerTime()
        {
            bool IsReturn = false;
            DateTime nowlocal = DateTime.Now;
            if (Client.IsConnected)
            {
                string returnInfo = Client.ApplyServerDate(StationId, true);
                string ParseDate = DataConvertHelperCls.ParseDateTime(returnInfo).Replace("/", "-");
                LogManager.AddMessage("收到服务器息" + ParseDate);
                if (!string.IsNullOrEmpty(ParseDate))
                {
                    if (DateTime.TryParse(ParseDate, out DateTime now))
                    {
                        if (Math.Abs((now - nowlocal).Seconds) > 5)
                        {
                            LogManager.AddMessage($"检定出现异常！时间差大于5秒，本地时间：{nowlocal}，服务器时间：{now}",
                                EnumLogSource.用户操作日志, EnumLevel.Error);
                        }
                        LogManager.AddMessage("解析时间正确:" + now.ToString());
                        IsReturn = true;
                    }
                }

            }
            return IsReturn;
        }

        public void CheckConnected()
        {
            int ApplyTime = 1;
            int waitTime = 0;
            while (true)
            {

                if (Client.IsConnected)
                {
                    if (ApplyTime > 0)
                    {
                        if (ApplyServerTime())//设置系统时间
                        {

                            ApplyTime--;

                        }
                    }
                }
                else
                {
                    if (ApplyTime <= 0)
                    {
                        ApplyTime++;

                    }
                }
                if (waitTime > 1200)   //每隔一小时自动对时一次
                {
                    ApplyServerTime();
                    waitTime = 0;
                }
                waitTime++;
                Thread.Sleep(5000);
            }
        }

        #region 下载表信息

        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            return false;
        }
        /// <summary>
        /// 下载表的信息
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        public bool Down(object Data, ref List<TestMeterInfo> meter)
        {
            //下载电表信息
            HeiLongJianDownHelper downData = (HeiLongJianDownHelper)Data;
            //台体编号, 托盘编号列表，1,true
            string xml = Client.ApplyMeterInfo(downData.systemID, downData.TrayNumList, downData.codeType, true);
            //List<TestMeterInfo> testMeters = new List<TestMeterInfo>();
            if (!ParsMeterXmlInfo(xml, out _, out string err))
            {
                LogManager.AddMessage("解析xml失败:\r\n" + err);
                return false;
            }

            meter = CalculateMeterId(new Dictionary<string, List<TestMeterInfo>>());    //解析表位号
            //解析xml

            return true;
        }

        //解析电表信息
        public static bool ParsMeterXmlInfo(string Meterxml, out Dictionary<string, List<TestMeterInfo>> MeterInfos, out string ErrInfo)
        {
            ErrInfo = string.Empty;
            bool Result = true;
            MeterInfos = new Dictionary<string, List<TestMeterInfo>>();
            try
            {
                XDocument doc = XDocument.Parse(Meterxml);
                string resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
                if (resultFlag == "1")
                {
                    foreach (XElement element in doc.Element("DATA").Element("METER_INFO").Elements("CONTAINER"))
                    {
                        List<TestMeterInfo> meters = new List<TestMeterInfo>();
                        string code = element.Element("CODE").Value;
                        if (!string.IsNullOrEmpty(code))
                        {

                            if (!MeterInfos.Keys.Contains(element.Element("CODE").Value))
                            {
                                MeterInfos.Add(code, meters);
                            }
                            foreach (XElement e in element.Elements("METER"))
                            {
                                if (string.IsNullOrEmpty(e.Element("BAR_CODE").Value) || e.Element("BAR_CODE").Value == "") continue;
                                TestMeterInfo meter = new TestMeterInfo
                                {
                                    MD_UB = float.Parse(e.Element("VOLTAGE").Value),//电压
                                    MD_UA = e.Element("CURRENT").Value,//电流
                                    MD_Frequency = int.Parse(e.Element("FREQUENCY").Value),//频率
                                    YaoJianYn = e.Element("IS_CHECK").Value == "1"
                                };

                                if (e.Element("CHECK_TYPE").Value == "0")//全j
                                {
                                    meter.MD_TestType = "到货全检";
                                }
                                else
                                {
                                    meter.MD_TestType = "质量抽检";
                                }
                                meter.MD_TestModel = "首检";

                                if (e.Element("LINK_TYPE").Value == "1")
                                {
                                    meter.MD_WiringMode = "单相";
                                }
                                else if (e.Element("LINK_TYPE").Value == "2")
                                {
                                    meter.MD_WiringMode = "三相三线";
                                }
                                else if (e.Element("LINK_TYPE").Value == "3")
                                {
                                    meter.MD_WiringMode = "三相四线";
                                }

                                if (e.Element("ACCESS_TYPE").Value == "1")
                                {
                                    meter.MD_ConnectionFlag = "直接式";
                                }
                                else
                                {
                                    meter.MD_ConnectionFlag = "互感式";

                                }
                                meter.FKType = 0;//费控类型
                                meter.MD_JJGC = "JJG596-2012";
                                if (meter.MD_UA.IndexOf("-") != -1)
                                {
                                    meter.MD_JJGC = "IR46";
                                }

                                meter.MD_BarCode = e.Element("BAR_CODE").Value ?? string.Empty;
                                //是否要检
                                if (string.IsNullOrEmpty(meter.MD_BarCode))
                                {
                                    meter.YaoJianYn = false;
                                }
                                else
                                {
                                    meter.YaoJianYn = true;
                                }
                                meter.MD_AssetNo = meter.MD_BarCode;//资产编号
                                meter.MD_MeterType = "电子式";
                                //METER_TYPE
                                //METER_CONST
                                meter.MD_Constant = e.Element("METER_CONST").Value;//常数

                                //if (e.Element("METER_RP_CONST").Value != null && e.Element("METER_RP_CONST").Value != "")
                                //{
                                //    meter.MD_Constant += $"({e.Element("METER_RP_CONST").Value})";
                                //}
                                meter.MD_Grane = e.Element("METER_CLASS").Value;//等级
                                if (e.Element("METER_RP_CLASS").Value != null && e.Element("METER_RP_CLASS").Value != "")
                                {
                                    meter.MD_Grane += $"({e.Element("METER_RP_CLASS").Value})";
                                }

                                meter.MD_MeterModel = e.Element("METER_MODE").Value;  //表型号METER_MODE
                                                                                      //
                                meter.MD_ProtocolName = "CDLT698";//通讯协议
                                if (e.Element("COMM_PROT_CODE").Value == "1")
                                {
                                    meter.MD_ProtocolName = "CDLT645-2013";
                                }

                                //meter.BatchId = e.Element("BATCH_ID").Value ?? string.Empty;
                                //meter.CarrierParam = e.Element("CARRIAER_PARAM").Value ?? string.Empty;
                                meter.MD_CarrName = "标准载波";

                                //if (e.Element("CARRIER_TYPE").Value == "0")
                                //{
                                //    meter.MD_CarrName = "标准载波";
                                //}
                                //if (e.Element("CARRIER_TYPE").Value == "1")
                                //{
                                //    meter.MD_CarrName = "晓程";
                                //}
                                //if (e.Element("CARRIER_TYPE").Value == "2")
                                //{
                                //    meter.MD_CarrName = "东软";
                                //}
                                //if (e.Element("CARRIER_TYPE").Value == "3")
                                //{
                                //    meter.MD_CarrName = "鼎信";
                                //}
                                //if (e.Element("CARRIER_TYPE").Value == "4")
                                //{
                                //    meter.MD_CarrName = "瑞士康";
                                //}

                                if (e.Element("CHECK_RESULT").Value == "-1")
                                {
                                    meter.Result = "待检";
                                }
                                if (e.Element("CHECK_RESULT").Value == "0")
                                {
                                    meter.Result = ConstHelper.不合格;
                                }
                                if (e.Element("CHECK_RESULT").Value == "1")
                                {
                                    meter.Result = ConstHelper.合格;
                                }



                                //meter.MD_UA = e.Element("CURRENT").Value;
                                //meter.MD_Constant = string.Format(e.Element("METER_CONST").Value + "(" + e.Element("METER_RP_CONST") + ")");
                                //meter.MD_Grane = string.Format(e.Element("METER_CLASS").Value + "(" + e.Element("METER_RP_CLASS") + ")");
                                //meter.MD_Frequency = int.Parse(e.Element("FREQUENCY").Value);
                                //meter.GuidId = e.Element("GUID_ID").Value;
                                //meter.Insulated = e.Element("INSULATED").Value == "1" ? true : false;



                                //meter.MD_TaskNo = "";

                                //string commPortCode = e.Element("COMM_PROT_CODE").Value;
                                //if (string.IsNullOrEmpty(commPortCode))
                                //{
                                //    meter.EmCommProtCode = EmCommProtCode.Procotol645;
                                //}
                                //else
                                //{
                                //    meter.EmCommProtCode = (EmCommProtCode)(Convert.ToInt32(commPortCode));
                                //}
                                meter.MD_PostalAddress = e.Element("METER_ADDRESS").Value ?? string.Empty;
                                meter.Meter_TrayTotalID = Convert.ToInt32(e.Element("METER_ID").Value);  //在托盘内的iD--因为前三和后三的算法不一致
                                //meter.ACPolar = (EmPolar)(0);
                                //meter.IsTomis = Convert.ToInt32(e.Element("ISTOMIS").Value);
                                //meter.CommParam = e.Element("COMM_PARAS").Value;
                                //meter.MD_MeterModel = e.Element("METER_MODE").Value;
                                //meter.MeterSort = e.Element("METER_SORT").Value;
                                //meter.MD_MeterType = e.Element("METER_TYPE").Value ?? string.Empty;
                                //meter.ReCheckCount = Convert.ToInt32(e.Element("RECHECK_COUNT").Value);
                                //meter.SaveDate = e.Element("SAVE_DATE").Value;
                                //meter.SchemeId = e.Element("SCHEME_ID").Value;
                                meter.MD_TaskNo = e.Element("TASK_NO").Value ?? string.Empty;
                                //meter.MD_UB = float.Parse(e.Element("VOLTAGE").Value);
                                //meter.Type = e.Element("EQUIP_CATEG").Value;
                                //string closeMode = e.Element("METER_CLOSE_MODE").Value;
                                //if (string.IsNullOrEmpty(closeMode))
                                //{
                                //    meter.CloseMode = EmCloseMode.Null;
                                //}
                                //else
                                //{
                                //    meter.CloseMode = (EmCloseMode)Convert.ToInt32(closeMode);//内置，外置
                                //}

                                meters.Add(meter);
                            }

                        }
                    }
                }
                else
                {
                    ErrInfo = doc.Element("DATA").Element("ERROR_INFO").Value ?? string.Empty;
                    Result = false;
                }
            }
            catch (Exception e)
            {
                ErrInfo = e.Message;
                Result = false;
            }
            return Result;
        }

        //public bool ParsMeterXmlInfo(string Meterxml, out Dictionary<string, List<TestMeterInfo>> dicMeterInfo, out string ErrInfo)
        //{
        //    ErrInfo = string.Empty;
        //    XDocument doc = null;
        //    bool Result = true;
        //    string resultFlag = string.Empty;
        //    // MeterInfos = new Dictionary<string, List<ClMeterInfo_Local>>();
        //    MeterInfos = new List<TestMeterInfo>();
        //    try
        //    {
        //        doc = XDocument.Parse(Meterxml);
        //        resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
        //        if (resultFlag == "1")
        //        {
        //            foreach (XElement element in doc.Element("DATA").Element("METER_INFO").Elements("CONTAINER"))
        //            {
        //                string code = element.Element("CODE").Value;
        //                if (!string.IsNullOrEmpty(code))
        //                {
        //                    foreach (XElement e in element.Elements("METER"))
        //                    {
        //                        if (string.IsNullOrEmpty(e.Element("BAR_CODE").Value) || e.Element("BAR_CODE").Value == "") continue;
        //                        //ClMeterInfo_Local meter = new ClMeterInfo_Local();
        //                        TestMeterInfo meter = new TestMeterInfo();
        //                        if (e.Element("ACCESS_TYPE").Value == "1")
        //                        {
        //                            meter.MD_ConnectionFlag = "互感式";
        //                        }
        //                        else
        //                        {
        //                            meter.MD_ConnectionFlag = "直接式";

        //                        }

        //                        meter.MD_BarCode = e.Element("BAR_CODE").Value ?? string.Empty;
        //                        //meter.BatchId = e.Element("BATCH_ID").Value ?? string.Empty;
        //                        //meter.CarrierParam = e.Element("CARRIAER_PARAM").Value ?? string.Empty;

        //                        if (e.Element("CARRIER_TYPE").Value == "0")
        //                        {
        //                            meter.MD_CarrName = "非载波";
        //                        }
        //                        if (e.Element("CARRIER_TYPE").Value == "1")
        //                        {
        //                            meter.MD_CarrName = "晓程";
        //                        }
        //                        if (e.Element("CARRIER_TYPE").Value == "2")
        //                        {
        //                            meter.MD_CarrName = "东软";
        //                        }
        //                        if (e.Element("CARRIER_TYPE").Value == "3")
        //                        {
        //                            meter.MD_CarrName = "鼎信";
        //                        }
        //                        if (e.Element("CARRIER_TYPE").Value == "4")
        //                        {
        //                            meter.MD_CarrName = "瑞士康";
        //                        }

        //                        if (e.Element("CHECK_RESULT").Value == "-1")
        //                        {
        //                            meter.Result = "待检";
        //                        }
        //                        if (e.Element("CHECK_RESULT").Value == "0")
        //                        {
        //                            meter.Result = "合格";
        //                        }
        //                        if (e.Element("CHECK_RESULT").Value == "1")
        //                        {
        //                            meter.Result = "不合格";
        //                        }

        //                        if (e.Element("CHECK_TYPE").Value == "0")
        //                        {
        //                            meter.MD_TestType = "全检";
        //                        }
        //                        if (e.Element("CHECK_TYPE").Value == "1")
        //                        {
        //                            meter.MD_TestType = "抽检";
        //                        }


        //                        meter.MD_UA = e.Element("CURRENT").Value;
        //                        meter.MD_Constant = string.Format(e.Element("METER_CONST").Value + "(" + e.Element("METER_RP_CONST") + ")");
        //                        meter.MD_Grane = string.Format(e.Element("METER_CLASS").Value + "(" + e.Element("METER_RP_CLASS") + ")");
        //                        meter.MD_Frequency = int.Parse(e.Element("FREQUENCY").Value);
        //                        //meter.GuidId = e.Element("GUID_ID").Value;
        //                        //meter.Insulated = e.Element("INSULATED").Value == "1" ? true : false;
        //                        meter.YaoJianYn = e.Element("IS_CHECK").Value == "1" ? true : false; ;

        //                        if (e.Element("LINK_TYPE").Value == "1")
        //                        {
        //                            meter.MD_WiringMode = "单相";
        //                        }
        //                        if (e.Element("LINK_TYPE").Value == "2")
        //                        {
        //                            meter.MD_WiringMode = "三相三线";
        //                        }
        //                        if (e.Element("LINK_TYPE").Value == "3")
        //                        {
        //                            meter.MD_WiringMode = "三相四线";
        //                        }

        //                        //meter.MD_TaskNo = "";

        //                        //string commPortCode = e.Element("COMM_PROT_CODE").Value;
        //                        //if (string.IsNullOrEmpty(commPortCode))
        //                        //{
        //                        //    meter.EmCommProtCode = EmCommProtCode.Procotol645;
        //                        //}
        //                        //else
        //                        //{
        //                        //    meter.EmCommProtCode = (EmCommProtCode)(Convert.ToInt32(commPortCode));
        //                        //}
        //                        meter.MD_PostalAddress = e.Element("METER_ADDRESS").Value ?? string.Empty;
        //                        meter.Meter_TrayTotalID = Convert.ToInt32(e.Element("METER_ID").Value);  //在托盘内的iD--因为前三和后三的算法不一致
        //                        //meter.ACPolar = (EmPolar)(0);
        //                        //meter.IsTomis = Convert.ToInt32(e.Element("ISTOMIS").Value);
        //                        //meter.CommParam = e.Element("COMM_PARAS").Value;
        //                        meter.MD_MeterModel = e.Element("METER_MODE").Value;
        //                        //meter.MeterSort = e.Element("METER_SORT").Value;
        //                        //meter.MD_MeterType = e.Element("METER_TYPE").Value ?? string.Empty;
        //                        //meter.ReCheckCount = Convert.ToInt32(e.Element("RECHECK_COUNT").Value);
        //                        //meter.SaveDate = e.Element("SAVE_DATE").Value;
        //                        //meter.SchemeId = e.Element("SCHEME_ID").Value;
        //                        meter.MD_TaskNo = e.Element("TASK_NO").Value ?? string.Empty;
        //                        meter.MD_UB = float.Parse(e.Element("VOLTAGE").Value);
        //                        //meter.Type = e.Element("EQUIP_CATEG").Value;
        //                        //string closeMode = e.Element("METER_CLOSE_MODE").Value;
        //                        //if (string.IsNullOrEmpty(closeMode))
        //                        //{
        //                        //    meter.CloseMode = EmCloseMode.Null;
        //                        //}
        //                        //else
        //                        //{
        //                        //    meter.CloseMode = (EmCloseMode)Convert.ToInt32(closeMode);//内置，外置
        //                        //}
        //                        MeterInfos.Add(meter);
        //                    }

        //                }
        //            }
        //        }
        //        else
        //        {
        //            ErrInfo = doc.Element("DATA").Element("ERROR_INFO").Value ?? string.Empty;
        //            Result = false;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ErrInfo = e.Message;
        //        Result = false;
        //    }
        //    return Result;
        //}

        public List<TestMeterInfo> CalculateMeterId(Dictionary<string, List<TestMeterInfo>> dicMeterInfo)
        {
            List<TestMeterInfo> lsmeter = new List<TestMeterInfo>();
            for (int i = 0; i < dicMeterInfo.Count; i++)
            {
                //SendLogMsg(string.Format("托盘编号：{0}", dicMeterInfo.ElementAt(i).Key));
                //LogManager.AddMessage(string.Format("托盘编号：{0}", dicMeterInfo.ElementAt(i).Key));

                //foreach (var item in dicMeterInfo)
                //{
                //TestMeterInfo meter = dicMeterInfo[i];
                //TODO 系统配置加个反向
                //if (true)
                //{
                //    //箭头方向
                //    dicMeterInfo[i].MD_Epitope = meter.Meter_TrayTotalID <= 3 ? (meter.Meter_TrayTotalID + 3 * i) : (meter.Meter_TrayTotalID + 6 * (10 - (i + 1)) + 3 * i);
                //}
                //else
                //{
                //    //反方向 
                //    dicMeterInfo[i].MD_Epitope = meter.Meter_TrayTotalID <= 3 ? (meter.Meter_TrayTotalID + 6 * (10 - 1) + 3 * (2 - (i + 1))) : (meter.Meter_TrayTotalID + 3 * (i - 1));
                //}

                if (dicMeterInfo.ElementAt(i).Value != null)
                {
                    for (int j = 0; j < dicMeterInfo.ElementAt(i).Value.Count; j++)
                    {
                        if (ConfigHelper.Instance.PipelineDirection == "正转")
                        {
                            //箭头方向
                            dicMeterInfo.ElementAt(i).Value[j].MD_Epitope = dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID <= 3 ? (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 3 * i) : (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 6 * (10 - (i + 1)) + 3 * i);
                        }
                        else
                        {
                            //反方向 
                            dicMeterInfo.ElementAt(i).Value[j].MD_Epitope = dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID <= 3 ? (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 6 * (10 - 1) + 3 * (2 - (i + 1))) : (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 3 * (i - 1));
                        }
                        lsmeter.Add(dicMeterInfo.ElementAt(i).Value[j]);
                    }
                }
                //}
                //if (dicMeterInfo.ElementAt(i).Value != null)
                //{
                //    for (int j = 0; j < dicMeterInfo.ElementAt(i).Value.Count; j++)
                //    {
                //        LogManager.AddMessage(string.Format("表条码：{0}的检定结论为：{1}", dicMeterInfo.ElementAt(i).Value.ElementAt(j).MD_BarCode,
                //              dicMeterInfo.ElementAt(i).Value.ElementAt(j).Result.ToString()));

                //        //TODO 系统配置加个反向
                //        if (true)
                //        {
                //            //箭头方向
                //            dicMeterInfo.ElementAt(i).Value[j].MD_Epitope = dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID <= 3 ? (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 3 * i) : (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 6 * (10 - (i + 1)) + 3 * i);
                //        }
                //        else
                //        {
                //            //反方向 
                //            dicMeterInfo.ElementAt(i).Value[j].MD_Epitope = dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID <= 3 ? (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 6 * (10 - 1) + 3 * (2 - (i + 1))) : (dicMeterInfo.ElementAt(i).Value[j].Meter_TrayTotalID + 3 * (i - 1));
                //        }
                //        tempMeterInfo.Add(dicMeterInfo.ElementAt(i).Value[j]);
                //    }
                //}
            }
            //List<string> l = new List<string>();
            //for (int i = 0; i < lsmeter.Count; i++)
            //{
            //    l.Add(lsmeter[i].MD_Epitope.ToString());
            //}

            lsmeter = lsmeter.OrderBy(m => m.MD_Epitope).ToList();//排序
            //l.Clear();
            //for (int i = 0; i < lsmeter.Count; i++)
            //{
            //    l.Add(lsmeter[i].MD_Epitope.ToString());
            //}                                                                  //if (!AddMeterInfo(tempMeterInfo)) { WorkThreadIsStart = false; }
            return lsmeter;                                                //return LstMeter_Info.Count > 0 ? LstMeter_Info[0].TaskId : "";
        }


        #endregion



        /// <summary>
        /// 下载方案
        /// </summary>
        /// <param name="barcode">这个当成台体号</param>
        /// <param name="schemeName">这个任务号</param>
        /// <param name="Schema"></param>
        /// <returns></returns>
        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {


            //获取方案信息  0-任务类型  1-方案编号类型
            string schemeInfos = Client.ApplySchemeInfo(systenID, TaskID, 0, true);
            //由于方案没有通信检测试验，所以暂时换别的方案
            //string schemeInfos = Client.ApplySchemeInfo(systenID, "20191109145832", 1, true);



            //string str = schemeInfos.ToString();
            //解析方法信息。创建方案
            //TODO 芯片ID需要时时上传
            //密钥更新之前判断电量是否是0，0的情况才能密钥更新
            schemeName = "";
            Schema = new Dictionary<string, SchemaNode>();
            XDocument doc = XDocument.Parse(schemeInfos);
            string resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
            try
            {
                if (resultFlag == "1")
                {
                    foreach (XElement e in doc.Element("DATA").Elements("BASE_SCHEME"))
                    {
                        schemeName = e.Element("SCHEME_NAME").Value;  //获取方案的名称
                    }
                    if (schemeName == "")
                    {
                        LogManager.AddMessage("下载方案失败,没有获取到方案名称");
                        return false;
                    }
                    string ID;
                    string values = "";
                    foreach (XElement e in doc.Element("DATA").Elements("SCHEME_INFO"))
                    {
                        string ItemName = GetVierifyName(e.Element("TRIAL_TYPE").Value); //获取到方案的编号
                        string[] PowerData = e.Element("SOURCE_PARAMS").Value.Split('|'); //控源参数
                        string[] TestData = e.Element("ITEM_PARAMS").Value.Split('|'); //检定参数
                        int SorData = int.Parse(e.Element("TRIAL_ORDER").Value); //项目编号  -顺序
                        string Itemid = e.Element("ITEM_ID").Value;
                        string IsAuto = "是";
                        double time = 0;
                        string[] SOURCE_PARAMS = null;
                        string ErrorDownLimit = string.Empty;
                        string ErrorUpLimit = string.Empty;
                        string TestElectricQuantity = string.Empty;
                        string ErrorLimitAFT = string.Empty;
                        string ErrorLimitBF = string.Empty;
                        string Glys = string.Empty;
                        string Code698 = string.Empty;
                        string Code645 = string.Empty;
                        string Name = string.Empty;
                        int Length = 0;
                        int DecimalPlace = 0;
                        string Format = string.Empty;
                        string Func = string.Empty;
                        string code = string.Empty;
                        MeterProtocalItem meter = null;




                        SOURCE_PARAMS = TestData[3].Split('/');
                        if (TestData[2] != "*" && TestData[2] != "")
                        {
                            time = int.Parse(TestData[2]) / 60;
                        }
                        if (TestData[1] != "0")
                            IsAuto = "否";
                        switch (ItemName)
                        {
                            case "起动试验":
                                ID = ProjectID.起动试验_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                //< SOURCE_PARAMS > 1 | 1 | 0 | *| 1.0 | *| 0.004Ib | 1.0 | *| *</ SOURCE_PARAMS >
                                //1功率方向-0正向有功--1反向有功| 合元|3实际电压--3为*或空用4|4电压倍数|5实际电流|6电流倍数|7功率元素|
                                //< ITEM_PARAMS > 5 | 0 | 120 | 2 | 1 </ ITEM_PARAMS >
                                ////1-4总尖峰平古-5没有|0规程1是自定义发|试验时间秒|4试验类型参数列表|
                                values = JoinValue(GetPowerDeiection(PowerData[1]), GetCurrentMultiple(PowerData[5], PowerData[6]), IsAuto, IsAuto, "否", time.ToString(), Itemid);
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "接线检查":  //RS485
                                ID = ProjectID.接线检查_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                values = Itemid;
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;

                                break;
                            case "潜动试验":
                                ID = ProjectID.潜动试验_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                string Voltage = float.Parse(GetVoltageMultiple(PowerData[3], PowerData[4])) * 100 + "%";
                                values = JoinValue(GetPowerDeiection(PowerData[1]), Voltage, "默认电流开路", IsAuto, "否", time.ToString(), Itemid);
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "误差":
                                ID = ProjectID.基本误差试验_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());


                                //< SOURCE_PARAMS > 1 | 1 | 0 | *| 1.0 | *| 0.2Ib | 0.5L | *| *</ SOURCE_PARAMS >

                                //< ITEM_PARAMS > 5 | *| *| -2.0 / +2.0 / 2 / 60 / 0 /#/2/3|1</ITEM_PARAMS>


                                ErrorDownLimit = SOURCE_PARAMS[0];//误差下线
                                ErrorUpLimit = SOURCE_PARAMS[1];//误差上限
                                string CheckNumber = SOURCE_PARAMS[2];//校验圈数
                                string ErrorLimitRatio = SOURCE_PARAMS[3];//误差限比
                                string ErrorNumber = SOURCE_PARAMS[6];//误差个数
                                string FNumber = SOURCE_PARAMS[7];//小数位数
                                Glys = PowerData[7];//功率因数


                                values = JoinValue("基本误差", GetPowerDeiection(PowerData[1]), ErrorDownLimit, ErrorUpLimit, CheckNumber, ErrorLimitRatio, ErrorNumber, FNumber, GetCurrentMultiple(PowerData[5], PowerData[6]), Glys, Itemid);
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "常数实验":
                                ID = ProjectID.电能表常数试验_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());

                                //< SOURCE_PARAMS > 1 | 1 | 0 | *| 1.0 | *| 1.0Imax | 1.0 | *| *</ SOURCE_PARAMS >
                                //< ITEM_PARAMS > 2 | 1 | *| 0.50 / -0.02 / 0.02 / 07:30 | 1 </ ITEM_PARAMS >


                                //电流倍数
                                string CurrentMultiple = PowerData[6];//电流倍数
                                if (CurrentMultiple.Equals("1.0Imax"))
                                {
                                    CurrentMultiple = "Imax";
                                }

                                Glys = PowerData[7];//功率因数
                                                    ////常数试验(走字试验方法类型)0 1-计数器脉冲法，2- 标准表法
                                string Type = GetConstantName(TestData[0]);

                                string Rate = GetRateName(TestData[0]);//费率
                                SOURCE_PARAMS = TestData[3].Split('/');
                                TestElectricQuantity = SOURCE_PARAMS[0];//试验电量
                                ErrorDownLimit = SOURCE_PARAMS[1];//误差下限
                                ErrorUpLimit = SOURCE_PARAMS[2];//误差上限
                                string RateStartTime = SOURCE_PARAMS[3];//费率起始时间

                                values = JoinValue(
                                    GetPowerDeiection(PowerData[1]),  //功率方向
                                    "H",                             //功率元件
                                    Glys,                             //功率因数
                                                                      //GetCurrentMultiple(PowerData[5], PowerData[6]),
                                    CurrentMultiple,                    //电流倍数
                                    Type,                               //走字试验方法类型
                                    Rate,                              //费率
                                    TestElectricQuantity,              //走字电量
                                    "0",                                //走字时间
                                    ErrorUpLimit,                       //误差上限   
                                    ErrorDownLimit,                      //误差下限
                                    RateStartTime,                      //费率起始时间
                                    Itemid                               //项目编号
                                    );
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;


                                break;
                            case "日计时":
                                ID = ProjectID.日计时误差_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());

                                //< SOURCE_PARAMS > 1 | 1 | 0 | *| 1.0 | 0 | *| 1.0 | *| *</ SOURCE_PARAMS >
                                //< ITEM_PARAMS > 5 | *| *| 500000 / 1 / 0.5 / 5 / 5 | 1 </ ITEM_PARAMS >

                                SOURCE_PARAMS = TestData[3].Split('/');
                                string StandTimeFrequency = SOURCE_PARAMS[0];//标准时钟频率
                                string CheckedTimeFrequency = SOURCE_PARAMS[1];//被检时钟频率
                                string ErrorLimit = SOURCE_PARAMS[2];//误差限
                                string TimePulseCount = SOURCE_PARAMS[3];//时钟脉冲个数（误差圈数）
                                string ErrorCount = SOURCE_PARAMS[4];//误差个数
                                values = JoinValue(ErrorLimit, ErrorCount,
                                    TimePulseCount, StandTimeFrequency, CheckedTimeFrequency, Itemid
                                  );
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "电能示值组合误差":
                                ID = ProjectID.电能示值组合误差_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                //< SOURCE_PARAMS > 1 | 1 | 0 | *| 1.0 | *| 1.0Imax | 1.0 | *| *</ SOURCE_PARAMS >
                                //< ITEM_PARAMS > 5 | *| *| 1 / 0.05 | 1 </ ITEM_PARAMS >

                                SOURCE_PARAMS = TestData[3].Split('/');
                                string TestMethod = SOURCE_PARAMS[0];//试验方式
                                if (TestMethod == "1")///如果是取表内时段
                                {

                                    TestElectricQuantity = SOURCE_PARAMS[1];//试验电量

                                    //费率数段/运行时间(分)/试验电量/是否24小时走字/项目编号
                                    values = JoinValue("", "", TestElectricQuantity, IsAuto, Itemid);


                                }
                                else if (TestMethod == "0") ///如果是取方案内时段
                                {
                                    IsAuto = "否";

                                    TestElectricQuantity = SOURCE_PARAMS[1];
                                    string RatePeriod = "";///时段信息
                                    TestElectricQuantity = SOURCE_PARAMS[1];//走字电量
                                    string xIb = string.Empty;
                                    //for (int i = 2; i < SOURCE_PARAMS.Count(); i++)
                                    //{
                                    //    if (i < SOURCE_PARAMS.Count())
                                    //    {
                                    //RatePeriod += SOURCE_PARAMS[i].Split(':')[1] + "|";

                                    foreach (var item in SOURCE_PARAMS)
                                    {
                                        if (item.StartsWith("1:"))
                                        {
                                            RatePeriod += GetRatePeriod(item.Split(':')[1], "尖") + ",";
                                        }
                                        if (item.StartsWith("2:"))
                                        {
                                            RatePeriod += GetRatePeriod(item.Split(':')[1], "峰") + ",";
                                        }
                                        if (item.StartsWith("3:"))
                                        {
                                            RatePeriod += GetRatePeriod(item.Split(':')[1], "平") + ",";
                                        }
                                        if (item.StartsWith("4:"))
                                        {
                                            RatePeriod += GetRatePeriod(item.Split(':')[1], "谷") + ",";

                                        }
                                    }
                                    //}
                                    //else
                                    //{
                                    //    RatePeriod += SOURCE_PARAMS[i].Split(':')[1];
                                    //}

                                    //}
                                    RatePeriod = RatePeriod.Substring(0, RatePeriod.LastIndexOf(','));



                                    //费率数段/运行时间(分)/走字电量/是否24小时走字/运行电量/项目编号
                                    values = JoinValue(RatePeriod, "",
                                        ConvertCurrent(GetCurrentMultiple(PowerData[5], PowerData[6]))
                                        , IsAuto,
                                        TestElectricQuantity,
                                        Itemid);
                                }

                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "通讯性能检测(HPLC)":
                                ID = ProjectID.载波通信测试;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());

                                //00010000
                                code = "00010000";//645的数据标识（标识编码）
                                meter = MeterProtocol.MeterProtocol.Select(code);
                                Code645 = meter.DataFlag645;//数据标识
                                Name = meter.Name;//数据项名称
                                values = JoinValue(Name, Code645, Itemid);


                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "芯片ID认证":
                                ID = ProjectID.载波芯片ID测试;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());

                                ////values = JoinValue(GetPowerDeiection(PowerData[0]), GetCurrentMultiple(PowerData[1], PowerData[2]), IsAuto, IsAuto, "否", time.ToString());

                                Schema[ID].SchemaNodeValue.Add(Itemid);
                                Schema[ID].SorData = SorData;
                                break;
                            case "时钟示值误差":
                                ID = ProjectID.时钟示值误差_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                //< SOURCE_PARAMS > 1 | 1 | 0 | *| 1.0 | *| 1.0Ib | 1.0 | *| *</ SOURCE_PARAMS >
                                //< ITEM_PARAMS > 5 | *| *| *| 1 </ ITEM_PARAMS >

                                ErrorLimitAFT = SOURCE_PARAMS[0];//校时后误差限
                                values = JoinValue(ErrorLimitAFT, Itemid);

                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "广播校时":
                                ID = ProjectID.GPS对时_黑龙江;

                                ErrorLimitBF = SOURCE_PARAMS[0];///校时前误差限
                                ErrorLimitAFT = SOURCE_PARAMS[1];///校时后误差限
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                values = JoinValue(ErrorLimitBF, ErrorLimitAFT, Itemid);
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "远程清零":
                                ID = ProjectID.电量清零_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());
                                values = JoinValue("0", Itemid);
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            case "参数验证":
                                ID = ProjectID.参数验证;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());


                                if (e.Element("ITEM_CHECKPARAMS") != null)
                                {
                                    bool isExist = false;
                                    foreach (XElement element in e.Element("ITEM_CHECKPARAMS").Elements("PARAM"))
                                    {
                                        string DATA_ID = (element.Element("PARAM_ID").Value);///子项编号
                                        string DATA_NAME = element.Element("PARAM_NAME").Value;//子项名称
                                        string DATA_TYPE = element.Element("PARAM_TYPE").Value;//参数类型
                                        string DATA_FORMAT = element.Element("PARAM_FORMAT").Value;//格式化字符
                                        string DATA_TAG = element.Element("PARAM_TAG").Value;//数据标识
                                        string DATA_UNIT = element.Element("PARAM_UNIT").Value;//单位
                                        string DATA_INTERCEPT = element.Element("PARAM_INTERCEPT_MODE").Value;//截取方式
                                        string DATA_ISREAD = element.Element("PARAM_OPERTAION_TYPE").Value;//读写方式
                                        string DATA_STANDARD = element.Element("PARAM_STANDARD_DATA").Value;//标准值
                                        isExist = element.Element("PARAM_ORDER") != null;//
                                        string PARAM_ORDER = "1";//
                                        if (isExist && !string.IsNullOrEmpty(element.Element("PARAM_ORDER").Value))
                                        {
                                            PARAM_ORDER = element.Element("PARAM_ORDER").Value;
                                        }
                                        string ITEM_ID = Itemid;

                                        MeterProtocalItem meterInfo = MeterProtocol.MeterProtocol.Select(DATA_TAG);

                                        Code698 = meterInfo.DataFlag698;//698的数据标识
                                        Name = meterInfo.Name;//数据项名称
                                        Length = meterInfo.Length645;//长度
                                        DecimalPlace = meterInfo.Dot698;//小数位
                                        Format = meterInfo.Format645;//数据格式
                                        if (DATA_ISREAD.Equals("0"))
                                        {
                                            Func = "读";
                                        }
                                        else
                                        {
                                            Func = "写";
                                        }


                                        values = JoinValue(DATA_NAME, Code698,
                                            Length.ToString(), DecimalPlace.ToString(), Format, Func, DATA_STANDARD, Itemid,
                                            DATA_TYPE, DATA_UNIT, DATA_INTERCEPT, DATA_ID);

                                        Schema[ID].SchemaNodeValue.Add(values);
                                    }

                                }

                                Schema[ID].SorData = SorData;
                                break;
                            case "远程密钥更新":
                                ID = ProjectID.密钥更新_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());

                                Schema[ID].SchemaNodeValue.Add(Itemid);
                                Schema[ID].SorData = SorData;
                                break;
                            //case "密钥恢复":
                            //    ID = ProjectID.密钥恢复;
                            //    if (!Schema.ContainsKey(ID))
                            //        Schema.Add(ID, new SchemaNode());

                            //    Schema[ID].SchemaNodeValue.Add(Itemid);
                            //    Schema[ID].SorData = SorData;
                            //    break;
                            case "规约一致性":   //
                                ID = ProjectID.通讯协议检查试验_黑龙江;
                                if (!Schema.ContainsKey(ID))
                                    Schema.Add(ID, new SchemaNode());

                                code = TestData[3].Split('/')[0];//645的数据标识（标识编码）
                                meter = MeterProtocol.MeterProtocol.Select(code);

                                string SetValue = "05";// TestData[3].Split('/')[1];//设定值（写入内容）

                                Code698 = meter.DataFlag698;//698的数据标识
                                Name = meter.Name;//数据项名称
                                Length = meter.Length645;//长度
                                DecimalPlace = meter.Dot698;//小数位
                                Format = meter.Format645;//数据格式
                                Func = "读";

                                values = JoinValue(Name, Code698, Length.ToString(), DecimalPlace.ToString(), Format, Func, SetValue, Itemid);
                                Schema[ID].SchemaNodeValue.Add(values);
                                Schema[ID].SorData = SorData;
                                break;
                            default:
                                break;
                        }
                    }

                    //排序方案
                    Schema = SortScheme(Schema);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.ToString(), EnumLogSource.检定业务日志, EnumLevel.Warning);
                Schema.Add("", null);
                return false;

            }
        }

        /// <summary>
        /// 转换电流倍数
        /// </summary>
        /// <param name="CurrentMultiple"></param>
        /// <returns></returns>
        private string ConvertCurrent(string CurrentMultiple)
        {
            string result = string.Empty;
            switch (CurrentMultiple)
            {
                case "":
                    result = "0.5Imax";
                    break;
                case "1.0Imax":
                    result = "Imax";
                    break;
                case "1Ib":
                    result = "Ib";
                    break;

            }

            return result;
        }

        private string GetRatePeriod(string RateTime, string RateType)
        {
            string hour = RateTime.Substring(0, 2);
            string minut = RateTime.Substring(2, 2);
            return $"{hour}:{minut}({RateType})";



        }

        private string GetConstantName(string type)
        {
            // 1-计数器脉冲法，2- 标准表法
            string Name = "";
            switch (type)
            {
                case "1":
                    Name = "计读脉冲法";
                    break;
                case "2":
                    Name = "标准表法";
                    break;
                default:
                    break;
            }
            return Name;


        }

        private string GetRateName(string type)
        {
            // 0-总 1-尖 2-峰  3-平 4-谷
            string Name = "";
            switch (type)
            {
                case "0":
                    Name = "总";
                    break;
                case "1":
                    Name = "尖";
                    break;
                case "2":
                    Name = "峰";
                    break;
                case "3":
                    Name = "平";
                    break;
                case "4":
                    Name = "谷";
                    break;
                default:
                    break;
            }
            return Name;


        }




        /// <summary>
        /// 获取项目名称
        /// </summary>
        /// <returns></returns>
        private string GetVierifyName(string Numebr)
        {
            string name = "";

            switch (Numebr)
            {
                //case "580":
                //    name = "密钥恢复";
                //    break;
                case "3":
                    name = "接线检查";   //RS485
                    break;
                case "54":
                    name = "起动试验";
                    break;
                case "55":
                    name = "潜动试验";
                    break;
                case "51":
                    name = "误差";
                    break;
                case "53":
                    name = "常数实验";
                    break;
                case "58":
                    name = "日计时";
                    break;
                case "56":
                    name = "电能示值组合误差";
                    break;
                case "1013":
                    name = "芯片ID认证";
                    break;
                case "1011":
                    name = "通讯性能检测(HPLC)";
                    break;
                case "66":
                    name = "时钟示值误差";
                    break;
                case "135":
                    name = "广播校时";
                    break;
                case "421":
                    name = "规约一致性";
                    break;
                case "130":
                    name = "远程清零";
                    break;
                case "852":
                    name = "参数验证";
                    break;
                case "556":
                    name = "远程密钥更新";
                    break;
                default:
                    break;
            }
            return name;
        }

        /// <summary>
        /// 获取电流倍数或者电流
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private string GetCurrentMultiple(string str1, string str2)
        {
            if (str1 != "*")  //第一个是型号就用第二个
            {
                return str1;
            }
            return str2;
        }
        /// <summary>
        /// 获取电压倍数或者实际电压
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private string GetVoltageMultiple(string str1, string str2)
        {
            if (str1 != "*")  //第一个是型号就用第二个
            {
                return str1;
            }
            return str2;
        }


        /// <summary>
        /// 获取功率方向
        /// </summary>
        /// <returns></returns>
        private string GetPowerDeiection(string str)
        {
            //-0正向有功--1反向有功
            string d = "正向有功";
            switch (str)
            {
                case "0":
                    d = "正向无功";
                    break;
                case "1":
                    d = "正向有功";
                    break;
                case "2":
                    d = "反向无功";
                    break;
                case "3":
                    d = "反向有功";
                    break;
                default:
                    break;
            }
            return d;

        }

        private string JoinValue(params string[] values)
        {
            return string.Join("|", values);
        }

        #region 上传数据

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        public bool Update(TestMeterInfo meter)
        {
            //SendPreWiringTest(meter);//上传接线检查
            //SendError(meter); //上传基本误差

            //上传起动

            return true;
        }



        public bool UpdateALL(TestMeterInfo[] meters, ref float FailureResultRatio)
        {
            LogManager.AddMessage("正在上传数据至MDS....，请勿操作！", EnumLogSource.服务器日志, EnumLevel.TipsTip);
            XmlData.Clear();

            #region 合格率不达标就上传失败

            int FailureNum = 0;//不合格表数量
            //bool Flag = false;
            int TotalCheckNum = 0;//要检表总数量
            foreach (var meter in meters)
            {
                if (!meter.YaoJianYn && string.IsNullOrEmpty(meter.MD_BarCode))
                    continue;

                if (meter.Result.Equals("不合格") || string.IsNullOrEmpty(meter.Result) || meter.Result.Length <= 0)
                {
                    FailureNum++;
                }
                TotalCheckNum++;

            }
            FailureResultRatio = (FailureNum / TotalCheckNum) * 100;//不合格率




            #endregion


            #region 组装数据

            SendErrorALL(meters);    //基本误差
            SendQiD(meters);         //启动
            SendQD(meters);         //潜动
            SendRJS(meters);         //日计时
            SendGPSTime(meters);     //GPS对时
            SendPreWiring(meters);   //接线检查
            SendConst(meters);       //常数试验
            SendEnergyError(meters);    //电能示值组合误差
            SendChipAuthentication(meters);   //芯片ID认证
            SendCommunicationHPLC(meters);    //通信性能检测(HPLC)
            SendClockError(meters);     //时钟示值误差
            SendProtocolConsistency(meters);//规约一致性
            SendClearEnerfy(meters);//电量清零
            SendParamConfirm(meters);         //参数验证
            SendRemotingKeyUpdate(meters);         //远程密钥更新

            #endregion


            #region 发送数据
            try
            {
                foreach (var item in XmlData)
                {
                    if (!ParseXmlData(item.Value))
                    {
                        continue;
                    }


                    //上传检定结果数据信息
                    //1-上传成功。0-上传失败
                    string result = Client.UploadCheckDataInfo(this.StationId, item.Value, true);
                    bool IsUpdate = ParseXMLUplodaResult(result);


                    if (!IsUpdate)
                    {
                        LogManager.AddMessage($"上传检定结果信息失败:{item.Key}，请重新上传！");
                        return false;
                    }

                }

                return true;


            }
            catch (Exception ex)
            {
                LogManager.AddMessage($@"发生未知错误:{ex.Message}");
                return false;
            }

            #endregion

        }
        public bool ParseXmlData(string ResultXml)
        {
            XDocument doc = XDocument.Parse(ResultXml);
            string IsHaveContent = doc.Element("PARA").Value;
            if (IsHaveContent.Length == 0 || IsHaveContent == null)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 载波通信测试
        /// </summary>
        /// <param name="meters"></param>
        private void SendCommunicationHPLC(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterCommunicationHPLC.Keys.Where(x => x.Contains(ProjectID.载波通信测试)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterCommunicationHPLC meterCommunicationHPLC = meter.MeterCommunicationHPLC[IsKeyReuslt[0]];
                if (meterCommunicationHPLC != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>1011</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterCommunicationHPLC.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string Electricity = (string.IsNullOrEmpty(meterCommunicationHPLC.Electricity) || meterCommunicationHPLC.Electricity.Length <= 0) ? "999" : meterCommunicationHPLC.Electricity;

                    //芯片ID
                    xml += $"<EXTENT_DATA>{meterCommunicationHPLC.Electricity}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterCommunicationHPLC.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterCommunicationHPLC.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }

            }


            xml += $"</PARA>";

            XmlData.Add("载波通信测试", xml);
        }


        /// <summary>
        /// 芯片ID认证
        /// </summary>
        /// <param name="meters"></param>
        private void SendChipAuthentication(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterHPLCIDAuthen.Keys.Where(x => x.Contains(ProjectID.载波芯片ID测试)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterHPLCIDAuthen meterHPLCIDAuthen = meter.MeterHPLCIDAuthen[IsKeyReuslt[0]];
                if (meterHPLCIDAuthen != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>1013</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterHPLCIDAuthen.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   



                    //芯片ID
                    xml += $"<EXTENT_DATA>{meterHPLCIDAuthen.ChipID}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterHPLCIDAuthen.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterHPLCIDAuthen.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }

            }


            xml += $"</PARA>";

            XmlData.Add("载波芯片ID测试", xml);
        }

        /// <summary>
        /// 远程密钥更新
        /// </summary>
        /// <param name="meters"></param>
        private void SendRemotingKeyUpdate(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterCostControls.Keys.Where(x => x.Contains(ProjectID.密钥更新_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterFK meterFK = meter.MeterCostControls[IsKeyReuslt[0]];
                if (meterFK != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>556</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterFK.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string MeterAddress = string.IsNullOrEmpty(meterFK.MeterAddress) ? "000000000000" : meterFK.MeterAddress;
                    string Electricity = string.IsNullOrEmpty(meterFK.Electricity) ? "999" : meterFK.Electricity;
                    string PrivateKey = string.IsNullOrEmpty(meterFK.PrivateKey) ? "0000000000000000" : meterFK.PrivateKey;
                    string PasswordState = string.IsNullOrEmpty(meterFK.PasswordState) ? "失败" : meterFK.PasswordState;
                    string PrivateKeyState = string.IsNullOrEmpty(meterFK.PrivateKeyState) ? "私钥认证失败" : meterFK.PrivateKeyState;

                    //表地址|电量|私钥密文|密钥下装状态|私钥认证状态
                    xml += $"<EXTENT_DATA>{MeterAddress}|" +
                        $"{Electricity}|{PrivateKey}|" +
                        $"{PasswordState}|" +
                        $"{PrivateKeyState}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterFK.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterFK.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }




            }


            xml += $"</PARA>";

            XmlData.Add("远程密钥更新", xml);
        }


        /// <summary>
        /// 参数验证
        /// </summary>
        /// <param name="meters"></param>
        private void SendParamConfirm(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterDLTDatas.Keys.Where(x => x.Contains(ProjectID.参数验证)).ToList();

                //if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0)
                //{
                //    continue;
                //}
                foreach (var item in IsKeyReuslt)
                {
                    MeterDLTData meterDLTData = meter.MeterDLTDatas[item];


                    if (meterDLTData != null)
                    {

                        xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                        xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                        xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                        xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                        xml += $"<TRIAL_TYPE>852</TRIAL_TYPE>\r\n";
                        xml += $"<ITEM_ID>{meterDLTData.ITEM_ID}</ITEM_ID>\r\n";//子项目ID
                        xml += $"<PARAM_ID>{meterDLTData.ChildredItemID}</PARAM_ID>\r\n";
                        xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                        xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                        xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                        string SetValue = string.IsNullOrEmpty(meterDLTData.SetValue) ? "999" : meterDLTData.SetValue;
                        string GetVavlue = string.IsNullOrEmpty(meterDLTData.GetVavlue) ? "999" : meterDLTData.GetVavlue;


                        //参数名称|读取值|标准值
                        xml += $"<EXTENT_DATA>{meterDLTData.Name}|" +
                            $"{GetVavlue}|{SetValue}</EXTENT_DATA>\r\n";
                        xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                        xml += $"<SCHEME_ID>{meterDLTData.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                        xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                        xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                        xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                        string TrialResult = meterDLTData.Result == ConstHelper.合格 ? "1" : "0";
                        xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                        xml += "</METER>\r\n";




                    }

                }


            }


            xml += $"</PARA>";
            XmlData.Add("参数验证", xml);
        }



        /// <summary>
        /// 电量清零
        /// </summary>
        /// <param name="meters"></param>
        private void SendClearEnerfy(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterClearEnerfy.Keys.Where(x => x.Contains(ProjectID.电量清零_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }
                MeterClearEnerfy meterClearEnerfy = meter.MeterClearEnerfy[IsKeyReuslt[0]];
                if (meterClearEnerfy != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>130</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterClearEnerfy.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE>{meterClearEnerfy.AvgValue}</ROUND_VALUE>\r\n";//平均值   

                    string ClearEnerfyBefore = string.IsNullOrEmpty(meterClearEnerfy.ClearEnerfyBefore) ? "999" : meterClearEnerfy.ClearEnerfyBefore;
                    string ClearEnerfyAfter = string.IsNullOrEmpty(meterClearEnerfy.ClearEnerfyAfter) ? "999" : meterClearEnerfy.ClearEnerfyAfter;

                    //清零前电量|清零后电量
                    xml += $"<EXTENT_DATA>{ClearEnerfyBefore}|" +
                        $"{ClearEnerfyAfter}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterClearEnerfy.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterClearEnerfy.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }


            }


            xml += $"</PARA>";
            XmlData.Add("电量清零", xml);
        }



        /// <summary>
        /// 规约一致性
        /// </summary>
        /// <param name="meters"></param>
        private void SendProtocolConsistency(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterDLTDatas.Keys.Where(x => x.Contains(ProjectID.通讯协议检查试验_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }
                MeterDLTData meterDLTData = meter.MeterDLTDatas[IsKeyReuslt[0]];
                if (meterDLTData != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>421</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterDLTData.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string SetValue = string.IsNullOrEmpty(meterDLTData.SetValue) ? "999" : meterDLTData.SetValue;
                    string GetVavlue = string.IsNullOrEmpty(meterDLTData.GetVavlue) ? "999" : meterDLTData.GetVavlue;

                    //数据标识|设定值|读取值|判定依据
                    xml += $"<EXTENT_DATA>{meterDLTData.DataFlag}|{SetValue}|" +
                        $"{meterDLTData.GetVavlue}|{meterDLTData.Result}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterDLTData.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterDLTData.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }
            }


            xml += $"</PARA>";
            XmlData.Add("规约一致性", xml);
        }


        /// <summary>
        /// 时钟示值误差
        /// </summary>
        /// <param name="meters"></param>
        private void SendClockError(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterClockError.Keys.Where(x => x.Contains(ProjectID.时钟示值误差_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterClockError meterClockError = meter.MeterClockError[IsKeyReuslt[0]];

                if (meterClockError != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>66</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterClockError.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string MeterCurrentReadDate = string.IsNullOrEmpty(meterClockError.MeterCurrentReadDate) ? DateTime.Now.ToString() : meterClockError.MeterCurrentReadDate;
                    string MeterCurrentReadTime = string.IsNullOrEmpty(meterClockError.MeterCurrentReadTime) ? DateTime.Now.ToString() : meterClockError.MeterCurrentReadTime;

                    //表当前日期|系统当前日期|日期差|表当前时间|系统当前时间|时间差|校时后电表时间|校时后系统当前时间|校时后时间差

                    xml += $"<EXTENT_DATA>{MeterCurrentReadDate}|{meterClockError.SystemCurrentDate}|" +
                        $"{meterClockError.Date}|{MeterCurrentReadTime}|{meterClockError.SystemCurrentTime}|" +
                        $"{meterClockError.TimeDifference}|{meterClockError.CheckedMeterTime}|{meterClockError.CheckedSystemCurrentTime}|" +
                        $"{meterClockError.CheckedTimeDifference}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterClockError.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterClockError.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }


            }
            xml += $"</PARA>";
            XmlData.Add("时钟示值误差", xml);



        }


        /// <summary>
        /// 通信性能检测(HPLC)
        /// </summary>
        /// <param name="meters"></param>
        //private void SendCommunicationHPLC(TestMeterInfo[] meters)
        //{
        //    xml = $"<PARA>";

        //    foreach (var meter in meters)
        //    {

        //        string[] keys = new string[meter.MeterCommunicationHPLC.Keys.Count];
        //        meter.MeterCommunicationHPLC.Keys.CopyTo(keys, 0);

        //        for (int i = 0; i < keys.Length; i++)
        //        {

        //            string key = keys[i];

        //            MeterCommunicationHPLC meterCommunicationHPLC = meter.MeterCommunicationHPLC[key];

        //            xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
        //            xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
        //            xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
        //            xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
        //            xml += $"<TRIAL_TYPE>1011</TRIAL_TYPE>\r\n";
        //            xml += $"<ITEM_ID>{meterCommunicationHPLC.ITEM_ID}</ITEM_ID>\r\n";//子项目ID
        //            xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
        //            xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
        //            xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
        //            xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

        //            //电量

        //            xml += $"<EXTENT_DATA>{meterCommunicationHPLC.Electricity}</EXTENT_DATA>\r\n"; //TODO
        //            xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
        //            xml += $"<SCHEME_ID>{meterCommunicationHPLC.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
        //            xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
        //            xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
        //            xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
        //            xml += "<TRIAL_RESULT>1</TRIAL_RESULT>\r\n";
        //            xml += "</METER>\r\n";
        //        }
        //    }


        //    xml += $"</PARA>";
        //    XmlData.Add("通信性能检测(HPLC)", xml);
        //}


        /// <summary>
        /// 芯片ID认证
        /// </summary>
        /// <param name="meters"></param>
        //private void SendChipAuthentication(TestMeterInfo[] meters)
        //{
        //    xml = $"<PARA>";

        //    foreach (var meter in meters)
        //    {
        //        meter.MeterEnergyError.ContainsKey(ProjectID.)
        //        string[] keys = new string[meter.MeterEnergyError.Keys.Count];
        //        meter.MeterEnergyError.Keys.CopyTo(keys, 0);

        //        for (int i = 0; i < keys.Length; i++)
        //        {

        //            string key = keys[i];

        //            MeterEnergyError meterEnergyError = meter.MeterEnergyError[key];

        //            xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
        //            xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
        //            xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
        //            xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
        //            xml += $"<TRIAL_TYPE>1013</TRIAL_TYPE>\r\n";
        //            xml += $"<ITEM_ID>{meterEnergyError.ITEM_ID}</ITEM_ID>\r\n";//子项目ID
        //            xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
        //            xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
        //            xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
        //            xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

        //            //走电前总|尖|峰|平|谷|走电后总|尖|峰|平|谷|组合误差|误差限|费率数|试验时间|电能增量|合格
        //            string[] ArrayPowerOutBefore = meterEnergyError.PowerOutBefore.Split('|');
        //            string[] ArrayPowerOutAfter = meterEnergyError.PowerOutAfter.Split('|');

        //            xml += $"<EXTENT_DATA>{ArrayPowerOutBefore[0]}|{ArrayPowerOutBefore[1]}|{ArrayPowerOutBefore[2]}|{ArrayPowerOutBefore[3]}|{ArrayPowerOutBefore[4]}| {ArrayPowerOutAfter[0]}|{ArrayPowerOutAfter[1]}|" +
        //                $"{ArrayPowerOutAfter[2]}|{ArrayPowerOutAfter[3]}|{ArrayPowerOutAfter[4]}|" +
        //                $"{meterEnergyError.CombinationError}|{meterEnergyError.WCRate}|{meterEnergyError.Rate}|" +
        //                $"{meterEnergyError.TrialTime}|{meterEnergyError.EnergyIncrement}|{meterEnergyError.Result}</EXTENT_DATA>\r\n"; //TODO
        //            xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
        //            xml += $"<SCHEME_ID>{meterEnergyError.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
        //            xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
        //            xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
        //            xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
        //            xml += "<TRIAL_RESULT>1</TRIAL_RESULT>\r\n";
        //            xml += "</METER>\r\n";
        //        }
        //    }


        //    xml += $"</PARA>";
        //    XmlData.Add("芯片ID认证", xml);
        //}

        /// <summary>
        /// 电能示值组合误差
        /// </summary>
        /// <param name="meters"></param>
        private void SendEnergyError(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterEnergyError.Keys.Where(x => x.Contains(ProjectID.电能示值组合误差_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }
                MeterEnergyError meterEnergyError = meter.MeterEnergyError[IsKeyReuslt[0]];
                if (meterEnergyError != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>56</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterEnergyError.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值   
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值  
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    #region
                    string[] ArrayPowerOutBefore = meterEnergyError.PowerOutBefore.Split(',');
                    string[] ArrayPowerOutAfter = meterEnergyError.PowerAfterTotal.Split(',');

                    string PowerBeforeTotal = string.IsNullOrEmpty(meterEnergyError.PowerBeforeTotal) ? "999" : meterEnergyError.PowerBeforeTotal;
                    string PowerBeforeJ = string.IsNullOrEmpty(ArrayPowerOutBefore[0]) ? "999" : ArrayPowerOutBefore[0];
                    string PowerBeforeF = string.IsNullOrEmpty(ArrayPowerOutBefore[1]) ? "999" : ArrayPowerOutBefore[1];
                    string PowerBeforeP = string.IsNullOrEmpty(ArrayPowerOutBefore[2]) ? "999" : ArrayPowerOutBefore[2];
                    string PowerBeforeG = string.IsNullOrEmpty(ArrayPowerOutBefore[3]) ? "999" : ArrayPowerOutBefore[3];

                    string PowerOutAfterTotal = string.IsNullOrEmpty(meterEnergyError.PowerOutAfter) ? "999" : meterEnergyError.PowerOutAfter;
                    string PowerAfterJ = string.IsNullOrEmpty(ArrayPowerOutAfter[0]) ? "999" : ArrayPowerOutAfter[0];
                    string PowerAfterF = string.IsNullOrEmpty(ArrayPowerOutAfter[1]) ? "999" : ArrayPowerOutAfter[1];
                    string PowerAfterP = string.IsNullOrEmpty(ArrayPowerOutAfter[2]) ? "999" : ArrayPowerOutAfter[2];
                    string PowerAfterG = string.IsNullOrEmpty(ArrayPowerOutAfter[3]) ? "999" : ArrayPowerOutAfter[3];

                    string CombinationError = string.IsNullOrEmpty(meterEnergyError.CombinationError) ? "999" : meterEnergyError.CombinationError;
                    string WCRate = string.IsNullOrEmpty(meterEnergyError.WCRate) ? "999" : meterEnergyError.WCRate;
                    string Rate = string.IsNullOrEmpty(meterEnergyError.Rate) ? "999" : meterEnergyError.Rate;
                    string TrialTime = string.IsNullOrEmpty(meterEnergyError.TrialTime) ? DateTime.Now.ToString() : meterEnergyError.TrialTime;
                    string EnergyIncrement = string.IsNullOrEmpty(meterEnergyError.EnergyIncrement) ? "999" : meterEnergyError.EnergyIncrement;
                    #endregion


                    //走电前总|尖|峰|平|谷|走电后总|尖|峰|平|谷|组合误差|误差限|费率数|试验时间|电能增量
                    xml += $"<EXTENT_DATA>{meterEnergyError.PowerBeforeTotal}|{PowerBeforeJ}|{PowerBeforeF}|{PowerBeforeP}|{PowerBeforeG}|" +
                        $"{PowerOutAfterTotal}|{PowerAfterJ}|{PowerAfterF}|{PowerAfterP}|{PowerAfterG}|{CombinationError}|{WCRate}|{Rate}|{TrialTime}|{EnergyIncrement}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterEnergyError.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterEnergyError.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }

            }


            xml += $"</PARA>";
            XmlData.Add("电能示值组合误差", xml);
        }


        /// <summary>
        /// 常数试验
        /// </summary>
        /// <param name="meters"></param>
        private void SendConst(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterZZErrors.Keys.Where(x => x.Contains(ProjectID.电能表常数试验_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }
                MeterZZError meterZZErrors = meter.MeterZZErrors[IsKeyReuslt[0]];
                if (meterZZErrors != null)
                {

                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>53</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterZZErrors.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值    
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值 
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string ZzCurrent = string.IsNullOrEmpty(meterZZErrors.ZzCurrent) ? "0" : meterZZErrors.ZzCurrent;
                    string Pules = string.IsNullOrEmpty(meterZZErrors.Pules) ? "0" : meterZZErrors.Pules;
                    string PowerStart = string.IsNullOrEmpty(meterZZErrors.PowerStart.ToString()) ? "0" : meterZZErrors.PowerStart.ToString();
                    string PowerEnd = string.IsNullOrEmpty(meterZZErrors.PowerEnd.ToString()) ? "0" : meterZZErrors.PowerEnd.ToString();
                    string MeterEnergy = string.IsNullOrEmpty(meterZZErrors.MeterEnergy.ToString()) ? "0" : meterZZErrors.MeterEnergy.ToString();
                    string ErrorValue = string.IsNullOrEmpty(meterZZErrors.ErrorValue) ? "999" : meterZZErrors.ErrorValue;
                    string ErrorRate = string.IsNullOrEmpty(meterZZErrors.ErrorRate) ? "0" : meterZZErrors.ErrorRate;
                    string STMEnergy = string.IsNullOrEmpty(meterZZErrors.STMEnergy) ? "0" : meterZZErrors.STMEnergy;

                    //试验电量(KW/h)|被检表脉冲个数|起始电量|终止电量|电能表累计电量|误差值|误差限|标准表电量
                    xml += $"<EXTENT_DATA>{ZzCurrent}|{Pules}|{PowerStart}|{PowerEnd}| {MeterEnergy}|{ErrorValue}|{ErrorRate}|{STMEnergy}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterZZErrors.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterZZErrors.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }


            }


            xml += $"</PARA>";
            XmlData.Add("常数试验", xml);
        }

        /// <summary>
        /// 上传接线检查
        /// </summary>
        /// <param name="meters"></param>
        private void SendPreWiring(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {
                var IsKeyReuslt = meter.MeterPreWiring.Keys.Where(x => x.Contains(ProjectID.接线检查_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterPreWiring meterPreWiring = meter.MeterPreWiring[IsKeyReuslt[0]];
                if (meterPreWiring != null)
                {

                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>3</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterPreWiring.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值 
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string BarCode = string.IsNullOrEmpty(meterPreWiring.BarCode) ? "000000000000" : meterPreWiring.BarCode;
                    string Address = string.IsNullOrEmpty(meterPreWiring.Address) ? "000000000000" : meterPreWiring.Address;
                    string ClockPulse = string.IsNullOrEmpty(meterPreWiring.ClockPulse) ? "999" : meterPreWiring.ClockPulse;
                    string EnergyPulses = string.IsNullOrEmpty(meterPreWiring.EnergyPulses) ? "0" : meterPreWiring.EnergyPulses;
                    string AVoltage = string.IsNullOrEmpty(meterPreWiring.AVoltage) ? "0" : meterPreWiring.AVoltage;
                    string ACurrent = string.IsNullOrEmpty(meterPreWiring.ACurrent) ? "0" : meterPreWiring.ACurrent;
                    string BatteryVoltage = string.IsNullOrEmpty(meterPreWiring.BatteryVoltage) ? "0" : meterPreWiring.BatteryVoltage;
                    string RunStatusWord = string.IsNullOrEmpty(meterPreWiring.RunStatusWord) ? "0000" : meterPreWiring.RunStatusWord;


                    //表条号码|表地址|时钟脉冲|电能脉冲|A相电压|A相电流|电池电压|运行状态字
                    xml += $"<EXTENT_DATA>{BarCode}|{Address}|{ClockPulse}|{EnergyPulses}|{AVoltage}| {ACurrent}|{BatteryVoltage}|{RunStatusWord}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterPreWiring.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterPreWiring.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }

            }
            xml += $"</PARA>";
            XmlData.Add("接线检查", xml);
        }





        /// <summary>
        /// 上传GPS对时
        /// </summary>
        /// <param name="meters"></param>
        private void SendGPSTime(TestMeterInfo[] meters)
        {

            xml = $"<PARA>";

            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterDgns.Keys.Where(x => x.Contains(ProjectID.GPS对时_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }
                MeterDgn meterDgns = meter.MeterDgns[IsKeyReuslt[0]];

                if (meterDgns != null)
                {
                    if (string.IsNullOrEmpty(meterDgns.ITEM_ID))
                    {
                        continue;
                    }

                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>135</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterDgns.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";
                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值 
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string MeterCurrentReadDate = string.IsNullOrEmpty(meterDgns.MeterCurrentReadDate) ? DateTime.Now.ToString() : meterDgns.MeterCurrentReadDate;
                    string MeterCurrentReadTime = string.IsNullOrEmpty(meterDgns.MeterCurrentReadTime) ? DateTime.Now.ToString() : meterDgns.MeterCurrentReadTime;


                    //表当前日期|系统当前日期|日期差|表当前时间|系统当前时间|校时前时间差|校时后时间差|误差限
                    xml += $"<EXTENT_DATA>{MeterCurrentReadDate}|{meterDgns.SystemCurrentDate}|{meterDgns.Date}|{MeterCurrentReadTime}|{meterDgns.SystemCurrentTime}| {meterDgns.BeforeDateDifference}|{meterDgns.AfterDateDifference}|{meterDgns.ErrorRate}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterDgns.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterDgns.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }


            }


            xml += $"</PARA>";
            XmlData.Add("广播校时", xml);
        }

        /// <summary>
        /// 上传日计时
        /// </summary>
        /// <param name="meters"></param>
        private void SendRJS(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";

            foreach (var meter in meters)
            {



                var IsKeyReuslt = meter.MeterDgns.Keys.Where(x => x.Contains(ProjectID.日计时误差_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterDgn meterDgns = meter.MeterDgns[IsKeyReuslt[0]];
                if (meterDgns != null)
                {


                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>58</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterDgns.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";


                    if (string.IsNullOrEmpty(meterDgns.WC1) || meterDgns.WC1 == null)
                    {
                        meterDgns.WC1 = "999";
                    }
                    //else if(float.Parse(meterDgns.WC1) > 0)
                    //{
                    //    meterDgns.WC1 = "+" + meterDgns.WC1;
                    //}

                    if (string.IsNullOrEmpty(meterDgns.WC2) || meterDgns.WC2 == null)
                    {
                        meterDgns.WC2 = "999";
                    }
                    //else if (float.Parse(meterDgns.WC2) > 0)
                    //{
                    //    meterDgns.WC2 = "+" + meterDgns.WC2;
                    //}

                    if (string.IsNullOrEmpty(meterDgns.WC3) || meterDgns.WC3 == null)
                    {
                        meterDgns.WC3 = "999";
                    }
                    //else if (float.Parse(meterDgns.WC3) > 0)
                    //{
                    //    meterDgns.WC3 = "+" + meterDgns.WC3;
                    //}

                    if (string.IsNullOrEmpty(meterDgns.WC4) || meterDgns.WC4 == null)
                    {
                        meterDgns.WC4 = "999";
                    }
                    //else if (float.Parse(meterDgns.WC4) > 0)
                    //{
                    //    meterDgns.WC4 = "+" + meterDgns.WC4;
                    //}

                    if (string.IsNullOrEmpty(meterDgns.WC5) || meterDgns.WC5 == null)
                    {
                        meterDgns.WC5 = "999";
                    }
                    //else if (float.Parse(meterDgns.WC5) > 0)
                    //{
                    //    meterDgns.WC5 = "+" + meterDgns.WC5;
                    //}

                    if (string.IsNullOrEmpty(meterDgns.AvgValue) || meterDgns.AvgValue == null)
                    {
                        meterDgns.AvgValue = "999";
                    }
                    //else if (float.Parse(meterDgns.AvgValue) > 0)
                    //{
                    //    meterDgns.AvgValue = "+" + meterDgns.AvgValue;
                    //}

                    if (string.IsNullOrEmpty(meterDgns.HzValue) || meterDgns.HzValue == null)
                    {
                        meterDgns.HzValue = "999";
                    }
                    //else if (float.Parse(meterDgns.HzValue) > 0)
                    //{
                    //    meterDgns.HzValue = "+" + meterDgns.HzValue;
                    //}





                    xml += $"<PRISTINE_VALUES>{meterDgns.WC1},{meterDgns.WC2},{meterDgns.WC3},{meterDgns.WC4},{meterDgns.WC5},</PRISTINE_VALUES>\r\n";//原始值 
                    xml += $"<TRIAL_VALUE>{meterDgns.HzValue}</TRIAL_VALUE>\r\n"; //化整值
                    xml += $"<ROUND_VALUE>{meterDgns.AvgValue}</ROUND_VALUE>\r\n";//平均值   

                    //误差原始数据|平均值|化整值|误差限
                    xml += $"<EXTENT_DATA>{meterDgns.WC1},{meterDgns.WC2},{meterDgns.WC3},{meterDgns.WC4},{meterDgns.WC5},|{meterDgns.AvgValue}|{meterDgns.HzValue}|{meterDgns.WCRate}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterDgns.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterDgns.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";
                }
            }



            xml += $"</PARA>";
            XmlData.Add("日计时试验", xml);
        }

        /// <summary>
        /// 上传潜动
        /// </summary>
        /// <param name="meters"></param>
        private void SendQD(TestMeterInfo[] meters)
        {
            xml = $"<PARA>";
            foreach (var meter in meters)
            {


                var IsKeyReuslt = meter.MeterQdQids.Keys.Where(x => x.Contains(ProjectID.潜动试验_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }
                MeterQdQid meterQd = meter.MeterQdQids[IsKeyReuslt[0]];
                if (meterQd != null)
                {

                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>55</TRIAL_TYPE>\r\n";
                    xml += $"<ITEM_ID>{meterQd.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";

                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    string MCCount = meterQd.Result;

                    string Pulse = string.IsNullOrEmpty(meterQd.Pulse) ? "999" : meterQd.Pulse;

                    //潜动电压|脉冲数|实际潜动时间|潜动标准时间
                    xml += $"<EXTENT_DATA>{meterQd.Voltage}|{Pulse}|{meterQd.ActiveTime}|{meterQd.StandartTime}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterQd.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                    string TrialResult = meterQd.Result == ConstHelper.合格 ? "1" : "0";
                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";


                }

            }


            xml += $"</PARA>";
            XmlData.Add("潜动试验", xml);
        }




        /// <summary>
        /// 上传基本误差的结论
        /// </summary>
        /// <param name="meter"></param>
        private void SendErrorALL(TestMeterInfo[] meters)
        {


            xml = $"<PARA>";
            string wc1 = string.Empty;
            string wc2 = string.Empty;
            string wcAvg = string.Empty;
            string wcHz = string.Empty;
            string upLimit = string.Empty;
            string downLimit = string.Empty;
            string wcvalue = string.Empty;


            foreach (var meter in meters)
            {

                var IsKeyReuslt = meter.MeterErrors.Keys.Where(x => x.Contains(ProjectID.基本误差试验_黑龙江)).ToList();

                //if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0)
                //{
                //    continue;
                //}
                foreach (var item in IsKeyReuslt)
                {
                    MeterError meterError = meter.MeterErrors[item];
                    if (meterError != null)
                    {

                        xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                        xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                        xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                        xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                        xml += $"<TRIAL_TYPE>51</TRIAL_TYPE>\r\n";
                        xml += $"<ITEM_ID>{meterError.ITEM_ID}</ITEM_ID>\r\n";//子项目ID
                        xml += $"<PARAM_ID>0</PARAM_ID>\r\n";


                        //TODO:这里上传的时候需要判断
                        //判断基本误差那些参数是否为空，空的化上传999
                        // meterError.WCData = ResultValue["误差1"]+ "|" +ResultValue["误差2"]+"|" + ResultValue["平均值"]+"|" + ResultValue["化整值"];

                        if (meterError.WCData.Length > 0 || meterError.WCData != "")
                        {
                            string[] wc = meterError.WCData.Split('|');


                            wc1 = wc[0] == "" ? "999" : wc[0];
                            wc2 = wc[1] == "" ? "999" : wc[1];
                            wcAvg = wc[2] == "" ? "999" : wc[2];
                            wcHz = wc[3] == "" ? "999" : wc[3];

                        }


                        xml += $"<PRISTINE_VALUES>{wc1},{wc2},</PRISTINE_VALUES>\r\n";//原始值
                        xml += $"<TRIAL_VALUE>{wcHz}</TRIAL_VALUE>\r\n"; //化整值
                        xml += $"<ROUND_VALUE>{wcAvg}</ROUND_VALUE>\r\n";//平均值


                        downLimit = meterError.BPHDownLimit; //误差下线
                        upLimit = meterError.BPHUpLimit;//误差上线



                        //误差原始数据|平均值|化整值|误差下限|误差上限
                        // xml += $"<EXTENT_DATA>{wc1},{wc2},|{wcAvg}|{wcHz}|{downLimit}|{upLimit}</EXTENT_DATA>"; //TODO
                        xml += $"<EXTENT_DATA>{wc1},{wc2},|{wcAvg}|{wcHz}|{downLimit}|{upLimit}</EXTENT_DATA>\r\n";
                        xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                        xml += $"<SCHEME_ID>{meterError.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                        xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                        xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                        xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";
                        string TrialResult = meterError.Result == ConstHelper.合格 ? "1" : "0";
                        xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                        xml += "</METER>\r\n";
                        //}

                    }

                }


            }
            xml += $"</PARA>";
            XmlData.Add("基本误差", xml);
        }

        /// <summary>
        /// 上传启动实验的结论
        /// </summary>
        /// <param name="meter"></param>
        private void SendQiD(TestMeterInfo[] meters)
        {


            xml = $"<PARA>";
            string wc1 = string.Empty;
            string wc2 = string.Empty;
            string wcAvg = string.Empty;
            string wcHz = string.Empty;
            string wcvalue = string.Empty;
            foreach (var meter in meters)
            {
                var IsKeyReuslt = meter.MeterQdQids.Keys.Where(x => x.Contains(ProjectID.起动试验_黑龙江)).ToList();

                if (IsKeyReuslt == null || IsKeyReuslt.Count <= 0 || string.IsNullOrEmpty(meter.MD_BarCode))
                {
                    continue;
                }

                MeterQdQid meterQd = meter.MeterQdQids[IsKeyReuslt[0]];
                if (meterQd != null)
                {
                    xml += "<METER><GUID_ID>000000000000</GUID_ID>\r\n";
                    xml += $"<METER_ID>{meter.MD_Epitope}</METER_ID>\r\n";
                    xml += $"<METER_NO>{meter.MD_BarCode}</METER_NO>\r\n";
                    xml += $"<STAION_ID>{meter.BenthNo}</STAION_ID>\r\n";
                    xml += $"<TRIAL_TYPE>54</TRIAL_TYPE>\r\n";

                    xml += $"<ITEM_ID>{meterQd.ITEM_ID}</ITEM_ID>\r\n";//子项目ID

                    xml += $"<PARAM_ID>0</PARAM_ID>\r\n";

                    xml += $"<PRISTINE_VALUES></PRISTINE_VALUES>\r\n";//原始值
                    xml += $"<TRIAL_VALUE></TRIAL_VALUE>\r\n"; //化整值
                    xml += $"<ROUND_VALUE></ROUND_VALUE>\r\n";//平均值   

                    //string MCCount = meterQd.Result;

                    string Pulse = string.IsNullOrEmpty(meterQd.Pulse) ? "0" : meterQd.Pulse;

                    //起动电流|脉冲数|实际启动时间|启动标准时间
                    xml += $"<EXTENT_DATA>{meterQd.Current}|{Pulse}|{meterQd.ActiveTime}|{meterQd.StandartTime}</EXTENT_DATA>\r\n";
                    xml += $"<RECHECK_INDEX>1</RECHECK_INDEX>\r\n";
                    xml += $"<SCHEME_ID>{meterQd.FK_LNG_SCHEME_ID}</SCHEME_ID>\r\n";
                    xml += $"<TASK_ID>{meter.MD_TaskNo}</TASK_ID>\r\n";
                    xml += $"<TRIAL_ENDDATE>{DateTime.Now}</TRIAL_ENDDATE>\r\n";
                    xml += $"<TRIAL_STARTDATE>{DateTime.Now}</TRIAL_STARTDATE>\r\n";

                    //if (string.IsNullOrEmpty(TrialResult))
                    //{

                    string TrialResult = meterQd.Result == ConstHelper.合格 ? "1" : "0";
                    //}
                    //else {
                    //    TrialResult = "0";
                    //}

                    xml += $"<TRIAL_RESULT>{TrialResult}</TRIAL_RESULT>\r\n";
                    xml += "</METER>\r\n";


                }


            }
            xml += $"</PARA>";
            XmlData.Add("起动试验", xml);
        }

        #endregion



        public void ShowPanel(Control panel)
        {

        }


        //上传完所有试验报文，才发送检定完成信息
        public bool UpdateCompleted()
        {
            try
            {
                string result = Client.UploadCheckFinish(this.StationId);
                if (!ParseXMLUplodaResult(result))
                {
                    LogManager.AddMessage($"上传检定完成信息失败，请重新上传！");
                    return false;
                }
                //返回信息
                return true;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage($@"发生未知错误:{ex.Message}");
            }
            return false;


        }

        public bool ParseXMLUplodaResult(string info)
        {
            try
            {
                //<DATA><RESULT_FLAG>1</RESULT_FLAG><ERROR_INFO></ERROR_INFO></DATA>
                //string DateTime = string.Empty;
                XDocument doc = XDocument.Parse(info);
                string resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
                if (resultFlag == "1")
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false; ;
        }

        public void UpdateInit()
        {

        }

        public Dictionary<string, SchemaNode> SortScheme(Dictionary<string, SchemaNode> Schema)
        {
            //var TemScheme = Schema.OrderBy(item => item.Value.SorData);
            //return TemScheme.ToDictionary(i => i.Key, i => i.Value);

            Dictionary<string, SchemaNode> TemScheme = new Dictionary<string, SchemaNode>();

            //if (Schema.ContainsKey(ProjectID.密钥恢复))
            //    TemScheme.Add(ProjectID.密钥恢复, Schema[ProjectID.密钥恢复]);
            if (Schema.ContainsKey(ProjectID.接线检查_黑龙江))
                TemScheme.Add(ProjectID.接线检查_黑龙江, Schema[ProjectID.接线检查_黑龙江]);
            if (Schema.ContainsKey(ProjectID.起动试验_黑龙江))
                TemScheme.Add(ProjectID.起动试验_黑龙江, Schema[ProjectID.起动试验_黑龙江]);
            if (Schema.ContainsKey(ProjectID.潜动试验_黑龙江))
                TemScheme.Add(ProjectID.潜动试验_黑龙江, Schema[ProjectID.潜动试验_黑龙江]);
            if (Schema.ContainsKey(ProjectID.基本误差试验_黑龙江))
                TemScheme.Add(ProjectID.基本误差试验_黑龙江, Schema[ProjectID.基本误差试验_黑龙江]);
            if (Schema.ContainsKey(ProjectID.电能表常数试验_黑龙江))
                TemScheme.Add(ProjectID.电能表常数试验_黑龙江, Schema[ProjectID.电能表常数试验_黑龙江]);
            if (Schema.ContainsKey(ProjectID.通讯协议检查试验_黑龙江))
                TemScheme.Add(ProjectID.通讯协议检查试验_黑龙江, Schema[ProjectID.通讯协议检查试验_黑龙江]);
            if (Schema.ContainsKey(ProjectID.参数验证))
                TemScheme.Add(ProjectID.参数验证, Schema[ProjectID.参数验证]);
            if (Schema.ContainsKey(ProjectID.载波通信测试))
                TemScheme.Add(ProjectID.载波通信测试, Schema[ProjectID.载波通信测试]);
            if (Schema.ContainsKey(ProjectID.载波芯片ID测试))
                TemScheme.Add(ProjectID.载波芯片ID测试, Schema[ProjectID.载波芯片ID测试]);
            if (Schema.ContainsKey(ProjectID.日计时误差_黑龙江))
                TemScheme.Add(ProjectID.日计时误差_黑龙江, Schema[ProjectID.日计时误差_黑龙江]);
            if (Schema.ContainsKey(ProjectID.GPS对时_黑龙江))
                TemScheme.Add(ProjectID.GPS对时_黑龙江, Schema[ProjectID.GPS对时_黑龙江]);
            if (Schema.ContainsKey(ProjectID.电能示值组合误差_黑龙江))
                TemScheme.Add(ProjectID.电能示值组合误差_黑龙江, Schema[ProjectID.电能示值组合误差_黑龙江]);

            if (Schema.ContainsKey(ProjectID.时钟示值误差_黑龙江))
                TemScheme.Add(ProjectID.时钟示值误差_黑龙江, Schema[ProjectID.时钟示值误差_黑龙江]);
            if (Schema.ContainsKey(ProjectID.电量清零_黑龙江))
                TemScheme.Add(ProjectID.电量清零_黑龙江, Schema[ProjectID.电量清零_黑龙江]);


            if (Schema.ContainsKey(ProjectID.密钥更新_黑龙江))
                TemScheme.Add(ProjectID.密钥更新_黑龙江, Schema[ProjectID.密钥更新_黑龙江]);



            return TemScheme;
        }

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }
    }
}
