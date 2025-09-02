namespace LYTest.Core.Enum
{
    /// <summary>
    /// 费控功能试验检定项目枚举
    /// </summary>
    public class Cus_CostControlItem
    {
        public const string 无效的项目 = "000";
        public const string 身份认证 = "25001";
        public const string 远程控制 = "25002";
        public const string 报警功能 = "25003";
        public const string 保电功能 = "25004";
        public const string 保电解除 = "25005";
        //public const string 远程保电 = "006";
        public const string 数据回抄 = "25006";
        public const string 钱包初始化 = "25007";
        public const string 密钥更新 = "25008";
        public const string 密钥恢复 = "25009";

        //add yjt 20220217 新增三个实验ID
        public const string 剩余电量递减准确度 = "25011";
        public const string 参数设置 = "25013";
        public const string 控制功能 = "25014";

        //public const string 电价切换 = "010";
        //public const string 负荷开关 = "011";
        //public const string 电量清零 = "012";


        //public const string 阶梯电价检测 = "016";
        //public const string 费率电价检测 = "017";
        //public const string 远程控制直接合闸 = "018";

        //public const string 预置内容检查 = "021";
        //public const string 预置内容设置 = "022";
        //public const string 剩余金额递减准确度_枯水期 = "023";
        //public const string 剩余金额递减准确度_丰水期 = "024";
        //public const string 剩余金额递减准确度_平水期 = "025";
        //public const string 费率电价切换 = "026";
    }
}
