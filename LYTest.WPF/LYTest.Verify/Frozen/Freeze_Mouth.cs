using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 月冻结检定,只支持698功能
    /// </summary>
    public class Freeze_Mouth : VerifyBase
    {
        /// <summary>
        /// 月冻结检定
        /// </summary>
        /// <param name="ItemNumber"></param>
        public override void Verify()
        {
            base.Verify();

            if (!PowerOn())
            {
                RefNoResoult();
                MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                return;
            }
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);  //延时源稳定5S
            ReadMeterAddrAndNo();

            FreezeDeal();

            //恢复表时间为当前时间
            Identity();

            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;  //读取GPS时间

            bool[] bReturnData = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            bool bResult = GetArrValue(bReturnData);
            if (!bResult) MessageAdd("写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);

            MessageAdd("月冻结检定完毕", EnumLogType.提示信息);

        }

        /// <summary>
        /// 冻结处理
        /// </summary>
        private void FreezeDeal()
        {
            int num = Convert.ToInt16(Test_Value);//冻结次数
            if (num == 0) num = 1;

            float[][] freezeDL = new float[MeterNumber][];//冻结电量
            for (int i = 0; i < MeterNumber; i++)
            {
                freezeDL[i] = new float[num];
            }

            if (Stop) return;
            MessageAdd("月冻结前读取当前电表总电量", EnumLogType.提示信息);
            float[] freezeQDL = ReadDL();

            if (Stop) return;
            MessageAdd("进行走字120S，请稍候......", EnumLogType.提示信息);
            float Xib = Core.Function.Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
            //升源
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
            {
                RefNoResoult();
                MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                return;
            }
            WaitTime("走电量", 30);


            #region 生成冻结
            DateTime now = DateTime.Now;
            for (int i = 0; i < num; i++)
            {
                if (Stop) return;
                WaitTime("走字", 120);

                if (Stop) return;
                Identity();

                if (Stop) return;
                MessageAdd("设置电表时间为日冻结时间前", EnumLogType.提示信息);
                now = now.AddMonths(1);
                DateTime dt = new DateTime(now.Year, now.Month, 1, 23, 59, 50).AddDays(-1);
                MessageAdd($"设置冻结前时间为：{dt:G}", EnumLogType.提示信息);
                MeterProtocolAdapter.Instance.WriteDateTime(dt);

                if (Stop) return;
                WaitTime($"等待第[{i + 1}]次月冻结产生", 60);
            }

            if (Stop) return;
            PowerOn();
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);  //延时源稳定5S
            #endregion
            if(DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
            #region -------------------月冻结处理----------------
            for (int i = 0; i < num; i++)
            {
                if (Stop) return;
                MessageAdd($"月冻结后读取上{i + 1}次冻结总电量", EnumLogType.提示信息);
                float[] powers = ReadFreeze(i + 1);
                for (int j = 0; j < MeterNumber; j++)
                {
                    freezeDL[j][i] = powers[j];
                }
            }

            MessageAdd("月冻结后读取当前电表总电量", EnumLogType.提示信息);
          
            float[] freezeHDL = ReadDL();


            //上报检定数据
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                string dl = "";
                foreach (float f in freezeDL[i])
                {
                    dl = f.ToString("f2") + "," + dl;
                }
                dl = dl.TrimEnd(',');

                ResultDictionary["试验前电量"][i] = freezeQDL[i].ToString();

                ResultDictionary["第1次冻结电量"][i] = dl.Split(',')[0].ToString();

                ResultDictionary["试验后电量"][i] = freezeHDL[i].ToString();
            }

            RefUIData("试验前电量");

            RefUIData("第1次冻结电量");

            RefUIData("试验后电量");

            #endregion

            #region --------------------处理结论------------------
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                string rst1 = "合格";
                if (freezeDL[j].Length > 1)
                {
                    for (int f = 0; f < freezeDL[j].Length - 1; f++)
                    {
                        if (freezeDL[j][f] < freezeDL[j][f + 1])
                        {
                            rst1 = "不合格";
                            break;
                        }
                    }
                }
                else
                {
                    if (freezeDL[j][0] <= freezeQDL[j])
                        rst1 = "不合格";
                }
                ResultDictionary["结论"][j] = rst1;
            }
            RefUIData("结论");
            #endregion
        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "试验前电量", "第1次冻结电量", "试验后电量", "结论" };
            return true;
        }
        /// <summary>
        /// 读取电量
        /// </summary>
        /// <returns></returns>
        private float[] ReadDL()
        {
            string[] dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
            }


            float[] powers = new float[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[i]))
                {
                    MessageAdd($"表位[{i + 1}]返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    powers[i] = Convert.ToSingle(dicEnergy[i]);
                }
            }

            return powers;

        }

        private float[] ReadFreeze(int num)
        {
            string[] dicEnergy = new string[MeterNumber];

            List<string> oad = new List<string> { "50060200" };

            Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            List<string> rcsd = new List<string>();
            Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, num, rcsd, ref dicObj);
            for (int i = 0; i < MeterNumber; ++i)
            {
                dicEnergy[i] = "";
                if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                {
                    if (dic[i].ContainsKey("00100200") || dic[i].ContainsKey("00100201")) //(当前)正向有功总电能
                    {
                        if (dic[i].ContainsKey("00100200")) //(当前)正向有功总电能
                        {
                            if (dic[i]["00100200"].Count <= 0) continue;
                            dicEnergy[i] = dic[i]["00100200"][0].ToString();
                            //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 100).ToString("f2");

                            //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                            dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 100) * 100) / 100).ToString();
                        }
                        else if (dic[i].ContainsKey("00100201")) //(当前)正向有功总电能
                        {
                            if (dic[i]["00100201"].Count <= 0) continue;
                            dicEnergy[i] = dic[i]["00100201"][0].ToString();
                            dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 100) * 100) / 100).ToString();
                        }
                    }
                    else
                    {
                        if (dic[i].ContainsKey("00100400")) //(当前)正向有功总电能
                        {
                            if (dic[i]["00100400"].Count <= 0) continue;
                            dicEnergy[i] = dic[i]["00100400"][0].ToString();
                            //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 10000).ToString("f2");

                            //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                            dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 10000) * 100) / 100).ToString();
                        }
                        else if (dic[i].ContainsKey("00100401")) //(当前)正向有功总电能
                        {
                            if (dic[i]["00100401"].Count <= 0) continue;
                            dicEnergy[i] = dic[i]["00100401"][0].ToString();
                            //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 10000).ToString("f2");

                            //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                            dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 10000) * 100) / 100).ToString();
                        }
                    }
                }
            }

            float[] powers = new float[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[i]))
                {
                    MessageAdd($"表位{i + 1}返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    powers[i] = Convert.ToSingle(dicEnergy[i]);
                }

            }
            return powers;

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
