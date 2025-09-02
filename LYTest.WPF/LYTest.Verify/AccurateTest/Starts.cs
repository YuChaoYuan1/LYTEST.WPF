using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 启动试验
    ///1:功率方向|启动电流倍数|是否自动计算启动电流|是否自动计算启动时间|是否默认合格|启动时间 
    ///2:功率方向|试验电压|标准试验时间|试验电流|开始时间|结束时间|实际运行时间|脉冲数
    /// </summary>
    public class Starts : VerifyBase
    {
        private StPlan_QiDong _plan = new StPlan_QiDong();
        /// <summary>
        /// 每一块表需要的起动时间
        /// </summary>
        float[] _arrStartTimes = new float[0];

        /// <summary>
        /// 最终起动电流
        /// </summary>
        float _startCurrent = 0F;

        WireMode _clfs = WireMode.三相四线;

        /// <summary>
        /// 误差计数类型：06-正向有功脉冲计数， 07-正向无功脉冲计数， 08-反向有功脉冲计数，09-反向无功脉冲计数
        /// </summary>
        int _wcCount = 6;


        public override void Verify()
        {
            MessageAdd("启动试验检定开始...", EnumLogType.流程信息);

            float MaxStartSec = InitVerifyPara();             //计算最大起动时间sec
            float MaxStartMin = MaxStartSec / 60F;            //计算最大起动时间min

            base.Verify();
            if (_plan.DefaultValue == 1 || IsDemo)    //默认合格
            {
                WaitTime("方案设置默认合格", 3);

                for (int Num = 0; Num < MeterNumber; Num++)
                {
                    if (!MeterInfo[Num].YaoJianYn) continue;
                    ResultDictionary["结论"][Num] = ConstHelper.合格;
                    ResultDictionary["试验电流"][Num] = _startCurrent.ToString();
                    ResultDictionary["开始时间"][Num] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ResultDictionary["功率方向"][Num] = _plan.PowerFangXiang.ToString();
                    ResultDictionary["试验电压"][Num] = MeterInfo[Num].MD_UB.ToString("F2");
                    ResultDictionary["实际运行时间"][Num] = (VerifyPassTime / 60.0).ToString("F4");
                    ResultDictionary["结束时间"][Num] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ResultDictionary["脉冲数"][Num] = "1";
                    _arrStartTimes[Num] = _arrStartTimes[Num] / 60;
                    ResultDictionary["脉冲间隔2"][Num] = "1";
                    ResultDictionary["误差"][Num] = "0.5";
                }

                ConvertTestResult("标准试验时间", _arrStartTimes, 2);

                RefUIData("结束时间");
                RefUIData("试验电压");
                RefUIData("试验电流");
                RefUIData("开始时间");
                RefUIData("功率方向");
                RefUIData("标准试验时间");
                RefUIData("实际运行时间");
                RefUIData("脉冲间隔2");
                RefUIData("误差");
                RefUIData("脉冲数");
                RefUIData("结论");

                MessageAdd("启动试验检定结束...", EnumLogType.流程信息);

                return;
            }


            int errType = 0;

            //设置功能参数
            if (Stop) return;
            SetBluetoothModule(_wcCount);

            //输出启动电压电流
            float realCurrent = VerifyConfig.Test_QuickModel ? _startCurrent * 10 : _startCurrent;
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, realCurrent, realCurrent, realCurrent, Cus_PowerYuanJian.H, _plan.PowerFangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            //启动误差版开始计数  06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）
            StartWcb(_wcCount, 0xff);
            if (Stop) return;

            float[] arrStartTimes2 = new float[MeterNumber];
            #region 上报试验参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    ResultDictionary["试验电流"][i] = _startCurrent.ToString("F4");
                    ResultDictionary["开始时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ResultDictionary["功率方向"][i] = _plan.PowerFangXiang.ToString();
                    ResultDictionary["试验电压"][i] = MeterInfo[i].MD_UB.ToString("F2");
                }
                arrStartTimes2[i] = _arrStartTimes[i] / 60;
            }


            ConvertTestResult("标准试验时间", arrStartTimes2, 2);

            RefUIData("试验电压");
            RefUIData("试验电流");
            RefUIData("开始时间");
            RefUIData("功率方向");
            RefUIData("标准试验时间");
            #endregion

            CheckOver = false;
            MessageAdd("开始检定", EnumLogType.提示信息);
            StartTime = DateTime.Now;
            if (OneMeterInfo.MD_JJGC == "IR46" || OneMeterInfo.MD_JJGC == "Q/GDW12175-2021" || OneMeterInfo.MD_JJGC == "Q/GDW10827-2020" || OneMeterInfo.MD_JJGC == "Q/GDW10364-2020")
            {
                while (true)
                {
                    long pastTime = base.VerifyPassTime;
                    Thread.Sleep(1000);
                    CheckOver = true;
                    if (!IsDemo)
                        ReadAndDealDataIR46(pastTime);
                    else
                        CheckOver = false;

                    float pastMinute = pastTime / 60F;

                    string strDes = $"起动时间{MaxStartMin}分，需要测试" + (MaxStartMin * 1.5f * 2).ToString("F2") + "分，已经经过" + pastMinute.ToString("F2") + "分";

                    if (MeterHelper.Instance.TypeCount > 1)
                        strDes += ",由于是多种表混检，大常数表可能提前出脉冲";
                    MessageAdd(strDes, EnumLogType.提示信息);

                    if (pastTime > MaxStartSec * 1.5f * 2 || Stop || CheckOver)
                    {
                        break;
                    }

                }


                bool ExError = false;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!string.IsNullOrWhiteSpace(ResultDictionary["脉冲数"][i]) && Convert.ToInt32(ResultDictionary["脉冲数"][i]) >= 2)
                    {
                        ExError = true;
                        break;
                    }
                }
                if (ExError)
                {
                    //进行误差实验
                    MessageAdd("开始进行启动试验的误差实验...", EnumLogType.流程信息);
                    bool[] arrCheckOver = new bool[MeterNumber];
                    int[] lastNum = new int[MeterNumber];                   //保存上一次误差的序号
                    lastNum.Fill(-1);
                    List<string>[] errList = new List<string>[MeterNumber]; //记录当前误差[数组长度，]
                    for (int i = 0; i < MeterNumber; i++)
                        errList[i] = new List<string>();

                    bool isP = _plan.PowerFangXiang == PowerWay.正向有功 || _plan.PowerFangXiang == PowerWay.反向有功;
                    int[] meterconst = MeterHelper.Instance.MeterConst(isP);
                    //CheckOver = false;

                    int index = isP ? 0 : 1;

                    errType = index;
                    SetBluetoothModule(index);
                    ulong StdConst = GetStaConst();


                    SetPulseType((index + 49).ToString("x"));

                    //设置误差版被检常数
                    MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
                    SetStandardConst(0, (int)(StdConst / 100), -2);
                    //设置误差版标准常数
                    MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
                    int[] qs = new int[MeterNumber];
                    qs.Fill(1);
                    if (!SetTestedConst(index, meterconst, 0, qs))
                    {
                        MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                    }
                    MessageAdd("正在启动误差板", EnumLogType.提示信息);
                    StartWcb(index, 0xff);
                    CheckOver = false;
                    DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
                    double MaxTimems = Math.Ceiling(MaxStartSec * 1000) * 3;

                    MessageAdd("开始基本误差检定", EnumLogType.提示信息);
                    while (!CheckOver)
                    {
                        if (Stop) break;
                        if (TimeSubms(DateTime.Now, TmpTime1) > MaxTimems) //超出最大处理时间
                        {
                            CheckOver = true;
                            MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                            break;
                        }
                        Thread.Sleep(1000);
                        string strDes = $"起动误差最大等待时间{(MaxTimems / 1000 / 60):F2}分，已经经过{TimeSubms(DateTime.Now, TmpTime1) / 1000 / 60:F2}分";
                        MessageAdd(strDes, EnumLogType.提示信息);
                        string[] curWC = new string[MeterNumber];   //重新初始化本次误差
                        int[] curNum = new int[MeterNumber];        //当前读取的误差序号
                        curWC.Fill("");
                        curNum.Fill(0);
                        if (!ReadWc(ref curWC, ref curNum, _plan.PowerFangXiang))    //读取误差
                        {
                            continue;
                        }
                        if (Stop) break;
                        CheckOver = true;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (Stop) break;
                            TestMeterInfo meter = MeterInfo[i];      //表基本信息
                            if (!meter.YaoJianYn) arrCheckOver[i] = true;  //不检表处理

                            if (arrCheckOver[i]) continue;
                            if (lastNum[i] >= curNum[i]) continue;
                            if (curWC[i] == "-999.0000") continue;
                            if (string.IsNullOrEmpty(curWC[i])) continue;
                            if (curNum[i] <= VerifyConfig.ErrorStartCount) continue; //当前误差次数小于去除的个数    
                            lastNum[i] = curNum[i];
                            errList[i].Insert(0, curWC[i]);
                            //计算误差
                        }
                        //检测是否全部完成
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];      //表基本信息
                            if (!meter.YaoJianYn) arrCheckOver[i] = true;  //不检表处理
                                                                           //MeterQdQid data = meter.MeterQdQids[ItemKey];
                            if (!meter.YaoJianYn) continue;
                            if (errList[i].Count > 0) arrCheckOver[i] = true;
                            if (!arrCheckOver[i])
                            {
                                CheckOver = false;
                                break;
                            }
                            else
                            {

                                ResultDictionary["误差"][i] = errList[i][0].ToString();
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (MeterInfo[i].YaoJianYn)
                        {
                            ResultDictionary["误差"][i] = "999.9";
                        }
                    }
                }

                RefUIData("误差");
            }
            else
            {
                while (true)
                {
                    long pastTime = base.VerifyPassTime;
                    Thread.Sleep(1000);
                    CheckOver = true;
                    if (!IsDemo)
                        ReadAndDealData(pastTime);
                    else
                        CheckOver = false;

                    if (Stop) return;
                    float pastMinute = pastTime / 60F;

                    string strDes = $"启(起)动时间{MaxStartMin:F2}分，已经经过{pastMinute:F2}分";
                    if (MeterHelper.Instance.TypeCount > 1)
                    {
                        strDes += ",由于是多种表混检，大常数表可能提前出脉冲";
                    }
                    MessageAdd(strDes, EnumLogType.提示信息);

                    if (pastTime > MaxStartSec || Stop || CheckOver)
                    {
                        break;
                    }
                }

                ReadAndDealData(VerifyPassTime);
            }  //其他表


            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (ResultDictionary["结论"][i] == "不合格")
                    {
                        ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }

            if (OneMeterInfo.MD_JJGC == "IR46" || OneMeterInfo.MD_JJGC == "Q/GDW12175-2021" || OneMeterInfo.MD_JJGC == "Q/GDW10827-2020" || OneMeterInfo.MD_JJGC == "Q/GDW10364-2020")
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (string.IsNullOrWhiteSpace(ResultDictionary["误差"][i]) || Convert.ToInt32(ResultDictionary["脉冲数"][i]) < 2 || Convert.ToInt32(ResultDictionary["脉冲数"][i]) > 3)
                        {
                            ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            ResultDictionary["结论"][i] = "不合格";
                        }
                        if (!string.IsNullOrWhiteSpace(ResultDictionary["脉冲间隔2"][i]))
                        {
                            if (MaxStartMin * 1.5 < float.Parse(ResultDictionary["脉冲间隔2"][i]))
                            {
                                ResultDictionary["结论"][i] = "不合格";
                            }
                        }
                        else
                        {
                            ResultDictionary["结论"][i] = "不合格";
                        }
                    }
                }
            }

            PowerOff();
            Thread.Sleep(3000);


            StopWcb(_wcCount, 0xff); //关闭误差版
            StopWcb(errType, 0xff); //关闭误差版

            RefUIData("结束时间");
            RefUIData("结论");
            MessageAdd("启动试验检定结束...", EnumLogType.流程信息);

        }




        protected override bool CheckPara()
        {
            //功率方向|启动电流倍数|是否自动计算启动电流|是否自动计算启动时间|是否默认合格|启动时间
            string[] tem = Test_Value.Split('|');
            if (tem.Length < 6) return false;

            _plan.PowerFangXiang = (PowerWay)Enum.Parse(typeof(PowerWay), tem[0]);

            if (tem[2] == "是")
            {
                _plan.FloatxIb = 0;
            }
            else
            {
                _plan.FloatxIb = float.Parse(tem[1].ToLower().Replace("ib", ""));
            }

            if (tem[3] == "是")
            {
                _plan.xTime = 0; //自动启动
            }
            else
            {
                float.TryParse(tem[5], out float t);//在方案指定起动时间时，起动时间单位为分钟
                _plan.xTime = t * 60;
                _plan.CheckTime = t * 60;
            }

            _plan.DefaultValue = tem[4] == "是" ? 1 : 0;

            //06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）</param>

            switch (tem[0])
            {
                case "正向有功":
                    _wcCount = 6;
                    break;
                case "正向无功":
                    _wcCount = 7;
                    break;
                case "反向有功":
                    _wcCount = 6;
                    break;
                case "反向无功":
                    _wcCount = 7;
                    break;
                default:
                    break;
            }

            _clfs = (WireMode)Enum.Parse(typeof(WireMode), OneMeterInfo.MD_WiringMode);

            ResultNames = new string[] { "功率方向", "试验电压", "标准试验时间", "试验电流", "开始时间", "结束时间", "实际运行时间", "脉冲间隔2", "误差", "脉冲数", "结论" };
            return true;
        }

        /// <summary>
        /// 初始化检定参数
        /// </summary>
        /// <returns>起动时间</returns>
        private float InitVerifyPara()
        {
            _arrStartTimes = new float[MeterNumber];

            for (int i = 0; i < MeterNumber; i++)
            {
                //计算起动电流
                TestMeterInfo meter = MeterInfo[i];
                if (meter == null || !meter.YaoJianYn)
                    continue;

                bool bFind = false;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    if (meter.MD_Constant == MeterInfo[j].MD_Constant && meter.MD_Grane == MeterInfo[j].MD_Grane)
                    {
                        _arrStartTimes[i] = _arrStartTimes[j];
                        bFind = true;
                        break;
                    }
                }


                if (!bFind)
                {
                    StPlan_QiDong p = (StPlan_QiDong)_plan;
                    p.CheckTimeAndIb(meter.MD_JJGC, _clfs, meter.MD_UB, meter.MD_UA, meter.MD_Grane, meter.MD_Constant, ZNQ, HGQ);
                    _arrStartTimes[i] = (float)Math.Round(p.CheckTime, 2);

                    if (p.FloatIb > _startCurrent)
                    {
                        _startCurrent = p.FloatIb;
                    }
                }
            }

            float[] arrStartTimeClone = (float[])_arrStartTimes.Clone();
            Core.Function.Number.PopDesc(ref arrStartTimeClone, false);                        //选择一个最大起动时间
            if (IsDemo)
                return 1F;
            else
                return arrStartTimeClone[0];
        }


        /// <summary>
        /// 读取并处理检定数据
        /// </summary>
        private void ReadAndDealData(long verifyTime)
        {
            StError[] stErrors = ReadWcbData(GetYaoJian(), _wcCount);
            CheckOver = true;
            for (int k = 0; k < MeterNumber; k++)
            {
                if (!MeterInfo[k].YaoJianYn)
                {
                    continue;
                }
                if (stErrors[k] == null)
                {
                    continue;
                }

                ResultDictionary["脉冲数"][k] = stErrors[k].szError;
                if (stErrors[k].szError == "0")
                {
                    if (verifyTime >= _arrStartTimes[k])
                    {
                        NoResoult[k] = "规程启动时间内没有脉冲输出";
                        ResultDictionary["结论"][k] = ConstHelper.不合格;
                        ResultDictionary["实际运行时间"][k] = (((float)verifyTime) / 60.0).ToString("F4") + "分";
                    }
                    CheckOver = false;
                }
                else if (!string.IsNullOrEmpty(stErrors[k].szError) && float.Parse(stErrors[k].szError) >= 1)
                {
                    //检测总时间是否已经超过本表起动时间

                    if (verifyTime <= _arrStartTimes[k] * 1.5)
                    {
                        ResultDictionary["结束时间"][k] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        ResultDictionary["实际运行时间"][k] = (((float)verifyTime) / 60.0).ToString("F4") + "分";
                        ResultDictionary["结论"][k] = ConstHelper.合格;
                    }
                }
                else
                {
                    CheckOver = false;
                }
                if (Stop) break;
            }

            RefUIData("结束时间");
            RefUIData("实际运行时间");
            RefUIData("脉冲数");
        }

        private void ReadAndDealDataIR46(long verifyTime)
        {
            StError[] stErrors = ReadWcbData(GetYaoJian(), _wcCount);
            CheckOver = true;
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                if (stErrors[i] == null || stErrors[i].szError == null)
                {
                    CheckOver = false;
                    continue;
                }
                ResultDictionary["脉冲数"][i] = stErrors[i].szError;
                //   MeterQdQid data = meter.MeterQdQids[ItemKey];
                if (stErrors[i].szError == "0")
                {
                    if (verifyTime >= _arrStartTimes[i])
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        NoResoult[i] = "规程启动时间内没有脉冲输出";
                    }
                    CheckOver = false;
                }
                else if (!string.IsNullOrEmpty(stErrors[i].szError) && float.Parse(stErrors[i].szError) >= 1)
                {
                    if (verifyTime < _arrStartTimes[i] * 1.5 && ResultDictionary["实际运行时间"][i] == null)
                    {
                        ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        //modify
                        //ResultDictionary["实际运行时间"][i] = (((float)verifyTime) / 60.0).ToString("F4") + "分";
                        //modify
                        ResultDictionary["实际运行时间"][i] = (((float)verifyTime) / 60.0).ToString("F4");
                        ResultDictionary["结论"][i] = ConstHelper.合格;

                    }
                    if (!string.IsNullOrEmpty(stErrors[i].szError) && float.Parse(stErrors[i].szError) >= 2 && ResultDictionary["实际运行时间"][i] != null && ResultDictionary["脉冲间隔2"][i] == null)
                    {
                        ResultDictionary["脉冲间隔2"][i] = ((verifyTime / 60f) - float.Parse(ResultDictionary["实际运行时间"][i])).ToString("f2");
                        ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        ResultDictionary["结论"][i] = ConstHelper.合格;
                    }
                    if (ResultDictionary["实际运行时间"][i] == null || ResultDictionary["脉冲间隔2"][i] == null)
                    {
                        CheckOver = false;
                    }
                }
                else
                {
                    CheckOver = false;
                }
                if (Stop) break;
            }
            RefUIData("结束时间");
            RefUIData("实际运行时间");
            RefUIData("脉冲数");

            //add
            RefUIData("脉冲间隔2");
        }


    }
}
