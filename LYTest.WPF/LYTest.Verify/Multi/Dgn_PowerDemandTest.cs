using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LYTest.Verify.Multi
{

    /// <summary>
    /// 电量法需量功能试验
    /// 方案：
    /// 结论：需量测量方式验证结论|
    /// </summary>
    class Dgn_PowerDemandTest : VerifyBase
    {
        /// <summary>
        /// 需要周期时间（分钟）,可取值：5，10，15，30，60
        /// </summary>
        int _demandPeriod = 5;

        public override void Verify()
        {
            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["需量测量方式验证结论"][i] = "";
                ResultDictionary["需量缓存空间验证结论"][i] = "";
                ResultDictionary["结论"][i] = "";
            }
            RefUIData("需量测量方式验证结论");
            RefUIData("需量缓存空间验证结论");
            RefUIData("结论");


            //初始化设备
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;


            if (Stop) return;
            MessageAdd("开始需量测量方式验证", EnumLogType.提示与流程信息);
            bool[] result1 = MeterageCheck();
            //bool[] result1 = new bool[MeterNumber];
            //result1.Fill(true);

            if (Stop) return;
            MessageAdd("开始需量缓存空间验证", EnumLogType.提示与流程信息);
            bool[] result2 = StoreCheck();

            //if (Stop) return;
            //MessageAdd("开始示例1验证", EnumLogType.提示与流程信息);
            //bool[] result2 = Demo1Check();


            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo m = MeterInfo[i];
                if (!m.YaoJianYn) continue;

                if (result1[i] && result2[i])
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                else
                    ResultDictionary["结论"][i] = ConstHelper.不合格;

            }

            RefUIData("结论");





        }


        /// <summary>
        /// 走字
        /// </summary>
        /// <param name="pw"></param>
        /// <returns></returns>
        private void Walk(PowerWay pw, int seconds)
        {
            string strGlys = "";
            switch (pw)
            {
                case PowerWay.正向有功:
                    strGlys = "1.0";
                    break;
                case PowerWay.反向有功:
                    strGlys = "-1.0";
                    break;
                case PowerWay.第一象限无功:
                    strGlys = "0.5L";
                    break;
                case PowerWay.第二象限无功:
                    strGlys = "0.8C";
                    break;
                case PowerWay.第三象限无功:
                    strGlys = "-0.8C";
                    break;
                case PowerWay.第四象限无功:
                    strGlys = "-0.5L";
                    break;
            }

            if (Stop) return;
            //float xib = OneMeterInfo.GetIb()[0];
            float xib = Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);  //获取走字的电流
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, pw, strGlys) == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return;
            }
            Thread.Sleep(300);

            WaitTime($"走{pw}电量", seconds);

            if (Stop) return;
            PowerOn();
            //PowerOff();
            Thread.Sleep(10000);
        }

        protected override bool CheckPara()
        {
            ResultNames = new string[] { "需量测量方式验证结论", "需量缓存空间验证结论", "结论" };
            return true;
        }

        /// <summary>
        /// 需量测量方式验证
        /// </summary>
        private bool[] MeterageCheck()
        {
            bool[] results = new bool[MeterNumber];
            results.Fill(false);

            if (Stop) return results;
            Identity();

            if (Stop) return results;
            MessageAdd("校准电表时间", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteDateTime(DateTime.Now);

            if (Stop) return results;
            MessageAdd("读取最大需量周期", EnumLogType.提示与流程信息);


            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
            string[] arrDemand = MeterProtocolAdapter.Instance.ReadData("最大需量周期");

            //if (Stop) return;
            //MessageAdd("设置最大需量周期", EnumLogType.提示与流程信息);
            //MeterProtocolAdapter.Instance.WriteData("最大需量周期", _demandPeriod.ToString());

            if (Stop) return results;
            MessageAdd("发送走字功能", EnumLogType.提示与流程信息);
            StartStdEnergy(31);

            if (Stop) return results;
            MessageAdd("电表清零", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.ClearDemand();


            //if (Stop) return;
            //WaitTime("清零后延时1分钟", (int)(1f * 60));
            //if (Stop) return;
            //MessageAdd("读取起始电量", EnumLogType.提示信息);
            //string[] powerStart = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");

            if (Stop) return results;
            WaitTime("无电流延时1.1分钟", (int)(1.1f * 60));


            if (Stop) return results;
            MessageAdd("正在升电流...", EnumLogType.提示与流程信息);
            float testI = Number.GetCurrentByIb("0.5Imax", OneMeterInfo.MD_UA, HGQ);
            //float testI = Number.GetCurrentByIb("Itr", OneMeterInfo.MD_UA, HGQ);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                return results;
            }

            WaitTime($"正在走字{_demandPeriod - 2.2f}分钟", (int)((_demandPeriod - 2.2f) * 60));

            float p = EquipmentData.StdInfo.P;

            if (Stop) return results;
            MessageAdd("正在降电流...", EnumLogType.提示与流程信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                return results;
            }


            if (Stop) return results;
            WaitTime("无电流延时1.5分钟", (int)(1.5f * 60));


            //读标准标电量kWh
            if (Stop) return results;
            MessageAdd("读取标准表电量", EnumLogType.提示与流程信息);
            float[] bzbdl = ReadEnergy(); //9有功总电量，10无功总电量

            MessageAdd($"标准表电量:有功：{bzbdl[9]}, 无功{bzbdl[10]}", EnumLogType.提示与流程信息);


            if (Stop) return results;
            MessageAdd("读取电表最大需量", EnumLogType.提示与流程信息);
            float[] demandValue = MeterProtocolAdapter.Instance.ReadDemand((byte)PowerWay.正向有功, 0);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                MessageAdd($"电表{i + 1}最大需量:有功：{demandValue[i]}", EnumLogType.提示与流程信息);
            }


            float testIb = Number.GetCurrentByIb("Ib", OneMeterInfo.MD_UA, HGQ);

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                float djs = MeterLevel(meter);

                //float err = Convert.ToSingle(powerEnd[i]) - Convert.ToSingle(powerStart[i]);
                //MessageAdd($"电表{i}内电量:起始：{powerStart[i]}, 结束:{powerEnd[i]},差值:{err}", EnumLogType.提示与流程信息);

                float stmP = bzbdl[9] / _demandPeriod * 60f;  // 标准表有功电能 / （15分* 60分）
                float errlimit = djs + ((0.05f * testIb * meter.MD_UB) / p);

                float errV = (demandValue[i] - stmP) / stmP * 100;
                MessageAdd($"表{i}标准值{stmP}, 表值{demandValue[i]}, 误差值{errV}, 限值:{errlimit}, 等级:{djs}, 电流:{testI}, 测量负载点功率:{p}", EnumLogType.提示与流程信息);

                if (Math.Abs(errV) <= errlimit)
                {
                    ResultDictionary["需量测量方式验证结论"][i] = ConstHelper.合格;
                    results[i] = true;

                }
                else
                {
                    ResultDictionary["需量测量方式验证结论"][i] = ConstHelper.不合格;
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                    results[i] = false;
                }
                MessageAdd($"电表{i}需量测量方式验证:{ResultDictionary["需量测量方式验证结论"][i]}", EnumLogType.提示与流程信息);


            }
            RefUIData("需量测量方式验证结论");
            RefUIData("结论");
            return results;
        }

        /// <summary>
        /// 需量缓存空间验证
        /// </summary>
        /// <returns></returns>
        private bool[] StoreCheck()
        {
            bool[] results = new bool[MeterNumber];
            results.Fill(false);

            if (Stop) return results;
            Identity();

            if (Stop) return results;
            MessageAdd("设置最大需量周期", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteData("最大需量周期", _demandPeriod.ToString());

            if (Stop) return results;
            MessageAdd("需量清零", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.ClearDemand();

            // 2. 需量缓存空间验证
            // 6类分别为：正向有功最大需量，反向有功最大需量，第一象限最大需量，第二象限最大需量，第三象限最大需量，第四象限最大需量
            //(当前)正向有功总最大需量及发生时间，(当前)反向有功总最大需量及发生时间，(当前)第一象限无功总最大需量及发生时间
            if (Stop) return results;
            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");


            if (Stop) return results;
            MessageAdd("读取起始[(当前)反向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerFYstart = MeterProtocolAdapter.Instance.ReadData("(当前)反向有功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取起始[(当前)第一象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX1start = MeterProtocolAdapter.Instance.ReadData("(当前)第一象限无功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取起始[(当前)第二象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX2start = MeterProtocolAdapter.Instance.ReadData("(当前)第二象限无功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取起始[(当前)第三象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX3start = MeterProtocolAdapter.Instance.ReadData("(当前)第三象限无功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取起始[(当前)第四象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX4start = MeterProtocolAdapter.Instance.ReadData("(当前)第四象限无功总最大需量及发生时间");

            if (Stop) return results;
            Walk(PowerWay.正向有功, 60 * (_demandPeriod + 1));
            if (Stop) return results;
            Walk(PowerWay.反向有功, 60 * (_demandPeriod + 1));
            if (Stop) return results;
            Walk(PowerWay.第一象限无功, 60 * (_demandPeriod + 1));
            if (Stop) return results;
            Walk(PowerWay.第二象限无功, 60 * (_demandPeriod + 1));
            if (Stop) return results;
            Walk(PowerWay.第三象限无功, 60 * (_demandPeriod + 1));
            if (Stop) return results;
            Walk(PowerWay.第四象限无功, 60 * (_demandPeriod + 1));

            WaitTime("", 10);

            if (Stop) return results;
            MessageAdd("读取结束[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYend = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");


            if (Stop) return results;
            MessageAdd("读取结束[(当前)反向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerFYend = MeterProtocolAdapter.Instance.ReadData("(当前)反向有功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取结束[(当前)第一象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX1end = MeterProtocolAdapter.Instance.ReadData("(当前)第一象限无功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取结束[(当前)第二象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX2end = MeterProtocolAdapter.Instance.ReadData("(当前)第二象限无功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取结束[(当前)第三象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX3end = MeterProtocolAdapter.Instance.ReadData("(当前)第三象限无功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取结束[(当前)第四象限无功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerXX4end = MeterProtocolAdapter.Instance.ReadData("(当前)第四象限无功总最大需量及发生时间");

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo m = MeterInfo[i];
                if (!m.YaoJianYn) continue;
                results[i] = true;

                MessageAdd($"读取的值{powerZYstart[i]}-{powerZYend[i]}", EnumLogType.提示与流程信息);

                string[] arr0 = powerZYstart[i].Split(',');
                string[] arr1 = powerZYend[i].Split(',');
                if (!float.TryParse(arr0[0], out float f0) || !float.TryParse(arr1[0], out float f1) || f1 <= f0)
                {
                    results[i] = false;
                    MessageAdd($"电表{i}需量缓存空间验证结论ZY:{ResultDictionary["需量缓存空间验证结论"][i]}; {powerZYstart[i]}-{powerZYend[i]}", EnumLogType.提示与流程信息);
                }


                arr0 = powerFYstart[i].Split(',');
                arr1 = powerFYend[i].Split(',');
                if (!float.TryParse(arr0[0], out float f2) || !float.TryParse(arr1[0], out float f3) || f3 <= f2)
                {
                    results[i] = false;
                    MessageAdd($"电表{i}需量缓存空间验证结论FY:{ResultDictionary["需量缓存空间验证结论"][i]}; {powerFYstart[i]}-{powerFYend[i]}", EnumLogType.提示与流程信息);

                }
                arr0 = powerXX1start[i].Split(',');
                arr1 = powerXX1end[i].Split(',');
                if (!float.TryParse(arr0[0], out float f4) || !float.TryParse(arr1[0], out float f5) || f5 <= f4)
                {
                    results[i] = false;
                    MessageAdd($"电表{i}需量缓存空间验证结论XX1:{ResultDictionary["需量缓存空间验证结论"][i]}; {powerXX1start[i]}-{powerXX1end[i]}", EnumLogType.提示与流程信息);

                }
                arr0 = powerXX2start[i].Split(',');
                arr1 = powerXX2end[i].Split(',');
                if (!float.TryParse(arr0[0], out float f6) || !float.TryParse(arr1[0], out float f7) || f7 <= f6)
                {
                    results[i] = false;
                    MessageAdd($"电表{i}需量缓存空间验证结论XX2:{ResultDictionary["需量缓存空间验证结论"][i]}; {powerXX2start[i]}-{powerXX2end[i]}", EnumLogType.提示与流程信息);

                }
                arr0 = powerXX3start[i].Split(',');
                arr1 = powerXX3end[i].Split(',');
                if (!float.TryParse(arr0[0], out float f8) || !float.TryParse(arr1[0], out float f9) || f9 <= f8)
                {
                    results[i] = false;
                    MessageAdd($"电表{i}需量缓存空间验证结论XX3:{ResultDictionary["需量缓存空间验证结论"][i]}; {powerXX3start[i]}-{powerXX3end[i]}", EnumLogType.提示与流程信息);

                }
                arr0 = powerXX4start[i].Split(',');
                arr1 = powerXX4end[i].Split(',');
                if (!float.TryParse(arr0[0], out float f10) || !float.TryParse(arr1[0], out float f11) || f11 <= f10)
                {
                    results[i] = false;
                    MessageAdd($"电表{i}需量缓存空间验证结论XX4:{ResultDictionary["需量缓存空间验证结论"][i]}; {powerXX4start[i]}-{powerXX4end[i]}", EnumLogType.提示与流程信息);
                }

                ResultDictionary["需量缓存空间验证结论"][i] = results[i] ? ConstHelper.合格 : ConstHelper.不合格;

            }

            RefUIData("需量缓存空间验证结论");

            if (Stop) return results;
            _demandPeriod = 15;
            MessageAdd($"设置最大需量周期:{_demandPeriod}", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteData("最大需量周期", _demandPeriod.ToString());


            MessageAdd("需量缓存空间验证结束", EnumLogType.提示与流程信息);

            return results;
        }

        /// <summary>
        /// 示例1
        /// </summary>
        /// <returns></returns>
        private bool[] Demo1Check()
        {
            bool[] results = new bool[MeterNumber];
            results.Fill(false);

            MessageAdd("示例1开始", EnumLogType.提示与流程信息);

            float xib = Number.GetCurrentByIb("Itr", OneMeterInfo.MD_UA, HGQ);  //获取走字的电流
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return results;
            }

            if (Stop) return results;
            Identity(true);


            if (Stop) return results;
            _demandPeriod = 15;
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
            MessageAdd($"设置最大需量周期:{_demandPeriod}", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteData("最大需量周期", _demandPeriod.ToString());

            if (Stop) return results;
            MessageAdd("需量清零", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.ClearDemand();

            DateTime now = DateTime.Now.AddDays(1);
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            if (Stop) return results;
            MessageAdd($"校准电表时间:{startTime:G}", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteDateTime(startTime);

            WaitTime("等待14分30秒", 14 * 60 + 30);

            if (Stop) return results;
            startTime = new DateTime(now.Year, now.Month, now.Day, 0, 15, 20);
            MessageAdd($"校准电表时间:{startTime:G}", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteDateTime(startTime);

            WaitTime("等待5分", 5 * 60);

            if (Stop) return results;
            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrEmpty(powerZYstart[i]))
                {
                    results[i] = true;
                }
                else
                {
                    string[] arr = powerZYstart[i].Split(',');
                    if (float.TryParse(arr[0], out float f) && f == 0f)
                    {
                        results[i] = true;
                    }
                    else
                    {
                        MessageAdd($"表[{i + 1}]不合格,校时后5分钟产生需量。", EnumLogType.提示与流程信息);

                    }

                }
            }

            WaitTime("等待10分40秒", 10 * 60 + 40);

            if (Stop) return results;
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;

            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart1 = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (string.IsNullOrEmpty(powerZYstart1[i]))
                {
                    results[i] = false;
                    MessageAdd($"表[{i + 1}]不合格,校时后等待15分40秒不产生需量。", EnumLogType.提示与流程信息);
                }

            }



            return results;
        }


        /// <summary>
        /// 示例2
        /// </summary>
        /// <returns></returns>
        private bool[] Demo2Check()
        {
            bool[] results = new bool[MeterNumber];
            results.Fill(false);

            MessageAdd("示例2开始", EnumLogType.提示与流程信息);

            float xib = Number.GetCurrentByIb("Itr", OneMeterInfo.MD_UA, HGQ);  //获取走字的电流
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return results;
            }

            if (Stop) return results;
            Identity();

            if (Stop) return results;
            MessageAdd("需量清零", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.ClearDemand();

            DateTime now = DateTime.Now.AddDays(1);
            if (Stop) return results;
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            MessageAdd($"校准电表时间:{startTime:G}", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteDateTime(startTime);

            WaitTime("等待15分", 15 * 60);

            if (Stop) return results;
            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");

            float[] demond0 = new float[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!string.IsNullOrEmpty(powerZYstart[i]))
                {
                    string[] arr = powerZYstart[i].Split(',');
                    if (float.TryParse(arr[0], out float f))
                    {
                        demond0[i] = f;
                        results[i] = true;
                    }
                }
                else
                {
                    MessageAdd($"表[{i + 1}]不合格,校时后15分钟不产生需量。", EnumLogType.提示与流程信息);
                }
            }
            WaitTime("等待30秒", 30);

            if (Stop) return results;
            startTime = new DateTime(now.Year, now.Month, now.Day, 0, 16, 30);
            MessageAdd($"校准电表时间:{startTime:G}", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteDateTime(startTime);


            float xib1 = Number.GetCurrentByIb("1.2Itr", OneMeterInfo.MD_UA, HGQ);  //获取走字的电流
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib1, xib1, xib1, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return results;
            }

            // 
            WaitTime("等待15分10秒", 15 * 60 + 10);

            if (Stop) return results;
            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart1 = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!string.IsNullOrEmpty(powerZYstart1[i]))
                {
                    string[] arr = powerZYstart1[i].Split(',');
                    if (float.TryParse(arr[0], out float f))
                    {
                        if (f != demond0[i])
                        {
                            results[i] = false;
                            MessageAdd($"表[{i + 1}]不合格,在[00:17:00-00:32:00]产生需量。", EnumLogType.提示与流程信息);
                        }

                    }
                    else
                    {
                        results[i] = false;
                        MessageAdd($"表[{i + 1}]读取数据转换失败。", EnumLogType.提示与流程信息);
                    }
                }
                else
                {
                    results[i] = false;
                    MessageAdd($"表[{i + 1}]读取数据返回失败。", EnumLogType.提示与流程信息);
                }
            }

            WaitTime("等待25秒", 25);


            if (Stop) return results;
            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart2 = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!string.IsNullOrEmpty(powerZYstart2[i]))
                {
                    string[] arr = powerZYstart2[i].Split(',');
                    if (float.TryParse(arr[0], out float f))
                    {
                        if (f <= demond0[i])
                        {
                            results[i] = false;
                            MessageAdd($"表[{i + 1}]不合格,在[00:32:00]后不产生需量或需量值不正确，Data2:{f}, Data1:{demond0[i]}。", EnumLogType.提示与流程信息);
                        }
                    }
                    else
                    {
                        results[i] = false;
                        MessageAdd($"表[{i + 1}]读取数据转换失败。", EnumLogType.提示与流程信息);
                    }
                }
                else
                {
                    results[i] = false;
                    MessageAdd($"表[{i + 1}]读取数据返回失败。", EnumLogType.提示与流程信息);
                }
            }

            return results;
        }

        /// <summary>
        /// 示例3
        /// </summary>
        /// <returns></returns>
        private bool[] Demo3Check()
        {
            bool[] results = new bool[MeterNumber];
            results.Fill(false);

            MessageAdd("示例3开始", EnumLogType.提示与流程信息);

            float xib = Number.GetCurrentByIb("0.5Imax", OneMeterInfo.MD_UA, HGQ);  //获取走字的电流
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return results;
            }

            if (Stop) return results;
            Identity();

            if (Stop) return results;
            MessageAdd("需量清零", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.ClearDemand();

            WaitTime("等待15分30秒", 15 * 60 + 30);

            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.反向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return results;
            }

            WaitTime("等待70秒", 70);


            if (Stop) return results;
            MessageAdd("读取起始[(当前)正向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerZYstart = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总最大需量及发生时间");

            if (Stop) return results;
            MessageAdd("读取起始[(当前)反向有功总最大需量及发生时间]", EnumLogType.提示与流程信息);
            string[] powerFYstart = MeterProtocolAdapter.Instance.ReadData("(当前)反向有功总最大需量及发生时间");


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!string.IsNullOrEmpty(powerZYstart[i]) && !string.IsNullOrEmpty(powerFYstart[i]))
                {
                    string[] arr0 = powerZYstart[i].Split(',');
                    string[] arr1 = powerFYstart[i].Split(',');

                    if (float.TryParse(arr0[0], out float f) && float.TryParse(arr1[0], out float f1) && f > 0 && f1 > 0)
                    {
                        results[i] = true;
                    }
                    else
                    {
                        MessageAdd($"表[{i + 1}]不合格,数据转换失败。", EnumLogType.提示与流程信息);
                    }
                }
                else
                {
                    MessageAdd($"表[{i + 1}]不合格,读取数据失败。", EnumLogType.提示与流程信息);
                }
            }

            return results;
        }


        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (Stop) return false;
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }
            WaitTime("升源成功，等待源稳定", 5);
            return true;
        }
    }

}

