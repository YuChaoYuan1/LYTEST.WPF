using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.AccurateTest
{
    struct LoadPoint
    {
        public PowerWay PowerDirection;
        public Cus_PowerYuanJian ABC;
        /// <summary>
        /// 电流倍数字符串
        /// </summary>
        public string CurrentXIx;
        public string PowerFactor;
        /// <summary>
        /// 误差限：±X 或 X
        /// </summary>
        public string LimitRange;
        public int LapCount;
        public float RoundingSpace;
    }
    /// <summary>
    /// 重复性
    /// </summary>
    class ReproducibilityTest : VerifyBase
    {
        LoadPoint CurPoint = new LoadPoint();//TODO:暂时用单表位
        int NeedErrCount;//规范不少于3个

        protected override bool CheckPara()
        {
            if (string.IsNullOrWhiteSpace(Test_Value))
            {
                MessageAdd("重复性参数为空", EnumLogType.错误信息);
                return false;
            }
            string[] tem = Test_Value.Split('|');
            if (tem.Length < 4)
            {
                MessageAdd("重复性参数数量不足", EnumLogType.错误信息);
                return false;
            }

            NeedErrCount = 10;//TODO:配置

            CurPoint.PowerDirection = (PowerWay)Enum.Parse(typeof(PowerWay), tem[0]);

            FangXiang = CurPoint.PowerDirection;

            CurPoint.ABC = Cus_PowerYuanJian.H;
            switch (tem[1])
            {
                case "H":
                    CurPoint.ABC = Cus_PowerYuanJian.H;
                    break;
                case "A":
                    CurPoint.ABC = Cus_PowerYuanJian.A;
                    break;
                case "B":
                    CurPoint.ABC = Cus_PowerYuanJian.B;
                    break;
                case "C":
                    CurPoint.ABC = Cus_PowerYuanJian.C;
                    break;
                default:
                    CurPoint.ABC = Cus_PowerYuanJian.H;
                    break;
            }
            CurPoint.CurrentXIx = tem[2];
            CurPoint.PowerFactor = tem[3];
            CurPoint.LimitRange = GetReptLmt(OneMeterInfo, CurPoint).ToString();
            CurPoint.LapCount = GetLapCount(OneMeterInfo, CurPoint);
            CurPoint.RoundingSpace = GetWuChaHzzJianJu(false, MeterLevel(OneMeterInfo));
            //误差值:多个误差,英文逗号
            ResultNames = new string[] { "功率方向", "功率元件", "电流倍数", "功率因数", "误差限", "误差圈数", "误差值", "平均值", "偏差", "最值差", "化整值", "结论" };
            return base.CheckPara();
        }

        private int GetLapCount(TestMeterInfo meter, LoadPoint curPoint)
        {
            float current = Number.GetCurrentByIb(curPoint.CurrentXIx, OneMeterInfo.MD_UA, HGQ);
            double OnekwhSec = CalcSecondsOfkWh(1, current, curPoint.ABC, curPoint.PowerFactor);
            int onePulsems = OnePulseNeedTime(IsYouGong, OnekwhSec / 60f);
            if (onePulsems == 0) return 5;
            int lap = (int)Math.Ceiling(5 * 1000f / onePulsems);
            return lap;
        }

        public override void Verify()
        {
            MessageAdd("重复性检定开始...", EnumLogType.流程信息);
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    ResultDictionary["功率方向"][i] = CurPoint.ABC.ToString();
                    ResultDictionary["功率元件"][i] = CurPoint.PowerDirection.ToString();
                    ResultDictionary["电流倍数"][i] = CurPoint.CurrentXIx;
                    ResultDictionary["功率因数"][i] = CurPoint.PowerFactor;
                    ResultDictionary["误差限"][i] = CurPoint.LimitRange.ToString();
                    ResultDictionary["误差圈数"][i] = CurPoint.LapCount.ToString();
                }
            }
            RefUIData("功率方向");
            RefUIData("功率元件");
            RefUIData("电流倍数");
            RefUIData("功率因数");
            RefUIData("误差限");
            RefUIData("误差圈数");

            if (IsDemo)
            {
                #region 演示模式
                float[] Errs;
                float pkpk;

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        Errs = DemoBasicErr(MeterLevel(OneMeterInfo), NeedErrCount);
                        pkpk = Errs.Max() - Errs.Min();

                        ResultDictionary["误差值"][i] = string.Join(",", Errs);
                        ResultDictionary["最值差"][i] = pkpk.ToString();
                        ResultDictionary["化整值"][i] = GetWuChaRounding(pkpk, CurPoint.RoundingSpace);
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    }
                }
                RefUIData("误差值");
                RefUIData("最值差");
                RefUIData("化整值");
                #endregion
            }//end if demo
            else
            {
                SetBluetoothModule(GetWcbFangXianIndex(CurPoint.PowerDirection));

                if (!ErrorInitEquipment(CurPoint.PowerDirection, CurPoint.ABC, CurPoint.PowerFactor, CurPoint.CurrentXIx, CurPoint.LapCount))
                {
                    MessageAdd("初始化基本误差设备参数失败", EnumLogType.提示信息);
                    return;
                }
                if (Stop) return;

                MessageAdd("正在启动误差版...", EnumLogType.提示信息);
                if (!StartWcb(GetWcbFangXianIndex(CurPoint.PowerDirection), 0xff))
                {
                    MessageAdd("误差板启动失败...", EnumLogType.提示信息);
                    return;
                }

                MessageAdd("采集数据...", EnumLogType.提示信息);
                bool[] allCheckOver = new bool[MeterNumber];
                int[] lastWcNum = new int[MeterNumber];
                lastWcNum.Fill(-1);
                List<string>[] allMeterErrs = new List<string>[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    allMeterErrs[i] = new List<string>();
                }
                float xIb = Number.GetCurrentByIb(CurPoint.CurrentXIx, OneMeterInfo.MD_UA, HGQ);
                int MaxTimems = (NeedErrCount + 2) * CurPoint.LapCount * OnePulseNeedTime(IsYouGong, 1000 * 60 / CalculatePower(OneMeterInfo.MD_UB, xIb, Clfs, CurPoint.ABC, CurPoint.PowerFactor, IsYouGong)) + ((NeedErrCount + 3) * 5000);
                DateTime BeginTime = DateTime.Now;
                while (true)
                {
                    if (Stop) break;
                    if (TimeSubms(DateTime.Now, BeginTime) > MaxTimems && !IsMeterDebug)
                    {
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                        break;
                    }
                    string[] curWC = new string[MeterNumber];
                    int[] curWcNum = new int[MeterNumber];
                    curWC.Fill("");
                    curWcNum.Fill(0);
                    if (!ReadWc(ref curWC, ref curWcNum, CurPoint.PowerDirection))
                    {
                        continue;
                    }

                    if (Stop) break;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn)
                        {
                            allCheckOver[i] = true;
                            continue;
                        }
                        if (allCheckOver[i] && !IsMeterDebug) continue;
                        if (lastWcNum[i] >= curWcNum[i]) continue;
                        if (string.IsNullOrEmpty(curWC[i])) continue;
                        if (curWcNum[i] <= VerifyConfig.ErrorStartCount) continue;
                        if (lastWcNum[i] < curWcNum[i]) //大于上一次误差次数
                        {
                            lastWcNum[i] = curWcNum[i];
                        }
                        allMeterErrs[i].Add(curWC[i]);
                        if (allMeterErrs[i].Count > NeedErrCount)
                            allMeterErrs[i].RemoveAt(0);
                        if (allMeterErrs[i].Count >= NeedErrCount)
                        {

                            var Errs = allMeterErrs[i].Select(v => float.Parse(v));
                            float pkpk = Errs.Max() - Errs.Min();
                            string Rounding = GetWuChaRounding(pkpk, CurPoint.RoundingSpace);
                            ResultDictionary["平均值"][i] = Errs.Average().ToString("F4");
                            ResultDictionary["偏差"][i] = Math.Round(Number.GetWindage(Errs.ToArray()), VerifyConfig.PjzDigit).ToString("F4");
                            ResultDictionary["最值差"][i] = pkpk.ToString("F4");
                            ResultDictionary["化整值"][i] = Rounding;

                            if (float.Parse(Rounding) <= float.Parse(CurPoint.LimitRange.Replace("±", "")))
                            {
                                if (Errs.All(v => v <= MeterLevel(MeterInfo[i])))
                                {
                                    ResultDictionary["结论"][i] = ConstHelper.合格;
                                    allCheckOver[i] = true;
                                }
                                else
                                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                            }
                            else
                            {
                                ResultDictionary["结论"][i] = ConstHelper.不合格;
                            }
                        }
                        ResultDictionary["误差值"][i] = string.Join(",", allMeterErrs[i]);
                    }
                    RefUIData("误差值");
                    RefUIData("平均值");
                    RefUIData("偏差");
                    RefUIData("最值差");
                    RefUIData("化整值");
                    if (Array.IndexOf(allCheckOver, false) < 0 && !IsMeterDebug) break;
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrWhiteSpace(ResultDictionary["化整值"][i]))
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    }
                }
            }

            RefUIData("结论");
            StopWcb(GetWcbFangXianIndex(CurPoint.PowerDirection), 0xff);
            MessageAdd("重复性检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// 重复性误差限。临时放这里
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="curPoint"></param>
        /// <returns></returns>
        private float GetReptLmt(TestMeterInfo meter, LoadPoint curPoint)
        {
            string curI = curPoint.CurrentXIx.ToLower();
            string curPF = curPoint.PowerFactor.ToUpper().Trim('L', 'C');
            //string curPFLC;
            //if (curPoint.PowerFactor.ToUpper().IndexOf("L") >= 0) 
            //    curPFLC = "L";
            //else if (curPoint.PowerFactor.ToUpper().IndexOf("C") >= 0) 
            //    curPFLC = "C";

            string[] dj = Number.GetDj(meter.MD_Grane);
            switch (dj[0])
            {
                case "0.2":
                case "0.2S":
                case "D":
                    if (curPF == "1" || curPF == "1.0")
                    {
                        if (curI.IndexOf("imax") >= 0)//重复性没有特殊点，没处理特殊点，例如imax-ib
                        {
                            return 0.02f;
                        }
                        else if (curI.IndexOf("imin") >= 0)
                        {
                            return 0.04f;
                        }
                        else if (curI.IndexOf("ib") >= 0 || curI.IndexOf("in") >= 0)
                        {
                            float xib = 0.1f;
                            if (HGQ) xib = 0.2f;

                            if (curI != "ib" && float.Parse(curI.Replace("ib", "").Replace("in", "")) < xib) return 0.04f;
                            else return 0.02f;
                        }
                        else if (curI.IndexOf("itr") >= 0)
                        {
                            if (curI != "itr" && float.Parse(curI.Replace("itr", "")) < 1) return 0.04f;
                            else return 0.02f;
                        }
                        else
                        {
                            return 0.04f;
                        }
                    }
                    else
                    {
                        return 0.03f;
                    }
                case "0.5":
                case "0.5S":
                case "C":
                    if (curPF == "1" || curPF == "1.0")
                    {
                        if (curI.IndexOf("imax") >= 0)//重复性没有特殊点，没处理特殊点，例如imax-ib
                        {
                            return 0.05f;
                        }
                        else if (curI.IndexOf("imin") >= 0)
                        {
                            return 0.1f;
                        }
                        else if (curI.IndexOf("ib") >= 0 || curI.IndexOf("in") >= 0)
                        {
                            float xib = 0.1f;
                            if (HGQ) xib = 0.2f;

                            if (curI != "ib" && float.Parse(curI.Replace("ib", "").Replace("in", "")) < xib) return 0.1f;
                            else return 0.05f;
                        }
                        else if (curI.IndexOf("itr") >= 0)
                        {
                            if (curI != "itr" && float.Parse(curI.Replace("itr", "")) < 1) return 0.1f;
                            else return 0.05f;
                        }
                        else
                        {
                            return 0.1f;
                        }
                    }
                    else
                    {
                        return 0.06f;
                    }
                case "1":
                case "1.0":
                case "2":
                case "2.0":
                case "B":
                case "A":
                    if (curPF == "1" || curPF == "1.0")
                    {
                        if (curI.IndexOf("imax") >= 0)//重复性没有特殊点，没处理特殊点，例如imax-ib
                        {
                            return 0.1f;
                        }
                        else if (curI.IndexOf("imin") >= 0)
                        {
                            return 0.15f;
                        }
                        else if (curI.IndexOf("ib") >= 0 || curI.IndexOf("in") >= 0)
                        {
                            float xib = 0.1f;
                            if (HGQ) xib = 0.2f;

                            if (curI != "ib" && float.Parse(curI.Replace("ib", "").Replace("in", "")) < xib) return 0.15f;
                            else return 0.1f;
                        }
                        else if (curI.IndexOf("itr") >= 0)
                        {
                            if (curI != "itr" && float.Parse(curI.Replace("itr", "")) < 1) return 0.15f;
                            else return 0.1f;
                        }
                        else
                        {
                            return 0.15f;
                        }
                    }
                    else
                    {
                        return 0.1f;
                    }
                default:
                    return 0.15F;
            }
        }

    }
}
