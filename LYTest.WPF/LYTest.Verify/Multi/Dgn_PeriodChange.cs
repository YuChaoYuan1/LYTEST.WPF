using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LYTest.Verify.Multi
{
    ///add lsj 20220218 时段投切实验
    /// <summary>
    /// 时段投切实验
    /// </summary>
    public class Dgn_PeriodChange : VerifyBase
    {
        /// <summary>
        /// 走字的费率列表
        /// </summary>
        List<FL_Data> FL_List = new List<FL_Data>();
        /// <summary>
        ///是否读取表内费率时间
        /// </summary>
        bool IsReadFLTime = false;

        /// <summary>
        /// 走字的电流
        /// </summary>
        float Xib = 0;

        /// <summary>
        /// 功率方向
        /// </summary>
        readonly PowerWay powerWay = PowerWay.正向有功;
        public override void Verify()
        {
            base.Verify();
            if (!IsDemo)
            {
                PowerOn();//Adapter.ComAdpater.PowerOnOnlyU();
                WaitTime("正在升源", 8);
                //读取表地址和表号
                if (Stop) return;
                ReadMeterAddrAndNo();
                if (Stop) return;
                Identity(false);
                if (Stop) return;
                ////读取表内费率时间
                //if (IsReadFLTime)
                //{
                //    MessageAdd("正在读取表内费率时段", EnumLogType.提示信息);
                //    //MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                //    string[] readdata = MeterProtocolAdapter.Instance.ReadData("第一套第1日时段数据");
                //    for (int i = 0; i < readdata.Length; i++)
                //    {
                //        if (readdata[i] != null)
                //        {
                //            string fl = "";//00:30(谷),08:30(平),10:00(峰),13:00(平),19:30(峰)
                //            string period;
                //            while (readdata[i].Length >= 6)
                //            {
                //                period = readdata[i].Substring(readdata[i].Length - 6, 6);  //从后面开始取数
                //                period = period.Substring(0, 2) + ":" + period.Substring(2, 2) + "(" + ((Cus_FeiLv)int.Parse(period.Substring(4, 2))).ToString() + ")";
                //                fl = period + "," + fl;
                //                readdata[i] = readdata[i].Substring(0, readdata[i].Length - 6); //去除后面6字符
                //            }
                //            FL_List = GetFlList(fl, true);
                //            break;
                //        }
                //    }

                //}

                string UseMMdd = $"{DateTime.Now:MM-dd}";
                if (IsReadFLTime)
                {
                    MessageAdd("正在读取表内费率时段", EnumLogType.提示信息);

                    #region 获取日时段表数量
                    MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                    //string[] dayCounts = MeterProtocolAdapter.Instance.ReadData("日时段表数");
                    //for (int i = 0; i < dayCounts.Length; i++)
                    //{
                    //    if (!string.IsNullOrWhiteSpace(dayCounts[i]))
                    //    {
                    //        //int dayCount;
                    //        //int.TryParse(dayCounts[i], out dayCount);
                    //        break;
                    //    }
                    //}
                    #endregion

                    #region 获取第一套时区表数据&&验证所有表的时区表数据是否一致
                    string[] readTimeZone = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据");

                    string TimeZoneOne = null;

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (MeterInfo[i].YaoJianYn)
                        {
                            if (!string.IsNullOrEmpty(TimeZoneOne))
                            {
                                if (TimeZoneOne != readTimeZone[i])
                                {
                                    Stop = true;
                                    MessageAdd("要检表内第一套时区表数据不一致", EnumLogType.错误信息);
                                    break;
                                }
                            }
                            else
                            {
                                TimeZoneOne = readTimeZone[i];
                            }

                        }
                    }
                    if (Stop) return;

                    #endregion

                    // 计算月份获取当前应该使用的时段数据
                    int UseDayBase = Common.GetPeriodNN(TimeZoneOne, (byte)DateTime.Now.Month, (byte)DateTime.Now.Day);

                    #region 获取对应的费率
                    MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                    string[] readdata = MeterProtocolAdapter.Instance.ReadData("第一套第" + UseDayBase + "日时段数据");

                    for (int i = 0; i < readdata.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(readdata[i]))
                        {
                            string fl = "";//00:30(谷),08:30(平),10:00(峰),13:00(平),19:30(峰)
                            string period;
                            while (readdata[i].Length >= 6)
                            {
                                period = readdata[i].Substring(readdata[i].Length - 6, 6);  //从后面开始取数
                                period = period.Substring(0, 2) + ":" + period.Substring(2, 2) + "(" + ((Cus_FeiLv)int.Parse(period.Substring(4, 2))).ToString() + ")";
                                fl = period + "," + fl;
                                readdata[i] = readdata[i].Substring(0, readdata[i].Length - 6); //去除后面6字符
                            }
                            FL_List = GetFlList(fl, true);
                            break;
                        }
                    }

                    #endregion

                }

                string[] data = Test_Value.Split('|');
                string str = "";
                for (int i = 0; i < FL_List.Count; i++)
                {
                    FL_List[i].Date = UseMMdd;
                    str += FL_List[i].Time + "(" + FL_List[i].Type + ")" + ",";
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["标准投切时间"][i] = str.TrimEnd(',');
                    ResultDictionary["结论"][i] = "合格";     //开始默认合格
                }
                RefUIData("标准投切时间");
                RefUIData("结论");

                //循环所有费率-开始走字
                for (int i = 0; i < FL_List.Count; i++)
                {
                    if (Stop) return;
                    Identity(false); //身份认证
                    DateTime time = DateTime.Parse($"{DateTime.Now.Year}-{FL_List[i].Date} {FL_List[i].Time}:10");
                    MessageAdd("修改电表时间到" + time.ToString("yyyy年MM月dd日 HH时mm分ss秒"), EnumLogType.提示与流程信息);
                    MeterProtocolAdapter.Instance.WriteDateTime(time);

                    MessageAdd("正在读取起始电量", EnumLogType.提示信息);
                    float[] State_Energy = MeterProtocolAdapter.Instance.ReadEnergy((byte)powerWay, (byte)FL_List[i].Type);
                    if (Stop) return;
                    if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Cus_PowerYuanJian.H, powerWay, "1") == false)
                    {
                        MessageAdd("控制源输出失败", EnumLogType.提示信息);
                        return;
                    }
                    //记录开始的时间
                    DateTime startTime = DateTime.Now;
                    bool[] Resoult = new bool[MeterNumber];//当前费率下的每个表结论
                    TimeSpan[] Err = new TimeSpan[MeterNumber];//存放误差时间

                    MessageAdd("正在进行走字,读取实时电量", EnumLogType.提示信息);
                    MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                    //开始走字-判断电量是否改变
                    while (true)
                    {
                        if (Stop) break;
                        if (TimeSubms(DateTime.Now, startTime) > 300 * 1000) //超出最大处理时间
                        {

                            DateTime TQTime = DateTime.Parse($"{DateTime.Now.Year}-{FL_List[i].Date} {FL_List[i].Time}").AddSeconds((long)(DateTime.Now - startTime).TotalSeconds);

                            for (int j = 0; j < MeterNumber; j++)
                            {
                                if (!MeterInfo[j].YaoJianYn || Resoult[j]) continue;
                                //实际投切时间
                                Err[j] = TimeSpan.FromSeconds(TQTime.Subtract(startTime).TotalSeconds);
                                if (string.IsNullOrWhiteSpace(ResultDictionary["实际投切时间"][j]))
                                {
                                    ResultDictionary["实际投切时间"][j] += TQTime.ToString("HH:mm:ss");
                                    ResultDictionary["投切误差"][j] = Err[j].ToString(@"hh\:mm\:ss");
                                }
                                else
                                {
                                    ResultDictionary["实际投切时间"][j] += "," + TQTime.ToString("HH:mm:ss");
                                    ResultDictionary["投切误差"][j] += "," + Err[j].ToString(@"hh\:mm\:ss");
                                }
                            }
                            break;
                        }
                        //读取当前电量
                        float[] End_Energy = MeterProtocolAdapter.Instance.ReadEnergy((byte)powerWay, (byte)FL_List[i].Type);
                        if (Stop) break;
                        for (int j = 0; j < MeterNumber; j++)
                        {
                            if (!MeterInfo[j].YaoJianYn || Resoult[j]) continue;
                            if (End_Energy[j] == -1) continue; //-1表示没有读到
                            if (End_Energy[j] - State_Energy[j] > 0)//电量发生了改变
                            {
                                Resoult[j] = true; //电量发生改变就是合格了
                                ///实际投切时间
                                DateTime TQTime = DateTime.Parse($"{DateTime.Now.Year}-{FL_List[i].Date} {FL_List[i].Time}").AddSeconds((long)(DateTime.Now - startTime).TotalSeconds);
                                Err[j] = TimeSpan.FromSeconds(TQTime.Subtract(time).TotalSeconds);
                                if (string.IsNullOrWhiteSpace(ResultDictionary["实际投切时间"][j]))
                                {
                                    ResultDictionary["实际投切时间"][j] += TQTime.ToString("HH:mm:ss");
                                    ResultDictionary["投切误差"][j] = Err[j].ToString(@"hh\:mm\:ss");
                                }
                                else
                                {
                                    ResultDictionary["实际投切时间"][j] += "," + TQTime.ToString("HH:mm:ss");
                                    ResultDictionary["投切误差"][j] += "," + Err[j].ToString(@"hh\:mm\:ss");
                                }
                            }
                        }
                        bool T = true;
                        //判断所有要检表位是否都结束了
                        for (int j = 0; j < MeterNumber; j++)
                            if (MeterInfo[j].YaoJianYn != Resoult[j]) T = false;
                        if (T) break; ;
                    }
                    if (Stop) return;
                    RefUIData("实际投切时间");
                    RefUIData("投切误差");
                    PowerOn();
                    WaitTime("正在关闭电流", 5);
                    for (int j = 0; j < MeterNumber; j++) //只要有某个费率不合格总结论就是不合格 
                    {
                        if (!MeterInfo[j].YaoJianYn) continue;
                        if (!Resoult[j] || Err[j].Seconds > 5 * 60) ResultDictionary["结论"][j] = "不合格";
                    }
                    if (Stop) return;
                }
                RefUIData("结论");
                MessageAdd("正在恢复电能表时间...", EnumLogType.提示信息);
                Identity(false);
                MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
            }
            else//演示模式
            {
                string str = "";
                string str2 = "";
                string str3 = "";
                TimeSpan ts = new TimeSpan(0, 0, 1);

                for (int i = 0; i < FL_List.Count; i++)
                {
                    str += FL_List[i].Time + ",";
                    str2 += DateTime.Parse($"{DateTime.Now.Year}-{FL_List[i].Date} {FL_List[i].Time}").AddSeconds(1).ToString("HH:mm:ss") + ",";
                    str3 += ts.ToString(@"hh\:mm\:ss") + ",";
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["标准投切时间"][i] = str.TrimEnd(',');
                    ResultDictionary["实际投切时间"][i] = str2.TrimEnd(',');
                    ResultDictionary["投切误差"][i] = str3.TrimEnd(',');
                    ResultDictionary["结论"][i] = "合格";
                }
                RefUIData("标准投切时间");
                RefUIData("实际投切时间");
                RefUIData("投切误差");
                RefUIData("结论");
                return;
            }



        }

        protected override bool CheckPara()
        {
            string[] data = Test_Value.Split('|');
            if (data.Length < 3) return false;
            if (data[2] == "是")     // 00:30(谷),08:30(平),10:00(峰),13:00(平),19:30(峰)
            {
                IsReadFLTime = true;
            }
            else
            {
                FL_List = GetFlList(data[0], true);
            }
            Xib = Number.GetCurrentByIb(data[1], OneMeterInfo.MD_UA, HGQ);
            ResultNames = new string[] { "标准投切时间", "实际投切时间", "投切误差", "结论" };
            return true;
        }

        //#region 用到的方法
        ///// <summary>
        ///// 解析费率时间
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //private List<FL_Data> GetFlList(string data, bool IsCF)
        //{
        //    //以防输错情况，把中文字符转成英文字符
        //    data = data.Replace("（", "(").Replace("）", ")").Replace("，", ",");
        //    string[] tem = data.Split(',');
        //    List<FL_Data> fL_s = new List<FL_Data>();
        //    for (int i = 0; i < tem.Length; i++)
        //    {
        //        string[] d = tem[i].Split('(');
        //        if (d.Length < 2) continue;
        //        // 00:30(谷)
        //        FL_Data t = new FL_Data
        //        {
        //            Time = d[0],
        //            Type = (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), d[1].Substring(0, d[1].Length - 1))
        //        };
        //        if (IsCF || fL_s.Count(item => item.Type == t.Type) == 0)  //费率重复的不添加
        //        {
        //            if (fL_s.Count(item => item.Time == t.Time) == 0)
        //            {
        //                fL_s.Add(t);
        //            }
        //        }
        //    }
        //    return fL_s;

        //}
        //#endregion

    }

}
