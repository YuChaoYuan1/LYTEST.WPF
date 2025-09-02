using System;
using System.Collections.Generic;
using System.IO;

namespace LYTest.Device.SocketModule
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

        static LogHelper()
        {
            foldPath = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "Log");
            if (!Directory.Exists(foldPath))
                Directory.CreateDirectory(foldPath);


            foldPath = $@"{foldPath}\Frame";
            if (!Directory.Exists(foldPath))
                Directory.CreateDirectory(foldPath);

            Enable = true;
        }

        public static void Write(bool dir, string portName, List<byte> frame, string msg = "")
        {
            Write(dir, portName, frame.ToArray(), msg);

        }


        /// <summary>
        /// 记录日志(每天一个文件)
        /// </summary>
        /// <param name="dir">true-发送数据，false-接收数据</param>
        /// <param name="portName">端口名称</param>
        /// <param name="frame">报文</param>
        /// <param name="msg">其他信息，如报文解析后内容</param>
        public static void Write(bool dir, string portName, byte[] frame, string msg = "")
        {
            if (!Enable) return;

            lock (lockObj)
            {
                string file = $@"{foldPath}\{DateTime.Now:yyyy-MM-dd}.txt";
                string str = BitConverter.ToString(frame).Replace("-", " ");
                string text = $"{DateTime.Now:hh:mm:ss ffff} [{portName}] {(dir ? ">>>" : "<<<")} {str}";
                if (!string.IsNullOrEmpty(msg))
                {
                    text += $" ({msg})";
                }
                text += "\n\r";

                File.AppendAllText(file, text);
            }
        }

    }

}
