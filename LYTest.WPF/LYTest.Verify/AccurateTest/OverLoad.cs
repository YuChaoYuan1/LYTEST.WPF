using LYTest.Core;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Data;
using System.Threading;

namespace LYTest.Verify.AccurateTest
{
    //  ///add lsj 20220218 电流过载实验
    /// <summary>
    /// 电流过载
    /// </summary>
    class OverLoad : VerifyBase
    {
        StPlan_WcPoint CurPlan = new StPlan_WcPoint();
        /// <summary>
        /// 每一个误差点取几次误差参与计算
        /// </summary>
        private readonly int m_WCTimesPerPoint=0;
        /// <summary>
        /// 每一个误差点最多读取多少次误差
        /// </summary>
        private readonly int m_WCMaxTimes = 0;
        private readonly int CzQs = 2;//参照圈数
        //检定入口
        public override void Verify()
        {
            //过载时间T1，对10Ib点检定 -> 恢复时间T2 -> 对1.0Ib点检定
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["功率方向"][i] = "正向有功";
                ResultDictionary["功率元件"][i] = "H";
                ResultDictionary["功率因数"][i] = "1.0";
            }
            RefUIData("功率方向");
            RefUIData("功率元件");
            RefUIData("功率因数");
            for (int i = 0; i < 2; i++)
            {
                //强制停止
                if (Stop) return;
                //遍历检定点
                CheckOver = false;


                //参数
                Verify2(i, true);
            }
        }

