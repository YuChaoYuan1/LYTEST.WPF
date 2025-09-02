using LYTest.Core.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 电能质量模组频率偏差试验
    /// </summary>
    public class FrequencyInfluenceModel : VerifyBase
    {
        float feq = 50.0f;
        float wcLimit = 0.01f;//误差限
        public override void Verify()
        {
            base.Verify();
            MessageAdd("电能质量模组频率偏差试验检定开始...", EnumLogType.提示信息);
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["误差限%"][j] = "±" + wcLimit.ToString();
            }

            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, feq, "1.0", Cus_PowerPhase.电流逆相序, PowerWay.正向有功))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            CalculationProcess("150周波读取", 3, "F2");
            CalculationProcess("一分钟读取", 60, "F2");
            CalculationProcess("五分钟读取", 300, "F2");
            CalculationProcess("十分钟读取", 600, "F2");
        }

        private void CalculationProcess(string name, int WaitTimeSecond,string DecimalPlaces)
        {
            WaitTime(name, WaitTimeSecond);
            float MeasurementValue = 0.01f;
            LoadResultDictionary("间隔测量值", MeasurementValue, DecimalPlaces);
            LoadResultDictionary("间隔标准值", EquipmentData.StdInfo.Freq, DecimalPlaces);
            LoadResultDictionary("模组频率偏差相对误差", EquipmentData.StdInfo.Freq- MeasurementValue, DecimalPlaces);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">结论名称</param>
        /// <param name="DecimalPlaces">小数位</param>
        private void LoadResultDictionary(string name, float Value, string DecimalPlaces)
        {
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name][j] = ResultDictionary[name][j].ToString() + Value.ToString(DecimalPlaces) + "|";
            }
        }


        protected override bool CheckPara()
        {
            string str = Test_Value;
            if (string.IsNullOrWhiteSpace(str)) return false;
            feq = float.Parse(str);
            ResultNames = new string[] { "误差限%", "间隔测量值", "间隔标准值", "模组频率偏差相对误差", "平均值", "化整值", "结论" };
            return true;
        }
    }
}
