using System;
using System.IO;
using System.Linq;

namespace LYbackup
{
    class BackupLYM2
    {
        internal void RunOnce(string sourcePath)
        {
#if MY_DEBUG
            DateTime start = DateTime.Now;
#endif
            try
            {
                if (!Directory.Exists(sourcePath)) return;
                if (Directory.GetLogicalDrives().Contains(sourcePath)) return;
                if (Directory.GetLogicalDrives().Contains(sourcePath + @"\")) return;
                if (Directory.GetLogicalDrives().Contains(sourcePath + @":\")) return;

                DirectoryInfo srcDir = new DirectoryInfo(sourcePath);

                string[] lgcdri = new string[] { "D:\\", "E:\\", srcDir.Root.FullName, "C:\\" };
                string[] subDir = new string[] { "DataBase", "Xml", @"Res\Word", "Ini" };// 
                string[] dmFiles = new string[] {
                    "Aspose.Cells.dll",
                    "Aspose.Words.dll",
                    "AxInterop.OALib.dll",
                    "Interop.OALib.dll",
                    "log4net.dll",
                    "LYTest.Core.dll",
                    "LYTest.DAL.dll",
                    "LYTest.DataManager.exe",
                    "LYTest.Mis.dll",
                    "LYTest.Utility.dll",
                    "LYTest.ViewModel.dll",
                    "Oracle.ManagedDataAccess.dll"
                };
                for (int i = 0; i < lgcdri.Length; i++)
                {
                    if (Directory.GetLogicalDrives().Contains(lgcdri[i]))
                    {
                        string destParent = Path.Combine(srcDir.FullName.Replace(srcDir.Root.FullName, $"{lgcdri[i]}自动备份"), $"{DateTime.Now:yyyy-MM-dd}");

                        for (int i_dm = 0; i_dm < dmFiles.Length; i_dm++)
                        {
                            CopyExeFile(Path.Combine(srcDir.FullName, dmFiles[i_dm]), Path.Combine(destParent, dmFiles[i_dm]));
                        }

                        for (int i_sub = 0; i_sub < subDir.Length; i_sub++)
                        {
                            CopySubDir(Path.Combine(srcDir.FullName, subDir[i_sub]), Path.Combine(destParent, subDir[i_sub]));
                        }

                        break;
                    }
                }
            }
            catch
            {
#if MY_DEBUG
                Console.WriteLine("RunOnce Exception");
#endif
            }
#if MY_DEBUG
            Console.WriteLine(DateTime.Now - start);
#endif
        }

        private void CopyExeFile(string fullName, string destFile)
        {
            if (!File.Exists(fullName)) return;
#if MY_DEBUG
            Console.WriteLine(fileName + "\t" + destFile);
#endif
            if (!Directory.Exists(Path.GetDirectoryName(destFile))) Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            File.Copy(fullName, destFile, true);
        }

        private void CopySubDir(string pathDir, string destDir)
        {
            if (!Directory.Exists(pathDir)) return;
#if MY_DEBUG
            Console.WriteLine(pathDir + "\t" + destDir);
#endif
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            foreach (string fullName in Directory.GetFiles(pathDir))
            {
                File.Copy(fullName, Path.Combine(destDir, Path.GetFileName(fullName)), true);
            }
        }
    }
}
