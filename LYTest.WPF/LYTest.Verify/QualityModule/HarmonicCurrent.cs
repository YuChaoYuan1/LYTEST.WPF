using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.QualityModule
{
    /// <summary>
    /// 谐波电流
    /// </summary>
    /// 结论数据：基波电流|基波频率|谐波含有率|谐波次数|允许误差|测量值|相对误差
    /// 方案参数：谐波电流含有率
    public class HarmonicCurrent : VerifyBase
    {
        float 谐波含量 = 0f;
        List<int> 谐波次数 = new List<int>();
        float wcLimit = 0.5f;//误差限
        public override void Verify()
        {
            base.Verify();
            if (谐波含量 == 1)
            {
                wcLimit = 0.015f;
            }
            else
            {
                wcLimit = 0.5f;
            }
            var xIb = Number.GetCurrentByIb("ib", OneMeterInfo.MD_UA, HGQ);//计算电流
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["允许误差"][j] = "±" + wcLimit;
                ResultDictionary["基波频率"][j] = "50";
                ResultDictionary["谐波含有率"][j] = 谐波含量.ToString();
                ResultDictionary["谐波次数"][j] = string.Join(",", 谐波次数);
                ResultDictionary["基波电流"][j] = xIb.ToString();
            }
            RefUIData("基波电流");
            RefUIData("基波频率");
            RefUIData("谐波含有率");
            RefUIData("谐波次数");
            RefUIData("允许误差");


            float[] HarmonicContent = new float[60];
            float[] HarmonicPhase = new float[60];
            //谐波含量 = 1;
            var arrPhi = Common.GetPhiGlys(Clfs, Core.Enum.PowerWay.正向有功, Core.Enum.Cus_PowerYuanJian.H, "1.0", Core.Enum.Cus_PowerPhase.正相序); ;
            for (int i = 0; i < 谐波次数.Count; i++)
            {
                HarmonicContent[谐波次数[i] - 2] = 谐波含量;
                HarmonicPhase[谐波次数[i] - 2] = arrPhi[3];
            }

            //切换为谐波采样模式
            DeviceControl.ChangeHarmonicModel(1);

            DeviceControl.SetHarmonicType(0x00);
            DeviceControl.StartHarmonicTest("0", "0", "0", "1", "1", "1", HarmonicContent, HarmonicPhase, true);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("", 60);
            MessageAdd("开始读取谐波数据");

            //List<string> Names = new List<string>() { "A", "B", "C" };
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            //string[] readA = MeterProtocolAdapter.Instance.ReadData("A相谐波电压含有率");//需要读取ABC三相的哦
            //string[] readB = MeterProtocolAdapter.Instance.ReadData("B相谐波电压含有率");//需要读取ABC三相的哦
            //string[] readC = MeterProtocolAdapter.Instance.ReadData("C相谐波电压含有率");//需要读取ABC三相的哦

            //string[] readdata = MeterProtocolAdapter.Instance.ReadData("谐波电流含有率");
            //string[] readdata = MeterProtocolAdapter.Instance.ReadData("谐波间电压含有率");
            //string[] readdata = MeterProtocolAdapter.Instance.ReadData("谐波间电流含有率");

            List<string[]> ReadData = new List<string[]>();
            List<string> Names = new List<string>();
            ReadData.Add(MeterProtocolAdapter.Instance.ReadData("A相谐波电流含有率"));
            Names.Add("A");
            if (OneMeterInfo.MD_WiringMode == "三相三线")
            {
                ReadData.Add(MeterProtocolAdapter.Instance.ReadData("C相谐波电流含有率"));
                Names.Add("C");
            }
            else
            {
                ReadData.Add(MeterProtocolAdapter.Instance.ReadData("B相谐波电流含有率"));
                ReadData.Add(MeterProtocolAdapter.Instance.ReadData("C相谐波电流含有率"));
                Names.Add("B");
                Names.Add("C");
            }

            MessageAdd("正在读取标准谐波含量", EnumLogType.提示信息);
            float[] stdHarmonicEnergy = DeviceControl.ReadHarmonicEnergy();//读取标准表谐波含有量--这里返回的就是小数了
            ResultDictionary["结论"].Fill("合格");

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                bool T = false;
                foreach (var item in ReadData)
                {
                    if (string.IsNullOrWhiteSpace(item[j]))
                    {
                        ResultDictionary["结论"][j] = "不合格";
                        T = true;
                        continue;
                    }
                }
                if (T) continue;

                List<List<float>> values = new List<List<float>>();   //表的谐波值外层ABC,内层次数
                foreach (var item in ReadData)
                {
                    values.Add(item[j].Split(',').Select(x => float.Parse(x)).ToList());
                }
                List<string> 误差值 = new List<string>();
                List<string> 总测量值 = new List<string>();

                foreach (var item in 谐波次数)
                {
                    //单相必须
                    List<string> 分项测量值 = new List<string>();
                    List<string> err = new List<string>();
                    //            Names.Add("A");
                    for (int i = 0; i < values.Count; i++)        //循环ABC三相
                    {
                        int stdIndex = 0;
                        switch (Names[i])
                        {
                            case "A":
                                stdIndex = (item - 1) * 6 + 1; //标准表谐波电压的，谐波电流全部+1
                                break;
                            case "B":
                                stdIndex = (item - 1) * 6 + 2 + 1;
                                break;
                            case "C":
                                stdIndex = (item - 1) * 6 + 4 + 1;
                                break;
                            default:
                                break;

                        }
                        分项测量值.Add((values[i][item - 1] / 100).ToString("F2"));
                        //var error2= Math.Round((values[i][item - 1] / 100 - stdHarmonicEnergy[stdIndex] * 100) * OneMeterInfo.MD_UB, 2);   //绝对误差
                        var error = (values[i][item - 1] / 100 - stdHarmonicEnergy[stdIndex] * 100) / (stdHarmonicEnergy[stdIndex] * 100); //相对误差
                        err.Add(error.ToString("F2"));
                        if (Math.Abs(error) > wcLimit)
                        {
                            ResultDictionary["结论"][j] = "不合格";
                        }


                    }
                    误差值.Add(string.Join(",", err));
                    总测量值.Add(string.Join(",", 分项测量值));  
                }
                ResultDictionary["测量值"][j] = string.Join("|", 总测量值);
                ResultDictionary["相对误差"][j] = string.Join("|", 误差值);
                //
            }
            RefUIData("测量值");
            RefUIData("相对误差");
            RefUIData("结论");
            HarmonicOff();

            // var a = readdata[0].Split(',');
            //10000.00,95.39,94.97,95.20,94.52,93.31,92.29,91.12,89.54,88.47,87.40,85.30,83.57,81.72,79.94,4.52,4.50,0.28,4.65,4.65,0.33,4.42,4.26,0.13,3.97,3.88,0.34,3.41,3.30,0.10,2.79,2.82,0.31,2.37,2.23,0.00,1.83,2.10,0.30,1.63,1.50,0.15,1.26,1.29,0.31,0.99,1.11,0.37,1.10,28.89
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
            string[] data = Test_Value.Split('|');
            if (data.Length < 1) return false;
            if (float.TryParse(data[0], out var s))
            {
                谐波含量 = s;
            }
            else
            {
                return false;
            }
            string str = "";
            if (谐波含量 == 3)
            {
                str = "2,3,4,5,6,7,8,9,10,11,12,13,14,15,50";
            }
            else
            {
                str = "2,3,4,5,6,7,8";
            }
            谐波次数 = str.Split(',').Select(x => int.Parse(x)).ToList();
            ResultNames = new string[] { "基波电流", "基波频率", "谐波含有率", "谐波次数", "允许误差","测量值", "相对误差", "结论" };
            return true;
        }
    }
}
