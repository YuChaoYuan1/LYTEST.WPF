using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /*
     * 操作一个对象方法请求                	         [1] ActionRequest，
       操作若干个对象方法请求                  	     [2] ActionRequestList，
       操作若干个对象方法后读取若干个对象属性请求    [3] ActionThenGetRequestNormalList

    */
    /// <summary>
    /// 操作请求模式
    /// </summary>
    public enum EmOperationMode
    {
        /// <summary>
        /// 操作一个对象方法请求
        /// </summary>
        ActionRequest = 1,
        /// <summary>
        /// 操作若干个对象方法请求
        /// </summary>
        ActionRequestList = 2,
        /// <summary>
        /// 操作若干个对象方法后读取若干个对象属性请求
        /// </summary>
        ActionThenGetRequestNormalList = 3



    }
}
