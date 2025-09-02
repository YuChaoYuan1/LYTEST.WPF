using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 明文操作APDU结构
    /// </summary>
    public class OperationByClearStructure
    {

        /// <summary>
        /// 操作请求结构
        /// </summary>
        public struct OperationApduClearText
        {
            /// <summary>
            /// 操作请求
            /// </summary>
            public string ActionRequest { get; set; }

            /// <summary>
            /// 操作模式
            /// </summary>
            public string ActionRequestMode { get; set; }

            /// <summary>
            /// 序号及优先级
            /// </summary>
            public string PIID { get; set; }

            /// <summary>
            ///方法描述符个数
            /// </summary>
            public string Num { get; set; }

            /// <summary>
            /// OMD+参数信息
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
            /// 对象方法描述符
            /// </summary>
            public string OMD { get; set; }

            /// <summary>
            /// 参数类型+参数信息
            /// </summary>
            public string Parameters { get; set; }
        }
    }
}
