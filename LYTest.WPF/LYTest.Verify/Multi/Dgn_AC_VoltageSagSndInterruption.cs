using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 交流电压骤降和中断
    /// </summary>
    public class Dgn_AC_VoltageSagSndInterruption : VerifyBase
    {
        public override void Verify()
        {
            MessageAdd("交流电压暂降和短时中断试验检定开始!", EnumLogType.提示与流程信息);
            base.Verify();

            //如何中途停止检定
            if (Stop) return;

            bool[] zjl = new bool[MeterNumber];
            zjl.Fill(true);
            //参数值
            string values = Test_Value;
            string[] condition = { "|" };
            string[] value = values.Split(condition, StringSplitOptions.None);

            //float[] xib = OneMeterInfo.GetIb(); 
            //float ib = xib[0];


            ////如果是IR46规程 ，电流
            //if (OneMeterInfo.MD_JJGC == "IR46")
            //{
            //    ib = 10 * ib;
            //}

            //float dwDL = ib;
            //if (ib < 10)
            //{
            //    dwDL = 10F;
            //}
            //else
            //{
            //    dwDL = 100F;
            //}
            ////标准表固定常数
            ////int constants = VerifyConfig.StdConst;
            //ulong constants = 1000000;
            ////设置标准表挡位
            //StdGear(0x13, constants, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, dwDL, dwDL, dwDL);
            //Thread.Sleep(3000);

            //float Zh311energys = 0f;
            //SetBluetoothModule(06);
            ////升源 只升电压 为了读取电量
            //PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

            if (!CheckVoltage())
            {
                return;
            }

            //读地址
            ReadMeterAddrAndNo();

            for (int k = 0; k < value.Length / 4; k++)
            {
                float[] energys = MeterProtocolAdapter.Instance.ReadEnergy((byte)PowerWay.正向有功, (byte)Cus_FeiLv.总);
                if (Stop) return;

                //关源
                PowerOff();
                WaitTime("关源", 5);
                string[] ps = new string[4];

                switch (k)
                {
                    case 0://%Un|间隔s|持续时间s|次数
                        ps[0] = value[0];//%Un
                        ps[1] = value[1];//间隔s
                        if (value[2] == "250" && value[6] == "1" && value[10] == "25" && value[14] == "0.5")
                            ps[2] = (float.Parse(value[2]) / 50.0000F).ToString();
                        else
                            ps[2] = value[2];//持续时间s
                        ps[3] = value[3];//次数
                        break;
                    case 1:
                        ps[0] = value[4];
                        ps[1] = value[5];
                        if (value[2] == "250" && value[6] == "1" && value[10] == "25" && value[14] == "0.5")
                            ps[2] = (float.Parse(value[6]) / 50.0000F).ToString();
                        else
                            ps[2] = value[6];
                        ps[3] = value[7];
                        break;
                    case 2:
                        ps[0] = value[8];
                        ps[1] = value[9];
                        if (value[2] == "250" && value[6] == "1" && value[10] == "25" && value[14] == "0.5")
                            ps[2] = (float.Parse(value[10]) / 50.0000F).ToString();
                        else
                            ps[2] = value[10];
                        ps[3] = value[11];
                        break;
                    case 3:
                        ps[0] = value[12];
                        ps[1] = value[13];
                        if (value[2] == "250" && value[6] == "1" && value[10] == "25" && value[14] == "0.5")
                            ps[2] = (float.Parse(value[14]) / 50.0000F).ToString();
                        else
                            ps[2] = value[14];
                        ps[3] = value[15];
                        break;
                }

                if (Stop) return;
                MessageAdd(string.Format("开始第{0}次交流电压骤降和中断试验，电压降低{4}%, 正常输出{1}秒，降低持续{2}秒, 试验{3}次。", k + 1, ps[1], ps[2], ps[3], ps[0]), EnumLogType.提示与流程信息);

                //读起码  //也就是读取电表初始电量
                //float[] energys = MeterProtocolAdapter.Instance.ReadEnergyGJD((byte)PowerWay.正向有功, (byte)Cus_FeiLv.总);
                //float[] energys = MeterProtocolAdapter.Instance.ReadEnergy((byte)PowerWay.正向有功, (byte)Cus_FeiLv.总);
                for (int i = 0; i < MeterNumber; i++)
                {
                    ResultDictionary["起码" + (k + 1)][i] = energys[i].ToString("f4");
                }
                RefUIData("起码" + (k + 1));

                CheckOver = false;


                if (float.Parse(ps[2]) == 0 || ps[2] == null || ps[2] == "")
                {
                    continue;
                }

                //3.设置开断时间
                AC_VoltageSagSndInterruption(
                    int.Parse(ps[2].Trim()) * 100,
                    int.Parse(ps[1].Trim()) * 100,
                    int.Parse(ps[3].Trim()),
                    int.Parse(ps[0].Trim()),
                    "1", "1", "1");

                if (!CheckVoltage())
                    return;

                DateTime lastTime = DateTime.Now;
                float YXtime = float.Parse(ps[1]) + float.Parse(ps[2]);
                float MaxTime = YXtime * float.Parse(ps[3]);

                while (true)
                {
                    if (Stop) return;
                    Thread.Sleep(1000);


                    float pastTime = (float)DateTime.Now.Subtract(lastTime).TotalSeconds;
                    if (pastTime >= MaxTime) CheckOver = true;
                    int zq = (int)(pastTime / YXtime) + 1;
                    string msg = string.Format("实验的时间为：{0}秒，已经实验：{1}秒，当前为第{2}次。", Math.Round(MaxTime, 2), Math.Round(pastTime, 2), zq.ToString());
                    MessageAdd(msg, EnumLogType.提示信息);
                    if (CheckOver) break;
                }

                //关源
                PowerOff();
                WaitTime("关源", 5);
                if (!CheckVoltage())
                    return;

                if (Stop) return;
                float[] energysEnd = MeterProtocolAdapter.Instance.ReadEnergy((byte)PowerWay.正向有功, (byte)Cus_FeiLv.总);
                for (int i = 0; i < MeterNumber; i++)
                {
                    ResultDictionary["止码" + (k + 1)][i] = energysEnd[i].ToString("f4");
                }
                RefUIData("止码" + (k + 1));

                if (Stop) return;

                for (int i = 0; i < MeterNumber; i++)
                {
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
                    if (wcCz > limit.UpLimit)
                    {
                        boolRst = false;
                    }
                    ResultDictionary["累计电量" + (k + 1)][i] = wcCz.ToString("f4");
                    ResultDictionary["误差" + (k + 1)][i] = wcCz.ToString("f4");

                    if (!boolRst)
                    {
                        zjl[i] = false;
                    }

                }
                ///刷新结论
                RefUIData("累计电量" + (k + 1));
                RefUIData("误差" + (k + 1));
            }
            //刷新总结论
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["结论"][i] = zjl[i] ? "合格" : "不合格";
            }

            RefUIData("结论");

            MessageAdd("交流电压暂降和短时中断试验检定完成!", EnumLogType.提示与流程信息);

        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //ResultNames = new string[] {"起码1", "止码1", "累计电量1", "标准电量1", "误差1", "起码2", "止码2", "累计电量2", "标准电量2", "误差2", "起码3", "止码3", "累计电量3", "标准电量3", "误差3", "起码4", "止码4", "累计电量4", "标准电量4", "误差4", "结论" };
            ResultNames = new string[] { "起码1", "止码1", "累计电量1", "误差1", "起码2", "止码2", "累计电量2", "误差2", "起码3", "止码3", "累计电量3", "误差3", "起码4", "止码4", "累计电量4", "误差4", "结论" };
            return true;
        }
    }
}
