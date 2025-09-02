using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.DAL.Config;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 基本误差试验
    /// </summary>
    class BasicError : VerifyBase
    {
        /// <summary>
        /// 误差限倍数
        /// </summary>
        private float ErrorProportion = 1f;  //误差限倍数

        public bool IsPC = false;
        private StPlan_WcPoint CurPlan;

        public override void Verify()
        {

            if (IsPC)
            {
                MessageAdd("标准偏差试验检定开始...", EnumLogType.流程信息);
            }
            else
            {
                MessageAdd("基本误差试验检定开始...", EnumLogType.流程信息);
            }

            base.Verify();
            bool[] arrCheckOver = new bool[MeterNumber];                                        //表位完成记录
            int[] lastNum = new int[MeterNumber];                   //保存上一次误差的序号
            lastNum.Fill(-1);
            int[] WCNumner = new int[MeterNumber]; //检定次数

            int maxWCnum = IsPC ? VerifyConfig.PcCount : VerifyConfig.ErrorCount;      //每个点合格误差次数

            StPlan_WcPoint[] arrPlanList = new StPlan_WcPoint[MeterNumber];      // 误差点数据
            int[] arrPulseLap = new int[MeterNumber];

            InitVerifyPara(ref arrPlanList, ref arrPulseLap);


            if (Stop) return;

            ErrorResoult[] errorResoults = new ErrorResoult[MeterNumber];

            #region 上传误差参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    ResultDictionary["功率元件"][i] = arrPlanList[i].PowerYuanJian.ToString();
                    ResultDictionary["功率方向"][i] = arrPlanList[i].PowerFangXiang.ToString();
                    ResultDictionary["电流倍数"][i] = arrPlanList[i].PowerDianLiu;
                    ResultDictionary["功率因数"][i] = arrPlanList[i].PowerYinSu;
                    ResultDictionary["误差下限"][i] = arrPlanList[i].ErrorXiaXian.ToString();
                    ResultDictionary["误差上限"][i] = arrPlanList[i].ErrorShangXian.ToString();
                    ResultDictionary["误差圈数"][i] = arrPlanList[i].LapCount.ToString();
                }
            }
            RefUIData("功率元件");
            RefUIData("功率方向");
            RefUIData("电流倍数");
            RefUIData("功率因数");
            RefUIData("误差下限");
            RefUIData("误差上限");
            RefUIData("误差圈数");

            float meterLevel;
            #endregion

            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    meterLevel = MeterLevel(MeterInfo[i]);
                    //演示模式获取随机数误差
                    float[] tpmWc = DemoBasicErr(meterLevel * ErrorProportion, maxWCnum);

                    //演示模式误差和偏差的结论
                    if (IsPC)
                    {
                        ErrorResoult errModel = SetPcCha(arrPlanList[i], meterLevel, tpmWc);
                        string[] value = errModel.ErrorValue.Split('|');
                        for (int q = 0; q < maxWCnum; q++)
                        {
                            ResultDictionary[$"误差" + (q + 1)][i] = value[q].ToString();
                            RefUIData($"误差" + (q + 1));
                        }

                        ResultDictionary[$"偏差值"][i] = value[value.Length - 2];
                        ResultDictionary[$"化整值"][i] = value[value.Length - 1];

                        RefUIData($"偏差值");
                        RefUIData($"化整值");

                        ResultDictionary[$"结论"][i] = errModel.Result;
                    }
                    else
                    {
                        ErrorResoult errModel = SetWuCha(arrPlanList[i], meterLevel, tpmWc);
                        string[] value = errModel.ErrorValue.Split('|');
                        ResultDictionary["误差1"][i] = value[0];
                        ResultDictionary["误差2"][i] = value[1];
                        ResultDictionary["平均值"][i] = value[2];
                        ResultDictionary["化整值"][i] = value[3];

                        RefUIData("误差1");
                        RefUIData("误差2");
                        RefUIData("平均值");
                        RefUIData("化整值");

                        ResultDictionary[$"结论"][i] = errModel.Result;
                    }


                }
                RefUIData("结论");
                return;
            }

            SetBluetoothModule(GetWcbFangXianIndex(CurPlan.PowerFangXiang));

            if (!ErrorInitEquipment(CurPlan.PowerFangXiang, CurPlan.PowerYuanJian, CurPlan.PowerYinSu, CurPlan.PowerDianLiu, arrPulseLap[0]))
            {
                MessageAdd("初始化基本误差设备参数失败", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;

            List<string>[] errList = new List<string>[MeterNumber]; //记录当前误差[数组长度，]
            for (int i = 0; i < MeterNumber; i++)
                errList[i] = new List<string>();

            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            if (!StartWcb(GetWcbFangXianIndex(CurPlan.PowerFangXiang), 0xff))
            {
                MessageAdd("误差板启动失败...", EnumLogType.提示信息);
                return;
            }
            MessageAdd("采集数据...", EnumLogType.提示信息);
            float xIb = Number.GetCurrentByIb(CurPlan.PowerDianLiu, OneMeterInfo.MD_UA, HGQ);
            // ms  = 2 * 理论时间+ 3分钟
            int MaxTime = 2 * maxWCnum * arrPulseLap[0] * OnePulseNeedTime(IsYouGong, 1000 * 60 / CalculatePower(OneMeterInfo.MD_UB, xIb, Clfs, CurPlan.PowerYuanJian, CurPlan.PowerYinSu, IsYouGong)) + 180 * 1000;//+读误差时间
            if (xIb < 0.08)
                MaxTime *= 5;

            DateTime TmpTime1 = DateTime.Now;
            while (true)
            {
                if (Stop) break;
                if (TimeSubms(DateTime.Now, TmpTime1) > MaxTime && !IsMeterDebug) //超出最大处理时间并且不是调表状态
                {
                    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                    break;
                }
                string[] curWC = new string[MeterNumber];   //重新初始化本次误差
                int[] curNum = new int[MeterNumber];        //当前读取的误差序号
                curWC.Fill("");
                curNum.Fill(0);

                if (!ReadWc(ref curWC, ref curNum, CurPlan.PowerFangXiang))    //读取误差
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

                    //计算误差
                    float[] tpmWc = ArrayConvert.ToSingle(errList[i].ToArray());  //Datable行到数组的转换

                    ErrorResoult tem;

                    //TODO标准偏差部分还没补充，后面补
                    if (IsPC)   //标准偏差
                    {
                        tem = SetPcCha(arrPlanList[i], meterLevel, tpmWc);
                    }
                    else  //基本误差
                    {
                        tem = SetWuCha(arrPlanList[i], meterLevel, tpmWc);
                    }

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
                    }
                    errorResoults[i] = tem;

                    string[] value = errorResoults[i].ErrorValue.Split('|');
                    ResultDictionary["误差1"][i] = value[0].ToString();
                    RefUIData("误差1");

                    RefUIData(ConstHelper.检定界面显示实时误差);

                    // modify yjt 20220327 修改偏差和误差的结论
                    if (!IsPC)
                    {
                        if (value.Length > 2)
                        {
                            if (!string.IsNullOrWhiteSpace(value[1]))
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
                                        MessageAdd($"检测到{i + 1}跳差，重新取误差进行计算", EnumLogType.流程信息);
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
                        }
                    }
                    else
                    {
                        for (int q = 0; q < value.Length - 2; q++)
                        {
                            ResultDictionary[$"误差" + (q + 1)][i] = value[q].ToString();
                            RefUIData($"误差" + (q + 1));
                        }

                        if (value.Length > 3)
                        {
                            //跳差判断
                            if (CheckJumpError(ResultDictionary["误差" + (value.Length - 3)][i], ResultDictionary["误差" + (value.Length - 2)][i], meterLevel, VerifyConfig.JumpJudgment))
                            {
                                arrCheckOver[i] = false;
                                if (WCNumner[i] > VerifyConfig.ErrorMax)
                                    arrCheckOver[i] = true;
                                else
                                {
                                    MessageAdd($"检测到{i + 1}跳差，重新取误差进行计算", EnumLogType.流程信息);
                                }
                            }
                        }

                        ResultDictionary["偏差值"][i] = value[value.Length - 2];
                        ResultDictionary["化整值"][i] = value[value.Length - 1];

                        RefUIData("偏差值");
                        RefUIData("化整值");
                    }

                    ResultDictionary["结论"][i] = errorResoults[i].Result;
                }

                if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)  //全部都为true了
                    break;
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检
                // modify yjt 20220327 修改偏差和误差的结论
                if (IsPC) //偏差
                {
                    if (ResultDictionary[$"偏差值"][i] == null || ResultDictionary[$"偏差值"][i].Trim() == "")
                    {
                        ResultDictionary[$"结论"][i] = ConstHelper.不合格;
                    }
                }
                else  //误差
                {
                    if (string.IsNullOrWhiteSpace(ResultDictionary[$"平均值"][i]))
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    }
                }
            }
            RefUIData("结论");
            StopWcb(GetWcbFangXianIndex(CurPlan.PowerFangXiang), 0xff);//停止误差板

            MessageAdd("检定完成", EnumLogType.提示信息);

            if (IsPC)
            {
                MessageAdd("标准偏差试验检定结束...", EnumLogType.流程信息);
            }
            else
            {
                MessageAdd("基本误差试验检定结束...", EnumLogType.流程信息);
            }
        }


        protected override bool CheckPara()
        {
            //误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序|误差圈数|误差限倍数(%)
            if (string.IsNullOrEmpty(Test_Value) || Test_Value.Split('|').Length < 9)
            {
                MessageAdd("基本误差参数错误", EnumLogType.错误信息);
                return false;
            }

            string[] arrayErrorPara = Test_Value.Split('|');
            IsPC = arrayErrorPara[0] == "标准偏差";  //是否是偏差

            if (!float.TryParse(arrayErrorPara[8], out float xLmt))
            {
                xLmt = 100;
            }
            ErrorProportion = xLmt / 100F;

            if (!string.IsNullOrWhiteSpace(VerifyConfig.ErrorRatio) && !IsPC)
            {
                ErrorProportion = float.Parse(VerifyConfig.ErrorRatio) / 100F;
            }

            CurPlan.PrjID = "111010700";

            CurPlan.IsCheck = true;
            CurPlan.LapCount = int.Parse(arrayErrorPara[7]);
            CurPlan.Dif_Err_Flag = 0;
            CurPlan.nCheckOrder = 1;
            CurPlan.Pc = 0;
            CurPlan.PointId = 1;
            CurPlan.PowerDianLiu = arrayErrorPara[4];
            CurPlan.PowerFangXiang = (PowerWay)Enum.Parse(typeof(PowerWay), arrayErrorPara[1]);
            CurPlan.PowerYinSu = arrayErrorPara[3];
            #region 功率元件
            switch (arrayErrorPara[2])
            {
                case "H":
                    CurPlan.PowerYuanJian = Cus_PowerYuanJian.H;
                    break;
                case "A":
                    CurPlan.PowerYuanJian = Cus_PowerYuanJian.A;
                    break;
                case "B":
                    CurPlan.PowerYuanJian = Cus_PowerYuanJian.B;
                    break;
                case "C":
                    CurPlan.PowerYuanJian = Cus_PowerYuanJian.C;
                    break;
                default:
                    CurPlan.PowerYuanJian = Cus_PowerYuanJian.H;
                    break;
            }
            #endregion
            CurPlan.XiangXu = 0;
            CurPlan.XieBo = 0;

            FangXiang = CurPlan.PowerFangXiang;

            if (IsPC)
            {
                ResultNames = new string[] { "功率元件", "功率方向", "电流倍数", "功率因数", "误差下限", "误差上限", "误差圈数", "误差1", "误差2", "误差3", "误差4", "误差5", "偏差值", "化整值", "结论" };
            }
            else
            {
                ResultNames = new string[] { "功率元件", "功率方向", "电流倍数", "功率因数", "误差下限", "误差上限", "误差圈数", "误差1", "误差2", "平均值", "化整值", "结论" };
            }

            return true;
        }


        #region 方法


        /// <summary>
        /// 计算基本误差
        /// </summary>
        /// <param name="data">要参与计算的误差数组</param>
        /// <returns></returns>
        private ErrorResoult SetWuCha(StPlan_WcPoint wcPoint, float meterLevel, float[] data)
        {
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

            //add yjt 20220701 根据平均值判断化整值的符号
            if (hz == 0)
            {
                HZNumber = $"{(avg < 0 ? "-" : "+")}{HZNumber}";
            }


            // 检测是否超过误差限
            ErrorResoult resoult = new ErrorResoult();
            if (avg >= wcPoint.ErrorXiaXian && avg <= wcPoint.ErrorShangXian)
                resoult.Result = ConstHelper.合格;
            else
                resoult.Result = ConstHelper.不合格;

            //记录误差
            string str = "";
            int wcCount = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != ConstHelper.没有误差默认值)
                {
                    wcCount++;
                    str += $"{AddFlag(data[i], VerifyConfig.PjzDigit)}|";
                }
                else
                {
                    str += " |";
                }
            }
            if (wcCount != data.Length)
            {
                resoult.Result = ConstHelper.不合格;
            }

            resoult.ErrorValue = $"{str}{AvgNumber}|{HZNumber}";

            return resoult;
        }

        /// <summary>
        /// 标准偏差计算
        /// </summary>
        /// <param name="wcPoint"></param>
        /// <param name="meterLevel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorResoult SetPcCha(StPlan_WcPoint wcPoint, float meterLevel, float[] data)
        {
            float space = GetWuChaHzzJianJu(true, meterLevel);   //化整间距 
            // 天津偏差化整小数位与基本误差一致
            if (ConfigHelper.Instance.Marketing_Type == "天津MIS接口")
                space = GetWuChaHzzJianJu(false, meterLevel);



            float Windage = Number.GetWindage(data); //计算标准偏差
            Windage = (float)Math.Round(Windage, VerifyConfig.PjzDigit);
            float hz = Number.GetHzz(Windage, space);

            //添加符号
            int hzPrecision = Common.GetPrecision(space.ToString());

            string AvgNumber, HZNumber;
            if (ConfigHelper.Instance.Marketing_Type == "天津MIS接口")
            {
                AvgNumber = AddFlag(Windage, 4).ToString();
                HZNumber = AddFlag(hz, hzPrecision);
                if (Windage > 0 && !HZNumber.StartsWith("+"))
                    HZNumber = $"+{HZNumber}";

            }
            else // 去掉正号
            {
                AvgNumber = AddFlag(Windage, 4).ToString().Replace("+", "");
                HZNumber = AddFlag(hz, hzPrecision).Replace("+", "");
            }


            // 检测是否超过误差限
            ErrorResoult resoult = new ErrorResoult();
            if (Windage >= wcPoint.ErrorXiaXian && Windage <= wcPoint.ErrorShangXian)
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
                    strWuCha += $"{AddFlag(data[i], VerifyConfig.PjzDigit)}|";
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
            //strWuCha = strWuCha.TrimEnd('|');

            resoult.ErrorValue = $"{strWuCha}{AvgNumber}|{HZNumber}";

            return resoult;
        }

        #endregion


        /// <summary>
        /// 初始化检定参数，包括初始化虚拟表单，初始化方案参数，初始化脉冲个数
        /// </summary>
        /// <param name="planList">方案列表</param>
        /// <param name="Pulselap">检定圈数</param>
        private void InitVerifyPara(ref StPlan_WcPoint[] planList, ref int[] Pulselap)
        {
            planList = new StPlan_WcPoint[MeterNumber];
            Pulselap = new int[MeterNumber];
            MessageAdd("开始初始化检定参数...", EnumLogType.提示信息);

            //填充空数据
            MeterHelper.Instance.Init();

            for (int iType = 0; iType < MeterHelper.Instance.TypeCount; iType++)
            {
                //从电能表数据管理器中取每一种规格型号的电能表
                string[] mTypes = MeterHelper.Instance.MeterType(iType);
                int curFirstiType = 0;//当前类型的第一块索引
                for (int i = 0; i < mTypes.Length; i++)
                {
                    if (!Number.IsIntNumber(mTypes[i])) continue;

                    //取当前要检的表号
                    int t = int.Parse(mTypes[i]);
                    TestMeterInfo meter = MeterInfo[t];

                    if (meter.YaoJianYn)
                    {
                        planList[t] = CurPlan;

                        if (VerifyConfig.IsTimeWcLapCount)
                        {
                            planList[t].SetLapCount2(OneMeterInfo.MD_UB, meter.MD_UA, Clfs, planList[t].PowerYuanJian, meter.MD_Constant, planList[t].PowerYinSu, IsYouGong, HGQ, VerifyConfig.WcMinTime);
                        }
                        else
                        {
                            planList[t].SetLapCount(MeterHelper.Instance.MeterConstMin(), meter.MD_Constant, meter.MD_UA, "1.0Ib", CurPlan.LapCount, HGQ);
                        }

                        // 新增判断误差限
                        if (IsPC)
                            planList[t].Pc = 1;
                        else
                            planList[t].Pc = 0;

                        planList[t].SetWcx(meter.MD_JJGC, meter.MD_Grane, HGQ);
                        planList[t].ErrorShangXian *= ErrorProportion;
                        planList[t].ErrorXiaXian *= ErrorProportion;
                        Pulselap[t] = planList[t].LapCount;
                        curFirstiType = t;
                    }
                    else
                    {
                        //不检定表设置为第一块要检定表圈数。便于发放统一检定参数。提高检定效率
                        Pulselap[t] = planList[curFirstiType].LapCount;
                    }
                }
            }

            //重新填充不检的表位
            for (int i = 0; i < MeterNumber; i++)
            {
                //如果有不检的表则直接填充为第一块要检表的圈数
                if (Pulselap[i] == 0)
                {
                    Pulselap[i] = planList[FirstIndex].LapCount;
                }
            }
            MessageAdd("初始化检定参数完毕! ", EnumLogType.提示信息);
        }
    }


    public class ErrorResoult
    {
        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 误差值
        /// </summary>
        public string ErrorValue { get; set; }

    }
}
