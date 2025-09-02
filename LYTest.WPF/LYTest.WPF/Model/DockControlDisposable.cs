using System;
using System.Windows.Controls;

namespace LYTest.WPF.Model
{
    /// <summary>
    /// 在项目中创建的自定义控件的基类
    /// </summary>
    public class DockControlDisposable : UserControl, IDisposable, IDockControl
    {
        /// <summary>
        /// 控件的事件和数据绑定注销
        /// </summary>
        public virtual void Dispose()
        {
            Content = null;
        }

        private ModelDockStyle dockStyle = new ModelDockStyle();
        /// <summary>
        /// 控件停靠样式
        /// </summary>
        public ModelDockStyle DockStyle
        {
            get { return dockStyle; }
            protected set { dockStyle = value; }
        }
    }
}
