using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// add lsj 20220711
    /// 欠压事件记录
    /// </summary>
    class EL_UnderVoltage : VerifyBase
    {
        private string[] m_strEventStatus = null;
        readonly List<Cus_PowerYuanJian> YJ = new List<Cus_PowerYuanJian>();
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        public override void Verify()
        {
            base.Verify();
            //初始化设备
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

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
                string dataFlag = "电表运行状态字4";
                string msg = $"读取电表运行状态字4(A相故障状态）";
                switch (YJ[i])
                {
                    case Cus_PowerYuanJian.B:
                        msg = $"读取电表运行状态字5(B相故障状态）";
                        dataFlag = "电表运行状态字5";
                        break;
                    case Cus_PowerYuanJian.C:
                        msg = $"读取电表运行状态字6(CA相故障状态）";
                        dataFlag = "电表运行状态字6";
                        break;
                    default:
                        break;
                }
                if (Stop) return;
                MessageAdd(msg, EnumLogType.提示信息);
                m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);
                int[] stateCount = ReadEventLogInfo2(YJ[i], "事件产生前");
                StartEventLog(YJ[i]);
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
        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog(Cus_PowerYuanJian yuanJian)
        {
            string eventMsg;
            //获取电流值
            float xib = OneMeterInfo.GetIb()[0];
            if (Stop) return;
            switch (yuanJian)
            {
                case Cus_PowerYuanJian.B:
                    eventMsg = "B相欠压事件";
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB * 0.75F, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
                case Cus_PowerYuanJian.C:
                    eventMsg = "C相欠压事件";
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB * 0.75F, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
                default: //1或其它
                    eventMsg = "A相欠压事件";
                    PowerOn(OneMeterInfo.MD_UB * 0.75F, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                    break;
            }

            MessageAdd(eventMsg, EnumLogType.提示信息);

            WaitTime(eventMsg, 65);

            //源按照正常输出
            if (Stop) return;
            PowerOn();

            WaitTime("恢复电压,等待" + eventMsg + "记录产生", 65);
        }


        private int[] ReadEventLogInfo2(Cus_PowerYuanJian yuanJian, string name)
        {
            if (Stop) return null;
            int[] bResult = new int[MeterNumber];
            MessageAdd("读取欠压记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData(yuanJian.ToString() + "相欠压总次数");
            string[] eventTimeStart = new string[MeterNumber];
            string[] eventTimeEnd = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                MessageAdd("欠压记录开始时间", EnumLogType.提示信息);
                eventTimeStart = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}相欠压产生时刻", 1, yuanJian.ToString()));

                if (Stop) return null;
                MessageAdd(string.Format("读取上【{0:D2}】次【{1}】相欠压记录结束时间", 1, yuanJian.ToString()), EnumLogType.提示信息);
                eventTimeEnd = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}相欠压结束时刻", 1, yuanJian.ToString()));
            }
            else
            {
                MessageAdd(string.Format("读取上【{0:D2}】次【{1}】欠压记录", 1, yuanJian.ToString()), EnumLogType.提示信息);

                List<string> oad = new List<string>();  //3000-电能表失压事件，07-A相事件记录表
                switch (yuanJian)
                {
                    case Cus_PowerYuanJian.H:
                        break;
                    case Cus_PowerYuanJian.A:
                        oad.Add("30010700");
                        break;
                    case Cus_PowerYuanJian.B:
                        oad.Add("30010800");
                        break;
                    case Cus_PowerYuanJian.C:
                        oad.Add("30010900");
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

                string status = "未欠压";
                int chr = m_strEventStatus[j].Length == 0 ? 0 : Convert.ToInt32(m_strEventStatus[j], 16);
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    if ((chr & 0x02) == 0x02) status = "欠压";
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    if ((chr & 0x4000) == 0x4000) status = "欠压";
                }

                ResultDictionary[name + "事件状态"][j] += yuanJian.ToString() + ":" + status + "  ";
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
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //List<string> list = new List<string>();
            YJ.Add(Cus_PowerYuanJian.A);
            if (OneMeterInfo.MD_WiringMode == "三相四线") YJ.Add(Cus_PowerYuanJian.B);
            if (OneMeterInfo.MD_WiringMode != "单相") YJ.Add(Cus_PowerYuanJian.C);
            ResultNames = new string[] { "事件产生前事件状态", "事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件状态", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            MessageAdd("开始升电压...", EnumLogType.提示信息);
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