        /// <summary>
        /// 误差一致性检定器2
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isSecondTime"></param>
        /// <param name="isLastPoint">是否是最后一次</param>
        /// <param name="index">误差点的序号</param>
        public void Verify2(int index, bool isLastPoint)
        {
            #region ----------变量声明----------
            //string value = Test_Value;
            //更新当前检定功率方向
            //PowerWay PowerWay = Core.Enum.PowerWay.正向无功;
            //更新当前检定ID
            base.Verify();
            //保存表方案
            //检定圈数
            int[] _VerifyTimes = new int[MeterNumber];          //有效误差次数
            int[] lastNum = new int[MeterNumber];           //表位取误差次数

            #endregion

            //初始化参数,带200MS延时
            DataTable _DT = new DataTable();
            int maxWcNum = m_WCTimesPerPoint; //误差个数
            int[] pulselap = new int[MeterNumber];
            StPlan_WcPoint[] arrPlanList = new StPlan_WcPoint[MeterNumber];
            InitVerifyPara(maxWcNum, ref arrPlanList, ref pulselap, _DT);

            //初始化设备
            if (Stop) return;
            InitEquipment(CurPlan, pulselap);

            #region ------开始检定------
            if (Stop) return;
            DateTime startTime = DateTime.Now;          //记录下检定开始时间
            bool[] checkOver = new bool[MeterNumber];      //是否已经完成本次检定
            while (true)
            {
                //强制停止
                if (Stop) return;
                MessageAdd("正在检定...", EnumLogType.提示信息);
                if (CheckOver)
                {
                    MessageAdd("当前操作完毕。", EnumLogType.提示信息);
                    break;
                }
                //在这儿作时间检测。可预防在小电流情况下表老是不发脉冲，超过最大检定时间但是不停止的情况

                ////修改后
                //MeterInfo meterIR46 = App.CUS.Meters.First;
                int maxSeconds = VerifyConfig.MaxHandleTime * 2;

                if (DateTimes.DateDiff(startTime) > maxSeconds && IsMeterDebug)
                {
                    MessageAdd($"当前点检定已经超过最大检定时间{ maxSeconds}秒！", EnumLogType.提示信息);
                    CheckOver = true;
                    break;
                }

                if (IsMeterDebug)
                    checkOver = new bool[MeterNumber];

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
                //string[] resultKey = new string[MeterNumber];
                //object[] resultValue = new object[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    //强制停止
                    if (Stop) return;
                    TestMeterInfo meter = MeterInfo[i];   //表基本信息
                    if (!MeterInfo[i].YaoJianYn)//不检表处理
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
                        if (otherWcnum[0] > maxWcNum * 2 && _VerifyTimes[i] == 0)
                        {
                            MessageAdd($"表位{i + 1}没有检测到误差,请检查接线", EnumLogType.提示信息);
                        }
                        //误差次数没有增加，则此次误差板数据没有更新
                        if (_VerifyTimes[i] < maxWcNum)
                            CheckOver = false;
                        continue;
                    }

                    if (curNum[i] == 0 || curNum[i] == 255)
                    {
                        CheckOver = false;
                        continue;            //如果本表位没有出误差，换下一表
                    }
                    //频率
                    //得到当前表的等级
                    float meterLevel = MeterLevel(meter);                   //当前表的等级

                    //推箱子,最后一次误差排列在最前面
                    if (_VerifyTimes[i] > 1)
                    {
                        for (int j = maxWcNum - 1; j > 0; j--)
                        {
                            _DT.Rows[i][j] = _DT.Rows[i][j - 1];
                        }
                    }
                    _DT.Rows[i][0] = curWC[i];     //最后一次误差始终放在第一位
                    float[] tpmWc = ArrayConvert.ToSingle(_DT.Rows[i].ItemArray);  //Datable行到数组的转换
                    ErrorResoult tem = SetWuCha(arrPlanList[i], meterLevel, tpmWc);
                    string[] parems = tem.ErrorValue.Split('|');
                    if (index == 0)
                    {
                        ResultDictionary["电流倍数1"][i] = "10Ib";
                        ResultDictionary["误差1"][i] = parems[0];
                        ResultDictionary["误差2"][i] = parems[1];
                        ResultDictionary["平均值1"][i] = parems[2];
                        ResultDictionary["化整值1"][i] = parems[3];
                    }
                    else
                    {
                        ResultDictionary["电流倍数2"][i] = "Ib";
                        ResultDictionary["误差3"][i] = parems[0];
                        ResultDictionary["误差4"][i] = parems[1];
                        ResultDictionary["平均值2"][i] = parems[2];
                        ResultDictionary["化整值2"][i] = parems[3];
                    }

                    //跳差检测
                    if (_VerifyTimes[i] > 1)
                    {
                        string preWc = _DT.Rows[i][1].ToString();
                        if (Number.IsNumeric(preWc) && Number.IsNumeric(curWC[i]))
                        {
                            float jump = float.Parse(curWC[i]) - float.Parse(preWc);
                            if (Math.Abs(jump) > meterLevel)
                            {
                                checkOver[i] = false;
                                if (index == 0)
                                {
                                    ResultDictionary["结论1"][i] = "不合格";
                                }
                                else
                                {
                                    ResultDictionary["结论2"][i] = "不合格";
                                }

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
                    if (_VerifyTimes[i] >= maxWcNum)
                    {
                        if (ResultDictionary[$"结论{index}"][i] == "不合格" && !checkOver[i])
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
                //刷新数据
                RefUIData("电流倍数1");
                RefUIData("误差1");
                RefUIData("误差2");
                RefUIData("平均值1");
                RefUIData("平均值2");
                RefUIData("化整值1");
                RefUIData("化整值2");
                RefUIData("电流倍数2");
                RefUIData("误差3");
                RefUIData("误差4");
                RefUIData("结论1");
                RefUIData("结论2");

                //如果是调表状态则不检测是否检定完毕
                if (IsMeterDebug)
                    CheckOver = false;

                if (CheckOver)
                {
                    if (IsDemo)
                        Thread.Sleep(3000);
                }
            }
            #endregion
            if (isLastPoint)
            {
                //EquipHelper.Instance.SetCurFunctionOnOrOff(true);//启动停止当前设置的功能

            }
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
            //string[] strResultKey = new string[MeterNumber];
            //object[] objResultValue = new object[MeterNumber];
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
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment(StPlan_WcPoint point, int[] pulselap)
        {
            if (IsDemo) return true;
            bool isP = (point.PowerFangXiang == LYTest.Core.Enum.PowerWay.正向有功 || point.PowerFangXiang == LYTest.Core.Enum.PowerWay.反向有功);
            int[] meterconst = MeterHelper.Instance.MeterConst(isP);
            //int constants = (point.PowerFangXiang == LYTest.Core.Enum.PowerWay.正向有功 || point.PowerFangXiang == LYTest.Core.Enum.PowerWay.反向有功) ? MeterHelper.Instance.MeterConstMin()[0] :
            //    MeterHelper.Instance.MeterConstMin()[1];

            float xIb = Number.GetCurrentByIb(point.PowerDianLiu, OneMeterInfo.MD_UA, HGQ);

            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, point.PowerYuanJian, point.PowerFangXiang, point.PowerYinSu))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return false;
            }
            if (Stop) return true;
            ulong StdConst = GetStaConst();

            MessageAdd("正在设置标准表脉冲...", EnumLogType.提示信息);
            int index = 0;
            if (point.PowerFangXiang == LYTest.Core.Enum.PowerWay.反向无功 || point.PowerFangXiang == LYTest.Core.Enum.PowerWay.正向无功)
            {
                index = 1;
            }
            SetPulseType((index + 49).ToString("x"));


            MessageAdd("开始初始化基本误差检定参数!", EnumLogType.提示信息);


            //设置误差版被检常数
            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
            SetStandardConst(0, (int)(StdConst / 100), -2, 0xff);
            //设置误差版标准常数 TODO
            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
            if (!SetTestedConst(index, meterconst, 0, pulselap))
            {
                MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "功率方向", "功率因数", "功率元件", "电流倍数1", "误差1", "误差2", "平均值1", "化整值1", "电流倍数2", "误差3", "误差4", "平均值2", "化整值2", "结论1", "结论2" };
            return true;
        }

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
    }

}