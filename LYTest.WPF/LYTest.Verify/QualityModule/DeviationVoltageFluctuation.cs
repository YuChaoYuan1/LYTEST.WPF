using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LYTest.Verify.QualityModule
{

    /// <summary>
    /// 电能质量模组电压波动试验检定 
    /// </summary>
    /// 结论数据:波动模式|允许误差|标准波动量|最大波动量|标准波动时间|波动时间|时间偏差|误差值
    /// 方案参数:电压波动模式
    public class DeviationVoltageFluctuation : VerifyBase
    {
        float wcLimit = 5.0f;//误差限

        byte Model = 0;
        float 标准波动量;
        //上下波动20ms'
        float 标准波动时间;
        public override void Verify()
        {
            base.Verify();
            MessageAdd("电能质量模组电压波动试验检定开始...", EnumLogType.提示信息);
            ResultDictionary["允许误差"].Fill("±" + wcLimit.ToString());
            ResultDictionary["波动模式"].Fill(Test_Value);
            ResultDictionary["标准波动量"].Fill(标准波动量 + "%");
            ResultDictionary["标准波动时间"].Fill(标准波动时间.ToString());

            RefUIData("允许误差");
            RefUIData("标准波动量");
            RefUIData("标准波动时间");
            RefUIData("波动模式");


            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("", 3);
            for (int i = 0; i < 3; i++)
            {
                if (DeviceControl.SetQualityItem(0x00, Model))
                {
                    break;
                }
            }
            WaitTime("等待测试完成", 30);
            if (Stop) return;
            //List<float> 标准值 = new List<float>();
            //标准值.Add(EquipmentData.StdInfo.Ua / OneMeterInfo.MD_UB*100);
            //if (OneMeterInfo.MD_WiringMode == "三相三线")
            //{
            //    标准值.Add(EquipmentData.StdInfo.Uc / OneMeterInfo.MD_UB*100);
            //}
            //else
            //{
            //    标准值.Add(EquipmentData.StdInfo.Ub / OneMeterInfo.MD_UB*100);
            //    标准值.Add(EquipmentData.StdInfo.Uc / OneMeterInfo.MD_UB*100);
            //}

            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            ResultDictionary["结论"].Fill("合格");
            MessageAdd("开始读取波动数据");
            string[] readdata = MeterProtocolAdapter.Instance.ReadData("电压波动量");
            
            //9.4134,0.2440,9.7478,0.2470,9.1181,0.2450
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrWhiteSpace(readdata[i]))
                {
                    ResultDictionary["结论"][i] = "不合格";
                    continue;
                }
                //ResultDictionary["测量值"][i] = readdata[i];
                float[] data = readdata[i].Split(',').Select(x => float.Parse(x)).ToArray();
                int count = data.Length / 2;
                float[] MaxData = new float[count];
                float[] MaxTime = new float[count];
                float[] TimeErr = new float[count];
                float[] DataErr = new float[count];
                //9.4134,0.2440,9.7478,0.2470,9.1181,0.2450
                for (int j = 0; j < count; j++)
                {
                    MaxData[j] = data[j * 2];
                    MaxTime[j] = data[j * 2+1]*10000;

                    DataErr[j] = (float)Math.Round(MaxData[j] - 标准波动量, 2);  //最大波动量
                    TimeErr[j] =MaxTime[j] - 标准波动时间 ;  //最大波动量
                    //err[j] = (float)Math.Round((100 - data[i] - 标准值[j]) / 标准值[j], 2);    //TODO z这里应该是要*100的
                    if (Math.Abs(DataErr[j]) > wcLimit)
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }
                ResultDictionary["最大波动量"][i] = string.Join(",", MaxData.Select(x => x.ToString("F2")));
                ResultDictionary["波动时间"][i] = string.Join(",", MaxTime.Select(x => x.ToString("F2")));
                ResultDictionary["误差值"][i] = string.Join(",", DataErr.Select(x => x.ToString("F2")));
                ResultDictionary["时间偏差"][i] = string.Join(",", TimeErr);
            }

            RefUIData("最大波动量");
            RefUIData("时间偏差");
            RefUIData("波动时间");
            RefUIData("误差值");
            RefUIData("结论");
            for (int i = 0; i < 2; i++)
            {
                if (DeviceControl.SetQualityItem(0x00, 0x00, 0x00))
                {
                    break;
                }
            }
        }
        private Dictionary<int, Dictionary<string, List<object>>> 读取冻结数据(string oada)
        {
            List<string> oad = new List<string> { oada };
            Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            List<string> rcsd = new List<string>();
            return MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
        }
        protected override bool CheckPara()
        {
            if (Test_Value.EndsWith("94%Un"))
            {
                Model = 3;
                标准波动量 = 9f;
                标准波动时间 = 2460f;//
            }
            else if (Test_Value.EndsWith("97%Un"))
            {
                Model = 2;
                标准波动量 = 7f;
                标准波动时间 = 490f;//
            }
            else if (Test_Value.EndsWith("93%Un"))
            {
                Model = 1;
                标准波动量 = 7f;
                标准波动时间 = 630f;//
            }
            ResultNames = new string[] { "波动模式", "允许误差", "标准波动量", "最大波动量", "标准波动时间", "波动时间","时间偏差", "误差值", "结论" };
            return true;
        }
    }
    //List<string> LstOad = new List<string>
    //{
    //    "21220200" //电流谐波含有量--返回的是2008--百分20.08  就是0.2008
    //};
    //Dictionary<int, object[]> dic = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormal);


    //20350201——A相电压波动量
    //20350202——B相电压波动量
    //20350203——C相电压波动量


    // 20370201——A相电压短时闪变
    //20370202——B相电压短时闪变
    //20370203——C相电压短时闪变
}
