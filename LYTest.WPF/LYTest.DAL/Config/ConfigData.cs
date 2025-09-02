using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LYTest.DAL.Config
{
    public class ConfigData
    {
        private readonly GeneralDal dbDal;
        public ConfigData(GeneralDal generalDal)
        {
            dbDal = generalDal;
        }

        /// <summary>
        /// 配置信息列表
        /// </summary>
        private readonly ConcurrentDictionary<EnumConfigId, List<string>> configDictionary = new ConcurrentDictionary<EnumConfigId, List<string>>();

        /// 从数据库加载所有配置信息
        /// <summary>
        /// 从数据库加载所有配置信息
        /// </summary>
        public void LoadAllConfig()
        {
            configDictionary.Clear();
            List<DynamicModel> models = dbDal.GetList(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString());
            for (int i = 0; i < models.Count; i++)
            {
                string stringValue = models[i].GetProperty("CONFIG_VALUE") as string;
                if (models[i].GetProperty("CONFIG_NO") is string configNo)
                {
                    configNo = configNo.TrimStart('0');
                    Enum.TryParse(configNo, out EnumConfigId configId);
                    if (configDictionary.ContainsKey(configId))
                    {
                        configDictionary[configId].Add(stringValue);
                    }
                    else
                    {
                        configDictionary.TryAdd(configId, new List<string> { stringValue });
                    }
                }
            }
            List<DynamicModel> modelsFormat = dbDal.GetList(EnumAppDbTable.T_CONFIG_PARA_FORMAT.ToString());
            for (int i = 0; i < modelsFormat.Count; i++)
            {
                string defaultValue = modelsFormat[i].GetProperty("CONFIG_DEFAULT_VALUE") as string;
                if (modelsFormat[i].GetProperty("CONFIG_NO") is string configNo)
                {
                    configNo = configNo.TrimStart('0');
                    Enum.TryParse(configNo, out EnumConfigId configId);
                    if (!dictionaryFormat.ContainsKey(configId))
                    {
                        dictionaryFormat.Add(configId, defaultValue);
                    }
                }
            }
        }

        public List<string> GetConfig(EnumConfigId configId)
        {
            if (configDictionary.ContainsKey(configId))
            {
                return configDictionary[configId];
            }
            else
            {
                return new List<string>();
            }
        }

        public string GetConfigString(EnumConfigId configId)
        {
            if (configDictionary.ContainsKey(configId))
            {
                List<string> valueList = configDictionary[configId];
                if (valueList.Count > 0)
                {
                    return valueList[0];
                }
            }
            return "";
        }

        #region 获取值
        /// <summary>
        /// 获取配置值,如果获取失败,取默认值
        /// </summary>
        /// <param name="configId">配置编号</param>
        /// <param name="indexTemp">值序号</param>
        /// <returns></returns>
        public string GetConfigString(EnumConfigId configId, int indexTemp)
        {
            string stringTemp = GetConfigString(configId);
            try
            {
                if (stringTemp.Split('|').Length > indexTemp)
                    return stringTemp.Split('|')[indexTemp];
                else
                    return GetDefaultValue(configId, indexTemp);
            }
            catch
            {
                return GetDefaultValue(configId, indexTemp);
            }
        }
        /// <summary>
        /// 获取配置值,如果获取失败,取默认值
        /// </summary>
        /// <param name="configId">配置编号</param>
        /// <param name="indexTemp">值序号</param>
        /// <returns></returns>
        public bool GetConfigBool(EnumConfigId configId, int indexTemp, bool initialValue)
        {
            string stringTemp = GetConfigString(configId);
            try
            {
                string boolString;
                if (stringTemp.Split('|').Length > indexTemp)
                    boolString = stringTemp.Split('|')[indexTemp];
                else
                    boolString = GetDefaultValue(configId, indexTemp);
                if (boolString == "是")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                string boolString = GetDefaultValue(configId, indexTemp);
                if (boolString == "是")
                {
                    return true;
                }
                else if (boolString == "否")
                {
                    return false;
                }
                else
                {
                    return initialValue;
                }
            }
        }
        /// <summary>
        /// 获取配置值,如果获取失败,取默认值
        /// </summary>
        /// <param name="configId">配置编号</param>
        /// <param name="indexTemp">值序号</param>
        /// <returns></returns>
        public int GetConfigInt(EnumConfigId configId, int indexTemp, int initialValue)
        {
            string stringTemp = GetConfigString(configId);
            try
            {
                string intString = stringTemp.Split('|')[indexTemp];
                return int.Parse(intString);
            }
            catch
            {
                string intString = GetDefaultValue(configId, indexTemp);
                if (!int.TryParse(intString, out int b))
                {
                    return initialValue;
                }
                return b;
            }
        }
        /// <summary>
        /// 获取配置值,如果获取失败,取默认值
        /// </summary>
        /// <param name="configId">配置编号</param>
        /// <param name="indexTemp">值序号</param>
        /// <returns></returns>
        public float GetConfigFloat(EnumConfigId configId, int indexTemp, float initialValue)
        {
            string stringTemp = GetConfigString(configId);
            try
            {
                if (stringTemp.Split('|').Length > indexTemp)
                {
                    string floatString = stringTemp.Split('|')[indexTemp];
                    return float.Parse(floatString);
                }
                else
                {
                    string floatString = GetDefaultValue(configId, indexTemp);
                    if (!float.TryParse(floatString, out float floatTemp))
                    {
                        return initialValue;
                    }
                    return floatTemp;
                }
            }
            catch
            {
                string floatString = GetDefaultValue(configId, indexTemp);
                if (!float.TryParse(floatString, out float floatTemp))
                {
                    return initialValue;
                }
                return floatTemp;
            }
        }
        #endregion

        private void SaveConfigValue(EnumConfigId configId, string configValue)
        {
            if (configDictionary.ContainsKey(configId))
            {
                configDictionary[configId].Insert(0, configValue);
            }
            //string sql = string.Format("update {0} set config_value = '{1}' where config_no='{2}'", EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), configValue, ((int)configId).ToString().PadLeft(5, '0'));
            //dbDal.ExecuteOperation(new List<string> { sql });
        }
        /// <summary>
        /// 保存配置值
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="indexTemp"></param>
        /// <param name="objTemp"></param>
        private void SaveConfigValue(EnumConfigId configId, int indexTemp, object objTemp)
        {
            string temp = GetConfigString(configId);
            string[] arrayTemp = temp.Split('|');
            if (arrayTemp.Length > indexTemp)
            {
                string valueTemp = objTemp == null ? "" : objTemp.ToString();
                if (objTemp is bool b)
                {
                    valueTemp = b ? "是" : "否";
                }
                arrayTemp[indexTemp] = valueTemp;
                temp = string.Join("|", arrayTemp);
                SaveConfigValue(configId, temp);
            }
        }

        #region 获取默认值

        private readonly Dictionary<EnumConfigId, string> dictionaryFormat = new Dictionary<EnumConfigId, string>();


        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="indexTemp"></param>
        /// <returns></returns>
        private string GetDefaultValue(EnumConfigId configId, int indexTemp)
        {
            string strResult = "";
            if (dictionaryFormat.ContainsKey(configId))
            {
                string valueTemp = dictionaryFormat[configId];
                if (valueTemp != null)
                {
                    string[] arrayDefault = valueTemp.Split('|');
                    if (arrayDefault.Length > indexTemp)
                    {
                        return arrayDefault[indexTemp];
                    }
                }
            }
            return strResult;
        }
        #endregion


        #region 软件配置

        #region 装置信息

        #region 基本信息
        public string EquipmentNo    //检定台的编号
        {
            get => GetConfigString(EnumConfigId.基本信息, 0);
            set => SaveConfigValue(EnumConfigId.基本信息, 0, value);
        }
        public string EquipmentType  //单相台还是三相台
        {
            get => GetConfigString(EnumConfigId.基本信息, 1);
            set => SaveConfigValue(EnumConfigId.基本信息, 1, value);
        }
        public int MeterCount   /// 表位的数量
        {
            get => GetConfigInt(EnumConfigId.基本信息, 2, 24);
            set => SaveConfigValue(EnumConfigId.基本信息, 2, value);
        }
        /// <summary>
        /// 检测表类型--终端还是电表
        /// </summary>
        public string MeterType
        {
            get => GetConfigString(EnumConfigId.基本信息, 3);
            set => SaveConfigValue(EnumConfigId.基本信息, 3, value);
        }

        /// <summary>
        /// 检定模式-自动模式--手动模式
        /// </summary>
        public string VerifyModel
        {
            get => GetConfigString(EnumConfigId.基本信息, 4);
            set => SaveConfigValue(EnumConfigId.基本信息, 4, value);
        }

        /// <summary>
        /// 自动登入
        /// </summary>
        public bool AutoLogin
        {
            get => GetConfigBool(EnumConfigId.基本信息, 5, false);
            set => SaveConfigValue(EnumConfigId.基本信息, 5, value);
        }
        /// <summary>
        /// 是否弹出错误提示界面
        /// </summary>
        public bool IsShowErrorTips
        {
            get => GetConfigBool(EnumConfigId.基本信息, 6, false);
            set => SaveConfigValue(EnumConfigId.基本信息, 6, value);
        }

        //add yjt 20220520 新增密钥更新前是否弹出提示
        /// <summary>
        /// 密钥更新前是否弹出提示 
        /// </summary>
        public string IsKeyUpdate
        {
            get => GetConfigString(EnumConfigId.基本信息, 7);
            set => SaveConfigValue(EnumConfigId.基本信息, 7, value);
        }

        /// <summary>
        /// 流水线方向
        /// </summary>
        public string PipelineDirection
        {
            get => GetConfigString(EnumConfigId.基本信息, 8);
            set => GetConfigString(EnumConfigId.基本信息, 8);
        }

        /// <summary>
        /// 参数录入时验证表地址
        /// </summary>
        public bool InputCheckAddress
        {
            get => GetConfigBool(EnumConfigId.基本信息, 9, false);
            set => SaveConfigValue(EnumConfigId.基本信息, 9, value);
        }

        /// <summary>
        /// 台体功能
        /// :工频磁场台体，辐射电磁场传导抗扰度试验
        /// </summary>
        public string BenthFun
        {
            get
            {
                return GetConfigString(EnumConfigId.基本信息, 10);
            }
            set
            {
                SaveConfigValue(EnumConfigId.基本信息, 10, value);
            }
        }
        #endregion

        #region 区域信息
        /// <summary>
        ///地区名称
        /// </summary>
        public string AreaName
        {
            get
            {
                return GetConfigString(EnumConfigId.地区信息, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.地区信息, 0, value);
            }
        }
        /// <summary>
        /// 误差限比例
        /// </summary>
        public string ErrorRatio
        {
            get
            {
                return GetConfigString(EnumConfigId.地区信息, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.地区信息, 1, value);
            }
        }
        #endregion

        #region 显示设置
        /// <summary>
        /// 是否显示版本号
        /// </summary>
        public bool IsVersionNumber
        {
            get
            {
                return GetConfigBool(EnumConfigId.显示设置, 0, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.显示设置, 0, value);
            }
        }
        /// <summary>
        /// 是否显示装置编号
        /// </summary>
        public bool IsDeviceNumber
        {
            get
            {
                return GetConfigBool(EnumConfigId.显示设置, 1, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.显示设置, 1, value);
            }
        }

        /// <summary>
        /// 开始检定按钮是否悬浮 --是就是悬浮
        /// </summary>
        public bool IsTestButtonSuspension = false;
        //{
        //    get
        //    {
        //        return GetConfigBool(EnumConfigId.显示设置, 2, false);
        //    }
        //    set
        //    {
        //        SaveConfigValue(EnumConfigId.显示设置, 2, value);
        //    }
        //}

        /// <summary>
        /// 检定界面误差显示 平均值/化整值
        /// </summary>
        public string VerifyUIErrorShow
        {
            get
            {
                string temp = GetConfigString(EnumConfigId.显示设置, 3);
                if (string.IsNullOrWhiteSpace(temp))
                    return "平均值";
                else
                    return temp;
            }
            set
            {
                SaveConfigValue(EnumConfigId.显示设置, 3, value);
            }
        }
        /// <summary>
        /// 参数录入显示无功列
        /// </summary>
        public bool InputUIShowReactive
        {
            get
            {
                bool temp = GetConfigBool(EnumConfigId.显示设置, 4, false);
                return temp;
            }
            set
            {
                SaveConfigValue(EnumConfigId.显示设置, 4, value);
            }
        }
        #endregion

        #region 日志设置
        /// <summary>
        /// 是否打开【流程】日志
        /// </summary>
        public bool IsOpenLog_Process
        {
            get => GetConfigBool(EnumConfigId.日志设置, 0, true);
            set => SaveConfigValue(EnumConfigId.日志设置, 0, value);
        }
        /// <summary>
        /// 是否打开【提示】日志
        /// </summary>
        public bool IsOpenLog_Tips
        {
            get => GetConfigBool(EnumConfigId.日志设置, 1, false);
            set => SaveConfigValue(EnumConfigId.日志设置, 1, value);
        }
        /// <summary>
        /// 是否打开【详细】日志
        /// </summary>
        public bool IsOpenLog_Detailed
        {
            get => GetConfigBool(EnumConfigId.日志设置, 2, false);
            set => SaveConfigValue(EnumConfigId.日志设置, 2, value);
        }
        /// <summary>
        /// 是否打开【表位通讯帧】日志
        /// </summary>
        public bool IsOpenLog_MeterFrame
        {
            get => GetConfigBool(EnumConfigId.日志设置, 3, false);
            set => SaveConfigValue(EnumConfigId.日志设置, 3, value);
        }
        //设备日志和设备日志输出到控制台单独到一边
        #endregion

        #endregion

        #region 检定设置

        #region 标准器设置
        /// <summary>
        /// 常数模式--是：固定常数；否：自动常数
        /// </summary>
        public bool FixedConstant
        {
            get
            {
                return GetConfigBool(EnumConfigId.标准器设置, 0, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.标准器设置, 0, value);
            }
        }

        /// <summary>
        /// 挡位模式：是：自动挡位；否：手动挡位；建议小电流0.01A一下用手动档，其他用自动档
        /// </summary>
        public bool AutoGear
        {
            get
            {
                return GetConfigBool(EnumConfigId.标准器设置, 1, true);
            }
            set
            {
                SaveConfigValue(EnumConfigId.标准器设置, 1, value);
            }
        }
        /// <summary>
        /// 特定试验档位控制，true自动档，运行变量,一般保持true
        /// </summary>
        public bool AutoGearTemp { get; set; } = true;

        /// <summary>
        /// 是否读取标准表数据（是的话联机成功会时时读取标准表数据）
        /// </summary>
        public bool IsReadStd
        {
            get
            {
                return GetConfigBool(EnumConfigId.标准器设置, 2, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.标准器设置, 2, value);
            }
        }


        /// <summary>
        ///标准表读取间隔
        /// </summary>
        public int Std_RedInterval
        {
            get
            {
                return GetConfigInt(EnumConfigId.标准器设置, 3, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.标准器设置, 3, value);
            }
        }

        /// <summary>
        ///标准表固定常数
        /// </summary>
        public ulong Std_Const
        {
            get
            {
                if (ulong.TryParse(GetConfigString(EnumConfigId.标准器设置, 4), out ulong refconst))
                    return refconst;
                else
                    return 1000000;
            }
            set
            {
                SaveConfigValue(EnumConfigId.标准器设置, 4, value);
            }
        }

        //add yjt 20220805 新增判断连续检定下切换点位是否关闭误差点的电流的参数
        /// <summary>
        ///连续检定切换点位是否关电流
        /// </summary>
        public bool Std_Switch_I
        {
            get
            {
                return GetConfigBool(EnumConfigId.标准器设置, 5, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.标准器设置, 5, true);
            }
        }
        #endregion

        #region 多功能检定配置
        /// <summary>
        ///功率源稳定时间
        /// </summary>
        public int Dgn_PowerSourceStableTime
        {
            get
            {
                return GetConfigInt(EnumConfigId.多功能检定配置, 0, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 0, value);
            }
        }

        /// <summary>
        ///  写操作时提示
        /// </summary>
        public bool Dgn_WriteTips
        {
            get
            {
                return GetConfigBool(EnumConfigId.多功能检定配置, 1, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 1, value);
            }
        }

        /// <summary>
        ///  走字电量输入方式--true:自动读取  false：手动输入
        /// </summary>
        public bool Dgn_ZZStartElectricQuantityModel
        {
            get
            {
                return GetConfigBool(EnumConfigId.多功能检定配置, 2, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 2, value);
            }
        }

        /// <summary>
        /// 走字试验前电表清零
        /// </summary>
        public bool Dgn_ZZTestClear
        {
            get
            {
                return GetConfigBool(EnumConfigId.多功能检定配置, 3, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 3, value);
            }
        }

        /// <summary>
        ///  清零前对时
        /// </summary>
        public bool Dgn_ClearFrontTiming
        {
            get
            {
                return GetConfigBool(EnumConfigId.多功能检定配置, 4, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 4, value);
            }
        }

        /// <summary>
        ///  日计时测量值 s/d  Hz
        /// </summary>
        public string Dgn_DayClockTestValue
        {
            get
            {
                string temp = GetConfigString(EnumConfigId.多功能检定配置, 5);
                if (string.IsNullOrWhiteSpace(temp)) return "s/d";
                else return temp;
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 5, value);
            }
        }

        /// <summary>
        ///  做最大需量时清零
        /// </summary>
        public bool Dgn_ClearWhenMaxDemand
        {
            get
            {
                return GetConfigBool(EnumConfigId.多功能检定配置, 6, true);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 6, value);
            }
        }

        /// <summary>
        ///  采用采用高精度电量
        /// </summary>
        public bool Dgn_UseHighPrecision
        {
            get
            {
                return GetConfigBool(EnumConfigId.多功能检定配置, 7, true);
            }
            set
            {
                SaveConfigValue(EnumConfigId.多功能检定配置, 7, value);
            }
        }


        #endregion

        #region 检定有效期
        /// <summary>
        /// 检定有效期
        /// </summary>
        public string TestEffectiveTime
        {
            get
            {
                return GetConfigString(EnumConfigId.检定有效期, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定有效期, 0, value);
            }
        }
        #endregion

        #region 检定配置
        /// <summary>
        /// 误差计算取值数
        /// </summary>
        public int ErrorCount
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定配置, 0, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 0, value);
            }
        }

        /// <summary>
        /// 最大处理时间
        /// </summary>
        public int MaxHandleTime
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定配置, 1, 300);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 1, value);
            }
        }

        /// <summary>
        /// 误差个数最大数
        /// </summary>
        public int ErrorMax
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定配置, 2, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 2, value);
            }
        }

        /// <summary>
        ///平均值小数位数
        /// </summary>
        public int PjzDigit
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定配置, 3, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 3, value);
            }
        }

        /// <summary>
        /// 误差起始采集次数(这个就是前面几个误差不要)
        /// </summary>
        public int ErrorStartCount
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定配置, 4, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 4, value);
            }
        }

        /// <summary>
        /// 跳差判断倍数
        /// </summary>
        public float JumpJudgment
        {
            get
            {
                return GetConfigFloat(EnumConfigId.检定配置, 5, 1f);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 5, value);
            }
        }

        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature
        {
            get
            {
                return GetConfigFloat(EnumConfigId.检定配置, 6, 20);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 6, value);
            }
        }

        /// <summary>
        /// 湿度
        /// </summary>
        public float Humidity
        {
            get
            {
                return GetConfigFloat(EnumConfigId.检定配置, 7, 60);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 7, value);
            }
        }

        /// <summary>
        /// 偏差计算取值数
        /// </summary>
        public int PcCount
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定配置, 8, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 8, value);
            }
        }

        /// <summary>
        /// 是否使用时间来计算误差圈数
        /// </summary>
        public bool IsTimeWcLapCount
        {
            get
            {
                return GetConfigBool(EnumConfigId.检定配置, 9, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定配置, 9, value);
            }
        }

        /// <summary>
        /// 出一个误差最小时间
        /// </summary>
        public float WcMinTime
        {
            get
            {
                return GetConfigFloat(EnumConfigId.检定配置, 10, 5);
            }
            set
            {
                GetConfigFloat(EnumConfigId.检定配置, 10, value);
            }
        }

        /// <summary>
        /// 新源为True
        /// </summary>
        public bool NewSource
        {
            get
            {
                return GetConfigBool(EnumConfigId.检定配置, 11, true);
            }
            set
            {
                GetConfigBool(EnumConfigId.检定配置, 11, value);
            }
        }
        #endregion

        #region 出厂编号配置
        /// <summary>
        ///是否从条形码截取出厂编号
        /// </summary>
        public string Factory_GenerateSource
        {
            get
            {
                return GetConfigString(EnumConfigId.出厂编号配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.出厂编号配置, 0, value);
            }
        }
        /// <summary>
        /// 是否从左往右截取条形码
        /// </summary>
        public bool Factory_LeftToRight
        {
            get
            {
                return GetConfigBool(EnumConfigId.出厂编号配置, 1, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.出厂编号配置, 1, value);
            }
        }
        /// <summary>
        /// 出厂编号前缀
        /// </summary>
        public string Factory_Prefix
        {
            get
            {
                return GetConfigString(EnumConfigId.出厂编号配置, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.出厂编号配置, 2, value);
            }
        }
        /// <summary>
        /// 出厂编号后缀
        /// </summary>
        public string Factory_Suffix
        {
            get
            {
                return GetConfigString(EnumConfigId.出厂编号配置, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.出厂编号配置, 3, value);
            }
        }

        /// <summary>
        ///出厂编号截取开始点从1开始
        /// </summary>
        public int Factory_StartIndex
        {
            get
            {
                return GetConfigInt(EnumConfigId.出厂编号配置, 4, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.出厂编号配置, 4, Math.Max(1, value));
            }
        }
        /// <summary>
        ///出厂编号截取长度
        /// </summary>
        public int Factory_Length
        {
            get
            {
                return GetConfigInt(EnumConfigId.出厂编号配置, 5, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.出厂编号配置, 5, Math.Abs(value));
            }
        }




        #endregion

        #region 蓝牙光电模式配置(通讯方式)
        /// <summary>
        ///ping码
        /// </summary>
        public string Bluetooth_Ping
        {
            get
            {
                return GetConfigString(EnumConfigId.蓝牙光电模式配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 0, value);
            }
        }
        /// <summary>
        ///光模式类型--内置光模块-外置光模块
        /// </summary>
        public string Bluetooth_LightModel
        {
            get
            {
                return GetConfigString(EnumConfigId.蓝牙光电模式配置, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 1, value);
            }
        }
        /// <summary>
        ///蓝牙模块发设功率
        /// </summary>
        public string Bluetooth_BluetoothTransmitPower
        {
            get
            {
                return GetConfigString(EnumConfigId.蓝牙光电模式配置, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 2, value);
            }
        }
        /// <summary>
        ///蓝牙模块通信模式-普通检定模式--脉冲跟随模式
        /// </summary>
        public string Bluetooth_CommunicationMode
        {
            get
            {
                return GetConfigString(EnumConfigId.蓝牙光电模式配置, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 3, value);
            }
        }
        /// <summary>
        ///检测表发射功率
        /// </summary>
        public string Bluetooth_MeterTransmitPower
        {
            get
            {
                return GetConfigString(EnumConfigId.蓝牙光电模式配置, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 4, value);
            }
        }
        /// <summary>
        ///检测表频段-全频段-带内频段-外频段
        /// </summary>
        public string Bluetooth_MeterFrequencyBand
        {
            get
            {
                return GetConfigString(EnumConfigId.蓝牙光电模式配置, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 5, value);
            }
        }
        /// <summary>
        ///检测表通道数量
        /// </summary>
        public int Bluetooth_MeterPassCount
        {
            get
            {
                return GetConfigInt(EnumConfigId.蓝牙光电模式配置, 6, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.蓝牙光电模式配置, 6, value);
            }
        }
        #endregion

        #region 电机配置
        /// <summary>
        ///  是否自动压接表位(是，检定开始前会压接表位，检定结束会抬起表位)
        /// </summary>
        public bool IsMete_Press
        {
            get
            {
                return GetConfigBool(EnumConfigId.电机配置, 0, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.电机配置, 0, value);
            }
        }
        /// <summary>
        ///电机压接等待时间秒
        /// </summary>
        public int Mete_Press_Time
        {
            get
            {
                return GetConfigInt(EnumConfigId.电机配置, 1, 8);
            }
            set
            {
                SaveConfigValue(EnumConfigId.电机配置, 1, value);
            }
        }
        /// <summary>
        ///是否自动切换互感器
        /// </summary>
        public bool Is_Hgq_AutoCut
        {
            get
            {
                return GetConfigBool(EnumConfigId.电机配置, 2, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.电机配置, 2, value);
            }
        }
        #endregion

        #region 检定过程配置
        /// <summary>
        /// 不合格率报警
        /// </summary>
        public int FailureRate
        {
            get
            {
                return GetConfigInt(EnumConfigId.检定过程配置, 0, 0);
            }
            set
            {
                GetConfigInt(EnumConfigId.检定过程配置, 0, value);
            }
        }
        /// <summary>
        /// 加密解密方式--根据表协议(什么表用什么)-根据698协议(都用698)，根据645协议(都用645)
        /// </summary>
        public string Test_CryptoModel
        {
            get
            {
                return GetConfigString(EnumConfigId.检定过程配置, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定过程配置, 1, value);
            }
        }

        /// <summary>
        ///启动和潜动的时候同步检定通讯协议试验
        /// </summary>
        public bool IsSame
        {
            get
            {
                return GetConfigBool(EnumConfigId.检定过程配置, 2, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定过程配置, 2, value);
            }
        }

        //add zjl 20220226 增加判断是否再次检定
        //modify yjt 20230103 修改数据下标2为3
        /// <summary>
        ///合格的检定点是否重复检定
        /// </summary>
        public bool Is_Ok_Second_Test
        {
            get
            {
                return GetConfigBool(EnumConfigId.检定过程配置, 3, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定过程配置, 3, value);
            }
        }
        //modify yjt 20230103 修改数据下标3为4
        /// <summary>
        /// 密钥更新模式
        /// </summary>
        public string Test_KeyUpdataModel
        {
            get
            {
                return GetConfigString(EnumConfigId.检定过程配置, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定过程配置, 4, value);
            }
        }
        /// <summary>
        /// 是否耐压
        /// </summary>
        public string Test_InsulationModel
        {
            get
            {
                return GetConfigString(EnumConfigId.检定过程配置, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定过程配置, 5, value);
            }
        }
        /// <summary>
        /// 快速试验
        /// </summary>
        public bool Test_QuickModel
        {
            get
            {
                return GetConfigBool(EnumConfigId.检定过程配置, 6, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.检定过程配置, 6, value);
            }
        }
        #endregion

        #region 特殊配置
        /// <summary>
        /// 功耗表位选择
        /// </summary>
        public string SelectBiaoWei
        {
            get
            {
                return GetConfigString(EnumConfigId.特殊配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.特殊配置, 0, value);
            }
        }
        /// <summary>
        /// B相电压补偿
        /// </summary>
        public float VbCompensation
        {
            get
            {
                return GetConfigFloat(EnumConfigId.特殊配置, 1, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.特殊配置, 1, value);
            }
        }

        /// <summary>
        /// 吉林省密钥验证软件，true
        /// </summary>
        public bool KeyCheckInGiLin
        {
            get
            {
                return GetConfigBool(EnumConfigId.特殊配置, 2, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.特殊配置, 2, value);
            }
        }

        /// <summary>
        /// 台体带电流继电器
        /// </summary>
        public bool HasCurrentSwitch
        {
            get
            {
                return GetConfigBool(EnumConfigId.特殊配置, 3, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.特殊配置, 3, value);
            }
        }

        /// <summary>
        /// 台体带电压继电器
        /// </summary>
        public bool HasVoltageSwitch
        {
            get
            {
                return GetConfigBool(EnumConfigId.特殊配置, 4, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.特殊配置, 4, value);
            }
        }

        #endregion

        #region 错误报警数
        /// <summary>
        /// 是否开启二次巡检
        /// </summary>
        public bool IsCheckAgin
        {
            get
            {
                return GetConfigBool(EnumConfigId.错误报警数, 0, true);
            }
            set
            {
                SaveConfigValue(EnumConfigId.错误报警数, 0, value);
            }
        }

        /// <summary>
        /// 不合格数量报警值
        /// </summary>
        public int MaxErrorNumber
        {
            get
            {
                return GetConfigInt(EnumConfigId.错误报警数, 1, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.错误报警数, 1, value);
            }
        }
        #endregion

        //add yjt 20230301 新增
        #region 人工台和流水线设置
        /// <summary>
        /// 不合格表位是否跳出
        /// </summary>
        public bool UnqualifiedJumpOutOf
        {
            get
            {
                return GetConfigBool(EnumConfigId.人工台和流水线设置, 0, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.人工台和流水线设置, 0, true);
            }
        }
        #endregion

        #endregion

        #region 营销接口

        #region 平台接口配置
        /// <summary>
        /// 平台接口类型
        /// </summary>
        public string MDS_Type
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 0, value);
            }
        }

        /// <summary>
        /// 平台下载标识--0条形码  1出厂编号 2表位号
        /// </summary>
        public string MDS_DewnLoadNumber
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 1, value);
            }
        }

        /// <summary>
        /// 平台系统IP地址
        /// </summary>
        public string MDS_IP
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 2, value);
            }
        }

        /// <summary>
        /// 平台系统端口号
        /// </summary>
        public string MDS_Prot
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 3, value);
            }
        }

        /// <summary>
        /// 平台系统数据源--就是表名吧应该
        /// </summary>
        public string MDS_DataSource
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 4, value);
            }
        }

        /// <summary>
        /// 平台——数据库用户名
        /// </summary>
        public string MDS_UserName
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 5, value);
            }
        }

        /// <summary>
        ///平台——数据库密码
        /// </summary>
        public string MDS_UserPassWord
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 6);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 6, value);
            }
        }

        /// <summary>
        /// 平台WebService路径
        /// </summary>
        public string MDS_WebService
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 7);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 7, value);
            }
        }

        /// <summary>
        /// 平台上传时时数据
        /// </summary>
        public bool MDS_UpData
        {
            get
            {
                return GetConfigBool(EnumConfigId.平台接口配置, 8, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 8, value);
            }
        }
        /// <summary>
        /// 生产平台IP地址
        /// </summary>
        public string MDSProduce_IP
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 9);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 9, value);
            }
        }

        /// <summary>
        /// 生产平台端口号
        /// </summary>
        public string MDSProduce_Prot
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 10);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 10, value);
            }
        }

        /// <summary>
        /// 生产平台数据源--就是表名吧应该
        /// </summary>
        public string MDSProduce_DataSource
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 11);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 11, value);
            }
        }

        /// <summary>
        /// 生产平台——数据库用户名
        /// </summary>
        public string MDSProduce_UserName
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 12);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 12, value);
            }
        }

        /// <summary>
        ///生产平台——数据库密码
        /// </summary>
        public string MDSProduce_UserPassWord
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 13);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 13, value);
            }
        }

        /// <summary>
        /// 是否自MDS下载检定方案
        /// </summary>
        public bool MDS_IsMdsDownScheme
        {
            get
            {
                return GetConfigBool(EnumConfigId.平台接口配置, 14, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 14, value);
            }
        }
        /// <summary>
        /// 平台系统编号
        /// </summary>
        public string MDS_SysNo
        {
            get
            {
                return GetConfigString(EnumConfigId.平台接口配置, 15);
            }
            set
            {
                SaveConfigValue(EnumConfigId.平台接口配置, 15, value);
            }
        }
        #endregion

        #region 营销接口配置
        /// <summary>
        /// 营销接口类型
        /// </summary>
        public string Marketing_Type
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 0, value);
            }
        }

        /// <summary>
        /// 营销下载标识--0条形码  1出厂编号 2表位号
        /// </summary>
        public string Marketing_DewnLoadNumber
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 1, value);
            }
        }

        /// <summary>
        /// 营销系统IP地址
        /// </summary>
        public string Marketing_IP
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 2, value);
            }
        }

        /// <summary>
        /// 营销系统端口号
        /// </summary>
        public string Marketing_Prot
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 3, value);
            }
        }

        /// <summary>
        /// 营销系统数据源--就是表名吧应该
        /// </summary>
        public string Marketing_DataSource
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 4, value);
            }
        }

        /// <summary>
        /// 营销——数据库用户名
        /// </summary>
        public string Marketing_UserName
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 5, value);
            }
        }

        /// <summary>
        ///营销——数据库密码
        /// </summary>
        public string Marketing_UserPassWord
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 6);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 6, value);
            }
        }

        /// <summary>
        /// WebService路径
        /// </summary>
        public string Marketing_WebService
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 7);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 7, value);
            }
        }

        /// <summary>
        /// 上传时时数据
        /// </summary>
        public bool Marketing_UpData
        {
            get
            {
                return GetConfigBool(EnumConfigId.营销接口配置, 8, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 8, value);
            }
        }
        /// <summary>
        /// 生产平台IP地址
        /// </summary>
        public string MarketingProduce_IP
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 9);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 9, value);
            }
        }

        /// <summary>
        /// 生产平台端口号
        /// </summary>
        public string MarketingProduce_Prot
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 10);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 10, value);
            }
        }

        /// <summary>
        /// 生产平台数据源--就是表名吧应该
        /// </summary>
        public string MarketingProduce_DataSource
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 11);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 11, value);
            }
        }

        /// <summary>
        /// 生产平台——数据库用户名
        /// </summary>
        public string MarketingProduce_UserName
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 12);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 12, value);
            }
        }

        /// <summary>
        ///生产平台——数据库密码
        /// </summary>
        public string MarketingProduce_UserPassWord
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 13);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 13, value);
            }
        }

        /// <summary>
        /// 是否自MDS下载检定方案
        /// </summary>
        public bool MarketingIsMdsDownScheme
        {
            get
            {
                return GetConfigBool(EnumConfigId.营销接口配置, 14, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 14, value);
            }
        }
        /// <summary>
        /// 系统编号
        /// </summary>
        public string Marketing_SysNo
        {
            get
            {
                return GetConfigString(EnumConfigId.营销接口配置, 15);
            }
            set
            {
                SaveConfigValue(EnumConfigId.营销接口配置, 15, value);
            }
        }
        #endregion

        #endregion

        #region 加密机

        #region 加密机配置
        /// <summary>
        /// 加密机类型
        /// </summary>
        public string Dog_Type
        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 0, value);
            }
        }
        /// <summary>
        /// 加密机IP
        /// </summary>
        public string Dog_IP
        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 1, value);
            }
        }
        /// <summary>
        /// 加密机端口
        /// </summary>
        public string Dog_Prot
        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 2, value);
            }
        }
        /// <summary>
        /// 加密机密钥
        /// </summary>
        public string Dog_key

        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 3, value);
            }
        }
        /// <summary>
        /// 加密机认证类型--公钥--私钥
        /// </summary>
        public string Dog_CheckingType
        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 4, value);
            }
        }
        //modify yjt 20230103 修改下标0为5
        /// <summary>
        /// 加密机-是否进行密码机服务器连接
        /// </summary>
        public bool Dog_IsPassWord
        {
            get
            {
                return GetConfigBool(EnumConfigId.加密机配置, 5, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 5, value);
            }
        }
        /// <summary>
        /// 加密机连接模式---服务器版-直连加密机版
        /// </summary>
        public string Dog_ConnectType
        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 6);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 6, value);
            }
        }
        /// <summary>
        /// 加密机超时时间
        /// </summary>
        public string Dog_Overtime
        {
            get
            {
                return GetConfigString(EnumConfigId.加密机配置, 7);
            }
            set
            {
                SaveConfigValue(EnumConfigId.加密机配置, 7, value);
            }
        }
        #endregion

        #endregion

        #region 网络信息

        #region 集控线配置
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string SetControl_Ip
        {
            get
            {
                return GetConfigString(EnumConfigId.集控线配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.集控线配置, 0, value);
            }
        }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public string SetControl_Port
        {
            get
            {
                return GetConfigString(EnumConfigId.集控线配置, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.集控线配置, 1, value);
            }
        }
        /// <summary>
        /// 流水线编号
        /// </summary>
        public string SetControl_No
        {
            get
            {
                return GetConfigString(EnumConfigId.集控线配置, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.集控线配置, 2, value);
            }
        }
        /// <summary>
        /// 集控线台体编号
        /// </summary>
        public string SetControl_BenthNo
        {
            get
            {
                return GetConfigString(EnumConfigId.集控线配置, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.集控线配置, 3, value);
            }
        }
        /// <summary>
        /// 是否启用工况信息上报
        /// </summary>
        public string OperatingConditionsYesNo
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 0, value);
            }
        }
        /// <summary>
        /// 工况服务器IP
        /// </summary>
        public string OperatingConditionsIp
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 1, value);
            }
        }
        /// <summary>
        /// 服务器端口
        /// </summary>  
        public string OperatingConditionsProt
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 2, value);
            }
        }


        /// <summary>
        /// 工况信息上报频率
        /// </summary>
        public string OperatingConditionsUpdataF
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 3);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 3, value);
            }
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string PlantNO
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 4);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 4, value);
            }
        }

        /// <summary>
        /// 工控服务器端口
        /// </summary>
        public string DevControllServerPort
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 5);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 5, value);
            }
        }

        /// <summary>
        /// 工控服务器IP
        /// </summary>
        public string DevControllServerIP
        {
            get
            {
                return GetConfigString(EnumConfigId.工控平台上报, 6);
            }
            set
            {
                SaveConfigValue(EnumConfigId.工控平台上报, 6, value);
            }
        }
        ///// <summary>
        ///// 本机IP
        ///// </summary>
        //public string SetLocal_Ip
        //{
        //    get
        //    {
        //        return GetConfigString(EnumConfigId.集控线配置, 4);
        //    }
        //    set
        //    {
        //        SaveConfigValue(EnumConfigId.集控线配置, 4, value);
        //    }
        //}
        ///// <summary>
        ///// 本机端口
        ///// </summary>
        //public string SetLocal_Port
        //{
        //    get
        //    {
        //        return GetConfigString(EnumConfigId.集控线配置, 5);
        //    }
        //    set
        //    {
        //        SaveConfigValue(EnumConfigId.集控线配置, 5, value);
        //    }
        //}
        #endregion

        #endregion

        #region 解析证书编号配置
        /// <summary>
        ///证书编号生成源
        /// </summary>
        public string Certificate_GenerateSource
        {
            get
            {
                return GetConfigString(EnumConfigId.证书编号配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.证书编号配置, 0, value);
            }
        }

        /// <summary>
        ///证书编号截取方向
        /// </summary>
        public bool Certificate_LeftToRight
        {
            get
            {
                return GetConfigBool(EnumConfigId.证书编号配置, 1, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.证书编号配置, 1, value);
            }
        }
        /// <summary>
        ///证书编号截取开始点从1开始
        /// </summary>
        /// <remarks>范围1~n</remarks>
        public int Certificate_StartIndex
        {
            get
            {
                return GetConfigInt(EnumConfigId.证书编号配置, 2, 1);
            }
            set
            {
                SaveConfigValue(EnumConfigId.证书编号配置, 2, Math.Max(1, value));
            }
        }
        /// <summary>
        ///证书编号截取长度
        /// </summary>
        public int Certificate_Length
        {
            get
            {
                return GetConfigInt(EnumConfigId.证书编号配置, 3, 12);
            }
            set
            {
                SaveConfigValue(EnumConfigId.证书编号配置, 3, Math.Abs(value));
            }
        }
        #endregion

        #region  资产编号配置
        /// <summary>
        /// 资产编号数据生成源
        /// </summary>
        public string Asset_GenerateSource
        {
            get
            {
                return GetConfigString(EnumConfigId.资产编号配置, 0);
            }
            set
            {
                SaveConfigValue(EnumConfigId.资产编号配置, 0, value);
            }
        }
        #endregion

        #endregion

        #region 一些其他配置--不是从数据库读取,而是在程序过程中设置的
        /// <summary>
        /// 是否是物联网表
        /// </summary>
        public bool IsITOMeter { get; set; } = false;
        /// <summary>
        /// 脉冲类型--无-光电脉冲--蓝牙脉冲
        /// </summary>
        public string PulseType { get; set; } = "无";
        /// <summary>
        /// 自动检定的脉冲类型--无-光电脉冲--蓝牙脉冲
        /// </summary>
        public string AutoCmdPulseType { get; set; } = "无";
        #endregion


        #region 解析通信地址配置
        /// <summary>
        ///是否从条形码解析通信地址
        /// </summary>
        public bool Address_IsIntercept
        {
            get
            {
                return GetConfigBool(EnumConfigId.通信地址配置, 0, true);
            }
            set
            {
                SaveConfigValue(EnumConfigId.通信地址配置, 0, value);
            }
        }

        /// <summary>
        ///通信地址截取方向
        /// </summary>
        public bool Address_LeftToRight
        {
            get
            {
                return GetConfigBool(EnumConfigId.通信地址配置, 1, false);
            }
            set
            {
                SaveConfigValue(EnumConfigId.通信地址配置, 1, value);
            }
        }
        /// <summary>
        ///通信地址截取开始点从1开始
        /// </summary>
        public int Address_StartIndex
        {
            get
            {
                return GetConfigInt(EnumConfigId.通信地址配置, 2, 2);
            }
            set
            {
                SaveConfigValue(EnumConfigId.通信地址配置, 2, value);
            }
        }
        /// <summary>
        ///通信地址截取长度
        /// </summary>
        public int Address_Len
        {
            get
            {
                return GetConfigInt(EnumConfigId.通信地址配置, 3, 12);
            }
            set
            {
                SaveConfigValue(EnumConfigId.通信地址配置, 3, value);
            }
        }

        /// <summary>
        /// 从营销认证ID，默认不认证（false）
        /// </summary>
        public bool AuthenHPLCID
        {
            get
            {
                return false;
            }
            //TODO:
        }

        #endregion
    }
}
