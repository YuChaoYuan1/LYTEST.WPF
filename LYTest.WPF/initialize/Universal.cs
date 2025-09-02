using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace initialize
{
    class Universal
    {
        public event EventHandler<string> OutMessage;
        public string ConnectionString
        {
            get
            {
                if (8 == IntPtr.Size)
                {
                    return $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\AppData.mdb";
                }
                else
                {
                    return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\AppData.mdb";
                }
            }
        }
        public string ConnectionStringMeterData
        {
            get
            {
                if (8 == IntPtr.Size)
                {
                    return $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\MeterData.mdb";
                }
                else
                {
                    return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\MeterData.mdb";
                }
            }
        }
        public string ConnectionStringTmpMeterData
        {
            get
            {
                if (8 == IntPtr.Size)
                {
                    return $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\TmpMeterData.mdb";
                }
                else
                {
                    return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\TmpMeterData.mdb";
                }
            }
        }

        public virtual void Execute()
        {
            bool debugLog = false;
            try
            {
                ShowMsg("更新数据库");
                try
                {
                    using (OleDbConnection connection = new OleDbConnection(ConnectionString))
                    {
                        connection.Open();
                        OleDbCommand command = connection.CreateCommand();
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = "alter table [T_CONFIG_PARA_VALUE] alter COLUMN [CONFIG_VALUE]  LONGTEXT;";
                        command.ExecuteNonQuery();
                    }
                }
                catch { }
                ShowMsg("检查结构");
                List<string> ColumnNames_METER_INFO = GetColumnNames(ConnectionStringMeterData, "METER_INFO");
                if (debugLog) ShowMsg("DB1");
                List<string> ColumnNames_T_TMP_METER_INFO = GetColumnNames(ConnectionStringTmpMeterData, "T_TMP_METER_INFO");
                if (debugLog) ShowMsg("DB2");
                List<string> ColumnNames_METER_COMMUNICATION = GetColumnNames(ConnectionStringMeterData, "METER_COMMUNICATION");
                if (debugLog) ShowMsg("DB3");
                List<string> ColumnNames_T_TMP_METER_COMMUNICATION = GetColumnNames(ConnectionStringTmpMeterData, "T_TMP_METER_COMMUNICATION");
                ShowMsg("检查类型");
                try
                {
                    //判断列名存在
                    string columnName = "MD_SORT";
                    if (!ColumnNames_METER_INFO.Contains(columnName))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = $"alter table METER_INFO add column {columnName} Text(50);";//类别：智能表，物联电能表
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("1");
                try
                {
                    if (!ColumnNames_T_TMP_METER_INFO.Contains("MD_SORT"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringTmpMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = "alter table T_TMP_METER_INFO add column MD_SORT Text(50);";//类别：智能表，物联电能表
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("2");
                try
                {
                    //判断列名存在
                    if (!ColumnNames_METER_INFO.Contains("MD_UPDOWN"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = "alter table METER_INFO add column MD_UPDOWN Text(25);";//：MDS，营销
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("3");
                try
                {
                    if (!ColumnNames_T_TMP_METER_INFO.Contains("MD_UPDOWN"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringTmpMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = "alter table T_TMP_METER_INFO add column MD_UPDOWN Text(25);";//：MDS，营销
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("4");
                try
                {
                    if (!ColumnNames_METER_COMMUNICATION.Contains("MD_PARAMETER"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = "alter table METER_COMMUNICATION add column MD_PARAMETER LONGTEXT;";//方案参数
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("5");
                try
                {
                    if (!ColumnNames_T_TMP_METER_COMMUNICATION.Contains("MD_PARAMETER"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringTmpMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = "alter table T_TMP_METER_COMMUNICATION add column MD_PARAMETER LONGTEXT;";//方案参数
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("6");
                using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                {
                    connection.Open();
                    OleDbCommand command = connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "alter table [METER_COMMUNICATION] alter COLUMN [MD_PARAMETER]  LONGTEXT;";
                    command.ExecuteNonQuery();
                }
                if (debugLog) ShowMsg("7");
                using (OleDbConnection connection = new OleDbConnection(ConnectionStringTmpMeterData))
                {
                    connection.Open();
                    OleDbCommand command = connection.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "alter table [T_TMP_METER_COMMUNICATION] alter COLUMN [MD_PARAMETER]  LONGTEXT;";
                    command.ExecuteNonQuery();
                }
                if (debugLog) ShowMsg("8");
                //接收日期
                try
                {
                    //判断列名存在
                    if (!ColumnNames_METER_INFO.Contains("MD_DATERECEIVED"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = "alter table METER_INFO add column MD_DATERECEIVED Text(50);";
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("9");
                try
                {
                    if (!ColumnNames_T_TMP_METER_INFO.Contains("MD_DATERECEIVED"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringTmpMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = CommandType.Text;
                            command.CommandText = "alter table T_TMP_METER_INFO add column MD_DATERECEIVED Text(50);";
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("10");
                //计量公共服务平台网址
                try
                {
                    //判断列名存在
                    if (!ColumnNames_METER_INFO.Contains("MD_FLAGCODE_URI"))
                    {
                        using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                        {
                            connection.Open();
                            OleDbCommand command = connection.CreateCommand();
                            command.CommandType = CommandType.Text;
                            command.CommandText = "alter table METER_INFO add column MD_FLAGCODE_URI Text(150);";
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
                if (debugLog) ShowMsg("11");
                try
                {
                    if (!ColumnNames_T_TMP_METER_INFO.Contains("MD_FLAGCODE_URI"))
                    {
                        using (OleDbConnection conn = new OleDbConnection(ConnectionStringTmpMeterData))
                        {
                            conn.Open();
                            OleDbCommand cmd = conn.CreateCommand();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "alter table T_TMP_METER_INFO add column MD_FLAGCODE_URI Text(150);";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }

                ShowMsg("检查配置");
                //---------------------------------------------------
                // 菜单项
                Add_T_MENU_VIEW("数据备份", "View_BackupData", "V_Schema.png", "1", "0", "0", "0", "0", "1", "020");


                // ------------------------------------------------------
                // 系统配置
                Add_T_CONFIG_PARA_FORMAT("01003", "显示版本号|显示台体编号|检定控制按钮是否悬浮|检定界面误差显示|参数录入显示无功列", "YesNo|YesNo|YesNo|VerifyErrorShow|YesNo", "否|否|否|平均值|否");
                Add_T_CONFIG_PARA_FORMAT("02001", "是否使用固定常数|是否使用自动档位|是否读取标准表数据|标准表读取间隔(ms)|标准表固定常数|连检切换点位关电流", "YesNo|YesNo|YesNo|||YesNo", "是|是|是|1000|0|是");
                Add_T_CONFIG_PARA_FORMAT("02002", "源稳定时间|写操作时提示|走字电量输入方式|走字实验前电表清零|清零时校时|日计时测量值|做最大需量时清零|采用高精度电量", "|YesNo|YesNo|YesNo|YesNo|DayClockTestValue|YesNo|YesNo", "5|否|否|否|否|s/d|是|否");
                Add_T_CONFIG_PARA_FORMAT("02004", "误差计算取值数|最大处理时间|误差个数最大数|平均值小数位数|误差起始采集次数|跳差判定倍数|温度|湿度|标准偏差计算取值数|是否使用时间计算圈数|一个误差最少使用时间|新版功率源", "|||||||||YesNo||YesNo", "2|300|5|4|0|1|20|60|5|是|5|是");
                Add_T_CONFIG_PARA_FORMAT("02005", "出厂编码的来源|是否从左往右截取|出厂编号前缀|出厂编号后缀|截取起始点（1~n）|截取长度",
                    "FactoryCodeSource|YesNo||||", "从通讯地址截取|否|||1|12");

                //检定配置 -> 特殊配置
                Add_T_CONFIG_PARA_FORMAT("02009", "功耗表位选择|B相电压补偿|存盘前密钥验证|台体带电流继电器|台体带电压继电器", "||YesNo|YesNo|YesNo", "0|0|0|是|否");
                Add_T_CONFIG_PARA_FORMAT("02014",
                   "证书编号的来源|是否从左往右截取|起始位置（1~n）|截取长度",
                   "CertificateSource|YesNo||", "从条形码截取|否|1|12");
                Add_T_CONFIG_PARA_FORMAT("02015", "资产号的来源", "AssetNumberSource", "从条形码获取");
                Add_T_CONFIG_PARA_FORMAT("03001", "MDS接口类型|下载参数标识|平台IP地址|端口号|数据源|数据库用户名|数据库密码|WebService链接|上传实时数据|平台IP地址|端口号|数据源|数据库用户名|数据库密码|是否下载方案|系统编号", "MisType|DownLoadNumber|||||||YesNo||||||YesNo|", "无|条形码|||||||||||||否|450");
                Add_T_CONFIG_PARA_FORMAT("03002", "营销接口类型|下载参数标识|营销IP地址|端口号|数据源|数据库用户名|数据库密码|WebService链接|上传实时数据|营销IP地址|端口号|数据源|数据库用户名|数据库密码|是否下载方案|系统编号", "MisType|DownLoadNumber|||||||YesNo||||||YesNo|", "无|条形码|||||||||||||否|450");

                Add_T_SCHEMA_PARA_FORMAT("12016", "走字48h试验", "PowerDirection|PowerElement|PowerFactor|CurrentTimes|Rate|ZouZiWay|||SwitchCurrent|SwitchCurrent|HarmonicAngle|HarmonicAngle||YesNo", "功率方向|功率元件|功率因数|电流倍数|费率|走字方式|定时走字时间(分钟)|定量走字电量(度)|48h切换电流白天|48h切换电流晚上|48h切换角度前|48h切换角度后|费率时段(英文逗号,间隔)|使用表内时段走字", "True|True|True|True|True|True|False|False|False|False|False|False|False|False", "True|False|False|True|True|True|False|False|False|False|False|False|False|False", "12016", "正向有功|H|1.0|0.5Imax|总|定时法|15|0.5|6|1.5|30|210|00:00(谷),08:00(峰)|是", "AccurateTest.Register48h", "1", "");

                ShowMsg("软件配置 -> 配置初始值");
                Add_T_CONFIG_PARA_VALUE("02009", "1,2,3|0.01|否|是|否");
                Add_T_CONFIG_PARA_VALUE("02004", "2|300|5|4|0|0.2|20|50|5|否|5|是");
                Add_T_CONFIG_PARA_VALUE("02005", "从通讯地址截取|否|||1|12");
                Add_T_CONFIG_PARA_VALUE("02014", "从条形码截取|否|1|12");
                Add_T_CONFIG_PARA_VALUE("02015", "从条形码获取");
                //--------------------------------------------------------
                // 检定项结论
                Add_T_VIEW_CONFIG("004", "日计时试验", "METER_COMMUNICATION", "误差1|误差2|误差3|误差4|误差5|平均值|化整值|误差限(s/d),结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("261", "自热试验结论", "METER_COMMUNICATION", "首个误差点|误差集合|最大变化值,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("900", "电表参数录入", "METER_INFO", "True|MD_UB|电压(V)|VoltageValue|True|编码名称|True|220|False,True|MD_UA|电流(A)|CurrentValue|True|编码名称|True|5(80)|False,True|MD_FREQUENCY|频率(Hz)|FreqTest|True|编码名称|True|50|False,True|MD_TESTMODEL|首检抽检|TestType2|True|编码名称|True|首检|False,True|MD_TEST_TYPE|检定类型|UserCheckType|True|编码名称|True|全检|False,True|MD_WIRING_MODE|测量方式|CLFS|True|编码名称|True|单相|False,True|MD_CONNECTION_FLAG|互感器|BothVRoadType|True|编码名称|True|直接式|False,True|MD_FKTYPE|费控类型|FKType|True|编码名称|True|远程费控|False,True|MD_JJGC|检定规程|JJGC|True|编码名称|True|JJG596-2012|False,True|MD_EPITOPE|表位||False|编码名称|True||False,True|MD_BAR_CODE|条形码||False|编码名称|True||True,True|MD_ASSET_NO|资产编号||False|编码名称|False||True,True|MD_METER_TYPE|表类型|TestMeterType|False|编码名称|True||False,True|MD_CONSTANT|常数|ConstantValue|False|编码名称|True||False,True|MD_GRADE|等级|AccuracyClass|False|编码名称|True||False,True|MD_METER_MODEL|表型号|MeterModel|False|编码名称|False||False,True|MD_PROTOCOL_NAME|通讯协议|ProtocolName|False|编码名称|True||False,True|MD_CARR_NAME|载波协议|CarrName|False|编码名称|True||False,True|MD_POSTAL_ADDRESS|通讯地址||False|编码名称|False||True,True|MD_FACTORY|制造厂家|MeterFactory|False|编码名称|False||True,True|MD_CUSTOMER|送检单位|SubmitInspection|False|编码名称|False||True,True|MD_TASK_NO|任务编号||False|编码名称|False||True,True|MD_MADE_NO|出厂编号||False|编码名称|False||True,True|MD_CERTIFICATE_NO|证书编号||False|编码名称|False||True,True|MD_OTHER_3|脉冲类型|PulseType|True|编码名称|True|无|False,True|MD_OTHER_4|样品单号||False|编码名称|False||True,True|MD_SORT|表类别|TestMeterSort|True|编码名称|True|智能表|False,True|MD_DATERECEIVED|接收日期||False|日期|False||True,False|MD_AUDIT_PERSON|核验员||False|编码名称|False||True,False|MD_CHECKED|是否要检||False|编码名称|False||True,False|MD_DEVICE_ID|台体编号||False|编码名称|False||False,False|MD_FLAGCODE_URI|||False|编码名称|False||False,False|MD_HUMIDITY|湿度||False|编码名称|False||True,False|MD_OTHER_1|是否需要上传||False|编码名称|False||True,False|MD_OTHER_2|上传标识||False|编码名称|False||True,False|MD_OTHER_5|备用5||False|编码名称|False||True,False|MD_RESULT|结论||False|编码名称|False||False,False|MD_SCHEME_ID|方案编号||False|编码名称|False||True,False|MD_SEAL_1|铅封1||False|编码名称|False||True,False|MD_SEAL_2|铅封2||False|编码名称|False||True,False|MD_SEAL_3|铅封3||False|编码名称|False||True,False|MD_SEAL_4|铅封4||False|编码名称|False||True,False|MD_SEAL_5|铅封5||False|编码名称|False||True,False|MD_SUPERVISOR|主管||False|编码名称|False||True,False|MD_TEMPERATURE|温度||False|编码名称|False||True,False|MD_TEST_DATE|检定日期||False|编码名称|False||True,False|MD_TEST_PERSON|检验员||False|编码名称|False||True,False|MD_UPDOWN|||False|编码名称|False||False,False|MD_VALID_DATE|计检日期||False|编码名称|False||True,False|METER_ID|表唯一编号||False|编码名称|False||False",
                    "MD_UB,MD_UA,MD_FREQUENCY,MD_TESTMODEL,MD_TEST_TYPE,MD_WIRING_MODE,MD_CONNECTION_FLAG,MD_FKTYPE,MD_JJGC,MD_EPITOPE,MD_BAR_CODE,MD_ASSET_NO,MD_METER_TYPE,MD_CONSTANT,MD_GRADE,MD_METER_MODEL,MD_PROTOCOL_NAME,MD_CARR_NAME,MD_POSTAL_ADDRESS,MD_FACTORY,MD_CUSTOMER,MD_TASK_NO,MD_MADE_NO,MD_CERTIFICATE_NO,MD_OTHER_3,MD_OTHER_4,MD_SORT,MD_DATERECEIVED");
                Add_T_VIEW_CONFIG("12016", "走字48h试验", "METER_COMMUNICATION", "试验前总电量|试验后总电量|总电量差值|试验前费率电量|试验后费率电量|费率电量差值|组合误差,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("12007", "初始固有误差结论", "METER_COMMUNICATION", "误差下限|误差上限|误差圈数|上升误差1|上升误差2|上升平均值|上升化整值|下降误差1|下降误差2|下降平均值|下降化整值|平均值,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("12017", "外部工频磁场试验", "METER_COMMUNICATION", "电流倍数|正常误差|面1误差|面2误差|面3误差|最大变差值|误差上限|误差下限,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("12018", "外部工频磁场(无负载条件)试验", "METER_COMMUNICATION", "电压倍数|方案时间(秒)|实际时间(秒),结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("12019", "外部工频磁场试验2", "METER_COMMUNICATION", "功率方向|元件|电流倍数|影响前误差|影响后误差|差值|托盘角度|线圈角度|磁场强度,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("15033", "预置内容检查", "METER_COMMUNICATION", "当前项目|检定信息|标准数据,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("15032", "预置内容设置", "METER_COMMUNICATION", "当前项目|检定信息|标准数据,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("18005", "重复性", "METER_COMMUNICATION", "功率元件|功率方向|电流倍数|功率因数|误差限|误差圈数|误差值|平均值|偏差|最值差|化整值,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("15041", "电量法需量功能试验", "METER_COMMUNICATION", "需量测量方式验证结论|需量缓存空间验证结论,结论", "MD_VALUE,MD_RESULT");

                //------------------------------------------------------------
                // 检定项方案
                Add_T_SCHEMA_PARA_FORMAT("12017", "外部工频磁场试验", "CurrentTimes|||", "电流倍数|持续时间(秒)|误差限|变差限|磁场强度电流(A)", "True|True|False|False|False", "True|False|False|False|False", "12017", "1.0Ib|60|2|1|5", "Influence.MFFs", "1", "");
                Add_T_SCHEMA_PARA_FORMAT("12018", "外部工频磁场(无负载条件)试验", "SneakVoltage|", "电压倍数|持续时间(秒)|托盘角度|线圈角度|磁场强度电流(A)", "True|False|False|False|False", "True|False|False|False|False", "12018", "115%|180|0|0|5", "Influence.MFFnoload", "1", "");
                Add_T_SCHEMA_PARA_FORMAT("12019", "外部工频磁场试验2", "PowerDirection|PowerElement|CurrentTimes|||", "功率方向|元件|电流倍数|托盘角度|线圈角度|磁场强度电流(A)", "True|True|True|False|False|False", "True|True|True|True|True|True", "12019", "正向有功|H|Ib|0|0|5", "Influence.MFFs2", "1", "");
                Add_T_SCHEMA_PARA_FORMAT("15023", "日计时误差", "|||||", "误差限(s/d)|误差次数|检定圈数|标准时钟频率|被检时钟频率|项目编号", "False|False|False|False|False|False", "False|False|False|False|False|False", "15023", "0.5|5|60|50||", "AccurateTest.ClockError", "1", "");
                Add_T_SCHEMA_PARA_FORMAT("15032", "预置内容设置", "ConnProtocolDataName|||||ComProtocolType|", "数据项名称|标识编码|长度|小数位|数据格式|功能|标准数据", "True|False|False|False|False|True|False", "True|False|False|False|False|True|False", "15032", "|||||读|", "ConnProtocolTest.ConnProtocol", "0", "");
                Add_T_SCHEMA_PARA_FORMAT("15033", "预置内容检查", "ConnProtocolDataName|||||ComProtocolType|", "数据项名称|标识编码|长度|小数位|数据格式|功能|标准数据", "True|False|False|False|False|True|False", "True|False|False|False|False|True|False", "15033", "|||||读|", "ConnProtocolTest.ConnProtocol", "0", "");
                Add_T_SCHEMA_PARA_FORMAT("18002", "变差要求试验", "|PowerFactor|CurrentTimes", "两次测试时间间隔(分钟)|功率因数|电流倍数", "False|True|True", "True|True|True", "182", "5|1.0|10Itr", "AccurateTest.ErrorVarietyErr", "1", "042");
                Add_T_SCHEMA_PARA_FORMAT("18005", "重复性", "PowerDirection|PowerElement|CurrentTimes|PowerFactor", "功率方向|功率元件|电流倍数|功率因数", "True|True|True|True", "True|True|True|True", "18005", "正向有功|H|Ib|1.0", "AccurateTest.ReproducibilityTest", "1", "");
                Add_T_SCHEMA_PARA_FORMAT("26001", "自热试验", "PowerFactor|||", "功率因数|预热时间(分钟)|间隔时间(秒)|最长运行时间(分)", "True|False|False|False", "True|False|False|False", "261", "1.0|120|180|60", "Selfheating.SelfheatVerify3", "1", "");
                Add_T_SCHEMA_PARA_FORMAT("15041", "电量法需量功能试验", "", "", "", "", "262", "", "Multi.Dgn_PowerDemandTest", "1", "");

                //-----------------------------------------------------------------
                // 

                Add_T_CODE_TREE("弹出对话框等待", "", "3", "KeyUpdataModel", "03", "1");
                Add_T_CODE_TREE("检测表类别", "TestMeterSort", "2", "ConfigSource", "", "1");
                Add_T_CODE_TREE("智能表", "", "3", "TestMeterSort", "01", "1");
                Add_T_CODE_TREE("物联电能表", "", "3", "TestMeterSort", "02", "1");

                Add_T_CODE_TREE("平台接口配置", "", "3", "MarketingInterface", "001", "1");
                Add_T_CODE_TREE("营销接口配置", "", "3", "MarketingInterface", "002", "1");


                Add_T_CODE_TREE("电量小数位", "MeterDecimalDigitsType", "2", "ConfigSource", "", "1");
                Add_T_CODE_TREE("2", "", "3", "MeterDecimalDigitsType", "1", "1");
                Add_T_CODE_TREE("4", "", "3", "MeterDecimalDigitsType", "2", "1");

                Add_T_CODE_TREE("CDLT6452007(09)", "", "3", "ProtocolName", "6", "1");
                Add_T_CODE_TREE("DLT6452007-4位", "", "3", "ProtocolName", "7", "1");
                Add_T_CODE_TREE("DLT6452007(09)-4位", "", "3", "ProtocolName", "8", "1");
                Add_T_CODE_TREE("DLT698-4位", "", "3", "ProtocolName", "9", "1");
                Add_T_CODE_TREE("DLT698-4800", "", "3", "ProtocolName", "10", "1");



                Add_T_CODE_TREE("深谷", "", "3", "Rate", "5");


                Add_T_CODE_TREE("隔离互感器", "IsolateCT", "3", "EquipmentType", "17", "1");
                Add_T_CODE_TREE("CT20303C", "", "4", "IsolateCT", "01", "1");

                Add_T_CODE_TREE("河北营销2.0", "", "3", "MisType");

                Add_T_CODE_TREE("Q/GDW12175-2021", "", "3", "JJGC");
                Add_T_CODE_TREE("Q/GDW10827-2020", "", "3", "JJGC");
                Add_T_CODE_TREE("Q/GDW10364-2020", "", "3", "JJGC");

                Add_T_CODE_TREE("4800,e,8,1", "", "3", "SerialPortPara");
                Add_T_CODE_TREE("4800,n,8,1", "", "3", "SerialPortPara");

                Add_T_CODE_TREE("检定误差显示", "VerifyErrorShow", "2", "ConfigSource");
                Add_T_CODE_TREE("平均值", "", "3", "VerifyErrorShow");
                Add_T_CODE_TREE("化整值", "", "3", "VerifyErrorShow");

                Add_T_CODE_TREE("日计时测量值", "DayClockTestValue", "2", "ConfigSource");
                Add_T_CODE_TREE("s/d", "", "3", "DayClockTestValue");
                Add_T_CODE_TREE("Hz", "", "3", "DayClockTestValue");

                Add_T_CODE_TREE("预置内容设置", "", "3", "DgnItem", "032");
                Add_T_CODE_TREE("预置内容设置", "", "2", "MeterResultViewId", "15032");

                Add_T_CODE_TREE("预置内容检查", "", "3", "DgnItem", "033");
                Add_T_CODE_TREE("预置内容检查", "", "2", "MeterResultViewId", "15033");

                //误差一致性 目录下加
                Add_T_CODE_TREE("重复性", "", "3", "ErrorAccordItem", "005");
                Add_T_CODE_TREE("重复性", "", "2", "MeterResultViewId", "18005");  //?



                Add_T_CODE_TREE("走字方式", "ZouZiWay", "2", "CheckParamSource");
                Add_T_CODE_TREE("定时法", "", "3", "ZouZiWay", "01");
                Add_T_CODE_TREE("定量法", "", "3", "ZouZiWay", "02");
                Add_T_CODE_TREE("48h切换象限法", "", "3", "ZouZiWay", "03");

                Add_T_CODE_TREE("48h小时切换电流(A)", "SwitchCurrent", "2", "CheckParamSource");
                Add_T_CODE_TREE("6", "", "3", "SwitchCurrent");
                Add_T_CODE_TREE("1.5", "", "3", "SwitchCurrent");
                Add_T_CODE_TREE("1.2", "", "3", "SwitchCurrent");
                Add_T_CODE_TREE("0.3", "", "3", "SwitchCurrent");
                //准确度 目录下加
                Add_T_CODE_TREE("走字48h试验", "", "3", "WcType", "016");
                Add_T_CODE_TREE("走字48h试验", "", "2", "MeterResultViewId", "12016");  //?

                Add_T_CODE_TREE("电量法需量功能试验","", "3","DgnItem", "41");  //?
                Add_T_CODE_TREE("电量法需量功能试验","", "2", "MeterResultViewId", "15041");  //?

                Add_T_CODE_TREE("0.2S", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("0.5", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("0.5S", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("1", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("1S", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("2", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("A", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("B", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("C", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("D", "", "3", "AccuracyClass");
                Add_T_CODE_TREE("E", "", "3", "AccuracyClass");

                Add_T_CODE_TREE("100", "", "3", "ConstantValue");
                Add_T_CODE_TREE("200", "", "3", "ConstantValue");
                Add_T_CODE_TREE("300", "", "3", "ConstantValue");
                Add_T_CODE_TREE("400", "", "3", "ConstantValue");
                Add_T_CODE_TREE("500", "", "3", "ConstantValue");
                Add_T_CODE_TREE("1000", "", "3", "ConstantValue");
                Add_T_CODE_TREE("2000", "", "3", "ConstantValue");
                Add_T_CODE_TREE("1200", "", "3", "ConstantValue");
                Add_T_CODE_TREE("6400", "", "3", "ConstantValue");
                Add_T_CODE_TREE("10000", "", "3", "ConstantValue");
                Add_T_CODE_TREE("20000", "", "3", "ConstantValue");
                Add_T_CODE_TREE("40000", "", "3", "ConstantValue");
                Add_T_CODE_TREE("50000", "", "3", "ConstantValue");
                Add_T_CODE_TREE("100000", "", "3", "ConstantValue");


                Add_T_CODE_TREE("上海营销2.0", "", "3", "MisType");

                Add_T_CODE_TREE("第一象限无功", "", "3", "PowerDirection", "5");
                Add_T_CODE_TREE("第二象限无功", "", "3", "PowerDirection", "6");
                Add_T_CODE_TREE("第三象限无功", "", "3", "PowerDirection", "7");
                Add_T_CODE_TREE("第四象限无功", "", "3", "PowerDirection", "8");

                //初始固有误差      初始固有误差改界面的初始化代码,由原来的差值改为平均值

                //准确度 【外部工频磁场试验】
                Add_T_CODE_TREE("外部工频磁场试验", "", "3", "WcType", "17");
                Add_T_CODE_TREE("外部工频磁场试验", "", "2", "MeterResultViewId", "12017");  //?

                //准确度 【外部工频磁场(无负载条件)试验】
                Add_T_CODE_TREE("外部工频磁场(无负载条件)试验", "", "3", "WcType", "18");
                Add_T_CODE_TREE("外部工频磁场(无负载条件)试验", "", "2", "MeterResultViewId", "12018");  //?

                //2024/9/26
                // 添加功率因数 0.1
                Add_T_CODE_TREE("0.1L", "", "3", "PowerFactor", "8");
                Add_T_CODE_TREE("0.1C", "", "3", "PowerFactor", "9");

                //准确度 【外部工频磁场试验2】
                Add_T_CODE_TREE("外部工频磁场试验2", "", "3", "WcType", "19");
                Add_T_CODE_TREE("外部工频磁场试验2", "", "2", "MeterResultViewId", "12019");  //?

                // 模组质量
                Add_T_CODE_TREE("模组质量检测", "QualityModule", "2", "SchemaCategory", "30");
                Add_T_CODE_TREE("三相电流不平衡", "", "3", "QualityModule", "01");
                Add_T_CODE_TREE("电能质量模组闪变试验检定", "", "3", "QualityModule", "02");
                Add_T_CODE_TREE("谐波有功功率", "", "3", "QualityModule", "03");
                Add_T_CODE_TREE("电能质量模组电压偏差试验检定", "", "3", "QualityModule", "04");
                Add_T_CODE_TREE("电能质量模组三相不平衡试验检定", "", "3", "QualityModule", "05");
                Add_T_CODE_TREE("电能质量模组三相不平衡试验检定", "", "3", "QualityModule", "05"); //TODO 有问题
                Add_T_CODE_TREE("电能质量模组电压波动试验检定", "", "3", "QualityModule", "07");
                Add_T_CODE_TREE("谐波电流", "", "3", "QualityModule", "08");
                Add_T_CODE_TREE("谐波电压", "", "3", "QualityModule", "09");
                Add_T_CODE_TREE("间谐波电流", "", "3", "QualityModule", "10");
                Add_T_CODE_TREE("间谐波电压", "", "3", "QualityModule", "11");
                Add_T_CODE_TREE("三相电压不平衡", "", "3", "QualityModule", "12");

                Add_T_CODE_TREE("证书编号配置", "", "3", "VerificationConfig", "14");
                Add_T_CODE_TREE("资产编号配置", "", "3", "VerificationConfig", "15");


                Add_T_CODE_TREE("送检单位", "SubmitInspection", "2", "ConfigSource", "", "1");
                Add_T_CODE_TREE("无送检单位", "NoSubmitInspection", "3", "SubmitInspection", "01", "1");

                Add_T_CODE_TREE("出厂编码数据源", "FactoryCodeSource", "2", "ConfigSource", "", "1");
                Add_T_CODE_TREE("无数据源", "NoSource", "3", "FactoryCodeSource", "01", "1");
                Add_T_CODE_TREE("从条形码截取", "FromBarcode", "3", "FactoryCodeSource", "02", "1");
                Add_T_CODE_TREE("从通讯地址截取", "FromAddress", "3", "FactoryCodeSource", "03", "1");

                Add_T_CODE_TREE("证书号数据源", "CertificateSource", "2", "ConfigSource", "", "1");
                Add_T_CODE_TREE("无数据源", "NoSource", "3", "CertificateSource", "01", "1");
                Add_T_CODE_TREE("从条形码截取", "FromBarcode", "3", "CertificateSource", "02", "1");
                Add_T_CODE_TREE("从当前时间点截取", "FromDate", "3", "CertificateSource", "03", "1");

                Add_T_CODE_TREE("资产号数据源", "AssetNumberSource", "2", "ConfigSource", "", "1");
                Add_T_CODE_TREE("无数据源", "NoSource", "3", "AssetNumberSource", "01", "1");
                Add_T_CODE_TREE("从条形码获取", "FromBarcode", "3", "AssetNumberSource", "02", "1");


                //添加平台接口信息配置，添加卡夫卡配置文件
                ShowMsg("软件配置/智慧工控平台");
                Add_T_CODE_TREE("智慧计量工控平台", "", "3", "MisType", "11");
                Add_T_CODE_TREE("智慧计量工控平台_新疆", "", "3", "MisType", "12");
                Add_T_CODE_TREE("智慧工控平台", "", "3", "NetworkInformation", "02");
                Add_T_CONFIG_PARA_FORMAT("05002", "是否启用|上报IP|上报端口|上报频率(s)|设备编号|工控服务器端口|工控服务器IP", "YesNo||||||", "是|192.168.100.10|44309|60||111|");

            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        private List<string> GetColumnNames(string connectionString, string tableName)
        {
            List<string> ColumnNames = new List<string>();
            if (string.IsNullOrWhiteSpace(tableName)) return ColumnNames;
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    DataTable table = connection.GetSchema("Columns", new string[] { null, null, tableName });
                    foreach (DataRow row in table.Rows)
                    {
                        string fieldName = row["COLUMN_NAME"].ToString();
                        ColumnNames.Add(fieldName);
                    }
                }
            }
            catch { }
            return ColumnNames;
        }

        public void ShowMsg(string msg)
        {
            if (OutMessage != null)
            {
                OutMessage.Invoke(this, msg);
            }
        }

        #region ADD 
        /// <summary>
        /// 添加编码树结构
        /// </summary>
        /// <param name="code_cn_name">树节点中文名</param>
        /// <param name="code_en_name">树节点英文名</param>
        /// <param name="code_level">树节点层级</param>
        /// <param name="code_parent">树节点父节点</param>
        /// <param name="code_value">树节点值</param>
        /// <param name="code_enabled">树节点启用标识符</param>
        protected internal void Add_T_CODE_TREE(string code_cn_name, string code_en_name, string code_level, string code_parent, string code_value = "", string code_enabled = "1")
        {
            bool increase_code_value = false;
            if (string.IsNullOrWhiteSpace(code_value))
            {
                increase_code_value = true;
            }
            int target_code_value = 0;

            bool exist;
            bool update = true;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                string sql = $"select ID,CODE_CN_NAME,CODE_EN_NAME,CODE_LEVEL,CODE_PARENT,CODE_VALUE,CODE_ENABLED from T_CODE_TREE where CODE_PARENT='{code_parent}' and CODE_LEVEL='{code_level}' ";
                if (!string.IsNullOrWhiteSpace(code_value))
                {
                    sql += $" and CODE_VALUE='{code_value}' ";
                }
                command.CommandText = sql;
                OleDbDataReader reader = command.ExecuteReader();
                exist = false;
                while (reader.Read())
                {
                    if (increase_code_value)
                    {
                        int.TryParse(reader["CODE_VALUE"].ToString(), out int tmp_code_value);
                        if (target_code_value <= tmp_code_value)
                        {
                            target_code_value = tmp_code_value + 1;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(code_value))
                    {
                        exist = true;
                        if (code_cn_name == reader["CODE_CN_NAME"].ToString())
                        {
                            update = false;
                        }
                    }
                    else
                    {
                        update = false;//code_value空时不能更新
                        if (code_cn_name == reader["CODE_CN_NAME"].ToString())
                        {
                            exist = true;
                        }

                    }
                }
                reader.Close();
            }
            if (exist)
            {
                if (update)
                {
                    Update_T_CODE_TREE(code_cn_name, code_en_name, code_level, code_parent, code_value, code_enabled);
                }
            }
            else
            {
                if (increase_code_value)
                {
                    code_value = target_code_value.ToString();
                }
                Insert_T_CODE_TREE(code_cn_name, code_en_name, code_level, code_parent, code_value, code_enabled);
            }

        }

        /// <summary>
        /// 添加树结构配置表参数格式
        /// </summary>
        /// <param name="config_no">唯一编码标识符</param>
        /// <param name="config_view">编码视窗，即该配置表的表头集合</param>
        /// <param name="config_code">数据源类型</param>
        /// <param name="config_default_value">数据源默认值</param>
        protected internal void Add_T_CONFIG_PARA_FORMAT(string config_no, string config_view, string config_code, string config_default_value)
        {
            bool exist;
            bool update = true;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"select CONFIG_NO,CONFIG_VIEW,CONFIG_CODE,CONFIG_DEFAULT_VALUE from T_CONFIG_PARA_FORMAT where CONFIG_NO='{config_no}';";
                OleDbDataReader reader = command.ExecuteReader();
                exist = reader.HasRows;
                if (reader.Read())
                {
                    if (config_view == reader["CONFIG_VIEW"].ToString()
                    && config_code == reader["CONFIG_CODE"].ToString()
                    && config_default_value == reader["CONFIG_DEFAULT_VALUE"].ToString())
                    {
                        update = false;
                    }
                }
                reader.Close();
            }
            if (exist)
            {
                if (update)
                {
                    Update_T_CONFIG_PARA_FORMAT(config_no, config_view, config_code, config_default_value);
                }
            }
            else
            {
                Insert_T_CONFIG_PARA_FORMAT(config_no, config_view, config_code, config_default_value);
            }
        }
        protected internal string Get_T_CONFIG_PARA_VALUE(string config_no)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"select ID,CONFIG_NO,CONFIG_VALUE from T_CONFIG_PARA_VALUE where CONFIG_NO='{config_no}';";
                OleDbDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return reader["CONFIG_VALUE"].ToString();
                }
                reader.Close();
            }
            return null;
        }
        /// <summary>
        /// 添加树结构配置参数实际值
        /// </summary>
        /// <param name="config_no"></param>
        /// <param name="config_value"></param>
        protected internal void Add_T_CONFIG_PARA_VALUE(string config_no, string config_value)
        {
            bool exist;
            bool update = true;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"select ID,CONFIG_NO,CONFIG_VALUE from T_CONFIG_PARA_VALUE where CONFIG_NO='{config_no}';";
                OleDbDataReader reader = command.ExecuteReader();
                exist = reader.HasRows;

                if (reader.Read())
                {
                    if (config_value == reader["CONFIG_VALUE"].ToString())
                    {
                        update = false;
                    }
                }
                reader.Close();
            }
            if (exist)
            {
                if (update)
                {
                    Update_T_CONFIG_PARA_VALUE(config_no, config_value);
                }
            }
            else
            {
                Insert_T_CONFIG_PARA_VALUE(config_no, config_value);
            }
        }
        /// <summary>
        /// 添加树结构方案参数格式
        /// </summary>
        /// <param name="para_no"></param>
        /// <param name="para_name"></param>
        /// <param name="para_p_code"></param>
        /// <param name="para_view"></param>
        /// <param name="para_key_rule"></param>
        /// <param name="para_view_rule"></param>
        /// <param name="result_view_id"></param>
        /// <param name="default_value"></param>
        /// <param name="check_class_name"></param>
        /// <param name="para_name_rule"></param>
        /// <param name="default_sort_no"></param>
        protected internal void Add_T_SCHEMA_PARA_FORMAT(string para_no, string para_name, string para_p_code, string para_view, string para_key_rule, string para_view_rule, string result_view_id, string default_value, string check_class_name, string para_name_rule, string default_sort_no)
        {
            bool exist;
            bool update = true;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT [PARA_NO], [PARA_NAME], [PARA_P_CODE], [PARA_VIEW], [PARA_KEY_RULE], [PARA_VIEW_RULE], [RESULT_VIEW_ID], [DEFAULT_VALUE], [CHECK_CLASS_NAME], [PARA_NAME_RULE], [DEFAULT_SORT_NO] FROM T_SCHEMA_PARA_FORMAT where PARA_NO='{para_no}'";

                OleDbDataReader reader = command.ExecuteReader();
                exist = reader.HasRows;
                if (reader.Read())
                {
                    if (para_name == reader["PARA_NAME"].ToString()
                    && para_p_code == reader["PARA_P_CODE"].ToString()
                    && para_view == reader["PARA_VIEW"].ToString()
                    && para_key_rule == reader["PARA_KEY_RULE"].ToString()
                    && para_view_rule == reader["PARA_VIEW_RULE"].ToString()
                    && result_view_id == reader["RESULT_VIEW_ID"].ToString()
                    && default_value == reader["DEFAULT_VALUE"].ToString()
                    && check_class_name == reader["CHECK_CLASS_NAME"].ToString()
                    && para_name_rule == reader["PARA_NAME_RULE"].ToString()
                    && default_sort_no == reader["DEFAULT_SORT_NO"].ToString()
                    )
                    {
                        update = false;
                    }
                }
                reader.Close();
            }
            if (exist)
            {
                if (update)
                {
                    Update_T_SCHEMA_PARA_FORMAT(para_no, para_name, para_p_code, para_view, para_key_rule, para_view_rule, result_view_id, default_value, check_class_name, para_name_rule, default_sort_no);
                }
            }
            else
            {
                Insert_T_SCHEMA_PARA_FORMAT(para_no, para_name, para_p_code, para_view, para_key_rule, para_view_rule, result_view_id, default_value, check_class_name, para_name_rule, default_sort_no);
            }
        }

        protected internal void Add_T_VIEW_CONFIG(string avr_view_id, string avr_check_name, string avr_table_name, string avr_col_show_name, string avr_col_name)
        {
            bool exist;
            bool update = true;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT [AVR_VIEW_ID],[AVR_CHECK_NAME],[AVR_TABLE_NAME],[AVR_COL_SHOW_NAME],[AVR_COL_NAME] FROM T_VIEW_CONFIG where AVR_VIEW_ID='{avr_view_id}' and AVR_CHECK_NAME='{avr_check_name}'";

                OleDbDataReader reader = command.ExecuteReader();
                exist = reader.HasRows;
                if (reader.Read())
                {
                    if (avr_table_name.Equals(reader["AVR_TABLE_NAME"].ToString())
                    && avr_col_show_name.Equals(reader["AVR_COL_SHOW_NAME"].ToString())
                    && avr_col_name.Equals(reader["AVR_COL_NAME"].ToString())
                    )
                    {
                        update = false;
                    }
                }
                reader.Close();
            }
            if (exist)
            {
                if (update)
                {
                    Update_T_VIEW_CONFIG(avr_view_id, avr_check_name, avr_table_name, avr_col_show_name, avr_col_name);
                }
            }
            else
            {
                Insert_T_VIEW_CONFIG(avr_view_id, avr_check_name, avr_table_name, avr_col_show_name, avr_col_name);
            }
        }

        protected internal void Add_T_USER_INFO(string user_id, string user_name, string user_password, string user_power)
        {

            bool exist;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT [USER_ID], [USER_NAME], [USER_PASSWORD], [USER_POWER] FROM T_USER_INFO where USER_ID = '{user_id}' or USER_NAME = '{user_name}'";

                OleDbDataReader reader = command.ExecuteReader();
                exist = reader.HasRows;

                reader.Close();
            }
            if (exist)
            {

            }
            else
            {
                Insert_T_USER_INFO(user_id, user_name, user_password, user_power);
            }
        }

        protected internal void Add_T_MENU_VIEW(string menu_name, string menu_class, string menu_image, string valid_flag, string menu_datasource, string menu_check_enable, string menu_user_visible, string manu_main, string menu_category, string sort_id)
        {
            bool exist;
            bool update = true;
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT [ID],[MENU_NAME],[MENU_CLASS],[MENU_IMAGE],[VALID_FLAG],[MENU_DATASOURCE],[MENU_CHECK_ENABLE],[MENU_USER_VISIBLE],[MENU_MAIN],[MENU_CATEGORY],[SORT_ID] FROM T_MENU_VIEW where MENU_NAME='{menu_name}'";

                OleDbDataReader reader = command.ExecuteReader();
                exist = reader.HasRows;
                if (reader.Read())
                {
                    if (menu_name.Equals(reader["MENU_NAME"].ToString())
                      && menu_class.Equals(reader["MENU_CLASS"].ToString())
                      && menu_image.Equals(reader["MENU_IMAGE"].ToString())
                      )
                    {
                        update = false;
                    }
                }
                reader.Close();
            }
            if (!exist)
            {
                Insert_T_MENU_VIEW(menu_name, menu_class, menu_image, valid_flag, menu_datasource, menu_check_enable, menu_user_visible, manu_main, menu_category, sort_id);
            }
            else if (update)
            {
                Update_T_MENU_VIEW(menu_name, menu_class, menu_image, valid_flag, menu_datasource, menu_check_enable, menu_user_visible, manu_main, menu_category, sort_id);
            }
        }

        #endregion

        #region CURD
        private void Insert_T_CODE_TREE(string target_code_cn_name, string target_code_en_name, string target_code_level, string target_code_parent, string target_code_value, string target_code_enabled)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_CODE_TREE(CODE_CN_NAME,CODE_EN_NAME,CODE_LEVEL,CODE_PARENT,CODE_VALUE,CODE_ENABLED) values(@CODE_CN_NAME,@CODE_EN_NAME,@CODE_LEVEL,@CODE_PARENT,@CODE_VALUE,@CODE_ENABLED);";

                command.Parameters.Add(new OleDbParameter("CODE_CN_NAME", target_code_cn_name));
                command.Parameters.Add(new OleDbParameter("CODE_EN_NAME", target_code_en_name));
                command.Parameters.Add(new OleDbParameter("CODE_LEVEL", target_code_level));
                command.Parameters.Add(new OleDbParameter("CODE_PARENT", target_code_parent));
                command.Parameters.Add(new OleDbParameter("CODE_VALUE", target_code_value));
                command.Parameters.Add(new OleDbParameter("CODE_ENABLED", target_code_enabled));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Update_T_CODE_TREE(string target_code_cn_name, string target_code_en_name, string target_code_level, string target_code_parent, string target_code_value, string target_code_enabled)
        {
            if (string.IsNullOrWhiteSpace(target_code_level)
                || string.IsNullOrWhiteSpace(target_code_parent)
                || string.IsNullOrWhiteSpace(target_code_value))
            {
                return;
            }
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"update T_CODE_TREE set CODE_CN_NAME=@CODE_CN_NAME,CODE_EN_NAME=@CODE_EN_NAME,CODE_ENABLED=@CODE_ENABLED where CODE_LEVEL='{target_code_level}' and CODE_PARENT='{target_code_parent}' and CODE_VALUE='{target_code_value}';";

                command.Parameters.Add(new OleDbParameter("CODE_CN_NAME", target_code_cn_name));
                command.Parameters.Add(new OleDbParameter("CODE_EN_NAME", target_code_en_name));
                command.Parameters.Add(new OleDbParameter("CODE_LEVEL", target_code_level));
                command.Parameters.Add(new OleDbParameter("CODE_PARENT", target_code_parent));
                command.Parameters.Add(new OleDbParameter("CODE_VALUE", target_code_value));
                command.Parameters.Add(new OleDbParameter("CODE_ENABLED", target_code_enabled));

                int row = command.ExecuteNonQuery();
                ShowMsg($"更新{row}行");
            }
        }

        private void Insert_T_CONFIG_PARA_FORMAT(string config_no, string config_view, string config_code, string config_default_value)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_CONFIG_PARA_FORMAT(CONFIG_NO,CONFIG_VIEW,CONFIG_CODE,CONFIG_DEFAULT_VALUE) values(@CONFIG_NO,@CONFIG_VIEW,@CONFIG_CODE,@CONFIG_DEFAULT_VALUE);";

                command.Parameters.Add(new OleDbParameter("CONFIG_NO", config_no));
                command.Parameters.Add(new OleDbParameter("CONFIG_VIEW", config_view));
                command.Parameters.Add(new OleDbParameter("CONFIG_CODE", config_code));
                command.Parameters.Add(new OleDbParameter("CONFIG_DEFAULT_VALUE", config_default_value));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Update_T_CONFIG_PARA_FORMAT(string config_no, string config_view, string config_code, string config_default_value)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"update T_CONFIG_PARA_FORMAT set CONFIG_VIEW=@CONFIG_VIEW,CONFIG_CODE=@CONFIG_CODE,CONFIG_DEFAULT_VALUE=@CONFIG_DEFAULT_VALUE where CONFIG_NO='{config_no}';";

                command.Parameters.Add(new OleDbParameter("CONFIG_VIEW", config_view));
                command.Parameters.Add(new OleDbParameter("CONFIG_CODE", config_code));
                command.Parameters.Add(new OleDbParameter("CONFIG_DEFAULT_VALUE", config_default_value));

                int row = command.ExecuteNonQuery();
                ShowMsg($"更新{row}行");
            }
        }

        private void Insert_T_CONFIG_PARA_VALUE(string target_config_no, string target_config_value)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_CONFIG_PARA_VALUE(CONFIG_NO,CONFIG_VALUE) values(@CONFIG_NO,@CONFIG_VALUE);";

                command.Parameters.Add(new OleDbParameter("CONFIG_NO", target_config_no));
                command.Parameters.Add(new OleDbParameter("CONFIG_VALUE", target_config_value));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Update_T_CONFIG_PARA_VALUE(string target_config_no, string target_config_value)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"update T_CONFIG_PARA_VALUE set CONFIG_VALUE=@CONFIG_VALUE where CONFIG_NO='{target_config_no}';";

                command.Parameters.Add(new OleDbParameter("CONFIG_VALUE", target_config_value));

                int row = command.ExecuteNonQuery();
                ShowMsg($"更新{row}行");
            }
        }

        private void Insert_T_SCHEMA_PARA_FORMAT(string para_no, string para_name, string para_p_code, string para_view, string para_key_rule, string para_view_rule, string result_view_id, string default_value, string check_class_name, string para_name_rule, string default_sort_no)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_SCHEMA_PARA_FORMAT([PARA_NO], [PARA_NAME], [PARA_P_CODE], [PARA_VIEW], [PARA_KEY_RULE], [PARA_VIEW_RULE], [RESULT_VIEW_ID], [DEFAULT_VALUE], [CHECK_CLASS_NAME], [PARA_NAME_RULE], [DEFAULT_SORT_NO]) values(@PARA_NO,@PARA_NAME,@PARA_P_CODE,@PARA_VIEW,@PARA_KEY_RULE,@PARA_VIEW_RULE,@RESULT_VIEW_ID,@DEFAULT_VALUE,@CHECK_CLASS_NAME,@PARA_NAME_RULE,@DEFAULT_SORT_NO);";

                command.Parameters.Add(new OleDbParameter("PARA_NO", para_no));
                command.Parameters.Add(new OleDbParameter("PARA_NAME", para_name));
                command.Parameters.Add(new OleDbParameter("PARA_P_CODE", para_p_code));
                command.Parameters.Add(new OleDbParameter("PARA_VIEW", para_view));
                command.Parameters.Add(new OleDbParameter("PARA_KEY_RULE", para_key_rule));
                command.Parameters.Add(new OleDbParameter("PARA_VIEW_RULE", para_view_rule));
                command.Parameters.Add(new OleDbParameter("RESULT_VIEW_ID", result_view_id));
                command.Parameters.Add(new OleDbParameter("DEFAULT_VALUE", default_value));
                command.Parameters.Add(new OleDbParameter("CHECK_CLASS_NAME", check_class_name));
                command.Parameters.Add(new OleDbParameter("PARA_NAME_RULE", para_name_rule));
                command.Parameters.Add(new OleDbParameter("DEFAULT_SORT_NO", default_sort_no));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Update_T_SCHEMA_PARA_FORMAT(string para_no, string para_name, string para_p_code, string para_view, string para_key_rule, string para_view_rule, string result_view_id, string default_value, string check_class_name, string para_name_rule, string default_sort_no)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"update T_SCHEMA_PARA_FORMAT set [PARA_NAME]=@PARA_NAME, [PARA_P_CODE]=@PARA_P_CODE, [PARA_VIEW]=@PARA_VIEW, [PARA_KEY_RULE]=@PARA_KEY_RULE, [PARA_VIEW_RULE]=@PARA_VIEW_RULE, [RESULT_VIEW_ID]=@RESULT_VIEW_ID, [DEFAULT_VALUE]=@DEFAULT_VALUE, [CHECK_CLASS_NAME]=@CHECK_CLASS_NAME, [PARA_NAME_RULE]=@PARA_NAME_RULE, [DEFAULT_SORT_NO]=@DEFAULT_SORT_NO  where [PARA_NO]='{para_no}';";

                command.Parameters.Add(new OleDbParameter("PARA_NAME", para_name));
                command.Parameters.Add(new OleDbParameter("PARA_P_CODE", para_p_code));
                command.Parameters.Add(new OleDbParameter("PARA_VIEW", para_view));
                command.Parameters.Add(new OleDbParameter("PARA_KEY_RULE", para_key_rule));
                command.Parameters.Add(new OleDbParameter("PARA_VIEW_RULE", para_view_rule));
                command.Parameters.Add(new OleDbParameter("RESULT_VIEW_ID", result_view_id));
                command.Parameters.Add(new OleDbParameter("DEFAULT_VALUE", default_value));
                command.Parameters.Add(new OleDbParameter("CHECK_CLASS_NAME", check_class_name));
                command.Parameters.Add(new OleDbParameter("PARA_NAME_RULE", para_name_rule));
                command.Parameters.Add(new OleDbParameter("DEFAULT_SORT_NO", default_sort_no));

                int row = command.ExecuteNonQuery();
                ShowMsg($"更新{row}行");
            }
        }

        private void Insert_T_VIEW_CONFIG(string avr_view_id, string avr_check_name, string avr_table_name, string avr_col_show_name, string avr_col_name)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_VIEW_CONFIG([AVR_VIEW_ID],[AVR_CHECK_NAME],[AVR_TABLE_NAME],[AVR_COL_SHOW_NAME],[AVR_COL_NAME]) values(@AVR_VIEW_ID,@AVR_CHECK_NAME,@AVR_TABLE_NAME,@AVR_COL_SHOW_NAME,@AVR_COL_NAME);";

                command.Parameters.Add(new OleDbParameter("AVR_VIEW_ID", avr_view_id));
                command.Parameters.Add(new OleDbParameter("AVR_CHECK_NAME", avr_check_name));
                command.Parameters.Add(new OleDbParameter("AVR_TABLE_NAME", avr_table_name));
                command.Parameters.Add(new OleDbParameter("AVR_COL_SHOW_NAME", avr_col_show_name));
                command.Parameters.Add(new OleDbParameter("AVR_COL_NAME", avr_col_name));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Update_T_VIEW_CONFIG(string avr_view_id, string avr_check_name, string avr_table_name, string avr_col_show_name, string avr_col_name)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"update T_VIEW_CONFIG set [AVR_VIEW_ID]=@AVR_VIEW_ID, [AVR_CHECK_NAME]=@AVR_CHECK_NAME, [AVR_TABLE_NAME]=@AVR_TABLE_NAME, [AVR_COL_SHOW_NAME]=@AVR_COL_SHOW_NAME, [AVR_COL_NAME]=@AVR_COL_NAME where [AVR_VIEW_ID]='{avr_view_id}' and [AVR_CHECK_NAME]='{avr_check_name}';";

                command.Parameters.Add(new OleDbParameter("AVR_VIEW_ID", avr_view_id));
                command.Parameters.Add(new OleDbParameter("AVR_CHECK_NAME", avr_check_name));
                command.Parameters.Add(new OleDbParameter("AVR_TABLE_NAME", avr_table_name));
                command.Parameters.Add(new OleDbParameter("AVR_COL_SHOW_NAME", avr_col_show_name));
                command.Parameters.Add(new OleDbParameter("AVR_COL_NAME", avr_col_name));

                int row = command.ExecuteNonQuery();
                ShowMsg($"更新{row}行");
            }
        }

        private void Insert_T_USER_INFO(string user_id, string user_name, string user_password, string user_power)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_USER_INFO([USER_ID], [USER_NAME], [USER_PASSWORD], [USER_POWER]) values(@USER_ID,@USER_NAME,@USER_PASSWORD,@USER_POWER);";

                command.Parameters.Add(new OleDbParameter("USER_ID", user_id));
                command.Parameters.Add(new OleDbParameter("USER_NAME", user_name));
                command.Parameters.Add(new OleDbParameter("USER_PASSWORD", user_password));
                command.Parameters.Add(new OleDbParameter("USER_POWER", user_power));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Insert_T_MENU_VIEW(string menu_name, string menu_class, string menu_image, string valid_flag, string menu_datasource, string menu_check_enable, string menu_user_visible, string menu_main, string menu_category, string sort_id)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"insert into T_MENU_VIEW([MENU_NAME],[MENU_CLASS],[MENU_IMAGE],[VALID_FLAG],[MENU_DATASOURCE],[MENU_CHECK_ENABLE],[MENU_USER_VISIBLE],[MENU_MAIN],[MENU_CATEGORY],[SORT_ID]) values(@MENU_NAME,@MENU_CLASS,@MENU_IMAGE,@VALID_FLAG,@MENU_DATASOURCE,@MENU_CHECK_ENABLE,@MENU_USER_VISIBLE,@MENU_MAIN,@MENU_CATEGORY,@SORT_ID);";

                command.Parameters.Add(new OleDbParameter("MENU_NAME", menu_name));
                command.Parameters.Add(new OleDbParameter("MENU_CLASS", menu_class));
                command.Parameters.Add(new OleDbParameter("MENU_IMAGE", menu_image));
                command.Parameters.Add(new OleDbParameter("VALID_FLAG", valid_flag));
                command.Parameters.Add(new OleDbParameter("MENU_DATASOURCE", menu_datasource));
                command.Parameters.Add(new OleDbParameter("MENU_CHECK_ENABLE", menu_check_enable));
                command.Parameters.Add(new OleDbParameter("MENU_USER_VISIBLE", menu_user_visible));
                command.Parameters.Add(new OleDbParameter("MENU_MAIN", menu_main));
                command.Parameters.Add(new OleDbParameter("MENU_CATEGORY", menu_category));
                command.Parameters.Add(new OleDbParameter("SORT_ID", sort_id));

                ShowMsg($"新增{command.ExecuteNonQuery()}行");
            }
        }

        private void Update_T_MENU_VIEW(string menu_name, string menu_class, string menu_image, string valid_flag, string menu_datasource, string menu_check_enable, string menu_user_visible, string menu_main, string menu_category, string sort_id)
        {
            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                OleDbCommand command = connection.CreateCommand();
                command.CommandText = $"update T_MENU_VIEW set [MENU_CLASS]=@MENU_CLASS, [MENU_IMAGE]=@MENU_IMAGE, [VALID_FLAG]=@VALID_FLAG, [MENU_DATASOURCE]=@MENU_DATASOURCE, [MENU_CHECK_ENABLE]=@MENU_CHECK_ENABLE, [MENU_USER_VISIBLE]=@MENU_USER_VISIBLE, [MENU_MAIN]=@MENU_MAIN, [MENU_CATEGORY]=@MENU_CATEGORY, [SORT_ID]=@SORT_ID where [MENU_NAME]='{menu_name}';";

                command.Parameters.Add(new OleDbParameter("MENU_CLASS", menu_class));
                command.Parameters.Add(new OleDbParameter("MENU_IMAGE", menu_image));
                command.Parameters.Add(new OleDbParameter("VALID_FLAG", valid_flag));
                command.Parameters.Add(new OleDbParameter("MENU_DATASOURCE", menu_datasource));
                command.Parameters.Add(new OleDbParameter("MENU_CHECK_ENABLE", menu_check_enable));
                command.Parameters.Add(new OleDbParameter("MENU_USER_VISIBLE", menu_user_visible));
                command.Parameters.Add(new OleDbParameter("MENU_MAIN", menu_main));
                command.Parameters.Add(new OleDbParameter("MENU_CATEGORY", menu_category));
                command.Parameters.Add(new OleDbParameter("SORT_ID", sort_id));
                command.Parameters.Add(new OleDbParameter("MENU_NAME", menu_name));

                int row = command.ExecuteNonQuery();
                ShowMsg($"更新{row}行");
            }
        }

        #endregion CURD
    }
}
