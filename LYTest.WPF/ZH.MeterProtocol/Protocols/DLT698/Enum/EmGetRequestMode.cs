
namespace LYTest.MeterProtocol.Protocols.DLT698.Enum
{
    /*
     * 读取一个对象属性请求               [1] GetRequestNormal，
        读取若干个对象属性请求            [2] GetRequestNormalList，
        读取一个记录型对象属性请求        [3] GetRequestRecord，
        读取若干个记录型对象属性请求	  [4] GetRequestRecordList，
        读取分帧响应的下一个数据块请求	  [5] GetRequestNext
     */
    /// <summary>
    /// 读取属性模式
    /// </summary>
    public enum EmGetRequestMode
    {
        /// <summary>
        /// 读取一个对象属性请求
        /// </summary>
        GetRequestNormal = 1,
        /// <summary>
        /// 读取若干个对象属性请求
        /// </summary>
        GetRequestNormalList = 2,
        /// <summary>
        /// 读取一个记录型对象属性请求
        /// </summary>
        GetRequestRecord = 3,
        /// <summary>
        /// 读取若干个记录型对象属性请求
        /// </summary>
        GetRequestRecordList = 4,
        /// <summary>
        /// 读取分帧响应的下一个数据块请求
        /// </summary>
        GetRequestNext = 5
    }
}
