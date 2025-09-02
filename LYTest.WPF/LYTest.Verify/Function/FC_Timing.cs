using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.Function
{
    /// <summary>
    /// 计时功能 
    /// add lsj 20220724
    /// </summary>
    class FC_Timing : VerifyBase
    {
        public override void Verify()
        {
            MessageAdd("项目：计时功能 检定开始。。", EnumLogType.提示与流程信息);
            base.Verify();
            if (Stop) return;

            //初始化设备
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            Identity(true);
            if (Stop) return;
            //string[] broadMsg = new string[] { "【23点55分前】广播校时", "【23点57分】广播校时", "【零点1分】广播校时", "【零点5分后】广播校时", "【小于5分后】广播校时", "【大于5分】广播校时", "【一天一次】广播校时", "【一天重复】广播校时" };
            string[] broadMsg = new string[] { "【23点55分前】", "【23点57分】", "【零点1分】", "【零点5分后】", "【小于5分后】", "【大于5分】", "【一天一次】", "【一天重复】" };
            string[] arrBroadCastTime = new string[] { "234800", "235500", "235900", "000500", "012800", "034000", "042800", "052800" };
            string[] arrSetMeterTime = new string[] { "235000", "235700", "000100", "000700", "013000", "033000", "043000", "053000" };

            bool[][] rst1 = new bool[arrBroadCastTime.Length][];
            for (int i = 0; i < arrBroadCastTime.Length; i++)
            {
                rst1[i] = StartBroadCastTime(i, broadMsg[i], arrSetMeterTime[i], arrBroadCastTime[i]);
            }

            if (Stop) return;
            Identity(false);
            MessageAdd("恢复电表时间", EnumLogType.提示信息);

            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间
            bool[] rst2 = MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            if (Stop) return;
            string msg = "";
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!rst2[i])
                    {
                        msg += (i + 1).ToString() + "号,";
                    }
                }
            }
            if (!string.IsNullOrEmpty(msg))
            {
                //msg = msg.Trim(',');
                //msg += "表位修改时间失败，试验停止";
                Stop = true;
                MessageAdd("有电能表写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }
            bool[] rst = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                rst[i] = true;
                for (int j = 0; j < arrBroadCastTime.Length; j++)
                {
                    if (!rst1[j][i])
                    {
                        rst[i] = false;
                        break;
                    }
                }
                ResultDictionary["结论"][i] = rst[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
        }
        private bool[] StartBroadCastTime(int key, string broadMsg, string setMeterTime, string strBroadCastTime)
        {
            MessageAdd(broadMsg, EnumLogType.提示信息);
            if (Stop) return null;
            Identity(false);

            string time = DateTime.Now.Date.ToString("yyMMdd") + "000000";
            DateTime dtMeterTime = LYTest.Core.Function.DateTimes.FormatStringToDateTime(time);

            dtMeterTime = dtMeterTime.AddSeconds(-5);
            dtMeterTime = dtMeterTime.AddDays(key);

            if (key < 8)
            {
                #region 将电表时间修改0点的前1分钟
                if (Stop) return null;
                MessageAdd("正在进行" + broadMsg + ",将电表时间修改到" + dtMeterTime.ToString("yyMMddHHmmss"), EnumLogType.提示与流程信息);
                bool[] result = MeterProtocolAdapter.Instance.WriteDateTime(dtMeterTime);
                string msg = "";
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (!result[i])
                        {
                            msg += (i + 1).ToString() + "号,";
                        }
                    }
                }
                if (msg != "")
                {
                    msg = msg.Trim(',');
                    msg += "表位修改时间失败，试验停止";
                    Stop = true;
                    MessageAdd(msg + "\r\n有电能表写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);

                }

                WaitTime(broadMsg, 20);
                #endregion
            }
            if (Stop) return null;
            if (key <= 2)
                dtMeterTime = dtMeterTime.AddDays(1);
            else if (key == 8)
                dtMeterTime = dtMeterTime.AddDays(-1);

            setMeterTime = dtMeterTime.ToString("yyyy/MM/dd ") + setMeterTime.Substring(0, 2) + ":" + setMeterTime.Substring(2, 2) + ":" + setMeterTime.Substring(4, 2);
            MessageAdd(string.Format("正在进行{0},将电表时间修改到{1}", broadMsg, setMeterTime), EnumLogType.提示信息);
            bool[] bReturn = MeterProtocolAdapter.Instance.WriteDateTime(DateTime.Parse(setMeterTime));

            if (Stop) return null;
            MessageAdd(string.Format("正在进行{0},读取电表时间", broadMsg), EnumLogType.提示信息);
            DateTime[] dtBefMeterTime = MeterProtocolAdapter.Instance.ReadDateTime();

            if (Stop) return null;
            strBroadCastTime = dtMeterTime.ToString("yyMMdd") + strBroadCastTime;
            MessageAdd(string.Format("正在进行{0},将电表时间广播校时到{1}", broadMsg, strBroadCastTime), EnumLogType.提示信息);
            dtMeterTime = LYTest.Core.Function.DateTimes.FormatStringToDateTime(strBroadCastTime);
            MeterProtocolAdapter.Instance.BroadCastTime(dtMeterTime);

            if (Stop) return null;
            MessageAdd("正在进行" + broadMsg + "," + "读取电表时间", EnumLogType.提示信息);
            DateTime[] dtCurMeterTime = MeterProtocolAdapter.Instance.ReadDateTime();

            if (Stop) return null;
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                if (dtCurMeterTime[j] == null) continue;

                dtMeterTime = LYTest.Core.Function.DateTimes.FormatStringToDateTime(strBroadCastTime);
                double diffTime = Math.Abs(dtMeterTime.Subtract(dtCurMeterTime[j]).TotalSeconds);
                string result;
                if (diffTime <= 20)
                {
                    result = "合格";
                    bReturn[j] = true;
                }
                else
                {
                    result = "不合格";
                    bReturn[j] = false;
                }
                ResultDictionary[broadMsg + "校时前时间"][j] = dtBefMeterTime[j].ToString("yyMMddHHmmss");
                ResultDictionary[broadMsg + "校时时间"][j] = strBroadCastTime;
                ResultDictionary[broadMsg + "校时后时间"][j] = dtCurMeterTime[j].ToString("yyMMddHHmmss");
                ResultDictionary[broadMsg + "结论"][j] = result;
            }
            RefUIData(broadMsg + "校时前时间");
            RefUIData(broadMsg + "校时时间");
            RefUIData(broadMsg + "校时后时间");
            RefUIData(broadMsg + "结论");

            return bReturn;
        }
        protected override bool CheckPara()
        {
            string[] broadMsg = new string[] { "【23点55分前】", "【23点57分】", "【零点1分】", "【零点5分后】", "【小于5分后】", "【大于5分】", "【一天一次】", "【一天重复】" };
            List<string> list = new List<string>();
            for (int i = 0; i < broadMsg.Length; i++)
            {
                list.Add(broadMsg[i] + "校时前时间");
                list.Add(broadMsg[i] + "校时时间");
                list.Add(broadMsg[i] + "校时后时间");
                list.Add(broadMsg[i] + "结论");
            }
            list.Add("结论");
            ResultNames = list.ToArray();
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
            WaitTime("升源成功，等待源稳定", 5);
            return true;
        }

    }
}
