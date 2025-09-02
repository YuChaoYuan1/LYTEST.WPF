using LYTest.Core.Function;
using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 钱包初始化
    /// </summary>
    class FK_Encryption_InitPurse : VerifyBase
    {
        string Money = "0";

        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("钱包初始化试验检定开始...", EnumLogType.流程信息);

            //FailureRate
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = "钱包初始化";
            }
            RefUIData("当前项目");

            if (VerifyConfig.FailureRate > 0)     //故障率不是0说明需要报警
            {
                bool[] IsResoult = new bool[MeterNumber];
                IsResoult.Fill(false);
                for (int i = 0; i < MeterNumber; i++)
                {
                    IsResoult[i] = GetMeterResult(i, Test_No);//获取表位的结论
                                                              //如果合格率低于百分20，就弹出提示，停止检定
                }
                int yaojianCount = GetYaoJian().Count<bool>(a => a == true);//要检定的数量
                int OkCount = IsResoult.Count<bool>(a => a == true); //合格的数量

                float s = (float)OkCount / (float)yaojianCount;
                if (yaojianCount < 1 || s <= (1 - VerifyConfig.FailureRate / 100))
                {
                    MessageAdd($"不合格率高于百分{VerifyConfig.FailureRate},请检查台体是否异常！！！", EnumLogType.错误信息);
                    TryStopTest();//停止检定
                    return;
                }
            }

            bool[] result = new bool[MeterNumber];
            string[] arrayLeftMoney = new string[MeterNumber];         //剩余金额
            float[] arrayLeftMoney2 = new float[MeterNumber];         //剩余电量

            MessageAdd(string.Format("准备开始钱包初始化，初始化金额为【{0}】元", Money), EnumLogType.提示信息);
            if (!IsDemo)
            {
                //初始化设备
                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
                if (Stop) return;
                ReadMeterAddrAndNo();

                if (Stop) return;

                Identity(true);

                if (Stop) return;
                MessageAdd("开始写表时间......", EnumLogType.提示信息);
                result = MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                MessageAdd("钱包初始化...", EnumLogType.提示信息);
                //if (OneMeterInfo.FKType != 1)
                //{
                //    MessageAdd($"{(OneMeterInfo.FKType == 0 ? "远程费控" : "不带费控")}，钱包初始化，清零。", EnumLogType.提示与流程信息);
                //    result = MeterProtocolAdapter.Instance.ClearEnergy();
                //    WaitTime("电量清零", 30);
                //    MessageAdd("读取钱包初始化后的电量...", EnumLogType.提示信息);
                //    arrayLeftMoney2 = MeterProtocolAdapter.Instance.ReadEnergy((byte)FangXiang, (byte)0);
                //}
                //else
                {
                    MessageAdd($"本地费控，钱包初始化、初始化金额【{Money}】", EnumLogType.提示与流程信息);
                    int intMoney = (int)(float.Parse(Money) * 100);
                    result = MeterProtocolAdapter.Instance.InitPurse(intMoney);
                    WaitTime("钱包初始化", 30);

                }

                MessageAdd("读取钱包初始化后的剩余金额...", EnumLogType.提示信息);
                arrayLeftMoney = MeterProtocolAdapter.Instance.ReadData("(当前)剩余金额");

            }

            if (Stop) return;
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                if (IsDemo)
                    result[i] = true;

                ResultDictionary["结论"][i] = result[i] ? ConstHelper.合格 : ConstHelper.不合格;
                //if (OneMeterInfo.FKType != 1)
                //{
                //    if (arrayLeftMoney2[i] != 0)
                //    {
                //        ResultDictionary["结论"][i] = ConstHelper.不合格;
                //    }
                //    ResultDictionary["检定信息"][i] = "钱包初始化后电量为：" + arrayLeftMoney2[i];
                //}
                //else
                {
                    float.TryParse(arrayLeftMoney[i], out float Lm);
                    if (Lm != float.Parse(Money))
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    }
                    ResultDictionary["检定信息"][i] = "钱包初始化后剩余金额为：" + arrayLeftMoney[i] + "元";
                }
                if (!result[i])
                {
                    NoResoult[i] = "钱包初始化失败";
                }

                //add yjt 20220327 新增演示模式
                if (IsDemo)
                {
                    ResultDictionary["检定信息"][i] = "钱包初始化后剩余金额为：100元";
                }
            }

            RefUIData("检定信息");
            RefUIData("结论");

            //string[] str = new string[12];
            //RefUIData("检定节点编号", "列名", str);//str数值
            //MessageAdd("获得结论", false); //提示信息
            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("钱包初始化试验检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] str = Test_Value.Split('|');
            Money = str[0] ?? str[0];
            ResultNames = new string[] { "当前项目", "检定信息", "结论" };
            return true;
        }
    }

}
