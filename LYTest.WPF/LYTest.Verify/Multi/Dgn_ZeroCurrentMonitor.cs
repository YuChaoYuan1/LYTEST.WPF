using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 零线电流检测
    /// add lsj 20220718
    /// 试验过程中关于继电器控制应该有问题  测试时需要重新修改
    /// </summary>
    class Dgn_ZeroCurrentMonitor : VerifyBase
    {
        public override void Verify()
        {
            //当程序开始切换误差板继电器后，将不允许中间停止
            //为防止做其他功能时，继电器状态不正确
            base.Verify();
            //先降源
            if (Stop) return;
            PowerOff();
            WaitTime("等待源稳定...", 5);

            bool[] YJMeter = new bool[MeterNumber];
            bool[] bBiaoweiBz = new bool[MeterNumber];

            MessageAdd("切换表位旁路", EnumLogType.提示信息);

            //bBiaoweiBz.Fill(false);
            //bBiaoweiBz[0] = true;

            //for (int i = 0; i < MeterNumber; i++)
            //{
            //    TestMeterInfo m = meterInfo[i];
            //    YJMeter[i] = m.YaoJianYn & bBiaoweiBz[i];
            //}
            YJMeter.Fill(false);
            YJMeter[0] = true;
            bBiaoweiBz.Fill(true);
            ControlMeterRelay(bBiaoweiBz, 2);
            //ControlMeterRelay(YJMeter, 2);
            //ControlMeterRelay(ReversalBool(YJMeter), 1);
            WaitTime("旁路除了一号表位的其他表位继电器...", 3);

            if (Stop) return;
            //开启零线电流板A 00开启，BC 01关闭
            StartZeroCurrent(00, 01);
            WaitTime("开启零线电流板...", 2);

            //string allResult = "合格";
            string[] arrParam = Test_Value.Split('|');//格式：电流值,误差限
                                                      //for (int k = 0; k < arrParam.Length; k++)
                                                      //{
                                                      //string[] ps = arrParam[k].Split(',');
            float xIb = Number.GetCurrentByIb(arrParam[0], OneMeterInfo.MD_UA, HGQ);
            //升源A相，和C相电流
            MessageAdd("升电压和A相电流", EnumLogType.提示信息);
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

            //读电表零线电流
            if (Stop) return;
            string[] meterData = MeterProtocolAdapter.Instance.ReadData("零线电流");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["结论"][i] = "合格";
            }
            if (Stop) return;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!YJMeter[i]) continue;
                if (!string.IsNullOrEmpty(meterData[i]))
                {
                    //零线电流减去A相电流
                    float errValue = Math.Abs(Convert.ToSingle(meterData[i])) - Math.Abs(EquipmentData.StdInfo.Ia);
                    ResultDictionary["电流值"][i] = Math.Abs(Convert.ToSingle(meterData[i])).ToString();
                    ResultDictionary["误差值"][i] = errValue.ToString("f4");

                    if (Math.Abs(errValue) <= Convert.ToSingle(arrParam[1])) //误差限
                    {
                        ResultDictionary["结论"][i] = "合格";
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        //allResult = "不合格";
                    }
                }
            }
            RefUIData("电流值");
            RefUIData("误差值");
            RefUIData("结论");

            //}
            //for (int i = 0; i < MeterNumber; i++)
            //{
            //    if (!meterInfo[i].YaoJianYn) continue;
            //    ResultDictionary["结论"][i] = allResult;
            //}
            //RefUIData("结论");

            //降源
            PowerOff();
            WaitTime("等待源稳定...", 5);

            if (Stop) return;
            //关闭零线电流板 A 01关闭 ，BC 00开启
            StartZeroCurrent(01, 00);
            WaitTime("关闭零线电流板...", 2);
            ControlMeterRelay(GetYaoJian(), 2);
            ControlMeterRelay(ReversalBool(GetYaoJian()), 1);
            MessageAdd("零线电流检测试验完成!", EnumLogType.提示信息);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] arrParam = Test_Value.Split('|');

            List<string> list = new List<string>();
            for (int i = 0; i < arrParam.Length; i++)
            {
                //string[] iWc = arrParam[i].Split(',');
                list.Add("电流值");
                list.Add("误差值");
            }
            list.Add("结论");
            ResultNames = list.ToArray();
            return true;
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
    }
}
