using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Struct;
using LYTest.Core;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;
using LYTest.Core.Model.Meter;

namespace LYTest.Verify.AccurateTest
{
    /// <summary>
    /// 潜动试验
    /// 只记录当前方向结论
    /// </summary>
    class Creep2 : VerifyBase
    {
        #region 参数
        float creepI = 9999999F;
        //检测系统参照规程
        float[] arrCreepTimes = new float[0];


        /// <summary>
        /// 电压倍数
        /// </summary>
        public float FloatxU = 1.15F;
        /// <summary>
        /// 电流倍数(数字)这个地方的倍数是指起动电流的倍数
        /// </summary>
        public float FloatxIb;
        /// <summary>
        /// 潜动时间，现在已经再是时间倍数，如果不为0，则是确切的潜动时间
        /// </summary>
        public float xTime;
        /// <summary>
        /// 潜动电流值
        /// </summary>
        public float CreepIb;
        /// <summary>
        /// 实际试验时间（分钟）
        /// </summary>
        public float CheckTime;

        int wcCount = 6;//误差计数
        #endregion
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("潜动试验检定开始...", EnumLogType.流程信息);
            float TotalTime = InitVerifyPara();
            base.Verify();
            if (Stop) return;
            //计算最大潜动时间
            float maxCreepTime = TotalTime * 60;

