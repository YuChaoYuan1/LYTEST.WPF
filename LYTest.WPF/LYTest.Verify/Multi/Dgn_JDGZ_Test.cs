using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.Verify.AccurateTest;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 接地故障
    /// </summary>
    public class Dgn_JDGZ_Test : VerifyBase
    {
        private float ErrorProportion = 1f;  //误差限倍数
        private StPlan_WcPoint CurPlan;

        protected override bool CheckPara()
        {
            //误差试验类型|功率方向|功率元件|功率因素|电流倍数|添加谐波|逆相序|误差圈数|误差限倍数(%)
            if (string.IsNullOrEmpty(Test_Value) || Test_Value.Split('|').Length < 5)
            {
                MessageAdd("基本误差参数错误", EnumLogType.错误信息);
                return false;
            }

            string[] arr = Test_Value.Split('|');

            ErrorProportion = 1;
            if (VerifyConfig.AreaName == "北京" && VerifyConfig.ErrorRatio != "")   //北京流水线误差限是60%
            {
                ErrorProportion *= (Convert.ToInt32(VerifyConfig.ErrorRatio) / 100F);
            }

            CurPlan.PrjID = "111010700";
            CurPlan.IsCheck = true;
            CurPlan.LapCount = 2;
            CurPlan.Dif_Err_Flag = 0;
            CurPlan.nCheckOrder = 1;
            CurPlan.Pc = 0;
            CurPlan.PointId = 1;
            CurPlan.PowerDianLiu = arr[1];
            CurPlan.PowerFangXiang = PowerWay.正向有功;
            CurPlan.PowerYinSu = arr[2];
            CurPlan.PowerYuanJian = Cus_PowerYuanJian.H;
            CurPlan.XiangXu = 0;
            CurPlan.XieBo = 0;
            //N误差1|N误差2|N化整值|N变差|A误差1|A误差2|A化整值|A变差|B误差1|B误差2|B化整值|B变差|C误差1|C误差2|C化整值|C变差
            ResultNames = new string[] { "误差上限", "误差下限", "N误差1", "N误差2", "N化整值", "N变差", "A误差1", "A误差2", "A化整值", "A变差", "B误差1", "B误差2", "B化整值", "B变差", "C误差1", "C误差2", "C化整值", "C变差", "结论" };

            return true;
        }

        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {
            base.Verify();

            string[] values = Test_Value.Split('|');
            if (values.Length < 5)
            {
                MessageAdd("方案参数错误，请重新配置方案后继续！", EnumLogType.错误信息);
                return;
            }

            //bool[] YJMeter = new bool[MeterNumber];
            bool[] bBiaoweiBz = new bool[MeterNumber];
            bBiaoweiBz.Fill(false);
            //string jdgzbw = "3,5,6";
            string jdgzbw = "1,2,3,4";

            string[] bws = jdgzbw.Split(','); //选中表位
            foreach (string s in bws)
            {
                int bw = Convert.ToInt32(s) - 1;
                bBiaoweiBz[bw] = true;
            }

            string[] curWC = new string[MeterNumber];   //当前误差
            int[] curNum = new int[MeterNumber];        //当前累计检定次数,是指误差板从启动开始到目前共产生了多少次误差

            int maxWCnum = VerifyConfig.ErrorCount;
            int[] arrPulseLap = new int[MeterNumber];                                           //检定圈数
            float[] arrStand = new float[MeterNumber];


            //3、读误差
            StPlan_WcPoint CurPlan = new StPlan_WcPoint
            {
                PowerDianLiu = values[1],
                PowerFangXiang = PowerWay.正向有功,
                PowerYinSu = "1.0",
                PowerYuanJian = Cus_PowerYuanJian.H,
                XiangXu = 0,
                XieBo = 0
            };

            int bwIndex = 3;
            if (Stop) return;
            float limit = GetErrBianChaLmt(OneMeterInfo);     //误差限
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn && bBiaoweiBz[i])
                {
                    bwIndex = i + 1;
                    ResultDictionary["误差上限"][i] = limit.ToString();
                    ResultDictionary["误差下限"][i] = (-limit).ToString();
                    break;
                }
            }

            RefUIData("误差上限");
            RefUIData("误差下限");

            StPlan_WcPoint[] arrPlanList = new StPlan_WcPoint[MeterNumber];                             //保存表方案
            InitVerifyPara(ref arrPlanList, ref arrPulseLap);

            for (int n = 0; n < 4; n++)
            {
                PowerOff();
                Thread.Sleep(5000);
                if (Stop) return;

                string yj;
                if (n == 1)//ABC 三相的选择
                {
                    yj = "A";
                    SetJDGZContrnl(bwIndex, 1, 0, 0, 0);
                }
                else if (n == 2)
                {
                    yj = "B";
                    SetJDGZContrnl(bwIndex, 0, 1, 0, 0);
                }
                else if (n == 3)
                {
                    yj = "C";
                    SetJDGZContrnl(bwIndex, 0, 0, 1, 0);
                }
                else
                {
                    yj = "N";
                    SetJDGZContrnl(bwIndex, 0, 0, 0, 1);
                }

                Thread.Sleep(5000);

                if (Stop) return;
                if (n == 0)
                {
                    PowerOn();//回复电压
                    Thread.Sleep(5000);
                }
                else
                {
                    float UPara = float.Parse(values[0].Replace("%", "")) / 100f;
                    bool hgq = OneMeterInfo.MD_ConnectionFlag != "直接式";

                    float fIb = Number.GetCurrentByIb(values[1], OneMeterInfo.MD_UA, hgq);

                    PowerOn(OneMeterInfo.MD_UB * UPara, OneMeterInfo.MD_UB * UPara, OneMeterInfo.MD_UB * UPara, fIb, fIb, fIb, Cus_PowerYuanJian.H, PowerWay.正向有功, values[2]);


                    int maxTime = (int)float.Parse(values[3]) * 60;
                    if (maxTime < 60) maxTime = 60;
                    WaitTime("接地故障", maxTime);

                    if (Stop) return;

                    PowerOff();//回复电压
                    Thread.Sleep(5000);
                    SetJDGZContrnl(bwIndex, 0, 0, 0, 1);


                    if (Stop) return;
                    maxTime = (int)float.Parse(values[4]) * 60;
                    if (maxTime < 60) maxTime = 60;

                    WaitTime("停电恢复", maxTime);

                    Thread.Sleep(5000);
                }

                Thread.Sleep(1000);
                //初始化设备
                if (!ErrorInitEquipment(CurPlan.PowerFangXiang, CurPlan.PowerYuanJian, CurPlan.PowerYinSu, CurPlan.PowerDianLiu, arrPulseLap[0]))
                    MessageAdd("初始化基本误差设备参数失败", EnumLogType.错误信息);
                int[] lastNum = new int[MeterNumber];                   //保存上一次误差的序号
                lastNum.Fill(-1);
                List<string>[] errList = new List<string>[MeterNumber]; //记录当前误差[数组长度，]
                for (int i = 0; i < MeterNumber; i++)
                    errList[i] = new List<string>();

                MessageAdd("正在启动误差版...", EnumLogType.提示信息);
                if (!StartWcb(GetFangXianIndex(CurPlan.PowerFangXiang), 0xff))
                {
                    MessageAdd("误差板启动失败...", EnumLogType.错误信息);
                    return;
                }
                MessageAdd("开始检定...", EnumLogType.提示信息);
                DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时

                int MaxTime = VerifyConfig.MaxHandleTime * 1000;
                if (OneMeterInfo.MD_JJGC == "IR46")
                {
                    if (OneMeterInfo.MD_UA.IndexOf("Imin") != -1)
                    {
                        if (HGQ)
                            MaxTime *= 6;
                        else
                            MaxTime *= 2;
                    }
                    else if (CurPlan.PowerDianLiu.IndexOf("0.0") != -1)
                    {
                        MaxTime *= 4;
                    }
                }


                //启动误差版
                while (true)
                {
                    if (Stop) break;
                    if (DateTime.Now.Subtract(TmpTime1).TotalMilliseconds > MaxTime && !IsMeterDebug) //超出最大处理时间并且不是调表状态
                    {
                        //NoResoult.Fill("超出最大处理时间");
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.错误信息);
                        break;
                    }
                    curWC = new string[MeterNumber];   //重新初始化本次误差
                    curNum = new int[MeterNumber];        //当前读取的误差序号
                    curWC.Fill("");
                    curNum.Fill(0);
                    if (!ReadWc(ref curWC, ref curNum, CurPlan.PowerFangXiang))    //读取误差
                    {
                        continue;
                    }
                    if (Stop) break;


                    bool[] arrCheckOver = new bool[MeterNumber];
                    //依次处理每个表位的误差数据
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) arrCheckOver[i] = true;     //表位不要检
                        if (arrCheckOver[i] && !IsMeterDebug) continue;   //表位检定通过了
                        if (lastNum[i] >= curNum[i]) continue;
                        if (string.IsNullOrEmpty(curWC[i])) continue;
                        if (curNum[i] <= VerifyConfig.ErrorStartCount) continue; //当前误差次数小于去除的个数

                        int[] WCNumner = new int[MeterNumber]; //检定次数
                        if (curNum[i] > lastNum[i]) //大于上一次误差次数
                        {
                            WCNumner[i]++;   //检定次数

                            lastNum[i] = curNum[i];
                        }
                        errList[i].Insert(0, curWC[i]);
                        if (errList[i].Count > maxWCnum)
                            errList[i].RemoveAt(errList[i].Count - 1);
                        float meterLevel = MeterLevel(MeterInfo[i]);

                        //计算误差
                        float[] tpmWc = ArrayConvert.ToSingle(errList[i].ToArray());  //Datable行到数组的转换

                        ErrorResoult tem = SetWuCha(meterLevel, tpmWc);

                        if (errList[i].Count >= maxWCnum)  //误差数量>=需要的最大误差数2
                        {
                            arrCheckOver[i] = true;
                            if (tem.Result != ConstHelper.合格 && WCNumner[i] <= VerifyConfig.ErrorMax)
                            {
                                arrCheckOver[i] = false;
                            }
                        }
                        else
                        {
                            arrCheckOver[i] = false;
                            tem.Result = ConstHelper.不合格;
                        }

                        ErrorResoult[] errorResoults = new ErrorResoult[MeterNumber];
                        errorResoults[i] = tem;

                        string[] value = errorResoults[i].ErrorValue.Split('|');
                        ResultDictionary[yj + "误差1"][i] = value[0].ToString();
                        RefUIData(yj + "误差1");

                        if (value.Length > 3)
                        {
                            if (value[1].ToString().Trim() != "")
                            {
                                ResultDictionary[yj + "误差2"][i] = value[0].ToString();
                                RefUIData(yj + "误差2");
                                ResultDictionary[yj + "误差1"][i] = value[1].ToString();
                                RefUIData(yj + "误差1");

                                //跳差判断
                                if (CheckJumpError(ResultDictionary[yj + "误差1"][i], ResultDictionary[yj + "误差2"][i], meterLevel, VerifyConfig.JumpJudgment))
                                {
                                    arrCheckOver[i] = false;
                                    if (WCNumner[i] > VerifyConfig.ErrorMax)
                                        arrCheckOver[i] = true;
                                    else
                                    {
                                        MessageAdd($"检测到{i + 1}跳差，重新取误差进行计算", EnumLogType.提示信息);
                                    }
                                }
                                else
                                {
                                    arrCheckOver[i] = true;
                                }
                            }

                        }
                        if (value.Length > 3)
                        {
                            //ResultDictionary[yj + "平均值"][i] = value[2];
                            ResultDictionary[yj + "化整值"][i] = value[3];

                            if (n == 0)
                            {

                                arrStand[i] = float.Parse(value[3]);
                                ResultDictionary[yj + "变差"][i] = arrStand[i].ToString("F2");
                            }
                            else
                            {
                                float fVarint = Math.Abs(arrStand[i] - float.Parse(value[3]));
                                ResultDictionary[yj + "变差"][i] = fVarint.ToString("F2");

                                if (fVarint > limit)
                                {
                                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                                    RefUIData("结论");
                                    break;
                                }
                            }
                            ResultDictionary["结论"][i] = errorResoults[i].Result;
                        }

                    }
                    RefUIData(yj + "化整值");
                    RefUIData(yj + "变差");
                    if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)  //全部都为true了
                        break;
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检
                    if (ResultDictionary[yj + "变差"][i] == null || ResultDictionary[yj + "变差"][i].Trim() == "" || float.Parse(ResultDictionary[yj + "变差"][i]) > limit)
                    {
                        ResultDictionary[$"结论"][i] = ConstHelper.不合格;
                    }
                }
                RefUIData("结论");
                StopWcb(GetFangXianIndex(CurPlan.PowerFangXiang), 0xff);//停止误差板

            }
            Thread.Sleep(5000);
            SetJDGZContrnl(bwIndex, 0, 0, 0, 1);

            MessageAdd(string.Format("接地故障测试试验完毕"), EnumLogType.提示与流程信息);
        }


        /// <summary>
        /// 计算基本误差
        /// </summary>
        /// <param name="data">要参与计算的误差数组</param>
        /// <returns></returns>
        public ErrorResoult SetWuCha(float meterLevel, float[] data)
        {
            float space = GetWuChaHzzJianJu(false, meterLevel);                              //化整间距 
            float avg = Number.GetAvgA(data);
            float hz = Number.GetHzz(avg, space);

            //添加符号
            int hzPrecision = Common.GetPrecision(space.ToString());
            string AvgNumber = AddFlag(avg, VerifyConfig.PjzDigit).ToString();

            string HZNumber = hz.ToString($"F{hzPrecision}");
            if (hz != 0f) //化整值为0时，不加正负号
                HZNumber = AddFlag(hz, hzPrecision);

            if (avg < 0) HZNumber = HZNumber.Replace('+', '-'); //平均值<0时，化整值需为负数

            // 检测是否超过误差限
            ErrorResoult resoult = new ErrorResoult();
            if (Math.Abs(avg) <= meterLevel)
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

            strWuCha += $"{AvgNumber}|{HZNumber}";
            resoult.ErrorValue = strWuCha;

            return resoult;
        }


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
                        bool Hgq = true;
                        if (meter.MD_ConnectionFlag == "直接式")
                        {
                            Hgq = false;
                        }
                        if (VerifyConfig.IsTimeWcLapCount)
                        {
                            planList[t].SetLapCount2(OneMeterInfo.MD_UB, meter.MD_UA, Clfs, planList[t].PowerYuanJian, meter.MD_Constant, planList[t].PowerYinSu, IsYouGong, HGQ, VerifyConfig.WcMinTime);
                        }
                        else
                        {
                            planList[t].SetLapCount(MeterHelper.Instance.MeterConstMin(), meter.MD_Constant, meter.MD_UA, "1.0Ib", CurPlan.LapCount, HGQ);
                        }

                        planList[t].Pc = 0;
                        planList[t].SetWcx(meter.MD_JJGC, meter.MD_Grane, Hgq);
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
            for (int i = 0; i < MeterNumber; i++)             //这个地方创建虚表行，多少表位创建多少行！！
            {
                //如果有不检的表则直接填充为第一块要检表的圈数
                if (Pulselap[i] == 0)
                {
                    Pulselap[i] = planList[FirstIndex].LapCount;
                }
            }
            MessageAdd("初始化检定参数完毕! ", EnumLogType.提示信息);
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

        private float GetErrBianChaLmt(TestMeterInfo meter)
        {
            string[] dj = Number.GetDj(meter.MD_Grane);
            float limit;
            switch (dj[0])
            {
                case "0.2":
                case "0.2S":
                case "D":
                    {
                        limit = 0.1F;
                    }
                    break;
                case "0.5":
                case "0.5S":
                case "C":
                    {
                        limit = 0.3F;
                    }
                    break;
                case "1":
                case "1.0":
                case "B":
                    {
                        limit = 0.7F;
                    }
                    break;
                default:
                    limit = 0.1F;
                    break;
            }
            return limit;
        }
    }
}
