using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager.SG
{
    public class SelectCollection<T> : ViewModelBase
    {
        private T selectedItem;
        /// <summary>
        /// 选中的点
        /// </summary>
        public T SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set { SetPropertyValue(value, ref selectedItem, "SelectedItem"); }
        }

        private AsyncObservableCollection<T> itemsSource = new AsyncObservableCollection<T>();
        /// <summary>
        /// 数据源
        /// </summary>
        public AsyncObservableCollection<T> ItemsSource
        {
            get { return itemsSource; }
            set { SetPropertyValue(value, ref itemsSource, "ItemsSource"); }
        }
    }
}
