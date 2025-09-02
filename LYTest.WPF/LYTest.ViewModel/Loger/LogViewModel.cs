using LYTest.DAL.Config;
using LYTest.Utility;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;

namespace LYTest.ViewModel.Log
{
    /// <summary>
    /// 日志视图模型
    /// </summary>
    public class LogViewModel : ViewModelBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }

        public bool IsShowWinForm = false;

        private static LogViewModel instance;
        public static LogViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogViewModel();
                }
                return instance;
            }
        }

        public void Initialize()
        {
            IsShowWinForm = ConfigHelper.Instance.IsShowErrorTips;
        }

        /// 添加新的日志数据
        /// <summary>
        /// 添加新的日志数据
        /// </summary>
        /// <param name="model"></param>
        public void AddLogModel(LogModel model)
        {
            if (IsShowWinForm)
            {
                if (model.Level == EnumLevel.Error)
                {
                    model.Level = EnumLevel.TipsError;
                }
                //else if (model.Level == EnumLevel.Tip)
                //{
                //    model.Level = EnumLevel.TipsTip;
                //}


            }

            //弹出窗体
            switch (model.Level)
            {
                case EnumLevel.Error:
                case EnumLevel.Tip:
                    TipMessage = model.Message;
                    break;
            }
            //switch (model.Level)
            //{
            //    case EnumLevel.InformationSpeech:
            //    case EnumLevel.WarningSpeech:
            //        //if (ConfigHelper.Instance.OpenVoice)
            //        //{
            //        //    SpeechClass.Instance.Speak(model);
            //        //}
            //        int temp = (int)model.Level;
            //        model.Level = (EnumLevel)(temp - 90);
            //        break;
            //}
            switch (model.LogSource)
            {
                case EnumLogSource.检定业务日志:
                    if (EquipmentData.Controller.NewArrived)
                    {
                        EquipmentData.Controller.NewArrived = false;
                    }
                    EquipmentData.Controller.NewArrived = true;
                    break;
            }
            LogsCheckLogic.Add(new LogUnitViewModel(model));
            SaveLog(model);
        }

        //用户操作日志
        //数据库存取日志
        //业务逻辑日志
        //检定业务日志
        //设备操作日志
        private LogCollection logsUserOperation = new LogCollection();
        /// 用户日志
        /// <summary>
        /// 用户日志
        /// </summary>
        public LogCollection LogsUserOperation
        {
            get { return logsUserOperation; }
            set { SetPropertyValue(value, ref logsUserOperation ); }
        }

        private LogCollection logsDatabase = new LogCollection();
        /// 数据库存取日志
        /// <summary>
        /// 数据库存取日志
        /// </summary>
        public LogCollection LogsDatabase
        {
            get { return logsDatabase; }
            set { SetPropertyValue(value, ref logsDatabase ); }
        }

        private LogCollection logsCheckLogic = new LogCollection();
        /// 检定业务日志
        /// <summary>
        /// 检定业务日志
        /// </summary>
        public LogCollection LogsCheckLogic
        {
            get { return logsCheckLogic; }
            set { SetPropertyValue(value, ref logsCheckLogic ); }
        }

        private LogCollection logsDevice = new LogCollection();
        /// 设备操作日志
        /// <summary>
        /// 设备操作日志
        /// </summary>
        public LogCollection LogsDevice
        {
            get { return logsDevice; }
            set { SetPropertyValue(value, ref logsDevice ); }
        }
        /// 日志的类,内部封装了一个定时器,如果日志的时间操作定时器的时间就会将日志移除
        /// <summary>
        /// 日志的类,内部封装了一个定时器,如果日志的时间操作定时器的时间就会将日志移除
        /// </summary>
        public class LogCollection : AsyncObservableCollection<LogUnitViewModel>
        {
            /// <summary>
            /// 当日志数量过大被删除时,会保留最近的日志数量
            /// </summary>
            private readonly int minLogCount = 20;
            /// <summary>
            ///当日志数量过大时会执行自动删除
            /// </summary>
            private readonly int maxLogCount = 200;
            readonly System.Timers.Timer timer = new System.Timers.Timer(60000);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="maxCount"></param>
            public LogCollection(int maxCount = 200)
            {
                maxLogCount = maxCount;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }

            private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                if (Items != null && Items.Count > maxLogCount)
                {
                    while (Count > minLogCount)
                    {
                        RemoveAt(0);
                    }
                }
            }
        }
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");//这里的 loginfo 和 log4net.config 里的名字要一样


        private void SaveLog(LogModel model)
        {
            TaskManager.AddSaveLogAction(() =>
            {
                //TODO 这里日志暂时统一存放，后续在根据什么类型的日志分开存储，更方便查询
                if (loginfo.IsInfoEnabled)
                {
                    //TODO 这里目前只写入了消息内容，后续可以写的更详细
                    string str = model.Message;
                    loginfo.Info(str);
                }


            });
        }

        private string tipMessage;
        /// <summary>
        /// 提示信息
        /// </summary>
        public string TipMessage
        {
            get { return tipMessage; }
            set
            {
                tipMessage = value;
                OnPropertyChanged("TipMessage");
            }
        }
        private int tipStepFinished;
        /// <summary>
        /// 单步测试完成提示信息触发++
        /// </summary>
        public int TipStepFinished
        {
            get { return tipStepFinished; }
            set
            {
                tipStepFinished = value;
                OnPropertyChanged("TipStepFinished");
            }
        }
    }
}
