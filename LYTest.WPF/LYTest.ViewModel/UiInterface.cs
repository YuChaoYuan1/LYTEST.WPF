using System;
using System.Threading;

namespace LYTest.ViewModel
{

    /// <summary>
    /// 与UI控制器之间的接口
    /// </summary>
    public class UiInterface
    {
        /// <summary>
        /// 界面消息到达事件
        /// </summary>
        public static event EventHandler UiMessageArrived;

        /// <summary>
        /// 关闭窗体
        /// </summary>
        public static event EventHandler EventCloseWindow;

        /// <summary>
        /// 更新界面显示
        /// </summary>
        /// <param name="uiHeader">要显示的控件的标题</param>
        /// <param name="uiClassName">控件类的名称</param>
        /// <param name="paramArray">控件构造函数的参数</param>
        public static void ChangeUi(string uiHeader, string uiClassName, string[] paramArray = null)
        {
            if (UiMessageArrived != null)
            {
                if (paramArray == null || paramArray.Length == 0)
                {
                    UiMessageArrived($"{uiHeader}|{uiClassName}", null);
                }
                else
                {
                    UiMessageArrived($"{uiHeader}|{uiClassName}|{string.Join(",", paramArray)}", null);
                }
            }
        }
        public static void CloseWindow(string windowName)
        {
            EventCloseWindow?.Invoke(windowName, null);
        }

        private static SynchronizationContext uiDispatcher = null;
        /// <summary>
        /// 界面线程,只允许设置一次
        /// </summary>
        public static SynchronizationContext UiDispatcher
        {
            get { return uiDispatcher; }
            set
            {
                //只允许设置一次
                if (uiDispatcher == null)
                {
                    uiDispatcher = value;
                }
            }
        }
    }
}
