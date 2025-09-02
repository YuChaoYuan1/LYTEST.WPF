using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 编程事件记录
    /// </summary>
    class EL_Program : VerifyBase
    {

        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        public override void Verify()
        {
            base.Verify();

            string[] arrParam = Test_Value.Split('|');
            if (arrParam.Length < 3) return;
            int sumTestNum = int.Parse(arrParam[2]);


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
            if (Stop) return;
            if (MeterInfo[FirstIndex].DgnProtocol.HaveProgrammingkey)
            {
                MessageBox.Show("请打开电能表编程开关后点击[确定]", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (Stop) return;
            Identity();
            MessageAdd("编程事件", EnumLogType.提示信息);

            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间

            if (Stop) return;
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            WaitTime("编程事件", 64);
        }

        private bool[] ReadEventLogInfo(int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd("读取编程记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("编程总次数");
            string[] timeStart = new string[MeterNumber];
            string[] dataFlag1 = new string[MeterNumber];
            for (int curNum = testNum; curNum >= 1; curNum--)
            {
                if (Stop) return null;
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    MessageAdd(string.Format("读取(上{0})次编程记录", curNum), EnumLogType.提示信息);
                    string[] eventTime = MeterProtocolAdapter.Instance.ReadData("(上1次)编程记录");
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        string tmp = eventTime[i].PadLeft(100, '0');
                        timeStart[i] = tmp.Substring(tmp.Length - 12, 12);
                        dataFlag1[i] = tmp.Substring(tmp.Length - 28, 8);
                    }
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    List<string> oad = new List<string> { "30120200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, curNum, rcsd, ref dicObj);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        timeStart[i] = "";
                        dataFlag1[i] = "";
                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = ((Convert.ToInt32(dic[i]["20220200"][0]) + 1)).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200"))
                                timeStart[i] = dic[i]["201E0200"][0].ToString();
                            if (dic[i].ContainsKey("33020206") && dic[i]["33020206"].Count > 0) //事件上报状态--,部分表无事件上报状态
                                dataFlag1[i] = dic[i]["33020206"][0].ToString();
                        }
                    }
                }
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;

                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件发生时刻"][j] = timeStart[j];
                        ResultDictionary["事件产生前事件记录操作标识"][j] = dataFlag1[j];
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件发生时刻"][j] = timeStart[j];
                        ResultDictionary["事件产生后事件记录操作标识"][j] = dataFlag1[j];
                        ResultDictionary["事件产生后总次数"][j] = eventTimes[j];
                        if (int.Parse(ResultDictionary["事件产生前总次数"][j]) >= int.Parse(eventTimes[j]))
                            bResult[j] = false;
                    }
                }
                if (happen == 1)
                {
                    RefUIData("事件产生前事件发生时刻");
                    RefUIData("事件产生前事件记录操作标识");
                    RefUIData("事件产生前总次数");
                }
                else
                {
                    RefUIData("事件产生后事件发生时刻");
                    RefUIData("事件产生后事件记录操作标识");
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
            ResultNames = new string[] { "事件产生前事件发生时刻", "事件产生前事件记录操作标识", "事件产生前总次数", "事件产生后事件发生时刻", "事件产生后事件记录操作标识", "事件产生后总次数", "结论" };
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