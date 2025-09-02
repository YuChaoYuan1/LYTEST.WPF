using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 功率反向事件记录
    /// </summary>
    class EL_ReversePower : VerifyBase
    {
        private string[] m_strEventStatus = null;

        public override void Verify()
        {
            base.Verify();

            m_strEventStatus = new string[MeterNumber];

            string[] arrParam = Test_Value.Split('|');
            if (arrParam.Length < 6) return;
            int sumTestNum = int.Parse(arrParam[5]);

            //初始化设备
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            bool[] arrResult = new bool[MeterNumber];

            for (int i = 0; i < MeterNumber; i++)
                arrResult[i] = true;

            string dataFlag = "电表运行状态字4";
            string msg = "读取电表运行状态字4（A相故障状态）";
            for (int p = 1; p <= 3; p++)
            {
                if (arrParam[p - 1] == "否") continue;
                if (p == 2)
                {
                    msg = "读取电表运行状态字5（B相故障状态）";
                    dataFlag = "电表运行状态字5";
                }
                else if (p == 3)
                {
                    msg = "读取电表运行状态字6（C相故障状态）";
                    dataFlag = "电表运行状态字6";
                }
                for (int curNum = 1; curNum <= sumTestNum; curNum++)
                {

                    MessageAdd(msg, EnumLogType.提示信息);
                    m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);

                    bool[] beforeLog = ReadEventLogInfo(p, curNum, 1);

                    StartEventLog(p);

                    bool[] afterLog = ReadEventLogInfo(p, curNum, 2);

                    if (Stop) return;

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;

                        if (Stop) return;
                        if (!beforeLog[i] || !afterLog[i]) arrResult[i] = false;
                        ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
                    }
                    RefUIData("结论");
                }
            }

        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private bool StartEventLog(int phase)
        {
            string eventMsg;
            string dataFlag;
            string msg;
            //获取电流值
            float xib = OneMeterInfo.GetIb()[0];
            switch (phase)
            {
                case 2:
                    eventMsg = "B相功率反向事件";
                    msg = "读取电表运行状态字5（B相故障状态）";
                    dataFlag = "电表运行状态字5";
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 0, 240, 120, 245, 60, 245, 50);
                    break;
                case 3:
                    eventMsg = "C相功率反向事件";
                    msg = "读取电表运行状态字6（C相故障状态）";
                    dataFlag = "电表运行状态字6";
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 0, 240, 120, 245, 240, 245, 50);

                    break;
                default:
                    eventMsg = "A相功率反向事件";
                    msg = "读取电表运行状态字4（A相故障状态）";
                    dataFlag = "电表运行状态字4";
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 0, 240, 120, 245, 240, 245, 50);
                    break;
            }
            MessageAdd(eventMsg, EnumLogType.提示信息);

            WaitTime(eventMsg, 65);

            MessageAdd(msg, EnumLogType.提示信息);
            m_strEventStatus = MeterProtocolAdapter.Instance.ReadData(dataFlag);
            //源按照正常输出
            PowerOn();
            WaitTime("正在恢复电压并等待" + eventMsg + "记录产生", 65);

            return true;
        }

        private bool[] ReadEventLogInfo(int phase, int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            string strPhase = "A相";
            if (phase == 2)
                strPhase = "B相";
            else if (phase == 3)
                strPhase = "C相";

            if (Stop) return null;
            MessageAdd(string.Format("读取{0}功率反向记录总次数", strPhase), EnumLogType.提示信息);
            //string dataFlag = "1B" + phase.ToString("D2") + "0001";
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData(strPhase + "功率反向总次数");

            for (int n = testNum; n >= 1; n--)
            {
                if (Stop) return null;
                string[] startTime = new string[MeterNumber];
                string[] endTime = new string[MeterNumber];
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    MessageAdd(string.Format("读取(上{0}次){1}功率反向记录开始时间", n, strPhase), EnumLogType.提示信息);
                    startTime = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}功率反向发生时刻", n, phase));

                    if (Stop) return null;
                    MessageAdd(string.Format("读取(上{0}次){1}功率反向记录结束时间", n, strPhase), EnumLogType.提示信息);
                    endTime = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次){1}功率反向结束时刻", n, phase));
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    MessageAdd(string.Format("读取(上{0}次){1}功率反向记录", n, strPhase), EnumLogType.提示信息);
                    List<string> oad = new List<string> { "30070600" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, n, rcsd, ref dicObj);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        startTime[i] = "00000000000000";
                        endTime[i] = "00000000000000";
                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200"))
                                startTime[i] = dic[i]["201E0200"][0].ToString();
                            if (dic[i].ContainsKey("20200200"))
                                endTime[i] = dic[i]["20200200"][0].ToString();
                        }
                    }
                }

                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;

                    string status = "未功率反向";
                    int chr = m_strEventStatus[j].Length == 0 ? 0 : Convert.ToInt32(m_strEventStatus[j], 16);
                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        if ((chr & 0x40) == 0x40) status = "功率反向";
                    }
                    else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                    { if ((chr & 0x200) == 0x200) status = "功率反向"; }

                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件状态"][j] = status;
                        ResultDictionary["事件产生前事件发生时刻"][j] = startTime[j];
                        ResultDictionary["事件产生前事件结束时刻"][j] = endTime[j];
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件状态"][j] = status;
                        ResultDictionary["事件产生后事件发生时刻"][j] = startTime[j];
                        ResultDictionary["事件产生后事件结束时刻"][j] = endTime[j];
                        ResultDictionary["事件产生后总次数"][j] = eventTimes[j];
                        if (int.Parse(ResultDictionary["事件产生前总次数"][j]) >= int.Parse(eventTimes[j]))
                            bResult[j] = false;
                    }
                }
                if (happen == 1)
                {
                    RefUIData("事件产生前事件状态");
                    RefUIData("事件产生前事件发生时刻");
                    RefUIData("事件产生前事件结束时刻");
                    RefUIData("事件产生前总次数");
                }
                else
                {
                    RefUIData("事件产生后事件状态");
                    RefUIData("事件产生后事件发生时刻");
                    RefUIData("事件产生后事件结束时刻");
                    RefUIData("事件产生后总次数");
                }
            }
            return bResult;
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            List<string> list = new List<string>();
            string[] data = Test_Value.Split('|');
            if (data[0] == "否" && data[1] == "否" && data[2] == "否") return false;
            //string xiang = "A相";
            for (int i = 0; i < 2; i++)
            {
                //if (i == 1) xiang = "B相";
                //if (i == 2) xiang = "C相";
                if (data[i] == "是")
                {
                    list.Add("事件产生前事件状态");
                    list.Add("事件产生前事件发生时刻");
                    list.Add("事件产生前事件结束时刻");
                    list.Add("事件产生前总次数");
                    list.Add("事件产生后事件状态");
                    list.Add("事件产生后事件发生时刻");
                    list.Add("事件产生后事件结束时刻");
                    list.Add("事件产生后总次数");
                }
            }
            list.Add("结论");
            ResultNames = list.ToArray();
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
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