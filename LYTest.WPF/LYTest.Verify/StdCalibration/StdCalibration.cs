using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.CheckController.MulitThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LYTest.Verify.StdCalibration
{
    public class StdCalibration : VerifyBase
    {
        #region 参数
        //校准电压|校准电流|功率因数|标准常数|被检常数|圈数|误差限
        /// <summary>
        /// 校准电压
        /// </summary>
        public float CalibrationU=60;   //校准电压
        /// <summary>
        /// 校准电流
        /// </summary>
        public float CalibrationI = 5; //校准电流
        /// <summary>
        ///  功率因数
        /// </summary>
        public string Glys = "0.5L";   //功率因数
        /// <summary>
        ///  标准常数
        /// </summary>
        public int StandardConst = 400000;   //标准常数
        /// <summary>
        ///  被检常数
        /// </summary>
        public int[] InspectedConst;  //被检常数
        /// <s]mary>
        ///   圈数
        /// </summary>
        public int[] Qs; //圈数
        /// <summary>
        /// 误差上限
        /// </summary>
        public float ErrorLimitMax = 0.02f;//误差上限
        /// <summary>
        ///  误差下限
        /// </summary>
        public float ErrorLimitMin = -0.02f;//误差下限

        #endregion




    

        //设置标准表状态
        //升源
        //做误差
        //将误差写入被检表（311校准指令）
        //完成

        public override void Verify()
        {
            base.Verify();
            //升源
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(CalibrationU, CalibrationU, CalibrationU, CalibrationI, CalibrationI, CalibrationI, Cus_PowerYuanJian.H, PowerWay.正向有功, Glys))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;
            //设置误差版被检常数
            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
            SetStandardConst(0, StandardConst / 100, -2, 0xff);
            //设置误差版标准常数
            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
            SetTestedConst(0, InspectedConst, 0, Qs, 0xff);
            #region 上报试验参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    ResultDictionary["误差限"][i] = ErrorLimitMin + "|" + ErrorLimitMax;
                }
            }
            RefUIData("误差限");
            #endregion

            if (Stop) return;

            //开始不断读取误差，和误差限进行判断
            bool[] arrCheckOver = new bool[MeterNumber];
            string[] arrCurWCValue = new string[MeterNumber];//当前误差
            int[] arrCurrentWcNum = new int[MeterNumber];//当前累计检定次数,是指误差板从启动开始到目前共产生了多少次误差
            int[] arrCurrentTopWcNum = new int[MeterNumber];//上一次误差次数
            int[] arrMeterWcNum = new int[MeterNumber];     //表位取误差次数

            //正在设置标准表常数
            for (int i = 0; i < MeterNumber; i++)
            {
                arrCurrentTopWcNum[i] = 0;
                if (MeterInfo[i].YaoJianYn) //表不要检定就跳过
                {
                    ResultDictionary["结论"][i] = "合格"; //默认合格,检定不通过时候会设置成不合格
                }
            }


            string[] FrameAry = null;
            object[] paras = new object[] { (byte)0x13, (double)InspectedConst[0], new double[] { 0, 0, 0, 0, 0, 0 }, FrameAry };
            MessageAdd("正在设置标准表常数...", EnumLogType.提示信息);
            SendDeviceControl2(GetYaoJian(), "StdGear2", paras);  //设置标准表常数

            #region 判断是电流校准还是电压校准

            int pulseStart = 1;
            string Vs = "04";
            bool againPowerOn = false; //重新升源，做电流的时候，需要吧其他俩个电流置0；

            string TestNo = Test_No.Substring(0, Test_No.IndexOf("_"));
            switch (TestNo)
            {
                case "13001":
                    pulseStart = 51;
                    Vs = "04";
                    break;
                case "13002":
                    pulseStart = 54;
                    Vs = "05";
                    againPowerOn = true;
                    break;
                case "13003":
                    pulseStart = 57;
                    againPowerOn = true;
                    Vs = "00";
                    break;
                default:
                    break;
            }
            #endregion



            float[] Ia = new float[] { CalibrationI, 0, 0, 0, CalibrationI, 0, 0, 0, CalibrationI };

            int endpulse = 4;
            if (OneMeterInfo.MD_WiringMode=="单相")
            {
                endpulse = 2;
            }
           

            //分别检定ABC
            for (int pulse = 1; pulse < endpulse; pulse++)
            {
                //重新升源
                if (againPowerOn)
                {
                    int index = (pulse - 1) * 3;
                    if (!PowerOn(CalibrationU, CalibrationU, CalibrationU, Ia[index], Ia[index + 1], Ia[index + 2], Cus_PowerYuanJian.H, PowerWay.正向有功, Glys))
                    {
                        MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                        return;
                    }
                    //WaitTime("重新升源成功", 3);
                }

                DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
                if (Stop) break;
                for (int i = 0; i < MeterNumber; i++)
                {
                    arrCheckOver[i] = false;
                }

                MessageAdd("正在设置表位脉冲...", EnumLogType.提示信息);

                
                SendDeviceControl2(ReversalBool(arrCheckOver), "SetPulseType", (pulseStart).ToString("x"));   //设置标准表脉冲
                //设置雷电标准表脉冲输出端口
                MessageAdd("正在设置雷电标准表脉冲输出端口...", EnumLogType.提示信息);
                SetPuase("02", Vs, (pulse - 1).ToString());
                if (Stop) break;
                //设置雷电标准表脉冲常数
                MessageAdd("正在设置雷电标准表脉冲常数...", EnumLogType.提示信息);
                SetConstant(Vs, StandardConst / 1000);
                if (Stop) break;
                //启动误差版
                MessageAdd("正在启动误差版...", EnumLogType.提示信息);
                StartWcb(0, 0xff);
                Thread.Sleep(200);
                if (Stop) break;
                MessageAdd("开始检定...", EnumLogType.提示信息);


                int readErrorNumber = 0;//空的次数

                string[] WcString = new string[MeterNumber];
                //检定部分
                while (true)
                {
                    if (Stop) break;
                    if (Array.IndexOf(arrCheckOver, false) < 0)  //全部都为true了
                        break;

                    if (TimeSubms(DateTime.Now, TmpTime1) > VerifyConfig.MaxHandleTime * 1000) //超出最大处理时间
                    {
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                        break;
                    }

                    if (Stop) break;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) //表不要检定就跳过
                            arrCheckOver[i] = true;
                        if (arrMeterWcNum[i] > VerifyConfig.ErrorMax) //超出需要取的最大误差数
                        {
                            MessageAdd("超出最大误差次数,正在退出...", EnumLogType.提示信息);
                            arrCheckOver[i] = true;
                            continue;
                        }
                    }


                    MessageAdd("正在读取误差...", EnumLogType.提示信息);
                    StError[] stErrors = ReadWcbData(ReversalBool(arrCheckOver), 0);  //读取表位误差
                    readErrorNumber++;
                    double[] CurWCValue = new double[MeterNumber];

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!arrCheckOver[i])
                        {
                            if (stErrors[i] == null)
                            {
                                if (readErrorNumber > VerifyConfig.ErrorMax) //一直读不到误差，直接不管他了
                                {
                                    ResultDictionary["结论"][i] = "不合格";
                                    arrCheckOver[i] = true;
                                }
                                continue;
                            }
                            arrCurWCValue[i] = stErrors[i].szError;  //误差值
                            arrCurrentWcNum[i] = stErrors[i].Index; //第几次误差
                            ResultDictionary[$"L{pulse}校准前误差"][i] = arrCurWCValue[i];
                            if (arrCurrentWcNum[i] > 1 && arrCurrentWcNum[i] > arrCurrentTopWcNum[i]) //次数大于1 并且大于上一次误差次数
                            {
                                arrCurrentTopWcNum[i] = arrCurrentWcNum[i];//记录下上一次次数
                                arrMeterWcNum[i]++; //读取到的第几次误差--有效误差
                                ResultDictionary[$"L{pulse}校准前误差"][i] = arrCurWCValue[i];
                                CurWCValue[i] = double.Parse(arrCurWCValue[i]);
                                WcString[i] = (CurWCValue[i] * 10000).ToString();
                            }
                        }
                    }
                    RefUIData($"L{pulse}校准前误差");

                    bool WcBool = false;
                    for (int i = 0; i < WcString.Length; i++)
                    {
                        if (arrCheckOver[i]) //表不要检定就跳过
                            continue;
                        if (WcString[i] != null)
                        {
                            WcBool = true;
                        }
                        else
                        {
                            WcBool = false;
                            break;
                        }
                    }

                    if (!WcBool)
                    {
                        MessageAdd("再次读取误差...", EnumLogType.提示信息);
                        continue;
                    }

                    RefUIData($"L{pulse}校准前误差");
                    if (Stop) return;
                    //下发校准
                    MessageAdd("正在设置脉冲校准误差...", EnumLogType.提示信息);
                    SendDeviceControl2(ReversalBool(arrCheckOver), "SetPulseCalibration", WcString);    // 脉冲校准误差

                    if (Stop) return;
                    WaitTime("等待校准", 6);


                    StartWcb(0, 0xff);//启动误差板
                    Thread.Sleep(200);
                    //再次读取误差
                    MessageAdd("正在读取校准后的误差", EnumLogType.提示信息);
                    //stErrors = ReadWcbData2(ReversalBool(arrCheckOver), 0);  //读取所有表位误差

                    while (true)
                    {
                        if (Stop) return;
                        bool t = true;
                        stErrors = ReadWcbData(ReversalBool(arrCheckOver), 0);
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!arrCheckOver[i])     //需要检定
                            {
                                if (stErrors[i] != null)    //读取到了误差
                                {
                                    if (stErrors[i].Index < 1)     //还有表位没有读取到有效误差
                                    {
                                        t = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    t = false;
                                    break;
                                }

                            }
                        }
                        if (t)
                        {
                            break;
                        }
                        if (TimeSubms(DateTime.Now, TmpTime1) > VerifyConfig.MaxHandleTime * 1000) //超出最大处理时间
                        {
                            MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                            break;
                        }

                    }


                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!arrCheckOver[i])
                        {
                            CurWCValue[i] = double.Parse(stErrors[i].szError);
                            ResultDictionary[$"L{pulse}校准后误差"][i] = CurWCValue[i].ToString("0.0000");
                            WcString[i] = (CurWCValue[i] * 10000).ToString();
                            if (CurWCValue[i] < ErrorLimitMax && CurWCValue[i] > ErrorLimitMin) //在误差范围内
                            {
                                arrCheckOver[i] = true;
                            }
                        }

                    }
                    RefUIData($"L{pulse}校准后误差");
                    if (Stop) return;


                    if (Array.IndexOf(arrCheckOver, false) >= 0)  //不合格的
                    {
                        MessageAdd("校准误差不合格第二次校准..", EnumLogType.提示信息);

                        SendDeviceControl2(ReversalBool(arrCheckOver), "SetPulseCalibration", WcString);    // 脉冲校准误差
                        WaitTime("等待第二次校准", 5);
                        MessageAdd("正在读取第二次校准后的误差", EnumLogType.提示信息);
                        //stErrors = ReadWcbData2(ReversalBool(arrCheckOver), 0);  //读取表位误差

                        StartWcb(0, 0xff);//启动误差板
                        Thread.Sleep(200);
                        //再次读取误差
                        MessageAdd("正在读取第二次校准后的误差", EnumLogType.提示信息);
                        //stErrors = ReadWcbData2(ReversalBool(arrCheckOver), 0);  //读取所有表位误差

                        while (true)
                        {
                            if (Stop) return;
                            bool t = true;
                            stErrors = ReadWcbData(ReversalBool(arrCheckOver), 0);
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (!arrCheckOver[i])     //需要检定
                                {
                                    if (stErrors[i] != null)    //读取到了误差
                                    {
                                        if (stErrors[i].Index < 1)     //还有表位没有读取到有效误差
                                        {
                                            t = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        t = false;
                                        break;
                                    }

                                }
                            }
                            if (t)
                            {
                                break;
                            }
                            if (TimeSubms(DateTime.Now, TmpTime1) > VerifyConfig.MaxHandleTime * 1000) //超出最大处理时间
                            {
                                MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                                break;
                            }

                        }


                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!arrCheckOver[i])
                            {
                                CurWCValue[i] = double.Parse(stErrors[i].szError);
                                ResultDictionary[$"L{pulse}校准后误差"][i] = CurWCValue[i].ToString("0.0000");
                                arrCheckOver[i] = true;
                                if (CurWCValue[i] > ErrorLimitMax || CurWCValue[i] < ErrorLimitMin) //在误差范围外
                                {
                                    ResultDictionary["结论"][i] = "不合格";
                                }
                            }

                        }
                        RefUIData($"L{pulse}校准后误差");
                        WaitTime("等待", 1);
                    }
                    for (int i = 0; i < WcString.Length; i++)
                    {
                        WcString[i] = null;
                    }
                    
                }


                 pulseStart++;
            }

            StopWcb(0, 0xff);//停止误差板
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);
        }


        /// <summary>
        /// bool数组反转
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool[] ReversalBool(bool[] Bt)
        {
            bool[] t = new bool[Bt.Length];
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = !Bt[i];
            }
            return t;
        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //校准电压|校准电流|功率因数|标准常数|被检常数|圈数|误差限
            string[] arrayTemp = Test_Value.Split('|');
            if (arrayTemp.Length != 7)
                return false;

            if (float.TryParse(arrayTemp[0], out float TemF))
                CalibrationU = TemF;
            if (float.TryParse(arrayTemp[1], out TemF))
                CalibrationI = TemF;

            Glys = arrayTemp[2];
            if (int.TryParse(arrayTemp[3], out int TemI))
                StandardConst = TemI;
            if (int.TryParse(arrayTemp[4], out TemI))
            {
                InspectedConst = new int[MeterNumber];
                InspectedConst.Fill(TemI);
            }
            if (int.TryParse(arrayTemp[5], out TemI))
            {
                Qs = new int[MeterNumber];
                Qs.Fill(TemI);
            }

            if (float.TryParse(arrayTemp[6], out TemF))
            {
                ErrorLimitMax = Math.Abs(TemF);
                ErrorLimitMin = -ErrorLimitMax;
            }
            //L1校准前误差|L1校准后误差|L2校准前误差|L2校准后误差|L3校准前误差|L3校准后误差|误差限|结论
            ResultNames = new string[] { "L1校准前误差", "L1校准后误差", "L2校准前误差", "L2校准后误差","L3校准前误差", "L3校准后误差", "误差限", "结论" };
            return true;
        }


        //private  bool[] arrCheckOver2 ;

        #region 旧
