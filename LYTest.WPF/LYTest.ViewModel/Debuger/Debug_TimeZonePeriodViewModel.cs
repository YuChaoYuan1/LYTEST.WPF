using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LYTest.ViewModel.Debug
{
    /// <summary>
    /// 时区时段设置
    /// </summary>
    public class Debug_TimeZonePeriodViewModel : ViewModelBase
    {
        private readonly VerifyBase verifyBase = new VerifyBase();
        readonly string Path = Directory.GetCurrentDirectory() + "\\Ini\\MeterData\\TimeZonePeriodData.ini";

        #region 参数

        private ObservableCollection<string> monthList = new ObservableCollection<string>();
        /// <summary>
        /// 月数据源
        /// </summary>
        public ObservableCollection<string> MonthList
        {
            get { return monthList; }
            set { SetPropertyValue(value, ref monthList, "MonthList"); }
        }
        private ObservableCollection<string> dayList = new ObservableCollection<string>();
        /// <summary>
        /// 日数据源
        /// </summary>
        public ObservableCollection<string> DayList
        {
            get { return dayList; }
            set { SetPropertyValue(value, ref dayList, "DayList"); }
        }
        private ObservableCollection<string> flList = new ObservableCollection<string>();
        /// <summary>
        /// 费率数据源
        /// </summary>
        public ObservableCollection<string> FlList
        {
            get { return flList; }
            set { SetPropertyValue(value, ref flList, "FlList"); }

        }


        private string timeZone_Month = "01";
        /// <summary>
        /// 时区月
        /// </summary>
        public string TimeZone_Month
        {
            get { return timeZone_Month; }
            set { SetPropertyValue(value, ref timeZone_Month, "TimeZone_Month"); }

        }

        private string timeZone_Day = "01";
        /// <summary>
        /// 时区日
        /// </summary>
        public string TimeZone_Day
        {
            get { return timeZone_Day; }
            set { SetPropertyValue(value, ref timeZone_Day, "TimeZone_Day"); }

        }

        private string timeInterval_FL = "尖";
        /// <summary>
        /// 时段费率
        /// </summary>
        public string TimeInterval_FL
        {
            get { return timeInterval_FL; }
            set { SetPropertyValue(value, ref timeInterval_FL, "TimeInterval_FL"); }

        }
        private string timeInterval_Time = "00:00";
        /// <summary>
        /// 时段 时间
        /// </summary>
        public string TimeInterval_Time
        {
            get { return timeInterval_Time; }
            set { SetPropertyValue(value, ref timeInterval_Time, "TimeInterval_Time"); }
        }
        private DateTime switchingTime;
        /// <summary>
        /// 两套费率切换时间
        /// </summary>
        public DateTime SwitchingTime
        {
            get { return switchingTime; }
            set { SetPropertyValue(value, ref switchingTime, "SwitchingTime"); }
        }

        #endregion


        #region 属性

        private string tips = "";
        /// <summary>
        /// 时段 时间
        /// </summary>
        public string Tips
        {
            get { return tips; }
            set { SetPropertyValue(value, ref tips, "Tips"); }
        }

        /// <summary>
        /// 当前选中的是第几套
        /// </summary>
        public ObservableCollection<string> SelectTimeData;

        private int selectIndex = -1;
        /// <summary>
        /// 当前选中是第几套
        /// </summary>
        public int SelectIndex
        {
            get { return selectIndex; }
            set
            {
                selectIndex = value;
                if (selectIndex == 0) SelectTimeData = timeData1;
                else SelectTimeData = timeData2;
            }
        }

        private int listSelectIndex = -1;
        /// <summary>
        ///当前选中的是那个项目
        /// </summary>
        public int ListSelectIndex
        {
            get { return listSelectIndex; }
            set { listSelectIndex = value; }
        }


        private int yearNumber;
        /// <summary>
        /// 年时区数
        /// </summary>
        public int YearNumber
        {
            get { return yearNumber; }
            set { yearNumber = value; }
        }

        private ObservableCollection<string> timeData1 = new ObservableCollection<string>();
        /// <summary>
        /// 第一套时段数据 --当前套
        /// </summary>
        public ObservableCollection<string> TimeData1
        {
            get { return timeData1; }
            set { timeData1 = value; }
        }
        private ObservableCollection<string> timeData2 = new ObservableCollection<string>();
        /// <summary>
        /// 第二套时段数据 --备用套
        /// </summary>
        public ObservableCollection<string> TimeData2
        {
            get { return timeData2; }
            set { timeData2 = value; }
        }


        #endregion


        public Debug_TimeZonePeriodViewModel()
        {
            TimeData1.Clear();
            TimeData2.Clear();
            Readini();

            SwitchingTime = DateTime.Now;

            for (int i = 1; i <= 12; i++)
            {
                MonthList.Add(i.ToString().PadLeft(2, '0'));
            }
            for (int i = 1; i <= 31; i++)
            {
                DayList.Add(i.ToString().PadLeft(2, '0'));
            }
            FlList.Add("尖");
            FlList.Add("峰");
            FlList.Add("平");
            FlList.Add("谷");
            FlList.Add("深谷");

            SelectIndex = 0;
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        private void Readini()
        {
            string str;

            for (int index = 1; index <= 2; index++)
            {

                str = Core.OperateFile.GetINI($"第{index}套时段", "count", Path).Trim();
                if (str != "" && Number.IsNumeric(str))
                {
                    int count = int.Parse(str);
                    for (int i = 1; i <= count; i++)
                    {
                        str = Core.OperateFile.GetINI($"第{index}套时段", i.ToString(), Path).Trim();
                        if (str != "")
                        {
                            if (index == 1) TimeData1.Add(str);
                            else TimeData2.Add(str);
                        }
                    }
                }
            }
        }

        #region 按钮事件

        private bool IsRun = false;

        /// <summary>
        /// 写入时段    --SelectIndex
        /// </summary>
        public void WriteDateTime()
        {
            if (IsRun)
            {
                Tips = "正在运行请稍后...";
                return;
            }
            if (EquipmentData.Equipment.IsDemo)
            {
                Tips = "请退出演示模式在进行操作";
                return;
            }
            IsRun = true;
            //这里解析数据
            //写数据主要需要一个名称--一个内容
            Utility.TaskManager.AddDeviceAction(() =>
            {
                try
                {
                    ///解析数据
                    List<WriteDataFormat> writes = ParseData();
                    EquipmentData.Controller.UpdateMeterProtocol();//每次开始检定，更新一下电表协议

                    verifyBase.PowerOn();
                    verifyBase.WaitTime("正在升电压", 10);

                    verifyBase.MessageAdd("开始读取表地址表号", EnumLogType.流程信息);
                    verifyBase.ReadMeterAddrAndNo();

                    verifyBase.MessageAdd("开始进行身份认证", EnumLogType.流程信息);
                    verifyBase.Identity(false);


                    verifyBase.MessageAdd("正在写入数据", EnumLogType.流程信息);
                    bool[] yaojian = EquipmentData.MeterGroupInfo.YaoJian;
                    string Error = "";
                    for (int i = 0; i < writes.Count; i++)
                    {
                        verifyBase.MessageAdd("正在写入" + writes[i].Name, EnumLogType.提示信息);
                        bool[] T = WriteData(writes[i]);

                        for (int index = 0; index < yaojian.Length; index++)
                        {
                            if (!yaojian[index]) continue;
                            if (!T[index])     //写入失败
                            {
                                Error += $"表位【{index + 1}】写入【{writes[i].Name}】失败\r\n";
                            }
                        }
                    }
                    //这里需要恢复电表时间
                    verifyBase.MessageAdd("正在恢复电表时间", EnumLogType.流程信息);
                    bool[] r = MeterProtocolAdapter.Instance.WriteDateTime(yaojian);
                    for (int index = 0; index < yaojian.Length; index++)
                    {
                        if (!yaojian[index]) continue;
                        if (!r[index])     //写入失败
                        {
                            Error += $"表位【{index + 1}】【恢复时间】失败\r\n";
                        }
                    }
                    Tips = Error;
                    verifyBase.PowerOff();
                    verifyBase.WaitTime("正在关源", 5);
                }
                catch (Exception ex)
                {
                    Tips = ex.ToString();
                }
                IsRun = false;
            });
        }

        /// <summary>
        /// 读取表内时段
        /// </summary>
        public void ReadDateTime()
        {
            if (IsRun)
            {
                Tips = "正在运行请稍后...";
                return;
            }
            if (EquipmentData.Equipment.IsDemo)
            {
                Tips = "请退出演示模式在进行操作";
                return;
            }
            IsRun = true;

            Utility.TaskManager.AddDeviceAction(() =>
            {
                try
                {
                    ///解析数据
                    List<WriteDataFormat> writes = ParseData();
                    EquipmentData.Controller.UpdateMeterProtocol();//每次开始检定，更新一下电表协议

                    verifyBase.PowerOn();
                    verifyBase.WaitTime("正在升电压", 10);

                    verifyBase.MessageAdd("开始读取表地址表号", EnumLogType.流程信息);
                    verifyBase.ReadMeterAddrAndNo();

                    List<string> ReadAllData = new List<string>();

                    bool[] yaojian = EquipmentData.MeterGroupInfo.YaoJian;
                    for (int index = 0; index < yaojian.Length; index++)
                    {
                        if (!yaojian[index]) continue;
                        ReadAllData.Add($"表位【{index + 1}】读回数据:");
                    }


                    List<string> ReadName = new List<string>
                    {
                        "年时区数",
                        "日时段表数",
                        "日时段数",
                        "两套时区表切换时间",
                        "两套日时段表切换时间"
                    };
                    for (int i = 0; i < ReadName.Count; i++)
                    {
                        verifyBase.MessageAdd("正在读取" + ReadName[i], EnumLogType.提示信息);
                        ParseReadData(ReadAllData, ReadName[i]);
                    }
                    ReadName.Clear();
                    //然后开始读取具体的第几日数据
                    for (int i = 0; i < readDayNumber; i++)
                    {
                        ReadName.Add($"第一套第{i + 1}日时段数据");
                    }
                    for (int i = 0; i < readDayNumber; i++)
                    {
                        ReadName.Add($"第二套第{i + 1}日时段数据");
                    }
                    for (int i = 0; i < ReadName.Count; i++)
                    {
                        verifyBase.MessageAdd("正在读取" + ReadName[i], EnumLogType.提示信息);
                        ParseReadData(ReadAllData, ReadName[i]);
                    }
                    Tips = string.Join("", ReadAllData); //显示读取回来的数据
                    verifyBase.PowerOff();
                    verifyBase.WaitTime("正在关源", 5);
                }
                catch (Exception ex)
                {
                    Tips = ex.ToString();
                }
                IsRun = false;
            });
        }
        /// <summary>
        /// 解析返回数据
        /// </summary>
        /// <returns></returns>
        public void ParseReadData(List<string> ReadAllData, string name)
        {
            string[] data = ReadData(name);
            bool[] yaojian = EquipmentData.MeterGroupInfo.YaoJian;
            for (int index = 0; index < yaojian.Length; index++)
            {
                if (!yaojian[index]) continue;
                ReadAllData[index] += ParseReadDataFormat(name, data[index]);//保存读回的数据
            }
        }
        private int readDayNumber = 0;//需要读取几日的数据
        public string ParseReadDataFormat(string name, string value)
        {
            if (value == null) return "未读取到数据";
            value = value.Trim();
            if (value == "") return "未读取到数据";

            string str = $"{name}:【{value}】";

            if (name.IndexOf("日时段数据") != -1)
            {
                for (int i = 0; i < value.Length; i += 6)
                {
                    string t = value.Substring(i, 6);
                    DateTime dt2 = DateTime.ParseExact(t.Substring(0, 4), "HHmm", System.Globalization.CultureInfo.CurrentCulture);
                    str += "\r\n---" + GetFLValue(t.Substring(4, 2)) + "---" + dt2.ToString("HH:mm");
                }
            }
            else
            {
                switch (name)
                {
                    case "年时区数": //04
                        readDayNumber = int.Parse(value);
                        break;
                    case "日时段表数": //04
                    case "日时段数":   //05
                        break;
                    case "两套时区表切换时间":  //20230101000000
                    case "两套日时段表切换时间":   //20230101000000
                        DateTime dt = DateTime.ParseExact(value, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        str += "\r\n：" + dt.ToString("yyyy年MM月dd日 HH时mm分ss秒");
                        break;
                    case "第一套时区表数据":  //010101 020102
                    case "第二套时区表数据":  // 010101 020102
                        //6个一组
                        for (int i = 0; i < value.Length; i += 6)
                        {
                            string t = value.Substring(i, 6);
                            DateTime dt2 = DateTime.ParseExact(t.Substring(0, 4), "HHmm", System.Globalization.CultureInfo.CurrentCulture);
                            str += "\r\n第" + t.Substring(4, 2) + "日时区:起始时间为" + dt2.ToString("HH月mm日");
                        }
                        break;
                    default:
                        break;
                }
            }
            return str + "\r\n";
        }



        /// <summary>
        /// 添加时区
        /// </summary>
        public void AddTimeZone()
        {
            int count = SelectTimeData.Count(item => item.Contains("起始时间"));
            string str = $"第{count + 1}日时区:起始时间为{TimeZone_Month}月{TimeZone_Day}日";
            SelectTimeData.Add(str);
        }
        /// <summary>
        /// 添加时段
        /// </summary>
        public void AddTimeInterval()
        {
            string str = "---" + TimeInterval_FL + "---" + TimeInterval_Time;
            SelectTimeData.Add(str);
        }
        /// <summary>
        /// 第二套复制第一套
        /// </summary>
        public void TowCopyOne()
        {
            timeData2.Clear();
            for (int i = 0; i < timeData1.Count; i++)
            {
                timeData2.Add(timeData1[i]);
            }
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearData()
        {
            SelectTimeData.Clear();

        }
        /// <summary>
        /// 删除数据
        /// </summary>
        public void DeleteData()
        {
            if (ListSelectIndex == -1) return;
            if (ListSelectIndex > SelectTimeData.Count) return;
            SelectTimeData.RemoveAt(ListSelectIndex);

        }
        /// <summary>
        ///添加时段时间
        /// </summary>
        public void AddTimeInterval_Time()
        {
            DateTime dt = DateTime.ParseExact(TimeInterval_Time, "HH:mm", System.Globalization.CultureInfo.CurrentCulture);
            dt = dt.AddMinutes(1);
            TimeInterval_Time = dt.ToString("HH:mm");

        }
        /// <summary>
        ///减少时段时间
        /// </summary>
        public void ReduceTimeInterval_Time()
        {
            DateTime dt = DateTime.ParseExact(TimeInterval_Time, "HH:mm", System.Globalization.CultureInfo.CurrentCulture);
            dt = dt.AddMinutes(-1);
            TimeInterval_Time = dt.ToString("HH:mm");

        }
        #endregion

        #region 数据的解析


        /// <summary>
        /// 获取费率数据
        /// </summary>
        /// <returns></returns>
        private string GetFLValue(string str)
        {
            string value = "";
            switch (str)
            {
                case "尖":
                    value = "01";
                    break;
                case "峰":
                    value = "02";
                    break;
                case "平":
                    value = "03";
                    break;
                case "谷":
                    value = "04";
                    break;
                case "深谷":
                    value = "05";
                    break;
                case "01":
                case "1":
                    value = "尖";
                    break;
                case "02":
                case "2":
                    value = "峰";
                    break;
                case "03":
                case "3":
                    value = "平";
                    break;
                case "04":
                case "4":
                    value = "谷";
                    break;
                case "05":
                case "5":
                    value = "深谷";
                    break;
                default:
                    break;
            }

            return value;
        }


        /// <summary>
        /// 解析数据
        /// </summary>
        /// <returns></returns>
        private List<WriteDataFormat> ParseData()
        {
            List<WriteDataFormat> data = new List<WriteDataFormat>();


            data.AddRange(AddWriteDataFormat(timeData1));//添加第一套

            //因为写入只能写入备用套就是第二套，所以这里需要先切换时间
            data.Add(new WriteDataFormat() { Name = "两套时区表切换时间", Data = "20300701010100" });
            data.Add(new WriteDataFormat() { Name = "两套日时段表切换时间", Data = "20300701010100" });
            data.Add(new WriteDataFormat() { Name = "日期时间", Data = "20300701010040" });


            data.AddRange(AddWriteDataFormat(timeData2));//添加第二套

            //这里写入正确的切换时间
            data.Add(new WriteDataFormat() { Name = "两套时区表切换时间", Data = SwitchingTime.ToString("yyyyMMddHHmmss") });
            data.Add(new WriteDataFormat() { Name = "两套日时段表切换时间", Data = SwitchingTime.ToString("yyyyMMddHHmmss") });
            return data;
        }

        private List<WriteDataFormat> AddWriteDataFormat(ObservableCollection<string> data)
        {
            Dictionary<string, PeriodData> keys = new Dictionary<string, PeriodData>();
            string dNum = "";
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == "") continue;
                if (data[i].IndexOf("起始时间") != -1)
                {
                    //dayData.Add(new List<string>());
                    string[] tem = data[i].Split(':');
                    dNum = MidStrEx(tem[0], "第", "日");
                    string MM = MidStrEx(tem[1], "起始时间", "月");
                    string dd = MidStrEx(tem[1], "月", "日");
                    if (!keys.ContainsKey(dNum))
                    {
                        keys.Add(dNum, new PeriodData() { startTime = MM + dd + dNum.ToString().PadLeft(2, '0') });
                    }
                }
                else
                {
                    if (dNum == "") continue;
                    if (keys.ContainsKey(dNum))
                    {
                        string[] tem = data[i].Split("---".ToCharArray());
                        tem[6] = tem[6].Replace(":", "");
                        keys[dNum].data.Add(tem[6] + GetFLValue(tem[3]));
                    }
                }
            }
            int dayTableNum = 0;//日时段数
            string TowData = "";//第二套日时区表数据
            foreach (var item in keys)
            {
                if (item.Value.data.Count > dayTableNum) dayTableNum = item.Value.data.Count; //取最多的时段数
                TowData += item.Value.startTime;
            }

            List<WriteDataFormat> writeList = new List<WriteDataFormat>
            {
                new WriteDataFormat() { Name = "年时区数", Data = keys.Keys.Count.ToString().PadLeft(2, '0') },
                new WriteDataFormat() { Name = "日时段表数", Data = keys.Keys.Count.ToString().PadLeft(2, '0') },
                new WriteDataFormat() { Name = "日时段数", Data = dayTableNum.ToString().PadLeft(2, '0') },
                new WriteDataFormat() { Name = "第二套时区表数据", Data = TowData }
            };
            foreach (var item in keys)
            {
                writeList.Add(new WriteDataFormat() { Name = $"第二套第{item.Key}日时段数据", Data = string.Join("", item.Value.data.ToArray()) });
            }

            return writeList;

        }

        /// <summary>
        /// 截取俩个字符串中间的字符
        /// </summary>
        /// <param name="sourse">数据源</param>
        /// <param name="startstr">开始字符串</param>
        /// <param name="endstr">结束字符串</param>
        /// <returns></returns>
        public string MidStrEx(string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                    return result;
                string tmpstr = sourse.Substring(startindex + startstr.Length);
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                    return result;
                result = tmpstr.Remove(endindex);
            }
            catch
            {
                //Log.WriteLog("MidStrEx Err:" + ex.Message);
            }
            return result;
        }


        #endregion

        #region 读写方法

        /// <summary>
        /// 读取指定名称的数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string[] ReadData(string name)
        {
            return MeterProtocolAdapter.Instance.ReadData(name);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="name">数据项名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private bool[] WriteData(WriteDataFormat writeData)
        {
            return MeterProtocolAdapter.Instance.WriteData(writeData.Name, writeData.Data);
        }
        #endregion

        #region 帮助类
        /// <summary>
        /// 时段数据
        /// </summary>
        class PeriodData
        {
            public string startTime = "";//开始时间
            public List<string> data = new List<string>();//时段数据
        }

        /// <summary>
        /// 写入数据帮助类
        /// </summary>
        class WriteDataFormat
        {
            /// <summary>
            /// 数据项名称
            /// </summary>
            public string Name;
            /// <summary>
            /// 写入的内容
            /// </summary>
            public string Data;
        }
        #endregion

    }
}
