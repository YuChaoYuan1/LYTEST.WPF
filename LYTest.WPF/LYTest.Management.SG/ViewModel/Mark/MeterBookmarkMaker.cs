using LYTest.DataManager.SG.ViewModel.Mark;
using LYTest.DataManager.SG.ViewModel.Meters;
using System;
using System.Threading;

namespace LYTest.DataManager.SG.Mark.ViewModel
{
    /// <summary>
    /// 表信息书签制作器
    /// </summary>
    class MeterBookmarkMaker : ViewModelBase
    {
        public MeterBookmarkMaker()
        {
            new Thread(() =>
            {
                Thread.Sleep(5000);
                for (int i = 0; i < MetersViewModel.ParasModel.AllUnits.Count; i++)
                {
                    resultNames.Add(MetersViewModel.ParasModel.AllUnits[i].DisplayName);
                }
                OnPropertyChanged("ResultNames");
            }).Start();
        }
        #region 结论列表
        private string resultName;
        /// <summary>
        /// 结论名称
        /// </summary>
        public string ResultName
        {
            get { return resultName; }
            set
            {
                SetPropertyValue(value, ref resultName, "ResultName");
                OnPropertyChanged("BookmarkName");
                OnPropertyChanged("EnableAdd");
            }
        }

        private AsyncObservableCollection<string> resultNames = new AsyncObservableCollection<string>();
        /// <summary>
        /// 结论列表
        /// </summary>
        public AsyncObservableCollection<string> ResultNames
        {
            get
            {
                return resultNames;
            }
            set { SetPropertyValue(value, ref resultNames, "ResultNames"); }
        }
        #endregion
        #region 表序号
        private string meterIndex = "1";

        public string MeterIndex
        {
            get { return meterIndex; }
            set
            {
                SetPropertyValue(value, ref meterIndex, "MeterIndex");
                OnPropertyChanged("BookmarkName");
                OnPropertyChanged("EnableAdd");
            }
        }
        private AsyncObservableCollection<string> indexCollection = new AsyncObservableCollection<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

        public AsyncObservableCollection<string> IndexCollection
        {
            get { return indexCollection; }
            set { SetPropertyValue(value, ref indexCollection, "IndexCollection"); }
        }

        #endregion
        private EnumFormat format = EnumFormat.无;

        public EnumFormat Format
        {
            get { return format; }
            set
            {
                SetPropertyValue(value, ref format, "Format");
                OnPropertyChanged("BookmarkName");
                OnPropertyChanged("EnableAdd");
            }
        }
        private string bookmarkName;
        /// <summary>
        /// 书签名称
        /// </summary>
        public string BookmarkName
        {
            get
            {
                if (string.IsNullOrEmpty(ResultName))
                {
                    bookmarkName = "";
                }
                else
                {
                    bookmarkName = string.Format("表{0}VXMeterInfoVX{1}VX{2}", MeterIndex, ResultName,Format);
                }
                return bookmarkName;
            }
        }
        /// <summary>
        /// 允许添加书签
        /// </summary>
        public bool EnableAdd
        {
            get
            {
                return !string.IsNullOrEmpty(bookmarkName);
            }
        }

        public event EventHandler EventAddBookmark;
        /// <summary>
        /// 添加书签
        /// </summary>
        public void AddBookmark()
        {
            if (EventAddBookmark != null)
            {
                EventAddBookmark(BookmarkName, null);
            }
        }
    }
}