//        private void Te()
//        {
//            base.Verify();
//            //升源
//            MessageAdd("正在升源...", EnumLogType.提示信息);
//            if (!PowerOn(CalibrationU, CalibrationU, CalibrationU, CalibrationI, CalibrationI, CalibrationI, Cus_PowerYuanJian
//.H, PowerWay.正向有功, Glys))
//            {
//                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
//                return;
//            }
//            //WaitTime("升源成功,等待源稳定", 5);
//            if (Stop) return;

//            #region 设置数据


//            //设置误差版标准常数
//            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
//            SetStandardConst(0, StandardConst / 100, -2, 0xff);
//            if (Stop) return;
//            //设置误差版被检常数         
//            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
//            SetTestedConst(0, InspectedConst, 0, Qs, 0xff);
//            if (Stop) return;

//            string[] FrameAry = null;
//            object[] paras = new object[] { (byte)0x13, (double)InspectedConst[0], new double[] { 0, 0, 0, 0, 0, 0 }, FrameAry };
//            MessageAdd("正在设置标准表常数...", EnumLogType.提示信息);
//            SendDeviceControl2(GetYaoJian(), "stdGear2", paras);  //设置标准表常数
//            if (Stop) return;
//            #endregion

//            #region 上报试验参数
//            for (int i = 0; i < MeterNumber; i++)
//            {
//                if (MeterInfo[i].YaoJianYn)
//                {
//                    ResultDictionary["误差限"][i] = ErrorLimitMin + "|" + ErrorLimitMax;
//                    ResultDictionary["结论"][i] = "合格"; //默认合格,检定不通过时候会设置成不合格
//                }
//            }
//            RefUIData("误差限");
//            if (Stop) return;
//            #endregion

