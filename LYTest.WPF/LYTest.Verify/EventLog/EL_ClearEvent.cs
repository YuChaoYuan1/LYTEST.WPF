using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// add lsj 20220711
    /// 事件清零事件记录
    /// </summary>
    class EL_ClearEvent : VerifyBase
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
                bool[] beforeLog = ReadEventLogInfo( 1);

                if (Stop) return;
                StartEventLog(curNum);

                if (Stop) return;
                bool[] afterLog = ReadEventLogInfo(2);

                for (int i = 0; i < MeterNumber; i++)
                {
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
        private void StartEventLog(int testNum)
        {
            Identity();

            if (Stop) return;
            MessageAdd( string.Format("第{0}事件清零事件", testNum), EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.ClearEventLog("FFFFFFFF");
            WaitTime( "事件清零事件",10);//倒计时35秒
        }

        private bool[] ReadEventLogInfo(int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd( "读取事件清零记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("事件清零总次数");

            if (Stop) return null;
            MessageAdd( string.Format("读取上({0:D2})次事件清零记录", 1), EnumLogType.提示信息);
            string[] eventDataFlag = new string[MeterNumber];
            string[] timeStart = new string[MeterNumber];

            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                string[] eventTime = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次)事件清零记录", 1));
                for (int i = 0; i < MeterNumber; ++i)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    string tmp = eventTime[i].PadLeft(28, '0');
                    eventDataFlag[i] = tmp.Substring(0, 16);
                    timeStart[i] = tmp.Substring(16, 12);
                }
            }
            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
            {
                List<string> oad = new List<string> { "30150200" };
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    //eventDataFlag[i] = "FFFFFFFF";
                    timeStart[i] = "00000000000000";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                            eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                        if (dic[i].ContainsKey("201E0200"))//事件发生时刻
                            timeStart[i] = dic[i]["201E0200"][0].ToString();
                        if (dic[i].ContainsKey("330C0206")) //事件清零事件记录 -参数-
                        {
                            eventDataFlag[i] += dic[i]["330C0206"][0].ToString();
                        }

                    }
                }
            }


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (happen == 1)         //事件发生前
                {
                    ResultDictionary["事件产生前事件发生时刻"][i] = timeStart[i];
                    ResultDictionary["事件产生前事件清零数据标识"][i] = eventDataFlag[i];
                    ResultDictionary["事件产生前总次数"][i] = eventTimes[i];
                }
                else if (happen == 2)    //事件发生后
                {
                    ResultDictionary["事件产生后事件发生时刻"][i] = timeStart[i];
                    ResultDictionary["事件产生后事件清零数据标识"][i] = eventDataFlag[i];
                    ResultDictionary["事件产生后总次数"][i] = eventTimes[i];
                    if (int.Parse(ResultDictionary["事件产生前总次数"][i]) >= int.Parse(eventTimes[i]))
                        bResult[i] = false;
                }
            }
            if (happen == 1)
            {
                RefUIData("事件产生前事件发生时刻");
                RefUIData("事件产生前事件清零数据标识");
                RefUIData("事件产生前总次数");
            }
            else
            {
                RefUIData("事件产生后事件发生时刻");
                RefUIData("事件产生后事件清零数据标识");
                RefUIData("事件产生后总次数");
            }
            return bResult;
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "事件产生前事件发生时刻", "事件产生前事件清零数据标识", "事件产生前总次数", "事件产生后事件发生时刻", "事件产生后事件清零数据标识", "事件产生后总次数", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            if (Stop) return false;
            MessageAdd( "开始升电压...", EnumLogType.提示信息);
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }
            return true;
        }
    }
}