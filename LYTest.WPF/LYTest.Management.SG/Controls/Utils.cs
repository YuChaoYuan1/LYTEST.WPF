using System.Windows;
using System.Windows.Media;

namespace LYTest.DataManager.SG.Controls
{
    /// 查找是某种类型的父元素
    /// <summary>
    /// 查找是某种类型的父元素
    /// </summary>
    internal static class Utils
    {
        /// 查找是某种类型的父元素,如果没找到返回空
        /// <summary>
        /// 查找是某种类型的父元素,如果没找到返回空
        /// </summary>
        /// <typeparam name="T">要查找的类型</typeparam>
        /// <param name="obj">子元素</param>
        /// <returns>找到的父元素</returns>
        public static T FindVisualParent<T>(this DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }


        public static T FindVisualChild<T>(this DependencyObject target)
            where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(target);
            if (childCount == 0) return null;

            for (int i = 0; i < childCount; i++)
            {
                var current = VisualTreeHelper.GetChild(target, i);
                if (current is T)
                    return (T)current;

                var desendent = FindVisualChild<T>(current);
                if (desendent != null)
                    return desendent;
            }
            return null;
        }

        public static double GetDpiFactor(this Visual target)
        {
            var source = PresentationSource.FromVisual(target);
            return source == null ? 1.0 : 1 / source.CompositionTarget.TransformToDevice.M11;
        }
    }
}