//            #region 判断是电流校准还是电压校准
//            int pulseStart = 1;
//            string Vs = "04";
//            bool againPowerOn = false; //重新升源，做电流的时候，需要吧其他俩个电流置0；
//            string TestNo = Test_No.Substring(0, Test_No.IndexOf("_"));
//            switch (TestNo)
//            {
//                case "13001":
//                    pulseStart = 33;
//                    Vs = "04";
//                    break;
//                case "13002":
//                    pulseStart = 36;
//                    Vs = "05";
//                    againPowerOn = true;
//                    break;
//                case "13003":
//                    pulseStart = 39;
//                    againPowerOn = true;
//                    Vs = "00";
//                    break;
//                default:
//                    break;
//            }
//            float[] Ia = new float[] { CalibrationI, 0, 0, 0, CalibrationI, 0, 0, 0, CalibrationI };

//            int endpulse = 4;
//            if (OneMeterInfo.MD_WiringMode == "单相") //单相的情况只做A相
//            {
//                endpulse = 2;
//            }
//            #endregion


//            #region 检定过程

//            #region 变量
//            bool[] arrCheckOver = new bool[MeterNumber];
//            string[] arrCurWCValue = new string[MeterNumber];//当前误差
//            int[] arrCurrentWcNum = new int[MeterNumber];//当前累计检定次数,是指误差板从启动开始到目前共产生了多少次误差
//            int[] arrCurrentTopWcNum = new int[MeterNumber];//上一次误差次数
//            int[] arrMeterWcNum = new int[MeterNumber];     //表位取误差次数
//            string[] Xw = new string[3] { "A", "B", "C" };


