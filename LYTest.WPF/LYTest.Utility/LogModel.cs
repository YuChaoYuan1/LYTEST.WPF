using System;
namespace LYTest.Utility.Log
{
    /// <summary>
    /// 日志数据模型
    /// </summary>
    public class LogModel
    {
        public LogModel(string message, EnumLogSource logSource = EnumLogSource.用户操作日志, EnumLevel level = EnumLevel.Information, Exception e = null)
        {
            Message = message;
            LogSource = logSource;
            Level = level;
            E = e;
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// 日志信息内容
        /// <summary>
        /// 日志信息内容
        /// </summary>
        public string Message { get;private set; }
        /// 日志由哪个模块发出来的
        /// <summary>
        /// 日志由哪个模块发出来的
        /// </summary>
        public EnumLogSource LogSource { get; private set; }
        /// 日志等级,分为信息,告警和错误
        /// <summary>
        /// 日志等级,分为信息,告警和错误
        /// </summary>
        public EnumLevel Level { get; set; }
        /// 如果是异常消息,会有完整的异常消息内容
        /// <summary>
        /// 如果是异常消息,会有完整的异常消息内容
        /// </summary>
        public Exception E { get; private set; }
        /// 消息发生的时间
        /// <summary>
        /// 消息发生的时间
        /// </summary>
        public string Time { get; set; }
    }
}
