using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 定时冻结检测
    /// </summary>
    public class Freeze_Timing : VerifyBase
    {

        /// <summary>
        /// 定时冻结检定
        /// </summary>
        /// <param name="ItemNumber"></param>
        public override void Verify()
        {
            base.Verify();

            if (!PowerOn())
            {
                MessageAdd("源输出失败", EnumLogType.提示信息);
                return;
            }
            MessageAdd("开始读取冻结模式字", EnumLogType.提示信息);
            ReadMeterAddrAndNo();

            for (int i = 0; i < 3; i++)
            {
                //0月冻结，1日冻结，2小时冻结
                FreezeDeal(i.ToString());
            }

            //恢复表时间为当前时间
            Identity();
            MessageAdd("恢复电表时间为当前时间", EnumLogType.提示信息);

            bool[] bReturn = MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
            bool bResult = GetArrValue(bReturn);
            if (!bResult)
            {
                MessageAdd("写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }

            //结论
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                if (ResultDictionary["月冻结结论"][j] == "合格" && ResultDictionary["日冻结结论"][j] == "合格" && ResultDictionary["小时冻结结论"][j] == "合格")
                {
                    ResultDictionary["结论"][j] = "合格";
                }
                else
                {
                    ResultDictionary["结论"][j] = "不合格";

                }
            }
            RefUIData("结论");
            MessageAdd("定时冻结检定完毕", EnumLogType.提示信息);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "月冻结前上一次冻结电量", "月冻结前电量", "月冻结后电量", "月冻结后上一次冻结总电量", "月冻结结论",
             "日冻结前上一次冻结电量", "日冻结前电量", "日冻结后电量", "日冻结后上一次冻结电量", "日冻结结论" ,"小时冻结前上一次冻结电量", "小时冻结前电量", "小时冻结后电量", "小时冻结后上一次冻结电量", "小时冻结结论","结论"};
            return true;
        }
        /// <summary>
        /// 冻结处理
        /// </summary>
        /// <param name="str_Type">冻结类别，0-月冻结 1-日冻结 2-小时冻结</param>
        /// <returns></returns>
        private void FreezeDeal(string str_Type)
        {
            #region -------------------初始化变量-------------------
            string msg = string.Empty;
            string str_Item = string.Empty;
            bool flagDL = false;                    //冻结电量是否与当前电量相等标志
            //维数 0=冻结前上一次冻结电量，1=冻结前当前表电量，2=冻结后当前表电量，3=冻结后上一次冻结电量
            float[][] flt_DL = new float[][] { new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber] };

            bool bResult;
            #endregion

            #region ------------------初始化冻结参数----------------------
            string str_DateTime = "";
            switch (str_Type)
            {
                case "0":
                    str_DateTime = "99010000";
                    msg = "月冻结";
                    break;
                case "1":
                    str_DateTime = "99990000";
                    msg = "日冻结";
                    break;
                case "2":
                    str_DateTime = "99999930";
                    msg = "小时冻结";
                    break;
            }
            #endregion

            #region ----------------读取冻结前数据------------------
            MessageAdd(msg + "前读取上一次冻结总电量", EnumLogType.提示信息);
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;

            ReadDL(true, 1, ref flt_DL[0]);
            MessageAdd(msg + "前读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, 1, ref flt_DL[1]);
            //判断冻结电量是否与当前电量相等
            for (int j = 0; j < flt_DL[0].Length; j++)
            {
                if (flt_DL[0][j] == flt_DL[1][j])// && flt_DL[0][j] != 0
                {
                    flagDL = true;
                    break;
                }
            }
            //冻结电量与当前电量相等，则进行走字
            if (flagDL)
            {
                MessageAdd("最大电流进行走字15S，请稍候......", EnumLogType.提示信息);
                float Xib = Core.Function.Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
                //升源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
                {
                    RefNoResoult();
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    return;
                }
                WaitTime("走电量", 30);

                Thread.Sleep(15000);
                PowerOn();

                //读冻结模式字
                MessageAdd("等待电表启动......", EnumLogType.提示信息);
                Thread.Sleep(5000);  //延时源稳定5S
            }
            #endregion

            #region ----------------执行冻结操作----------------------
            MessageAdd("开始进行" + msg, EnumLogType.提示信息);
            bool[] bReturn = MeterProtocolAdapter.Instance.FreezeCmd(str_DateTime);
            bResult = GetArrValue(bReturn);
            if (!bResult)
            {
                MessageAdd(msg + "失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }

            DateTime now = DateTime.Now;
            DateTime meterTime = now;
            switch (str_Type)
            {
                case "0":
                    //设置为下月初0时前1秒钟 yyMMddHHmmss
                    meterTime = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1);
                    break;
                case "1":
                    //设置为下一日0时前1秒钟 yyMMddHHmmss
                    meterTime = DateTime.Parse(now.AddDays(1).ToString("yyyy/MM/dd 23:59:59"));
                    break;
                case "2":
                    //设置为当前小时的半点前1秒钟 yyMMddHHmmss
                    now.AddMinutes(30);
                    meterTime = DateTime.Parse(now.AddMinutes(30).ToString("yyyy/MM/dd HH:29:59"));
                    break;
            }
            Identity();
            MeterProtocolAdapter.Instance.WriteDateTime(meterTime);

            MessageAdd("延时15S,请稍候......", EnumLogType.提示信息);
            Thread.Sleep(15000);
            #endregion
            //if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;

            #region -------------------读取冻结后的数据-----------------
            MessageAdd(msg + "后读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, 1, ref flt_DL[2]);
            MessageAdd(msg + "后读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, 1, ref flt_DL[3]);
            #endregion

            #region -------------------上报数据-----------------

            //上报检定数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;


                ResultDictionary[msg + "前上一次冻结电量"][j] = flt_DL[0][j].ToString();
                ResultDictionary[msg + "前电量"][j] = flt_DL[1][j].ToString();
                ResultDictionary[msg + "后电量"][j] = flt_DL[2][j].ToString();
                ResultDictionary[msg + "后上一次冻结电量"][j] = flt_DL[3][j].ToString();
                if (Math.Abs(flt_DL[2][j] - flt_DL[3][j]) < 0.01)
                    ResultDictionary[msg + "结论"][j] = flt_DL[3][j].ToString();

            }
            RefUIData(msg + "前上一次冻结电量");
            RefUIData(msg + "前电量");
            RefUIData(msg + "后电量");
            RefUIData(msg + "后上一次冻结电量");
            RefUIData(msg + "结论");
        }
        #endregion


        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="isSpecial">true=冻结电量，false=实际电量</param>
        /// <param name="allBWDL">存储所有表位电量</param>
        /// <returns></returns>
        private void ReadDL(bool isSpecial, int times, ref float[] allBWDL)
        {
            Dictionary<int, float[]> dicEnergy;
            if (isSpecial)
                dicEnergy = MeterProtocolAdapter.Instance.ReadSpecialEnergy(3, times);
            else
                dicEnergy = MeterProtocolAdapter.Instance.ReadEnergys((int)PowerWay.正向有功, 0);

            for (int j = 0; j < MeterNumber; j++)
            {
                //强制停止
                if (Stop) return;
                if (!MeterInfo[j].YaoJianYn || !dicEnergy.ContainsKey(j)) continue;

                float[] dl = dicEnergy[j];
                if (dl.Length <= 0 || dl[0] < 0F)
                {
                    MessageAdd($"表位{j + 1}返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    allBWDL[j] = dl[0];
                }
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