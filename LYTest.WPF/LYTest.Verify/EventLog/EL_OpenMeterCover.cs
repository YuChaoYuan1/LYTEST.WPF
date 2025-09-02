using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 开表盖事件记录
    /// </summary>
    class EL_OpenMeterCover : VerifyBase
    {
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
            for (int i = 0; i < MeterNumber; i++)
                arrResult[i] = true;

            if (Stop) return;
            string[] beforeLog = ReadEventLogInfo(1);

            if (Stop) return;
            StartEventLog();

            if (Stop) return;
            string[] afterLog = ReadEventLogInfo(2);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (beforeLog[i].IndexOf("|") == -1 || afterLog[i].IndexOf("|") == -1)
                {
                    arrResult[i] = false;
                }
                else
                {
                    string[] bf = beforeLog[i].Split('|');
                    string[] af = afterLog[i].Split('|');

                    if ((bf.Length <= 0) || (af.Length <= 0))
                        arrResult[i] = false;
                    else if (string.IsNullOrEmpty(bf[0]) || string.IsNullOrEmpty(af[0]))
                        arrResult[i] = false;
                    else if (Convert.ToInt32(bf[0]) >= Convert.ToInt32(af[0]))
                        arrResult[i] = false;
                }
                ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");

        }

        /// <summary>
        /// 开始切换
        /// </summary>
        /// <param name="strCurItem"></param>
        /// <param name="strSwitchInfo"></param>
        /// <returns></returns>
        private void StartEventLog()
        {
            Identity();
            MessageAdd("进行开盖", EnumLogType.提示信息);



            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间


            if (Stop) return;
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            MessageAdd("用户进行进行开盖...", EnumLogType.提示信息);
            MessageBox.Show("请进行开表盖操作\n操作完成后请按确定键", "开表盖事件", MessageBoxButtons.OK, MessageBoxIcon.Information);

            WaitTime( "进行开盖",3);
        }

        private string[] ReadEventLogInfo(int happen)
        {
            if (Stop) return null;
            MessageAdd("读取电表开盖记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("开表盖总次数");

            if (Stop) return null;
            MessageAdd("读取上1次电表开盖记录", EnumLogType.提示信息);
            string[] startTime = new string[MeterNumber];
            string[] endTime = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                string[] readData = MeterProtocolAdapter.Instance.ReadData("(上1次)开表盖记录");
                for (int i = 0; i < MeterNumber; ++i)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    readData[i] = readData[i].PadLeft(120, '0');
                    startTime[i] = readData[i].Substring(0, 12);
                    endTime[i] = readData[i].Substring(12, 12);
                }
            }
            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
            {
                List<string> oad = new List<string> { "301B0200" };
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
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
                        if (dic[i].ContainsKey("20200200") && dic[i]["20200200"].Count > 0)
                            endTime[i] = dic[i]["20200200"][0].ToString();
                    }
                }
            }
            if (Stop) return null;
            string[] result = new string[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (happen == 1)
                {
                    ResultDictionary["事件产生前事件发生时刻"][i] = startTime[i];
                    ResultDictionary["事件产生前事件结束时刻"][i] = endTime[i];
                    ResultDictionary["事件产生前总次数"][i] = eventTimes[i];
                }
                else
                {
                    ResultDictionary["事件产生后事件发生时刻"][i] = startTime[i];
                    ResultDictionary["事件产生后结束时刻"][i] = endTime[i];
                    ResultDictionary["事件产生后总次数"][i] = eventTimes[i];
                }
                result[i] = eventTimes[i] + "|" + startTime[i] + "|" + endTime[i];

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
            return result;
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
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
