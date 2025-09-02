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
    /// 电能质量模组频率偏差试验
    /// </summary>
    /// 结论数据：测量频率|允许误差|测量值1|测量值2|测量值3|测量值4|测量值5|测量值6|标准值|平均值|绝对误差
    /// 方案参数：频率
    public class FrequencyInfluenceModel : VerifyBase
    {
        float feq = 50f;
        float wcLimit = 0.005f;//误差限
        public override void Verify()
        {
            base.Verify();
            MessageAdd("电能质量模组频率偏差试验检定开始...", EnumLogType.提示信息);
            if (OneMeterInfo.MD_WiringMode == "单相")
            {
                wcLimit = 0.01f;
            }
            else
            {
                wcLimit = 0.005f;
            }
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["允许误差"][j] = "±" + wcLimit.ToString();
                ResultDictionary["测量频率"][j] = feq.ToString();
                //测量频率
            }
            RefUIData("允许误差");
            RefUIData("测量频率");
            //if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            //{
            //    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
            //    return;
            //}
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, feq, "1.0", Cus_PowerPhase.正相序, PowerWay.正向有功))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            //WaitTime("", 60);
            //MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
            //string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("电网频率");
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            //string[] readdata3 = MeterProtocolAdapter.Instance.ReadData("电网频率");

            for (int index = 0; index < 6; index++)
            {
                WaitTime("误差" + (index + 1) + "读取等待", 10);
                string[] readdata = MeterProtocolAdapter.Instance.ReadData("电网频率");

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (readdata[i]== "0.0000")
                    {
                        WaitTime("读取失败，重新读取等待中", 10);
                        readdata = MeterProtocolAdapter.Instance.ReadData("电网频率");
                    }
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ////测量频率|允许误差|测量值|标准值|绝对误差
                    ResultDictionary["测量值" + (index + 1)][i] = readdata[i];
                }
                RefUIData("测量值" + (index + 1));
            }

            var std = EquipmentData.StdInfo.Freq;
            ResultDictionary["标准值"].Fill(std.ToString("F4"));


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                float[] err = new float[6];
                for (int index = 0; index < 6; index++)
                {
                    if (!string.IsNullOrWhiteSpace(ResultDictionary[$"测量值{index + 1}"][i]))
                    {
                        err[index] = float.Parse(ResultDictionary[$"测量值{index + 1}"][i]);
                    }
                }

                float vga = err.Sum() / 6; ;
                ResultDictionary["平均值"][i] = vga.ToString("F4");
                var error = Math.Round(vga - std, 4);
                ResultDictionary["绝对误差"][i] = error.ToString();
                if (Math.Abs(error) <= wcLimit)
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
            }

            RefUIData("平均值");
            RefUIData("标准值");
            RefUIData("绝对误差");
            RefUIData("结论");
        }


        protected override bool CheckPara()
        {
            string str = Test_Value;
            if (string.IsNullOrWhiteSpace(str)) return false;
            feq = float.Parse(str);
            //测量频率|允许误差|测量值|标准值|绝对误差
            ResultNames = new string[] { "测量频率", "允许误差", "测量值1", "测量值2", "测量值3", "测量值4", "测量值5", "测量值6", "标准值", "平均值", "绝对误差", "结论" };
            return true;
        }
    }
}
