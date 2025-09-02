using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel;
using LYTest.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace LYTest.WPF.Model
{
    /// <summary>
    /// 主窗体信息视图
    /// </summary>
    public class MainViewModel : ViewModelBase
    {


        /// <summary>
        /// 当前选择页
        /// </summary>
        public int SelectIndex
        {
            get => GetProperty(0);
            set => SetProperty(value);
        }

        /// <summary>
        /// 窗体集合的单例
        /// </summary>
        public static MainViewModel Instance { get; } = new MainViewModel();



        /// <summary>
        /// 获取界面原始线程
        /// </summary>
        private readonly SynchronizationContext originalDispatcher = SynchronizationContext.Current;
        readonly List<DynamicModel> models;
        readonly ViewModel.Menu.MenuViewModel menuModel;
        /// 初始化要显示的窗体
        /// <summary>
        /// 初始化要显示的窗体
        /// </summary>
        public MainViewModel()
        {
            models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_MENU_VIEW.ToString());
            menuModel = new ViewModel.Menu.MenuViewModel();
            //必须进入当前线程调用界面更新
            UiInterface.UiMessageArrived += (sender, e) =>
            {
                if (SynchronizationContext.Current == originalDispatcher && originalDispatcher != null)
                {
                    CommandFactoryMethod(sender as string);
                }
                else
                {
                    originalDispatcher.Post(obj =>
                    {
                        CommandFactoryMethod(obj as string);
                    }, sender);
                }
            };
            UiInterface.EventCloseWindow += (sender, e) =>
            {
                string windowName = sender as string;
                if (string.IsNullOrEmpty(windowName))
                {
                    return;
                }
                if (SynchronizationContext.Current == originalDispatcher && originalDispatcher != null)
                {
                    for (int i = 0; i < WindowsAll.Count; i++)
                    {
                        DockWindowDisposable windowTemp = WindowsAll[i];
                        if (windowTemp.Name == windowName)
                        {
                            //WindowsAll.Remove(windowTemp);
                            windowTemp.OnClosed2(null);

                            return;
                        }
                    }
                }
                else
                {
                    originalDispatcher.Post(obj =>
                    {
                        for (int i = 0; i < WindowsAll.Count; i++)
                        {
                            DockWindowDisposable windowTemp = WindowsAll[i];
                            if (windowTemp.Name == windowName)
                            {
                                //WindowsAll.Remove(windowTemp);
                                windowTemp.OnClosed2(null);

                                return;
                            }
                        }
                    }, sender);
                }

            };
        }
        /// <summary>
        /// 所有窗体列表
        /// </summary>
        public AsyncObservableCollection<DockWindowDisposable> WindowsAll { get; } = new AsyncObservableCollection<DockWindowDisposable>();

        public override void CommandFactoryMethod(string windowName)
        {
            #region commandparameter里面的参数格式:页面名称|页面类的名称,参数,参数......
            if (string.IsNullOrEmpty(windowName))
            {
                return;
            }
            string[] windowNameArray = windowName.Split('|');
            if (windowNameArray.Length < 2)
            {
                return;
            }
            if (windowNameArray[0] == "检定" || windowNameArray[0] == "详细数据")
                EquipmentData.Controller.IsTestMainVisible = true;
            else
                EquipmentData.Controller.IsTestMainVisible = false;
            #endregion

            #region 首先遍历现在的窗体,如果与现有窗体,将窗体显示出来
            DockWindowDisposable windowNow = WindowsAll.ToList().Find(item => item.Name == windowNameArray[0]);
            //if (windowNameArray[0] != "更多操作")
            //{
            //    for (int i = 0; i < WindowsAll.Count; i++)
            //    {
            //        DockWindowDisposable windowTemp = WindowsAll[i];
            //        if (windowTemp.Name == "更多操作")
            //        {
            //            //WindowsAll.Remove(windowTemp);
            //            windowTemp.OnClosed(null);
            //            //windowTemp.Close();
            //            break;
            //        }
            //    }
            //}
            if (windowNow is DockWindowDisposable)
            {
                if (!windowNow.IsSelected && windowNameArray[1] != "ViewLog")
                {
                    LogManager.AddMessage(string.Format("切换到{0}窗口.", windowNameArray[0]));
                    //windowNow.Show();
                }
                //int index = WindowsAll.ToList().FindIndex(item => item.Name == windowNameArray[0]) - 2;
                //if (index > -1) SelectIndex = index;
                SelectIndex = windowNow.Index;
                //SelectItem = windowNow;
                windowNow.IsSelected = true;
                //windowNow.IsAutoHide = false;
                return;
            }
            #endregion

            #region 如果窗体不存在则创建窗体
            string className = string.Format("LYTest.WPF.View.{0}", windowNameArray[1]);
            Assembly assemblyCurrent = Assembly.Load("LYTest.WPF");
            object obj = null;
            if (windowNameArray.Length == 2)
            {
                obj = assemblyCurrent.CreateInstance(className);
            }
            else
            {
                string[] paras = windowNameArray[2].Split(',');
                obj = assemblyCurrent.CreateInstance(className, true, BindingFlags.CreateInstance, null, paras, null, null);
            }
            if (obj is DockControlDisposable dockControl)
            {
                DockWindowDisposable windowNew = UIGeneralClass.CreateDockWindow(dockControl);


                //载入对应的图标
                var viewName = models.FirstOrDefault(item => item.GetProperty("MENU_CLASS") as string == windowNameArray[1]);
                if (viewName != null)
                {
                    string imageName = viewName.GetProperty("MENU_IMAGE") as string;  //获得图片名字
                    windowNew.ImageControl = menuModel.Images.FirstOrDefault(item => item.ImageName == imageName).ImageControl;
                }

                WindowsAll.Add(windowNew);
            }
            else
            {
                LogManager.AddMessage(string.Format("创建{0}窗口操作失败.", windowNameArray[0]), EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
            #endregion
        }


    }
}
