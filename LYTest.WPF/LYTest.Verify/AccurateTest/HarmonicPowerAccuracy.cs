using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;


namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 谐波电能准确度试验 --就是施加谐波后做误差--这里把做基本误差封装成一个方法-免得每次都写
    /// </summary>
    public class HarmonicPowerAccuracy : VerifyBase     //HarmonicMeasureAccuracy
    {
        /// <summary>
        /// 功率方向
        /// </summary>
        PowerWay FangXian = PowerWay.正向有功;
        /// <summary>
        /// 电流
        /// </summary>
        float xib = 1f;
        /// <summary>
        /// 第几次
        /// </summary>
        int Frequency = 2;
        /// <summary>
        /// 谐波电流
        /// </summary>
        float HarmonicXib;
        /// <summary>
        /// 谐波电压
        /// </summary>
        float HarmonicUb;
        /// <summary>
        /// 谐波功率因数
        /// </summary>
        string HarmonicGLYS;
        /// <summary>
        /// 谐波相位角度
        /// </summary>
        float[] arrPhi;
        /// <summary>
        ///误差限
        /// </summary>
        float ErrorLimit;

        int qs = 2;

        //private StPlan_WcPoint CurPlan;

        bool IsStop = false; //是否退出当前检定项目--

        int MaxTime = 300000;
        readonly int maxWCnum = VerifyConfig.ErrorCount;//最多误差次数

        float meterLevel = 2;//等级

        ///// <summary>
        ///// 电压倍数
        ///// </summary>
        //float VoltageMultiple = 1f;

        public override void Verify()
        {
            base.Verify();
            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
            // 获取误差限
            GetErrorLimit();


            IsStop = Stop;
            qs = GetQs(VerifyConfig.IsTimeWcLapCount, OneMeterInfo.MD_UB, OneMeterInfo.MD_UA, "0.5Imax", Clfs, Cus_PowerYuanJian.H, OneMeterInfo.MD_Constant, "1.0", HGQ, VerifyConfig.WcMinTime);

            MaxTime = VerifyConfig.MaxHandleTime * 1000;

            if (Stop) return;

            #region

            string[] data = Test_Value.Split('|');

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["功率方向"][i] = FangXian.ToString();
                ResultDictionary["误差上限"][i] = ErrorLimit.ToString();
                ResultDictionary["误差下限"][i] = (-1 * ErrorLimit).ToString();
                ResultDictionary["谐波电流"][i] = data[2];
                ResultDictionary["谐波电压"][i] = data[1];
                ResultDictionary["谐波功率因数"][i] = HarmonicGLYS;
            }
            RefUIData("误差上限");
            RefUIData("误差下限");
            RefUIData("功率方向");
            RefUIData("谐波电流");
            RefUIData("谐波电压");
            RefUIData("谐波功率因数");


            //PowerOff();
            //WaitTime("正在关闭电流", 5);
            if (Stop) return;

            MessageAdd("正在设置谐波含量", EnumLogType.提示信息);
            SetHarmonic();
            WaitTime("等待功率源调整谐波", 5);
            MessageAdd("正在升源", EnumLogType.提示信息);
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
            if (Stop) return;

            //WaitTime("等待升源稳定", 15);
            //if (Stop) return;

            //MessageAdd("正在判断电能表连接状态", EnumLogType.提示信息);
            //ConnectIOTMeter();
            if (Stop) return;

            #endregion

            StartError();
            if (Stop) return;
            //MessageAdd("正在关源", EnumLogType.提示信息);
            //PowerOff();
            //WaitTime("正在关闭电流", 20);
            //if (Stop) return;

            MessageAdd("正在关闭谐波", EnumLogType.提示信息);
            HarmonicOff();

            MessageAdd("正在升源", EnumLogType.提示信息);
            PowerOn();
            WaitTime("正在升源", 10);
            if (Stop) return;

            //MessageAdd("正在判断电能表连接状态", EnumLogType.提示信息);
            //ConnectIOTMeter();


        }

        /// <summary>
        /// 启动谐波
        /// </summary>
        private void SetHarmonic()
        {
            //TODO 目前只用A限--后续需要三相把这里循环次数改成3就好--注意的是三相的情况电能表谐波总含量需要重新计算--标准表的谐波含量需要三相加起来
            for (int j = 0; j < 1; j++)  //相线  0单相--1b相-2c相
            {
                if (Clfs == Core.Enum.WireMode.单相 && j > 0) continue; //单相只需要a相的
                if (Clfs == Core.Enum.WireMode.三相三线 && j == 1) continue; //三相三线的只需要b相的
                for (int i = 0; i < 2; i++) //电压还是电流
                {
                    float[] HarmonicContent = new float[60];
                    float[] HarmonicPhase = new float[60];
                    if (i == 0)
                    {
                        HarmonicContent[Frequency - 2] = HarmonicUb / OneMeterInfo.MD_UB * 100;//谐波电流/标准电流=谐波含量
                    }
                    else
                    {
                        HarmonicContent[Frequency - 2] = HarmonicXib / xib * 100;//谐波电流/标准电流=谐波含量
                    }
                    HarmonicPhase[Frequency - 2] = arrPhi[i * 3 + j];//这里是相位角度--需要换算

                    //这里是因为一次只能设置一个的
                    string[] value = new string[6];
                    value.Fill("0");
                    value[i * 3 + j] = "1";
                    DeviceControl.StartHarmonicTest(value[0], value[1], value[2], value[3], value[4], value[5], HarmonicContent, HarmonicPhase, true);

                }
            }

            PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 30, 270, 150, 50, 110, 170, 50);
            WaitTime("正在开启谐波", 15);

        }

        /// <summary>
        /// 关闭谐波
        /// </summary>
        private void HarmonicOff()
        {
            MessageAdd("正在关闭谐波", EnumLogType.提示信息);
            DeviceControl.HarmonicOff();
            PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 30, 270, 150, 50, 110, 170, 50);
            WaitTime("正在关闭谐波", 15);
        }

        #region 误差

        private void StartError()
        {
            if (Stop) return;

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
                if (!ReadWc(ref curWC, ref curNum, FangXiang))    //读取误差
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
                    ResultDictionary[$"误差1"][i] = value[0].ToString();
                    RefUIData($"误差1");
                    if (Stop) break;
                    if (value.Length > 3)
                    {
                        if (value[1].ToString().Trim() != "")
                        {
                            ResultDictionary["误差2"][i] = value[0].ToString();
                            RefUIData("误差2");
                            ResultDictionary["误差1"][i] = value[1].ToString();
                            RefUIData("误差1");

                            //跳差判断
                            if (CheckJumpError(ResultDictionary["误差1"][i], ResultDictionary["误差2"][i], meterLevel, VerifyConfig.JumpJudgment))
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
                        ResultDictionary["平均值"][i] = value[2];
                        ResultDictionary["化整值"][i] = value[3];
                        RefUIData("平均值");
                        RefUIData("化整值");

                        // 根据平均值判断是否合格
                        RefUIReslut(value[2], i);
                    }
                }



                if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)  //全部都为true了
                    break;
            }

            RefUIData("结论");

            if (Stop) return;
            StopWcb(GetFangXianIndex(FangXiang), 0xff);//停止误差板

        }

        ///// <summary>
        ///// 刷超时结论
        ///// </summary>
        //private void RefUIOverTimeRelust()
        //{
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        ResultDictionary["结论"][i] = ConstHelper.不合格;
        //    }
        //    RefUIData("结论");
        //}

        /// <summary>
        /// 根据平均值判断是否合格
        /// </summary>
        /// <param name="avg"></param>
        /// <param name="i"></param>
        private void RefUIReslut(string avg, int i)
        {
            if (float.Parse(avg) > (-1 * ErrorLimit) && float.Parse(avg) < ErrorLimit)
            {
                ResultDictionary["结论"][i] = ConstHelper.合格;
            }
            else
            {
                ResultDictionary["结论"][i] = ConstHelper.不合格;
            }

        }

        //#region 计算方法

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
                float Tqs = (float)CzQS * ((float)_MeConst / (float)_MeConst);
                Tqs *= xIb / Number.GetCurrentByIb(czdl, I, HGQ) * Number.GetGlysValue(GLYS);
                if (YJ != Cus_PowerYuanJian.H)
                {
                    Tqs /= 3;
                }
                QS = (int)Math.Round((double)Tqs, 0);
            }

            if (QS <= 0)
                QS = 1;
            return QS;

        }


        private bool InitEquipment(int qs)
        {
            if (IsDemo) return true;
            bool isP = (FangXiang == PowerWay.正向有功 || FangXiang == PowerWay.反向有功);
            int[] meterconst = MeterHelper.Instance.MeterConst(isP);

            //float xIb = Number.GetCurrentByIb(xib.ToString(), OneMeterInfo.MD_UA, HGQ);

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

        ///// <summary>
        ///// 修正数字加+-号
        ///// </summary>
        ///// <param name="data">要修正的数字</param>
        ///// <param name="Priecision">修正精度</param>
        ///// <returns>返回指定精度的带+-号的字符串</returns>
        //private string AddFlag(float data, int Priecision)
        //{
        //    string v = data.ToString(string.Format("F{0}", Priecision));
        //    if (float.Parse(v) > 0)
        //        return string.Format("+{0}", v);
        //    else
        //        return v;
        //}
        ///// <summary>
        ///// 返回修正间距
        ///// </summary>
        ///// <IsWindage>是否是偏差</IsWindage> 
        ///// <returns></returns>
        //private float GetWuChaHzzJianJu(bool IsWindage, float meterLevel)
        //{
        //    Dictionary<string, float[]> DicJianJu = null;
        //    string Key = string.Format("Level{0}", meterLevel);
        //    //根据表精度及表类型生成主键
        //    //if (ErrLimit.IsSTM)
        //    //    Key = string.Format("Level{0}B", ErrLimit.MeterLevel);
        //    //else
        //    //    Key = string.Format("Level{0}", ErrLimit.MeterLevel);

        //    if (DicJianJu == null)
        //    {
        //        DicJianJu = new Dictionary<string, float[]>
        //        {
        //            { "Level0.02B", new float[] { 0.002F, 0.0002F } },      //0.02级表标准表
        //            { "Level0.05B", new float[] { 0.005F, 0.0005F } },      //0.05级表标准表
        //            { "Level0.1B", new float[] { 0.01F, 0.001F } },         //0.1级表标准表
        //            { "Level0.2B", new float[] { 0.02F, 0.002F } },         //0.2级标准表
        //            { "Level0.2", new float[] { 0.02F, 0.004F } },          //0.2级普通表
        //            { "Level0.5", new float[] { 0.05F, 0.01F } },           //0.5级表
        //            { "Level1", new float[] { 0.1F, 0.02F } },              //1级表
        //            { "Level1.5", new float[] { 0.2F, 0.04F } }  ,           //2级表
        //            { "Level2", new float[] { 0.2F, 0.04F } }               //2级表
        //        };
        //    }

        //    float[] JianJu;
        //    if (DicJianJu.ContainsKey(Key))
        //    {
        //        JianJu = DicJianJu[Key];
        //    }
        //    else
        //    {
        //        JianJu = new float[] { 2, 2 };    //没有在字典中找到，则直接按2算
        //    }

        //    if (IsWindage)
        //        return JianJu[1];//标偏差
        //    else
        //        return JianJu[0];//普通误差
        //}

        //#endregion

        ///// <summary>
        ///// 加+-符号
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //private string AddFlag(string data)
        //{
        //    if (float.Parse(data) > 0)
        //        return string.Format("+{0}", data);
        //    else
        //        return data;
        //}

        #endregion

        /// <summary>
        /// 获取误差限
        /// </summary>
        private void GetErrorLimit()
        {
            if (Number.GetCurrentByIb("0.05Ib", OneMeterInfo.MD_UA, HGQ) <= HarmonicXib && HarmonicXib < Number.GetCurrentByIb("0.1Ib", OneMeterInfo.MD_UA, HGQ) && HarmonicGLYS == "1.0")
            {
                ErrorLimit = 0.15F * (1 + (0.01F * Frequency));
            }
            else if (Number.GetCurrentByIb("0.1Ib", OneMeterInfo.MD_UA, HGQ) <= HarmonicXib && HarmonicXib <= Number.GetCurrentByIb("0.4Imax", OneMeterInfo.MD_UA, HGQ) && HarmonicGLYS == "1.0")
            {
                ErrorLimit = 0.05F * (1 + (0.01F * Frequency));
            }
            else if (Number.GetCurrentByIb("0.1Ib", OneMeterInfo.MD_UA, HGQ) <= HarmonicXib && HarmonicXib < Number.GetCurrentByIb("0.2Ib", OneMeterInfo.MD_UA, HGQ) && (HarmonicGLYS == "0.5L" || HarmonicGLYS == "0.8C"))
            {
                ErrorLimit = 0.15F * (1 + (0.01F * Frequency));
            }
            else if (Number.GetCurrentByIb("0.2Ib", OneMeterInfo.MD_UA, HGQ) <= HarmonicXib && HarmonicXib <= Number.GetCurrentByIb("0.4Imax", OneMeterInfo.MD_UA, HGQ) && (HarmonicGLYS == "0.5L" || HarmonicGLYS == "0.8C"))
            {
                ErrorLimit = 0.06F * (1 + (0.01F * Frequency));
            }



        }




        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            try
            {
                //参数--第几次--谐波电压--谐波电流--谐波功率因数 --这里可能还需要电压电流功率方向等

                //实际电流--可以理解为谐波的电流--也可以理解为施加谐波后升源从标准表读回的电流


                string[] data = Test_Value.Split('|');
                xib = Number.GetCurrentByIb("0.5Imax", OneMeterInfo.MD_UA, HGQ);
                Frequency = int.Parse(data[4].TrimStart('第').TrimEnd('次'));
                float value = float.Parse(data[1].TrimEnd("Unom".ToCharArray()));
                HarmonicUb = OneMeterInfo.MD_UB * value;
                HarmonicXib = Number.GetCurrentByIb(data[2], OneMeterInfo.MD_UA, HGQ);
                HarmonicGLYS = data[3];
                FangXian = (PowerWay)Enum.Parse(typeof(PowerWay), data[0]);
                ResultNames = new string[] { "功率方向", "误差上限", "误差下限", "谐波电流", "谐波电压", "谐波功率因数", "误差1", "误差2", "平均值", "化整值", "结论" };
                arrPhi = Common.GetPhiGlys(Clfs, FangXian, Core.Enum.Cus_PowerYuanJian.H, HarmonicGLYS, Core.Enum.Cus_PowerPhase.正相序);
                //计算误差线
                //ErrorLimitUB = OneMeterInfo.MD_UB * 0.0015f;
                //if (value >= 0.03)
                //{
                //    ErrorLimitUB = OneMeterInfo.MD_UB * 0.05f;
                //}
                //value = HarmonicXib / xib;
                //ErrorLimitIB = xib * 0.005f;
                //if (value >= 0.1)
                //{
                //    ErrorLimitIB = xib * 005f;
                //}
                //ErrorLimit
            }
            catch (Exception ex)
            {
                MessageAdd("参数验证失败\r\n" + ex.ToString(), EnumLogType.错误信息);
                return false;
            }
            return true;
        }
    }
}
