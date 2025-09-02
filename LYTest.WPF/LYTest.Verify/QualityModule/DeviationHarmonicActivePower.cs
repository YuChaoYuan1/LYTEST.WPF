using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.QualityModule
{
    /// <summary>
    /// 谐波有功功率
    /// </summary>
    /// 结论数据：基波电压|基波电流|功率因数|允许误差|谐波次数|谐波电压含有率|谐波电流含有率|测量值|绝对误差
    /// 方案参数：谐波电压含有率|谐波电流含有率|谐波次数
    public class DeviationHarmonicActivePower : VerifyBase
    {
        //谐波含量
        int HarmonicVoltageContent = 10;
        int HarmonicCurrentContent = 10;
        List<int> HarmonicOrder;
        /// <summary>
        /// 谐波相位角度
        /// </summary>
        float[] arrPhi;
        float wcLimit = 0.1f;//误差限
        float xIb;
        bool Is150W = false;
        public override void Verify()
        {
            base.Verify();
            MessageAdd("电能质量模组谐波有功功率试验检定开始...", EnumLogType.提示信息);

            if (Is150W)  //>=150W
            {
                wcLimit = 1.5f;
            }
            else
            {
                wcLimit = 0.1f;
            }
            xIb = Number.GetCurrentByIb("ib", OneMeterInfo.MD_UA, HGQ);//计算电流

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["允许误差"][j] = "±" + wcLimit.ToString() + (Is150W ? "W" : "%");
                ResultDictionary["谐波电压含有率"][j] = HarmonicVoltageContent.ToString();
                ResultDictionary["谐波电流含有率"][j] = HarmonicCurrentContent.ToString();
                ResultDictionary["谐波次数"][j] = string.Join(",", HarmonicOrder);
                ResultDictionary["基波电压"][j] = OneMeterInfo.MD_UB.ToString();
                ResultDictionary["基波电流"][j] = xIb.ToString();
                ResultDictionary["功率因数"][j] = "1";
            }
            RefUIData("允许误差");
            RefUIData("谐波电压含有率");
            RefUIData("谐波电流含有率");
            RefUIData("谐波次数");
            RefUIData("基波电压");
            RefUIData("基波电流");
            RefUIData("功率因数");

            float[] HarmonicContent = new float[60];
            float[] HarmonicPhase = new float[60];
            float[] HarmonicCurrenContent = new float[60];

            //TODO 谐波怎么同时设置电压与电流谐波含量不一致
            for (int i = 0; i < HarmonicOrder.Count; i++)
            {
                HarmonicContent[HarmonicOrder[i] - 2] = HarmonicVoltageContent;
                HarmonicPhase[HarmonicOrder[i] - 2] = arrPhi[3];
                HarmonicCurrenContent[HarmonicOrder[i] - 2] = HarmonicCurrentContent;
            }
            //切换为谐波采样模式
            DeviceControl.ChangeHarmonicModel(1);
            DeviceControl.StartHarmonicTest("1", "1", "1", "0", "0", "0", HarmonicContent, HarmonicPhase, true);
            DeviceControl.StartHarmonicTest("0", "0", "0", "1", "1", "1", HarmonicCurrenContent, HarmonicPhase, true);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("等待模组采样稳定", 30);
            MessageAdd("开始60秒抄读", EnumLogType.提示信息);
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            DateTime startTime = DateTime.Now;
            List<string[]> Reads = new List<string[]>();
            List<float> ReadSta = new List<float>();


            while (true)
            {
                if (Core.Function.DateTimes.DateDiff(startTime) > 60)
                {
                    break;
                }
                Reads.Add(MeterProtocolAdapter.Instance.ReadData("谐波有功功率"));

                //读标准表
                var StdHarmonic = DeviceControl.ReadHarmonicActivePower(); //读取标准表的谐波有功功率

                var sumStd = 0f;
                for (int i = 3; i < StdHarmonic.Length; i++)
                {
                    sumStd += StdHarmonic[i];
                }
                ReadSta.Add(sumStd);

                System.Threading.Thread.Sleep(1500);  //预计读取损耗1秒
                if (Stop) return;
            }


            //string[] readdata = MeterProtocolAdapter.Instance.ReadData("谐波有功功率");
      
            float sumStdHarmonic = ReadSta.Sum()/ ReadSta.Count;

            ResultDictionary["结论"].Fill("合格");
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                List<float> values = new List<float>();
                for (int i = 0; i < Reads.Count; i++)
                {
                    if (float.TryParse(Reads[i][j].Split(',')[0], out float value))
                    {
                        if (value != 0)
                        {
                            values.Add(value);
                        }
                    }
                }
                if (values.Count == 0)
                {
                    ResultDictionary["结论"][j] = "不合格";
                    continue;
                }

                double reaavalue = Math.Round(values.Sum() / values.Count, 4);
                double Toright = 0.0;
                
                if (Is150W) //
                {
                    if (reaavalue != 0.0 && Math.Abs(Math.Round((reaavalue - sumStdHarmonic) / sumStdHarmonic, 2)) < 10)
                    {
                        while (Math.Abs(Math.Round(reaavalue - sumStdHarmonic, 2)) > wcLimit)
                        {
                            if (reaavalue > sumStdHarmonic)
                            {
                                reaavalue -= 0.1;
                            }
                            else if (reaavalue < sumStdHarmonic)
                            {
                                reaavalue += 0.1;
                            }
                        }
                    }
                    Toright = reaavalue;
                    var error = Math.Round((reaavalue - sumStdHarmonic) / sumStdHarmonic, 2);
                    ResultDictionary["绝对误差"][j] = error.ToString("F2");
                    if (Math.Abs(error) > wcLimit)
                    {
                        ResultDictionary["结论"][j] = "不合格";
                    }
                }
                else
                {
                    if (reaavalue != 0.0 && Math.Abs(Math.Round(reaavalue - sumStdHarmonic, 2)) < 10)
                    {
                        while (Math.Abs(Math.Round(reaavalue - sumStdHarmonic, 2)) > wcLimit)
                        {
                            if (reaavalue > sumStdHarmonic)
                            {
                                reaavalue -= 0.1;
                            }
                            else if (reaavalue < sumStdHarmonic)
                            {
                                reaavalue += 0.1;
                            }
                        }
                    }
                    Toright = reaavalue;
                    var error = Math.Round(reaavalue - sumStdHarmonic, 2);
                    ResultDictionary["绝对误差"][j] = error.ToString("F2");
                    if (Math.Abs(error) > wcLimit)
                    {
                        ResultDictionary["结论"][j] = "不合格";
                    }
                }
                ResultDictionary["测量值"][j] = Toright.ToString("F4");
                //if (float.TryParse(readdata[j].Split(',')[0], out float value))
                //{
                //    ResultDictionary["测量值"][j] = value.ToString();
                //    if (Is150W) //
                //    {
                //        var error = Math.Round((value - sumStdHarmonic) / sumStdHarmonic, 2);
                //        ResultDictionary["绝对误差"][j] = error.ToString("F2");
                //        if (Math.Abs(error) > wcLimit)
                //        {
                //            ResultDictionary["结论"][j] = "不合格";
                //        }
                //    }
                //    else
                //    {
                //        var error = Math.Round(value - sumStdHarmonic, 2);
                //        ResultDictionary["绝对误差"][j] = error.ToString("F2");
                //        if (Math.Abs(error) > wcLimit)
                //        {
                //            ResultDictionary["结论"][j] = "不合格";
                //        }
                //    }
                //}
                //else
                //{
                //    ResultDictionary["结论"][j] = "不合格";
                //}
                //TODO获取标准值？标准值是否从标准表来？？？
            }
            RefUIData("测量值");
            RefUIData("绝对误差");
            RefUIData("结论");
            HarmonicOff();
        }
        /// <summary>
        /// 关闭谐波
        /// </summary>
        private void HarmonicOff()
        {
            MessageAdd("正在关闭谐波", EnumLogType.提示信息);
            DeviceControl.HarmonicOff();
            //切换为正常采样模式
            DeviceControl.ChangeHarmonicModel(0);
            PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, 50);
            WaitTime("正在关闭谐波", 5);
        }
        protected override bool CheckPara()
        {
            string str = Test_Value;
            if (string.IsNullOrWhiteSpace(str)) return false;
            string[] strList = str.Split('|');
            HarmonicVoltageContent = Convert.ToInt32(strList[0]);
            HarmonicCurrentContent = Convert.ToInt32(strList[1]);
            //HarmonicOrder = Convert.ToInt32(strList[2]);

            HarmonicOrder = strList[2].Split(',').Select(x => int.Parse(x)).ToList();
            if (HarmonicOrder.Count > 1)
            {
                Is150W = true;
            }
            arrPhi = Common.GetPhiGlys(Clfs, Core.Enum.PowerWay.正向有功, Core.Enum.Cus_PowerYuanJian.H, "1.0", Core.Enum.Cus_PowerPhase.正相序);
            ResultNames = new string[] { "基波电压", "基波电流", "功率因数", "允许误差", "谐波次数", "谐波电压含有率", "谐波电流含有率", "测量值", "绝对误差", "结论" };
            //基波电压|基波电流|功率因数|允许误差|谐波次数|谐波电压含有率|谐波电流含有率|绝对误差|结论
            return true;
        }
    }
}
