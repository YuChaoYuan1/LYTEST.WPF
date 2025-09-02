using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.DataBackup
{
    /// <summary>
    /// 备份文件数据模型
    /// </summary>
   public class BackupData : ViewModelBase
    {
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        private string filePath;
        /// <summary>
        /// 路径
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
            set { SetPropertyValue(value, ref filePath, "FilePath"); }
        }
    }
}
