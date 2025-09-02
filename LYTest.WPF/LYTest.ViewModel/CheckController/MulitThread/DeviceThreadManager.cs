using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LYTest.MeterProtocol.Comm;

namespace LYTest.ViewModel.CheckController.MulitThread
{
    /// <summary>
    /// 设备控制线程
    /// </summary>
    public class DeviceThreadManager : SingletonBase<DeviceThreadManager>
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
        private WorkThread2[] workThreads = new WorkThread2[0];

        public Action<int,int> DoWork
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
            workThreads = new WorkThread2[MaxThread];
            for (int i = 0; i < MaxThread; i++)
            {
                WorkThread2 newThread = new WorkThread2()
                {
                    //EquipmentData.DeviceManager.Devices
                    ThreadID = i,       //线程编号,用于线程自己推导起始位置
                    TaskCount = MaxTaskCountPerThread,
                    DoWork = this.DoWork
                };
                workThreads[i] = newThread;
                newThread.Start();
                //System.Threading.Thread.Sleep(400);

            }
            return true;
        }

        /// <summary>
        /// 启动线程
        /// </summary>
        /// <param name="SleepTime">线程等待时间：单位ms</param>
        /// <returns>启动线程是否成功</returns>
        public bool Start(int SleepTime)
        {
            //结束上一次的线程

            workThreads = new WorkThread2[MaxThread];
            for (int i = 0; i < MaxThread; i++)
            {
                WorkThread2 newThread = new WorkThread2()
                {
                    ThreadID = i ,                      //线程编号,用于线程自己推导起始位置
                    TaskCount = MaxTaskCountPerThread,
                    DoWork = this.DoWork
                };
                workThreads[i] = newThread;
                newThread.Start();
                System.Threading.Thread.Sleep(SleepTime);
            }
            return true;
        }

        /// <summary>
        /// 停止所有工作线程
        /// </summary>
        public void Stop()
        {
            //首先发出停止指令
            foreach (WorkThread2 workthread in workThreads)
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

            foreach (WorkThread2 workthread in workThreads)
            {
                if (workthread == null)
                    continue;
                isAllThreaWorkDone = workthread.IsWorkFinish();
                if (!isAllThreaWorkDone) break;
            }
            //if (isAllThreaWorkDone)
            //{
                //Comm.MessageController.Instance.AddMessage("当前操作已经完成!", false);
                //所有操作完成后，关闭一下485端口，如果不关，标准表读取线程将无法读取标准表
                //bool[] isOpen = new bool[VerifyBase.MeterInfo.Length];
                //Helper.EquipHelper.Instance.InitPara_CommTest(isOpen); 
            //}
            return isAllThreaWorkDone;
        }
    }

    class WorkThread2
    {
        Thread workThread = null;

        /// <summary>
        /// 运行标志
        /// </summary>
        private bool runFlag = false;

        /// <summary>
        /// 工作完成标志
        /// </summary>
        private bool workOverFlag = false;

        /// <summary>
        /// 线程编号
        /// </summary>
        public int ThreadID { get; set; }
        /// <summary>
        /// 任务数量
        /// </summary>
        public int TaskCount { get; set; }

        public Action<int,int> DoWork { get; set; }

        /// <summary>
        /// 停止当前工作任务
        /// </summary>
        public void Stop()
        {
            runFlag = true;
        }

        /// <summary>
        /// 工作线程是否完成
        /// </summary>
        /// <returns></returns>
        public bool IsWorkFinish()
        {
            return workOverFlag;
        }

        /// <summary>
        /// 启动工作线程
        /// </summary>
        /// <param name="paras"></param>
        public void Start()
        {
            workThread = new Thread(StartWork);
            workThread.Start();
        }

        private void StartWork()
        {
            //初始化标志
            runFlag = true;
            workOverFlag = false;
            //计算负载
            int startpos =ThreadID * TaskCount;
            int endpos = startpos + TaskCount;
            //调用方法
            try
            {
                for (int i = startpos; i < endpos; i++)
                {
                    if (DoWork != null)
                    {
                        DateTime startTime = DateTime.Now;
                        bool openRet = true;
                        if (openRet)
                        {
                            DoWork(ThreadID,i);
                        }
                    }
                    if (!runFlag)
                        break;
                }
            }
            catch { }
            finally
            {
                //恢复标志
                runFlag = false;
                workOverFlag = true;
            }
        }


    }
}
