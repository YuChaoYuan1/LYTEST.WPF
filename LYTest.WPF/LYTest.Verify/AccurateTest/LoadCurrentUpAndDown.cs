using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Data;

namespace LYTest.Verify.AccurateTest
{
    public class LoadCurrentUpAndDown : VerifyBase
    {
        private StPlan_WcPoint CurPlan = new StPlan_WcPoint();
        /// <summary>
        /// 每一个误差点最多读取多少次误差
        /// </summary>
        private int m_WCMaxTimes;
        readonly string[] XIb = new string[6];
        private readonly int CzQs = 2;//参照圈数
        private readonly int time = 2;

        string[] TempResult;//中间临时结论

        public override void Verify()
        {
            base.Verify();
            TempResult = new string[MeterNumber];
            for (int i = 1; i <= 6; i++)
            {
                //if (Clfs == WireMode.单相)//单相不做Imin(0.1Itr)
                //{
                //    if (i ==1 || i == 6)
                //        continue;
                //}
                CheckOver = false;
                CurPlan.PowerDianLiu = XIb[i - 1];

                //if (Stop) return;
                //CheckPara1(i);
                if (Stop) return;
                Verify2(i);


                if (i == 3)   //上升下降切换
                {
                    if (Stop) return;
                    WaitTime("请稍候", time * 60);
                }
            }


            //处理结论
            MessageAdd("开始处理结论,请稍候....", EnumLogType.提示信息);
            for (int i = 0; i < MeterInfo.Length; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    string Hz1 = ResultDictionary["01Ib上升化整值"][i];
                    string Hz2 = ResultDictionary["01Ib下降化整值"][i];
                    string Hz3 = ResultDictionary["Ib上升化整值"][i];
                    string Hz4 = ResultDictionary["Ib下降化整值"][i];
                    string Hz5 = ResultDictionary["Imax上升化整值"][i];
                    string Hz6 = ResultDictionary["Imax下降化整值"][i];


                    float VarietyErr1 = 100F;
                    float VarietyErr2 = 100F;
                    float VarietyErr3 = 100F;

                    if (!string.IsNullOrEmpty(Hz1) && !string.IsNullOrEmpty(Hz2))
                    {
                        VarietyErr1 = Math.Abs(float.Parse(Hz1) - float.Parse(Hz2));
                        ResultDictionary["01Ib差值"][i] = VarietyErr1.ToString();
                    }
                    if (!string.IsNullOrEmpty(Hz3) && !string.IsNullOrEmpty(Hz4))
                    {
                        VarietyErr2 = Math.Abs(float.Parse(Hz3) - float.Parse(Hz4));
                        ResultDictionary["Ib差值"][i] = VarietyErr2.ToString();
                    }
                    if (!string.IsNullOrEmpty(Hz5) && !string.IsNullOrEmpty(Hz6))
                    {
                        VarietyErr3 = Math.Abs(float.Parse(Hz5) - float.Parse(Hz6));
                        ResultDictionary["Imax差值"][i] = VarietyErr3.ToString();
                    }
                    float tmpLmt = GetErrBianChaLmt(MeterInfo[i]);

                    //if (Clfs == WireMode.单相)
                    //{
                    //    if (VarietyErr2 <= tmpLmt && VarietyErr3 <= tmpLmt && TempResult[i] == ConstHelper.合格)
                    //    {
                    //        ResultDictionary["结论"][i] = ConstHelper.合格;
                    //    }
                    //    else
                    //    {
                    //        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    //        NoResoult[i] = "差值超出误差限";
                    //    }
                    //}
                    //else
                    {
                        if (VarietyErr1 <= tmpLmt && VarietyErr2 <= tmpLmt && VarietyErr3 <= tmpLmt && TempResult[i] == ConstHelper.合格)
                        {
                            ResultDictionary["结论"][i] = ConstHelper.合格;
                        }
                        else
                        {
                            ResultDictionary["结论"][i] = ConstHelper.不合格;
                            NoResoult[i] = "差值超出误差限";
                        }
                    }
                }
            }
            //通知界面
            RefUIData("01Ib差值");
            RefUIData("Ib差值");
            RefUIData("Imax差值");
            RefUIData("结论");
            MessageAdd("检定完成....", EnumLogType.提示信息);
        }
        //TODO:整理误差限
        private float GetErrBianChaLmt(TestMeterInfo meter)
        {
            string[] arr = Number.GetDj(meter.MD_Grane);
            //modify yjt 20220621 修改负载电流升降变差的误差限
            float tmpLmt;
            switch (arr[0])
            {
                case "0.2":
                case "0.2S":
                case "D":
                    tmpLmt = 0.05F;
                    break;
                case "0.5":
                case "0.5S":
                case "C":
                    tmpLmt = 0.12F;
                    break;
                case "1":
                case "1.0":
                case "2":
                case "2.0":
                case "B":
                case "A":
                    tmpLmt = 0.25F;
                    break;
                default:
                    tmpLmt = 0.05F;
                    break;
            }
            return tmpLmt;

            //modify yjt 20220621 修改负载电流升降变差的误差限
            //float tmpLmt = 0;
            //string[] arr = Number.GetDj(meter.MD_Grane);
            //switch (arr[0])
            //{
            //    case "0.2":
            //    case "0.2S":
            //        tmpLmt = 0.05F;
            //        break;
            //    case "0.5":
            //    case "0.5S":
            //        tmpLmt = 0.12F;
            //        break;
            //    case "1":
            //    case "1.0":
            //    case "2":
            //    case "2.0":
            //        tmpLmt = 0.25F;
            //        break;
            //    case "B":
            //        tmpLmt = 0.25F;
            //        break;
            //    case "C":
            //        tmpLmt = 0.12F;
            //        break;
            //    case "D":
            //        tmpLmt = 0.05F;
            //        break;
            //    default:

            //        break;
            //}
            //return tmpLmt;
        }


