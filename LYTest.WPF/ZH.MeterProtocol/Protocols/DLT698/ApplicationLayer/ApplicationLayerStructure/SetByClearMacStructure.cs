

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 明文+MAC设置属性结构
    /// </summary>
    public class SetByClearMacStructure
    {

        /// <summary>
        /// 请求设置
        /// </summary>
        public struct SetApduClearMac
        {

            /// <summary>
            /// 安全传输请求
            /// </summary>
            public string SecurtiyRequest { get; set; }

            /// <summary>
            /// 明文模式
            /// </summary>
            public string ClearMode { get; set; }

            /// <summary>
            /// 长度域标识
            /// </summary>
            public string LenFlag { get; set; }

            /// <summary>
            /// 明文长度
            /// </summary>
            public string ClearLen { get; set; }

            /// <summary>
            /// 明文
            /// </summary>
            public string ClearText { get; set; }

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
