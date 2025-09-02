using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /// <summary>
    /// 服务类别
    /// </summary>
    public enum EmServieType
    {
        /// <summary>
        /// 建立应用连接请求
        /// </summary>
        CONNECT_Request = 2,
        /// <summary>
        /// 断开应用连接请求
        /// </summary>
        RELEASE_Request = 3,
        /// <summary>
        /// 读取请求
        /// </summary>
        GET_Request = 5,
        /// <summary>
        /// 设置请求
        /// </summary>
        SET_Request = 6,
        /// <summary>
        /// 操作请求
        /// </summary>
        ACTION_Request = 7,
        /// <summary>
        /// 上报应答
        /// </summary>
        REPORT_Response = 8,
        /// <summary>
        /// 代理请求
        /// </summary>
        PROXY_Request = 9,


        /// <summary>
        /// 建立应用连接响应
        /// </summary>
        CONNECT_Response = 130,
        /// <summary>
        /// 断开应用连接响应
        /// </summary>
        RELEASE_Response = 131,
        /// <summary>
        /// 断开应用连接通知
        /// </summary>
        RELEASE_Notification = 132,
        /// <summary>
        /// 读取响应
        /// </summary>
        GET_Response = 133,

        /// <summary>
        /// 设置响应
        /// </summary>
        SET_Response = 134,
        /// <summary>
        /// 操作响应
        /// </summary>
        ACTION_Response = 135,
        /// <summary>
        /// 上报通知
        /// </summary>
        REPORT_Notification = 136,
        /// <summary>
        /// 代理响应
        /// </summary>
        PROXY_Response = 137,
        /// <summary>
        /// 安全请求
        /// </summary>
        SECURITY_Request = 16,
        /// <summary>
        /// 安全响应
        /// </summary>
        SECURITY_Response = 144





    }
}
