using LYTest.ViewModel.DataBackup;
using LYTest.WPF.Controls;
using LYTest.WPF.View.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_BackupData.xaml 的交互逻辑
    /// </summary>
    public partial class View_BackupData
    {
        BackupViewModel viewModel;
        public View_BackupData()
        {
            InitializeComponent();
            Name = "数据备份";
            DockStyle.IsMaximized = false;
            DockStyle.IsFloating = true; //是否开始是全屏  
            DockStyle.FloatingSize = new Size(1000,650);
            viewModel = new BackupViewModel();
            this.DataContext = viewModel;
            //InitBackFile();
        }

        private void InitBackFile()
        {
            //for (int i = 0; i < viewModel.BackupFileList.Count; i++)
            //{
            //    BackipListPanel.Children.Add(new BcakupFileControl()
            //    {
            //        Width = 120,
            //        Height = 120 ,
            //        DataContext = viewModel.BackupFileList[i]
            //    });
            //}


        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            var source = btn.DataContext as BackupData;
            if (source == null) return;
            try
            {
                Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName("LYTest.DataManager.exe");
                if (objProcesses.Length > 0)
                {

                }
                else
                {
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "LYTest.DataManager.exe"), source.FilePath);
                }

            }
            catch { }
        }

        private void ExporttDataClick(object sender, RoutedEventArgs e)
        {
            //lists.Focus();


            if (viewModel.SelectBackup == null)
            {
                MessageBox.Show("请选择需要备份的文件");
                return;
            }
            new Window_ExportData(viewModel).ShowDialog();
            //viewModel.ImportBackupData(viewModel.SelectBackup,"");

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            var source = btn.DataContext as BackupData;
            if (source == null) return;
            viewModel.SelectBackup = source;
        }

        private void BorderRadiusButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectBackup == null)
            {
                MessageBox.Show("请选择需要预览的文件");
                return;
            }
            try
            {
                Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName("LYTest.DataManager.exe");
                if (objProcesses.Length > 0)
                {

                }
                else
                {
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "LYTest.DataManager.exe"), viewModel.SelectBackup.FilePath);
                }

            }
            catch { }
        }
    }
}
