using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LYTest.Mis.HEB
{
    public static class DataConvertHelperCls
    {
        public static string ParseDateTime(string info)
        {
            string DateTime = string.Empty;
            try
            {
                XDocument doc = XDocument.Parse(info);
                string resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
                if (resultFlag == "1")
                {
                    DateTime = doc.Element("DATA").Element("SERVER").Element("DATE").Value;
                }
            }
            catch
            {
                return DateTime;
            }
            return DateTime;
        }
        public static string CreateUploadDeviceStateXml(ClDeviceState DeviceState)
        {
            string FirstEleItem = string.Format(@"<SYSTEM>
                                                <ID>{0}</ID>
                                                <STATE>{1}</STATE>
                                                </SYSTEM>", DeviceState.ID, DeviceState.State);
            XDocument doc;
            try
            {
                doc = XDocument.Parse(FirstEleItem);
                if (DeviceState.Device != null && DeviceState.Device.Count > 0)
                {
                    foreach (var item in DeviceState.Device)
                    {
                        var xmlItem = string.Format(@"<DEVICE>
                                                    <ID>{0}</ID>
                                                    <STATE>{1}</STATE>
                                                   </DEVICE>", item.DeviceId, item.DeviceState);
                        var element = XElement.Parse(xmlItem);
                        if (element != null)
                        {
                            doc.Element("SYSTEM").Add(element);
                        }

                    }
                }
            }
            catch
            {
                return null;
            }
            return doc.ToString();

        }
        public static string CreateUploadPictureXml(ClBuHeGePicture picture)
        {
            string xml = string.Format(@"   <PARA>
                                          <METER>
                                                <METER_NO>{0}</METER_NO>
                                                <STAION_ID>{1}</STAION_ID>
                                                <TASK_ID>{2}</TASK_ID>
                                                <FILE_NAME>{3}</FILE_NAME>
                                                <SUFFIX>{4}</SUFFIX>
                                                <FILE_CONTENT>{5}</FILE_CONTENT>
                                            </METER>
                                       </PARA>", picture.MeterNo, picture.StationId, picture.TaskId,
                                   picture.FileName, picture.Suffix, picture.FileContent);
            xml = XDocument.Parse(xml).ToString();

            return xml;

        }
        /// <summary>
        /// 从XML解析表信息到Dictionary
        /// </summary>
        /// <param name="Meterxml"></param>
        /// <param name="MeterInfos"></param>
        /// <param name="ErrInfo"></param>
        /// <returns></returns>
        //public static bool ParsMeterXmlInfo(string Meterxml, out Dictionary<string, List<ClMeterInfo_Local>> MeterInfos, out string ErrInfo)
        public static bool ParsMeterXmlInfo(string Meterxml, out Dictionary<string, List<TestMeterInfo>> MeterInfos, out string ErrInfo)
        {
            ErrInfo = string.Empty;
            bool Result = true;
            // MeterInfos = new Dictionary<string, List<ClMeterInfo_Local>>();
            MeterInfos = new Dictionary<string, List<TestMeterInfo>>();
            try
            {
                XDocument doc = XDocument.Parse(Meterxml);
                string resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
                if (resultFlag == "1")
                {
                    foreach (XElement element in doc.Element("DATA").Element("METER_INFO").Elements("CONTAINER"))
                    {
                        //List<ClMeterInfo_Local> meters = new List<ClMeterInfo_Local>();
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
                                    MD_ConnectionFlag = e.Element("ACCESS_TYPE").Value ?? string.Empty,
                                    MD_BarCode = e.Element("BAR_CODE").Value ?? string.Empty,
                                    MD_CarrName = e.Element("CARRIER_TYPE").Value ?? string.Empty,
                                    Result = e.Element("CHECK_RESULT").Value ?? string.Empty,
                                    MD_TestType = e.Element("CHECK_TYPE").Value ?? string.Empty,
                                    MD_UA = e.Element("CURRENT").Value,
                                    MD_Constant = e.Element("METER_CONST").Value ?? string.Empty,
                                    MD_Frequency = int.Parse(e.Element("FREQUENCY").Value),
                                    YaoJianYn = e.Element("IS_CHECK").Value == "1"
                                };
                                ;
                                meter.MD_WiringMode = e.Element("LINK_TYPE").Value ?? string.Empty;
                                meter.MD_PostalAddress = e.Element("METER_ADDRESS").Value ?? string.Empty;
                                meter.MD_Epitope = Convert.ToInt32(e.Element("METER_ID").Value);
                                meter.MD_MeterModel = e.Element("METER_MODE").Value;
                                meter.MD_Grane = e.Element("METER_CLASS").Value;
                                meter.MD_MeterType = e.Element("METER_TYPE").Value ?? string.Empty;
                                meter.MD_TaskNo = e.Element("TASK_NO").Value ?? string.Empty;
                                meter.MD_UB = float.Parse(e.Element("VOLTAGE").Value);
                                meter.Type = e.Element("EQUIP_CATEG").Value.GetType();
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

        public static bool ParsMeterXmlInfoManu(string Meterxml, out List<ClMeterInfo_Local> meters, out string ErrInfo)
        {
            ErrInfo = string.Empty;
            bool Result = true;
            meters = new List<ClMeterInfo_Local>();
            try
            {
                XDocument doc = XDocument.Parse(Meterxml);
                string resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
                if (resultFlag == "1")
                {
                    foreach (XElement element in doc.Element("DATA").Element("METER_INFO").Elements("CONTAINER"))
                    {
                        foreach (XElement e in element.Elements("METER"))
                        {
                            if (string.IsNullOrEmpty(e.Element("BAR_CODE").Value) || e.Element("BAR_CODE").Value == "") continue;
                            ClMeterInfo_Local meter = new ClMeterInfo_Local
                            {
                                AccessType = (EmLinkType)Convert.ToInt32(e.Element("ACCESS_TYPE").Value),
                                BarCode = e.Element("BAR_CODE").Value ?? string.Empty,
                                BatchId = e.Element("BATCH_ID").Value ?? string.Empty,
                                CarrierParam = e.Element("CARRIAER_PARAM").Value ?? string.Empty,
                                CarrierType = (EmCarrierType)Convert.ToInt32(e.Element("CARRIER_TYPE").Value),
                                CheckResult = (EmTrialConclusion)Convert.ToInt32(e.Element("CHECK_RESULT").Value),
                                CheckType = (EmTestMode)Convert.ToInt32(e.Element("CHECK_TYPE").Value),
                                Current = e.Element("CURRENT").Value,
                                MeterConst = e.Element("METER_CONST").Value,
                                MeterQConst = e.Element("METER_RP_CONST").Value,//增加无功常数
                                Frequency = e.Element("FREQUENCY").Value,
                                GuidId = e.Element("GUID_ID").Value,
                                Insulated = e.Element("INSULATED").Value == "1",
                                IsCheck = e.Element("IS_CHECK").Value == "1"
                            };
                            ;
                            meter.LinkType = (EmWireMode)Convert.ToInt32(e.Element("LINK_TYPE").Value);
                            string commPortCode = e.Element("COMM_PROT_CODE").Value;
                            if (string.IsNullOrEmpty(commPortCode))
                            {
                                meter.EmCommProtCode = EmCommProtCode.Procotol645;
                            }
                            else
                            {
                                meter.EmCommProtCode = (EmCommProtCode)Convert.ToInt32(commPortCode);
                            }
                            meter.MeterAddress = string.Empty;
                            meter.MeterId = Convert.ToInt32(e.Element("METER_ID").Value);
                            meter.ACPolar = EmPolar.Cathode;
                            meter.IsTomis = Convert.ToInt32(e.Element("ISTOMIS").Value);
                            meter.CommParam = e.Element("COMM_PARAS").Value;
                            meter.MeterModel = e.Element("METER_MODE").Value;
                            meter.MeterSort = e.Element("METER_SORT").Value;
                            meter.MeterClass = e.Element("METER_CLASS").Value;
                            meter.MeterQClass = e.Element("METER_RP_CLASS").Value;
                            meter.MeterType = (EmMeterType)Convert.ToInt32(e.Element("METER_TYPE").Value);
                            meter.ReCheckCount = Convert.ToInt32(e.Element("RECHECK_COUNT").Value);
                            meter.SaveDate = e.Element("SAVE_DATE").Value;
                            meter.SchemeId = e.Element("SCHEME_ID").Value;
                            meter.TaskId = e.Element("TASK_NO").Value;
                            meter.Voltage = e.Element("VOLTAGE").Value;
                            meter.EquipCatage = Convert.ToInt32(e.Element("EQUIP_CATEG").Value);
                            string closeMode = e.Element("METER_CLOSE_MODE").Value;
                            if (string.IsNullOrEmpty(closeMode))
                            {
                                meter.CloseMode = EmCloseMode.Null;
                            }
                            else
                            {
                                meter.CloseMode = (EmCloseMode)Convert.ToInt32(closeMode);//内置，外置
                            }
                            meters.Add(meter);
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
        /// <summary>
        /// 解析方案信息
        /// </summary>
        /// <param name="Schemexml">方案</param>
        /// <param name="schemes">方案信息结构体</param>
        /// <param name="ParameterCheck_Info">参数验证信息</param>
        /// <param name="ErrInfo">错误信息</param>
        /// <returns></returns>
        public static bool ParsSchemeXmlInfo(string Schemexml, out List<StTrialScheme_Local> schemes, out string ErrInfo)
        {

            schemes = new List<StTrialScheme_Local>();

            ErrInfo = string.Empty;
            ErrInfo = string.Empty;
            XDocument doc = null;
            bool Result = true;
            string resultFlag = string.Empty;
            try
            {
                doc = XDocument.Parse(Schemexml);
                resultFlag = doc.Element("DATA").Element("RESULT_FLAG").Value;
                if (resultFlag == "1")
                {


                    foreach (XElement e in doc.Element("DATA").Elements("SCHEME_INFO"))
                    {

                        StTrialScheme_Local scheme = new StTrialScheme_Local
                        {
                            ID = Convert.ToInt32(e.Element("ITEM_ID").Value),
                            SchemeId = e.Element("SCHEME_ID").Value,
                            SchemeName = e.Element("SCHEME_NAME").Value,
                            ItemName = e.Element("ITEM_NAME").Value,
                            ItemParams = e.Element("ITEM_PARAMS").Value,
                            SourceParams = e.Element("SOURCE_PARAMS").Value,
                            StationType = (EmLocalStationType)Convert.ToInt32(e.Element("STATION_TYPE").Value),
                            TrialOrder = Convert.ToInt32(e.Element("TRIAL_ORDER").Value),
                            TrialType = (EmTrialType)Convert.ToInt32(e.Element("TRIAL_TYPE").Value),
                            CommMode = Convert.ToInt32(string.IsNullOrEmpty(e.Element("COMM_MODE").Value) ? "0" : e.Element("COMM_MODE").Value),
                            IsUploadData = e.Element("IS_UPDATA") == null || string.IsNullOrEmpty(e.Element("IS_UPDATA").Value) || e.Element("IS_UPDATA").Value == "1"
                        };
                        if (e.Element("ITEM_CHECKPARAMS") != null)
                        {
                            List<StParameterCheck> ParameterCheck_Info = new List<StParameterCheck>();
                            StParameterCheck item = new StParameterCheck();
                            bool isExist = false;
                            foreach (XElement element in e.Element("ITEM_CHECKPARAMS").Elements("PARAM"))
                            {
                                item.DATA_ID = (element.Element("PARAM_ID").Value);
                                item.DATA_NAME = element.Element("PARAM_NAME").Value;
                                item.DATA_TYPE = element.Element("PARAM_TYPE").Value;
                                item.DATA_FORMAT = element.Element("PARAM_FORMAT").Value;
                                item.DATA_TAG = element.Element("PARAM_TAG").Value;
                                item.DATA_UNIT = element.Element("PARAM_UNIT").Value;
                                item.DATA_INTERCEPT = element.Element("PARAM_INTERCEPT_MODE").Value;
                                item.DATA_ISREAD = element.Element("PARAM_OPERTAION_TYPE").Value;
                                item.DATA_STANDARD = element.Element("PARAM_STANDARD_DATA").Value;
                                isExist = element.Element("PARAM_ORDER") != null;
                                if (isExist && !string.IsNullOrEmpty(element.Element("PARAM_ORDER").Value))
                                {
                                    item.PARAM_ORDER = Convert.ToInt32(element.Element("PARAM_ORDER").Value);
                                }
                                item.ITEM_ID = scheme.ID;
                                ParameterCheck_Info.Add(item);

                            }
                            scheme.ParamtrCheck = ParameterCheck_Info.OrderBy(x => isExist ? x.PARAM_ORDER : decimal.Parse(x.DATA_ID)).ToList();

                        }
                        schemes.Add(scheme);
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

        /// <summary>
        /// 解析执行结果返回信息
        /// </summary>
        /// <param name="xmldata"></param>
        /// <param name="errInfo"></param>
        /// <returns></returns>
        public static bool ParsFinishReturnXml(string xmldata, out List<StReslultDataXml> retData)
        {

            retData = new List<StReslultDataXml>();
            bool flag = false;
            if (string.IsNullOrEmpty(xmldata))
            {
                throw new ArgumentNullException();
            }

            try
            {
                XDocument doc = XDocument.Parse(xmldata);
                if (doc != null)
                {
                    if (doc.Element("DATA").Element("RESULT_FLAG").Value == "1")
                    {
                        flag = true;

                    }
                    else
                    {
                        flag = false;
                        foreach (var item in doc.Element("DATA").Elements("METER"))
                        {
                            StReslultDataXml Data = new StReslultDataXml
                            {
                                MeterNo = item.Element("METER_NO").Value,
                                ItemId = Convert.ToInt32(item.Element("ITEM_ID").Value)
                            };
                            retData.Add(Data);

                        }
                    }
                }
            }
            catch
            {
                retData = new List<StReslultDataXml>();
                throw;

            }

            return flag;
        }

        /// <summary>
        /// 解析执行结果返回信息
        /// </summary>
        /// <param name="xmldata"></param>
        /// <param name="errInfo"></param>
        /// <returns></returns>
        public static bool ParsReturnXml(string xmldata, out string err)
        {

            err = string.Empty;
            bool flag = false;
            if (string.IsNullOrEmpty(xmldata))
            {
                throw new ArgumentNullException();
            }

            try
            {
                XDocument doc = XDocument.Parse(xmldata);
                if (doc != null)
                {
                    if (doc.Element("DATA").Element("RESULT_FLAG").Value == "1")
                    {
                        flag = true;

                    }
                    else
                    {
                        flag = false;
                        err = doc.Element("DATA").Element("ERROR_INFO").Value;

                    }
                }
            }
            catch (Exception e)
            {
                err = e.Message;
                flag = false;

            }

            return flag;
        }
        /// <summary>
        /// 解析上传结果集后返回保存成功的数据
        /// </summary>
        /// <param name="RetXmlData"></param>
        /// <returns></returns>
        public static List<StReslultDataXml> ParseRetResultData(string RetXmlData)
        {
            List<StReslultDataXml> resultdata = new List<StReslultDataXml>();
            try
            {
                XDocument doc = XDocument.Parse(RetXmlData);
                foreach (XElement element in doc.Element("DATA").Elements("METER"))
                {
                    bool flag = element.Element("RESULT_FLAG").Value == "1";
                    StReslultDataXml st = new StReslultDataXml
                    {
                        IsUpload = flag
                    };
                    if (element.Elements("TRIAL_RESULT").FirstOrDefault() != null)
                    {
                        st.IsPass = element.Element("TRIAL_RESULT").Value == "1";
                    }
                    else
                    {
                        st.IsPass = true;
                    }
                    st.ItemId = Convert.ToInt32(element.Element("ITEM_ID").Value);
                    st.MeterNo = element.Element("METER_NO").Value;
                    resultdata.Add(st);
                }
            }
            catch
            {
                resultdata = new List<StReslultDataXml>();
            }
            return resultdata;

        }
        //          <PARA>
        //<TRIAL_TYPE>试验类型</TRIAL_TYPE>
        //<ITEM_MAME>项目名称</ITEM_NAME>
        //     <METER>
        //<METER_NO>表条码</METER_NO>
        //<METER_ID>表位号</METER_ID>
        //    </METER>

        /// <summary>
        /// 创建上传检定不合格表位信息XML
        /// </summary>
        /// <param name="FaildMeter"></param>
        /// <param name="TrialType"></param>
        /// <param name="TrialName"></param>
        /// <returns></returns>
        public static string CreateFailedMeterInfoXml(List<ClMeterInfo_Local> FaildMeter, int TrialType, string TrialName)
        {
            string FirstEleItem = string.Format(@"<PARA>
                                                <TRIAL_TYPE>{0}</TRIAL_TYPE>
                                                <ITEM_NAME>{1}</ITEM_NAME>
                                                </PARA>", TrialType, TrialName);
            XDocument doc;
            try
            {
                doc = XDocument.Parse(FirstEleItem);
                if (FaildMeter != null && FaildMeter.Count > 0)
                {
                    foreach (var item in FaildMeter)
                    {
                        var xmlItem = string.Format(@"<METER>
                                                    <METER_NO>{0}</METER_NO>
                                                    <METER_ID>{1}</METER_ID>
                                                   </METER>", item.BarCode, item.MeterId);
                        var element = XElement.Parse(xmlItem);
                        if (element != null)
                        {
                            doc.Element("PARA").Add(element);
                        }

                    }
                }
            }
            catch
            {
                return null;
            }
            return doc.ToString();

        }
        public static string CreatTempretureXmlInfo(Dictionary<int, List<object>> data)
        {
            string FirstEleItem = @"<PARA><PARA_TYPE>2</PARA_TYPE></PARA>";


            XDocument doc;
            try
            {
                doc = XDocument.Parse(FirstEleItem);
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        var xmlItem = string.Format(@"<METER>
                                                    <METER_ID>{0}</METER_ID>
                                                    <METER_TEMP>{1}</METER_TEMP>
                                                   </METER>", item.Key, item.Value[0].ToString());
                        var element = XElement.Parse(xmlItem);
                        if (element != null)
                        {
                            doc.Element("PARA").Add(element);
                        }

                    }
                }
            }
            catch
            {
                return null;
            }
            return doc.ToString();
        }
        /// <summary>
        /// 生成状态信息XML格式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string CreateStateInfoXml(StStationState state)
        {
            string xml = @" <PARA>
                             <CHECK_ITEM>{0}</CHECK_ITEM>
                             <CHECK_PROGRESS>{1}</CHECK_PROGRESS>
                            </PARA>";
            XDocument doc;
            try
            {
                xml = string.Format(xml, state.AnnexText, state.ProccessValue);
                //xml = string.Format(xml, state.AnnexText, state.ProccessValue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                doc = XDocument.Parse(xml);
            }
            catch
            {
                throw new Exception();
            }
            return doc.ToString();


        }
        /// <summary>
        /// 生成标准监视信息XML格式
        /// </summary>
        /// <param name="state"></param>
        /// <returns>XML</returns>
        public static string CreateMoniterXml(StMonitorData state)
        {
            string xml = @" <PARA>
                              <FREQUENCY>{0}</FREQUENCY>
                               <U>{1}</U>  
                              <I>{2}</I>
                              <I_PHASE>{3}</I_PHASE>
                              <U_PHASE>{4}</U_PHASE>
                              <PHASE_ANGLE>{5}</PHASE_ANGLE>
                              <POWER_ANGLE>{6}</POWER_ANGLE>
                              <POWER>{7}</POWER>
                              <Q_POWER>{8}</Q_POWER>
                              <S_POWER>{9}</S_POWER>
                              <P>{10}</P>
                              <Q>{11}</Q>
                              <S>{12}</S>
                              <PCOS>{13}</PCOS>
                              <QCOS>{14}</QCOS>
                              <COS>{15}</COS>
                        </PARA>";

            XDocument doc;
            try
            {   //三相

                if (state.UbValue != -999.0F && state.UcValue != -999.0F && state.IbValue != -999.0F && state.IcValue != -999.0F)
                {
                    xml = string.Format(xml,
                      state.Frequency,
                      state.UaValue + "," + state.UbValue + "," + state.UcValue,
                      state.IaValue + "," + state.IbValue + "," + state.IcValue,
                      state.IaPhase + "," + state.IbPhase + "," + state.IcPhase,
                      state.UaPhase + "," + state.UbPhase + "," + state.UcPhase,
                      state.Aa + "," + state.Ab + "," + state.Ac,
                      state.PowerAngle,
                      state.Pa + "," + state.Pb + "," + state.Pc,
                      state.Qa + "," + state.Qb + "," + state.Qc,
                      state.Sa + "," + state.Sb + "," + state.Sc,
                      state.P,
                      state.Q,
                      state.S,
                      state.Pcos,
                      state.Qsin,
                       state.ApCos + "," + state.BpCos + "," + state.CpCos
                            );
                }
                else
                {//单相表
                    xml = string.Format(xml,
                      state.Frequency,
                      state.UaValue,
                      state.IaValue,
                      state.IaPhase,
                      state.UaPhase,
                      state.Aa,
                      state.PowerAngle,
                      state.Pa,
                      state.Qa,
                      state.Sa,
                      state.P,
                      state.Q,
                      state.S,
                      state.Pcos,
                      state.Qsin,
                      state.ApCos
                            );

                }
                doc = XDocument.Parse(xml);
            }
            catch
            {
                throw new Exception();
            }
            return doc.ToString();


        }


        /// <summary>
        /// 生成状态信息XML格式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string CreateErrInfoXml(string code, string expand, int sys, string color)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException();
            }
            string xml = @"<PARA>
                                    <ERROR_MSG>{0}</ERROR_MSG>
                                    <ERROR_DATE>{1}</ERROR_DATE>
                                    <ERROR_SYS>{2}</ERROR_SYS>
                                    <ERROR_EXP>{3}</ERROR_EXP>
                                    <LED_COLOR>{4}</LED_COLOR>
                               </PARA>
                            ";
            XDocument doc;
            try
            {
                xml = string.Format(xml, code, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sys, expand, color);
                doc = XDocument.Parse(xml);
            }
            catch
            {
                throw new Exception();
            }
            return doc.ToString();


        }


        public static string CreateReturnXml(bool flag, string err)
        {
            if (string.IsNullOrEmpty(err))
            {
                err = "";

            }
            string xml = @" <DATA>
                    <RESULT_FLAG>{0}</RESULT_FLAG>
                    <ERROR_INFO>{1}</ERROR_INFO>
                    </DATA> ";
            XDocument doc;
            try
            {
                xml = string.Format(xml, flag == true ? "1" : "0", err);
                doc = XDocument.Parse(xml);
            }
            catch
            {
                throw new Exception();
            }
            return doc.ToString();


        }

        public static string CreateCheckResultsDataXml(List<ClTrialData_Local> lstTrialData)
        {
            string xml = @"<PARA>                          
                           </PARA> ";
            string xml_sub = @"<METER>
                                <GUID_ID>{0}</GUID_ID>
                                <METER_ID>{1}</METER_ID>
                                <METER_NO>{2}</METER_NO>
                                <STAION_ID>{3}</STAION_ID>
                                <TRIAL_TYPE>{4}</TRIAL_TYPE >
                                <ITEM_ID>{5}</ITEM_ID>
                                <PARAM_ID>{6}</PARAM_ID>
                                <PRISTINE_VALUES>{7}</PRISTINE_VALUES>
                                <TRIAL_VALUE>{8}</TRIAL_VALUE> 
                                <ROUND_VALUE>{9}</ROUND_VALUE>
                                <EXTENT_DATA>{10}</EXTENT_DATA>
                                <RECHECK_INDEX>{11}</RECHECK_INDEX>
                                <SCHEME_ID>{12}</SCHEME_ID>
                                <TASK_ID>{13}</TASK_ID>
                                <TRIAL_ENDDATE>{14}</TRIAL_ENDDATE>
                                <TRIAL_STARTDATE>{15}</TRIAL_STARTDATE>
                                <TRIAL_RESULT>{16}</TRIAL_RESULT>                              
                            </METER>";

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
                xml_sub = XDocument.Parse(xml_sub).ToString();
                if (lstTrialData != null && lstTrialData.Count > 0)
                {
                    foreach (ClTrialData_Local item in lstTrialData)
                    {
                        string xml_doc = string.Format(xml_sub,
                                                     item.GuidId,
                                                     item.MeterId,
                                                     item.MeterNo,
                                                     item.StationId,
                                                     (int)item.TrialType,
                                                     item.ItemId,
                                                     item.ParamID,//参数验证子项Id
                                                     item.PristineValues,
                                                     item.TrialValue,
                                                     item.RoundValue,
                                                     item.ExtentData,
                                                     item.ReCheckCount,
                                                     item.SchemeId,
                                                     item.Taskid,
                                                     item.SaveDate,
                                                     item.TrialStartDate,
                                                     (int)item.TrialResult
                                                    );


                        XElement element = XElement.Parse(xml_doc);
                        if (element != null)
                        {
                            doc.Element("PARA").Add(element);
                        }

                    }
                }

            }
            catch
            {
                return string.Empty;
            }

            return doc.ToString();
        }
        public static string CreateBindCarveInfo(string meterNO, string bindCarveInfo)
        {
            string xml = @" <PARA>
                                <CONTAINER>
                                  <CODE>{0}</CODE>
                                  <OBJECT>
                                     <POSTION>1</POSTION>
                                  <CODE>{1}</CODE>
                                  </OBJECT>
                               </CONTAINER>
                           </PARA>";
            XDocument doc;
            try
            {
                xml = string.Format(xml, meterNO, bindCarveInfo);
                doc = XDocument.Parse(xml);
            }
            catch
            {
                throw new Exception();
            }
            return doc.ToString();
        }

        public static bool ParseXmlReturnDictionary(string meterInfos, out Dictionary<int, string> MeterNos, out string err)
        {
            MeterNos = new Dictionary<int, string>();
            err = string.Empty;
            bool flag = false;
            if (string.IsNullOrEmpty(meterInfos))
            {
                throw new ArgumentNullException();
            }

            try
            {
                XDocument doc = XDocument.Parse(meterInfos);
                if (doc != null)
                {
                    if (doc.Element("DATA").Element("RESULT_FLAG").Value == "1")
                    {
                        foreach (XElement element in doc.Element("DATA").Element("METER_INFO").Elements("CONTAINER"))
                        {
                            foreach (XElement e in element.Elements("METER"))
                            {
                                //这里只获取待检的表信息
                                //-1为待检，0为合格，1为不合格
                                if (e.Element("CHECK_RESULT").Value.Equals("-1"))
                                {
                                    int METER_ID = int.Parse(e.Element("METER_ID").Value);
                                    string BAR_CODE = e.Element("BAR_CODE").Value;
                                    MeterNos.Add(METER_ID, BAR_CODE);
                                    flag = true;
                                }


                            }
                        }

                    }
                    else
                    {
                        err = doc.Element("DATA").Element("ERROR_INFO").Value;
                        flag = false;
                    }
                }
            }
            catch (Exception e)
            {
                err = e.Message;
                flag = false;

            }

            return flag;


        }
    }
}
