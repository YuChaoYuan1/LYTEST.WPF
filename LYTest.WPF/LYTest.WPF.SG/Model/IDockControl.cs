

namespace LYTest.WPF.SG.Model
{
    /// 悬浮控件接口
    /// <summary>
    /// 悬浮控件接口
    /// </summary>
    public interface IDockControl
    {
        /// 控件的停靠属性
        /// <summary>
        /// 控件的停靠属性
        /// </summary>
        ModelDockStyle DockStyle { get; }
    }
}
