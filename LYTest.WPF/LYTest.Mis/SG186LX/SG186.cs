using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.Mis.Common;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LYTest.Mis.SG186LX
{
    //add yjt 20230131 合并张工河北代码SG186LX
    /// <summary>
    /// 朗新SG186
    /// </summary>
    public class SG186 : OracleHelper, IMis
    {
        private static WebService m_webSev;
        public SG186(string ip, int port, string dataSource, string userId, string pwd, string url1)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url1;
        }




        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            if (string.IsNullOrEmpty(barcode)) return false;

            try
            {
                string url = this.WebServiceURL;

                m_webSev = new WebService(url, "http://server.webservice.core.epm");
                WebService.HeaderBlock[] header = new WebService.HeaderBlock[2];
                header[0] = new WebService.HeaderBlock
                {
                    Name = "username",
                    Content = this.UserId,
                    Namespace = "Authorization",
                    Prefix = "ns1",
                    MustUnderstand = true,
                    Actor = "http://schemas.xmlsoap.org/soap/actor/next"
                };
                header[1] = new WebService.HeaderBlock
                {
                    Name = "password",
                    Content = this.Password,
                    Namespace = "Authorization",
                    Prefix = "ns2",
                    MustUnderstand = true,
                    Actor = "http://schemas.xmlsoap.org/soap/actor/next"
                };

                XmlDocument xml = new XmlDocument();
                string dataxmlstr = "";
                string xmlRtn = "";
                string strBdj = "";
                string strBdjwg = "";
                string strBcs = "";
                string strBcswg = "";

                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET><R><C N=\"BAR_CODE\">" + barcode + "</C></R></DBSET>";
                xmlRtn = string.Empty;

                string[] Params = new string[] { "path:epm/am/calibrate/interfaces/service/ExaminationService", "methodName:GET_METER", "dataXmlStr:" + dataxmlstr };
                xmlRtn = m_webSev.ExeMethod("invokeService", header, Params, "ns1:invokeServiceResponse");

                //try
                //{
                //    MessageBox.Show("下载成功\r\n"+ xmlRtn);
                //    System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DataBase\\输出.txt", xmlRtn);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show("保存失败"+ex.ToString());
                //}

                xml.LoadXml(xmlRtn);
                XmlNodeList topM = xml.DocumentElement.ChildNodes;


                XmlDocument xmlaa = new XmlDocument();

                foreach (XmlElement elm in topM)
                {

                    foreach (XmlElement aa in elm)
                    {


                        if (aa.GetAttribute("N").Equals("APP_NO"))
                        {//申请编号
                            meter.MD_TaskNo = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("SEND_SN"))
                        {//发送批号
                            //meter.Mb_chrzzcj = aa.InnerText;
                            meter.Other4 = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("ORG_NO"))
                        {//供电单位
                            //RV_CODE = aa.InnerText;
                            meter.MD_Customer = aa.InnerText;
                            //meter.MD_Customer = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("CHK_NO"))
                        {//检定编号
                            meter.Other1 = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("METER_ID"))
                        {//电能表标识
                            meter.Other5 = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("CONSIGN_CODE"))
                        {//委托编号
                            //meter.Mb_Bxh = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("BAR_CODE"))
                        {//条码号
                            meter.MD_BarCode = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("ASSET_NO"))
                        {//资产编号
                            meter.MD_AssetNo = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("MADE_NO"))
                        {//出厂编号
                            meter.MD_MadeNo = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("CHK_TYPE_CODE"))
                        {//检定类别
                            //meter.Mb_ChrTxm = aa.InnerText;                                                       //条形码
                            //meter.Mb_ChrJlbh = aa.InnerText;                                                      //申请编号
                            //meter.Mb_ChrCcbh = aa.InnerText;                                                      //出厂编号
                        }
                        else if (aa.GetAttribute("N").Equals("SORT_CODE"))
                        {//类别
                            //EQUIP_CATEG = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("TYPE_CODE"))
                        {//类型
                            meter.MD_MeterType = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("COMM_PROT_CODE"))
                        {//类型
                            if (aa.InnerText.Contains("698") || aa.InnerText.Contains("面向对象"))
                            {
                                meter.MD_ProtocolName = "CDLT698";
                            }
                            else
                            {
                                meter.MD_ProtocolName = "CDLT645";
                            }

                        }
                        else if (aa.GetAttribute("N").Equals("WIRING_MODE"))
                        {//	接线方式
                            switch (aa.InnerText)
                            {
                                case "3":
                                    meter.MD_WiringMode = "三相四线";
                                    break;
                                case "2":
                                    meter.MD_WiringMode = "三相三线";
                                    break;
                                case "1":
                                    meter.MD_WiringMode = "单相";
                                    break;
                                default:
                                    meter.MD_WiringMode = "三相四线";
                                    break;
                            }
                        }
                        else if (aa.GetAttribute("N").Equals("RATED_CURRENT"))
                        {//	电流
                            meter.MD_UA = aa.InnerText.Trim('A').Replace("3*", "").Replace("3x", "");
                            meter.MD_UA = meter.MD_UA.Replace("（", "(").Replace("）", ")");
                        }
                        else if (aa.GetAttribute("N").Equals("MANUFACTURER"))
                        {//	制造单位
                            meter.MD_Factory = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("MODEL_CODE"))
                        {//	型号
                            meter.MD_MeterModel = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("CONST_CODE"))
                        {//	有功常数
                            strBcs = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("RP_CONSTANT"))
                        {//	无功常数
                            strBcswg = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("AP_PRE_LEVEL_CODE"))
                        {//	有功等级
                            strBdj = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("RP_PRE_LEVEL_CODE"))
                        {//	无功等级
                            strBdjwg = aa.InnerText;
                        }
                        else if (aa.GetAttribute("N").Equals("FREQ_CODE"))
                        {//	频率
                            meter.MD_Frequency = 50;
                        }
                        else if (aa.GetAttribute("N").Equals("ADDRESS_CODE"))
                        {//	通讯地址
                            meter.MD_PostalAddress = aa.InnerText;
                        }
                        //else if (aa.GetAttribute("N").Equals("CC_PREVENT_FLAG"))
                        //{//	是否阻尼
                        //    if (aa.InnerText.Contains("1"))
                        //    {
                        //        meter.HasZNQ = true;
                        //    }
                        //    else
                        //    {
                        //        meter.HasZNQ = false;
                        //    }

                        //}
                        else if (aa.GetAttribute("N").Equals("CON_MODE"))
                        {//	接入方式
                            //strBcs = aa.InnerText;
                            if (aa.InnerText.IndexOf("经互感器") >= 0)
                            {
                                meter.MD_ConnectionFlag = "互感式";
                            }
                            else
                            {
                                meter.MD_ConnectionFlag = "直接式";
                            }
                        }
                        else if (aa.GetAttribute("N").Equals("VOLT_CODE"))
                        {//电压
                            if (aa.InnerText.IndexOf("57.7") >= 0)
                            {
                                meter.MD_UB = 57.7f;
                            }
                            else if (aa.InnerText.IndexOf("100") >= 0)
                            {
                                meter.MD_UB = 100;
                            }
                            else if (aa.InnerText.IndexOf("220") >= 0)
                            {
                                meter.MD_UB = 220;
                            }
                            else
                            {
                                if (meter.MD_WiringMode == "单相")
                                {
                                    meter.MD_UB = 220;
                                }
                                else
                                {
                                    meter.MD_UB = 57.7f;
                                }
                            }
                        }
                    }

                    if (meter.MD_WiringMode == "单相")
                    {
                        meter.MD_Grane = strBdj;
                        meter.MD_Constant = strBcs;
                    }
                    else
                    {
                        meter.MD_Grane = strBdj + "(" + strBdjwg + ")";
                        meter.MD_Constant = strBcs + "(" + strBcs + ")";
                    }
                    meter.YaoJianYn = true;

                    //meter._intMyId = 1;
                    //meter.Mb_chrSjdw = "";                                                      //送检单位

                    meter.MD_JJGC = "JJG596-2012";
                    meter.DgnProtocol = null;
                    meter.Seal1 = "";                                                 //铅封1,暂时置空
                    meter.Seal2 = "";                                                 //铅封2,暂时置空
                    meter.Seal3 = "";
                    meter.Other1 = "1";
                    //铅封3,暂时置空
                    //meter.SetBno(index);
                    //MessageBox.Show("下载完成3");
                    return true;
                }


            }
            catch (Exception ex)
            {
                LogManager.AddMessage("获取表信息失败：\r\n" + ex.Message + "\r\n" + ex.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                return false;
            }
            return true;
        }

        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }
        public bool Update(TestMeterInfo meter)
        {
            //XmlDocument xml = new XmlDocument();
            XmlNodeList topM;
            try
            {

                //MessageBox.Show("开始上传数据");
                string url = this.WebServiceURL;

                m_webSev = new WebService(url, "http://server.webservice.core.epm");
                WebService.HeaderBlock[] header = new WebService.HeaderBlock[2];
                header[0] = new WebService.HeaderBlock
                {
                    Name = "username",
                    Content = this.UserId,
                    Namespace = "Authorization",
                    Prefix = "ns1",
                    Actor = "http://schemas.xmlsoap.org/soap/actor/next"
                };
                header[1] = new WebService.HeaderBlock
                {
                    Name = "password",
                    Content = this.Password,
                    Namespace = "Authorization",
                    Prefix = "ns2",
                    Actor = "http://schemas.xmlsoap.org/soap/actor/next"
                };

                string dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET><R><DBSET>";
                string strData = "";//, strDataTemp = "", strJL = "", strKey = "";
                strData = "";

                #region 基本信息
                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";

                //////申请编号	APP_NO	VARCHAR2(16)	申请编号，★必须传入
                //////发送批号	SEND_SN	VARCHAR2(16)	发送批号
                //////检定编号	CHK_NO	VARCHAR2(16)	检定编号
                //////电能表标识	METER_ID	VARCHAR2(16)	电能表的唯一标识，★必须传入
                //////检定人员  	CHECKER_NAME	VARCHAR2(32)	传送代码或名称(在营销系统中的操作员编号或姓名)检定人员  
                //////检定日期  	CHK_DATE	VARCHAR2 (32)	检定日期，★必须传入
                //////核验人员  	CHECKER_NO	VARCHAR2(32)	传送代码或名称(在营销系统中的操作员编号或姓名)核验人员  
                //////核验日期  	CHK_REC_DATE	VARCHAR2(32)	核验日期  
                //////校验台编号 	CHK_DESK_NO	VARCHAR2(16)	校验台编号 
                //////挂表位置 	METER_LOC	VARCHAR2(16)	挂表位置 
                //////标准装置检定证书号 	CERT_ID	VARCHAR2(32)	标准装置检定证书号 
                //////温度	TEMP	VARCHAR2 (16)	温度
                //////湿度	HUMIDITY	VARCHAR2 (16)	湿度
                //////直观检查内容	INTUITIVE_CONTENT	VARCHAR2(254)	直观检查的内容
                //////直观检查和通电检查	INTUITIVE_CONC_CODE	VARCHAR2 (8)	传送代码(qlfFlag)直观检查和通电检查结论，0 不合格 1合格
                //////启动实验结论	START_CONC_CODE	VARCHAR2 (8)	传送代码(qlfFlag)总的启动实验结论，0 不合格 1合格
                //////潜动实验结论	CREEP_CONC_CODE	VARCHAR2 (8)	传送代码(qlfFlag)总的潜动实验结论，0 不合格 1合格
                //////基本误差结论	CHK_CONST	VARCHAR2 (8)	传送代码(qlfFlag)校核常数结论（校核记度器示数结论），0 不合格 1合格，（改成基本误差结论）
                //////结论	CHK_CONC	VARCHAR2(8)	传送代码(qlfFlag)基本误差结论，0 不合格 1合格 ；（改成检定环节的结论）★必须传入
                //////有效日期 	CHK_VALID_DATE	VARCHAR2(32)	有效日期 
                //////检定说明	CHK_REMARK	VARCHAR2(1000)	检定说明
                //////检定依据 	CHK_BASIS	VARCHAR2(254)	检定依据 
                //////检定证书编号	CERT_NO	VARCHAR2(32)	检定证书的编号
                //////成功标志	VARCHAR2(32)	成功，失败返回异常
                //////Exception	错误

                dataxmlstr += "<R>";

                strData = meter.MD_TaskNo;//申请编号
                dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other4;//发送批号
                dataxmlstr += "<C N=\"SEND_SN\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other5;//检定编号//实际下载在Other1但是下载无数据，Other5
                dataxmlstr += "<C N=\"CHK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other5;//电能表标识
                dataxmlstr += "<C N=\"METER_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Checker1;//检定人员
                dataxmlstr += "<C N=\"CHECKER_NAME\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.VerifyDate;//检定日期
                strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                dataxmlstr += "<C N=\"CHK_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Checker2;//核验人员
                dataxmlstr += "<C N=\"CHECKER_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.VerifyDate;//核验日期
                strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                dataxmlstr += "<C N=\"CHK_REC_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.BenthNo;//校验台编号
                dataxmlstr += "<C N=\"CHK_DESK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.MD_Epitope.ToString();//挂表位置
                dataxmlstr += "<C N=\"METER_LOC\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.MD_CertificateNo;//标准装置检定证书号
                dataxmlstr += "<C N=\"CERT_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Temperature;//温度
                dataxmlstr += "<C N=\"TEMP\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Humidity;//湿度
                dataxmlstr += "<C N=\"HUMIDITY\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//直观检查内容
                dataxmlstr += "<C N=\"INTUITIVE_CONTENT\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.外观检查);//直观检查和通电检查
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"INTUITIVE_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetQiQianDongConclusion(meter, ProjectID.起动试验);//启动实验结论
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"START_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetQiQianDongConclusion(meter, ProjectID.潜动试验);//潜动实验结论
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"CREEP_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.基本误差试验);//基本误差结论
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"CHK_CONST\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Result.Trim() == ConstHelper.合格 ? "01" : "02"; ;////结论
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"CHK_CONC\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.ExpiryDate.Trim();//有效日期
                strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                dataxmlstr += "<C N=\"CHK_VALID_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//检定说明
                dataxmlstr += "<C N=\"CHK_REMARK\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.MD_JJGC.Trim();// 检定依据
                dataxmlstr += "<C N=\"CHK_BASIS\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.MD_CertificateNo.Trim();//检定证书编号
                dataxmlstr += "<C N=\"CERT_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";


                dataxmlstr += "</R></DBSET>";
                string[] Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ExaminationService", "methodName:SET_METER_DETECT", "dataXmlStr:" + dataxmlstr };
                //string[] Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ExaminationService", "methodName:SET_METER_DETECT", "dataXmlStr:" + "" };
                string rtnXml1 = string.Empty;
                XmlDocument xml = new XmlDocument();
                rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                xml.LoadXml(rtnXml1);
                if (rtnXml1.Contains("错误"))
                {
                    MessageBox.Show("上传失败，错误原因:" + rtnXml1.ToString());
                    LogManager.AddMessage("上传失败，错误原因:" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }
                topM = xml.DocumentElement.ChildNodes;

                //System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DataBase\\输出.txt", "\r\n基本信息:\r\n" + rtnXml1);
                //MessageBox.Show("上传基本信息完成");

                #endregion

                #region 耐压信息
                try
                {
                    dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";

                    ////申请编号	APP_NO	VARCHAR2(16)	申请编号，必须传入
                    ////发送批号	SEND_SN	VARCHAR2(16)	发送批号
                    ////耐压编号	WITHSTAND_VOLT_NO	VARCHAR2(16)	耐压编号
                    ////设备编号	EQUIP_ID	VARCHAR2(16)	设备编号，必须传入
                    ////耐压人员  	VOLT_CHK_PERSON_NO	VARCHAR2(32)	耐压人员  
                    ////耐压日期  	VOLT_DATE	VARCHAR2 (32)	耐压日期  
                    ////核验人员  	CHECKER_NO	VARCHAR2(32)	核验人员  
                    ////核验日期  	CHK_REC_DATE	VARCHAR2(32)	核验日期  
                    ////耐压台号 	VOLT_DESK_NO	VARCHAR2(16)	耐压台号 
                    ////温度	TEMP	VARCHAR2 (16)	温度
                    ////湿度	HUMIDITY	VARCHAR2 (16)	湿度
                    ////耐压试验值	VOLT_TEST_VALUE	VARCHAR2 (6)	耐压试验值,单位用kv 
                    ////结论	VOLT_CONC	VARCHAR2(6)	耐压试验结论，0不合格、1合格，必须传入
                    ////耐压时间	FARADIC_VOLT_TIME	VARCHAR2(16)	耐压时间


                    dataxmlstr += "<R>";

                    strData = meter.MD_TaskNo;//申请编号
                    dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Other4;//发送批号
                    dataxmlstr += "<C N=\"SEND_SN\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Other5;//耐压编号
                    dataxmlstr += "<C N=\"WITHSTAND_VOLT_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Other5;//设备编号
                    dataxmlstr += "<C N=\"EQUIP_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Checker1;//耐压人员
                    dataxmlstr += "<C N=\"VOLT_CHK_PERSON_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.VerifyDate;//耐压日期
                    strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                    dataxmlstr += "<C N=\"VOLT_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Checker1;//核验人员
                    dataxmlstr += "<C N=\"CHECKER_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.VerifyDate;//核验日期
                    strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                    dataxmlstr += "<C N=\"CHK_REC_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.BenthNo;//耐压台号
                    dataxmlstr += "<C N=\"VOLT_DESK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Temperature;//温度
                    dataxmlstr += "<C N=\"TEMP\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = meter.Humidity;//湿度
                    dataxmlstr += "<C N=\"HUMIDITY\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = "4";//耐压试验值,单位用kv
                    dataxmlstr += "<C N=\"VOLT_TEST_VALUE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = "1";//结论
                    dataxmlstr += "<C N=\"VOLT_CONC\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                    strData = "1";//耐压时间
                    dataxmlstr += "<C N=\"FARADIC_VOLT_TIME\">" + "<![CDATA[" + strData + "]]>" + "</C>";


                    dataxmlstr += "</R></DBSET>";
                    Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/CompressionTestService", "methodName:SET_METER_VOLT_TEST", "dataXmlStr:" + dataxmlstr };
                    rtnXml1 = string.Empty;
                    xml = new XmlDocument();
                    rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                    xml.LoadXml(rtnXml1);
                    if (rtnXml1.Contains("错误"))
                    {
                        MessageBox.Show("耐压实验上传失败，错误原因:：" + rtnXml1.ToString());
                        LogManager.AddMessage("耐压实验上传失败，错误原因:：" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                        return false;
                    }
                    topM = xml.DocumentElement.ChildNodes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("耐压实验上传失败，错误原因:：" + ex.ToString());
                    LogManager.AddMessage("耐压实验上传失败，错误原因:：" + ex.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }


                #endregion

                #region 走字基本信息
                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";

                //////申请编号	APP_NO	VARCHAR2(16)	申请编号，必须传入
                //////发送批号	SEND_SN	VARCHAR2(16)	发送批号
                //////走字编号	WALK_NO	VARCHAR2(16)	走字编号
                //////电能表标识	METER_ID	VARCHAR2(16)	电能表的唯一标识，必须传入
                //////走字人员  	RUNING_PERSON_NAME	VARCHAR2(32)	走字人员  
                //////走字日期  	RUNING_DATE	VARCHAR2 (32)	走字日期 
                //////核验人员  	CHECKER_NO	VARCHAR2(32)	核验人员  
                //////核验日期  	CHK_REC_DATE	VARCHAR2(32)	核验日期  
                //////走字台编号 	RUNING_DESK_NO	VARCHAR2(16)	走字台编号 
                //////温度	TEMP	VARCHAR2 (16)	温度
                //////湿度	HUMIDITY	VARCHAR2 (16)	湿度
                //////结论	CONC_CODE	VARCHAR2(8)	走字结论， 0不合格、1合格；必须传入
                //////说明	RUNING_REMARK	VARCHAR2(1000)	说明
                //////是否已校时 	TIME_CALIBRATE_FLAG	VARCHAR2(16)	是否已校时 1是 0 否


                dataxmlstr += "<R>";

                strData = meter.MD_TaskNo;//申请编号
                dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other4;//发送批号
                dataxmlstr += "<C N=\"SEND_SN\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other5;//走字编号
                dataxmlstr += "<C N=\"WALK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other5;//电能表标识
                dataxmlstr += "<C N=\"METER_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Checker1;//走字人员
                dataxmlstr += "<C N=\"RUNING_PERSON_NAME\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.VerifyDate;//走字日期
                strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                dataxmlstr += "<C N=\"RUNING_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Checker2;//核验人员
                dataxmlstr += "<C N=\"CHECKER_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.VerifyDate;//核验日期
                strData = DateTime.Parse(strData).ToString("yyyy-MM-dd");
                dataxmlstr += "<C N=\"CHK_REC_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.BenthNo;//走字台编号
                dataxmlstr += "<C N=\"RUNING_DESK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Temperature;//温度
                dataxmlstr += "<C N=\"TEMP\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Humidity;//湿度
                dataxmlstr += "<C N=\"HUMIDITY\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.电能表常数试验);//结论
                if (strData == "")
                {
                    strData = "01";
                }
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//说明
                dataxmlstr += "<C N=\"RUNING_REMARK\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "1";//是否已校时
                dataxmlstr += "<C N=\"TIME_CALIBRATE_FLAG\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                dataxmlstr += "</R></DBSET>";
                Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ReadExaminationService", "methodName:SET_METER_DIGIT_WALK", "dataXmlStr:" + dataxmlstr };
                rtnXml1 = string.Empty;
                xml = new XmlDocument();
                rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                xml.LoadXml(rtnXml1);
                if (rtnXml1.Contains("错误"))
                {
                    MessageBox.Show("上传失败，错误原因:" + rtnXml1.ToString());
                    LogManager.AddMessage("上传失败，错误原因:" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }
                topM = xml.DocumentElement.ChildNodes;

                //System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DataBase\\输出.txt","\r\n走字信息:\r\n"+ rtnXml1);
                //MessageBox.Show("上传走字信息完成");
                #endregion

                #region 走字计度器数据
                string XmlTmp = "";
                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";
                //////申请编号	APP_NO	VARCHAR2(16)	申请编号，必须传入
                //////走字编号 	WALK_NO	VARCHAR2(16)	走字编号 
                //////电能表标识	METER_ID	VARCHAR2(16)	电能表的唯一标识，必须传入
                //////计度器类型 	READ_TYPE_CODE	VARCHAR2(8)	传入代码；必须传入；计度器类型 ，11	有功(总)12	有功（尖峰）13	有功（峰）14	有功（谷）15	有功（平）21	无功(总)
                //////22	无功（尖峰）23	无功（峰）24	无功（谷）25	无功（平）26	无功（Q1象限）27	无功（Q2象限）28	无功（Q3象限）29	无功（Q4象限）	
                //////31	最大需量32	累加需量33	小时需量34	30 分钟需量35	冻结量  
                //////起码	LAST_READING	Varchar2(20)	起码
                //////止码	READ	Varchar2(20)	止码★必须传入
                //////同时总起码 	T_LAST_READING	Varchar2(20)	同时总起码 
                //////同时总止码  	T_END_READING	Varchar2(20)	同时总止码  
                //////校核计度器示数误差	CHK_CONST_ERR	VARCHAR2(20)	校核计度器示数误差
                //////费率时段电能示值误差	AR_TS_READING_ERR	VARCHAR2(20)	费率时段电能示值误差
                //////计数器示值组合误差	COMP_ERR	VARCHAR2(20)	计数器示值组合误差
                //////初始示数	INIT_READ	Varchar2(20)	传回初始读数，用于走字后清零情况，不清零传回止码


                XmlTmp = GetLangXinZouzi(meter);//获取走字信息数据
                dataxmlstr += XmlTmp;

                dataxmlstr += "</DBSET>";
                Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ReadExaminationService", "methodName:SET_REGISTER_DIGIT_WALK", "dataXmlStr:" + dataxmlstr };
                rtnXml1 = string.Empty;
                xml = new XmlDocument();
                rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                xml.LoadXml(rtnXml1);
                if (rtnXml1.Contains("错误"))
                {
                    MessageBox.Show("上传失败，错误原因:" + rtnXml1.ToString());
                    LogManager.AddMessage("上传失败，错误原因:" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }
                topM = xml.DocumentElement.ChildNodes;
                //System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DataBase\\输出.txt", "\r\n走字计度器数据:\r\n" + rtnXml1);
                //MessageBox.Show("上传走字计度器数据完成");
                #endregion

                #region 检定结论===起动潜动
                XmlTmp = "";
                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";
                //////申请编号	APP_NO	VARCHAR2(16)	申请编号，必须传入
                //////检定编号	CHK_NO	VARCHAR2(16)	检定编号
                //////电能表标识	METER_ID	VARCHAR2(16)	电能表的唯一标识，必须传入
                //////正反向有无功 	BOTH_WAY_POWER_FLAG	VARCHAR2(16)	正向有功,反向有功,正向无功,反向无功)；必须传入
                //////启动试验结论	START_CONC_CODE	VARCHAR2(8)	启动试验结论，0 不合格 1合格
                //////潜动试验结论	CREEP_CONC_CODE	VARCHAR2(8)	潜动试验结论，0 不合格 1合格
                //////启动电流值	START_CURRENT	VARCHAR2(16)	启动电流值，单位用mA
                //////启动时间	START_DATE	Varchar2(16)	启动时间，单位用分钟

                //获取起动潜动记录
                XmlTmp = GetLangXinQiQiandong(meter);
                dataxmlstr += XmlTmp;


                ////获取潜动记录
                //XmlTmp = GetLangXinQiandong(meter);
                //dataxmlstr = dataxmlstr + XmlTmp;


                dataxmlstr += "</DBSET>";
                Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ExaminationService", "methodName:SET_METER_DETECT_CONC", "dataXmlStr:" + dataxmlstr };
                rtnXml1 = string.Empty;
                xml = new XmlDocument();
                rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                xml.LoadXml(rtnXml1);
                if (rtnXml1.Contains("错误"))
                {
                    MessageBox.Show("上传失败，错误原因:" + rtnXml1.ToString());
                    LogManager.AddMessage("上传失败，错误原因:" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }
                topM = xml.DocumentElement.ChildNodes;
                //System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DataBase\\输出.txt", "\r\n起动潜动:\r\n" + rtnXml1);
                //MessageBox.Show("上传起动潜动数据完成");


                #endregion

                #region 检定误差数据
                XmlTmp = "";
                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";
                ////申请编号	APP_NO	VARCHAR2(16)	申请编号，必须传入
                ////检定编号	CHK_NO	VARCHAR2(16)	检定编号
                ////电能表标识	METER_ID	VARCHAR2(16)	电能表的唯一标识，必须传入
                ////测试元组	TEST_GROUP	VARCHAR2(16)	A:A相,B:B相,C:C相,L:合组，必须传入
                ////正反向有无功	BOTH_WAY_POWER_FLAG	VARCHAR2(16)	正向有功,反向有功,正向无功,反向无功)；必须传入
                ////功率因数	PF	VARCHAR2(16)	功率因数1.0,0.5L,0.25L,0.5C,0.8C ////L(感性)  C(容性)，必须传入
                ////负载电流 	LOAD_CURRENT	VARCHAR2(16)	'Imax','Ib'////Imax,Ib,0.2Ib,0.5Imax,1.5Ib,0.1Ib,0.5Ib，必须传入
                ////误差1	ERR1	VARCHAR2(20)	误差1，必须传入
                ////误差2	ERR2	VARCHAR2(20)	误差2
                ////误差3	ERR3	VARCHAR2(20)	误差3
                ////误差4	ERR4	VARCHAR2(20)	误差4
                ////误差5	ERR5	VARCHAR2(20)	误差5
                ////平均误差	AVE_ERR	VARCHAR2(20)	平均误差
                ////化整误差	INT_CONVERT_ERR	VARCHAR2(20)	化整误差
                ////标准偏差原始值	ORGN_STD_ERR	VARCHAR2(20)	标准偏差原始值
                ////标准偏差化整值	STD_ERR_INT	VARCHAR2(20)	标准偏差化整值
                ////不平衡时误差和平衡时误差之差	LOAD_ERR	VARCHAR2(20)	不平衡时的误差和平衡时的误差之差（机电式、感应式才有）

                XmlTmp = GetLangXinWc(meter);
                dataxmlstr += XmlTmp;
                dataxmlstr += "</DBSET>";
                Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ExaminationService", "methodName:SET_METER_ERR", "dataXmlStr:" + dataxmlstr };
                rtnXml1 = string.Empty;
                xml = new XmlDocument();
                rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                xml.LoadXml(rtnXml1);
                if (rtnXml1.Contains("错误"))
                {
                    MessageBox.Show("上传失败，错误原因:" + rtnXml1.ToString());
                    LogManager.AddMessage("上传失败，错误原因:" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }
                topM = xml.DocumentElement.ChildNodes;

                //System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DataBase\\输出.txt", "\r\n误差:\r\n" + rtnXml1);
                //MessageBox.Show("上传误差数据完成");

                #endregion

                #region 多功能检定
                XmlTmp = "";
                dataxmlstr = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><DBSET>";

                ////申请编号	APP_NO	VARCHAR2(16)	申请编号，必须传入
                ////检定编号	CHK_NO	VARCHAR2(16)	检定编号
                ////电能表标识	METER_ID	VARCHAR2(16)	电能表的唯一标识，必须传入
                ////费率时段检查 	AR_TS_CHK	VARCHAR2(254)	费率时段检查 
                ////费率时段检查结论	TS_CHK_CONC_CODE	VARCHAR2(8)	费率时段检查结论 0 不合格 1合格
                ////费率时段投切误差结论	TS_ERR_CONC_CODE	VARCHAR2(8)	费率时段投切误差结论 0 不合格 1合格
                ////需量周期误差结论	DE_PERIOD_CONC_CODE	VARHCAR2(8)	需量周期误差结论 0 不合格 1合格
                ////由电源供电的时钟试验(s)1	DAILY_TIMING_ERR1	VARCHAR2(20)	由电源供电的时钟试验(s)1
                ////由电源供电的时钟试验(s)2 	DAILY_TIMING_ERR2	VARCHAR2(20)	由电源供电的时钟试验(s)2 
                ////由电源供电的时钟试验(s)3 	DAILY_TIMING_ERR3	VARCHAR2(20)	由电源供电的时钟试验(s)3 
                ////由电源供电的时钟试验(s)4 	DAILY_TIMING_ERR4	VARCHAR2(20)	由电源供电的时钟试验(s)4 
                ////由电源供电的时钟试验(s)5	DAILY_TIMING_ERR5	VARCHAR2(20)	由电源供电的时钟试验(s)5
                ////由电源供电的时钟试验(s)6	DAILY_TIMING_ERR6	VARCHAR2(20)	由电源供电的时钟试验(s)6
                ////由电源供电的时钟试验(s)7	DAILY_TIMING_ERR7	VARCHAR2(20)	由电源供电的时钟试验(s)7
                ////由电源供电的时钟试验(s)8	DAILY_TIMING_ERR8	VARCHAR2(20)	由电源供电的时钟试验(s)8
                ////由电源供电的时钟试验(s)9	DAILY_TIMING_ERR9	VARCHAR2(20)	由电源供电的时钟试验(s)9
                ////由电源供电的时钟试验(s)10	DAILY_TIMING_ERR10	VARCHAR2(20)	由电源供电的时钟试验(s)10
                ////由电源供电的时钟试验平均值 	DAILY_TIMING_ERR_AVG	VARCHAR2(20)	由电源供电的时钟试验平均值 
                ////由电源供电的时钟试验化整值 	DAILY_TIMING_ERR_INT	VARCHAR2(20)	由电源供电的时钟试验化整值 
                ////需量示数误差(负荷点Imax)标准值 	DE_STD_IMAX	VARCHAR2(20)	需量示数误差(负荷点Imax)标准值
                ////需量示数误差(负荷点Imax)实际值 	DE_IMAX	VARCHAR2(20)	需量示数误差(负荷点Imax)实际值
                ////需量示数误差(负荷点Imax) (校验台软件写入)	DEMAND_READING_ERR	VARCHAR2(20)	需量示数误差(负荷点Imax) (校验台软件写入)
                ////需量示数误差负荷点(Imax)化整值	DE_INT_IMAX	VARCHAR2(20)	需量示数误差负荷点(Imax)化整值
                ////需量示数误差(负荷点Ib)标准值	DE_STD_IB	VARCHAR2(20)	需量示数误差(负荷点Ib)标准值
                ////需量示数误差负荷点(Ib)实际值	DE_IB_ACT	VARCHAR2(20)	需量示数误差负荷点(Ib)实际值
                ////需量示数误差(负荷点Ib)	DE_IB	VARCHAR2(20)	需量示数误差(负荷点Ib)
                ////需量示数误差(负荷点Ib)化整值 	DE_IB_INT	VARCHAR2(20)	需量示数误差(负荷点Ib)化整值 
                ////需量示数误差(负荷点0.1Ib)标准值 	DE_P1IB_STD	VARCHAR2(20)	需量示数误差(负荷点0.1Ib)标准值 
                ////需量示数误差(负荷点0.1Ib)实际值 	DE_P1IB_ACT	VARCHAR2(20)	需量示数误差(负荷点0.1Ib)实际值 
                ////需量示数误差(负荷点0.1Ib) 	DE_P1IB	VARCHAR2(20)	需量示数误差(负荷点0.1Ib) 
                ////需量示数误差(负荷点0.1Ib)化整值 	DE_P1IB_INT	VARCHAR2(20)	需量示数误差(负荷点0.1Ib)化整值 
                ////需量选定周期(负荷点Ib)	SEL_PERIOD	VARCHAR2(20)	需量选定周期(负荷点Ib)
                ////需量实测周期(负荷点Ib) (校验台软件写入)	DMD_PERIOD_IB	VARCHAR2(20)	需量实测周期(负荷点Ib) (校验台软件写入)
                ////需量周期误差(负荷点Ib)	DE_PERIOD_IB	VARCHAR2(20)	需量周期误差(负荷点Ib)
                ////需量周期误差(负荷点Ib)化整值 	DE_PERIOD_IB_INT	VARCHAR2(20)	需量周期误差(负荷点Ib)化整值
                ////(电压中断ΔU=100%，t=1s) 变化前电量	BF_PQ	VARCHAR2(20)	(电压中断ΔU=100%，t=1s) 变化前电量
                ////(电压中断ΔU=100%，t=1s) 变化后电量	AF_PQ	VARCHAR2(20)	(电压中断ΔU=100%，t=1s) 变化后电量
                ////(电压中断ΔU=100%，t=1s) 结论	CONC	VARCHAR2(20)	(电压中断ΔU=100%，t=1s) 结论 0 不合格 1合格
                ////(电压中断ΔU=100%，t=20ms) 变化前电量	BF_PQ_U100T20MS	VARCHAR2(20)	(电压中断ΔU=100%，t=20ms) 变化前电量
                ////(电压中断ΔU=100%，t=20ms) 变化后电量	AF_PQ_U100T20MS	VARCHAR2(20)	(电压中断ΔU=100%，t=20ms) 变化后电量
                ////(电压中断ΔU=100%，t=20ms) 结论 	CONC_U100T20MS	VARCHAR2(20)	(电压中断ΔU=100%，t=20ms) 结论 0 不合格 1合格
                ////(电压降落ΔU=50%，t=1min) 变化前电量	BF_PQ_U50T1M	VARCHAR2(20)	(电压降落ΔU=50%，t=1min) 变化前电量
                ////(电压降落ΔU=50%，t=1min) 变化后电量	AF_PQ_U50T1M	VARCHAR2(20)	(电压降落ΔU=50%，t=1min) 变化后电量
                ////(电压降落ΔU=50%，t=1min) 结论	CONCLUSION_U50T1M	VARCHAR2(20)	(电压降落ΔU=50%，t=1min) 结论 0 不合格 1合格
                ////通讯接口测试结论	CI_CHK_CONC_CODE	VARCHAR2(6)	通讯接口测试结论 0 不合格 1合格
                ////无功存储器检查	RP_MEMORY_CHK	VARCHAR2(6)	无功存储器检查 0 不合格 1合格
                ////其它存储器检查	OTHER_MEMORY_CHK	VARCHAR2(6)	其它存储器检查 0 不合格 1合格
                ////GPS对时	GPS_CALIBRATE_FLAG	VARCHAR2(6)	GPS对时 1是 0 否



                dataxmlstr += "<R>";

                strData = meter.MD_TaskNo;//申请编号
                dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other5;//检定编号
                dataxmlstr += "<C N=\"CHK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.Other5;//电能表标识
                dataxmlstr += "<C N=\"METER_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.费率时段检查);//费率时段检查
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"AR_TS_CHK\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.费率时段检查);//费率时段检查结论
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"TS_CHK_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.时段投切);//费率时段投切误差结论
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"TS_ERR_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = "";
                strData = GetBasicConclusion(meter, ProjectID.需量示值误差); //需量周期误差结论
                strData = GetLangXin_CONC_CODE(strData);

                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"DE_PERIOD_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                //获取由电源供电的时钟试验数据
                XmlTmp = GetLangXinRjs(meter);
                dataxmlstr += XmlTmp;


                //获取需量误差数据
                XmlTmp = GetLangXinXlWc(meter);
                dataxmlstr += XmlTmp;


                strData = "";//(电压中断ΔU=100%，t=1s) 变化前电量
                dataxmlstr += "<C N=\"BF_PQ\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压中断ΔU=100%，t=1s) 变化后电量
                dataxmlstr += "<C N=\"AF_PQ\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压中断ΔU=100%，t=1s) 结论
                dataxmlstr += "<C N=\"CONC\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压中断ΔU=100%，t=20ms) 变化前电量
                dataxmlstr += "<C N=\"BF_PQ_U100T20MS\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压中断ΔU=100%，t=20ms) 变化后电量
                dataxmlstr += "<C N=\"AF_PQ_U100T20MS\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压中断ΔU=100%，t=20ms) 结论
                dataxmlstr += "<C N=\"CONC_U100T20MS\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压降落ΔU=50%，t=1min) 变化前电量
                dataxmlstr += "<C N=\"BF_PQ_U50T1M\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压降落ΔU=50%，t=1min) 变化后电量
                dataxmlstr += "<C N=\"AF_PQ_U50T1M\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//(电压降落ΔU=50%，t=1min) 结论
                dataxmlstr += "<C N=\"CONCLUSION_U50T1M\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "1";   //通讯接口测试结论
                dataxmlstr += "<C N=\"CI_CHK_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = ""; //无功存储器检查
                dataxmlstr += "<C N=\"RP_MEMORY_CHK\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//其它存储器检查
                dataxmlstr += "<C N=\"OTHER_MEMORY_CHK\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = GetBasicConclusion(meter, ProjectID.GPS对时);//GPS对时
                strData = GetLangXin_CONC_CODE(strData);
                dataxmlstr += "<C N=\"GPS_CALIBRATE_FLAG\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                dataxmlstr += "</R></DBSET>";
                Params1 = new string[] { "path:epm/am/calibrate/interfaces/service/ExaminationService", "methodName:SET_MULTFUNC_DETECT", "dataXmlStr:" + dataxmlstr };
                rtnXml1 = string.Empty;
                xml = new XmlDocument();
                rtnXml1 = m_webSev.ExeMethod("invokeService", header, Params1, "ns1:invokeServiceResponse");
                xml.LoadXml(rtnXml1);
                if (rtnXml1.Contains("错误"))
                {
                    MessageBox.Show("上传失败，错误原因:" + rtnXml1.ToString());
                    LogManager.AddMessage("上传失败，错误原因:" + rtnXml1.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                    return false;
                }
                topM = xml.DocumentElement.ChildNodes;



                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("上传失败，错误原因:：\r\n" + ex.Message + "\r\n" + ex.ToString());
                LogManager.AddMessage("上传失败，错误原因:：\r\n" + ex.Message + "\r\n" + ex.ToString(), EnumLogSource.数据库存取日志, EnumLevel.Error);
                return false;
            }

        }

        /// <summary>
        ///  获取基本结论
        /// </summary>
        /// <param name="meterInfo"></param>
        /// <param name="dgnItem"></param>
        /// <returns></returns>
        private static string GetBasicConclusion(TestMeterInfo meterInfo, string key)
        {
            Dictionary<string, MeterResult> dic = meterInfo.MeterResults;

            string r;
            if (dic.ContainsKey(key))
            {
                if (dic[key].Result == ConstHelper.不合格)
                    r = "02";     //如果不合格
                else
                    r = "01";
            }
            else
            {
                r = "";
            }

            return r;
        }

        /// <summary>
        ///  获取启动结论
        /// </summary>
        /// <param name="meterInfo"></param>
        /// <param name="dgnItem"></param>
        /// <returns></returns>
        private static string GetQiQianDongConclusion(TestMeterInfo meterInfo, string key)
        {
            string r = "";

            Dictionary<string, MeterQdQid> dic = meterInfo.MeterQdQids;
            foreach (string sKey in dic.Keys)
            {
                if (sKey.IndexOf(key) != -1)
                {
                    if (dic.ContainsKey(sKey))
                    {
                        dic[sKey].Result = dic[sKey].Result.Trim();
                        if (dic[sKey].Result == ConstHelper.不合格)
                            r = "02";     //如果不合格
                        else
                            r = "01";
                    }
                    else
                    {
                        r = "";
                    }
                }
            }
            return r;
        }

        //获取郎新需量误差数据
        public static string GetLangXinXlWc(TestMeterInfo meterInfo)
        {
            string strData = "";
            string dataxmlstr = "";

            ////需量误差数据共有19个数据，数据结构为===Imax示值误差数据5个+Ib示值误差数据5个+0.1Ib示值误差数据5个+周期误差数据4个
            ////示值误差数据结构===标准需量，实际需量，需量示值误差，需量示值误差化整，示值误差结论
            ////周期误差数据结构===需量周期，周期误差，需量周期误差，需量周期误差化整
            //string[] strxlwc = ZH.MisInterface.Common.DataManager.GetXLData(meterInfo);

            //////需量示数误差(负荷点Imax)标准值 	DE_STD_IMAX	VARCHAR2(20)	需量示数误差(负荷点Imax)标准值
            //////需量示数误差(负荷点Imax)实际值 	DE_IMAX	VARCHAR2(20)	需量示数误差(负荷点Imax)实际值
            //////需量示数误差(负荷点Imax) (校验台软件写入)	DEMAND_READING_ERR	VARCHAR2(20)	需量示数误差(负荷点Imax) (校验台软件写入)
            //////需量示数误差负荷点(Imax)化整值	DE_INT_IMAX	VARCHAR2(20)	需量示数误差负荷点(Imax)化整值
            //////需量示数误差(负荷点Ib)标准值	DE_STD_IB	VARCHAR2(20)	需量示数误差(负荷点Ib)标准值
            //////需量示数误差负荷点(Ib)实际值	DE_IB_ACT	VARCHAR2(20)	需量示数误差负荷点(Ib)实际值
            //////需量示数误差(负荷点Ib)	DE_IB	VARCHAR2(20)	需量示数误差(负荷点Ib)
            //////需量示数误差(负荷点Ib)化整值 	DE_IB_INT	VARCHAR2(20)	需量示数误差(负荷点Ib)化整值 
            //////需量示数误差(负荷点0.1Ib)标准值 	DE_P1IB_STD	VARCHAR2(20)	需量示数误差(负荷点0.1Ib)标准值 
            //////需量示数误差(负荷点0.1Ib)实际值 	DE_P1IB_ACT	VARCHAR2(20)	需量示数误差(负荷点0.1Ib)实际值 
            //////需量示数误差(负荷点0.1Ib) 	DE_P1IB	VARCHAR2(20)	需量示数误差(负荷点0.1Ib) 
            //////需量示数误差(负荷点0.1Ib)化整值 	DE_P1IB_INT	VARCHAR2(20)	需量示数误差(负荷点0.1Ib)化整值 
            //////需量选定周期(负荷点Ib)	SEL_PERIOD	VARCHAR2(20)	需量选定周期(负荷点Ib)
            //////需量实测周期(负荷点Ib) (校验台软件写入)	DMD_PERIOD_IB	VARCHAR2(20)	需量实测周期(负荷点Ib) (校验台软件写入)
            //////需量周期误差(负荷点Ib)	DE_PERIOD_IB	VARCHAR2(20)	需量周期误差(负荷点Ib)
            //////需量周期误差(负荷点Ib)化整值 	DE_PERIOD_IB_INT	VARCHAR2(20)	需量周期误差(负荷点Ib)化整值

            ////dataxmlstr += "<R>";
            string[] arr = new string[3];     //imax=_1

            string[] name = new string[] { "需量示值误差_Imax", "需量示值误差_1.0Ib", "需量示值误差_0.1Ib" };
            for (int i = 0; i < name.Length; i++)
            {

                for (int j = 0; j < arr.Length; j++)
                    arr[j] = "";
                if (meterInfo.MeterDgns.Values.Count(x => x.Name == name[i]) > 0)
                {
                    MeterDgn meterDgn = meterInfo.MeterDgns.Values.First(x => x.Name == name[i]);
                    string[] data = meterDgn.Value.Split('|');
                    if (data.Length > 5)
                    {
                        arr[0] = data[3];
                        arr[1] = data[4];
                        arr[2] = data[5];
                    }
                }

                if (name[i] == "需量示值误差_Imax")
                {
                    strData = arr[0];//需量示数误差(负荷点Imax)标准值
                    dataxmlstr += "<C N=\"DE_STD_IMAX\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[1];//需量示数误差(负荷点Imax)实际值
                    dataxmlstr += "<C N=\"DE_IMAX\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[2];//需量示数误差(负荷点Imax) (校验台软件写入)
                    dataxmlstr += "<C N=\"DEMAND_READING_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[2];//需量示数误差负荷点(Imax)化整值
                    dataxmlstr += "<C N=\"DE_INT_IMAX\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                }
                else if (name[i] == "需量示值误差_1.0Ib")
                {
                    strData = arr[0];//需量示数误差(负荷点Ib)标准值
                    dataxmlstr += "<C N=\"DE_STD_IB\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[1];//需量示数误差负荷点(Ib)实际值
                    dataxmlstr += "<C N=\"DE_IB_ACT\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[2];//需量示数误差(负荷点Ib)
                    dataxmlstr += "<C N=\"DE_IB\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[2];//需量示数误差(负荷点Ib)化整值 
                    dataxmlstr += "<C N=\"DE_IB_INT\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                }
                else if (name[i] == "需量示值误差_0.1Ib")
                {
                    strData = arr[0];//需量示数误差(负荷点0.1Ib)标准值
                    dataxmlstr += "<C N=\"DE_P1IB_STD\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[1];//需量示数误差(负荷点0.1Ib)实际值
                    dataxmlstr += "<C N=\"DE_P1IB_ACT\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[2];//需量示数误差(负荷点0.1Ib)
                    dataxmlstr += "<C N=\"DE_P1IB\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                    strData = arr[2];//需量示数误差(负荷点0.1Ib)化整值
                    dataxmlstr += "<C N=\"DE_P1IB_INT\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                }

            }
            if (dataxmlstr != "")
            {
                strData = "15";//需量选定周期(负荷点Ib)
                dataxmlstr += "<C N=\"SEL_PERIOD\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = "15";//需量实测周期(负荷点Ib) (校验台软件写入)
                dataxmlstr += "<C N=\"DMD_PERIOD_IB\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = "0.01";//需量周期误差(负荷点Ib)
                dataxmlstr += "<C N=\"DE_PERIOD_IB\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = "0";//需量周期误差(负荷点Ib)化整值
                dataxmlstr += "<C N=\"DE_PERIOD_IB_INT\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            }
            return dataxmlstr;
        }

        #region 转换
        //郎新结论转化
        private static string GetLangXin_CONC_CODE(string Rslt)
        {
            string strCONC_CODE = "";

            if (Rslt == "01")
            {
                strCONC_CODE = "1";
            }
            else if (Rslt == "02")
            {
                strCONC_CODE = "0";
            }

            return strCONC_CODE;
        }

        //获取郎新走字数据
        public static string GetLangXinZouzi(TestMeterInfo meterInfo)
        {

            string dataxmlstr = "";
            foreach (MeterZZError meter in meterInfo.MeterZZErrors.Values)
            {
                dataxmlstr += "<R>";

                string strData = meterInfo.MD_TaskNo;
                dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterInfo.Other5;//走字编号
                dataxmlstr += "<C N=\"WALK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterInfo.Other5;//电能表标识
                dataxmlstr += "<C N=\"METER_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = SET_REGISTER_DIGIT_WALK(meter.PowerWay, meter.Fl);//计度器类型
                dataxmlstr += "<C N=\"READ_TYPE_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.PowerStart.ToString();//起码
                dataxmlstr += "<C N=\"LAST_READING\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.PowerEnd.ToString();//止码
                dataxmlstr += "<C N=\"READ\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                //add
                strData = meter.PowerStart.ToString();//同时总起码
                dataxmlstr += "<C N=\"T_LAST_READING\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                //add
                strData = meter.PowerEnd.ToString();//同时总止码
                dataxmlstr += "<C N=\"T_END_READING\">" + "<![CDATA[" + strData + "]]>" + "</C>";


                //strData = meterError.Mz_chrQiZiMaC;//校核计度器示数误差
                strData = meter.PowerError;
                dataxmlstr += "<C N=\"CHK_CONST_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meter.PowerError;//费率时段电能示值误差
                dataxmlstr += "<C N=\"AR_TS_READING_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                //strData = meterError.Mz_chrWc;//计数器示值组合误差
                strData = meter.PowerError;
                dataxmlstr += "<C N=\"COMP_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";



                strData = meter.PowerEnd.ToString();//初始示数
                dataxmlstr += "<C N=\"INIT_READ\">" + "<![CDATA[" + strData + "]]>" + "</C>";


                if (meterInfo.MeterDgns.ContainsKey(ProjectID.电量清零))
                {
                    strData = "0";//初始示数
                    dataxmlstr += "<C N=\"INIT_READ\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                }

                dataxmlstr += "</R>";


            }
            return dataxmlstr;
        }

        class QTest
        {
            /// <summary>
            /// 启动试验结论
            /// </summary>
            public string START_CONC_CODE;
            /// <summary>
            /// 潜动试验结论
            /// </summary>
            public string CREEP_CONC_CODE;
            /// <summary>
            /// 启动电流值
            /// </summary>
            public string START_CURRENT;
            /// <summary>
            /// 启动时间
            /// </summary>
            public string START_DATE;
        }
        //获取郎新起动数据
        public static string GetLangXinQiQiandong(TestMeterInfo meterInfo)
        {

            string dataxmlstr = "";
            Dictionary<string, MeterQdQid> _MeterQdQids = meterInfo.MeterQdQids;

            //先根据功率方向将启动和潜动归类
            Dictionary<string, QTest> UpValue = new Dictionary<string, QTest>();
            //4个方向的数据麻
            foreach (MeterQdQid item in _MeterQdQids.Values)
            {
                string fx = item.Name.Substring(item.Name.IndexOf('_') + 1);
                if (!UpValue.ContainsKey(fx))
                {
                    UpValue.Add(fx, new QTest());
                }
                if (item.PrjNo.Substring(0, item.PrjNo.IndexOf('_')) == ProjectID.起动试验)
                {
                    UpValue[fx].START_CONC_CODE = item.Result;
                    UpValue[fx].START_CURRENT = item.Current;
                    UpValue[fx].START_DATE = item.ActiveTime;

                }
                else if (item.PrjNo.Substring(0, item.PrjNo.IndexOf('_')) == ProjectID.潜动试验)
                {

                    UpValue[fx].CREEP_CONC_CODE = item.Result;
                }
            }
            foreach (var key in UpValue.Keys)
            {

                dataxmlstr += "<R>";

                dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + meterInfo.MD_TaskNo + "]]>" + "</C>";        //申请编号
                dataxmlstr += "<C N=\"CHK_NO\">" + "<![CDATA[" + meterInfo.Other5 + "]]>" + "</C>";  //检定编号//实际下载在Other1但是下载无数据，先用other2代替
                dataxmlstr += "<C N=\"METER_ID\">" + "<![CDATA[" + meterInfo.Other5 + "]]>" + "</C>";  //电能表标识

                dataxmlstr += "<C N=\"BOTH_WAY_POWER_FLAG\">" + "<![CDATA[" + key + "]]>" + "</C>";   //正反向有无功

                string strData = UpValue[key].START_CONC_CODE == ConstHelper.合格 ? "1" : "0";
                dataxmlstr += "<C N=\"START_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//潜动试验结论
                if (UpValue[key].CREEP_CONC_CODE != null)
                {
                    strData = UpValue[key].CREEP_CONC_CODE == ConstHelper.合格 ? "1" : "0";//启动试验结论
                }
                dataxmlstr += "<C N=\"CREEP_CONC_CODE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = UpValue[key].START_CURRENT == null ? "" : UpValue[key].START_CURRENT + "mA";//启动电流值
                dataxmlstr += "<C N=\"START_CURRENT\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = UpValue[key].START_DATE ?? "";//启动时间
                dataxmlstr += "<C N=\"START_DATE\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                dataxmlstr += "</R>";
            }
            return dataxmlstr;
        }
        //获取郎新基本误差数据
        public static string GetLangXinWc(TestMeterInfo meterInfo)
        {
            string[] Arr_ID = new string[meterInfo.MeterErrors.Keys.Count];
            string dataxmlstr = "";
            string strData;
            string[] strWcTmp = new string[5];
            meterInfo.MeterErrors.Keys.CopyTo(Arr_ID, 0);
            for (int i = 0; i < Arr_ID.Length; i++)
            {

                MeterError meterErr = meterInfo.MeterErrors[Arr_ID[i]];
                string[] strWc = meterErr.WCData.Split('|');
                for (int j = 0; j < 5; j++)
                {
                    if (j < strWc.Length)
                    {
                        strWcTmp[j] = strWc[j];
                    }
                    else
                    {
                        strWcTmp[j] = "";
                    }
                }
                string strYj;
                switch (meterErr.YJ)
                {
                    case "H":
                        strYj = "L";
                        break;
                    case "A":
                        strYj = "A";
                        break;
                    case "B":
                        strYj = "B";
                        break;
                    case "C":
                        strYj = "C";
                        break;
                    default:
                        strYj = "L";
                        break;
                }
                dataxmlstr += "<R>";

                strData = meterInfo.MD_TaskNo;//申请编号
                dataxmlstr += "<C N=\"APP_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterInfo.Other5;//检定编号
                dataxmlstr += "<C N=\"CHK_NO\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterInfo.Other5;//电能表标识
                dataxmlstr += "<C N=\"METER_ID\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = strYj;//测试元组
                dataxmlstr += "<C N=\"TEST_GROUP\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterErr.GLFX;//正反向有无功
                dataxmlstr += "<C N=\"BOTH_WAY_POWER_FLAG\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterErr.GLYS.Trim();//功率因数
                dataxmlstr += "<C N=\"PF\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterErr.IbX;//负载电流
                if (strData == "1.0Ib")
                {
                    strData = "Ib";
                }
                dataxmlstr += "<C N=\"LOAD_CURRENT\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = strWcTmp[0];//误差1
                dataxmlstr += "<C N=\"ERR1\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = strWcTmp[1];//误差2
                dataxmlstr += "<C N=\"ERR2\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = strWcTmp[2];//误差3
                dataxmlstr += "<C N=\"ERR3\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = strWcTmp[3];//误差4
                dataxmlstr += "<C N=\"ERR4\">" + "<![CDATA[" + strData + "]]>" + "</C>";
                strData = strWcTmp[4];//误差5
                dataxmlstr += "<C N=\"ERR5\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterErr.WCValue;//平均误差
                dataxmlstr += "<C N=\"AVE_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterErr.WCHZ;//化整误差
                dataxmlstr += "<C N=\"INT_CONVERT_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//标准偏差原始值
                dataxmlstr += "<C N=\"ORGN_STD_ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = "";//标准偏差化整值
                dataxmlstr += "<C N=\"STD_ERR_INT\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                strData = meterErr.BPHHZ;//不平衡时误差和平衡时误差之差
                dataxmlstr += "<C N=\"LOAD_ ERR\">" + "<![CDATA[" + strData + "]]>" + "</C>";

                dataxmlstr += "</R>";

            }
            return dataxmlstr;

        }

        //获取郎新由电源供电的时钟试验数据
        public static string GetLangXinRjs(TestMeterInfo meterInfo)
        {
            string dataxmlstr = "";
            string[] RjsWc = new string[10];
            for (int i = 0; i < RjsWc.Length; i++)
            {
                RjsWc[i] = "";
            }
            string strAverage = ""; //平均值
            string strHz = ""; //化整值
            if (meterInfo.MeterDgns.ContainsKey(ProjectID.日计时误差))
            {
                string[] data = meterInfo.MeterDgns[ProjectID.日计时误差].Value.Split('|');
                for (int i = 0; i < data.Length - 3 && i < 10; i++)
                {
                    RjsWc[i] = data[i];
                }

                strAverage = data[data.Length - 3];
                strHz = data[data.Length - 2];

            }


            //strResult = meterInfo.MeterDgns[ItemKey].Value == ConstHelper.合格 ? "01" : "02";
            //strResult = GetLangXin_CONC_CODE(strResult);
            ////由电源供电的时钟试验(s)1	DAILY_TIMING_ERR1	VARCHAR2(20)	由电源供电的时钟试验(s)1
            ////由电源供电的时钟试验(s)2 	DAILY_TIMING_ERR2	VARCHAR2(20)	由电源供电的时钟试验(s)2 
            ////由电源供电的时钟试验(s)3 	DAILY_TIMING_ERR3	VARCHAR2(20)	由电源供电的时钟试验(s)3 
            ////由电源供电的时钟试验(s)4 	DAILY_TIMING_ERR4	VARCHAR2(20)	由电源供电的时钟试验(s)4 
            ////由电源供电的时钟试验(s)5	DAILY_TIMING_ERR5	VARCHAR2(20)	由电源供电的时钟试验(s)5
            ////由电源供电的时钟试验(s)6	DAILY_TIMING_ERR6	VARCHAR2(20)	由电源供电的时钟试验(s)6
            ////由电源供电的时钟试验(s)7	DAILY_TIMING_ERR7	VARCHAR2(20)	由电源供电的时钟试验(s)7
            ////由电源供电的时钟试验(s)8	DAILY_TIMING_ERR8	VARCHAR2(20)	由电源供电的时钟试验(s)8
            ////由电源供电的时钟试验(s)9	DAILY_TIMING_ERR9	VARCHAR2(20)	由电源供电的时钟试验(s)9
            ////由电源供电的时钟试验(s)10	DAILY_TIMING_ERR10	VARCHAR2(20)	由电源供电的时钟试验(s)10
            ////由电源供电的时钟试验平均值 	DAILY_TIMING_ERR_AVG	VARCHAR2(20)	由电源供电的时钟试验平均值 
            ////由电源供电的时钟试验化整值 	DAILY_TIMING_ERR_INT	VARCHAR2(20)	由电源供电的时钟试验化整值 

            string strData = RjsWc[0];
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR1\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[1];//由电源供电的时钟试验(s)2
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR2\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[2];//由电源供电的时钟试验(s)3
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR3\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[3];//由电源供电的时钟试验(s)4
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR4\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[4];//由电源供电的时钟试验(s)5
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR5\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[5];//由电源供电的时钟试验(s)6
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR6\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[6];//由电源供电的时钟试验(s)7
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR7\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[7];//由电源供电的时钟试验(s)8
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR8\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[8];//由电源供电的时钟试验(s)9
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR9\">" + "<![CDATA[" + strData + "]]>" + "</C>";
            strData = RjsWc[9];//由电源供电的时钟试验(s)10
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR10\">" + "<![CDATA[" + strData + "]]>" + "</C>";

            strData = strAverage;//由电源供电的时钟试验平均值 
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR_AVG\">" + "<![CDATA[" + strData + "]]>" + "</C>";

            strData = strHz;//由电源供电的时钟试验化整值 
            dataxmlstr += "<C N=\"DAILY_TIMING_ERR_INT\">" + "<![CDATA[" + strData + "]]>" + "</C>";

            return dataxmlstr;
        }
        #endregion


        #region 标准代码
        /// <summary>
        /// 计度器类型
        /// </summary>
        /// <param name="glfx">功率方向</param>
        /// <param name="fl">费率</param>
        /// <returns></returns>
        private static string SET_REGISTER_DIGIT_WALK(string glfx, string fl)
        {
            string str1 = "1";
            string str2 = "1";

            switch (glfx)
            {
                case "正向有功":
                    str1 = "1";
                    break;
                case "反向有功":
                    str1 = "4";
                    break;
                case "正向无功":
                    str1 = "2";
                    break;
                case "反向无功":
                    str1 = "5";
                    break;
                default:
                    break;
            }
            switch (fl)
            {
                case "总":
                    str2 = "1";
                    break;
                case "尖":
                    str2 = "2";
                    break;
                case "峰":
                    str2 = "3";
                    break;
                case "平":
                    str2 = "5";
                    break;
                case "谷":
                    str2 = "4";
                    break;
                default:
                    break;
            }

            return str1 + str2;
        }
        #endregion

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }
        public bool UpdateCompleted()
        {
            return true;
        }

        public void UpdateInit()
        {

        }

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }
    }
}
