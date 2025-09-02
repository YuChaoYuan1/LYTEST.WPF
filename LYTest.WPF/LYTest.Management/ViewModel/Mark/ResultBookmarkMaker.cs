using LYTest.DAL.DataBaseView;
using LYTest.DataManager.ViewModel.Mark;
using System;

namespace LYTest.DataManager.Mark.ViewModel
{
    /// <summary>
    /// 检定结论书签制作器
    /// </summary>
    class ResultBookmarkMaker : ViewModelBase
    {
        #region 检定点相关
        private SchemaViewModel shema = new SchemaViewModel();

        public SchemaViewModel Schema
        {
            get { return shema; }
            set { shema = value; }
        }

        private string itemKey;
        /// <summary>
        /// 检定点编号
        /// </summary>
        public string ItemKey
        {
            get { return itemKey; }
            set
            {
                SetPropertyValue(value, ref itemKey, "ItemKey");
                OnPropertyChanged("BookmarkName");
                OnPropertyChanged("EnableAdd");
            }
        }
        /// <summary>
        /// 加载检定点的编号
        /// </summary>
        public void LoadCurrentKey()
        {
            OnPropertyChanged("SummaryMarkName");
            OnPropertyChanged("EnableAddSummary");
            if (shema.ParaInfo.CheckParas.Count == 0)
            {
                ItemKey = shema.ParaNo;
                return;
            }
            string temp = "";
            for (int i = 0; i < shema.ParaInfo.CheckParas.Count; i++)
            {
                if (shema.ParaInfo.CheckParas[i].IsKeyMember)
                {
                    string tempId = shema.ParaInfo.CheckParas[i].CodeId;
                    if (string.IsNullOrEmpty(tempId))
                    {
                        ItemKey = "";
                        return;
                    }
                    else
                    {
                        temp = temp + tempId;
                    }
                }
            }
            if (string.IsNullOrEmpty(temp))
            {
                ItemKey = shema.ParaNo;
            }
            else
            {
                ItemKey = shema.ParaNo + "_" + temp;
            }
        }
        #endregion
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
            get { return resultNames; }
            set { SetPropertyValue(value, ref resultNames, "ResultNames"); }
        }
        /// <summary>
        /// 加载结论名称
        /// </summary>
        public void LoadResultNames()
        {
            ResultNames.Clear();
            if (Schema.ParaNo == null)
            {
                return;
            }
            TableDisplayModel displayModel = ResultViewHelper.GetParaNoDisplayModel(Schema.ParaNo);
            if (displayModel == null)
            { return; }
            for (int i = 0; i < displayModel.ColumnModelList.Count; i++)
            {
                string[] arrayDisplayName = displayModel.ColumnModelList[i].DisplayName.Split('|');
                for (int j = 0; j < arrayDisplayName.Length; j++)
                {
                    if (!string.IsNullOrEmpty(arrayDisplayName[j]))
                    {
                        ResultNames.Add(arrayDisplayName[j].Trim());
                    }
                }
            }
            for (int i = 0; i < displayModel.FKDisplayModelList.Count; i++)
            {
                FKDisplayConfigModel fkModel = displayModel.FKDisplayModelList[i];
                for (int j = 0; j < fkModel.DisplayNames.Count; j++)
                {
                    ResultNames.Add(fkModel.DisplayNames[j]);
                }
            }
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
        #region 数据格式
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

        #endregion
        private string bookmarkName;
        /// <summary>
        /// 书签名称
        /// </summary>
        public string BookmarkName
        {
            get
            {
                if (string.IsNullOrEmpty(ItemKey) || string.IsNullOrEmpty(ResultName))
                {
                    bookmarkName = "";
                }
                else
                {
                    bookmarkName = string.Format("表{0}VX{1}VX{2}VX{3}", MeterIndex, ItemKey, ResultName, Format);
                }
                return bookmarkName;
            }
        }

        public string CategoryNo { get; set; }
        private string summaryMarkName;
        /// <summary>
        /// 某一个类型的总结论
        /// </summary>
        public string SummaryMarkName
        {
            get
            {
                if (string.IsNullOrEmpty(CategoryNo))
                {
                    summaryMarkName = "";
                }
                else
                {
                    summaryMarkName = string.Format("表{0}VX{1}", MeterIndex, CategoryNo);
                }
                return summaryMarkName;
            }
        }

        /// <summary>
        /// 允许添加总结论书签
        /// </summary>
        public bool EnableAddSummary
        {
            get
            {
                return !string.IsNullOrEmpty(SummaryMarkName);
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
        public void AddSummaryBookmark()
        {
            if (EventAddBookmark != null)
            {
                EventAddBookmark(SummaryMarkName, null);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (EventAddBookmark != null)
            {
                Delegate[] arrayDelegate = EventAddBookmark.GetInvocationList();
                foreach (Delegate d in arrayDelegate)
                {
                    if (d is EventHandler)
                    {
                        EventAddBookmark -= (EventHandler)d;
                    }
                }
                EventAddBookmark = null;
            }
            base.Dispose(disposing);
        }
    }
}
