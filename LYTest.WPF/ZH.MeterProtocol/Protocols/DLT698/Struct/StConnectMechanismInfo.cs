
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.MeterProtocol.Protocols.DLT698.Struct
{
    /// <summary>
    /// 应用连接请求的认证机制信息
    /// </summary>
    public struct StConnectMechanismInfo
    {
        /// <summary>
        /// 认证机制
        /// </summary>
        public EmConnectMechanismInfo ConnectInfo { get; set; }

        /// <summary>
        /// 主站会话协商数据
        /// </summary>
        public StSignatureSecurity SessionData { get; set; }
    }
}
