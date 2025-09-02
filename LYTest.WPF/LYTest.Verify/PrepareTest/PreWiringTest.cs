using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LYTest.Verify.PrepareTest
{
    /// <summary>
    /// 接线检查
    /// </summary>
    public class PreWiringTest : VerifyBase
    {
        MeterPrepare[] meterPrepare;

        MeterState[] meterStates;//表位状态
        float xIb;

        public override void Verify()
        {
            base.Verify();

            if (VerifyConfig.IsMete_Press == true)
            {
                #region 判断表位状态是否压接了
                if (Stop) return;
                meterStates = Read_Meterstate(GetYaoJian());   //读取表位状态
                bool[] t = new bool[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (meterStates[i] == null) continue;
                    if (meterStates[i].Motor != Core.Enum.MeterState_Motor.电机行在下)
                    {
                        t[i] = true;
                    }
                }

                if (Array.IndexOf<bool>(t, true) >= 0)
                {
                    if (Stop) return;
                    MessageAdd("开始重压不合格表位", EnumLogType.提示信息);
                    ElectricmachineryContrnl(t, 01);
                    WaitTime("正在重压不合格表位", VerifyConfig.Mete_Press_Time);
                }
                #endregion
            }



            bool[] bln_OffMeter = new bool[MeterNumber];
            bool[] result = new bool[MeterNumber];
            result.Fill(true);
            int retry = 1;
            if (VerifyConfig.IsMete_Press) retry = 2;
            for (int r = 0; r < retry; r++)  //检查2次
            {
                if (Stop) break;
                if (r == 1) //第二次，只需要控制第一次没合格的表位就行，进制电机重压
                {
                    if (VerifyConfig.IsMete_Press == true)
                    {
                        #region 第二次重压不合格位
                        //关源
                        PowerOff();
                        WaitTime("关源", 5);
                        //Thread.Sleep(2000);

                        //开启要检定表的继电器，要检定表中不合格的电机重压

                        //开启要检表位继电器
                        if (Stop) break;
                        ControlMeterRelay(GetYaoJian(), 2);
                        bool[] Ctrol = new bool[MeterNumber];
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            Ctrol[i] = !result[i];
                        }
                        if (Stop) break;
                        if (!ElectricmachineryContrnl(Ctrol, 1))
                        {
                            if (Stop) break;
                            ElectricmachineryContrnl(Ctrol, 1);
                        }
                        WaitTime("正在抬起不合格表位", VerifyConfig.Mete_Press_Time);

                        if (Stop) break;
                        if (!ElectricmachineryContrnl(Ctrol, 0))
                        {
                            if (Stop) break;
                            ElectricmachineryContrnl(Ctrol, 0);
                        }
                        WaitTime("正在重新压下不合格表位", VerifyConfig.Mete_Press_Time);
                        bln_OffMeter = new bool[MeterNumber];
                        result.Fill(true);  //默认合格
                        #endregion
                    }
                }
                if (Stop) break;
                #region 电流开路检查
                MessageAdd("开始判断电流开路表位,开始升源...", EnumLogType.提示与流程信息);
                //升源
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                MessageAdd("正在读取表位状态", EnumLogType.提示信息);
                //读取状态
                if (Stop) break;
                meterStates = Read_Meterstate(GetYaoJian());

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    //add
                    if (meterStates[i] == null)
                    {
                        if (MeterInfo[i].MD_WiringMode == "单相")
                        {
                            if (EquipmentData.StdInfo.Ia < 0.001)
                            {
                                bln_OffMeter[i] = true;
                            }
                        }
                        else
                        {
                            if (EquipmentData.StdInfo.Ia < 0.001 || EquipmentData.StdInfo.Ic < 0.001)
                            {
                                bln_OffMeter[i] = true;
                            }
                            if (OneMeterInfo.MD_WiringMode == "三相四线")
                            {
                                if (EquipmentData.StdInfo.Ib < 0.001)
                                {
                                    bln_OffMeter[i] = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (meterStates[i].I != Core.Enum.MeterState_I.正常)//
                        {
                            bln_OffMeter[i] = true;
                        }
                    }

                }

                //刷新结论UI
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (bln_OffMeter[i])
                        {
                            ResultDictionary[$"电流开路状态"][i] = "×";
                            result[i] = false;
                        }
                        else
                        {
                            ResultDictionary[$"电流开路状态"][i] = "√";
                        }
                    }
                }

                RefUIData("电流开路状态");
                if (Stop) break;
                #endregion

                #region 485通讯检查
                MessageAdd("开始判断表位485通讯。", EnumLogType.提示与流程信息);
                //升源
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                //读取表地址
                if (Stop) break;
                MessageAdd("正在读取表地址", EnumLogType.提示信息);
                string[] address = MeterProtocolAdapter.Instance.ReadAddress();
                //判断是否读到表地址
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) break;
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrWhiteSpace(address[i]))
                    {
                        bln_OffMeter[i] = true;
                    }
                    else
                    {
                        if (MeterInfo[i].MD_PostalAddress != address[i])
                        {
                            bln_OffMeter[i] = true;
                        }
                    }
                }
                if (Stop) break;
                //更新电能表协议
                //UpdateMeterProtocol();
                //判断结果
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (meter.YaoJianYn)
                    {
                        if (bln_OffMeter[i])
                        {
                            ResultDictionary[$"485通讯状态"][i] = "×";
                            result[i] = false;
                        }
                        else
                        {
                            ResultDictionary[$"485通讯状态"][i] = "√";
                            //TODO这里还要对比条码号和表号操作，暂时不管他
                        }
                    }


                }
                //刷新UI
                RefUIData("485通讯状态");
                //读取表号
                ReadMeterAddrAndNo();
                if (Stop) break;
                #endregion

                #region 新添加-检定前进行加密解密--修改成单独的检定项目了
                //if (OneMeterInfo.MD_JJGC == "IR46" && VerifyConfig.Test_IsCrypto) //IR46表,并且需要检定前进行加密解密
                //{
                //}
                #endregion

                #region 读取电表停电抄表状态字
                MessageAdd("开始读取电表停电抄表状态字", EnumLogType.提示与流程信息);

                //使用电表读取指令，读取电表状态字
                string[] strPowerOffDown = MeterProtocolAdapter.Instance.ReadData("电表运行状态字1");
                //判断是否读取到状态字，并且状态字合法
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) break;
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
                            ResultDictionary[$"状态字"][i] = "×";
                            result[i] = false;
                        }
                        else
                        {
                            ResultDictionary[$"状态字"][i] = "√";
                        }
                    }
                }
                //刷新结论UI
                RefUIData("状态字");
                if (Stop) break;
                #endregion

                #region 时钟脉冲检查
                MessageAdd("开始判断表位时钟脉冲接线", EnumLogType.提示与流程信息);
                //设置误差板参数，启动误差板为电能脉冲
                MeterProtocolAdapter.Instance.SetPulseCom(0);
                if (Stop) break;
                SetBluetoothModule(04);
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
                        quans[i] = 10;
                    }

                }
                if (Stop) break;
                Thread.Sleep(1000);
                if (Stop) break;
                //设置误差版被检常数
                MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
                SetStandardConst(1, 500000, 0);
                if (Stop) break;
                //设置误差版标准常数
                MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
                int[] meterconst = MeterHelper.Instance.MeterConst(true);
                bcs.Fill(1);
                int[] pulselap = new int[MeterNumber];
                if (!SetTestedConst(04, bcs, 0, quans))
                {
                    MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                    break;
                }
                if (Stop) break;
                MessageAdd("正在启动误差板", EnumLogType.提示信息);
                StartWcb(04, 0xff);   //启动误差板

                //等待10秒开始读取
                //时钟误差延时要大于10秒
                for (int i = 12; i > 0; i--)
                {
                    if (Stop)
                    {
                        MessageAdd("检定被手动终止！", EnumLogType.提示信息);
                        break;
                    }
                    Thread.Sleep(1000);
                    MessageAdd($"距离读取时钟误差还有{(i - 1) }秒", EnumLogType.提示信息);
                }
                string[] _CurWC = new string[MeterNumber];               //重新初始化本次误差
                int[] _CurrentWcNum = new int[MeterNumber];
                //读取时钟脉冲
                MessageAdd($"正在读取时钟脉冲", EnumLogType.提示信息);
                for (int i = 0; i < 10; i++)
                {
                    if (Stop)
                    {
                        MessageAdd("检定被手动终止！", EnumLogType.提示信息);
                        break;
                    }
                    bool clockReadResultTemp = ReadWc(ref _CurWC, ref _CurrentWcNum, 04);
                    #region 解析临时读到的检定结论
                    for (int j = 0; j < MeterNumber; j++)
                    {
                        if (bln_OffMeter[j]) continue;
                        if (_CurWC[j] != null && string.IsNullOrEmpty(_CurWC[j]) == false && _CurWC[j] != "0")
                        {
                            float tmp = 999;
                            if (float.TryParse(_CurWC[j], out tmp) && Math.Abs(tmp) < 1)
                            {
                                arrayReadClock[j] = _CurWC[j];
                            }
                        }
                    }
                    #endregion
                    if (clockReadResultTemp) continue;
                    MessageAdd("读取时钟误差失败，等待3秒再次读取误差", EnumLogType.提示信息);

                    if (Stop)
                    {
                        MessageAdd("检定被手动终止！", EnumLogType.提示信息);
                        break;
                    }
                    Thread.Sleep(3000);
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (meter.YaoJianYn)
                    {
                        bln_OffMeter[i] = string.IsNullOrEmpty(arrayReadClock[i]);
                        // meter.IsTest = !bln_OffMeter[i];
                        if (bln_OffMeter[i])
                        {
                            ResultDictionary[$"时钟脉冲状态"][i] = "×";
                            result[i] = false;
                        }
                        else
                        {
                            ResultDictionary[$"时钟脉冲状态"][i] = "√";
                        }
                    }
                }

                //判断结论，刷新UI
                RefUIData("时钟脉冲状态");
                //关闭误差板
                StopWcb(04, 0xff);
                if (Stop) break;
                #endregion

                #region 电能脉冲检查

                #region 有功
                MessageAdd("开始判断表位电能有功脉冲接线", EnumLogType.提示与流程信息);
                SetBluetoothModule(00);
                if (Stop) break;
                //升源                                                                   
                string[] arrayReadEnergyP = new string[MeterNumber];
                string[] arrayReadEnergyQ = new string[MeterNumber];
                int[] circleCount = new int[MeterNumber];
                int pulses = CalcPulseOfSeconds(5, OnePulseNeedTime(IsYouGong, 1000 * 60 / CalculatePower(OneMeterInfo.MD_UB, xIb, Clfs, Cus_PowerYuanJian.H, "1.0", IsYouGong)));
                circleCount.Fill(pulses);
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                
                if (Stop) break;
                ulong StdConst = GetStaConst();


                MessageAdd("正在设置标准表脉冲...", EnumLogType.提示信息);
                SetPulseType((0 + 49).ToString("x"));
                FangXiang = Core.Enum.PowerWay.正向有功;
                //设备参数初始化，启动误差板
                //设置误差版被检常数
                if (Stop) break;
                MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
                SetStandardConst(0, (int)(StdConst / 100), -2, 0xff);
                //设置误差版标准常数 TODO
                MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
                if (!SetTestedConst(00, meterconst, 0, circleCount))
                {
                    MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                }
                if (Stop) break;
                StartWcb(00, 0xff);
                string[] arrCurWCValueP = new string[MeterNumber];
                int[] arrCurrentWcNumP = new int[MeterNumber];
                string[] arrCurWCValueQ = new string[MeterNumber];
                int[] arrCurrentWcNumQ = new int[MeterNumber];

                int twoPulseTime = 3 * GetOneErrorTime();
                WaitTime("等待电表走2个有功脉冲", twoPulseTime / 1000);
                //读取电能脉冲，读取3次，都没读到不合格
                for (int i = 0; i < 5; i++)
                {
                    if (Stop) break;
                    bool resultReadTemp = ReadWc(ref arrCurWCValueP, ref arrCurrentWcNumP, 0);
                    int readcount = 0;
                    #region 解析临时读到的检定结论
                    for (int j = 0; j < MeterNumber; j++)
                    {
                        if (!MeterInfo[j].YaoJianYn) continue;
                        if (!bln_OffMeter[j])
                        {
                            if (!string.IsNullOrWhiteSpace(arrCurWCValueP[j]) && arrCurWCValueP[j] != "0")
                            {
                                float tmp = 999;
                                if (float.TryParse(arrCurWCValueP[j], out tmp) && Math.Abs(tmp) < 2 && tmp != 0)
                                {
                                    arrayReadEnergyP[j] = arrCurWCValueP[j];
                                    ++readcount;
                                }
                            }
                        }
                    }
                    #endregion
                    if (GetYaoJian().Count(v => v == true) == readcount) break;
                    if (Stop) break;
                    if (resultReadTemp)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        MessageAdd("读取误差失败，等待2秒后重试", EnumLogType.提示信息);
                        Thread.Sleep(2000);
                    }
                }
                #endregion
                if (Stop) break;
                #region 无功
                bool rePowerOn = false;
                if (OneMeterInfo.MD_WiringMode != "单相")
                {
                    rePowerOn = true;
                    MessageAdd("开始判断表位电能无功脉冲接线", EnumLogType.提示与流程信息);
                    //增加无功脉冲判断
                    StopWcb(0, 0xff);
                    //PowerOff();
                    if (Stop) break;
                    Thread.Sleep(1000);
                    if (Stop) break;
                    SetBluetoothModule(01);
                    if (Stop) break;
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, PowerWay.正向无功, "1.0");
                    if (Stop) break;

                    StdConst = GetStaConst();


                    MessageAdd("正在设置标准表脉冲...", EnumLogType.提示信息);
                    SetPulseType((1 + 49).ToString("x"));
                    FangXiang = PowerWay.正向无功;
                    //设备参数初始化，启动误差板
                    //设置误差版被检常数
                    if (Stop) break;
                    MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
                    SetStandardConst(01, (int)(StdConst / 100), -2, 0xff);
                    //设置误差版标准常数 TODO
                    MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
                    if (!SetTestedConst(01, meterconst, 0, circleCount))
                    {
                        MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                    }
                    StartWcb(01, 0xff);
                    twoPulseTime = 3 * GetOneErrorTime();
                    WaitTime("正在进行无功脉冲检查,等待电表走2个无功脉冲", twoPulseTime / 1000);
                    for (int i = 0; i < 5; i++)
                    {
                        if (Stop) break;
                        bool resultReadTemp = ReadWc(ref arrCurWCValueQ, ref arrCurrentWcNumQ, 1);
                        int readcount = 0;
                        #region 解析临时读到的检定结论
                        for (int j = 0; j < MeterNumber; j++)
                        {
                            if (!MeterInfo[j].YaoJianYn) continue;
                            if (!bln_OffMeter[j])
                            {
                                if (!string.IsNullOrWhiteSpace(arrCurWCValueQ[j]) && arrCurWCValueQ[j] != "0")
                                {
                                    float tmp = 999;
                                    if (float.TryParse(arrCurWCValueQ[j], out tmp) && Math.Abs(tmp) < 2 && tmp != 0)
                                    {
                                        arrayReadEnergyQ[j] = arrCurWCValueQ[j];
                                        ++readcount;
                                    }
                                }
                            }
                        }
                        #endregion
                        if (GetYaoJian().Count(v => v == true) == readcount) break;
                        if (Stop) break;
                        if (resultReadTemp)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            MessageAdd("读取误差失败，等待2秒后重试", EnumLogType.提示信息);
                            Thread.Sleep(2000);
                        }
                    }
                }

                #endregion

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!bln_OffMeter[i])
                    {
                        //
                        if (OneMeterInfo.MD_WiringMode != "单相")
                        {
                            bln_OffMeter[i] = string.IsNullOrEmpty(arrayReadEnergyP[i]) || string.IsNullOrEmpty(arrayReadEnergyQ[i]);//ZYF
                        }
                        else
                        {
                            bln_OffMeter[i] = string.IsNullOrEmpty(arrayReadEnergyP[i]);
                        }

                        if (bln_OffMeter[i])
                        {
                            ResultDictionary[$"电能脉冲状态"][i] = "×";
                            result[i] = false;
                            MessageAdd(string.Format("{0}表位电能脉冲检查失败！", i + 1), EnumLogType.提示信息);
                        }
                        else
                        {
                            ResultDictionary[$"电能脉冲状态"][i] = "√";
                        }
                    }
                }
                //刷新结论UI
                RefUIData("电能脉冲状态");
                StopWcb(1, 0xff);
                #endregion

                #region 电压和电流
                MessageAdd("开始判断标准表和被检表的电压电流", EnumLogType.提示与流程信息);
                if (rePowerOn)
                {
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                }
                if (Stop) break;

                //MessageAdd("读取电表的参数值....", EnumLogType.提示信息);
                string[] ValueV = MeterProtocolAdapter.Instance.ReadData("电压数据块"); //被测电能表电压
                string[] ValueA = MeterProtocolAdapter.Instance.ReadData("电流数据块"); //被测电能表电流

                //计算相应的误差
                MessageAdd("开始计算误差数据....", EnumLogType.提示信息);
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    //模型误差值
                    string valueV = string.Empty; //标准表电压
                    string valueA = string.Empty; //标准表电流
                    List<double> wcListV = new List<double>(); //电压误差数据集
                    List<double> wcListA = new List<double>(); //电流误差数据集
                    string wcValueV = string.Empty;  //电压误差值
                    string wcValueA = string.Empty;  //电流误差值

                    #region 电压
                    //电压
                    if (ValueV[j] == null) ValueV[j] = "0";
                    string[] strV = ValueV[j].Split(',');
                    float wc0V;
                    if (string.IsNullOrWhiteSpace(strV[0]))
                        wc0V = (0 - EquipmentData.StdInfo.Ua) / EquipmentData.StdInfo.Ua * 100;//UA
                    else
                        wc0V = (Convert.ToSingle(strV[0]) - EquipmentData.StdInfo.Ua) / EquipmentData.StdInfo.Ua * 100;//UA
                    wcListV.Add(wc0V);
                    wcValueV = wc0V.ToString("F2");
                    valueV = EquipmentData.StdInfo.Ua.ToString("F4");
                    if (strV.Length > 1 && strV[1] != "FFF.F")
                    {
                        float wc1V;
                        if (string.IsNullOrWhiteSpace(strV[1]))
                            wc1V = (0 - EquipmentData.StdInfo.Ub) / EquipmentData.StdInfo.Ub * 100;//UB
                        else
                            wc1V = (Convert.ToSingle(strV[1]) - EquipmentData.StdInfo.Ub) / EquipmentData.StdInfo.Ub * 100;//UB
                        float wc2V;
                        if (string.IsNullOrWhiteSpace(strV[2]))
                            wc2V = (0 - EquipmentData.StdInfo.Uc) / EquipmentData.StdInfo.Uc * 100;//UC
                        else
                            wc2V = (Convert.ToSingle(strV[2]) - EquipmentData.StdInfo.Uc) / EquipmentData.StdInfo.Uc * 100;//UC

                        if (OneMeterInfo.MD_WiringMode == "三相三线")
                        {
                            wc1V = 0f;
                            EquipmentData.StdInfo.Ub = 0f;
                        }

                        //添加出参
                        wcListV.Add(wc1V);
                        wcListV.Add(wc2V);

                        wcValueV += "," + wc1V.ToString("F2") + "," + wc2V.ToString("F2");//误差值
                        valueV += "," + EquipmentData.StdInfo.Ub.ToString("F4") + "," + EquipmentData.StdInfo.Uc.ToString("F4");//标准值
                    }
                    ResultDictionary["电压"][j] = ValueV[j];// + "|" + valueV + "|" + wcValueV
                    #endregion

                    #region 电流
                    //电流
                    if (ValueA[j] == null) ValueA[j] = "0";
                    string[] strA = ValueA[j].Split(',');
                    float wc0A;
                    if (string.IsNullOrWhiteSpace(strA[0]))
                        wc0A = (0 - EquipmentData.StdInfo.Ia) / EquipmentData.StdInfo.Ia * 100;//IA
                    else
                        wc0A = (Convert.ToSingle(strA[0]) - EquipmentData.StdInfo.Ia) / EquipmentData.StdInfo.Ia * 100;//IA
                    wcListA.Add(wc0A);
                    wcValueA = wc0A.ToString("F2");
                    valueA = EquipmentData.StdInfo.Ia.ToString("F4");
                    if (strA.Length > 1 && strA[1] != "FFF.F")
                    {
                        float wc1A, wc2A;
                        if (string.IsNullOrWhiteSpace(strA[1]))
                            wc1A = (0 - EquipmentData.StdInfo.Ib) / EquipmentData.StdInfo.Ib * 100;//IB
                        else
                            wc1A = (Convert.ToSingle(strA[1]) - EquipmentData.StdInfo.Ib) / EquipmentData.StdInfo.Ib * 100;//IB

                        if (string.IsNullOrWhiteSpace(strA[1]))
                            wc2A = (0 - EquipmentData.StdInfo.Ic) / EquipmentData.StdInfo.Ic * 100;//IC
                        else
                            wc2A = (Convert.ToSingle(strA[2]) - EquipmentData.StdInfo.Ic) / EquipmentData.StdInfo.Ic * 100;//IC

                        if (OneMeterInfo.MD_WiringMode == "三相三线")
                        {
                            wc1A = 0f;
                            EquipmentData.StdInfo.Ib = 0f;
                        }

                        //添加出参
                        wcListA.Add(wc1A);
                        wcListA.Add(wc2A);

                        wcValueA += "," + wc1A.ToString("F2") + "," + wc2A.ToString("F2");//误差值
                        valueA += "," + EquipmentData.StdInfo.Ib.ToString("F4") + "," + EquipmentData.StdInfo.Ic.ToString("F4");//标准值
                    }
                    ResultDictionary["电流"][j] = ValueA[j];// + "|" + valueA + "|" + wcValueA
                    #endregion
                    FangXiang = Core.Enum.PowerWay.正向有功;
                    string[] level = Number.GetDj(MeterInfo[j].MD_Grane);
                    float _MeterLevel = 1;
                    //_MeterLevel = MeterLevel(meterInfo[j]);//功能规范测量及监测1%，非等级

                    for (int k = 0; k < wcListA.Count; k++)
                    {
                        //检定结果 节点结论
                        if (result.Equals(false)) break;

                        if (double.IsNaN(wcListV[k]))
                            wcListV[k] = 0;
                        if (double.IsNaN(wcListA[k]))
                            wcListA[k] = 0;
                        //ResultDictionary["结论"][j] = Math.Abs(wcListA[k]) < _MeterLevel ? "合格" : "不合格";

                        if (Math.Abs(wcListV[k]) > _MeterLevel)
                        {
                            ResultDictionary["结论"][j] = "不合格";
                            result[j] = false;
                            MessageAdd(string.Format("{0}表位电压不正确！", j + 1), EnumLogType.提示与流程信息);
                        }
                        if (Math.Abs(wcListA[k]) > _MeterLevel)
                        {
                            ResultDictionary["结论"][j] = "不合格";
                            result[j] = false;
                            MessageAdd(string.Format("{0}表位电流不正确！", j + 1), EnumLogType.提示与流程信息);
                        }

                        if (result[j] == true)
                        {
                            ResultDictionary["结论"][j] = "合格";
                        }
                    }
                }

                //RefUIData(name);
                //RefUIData("结论");

                RefUIData("电压");
                RefUIData("电流");


                MessageAdd("开始关源....", EnumLogType.提示信息);
                //关源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    MessageAdd("升源失败", EnumLogType.提示信息);
                }

                if (Stop) break;

                #endregion

                if (Array.IndexOf(result, false) == -1)
                {
                    break;
                }
            }

            if (Stop) return;


            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (result[i])
                    {
                        ResultDictionary["结论"][i] = Core.ConstHelper.合格;
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = Core.ConstHelper.不合格;
                    }
                }
            }
            RefUIData("结论");

            #region 流水线隔离、告警判断
            if (ResultDictionary["结论"].Count(v => v == Core.ConstHelper.不合格) == MeterNumber)
            {
                MessageAdd("全部不合格！", EnumLogType.错误信息);
                PowerOff();
                EquipmentData.Controller.TryStopVerify();
                ViewModel.InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.故障, "接线检查全部不合格");
            }
            else if (ResultDictionary["结论"].Count(v => v == Core.ConstHelper.合格) == MeterNumber)
            {

            }
            else if (GetYaoJian().Count(v => v == true) <= 0)
            {
                MessageAdd("全部被设置不检！", EnumLogType.错误信息);
                PowerOff();
                EquipmentData.Controller.TryStopVerify();
                ViewModel.InnerCommand.VerifyControl.SendMsg(CtrlCmd.MsgType.故障, "接线检查全部被设置不检");
            }
            else if (GetYaoJian().Count(v => v == true) == ResultDictionary["结论"].Count(v => v == Core.ConstHelper.合格))
            {

            }
            else
            {
                if (Stop) return;
                //这里关闭没通过的表的继电器 
                if (VerifyConfig.UnqualifiedJumpOutOf)
                {
                    //PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                    MessageAdd("开始隔离故障表位。", EnumLogType.提示与流程信息);
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
                }
            }
            #endregion 流水线隔离、告警判断

            MessageAdd("接线检查完成", EnumLogType.提示与流程信息);

        }



        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            meterPrepare = new MeterPrepare[MeterNumber];
            xIb = Number.GetCurrentByIb("1.0Ib", OneMeterInfo.MD_UA, HGQ);
            ResultNames = new string[] { "电流开路状态", "485通讯状态", "状态字", "时钟脉冲状态", "电能脉冲状态", "电压", "电流", "结论" };
            //电流开路状态|485通讯状态|状态字|时钟脉冲状态|电能脉冲状态
            return true;
        }

        /// <summary>
        /// 计算出一个误差需要的时间ms
        /// </summary>
        ///<remarks>
        ///如果存在多种常数的电能表，则以最先出脉冲的电能表为准
        ///</remarks>
        /// <returns>出一个误差需要时间估算值,单位ms</returns>
        private int GetOneErrorTime()
        {
            //计算当前负载功率
            float current = Number.GetCurrentByIb("1.0Ib", OneMeterInfo.MD_UA, HGQ);
            float currentPower = base.CalculatePower(OneMeterInfo.MD_UB, current, Clfs, Core.Enum.Cus_PowerYuanJian.H, "1.0", IsYouGong);
            //计算一度大需要的时间,单位分钟
            float needTime = 1000F / currentPower * 60F;
            return base.OnePulseNeedTime(IsYouGong, needTime);
        }


    }
}
