using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.DAL.Config;
using LYTest.MeterProtocol.Enum;
using LYTest.Mis.Common;
using LYTest.Mis.MDS.Table;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Data;

namespace LYTest.Verify.CarrierTest
{
    class HPLC_ID_Authen : VerifyBase
    {
        string ItemId = string.Empty;

        public override void Verify()
        {
            try
            {
                base.Verify();

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    ResultDictionary["项目编号"][i] = ItemId;
                }

                RefUIData("项目编号");

                string NoticeNo = DateTime.Now.ToString("yyyyMMddHHmmss");
                string taskNo = "";
                string sys_no = "201";
                OracleHelper oraHelper = new OracleHelper
                {
                    Ip = VerifyConfig.Marketing_IP,
                    Port = int.Parse(VerifyConfig.Marketing_Prot),
                    UserId = VerifyConfig.Marketing_UserName,
                    Password = VerifyConfig.Marketing_UserPassWord,
                    DataSource = VerifyConfig.Marketing_DataSource,
                    WebServiceURL = VerifyConfig.Marketing_WebService
                };
                if (!PowerOn())
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    TryStopTest();
                    return;
                }
                WaitTime("正在升源", 5);
                LYTest.MeterProtocol.App.g_ChannelType = Cus_ChannelType.通讯载波;
                SwitchChannel(Cus_ChannelType.通讯载波);

                Dictionary<string, string> dicID = new Dictionary<string, string>();

                bool tc = false;
                int YaojianNum = GetYaoJianNum();

                if (Stop) return;

                MessageAdd("正在读取HPLC芯片ID...", EnumLogType.提示信息);

                if (ConfigHelper.Instance.AuthenHPLCID)
                {

                    //DateTime TmpTime1 = DateTime.Now;

                    //while (TimeSub(DateTime.Now, TmpTime1) < 240000) //读取超时了
                    //{
                    //    dicID = MeterProtocolAdapter.Instance.ReadHPLCID();
                    //    if (dicID.Count >= YaojianNum + 1) break;
                    //    System.Threading.Thread.Sleep(5000);
                    //}
                    dicID = ReadID();
                    //if (dicID == null) continue;

                    //MessageAdd(string.Format("共读取出{0}块表芯片ID...", dicID.Count), EnumLogType.提示信息);
                    if (Stop) return;
                    if (dicID.Count < YaojianNum + 1)
                    {
                        WaitTime("重试读取HPLC芯片ID第2次读取", 10);
                        Dictionary<string, string> dic = MeterProtocolAdapter.Instance.ReadHPLCID();

                        foreach (KeyValuePair<string, string> kvp in dic)
                        {
                            if (!dicID.ContainsKey(kvp.Key))
                                dicID.Add(kvp.Key, kvp.Value);
                        }
                        MessageAdd(string.Format("第2次共读取出{0}块表芯片ID...", dicID.Count - 1), EnumLogType.流程信息);
                    }
                    if (dicID.Count > 1)
                    {
                        tc = true;
                    }
                    if (dicID == null || dicID.Count < 1)//应该用提示，后期在说
                    {
                        Stop = true;
                        MessageAdd("未读取到芯片ID", EnumLogType.错误信息);
                        return;
                    }
                    List<string> sqlList = new List<string>();


                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (Stop) break;

                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;

                        sys_no = meter.Meter_SysNo;   //系统编号
                        taskNo = meter.MD_TaskNo;  //任务号

                        string moduleID = "";
                        string chipID = "";

                        if (dicID.ContainsKey(meter.MD_PostalAddress))
                        {
                            moduleID = meter.MD_PostalAddress;
                            chipID = dicID[meter.MD_PostalAddress];
                            ResultDictionary["表地址"][i] = moduleID;
                            ResultDictionary["芯片ID号"][i] = chipID;
                            ResultDictionary["结论"][i] = ConstHelper.合格;

                        }
                        else
                        {
                            ResultDictionary["表地址"][i] = moduleID;
                            ResultDictionary["芯片ID号"][i] = "000000000000000000000000000000000000000000000000";
                            ResultDictionary["结论"][i] = ConstHelper.不合格;
                        }
                        MessageAdd($"表地址[{moduleID}] Id：{chipID}", EnumLogType.提示信息);

                    }
                    SwitchChannel(Cus_ChannelType.通讯485);

