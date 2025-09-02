using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.Mis.Common;
using LYTest.Mis.IMICP.SXDataTables;
using LYTest.Utility.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace LYZD.Mis.ShanXi
{
    public class ShanXiMis : OracleHelper, IMis
    {
        private int SusessCount = 0;//
        public ShanXiMis(string ip, int port, string dataSource, string userId, string pwd, string url)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;
        }
        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            meter.Other5 = ConfigHelper.Instance.MDS_SysNo;
            string str = GetVeriTask(barcode, meter.Other5);
            try
            {
                RecvData data = JsonHelper.反序列化字符串<RecvData>(str);
                if (data.resultFlag == "1")
                {
                    //meter.
                    //meter.MD_SchemeID = data.veriTask.trialSchId;  //检定方案标识
                    ////meter.veriSch = data.veriTask.veriSch;         //方案名称
                    //meter.MD_TerminalModel = data.veriTask.devCls; //设备分类
                                                                   //meter.taskStatus = data.veriTask.taskStatus;   //任务状态
                                                                   //meter.rckSchId = data.veriTask.rckSchId;       //复检方案标识
                    meter.MD_TaskNo = data.veriTask.taskNo;        //任务编号
                                                                   //meter.taskIssuTime = data.veriTask.taskIssuTime; //任务下发时间
                                                                   //meter.autoSealFlag = data.veriTask.autoSealFlag; //是否自动施封
                                                                   //meter.taskCateg = data.veriTask.taskCateg;       //任务类型
                                                                   //meter.taskPri = data.veriTask.taskPri;           //任务优先级
                                                                   //meter.testMode = data.veriTask.testMode;         //检定方式
                                                                   //meter.erpBatchNo = data.veriTask.erpBatchNo;     //ERP物料代码
                                                                   //meter.devNum = data.veriTask.devNum;             //设备数量
                                                                   //meter.tPileNum = data.veriTask.tPileNum;         //总垛数
                    meter.MD_MeterModel = data.veriTask.devModel; //型号
                                                                  //meter. = data.veriTask.equipCodeNew;             //新设备码
                                                                  //meter.veriDevStat = data.veriTask.veriDevStat;   //检定设备状态
                    meter.BatchNo = data.veriTask.arrBatchNo;     //到货批次号

                    string str69 = GetEquioParam("03", barcode);
                    LogManager.AddMessage($"返回数据，------返回数据{str69}");

                    //str69="{\"errorinfo\":\"SUCCESS\",\"resultFlag\":\"1\",\"tmnlDet\":[{\"logAddr\":null,\"commProtVer\":null,\"estabArchDate\":\"2025-02-18 12:00:00\",\"tmnlCarrSoftVer\":\"无\",\"tmnlType\":\"03\",\"tmnlSpec\":\"03\",\"tmnlRs485Route\":null,\"tmnlMfr\":\"10007\",\"devCodeNo\":\"8000000020011311\",\"tmnlConst\":\"003\",\"tmnlChipMfr\":\"无\",\"swVer\":null,\"tmnlReferFreg\":null,\"tmnlCommMode\":\"02\",\"tmnlCarrFreqRng\":\"06\",\"tmnlDoChannel\":null,\"tmnlChipMode\":\"无\",\"tmnlApPreLv\":null,\"commProt\":\"09\",\"ftyNo\":\"441011305\",\"removeDate\":null,\"dsqnDur\":null,\"tmnlCarrType\":\"07\",\"tmnlWireMode\":\"03\",\"tmniReferCur\":null,\"tmnlReferVolt\":\"03\",\"hwVer\":\"05\",\"devStat\":\"01\",\"rmReason\":null,\"dsgnDur\":null,\"tmnlCarrType\":\"07\",\"tmnlWireMode\":\"03\",\"tmnlReferCur\":null,\"tmnlReferVolt\":\"03\",\"hwVer\":\"05\",\"devStat\":\"01\",\"rmReason\":null,\"assetNo\":\"1430009000004410113050\",\"tmnlRc\":\"157\",\"lastChkDate\":null,\"tmnlSampPrcsLv\":null,\"barCode\":\"1430009000004410113050\",\"baudRate\":null,\"tmnlUpChannel\":null,\"madeStd\":null,\"tmnlCollMode\":\"05\",\"tmnlCateg\":\"05\",\"tmnlRv\":\"03\",\"tmnlModel\":\"3117\",\"tmnlRpPreLy\":null,\"arrBatchNo\":\"1425021800002010\"}],\"devCls\":\"09\"}";
                    RecvData data69 = JsonHelper.反序列化字符串<RecvData>(str69);
                    {
                        //读取数据库文件
                        //p_code_type;
                        //p_code;
                        if (data69.tmnlDet.Count > 0)
                        {
                            str = data69.tmnlDet[0].barCode;  //条形码

                            str = data69.tmnlDet[0].tmnlModel; //采集终端型号  //wu
                            string sss = GetData("tmnlModel", data69.tmnlDet[0].tmnlModel);

                            str = data69.tmnlDet[0].tmnlCollMode;//通讯类型    //wu1
                            sss = GetData("tmnlCollMode", data69.tmnlDet[0].tmnlCollMode);

                            string strtype = data69.tmnlDet[0].tmnlType; //类型

                            str = data69.tmnlDet[0].tmnlWireMode;//接线方式
                            meter.MD_WiringMode = GetData("tmnlWireMode", data69.tmnlDet[0].tmnlType);

                            str = data69.tmnlDet[0].tmnlUpChannel; //上行通信信道
                            sss = GetData("tmnlUpChannel", data69.tmnlDet[0].tmnlUpChannel);

                            str = data69.tmnlDet[0].tmnlConst; //有功无功常数
                            sss = GetData("tmnlConst", data69.tmnlDet[0].tmnlConst);
                            meter.MD_Constant = sss + "(" + sss + ")";

                            str = data69.tmnlDet[0].tmnlCateg; //采集终端类别
                            sss = GetData("tmnlCateg", data69.tmnlDet[0].tmnlCateg);

                            str = data69.tmnlDet[0].tmnlHwVer; //采集终端版本

                            //if (strtype == "20" || strtype == "21" || strtype == "22" || strtype == "23" || strtype == "24")
                            //{
                            //    meter.MD_TerminalType = "融合终端";
                            //}
                            //else if (strtype == "27" || strtype == "28")
                            //{
                            //    meter.MD_TerminalType = "能源控制器";
                            //}

                            if (str == "02" && strtype == "")
                            {
                                sss = "13版";
                            }
                            else if (str == "05" && strtype == "")
                            {
                                sss = "22版";
                            }
                            str = data69.tmnlDet[0].tmnlApPreLv; //有功准确度等级
                            sss = GetData("tmnlApPreLv", data69.tmnlDet[0].tmnlApPreLv);

                            str = data69.tmnlDet[0].tmnlRpPreLv; //无功准确度等级
                            sss = GetData("tmnlRpPreLv", data69.tmnlDet[0].tmnlRpPreLv);

                            str = data69.tmnlDet[0].tmnlVoltLv; //交流电压等级
                            str = data69.tmnlDet[0].tmnlRcLv; //交流电流等级
                            str = data69.tmnlDet[0].tmnlReferFreg; //参比频率

                            meter.MD_Frequency = int.Parse(GetData("tmnlReferFreg", str) == "" ? "50" : "50");

                            str = data69.tmnlDet[0].tmnlCommMode;  //通讯方式
                            sss = GetData("tmnlCommMode", data69.tmnlDet[0].tmnlCommMode);

                            str = data69.tmnlDet[0].tmnlRc; //电流
                            sss = GetData("tmnlRc", data69.tmnlDet[0].tmnlRc);


                            str = data69.tmnlDet[0].tmnlReferVolt; //参比电压
                            int temp = 220;
                            int.TryParse(GetData("tmnlReferVolt", data69.tmnlDet[0].tmnlReferVolt), out temp);


                            meter.MD_UB = temp;

                            str = data69.tmnlDet[0].tmnlReferCur;  //参比电流
                            sss = GetData("tmnlReferCur", data69.tmnlDet[0].tmnlRc);
                            meter.MD_UA = sss;

                            str = data69.tmnlDet[0].tmnlDoChannel; //下行通信信道
                            sss = GetData("tmnlDoChannel", data69.tmnlDet[0].tmnlDoChannel);


                            str = data69.tmnlDet[0].tmnlSampPrcsLv; //采样精度等级 
                            sss = GetData("tmnlSampPrcsLv", data69.tmnlDet[0].tmnlSampPrcsLv);

                            str = data69.tmnlDet[0].tmnlSpec; //规格
                            str = data69.tmnlDet[0].tmnlChipMfr; //载波芯片厂商
                            str = data69.tmnlDet[0].tmnlChipMode; //载波芯片型号
                            str = data69.tmnlDet[0].tmnlCarrSoftVer; //载波软件版本
                            str = data69.tmnlDet[0].tmnlCarrFreqRng; //载波频率范围
                            str = data69.tmnlDet[0].tmnlRs485Route; //RS485路数
                            str = data69.tmnlDet[0].tmnlCarrType; //载波类型
                            str = data69.tmnlDet[0].tmnlPlusRoute; //脉冲路数
                            str = data69.tmnlDet[0].tmnlMfr; //生产厂家
                            sss = GetData("tmnlMfr", str);
                            meter.MD_Factory = sss;

                            str = data69.tmnlDet[0].estabArchDate; //建档时间
                            str = data69.tmnlDet[0].lastChkDate;//上次检定时间
                            str = data69.tmnlDet[0].devCodeNo;//设备码
                            str = data69.tmnlDet[0].arrBatchNo;//到货批次号
                            str = data69.tmnlDet[0].devStat;//设备状态
                            str = data69.tmnlDet[0].ftyNo;//出厂编号
                            meter.Other3 = data69.tmnlDet[0].assetNo; //资产编号
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetData(string codeKey, string codevalue)
        {
            try
            {
                string str = "";
                if (!string.IsNullOrWhiteSpace(codevalue))
                {
                    string infoname = "p_code_type";
                    string dataname = "p_code";
                    //GeneralDal generalDal = DALManager.ShanXiData;

                    //DynamicModel model = generalDal.GetByID(infoname, $"devCls ='{"09"}' and paraNo='{codeKey}'");
                    //if (model == null)
                    //{
                    //    return str;
                    //}

                    //DynamicModel models = generalDal.GetByID(dataname, $"codeKey ='{model.GetProperty("codeKey")}' and codeValue ='{codevalue}'");


                    //if (models != null)
                    //{
                    //    str = models.GetProperty("codeValueName").ToString();
                    //}

                    return str;
                }
                else
                {
                    return str;
                }
            }
            catch
            {
                string str = "";
                return str;
            }


        }

        public void DownTask(string type, string data, ref RecvData obj)
        {
            throw new NotImplementedException();
        }
        //public bool DownTask(string MD_BarCode, ref MT_DETECT_OUT_EQUIP mT_DETECT_TASK)
        //{
        //    throw new NotImplementedException();
        //}

        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            string str = GetVeriSchInfo64(barcode);
            Schema = new Dictionary<string, SchemaNode>();
            schemeName = "";
            return true;
        }

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }

        public bool Update(TestMeterInfo meters)
        {
            throw new NotImplementedException();
        }

        public bool Update(List<TestMeterInfo> meters)
        {
            bool flag = false;
            foreach (TestMeterInfo item in meters)
            {

                UploadSealsCode(item); //施封信息上传
                //6.15 数据上传
                flag = ZongResult(item);
                if (!flag)
                {
                    LogManager.AddMessage("综合结论上传失败" + item.MD_BarCode, EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }

                flag = FenxiangResult(item);
                if (!flag)
                {
                    LogManager.AddMessage("分项结论上传失败" + item.MD_BarCode, EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }

            }


            return flag;
        }

        private bool UploadPackInfo(string barCode, string sysno, string detectTaskNo, string moduleBarCode, string id)
        {
            string webNoticeNo = "01";
            string moduleTypeCode = "";

            JObject josend = new JObject
                {
                    { "sysNo", sysno },
                    { "detectTaskNo", detectTaskNo },
                    { "webNoticeNo", webNoticeNo }
                };

            JObject josendInfo = new JObject
                {
                    { "barCode", barCode }, //
                    { "certDate", DateTime.Now.ToString("yyyy-MM-dd") },
                    { "moduleTypeCode", moduleTypeCode },
                    { "moduleBarCode", moduleBarCode },
                    { "hplcID", moduleBarCode },
                    { "isLegal", "00" },
                    { "writeDate", DateTime.Now.ToString("yyyy-MM-dd") }
                };

            josend.Add("hplcInfo", josendInfo);


            string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/getLegalCertRslt";
            string str = josend.ToString();
            JObject json = JObject.Parse(str);
            string log = json.ToString();
            LogManager.AddMessage($"调用接口{url}，上报数据{log}");
            string strrecv = Post(url, str);

            LogManager.AddMessage($"接口{url}，返回数据{strrecv}");
            RecvData recv = JsonHelper.反序列化字符串<RecvData>(strrecv);
            return recv.resultFlag == "1";


        }

        public void UpdateCompleted()
        {

        }

        public bool UpdateCompleted(string DETECT_TASK_NO, string SYS_NO)
        {
            return SendTaskFinish(DETECT_TASK_NO);
        }

        public void UpdateInit()
        {
            SusessCount = 0;
        }

        #region



        /// <summary>
        /// 分项结论上传
        /// </summary>
        /// <param name="meters"></param>
        /// <returns></returns>
        private bool FenxiangResult(TestMeterInfo item)
        {
            item.Checker1 = item.Checker2 = "wuwl3857";
            string sysno = item.Other5;

            VeriDtlFormListsFX objFX = new VeriDtlFormListsFX
            {
                devCls = "09",
                veriTaskNo = item.MD_TaskNo,
                sysNO = sysno
            };

            foreach (var item1 in item.MeterResults)
            {
                //芯片ID单独上传
                if (item1.Key == "02016")
                {
                    //string id = item1.ItemDatas[1].TerminalData;
                    //UploadPackInfo(item.MD_BarCode, sysno, item.MD_TaskNo, item.MD_CertificateNo, id);
                    continue;
                }

                MtDetectTrmlItemRslt objFXYes = new MtDetectTrmlItemRslt
                {
                    veriTaskNo = item.MD_TaskNo, //"检定任务编号",
                    //veriItem = GetItemId(item1.Value.meterResoults[0].Datas["项目名"]).Split(',')[0], //检定项目
                    veriRslt = item1.Value.Result == "合格" ? "02" : "01", //检定结果

                    veriStf = item.Checker1, //检定人员
                    veriDate = item.VerifyDate.Replace("/", "-"), //检定日期
                    //veriItemNo = GetItemId(item1.Value.meterResoults[0].Datas["项目名"]).Split(',')[1],  //检定项编号
                    //plantNo = ConfigHelper.Instance.PlantNO,  //设备档案编号，检定线台标识
                    plantElementNo = sysno,  //设备单元编号，多功能检定单元标识
                    machNo = sysno,          //专机编号，多功能检定仓标识
                    devSeatNo = item.MD_Epitope.ToString(), //表位编号
                    trialStf = item.Checker2,    //试验人员
                    checkStf = item.Checker2,    //核验人员
                    assetNo = item.Other3,               //资产编号 //挂表扫码
                    barCode = item.MD_BarCode
                };
                objFX.mtDetectTrmlItemRslt.Add(objFXYes);

                int count = 1;
                //foreach (var itemss in item1.Value.meterResoults[0].ItemDatas)
                //{
                //    MtDetectTrmlSubitemRslt objFXNo = new MtDetectTrmlSubitemRslt
                //    {
                //        veriSpotName = itemss.Name,     // 检定点名称
                //        veriTaskNo = item.MD_TaskNo,    // 检定点编号--任务编号
                //        veriRslt = itemss.Resoult == "合格" ? "02" : "01", // 检定结果 
                //        veriSpotNo = count.ToString(),  // 检定点编码
                //        assetNo = item.Other3,          // 资产编号
                //        sysNo = sysno,                  // 系统编号
                //        veriOrgNo = "01",               // 检定单位
                //        veriDate = Convert.ToDateTime(item.VerifyDate).ToString(),  // 检定日期

                //        veriData = "",                  // 检定数据
                //        veriItemParaNo = GetItemId(item1.Value.meterResoults[0].Datas["项目名"]).Split(',')[1],  // 检定项参数编码,
                //        barCode = item.MD_BarCode
                //    };
                //    objFX.mtDetectTrmlSubitemRslt.Add(objFXNo);
                //    count++;
                //}
            }

            string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/getDETedTestData";

            string str = JsonHelper.序列化对象(objFX);
            JObject json = JObject.Parse(str);
            string log = json.ToString();
            LogManager.AddMessage($"调用接口{url}，上报数据{log}");
            string strrecv = Post(url, str);
            LogManager.AddMessage($"分项结论上传，返回数据{strrecv}");
            RecvData recv = JsonHelper.反序列化字符串<RecvData>(strrecv);

            LogManager.AddMessage($"分项结论上传完成{item.MD_BarCode}---{strrecv}", EnumLogSource.服务器日志, EnumLevel.Information);
            return recv.resultFlag == "1";


        }

        /// <summary>
        /// 总结论上传
        /// </summary>
        /// <param name="meters"></param>
        /// <returns></returns>
        private bool ZongResult(TestMeterInfo item)
        {
            try
            {
                item.Checker1 = item.Checker2 = "wuwl3857";
                //"1409202411170006"
                string sysno = item.Other5;

                VeriDtlFormLists obj = new VeriDtlFormLists
                {
                    sysNO = sysno,
                    veriTaskNo = item.MD_TaskNo,
                    devCls = "09"
                };
                if (item.Result == ConstHelper.合格)
                {
                    VeriDtlFormList objYes = new VeriDtlFormList
                    {
                        veriTaskNo = item.MD_TaskNo,
                        plantElementNo = sysno,
                        machNo = sysno,
                        devSeatNo = item.MD_Epitope.ToString(),
                        devCls = "09",
                        assetNo = item.Other3,   //--资产编号
                        barCode = item.MD_BarCode,
                        veriRslt = item.Result == "合格" ? "02" : "01",
                        veriStf = item.Checker1,
                        veriDept = "",
                        veriDate = item.VerifyDate.Replace("/", "-"),
                        faultReason = "",  //-- 
                        checkStf = item.Checker2,
                        trialStf = item.Checker2,
                        //plantNo = ConfigHelper.Instance.PlantNO,
                        //platformNo = item.MD_DeviceID,
                        temp = item.Temperature,
                        humid = item.Humidity,
                        checkDate = item.VerifyDate.Replace("/", "-"),
                        frstLvFaultReason = "",  //-- 
                        scndLvFaultReason = "" //-- 
                    };
                    obj.veriDtlFormList.Add(objYes);
                }
                if (item.Result == ConstHelper.不合格)
                {
                    VeriDisqualReasonList objNO = new VeriDisqualReasonList
                    {
                        veriTaskNo = item.MD_TaskNo,
                        sysNo = sysno,
                        veriUnitNo = "",
                        barCode = item.MD_BarCode,
                        disqualReason = ""
                    };
                    obj.veriDisqualReasonList.Add(objNO);
                }

                //string url = GetJson("6.12");
                string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/setResults";
                string str = JsonHelper.序列化对象(obj);
                JObject json = JObject.Parse(str);
                string log = json.ToString();
                LogManager.AddMessage($"调用接口{url}，上报数据{log}");
                string strrecv = Post(url, str);
                LogManager.AddMessage($"综合结论上传，接口{url}，返回数据{strrecv}");
                RecvData recv = JsonHelper.反序列化字符串<RecvData>(strrecv);
                return recv.resultFlag == "1";


            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.Message, EnumLogSource.服务器日志, EnumLevel.Error);
                return false;
            }
        }
        #endregion

        #region  检定业务接口
        /// <summary>
        /// 检定任务信息获取 6.1
        /// </summary>
        private string GetVeriTask(string barcode, string sysNO)
        {
            try
            {
                string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/getVeriTask";

                SXDataTableSend obj = new SXDataTableSend
                {
                    barCode = barcode,
                    sysNo = sysNO
                };
                string str = JsonHelper.序列化对象(obj);
                LogManager.AddMessage($"调用接口{url}，上报数据{str}");
                string strrecv = Post(url, str);

                LogManager.AddMessage($"接口{url}，返回数据{strrecv}");
                return strrecv;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.Message, EnumLogSource.服务器日志, EnumLevel.Error);
                return "";
            }

        }

        /// <summary>
        /// 6.2
        /// </summary>
        /// <param name="v"></param>
        private string GetEquipDet()
        {
            try
            {
                SendData62 obj = new SendData62
                {
                    taskNo = "1401731313767250",
                    sysNo = "997",
                    pageNo = 0,
                    pageSize = 1000
                };

                string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/getEquipDET";

                string str = JsonHelper.序列化对象(obj);
                LogManager.AddMessage($"调用接口{url}，上报数据{str}");
                string strrecv = Post(url, str);
                LogManager.AddMessage($"接口{url}，返回数据{strrecv}");

                return strrecv;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.Message, EnumLogSource.服务器日志, EnumLevel.Error);
                return "";
            }
        }

        /// <summary>
        /// 6.4 检定方案信息获取接口
        /// </summary>
        /// <param name="v"></param>
        /// <param name="meter"></param>
        /// <returns></returns>
        private string GetVeriSchInfo64(string SchId)
        {
            try
            {
                GetVeriSchInfo obj = new GetVeriSchInfo
                {
                    trialSchId = SchId
                };

                string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/getVeriSchInfo";

                string str = JsonHelper.序列化对象(obj);
                LogManager.AddMessage($"调用接口{url}，上报数据{str}");
                string strrecv = Post(url, str);

                LogManager.AddMessage($"接口{url}，返回数据{strrecv}");

                return strrecv;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.Message, EnumLogSource.服务器日志, EnumLevel.Error);
                return "";
            }
        }

        /// <summary>
        /// 6.9
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        private string GetEquioParam(string type, string data)
        {
            string url = $"http://{Ip}:{Port}/restful/common/getEquipParam";

            SXDataTableSend obj = new SXDataTableSend
            {
                devCls = "09",
                type = type,
                barCodes = data,
                arrBatchNo = "",
                devCodeNo = ""
            };
            string str = JsonHelper.序列化对象(obj);

            LogManager.AddMessage($"调用接口{url}，上报数据{str}");
            string strrecv = Post(url, str);
            LogManager.AddMessage($"接口{url}，返回数据{strrecv}");
            return strrecv;
        }

        /// <summary>
        /// 6.14
        /// </summary>
        /// <returns></returns>
        public bool UploadSealsCode(TestMeterInfo item)
        {
            string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/uploadSealsCode";

            UpLoadSeal obj = new UpLoadSeal
            {
                devCls = "09",
                taskNo = item.MD_TaskNo,
                sysNo = item.Other5,
                psOrgNo = "01" //供电单位编号
            };

            SealInst seal = new SealInst
            {
                barCode = item.MD_BarCode,
                sealPosition = "01",
                sealBarCode = item.Other4,
                sealDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sealerNo = item.Checker1,
                validFlag = "01",
                releaseInfo = ""
            };
            obj.sealInst.Add(seal);

            string str = JsonHelper.序列化对象(obj);

            LogManager.AddMessage($"调用接口{url}，上报数据{str}");

            string strrecv = Post(url, str);
            LogManager.AddMessage($"接口{url}，返回数据{strrecv}");
            RecvData recv = JsonHelper.反序列化字符串<RecvData>(strrecv);

            return recv.resultFlag == "1";

        }


        /// <summary>
        /// 6.18
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="jsonParas"></param>
        /// <returns></returns>

        public bool SendTaskFinish(string taskno)
        {
            SXDataTableSend obj = new SXDataTableSend
            {
                taskNo = taskno,
                flag = "01"
            };
            string url = $"http://{Ip}:{Port}/restful/sxyk/detectService/sendTaskFinish";

            string str = JsonHelper.序列化对象(obj);
            LogManager.AddMessage($"调用接口{url}，上报数据{str}");
            string strrecv = Post(url, str);

            LogManager.AddMessage($"接口{url}，返回数据{strrecv}");
            RecvData recv = JsonHelper.反序列化字符串<RecvData>(strrecv);
            return recv.resultFlag == "1";

        }
        #endregion

        #region 方法
        public string Post(string Url, string jsonParas)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/Json";

            //设置参数，并进行URL编码 
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(jsonParas);
            request.ContentLength = payload.Length;

            Stream writer;
            try
            {
                writer = request.GetRequestStream();
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("连接服务器失败" + ex.Message, EnumLogSource.服务器日志, EnumLevel.Tip);
                return "连接服务器失败" + ex.Message;
            }

            writer.Write(payload, 0, payload.Length);
            writer.Close();

            HttpWebResponse response;
            try
            {
                //获得响应流
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }
            Stream s = response.GetResponseStream();
            //  Stream postData = Request.InputStream;
            StreamReader sRead = new StreamReader(s);
            string postContent = sRead.ReadToEnd();
            sRead.Close();
            return postContent;//返回Json数据
        }



        public class JsonHelper
        {
            public static string 序列化对象(object obj)
            {

                return JsonConvert.SerializeObject(obj);
            }

            public static T 反序列化字符串<T>(string data)
            {
                return JsonConvert.DeserializeObject<T>(data);  // 尖括号<>中填入对象的类名
            }
        }

        //public string GetJson(string str)
        //{
        //    string url = "";
        //    switch (str)
        //    {
        //        case "6.1":
        //            url = "/restful/sxyk/detectService/getVeriTask";
        //            break;
        //        case "6.2":
        //            url = "/restful/sxyk/detectService/getEquipDET";
        //            break;
        //        case "6.4":
        //            url = "/restful/sxyk/detectService/getVeriSchInfo";
        //            break;
        //        case "6.9":
        //            url = "/restful/common/getEquipParam";
        //            break;
        //        case "6.12":
        //            url = "/restful/sxyk/detectService/setResults";
        //            break;
        //        case "6.13":
        //            url = "/restful/sxyk/detectService/getDETedTestData";
        //            break;
        //        case "6.14":
        //            url = "/restful/sxyk/detectService/uploadSealsCode";
        //            break;
        //        case "6.15":
        //            url = "/restful/sxyk/detectService/getLegalCertRslt";
        //            break;
        //        case "6.18":
        //            url = "/restful/sxyk/detectService/sendTaskFinish";
        //            break;

        //    }
        //    return "http://" + Ip + ":" + Port + url;
        //}


        private string GetItemId(string Name)
        {
            switch (Name)
            {
                case "时钟召测和对时":
                    return "时钟召测和对时,1";
                case "基本参数":
                    return "基本参数,2";
                case "抄表与费率参数":
                    return "抄表与费率参数,3";
                //case "基本参数":
                //    return "以太网参数设置,2";
                case "状态量采集":
                    return "状态量采集,8";
                case "12个/分脉冲量采集":
                    return "12个/分脉冲量采集,10";
                case "120个/分脉冲量采集":
                    return "120个/分脉冲量采集,11";
                case "总加组日和月电量召集":
                    return "总加组日电量与月电量采集,12";
                case "分时段电能量数据存储":
                    return "分时段电能量数据存储,13";
                case "实时和当前数据":
                    return "实时和当前数据,15";
                case "历史日数据":
                    return "历史日数据,16";
                case "负荷曲线":
                    return "负荷曲线,17";
                case "历史月数据":
                    return "历史月数据,18";
                case "时段功控":
                    return "时段功控,20";
                case "厂休功控":
                    return "厂休功控,21";
                case "营业报停功控":
                    return "营业报停功控,22";
                case "当前功率下浮控":
                    return "营业报停功控,23";
                case "月电控":
                    return "月电控,24";
                case "购电控":
                    return "购电控,25";
                case "催费告警":
                    return "催费告警,26";
                case "保电功能":
                    return "保电功能,27";
                case "剔除功能":
                    return "剔除功能,28";
                case "遥控功能":
                    return "遥控功能,29";
                case "电能表超差事件":
                    return "电能量超差事件,38";
                case "电能表飞走事件":
                    return "电能表飞走事件,39";
                case "电能表停走事件":
                    return "电能表停走事件,40";
                case "电能表时间超差事件":
                    return "电能表时间超差事件,41";
                case "终端停/上电事件":
                    return "终端停/上电事件,50";
                case "终端485抄表错误":
                    return "终端485抄表错误事件,55";
                case "终端对时事件":
                    return "对时事件,64";
                case "常温基本误差":
                    return "常温基本误差,70";
                case "功率因素基本误差":
                    return "功率因素基本误差,71";
                case "谐波影响":
                    return "谐波影响,72";
                case "频率影响":
                    return "频率影响,73";
                case "电流不平衡影响":
                    return "电流不平衡影响,74";
                case "电源影响":
                    return "电源影响试验,75";
                case "日计时误差":
                    return "日计时误差,91";
                case "终端维护":
                    return "数据初始化,99";
                //case "密钥下装":
                //    return "身份认证及密钥协商,100";
                case "密钥下装":
                    return "密钥下装,100";
                case "事件参数":
                    return "事件参数,7";
                case "电能表实时数据":
                    return "电能表实时数据,14";
                case "电能表当前数据":
                    return "电能表当前数据,9";
                case "读取终端信息":
                    return "读取终端信息,19";
                case "终端逻辑地址查询":
                    return "终端逻辑地址查询,65";
                //case "交采电量清零":
                //    return "数据初始化(通信参数除外),66";
                case "交采电量清零":
                    return "交采电量清零,66";
                case "终端编程事件":
                    return "终端编程事件,67";
                case "终端密钥恢复":
                    return "终端密钥恢复,97";
                case "安全模式":
                    return "禁用安全模式字,79";
                case "外观":
                    return "外观检查,80";
                case "全事件采集上报":
                    return "全事件采集上报,80";

            }
            return "";
        }

        //public bool Update(TestMeterInfo meter)
        //{
        //    throw new NotImplementedException();
        //}

        bool IMis.UpdateCompleted()
        {
            throw new NotImplementedException();
        }

        //public bool Down(string barcode, ref TestMeterInfo meter)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        //{
        //    throw new NotImplementedException();
        //}

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
