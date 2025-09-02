using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace E_CLSocketModule.Enum
{
    /// <summary>
    /// 电表电压回路选择
    /// </summary>
    public enum Cus_BothVRoadType
    {
        /// <summary>
        /// 直接接入式
        /// </summary>
        直接接入式 = 0,
        /// <summary>
        /// 互感器接入式
        /// </summary>
        互感器接入式 = 1,
        /// <summary>
        /// 本表位无电表接入
        /// </summary>
        本表位无电表接入 = 2
    }
}
