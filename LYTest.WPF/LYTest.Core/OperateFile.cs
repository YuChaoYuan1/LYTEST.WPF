using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LYTest.Core
{
    public class OperateFile
    {
        #region "读取写入INI"
        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileBytes(string section, string key, string def, byte[] retVal, int size, string filePath);
        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 写入INI文件bai
        /// </summary>
        /// <param name=^Section^>节点名称</param>
        /// <param name=^Key^>关键字</param>
        /// <param name=^Value^>值</param>
        /// <param name=^filepath^>INI文件路径</param>
        public static void WriteINI(string Section, string Key, string Value, string filepath)
        {
            WritePrivateProfileString(Section, Key, Value, filepath);
        }

        public static bool ExistINIKey(string Section, string Key, string filepath)
        {
            try
            {
                filepath = GetPhyPath(filepath);
                if (File.Exists(filepath) == false) return false;
                string def = "没有节点";
                StringBuilder temp = new StringBuilder(2048);
                int i = GetPrivateProfileString(Section, Key, def, temp, 2048, filepath);
                if (temp.ToString() == def)
                {
                    return false;
                }
                if (i == 0 && string.IsNullOrEmpty(temp.ToString()))
                {
                    return true;
                }
                return i > 0;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section">节点名称</param>
        /// <param name="Key">键</param>
        /// <param name="filepath">路径</param>
        /// <returns></returns>
        public static string GetINI(string Section, string Key, string filepath)
        {
            try
            {
                filepath = GetPhyPath(filepath);
                if (File.Exists(filepath) == false)
                {
                    File.Create(filepath).Close();
                }
                StringBuilder temp = new StringBuilder(2048);
                int i = GetPrivateProfileString(Section, Key, "", temp, 2048, filepath);
                return temp.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }



        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section">节点名称</param>
        /// <param name="Key">键</param>
        /// <param name="filepath">路径</param>
        /// <param name="def">默认值</param>
        /// <returns></returns>
        public static string GetINI(string Section, string Key, string filepath, string def)
        {
            try
            {
                filepath = GetPhyPath(filepath);
                if (File.Exists(filepath) == false)
                {
                    File.Create(filepath).Close();
                }
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Section, Key, def, temp, 255, filepath);
                return temp.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// 根据相对路径获取文件、文件夹绝对路径
        /// </summary>
        /// <param name="FileName">相对路径</param>   
        /// <returns></returns>
        public static string GetPhyPath(string FileName)
        {
            FileName = FileName.Replace('/', '\\');             //规范路径写法
            if (FileName.IndexOf(':') != -1) return FileName;   //已经是绝对路径了
            if (FileName.Length > 0 && FileName[0] == '\\') FileName = FileName.Substring(1);
            return string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), FileName);
        }
        #endregion;

    }
}
