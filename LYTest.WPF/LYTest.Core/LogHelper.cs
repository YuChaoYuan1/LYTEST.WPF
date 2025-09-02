using System;
using System.IO;

namespace LYTest.Core
{
    /// <summary>
    /// 日志类
    /// </summary>
    public class LogHelper
    {
        private static readonly object lockObj = new object();
        private static readonly string foldPath;
        /// <summary>
        /// 是否启用文件日志记录
        /// </summary>
        public static bool Enable { get; set; }

        /// <summary>
        /// 初始化文件日志
        /// </summary>
        /// <param name="enable">是否开启日志记录:true-开启,否则为false</param>
        static LogHelper()
        {
            foldPath = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "Log");
            if (!Directory.Exists(foldPath))
                Directory.CreateDirectory(foldPath);

            //if (!Directory.Exists(string.Format(@"{0}\{1}", filePath, "Exception")))
            //    Directory.CreateDirectory(string.Format(@"{0}\{1}", filePath, "Exception"));

            foldPath = $@"{foldPath}\RUN";
            if (!Directory.Exists(foldPath))
                Directory.CreateDirectory(foldPath);

            //if (!Directory.Exists(string.Format(@"{0}\{1}", filePath, "Frame")))
            //    Directory.CreateDirectory(string.Format(@"{0}\{1}", filePath, "Frame"));
            Enable = false;
        }

        ///// <summary>
        ///// 写错误日志文件(每天一个文件)
        ///// </summary>
        ///// <param name="msg"></param>
        //public static void WriteLog(Exception ex)
        //{
        //    if (!Enable) return;

        //    string msg = string.Format("{0}\n{1}\n{2}", ex.Message, ex.StackTrace, ex.InnerException);
        //    WriteLog(msg);
        //}



        /// <summary>
        /// 写运行日志文件(每天一个文件)
        /// </summary>
        public static void WriteLog(string msg)
        {
            if (!Enable) return;

            lock (lockObj)
            {
                string file = $@"{foldPath}\{DateTime.Now:yyyy-MM-dd}.txt";
                string text = $"{DateTime.Now:hh:mm:ss}:{msg}\n";

                File.AppendAllText(file, text);
            }
        }
        ///// <summary>
        ///// 写命令帧日志文件(每天一个文件)
        ///// </summary>
        ///// <param name="ex"></param>
        //public static void FrameWrite(string fileName, string msg)
        //{
        //    if (!Enable) return;

        //    lock (lockObj)
        //    {
        //        string path = string.Format(@"{0}\{1}\{2}", foldPath, "Frame", DateTime.Now.ToString("yyyy-MM-dd"));
        //        if (!Directory.Exists(path))
        //            Directory.CreateDirectory(path);
        //        string LogPath = string.Format(@"{0}\{1}.txt", path, fileName);
        //        System.IO.File.AppendAllText(LogPath, msg);

        //    }

        //}
    }

}
