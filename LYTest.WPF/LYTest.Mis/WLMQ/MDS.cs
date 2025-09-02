using ICInterface.Base_ICStructure;
using ICInterface.ICApiStructure;
using ICInterface.Meter_ICStructure;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.Mis.Common;
using LYTest.Utility.Log;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LYTest.Mis.WLMQ
{
    public class MDS : IMis
    {
        /// <summary>
        /// 工控接口服务类
        /// </summary>
        GK_BaseApi GK_Base;
        /// <summary>
        /// WebApi接口地址
        /// </summary>
        public string WebApiUrl { get; set; }
        /// <summary>
        /// 标准代码
        /// </summary>
        StdDicCode stdDicCode;
        /// <summary>
        /// 系统编号
        /// </summary>
        public string SysNo;

        MDS_MeterResultHelper MDSResult;

        public MDS(string ip, int port, string sysno)
        {
            SysNo = sysno;
            WebApiUrl = $"http://{ip}:{port}";
            var apiConfig = new GK_ApiConfig()
            {
                检定设备申请接口 = WebApiUrl + "/restful/sxyk/detectService/applyEquip",
                空箱设备申请接口 = WebApiUrl + "/restful/sxyk/detectService/applyAssistEquip",
                设备参数信息获取接口 = WebApiUrl + "/restful/common/getEquipParam",
                箱核对信息接口 = WebApiUrl + "/restful/sxyk/detectService/boxCheckInfo",
                检定设备核对信息上传接口 = WebApiUrl + "/restful/sxyk/detectService/equipCheckInfo",
                芯片id合法性认证接口 = WebApiUrl + "/restful/sxyk/detectService/getLegalCertRslt",
                检定分项结论接口 = WebApiUrl + "/restful/sxyk/detectService/getDETedTestData",
                检定综合结论接口 = WebApiUrl + "/restful/sxyk/detectService/setResults",
                施封信息接口 = WebApiUrl + "/restful/sxyk/detectService/uploadSealsCode",
                设备组箱信息接口 = WebApiUrl + "/restful/sxyk/detectService/uploadPackInfo",
                周转箱组垛信息接口 = WebApiUrl + "/restful/sxyk/detectService/upboxPileDet",
                检定任务完成接口 = WebApiUrl + "/restful/sxyk/detectService/sendTaskFinish",
                标准代码获取接口 = WebApiUrl + "/restful/common/getStdCodes",
                检定台任务信息获取接口 = WebApiUrl + "/restful/sxyk/detectService/getVeriTask",
                检定方案信息获取接口 = WebApiUrl + "/restful/sxyk/detectService/getVeriSchInfo",
            };
            MDSResult = new MDS_MeterResultHelper(new MDS_MeterResultHelper.ResultConfig()
            {
                AreaCode = "6510",
                DevCls = "01",
                DevId = "",
                PsOrgNo = "6510",
                SysNo = SysNo,
                VeriMeterRsltId = "",

            });
            GK_Base = new GK_BaseApi(apiConfig);
            IniStd();
        }
        void IniStd()
        {
            List<string> CodeNames = new List<string>();

            CodeNames.Add("rateFreq");
            CodeNames.Add("meterConst");
            CodeNames.Add("accuLv");
            CodeNames.Add("wireMode");
            CodeNames.Add("meterModel");
            CodeNames.Add("meterType");
            CodeNames.Add("commProt");
            CodeNames.Add("meterCateg");
            CodeNames.Add("meterRv");
            CodeNames.Add("meterFacturer");
            CodeNames.Add("rc");
            CodeNames.Add("accsMode");


            //结论部分
            CodeNames.Add("powerFlag"); //功率方向
            CodeNames.Add("meterTestPowerFactor");  //功率因数
            CodeNames.Add("curPhase");  //功率元件
            CodeNames.Add("meterTestCurLoad");  //电流倍数
            CodeNames.Add("meterTestVolt"); //电压倍数
            CodeNames.Add("meterTestCtrlMode");   //走字试验方法类型

            CodeNames.Add("GetCodeValue");   //身份认证密钥
            CodeNames.Add("secretKeyStatus");   //正式密钥
            stdDicCode = new StdDicCode(SysNo, GK_Base, CodeNames);

        }


        /// <summary>
        /// flag  是否调用码值接口获取码值  江西调试无法获取码值数据
        /// </summary>
        bool flag = false;

        string trialSchId = "";

        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            string Msg;
            try
            {
                    if (flag)
                    {
                        if (!stdDicCode.InitCode(out Msg))
                        {
                            LogManager.AddMessage($"下载表【{barcode}】信息失败:" + Msg, EnumLogSource.服务器日志, EnumLevel.Error);
                            return false;
                        }
                        if (string.IsNullOrWhiteSpace(barcode)) return false;
                    }
                    else
                    {
                        stdDicCode.初始化码值表("");
                    }
                   
                var TaskInfo = GK_Base.检定台任务信息获取接口(new getVeriTaskCell()
                {
                    barCode = barcode,
                    sysNo = SysNo,

                }, out Msg);
                trialSchId = TaskInfo.veriTask.trialSchId;
                if (TaskInfo == null)
                {
                    LogManager.AddMessage($"下载表【{barcode}】信息失败:检定台任务信息获取接口", EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }

                if (TaskInfo.resultFlag != "1")
                {
                    LogManager.AddMessage($"下载表【{barcode}】信息失败2:" + TaskInfo.errorInfo, EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }

                //TaskInfo.taskNo

                getEquipParamCell getEquipParamCell = new getEquipParamCell()
                {
                    arrBatchNo = TaskInfo.veriTask.arrBatchNo,
                    barCodes = barcode,
                    devCls = TaskInfo.veriTask.devCls,
                    devCodeNo = "",
                    type = "03",
                };
                var meterInfos = GK_Base.设备参数信息获取接口<getEquipParamResult_Meter>(getEquipParamCell, out Msg);
                if (meterInfos == null)
                {
                    LogManager.AddMessage($"下载表【{barcode}】信息失败1,设备参数信息获取接口没有返回值", EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }
                if (meterInfos.resultFlag != "1")
                {
                    LogManager.AddMessage($"下载表【{barcode}】信息失败2" + meterInfos.errorInfo, EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }
                if (meterInfos.meterDet == null || meterInfos.meterDet.Count < 1)
                {
                    LogManager.AddMessage($"下载表【{barcode}】信息失败3,没有获取到【{barcode}】表的资产信息", EnumLogSource.服务器日志, EnumLevel.Error);
                    return false;
                }


                MeterDet meterDet = meterInfos.meterDet[0];

                meter.MD_BarCode = meterDet.barCode;//表条码
                meter.MD_PostalAddress = meterDet.meterAddr;
                meter.Other4 = TaskInfo.veriTask.devCls;
                //if (meter.MD_BAR_CODE.Length > 21)
                //{
                //    meter.MD_POSTAL_ADDRESS = meter.MD_BAR_CODE.Substring(meter.MD_BAR_CODE.Length - 13, 12); //表地址
                //}

                meter.MD_TaskNo = TaskInfo.veriTask.taskNo;
                //meter.nam = TaskInfo.EXECUTOR_NAME;//执行人名称-核验员
                meter.MD_MadeNo = meterDet.ftyNo;//出厂编号
                meter.MD_AssetNo = meterDet.assetNo;//资产编号
                                                    //meter.MD_TEST_PERSON = TaskInfo.EXECUTOR_NO;
                if (int.TryParse(stdDicCode.GetCodeName("rateFreq", meterDet.metFreq), out int fl))
                {
                    meter.MD_Frequency = fl;
                }
                else
                {
                    meter.MD_Frequency = 50;
                }
                //频率
                meter.MD_Constant = stdDicCode.GetCodeName("meterConst", meterDet.metApConst);//有功常数
                if (!string.IsNullOrEmpty(meterDet.metRpConst))
                {
                    meter.MD_Constant += $"({stdDicCode.GetCodeName("meterConst", meterDet.metRpConst)})";
                }
                meter.MD_Grane = stdDicCode.GetCodeName("accuLv", meterDet.metApAccuLv);//有功准确度等级
                if (!string.IsNullOrEmpty(meterDet.metRpAccuLv))      //无功准确度等级
                {
                    meter.MD_Grane += $"({stdDicCode.GetCodeName("accuLv", meterDet.metRpAccuLv)})";
                }
                meter.MD_WiringMode = stdDicCode.GetCodeName("wireMode", meterDet.metWireMode); //接线方式
                meter.MD_MeterModel = stdDicCode.GetCodeName("meterModel", meterDet.metModel); //电能表型号 //DTZ395-Z
                meter.MD_MeterType = stdDicCode.GetCodeName("meterType", meterDet.metType); //电能表类型
                meter.FKType = meter.MD_MeterType.IndexOf("本地") != -1 ? 1 : 0;
                meter.MD_ProtocolName = stdDicCode.GetCodeName("commProt", meterDet.metCommProt).IndexOf("698") >= 0 || stdDicCode.GetCodeName("commProt", meterDet.metCommProt).IndexOf("对象") >= 0 ? "CDLT698" : "CDLT6452007";
                meter.MD_JJGC = meter.MD_Grane.IndexOfAny(new char[] { 'A', 'B', 'C', 'D' }) >= 0 ? "IR46" : "JJG596-2012";
                //if (!string.IsNullOrWhiteSpace(TaskInfo.VERIFICATION_REGULACTION)) meter.MD_JJGC = TaskInfo.VERIFICATION_REGULACTION;
                meter.MD_Sort = stdDicCode.GetCodeName("meterCateg", meterDet.metCateg).IndexOf("物联") == -1 ? "智能表" : "物联电能表";//电能表类别
                string ubstring = stdDicCode.GetCodeName("meterRv", meterDet.metRv); // //电能表电压 //380
                if (ubstring.IndexOf("57.7") >= 0)
                    meter.MD_UB = 57.7f;
                else if (ubstring.IndexOf("100") >= 0)
                    meter.MD_UB = 100f;
                else if (ubstring.IndexOf("220") >= 0)
                    meter.MD_UB = 220f;

                meter.MD_Factory = stdDicCode.GetCodeName("meterFacturer", meterDet.metMfr); //厂家;

                meter.MD_UA = stdDicCode.GetCodeName("rc", meterDet.metRc).TrimEnd('A').Replace('（', '(').Replace('）', ')'); //; //电流    //0.2-0.5(60)

                meter.BatchNo = TaskInfo.veriTask.arrBatchNo;//批次号
                //meter.MD_METER_VERSION = meterDet.metCommHardVer;
                meter.MD_ConnectionFlag = stdDicCode.GetCodeName("accsMode", meterDet.metAccsMode).IndexOf("互感") == -1 ? "直接式" : "互感式";


                meter.MD_CarrName = "标准载波";
                meter.YaoJianYn = true;
                meter.MD_TestModel = "首检";
                return true;
            }
            catch (Exception ex)
            {
                LogManager.AddMessage($"下载表【{barcode}】信息异常:" + ex.Message + "\r\n" + ex, EnumLogSource.服务器日志, EnumLevel.Error);
                return false;
            }
        }

        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {

            Dictionary<string, SchemaNode> Schema1 = new Dictionary<string, SchemaNode>();
            JObject joSend = new JObject();
            joSend.Add("trialSchId", "8000000020005303");
            string jsonSend = joSend.ToString();
            var getVeriSchInfo = GK_Base.检定方案信息获取接口(jsonSend, out string Msg);
            schemeName = Msg;
            Schema = Schema1;
            return true;
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
            string Msg;
            try
            {
                if (flag)
                {
                    if (!stdDicCode.InitCode(out  Msg))
                    {
                        LogManager.AddMessage($"上传数据失败:" + Msg, EnumLogSource.服务器日志, EnumLevel.Error);
                        return false;
                    }

                }
                else
                {
                    stdDicCode.初始化码值表("");
                }
                MDSResult.InitStddic(stdDicCode, false);
                DateTime time = DateTime.Now;

                var RConfig = new MDS_MeterResultHelper.ResultConfig();
                string s = RConfig.DevCls;
                //总结论
                MeterVeriResult result = new MeterVeriResult()
                {
                    devCls = meter.Other4,
                    sysNo = SysNo,
                    veriTaskNo = meter.MD_TaskNo,
                    veriDtlFormList = new List<VeriDtlFormList>(),
                };
                //分项结论
                DETedTestData itemResult = new DETedTestData()
                {
                    devCls = meter.Other4,
                    sysNo = SysNo,
                    veriTaskNo = meter.MD_TaskNo,
                };
                var Result = ResultConverter(meter);

                string DETECT_PERSON = meter.Checker1;
                string AUDIT_PERSON = meter.Checker2;
                Dictionary<string, VeriDisqualReasonList> 不合格列表 = new Dictionary<string, VeriDisqualReasonList>();
                MDSResult.基本误差(itemResult, meter, Result, 不合格列表);
                MDSResult.电能表常数试验(itemResult, meter, Result, 不合格列表);
                MDSResult.起动试验(itemResult, meter, Result, 不合格列表);
                MDSResult.潜动试验(itemResult, meter, Result, 不合格列表);
                MDSResult.计度器总电能示值组合误差(itemResult, meter, Result, 不合格列表);
                MDSResult.日计时误差(itemResult, meter, Result, 不合格列表);
                MDSResult.规约一致性检查(itemResult, meter, Result, 不合格列表);
                MDSResult.误差变差试验(itemResult, meter, Result, 不合格列表);
                MDSResult.误差一致性试验(itemResult, meter, Result, 不合格列表);
                MDSResult.负载电流升降变差试验(itemResult, meter, Result, 不合格列表);
                MDSResult.时段投切试验(itemResult, meter, Result, 不合格列表);
                MDSResult.测量重复性(itemResult, meter, Result, 不合格列表);
                MDSResult.密钥下装(itemResult, meter, Result, 不合格列表);
                MDSResult.剩余电量递减度试验(itemResult, meter, Result, 不合格列表);
                MDSResult.通讯测试(itemResult, meter, Result, 不合格列表);
                MDSResult.安全认证试验(itemResult, meter, Result, 不合格列表);
                MDSResult.预置参数设置(itemResult, meter, Result, 不合格列表);
                MDSResult.费率时段和功能(itemResult, meter, Result, 不合格列表);
                MDSResult.费控试验(itemResult, meter, Result, 不合格列表);
                MDSResult.预置参数检查(itemResult, meter, Result, 不合格列表);
                MDSResult.需量示值误差(itemResult, meter, Result, 不合格列表);
                MDSResult.时钟示值误差(itemResult, meter, Result, 不合格列表);
                MDSResult.电能表清零(itemResult, meter, Result, 不合格列表);
                MDSResult.GPS对时(itemResult, meter, Result, 不合格列表);
             
                MDSResult.检定综合结论(result, meter, Result, 不合格列表, DETECT_PERSON, AUDIT_PERSON);

                //上传
                if (GK_Base.检定分项结论接口(itemResult, out Msg, time, 600000))
                {
                    return GK_Base.检定综合结论接口(result, out Msg, time, 600000);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 结论转换器
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> ResultConverter(TestMeterInfo meter)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            foreach (var item in meter.MeterTestDatas)
            {
                string ParaNo = item.Key.Split('_')[0];
                if (!Results.ContainsKey(ParaNo))
                {
                    Results.Add(ParaNo, new Dictionary<string, Dictionary<string, string>>());
                }
                if (!Results[ParaNo].ContainsKey(item.Value.ItemNo))
                {
                    Results[ParaNo].Add(item.Value.ItemNo, new Dictionary<string, string>());
                }
                var dic = Results[ParaNo][item.Value.ItemNo];
                AddMeterResultDic(dic, "项目编号", item.Value.ItemNo);
                AddMeterResultDic(dic, "项目名称", item.Value.Name);
                AddMeterResultDic(dic, "结论", item.Value.Result);
                AddMeterResultDic(dic, "检测时间", meter.VerifyDate);
                AddMeterResultDic(dic, "检测系统编号", SysNo);
                AddMeterResultDic(dic, "表位号", meter.MD_Epitope.ToString());

                if (!string.IsNullOrWhiteSpace(item.Value.ParameterNames))
                {
                    var ParameterNames = item.Value.ParameterNames.Split('|');
                    var ParameterValues = item.Value.ParameterValues.Split('|');
                    for (int i = 0; i < ParameterNames.Length; i++)
                    {
                        if (ParameterValues.Length > i)
                        {
                            AddMeterResultDic(dic, ParameterNames[i], ParameterValues[i]);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(item.Value.ResultNames))
                {
                    var ResultNames = item.Value.ResultNames.Split('|');
                    var ResultValues = item.Value.ResultValues.Split('^');
                    for (int i = 0; i < ResultNames.Length; i++)
                    {
                        if (ResultNames.Length > i)
                        {
                            AddMeterResultDic(dic, ResultNames[i], ResultValues[i]);
                        }
                    }
                }
            }
            return Results;
        }
        private void AddMeterResultDic(Dictionary<string, string> dic, string Name, string Value)
        {
            if (!dic.ContainsKey(Name))
            {
                dic.Add(Name, Value);
            }

        }


        public bool UpdateCompleted()
        {
            return true;
        }

        public void UpdateInit()
        {
            throw new NotImplementedException();
        }

        public bool SendTaskFinish(string taskno)
        {
            try
            {
                sendTaskFinish res = new sendTaskFinish()
                {
                    taskNo = taskno,
                    flag = "01"
                };
                return GK_Base.检定任务完成接口(res, out string Msg);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }


    public class MeterDet1 : MeterDet
    {
        public string trialSchId { get; set; }
    }
}

