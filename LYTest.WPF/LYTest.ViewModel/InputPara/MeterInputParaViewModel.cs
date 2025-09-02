using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.MeterProtocol.Struct;
using LYTest.Mis;
using LYTest.Mis.Common;
using LYTest.Mis.MDS;
using LYTest.Utility.Log;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Model;
using LYTest.ViewModel.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;

namespace LYTest.ViewModel.InputPara
{
    /// <summary>
    /// 表信息录入数据模型
    /// </summary>
    public class MeterInputParaViewModel : ViewModelBase
    {
        /// <summary>
        /// 正在下载true
        /// </summary>
        private static bool DownLoading = false;
        ///// <summary>
        ///// 参数录入时的构造函数,从内存中加载表信息,由于是在xaml页面中构造,所有没有参数
        ///// </summary>
        //public MeterInputParaViewModel()
        //{

        //    Initial();
        //    for (int i = 0; i < ParasModel.AllUnits.Count; i++)
        //    {
        //        for (int j = 0; j < Meters.Count; j++)
        //        {
        //            string fieldName = ParasModel.AllUnits[i].FieldName;
        //            object objTemp = EquipmentData.MeterGroupInfo.Meters[j].GetProperty(fieldName);
        //            Meters[j].SetProperty(fieldName, objTemp);
        //        }
        //    }
        //    RefreshFirstMeterInfo();
        //}



        /// <summary>
        /// 程序启动时的构造函数,从数据库加载
        /// </summary>
        /// <param name="isCurrent"></param>
        public MeterInputParaViewModel()
        {
            Initial();
            LoadMetersFromTempDb();
            //如果表位数与当前表位数不符,执行换新表
            if (Meters.Count != EquipmentData.Equipment.MeterCount)
            {
                NewMeters();
            }
        }
        ///// <summary>
        ///// 空方法,用于初始化表信息
        ///// </summary>
        //public void Initialize()
        //{
        //    //EquipmentData.ZaiBoInfo = new CarrierList();
        //    //EquipmentData.ZaiBoInfo.Load();
        //}

        private InputParaViewModel parasModel = new InputParaViewModel();
        /// <summary>
        /// 表信息录入相关的数据模型
        /// </summary>
        public InputParaViewModel ParasModel
        {
            get { return parasModel; }
            set { SetPropertyValue(value, ref parasModel, "ParasModel"); }
        }
        //private AsyncObservableCollection<DynamicViewModel> meters = new AsyncObservableCollection<DynamicViewModel>();
        /// <summary>
        /// 表信息集合
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> Meters { get; } = new AsyncObservableCollection<DynamicViewModel>();
        //{
        //    get { return meters; }
        //    set { SetPropertyValue(value, ref meters); }
        //}

        private DynamicViewModel firstMeter = new DynamicViewModel(0);
        /// <summary>
        /// 表位基本信息
        /// </summary>
        public DynamicViewModel FirstMeter
        {
            get { return firstMeter; }
            set { SetPropertyValue(value, ref firstMeter, "FirstMeter"); }
        }
        /// <summary>
        /// 表位是否要检
        /// </summary>
        /// 
        public bool[] YaoJian
        {
            get
            {
                bool[] arrayTemp = new bool[EquipmentData.Equipment.MeterCount];
                for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
                {
                    arrayTemp[i] = Meters[i].GetProperty("MD_CHECKED") as string == "1";
                }
                return arrayTemp;
            }
        }
        /// <summary>
        /// 首次参数变化
        /// </summary>
        private bool FirstClassChanged;

        public bool[] classChanged = new bool[EquipmentData.Equipment.MeterCount];
        /// <summary>
        /// 等级变化
        /// </summary>
        public bool[] ClassChanged
        {
            get
            {
                return classChanged;
            }
            private set { classChanged = value; }
        }
        private int parameterChanged = 0;
        /// <summary>
        /// 参数变化通知
        /// </summary>
        public int InputParameterChanged
        {
            get { return parameterChanged; }
            set { SetPropertyValue(value, ref parameterChanged, "InputParameterChanged"); }
        }
        private int parameterHandled = 0;
        /// <summary>
        /// 参数变化已处理
        /// </summary>
        public int ParameterHandled
        {
            get { return parameterHandled; }
            set { SetPropertyValue(value, ref parameterHandled, "ParameterHandled"); }
        }


        /// <summary>
        /// 初始化表信息
        /// </summary>
        private void Initial()
        {
            int meterCount = EquipmentData.Equipment.MeterCount;
            #region 赋初值
            for (int i = 0; i < meterCount; i++)
            {
                DynamicViewModel viewModel = null;
                if (i >= Meters.Count)
                {
                    viewModel = new DynamicViewModel(i + 1);
                    Meters.Add(viewModel);
                }
                else
                {
                    viewModel = Meters[i];
                }
                //设置默认值
                for (int j = 0; j < ParasModel.AllUnits.Count; j++)
                {
                    InputParaUnit paraUnit = ParasModel.AllUnits[j];
                    if (paraUnit.FieldName == "MD_DEVICE_ID")
                    {
                        viewModel.SetProperty("MD_DEVICE_ID", EquipmentData.Equipment.ID);
                    }
                    else if (paraUnit.FieldName == "MD_EPITOPE")
                    {
                        viewModel.SetProperty("MD_EPITOPE", i + 1);
                    }
                    else if (paraUnit.FieldName == "DTM_TEST_DATE")
                    {
                        viewModel.SetProperty("DTM_TEST_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else if (paraUnit.FieldName == "MD_OTHER_2") //上传状态，默认未上传
                    {
                        viewModel.SetProperty("MD_OTHER_2", "未上传");
                    }
                    else
                    {
                        if (paraUnit.IsNewValue)
                        {
                            if (!string.IsNullOrEmpty(paraUnit.DefaultValue))
                            {
                                viewModel.SetProperty(paraUnit.FieldName, paraUnit.DefaultValue);
                            }
                            else
                            {
                                viewModel.SetProperty(paraUnit.FieldName, "");
                            }
                        }
                    }
                }
            }
            #endregion

            for (int i = 0; i < Meters.Count; i++)
            {
                Meters[i].PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "MD_CHECKED")
                    {
                        RefreshFirstMeterInfo();
                    }
                };
            }
        }

