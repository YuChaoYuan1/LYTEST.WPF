using System.Windows;

namespace LYTest.WPF.SG.Model
{
    public enum DockSideE
    {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3,
        Tab = 4
    }
    /// <summary>
    /// 用来设置控件转换成DockWindow时的各种属性
    /// </summary>
    public class ModelDockStyle
    {
        public ModelDockStyle()
        {
            CanClose = true;
            CanDockAsDocument = true;
            CanDockBottom = true;
            CanDockLeft = true;
            CanDockRight = true;
            CanDockTop = true;
            CanFloat = true;
            CloseButtonVisibility = Visibility.Visible;
            FloatingSize = new Size(1100, 700);
            ResizeMode = ResizeMode.CanResize;
        }
        /// <summary>
        /// 允许关闭
        /// </summary>
        public bool CanClose { get; set; }
        /// <summary>
        /// 允许作为文档停靠
        /// </summary>
        public bool CanDockAsDocument { get; set; }
        /// <summary>
        /// 允许在下部停靠
        /// </summary>
        public bool CanDockBottom { get; set; }
        /// <summary>
        /// 允许在左边停靠
        /// </summary>
        public bool CanDockLeft { get; set; }
        /// <summary>
        /// 允许在右边停靠
        /// </summary>
        public bool CanDockRight { get; set; }
        /// <summary>
        /// 允许在上面停靠
        /// </summary>
        public bool CanDockTop { get; set; }
        /// <summary>
        /// 允许悬浮
        /// </summary>
        public bool CanFloat { get; set; }
        /// <summary>
        /// 关闭按钮是否可见
        /// </summary>
        public Visibility CloseButtonVisibility { get; set; }
        private DockSideE position = DockSideE.Tab;
        /// <summary>
        /// 界面加载时出现在界面中的位置
        /// </summary>
        public DockSideE Position { get { return position; } set { position = value; } }
        /// <summary>
        /// 窗体是否悬浮
        /// </summary>
        public bool IsFloating { get; set; }
        /// <summary>
        /// 窗口悬浮时的大小
        /// </summary>
        public Size FloatingSize { get; set; }
        /// <summary>
        /// 窗体悬浮时设置大小的模式
        /// </summary>
        public ResizeMode ResizeMode { get; set; }
        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool IsMaximized { get; set; }
    }
}
