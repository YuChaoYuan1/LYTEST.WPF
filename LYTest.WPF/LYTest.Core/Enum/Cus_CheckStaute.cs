using System;

namespace LYTest.Core.Enum
{
    /// <summary>
    /// 检定状态,0,2,32,16独立状态，（1、8）与（4）要组合
    /// </summary>
    [Flags]
    public enum Cus_CheckStaute
    {
        未赋值的 = 0,
        检定 = 1,
        停止检定 = 2,
        调表 = 4,
        单步检定 = 8,
        录入完成 = 16,
        错误 = 32,
        网控检定 = 33,
    }
}