        /// <summary>
        /// 基本误差和标准偏差误差检定
        /// </summary>
        public void Verify2(int count)
        {
            #region 变量
            int tableHeader = 2;
            DataTable errorTable = new DataTable();                                         //误差值虚表
            StPlan_WcPoint[] arrPlanList = new StPlan_WcPoint[MeterNumber];
            int[] arrPulseLap = new int[MeterNumber];
            string[] c = new string[] { "01Ib", "Ib", "Imax", "Imax", "Ib", "01Ib" };
            int[] _VerifyTimes = new int[MeterNumber];          //有效误差次数
            int[] lastNum = new int[MeterNumber];           //表位取误差次数
            #endregion
            string[] tmpNResult = new string[MeterNumber];

            //初始化参数,带200MS延时
            InitVerifyPara(tableHeader, ref arrPlanList, ref arrPulseLap, errorTable);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary[$"{c[count - 1]}检定圈数"][i] = arrPulseLap[i].ToString();
            }
            RefUIData($"{c[count - 1]}检定圈数");


            if (Stop) return;
            int maxWCnum = tableHeader;                         //最大误差次数
            SetBluetoothModule(GetFangXianIndex(CurPlan.PowerFangXiang));
            //modify yjt 20220822 修改传入参数电流倍数
            //if (!ErrorInitEquipment(CurPlan.PowerFangXiang, CurPlan.PowerYuanJian, CurPlan.PowerYinSu, CurPlan.PowerDianYa, arrPulseLap[0]))
            if (!ErrorInitEquipment(CurPlan.PowerFangXiang, CurPlan.PowerYuanJian, CurPlan.PowerYinSu, CurPlan.PowerDianLiu, arrPulseLap[0]))
            {
                MessageAdd("初始化基本误差设备参数失败", EnumLogType.提示信息);
                if (Stop) return;
            }

            //add yjt 20220621 演示模式
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        float wcx = GetErrBianChaLmt(MeterInfo[i]);
                        float[] tpmWc = BasicErr(wcx, maxWCnum);
                        float meterLevel = MeterLevel(MeterInfo[i]);
                        string[] value = SetWuCha(arrPlanList[i], meterLevel, tpmWc).ErrorValue.Split('|');

