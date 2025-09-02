using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// add lsj 20220711
    /// 电流不平衡事件记录
    /// </summary>
    class EL_ImbalanceI : VerifyBase
    {

        private string[] eventStatus = null;
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
                ////读取电表状态字7 不平衡状态
                if (Stop) return;
                MessageAdd( "读取电表运行状态字7（合相故障状态）", EnumLogType.提示信息);
                eventStatus = MeterProtocolAdapter.Instance.ReadData("电表运行状态字7");

                //读取事件记录
                if (Stop) return;
                bool[] beforeLogs = ReadEventLogInfo( curNum, 1);

                //产生电流不平衡
                if (Stop) return;
                //获取电流值
                float xib = OneMeterInfo.GetIb()[0];
                PowerOn(OneMeterInfo.MD_UB , OneMeterInfo.MD_UB , OneMeterInfo.MD_UB , xib * 0.02F, xib , xib , Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
                MessageAdd( "电流不平衡事件", EnumLogType.提示信息);

                //倒计时65秒
                if (Stop) return;
                WaitTime( "电流不平衡事件",65);

                //读取电表状态字7 不平衡状态
                if (Stop) return;
                MessageAdd( "读取电表运行状态字7（合相故障状态）", EnumLogType.提示信息);
                eventStatus = MeterProtocolAdapter.Instance.ReadData("电表运行状态字7");

                //恢复平衡状态
                if (Stop) return;
                PowerOn();
                //倒计时65秒
                if (Stop) return;
                WaitTime( "恢复电压并等待电流不平衡事件记录产生",65);

                //读取不平衡事件
                if (Stop) return;
                bool[] afterLogs = ReadEventLogInfo( curNum, 2);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (!beforeLogs[i] || !afterLogs[i])
                        arrResult[i] = false;
                    ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
                }
                RefUIData("结论");
            }

        }

        private bool[] ReadEventLogInfo( int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd( "读取电流不平衡记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("电流不平衡总次数");

            string[] eventTimeStart = new string[MeterNumber];
            string[] eventTimeEnd = new string[MeterNumber];
            for (int curNum = testNum; curNum >= 1; curNum--)
            {
                if (OneMeterInfo.DgnProtocol.ClassName== "CDLT6452007")
                {

                    if (Stop) return null;
                    MessageAdd( string.Format("读取上【{0}】次电流不平衡记录开始时间", curNum), EnumLogType.提示信息);
                    eventTimeStart = MeterProtocolAdapter.Instance.ReadData("(上" + curNum + "次)电压不平衡发生时刻");

                    if (Stop) return null;
                    MessageAdd( string.Format("读取上【{0}】次电流不平衡记录结束时间", curNum), EnumLogType.提示信息);
                    eventTimeEnd = MeterProtocolAdapter.Instance.ReadData("(上" + curNum + "次)电压不平衡结束时刻");
                }
                else if (OneMeterInfo.DgnProtocol.ClassName== "CDLT698")
                {
                    if (Stop) return null;
                    List<string> oad = new List<string>() { "301E0200" }; //电能表电流不平衡事件--事件记录表
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, curNum, rcsd, ref dicObj);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        eventTimeStart[i] = "00000000000000";
                        eventTimeEnd[i] = "00000000000000";
                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200"))  //事件发生时刻
                                eventTimeStart[i] = dic[i]["201E0200"][0].ToString();
                            if (dic[i].ContainsKey("20200200")) //事件结束时刻
                                eventTimeEnd[i] = dic[i]["20200200"][0].ToString();
                        }
                    }
                }

                if (Stop) return null;
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    int chr = eventStatus[j].Length == 0 ? 0 : Convert.ToInt32(eventStatus[j], 16);
                    string status = "电流平衡";

                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007" && (chr & 0x0008) == 0x0008)
                        status = "电流不平衡";
                    else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698" && (chr & 0x1000) == 0x1000)
                        status = "电流不平衡";
                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件状态"][j] = status;
                        ResultDictionary["事件产生前事件发生时刻"][j] = eventTimeStart[j];
                        ResultDictionary["事件产生前事件结束时刻"][j] = eventTimeEnd[j];
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件状态"][j] = status;
                        ResultDictionary["事件产生后事件发生时刻"][j] = eventTimeStart[j];
                        ResultDictionary["事件产生后事件结束时刻"][j] = eventTimeEnd[j];
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
            ResultNames = new string[] { "事件产生前事件状态", "事件产生前事件发生时刻", "事件产生前事件结束时刻", "事件产生前总次数", "事件产生后事件状态", "事件产生后事件发生时刻", "事件产生后事件结束时刻", "事件产生后总次数", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
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