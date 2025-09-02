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
    /// 方案：功率方向，元件，电流倍数， 托盘角度，线圈角度，磁场强度
    /// 结果：功率方向，元件，电流倍数，影响前误差，影响后误差，差值E，托盘角度，线圈角度，磁场强度，结论

    public class MFFs2 : VerifyBase
    {
        PowerWay _glfx = PowerWay.正向有功;
        Cus_PowerYuanJian _yj = Cus_PowerYuanJian.H;
        string _ibX = "1.0Ib";
        int _trayArg = 0; //托盘角度
        int _coilArg = 0; //线圈角度
        float _magneticCurrent = 5f; //磁场强度电流

        readonly int maxWCnum = VerifyConfig.ErrorCount;//最多误差次数
        // 1000* 5 * 60 = 5分钟
        int MaxTime = 300000;


        public override void Verify()
        {
            base.Verify();

            #region 上传误差参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
                    ResultDictionary["功率方向"][i] = _glfx.ToString();
                    ResultDictionary["元件"][i] = _yj.ToString();
                    ResultDictionary["电流倍数"][i] = _ibX;
                    ResultDictionary["影响前误差"][i] = "";
                    ResultDictionary["影响后误差"][i] = "";
                    ResultDictionary["差值"][i] = "";
                    ResultDictionary["托盘角度"][i] = _trayArg.ToString();
                    ResultDictionary["线圈角度"][i] = _coilArg.ToString();
                    ResultDictionary["磁场强度"][i] = _magneticCurrent.ToString();
                    ResultDictionary["结论"][i] = "";



                }
            }
            RefUIData("功率方向");
            RefUIData("元件");
            RefUIData("电流倍数");
            RefUIData("影响前误差");
            RefUIData("影响后误差");

            RefUIData("差值");
            RefUIData("托盘角度");
            RefUIData("线圈角度");
            RefUIData("磁场强度");
            RefUIData("结论");



            #endregion

            //if (MeterInfo[0].MD_MeterType == EutTypes.导轨表)
            //{
            //    EquipmentData.DeviceManager.LY2001_MultiControl(1); // 小电流接入
            //}
            //else
            //{
            //    EquipmentData.DeviceManager.LY2001_MultiControl(0); // 大电流接入
            //}

            MaxTime = VerifyConfig.MaxHandleTime * 1000;


            float meterLevel = MeterLevel(MeterInfo[0]);

            if (Stop) return;
            SetBluetoothModule(GetFangXianIndex(PowerWay.正向有功));

            float xIb = Number.GetCurrentByIb(_ibX, OneMeterInfo.MD_UA, HGQ);
            //if (OneMeterInfo.MD_MeterType == EutTypes.导轨表)
            //    xIb = xIb / 100;
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, _yj, _glfx, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            if (Stop) return;
            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            int[] meterconst = MeterHelper.Instance.MeterConst(true);
            if (!SetErrorData(0x01, 0x00, meterconst[0], 2))
            {
                MessageAdd("误差板启动失败...", EnumLogType.提示信息);
                return;
            }
            float limitUp = meterLevel;
            float limitLow = -meterLevel;
            StartError("影响前误差", limitUp, limitLow);


            //float meterLevel = MeterLevel(MeterInfo[0]);
            float changeLimit = GetChangeLimit(meterLevel);

            if (Stop) return;
            MessageAdd("调整磁场位置", EnumLogType.提示信息);
            InitEquipment2(_trayArg, _coilArg); //

            // 磁场升36A，约等于0.5mt
            if (Stop) return;
            MessageAdd("升加磁电流。", EnumLogType.提示信息);
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, _magneticCurrent, _magneticCurrent, _magneticCurrent, 0, 0, 0, 0, 0, 0);
            //DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, 4, 4, 0, 0, 0, 0, 0, 0, 0);
            Thread.Sleep(3000);

            if (Stop) return;
            if (!InitEquipment())
            {
                MessageAdd("初始化基本误差设备参数失败", EnumLogType.提示信息);
                return;
            }

            //面1做误差
            if (Stop) return;
            WaitTime("等待时间", 60);
            StartError("影响后误差", limitUp, limitLow);

            //停止误差板
            if (Stop) return;
            //StopWcb(GetFangXianIndex(FangXiang), 0xff);
            SetErrorData(0x00, 0x00, meterconst[0], 2);

            // 磁场升10A的电流
            if (Stop) return;
            MessageAdd("降磁电流", EnumLogType.提示信息);
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);


            if (Stop) return;
            MessageAdd("复位磁场位置", EnumLogType.提示信息);
            InitEquipment2(0, 0);

            if (Stop) return;
            PowerOn();

            //计算变差值
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检

                string[] err0 = ResultDictionary["影响前误差"][i].Split(',');
                string[] err1 = ResultDictionary["影响后误差"][i].Split(',');


                if (err1.Length >= 4 && err0.Length >= 4)
                {
                    float bc;
                    if (float.TryParse(err1[err1.Length - 2], out float nef) && float.TryParse(err0[err0.Length - 2], out float f1f))
                    {
                        bc = f1f - nef;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        continue;
                    }

                    ResultDictionary["差值"][i] = bc.ToString("f5");
                    if (Math.Abs(bc) <= changeLimit && ResultDictionary["结论"][i] != ConstHelper.不合格)
                        ResultDictionary["结论"][i] = "合格";
                    else
                        ResultDictionary["结论"][i] = "不合格";

                }

                //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
            }
            //RefUIData("最大变差值");
            RefUIData("结论");
            RefUIData("差值");

            WaitTime("检定完成，关闭电流", 5);
            MessageAdd("检定完成", EnumLogType.提示信息);
        }


        protected override bool CheckPara()
        {
            // 电流倍数，误差限，变差限
            string[] data = Test_Value.Split('|');
            if (data.Length < 6)
            {
                _glfx = PowerWay.正向有功;
                _yj = Cus_PowerYuanJian.H;
                _ibX = "1.0Ib";
                _trayArg = 0;
                _coilArg = 0;
                _magneticCurrent = 5f;
            }
            else
            {
                _glfx = (PowerWay)Enum.Parse(typeof(PowerWay), data[0]);
                _yj = (Cus_PowerYuanJian)Enum.Parse(typeof(Cus_PowerYuanJian), data[1]);
                _ibX = data[2];
                if (int.TryParse(data[3], out int a))
                    _trayArg = a;
                if (int.TryParse(data[4], out int b))
                    _coilArg = b;
                if (float.TryParse(data[5], out float f))
                    _magneticCurrent = f;

                // 磁场电流最大输出6A，
                if (_magneticCurrent > 6f)
                    _magneticCurrent = 6f;


            }


            //"电流倍数",正常误差,"面1误差","面2误差","面3误差","最大变差值","误差上限","误差下限"
            ResultNames = new string[] { "功率方向", "元件", "电流倍数", "影响前误差", "影响后误差", "差值", "托盘角度", "线圈角度", "磁场强度", "结论" };
            return true;
        }


        #region 计算方法



        private bool InitEquipment()
        {
            if (IsDemo) return true;

            if (Stop) return true;
            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            int[] meterconst = MeterHelper.Instance.MeterConst(true);
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

        private void StartError(string name, float limitUp, float limitLow)
        {
            if (Stop) return;
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
                    continue;

                List<float> errs = new List<float>();
                for (int i = 1; i < curWC.Length; i++)
                {
                    if (curWC[i] != 0f)
                        errs.Add(curWC[i]);
                }
                if (errs.Count <= 0) continue;

                TestMeterInfo meter = MeterInfo[0];
                float meterLevel = MeterLevel(meter);
                ErrorResoult tem = SetWuCha(limitUp, limitLow, meterLevel, errs.ToArray());

                if (ResultDictionary["结论"][0] != ConstHelper.不合格)
                    ResultDictionary["结论"][0] = tem.Result;

                ResultDictionary[name][0] = tem.ErrorValue;

                RefUIData("结论");
                RefUIData(name);

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
        public bool InitEquipment2(int trayArg, int coilArg)
        {
            if (IsDemo) return true;

            if (trayArg > 90) trayArg = 90;
            if (trayArg < -90) trayArg = -90;
            if (coilArg > 90) coilArg = 90;
            if (coilArg < -90) coilArg = -90;

            EquipmentData.DeviceManager.LY2001_7000HSet(0, coilArg); // 线圈

            WaitTime("正在调整线圈位置", 30);
            DateTime timeS = DateTime.Now;
            while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
            {
                EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out _);
                if ((Math.Abs(angle1) - Math.Abs(coilArg)) < 10) break;
                Thread.Sleep(1000);
            }

            Thread.Sleep(10000);
            EquipmentData.DeviceManager.LY2001_7000HSet(1, trayArg); // 托盘


            WaitTime("正在调整托盘位置", 10);
            timeS = DateTime.Now;
            while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
            {
                EquipmentData.DeviceManager.LY2001_7001HGet(out _, out float angle2);
                if ((Math.Abs(angle2) - Math.Abs(trayArg)) < 10) break;
                Thread.Sleep(1000);
            }
            //WaitTime("正在调整检测面", 2);


            return true;
        }



        private float GetChangeLimit(float meterLevel)
        {
            // A == 2.0
            // B == 1.0
            // C == 0.5
            // D == 0.2
            // E == 0.1
            if (meterLevel == 1.0)
                return 1.3f;
            else if (meterLevel == 0.5f)
                return 0.5f;
            else if (meterLevel == 0.2f)
                return 0.25f;
            else
                return 1.3f;
        }
    }
}