//            //arrCheckOver2 = new bool[MeterNumber];

//            #endregion
//            //开始不断读取误差，和误差限进行判断
//            for (int pulse = 1; pulse < endpulse; pulse++)
//            {
//                if (againPowerOn)
//                {
//                    MessageAdd($"开始检定【{Xw[pulse]}相】,正在升源，请稍后...", EnumLogType.提示信息);
//                    int index = (pulse - 1) * 3;
//                    if (!PowerOn(CalibrationU, CalibrationU, CalibrationU, Ia[index], Ia[index + 1], Ia[index + 2], Cus_PowerYuanJian
//.H, PowerWay.正向有功, Glys))
//                    {
//                        MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
//                        return;
//                    }
//                    //WaitTime("重新升源成功", 3);
//                    if (Stop) return;
//                }     //重新升源
//                DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时

//                #region 设置参数
//                MessageAdd("正在设置表位脉冲...", EnumLogType.提示信息);
//                SendDeviceControl2(ReversalBool(arrCheckOver), "SetPulseType", pulseStart.ToString());   //设置标准表脉冲
//                //设置雷电标准表脉冲输出端口
//                MessageAdd("正在设置雷电标准表脉冲输出端口...", EnumLogType.提示信息);
//                SetPuase("02", Vs, (pulse - 1).ToString());
//                if (Stop) break;
//                //设置雷电标准表脉冲常数
//                MessageAdd("正在设置雷电标准表脉冲常数...", EnumLogType.提示信息);
//                SetConstant(Vs, StandardConst / 1000);
//                if (Stop) break;
//                //启动误差版
//                MessageAdd("正在启动误差版...", EnumLogType.提示信息);
//                StartWcb(0, 0xff);
//                Thread.Sleep(200);
//                if (Stop) break;
//                #endregion




