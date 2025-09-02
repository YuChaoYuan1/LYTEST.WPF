using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.Selfheating
{

    //自热实验
    class SelfheatVerify : VerifyBase
    {
        /// <summary>
        /// 每一个误差点最多读取多少次误差
        /// </summary>
        private readonly int m_WCMaxTimes=0;

        public override void Verify()
        {
            base.Verify();
            //检定参数
            string value = Test_Value;
            string[] parameter = value.Split('|');//功率方向|功率元件|电流倍数|功率因数|运行时间(分钟)|圈数|次数|误差限|标准点|提示
            List<string>[] err = new List<string>[MeterNumber];//误差存值
            for (int i = 0; i < MeterNumber; i++)
                err[i] = new List<string>();

            int[] lastWcNum = new int[MeterNumber];     //表位取误差次数


            int[] pulseLap = new int[MeterNumber];      //检定圈数
            InitPara(ref pulseLap);                 //初始化参数

            if (Stop) return;
            Thread.Sleep(100);

            //初始化设备
            CheckOver = false;

            if (!InitEquipment(parameter)) return;
            WaitTime("自热试验前预热", 60);//预热1分钟

            //源给的方案要求设置
            ViewModel.EquipmentData.DeviceManager.SetPowerSafe(false);

            DateTime startTime = DateTime.Now;     //记录下检定开始时间
            bool[] checkOver = new bool[MeterNumber];

            int maxTime = (int)(Convert.ToSingle(parameter[4]) * 60); //单位 秒

            //开始检定
            while (!CheckOver)
            {
                MessageAdd("正在检定......", EnumLogType.提示信息);
                WaitTime("自热运行", 3);
                if (Stop) break;
                if ((DateTime.Now.Subtract(startTime).TotalSeconds > maxTime) && !IsMeterDebug)
                {
                    MessageAdd($"当前点检定已经超过最大检定时间 {maxTime}秒！", EnumLogType.提示信息);
                    CheckOver = true;
                    break;
                }

                if (!IsMeterDebug)
                    checkOver = new bool[MeterNumber];              //是否已经完成本次检定

                string[] curWC = new string[MeterNumber];           //重新初始化本次误差
                int[] curNum = new int[MeterNumber];                //当前累计检定次数
                PowerWay glfx = Glys(parameter[1]);
                if (!ReadWc(ref curWC, ref curNum, glfx)) continue;

                //处理每一次检定数据
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) break;

                    #region ----------数据合法性检测----------
                    if (!MeterInfo[i].YaoJianYn)
                        checkOver[i] = true;

                    if (curNum[i] <= lastWcNum[i] || checkOver[i]) continue;
                    lastWcNum[i] = curNum[i];

                    err[i].Insert(0, curWC[i]);

                    //误差次数没有增加，则此次误差板数据没有更新
                    if (err[i].Count < int.Parse(parameter[6]))
                        CheckOver = false;
                    //处理超过255次的情况
                    if (curNum[i] > 1)
                    {
                        TestMeterInfo meter = MeterInfo[i];

                        //float[] iBiMax = OneMeterInfo.GetIb();
                        //float xIb = iBiMax[0];

                        float meterLevel = MeterLevel(meter);

                        string UpDown = parameter[7];
                        float UP = float.Parse($"+{UpDown}");
                        float Down = float.Parse($"-{UpDown}");
                        //设置误差参数
                        ErrorLimit limit = new ErrorLimit
                        {
                            UpLimit = UP,
                            DownLimit = Down,
                            MeterLevel = meterLevel
                        };

                        float[] wc = ArrayConvert.ToSingle(err[i].ToArray());  //Datable行到数组的转换
                        MeterError curerror = SetWuCha(limit, meterLevel, wc);
                        ResultDictionary["结论"][i] = curerror.Result;
                        string[] arrWc = curerror.WCValue.Split('|');
                        if (arrWc.Length > 2)
                        {
                            if (arrWc[arrWc.Length - 2].Length > 0)
                            {
                                if (string.IsNullOrEmpty(arrWc[0]))
                                    ResultDictionary["误差1"][i] = arrWc[0];
                                if (string.IsNullOrEmpty(arrWc[1]))
                                    ResultDictionary["误差2"][i] = arrWc[1];
                                if (string.IsNullOrEmpty(arrWc[2]))
                                    ResultDictionary["平均值"][i] = arrWc[2];

                                if (Number.IsNumeric(ResultDictionary["误差1"][i] = arrWc[0]) && Number.IsNumeric(ResultDictionary["误差2"][i] = arrWc[1]))
                                {
                                    //ResultDictionary["化整值"][i] = Math.Abs(float.Parse(ResultDictionary["误差1"][i] = arrWc[0]) - float.Parse(arrWc[arrWc.Length - 2])).ToString("#0.00");
                                    ResultDictionary["化整值"][i] = arrWc[3];
                                    if (ResultDictionary["结论"][i] != "不合格")
                                    {
                                        if (float.Parse(ResultDictionary["化整值"][i]) > limit.UpLimit)
                                        {
                                            ResultDictionary["结论"][i] = "不合格";
                                        }
                                    }
                                }
                            }
                            RefUIData("误差1");
                            RefUIData("误差2");
                            RefUIData("平均值");
                            RefUIData("化整值");
                            RefUIData("误差改变量(%)");
                            RefUIData("结论");
                        }
                        //跳差检测
                        if (err[i].Count > 1)
                        {
                            string preWc = err[i][1].ToString();
                            if (Number.IsNumeric(preWc) && Number.IsNumeric(curWC[i]))
                            {
                                float jump = float.Parse(curWC[i]) - float.Parse(preWc);
                                if (Math.Abs(jump) > meterLevel)
                                {
                                    checkOver[i] = false;
                                    ResultDictionary["结论"][i] = "不合格";
                                    MessageAdd($"检测到{i + 1}跳差，重新取误差进行计算", EnumLogType.提示信息);
                                    if (err[i].Count > m_WCMaxTimes && DateTime.Now.Subtract(startTime).TotalSeconds > maxTime)
                                        checkOver[i] = true;
                                    else
                                        CheckOver = false;
                                }
                            }
                        }
                        if (err[i].Count >= int.Parse(parameter[6]))  //实际误差个数 >= 需要的误差个数
                        {
                            if (ResultDictionary["结论"][i] != "合格" && !checkOver[i])
                            {
                                if (err[i].Count > m_WCMaxTimes && DateTimes.DateDiff(startTime) > maxTime)
                                {
                                    checkOver[i] = true;
                                    MessageAdd($"第{i}表位超过最大检定次数", EnumLogType.提示信息);
                                }
                            }
                            else
                            {
                                if (err[i].Count > m_WCMaxTimes && DateTimes.DateDiff(startTime) > maxTime)
                                    checkOver[i] = true;
                            }
                        }
                        else
                        {
                            checkOver[i] = false;
                        }
                    }
                    #endregion
                }
                CheckOver = true;
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!checkOver[j])
                    {
                        CheckOver = false;
                        break;
                    }
                }
                //如果是调表状态则不检测是否检定完毕
                if (!IsMeterDebug)
                {
                    CheckOver = false;
                }
                if (CheckOver)
                    if (IsDemo)
                        Thread.Sleep(1000);
            }
            //EquipHelper.Instance.SetCurFunctionOnOrOff(true);  //DOTO启动设置当前功能
        }

        /// <summary>
        /// 初始化检定参数
        /// </summary>
        /// <param name="pulseLap">检定圈数</param>
        private void InitPara(ref int[] pulseLap)
        {
            string value = Test_Value;
            string[] parameter = value.Split('|');//功率方向|功率元件|电流倍数|功率因数|运行时间(分钟)|圈数|次数|误差限|标准点|提示
            //上报数据参数
            //string[] resultKey = new string[MeterNumber];
            pulseLap = new int[MeterNumber];
            MessageAdd("开始初始化检定参数...", EnumLogType.提示信息);

            //填充空数据
            for (int i = 0; i < MeterNumber; i++)
            {
                pulseLap[i] = int.Parse(parameter[5]);
            }
            MessageAdd("初始化检定参数完毕! ", EnumLogType.提示信息);
        }


        /// <summary>
        /// 设置设备参数[带升源]
        /// </summary>
        /// <param name="plan">当前检定参数</param>
        /// <param name="pulselap">检定圈数</param>
        /// <returns></returns>
        private bool InitEquipment(string[] plan)
        {
            if (IsDemo) return true;
            PowerWay glfx = Glys(plan[1]);
            float[] IbImax = OneMeterInfo.GetIb();
            float xIb = IbImax[0];
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, glfx, plan[3]))
            {
                return false;
            }
            return true;

        }
        //private float[] MeterConst(bool isP)
        //{
        //    float[] Cs = new float[12];
        //    return Cs;
        //}
        ///// <summary>
        ///// 基本误差参数初始化
        ///// </summary>
        ///// <returns></returns>
        //public bool InitPara_BasicError(PowerWay glfx, float[] meterconst, int[] circleCount)
        //{
        //    return true;
        //}
        private PowerWay Glys(string glys)
        {
            switch (glys)
            {
                case "正向有功":
                    return PowerWay.正向有功;
                case "反向有功":
                    return PowerWay.反向有功;
                case "正向无功":
                    return PowerWay.正向无功;
                case "反向无功":
                    return PowerWay.反向无功;
                default:
                    return PowerWay.正向有功;
            }
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "误差1", "误差2", "平均值", "化整值", "误差改变量(%)", "结论" };
            return true;
        }



        /// <summary>
        /// 计算基本误差
        /// </summary>
        /// <param name="data">要参与计算的误差数组</param>
        /// <returns></returns>
        public MeterError SetWuCha(ErrorLimit wcPoint, float meterLevel, float[] data)
        {
            MeterError resoult = new MeterError();
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
            if (avg >= wcPoint.DownLimit && avg <= wcPoint.UpLimit)
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
            resoult.WCValue = strWuCha;

            return resoult;
        }
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


    }
}
