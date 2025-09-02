using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 结算日冻结检定
    /// </summary>
    public class Freeze_CheckedDate : VerifyBase
    {
        float Xib = 1f;
        /// <summary>
        /// 结算日冻结检定
        /// </summary>
        public override void Verify()
        {
            base.Verify();

            if (Stop) return;

            MessageAdd("升电压", EnumLogType.提示信息);
            if (!PowerOn())
            {
                MessageAdd("源输出失败", EnumLogType.提示信息);
                RefNoResoult();
                return;
            }

            WaitTime("等待电表启动", 10);

            FreezeDeal();

            //恢复表时间为当前时间
            Identity();

            MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());

            MessageAdd("结算日冻结检定完毕", EnumLogType.提示信息);

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
            Identity();

            MessageAdd("结算日冻结前读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, ref powers[0]);
            RefDJData(powers[0], "冻结前上一次冻结总电量");

            MessageAdd("结算日冻结前读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, ref powers[1]);
            RefDJData(powers[1], "冻结前上一次冻结总电量");
            //判断冻结电量是否与当前电量相等
            //for (int j = 0; j < powers[0].Length; j++)
            //{
            //    if (powers[0][j] == powers[1][j])// && flt_DL[0][j] != 0
            //    {
            //        //bool flag = true;
            //        break;
            //    }
            //}
            //冻结电量与当前电量相等，则进行走字
            //if (flag)
            {
                MessageAdd("进行走字30S，请稍候......", EnumLogType.提示信息);
                //升源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
                {
                    RefNoResoult();
                    MessageAdd("升源失败,退出检定", EnumLogType.流程信息);
                    return;
                }
                WaitTime("走电量", 30);
                PowerOff();

                MessageAdd("延时10S,等待重新升源,请稍候......", EnumLogType.提示信息);
                Thread.Sleep(10000);

                MessageAdd("正在设置冻结测试参数", EnumLogType.提示信息);

                //读冻结模式字
                PowerOn();
                MessageAdd("等待电表启动......", EnumLogType.提示信息);
                Thread.Sleep(5000);  //延时源稳定5S
            }
            #endregion

            #region -------------------结算日冻结处理----------------

            Identity(true);
            if (Stop) return;
            string checkedDate = "0100";
            string[] data = MeterProtocolAdapter.Instance.ReadData("每月第1结算日");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrEmpty(data[i])) continue;
                checkedDate = data[i];
                break;
            }

            if (Stop) return;
            DateTime now = DateTime.Now;
            DateTime dt = new DateTime(now.Year, now.Month, int.Parse(checkedDate.Substring(0, 2)), int.Parse(checkedDate.Substring(2, 2)), 0, 0);
            dt = dt.AddSeconds(-30);
            MessageAdd("设置电表时间为结算日冻结时间前" + dt.ToString("yyyyMMdd HH:mm:ss"), EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteDateTime(dt);

            if (Stop) return;
            WaitTime("等待冻结产生", 120);

            if (Stop) return;
            MessageAdd("冻结后电量", EnumLogType.提示信息);
            ReadDL(false, ref powers[2]);
            RefDJData(powers[2], "冻结后电量");


            if (Stop) return;
            MessageAdd("冻结后上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, ref powers[3]);
            RefDJData(powers[3], "冻结后上一次冻结总电量");


            ////上报检定数据
            //for (int j = 0; j < MeterNumber; j++)
            //{
            //    if (!meterInfo[j].YaoJianYn) continue;

            //    ResultDictionary["冻结前上一次冻结总电量"][j] = powers[0][j].ToString();
            //    ResultDictionary["冻结前电量"][j] = powers[1][j].ToString();
            //    ResultDictionary["冻结后电量"][j] = powers[2][j].ToString();
            //    ResultDictionary["冻结后上一次冻结总电量"][j] = powers[3][j].ToString();

            //}
            //RefUIData("冻结前上一次冻结总电量");
            //RefUIData("冻结前电量");
            //RefUIData("冻结后电量");
            //RefUIData("冻结后上一次冻结总电量");
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
            ResultNames = new string[] { "冻结前上一次冻结总电量", "冻结前电量", "冻结后电量", "冻结后上一次冻结总电量", "结论" };

            try
            {
                Xib = Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
            }
            catch (Exception ex)
            {
                MessageAdd("数据校验失败\r\n" + ex.ToString(), EnumLogType.错误信息);
                RefNoResoult();
                return false;
            }

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
            string[] dicEnergy = new string[MeterNumber];
            if (isSpecial)
            {
                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    dicEnergy = MeterProtocolAdapter.Instance.ReadData("(上1结算日)正向有功总电能");
                }
                else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {

                    if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                    List<string> oad = new List<string> { "50050200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
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
            }
            else
            {
                dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");
                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
                    }
                }
            }
            if (Stop) return;
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[j]))
                {
                    MessageAdd($"表位[{j + 1}]返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    powers[j] = Convert.ToSingle(dicEnergy[j]);
                }

            }




        }

        private void RefDJData(float[] data, string name)
        {

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name][j] = data[j].ToString();

            }
            RefUIData(name);
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
