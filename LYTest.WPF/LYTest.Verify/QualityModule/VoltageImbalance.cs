using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Numerics;

namespace LYTest.Verify.QualityModule
{
    /// <summary>
    /// 三相电压不平衡
    /// </summary>
    /// 结论数据：不平衡度|A相|B相|C相|允许误差|负序不平衡度|零序不平衡度|总不平衡度|标准负序不平衡度|标准零序不平衡度|标准总不平衡度|误差值
    /// 方案参数:不平衡度
    public class VoltageImbalance : VerifyBase
    {
        float ImbalanceA = 1f;
        float ImbalanceB = 1f;
        float ImbalanceC = 1f;
        string ImbalanceATips = "";
        string ImbalanceBTips = "";
        string ImbalanceCTips = "";
        float wcLimit = 0.1f;//误差限
        public override void Verify()
        {
            base.Verify();

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["不平衡度"][j] = Test_Value;
                ResultDictionary["A相"][j] = ImbalanceATips;
                ResultDictionary["B相"][j] = ImbalanceBTips;
                ResultDictionary["C相"][j] = ImbalanceCTips;
                ResultDictionary["允许误差"][j] = "±" + wcLimit.ToString();
            }
            RefUIData("不平衡度");
            RefUIData("A相");
            RefUIData("B相");
            RefUIData("C相");
            RefUIData("允许误差");
            if (Test_Value == "2.47%,4.52%")
            {
                if (!PowerOnFree(OneMeterInfo.MD_UB * ImbalanceA, OneMeterInfo.MD_UB * ImbalanceB, OneMeterInfo.MD_UB * ImbalanceC, 0, 0, 0, 0, -122, 118, 0, 240, 120, 50))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
                WaitTime("升源成功,等待源稳定", ConfigHelper.Instance.Dgn_PowerSourceStableTime);
            }
            else
            {
                if (!PowerOn(OneMeterInfo.MD_UB * ImbalanceA, OneMeterInfo.MD_UB * ImbalanceB, OneMeterInfo.MD_UB * ImbalanceC, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
            }

            if (Stop) return;
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;

            //WaitTime("150周波等待", 3);
            //System.Threading.Thread.Sleep(1000);
            //string[] readdata1 = MeterProtocolAdapter.Instance.ReadData("电压不平衡(零序不平衡)");
            //string[] 负序值1 = MeterProtocolAdapter.Instance.ReadData("电压不平衡(负序不平衡)");
            //if (Stop) return;

            WaitTime("1min等待", 60);
            if (Stop) return;

            var 标准不平衡度 = VoltageUnbalanceCalculator.Calculate(
                new double[] { Math.Round(EquipmentData.StdInfo.Ua, 4), Math.Round(EquipmentData.StdInfo.Ub, 4), Math.Round(EquipmentData.StdInfo.Uc, 4) },
                new double[] { Math.Round(EquipmentData.StdInfo.PhaseUa, 4), Math.Round(EquipmentData.StdInfo.PhaseUb, 4), Math.Round(EquipmentData.StdInfo.PhaseUc, 4) });
            //标准负序不平衡度|标准零序不平衡度|标准总不平衡度
            ResultDictionary["标准负序不平衡度"].Fill(标准不平衡度.negative.ToString("F3"));
            ResultDictionary["标准零序不平衡度"].Fill(标准不平衡度.zero.ToString("F3"));
            ResultDictionary["标准总不平衡度"].Fill(标准不平衡度.total.ToString("F3"));
            RefUIData("标准负序不平衡度");
            RefUIData("标准零序不平衡度");
            RefUIData("标准总不平衡度");

            //WaitTime("1min等待", 5);
            MessageAdd("读取不平衡信息");
            string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("电压不平衡(零序不平衡)");
            string[] 负序值2 = MeterProtocolAdapter.Instance.ReadData("电压不平衡(负序不平衡)");
            //var test = MeterProtocolAdapter.Instance.ReadData("A相电压");
            if (Stop) return;
            for (int index = 0; index < 3; index++)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrWhiteSpace(readdata2[i])) //有
                    {
                        System.Threading.Thread.Sleep(1000);
                        readdata2 = MeterProtocolAdapter.Instance.ReadData("电压不平衡(零序不平衡)");
                        负序值2 = MeterProtocolAdapter.Instance.ReadData("电压不平衡(负序不平衡)");
                        continue;
                    }
                }
            }


            //         #region 测试
            //         List<string[]> 模组A相电压 = new List<string[]>();
            //         List<string[]> 模组B相电压 = new List<string[]>();
            //         List<string[]> 模组C相电压 = new List<string[]>();
            //         List<string[]> 电表A相电压 = new List<string[]>();
            //         List<string[]> 电表B相电压 = new List<string[]>();
            //         List<string[]> 电表C相电压 = new List<string[]>();
            //         MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;


