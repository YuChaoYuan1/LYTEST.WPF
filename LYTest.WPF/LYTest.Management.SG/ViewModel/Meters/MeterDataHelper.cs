using LYTest.Core.Enum;
using LYTest.Core;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LYTest.Core.Model.DnbModel;
using LYTest.DAL.DataBaseView;

namespace LYTest.DataManager.SG.ViewModel.Meters
{
    public class MeterDataHelper
    {

        #region MyRegion
        /// <summary>
        /// 方案参数
        /// </summary>
        public static Dictionary<string, DynamicModel> Models;
        public static Dictionary<string, List<string>> ModelsName;
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
                    List<string> value = null;
                    if (models[i].GetProperty("PARA_VIEW") is string str)
                    {
                        value = str.Split('|').ToList();
                    }
                    ModelsName.Add(name, value);
                }
                if (!Models.ContainsKey(name))
                {
                    Models.Add(name, models[i]);
                }
            }


        }
        #endregion
        #region 新--优化速度
        public static TestMeterInfo GetDnbInfoNew(OneMeterResult meterResult)
        {
            string str = "";
            TestMeterInfo meterTemp = new TestMeterInfo();
            if (meterResult == null || meterResult.MeterInfo == null) return meterTemp;
            DynamicViewModel model = meterResult.MeterInfo;
            if (string.IsNullOrWhiteSpace(model.GetProperty("MD_BAR_CODE") as string))
            {
                return meterTemp;
            }
            #region 基本信息
            meterTemp.Meter_ID = model.GetProperty("METER_ID").ToString();
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
            //meterTemp.MD_CheckDevice = model.GetProperty("MD_UB").ToString();
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
            meterTemp.YaoJianYn = model.GetProperty("MD_CHECKED").ToString() == "1";
            meterTemp.Result = model.GetProperty("MD_RESULT").ToString();  //表结论
            //modify
            meterTemp.Humidity = model.GetProperty("MD_HUMIDITY").ToString();  //湿度
            meterTemp.Temperature = model.GetProperty("MD_TEMPERATURE").ToString();  //温度
            meterTemp.Checker1 = model.GetProperty("MD_TEST_PERSON").ToString();  //检验员
            meterTemp.Checker2 = model.GetProperty("MD_AUDIT_PERSON").ToString();  //核验员
            //add
            meterTemp.ExpiryDate = model.GetProperty("MD_VALID_DATE").ToString();
            meterTemp.MD_Sort = model.GetProperty("MD_SORT") as string;
            meterTemp.MD_UPDOWN = model.GetProperty("MD_UPDOWN") as string;
            #endregion
            List<DynamicModel> models = new List<DynamicModel>();
            GeneralDal dal = DALManager.SchemaDal;
            List<string> tableNames = dal.GetTableNames();
            tableNames.Remove("T_SCHEMA_INFO");
            List<string> sqlList = new List<string>();
            string schemdID = model.GetProperty("MD_SCHEME_ID").ToString();  //方案id

            for (int i = 0; i < tableNames.Count; i++)
            {
                sqlList.Add(string.Format("select * from {0} where SCHEMA_ID={1}", tableNames[i], schemdID));
            }
            models = dal.GetList(tableNames, sqlList);

            GetModels();
            GetResoult(meterTemp);

            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                for (int j = 0; j < meterResult.Categories[i].ResultUnits.Count; j++)
                {
                    DynamicViewModel result = meterResult.Categories[i].ResultUnits[j];
                    string ItemKey = result.GetProperty("项目号") as string; //这个项目号的详细的项目号，需要分割找到大类编号
                    string ItemName = result.GetProperty("项目名") as string;
                    string DID = ItemKey.Split('_')[0];


                    List<string> PlanParamsName = null;
                    if (ModelsName != null || ModelsName.ContainsKey(DID))
                        PlanParamsName = ModelsName[DID];//检定点的格式

                    //TODO:检定参数
                    DynamicModel TestParamValueModel = models.Find(a => a.GetProperty("PARA_VALUE_NO").ToString() == ItemKey);
                    string testParamsValue = "";
                    if (TestParamValueModel != null)
                    {
                        testParamsValue = TestParamValueModel.GetProperty("PARA_VALUE") as string;
                    }
                    Dictionary<string, string> TestParamValue = new Dictionary<string, string>();
                    if (PlanParamsName != null && testParamsValue != null)
                    {
                        string[] value = testParamsValue.Split('|');//检定用到的参数
                        if (PlanParamsName.Count == value.Length)
                        {
                            for (int n = 0; n < PlanParamsName.Count; n++)
                            {
                                if (!TestParamValue.ContainsKey(PlanParamsName[n]))
                                {
                                    TestParamValue.Add(PlanParamsName[n], value[n]);
                                }
                            }
                        }
                    }

                    if (meterTemp != null && !string.IsNullOrWhiteSpace(meterTemp.MD_BarCode))
                    {
                        MeterTestData testData = new MeterTestData();
                        if (TestParamValueModel != null)
                        {
                            testData.index = int.Parse(TestParamValueModel.GetProperty("PARA_INDEX") as string);
                        }
                        testData.ItemNo = ItemKey;
                        testData.Name = ItemName;
                        testData.ParameterNames = string.Join("|", PlanParamsName);
                        testData.ParameterValues = testParamsValue;

                        List<string> names = new List<string>();
                        List<string> datas = new List<string>();
                        TableDisplayModel displayModel = ResultViewHelper.GetTableDisplayModel(meterResult.Categories[i].ViewName);
                        foreach (string item in displayModel.GetDisplayNames())
                        {
                            if (item == "结论") continue;
                            names.Add(item);
                            datas.Add(result.GetProperty(item) as string);
                        }
                        testData.ResultNames = string.Join("|", names);
                        testData.ResultValues = string.Join("^", datas);
                        testData.Result = result.GetProperty("结论") as string;

                        if (meterTemp.MeterTestDatas.ContainsKey(ItemKey))
                            meterTemp.MeterTestDatas.Remove(ItemKey);

                        meterTemp.MeterTestDatas.Add(ItemKey, testData);
                    }
                    switch (DID)
                    {
                        case ProjectID.接线检查:
                            break;
                        case ProjectID.基本误差试验://基本误差
                        case ProjectID.标准偏差试验:
                            GetMeterError(meterTemp, result);
                            break;
                        //add
                        case ProjectID.初始固有误差:
                            GetMeterInitialError(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.起动试验:
                        case ProjectID.潜动试验:
                            GetMeterQdQid(meterTemp, result);
                            break;
                        case ProjectID.电能表常数试验:
                            GetMeterZZError(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.身份认证:
                        case ProjectID.远程控制:
                        case ProjectID.报警功能:
                        case ProjectID.远程保电:
                        case ProjectID.保电解除:
                        case ProjectID.数据回抄:
                        case ProjectID.钱包初始化:
                        case ProjectID.密钥更新:
                        case ProjectID.密钥恢复:
                        case ProjectID.费控_电量清零:
                        case ProjectID.剩余电量递减准确度:
                        case ProjectID.负荷开关:
                        case ProjectID.参数设置:
                        case ProjectID.控制功能:
                        case ProjectID.身份认证_计量芯:
                        case ProjectID.密钥更新_预先调试:
                        case ProjectID.密钥恢复_预先调试:
                            GetMeterFK(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.GPS对时:
                        case ProjectID.日计时误差:
                        case ProjectID.需量示值误差:
                        case ProjectID.电量清零:
                        case ProjectID.需量清空:
                        case ProjectID.通讯测试:
                        case ProjectID.负载电流快速改变:
                        case ProjectID.读取电量:
                        case ProjectID.费率时段检查:
                        case ProjectID.时段投切:
                        case ProjectID.电能示值组合误差:
                        case ProjectID.时钟示值误差:
                        case ProjectID.测量及监测误差:
                        case ProjectID.停电转存试验:
                        case ProjectID.费率时段示值误差:
                        case ProjectID.闰年判断功能:
                        case ProjectID.采用备用电源工作的时钟试验:
                        case ProjectID.零线电流检测:
                        case ProjectID.交流电压暂降和短时中断:
                        //case ProjectID.载波芯片ID测试:
                            GetDgn(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.误差一致性:
                        case ProjectID.误差变差:
                        case ProjectID.负载电流升将变差:
                            GetMeterErrAccord(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.通讯协议检查试验:
                            //delete yjt 20220516 暂时注释
                            //GetMeterDLTData(meterTemp, result, TestValue);
                            break;
                        case ProjectID.通讯协议检查试验2:
                            GetMeterDLTData2(meterTemp, result, TestParamValue);
                            break;
                        //add yjt 20220310 新增外观检查，工频耐压试验，显示功能
                        case ProjectID.外观检查:
                            GetMeterDefault(meterTemp, result);
                            break;
                        case ProjectID.工频耐压试验:
                            GetMeterInsulation(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.显示功能:
                            GetMeterShow(meterTemp, result);
                            break;
                        case ProjectID.第N次谐波试验:
                        case ProjectID.方顶波波形试验:
                        case ProjectID.尖顶波波形改变:
                        case ProjectID.脉冲群触发波形试验:
                        case ProjectID.九十度相位触发波形试验:
                        case ProjectID.半波整流波形试验:
                        case ProjectID.频率改变:
                        case ProjectID.电压改变:
                        case ProjectID.负载不平衡试验:
                        case ProjectID.辅助装置试验:
                        case ProjectID.逆相序试验:
                        case ProjectID.一相或两相电压中断试验:
                        case ProjectID.高次谐波:
                        case ProjectID.自热试验:
                            GetInfluence(meterTemp, result, TestParamValue);
                            break;
                        //case ProjectID.载波通信测试:
                            //case ProjectID.载波芯片ID测试:
                            //GetCarrier(meterTemp, result, TestParamValue);
                            //break;
                        case ProjectID.功耗试验:
                            GetPowerConsume(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.计量功能:
                        case ProjectID.计时功能:
                        case ProjectID.费率时段功能:
                        case ProjectID.脉冲输出功能:
                        case ProjectID.最大需量功能:
                        case ProjectID.停电抄表功能:
                        case ProjectID.时区时段功能:
                            GetFunction(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.定时冻结:
                        case ProjectID.约定冻结:
                        case ProjectID.瞬时冻结:
                        case ProjectID.日冻结:
                        case ProjectID.整点冻结:
                        case ProjectID.分钟冻结:
                        case ProjectID.小时冻结:
                        case ProjectID.月冻结:
                        case ProjectID.年冻结:
                        case ProjectID.结算日冻结:
                            GetFrozen(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.失压记录:
                        case ProjectID.过压记录:
                        case ProjectID.欠压记录:
                        case ProjectID.失流记录:
                        case ProjectID.断流记录:
                        case ProjectID.过流记录:
                        case ProjectID.过载记录:
                        case ProjectID.断相记录:
                        case ProjectID.掉电记录:
                        case ProjectID.全失压记录:
                        case ProjectID.电压不平衡记录:
                        case ProjectID.电流不平衡记录:
                        case ProjectID.电压逆向序记录:
                        case ProjectID.电流逆向序记录:
                        case ProjectID.校时记录:
                        case ProjectID.开表盖记录:
                        case ProjectID.开端钮盖记录:
                        case ProjectID.编程记录:
                        case ProjectID.需量清零记录:
                        case ProjectID.事件清零记录:
                        case ProjectID.电表清零记录:
                        case ProjectID.潮流反向记录:
                        case ProjectID.功率反向记录:
                        case ProjectID.需量超限记录:
                        case ProjectID.功率因数超下限记录:
                        case ProjectID.恒定磁场干扰记录:
                        case ProjectID.跳闸合闸事件记录:

                        case ProjectID.时钟故障事件记录:
                        case ProjectID.事件跟随上报:
                        case ProjectID.事件主动上报_载波:
                        case ProjectID.广播校时事件记录:
                        case ProjectID.零线电流异常事件记录:
                            GetEventLog(meterTemp, result, TestParamValue);
                            break;

                        case ProjectID.负荷记录:
                            GetLoadRecord(meterTemp, result, TestParamValue);
                            break;

                        //黑龙江使用
                        case ProjectID.电能表常数试验_黑龙江:
                            GetMeterZZError_HLJ(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.GPS对时_黑龙江:
                            GetDgn_HLJ(meterTemp, result);
                            break;
                        case ProjectID.日计时误差_黑龙江:
                            GetClockError_HLJ(meterTemp, result, TestParamValue);
                            break;
                        //case ProjectID.身份认证_黑龙江:
                        case ProjectID.密钥更新_黑龙江:
                            GetMeterFK_HLJ(meterTemp, result);
                            break;
                        case ProjectID.通讯协议检查试验_黑龙江:
                            GetMeterDLTData_HLJ(meterTemp, result, TestParamValue);
                            break;
                        case ProjectID.通讯协议检查试验2_黑龙江:
                            GetMeterDLTData2_HLJ(meterTemp, result, TestParamValue);
                            break;
                        default:
                            break;
                    }

                }
            }
            return meterTemp;

        }

        /// <summary>
        /// 排除没有读取到null的情况
        /// </summary>
        /// <param name="model"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetValue(DynamicViewModel model, string name)
        {
            object obj = model.GetProperty(name);
            if (obj == null)
            {
                return "";
            }
            return obj.ToString();
        }


        #region 检定项目结论
        /// <summary>
        /// 获得总结论
        /// </summary>
        private static void GetResoult(TestMeterInfo meter)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            for (int j = 0; j < ID.Count; j++)
            {
                string strVaule = ID[j];//获取值
                if (meter.MeterResults.ContainsKey(strVaule))
                    meter.MeterResults.Remove(strVaule);
                MeterResult meterResult = new MeterResult
                {
                    Result = ConstHelper.合格
                };
                meter.MeterResults.Add(strVaule, meterResult);
            }
        }


        #region //黑龙江地区结论
        /// <summary>
        ///规约一致性（通讯协议检查）
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetMeterDLTData_HLJ(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();
            MeterDLTData meterDLTData = new MeterDLTData();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];


            meterDLTData.DataFlag = TestValue["标识编码"]; //645
            meterDLTData.DataFormat = TestValue["数据格式"];
            meterDLTData.DataLen = TestValue["长度"];
            meterDLTData.FlagMsg = TestValue["数据项名称"];
            meterDLTData.StandardValue = TestValue["写入内容"];
            meterDLTData.Value = ResultValue["检定信息"];
            meterDLTData.Result = ResultValue["结论"];

            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                meterDLTData.Result = ConstHelper.不合格;
            }
            if (meter.MeterDLTDatas.ContainsKey(ItemKey))
                meter.MeterDLTDatas.Remove(ItemKey);
            meter.MeterDLTDatas.Add(ItemKey, meterDLTData);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterDLTData.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterDLTData2_HLJ(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterDLTData meterDLTData = new MeterDLTData();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            List<string> list = new List<string>();
            foreach (string item in ResultValue.Keys)
            {
                if (item != "结论")
                {
                    list.Add(item);
                }
            }
            if (meter.MD_ProtocolName.IndexOf("645") != -1)
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
            if (meter.MeterDLTDatas.ContainsKey(ItemKey))
                meter.MeterDLTDatas.Remove(ItemKey);
            meter.MeterDLTDatas.Add(ItemKey, meterDLTData);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterDLTData.Result == "不合格" && meter.MeterResults[Para].Result == "合格")
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterZZError_HLJ(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterZZError meterError = new MeterZZError();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];
            meterError.Result = ResultValue["结论"];
            meterError.Fl = TestValue["费率"];
            meterError.GLYS = TestValue["功率因数"];
            meterError.IbX = TestValue["电流倍数"];
            if (meterError.IbX == "Ib")
            {
                meterError.IbX = "1.0Ib";
            }
            meterError.PowerWay = TestValue["功率方向"];
            meterError.TestWay = TestValue["走字试验方法类型"];
            meterError.YJ = TestValue["功率元件"];
            meterError.NeedEnergy = TestValue["走字电量(度)"];
            meterError.PowerSumEnd = ResultValue["起码"];
            meterError.PowerSumStart = ResultValue["止码"];
            meterError.WarkPower = "0";
            if (meterError.PowerSumEnd != "" && meterError.PowerSumStart != "")
            {
                meterError.WarkPower = (float.Parse(meterError.PowerSumStart) - float.Parse(meterError.PowerSumEnd)).ToString("f5");
            }
            meterError.PowerError = ResultValue["表码差"];
            meterError.STMEnergy = ResultValue["标准表脉冲"];  // ResultValue["标准表脉冲"]/d_meterInfo.GetBcs()[0])
            //meterError.STMEnergy = ResultValue["标准表脉冲"];  // ResultValue["标准表脉冲"]/d_meterInfo.GetBcs()[0])
            meterError.Pules = ResultValue["表脉冲"];
            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterZZErrors.ContainsKey(ItemKey))
                meter.MeterZZErrors.Remove(ItemKey);
            meter.MeterZZErrors.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetDgn_HLJ(TestMeterInfo meter, DynamicViewModel CheckResult)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterDgn meterError = new MeterDgn();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];
            meterError.Result = ResultValue["结论"];

            List<string> list = new List<string>();
            foreach (string item in ResultValue.Keys)
            {
                if (item != "结论")
                {
                    list.Add(item);
                }
            }
            meterError.Value = string.Join("|", list);
            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                meterError.Result = "不合格";
            }
            if (meter.MeterDgns.ContainsKey(ItemKey))
                meter.MeterDgns.Remove(ItemKey);
            meter.MeterDgns.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == "不合格" && meter.MeterResults[Para].Result == "合格")
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterFK_HLJ(TestMeterInfo meter, DynamicViewModel CheckResult)
        {

            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterFK meterError = new MeterFK();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];
            List<string> list = new List<string>();
            foreach (string item in ResultValue.Keys)
            {
                if (item != "结论")
                {
                    list.Add(item);
                }
            }
            switch (ItemKey)
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
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterCostControls.ContainsKey(ItemKey))
                meter.MeterCostControls.Remove(ItemKey);
            meter.MeterCostControls.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetClockError_HLJ(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();
            MeterDgn meterError = new MeterDgn();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];
            meterError.Result = ResultValue["结论"];

            meterError.WCData = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["误差3"] + "|" + ResultValue["误差4"] + "|" + ResultValue["误差5"];
            meterError.AvgValue = ResultValue["平均值"];
            meterError.HzValue = ResultValue["化整值"];
            meterError.ErrorRate = "±" + ResultValue["误差限(s/d)"];
            meterError.WCRate = ResultValue["误差圈数"];

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

            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterDgns.ContainsKey(ItemKey))
                meter.MeterDgns.Remove(ItemKey);
            meter.MeterDgns.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        #endregion
        #region //其他地区结论
        /// <summary>
        /// 获得基本误差结论
        /// </summary>
        /// <param name="meter">表类</param>
        /// <param name="CheckResult">检定结论</param>
        /// <param name="TestValue">检定参数值</param>
        private static void GetMeterError(TestMeterInfo meter, DynamicViewModel CheckResult)
        {

            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterError meterError = new MeterError();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];
            //add yjt 20220310 新增项目号
            meterError.PrjNo = ResultValue["项目号"];

            meterError.GLYS = ResultValue["功率因数"];
            meterError.GLFX = ResultValue["功率方向"];
            //meterError.PrjNo = CheckResult.ItemKey;
            meterError.YJ = ResultValue["功率元件"];
            if (ResultValue.ContainsKey("结论"))
                meterError.Result = ResultValue["结论"];
            //string[] results = result.Split('|');
            //meterError.Result = results[0];
            //meterError.AVR_DIS_REASON = results[1];
            meterError.IbX = ResultValue["电流倍数"];
            if (meterError.IbX == "Ib")
            {
                meterError.IbX = "1.0Ib";
            }
            else if (meterError.IbX == "1.0In" || meterError.IbX == "1In")
            {
                meterError.IbX = "In";
            }
            else if (meterError.IbX == "10Itr")
            {
                meterError.IbX = "10.0Itr";
            }
            else if (meterError.IbX == "1Itr" || meterError.IbX == "1.0Itr")
            {
                meterError.IbX = "Itr";
            }

            if (Para == ProjectID.标准偏差试验)
            {
                meterError.WCData = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["误差3"] + "|" + ResultValue["误差4"] + "|" + ResultValue["误差5"];
                meterError.WCPC = ResultValue["偏差值"];
            }
            else
            {
                meterError.WCData = ResultValue["误差1"] + "|" + ResultValue["误差2"];
                meterError.WCValue = ResultValue["平均值"];
            }

            meterError.WCHZ = ResultValue["化整值"];
            meterError.Limit = "±" + ResultValue["误差上限"];
            meterError.BPHUpLimit = ResultValue["误差上限"];
            meterError.BPHDownLimit = ResultValue["误差下限"];
            meterError.Circle = ResultValue["误差圈数"];
            if (ResultValue.ContainsKey("结论"))
            {
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
                {
                    meterError.Result = ConstHelper.不合格;
                }
            }
            if (meter.MeterErrors.ContainsKey(ItemKey))
                meter.MeterErrors.Remove(ItemKey);
            meter.MeterErrors.Add(ItemKey, meterError);

            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        /// <summary>
        /// 初始固有误差结论
        /// </summary>
        /// <param name="meter">表类</param>
        /// <param name="CheckResult">检定结论</param>
        /// <param name="TestValue">检定参数值</param>
        private static void GetMeterInitialError(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterInitialError meterError = new MeterInitialError();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            //PrjNo;GLYS;GLFX;YJ;IbX;IbXString;UpWCValue;UpWCHZ;DownWCValue;DownWCPJ;DownWCHZ;WCData;UpLimit;DownLimit;Remark;Result;
            string ItemKey = ResultValue["项目号"];
            string Para = meterError.PrjNo.Split('_')[0];
            meterError.PrjNo = ItemKey;
            meterError.Name = ResultValue["项目名"];

            meterError.Result = ResultValue["结论"];

            meterError.GLYS = TestValue["功率因数"];
            meterError.GLFX = TestValue["功率方向"];
            meterError.YJ = TestValue["功率元件"];
            meterError.IbX = TestValue["电流倍数"];
            if (TestValue["电流倍数"] == "Ib")
            {
                meterError.IbX = "1.0Ib";
            }

            meterError.UpWCValue = ResultValue["上升误差1"] + "|" + ResultValue["上升误差2"];
            meterError.UpWCPJ = ResultValue["上升平均值"];
            meterError.UpWCHZ = ResultValue["上升化整值"];
            meterError.DownWCValue = ResultValue["下降误差1"] + "|" + ResultValue["下降误差2"];
            meterError.DownWCPJ = ResultValue["下降平均值"];
            meterError.DownWCHZ = ResultValue["下降化整值"];
            meterError.WCData = ResultValue["差值"];
            meterError.WcMore = ResultValue["上升误差1"] + "|" + ResultValue["上升误差2"] + "|" + ResultValue["上升平均值"] + "|" + ResultValue["上升化整值"] + "|"
                                + ResultValue["下降误差1"] + "|" + ResultValue["下降误差2"] + "|" + ResultValue["下降平均值"] + "|" + ResultValue["下降化整值"] + "|" + ResultValue["差值"];
            meterError.UpLimit = ResultValue["误差上限"];
            meterError.DownLimit = ResultValue["误差下限"];
            meterError.Circle = ResultValue["误差圈数"];

            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterInitialError.ContainsKey(ItemKey))
                meter.MeterInitialError.Remove(ItemKey);
            meter.MeterInitialError.Add(ItemKey, meterError);

            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterZZError(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterZZError meterError = new MeterZZError();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            //add yjt 20220310 新增项目号
            meterError.PrjID = ResultValue["项目号"];
            string[] zParam = ResultValue["项目名"].Split('_');
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
            if (ResultValue.ContainsKey("误差"))
                meterError.ErrorValue = ResultValue["误差"];
            if (TestValue.ContainsKey("走字电量(度)"))
                meterError.NeedEnergy = TestValue["走字电量(度)"];
            else
            {

            }
            meterError.PowerSumStart = ResultValue["起码"];
            meterError.PowerSumEnd = ResultValue["止码"];
            //add yjt 20220310 新增起码止码
            decimal.TryParse(ResultValue["起码"], out decimal startTmp);
            meterError.PowerStart = startTmp;
            decimal.TryParse(ResultValue["止码"], out decimal endTmp);
            meterError.PowerEnd = endTmp;
            meterError.WarkPower = "0";
            if (!string.IsNullOrWhiteSpace(meterError.PowerSumEnd) && !string.IsNullOrWhiteSpace(meterError.PowerSumStart))
            {
                meterError.WarkPower = (float.Parse(meterError.PowerSumEnd) - float.Parse(meterError.PowerSumStart)).ToString("f5");
            }
            meterError.PowerError = ResultValue["表码差"];
            if (ResultValue.ContainsKey("标准表脉冲"))
                meterError.STMEnergy = ResultValue["标准表脉冲"];

            if (ResultValue.ContainsKey("表脉冲"))
                meterError.Pules = ResultValue["表脉冲"];
            if (!ResultValue.ContainsKey("结论") || string.IsNullOrWhiteSpace(ResultValue["结论"])) //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterZZErrors.ContainsKey(ItemKey))
                meter.MeterZZErrors.Remove(ItemKey);
            meter.MeterZZErrors.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterQdQid(TestMeterInfo meter, DynamicViewModel CheckResult)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterQdQid meterError = new MeterQdQid();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            //add yjt 20220310 新增项目名,项目号
            meterError.Name = ResultValue["项目名"];
            meterError.PrjNo = ResultValue["项目号"];

            meterError.ActiveTime = ResultValue["实际运行时间"].Trim('分');
            meterError.PowerWay = ResultValue["功率方向"];
            meterError.Voltage = ResultValue["试验电压"];
            meterError.TimeEnd = ResultValue["结束时间"];
            meterError.TimeStart = ResultValue["开始时间"];

            meterError.Pulse = ResultValue["脉冲数"];
            meterError.ErrorValue = ResultValue["误差"];

            meterError.StandartTime = ResultValue["标准试验时间"];
            meterError.Current = ResultValue["试验电流"];
            meterError.Result = ResultValue["结论"];
            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterQdQids.ContainsKey(ItemKey))
                meter.MeterQdQids.Remove(ItemKey);
            meter.MeterQdQids.Add(ItemKey, meterError);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }


        /// <summary>
        /// 影响量结论
        /// </summary>
        /// <param name="meter">表类</param>
        /// <param name="CheckResult">检定结论</param>
        /// <param name="TestValue">检定参数值</param>
        private static void GetInfluence(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterSpecialErr SpecialErr = new MeterSpecialErr();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            //PrjNo;GLYS;GLFX;YJ;IbX;IbXString;UpWCValue;UpWCHZ;DownWCValue;DownWCPJ;DownWCHZ;WCData;UpLimit;DownLimit;Remark;Result;
            string ItemKey = ResultValue["项目号"];
            string Para = SpecialErr.PrjNo.Split('_')[0];
            SpecialErr.PrjNo = ItemKey;
            SpecialErr.Name = ResultValue["项目名"];

            SpecialErr.Result = ResultValue["结论"];

            if (TestValue.Count > 0)
            {
                if (ItemKey != ProjectID.自热试验)
                {
                    SpecialErr.GLYS = TestValue["功率因数"];
                    SpecialErr.GLFX = TestValue["功率方向"];
                    SpecialErr.YJ = TestValue["功率元件"];
                    SpecialErr.IbX = TestValue["电流倍数"];
                    if (TestValue["电流倍数"] == "Ib")
                    {
                        SpecialErr.IbX = "1.0Ib";
                    }
                }
            }

            if (ItemKey == ProjectID.高次谐波)
            {
                SpecialErr.Error1 = ResultValue["误差1"] + "~" + ResultValue["误差2"] + "~" + ResultValue["平均值"] + "~" + ResultValue["化整值"];
                SpecialErr.ErrValue = ResultValue["偏差值"];
            }
            else if (ItemKey == ProjectID.自热试验)
            {
                SpecialErr.Error1 = ResultValue["10误差变化值"];
                SpecialErr.Error2 = ResultValue["05L误差变化值"];
            }
            else
            {
                SpecialErr.Error1 = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["平均值"] + "|" + ResultValue["化整值"];
                SpecialErr.Error2 = ResultValue["影响后误差1"] + "|" + ResultValue["影响后误差2"] + "|" + ResultValue["影响后平均值"] + "|" + ResultValue["影响后化整值"];
                SpecialErr.ErrInt = ResultValue["影响后化整值"];
                SpecialErr.ErrAvg = ResultValue["影响后平均值"];
                SpecialErr.ErrValue = ResultValue["变差值"];

                SpecialErr.WcMore = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["平均值"] + "|" + ResultValue["化整值"] + "|"
                                    + ResultValue["影响后误差1"] + "|" + ResultValue["影响后误差2"] + "|" + ResultValue["影响后平均值"] + "|" + ResultValue["影响后化整值"] + "|" + ResultValue["变差值"];
                SpecialErr.ErrLimitUp = ResultValue["误差上限"];
                SpecialErr.ErrLimitDown = ResultValue["误差下限"];
                SpecialErr.ErrLimit = ResultValue["误差上限"] + "|" + ResultValue["误差下限"];
                SpecialErr.Circle = ResultValue["误差圈数"];
            }


            if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
            {
                SpecialErr.Result = ConstHelper.不合格;
            }
            if (meter.MeterSpecialErrs.ContainsKey(ItemKey))
                meter.MeterSpecialErrs.Remove(ItemKey);
            meter.MeterSpecialErrs.Add(ItemKey, SpecialErr);

            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (SpecialErr.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        /// <summary>
        ///智能表功能试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetFunction(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterFunction Function = new MeterFunction();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            Function.Name = ResultValue["项目名"];
            Function.PrjID = ResultValue["项目号"];
            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }

            Function.Value = string.Join("|", listErr);
            if (TestValue.Count > 0)
            {
                Function.TestValue = string.Join("|", list);
            }

            Function.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                Function.Result = ConstHelper.不合格;
            }
            if (meter.MeterFunctions.ContainsKey(ItemKey))
                meter.MeterFunctions.Remove(ItemKey);
            meter.MeterFunctions.Add(ItemKey, Function);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (Function.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        /// <summary>
        ///事件记录
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetEventLog(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterSjJLgn SjJLgn = new MeterSjJLgn();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            SjJLgn.Name = ResultValue["项目名"];
            SjJLgn.PrjID = ResultValue["项目号"];
            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }

            SjJLgn.Value = string.Join("|", listErr);
            if (TestValue.Count > 0)
            {
                SjJLgn.TestValue = string.Join("|", list);
            }

            SjJLgn.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                SjJLgn.Result = ConstHelper.不合格;
            }
            if (meter.MeterSjJLgns.ContainsKey(ItemKey))
                meter.MeterSjJLgns.Remove(ItemKey);
            meter.MeterSjJLgns.Add(ItemKey, SjJLgn);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (SjJLgn.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        /// <summary>
        ///冻结
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetFrozen(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterFreeze Freeze = new MeterFreeze();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            Freeze.Name = ResultValue["项目名"];
            Freeze.PrjID = ResultValue["项目号"];
            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }

            Freeze.Value = string.Join("|", listErr);
            if (TestValue.Count > 0)
            {
                Freeze.TestValue = string.Join("|", list);
            }

            Freeze.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                Freeze.Result = ConstHelper.不合格;
            }
            if (meter.MeterFreezes.ContainsKey(ItemKey))
                meter.MeterFreezes.Remove(ItemKey);
            meter.MeterFreezes.Add(ItemKey, Freeze);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (Freeze.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        /// <summary>
        ///负荷记录
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetLoadRecord(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterLoadRecord LoadRecord = new MeterLoadRecord();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            LoadRecord.Name = ResultValue["项目名"];
            LoadRecord.PrjID = ResultValue["项目号"];
            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }

            LoadRecord.Value = string.Join("|", listErr);
            if (TestValue.Count > 0)
            {
                LoadRecord.TestValue = string.Join("|", list);
            }

            LoadRecord.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                LoadRecord.Result = ConstHelper.不合格;
            }
            if (meter.MeterLoadRecords.ContainsKey(ItemKey))
                meter.MeterLoadRecords.Remove(ItemKey);
            meter.MeterLoadRecords.Add(ItemKey, LoadRecord);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (LoadRecord.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }


        /// <summary>
        ///功耗
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetPowerConsume(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterPower Power = new MeterPower();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            Power.Name = ResultValue["项目名"];
            Power.PrjID = ResultValue["项目号"];
            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            Power.UaPowerP = ValueErr["电压A线路有功(W)"];
            Power.UbPowerP = ValueErr["电压B线路有功(W)"];
            Power.UcPowerP = ValueErr["电压C线路有功(W)"];
            Power.UaPowerS = ValueErr["电压A线路视在(VA)"];
            Power.UbPowerS = ValueErr["电压B线路视在(VA)"];
            Power.UcPowerS = ValueErr["电压C线路视在(VA)"];
            Power.IaPowerS = ValueErr["电流A线路视在(VA)"];
            Power.IbPowerS = ValueErr["电流B线路视在(VA)"];
            Power.IcPowerS = ValueErr["电流C线路视在(VA)"];

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }

            Power.Value = string.Join("|", listErr);
            if (TestValue.Count > 0)
            {
                Power.TestValue = string.Join("|", list);
            }

            Power.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                Power.Result = ConstHelper.不合格;
            }
            if (meter.MeterPowers.ContainsKey(ItemKey))
                meter.MeterPowers.Remove(ItemKey);
            meter.MeterPowers.Add(ItemKey, Power);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (Power.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }
        #region OLD DayError
        /// <summary>
        ///日计时试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        //private static void GetClockError(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        //{
        //    List<string> ModelName = CheckResult.GetAllProperyName();
        //    MeterDgn meterError = new MeterDgn();

        //    string IsResult = ""; //是否合格
        //    Dictionary<string, string> ResultValue = new Dictionary<string, string>();
        //    Dictionary<string, string> ValueErr = new Dictionary<string, string>();

        //    for (int j = 0; j < ModelName.Count; j++)
        //    {
        //        if (!ResultValue.ContainsKey(ModelName[j]))
        //        {
        //            string str = GetValue(CheckResult, ModelName[j]);

        //            if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
        //            {
        //                ValueErr.Add(ModelName[j], str);
        //            }
        //            if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
        //            {
        //                ResultValue.Add(ModelName[j], str);
        //            }
        //            if (ModelName[j] == "结论")
        //            {
        //                IsResult = str;
        //            }
        //        }
        //    }

        //    string ItemKey = ResultValue["项目号"];
        //    string Para = ItemKey.Split('_')[0];

        //    meterError.Name = ResultValue["项目名"];
        //    meterError.PrjID = ResultValue["项目号"];

        //    List<string> list = new List<string>();
        //    List<string> listErr = new List<string>();

        //    if (TestValue.Count > 0)
        //    {
        //        foreach (string item in TestValue.Values)
        //        {
        //            list.Add(item);
        //        }
        //    }

        //    foreach (string itemErr in ValueErr.Values)
        //    {
        //        listErr.Add(itemErr);
        //    }

        //    meterError.Value = string.Join("|", listErr);

        //    meterError.TestValue = string.Join("|", list);

        //    meterError.Result = IsResult;

        //    if (IsResult == null || IsResult == "") //没有结论的就跳过
        //    {
        //        meterError.Result = "不合格";
        //    }
        //    if (meter.MeterDgns.ContainsKey(ItemKey))
        //        meter.MeterDgns.Remove(ItemKey);
        //    meter.MeterDgns.Add(ItemKey, meterError);
        //    //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
        //    if (meterError.Result == "不合格" && meter.MeterResults[Para].Result == "合格")
        //    {
        //        meter.MeterResults[Para].Result = ConstHelper.不合格;
        //    }
        //}
        #endregion


        /// <summary>
        ///费控试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterFK(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterFK meterError = new MeterFK();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            meterError.Name = ResultValue["项目名"];

            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }



            meterError.Data = string.Join("|", listErr);
            if (TestValue.Count > 0)
            {
                meterError.TestValue = string.Join("|", list);
            }

            #region 旧判断实验，注释
            //    switch (ItemKey)
            //{
            //    case ProjectID.控制功能:
            //    case ProjectID.钱包初始化:


            //        break;
            //    case ProjectID.身份认证:
            //    case ProjectID.远程控制:
            //    case ProjectID.报警功能:
            //    case ProjectID.远程保电:
            //    case ProjectID.保电解除:
            //    case ProjectID.数据回抄:
            //    case ProjectID.密钥更新:
            //    case ProjectID.密钥恢复:
            //    case ProjectID.参数设置:
            //    case ProjectID.密钥更新_预先调试:
            //    case ProjectID.密钥恢复_预先调试:
            //        meterError.Data = string.Join("|", listErr);
            //        break;
            //    case ProjectID.剩余电量递减准确度:
            //        meterError.Data = string.Join("|", listErr);
            //        break;
            //    default:  //费控通用
            //        meterError.Data = string.Join("|", listErr);
            //        break;
            //}
            #endregion

            meterError.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterCostControls.ContainsKey(ItemKey))
                meter.MeterCostControls.Remove(ItemKey);
            meter.MeterCostControls.Add(ItemKey, meterError);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        /// <summary>
        ///多功能试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetDgn(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterDgn meterError = new MeterDgn();

            string IsResult = ""; //是否合格
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            Dictionary<string, string> ValueErr = new Dictionary<string, string>();

            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论" && ModelName[j] != "项目号" && ModelName[j] != "项目名" && ModelName[j] != "项目参数")
                    {
                        ValueErr.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "项目号" || ModelName[j] == "项目名")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }

            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            meterError.Name = ResultValue["项目名"];
            meterError.PrjID = ResultValue["项目号"];

            List<string> list = new List<string>();
            List<string> listErr = new List<string>();

            if (TestValue.Count > 0)
            {
                foreach (string item in TestValue.Values)
                {
                    list.Add(item);
                }
            }

            foreach (string itemErr in ValueErr.Values)
            {
                listErr.Add(itemErr);
            }


            meterError.Value = string.Join("|", listErr);

            if (TestValue.Count > 0)
            {
                meterError.TestValue = string.Join("|", list);
            }

            meterError.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterDgns.ContainsKey(ItemKey))
                meter.MeterDgns.Remove(ItemKey);
            meter.MeterDgns.Add(ItemKey, meterError);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterErrAccord(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            string index = "";
            switch (Para)
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

            MeterErrAccord meterError = new MeterErrAccord();
            if (meter.MeterErrAccords.ContainsKey(index))   //先判断是不是有了，有了在原来基础上添加
                meterError = meter.MeterErrAccords[index];
            else
                meter.MeterErrAccords.Add(index, meterError);

            MeterErrAccordBase meterErr = new MeterErrAccordBase
            {

                //add yjt 20220310 新增项目名,项目号
                Name = ResultValue["项目名"],
                PrjID = ResultValue["项目号"],

                Freq = meter.MD_Frequency.ToString()
            };
            meterError.Result = ResultValue["结论"];
            if (index == "1")    //补上圈数和误差限
            {
                meterErr.Name = "误差一致性";
                //误差1|误差2|平均值|化整值|样品均值|差值
                meterErr.IbX = TestValue["电流倍数"];  //电流
                if (TestValue["电流倍数"] == "Ib")
                {
                    meterErr.IbX = "1.0Ib";
                }
                meterErr.PF = TestValue["功率因数"];   //功率因数
                meterErr.PulseCount = ResultValue["检定圈数"];
                meterErr.Limit = "±" + ResultValue["误差上限"];
                meterErr.Data1 = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["平均值"] + "|" + ResultValue["化整值"];
                meterErr.ErrAver = ResultValue["样品均值"];
                meterErr.Error = ResultValue["差值"];
                meterErr.Result = ResultValue["结论"];

                int count = 1;
                if (meter.MeterErrAccords.ContainsKey(index))
                    count = meter.MeterErrAccords[index].PointList.Keys.Count + 1;
                meterError.PointList.Add(count.ToString(), meterErr);
            }
            else if (index == "2")    //1.0IB，
            {
                meterErr.Name = "误差变差";
                meterErr.PulseCount = ResultValue["检定圈数"];
                meterErr.Limit = "±" + ResultValue["误差上限"];

                meterErr.IbX = "1.0Ib"; //电流
                if (meter.MD_JJGC == "IR46")
                {
                    meterErr.IbX = "10Itr"; //电流
                }
                meterErr.PF = TestValue["功率因数"];  //功率因数
                                                  //第一次误差1|第一次误差2|第一次平均值|第一次化整值|第二次误差1|第二次误差2|第二次平均值|第二次化整值|变差值
                meterErr.Data1 = ResultValue["第一次误差1"] + "|" + ResultValue["第一次误差2"] + "|" + ResultValue["第一次平均值"] + "|" + ResultValue["第一次化整值"];
                meterErr.Data2 = ResultValue["第二次误差1"] + "|" + ResultValue["第二次误差2"] + "|" + ResultValue["第二次平均值"] + "|" + ResultValue["第二次化整值"];
                meterErr.Error = ResultValue["变差值"];
                meterErr.Result = ResultValue["结论"];
                int count = 1;
                if (meter.MeterErrAccords.ContainsKey(index))
                    count = meter.MeterErrAccords[index].PointList.Keys.Count + 1;
                meterError.PointList.Add(count.ToString(), meterErr);
            }
            else if (index == "3")   //这个需要加个判断，判断子项是否合格
            {
                string[] strI = new string[] { TestValue["电流点1"], TestValue["电流点2"], TestValue["电流点3"] };
                string[] str = new string[] { "01Ib", "Ib", "Imax" };
                for (int j = 0; j < 3; j++)
                {
                    meterErr = new MeterErrAccordBase
                    {
                        Freq = meter.MD_Frequency.ToString(),
                        Name = "负载电流升降变差",
                        PulseCount = ResultValue[str[j] + "检定圈数"],
                        Limit = "±1",
                        IbX = strI[j]  //电流
                    };
                    if (meterErr.IbX == "Ib")
                    {
                        meterErr.IbX = "1.0Ib";
                    }
                    meterErr.PF = "1.0";   //功率因数
                    meterErr.Data1 = ResultValue[str[j] + "上升误差1"] + "|" + ResultValue[str[j] + "上升误差2"] + "|" + ResultValue[str[j] + "上升平均值"] + "|" + ResultValue[str[j] + "上升化整值"];
                    meterErr.Data2 = ResultValue[str[j] + "下降误差1"] + "|" + ResultValue[str[j] + "下降误差2"] + "|" + ResultValue[str[j] + "下降平均值"] + "|" + ResultValue[str[j] + "下降化整值"];
                    meterErr.Error = ResultValue[str[j] + "差值"];
                    meterErr.Result = ResultValue["结论"];
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
                meterError.Result = ConstHelper.不合格;
            }
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
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
        private static void GetMeterDLTData2(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            MeterDLTData meterDLTData = new MeterDLTData();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);
                    ResultValue.Add(ModelName[j], str);
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            List<string> list = new List<string>();
            foreach (string item in ResultValue.Keys)
            {
                if (item != "结论")
                {
                    list.Add(item);
                }
            }
            if (meter.MD_ProtocolName.IndexOf("645") != -1)
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
                meterDLTData.Result = ConstHelper.不合格;
            }
            if (meter.MeterDLTDatas.ContainsKey(ItemKey))
                meter.MeterDLTDatas.Remove(ItemKey);
            meter.MeterDLTDatas.Add(ItemKey, meterDLTData);
            //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            if (meterDLTData.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        //add yjt 20220310 新增默认合格的方案 如外观检查
        /// <summary>
        /// 默认合格的方案 如外观检查，工频耐压试验，显示功能
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetMeterDefault(TestMeterInfo meter, DynamicViewModel CheckResult)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            string IsResult = ""; //是否合格

            MeterDefault meterError = new MeterDefault();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            meterError.DefaultName = ResultValue["项目名"];
            meterError.DefaultKey = ResultValue["项目号"];

            List<string> list = new List<string>();

            foreach (string item in ResultValue.Values)
            {
                list.Add(item);
            }

            meterError.Value = string.Join("|", list);

            meterError.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterDefaults.ContainsKey(ItemKey))
                meter.MeterDefaults.Remove(ItemKey);
            meter.MeterDefaults.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        //add yjt 20220310 新增工频耐压试验
        /// <summary>
        /// 工频耐压试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetMeterInsulation(TestMeterInfo meter, DynamicViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            string IsResult = ""; //是否合格

            MeterInsulation meterError = new MeterInsulation();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            meterError.DefaultName = ResultValue["项目名"];
            meterError.DefaultKey = ResultValue["项目号"];

            List<string> list = new List<string>();

            foreach (string item in ResultValue.Values)
            {
                list.Add(item);
            }
            meterError.Value = string.Join("|", list);

            if (ResultValue.ContainsKey("耐压值"))
            {
                if (!float.TryParse(ResultValue["耐压值"], out float vt))
                {
                    float.TryParse(TestValue["耐压电压值"], out vt);
                }
                meterError.Voltage = (int)vt;
            }
            else if (TestValue.ContainsKey("耐压电压值"))
            {
                float.TryParse(TestValue["耐压电压值"], out float vt);
                meterError.Voltage = (int)vt;
            }
            else meterError.Voltage = 4000;


            meterError.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterInsulations.ContainsKey(ItemKey))
                meter.MeterInsulations.Remove(ItemKey);
            meter.MeterInsulations.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }

        //add yjt 20220310 新增显示功能
        /// <summary>
        /// 显示功能
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetMeterShow(TestMeterInfo meter, DynamicViewModel CheckResult)
        {
            List<string> ModelName = CheckResult.GetAllProperyName();

            string IsResult = ""; //是否合格

            MeterShow meterError = new MeterShow();
            Dictionary<string, string> ResultValue = new Dictionary<string, string>();
            for (int j = 0; j < ModelName.Count; j++)
            {
                if (!ResultValue.ContainsKey(ModelName[j]))
                {
                    string str = GetValue(CheckResult, ModelName[j]);

                    if (ModelName[j] != "结论")
                    {
                        ResultValue.Add(ModelName[j], str);
                    }
                    if (ModelName[j] == "结论")
                    {
                        IsResult = str;
                    }
                }
            }
            string ItemKey = ResultValue["项目号"];
            string Para = ItemKey.Split('_')[0];

            meterError.DefaultName = ResultValue["项目名"];
            meterError.DefaultKey = ResultValue["项目号"];

            //List<string> list = new List<string>();

            //foreach (string item in ResultValue.Values)
            //{
            //    list.Add(item);
            //}

            //meterError.Value = string.Join("|", list);

            meterError.Result = IsResult;

            if (IsResult == null || IsResult == "") //没有结论的就跳过
            {
                meterError.Result = ConstHelper.不合格;
            }
            if (meter.MeterShows.ContainsKey(ItemKey))
                meter.MeterShows.Remove(ItemKey);
            meter.MeterShows.Add(ItemKey, meterError);
            ////如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
            ///
            if (meterError.Result == ConstHelper.不合格 && meter.MeterResults[Para].Result == ConstHelper.合格)
            {
                meter.MeterResults[Para].Result = ConstHelper.不合格;
            }
        }


        #endregion
        #endregion
        #endregion
    }
}
