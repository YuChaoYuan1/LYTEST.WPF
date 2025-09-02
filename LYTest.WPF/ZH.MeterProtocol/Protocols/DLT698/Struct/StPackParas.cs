using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.MeterProtocol.Protocols.DLT698.Struct
{
    /// <summary>
    /// 698协议组侦参数
    /// </summary>
    public struct StPackParas
    {
        /// <summary>
        /// 表通信地址
        /// </summary>
        public string MeterAddr { get; set; }

        /// <summary>
        /// 对象属性标识符或对象方法标识符
        /// </summary>
        public List<string> OD { get; set; }

        /// <summary>
        /// 数据+验证码
        /// </summary>
        public StSIDMAC SidMac { get; set; }

        /// <summary>
        /// 传输安全模式
        /// </summary>
        public EmSecurityMode SecurityMode { get; set; }

        /// <summary>
        /// 读取属性模式
        /// </summary>
        public EmGetRequestMode GetRequestMode { get; set; }

    }
}
