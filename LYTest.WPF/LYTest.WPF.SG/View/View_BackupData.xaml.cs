using LYTest.ViewModel.DataBackup;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_BackupData.xaml 的交互逻辑
    /// </summary>
    public partial class View_BackupData
    {
        readonly BackupViewModel viewModel = new BackupViewModel();
        public View_BackupData()
        {
            InitializeComponent();
            Name = "数据备份";
            DockStyle.IsMaximized = false;
            DockStyle.IsFloating = true; //是否开始是全屏  
            DockStyle.FloatingSize = new Size(1000, 650);
            this.DataContext = viewModel;
        }



        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Button btn)) return;

            if (!(btn.DataContext is BackupData source)) return;
            try
            {
                Process[] objProcesses = Process.GetProcessesByName("LYTest.DataManager.exe");
                if (objProcesses.Length <= 0)
                {
                    Process process = Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "LYTest.DataManager.exe"), source.FilePath);
                }

            }
            catch { }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.DataContext is BackupData source)) return;
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
                Process[] objProcesses = Process.GetProcessesByName("LYTest.DataManager.exe");
                if (objProcesses.Length <= 0)
                {
                    Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "LYTest.DataManager.exe"), viewModel.SelectBackup.FilePath);
                }

            }
            catch { }
        }

        private void ExporttDataClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
