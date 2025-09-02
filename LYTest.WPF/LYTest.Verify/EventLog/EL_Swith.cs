using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 跳闸合闸事件记录
    /// </summary>
    class EL_Swith : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            string[] beforeLog = ReadEventLogInfo(1);

            if (Stop) return;
            StartEventLog();

            if (Stop) return;
            string[] afterLog = ReadEventLogInfo(2);

            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                arrResult[i] = true;

                if (!MeterInfo[i].YaoJianYn) continue;

                if (beforeLog[i].IndexOf("|") == -1 || afterLog[i].IndexOf("|") == -1)
                {
                    arrResult[i] = false;
                }
                else
                {
                    string[] arrBefore = beforeLog[i].Split('|');
                    string[] arrAfter = afterLog[i].Split('|');

                    if ((arrBefore.Length <= 0) || (arrAfter.Length <= 0))
                        arrResult[i] = false;
                    else if (string.IsNullOrEmpty(arrBefore[0]) || string.IsNullOrEmpty(arrAfter[0])
                          || string.IsNullOrEmpty(arrBefore[2]) || string.IsNullOrEmpty(arrAfter[2]))
                        arrResult[i] = false;
                    else if (Convert.ToInt32(arrBefore[0]) >= Convert.ToInt32(arrAfter[0])
                        && Convert.ToInt32(arrBefore[2]) >= Convert.ToInt32(arrAfter[2]))
                        arrResult[i] = false;
                }

                ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 开始切换
        /// </summary>
        /// <returns></returns>
        private void StartEventLog()
        {
            if (Stop) return;
            Identity();
            MessageAdd("跳闸合闸事件", EnumLogType.提示信息);

            if (Stop) return;

            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; ;  //读取GPS时间

            if (Stop) return;
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            if (Stop) return;
            SendCostCommand(Cus_EncryptionTrialType.远程拉闸);

            if (Stop) return;
            WaitTime("跳闸事件", 10);

            if (Stop) return;
            SendCostCommand(Cus_EncryptionTrialType.远程合闸);

            if (Stop) return;
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0.5f, 0.5f, 0.5f, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

            WaitTime("合闸事件", 10);

        }

        private string[] ReadEventLogInfo(int happen)
        {

            if (Stop) return null;
            MessageAdd("读取跳闸总次数", EnumLogType.提示信息);
            string[] eventTimes1 = MeterProtocolAdapter.Instance.ReadData("跳闸总次数");
            string[] eventTimes2 = MeterProtocolAdapter.Instance.ReadData("合闸总次数");
            if (Stop) return null;
            MessageAdd("读取上1次跳闸记录", EnumLogType.提示信息);
            string[] startTime1 = new string[MeterNumber];
            //string[] endTime1 = new string[MeterNumber];

            string[] startTime2 = new string[MeterNumber];
            //string[] endTime2 = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
            }
            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
            {
                List<string> oad = new List<string> { "301F0200" };//跳闸
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    startTime1[i] = "00000000000000";
                    //endTime1[i] = "00000000000000";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                            eventTimes1[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                        if (dic[i].ContainsKey("201E0200")) //事件开始时间
                            startTime1[i] = dic[i]["201E0200"][0].ToString();
                        //if (dic[i].ContainsKey("20200200") && dic[i]["20200200"].Count > 0) //事件结果时间
                        //    endTime1[i] = dic[i]["20200200"][0].ToString();
                    }
                }
                oad = new List<string> { "30200200" };//合闸
                dicObj = new Dictionary<int, List<object>>();
                rcsd = new List<string>();
                dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    startTime2[i] = "00000000000000";
                    //endTime2[i] = "00000000000000";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                            eventTimes2[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                        if (dic[i].ContainsKey("201E0200")) //事件开始时间
                            startTime2[i] = dic[i]["201E0200"][0].ToString();
                        //if (dic[i].ContainsKey("20200200") && dic[i]["20200200"].Count > 0) //事件结果时间
                        //    endTime2[i] = dic[i]["20200200"][0].ToString();
                    }
                }

            }

            string[] result = new string[MeterNumber];

            string name = happen == 1 ? "事件产生前" : "事件产生后";
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name+"跳闸事件发生时刻"][j] = startTime1[j];
                ResultDictionary[name+"跳闸总次数"][j] = eventTimes1[j];
                ResultDictionary[name+"合闸事件发生时刻"][j] = startTime2[j];
                ResultDictionary[name+"合闸总次数"][j] = eventTimes2[j];
                result[j] = eventTimes1[j] + "|" + startTime1[j] + "|" + eventTimes2[j] + "|" + startTime2[j];
            }
            RefUIData(name+"跳闸事件发生时刻");
            RefUIData(name+"跳闸总次数");
            RefUIData(name+"合闸事件发生时刻");
            RefUIData(name+"合闸总次数");

            return result;
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "事件产生前跳闸事件发生时刻", "事件产生前跳闸总次数", "事件产生后跳闸事件发生时刻", "事件产生后跳闸总次数", "事件产生前合闸事件发生时刻", "事件产生前合闸总次数", "事件产生后合闸事件发生时刻", "事件产生后合闸总次数", "结论" };
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