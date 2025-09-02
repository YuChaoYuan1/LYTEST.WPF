using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Verify.AccurateTest;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.CheckInfo;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace LYTest.Verify.Influence
{
    /// <summary>
    /// 外部工频磁场试验
    /// </summary>
    /// 方案：功率方向，元件，电流倍数， 托盘角度，线圈角度，磁场强度
    /// 结果：功率方向，元件，电流倍数，影响前误差，影响后误差，差值E，托盘角度，线圈角度，磁场强度，结论

    public class MFFs2_2 : VerifyBase
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


            MaxTime = VerifyConfig.MaxHandleTime * 1000;

            string name = $"{_glfx}_{_yj}_1.0_{_ibX}";
            //Dictionary<string, string[]> dic = EquipmentData.CheckResults.ResultCollection
            AsyncObservableCollection<CheckNodeViewModel> dic = EquipmentData.CheckResults.ResultCollection;
            var basicErr = from e in dic
                           where e.ParaNo == ProjectID.基本误差试验 && e.Name == name
                           select e;

            if (!basicErr.Any())
            {
                MessageAdd("在基本误差数据中找到相同负载点的数据, 退出当前检测项", EnumLogType.错误信息);

                return;
            }
            DynamicViewModel err = basicErr.First().CheckResults[6]; // 第6表位
            ResultDictionary["影响前误差"][6] = $"{err.GetProperty("误差1")},{err.GetProperty("误差2")},{err.GetProperty("平均值")},{err.GetProperty("化整值")}";
            RefUIData("影响前误差");




            float limitUp = float.Parse(err.GetProperty("误差上限").ToString());
            float limitLow = float.Parse(err.GetProperty("误差下限").ToString());
            float meterLevel = MeterLevel(MeterInfo[0]);
            float changeLimit = GetChangeLimit(meterLevel);

            if (Stop) return;
            MessageAdd("调整磁场位置", EnumLogType.提示信息);
            InitEquipment2(_trayArg, _coilArg); //

            // 磁场升10A的电流
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
            StartError(limitUp, limitLow);

            //停止误差板
            if (Stop) return;
            StopWcb(GetFangXianIndex(FangXiang), 0xff);


            // 磁场升10A的电流
            if (Stop) return;
            MessageAdd("降磁电流", EnumLogType.提示信息);
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);


            if (Stop) return;
            MessageAdd("复位磁场位置", EnumLogType.提示信息);
            InitEquipment2(0, 0);

            if (Stop) return;

            //计算变差值
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;     //表位不要检

                string[] err0 = ResultDictionary["影响前误差"][i].Split(',');
                string[] err1 = ResultDictionary["影响后误差"][i].Split(',');


                if (err1.Length >= 4 && err0.Length >= 4)
                {
                    float bc;
                    if (float.TryParse(err1[2], out float nef) && float.TryParse(err0[2], out float f1f))
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

            PowerOn();
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
            SetBluetoothModule(GetFangXianIndex(PowerWay.正向有功));


            int[] meterconst = MeterHelper.Instance.MeterConst(true);

            // 正常误差

            float xIb = Number.GetCurrentByIb(_ibX, OneMeterInfo.MD_UA, HGQ);
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, _yj, _glfx, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return false;
            }

            ulong constants = GetStaConst();

            MessageAdd("正在设置标准表脉冲...", EnumLogType.提示信息);
            SetPulseType(49.ToString("x"));
            if (Stop) return true;
            MessageAdd("开始初始化基本误差检定参数!", EnumLogType.提示信息);
            //设置误差版被检常数
            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
            int SetConstants = (int)(constants / 100);
            SetStandardConst(0, SetConstants, -2, 0xff);
            //设置误差版标准常数 TODO2
            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
            int[] q = new int[MeterNumber];
            q.Fill(2);
            if (!SetTestedConst(0, meterconst, 0, q))
            {
                MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                return false;
            }

            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            if (!StartWcb(GetFangXianIndex(PowerWay.正向有功), 0xff))
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

        private void StartError(float limitUp, float limitLow)
        {
            if (Stop) return;
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
                if (DateTime.Now.Subtract(TmpTime1).TotalMilliseconds > MaxTime && !IsMeterDebug) //超出最大处理时间并且不是调表状态
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
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) arrCheckOver[i] = true;     //表位不要检
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
                    if (errList[i].Count > maxWCnum)
                        errList[i].RemoveAt(errList[i].Count - 1);
                    if (Stop) break;
                    //计算误差
                    float[] tpmWc = ArrayConvert.ToSingle(errList[i].ToArray());  //Datable行到数组的转换

                    float meterLevel = MeterLevel(meter);
                    ErrorResoult tem = SetWuCha(limitUp, limitLow, meterLevel, tpmWc);

                    if (errList[i].Count >= maxWCnum)  //误差数量>=需要的最大误差数2
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
                        //tem.Result = ConstHelper.不合格;
                        NoResoult[i] = "没有读取到俩次误差";
                    }

                    if (ResultDictionary["结论"][i] != ConstHelper.不合格)
                        ResultDictionary["结论"][i] = tem.Result;

                    ResultDictionary["影响后误差"][i] = tem.ErrorValue;
                }
                RefUIData("影响后误差");

                if (Array.IndexOf(arrCheckOver, false) < 0 && !IsMeterDebug)  //全部都为true了
                    break;
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
            Thread.Sleep(500);
            EquipmentData.DeviceManager.LY2001_7000HSet(1, trayArg); // 托盘


            WaitTime("正在调整检测面", 30);

            //DateTime timeS = DateTime.Now;
            //while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 60 * 1000)
            //{
            //    //读取线圈角度
            //    EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out float angle2);
            //    if (Math.Abs(angle1 - coilArg) < 5 && Math.Abs(angle2 - trayArg) < 5)
            //        break;

            //    Thread.Sleep(1000);
            //}


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
