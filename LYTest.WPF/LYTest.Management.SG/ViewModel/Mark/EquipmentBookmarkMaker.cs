using LYTest.DAL.DataBaseView;
using LYTest.DataManager.SG.ViewModel.Mark;
using System;
using System.Threading;

namespace LYTest.DataManager.SG.Mark.ViewModel
{
    /// <summary>
    /// 台体信息书签制作器
    /// </summary>
    class EquipmentBookmarkMaker : ViewModelBase
    {
        public EquipmentBookmarkMaker()
        {
            new Thread(() =>
            {
                TableDisplayModel displayModel = ResultViewHelper.GetTableDisplayModel("41",true);
                if (displayModel != null)
                {
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

        /// <summary>
        /// 结论列表
        /// </summary>
        public AsyncObservableCollection<string> ResultNames { get; } = new AsyncObservableCollection<string>();

        #endregion
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
                    bookmarkName = string.Format("EquipmentInfoVX{0}VX{1}",  ResultName,Format);
                }
                return bookmarkName;
            }
        }
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
            EventAddBookmark?.Invoke(BookmarkName, null);
        }
    }
}
