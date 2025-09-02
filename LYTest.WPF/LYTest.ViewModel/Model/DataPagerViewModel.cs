using System;
using System.Collections.ObjectModel;

namespace LYTest.ViewModel.Model
{
    /// 分页控件的数据模型
    /// <summary>
    /// 分页控件的数据模型
    /// </summary>
    public class DataPagerViewModel : ViewModelBase
    {
        private int total;

        public int Total
        {
            get { return total; }
            set
            {
                SetPropertyValue(value, ref total, "Total");
                RefreshModel();
            }
        }
        private int pageIndex;

        public int PageIndex
        {
            get { return pageIndex; }
            set
            {
                SetPropertyValue(value, ref pageIndex, "PageIndex");
                RefreshModel();
            }
        }
        private int pageCount;

        public int PageCount
        {
            get { return pageCount; }
            set { SetPropertyValue(value, ref pageCount, "PageCount"); }
        }

        private int pageSize = 60;

        public int PageSize
        {
            get { return pageSize; }
            set
            {
                SetPropertyValue(value, ref pageSize, "PageSize");
                RefreshModel();
            }
        }
        private int start;

        public int Start
        {
            get { return start; }
            set { SetPropertyValue(value, ref start, "Start"); }
        }
        private int end;

        public int End
        {
            get { return end; }
            set { SetPropertyValue(value, ref end, "End"); }
        }

        private ObservableCollection<int> sizeCollection = new ObservableCollection<int>() { 10, 20, 24, 48, 60, 500, 1000 };

        public ObservableCollection<int> SizeCollection
        {
            get { return sizeCollection; }
            set { SetPropertyValue(value, ref sizeCollection, "SizeCollection"); }
        }

        public event EventHandler EventUpdateData;

        public void RefreshModel()
        {
            if (Total == 0)
            {
                pageIndex = 0;
                OnPropertyChanged("PageIndex");
                Start = 0;
                End = 0;
                PageCount = 0;
            }
            else
            {
                int temp = Total / PageSize;
                if (temp * PageSize < Total)
                {
                    temp = temp + 1;
                }
                PageCount = temp;
                if (PageIndex > PageCount)
                {
                    pageIndex = PageCount;
                    OnPropertyChanged("PageIndex");
                }
                if (pageIndex == 0)
                {
                    pageIndex = 1;
                    OnPropertyChanged("PageIndex");
                }
                Start = (pageIndex - 1) * PageSize + 1;
                End = Start + PageSize > Total ? Total : Start + PageSize - 1;
            }
            if (EventUpdateData != null)
            {
                EventUpdateData(this, null);
            }
        }
        public void MoveToFirst()
        {
            if (PageIndex > 1)
            {
                PageIndex = 1;
            }
        }
        public void MoveToPrev()
        {
            if (PageIndex > 1)
            {
                PageIndex -= 1;
            }
        }
        public void MoveToNext()
        {
            if (PageIndex < pageCount)
            {
                PageIndex += 1;
            }
        }
        public void MoveToEnd()
        {
            if (PageIndex < pageCount)
            {
                PageIndex = pageCount;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (EventUpdateData != null)
            {
                Delegate[] ds = EventUpdateData.GetInvocationList();
                foreach (Delegate d in ds)
                {
                    EventHandler pd = d as EventHandler;
                    if (pd != null)
                    {
                        EventUpdateData -= pd;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}
