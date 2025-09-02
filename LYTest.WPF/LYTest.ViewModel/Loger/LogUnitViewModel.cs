using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Log
{
    /// 日志数据模型
    /// <summary>
    /// 日志数据模型
    /// </summary>
    public class LogUnitViewModel : ViewModelBase
    {
        public LogUnitViewModel(LogModel model)
        {
            Message = model.Message;
            LogSource = model.LogSource;
            Level = model.Level;
            E = model.E;
            Time = model.Time;
        }
        private string message;
        /// 日志信息内容
        /// <summary>
        /// 日志信息内容
        /// </summary>
        public string Message
        {
            get { return message; }
            set { SetPropertyValue(value, ref message, "Message"); }
        }
        private EnumLogSource? logSource;
        /// 日志由哪个模块发出来的
        /// <summary>
        /// 日志由哪个模块发出来的
        /// </summary>
        public EnumLogSource? LogSource
        {
            get { return logSource; }
            set { SetPropertyValue(value, ref logSource, "LogSource"); }
        }
        private EnumLevel? level;
        /// 日志等级,分为信息,告警和错误
        /// <summary>
        /// 日志等级,分为信息,告警和错误
        /// </summary>
        public EnumLevel? Level
        {
            get { return level; }
            set { SetPropertyValue(value, ref level, "Level"); }
        }
        private Exception ex;
        /// 如果是异常消息,会有完整的异常消息内容
        /// <summary>
        /// 如果是异常消息,会有完整的异常消息内容
        /// </summary>
        public Exception E
        {
            get { return ex; }
            set { SetPropertyValue(value, ref ex, "E"); }
        }
        private string time;
        /// 消息发生的时间
        /// <summary>
        /// 消息发生的时间
        /// </summary>
        public string Time
        {
            get { return time; }
            set { SetPropertyValue(value, ref time, "Time"); }
        }
    }
}
