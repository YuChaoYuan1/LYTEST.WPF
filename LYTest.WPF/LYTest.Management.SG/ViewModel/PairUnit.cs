using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager.SG
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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
