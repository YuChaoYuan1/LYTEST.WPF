using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using LYTest.ViewModel.Log;
using LYTest.WPF.SG.Model;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_Log.xaml 的交互逻辑
    /// </summary>
    public partial class View_Log
    {
        public View_Log()
        {

            InitializeComponent();
            Name = "运行日志";
            DockStyle.Position = DockSideE.Bottom;
            DockStyle.CanClose = false;
            DockStyle.CanFloat = false;
            DockStyle.CanDockAsDocument = false;
            //DockStyle.CanDockBottom = false;
            DockStyle.CanDockLeft = false;
            DockStyle.CanDockRight = false;
            DockStyle.CanDockTop = false;

            dataGrid.ItemsSource = LogViewModel.Instance.LogsCheckLogic;
            LogViewModel.LogCollection logCollection = dataGrid.ItemsSource as LogViewModel.LogCollection;
            if (logCollection != null)
            {
                logCollection.CollectionChanged += logCollection_CollectionChanged;
            }

            //【标注】 需要修改部分，右键清空日志，加给清空所有日志，清空日志是清空当前选中的tab的日志，tab绑定数据需要修改
            //logTab.DataContext = new LogViewModelS();

        }
        void logCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LogViewModel.LogCollection logCollection = dataGrid.ItemsSource as LogViewModel.LogCollection;
            if (logCollection != null && logCollection.Count > 0)
            {
                dataGrid.ScrollIntoView(logCollection[logCollection.Count - 1]);
            }
        }

        //清空日志
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogViewModel.LogCollection collection = dataGrid.ItemsSource as LogViewModel.LogCollection;
            if (collection != null)
            {
                collection.Clear();
            }
        }

        public override void Dispose()
        {
            LogViewModel.LogCollection logCollection = dataGrid.ItemsSource as LogViewModel.LogCollection;
            if (logCollection != null)
            {
                logCollection.CollectionChanged -= logCollection_CollectionChanged;
            }
            //清除绑定
            BindingOperations.ClearAllBindings(this);
            dataGrid.ItemsSource = null;
            menuItemClearLog.Click -= MenuItem_Click;
            base.Dispose();
        }
    }
}

