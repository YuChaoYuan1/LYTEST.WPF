using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Struct;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;

namespace LYTest.Verify.Multi
{
    ////add yjt 20230131 合并修改负载点电流快速改变
    ///add lsj 20220218     //负载点电流快速改变
    //负载点电流快速改变
    public class Dgn_CurrentChangesRapidly : VerifyBase
    {
        //5 5 4    10 10 4     5 0.5 4
        //电流电路应在开通和关断状态之间重复切换,读取电表电量和误差
        public override void Verify()
        {
            try
            {
                MessageAdd("负载电流快速改变试验检定开始!", EnumLogType.提示与流程信息);
                base.Verify();

                //如何中途停止检定
                if (Stop) return;

                bool[] zjl = new bool[MeterNumber];
                zjl.Fill(true);
                //参数值
                string values = Test_Value;
                string[] condition = { "|" };
                string[] value = values.Split(condition, StringSplitOptions.None);

                float ib = Number.GetCurrentByIb("Ib", OneMeterInfo.MD_UA, HGQ);
                _ = ib * 2;
                float dwDL;
                if (ib < 10)
                    dwDL = 10F;
                else
                    dwDL = 100F;

                //标准表固定常数
                ulong constants = EquipmentData.DeviceManager.GetStdConst(dwDL);
                //设置标准表挡位
                ConfigHelper.Instance.AutoGearTemp = false;
                VerifyConfig.Test_ZouZi_Control = true;
                StdGear(0x13, constants, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, dwDL, dwDL, dwDL);
                Thread.Sleep(3000);

                Thread.Sleep(50);

                float Zh311energys = 0f;
                SetBluetoothModule(06);
                //升源 只升电压 为了读取电量
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

                //读地址
                ReadMeterAddrAndNo();

                for (int k = 0; k < value.Length / 3; k++)
                {
                    if (Stop) return;

                    string[] ps = new string[3];

                    switch (k)
                    {
                        case 0:
                            ps[0] = value[0];
                            ps[1] = value[1];
                            ps[2] = value[2];
                            break;
                        case 1:
                            ps[0] = value[3];
                            ps[1] = value[4];
                            ps[2] = value[5];
                            break;
                        case 2:
                            ps[0] = value[6];
                            ps[1] = value[7];
                            ps[2] = value[8];
                            break;
                    }

                    if (Stop) return;
                    MessageAdd(string.Format("开始做第{0}次负载电流快速改变试验, 开{1}秒，关{2}秒, 持续时间{3}分钟。", k + 1, ps[0], ps[1], Math.Round(float.Parse(ps[2]) * 60, 2)), EnumLogType.提示与流程信息);
                    //读起码
                    float[] energys = MeterProtocolAdapter.Instance.ReadEnergy((byte)PowerWay.正向有功, (byte)Cus_FeiLv.总);
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        ResultDictionary["起码" + (k + 1)][i] = energys[i].ToString();
                    }
                    RefUIData("起码" + (k + 1));

                    CheckOver = false;


                    if (float.Parse(ps[2]) == 0 || ps[2] == null || ps[2] == "")
                    {
                        continue;
                    }

                    //3.设置开断时间
                    SetCurrentChangeByPower(Convert.ToInt32(float.Parse(ps[0].Trim()) * 100), Convert.ToInt32(float.Parse(ps[1].Trim()) * 100), "1", "1", "1");

                    //启动标准表
                    StartStdEnergy(31);
                    //启动误差板脉冲计数
                    StartWcb(0x06, 0xff);

                    //4.升源
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, ib, ib, ib, Core.Enum.Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

                    DateTime lastTime = DateTime.Now.AddSeconds(-4);
                    float MaxTime = float.Parse(ps[2]) * 60 * 60;

                    while (true)
                    {
                        if (Stop) return;
                        try
                        {
                            Zh311energys = float.Parse((float.Parse(ReadEnrgyZh311()[9]) / 3600 / 1000).ToString("0.000000"));
                        }
                        catch 
                        {
                            //读标准标电量
                            float[] bzbdl = ReadStmEnergy();

                            if (bzbdl != null)
                            {
                                Zh311energys = bzbdl[9];
                            }
                        }

                        float pastTime = (float)DateTime.Now.Subtract(lastTime).TotalSeconds;
                        if (pastTime >= MaxTime) CheckOver = true;
                        float time = pastTime / 60F;
                        string msg = string.Format("实验的时间为：{0}分，已经实验：{1}分, 走电量{2} KW/h", Math.Round(float.Parse(ps[2]) * 60, 2), Math.Round(time, 2), Zh311energys.ToString());
                        MessageAdd(msg, EnumLogType.提示信息);
                        if (CheckOver) break;

                    }

                    //关源
                    PowerOff();
                    WaitTime("关源", 5);
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

                    //读取止码
                    if (Stop) return;
                    float[] energysEnd = MeterProtocolAdapter.Instance.ReadEnergy((byte)PowerWay.正向有功, (byte)Cus_FeiLv.总);
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        ResultDictionary["止码" + (k + 1)][i] = energysEnd[i].ToString();
                    }
                    RefUIData("止码" + (k + 1));

                    //标准表电量
                    try
                    {
                        Zh311energys = float.Parse((float.Parse(ReadEnrgyZh311()[9]) / 3600 / 1000).ToString("0.000000"));
                    }
                    catch 
                    {
                        //读标准标电量
                        float[] bzbdl = ReadStmEnergy();

                        if (bzbdl != null)
                        {
                            Zh311energys = bzbdl[9];
                        }
                    }

                    if (Stop) return;

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (Stop) return;
                        //判断是否为要检表
                        if (!MeterInfo[i].YaoJianYn) continue;
                        float level = MeterLevel(OneMeterInfo);        //当前表的等级
                                                                       //设置计算参数
                        ErrorLimit limit = new ErrorLimit
                        {
                            IsSTM = false,
                            UpLimit = level
                        };

                        //当前检定项结论
                        bool boolRst = true;
                        //标准表电量-（电表结束电量-电表开始电量）
                        float wcCz = energysEnd[i] - energys[i];
                        float wc = Math.Abs((Zh311energys - (wcCz)) / Zh311energys) * 100;
                        if (wc > limit.UpLimit)
                        {
                            boolRst = false;
                        }
                        ResultDictionary["累计电量" + (k + 1)][i] = Math.Round(wcCz, 4).ToString();
                        ResultDictionary["标准电量" + (k + 1)][i] = Zh311energys.ToString();
                        ResultDictionary["误差" + (k + 1)][i] = wc.ToString("F4");

                        //if (ResultDictionary["结论"][i] == "合格")
                        //{
                        //    ResultDictionary["结论"][i] = boolRst ? "合格" : "不合格";
                        //}
                        //else
                        //{
                        //    ResultDictionary["结论"][i] = "不合格";
                        //    zjl[i] = false;
                        //}
                        if (!boolRst)
                        {
                            zjl[i] = false;

                        }
                    }
                    ///刷新结论
                    RefUIData("累计电量" + (k + 1));
                    RefUIData("标准电量" + (k + 1));
                    RefUIData("误差" + (k + 1));
                    ////关源
                    //PowerOff();
                    //WaitTime("关源，等待源稳定", 5);

                }
                //刷新总结论
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (zjl[i] == false)
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "合格";
                    }
                }

                RefUIData("结论");

                MessageAdd("负载电流快速改变试验检定完成!", EnumLogType.提示与流程信息);
            }
            finally
            {
                ConfigHelper.Instance.AutoGearTemp = true;
                VerifyConfig.Test_ZouZi_Control = false;
                SetCurrentChangeByPower(0, 0, "0", "0", "0");
            }
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "起码1", "止码1", "累计电量1", "标准电量1", "误差1", "起码2", "止码2", "累计电量2", "标准电量2", "误差2", "起码3", "止码3", "累计电量3", "标准电量3", "误差3", "结论" };
            return true;
        }


    }
}

