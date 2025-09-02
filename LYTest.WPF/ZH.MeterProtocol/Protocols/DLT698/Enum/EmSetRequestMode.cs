using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /*
     *  设置一个对象属性请求        	[1] SetRequestNormal，
        设置若干个对象属性请求       	[2] SetRequestNormalList，
        设置后读取若干个对象属性请求	[3] SetThenGetRequestNormalList

     */
    /// <summary>
    /// 设置请求模式
    /// </summary>
    public enum EmSetRequestMode
    {
        /// <summary>
        /// 设置一个对象属性请求
        /// </summary>
        SetRequestNormal = 1,
        /// <summary>
        /// 设置若干个对象属性请求
        /// </summary>
        SetRequestNormalList = 2,
        /// <summary>
        /// 设置后读取若干个对象属性请求
        /// </summary>
        SetThenGetRequestNormalList = 3


    }
}
