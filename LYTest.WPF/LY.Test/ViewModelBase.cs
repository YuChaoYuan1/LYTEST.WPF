using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LY.Test
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase()
        {
        }

        private readonly Dictionary<string, object?> dir = [];


        /// <summary>
        /// 当此对象的属性具有新值时引发。
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">新的属性.</param>
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///用于设置具有相等属性值的泛型方法
        /// 检查并引发属性更改事件。
        /// </summary>
        protected internal void SetPropertyValue(object? value, [CallerMemberName] string propertyName = "")
        {
            if (dir.TryGetValue(propertyName, out object? obj))
            {
                if (obj != value)
                {
                    dir[propertyName] = value;
                }
                else
                {
                    return;
                }
            }
            else
            {
                dir.Add(propertyName, value);
            }

            OnPropertyChanged(propertyName);
        }

        protected internal T? GetPropertyValue<T>(T? defValue, [CallerMemberName] string propertyName = "")
        {
            if (dir.TryGetValue(propertyName, out object? value))
                return (T?)value;
            else
            {
                dir.Add(propertyName, defValue);
                return defValue;
            }
        }


    }

}
