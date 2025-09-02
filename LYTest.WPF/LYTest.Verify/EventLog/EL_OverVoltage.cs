using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 过压事件记录
    /// </summary>
    class EL_OverVoltage : VerifyBase
    {
        readonly List<Cus_PowerYuanJian> YJ = new List<Cus_PowerYuanJian>();
        private string[] m_strEventStatus = null;
        private float MD_UB = 0.0f;
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {

            base.Verify();
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            bool[] arrResult = new bool[MeterNumber];
            arrResult.Fill(true);

            if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
            {
                Identity(true);
                MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                string[] dicEnergy = MeterProtocolAdapter.Instance.ReadData("电能表过压事件_配置参数");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!string.IsNullOrWhiteSpace(dicEnergy[i]) && float.Parse(dicEnergy[i].Split('|')[0]) > 0.0f && float.Parse(dicEnergy[i].Split('|')[0]) > MD_UB)
                    {
                        MD_UB = float.Parse(dicEnergy[i].Split('|')[0]);
                    }
                }
            }

            for (int i = 0; i < YJ.Count; i++)
            {
                if (Stop) return;
                string msg = $"读取电表运行状态字{4 + i}（{YJ[i]}相故障状态）";
                string dataFlag = "电表运行状态字" + (4 + i);
                MessageAdd(msg, EnumLogType.提示信息);
                m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);

                if (Stop) return;
                int[] stateCount = ReadEventLogInfo2(YJ[i], "事件产生前");
                if (Stop) return;
                StartEventLog(YJ[i]);
                if (Stop) return;
                int[] endCount = ReadEventLogInfo2(YJ[i], "事件产生后");
                for (int j = 0; j < endCount.Length; j++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (arrResult[j] && endCount[j] <= stateCount[j])
                    {
                        arrResult[j] = false;
                    }
                }
            }
            if (Stop) return;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (arrResult[i])
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
            }
            RefUIData("结论");

        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog(Cus_PowerYuanJian yuanJian)
        {
            if (Stop) return;
            string msg;
            string dataFlag;
            string logInfo;
            //获取电流值
            float xib = OneMeterInfo.GetIb()[0];
            switch (yuanJian)
            {
                case Cus_PowerYuanJian.B:
                    logInfo = "B相过压事件";
                    msg = "读取电表运行状态字5（B相故障状态）";
                    dataFlag = "电表运行状态字5";
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB + 10.0F, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
                case Cus_PowerYuanJian.C:
                    logInfo = "C相过压事件";
                    msg = "读取电表运行状态字6（C相故障状态）";
                    dataFlag = "电表运行状态字6";
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB + 30.0F, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
                default: //1或其它
                    logInfo = "A相过压事件";
                    msg = "读取电表运行状态字4（A相故障状态）";
                    dataFlag = "电表运行状态字4";
                    PowerOn(OneMeterInfo.MD_UB + 30.0F, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
            }
            MessageAdd(logInfo, EnumLogType.提示信息);

            WaitTime(logInfo, 65);

            if (Stop) return;
            MessageAdd(msg, EnumLogType.提示信息);
            m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);

            if (Stop) return;
            PowerOn();

            WaitTime("恢复电压,等待" + logInfo + "记录产生", 65);
        }

        private int[] ReadEventLogInfo2(Cus_PowerYuanJian yuanJian, string name)
        {
            if (Stop) return null;
            int[] bResult = new int[MeterNumber];
            MessageAdd("读取过压记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData(yuanJian.ToString() + "相过压总次数");
            string[] eventTimeStart = new string[MeterNumber];
            string[] eventTimeEnd = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                MessageAdd("过压记录开始时间", EnumLogType.提示信息);
                eventTimeStart = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}相过压产生时刻", 1, yuanJian.ToString()));

                if (Stop) return null;
                MessageAdd(string.Format("读取上【{0:D2}】次【{1}】相过压记录结束时间", 1, yuanJian.ToString()), EnumLogType.提示信息);
                eventTimeEnd = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}相过压结束时刻", 1, yuanJian.ToString()));
            }
            else
            {
                MessageAdd(string.Format("读取上【{0:D2}】次【{1}】过压记录", 1, yuanJian.ToString()), EnumLogType.提示信息);

                List<string> oad = new List<string>();  //3002-电能表失压事件，07-A相事件记录表
                switch (yuanJian)
                {
                    case Cus_PowerYuanJian.H:
                        break;
                    case Cus_PowerYuanJian.A:
                        oad.Add("30020700");
                        break;
                    case Cus_PowerYuanJian.B:
                        oad.Add("30020800");
                        break;
                    case Cus_PowerYuanJian.C:
                        oad.Add("30020900");
                        break;
                    default:
                        break;
                }
                Dictionary<int, List<object>> DicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref DicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    eventTimeStart[i] = "00000000000000";  //默认值
                    eventTimeEnd[i] = "00000000000000";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                            eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                        if (dic[i].ContainsKey("201E0200"))  //201E-事件产生时间 02-参数yyyyMMddhhmmss
                            eventTimeStart[i] = dic[i]["201E0200"][0].ToString();
                        if (dic[i].ContainsKey("20200200"))  //2020-事件结束时间 02-参数yyyyMMddhhmmss
                            eventTimeEnd[i] = dic[i]["20200200"][0].ToString();
                    }
                }
            }

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                string status = "未过压";
                if (!String.IsNullOrEmpty(m_strEventStatus[j]))
                {
                    int chr = Convert.ToInt32(m_strEventStatus[j], 16);
                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                    { if ((chr & 0x04) == 0x04) status = "过压"; }
                    else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                    { if ((chr & 0x04) == 0x2000) status = "过压"; }
                }

                string dateTimeStart = "未产生";
                string dateTimeEnd = "未产生";

                if (eventTimeStart[j] != "00000000000000")
                {
                    dateTimeStart = DateTime.ParseExact(eventTimeStart[j], "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (eventTimeStart[j] != "00000000000000")
                {
                    dateTimeEnd = DateTime.ParseExact(eventTimeEnd[j], "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                int c = 0;
                string NumberOfTime = c.ToString();
                if (!string.IsNullOrWhiteSpace(eventTimes[j]))
                {
                    c = int.Parse(eventTimes[j]);
                }


                string Cus_PowerYuanJianName;
                if (YJ.Count < 1)
                {
                    Cus_PowerYuanJianName = " ";
                }
                else
                {
                    Cus_PowerYuanJianName = yuanJian.ToString() + ":";
                    dateTimeStart = "(" + dateTimeStart + ")";
                    dateTimeEnd = "(" + dateTimeEnd + ")";
                    NumberOfTime = "(" + c + ")";
                }

                ResultDictionary[name + "事件状态"][j] += $"{Cus_PowerYuanJianName}{status}{m_strEventStatus[j]}";
                ResultDictionary[name + "事件发生时刻"][j] += $"{Cus_PowerYuanJianName}{dateTimeStart}";
                ResultDictionary[name + "事件结束时刻"][j] += $"{Cus_PowerYuanJianName}{dateTimeEnd}";
                ResultDictionary[name + "总次数"][j] += $"{Cus_PowerYuanJianName}{NumberOfTime}";
                bResult[j] = c;
            }

            RefUIData(name + "事件状态");
            RefUIData(name + "事件发生时刻");
            RefUIData(name + "事件结束时刻");
            RefUIData(name + "总次数");
            return bResult;

        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //List<string> list = new List<string>();
            YJ.Add(Cus_PowerYuanJian.A);
            if (OneMeterInfo.MD_WiringMode == "三相四线") YJ.Add(Cus_PowerYuanJian.B);
            if (OneMeterInfo.MD_WiringMode != "单相") YJ.Add(Cus_PowerYuanJian.C);
            MD_UB = OneMeterInfo.MD_UB;
            ResultNames = new string[] { "事件产生前事件状态", "事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件状态", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
        private bool InitEquipment()
        {
            if (Stop) return false;
            if (!DAL.Config.ConfigHelper.Instance.IsITOMeter)
            {
                return true;
            }
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }

            return true;
        }

    }
}