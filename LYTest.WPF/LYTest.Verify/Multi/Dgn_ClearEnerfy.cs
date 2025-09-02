using LYTest.Core.Function;
using LYTest.Core;
using LYTest.ViewModel.CheckController;
using System;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 电量清零
    /// </summary>
    public class Dgn_ClearEnerfy : VerifyBase
    {

        int data = 0;
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("电量清零试验检定开始...", EnumLogType.流程信息);

            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定项目"][i] = "电量清零";
                ResultDictionary["检定数据"][i] = data.ToString();
            }
            RefUIData("检定项目");
            RefUIData("检定数据");

            //初始化设备
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            bool[] result = new bool[MeterNumber];
            bool[] result2 = new bool[MeterNumber];
            result2.Fill(true);
            string[] aryLeftMoney = new string[MeterNumber];
            if (!IsDemo)
            {
                if (Stop) return;

                ReadMeterAddrAndNo();

                if (Stop) return;

                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                //Identity();
                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                Identity(false);

                if (Stop) return;

                if (VerifyConfig.Dgn_ClearFrontTiming)  //清零前校时
                {
                    MessageAdd("正在清零前校时", EnumLogType.提示信息);
                    if (!IsDemo)
                    {
                        MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                    }
                }
                if (Stop) return;
                MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                if (!IsDemo)
                {
                    string[] strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
                    string firstFKType = "";
                    string firstFKTypeNote = "";
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;

                        if (!string.IsNullOrWhiteSpace(strReadData[i]))
                        {
                            int runningState3 = Convert.ToInt32(strReadData[i], 16);
                            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                            {

                            }
                            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                            {
                                runningState3 = Number.BitRever(runningState3, 16);
                            }

                            if ((runningState3 & 0x0300) == 0x0000)
                            {
                                firstFKType = "非预付费表";//包括远程费控
                                firstFKTypeNote = "包括远程费控";
                            }
                            else if ((runningState3 & 0x0300) == 0x0100)
                                firstFKType = "电量型预付费表";
                            else if ((runningState3 & 0x0300) == 0x0200)
                            {
                                firstFKType = "电费型预付费表";//包括本地费控
                                firstFKTypeNote = "包括本地费控";
                            }

                            if (!string.IsNullOrWhiteSpace(firstFKType)) break;
                        }
                    }
                    {
                        string inputFKType = "";
                        if (OneMeterInfo.FKType == 0) inputFKType = "远程费控";
                        else if (OneMeterInfo.FKType == 1) inputFKType = "本地费控";
                        else if (OneMeterInfo.FKType == 2) inputFKType = "不带费控";

                        if (OneMeterInfo.FKType == 0 && firstFKType != "非预付费表")
                        {
                            MessageAdd($"检测到【{firstFKType}】{firstFKTypeNote}，参数录入【{inputFKType}】。", EnumLogType.错误信息);
                        }
                        else if (OneMeterInfo.FKType == 1 && firstFKType != "电费型预付费表")
                        {
                            MessageAdd($"检测到【{firstFKType}】{firstFKTypeNote}，参数录入【{inputFKType}】。", EnumLogType.错误信息);
                        }
                        else
                        {
                            MessageAdd($"检测到【{firstFKType}】{firstFKTypeNote}，参数录入【{inputFKType}】。", EnumLogType.流程信息);
                        }
                    }
                }
                MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
                if (Stop) return;
                Identity(false);
                if (Stop) return;
                if (OneMeterInfo.FKType == 1) //本地
                {
                    MessageAdd($"本地费控，清零、初始化金额【{data}】", EnumLogType.提示与流程信息);
                    result = MeterProtocolAdapter.Instance.InitPurse(data * 100);
                }
                else
                {
                    MessageAdd($"{(OneMeterInfo.FKType == 0 ? "远程费控" : "不带费控")}，清零。", EnumLogType.提示与流程信息);
                    result = MeterProtocolAdapter.Instance.ClearEnergy();
                    if (DAL.Config.ConfigHelper.Instance.IsITOMeter)//物联表的情况
                    {
                        MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                        result2 = MeterProtocolAdapter.Instance.ClearEnergy();
                        MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
                    }
                }
                WaitTime("清零", 30);

                if (OneMeterInfo.FKType == 1)
                {
                    MessageAdd("读取钱包初始化后的剩余金额...", EnumLogType.提示信息);
                    aryLeftMoney = MeterProtocolAdapter.Instance.ReadData("(当前)剩余金额");
                }
            }
            MessageAdd("正在读电量");
            float[] arrayLeft = MeterProtocolAdapter.Instance.ReadEnergy((byte)FangXiang, (byte)0);
            float[] arrayLeftRA = MeterProtocolAdapter.Instance.ReadEnergy((byte)Core.Enum.PowerWay.反向有功, (byte)0);
            float[] arrayLeftFRA = MeterProtocolAdapter.Instance.ReadEnergy((byte)Core.Enum.PowerWay.正向无功, (byte)0);
            float[] arrayLeftRRA = MeterProtocolAdapter.Instance.ReadEnergy((byte)Core.Enum.PowerWay.反向无功, (byte)0);

            //物联表的情况
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
            {
                MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;

                float[] arrayLeftManagementChip = MeterProtocolAdapter.Instance.ReadEnergy((byte)FangXiang, (byte)0);
                float[] arrayLeftRAManagementChip = MeterProtocolAdapter.Instance.ReadEnergy((byte)Core.Enum.PowerWay.反向有功, (byte)0);
                float[] arrayLeftFRAManagementChip = MeterProtocolAdapter.Instance.ReadEnergy((byte)Core.Enum.PowerWay.正向无功, (byte)0);
                float[] arrayLeftRRAManagementChip = MeterProtocolAdapter.Instance.ReadEnergy((byte)Core.Enum.PowerWay.反向无功, (byte)0);


                //反向有功，正向无功，反向无功 都==0
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (arrayLeft[i] == 0 && arrayLeftRA[i] == 0
                        && (arrayLeftFRA[i] == 0 || arrayLeftFRA[i] == -1) && (arrayLeftRRA[i] == 0 || arrayLeftRRA[i] == -1)
                        && arrayLeftManagementChip[i] == 0 && arrayLeftRAManagementChip[i] == 0
                        && arrayLeftFRAManagementChip[i] == 0 && arrayLeftRRAManagementChip[i] == 0)
                    {
                        ResultDictionary["检定数据"][i] = "0";
                        ResultDictionary["结论"][i] = ConstHelper.合格;
                    }
                    else
                    {
                        ResultDictionary["检定数据"][i] = $"计量芯:{arrayLeft[i]},{arrayLeftRA[i]},{arrayLeftFRA[i]},{arrayLeftRRA[i]}" +
                            $"|管理芯:{arrayLeftManagementChip[i]},{arrayLeftRAManagementChip[i]},{arrayLeftFRAManagementChip[i]},{arrayLeftRRAManagementChip[i]}";
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        NoResoult[i] = "清空电量失败";
                    }
                }
            }
            else
            {
                //反向有功，正向无功，反向无功 都==0
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (arrayLeft[i] == 0 && arrayLeftRA[i] == 0
                        && (arrayLeftFRA[i] == 0 || arrayLeftFRA[i] == -1) && (arrayLeftRRA[i] == 0 || arrayLeftRRA[i] == -1))
                    {
                        ResultDictionary["检定数据"][i] = "0";
                        ResultDictionary["结论"][i] = ConstHelper.合格;
                    }
                    else
                    {
                        ResultDictionary["检定数据"][i] = $"有功无功电量:{arrayLeft[i]},{arrayLeftRA[i]},{arrayLeftFRA[i]},{arrayLeftRRA[i]}";
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        NoResoult[i] = "清空电量失败";
                    }
                }
            }

            if (OneMeterInfo.FKType == 1)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    float.TryParse(aryLeftMoney[i], out float Lm);
                    if (Lm != data)
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    }
                    ResultDictionary["检定数据"][i] += $",剩余金额：{aryLeftMoney[i]}";
                }
            }

            RefUIData("检定数据");
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("电量清零试验检定结束...", EnumLogType.流程信息);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            int.TryParse(Test_Value, out data);
            ResultNames = new string[] { "检定项目", "检定数据", "结论" };
            return true;
        }
    }
}
