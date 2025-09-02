using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 需量超限事件记录
    /// </summary>
    class EL_TransDemand : VerifyBase
    {
        private string[] m_strEventStatus = null;

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
                MessageAdd( "读取电表运行状态字7(合相故障状态)", EnumLogType.提示信息);
                m_strEventStatus = MeterProtocolAdapter.Instance.ReadData("电表运行状态字7");

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
            //获取电流值
            float ximax = OneMeterInfo.GetIb()[1];
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, ximax * 1.3f, ximax * 1.3f, ximax * 1.3f, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            MessageAdd( "需量超限事件", EnumLogType.提示信息);

            WaitTime( "需量超限事件",960);

            if (Stop) return;
            MessageAdd( "读取电表运行状态字7(合相故障状态)", EnumLogType.提示信息);
            m_strEventStatus = MeterProtocolAdapter.Instance.ReadData("电表运行状态字7");

            //源按照正常输出
            if (Stop) return;
            PowerOn();

            WaitTime( "恢复电压,等待需量超限事件记录产生",65);
        }

        private bool[] ReadEventLogInfo( int testNum, int happen)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            if (Stop) return null;
            MessageAdd( "读取需量超限记录总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("需量超限总次数记录");

            for (int curNum = testNum; curNum >= 1; curNum--)
            {
                if (Stop) return null;
                MessageAdd( string.Format("读取(上{0}次）需量超限记录", curNum), EnumLogType.提示信息);
                //string dataFlag = "031201" + Convert.ToString(curNum, 16).PadLeft(2, '0');
                string[] timeStart = new string[MeterNumber];
                string[] timeEnd = new string[MeterNumber];
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    string[] eventValue = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}次)正向有功需量超限记录", curNum));
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        string tmp = eventValue[i].PadLeft(40, '0');
                        timeStart[i] = tmp.Substring(0, 12);
                        timeEnd[i] = tmp.Substring(12, 12);
                    }

                }
                else if (OneMeterInfo.DgnProtocol.ClassName== "CDLT698")
                {
                    List<string> oad = new List<string> { "30090200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, curNum, rcsd, ref dicObj);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        timeStart[i] = "";
                        timeEnd[i] = "";
                        if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                        {
                            if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                                eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                            if (dic[i].ContainsKey("201E0200"))
                                timeStart[i] = dic[i]["201E0200"][0].ToString();
                            if (dic[i].ContainsKey("20200200"))
                                timeEnd[i] = dic[i]["20200200"][0].ToString();
                        }
                    }
                }
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;

                    string status = "未需量超限";
                    int chr = m_strEventStatus[j].Length == 0 ? 0 : Convert.ToInt32(m_strEventStatus[j], 16);
                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                    {
                        if ((chr & 0x40) == 0x40) status = "需量超限";
                    }
                    else if (OneMeterInfo.DgnProtocol.ClassName== "CDLT698")
                    {
                        if ((chr & 0x200) == 0x200) status = "需量超限";
                    }

                    if (happen == 1)         //事件发生前
                    {
                        ResultDictionary["事件产生前事件状态"][j] = status;
                        ResultDictionary["事件产生前事件发生时刻"][j] = timeStart[j];
                        ResultDictionary["事件产生前事件结束时刻"][j] = timeEnd[j];
                        ResultDictionary["事件产生前总次数"][j] = eventTimes[j];
                    }
                    else if (happen == 2)    //事件发生后
                    {
                        ResultDictionary["事件产生后事件状态"][j] = status;
                        ResultDictionary["事件产生后事件发生时刻"][j] = timeStart[j];
                        ResultDictionary["事件产生后事件结束时刻"][j] = timeEnd[j];
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