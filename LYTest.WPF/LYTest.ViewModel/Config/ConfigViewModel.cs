using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace LYTest.ViewModel.Config
{
    /// <summary>
    /// 配置视图模型
    /// </summary>
    public class ConfigViewModel : ViewModelBase
    {

        #region 属性

        private ConfigTreeNode treeConfig = new ConfigTreeNode();

        public ConfigTreeNode TreeConfig
        {
            get { return treeConfig; }
            set { SetPropertyValue(value, ref treeConfig, "TreeConfig"); }
        }

        private AsyncObservableCollection<ConfigGroup> groups = new AsyncObservableCollection<ConfigGroup>();

        public AsyncObservableCollection<ConfigGroup> Groups
        {
            get { return groups; }
            set { groups = value; }
        }


        private ConfigTreeNode currentNode;

        public ConfigTreeNode CurrentNode
        {
            get { return currentNode; }
            set
            {
                SetPropertyValue(value, ref currentNode, "CurrentNode");
                //选中项改变，载入当前选中的数据
                LoadConfigFormat();
                LoadGroups();
            }
        }

        private AsyncObservableCollection<ConfigInfo> infos = new AsyncObservableCollection<ConfigInfo>();
        public AsyncObservableCollection<ConfigInfo> Infos
        {
            get { return infos; }
            set { SetPropertyValue(value, ref infos, "Infos"); }
        }

        #endregion

        public ConfigViewModel()
        {
            Initialize();
        }

        public void Initialize()
        {
            TreeConfig.Children.Clear();
            Dictionary<string, string> dictionary = CodeDictionary.GetLayer2("SystemConfig");
            foreach (string codeName in dictionary.Keys)
            {
                ConfigTreeNode node = new ConfigTreeNode
                {
                    ConfigName = codeName,
                    ConfigNo = dictionary[codeName]
                };
                string code = CodeDictionary.GetCodeLayer1(codeName);
                Dictionary<string, string> dictionary1 = CodeDictionary.GetLayer2(code);
                foreach (string codeName1 in dictionary1.Keys)
                {
                    node.Children.Add(
                    new ConfigTreeNode
                    {
                        ConfigName = codeName1,
                        ConfigNo = $"{node.ConfigNo.PadLeft(2, '0')}{dictionary1[codeName1].PadLeft(3, '0')}"
                    }
                  );
                }
                TreeConfig.Children.Add(node);
            }
        }

        #region 配置编辑
        /// 加载当前的配置
        /// <summary>
        /// 加载当前的配置
        /// </summary>
        public void LoadGroups()
        {
            Groups.Clear();
            List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), string.Format("CONFIG_NO='{0}'", CurrentNode.ConfigNo));
            for (int i = 0; i < models.Count; i++)
            {
                ConfigGroup group = new ConfigGroup
                {
                    ID = (int)models[i].GetProperty("ID")
                };
                string stringValue = models[i].GetProperty("CONFIG_VALUE") as string;
                string[] values = stringValue.Split('|');
                for (int j = 0; j < Infos.Count; j++)
                {
                    string temp = "";
                    if (values.Length > j)
                    {
                        temp = values[j];
                    }
                    ConfigUnit unit = new ConfigUnit
                    {
                        Name = Infos[j].Name,
                        Code = Infos[j].Code,
                        ConfigValue = temp,
                        IsCombo = !string.IsNullOrEmpty(Infos[j].Code)
                    };
                    unit.PropertyChanged += (sender, e) =>
                    {
                        group.ChangeFlag = true;
                        group.OnPropertyChanged("StringValue");
                    };
                    group.ChangeFlag = false;
                    group.Units.Add(unit);
                }
                Groups.Add(group);
            }
        }

        public void AddGroup()
        {
            ConfigGroup group = new ConfigGroup
            {
                ID = 0
            };
            for (int i = 0; i < Infos.Count; i++)
            {
                ConfigUnit unit = new ConfigUnit
                {
                    Name = Infos[i].Name,
                    Code = Infos[i].Code,
                    ConfigValue = infos[i].DefaultValue,
                    IsCombo = !string.IsNullOrEmpty(Infos[i].Code)
                };
                group.Units.Add(unit);
            }
            group.ChangeFlag = true;
            Groups.Add(group);
        }
        public void DeleteGroup(ConfigGroup group)
        {
            Groups.Remove(group);
            if (group.ID != 0)
            {
                if (DALManager.ApplicationDbDal.Delete(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), string.Format("ID={0}", group.ID)) > 0)
                {
                    LogManager.AddMessage("配置项删除成功!", EnumLogSource.数据库存取日志);
                }
                else
                {
                    LogManager.AddMessage("配置项删除失败,请检查原因并重试!", EnumLogSource.数据库存取日志, EnumLevel.Warning);
                }
            }
        }
        public void SaveGroup(ConfigGroup configGroup)
        {
            DynamicModel model = new DynamicModel();
            model.SetProperty("CONFIG_NO", CurrentNode.ConfigNo);
            model.SetProperty("CONFIG_VALUE", configGroup.StringValue);
            int result;
            if (configGroup.ID == 0)
            {
                result = DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), model);
            }
            else
            {
                result = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), string.Format("ID={0}", configGroup.ID), model, new List<string> { "CONFIG_NAME", "CONFIG_VALUE" });

                if (model != null)
                {


                    if (CurrentNode.ConfigNo == "01001")//基本设置
                    {
                        string AVR_DEVICE_ID = configGroup.StringValue.Split('|')[0];    //台子编号
                        string AVR_EQUIP_TYPE = configGroup.StringValue.Split('|')[1];   //台子类型--三相单相
                        string LNG_BENCH_NUM = configGroup.StringValue.Split('|')[2];    //表位数量

                        string where1 = "1 = 1";
                        DynamicModel Model2 = new DynamicModel();
                        Model2.SetProperty("MD_DEVICE_ID", AVR_DEVICE_ID);
                        Model2.SetProperty("LAST_DEVICE_ID", AVR_DEVICE_ID);
                        Model2.SetProperty("AVR_EQUIP_TYPE", AVR_EQUIP_TYPE);
                        Model2.SetProperty("LNG_BENCH_NUM", LNG_BENCH_NUM);
                        DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_DEVICE_ID" });//修改临时数据库编号
                        DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_LAST_INFO.ToString(), where1, Model2, new List<string> { "LAST_DEVICE_ID" });//修改最后一次登入数据

                    }
                    else if (CurrentNode.ConfigNo == "01004")    //日志设置
                    {
                        //这里需要保存一下设备日志的俩个参数
                        //SetKey
                        string p = System.IO.Directory.GetCurrentDirectory() + "\\Log\\Config\\Device.config";
                        string IsDebug = configGroup.StringValue.Split('|')[5] == "是" ? "true" : "false";    //控制台输出日志
                        string IsSave = configGroup.StringValue.Split('|')[4] == "是" ? "true" : "false";//保存设备日志
                        SetKey(p, "IsDebugLog", IsDebug);
                        SetKey(p, "IsSaveLog", IsSave);
                    }
                }
            }
            if (result > 0)
            {
                LogManager.AddMessage(string.Format("配置信息:{0} 保存成功!", currentNode.ConfigNo));
                configGroup.ChangeFlag = false;
            }
            else
            {
                LogManager.AddMessage(string.Format("配置信息:{0} 保存失败!", currentNode.ConfigNo), EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
        }
        #endregion

        #region 加载当前配置的配置项
        public void AddConfigInfo()
        {
            Infos.Add(new ConfigInfo());
        }
        public void SaveConfigInfo()
        {
            if (CurrentNode == null)
            {
                return;
            }
            for (int i = 0; i < Infos.Count; i++)
            {
                if (string.IsNullOrEmpty(Infos[i].Name))
                {
                    LogManager.AddMessage("配置项名称不能为空!", EnumLogSource.用户操作日志, EnumLevel.Warning);
                    return;
                }
            }
            var names = from item in Infos select item.Name;
            var codes = from item in Infos select item.Code;
            var defaultValues = from item in infos select item.DefaultValue;
            DynamicModel model = new DynamicModel();
            model.SetProperty("CONFIG_NO", CurrentNode.ConfigNo);
            model.SetProperty("CONFIG_VIEW", string.Join("|", names));
            model.SetProperty("CONFIG_CODE", string.Join("|", codes));
            model.SetProperty("CONFIG_DEFAULT_VALUE", string.Join("|", defaultValues));
            string where = string.Format("CONFIG_NO = '{0}'", currentNode.ConfigNo);
            int result = 0;
            if (DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.T_CONFIG_PARA_FORMAT.ToString(), where) > 0)
            {
                result = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_CONFIG_PARA_FORMAT.ToString(), where, model, new List<string> { "CONFIG_VIEW", "CONFIG_CODE", "CONFIG_DEFAULT_VALUE" });
            }
            else
            {
                result = DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_CONFIG_PARA_FORMAT.ToString(), model);
            }
            if (result > 0)
            {
                LogManager.AddMessage("配置项保存完毕!");

                if (Groups.Count == 0)
                {
                    AddGroup();
                    ConfigGroup group = Groups[0];
                    SaveGroup(group);
                }
            }
            else
            {
                LogManager.AddMessage("配置项保存失败!", EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
        }




        /// <summary>
        /// 载入当前配置格式源
        /// </summary>
        public void LoadConfigFormat()
        {
            Infos.Clear();
            string where = string.Format("CONFIG_NO = '{0}'", currentNode.ConfigNo);
            DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_CONFIG_PARA_FORMAT.ToString(), where);
            if (model == null)
            {
                return;
            }
            string viewString = model.GetProperty("CONFIG_VIEW") as string;
            string codeString = model.GetProperty("CONFIG_CODE") as string;
            string defaultValueString = model.GetProperty("CONFIG_DEFAULT_VALUE") as string;
            codeString = codeString ?? "";
            viewString = viewString ?? "";
            defaultValueString = defaultValueString ?? "";
            string[] viewArray = viewString.Split('|');
            string[] codeArray = codeString.Split('|');
            string[] defaultValueArray = defaultValueString.Split('|');
            for (int i = 0; i < viewArray.Length; i++)
            {
                string code = codeArray[i];
                string codeName = "";
                if (!string.IsNullOrEmpty(code))
                {
                    codeName = CodeDictionary.GetNameLayer1(code);
                }
                Infos.Add(new ConfigInfo
                {
                    Name = viewArray[i],
                    CodeName = codeName,
                    DefaultValue = (defaultValueArray.Length > i) ? defaultValueArray[i] : "",
                });
            }
        }

        public static bool SetKey(string configPath, string key, string vls)
        {
            try
            {
                Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = configPath
                }, ConfigurationUserLevel.None);

                if (ConfigurationInstance.AppSettings.Settings[key] != null)
                    ConfigurationInstance.AppSettings.Settings[key].Value = vls;
                else
                    ConfigurationInstance.AppSettings.Settings.Add(key, vls);
                ConfigurationInstance.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void DeleteConfigInfo(ConfigInfo info)
        {
            Infos.Remove(info);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            for (int i = 0; i < treeConfig.Children.Count; i++)
            {
                TreeConfig.Children[i].Children.Clear();
            }
            TreeConfig.Children.Clear();
            Groups.Clear();
            base.Dispose(disposing);
        }
    }
}
