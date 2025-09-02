using LYTest.DAL.Config;
using LYTest.Utility.Log;
using LYTest.ViewModel;
using LYTest.ViewModel.CodeTree;
using LYTest.ViewModel.Log;
using LYTest.ViewModel.Time;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LYTest.WPF.SG
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        //private Mutex mutex;
        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);

        }
        private void App_Startup(object sender, StartupEventArgs e)
        {
            //bool ret;
            new Mutex(true, "LYTest.WPF.SG", out bool ret);
            if (!ret)
            {
                MessageBox.Show("程序已启动", "", MessageBoxButton.OK, MessageBoxImage.Stop);
                Environment.Exit(0);
            }
        }

        private readonly DispatcherTimer timer = new DispatcherTimer();

        // 程序启动前要执行的动作
        protected override void OnStartup(StartupEventArgs e)
        {
            UiInterface.UiDispatcher = SynchronizationContext.Current;
            CodeTreeViewModel.Instance.InitializeTree();

            timer.Interval = new TimeSpan(1000);
            timer.Tick += Timer_Tick;
            timer.Start();

            #region 新版本--2021-08-19：修改目的加上终端表位的日志信息

            ConfigHelper.Instance.LoadAllConfig();

            LogViewModel.Instance.Initialize();
            LogManager.LogMessageArrived += (sender, args) =>
            {
                if (sender is LogModel)
                {
                    LogViewModel.Instance.AddLogModel(sender as LogModel);
                }
            };
            EquipmentData.LastCheckInfo.LoadLastCheckInfo();    //加载最后一次的信息

            string path = System.IO.Directory.GetCurrentDirectory() + @"\Log\系统日志";
            System.IO.Directory.CreateDirectory(path);
            LogFile = $@"{path}\{DateTime.Now:yyyy-MM-dd}.txt";
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //Log(DateTime.Now.ToString() + "：未处理的异常");
            base.OnStartup(e);
            #endregion
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeMonitor.Instance.Timer_Elapsed();
        }

        /// <summary>
        /// 界面初始化标记
        /// </summary>
        private bool flagInitialize = false;

        // 程序可见后要执行的动作
        protected override void OnActivated(EventArgs e)
        {
            if (!flagInitialize)
            {
                UiInterface.UiDispatcher = SynchronizationContext.Current;
                flagInitialize = true;
            }
            base.OnActivated(e);
        }


        #region 异常处理
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出
                Log(DateTime.Now.ToString() + "：未处理的异常" + "\r\n" + e.Exception.ToString());
                MessageBox.Show("捕获未处理异常:" + e.Exception.Message + "\r\n" + e.Exception.ToString());

            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                Log(DateTime.Now.ToString() + "：未处理的异常" + "\r\n" + ex.ToString());
                MessageBox.Show("程序发生致命错误，将终止，请联系运营商！");
            }

        }
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            Log(DateTime.Now.ToString() + "：未处理的异常" + "\r\n" + e.ToString());
            MessageBox.Show("捕获线程内未处理异常：" + e.Exception.Message + "\r\n" + e.Exception.ToString());
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log(DateTime.Now.ToString() + "：未处理的异常");
            Log(e.ExceptionObject.ToString());
            Log(e.IsTerminating.ToString());

            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("程序将终止！\n");
            }
            sbEx.Append("捕获未处理异常：");
            if (e.ExceptionObject is FormatException ex0)
            {
                sbEx.Append(ex0.Message);
                sbEx.Append("请尝试用演示模式进入重新输入数据。");
            }
            else if (e.ExceptionObject is Exception ex1)
            {
                sbEx.Append(ex1.Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            MessageBox.Show("检测到未处理异常\r\n请查看文件:" + LogFile + "\r\n" + sbEx.ToString());

        }
        static string LogFile;
        public static void Log(string message)
        {
            System.IO.File.AppendAllText(LogFile, message + Environment.NewLine);
        }
        #endregion
    }
}
