using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// add lsj 20220711
    /// 需量清零事件记录
    /// </summary>
    class EL_ClearDemand : VerifyBase
    {
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {
            base.Verify();

            string[] arrParam = Test_Value.Split('|');
            if (arrParam.Length < 3) return;
            int sumTestNum = int.Parse(arrParam[2]);

            //初始化设备
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                arrResult[i] = true;

            for (int curNum = 1; curNum <= sumTestNum; curNum++)
            {
                if (Stop) return;
                bool[] beforeLog = ReadEventLogInfo( curNum, 1);
                if (Stop) return;
                StartEventLog();

                if (Stop) return;
                bool[] afterLog = ReadEventLogInfo( curNum, 2);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (!beforeLog[i] || !afterLog[i])
                        arrResult[i] = false;
                    ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
                }
                RefUIData("结论");
            }

        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog()
        {
            Identity();
            MessageAdd("需量清零事件", EnumLogType.提示信息);
            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;
            //读取GPS时间
            if (Stop) return;
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            if (Stop) return;
            MeterProtocolAdapter.Instance.ClearDemand();
            WaitTime( "需量清零事件",65); //倒计时65秒
        }

        private bool[] ReadEventLogInfo(int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd("读取需量清零记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("需量清零总次数");

            for (int i = testNum; i >= 1; i--)
            {
                if (Stop) return null;
                MessageAdd(string.Format("读取上({0:D2})次需量清零记录", i), EnumLogType.提示信息);
                string[] eventTime = new string[MeterNumber];
                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    eventTime = MeterProtocolAdapter.Instance.ReadData("(上" + i + "次)需量清零记录");
                }
                else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {
                    List<string> oad = new List<string> { "30140200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, i, rcsd, ref dicObj);
                    for (int j = 0; j < MeterNumber; ++j)
                    {
                        eventTime[j] = "";
                        if (MeterInfo[j].YaoJianYn&& dic.ContainsKey(j) && dic[j] != null)
                        {
                            if (dic[j].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[j] = (Convert.ToInt32(dic[j]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[j].ContainsKey("201E0200"))  //事件发生时间
                                eventTime[j] = dic[j]["201E0200"][0].ToString();

                            if (dic[j].ContainsKey("00102201")) //正向有功电能
                                eventTime[j] += dic[j]["00102201"][0].ToString();
                        }
                    }
                }

                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    string timeStart = "";
                    string energy = "";
                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                    {
                        eventTime[j] = eventTime[j].PadLeft(388, '0');
                        timeStart = eventTime[j].Substring(eventTime[j].Length - 12, 12);
                        energy = eventTime[j].Substring(eventTime[j].Length - 26, 6);
                    }
                    else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                    {
                        eventTime[j] = eventTime[j].PadRight(17, '0');
                        timeStart = eventTime[j].Substring(0, 14);
                        energy = eventTime[j].Substring(14, eventTime[j].Length - 14);
                    }

                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件发生时刻"][j] = timeStart;
                        ResultDictionary["事件产生前需量清零正向有功需量"][j] = energy;
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件发生时刻"][j] = timeStart;
                        ResultDictionary["事件产后前需量清零正向有功需量"][j] = energy;
                        ResultDictionary["事件产生后总次数"][j] = eventTimes[j];
                        if (int.Parse(ResultDictionary["事件产生前总次数"][j]) >= int.Parse(eventTimes[j]))
                            bResult[j] = false;
                    }
                }
                if (happen == 1)
                {
                    RefUIData("事件产生前事件发生时刻");
                    RefUIData("事件产生前需量清零正向有功需量");
                    RefUIData("事件产生前总次数");
                }
                else
                {
                    RefUIData("事件产生后事件发生时刻");
                    RefUIData("事件产后前需量清零正向有功需量");
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
            ResultNames = new string[] { "事件产生前事件发生时刻", "事件产生前需量清零正向有功需量", "事件产生前总次数", "事件产生后事件发生时刻", "事件产后前需量清零正向有功需量", "事件产生后总次数", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
        private bool InitEquipment()
        {
            if (Stop) return false;
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