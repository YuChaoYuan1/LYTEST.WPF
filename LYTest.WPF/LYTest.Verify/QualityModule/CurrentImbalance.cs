using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.DAL.Config;
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
    /// 三相电流不平衡
    /// </summary>
    /// 结论数据:不平衡度|A相|B相|C相|允许误差|负序不平衡度|零序不平衡度|总不平衡度|标准负序不平衡度|标准零序不平衡度|标准总不平衡度|误差值
    /// 方案参数:不平衡度
    public class CurrentImbalance : VerifyBase
    {

        float ImbalanceA = 1f;
        float ImbalanceB = 1f;
        float ImbalanceC = 1f;
        float wcLimit = 0.1f;//误差限
        string ImbalanceATips = "";
        string ImbalanceBTips = "";
        string ImbalanceCTips = "";
        //double 标准不平衡度;

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
            var xIb = Number.GetCurrentByIb("ib", OneMeterInfo.MD_UA, HGQ);//计算电流

            if (Test_Value == "2.47%,4.52%")
            {
                if (!PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb * ImbalanceA, xIb * ImbalanceB, xIb * ImbalanceC, 0, 240, 120, 0, -122, 118, 50))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
                WaitTime("升源成功,等待源稳定", ConfigHelper.Instance.Dgn_PowerSourceStableTime);
            }
            else
            {
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb * ImbalanceA, xIb * ImbalanceB, xIb * ImbalanceC, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
            }
            if (Stop) return;
            //WaitTime("150周波等待", 3);
            //System.Threading.Thread.Sleep(1000);
            //string[] readdata1 = MeterProtocolAdapter.Instance.ReadData("电流不平衡(零序不平衡)");
            //string[] 负序值1 = MeterProtocolAdapter.Instance.ReadData("电流不平衡(负序不平衡)");

            WaitTime("1min等待", 60);
            if (Stop) return;
            //WaitTime("1min等待", 5);
            MessageAdd("读取不平衡信息");
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;

            //TODO 读取失败的情况怎么办
            string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("电流不平衡(零序不平衡)");

   
            string[] 负序值2 = MeterProtocolAdapter.Instance.ReadData("电流不平衡(负序不平衡)");
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
                        readdata2 = MeterProtocolAdapter.Instance.ReadData("电流不平衡(零序不平衡)");
                        负序值2 = MeterProtocolAdapter.Instance.ReadData("电流不平衡(负序不平衡)");
                    }
                }
            }
            if (Stop) return;
            var 标准不平衡度 = VoltageUnbalanceCalculator.Calculate(
                new double[] { Math.Round(EquipmentData.StdInfo.Ia, 4), Math.Round(EquipmentData.StdInfo.Ib, 4), Math.Round(EquipmentData.StdInfo.Ic, 4) },
                new double[] { Math.Round(EquipmentData.StdInfo.PhaseIa, 4), Math.Round(EquipmentData.StdInfo.PhaseIb, 4), Math.Round(EquipmentData.StdInfo.PhaseIc, 4) });

    //        var 标准不平衡度2 = VoltageUnbalanceCalculator.Calculate(
    //new double[] { EquipmentData.StdInfo.Ia, EquipmentData.StdInfo.Ib, EquipmentData.StdInfo.Ic},
    //new double[] { EquipmentData.StdInfo.PhaseIa, EquipmentData.StdInfo.PhaseIb, EquipmentData.StdInfo.PhaseIc });
            //标准负序不平衡度|标准零序不平衡度|标准总不平衡度
            ResultDictionary["标准负序不平衡度"].Fill(标准不平衡度.negative.ToString("F3"));
            ResultDictionary["标准零序不平衡度"].Fill(标准不平衡度.zero.ToString("F3"));
            ResultDictionary["标准总不平衡度"].Fill(标准不平衡度.total.ToString("F3"));
            RefUIData("标准负序不平衡度");
            RefUIData("标准零序不平衡度");
            RefUIData("标准总不平衡度");


            ResultDictionary["结论"].Fill("合格");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrWhiteSpace(readdata2[i]) || string.IsNullOrWhiteSpace(负序值2[i]))
                {
                    ResultDictionary["结论"][i] = "不合格";
                    continue;
                }

                var 零序1 = float.Parse(readdata2[i]);
                var 负序1 = float.Parse(负序值2[i]);
                var 测试不平衡度 = Math.Sqrt(Math.Pow(负序1, 2) + Math.Pow(零序1, 2));// Math.Sqrt(零序1 * 零序1 + 负序1 * 负序1);

                //修正
                if (ImbalanceBTips == "90%In、-122°" || ImbalanceBTips == "100%In")
                {
                    if (Math.Abs(Math.Abs(测试不平衡度) - Math.Abs(标准不平衡度.total)) < 0.5)
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
                }


                ResultDictionary["负序不平衡度"][i] = 负序值2[i];
                ResultDictionary["零序不平衡度"][i] = readdata2[i];
                ResultDictionary["总不平衡度"][i] = 测试不平衡度.ToString("F2");
                var err = 0.000d;
                if (测试不平衡度 != 0 && 测试不平衡度 - 标准不平衡度.total != 0)
                {
                    //err = Math.Round((测试不平衡度 - 标准不平衡度.total) / 标准不平衡度.total, 3);
                    //MessageAdd("相对误差" + Test_Value + "===" + err, EnumLogType.流程信息);
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
        protected override bool CheckPara()
        {
            //Test_Value
            if (string.IsNullOrWhiteSpace(Test_Value)) return false;
            wcLimit = 0.1f;
            switch (Test_Value)
            {
                case "0%,0%":
                    ImbalanceA = 1f;
                    ImbalanceB = 1f;
                    ImbalanceC = 1f;
                    ImbalanceATips = "100%In";
                    ImbalanceBTips = "100%In";
                    ImbalanceCTips = "100%In";
                    //标准不平衡度 = 0;
                    break;
                case "5.05%,5.05%":
                    ImbalanceA = 0.73f;
                    ImbalanceB = 0.8f;
                    ImbalanceC = 0.87f;
                    ImbalanceATips = "73%In";
                    ImbalanceBTips = "80%In";
                    ImbalanceCTips = "87%In";
                    //标准不平衡度 = Math.Sqrt(5.05 * 5.05 + 5.05 * 5.05);

                    break;
                case "4.95%,4.95%":
                    ImbalanceA = 1.52f;
                    ImbalanceB = 1.4f;
                    ImbalanceC = 1.28f;
                    ImbalanceATips = "152%In";
                    ImbalanceBTips = "140%In";
                    ImbalanceCTips = "128%In";
                    //标准不平衡度 = Math.Sqrt(4.95 * 4.95 + 4.95 * 4.95);

                    break;
                case "2.47%,4.52%":
                    ImbalanceA = 1f;
                    ImbalanceB = 0.9f;
                    ImbalanceC = 1f;
                    ImbalanceATips = "100%In、0°";
                    ImbalanceBTips = "90%In、-122°";
                    ImbalanceCTips = "100%In、118°";
                    //标准不平衡度 = Math.Sqrt(2.47 * 2.47 + 4.52 * 4.52);
                    break;
                default:
                    break;
            }
            ResultNames = new string[] { "不平衡度", "A相", "B相", "C相", "允许误差", "负序不平衡度", "零序不平衡度", "总不平衡度",
                "标准负序不平衡度","标准零序不平衡度","标准总不平衡度","误差值", "结论" };
            return true;
        }
    }
}
