using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.DAL;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace LYTest.ViewModel.MeterResoultModel
{
    public class MeterDataHelper
    {
        /// <summary>
        /// 方案参数
        /// </summary>
        public static Dictionary<string, DynamicModel> Models;
        public static Dictionary<string, List<string>> ModelsName;

        public static TestMeterInfo[] GetDnbInfoNew()
        {
            TestMeterInfo[] meterTemp = GetMeterInfo();
            GetModels();

            GetResoult(meterTemp);
            for (int i = 0; i < EquipmentData.CheckResults.ResultCollection.Count; i++)
            {
                CheckNodeViewModel result = EquipmentData.CheckResults.ResultCollection[i];   //结论
                DynamicViewModel TestParamValueModel = EquipmentData.Schema.ParaValues[i];
                string TestParamsValueString = "";
                if (TestParamValueModel != null)
                {
                    TestParamsValueString = TestParamValueModel.GetProperty("PARA_VALUE") as string;  //检定的参数
                }
                List<string> PlanParamsName = null;
                if (ModelsName != null || ModelsName.ContainsKey(result.ParaNo))
                    PlanParamsName = ModelsName[result.ParaNo];//检定点的格式
                Dictionary<string, string> TestParamValue = new Dictionary<string, string>();
                if (PlanParamsName != null && TestParamsValueString != null)
                {
                    string[] value = TestParamsValueString.Split('|');//检定用到的参数
                    for (int j = 0; j < PlanParamsName.Count; j++)
                    {
                        if (!TestParamValue.ContainsKey(PlanParamsName[j]))
                        {
                            TestParamValue.Add(PlanParamsName[j], value[j]);
                        }
                    }
                }

                for (int pos = 0; pos < meterTemp.Length; pos++)
                {
                    if (meterTemp[pos] == null || string.IsNullOrWhiteSpace(meterTemp[pos].MD_BarCode)) continue;

                    MeterTestData testData = new MeterTestData();
                    testData.index = int.Parse(TestParamValueModel.GetProperty("PARA_INDEX") as string);
                    testData.ItemNo = result.ItemKey;
                    testData.Name = result.Name;
                    testData.ParameterNames = string.Join("|", PlanParamsName);
                    testData.ParameterValues = TestParamsValueString;
                    testData.ResultNames = result.DisplayModel.ColumnModelList.Find(v => v.Field == "MD_VALUE").DisplayName;
                    List<string> datas = new List<string>();
                    foreach (string item in result.DisplayModel.GetDisplayNames())
                    {
                        if (item == "结论") continue;
                        datas.Add(result.CheckResults[pos].GetProperty(item) as string);
                    }
                    testData.ResultValues = string.Join("^", datas);
                    testData.Result = result.CheckResults[pos].GetProperty("结论") as string;

                    if (meterTemp[pos].MeterTestDatas.ContainsKey(result.ItemKey))
                        meterTemp[pos].MeterTestDatas.Remove(result.ItemKey);

                    meterTemp[pos].MeterTestDatas.Add(result.ItemKey, testData);
                }

                switch (result.ParaNo)
                {

                    case ProjectID.基本误差试验://基本误差
                        GetMeterError(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.起动试验:
                        GetMeterQdQid(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.潜动试验:
                        GetMeterQdQid(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.电能表常数试验:
                        GetMeterZZError(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.日计时误差:
                        GetClockError(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.身份认证:
                    case ProjectID.密钥更新:
                        GetMeterFK(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.GPS对时:
                    case ProjectID.需量示值误差:
                        GetDgn(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.误差一致性:
                    case ProjectID.误差变差:
                    case ProjectID.负载电流升将变差:
                        GetMeterErrAccord(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.通讯协议检查试验:
                        GetMeterDLTData(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.通讯协议检查试验2:
                        GetMeterDLTData2(meterTemp, result, TestParamValue);
                        break;
                    case ProjectID.远程控制:
                    case ProjectID.报警功能:
                    case ProjectID.远程保电:
                    case ProjectID.保电解除:
                        GetMeterFK(meterTemp, result, TestParamValue);
                        break;
                    default:
                        break;
                }

            }
            return meterTemp;
        }


        /// <summary>
        /// 获得检定参数模型
        /// </summary>
        private static void GetModels()
        {
            if (Models != null && ModelsName != null) return;
            Models = new Dictionary<string, DynamicModel>();
            ModelsName = new Dictionary<string, List<string>>();
            List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString());
            for (int i = 0; i < models.Count; i++)
            {
                string name = models[i].GetProperty("PARA_NO").ToString();

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                if (!ModelsName.ContainsKey(name))
                {
                    string str = models[i].GetProperty("PARA_VIEW") as string;
                    List<string> value = null;
                    if (str != null)
                    {
                        value = str.Split('|').ToList();
                    }
                    ModelsName.Add(name, value);
                };
                if (!Models.ContainsKey(name))
                {
                    Models.Add(name, models[i]);
                };
            }


        }


        /// <summary>
        /// 获取表的基本数据(参数录入的数据)
        /// </summary>
        /// <returns></returns>
        private static TestMeterInfo[] GetMeterInfo()
        {
            TestMeterInfo[] testMeters = new TestMeterInfo[EquipmentData.MeterGroupInfo.Meters.Count];
            for (int i = 0; i < testMeters.Length; i++)
            {
                DynamicViewModel model = EquipmentData.MeterGroupInfo.Meters[i];
                if (string.IsNullOrWhiteSpace(model.GetProperty("MD_BAR_CODE") as string))
                {
                    continue;
                }

                //if(model.GetProperty("MD_UPDOWN") as string != "1")
                //{
                //    continue;
                //}

                TestMeterInfo meterTemp = new TestMeterInfo();

                string str = "";
                #region 基本信息
                //meterTemp.Meter_ID = meter.Meter_ID;
                meterTemp.BenthNo = model.GetProperty("MD_DEVICE_ID").ToString();  //台体编号     MD_DEVICE_ID
                str = model.GetProperty("MD_FKTYPE").ToString();
                if (str == "远程费控")
                    meterTemp.FKType = 0;
                else if (str == "本地费控")
                    meterTemp.FKType = 1;
                else
                    meterTemp.FKType = 2;
                meterTemp.MD_AssetNo = model.GetProperty("MD_ASSET_NO").ToString();
                meterTemp.MD_BarCode = model.GetProperty("MD_BAR_CODE").ToString();
                meterTemp.MD_CarrName = model.GetProperty("MD_CARR_NAME").ToString();
                meterTemp.MD_CertificateNo = model.GetProperty("MD_CERTIFICATE_NO").ToString();
                meterTemp.MD_ConnectionFlag = model.GetProperty("MD_CONNECTION_FLAG").ToString();
                meterTemp.MD_Constant = model.GetProperty("MD_CONSTANT").ToString();
                meterTemp.MD_Customer = model.GetProperty("MD_CUSTOMER").ToString();
                meterTemp.MD_Epitope = int.Parse(model.GetProperty("MD_EPITOPE").ToString());
                meterTemp.MD_Factory = model.GetProperty("MD_FACTORY").ToString();
                meterTemp.MD_Frequency = int.Parse(model.GetProperty("MD_FREQUENCY").ToString());
                meterTemp.MD_Grane = model.GetProperty("MD_GRADE").ToString();
                meterTemp.MD_JJGC = model.GetProperty("MD_JJGC").ToString();
                meterTemp.MD_MadeNo = model.GetProperty("MD_MADE_NO").ToString();
                meterTemp.MD_MeterModel = model.GetProperty("MD_METER_MODEL").ToString();
                meterTemp.MD_MeterType = model.GetProperty("MD_METER_TYPE").ToString();
                meterTemp.MD_PostalAddress = model.GetProperty("MD_POSTAL_ADDRESS").ToString();
                meterTemp.MD_ProtocolName = model.GetProperty("MD_PROTOCOL_NAME").ToString();
                meterTemp.MD_TaskNo = model.GetProperty("MD_TASK_NO").ToString();
                meterTemp.MD_TestModel = model.GetProperty("MD_TESTMODEL").ToString();
                meterTemp.MD_TestType = model.GetProperty("MD_TEST_TYPE").ToString();
                meterTemp.MD_UA = model.GetProperty("MD_UA").ToString();
                meterTemp.MD_UB = float.Parse(model.GetProperty("MD_UB").ToString());
                meterTemp.MD_WiringMode = model.GetProperty("MD_WIRING_MODE").ToString();
                meterTemp.Other1 = model.GetProperty("MD_OTHER_1").ToString();
                meterTemp.Other2 = model.GetProperty("MD_OTHER_2").ToString();
                meterTemp.Other3 = model.GetProperty("MD_OTHER_3").ToString();
                meterTemp.Other4 = model.GetProperty("MD_OTHER_4").ToString();
                meterTemp.Other5 = model.GetProperty("MD_OTHER_5").ToString();
                meterTemp.Seal1 = model.GetProperty("MD_SEAL_1").ToString();
                meterTemp.Seal2 = model.GetProperty("MD_SEAL_2").ToString();
                meterTemp.Seal3 = model.GetProperty("MD_SEAL_3").ToString();
                meterTemp.Seal4 = model.GetProperty("MD_SEAL_4").ToString();
                meterTemp.Seal5 = model.GetProperty("MD_SEAL_5").ToString();
                meterTemp.VerifyDate = model.GetProperty("MD_TEST_DATE").ToString();
                meterTemp.YaoJianYn = model.GetProperty("MD_CHECKED").ToString() == "1" ? true : false;
                meterTemp.Result = model.GetProperty("MD_RESULT").ToString();  //表结论
                meterTemp.Humidity = model.GetProperty("MD_TEMPERATURE").ToString();  //湿度
                meterTemp.Temperature = model.GetProperty("MD_HUMIDITY").ToString();  //温度
                meterTemp.Checker1 = model.GetProperty("MD_TEST_PERSON").ToString();  //检验员
                meterTemp.Checker2 = model.GetProperty("MD_AUDIT_PERSON").ToString();  //核验员
                meterTemp.ExpiryDate = model.GetProperty("MD_VALID_DATE") as string;
                meterTemp.MD_Sort = model.GetProperty("MD_SORT") as string;
                meterTemp.MD_UPDOWN = model.GetProperty("MD_UPDOWN") as string;
                #endregion
                testMeters[meterTemp.MD_Epitope - 1] = meterTemp;
            }

            return testMeters;
        }

        /// <summary>
        /// 获得总结论
        /// </summary>
        private static void GetResoult(TestMeterInfo[] meter)
        {
            //初始化所有的结论都是合格，如果检定过程中有不合格就修改为不合格
            List<string> ID = new List<string>();
            try
            {
                FieldInfo[] f_key = typeof(ProjectID).GetFields();
                for (int i = 0; i < f_key.Length; i++)
                {
                    ID.Add(f_key[i].GetValue(new ProjectID()).ToString());
                }

            }
            catch
            {
            }
            for (int j = 0; j < ID.Count; j++)
            {
                string strVaule = ID[j];//获取值
                for (int i = 0; i < meter.Length; i++) //循环所有的表，获得这个表的检定结论
                {
                    if (meter[i] == null || !meter[i].YaoJianYn)
                    {
                        continue;
                    }
                    if (meter[i].MeterResults.ContainsKey(strVaule))
                        meter[i].MeterResults.Remove(strVaule);
                    MeterResult meterResult = new MeterResult();
                    meterResult.Result = ConstHelper.合格;
                    meter[i].MeterResults.Add(strVaule, meterResult);
                }
            }
        }

        #region 检定项目结论
        /// <summary>
        /// 获得基本误差结论
        /// </summary>
        /// <param name="meter">表类</param>
        /// <param name="CheckResult">检定结论</param>
        /// <param name="TestValue">检定参数值</param>
        private static void GetMeterError(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名

            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterError meterError = new MeterError();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                meterError.GLYS = ResultValue["功率因数"];
                meterError.GLFX = ResultValue["功率方向"];
                meterError.PrjNo = CheckResult.ItemKey;
                meterError.YJ = ResultValue["功率元件"];
                meterError.Result = ResultValue["结论"];
                //string[] results = result.Split('|');
                //meterError.Result = results[0];
                //meterError.AVR_DIS_REASON = results[1];
                meterError.IbX = ResultValue["电流倍数"];
                if (meterError.IbX == "Ib")
                {
                    meterError.IbX = "1.0Ib";
                }
                meterError.WCData = ResultValue["误差1"] + "|" + ResultValue["误差2"];
                meterError.WCHZ = ResultValue["化整值"];
                meterError.WCValue = ResultValue["平均值"];
                meterError.Limit = ResultValue["误差上限"] + "|" + ResultValue["误差下限"];
                meterError.BPHUpLimit = ResultValue["误差上限"];
                meterError.BPHDownLimit = ResultValue["误差下限"];
                meterError.Circle = ResultValue["误差圈数"];

                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                if (meter[i].MeterErrors.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterErrors.Remove(CheckResult.ItemKey);
                meter[i].MeterErrors.Add(CheckResult.ItemKey, meterError);

                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }
        /// <summary>
        ///走字试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterZZError(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterZZError meterError = new MeterZZError();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                string[] zParam;
                if (ResultValue.ContainsKey("项目名"))
                    zParam = ResultValue["项目名"].Split('_');
                else
                    zParam = CheckResult.Name.Split('_');

                meterError.Result = ResultValue["结论"];
                if (TestValue.ContainsKey("费率"))
                    meterError.Fl = TestValue["费率"];
                else
                {
                    if (ResultValue["项目名"].IndexOf("尖") > 0) meterError.Fl = "尖";
                    else if (ResultValue["项目名"].IndexOf("峰") > 0) meterError.Fl = "峰";
                    else if (ResultValue["项目名"].IndexOf("平") > 0) meterError.Fl = "平";
                    else if (ResultValue["项目名"].IndexOf("深谷") > 0) meterError.Fl = "深谷";
                    else if (ResultValue["项目名"].IndexOf("谷") > 0) meterError.Fl = "谷";
                    else meterError.Fl = "总";
                }
                if (TestValue.ContainsKey("功率因数"))
                    meterError.GLYS = TestValue["功率因数"];
                else
                {
                    if (zParam.Length >= 6)
                        meterError.GLYS = zParam[3];
                    else meterError.GLYS = "1.0";
                }
                if (TestValue.ContainsKey("电流倍数"))
                {
                    meterError.IbX = TestValue["电流倍数"];
                    //add yjt 20220310 新增电流倍数
                    meterError.IbXString = TestValue["电流倍数"];
                }
                else
                {
                    if (zParam.Length >= 6)
                    {
                        meterError.IbX = zParam[4];
                        meterError.IbXString = zParam[4];
                    }
                    else
                    {
                        meterError.IbX = "Imax";
                        meterError.IbXString = "Imax";
                    }
                }
                if (meterError.IbX == "Ib")
                {
                    meterError.IbX = "1.0Ib";
                    //add yjt 20220310 新增电流倍数
                    meterError.IbXString = "1.0Ib";
                }
                if (TestValue.ContainsKey("功率方向"))
                    meterError.PowerWay = TestValue["功率方向"];
                else
                {
                    if (zParam.Length >= 6)
                        meterError.PowerWay = zParam[1];
                }
                if (TestValue.ContainsKey("走字试验方法类型"))
                    meterError.TestWay = TestValue["走字试验方法类型"];
                else
                {
                    meterError.TestWay = "标准表法";
                }
                if (TestValue.ContainsKey("功率元件"))
                    meterError.YJ = TestValue["功率元件"];
                else
                {
                    if (zParam.Length >= 6)
                        meterError.YJ = zParam[2];
                }
                meterError.ErrorValue = ResultValue["误差"];
                if (TestValue.ContainsKey("走字电量(度)"))
                    meterError.NeedEnergy = TestValue["走字电量(度)"];
                else
                {

                }
                //meterError.Fl = TestValue["费率"];
                //meterError.GLYS = TestValue["功率因数"];
                //meterError.IbX = TestValue["电流倍数"];
                //if (meterError.IbX == "Ib")
                //{
                //    meterError.IbX = "1.0Ib";
                //}
                //meterError.PowerWay = TestValue["功率方向"];
                //meterError.TestWay = TestValue["走字试验方法类型"];
                //meterError.YJ = TestValue["功率元件"];
                //meterError.NeedEnergy = TestValue["走字电量(度)"];
                meterError.PowerSumStart = ResultValue["起码"];
                meterError.PowerSumEnd = ResultValue["止码"];
                meterError.WarkPower = "0";
                if (meterError.PowerSumEnd != "" && meterError.PowerSumStart != "")
                {
                    meterError.WarkPower = (float.Parse(meterError.PowerSumEnd) - float.Parse(meterError.PowerSumStart)).ToString("f5");
                }
                meterError.PowerError = ResultValue["表码差"];
                meterError.STMEnergy = ResultValue["标准表脉冲"];  // ResultValue["标准表脉冲"]/d_meterInfo.GetBcs()[0])
                meterError.STMEnergy = ResultValue["标准表脉冲"];  // ResultValue["标准表脉冲"]/d_meterInfo.GetBcs()[0])
                meterError.Pules = ResultValue["表脉冲"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                if (meter[i].MeterZZErrors.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterZZErrors.Remove(CheckResult.ItemKey);
                meter[i].MeterZZErrors.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///启动潜动试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterQdQid(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterQdQid meterError = new MeterQdQid();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                meterError.ActiveTime = ResultValue["实际运行时间"].Trim('分');
                meterError.PowerWay = ResultValue["功率方向"];
                meterError.Voltage = ResultValue["试验电压"];
                meterError.TimeEnd = ResultValue["结束时间"];
                meterError.TimeStart = ResultValue["开始时间"];
                meterError.TimeStart = ResultValue["开始时间"];
                meterError.StandartTime = ResultValue["标准试验时间"];
                meterError.Current = ResultValue["试验电流"];
                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                if (meter[i].MeterQdQids.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterQdQids.Remove(CheckResult.ItemKey);
                meter[i].MeterQdQids.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///日计时试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetClockError(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterDgn meterError = new MeterDgn();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                List<string> list = new List<string>();
                foreach (string item in ResultValue.Keys)
                {
                    if (item != "结论")
                    {
                        list.Add(item);
                    }
                }
                foreach (string item in TestValue.Keys)
                {
                    list.Add(item);
                }
                meterError.Value = string.Join("|", list);
                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                if (meter[i].MeterDgns.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDgns.Remove(CheckResult.ItemKey);
                meter[i].MeterDgns.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///费控试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterFK(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterFK meterError = new MeterFK();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                List<string> list = new List<string>();
                foreach (string item in ResultValue.Keys)
                {
                    if (item != "结论")
                    {
                        list.Add(item);
                    }
                }
                switch (CheckResult.ItemKey)
                {
                    case ProjectID.远程控制:
                        meterError.Name = "远程控制";
                        meterError.Data = string.Join("|", list);
                        break;
                    case ProjectID.报警功能:
                        meterError.Name = "报警功能";
                        meterError.Data = string.Join("|", list);
                        break;
                    case ProjectID.远程保电:
                    case "保电解除":
                    case "密钥更新":
                        meterError.Name = ResultValue["当前项目"];
                        meterError.Data = ResultValue["检定信息"];
                        break;
                    default:  //费控通用
                        meterError.Name = ResultValue["当前项目"];
                        meterError.Data = ResultValue["检定信息"];
                        break;
                }

                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                if (meter[i].MeterCostControls.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterCostControls.Remove(CheckResult.ItemKey);
                meter[i].MeterCostControls.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///多功能试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetDgn(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {


            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterDgn meterError = new MeterDgn();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                List<string> list = new List<string>();
                foreach (string item in ResultValue.Keys)
                {
                    if (item != "结论")
                    {
                        list.Add(item);
                    }
                }
                meterError.Value = string.Join("|", list);
                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                if (meter[i].MeterDgns.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDgns.Remove(CheckResult.ItemKey);
                meter[i].MeterDgns.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }


        /// <summary>
        /// 误差一致性
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        /// <param name="Parameter_colName"></param>
        /// <param name="Parameter_Value"></param>
        private static void GetMeterErrAccord(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            string index = "";
            switch (CheckResult.ItemKey)
            {
                case ProjectID.误差一致性:
                    index = "1";
                    break;
                case ProjectID.误差变差:
                    index = "2";
                    break;
                case ProjectID.负载电流升将变差:
                    index = "3";
                    break;
                default:
                    break;
            }
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterErrAccord meterError = new MeterErrAccord();
                if (meter[i].MeterErrAccords.ContainsKey(index))   //先判断是不是有了，有了在原来基础上添加
                    meterError = meter[i].MeterErrAccords[index];
                else
                    meter[i].MeterErrAccords.Add(index, meterError);


                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                MeterErrAccordBase meterErr = new MeterErrAccordBase();

                meterErr.Freq = meter[i].MD_Frequency.ToString();
                meterError.Result = ResultValue["结论"];
                if (index == "1")    //补上圈数和误差限
                {
                    meterErr.Name = "误差一致性";
                    //误差1|误差2|平均值|化整值|样品均值|差值
                    meterErr.IbX = TestValue["电流"];  //电流
                    if (TestValue["电流"] == "Ib")
                    {
                        meterErr.IbX = "1.0Ib";
                    }
                    meterErr.PF = TestValue["功率因数"];   //功率因数
                    meterErr.PulseCount = ResultValue["检定圈数"];
                    meterErr.Limit = ResultValue["误差上限"] + "|" + ResultValue["误差下限"];
                    meterErr.Data1 = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["平均值"] + "|" + ResultValue["化整值"];
                    meterErr.ErrAver = ResultValue["样品均值"];
                    meterErr.Error = ResultValue["差值"];

                    int count = 1;
                    if (meter[i].MeterErrAccords.ContainsKey(index))
                        count = meter[i].MeterErrAccords[index].PointList.Keys.Count + 1;
                    meterError.PointList.Add(count.ToString(), meterErr);
                }
                else if (index == "2")    //1.0IB，
                {
                    meterErr.Name = "误差变差";
                    meterErr.PulseCount = ResultValue["检定圈数"];
                    meterErr.Limit = ResultValue["误差上限"] + "|" + ResultValue["误差下限"];
                    meterErr.IbX = "1.0Ib";  //电流
                    meterErr.PF = TestValue["功率因数"];  //功率因数
                                                      //第一次误差1|第一次误差2|第一次平均值|第一次化整值|第二次误差1|第二次误差2|第二次平均值|第二次化整值|变差值
                    meterErr.Data1 = ResultValue["第一次误差1"] + "|" + ResultValue["第一次误差2"] + "|" + ResultValue["第一次平均值"] + "|" + ResultValue["第一次化整值"];
                    meterErr.Data2 = ResultValue["第二次误差1"] + "|" + ResultValue["第二次误差2"] + "|" + ResultValue["第二次平均值"] + "|" + ResultValue["第二次化整值"];
                    meterErr.Error = ResultValue["变差值"];
                    int count = 1;
                    if (meter[i].MeterErrAccords.ContainsKey(index))
                        count = meter[i].MeterErrAccords[index].PointList.Keys.Count + 1;
                    meterError.PointList.Add(count.ToString(), meterErr);
                }
                else if (index == "3")   //这个需要加个判断，判断子项是否合格
                {

                    string[] str = new string[] { "01Ib", "Ib", "Imax" };
                    for (int j = 0; j < 3; j++)
                    {
                        meterErr = new MeterErrAccordBase();
                        meterErr.Freq = meter[i].MD_Frequency.ToString();
                        meterErr.Name = "负载电流升降变差";
                        meterErr.PulseCount = ResultValue[str[j] + "检定圈数"];
                        meterErr.IbX = str[j];  //电流
                        if (meterErr.IbX == "Ib")
                        {
                            meterErr.IbX = "1.0Ib";
                        }
                        if (str[j] == "01Ib") meterErr.IbX = "0.1Ib";
                        meterErr.PF = "1.0";   //功率因数
                        meterErr.Data1 = ResultValue[str[j] + "上升误差1"] + "|" + ResultValue[str[j] + "上升误差2"] + "|" + ResultValue[str[j] + "上升平均值"] + "|" + ResultValue[str[j] + "上升化整值"];
                        meterErr.Data2 = ResultValue[str[j] + "下降误差1"] + "|" + ResultValue[str[j] + "下降误差2"] + "|" + ResultValue[str[j] + "下降平均值"] + "|" + ResultValue[str[j] + "下降化整值"];
                        meterErr.Error = ResultValue[str[j] + "差值"];
                        meterErr.Result = ConstHelper.不合格;
                        if (!string.IsNullOrEmpty(meterErr.Error))
                        {
                            float t = float.Parse(meterErr.Error);
                            if (Math.Abs(t) <= 0.25)
                                meterErr.Result = ConstHelper.合格;
                        }
                        meterError.PointList.Add((j + 1).ToString(), meterErr);
                    }
                }
                meterError.Result = ConstHelper.合格;
                for (int j = 1; j <= meterError.PointList.Count; j++)
                {
                    if (meterError.PointList[j.ToString()].Result == ConstHelper.不合格)     //有一个点不合格，总结论不合格
                    {
                        meterError.Result = ConstHelper.不合格;
                    }
                }
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }

        }


        /// <summary>
        ///规约一致性（通讯协议检查）
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetMeterDLTData(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterDLTData meterDLTData = new MeterDLTData();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                meterDLTData.DataFlag = TestValue["标识编码"]; //645
                meterDLTData.DataFormat = TestValue["数据格式"];
                meterDLTData.DataLen = TestValue["长度"];
                meterDLTData.FlagMsg = TestValue["数据项名称"];
                meterDLTData.StandardValue = TestValue["写入内容"];
                meterDLTData.Value = ResultValue["检定信息"];
                meterDLTData.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterDLTData.Result = "不合格";
                }
                if (meter[i].MeterDLTDatas.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDLTDatas.Remove(CheckResult.ItemKey);
                meter[i].MeterDLTDatas.Add(CheckResult.ItemKey, meterDLTData);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterDLTData.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///规约一致性（通讯协议检查）
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>           
        private static void GetMeterDLTData2(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i] == null || !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterDLTData meterDLTData = new MeterDLTData();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }
                List<string> list = new List<string>();
                foreach (string item in ResultValue.Keys)
                {
                    if (item != "结论")
                    {
                        list.Add(item);
                    }
                }
                if (meter[i].MD_ProtocolName.IndexOf("645") != -1)
                {
                    meterDLTData.DataFlag = TestValue["标识编码"]; //645
                }
                else
                {
                    meterDLTData.DataFlag = TestValue["标识编码698"]; //698
                }
                meterDLTData.DataFormat = TestValue["数据格式"];
                meterDLTData.DataLen = TestValue["长度"];
                meterDLTData.FlagMsg = TestValue["数据项名称"];
                meterDLTData.StandardValue = TestValue["写入内容"];

                meterDLTData.Value = ResultValue["检定信息"];
                meterDLTData.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterDLTData.Result = "不合格";
                }
                if (meter[i].MeterDLTDatas.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDLTDatas.Remove(CheckResult.ItemKey);
                meter[i].MeterDLTDatas.Add(CheckResult.ItemKey, meterDLTData);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meterDLTData.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        #endregion


    }
}
