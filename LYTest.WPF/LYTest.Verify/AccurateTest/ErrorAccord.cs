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
    /// <summary>
    /// 误差一致性
    /// </summary>
    public class ErrorAccord : VerifyBase
    {
        StPlan_WcPoint CurPlan = new StPlan_WcPoint();
        /// <summary>
        /// 每一个误差点最多读取多少次误差
        /// </summary>
        private int m_WCMaxTimes;
        private readonly int CzQs = 2;//参照圈数

        //private int time = 1;

        ErrorResoult[] errorData;

        public override void Verify()
        {
            base.Verify();

            //modify yjt 20220610 误差上下限
            float Wcx = GetErrBianChaLmt(OneMeterInfo, CurPlan);     //误差限
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                ResultDictionary["误差上限"][i] = Wcx.ToString();
                ResultDictionary["误差下限"][i] = (-Wcx).ToString();
            }
            RefUIData("误差上限");
            RefUIData("误差下限");

            CheckOver = false;

            if (Stop) return;
            errorData = new ErrorResoult[MeterNumber];
            Verify2();
            //WaitTime("请稍候", time * 5);

            MessageAdd("正在处理结论,请稍候....", EnumLogType.提示信息);
            float[] AvgErr = new float[MeterNumber];   //所有表4次误差的平均值
            int avrNum = 0;//次数
            float Sum = 0f; ;    //和
            if (Stop) return;

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                string str = ResultDictionary["平均值"][i];
                if (string.IsNullOrEmpty(str)) continue;
                AvgErr[i] = float.Parse(str);  //每个表位的平均值
                Sum += AvgErr[i];   //每个表位的平均值
                avrNum++;
            }
            float YP_Avg = Sum / avrNum;

            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    float VarietyErr = Math.Abs(AvgErr[i] - YP_Avg);
                    ResultDictionary["差值"][i] = VarietyErr.ToString("F3");

                    if (VarietyErr <= Wcx && errorData[i].Result == ConstHelper.合格)
                    {
                        ResultDictionary["结论"][i] = ConstHelper.合格;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        NoResoult[i] = "差值超出误差限";
                    }
                    ResultDictionary["样品均值"][i] = YP_Avg.ToString("F3");
                }
            }
            //PowerOn();
            //WaitTime("正在关闭电流", 5);
            //通知界面
            RefUIData("样品均值");
            RefUIData("差值");
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);

        }


        /// <summary>
        /// 基本误差和标准偏差误差检定
        /// </summary>
        public void Verify2()
        {
            #region 变量
            int tableHeader = 2;
            DataTable errorTable = new DataTable();                                         //误差值虚表
            StPlan_WcPoint[] arrPlanList = new StPlan_WcPoint[MeterNumber];
            int[] arrPulseLap = new int[MeterNumber];

            int[] _VerifyTimes = new int[MeterNumber];          //有效误差次数
            int[] lastNum = new int[MeterNumber];           //表位取误差次数
            #endregion


            //初始化参数,带200MS延时
            InitVerifyPara(tableHeader, ref arrPlanList, ref arrPulseLap, errorTable);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定圈数"][i] = arrPulseLap[i].ToString();
            }

            RefUIData("检定圈数");

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
                float Wcx = GetErrBianChaLmt(OneMeterInfo, CurPlan);     //误差限

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        float[] tpmWc = BasicErr(Wcx, maxWCnum);
                        float meterLevel = MeterLevel(MeterInfo[i]);
                       
                        string[] value = SetWuCha(arrPlanList[i], meterLevel, tpmWc).ErrorValue.Split('|');

                        ResultDictionary["误差1"][i] = value[0];
                        ResultDictionary["误差2"][i] = value[1];
                        ResultDictionary["平均值"][i] = value[2];
                        ResultDictionary["化整值"][i] = value[3];

                        errorData[i] = SetWuCha(arrPlanList[i], meterLevel, tpmWc);
                    }
                }
                RefUIData("误差1");
                RefUIData("误差2");
                RefUIData("平均值");
                RefUIData("化整值");

                return;
            }

            if (Stop) return;
            DateTime startTime = DateTime.Now;          //记录下检定开始时间


            bool[] checkOver = new bool[MeterNumber];

            StartWcb(GetFangXianIndex(CurPlan.PowerFangXiang), 0xff);
            float xIb = Number.GetCurrentByIb(CurPlan.PowerDianLiu, OneMeterInfo.MD_UA, HGQ);
            int maxSeconds = (maxWCnum + 2) * arrPulseLap[0] * OnePulseNeedTime(IsYouGong, 1000 * 60 / CalculatePower(OneMeterInfo.MD_UB, xIb, Clfs, CurPlan.PowerYuanJian, CurPlan.PowerYinSu, IsYouGong)) + ((maxWCnum + 3) * 5000);
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
                        MessageAdd($"第{j + 1}块表还没有通过", EnumLogType.提示信息);
                        CheckOver = false;
                        break;
                    }
                }

                #endregion


                if (Stop) return;
                //string[] c = new string[] { "检定点1", "检定点2", "检定点3", "检定点4"};
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (errorData[i] == null) continue;
                    string[] arrayTemp = errorData[i].ErrorValue.Split('|');
                    if (arrayTemp.Length > 3)
                    {

                        ResultDictionary[$"误差1"][i] = arrayTemp[0];
                        ResultDictionary[$"误差2"][i] = arrayTemp[1];
                        ResultDictionary[$"平均值"][i] = arrayTemp[2];
                        ResultDictionary[$"化整值"][i] = arrayTemp[3];
                        RefUIData($"误差1");
                        RefUIData($"误差2");
                        RefUIData($"平均值");
                        RefUIData($"化整值");

                    }
                }
            }
            StopWcb(GetFangXianIndex(CurPlan.PowerFangXiang), 0xff);//停止误差板
        }




        protected override bool CheckPara()
        {
            string[] tem = Test_Value.Split('|');

            //ErrorData[0] = new ErrorAccordData(tem[0], tem[2]);
            //ErrorData[1] = new ErrorAccordData(tem[0], tem[3]);
            //ErrorData[2] = new ErrorAccordData(tem[1], tem[2]);
            //ErrorData[3] = new ErrorAccordData(tem[1], tem[3]);

            CurPlan.PowerDianLiu = tem[0];
            CurPlan.PowerYinSu = tem[1];
            CurPlan.PowerFangXiang = PowerWay.正向有功;
            CurPlan.PowerYuanJian = Cus_PowerYuanJian.H;
            m_WCMaxTimes = VerifyConfig.ErrorMax;
            ResultNames = new string[] { "检定圈数", "误差上限", "误差下限", "误差1", "误差2", "平均值", "化整值", "样品均值", "差值", "结论" };
            //ResultNames = new string[] {"检定点1误差1", "检定点1误差2", "检定点1平均值", "检定点1化整值",
            //    "检定点2误差1", "检定点2误差2", "检定点2平均值", "检定点2化整值",
            //    "检定点3误差1", "检定点3误差2", "检定点3平均值", "检定点3化整值",
            //    "检定点4误差1", "检定点4误差2", "检定点4平均值", "检定点4化整值",
            //    "样品均值", "差值", "结论" };

            //检定点1
            //检定点1误差1|检定点1误差2|检定点1平均值|检定点1化整值|检定点2误差1|检定点2误差2|检定点2平均值|检定点2化整值|检定点3误差1|检定点3误差2|检定点3平均值|检定点3化整值|检定点4误差1|检定点4误差2|检定点4平均值|检定点4化整值|样品均值|差值|结论
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
            //上报数据参数
            planList = new StPlan_WcPoint[MeterNumber];
            Pulselap = new int[MeterNumber];
            MessageAdd("开始初始化检定参数...", EnumLogType.提示信息);
            //初始化虚表头
            for (int i = 0; i < tableHeader; i++)
            {
                DT.Columns.Add("WC" + i.ToString());
            }
            //填充空数据
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
                    //得到当前表的基本信息
                  TestMeterInfo  meter = MeterInfo[index];
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
                        /*                       try
                                               {
                                                   planList[index].SetLapCount2(OneMeterInfo.MD_UB, meter.MD_UA, Clfs, planList[index].PowerYuanJian, meter.MD_Constant, planList[index].PowerYinSu, IsYouGong,HGQ);
                                               }
                                               catch (Exception)
                                               {
                                                   planList[index].SetLapCount(MeterHelper.Instance.MeterConstMin(), meter.MD_Constant, meter.MD_UA, "1.0Ib", CzQs);
                                               }*/
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
            for (int i = 0; i < MeterNumber; i++)
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

        ///// <summary>
        ///// 修正数字加+-号
        ///// </summary>
        ///// <param name="data">要修正的数字</param>
        ///// <param name="Priecision">修正精度</param>
        ///// <returns>返回指定精度的带+-号的字符串</returns>
        //private string AddFlag(float data, int Priecision)
        //{
        //    string v = data.ToString(string.Format("F{0}", Priecision));
        //    return AddFlag(v);
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

        //TODO:整理变差限
        private float GetErrBianChaLmt(TestMeterInfo meter, StPlan_WcPoint curPoint)
        {
            string[] dj = Number.GetDj(meter.MD_Grane);
            //modify yjt 20220621 修改一致性的误差限
            float limit;
            switch (dj[0])
            {
                case "0.2":
                case "0.2S":
                case "D":
                    {
                        if (curPoint.PowerDianLiu == "1.0Ib" || curPoint.PowerDianLiu == "10Itr")
                            limit = 0.06F;
                        else
                            limit = 0.08F;
                    }
                    break;
                case "0.5":
                case "0.5S":
                case "C":
                    {
                        if (curPoint.PowerDianLiu == "1.0Ib" || curPoint.PowerDianLiu == "10Itr")
                            limit = 0.15F;
                        else
                            limit = 0.2F;
                    }
                    break;
                case "1":
                case "1.0":
                case "2":
                case "2.0":
                case "B":
                case "A":
                    {
                        if (curPoint.PowerDianLiu == "1.0Ib" || curPoint.PowerDianLiu == "10Itr")
                            limit = 0.3F;
                        else
                            limit = 0.4F;
                    }
                    break;
                default:
                    limit = 0.06F;
                    break;
            }
            return limit;
        }

    }

}
