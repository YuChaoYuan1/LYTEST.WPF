using LYTest.ViewModel.DataBackup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_ImportData.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ExportData : Window
    {
        BackupViewModel viewModel;
        public Window_ExportData(BackupViewModel data)
        {
            InitializeComponent();
            SaveName.Text = data.SelectBackup.Name;
            viewModel = data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = f.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(f.SelectedPath))
                {
                    pathText.Text = f.SelectedPath;
                }
            }
        }

        private void close_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            string path = pathText.Text.Trim();
            if (string.IsNullOrWhiteSpace(SaveName.Text))
            {
                MessageBox.Show("保存文件名不能为空");
                return;
            }
            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show("保存路径不正确,请重新选择");
                return;
            }
            path = path + "\\" + SaveName.Text;
            //"C:\Users\lenovo\Desktop\新建文件夹\2023-12-21.zip"
            if (System.IO.File.Exists(path+ ".zip"))
            {
                MessageBox.Show("文件名已存在,请重命名");
                return;
            }
            viewModel.ExportBackupData(viewModel.SelectBackup, path);
            this.Close();
        }
    }
}
