using LYTest.Utility.Log;
using LYTest.WPF.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LYTest.WPF.Skin
{
    /// <summary>
    /// 皮肤管理器
    /// </summary>
    public class SkinManager
    {
        /// <summary>
        /// 设置程序的颜色
        /// </summary>
        public static void SetAppColor()
        {
            try
            {
                MainWindow windowTemp = Application.Current.MainWindow as MainWindow;
                ChangeSkin(windowTemp.AppDock2);
                //List<DockSplitter> splitters = Utils.FindChildren<DockSplitter>(windowTemp.AppDock);
                //for (int i = 0; i < splitters.Count; i++)
                //{
                //    splitters[i].Background = Application.Current.Resources["分隔条颜色"] as Brush;
                //}
                //List<DockWindowGroup> windowGroups = Utils.FindChildren<DockWindowGroup>(windowTemp.AppDock);
                //for (int i = 0; i < splitters.Count; i++)
                //{
                //    windowGroups[i].Background = Application.Current.Resources["窗口背景色"] as Brush;
                //    windowGroups[i].BorderBrush = Application.Current.Resources["边框颜色"] as Brush;
                //}
                List<FrameworkElement> elements = Utils.FindChildrenByName(windowTemp.AppDock2, new List<string>() { "PART_DockBackground" });
                if (elements.Count > 0)
                {
                    Border borderTemp = Utils.FindVisualChild<Border>(elements[0]);
                    if (borderTemp != null)
                    {
                        borderTemp.BorderThickness = new Thickness(0);
                        borderTemp.Background = Application.Current.Resources["窗口背景色"] as Brush;
                    }
                }
                elements = Utils.FindChildrenByName(windowTemp.AppDock2, new List<string>() { "PART_PanelBorder" });
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is Border)
                    {
                        ((Border)elements[i]).Background = Application.Current.Resources["窗口背景色"] as Brush;
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.AddMessage(string.Format("更改程序外观失败:{0}", e.Message), EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
        }
        #region DockSite设置颜色用到的一些东西
        /// <summary>
        /// 刷新所有Dock控件相关的皮肤颜色
        /// </summary>
        private static void ChangeSkin(FrameworkElement elementTemp)
        {
            if (elementTemp == null)
            {
                return;
            }
            List<string> listNames = new List<string> { "PART_GroupCaption", "PART_TabPanel" };
            //PART_GroupCaption
            //PART_TabPanel
            List<FrameworkElement> listTemp = Utils.FindChildrenByName(elementTemp, listNames);
            for (int i = 0; i < listTemp.Count; i++)
            {
                SetpropertyInfo(listTemp[i], "Background", Application.Current.Resources["线性渐变颜色"]);
            }
            listNames = new List<string> { "PART_DockBackground" };
            //PART_GroupCaption
            //PART_TabPanel
            listTemp = Utils.FindChildrenByName(elementTemp, listNames);
            for (int i = 0; i < listTemp.Count; i++)
            {
                SetpropertyInfo(listTemp[i], "Background", Application.Current.Resources["线性渐变颜色"]);
            }
            List<FrameworkElement> borders = Utils.FindChildrenByName(elementTemp, new List<string> { "TabBorder" });
            for (int i = 0; i < borders.Count; i++)
            {
                SetpropertyInfo(borders[i], "Background", Application.Current.Resources["窗口背景色"] as Brush);
                SetpropertyInfo(borders[i], "BorderBrush", Application.Current.Resources["边框颜色"] as Brush);
            }
        }
        /// <summary>
        /// 改变停靠窗体的颜色
        /// </summary>
        /// <param name="windowTemp"></param>
        public static void ChangeWindowSkin(DockControlDisposable dockControl)
        {
            //if (dockControl == null)
            //{
            //    return;
            //}
            //DockWindowGroup windowGroup = Utils.FindLogicalParent<DockWindowGroup>(dockControl);
            //if (windowGroup != null)
            //{
            //    windowGroup.Background = Application.Current.Resources["窗口背景色"] as Brush;
            //    windowGroup.BorderBrush = Application.Current.Resources["边框颜色"] as Brush;
            //    ChangeSkin(windowGroup);
            //}
        }

        private static void SetpropertyInfo(object obj, string propertyName, object valueTemp)
        {
            try
            {
                Type typeTemp = obj.GetType();
                PropertyInfo propertyInfo = typeTemp.GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    return;
                }
                propertyInfo.SetValue(obj, valueTemp, null);
            }
            catch
            {
            }
        }
        #endregion
    }
}
