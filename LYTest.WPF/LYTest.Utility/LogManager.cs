using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LYTest.Utility.Log
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public class LogManager
    {
        static LogManager()
        {
            //日志处理线程
            new Task(new Action(LogProcess)).Start();
        }
        /// <summary>
        /// 进程里面的等待句柄
        /// 如果显示完毕以后设置为false
        /// </summary>
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        /// <summary>
        /// 日志队列
        /// </summary>
        private static readonly ConcurrentQueue<LogModel> queueMessage = new ConcurrentQueue<LogModel>();
        /// <summary>
        /// 添加到日志队列
        /// </summary>
        /// <param name="message">概要信息</param>
        /// <param name="source">数据源</param>
        /// <param name="level">日志等级</param>
        /// <param name="e">异常内容</param>
        public static void AddMessage(string message, EnumLogSource source = EnumLogSource.用户操作日志, EnumLevel level = EnumLevel.Information, Exception e = null)
        {
            queueMessage.Enqueue(new LogModel(message, source, level, e));

            waitHandle.Set();

        }
    
        /// <summary>
        /// 重新日志，服务器日志内容单独打印
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"> 6 对应业务类接口日志 12 控制类 13 工况类</param>
        public static void AddMessage(string strMessage,int type)
        {
            string LogPath = string.Format(@"Log\服务器日志\业务类\{0}.Log", DateTime.Now.ToString("yyyy-MM-dd"));
            //这里要判断任务的状态
            waitHandle.Set();
            switch (type)
            {
                case 6:
                    LogPath = string.Format(@"Log\服务器日志\业务类\{0}.Log", DateTime.Now.ToString("yyyy-MM-dd"));
                    break;
                case 12:
                    LogPath = string.Format(@"Log\服务器日志\控制类\{0}.Log", DateTime.Now.ToString("yyyy-MM-dd"));
                    break;
                case 13:
                    LogPath = string.Format(@"Log\服务器日志\工况类\{0}.Log", DateTime.Now.ToString("yyyy-MM-dd"));
                    break;
                default:
                    break;
            }
           
            LogPath = Directory.GetCurrentDirectory() + "\\" + LogPath;
            FileStream fs = Create(LogPath);
            if (fs == null)
            {
                return;
            }
            fs.Close();
            fs.Dispose();
            strMessage = DateTime.Now + "\r\n" + strMessage;
            System.IO.File.AppendAllText(LogPath, strMessage + "\r\n\r\n");
        }
        /// <summary>
        /// 日志线程要执行的动作
        /// </summary>
        private static void LogProcess()
        {
            while (true)
            {
                while (queueMessage.Count > 0)
                {
                    queueMessage.TryDequeue(out LogModel logClass);
                    //执行事件处理函数
                    try
                    {
                        ExecuteException(logClass);
                    }
                    catch { }
                }
                waitHandle.Reset();
                waitHandle.WaitOne();
            }
        }
        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="logClass"></param>
        private static void ExecuteException(LogModel logClass)
        {
            // 触发事件
            LogMessageArrived?.Invoke(logClass, null);

            if (logClass != null && logClass.Level == EnumLevel.Error)
            {
                WriteErrLog(logClass.Message);
            }
        }
        public static event EventHandler LogMessageArrived;

        /// <summary>
        /// 记录运行错误
        /// </summary>
        /// <param name="strMessage"></param>
        private static void WriteErrLog(string strMessage)
        {

            string LogPath = string.Format(@"Log\异常日志\{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));
            LogPath = Directory.GetCurrentDirectory() + "\\" + LogPath;
            FileStream fs = Create(LogPath);
            if (fs == null)
            {
                return;
            }
            fs.Close();
            fs.Dispose();
            strMessage = DateTime.Now + "\r\n" + strMessage;
            File.AppendAllText(LogPath, strMessage + "\r\n\r\n");

        }


        /// <summary>
        /// 创建文件、如果目录不存在则自动创建、路径既可以是绝对路径也可以是相对路径
        /// 返回文件数据流，如果创建失败在返回null、如果文件存在则打开它
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static FileStream Create(string FileName)
        {

            string folder = FileName.Substring(0, FileName.LastIndexOf('\\') + 1);

            string tmpFolder = folder.Substring(0, FileName.IndexOf('\\')); //磁盘跟目录
            //逐层创建文件夹
            try
            {
                while (tmpFolder != folder)
                {
                    tmpFolder = folder.Substring(0, FileName.IndexOf('\\', tmpFolder.Length) + 1);
                    if (!System.IO.Directory.Exists(tmpFolder))
                        System.IO.Directory.CreateDirectory(tmpFolder);
                }
            }
            catch { return null; }

            if (System.IO.File.Exists(FileName))
            {
                return System.IO.File.Open(FileName, FileMode.Open, FileAccess.ReadWrite);
                //return null;
            }
            else
            {
                try
                {
                    return System.IO.File.Create(FileName);
                }
                catch { return null; }
            }
        }
    }
}
