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
    /// 电能质量模组电压偏差试验检定
    /// </summary>
    /// 结论数据：电压|测量间隔|允许误差|测量值|相对误差
    /// 方案参数：电压倍数
    public class DeviationModel : VerifyBase
    {
        //百分之多少的电压
        float SetUn = 1.0f;
        float wcLimit = 0.01f;//误差限
        string 测量间隔 = "";
        public override void Verify()
        {
            base.Verify();
            if (OneMeterInfo.MD_WiringMode == "单相")
            {
                wcLimit = 0.05f;
            }
            else
            {
                wcLimit = 0.01f;
            }
            MessageAdd("电能质量模组电压偏差试验检定开始...", EnumLogType.提示信息);


            WaitTime($"等待", 30);

            List<string> Meterfs = new List<string>();
            #region 读100%情况的值
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            string[] StdReaddataList = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");
            float UASTD = float.Parse(EquipmentData.StdInfo.Ua.ToString("F4"));
            float UBSTD = float.Parse(EquipmentData.StdInfo.Ub.ToString("F4"));
            float UCSTD = float.Parse(EquipmentData.StdInfo.Ua.ToString("F4"));

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn)
                {
                    Meterfs.Add("0.0000,0.0000,0.0000");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(StdReaddataList[j]))
                {
                    ResultDictionary["结论"][j] = "不合格";
                    break;
                }
                string[] StdReaddata = StdReaddataList[j].Split(',');
                string UaStdError = ((UASTD - float.Parse(StdReaddata[0])) / UASTD).ToString("F4");
                string UbStdError = ((UBSTD - float.Parse(StdReaddata[1])) / UBSTD).ToString("F4");
                string UcStdError = ((UCSTD - float.Parse(StdReaddata[2])) / UCSTD).ToString("F4");
                Meterfs.Add(UaStdError + "," + UbStdError + "," + UcStdError);
            }
            #endregion

            float Ub = OneMeterInfo.MD_UB * SetUn;
            //   ResultNames = new string[] { "谐波电压", "测量间隔", "允许误差", "相对误差", "结论" };
            //
            ResultDictionary["谐波电压"].Fill(Ub.ToString());//l= "±" + wcLimit.ToString();
            ResultDictionary["测量间隔"].Fill(测量间隔);//l= "±" + wcLimit.ToString();
            ResultDictionary["允许误差"].Fill("±" + wcLimit.ToString());//l= "±" + wcLimit.ToString();
            RefUIData("谐波电压");
            RefUIData("测量间隔");
            RefUIData("允许误差");

            if (!PowerOn(Ub, Ub, Ub, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime($"等待稳定后读取", 20);


            if (Stop) return;

            // CalculationProcess("150周波读取", 3, "F2");
            //CalculationProcess("一分钟读取", 60, "F2");
            //CalculationProcess("五分钟读取", 300, "F2");
            //CalculationProcess("十分钟读取", 600, "F2");
            ResultDictionary["结论"].Fill("合格");

            string[] names = 测量间隔.Split('|');
            int SumTime = 0;
            foreach (var item in names)
            {
                var time = GetWaitTime(item);
                CalculationProcess999(item, time - SumTime, Meterfs); //减去之前已经等待的时间
                SumTime += GetWaitTime(item);
                if (Stop) return;
            }
            RefUIData("结论");


        }

        //private void CalculationProcess(string name, int Time)
        //{
        //    WaitTime($"【{name}】后读取", Time);
        //    if (Stop) return;
        //    //ABC三相的，单相只有1个？三相三线只有2个？？？
        //    MessageAdd("开始读取误差数据");
        //    //MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
        //    MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
        //    string[] readdata = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");
        //    readdata = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");

        //    //float UaStdError = (float)Math.Round((EquipmentData.StdInfo.Ua - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //    //float UbStdError = (float)Math.Round((EquipmentData.StdInfo.Ub - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //    //float UcStdError = (float)Math.Round((EquipmentData.StdInfo.Uc - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //    float UASTD = float.Parse(EquipmentData.StdInfo.Ua.ToString("F2"));
        //    float UBSTD = float.Parse(EquipmentData.StdInfo.Ub.ToString("F2"));
        //    float UCSTD = float.Parse(EquipmentData.StdInfo.Ua.ToString("F2"));
        //    string s = ((EquipmentData.StdInfo.Ua - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100).ToString("F2");
        //    float UaStdError = float.Parse(((EquipmentData.StdInfo.Ua - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100).ToString("F6"));
        //    float UbStdError = float.Parse(((EquipmentData.StdInfo.Ub - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100).ToString("F6"));
        //    float UcStdError = float.Parse(((EquipmentData.StdInfo.Uc - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100).ToString("F6"));

        //    //-9.98,-10.00,-9.98
        //    MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
        //    var readA = MeterProtocolAdapter.Instance.ReadData("B相电压");
        //    //var a = (198f - 220f) / 220f*100;
        //    //var b = -9.98;
        //    //var c = (a - b) / b * 100;


        //    for (int j = 0; j < MeterNumber; j++)
        //    {
        //        if (!MeterInfo[j].YaoJianYn) continue;
        //        if (string.IsNullOrWhiteSpace(readdata[j]))
        //        {
        //            ResultDictionary["结论"][j] = "不合格";
        //            break;
        //        }


        //        List<float> errors = new List<float>();
        //        var data = readdata[j].Split(',');
        //        ResultDictionary["测量值"][j] = readdata[j];


        //        float[] meterError = new float[data.Length];
        //        for (int i = 0; i < data.Length; i++)
        //        {
        //            if (float.TryParse(data[i], out var e))
        //            {
        //                meterError[i] = float.Parse(((e - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100).ToString("F4"));  //实际偏差
        //                //meterError[i] = float.Parse(((e - UaStdError) / UaStdError * 100).ToString("F2"));  //实际偏差

        //            }
        //            else
        //            {
        //                meterError[i] = 99;
        //            }
        //        }

        //        float[] StaError = new float[data.Length];    //标准偏差
        //        StaError[0] = UaStdError;
        //        if (data.Length == 2)   //三相三
        //        {
        //            StaError[1] = UcStdError;
        //        }
        //        else
        //        {
        //            //三相四
        //            StaError[1] = UbStdError;
        //            StaError[2] = UcStdError;
        //        }
        //        int 百分比 = 1;
        //        for (int i = 0; i < StaError.Length; i++)
        //        {
        //            var rtt = (StaError[i] - meterError[i]) / meterError[i] * 100;
        //            //errors.Add((float)Math.Round((StaError[i] - meterError[i]) / meterError[i] * 百分比, 2));
        //            errors.Add((float)Math.Round((StaError[i] - meterError[i]) * 百分比, 2));
        //            var e2 = ((220 - 198.0258) / 198.0258 - (220 - 198.0538) / 198.0538) / ((220 - 198.0538) / 198.0538);
        //        }
        //        for (int i = 0; i < errors.Count; i++)
        //        {
        //            if (float.IsInfinity(errors[i])) errors[i] = 0;
        //        }
        //        if (string.IsNullOrWhiteSpace(ResultDictionary["相对误差"][j]))
        //        {
        //            ResultDictionary["相对误差"][j] = string.Join(",", errors.Select(x => x.ToString("F2")));
        //        }
        //        else
        //        {
        //            ResultDictionary["相对误差"][j] += "|" + string.Join(",", errors.Select(x => x.ToString("F2")));
        //        }

        //        foreach (var item in errors)
        //        {
        //            if (Math.Abs(item) > wcLimit)
        //            {
        //                ResultDictionary["结论"][j] = "不合格";
        //            }
        //        }
        //    }
        //    for (int k = 0; k < MeterNumber; k++)
        //    {
        //        if (!MeterInfo[k].YaoJianYn) continue;
        //        ResultDictionary["谐波电压"][k] = $"{UASTD},{UBSTD},{UCSTD}";
        //    }
        //    RefUIData("谐波电压");
        //    RefUIData("测量值");
        //    RefUIData("相对误差");
        //}

        private void CalculationProcess999(string name, int Time, List<string> Meterfs)
        {
            WaitTime($"【{name}】后读取", Time);
            if (Stop) return;
            //ABC三相的，单相只有1个？三相三线只有2个？？？
            MessageAdd("开始读取误差数据");

            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            string[] readdata = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                if (string.IsNullOrWhiteSpace(readdata[j]))
                {
                    ResultDictionary["结论"][j] = "不合格";
                    break;
                }
                string[] UALL = Meterfs[j].Split(',');

                List<float> errors = new List<float>();
                var data = readdata[j].Split(',');
                ResultDictionary["测量值"][j] = readdata[j];

                float UASTD = float.Parse(EquipmentData.StdInfo.Ua.ToString("F4"));
                float UBSTD = float.Parse(EquipmentData.StdInfo.Ub.ToString("F4"));
                float UCSTD = float.Parse(EquipmentData.StdInfo.Uc.ToString("F4"));

                string UaStdError = ((UASTD - float.Parse(data[0])) / UASTD).ToString("F4");
                string UbStdError = ((UBSTD - float.Parse(data[1])) / UBSTD).ToString("F4");
                string UcStdError = ((UCSTD - float.Parse(data[2])) / UCSTD).ToString("F4");

                //float[] meterError = new float[data.Length];
                // -0.02619934 -0.02470398

                string ErrUa = ((Math.Abs(float.Parse(UALL[0])) - Math.Abs(float.Parse(UaStdError))) * 100).ToString("F2");
                string ErrUb = ((Math.Abs(float.Parse(UALL[1])) - Math.Abs(float.Parse(UbStdError))) * 100).ToString("F2");
                string ErrUc = ((Math.Abs(float.Parse(UALL[2])) - Math.Abs(float.Parse(UcStdError))) * 100).ToString("F2");
                errors.Add(float.Parse(ErrUa));
                errors.Add(float.Parse(ErrUb));
                errors.Add(float.Parse(ErrUc));
                if (string.IsNullOrWhiteSpace(ResultDictionary["相对误差"][j]))
                {
                    ResultDictionary["相对误差"][j] = ErrUa + "," + ErrUb + "," + ErrUc;
                }
                else
                {
                    ResultDictionary["相对误差"][j] += "|" + ErrUa + "," + ErrUb + "," + ErrUc;
                }
                foreach (var item in errors)
                {
                    if (Math.Abs(item) > wcLimit)
                    {
                        ResultDictionary["结论"][j] = "不合格";
                    }
                }

            }

            RefUIData("测量值");
            RefUIData("相对误差");
        }
        //private void CalculationProcess2(string name, int Time)
        //{
        //    WaitTime($"【{name}】后读取", Time);
        //    if (Stop) return;
        //    //ABC三相的，单相只有1个？三相三线只有2个？？？
        //    MessageAdd("开始读取误差数据");
        //    //MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
        //    MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
        //    string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");
        //    string[] readdata = MeterProtocolAdapter.Instance.ReadData("电压分相偏差值数组");    //F2

        //    float UaStdError = (float)Math.Round((EquipmentData.StdInfo.Ua - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //    float UbStdError = (float)Math.Round((EquipmentData.StdInfo.Ub - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //    float UcStdError = (float)Math.Round((EquipmentData.StdInfo.Uc - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //    for (int j = 0; j < MeterNumber; j++)
        //    {
        //        if (!MeterInfo[j].YaoJianYn) continue;
        //        if (string.IsNullOrWhiteSpace(readdata[j]))
        //        {
        //            ResultDictionary["结论"][j] = "不合格";
        //            break;
        //        }
        //        List<float> errors = new List<float>();
        //        var data = readdata[j].Split(',');

        //        float[] meterError = new float[data.Length];
        //        for (int i = 0; i < data.Length; i++)
        //        {
        //            if (float.TryParse(data[i], out var e))
        //            {
        //                meterError[i] = e;
        //            }
        //            else
        //            {
        //                meterError[i] = 99;
        //            }
        //        }
        //        //标准偏差
        //        data = readdata2[j].Split(',');
        //        float[] StaError = new float[data.Length];
        //        for (int i = 0; i < data.Length; i++)
        //        {
        //            if (float.TryParse(data[i], out var e))
        //            {
        //                //StaError[i] = (float)Math.Round((e - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100, 2);
        //                StaError[i] = (e - OneMeterInfo.MD_UB) / OneMeterInfo.MD_UB * 100;
        //            }
        //        }
        //        int 百分比 = 1;

        //        for (int i = 0; i < StaError.Length; i++)
        //        {
        //            errors.Add((float)Math.Round((StaError[i] - meterError[i]) / meterError[i] * 百分比, 2));
        //        }
        //        for (int i = 0; i < errors.Count; i++)
        //        {
        //            if (float.IsInfinity(errors[i])) errors[i] = 0;
        //        }
        //        if (string.IsNullOrWhiteSpace(ResultDictionary["相对误差"][j]))
        //        {
        //            ResultDictionary["相对误差"][j] = string.Join(",", errors.Select(x => x.ToString("F2")));
        //        }
        //        else
        //        {
        //            ResultDictionary["相对误差"][j] += "|" + string.Join(",", errors.Select(x => x.ToString("F2")));
        //        }

        //        foreach (var item in errors)
        //        {
        //            if (Math.Abs(item) > wcLimit)
        //            {
        //                ResultDictionary["结论"][j] = "不合格";
        //            }
        //        }
        //    }
        //    RefUIData("相对误差");
        //    RefUIData("相对误差");
        //}
        private int GetWaitTime(string name)
        {
            int time = 0;
            switch (name)
            {
                case "150周波":
                    time = 3;
                    break;
                case "1min":
                    time = 60;
                    break;
                case "5min":
                    time = 300;
                    break;
                case "10min":
                    time = 600;
                    break;
                default:
                    break;
            }
            return time;
        }


        //private void CalculationProcess2(string name, int WaitTimeSecond, string DecimalPlaces)
        //{

        //    WaitTime(name, WaitTimeSecond);
        //    string[] readdata = MeterProtocolAdapter.Instance.ReadData("电压分相偏差值数组");
        //    //string[] readdata = MeterProtocolAdapter.Instance.ReadData("电压数据块");
        //    float UaStdError = float.Parse(((float.Parse((OneMeterInfo.MD_UB * SetUn - EquipmentData.StdInfo.Ua).ToString("f4")) / OneMeterInfo.MD_UB * SetUn) * 100).ToString("F4"));
        //    float UbStdError = float.Parse(((float.Parse((OneMeterInfo.MD_UB * SetUn - EquipmentData.StdInfo.Ub).ToString("f4")) / OneMeterInfo.MD_UB * SetUn) * 100).ToString("F4"));
        //    float UcStdError = float.Parse(((float.Parse((OneMeterInfo.MD_UB * SetUn - EquipmentData.StdInfo.Uc).ToString("f4")) / OneMeterInfo.MD_UB * SetUn) * 100).ToString("F4"));
        //    LoadResultDictionary("间隔测量值", readdata);
        //    string UStdError = UaStdError.ToString("F4") + "," + UbStdError.ToString("F4") + "," + UcStdError.ToString("F4");
        //    LoadStdInfoResultDictionary("间隔标准值", UStdError);
        //    LoadErrorResultDictionary("模组电压偏差相对误差", readdata, UaStdError, UbStdError, UcStdError);
        //    RefUIData("间隔测量值");
        //    RefUIData("间隔标准值");
        //    RefUIData("模组电压偏差相对误差");
        //}

        ///// <summary>
        ///// 间隔测量值
        ///// </summary>
        ///// <param name="name">结论名称</param>
        ///// <param name="DecimalPlaces">小数位</param>
        //private void LoadResultDictionary(string name, string[] Value)
        //{
        //    for (int j = 0; j < MeterNumber; j++)
        //    {
        //        if (!MeterInfo[j].YaoJianYn) continue;
        //        ResultDictionary[name][j] = ResultDictionary[name][j] + Value[j] + "|";
        //    }
        //}

        ///// <summary>
        ///// 间隔标准值
        ///// </summary>
        ///// <param name="name">结论名称</param>
        ///// <param name="DecimalPlaces">小数位</param>
        //private void LoadStdInfoResultDictionary(string name, string Value)
        //{
        //    for (int j = 0; j < MeterNumber; j++)
        //    {
        //        if (!MeterInfo[j].YaoJianYn) continue;
        //        ResultDictionary[name][j] = ResultDictionary[name][j] + Value + "|";
        //    }
        //}
        ///// <summary>
        ///// 模组电压偏差相对误差
        ///// </summary>
        ///// <param name="name">结论名称</param>
        ///// <param name="DecimalPlaces">小数位</param>
        //private void LoadErrorResultDictionary(string name, string[] Value, float ErrorStdValueA, float ErrorStdValueB, float ErrorStdValueC)
        //{
        //    for (int j = 0; j < MeterNumber; j++)
        //    {
        //        if (!MeterInfo[j].YaoJianYn) continue;
        //        string[] Values = Value[j].Split(',');

        //        string ErrorA = ((float.Parse(Values[0]) - ErrorStdValueA) / float.Parse(Values[0])).ToString("F6");
        //        string ErrorB = ((float.Parse(Values[1]) - ErrorStdValueB) / float.Parse(Values[1])).ToString("F6");
        //        string ErrorC = ((float.Parse(Values[2]) - ErrorStdValueC) / float.Parse(Values[2])).ToString("F6");

        //        ResultDictionary[name][j] = ResultDictionary[name][j] + ErrorA + "," + ErrorB + "," + ErrorC + "," + "|";
        //    }
        //}

        protected override bool CheckPara()
        {
            //string[] str = Test_Value.Split('|');
            //if (str.Length < 2) return false;

            if (int.TryParse(Test_Value.Replace("%Un", ""), out int Un))
            {
                SetUn = (float)Un / 100;
            }
            else
            {
                return false;
            }
            测量间隔 = "150周波|1min|5min|10min";
            //测量间隔 = "150周波";

            //电压倍数|测量间隔|允许误差|相对误差
            ResultNames = new string[] { "测量电压", "测量间隔", "允许误差", "测量值", "相对误差", "结论" };

            //ResultNames = new string[] { "误差限", "间隔测量值", "间隔标准值", "模组电压偏差相对误差", "平均值", "化整值", "结论" };
            return true;
        }
    }
}