        /// <summary>
        /// 从临时数据库加载表信息
        /// </summary>
        private void LoadMetersFromTempDb()
        {

            List<DynamicModel> models = DALManager.MeterTempDbDal.GetList("T_TMP_METER_INFO", string.Format("MD_DEVICE_ID='{0}' order by MD_EPITOPE", EquipmentData.Equipment.ID));
            for (int i = 0; i < models.Count; i++)
            {
                object obj = models[i].GetProperty("MD_EPITOPE");
                if (obj is int index)
                {
                    if (index <= Meters.Count && index > 0)
                    {
                        for (int j = 0; j < ParasModel.AllUnits.Count; j++)
                        {
                            InputParaUnit unit = ParasModel.AllUnits[j];
                            if (unit.ValueType == EnumValueType.编码值)
                            {
                                Meters[index - 1].SetProperty(unit.FieldName, CodeDictionary.GetNameLayer2(unit.CodeType, models[i].GetProperty(unit.FieldName) as string));
                            }
                            else
                            {
                                Meters[index - 1].SetProperty(unit.FieldName, models[i].GetProperty(unit.FieldName));
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < Meters.Count; i++)
            {
                string pkObj = Meters[i].GetProperty("METER_ID") as string;
                if (string.IsNullOrEmpty(pkObj) || pkObj.Length < 8)
                {
                    Meters[i].SetProperty("METER_ID", GetUniquenessID8(i + 1).ToString());
                }
            }
            RefreshFirstMeterInfo();
        }

        /// <summary>
        /// 验证数据是否完整
        /// </summary>
        /// <param name="stringError"></param>
        /// <returns></returns>
        public bool CheckInfoCompleted(out string stringError)
        {
            stringError = "";
            if (EquipmentData.SchemaModels.SelectedSchema == null)
            {
                stringError = "检定方案不能为空,请指定当前检定方案!";
                return false;
            }
            bool[] yaojian = YaoJian;
            bool flagHaveYaojian = false;
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i])
                {
                    flagHaveYaojian = true;
                    break;
                }
            }
            if (!flagHaveYaojian)
            {
                stringError = "请至少选择一块要检的表";
                return false;
            }
            for (int j = 0; j < Meters.Count; j++)
            {
                if (!yaojian[j])
                {
                    continue;
                }
                for (int i = 0; i < ParasModel.AllUnits.Count; i++)
                {
                    if (ParasModel.AllUnits[i].IsDisplayMember && ParasModel.AllUnits[i].IsNecessary)
                    {
                        if (Meters[j].GetProperty(ParasModel.AllUnits[i].FieldName) == null || string.IsNullOrEmpty(Meters[j].GetProperty(ParasModel.AllUnits[i].FieldName).ToString()))
                        {
                            stringError = string.Format("表位{0}缺少信息: {1}", j + 1, ParasModel.AllUnits[i].DisplayName);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 验证条码是否包含表地址
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public List<int> CheckAddressCompleted()
        {
            List<int> list = new List<int>();

            bool[] yaojian = YaoJian;

            for (int j = 0; j < Meters.Count; j++)
            {
                Meters[j].SetProperty("IS_ERR", false);
                if (!yaojian[j]) continue;

                if (Meters[j].GetProperty("MD_BAR_CODE") == null) continue;
                if (Meters[j].GetProperty("MD_POSTAL_ADDRESS") == null) continue;
                string barcode = Meters[j].GetProperty("MD_BAR_CODE").ToString();
                string address = Meters[j].GetProperty("MD_POSTAL_ADDRESS").ToString();

                if (string.IsNullOrEmpty(barcode)) continue;
                if (string.IsNullOrEmpty(address)) continue;  // 通信地址

                if (!barcode.Contains(address))
                {
                    list.Add(j);
                    Meters[j].SetProperty("IS_ERR", true);
                }
            }

            return list;
        }


        public DynamicViewModel GetFirstMeter()
        {
            bool[] yaojian = YaoJian;
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i])
                {
                    return Meters[i];
                }
            }
            return new DynamicViewModel(0);
        }
        /// <summary>
        /// 更新第一块要检表信息
        /// </summary>
        private void RefreshFirstMeterInfo()
        {
            bool[] yaojian = YaoJian;
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i])
                {
                    FirstMeter = Meters[i];
                    break;
                }
            }
        }
        /// <summary>
        /// 保存表信息
        /// </summary>
        private void SaveMeterInfo()
        {
            #region 转换显示数据为数据库数据
            List<DynamicModel> models = new List<DynamicModel>();
            for (int i = 0; i < Meters.Count; i++)
            {
                DynamicModel modelTemp = new DynamicModel();

                for (int j = 0; j < ParasModel.AllUnits.Count; j++)
                {
                    InputParaUnit paraUnitTemp = ParasModel.AllUnits[j];
                    if (paraUnitTemp.ValueType == EnumValueType.编码值)
                    {
                        modelTemp.SetProperty(paraUnitTemp.FieldName, CodeDictionary.GetValueLayer2(paraUnitTemp.CodeType, Meters[i].GetProperty(paraUnitTemp.FieldName) as string));
                    }
                    else
                    {
                        modelTemp.SetProperty(paraUnitTemp.FieldName, Meters[i].GetProperty(paraUnitTemp.FieldName));
                    }
                }
                modelTemp.SetProperty("MD_SCHEME_ID", EquipmentData.Schema.SchemaId);  //方案编号
                modelTemp.SetProperty("MD_TEMPERATURE", ConfigHelper.Instance.Temperature);  //温度
                modelTemp.SetProperty("MD_HUMIDITY", ConfigHelper.Instance.Humidity);  //湿度
                //modelTemp.SetProperty("MD_SUPERVISOR", "");  //主管
                //if (GetMeterInfo(i, "MD_AUDIT_PERSON").Trim()=="")
                //{
                //    modelTemp.SetProperty("MD_AUDIT_PERSON", EquipmentData.LastCheckInfo.AuditPerson);  //核验员
                //}
                //if (GetMeterInfo(i, "MD_AUDIT_PERSON").Trim() == "")
                //{
                //    modelTemp.SetProperty("MD_TEST_PERSON", EquipmentData.LastCheckInfo.TestPerson);  //检验员
                //}
                //if (GetMeterInfo(i, "MD_AUDIT_PERSON").Trim() == "")
                //{
                //    modelTemp.SetProperty("MD_TEST_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));  //检定日期   
                //}
                //if (GetMeterInfo(i, "MD_AUDIT_PERSON").Trim() == "")
                //{
                //   modelTemp.SetProperty("MD_VALID_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));  //计检日
                //}
                //modelTemp.SetProperty("MD_AUDIT_PERSON", EquipmentData.LastCheckInfo.AuditPerson);  //核验员
                //modelTemp.SetProperty("MD_TEST_PERSON", EquipmentData.LastCheckInfo.TestPerson);  //检验员
                //modelTemp.SetProperty("MD_TEST_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));  //检定日期   
                //modelTemp.SetProperty("MD_VALID_DATE", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));  //计检日期

                //TODO保存没有录入的数据
                models.Add(modelTemp);
            }
            #endregion
            #region 获取当前表数量
            List<string> pkList = new List<string>();
            for (int i = 0; i < Meters.Count; i++)
            {
                pkList.Add(string.Format("METER_ID = '{0}'", Meters[i].GetProperty("METER_ID")));
            }
            string pkWhere = string.Join(" or ", pkList);
            #endregion
            int countInDb = DALManager.MeterTempDbDal.GetCount("T_TMP_METER_INFO", pkWhere);
            #region 插入新数据
            if (countInDb != Meters.Count)
            {
                int deleteCount = DALManager.MeterTempDbDal.Delete("T_TMP_METER_INFO", string.Format("MD_DEVICE_ID='{0}'", EquipmentData.Equipment.ID));
                LogManager.AddMessage(string.Format("数据库中表数量:{1}块 与当前录入表数量:{2}块 不一致,删除表信息,共删除{0}条表信息数据.", deleteCount, countInDb, Meters.Count), EnumLogSource.数据库存取日志);
                int insertCount = DALManager.MeterTempDbDal.Insert("T_TMP_METER_INFO", models);
                LogManager.AddMessage(string.Format("更新表信息,共插入{0}条表信息数据.", insertCount), EnumLogSource.数据库存取日志);
                return;
            }
            #endregion
            #region 更新现有信息
            List<string> fieldNames = new List<string>();
            var namesTemp = from item in ParasModel.AllUnits select item.FieldName;
            fieldNames = namesTemp.ToList();
            fieldNames.Remove("METER_ID");
            int updateCount = DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", "METER_ID", models, fieldNames);
            LogManager.AddMessage(string.Format("更新表信息,共更新{0}条表信息数据.", updateCount), EnumLogSource.数据库存取日志);
            #endregion
        }
        /// <summary>
        /// 更新表信息
        /// </summary>
        public void UpdateMeterInfo()
        {
            if (!CheckInfoCompleted(out string err))
            {
                MessageBox.Show(err, "表信息不完整");
                return;
            }

            //add yjt zxg 20220425 新增录入完成提示
            if (ConfigHelper.Instance.VerifyModel != "自动模式")
            {
                //要检定数量-电压-电流-频率-解析方式-互感器-协议类型-等级-常数
                string tips = $"检定数量：{YaoJian.Count(item => item == true)}";
                tips += $"\r\n电       压：{FirstMeter.GetProperty("MD_UB")}";
                tips += $"\r\n电       流：{FirstMeter.GetProperty("MD_UA")}";
                tips += $"\r\n接线方式：{FirstMeter.GetProperty("MD_WIRING_MODE")}";
                //add yjt 20220427 新增n费控类型
                tips += $"\r\n费控类型：{FirstMeter.GetProperty("MD_FKTYPE")}";
                tips += $"\r\n互  感  器： {FirstMeter.GetProperty("MD_CONNECTION_FLAG")}";
                tips += $"\r\n等       级：{FirstMeter.GetProperty("MD_GRADE")}";
                tips += $"\r\n常       数：{FirstMeter.GetProperty("MD_CONSTANT")}";
                //add yjt 20220427 新增检定规程
                tips += $"\r\n检定规程：{FirstMeter.GetProperty("MD_JJGC")}";
                tips += $"\r\n通讯协议：{FirstMeter.GetProperty("MD_PROTOCOL_NAME")}";

                if (MessageBox.Show(tips, "请确定", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            SaveMeterInfo();
            EquipmentData.MeterGroupInfo.LoadMetersFromTempDb(); //重新读取
            EquipmentData.CheckResults.RefreshYaojian();
            RefreshFirstMeterInfo();
            //记录参数变化
            if (!FirstClassChanged)
            {
                for (int i = 0; i < Meters.Count; i++)
                {
                    var meter = VerifyBase.MeterInfo[i];
                    if (meter.MD_Grane == GetMeterInfo(i, "MD_GRADE"))
                    {
                        ClassChanged[i] = false;
                    }
                    else
                    {
                        ClassChanged[i] = true;
                    }
                }
            }

            //将表数据存放到类中
            VerifyBase.MeterInfo = GetVerifyMeterInfo();
            if (!FirstClassChanged && ClassChanged.Any(v => v == true))
            {
                ++InputParameterChanged;

                for (int i = 0; i < Meters.Count; i++)
                {
                    if (ClassChanged[i])
                    {
                        EquipmentData.CheckResults.UpdateRoundingValueAndResult(i, GetMeterInfo(i, "MD_GRADE"), GetMeterInfo(i, "MD_JJGC"));
                    }
                }

                ++ParameterHandled;
            }
            OperateFile.WriteINI("Config", "IsReadMeterInfo", EquipmentData.IsReadMeterInfo.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ConfigTem.ini");

            if (EquipmentData.Controller.Index == -1)
            {
                EquipmentData.LastCheckInfo.SchemaId = EquipmentData.Schema.SchemaId;
                EquipmentData.LastCheckInfo.CheckIndex = 0;
                EquipmentData.Controller.Index = 0;
            }
            UiInterface.CloseWindow("参数录入");
            EquipmentData.NavigateCurrentUi();
            FirstClassChanged = false;
        }
        /// <summary>
        /// 更新表信息
        /// </summary>
        public bool UpdateMeterInfoAuto(out string msg)
        {
            msg = "";

            if (!CheckInfoCompleted(out string errorString))
            {
                msg = $"表信息不完整{errorString}";
                LogManager.AddMessage(msg, EnumLogSource.服务器日志, EnumLevel.Error);
                return false;
            }

            #region ADD WKW 20220626
            //换新表必须重置组网状态
            EquipmentData.IsHplcNet = false;
            #endregion

            SaveMeterInfo();
            EquipmentData.MeterGroupInfo.LoadMetersFromTempDb(); //重新读取
            EquipmentData.CheckResults.RefreshYaojian();
            RefreshFirstMeterInfo();
            //将表数据存放到类中
            VerifyBase.MeterInfo = GetVerifyMeterInfo();


            if (EquipmentData.Controller.Index == -1)
            {
                EquipmentData.LastCheckInfo.SchemaId = EquipmentData.Schema.SchemaId;
                EquipmentData.LastCheckInfo.CheckIndex = 0;
                EquipmentData.Controller.Index = 0;
            }
            UiInterface.CloseWindow("参数录入");
            EquipmentData.NavigateCurrentUi();
            return true;
        }
        /// <summary>
        /// 换新表
        /// </summary>
        public void NewMeters()
        {
            if (MessageBox.Show("确认要更换新表吗？更换新表操作将会删除当前这批表的检定数据和结论,请确认检定数据已经保存、上传，或者不需要当前检定数据了，再执行更换新表操作", "更换新表", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                EquipmentData.Controller.Index = -1;
                Initial();
                for (int i = 0; i < Meters.Count; i++)
                {
                    Meters[i].SetProperty("METER_ID", GetUniquenessID8(i + 1).ToString());
                }
                SaveMeterInfo();
                LoadMetersFromTempDb();

                //清空临时数据库中的结论
                CheckResultBll.Instance.DeleteResultFromTempDb();
                //删除检定结论
                EquipmentData.CheckResults.ClearAllResult();

                EquipmentData.LastCheckInfo.CheckIndex = -1;

                //2022/09/13/1642 ZXG
                EquipmentData.SchemaModels.RefreshCurrrentSchema();

                EquipmentData.NavigateCurrentUi();
                FirstClassChanged = true;
            }
        }

        /// <summary>
        /// 换新表 --不跳转界面--用于自动检定
        /// </summary>
        public void NewMetersAuto()
        {
            EquipmentData.Controller.Index = -1;
            Initial();
            for (int i = 0; i < Meters.Count; i++)
            {
                Meters[i].SetProperty("METER_ID", GetUniquenessID8(i + 1).ToString());
            }
            SaveMeterInfo();
            LoadMetersFromTempDb();

            //清空临时数据库中的结论
            CheckResultBll.Instance.DeleteResultFromTempDb();
            //删除检定结论
            EquipmentData.CheckResults.ClearAllResult();
            EquipmentData.LastCheckInfo.CheckIndex = -1;
        }



        /// <summary>
        /// 转换数据库信息到检定时需要用的表数据类
        /// </summary>
        /// <returns></returns>
        public TestMeterInfo[] GetVerifyMeterInfo()
        {
            //bool[] yaojianTemp = YaoJian;
            TestMeterInfo[] meterInfos = new TestMeterInfo[Meters.Count];
            LYTest.MeterProtocol.App.CarrierInfos = new CarrierWareInfo[Meters.Count];
            for (int i = 0; i < Meters.Count; i++)
            {
                float.TryParse(GetMeterInfo(i, "MD_UB"), out float t2);
                int.TryParse(GetMeterInfo(i, "MD_FREQUENCY"), out int t);
                meterInfos[i] = new TestMeterInfo
                {
                    YaoJianYn = YaoJian[i],
                    Meter_ID = GetMeterInfo(i, "METER_ID"),
                    MD_Epitope = i + 1,//表位
                    MD_UB = t2,//电压
                    MD_UA = GetMeterInfo(i, "MD_UA"),//电流
                                                     //频率
                    MD_Frequency = t,
                    //首检抽检
                    MD_TestModel = GetMeterInfo(i, "MD_TESTMODEL"),
                    //全检抽检
                    MD_TestType = GetMeterInfo(i, "MD_TEST_TYPE"),
                    //  测量方式--单相-三相三线-三相四线
                    MD_WiringMode = GetMeterInfo(i, "MD_WIRING_MODE"),
                    //互感器
                    MD_ConnectionFlag = GetMeterInfo(i, "MD_CONNECTION_FLAG"),
                    //检定规程
                    MD_JJGC = GetMeterInfo(i, "MD_JJGC"),
                    //条形码
                    MD_BarCode = GetMeterInfo(i, "MD_BAR_CODE"),
                    //资产编号
                    MD_AssetNo = GetMeterInfo(i, "MD_ASSET_NO"),
                    //表类型
                    MD_MeterType = GetMeterInfo(i, "MD_METER_TYPE"),
                    //常数
                    MD_Constant = GetMeterInfo(i, "MD_CONSTANT"),
                    //等级
                    MD_Grane = GetMeterInfo(i, "MD_GRADE"),
                    //表型号
                    MD_MeterModel = GetMeterInfo(i, "MD_METER_MODEL"),
                    //通讯协议
                    MD_ProtocolName = GetMeterInfo(i, "MD_PROTOCOL_NAME"),
                    //载波协议
                    MD_CarrName = GetMeterInfo(i, "MD_CARR_NAME"),
                    //通讯地址
                    MD_PostalAddress = GetMeterInfo(i, "MD_POSTAL_ADDRESS"),
                    //制造厂家
                    MD_Factory = GetMeterInfo(i, "MD_FACTORY"),
                    //送检单位
                    MD_Customer = GetMeterInfo(i, "MD_CUSTOMER"),
                    //任务编号
                    MD_TaskNo = GetMeterInfo(i, "MD_TASK_NO"),
                    //出厂编号
                    MD_MadeNo = GetMeterInfo(i, "MD_MADE_NO"),
                    MD_Sort = GetMeterInfo(i, "MD_SORT"),//类别
                                                         //证书编号
                    MD_CertificateNo = GetMeterInfo(i, "MD_CERTIFICATE_NO")
                };


                string str = GetMeterInfo(i, "MD_FKTYPE");

                if (str != null && str == "远程费控")
                    meterInfos[i].FKType = 0;
                else if (str == "本地费控")
                    meterInfos[i].FKType = 1;
                else
                    meterInfos[i].FKType = 2;

                meterInfos[i].Seal1 = GetMeterInfo(i, "MD_SEAL_1");// 铅封1
                meterInfos[i].Seal2 = GetMeterInfo(i, "MD_SEAL_2");// 铅封2
                meterInfos[i].Seal3 = GetMeterInfo(i, "MD_SEAL_3");// 铅封3
                meterInfos[i].Seal4 = GetMeterInfo(i, "MD_SEAL_4");// 铅封4
                meterInfos[i].Seal5 = GetMeterInfo(i, "MD_SEAL_5");// 铅封5
                meterInfos[i].Other1 = GetMeterInfo(i, "MD_OTHER_1");// 备用1
                meterInfos[i].Other2 = GetMeterInfo(i, "MD_OTHER_2");// 备用2

                meterInfos[i].Other3 = GetMeterInfo(i, "MD_OTHER_3");// 备用3   --脉冲类型--蓝牙脉冲光电脉冲

                meterInfos[i].Other4 = GetMeterInfo(i, "MD_OTHER_4");// 备用4
                meterInfos[i].Other5 = GetMeterInfo(i, "MD_OTHER_5");// 备用5


                meterInfos[i].ProtInfo = new ComPortInfo();
                if (i < EquipmentData.DeviceManager.MeterUnits.Count)
                {
                    int.TryParse(EquipmentData.DeviceManager.MeterUnits[i].PortNum, out t);//端口号
                    meterInfos[i].ProtInfo.Port = t;
                    meterInfos[i].ProtInfo.Setting = EquipmentData.DeviceManager.MeterUnits[i].ComParam;
                    meterInfos[i].ProtInfo.IP = EquipmentData.DeviceManager.MeterUnits[i].Address;
                    meterInfos[i].ProtInfo.IsExist = true;
                    meterInfos[i].ProtInfo.OtherParams = string.Empty;
                    int.TryParse(EquipmentData.DeviceManager.MeterUnits[i].StartPort, out t);//端口号
                    meterInfos[i].ProtInfo.StartPort = t;
                    int.TryParse(EquipmentData.DeviceManager.MeterUnits[i].RemotePort, out t);//端口号
                    meterInfos[i].ProtInfo.RemotePort = t;
                    //if (EquipmentData.Equipment.Conn_Type == "RS485")
                    //{
                    //    meterInfos[i].ProtInfo.LinkType = LinkType.COM;
                    //}
                    //else
                    //{
                    //    meterInfos[i].ProtInfo.LinkType = LinkType.UDP ;
                    //}
                    if (EquipmentData.DeviceManager.MeterUnits[i].Conn_Type == CommMode.COM)
                    {
                        meterInfos[i].ProtInfo.LinkType = LinkType.COM;
                    }
                    else
                    {
                        meterInfos[i].ProtInfo.LinkType = LinkType.UDP;
                    }
                    meterInfos[i].ProtInfo.MaxTimePerByte = EquipmentData.DeviceManager.MeterUnits[i].MaxTimePerByte;
                    meterInfos[i].ProtInfo.MaxTimePerFrame = EquipmentData.DeviceManager.MeterUnits[i].MaxTimePerFrame;
                }


            }
            //设置物联网表
            for (int i = 0; i < meterInfos.Length; i++)
            {
                if (meterInfos[i].YaoJianYn)
                {
                    ErrLimitHelper.IBString = meterInfos[i].MD_UA.Replace("（", "(").Replace("）", ")");

                    if (meterInfos[i].MD_Sort == "物联电能表")
                        ConfigHelper.Instance.IsITOMeter = true;
                    else
                        ConfigHelper.Instance.IsITOMeter = false;
                    ConfigHelper.Instance.PulseType = meterInfos[i].Other3;
                    break;
                }
            }


            if (ConfigHelper.Instance.AreaName == "北京" && meterInfos[0].MD_JJGC == "IR46")
            {
                VerifyBase.IsDoubleProtocol = true;  //双协议电表
            }
            else
            {
                VerifyBase.IsDoubleProtocol = false;  //双协议电表
            }
            return meterInfos;
        }

        public void UpdateCheckFlag()
        {
            for (int i = 0; i < Meters.Count; i++)
            {
                VerifyBase.MeterInfo[i].YaoJianYn = YaoJian[i];
            }
        }







        #region 表的唯一编号
        private long longMac = 0;
        /// <summary>
        /// 获取8字节唯一ID：4字节时间戳+3字节主机MAC+1字节自增序列
        /// </summary>
        /// <param name="id">自增序列，只取1字节</param>
        /// <returns>8字节唯一ID</returns>
        private long GetUniquenessID8(int id)
        {
            long lngMac = GetMac(out _);

            string s = string.Format("{0:X8}{1:X6}{2:X2}", GetTimeStamp(), ((int)lngMac) & 0x00FFFFFF, (byte)id);
            long n = Convert.ToInt64(s, 16);

            return n;
        }
        /// <summary>
        /// 获取本机MAC地址
        /// </summary>
        /// <param name="MacString">MAC字符串</param>
        /// <returns>MAC值</returns>
        private long GetMac(out string MacString)
        {
            string macAddress = "";
            if (longMac == 0)
            {
                try
                {
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface adapter in nics)
                    {
                        if (!adapter.GetPhysicalAddress().ToString().Equals(""))
                        {
                            macAddress = adapter.GetPhysicalAddress().ToString();
                            longMac = Convert.ToInt64(macAddress, 16);
                            for (int i = 1; i < 6; i++)
                            {
                                macAddress = macAddress.Insert(3 * i - 1, ":");
                            }
                            break;
                        }
                    }

                }
                catch
                {
                }
            }
            MacString = macAddress;
            return longMac;
        }
        /// <summary>
        /// 获得当前时间的4字节时间戳
        /// </summary>
        /// <returns></returns>
        private int GetTimeStamp()
        {
            DateTime timeStamp = new DateTime(1970, 1, 1); //得到1970年的时间戳 
            long a = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000; //注意这里有时区问题，用now就要减掉8个小时
            int b = (int)a;
            return b;
        }
        ///// <summary>
        ///// 获取12字节唯一ID：4字节时间戳+4字节主机MAC+2字节进程PID+2字节自增序列
        ///// </summary>
        ///// <param name="id">自增序列，只取2字节</param>
        ///// <returns>12字节唯一ID</returns>
        //private long GetUniquenessID12(int id)
        //{
        //    string strMac = "";
        //    long lngMac = GetMac(out strMac);
        //    Process curPro = Process.GetCurrentProcess();
        //    string s = string.Format("{0:X8}{1:X8}{2:X4}{3:X4}", GetTimeStamp(), (int)(lngMac), (short)curPro.Id, ((short)id));
        //    long n = Convert.ToInt64(s, 16);

        //    return n;
        //}
        #endregion

        /// <summary>
        /// 获取表信息
        /// </summary>
        /// <param name="index">表序号,从0开始</param>
        /// <param name="fieldName">表字段名称</param>
        /// <returns></returns>
        public string GetMeterInfo(int index, string fieldName)
        {
            InputParaUnit paraUnit = ParasModel.AllUnits.FirstOrDefault(item => item.FieldName == fieldName);
            if (paraUnit != null && index >= 0 && index < Meters.Count)
            {
                object obj = Meters[index].GetProperty(fieldName);
                string resultTemp = null;
                if (obj != null)
                    resultTemp = obj.ToString();

                if (paraUnit.ValueType == EnumValueType.编码值)
                {
                    resultTemp = CodeDictionary.GetValueLayer2(paraUnit.CodeType, resultTemp);
                }
                return resultTemp;
            }
            else
            { return ""; }
        }
        /// <summary>
        /// set表信息,0-
        /// </summary>
        /// <param name="index">表序号,从0开始</param>
        /// <param name="fieldName">表字段名称</param>
        /// <returns></returns>
        public void SetMeterInfo(int index, string fieldName, string value)
        {
            InputParaUnit paraUnit = ParasModel.AllUnits.FirstOrDefault(item => item.FieldName == fieldName);
            if (paraUnit != null && index >= 0 && index < Meters.Count)
            {
                Meters[index].SetProperty(fieldName, value);
            }
        }


        #region 电表信息下载

        public async void Frame_DownMeterInfoFromMisByPos(int i, string UpDownUri = "MDS")
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                IMis mis = MISFactory.Create(UpDownUri);
                bool down = DownLoadMeterInfoFromMis(mis, i);

                return down;
            });
        }
        /// <summary>
        /// 下载电表信息
        /// </summary>
        public bool Frame_DownMeterInfoFromMis(string UpDownUri = "MDS")
        {
            if (DownLoading)
            {
                LogManager.AddMessage("正在下载电表信息...", EnumLogSource.服务器日志, EnumLevel.Information);
                return false;
            }
            try
            {
                DownLoading = true;
                bool down = false;
                LogManager.AddMessage(EquipmentData.Equipment.ID + "号开始下载电表信息!", EnumLogSource.服务器日志, EnumLevel.Information);

                string Marketing_Type;
                if (UpDownUri == "MDS")
                {
                    Marketing_Type = ConfigHelper.Instance.MDS_Type;
                }
                else
                {
                    Marketing_Type = ConfigHelper.Instance.Marketing_Type;
                }

                switch (Marketing_Type)
                {
                    case "黑龙江调度平台":
                        Do_DownHeiLongJianMeterInfo();
                        break;
                    case "西安调度平台":
                        Do_DownXiAnMeterInfo();
                        break;
                    case "厚达":
                        Do_DownFromHoudaMeterInfoFromMis();
                        break;
                    case "天津MIS接口":
                        Do_DownMeterInfoFromMis(out string err);
                        break;
                    case "新疆隆元主控":
                        down = false;
                        break;
                    default:
                        down = Do_DownMeterInfoFromMis(out string _, UpDownUri);
                        break;
                }


                return down;
            }
            catch (Exception ex)
            {
                InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.故障, ex.ToString());
                LogManager.AddMessage("下载数据出错：\n\r" + ex, EnumLogSource.检定业务日志, EnumLevel.Error);
                return false;
            }
            finally
            {
                DownLoading = false;
            }
        }

