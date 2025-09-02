using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 明文设置组帧结构
    /// </summary>
    public class SetByClearStructure
    {
        /// <summary>
        /// 请求
        /// </summary>
        public struct SetApduFrameClearText
        {
            /// <summary>
            /// 设置属性请求
            /// </summary>
            public string SetRequest { get; set; }

            /// <summary>
            /// 设置模式
            /// </summary>
            public string SetRequestMode { get; set; }

            /// <summary>
            /// 优先级及序号
            /// </summary>
            public string PIID { get; set; }

            /// <summary>
            /// OAD个数
            /// </summary>
            public string OadNum { get; set; }

            /// <summary>
            /// 数据信息
            /// </summary>
            public List<DataInfos> DataInfo { get; set; }

            /// <summary>
            /// 时间标签
            /// </summary>
            public string TimeFlag { get; set; }

        }

        public struct DataInfos
        {
            /// <summary>
            /// O对象属性标示符
            /// </summary>
            public string OAD { get; set; }

            /// <summary>
            ///数据类型+数据长度+数据内容
            /// </summary>
            public string Data { get; set; }


        }
    }
}
