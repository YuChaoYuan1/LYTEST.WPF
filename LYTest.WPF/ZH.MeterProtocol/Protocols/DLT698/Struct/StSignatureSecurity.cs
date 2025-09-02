

namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /// <summary>
    /// 数字签名
    /// </summary>
    public struct StSignatureSecurity
    {
        /// <summary>
        /// 会话协商数据
        /// </summary>
        public string SessionData { get; set; }

        /// <summary>
        /// MAC
        /// </summary>
        public string MAC { get; set; }
    }
}
