using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 密文+MAC设置 对象属性属性
    /// </summary>
    public class SetByCipherMacStructure
    {
        /// <summary>
        /// 请求设置
        /// </summary>
        public struct SetApduCipherMac
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
