using System;
using System.Collections.Generic;
using System.Text;

namespace CLOU.LogModel
{
    public class DataLoger
    {
        private const string Name = "CL3115";
        private const string LogPath = @".\LogFile\";
        /// <summary>
        /// 日志路径
        /// </summary>
        private static string LogFile = string.Format(@"{0}{1} {2}.log", LogPath,DateTime.Now.ToString("yyyy-MM-dd"),Name);
        /// <summary>
        /// 是否开始记录，true-开始，false-停止
        /// </summary>
        private static bool Start = false;
        private const string ValueName="ActualValues";
        private static string ValueFile = string.Format(@"{0}{1} {2}.log", LogPath, DateTime.Now.ToString("yyyy-MM-dd"), ValueName);
        private static bool ValueStart = false;
        /// <summary>
        /// 写TXT文件(传入文件名,或（绝对、相对）路径+文件名)
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool Log(string file,string msg)
        {
            try
            {
                file = GetPhyPath(file);
                System.IO.FileStream Fs = Create(file);
                if (Fs == null)
                {
                    //MessageBox.Show(string.Format("日志文件{0}创建失败!", LogPath));
                    return false;
                }
                Fs.Close();
                Fs.Dispose();

                string ErrTxt = string.Format(@"{0}{1}", msg, Environment.NewLine);

                System.IO.File.AppendAllText(file, ErrTxt);
                return true;
            }
            catch { return false; }
        }
        private static string GetPhyPath(string FileName)
        {
            FileName = FileName.Replace('/', '\\');             //规范路径写法
            if (FileName.IndexOf(':') != -1) return FileName;   //已经是绝对路径了
            if (FileName.Length > 0 && FileName[0] == '\\') FileName = FileName.Substring(1);
            return string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, FileName);
        }
        private static System.IO.FileStream Create(string FileName)
        {
            FileName = GetPhyPath(FileName);
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
                return System.IO.File.Open(FileName, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
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
        public static int Set(string deviceID,bool bStart)
        {
            if (deviceID == "-1") ValueStart = bStart;
            else Start = bStart;
            return 0;
        }
        /// <summary>
        /// 写报文
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <param name="type">类型，0-发送，1-接收</param>
        /// <param name="msg">数据帧</param>
        /// <returns></returns>
        public static bool WriteLog(DateTime dateTime, int type, string msg)
        {
            if (!Start) return true;
            string tmp = string.Format("[{0}] {1}->{2}", dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), type == 0 ? "O" : "I", msg);
            return Log(LogFile,tmp);
        }
        public static bool WriteValue(string msg)
        {
            if (!ValueStart) return true;
            string tmp = string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
            return Log(ValueFile, tmp);
        }
        /// <summary>
        /// 删除超过日期的日志
        /// </summary>
        /// <returns></returns>
        public static bool DeleteLog(string path,string name)
        {
            try
            {
                if (!System.IO.Directory.Exists(path)) return true;
                int date = 14;
                string[] files = System.IO.Directory.GetFiles(path);
                string[] tmpArry = Array.FindAll(files, str => str.IndexOf(name + ".log") != -1);
                int len = tmpArry.Length;
                if (len <= date) return true;
                Array.Sort(tmpArry);
                for (int i = 0; i < len - date; i++)
                {
                    System.IO.File.Delete(string.Format("{0}", tmpArry[i]));
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static void DeleteAllLog()
        {
            DeleteLog(LogFile, Name);
            DeleteLog(ValueFile, ValueName);
        }
    }
}
