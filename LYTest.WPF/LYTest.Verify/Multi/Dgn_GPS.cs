using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using LYTest.ViewModel.Model;
using System;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// GPS对时
    /// </summary>
    public class Dgn_GPS : VerifyBase
    {
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("GPS对时试验检定开始...", EnumLogType.流程信息);

            //int s = 60;
            //while (s > 0)
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    s--;
            //    MessageAdd($"正在进行GPS对时{s}", false);
            //    if (Stop) break;

            //}
            //return;
            //开始检定
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定项目"][i] = "GPS对时";
            }
            RefUIData("检定项目");

            if (IsDemo)
            {
                if (Stop) return;

                for (int bw = 0; bw < MeterNumber; bw++)
                {
                    //表位的检定数据
                    if (!MeterInfo[bw].YaoJianYn)
                        continue;
                    DateTime GPSs = ReadGpsTime();
                    ResultDictionary["检定数据"][bw] = GPSs.ToString("yyyyMMdd HH:mm:ss");
                    ResultDictionary["结论"][bw] = ConstHelper.合格;
                }

                RefUIData("检定数据");
            }
            else
            {
                if (!CheckVoltageAndCurrent())
                {
                    return;
                }
                //获取检定状态 
                if (Stop) return;
                ReadMeterAddrAndNo();

                if (Stop) return;

                Identity(false);

                //add 加个判断物联表
                if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
                    MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;

                if (Stop) return;

                MessageAdd("开始写表时间......", EnumLogType.提示信息);
                bool[] result = MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                bool allFailed = true;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (result[i])
                    {
                        allFailed = false;
                        break;
                    }
                }
                MessageAdd("开始读表时间......", EnumLogType.提示信息);
                DateTimePair[] readDates = MeterProtocolAdapter.Instance.ReadDateTimePair();

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (!allFailed && !result[i])
                    {
                        //读取GPS时间
                        MessageAdd("正在读取GPS时间....", EnumLogType.提示信息);
                        //DateTime dateGPS = ReadGpsTime();

                        MessageAdd($"开始写表{i + 1}时间......", EnumLogType.提示信息);
                        result[i] = MeterProtocolAdapter.Instance.WriteDateTime(i);
                        readDates[i] = MeterProtocolAdapter.Instance.ReadDateTimePair(i);
                    }
                    MessageAdd("正在处理结果.....", EnumLogType.提示信息);

                    //表位的检定数据
                    ResultDictionary["检定数据"][i] = readDates[i].MeterTime.ToString("yyyy-MM-dd HH:mm:ss");

                    if (result[i])
                    {
                        ResultDictionary["结论"][i] = ConstHelper.合格;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        NoResoult[i] = "写入表时间失败";
                    }

                    RefUIData("检定数据");

                }

                ////delete yjt 20220311 用不到，删除 先不改
                //MessageAdd("正在读取表内时间...", EnumLogType.提示信息);
                //DateTime[] readTime = MeterProtocolAdapter.Instance.ReadDateTime();

                //MessageAdd("正在处理结果.....", EnumLogType.提示信息);
                //for (int bw = 0; bw < result.Length; bw++)
                //{
                //    //表位的检定数据
                //    if (!meterInfo[bw].YaoJianYn)
                //        continue;
                //    ResultDictionary["检定数据"][bw] = dateGPS.ToString("yyyyMMdd HH:mm:ss");


                //    //System.TimeSpan a = readTime[bw] - DateTime.Now;
                //    if (result[bw])
                //    {
                //        ResultDictionary["结论"][bw] = ConstHelper.合格;
                //    }
                //    else
                //    {
                //        ResultDictionary["结论"][bw] = ConstHelper.不合格;
                //        NoResoult[bw] = "写入表时间失败";
                //    }
                //}
            }
            RefUIData("结论");
            //MessageAdd("检定数据完成", EnumLogType.提示信息);
            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("GPS对时试验检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// 读取GPS时间
        /// </summary>
        /// <returns></returns>
        public DateTime ReadGpsTime()
        {
            return DateTime.Now;
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "检定项目", "检定数据", "结论" };
            return true;
        }
    }
}