            //add yjt 20220320 新增演示模式的判断  默认合格 或者 演示模式
            if (DefaultValue || IsDemo)
            {
                int totalTime = 3000;
                while (totalTime > 0)
                {
                    MessageAdd("方案设置默认合格,等待" + (totalTime / 1000) + "秒", EnumLogType.提示信息);
                    if (Stop) break;
                    Thread.Sleep(1000);
                    totalTime -= 1000;
                }
                for (int Num = 0; Num < MeterNumber; Num++)
                {
                    if (!MeterInfo[Num].YaoJianYn) continue;

                    ResultDictionary["试验电流"][Num] = creepI.ToString("F2");
                    ResultDictionary["开始时间"][Num] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    ResultDictionary["功率方向"][Num] = FangXiang.ToString();
                    ResultDictionary["试验电压"][Num] = (FloatxU * MeterInfo[Num].MD_UB).ToString("F4");
                    ResultDictionary["实际运行时间"][Num] = (VerifyPassTime / 60.0).ToString("F4") + "分";
                    ResultDictionary["结论"][Num] = ConstHelper.合格;
                    ResultDictionary["脉冲数"][Num] = "0";  //add  zjl 20220216 增加脉冲显示
                    arrCreepTimes[Num] = arrCreepTimes[Num] / 60;
                }
                ConvertTestResult("标准试验时间", arrCreepTimes, 2);
                RefUIData("试验电压");
                RefUIData("试验电流");
                RefUIData("开始时间");
                RefUIData("功率方向");
                RefUIData("标准试验时间");
                RefUIData("实际运行时间");
                RefUIData("脉冲数"); //add  zjl 20220216 增加脉冲显示
                RefUIData("结论");
            }//modify zjl 20220216 修改默认合格
            else
            {
                #region

                if (Stop) return;
                //初始化设置
                int[] creepTimes = new int[MeterNumber];
                for (int bw = 0; bw < MeterNumber; bw++)
                {
                    creepTimes[bw] = (int)(arrCreepTimes[bw] * 60F);
                }

                SetBluetoothModule(wcCount);
                MessageAdd("正在升源...", EnumLogType.提示信息);
                //控制源输出
                if (!PowerOn(OneMeterInfo.MD_UB * FloatxU, OneMeterInfo.MD_UB * FloatxU, OneMeterInfo.MD_UB * FloatxU, creepI, creepI, creepI, Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    return;
                }
                StartTime = DateTime.Now.AddSeconds(-VerifyConfig.Dgn_PowerSourceStableTime);
                //TODO先测试
                MessageAdd("正在初始化误差板...", EnumLogType.提示信息);

                StartWcb(wcCount, 0xff);   //打开误差版计数
                if (Stop) return;

                //modify yjt 20220601 移动到非默认合格里面
                #region 上报试验参数
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        ResultDictionary["试验电流"][i] = creepI.ToString("F2");
                        ResultDictionary["开始时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        ResultDictionary["功率方向"][i] = FangXiang.ToString();
                        ResultDictionary["试验电压"][i] = (FloatxU * MeterInfo[i].MD_UB).ToString("F4");
                    }
                }
                ConvertTestResult("标准试验时间", arrCreepTimes, 2);
                RefUIData("试验电压");
                RefUIData("试验电流");
                RefUIData("开始时间");
                RefUIData("功率方向");
                RefUIData("标准试验时间");
                #endregion
                //add yjt 20220224 移动到非默认合格里面
                Stop = false;
                MessageAdd("开始检定...", EnumLogType.提示信息);
                while (true)
                {
                    long _PastTime = VerifyPassTime;
                    //每一秒刷新一次数据
                    Thread.Sleep(1000);
                    if (Stop)
                    {
                        MessageAdd("外部停止，退出检定", EnumLogType.提示信息);
                        //关闭误差板，关源
                        //PowerOff();
                        //StopWcb();
                        //m_CheckOver = true;
                        StopWcb(wcCount, 0xff);
                        return;
                    }

                    //读取误差板脉冲
                    ReadAndDealData(_PastTime);


                    MessageAdd("潜动时间" + TotalTime.ToString("F2") + "分，已经经过" + (_PastTime / 60F).ToString("F2") + "分", EnumLogType.提示信息);

                    if ((_PastTime > maxCreepTime) || CheckOver)
                    {
                        CheckOver = true;
                        MessageAdd("潜动时间已到，退出检定", EnumLogType.提示信息);
                        break;
                    }
                    if (VerifyConfig.Test_QuickModel)
                    {
                        break;
                    }
                }

                if (!IsDemo && !Stop && !CheckOver)
                {
                    //完了再读一次，以防万一
                    ReadAndDealData((long)maxCreepTime);
                }

                #endregion

                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0");
                IsInitIto = true;
            }//DefaultValue || IsDemo

            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    string resultTemp = ResultDictionary["结论"][i];
                    ResultDictionary["结论"][i] = resultTemp == "不合格" ? "不合格" : "合格";
                    ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (resultTemp == "合格")
                    {
                        ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
            RefUIData("实际运行时间");
            RefUIData("结束时间");
            RefUIData("结论");

            //关闭误差板，关源
            //Helper.EquipHelper.Instance.SetCurFunctionOnOrOff(false, 3);
            //PowerOff();
            //StopWcb();
            if (!IsDemo)
            {
                StopWcb(wcCount, 0xff);
            }
            CheckOver = true;

            //add yjt 20220305 新增日志提示
            MessageAdd("检定完成", EnumLogType.提示信息);
            MessageAdd("潜动试验检定结束...", EnumLogType.流程信息);
        }

        protected override bool CheckPara()
        {
            //功率方向|潜动电压|潜动电流|自动计算潜动时间|是否默认合格|潜动时间
            string[] arrayTemp = Test_Value.Split('|');
            if (arrayTemp.Length != 6)
                return false;

            //获得功率方向
            FangXiang = (PowerWay)Enum.Parse(typeof(PowerWay), arrayTemp[0]);

            //电压倍数
            if (float.TryParse(arrayTemp[1].Replace("%", ""), out float floatTemp))
                FloatxU = floatTemp / 100F;
            else
                FloatxU = 1.15F;

            //潜动电流
            switch (arrayTemp[2])
            {
                case "1/4启动电流":
                    FloatxIb = 0.25F;
                    break;
                case "1/5启动电流":
                    FloatxIb = 0.2F;
                    break;
            }

            //潜动时间
            if (arrayTemp[3] == "否")
            {
                if (float.TryParse(arrayTemp[5], out float fTemp))
                {
                    xTime = fTemp;
                }
                else
                {
                    xTime = 0;
                }
            }
            switch (arrayTemp[0])
            {
                case "正向有功":
                    wcCount = 6;
                    break;
                case "正向无功":
                    wcCount = 7;
                    break;
                case "反向有功":
                    wcCount = 6;
                    break;
                case "反向无功":
                    wcCount = 7;
                    break;
                default:
                    break;
            }
            DefaultValue = arrayTemp[4] == "是";
            //xTime
            //功率方向|试验电压|标准试验时间|试验电流|开始时间|结束时间|实际运行时间|脉冲数
            ResultNames = new string[] { "功率方向", "试验电压", "标准试验时间", "试验电流", "开始时间", "结束时间", "实际运行时间", "脉冲数", "结论" };
            return true;
        }


