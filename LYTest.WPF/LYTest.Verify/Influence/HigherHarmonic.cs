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
    /// <summary>
    ///高次谐波
    /// </summary>
    public class HigherHarmonic : VerifyBase
    {
        float meterLevel = 2;//等级
        /// <summary>
        /// 误差限
        /// </summary>
        float ErrorLimit = 1f;
        class ErrData
        {
            public string Err1 { get; set; }
            public string Err2 { get; set; }
            public string Pjz { get; set; }
            public string Hzz { get; set; }
            public string Pc { get; set; }
        }

        readonly Dictionary<int, ErrData[]> errDatas = new Dictionary<int, ErrData[]>();


        //errData[] errDatas = new errData[53];
        public override void Verify()
        {
            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                errDatas.Add(i, new ErrData[53]);
                for (int n = 0; n < 53; n++)
                {
                    errDatas[i][n] = new ErrData();
                }
            }

            ReadWc(-1, "");
            for (int i = 15; i < 41; i++)
            {
                if (Stop) break;
                ReadWc(i, "上升");
            }
            for (int i = 40; i >= 15; i--)
            {
                if (Stop) break;
                ReadWc(i, "下降");
            }

            float[] HarmonicContent = new float[60];
            float[] HarmonicPhase = new float[60];
            DeviceControl.StartHarmonicTest("1", "1", "1", "1", "1", "1", HarmonicContent, HarmonicPhase, true);
            PowerOn();
            WaitTime("恢复电压", 5);

            if (Stop) return;

            bool[] resoult = new bool[MeterNumber];
            resoult.Fill(true);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                for (int n = 0; n < errDatas[i].Length; n++)
                {
                    string str = errDatas[i][n].Pc;
                    if (string.IsNullOrEmpty(str))
                    {
                        resoult[i] = false;
                        break;
                    }
                    if (Math.Abs(float.Parse(str)) > ErrorLimit)
                    {
                        resoult[i] = false;
                        break;
                    }
                }
            }
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (resoult[i])
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }

            }
            RefUIData("结论");

        }

        private void RefUI()
        {
            string[] value = new string[5];
            // ResultNames = new string[] { "误差1", "误差2", "平均值", "化整值", "偏差值", "结论" }
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                //value[i] = "";
                value.Fill("");
                for (int n = 0; n < errDatas[i].Length; n++)
                {
                    value[0] += "|" + errDatas[i][n].Err1;
                    value[1] += "|" + errDatas[i][n].Err2;
                    value[2] += "|" + errDatas[i][n].Pjz;
                    value[3] += "|" + errDatas[i][n].Hzz;
                    value[4] += "|" + errDatas[i][n].Pc;
                }
                ResultDictionary["误差1"][i] = value[0].Trim('|');
                ResultDictionary["误差2"][i] = value[1].Trim('|');
                ResultDictionary["平均值"][i] = value[2].Trim('|');
                ResultDictionary["化整值"][i] = value[3].Trim('|');
                ResultDictionary["偏差值"][i] = value[4].Trim('|');
            }
            RefUIData("误差1");
            RefUIData("误差2");
            RefUIData("平均值");
            RefUIData("化整值");
            RefUIData("偏差值");
        }

        readonly int MaxTime = VerifyConfig.MaxHandleTime * 1000;
        //第几次,误差值
        private void ReadWc(int c, string UpOrDown)//-1代表正常的
        {
            if (Stop) return;
            if (c != -1)
            {
                Opnein(c);
            }
            int index = 0;
            if (UpOrDown == "上升")
            {
                index = c - 14;
            }
            else if (UpOrDown == "下降")
            {
                index = 27 + 40 - c;
            }
            SetBluetoothModule(0);
            ErrorInitEquipment(PowerWay.正向有功, Cus_PowerYuanJian.H, "1.0", "Itr", 1);


            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            if (!StartWcb(0, 0xff))
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
                    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                    break;
                }
                if (Stop) break;
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
                    if (errList[i].Count > 2)
                        errList[i].RemoveAt(errList[i].Count - 1);
                    meterLevel = MeterLevel(MeterInfo[i]);
                    if (Stop) break;
                    //计算误差
                    float[] tpmWc = ArrayConvert.ToSingle(errList[i].ToArray());  //Datable行到数组的转换
                    arrPlanList[i].ErrorShangXian = ErrorLimit;
                    arrPlanList[i].ErrorXiaXian = -ErrorLimit;

                    ErrorResoult tem = SetWuCha(arrPlanList[i], meterLevel, tpmWc);

                    if (errList[i].Count >= 2)  //误差数量>=需要的最大误差数2
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
                    errDatas[i][index].Err1 = value[0].ToString();
                    if (Stop) break;
                    if (value.Length > 3)
                    {
                        if (value[1].ToString().Trim() != "")
                        {
                            errDatas[i][index].Err2 = value[0].ToString();
                            errDatas[i][index].Err1 = value[1].ToString();
                            //跳差判断
                            if (CheckJumpError(errDatas[i][index].Err1, errDatas[i][index].Err2, meterLevel, VerifyConfig.JumpJudgment))
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
                        errDatas[i][index].Pjz = value[2];
                        errDatas[i][index].Hzz = value[3];
                        if (index == -1)
                        {
                            if (string.IsNullOrEmpty(errDatas[i][index].Pjz))
                            {
                                errDatas[i][index].Pjz = "99.99";//第一个不能空，否则没办法计算后面的了，给个99
                            }
                            errDatas[i][index].Pc = "0";
                        }
                        else
                        {

                            if (!string.IsNullOrEmpty(value[2]))
                            {
                                errDatas[i][index].Pc = Math.Round(float.Parse(value[2]) - float.Parse(errDatas[i][0].Pjz), 4).ToString();
                            }
                        }
                        RefUI();
                    }
                }
                if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)  //全部都为true了
                    break;
            }
            if (Stop) return;
            StopWcb(GetFangXianIndex(FangXiang), 0xff);//停止误差板
        }
        #region 计算
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
        //    {
        //        return string.Format("+{0}", v);
        //    }

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

        #endregion


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] level = Number.GetDj(OneMeterInfo.MD_Grane);
            switch (level[0])
            {
                case "B":
                    ErrorLimit = 1;
                    break;
                case "C":
                case "D":
                    ErrorLimit = 0.5f;
                    break;
                default:
                    break;
            }
            ResultNames = new string[] { "误差1", "误差2", "平均值", "化整值", "偏差值", "结论" };
            return true;
        }


        /// <summary>
        /// 开启谐波
        /// </summary>
        private void Opnein(int count)
        {
            float[] HarmonicContent = new float[60];
            float[] HarmonicPhase = new float[60];
            HarmonicContent[count - 2] = 2;
            HarmonicPhase[count - 2] = 0;
            DeviceControl.StartHarmonicTest("1", "1", "1", "0", "0", "0", HarmonicContent, HarmonicPhase, true);
            HarmonicContent[count - 2] = 10;
            HarmonicPhase[count - 2] = 0;
            DeviceControl.StartHarmonicTest("0", "0", "0", "1", "1", "1", HarmonicContent, HarmonicPhase, true);
            //PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, 50);
            WaitTime("正在开启谐波", 1);
        }
    }
}
