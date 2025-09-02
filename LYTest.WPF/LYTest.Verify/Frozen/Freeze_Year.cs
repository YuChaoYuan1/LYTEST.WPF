using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol;
using LYTest.MeterProtocol.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 年冻结检定,只支持698功能
    /// </summary>
    public class Freeze_Year : VerifyBase
    {
        /// <summary>
        /// 年冻结检定
        /// </summary>
        /// <param name="ItemNumber"></param>
        public override void Verify()
        {
            base.Verify();
            if (!PowerOn())
            {
                MessageAdd("源输出失败", EnumLogType.提示信息);
                RefNoResoult();
                return;
            }
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);  //延时源稳定5S
            ReadMeterAddrAndNo();

            //恢复表时间为当前时间
            //Identity();
            //DateTime now = DateTime.Now;
            //DateTime dt = new DateTime(now.Year - 1, 1, 1, 0, 0, 0).AddSeconds(-5);

            //if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;

            //MeterProtocolAdapter.Instance.WriteDateTime(dt);

            //if (Stop) return;
            //if (App.g_ChannelType == Cus_ChannelType.通讯485)
            //    SwitchChannel(Cus_ChannelType.通讯485);

            //WaitTime("等待冻结产生", 30);

            FreezeDeal();

            #region -----------------恢复电表原来状态---------------

            //恢复表时间为当前时间
            Identity(false);

            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;  //读取GPS时间

            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;

            bool[] bReturnData = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            bool bResult = GetArrValue(bReturnData);
            if (!bResult)
                MessageAdd("写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);


            #endregion
            MessageAdd("年冻结检定完毕", EnumLogType.提示信息);

        }

        /// <summary>
        /// 冻结处理
        /// </summary>
        /// <returns></returns>
        private void FreezeDeal()
        {
            //维数 0=冻结前上一次冻结电量，1=冻结前当前表电量，2=冻结后当前表电量，3=冻结后上一次冻结电量
            float[][] powers = new float[][] { new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber] };

            #region ----------------读取冻结前数据------------------
            if (Stop) return;
            MessageAdd("年冻结前读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, ref powers[0]);

            if (Stop) return;
            MessageAdd("年冻结前读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, ref powers[1]);
            bool flag = false;
            //判断冻结电量是否与当前电量相等
            for (int j = 0; j < powers[0].Length; j++)
            {
                if (powers[0][j] == powers[1][j])
                {
                    flag = true;
                    break;
                }
            }
            //冻结电量与当前电量相等，则进行走字
            if (flag)
            {
                MessageAdd("进行走字30S，请稍候......", EnumLogType.提示信息);
              float  Xib = Core.Function. Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
                //升源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
                {
                    RefNoResoult();
                    MessageAdd("升源失败,退出检定", EnumLogType.流程信息);
                    return;
                }
                WaitTime("走电量", 30);



                MessageAdd("正在设置冻结测试参数", EnumLogType.提示信息);
                PowerOn();
                WaitTime("正在关闭电流", 5);

                //读冻结模式字
                MessageAdd("等待电表启动......", EnumLogType.提示信息);
            }
            #endregion
            #region -------------------年冻结处理----------------

            //设置为下一日0时59秒 yyMMddHHmmss
            if (Stop) return;
            if (App.g_ChannelType == Cus_ChannelType.通讯载波)
                SwitchChannel(Cus_ChannelType.通讯载波);

            if (Stop) return;
            Identity(false);

            if (Stop) return;
            MessageAdd("设置电表时间为年冻结时间前", EnumLogType.提示信息);
            DateTime now = DateTime.Now;
            DateTime dt = new DateTime(now.Year + 1, 1, 1, 0, 0, 0).AddSeconds(-5);

            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;

            MeterProtocolAdapter.Instance.WriteDateTime(dt);

            if (Stop) return;
            if (App.g_ChannelType == Cus_ChannelType.通讯485)
                SwitchChannel(Cus_ChannelType.通讯485);

            WaitTime("等待冻结产生", 30);

            if (Stop) return;
            MessageAdd("年冻结后读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, ref powers[2]);

            if (Stop) return;
            MessageAdd("年冻结后读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, ref powers[3]);

            //上报检定数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["年冻结前上一次冻结总电量"][j] = powers[0][j].ToString();
                ResultDictionary["年冻结前电量"][j] = powers[1][j].ToString();
                ResultDictionary["年冻结后电量"][j] = powers[2][j].ToString();
                ResultDictionary["年冻结后上一次冻结总电量"][j] = powers[3][j].ToString();
            }
            RefUIData("年冻结前上一次冻结总电量");
            RefUIData("年冻结前电量");
            RefUIData("年冻结后电量");
            RefUIData("年冻结后上一次冻结总电量");
            #endregion
            #region --------------------处理结论------------------
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = (powers[2][j] == powers[3][j]) ? "合格" : "不合格";
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
            ResultNames = new string[] { "年冻结前上一次冻结总电量", "年冻结前电量", "年冻结后电量", "年冻结后上一次冻结总电量", "结论" };
            return true;
        }
        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="isSpecial">true=冻结电量，false=实际电量</param>
        /// <param name="powers">存储所有表位电量</param>
        /// <returns></returns>
        private void ReadDL(bool isSpecial, ref float[] powers)
        {
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
            string[] dicEnergy = new string[MeterNumber];
            if (isSpecial)
            {
                

                List<string> oad = new List<string> { "50070200" };
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                //if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
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
            }
            else
            {
                dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
                }

            }
            if (Stop) return;
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[j]))
                {
                    MessageAdd($"表位{j + 1}返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    powers[j] = Convert.ToSingle(dicEnergy[j]);
                }

            }

        }

        //private void RefDJData(float[] data, string name)
        //{

        //    for (int j = 0; j < MeterNumber; j++)
        //    {
        //        if (!MeterInfo[j].YaoJianYn) continue;
        //        ResultDictionary[name][j] = data[j].ToString();

        //    }
        //    RefUIData(name);
        //}

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
