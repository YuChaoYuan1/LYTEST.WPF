using System;

namespace LYTest.Utility
{
    /// <summary>
    /// 程序的任务管理器，负责管理程序内部所有任务
    /// </summary>
    public class TaskManager
    {

        /// <summary>
        /// 设备调用队列
        /// </summary>
        private static readonly ActionQueue deviceQueue = new ActionQueue();
        public static void AddDeviceAction(Action action)
        {
            deviceQueue.AddMessage(action);
        }


        /// <summary>
        /// 调用wcf的线程队列
        /// </summary>
        private static readonly ActionQueue wcfQueue = new ActionQueue();
        /// <summary>
        /// 调用数据库存储的线程队列
        /// </summary>
        private static readonly ActionQueue dataBaseQueue = new ActionQueue();
        /// <summary>
        /// 更新UI显示的线程队列
        /// </summary>
        private static readonly ActionQueue uiQueue = new ActionQueue();
        /// <summary>
        /// 日志保存任务
        /// </summary>
        private static readonly ActionQueue saveLogQueue = new ActionQueue();
        /// <summary>
        /// 添加数据库操作方法
        /// </summary>
        /// <param name="action"></param>
        public static void AddDataBaseAction(Action action)
        {
            dataBaseQueue.AddMessage(action);
        }
        /// <summary>
        /// 添加WebService调用方法
        /// </summary>
        /// <param name="action"></param>
        public static void AddWcfAction(Action action)
        {
            wcfQueue.AddMessage(action);
        }
        /// <summary>
        /// 添加UI显示调用方法
        /// </summary>
        /// <param name="action"></param>
        public static void AddUIAction(Action action)
        {
            uiQueue.AddMessage(action);
        }
        /// <summary>
        /// 将要保存的日志添加到进程队列
        /// </summary>
        /// <param name="action"></param>
        public static void AddSaveLogAction(Action action)
        {
            saveLogQueue.AddMessage(action);
        }
        private static readonly ActionQueue collectionQueue = new ActionQueue();
        public static void AddCollectionQueue(Action action)
        {
            collectionQueue.AddMessage(action);
        }
    }
}
