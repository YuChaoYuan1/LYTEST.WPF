

namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /// <summary>
    /// 应用连接请求的认证机制信息
    /// </summary>
    public enum EmConnectMechanismInfo
    {
        /// <summary>
        ///公共连接 
        /// </summary>
        NullSecurity = 0,
        /// <summary>
        /// 一般密码
        /// </summary>
        PasswordSecurity = 1,
        /// <summary>
        /// 对称加密
        /// </summary>
        SymmetrySecurity = 2,
        /// <summary>
        /// 数字签名
        /// </summary>
        SignatureSecurity = 3

    }
}
