using LYTest.Core;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.DAL;
using System;
using System.Collections.Generic;

namespace LYTest.Mis.DataHelper
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public class DataManage
    {

        /// <summary>
        /// 方案参数
        /// </summary>
        public static Dictionary<string, DynamicModel> Models;

        /// <summary>
        /// 视图参数
        /// </summary>
        public static Dictionary<string, DynamicModel> ViewModels;



        #region 旧方法

        /// <summary>
        /// 获取电能表详细数据
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="FlagOfTmp">true正式库，false临时库</param>
        /// <returns></returns>
        public static TestMeterInfo GetDnbInfoNew(TestMeterInfo meter, bool FlagOfTmp)
        {
            string InfoName = "T_TMP_METER_INFO";//表信息
            string TestDataName = "T_TMP_METER_COMMUNICATION"; //检定数据
            GeneralDal generalDal = DALManager.MeterTempDbDal;
            if (FlagOfTmp)
            {
                generalDal = DALManager.MeterDbDal;
                InfoName = "METER_INFO";//
                TestDataName = "METER_COMMUNICATION"; //检定数据
            }
            DynamicModel model = generalDal.GetByID(InfoName, $"METER_ID='{meter.Meter_ID}'");
            if (model == null) return null;

            //string str = "";
            //从数据库拿到表的数据
            TestMeterInfo meterTemp = new TestMeterInfo();

            #region 基本信息
            meterTemp.Meter_ID = meter.Meter_ID;
            meter.BenthNo = model.GetProperty("MD_DEVICE_ID").ToString();  //台体编号 
            meterTemp.BenthNo = model.GetProperty("MD_DEVICE_ID").ToString();  //台体编号     MD_DEVICE_ID
            string str = model.GetProperty("MD_FKTYPE").ToString();
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
            //meterTemp.MD_MeterNo = model.GetProperty("MD_UB").ToString();
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
            meterTemp.Humidity = model.GetProperty("MD_TEMPERATURE").ToString();  //湿度
            meterTemp.Temperature = model.GetProperty("MD_HUMIDITY").ToString();  //温度
            meterTemp.Checker1 = model.GetProperty("MD_TEST_PERSON").ToString();  //检验员
            meterTemp.Checker2 = model.GetProperty("MD_AUDIT_PERSON").ToString();  //核验员


            #endregion

            //DetailResultView

            // string SchemaID = model.GetProperty("MD_SCHEME_ID").ToString(); //方案编号，用于后面查找该表的检定项目数据

            //找到这个表的所有检定项的数据
            List<DynamicModel> testmodel = generalDal.GetList(TestDataName, $"METER_ID='{meter.Meter_ID}'");
            GetResoult(meterTemp, TestDataName, generalDal);

            list = new List<string>();
            foreach (DynamicModel item in testmodel)  //找到每个检定项
            {
                string keyNo = item.GetProperty("MD_PROJECT_NO").ToString();
                string value = item.GetProperty("MD_VALUE").ToString();
                string result = item.GetProperty("MD_RESULT").ToString();
                string testparams = item.GetProperty("MD_PARAMETER").ToString();


                try
                {
                    AddTestData(meterTemp, keyNo, testparams, value, result);
                }
                catch (Exception)
                {
                }

            }
            //List<string> list2 = list;
            return meterTemp;

        }
        public static List<string> list = new List<string>();
        /// <summary>
        /// 添加检定节点数据
        /// </summary>
        private static void AddTestData(TestMeterInfo meter, string KeyNo, string testparams, string value, string result)
        {
            string TestNo = KeyNo.Split('_')[0]; //检定大类编号--基本误差-走字。。。

            DynamicModel model;
            GetModels();
            if (Models != null || Models.ContainsKey(TestNo))
                model = Models[TestNo];//检定点的格式
            else
                model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), $"PARA_NO='{TestNo}'");//检定点的格式

            if (model == null) return;
            string TestName = model.GetProperty("PARA_NAME").ToString();         //检定项目名称
            string ViewNo = model.GetProperty("RESULT_VIEW_ID").ToString(); //结论视图ID
            string Parameter_colName = model.GetProperty("PARA_VIEW").ToString();          //检定参数列名

            DynamicModel viewModel;
            GetViewModels();
            if (ViewModels != null || ViewModels.ContainsKey(ViewNo))
                viewModel = ViewModels[ViewNo];//检定点的格式
            else
                viewModel = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), $"AVR_VIEW_ID='{ViewNo}'");//检定点的格式

            //找到结论视图
            string colunmName = viewModel.GetProperty("AVR_COL_SHOW_NAME").ToString();  //列名


            string Parameter_Value = testparams;
            if (string.IsNullOrWhiteSpace(testparams))
            {
                DynamicModel viewModel2 = DALManager.SchemaDal.GetByID(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), $"PARA_VALUE_NO='{KeyNo}'");
                if (viewModel2 == null) return;
                Parameter_Value = viewModel2.GetProperty("PARA_VALUE").ToString();
            }

            switch (TestName)
            {
                case "起动试验":
                case "启动试验":
                    GetMeterQdQid(meter, colunmName, value, result, KeyNo);
                    break;
                case "潜动试验":
                    GetMeterQdQid(meter, colunmName, value, result, KeyNo);
                    break;
                case "基本误差试验":
                    GetMeterError(meter, colunmName, value, result, KeyNo);
                    break;
                case "走字试验":
                    GetMeterZZError(meter, colunmName, value, result, KeyNo, Parameter_colName, Parameter_Value);
                    break;
                case "日计时误差":
                    GetClockError(meter, value, result, KeyNo, Parameter_Value);
                    break;
                case "身份认证":
                case "密钥更新":
                    GetMeterFK(meter, colunmName, value, result, KeyNo, TestName);
                    break;
                case "GPS对时":
                case "需量示值误差":
                    GetDgn(meter, value, result, KeyNo, Parameter_Value);
                    break;
                case "误差一致性":
                case "误差变差":
                case "负载电流升降变差":
                    GetMeterErrAccord(meter, colunmName, value, result, TestName, Parameter_Value);
                    break;
                case "通讯协议检查":
                    GetMeterDLTData(meter, colunmName, value, result, KeyNo, Parameter_colName, Parameter_Value);
                    break;
                case "通讯协议检查2":
                    GetMeterDLTData2(meter, colunmName, value, result, KeyNo, Parameter_colName, Parameter_Value);
                    break;
                case "远程控制":
                case "报警功能":
                case "远程保电":
                case "保电解除":
                    GetMeterFK(meter, colunmName, value, result, KeyNo, TestName);
                    break;
                default:
                    break;
            }
        }

        private static void GetModels()
        {
            if (Models != null) return;
            Models = new Dictionary<string, DynamicModel>();
            List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString());
            for (int i = 0; i < models.Count; i++)
            {
                string name = models[i].GetProperty("PARA_NO").ToString();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                if (!Models.ContainsKey(name))
                {
                    Models.Add(name, models[i]);
                };
            }
        }
        private static void GetViewModels()
        {
            if (ViewModels != null) return;
            ViewModels = new Dictionary<string, DynamicModel>();
            List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_VIEW_CONFIG.ToString());
            for (int i = 0; i < models.Count; i++)
            {
                string name = models[i].GetProperty("AVR_VIEW_ID").ToString();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                if (!ViewModels.ContainsKey(name))
                {
                    ViewModels.Add(name, models[i]);
                };
            }
        }

        /// <summary>
        /// 获得总结论
        /// </summary>
        private static void GetResoult(TestMeterInfo meter, string TestDataName, GeneralDal generalDal)
        {
            //先取出所有数
            List<DynamicModel> testmodel = generalDal.GetList(TestDataName, $"METER_ID='{meter.Meter_ID}' and MD_DEVICE_ID='{meter.BenthNo}'");
            Dictionary<string, List<string>> keyValues = new Dictionary<string, List<string>>();
            foreach (DynamicModel item in testmodel)
            {
                string keyNo = item.GetProperty("MD_PROJECT_NO").ToString();  //获得编号
                string result = item.GetProperty("MD_RESULT").ToString();  //获得结论
                string id = keyNo.Split('_')[0];  //获得大节点的编号
                if (keyValues.ContainsKey(id))   //如果已经存在了中这个大项目，就添加到他对应的列表中
                {
                    List<string> list = keyValues[id];
                    list.Add(result);
                    keyValues[id] = list;
                }
                else
                {
                    List<string> list = new List<string>
                    {
                        result
                    };
                    keyValues.Add(id, list);
                }
            }

            foreach (var item in keyValues.Keys)
            {
                List<string> lists = keyValues[item];
                MeterResult meterResult = new MeterResult
                {
                    Result = ConstHelper.合格
                };
                if (lists.Contains(ConstHelper.不合格))
                {
                    meterResult.Result = ConstHelper.不合格;
                }
                if (meter.MeterResults.ContainsKey(item))
                    meter.MeterResults.Remove(item);
                meter.MeterResults.Add(item, meterResult);
            }
        }


        #region 检定项目解析

        /// <summary>
        /// 基本误差解析
        /// </summary>
        /// <param name="meter">表</param>
        /// <param name="colunmName">结论详细数据列名</param>
        /// <param name="value">检定详细数据</param>
        /// <param name="result">结论</param>
        /// 
        private static void GetMeterError(TestMeterInfo meter, string colunmName, string value, string result, string KeyNo)
        {
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            string[] values = value.Split('^');
            Dictionary<string, string> keys = new Dictionary<string, string>();

            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }

            MeterError meterError = new MeterError
            {
                GLYS = keys["功率因数"],
                GLFX = keys["功率方向"],
                PrjNo = KeyNo,
                YJ = keys["功率元件"],
                Result = result,
                IbX = keys["电流倍数"],
                WCData = keys["误差1"] + "|" + keys["误差2"],
                WCHZ = keys["化整值"],
                WCValue = keys["平均值"],
                Limit = keys["误差上限"] + "|" + keys["误差下限"],
                BPHUpLimit = keys["误差上限"],
                BPHDownLimit = keys["误差下限"],
                Circle = keys["误差圈数"]
            };

            if (meterError.IbX == "Ib")
            {
                meterError.IbX = "1.0Ib";
            }

            if (meter.MeterErrors.ContainsKey(KeyNo))
                meter.MeterErrors.Remove(KeyNo);
            meter.MeterErrors.Add(KeyNo, meterError); ;
        }

        /// <summary>
        ///走字试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterZZError(TestMeterInfo meter, string colunmName, string value, string result, string KeyNo, string Parameter_colName, string Parameter_Value)
        {
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            string[] values = value.Split('^');
            Dictionary<string, string> keys = new Dictionary<string, string>();  //检定详细数据
            Dictionary<string, string> CS_keys = new Dictionary<string, string>();  //检定参数数据

            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }

            tem = Parameter_colName.Split('|'); //获得每一列的数据
            values = Parameter_Value.Split('|');
            for (int i = 0; i < tem.Length; i++)
            {
                if (CS_keys.ContainsKey(tem[i]))
                    CS_keys.Remove(tem[i]);
                CS_keys.Add(tem[i], values[i]);
            }

            MeterZZError meterError = new MeterZZError
            {
                Result = result,// keys["功率因数"];
                Fl = CS_keys["费率"],
                GLYS = CS_keys["功率因数"],
                IbX = CS_keys["电流倍数"],
                PowerWay = CS_keys["功率方向"],
                TestWay = CS_keys["走字试验方法类型"],
                YJ = CS_keys["功率元件"],
                NeedEnergy = CS_keys["走字电量(度)"],
                PowerSumStart = keys["起码"],
                PowerSumEnd = keys["止码"],
                WarkPower = "0",

                PowerError = keys["表码差"],
                STMEnergy = keys["标准表脉冲"]  // keys["标准表脉冲"]/d_meterInfo.GetBcs()[0])
            };
            meterError.Pules = keys["表脉冲"];

            if (meterError.IbX == "Ib")
            {
                meterError.IbX = "1.0Ib";
            }
            if (meterError.PowerSumEnd != "" && meterError.PowerSumStart != "")
            {
                meterError.WarkPower = (float.Parse(meterError.PowerSumEnd) - float.Parse(meterError.PowerSumStart)).ToString("f5");
            }
            if (meter.MeterZZErrors.ContainsKey(KeyNo))
                meter.MeterZZErrors.Remove(KeyNo);
            meter.MeterZZErrors.Add(KeyNo, meterError); ;
        }

        /// <summary>
        ///启动潜动试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterQdQid(TestMeterInfo meter, string colunmName, string value, string result, string KeyNo)
        {
            MeterQdQid meterError = new MeterQdQid();
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            string[] values = value.Split('^');
            Dictionary<string, string> keys = new Dictionary<string, string>();

            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }

            meterError.ActiveTime = keys["实际运行时间"].Trim('分');
            meterError.PowerWay = keys["功率方向"];
            meterError.Voltage = keys["试验电压"];
            meterError.TimeEnd = keys["结束时间"];
            meterError.TimeStart = keys["开始时间"];
            meterError.TimeStart = keys["开始时间"];
            meterError.StandartTime = keys["标准试验时间"];
            meterError.Current = keys["试验电流"];

            meterError.Result = result;
            if (meter.MeterQdQids.ContainsKey(KeyNo))
                meter.MeterQdQids.Remove(KeyNo);
            meter.MeterQdQids.Add(KeyNo, meterError); ;
        }

        /// <summary>
        ///日计时试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetClockError(TestMeterInfo meter, string value, string result, string KeyNo, string Parameter_Value)
        {
            MeterDgn meterError = new MeterDgn
            {
                Value = value.Replace("^", "|") + "|" + Parameter_Value,
                Result = result
            };
            if (meter.MeterDgns.ContainsKey(KeyNo))
                meter.MeterDgns.Remove(KeyNo);
            meter.MeterDgns.Add(KeyNo, meterError); ;
        }

        /// <summary>
        ///费控试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetMeterFK(TestMeterInfo meter, string colunmName, string value, string result, string KeyNo, string TestName)
        {
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            string[] values = value.Split('^');
            Dictionary<string, string> keys = new Dictionary<string, string>();

            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }

            MeterFK meterError = new MeterFK
            {
                Result = result
            };

            switch (TestName)
            {
                case "远程控制":
                    meterError.Name = TestName;
                    meterError.Data = value.Replace("^", "|");
                    break;
                case "报警功能":
                    meterError.Name = TestName;
                    meterError.Data = value.Replace("^", "|");
                    break;
                case "远程保电":
                case "保电解除":
                case "密钥更新":
                    meterError.Name = keys["当前项目"];
                    meterError.Data = keys["检定信息"];
                    break;
                default:  //费控通用
                    meterError.Name = keys["当前项目"];
                    meterError.Data = keys["检定信息"];
                    break;
            }

            if (meter.MeterCostControls.ContainsKey(KeyNo))
                meter.MeterCostControls.Remove(KeyNo);
            meter.MeterCostControls.Add(KeyNo, meterError); ;
        }

        /// <summary>
        ///多功能试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetDgn(TestMeterInfo meter, string value, string result, string KeyNo, string Parameter_Value)
        {
            MeterDgn meterError = new MeterDgn
            {
                Value = value.Replace("^", "|"),
                Result = result,
                TestValue = Parameter_Value
            };
            if (meter.MeterDgns.ContainsKey(KeyNo))
                meter.MeterDgns.Remove(KeyNo);
            meter.MeterDgns.Add(KeyNo, meterError);
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
        private static void GetMeterErrAccord(TestMeterInfo meter, string colunmName, string value, string result, string Name, string Parameter_Value)
        {
            string index = "";
            switch (Name)
            {
                case "误差一致性":
                    index = "1";
                    break;
                case "误差变差":
                    index = "2";
                    break;
                case "负载电流升降变差":
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
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            string[] values = value.Split('^');


            Dictionary<string, string> keys = new Dictionary<string, string>();

            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }

            string[] Testvalue = Parameter_Value.Split('|');


            MeterErrAccordBase meterErr = new MeterErrAccordBase
            {
                Freq = meter.MD_Frequency.ToString(),
                Result = result,
                Name = Name
            };

            if (Name == "误差一致性")    //补上圈数和误差限
            {
                //误差1|误差2|平均值|化整值|样品均值|差值
                meterErr.IbX = Testvalue[0];  //电流
                if (Testvalue[0] == "Ib")
                {
                    meterErr.IbX = "1.0Ib";
                }
                meterErr.PF = Testvalue[1];   //功率因数
                meterErr.PulseCount = keys["检定圈数"];
                meterErr.Limit = keys["误差上限"] + "|" + keys["误差下限"];
                meterErr.Data1 = keys["误差1"] + "|" + keys["误差2"] + "|" + keys["平均值"] + "|" + keys["化整值"];
                meterErr.ErrAver = keys["样品均值"];
                meterErr.Error = keys["差值"];

                int count = 1;
                if (meter.MeterErrAccords.ContainsKey(index))
                    count = meter.MeterErrAccords[index].PointList.Keys.Count + 1;
                meterError.PointList.Add(count.ToString(), meterErr);
            }
            else if (Name == "误差变差")    //1.0IB，
            {
                meterErr.PulseCount = keys["检定圈数"];
                meterErr.Limit = keys["误差上限"] + "|" + keys["误差下限"];
                meterErr.IbX = "1.0Ib";  //电流
                meterErr.PF = Testvalue[1];   //功率因数
                //第一次误差1|第一次误差2|第一次平均值|第一次化整值|第二次误差1|第二次误差2|第二次平均值|第二次化整值|变差值
                meterErr.Data1 = keys["第一次误差1"] + "|" + keys["第一次误差2"] + "|" + keys["第一次平均值"] + "|" + keys["第一次化整值"];
                meterErr.Data2 = keys["第二次误差1"] + "|" + keys["第二次误差2"] + "|" + keys["第二次平均值"] + "|" + keys["第二次化整值"];
                meterErr.Error = keys["变差值"];
                int count = 1;
                if (meter.MeterErrAccords.ContainsKey(index))
                    count = meter.MeterErrAccords[index].PointList.Keys.Count + 1;
                meterError.PointList.Add(count.ToString(), meterErr);
            }
            else if (Name == "负载电流升降变差")   //这个需要加个判断，判断子项是否合格
            {
                string[] str = new string[] { "01Ib", "Ib", "Imax" };
                for (int i = 0; i < 3; i++)
                {
                    meterErr = new MeterErrAccordBase
                    {
                        Freq = meter.MD_Frequency.ToString(),
                        Name = Name,
                        PulseCount = keys[str[i] + "检定圈数"],
                        IbX = str[i]  //电流
                    };
                    if (meterErr.IbX == "Ib")
                    {
                        meterErr.IbX = "1.0Ib";
                    }
                    if (str[i] == "01Ib") meterErr.IbX = "0.1Ib";
                    meterErr.PF = "1.0";   //功率因数
                    meterErr.Data1 = keys[str[i] + "上升误差1"] + "|" + keys[str[i] + "上升误差2"] + "|" + keys[str[i] + "上升平均值"] + "|" + keys[str[i] + "上升化整值"];
                    meterErr.Data2 = keys[str[i] + "下降误差1"] + "|" + keys[str[i] + "下降误差2"] + "|" + keys[str[i] + "下降平均值"] + "|" + keys[str[i] + "下降化整值"];
                    meterErr.Error = keys[str[i] + "差值"];
                    meterErr.Result = ConstHelper.不合格;
                    if (!string.IsNullOrEmpty(meterErr.Error))
                    {
                        float t = float.Parse(meterErr.Error);
                        if (Math.Abs(t) <= 0.25)
                            meterErr.Result = ConstHelper.合格;
                    }
                    meterError.PointList.Add((i + 1).ToString(), meterErr);
                }

            }

            meterError.Result = ConstHelper.合格;
            for (int i = 1; i <= meterError.PointList.Count; i++)
            {
                if (meterError.PointList[i.ToString()].Result == ConstHelper.不合格)     //有一个点不合格，总结论不合格
                {
                    meterError.Result = ConstHelper.不合格;
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
        private static void GetMeterDLTData(TestMeterInfo meter, string colunmName, string value, string result, string KeyNo, string Parameter_colName, string Parameter_Value)
        {
           string[] values = value.Split('^');
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            Dictionary<string, string> keys = new Dictionary<string, string>();
            Dictionary<string, string> CS_keys = new Dictionary<string, string>();  //检定参数数据
            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }
            tem = Parameter_colName.Split('|'); //获得每一列的数据
            values = Parameter_Value.Split('|');
            for (int i = 0; i < tem.Length; i++)
            {
                if (CS_keys.ContainsKey(tem[i]))
                    CS_keys.Remove(tem[i]);
                CS_keys.Add(tem[i], values[i]);
            }

            MeterDLTData meterDLTData = new MeterDLTData
            {
                Value = value.Replace("^", "|"),
                Result = result
            };
            meterDLTData.DataFlag = CS_keys["标识编码"]; //645
            meterDLTData.DataFormat = CS_keys["数据格式"];
            meterDLTData.DataLen = CS_keys["长度"];
            meterDLTData.FlagMsg = CS_keys["数据项名称"];
            meterDLTData.StandardValue = CS_keys["写入内容"];

            meterDLTData.Value = keys["检定信息"];


            if (meter.MeterDLTDatas.ContainsKey(KeyNo))
                meter.MeterDLTDatas.Remove(KeyNo);
            meter.MeterDLTDatas.Add(KeyNo, meterDLTData);
        }

        /// <summary>
        ///规约一致性（通讯协议检查）
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetMeterDLTData2(TestMeterInfo meter, string colunmName, string value, string result, string KeyNo, string Parameter_colName, string Parameter_Value)
        {
            MeterDLTData meterDLTData = new MeterDLTData
            {
                Value = value.Replace("^", "|"),
                Result = result
            };

            string[] values = value.Split('^');
            string[] tem = colunmName.Split(',')[0].Split('|'); //获得每一列的数据
            Dictionary<string, string> keys = new Dictionary<string, string>();
            Dictionary<string, string> CS_keys = new Dictionary<string, string>();  //检定参数数据
            for (int i = 0; i < tem.Length; i++)
            {
                if (keys.ContainsKey(tem[i]))
                    keys.Remove(tem[i]);
                keys.Add(tem[i], values[i]);
            }
            tem = Parameter_colName.Split('|'); //获得每一列的数据
            values = Parameter_Value.Split('|');
            for (int i = 0; i < tem.Length; i++)
            {
                if (CS_keys.ContainsKey(tem[i]))
                    CS_keys.Remove(tem[i]);
                CS_keys.Add(tem[i], values[i]);
            }

            //meterDLTData.DataFlag = CS_keys["标识编码"]; //645

            if (meter.MD_ProtocolName.IndexOf("645") != -1)
            {
                meterDLTData.DataFlag = CS_keys["标识编码"]; //645
            }
            else
            {
                meterDLTData.DataFlag = CS_keys["标识编码698"]; //698
            }
            meterDLTData.DataFormat = CS_keys["数据格式"];
            meterDLTData.DataLen = CS_keys["长度"];
            meterDLTData.FlagMsg = CS_keys["数据项名称"];
            meterDLTData.StandardValue = CS_keys["写入内容"];

            meterDLTData.Value = keys["检定信息"];

            if (meter.MeterDLTDatas.ContainsKey(KeyNo))
                meter.MeterDLTDatas.Remove(KeyNo);
            meter.MeterDLTDatas.Add(KeyNo, meterDLTData);
        }
        #endregion

        #endregion
    }
}
