using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 谐波测量准确度
    /// </summary>
    public class HarmonicMeasureAccuracy : VerifyBase
    {
        float xib = 1f;
        /// <summary>
        /// 第几次
        /// </summary>
        int Frequency = 2;
        /// <summary>
        /// 谐波电流
        /// </summary>
        float HarmonicXib;
        /// <summary>
        /// 谐波电压
        /// </summary>
        float HarmonicUb;
        /// <summary>
        /// 谐波功率因数
        /// </summary>
        string HarmonicGLYS;
        /// <summary>
        /// 谐波相位角度
        /// </summary>
        float[] arrPhi;
        /// <summary>
        ///电压误差限
        /// </summary>
        float ErrorLimitUB;
        ///电流
        /// </summary>
        float ErrorLimitIB;
        public override void Verify()
        {
            base.Verify();
            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["电压误差限"][i] = ErrorLimitUB.ToString();
                ResultDictionary["电流误差限"][i] = ErrorLimitIB.ToString();
            }
            RefUIData("电压误差限");
            RefUIData("电流误差限");


            //ConnectIOTMeter()
            //PowerOff();
            //WaitTime("正在关闭电流", 5);
            //if (Stop) return;

            MessageAdd("正在设置谐波含量", EnumLogType.提示信息);
            SetHarmonic();
            if (Stop) return;

            MessageAdd("正在升源", EnumLogType.提示信息);
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
            if (Stop) return;

            //WaitTime("等待电表正常运行",10);
            //if (Stop) return;

            //MessageAdd("正在判断电能表连接状态", EnumLogType.提示信息);
            //ConnectIOTMeter();
            if (Stop) return;

            MessageAdd("正在读取标准表谐波含量", EnumLogType.提示信息);
            float[] stdHarmonicEnergy = DeviceControl.ReadHarmonicEnergy();//读取标准表谐波含有量--这里返回的就是小数了

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["标准谐波电压含量"][i] = stdHarmonicEnergy[(Frequency - 1) * 6].ToString("F4");//A相电压的
                ResultDictionary["标准谐波电流含量"][i] = stdHarmonicEnergy[(Frequency - 1) * 6 + 1].ToString("F4");//A相电流的
            }
            RefUIData("标准谐波电压含量");
            RefUIData("标准谐波电流含量");
            if (Stop) return;

            MessageAdd("正在读取表内电压谐波含量", EnumLogType.提示信息);
            List<string> LstOad2 = new List<string>
            {
                "200d0200" //电流谐波含有量--返回的是2008--百分20.08  就是0.2008
            };
            Dictionary<int, object[]> dic2 = MeterProtocolAdapter.Instance.ReadData(LstOad2, LYTest.MeterProtocol.Protocols.DLT698.Enum.EmSecurityMode.ClearText, LYTest.MeterProtocol.Protocols.DLT698.Enum.EmGetRequestMode.GetRequestNormalList);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!dic2.ContainsKey(i) || dic2[i].Length <= i)
                {
                    ResultDictionary["电表谐波电压含量"][i] = "";
                    continue;
                }
                if (dic2[i][Frequency - 1] == null || dic2[i][Frequency - 1].ToString() == "")
                {
                    ResultDictionary["电表谐波电压含量"][i] = "";
                }
                else
                {
                    ResultDictionary["电表谐波电压含量"][i] = (float.Parse(dic2[i][Frequency - 1].ToString()) / 10000).ToString("F4");//A相电压的
                }
            }
            RefUIData("电表谐波电压含量");
            if (Stop) return;


            MessageAdd("正在读取表内电流谐波含量", EnumLogType.提示信息);
            List<string> LstOad = new List<string>
            {
                "200e0200" //电流谐波含有量--返回的是2008--百分20.08  就是0.2008
            };
            Dictionary<int, object[]> dic = MeterProtocolAdapter.Instance.ReadData(LstOad, LYTest.MeterProtocol.Protocols.DLT698.Enum.EmSecurityMode.ClearText, LYTest.MeterProtocol.Protocols.DLT698.Enum.EmGetRequestMode.GetRequestNormalList);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!dic.ContainsKey(i) || dic[i].Length <= i)
                {
                    ResultDictionary["电表谐波电流含量"][i] = "";
                    continue;
                }
                if (dic[i][Frequency - 1] == null || dic[i][Frequency - 1].ToString() == "")
                {
                    ResultDictionary["电表谐波电流含量"][i] = "";
                }
                else
                {
                    ResultDictionary["电表谐波电流含量"][i] = (float.Parse(dic[i][Frequency - 1].ToString()) / 10000).ToString("F4");//A相电压的
                }
            }
            RefUIData("电表谐波电流含量");
            if (Stop) return;


            //这里判断合不合格
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                //直接从字典取这个值--免得后面出问题在修改
                // "标准谐波电压含量"
                //"标流谐波电压含量"
                //电表谐波电压含量
                //电表谐波电流含量
                if (ResultDictionary["标准谐波电压含量"][i] == "" || ResultDictionary["电表谐波电压含量"][i] == "")
                {
                    ResultDictionary["结论"][i] = "不合格";
                    continue;
                }
                float value1 = float.Parse(ResultDictionary["标准谐波电压含量"][i]);
                float value2 = float.Parse(ResultDictionary["电表谐波电压含量"][i]);
                float resoult = value1 - value2;
                ResultDictionary["电压误差"][i] = resoult.ToString("F4");
                if (resoult > ErrorLimitIB || resoult < -ErrorLimitUB)
                {
                    ResultDictionary["结论"][i] = "不合格";
                }

                if (ResultDictionary["标准谐波电流含量"][i] == "" || ResultDictionary["电表谐波电流含量"][i] == "")
                {
                    ResultDictionary["结论"][i] = "不合格";
                    continue;
                }
                value1 = float.Parse(ResultDictionary["标准谐波电流含量"][i]);
                value2 = float.Parse(ResultDictionary["电表谐波电流含量"][i]);
                resoult = value1 - value2;
                ResultDictionary["电流误差"][i] = resoult.ToString("F4");
                if (resoult > ErrorLimitIB || resoult < -ErrorLimitUB)
                {
                    ResultDictionary["结论"][i] = "不合格";
                }

                if (ResultDictionary["结论"][i] != "不合格")
                {
                    ResultDictionary["结论"][i] = "合格";
                }
            }
            RefUIData("电压误差");
            RefUIData("电流误差");
            RefUIData("结论");



            if (Stop) return;
            HarmonicOff();
            if (Stop) return;

            MessageAdd("正在升源", EnumLogType.提示信息);
            PowerOn();
            WaitTime("正在升源", 10);
            if (Stop) return;

            //MessageAdd("正在判断电能表连接状态", EnumLogType.提示信息);
            //ConnectIOTMeter();
        }

        /// <summary>
        /// 启动谐波
        /// </summary>
        private void SetHarmonic()
        {
            //TODO 目前只用A限--后续需要三相把这里循环次数改成3就好--注意的是三相的情况电能表谐波总含量需要重新计算--标准表的谐波含量需要三相加起来
            for (int j = 0; j < 1; j++)  //相线  0单相--1b相-2c相
            {
                if (Clfs == Core.Enum.WireMode.单相 && j > 0) continue; //单相只需要a相的
                if (Clfs == Core.Enum.WireMode.三相三线 && j == 1) continue; //三相三线的只需要b相的
                for (int i = 0; i < 2; i++) //电压还是电流
                {
                    float[] HarmonicContent = new float[60];
                    float[] HarmonicPhase = new float[60];
                    if (i == 0)
                    {
                        HarmonicContent[Frequency - 2] = HarmonicUb / OneMeterInfo.MD_UB * 100;//谐波电流/标准电流=谐波含量
                    }
                    else
                    {
                        HarmonicContent[Frequency - 2] = HarmonicXib / xib * 100;//谐波电流/标准电流=谐波含量
                    }
                    HarmonicPhase[Frequency - 2] = arrPhi[i * 3 + j];//这里是相位角度--需要换算

                    //这里是因为一次只能设置一个的
                    string[] value = new string[6];
                    value.Fill("0");
                    value[i * 3 + j] = "1";
                    DeviceControl.StartHarmonicTest(value[0], value[1], value[2], value[3], value[4], value[5], HarmonicContent, HarmonicPhase, true);

                }
            }

            PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 30, 270, 150, 50, 110, 170, 50);
            WaitTime("正在开启谐波", 6);

        }

        private void HarmonicOff()
        {
            MessageAdd("正在关闭谐波", EnumLogType.提示信息);
            DeviceControl.HarmonicOff();
            PowerOnFree(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, 30, 270, 150, 50, 110, 170, 50);
            WaitTime("正在关闭谐波", 6);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            try
            {
                //参数--第几次--谐波电压--谐波电流--谐波功率因数
                string[] data = Test_Value.Split('|');
                xib = Number.GetCurrentByIb("ib", OneMeterInfo.MD_UA, HGQ);
                Frequency = int.Parse(data[3].TrimStart('第').TrimEnd('次'));
                float value = float.Parse(data[0].TrimEnd("Unom".ToCharArray()));
                HarmonicUb = OneMeterInfo.MD_UB * value;
                HarmonicXib = Number.GetCurrentByIb(data[1], OneMeterInfo.MD_UA, HGQ);
                HarmonicGLYS = data[2];

                ResultNames = new string[] { "电压误差限", "电流误差限", "标准谐波电压含量", "标准谐波电流含量", "电表谐波电压含量", "电表谐波电流含量", "电压误差", "电流误差", "结论" };
                arrPhi = Common.GetPhiGlys(Clfs, Core.Enum.PowerWay.正向有功, Core.Enum.Cus_PowerYuanJian.H, HarmonicGLYS, Core.Enum.Cus_PowerPhase.正相序);

                //计算误差线
                ErrorLimitUB = OneMeterInfo.MD_UB * 0.15f;
                if (value >= 0.03)
                {
                    ErrorLimitUB = value * 5f;
                }
                value = HarmonicXib / xib;
                ErrorLimitIB = xib * 0.5f;
                if (value >= 0.1)
                {
                    ErrorLimitIB = value * 5f;
                }

            }
            catch (Exception ex)
            {
                MessageAdd("参数验证失败\r\n" + ex.ToString(), EnumLogType.错误信息);
                return false;
            }
            return true;
        }



    }
}
