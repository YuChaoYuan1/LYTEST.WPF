
namespace LYTest.Core.Enum
{
    /// <summary>
    /// 检定项目编码--主要用于数据上传部分
    /// </summary>
    public class ProjectID
    {
        //modify yjt 20221121 新增全性能的试验ID 调整位置
        //add yjt 20220306 新增 外观检查 需量清空 通讯测试 费率时段检查 时段投切 电能示值组合误差 时钟示值误差 工频耐压试验 显示功能
        //剩余电量递减准确度 参数设置 控制功能

        //外观检测（10）
        public const string 外观检查 = "10001";

        //准确度试验（12）
        public const string 基本误差试验 = "12001";
        public const string 起动试验 = "12002";
        public const string 潜动试验 = "12003";
        public const string 电能表常数试验 = "12004";

        public const string 第N次谐波试验 = "12005001";
        public const string 方顶波波形试验 = "12005002";
        public const string 尖顶波波形改变 = "12005003";
        public const string 脉冲群触发波形试验 = "12005004"; //间谐波波形改变
        public const string 九十度相位触发波形试验 = "12005005"; //奇次谐波波形试验
        public const string 半波整流波形试验 = "12005006";  //偶次谐波试验
        public const string 频率改变 = "12005007";
        public const string 电压改变 = "12005008";
        public const string 负载不平衡试验 = "12005009";
        public const string 辅助装置试验 = "12005010";
        public const string 逆相序试验 = "12005011";
        public const string 一相或两相电压中断试验 = "12005012";
        public const string 高次谐波 = "12005013";
        public const string 冲击试验 = "12005014";
        public const string 振动试验 = "12005015";
        public const string 外部恒定磁场试验 = "12005016";

        public const string 标准偏差试验 = "12006";
        public const string 初始固有误差 = "12007";
        public const string 谐波电能准确度试验 = "12008";
        public const string 谐波测量准确度试验 = "12009";
        public const string 起动试验_黑龙江 = "12010";
        public const string 潜动试验_黑龙江 = "12011";
        public const string 基本误差试验_黑龙江 = "12012";
        public const string 电能表常数试验_黑龙江 = "12013";
        public const string 端子座温度监测准确度 = "12014";
        public const string 初始固有误差试验 = "12015";

        //预先调试（14）
        public const string 蓝牙预处理 = "14000";
        public const string 接线检查 = "14001";
        public const string 密钥更新_预先调试 = "14002";
        public const string 密钥恢复_预先调试 = "14003";
        public const string 通信测试_预先调试 = "14004";
        public const string 钱包初始化_预先调试 = "14005";
        public const string 接线检查_黑龙江 = "14006";
        public const string GPS预校时 = "14007";
        public const string 写地址_预先调试 = "14008";
        public const string 表位电流开路检查 = "14009";

        //多功能试验（15）
        public const string GPS对时 = "15001";
        public const string 日计时误差 = "15002";
        public const string 需量示值误差 = "15003";
        public const string 电量清零 = "15006";
        public const string 需量清空 = "15007";
        public const string 通讯测试 = "15008";
        public const string 负载电流快速改变 = "15009";
        public const string 读取电量 = "15010";
        public const string 费率时段检查 = "15011";
        public const string 时段投切 = "15012";
        public const string 电能示值组合误差 = "15013";
        public const string 时钟示值误差 = "15014";
        public const string 测量及监测误差 = "15015";
        public const string 停电转存试验 = "15016";
        public const string 费率时段示值误差 = "15019";//功能还未添加
        public const string 闰年判断功能 = "15020";
        public const string 采用备用电源工作的时钟试验 = "15021";
        public const string 零线电流检测 = "15022";
        public const string 日计时误差_黑龙江 = "15023";
        public const string GPS对时_黑龙江 = "15025";
        public const string 电能示值组合误差_黑龙江 = "15027";
        public const string 时钟示值误差_黑龙江 = "15028";
        public const string 电能示值组合误差_西安 = "15029";
        public const string 交流电压暂降和短时中断 = "15030";
        public const string 接地故障 = "15031"; //功能还未测试
        public const string 预置内容设置 = "15032";
        public const string 预置内容检查 = "15033";
        public const string 电流回路阻抗 = "15034";
        public const string 电量法需量功能试验 = "15041";

        //通讯协议检查试验（17）
        public const string 通讯协议检查试验 = "17001";
        public const string 参数验证 = "17002";
        public const string 通讯协议检查试验2 = "17003";
        public const string 通讯协议检查试验_黑龙江 = "17004";
        public const string 通讯协议检查试验2_黑龙江 = "17005";//功能还未添加
        public const string 状态核查 = "17006";//功能还未添加

