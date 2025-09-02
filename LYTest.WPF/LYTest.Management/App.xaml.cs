using System.Threading;
using System.Windows;

namespace LYTest.DataManager
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UiInterface.UiDispatcher = SynchronizationContext.Current;
            base.OnStartup(e);
        }
    }
}
