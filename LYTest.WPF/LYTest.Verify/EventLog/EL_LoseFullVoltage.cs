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
    /// 全失压事件记录
    /// </summary>
    class EL_LoseFullVoltage : VerifyBase
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
            //获取电流值
            //float xib = OneMeterInfo.GetIb()[0];

            float xIb = Number.GetCurrentByIb("Ib", OneMeterInfo.MD_UA, HGQ);
            PowerOn(OneMeterInfo.MD_UB * 0.2F, OneMeterInfo.MD_UB * 0.2F, OneMeterInfo.MD_UB * 0.2F, xIb * 0.1F, xIb * 0.1F, xIb * 0.1F, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            MessageAdd( "全失压事件", EnumLogType.提示信息);

            if (Stop) return;
            WaitTime( "全失压事件",90);


            if (Stop) return;//源按照正常输出
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            if (Stop) return;
            WaitTime( "恢复电压并等待全失压事件记录产生",80);
        }

        private bool[] ReadEventLogInfo(int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd( "读取全失压记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("全失压总次数,总累计时间");

            for (int curNum = testNum; curNum >= 1; curNum--)
            {
                if (Stop) return null;
                string[] startTime = new string[MeterNumber];
                string[] endTime = new string[MeterNumber];
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    MessageAdd( string.Format("读取(上{0}次)全失压记录", curNum), EnumLogType.提示信息);
                    string[] strEventTime = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次)全失压发生时刻,电流值,结束时刻", curNum));
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        string s = strEventTime[i].PadLeft(30, '0');
                        startTime[i] = s.Substring(18, 12);
                        endTime[i] = s.Substring(0, 12);
                    }
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    List<string> oad = new List<string> { "300D0200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, curNum, rcsd, ref dicObj);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        startTime[i] = "";
                        endTime[i] = "";
                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200")) //事件开始时间
                                startTime[i] = dic[i]["201E0200"][0].ToString();
                            if (dic[i].ContainsKey("20200200")) //事件结束时间
                                endTime[i] = dic[i]["20200200"][0].ToString();
                        }
                    }
                }
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件发生时刻"][j] = startTime[j];
                        ResultDictionary["事件产生前事件结束时刻"][j] = endTime[j];
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件发生时刻"][j] = startTime[j];
                        ResultDictionary["事件产生后事件结束时刻"][j] = endTime[j];
                        ResultDictionary["事件产生后总次数"][j] = eventTimes[j];
                        if (int.Parse(ResultDictionary["事件产生前总次数"][j]) >= int.Parse(eventTimes[j]))
                            bResult[j] = false;
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
            MessageAdd( "开始升电压...", EnumLogType.提示信息);
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