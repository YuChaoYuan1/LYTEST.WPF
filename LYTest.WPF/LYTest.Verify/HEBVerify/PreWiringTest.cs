using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LYTest.Verify.HEBVerify
{
    /// <summary>
    /// 接线检查
    /// </summary>
    public class PreWiringTest : VerifyBase
    {
        float xIb;
        string ItemID = "";
        public override void Verify()
        {
            try
            {


                base.Verify();

                bool[] bln_OffMeter = new bool[MeterNumber];
                bool[] result = new bool[MeterNumber];
                result.Fill(true);
                for (int i = 0; i < MeterNumber; i++)
                {

                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["表条码号"][i] = MeterInfo[i].MD_BarCode;
                    //Utility.Log.LogManager.AddMessage("表条码:" + meterInfo[i].MD_BarCode.ToString(), Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Information);
                    if (MeterInfo[i].MD_BarCode == "")
                    {
                        result[i] = false;
                    }
                    ResultDictionary["项目编号"][i] = ItemID;
                    //Utility.Log.LogManager.AddMessage("项目编号:" + ItemID.ToString(), Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Information);
                }
                RefUIData("表条码号");
                RefUIData("项目编号");


                //PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    Utility.Log.LogManager.AddMessage(DateTime.Now.ToString() + "|接线检查:升源失败，退出检定", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Information);
                    TryStopTest();
                    Stop = true;
                    return;
                }

                string[] address = MeterProtocolAdapter.Instance.ReadAddress();
                //判断是否读到表地址
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    MeterInfo[i].MD_PostalAddress = address[i];
                    ResultDictionary["表地址"][i] = address[i];
                    if (result[i])
                    {
                        if (MeterInfo[i].MD_PostalAddress == "")
                        {
                            result[i] = false;
                        }
                    }
                }
                MessageAdd("正在读取表号...", EnumLogType.提示信息);
                ReadMeterAddrAndNo();
                //更新电能表协议
                UpdateMeterProtocol();
                RefUIData("表地址");
                if (Stop) return;


                #region 时钟脉冲检查
                MessageAdd("开始判断表位时钟脉冲接线", EnumLogType.提示信息);
                //设置误差板参数，启动误差板为电能脉冲
                //MeterProtocolAdapter.Instance.SetPulseCom(0);
                float[] values = new float[MeterNumber];
                string[] arrayReadClock = new string[MeterNumber];
                int[] bcs = new int[MeterNumber];   //表常数
                int[] quans = new int[MeterNumber];        //圈数
                for (int i = 0; i < values.Length; i++)
                {
                    if (MeterInfo[i].DgnProtocol != null && MeterInfo[i].DgnProtocol.Loading)
                    {
                        values[i] = MeterInfo[i].DgnProtocol.ClockPL;
                    }
                    if (MeterInfo[i].MD_Constant != "")
                    {
                        string strBcs = MeterInfo[i].MD_Constant.ToString();
                        if (strBcs.IndexOf('(') != -1)
                        {
                            strBcs = strBcs.Substring(0, strBcs.IndexOf('('));
                        }
                        bcs[i] = int.Parse(strBcs);
                        quans[i] = 1;
                    }

                }
                //Thread.Sleep(1000);
                if (Stop) return;
                //设置误差版被检常数
                MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
                SetStandardConst(1, 500000, 0, 0xff);
                //设置误差版标准常数
                MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
                int[] meterconst = MeterHelper.Instance.MeterConst(true);
                bcs.Fill(1);
                int[] pulselap = new int[MeterNumber];
                if (!SetTestedConst(04, bcs, 0, quans, 0xff))
                {
                    MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                    return;
                }
                MessageAdd("正在启动误差板", EnumLogType.提示信息);
                StartWcb(04, 0xff);   //启动误差板

                //等待10秒开始读取
                //时钟误差延时要大于10秒
                // for (int i = 12; i > 0; i--)
                //{
                // if (Stop)
                //{
                // MessageAdd("检定被手动终止！", EnumLogType.提示信息);
                //return;
                // }
                // Thread.Sleep(1000);
                //MessageAdd($"距离读取时钟误差还有{(i - 1) }秒", EnumLogType.提示信息);
                //}
                string[] _CurWC = new string[MeterNumber];               //重新初始化本次误差
                int[] _CurrentWcNum = new int[MeterNumber];

                bool[] booLRev = new bool[MeterNumber];
                bool revOK = false;
                //读取时钟脉冲
                MessageAdd($"正在读取时钟脉冲", EnumLogType.提示信息);
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < MeterNumber; j++)
                    {
                        revOK = true;
                        if (!booLRev[j])
                        {
                            revOK = false;
                            break;
                        }
                    }
                    if (revOK) break;

                    if (Stop)
                    {
                        MessageAdd("检定被手动终止！", EnumLogType.提示信息);
                        return;
                    }
                    bool clockReadResultTemp = ReadWc(ref _CurWC, ref _CurrentWcNum, 04);
                    #region 解析临时读到的检定结论
                    for (int j = 0; j < MeterNumber; j++)
                    {
                        if (bln_OffMeter[i]) continue;
                        if (_CurWC[j] != null && string.IsNullOrEmpty(_CurWC[j]) == false && _CurWC[j] != "0")
                        {
                            float tmp = 999;
                            if (float.TryParse(_CurWC[j], out tmp) && Math.Abs(tmp) < 1 && !booLRev[j])
                            {
                                arrayReadClock[j] = _CurWC[j];
                                booLRev[j] = true;
                            }
                        }
                    }
                    #endregion
                    if (clockReadResultTemp) continue;
                    MessageAdd("读取时钟误差失败，等待3秒再次读取误差", EnumLogType.提示信息);

                    if (Stop)
                    {
                        MessageAdd("检定被手动终止！", EnumLogType.提示信息);
                        return;
                    }
                    Thread.Sleep(2000);
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (meter.YaoJianYn)
                    {
                        ResultDictionary["时钟脉冲"][i] = arrayReadClock[i];
                        if (result[i])
                        {
                            if (arrayReadClock[i] == "")
                            {
                                result[i] = false;
                            }
                        }
                    }
                }
                RefUIData("时钟脉冲");
                //关闭误差板
                StopWcb(04, 0xff);
                if (Stop) return;
                #endregion
                Thread.Sleep(1000);
                #region 电能脉冲
                MessageAdd("启动误差板脉冲计数", EnumLogType.提示信息);
                // PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                StartWcb(0x06, 0xff); //启动误差板脉冲计数
                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    Utility.Log.LogManager.AddMessage(DateTime.Now.ToString() + "|接线检查:启动误差板脉冲计数,升源失败，退出检定", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Information);
                    TryStopTest();
                    Stop = true;
                    return;
                }

                bool[] IsOk = new bool[MeterNumber];
                //IsOk.Fill(true);
                DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
                while (true)
                {
                    for (int j = 0; j < MeterNumber; j++)
                    {
                        revOK = true;
                        if (!IsOk[j])
                        {
                            revOK = false;
                            break;
                        }
                    }
                    if (revOK) break;
                    //超时处理
                    if (TimeSubms(DateTime.Now, TmpTime1) > 10000 && !IsMeterDebug) //超出最大处理时间并且不是调表状态
                    {
                        //NoResoult.Fill("超出最大处理时间");
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (Stop) return;
                            if (!MeterInfo[i].YaoJianYn) continue;
                            if (ResultDictionary["电能脉冲"][i] == null || ResultDictionary["电能脉冲"][i] == "")
                            {
                                ResultDictionary["电能脉冲"][i] = "0";
                            }
                        }
                        break;
                    }
                    StError[] errors = ReadWcbData(GetYaoJian(), 0x06);  //读取脉冲数量
                    for (int i = 0; i < errors.Length; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;

                        if (errors[i] == null && errors[i].szError != "0") continue;
                        {
                            if (!IsOk[i])
                            {
                                ResultDictionary["电能脉冲"][i] = errors[i].szError;
                                IsOk[i] = true;
                            }
                        }
                    }
                    //Thread.Sleep(1000);
                    if (Array.IndexOf(IsOk, false) < 0)  //全部都为true了
                        break;
                }
                for (int i = 0; i < IsOk.Length; i++)
                {
                    if (result[i])
                    {
                        if (IsOk[i] == false)
                        {
                            result[i] = false;
                        }
                    }
                }
                RefUIData("电能脉冲");
                //关闭误差板
                StopWcb(04, 0xff);
                #endregion


                IsInitIto = true;
                #region 电池电压
                MessageAdd("时钟电池电压", EnumLogType.提示信息);
                MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
                string[] arrayTimeVoltage = MeterProtocolAdapter.Instance.ReadData("时钟电池电压");
                MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["电池电压"][i] = arrayTimeVoltage[i];
                    if (result[i])
                    {
                        if (arrayTimeVoltage[i] == "")
                        {
                            result[i] = false;
                        }
                    }
                }
                RefUIData("电池电压");

                #endregion

                #region 读取电表停电抄表状态字
                MessageAdd("开始读取电表停电抄表状态字", EnumLogType.提示信息);

                //使用电表读取指令，读取电表状态字
                string[] strPowerOffDown = MeterProtocolAdapter.Instance.ReadData("电表运行状态字1");
                //判断是否读取到状态字，并且状态字合法
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    bln_OffMeter[i] = (strPowerOffDown[i] != null && strPowerOffDown[i].Length == 4 && (Convert.ToInt32(strPowerOffDown[i], 16) & 0x0008) == 0x0000) ? false : true;
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (meter.YaoJianYn)
                    {
                        if (bln_OffMeter[i])
                        {
                            ResultDictionary[$"状态字"][i] = strPowerOffDown[i];
                            result[i] = false;
                        }
                        else
                        {
                            ResultDictionary[$"状态字"][i] = strPowerOffDown[i];
                        }
                    }
                }
                //刷新结论UI
                RefUIData("状态字");
                #endregion

                #region 读取A相电压 ADD WKW 20220527
                MessageAdd("开始读取A相电压", EnumLogType.提示信息);
                string[] arrayAPhaseVoltage = MeterProtocolAdapter.Instance.ReadData("A相电压");
                //判断是否读到A相电压
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    bln_OffMeter[i] = (arrayAPhaseVoltage[i] != null && arrayAPhaseVoltage[i].Trim() != "") ? false : true;
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (meter.YaoJianYn)
                    {
                        //如果读取到的电压是空的，就设置为零
                        if (arrayAPhaseVoltage[i] == null || arrayAPhaseVoltage[i] == "")
                        {
                            arrayAPhaseVoltage[i] = "0";
                        }
                        else
                        {
                            ResultDictionary["电压"][i] = (float.Parse(arrayAPhaseVoltage[i]) / 10).ToString();//A相电压
                        }
                        if (result[i])
                        {
                            if (arrayAPhaseVoltage[i] == "")
                            {
                                result[i] = false;
                            }
                        }
                    }
                }

                RefUIData("电压");
                #endregion

                #region 读取A相电流 ADD WKW 20220527
                MessageAdd("开始读取A相电流", EnumLogType.提示信息);
                string[] arrayAPhaseCurrent = MeterProtocolAdapter.Instance.ReadData("A相电流");
                //判断是否读到A相电流
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    bln_OffMeter[i] = (arrayAPhaseCurrent[i] != null && arrayAPhaseCurrent[i].Trim() != "") ? false : true;
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (meter.YaoJianYn)
                    {
                        //如果读取到的电流是空的，就设置为零
                        if (arrayAPhaseCurrent[i].Length == 0 || string.IsNullOrEmpty(arrayAPhaseCurrent[i]))
                        {
                            ResultDictionary["电流"][i] = "0";//A相电流

                        }
                        else
                        {
                            ResultDictionary["电流"][i] = (float.Parse(arrayAPhaseCurrent[i]) / 1000).ToString();//A相电流
                        }

                        if (float.Parse(string.IsNullOrEmpty(arrayAPhaseCurrent[i]) ? "0" : arrayAPhaseCurrent[i].ToString()) < 0 || string.IsNullOrEmpty(arrayAPhaseCurrent[i])) //电流负值，
                        { result[i] = false; }

                        if (result[i])
                        {
                            if (arrayAPhaseCurrent[i] == "")
                            {
                                result[i] = false;
                            }
                        }
                    }
                }
                RefUIData("电流");
                #endregion


                if (Stop) return;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (result[i])
                        {
                            ResultDictionary["结论"][i] = ConstHelper.合格;
                        }
                        #region ADD WKW 20220617
                        //外观不合格也设置接线检查为不合格
                        else if (!result[i] || MeterInfo[i].Result.Equals("不合格"))
                        {
                            ResultDictionary["结论"][i] = ConstHelper.不合格;
                        }
                        #endregion
                        else
                        {
                            ResultDictionary["结论"][i] = ConstHelper.不合格;
                        }
                    }
                }
                //这里关闭没通过的表的继电器 
                RefUIData("结论");
                //PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    Utility.Log.LogManager.AddMessage(DateTime.Now.ToString() + "|接线检查:隔离故障表位恢复电压电流失败，退出检定", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Warning);
                    TryStopTest();
                    Stop = true;
                    return;
                }

                MessageAdd("开始隔离故障表位。", EnumLogType.提示信息);
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                    {
                        MeterInfo[i].YaoJianYn = false;//隔离的表位不检查了
                    }
                }



                ControlMeterRelay(GetYaoJian(false), 1);    //将不要检定的关闭
                RefMeterYaoJian();
                Thread.Sleep(50);

                //Networking();
                MessageAdd("检定完成", EnumLogType.提示信息);

            }
            catch (Exception ex)
            {
                TryStopTest();
                MessageAdd(ex.ToString(), EnumLogType.错误信息);
            }

            
        }



        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            xIb = Number.GetCurrentByIb("1.0Ib", OneMeterInfo.MD_UA, HGQ);
            ItemID = Test_Value;
            //ItemID = "2891";
            //表条码号|表地址|时钟脉冲|电能脉冲|电池电压|状态字|电压|电流|结论|项目编号
            ResultNames = new string[] { "表条码号", "表地址", "时钟脉冲", "电能脉冲", "电池电压", "状态字", "电压", "电流", "结论", "项目编号" };
            //电流开路状态|485通讯状态|状态字|时钟脉冲状态|电能脉冲状态
            return true;
        }
    }

}
