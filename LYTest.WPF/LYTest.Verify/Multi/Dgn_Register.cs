using LYTest.Core.Enum;
using LYTest.Core.Model;
using LYTest.DAL.Config;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using LYTest.Core.Function;

namespace LYTest.Verify.Multi
{
    // add lsj 20220218 电能示值组合误差试验
    //modify yjt 20220303 所有的"实验"替换为"试验"
    //modify yjt 20220303 设置-结论配置修改“试验前总电量|试验后总电量|总电量差值|组合误差”
    //为“试验前总电量|试验后总电量|总电量差值|试验前费率电量|试验后费率电量|费率电量差值|组合误差”

    /// <summary>
    //电能示值组合误差试验
    /// </summary>
    public class Dgn_Register : VerifyBase
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
        /// 运行时间
        /// </summary>
        int RunTime = 0;

        /// <summary>
        /// 运行电量
        /// </summary>
        float RunkWh = 0;

        /// <summary>
        /// 走字的电流
        /// </summary>
        float Xib = 0;

        /// <summary>
        /// 是否24小时走字
        /// </summary>
        bool Is24ZZ = false;

        /// <summary>
        /// 功率方向
        /// </summary>
        PowerWay powerWay = PowerWay.正向有功;
        /// <summary>
        /// 误差限，规程的，或方案配置的，或下载的
        /// </summary>
        double ErrLmt = -1;

