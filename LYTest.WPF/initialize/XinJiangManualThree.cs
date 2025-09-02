using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace initialize
{
    //人工三相台
    class XinJiangManualThree : Universal
    {
        public override void Execute()
        {
            try
            {
                base.Execute();

                ShowMsg("软件配置");
                string cfgValue = Get_T_CONFIG_PARA_VALUE("01001");
                if (string.IsNullOrWhiteSpace(cfgValue))
                {
                    Add_T_CONFIG_PARA_VALUE("01001", "1|三相台|36|电能表|手动模式|否|是|是|正转");
                }
                else
                {
                    string[] cfgs = cfgValue.Split('|');
                    if (cfgs.Length > 2)
                    {
                        Add_T_CONFIG_PARA_VALUE("01001", $"{cfgs[0]}|三相台|{cfgs[2]}|电能表|手动模式|否|是|是|正转");
                    }
                }

                //Add_T_CONFIG_PARA_VALUE("02007", "否|15|否");//自动压接|压接等待时间|直接互感自动切换

                Add_T_CODE_TREE("通信地址配置", "", "3", "VerificationConfig", "13");
                Add_T_CONFIG_PARA_FORMAT("02013", "从条形码解析|从左到右|起始位置（1~n）|截取长度", "YesNo|YesNo||", "是|否|1|12");
                Add_T_CONFIG_PARA_VALUE("02013", "是|否|2|12");

                //修改 日计时 
                Add_T_SCHEMA_PARA_FORMAT("15002", "由电源供电的时钟误差", "||", "误差限(s/d)|误差次数|检定圈数", "False|False|False", "False|False|False", "004", "0.5|5|60", "AccurateTest.ClockError", "1", "032");
                Add_T_SCHEMA_PARA_FORMAT("15023", "日计时误差", "|||||", "误差限(s/d)|误差次数|检定圈数|标准时钟频率|被检时钟频率|项目编号", "False|False|False|False|False|False", "False|False|False|False|False|False", "15023", "0.5|5|60|50||", "AccurateTest.ClockError", "1", "");
                // 需量示值开放误差限
                Add_T_SCHEMA_PARA_FORMAT("15003", "需量示值误差", "MaxDemand|||||", "最大需量|功率方向|需量周期|滑差时间|滑差次数|误差限(±)", "True|False|False|False|False|False", "True|False|False|False|False|False", "153", "Imax|正向有功|15|1|1|2", "Multi.Dgn_MaxDemand", "1", "033");

                Add_T_CODE_TREE("初始固有误差试验", "", "3", "WcType", "15");
                Add_T_SCHEMA_PARA_FORMAT("12015", "初始固有误差试验", "ErrorType|PowerDirection|PowerElement|PowerFactor|CurrentTimes|YesNo|YesNo||", "误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序|误差圈数(Ib)|误差限倍数(%)", "True|True|True|True|True|True|True|False|False", "False|True|True|True|True|False|False|False|False", "012", "基本误差|正向有功|H|1.0|Ib|否|否|2|100", "AccurateTest.BasicError", "0", "013");

                //写地址
                Add_T_CODE_TREE("写地址_预先调试", "", "3", "PrepareTest", "08");
                Add_T_SCHEMA_PARA_FORMAT("14008", "写地址_预先调试", "", "", "", "", "14008", "", "PrepareTest.PreWriteAddress", "1", "");
                Add_T_VIEW_CONFIG("14008", "写地址_预先调试", "METER_COMMUNICATION", "条码中的地址|表内地址,结论", "MD_VALUE,MD_RESULT");

                //电量清零 改为默认15元
                Add_T_SCHEMA_PARA_FORMAT("15006", "电量清零", "", "钱包初始化值金额", "False", "False", "021", "15", "Multi.Dgn_ClearEnerfy", "1", "034");
                //最大需量电流 增加 2Itr
                Add_T_CODE_TREE("2Itr", "", "3", "MaxDemand", "6");
                //增加显示功能测试方法配置

                ShowMsg("配置营销接口");
                Add_T_CODE_TREE("新疆生产调度平台", "", "3", "MisType");
                //                Add_T_CONFIG_PARA_VALUE("03001"
                //, "新疆生产调度平台|条形码|10.218.15.201|11521|pmcpdp|sxykjd|djkyxs_121#|http://10.218.163.113:17011/InterfaceWS/InterfaceBusiness/services/DetectService?wsdl|否|10.218.15.201|11521|pmcpdp|sxykjd|djkyxs_121#|否");

                Add_T_CODE_TREE("表位电流开路检查", "", "3", "PrepareTest", "09");
                Add_T_SCHEMA_PARA_FORMAT("14009", "表位电流开路检查", "", "", "", "", "14009", "", "PrepareTest.CurrentOpenCircuit", "1", "");
                Add_T_VIEW_CONFIG("14009", "表位电流开路检查", "METER_COMMUNICATION", "检查信息,结论", "MD_VALUE,MD_RESULT");
                Add_T_VIEW_CONFIG("004", "日计时试验", "METER_COMMUNICATION", "误差1|误差2|误差3|误差4|误差5|平均值|化整值|误差限(s/d),结论", "MD_VALUE,MD_RESULT");

            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }
    }
}
