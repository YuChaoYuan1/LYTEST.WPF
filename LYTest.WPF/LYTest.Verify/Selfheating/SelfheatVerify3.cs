using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LYTest.Verify.Selfheating
{
    /// <summary>
    /// 自热试验
    /// </summary>
    // 方案：预热时间(分钟),间隔时间(秒)，最长运行时间(分）
    // 结论：首个误差点1.0, 误差集合1.0, 最大变化值1.0, 首个误差点0.5L, 误差集合0.5L, 最大变化值0.5L

    public class SelfheatVerify3 : VerifyBase
    {
        /// <summary>
        /// 预热时间（分）
        /// </summary>
        float PreheatTime = 120;

        //string GLYS = "1.0";

        /// <summary>
        /// 间隔时间，秒，默认1分钟
        /// </summary>
        int IntervalTime = 60;

        /// <summary>
        /// 最长运行时间(分） ，默认1小时20分
        /// </summary>
        float MaxRunTime = 80;


        /// <summary>
        /// 偏差误差限
        /// </summary>
        float Limit = 1f;

        string GLYS = "1.0";

        ///// <summary>
        ///// 暂停的时间，免得出太多误差了,ms
        ///// </summary>
        //readonly int stopTime = 10000;

        ///// <summary>
        ///// 采用的判断时间,分钟
        ///// </summary>
        //readonly int okTime = 20;
        int[] lastNum;  //保存上一次误差的序号

        /// <summary>
        /// 误差集合
        /// </summary>
        readonly Dictionary<int, List<float>> errDic = new Dictionary<int, List<float>>();

        /// <summary>
        /// 首次误差
        /// </summary>
        readonly Dictionary<int, float> fErr = new Dictionary<int, float>();

        public override void Verify()
        {
            base.Verify();
            //string[] glysArr = new[] { "1.0", "0.5L" };

            lastNum = new int[MeterNumber];        //当前读取的误差序号
            lastNum.Fill(-1);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                errDic.Add(i, new List<float>());
                fErr.Add(i, -999.99f);
            }


            DataClear();
            // 升电压
            if (!PowerOn())
            {
                MessageAdd("升源失败", EnumLogType.错误信息);
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }
                return;
            }
            WaitTime("正在预热中", (int)(PreheatTime * 60));


            SetBluetoothModule(0);


            //预热完成,开始误差
            if (Stop) return;
            float xIb = Number.GetCurrentByIb("Imax", OneMeterInfo.MD_UA, HGQ);//计算电流
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, GLYS);

            if (Stop) return;
            EquipmentInit();



            if (Stop) return;
            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            StartWcb(0, 0xff);
            Thread.Sleep(10000);

            //关闭长时间升源保护功能
            if (Stop) return;
            ViewModel.EquipmentData.DeviceManager.SetPowerSafe(false);



            if (Stop) return;
            MessageAdd("进始首次误差", EnumLogType.提示信息);
            ErrorRead(); // 读取误差

            if (Stop) return;
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;     //表位不要检
                if (errDic[j].Count > 0)
                {
                    fErr[j] = errDic[j].First();
                    ResultDictionary[$"首个误差点"][j] = fErr[j].ToString("F5");
                    errDic[j].Clear();
                }
            }
            RefUIData("首个误差点");

            //最近源的功率因数 true-0.5L, false-1.0
            DateTime endTime = DateTime.Now.AddMinutes(MaxRunTime);
            //bool lastYS = true;
            while (true)
            {
                WaitTime($"剩余时间{endTime.Subtract(DateTime.Now).TotalSeconds:f2}秒，总时间{MaxRunTime * 60}秒;  运行间隔时间", IntervalTime);

                if (Stop) break;
                ErrorRead();

                if (Stop) return;
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;     //表位不要检
                    if (errDic[j].Count > 0 && CalMaxPc(fErr[j], errDic[j], out string errStr0, out float pc0))
                    {
                        ResultDictionary[$"误差集合"][j] = errStr0;
                        ResultDictionary[$"最大变化值"][j] = pc0.ToString("f5");

                        if (Math.Abs(pc0) > Math.Abs(Limit))
                        {
                            ResultDictionary["结论"][j] = ConstHelper.不合格;
                        }
                        else if (ResultDictionary["结论"][j] != ConstHelper.不合格)
                        {
                            ResultDictionary["结论"][j] = ConstHelper.合格;
                        }
                    }
                }

                //lastYS = !lastYS;
                //ys = glysArr[lastYS ? 1 : 0];
                //MessageAdd($"切换到功率因数【{ys}】", EnumLogType.提示与流程信息);
                //PowerFreeOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, ys);
                //WaitTime("源稳定中", 15);

                //if (Stop) return;
                //ErrorRead(ys); // 读取误差
                //for (int j = 0; j < MeterNumber; j++)
                //{
                //    if (!MeterInfo[j].YaoJianYn) continue;     //表位不要检
                //    if (errDic[j][ys].Count > 0 && CalMaxPc(fErr[j][ys], errDic[j][ys], out string errStr1, out float pc1))
                //    {
                //        ResultDictionary[$"误差集合"][j] = errStr1;
                //        ResultDictionary[$"最大变化值"][j] = pc1.ToString("f5");
                //        if (Math.Abs(pc1) > Math.Abs(Limit))
                //        {
                //            ResultDictionary["结论"][j] = ConstHelper.不合格;
                //        }
                //        else if (ResultDictionary["结论"][j] != ConstHelper.不合格)
                //        {
                //            ResultDictionary["结论"][j] = ConstHelper.合格;
                //        }
                //    }
                //}
                RefUIData("误差集合");
                RefUIData("最大变化值");
                RefUIData("结论");

                if (DateTime.Now.Subtract(endTime).TotalMilliseconds > 0)
                {
                    break;
                }
            }



            //判断结论

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrEmpty(ResultDictionary["最大变化值"][i]))
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                    continue;
                }
                try
                {
                    if (float.Parse(ResultDictionary["最大变化值"][i]) > Limit)
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        continue;
                    }
                }
                catch (Exception)
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }

            }
            RefUIData("结论");

            ////开启长时间升源保护功能
            //if (Stop) return;
            //ViewModel.EquipmentData.DeviceManager.SetPowerSafe(true);

        }


        private bool CalMaxPc(float firstErr, List<float> errs, out string errStr, out float pc)
        {

            errStr = "";
            pc = 0;
            if (errs.Count == 0)
            {
                return false;
            }

            foreach (float f in errs)
            {
                errStr += $"{f:f5},";
                float cz = f - firstErr;
                if (Math.Abs(cz) > Math.Abs(pc))
                    pc = cz;
            }

            return true;
        }


        private void ErrorRead()
        {
            string[] curWC = new string[MeterNumber];   //重新初始化本次误差
            int[] curNum = new int[MeterNumber];        //当前读取的误差序号
            bool[] flag = new bool[MeterNumber];
            curWC.Fill("");
            curNum.Fill(0);
            flag.Fill(false);

            DateTime eTime = DateTime.Now.AddSeconds(IntervalTime / 3);
            while (DateTime.Now < eTime)
            {
                if (Stop) break;
                if (!ReadWc(ref curWC, ref curNum, PowerWay.正向有功))    //读取误差
                {
                    continue;
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn)
                    {
                        flag[i] = true;
                        continue;
                    }
                    if (curNum[i] <= lastNum[i]) continue;
                    if (curNum[i] <= 1) continue;
                    if (string.IsNullOrEmpty(curWC[i]) || curWC[i] == "999.99") continue;
                    if (!float.TryParse(curWC[i], out float f)) continue;

                    errDic[i].Add(f);
                    flag[i] = true;

                }

                if (Array.IndexOf(flag, false) < 0)
                    break;

            }


        }


        /// <summary>
        /// 初始化误差板
        /// </summary>
        public bool EquipmentInit()
        {
            if (IsDemo) return true;
            int[] meterconst = MeterHelper.Instance.MeterConst(true);
            ulong constants = GetStaConst();

            SetPulseType(49.ToString("x"));
            if (Stop) return true;
            MessageAdd("开始初始化基本误差检定参数!", EnumLogType.提示信息);
            //设置误差版被检常数
            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
            int SetConstants = (int)(constants / 100);
            SetStandardConst(0, SetConstants, -2, 0xff);
            //设置误差版标准常数 TODO2
            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
            int[] pulselaps = new int[MeterNumber];  //这里是为了以后不同表位不同圈数预留--目前暂时用着吧
            pulselaps.Fill(10);
            if (!SetTestedConst(0, meterconst, 0, pulselaps, 0xff))
            {
                MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 刷新不合格结论
        /// </summary>
        private void DataClear()
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["首个误差点"][i] = "";
                ResultDictionary["误差集合"][i] = "";
                ResultDictionary["最大变化值"][i] = "";
                ResultDictionary["结论"][i] = "";
            }
            RefUIData("首个误差点");
            RefUIData("误差集合");
            RefUIData("最大变化值");

            RefUIData("结论");
        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //一次误差变化值，二次误差变化值，结论
            string[] data = Test_Value.Split('|');

            GLYS = data[0];

            if (data.Length > 1 && float.TryParse(data[1], out float h))
            {
                PreheatTime = h;
            }
            else
            {
                MessageAdd("请设置预热时间", EnumLogType.错误信息);
            }

            if (data.Length > 2 && int.TryParse(data[2], out int t))
            {
                IntervalTime = t;
            }
            else
            {
                MessageAdd("请设置间隔时间", EnumLogType.错误信息);
            }

            if (data.Length > 3 && float.TryParse(data[3], out float r))
            {
                MaxRunTime = r;
            }
            else
            {
                MessageAdd("请设置最长运行时间", EnumLogType.错误信息);
            }


            //计算误差限
            Limit = MeterLevel(OneMeterInfo) * 0.1f;
            ResultNames = new string[] { "首个误差点", "误差集合", "最大变化值", "结论" };
            return true;


        }



    }
}
