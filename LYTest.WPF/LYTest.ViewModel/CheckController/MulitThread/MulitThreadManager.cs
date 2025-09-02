using LYTest.MeterProtocol.Comm;
using System;

namespace LYTest.ViewModel.CheckController.MulitThread
{
    public class MulitThreadManager : SingletonBase<MulitThreadManager>
    {
        /// <summary>
        /// 最大线程数量
        /// </summary>
        public int MaxThread { get; set; }

        /// <summary>
        /// 每个线程最大任务数
        /// </summary>
        public int MaxTaskCountPerThread { get; set; }

        /// <summary>
        /// 工作线程数组
        /// </summary>
        private WorkThread[] workThreads = new WorkThread[0];

        public Action<int> DoWork
        {
            private get;
            set;
        }
        /// <summary>
        /// 启动线程
        /// </summary>
        /// <returns>启动线程是否成功</returns>
        public bool Start()
        {
            //结束上一次的线程
            workThreads = new WorkThread[MaxThread];
            for (int i = 0; i < MaxThread; i++)
            {
                if (!VerifyBase.MeterInfo[i].YaoJianYn) continue;
                WorkThread newThread = new WorkThread()
                {
                    ThreadID = i + 1,                      //线程编号,用于线程自己推导起始位置
                    TaskCount = MaxTaskCountPerThread,
                    DoWork = this.DoWork
                };
                workThreads[i] = newThread;
                newThread.Start();
                //modify yjt zxg 20220425 时间改为30
                //System.Threading.Thread.Sleep(100);
                //modify yjt zxg 20220425 时间改为30
                //System.Threading.Thread.Sleep(30);
            }
            return true;
        }

        ///// <summary>
        ///// 启动线程
        ///// </summary>
        ///// <param name="SleepTime">线程等待时间：单位ms</param>
        ///// <returns>启动线程是否成功</returns>
        //public bool Start(int SleepTime)
        //{
        //    //结束上一次的线程
        //    workThreads = new WorkThread[MaxThread];
        //    for (int i = 0; i < MaxThread; i++)
        //    {
        //        if (!VerifyBase.MeterInfo[i].YaoJianYn) continue;
        //        WorkThread newThread = new WorkThread()
        //        {
        //            ThreadID = i + 1,                      //线程编号,用于线程自己推导起始位置
        //            TaskCount = MaxTaskCountPerThread,
        //            DoWork = this.DoWork
        //        };
        //        workThreads[i] = newThread;
        //        newThread.Start();
        //        System.Threading.Thread.Sleep(SleepTime);
        //    }
        //    return true;
        //}

        /// <summary>
        /// 停止所有工作线程
        /// </summary>
        public void Stop()
        {
            //首先发出停止指令
            foreach (WorkThread workthread in workThreads)
            {
                workthread.Stop();
            }
            //等待所有工作线程都完成
            bool isAllThreaWorkDone = false;
            while (!isAllThreaWorkDone)
            {
                isAllThreaWorkDone = IsWorkDone();
            }

        }

        /// <summary>
        /// 等待所有线程工作完成
        /// </summary>
        public bool IsWorkDone()
        {
            bool isAllThreaWorkDone = true;

            foreach (WorkThread workthread in workThreads)
            {
                if (workthread == null)
                    continue;
                isAllThreaWorkDone = workthread.IsWorkFinish();
                if (!isAllThreaWorkDone) break;
            }
            if (isAllThreaWorkDone)
            {
                //Comm.MessageController.Instance.AddMessage("当前操作已经完成!", false);
                //所有操作完成后，关闭一下485端口，如果不关，标准表读取线程将无法读取标准表
                //bool[] isOpen = new bool[VerifyBase.meterInfo.Length];
                //Helper.EquipHelper.Instance.InitPara_CommTest(isOpen); 
            }
            return isAllThreaWorkDone;
        }
    }
}
