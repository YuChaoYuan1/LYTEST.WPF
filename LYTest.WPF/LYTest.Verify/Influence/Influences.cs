using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Struct;
using LYTest.Verify.AccurateTest;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.Influence
{
    //add yjt 20220805 新增影响量检定类
    /// <summary>
    /// 影响量试验
    /// </summary>
    public class Influences : VerifyBase
    {
        #region 参数

        /// <summary>
        /// 误差限
        /// </summary>
        float ErrorLimit = 1f;
        /// <summary>
        /// 功率元件
        /// </summary>
        Cus_PowerYuanJian YJ = Cus_PowerYuanJian.H;
        /// <summary>
        /// 功率方向
        /// </summary>
        PowerWay FangXian = PowerWay.正向有功;
        /// <summary>
        /// 功率因数
        /// </summary>
        string Glys = "1.0";
        /// <summary>
        /// 电流倍数
        /// </summary>
        string Xib = "1.0Ib";

        /// <summary>
        /// 频率
        /// </summary>
        float _pl = 50f;

        /// <summary>
        /// 电压倍数
        /// </summary>
        float VoltageMultiple = 1f;

        /// <summary>
        /// 相序
        /// </summary>
        Cus_PowerPhase phaseSequenceType = Cus_PowerPhase.正相序;
        readonly int maxWCnum = VerifyConfig.ErrorCount;//最多误差次数
        float meterLevel = 2;//等级
        int MaxTime = 300000;
        bool IsStop = false; //是否退出当前检定项目--
        int qs = 2;
        #endregion

        public override void Verify()
        {
            base.Verify();
            IsStop = Stop;
            qs = GetQs(VerifyConfig.IsTimeWcLapCount, OneMeterInfo.MD_UB, OneMeterInfo.MD_UA, Xib, Clfs, YJ, OneMeterInfo.MD_Constant, Glys, HGQ, VerifyConfig.WcMinTime);
            this._pl = OneMeterInfo.MD_Frequency;

            #region 上传误差参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    //"功率元件","功率方向","电流倍数","功率因数","误差下限","误差上限","误差圈数"
                    ResultDictionary["功率元件"][i] = YJ.ToString();
                    ResultDictionary["功率方向"][i] = FangXian.ToString();
                    ResultDictionary["电流倍数"][i] = Xib.ToString();
                    ResultDictionary["功率因数"][i] = Glys;
                    ResultDictionary["误差下限"][i] = (-ErrorLimit).ToString();
                    if (ErrorLimit == 10)
                    {
                        ResultDictionary["误差下限"][i] = (-100).ToString();
                    }
                    ResultDictionary["误差上限"][i] = ErrorLimit.ToString();
                    ResultDictionary["误差圈数"][i] = qs.ToString();
                }
            }
            RefUIData("功率元件");
            RefUIData("功率方向");
            RefUIData("电流倍数");
            RefUIData("功率因数");
            RefUIData("误差下限");
            RefUIData("误差上限");
            RefUIData("误差圈数");
            #endregion


            MaxTime = VerifyConfig.MaxHandleTime * 1000;

            //开始做一次基本误差
            StartError("");
            if (Stop) return;
            if (!IsStop)//如果之前的误差有问题就别做影响量后的了，浪费时间
            {
                PowerOn();//先把电流关了
                WaitTime("影响量前检定完成，关闭电流", 5);

                InitEquipMent();
                WaitTime("正在修改影响量", 3);
                //影响后误差
                StartError("影响后");
                //这里计算改变量--计算误差
            }
            SwitchChannel(LYTest.MeterProtocol.Enum.Cus_ChannelType.通讯485);

            if (Stop) return;
            if (IsStop)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检
                    ResultDictionary["结论"][i] = "不合格";
                }
            }
            else
            {
                //计算变差值

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检

                    string bc = "";
                    if (ResultDictionary["平均值"][i] != null && ResultDictionary["影响后平均值"][i] != null
                        && !string.IsNullOrWhiteSpace(ResultDictionary["平均值"][i]) || !string.IsNullOrWhiteSpace(ResultDictionary["影响后平均值"][i]))
                    {
                        bc = Math.Abs(float.Parse(ResultDictionary["影响后平均值"][i]) - float.Parse(ResultDictionary["平均值"][i])).ToString("F4");
                    }
                    ResultDictionary["变差值"][i] = bc;
                    if (bc != "")
                    {
                        if (Math.Abs(float.Parse(bc)) > ErrorLimit)
                        {
                            ResultDictionary["结论"][i] = "不合格";
                        }
                        else
                        {
                            ResultDictionary["结论"][i] = "合格";
                        }
                    }
                }
                RefUIData("变差值");
            }

            RefUIData("结论");



            //切换回去正常谐波
            if (!IsStop)
            {
                var Test_No_ID = Test_No;
                if (Test_No_ID.IndexOf('_') >= 0) Test_No_ID = Test_No_ID.Substring(0, Test_No_ID.IndexOf('_'));
                if (Test_No_ID == ProjectID.方顶波波形试验 || Test_No_ID == ProjectID.尖顶波波形改变 || Test_No_ID == ProjectID.脉冲群触发波形试验 || Test_No_ID == ProjectID.九十度相位触发波形试验)
                {
                    if (!DeviceControl.SetPowerHarmonic("1", "1", "1", "1", "1", "1", 0))
                    {
                        MessageAdd("切换回正常谐波出错,请检查", EnumLogType.错误信息);
                    }

                }
                else if (Test_No_ID == ProjectID.第N次谐波试验)
                {
                    float[] HarmonicContent = new float[59];
                    float[] HarmonicPhase = new float[59];
                    DeviceControl.StartHarmonicTest("1", "1", "1", "1", "1", "1", HarmonicContent, HarmonicPhase, false);

                }
                else if (Test_No_ID == ProjectID.半波整流波形试验)
                {
                    DeviceControl.SetControlboard("0", "0", "0");

                }
                PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, 50);
                WaitTime("正在关闭谐波", 15);
            }
            PowerOn();
            WaitTime("检定完成，关闭电流", 5);
            MessageAdd("检定完成", EnumLogType.提示信息);
        }
        protected override bool CheckPara()
        {
            string[] data = Test_Value.Split('|');
            FangXian = (PowerWay)Enum.Parse(typeof(PowerWay), data[0]);
            Glys = data[2];
            Xib = data[3];
            ErrorLimit = float.Parse(data[4] ?? "1");
            //string[] Test_Nos = Test_No.Split('_');

            ResultNames = new string[] { "功率元件", "功率方向", "电流倍数", "功率因数", "误差下限", "误差上限", "误差圈数", "误差1", "误差2", "平均值", "化整值", "影响后误差1", "影响后误差2", "影响后平均值", "影响后化整值", "结论", "变差值" };
            return true;
        }


        #region 计算方法

        /// <summary>
        /// 获取检定圈数
        /// </summary>
        /// <param name="IsTimecalculation">是否使用时间计算</param>
        /// <param name="U">电压</param>
        /// <param name="I">电流</param>
        /// <param name="PowerDianLiu">电流倍数</param>
        /// <param name="JXFS">接线方式</param>
        /// <param name="YJ">功率元件</param>
        /// <param name="MeConst">常数</param>
        /// <param name="GLYS">功率因数</param>
        /// <param name="HGQ">是否经过互感器</param>
        /// <param name="time">最小时间</param>
        /// <returns></returns>
        public int GetQs(bool IsTimecalculation, float U, string I, string PowerDianLiu, WireMode JXFS, Cus_PowerYuanJian YJ, string MeConst, string GLYS, bool HGQ, float time = 5)
        {
            //电压，电流，测量方式，元件，功率因数，有功无功，
            float xIb = Number.GetCurrentByIb(PowerDianLiu, I, HGQ);

            float CzQS = 2; //参照圈数
            string czdl = "1.0Ib"; //参照电流
            if (OneMeterInfo.MD_JJGC == "IR46")
            {
                CzQS = 1;
                czdl = "10Itr";
            }

            int _MeConst;
            bool IsYouGong = true;
            if (this.FangXian == PowerWay.正向有功 || this.FangXian == PowerWay.反向有功)
            {
                _MeConst = Number.GetBcs(MeConst, FangXian);
            }
            else
            {
                _MeConst = Number.GetBcs(MeConst, FangXian);
                IsYouGong = false;
            }
            int QS;
            if (IsTimecalculation)    //使用时间计算
            {
                // 圈数计算方法
                float currentPower = Number.CalculatePower(U, xIb, JXFS, YJ, GLYS, IsYouGong);
                //计算一度大需要的时间,单位分钟
                int onePulseTime = (int)Math.Ceiling(time / (1 / (currentPower * _MeConst / 3600000)));
                QS = onePulseTime;

            }
            else
            {
                float Tqs = (float)CzQS * (_MeConst / (float)_MeConst);
                Tqs *= xIb / Number.GetCurrentByIb(czdl, I, HGQ) * Number.GetGlysValue(GLYS);
                if (YJ != Cus_PowerYuanJian.H)
                {
                    Tqs /= 3;
                }
                QS = (int)Math.Round(Tqs, 0);
            }

            if (QS <= 0)
                QS = 1;
            return QS;

        }


        private bool InitEquipment(int qs)
        {
            if (IsDemo) return true;
            if (IsDemo) return true;
            bool isP = (FangXian == PowerWay.正向有功 || FangXian == PowerWay.反向有功);
            int[] meterconst = MeterHelper.Instance.MeterConst(isP);

            float xIb = Number.GetCurrentByIb(Xib, OneMeterInfo.MD_UA, HGQ);

            MessageAdd("正在升源...", EnumLogType.提示信息);



            if (!PowerOn(OneMeterInfo.MD_UB * VoltageMultiple, OneMeterInfo.MD_UB * VoltageMultiple, OneMeterInfo.MD_UB * VoltageMultiple, xIb, xIb, xIb, YJ, FangXian, Glys, _pl, phaseSequenceType))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return false;
            }

            ulong constants = GetStaConst();


            MessageAdd("正在设置标准表脉冲...", EnumLogType.提示信息);
            int index = 0;
            if (!isP)
            {
                index = 1;
            }
            SetPulseType((index + 49).ToString("x"));
            if (Stop) return true;
            MessageAdd("开始初始化基本误差检定参数!", EnumLogType.提示信息);
            //设置误差版被检常数
            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
            int SetConstants = (int)(constants / 100);
            SetStandardConst(0, SetConstants, -2, 0xff);
            //设置误差版标准常数 TODO2
            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
            int[] q = new int[MeterNumber];
            q.Fill(qs);
            if (!SetTestedConst(index, meterconst, 0, q))
            {
                MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                return false;
            }
            return true;
        }


        private int GetFangXianIndex(PowerWay fx)
        {
            int readType = 0;
            switch (fx)
            {
                case PowerWay.正向有功:
                    readType = 0;
                    break;
                case PowerWay.正向无功:
                    readType = 1;
                    break;
                case PowerWay.反向有功:
                    readType = 0;
                    break;
                case PowerWay.反向无功:
                    readType = 1;
                    break;
                default:
                    break;
            }
            return readType;
        }

        /// <summary>
        /// 计算基本误差
        /// </summary>
        /// <param name="data">要参与计算的误差数组</param>
        /// <returns></returns>
        public ErrorResoult SetWuCha(StPlan_WcPoint wcPoint, float meterLevel, float[] data)
        {
            ErrorResoult resoult = new ErrorResoult();
            float space = GetWuChaHzzJianJu(false, meterLevel);                              //化整间距 
            float avg = Number.GetAvgA(data);
            float hz = Number.GetHzz(avg, space);

            //添加符号
            int hzPrecision = Common.GetPrecision(space.ToString());
            string AvgNumber = AddFlag(avg, VerifyConfig.PjzDigit).ToString();

            string HZNumber = hz.ToString(string.Format("F{0}", hzPrecision));
            if (hz != 0f) //化整值为0时，不加正负号
                HZNumber = AddFlag(hz, hzPrecision);

            if (avg < 0) HZNumber = HZNumber.Replace('+', '-'); //平均值<0时，化整值需为负数

            // 检测是否超过误差限
            if (avg >= wcPoint.ErrorXiaXian && avg <= wcPoint.ErrorShangXian)
                resoult.Result = ConstHelper.合格;
            else
                resoult.Result = ConstHelper.不合格;

            //记录误差
            string strWuCha = string.Empty;
            int wcCount = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != ConstHelper.没有误差默认值)
                {
                    wcCount++;
                    strWuCha += string.Format("{0}|", AddFlag(data[i], VerifyConfig.PjzDigit));
                }
                else
                {
                    strWuCha += " |";
                }
            }
            if (wcCount != data.Length)
            {
                resoult.Result = ConstHelper.不合格;
            }

            strWuCha += string.Format("{0}|", AvgNumber);
            strWuCha += string.Format("{0}", HZNumber);
            resoult.ErrorValue = strWuCha;

            return resoult;
        }


        #endregion

        private void StartError(string startString)
        {
            if (Stop || IsStop) return;

            if (Stop) return;
            SetBluetoothModule(GetFangXianIndex(FangXian));
            if (!InitEquipment(qs))
            {
                MessageAdd("初始化基本误差设备参数失败", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;

            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            if (!StartWcb(GetFangXianIndex(FangXian), 0xff))
            {
                MessageAdd("误差板启动失败...", EnumLogType.提示信息);
                return;
            }
            MessageAdd("开始检定...", EnumLogType.提示信息);
            ErrorResoult[] errorResoults = new ErrorResoult[MeterNumber];
            StPlan_WcPoint[] arrPlanList = new StPlan_WcPoint[MeterNumber];      // 误差点数据
            int[] WCNumner = new int[MeterNumber]; //检定次数
            bool[] arrCheckOver = new bool[MeterNumber];     //表位完成记录
            int[] lastNum = new int[MeterNumber];                   //保存上一次误差的序号
            lastNum.Fill(-1);
            List<string>[] errList = new List<string>[MeterNumber]; //记录当前误差[数组长度，]
            for (int i = 0; i < MeterNumber; i++)
                errList[i] = new List<string>();

            DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
            while (true)
            {
                if (Stop) break;
                if (TimeSubms(DateTime.Now, TmpTime1) > MaxTime && !IsMeterDebug) //超出最大处理时间并且不是调表状态
                {
                    IsStop = true;
                    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                    break;
                }
                if (IsStop || Stop) break;
                string[] curWC = new string[MeterNumber];   //重新初始化本次误差
                int[] curNum = new int[MeterNumber];        //当前读取的误差序号
                curWC.Fill("");
                curNum.Fill(0);
                if (!ReadWc(ref curWC, ref curNum, FangXian))    //读取误差
                {
                    continue;
                }
                if (Stop) break;

                //依次处理每个表位的误差数据
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) arrCheckOver[i] = true;     //表位不要检
                    if (arrCheckOver[i] && !IsMeterDebug) continue;   //表位检定通过了
                    if (lastNum[i] >= curNum[i]) continue;
                    if (string.IsNullOrEmpty(curWC[i])) continue;
                    if (curNum[i] <= VerifyConfig.ErrorStartCount) continue; //当前误差次数小于去除的个数

                    if (curNum[i] > lastNum[i]) //大于上一次误差次数
                    {
                        WCNumner[i]++;   //检定次数

                        lastNum[i] = curNum[i];
                    }
                    errList[i].Insert(0, curWC[i]);
                    if (errList[i].Count > maxWCnum)
                        errList[i].RemoveAt(errList[i].Count - 1);
                    meterLevel = MeterLevel(MeterInfo[i]);
                    if (Stop) break;
                    //计算误差
                    float[] tpmWc = ArrayConvert.ToSingle(errList[i].ToArray());  //Datable行到数组的转换
                    arrPlanList[i].ErrorShangXian = ErrorLimit;
                    arrPlanList[i].ErrorXiaXian = -ErrorLimit;

                    ErrorResoult tem = SetWuCha(arrPlanList[i], meterLevel, tpmWc);

                    if (errList[i].Count >= maxWCnum)  //误差数量>=需要的最大误差数2
                    {
                        arrCheckOver[i] = true;
                        if (tem.Result != ConstHelper.合格)
                        {
                            if (WCNumner[i] <= VerifyConfig.ErrorMax)
                            {
                                arrCheckOver[i] = false;
                            }
                        }
                    }
                    else
                    {
                        arrCheckOver[i] = false;
                        tem.Result = ConstHelper.不合格;
                        NoResoult[i] = "没有读取到俩次误差";
                    }
                    errorResoults[i] = tem;

                    string[] value = errorResoults[i].ErrorValue.Split('|');
                    ResultDictionary[startString + $"误差1"][i] = value[0].ToString();
                    RefUIData(startString + $"误差1");
                    if (Stop) break;
                    if (value.Length > 3)
                    {
                        if (value[1].ToString().Trim() != "")
                        {
                            ResultDictionary[startString + "误差2"][i] = value[0].ToString();
                            RefUIData(startString + "误差2");
                            ResultDictionary[startString + "误差1"][i] = value[1].ToString();
                            RefUIData(startString + "误差1");

                            //跳差判断
                            if (CheckJumpError(ResultDictionary[startString + "误差1"][i], ResultDictionary[startString + "误差2"][i], meterLevel, VerifyConfig.JumpJudgment))
                            {
                                arrCheckOver[i] = false;
                                if (WCNumner[i] > VerifyConfig.ErrorMax)
                                    arrCheckOver[i] = true;
                                else
                                {
                                    MessageAdd("检测到" + string.Format("{0}", i + 1) + "跳差，重新取误差进行计算", EnumLogType.提示信息);
                                    MessageAdd("检测到" + string.Format("{0}", i + 1) + "跳差，重新取误差进行计算", EnumLogType.流程信息);
                                }
                            }
                        }

                    }
                    if (value.Length > 3)
                    {
                        ResultDictionary[startString + "平均值"][i] = value[2];
                        ResultDictionary[startString + "化整值"][i] = value[3];
                        RefUIData(startString + "平均值");
                        RefUIData(startString + "化整值");
                    }
                }

                if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)  //全部都为true了
                    break;
            }
            if (Stop) return;
            StopWcb(GetFangXianIndex(FangXian), 0xff);//停止误差板

        }


        public bool InitEquipMent()
        {
            if (IsDemo) return true;
            bool T = true;
            string[] arr = Test_Value.Split('|');
            //是否有电流
            float.TryParse(OneMeterInfo.MD_UA, out float xIb);


            string[] Test_Nos = Test_No.Split('_');
            switch (Test_Nos[0])
            {
                #region add wkw 20220413 谐波影响
                //正向有功|H|1.0|Ib|1|5,10,120|5,10,120|5,10,120|5,10,120|5,10,120|5,10,120
                case ProjectID.第N次谐波试验:
                    float[] HarmonicContent = new float[60];
                    float[] HarmonicPhase = new float[60];
                    int count;
                    float content;
                    float phase;
                    if (arr[5] == arr[6] &&
                        arr[5] == arr[7] &&
                        arr[5] == arr[8] &&
                        arr[5] == arr[9] &&
                        arr[5] == arr[10])  //设置相同
                    {
                        count = int.Parse(arr[5].Split(',')[0]);
                        content = float.Parse(arr[5].Split(',')[1]);
                        phase = float.Parse(arr[5].Split(',')[2]);

                        HarmonicContent[count - 2] = content;
                        HarmonicPhase[count - 2] = phase;
                        DeviceControl.StartHarmonicTest("1", "1", "1", "1", "1", "1", HarmonicContent, HarmonicPhase, true);
                    }
                    else //设置不相同
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            HarmonicContent = new float[60];
                            HarmonicPhase = new float[60];
                            string ua = "0";
                            string ub = "0";
                            string uc = "0";
                            string ia = "0";
                            string ib = "0";
                            string ic = "0";
                            if (i == 0)
                            {
                                ua = "1";
                            }
                            if (i == 1)
                            {
                                ub = "1";
                            }
                            if (i == 2)
                            {
                                uc = "1";
                            }
                            if (i == 3)
                            {
                                ia = "1";
                            }
                            if (i == 4)
                            {
                                ib = "1";
                            }
                            if (i == 5)
                            {
                                ic = "1";
                            }

                            //谐波次数、谐波含量、谐波相位
                            count = int.Parse(arr[5 + i].Split(',')[0]);
                            content = float.Parse(arr[5 + i].Split(',')[1]);
                            phase = float.Parse(arr[5 + i].Split(',')[2]);
                            HarmonicContent[count - 2] = content;
                            HarmonicPhase[count - 2] = phase;

                            DeviceControl.StartHarmonicTest(ua, ub, uc, ia, ib, ic, HarmonicContent, HarmonicPhase, true);

                        }
                    }
                    break;
                case ProjectID.方顶波波形试验:
                    DeviceControl.SetPowerHarmonic("1", "1", "1", "1", "1", "1", 1);
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, _pl);
                    WaitTime("正在开启谐波", 15);
                    break;
                case ProjectID.尖顶波波形改变:
                    T = DeviceControl.SetPowerHarmonic("1", "1", "1", "1", "1", "1", 2);
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, _pl);
                    WaitTime("正在开启谐波", 15);
                    break;
                case ProjectID.脉冲群触发波形试验:    // 间谐波波形改变
                    T = DeviceControl.SetPowerHarmonic("1", "1", "1", "1", "1", "1", 3);
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, _pl);
                    WaitTime("正在开启谐波", 15);
                    break;
                case ProjectID.九十度相位触发波形试验:   //奇次谐波波形试验
                    T = DeviceControl.SetPowerHarmonic("1", "1", "1", "1", "1", "1", 4);
                    PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, _pl);
                    WaitTime("正在开启谐波", 15);
                    break;
                case ProjectID.半波整流波形试验:  //偶次谐波试验
                    T = DeviceControl.SetControlboard("1", "1", "1");

                    break;
                #endregion

                #region add wkw 20220512 其他影响
                //正向有功|H|1.0|Ib|1|10
                case ProjectID.电压改变:

                    VoltageMultiple = float.Parse(arr[5]) / 100;
                    break;

                //正向有功|H|1.0|Ib|1|49
                case ProjectID.频率改变:
                    if (int.TryParse(arr[5], out int f))
                        _pl = f;
                    break;

                //正向有功|H|1.0|Ib|1|20|10|30
                case ProjectID.负载不平衡试验:

                    switch (arr[1])
                    {
                        case "H":
                            YJ = Cus_PowerYuanJian.H;
                            break;
                        case "A":
                            YJ = Cus_PowerYuanJian.A;
                            break;
                        case "B":
                            YJ = Cus_PowerYuanJian.B;
                            break;
                        case "C":
                            YJ = Cus_PowerYuanJian.C;
                            break;
                        default:
                            YJ = Cus_PowerYuanJian.H;
                            break;
                    }
                    break;
                #endregion

                #region add wkw 20220513
                //正向有功|H|1.0|Ib|1
                case ProjectID.辅助装置试验:

                    SwitchChannel(LYTest.MeterProtocol.Enum.Cus_ChannelType.通讯载波);
                    break;

                //正向有功|H|1.0|Ib|1|正相序
                case ProjectID.逆相序试验:
                    phaseSequenceType = arr[5].Equals("正相序") ? Cus_PowerPhase.正相序 : Cus_PowerPhase.逆相序;
                    break;
                //正向有功|H|1.0|Ib|1|0|100|0
                case ProjectID.一相或两相电压中断试验:

                    #region 旧
                    #endregion
                    if (float.TryParse(arr[5].ToString(), out float ua_percentage) &&
                        float.TryParse(arr[6].ToString(), out float ub_percentage) &&
                        float.TryParse(arr[7].ToString(), out float uc_percentage)
                        )
                    {
                        PowerOn(OneMeterInfo.MD_UB * ua_percentage / 100,
                            OneMeterInfo.MD_UB * ub_percentage / 100,
                            OneMeterInfo.MD_UB * uc_percentage / 100,
                            xIb, xIb, xIb,
                            YJ,
                            FangXian,
                            Glys,
                            PL);
                    }


                    break;
                #endregion

                case ProjectID.高次谐波:

                    break;

                default:
                    break;
            }



            return T;
        }

    }
}