        /// <summary>
        /// 读取并处理数据[演示版无效]
        /// </summary>
        /// <param name="verifyTimes"></param>
        private void ReadAndDealData(long verifyTimes)
        {

            //stError[] arrTagError = ReadWc(FangXiang);
            StError[] arrTagError = ReadWcbData(GetYaoJian(), wcCount);
            //  stError[] arrTagError = null;
            if (Stop)
            {
                return;
            }
            //当所有表位均为不合格时,检定完毕
            CheckOver = true;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (arrTagError[i] == null) continue;
                if (!MeterInfo[i].YaoJianYn) continue;

                int.TryParse(arrTagError[i].szError, out int intTemp);
                ResultDictionary["脉冲数"][i] = intTemp.ToString();
                //分析数据
                //如果脉冲数大于0,不合格
                if (intTemp > 0)
                {
                    if (ResultDictionary["结论"][i] != ConstHelper.不合格)
                    {
                        NoResoult[i] = "规程潜动时间内出了电能脉冲";
                        ResultDictionary["实际运行时间"][i] = (verifyTimes / 60.0).ToString("F4") + "分";
                        ResultDictionary["结束时间"][i] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                    }
                }
                else
                {
                    ResultDictionary["实际运行时间"][i] = (((float)verifyTimes) / 60.0).ToString("F4") + "分";
                    CheckOver = false;
                }
            }
            RefUIData("实际运行时间");
            RefUIData("结束时间");
            RefUIData("脉冲数");

        }
        #region MyRegion

        /// <summary>
        /// 初始化潜动参数
        /// </summary>
        /// <returns></returns>
        private float InitVerifyPara()
        {
            switch (OneMeterInfo.MD_JJGC)
            {
                case "JJG596-2012":
                    break;
                case "IR46":
                case "Q/GDW12175-2021":
                case "Q/GDW10827-2020":
                case "Q/GDW10364-2020":
                    FloatxU = 1.1F;
                    break;
            }
            arrCreepTimes = new float[MeterNumber];

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn)
                {
                    meter = OneMeterInfo;
                }
                WireMode clfs = (WireMode)Enum.Parse(typeof(WireMode), meter.MD_WiringMode);
                bool CheckDevice = meter.MD_CheckDevice == "有止逆";
                bool ConnectionFlag = meter.MD_ConnectionFlag != "直接式";

                StPlan_QianDong.CheckTimeAndIb(meter.MD_JJGC, clfs, meter.MD_UB, meter.MD_UA, meter.MD_Grane,
                                              meter.MD_Constant, CheckDevice, ConnectionFlag
                                               , FloatxIb, FloatxU, FangXiang, xTime, ref CreepIb, ref CheckTime);
                arrCreepTimes[i] = (float)Math.Round(CheckTime, 2);
                if (CreepIb < creepI)
                    creepI = CreepIb;
            }

            float[] aa =new float[3];
            float[] arrCreepTimeClone = (float[])arrCreepTimes.Clone();
            Number.PopDesc(ref arrCreepTimeClone, false);
            if (IsDemo)
                return 1F;
            else
                return arrCreepTimeClone[0];
        }



        #endregion
    }
}
