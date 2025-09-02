using LYTest.DAL;
using LYTest.ViewModel.Model;
using System.Collections.Generic;

namespace LYTest.ViewModel.Time
{
    /// <summary>
    /// 时间监视器
    /// </summary>
    public class TimeMonitor : ViewModelBase
    {
        private static TimeMonitor instance = null;
        public static TimeMonitor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TimeMonitor();
                }
                return instance;
            }
        }

        /// <summary>
        /// 时间定时器
        /// </summary>
        public void Timer_Elapsed()
        {
            if (isLogIn)
            {
                LoginTime.Tick();
            }
            else
            {
                //if (EquipmentData.Controller.IsChecking)
                {
                    if (TimeCollection.SelectedItem == null)
                    {
                        if (EquipmentData.Controller.Index < TimeCollection.ItemsSource.Count && EquipmentData.Controller.Index >= 0)
                        {
                            TimeCollection.SelectedItem = TimeCollection.ItemsSource[EquipmentData.Controller.Index];
                        }
                    }
                    if (TimeCollection.SelectedItem != null)
                    {
                        TimeCollection.SelectedItem.Tick();
                    }
                    OnPropertyChanged(nameof(TotalTime));
                    OnPropertyChanged(nameof(StringTotalTime));
                    OnPropertyChanged(nameof(LeftTime));
                    OnPropertyChanged(nameof(StringLeftTime));
                    OnPropertyChanged(nameof(PastTime));
                    OnPropertyChanged(nameof(StringPastTime));
                    OnPropertyChanged(nameof(TextTooltip));
                }
            }
        }

        public TimeMonitor()
        {
            //检定点发生变化时，更新当前的检定点
            EquipmentData.Controller.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Index")
                {
                    if (EquipmentData.Controller.Index < TimeCollection.ItemsSource.Count)
                    {
                        TimeCollection.SelectedItem = TimeCollection.ItemsSource[EquipmentData.Controller.Index];
                    }
                }
            };
        }

        private List<DynamicModel> resources = new List<DynamicModel>();
        public void LogIn()
        {
            LoginTime.CheckModel = new CheckModelTime()
            {
                ParaNo = "-1"
            };
            LoginTime.Intialize();
            LoginTime.Start();
            LoginTime.IsValid = true;
        }

        /// <summary>
        /// 初始化时间监视器
        /// 加载每一个检定项的时常,计算出总的检定时长
        /// </summary>
        public void Initialize()
        {
            isLogIn = false;
            TimeCollection.ItemsSource.Clear();
            bool[] yaojian = EquipmentData.MeterGroupInfo.YaoJian;
            int firstIndex = 0;
            for (int Index = 0; Index < yaojian.Length; Index++)
            {
                if (yaojian[Index])
                {
                    firstIndex = Index;
                    break;
                }
            }
            string meterInfo = string.Format("{0}|{1}|{2}|{3}", EquipmentData.MeterGroupInfo.Meters[firstIndex].GetProperty("MD_UB") as string, EquipmentData.MeterGroupInfo.Meters[firstIndex].GetProperty("MD_UA") as string, EquipmentData.MeterGroupInfo.Meters[firstIndex].GetProperty("MD_WIRING_MODE") as string, EquipmentData.MeterGroupInfo.Meters[firstIndex].GetProperty("MD_CONSTANT") as string);
            //在这里加载每一个检定项的检定时长
            for (int i = 0; i < EquipmentData.Schema.ParaValues.Count; i++)
            {
                DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[i];
                if (viewModel != null)
                {
                    TimeItem timeItem = new TimeItem();
                    CheckModelTime timeModel = new CheckModelTime()
                    {
                        ParaNo = viewModel.GetProperty("PARA_NO") as string,
                        ParaValue = viewModel.GetProperty("PARA_VALUE") as string,
                        MeterInfo = meterInfo
                    };
                    timeItem.CheckModel = timeModel;
                    timeItem.Intialize();
                    TimeCollection.ItemsSource.Add(timeItem);
                }
            }
            if (EquipmentData.Controller.Index < TimeCollection.ItemsSource.Count && EquipmentData.Controller.Index >= 0)
            {
                TimeCollection.SelectedItem = TimeCollection.ItemsSource[EquipmentData.Controller.Index];
            }
            OnPropertyChanged("TotalTime");
            OnPropertyChanged("StringTotalTime");
            OnPropertyChanged("LeftTime");
            OnPropertyChanged("StringLeftTime");
            OnPropertyChanged("PastTime");
        }

        private SelectCollection<TimeItem> timeCollection = new SelectCollection<TimeItem>();
        /// <summary>
        /// 所有检定项时间列表
        /// </summary>
        public SelectCollection<TimeItem> TimeCollection
        {
            get { return timeCollection; }
            set { SetPropertyValue(value, ref timeCollection, "TimeCollection"); }
        }
        /// <summary>
        /// 已用时间
        /// </summary>
        public int PastTime
        {
            get { return totalTime - LeftTime; }
        }
        public string StringPastTime
        {
            get { return TimeItem.GetTimeString(PastTime); }
        }
        private int leftTime;
        /// <summary>
        /// 剩余检定时间
        /// </summary>
        public int LeftTime
        {
            get
            {
                if (TimeCollection.SelectedItem == null)
                {
                    leftTime = 0;
                    return 0;
                }
                int indexTemp = TimeCollection.ItemsSource.IndexOf(TimeCollection.SelectedItem);
                if (indexTemp < 0)
                {
                    leftTime = 0;
                    return 0;
                }
                int sum = TimeCollection.SelectedItem.TotalTime - TimeCollection.SelectedItem.PastTime;
                if (sum < 0)
                {
                    sum = 0;
                }
                for (int i = indexTemp + 1; i < TimeCollection.ItemsSource.Count; i++)
                {
                    sum += TimeCollection.ItemsSource[i].TotalTime;
                }
                leftTime = sum;
                return leftTime;
            }
        }
        /// <summary>
        /// 剩余时间字符串
        /// </summary>
        public string StringLeftTime
        {
            get { return TimeItem.GetTimeString(leftTime); }
        }
        /// <summary>
        /// 总时间对应字符串
        /// </summary>
        public string StringTotalTime
        {
            get { return TimeItem.GetTimeString(totalTime); }
        }
        private int totalTime;
        /// <summary>
        /// 项目预计总时间
        /// </summary>
        public int TotalTime
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < TimeCollection.ItemsSource.Count; i++)
                {
                    if (TimeCollection.ItemsSource[i].IsSelected)
                    {
                        sum += TimeCollection.ItemsSource[i].TotalTime;
                    }
                }
                totalTime = sum;
                return sum;
            }
        }

        private TimeItem loginTime = new TimeItem();
        /// <summary>
        /// 登录时间管理
        /// </summary>
        public TimeItem LoginTime
        {
            get { return loginTime; }
            set { SetPropertyValue(value, ref loginTime, "LoginTime"); }
        }
        private bool isLogIn = true;
        /// <summary>
        /// 将对应序号的点的时间设置为有效或无效
        /// </summary>
        /// <param name="index">要设置的项目的序号</param>
        public void ActiveCurrentItem(int index, bool isValid = false)
        {
            if (index >= 0 && index < TimeCollection.ItemsSource.Count)
            {
                TimeCollection.ItemsSource[index].IsValid = isValid;
            }
        }
        /// <summary>
        /// 开始当前的时间统计
        /// </summary>
        public void StartCurrentItem(int index)
        {
            if (index >= 0 && index < TimeCollection.ItemsSource.Count)
            {
                TimeCollection.SelectedItem = TimeCollection.ItemsSource[index];
                TimeCollection.ItemsSource[index].Start();
            }
        }
        /// <summary>
        /// 结束当前的时间统计
        /// </summary>
        public void FinishCurrentItem(int index)
        {
            if (index >= 0 && index < TimeCollection.ItemsSource.Count)
            {
                TimeCollection.ItemsSource[index].FinishItem();
            }
        }

        public string TextTooltip
        {
            get { return string.Format("预计总时间:{0},已过去:{1},剩余:{2}", StringTotalTime, StringPastTime, StringLeftTime); }
        }
    }
}
