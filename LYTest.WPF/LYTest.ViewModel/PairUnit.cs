using System.ComponentModel;

namespace LYTest.ViewModel
{
    public class PairUnit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// 属性更改事件
        /// <summary>
        /// 属性更改事件
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string key;
        /// 关键字
        /// <summary>
        /// 关键字
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; OnPropertyChanged("Key"); }
        }
        private object _value;

        public object Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }

    }
}