        //一致性试验（18）
        public const string 误差一致性 = "18001";
        public const string 误差变差 = "18002";
        public const string 负载电流升将变差 = "18003";
        public const string 电流过载 = "18004";
        public const string 重复性 = "18005";
        //载波功能试验（19）
        public const string 载波通信测试 = "19002";
        public const string 载波芯片ID测试 = "19004";


        //电气性能试验（20）
        public const string 工频耐压试验 = "20001";
        public const string 功耗试验 = "20002";

        //智能表功能试验（21）
        public const string 计量功能 = "21001";
        public const string 计时功能 = "21002";
        public const string 费率时段功能 = "21003";
        public const string 脉冲输出功能 = "21004";
        public const string 最大需量功能 = "21005";
        public const string 停电抄表功能 = "21006";
        public const string 显示功能 = "21007";
        public const string 时区时段功能 = "21008";

        //事件记录试验（22）
        public const string 失压记录 = "22001";
        public const string 过压记录 = "22002";
        public const string 欠压记录 = "22003";
        public const string 失流记录 = "22004";
        public const string 断流记录 = "22005";
        public const string 过流记录 = "22006";
        public const string 过载记录 = "22007";
        public const string 断相记录 = "22008";
        public const string 掉电记录 = "22009";
        public const string 全失压记录 = "22010";
        public const string 电压不平衡记录 = "22011";
        public const string 电流不平衡记录 = "22012";
        public const string 电压逆向序记录 = "22013";
        public const string 电流逆向序记录 = "22014";
        public const string 校时记录 = "22015";
        public const string 开表盖记录 = "22016";
        public const string 开端钮盖记录 = "22017";
        public const string 编程记录 = "22018";
        public const string 需量清零记录 = "22019";
        public const string 事件清零记录 = "22020";
        public const string 电表清零记录 = "22021";
        public const string 潮流反向记录 = "22022";
        public const string 功率反向记录 = "22023";
        public const string 需量超限记录 = "22024";
        public const string 功率因数超下限记录 = "22025";
        public const string 恒定磁场干扰记录 = "22026";
        public const string 跳闸合闸事件记录 = "22027";
        //add
        public const string 时钟故障事件记录 = "22028";
        public const string 事件跟随上报 = "22029";
        public const string 事件主动上报_载波 = "22030";
        public const string 广播校时事件记录 = "22031";
        public const string 零线电流异常事件记录 = "22032";

        //冻结功能（23）
        public const string 定时冻结 = "23001";
        public const string 约定冻结 = "23002";
        public const string 瞬时冻结 = "23003";
        public const string 日冻结 = "23004";
        public const string 整点冻结 = "23005";
        public const string 分钟冻结 = "23006";
        public const string 小时冻结 = "23007";
        public const string 月冻结 = "23008";
        public const string 年冻结 = "23009";
        public const string 结算日冻结 = "23010";

        //红外通信试验（24）
        public const string 红外通信试验 = "24001";

        //费控功能试验（25）
        public const string 身份认证 = "25001";
        public const string 远程控制 = "25002";
        public const string 报警功能 = "25003";
        public const string 远程保电 = "25004";
        public const string 保电解除 = "25005";
        public const string 数据回抄 = "25006";
        public const string 钱包初始化 = "25007";
        public const string 密钥更新 = "25008";
        public const string 密钥恢复 = "25009";
        public const string 费控_电量清零 = "25010";
        public const string 剩余电量递减准确度 = "25011";
        public const string 负荷开关 = "25012";
        public const string 参数设置 = "25013";
        public const string 控制功能 = "25014";
        public const string 身份认证_计量芯 = "25015";
        public const string 密钥更新_黑龙江 = "25016";
        public const string 远程控制_西安 = "25017";
        public const string 电量清零_黑龙江 = "25018";
        public const string 保电控制 = "25019";
        public const string 拉合闸控制 = "25020";
        public const string 报警控制 = "25021";
        public const string 最后核查 = "25022";


        //自热试验（26）
        public const string 自热试验 = "26001";

        //计量性能保护试验（27）
        public const string 数据同步 = "27003";

        //负荷记录（28）
        public const string 负荷记录 = "28001";

        public const string 三相电流不平衡 = "30001";
        public const string 电能质量模组闪变试验检定 = "30002";
        public const string 谐波有功功率 = "30003";
        public const string 电能质量模组电压偏差试验检定 = "30004";
        //public const string 三相电流不平衡 = "30005";
        public const string 电压暂升电压暂降和短时中断 = "30006";
        public const string 电能质量模组电压波动试验检定 = "30007";
        public const string 谐波电流 = "30008";
        public const string 谐波电压 = "30009";
        public const string 间谐波电流 = "30010";
        public const string 间谐波电压 = "30011";
        public const string 三相电压不平衡 = "30012";
        public const string 电能质量模组频率偏差试验 = "30013";



    }
}
