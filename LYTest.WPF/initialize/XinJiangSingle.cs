using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace initialize
{
    class XinJiangSingle : Universal
    {
        public override void Execute()
        {
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
                try
                {
                    //TODO:判断列名存在
                    using (OleDbConnection connection = new OleDbConnection(ConnectionStringMeterData))
                    {
                        connection.Open();
                        OleDbCommand command = connection.CreateCommand();
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = "alter table METER_INFO add column MD_SORT Text(50);";//类别：智能表，物联电能表
                        command.ExecuteNonQuery();
                    }
                }
                catch { }
                try
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
                catch { }

                ShowMsg("软件配置");
                Add_T_CONFIG_PARA_FORMAT("01001"
                    , "台体编号|台体类型|表位数|表类型|检定模式|自动登入|是否弹出错误提示|密钥更新前是否弹出提示|流水线方向"
                    , "|DeviceType||MeterType|VerifyModel|YesNo|YesNo|YesNo|PipelineDirection"
                    , "1|单相台|24|电能表|手动模式|否|是|是|正转");
                Add_T_CONFIG_PARA_VALUE("01001", "1|单相台|60|电能表|自动模式|是|否|否|正转");
                //地区名称|误差限比例(%)
                Add_T_CONFIG_PARA_VALUE("01002", "全国|60");
                //源稳定时间|写操作时提示|走字电量输入方式|走字实验前电表清零|清零时校时
                Add_T_CONFIG_PARA_VALUE("02002", "10|否|否|否|否");

                //不合格率报警(%)|加密解密方式|启动潜动时同步检定协议|合格检定点是否重复检定|密钥更新模式选择|耐压|快速试验
                Add_T_CONFIG_PARA_FORMAT("02008", "不合格率报警(%)|加密解密方式|启动潜动时同步检定协议|合格检定点是否重复检定|密钥更新模式选择|耐压|快速试验", "|CryptoModel|YesNo|YesNo|KeyUpdataModel|YesNo|YesNo", "0|根据表协议|否|否|全部更新|否|否");
                Add_T_CONFIG_PARA_VALUE("02008", "2|根据表协议|否|否|只更新合格表位|否|是");
                //功耗表位选择
                Add_T_CONFIG_PARA_VALUE("02009", "1,2,3,4,5,6");
                //是否开始二次巡检|不合格报警数量
                Add_T_CONFIG_PARA_VALUE("02011", "是|600");
                //不合格表位是否跳出
                Add_T_CONFIG_PARA_VALUE("02012", "否");


                Add_T_CODE_TREE("通信地址配置", "", "3", "VerificationConfig", "13");
                Add_T_CONFIG_PARA_FORMAT("02013", "从条形码解析|从左到右|起始位置（1~n）|截取长度", "YesNo|YesNo||", "是|否|1|12");
                Add_T_CONFIG_PARA_VALUE("02013", "是|否|2|12");

                Add_T_CODE_TREE("走字试验法", "", "3", "ZouZiMethod", "2");
                Add_T_CODE_TREE("计读脉冲法", "", "3", "ZouZiMethod", "3");
                Add_T_SCHEMA_PARA_FORMAT("12004", "走字试验"
                    , "PowerDirection|PowerElement|PowerFactor|CurrentTimes|ZouZiMethod|Rate||"
                    , "功率方向|功率元件|功率因素|电流倍数|走字试验方法类型|费率|走字电量(度)|走字时间(分钟)"
                    , "True|True|True|True|False|True|False|False"
                    , "True|True|True|True|False|True|False|False"
                    , "003"
                    , "正向有功|H|1.0|0.5Imax|计读脉冲法|总|0.5|0"
                    , "AccurateTest.ConstantVerify2"
                    , "1", "014");


                ShowMsg("配置营销接口");
                Add_T_CODE_TREE("新疆生产调度平台", "", "3", "MisType");
                Add_T_CODE_TREE("条形码", "", "3", "DownLoadNumber");
                Add_T_CODE_TREE("出厂编号", "", "3", "DownLoadNumber");
                Add_T_CODE_TREE("表位号", "", "3", "DownLoadNumber");

                Add_T_CONFIG_PARA_FORMAT("03001"
, "接口类型|下载参数标识|系统IP地址|系统端口号|系统数据源|数据库用户名|数据库密码|WebService链接|上传实时数据||||||是否下载方案"
, "MisType|DownLoadNumber|||||||YesNo||||||YesNo"
, "无|条形码||||||||否||||||是");
                Add_T_CONFIG_PARA_VALUE("03001"
, "新疆生产调度平台|条形码|10.218.15.201|11521|pmcpdp|sxykjd|djkyxs_121#|http://10.218.163.113:17011/InterfaceWS/InterfaceBusiness/services/DetectService?wsdl|否|10.218.15.201|11521|pmcpdp|sxykjd|djkyxs_121#|是");

                //生产调度平台 MDS
                //http://10.218.163.113:17011/InterfaceWS/InterfaceBusiness/services/DetectService?wsdl

                //检定控制系统 DCS
                //http://10.218.222.101:8880/XJ-CONTR-DCS/services/SingleServicePort


                ShowMsg("配置加密机");
                Add_T_CODE_TREE("国网加密机", "", "3", "DogType");
                Add_T_CODE_TREE("公钥", "", "3", "DogCheckingType");
                Add_T_CODE_TREE("私钥", "", "3", "DogCheckingType");
                Add_T_CODE_TREE("服务器版", "", "3", "DogConnectMode");
                Add_T_CODE_TREE("直连密码机版", "", "3", "DogConnectMode");

                Add_T_CONFIG_PARA_FORMAT("04001"
                    , "加密机类型|加密机IP|加密机端口|加密机密钥|默认认证状态|是否进行密码机服务器连接|加密机连接模式|加密机超时时间", "DogType||||DogCheckingType|YesNo|DogConnectMode|"
                    , "");
                Add_T_CONFIG_PARA_VALUE("04001"
                    , "国网加密机|10.218.222.223|8001|11111111|公钥|是|服务器版|45");

                ShowMsg("配置集控");
                Add_T_CODE_TREE("集控线配置", "", "3", "NetworkInformation", "01");
                Add_T_CONFIG_PARA_FORMAT("05001"
                    , "服务器IP|服务器端口|流水线编号|集控线台体编号"
                    , "|||"
                    , "|||");
                Add_T_CONFIG_PARA_VALUE("05001"
                    , "10.218.222.194|9999|1|1");

                ShowMsg("配置试验项目");
                Add_T_CODE_TREE("预置内容设置", "", "3", "DgnItem", "32");
                Add_T_SCHEMA_PARA_FORMAT("15032", "预置内容设置"
                    , "ConnProtocolDataName|||||ComProtocolType|"
                    , "数据项名称|标识编码|长度|小数位|数据格式|功能|标准数据"
                    , "True|False|False|False|False|True|False"
                    , "True|False|False|False|False|True|False"
                    , "017"
                    , "|||||写|"
                    , "ConnProtocolTest.ConnProtocol"
                    , "0"
                    , "0");

                Add_T_CODE_TREE("预置内容检查", "", "3", "DgnItem", "33");
                Add_T_SCHEMA_PARA_FORMAT("15033", "预置内容检查"
                    , "ConnProtocolDataName|||||ComProtocolType|"
                    , "数据项名称|标识编码|长度|小数位|数据格式|功能|标准数据"
                    , "True|False|False|False|False|True|False"
                    , "True|False|False|False|False|True|False"
                    , "017"
                    , "|||||读|"
                    , "ConnProtocolTest.ConnProtocol"
                    , "0"
                    , "0");
                //可修改中文名、英文名、enabled
                Add_T_CODE_TREE("通信测试", "", "3", "DgnItem", "08");
                Add_T_SCHEMA_PARA_FORMAT("15008", "通信测试", "", "", "", "", "154", "", "Multi.Dgn_CommTest", "1", "");

                //修改 日计时 
                Add_T_SCHEMA_PARA_FORMAT("15002", "由电源供电的时钟误差", "||", "误差限(s/d)|误差次数|检定圈数", "False|False|False", "False|False|False", "004", "0.5|5|60", "AccurateTest.ClockError", "1", "032");

                Add_T_SCHEMA_PARA_FORMAT("15023", "日计时误差", "|||||", "误差限(s/d)|误差次数|检定圈数|标准时钟频率|被检时钟频率|项目编号", "False|False|False|False|False|False", "False|False|False|False|False|False", "15023", "0.5|5|60|50||", "AccurateTest.ClockError", "1", "");
                //修改 显示功能 配置
                Add_T_SCHEMA_PARA_FORMAT("21007", "显示功能", "", "数据项", "False", "False", "217", "", "Function.FC_Show", "1", "");

                ShowMsg("配置用户信息");
                Add_T_USER_INFO("10001380", "10001380", "123456", "1");
                Add_T_USER_INFO("20014218", "20014218", "123456", "1");
                Add_T_USER_INFO("XYC1519", "XYC1519", "123456", "1");

                Add_T_CODE_TREE("初始固有误差试验", "", "3", "WcType", "15");
                Add_T_SCHEMA_PARA_FORMAT("12015", "初始固有误差试验", "ErrorType|PowerDirection|PowerElement|PowerFactor|CurrentTimes|YesNo|YesNo||", "误差试验类型|功率方向|功率元件|功率因素|电流倍数|添加谐波|逆相序|误差圈数(Ib)|误差限倍数(%)", "True|True|True|True|True|True|True|False|False", "False|True|True|True|True|False|False|False|False", "012", "基本误差|正向有功|H|1.0|Ib|否|否|2|100", "AccurateTest.BasicError", "0", "013");

            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

    }
}
