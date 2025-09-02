using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.ViewModel.DataBackup
{
    /// <summary>
    /// 数据备份业务模型
    /// </summary>
    public class BackupViewModel : ViewModelBase
    {
        private AsyncObservableCollection<BackupData> backupFileList=new AsyncObservableCollection<BackupData>();
        /// <summary>
        /// 注释
        /// </summary>
        public AsyncObservableCollection<BackupData> BackupFileList
        {
            get { return backupFileList; }
            set { SetPropertyValue(value, ref backupFileList, "BackupFileList"); }
        }

        private BackupData selectBackup;
        /// <summary>
        /// 注释
        /// </summary>
        public BackupData SelectBackup
        {
            get { return selectBackup; }
            set { SetPropertyValue(value, ref selectBackup, "SelectBackup"); }
        }


        public BackupViewModel()
        { 
            initData();
        }

        private void initData()
        {
            BackupFileList.Clear();
            string sourcePath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(sourcePath)) return;
            if (Directory.GetLogicalDrives().Contains(sourcePath)) return;
            if (Directory.GetLogicalDrives().Contains(sourcePath + @"\")) return;
            if (Directory.GetLogicalDrives().Contains(sourcePath + @":\")) return;
            DirectoryInfo srcDir = new DirectoryInfo(sourcePath);
            string[] lgcdri = new string[] { "D:\\", "E:\\", srcDir.Root.FullName, "C:\\" };
            string destParent = "";
            for (int i = 0; i < lgcdri.Length; i++)
            {
                if (Directory.GetLogicalDrives().Contains(lgcdri[i]))
                {
                    destParent = Path.Combine(srcDir.FullName.Replace(srcDir.Root.FullName, $"{lgcdri[i]}自动备份"));
                    break;
                }
            }
            if (string.IsNullOrWhiteSpace(destParent)) return;
            if (!Directory.Exists(destParent)) return;

            string[] Names = Directory.GetDirectories(destParent);
            for (int i = 0; i < Names.Length; i++)
            {
                BackupData backup = new BackupData()
                {

                    Name = Path.GetFileName(Names[i]) ,
                    FilePath = Names[i],
                };
                BackupFileList.Add(backup);
            }
        }

        public void BackupData()
        {
            try
            {
                Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName("LYbackup");
                if (objProcesses.Length > 0)
                {

                }
                else
                {
                    System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "LYbackup.exe"), Directory.GetCurrentDirectory());
                    System.Threading.Thread.Sleep(500);
                    MessageBox.Show("备份成功");
                    initData();
                }

            }
            catch { }
        }
        private bool isRar=true;
        /// <summary>
        /// 注释
        /// </summary>
        public bool IsRar
        {
            get { return isRar; }
            set { SetPropertyValue(value, ref isRar, "IsRar"); }
        }


        /// <summary>
        /// 存放临时压缩文件路径
        /// </summary>
        string TempZipFilePath = Directory.GetCurrentDirectory()+"\\ZipTempData.zip";


        /// <summary>
        /// 导出备份数据
        /// </summary>
        public void ExportBackupData(BackupData Data,string Path)
        {

            if (IsRar) //导出压缩文件
            {
                TempZipFilePath = Data.FilePath + ".zip";
                //Path = Path +"\\"+ Data.Name + ".zip";
                Path = Path + ".zip";

                LYTest.Utility.TaskManager.AddUIAction(() =>
                {

                    if (System.IO.File.Exists(TempZipFilePath))
                    {
                        System.IO.File.Delete(TempZipFilePath);
                    }
                    if (File.Exists(Path))
                    {
                        MessageBox.Show("保存失败,保存位置已存在此文件");
                        return;
                    }
                    //压缩文件到临时文件
                    SharpZip.CompressDirectory(Data.FilePath, TempZipFilePath);
                    //把压缩完成的文件拷贝到指定的目录
                    System.IO.File.Copy(TempZipFilePath, Path);
                    //把压缩完的文件复制到指定位置
                    MessageBox.Show("保存成功,保存路径:" + Path);
                    if (System.IO.File.Exists(TempZipFilePath))
                    {
                        System.IO.File.Delete(TempZipFilePath);
                    }
                });
                //SharpZip.CompressFile(@"E:\新建文件夹\123.zip", Data.FilePath);
            }
            else
            {
                //整个文件夹备份到指定的路径
                CopyFolder(Data.FilePath, Path + $"\\{DateTime.Now.ToString("yyyy-MM-dd")}导出");
                MessageBox.Show("导出完成");
            }
        }
        public static void CopyFolder(string sourceFolder, string destinationFolder)
        {
            // 创建目标文件夹
            Directory.CreateDirectory(destinationFolder);

            // 复制文件
            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                File.Copy(file, destinationFile);
            }

            // 递归复制子文件夹
            foreach (string subfolder in Directory.GetDirectories(sourceFolder))
            {
                string destinationSubfolder = Path.Combine(destinationFolder, Path.GetFileName(subfolder));
                CopyFolder(subfolder, destinationSubfolder);
            }
        }


    }
}
