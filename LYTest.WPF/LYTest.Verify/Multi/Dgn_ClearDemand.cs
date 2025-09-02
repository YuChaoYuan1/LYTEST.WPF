using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 需量清空
    /// </summary>
    public class Dgn_ClearDemand : VerifyBase
    {
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("需量清空试验检定开始...", EnumLogType.流程信息);

            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定项目"][i] = "需量清空";
            }
            RefUIData("检定项目");

            //add yjt 20220327 新增演示模式
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;

                    ResultDictionary["检定数据"][i] = "0";
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }
            }
            else
            {
                if (!PowerOn())
                {
                    MessageAdd("源输出失败", EnumLogType.提示信息);
                    return;
                }

                //add yjt 20220415 新增升源等待时间
                WaitTime("正在升源", 5);

                if (Stop) return;

                ReadMeterAddrAndNo();

                if (Stop) return;

                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                //Identity();
                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                Identity(false);

                if (Stop) return;
                MessageAdd("正在清空需量", EnumLogType.提示信息);
                bool[] result = MeterProtocolAdapter.Instance.ClearDemand();
                if (Stop) return;
                MessageAdd("正在读取需量", EnumLogType.提示信息);
                float[] readDemand = MeterProtocolAdapter.Instance.ReadDemand(1, (byte)0);
                if (Stop) return;

                if (IsAllValueMatch(readDemand, -1F))
                {
                    MessageAdd("读取电能表需量失败", EnumLogType.提示信息);
                    return;
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    string Result = ConstHelper.不合格;

                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;

                    if (readDemand[i] == 0F)
                        Result = ConstHelper.合格;
                    ResultDictionary["检定数据"][i] = readDemand[i].ToString();
                    ResultDictionary["结论"][i] = Result;
                    if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                    {
                        NoResoult[i] = "需量没有清空";
                    }
                }               
            }

            RefUIData("检定数据");
            RefUIData("结论");

            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("需量清空试验检定结束...", EnumLogType.流程信息);
        }
        /// <summary>
        /// 判断数组中的所有元素是否与给定的 对象相同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="va"></param>
        /// <returns></returns>
        public static bool IsAllValueMatch<T>(T[] values, T va)
        {
            foreach (T t in values)
            {
                if (va.Equals(t) == false)
                {
                    return false;
                }
            }

            return true;
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