        //add zxg yjt 20220228 新增自动添加编码
        /// <summary>
        /// 自动添加编码
        /// </summary>
        /// <param name="FiledName">编码对应的数据库列名</param>
        /// <param name="FiledValue">添加的新值</param>
        public void AutoFieldName(string FiledName, string FiledValue)
        {
            //1:判断是否有这个列
            InputParaUnit unitTemp = ParasModel.AllUnits.FirstOrDefault(item => item.FieldName == FiledName);
            if (unitTemp != null)
            {
                //找到这个列对应的节点
                CodeTree.CodeTreeNode nodeTemp = CodeTree.CodeTreeViewModel.Instance.GetCodeByEnName(unitTemp.CodeType, 2);
                if (nodeTemp != null)
                {
                    string name = FiledValue;//这里就是要添加的新值
                    CodeTree.CodeTreeNode nodeTemp1 = nodeTemp.Children.FirstOrDefault(item => item.CODE_NAME == name);
                    if (nodeTemp1 == null) //没有这个节点的话
                    {
                        //编码的话先获取最大的长度
                        //然后从长度那位数开始循环，判断那个没有用过
                        //nodeTemp.Children.
                        List<string> list = new List<string>();
                        var qry = from s in nodeTemp.Children
                                  orderby s.CODE_VALUE.Length descending
                                  select s;
                        int len = qry.First().CODE_VALUE.Length - 1;

                        int index = (int)Math.Pow(10, len);
                        int end = (int)Math.Pow(10, len + 1);
                        string value = "999";
                        for (int i = index; i < end; i++)
                        {
                            if (nodeTemp.Children.FirstOrDefault(item => item.CODE_VALUE == i.ToString()) == null)
                            {
                                value = i.ToString();
                                break;
                            }
                        }
                        nodeTemp.Children.Add(new CodeTree.CodeTreeNode()
                        {
                            CODE_LEVEL = nodeTemp.CODE_LEVEL + 1,
                            CODE_PARENT = nodeTemp.CODE_TYPE,
                            CODE_NAME = name,
                            CODE_VALUE = value,
                            CODE_ENABLED = true,
                            FlagChanged = true
                        });
                        //add yjt zxg 20230208 新增自动添加编码刷新数据
                        Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(nodeTemp.CODE_TYPE);
                        dictionary.Add(name, value);
                        nodeTemp.SaveCode();
                    }
                }
            }
        }

        /// <summary>
        /// 读取参数
        /// </summary>
        private readonly object Locked = new object();

        #region 黑龙江下载表信息

