using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.Mis.Common;
using LYTest.Mis.IMICP.IMICPDataTables;
using LYTest.Utility.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace LYTest.Mis.IMICP
{
    /// <summary>
    /// 智慧计量工控平台
    /// </summary>
    public class SmartmeteringIndustrialPlatform : OracleHelper, IMis
    {
        public string SysNo { get; private set; } = "450";

        public string trialschId = "";
        private int SusessCount = 0;//
        public SmartmeteringIndustrialPlatform(string ip, int port, string dataSource, string userId, string pwd, string url, string sysno)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;
            SysNo = ConfigHelper.Instance.MDS_SysNo;
        }
        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode)) return false;
                //6.1 检定台任务信息获取接口
                string strrecv = getVeriTask(barcode, SysNo);
                // strrecv = "{ \"resultFlag\":\"结果标识\", \"errorInfo\":\"错误信息\", \"veriTask\":{ \"trialSchId\":\"检定方案标识\", \"veriSch\":\"试验方案名称\", \"devCls\":\"设备分类\", \"taskStatus\":\"任务状态\", \"rckSchId\":\"复检方案标识\", \"taskNo\":\"检定任务号\", \"taskIssuTime\":\"任务下发时间\", \"autoSealFlag\":\"是否自动施封\", \"taskCateg\":\"任务类型\", \"taskPri\":\"任务优先级\", \"testModeveriReason\":\"检定方式\", \"erpBatchNo\":\"ERP物料代码\", \"devNum\":\"设备数量\", \"tPileNum\":\"总垛数\", \"devModel\":\"型号\", \"equipCodeNew\":\"新设备码\", \"veriDevStat\":\"设备状态码\", \"arrBatchNo\":\"到货批次号\" } }";
                RecvData recv = JsonHelper.反序列化字符串<RecvData>(strrecv);
                if (recv == null || string.IsNullOrWhiteSpace(recv.veriTask.taskNo)) return false;

                meter.MD_TaskNo = recv.veriTask.taskNo; // 检定任务号
                trialschId = recv.veriTask.trialSchId;  // 检定方案标识
                                                        //获取资产信息

                //6.2检定出库明细获取接口
                JObject josend = new JObject
                {
                    { "taskNo", meter.MD_TaskNo },
                    { "sysNo", "45001" },
                    { "pageNo", 0 },
                    { "pageSize", 1000 }
                };

                //josend = new JObject
                //{
                //    { "devCls", "01" },
                //    { "type", "03" },
                //    { "barCodes", "202506241646" },
                //    { "arrBatchNo", "200709070005" },
                //    { "devCodeNo", "202506241646" },

                //};
                //设备分类 devCls  VARCHAR2(16)    是 设备分类
                //获取类型 type    VARCHAR2(8) 是 获取类型，三选一，到货批次号 = 01 / 设备码 = 02 / 条形码串 = 03
                //条形码串 barCodes    VARCHAR2(2048)  否 条形码串，逗号分割
                //到货批次号   arrBatchNo VARCHAR2(32)	否 到货批次号
                //设备码 devCodeNo   VARCHAR2(32)    否 设备码
                string str = josend.ToString();
                string url = GetJson("6.2");
                strrecv = Post(url, str, 6);
                var recvGetEquipDETData = JsonHelper.反序列化字符串<RecvGetEquipDETData>(strrecv);
                //解析
                //            检定任务号 taskNo  VARCHAR2(32)    是 检定任务号
                //系统编号 sysNo   VARCHAR2(32)    是 系统编号
                //页数 pageNo  INT 是   页数，默认从0开始
                //页大小 pageSize INT 是 页大小，<= 2000
                return true;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.Message + "\r\n" + ex, EnumLogSource.服务器日志, EnumLevel.Error);
                return false;
            }

        }

        /// <summary>
        /// 6.1 检定台任务信息获取接口
        /// </summary>
        /// <param name="str"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string getVeriTask(string barCode, string sysNo)
        {
            try
            {
                JObject josend = new JObject
                {
                    { "barCode", barCode },
                    { "sysNo", sysNo }
                };
                string str = josend.ToString();
                string url = GetJson("6.1");
                string strrecv = Post(url, str, 6);
                return strrecv;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(ex.Message, EnumLogSource.服务器日志, EnumLevel.Error);
                return "";
            }
        }




        public readonly static Dictionary<string, Dictionary<string, string>> PCodeTable = new Dictionary<string, Dictionary<string, string>>();
        /// <summary>
        /// 方案编号
        /// </summary>
        /// <param name="barcode">trialSchId 方案标识</param>
        /// <param name="schemeName"></param>
        /// <param name="Schema"></param>
        /// <returns></returns>
        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            JObject josend = new JObject
            {
                { "trialSchId", trialschId }
            };
            string str = josend.ToString();
            string url = GetJson("6.4");
            string strrecv = Post(url, str, 6);
            //解析

            throw new NotImplementedException();
        }

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }

        public bool Update(TestMeterInfo meters)
        {
            if (PCodeTable.Count <= 0)
                GetDicPCodeTable();
            GetAllTask(meters);
            throw new NotImplementedException();
        }

        public bool Update(List<TestMeterInfo> meters)
        {
            bool flag = false;
            return flag;
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


        public bool SendTaskFinish(string taskno)
        {
            return true;
        }



        public string GetAllTask(TestMeterInfo meters)
        {

            #region  检定综合结论
            // 检定明细单
            VeriDtlFormList veriDtlFormList = new VeriDtlFormList
            {
                veriTaskNo = meters.MD_TaskNo,          //检定任务编号
                plantElementNo = "",                    //设备单元编号
                machNo = "02",                          //专机编号
                devSeatNo = meters.MD_Epitope.ToString(),   //表位编号
                devCls = "02",                              //设备分类
                assetNo = meters.MD_AssetNo.ToString(),     //资产编号
                barCode = meters.MD_BarCode,                //条形码
                veriRslt = meters.Result == "合格" ? "02" : "01",  //检定结果
                veriStf = meters.Checker1,   //检定人员
                veriDept = "",  //检定部门
                veriDate = meters.VerifyDate,  //检定日期
                faultReason = "", //故障原因
                checkStf = meters.Checker2, //核验人员
                trialStf = "", //试验人员
                plantNo = "",  //设备档案编号，检定线台标识
                platformNo = meters.BenthNo,    //台体编号
                temp = meters.Temperature,  //温度
                humid = meters.Humidity,    //湿度
                checkDate = SysNo,          //核验日期
                frstLvFaultReason = "",     //一级故障原因
                scndLvFaultReason = ""      //二级故障原因
            };

            // VeriDisqualReasonList
            VeriDisqualReasonList veriDisqualReasonList = new VeriDisqualReasonList
            {
                veriTaskNo = meters.MD_TaskNo,
                sysNo = SysNo,
                veriUnitNo = "",
                barCode = meters.MD_BarCode,
                disqualReason = ""
            };

            MeterVeriCompConc meterVeriCompConc = new MeterVeriCompConc
            {
                veriTaskNo = meters.MD_TaskNo,  // 检定任务编号
                areaCode = "",//地区码
                psOrgNo = "",//供电单位编号
                barCode = meters.MD_BarCode,//条形码
                intuitConcCode = meters.MeterDefaults.ToString(),//外观标志及通电检查试验结论
                basicErrorConcCode = "",//基本误差试验结论
                constConcCode = "",//电能表常数试验结论
                creepingConcCode = "",//潜动试验结论
                dayerrConcCode = "",//日计时误差试验结论
                powerConcCode = "",//功率消耗试验结论
                voltConcCode = "",//交流电压试验结论
                standardConcCode = "",//通信规约一致性检查试验结论
                waveConcCode = "",//载波通信性能试验结论
                errorConcCode = "",//误差变差试验结论
                consistConcCode = "",//误差一致性试验结论
                variationConcCode = "",//负载电流升降变差试验结论
                overloadConcCode = "",//电流过载试验结论
                tsConcCode = "",//时段投切误差试验结论
                runingConcCode = "",//走字实验误差试验结论
                periodConcCode = "",//需量周期误差试验结论
                valueConcCode = "",//需量示值误差试验结论
                keyConcCode = "",//密钥下装试验结论
                settingConcCode = "",//参数设置试验结论
                esamConcCode = "",//安全认证试验结论
                remoteConcCode = "",//费控远程数据回抄试验结论
                ehConcCode = "",//费控保电试验结论
                warnConcCode = "",//费控告警试验结论
                surplusConcCode = "",//剩余电量递减准确度试验结论
                ecConcCode = "",//费控取消保电试验结论
                warnCancelConcCode = "",//费控取消告警试验结论
                switchOnConcCode = "",//费控合闸试验结论
                switchOutConcCode = "",//费控拉闸试验结论
                resetEqMetConcCode = "",//电能表清零试验结论
                resetDemandMetConcCode = "",//
                timingMetConcCode = "",//
                comminicateMetConcCode = "",//
                addressMetConcCode = "",
                multiInterfaceMetConcCode = "",//
                leapYearMetConc = "",//
                paraReadMetConcCode = "",//
                paraSetMetConc = "",//
                deviationMetConc = "",//
                gpsConc = "",//
                sealHandleFlag = "",//
                sealHandleDate = "",//
                startingConcCode = "",
                presetPara1CheckConcCode = "",
                passwordChangeConcCode = "",
                presetPara1SetConcCode = "",
                reliabilityConcCode = "",
                moveStabilityTestConcCode = "",
                antiSeismictestConcCode = "",
                presetPara2CheckConcCode = "",
                presetPara3CheckConcCode = "",
                presetPara2SetConcCode = "",
                presetPara3SetConcCode = "",
                hwVer = "",
                faultReason = "",
                eleEnergyFuncConcCode = "",
                rateTimeFuncConcCode = "",
                eventRecFuncConcCode = "",
                influenceQtyConcCode = ""
            };

            // 检定综合结论
            SetResults set = new SetResults
            {
                veriTaskNo = "",
                devCls = "",
                sysNo = SysNo,
            };

            set.veriDtlFormList.Add(veriDtlFormList);
            set.veriDisqualReasonList.Add(veriDisqualReasonList);
            set.meterVeriCompConc.Add(meterVeriCompConc);

            #endregion

            #region 检定分项结论接口

            SubResult sRst = new SubResult();

            CommonDatas_DETedTestData data = new CommonDatas_DETedTestData
            {
                veriMeterRsltId = "",
                areaCode = "",
                psOrgNo = "",
                devId = "",
                assetNo = meters.MD_AssetNo,
                barCode = meters.MD_BarCode,
                veriTaskNo = meters.MD_TaskNo,
                devCls = "02",
                sysNo = SysNo,
                plantNo = "",
                plantElementNo = "",
                machNo = "",
                devSeatNo = meters.MD_Epitope.ToString(),
                veriCaliDate = Convert.ToDateTime(meters.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss")
            };

            //基本误差
            sRst.vBasicerrMeterConc.AddRange(GetvBasicerrMeterConc(meters, data));

            //电能表时钟示值误差
            sRst.vClockValueMeterConc.Add(GetvClockValueMeterConc(meters, data));



            #endregion 检定分项结论接口

            string url = GetJson("6.1");



            return "";
        }




        #region
        /// <summary>
        /// 基本误差数据
        /// </summary>
        /// <param name="meters"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<VBasicerrMeterConc> GetvBasicerrMeterConc(TestMeterInfo meters, CommonDatas_DETedTestData data)
        {
            List<VBasicerrMeterConc> list = new List<VBasicerrMeterConc>();
            string[] keys = new string[meters.MeterErrors.Keys.Count];
            meters.MeterErrors.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {
                VBasicerrMeterConc dataMeterConc = new VBasicerrMeterConc();
                string key = keys[i];
                if (!key.StartsWith(ProjectID.基本误差试验)) continue;

                MeterError meterErr = meters.MeterErrors[key];
                AutoCopy(dataMeterConc, data);
                dataMeterConc.sn = "";
                dataMeterConc.veriPointSn = "";
                dataMeterConc.validFlag = "";

                dataMeterConc.bothWayPowerFlag = meterErr.GLFX;
                dataMeterConc.curPhaseCode = meterErr.YJ;
                dataMeterConc.loadCurrent = meterErr.IbX;
                dataMeterConc.loadVoltage = meters.MD_UB.ToString();  //"100%Un"
                dataMeterConc.trialFreq = meters.MD_Frequency.ToString();
                dataMeterConc.pf = meterErr.GLYS;
                dataMeterConc.detectCircle = meterErr.Circle;
                string[] wc = meterErr.WCData.Split('|');
                dataMeterConc.simpling = wc.Length.ToString();
                dataMeterConc.actlError = meterErr.WCData;
                dataMeterConc.aveErr = meterErr.WCValue;
                dataMeterConc.intConvertErr = meterErr.WCHZ;

                if (meterErr.Limit != null)
                {
                    if (meterErr.Limit.IndexOf('|') > 0)
                    {
                        dataMeterConc.errUp = meterErr.Limit.Trim().Split('|')[0];
                        dataMeterConc.errDown = meterErr.Limit.Trim().Split('|')[1];

                    }
                    else if (meterErr.Limit.IndexOf('±') >= 0)
                    {
                        dataMeterConc.errUp = "+" + meterErr.Limit.Trim('±', ' ');
                        dataMeterConc.errDown = meterErr.Limit.Trim().Replace("±", "-");
                    }
                }

                dataMeterConc.checkConc = meterErr.Result == "合格" ? "01" : "02";
                list.Add(dataMeterConc);
            }
            return list;
        }
        /// <summary>
        /// 电能表时钟示值误差
        /// </summary>
        /// <param name="meters"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private VClockValueMeterConc GetvClockValueMeterConc(TestMeterInfo meters, CommonDatas_DETedTestData data)
        {
            VClockValueMeterConc dataMeterConc = new VClockValueMeterConc();
            AutoCopy(dataMeterConc, data);
            return dataMeterConc;
        }


        //public void 外观检查(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    //01  铭牌
        //    //02  液晶屏
        //    //03  指示灯
        //    int index = 1;
        //    if (Results.ContainsKey(ProjectID.外观检查))
        //    {
        //        foreach (var item in Results[ProjectID.外观检查].Values)
        //        {

        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            string DETECT_CONTENT = "02";
        //            switch (item["识别区域"])
        //            {
        //                case "液晶显示屏":
        //                    DETECT_CONTENT = "02";
        //                    break;
        //                case "铭牌":
        //                    DETECT_CONTENT = "01";
        //                    break;
        //                case "指示灯":
        //                    DETECT_CONTENT = "03";
        //                    break;
        //                default:
        //                    break;
        //            }
        //            VIntuitMeterConc meterConc = new VIntuitMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BarCode,
        //                barCode = meter.MD_BarCode,
        //                checkConc = GetResultCode(item["结论"]),
        //                chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.VerifyDate),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,
        //            };
        //            if (dETedTestData.vIntuitMeterConc == null) dETedTestData.vIntuitMeterConc = new List<VIntuitMeterConc>();
        //            dETedTestData.vIntuitMeterConc.Add(meterConc);

        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ProjectID.外观检查))
        //            {
        //                不合格列表.Add(ProjectID.外观检查, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ProjectID.外观检查),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 交流耐压试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.工频耐压试验;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VVoltMeterConc meterConc = new VVoltMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                leakCurrentLimit = item["耐压仪漏电流"],
        //                positionLeakLimit = item["标准值"],
        //                testDate = GetTime(meter.MD_TEST_DATE),
        //                testTime = item["耐压时间"],
        //                testVoltValue = item["耐压电压"],
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,
        //                voltObj = "01"

        //            };
        //            if (dETedTestData.vVoltMeterConc == null) dETedTestData.vVoltMeterConc = new List<VVoltMeterConc>();
        //            dETedTestData.vVoltMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 基本误差(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.基本误差试验;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VBasicerrMeterConc meterConc = new VBasicerrMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,


        //                actlError = item["误差1"] + "|" + item["误差2"],
        //                aveErr = item["平均值"],
        //                errDown = item["误差下限"],
        //                errUp = item["误差上限"],
        //                trialFreq = meter.MD_FREQUENCY.ToString(),
        //                simpling = "2",
        //                bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
        //                pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
        //                curPhaseCode = 功率元件转换(item["功率元件"]),
        //                detectCircle = item["误差圈数"],
        //                intConvertErr = item["化整值"],
        //                loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
        //                loadVoltage = GetCodeValue("meterTestVolt", "100%Un"),
        //            };
        //            if (dETedTestData.vBasicerrMeterConc == null) dETedTestData.vBasicerrMeterConc = new List<VBasicerrMeterConc>();
        //            dETedTestData.vBasicerrMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 电能表常数试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.电能表常数试验;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        var levs = GetMeterGrade(meter.MD_GRADE);
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VConstMeterConc meterConc = new VConstMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                actlError = item["误差"],
        //                bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
        //                constConcCode = "",
        //                constErr = item["误差"],
        //                controlMethod = GetCodeValue("meterTestCtrlMode", item["走字试验方法类型"]),
        //                diffReading = item["表码差"],
        //                divideElectricQuantity = item["走字电量(度)"],
        //                endReading = item["止码"],
        //                errDown = (Convert.ToDouble(levs[0]) * -1.0).ToString("0.0"),
        //                errUp = (Convert.ToDouble(levs[0]) * 1.0).ToString("0.0"),
        //                feeStartTime = "",
        //                irLastReading = "0",
        //                loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
        //                pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
        //                qualifiedPules = "",
        //                rate = item["费率"],
        //                readType = GetREAD_TYPE_CODE(item["功率方向"], item["费率"]),
        //                realPules = item["表脉冲"],
        //                rv = GetCodeValue("meterTestVolt", "100%Un"),
        //                standardReading = string.IsNullOrWhiteSpace(item["标准表脉冲"]) ? "0" : (Convert.ToSingle(item["标准表脉冲"]) / GetBcs(meter.MD_CONSTANT)[0]).ToString("F4"),
        //                startReading = item["起码"],
        //                valueConcCode = item["误差"],
        //            };
        //            if (!string.IsNullOrWhiteSpace(meterConc.valueConcCode) && meterConc.valueConcCode.Length > 8)
        //            {
        //                meterConc.valueConcCode = meterConc.valueConcCode.Substring(0, 8);
        //            }
        //            if (dETedTestData.vConstMeterConc == null) dETedTestData.vConstMeterConc = new List<VConstMeterConc>();
        //            dETedTestData.vConstMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 起动试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.起动试验;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VStartingMeterConc meterConc = new VStartingMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                current = "0",
        //                esamNo = "",
        //                testTime = "",
        //                theorTime = "",
        //            };
        //            if (float.TryParse(item["试验电流"], out float curtmp))
        //            {
        //                meterConc.current = curtmp.ToString("F4");
        //            }
        //            if (!string.IsNullOrWhiteSpace(item["标准试验时间"]))
        //            {
        //                float.TryParse(item["标准试验时间"], out float timetmp);
        //                meterConc.theorTime = (timetmp * 60).ToString("F0");
        //            }
        //            if (!string.IsNullOrWhiteSpace(item["实际运行时间"]))
        //            {
        //                float.TryParse(item["实际运行时间"].Replace("分", ""), out float timetmp);
        //                meterConc.testTime = (timetmp * 60).ToString("F0");
        //            }
        //            if (dETedTestData.vStartingMeterConc == null) dETedTestData.vStartingMeterConc = new List<VStartingMeterConc>();
        //            dETedTestData.vStartingMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 潜动试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.潜动试验;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VCreepingMeterConc meterConc = new VCreepingMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
        //                constConcCode = "",
        //                loadCurrent = GetCodeValue("meterTestCurLoad", "0"),
        //                loadVoltage = GetCodeValue("meterTestVolt", $"{item["潜动电压"].TrimEnd('%')}%Un"),
        //                pules = "0",
        //                realTestTime = "0",
        //                testCircleNum = "1",
        //                testTime = "0",
        //                volt = GetCodeValue("meterTestVolt", $"{item["潜动电压"].TrimEnd('%')}%Un"),
        //            };
        //            if (!string.IsNullOrWhiteSpace(item["标准试验时间"]))
        //            {
        //                float.TryParse(item["标准试验时间"], out float timetmp);
        //                meterConc.testTime = (timetmp * 60).ToString("F0");
        //            }
        //            if (!string.IsNullOrWhiteSpace(item["实际运行时间"]))
        //            {
        //                float.TryParse(item["实际运行时间"].Replace("分", ""), out float timetmp);
        //                meterConc.realTestTime = (timetmp * 60).ToString("F0");
        //            }
        //            if (dETedTestData.vCreepingMeterConc == null) dETedTestData.vCreepingMeterConc = new List<VCreepingMeterConc>();
        //            dETedTestData.vCreepingMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 计度器总电能示值组合误差(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.电能示值组合误差;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            int count = 0;
        //            if (!string.IsNullOrWhiteSpace(item["试验前费率电量"]))
        //            {
        //                count = item["试验前费率电量"].Split(',').Length;
        //            }
        //            VHutCombinaMeterConc meterConc = new VHutCombinaMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,


        //                bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
        //                loadCurrent = GetCodeValue("meterTestCurLoad", item["走字电流"]),
        //                pf = GetCodeValue("meterTestPowerFactor", "1.0"),
        //                feeRatio = "总",
        //                controlWay = GetCodeValue("meterTestCtrlMode", "标准表法"),
        //                irTime = count.ToString(),
        //                irReading = item["总电量差值"],
        //                errUp = (0.01 * (count - 1)).ToString("f2"),
        //                errDown = (-0.01 * (count - 1)).ToString("f2"),
        //                voltage = GetCodeValue("meterTestVolt", "100%Un"),
        //                totalReadingErr = item["组合误差"],
        //                totalIncrement = item["总电量差值"],
        //                valueConcCode = item["组合误差"],


        //                //eleIncrement = "",
        //                //endValue = "",
        //                //feeValue = "",
        //                //flatIncrement = "",
        //                otherIncrementOne = "",
        //                otherIncrementTwo = "",
        //                //peakIncrement = "",
        //                //sharpIncrement = "",
        //                //startValue = "",
        //                //sumerAllIncrement = "",
        //                //valleyIncrement = "",
        //            };

        //            bool isNullData = string.IsNullOrWhiteSpace(item["费率电量差值"]) || string.IsNullOrWhiteSpace(item["试验前费率电量"]) || string.IsNullOrWhiteSpace(item["试验后费率电量"])
        //           || string.IsNullOrWhiteSpace(item["试验后费率电量"]);

        //            费率电量结构 试验前电量 = new 费率电量结构();
        //            费率电量结构 试验后电量 = new 费率电量结构();
        //            费率电量结构 费率差值 = new 费率电量结构();
        //            if (!isNullData)
        //            {
        //                float[] tmp_费率差值 = item["费率电量差值"].Split(',').Select(x => float.Parse(x)).ToArray();
        //                float[] tmp_试验前电量 = item["试验前费率电量"].Split(',').Select(x => float.Parse(x)).ToArray();
        //                float[] tmp_试验后电量 = item["试验后费率电量"].Split(',').Select(x => float.Parse(x)).ToArray();
        //                string[] names = item["费率数段(英文逗号,间隔)"].Split(',');
        //                for (int i = 0; i < names.Length; i++)
        //                {
        //                    int Index = -1;
        //                    if (names[i].IndexOf("尖") != -1)
        //                        Index = 0;
        //                    else if (names[i].IndexOf("峰") != -1)
        //                        Index = 1;
        //                    else if (names[i].IndexOf("平") != -1)
        //                        Index = 2;
        //                    else if (names[i].IndexOf("谷") != -1 && names[i].IndexOf("深") == -1)
        //                        Index = 3;
        //                    else if (names[i].IndexOf("深谷") != -1)
        //                        Index = 4;
        //                    if (Index >= 0)
        //                    {
        //                        if (item["结论"] == "合格")
        //                        {
        //                            试验前电量.电量[Index] = tmp_试验前电量[Index];
        //                            试验后电量.电量[Index] = tmp_试验后电量[Index];
        //                            费率差值.电量[Index] = tmp_费率差值[Index];
        //                        }
        //                        else
        //                        {
        //                            试验前电量.电量[Index] = (tmp_试验前电量.Length - 1) >= Index ? tmp_试验前电量[Index] : 0;
        //                            试验后电量.电量[Index] = (tmp_试验后电量.Length - 1) >= Index ? tmp_试验后电量[Index] : 0;
        //                            费率差值.电量[Index] = (tmp_费率差值.Length - 1) >= Index ? tmp_费率差值[Index] : 0;
        //                        }
        //                    }
        //                }

        //                meterConc.sharpIncrement = 费率差值.电量[0].ToString("F4");
        //                meterConc.peakIncrement = 费率差值.电量[1].ToString("F4");
        //                meterConc.flatIncrement = 费率差值.电量[2].ToString("F4");
        //                meterConc.valleyIncrement = 费率差值.电量[3].ToString("F4");
        //                meterConc.sumerAllIncrement = 费率差值.总.ToString("F4");
        //            }
        //            if (count > 4)//有深谷的
        //            {
        //                meterConc.feeValue = "尖|峰|平|谷|深谷|总";
        //            }
        //            else
        //            {
        //                meterConc.feeValue = "尖|峰|平|谷|总";
        //            }
        //            meterConc.startValue = $"{string.Join("|", 试验前电量.电量)}|{item["试验前总电量"]}";
        //            meterConc.endValue = $"{string.Join("|", 试验后电量.电量)}|{item["试验后总电量"]}";
        //            meterConc.eleIncrement = $"{string.Join("|", 费率差值.电量)}|{item["总电量差值"]}";

        //            if (dETedTestData.vHutCombinaMeterConc == null) dETedTestData.vHutCombinaMeterConc = new List<VHutCombinaMeterConc>();

        //            dETedTestData.vHutCombinaMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 日计时误差(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.日计时误差;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VDayerrMeterConc meterConc = new VDayerrMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                actlError = $"{item["误差1"]}|{item["误差2"]}|{item["误差3"]}|{item["误差4"]}|{item["误差5"]}",
        //                errAbs = item["误差限(s/d)"]?.Replace("±", ""),
        //                intConvertErr = item["化整值"],
        //                secPiles = "1",
        //                simpling = "5",
        //                testAvgErr = item["平均值"],
        //                testTime = "60",
        //            };
        //            if (double.TryParse(item["平均值"], out double err))
        //            {
        //                meterConc.testAvgErr = err.ToString("F5");
        //            }
        //            if (dETedTestData.vDayerrMeterConc == null) dETedTestData.vDayerrMeterConc = new List<VDayerrMeterConc>();

        //            dETedTestData.vDayerrMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 规约一致性检查(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.通讯协议检查试验;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VStandardMeterConc meterConc = new VStandardMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                chkBasis = "",// item["标识编码"],
        //                dataFlag = item["标识编码"],
        //                readValue = item["检定信息"],
        //                settingValue = item["标准数据"],
        //            };
        //            if (item["检定信息"].Length > 250)
        //            {
        //                meterConc.readValue = item["检定信息"].Substring(0, 250);
        //            }
        //            if (dETedTestData.vStandardMeterConc == null) dETedTestData.vStandardMeterConc = new List<VStandardMeterConc>();
        //            dETedTestData.vStandardMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 误差变差试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.误差变差;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VErrorMeterConc meterConc = new VErrorMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                actlError = item["变差值"],
        //                avgOnceErr = item["第一次平均值"],
        //                avgTwiceErr = item["第二次平均值"],
        //                bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
        //                errDown = item["误差下限"],
        //                errUp = item["误差上限"],
        //                intOnceErr = item["第一次化整值"],
        //                intTwiceErr = item["第二次化整值"],
        //                loadCurrent = GetCodeValue("meterTestCurLoad", "1.0Ib"),
        //                onceErr = item["第一次误差1"] + "|" + item["第一次误差2"],
        //                pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
        //                pules = item["检定圈数"],
        //                simpling = "2",
        //                twiceErr = item["第二次误差1"] + "|" + item["第二次误差2"],
        //            };
        //            if (dETedTestData.vErrorMeterConc == null) dETedTestData.vErrorMeterConc = new List<VErrorMeterConc>();
        //            dETedTestData.vErrorMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 误差一致性试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.外观检查;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            string xib = item["电流倍数"];
        //            if (xib == "Ib") xib = "1.0Ib";
        //            else if (xib == "10Itr") xib = "10.0Itr";
        //            else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
        //            else if (xib == "1In" || xib == "1.0In") xib = "In";

        //            VConsistMeterConc meterConc = new VConsistMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                actlError = item["误差1"] + "|" + item["误差2"],
        //                allAvgError = item["平均值"],
        //                simpling = "2",
        //                loadCurrent = GetCodeValue("meterTestCurLoad", xib),
        //                errDown = item["误差下限"],
        //                errUp = item["误差上限"],
        //                pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
        //                pules = item["检定圈数"],
        //                testAvgErr = item["平均值"],
        //                intConvertErr = item["化整值"],
        //                bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
        //                allError = item["误差1"] + "|" + item["误差2"],
        //                allIntError = item["样品均值"],
        //                avgOnceErr = "",
        //                avgTwiceErr = "",
        //                onceErr = "",
        //                twiceErr = "",
        //                intOnceErr = "",
        //                intTwiceErr = "",

        //            };
        //            if (float.TryParse(meterConc.intConvertErr, out float HZ1))
        //            {
        //                if (Math.Abs(HZ1) < 0.00001)
        //                {
        //                    meterConc.intConvertErr = meterConc.intConvertErr.Replace("+", "").Replace("-", "");
        //                }
        //            }
        //            if (dETedTestData.vConsistMeterConc == null) dETedTestData.vConsistMeterConc = new List<VConsistMeterConc>();
        //            dETedTestData.vConsistMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 负载电流升降变差试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.负载电流升将变差;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;

        //            List<string> Names = new List<string>() { "01Ib", "Ib", "Imax" };
        //            for (int i = 0; i < Names.Count; i++)
        //            {
        //                string xib = item["电流点" + (i + 1).ToString()];
        //                if (xib == "Ib") xib = "1.0Ib";
        //                else if (xib == "10Itr") xib = "10.0Itr";
        //                else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
        //                else if (xib == "1In" || xib == "1.0In") xib = "In";

        //                VVariationMeterConc meterConc = new VVariationMeterConc()
        //                {
        //                    areaCode = RConfig.AreaCode,
        //                    assetNo = meter.MD_BAR_CODE,
        //                    barCode = meter.MD_BAR_CODE,
        //                    checkConc = GetResultCode(item["结论"]),
        //                    devCls = RConfig.DevCls,
        //                    devId = RConfig.DevId,
        //                    devSeatNo = item["表位号"],
        //                    machNo = item["检测系统编号"],
        //                    plantElementNo = item["检测系统编号"],
        //                    plantNo = item["检测系统编号"],
        //                    psOrgNo = RConfig.PsOrgNo,
        //                    sn = "1",
        //                    sysNo = RConfig.SysNo,
        //                    validFlag = "01",
        //                    veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                    veriPointSn = index.ToString(),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                    bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
        //                    loadCurrent = GetCodeValue("meterTestCurLoad", xib),
        //                    pf = GetCodeValue("meterTestPowerFactor", "1.0"),
        //                    curPhaseCode = 功率元件转换("H"),

        //                    detectCircle = item["Imax检定圈数"],
        //                    downErr1 = item[Names[i] + "下降误差1"],
        //                    downErr2 = item[Names[i] + "下降误差2"],
        //                    upErr1 = item[Names[i] + "上升误差1"],
        //                    upErr2 = item[Names[i] + "上升误差2"],
        //                    avgDownErr = item[Names[i] + "下降平均值"],
        //                    intDownErr = item[Names[i] + "下降化整值"],
        //                    variationErr = item[Names[i] + "差值"],
        //                    intVariationErr = item[Names[i] + "差值"],
        //                    avgUpErr = item[Names[i] + "上升平均值"],
        //                    intUpErr = item[Names[i] + "上升化整值"],
        //                    simpling = "2",
        //                    waitTime = Names[i] == "Imax" ? "120" : "0",
        //                };
        //                if (dETedTestData.vVariationMeterConc == null) dETedTestData.vVariationMeterConc = new List<VVariationMeterConc>();
        //                dETedTestData.vVariationMeterConc.Add(meterConc);
        //                index++;
        //            }
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 时段投切试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.时段投切;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VTsMeterConc meterConc = new VTsMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                tsErrConcCode = item["误差"],
        //                rv = GetCodeValue("meterTestVolt", "100%Un"),
        //                errAbs = "",
        //                rate = "",
        //                tsStartTime = "",
        //                tsRealTime = "",
        //                tsWay = "",
        //                errDowm = "",
        //                errUp = "",
        //                startTime = "",
        //            };
        //            if (dETedTestData.vTsMeterConc == null) dETedTestData.vTsMeterConc = new List<VTsMeterConc>();
        //            dETedTestData.vTsMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 测量重复性(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    //int index = 1;
        //    //string ItemNo = ProjectID.外观检查;
        //    //if (Results.ContainsKey(ItemNo))
        //    //{
        //    //    foreach (var item in Results[ItemNo].Values)
        //    //    {
        //    //        if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //    //        VMeasureRptMeterConc meterConc = new VMeasureRptMeterConc()
        //    //        {
        //    //            areaCode = RConfig.AreaCode,
        //    //            assetNo = meter.MD_BAR_CODE,
        //    //            barCode = meter.MD_BAR_CODE,
        //    //            checkConc = GetResultCode(item["结论"]),
        //    //            devCls = RConfig.DevCls,
        //    //            devId = RConfig.DevId,
        //    //            devSeatNo = item["表位号"],
        //    //            machNo = item["检测系统编号"],
        //    //            plantElementNo = item["检测系统编号"],
        //    //            plantNo = item["检测系统编号"],
        //    //            psOrgNo = RConfig.PsOrgNo,
        //    //            sn = "1",
        //    //            sysNo = RConfig.SysNo,
        //    //            validFlag = "01",
        //    //            veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //    //            veriPointSn = index.ToString(),
        //    //            veriTaskNo = meter.MD_TASK_NO,
        //    //            veriMeterRsltId = RConfig.VeriMeterRsltId,
        //    //        };
        //    //        dETedTestData.vMeasureRptMeterConc.Add(meterConc);
        //    //        index++;
        //    //        if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //    //        {
        //    //            不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //    //            {
        //    //                barCode = meter.MD_BAR_CODE,
        //    //                disqualReason = GetErrCode(ItemNo),
        //    //                veriTaskNo = meter.MD_TASK_NO,
        //    //                sysNo = RConfig.SysNo,
        //    //                veriUnitNo = item["检测系统编号"],
        //    //            });
        //    //        }
        //    //    }
        //    //}
        //}

        //public void 密钥下装(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.密钥更新;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VEsamMeterConc meterConc = new VEsamMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,


        //                rv = GetCodeValue("meterTestVolt", "100%Un"),
        //                keyType = GetCodeValue("secretKeyType", "身份认证密钥"),
        //                keyStatus = GetCodeValue("secretKeyStatus", "正式密钥"),
        //                keyNum = "",
        //                keyVer = ""
        //            };
        //            if (dETedTestData.vEsamMeterConc == null) dETedTestData.vEsamMeterConc = new List<VEsamMeterConc>();
        //            dETedTestData.vEsamMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 剩余电量递减度试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.剩余电量递减准确度;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VEqMeterConc meterConc = new VEqMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                totalEq = "",
        //                surplusEq = "",
        //                currElecPrice = "",
        //                loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
        //                pf = GetCodeValue("meterTestPowerFactor", item["功率因素"]),

        //            };
        //            if (dETedTestData.vEqMeterConc == null) dETedTestData.vEqMeterConc = new List<VEqMeterConc>();
        //            dETedTestData.vEqMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 通讯测试(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.通讯测试;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VComminicatMeterConc meterConc = new VComminicatMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,
        //            };
        //            if (dETedTestData.vComminicatMeterConc == null) dETedTestData.vComminicatMeterConc = new List<VComminicatMeterConc>();
        //            dETedTestData.vComminicatMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 安全认证试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.密钥更新;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VEsamSecMeterConc meterConc = new VEsamSecMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,
        //                esamNo = "",
        //            };
        //            if (dETedTestData.vEsamSecMeterConc == null) dETedTestData.vEsamSecMeterConc = new List<VEsamSecMeterConc>();
        //            dETedTestData.vEsamSecMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ProjectID.身份认证))
        //            {
        //                不合格列表.Add(ProjectID.身份认证, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ProjectID.身份认证),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 预置参数设置(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.预置内容设置;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VPreParamMeterConc meterConc = new VPreParamMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                dataName = item["数据项名称"],
        //                dataId = item["标识编码"],
        //                controlCode = "",
        //                dataBlockFlag = "",
        //                delayWaitTime = "",
        //                loginPwd = "",
        //                dataFormat = item["数据格式"],
        //                standardValue = item["标准数据"],
        //            };
        //            if (dETedTestData.vPreParamMeterConc == null) dETedTestData.vPreParamMeterConc = new List<VPreParamMeterConc>();
        //            dETedTestData.vPreParamMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 费率时段和功能(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.费率时段检查;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VFeeMeterConc meterConc = new VFeeMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                pf = GetCodeValue("meterTestPowerFactor", item["功率因素"]),
        //                loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
        //                bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
        //                curPhaseCode = 功率元件转换(item["功率元件"]),
        //            };
        //            if (dETedTestData.vFeeMeterConc == null) dETedTestData.vFeeMeterConc = new List<VFeeMeterConc>();
        //            dETedTestData.vFeeMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 费控试验(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    //int index = 1;
        //    //string ItemNo = ProjectID.外观检查;
        //    //if (Results.ContainsKey(ItemNo))
        //    //{
        //    //    foreach (var item in Results[ItemNo].Values)
        //    //    {
        //    //        if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //    //        //VIntuitMeterConc meterConc = new VIntuitMeterConc()
        //    //        //{
        //    //        //    areaCode = RConfig.AreaCode,
        //    //        //    assetNo = meter.MD_BAR_CODE,
        //    //        //    barCode = meter.MD_BAR_CODE,
        //    //        //    checkConc = GetResultCode(item["结论"]),
        //    //        //    chkCont = DETECT_CONTENT,
        //    //        //    devCls = RConfig.DevCls,
        //    //        //    devId = RConfig.DevId,
        //    //        //    devSeatNo = item["表位号"],
        //    //        //    machNo = item["检测系统编号"],
        //    //        //    plantElementNo = item["检测系统编号"],
        //    //        //    plantNo = item["检测系统编号"],
        //    //        //    psOrgNo = RConfig.PsOrgNo,
        //    //        //    sn = "1",
        //    //        //    sysNo = RConfig.SysNo,
        //    //        //    validFlag = "01",
        //    //        //    veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //    //        //    veriPointSn = index.ToString(),
        //    //        //    veriTaskNo = meter.MD_TASK_NO,
        //    //        //};
        //    //        //dETedTestData.vIntuitMeterConc.Add(meterConc);
        //    //        index++;
        //    //        if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //    //        {
        //    //            不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //    //            {
        //    //                barCode = meter.MD_BAR_CODE,
        //    //                disqualReason = GetErrCode(ItemNo),
        //    //                veriTaskNo = meter.MD_TASK_NO,
        //    //                sysNo = RConfig.SysNo,
        //    //                veriUnitNo = item["检测系统编号"],
        //    //            });
        //    //        }
        //    //    }
        //    //}
        //}

        //public void 预置参数检查(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.预置内容检查;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VPreCheckMeterConc meterConc = new VPreCheckMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                dataName = item["数据项名称"],
        //                dataId = item["标识编码"],
        //                controlCode = "",
        //                dataBlockFlag = "",
        //                realValue = item["检定信息"],
        //                deterUpperLimit = "",
        //                deterLowerLimit = "",
        //                dataFormat = item["数据格式"],
        //                standardValue = item["标准数据"],
        //            };
        //            if (dETedTestData.vPreCheckMeterConc == null) dETedTestData.vPreCheckMeterConc = new List<VPreCheckMeterConc>();
        //            dETedTestData.vPreCheckMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 需量示值误差(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.需量示值误差;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VDemandvalMeterConc meterConc = new VDemandvalMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                demandPeriod = item["需量周期"],
        //                demandTime = item["滑差时间"],
        //                demandInterval = item["滑差次数"],
        //                realDemand = item["实际需量"]?.Replace(" ", ""),
        //                realPeriod = item["需量周期"],
        //                demandValueErr = item["需量误差"],
        //                demandStandard = item["标准需量"]?.Replace(" ", ""),
        //                demandValueErrAbs = "",
        //                clearDataRst = "成功",
        //                valueConcCode = GetResultCode(item["结论"]),

        //                bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
        //                rv = GetCodeValue("meterTestVolt", "100%Un"),
        //                loadCurrent = GetCodeValue("meterTestCurLoad", item["电流"]),
        //                pf = GetCodeValue("meterTestPowerFactor", "1.0"),
        //                controlMethod = "01",
        //                errDowm = "",
        //                demandMeter = item["实际需量"]?.Replace(" ", ""),
        //                errUp = "",
        //                intConvertErr = "",
        //            };
        //            if (double.TryParse(item["误差下限"], out double err2))
        //            {
        //                meterConc.errDowm = Math.Round(err2, 4).ToString();
        //            }
        //            if (double.TryParse(item["误差上限"], out double err))
        //            {
        //                meterConc.errUp = Math.Round(err, 4).ToString();
        //            }
        //            if (dETedTestData.vDemandvalMeterConc == null) dETedTestData.vDemandvalMeterConc = new List<VDemandvalMeterConc>();
        //            dETedTestData.vDemandvalMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 时钟示值误差(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.时钟示值误差;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VClockValueMeterConc meterConc = new VClockValueMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                timeErr = item["时间差"],
        //                writeDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //                meterDate = "",
        //                meterValue = "",
        //                stdDate = "",
        //                stdValue = "",
        //            };
        //            if (DateTime.TryParse(item["GPS时间"], out DateTime stdDate))
        //            {
        //                meterConc.stdDate = stdDate.ToString("yyyy-MM-dd");
        //                meterConc.stdValue = stdDate.ToString("HH:mm:ss");
        //            }
        //            else
        //            {
        //                meterConc.stdDate = "";
        //                meterConc.stdValue = "";
        //            }
        //            if (DateTime.TryParse(item["被检表时间"], out DateTime meterDate))
        //            {
        //                meterConc.meterDate = meterDate.ToString("yyyy-MM-dd HH:mm:ss");
        //                meterConc.meterValue = meterDate.ToString("HH:mm:ss");
        //            }
        //            else
        //            {
        //                meterConc.meterDate = "";
        //                meterConc.meterValue = "";
        //            }
        //            if (dETedTestData.vClockValueMeterConc == null) dETedTestData.vClockValueMeterConc = new List<VClockValueMeterConc>();
        //            dETedTestData.vClockValueMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}

        //public void 电能表清零(DETedTestData dETedTestData, MeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        //{
        //    int index = 1;
        //    string ItemNo = ProjectID.电量清零;
        //    if (Results.ContainsKey(ItemNo))
        //    {
        //        foreach (var item in Results[ItemNo].Values)
        //        {
        //            if (string.IsNullOrWhiteSpace(item["结论"])) continue;
        //            VResetEqMeterConc meterConc = new VResetEqMeterConc()
        //            {
        //                areaCode = RConfig.AreaCode,
        //                assetNo = meter.MD_BAR_CODE,
        //                barCode = meter.MD_BAR_CODE,
        //                checkConc = GetResultCode(item["结论"]),
        //                //chkCont = DETECT_CONTENT,
        //                devCls = RConfig.DevCls,
        //                devId = RConfig.DevId,
        //                devSeatNo = item["表位号"],
        //                machNo = item["检测系统编号"],
        //                plantElementNo = item["检测系统编号"],
        //                plantNo = item["检测系统编号"],
        //                psOrgNo = RConfig.PsOrgNo,
        //                sn = "1",
        //                sysNo = RConfig.SysNo,
        //                validFlag = "01",
        //                veriCaliDate = GetTime(meter.MD_TEST_DATE),
        //                veriPointSn = index.ToString(),
        //                veriTaskNo = meter.MD_TASK_NO,
        //                veriMeterRsltId = RConfig.VeriMeterRsltId,

        //                eq = item["清零前电量"],
        //                resetEq = item["清零后电量"],
        //            };

        //            if (dETedTestData.vResetEqMeterConc == null) dETedTestData.vResetEqMeterConc = new List<VResetEqMeterConc>();
        //            dETedTestData.vResetEqMeterConc.Add(meterConc);
        //            index++;
        //            if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
        //            {
        //                不合格列表.Add(ItemNo, new VeriDisqualReasonList()
        //                {
        //                    barCode = meter.MD_BAR_CODE,
        //                    disqualReason = GetErrCode(ItemNo),
        //                    veriTaskNo = meter.MD_TASK_NO,
        //                    sysNo = RConfig.SysNo,
        //                    veriUnitNo = item["检测系统编号"],
        //                });
        //            }
        //        }
        //    }
        //}









        public void AutoCopy<T>(T child, CommonDatas_DETedTestData parent)
        {
            var ParentType = typeof(CommonDatas_DETedTestData);
            var Properties = ParentType.GetProperties();
            foreach (var Propertie in Properties)
            {
                if (Propertie.CanRead && Propertie.CanWrite)
                {
                    Propertie.SetValue(child, Propertie.GetValue(parent, null), null);
                }
            }

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
        /// 此函数可以添加字典，不可以修改或删除字典
        /// </summary>
        private void GetDicPCodeTable()
        {
            //获取MIS字典表信息
            //功率方向
            //Dictionary<string, string> powerFlag = GetPCodeDic("powerFlag");
            //PCodeTable.Add("powerFlag", powerFlag);
            ////电流相别
            //PCodeTable.Add("currentPhaseCode", GetPCodeDic("currentPhaseCode"));
            ////电流 用后面的 meter_Test_CurLoad
            //PCodeTable.Add("meterTestCurLoad", GetPCodeDic("meterTestCurLoad"));
            ////功率因数
            //PCodeTable.Add("itRatedLoadPf", GetPCodeDic("itRatedLoadPf"));
            ////功率因数
            //PCodeTable.Add("meterTestPowerFactor", GetPCodeDic("meterTestPowerFactor"));
            ////试验电压
            //PCodeTable.Add("meter_Test_Volt", GetPCodeDic("meter_Test_Volt"));
            ////试验电压
            //PCodeTable.Add("meterTestVolt", GetPCodeDic("meterTestVolt"));
            ////额定电压
            //PCodeTable.Add("meterVolt", GetPCodeDic("meterVolt"));
            ////额定电流
            //PCodeTable.Add("meterRcSort", GetPCodeDic("meterRcSort"));

            ////经互感器
            //PCodeTable.Add("conMode", GetPCodeDic("conMode"));

            ////费率
            //PCodeTable.Add("tari_ff", GetPCodeDic("tari_ff"));
            //PCodeTable.Add("fee", GetPCodeDic("fee"));
            ////等级
            //PCodeTable.Add("meterAccuracy", GetPCodeDic("meterAccuracy"));
            ////电表类型
            //PCodeTable.Add("meterTypeCode", GetPCodeDic("meterTypeCode"));
            ////电表常数
            //PCodeTable.Add("meterConstCode", GetPCodeDic("meterConstCode"));
            ////接线方式
            //PCodeTable.Add("wiringMode", GetPCodeDic("wiringMode"));
            ////电表型号
            //PCodeTable.Add("meterModelNo", GetPCodeDic("meterModelNo"));
            ////厂家
            //PCodeTable.Add("meterFacturer", GetPCodeDic("meterFacturer"));
            ////频率
            //PCodeTable.Add("meter_Test_Freq", GetPCodeDic("meter_Test_Freq"));
            ////密钥状态
            //PCodeTable.Add("secretKeyStatus", GetPCodeDic("secretKeyStatus"));
            ////密钥类型
            //PCodeTable.Add("secretKeyType", GetPCodeDic("secretKeyType"));
            ////走字方法
            //PCodeTable.Add("meterTestCtrlMode", GetPCodeDic("meterTestCtrlMode"));
            ////载波模块
            //PCodeTable.Add("LocalChipManu", GetPCodeDic("LocalChipManu"));

            ////电能表通讯规约
            //PCodeTable.Add("commProtocol", GetPCodeDic("commProtocol"));

            //PCodeTable.Add("meter_Test_CurLoad", GetPCodeDic("meter_Test_CurLoad"));//电流
            //PCodeTable.Add("meterTestFreq", GetPCodeDic("meterTestFreq"));
            //PCodeTable.Add("meterSort", GetPCodeDic("meterSort"));//类别
        }


        #endregion
        #region 方法


        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="Url"> 上传地址</param>
        /// <param name="jsonParas">上传文本内容</param>
        /// <param name="type">日志类型</param>
        /// <returns></returns>
        public string Post(string Url, string jsonParas, int type)
        {
            LogManager.AddMessage(string.Format("调用接口{0}，上报数据{1}\r\n", Url, jsonParas), type);
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
            JObject json = JObject.Parse(postContent);
            LogManager.AddMessage(string.Format("接口{0}，返回数据 \r\n{1}", Url, json.ToString()));
            return postContent;//返回Json数据
        }


  

        /// <summary>
        /// 获取Json 对象
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetJson(string str)
        {
            string url = "";
            switch (str)
            {
                case "6.1":
                    url = "/restful/sxyk/detectService/getVeriTask";
                    break;
                case "6.2":
                    url = "/restful/sxyk/detectService/getEquipDET";
                    break;
                case "6.4":
                    url = "/restful/sxyk/detectService/getVeriSchInfo";
                    break;
                case "6.9":
                    url = "/restful/common/getEquipParam";
                    break;
                case "6.12":
                    url = "/restful/sxyk/detectService/setResults";
                    break;
                case "6.13":
                    url = "/restful/sxyk/detectService/getDETedTestData";
                    break;
                case "6.14":
                    url = "/restful/sxyk/detectService/uploadSealsCode";
                    break;
                case "6.15":
                    url = "/restful/sxyk/detectService/getLegalCertRslt";
                    break;
                case "6.18":
                    url = "/restful/sxyk/detectService/sendTaskFinish";
                    break;
                case "7.16":
                    url = "/restful/common/getStdCodes";
                    break;
            }
            return "http://" + Ip + ":" + Port + url;
        }


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

        bool IMis.UpdateCompleted()
        {
            throw new NotImplementedException();
        }

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }


        #endregion


        private string GetResultCode(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return "03";
            }
            else
            {
                if (result == "合格")
                {
                    return "02";
                }
                else
                {
                    return "01";
                }
            }
        }
        private string GetTime(string result)
        {
            if (DateTime.TryParse(result, out DateTime time))
            {
                return time.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "";
        }
    }
}
