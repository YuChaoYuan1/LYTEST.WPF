
namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /// <summary>
    /// 参数安全模式
    /// </summary>
    public enum EmSecurityMode
    {
        /// <summary>
        /// 明文
        /// </summary>
        ClearText=0,
        /// <summary>
        /// 明文＋随机数
        /// </summary>
        ClearTextRand=1,
        /// <summary>
        ///  明文+数据验证码
        /// </summary>
        ClearTextMac=2,
        /// <summary>
        ///密文
        /// </summary>
        Ciphertext=3,
        /// <summary>
        /// 密文+数据验证码
        /// </summary>
        CiphertextMac=4
    }
}
