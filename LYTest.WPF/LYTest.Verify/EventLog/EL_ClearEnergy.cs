using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// add lsj 20220711
    /// 电表清零事件记录
    /// </summary>
    class EL_ClearEnergy : VerifyBase
    {

        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        public override void Verify()
        {
            base.Verify();




            //初始化设备
            if (Stop) return;
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                arrResult[i] = true;

            if (Stop) return;
            float Xib = Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
            PowerOn(Xib);
             WaitTime("正在走电", 15);
            PowerOn();
            WaitTime("正在关闭电流",5);

            //Identity(false);
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
                    string[] arrBefore = beforeLog[i].Split('|');
                    string[] arrAfter = afterLog[i].Split('|');

                    if ((arrBefore.Length <= 0) || (arrAfter.Length <= 0))
                        arrResult[i] = false;
                    else if (Convert.ToInt32(arrBefore[0]) >= Convert.ToInt32(arrAfter[0]))
                        arrResult[i] = false;
                }
                ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog()
        {
            MessageAdd("进行清零", EnumLogType.提示信息);
            Identity();
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间

            MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            //Identity(false);

            if (OneMeterInfo.FKType == 1) //本地表
            {
                string msg = string.Format("准备开始钱包初始化，初始化金额为【{0}】元", Test_Value);
                MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MeterProtocolAdapter.Instance.InitPurse(int.Parse(Test_Value));
            }
            else
            {
                //判断物联表的情况进行一次电量清零--因为物联表一天只能进行三次瞬时冻结
                if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                {
                    MessageAdd("开始电量清零--确定冻结触发", EnumLogType.提示信息);
                    MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
                    MeterProtocolAdapter.Instance.ClearEnergy();
                    MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                    MeterProtocolAdapter.Instance.ClearEnergy();
                    WaitTime("清零", 5);
                }
                else
                {
                    MeterProtocolAdapter.Instance.ClearEnergy();
                    //WaitTime("清零", 5);
                }
            }
            WaitTime("进行清零", 10);
        }

        private string[] ReadEventLogInfo(int happen)
        {
            string[] result = new string[MeterNumber];

            if (Stop) return null;
            MessageAdd("读取电表清零总次数", EnumLogType.提示信息);
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("电表清零总次数");

            if (Stop) return null;
            MessageAdd("读取上1次电表清零记录", EnumLogType.提示信息);
            string[] eventTime = new string[MeterNumber];
            string[] eventEnergy = new string[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                string[] readData = MeterProtocolAdapter.Instance.ReadData("(上1次)电表清零记录");
                for (int i = 0; i < MeterNumber; ++i)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    string tmp = readData[i];
                    if (tmp.Length > 12)
                        eventTime[i] = tmp.Substring(tmp.Length - 12, 12);

                    if (tmp.Length > 28)
                    {
                        eventEnergy[i] = tmp.Substring(tmp.Length - 28, 8);
                        eventEnergy[i] = (Convert.ToDouble(eventEnergy[i]) / 100).ToString("F2");
                    }
                }
            }
            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
            {
                List<string> oad = new List<string>() { "30130200" }; //电能表清零事件--事件记录表
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    eventTime[i] = "00000000000000";
                    eventEnergy[i] = "";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("20220200"))  // 事件记录序号
                            eventTimes[i] = (Convert.ToInt32(dic[i]["20220200"][0]) + 1).ToString(); //由于698没有编程总次数，固以序号表示

                        if (dic[i].ContainsKey("201E0200"))  //事件发生时刻
                            eventTime[i] = dic[i]["201E0200"][0].ToString();
                        if (dic[i].ContainsKey("00102201")) //正向有功电量
                        {
                            eventEnergy[i] = dic[i]["00102201"][0].ToString();
                            eventEnergy[i] = (Convert.ToDouble(eventEnergy[i]) / 100).ToString("F2");
                        }
                    }
                }
            }
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (happen == 1)
                {
                    ResultDictionary["事件产生前事件发生时刻"][i] = eventTime[i];
                    ResultDictionary["事件产生前电量清零正向有功电量"][i] = eventEnergy[i];
                    ResultDictionary["事件产生前总次数"][i] = eventTimes[i];
                }
                else
                {
                    ResultDictionary["事件产生后事件发生时刻"][i] = eventTime[i];
                    ResultDictionary["事件产生后电量清零正向有功电量"][i] = eventEnergy[i];
                    ResultDictionary["事件产生后总次数"][i] = eventTimes[i];
                }
                result[i] = eventTimes[i] + "|" + eventTime[i] + "|" + eventEnergy[i];
            }
            if (happen == 1)
            {
                RefUIData("事件产生前事件发生时刻");
                RefUIData("事件产生前电量清零正向有功电量");
                RefUIData("事件产生前总次数");
            }
            else
            {
                RefUIData("事件产生后事件发生时刻");
                RefUIData("事件产生后电量清零正向有功电量");
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
            ResultNames = new string[] { "事件产生前事件发生时刻", "事件产生前电量清零正向有功电量", "事件产生前总次数", "事件产生后事件发生时刻", "事件产生后电量清零正向有功电量", "事件产生后总次数", "结论" };
            return true;
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
    }
}