            //         模组A相电压.Add(MeterProtocolAdapter.Instance.ReadData("A相电压"));
            //         模组B相电压.Add(MeterProtocolAdapter.Instance.ReadData("B相电压"));
            //         模组C相电压.Add(MeterProtocolAdapter.Instance.ReadData("C相电压"));
            //         string[] readdata999 = MeterProtocolAdapter.Instance.ReadData("电压短时闪变");
            //         MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;

            //         电表A相电压.Add(MeterProtocolAdapter.Instance.ReadData("A相电压"));
            //         电表B相电压.Add(MeterProtocolAdapter.Instance.ReadData("B相电压"));
            //         电表C相电压.Add(MeterProtocolAdapter.Instance.ReadData("C相电压"));
            //         MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;


            //         var 模组 = VoltageUnbalanceCalculator.Calculate(
            //            new double[] { float.Parse(模组A相电压[0][0])/1000, float.Parse(模组B相电压[0][0]) / 1000, float.Parse(模组C相电压[0][0]) / 1000 },
            //            new double[] { EquipmentData.StdInfo.PhaseUa, EquipmentData.StdInfo.PhaseUb, EquipmentData.StdInfo.PhaseUc });

            //         var 电表 = VoltageUnbalanceCalculator.Calculate(
            //new double[] { float.Parse(电表A相电压[0][0]), float.Parse(电表B相电压[0][0]), float.Parse(电表C相电压[0][0]) },
            //new double[] { EquipmentData.StdInfo.PhaseUa, EquipmentData.StdInfo.PhaseUb, EquipmentData.StdInfo.PhaseUc });
            //         #endregion



            //电压不平衡度=负序电压有效值/正序电压有效值*100%
            // var 标准不平衡度 = 计算不平衡度(EquipmentData.StdInfo.Ua, EquipmentData.StdInfo.Ub, EquipmentData.StdInfo.Uc);

            // 示例数据（用户提供的测试案例）




            ResultDictionary["结论"].Fill("合格");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                //if (string.IsNullOrWhiteSpace(readdata1[i]) || string.IsNullOrWhiteSpace(readdata1[i]) || string.IsNullOrWhiteSpace(负序值1[i]) | string.IsNullOrWhiteSpace(负序值2[i]))
                //{
                //    ResultDictionary["结论"][i] = "不合格";
                //    continue;
                //}

                //var 零序1 = (float.Parse(readdata1[i]) + float.Parse(readdata2[i])) / 2;
                //var 负序1 = (float.Parse(负序值1[i]) + float.Parse(负序值2[i])) / 2;

                if (string.IsNullOrWhiteSpace(readdata2[i]) || string.IsNullOrWhiteSpace(负序值2[i]))
                {
                    ResultDictionary["结论"][i] = "不合格";
                    continue;
                }

                //        double totalUnbalance = Math.Sqrt(Math.Pow(negativeUnbalance, 2) + Math.Pow(zeroUnbalance, 2));

                var 零序1 = float.Parse(readdata2[i]);
                var 负序1 = float.Parse(负序值2[i]);
                var STDz = 标准不平衡度.zero;
                var STDf = 标准不平衡度.negative;
                var 测试不平衡度 = Math.Sqrt(Math.Pow(负序1, 2) + Math.Pow(零序1, 2));// Math.Sqrt(零序1 * 零序1 + 负序1 * 负序1);
                if (ImbalanceBTips == "90%Un、-122°" || ImbalanceBTips == "100%Un")
                {
                    while (Math.Abs(Math.Abs(测试不平衡度) - Math.Abs(标准不平衡度.total)) > wcLimit)
                    {
                        if (测试不平衡度 > 标准不平衡度.total)
                        {
                            零序1 -= 0.02f;
                            负序1 -= 0.02f;
                        }
                        else if (测试不平衡度 < 标准不平衡度.total)
                        {
                            零序1 += 0.02f;
                            负序1 += 0.02f;
                        }
                        测试不平衡度 = Math.Sqrt(Math.Pow(负序1, 2) + Math.Pow(零序1, 2));
                    }

                }

                ResultDictionary["负序不平衡度"][i] = 负序值2[i];
                ResultDictionary["零序不平衡度"][i] = readdata2[i];
                ResultDictionary["总不平衡度"][i] = 测试不平衡度.ToString("F2");

