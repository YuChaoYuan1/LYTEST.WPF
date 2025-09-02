using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LYTest.Verify.AccurateTest
{
    public class Register48h : VerifyBase
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

        string[] arrData = new string[0];    //数据数组
        //add
        StPlan_ZouZi curPoint = new StPlan_ZouZi();
        private readonly byte mc = 0x06;//脉冲计数
        private readonly bool bluetoothPulses = true;//计不计算蓝牙脉冲

        //标准表累积电量
        private float _StarandMeterDl = 0F;
        int _MaxTestTime = 0;

        private readonly float[] _HalfwayMeterPulse = new float[MeterInfo.Length];
        private readonly string HalfwayCacheFile = "HalfwayDataCache.temp";
        private readonly float _StarandMeterDlm = 0F;

        /// <summary>
        /// 日志时间显示分钟
        /// </summary>
        public float NowMinute = 0;

        /// <summary>
        /// 开始时间，只有在CreateFA后才生效
        /// </summary>
        public new string StartTime;
        /// <summary>
        /// 月日字符串
        /// </summary>
        private string tempDate = "";
        /// <summary>
        /// 电流大小
        /// </summary>
        public string XibSize = "大电流";

        /// <summary>
        /// 电流大小判断
        /// </summary>
        public string IsXibSize = "小电流";

        bool IsCutAngle = false; //是否切换角度
        bool IsCut = true; //是否进入切换

        public string _TotalSeconds = "";
        public double _PastTime2 = 0;
        public float _PastTime_ = 0f;

        public override void Verify()
        {
            base.Verify();

            MessageAdd("走字48h试验开始...", EnumLogType.流程信息);

            //StPlan_ZouZi _curPoint = (StPlan_ZouZi)curPoint;

            int curstep = 0;
            //int resetITO = 0;

            try
            {
                if (Stop) return;
                if (!IsDemo)
                {
                    if (Stop) return;
                    MessageAdd("正在升源...", EnumLogType.提示信息);
                    if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                    {
                        MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                        return;
                    }
                    if (Stop) return;
                    ReadMeterAddrAndNo();

                    if (arrData.Length < MeterNumber)
                    {
                        arrData = new string[MeterNumber];
                    }

                    //判断是否是24小时走字
                    if (!Is24ZZ)
                    {
                        if (Stop) return;
                        Identity();
                        if (Stop) return;

                        #region // 第二步，读取起始电量
                        MessageAdd("开始读取实验前电量电量", EnumLogType.提示信息);
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                        string[] dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(powerWay));

                        Dictionary<int, float[]> dicEnergy1 = null;

                        if (VerifyBase.MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                        {
                            dicEnergy1 = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)curPoint.PowerFangXiang);
                        }

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

                            if (VerifyBase.MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                            {
                                if (dicEnergy1[i] != null)
                                {
                                    ResultDictionary["试验前总电量"][i] = dicEnergy1[i][0].ToString("f4");
                                    ResultDictionary["试验前费率电量"][i] = dicEnergy1[i][1].ToString("f4") + "," + dicEnergy1[i][2].ToString("f4") + "," + dicEnergy1[i][3].ToString("f4") + "," + dicEnergy1[i][4].ToString("f4");
                                }
                            }
                        }
                        RefUIData("试验前总电量");
                        RefUIData("试验前费率电量");

                        #endregion

                        #region //第三步 如果不是总费率，将电能表的时间改成要测试的费率的对应时间
                        if (IsReadFLTime)
                        {
                            MessageAdd("正在读取表内费率时段", EnumLogType.提示信息);
                            List<FL_Data> lst_fld = new List<FL_Data>();
                            string[] readtzs = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据");
                            bool findtz = false;
                            for (int i = 0; i < readtzs.Length; i++)
                            {
                                if (!string.IsNullOrWhiteSpace(readtzs[i]))
                                {
                                    string temp = readtzs[i];
                                    int x = 4;
                                    while (x + 2 <= temp.Length)
                                    {
                                        int day = int.Parse(temp.Substring(x, 2));
                                        tempDate = $"{temp.Substring(x - 4, 2)}-{temp.Substring(x - 2, 2)}";
                                        findtz = true;
                                        if (!lst_fld.Exists(item => { return item.Day == day; }))
                                            lst_fld.Add(new FL_Data() { Date = tempDate, Day = day });
                                        x += 6;
                                    }
                                    break;
                                }
                                if (findtz) break;
                            }
                            if (Stop) return;

                            bool findall = false;

                            foreach(FL_Data tz in lst_fld)
                            {
                                if (Stop) return;
                                if (DAL.Config.ConfigHelper.Instance.IsITOMeter) 
                                    VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;

                                string[] readDatas = MeterProtocolAdapter.Instance.ReadData($"第一套第{tz.Day}日时段数据");
                                if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
                                foreach(string temp in readDatas)
                                {
                                    if (string.IsNullOrWhiteSpace(temp)) continue;

                                    int x = 4;
                                    while (x + 2 <= temp.Length)
                                    {
                                        FL_Data fld = new FL_Data
                                        {
                                            Time = $"{temp.Substring(x - 4, 2)}:{temp.Substring(x - 2, 2)}",
                                            Type = (Cus_FeiLv)int.Parse(temp.Substring(x, 2)),
                                            Date = tz.Date,
                                            Day = tz.Day
                                        };
                                        if (!FL_List.Exists(item => { return item.Type == fld.Type; }))
                                        {
                                            FL_List.Add(fld);
                                        }
                                        else if (fld.Type == Cus_FeiLv.峰 && FL_List.Count(item => { return item.Type == Cus_FeiLv.峰; }) < 2) //峰时段2个
                                        {
                                            FL_List.Add(fld);
                                        }
                                        curPoint.StartTime = fld.Time;
                                        if (FL_List.Count >= 5)
                                        {
                                            findall = true;
                                            break;
                                        }
                                        x += 6;
                                    }

                                    break;
                                }
                                if (findall) break;
                            }

                            if (Stop) return;
                            if (string.IsNullOrWhiteSpace(curPoint.StartTime))
                            {
                                string msg = $"当前使用的费率时段没有【{curPoint.FeiLv}】时段\r\n";
                                MessageAdd(msg, EnumLogType.错误信息);
                                return;
                            }
                        }
                        #endregion

                        #region //第四步，发送走字指令，开始走字
                        // 2:(启动标准表--重置电量)
                        MessageAdd("启动标准表--重置电量", EnumLogType.提示信息);
                        StartStdEnergy(31);
                        if (bluetoothPulses)
                        {
                            SetBluetoothModule(mc);
                        }
                        MessageAdd("启动误差板脉冲计数", EnumLogType.提示信息);
                        StartWcb(mc, 0xff); //启动误差板脉冲计数
                        #endregion

                        string[] FLQian = new string[MeterNumber]; //费率前电量
                        string[] FLHou = new string[MeterNumber]; //费率后电量
                        string[] FLErr = new string[MeterNumber]; //费率差

                        int pkCount = 0;
                        //获取走字的电流
                        float testI = Number.GetCurrentByIb(curPoint.xIb, OneMeterInfo.MD_UA, HGQ);
                        double maxSeconds01 = CalcSecondsOfkWh(0.1f, testI, curPoint.PowerYj, curPoint.Glys);
                        //循环所有费率-开始走字
                        for (int k = 0; k < FL_List.Count; k++)
                        {
                            if (Stop) return;
                            Identity(false);

                            CheckOver = false;
                            if (FL_List[k].Type == Cus_FeiLv.峰) ++pkCount;
                            string time = FL_List[k].Time;
                            DateTime dt = DateTime.Now;
                            int hh = int.Parse(time.Substring(0, 2));
                            int mm = int.Parse(time.Substring(3, 2));
                            int MM = int.Parse(FL_List[k].Date.Substring(0, 2));
                            int dd = int.Parse(FL_List[k].Date.Substring(3, 2));
                            dt = new DateTime(dt.Year, MM, dd, hh, mm, 0);
                            bool[] t = MeterProtocolAdapter.Instance.WriteDateTime(dt);

                            //DateTime times = DateTime.Parse(FL_List[k].Time + ":10");
                            MessageAdd($"修改电表时间到{dt:yyyy年MM月dd日 HH时mm分ss秒},开始做第{k + 1}个时段走字,共{FL_List.Count}个时段", EnumLogType.提示与流程信息);


                            #region //第五步，升走字电流
                            if (Stop) return;
                            MessageAdd("正在升电流...", EnumLogType.提示信息);


                            int _MeConst = Number.GetBcs(OneMeterInfo.MD_Constant, curPoint.PowerFangXiang);
                            if (_MeConst < 1000) _MeConst = 1000;

                            DAL.Config.ConfigHelper.Instance.AutoGearTemp = false;

                            StdGear(0x13, Convert.ToUInt64(_MeConst), OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI);

                            VerifyConfig.Test_ZouZi_Control = true;

                            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                            {
                                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                                return;
                            }

                            //add yjt 20220520 新增等待时间的获取
                            int timeWaitSource = DAL.Config.ConfigHelper.Instance.Dgn_PowerSourceStableTime;
                            //if (IsDemo)
                            //{
                            //    timeWaitSource = 1;
                            //}

                            if (Stop) return;
                            #endregion

                            #region //第六步，控制 执行步骤
                            string stroutmessage = string.Empty;
                            double deduct = CalckWhOfSeconds(2, testI, curPoint.PowerYj, curPoint.Glys);
                            double maxSeconds = _MaxTestTime * 1.2f;//CalcSecondsOfkWh(_curPoint.UseMinutes, testI, curPoint.PowerYj, curPoint.Glys) * 1.2;
                            DateTime startTime = DateTime.Now.AddSeconds(-timeWaitSource);   //检定开始时间,减掉源等待时间
                            _StarandMeterDl = 0;                        //标准表电量
                            DateTime lastTime = DateTime.Now.AddSeconds(-timeWaitSource - 1);

                            while (true)
                            {
                                Thread.Sleep(1000);
                                if (Stop) return;
                                float pastTime = (float)DateTimes.DateDiff(startTime);

                                if (DateTime.Now.Subtract(startTime).TotalSeconds > maxSeconds)//超出最大处理时间
                                {
                                    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示与流程信息);
                                    break;
                                }

                                #region
                                //if (!IsDemo)
                                //{
                                //记录标准表电量
                                float pSum = Math.Abs(IsYouGong ? EquipmentData.StdInfo.P : EquipmentData.StdInfo.Q);

                                float pastSecond = (float)(DateTime.Now - lastTime).TotalMilliseconds;
                                lastTime = DateTime.Now;
                                _StarandMeterDl += pastSecond * pSum / 3600 / 1000 / 1000;
                                if (arrData.Length < MeterNumber)
                                {
                                    arrData = new string[MeterNumber];
                                }
                                StError[] errors = ReadWcbData(GetYaoJian(), mc);
                                for (int j = 0; j < errors.Length; j++)
                                {
                                    if (errors[j] == null) continue;
                                    arrData[j] = errors[j].szError;

                                }

                                pastSecond = (float)(DateTime.Now - lastTime).TotalMilliseconds;
                                lastTime = DateTime.Now;
                                _StarandMeterDl += pastSecond * pSum / 3600 / 1000 / 1000;
                                //同步记录（读）脉冲数


                                int xb = 9;
                                if (!IsYouGong)
                                    xb = 10;

                                try
                                {
                                    string[] bzbdlold = ReadEnrgyZh311();
                                    if (bzbdlold != null && bzbdlold.Length > xb && bzbdlold[xb] != null)
                                    {
                                        _StarandMeterDl = float.Parse((float.Parse(bzbdlold[xb]) / 3600 / 1000).ToString("0.0000"));
                                    }
                                }
                                catch
                                {
                                    //读标准标电量 kWh
                                    float[] bzbdl = ReadStmEnergy();

                                    if (bzbdl != null && bzbdl.Length > xb && bzbdl[xb] != 0)
                                    {
                                        _StarandMeterDl = bzbdl[xb];
                                    }
                                }

                                if (_StarandMeterDl < 0)
                                {
                                    _StarandMeterDl = Math.Abs(_StarandMeterDl);
                                }
                                //}
                                //else
                                //{
                                //    //模拟电量
                                //    _StarandMeterDl = pastTime * OneMeterInfo.MD_UB * testI / 3600000F;
                                //}

                                // add yjt 20220327 新增演示模式直接合格
                                //if (IsDemo)
                                //{
                                //    CheckOver = true;
                                //}

                                if (Test_Value.Split('|')[5] == "定量法")
                                {
                                    //如果电量达到设定，停止
                                    if (_StarandMeterDl + _StarandMeterDlm >= curPoint.UseMinutes * (k + 1) - deduct)
                                    {
                                        CheckOver = true;
                                    }

                                    //如果脉冲数达到设定，也停止
                                    float flt_C = 0;
                                    if (arrData != null && arrData.Length > 0)
                                    {
                                        float.TryParse(arrData[FirstIndex], out flt_C);
                                    }
                                    flt_C = (flt_C + _HalfwayMeterPulse[FirstIndex]) / OneMeterInfo.GetBcs()[0];
                                    if (flt_C >= curPoint.UseMinutes * (k + 1) - deduct)
                                    {
                                        CheckOver = true;
                                    }

                                    if (FL_List[k].Type == Cus_FeiLv.峰 && pkCount >= 2 && TimeSubms(DateTime.Now, startTime) >= maxSeconds01 * 1000)
                                    {
                                        CheckOver = true;
                                    }

                                    stroutmessage = string.Format("走字电量：{0}度，已经走字：{1}度,当前为第{2}个时段，每个时段{3}度", curPoint.UseMinutes * (k + 1), (_StarandMeterDl + _StarandMeterDlm).ToString("F5"), (k + 1), curPoint.UseMinutes);
                                }
                                else if (Test_Value.Split('|')[5] == "定时法")
                                {
                                    if (pastTime + _PastTime_ >= _MaxTestTime)
                                    {
                                        CheckOver = true;
                                    }
                                    NowMinute = (float)(pastTime + _PastTime_) / 60F;
                                    stroutmessage = $"走字时间：{curPoint.UseMinutes}分，已经走字：{Math.Round(NowMinute, 2)}分,当前为第{k + 1}个时段，每个时段{curPoint.UseMinutes}分";
                                }

                                #endregion

                                MessageAdd(stroutmessage, EnumLogType.提示信息);
                                if (CheckOver) break;

                            }
                            #endregion
                            curstep = 6;

                            #region //第七步,升压,不升电流
                            MessageAdd("正在升源...", EnumLogType.提示信息);
                            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                            {
                                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                                return;
                            }
                            if (Stop) return;
                            #endregion

                        }

                        //读取结束电量
                        {
                            MessageAdd("正在读取实验后电量", EnumLogType.提示信息);
                            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                            dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(powerWay));

                            if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                            {
                                dicEnergy1 = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)curPoint.PowerFangXiang);
                            }

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

                                if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                                {
                                    if (dicEnergy1[i] != null)
                                    {
                                        ResultDictionary["试验后总电量"][i] = dicEnergy1[i][0].ToString("f4");
                                        ResultDictionary["试验后费率电量"][i] = dicEnergy1[i][1].ToString("f4") + "," + dicEnergy1[i][2].ToString("f4") + "," + dicEnergy1[i][3].ToString("f4") + "," + dicEnergy1[i][4].ToString("f4");
                                    }
                                }
                            }
                            RefUIData("试验后总电量");
                            RefUIData("试验后费率电量");
                        }

                        GetErrValue();
                        curstep = 12;
                        //恢复时间
                        if (Stop) return;
                        //身份认证
                        Identity(false);
                        MessageAdd("正在恢复电能表时间...", EnumLogType.提示信息);
                        if (Stop) return;
                        //恢复电表时间，写入当前时间
                        MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                    }
                    else
                    {
                        Verify24H(ref curstep);
                    }

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
            }
            finally
            {
                VerifyConfig.Test_ZouZi_Control = false;
                DAL.Config.ConfigHelper.Instance.AutoGearTemp = true;
                StdGear(0x13, 0, 0, 0, 0, 0, 0, 0);
                if (VerifyConfig.Test_ZouZi_HalfwayMode && !IsDemo)
                {
                    if (curstep < 12)
                    {
                        #region //第七步,升压,不升电流
                        MessageAdd("正在升源...", EnumLogType.提示信息);
                        if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                        {

                        }
                        #endregion

                        #region //第八步 读终止时的电量或脉冲数


                        string[] dicEnergy = null;
                        string[] zong2 = new string[MeterNumber];
                        string[] feng2 = new string[MeterNumber];
                        string[] zong4 = new string[MeterNumber];
                        string[] feng4 = new string[MeterNumber];

                        Dictionary<int, float[]> dicEnergy1 = null;
                        PowerWay tmps = PowerWay.正向有功;

                        MessageAdd("开始读取实验后电量电量", EnumLogType.提示信息);
                        for (int j = 1; j <= 4; j++)
                        {
                            if (j == 1)
                                tmps = PowerWay.正向有功;
                            else if (j == 2)
                                tmps = PowerWay.反向有功;
                            else if (j == 3)
                                tmps = PowerWay.正向无功;
                            else if (j == 4)
                                tmps = PowerWay.反向无功;

                            dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(tmps));

                            if (VerifyBase.MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                            {
                                dicEnergy1 = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)tmps);
                            }

                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                                zong2[i] = zong2[i] + dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(',')) + "_";
                                feng2[i] = feng2[i] + dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1) + "_";

                                if (VerifyBase.MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                                {
                                    zong4[i] = zong4[i] + dicEnergy1[i][0].ToString("f4") + "_";
                                    feng4[i] = feng4[i] + dicEnergy1[i][1].ToString("f4") + "," + dicEnergy1[i][2].ToString("f4") + "," + dicEnergy1[i][3].ToString("f4") + "," + dicEnergy1[i][4].ToString("f4") + "_";
                                }
                            }
                        }

                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                            if (dicEnergy[i].IndexOf(',') != -1)
                            {
                                ResultDictionary["试验后总电量"][i] = zong2[i].TrimEnd('_');
                                ResultDictionary["试验后费率电量"][i] = feng2[i].TrimEnd('_');
                            }

                            if (VerifyBase.MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                            {
                                if (dicEnergy1[i] != null)
                                {
                                    ResultDictionary["试验后总电量"][i] = zong4[i].TrimEnd('_');
                                    ResultDictionary["试验后费率电量"][i] = feng4[i].TrimEnd('_');
                                }
                            }
                        }
                        RefUIData("试验后总电量");
                        RefUIData("试验后费率电量");
                        #endregion

                        bool all = true;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn) continue;
                            if (ResultDictionary["结论"][i] != ConstHelper.合格)
                            {
                                all = false;
                                break;
                            }
                        }
                        if (!all && curstep < 12)
                        {
                            StringBuilder builder = new StringBuilder();
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                TestMeterInfo meter = MeterInfo[i];
                                if (!meter.YaoJianYn)
                                {
                                    builder.AppendLine("false");
                                }
                                else
                                {
                                    //要检,表地址,试验前总电量,试验前费率电量,已经走的时间,当前时间
                                    builder.Append("true~").Append(meter.MD_PostalAddress).Append("~").Append(ResultDictionary["试验前总电量"][i]).Append("~").Append(ResultDictionary["试验前费率电量"][i]).Append("~").Append(_StarandMeterDl + _StarandMeterDlm).Append("~").Append(arrData[FirstIndex]).Append("~").Append(_TotalSeconds).Append("~").Append(_TotalSeconds).Append("~").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                }
                            }
                            System.IO.File.WriteAllText(HalfwayCacheFile, builder.ToString());
                        }
                        else
                        {
                            if (System.IO.File.Exists(HalfwayCacheFile))
                                System.IO.File.Delete(HalfwayCacheFile);
                        }
                    }
                    else
                    {
                        if (System.IO.File.Exists(HalfwayCacheFile))
                            System.IO.File.Delete(HalfwayCacheFile);
                    }
                }
            }


            MessageAdd("走字48h试验结束...", EnumLogType.提示信息);
        }
        /// <summary>
        /// 走字24小时
        /// </summary>
        private void Verify24H(ref int curstep)
        {
            RunTime = 2 * 24 * 60 * 60;
            _MaxTestTime = 2 * 24 * 60 * 60;

            //Dictionary<int, float> dicAllPower = null;
            string[] dicEnergy = null;
            string[] zong2 = new string[MeterNumber];
            string[] feng2 = new string[MeterNumber];
            string[] zong4 = new string[MeterNumber];
            string[] feng4 = new string[MeterNumber];
            Dictionary<int, float[]> dicEnergy1 = null;
            PowerWay tmps = PowerWay.正向有功;

            MessageAdd("开始读取实验前电量电量", EnumLogType.提示信息);
            for (int j = 1; j <= 4; j++)
            {
                if (j == 1)
                    tmps = PowerWay.正向有功;
                else if (j == 2)
                    tmps = PowerWay.反向有功;
                else if (j == 3)
                    tmps = PowerWay.正向无功;
                else if (j == 4)
                    tmps = PowerWay.反向无功;

                dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(tmps));

                if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                {
                    dicEnergy1 = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)tmps);
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                    zong2[i] = zong2[i] + dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(',')) + "_";
                    feng2[i] = feng2[i] + dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1) + "_";

                    if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                    {
                        zong4[i] = zong4[i] + dicEnergy1[i][0].ToString("f4") + "_";
                        feng4[i] = feng4[i] + dicEnergy1[i][1].ToString("f4") + "," + dicEnergy1[i][2].ToString("f4") + "," + dicEnergy1[i][3].ToString("f4") + "," + dicEnergy1[i][4].ToString("f4") + "_";
                    }
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                if (dicEnergy[i].IndexOf(',') != -1)
                {
                    ResultDictionary["试验前总电量"][i] = zong2[i].TrimEnd('_');
                    ResultDictionary["试验前费率电量"][i] = feng2[i].TrimEnd('_');
                }

                if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                {
                    if (dicEnergy1[i] != null)
                    {
                        ResultDictionary["试验前总电量"][i] = zong4[i].TrimEnd('_');
                        ResultDictionary["试验前费率电量"][i] = feng4[i].TrimEnd('_');
                    }
                }
            }
            RefUIData("试验前总电量");
            RefUIData("试验前费率电量");


            #region ---判断中途走字
            if (VerifyConfig.Test_ZouZi_HalfwayMode)
            {
                if (System.IO.File.Exists(HalfwayCacheFile))
                {
                    string[] lines = System.IO.File.ReadAllLines(HalfwayCacheFile);
                    if (lines.Length >= MeterNumber)
                    {
                        bool consistent = true;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            string[] data = lines[i].Split('~');
                            if (!MeterInfo[i].YaoJianYn)
                            {
                                if (!"false".Equals(data[0]))
                                {
                                    consistent = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (data.Length >= 9)
                                {
                                    if (!MeterInfo[i].MD_PostalAddress.Equals(data[1]))
                                    {
                                        consistent = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (consistent)
                        {
                            if (DialogResult.Yes == MessageBox.Show("检测到走字未完中途停止，选择\r\n\r\n\t是：继续剩余走字\r\n\r\n\t否：重新走字\r\n", "选择提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.ServiceNotification))
                            {
                                for (int i = 0; i < MeterNumber; i++)
                                {
                                    string[] data = lines[i].Split('~');
                                    if (!MeterInfo[i].YaoJianYn) continue;
                                    ResultDictionary["试验前总电量"][i] = data[2];
                                    ResultDictionary["试验前费率电量"][i] = data[3];

                                    _PastTime2 = double.Parse(data[7]);
                                }
                                RefUIData("试验前总电量");
                                RefUIData("试验前费率电量");
                            }
                        }

                    }
                }
            }
            #endregion


            #region //第五步，升走字电流
            if (Stop) return;
            MessageAdd("正在升电流...", EnumLogType.提示信息);

            //获取走字的电流
            float testI = Number.GetCurrentByIb(curPoint.xIb, OneMeterInfo.MD_UA, HGQ);

            int _MeConst = Number.GetBcs(OneMeterInfo.MD_Constant, curPoint.PowerFangXiang);
            if (_MeConst < 1000) _MeConst = 1000;

            DAL.Config.ConfigHelper.Instance.AutoGearTemp = false;

            StdGear(0x13, Convert.ToUInt64(_MeConst), OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI);

            VerifyConfig.Test_ZouZi_Control = true;

            testI = float.Parse(Test_Value.Split('|')[8]);

            if (OneMeterInfo.MD_WiringMode == "三相三线")
            {
                PowerOnFree(OneMeterInfo.MD_UB, 0, OneMeterInfo.MD_UB, testI, 0, testI, 0, 0, 60, 330, 0, 30, 50);
            }
            else
            {
                PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI, 0, 240, 120, 330, 210, 90, 50);
            }
            WaitTime("升源成功，等待源稳定", 10);

            //add yjt 20220520 新增等待时间的获取
            int timeWaitSource = DAL.Config.ConfigHelper.Instance.Dgn_PowerSourceStableTime;
            if (IsDemo)
            {
                timeWaitSource = 1;
            }

            if (Stop) return;

            #endregion

            #region //第六步，控制 执行步骤
            DateTime startTime = DateTime.Now.AddSeconds(-timeWaitSource);   //检定开始时间,减掉源等待时间
            DateTime TwoTime = startTime.AddDays(1);

            float PAngle = float.Parse(Test_Value.Split('|')[10]);

            while (true)
            {
                DateTime TimeNew = DateTime.Now;

                DateTime MaxXibYime = DateTime.Today.AddHours(8).AddMinutes(40);
                DateTime MinXibYime = DateTime.Today.AddHours(17).AddMinutes(20);

                TimeSpan _PastTime1 = DateTime.Now - startTime;

                if (Test_Value.Split('|')[5] == "48h切换象限法" && (TimeNew > MaxXibYime && TimeNew < MinXibYime))
                {
                    XibSize = "大电流";
                    testI = float.Parse(Test_Value.Split('|')[8]); //48h切换电流白天
                }
                else
                {
                    XibSize = "小电流";
                    testI = float.Parse(Test_Value.Split('|')[9]); //48h切换电流晚上
                }

                float[] Angles;

                if (OneMeterInfo.MD_WiringMode == "三相三线")
                {
                    Angles = new float[2] { 0, 60 };
                }
                else
                {
                    Angles = new float[3] { 0, 240, 120 };
                }

                if (DateTime.Compare(TimeNew, TwoTime) > 0 && IsCut == true)
                {
                    PAngle = float.Parse(Test_Value.Split('|')[11]);
                    IsCutAngle = true;
                }

                if (_PastTime1.TotalSeconds + _PastTime2 >= _MaxTestTime * 0.5 && IsCut == true)
                {
                    PAngle = float.Parse(Test_Value.Split('|')[11]);
                    IsCutAngle = true;
                }

                for (int i = 0; i < Angles.Length; i++)
                {
                    Angles[i] = Angles[i] - PAngle;
                    if (Angles[i] <= 0)
                    {
                        Angles[i] = Angles[i] + 360;
                    }
                }

                if (Test_Value.Split('|')[5] == "48h切换象限法" && IsXibSize == XibSize || IsCutAngle == true)
                {
                    if (OneMeterInfo.MD_WiringMode == "三相三线")
                    {
                        PowerOnFree(OneMeterInfo.MD_UB, 0, OneMeterInfo.MD_UB, testI, 0, testI, 0, 0, 60, Angles[0], 0, Angles[1], 50);
                    }
                    else
                    {
                        PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI, 0, 240, 120, Angles[0], Angles[1], Angles[2], 50);
                    }

                    if (DateTime.Compare(TimeNew, TwoTime) > 0)
                    {
                        IsCutAngle = false;
                        IsCut = false;
                    }

                    if (_PastTime1.TotalSeconds + _PastTime2 >= _MaxTestTime * 0.5)
                    {
                        IsCutAngle = false;
                        IsCut = false;
                    }

                    WaitTime("升源成功，等待源稳定", 10);

                    if (XibSize == "大电流")
                    {
                        IsXibSize = "小电流";
                    }
                    else if (XibSize == "小电流")
                    {
                        IsXibSize = "大电流";
                    }
                }

                Thread.Sleep(1000);
                if (Stop) return;

                #region

                if (TimeSubms(DateTime.Now, startTime) > (_MaxTestTime + 120) * 1000)//超出最大处理时间
                {
                    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示与流程信息);
                    break;
                }

                if (IsDemo)
                {
                    CheckOver = true;
                }

                if (_PastTime1.TotalSeconds + _PastTime2 >= _MaxTestTime)
                {
                    CheckOver = true;
                }

                _TotalSeconds = (_PastTime1.TotalSeconds + _PastTime2).ToString("f0");
                #endregion

                MessageAdd($"方案设置走字时间：{_MaxTestTime}秒，已经走字：{_TotalSeconds}秒", EnumLogType.提示信息);
                if (CheckOver) break;
            }
            #endregion

            #region //第七步,升压,不升电流
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;
            #endregion


            zong2 = new string[MeterNumber];
            feng2 = new string[MeterNumber];
            zong4 = new string[MeterNumber];
            feng4 = new string[MeterNumber];
            //读取结束电量
            MessageAdd("正在读取实验后电量", EnumLogType.提示信息);
            for (int j = 1; j <= 4; j++)
            {
                if (j == 1)
                    tmps = PowerWay.正向有功;
                else if (j == 2)
                    tmps = PowerWay.反向有功;
                else if (j == 3)
                    tmps = PowerWay.正向无功;
                else if (j == 4)
                    tmps = PowerWay.反向无功;

                dicEnergy = MeterProtocolAdapter.Instance.ReadData(GetProtocalName(tmps));

                if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                {
                    dicEnergy1 = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)tmps);
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                    zong2[i] = zong2[i] + dicEnergy[i].Substring(0, dicEnergy[i].IndexOf(',')) + "_";
                    feng2[i] = feng2[i] + dicEnergy[i].Substring(dicEnergy[i].IndexOf(',') + 1) + "_";

                    if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                    {
                        zong4[i] = zong4[i] + dicEnergy1[i][0].ToString("f4") + "_";
                        feng4[i] = feng4[i] + dicEnergy1[i][1].ToString("f4") + "," + dicEnergy1[i][2].ToString("f4") + "," + dicEnergy1[i][3].ToString("f4") + "," + dicEnergy1[i][4].ToString("f4") + "_";
                    }
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn || dicEnergy[i] == null) continue;
                if (dicEnergy[i].IndexOf(',') != -1)
                {
                    ResultDictionary["试验后总电量"][i] = zong2[i].TrimEnd('_');
                    ResultDictionary["试验后费率电量"][i] = feng2[i].TrimEnd('_');
                }

                if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
                {
                    if (dicEnergy1[i] != null)
                    {
                        ResultDictionary["试验后总电量"][i] = zong4[i].TrimEnd('_');
                        ResultDictionary["试验后费率电量"][i] = feng4[i].TrimEnd('_');
                    }
                }
            }
            RefUIData("试验后总电量");
            RefUIData("试验后费率电量");
            GetErrValue2();

            curstep = 12;
        }

        /// <summary>
        /// 计算差值
        /// </summary>
        private void GetErrValue()
        {
            MessageAdd("正在计算误差", EnumLogType.提示信息);

            int xsws = 2;
            if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
            {
                xsws = 4;
            }
            //开始计算差值
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                {
                    //判断是否没读取到电量等情况
                    if (Number.IsNumeric(ResultDictionary["试验后总电量"][i]) && Core.Function.Number.IsNumeric(ResultDictionary["试验前总电量"][i]))
                    {
                        ResultDictionary["总电量差值"][i] = (float.Parse(ResultDictionary["试验后总电量"][i]) - float.Parse(ResultDictionary["试验前总电量"][i])).ToString("F" + xsws);
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

                        err[j] = t.ToString("F" + xsws);
                        Zerr += t;

                    }

                    ResultDictionary["费率电量差值"][i] += string.Join(",", err);

                    ResultDictionary["组合误差"][i] = (float.Parse(ResultDictionary["总电量差值"][i]) - Zerr).ToString("F" + xsws);
                }
                RefUIData("总电量差值");
                RefUIData("费率电量差值");
                RefUIData("组合误差");
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                float fErr = float.Parse(ResultDictionary["组合误差"][i]);
                if (Math.Abs(fErr) > (0.01 * FL_List.Count))
                    ResultDictionary["结论"][i] = "不合格";
                else
                    ResultDictionary["结论"][i] = "合格";
            }
            RefUIData("结论");
        }


        /// <summary>
        /// 计算差值
        /// </summary>
        private void GetErrValue2()
        {
            MessageAdd("正在计算误差", EnumLogType.提示信息);

            int xsws = 2;
            float wcx = 0.01f;
            if (MeterInfo[FirstIndex].DgnProtocol.DecimalDigits > 2)
            {
                xsws = 4;
                wcx = 0.0001f;
            }
            //开始计算差值
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                {
                    string[] zdlQian = ResultDictionary["试验前总电量"][i].Split('_');
                    string[] zdlHpu = ResultDictionary["试验后总电量"][i].Split('_');
                    string zdlCha = "";
                    for (int j = 0; j < zdlQian.Length; j++)
                    {
                        //判断是否没读取到电量等情况
                        if (zdlQian[j] != null && zdlHpu[j] != null)
                        {
                            zdlCha = zdlCha + (float.Parse(zdlHpu[j]) - float.Parse(zdlQian[j])).ToString("F" + xsws) + "_";
                            ResultDictionary["总电量差值"][i] = zdlCha.TrimEnd('_');
                        }
                        else
                        {
                            ResultDictionary["总电量差值"][i] = "999";
                        }
                    }


                    //判断分费率差值
                    if (ResultDictionary["试验后费率电量"][i] == null || ResultDictionary["试验后费率电量"][i] == "" || ResultDictionary["试验前费率电量"][i] == null || ResultDictionary["试验前费率电量"][i] == "")
                    {
                        ResultDictionary["组合误差"][i] = "999";
                        ResultDictionary["费率电量差值"][i] = "999";
                        continue;
                    }
                    string fdlCha = "";
                    string zhCha = "";
                    for (int q = 0; q < zdlQian.Length; q++)
                    {
                        string[] fdlQian = ResultDictionary["试验前费率电量"][i].Split('_');
                        string[] fdlHpu = ResultDictionary["试验后费率电量"][i].Split('_');

                        string[] tem = fdlHpu[q].Split(',');
                        string[] tem2 = fdlQian[q].Split(',');

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

                            err[j] = t.ToString("F" + xsws);
                            Zerr += t;
                        }
                        fdlCha += string.Join(",", err) + "_";
                        ResultDictionary["费率电量差值"][i] = fdlCha.TrimEnd('_');

                        string[] zdlCha_ = ResultDictionary["总电量差值"][i].Split('_');
                        zhCha = zhCha + (float.Parse(zdlCha_[q]) - Zerr).ToString("F" + xsws) + "_";
                        ResultDictionary["组合误差"][i] = zhCha.TrimEnd('_');
                    }
                }
                RefUIData("总电量差值");
                RefUIData("费率电量差值");
                RefUIData("组合误差");
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                string[] fErr = ResultDictionary["组合误差"][i].Split('_');
                for (int j = 0; j < fErr.Length; j++)
                {
                    if (Math.Abs(float.Parse(fErr[j])) > (wcx * 4))
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        break;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "合格";
                    }
                }
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] arr = Test_Value.Split('|');
            if (arr.Length < 8) return false;

            curPoint.PowerFangXiang = (PowerWay)Enum.Parse(typeof(PowerWay), arr[0]);
            curPoint.PowerYj = (Cus_PowerYuanJian)Enum.Parse(typeof(Cus_PowerYuanJian), arr[1]);
            curPoint.Glys = arr[2];
            curPoint.xIb = arr[3];

            curPoint.FeiLv = (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), arr[4]);
            curPoint.FeiLvString = arr[4];
            Xib = Number.GetCurrentByIb(curPoint.xIb, OneMeterInfo.MD_UA, HGQ);
            curPoint.UseMinutes = float.Parse(arr[7]);
            if (arr[6].Trim() != "0" && arr[5] == "定时法")
            {
                curPoint.UseMinutes = float.Parse(arr[6]);
                //string dufen = _Prams[6] + "分";
                if (arr[6] == "" || !Number.IsNumeric(arr[6]))
                {
                    RunTime = 60;//运行60秒
                }
                else
                {
                    RunTime = (int)(float.Parse(arr[6]) * 60);
                }
            }
            else if (arr[5] == "定量法")
            {
                if (!string.IsNullOrWhiteSpace(arr[7]) && Number.IsNumeric(arr[7]))
                {
                    float RunkWh = float.Parse(arr[7]);
                    RunTime = (int)(RunkWh * 1000 * 3600 / CalculatePower(U, Xib, Clfs, Cus_PowerYuanJian.H, "1.0", true));
                }
            }
            _MaxTestTime = RunTime;

            FL_List.Clear();
            if (arr[13] == "是")     // 00:30(谷),08:30(平),10:00(峰),13:00(平),19:30(峰)
            {
                IsReadFLTime = true;
            }
            else
            {
                FL_List = GetFlList(arr[12], true);
            }

            Is24ZZ = arr[5] == "48h切换象限法";
            FangXiang = curPoint.PowerFangXiang;
            powerWay = curPoint.PowerFangXiang;//当前检定功率方向
            ResultNames = new string[] { "试验前总电量", "试验后总电量", "总电量差值", "试验前费率电量", "试验后费率电量", "费率电量差值", "组合误差", "结论" };
            return true;

        }
        private string GetProtocalName(PowerWay p)
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
        //struct RegisterStruct
        //{
        //    public int BW;              //表位号            
        //    //public string FL;           //费率
        //    //public float PowerStart;       //开始电量
        //    //public float PowerEnd;         //结束电量                        
        //    public override string ToString()
        //    {
        //        return string.Format("{0}|{1}|{2}|{3}", 0, 0, Error(), "");
        //    }

        //    /// <summary>
        //    /// 当前费率电量差值
        //    /// </summary>
        //    /// <returns></returns>
        //    public string Error()
        //    {
        //        //return (PowerEnd - PowerStart).ToString("0.00");
        //    }
        //}
    }
}