                    RefUIData("表地址");
                    RefUIData("芯片ID号");

                }
                else//分支2-认证
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        //1. 读取HPLC芯片ID
                        if (Stop) return;

                        if (tc == true) break;

                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;

                        LYTest.MeterProtocol.App.Carrier_Cur_BwIndex = i;

                        MessageAdd("正在读取HPLC芯片ID...");

                        dicID = MeterProtocolAdapter.Instance.ReadHPLCID();

                        if (dicID == null) continue;

                        MessageAdd(string.Format("共读取出{0}块表芯片ID...", dicID.Count));
                        if (Stop) return;
                        if (dicID.Count < YaojianNum + 1)
                        {
                            WaitTime("重试读取HPLC芯片ID第2次读取", 10);
                            Dictionary<string, string> dic = MeterProtocolAdapter.Instance.ReadHPLCID();
                            if (dic == null) continue;

                            foreach (KeyValuePair<string, string> kvp in dic)
                            {
                                if (!dicID.ContainsKey(kvp.Key))
                                    dicID.Add(kvp.Key, kvp.Value);
                            }
                            MessageAdd(string.Format("第2次共读取出{0}块表芯片ID...", dicID.Count - 1));
                        }
                        if (dicID.Count > 1)
                        {
                            tc = true;
                        }
                    }
                    if (dicID == null || dicID.Count < 1)//应该用提示，后期在说
                    {
                        MessageAdd("未读取到芯片ID!.", EnumLogType.错误信息);
                        Stop = true;
                        return;
                    }
                    List<string> sqlList = new List<string>();

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (Stop) break;

                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;

                        if (string.IsNullOrEmpty(meter.MD_TaskNo))
                        {
                            MessageAdd("表号[" + meter.MD_BarCode + "]的任务单号为空，MDS要求任务单号不为空，检测将停止!.");
                            Stop = true;
                            return;
                        }


                        sys_no = meter.Meter_SysNo;   //系统编号
                        taskNo = meter.MD_TaskNo;  //任务号

                        string moduleID = "";
                        string chipID = "";
                        if (dicID.ContainsKey(meter.MD_PostalAddress))
                        {
                            moduleID = meter.MD_PostalAddress;
                            chipID = dicID[meter.MD_PostalAddress];
                            ResultDictionary["表地址"][i] = moduleID;
                            ResultDictionary["芯片ID号"][i] = moduleID;

                        }
                        MessageAdd($"表地址[{moduleID}] Id：{chipID}");

                        //MeterDgn data = meter.MeterDgns.GetValue(ItemKey);
                        //data.PrjID = ItemKey;           //项目ID
                        //data.Name = "HPLC芯片ID认证";   //项目名称
                        //data.Value = string.Format("{0}|{1}", moduleID, chipID);
                        //data.Result = "";               //项目结果


                        if (string.IsNullOrEmpty(chipID) || chipID == "000000000000000000000000000000000000000000000000") continue;

                        MT_HPLCID_CERT_INFO info = new MT_HPLCID_CERT_INFO()
                        {
                            DETECT_TASK_NO = meter.MD_TaskNo,
                            WEB_NOTICE_NO = NoticeNo,
                            SYS_NO = sys_no,
                            BAR_CODE = meter.MD_BarCode,
                            MODULE_TYPE_CODE = "03", //载波模块类别
                            MODULE_BAR_CODE = "",
                            HPLCID = chipID,
                            WRITE_DATE = DateTime.Now.ToString(),
                            HANDLE_FLAG = "0",
                            HANDLE_DATE = "",
                        };

                        sqlList.Add($"DELETE FROM MT_HPLCID_CERT_INFO WHERE BAR_CODE = '{meter.MD_BarCode}'");
                        sqlList.Add(info.ToInsertString());

                        SwitchChannel(Cus_ChannelType.通讯485);

                    }
                    RefUIData("表地址");
                    RefUIData("芯片ID号");

                    //2. 芯片信息写入数据库
                    if (Stop) return;
                    MessageAdd("正在数据库写入芯片信息");
                    oraHelper.Execute(sqlList);

                    //3. 调用WebServer
                    if (Stop) return;
                    MessageAdd("正在调用WebServer");
                    string url = VerifyConfig.Marketing_WebService;
                    string xml = "<PARA>";
                    xml += "<SYS_NO>" + sys_no + "</SYS_NO>";
                    xml += "<DETECT_TASK_NO>" + taskNo + "</DETECT_TASK_NO>";
                    xml += "<WEB_NOTICE_NO>" + NoticeNo + "</WEB_NOTICE_NO>";
                    xml += "</PARA>";

                    string[] args = new string[1] { xml };
                    object result = WebServiceHelper.InvokeWebService(url, "getLegalCertRslt", args);
                    if (!WebServiceHelper.GetResultByXml(result.ToString()))
                    {
                        MessageAdd("检定结束，芯片ID认证错误信息：" + WebServiceHelper.GetMessageByXml(result.ToString()));
                    }

                    //4. 读取MDS数据认证结论
                    if (Stop) return;
                    MessageAdd("正在读取数据库认证结论");
                    string sql = string.Format("SELECT * FROM MT_HPLCID_CERT_INFO WHERE DETECT_TASK_NO='{0}' AND WEB_NOTICE_NO='{1}' AND SYS_NO='{2}' AND HANDLE_FLAG=2", taskNo, NoticeNo, sys_no);
                    DataTable table = oraHelper.ExecuteReader(sql);

                    //5. 保存结论
                    string[] resultKey = new string[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;

                        DataRow[] rows = table.Select("BAR_CODE='" + meter.MD_BarCode + "'");
                        ResultDictionary["结论"][i] = ConstHelper.不合格;                                //项目结果
                        if (rows.Length > 0 && rows[0]["IS_LEGAL"].ToString() == "1")   //认证合格
                            ResultDictionary["结论"][i] = ConstHelper.合格;
                    }
                }
                RefUIData("结论");

                MessageAdd("检定结束...", EnumLogType.提示信息);
            }
            catch (Exception)
            {

                SwitchChannel(Cus_ChannelType.通讯485);
                TryStopTest();
                MessageAdd("芯片id检定出现异常...", EnumLogType.错误信息);

            }
        }

        private Dictionary<string, string> ReadID()
        {
            DateTime TmpTime1 = DateTime.Now;
            int YaojianNum = GetYaoJianNum();
            Dictionary<string, string> dicID = new Dictionary<string, string>();
            try
            {
                //正常读取
                while (TimeSubms(DateTime.Now, TmpTime1) < 240000) //读取超时了
                {
                    Dictionary<string, string> dic = MeterProtocolAdapter.Instance.ReadHPLCID();
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        if (!dicID.ContainsKey(kvp.Key))
                            dicID.Add(kvp.Key, kvp.Value);
                    }
                    if (dicID.Count >= YaojianNum + 1) break;
                    System.Threading.Thread.Sleep(1000);
                }
                MessageAdd(string.Format("共读取出{0}块表芯片ID...", dicID.Count), EnumLogType.流程信息);

                //没有读取到的情况
                while (TimeSubms(DateTime.Now, TmpTime1) < 60000) //读取超时了
                {
                    Dictionary<string, string> dic = MeterProtocolAdapter.Instance.ReadHPLCID();
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        if (!dicID.ContainsKey(kvp.Key))
                            dicID.Add(kvp.Key, kvp.Value);
                    }
                    if (dicID.Count >= YaojianNum + 1) break;
                    System.Threading.Thread.Sleep(1000);
                }


            }
            catch (Exception ex)
            {
                //出现异常在读取一次
                MessageAdd("捕抓到芯片ID读取错误" + ex.ToString(), EnumLogType.错误信息);
                //正常读取
                while (TimeSubms(DateTime.Now, TmpTime1) < 240000) //读取超时了
                {
                    Dictionary<string, string> dic = MeterProtocolAdapter.Instance.ReadHPLCID();
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        if (!dicID.ContainsKey(kvp.Key))
                            dicID.Add(kvp.Key, kvp.Value);
                    }
                    if (dicID.Count >= YaojianNum + 1) break;
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return dicID;
        }

        protected override bool CheckPara()
        {
            ItemId = Test_Value;
            //ItemId = "1312";
            ResultNames = new string[] { "表地址", "芯片ID号", "结论", "项目编号" };
            return true;
        }

        private int GetYaoJianNum()
        {
            int N = 0;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    N++;
                }
            }
            return N;

        }
    }
}
