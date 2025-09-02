using LYTest.Core.Enum;
using LYTest.Core.Model.Meter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LYTest.MeterProtocol.Comm;

namespace LYTest.ViewModel.CheckController.MulitThread
{
    public class MulitEncryptionWorkThreadManager : SingletonBase<MulitThreadManager>
    {

        public MulitEncryptionWorkThreadManager(int ChannelCount)
        {
            WorkThreads = new EncryptionWorkThread[ChannelCount];
        }
        /// <summary>
        /// 本地表为True,远程表为False
        /// </summary>
        public bool IsLocalMeter { get; set; }

        /// <summary>
        /// 通道个数,8
        /// </summary>
        public int ChannelCount { get; set; }

        /// <summary>
        /// 每一个通道对应的个数,6
        /// </summary>
        public int OneChannelMeterCount { get; set; }

        /// <summary>
        /// 当前通道序列
        /// </summary>
        public int CurrentChannelIndex { get; set; }
        /// <summary>
        /// 每个线程允许执行
        /// </summary>
        public bool[] CanExecute { get; set; }
        /// <summary>
        /// 工作线程数组
        /// </summary>
        public EncryptionWorkThread[] WorkThreads { get; set; }

        /// <summary>
        /// 启动线程
        /// </summary>
        /// <returns>启动线程是否成功</returns>
        public bool Start(Cus_EncryptionTrialType trialType)
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                int bw = i * OneChannelMeterCount + CurrentChannelIndex - 1;

                TestMeterInfo meter = VerifyBase.MeterInfo[bw];
                if (meter == null || !meter.YaoJianYn || !CanExecute[bw]) continue;

                if (WorkThreads[bw] == null)
                {
                    WorkThreads[bw] = new EncryptionWorkThread()
                    {
                        //IsLocalMeter = IsLocalMeter,
                        CurrentChannelIndex = CurrentChannelIndex,
                        Meter = meter,
                        CertificationResult = 0,
                        RemoteControlResult = false,
                        ClearEnergyResult = false,
                        UpdateKeyResult = false,
                        CoverKeyResult = false,
                        ClearKeyResult = false,
                    };
                }
                else
                {
                    //WorkThreads[bw].IsLocalMeter = IsLocalMeter;
                    WorkThreads[bw].CurrentChannelIndex = CurrentChannelIndex;
                    WorkThreads[bw].Meter = meter;

                    WorkThreads[bw].RemoteControlResult = false;
                    WorkThreads[bw].ClearEnergyResult = false;
                    WorkThreads[bw].CoverKeyResult = false;
                    WorkThreads[bw].ClearKeyResult = false;
                    if (meter.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        if (WorkThreads[bw].CertificationResult == 0 
                            && trialType != Cus_EncryptionTrialType.身份认证) continue;
                    }
                    if (WorkThreads[bw].CertificationResult == 2 
                        && trialType == Cus_EncryptionTrialType.主控密钥更新) continue;

                    if (WorkThreads[bw].curKeyInfo.KeyVer == 0 
                        && trialType == Cus_EncryptionTrialType.密钥恢复 
                        && meter.DgnProtocol.HaveProgrammingkey) continue;

                    WorkThreads[bw].RemoteControlResult = false;
                    WorkThreads[bw].ClearEnergyResult = false;
                    WorkThreads[bw].CoverKeyResult = false;
                    WorkThreads[bw].ClearKeyResult = false;
                }
                WorkThreads[bw].Start(trialType);

                if (trialType == Cus_EncryptionTrialType.密钥更新 || trialType == Cus_EncryptionTrialType.密钥恢复)
                    Thread.Sleep(10);//750
                else
                    Thread.Sleep(10);//200
            }
            return true;
        }

        /// <summary>
        /// 停止所有工作线程
        /// </summary>
        public void Stop()
        {
            //首先发出停止指令
            foreach (EncryptionWorkThread workthread in WorkThreads)
            {
                if (workthread != null)
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
            foreach (EncryptionWorkThread workthread in WorkThreads)
            {
                if (workthread == null) continue;

                bool finish = workthread.IsWorkFinish();
                if (!finish) return false;
            }

            return true;
        }
    }
}
