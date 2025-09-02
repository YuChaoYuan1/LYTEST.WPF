

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 密文+MAC
    /// </summary>
    public class OperationByCipherMacStructure
    {
        /// <summary>
        /// 请求
        /// </summary>
        public struct OperationApduCipherMac
        {
            /// <summary>
            /// 安全传输请求
            /// </summary>
            public string SecurtiyRequest { get; set; }

            /// <summary>
            /// 密文模式
            /// </summary>
            public string CipherMode { get; set; }

            /// <summary>
            /// 密文长度
            /// </summary>
            public string CipherLen { get; set; }

            /// <summary>
            /// 密文
            /// </summary>
            public string CipherText { get; set; }

            /// <summary>
            /// 数据验证模式
            /// </summary>
            public string DataValidateMode { get; set; }

            /// <summary>
            /// 数据验证信息
            /// </summary>
            public string ValidationInfo { get; set; }
        }
    }
}