        public override void Verify()
        {
            base.Verify();
            //add yjt 20220305 新增日志提示
            MessageAdd("电能示值组合误差试验检定开始...", EnumLogType.流程信息);
            if (Stop) return;
            if (!IsDemo)
            {
                if (Stop) return;
                if (!CheckVoltage())
                {
                    return;
                }
                if (Stop) return;
                ReadMeterAddrAndNo();
                //add
                if (Stop) return;
                Identity();
                if (Stop) return;
                //判断是否是24小时走字
                if (!Is24ZZ)
                {
                    MessageAdd("开始读取实验前电量电量", EnumLogType.提示信息);
                    if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                    string[] dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(powerWay));

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                        if (dicEnergy[i].IndexOf(',') != -1)
                        {
                            ResultDictionary["试验前总电量"][i] = dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(','));
                            ResultDictionary["试验前费率电量"][i] = dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1);
                        }
                        else
                        {
                            ResultDictionary["试验前总电量"][i] = dicEnergy[i];
                            ResultDictionary["试验前费率电量"][i] = dicEnergy[i];
                        }

                    }
                    RefUIData("试验前总电量");
                    RefUIData("试验前费率电量");

                    if (IsReadFLTime)
                    {
                        Dictionary<string, string> tempDate = new Dictionary<string, string>();
                        string[] readtzs = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据");

                        for (int i = 0; i < readtzs.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(readtzs[i]))
                            {
                                string temp = readtzs[i];
                                int x = 4;
                                while (x + 2 <= temp.Length)
                                {

                                    tempDate.Add(temp.Substring(x - 4, 2) + temp.Substring(x - 2, 2), temp.Substring(x + 1, 1));
                                    x += 6;
                                }
                                break;
                            }

                        }
                        string noN = "1";//第n日时段
                        List<string> keyList = tempDate.Keys.ToList();
                        keyList.Sort();
                        string time = DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                        for (int i = 0; i < keyList.Count; i++)
                        {
                            if (int.Parse(keyList[i]) < int.Parse(time) && int.Parse(time) < int.Parse(keyList[i + 1]))
                            {
                                noN = tempDate[keyList[i]];
                            }
                        }


                        //string TimeZoneOne="";
                        //string[] readtzs = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据");
                        //for (int i = 0; i < readtzs.Length; i++)
                        //{
                        //    if (!string.IsNullOrWhiteSpace(readtzs[i]))
                        //    {
                        //         TimeZoneOne = readtzs[i];
                        //    }
                        //    break;
                        //}
                        //int UseDayBase = Common.GetPeriodNN(TimeZoneOne, (byte)DateTime.Now.Month, (byte)DateTime.Now.Day);
                        MessageAdd("正在读取表内费率时段", EnumLogType.提示信息);
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                        string[] readdata = MeterProtocolAdapter.Instance.ReadData($"第一套第{noN}日时段数据");
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
                        //string[] readdata = MeterProtocolAdapter.Instance.ReadData(GetPeriodName());
                        for (int i = 0; i < readdata.Length; i++)
                        {
                            if (readdata[i] != null)
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
                                FL_List = GetFlList(fl, false);
                                break;
                            }
                        }
                    }

                    //循环所有费率-开始走字
                    for (int i = 0; i < FL_List.Count; i++)
                    {
                        if (Stop) return;
                        Identity(false); //身份认证
                        //设置时间
                        if (Stop) return;
                        DateTime time = DateTime.Parse(FL_List[i].Time + ":10");
                        MessageAdd("修改电表时间到" + time.ToString("yyyy年MM月dd日 HH时mm分ss秒"), EnumLogType.提示与流程信息);
                        MeterProtocolAdapter.Instance.WriteDateTime(time);

                        if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1") == false)
                        {
                            MessageAdd("控制源输出失败", EnumLogType.提示信息);
                            return;
                        }
                        WaitTime(string.Format($"正在进行费率{FL_List[i].Type}走字"), RunTime);//倒计时
                        if (Stop) return;

                        if (PowerOn() == false)
                        {
                            MessageAdd("控制源输出失败", EnumLogType.提示信息);
                            return;
                        }
                        WaitTime("关闭电流", 5);

                    }

                    //读取结束电量
                    MessageAdd("正在读取实验后电量", EnumLogType.提示信息);
                    if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                    dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(powerWay));
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                        if (dicEnergy[i].IndexOf(',') >= 0)
                        {
                            ResultDictionary["试验后总电量"][i] = dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(','));
                            ResultDictionary["试验后费率电量"][i] = dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1);
                        }
                        else
                        {
                            ResultDictionary["试验后总电量"][i] = dicEnergy[i];
                            ResultDictionary["试验后费率电量"][i] = dicEnergy[i];
                        }
                    }
                    RefUIData("试验后总电量");
                    RefUIData("试验后费率电量");


                    GetErrValue();
                }
                else
                {
                    Verify24H();
                }
                //恢复时间
                if (Stop) return;
                //身份认证
                Identity(false);
                MessageAdd("正在恢复电能表时间...", EnumLogType.提示信息);
                if (Stop) return;
                //恢复电表时间，写入当前时间
                MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
            }
            else //演示模式
            {
                int RateCount = 4;
                for (int i = 0; i < RateCount; i++)
                {
                    FL_List.Add(new FL_Data());
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    double start = 0;

                    for (int j = 0; j < RateCount; j++)
                    {
                        double stratRate = new Random().NextDouble() * 2;
                        start += stratRate;
                        ResultDictionary["试验前费率电量"][i] += stratRate.ToString("F3") + ",";
                        ResultDictionary["试验后费率电量"][i] += (stratRate + 0.01).ToString("F3") + ",";
                        System.Threading.Thread.Sleep(10);
                    }
                    ResultDictionary["试验前总电量"][i] = start.ToString("F3");
                    ResultDictionary["试验后总电量"][i] = (start + 0.01 * RateCount).ToString("F3");
                    ResultDictionary["试验前费率电量"][i] = ResultDictionary["试验前费率电量"][i].TrimEnd(',');
                    ResultDictionary["试验后费率电量"][i] = ResultDictionary["试验后费率电量"][i].TrimEnd(',');
                    System.Threading.Thread.Sleep(10);

                }
                RefUIData("试验前总电量");
                RefUIData("试验前费率电量");
                RefUIData("试验后总电量");
                RefUIData("试验后费率电量");
                GetErrValue();
            }
            MessageAdd("电能示值组合误差试验检定结束...", EnumLogType.提示信息);
        }
        /// <summary>
        /// 走字24小时
        /// </summary>
        private void Verify24H()
        {
            MessageAdd("开始读取实验前电量电量", EnumLogType.提示信息);
            string[] dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(powerWay));
            RunTime = 24 * 60 * 60;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                ResultDictionary["试验前总电量"][i] = dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(','));
                ResultDictionary["试验前费率电量"][i] = dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1);
            }
            RefUIData("试验前总电量");
            RefUIData("试验前费率电量");

            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return;
            }

            WaitTime(string.Format($"正在进行24小时走字"), RunTime);//倒计时

            if (Stop) return;
            if (PowerOn() == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return;
            }
            WaitTime("关闭电流", 5);

            //读取结束电量
            MessageAdd("正在读取实验后电量", EnumLogType.提示信息);
            dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(powerWay));
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                ResultDictionary["试验后总电量"][i] = dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(','));
                ResultDictionary["试验后费率电量"][i] = dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1);
            }
            RefUIData("试验后总电量");
            RefUIData("试验后费率电量");
            GetErrValue();
        }

        /// <summary>
        /// 计算差值
        /// </summary>
        private void GetErrValue()
        {
            MessageAdd("正在计算误差", EnumLogType.提示信息);
            //开始计算差值
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                {
                    //判断是否没读取到电量等情况
                    if (Core.Function.Number.IsNumeric(ResultDictionary["试验后总电量"][i]) && Core.Function.Number.IsNumeric(ResultDictionary["试验前总电量"][i]))
                    {
                        if (ConfigHelper.Instance.Dgn_UseHighPrecision)
                        {
                            ResultDictionary["总电量差值"][i] = (float.Parse(ResultDictionary["试验后总电量"][i]) - float.Parse(ResultDictionary["试验前总电量"][i])).ToString("F4");
                        }
                        else
                        {
                            ResultDictionary["总电量差值"][i] = (float.Parse(ResultDictionary["试验后总电量"][i]) - float.Parse(ResultDictionary["试验前总电量"][i])).ToString("F2");
                        }

                    }
                    else
                    {
                        ResultDictionary["总电量差值"][i] = "999";
                    }

                    //判断分费率差值
                    if (ResultDictionary["试验后费率电量"][i] == null || ResultDictionary["试验后费率电量"][i] == "" || ResultDictionary["试验前费率电量"][i] == null || ResultDictionary["试验前费率电量"][i] == "")
                    {
                        ResultDictionary["组合误差"][i] = "999";
                        ResultDictionary["费率电量差值"][i] = "999";
                        continue;
                    }
                    string[] tem = ResultDictionary["试验后费率电量"][i].Split(',');
                    string[] tem2 = ResultDictionary["试验前费率电量"][i].Split(',');


                    if (tem.Length != tem2.Length)
                    {
                        ResultDictionary["组合误差"][i] = "999";
                        ResultDictionary["费率电量差值"][i] = "999";
                        continue;
                    }
                    float Zerr = 0;  //总费率的差值
                    string[] err = new string[tem.Length];
                    //循环所有费率-计算差值
                    for (int j = 0; j < tem.Length; j++)
                    {
                        float t = float.Parse(tem[j]) - float.Parse(tem2[j]);

                        err[j] = (ConfigHelper.Instance.Dgn_UseHighPrecision) ? t.ToString("F4") : t.ToString("F2");
                        Zerr += t;

                    }

                    ResultDictionary["费率电量差值"][i] += string.Join(",", err);

                    ResultDictionary["组合误差"][i] = (ConfigHelper.Instance.Dgn_UseHighPrecision) ? (float.Parse(ResultDictionary["总电量差值"][i]) - Zerr).ToString("F4") : (float.Parse(ResultDictionary["总电量差值"][i]) - Zerr).ToString("F2");
                }
                RefUIData("总电量差值");
                RefUIData("费率电量差值");
                RefUIData("组合误差");
            }
            if (ErrLmt == -1)
            {
                ErrLmt = 0.01 * FL_List.Select(x => x.Type).Distinct().Count();
            }
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                float fErr = float.Parse(ResultDictionary["组合误差"][i]);
                if (Math.Abs(fErr) > ErrLmt)
                    ResultDictionary["结论"][i] = "不合格";
                else
                    ResultDictionary["结论"][i] = "合格";
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] data = Test_Value.Split('|');
            if (data.Length < 5) return false;
            FL_List.Clear();
            if (data[4] == "是")     // 00:30(谷),08:30(平),10:00(峰),13:00(平),19:30(峰)
            {
                IsReadFLTime = true;
            }
            else
            {
                FL_List = GetFlList(data[0], true);
            }
            //判断时间是否为空或者不是数字的情况。给个默认值1
            if (string.IsNullOrWhiteSpace(data[1]) || !Core.Function.Number.IsNumeric(data[1]))
            {
                RunTime = 60;//运行60秒
            }
            else
            {
                RunTime = (int)(float.Parse(data[1]) * 60);
            }
            Xib = Core.Function.Number.GetCurrentByIb(data[2], OneMeterInfo.MD_UA, HGQ);
            Is24ZZ = data[3] == "是";
            powerWay = GetPowerWay();//当前检定功率方向
            if (data.Length > 5)
            {
                if (!string.IsNullOrWhiteSpace(data[5]) && Core.Function.Number.IsNumeric(data[5]))
                {
                    RunkWh = float.Parse(data[5]);
                    RunTime = (int)(RunkWh * 1000 * 3600 / CalculatePower(U, Xib, Clfs, Cus_PowerYuanJian.H, "1.0", true) - 10);
                    if (RunTime < 10) RunTime = 10;
                }
                else
                {
                    RunTime = 60;
                }
            }
            if (data.Length > 6)
            {
                if (double.TryParse(data[6], out double temp))
                {
                    ErrLmt = temp;
                }
            }
            ResultNames = new string[] { "试验前总电量", "试验后总电量", "总电量差值", "试验前费率电量", "试验后费率电量", "费率电量差值", "组合误差", "结论" }; ;
            return true;

        }



        #region 用到的方法
        private string GetProtocalName(PowerWay p)
        {
            if (ConfigHelper.Instance.Dgn_UseHighPrecision)
            {
                switch (p)
                {
                    case PowerWay.组合有功:
                        return "(当前)正向有功电能数据块高精度";
                    case PowerWay.反向有功:
                        return "(当前)反向有功电能数据块高精度";
                    case PowerWay.正向无功:
                        return "(当前)组合无功1电能数据块高精度";
                    case PowerWay.反向无功:
                        return "(当前)组合无功2电能数据块高精度";
                    case PowerWay.第一象限无功:
                        return "(当前)第一象限无功总电能";
                    case PowerWay.第二象限无功:
                        return "(当前)第二象限无功总电能";
                    case PowerWay.第三象限无功:
                        return "(当前)第三象限无功总电能";
                    case PowerWay.第四象限无功:
                        return "(当前)第四象限无功总电能";
                    default:
                        return "(当前)组合有功电能数据块高精度";
                }
            }
            else
            {

                switch (p)
                {
                    case PowerWay.组合有功:
                        return "(当前)组合有功电能数据块";
                    case PowerWay.正向有功:
                        return "(当前)正向有功电能数据块";
                    case PowerWay.反向有功:
                        return "(当前)反向有功电能数据块";
                    case PowerWay.正向无功:
                        return "(当前)组合无功1电能数据块";
                    case PowerWay.反向无功:
                        return "(当前)组合无功2电能数据块";
                    case PowerWay.第一象限无功:
                        return "(当前)第一象限无功总电能";
                    case PowerWay.第二象限无功:
                        return "(当前)第二象限无功总电能";
                    case PowerWay.第三象限无功:
                        return "(当前)第三象限无功总电能";
                    case PowerWay.第四象限无功:
                        return "(当前)第四象限无功总电能";
                    default:
                        return "(当前)组合有功总电能";
                }
            }
        }

        //当前检定功率方向 默认 正向有功
        private PowerWay GetPowerWay()
        {
            PowerWay tmp = PowerWay.正向有功;
            //string aa = Test_Value;
            //string[] bb = aa.Split('|');
            //if (bb.AsQueryable().Last() == "反向有功")
            //    tmp = PowerWay.反向有功;
            //else if (bb.AsQueryable().Last() == "正向无功")
            //    tmp = PowerWay.正向无功;
            //else if (bb.AsQueryable().Last() == "反向无功")
            //    tmp = PowerWay.反向无功;
            return tmp;
        }
        ///// <summary>
        ///// 解析费率时间
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //private List<FL_Data> GetFlList(string data, bool IsCF)
        //{
        //    //以防输错情况，把中文字符转成英文字符
        //    data = data.Replace("（", "(").Replace("）", ")").Replace("，", ",").Replace("：", ":");
        //    data = data.Replace("):", "),");
        //    string[] tem = data.Split(',');
        //    List<FL_Data> fL_s = new List<FL_Data>();
        //    for (int i = 0; i < tem.Length; i++)
        //    {
        //        if (string.IsNullOrWhiteSpace(tem[i])) continue;
        //        string[] d = tem[i].Split('(');
        //        if (d.Length < 2) continue;
        //        // 00:30(谷)
        //        FL_Data t = new FL_Data();
        //        t.Time = d[0];
        //        t.Type = (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), d[1].Substring(0, d[1].Length - 1));
        //        if (IsCF || fL_s.Count(item => item.Type == t.Type) == 0)  //费率重复的不添加
        //        {
        //            fL_s.Add(t);
        //        }
        //    }
        //    return fL_s;

        //}
        #endregion

        ///// <summary>
        ///// 费率类
        ///// </summary>
        //private class FL_Data
        //{
        //    /// <summary>
        //    /// 费率类型
        //    /// </summary>
        //    public Cus_FeiLv Type;
        //    /// <summary>
        //    /// 费率时间 HH:mm
        //    /// </summary>
        //    public string Time;
        //    /// <summary>
        //    ///  时区 MM-dd
        //    /// </summary>
        //    public string Date;
        //}

        //add
        //#region 电子指示显示器电能示值组合误差结构体
        //struct RegisterStruct
        //{
        //    public int BW;              //表位号            
        //    public string FL;           //费率
        //    public float PowerStart;       //开始电量
        //    public float PowerEnd;         //结束电量                        
        //    public override string ToString()
        //    {
        //        return string.Format("{0}|{1}|{2}|{3}", PowerStart, PowerEnd, Error(), FL);
        //    }

        //    /// <summary>
        //    /// 当前费率电量差值
        //    /// </summary>
        //    /// <returns></returns>
        //    public string Error()
        //    {
        //        return (PowerEnd - PowerStart).ToString("0.00");
        //    }
        //}
        //#endregion
    }


}
