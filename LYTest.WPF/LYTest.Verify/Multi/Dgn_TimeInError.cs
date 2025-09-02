using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Model;
using System;

namespace LYTest.Verify.Multi
{
    ///add lsj 20220218 时钟示值误差实验
    // 时钟示值误差实验
    public class Dgn_TimeInError : VerifyBase
    {
        //modify yjt 20220303 设置-结论配置修改 “被检表时间|GSPS时间|时间差"”为“被检表时间|GPS时间|时间差”
        public override void Verify()
        {
            //add yjt 20220306 新增日志提示
            MessageAdd("时钟示值误差实验检定开始...", EnumLogType.流程信息);

            base.Verify();
            //检定点的参数值
            if (!int.TryParse(Test_Value, out int values))
            {
                values = 5;
            }
            MessageAdd("开始进行时钟示值误差试验", EnumLogType.提示信息);

            //add yjt 20220327 新增演示模式
            if (IsDemo)
            {
                if (Stop) return;

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    DateTime readTime = DateTime.Now;

                    ResultDictionary["GPS时间"][i] = readTime.ToString("yyyy-MM-dd HH:mm:ss");
                    ResultDictionary["被检表时间"][i] = readTime.ToString("yyyy-MM-dd HH:mm:ss");

                    int iTime = (int)readTime.Subtract(readTime).TotalSeconds;

                    ResultDictionary["时间差"][i] = iTime.ToString();

                    ResultDictionary["结论"][i] = "合格";
                }

                RefUIData("GPS时间");
                RefUIData("被检表时间");
                RefUIData("时间差");
                RefUIData("结论");
            }
            else
            {
                if (Stop) return;
                if (!CheckVoltage())
                {
                    return;
                }
                //ShowWirteMeterWwaring();

                if (Stop) return;
                //读取表地址和表号
                ReadMeterAddrAndNo();

                if (Stop) return;

                //delete yjt 20220311 删除旧的过程
                //if (Stop) return;
                //DateTime readTime = DateTime.Now;
                //delete yjt 20220303 删除GPS时间的获取
                //for (int i = 0; i < MeterNumber; i++)
                //{
                //    if (!meterInfo[i].YaoJianYn) continue;
                //    ResultDictionary["GPS时间"][i] = readTime.ToString();
                //}
                //    RefUIData("GPS时间");

                //if (Stop) return;
                //MessageAdd( "开始写表时间......", EnumLogType.提示信息);
                //MeterProtocolAdapter.Instance.WriteDateTime(readTime);

                //if (Stop) return;
                //DateTime now = DateTime.Now;
                //DateTime GPSTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

                //if (Stop) return;
                //DateTime[] readData = MeterProtocolAdapter.Instance.ReadDateTime();

                ////add yjt 20220303 新增GPS时间的获取
                //for (int i = 0; i < MeterNumber; i++)
                //{
                //    if (!meterInfo[i].YaoJianYn) continue;
                //    ResultDictionary["GPS时间"][i] = GPSTime.ToString();
                //}
                //RefUIData("GPS时间");

                //for (int i = 0; i < MeterNumber; i++)
                //{
                //    if (!meterInfo[i].YaoJianYn) continue;
                //    //ResultDictionary["被检表时间"][i] = readData.ToString();
                //    //modify yjt 20220303 修改被检表时间的获取
                //    ResultDictionary["被检表时间"][i] = readData[i].ToString();
                //}
                //RefUIData("被检表时间");

                MessageAdd($"开始读表时间", EnumLogType.提示信息);

                DateTimePair[] readData = MeterProtocolAdapter.Instance.ReadDateTimePair();

                bool[] needWrite = new bool[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    ResultDictionary["GPS时间"][i] = readData[i].LocalTime.ToString();
                    ResultDictionary["被检表时间"][i] = readData[i].MeterTime.ToString();

                    int iTime = (int)readData[i].TimeSpan.TotalSeconds;

                    ResultDictionary["时间差"][i] = iTime.ToString();

                    if (Math.Abs(iTime) <= values)
                        ResultDictionary["结论"][i] = "合格";
                    else
                    {
                        needWrite[i] = true;
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }

                if (Array.Exists(needWrite, t => { return t == true; }))
                {
                    if (Stop) return;
                    //Identity(false);
                    //MeterProtocolAdapter.Instance.WriteDateTime(needWrite);

                    readData = MeterProtocolAdapter.Instance.ReadDateTimePair();

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (!needWrite[i]) continue;

                        ResultDictionary["GPS时间"][i] = readData[i].LocalTime.ToString("yyyy-MM-dd HH:mm:ss");
                        ResultDictionary["被检表时间"][i] = readData[i].MeterTime.ToString("yyyy-MM-dd HH:mm:ss");

                        int iTime = (int)readData[i].TimeSpan.TotalSeconds;

                        ResultDictionary["时间差"][i] = iTime.ToString();

                        if (Math.Abs(iTime) <= values)
                            ResultDictionary["结论"][i] = "合格";
                        else
                            ResultDictionary["结论"][i] = "不合格";

                    }
                }
                RefUIData("GPS时间");
                RefUIData("被检表时间");
                RefUIData("时间差");
                RefUIData("结论");
                //delete yjt 20220311 删除旧的过程
                //if (Stop) return;
                //string[] resultKey = new string[MeterNumber];
                //for (int i = 0; i < MeterNumber; i++)
                //{
                //    if (!meterInfo[i].YaoJianYn) continue;
                //    if (readData[i] == null) continue;
                //    //数据项目
                //    DateTime meterTime = readData[i];
                //    //int iTime = (int)meterTime.Subtract(GPSTime).TotalSeconds;
                //    //modify
                //    int iTime = (int)meterTime.Subtract(GPSTime[i]).TotalSeconds;

                //    ResultDictionary["时间差"][i] = iTime.ToString();

                //    //if (Math.Abs(iTime) > values)
                //    //modify yjt 20220303 大于改为小于
                //    if (Math.Abs(iTime) < values)
                //        ResultDictionary["结论"][i] = "合格";
                //    else
                //        ResultDictionary["结论"][i] = "不合格";
                //}
                //RefUIData("时间差");
                //RefUIData("结论");
            }

            //MessageAdd("时钟示值误差试验完毕", EnumLogType.提示信息);
            //add yjt 20220306 新增日志提示
            MessageAdd("时钟示值误差实验检定结束...", EnumLogType.流程信息);

        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "被检表时间", "GPS时间", "时间差", "结论" };
            return true;
        }
    }
}