//                #region 检定循环
//                //每个表位,各检个的，读取误差，读完发送校准，校准完在次读取，在校准

//                DeviceThreadManager .Instance.MaxThread = MeterNumber;
//                DeviceThreadManager.Instance.MaxTaskCountPerThread = 1;
//                DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
//                {
//                    if (MeterInfo[pos].YaoJianYn)
//                    {
//                        ErrorCalibration(pos);
//                    }
//                    if (Stop) return;
//                };
//                DeviceThreadManager.Instance.Start();
//                WaitWorkDone();


//                #endregion


//                pulseStart++;
//            }

//            #endregion


//            StopWcb(0, 0xff);//停止误差板
//            RefUIData("结论");
//            MessageAdd("检定完成", EnumLogType.提示信息);
//        }
        ///// <summary>
        ///// 误差校准
        ///// </summary>
        ///// <param name="index">表位id0开始</param>
        //private void ErrorCalibration(int index)
        //{
        //    //string arrCurWCValue;//当前误差
        //    //int arrCurrentWcNum;//当前累计检定次数,是指误差板从启动开始到目前共产生了多少次误差
        //    //int arrCurrentTopWcNum;//上一次误差次数
        //    //int arrMeterWcNum;     //表位取误差次数
        //    bool arrCheckOver = false;
        //    while (true)
        //    {
        //        if (Stop) break;
        //        if (arrCheckOver == true)  //结束了
        //            break;

        //        //if (TimeSub(DateTime.Now, TmpTime1) > VerifyConfig.MaxHandleTime * 1000) //超出最大处理时间
        //        //{
        //        //    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
        //        //    break;
        //        //}
        //    }
        //}
        #endregion
    }
}