                        if (count <= 3)         //上升
                        {
                            ResultDictionary[$"{c[count - 1]}上升误差1"][i] = value[0];
                            ResultDictionary[$"{c[count - 1]}上升误差2"][i] = value[1];
                            ResultDictionary[$"{c[count - 1]}上升平均值"][i] = value[2];
                            ResultDictionary[$"{c[count - 1]}上升化整值"][i] = value[3];
                        }
                        else
                        {
                            ResultDictionary[$"{c[count - 1]}下降误差1"][i] = value[0];
                            ResultDictionary[$"{c[count - 1]}下降误差2"][i] = value[1];
                            ResultDictionary[$"{c[count - 1]}下降平均值"][i] = value[2];
                            ResultDictionary[$"{c[count - 1]}下降化整值"][i] = value[3];

                        }
                        if (string.IsNullOrWhiteSpace(TempResult[i]) || TempResult[i] == ConstHelper.合格)
                        {
                            TempResult[i] = SetWuCha(arrPlanList[i], meterLevel, tpmWc).Result;
                        }
                    }
                }
                RefUIData($"{c[count - 1]}上升误差1");
                RefUIData($"{c[count - 1]}上升误差2");
                RefUIData($"{c[count - 1]}上升平均值");
                RefUIData($"{c[count - 1]}上升化整值");
                RefUIData($"{c[count - 1]}下降误差1");
                RefUIData($"{c[count - 1]}下降误差2");
                RefUIData($"{c[count - 1]}下降平均值");
                RefUIData($"{c[count - 1]}下降化整值");

                return;
            }

            if (Stop) return;
            DateTime startTime = DateTime.Now;          //记录下检定开始时间


            bool[] checkOver = new bool[MeterNumber];
            ErrorResoult[] errorData = new ErrorResoult[MeterNumber];
            StartWcb(GetFangXianIndex(CurPlan.PowerFangXiang), 0xff);
            while (true)
            {
                //强制停止
                if (Stop) return;
                MessageAdd("正在检定...", EnumLogType.提示信息);
                if (CheckOver && !IsMeterDebug)
                {
                    MessageAdd("当前电流点检定结束。", EnumLogType.提示信息);
                    break;
                }
                //在这儿作时间检测。可预防在小电流情况下表老是不发脉冲，超过最大检定时间但是不停止的情况

                int maxSeconds = VerifyConfig.MaxHandleTime * 2;

                if (DateTimes.DateDiff(startTime) > maxSeconds && !IsMeterDebug)
                {
                    MessageAdd($"当前点检定已经超过最大检定时间{maxSeconds}秒！", EnumLogType.提示信息);
                    CheckOver = true;
                    break;
                }

                string[] curWC = new string[MeterNumber];               //重新初始化本次误差
                int[] curNum = new int[MeterNumber];           //重新初始化当前累积误差数

                if (!ReadWc(ref curWC, ref curNum, CurPlan.PowerFangXiang))
                {
                    MessageAdd("读取误差失败!", EnumLogType.提示信息);
                    continue;
                }
                if (Stop) break;

                CheckOver = true;

                #region ------循环表位------
                for (int i = 0; i < MeterNumber; i++)
                {
                    //强制停止
                    if (Stop) return;
                    TestMeterInfo meter = MeterInfo[i];   //表基本信息

                    if (!meter.YaoJianYn)//不检表处理
                        checkOver[i] = true;

                    //已经合格的表不再处理 去掉第一次误差
                    if (checkOver[i] || curNum[i] <= 1) continue;

                    //处理超过255次的情况
                    if (lastNum[i] > 0 && curNum[i] < lastNum[i])
                    {
                        while (lastNum[i] > curNum[i])
                        {
                            curNum[i] += 255;
                        }
                    }

                    //误差次数处理
                    if (lastNum[i] < curNum[i])
                    {
                        lastNum[i] = curNum[i];
                        _VerifyTimes[i]++;  //这个才是真正的误差处理次数
                    }
                    else  //相等时
                    {
                        //检测其它表位有没有出误差，给出相应的提示
                        int[] copy = (int[])_VerifyTimes.Clone();
                        float[] otherWcnum = ArrayConvert.ToSingle(copy);
                        Number.PopDesc(ref otherWcnum, false);
                        if (otherWcnum[0] > maxWCnum * 2 && _VerifyTimes[i] == 0)
                        {
                            MessageAdd($"表位{ i + 1}没有检测到误差,请检查接线", EnumLogType.提示信息);
                        }
                        //误差次数没有增加，则此次误差板数据没有更新
                        if (_VerifyTimes[i] < maxWCnum)
                            CheckOver = false;
                        continue;
                    }

                    if (curNum[i] == 0 || curNum[i] == 255)
                    {
                        CheckOver = false;
                        continue;            //如果本表位没有出误差，换下一表
                    }

                    CurPlan = arrPlanList[i];     //当前检定方案

                    //得到当前表的等级
                    float meterLevel = MeterLevel(meter);                   //当前表的等级

                    //推箱子,最后一次误差排列在最前面
                    if (_VerifyTimes[i] > 1)
                    {
                        for (int j = maxWCnum - 1; j > 0; j--)
                        {
                            errorTable.Rows[i][j] = errorTable.Rows[i][j - 1];
                        }
                    }
                    errorTable.Rows[i][0] = curWC[i];     //最后一次误差始终放在第一位

                    float[] tpmWc = ArrayConvert.ToSingle(errorTable.Rows[i].ItemArray);  //Datable行到数组的转换
                    ErrorResoult tem = SetWuCha(arrPlanList[i], meterLevel, tpmWc);

                    if (Stop) return;
                    //跳差检测
                    if (_VerifyTimes[i] > 1)
                    {
                        string preWc = errorTable.Rows[i][1].ToString();
                        if (Number.IsNumeric(preWc) && Number.IsNumeric(curWC[i]))
                        {
                            float jump = float.Parse(curWC[i]) - float.Parse(preWc);
                            if (Math.Abs(jump) > meterLevel * VerifyConfig.JumpJudgment)
                            {
                                checkOver[i] = false;
                                tem.Result = ConstHelper.不合格;
                                if (_VerifyTimes[i] > m_WCMaxTimes)
                                {
                                    checkOver[i] = true;
                                }
                                else
                                {
                                    MessageAdd($"检测到{i + 1}跳差，重新取误差进行计算", EnumLogType.提示信息);
                                    _VerifyTimes[i] = 1;     //复位误差计算次数到
                                    CheckOver = false;
                                }
                            }
                        }
                    }
                    errorData[i] = tem;
                    if (_VerifyTimes[i] >= maxWCnum)
                    {
                        if (tem.Result != ConstHelper.合格 && !checkOver[i])
                        {
                            if (_VerifyTimes[i] > m_WCMaxTimes)
                            {
                                checkOver[i] = true;
                                MessageAdd(string.Format("第{0}表位超过最大检定次数", i + 1), EnumLogType.提示信息);
                            }
                        }
                        else
                        {
                            checkOver[i] = true;
                        }
                    }
                    else
                    {
                        checkOver[i] = false;
                        MessageAdd($"{i + 1}表位还没有达到检定次数", EnumLogType.提示信息);
                    }

                }
                if (Stop) return;
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    if (!checkOver[j])
                    {
                        //    MessageAdd($"第{j + 1}块表还没有通过", EnumLogType.提示信息);
                        CheckOver = false;
                        break;
                    }
                }

                #endregion


                if (Stop) return;

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (errorData[i] == null) continue;
                    string[] arrayTemp = errorData[i].ErrorValue.Split('|');
                    if (arrayTemp.Length > 3)
                    {
                        if (count <= 3)         //上升
                        {
                            ResultDictionary[$"{c[count - 1]}上升误差1"][i] = arrayTemp[0];
                            ResultDictionary[$"{c[count - 1]}上升误差2"][i] = arrayTemp[1];
                            ResultDictionary[$"{c[count - 1]}上升平均值"][i] = arrayTemp[2];
                            ResultDictionary[$"{c[count - 1]}上升化整值"][i] = arrayTemp[3];
                            RefUIData($"{c[count - 1]}上升误差1");
                            RefUIData($"{c[count - 1]}上升误差2");
                            RefUIData($"{c[count - 1]}上升平均值");
                            RefUIData($"{c[count - 1]}上升化整值");
                        }
                        else
                        {
                            ResultDictionary[$"{c[count - 1]}下降误差1"][i] = arrayTemp[0];
                            ResultDictionary[$"{c[count - 1]}下降误差2"][i] = arrayTemp[1];
                            ResultDictionary[$"{c[count - 1]}下降平均值"][i] = arrayTemp[2];
                            ResultDictionary[$"{c[count - 1]}下降化整值"][i] = arrayTemp[3];
                            RefUIData($"{c[count - 1]}下降误差1");
                            RefUIData($"{c[count - 1]}下降误差2");
                            RefUIData($"{c[count - 1]}下降平均值");
                            RefUIData($"{c[count - 1]}下降化整值");
                        }
                    }
                    float meterLevel = MeterLevel(MeterInfo[i]);
                    float[] fetmp = ArrayConvert.ToSingle(arrayTemp);

                    tmpNResult[i] = SetWuCha(arrPlanList[i], meterLevel, fetmp).Result;
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrWhiteSpace(TempResult[i]) || TempResult[i] == ConstHelper.合格)
                {
                    TempResult[i] = tmpNResult[i];
                }
            }
            StopWcb(GetFangXianIndex(CurPlan.PowerFangXiang), 0xff);//停止误差板
            if (Stop) return;
            //PowerOn();
            //WaitTime("正在关闭电流", 5);
        }



        protected override bool CheckPara()
        {
            string[] tem = Test_Value.Split('|');
            for (int i = 0; i < 3; i++)
            {
                XIb[i] = tem[i];
                XIb[i + 3] = tem[2 - i];
            }
            //for (int i = 0; i < 3; i++)
            //{
            //    XIb[i] = tem[i];
            //}



            m_WCMaxTimes = VerifyConfig.ErrorMax;
            CurPlan.PowerYinSu = "1.0";

            CurPlan.PowerFangXiang = PowerWay.正向有功;
            CurPlan.PowerYuanJian = Cus_PowerYuanJian.H;

            ResultNames = new string[] { "01Ib检定圈数", "01Ib上升误差1", "01Ib上升误差2", "01Ib上升平均值", "01Ib上升化整值", "01Ib下降误差1", "01Ib下降误差2", "01Ib下降平均值", "01Ib下降化整值", "01Ib差值", "Ib检定圈数", "Ib上升误差1", "Ib上升误差2", "Ib上升平均值", "Ib上升化整值", "Ib下降误差1", "Ib下降误差2", "Ib下降平均值", "Ib下降化整值", "Ib差值", "Imax检定圈数", "Imax上升误差1", "Imax上升误差2", "Imax上升平均值", "Imax上升化整值", "Imax下降误差1", "Imax下降误差2", "Imax下降平均值", "Imax下降化整值", "Imax差值", "结论" };
            return true;
        }





        #region ----------参数初始化InitVerifyPara----------
        /// <summary>
        /// 初始化检定参数，包括初始化虚拟表单，初始化方案参数，初始化脉冲个数
        /// </summary>
        /// <param name="tableHeader">表头数量</param>
        /// <param name="planList">方案列表</param>
        /// <param name="Pulselap">检定圈数</param>
        /// <param name="DT">虚表</param>
        private void InitVerifyPara(int tableHeader, ref StPlan_WcPoint[] planList, ref int[] Pulselap, DataTable DT)
        {
            planList = new StPlan_WcPoint[MeterNumber];
            Pulselap = new int[MeterNumber];
            MessageAdd("开始初始化检定参数...", EnumLogType.提示信息);
            //初始化虚表头
            for (int i = 0; i < tableHeader; i++)
            {
                DT.Columns.Add("WC" + i.ToString());
            }

            MeterHelper.Instance.Init();
            for (int iType = 0; iType < MeterHelper.Instance.TypeCount; iType++)
            {
                //从电能表数据管理器中取每一种规格型号的电能表
                string[] arrCurTypeBw = MeterHelper.Instance.MeterType(iType);
                int curFirstiType = 0;//当前类型的第一块索引
                for (int i = 0; i < arrCurTypeBw.Length; i++)
                {
                    if (!Number.IsIntNumber(arrCurTypeBw[i]))
                        continue;
                    //取当前要检的表号
                    int index = int.Parse(arrCurTypeBw[i]);
                    //填充空数据
                    //得到当前表的基本信息
                    TestMeterInfo meter = MeterInfo[index];
                    if (meter.YaoJianYn)
                    {
                        planList[index] = CurPlan;
                        if (VerifyConfig.IsTimeWcLapCount)
                        {
                            planList[index].SetLapCount2(OneMeterInfo.MD_UB, meter.MD_UA, Clfs, planList[index].PowerYuanJian, meter.MD_Constant, planList[index].PowerYinSu, IsYouGong, HGQ, VerifyConfig.WcMinTime);
                        }
                        else
                        {
                            planList[index].SetLapCount(MeterHelper.Instance.MeterConstMin(), meter.MD_Constant, meter.MD_UA, "1.0Ib", CzQs, HGQ);
                        }
                        planList[index].SetWcx( meter.MD_JJGC, meter.MD_Grane, HGQ);
                        planList[index].ErrorShangXian *= 1f;
                        planList[index].ErrorXiaXian *= 1f;
                        Pulselap[index] = planList[index].LapCount;
                        curFirstiType = index;
                    }
                    else
                    {
                        //不检定表设置为第一块要检定表圈数。便于发放统一检定参数。提高检定效率
                        Pulselap[index] = planList[curFirstiType].LapCount;
                    }

                }
            }
            //重新填充不检的表位
            for (int i = 0; i < MeterNumber; i++)             //这个地方创建虚表行，多少表位创建多少行！！
            {
                DT.Rows.Add(new string[(tableHeader - 1)]);
                //如果有不检的表则直接填充为第一块要检表的圈数
                if (Pulselap[i] == 0)
                {
                    Pulselap[i] = planList[FirstIndex].LapCount;
                }
            }

            MessageAdd("初始化检定参数完毕! ", EnumLogType.提示信息);
        }


        ///// <summary>
        ///// 初始化设备参数,计算每一块表需要检定的圈数
        ///// </summary>
        //private bool InitEquipment(StPlan_WcPoint point, int[] pulselap)
        //{
        //    if (IsDemo) return true;
        //    bool isP = (point.PowerFangXiang == PowerWay.正向有功 || point.PowerFangXiang == PowerWay.反向有功);
        //    int[] meterconst = MeterHelper.Instance.MeterConst(isP);
        //    //int constants = (point.PowerFangXiang == PowerWay.正向有功 || point.PowerFangXiang == PowerWay.反向有功) ? MeterHelper.Instance.MeterConstMin()[0] :
        //    //    MeterHelper.Instance.MeterConstMin()[1];
        //    float xIb = Number.GetCurrentByIb(point.PowerDianLiu, OneMeterInfo.MD_UA, HGQ);

        //    MessageAdd("正在升源...", EnumLogType.提示信息);
        //    if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, point.PowerYuanJian, point.PowerFangXiang, point.PowerYinSu))
        //    {
        //        MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
        //        return false;
        //    }
        //    if (Stop) return true;
        //    ulong StdConst = GetStaConst();

        //    MessageAdd("正在设置标准表脉冲...", EnumLogType.提示信息);
        //    int index = 0;
        //    if (point.PowerFangXiang == PowerWay.反向无功 || point.PowerFangXiang == PowerWay.正向无功)
        //    {
        //        index = 1;
        //    }
        //    SetPulseType((index + 49).ToString("x"));


        //    MessageAdd("开始初始化基本误差检定参数!", EnumLogType.提示信息);


        //    //设置误差版被检常数
        //    MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
        //    SetStandardConst(0, (int)(StdConst / 100), -2, 0xff);
        //    //设置误差版标准常数 TODO
        //    MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
        //    if (!SetTestedConst(index, meterconst, 0, pulselap))
        //    {
        //        MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
        //        return false;
        //    }
        //    return true;
        //}
        #endregion

        #region   计算基本误差
        /// <summary>
        /// 计算基本误差
        /// </summary>
        /// <param name="data">要参与计算的误差数组</param>
        /// <returns></returns>
        public ErrorResoult SetWuCha(StPlan_WcPoint wcPoint, float meterLevel, float[] data)
        {
            ErrorResoult resoult = new ErrorResoult();
            //取平均值修约精度 
            float intSpace = GetWuChaHzzJianJu(false, meterLevel);                              //化整间距 
            float AvgWuCha = Number.GetAvgA(data);               //平均值
            float HzzWuCha = Number.GetHzz(AvgWuCha, intSpace);       //化整值
            string AvgNumber;
            string HZNumber;
            //添加符号
            int hzPrecision = Common.GetPrecision(intSpace.ToString());

            AvgNumber = AddFlag(AvgWuCha, VerifyConfig.PjzDigit).ToString();
            HZNumber = AddFlag(HzzWuCha, hzPrecision);

            // 检测是否超过误差限
            if (AvgWuCha >= wcPoint.ErrorXiaXian &&
                AvgWuCha <= wcPoint.ErrorShangXian)
            {
                resoult.Result = ConstHelper.合格;
            }
            else
            {
                resoult.Result = ConstHelper.不合格;
            }

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
    }
}
