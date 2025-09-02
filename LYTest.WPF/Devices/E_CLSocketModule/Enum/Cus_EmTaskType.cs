using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace E_CLSocketModule.Enum
{
    /// <summary>
    /// 试验类型
    /// </summary>
    public enum Cus_EmTaskType
    {
        AutoThreadMethod = -1,
        电能误差 = 0,
        需量周期 = 1,
        时钟日误差 = 2,
        脉冲计数 = 3,
        对标 = 4,
        走字 = 5,
        设置预付费试验 = 6,
        多功能脉冲计数 = 7,
        误差板功耗数据 = 8,
        读取误差板温度 = 9,
        只返回成功失败的命令14 = 10,
        只返回成功失败的命令16 = 11,
        只返回成功失败的命令18 = 12,
        耐压 = 13,
        压接电机延时时间 = 14

    }
}
