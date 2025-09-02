using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.ViewModel.Debug
{
    /// <summary>
    /// 误差板
    /// </summary>
    public class ErrorControlViewModel : ViewModelBase
    {
        #region 旧

        #region 属性

        private bool TaskState = false;

        private int startNo = 1;
        /// <summary>
        /// 开始编号
        /// </summary>
        public int StartNo
        {
            get { return startNo; }
            set
            { SetPropertyValue(value, ref startNo, "StartNo"); }
        }

        private int endNo = 1;
        /// <summary>
        /// 结束编号
        /// </summary>
        public int EndNo
        {
            get { return endNo; }
            set
            {
                SetPropertyValue(value, ref endNo, "EndNo");
            }
        }


        #endregion


        #region 继电器控制方法

        /// <summary>
        /// 关闭所有继电器
        /// </summary>
        public void RelayAll_Off()
        {
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.ControlMeterRelay(1, (byte)0xFF);
            });
        }

        /// <summary>
        /// 开启所有继电器
        /// </summary>
        public void RelayAll_On()
        {

            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.ControlMeterRelay(2, (byte)0xFF);
            });
        }

        public void Relay_On()
        {

            Utility.TaskManager.AddDeviceAction(() =>
            {
                for (int i = StartNo; i <= EndNo; i++)
                {
                    EquipmentData.DeviceManager.ControlMeterRelay(2, (byte)i);
                }
            });

        }
        public void Relay_Off()
        {
            Utility.TaskManager.AddDeviceAction(() =>
            {
                for (int i = StartNo; i <= EndNo; i++)
                {
                    EquipmentData.DeviceManager.ControlMeterRelay(1, (byte)i);
                }
            });
        }
        #endregion


        #region 电机控制

        public void Read_Fault()
        {

            //电压短路标标志00正常，01短路，02继电器不工作
            //电流短路标标志，00正在，01旁路成功，02旁路继电器不工作
            //电机行程标志，00电机行程不确定，01电机行程在上取表出座位置，02电机行程在下，压表入座位置
            //挂表状态标志，00没挂表，01以挂表
            //CT电量过载标志，00正常，01过载(北京改造线科陆CT)
            //跳匝指示灯标志 00正常，01以输出跳匝信号
            //二级设备温度板电流过载标志，00没过载，01过载(北京改造线，互感表)

            Utility.TaskManager.AddDeviceAction(() =>
            {
                string str = "";
                for (int i = StartNo; i <= EndNo; i++)
                {
                    // 返回状态0:00表示没短路，01表示短路；DATA1-电流开路标志，1:00表示没开路，01表示开路。</param>
                    bool t = EquipmentData.DeviceManager.Read_Fault(03, (byte)i, out byte[] OutResult);
                    if (t)
                    {
                        str += $"表位【{i}】";
                        str += (MeterState_U)OutResult[0] + "|";
                        str += (MeterState_I)OutResult[1] + "|";
                        str += (MeterState_Motor)OutResult[2] + "|";
                        str += (MeterState_YesOrNo)OutResult[3] + "|";
                        str += (MeterState_CT)OutResult[4] + "|";
                        str += (MeterState_Trip)OutResult[5] + "|";
                        str += (MeterState_TemperatureI)OutResult[6] + "|";
                    }
                    else
                    {
                        str += $"表位【{i}】读取失败";
                    }
                    str += "\r\n";
                }
                Utility.Log.LogManager.AddMessage(str);
            });
        }

        public void Tesk_Read_Fault()
        {

            if (TaskState)
            {
                return;
            }
            Utility.Log.LogManager.AddMessage("开始读取");
            TaskState = true;
            Task task = new Task(() =>
            {
                while (true)
                {
                    Read_Fault2();
                    System.Threading.Thread.Sleep(1000);
                    if (!TaskState)
                    {
                        Utility.Log.LogManager.AddMessage("停止读取");
                        break;
                    }

                }
            });
            task.Start();

        }
        public void StopTask()
        {
            TaskState = false;
        }
        public void Read_Fault2()
        {
            string str = "";
            for (int i = StartNo; i <= EndNo; i++)
            /// <param name="OutResult">返回状态0:00表示没短路，01表示短路；DATA1-电流开路标志，1:00表示没开路，01表示开路。</param>
            {
                bool t = EquipmentData.DeviceManager.Read_Fault(03, (byte)i, out byte[] OutResult);
                if (t)
                {
                    str += $"表位【{i}】";
                    str += (MeterState_U)OutResult[0] + "|";
                    str += (MeterState_I)OutResult[1] + "|";
                    str += (MeterState_Motor)OutResult[2] + "|";
                    str += (MeterState_YesOrNo)OutResult[3] + "|";
                    str += (MeterState_CT)OutResult[4] + "|";
                    str += (MeterState_Trip)OutResult[5] + "|";
                    str += (MeterState_TemperatureI)OutResult[6] + "|";
                }
                else
                {
                    str += $"表位【{i}】读取失败";
                }
                str += "\r\n";
            }
            Utility.Log.LogManager.AddMessage(str);

        }
        #endregion

        #endregion

        public ErrorControlViewModel()
        {
            PulseTypeList.Clear();
            PulseTypeList.Add("有功误差");
            PulseTypeList.Add("无功误差");
            PulseTypeList.Add("有功脉冲计数");
            PulseTypeList.Add("无功脉冲计数");
            PulseTypeList.Add("时钟误差");
            ItoPulseType = "秒脉冲输出";
        }

        public Dictionary<string, MeterStartControlViewModel> meterStartS = new Dictionary<string, MeterStartControlViewModel>();

        public bool isAllCheck1 = false;
        public bool IsAllCheck1
        {
            get { return isAllCheck1; }
            set
            {
                SetPropertyValue(value, ref isAllCheck1, "IsAllCheck1");
                foreach (var item in meterStartS.Keys)
                {
                    if (int.Parse(item) <= meterStartS.Keys.Count / 2)
                    {
                        MeterStartControlViewModel model = meterStartS[item];
                        model.IsCheck = isAllCheck1;
                    }
                }
            }
        }
        public bool isAllCheck2 = false;
        public bool IsAllCheck2
        {
            get { return isAllCheck2; }
            set
            {
                SetPropertyValue(value, ref isAllCheck2, "IsAllCheck2");
                foreach (var item in meterStartS.Keys)
                {
                    if (int.Parse(item) > meterStartS.Keys.Count / 2)
                    {
                        MeterStartControlViewModel model = meterStartS[item];
                        model.IsCheck = IsAllCheck2;
                    }
                }
            }
        }
        #region 新--表位控制
        private int meterNo = 1;
        /// <summary>
        ///表位编号
        /// </summary>
        public int MeterNo
        {
            get { return meterNo; }
            set
            { SetPropertyValue(value, ref meterNo, "MeterNo"); }
        }
        private string address = "";
        /// <summary>
        /// 开始编号
        /// </summary>
        public string Address
        {
            get { return address; }
            set
            { SetPropertyValue(value, ref address, "Address"); }
        }

        //读取表地址
        public void ReadAddress()
        {

            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.Controller.UpdateMeterProtocol();
                Address = MeterProtocolAdapter.Instance.ReadAddress(meterNo - 1);
            });

        }
        /// <summary>
        /// 读取电表电流
        /// </summary>
        public void ReadCurrent()
        {
            if (VerifyBase.OneMeterInfo.DgnProtocol == null) EquipmentData.Controller.UpdateMeterProtocol();

            //关闭所有继电器
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            for (int i = 0; i < num; i++)
            {
                EquipmentData.DeviceManager.ControlMeterRelay(1, (byte)0xFF, i);
                System.Threading.Thread.Sleep(150);
            }
            var m = VerifyBase.OneMeterInfo;
            int v = 1;
            switch (m.MD_WiringMode)
            {
                case "三相四线":
                    v = 0;
                    break;
                case "三相三线":
                    v = 1;
                    break;
                case "单相":
                    v = 5;
                    break;
                default:
                    break;
            }
            EquipmentData.DeviceManager.PowerOn(v, m.MD_UB, m.MD_UB, m.MD_UB, 1, 1, 1, 0, 240, 120, 0, 240, 120, 50, 1);
            System.Threading.Thread.Sleep(8000);

            string[] s = MeterProtocolAdapter.Instance.ReadData("A相电流");
            string str = "";
            string rowstr = "";
            for (int i = 0; i < s.Length; i++)
            {
                int len;
                string value;
                if (string.IsNullOrEmpty(s[i]))
                {
                    value = "异常";
                    len = -2;
                }
                else
                {
                    value = (float.Parse(s[i]) / 1000f).ToString();
                    len = 4 - value.Length;
                }
                rowstr += $"表位【{i + 1}】：{value}".PadRight(20 + len, ' ');
                if ((i + 1) % 3 == 0)
                {
                    str += rowstr;
                    rowstr = "";
                    str += "\r\n";
                }
            }
            if (rowstr != "") str += rowstr;
            MessageBox.Show(str);
        }


        public void Set_HG()
        {
            Utility.Log.LogManager.AddMessage("正在切换探针到互感式");
            //EquipmentData.DeviceManager.Hgq_Set(0);
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.Hgq_Set(0);
            });

        }
        public void Set_ZJ()
        {
            Utility.Log.LogManager.AddMessage("正在切换探针到直接式");
            //EquipmentData.DeviceManager.Hgq_Set(1);
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.Hgq_Set(1);
            });

        }
        public void Set_Off()
        {
            Utility.Log.LogManager.AddMessage("正在关闭互感器电机");
            //EquipmentData.DeviceManager.Hgq_Set(1);
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.Hgq_Off2();
            });

        }

        public void Set_HGRelay()
        {
            Utility.Log.LogManager.AddMessage("正在切换到互感式");
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.PowerOff();
                EquipmentData.DeviceManager.ControlHGQRelay(true);
                EquipmentData.DeviceManager.ControlMeterRelay(1, Convert.ToByte(127), 0);
            });

        }
        public void Set_ZJRelay()
        {
            Utility.Log.LogManager.AddMessage("正在切换到直接式");
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.PowerOff();
                EquipmentData.DeviceManager.ControlHGQRelay(false);
                EquipmentData.DeviceManager.ControlMeterRelay(2, Convert.ToByte(127), 0);
            });

        }


        #region 台体状态灯
        /// <summary>
        /// 红
        /// </summary>
        public void Set_Hong()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.黄, 1);
        }
        /// <summary>
        /// 绿
        /// </summary>
        public void Set_Lv()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.绿, 1);
        }
        /// <summary>
        ///黄
        /// </summary>
        public void Set_Huang()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 1);
        }
        /// <summary>
        /// 红 --闪烁
        /// </summary>
        public void Set_Hong2()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.黄, 2);
        }
        /// <summary>
        /// 绿
        /// </summary>
        public void Set_Lv2()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.绿, 2);
        }
        /// <summary>
        ///黄
        /// </summary>
        public void Set_Huang2()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.红, 2);
        }
        /// <summary>
        ///灭
        /// </summary>
        public void Set_Guan()
        {
            EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.绿, 0);
        }

        public void Remote_SupplyOn()
        {
            EquipmentData.DeviceManager.Remote_Supply(true);
        }
        public void Remote_SupplyOff()
        {
            EquipmentData.DeviceManager.Remote_Supply(false);
        }
        #endregion

        #region 继电器控制
        /// <summary>
        /// 广播恢复所有表位
        /// </summary>
        public void Set_RelayAll_On()
        {
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;

            for (int i = 0; i < num; i++)
            {
                EquipmentData.DeviceManager.ControlMeterRelay(2, (byte)0xFF, i);
                System.Threading.Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 逐个隔离选中表位
        /// </summary>
        public void Set_RelayOnebyOne_Off()
        {
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            num = meterStartS.Keys.Count / num;//每一路线上的数量
            foreach (var item in meterStartS.Keys)
            {
                MeterStartControlViewModel model = meterStartS[item];
                if (model.IsCheck)
                {
                    byte pos = byte.Parse(item);
                    EquipmentData.DeviceManager.ControlMeterRelay(1, pos, (pos - 1) / num);
                    //System.Threading.Thread.Sleep(50);
                }
            }
        }
        /// <summary>
        /// 隔离选中表位
        /// </summary>
        public void Set_RelayAll_Off()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < EquipmentData.DeviceManager.DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < EquipmentData.DeviceManager.BWCountPerLine; n++)
                    {
                        int pos = (line * EquipmentData.DeviceManager.BWCountPerLine) + n + 1;
                        if (meterStartS[pos.ToString()].IsCheck)
                        {
                            EquipmentData.DeviceManager.ControlMeterRelay(1, (byte)pos, line);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }
        /// <summary>
        /// 逐个恢复选中表位
        /// </summary>
        public void Set_RelayOnebyOne_On()
        {
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            num = meterStartS.Keys.Count / num;//每一路线上的数量

            foreach (var item in meterStartS.Keys)
            {
                MeterStartControlViewModel model = meterStartS[item];
                if (model.IsCheck)
                {
                    byte pos = byte.Parse(item);
                    EquipmentData.DeviceManager.ControlMeterRelay(2, pos, (pos - 1) / num);
                    //System.Threading.Thread.Sleep(50);
                }
            }
        }
        /// <summary>
        /// 恢复选中表位
        /// </summary>
        public void Set_Relay_On()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < EquipmentData.DeviceManager.DeviceCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < EquipmentData.DeviceManager.BWCountPerLine; n++)
                    {
                        int pos = (line * EquipmentData.DeviceManager.BWCountPerLine) + n + 1;
                        if (meterStartS[pos.ToString()].IsCheck)
                        {
                            EquipmentData.DeviceManager.ControlMeterRelay(2, (byte)pos, line);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        #endregion

        /// <summary>
        /// 电机下行
        /// </summary>
        public void ElectricmachineryContrnl_Down()
        {
            int lineCount = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            int num = meterStartS.Keys.Count / lineCount;//每一路线上的数量
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < lineCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < num; n++)
                    {
                        int pos = (line * num) + n + 1;
                        if (meterStartS[pos.ToString()].IsCheck)
                        {
                            EquipmentData.DeviceManager.ElectricmachineryContrnl(0, (byte)pos, line);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

        }
        /// <summary>
        /// 上
        /// </summary>
        public void ElectricmachineryContrnl_Up()
        {
            int lineCount = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            int num = meterStartS.Keys.Count / lineCount;//每一路线上的数量
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < lineCount; i++)
            {
                int line = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int n = 0; n < num; n++)
                    {
                        int pos = (line * num) + n + 1;
                        if (meterStartS[pos.ToString()].IsCheck)
                        {
                            EquipmentData.DeviceManager.ElectricmachineryContrnl(1, (byte)pos, line);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

        }
        #endregion


        /// <summary>
        /// 重启电脑
        /// </summary>
        public void RestareWindow()
        {
            if (MessageBox.Show("确定重启计算机！", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                EquipmentData.RestareWindow();
            }
        }


        /// <summary>
        /// 发送检定完成
        /// </summary>
        public void VerfiyEnd()
        {

            //System.Diagnostics.Stopwatch TimeS = new System.Diagnostics.Stopwatch();
            //TimeS.Start();// 开始
            // ViewModel.MeterResoultModel.MeterDataHelper.GetDnbInfoNew();
            //TimeS.Stop();// 结束
            //Utility.Log.LogManager.AddMessage(TimeS.ElapsedMilliseconds.ToString());
            EquipmentData.CallMsg("CompelateOneBatch");
        }

        /// <summary>
        /// 开始自动检定
        /// </summary>
        public void StartAutoVerify()
        {
            MeterProtocolAdapter.Instance.IOTMete_Reset();
        }


        #region 蓝牙光电部分
        private string itoPulseType;
        /// <summary>
        ///物联表的脉冲方式
        /// </summary>
        public string ItoPulseType
        {
            get { return itoPulseType; }
            set
            {
                SetPropertyValue(value, ref itoPulseType, "ItoPulseType");
                switch (ItoPulseType)
                {
                    case "秒脉冲输出":
                        SetPulseType = 0x00;
                        break;
                    case "需量周期":
                        SetPulseType = 0x01;
                        break;
                    case "时段投切":
                        SetPulseType = 0x02;
                        break;
                    case "正向谐波脉冲":
                        SetPulseType = 0x03;
                        break;
                    case "反向谐波脉冲":
                        SetPulseType = 0x04;
                        break;
                    case "无功脉冲":
                        SetPulseType = 0x05;
                        break;
                    case "有功脉冲":
                        SetPulseType = 0x06;
                        break;
                    case "退出检定模式":
                        SetPulseType = 0xFF;
                        break;
                    default:
                        break;
                }
            }
        }
        private byte SetPulseType = 0x00;


        /// <summary>
        /// 复位
        /// </summary>
        public void GD_Reset()
        {

            bool[] yaojian = new bool[meterStartS.Keys.Count];

            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在复位光电模块");
            EquipmentData.Controller.UpdateMeterProtocol();
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Reset(yaojian);
            string tips = "";
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i] && !resoult[i])
                {
                    tips += $"【{i + 1}】";
                }
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "复位失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }
            Utility.Log.LogManager.AddMessage("复位完成");
        }
        /// <summary>
        /// 连接电表
        /// </summary>
        public void GD_Connect()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            string[] addres = new string[yaojian.Length];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                if (yaojian[index])
                {
                    addres[index] = EquipmentData.MeterGroupInfo.Meters[index].GetProperty("MD_POSTAL_ADDRESS") as string;
                }
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在连接电能表");
            EquipmentData.Controller.UpdateMeterProtocol();
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Connect(addres, yaojian, "123456");
            string tips = "";
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i] && !resoult[i])
                {
                    tips += $"【{i + 1}】";
                }
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "连接失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }

            Utility.Log.LogManager.AddMessage("连接成功");
        }
        /// <summary>
        ///  物联网表--预处理--需要等待35秒以上
        /// </summary>
        /// <returns></returns>
        public void GD_Pretreatment()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在进行预处理");
            EquipmentData.Controller.UpdateMeterProtocol();
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Pretreatment(yaojian);
            string tips = "";
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i] && !resoult[i])
                {
                    tips += $"【{i + 1}】";
                }
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "预处理失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }

            Utility.Log.LogManager.AddMessage("预处理成功");
        }
        /// <summary>
        ///  物联网表--预处理-状态查询
        /// </summary>
        /// <returns></returns>
        public void GD_PretreatmentSelect()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在进行预处理状态查询");
            EquipmentData.Controller.UpdateMeterProtocol();
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_PretreatmentSelect(yaojian);
            string tips = "";
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i] && !resoult[i])
                {
                    tips += $"【{i + 1}】";
                }
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "预处理失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }

            Utility.Log.LogManager.AddMessage("预处理成功");
        }
        /// <summary>
        ///   物联网表--切换到光脉冲
        /// </summary>
        /// <returns></returns>
        public void GD_SetLightPulse()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在切到到光电脉冲");
            EquipmentData.Controller.UpdateMeterProtocol();
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_SetLightPulse(SetPulseType, yaojian);
            string tips = "";
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i] && !resoult[i])
                {
                    tips += $"【{i + 1}】";
                }
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "切换光电脉冲失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }

            Utility.Log.LogManager.AddMessage("切换光电脉冲完成");
        }
        /// <summary>
        /// 切换电表到检定模式(待测电能表蓝牙模式切换)：0x02--参数：脉冲类型，通道数量，发射频率，频段
        /// </summary>
        /// <returns></returns>
        public void GD_SetMeterTestModel()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在切换电表到检定模式");
            EquipmentData.Controller.UpdateMeterProtocol();
            byte[] resoult = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel(SetPulseType, 1, 0, 1, yaojian);
            string tips = "";
            string tips2 = "";

            for (int i = 0; i < yaojian.Length; i++)
            {
                if (!yaojian[i] || resoult[i] == 0x00) continue;
                if (resoult[i] == 0x03)
                    tips2 += $"【{i + 1}】";
                else
                    tips += $"【{i + 1}】";
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "切换失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }
            if (tips2 != "")
            {
                tips2 = "表位：" + tips2 + "未授权";
                Utility.Log.LogManager.AddMessage(tips2);
                return;
            }
            Utility.Log.LogManager.AddMessage("切换完成");
        }
        /// <summary>
        ///  切换转换器到检定模式(转换器蓝牙模式切换)---脉冲类型，发射功率，通信模式
        /// </summary>
        /// <returns></returns>
        public void GD_SetConverterModel()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            Utility.Log.LogManager.AddMessage("正在切换转换器到检定模式");
            EquipmentData.Controller.UpdateMeterProtocol();
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_SetConverterModel(SetPulseType, 1, 0, yaojian);
            string tips = "";
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (yaojian[i] && !resoult[i])
                {
                    tips += $"【{i + 1}】";
                }
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "切换失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }

            Utility.Log.LogManager.AddMessage("切换完成");
        }

        public void GD_SetMeterTestModelConn()
        {
            if (SetPulseType == 0x06)
            {
                Utility.Log.LogManager.AddMessage("有功模式不需要切换");
                return;
            }
            string tips = "";
            var s = MeterProtocolAdapter.Instance.SetPulseCom(SetPulseType);
            bool[] yaojian = new bool[meterStartS.Keys.Count];
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }
            for (int i = 0; i < yaojian.Length; i++)
            {
                if (!yaojian[i] || s[i]) continue;
                tips += $"【{i + 1}】";
            }
            if (tips != "")
            {
                tips = "表位：" + tips + "切换失败";
                Utility.Log.LogManager.AddMessage(tips);
                return;
            }
            Utility.Log.LogManager.AddMessage("切换完成");

        }
        #endregion

        #region 误差调试

        #region 属性
        private ulong stdConst = 1000000;
        /// <summary>
        /// 标准常数
        /// </summary>
        public ulong StdConst
        {
            get { return stdConst; }
            set
            { SetPropertyValue(value, ref stdConst, "StdConst"); }
        }
        private int testedConst = 6400;
        /// <summary>
        /// 被检常数
        /// </summary>
        public int TestedConst
        {
            get { return testedConst; }
            set
            { SetPropertyValue(value, ref testedConst, "TestedConst"); }
        }
        private int testQs = 2;
        /// <summary>
        /// 检定圈数
        /// </summary>
        public int TestQs
        {
            get { return testQs; }
            set
            { SetPropertyValue(value, ref testQs, "TestQs"); }
        }

        private ObservableCollection<string> pulseTypeList = new ObservableCollection<string>();
        /// <summary>
        ///脉冲类型
        public ObservableCollection<string> PulseTypeList
        {
            get { return pulseTypeList; }
            set { SetPropertyValue(value, ref pulseTypeList, "PulseTypeList"); }
        }
        private string pulseType;
        /// <summary>
        ///脉冲类型
        public string PulseType
        {
            get { return pulseType; }
            set
            {
                SetPropertyValue(value, ref pulseType, "PulseType");
                switch (PulseType)
                {
                    case "有功误差":
                        PulseTypeValee = 0;
                        break;
                    case "无功误差":
                        PulseTypeValee = 1;
                        break;
                    case "有功脉冲计数":
                        PulseTypeValee = 6;
                        break;
                    case "无功脉冲计数":
                        PulseTypeValee = 7;
                        break;
                    case "时钟误差":
                        PulseTypeValee = 4;
                        break;
                    default:
                        break;
                }
            }
        }

        int PulseTypeValee = 0;
        #endregion


        /// <summary>
        /// 设置标准表标准常数
        /// </summary>
        public void SetStdConst()
        {
            ulong sstdConst = StdConst;
            double[] stdUIGear = new double[] { 0, 0, 0, 0, 0, 0 };
            EquipmentData.DeviceManager.StdGear(0x13, ref sstdConst, ref stdUIGear);
            Utility.Log.LogManager.AddMessage("设置标准表标准常数完成");

        }

        // add yjt jx 20230205 新增设置标准表标准常数
        /// <summary>
        /// 设置副标准表标准常数
        /// </summary>
        public void SetStdConstDeviceID()
        {
            ulong sstdConst = StdConst;
            double[] stdUIGear = new double[] { 0, 0, 0, 0, 0, 0 };
            EquipmentData.DeviceManager.StdGearDeviceID(0x13, ref sstdConst, ref stdUIGear, 1);
            Utility.Log.LogManager.AddMessage("设置标准表副表标准常数完成");

        }
        /// <summary>
        /// 设置误差版标准常数
        /// </summary>
        public void SetWCStdConst()
        {
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            int errStaconst = (int)(StdConst / 100);
            if (PulseType == "时钟误差") errStaconst = (int)StdConst;
            for (int i = 0; i < num; i++)
            {
                EquipmentData.DeviceManager.SetStandardConst(PulseTypeValee, errStaconst, -2, (byte)0xFF, i);
                System.Threading.Thread.Sleep(150);
            }
            Utility.Log.LogManager.AddMessage("设置误差版标准常数完成");
        }
        /// <summary>
        /// 设置误差板被检常数
        /// </summary>
        public void SetWcTestConst()
        {
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            for (int i = 0; i < num; i++)
            {
                EquipmentData.DeviceManager.SetTestedConst(PulseTypeValee, TestedConst, 0, TestQs, (byte)0xFF, i);
                System.Threading.Thread.Sleep(150);
            }
            Utility.Log.LogManager.AddMessage("设置误差板被检常数完成");

        }

        bool WcStart = false;
        /// <summary>
        /// 启动误差版
        /// </summary>
        public void StartWc()
        {
            if (WcStart) return;
            if (PulseTypeValee != 4)
            {
                EquipmentData.DeviceManager.SetPulseType((PulseTypeValee + 49).ToString("x"));
            }
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            for (int i = 0; i < num; i++)
            {
                EquipmentData.DeviceManager.StartWcb(PulseTypeValee, (byte)0xFF, i);
                System.Threading.Thread.Sleep(150);
            }
            WcStart = true;
            Utility.Log.LogManager.AddMessage("启动误差版完成");

        }
        /// <summary>
        /// 停止误差版
        /// </summary>
        public void StopWc()
        {
            WcStart = false;
            int num = EquipmentData.DeviceManager.Devices[DeviceName.误差板].Count;
            for (int i = 0; i < num; i++)
            {
                EquipmentData.DeviceManager.StopWcb(PulseTypeValee, (byte)0xFF, i);
                System.Threading.Thread.Sleep(150);
            }
            Utility.Log.LogManager.AddMessage("停止误差版完成");


        }

        /// <summary>
        /// 根据电压电流获取常数
        /// </summary>
        public void GetStdConst()
        {
            StdConst = EquipmentData.DeviceManager.GetStdConst(EquipmentData.StdInfo.Ia);
        }

        #endregion

        #region 表位状态
        /// <summary>
        /// 判断电流开路
        /// </summary>
        /// <returns></returns>
        public void CurrentOpenCircuits()
        {
            bool[] yaojian = new bool[meterStartS.Keys.Count];

            bool[] bln_OffMeter = new bool[yaojian.Length];
            bool[] result = new bool[yaojian.Length];
            bln_OffMeter.Fill(true);
            result.Fill(true);
            int index = 0;
            foreach (var item in meterStartS.Keys)
            {
                yaojian[index] = meterStartS[item].IsCheck;
                index++;
            }

            Set_RelayAll_Off();

            float UB = float.Parse(EquipmentData.MeterGroupInfo.Meters[0].GetProperty("MD_UB").ToString());
            string UA = EquipmentData.MeterGroupInfo.Meters[0].GetProperty("MD_UA").ToString();
            string jxfs = EquipmentData.MeterGroupInfo.Meters[0].GetProperty("MD_WIRING_MODE").ToString();

            float xIb = Number.GetCurrentByIb("1.0Ib", UA, false);
            string tips = "";

            int jxfsvalue = 5;
            if (jxfs == "三相四线")
            {
                jxfsvalue = 0;
            }
            else if (jxfs == "三相三线")
            {
                jxfsvalue = 1;
            }

            EquipmentData.DeviceManager.PowerOn(jxfsvalue, UB, UB, UB, xIb, xIb, xIb, 0, 240, 120, 0, 240, 120, 50, 1);
            Thread.Sleep(10000);
            EquipmentData.Controller.UpdateMeterProtocol();

            string[] meterData = MeterProtocolAdapter.Instance.ReadDataYJ("电流数据块", yaojian);

            for (int i = 0; i < yaojian.Length; i++)
            {
                if (!yaojian[i]) continue;

                if (meterData[i] != null)
                {
                    string[] Iabc = meterData[i].Split(',');

                    //string values;
                    if (jxfs == "单相")
                    {
                        //values = "A:" + Iabc[0];
                        if (float.Parse(Iabc[0]) <= 0)
                        {
                            bln_OffMeter[i] = false;
                        }
                    }
                    else
                    {
                        //values = "A:" + Iabc[0] + " , B:" + Iabc[1] + " , C:" + Iabc[2];
                        if (float.Parse(Iabc[0]) <= 0 || float.Parse(Iabc[1]) <= 0 || float.Parse(Iabc[2]) <= 0)
                        {
                            bln_OffMeter[i] = false;
                        }
                    }

                    if (bln_OffMeter[i] == false)
                    {
                        tips += $"【{i + 1}】";
                    }
                }
            }

            EquipmentData.DeviceManager.PowerOff();
            Thread.Sleep(3000);

            Set_Relay_On();

            if (Array.IndexOf(bln_OffMeter, false) != -1)
            {
                tips = "表位：" + tips + "电流开路。";
                Utility.Log.LogManager.AddMessage(tips);
            }
            else
            {
                tips = "无表位电流开路。";
                Utility.Log.LogManager.AddMessage(tips);
            }
        }
        #endregion
    }

}
