using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// add lsj 20220711
    /// 掉电事件记录
    /// </summary>
    class EL_Acdump : VerifyBase
    {
        private string[] eventStatus = null;
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
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

            //Identity(true);
            //List<string> oad = new List<string>() { "30260200" };
            //Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            //List<string> rcsd = new List<string>();
            //Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand  , EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);

            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                arrResult[i] = true;
            //arrResult.Fill(true);
            //WaitTime("恢复电压,等待掉电记录产生", 65);

            for (int curNum = 1; curNum <= sumTestNum; curNum++)
            {
                if (Stop) return;
                MessageAdd("读取电表运行状态字7（合相故障状态）", EnumLogType.提示信息);
                eventStatus = MeterProtocolAdapter.Instance.ReadData("电表运行状态字7");

                if (Stop) return;
                bool[] beforeLog = ReadEventLogInfo(  1);

                if (Stop) return;
                StartEventLog(curNum);

                if (Stop) return;
                bool[] afterLog = ReadEventLogInfo(  2);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (!beforeLog[i] || !afterLog[i])
                        arrResult[i] = false;
                    ResultDictionary["结论"][i] = arrResult[i] ? "合格" :"不合格";
                }
                RefUIData("结论");
            }
        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog(int testNum)
        {
            //获取电流值
            float xib = OneMeterInfo.GetIb()[0];
            PowerOn(OneMeterInfo.MD_UB * 0.55F, OneMeterInfo.MD_UB * 0.55F, OneMeterInfo.MD_UB * 0.55F, xib * 0.04f, xib * 0.04f, xib * 0.04f, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

            MessageAdd("掉电事件", EnumLogType.提示信息);
            WaitTime(string.Format("第{0}次掉电事件并等待发生事件", testNum), 65);

            //源按照正常输出
            if (Stop) return;
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            Thread.Sleep(5000);

            if (Stop) return;
            MessageAdd("读取电表运行状态字7（合相故障状态）", EnumLogType.提示信息);
            eventStatus = MeterProtocolAdapter.Instance.ReadData("电表运行状态字7");

            WaitTime(string.Format("第{0}次恢复电压并等待掉电事件记录产生", testNum), 65);
        }

        private bool[] ReadEventLogInfo( int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd("读取掉电记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("掉电总次数");

            if (Stop) return null;
            MessageAdd(string.Format("读取(上{0}次)掉电记录", 1), EnumLogType.提示信息);
            string[] eventTime = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                eventTime = MeterProtocolAdapter.Instance.ReadData("(上1次)掉电发生时刻,结束时刻");
            }
            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
            {
                List<string> oad = new List<string>() { "30110200" };
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    eventTime[i] = "";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                            eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                        if (dic[i].ContainsKey("201E0200")) //事件发生时间
                            eventTime[i] = dic[i]["201E0200"][0].ToString();

                        if (dic[i].ContainsKey("20200200")) //事件结束时间
                            eventTime[i] += "," + dic[i]["20200200"][0].ToString();
                    }
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {

                if (!MeterInfo[i].YaoJianYn) continue;
                string status = "未掉电";
                string timeStart = "";
                string timeEnd = "";
                int chr = Convert.ToInt32(eventStatus[i], 16);
                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    if ((chr & 0x0020) == 0x0020)
                        status = "掉电";
                    eventTime[i] = eventTime[i].PadLeft(24, '0');
                    timeStart = eventTime[i].Substring(12, 12);
                    timeEnd = eventTime[i].Substring(0, 12);
                }
                else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {
                    if ((chr & 0x0200) == 0x0200)
                        status = "掉电";
                    if (eventTime[i].Split(',').Length > 1)
                    {
                        timeStart = eventTime[i].Split(',')[0];
                        timeEnd = eventTime[i].Split(',')[1];
                    }
                }
                if (happen == 1)         //事件发生前
                {
                    ResultDictionary["事件产生前事件状态"][i] = status;
                    ResultDictionary["事件产生前事件发生时刻"][i] = timeStart;
                    ResultDictionary["事件产生前事件结束时刻"][i] = timeEnd;
                    ResultDictionary["事件产生前总次数"][i] = eventTimes[i];
                }
                else if (happen == 2)    //事件发生后
                {
                    ResultDictionary["事件产生后事件状态"][i] = status;
                    ResultDictionary["事件产生后事件发生时刻"][i] = timeStart;
                    ResultDictionary["事件产生后事件结束时刻"][i] = timeEnd;
                    ResultDictionary["事件产生后总次数"][i] = eventTimes[i];
                    if (int.Parse(ResultDictionary["事件产生前总次数"][i]) >= int.Parse(eventTimes[i]))
                        bResult[i] = false;
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
            return bResult;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
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

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "事件产生前事件状态", "事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件状态", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
            return true;
        }
    }
}