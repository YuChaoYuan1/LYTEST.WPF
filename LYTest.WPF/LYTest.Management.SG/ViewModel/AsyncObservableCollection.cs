using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace LYTest.DataManager.SG
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private readonly object lockObj = new object();
        public AsyncObservableCollection()
        { }
        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        { }
        /// <summary>
        /// 重写集合更改事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == UiInterface.UiDispatcher)
            {
                base.OnCollectionChanged(e);
            }
            else
            {
                UiInterface.UiDispatcher.Send(obj =>
                {
                    try
                    {
                        base.OnCollectionChanged(e);
                    }
                    catch (Exception ex)
                    {
                        LogManager.AddMessage(string.Format("列表数据操作失败:{0}", ex.Message), EnumLogSource.用户操作日志, EnumLevel.Warning, ex);
                    }
                }, e);
            }
        }

        protected override void ClearItems()
        {
            if (SynchronizationContext.Current == UiInterface.UiDispatcher)
            {
                lock (lockObj)
                {
                    base.ClearItems();
                }
            }
            else
            {
                UiInterface.UiDispatcher.Send(obj =>
                {
                    lock (lockObj)
                    {
                        base.ClearItems();
                    }
                }, null);
            }
        }
        protected override void InsertItem(int index, T item)
        {
            if (SynchronizationContext.Current == UiInterface.UiDispatcher)
            {
                //if (index <= Count && index >= 0)
                //{
                lock (lockObj)
                {
                    base.InsertItem(index, item);
                }
                //}
            }
            else
            {
                UiInterface.UiDispatcher.Send(obj =>
                {
                    try
                    {
                        lock (lockObj)
                        {
                            //if (index <= Count && index >= 0)
                            //{
                            base.InsertItem(index, item);
                            //}
                        }
                    }
                    catch
                    { }
                }, null);
            }
        }
        protected override void RemoveItem(int index)
        {
            if (SynchronizationContext.Current == UiInterface.UiDispatcher)
            {
                lock (lockObj)
                {
                    base.RemoveItem(index);
                }
            }
            else
            {
                UiInterface.UiDispatcher.Send(obj =>
                {
                    try
                    {
                        lock (lockObj)
                        {
                            if (Count > index)
                            {
                                base.RemoveItem(index);
                            }
                        }
                    }
                    catch
                    {
                    }
                }, null);
            }
        }
        /// <summary>
        /// 重写属性更改事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            try
            {
                if (SynchronizationContext.Current == UiInterface.UiDispatcher)
                {
                    base.OnPropertyChanged(e);
                }
                else
                {
                    UiInterface.UiDispatcher.Send(obj => base.OnPropertyChanged(obj as PropertyChangedEventArgs), e);
                }
            }
            catch (Exception ex)
            {
                LogManager.AddMessage(string.Format("列表数据操作失败:{0}", ex.Message), EnumLogSource.用户操作日志, EnumLevel.Warning, ex);
            }
        }
        /// <summary>
        /// 内部排序方法
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="keySelector"></param>
        public void Sort<TKey>(Func<T, TKey> keySelector)
        {
            var sortedItems = Items.OrderBy(keySelector);
            for (int i = 0; i < sortedItems.Count(); i++)
            {
                var item = sortedItems.ElementAt(i);
                Move(IndexOf(item), i);
            }
        }
    }
}
