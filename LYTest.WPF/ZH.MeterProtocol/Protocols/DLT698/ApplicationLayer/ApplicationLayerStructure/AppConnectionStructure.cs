using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 应用连接APDU结构
    /// </summary>
    public class AppConnectionStructure
    {
        /*02 —— [2] CONNECT-Request
        00 —— PIID
        00 10 —— 期望的应用层协议版本号
        FF FF FF FF FF FF FF FF —— ProtocolConformance
        FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF —— FunctionConformance
        04 00 —— 客户机发送帧最大尺寸
        04 00 —— 客户机接收帧最大尺寸
        01    —— 客户机接收帧最大窗口尺寸
        04 00 —— 客户机最大可处理APDU尺寸
        00 00 00 64 —— 期望的应用连接超时时间
        00 —— 认证请求对象 [0] NullSecurity，
        00 —— 没有时间标签
        */
        public struct ConnectionRequestText
        {
            /// <summary>
            /// 应用连接请求服务
            /// </summary>
            public string ConnectRequest { get; set; }

            /// <summary>
            /// 序号及优先级
            /// </summary>
            public string PIID { get; set; }

            /// <summary>
            /// 期望的应用层协议版本号
            /// </summary>
            public string ProtocolVersion { get; set; }

            /// <summary>
            /// 协议一致性
            /// </summary>
            public string ProtocolConformance { get; set; }

            /// <summary>
            /// 功能一致性
            /// </summary>
            public string FunctionConformance { get; set; }

            /// <summary>
            /// 客户机发送帧最大尺寸
            /// </summary>
            public string ClientSendMaxFrameSize { get; set; }

            /// <summary>
            /// 客户机接收帧最大尺寸
            /// </summary>
            public string ClientReceiveMaxFrameSize { get; set; }

            /// <summary>
            /// 客户机接收帧最大窗口尺寸
            /// </summary>
            public string ClientReceiveMaxFrameFormSize { get; set; }

            /// <summary>
            /// 客户机最大可处理APDU尺寸
            /// </summary>
            public string ClientHandleMaxApduSize { get; set; }

            /// <summary>
            /// 期望的应用连接超时时间
            /// </summary>
            public string OutTime { get; set; }

            /// <summary>
            /// 认证请求对象
            /// </summary>
            public string ConnectMechanismInfos { get; set; }


            /// <summary>
            /// 时间标签
            /// </summary>
            public string TimeFlag { get; set; }

        }
    }
}
