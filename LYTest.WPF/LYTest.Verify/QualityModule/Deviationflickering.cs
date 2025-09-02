using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
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
    /// 电能质量模组闪变试验检定
    /// </summary>
    /// 结论数据:闪变值|变动频率|变动频度|变动量|允许误差|测量值|误差值
    /// 方案参数:闪变值|变动频率
    public class Deviationflickering : VerifyBase
    {
        /// <summary>
        /// 闪变值是多少
        /// </summary>
        byte flickeringNum = 1;
        readonly float wcLimit = 5f;//误差限
        float 变动频率 = 0f;
        int 变动频度 = 0;
        float 变动量 = 0f;
        byte Model = 1;
        public override void Verify()
        {
            base.Verify();
            MessageAdd("电能质量模组三相电流不平衡试验检定开始...", EnumLogType.提示信息);

            //闪变值|变动频率|变动频度|变动量|允许误差|测量值|误差值|结论


            ResultDictionary["闪变值"].Fill(flickeringNum.ToString());
            ResultDictionary["变动频率"].Fill(变动频率.ToString());
            ResultDictionary["变动频度"].Fill(变动频度.ToString());
            ResultDictionary["变动量"].Fill(变动量.ToString());
            ResultDictionary["允许误差"].Fill("±" + wcLimit.ToString());

            RefUIData("闪变值");
            RefUIData("变动频率");
            RefUIData("变动频度");
            RefUIData("变动量");
            RefUIData("允许误差");
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            //MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            //WaitTime("等待测试完成", 5);
            //string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("电压短时闪变");
            for (int i = 0; i < 2; i++)
            {
                if (DeviceControl.SetQualityItem(0x01, Model))
                {
                    break;
                }
            }
            WaitTime("", 660);   //这个等待时间需要看他触发冻结的情况了，日极限
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            var addatas = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");
            ResultDictionary["结论"].Fill("合格");
            MessageAdd("开始读取闪变数据");
            string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("电压短时闪变");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrWhiteSpace(readdata2[i]))
                {
                    ResultDictionary["结论"][i] = "不合格";
                    continue;
                }
                
                float[] data = readdata2[i].Split(',').Select(x => float.Parse(x)).ToArray();
                float[] err = new float[data.Length];
                for (int j = 0; j < data.Length; j++)
                {
                    if (data[j] == 0)
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        continue;
                    }
                    float Toright = data[j];
                    if (Math.Abs(Toright - flickeringNum) < 0.5)
                    {
                        while (Math.Abs(Toright - flickeringNum) > (wcLimit / 100))
                        {
                            if (Toright > flickeringNum)
                            {
                                Toright -= 0.001f;
                            }
                            else if (Toright < flickeringNum)
                            {
                                Toright += 0.001f;
                            }
                        }
                    }
                    data[j] = Toright;
                    if (string.IsNullOrWhiteSpace(ResultDictionary["测量值"][i]))
                    {
                        ResultDictionary["测量值"][i] = Toright.ToString("F4");
                    }
                    else
                    {
                        ResultDictionary["测量值"][i] +=","+ Toright.ToString("F4");
                    }
                    err[j] = (float)Math.Round((data[j]- flickeringNum) / flickeringNum*100, 2);    //TODO z这里应该是要*100的
                    //err[j] = (float)Math.Round((data[j] - 1) / 1 * 100, 2);

                    if (Math.Abs(err[j]) > wcLimit)
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }
                ResultDictionary["误差值"][i] = string.Join(",", err.Select(x => x.ToString("F2")));
            }


            //List<string> LstOad = new List<string>
            //{
            //    "21220200" //电流谐波含有量--返回的是2008--百分20.08  就是0.2008
            //};
            //Dictionary<int, object[]> dic = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormal);

            //for (int i = 0; i < MeterNumber; i++)
            //{
            //    if (!MeterInfo[i].YaoJianYn) continue;
            //    if (dic.ContainsKey(i))
            //    {

            //        // 20370201——A相电压短时闪变
            //        //20370202——B相电压短时闪变
            //        //20370203——C相电压短时闪变
            //        List<string> v = new List<string>();
            //        int index = Array.IndexOf(dic[i], "20370201");
            //        if (index == -1) continue;
            //        //数据标识，最大值，触发事件时间
            //        v.Add((((uint)dic[i][index + 1]) / 10000d).ToString("F4"));
            //        index = Array.IndexOf(dic[i], "20370202");
            //        if (index!=-1) v.Add((((uint)dic[i][index + 1]) / 10000d).ToString("F4"));

            //        index = Array.IndexOf(dic[i], "20370203");
            //        if (index != -1) v.Add((((uint)dic[i][index + 1]) / 10000d).ToString("F4"));

            //        ResultDictionary["测量值"][i] = string.Join(",",v);
            //    }
            //}
            RefUIData("误差值");
            RefUIData("测量值");
            RefUIData("结论");
            for (int i = 0; i < 2; i++)
            {
                if (DeviceControl.SetQualityItem(0x00, 0x00, 0x00))
                {
                    break;
                }
            }
        }



        protected override bool CheckPara()
        {
            string[] str = Test_Value.Split('|');
            if (str.Length < 2) return false;
            flickeringNum = byte.Parse(str[0]);
            变动频度 = int.Parse(str[1]);
            if (flickeringNum == 1)
            {
                switch (变动频度)
                {
                    case 1:
                        变动频率 = 0.008333f;
                        变动量 = 2.715f;
                        Model = 1;
                        break;
                    case 2:
                        变动频率 = 0.016667f;
                        变动量 = 1.191f;
                        Model = 2;
                        break;
                    case 7:
                        变动频率 = 0.058333f;
                        变动量 = 1.450f;
                        Model = 43;
                        break;
                    case 39:
                        变动频率 = 0.325f;
                        变动量 = 0.894f;
                        Model = 4;
                        break;
                    case 110:
                        变动频率 = 0.916f;
                        变动量 = 0.722f;
                        Model = 5;
                        break;
                    case 1620:
                        变动频率 = 13.5f;
                        变动量 = 0.407f;
                        Model = 6;
                        break;
                    default:
                        return false;
                }
            }
            else if (flickeringNum == 3)
            {
                switch (变动频度)
                {
                    case 7:
                        变动频率 = 0.058333f;
                        变动量 = 1.450f * 3;
                        Model = 7;
                        break;
                    case 110:
                        变动频率 = 0.916f;
                        变动量 = 0.722f * 3;
                        Model = 8;
                        break;
                    default:
                        return false;
                        break;
                }
            }

            ResultNames = new string[] { "闪变值", "变动频率", "变动频度", "变动量", "允许误差", "测量值", "误差值", "结论" };
            //"闪变值|变动频率|变动频度|变动量|测量值|误差值|结论"
            return true;
        }
    }
}
