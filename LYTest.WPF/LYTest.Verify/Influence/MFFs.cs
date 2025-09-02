using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Verify.AccurateTest;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.Influence
{
    /// <summary>
    /// 外部工频磁场试验
    /// </summary>
    /// 结论数据："电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"，"结论"
    /// 方案数据："电流倍数","持续时间(秒)","误差限","变差限"
    //磁感应强度为 0.5mT(400 A/m)；
    public class MFFs : VerifyBase
    {
        const string NormalError = "正常误差";
        const string Face1Error = "面1误差";
        const string Face2Error = "面2误差";
        const string Face3Error = "面3误差";

        //磁场源电流为10A

        #region 参数

        /// <summary>
        /// 误差限
        /// </summary>
        float ErrorLimit = 1f;

        /// <summary>
        /// 变差限
        /// </summary>
        float ChangeLimit = 1f;

        /// <summary>
        /// 电流倍数
        /// </summary>
        string Xib = "1.0Ib";

        /// <summary>
        /// 持续时间秒
        /// </summary>
        int RunTime = 0;

        readonly int maxWCnum = VerifyConfig.ErrorCount;//最多误差次数
        int MaxTime = 300000;

        float _magneticCurrent = 5f; //磁场强度电流

        #endregion

        public override void Verify()
        {
            base.Verify();
            //IsStop = Stop;

            #region 上传误差参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
                    ResultDictionary["电流倍数"][i] = Xib;
                    ResultDictionary[NormalError][i] = "";
                    ResultDictionary[Face1Error][i] = "";
                    ResultDictionary[Face2Error][i] = "";
                    ResultDictionary[Face3Error][i] = "";
                    ResultDictionary["最大变差值"][i] = "";
                    ResultDictionary["误差上限"][i] = ErrorLimit.ToString();
                    ResultDictionary["误差下限"][i] = (-ErrorLimit).ToString();
                    ResultDictionary["结论"][i] = "";
                }
            }
            RefUIData("电流倍数");
            RefUIData(NormalError);
            RefUIData(Face1Error);
            RefUIData(Face2Error);
            RefUIData(Face3Error);
            RefUIData("最大变差值");
            RefUIData("误差下限");
            RefUIData("误差上限");
            #endregion

            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
                        ResultDictionary["电流倍数"][i] = Xib;
                        ResultDictionary[NormalError][i] = "0";
                        ResultDictionary[Face1Error][i] = "0";
                        ResultDictionary[Face2Error][i] = "0";
                        ResultDictionary[Face3Error][i] = "0";
                        ResultDictionary["最大变差值"][i] = "0";
                        ResultDictionary["误差上限"][i] = ErrorLimit.ToString();
                        ResultDictionary["误差下限"][i] = (-ErrorLimit).ToString();
                        ResultDictionary["结论"][i] = "合格";
                    }
                }
                RefUIData("电流倍数");
                RefUIData(NormalError);
                RefUIData(Face1Error);
                RefUIData(Face2Error);
                RefUIData(Face3Error);
                RefUIData("最大变差值");
                RefUIData("误差下限");
                RefUIData("误差上限");
                RefUIData("结论");
                return;
            }

            MaxTime = VerifyConfig.MaxHandleTime * 1000;

            //if (MeterInfo[0].MD_MeterType == EutTypes.导轨表)
            //{
            //    EquipmentData.DeviceManager.LY2001_MultiControl(1); // 小电流接入
            //}
            //else
            //{
            //    EquipmentData.DeviceManager.LY2001_MultiControl(0); // 大电流接入
            //}

            if (!InitEquipment())
            {
                MessageAdd("初始化基本误差设备参数失败", EnumLogType.提示信息);
                return;
            }

            MessageAdd("升加磁电流。", EnumLogType.提示信息);

            //开始做一次基本误差
            if (Stop) return;
            MessageAdd("开始正常误差测试", EnumLogType.提示信息);
            StartError(NormalError);

            // 磁场最大升6A的电流  40A约等下0.5mt
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, _magneticCurrent, _magneticCurrent, _magneticCurrent, 0, 0, 0, 0, 0, 0);
            Thread.Sleep(3000);

            //面1做误差
            if (Stop) return;
            MessageAdd("开始面1误差测试", EnumLogType.提示信息);
            InitEquipment2(0); //
            WaitTime("等待时间", RunTime);
            StartError(Face1Error);

            //面2做误差
            if (Stop) return;
            MessageAdd("开始面2误差测试", EnumLogType.提示信息);
            InitEquipment2(1); //
            WaitTime("等待时间", RunTime);
            StartError(Face2Error);

            //面3做误差
            if (Stop) return;
            MessageAdd("开始面3误差测试", EnumLogType.提示信息);
            InitEquipment2(2); //
            WaitTime("等待时间", RunTime);
            StartError(Face3Error);

            //停止误差板
            if (Stop) return;
            SetErrorData(0x00, 0x00, 2000, 2);

            // 磁场降磁电流
            MessageAdd("降磁电流", EnumLogType.提示信息);
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            PowerOn();

            //停止误差板
            if (Stop) return;
            SetErrorData(0x00, 0x00, 2000, 2);

            if (Stop) return;
            MessageAdd("复位磁场位置", EnumLogType.提示信息);
            InitEquipment2(3);


            if (Stop) return;

            //计算变差值
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检

                string[] neArr = ResultDictionary[NormalError][i].Split(',');
                string[] f1Arr = ResultDictionary[Face1Error][i].Split(',');
                string[] f2Arr = ResultDictionary[Face2Error][i].Split(',');
                string[] f3Arr = ResultDictionary[Face3Error][i].Split(',');

                if (neArr.Length >= 4 && f1Arr.Length >= 4 && f2Arr.Length >= 4 && f3Arr.Length >= 4)
                {
                    float bc;
                    if (float.TryParse(neArr[2], out float nef) && float.TryParse(f1Arr[2], out float f1f))
                    {
                        bc = f1f - nef;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        continue;
                    }

                    if (float.TryParse(f2Arr[2], out float f2f))
                    {
                        if (Math.Abs(bc) < Math.Abs(f2f))
                            bc = f2f;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        continue;
                    }

                    if (float.TryParse(f3Arr[2], out float f3f))
                    {
                        if (Math.Abs(bc) < Math.Abs(f3f))
                            bc = f3f;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        continue;
                    }

                    ResultDictionary["最大变差值"][i] = bc.ToString("f5");
                    if (Math.Abs(bc) <= ChangeLimit && ResultDictionary["结论"][i] != ConstHelper.不合格)
                        ResultDictionary["结论"][i] = "合格";
                    else
                        ResultDictionary["结论"][i] = "不合格";

                }

                //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
            }
            RefUIData("最大变差值");
            RefUIData("结论");

            WaitTime("检定完成，关闭电流", 5);
            MessageAdd("检定完成", EnumLogType.提示信息);
        }


        protected override bool CheckPara()
        {
            // 电流倍数，误差限，变差限
            string[] data = Test_Value.Split('|');
            Xib = "1.0Ib";
            RunTime = 60;
            ErrorLimit = 2;
            ChangeLimit = 1;


            if (data.Length >= 5)
            {
                if (!string.IsNullOrWhiteSpace(data[0]))
                    Xib = data[0];
                if (int.TryParse(data[1], out int i1))
                    RunTime = i1;

                if (float.TryParse(data[2], out float f1))
                    ErrorLimit = f1;

                if (float.TryParse(data[3], out float f2))
                    ChangeLimit = f2;

                if (float.TryParse(data[4], out float f3))
                    _magneticCurrent = f3;

                if (_magneticCurrent > 6)
                    _magneticCurrent = 6;


            }


            //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
            ResultNames = new string[] { "电流倍数", "正常误差", "面1误差", "面2误差", "面3误差", "最大变差值", "误差上限", "误差下限", "结论" };
            return true;
        }


        #region 计算方法



        private bool InitEquipment()
        {
            if (IsDemo) return true;

            if (Stop) return true;
            SetBluetoothModule(GetFangXianIndex(PowerWay.正向有功));

            // 正常误差
            float xIb = Number.GetCurrentByIb(Xib, OneMeterInfo.MD_UA, HGQ);
            //if (OneMeterInfo.MD_MeterType == EutTypes.导轨表)
            //    xIb = xIb / 100;
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return false;
            }


            if (Stop) return true;
            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            int[] meterconst = MeterHelper.Instance.MeterConst(true);
            //if (OneMeterInfo.MD_MeterType == EutTypes.导轨表)
            //    meterconst[0] = meterconst[0] * 100;
            if (!SetErrorData(0x01, 0x00, meterconst[0], 2))
            {
                MessageAdd("误差板启动失败...", EnumLogType.提示信息);
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
        public ErrorResoult SetWuCha(float upLimit, float lowLimit, float meterLevel, float[] data)
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

            ErrorResoult resoult = new ErrorResoult();
            // 检测是否超过误差限
            if (avg <= upLimit && avg >= lowLimit)
                resoult.Result = ConstHelper.合格;
            else
                resoult.Result = ConstHelper.不合格;

            //记录误差
            string str = string.Empty;
            int wcCount = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != ConstHelper.没有误差默认值)
                {
                    wcCount++;
                    str += AddFlag(data[i], VerifyConfig.PjzDigit) + ",";
                }
                else
                {
                    str += "|";
                }
            }
            if (wcCount != data.Length)
            {
                resoult.Result = ConstHelper.不合格;
            }

            resoult.ErrorValue = $"{str}{AvgNumber},{HZNumber}";

            return resoult;
        }

        #endregion

        private void StartError(string test)
        {
            if (Stop) return;
            //bool[] arrCheckOver = new bool[MeterNumber];    //表位完成记录

            DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
            while (true)
            {
                if (Stop) break;
                if (DateTime.Now.Subtract(TmpTime1).TotalMilliseconds > MaxTime && !IsMeterDebug) //超出最大处理时间并且不是调表状态
                {
                    MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                    break;
                }

                if (!ReadErrorData(out float[] curWC))    //读取误差
                {
                    continue;
                }

                TestMeterInfo meter = MeterInfo[0];
                if (IsMeterDebug) continue;   //表位检定通过了

                List<float> errs = new List<float>();
                for (int i = 1; i < curWC.Length; i++)
                {
                    if (curWC[i] != 0)
                        errs.Add(curWC[i]);
                }

                float meterLevel = MeterLevel(meter);
                ErrorResoult tem = SetWuCha(ErrorLimit, -ErrorLimit, meterLevel, errs.ToArray());


                if (tem.Result != ConstHelper.不合格)
                    ResultDictionary["结论"][0] = tem.Result;
                ResultDictionary[test][0] = tem.ErrorValue;

                RefUIData("结论");
                RefUIData(test);

                if (errs.Count >= maxWCnum)  //误差数量>=需要的最大误差数2
                {
                    break;
                }


            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face">0-面1，1-面2，2-面3</param>
        /// <returns></returns>
        public bool InitEquipment2(int face)
        {
            if (IsDemo) return true;

            DateTime timeS;
            if (face == 0) // 面1
            {
                EquipmentData.DeviceManager.LY2001_7000HSet(0, 0); // 正面
                //EquipmentData.DeviceManager.LY2001_7000HSet(1, 0); // 正面
                //EquipmentData.DeviceManager.LY2001_7000HSet(0, -80); // 正面
                //EquipmentData.DeviceManager.LY2001_7000HSet(0, -90); // 正面

                timeS = DateTime.Now;
                WaitTime("正在调整检测面", 5);

                while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
                {
                    EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out _);
                    if (Math.Abs(angle1) < 5) break;
                    Thread.Sleep(1000);
                }


                Thread.Sleep(10000);
                EquipmentData.DeviceManager.LY2001_7000HSet(1, 0); // 托盘
                WaitTime("正在调整检测面", 5);

                timeS = DateTime.Now;
                while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
                {
                    EquipmentData.DeviceManager.LY2001_7001HGet(out _, out float angle2);
                    if (Math.Abs(angle2) < 5) break;
                    Thread.Sleep(1000);
                }
            }
            else if (face == 1) // 面2
            {
                EquipmentData.DeviceManager.LY2001_7000HSet(0, 90); // 正面
                WaitTime("正在调整检测面", 5);

                timeS = DateTime.Now;
                while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
                {
                    EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out _);
                    if (Math.Abs(angle1 - 90) < 10) break;
                    Thread.Sleep(1000);
                }
            }
            else if (face == 2) // 面3
            {
                EquipmentData.DeviceManager.LY2001_7000HSet(1, 90); // 侧面
                WaitTime("正在调整检测面", 20);

                timeS = DateTime.Now;
                while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
                {
                    EquipmentData.DeviceManager.LY2001_7001HGet(out _, out float angle2);
                    if (Math.Abs(Math.Abs(angle2) - 90) < 10) break;
                    Thread.Sleep(1000);
                }
            }
            else if (face == 3)//复位
            {
                EquipmentData.DeviceManager.LY2001_7000HSet(0, 0);
                WaitTime("正在复位", 40);
                //EquipmentData.DeviceManager.LY2001_7000HSet(1, 0);
                EquipmentData.DeviceManager.LY2001_7000HSet(1, 0);
                WaitTime("正在复位", 35);

                timeS = DateTime.Now;
                while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
                {
                    EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out float angle2);
                    if (Math.Abs(angle1 - 0) < 10 && Math.Abs(angle2 - 0) < 10) break;
                    Thread.Sleep(1000);
                }
            }
            return true;
        }

    }
}
