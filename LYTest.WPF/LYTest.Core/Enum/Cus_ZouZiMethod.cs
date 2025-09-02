
namespace LYTest.Core.Enum
{
    /// <summary>
    /// 走字试验方法类型枚举
    /// </summary>
    public enum Cus_ZouZiMethod
    {
        ///// <summary>
        ///// Undefined
        ///// </summary>
        //Undefined = 0,

        /// <summary>
        /// 标准表法
        /// </summary>
        标准表法 = 1,

        /// <summary>
        /// 走字试验法，两只参照表
        /// </summary>
        //走字试验法 = 2,
        基本走字法 = 2,//兼容旧

        /// <summary>
        /// 计读脉冲法
        /// </summary>
        //计读脉冲法 = 3,
        校核常数 = 3,//兼容旧

    }
}
