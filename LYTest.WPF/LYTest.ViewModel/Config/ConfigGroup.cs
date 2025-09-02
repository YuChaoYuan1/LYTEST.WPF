using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Config
{
    /// <summary>
    /// 一组配置数据
    /// </summary>
    public class ConfigGroup : ViewModelBase
    {
        public int ID { get; set; }


        public string StringValue
        {
            get
            {
                var values = from item in Units select item.ConfigValue;
                return string.Join("|", values);
            }
        }

        private AsyncObservableCollection<ConfigUnit> units = new AsyncObservableCollection<ConfigUnit>();
        /// <summary>
        /// 数据源
        /// </summary>
        public AsyncObservableCollection<ConfigUnit> Units
        {
            get { return units; }
            set { units = value; }
        }
        private bool changeFlag;

        public bool ChangeFlag
        {
            get { return changeFlag; }
            set
            {
                SetPropertyValue(value, ref changeFlag, "ChangeFlag");
                OnPropertyChanged("StringValue");
            }
        }

    }
}
