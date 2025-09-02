using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.DAL;
using LYTest.ViewModel.CheckInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LYTest.ViewModel.MeterResoultModel
{
    public class MeterDataHelper_XiAn
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
            var s = EquipmentData.MeterGroupInfo;

            GetResoult(meterTemp);
            for (int i = 0; i < EquipmentData.CheckResults.ResultCollection.Count; i++)
            {
                CheckNodeViewModel result = EquipmentData.CheckResults.ResultCollection[i];   //结论
                string TestValueString = EquipmentData.Schema.ParaValues[i].GetProperty("PARA_VALUE") as string;  //检定的参数
                List<string> model = null;
                if (ModelsName != null || ModelsName.ContainsKey(result.ParaNo))
                    model = ModelsName[result.ParaNo];//检定点的格式
                Dictionary<string, string> TestValue = new Dictionary<string, string>();
                if (model != null && TestValueString != null)
                {
                    string[] value = TestValueString.Split('|');//检定用到的参数
                    for (int j = 0; j < model.Count; j++)
                    {
                        if (!TestValue.ContainsKey(model[j]))
                        {
                            TestValue.Add(model[j], value[j]);
                        }
                    }
                }
                switch (result.ParaNo)
                {
                    case ProjectID.载波通信测试:
                        GetCarrierVerify(meterTemp, result, TestValue);
                        break;
                    case ProjectID.载波芯片ID测试:
                        GetHPLCIDAuthen(meterTemp, result, TestValue);
                        break;
                    case ProjectID.时钟示值误差_黑龙江:
                        GetTimeInError(meterTemp, result, TestValue);
                        break;
                    case ProjectID.电能示值组合误差_西安:
                        GetRegister(meterTemp, result, TestValue);
                        break;
                    case ProjectID.电量清零_黑龙江:
                        GetClearEnerfy(meterTemp, result, TestValue);
                        break;
                    case ProjectID.接线检查_黑龙江:
                        GetPreWiring(meterTemp, result, TestValue);
                        break;
                    case ProjectID.基本误差试验_黑龙江://基本误差
                        GetMeterError(meterTemp, result, TestValue);
                        break;
                    case ProjectID.起动试验_黑龙江:
                        GetMeterQdQid(meterTemp, result, TestValue);
                        break;
                    case ProjectID.潜动试验_黑龙江:
                        GetMeterQdQid(meterTemp, result, TestValue);
                        break;
                    case ProjectID.电能表常数试验_黑龙江:
                        GetMeterZZError(meterTemp, result, TestValue);
                        break;
                    case ProjectID.日计时误差_黑龙江:
                        GetClockError(meterTemp, result, TestValue);
                        break;
                    case ProjectID.身份认证:
                    case ProjectID.密钥更新_黑龙江:
                        GetMeterFK(meterTemp, result, TestValue);
                        break;
                    case ProjectID.GPS对时_黑龙江:
                    case ProjectID.需量示值误差:
                        GetDgn(meterTemp, result, TestValue);
                        break;
                    case ProjectID.误差一致性:
                    case ProjectID.误差变差:
                    case ProjectID.负载电流升将变差:
                        GetMeterErrAccord(meterTemp, result, TestValue);
                        break;
                    case ProjectID.通讯协议检查试验_黑龙江:
                        GetMeterDLTData(meterTemp, result, TestValue);
                        break;
                    case ProjectID.参数验证:
                        GetParameterValidation(meterTemp, result, TestValue);
                        break;
                    case ProjectID.通讯协议检查试验2_黑龙江:
                        GetMeterDLTData2(meterTemp, result, TestValue);
                        break;
                    case ProjectID.远程控制:
                    case ProjectID.报警功能:
                    case ProjectID.远程保电:
                    case ProjectID.保电解除:
                        GetMeterFK(meterTemp, result, TestValue);
                        break;
                    case ProjectID.保电控制:
                        GetMeterXiAnEnPower(meterTemp, result, TestValue);
                        break;
                    case ProjectID.拉合闸控制:
                        GetMeterXiAnEnControl(meterTemp, result, TestValue);
                        break;
                    case ProjectID.报警控制:
                        GetMeterXiAnWaarning(meterTemp, result, TestValue);
                        break;
                    case ProjectID.最后核查:
                        GetParameterValidationLastCheck(meterTemp, result, TestValue);
                        break;
                    case ProjectID.GPS预校时:
                        GetDgn(meterTemp, result, TestValue);
                        break;
                    default:
                        break;
                }

            }
            return meterTemp;
        }

        /// <summary>
        /// 最后核查
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetParameterValidationLastCheck(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }


                //meterDLTData.DataFlag = TestValue["标识编码"]; //645

                meterDLTData.SetValue = TestValue["写入内容"];
                meterDLTData.GetVavlue = string.IsNullOrEmpty(ResultValue["读取值"]) ? "999" : ResultValue["读取值"];
                meterDLTData.Name = ResultValue["当前项目"];
                meterDLTData.ChildredItemID = TestValue["子项编号"];
                meterDLTData.ITEM_ID = ResultValue["项目编号"];
                meterDLTData.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterDLTData.Result = "不合格";
                    #region ADD WKW 20220617
                    //如果参数验证不合格，就重置起动试验的结论为不合格
                    var IsKeyReuslt = meter[i].MeterQdQids.Keys.Where(x => x.Contains(ProjectID.起动试验)).ToList();
                    if (IsKeyReuslt.Count == 0 || IsKeyReuslt == null)
                    {
                        continue;
                    }
                    MeterQdQid meterQd = meter[i].MeterQdQids[IsKeyReuslt[0]];
                    meterQd.Result = "不合格";
                    #endregion

                }
                else
                {
                    meterDLTData.Result = "合格";
                }
                if (meter[i].MeterDLTDatas.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDLTDatas.Remove(CheckResult.ItemKey);
                meter[i].MeterDLTDatas.Add(CheckResult.ItemKey, meterDLTData);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterDLTData.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        /// 报警控制
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetMeterXiAnWaarning(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode==null)
                {
                    continue;
                }
                MeterFKWarning meterFKWarning = new MeterFKWarning();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterFKWarning.WarningOnSata = ResultValue["远程报警状态字3"];
                meterFKWarning.WarningOffSata = ResultValue["解除报警状态字3"];

                meterFKWarning.ITEM_ID = ResultValue["项目编号"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterFKWarning.Result = "不合格";
                }
                else
                {

                    meterFKWarning.Result = "合格";
                }
                if (meter[i].MeterFKWarning.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterFKWarning.Remove(CheckResult.ItemKey);
                meter[i].MeterFKWarning.Add(CheckResult.ItemKey, meterFKWarning);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterFKWarning.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        /// 拉合闸控制
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetMeterXiAnEnControl(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null)
                {
                    continue;
                }
                MeterEncryptionControl meterEncryption = new MeterEncryptionControl();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterEncryption.SwitchOffState = ResultValue["拉闸状态字3"];
                meterEncryption.SwitchOnState = ResultValue["合闸状态字3"];

                meterEncryption.ITEM_ID = ResultValue["项目编号"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterEncryption.Result = "不合格";
                }
                else
                {

                    meterEncryption.Result = "合格";
                }
                if (meter[i].MeterEncryptionControl.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterEncryptionControl.Remove(CheckResult.ItemKey);
                meter[i].MeterEncryptionControl.Add(CheckResult.ItemKey, meterEncryption);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterEncryption.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        /// 保电控制
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetMeterXiAnEnPower(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null&& !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterFKEnPower meterEncryption = new MeterFKEnPower();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterEncryption.EnPowerOnState = ResultValue["保电后状态"];
                meterEncryption.EnPowerOffState = ResultValue["解除保电后状态"];

                meterEncryption.ITEM_ID = ResultValue["项目编号"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterEncryption.Result = "不合格";
                }
                else
                {

                    meterEncryption.Result = "合格";
                }
                if (meter[i].MeterFKEnPower.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterFKEnPower.Remove(CheckResult.ItemKey);
                meter[i].MeterFKEnPower.Add(CheckResult.ItemKey, meterEncryption);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterEncryption.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///芯片ID认证
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetHPLCIDAuthen(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterHPLCIDAuthen meterHPLCIDAuthen = new MeterHPLCIDAuthen();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }
                meterHPLCIDAuthen.ChipID = ResultValue["芯片ID号"];
                meterHPLCIDAuthen.ITEM_ID = ResultValue["项目编号"];

                //meterHPLCIDAuthen.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterHPLCIDAuthen.Result = "不合格";
                }
                else
                {
                    meterHPLCIDAuthen.Result = "合格";
                }
                if (meter[i].MeterHPLCIDAuthen.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterHPLCIDAuthen.Remove(CheckResult.ItemKey);
                meter[i].MeterHPLCIDAuthen.Add(CheckResult.ItemKey, meterHPLCIDAuthen);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterHPLCIDAuthen.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }



        /// <summary>
        ///载波通讯试验
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>
        private static void GetCarrierVerify(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterCommunicationHPLC meterCommunicationHPLC = new MeterCommunicationHPLC();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }
                meterCommunicationHPLC.Electricity = ResultValue["读取值"];
                meterCommunicationHPLC.ITEM_ID = ResultValue["项目编号"];

                //meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterCommunicationHPLC.Result = "不合格";
                }
                else
                {
                    meterCommunicationHPLC.Result = "合格";
                }
                if (meter[i].MeterCommunicationHPLC.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterCommunicationHPLC.Remove(CheckResult.ItemKey);
                meter[i].MeterCommunicationHPLC.Add(CheckResult.ItemKey, meterCommunicationHPLC);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterCommunicationHPLC.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }


        /// <summary>
        ///参数验证
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="colunmName"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="KeyNo"></param>            +
        private static void GetParameterValidation(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {
            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名
            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }


                //meterDLTData.DataFlag = TestValue["标识编码"]; //645

                meterDLTData.SetValue = TestValue["写入内容"];
                meterDLTData.GetVavlue = string.IsNullOrEmpty(ResultValue["读取值"]) ? "999" : ResultValue["读取值"];
                meterDLTData.Name = ResultValue["当前项目"];
                meterDLTData.ChildredItemID = TestValue["子项编号"];
                meterDLTData.ITEM_ID = ResultValue["项目编号"];
                meterDLTData.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterDLTData.Result = "不合格";
                    #region ADD WKW 20220617
                    //如果参数验证不合格，就重置起动试验的结论为不合格
                    var IsKeyReuslt = meter[i].MeterQdQids.Keys.Where(x => x.Contains(ProjectID.起动试验)).ToList();
                    if (IsKeyReuslt.Count == 0 || IsKeyReuslt == null)
                    {
                        continue;
                    }
                    MeterQdQid meterQd = meter[i].MeterQdQids[IsKeyReuslt[0]];
                    meterQd.Result = "不合格";
                    #endregion

                }
                else
                {
                    meterDLTData.Result = "合格";
                }
                if (meter[i].MeterDLTDatas.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDLTDatas.Remove(CheckResult.ItemKey);
                meter[i].MeterDLTDatas.Add(CheckResult.ItemKey, meterDLTData);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterDLTData.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }


        /// <summary>
        /// 时钟示值误差
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetTimeInError(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名


            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterClockError meterClockError = new MeterClockError();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                #region ADD WKW 20220525

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterClockError.SystemCurrentDate = ResultValue["表当前日期"];
                meterClockError.MeterCurrentReadDate = ResultValue["系统当前日期"];
                meterClockError.Date = ResultValue["日期差"];
                meterClockError.MeterCurrentReadTime = ResultValue["表当前时间"];
                meterClockError.SystemCurrentTime = ResultValue["系统当前时间"];
                meterClockError.TimeDifference = ResultValue["时间差"];
                meterClockError.CheckedMeterTime = ResultValue["校时后电表时间"];
                meterClockError.CheckedSystemCurrentTime = ResultValue["校时后系统当前时间"];
                meterClockError.CheckedTimeDifference = ResultValue["校时后时间差"];
                meterClockError.ITEM_ID = ResultValue["项目编号"];
                #endregion


                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterClockError.Result = "不合格";
                }
                else
                {
                    meterClockError.Result = "合格";
                }
                if (meter[i].MeterClockError.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterClockError.Remove(CheckResult.ItemKey);
                meter[i].MeterClockError.Add(CheckResult.ItemKey, meterClockError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterClockError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }


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
                TestMeterInfo meterTemp = new TestMeterInfo();

                string str = "";
                #region 基本信息
                //meterTemp.Meter_ID = meter.Meter_ID;
                //meter.BenthNo = model.GetProperty("MD_DEVICE_ID").ToString();  //台体编号 
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
                meterTemp.YaoJianYn = model.GetProperty("MD_CHECKED").ToString() == "1" ? true : false;
                meterTemp.Result = model.GetProperty("MD_RESULT").ToString();  //表结论
                meterTemp.Humidity = model.GetProperty("MD_TEMPERATURE").ToString();  //湿度
                meterTemp.Temperature = model.GetProperty("MD_HUMIDITY").ToString();  //温度
                meterTemp.Checker1 = model.GetProperty("MD_TEST_PERSON").ToString();  //检验员
                meterTemp.Checker2 = model.GetProperty("MD_AUDIT_PERSON").ToString();  //核验员
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
        //private static void GetMeterError(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        //{

        //    List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名

        //    for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
        //    {
        //        if (meter[i] == null || !meter[i].YaoJianYn)
        //        {
        //            continue;
        //        }
        //        MeterError meterError = new MeterError();
        //        Dictionary<string, string> ResultValue = new Dictionary<string, string>();
        //        for (int j = 0; j < ModelName.Count; j++)
        //        {
        //            if (!ResultValue.ContainsKey(ModelName[j]))
        //            {
        //                string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
        //                ResultValue.Add(ModelName[j], str);
        //            }
        //        }
        //        meterError.ITEM_ID = TestValue["子项目ID"];

        //        meterError.GLYS = ResultValue["功率因数"];
        //        meterError.GLFX = ResultValue["功率方向"];
        //        meterError.PrjNo = CheckResult.ItemKey;
        //        meterError.YJ = ResultValue["功率元件"];
        //        meterError.Result = ResultValue["结论"];
        //        //string[] results = result.Split('|');
        //        //meterError.Result = results[0];
        //        //meterError.AVR_DIS_REASON = results[1];
        //        meterError.IbX = ResultValue["电流倍数"];
        //        if (meterError.IbX == "Ib")
        //        {
        //            meterError.IbX = "1.0Ib";
        //        }
        //        meterError.WCData = ResultValue["误差1"] + "|" + ResultValue["误差2"];
        //        meterError.WCHZ = ResultValue["化整值"];
        //        meterError.WCValue = ResultValue["平均值"];
        //        meterError.Limit = ResultValue["误差上限"] + "|" + ResultValue["误差下限"];
        //        meterError.BPHUpLimit = ResultValue["误差上限"];
        //        meterError.BPHDownLimit = ResultValue["误差下限"];
        //        meterError.Circle = ResultValue["误差圈数"];
        //        if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "") //没有结论的就跳过
        //        {
        //            meterError.Result = "不合格";
        //        }
        //        if (meter[i].MeterErrors.ContainsKey(CheckResult.ItemKey))
        //            meter[i].MeterErrors.Remove(CheckResult.ItemKey);
        //        meter[i].MeterErrors.Add(CheckResult.ItemKey, meterError);

        //        //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
        //        if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result=="合格")
        //        {
        //            meter[i].MeterResults[CheckResult.ParaNo].Result = Core.Helper.ConstHelper.不合格;
        //        }
        //    }
        //}

        private static void GetMeterError(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名


            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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
                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                //误差类型 | 功率方向 | 误差下限 | 误差上限 | 校验圈数(Ib) | 误差限比(%) | 误差个数 | 小数位数 | 电流倍数 | 功率因数 | 误差1|误差2|平均值|化整值 | 项目编号

                meterError.WCType = ResultValue["误差类型"].ToString().Equals("基本误差") ? 0 : 1;
                meterError.GLFX = ResultValue["功率方向"];

                meterError.BPHDownLimit = ResultValue["误差下限"];
                meterError.BPHUpLimit = ResultValue["误差上限"];
                meterError.Circle = ResultValue["校验圈数(Ib)"];
                meterError.WCRatio = ResultValue["误差限比(%)"];
                meterError.WCNumber = ResultValue["误差个数"];
                meterError.DecimalPlace = ResultValue["小数位数"];



                meterError.IbX = ResultValue["电流倍数"];
                if (meterError.IbX == "Ib")
                {
                    meterError.IbX = "1.0Ib";
                }
                meterError.GLYS = ResultValue["功率因数"];

                //meterError.PHVag = ResultValue["平均值"];
                //meterError.WCHZ = ResultValue["化整值"];
                meterError.WCData = ResultValue["误差1"] + "|" + ResultValue["误差2"] + "|" + ResultValue["平均值"] + "|" + ResultValue["化整值"];


                meterError.ITEM_ID = ResultValue["项目编号"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                if (meter[i].MeterErrors.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterErrors.Remove(CheckResult.ItemKey);
                meter[i].MeterErrors.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }


        }

        /// <summary>
        /// 电能示值误差
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetRegister(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名


            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterEnergyError meterEnergyError = new MeterEnergyError();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterEnergyError.ITEM_ID = ResultValue["项目编号"];
                meterEnergyError.PowerBeforeTotal = ResultValue["走字前总"] == null ? "0.00" : ResultValue["走字前总"];
                meterEnergyError.PowerOutBefore = ResultValue["走字前尖峰平谷"].Length <= 0 ? "0.00,0.00,0.00,0.00" : ResultValue["走字前尖峰平谷"];
                meterEnergyError.PowerOutAfter = ResultValue["走字后总"].Length <= 0 ? "0.00" : ResultValue["走字后总"];
                meterEnergyError.PowerAfterTotal = ResultValue["走字后尖峰平谷"].Length <= 0 ? "0.00,0.00,0.00,0.00" : ResultValue["走字后尖峰平谷"];
                meterEnergyError.CombinationError = ResultValue["组合误差"].Length <= 0 ? "0.00" : ResultValue["组合误差"];
                meterEnergyError.WCRate = ResultValue["误差限"].Length <= 0 ? "0.00" : ResultValue["误差限"];
                meterEnergyError.Rate = ResultValue["费率数"].Length <= 0 ? "0" : ResultValue["费率数"];
                meterEnergyError.TrialTime = ResultValue["试验时间"].Length <= 0 ? "0" : ResultValue["试验时间"];
                meterEnergyError.EnergyIncrement = ResultValue["电能增量"].Length <= 0 ? "0.00" : ResultValue["电能增量"];


                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterEnergyError.Result = "不合格";
                }
                else
                {
                    meterEnergyError.Result = "合格";
                }
                if (meter[i].MeterEnergyError.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterEnergyError.Remove(CheckResult.ItemKey);
                meter[i].MeterEnergyError.Add(CheckResult.ItemKey, meterEnergyError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterEnergyError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }


        }


        /// <summary>
        /// 电量清零
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetClearEnerfy(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名


            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterClearEnerfy meterClearEnerfy = new MeterClearEnerfy();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterClearEnerfy.ClearEnerfyBefore = ResultValue["清零前电量"];
                meterClearEnerfy.ClearEnerfyAfter = ResultValue["清零后电量"];

                meterClearEnerfy.ITEM_ID = ResultValue["项目编号"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterClearEnerfy.Result = "不合格";
                }
                else
                {

                    meterClearEnerfy.Result = "合格";
                }
                if (meter[i].MeterClearEnerfy.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterClearEnerfy.Remove(CheckResult.ItemKey);
                meter[i].MeterClearEnerfy.Add(CheckResult.ItemKey, meterClearEnerfy);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterClearEnerfy.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }


        }

        /// <summary>
        /// 接线检查
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="CheckResult"></param>
        /// <param name="TestValue"></param>
        private static void GetPreWiring(TestMeterInfo[] meter, CheckNodeViewModel CheckResult, Dictionary<string, string> TestValue)
        {

            List<string> ModelName = CheckResult.DisplayModel.GetDisplayNames(); //结论的列名


            for (int i = 0; i < CheckResult.CheckResults.Count; i++) //循环所有的表，获得这个表的检定结论
            {
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
                {
                    continue;
                }
                MeterPreWiring meterPreWiring = new MeterPreWiring();
                Dictionary<string, string> ResultValue = new Dictionary<string, string>();
                for (int j = 0; j < ModelName.Count; j++)
                {
                    if (!ResultValue.ContainsKey(ModelName[j]))
                    {
                        string str = CheckResult.CheckResults[i].GetProperty(ModelName[j]) as string;
                        ResultValue.Add(ModelName[j], str);
                    }
                }

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterPreWiring.BarCode = ResultValue["表条码号"];
                meterPreWiring.Address = ResultValue["表地址"];
                meterPreWiring.ClockPulse = ResultValue["时钟脉冲"];
                meterPreWiring.EnergyPulses = ResultValue["电能脉冲"];

                //meterPreWiring.AVoltage = (float.Parse(ResultValue["电压"])).ToString();
                //meterPreWiring.ACurrent = (float.Parse(ResultValue["电流"])).ToString();
                meterPreWiring.AVoltage = ResultValue["电压"] == "" ? 0.ToString("F2") : (float.Parse(ResultValue["电压"])).ToString();
                meterPreWiring.ACurrent = ResultValue["电流"] == "" ? 0.ToString("F2") : (float.Parse(ResultValue["电流"])).ToString();
                //meterPreWiring.BatteryVoltage = (float.Parse(ResultValue["电池电压"])).ToString();
                meterPreWiring.BatteryVoltage = ResultValue["电池电压"] == "" ? 0.ToString("F2") : (float.Parse(ResultValue["电池电压"])).ToString();
                if (ResultValue["状态字"].Equals("×"))
                {
                    meterPreWiring.RunStatusWord = "异常";
                }
                else
                {
                    meterPreWiring.RunStatusWord = "正常";
                }



                meterPreWiring.ITEM_ID = ResultValue["项目编号"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterPreWiring.Result = "不合格";
                }
                else
                {
                    meterPreWiring.Result = "合格";
                }
                if (meter[i].MeterPreWiring.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterPreWiring.Remove(CheckResult.ItemKey);
                meter[i].MeterPreWiring.Add(CheckResult.ItemKey, meterPreWiring);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {

                    continue;
                }
                if (meterPreWiring.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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
                // 试验电量 | 被检表脉冲个数 | 起始电量 | 终止电量 | 电能表累计电量 | 误差值 | 误差限 | 标准表电量 | 项目编号
                //meterError.Result = ResultValue["结论"];
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
                //meterError.PowerSumEnd = ResultValue["起码"];
                //meterError.PowerSumStart = ResultValue["止码"];
                //meterError.WarkPower = "0";
                //if (meterError.PowerSumEnd != "" && meterError.PowerSumStart != "")
                //{
                //    meterError.WarkPower = (float.Parse(meterError.PowerSumStart) - float.Parse(meterError.PowerSumEnd)).ToString("f5");
                //}
                //meterError.PowerError = ResultValue["表码差"];
                //meterError.STMEnergy = ResultValue["标准表脉冲"];  // ResultValue["标准表脉冲"]/d_meterInfo.GetBcs()[0])
                //meterError.STMEnergy = ResultValue["标准表脉冲"];  // ResultValue["标准表脉冲"]/d_meterInfo.GetBcs()[0])
                //meterError.Pules = ResultValue["表脉冲"];

                #region Update WKW 20220530
                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                meterError.ZzCurrent = ResultValue["试验电量"];
                meterError.Pules = ResultValue["被检表脉冲个数"];
                meterError.PowerStart = ResultValue["起始电量"] == "" ? 0 : decimal.Parse(ResultValue["起始电量"]);
                meterError.PowerEnd = ResultValue["终止电量"] == "" ? 0 : decimal.Parse(ResultValue["终止电量"]);
                meterError.MeterEnergy = ResultValue["电能表累计电量"] == "" ? 0.ToString("F2") : ResultValue["电能表累计电量"];
                meterError.ErrorValue = ResultValue["误差值"];
                meterError.ErrorRate = ResultValue["误差限"].Split(',')[0];
                meterError.STMEnergy = ResultValue["标准表电量"] == "" ? 0.ToString("F2") : ResultValue["标准表电量"];

                meterError.ITEM_ID = ResultValue["项目编号"];
                #endregion

                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                if (meter[i].MeterZZErrors.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterZZErrors.Remove(CheckResult.ItemKey);
                meter[i].MeterZZErrors.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }
                meterError.ActiveTime = ResultValue["实际运行时间"].Trim('分');
                meterError.PowerWay = ResultValue["功率方向"];
                meterError.Voltage = ResultValue["试验电压"];
                meterError.TimeEnd = ResultValue["结束时间"];
                meterError.TimeStart = ResultValue["开始时间"];
                meterError.TimeStart = ResultValue["开始时间"];
                meterError.StandartTime = ResultValue["标准试验时间"];
                meterError.Current = ResultValue["试验电流"];
                meterError.Pulse = ResultValue["脉冲数"];
                meterError.ITEM_ID = ResultValue["项目编号"];

                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                if (meter[i].MeterQdQids.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterQdQids.Remove(CheckResult.ItemKey);
                meter[i].MeterQdQids.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterError.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        ///日记时试验
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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
                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }

                // 误差1 | 误差2 | 误差3 | 误差4 | 误差5 | 平均值 | 化整值 | 误差限(%) | 结论 | 项目编号 | 标准表时钟频率 | 被检时钟频率

                meterError.ITEM_ID = ResultValue["项目编号"];
                //meterError.WCData = ResultValue["误差1"] +"|" +ResultValue["误差2"]+ ResultValue["误差3"] + "|" + ResultValue["误差4"]+ ResultValue["误差5"] + "|" + ResultValue["平均值"]
                //    + "|" + ResultValue["化整值"] + "|" + ResultValue["误差限(%)"];
                meterError.WC1 =float.Parse(ResultValue["误差1"]).ToString("F3");
                meterError.WC2 = float.Parse(ResultValue["误差2"]).ToString("F3");
                meterError.WC3 = float.Parse(ResultValue["误差3"]).ToString("F3");
                meterError.WC4 = float.Parse(ResultValue["误差4"]).ToString("F3");
                meterError.WC5 = float.Parse(ResultValue["误差5"]).ToString("F3");
                meterError.AvgValue = ResultValue["平均值"];
                meterError.HzValue = float.Parse(ResultValue["化整值"]).ToString("F2");
                meterError.WCRate = ResultValue["误差限(s/d)"];





                meterError.Value = string.Join("|", list);
                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                if (meter[i].MeterDgns.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDgns.Remove(CheckResult.ItemKey);
                meter[i].MeterDgns.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
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
                    case ProjectID.保电解除:
                    case ProjectID.密钥更新_黑龙江:
                        meterError.Name = ResultValue["当前项目"];
                        meterError.Data = ResultValue["检定信息"];
                        meterError.MeterAddress = ResultValue["表地址"];
                        meterError.Electricity = ResultValue["电量"];
                        meterError.PrivateKey = ResultValue["私钥密文"];
                        meterError.PasswordState = ResultValue["密钥下装状态"];
                        meterError.PrivateKeyState = ResultValue["私钥认证状态"];
                        meterError.ITEM_ID = ResultValue["项目编号"];
                        break;
                    default:  //费控通用
                        meterError.Name = ResultValue["当前项目"];
                        meterError.Data = ResultValue["检定信息"];
                        break;
                }

                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                if (meter[i].MeterCostControls.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterCostControls.Remove(CheckResult.ItemKey);
                meter[i].MeterCostControls.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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

                #region ADD WKW 20220525

                meterError.MeterCurrentReadDate = ResultValue["表当前日期"];
                meterError.SystemCurrentDate = ResultValue["系统当前日期"];
                meterError.Date = ResultValue["日期差"];
                meterError.MeterCurrentReadTime = ResultValue["表当前时间"];
                meterError.SystemCurrentTime = ResultValue["系统当前时间"];
                meterError.BeforeDateDifference = ResultValue["校时前时间差"];
                meterError.AfterDateDifference = ResultValue["校时后时间差"];
                meterError.ErrorRate = ResultValue["误差限"];
                meterError.ITEM_ID = ResultValue["项目编号"];
                #endregion



                meterError.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                if (meter[i].MeterDgns.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDgns.Remove(CheckResult.ItemKey);
                meter[i].MeterDgns.Add(CheckResult.ItemKey, meterError);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterError.Result = "不合格";
                }
                else
                {
                    meterError.Result = "合格";
                }
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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

                if (string.IsNullOrEmpty(ResultValue["项目编号"]))
                {
                    continue;
                }
                //meterDLTData.DataFlag = TestValue["标识编码"]; //645
                meterDLTData.DataFormat = TestValue["数据格式"];
                meterDLTData.DataLen = TestValue["长度"];
                meterDLTData.FlagMsg = TestValue["数据项名称"];
                meterDLTData.SetValue = TestValue["写入内容"];
                meterDLTData.GetVavlue = ResultValue["读取值"];
                meterDLTData.Name = ResultValue["当前项目"];
                meterDLTData.DataFlag = $"{ResultValue["当前项目"]}通讯成功,数据标识:{TestValue["标识编码"]}";
                meterDLTData.Value = ResultValue["读取值"];
                meterDLTData.ITEM_ID = ResultValue["项目编号"];
                meterDLTData.Result = ResultValue["结论"];
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterDLTData.Result = "不合格";
                }
                else
                {
                    meterDLTData.Result = "合格";
                }
                if (meter[i].MeterDLTDatas.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDLTDatas.Remove(CheckResult.ItemKey);
                meter[i].MeterDLTDatas.Add(CheckResult.ItemKey, meterDLTData);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
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
                if (meter[i].MD_BarCode == null && !meter[i].YaoJianYn)
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
                if (ResultValue["结论"] == null || ResultValue["结论"].Trim() == "" || ResultValue["结论"] == "不合格") //没有结论的就跳过
                {
                    meterDLTData.Result = "不合格";
                }
                else
                {
                    meterDLTData.Result = "合格";
                }
                if (meter[i].MeterDLTDatas.ContainsKey(CheckResult.ItemKey))
                    meter[i].MeterDLTDatas.Remove(CheckResult.ItemKey);
                meter[i].MeterDLTDatas.Add(CheckResult.ItemKey, meterDLTData);
                //如果当前项目不合格，并且总结论是合格，那么就把总结论修改成不合格(总结论指大项目结论)
                if (meter[i].MeterResults.Count == 0)
                {
                    continue;
                }
                if (meterDLTData.Result == "不合格" && meter[i].MeterResults[CheckResult.ParaNo].Result == "合格")
                {
                    meter[i].MeterResults[CheckResult.ParaNo].Result = ConstHelper.不合格;
                }
            }
        }

        #endregion


    }
}
