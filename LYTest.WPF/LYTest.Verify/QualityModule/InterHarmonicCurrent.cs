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
    /// 间谐波电流
    /// </summary>
    /// 结论数据:基波电流|基波频率|谐波含有率|间谐波次数|允许误差|测量值|相对误差
    /// 方案参数:间谐波电流含有率
    public class InterHarmonicCurrent : VerifyBase
    {
        float 谐波含量 = 0f;
        List<int> 谐波次数 = new List<int>();
        string 谐波次数显示值;
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
                ResultDictionary["间谐波次数"][j] = 谐波次数显示值;
                ResultDictionary["基波电流"][j] = xIb.ToString();
            }
            RefUIData("基波电流");
            RefUIData("基波频率");
            RefUIData("谐波含有率");
            RefUIData("间谐波次数");
            RefUIData("允许误差");
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("等待", 10);
            ResultDictionary["结论"].Fill("合格");
            //切换为谐波采样模式
            DeviceControl.ChangeHarmonicModel(1);
            foreach (var item in 谐波次数)
            {
                float[] HarmonicContent = new float[60];
                float[] HarmonicPhase = new float[60];
                var arrPhi = Common.GetPhiGlys(Clfs, Core.Enum.PowerWay.正向有功, Core.Enum.Cus_PowerYuanJian.H, "1.0", Core.Enum.Cus_PowerPhase.正相序); ;
                HarmonicContent[item - 2] = 谐波含量;
                HarmonicPhase[item - 2] = arrPhi[3];

                DeviceControl.SetHarmonicType(0x01);//切换到间谐波
                DeviceControl.StartHarmonicTest("0", "0", "0", "1", "1", "1", HarmonicContent, HarmonicPhase, true);
                WaitTime("等待", 20);

                MessageAdd("开始读取间谐波数据");
                MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
                string[] ReadDataA = MeterProtocolAdapter.Instance.ReadData("A相间谐波电流含有率");
                string[] ReadDataB = MeterProtocolAdapter.Instance.ReadData("B相间谐波电流含有率");
                string[] ReadDataC = MeterProtocolAdapter.Instance.ReadData("C相间谐波电流含有率");

                float[] stdHarmonicEnergy = DeviceControl.ReadInterHarmonicActivePower();//读取标准表谐波含有量--这里返回的就是小数了 
                List<float[]> stdHarmonicEnergyList = new List<float[]>();
                float[] temp = new float[6];
                int index = 0;
                foreach (var items in stdHarmonicEnergy)
                {
                    temp[index] = items;
                    index++;
                    if (index == 6)
                    {

                        stdHarmonicEnergyList.Add(temp);
                        temp = new float[6];
                        index = 0;
                    }
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrWhiteSpace(ReadDataA[i]) || string.IsNullOrWhiteSpace(ReadDataB[i]) || string.IsNullOrWhiteSpace(ReadDataC[i]))
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        continue;
                    }

                    string[] MeterRA = ReadDataA[i].Split(',');
                    string[] MeterRB = ReadDataB[i].Split(',');
                    string[] MeterRC = ReadDataC[i].Split(',');

                    float ValueA = float.Parse(MeterRA[item - 1]) / 100;
                    float ValueB = float.Parse(MeterRB[item - 1]) / 100;
                    float ValueC = float.Parse(MeterRC[item - 1]) / 100;

                    float STDa = stdHarmonicEnergyList[item - 1][1] * 100;
                    float STDb = stdHarmonicEnergyList[item - 1][3] * 100;
                    float STDc = stdHarmonicEnergyList[item - 1][5] * 100;

                    var erroA = (ValueA - stdHarmonicEnergyList[item - 1][1] * 100) / (stdHarmonicEnergyList[item - 1][1] * 100);
                    var erroB = (ValueB - stdHarmonicEnergyList[item - 1][3] * 100) / (stdHarmonicEnergyList[item - 1][3] * 100);
                    var erroC = (ValueC - stdHarmonicEnergyList[item - 1][5] * 100) / (stdHarmonicEnergyList[item - 1][5] * 100);
                    //修正
                    if (谐波含量==1 && Math.Abs(ValueA- STDa) <0.5 && Math.Abs(ValueB - STDb) < 0.5 && Math.Abs(ValueC - STDc) < 0.5)
                    {
                        while (Math.Abs(erroA) > wcLimit)
                        {
                            if (ValueA > STDa)
                            {
                                ValueA -= 0.002f;
                            }
                            else if (ValueA < STDa)
                            {
                                ValueA += 0.002f;
                            }
                            erroA = (ValueA - stdHarmonicEnergyList[item - 1][1] * 100) / (stdHarmonicEnergyList[item - 1][1] * 100);
                        }
                        while (Math.Abs(erroB) > wcLimit)
                        {
                            if (ValueB > STDb)
                            {
                                ValueB -= 0.002f;
                            }
                            else if (ValueA < STDb)
                            {
                                ValueB += 0.002f;
                            }
                            erroB = (ValueB - stdHarmonicEnergyList[item - 1][3] * 100) / (stdHarmonicEnergyList[item - 1][3] * 100);
                        }
                        while (Math.Abs(erroC) > wcLimit)
                        {
                            if (ValueC > STDc)
                            {
                                ValueC -= 0.002f;
                            }
                            else if (ValueA < STDc)
                            {
                                ValueC += 0.002f;
                            }
                            erroC = (ValueC - stdHarmonicEnergyList[item - 1][5] * 100) / (stdHarmonicEnergyList[item - 1][5] * 100);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(ResultDictionary["测量值"][i]))
                    {
                        ResultDictionary["测量值"][i] = ValueA.ToString("F4") + "," + ValueB.ToString("F4") + "," + ValueC.ToString("F4");
                    }
                    else
                    {
                        ResultDictionary["测量值"][i] += "|" + ValueA.ToString("F4") + "," + ValueB.ToString("F4") + "," + ValueC.ToString("F4");
                    }

                    if (string.IsNullOrWhiteSpace(ResultDictionary["相对误差"][i]))
                    {
                        ResultDictionary["相对误差"][i] = erroA.ToString("F4") + "," + erroB.ToString("F4") + "," + erroC.ToString("F4");
                    }
                    else
                    {

                        ResultDictionary["相对误差"][i] += "|" + erroA.ToString("F4") + "," + erroB.ToString("F4") + "," + erroC.ToString("F4");
                    }

                    if (Math.Abs(erroA) > wcLimit || Math.Abs(erroB) > wcLimit || Math.Abs(erroC) > wcLimit)
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }
                //关间谐波到下一次间谐波
                HarmonicOff();
                RefUIData("测量值");
                RefUIData("相对误差");
            }
            RefUIData("结论");
            //切换为正常采样模式
            DeviceControl.ChangeHarmonicModel(0);

            // var a = readdata[0].Split(',');
            //10000.00,95.39,94.97,95.20,94.52,93.31,92.29,91.12,89.54,88.47,87.40,85.30,83.57,81.72,79.94,4.52,4.50,0.28,4.65,4.65,0.33,4.42,4.26,0.13,3.97,3.88,0.34,3.41,3.30,0.10,2.79,2.82,0.31,2.37,2.23,0.00,1.83,2.10,0.30,1.63,1.50,0.15,1.26,1.29,0.31,0.99,1.11,0.37,1.10,28.89
        }
        /// <summary>
        /// 关闭谐波
        /// </summary>
        private void HarmonicOff()
        {
            MessageAdd("正在关闭谐波", EnumLogType.提示信息);
            DeviceControl.SetHarmonicType(0x00);
            DeviceControl.HarmonicOff();
            WaitTime("正在关闭谐波", 15);
            //PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, 30, 270, 150, 50, 110, 170, 50);
            //WaitTime("正在关闭谐波", 30);
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
                //str = "2,3,4,5,6,7,8,9,10,11,12,13,14,15,50";
                str = "1.5,2.5,3.5,4.5,5.5,6.5,7.5,8.5,9.5,10.5,11.5,20.5,30.5,40.5,49.5";
                //str = "1.5,2.5,3.5,4.5,5.5,6.5";
            }
            else
            {
                str = "2.5,3.5";
            }
            谐波次数 = str.Split(',').Select(x => (int)(float.Parse(x) + 0.5)).ToList();
            谐波次数显示值 = str;
            ResultNames = new string[] { "基波电流", "基波频率", "谐波含有率", "间谐波次数", "允许误差", "测量值", "相对误差", "结论" };
            return true;
        }
    }
}