        /// <summary>
        /// 黑龙江下载表信息
        /// </summary>
        private void Do_DownHeiLongJianMeterInfo()
        {
            lock (Locked)
            {
                List<TestMeterInfo> testMeters = new List<TestMeterInfo>();

                HeiLongJianDownHelper downData = new HeiLongJianDownHelper
                {
                    systemID = int.Parse(EquipmentData.Equipment.ID)
                };

                if (!EquipmentData.heiLongJian.Down(EquipmentData.heiLongJian.downHelpe, ref testMeters))
                {
                    LogManager.AddMessage(EquipmentData.Equipment.ID + "号下载电表信息失败!", EnumLogSource.数据库存取日志, EnumLevel.Error);
                }

                for (int i = 0; i < Meters.Count; i++)
                {
                    Meters[i].SetProperty("MD_CHECKED", "0");
                }
                //用这个循环代替下面的
                foreach (var item in testMeters)
                {
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CHECKED", "1");//勾选项
                    Meters[item.MD_Epitope - 1].SetProperty("MD_BAR_CODE", item.MD_BarCode);//频率
                    Meters[item.MD_Epitope - 1].SetProperty("MD_UB", item.MD_UB);//电压
                    Meters[item.MD_Epitope - 1].SetProperty("MD_UA", item.MD_UA);//电流
                    Meters[item.MD_Epitope - 1].SetProperty("MD_FREQUENCY", item.MD_Frequency);//频率
                    Meters[item.MD_Epitope - 1].SetProperty("MD_TESTMODEL", item.MD_TestModel);//首检抽检
                    Meters[item.MD_Epitope - 1].SetProperty("MD_RESULT", item.Result);//检定结果

                    Meters[item.MD_Epitope - 1].SetProperty("MD_TEST_TYPE", item.MD_TestType);//全检抽检
                    Meters[item.MD_Epitope - 1].SetProperty("MD_WIRING_MODE", item.MD_WiringMode);// 测量方式--单相-三相三线-三相四线
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CONNECTION_FLAG", item.MD_ConnectionFlag);//互感器
                    Meters[item.MD_Epitope - 1].SetProperty("MD_JJGC", item.MD_JJGC);//检定规程
                    Meters[item.MD_Epitope - 1].SetProperty("MD_ASSET_NO", item.MD_AssetNo);//资产编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_METER_TYPE", item.MD_MeterType);//表类型
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CONSTANT", item.MD_Constant);//常数
                    Meters[item.MD_Epitope - 1].SetProperty("MD_GRADE", item.MD_Grane);//等级
                    Meters[item.MD_Epitope - 1].SetProperty("MD_METER_MODEL", item.MD_MeterModel);//表型号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_PROTOCOL_NAME", item.MD_ProtocolName);//通讯协议
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CARR_NAME", item.MD_CarrName);//载波协议
                    Meters[item.MD_Epitope - 1].SetProperty("MD_POSTAL_ADDRESS", item.MD_PostalAddress);//通讯地址
                    Meters[item.MD_Epitope - 1].SetProperty("MD_FACTORY", item.MD_Factory);//制造厂家
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CUSTOMER", item.MD_Customer);//送检单位
                    Meters[item.MD_Epitope - 1].SetProperty("MD_TASK_NO", item.MD_TaskNo);//任务编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_MADE_NO", item.MD_MadeNo);//出厂编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CERTIFICATE_NO", item.MD_CertificateNo);//证书编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", item.FKType);//费控类型
                    //Meters[item.MD_Epitope].SetProperty("MD_UB", meter.MD_UB);//电压              //delete yjt 20220216 多了一个电压 
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_1", item.Seal1);//铅封1
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_2", item.Seal2);//铅封2
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_3", item.Seal3);//铅封3
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_4", item.Seal4);//铅封4
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_5", item.Seal5);//铅封5

                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_1", item.Other1);//备用1
                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_2", item.Other2);//备用2
                    if (item.Other3 == null || item.Other3 == "")
                    {
                        Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_3", 0);//备用3
                    }
                    else
                    {
                        Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_3", item.Other3);//备用3
                    }
                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_4", item.Other4);//备用4
                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_5", item.Other5);//备用5

                    //add yjt 20220216 新加费控判断
                    if (item.FKType == 0)
                        Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", "远程费控");//费控类型
                    else if (item.FKType == 1)
                        Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", "本地费控");//费控类型
                    else
                        Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", "不带费控");//费控类型
                    AutoFieldName("MD_METER_MODEL", item.MD_MeterModel);
                }

                VerifyBase.MeterInfo = GetVerifyMeterInfo();  //把表信息给到检定系统
                LogManager.AddMessage(EquipmentData.Equipment.ID + "号下载电表信息完毕!", EnumLogSource.数据库存取日志, EnumLevel.TipsTip);
            }

        }

        #endregion

        #region 西安下载表信息

        /// <summary>
        /// 西安下载表信息
        /// </summary>
        private void Do_DownXiAnMeterInfo()
        {
            lock (Locked)
            {
                List<TestMeterInfo> testMeters = new List<TestMeterInfo>();

                //IMis mis = MISFactory.Create();
                LYTest.Core.DownHelper.XiAnProjectDownHelper downData = new Core.DownHelper.XiAnProjectDownHelper
                {
                    systemID = int.Parse(EquipmentData.Equipment.ID)
                };

                if (!EquipmentData.xiAnProject.Down(EquipmentData.xiAnProject.downHelpe, ref testMeters))
                {
                    LogManager.AddMessage(EquipmentData.Equipment.ID + "号下载电表信息失败!", EnumLogSource.数据库存取日志, EnumLevel.Error);
                }

                for (int i = 0; i < Meters.Count; i++)
                {
                    Meters[i].SetProperty("MD_CHECKED", "0");
                }
                LogManager.AddMessage(EquipmentData.Equipment.ID + "号本次电表数量：" + Meters.Count, EnumLogSource.数据库存取日志, EnumLevel.Information);
                //用这个循环代替下面的
                foreach (var item in testMeters)
                {
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CHECKED", "1");//勾选项
                    Meters[item.MD_Epitope - 1].SetProperty("MD_BAR_CODE", item.MD_BarCode);//频率
                    Meters[item.MD_Epitope - 1].SetProperty("MD_UB", item.MD_UB);//电压
                    Meters[item.MD_Epitope - 1].SetProperty("MD_UA", item.MD_UA);//电流
                    Meters[item.MD_Epitope - 1].SetProperty("MD_FREQUENCY", item.MD_Frequency);//频率
                    Meters[item.MD_Epitope - 1].SetProperty("MD_TESTMODEL", item.MD_TestModel);//首检抽检
                    Meters[item.MD_Epitope - 1].SetProperty("MD_RESULT", item.Result);//检定结果

                    Meters[item.MD_Epitope - 1].SetProperty("MD_TEST_TYPE", item.MD_TestType);//全检抽检
                    Meters[item.MD_Epitope - 1].SetProperty("MD_WIRING_MODE", item.MD_WiringMode);// 测量方式--单相-三相三线-三相四线
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CONNECTION_FLAG", item.MD_ConnectionFlag);//互感器
                    Meters[item.MD_Epitope - 1].SetProperty("MD_JJGC", item.MD_JJGC);//检定规程
                    Meters[item.MD_Epitope - 1].SetProperty("MD_ASSET_NO", item.MD_AssetNo);//资产编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_METER_TYPE", item.MD_MeterType);//表类型
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CONSTANT", item.MD_Constant);//常数
                    Meters[item.MD_Epitope - 1].SetProperty("MD_GRADE", item.MD_Grane);//等级
                    Meters[item.MD_Epitope - 1].SetProperty("MD_METER_MODEL", item.MD_MeterModel);//表型号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_PROTOCOL_NAME", item.MD_ProtocolName);//通讯协议
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CARR_NAME", item.MD_CarrName);//载波协议
                    Meters[item.MD_Epitope - 1].SetProperty("MD_POSTAL_ADDRESS", item.MD_PostalAddress);//通讯地址
                    Meters[item.MD_Epitope - 1].SetProperty("MD_FACTORY", item.MD_Factory);//制造厂家
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CUSTOMER", item.MD_Customer);//送检单位
                    Meters[item.MD_Epitope - 1].SetProperty("MD_TASK_NO", item.MD_TaskNo);//任务编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_MADE_NO", item.MD_MadeNo);//出厂编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_CERTIFICATE_NO", item.MD_CertificateNo);//证书编号
                    Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", item.FKType);//费控类型
                    //Meters[item.MD_Epitope].SetProperty("MD_UB", meter.MD_UB);//电压              //delete yjt 20220216 多了一个电压 
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_1", item.Seal1);//铅封1
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_2", item.Seal2);//铅封2
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_3", item.Seal3);//铅封3
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_4", item.Seal4);//铅封4
                    Meters[item.MD_Epitope - 1].SetProperty("MD_SEAL_5", item.Seal5);//铅封5

                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_1", item.Other1);//备用1
                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_2", item.Other2);//备用2
                    if (item.Other3 == null || item.Other3 == "")
                    {
                        Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_3", 0);//备用3
                    }
                    else
                    {
                        Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_3", item.Other3);//备用3
                    }
                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_4", item.Other4);//备用4
                    Meters[item.MD_Epitope - 1].SetProperty("MD_OTHER_5", item.Other5);//备用5

                    //add yjt 20220216 新加费控判断
                    if (item.FKType == 0)
                        Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", "远程费控");//费控类型
                    else if (item.FKType == 1)
                        Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", "本地费控");//费控类型
                    else
                        Meters[item.MD_Epitope - 1].SetProperty("MD_FKTYPE", "不带费控");//费控类型
                    AutoFieldName("MD_METER_MODEL", item.MD_MeterModel);
                }

                VerifyBase.MeterInfo = GetVerifyMeterInfo();  //把表信息给到检定系统
                LogManager.AddMessage(EquipmentData.Equipment.ID + "号下载电表信息完毕!", EnumLogSource.数据库存取日志, EnumLevel.TipsTip);
            }

        }

        #endregion

        #region 下载电表信息_通用
        private bool DownLoadMeterInfoFromMis(IMis mis, int i, string UpDownUri = "MDS")
        {
            if (!YaoJian[i]) return false;

            try
            {
                string strValue = "";

                if (UpDownUri == "MDS")
                {
                    if (ConfigHelper.Instance.MDS_DewnLoadNumber == "出厂编号")      //出厂编号
                    {
                        strValue = GetMeterInfo(i, "MD_MADE_NO");
                    }
                    else if (ConfigHelper.Instance.MDS_DewnLoadNumber == "条形码") //条形码
                    {
                        strValue = GetMeterInfo(i, "MD_BAR_CODE");
                    }
                    else if (ConfigHelper.Instance.MDS_DewnLoadNumber == "表位号")  //表位号
                    {
                        strValue = (i + 1).ToString();//TODO:从0还是1开始
                    }
                }
                else
                {
                    if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "出厂编号")      //出厂编号
                    {
                        strValue = GetMeterInfo(i, "MD_MADE_NO");
                    }
                    else if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "条形码") //条形码
                    {
                        strValue = GetMeterInfo(i, "MD_BAR_CODE");
                    }
                    else if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "表位号")  //表位号
                    {
                        strValue = (i + 1).ToString();//TODO:从0还是1开始
                    }
                }

                if (string.IsNullOrEmpty(strValue)) return false;

