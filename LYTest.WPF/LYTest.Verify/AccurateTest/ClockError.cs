using LYTest.Core;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 日计时误差
    /// </summary>

    public class ClockError : VerifyBase
    {

        #region 变量
        int WcCount = 5;   //误差次数
        int WcQs = 60;      //误差圈数
        float wcLimit = 0.5f;//误差限
        int maxErrorTimes = 5;               //最大误差次数
        //string ItemId = string.Empty;  //项目编号
        //string StandardClockFrequency = "50000";//标准时钟频率
        //string CheckedClockFrequency = "1";//被检时钟频率
        string errModel = "秒脉冲";
        #endregion


        /// <summary>
        /// 日计时误差
        /// </summary>
        public override void Verify()
        {
            base.Verify();
            MessageAdd("日计时误差试验检定开始...", EnumLogType.提示信息);
            int dot = 4;
            if (errModel == "脉冲频率" || DAL.Config.ConfigHelper.Instance.Dgn_DayClockTestValue == "Hz")
            {
                dot = 7;
            }

            if (IsDemo)
            {
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    float avg = 0;
                    if (errModel == "脉冲频率"
                        || DAL.Config.ConfigHelper.Instance.Dgn_DayClockTestValue == "Hz")
                    {
                        for (int i = 0; i < WcCount; i++)
                        {
                            ResultDictionary[$"误差{i + 1}"][j] = "0.9999981";
                            RefUIData($"误差{i + 1}");
                        }
                        avg = 0.9999981f;
                        ResultDictionary["平均值"][j] = "0.9999981";//0.0000019
                    }
                    else
                    {
                        float[] vs = new float[WcCount];
                        for (int i = 0; i < WcCount; i++)
                        {
                            vs[i] = DemoErr(0.1f);
                            ResultDictionary[$"误差{i + 1}"][j] = vs[i].ToString("F4");
                            RefUIData($"误差{i + 1}");
                        }
                        avg = vs.Average();
                        ResultDictionary["平均值"][j] = avg.ToString("F4");
                    }
                    ResultDictionary["化整值"][j] = Number.GetHzz(avg, 0.01F).ToString("F2");
                    ResultDictionary["误差限(s/d)"][j] = "±0.5";
                    ResultDictionary["结论"][j] = "合格";
                }
                RefUIData("平均值");
                RefUIData("化整值");
                RefUIData("误差限(s/d)");
                RefUIData("结论");
            }
            else
            {
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    ResultDictionary["误差限(s/d)"][j] = "±" + wcLimit.ToString();

                }
                RefUIData("误差限(s/d)");
                if (!InitEquipment())   //设备控制
                {
                    MessageAdd("【日计时试验】初始化基本误差设备参数失败", EnumLogType.错误信息);
                }
                if (Stop) return;

                List<string>[] errData = new List<string>[MeterNumber]; //误差数据
                List<string>[] errData2 = new List<string>[MeterNumber]; //误差数据

                for (int i = 0; i < MeterNumber; i++)
                {
                    errData[i] = new List<string>();
                    errData2[i] = new List<string>();

                }
                int[] lastNums = new int[MeterNumber];
                lastNums.Fill(-1);

                MessageAdd("正在启动误差板", EnumLogType.提示信息);
                SetBluetoothModule(04);
                StartWcb(04, 0xff);   //启动误差板
                Thread.Sleep(200);

                MessageAdd("采集数据", EnumLogType.提示信息);

                bool[] arrCheckOver = new bool[MeterNumber];            //表位完成记录
                arrCheckOver.Fill(false);
                DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
                //(VerifyConfig.MaxHandleTime + 100) * 1000
                int maxTimems = ((WcCount + 1) * WcQs * 1000) + ((WcCount + 2) * 5000);
                while (true)
                {
                    if (Stop)
                    {
                        MessageAdd("停止检定", EnumLogType.提示信息);
                        break;
                    }
                    if (TimeSubms(DateTime.Now, TmpTime1) > maxTimems && !IsMeterDebug) //超出最大处理时间
                    {
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.错误信息);
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!MeterInfo[i].YaoJianYn) continue;

                            if (string.IsNullOrWhiteSpace(ResultDictionary["结论"][i]))
                            {
                                ResultDictionary["结论"][i] = ConstHelper.不合格;
                                NoResoult[i] = "超时";
                            }
                        }

                        break;
                    }

                    string[] curWC = new string[MeterNumber];   //重新初始化本次误差
                    int[] curNum = new int[MeterNumber];        //当前读取的误差序号
                    curWC.Fill("");
                    curNum.Fill(0);
                    if (!ReadWc(ref curWC, ref curNum, 4))    //读取误差
                    {
                        continue;
                    }
                    if (Stop) break;

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo meter = MeterInfo[i];      //表基本信息
                        if (!MeterInfo[i].YaoJianYn) arrCheckOver[i] = true;     //表位不要检
                        if (arrCheckOver[i]) continue;   //表位检定通过了
                        if (string.IsNullOrEmpty(curWC[i])) continue;
                        if (curNum[i] <= lastNums[i]) continue;
                        if (curNum[i] < 1) continue;
                        errData2[i].Insert(0, curWC[i]);
                        //curWC[i] = "0.00186";
                        if (errModel == "脉冲频率"
                            || DAL.Config.ConfigHelper.Instance.Dgn_DayClockTestValue == "Hz")
                        {
                            //curWC[i]= curWC[i]* WcQs*500000
                            decimal err = decimal.Parse(curWC[i]);
                            //err = ((err * WcQs * 500000) / (24 * 3600)) + WcQs * 500000))/(10*500000);
                            err = (WcQs * 500000) / ((err / 86400 * WcQs * 500000) + WcQs * 500000);
                            curWC[i] = err.ToString($"F{dot}");
                        }
                        errData[i].Insert(0, curWC[i]);
                        lastNums[i] = curNum[i];

                        ErrorLimit limit = new ErrorLimit
                        {
                            UpLimit = wcLimit * 1F,
                            DownLimit = -wcLimit * 1F,
                        };
                        string[] dj = Number.GetDj(meter.MD_Grane);
                        decimal[] wc = ArrayConvert.Todecimal(errData[i].ToArray());  //Hz
                        decimal[] wc2 = ArrayConvert.Todecimal(errData2[i].ToArray());  //s/d

                        string Result = SetWuCha(wc.Select(d => (float)d).ToArray(), limit);
                        ResultDictionary["结论"][i] = Result;
                        if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                        {
                            NoResoult[i] = "误差超出误差限";
                        }
                        //5.787037037037037
                        #region 获取详细数据
                        double fSum = 0.0f;
                        float fSum2 = 0.0f;//化整值单独计算

                        string[] strWc = new string[WcCount];
                        for (int j = 0; j < wc.Length; j++)
                        {
                            strWc[j] = wc[j].ToString($"F{dot}");
                            fSum += Convert.ToSingle(strWc[j]);
                            fSum2 += Convert.ToSingle(wc2[j]);

                            ResultDictionary[$"误差{j + 1}"][i] = wc[j].ToString($"F{dot}");
                            if (errModel == "脉冲频率" || DAL.Config.ConfigHelper.Instance.Dgn_DayClockTestValue == "Hz")
                            {
                                //nothing
                            }
                            else
                            {
                                if (float.Parse(strWc[j]) > 0)
                                {
                                    ResultDictionary[$"误差{j + 1}"][i] = "+" + wc[j].ToString($"F{dot}");
                                }
                            }
                        }
                        fSum /= wc.Length;
                        fSum2 /= wc.Length;
                        fSum2 = Number.GetHzz(fSum2, 0.01F);
                        if (wc.Length == maxErrorTimes)
                        {
                            ResultDictionary["平均值"][i] = AddFlag((float)fSum, dot);
                            //ResultDictionary["化整值"][i] = fSum.ToString("0.00");

                            ResultDictionary["化整值"][i] = AddFlag(fSum2, 2);

                            arrCheckOver[i] = true;
                        }
                        #endregion
                    }

                    for (int i = 0; i < WcCount; i++)
                    {
                        RefUIData($"误差{i + 1}");
                    }

                    if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)
                        break;
                    Thread.Sleep(100);

                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    for (int n = 1; n <= WcCount; n++)
                    {
                        if (string.IsNullOrWhiteSpace(ResultDictionary[$"误差{n}"][i]))
                        {
                            ResultDictionary["结论"][i] = ConstHelper.不合格;
                            MessageAdd($"【日计时试验】表位{i}有空误差", EnumLogType.提示信息);
                        }
                    }
                    if (MeterInfo[i].YaoJianYn && string.IsNullOrWhiteSpace(ResultDictionary["结论"][i]))
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        MessageAdd($"【日计时试验】表位{i}结论为空", EnumLogType.提示信息);
                    }
                }

                RefUIData("平均值");
                RefUIData("化整值");

                StopWcb(04, 0xff);//关闭误差板
                RefUIData("结论");
            }

            MessageAdd("检定完成", EnumLogType.提示信息);
        }


        protected override bool CheckPara()
        {
            string[] str = Test_Value.Split('|');
            if (str.Length < 3) return false;
            if (float.TryParse(str[0], out float Count))
            {
                wcLimit = Count;
            }
            if (wcLimit == 0) wcLimit = 0.5F;

            if (int.TryParse(str[1], out int count2))
            {
                WcCount = count2;
                maxErrorTimes = WcCount;
            }
            if (int.TryParse(str[2], out count2))
            {
                WcQs = count2;
                if (VerifyConfig.Test_QuickModel)
                {
                    if (!DAL.Config.ConfigHelper.Instance.IsITOMeter)
                        WcQs = 10;
                }
            }
            if (str.Length > 3)
            {
                if (str[3] == "脉冲频率")
                    errModel = "脉冲频率";
                else
                    errModel = "秒误差";
            }
            else
            {
                errModel = "秒误差";
            }
            ResultNames = new string[] { "误差限(s/d)", "误差1", "误差2", "误差3", "误差4", "误差5", "平均值", "化整值", "结论" };
            return true;
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns></returns>
        private bool InitEquipment()
        {
            MessageAdd("日计时误差实验开始初始化设备", EnumLogType.详细信息);
            if (IsDemo) return true;
            try
            {
                if (!CheckVoltage())
                {
                    return false;
                }
                if (Stop) return true;
                //设置误差版被检常数
                MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
                SetStandardConst(1, 500000, 0);
                //设置误差版标准常数
                MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
                int[] meterconst = MeterHelper.Instance.MeterConst(true);
                meterconst.Fill(1);
                int[] pulselap = new int[MeterNumber];
                pulselap.Fill(WcQs);
                if (!SetTestedConst(04, meterconst, 0, pulselap))
                {
                    MessageAdd("初始化误差检定参数失败", EnumLogType.错误信息);
                    return false;
                }
                MessageAdd("日计时误差实验初始化设备完成", EnumLogType.详细信息);
                return true;
            }
            catch (Exception ex)
            {
                MessageAdd("日计时误差实验初始化设备异常" + ex.ToString(), EnumLogType.错误信息);
                return false;
            }

        }


        public string SetWuCha(float[] indata, ErrorLimit limit)
        {
            float[] data = indata;
            if (errModel == "脉冲频率"
                || DAL.Config.ConfigHelper.Instance.Dgn_DayClockTestValue == "Hz")
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (indata[i] == 0) data[i] = 0;
                    else data[i] = (1 - indata[i]) / indata[i] * 86400;
                }
            }

            float avgWC = Number.GetAvgA(data);
            //取平均值
            avgWC = (float)Math.Round(avgWC, VerifyConfig.PjzDigit);

            ////由电源供电的时钟试验化整间距为0.01
            ////参照JJG-596-1999 5.1.1
            avgWC = Number.GetHzz(avgWC, 0.01F);


            for (int i = 0; i < data.Length; i++)
            {
                if (Math.Abs(data[i]) > 0.5)
                    return ConstHelper.不合格;
            }
            if (avgWC >= limit.DownLimit && avgWC <= limit.UpLimit)
            {
                return ConstHelper.合格;
            }
            return ConstHelper.不合格;
        }
    }
}
