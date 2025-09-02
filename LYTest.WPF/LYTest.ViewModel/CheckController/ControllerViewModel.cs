using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.DAL.Config;
using LYTest.Utility.Log;
using LYTest.ViewModel.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LYTest.ViewModel.CheckController
{
    /// 检定控制器视图
    /// <summary>
    /// 检定控制器视图
    /// </summary>
    public class ControllerViewModel : ViewModelBase
    {
        private bool everyStartTestIdent = true;
        /// <summary>
        /// 每次开始检定时必认证一次
        /// </summary>
        public bool EveryStartTestIdent
        {
            get { return everyStartTestIdent; }
            set { everyStartTestIdent = value; }
        }

        #region 一些信息
        private bool isVisibleStdMessage = true;
        /// <summary>
        ///是否显示标准表信息
        /// </summary>
        public bool IsVisibleStdMessage
        {
            get { return isVisibleStdMessage; }
            set { SetPropertyValue(value, ref isVisibleStdMessage, "IsVisibleStdMessage"); }
        }
        private bool isVisibleStdMessages = true;
        /// <summary>
        ///是否显示标准表信息
        /// </summary>
        public bool IsVisibleStdMessages
        {
            get { return isVisibleStdMessages; }
            set { SetPropertyValue(value, ref isVisibleStdMessages, "IsVisibleStdMessages"); }
        }
        private bool isVisibleLogMessage = true;
        /// <summary>
        ///是否显示日志信息
        /// </summary>
        public bool IsVisibleLogMessage
        {
            get { return isVisibleLogMessage; }
            set { SetPropertyValue(value, ref isVisibleLogMessage, "IsVisibleLogMessage"); }
        }
        #endregion

        /// <summary>
        /// 是否显示检定悬浮按钮
        /// </summary>
        //public bool IsTestMainVisible { get; set; }
        private bool isTestMainVisible = true;
        /// <summary>
        /// 检定提示信息
        /// </summary>
        public bool IsTestMainVisible
        {
            get { return isTestMainVisible; }
            set { SetPropertyValue(value, ref isTestMainVisible, "IsTestMainVisible"); }
        }



        //public int DeviceStart=0;//
        private int deviceStart;
        /// <summary>
        /// 台体状态 0-空闲； 1-检测中； 2-暂停；3-停止；4-完成； 5-关机中；8-不合格率报警9-异常
        /// </summary>
        public int DeviceStart
        {
            get { return deviceStart; }
            set
            {
                deviceStart = value;
                if (deviceStart >= 0)
                {
                    EquipmentData.CallMsg("DeviceState"); //发生改变，通知主控
                    DeviceStart = -1;
                }
            }
        }

        /// <summary>
        /// 允许检定
        /// </summary>
        public bool IsEnable
        {
            get
            {
                // return true;
                return index >= 0 && EquipmentData.DeviceManager.IsReady;
            }
        }

        private int index;
        /// <summary>
        /// 当前检定点序号,逻辑里面的核心
        /// </summary>
        public int Index
        {
            get
            {
                if (index >= CheckCount)
                {
                    index = CheckCount - 1;
                }
                return index;
            }
            set
            {
                //将执行完毕的检定点设置为非检定状态
                if (EquipmentData.CheckResults.ResultCollection.Count > Index && Index >= 0)
                {
                    EquipmentData.CheckResults.ResultCollection[Index].IsCurrent = false;
                    EquipmentData.CheckResults.ResultCollection[Index].IsChecking = false;
                }
                SetPropertyValue(value, ref index, nameof(StringCheckIndex));
                if (EquipmentData.CheckResults.ResultCollection.Count > Index && Index >= 0)
                {
                    EquipmentData.CheckResults.ResultCollection[Index].IsCurrent = true;
                    EquipmentData.CheckResults.CheckNodeCurrent = EquipmentData.CheckResults.ResultCollection[Index];
                }
                OnPropertyChanged(nameof(CheckCount));
                #region 加载检定参数
                if (Index >= 0 && Index < EquipmentData.Schema.ParaValues.Count)
                {
                    DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[Index];
                    EquipmentData.Schema.ParaNo = viewModel.GetProperty("PARA_NO") as string;
                    var temp = from item in EquipmentData.Schema.ParaInfo.CheckParas select item.ParaDisplayName;
                    if (!(viewModel.GetProperty("PARA_VALUE") is string paraValue))
                    {
                        paraValue = "";
                    }
                    string[] tempValues = paraValue.Split('|');
                    List<string> listTemp = new List<string>();
                    for (int i = 0; i < temp.Count(); i++)
                    {
                        if (tempValues.Length > i)
                        {
                            listTemp.Add($"{temp.ElementAt(i)}:{tempValues[i]}");
                        }
                    }
                    if (listTemp.Count > 0)
                    {
                        StringPara = "参数:" + string.Join(",", listTemp);
                    }
                    else
                    {
                        StringPara = "无参数";
                    }

                    CheckingName = EquipmentData.CheckResults.ResultCollection[Index].Name;
                }
                #endregion
                OnPropertyChanged(nameof(IsEnable));
                EquipmentData.LastCheckInfo.SaveCurrentCheckInfo();
                #region 时间统计
                if (index >= 0 && index < TimeMonitor.Instance.TimeCollection.ItemsSource.Count)
                {
                    TimeMonitor.Instance.TimeCollection.SelectedItem = TimeMonitor.Instance.TimeCollection.ItemsSource[index];
                }
                #endregion
            }
        }

        /// 检定点数量
        /// <summary>
        /// 检定点数量
        /// </summary>
        public int CheckCount
        {
            get
            {
                return EquipmentData.CheckResults.ResultCollection.Count;
            }
        }
        /// 检定点序号字符串
        /// <summary>
        /// 检定点序号字符串
        /// </summary>
        public string StringCheckIndex
        {
            get
            {
                if (Index == -1)
                {
                    return "参数录入";
                }
                else if (Index == -3)
                {
                    return "审核存盘";
                }
                return string.Format("({0}/{1})", index + 1, CheckCount);
            }
        }
        /// 当前检定点编号
        /// <summary>
        /// 当前检定点编号
        /// </summary>
        public string CurrentKey
        {
            get
            {
                if (Index >= 0 && Index < EquipmentData.Schema.ParaValues.Count)
                {
                    DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[Index];
                    EquipmentData.Schema.ParaNo = viewModel.GetProperty("PARA_NO") as string;
                    return viewModel.GetProperty("PARA_KEY") as string;
                }
                else
                {
                    return "";
                }
            }
        }

        /// 手动结束检定标记
        /// <summary>
        /// 手动结束检定标记
        /// </summary>
        private bool flagHandStop = false;

        private EnumCheckMode checkMode = EnumCheckMode.连续模式;
        /// 检定模式
        /// <summary>
        /// 检定模式
        /// </summary>
        public EnumCheckMode CheckMode
        {
            get { return checkMode; }
            set { SetPropertyValue(value, ref checkMode, "CheckMode"); }
        }
        private string messageTips = "";
        /// <summary>
        /// 检定提示信息
        /// </summary>
        public string MessageTips
        {
            get { return messageTips; }
            set { SetPropertyValue(value, ref messageTips, "MessageTips"); }
        }
        /// <summary>
        /// 提示信息-日志
        /// </summary>
        /// <param name="Tips">内容</param>
        /// <param name="Error">True故障，false正常</param>
        public void MessageAdd(string Tips, EnumLogType logType)
        {
            switch (logType)
            {
                case EnumLogType.错误信息://必须全部显示
                    LogManager.AddMessage(Tips, EnumLogSource.检定业务日志, EnumLevel.Error);
                    break;
                case EnumLogType.提示信息://提示日志打开才会保存，否是只是显示在下面
                    EquipmentData.Controller.MessageTips = Tips.Replace("\r\n", "");
                    if (ConfigHelper.Instance.IsOpenLog_Tips)
                    {
                        LogManager.AddMessage(Tips, EnumLogSource.检定业务日志, EnumLevel.Information);
                    }
                    break;
                case EnumLogType.详细信息:
                    if (ConfigHelper.Instance.IsOpenLog_Detailed)
                    {
                        LogManager.AddMessage(Tips, EnumLogSource.检定业务日志, EnumLevel.Information);
                    }
                    break;
                case EnumLogType.流程信息:
                    LogManager.AddMessage(Tips, EnumLogSource.检定业务日志, EnumLevel.Information);
                    break;
                case EnumLogType.提示与流程信息:
                    LogManager.AddMessage(Tips, EnumLogSource.检定业务日志, EnumLevel.Information);
                    EquipmentData.Controller.MessageTips = Tips.Replace("\r\n", "");
                    break;
                default:
                    break;
            }
        }


        private bool isChecking;
        /// <summary>
        /// 是否正在检定
        /// </summary>
        public bool IsChecking
        {
            get { return isChecking; }
            private set
            {
                if (value != isChecking)
                {
                    isChecking = value;
                    if (isChecking && !isBusy)
                    {
                        Task.Factory.StartNew(() => VerifyProcess());
                    }
                    else
                    {
                        EquipmentData.CheckResults.ResultCollection[Index].IsChecking = false;
                        LogManager.AddMessage("停止检定!", EnumLogSource.检定业务日志, EnumLevel.Information);
                        currentStepWaitHandle.Set();
                    }
                }
                OnPropertyChanged("IsChecking");
            }
        }

        private bool newArrived;
        /// 新消息到来
        /// <summary>
        /// 新消息到来
        /// </summary>
        public bool NewArrived
        {
            get { return newArrived; }
            set
            {
                SetPropertyValue(value, ref newArrived, "NewArrived");
            }
        }

        #region 检定过程控制
        /// <summary>
        /// 异常停止检定
        /// </summary>
        public void TryStopVerify()
        {
            flagHandStop = true;
            //手动停止，时间计数无效
            TimeMonitor.Instance.ActiveCurrentItem(Index);
            TimeMonitor.Instance.FinishCurrentItem(Index);
            if (temVerify != null)
            {
                temVerify.Stop = true;//停止检定
            }
            if (temVerify2 != null)
            {
                temVerify2.Stop = true;//停止检定
            }
            LogManager.AddMessage("达到设定的停止条件,正在停止检定台...", EnumLogSource.检定业务日志, EnumLevel.Warning);
            if (EquipmentData.DeviceManager.Devices.ContainsKey(Device.DeviceName.多功能板))
            {
                EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 1);
                InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.故障, "异常停止检定");
            }
        }

        /// <summary>
        /// 停止检定
        /// </summary>
        public void StopVerify()
        {

            //DeviceStart = 3;
            flagHandStop = true;
            //手动停止，时间计数无效
            TimeMonitor.Instance.ActiveCurrentItem(Index);
            TimeMonitor.Instance.FinishCurrentItem(Index);

            if (temVerify != null)
            {
                temVerify.Stop = true;//停止检定
            }
            if (temVerify2 != null)
            {
                temVerify2.Stop = true;//停止检定
            }
            IsMeterDebug = false;
            LogManager.AddMessage("停止检定台...", EnumLogSource.检定业务日志);
            InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.空闲, "停止检定");
        }
        /// 单步检定
        /// <summary>
        /// 单步检定
        /// </summary>
        public void StepVerify()
        {
            CheckMode = EnumCheckMode.单步模式;
            IsChecking = true;
        }
        /// 单组检定
        /// <summary>
        /// 单组检定
        /// </summary>
        public void SingleSetVerify()
        {
            CheckMode = EnumCheckMode.单组模式;
            IsChecking = true;
        }

        /// <summary>
        /// 调表
        /// </summary>
        public void MeterDebug()
        {
            IsMeterDebug = !IsMeterDebug;
        }

        public bool isMeterDebug = false;
        /// <summary>
        /// 是否调表
        /// </summary>
        public bool IsMeterDebug
        {
            get { return isMeterDebug; }
            set
            {
                VerifyBase.IsMeterDebug = value;
                SetPropertyValue(value, ref isMeterDebug, "IsMeterDebug");
            }
        }
        /// 连续检定
        /// <summary>
        /// 连续检定
        /// </summary>
        public void RunningVerify()
        {
            CheckMode = EnumCheckMode.连续模式;
            IsChecking = true;
        }
        /// 循环检定
        /// <summary>
        /// 循环检定
        /// </summary>
        public void LoopVerify()
        {
            IsChecking = true;
            CheckMode = EnumCheckMode.循环模式;
        }
        /// 当前检定项检定完毕
        /// <summary>
        /// 当前检定项检定完毕
        /// </summary>
        public void FinishCurrentStep()
        {
            string checkName = EquipmentData.CheckResults.ResultCollection[Index].Name;
            LogManager.AddMessage(string.Format("项目： {0}  检定完成.", checkName), EnumLogSource.检定业务日志);
            if (isBusy)
            {
                currentStepWaitHandle.Set();
            }
            else
            {
                IsChecking = false;
            }
        }
        #endregion

        #region 检定线程
        /// <summary>
        /// 更新电能表协议信息
        /// </summary>
        public void UpdateMeterProtocol()
        {

            MeterHelper.Instance.InitCarrier();//初始化一下载波协议

            MeterHelper.Instance.Init();
            //把数据库中读取到的串口数据给到类里面
            DgnProtocolInfo[] protocols = MeterHelper.Instance.GetAllProtocols();
            //VerifyBase.meterInfo[0].DgnProtocol = protocols[0];
            string[] meterAddress = MeterHelper.Instance.GetMeterAddress();
            ComPortInfo[] comPorts = MeterHelper.Instance.GetComPortInfo();
            MeterProtocolAdapter.Instance.Initialize(protocols, meterAddress, comPorts);
        }

        private static bool CheckAutoUpdate = false;
        private readonly AutoResetEvent currentStepWaitHandle = new AutoResetEvent(false);
        /// <summary>
        /// 检定执行过程
        /// </summary>
        private void VerifyProcess()   //【标注--开始检定】
        {
            isBusy = true;//正在忙碌
            InnerCommand.VerifyControl.SendMsg($"开始检定({CheckMode})。");
            InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.正在运行, "");
            IsCheclProtocol = false;
            IsMeterDebug = false;
            TestStratSet();

            //Thread.Sleep(1500);

            //for (int i = 0; i <EquipmentData.CheckResults.ResultCollection.Count ; i++)
            //{
            //    EquipmentData.CheckResults.ResultCollection[i].IsChecked = false;
            //}
            if (CheckMode != EnumCheckMode.连续模式) //单步检定和循环检定的时候可以重复检定
            {
                if (Index >= 0 || Index < CheckCount)
                {
                    EquipmentData.CheckResults.ResultCollection[Index].IsChecked = false;
                }
            }
            while (IsChecking)
            {
                try
                {

                    if (Index < 0 || Index >= CheckCount)
                    {
                        IsChecking = false;
                        break;
                    }

                    VerifyBase.Progress = $"{Index}/{CheckCount}";

                    if (EquipmentData.CheckResults.ResultCollection[Index].IsSelected && !EquipmentData.CheckResults.ResultCollection[Index].IsChecked)
                    {
                        InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.检定进度, VerifyBase.Progress);
                        EquipmentData.CheckResults.ResultCollection[Index].ItemTime = "";

                        Stopwatch sw = Stopwatch.StartNew();
                        if (!InvokeStartVerify()) //开始检定，根据类名调用检定方法
                        {
                            sw.Stop();
                            break;
                        }

                        #region 等待当前检定项结束
                        currentStepWaitHandle.Reset();
                        currentStepWaitHandle.WaitOne();
                        #endregion

                        sw.Stop();
                        EquipmentData.CheckResults.ResultCollection[Index].ItemTime = sw.Elapsed.TotalMinutes.ToString("F2");

                    }
                    else
                    {
                        if (CheckMode == EnumCheckMode.单步模式)
                        {
                            LogManager.AddMessage("当前试验项没有勾选!", EnumLogSource.用户操作日志, EnumLevel.Warning);
                        }
                    }

                    #region 检定器将要执行的动作
                    //如果手动终止
                    if (flagHandStop)
                    {
                        LogManager.AddMessage("当前检定项被手动终止!", EnumLogSource.检定业务日志);
                        flagHandStop = false;
                        IsChecking = false;
                        EquipmentData.CheckResults.ResultCollection[Index].IsChecked = false;

                        if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                        {
                            IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                            open.WorkingStatus("02", "04");
                            open.EventEscalation("0120", EquipmentData.CheckResults.ResultCollection[Index].ItemKey);
                        }
                        break;
                    }
                    //统计检定项的时间
                    TimeMonitor.Instance.FinishCurrentItem(Index);
                    CheckAutoUpdate = false;
                    //根据检定模式判断将要执行的检定动作
                    switch (CheckMode)
                    {
                        case EnumCheckMode.单步模式:
                            IsChecking = false;

                            if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                            {
                                IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                                open.WorkingStatus("02", "04");
                                open.EventEscalation("0120", EquipmentData.CheckResults.ResultCollection[Index].ItemKey);
                            }
                            break;
                        case EnumCheckMode.单组模式:
                            if (IsChecking && Index < CheckCount - 1 && Index >= 0)
                            {
                                if (EquipmentData.CheckResults.ResultCollection[Index].ItemKey.Split('_')[0] == EquipmentData.CheckResults.ResultCollection[Index + 1].ItemKey.Split('_')[0])
                                {
                                    Index += 1;
                                }
                                else
                                {
                                    IsChecking = false;
                                    if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                                    {
                                        IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                                        open.WorkingStatus("02", "04");
                                        open.EventEscalation("0120", EquipmentData.CheckResults.ResultCollection[Index].ItemKey);
                                    }
                                }
                            }
                            else
                            {
                                IsChecking = false;
                            }
                            break;
                        case EnumCheckMode.连续模式:
                            //判断是否要执行下一个检定项
                            if (IsChecking && Index < CheckCount - 1 && Index >= 0)
                            {
                                //如果当前点是最后一个检定点
                                Index += 1;
                            }
                            else
                            {
                                IsChecking = false;

                                CheckAutoUpdate = true;

                            }
                            break;
                        case EnumCheckMode.循环模式:
                            //进入下一个循环
                            EquipmentData.CheckResults.ResultCollection[Index].IsChecked = false;
                            break;
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    LogManager.AddMessage(string.Format("调用检定开始服务异常:{0}", e.Message), EnumLogSource.检定业务日志, EnumLevel.Error);
                    IsChecking = false;
                    //DeviceStart = 9;
                }
            }
            isBusy = false;
            TestEndSet();
            InnerCommand.VerifyControl.SendMsg("停止检定。");
        }

        /// <summary>
        /// 检定开始前的设置
        /// </summary>
        private void TestStratSet()
        {
            VerifyConfig.UpdateKeyAndUpdateData = false;
            EveryStartTestIdent = true;
            //add zxg yjt 20220426 新增
            //每次开始检定,设置为需要，因为手工台不确定是否换表了
            VerifyBase.ReadMeterAddressAndNo = true;
            UpdateMeterProtocol();//每次开始检定，更新一下电表协议
            //add yjt 20220805 新增判断是连续模式还是单步模式
            if (CheckMode == EnumCheckMode.连续模式)
            {
                VerifyBase.IsSwitch_I = true;
            }
            else
            {
                VerifyBase.IsSwitch_I = false;
            }
            //控制表位继电器
            if (!EquipmentData.Equipment.IsDemo)
            {
                if (EquipmentData.DeviceManager.Devices.ContainsKey(Device.DeviceName.多功能板))
                {
                    //检定时绿
                    EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.绿, 1);
                    //检定时根据结论绿或红
                    //EquipmentData.DeviceManager.SetEquipmentThreeColor(MeterHelper.Instance.GetResult() ? EmLightColor.绿 : EmLightColor.红, 1);
                    if (EquipmentData.Equipment.EquipmentType != "单相台")
                        EquipmentData.DeviceManager.SetEquipmentPowerSupply(0, true);
                    else
                        EquipmentData.DeviceManager.SetEquipmentPowerSupply(0, false);

                }
                if (EquipmentData.DeviceManager.Devices.ContainsKey(Device.DeviceName.时基源))
                    EquipmentData.DeviceManager.SetTimePulse(true);

                #region 翻转电机
                if (ConfigHelper.Instance.Is_Hgq_AutoCut)//自动切换互感器
                {
                    if (VerifyBase.OneMeterInfo.MD_ConnectionFlag == "直接式")
                        EquipmentData.DeviceManager.Hgq_Set(1);
                    else
                        EquipmentData.DeviceManager.Hgq_Set(0);
                    int times = 5;
                    while (true)
                    {
                        if (times < 0)
                            break;
                        MessageAdd($"正在切换到{VerifyBase.OneMeterInfo.MD_ConnectionFlag}，等待{times}秒...", EnumLogType.提示信息);
                        Thread.Sleep(1000);
                        times--;
                    }
                }
                #endregion

                #region 表位下压电机

                //加上判断是自动模式并且自动压接表位的情况，下压电机，等待8秒
                if (ConfigHelper.Instance.IsMete_Press)
                {
                    MessageAdd("正在电机下行...", EnumLogType.提示信息);

                    EquipmentData.DeviceManager.E_M_Down();

                    int times = ConfigHelper.Instance.Mete_Press_Time;
                    while (true)
                    {
                        if (times < 0) break;
                        MessageAdd($"电机下行，等待{times}秒...", EnumLogType.提示信息);
                        Thread.Sleep(1000);
                        times--;
                    }

                }
                #endregion

                #region 表位继电器
                if (ConfigHelper.Instance.HasCurrentSwitch)
                {
                    MessageAdd($"正在设置表位继电器...", EnumLogType.提示信息);
                    EquipmentData.DeviceManager.ControlYaoJianPositions();
                }
                if (ConfigHelper.Instance.HasVoltageSwitch)
                {
                    MessageAdd($"正在设置电压继电器...", EnumLogType.提示信息);
                    EquipmentData.DeviceManager.ControlULoadPositions();
                }

                if (EquipmentData.Equipment.EquipmentType != "单相台")
                {
                    //三相直接和互感器切换，用第127表位控制继电器//20231115以后不用
                    if (VerifyBase.HGQ)
                    {
                        EquipmentData.DeviceManager.ControlHGQRelay(true);
                        EquipmentData.DeviceManager.ControlMeterRelay(1, Convert.ToByte(127), 0);  //继电器切换到经互感器
                    }
                    else
                    {
                        EquipmentData.DeviceManager.ControlHGQRelay(false);
                        EquipmentData.DeviceManager.ControlMeterRelay(2, Convert.ToByte(127), 0);  //继电器切换到直接接入
                    }
                    Thread.Sleep(50);
                }
                #endregion

                #region 标准表切回自动挡位
                ulong conststd = 0;//自动常数，每次检定前发送切回自动档
                if (ConfigHelper.Instance.FixedConstant)
                {
                    conststd = 1000000;//固定常数
                }
                double[] d = new double[6] { 0, 0, 0, 0, 0, 0 };
                EquipmentData.DeviceManager.StdGear(0x13, ref conststd, ref d);
                #endregion

                #region 物联表处理
                if (ConfigHelper.Instance.IsITOMeter)
                {
                    VerifyBase.ItoControlType = -1;//第一次必须切换
                    //VerifyBase.IsCheckITOMeter = true;
                    //需要升源
                    if (!EquipmentData.DeviceManager.Devices.ContainsKey(Device.DeviceName.功率源)) return;

                    string jxfs = EquipmentData.MeterGroupInfo.Meters[0].GetProperty("MD_WIRING_MODE").ToString();
                    int jxfsvalue = 5;
                    if (jxfs == "三相四线") jxfsvalue = 0;
                    else if (jxfs == "三相三线") jxfsvalue = 1;
                    if (!float.TryParse(EquipmentData.MeterGroupInfo.Meters[0].GetProperty("MD_UB").ToString(), out float Ub))
                        Ub = 57.7f;
                    EquipmentData.DeviceManager.PowerOn(jxfsvalue, Ub, Ub, Ub, 0, 0, 0, 0, 240, 120, 0, 240, 120, 50, 1);
                    int waittime = 15;
                    while (true)
                    {
                        if (waittime < 0 || flagHandStop)
                            break;
                        MessageAdd($"正在升源,等待{waittime}秒...", EnumLogType.提示信息);
                        Thread.Sleep(1000);
                        waittime--;
                    }

                    ////模块初始化
                    ////蓝牙连接
                    ////判断预处理状态
                    ////是否进行预处理
                    ////
                    MessageAdd("正在复位蓝牙光电模块...", EnumLogType.提示信息);
                    bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Reset();
                    bool all = true;
                    int MeterNumber = EquipmentData.Equipment.MeterCount;

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;

                        if (!resoult[i]) all = false;
                    }
                    if (!all)
                    {
                        bool[] reTrys = new bool[MeterNumber];
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;
                            if (!resoult[i]) reTrys[i] = true;
                        }
                        MeterProtocolAdapter.Instance.IOTMete_Reset(reTrys);
                    }

                    MessageAdd("正在蓝牙连接物联表...", EnumLogType.提示信息);
                    //这里还需要获取地址来连接蓝牙表
                    string[] address = new string[EquipmentData.MeterGroupInfo.Meters.Count];
                    for (int i = 0; i < EquipmentData.MeterGroupInfo.Meters.Count; i++)
                    {
                        address[i] = EquipmentData.MeterGroupInfo.Meters[i].GetProperty("MD_POSTAL_ADDRESS") as string; ;
                    }

                    bool[] resoult3 = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
                    all = true;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;
                        if (!resoult3[i]) all = false;
                    }
                    if (!all)
                    {
                        bool[] reTrys = new bool[MeterNumber];
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;
                            if (!resoult3[i]) reTrys[i] = true;
                        }
                        bool[] resoulttmp = MeterProtocolAdapter.Instance.IOTMete_Connect(address, reTrys, ConfigHelper.Instance.Bluetooth_Ping);
                        string err = "";
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!EquipmentData.MeterGroupInfo.YaoJian[i]) continue;
                            if (resoulttmp[i]) resoult3[i] = true;
                            if (resoult3[i] == false)
                            {
                                err += $"【{i + 1}】";
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(err))
                            MessageAdd($"表位：{err}蓝牙连接电表失败", EnumLogType.错误信息);
                    }


                    //这里没办法判断预处理状态-所以进行一次预处理
                    //TODO 如果这里预处理失败怎么办
                    //resoult = MeterProtocolAdapter.Instance.IOTMete_Pretreatment();
                    //waittime = 35;
                    //while (true)
                    //{
                    //    if (waittime < 0 || flagHandStop)
                    //        break;
                    //    MessageAdd($"正在进行蓝牙预处理,等待{waittime}秒...", EnumLogType.提示信息);
                    //    Thread.Sleep(1000);
                    //    waittime--;
                    //}

                }
                #endregion
            }
        }


        /// <summary>
        /// 检定结束后的设置
        /// </summary>
        private void TestEndSet()
        {
            //if (ConfigHelper.Instance.IsITOMeter)   //需要关闭检定模式
            //{
            //    byte Power = (byte)((BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_MeterTransmitPower));
            //    //表频段
            //    byte Frequency = (byte)((BluetoothModule_TableFrequencyBand)Enum.Parse(typeof(BluetoothModule_TableFrequencyBand), ConfigHelper.Instance.Bluetooth_MeterFrequencyBand));
            //    //表通道数量
            //    byte count = (byte)ConfigHelper.Instance.Bluetooth_MeterPassCount;
            //    MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel(0xff, Power, Frequency, count);
            //}

            //TODO:配置是否关源
            if (ConfigHelper.Instance.VerifyModel == "自动模式")
            {
                VerifyBase.DeviceControl.PowerOff();  //连续停止检定后关源 
                VerifyBase.DeviceControl.PowerOff();
                LogManager.AddMessage("关源。", EnumLogSource.检定业务日志);
            }
            else
            {
                if (VerifyConfig.Test_Finished_OffPower || CheckMode == EnumCheckMode.连续模式)
                {
                    VerifyBase.DeviceControl.PowerOff();  //连续停止检定后关源 
                    VerifyBase.DeviceControl.PowerOff();
                }
                else
                    EquipmentData.DeviceManager.OnVoltage();
            }

            //所有项目检测完成
            if (CheckAutoUpdate && ConfigHelper.Instance.VerifyModel == "自动模式")
            {
                CheckAutoUpdate = false;
                //bool allchecked = true;
                for (int i = 0; i < CheckCount; i++)
                {
                    if (!EquipmentData.CheckResults.ResultCollection[i].IsChecked)
                    {
                        //allchecked = false;
                        break;
                    }
                }
                //bool allPassed = true;

                for (int i = 0; i < CheckCount; i++)
                {
                    if (!EquipmentData.CheckResults.ResultCollection[i].TestPass)
                    {
                        //allPassed = false;
                        break;
                    }
                }
                //(allchecked || allPassed) && 
                float failedRate = 100 - EquipmentData.CheckResults.GetPassRate();
                if ((failedRate == 0 || failedRate < VerifyConfig.FailureRate)
                    && VerifyConfig.UpdateKeyAndUpdateData)

                {
                    //全部检定完成，判断是否要自动上传数据
                    EquipmentData.CallMsg("VerifyCompelate");
                }
                else
                {
                    string errmsg = "";
                    //if (!(allchecked || allPassed))
                    //{
                    //    errmsg += "存在要检表位没有检完所有项目！";
                    //}
                    if (!(failedRate < VerifyConfig.FailureRate))
                    {
                        errmsg += $"不合格率{failedRate}！(配置值{VerifyConfig.FailureRate}%)";
                    }

                    if (!VerifyConfig.UpdateKeyAndUpdateData)
                    {
                        errmsg += "没有密钥更新！";
                    }

                    LogManager.AddMessage($"停止上传：{errmsg}。", EnumLogSource.检定业务日志, EnumLevel.Error);
                }
            }

            if (EquipmentData.DeviceManager.Devices.ContainsKey(Device.DeviceName.多功能板))
            {
                if (VerifyBase.LightColorType != EmLightColor.红)
                {
                    EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.黄, 1);
                }
            }

            //System.Windows.MessageBoxResult boxResult = System.Windows.MessageBoxResult.Yes;
            if (CheckMode == EnumCheckMode.单步模式 || CheckMode == EnumCheckMode.单组模式)
            {
                if (ConfigHelper.Instance.VerifyModel == "自动模式")
                {
                    LogManager.AddMessage("单点测试完毕。", EnumLogSource.检定业务日志);
                }
                else
                {
                    ++Log.LogViewModel.Instance.TipStepFinished;
                }
            }
            else if (CheckMode == EnumCheckMode.连续模式)
            {
                if (ConfigHelper.Instance.VerifyModel == "自动模式")
                {
                    LogManager.AddMessage("测试完毕。", EnumLogSource.检定业务日志);
                }
                else
                {
                    System.Windows.MessageBox.Show($"测试完毕。{Environment.NewLine}", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, System.Windows.MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        ///正在忙碌
        private bool isBusy = false;
        public bool GetIsBusy()
        {
            return isBusy;
        }

        private string stringPara;
        /// <summary>
        /// 检定参数字符串
        /// </summary>
        public string StringPara
        {
            get { return stringPara; }
            set { SetPropertyValue(value, ref stringPara, "StringPara"); }
        }
        #endregion

        private string checkingName;
        /// <summary>
        /// 当前检定项的名称
        /// </summary>
        public string CheckingName
        {
            get { return checkingName; }
            set { SetPropertyValue(value, ref checkingName, "CheckingName"); }
        }


        /// <summary>
        /// 开始检定
        /// </summary>
        /// <returns></returns>
        private bool InvokeStartVerify()
        {
            CheckInfo.CheckNodeViewModel nodeTemp = EquipmentData.CheckResults.ResultCollection[Index];

            //解析检定参数
            DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[Index];
            if (viewModel != null)
            {
                EquipmentData.Schema.ParaNo = viewModel.GetProperty("PARA_NO") as string;

                if (EquipmentData.Schema.ParaNo == "12002" || EquipmentData.Schema.ParaNo == "12003") //启动潜动的时候同步进行通讯协议检查
                {
                    VerifyProcess2();
                }
                else
                {
                    WriteProtocol();//如果正在第二个线程执行通讯协议检查，就需要等待这个通讯协议检查结束
                }

                string className = EquipmentData.Schema.ParaInfo.ClassName;//检定点的类名
                var temp = from item in EquipmentData.Schema.ParaInfo.CheckParas select item.ParaDisplayName;
                string paraFormat = string.Join("|", temp);
                nodeTemp.IsChecking = true;
                string key = viewModel.GetProperty("PARA_KEY") as string;
                string paraValue = viewModel.GetProperty("PARA_VALUE") as string;
                string name = viewModel.GetProperty("PARA_NAME") as string;

                if (ConfigHelper.Instance.OperatingConditionsYesNo.Trim() == "是")
                {
                    IMICP.OpenPortIMICP open = new IMICP.OpenPortIMICP();
                    open.WorkingStatus("04", "02");
                    open.EventEscalation("0119", key);
                }

                //清除当前实时报文
                EquipmentData.CheckResults.ResultCollection[index].LiveFrames.ClearFrames();

                bool resultStart = StartVerify(key, className, paraFormat, paraValue, name);//调用检定方法，开始检定

                if (resultStart)   //调用成功
                {
                    TimeMonitor.Instance.StartCurrentItem(Index);
                    if (CheckMode != EnumCheckMode.循环模式)
                    {
                        EquipmentData.CheckResults.ResetCurrentResult(); // 清空旧的检定结果
                    }
                    InnerCommand.VerifyControl.SendMsg($"开始检定:{name} ({key})");//({className})
                }
                else//调用失败
                {
                    IsChecking = false;
                    LogManager.AddMessage("调用开始检定方法失败!错误代码10001-001", EnumLogSource.检定业务日志, EnumLevel.Error);
                    InnerCommand.VerifyControl.SendMsg($"调用检定失败:{name} ({key})({className})");
                    return false;
                }
            }
            else
            {

                IsChecking = false;
                EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 1);
                LogManager.AddMessage("检定过程出现异常,索引超出范围!错误代码10001-002", EnumLogSource.检定业务日志, EnumLevel.Error);

                return false;
            }
            return true;
        }


        public static Assembly assemblys = null;
        public VerifyBase temVerify = null;
        public VerifyBase temVerify2 = null;

        /// 开始检定
        /// <summary>
        /// 开始检定
        /// </summary>
        /// <param name="testNo">检定点编号</param>
        /// <param name="testName">检定方法名称</param>
        /// <param name="testFormat">检定参数值的描述</param>
        /// <param name="testValue">检定参数值</param>
        private bool StartVerify(string testNo, string testName, string testFormat, string testValue, string name)
        {
            try
            {
                if (assemblys == null)
                {
                    string filePath = @"LYTest.Verify.dll";
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] bFile = br.ReadBytes((int)fs.Length);
                    br.Close();
                    fs.Close();
                    assemblys = Assembly.Load(bFile);

                }
                VerifyBase Verify;
                if (testName != null && testName.Trim() != "")
                {
                    string className = "LYTest.Verify." + testName;
                    Type type = assemblys.GetType(className); //获取当前类的类型
                    if (type == null)
                    {
                        LogManager.AddMessage("调用开始检定方法失败!错误代码10001-004", EnumLogSource.检定业务日志, EnumLevel.Error);
                        return false;
                    }

                    object obj = Activator.CreateInstance(type);// 创建此类型实例
                    Verify = (VerifyBase)obj;
                }
                else
                {
                    Verify = new VerifyBase();
                }
                //设置检定数据
                Verify.Test_Format = testFormat;
                Verify.Test_Name = name;
                Verify.Test_No = testNo;
                Verify.Test_Value = testValue;
                Verify.IsDemo = EquipmentData.Equipment.IsDemo;
                temVerify = Verify;
                //调用检定方法
                Task task = new Task(() => { InvokeVerify(Verify); });
                task.Start();
                //Verify.Verify();
            }
            catch (Exception e)
            {
                EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 1);
                LogManager.AddMessage("调用开始检定方法失败!错误代码10001-003:" + e.Message, EnumLogSource.检定业务日志, EnumLevel.Error);
                return false;
            }

            return true;
        }


        private void InvokeVerify(VerifyBase Verify)
        {
            Thread.Sleep(50);  //稍微等待一下，保证当前项目切换过来了，以防数据丢失
            Verify.DoVerify();
            FinishCurrentStep();
        }




        #region 同时进行通讯协议检查试验
        private void WriteProtocol()
        {
            if (!IsCheclProtocol)
            {
                return;
            }
            LogManager.AddMessage("正在等待当前同步进行的通讯协议项目结束", EnumLogSource.检定业务日志);
            IsCheclProtocol = false;//结束通讯协议检查
            while (OneIsProtocolCheck)
            {
                Thread.Sleep(200);
            }

        }



        //条件：1需要是连续检定，2：需要是启动或者潜动的时候，3：通讯协议项目大于0
        //有可能遇到的情况：
        //1：启动或者潜动还没结束，通讯协议检查结束
        //2：启动潜动结束了，通讯协议检查还在进行中
        //3：项目同时结束，都在刷新界面
        //4：启动做完了切换到潜动的过程中还在协议检查，是否需要等待
        //5：多个启动潜动项目,是否需要等待
        //6：停止检定了
        //7：检定项目都结束了，通讯协议检查没有结束
        //8：遇到错误停止检定了
        //9：启动潜动和通讯协议检查都在控制源
        //10：启动有电流，而通讯协议检查没有电流，不需要重新升源===因为检定过程没有关源，所以这里进行判断一次，有电压就不需要升源

        //需要的参数：
        //每个检定点检定过没有,开始检定全部置为false，检定完成置为true
        //还需要一个通讯协议检查当前检定点的序号,用于连续检定
        //开始检定中判断当前检定点是不是启动或者潜动，还需要判断当前是否正在进行通讯协议检查试验
        //如果是启动和潜动并且正在进行通讯协议检查就跳过，如果不在就开启线程进行通讯协议检查试验
        //如果不是通讯协议检查试验，并且正在进行通讯协议检查，就需要等待当前通讯协议检查结束，然后关闭通讯协议检查线程。

        /// <summary>
        /// 是否正在进行通讯协议检查
        /// </summary>
        private bool IsCheclProtocol { get; set; }

        private bool OneIsProtocolCheck;//单个项目是否结束--用于启动潜动结束时候结束线程

        /// <summary>
        /// 通讯协议检查点ID
        /// </summary>
        private int ProtocolIndex { get; set; }

        private readonly AutoResetEvent currentStepWaitHandle2 = new AutoResetEvent(false);
        private void VerifyProcess2()
        {
            if (CheckMode != EnumCheckMode.连续模式)
                return;
            if (!EquipmentData.Equipment.IsSame)
                return;
            if (IsCheclProtocol)
                return;
            IsCheclProtocol = true;
            if (!EquipmentData.Schema.ExistNode("17003") && !EquipmentData.Schema.ExistNode("17001"))
                return;
            ProtocolIndex = -1;
            for (int i = 0; i < EquipmentData.Schema.ParaValues.Count; i++)
            {
                DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[i];
                if (viewModel != null)
                {
                    string PARA_NO = viewModel.GetProperty("PARA_NO") as string;
                    if (PARA_NO == "17003" || PARA_NO == "17001")//通讯协议检查2
                    {
                        if (!EquipmentData.CheckResults.ResultCollection[i].IsChecked)//找到第一个没有检定过的
                        {
                            ProtocolIndex = i;
                            break;
                        }
                    }
                }
            }
            if (ProtocolIndex < 0) //都检定过了,就退出了
            {
                return;
            }
            Task.Factory.StartNew(() => Sss());
        }

        private void Sss()
        {
            Thread.Sleep(10000); //启动潜都得升源，需要等待10秒在开始
            LogManager.AddMessage("开始同步进行通讯协议检查", EnumLogSource.检定业务日志);
            while (IsChecking)
            {
                try
                {
                    if (ProtocolIndex < 0 || ProtocolIndex >= CheckCount)
                    {
                        //IsChecking = false;
                        LogManager.AddMessage("通讯协议项目结束====", EnumLogSource.检定业务日志);
                        break;
                    }
                    DynamicViewModel viewModel = EquipmentData.Schema.ParaValues[ProtocolIndex];
                    string PARA_NO = viewModel.GetProperty("PARA_NO") as string;
                    if ((PARA_NO != "17003" && PARA_NO != "17001") || !IsCheclProtocol)   //这里说明通讯协议检查全部结束了
                    {
                        LogManager.AddMessage("通讯协议项目结束====", EnumLogSource.检定业务日志);
                        break;
                    }
                    if (EquipmentData.CheckResults.ResultCollection[ProtocolIndex].IsSelected && !EquipmentData.CheckResults.ResultCollection[ProtocolIndex].IsChecked)
                    {
                        OneIsProtocolCheck = true;
                        if (!StartVerifyProtocol(viewModel)) //开始检定，根据类名调用检定方法
                        {
                            continue;
                        }

                        #region 等待当前检定项结束
                        currentStepWaitHandle2.Reset();
                        currentStepWaitHandle2.WaitOne();
                        //如果手动终止
                        if (!IsChecking)
                        {
                            LogManager.AddMessage("当前检定项被手动终止!", EnumLogSource.检定业务日志);
                            EquipmentData.CheckResults.ResultCollection[ProtocolIndex].IsChecked = false;
                            break;
                        }
                        #endregion
                    }
                    else
                    {
                        ProtocolIndex++;
                    }
                }
                catch (Exception e)
                {

                    LogManager.AddMessage(string.Format("调用检定开始服务异常:{0}", e.Message), EnumLogSource.检定业务日志, EnumLevel.Error);
                    OneIsProtocolCheck = false;
                    //IsChecking = false;
                }
            }
            IsCheclProtocol = false;
        }

        private bool StartVerifyProtocol(DynamicViewModel viewModel)
        {
            //CheckInfo.CheckNodeViewModel nodeTemp = EquipmentData.CheckResults.ResultCollection[Index];
            if (viewModel == null)
            {
                return false;
            }
            Schema.ParaInfoViewModel ParaInfo = new Schema.ParaInfoViewModel
            {
                ParaNo = viewModel.GetProperty("PARA_NO") as string
            };

            string className = ParaInfo.ClassName;//检定点的类名
            var temp = from item in ParaInfo.CheckParas select item.ParaDisplayName;
            string paraFormat = string.Join("|", temp);
            //nodeTemp.IsChecking = true;
            string key = viewModel.GetProperty("PARA_KEY") as string;
            string paraValue = viewModel.GetProperty("PARA_VALUE") as string;

            bool resultStart = StartVerify2(key, className, paraFormat, paraValue);//调用检定方法，开始检定
            if (resultStart)   //调用成功
            {
                EquipmentData.CheckResults.ResultCollection[ProtocolIndex].IsChecked = true;
                EquipmentData.CheckResults.ResetCurrentResult2(ProtocolIndex); // 清空旧的检定结果
            }

            return true;
        }
        /// <summary>
        /// 开始检定
        /// </summary>
        /// <param name="testNo">检定点编号</param>
        /// <param name="testName">检定方法名称</param>
        /// <param name="testFormat">检定参数值的描述</param>
        /// <param name="testValue">检定参数值</param>
        private bool StartVerify2(string testNo, string testName, string testFormat, string testValue)
        {
            try
            {
                if (assemblys == null)
                {
                    string filePath = @"LYTest.Verify.dll";
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] bFile = br.ReadBytes((int)fs.Length);
                    br.Close();
                    fs.Close();
                    assemblys = Assembly.Load(bFile);
                }
                VerifyBase Verify;
                if (testName != null && testName.Trim() != "")
                {
                    string className = "LYTest.Verify." + testName;
                    Type type = assemblys.GetType(className); //获取当前类的类型
                    if (type == null)
                    {
                        LogManager.AddMessage("调用开始检定方法失败!错误代码10001-004", EnumLogSource.检定业务日志, EnumLevel.Error);
                        return false;
                    }

                    object obj = Activator.CreateInstance(type);// 创建此类型实例
                    Verify = (VerifyBase)obj;
                }
                else
                {
                    Verify = new VerifyBase();
                }
                //设置检定数据
                Verify.Test_Format = testFormat;
                Verify.Test_No = testNo;
                Verify.Test_Value = testValue;
                Verify.IsDemo = EquipmentData.Equipment.IsDemo;

                temVerify2 = Verify;
                //调用检定方法
                Task task = new Task(() => { InvokeVerify2(Verify); });
                task.Start();
                //Verify.Verify();
            }
            catch (Exception e)
            {
                LogManager.AddMessage("调用开始检定方法失败!错误代码10001-003:" + e, EnumLogSource.检定业务日志, EnumLevel.Error);
                return false;
            }

            return true;
        }

        private void InvokeVerify2(VerifyBase Verify)
        {
            //  LogManager.AddMessage(EquipmentData.CheckResults.ResultCollection[ProtocolIndex].Name + "=======开始检定", EnumLogSource.检定业务日志);
            Thread.Sleep(50);  //稍微等待一下，保证当前项目切换过来了，以防数据丢失
            Verify.DoVerify();
            FinishCurrentStep2();
            //Thread.Sleep(50);
        }
        /// 当前检定项检定完毕
        /// <summary>
        /// 当前检定项检定完毕
        /// </summary>
        public void FinishCurrentStep2()
        {
            string checkName = EquipmentData.CheckResults.ResultCollection[ProtocolIndex].Name;
            LogManager.AddMessage(string.Format("项目： {0}  检定完成.", checkName), EnumLogSource.检定业务日志);
            OneIsProtocolCheck = false;
            ProtocolIndex++;
            if (isBusy)
            {
                currentStepWaitHandle2.Set();
            }
        }
        #endregion
    }
}
