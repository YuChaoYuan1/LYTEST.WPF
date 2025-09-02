using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager
{
    public abstract class ViewModelBase : IDisposable, INotifyPropertyChanged
    {
        #region 构造器 / Fields 

        /// <summary>
        /// 默认构造函数。
        /// </summary>
        protected ViewModelBase()
        {
            LocalCommand.CommandAction = (obj) => CommandFactoryMethod(obj as string);
        }
        /// <summary>
        /// 使用显示名称构造。
        /// </summary>
        protected ViewModelBase(string displayName)
        {
        }

        #endregion //构造器


        #region INotifyPropertyChanged Members

        /// <summary>
        /// 当此对象的属性具有新值时引发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">新的属性.</param>
        internal protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        ///用于设置具有相等属性值的泛型方法
        /// 检查并引发属性更改事件。
        /// </summary>
        internal protected bool SetPropertyValue<T>(T value, ref T field, string propertyName)
        {
            if ((value != null && !value.Equals(field)) || (value == null && field != null))
            {
                field = value;
                if (propertyName != null)
                {
                    OnPropertyChanged(propertyName);
                }
                return true;
            }
            return false;
        }


        #endregion // INotifyPropertyChanged Members

        /// <summary>
        /// 从应用程序中删除此对象时调用
        /// 并将接受垃圾收集。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 子类可以重写此方法以执行
        /// 清理逻辑，例如删除事件实例处理程序。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (PropertyChanged != null)
            {
                Delegate[] ds = PropertyChanged.GetInvocationList();
                foreach (Delegate d in ds)
                {
                    PropertyChangedEventHandler pd = d as PropertyChangedEventHandler;
                    if (pd != null)
                    {
                        PropertyChanged -= pd;
                    }
                }
            }
            LocalCommand.CommandAction = null;
            LocalCommand = null;
        }

        #region 命令相关
        private BasicCommand localCommand;
        /// 控件命令
        /// <summary>
        /// 控件命令
        /// </summary>
        public BasicCommand LocalCommand
        {
            get
            {
                if (localCommand == null)
                {
                    localCommand = new BasicCommand();
                }
                return localCommand;
            }
            set { localCommand = value; }
        }
        /// <summary>
        /// 命令工厂方法
        /// </summary>
        /// <param name="methodName"></param>
        public virtual void CommandFactoryMethod(string methodName)
        {
            try
            {
                //将方法添加到数据库进程去处理
                MethodInfo method = GetType().GetMethod(methodName);
                method.Invoke(this, new object[] { });
            }
            catch (Exception e)
            {
                LogManager.AddMessage(string.Format("调用方法:{0} 出错:{1}", methodName, e.Message), EnumLogSource.用户操作日志, EnumLevel.Error, e);
            }
        }
        #endregion

        /// <summary>
        /// 用于确保ViewModel对象被正确地垃圾收集。
        /// </summary>
        ~ViewModelBase()
        {
        }

    }
}
