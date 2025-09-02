

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 明文+MAC 操作对象结构
    /// </summary>
    public class OperationByClearMacStructure
    {

        public struct OperationApduClearMac
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
