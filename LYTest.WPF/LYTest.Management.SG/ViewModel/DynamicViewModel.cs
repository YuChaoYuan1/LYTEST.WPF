using LYTest.DAL;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager.SG
{
    public class DynamicViewModel : DynamicObject, INotifyPropertyChanged, IDisposable
    {

        private int index;
        /// 序号
        /// <summary>
        /// 序号
        /// </summary>
        public int Index
        {
            get { return index; }
            set
            {
                if (value != index)
                {
                    index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Index"));
                }
            }
        }

        ///// 存储在后台的数据源
        ///// <summary>
        ///// 存储在后台的数据源
        ///// </summary>
        //private DynamicModel dataSource = new DynamicModel();
        /// 存储在后台的数据源
        /// <summary>
        /// 存储在后台的数据源
        /// </summary>
        private readonly ObservableCollection<PairUnit> dataSource = new ObservableCollection<PairUnit>();
        public DynamicViewModel(int index)
        {
            Index = index;
        }
        /// 初始化动态属性,不要有相同的名字
        /// <summary>
        /// 初始化动态属性,不要有相同的名字
        /// </summary>
        /// <param name="listPropertyName"></param>
        /// <param name="index"></param>
        public DynamicViewModel(List<string> listPropertyName, int index)
        {
            Index = index;
            for (int i = 0; i < listPropertyName.Count; i++)
            {
                SetProperty(listPropertyName[i], null);
            }
        }
        public DynamicViewModel(DynamicModel model, int index)
        {
            Index = index;
            dataSource = new ObservableCollection<PairUnit>();
            List<string> propertyNames = model.GetAllProperyName();
            for (int i = 0; i < propertyNames.Count; i++)
            {
                dataSource.Add(new PairUnit
                {
                    Key = propertyNames[i],
                    Value = model.GetProperty(propertyNames[i])
                });
            }
        }
        /// 重写Set方法
        /// <summary>
        /// 重写Set方法
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.Name == "写入内容")
            {
                bool isNuber = true;
                string strValue = value.ToString();
                //for (int i = 0; i < strValue.Length; i++)
                //{
                //    if (!Char.IsNumber(strValue, i))
                //    {
                //        isNuber = false;
                //        break;
                //    }
                //}  
                if (!IsNumber(strValue) && !IsHex(strValue))
                {
                    isNuber = false;
                }
                if (!isNuber)
                    System.Windows.MessageBox.Show("输入格式有误，请输入数字或十六进制。");

            }

            SetProperty(binder.Name, value);
            if (value == null || !value.Equals(GetProperty(binder.Name)))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(binder.Name));
            }
            return true;
        }
        private bool IsNumber(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$");
        }
        private bool IsHex(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^[0-9A-Fa-f]+$");

        }

        public void SetProperty(string propertyName, object value)
        {
            var unit = dataSource.ToList().Find(item => item.Key == propertyName);
            if (unit == null)
            {
                dataSource.Add(new PairUnit { Key = propertyName, Value = value });
            }
            else
            {
                unit.Value = value;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public object GetProperty(string propertyName)
        {
            try
            {
                var unit = dataSource.ToList().FirstOrDefault(item => item.Key == propertyName);
                if (unit == null)
                {
                    return null;
                }
                else
                {
                    return unit.Value;
                }
            }
            catch (Exception)
            {
                try
                {
                    System.Threading.Thread.Sleep(20);
                    var unit = dataSource.ToList().FirstOrDefault(item => item.Key == propertyName);
                    if (unit == null)
                    {
                        return null;
                    }
                    else
                    {
                        return unit.Value;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddMessage(string.Format("获取数据异常,这个异常往往是线程不同步引起的:{0}", ex.Message), EnumLogSource.用户操作日志);
                    return null;
                }

            }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get => GetProperty(propertyName);
            set => SetProperty(propertyName, value);
        }

        /// <summary>
        ///  使用默认比较器对整个 DynamicViewModel 中的元素进行排序。
        /// </summary>
        /// <param name="alert">是否启用警报</param>
        /// <returns>返回排序是否成功</returns>
        public virtual bool Sort(bool alert = false)
        {
            try
            {
                List<PairUnit> temp = dataSource.ToList();
                temp.Sort((x, y) => x.Key.CompareTo(y.Key));
                dataSource.Clear();
                foreach (PairUnit kvp in temp)
                {
                    dataSource.Add(kvp);
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!alert)
                {
                    System.Threading.Thread.Sleep(200);
                    return Sort(true);
                }
                LogManager.AddMessage(string.Format("数据排序异常,这个异常往往是线程不同步引起的:{0}", ex.Message), EnumLogSource.用户操作日志);
                return false;
            }
        }

        /// <summary>
        ///  将整个DynamicViewModel中元素的顺序反转。
        /// </summary>
        /// <param name="alert">是否启用警报</param>
        /// <returns>返回是否逆序成功</returns>
        public virtual bool Reverse(bool alert = false)
        {
            try
            {
                List<PairUnit> temp = dataSource.ToList();
                temp.Reverse();
                dataSource.Clear();
                foreach (PairUnit kvp in temp)
                {
                    dataSource.Add(kvp);
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!alert)
                {
                    System.Threading.Thread.Sleep(200);
                    return Reverse(true);
                }
                LogManager.AddMessage(string.Format("数据逆序异常,这个异常往往是线程不同步引起的:{0}", ex.Message), EnumLogSource.用户操作日志);
                return false;
            }
        }


        /// <summary>
        /// 重写get方法
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetProperty(binder.Name);
            return true;
        }

        /// <summary>
        /// 获取所有属性
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllProperyName()
        {
            List<string> list = new List<string>();
            var temp = from item in dataSource select item.Key;
            if (temp != null)
            {
                list = temp.ToList();
            }
            return list;
        }

        /// <summary>
        /// 获取视图的数据源
        /// </summary>
        public DynamicModel GetDataSource()
        {
            DynamicModel model = new DynamicModel();
            for (int i = 0; i < dataSource.Count; i++)
            {
                model.SetProperty(dataSource[i].Key, dataSource[i].Value);
            }
            return model;
        }
        public void RemoveProperty(string propertyName)
        {
            var units = dataSource.Where(item => item.Key == propertyName);
            if (units != null)
            {
                dataSource.Remove(units.ElementAt(0));
            }
        }
        #region
        /// 属性变化事件
        /// <summary>
        /// 属性变化事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public void Dispose()
        {
            if (PropertyChanged != null)
            {
                //Delegate[] ds = PropertyChanged.GetInvocationList();
                foreach (Delegate d in PropertyChanged.GetInvocationList())
                {
                    if (d is PropertyChangedEventHandler pd)
                    {
                        PropertyChanged -= pd;
                    }
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
