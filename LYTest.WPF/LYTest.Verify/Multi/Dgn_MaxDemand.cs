using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 需量示值误差
    /// </summary>
    public class Dgn_MaxDemand : VerifyBase
    {

        private PowerWay powerWay = PowerWay.正向有功;
        private int demandPeriod = 15;//需量周期
        private int slipTimes = 1;//滑差时间
        private int slipPage; //滑差次数
        private string limit = "";

        private float ib;
        string name = "0.1Ib";

        public float dmdCurrentt;
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("需量示值误差试验检定开始...", EnumLogType.流程信息);

            //开始检定
            base.Verify();

            //add yjt 20220327 新增演示模式
            if (IsDemo)
            {
                if (Stop) return;
                ErrorLimit[] limit = new ErrorLimit[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;
                    float curLevel = Number.SplitKF(meter.MD_Grane, IsYouGong);
                    float errorLevel = GetErrorLevel(curLevel);
                    limit[i] = new ErrorLimit
                    {
                        IsSTM = false,
                        MeterLevel = curLevel,

                        UpLimit = errorLevel,
                        DownLimit = -errorLevel
                    };

                    float sj = CalculatePower(U, dmdCurrentt, Clfs, Cus_PowerYuanJian.H, "1.0", IsYouGong) / 1000f;
                    float bz = sj;
                    float wc = sj - bz;

                    ResultDictionary["电流"][i] = name;
                    ResultDictionary["误差上限"][i] = limit[i].UpLimit.ToString();
                    ResultDictionary["误差下限"][i] = limit[i].DownLimit.ToString();
                    ResultDictionary["实际需量"][i] = sj.ToString("f4");
                    ResultDictionary["标准需量"][i] = bz.ToString("f4");
                    ResultDictionary["需量误差"][i] = wc.ToString("f2");
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }
                RefUIData("电流");
                RefUIData("误差上限");
                RefUIData("误差下限");
                RefUIData("实际需量");
                RefUIData("标准需量");
                RefUIData("需量误差");
                RefUIData("结论");
            }
            else
            {
                int maxMinute = demandPeriod + slipTimes * slipPage;
                int maxTime = maxMinute * 60;
                for (int i = 0; i < MeterNumber; i++)
                {
                    ResultDictionary["电流"][i] = name;
                }
                RefUIData("电流");

                if (!CheckVoltage())
                    return;

                if (Stop) return;
                ReadMeterAddrAndNo();
                if (Stop) return;
                Identity();  //身份认证
                if (Stop) return;
                //第一步：清空需量

                if (DAL.Config.ConfigHelper.Instance.Dgn_ClearWhenMaxDemand)
                {
                    //将GPS时间写到表中
                    if (Stop) return;
                    MessageAdd("开始写表时间......", EnumLogType.提示信息);
                    MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                    if (Stop) return;
                    MessageAdd("开始清空需量......", EnumLogType.提示信息);
                    if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                    MeterProtocolAdapter.Instance.ClearDemand();
                }
                if (Stop) return;
                MessageAdd("开始做最大需量......", EnumLogType.提示信息);


                // 由于经互感器表0.003-0.015(1.2)的表的0.1Ib时功率较小，导致通过机率少
                // 从测试效果来看，
                float current = dmdCurrentt;
                if (current <= 0.05 && HGQ)
                    //current *= 1.15f;
                    current *= 1.0034f;

                //current *= Clfs==WireMode.三相三线?  1.25f:1.15f;


                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, current, current, current, Cus_PowerYuanJian.H, powerWay, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    return;
                }
                ////同时做需量周期脉冲
                //if (PrjPara[3] == "1" && pq == 0)
                //{
                //    //电能表脉冲切换到需量周期
                //    MeterProtocolAdapter.Instance.SetPulseCom(1);
                //    EquipHelper.Instance.InitPara_InitDemandPeriod(demandPeriod, slipPage);
                //    if (!EquipHelper.Instance.InitPara_InitDemandPeriod(demandPeriod, slipPage)) return;
                //}
                if (Stop) return;
                ErrorLimit[] errlimit = new ErrorLimit[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;
                    float curLevel = Number.SplitKF(meter.MD_Grane, IsYouGong);
                    float errorLevel = GetErrorLevel(curLevel);
                    errlimit[i] = new ErrorLimit
                    {
                        IsSTM = false,
                        MeterLevel = curLevel,

                        UpLimit = errorLevel,
                        DownLimit = -errorLevel
                    };
                    if (!string.IsNullOrEmpty(this.limit))
                    {
                        errlimit[i].UpLimit = float.Parse(this.limit);
                        errlimit[i].DownLimit = -errlimit[i].UpLimit;


                    }
                    ResultDictionary["误差上限"][i] = errlimit[i].UpLimit.ToString();
                    ResultDictionary["误差下限"][i] = errlimit[i].DownLimit.ToString();
                }
                RefUIData("误差上限");
                RefUIData("误差下限");

                StartTime = DateTime.Now;
                float standMeterP = 0;
                int PastMinute = 0;
                float[] meterXL = new float[MeterNumber];    //被检表最大需量
                MessageAdd("开始检定", EnumLogType.提示信息);
                //标准表功率
                while (true)
                {
                    if (Stop) return;
                    Thread.Sleep(2000);

                    int pastTime = (int)VerifyPassTime;
                    int tempMinute = (int)(VerifyPassTime / 55);
                    if (VerifyPassTime >= maxTime)
                    {
                        MessageAdd("检定时间达到方案预定时间，检定完成", EnumLogType.提示信息);
                        break;
                    }
                    else
                    {
                        pastTime *= 100;
                        pastTime /= 60;
                        float curPorcess = pastTime / 100F;
                        //App.CUS.NowMinute = curPorcess;
                        MessageAdd(string.Format("{0}周期误差检定需要{1}分，已经进行{2}分", powerWay.ToString(), maxMinute, curPorcess), EnumLogType.提示信息);
                    }
                    //有功/无功功率
                    float stmPower = EquipmentData.StdInfo.P; //标准有功功率
                    if (!IsYouGong)
                        stmPower = EquipmentData.StdInfo.Q;   //标准无功功率

                    //这个需要刷新到界面
                    standMeterP = (float)(Math.Truncate(stmPower / 1000 * 1000000) / 1000000);//Math.Abs(curStandMeterP) / 1000;

                    //每一次分钟读取一次需量数据
                    if (tempMinute > PastMinute)
                    {
                        PastMinute = tempMinute;
                        byte curPD = (byte)powerWay;
                        if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;
                        float[] dv1 = MeterProtocolAdapter.Instance.ReadDemand(curPD, (byte)0);
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn) continue;
                            float readXl = float.Parse(dv1[i].ToString("#0.000000"));

                            if (meterXL[i] < readXl)
                                meterXL[i] = readXl;    //记录下需量
                            ResultDictionary["实际需量"][i] = meterXL[i].ToString("f5");
                            ResultDictionary["标准需量"][i] = standMeterP.ToString("f5");
                        }
                        RefUIData("实际需量");
                        RefUIData("标准需量");
                    }
                }

                float[] demandValue = MeterProtocolAdapter.Instance.ReadDemand((byte)powerWay, 0);
                bool bResult = true;
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (MeterInfo[j].YaoJianYn)
                    {
                        if (demandValue[j] == 0)
                        {
                            bResult = false;
                            break;
                        }
                    }
                }
                if (!bResult)
                    demandValue = MeterProtocolAdapter.Instance.ReadDemand((byte)powerWay, 0);
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;

                    float readXl = demandValue[i];
                    if (meterXL[i] < readXl)
                        meterXL[i] = readXl;    //记录下需量
                    ResultDictionary["实际需量"][i] = meterXL[i].ToString("f5");
                }
                RefUIData("实际需量");
                //if (PrjPara[3] == "1" && pq == 0)
                //{
                //    ControlXLZQError();//处理需量周期误差
                //}

                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;

                    float meterD = (float)Math.Round(meterXL[i], 5);
                    float refD = (float)Math.Round(standMeterP, 5);
                    MeterDgn result = SetWuCha(errlimit[i], meterD, refD);
                    ResultDictionary["需量误差"][i] = result.Value;
                    ResultDictionary["结论"][i] = result.Result;
                    if (result.Result == ConstHelper.不合格)
                    {
                        NoResoult[i] = "误差超出误差限";
                    }

                    //add
                    ResultDictionary["实际需量"][i] = "+" + meterD.ToString("f5");
                    ResultDictionary["标准需量"][i] = "+" + refD.ToString("f5");

                    //ResultDictionary["需量误差"][i] = result.Value;

                }
                RefUIData("实际需量");
                RefUIData("标准需量");
                RefUIData("需量误差");
                RefUIData("结论");
            }

            //add yjt 20220415 新增需量功能做完后再一次清空需量
            MessageAdd("做完再清空一次需量", EnumLogType.提示信息);
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;

            if (DAL.Config.ConfigHelper.Instance.Dgn_ClearWhenMaxDemand)
                MeterProtocolAdapter.Instance.ClearDemand();

            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("需量示值误差试验检定结束...", EnumLogType.提示信息);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {

            //参数：电流，功率方向，需量周期，滑差时间，误差次数
            string[] tem = Test_Value.Split('|');
            ib = Number.GetCurrentByIb("ib", OneMeterInfo.MD_UA, HGQ);

            name = tem[0];
            dmdCurrentt = Number.GetCurrentByIb(tem[0], OneMeterInfo.MD_UA, HGQ);


            powerWay = (PowerWay)Enum.Parse(typeof(PowerWay), tem[1]); //功率方向
            int.TryParse(tem[2], out demandPeriod);
            int.TryParse(tem[3], out slipTimes);
            int.TryParse(tem[4], out slipPage);
            if (tem.Length >= 6 && float.TryParse(tem[5], out _))
            {
                limit = tem[5];
            }


            //电流（imax，ib，0.1ib），误差上限，误差下限，结论
            ResultNames = new string[] { "电流", "误差上限", "误差下限", "标准需量", "实际需量", "需量误差", "结论" };
            return true;
        }

        /// <summary>
        /// 根据南网标准计算需量示值误差限
        /// </summary>
        /// <param name="meterLevel">电能表等级</param>
        /// <returns></returns>
        private float GetErrorLevel(float meterLevel)
        {
            //表等级+0.05*额定功率/实际功率
            //标准功率

            WireMode clfs = (WireMode)Enum.Parse(typeof(WireMode), OneMeterInfo.MD_WiringMode);
            float strandPower = CalculatePower(OneMeterInfo.MD_UB, ib, clfs, Cus_PowerYuanJian.H, "1.0", IsYouGong);
            //负载功率
            float current = dmdCurrentt;

            float currentPower = CalculatePower(OneMeterInfo.MD_UB, current, clfs, Cus_PowerYuanJian.H, "1.0", IsYouGong);
            if (currentPower == 0)
            {
                if (strandPower == 0) currentPower = 1;
                else currentPower = strandPower;
            }
            return (float)Math.Round(meterLevel + 0.05F * strandPower / currentPower, 4);


        }

        /// <summary>
        /// 计算电能表最大需量误差
        /// </summary>
        /// <param name="meterData">电表实际需量</param>
        /// <returns>多功能数据结构体 MeterDgn </returns>
        public MeterDgn SetWuCha(ErrorLimit ErrLimit, float meterData, float refData)
        {
            //计算标准功率
            //float starndP = float.Parse(refData);

            //string strStarndP = refData.ToString("F6");
            //if (refData > 0)
            //    strStarndP = "+" + strStarndP;

            //string strMeterP = meterData.ToString("F5");
            //if (meterData > 0)
            //    strMeterP = "+" + strMeterP;

            //OtherData = "";

            //修约间距
            //float space = GetWuChaHzzJianJu(false, ErrLimit.MeterLevel);
            //int hzPrecision = Common.GetPrecision(space.ToString());

            float xlError = Number.GetRelativeWuCha(meterData, refData); //误差值
            //float hz = Number.GetHzz(xlError, space);
            //string err = hz.ToString("F" + hzPrecision);
            string err = Math.Round(xlError, 2).ToString("F2"); //.ToString("F" + hzPrecision);

            if (xlError >= 0)
                err = "+" + err;
            //else
            //    err = "-" + Math.Abs(float.Parse(err)).ToString("F" + hzPrecision);

            MeterDgn ret = new MeterDgn
            {
                //Value = string.Format("{0}|{1}|{2}", strStarndP, strMeterP, err)
            };
            if (meterData != 0f && Math.Abs(xlError) <= ErrLimit.UpLimit)
            {
                ret.Result = ConstHelper.合格;
            }
            else
            {
                ret.Result = ConstHelper.不合格;
            }
            //ret.Value = xlError.ToString();
            ret.Value = err;
            return ret;
        }

    }
}