                TestMeterInfo meter = new TestMeterInfo();
                if (!mis.Down(strValue, ref meter))
                {
                    return false;
                }
                meter.YaoJianYn = true;
                if (UpDownUri == "MDS")
                {
                    if (ConfigHelper.Instance.MDS_DewnLoadNumber == "表位号")
                    {
                        Meters[i].SetProperty("MD_BAR_CODE", meter.MD_BarCode);//条形码
                    }
                }
                else
                {
                    if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "表位号")
                    {
                        Meters[i].SetProperty("MD_BAR_CODE", meter.MD_BarCode);//条形码
                    }
                }
                //add yjt 20220216 新加默认数据
                meter.MD_TestModel = meter.MD_TestModel ?? "首检";
                meter.MD_TestType = meter.MD_TestType ?? "到货全检";
                meter.MD_Frequency = meter.MD_Frequency == 0 ? 50 : meter.MD_Frequency;
                meter.MD_CarrName = meter.MD_CarrName ?? "标准载波";
                Meters[i].SetProperty("MD_UB", meter.MD_UB.ToString());//电压
                Meters[i].SetProperty("MD_UA", meter.MD_UA);//电流
                Meters[i].SetProperty("MD_FREQUENCY", meter.MD_Frequency);//频率
                Meters[i].SetProperty("MD_TESTMODEL", meter.MD_TestModel);//首检抽检

                Meters[i].SetProperty("MD_TEST_TYPE", meter.MD_TestType);//全检抽检
                Meters[i].SetProperty("MD_WIRING_MODE", meter.MD_WiringMode);//测量方式--单相-三相三线-三相四线
                Meters[i].SetProperty("MD_CONNECTION_FLAG", meter.MD_ConnectionFlag);//互感器
                Meters[i].SetProperty("MD_JJGC", meter.MD_JJGC);//检定规程
                Meters[i].SetProperty("MD_ASSET_NO", meter.MD_AssetNo);//资产编号
                Meters[i].SetProperty("MD_METER_TYPE", meter.MD_MeterType);//表类型
                Meters[i].SetProperty("MD_CONSTANT", meter.MD_Constant);//常数
                Meters[i].SetProperty("MD_GRADE", meter.MD_Grane);//等级
                Meters[i].SetProperty("MD_METER_MODEL", meter.MD_MeterModel);//表型号
                Meters[i].SetProperty("MD_PROTOCOL_NAME", meter.MD_ProtocolName);//通讯协议
                Meters[i].SetProperty("MD_CARR_NAME", meter.MD_CarrName);//载波协议
                Meters[i].SetProperty("MD_POSTAL_ADDRESS", meter.MD_PostalAddress);//通讯地址
                Meters[i].SetProperty("MD_FACTORY", meter.MD_Factory);//制造厂家
                Meters[i].SetProperty("MD_CUSTOMER", meter.MD_Customer);//送检单位
                Meters[i].SetProperty("MD_TASK_NO", meter.MD_TaskNo);//任务编号
                Meters[i].SetProperty("MD_MADE_NO", meter.MD_MadeNo);//出厂编号
                Meters[i].SetProperty("MD_CERTIFICATE_NO", meter.MD_CertificateNo);//证书编号
                Meters[i].SetProperty("MD_FKTYPE", meter.FKType);//费控类型

                Meters[i].SetProperty("MD_SEAL_1", meter.Seal1);//铅封1
                Meters[i].SetProperty("MD_SEAL_2", meter.Seal2);//铅封2
                Meters[i].SetProperty("MD_SEAL_3", meter.Seal3);//铅封3
                //--脉冲类型--蓝牙脉冲光电脉冲
                if (meter.MD_Sort == "物联电能表") meter.Other3 = "蓝牙脉冲";
                else meter.Other3 = "无";

                Meters[i].SetProperty("MD_SEAL_4", meter.Seal4);//铅封4
                Meters[i].SetProperty("MD_SEAL_5", meter.Seal5);//铅封5

                Meters[i].SetProperty("MD_OTHER_1", meter.Other1);//备用1
                Meters[i].SetProperty("MD_OTHER_2", meter.Other2);//备用2
                Meters[i].SetProperty("MD_OTHER_3", meter.Other3);//备用3
                Meters[i].SetProperty("MD_OTHER_4", meter.Other4);//备用4
                Meters[i].SetProperty("MD_OTHER_5", meter.Other5);//备用5

                //add yjt 20220216 新加费控判断
                if (meter.FKType == 0)
                    Meters[i].SetProperty("MD_FKTYPE", "远程费控");//费控类型
                else if (meter.FKType == 1)
                    Meters[i].SetProperty("MD_FKTYPE", "本地费控");//费控类型
                else
                    Meters[i].SetProperty("MD_FKTYPE", "不带费控");//费控类型
                Meters[i].SetProperty("MD_OTHER_1", "1");   //设置表位需要上传数据
                Meters[i].SetProperty("MD_OTHER_2", "未上传");   //设置表位需要上传数据
                Meters[i].SetProperty("MD_UPDOWN", UpDownUri);
                VerifyBase.MeterInfo = GetVerifyMeterInfo();  //把表信息给到检定系统


                //meterInfos[i].Meter_ID = GetMeterInfo(i, "METER_ID");
                ////表位
                //meterInfos[i].MD_Epitope = i + 1;
                ////电压
                //float.TryParse(GetMeterInfo(i, "MD_UB"), out t2);
                ////条形码
                //meterInfos[i].MD_BarCode = GetMeterInfo(i, "MD_BAR_CODE");

                //add yjt 20220228 新增表型号在没有情况下自增
                AutoFieldName("MD_METER_MODEL", meter.MD_MeterModel);

                //add yjt 20220413 新增表类型和制造厂家在没有情况下自增
                AutoFieldName("MD_METER_TYPE", meter.MD_MeterType);
                AutoFieldName("MD_FACTORY", meter.MD_Factory);
                return true;
            }
            catch(Exception ex) 
            {
            
            }
            return false;
        }
        /// <summary>
        /// 下载电表信息
        /// </summary>
        /// <param name="obj"></param>
        private bool Do_DownMeterInfoFromMis(out string err, string UpDownUri = "MDS")
        {
            err = "";
            TestMeterInfo meter = new TestMeterInfo();
            bool down = true;
            IMis mis = MISFactory.Create(UpDownUri);

            //string addressErr = "";
            for (int i = 0; i < Meters.Count; i++)
            {
                string strValue = "";

                if (!YaoJian[i]) continue;
                if (UpDownUri == "MDS")
                {
                    if (ConfigHelper.Instance.MDS_DewnLoadNumber == "出厂编号")      //出厂编号
                    {
                        strValue = GetMeterInfo(i, "MD_MADE_NO");
                    }
                    else if (ConfigHelper.Instance.MDS_DewnLoadNumber == "条形码") //条形码
                    {
                        strValue = GetMeterInfo(i, "MD_BAR_CODE");
                    }
                    else if (ConfigHelper.Instance.MDS_DewnLoadNumber == "表位号")  //表位号
                    {
                        strValue = (i + 1).ToString();//TODO:从0还是1开始
                    }
                }
                else
                {
                    if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "出厂编号")      //出厂编号
                    {
                        strValue = GetMeterInfo(i, "MD_MADE_NO");
                    }
                    else if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "条形码") //条形码
                    {
                        strValue = GetMeterInfo(i, "MD_BAR_CODE");
                    }
                    else if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "表位号")  //表位号
                    {
                        strValue = (i + 1).ToString();//TODO:从0还是1开始
                    }
                }

                if (string.IsNullOrEmpty(strValue)) continue;

                //modify yjt 20220228 移动到方法最上面
                //TestMeterInfo meter = new TestMeterInfo();

                if (!mis.Down(strValue, ref meter))
                {
                    down = false;
                    continue;
                }
                meter.YaoJianYn = true;
                if (UpDownUri == "MDS")
                {
                    if (ConfigHelper.Instance.MDS_DewnLoadNumber == "表位号")
                    {
                        Meters[i].SetProperty("MD_BAR_CODE", meter.MD_BarCode);//条形码
                    }
                }
                else
                {
                    if (ConfigHelper.Instance.Marketing_DewnLoadNumber == "表位号")
                    {
                        Meters[i].SetProperty("MD_BAR_CODE", meter.MD_BarCode);//条形码
                    }
                }

                //string oldAddr = (string)Meters[i].GetProperty("MD_POSTAL_ADDRESS");
                //if (oldAddr != meter.MD_PostalAddress)
                //{
                //    err += $"表位{i + 1}\n";
                //}

                //add yjt 20220216 新加默认数据
                meter.MD_TestModel = meter.MD_TestModel ?? "首检";
                meter.MD_TestType = meter.MD_TestType ?? "到货全检";
                meter.MD_Frequency = meter.MD_Frequency == 0 ? 50 : meter.MD_Frequency;
                meter.MD_CarrName = meter.MD_CarrName ?? "标准载波";
                Meters[i].SetProperty("MD_UB", meter.MD_UB.ToString());//电压
                Meters[i].SetProperty("MD_UA", meter.MD_UA);//电流
                Meters[i].SetProperty("MD_FREQUENCY", meter.MD_Frequency);//频率
                Meters[i].SetProperty("MD_TESTMODEL", meter.MD_TestModel);//首检抽检

                Meters[i].SetProperty("MD_TEST_TYPE", meter.MD_TestType);//全检抽检
                Meters[i].SetProperty("MD_WIRING_MODE", meter.MD_WiringMode);// 测量方式--单相-三相三线-三相四线
                Meters[i].SetProperty("MD_CONNECTION_FLAG", meter.MD_ConnectionFlag);//互感器
                Meters[i].SetProperty("MD_JJGC", meter.MD_JJGC ?? "JJG596-2012");//检定规程
                Meters[i].SetProperty("MD_ASSET_NO", meter.MD_AssetNo);//资产编号
                Meters[i].SetProperty("MD_METER_TYPE", meter.MD_MeterType);//表类型
                Meters[i].SetProperty("MD_CONSTANT", meter.MD_Constant);//常数
                Meters[i].SetProperty("MD_GRADE", meter.MD_Grane);//等级
                Meters[i].SetProperty("MD_METER_MODEL", meter.MD_MeterModel);//表型号
                Meters[i].SetProperty("MD_PROTOCOL_NAME", meter.MD_ProtocolName);//通讯协议
                Meters[i].SetProperty("MD_CARR_NAME", meter.MD_CarrName);//载波协议
                Meters[i].SetProperty("MD_POSTAL_ADDRESS", meter.MD_PostalAddress);//通讯地址
                Meters[i].SetProperty("MD_FACTORY", meter.MD_Factory);//制造厂家
                Meters[i].SetProperty("MD_CUSTOMER", meter.MD_Customer);//送检单位
                Meters[i].SetProperty("MD_TASK_NO", meter.MD_TaskNo);//任务编号
                Meters[i].SetProperty("MD_MADE_NO", meter.MD_MadeNo);//出厂编号
                Meters[i].SetProperty("MD_SORT", meter.MD_Sort ?? "智能表");//类别
                Meters[i].SetProperty("MD_CERTIFICATE_NO", meter.MD_CertificateNo);//证书编号
                Meters[i].SetProperty("MD_FKTYPE", meter.FKType);//费控类型
                                                                 //Meters[i].SetProperty("MD_UB", meter.MD_UB);//电压              //delete yjt 20220216 多了一个电压 
                Meters[i].SetProperty("MD_SEAL_1", meter.Seal1);//铅封1
                Meters[i].SetProperty("MD_SEAL_2", meter.Seal2);//铅封2
                Meters[i].SetProperty("MD_SEAL_3", meter.Seal3);//铅封3

                //--脉冲类型--蓝牙脉冲光电脉冲
                if (meter.MD_Sort == "物联电能表")
                {
                    meter.Other3 = "蓝牙脉冲";
                }
                else
                {
                    meter.Other3 = "无";
                }

                Meters[i].SetProperty("MD_SEAL_4", meter.Seal4);//铅封4
                Meters[i].SetProperty("MD_SEAL_5", meter.Seal5);//铅封5

                Meters[i].SetProperty("MD_OTHER_1", meter.Other1);//备用1
                Meters[i].SetProperty("MD_OTHER_2", meter.Other2);//备用2
                Meters[i].SetProperty("MD_OTHER_3", meter.Other3);//备用3
                Meters[i].SetProperty("MD_OTHER_4", meter.Other4);//备用4
                Meters[i].SetProperty("MD_OTHER_5", meter.Other5);//备用5

                //add yjt 20220216 新加费控判断
                if (meter.FKType == 0)
                    Meters[i].SetProperty("MD_FKTYPE", "远程费控");//费控类型
                else if (meter.FKType == 1)
                    Meters[i].SetProperty("MD_FKTYPE", "本地费控");//费控类型
                else
                    Meters[i].SetProperty("MD_FKTYPE", "不带费控");//费控类型
                Meters[i].SetProperty("MD_OTHER_1", "1");   //设置表位需要上传数据
                Meters[i].SetProperty("MD_OTHER_2", "未上传");   //设置表位需要上传数据

                VerifyBase.MeterInfo = GetVerifyMeterInfo();  //把表信息给到检定系统


                //meterInfos[i].Meter_ID = GetMeterInfo(i, "METER_ID");
                ////表位
                //meterInfos[i].MD_Epitope = i + 1;
                ////电压
                //float.TryParse(GetMeterInfo(i, "MD_UB"), out t2);
                ////条形码
                //meterInfos[i].MD_BarCode = GetMeterInfo(i, "MD_BAR_CODE");


                //modify yjt 20230131 放在循环内，使每个表位都可以
                //add yjt 20220228 新增表型号在没有情况下自增
                AutoFieldName("MD_METER_MODEL", meter.MD_MeterModel);

                //add yjt 20220413 新增表类型和制造厂家在没有情况下自增
                AutoFieldName("MD_METER_TYPE", meter.MD_MeterType);
                AutoFieldName("MD_FACTORY", meter.MD_Factory);
            }
            LogManager.AddMessage(EquipmentData.Equipment.ID + "号下载电表信息完毕!", EnumLogSource.数据库存取日志, EnumLevel.Tip);
            return down;
        }

        //private bool Do_DownMeterInfoFromDataLY()
        //{
        //    bool down = false;
        //    return down;
        //}
        #endregion

        #region 下载电表信息_厚达
        /// <summary>
        ///从厚达数据库下载数据
        /// </summary>
        private void Do_DownFromHoudaMeterInfoFromMis()
        {

            //Meters[1].SetProperty("MD_METER_TYPE", "112233");//电压
            //初始化所有表位 为 不挂表状态
            for (int i = 0; i < Meters.Count; i++)
            {
                Meters[i].SetProperty("MD_CHECKED", "0");
            }

            IMis mis = MISFactory.Create();
            //从厚达数据库下载数据
            Dictionary<int, TestMeterInfo> meterDic = ((Mis.Houda.Houda)mis).GetMeterModel();
            if (meterDic.Count <= 0)
            {
                LogManager.AddMessage("下载电表信息失败");
                return;
            }

            foreach (int key in meterDic.Keys)
            {
                TestMeterInfo meter = meterDic[key];
                if (meter == null) continue;
                Meters[key - 1].SetProperty("MD_BAR_CODE", meter.MD_BarCode);//条形码
                                                                             //string str=
                Meters[key - 1].SetProperty("MD_UB", meter.MD_UB);//电压
                Meters[key - 1].SetProperty("MD_UA", meter.MD_UA);//电流
                Meters[key - 1].SetProperty("MD_FREQUENCY", 50);//频率
                Meters[key - 1].SetProperty("MD_TESTMODEL", meter.MD_TestModel);//首检抽检
                if (meter.MD_TestModel == null || meter.MD_TestModel == "")
                {
                    Meters[key - 1].SetProperty("MD_TESTMODEL", "首检");//首检抽检
                }

                if (meter.MD_TestType.IndexOf("抽") != -1)
                {
                    Meters[key - 1].SetProperty("MD_TEST_TYPE", "质量抽检");//全检抽检
                }
                else
                {
                    Meters[key - 1].SetProperty("MD_TEST_TYPE", "到货全检");//全检抽检
                }

                Meters[key - 1].SetProperty("MD_WIRING_MODE", meter.MD_WiringMode);// 测量方式--单相-三相三线-三相四线
                Meters[key - 1].SetProperty("MD_CONNECTION_FLAG", meter.MD_ConnectionFlag);//互感器
                Meters[key - 1].SetProperty("MD_JJGC", meter.MD_JJGC);//检定规程
                Meters[key - 1].SetProperty("MD_ASSET_NO", meter.MD_AssetNo);//资产编号
                Meters[key - 1].SetProperty("MD_METER_TYPE", meter.MD_MeterType);//表类型
                Meters[key - 1].SetProperty("MD_CONSTANT", meter.MD_Constant);//常数
                Meters[key - 1].SetProperty("MD_GRADE", meter.MD_Grane.ToUpper());//等级
                Meters[key - 1].SetProperty("MD_METER_MODEL", meter.MD_MeterModel);//表型号
                Meters[key - 1].SetProperty("MD_PROTOCOL_NAME", meter.MD_ProtocolName);//通讯协议
                Meters[key - 1].SetProperty("MD_CARR_NAME", meter.MD_CarrName);//载波协议
                Meters[key - 1].SetProperty("MD_POSTAL_ADDRESS", meter.MD_PostalAddress);//通讯地址
                Meters[key - 1].SetProperty("MD_FACTORY", meter.MD_Factory);//制造厂家
                Meters[key - 1].SetProperty("MD_CUSTOMER", meter.MD_Customer);//送检单位
                Meters[key - 1].SetProperty("MD_TASK_NO", meter.MD_TaskNo);//任务编号
                Meters[key - 1].SetProperty("MD_MADE_NO", meter.MD_MadeNo);//出厂编号
                Meters[key - 1].SetProperty("MD_CERTIFICATE_NO", meter.MD_CertificateNo);//证书编号

                if (meter.FKType == 0)
                    Meters[key - 1].SetProperty("MD_FKTYPE", "远程费控");//费控类型
                else if (meter.FKType == 1)
                    Meters[key - 1].SetProperty("MD_FKTYPE", "本地费控");//费控类型
                else
                    Meters[key - 1].SetProperty("MD_FKTYPE", "不带费控");//费控类型


                Meters[key - 1].SetProperty("MD_UB", meter.MD_UB);//电压
                Meters[key - 1].SetProperty("MD_SEAL_1", meter.Seal1);//铅封1
                Meters[key - 1].SetProperty("MD_SEAL_2", meter.Seal2);//铅封2
                Meters[key - 1].SetProperty("MD_SEAL_3", meter.Seal3);//铅封3
                Meters[key - 1].SetProperty("MD_SEAL_4", meter.Seal4);//铅封4
                Meters[key - 1].SetProperty("MD_SEAL_5", meter.Seal5);//铅封5
                Meters[key - 1].SetProperty("MD_OTHER_1", meter.Other1);//备用1
                Meters[key - 1].SetProperty("MD_OTHER_2", meter.Other2);//备用2
                Meters[key - 1].SetProperty("MD_OTHER_3", meter.Other3);//备用3
                Meters[key - 1].SetProperty("MD_OTHER_4", meter.Other4);//备用4
                Meters[key - 1].SetProperty("MD_OTHER_5", meter.Other5);//备用5
                Meters[key - 1].SetProperty("MD_CHECKED", "1");   //设置表位要检定
                Meters[key - 1].SetProperty("MD_OTHER_1", "1");   //设置表位需要上传数据
                Meters[key - 1].SetProperty("MD_OTHER_2", "未上传");   //设置表位需要上传数据

            }

            LogManager.AddMessage(EquipmentData.Equipment.ID + "号下载电表信息完毕!", EnumLogSource.数据库存取日志, EnumLevel.Information);

            VerifyBase.MeterInfo = GetVerifyMeterInfo();  //把表信息给到检定系统
            //if (ConfigHelper.Instance.AreaName == "北京" && VerifyBase.OneMeterInfo.MD_JJGC == "IR46")
            //{
            //    VerifyBase.IsDoubleProtocol = true;  //双协议电表
            //}
            //else
            //{ 
            //    VerifyBase.IsDoubleProtocol = false ;  //双协议电表
            //}
        }
        #endregion

        #endregion

        #region 方案下载

        #region 下载方案_哈尔滨
        /// <summary>
        /// 下载方案_哈尔滨
        /// </summary>
        public void Frame_DownSchemeMis_HLJ()
        {

            //不开线程下载，下载完成了再执行 
            LogManager.AddMessage("开始下载方案");
            Do_DownSchemeInfoFromMis_HLJ();

        }

        /// <summary>
        /// 下载方案信息_哈尔滨
        /// </summary>
        /// <param name="obj"></param>
        private void Do_DownSchemeInfoFromMis_HLJ()
        {

            bool bOK = false;
            TestMeterInfo meter = VerifyBase.OneMeterInfo;
            if (meter == null)
            {
                LogManager.AddMessage("没有要检表信息，下载方案信息失败");
                return;
            }
            string ip = ConfigHelper.Instance.MDSProduce_IP;
            int port = int.Parse(ConfigHelper.Instance.MDSProduce_Prot);
            string dataSource = ConfigHelper.Instance.MDSProduce_DataSource;
            string userId = ConfigHelper.Instance.MDSProduce_UserName;
            string pwd = ConfigHelper.Instance.MDSProduce_UserPassWord; ;
            string url = ConfigHelper.Instance.MDS_WebService;




            //EquipmentData.heiLongJian


            string schemeName = "";
            Dictionary<string, SchemaNode> Schema = null;
            if (meter.MD_BarCode.Length > 0)
            {
                if (ConfigHelper.Instance.Marketing_Type == "黑龙江调度平台")
                {
                    EquipmentData.heiLongJian.TaskID = meter.MD_TaskNo;

                    EquipmentData.heiLongJian.systenID = int.Parse(EquipmentData.Equipment.ID);

                    bOK = EquipmentData.heiLongJian.SchemeDown(meter.MD_BarCode, out schemeName, out Schema);


                }
                else
                {
                    IMis mis = new MDS(ip, port, dataSource, userId, pwd, url);
                    bOK = mis.SchemeDown(meter.MD_BarCode, out schemeName, out Schema);
                }
            }
            if (bOK)
            {
                //删除所有本地方案

                EquipmentData.SchemaModels.DeleteSchema();
                //创建方案
                EquipmentData.SchemaModels.NewName = schemeName;
                EquipmentData.SchemaModels.AddDownSchema(); //这个需要重写一个方法，刷新方案放在背后，并且把选中等方法添加到里面
                                                            //EquipmentData.SchemaModels.SelectedSchema =
                try
                {
                    EquipmentData.SchemaModels.SelectedSchema = EquipmentData.SchemaModels.Schemas[EquipmentData.SchemaModels.Schemas.Count - 1];
                    //获取数据库ID进行绑定首页的方案下拉选项
                    EquipmentData.Schema.SchemaId = int.Parse(EquipmentData.SchemaModels.SelectedSchema.GetDataSource().GetProperty("ID").ToString());

                }
                catch { }


                foreach (var key in Schema.Keys)
                {
                    if (!EquipmentData.Schema.ExistNode(key))
                    {
                        EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
                        EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
                    }
                    List<string> propertyNames = new List<string>();

                    for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
                    {
                        propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
                    }


                    for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
                    {

                        DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
                        viewModel2.SetProperty("IsSelected", true);
                        string[] value = Schema[key].SchemaNodeValue[i].Split('|');
                        for (int j = 0; j < propertyNames.Count; j++)
                        {
                            viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
                        }
                        EquipmentData.Schema.ParaValuesView.Add(viewModel2);
                    }
                    EquipmentData.Schema.SelectedNode.ParaValuesCurrent.Clear();
                    EquipmentData.Schema.SelectedNode.ParaValuesCurrent.AddRange(EquipmentData.Schema.ParaValuesConvertBack());
                    //EquipmentData.Schema.SaveDownParaValue();    //保存方案
                    EquipmentData.Schema.RefreshPointCount();
                }
                EquipmentData.Schema.SaveParaValue();    //保存方案
                EquipmentData.SchemaModels.RefreshCurrrentSchema();


                //if (modelTemp != null)   //说明本地已经有这个方案了，切换方案就好
                //{
                //    EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                //}
                //else  //没有找到方案，进行创建方案
                //{

                //    modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == EquipmentData.SchemaModels.NewName);
                //    EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                //    System.Threading.Thread.Sleep(200);
                //    foreach (var key in Schema.Keys)
                //    {
                //        if (!EquipmentData.Schema.ExistNode(key))
                //        {
                //            SchemaNodeViewModel nodeNew = EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
                //            EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
                //        }
                //        List<string> propertyNames = new List<string>();

                //        for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
                //        {
                //            propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
                //        }

                //        for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
                //        {
                //            DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
                //            viewModel2.SetProperty("IsSelected", true);
                //            string[] value = Schema[key].SchemaNodeValue[i].Split('|');
                //            for (int j = 0; j < propertyNames.Count; j++)
                //            {
                //                viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
                //            }
                //            EquipmentData.Schema.ParaValuesView.Add(viewModel2);
                //        }
                //        EquipmentData.Schema.SelectedNode.ParaValuesCurrent = EquipmentData.Schema.ParaValuesConvertBack();
                //        //EquipmentData.Schema.SaveDownParaValue();    //保存方案
                //        EquipmentData.Schema.RefreshPointCount();
                //    }
                //    EquipmentData.Schema.SaveParaValue();    //保存方案
                //    EquipmentData.SchemaModels.RefreshCurrrentSchema();
                //}
            }
            else
            {
                LogManager.AddMessage("下载方案失败", EnumLogSource.设备操作日志, EnumLevel.Error);
            }
        }

        #endregion

        #region 下载方案_西安
        /// <summary>
        /// 下载方案_西安
        /// </summary>
        public void Frame_DownSchemeMis_XiAn()
        {

            //不开线程下载，下载完成了再执行 
            LogManager.AddMessage("开始下载方案");
            Do_DownSchemeInfoFromMis_XiAn();

        }

        /// <summary>
        /// 下载方案信息_哈尔滨
        /// </summary>
        /// <param name="obj"></param>
        private void Do_DownSchemeInfoFromMis_XiAn()
        {

            bool bOK = false;
            TestMeterInfo meter = VerifyBase.OneMeterInfo;
            if (meter == null)
            {
                LogManager.AddMessage("没有要检表信息，下载方案信息失败");
                return;
            }
            string ip = ConfigHelper.Instance.MDSProduce_IP;
            int port = int.Parse(ConfigHelper.Instance.MDSProduce_Prot);
            string dataSource = ConfigHelper.Instance.MDSProduce_DataSource;
            string userId = ConfigHelper.Instance.MDSProduce_UserName;
            string pwd = ConfigHelper.Instance.MDSProduce_UserPassWord; ;
            string url = ConfigHelper.Instance.MDS_WebService;




            //EquipmentData.heiLongJian


            string schemeName = "";
            Dictionary<string, SchemaNode> Schema = null;
            if (meter.MD_BarCode.Length > 0)
            {
                if (ConfigHelper.Instance.Marketing_Type == "西安调度平台")
                {
                    EquipmentData.xiAnProject.TaskID = meter.MD_TaskNo;

                    EquipmentData.xiAnProject.systenID = int.Parse(EquipmentData.Equipment.ID);

                    bOK = EquipmentData.xiAnProject.SchemeDown(meter.MD_BarCode, out schemeName, out Schema);


                }
                else
                {
                    IMis mis = new MDS(ip, port, dataSource, userId, pwd, url);
                    bOK = mis.SchemeDown(meter.MD_BarCode, out schemeName, out Schema);
                }
            }
            if (bOK)
            {
                //删除所有本地方案
                EquipmentData.SchemaModels.DeleteSchema();
                //EquipmentData.SchemaModels
                //EquipmentData.Schema.SaveParaValue();    //保存方案

                //DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == schemeName);
                //创建方案
                EquipmentData.SchemaModels.NewName = schemeName;
                EquipmentData.SchemaModels.AddDownSchema(); //这个需要重写一个方法，刷新方案放在背后，并且把选中等方法添加到里面
                                                            //EquipmentData.SchemaModels.SelectedSchema =
                try
                {
                    EquipmentData.SchemaModels.SelectedSchema = EquipmentData.SchemaModels.Schemas[EquipmentData.SchemaModels.Schemas.Count - 1];
                    //获取数据库ID进行绑定首页的方案下拉选项
                    EquipmentData.Schema.SchemaId = int.Parse(EquipmentData.SchemaModels.SelectedSchema.GetDataSource().GetProperty("ID").ToString());

                }
                catch { }


                foreach (var key in Schema.Keys)
                {
                    if (!EquipmentData.Schema.ExistNode(key))
                    {
                        EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
                        EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
                    }
                    List<string> propertyNames = new List<string>();

                    for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
                    {
                        propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
                    }


                    for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
                    {

                        DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
                        viewModel2.SetProperty("IsSelected", true);
                        string[] value = Schema[key].SchemaNodeValue[i].Split('|');
                        for (int j = 0; j < propertyNames.Count; j++)
                        {
                            viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
                        }
                        EquipmentData.Schema.ParaValuesView.Add(viewModel2);
                    }
                    EquipmentData.Schema.SelectedNode.ParaValuesCurrent.Clear();
                    EquipmentData.Schema.SelectedNode.ParaValuesCurrent.AddRange(EquipmentData.Schema.ParaValuesConvertBack());
                    //EquipmentData.Schema.SaveDownParaValue();    //保存方案
                    EquipmentData.Schema.RefreshPointCount();
                }
                EquipmentData.Schema.SaveParaValue();    //保存方案
                EquipmentData.SchemaModels.RefreshCurrrentSchema();


                //if (modelTemp != null)   //说明本地已经有这个方案了，切换方案就好
                //{
                //    EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                //}
                //else  //没有找到方案，进行创建方案
                //{

                //    modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == EquipmentData.SchemaModels.NewName);
                //    EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                //    System.Threading.Thread.Sleep(200);
                //    foreach (var key in Schema.Keys)
                //    {
                //        if (!EquipmentData.Schema.ExistNode(key))
                //        {
                //            SchemaNodeViewModel nodeNew = EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
                //            EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
                //        }
                //        List<string> propertyNames = new List<string>();

                //        for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
                //        {
                //            propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
                //        }

                //        for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
                //        {
                //            DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
                //            viewModel2.SetProperty("IsSelected", true);
                //            string[] value = Schema[key].SchemaNodeValue[i].Split('|');
                //            for (int j = 0; j < propertyNames.Count; j++)
                //            {
                //                viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
                //            }
                //            EquipmentData.Schema.ParaValuesView.Add(viewModel2);
                //        }
                //        EquipmentData.Schema.SelectedNode.ParaValuesCurrent = EquipmentData.Schema.ParaValuesConvertBack();
                //        //EquipmentData.Schema.SaveDownParaValue();    //保存方案
                //        EquipmentData.Schema.RefreshPointCount();
                //    }
                //    EquipmentData.Schema.SaveParaValue();    //保存方案
                //    EquipmentData.SchemaModels.RefreshCurrrentSchema();
                //}
            }
            else
            {
                LogManager.AddMessage("下载方案失败", EnumLogSource.设备操作日志, EnumLevel.Error);
            }
        }
        #endregion

        #region 下载方案_通用

        /// <summary>
        /// 下载方案_通用
        /// </summary>
        public bool Frame_DownSchemeMis()
        {
            //不开线程下载，下载完成了再执行 
            LogManager.AddMessage("开始下载方案");
            return Do_DownSchemeInfoFromMis();

        }
        /// <summary>
        /// 下载方案信息_通用
        /// </summary>
        /// <param name="obj"></param>
        private bool Do_DownSchemeInfoFromMis(string UpDownUri = "MDS")
        {
            try
            {
                TestMeterInfo meter = VerifyBase.OneMeterInfo;
                if (meter == null || !meter.YaoJianYn)
                {
                    LogManager.AddMessage("没有要检表信息，下载方案信息失败");
                    return false;
                }

                IMis mis = MISFactory.Create();
                if (!string.IsNullOrWhiteSpace(meter.MD_BarCode))
                {
                    string schemeName;
                    Dictionary<string, SchemaNode> Schema;
                    bool bOK;
                    if ((UpDownUri == "MDS" && ConfigHelper.Instance.MDS_Type == "天津MIS接口")
                        || (UpDownUri == "营销" && ConfigHelper.Instance.Marketing_Type == "天津MIS接口"))
                    {
                        bOK = mis.SchemeDown(meter, out schemeName, out Schema);
                    }
                    else
                    {
                        bOK = mis.SchemeDown(meter.MD_BarCode, out schemeName, out Schema);
                        if (!bOK)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                bOK = mis.SchemeDown(meter.MD_BarCode, out schemeName, out Schema);
                                if (bOK) break;
                            }
                        }
                    }

                    if (bOK)
                    {
                        DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == schemeName);
                        if (modelTemp != null)   //说明本地已经有这个方案了，切换方案就好，删除方案
                        {
                            EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                            EquipmentData.SchemaModels.DeleteSchema();
                            //TODO:判断内容变更
                        }
                        EquipmentData.SchemaModels.NewName = schemeName;
                        EquipmentData.SchemaModels.AddDownSchema(); //这个需要重写一个方法，刷新方案放在背后，并且把选中等方法添加到里面
                        modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == EquipmentData.SchemaModels.NewName);
                        EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                        EquipmentData.Schema.SchemaId = int.Parse(EquipmentData.SchemaModels.SelectedSchema.GetProperty("ID").ToString());
                        EquipmentData.Schema.Children.Clear();
                        System.Threading.Thread.Sleep(200);
                        foreach (var key in Schema.Keys)
                        {
                            if (!EquipmentData.Schema.ExistNode(key))
                            {
                                SchemaNodeViewModel nodeNew = EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
                                EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
                            }
                            List<string> propertyNames = new List<string>();

                            for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
                            {
                                propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
                            }

                            for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
                            {
                                DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
                                viewModel2.SetProperty("IsSelected", true);
                                string propertyValue = Schema[key].SchemaNodeValue[i];
                                int propertyNamesCount = propertyNames.Count;
                                //下载参数不匹配“|”格式，且多项为一个参数“||，||，”将显示参数配置成一个，显示全部参数
                                if (propertyNamesCount == 1)
                                {
                                    viewModel2.SetProperty(propertyNames[0], propertyValue);
                                }
                                else
                                {
                                    string[] value = Schema[key].SchemaNodeValue[i].Split('|');
                                    for (int j = 0; j < propertyNamesCount; j++)
                                    {
                                        viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
                                    }
                                }
                                EquipmentData.Schema.ParaValuesView.Add(viewModel2);
                            }
                            if (EquipmentData.Schema.SelectedNode != null)
                            {
                                EquipmentData.Schema.SelectedNode.ParaValuesCurrent.Clear();
                                EquipmentData.Schema.SelectedNode.ParaValuesCurrent.AddRange(EquipmentData.Schema.ParaValuesConvertBack());


                            }
                            //EquipmentData.Schema.SaveDownParaValue();    //保存方案
                            EquipmentData.Schema.RefreshPointCount();
                        }
                        EquipmentData.Schema.SaveParaValue();    //保存方案

                        return true;
                    }
                    else
                    {
                        InnerCommand.VerifyControl.SendMsg("下载方案信息失败");
                        return false;
                    }
                }
                else
                {
                    InnerCommand.VerifyControl.SendMsg("空条码不下载方案");
                    return false;
                }

            }
            catch (Exception ex)
            {
                InnerCommand.VerifyControl.SendMsg($"下载方案异常，{ex.Message}");
                return false;
            }
        }
        public bool DownSchemeInfoFromMis(string SchemeID)
        {
            TestMeterInfo meter = VerifyBase.OneMeterInfo;

            IMis mis = MISFactory.Create();
            if (mis is Mis.NanRui.NanRui nr)
            {
                bool bOK;

                bOK = nr.SchemeDownBySchemeID(SchemeID, out string schemeName, out Dictionary<string, SchemaNode> Schema);


                if (bOK)
                {
                    DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == schemeName);
                    if (modelTemp != null)   //说明本地已经有这个方案了，切换方案就好
                    {
                        EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                        //TODO:判断内容变更
                    }
                    else  //没有找到方案，进行创建方案
                    {
                        EquipmentData.SchemaModels.NewName = schemeName;
                        EquipmentData.SchemaModels.AddDownSchema(); //这个需要重写一个方法，刷新方案放在背后，并且把选中等方法添加到里面
                        modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == EquipmentData.SchemaModels.NewName);
                        EquipmentData.SchemaModels.SelectedSchema = modelTemp;
                        System.Threading.Thread.Sleep(200);
                        foreach (var key in Schema.Keys)
                        {
                            if (!EquipmentData.Schema.ExistNode(key))
                            {
                                SchemaNodeViewModel nodeNew = EquipmentData.Schema.AddParaNode(key);//根据方案的编号，添加进了节点
                                EquipmentData.Schema.ParaValuesView.Clear();//删除默认值的方案
                            }
                            List<string> propertyNames = new List<string>();

                            for (int j = 0; j < EquipmentData.Schema.ParaInfo.CheckParas.Count; j++)
                            {
                                propertyNames.Add(EquipmentData.Schema.ParaInfo.CheckParas[j].ParaDisplayName);
                            }

                            for (int i = 0; i < Schema[key].SchemaNodeValue.Count; i++)
                            {
                                DynamicViewModel viewModel2 = new DynamicViewModel(propertyNames, 0);
                                viewModel2.SetProperty("IsSelected", true);
                                string[] value = Schema[key].SchemaNodeValue[i].Split('|');
                                for (int j = 0; j < propertyNames.Count; j++)
                                {
                                    viewModel2.SetProperty(propertyNames[j], value[j]); //这里改成参数的值
                                }
                                EquipmentData.Schema.ParaValuesView.Add(viewModel2);
                            }
                            EquipmentData.Schema.SelectedNode.ParaValuesCurrent.Clear();
                            EquipmentData.Schema.SelectedNode.ParaValuesCurrent.AddRange(EquipmentData.Schema.ParaValuesConvertBack());
                            //EquipmentData.Schema.SaveDownParaValue();    //保存方案
                            EquipmentData.Schema.RefreshPointCount();
                        }
                        EquipmentData.Schema.SaveParaValue();    //保存方案
                        EquipmentData.SchemaModels.RefreshCurrrentSchema();
                    }
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region 电表地址
        bool IsReadMeterAddres = false;
        /// <summary>
        /// 读取电表地址
        /// </summary>
        public System.Threading.Tasks.Task Read_Meter_Addres()
        {
            if (IsReadMeterAddres) return null;
            return System.Threading.Tasks.Task.Factory.StartNew(() => IsReadAddress());
        }

        /// <summary>
        /// 探测地址的
        /// </summary>
        /// <param name="meterInfos"></param>
        public void IsReadAddress()
        {
            DeviceControlS device = new DeviceControlS();
            int index = -1;
            for (int i = 0; i < Meters.Count; i++)
            {
                if (YaoJian[i])
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                LogManager.AddMessage("没有要检的表", EnumLogSource.用户操作日志, EnumLevel.Warning);
                return;
            }
            VerifyBase.MeterInfo = GetVerifyMeterInfo();  //把表信息给到检定系统
            LogManager.AddMessage("开始升源", EnumLogSource.用户操作日志, EnumLevel.Information);
            Cus_PowerYuanJian ele = Cus_PowerYuanJian.H;
            if (GetMeterInfo(index, "MD_WIRING_MODE") == "单相") ele = Cus_PowerYuanJian.A;
            float ub = float.Parse(GetMeterInfo(index, "MD_UB"));
            if (EquipmentData.Equipment.EquipmentType != "单相台")
            {
                bool hgq = GetMeterInfo(index, "MD_CONNECTION_FLAG") == "互感式";
                //三相直接和互感器切换，用第127表位控制继电器//20231115以后不用
                if (hgq)
                {
                    EquipmentData.DeviceManager.ControlHGQRelay(true);
                    EquipmentData.DeviceManager.ControlMeterRelay(1, Convert.ToByte(127), 0);  //继电器切换到经互感器
                }
                else
                {
                    EquipmentData.DeviceManager.ControlHGQRelay(false);
                    EquipmentData.DeviceManager.ControlMeterRelay(2, Convert.ToByte(127), 0);  //继电器切换到直接接入
                }
                System.Threading.Thread.Sleep(50);
            }

            //if (ConfigHelper.Instance.HasCurrentSwitch)
            //{
            //    LogManager.AddMessage("正在设置表位继电器...", EnumLogSource.用户操作日志, EnumLevel.Information);
            //    EquipmentData.DeviceManager.ControlYaoJianPositions();
            //}
            if (ConfigHelper.Instance.HasVoltageSwitch)
            {
                LogManager.AddMessage("正在设置电压继电器...", EnumLogSource.用户操作日志, EnumLevel.Information);
                EquipmentData.DeviceManager.ControlULoadPositions(YaoJian);
            }

            device.PowerOn(ub, ub, ub, 0, 0, 0, ele, PowerWay.正向有功, "1.0"); //升个电压
            int wait = ConfigHelper.Instance.Dgn_PowerSourceStableTime;
            if (wait < 10) wait = 10;
            for (int i = 0; i < wait; i++)
            {
                System.Threading.Thread.Sleep(1000);
            }

            LogManager.AddMessage("正在读取电表地址", EnumLogSource.用户操作日志, EnumLevel.Information);
            EquipmentData.Controller.UpdateMeterProtocol();
            IsReadMeterAddres = true;
            //地址
            string[] result = new string[Meters.Count];
            try
            {
                //表号
                string[] BarCodeResoult = new string[Meters.Count];
                //有功等级
                string[] ActiveLevelresoult = new string[Meters.Count];
                //无功等级
                string[] ReactiveLevelresoult = new string[Meters.Count];
                //有功常数
                string[] ActiveConstantResoult = new string[Meters.Count];
                //无功常数
                string[] ReactiveConstantResoult = new string[Meters.Count];
                //电表型号
                string[] ModelNameResoult = new string[Meters.Count];
                //The asset number 资产编号
                string[] AssetNumber = new string[Meters.Count];

                CheckController.MulitThread.MulitThreadManager.Instance.MaxThread = Meters.Count;
                CheckController.MulitThread.MulitThreadManager.Instance.MaxTaskCountPerThread = 1;
                CheckController.MulitThread.MulitThreadManager.Instance.DoWork = delegate (int pos)
                {
                    if (YaoJian[pos])
                    {
                        result[pos] = MeterProtocolAdapter.Instance.ReadAddress(pos);
                        //Meters[pos].SetProperty("MD_POSTAL_ADDRESS", result[pos]);//通讯地址
                        if (EquipmentData.IsReadMeterInfo == 3)
                        {
                            BarCodeResoult[pos] = MeterProtocolAdapter.Instance.ReadDataPos("表号", pos);//表号
                            Meters[pos].SetProperty("MD_BAR_CODE", BarCodeResoult[pos]);//表号

                            AssetNumber[pos] = MeterProtocolAdapter.Instance.ReadDataPos("资产编号", pos);
                            Meters[pos].SetProperty("MD_ASSET_NO", BarCodeResoult[pos]);//资产编号

                            ActiveLevelresoult[pos] = MeterProtocolAdapter.Instance.ReadDataPos("有功等级", pos);//有功等级
                            ReactiveLevelresoult[pos] = MeterProtocolAdapter.Instance.ReadDataPos("无功等级", pos);//无功等级

                            if (!string.IsNullOrWhiteSpace(ReactiveLevelresoult[pos]))
                            {
                                Meters[pos].SetProperty("MD_GRADE", $"{ActiveLevelresoult[pos]}({ReactiveLevelresoult[pos]})");//等级
                            }
                            else
                            {
                                Meters[pos].SetProperty("MD_GRADE", ActiveLevelresoult[pos]);//等级
                            }

                            ActiveConstantResoult[pos] = MeterProtocolAdapter.Instance.ReadDataPos("有功常数", pos);//有功常数
                            ReactiveConstantResoult[pos] = MeterProtocolAdapter.Instance.ReadDataPos("无功常数", pos);//无功常数
                            if (!string.IsNullOrWhiteSpace(ReactiveConstantResoult[pos]))
                            {
                                if (!string.IsNullOrWhiteSpace(ActiveConstantResoult[pos]))
                                {
                                    SelectInsertT_CODE_TREE("MD_CONSTANT", $"{ActiveConstantResoult[pos]}({ReactiveConstantResoult[pos]})", "3", "ConstantValue");
                                }
                                Meters[pos].SetProperty("MD_CONSTANT", $"{ActiveConstantResoult[pos]}({ReactiveConstantResoult[pos]})");//常数
                            }
                            else
                            {
                                SelectInsertT_CODE_TREE("MD_CONSTANT", ActiveConstantResoult[pos], "3", "ConstantValue");
                                Meters[pos].SetProperty("MD_CONSTANT", ActiveConstantResoult[pos]);//常数
                            }

                            ModelNameResoult[pos] = MeterProtocolAdapter.Instance.ReadDataPos("电表型号", pos);//电表型号
                            SelectInsertT_CODE_TREE("MD_METER_MODEL", ModelNameResoult[pos], "3", "MeterModel");
                            Meters[pos].SetProperty("MD_METER_MODEL", ModelNameResoult[pos]);//电表型号

                        }
                    }
                };
                CheckController.MulitThread.MulitThreadManager.Instance.Start();



                while (true)
                {
                    if (CheckController.MulitThread.MulitThreadManager.Instance.IsWorkDone())
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(50);
                }
                if (ConfigHelper.Instance.VerifyModel != "自动模式")
                {
                    string tips = "";
                    for (int i = 0; i < Meters.Count; i++)
                    {
                        if (!YaoJian[i]) continue;
                        if (result[i] == "" || result[i] == null)
                        {
                            tips += (i + 1).ToString() + "号,";
                        }
                    }

                    if (tips != "")
                    {
                        tips = tips.Trim(',');
                        tips += "表位探测表地址失败，请检查。";

                    }
                    else
                    {
                        tips = "读取地址完成!";
                    }

                    LogManager.AddMessage(tips, EnumLogSource.用户操作日志, EnumLevel.Tip);



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

            // 天津用
            if (ConfigHelper.Instance.InputCheckAddress)
            {
                string err = "";
                for (int i = 0; i < Meters.Count; i++)
                {
                    if (!YaoJian[i]) continue;
                    string oldAddr = Meters[i].GetProperty("MD_POSTAL_ADDRESS").ToString();//通讯地址
                    if (oldAddr != result[i])
                    {
                        err += $"表位{i + 1}\n";
                    }
                }

                if (err.Trim().Length > 0)
                {
                    StringBuilder msg = new StringBuilder();

                    msg.Append($"以下表位地址比对失败:\n\r");
                    msg.AppendLine();
                    msg.AppendLine(err);
                    msg.Append("应用读取地址按[是]，否则按[否]");

                    MessageBox.Show(msg.ToString(), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    LogManager.AddMessage("地址读取完成", EnumLogSource.用户操作日志, EnumLevel.Information);
                }
            }
            else
            {
                for (int i = 0; i < Meters.Count; i++)
                {
                    if (!YaoJian[i]) continue;
                    Meters[i].SetProperty("MD_POSTAL_ADDRESS", result[i]);

                }
                LogManager.AddMessage("地址读取完成", EnumLogSource.用户操作日志, EnumLevel.Information);
                HandleAfterReadAddress();
            }



        }
        /// <summary>
        /// 处理获取电表地址后的逻辑
        /// </summary>
        /// <returns>处理成功标识符</returns>
        public bool HandleAfterReadAddress()
        {
            bool handleResult = true;
            string tips = string.Empty;
            for (int index = 0; index < Meters.Count; index++)
            {
                try
                {
                    // 是否从通讯地址生成出厂编号
                    if (ConfigHelper.Instance.Factory_GenerateSource.Contains("地址"))
                    {
                        string MD_POSTAL_ADDRESS = Meters[index].GetProperty("MD_POSTAL_ADDRESS") as string;
                        // 如果数据为空，则不处理
                        if (string.IsNullOrEmpty(MD_POSTAL_ADDRESS))
                        {
                            continue;
                        }
                        string MD_MADE_NO = MD_POSTAL_ADDRESS;
                        // 获取起始索引，范围[0,MD_MADE_NO.Length - 1]
                        int factorySubStartIndex = Math.Min(Math.Max(ConfigHelper.Instance.Factory_StartIndex - 1, 0), Math.Max(0, MD_MADE_NO.Length - 1));
                        // 限制截取长度，使其范围不出最大合法范围
                        int factorySubLength = Math.Min(Math.Max(ConfigHelper.Instance.Factory_Length, 0), Math.Max(MD_MADE_NO.Length - factorySubStartIndex, 0));
                        // 如果截取长度大于0
                        if (factorySubLength > 0)
                        {
                            if (ConfigHelper.Instance.Factory_LeftToRight)
                            {
                                MD_MADE_NO = MD_MADE_NO.Substring(factorySubStartIndex, factorySubLength).PadLeft(ConfigHelper.Instance.Factory_Length, '0');
                            }
                            else
                            {
                                MD_MADE_NO = MD_MADE_NO.Substring(MD_MADE_NO.Length - factorySubStartIndex - factorySubLength, factorySubLength).PadLeft(ConfigHelper.Instance.Factory_Length, '0');
                            }
                        }
                        MD_MADE_NO = ConfigHelper.Instance.Factory_Prefix + MD_MADE_NO + ConfigHelper.Instance.Factory_Suffix;
                        Meters[index].SetProperty("MD_MADE_NO", MD_MADE_NO);
                    }
                }
                catch (Exception ex)
                {
                    handleResult = false;
                    tips = $"处理读取电表地址后逻辑失败！\n错误信息：{ex.Message}";
                    LogManager.AddMessage(tips, EnumLogSource.用户操作日志, EnumLevel.Tip);
                }
            }
            return handleResult;
        }
        #endregion

        #region
        private static readonly object thisLock = new object();

        private void SelectInsertT_CODE_TREE(string propertyName, string CODE_CN_NAME, string CODE_LEVEL, string CODE_PARENT)
        {
            lock (thisLock)
            {
                try
                {
                    DynamicModel modelsFormat = DALManager.ApplicationDbDal.GetByID("T_CODE_TREE", $"(CODE_PARENT = '{CODE_PARENT}') AND (CODE_LEVEL = '{CODE_LEVEL}') AND (CODE_CN_NAME = '{CODE_CN_NAME}')  ORDER BY CODE_VALUE DESC");
                    if (modelsFormat != null)
                    {
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(CODE_CN_NAME))
                        {
                            AutoFieldName(propertyName, CODE_CN_NAME);
                        }
                    }
                }
                catch
                {
                    return;
                }
            }

        }

        #endregion
    }
}
