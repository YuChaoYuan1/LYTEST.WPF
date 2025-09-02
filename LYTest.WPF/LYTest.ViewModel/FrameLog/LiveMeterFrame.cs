using LYTest.DAL;
using LYTest.ViewModel.Device;
using LYTest.ViewModel.Model;
using System;
using System.Linq;

namespace LYTest.ViewModel.FrameLog
{
    /// <summary>
    /// 实时报文记录
    /// </summary>
    public class LiveMeterFrame : ViewModelBase
    {
        public LiveMeterFrame()
        {
            BwCollection = new SelectCollection<int>();
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                BwCollection.ItemsSource.Add(i + 1);
            }
            BwCollection.PropertyChanged += BwCollection_PropertyChanged;
            BwCollection.SelectedItem = 1;
        }

        void BwCollection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                var modelsTemp = framesAll.Where(item => item.Index == BwCollection.SelectedItem);
                DisplayModels = new AsyncObservableCollection<DynamicViewModel>(modelsTemp);
            }
        }
        /// <summary>
        /// 报文集合
        /// </summary>
        private AsyncObservableCollection<DynamicViewModel> framesAll = new Model.AsyncObservableCollection<DynamicViewModel>();
        /// <summary>
        /// 添加到实时报文
        /// </summary>
        /// <param name="modelTemp"></param>
        public void AddFrame(DynamicModel modelTemp)
        {
            string portNo = modelTemp.GetProperty("chrPortNo") as string;
            string[] arrayTemp = portNo.Split('_');
            if (arrayTemp.Length != 3)
            {
                return;
            }
            PortUnit port = ServersViewModel.Instance.FindMeterPort(arrayTemp[1], arrayTemp[2]);
            if (port != null)
            {
                DateTime timeTemp = new DateTime();
                string sTime = modelTemp.GetProperty("chrSTime") as string;
                if (DateTime.TryParse(sTime, out timeTemp))
                {
                    modelTemp.SetProperty("chrSTime", timeTemp.ToString("HH:mm:ss.fff"));
                }
                string rTime = modelTemp.GetProperty("chrRTime") as string;
                if (DateTime.TryParse(rTime, out timeTemp))
                {
                    modelTemp.SetProperty("chrRTime", timeTemp.ToString("HH:mm:ss.fff"));
                }
                DynamicViewModel viewModelTemp = new DynamicViewModel(modelTemp, port.DeviceIndex + 1);
                framesAll.Add(viewModelTemp);
                if (port.DeviceIndex + 1 == BwCollection.SelectedItem)
                {
                    DisplayModels.Add(viewModelTemp);
                    if (EventNewFrame != null)
                    {
                        EventNewFrame(null, null);
                    }
                }
            }
        }

        public static event EventHandler EventNewFrame;

        public void ClearFrames()
        {
            framesAll.Clear();
            DisplayModels.Clear();
        }

        #region 显示相关
        private AsyncObservableCollection<DynamicViewModel> displayModels = new AsyncObservableCollection<DynamicViewModel>();
        /// <summary>
        /// 要显示的报文集合
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> DisplayModels
        {
            get { return displayModels; }
            set { SetPropertyValue(value, ref displayModels, "DisplayModels"); }
        }
        /// <summary>
        /// 表位集合
        /// </summary>
        public SelectCollection<int> BwCollection { get; set; }
        #endregion
    }
}
