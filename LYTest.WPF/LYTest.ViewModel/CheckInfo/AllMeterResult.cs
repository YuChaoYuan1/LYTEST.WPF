using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.Utility.Log;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Model;
using LYTest.ViewModel.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.ViewModel.CheckInfo
{
    /// <summary>
    /// 按表位显示的详细结论
    /// </summary>
    public class AllMeterResult : ViewModelBase
    {
        private SelectCollection<OneMeterResult> resultCollection = new SelectCollection<OneMeterResult>();
        /// <summary>
        /// 所有检定结论
        /// </summary>
        public SelectCollection<OneMeterResult> ResultCollection
        {
            get { return resultCollection; }
            set { SetPropertyValue(value, ref resultCollection, "ResultCollection"); }
        }
        /// <summary>
        /// 加载临时库所有表信息
        /// </summary>
        public AllMeterResult()
        {
            ResultCollection.ItemsSource.Clear();
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                string meterPk = EquipmentData.MeterGroupInfo.Meters[i].GetProperty("METER_ID") as string;
                ResultCollection.ItemsSource.Add(new OneMeterResult(meterPk, true));
            }
            CheckCurrentSchemeResult();
        }
        /// <summary>
        /// 从正式库加载表信息
        /// </summary>
        /// <param name="meters"></param>
        public AllMeterResult(IEnumerable<DynamicViewModel> meters)
        {
            LoadMeters(meters);
        }

        public void LoadMeters(IEnumerable<DynamicViewModel> meters)
        {
            ResultCollection.ItemsSource.Clear();
            if (meters == null)
            {
                return;
            }
            for (int i = 0; i < meters.Count(); i++)
            {
                string meterPk = meters.ElementAt(i).GetProperty("METER_ID") as string;
                ResultCollection.ItemsSource.Add(new OneMeterResult(meterPk, false));
            }
        }
        public void ChangeSaveInfo(int TestEffectiveTime, float Temperature, float Humidity)
        {

            //温度,湿度,批准人,检验员,核验员,有效期,检定日期
            string[] arrayField = new string[]
                {
                    "MD_TEMPERATURE",
                    "MD_HUMIDITY",
                    "MD_SUPERVISOR",
                    "MD_TEST_PERSON",
                    "MD_AUDIT_PERSON",
                    "MD_VALID_DATE",
                    "MD_TEST_DATE"
                };

            int intTemp = TestEffectiveTime;

            //有效期
            string stringValidDate = DateTime.Now.AddMonths(intTemp).AddDays(-1).ToString();
            string strTestDate = DateTime.Now.ToString();
            string[] arrayValue = new string[]
                {
                    Temperature.ToString(),//textBoxTemperature.Text
                    Humidity.ToString(),//textBoxHumidy.Text
                    "",//comboBoxBoss.SelectedItem as string
                    EquipmentData.LastCheckInfo.TestPerson,//comboBoxTester.Text
                    EquipmentData.LastCheckInfo.AuditPerson,//comboBoxAuditor.SelectedItem as string
                    stringValidDate,
                    strTestDate,
                };

            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;

            for (int i = 0; i < EquipmentData.MeterGroupInfo.Meters.Count; i++)
            {
                if (yaojianTemp[i])
                {
                    for (int j = 0; j < arrayField.Length; j++)
                    {
                        EquipmentData.MeterGroupInfo.Meters[i].SetProperty(arrayField[j], arrayValue[j]);
                    }
                }
            }
        }
        /// <summary>
        /// 保存所有的信息
        /// </summary>
        public void SaveAllInfo()
        {
            string msg = string.Empty;
            //修复了审核存盘会存储其他数据的问题，导致结论出错。目前只存储当前方案的数据 2022/09/13/1638 ZXG
            #region 更新表信息
            bool[] yaojianTemp = EquipmentData.MeterGroupInfo.YaoJian;
            List<DynamicModel> meterModels = new List<DynamicModel>();
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                if (yaojianTemp[i])
                {
                    meterModels.Add(EquipmentData.MeterGroupInfo.Meters[i].GetDataSource());
                }
            }
            if (meterModels.Count <= 0)
            {
                msg = "保存失败！未勾选被检表。";
                LogManager.AddMessage(msg, EnumLogSource.用户操作日志, EnumLevel.Warning);
                MessageBox.Show(msg, "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int updateCount = DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", "METER_ID", meterModels, new List<string>()
            {
                    "MD_TEMPERATURE",   // 温度
                    "MD_HUMIDITY",                                             //湿度
                    "MD_SUPERVISOR",                                         //主管
                    "MD_TEST_PERSON",                                      //检验员
                    "MD_AUDIT_PERSON",                                      //核验员
                    "MD_RESULT",                           //总结论
                    "MD_VALID_DATE",                                         //有效期
                    "MD_TEST_DATE",
            });
            LogManager.AddMessage(string.Format("更新表温湿度,检验员及总结论,共更新{0}条记录", updateCount), EnumLogSource.数据库存取日志, EnumLevel.Information);
            #endregion
            #region 将临时库数据转存到正式库
            List<string> listWhere = new List<string>();
            List<string> listWhereTemp = new List<string>();

            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                if (!yaojianTemp[i]) continue;
                listWhere.Add(string.Format("METER_ID='{0}'", EquipmentData.MeterGroupInfo.Meters[i].GetProperty("METER_ID")));
                listWhereTemp.Add(string.Format("METER_ID='{0}'", EquipmentData.MeterGroupInfo.Meters[i].GetProperty("METER_ID")));
            }
            string wherePk = string.Join(" or ", listWhere);
            string whereFk = string.Join(" or ", listWhereTemp);
            if (string.IsNullOrEmpty(wherePk) || string.IsNullOrEmpty(whereFk))
            {
                msg = "保存失败！SQL语句不合法。";
                LogManager.AddMessage(msg, EnumLogSource.用户操作日志, EnumLevel.Error);
                //MessageBox.Show(msg, "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            List<string> tableNames = DALManager.MeterDbDal.GetTableNames();
            tableNames.Remove("METER_INFO");
            #region 先删除正式库中对应的检定信息
            List<string> deleteSqlList = new List<string>
            {
                $"delete from meter_info where {wherePk}"
            };
            for (int i = 0; i < tableNames.Count; i++)
            {
                if (tableNames[i].Contains("~"))
                    continue;
                deleteSqlList.Add($"delete from {tableNames[i]} where {whereFk}");
            }
            int deleteCount = DALManager.MeterDbDal.ExecuteOperation(deleteSqlList);
            LogManager.AddMessage($"删除正式库中过时数据,共删除{deleteCount}条", EnumLogSource.数据库存取日志, EnumLevel.Information);
            #endregion

            #region 插入临时库中的检定信息
            List<DynamicModel> metersTemp = DALManager.MeterTempDbDal.GetList("T_TMP_METER_INFO", wherePk);
            int insertCount;
            for (int i = 0; i < tableNames.Count; i++)
            {
                if (tableNames[i].Contains("~"))
                {
                    continue;
                }
                List<DynamicModel> modelsResult = DALManager.MeterTempDbDal.GetList("T_TMP_" + tableNames[i], whereFk);

                #region 存盘只存储当前检定方案的数据

                //筛选出当前方案的项目
                List<Schema.SchemaNodeViewModel> schemaNodeViewModel = EquipmentData.Schema.GetTerminalNodes();
                List<string> selectSchemaId = GetCurrentSchemeParNoList();
                string id;
                //将不是当前方案的项目全部删除
                Dictionary<string, string> result = new Dictionary<string, string>();
                List<DynamicModel> modelsResult2 = new List<DynamicModel>();
                for (int j = modelsResult.Count - 1; j >= 0; j--)
                {
                    id = modelsResult[j].GetProperty("MD_PROJECT_NO") as string;
                    if (selectSchemaId.Contains(id))
                    {
                        string meterID = modelsResult[j].GetProperty("METER_ID") as string;
                        string md_result = modelsResult[j].GetProperty("MD_RESULT") as string;
                        if (!result.ContainsKey(meterID)) result.Add(meterID, "合格");

                        if (md_result == "不合格")
                        {
                            result[meterID] = "不合格";
                        }

                        modelsResult2.Add(modelsResult[j]);
                    }
                }
                foreach (var item in result)
                {
                    var r = metersTemp.Find(x => x.GetProperty("METER_ID") as string == item.Key);
                    if (r != null) r.SetProperty("MD_RESULT", item.Value);
                }
                #endregion

                if (modelsResult2.Count > 0)
                {
                    insertCount = DALManager.MeterDbDal.Insert(tableNames[i], modelsResult2);
                    LogManager.AddMessage(string.Format("向结论表:{0}添加检定结论,共添加{1}条记录", tableNames[i], insertCount), EnumLogSource.数据库存取日志, EnumLevel.Information);
                }
            }
            #endregion
            insertCount = DALManager.MeterDbDal.Insert("METER_INFO", metersTemp);
            int qualifiedCount = metersTemp.Count(x => x.GetProperty("MD_RESULT") as string == "合格");
            int unqualifiedCount = metersTemp.Count(x => x.GetProperty("MD_RESULT") as string == "不合格");
            if (insertCount == 0)
            {
                LogManager.AddMessage("向正式库中添加表信息失败", EnumLogSource.数据库存取日志, EnumLevel.Error);
                MessageBox.Show("向正式库中添加表信息失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LogManager.AddMessage(string.Format("向正式库中添加表信息,共添加{0}条记录", insertCount), EnumLogSource.数据库存取日志, EnumLevel.Information);
            LogManager.AddMessage("将检定数据存储到正式数据库成功!", EnumLogSource.数据库存取日志, EnumLevel.Tip);
            //msg = $"合格个数统计：{qualifiedCount}\n" +
            //    $"不合格个数统计：{unqualifiedCount}\n" +
            //    $"温度：{ConfigHelper.Instance.Temperature}℃\n" +
            //    $"湿度：{ConfigHelper.Instance.Humidity}%\n" +
            //    $"检验员：{EquipmentData.LastCheckInfo.TestPerson}\n" +
            //    $"核检员：{EquipmentData.LastCheckInfo.AuditPerson}";
            //MessageBox.Show(msg, "汇总报告", MessageBoxButton.OK, MessageBoxImage.Information);
            #endregion
        }

        /// <summary>
        /// 检查方案结论数据,剔除不是当前方案的数据,并且更新总结论
        /// </summary>
        private void CheckCurrentSchemeResult()
        {
            // ResultCollection.ItemsSource
            //var a = ResultCollection.ItemsSource[0].Categories[0];
            List<string> selectSchemaId = GetCurrentSchemeParNoList();
            //项目号
            for (int i = 0; i < ResultCollection.ItemsSource.Count; i++)
            {
                bool IsOver = false;
                string Result = "合格";
                for (int j = 0; j < ResultCollection.ItemsSource[i].Categories.Count; j++)
                {
                    //AsyncObservableCollection<DynamicViewModel> tempCarte = new AsyncObservableCollection<DynamicViewModel>();
                    var data = ResultCollection.ItemsSource[i].Categories[j];
                    for (int n = 0; n < data.ResultUnits.Count; n++)
                    {
                        if (selectSchemaId.Contains(data.ResultUnits[n].GetProperty("项目号") as string))
                        {
                            //tempCarte.Add(data.ResultUnits[n]);
                            string tmpResult = data.ResultUnits[n].GetProperty("结论") as string;
                            if (tmpResult == "不合格")
                            {
                                Result = "不合格";
                                IsOver = true;
                                break;
                            }
                        }
                    }
                    if (IsOver) break;
                    //data.ResultUnits = tempCarte;
                }
                ResultCollection.ItemsSource[i].MeterInfo.SetProperty("MD_RESULT", Result);
            }
        }

        /// <summary>
        /// 获取这个方案的项目列表
        /// </summary>
        /// <returns></returns>
        private List<string> GetCurrentSchemeParNoList()
        {
            List<Schema.SchemaNodeViewModel> schemaNodeViewModel = EquipmentData.Schema.GetTerminalNodes();
            List<string> selectSchemaId = new List<string>();
            string id;
            for (int j = 0; j < schemaNodeViewModel.Count; j++)
            {
                for (int z = 0; z < schemaNodeViewModel[j].ParaValuesCurrent.Count; z++)
                {
                    id = schemaNodeViewModel[j].ParaValuesCurrent[z].GetProperty("PARA_VALUE_NO") as string;
                    if (id != null && id != "") selectSchemaId.Add(id);
                }
            }
            return selectSchemaId;
        }


        #region 读取清零状态及密码状态
        bool IsReadMeterAddres = false;
        /// <summary>
        /// 读取清零状态及密码状态
        /// </summary>
        public System.Threading.Tasks.Task<string> Read_Meter_ClearAndKey()
        {
            if (IsReadMeterAddres) return null;
            Task<string> task = Task.Run(() => IsReadClearAndKey());
            //return System.Threading.Tasks.Task.Factory.StartNew(() => IsReadClearAndKey());
            return task;
        }

        /// <summary>
        /// 读取清零状态及密码状态
        /// </summary>
        /// <param name="meterInfos"></param>
        public string IsReadClearAndKey()
        {

            DeviceControlS device = new DeviceControlS();
            int index = -1;

            AsyncObservableCollection<DynamicViewModel> meters = EquipmentData.MeterGroupInfo.Meters;
            for (int i = 0; i < meters.Count; i++)
            {
                if (meters[i].GetProperty("MD_CHECKED") as string == "1")
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                LogManager.AddMessage("没有要检的表", EnumLogSource.用户操作日志, EnumLevel.Warning);
                return "没有要检的表";
            }

            LogManager.AddMessage("开始升源", EnumLogSource.用户操作日志, EnumLevel.Information);
            float ub = float.Parse(meters[index].GetProperty("MD_UB") as string);

            device.PowerOn(ub, 0, 0, 0, 0, 0, Cus_PowerYuanJian.A, PowerWay.正向有功, "1.0"); //升个电压
            int wait = ConfigHelper.Instance.Dgn_PowerSourceStableTime;
            if (wait < 10) wait = 10;
            for (int i = 0; i < wait; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }

            LogManager.AddMessage("正在读取电量", EnumLogSource.用户操作日志, EnumLevel.Information);
            EquipmentData.Controller.UpdateMeterProtocol();
            IsReadMeterAddres = true;
            //地址
            //string[] resoult = new string[meters.Count];
            string err1 = "";
            string err2 = "";
            try
            {
                string[] keys645 = new string[meters.Count];
                string[] keys698 = new string[meters.Count];
                keys645.Fill("");
                keys698.Fill("");

                string[] powers = MeterProtocolAdapter.Instance.ReadData("(当前)组合有功总电能");

                if (meters[index].GetProperty("MD_PROTOCOL_NAME").ToString().Contains("DLT645"))
                {
                    LogManager.AddMessage("正在进行【读取状态信息】操作...", EnumLogSource.用户操作日志, EnumLevel.Information);

                    keys645 = MeterProtocolAdapter.Instance.ReadData("密钥状态");
                }
                else if (meters[index].GetProperty("MD_PROTOCOL_NAME").ToString().Contains("DLT698"))
                {
                    List<string> LstOad = new List<string> { "F1000700", "F1000200", "F1000400" };
                    Dictionary<int, object[]> DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormalList);
                    foreach (KeyValuePair<int, object[]> kvp in DicObj)
                    {
                        keys698[kvp.Key] = kvp.Value[4].ToString();
                    }
                }


                for (int i = 0; i < meters.Count; i++)
                {
                    if (meters[i].GetProperty("MD_CHECKED") as string == "1")
                    {
                        if (float.TryParse(powers[i], out float value) && value > 0)
                        {
                            err1 += $"{i + 1},";
                            continue;
                        }

                        // TODO 当前字段可能不正确，需要验证
                        if (!(keys645[i].EndsWith("FFFFFFFF") || keys698[i] == "7FFFFFFFF07F80FF8000000000000000"))
                        {
                            err2 += $"{i + 1},";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage($"读取地址异常{ex}", EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
            finally
            {
                IsReadMeterAddres = false;
                device.PowerOff();//关源
            }

            if (err1.Trim() == "" && err2.Trim() == "")
                return "";
            else
                return $"表位[{err1}]存在电量，表位[{err2}]密钥状态不正确。";
        }


        #endregion


        #region 验证检定数据及方案

        /// <summary>
        /// 读取清零状态及密码状态
        /// </summary>
        public System.Threading.Tasks.Task<string> Check_DataAndScheme()
        {
            if (IsReadMeterAddres) return null;
            Task<string> task = Task.Run(() => CheckDataAndScheme());
            return task;
        }

        private string CheckDataAndScheme()
        {
            //AsyncObservableCollection<DynamicViewModel> scheme = EquipmentData.SchemaModels.Schemas;
            SchemaViewModel scheme = EquipmentData.Schema;
            AsyncObservableCollection<DynamicViewModel> meters = EquipmentData.MeterGroupInfo.Meters;
            AsyncObservableCollection<CheckNodeViewModel> datas = EquipmentData.CheckResults.Categories;

            string err = "";
            int s = 0;
            foreach (DynamicViewModel sc in scheme.ParaValues)
            {
                string paraNo = sc.GetProperty("PARA_KEY") as string;
                string paraName = $"{sc.GetProperty("PARA_NAME")}";
                System.Diagnostics.Trace.WriteLine($"{paraNo} -- {paraName}");

                for (int i = 0; i < meters.Count; i++)
                {
                    DynamicViewModel m = meters[i];
                    if (m.GetProperty("MD_CHECKED").ToString() == "0") continue;

                    bool find = FindResult(i, paraNo, datas);

                    if (!find)
                    {
                        err += $"#{i + 1}[{paraName}]没有数据\n";
                    }
                }
                s++;
            }

            return err;
        }

        private bool FindResult(int bwn, string paraNo, AsyncObservableCollection<CheckNodeViewModel> datas)
        {
            foreach (var d in datas)
            {
                System.Diagnostics.Trace.WriteLine($"{bwn} -- {paraNo} -- {d.ItemKey}");

                if (paraNo == d.ItemKey)
                {
                    if (d.CheckResults.Count > bwn)
                    {
                        if (d.CheckResults[bwn] != null)
                        {
                            string res = d.CheckResults[bwn].GetProperty("结论") as string;
                            if (!string.IsNullOrEmpty(res))
                                return true;
                        }
                    }
                    else
                    {
                        if (d.Children != null && FindResult(bwn, paraNo, d.Children))
                            return true;
                    }
                }
            }
            return false;
        }
        #endregion

    }
}
