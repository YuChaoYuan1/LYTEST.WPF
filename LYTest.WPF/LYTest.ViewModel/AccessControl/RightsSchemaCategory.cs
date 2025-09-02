#define SampleCheck
#define FullCheck

using LYTest.Core.Enum;
using LYTest.ViewModel.Model;
using System.Collections.Generic;

namespace LYTest.ViewModel.AccessControl
{
    public sealed class RightsSchemaCategory
    {
        public static readonly List<SchemaCategory> Categories = new List<SchemaCategory>()
        {
            new SchemaCategory("外观检查","10",0b111),
            new SchemaCategory("外观检查","10001",0b111),

            new SchemaCategory("预热试验","11",0b111),
            new SchemaCategory("预热试验","11001",0b111),

            new SchemaCategory("准确度试验","12",0b00000111),
            new SchemaCategory("基本误差试验","12001",0b111),
            new SchemaCategory("起动试验","12002",0b111),
            new SchemaCategory("潜动试验","12003",0b111),
            new SchemaCategory("电能表常数试验","12004",0b111),

#if FullCheck
            new SchemaCategory("影响量试验","12005",0b100),
            new SchemaCategory("第N次谐波试验","12005001",0b100),
            new SchemaCategory("方顶波波形试验","12005002",0b100),
            new SchemaCategory("尖顶波波形试验","12005003",0b100),
            new SchemaCategory("间谐波波形试验","12005004",0b100),
            new SchemaCategory("奇次谐波波形试验","12005005",0b100),
            new SchemaCategory("偶次谐波波形试验","12005006",0b100),
            new SchemaCategory("频率改变","12005007",0b100),
            new SchemaCategory("电压改变","12005008",0b100),
            new SchemaCategory("负载不平衡试验","12005009",0b100),
            new SchemaCategory("辅助装置试验","12005010",0b100),
            new SchemaCategory("逆相序试验","12005011",0b100),
            new SchemaCategory("一相或两相电压中断试验","12005012",0b100),
            new SchemaCategory("高次谐波试验","12005013",0b100),
            new SchemaCategory("冲击试验","12005014",0b100),
            new SchemaCategory("振动试验","12005015",0b100),
            new SchemaCategory("辅助电源电压改变试验","12005016",0b100),
#endif

            new SchemaCategory("标准偏差试验","12006",0b111),
#if SampleCheck
            new SchemaCategory("初始固有误差","12007",0b110),
#endif
#if FullCheck
            new SchemaCategory("谐波电能准确度试验","12008",0b100),
            new SchemaCategory("谐波测量准确度试验","12009",0b100),
            new SchemaCategory("外部工频磁场试验","12017",0b100),
            new SchemaCategory("外部工频磁场(无负载条件)试验","12018",0b100),
            new SchemaCategory("外部工频磁场试验2","12019",0b100),


#endif
            new SchemaCategory("起动试验","12010",0b000),
            new SchemaCategory("潜动试验","12011",0b000),
            new SchemaCategory("基本误差试验","12012",0b000),
            new SchemaCategory("电能表常数试验","12013",0b000),
#if FullCheck
            new SchemaCategory("端子座温度监测准确度","12014",0b100),
#endif
#if SampleCheck
            new SchemaCategory("初始固有误差试验","12015",0b110),
#endif
            new SchemaCategory("走字48h试验","12016",0b110),

            new SchemaCategory("标准表校准","13",0b000),
            new SchemaCategory("标准表电压校准","13001",0b000),
            new SchemaCategory("标准表电流校准","13002",0b000),
            new SchemaCategory("标准表功率校准","13003",0b000),

            new SchemaCategory("预先调试","14",0b111),
            new SchemaCategory("接线检查","14001",0b111),
            new SchemaCategory("密钥更新_预先调试","14002",0b111),
            new SchemaCategory("密钥恢复_预先调试","14003",0b111),
            new SchemaCategory("通信测试_预先调试","14004",0b111),
            new SchemaCategory("钱包初始化_预先调试","14005",0b111),
#if FullCheck
            new SchemaCategory("蓝牙预处理","14000",0b100),
#endif
            new SchemaCategory("接线检查","14006",0b000),
            new SchemaCategory("GPS预校时","14007",0b111),
            new SchemaCategory("写地址_预先调试","14008",0b110),
            new SchemaCategory("表位电流开路检查","14009",0b111),

            new SchemaCategory("多功能试验","15",0b111),
            new SchemaCategory("GPS对时","15001",0b111),
            new SchemaCategory("由电源供电的时钟误差","15002",0b111),
            new SchemaCategory("需量示值误差","15003",0b111),
            new SchemaCategory("电量清零","15006",0b111),
            new SchemaCategory("需量清空","15007",0b111),
            new SchemaCategory("通信测试","15008",0b111),
#if FullCheck
            new SchemaCategory("负载电流快速改变","15009",0b100),
#endif
            new SchemaCategory("费率时段检查","15011",0b111),
            new SchemaCategory("时段投切","15012",0b111),
            new SchemaCategory("电子指示显示器电能示值组合误差","15013",0b111),
            new SchemaCategory("时钟示值误差","15014",0b111),
#if FullCheck
            new SchemaCategory("测量及监测试误差","15015",0b100),
            new SchemaCategory("停电转存试验","15016",0b100),
#endif
            new SchemaCategory("读取电量","15010",0b111),
#if FullCheck
            new SchemaCategory("蓝牙连接","15017",0b100),
#endif
#if SampleCheck
            new SchemaCategory("费率时段示值误差","15019",0b110),
#endif
#if FullCheck
            new SchemaCategory("闰年判断功能","15020",0b100),
            new SchemaCategory("采用备用电源工作的时钟试验","15021",0b100),
            new SchemaCategory("零线电流检测","15022",0b100),
#endif
            new SchemaCategory("日计时误差","15023",0b111),
#if FullCheck
            new SchemaCategory("GPS对时","15025",0b100),
            new SchemaCategory("电能示值组合误差","15027",0b100),
#endif
            new SchemaCategory("时钟示值误差_黑龙江","15028",0b000),
            new SchemaCategory("电能示值组合误差_西安","15029",0b000),
#if FullCheck
            new SchemaCategory("交流电压暂降和短时中断","15030",0b100),
            new SchemaCategory("接地故障试验","15031",0b100),
            new SchemaCategory("预置内容设置","15032",0b100),
            new SchemaCategory("预置内容检查","15033",0b100),
            new SchemaCategory("电流回路阻抗","15034",0b100),
            new SchemaCategory("电量法需量功能试验","15041",0b111),

#endif

            new SchemaCategory("费控功能试验","25",0b111),
            new SchemaCategory("身份认证","25001",0b111),
#if SampleCheck
            new SchemaCategory("远程控制","25002",0b110),
            new SchemaCategory("报警功能","25003",0b110),
            new SchemaCategory("远程保电","25004",0b110),
            new SchemaCategory("保电解除","25005",0b110),
            new SchemaCategory("数据回抄","25006",0b110),
#endif
            new SchemaCategory("钱包初始化","25007",0b111),
            new SchemaCategory("密钥更新","25008",0b111),
            new SchemaCategory("密钥恢复","25009",0b111),
#if FullCheck
            new SchemaCategory("电量清零","25010",0b100),
#endif
#if SampleCheck
            new SchemaCategory("剩余电量递减度","25011",0b110),
#endif
            new SchemaCategory("负荷开关","25012",0b000),
            new SchemaCategory("参数设置","25013",0b000),
#if SampleCheck
            new SchemaCategory("控制功能","25014",0b110),
#endif
#if FullCheck
            new SchemaCategory("身份认证_计量芯","25015",0b100),
#endif
            new SchemaCategory("密钥更新","25016",0b000),
            new SchemaCategory("远程控制_西安","25017",0b000),
            new SchemaCategory("远程清零","25018",0b000),
            new SchemaCategory("保电控制","25019",0b000),
            new SchemaCategory("拉合闸控制","25020",0b000),
            new SchemaCategory("报警控制","25021",0b000),
            new SchemaCategory("最后核验","25022",0b000),

            new SchemaCategory("通讯协议检查试验","17",0b111),
            new SchemaCategory("通讯协议检查","17001",0b111),
#if FullCheck
            new SchemaCategory("通讯协议检查2","17003",0b100),
#endif
            new SchemaCategory("通讯协议检查试验_黑龙江","17004",0b000),
            new SchemaCategory("参数验证","17002",0b000),
            new SchemaCategory("通讯协议检查试验2_黑龙江","17005",0b000),
            new SchemaCategory("状态核验","17006",0b000),

#if SampleCheck
            new SchemaCategory("一致性试验","18",0b110),
            new SchemaCategory("误差一致性","18001",0b110),
            new SchemaCategory("变差要求试验","18002",0b110),
            new SchemaCategory("负载电流升降变差","18003",0b110),
            new SchemaCategory("电流过载","18004",0b100),
            new SchemaCategory("重复性","18005",0b110),
#endif

#if FullCheck
            new SchemaCategory("载波功能试验","19",0b100),
            new SchemaCategory("载波通讯试验","19002",0b100),
            new SchemaCategory("芯片ID认证","19004",0b100),
            new SchemaCategory("载波模块互换试验","19003",0b000),
            new SchemaCategory("功耗试验","19005",0b000),
#endif
            new SchemaCategory("电气性能试验","20",0b111),
            new SchemaCategory("工频耐压试验","20001",0b111),
            
#if FullCheck
            new SchemaCategory("功耗实验","20002",0b100),
#endif

#if FullCheck
            new SchemaCategory("智能表功能试验","21",0b100),
            new SchemaCategory("计量功能","21001",0b100),
            new SchemaCategory("计时功能","21002",0b100),
            new SchemaCategory("费率时段功能","21003",0b100),
            new SchemaCategory("脉冲输出功能","21004",0b100),
            new SchemaCategory("最大需量功能","21005",0b100),
            new SchemaCategory("停电抄表功能","21006",0b100),
            new SchemaCategory("显示功能","21007",0b111),
            new SchemaCategory("时区时段功能","21008",0b100),

            new SchemaCategory("事件记录试验","22",0b100),
            new SchemaCategory("失压记录","22001",0b100),
            new SchemaCategory("过压记录","22002",0b100),
            new SchemaCategory("欠压记录","22003",0b100),
            new SchemaCategory("失流记录","22004",0b100),
            new SchemaCategory("断流记录","22005",0b100),
            new SchemaCategory("过流记录","22006",0b100),
            new SchemaCategory("过载记录","22007",0b100),
            new SchemaCategory("断相记录","22008",0b100),
            new SchemaCategory("掉电记录","22009",0b100),
            new SchemaCategory("全失压记","22010",0b100),
            new SchemaCategory("电压不平衡记录","22011",0b100),
            new SchemaCategory("电流不平衡记录","22012",0b100),
            new SchemaCategory("电压逆相序记录","22013",0b100),
            new SchemaCategory("电流逆相序记录","22014",0b100),
            new SchemaCategory("校时记录","22015",0b100),
            new SchemaCategory("开表盖记录","22016",0b100),
            new SchemaCategory("开端钮盒记录","22017",0b100),
            new SchemaCategory("编程记录","22018",0b100),
            new SchemaCategory("需量清零记录","22019",0b100),
            new SchemaCategory("事件清零记录","22020",0b100),
            new SchemaCategory("电表清零记录","22021",0b100),
            new SchemaCategory("潮流反向记录","22022",0b100),
            new SchemaCategory("功率反向记录","22023",0b100),
            new SchemaCategory("需量超限记录","22024",0b100),
            new SchemaCategory("功率因数超下限记录","22025",0b100),
            new SchemaCategory("恒定磁场干扰记录","22026",0b100),
            new SchemaCategory("跳闸合闸事件记录","22027",0b100),
            new SchemaCategory("时钟故障事件记录","22028",0b100),
            new SchemaCategory("事件跟随上报","22029",0b100),
            new SchemaCategory("事件主动上报(载波)","22030",0b100),
            new SchemaCategory("广播校时事件记录","22031",0b100),
            new SchemaCategory("零线电流异常事件","22032",0b100),

            new SchemaCategory("冻结功能试验","23",0b100),
            new SchemaCategory("定时冻结","23001",0b100),
            new SchemaCategory("约定冻结","23002",0b100),
            new SchemaCategory("瞬时冻结","23003",0b100),
            new SchemaCategory("日冻结","23004",0b100),
            new SchemaCategory("整点冻结","23005",0b100),
            new SchemaCategory("分钟冻结","23006",0b100),
            new SchemaCategory("小时冻结","23007",0b100),
            new SchemaCategory("月冻结","23008",0b100),
            new SchemaCategory("年冻结","23009",0b100),
            new SchemaCategory("结算日冻结","23010",0b100),
            new SchemaCategory("时区表切换冻结","23011",0b100),
            new SchemaCategory("日时段表切换冻结","23012",0b100),

            new SchemaCategory("红外通信试验","24",0b100),
            new SchemaCategory("红外通信试验","24001",0b100),

            new SchemaCategory("自热试验","26",0b100),
            new SchemaCategory("自热试验","26001",0b100),

            new SchemaCategory("计量性能保护试验","27",0b100),
            new SchemaCategory("数据同步","27003",0b100),

            new SchemaCategory("负荷记录试验","28",0b100),
            new SchemaCategory("负荷记录","28001",0b100),

            new SchemaCategory(nameof(ProjectID.三相电流不平衡),ProjectID.三相电流不平衡,0b100),
            new SchemaCategory(nameof(ProjectID.电能质量模组闪变试验检定),ProjectID.电能质量模组闪变试验检定,0b100),
            new SchemaCategory(nameof(ProjectID.谐波有功功率),ProjectID.谐波有功功率,0b100),
            new SchemaCategory(nameof(ProjectID.电能质量模组电压偏差试验检定),ProjectID.电能质量模组电压偏差试验检定,0b100),
            new SchemaCategory(nameof(ProjectID.电压暂升电压暂降和短时中断),ProjectID.电压暂升电压暂降和短时中断,0b100),
            new SchemaCategory(nameof(ProjectID.电能质量模组电压波动试验检定),ProjectID.电能质量模组电压波动试验检定,0b100),
            new SchemaCategory(nameof(ProjectID.谐波电流),ProjectID.谐波电流,0b100),
            new SchemaCategory(nameof(ProjectID.谐波电压),ProjectID.谐波电压,0b100),
            new SchemaCategory(nameof(ProjectID.间谐波电流),ProjectID.间谐波电流,0b100),
            new SchemaCategory(nameof(ProjectID.间谐波电压),ProjectID.间谐波电压,0b100),
            new SchemaCategory(nameof(ProjectID.三相电压不平衡),ProjectID.三相电压不平衡,0b100),
            new SchemaCategory(nameof(ProjectID.电能质量模组频率偏差试验),ProjectID.电能质量模组频率偏差试验,0b100),

#endif

    };
    }
}
