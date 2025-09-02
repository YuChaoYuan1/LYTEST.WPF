using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// \add lsj 20220711
    /// 校时事件记录
    /// </summary>
    class EL_CalibrationTime : VerifyBase
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
            int testCount = int.Parse(arrParam[2]);

            //初始化设备
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                arrResult[i] = true;

            for (int curNo = 1; curNo <= testCount; curNo++)
            {
                if (Stop) return;
                 bool[] beforeLog = ReadEventLogInfo( curNo, 1);

                if (Stop) return;
                StartEventLog();

                if (Stop) return;
                 bool[] afterLog = ReadEventLogInfo( curNo, 2);

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
        private void StartEventLog()
        {
            Identity();
            MessageAdd("校时事件", EnumLogType.提示信息);
            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;  //读取GPS时间
            if (Stop) return;
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            WaitTime( "校时事件",35);
        }

        private bool[] ReadEventLogInfo( int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd("读取校时记录总次数", EnumLogType.提示信息);
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("校时总次数");

            for (int curNum = testNum; curNum >= 1; curNum--)
            {
                if (Stop) return null;
                MessageAdd(string.Format("读取上【{0}】次校时记录", curNum.ToString("D2")), EnumLogType.提示信息);

                string[] timeStart = new string[MeterNumber];
                string[] timeEnd = new string[MeterNumber];

                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    string[] eventTime = MeterProtocolAdapter.Instance.ReadData("(上1次)校时记录");
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        string tmp = eventTime[i].PadLeft(32, '0');

                        timeStart[i] = tmp.Substring(12, 12);
                        timeEnd[i] = tmp.Substring(0, 12);
                    }
                }
                else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {
                    List<string> oad = new List<string>() { "30160200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, curNum, rcsd, ref dicObj);
                    for (int i = 0;     i < MeterNumber; ++i)
                    {
                        timeStart[i] = "";
                        timeEnd[i] = "";

                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200"))  //事件发生时间
                                timeStart[i] = dic[i]["201E0200"][0].ToString();

                            if (dic[i].ContainsKey("20200200") && dic[i]["20200200"].Count > 0)  //事件结束时间
                                timeEnd[i] = dic[i]["20200200"][0].ToString();
                        }
                    }
                }
                for (int i= 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (happen == 1 || curNum > 1)
                    {
                        ResultDictionary["事件产生前事件发生时刻"][i] = timeStart[i];
                        ResultDictionary["事件产生前事件结束时刻"][i] = timeEnd[i];
                        ResultDictionary["事件产生前总次数"][i] = eventTimes[i];
                    }

                    if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件发生时刻"][i] = timeStart[i];
                        ResultDictionary["事件产生后事件结束时刻"][i] = timeEnd[i];
                        ResultDictionary["事件产生后总次数"][i] = eventTimes[i];
                        if (int.Parse(ResultDictionary["事件产生前总次数"][i]) >= int.Parse(eventTimes[i]))
                            bResult[i] = false;
                    }
                }
                if (happen == 1)
                {
                    RefUIData("事件产生前事件发生时刻");
                    RefUIData("事件产生前事件结束时刻");
                    RefUIData("事件产生前总次数");
                }
                else
                {
                    RefUIData("事件产生后事件发生时刻");
                    RefUIData("事件产生后事件结束时刻");
                    RefUIData("事件产生后总次数");
                }
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
            ResultNames = new string[] {"事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
            return true;
        }
    }
}