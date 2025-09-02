using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LYTest.WPF.Model
{
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

        public static T FindLogicalParent<T>(this DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = LogicalTreeHelper.GetParent(obj);
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

        public static List<FrameworkElement> FindChildrenByName(FrameworkElement target, List<string> childNames)
        {
            List<FrameworkElement> listResult = new List<FrameworkElement>();
            if (childNames == null || childNames.Count == 0)
            {
                return listResult;
            }
            int childCount = VisualTreeHelper.GetChildrenCount(target);
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    FrameworkElement current = VisualTreeHelper.GetChild(target, i) as FrameworkElement;
                    if (current == null)
                    {
                        continue;
                    }
                    else if (current != null && childNames.Contains(current.Name))
                    {
                        listResult.Add(current);
                    }
                    else
                    {
                        listResult.AddRange(FindChildrenByName(current, childNames));
                    }
                }
            }
            return listResult;
        }

        public static List<T> FindChildren<T>(DependencyObject target)
            where T : DependencyObject
        {
            List<T> listTemp = new List<T>();
            if (target == null) { return listTemp; }
            var childCount = VisualTreeHelper.GetChildrenCount(target);
            if (childCount == 0)
            {
                return listTemp;
            }

            for (int i = 0; i < childCount; i++)
            {
                var current = VisualTreeHelper.GetChild(target, i);
                if (current is T)
                {
                    listTemp.Add(current as T);
                }

                listTemp.AddRange(FindChildren<T>(current));
            }
            return listTemp;
        }

        public static double GetDpiFactor(this Visual target)
        {
            var source = PresentationSource.FromVisual(target);
            return source == null ? 1.0 : 1 / source.CompositionTarget.TransformToDevice.M11;
        }
    }
}
