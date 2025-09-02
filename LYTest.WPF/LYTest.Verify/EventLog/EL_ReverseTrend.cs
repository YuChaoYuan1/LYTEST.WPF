using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 潮流反向事件记录
    /// </summary>
    class EL_ReverseTrend : VerifyBase
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
                bool[] beforeLog = ReadEventLogInfo(curNum, 1);

                if (Stop) return;
                StartEventLog();

                if (Stop) return;
                bool[] afterLog = ReadEventLogInfo(curNum, 2);

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
            //获取电流值
            float ximax = OneMeterInfo.GetIb()[1];
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, ximax, ximax, ximax, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            MessageAdd("潮流反向事件", EnumLogType.提示信息);

            WaitTime("潮流反向事件", 65);

            if (Stop) return;
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, ximax, ximax, ximax, Cus_PowerYuanJian.H, PowerWay.正向有功, "-1.0");
            WaitTime("潮流反向事件并等待事件发生", 65);


            //源按照正常输出
            if (Stop) return;
            PowerOn();
            WaitTime("恢复电压并等待潮流反向事件记录产生", 65);
        }

        private bool[] ReadEventLogInfo(int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd("读取潮流反向记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("潮流反向总次数");

            for (int n = testNum; n >= 1; n--)
            {
                if (Stop) return null;
                MessageAdd(string.Format("读取(上{0}次)潮流反向记录", n), EnumLogType.提示信息);

                string[] startTime = new string[MeterNumber];
                string[] powerWay = new string[MeterNumber];
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    string[] eventValue = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次)潮流反向记录", n));

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        string tmp = eventValue[i].PadLeft(182, '0');
                        startTime[i] = tmp.Substring(tmp.Length - 12, 12);
                        powerWay[i] = tmp.Substring(tmp.Length - 14, 2);
                    }
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    List<string> oad = new List<string> { "30070200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, n, rcsd, ref dicObj);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        startTime[i] = "";
                        powerWay[i] = "";
                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200"))
                                startTime[i] = dic[i]["201E0200"][0].ToString();
                            if (dic[i].ContainsKey("20200200"))  //事件结束时间
                                powerWay[i] = dic[i]["20200200"][0].ToString();
                        }
                    }
                }


                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件发生时刻"][j] = startTime[j];
                        ResultDictionary["事件产生前总有功功率方向"][j] = powerWay[j];
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件发生时刻"][j] = startTime[j];
                        ResultDictionary["事件产生后总有功功率方向"][j] = powerWay[j];
                        ResultDictionary["事件产生后总次数"][j] = eventTimes[j]; 
                    }
                }
                if (happen == 1)
                {
                    RefUIData("事件产生前事件发生时刻");
                    RefUIData("事件产生前总有功功率方向");
                    RefUIData("事件产生前总次数");
                }
                else
                {
                    RefUIData("事件产生后事件发生时刻");
                    RefUIData("事件产生后总有功功率方向");
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
            ResultNames = new string[] { "事件产生前事件发生时刻", "事件产生前总有功功率方向", "事件产生前总次数", "事件产生后事件发生时刻", "事件产生后总有功功率方向", "事件产生后总次数", "结论" };
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