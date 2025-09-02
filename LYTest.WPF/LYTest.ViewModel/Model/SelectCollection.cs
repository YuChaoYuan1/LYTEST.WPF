namespace LYTest.ViewModel.Model
{
    /// <summary>
    /// 用于选择的集合列表,里面包含了一个被选中的点和集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectCollection<T>:ViewModelBase
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

        private AsyncObservableCollection<T> itemsSource=new AsyncObservableCollection<T>();
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
