using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.CostControl
{
    //add yjt 20220305 新增控制功能

    /// <summary>
    /// 控制功能
    /// </summary>
    public class FK_Encryption_ControlFunction : VerifyBase
    {
        /// <summary>
        /// 运行电流
        /// </summary>
        private string currentRun = "";

        public override void Verify()
        {
            MessageAdd("控制功能试验检定开始...", EnumLogType.流程信息);

            base.Verify();

            string[] fatest = Test_Value.Split('|');//获取方案的信息
            currentRun = fatest[2];

            //add yjt 20220327 新增演示模式
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;

                    if (!MeterInfo[i].YaoJianYn) continue;

                    ResultDictionary["报警测试写剩余金额"][i] = "成功";
                    ResultDictionary["写报警金额"][i] = "成功";
                    ResultDictionary["报警测试读电价"][i] = "0.4900";
                    ResultDictionary["报警状态字"][i] = "0107";
                    ResultDictionary["跳闸测试写剩余金额"][i] = "成功";
                    ResultDictionary["写跳闸金额"][i] = "成功";
                    ResultDictionary["跳闸测试读电价"][i] = "0.4900";
                    ResultDictionary["跳闸状态字"][i] = "0107";
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }

                RefUIData("报警测试写剩余金额");
                RefUIData("写报警金额");
                RefUIData("报警测试读电价");
                RefUIData("报警状态字");
                RefUIData("跳闸测试写剩余金额");
                RefUIData("写跳闸金额");
                RefUIData("跳闸测试读电价");
                RefUIData("跳闸状态字");
                RefUIData("结论");
            }
            else
            {
                if (Stop) return;
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }

                if (Stop) return;
                ReadMeterAddrAndNo();

                #region 进行报警金额和跳闸金额测试
                //读报警金额1
                bool[] warningResult = WarningVerify();
                if (Stop) return;

                //跳闸金额测试
                bool[] cutoffResult = TurnOffVerify();
                if (Stop) return;

                #endregion

                SaveTestResult(warningResult, cutoffResult);//保存和显示测试结果

                if (Stop) return;

                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                Identity(false);

                if (Stop) return;

                MessageAdd("写入剩余金额", EnumLogType.提示信息);
                bool[] res = MeterProtocolAdapter.Instance.InitPurse(15 * 100);
            }

            MessageAdd("测试完毕", EnumLogType.提示信息);

            MessageAdd("控制功能试验检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// 报警金额检定
        /// </summary>
        /// <returns></returns>
        private bool[] WarningVerify()
        {
            //1. 获取报警金额1
            //2. 设置钱包金额为报警金额1+0.1
            //3. 走字
            //4. 读取状态字3，判断是否已报警

            bool[] result = new bool[MeterNumber];

            if (Stop) return result;

            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            //Identity();
            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            Identity(true);

            MessageAdd("读取报警金额1", EnumLogType.提示信息);
            string[] warningMoney = MeterProtocolAdapter.Instance.ReadData("报警金额1限值");

            // 充值
            if (Stop) return result;
            int cash = (int)((Convert.ToSingle(warningMoney[FirstIndex]) + 0.1) * 100);

            string[] je = new string[MeterNumber];
            double jeF = (float)Convert.ToSingle(warningMoney[FirstIndex]) + 0.1;
            //jeF = (float)((Convert.ToSingle(warningMoney[FirstIndex]) + 0.1) * 100);
            je.Fill(jeF.ToString("f" + DecimalDigits));

            if (Stop) return result;
            MessageAdd("写入剩余金额", EnumLogType.提示信息);
            bool[] resultInitpurse = MeterProtocolAdapter.Instance.InitPurse(cash);

            InsertChkingData("报警测试写剩余金额", resultInitpurse, je);
            if (Stop) return result;

            InsertChkingData("写报警金额", resultInitpurse, warningMoney);

            #region 计算试验时间
            if (Stop) return result;
            WaitTime("等待", 3);
            string[] curPrice = MeterProtocolAdapter.Instance.ReadData("当前电价");//0280000B
            InsertChkingData("报警测试读电价", curPrice);
            //获取最小电价
            float priceMin = 0;
            for (int i = 0; i < curPrice.Length; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (string.IsNullOrEmpty(curPrice[i])) continue;

                    if (priceMin > Convert.ToSingle(curPrice[i]) || priceMin == 0)
                        priceMin = Convert.ToSingle(curPrice[i]);
                }
            }
            if (priceMin == 0) return result;

            //试验完需要走的电量（KWH）和时间（秒）
            float[] IbImax = OneMeterInfo.GetIb();
            string XIbstr = currentRun;
            float XIb = XIbitem(IbImax, XIbstr); //根据方案获取电流
            WireMode clfs = (WireMode)Enum.Parse(typeof(WireMode), OneMeterInfo.MD_WiringMode);
            float currentPower = base.CalculatePower(OneMeterInfo.MD_UB, XIb, clfs, Cus_PowerYuanJian.H, "1.0", true);
            float totalTime = (float)((3600 * 1000 * 0.1) / currentPower / priceMin);

            #endregion 计算试验事件
            if (Stop) return result;
            MessageAdd("给电表输出电流", EnumLogType.提示信息);
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, XIb, XIb, XIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
            }

            if (Stop) return result;
            //modify yjt 20002815 修改时间
            //DateTime timeEnd = DateTime.Now.AddSeconds(totalTime + 20);
            DateTime timeEnd = DateTime.Now.AddSeconds(totalTime + 2);
            while (timeEnd > DateTime.Now)
            {
                if (Stop) return result;

                System.Threading.Thread.Sleep(1000);
                TimeSpan timeSpan = timeEnd - DateTime.Now;
                MessageAdd(string.Format("测试剩余时间{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds), EnumLogType.提示信息);
            }

            //关源
            if (Stop) return result;
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
            }
            WaitTime("等待", 3);
            if (Stop) return result;
            //读取电表运行状态字3
            MessageAdd("读取电表报警状态", EnumLogType.提示信息);
            string[] mChar3 = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
            InsertChkingData("报警状态字", mChar3);
            for (int i = 0; i < mChar3.Length; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    int chr = Convert.ToInt16(mChar3[i], 16); //CDLT6352007

                    //modify yjt 20220427 修改获取的协议名称
                    //if (OneMeterInfo.MD_ProtocolName == "CDLT698")
                    //modify yjt 20220427 修改获取的协议名称
                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                        chr = Number.BitRever(chr, 16);
                    if ((chr & 0x80) == 0x80 && resultInitpurse[i])
                        result[i] = true;
                    else
                        result[i] = false;

                    ////add yjt 20220427 新增过载的情况下直接将低于报警金额1的电价写到电表里面，读状态中
                    //if (result[i] == false)
                    //{
                    //    cash = (int)((Convert.ToSingle(warningMoney[FirstIndex]) + 0.1) * 100) - 200;
                    //    MeterProtocolAdapter.Instance.InitPurse(cash);
                    //    string mChar3str = MeterProtocolAdapter.Instance.ReadData1("电表运行状态字3", i);
                    //    int chr1 = Convert.ToInt16(mChar3str, 16);
                    //    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                    //        chr1 = Number.BitRever(chr1, 16);
                    //    if ((chr1 & 0x80) == 0x80 && resultInitpurse[i])
                    //        result[i] = true;
                    //    else
                    //        result[i] = false;
                    //}
                }
            }

            return result;
        }

        /// <summary>
        /// 跳闸金额检定
        /// </summary>
        /// <returns></returns>
        private bool[] TurnOffVerify()
        {
            bool[] result = new bool[MeterNumber];

            if (Stop) return result;

            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            //Identity();
            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            Identity(true);

            if (Stop) return result;
            MessageAdd("读取报警金额2", EnumLogType.提示信息);
            string[] warningMoney = MeterProtocolAdapter.Instance.ReadData("报警金额2限值");

            //写入预置金额
            if (Stop) return result;
            int cash = (int)((Convert.ToSingle(warningMoney[FirstIndex]) + 0.01) * 100);

            string[] je = new string[MeterNumber];
            double jeF = (float)Convert.ToSingle(warningMoney[FirstIndex]) + 0.01;
            je.Fill(jeF.ToString("f" + DecimalDigits));

            if (Stop) return result;
            MessageAdd("写入剩余金额", EnumLogType.提示信息);
            bool[] arrayTemp = MeterProtocolAdapter.Instance.InitPurse(cash);

            if (Stop) return result;
            InsertChkingData("跳闸测试写剩余金额", arrayTemp, je);

            InsertChkingData("写跳闸金额", arrayTemp, je);

            if (Stop) return result;
            #region 计算试验时间
            WaitTime("等待", 3);
            string[] curPrice = MeterProtocolAdapter.Instance.ReadData("当前电价");
            InsertChkingData("跳闸测试读电价", curPrice);

            //获取最小电价
            float priceMin = 0;
            for (int i = 0; i < curPrice.Length; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (string.IsNullOrEmpty(curPrice[i])) continue;

                    if (priceMin > Convert.ToSingle(curPrice[i]) || priceMin == 0)
                        priceMin = Convert.ToSingle(curPrice[i]);
                }
            }
            if (priceMin == 0) return result;
            //试验完需要走的电量（KWH）和时间（秒）
            float[] IbImax = OneMeterInfo.GetIb();
            string XIbstr = currentRun;
            float XIb = XIbitem(IbImax, XIbstr); //根据方案获取电流
            WireMode clfs = (WireMode)Enum.Parse(typeof(WireMode), OneMeterInfo.MD_WiringMode);
            float currentPower = base.CalculatePower(OneMeterInfo.MD_UB, XIb, clfs, Cus_PowerYuanJian.H, "1.0", true);
            //modify yjt 20002815 修改时间
            //float totalTime = (float)((3600 * 1000 * 0.1) / currentPower / priceMin);
            float totalTime = (float)((3600 * 1000 * 0.01) / currentPower / priceMin);
            #endregion 计算试验事件

            //低于报警金额2会断电，不走字
            MessageAdd("给电表输出电流", EnumLogType.提示信息);

            if (Stop) return result;
            MessageAdd("给电表输出电流", EnumLogType.提示信息);
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, XIb, XIb, XIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
            }

            if (Stop) return result;
            //modify yjt 20002815 修改时间
            //DateTime timeEnd = DateTime.Now.AddSeconds(20);
            DateTime timeEnd = DateTime.Now.AddSeconds(totalTime);
            while (timeEnd > DateTime.Now)
            {
                if (Stop) return result;

                System.Threading.Thread.Sleep(10);
                TimeSpan timeSpan = timeEnd - DateTime.Now;
                MessageAdd(string.Format("测试剩余时间{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds), EnumLogType.提示信息);
            }

            //关源
            if (Stop) return result;
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
            }
            WaitTime("等待", 3);
            if (Stop) return result;


            ////modify yjt 20002815 修改判断拉闸方式
            bool IsPower = false;

            //if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, XIb, XIb, XIb, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            //{
            ////    MessageAdd("控制源输出失败", EnumLogType.提示信息);
            //}
            //IsPower = true;
            //if (XIb > 0 && EquipmentData.StdInfo.Ia < 0.001) IsPower = false;
            //if (XIb > 0 && EquipmentData.StdInfo.Ib < 0.001 && Clfs == WireMode.三相四线) IsPower = false;
            //if (XIb > 0 && EquipmentData.StdInfo.Ic < 0.001 && (Clfs == WireMode.三相四线 || Clfs == WireMode.三相三线)) IsPower = false;

            //if (IsPower == false)
            //{
            //    MessageAdd("升电流不成功，电能表为拉闸状态，拉闸成功！", EnumLogType.提示与流程信息);
            //}
            //else
            //{
            //    MessageAdd("升电流成功，电能表不为拉闸状态，拉闸失败！", EnumLogType.提示与流程信息);
            //}

            ////关源
            //if (Stop) return result;
            //if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            //{
            //    MessageAdd("控制源输出失败", EnumLogType.提示信息);
            //}

            //读取电表运行状态字3
            MessageAdd("读取电表状态", EnumLogType.提示信息);

            string[] warningArray = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");

            InsertChkingData("跳闸状态字", warningArray);

            for (int i = 0; i < warningArray.Length; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    //int warningValue = Convert.ToInt32(warningArray[i], 16);
                    //result[i] = true;

                    //modify yjt 20002815 修改判断拉闸方式
                    //int chr = Convert.ToInt16(warningArray[i], 16);
                    //if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                    //    chr = Number.BitRever(chr, 16);

                    //if ((chr & 0x80) == 0x80 && arrayTemp[i])
                    //{
                    //    result[i] = true;
                    //}
                    //else
                    //{
                    //    result[i] = false;
                    //}

                    //modify yjt 20002815 修改判断拉闸方式
                    if (IsPower == false && arrayTemp[i])
                    {
                        result[i] = true;
                    }
                    else
                    {
                        result[i] = false;
                    }

                    //if (result[i] == false)
                    //{
                    //    cash = (int)((Convert.ToSingle(warningMoney[FirstIndex]) + 0.1) * 100) - 200;
                    //    MeterProtocolAdapter.Instance.InitPurse(cash);
                    //    string mChar3str = MeterProtocolAdapter.Instance.ReadData1("电表运行状态字3", i);
                    //    int chr1 = Convert.ToInt16(mChar3str, 16);
                    //    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                    //        chr1 = Number.BitRever(chr1, 16);
                    //    if ((chr1 & 0x40) == 0x40 && arrayTemp[i])
                    //        result[i] = true;
                    //    else
                    //        result[i] = false;
                    //}
                }
            }
            return result;
        }

        private void SaveTestResult(bool[] warningResult, bool[] cutOffResult)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                if (!MeterInfo[i].YaoJianYn) continue;

                ResultDictionary["结论"][i] = warningResult[i] && cutOffResult[i] ? ConstHelper.合格 : ConstHelper.不合格;
            }
            RefUIData("结论");
        }

        private void InsertChkingData(string key, bool[] result, string[] je)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) break;

                if (!MeterInfo[i].YaoJianYn) continue;
                //ResultDictionary[key][i] = result[i] ? "成功" : "失败";
                if (key.IndexOf("读") != -1)
                {
                    if (je[i] != null || je[i] != "")
                        ResultDictionary[key][i] = je[i];
                    else
                        ResultDictionary[key][i] = "失败";
                }
                else
                {
                    ResultDictionary[key][i] = result[i] ? je[i] : "失败";
                }
            }
            RefUIData(key);
        }

        private void InsertChkingData(string childKey, string[] resultInitpurse)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) break;

                if (!MeterInfo[i].YaoJianYn) continue;

                ResultDictionary[childKey][i] = resultInitpurse[i];
            }
            RefUIData(childKey);
        }

        /// <summary>
        /// 获取电流值
        /// </summary>
        /// <param name="IbImax"></param>
        /// <param name="XIbstr"></param>
        /// <returns></returns>
        public float XIbitem(float[] IbImax, string XIbstr)
        {
            float XIb = 0f;
            if (IbImax.Length == 2)
            {
                if (XIbstr.IndexOf("Imax") != -1)
                {
                    if (XIbstr == "Imax")
                    {
                        XIb = IbImax[1];
                    }
                    else
                    {
                        XIb = float.Parse(XIbstr.Replace("Imax", "")) * IbImax[1];
                    }
                }
                else if (XIbstr.IndexOf("Ib") != -1)
                {
                    if (XIbstr == "Ib")
                    {
                        XIb = IbImax[0];
                    }
                    else
                    {
                        XIb = float.Parse(XIbstr.Replace("Ib", "")) * IbImax[0];
                    }
                }
            }
            else if (IbImax.Length == 3)
            {
                if (XIbstr.IndexOf("Imax") != -1)
                {
                    if (XIbstr == "Imax")
                    {
                        XIb = IbImax[1];
                    }
                    else
                    {
                        XIb = float.Parse(XIbstr.Replace("Imax", "")) * IbImax[1];
                    }
                }
                else if (XIbstr.IndexOf("Ib") != -1)
                {
                    if (XIbstr == "Ib")
                    {
                        XIb = IbImax[0];
                    }
                    else
                    {
                        XIb = float.Parse(XIbstr.Replace("Ib", "")) * IbImax[0];
                    }
                }
                else if (XIbstr.IndexOf("Itr") != -1)
                {
                    if (XIbstr == "Itr")
                    {
                        XIb = IbImax[0];
                    }
                    else
                    {
                        XIb = float.Parse(XIbstr.Replace("Itr", "")) * IbImax[0];
                    }
                }
                else if (XIbstr.IndexOf("Imin") != -1)
                {
                    if (XIbstr == "Imin")
                    {
                        XIb = IbImax[2];
                    }
                    else
                    {
                        XIb = float.Parse(XIbstr.Replace("Imin", "")) * IbImax[2];
                    }
                }
            }
            else
            {
                XIb = IbImax[0];
            }

            return XIb;
        }


        protected override bool CheckPara()
        {
            ResultNames = new string[] { "报警测试写剩余金额", "写报警金额", "报警测试读电价", "报警状态字", "跳闸测试写剩余金额", "写跳闸金额", "跳闸测试读电价", "跳闸状态字", "结论" };
            return true;
        }
    }
}