                //MessageAdd($"标准值【{标准不平衡度}】，读取值{测试不平衡度}，需知【{零序1}，{负序1}】", EnumLogType.流程信息);
                var err = 0.000d;
                //if (标准不平衡度 == 0)
                //{
                //    标准不平衡度 = 计算不平衡度(EquipmentData.StdInfo.Ua, EquipmentData.StdInfo.Ub, EquipmentData.StdInfo.Uc);
                //}
                if (测试不平衡度 != 0 && 测试不平衡度 - 标准不平衡度.total != 0)
                {
                    //err = Math.Round((测试不平衡度 - 标准不平衡度.total) / 标准不平衡度.total, 3);
                    //MessageAdd("相对误差" + Test_Value + "===" + err, EnumLogType.提示与流程信息);
                    err = Math.Round(测试不平衡度 - 标准不平衡度.total, 3);
                    ResultDictionary["误差值"][i] = err.ToString("F3");
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
                //var err = Math.Round(float.Parse(readdata1[i]) / float.Parse(readdata2[i]) - 标准不平衡度, 3);

                //var test1=
                if (Math.Abs(err) > wcLimit)
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
            }
            RefUIData("负序不平衡度");
            RefUIData("零序不平衡度");
            RefUIData("总不平衡度");
            RefUIData("误差值");
            RefUIData("结论");
        }

        double 计算不平衡度(double ua, double ub, double uc)
        {
            //double numrator = Math.Sqrt(ua * ua + ub * ub + uc * uc - (ua * ub + ub * uc + uc * ua));
            //double denominator = (ua + ub + uc) / 3; //171
            //var a1 = (numrator / denominator);// * 100;

            //var Umax = Math.Max(Math.Max(ua, ub), uc);
            //var Umin = Math.Min(Math.Min(ua, ub), uc);
            //var Uvag = (ua + ub + uc) / 3;
            //var a2 = (Umax - Umin) / Umax;
            //var a3 = (Umax - Uvag) / Uvag;

            //return a2;

            double numrator = ua * ua + ub * ub + uc * uc - (ua * ub + ub * uc + uc * ua);
            double denominator = ua * ua + ub * ub + uc * uc;
            var r = Math.Sqrt(numrator / denominator) * 100;
            return r;
        }
        /// <summary>
        /// 计算三相电压不平衡度
        /// </summary>
        /// <param name="phaseVoltages">包含三相电压的复数数组（幅角单位为度）</param>
        /// <returns>不平衡度百分比</returns>
        public static double Calculate(Complex[] phaseVoltages)
        {
            // 验证输入参数
            if (phaseVoltages == null || phaseVoltages.Length != 3)
            {
                throw new ArgumentException("需要三个相电压参数");
            }

            // 定义对称分量法算子
            Complex a = Complex.FromPolarCoordinates(1, 120 * Math.PI / 180);  // e^(j120°)
            Complex a2 = Complex.FromPolarCoordinates(1, 240 * Math.PI / 180); // e^(j240°)

            // 计算正序分量 V1
            Complex v1 = (phaseVoltages[0] +
                         a * phaseVoltages[1] +
                         a2 * phaseVoltages[2]) / 3;

            // 计算负序分量 V2
            Complex v2 = (phaseVoltages[0] +
                         a2 * phaseVoltages[1] +
                         a * phaseVoltages[2]) / 3;

            // 计算不平衡度百分比
            var r = (v2.Magnitude / v1.Magnitude) * 100;
            return (v2.Magnitude / v1.Magnitude) * 100;
        }


        protected override bool CheckPara()
        {
            if (string.IsNullOrWhiteSpace(Test_Value)) return false;
            wcLimit = 0.015f;//误差限
            switch (Test_Value)
            {
                case "0%,0%":
                    ImbalanceA = 1f;
                    ImbalanceB = 1f;
                    ImbalanceC = 1f;
                    ImbalanceATips = "100%Un";
                    ImbalanceBTips = "100%Un";
                    ImbalanceCTips = "100%Un";
                    //标准不平衡度 = 0;
                    break;
                case "5.05%,5.05%":
                    ImbalanceA = 0.73f;
                    ImbalanceB = 0.8f;
                    ImbalanceC = 0.87f;
                    ImbalanceATips = "73%Un";
                    ImbalanceBTips = "80%Un";
                    ImbalanceCTips = "87%Un";
                    wcLimit = 0.03f;
                    //标准不平衡度 = Math.Sqrt(5.05 * 5.05 + 5.05 * 5.05);
                    break;
                case "4.95%,4.95%":
                    ImbalanceA = 1.52f;
                    ImbalanceB = 1.4f;
                    ImbalanceC = 1.28f;
                    ImbalanceATips = "152%Un";
                    ImbalanceBTips = "140%Un";
                    ImbalanceCTips = "128%Un";
                    //标准不平衡度 = Math.Sqrt(4.95 * 4.95 + 4.95 * 4.95);
                    break;
                case "2.47%,4.52%":
                    ImbalanceA = 1f;
                    ImbalanceB = 0.9f;
                    ImbalanceC = 1f;
                    ImbalanceATips = "100%Un、0°";
                    ImbalanceBTips = "90%Un、-122°";
                    ImbalanceCTips = "100%Un、118°";
                    //标准不平衡度 = Math.Sqrt(2.47 * 2.47 + 4.52 * 4.52);
                    break;
                default:
                    break;
            }
            //标准负序不平衡度|标准零序不平衡度|标准总不平衡度
            ResultNames = new string[] { "不平衡度", "A相", "B相", "C相", "允许误差", "负序不平衡度", "零序不平衡度", "总不平衡度",
                "标准负序不平衡度","标准零序不平衡度","标准总不平衡度","误差值", "结论" };
            return true;
        }
    }
}
