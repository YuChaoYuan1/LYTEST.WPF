using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 断流事件记录
    /// </summary>
    class EL_StopCurrent : VerifyBase
    {
        private string[] m_strEventStatus = null;
        private string[] m_strReportStatus = null;
        readonly List<Cus_PowerYuanJian> YJ = new List<Cus_PowerYuanJian>();

        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {

            base.Verify();
            //string[] arrParam = Test_Value.Split('|');
            //if (arrParam.Length < 6) return;
            //int sumTestNum = int.Parse(arrParam[5]);
            m_strEventStatus = new string[MeterNumber];
            //初始化设备
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            string[] reportChar = null;
            if (!MeterInfo[FirstIndex].DgnProtocol.HaveProgrammingkey && OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                if (Stop) return;
                MessageAdd("读取主动上报模式字", EnumLogType.提示信息);
                reportChar = MeterProtocolAdapter.Instance.ReadData("主动上报模式字");

                if (Stop) return;
                Identity();

                if (Stop) return;
                MessageAdd("设置主动上报模式字", EnumLogType.提示信息);
                MeterProtocolAdapter.Instance.WriteData("主动上报模式字", "FFFFFFFFFFFF");
            }

            bool[] arrResult = new bool[MeterNumber];

            arrResult.Fill(true);
            ResultDictionary["事件产生前事件状态"].Fill("");
            ResultDictionary["事件产生前事件发生时刻"].Fill("");
            ResultDictionary["事件产生前事件结束时刻"].Fill("");
            ResultDictionary["事件产生后事件状态"].Fill("");
            ResultDictionary["事件产生后事件发生时刻"].Fill("");
            ResultDictionary["事件产生后事件结束时刻"].Fill("");
            ResultDictionary["事件产生前总次数"].Fill("");
            ResultDictionary["事件产生后总次数"].Fill("");

            bool[] resoult = new bool[MeterNumber];
            resoult.Fill(true);

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
                    if (resoult[j] && endCount[j] <= stateCount[j])
                    {
                        resoult[j] = false;
                    }
                }
            }
            if (Stop) return;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (resoult[i])
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
            }
            RefUIData("结论");
            if (!MeterInfo[FirstIndex].DgnProtocol.HaveProgrammingkey && OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                Identity();
                MessageAdd("恢复主动上报模式字", EnumLogType.提示信息);
                MeterProtocolAdapter.Instance.WriteData("主动上报模式字", reportChar);
            }

            //bool[] arrResult = new bool[MeterNumber];
            //for (int i = 0; i < MeterNumber; i++)
            //    arrResult[i] = true;

            //if (Stop) return;
            ////获取电流值
            //float xib = OneMeterInfo.GetIb()[0];
            //PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            //WaitTime( "恢复电流并等待掉电记录产生",65);

            //for (int phase = 1; phase <= 3; phase++)
            //{

            //    if (arrParam[phase - 1] == "否") continue;

            //    string msg = "读取电表运行状态字4（A相故障状态）";
            //    string dataFlag = "电表运行状态字4";
            //    switch (phase)
            //    {
            //        case 2:
            //            msg = "读取电表运行状态字5（B相故障状态）";
            //            dataFlag = "电表运行状态字5";
            //            break;
            //        case 3:
            //            msg = "读取电表运行状态字6（C相故障状态）";
            //            dataFlag = "电表运行状态字6";
            //            break;
            //    }
            //    for (int curNum = 1; curNum <= sumTestNum; curNum++)
            //    {
            //        if (Stop) return;
            //        MessageAdd( msg, EnumLogType.提示信息);
            //        m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);

            //        if (Stop) return;
            //        bool[] beforeLog = ReadEventLogInfo(phase, curNum, 1);

            //        if (Stop) return;
            //        StartEventLog(phase);

            //        if (Stop) return;
            //        bool[] afterLog = ReadEventLogInfo(phase, curNum, 2);

            //        for (int i = 0; i < MeterNumber; i++)
            //        {
            //            if (!meterInfo[i].YaoJianYn) continue;

            //            if (!beforeLog[i] || !afterLog[i])
            //                arrResult[i] = false;
            //            ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
            //        }
            //        RefUIData("结论");
            //    }
            //}
        }

        private int[] ReadEventLogInfo2(Cus_PowerYuanJian yuanJian, string name)
        {
            if (Stop) return null;
            int[] bResult = new int[MeterNumber];
            MessageAdd("读取断流记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData(yuanJian.ToString() + "相断流总次数");
            string[] eventTimeStart = new string[MeterNumber];
            string[] eventTimeEnd = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                MessageAdd("断流记录开始时间", EnumLogType.提示信息);
                eventTimeStart = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}相断流产生时刻", 1, yuanJian.ToString()));

                if (Stop) return null;
                MessageAdd(string.Format("读取上【{0:D2}】次【{1}】相断流记录结束时间", 1, yuanJian.ToString()), EnumLogType.提示信息);
                eventTimeEnd = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}相断流结束时刻", 1, yuanJian.ToString()));
            }
            else
            {
                MessageAdd(string.Format("读取上【{0:D2}】次【{1}】断流记录", 1, yuanJian.ToString()), EnumLogType.提示信息);

                List<string> oad = new List<string>();  //3000-电能表失压事件，07-A相事件记录表
                switch (yuanJian)
                {
                    case Cus_PowerYuanJian.H:
                        break;
                    case Cus_PowerYuanJian.A:
                        oad.Add("30060700");
                        break;
                    case Cus_PowerYuanJian.B:
                        oad.Add("30060800");
                        break;
                    case Cus_PowerYuanJian.C:
                        oad.Add("30060900");
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


                string statu = "未断流";
                if (!MeterInfo[j].DgnProtocol.HaveProgrammingkey && m_strReportStatus != null)
                {
                    if (m_strReportStatus[j] != null && m_strReportStatus[j].Length != 0)
                    {
                        string strStatus = Common.ConvertTo2From16(m_strReportStatus[j]);
                        if (strStatus[16] == '1')
                            statu = "断流";
                    }
                }

                int chr = m_strEventStatus[j].Length == 0 ? 0 : Convert.ToInt32(m_strEventStatus[j], 16);
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007" && (chr & 0x01) == 0x01)
                    statu = "断流";
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698" && (chr & 0x8000) == 0x8000)
                    statu = "断流";


                ResultDictionary[name + "事件状态"][j] += yuanJian.ToString() + ":" + statu + "  ";
                ResultDictionary[name + "事件发生时刻"][j] += yuanJian.ToString() + ":" + eventTimeStart[j] + "  ";
                ResultDictionary[name + "事件结束时刻"][j] += yuanJian.ToString() + ":" + eventTimeEnd[j] + "  ";
                int c = 0;
                if (!string.IsNullOrEmpty(eventTimes[j]))
                {
                    c = int.Parse(eventTimes[j]);
                }
                ResultDictionary[name + "总次数"][j] += yuanJian.ToString() + ":" + c.ToString() + "  ";
                bResult[j] = c;
            }

            RefUIData(name + "事件状态");
            RefUIData(name + "事件发生时刻");
            RefUIData(name + "事件结束时刻");
            RefUIData(name + "总次数");
            return bResult;

        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog(Cus_PowerYuanJian yuanJian)
        {
            string eventMsg;
            string dataFlag;
            string msg;
            //获取电流值
            float xib = OneMeterInfo.GetIb()[0];

            if (Stop) return;
            switch (yuanJian)
            {
                case Cus_PowerYuanJian.B:
                    eventMsg = "B相断流事件";
                    msg = "读取电表运行状态字5（B相故障状态）";
                    dataFlag = "电表运行状态字5";
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB * 0.75F, OneMeterInfo.MD_UB, xib, 0, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
                case Cus_PowerYuanJian.C:
                    eventMsg = "C相断流事件";
                    msg = "读取电表运行状态字6（C相故障状态）";
                    dataFlag = "电表运行状态字6";
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB * 0.75F, xib,  xib, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
                default:
                    eventMsg = "A相断流事件";
                    msg = "读取电表运行状态字4（A相故障状态）";
                    dataFlag = "电表运行状态字4";
                    PowerOn(OneMeterInfo.MD_UB * 0.75F, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB , 0, xib, xib,  Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
            }
            MessageAdd( eventMsg, EnumLogType.提示信息);

            WaitTime( eventMsg,65);

            if (!MeterInfo[FirstIndex].DgnProtocol.HaveProgrammingkey)
            {
                if (Stop) return;
                MessageAdd("读取主动上报状态字", EnumLogType.提示信息);
                m_strReportStatus = MeterProtocolAdapter.Instance.ReadData("主动上报状态字");
            }

            if (Stop) return;
            MessageAdd( msg, EnumLogType.提示信息);
            m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);

            //源按照正常输出
            if (Stop) return;
            PowerOn(OneMeterInfo.MD_UB , OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            WaitTime( "恢复电压,等待" + eventMsg + "记录产生",65);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //List<string> list = new List<string>();
            YJ.Add(Cus_PowerYuanJian.A);
            if (OneMeterInfo.MD_WiringMode != "单相") YJ.Add(Cus_PowerYuanJian.B);
            if (OneMeterInfo.MD_WiringMode == "三相四线") YJ.Add(Cus_PowerYuanJian.C);
            ResultNames = new string[] { "事件产生前事件状态", "事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件状态", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            MessageAdd( "开始升电压...", EnumLogType.提示信息);
            if (Stop) return false;
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }
            return true;
        }

    }
}