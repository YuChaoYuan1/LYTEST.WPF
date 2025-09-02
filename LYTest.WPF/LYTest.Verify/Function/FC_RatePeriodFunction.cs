using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Function
{

    /// <summary>
    /// 费率时段功能试验
    /// add lsj 20220725
    /// </summary>
    class FC_RatePeriodFunction : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;

            //初始化设备
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            MessageAdd("读取两套时区表切换时间", EnumLogType.提示信息);
            string[] readData = MeterProtocolAdapter.Instance.ReadData("两套时区表切换时间");
            string zoneTime = readData[FirstIndex];//两套时区表切换时间
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (readData[i] == null || readData[i] == "") continue;
                ResultDictionary["两套时区表切换时间"][i] = readData[i];
                if (zoneTime != readData[i])
                {
                    MessageAdd("有表位两套时区表切换时间不一致，试验终止", EnumLogType.错误信息);
                }
            }
            RefUIData("两套时区表切换时间");

            if (Stop) return;
            MessageAdd("读取两套日时段表切换时间", EnumLogType.提示信息);
            readData = MeterProtocolAdapter.Instance.ReadData("两套日时段表切换时间");
            string periodTime = readData[FirstIndex];//两套时段表切换时间
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (readData[i] == null || readData[i] == "") continue;
                ResultDictionary["两套日时段表切换时间"][i] = readData[i];
                if (periodTime != readData[i])
                {
                    MessageAdd("有表位两套时段表切换时间不一致，试验终止", EnumLogType.错误信息);
                }
            }
            RefUIData("两套日时段表切换时间");


            if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
            {
                if (Stop) return;
                MessageAdd("读取约定冻结模式字", EnumLogType.提示信息);
                readData = MeterProtocolAdapter.Instance.ReadData("约定冻结数据模式字");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (readData[i] == null || readData[i] == "") continue;

                    string tmp = Convert.ToString(Convert.ToByte(readData[i].Substring(0, 2), 16), 2).PadLeft(8, '0');
                    string chr = "";
                    if (tmp.Substring(tmp.Length - 1, 1) == "1")
                        chr = "|正向有功";

                    if (tmp.Substring(tmp.Length - 2, 1) == "1")
                        chr += "|反向有功";

                    if (tmp.Substring(tmp.Length - 3, 1) == "1")
                        chr += "|组合无功1";

                    if (tmp.Substring(tmp.Length - 4, 1) == "1")
                        chr += "加组合无功2";

                    if (tmp.Substring(tmp.Length - 5, 1) == "1")
                        chr += "|四象限无功";

                    if (tmp.Substring(tmp.Length - 6, 1) == "1")
                        chr += "|正向有功最大需量";

                    if (tmp.Substring(tmp.Length - 7, 1) == "1")
                        chr += "|反向有功最大需量";
                    ResultDictionary["约定冻结数据模式字"][i] = chr;
                }
                RefUIData("约定冻结数据模式字");

            }

            if (Stop) return;
            MessageAdd("读取(当前)正向有功总电能", EnumLogType.提示信息);
            string[] data = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                if (string.IsNullOrEmpty(data[j])) continue;
                ResultDictionary["当前正向有功电能"][j] = data[j];
            }
            RefUIData("当前正向有功电能");


            //身份认证
            if (Stop) return;
            Identity();

            if (Stop) return;
            string[] switchBefore = ReadSwitch();          //读取上一次切换时间
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                string[] data2 = switchBefore[i].Split('|');
                ResultDictionary["运行时区"][i] = data2[0];
                ResultDictionary["运行时段"][i] = data2[1];
                ResultDictionary["时区表切换时间"][i] = data2[2];
                ResultDictionary["日时段表切换时间"][i] = data2[3];
                ResultDictionary["时区切换正向有功"][i] = data2[4];
                ResultDictionary["日时段切换正向有功"][i] = data2[5];
            }
            RefUIData("运行时区");
            RefUIData("运行时段");
            RefUIData("时区表切换时间");
            RefUIData("日时段表切换时间");
            RefUIData("时区切换正向有功");
            RefUIData("日时段切换正向有功");
            if (Stop) return;
            StartSwitch("进行两套时区表切换", DateTime.Now.AddYears(1));           //先切换时区表时间

            if (Stop) return;
            string[] switchAfter = ReadSwitch();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                string[] data2 = switchAfter[i].Split('|');
                ResultDictionary["时区表切换后运行时区"][i] = data2[0];
                ResultDictionary["时区表切换后运行时段"][i] = data2[1];
                ResultDictionary["时区表切换后时区表切换时间"][i] = data2[2];
                ResultDictionary["时区表切换后时段表切换时间"][i] = data2[3];
                ResultDictionary["时区表切换后时区切换正向有功"][i] = data2[4];
                ResultDictionary["时区表切换后时段切换正向有功"][i] = data2[5];
            }
            RefUIData("时区表切换后运行时区");
            RefUIData("时区表切换后运行时段");
            RefUIData("时区表切换后时区表切换时间");
            RefUIData("时区表切换后时段表切换时间");
            RefUIData("时区表切换后时区切换正向有功");
            RefUIData("时区表切换后时段切换正向有功");
            //进行两套时段表切换
            if (Stop) return;
            Identity(false);

            if (Stop) return;
            StartSwitch("进行两套时段表切换", DateTime.Now.AddYears(1));

            if (Stop) return;
            string[] finshSwitch = ReadSwitch();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                string[] data2 = finshSwitch[i].Split('|');
                ResultDictionary["时段表切换后运行时区"][i] = data2[0];
                ResultDictionary["时段表切换后运行时段"][i] = data2[1];
                ResultDictionary["时段表切换后时区表切换时间"][i] = data2[2];
                ResultDictionary["时段表切换后时段表切换时间"][i] = data2[3];
                ResultDictionary["时段表切换后时区切换正向有功"][i] = data2[4];
                ResultDictionary["时段表切换后时段切换正向有功"][i] = data2[5];
            }
            RefUIData("时段表切换后运行时区");
            RefUIData("时段表切换后运行时段");
            RefUIData("时段表切换后时区表切换时间");
            RefUIData("时段表切换后时段表切换时间");
            RefUIData("时段表切换后时区切换正向有功");
            RefUIData("时段表切换后时段切换正向有功");

            if (Stop) return;
            MessageAdd("恢复两套时区表切换时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("恢复两套时区表切换时间", zoneTime);
            if (Stop) return;
            MessageAdd("恢复两套时段表切换时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("两套日时段表切换时间", periodTime);
            if (Stop) return;
            Identity(false);
            if (Stop) return;
            MessageAdd("恢复电表时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                arrResult[i] = true;
                if (switchBefore[i].IndexOf("|") == -1 || switchAfter[i].IndexOf("|") == -1 || finshSwitch[i].IndexOf("|") == -1)
                {
                    arrResult[i] = false;
                }
                else
                {
                    string[] arrBefore = switchBefore[i].Split('|');
                    string[] arrAfter = switchAfter[i].Split('|');
                    string[] arrFinsh = finshSwitch[i].Split('|');

                    if ((arrBefore.Length < 0) || (arrAfter.Length < 0) || (arrFinsh.Length < 0))
                        arrResult[i] = false;
                    else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007" && (arrBefore[0] != arrFinsh[0] || arrBefore[0] == arrAfter[0]))
                        arrResult[i] = false;
                    else if (arrBefore[2].Trim() == "" || arrFinsh[2].Trim() == "" || arrAfter[2].Trim() == "")
                        arrResult[i] = false;
                }
                ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
            MessageAdd("费率时段功能试验检定结束...", EnumLogType.提示与流程信息);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "两套时区表切换时间", "两套日时段表切换时间", "约定冻结数据模式字", "当前正向有功电能", "运行时区", "运行时段", "时区表切换时间", "日时段表切换时间", "时区切换正向有功", "日时段切换正向有功", "时区表切换后运行时区", "时区表切换后运行时段", "时区表切换后时区表切换时间", "时区表切换后时段表切换时间", "时区表切换后时区切换正向有功", "时区表切换后时段切换正向有功", "时段表切换后运行时区", "时段表切换后运行时段", "时段表切换后时区表切换时间", "时段表切换后时段表切换时间", "时段表切换后时区切换正向有功", "时段表切换后时段切换正向有功", "结论" };
            return true;
        }
        /// <summary>
        /// 开始切换
        /// </summary>
        /// <param name="switchInfo"></param>
        /// <param name="switchTime"></param>
        /// <returns></returns>
        private void StartSwitch(string switchInfo, DateTime switchTime)
        {
            Identity(false);

            if (Stop) return;
            #region 将电表时间修改切换时间前15秒
            if (Stop) return;
            DateTime time = switchTime.AddSeconds(-15);
            MessageAdd("正在进行" + switchInfo + ",将电表时间修改到" + time.ToString("yyyyMMddHHmmss"), EnumLogType.提示与流程信息);
            bool[] result = MeterProtocolAdapter.Instance.WriteDateTime(time);
            string msg = "";
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!result[i])
                        msg += (i + 1).ToString() + "号,";
                }
            }
            if (msg != "")
            {
                msg = msg.Trim(',');
                msg += "表位修改时间失败，试验停止";
                MessageAdd(msg, EnumLogType.提示信息);
                MessageAdd("有电能表写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }
            #endregion


            MessageAdd("设置两套时区表切换时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("两套时区表切换时间", switchTime.ToString("yyyyMMddHHmmss"));

            if (Stop) return;
            MessageAdd("设置两套时段表切换时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("两套日时段表切换时间", switchTime.ToString("yyyyMMddHHmmss"));

            //倒计时60秒
            WaitTime(string.Format("{0},电表时间从{1}运行", switchInfo, time), 60);
        }


        private string[] ReadSwitch()
        {
            if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
            {
                return ReadSwitch645();
            }
            else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
            {
                return ReadSwitch698();
            }

            return new string[MeterNumber];
        }
        private string[] ReadSwitch698()
        {
            string[] zoneTime = new string[MeterNumber];//上1次时区表切换时间
            string[] zoneEnergy = new string[MeterNumber];//上1次时区表切换时正向有功电能

            string[] periodTime = new string[MeterNumber];//上1次时段表切换时间
            string[] periodEnergy = new string[MeterNumber];//上1次时段表切换时正向有功电能

            //上1次时区表切换冻结
            if (Stop) return null;
            List<string> oad = new List<string> { "50080200" };
            Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            List<string> rcsd = new List<string>();
            Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
            for (int i = 0; i < MeterNumber; ++i)
            {
                zoneTime[i] = "";
                zoneEnergy[i] = "";
                if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                {
                    if (dic[i].ContainsKey("20210200")) //数据冻结时间
                        zoneTime[i] = dic[i]["20210200"][0].ToString();
                    if (dic[i].ContainsKey("00100200")) //(当前)正向有功总电能
                    {
                        zoneEnergy[i] = dic[i]["00100200"][0].ToString();
                        zoneEnergy[i] = (Math.Floor((Convert.ToSingle(zoneEnergy[i]) / 100) * 100) / 100).ToString();
                    }
                }
            }

            //上1次时段表切换冻结
            if (Stop) return null;
            oad = new List<string> { "50090200" };
            dicObj = new Dictionary<int, List<object>>();
            rcsd = new List<string>();
            dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
            for (int i = 0; i < MeterNumber; ++i)
            {
                periodTime[i] = "";
                periodEnergy[i] = "";
                if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                {
                    if (dic[i].ContainsKey("20210200")) //数据冻结时间
                        periodTime[i] = dic[i]["20210200"][0].ToString();
                    if (dic[i].ContainsKey("00100200")) //(当前)正向有功总电能
                    {
                        periodEnergy[i] = dic[i]["00100200"][0].ToString();
                        periodEnergy[i] = (Math.Floor((Convert.ToSingle(periodEnergy[i]) / 100) * 100) / 100).ToString();
                    }
                }
            }

            string[] result = new string[MeterNumber];
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (zoneEnergy[j].Length > 8)
                {
                    zoneEnergy[j] = zoneEnergy[j].Substring(zoneEnergy[j].Length - 8, 8);
                    zoneEnergy[j] = (Convert.ToDouble(zoneEnergy[j]) / 100).ToString("F2");
                }
                if (periodEnergy[j].Length > 8)
                {
                    periodEnergy[j] = periodEnergy[j].Substring(periodEnergy[j].Length - 8, 8);
                    periodEnergy[j] = (Convert.ToDouble(periodEnergy[j]) / 100).ToString("F2");
                }
                result[j] = "第一套" + "|" + "第一套" + "|" + zoneTime[j] + "|" + periodTime[j] + "|" + zoneEnergy[j] + "|" + periodEnergy[j];

            }
            return result;
        }

        private string[] ReadSwitch645()
        {
            string[] statusZone = new string[MeterNumber];
            string[] statusPeriod = new string[MeterNumber];

            MessageAdd("读取状态运行字3", EnumLogType.提示信息);
            string[] readData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (readData[i] == null || readData[i] == "") continue;

                int chr = Convert.ToInt32(readData[i], 16);

                if ((chr & 0x01) == 0x01)
                    statusPeriod[i] = "第二套";
                else
                    statusPeriod[i] = "第一套";

                if ((chr & 0x20) == 0x20)
                    statusZone[i] = "第二套";
                else
                    statusZone[i] = "第一套";
            }

            if (Stop) return null;
            MessageAdd("读取上1次两套时区表切换时间", EnumLogType.提示信息);    //时区表切换冻结
            string[] zoneTime = MeterProtocolAdapter.Instance.ReadData("(上1次)两套时区表切换时间");

            if (Stop) return null;
            MessageAdd("读取上1次两套时段表切换时间", EnumLogType.提示信息);    //日时段表切换冻结
            string[] periodTime = MeterProtocolAdapter.Instance.ReadData("(上1次)两套时区表切换正向有功电能数据");

            if (Stop) return null;
            MessageAdd("读取上1次两套时区表切换电量", EnumLogType.提示信息);
            string[] zoneEnergy = MeterProtocolAdapter.Instance.ReadData("(上1次)两套日时段表切换时间");

            if (Stop) return null;
            MessageAdd("读取上1次两套时段表切换电量", EnumLogType.提示信息);
            string[] periodEnergy = MeterProtocolAdapter.Instance.ReadData("(上1次)两套日时段表切换正向有功电能数据");

            string[] result = new string[MeterNumber];
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (zoneEnergy[j].Length > 8)
                {
                    zoneEnergy[j] = zoneEnergy[j].Substring(zoneEnergy[j].Length - 8, 8);
                    zoneEnergy[j] = (Convert.ToDouble(zoneEnergy[j]) / 100).ToString("F2");
                }
                if (periodEnergy[j].Length > 8)
                {
                    periodEnergy[j] = periodEnergy[j].Substring(periodEnergy[j].Length - 8, 8);
                    periodEnergy[j] = (Convert.ToDouble(periodEnergy[j]) / 100).ToString("F2");
                }
                result[j] = statusZone[j] + "|" + statusPeriod[j] + "|" + zoneTime[j] + "|" + periodTime[j] + "|" + zoneEnergy[j] + "|" + periodEnergy[j];
            }
            return result;
        }


        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }
            WaitTime("升电压", 5);
            return true;
        }

    }
}
