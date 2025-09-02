using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 整点冻结检定
    /// </summary>
    public class Freeze_Whole_Point : VerifyBase
    {
        /// <summary>
        /// 整点冻结检定
        /// </summary>
        public override void Verify()
        {
            #region  --------------局部变量---------------
            string[][] lstFreezePW = new string[][] { new string[MeterNumber], new string[MeterNumber] };               //存储两次冻结模式字
            string[][] str_StartTime = new string[][] { new string[MeterNumber], new string[MeterNumber] };             //存储两次冻结开始时间
            string[][] str_Interval = new string[][] { new string[MeterNumber], new string[MeterNumber] };              //存储两次冻结时间间隔
            //维数 0=冻结前上一次冻结电量，1=冻结前当前表电量，2=冻结后当前表电量，3=冻结后上一次冻结电量
            float[][] flt_DL = new float[][] { new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber] };
            #endregion

            base.Verify();

            if (!PowerOn())
            {
                RefNoResoult();
                MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                return;
            }
            #region ------------------正式冻结前保存电表原有数据-------------------
            //读冻结模式字
            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            MessageAdd("开始读取冻结模式字", EnumLogType.提示信息);
            FillPatternWord(ref lstFreezePW[0]);  //04 00 09 05 FF

            if (Stop) return;
            MessageAdd("开始读取整点冻结起始时间", EnumLogType.提示信息);
            FillFreezeTime(ref str_StartTime[0], 5);

            if (Stop) return;
            MessageAdd("开始读取整点冻结间隔时间", EnumLogType.提示信息);
            FillFreezeTime(ref str_Interval[0], 6);

            if (Stop) return;
            Identity();

            MessageAdd("开始设置整点冻结间隔时间", EnumLogType.提示信息);
            bool[] bs = MeterProtocolAdapter.Instance.WriteData("整点冻结时间间隔", "60");
            bool b = GetArrValue(bs);
            if (!b)
            {
                MessageAdd("整点冻结失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
                return;
            }

            if (Stop) return;
            MessageAdd("开始设置整点冻结起始时间", EnumLogType.提示信息);
            string time = DateTime.Now.AddHours(1).ToString("yyMMddhh") + "00";
            bs = MeterProtocolAdapter.Instance.WriteData("整点冻结起始时间", time);
            b = GetArrValue(bs);
            if (!b)
            {
                MessageAdd("整点冻结失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
                return;
            }

            if (Stop) return;
            MessageAdd("整点冻结前读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, 1, ref flt_DL[0]);

            if (Stop) return;
            MessageAdd("整点冻结前读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, 1, ref flt_DL[1]);


            //判断冻结电量是否与当前电量相等
            bool flag = false;
            for (int j = 0; j < flt_DL[0].Length; j++)
            {
                if (flt_DL[0][j] == flt_DL[1][j])
                {
                    flag = true;
                    break;
                }
            }
            //冻结电量与当前电量相等，则进行走字
            if (!Stop && flag)
            {
                MessageAdd("进行走字30S，请稍候......", EnumLogType.提示信息);
                float Xib = Core.Function.Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
                //升源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
                {
                    RefNoResoult();
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    return;
                }
                WaitTime("走电量", 30);

                MessageAdd("正在设置冻结测试参数", EnumLogType.提示信息);
                PowerOn();
                //读冻结模式字
                MessageAdd("等待电表启动......", EnumLogType.提示信息);
                Thread.Sleep(5000);  //延时源稳定5S
            }
            #endregion


            #region ------------------冻结处理------------------
            if (Stop) return;
            MessageAdd("开始进行整点冻结", EnumLogType.提示信息);
            bs = MeterProtocolAdapter.Instance.FreezeCmd("99999900");

            b = GetArrValue(bs);
            if (!b) MessageAdd("整点冻结失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);

            if (Stop) return;
            Identity();


            //设置电表时间为下一整点前2秒
            MessageAdd("恢复电表时间为当前时间", EnumLogType.提示信息);
            DateTime now = DateTime.Now;
            bs = MeterProtocolAdapter.Instance.WriteDateTime(new DateTime(now.Year, now.Month, now.Day, now.Hour, 59, 58));
            b = GetArrValue(bs);
            if (!b)    MessageAdd("整点冻结失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);

            WaitTime("延时", 8);
            #endregion

            #region -------------------读取冻结后的数据-----------------
            if (Stop) return;
            MessageAdd("整点冻结后读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, 1, ref flt_DL[2]);

            if (Stop) return;
            MessageAdd("整点冻结后读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, 1, ref flt_DL[3]);

            //恢复冻结模式字
            if (Stop) return;
            MessageAdd("开始恢复整点冻结模式字", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WritePatternWord(5, lstFreezePW[0][0]);

            //恢复起始冻结时间
            MessageAdd("开始恢复整点冻结起始时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("整点冻结起始时间", str_StartTime[0][0]);

            //恢复冻结间隔
            MessageAdd("开始恢复整点冻结间隔时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("整点冻结时间间隔", str_Interval[0][0]);

            //上报检定数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["冻结前上一次冻结电量"][j] = flt_DL[0][j].ToString();
                ResultDictionary["冻结前电量"][j] = flt_DL[1][j].ToString();
                ResultDictionary["冻结后电量"][j] = flt_DL[2][j].ToString();
                ResultDictionary["冻结后上一次冻结电量"][j] = flt_DL[3][j].ToString();
            }
            #endregion
            RefUIData("冻结前上一次冻结电量");
            RefUIData("冻结前电量");
            RefUIData("冻结后电量");
            RefUIData("冻结后上一次冻结电量");
            #region -----------------------结果处理--------------------
            string[] resultKey = new string[MeterNumber];
            //处理结论
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (flt_DL[2][j] == flt_DL[3][j])
                    ResultDictionary["结论"][j] = "合格";
                else
                    ResultDictionary["结论"][j] = "不合格";
            }
            RefUIData("结论");
            #endregion

            //恢复表时间为当前时间
            if (Stop) return;
            Identity();
            MessageAdd("恢复电表时间为当前时间", EnumLogType.提示信息);

            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);

            DateTime readTime = DateTime.Now; //读取GPS时间

            if (Stop) return;
            bs = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            b = GetArrValue(bs);
            if (!b) MessageAdd("写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);

            MessageAdd("整点冻结检定完毕", EnumLogType.提示信息);


        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "冻结前上一次冻结电量", "冻结前电量", "冻结后电量", "冻结后上一次冻结电量", "结论" };
            return true;
        }
        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="isSpecial">true=冻结电量，false=实际电量</param>
        /// <param name="powers">存储所有表位电量</param>
        /// <returns></returns>
        private void ReadDL(bool isSpecial, int times, ref float[] powers)
        {
            Dictionary<int, float[]> dicEnergy;

            if (isSpecial)
                dicEnergy = MeterProtocolAdapter.Instance.ReadSpecialEnergy(5, times);
            else
                dicEnergy = MeterProtocolAdapter.Instance.ReadEnergys((int)PowerWay.正向有功, 0);

            for (int j = 0; j < MeterNumber; j++)
            {
                if (Stop) return;
                if (!MeterInfo[j].YaoJianYn || !dicEnergy.ContainsKey(j)) continue;

                float[] dl = dicEnergy[j];
                if (dl[0] < 0F)
                    MessageAdd($"表位{j + 1}返回的数据不符合要求", EnumLogType.提示信息);
                else
                    powers[j] = dl[0];
            }

        }

        /// <summary>
        /// 存储冻结模式字
        /// </summary>
        /// <param name="pw"></param>
        /// <returns></returns>
        private void FillPatternWord(ref string[] pw)
        {
            string[] rData = MeterProtocolAdapter.Instance.ReadPatternWord(5);

            for (int k = 0; k < MeterNumber; k++)
            {
                if (Stop) return;
                if (!MeterInfo[k].YaoJianYn) continue;

                if (rData[k] == "")
                    MessageAdd($"表位{k + 1}读取冻结模式字失败", EnumLogType.提示信息);

                pw[k] = rData[k];
            }
        }

        /// <summary>
        /// 存储整点冻结起始时间
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private void FillFreezeTime(ref string[] startTime, int type)
        {
            string[] rData = MeterProtocolAdapter.Instance.ReadFreezeTime(type);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                if (!MeterInfo[i].YaoJianYn) continue;
                if (rData[i] == "") continue;

                startTime[i] = rData[i];
            }
        }

        private void RefNoResoult()
        {

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = "不合格";

            }
            RefUIData("结论");
        }


    }
}
