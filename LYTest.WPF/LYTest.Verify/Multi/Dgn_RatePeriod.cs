using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 费率时段示值误差
    /// add lsj 20220720
    /// </summary>
    class Dgn_RatePeriod : VerifyBase
    {
        private PowerWay PowerWay { get; set; }
        public override void Verify()
        {
            Stop = false;
            int maxTestTime = 60;
            base.Verify();
            //升电压
            PowerOn();
            //读地址和表号
            ReadMeterAddrAndNo();
            string[] arrSD = new string[0];
            string[] arrTime = new string[0];
            string[] param = Test_Value.Split('|');
            if (param.Length > 0)
                arrSD = param[0].Replace(" ", "").Replace("，", ",").Split(','); //走的费率时段
            if (param.Length >= 3)
                arrTime = param[2].Split(',');  //检定时间

            if (arrSD.Length < 1) return;
            bool[][] Allresult = new bool[arrSD.Length][];
            PowerWay = GetCurPowerFangXiang();  //当前检定功率方向

            //有无编程键
            if (OneMeterInfo.DgnProtocol.HaveProgrammingkey)
            {
                MessageBox.Show("请打开电能表编程开关后点击[确定]", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            //对每一个时段进行试验
            for (int k = 0; k < arrSD.Length; k++)
            {
                string curSD = arrSD[k];
                if (arrTime.Length > k)
                    maxTestTime = Convert.ToInt32(Convert.ToSingle(arrTime[k]) * 60);
                Allresult[k] = new bool[MeterNumber];
                string[] para = curSD.Split('(');
                if (para.Length != 2)
                    continue;
                MessageAdd("正在做" + curSD + "费率时段检查", EnumLogType.提示信息);
                //设置时间
                Identity();
                //bool[] result = MeterProtocolAdapter.Instance.WriteDateTime(DateTime.Parse(para[0]));
                //费率
                Cus_FeiLv curTri = (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), para[1].Replace(")", ""));

                //注意电量读取组件的位置，处理事件冲突
                RatePeriodData[] tagRatePeriodData = new RatePeriodData[MeterNumber];
                MessageAdd($"正在做{curSD}{"费率时段检查"}", EnumLogType.提示信息);
                if (Stop) break;

                Dictionary<int, float[]> dicEnergy = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)PowerWay);

                for (int i = 0; i < MeterNumber; i++)
                {
                    //把方案费率编号转换到被检表费率顺序号
                    //int feilvOrder = SwitchFeiLvNameToOrder(para[1].Replace(")", ""));
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (dicEnergy.ContainsKey(i) == false) continue;

                    int intCurTri = Convert.ToInt32(curTri);
                    if (dicEnergy[i] != null)
                    {
                        if (dicEnergy[i].Length > intCurTri)
                            tagRatePeriodData[i].StartPower = dicEnergy[i][intCurTri];
                    }

                    if (dicEnergy[i] != null)
                    {
                        if (dicEnergy[i].Length > 0)
                            tagRatePeriodData[i].StartSumPower = dicEnergy[i][0];
                    }

                    tagRatePeriodData[i].FL = curTri.ToString();
                    ResultDictionary[$"试验前分费率电量"][i] = tagRatePeriodData[i].StartPower.ToString();
                    ResultDictionary["试验前总电量"][i] = tagRatePeriodData[i].StartSumPower.ToString();
                    //读取电量完毕，刷新一次数据
                }

                RefUIData($"试验前分费率电量");
                RefUIData($"试验前总电量");

                float xib = OneMeterInfo.GetIb()[0];

                if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1") == false)
                {
                    MessageAdd("控制源输出失败", EnumLogType.提示信息);
                    return;
                }
                Thread.Sleep(300);
                DateTime startTime = DateTime.Now;

                CheckOver = false;
                while (true)
                {
                    Thread.Sleep(1000);
                    if (Stop) return;
                    float _PastTime = (float)DateTimes.DateDiff(startTime);

                    if (_PastTime >= maxTestTime) CheckOver = true;

                    MessageAdd($"设置费率{ curSD}运行：{maxTestTime / 60}分，已经走字：{ Math.Round(_PastTime / 60F, 2)}分", EnumLogType.提示信息);
                    if (CheckOver) break;
                }
                if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1") == false)
                {
                    MessageAdd("控制源输出失败", EnumLogType.提示信息);
                    //return;
                }
                Thread.Sleep(5000);

                dicEnergy = MeterProtocolAdapter.Instance.ReadEnergy((byte)(int)PowerWay);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (dicEnergy.ContainsKey(i) == false) continue;

                    //总检定结果
                    int intcurTri = Convert.ToInt32(curTri);
                    if (dicEnergy[i] != null)
                    {
                        if (dicEnergy[i].Length > intcurTri)
                            tagRatePeriodData[i].EndPower = dicEnergy[i][intcurTri];
                    }

                    if (dicEnergy[i] != null)
                    {
                        if (dicEnergy[i].Length > 1)
                            tagRatePeriodData[i].EndSumPower = dicEnergy[i][0];
                    }
                    tagRatePeriodData[i].FL = curTri.ToString();
                    float fErr = Convert.ToSingle(tagRatePeriodData[i].Error());

                    ResultDictionary[$"试验后分费率电量"][i] = tagRatePeriodData[i].StartPower.ToString();
                    ResultDictionary[$"试验后总电量"][i] = tagRatePeriodData[i].StartSumPower.ToString();
                    ResultDictionary[$"差值"][i] = fErr.ToString();

                    if (Math.Abs(fErr) > 0.02)
                        Allresult[k][i] = false;
                    else
                        Allresult[k][i] = true;
                    //读取电量完毕，刷新一次数据

                }
                RefUIData($"试验后分费率电量");
                RefUIData("试验后总电量");
                RefUIData($"差值");


            }
            #region 总结论
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                ResultDictionary["结论"][i] = "合格";
                for (int j = 0; j < arrSD.Length; j++)
                {
                    if (arrSD[j].Contains("(") == false) continue;
                    if (Allresult[j][i] == false)
                    {
                        ResultDictionary["结论"][i] = "不合格";
                        break;
                    }
                }
            }
            RefUIData("结论");
            #endregion
            //恢复时间
            MessageAdd("正在恢复电能表时间...", EnumLogType.提示信息);
            Identity();
            MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
        }
        private PowerWay GetCurPowerFangXiang()
        {
            PowerWay tmp = PowerWay.正向有功;
            string[] value = Test_Value.Split('|');
            if (value[1] == "反向有功")
            {
                tmp = PowerWay.反向有功;
            }
            else if (value[1] == "正向无功")
            {
                tmp = PowerWay.正向无功;
            }
            else if (value[1] == "反向无功")
            {
                tmp = PowerWay.反向无功;
            }
            return tmp;
        }
        /// <summary>
        /// 检定参数校验
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            if (Test_Value.Length <= 0)
                MessageAdd("检定方案中没有设置被检表时段，请到检定方案中设置", EnumLogType.提示信息);
            //string[] param = Test_Value.Split('|')[0].Replace(" ", "").Replace("，", ",").Split(',');
            List<string> list = new List<string>
            {
                "试验前总电量",
                "试验前分费率电量",
                "试验后总电量",
                "试验后分费率电量",
                "差值",
                "结论"
            };
            ResultNames = list.ToArray();

            return true;
        }

        /// <summary>
        /// 费率电能示值误差结构体
        /// </summary>
        struct RatePeriodData
        {
            public string FL;           //费率
            public float StartPower;       //分费率起始电量
            public float EndPower;         //分费率结束电量
            public float StartSumPower;          //总起始电量
            public float EndSumPower;            //总结束
            public override string ToString()
            {
                return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", StartPower, EndPower, StartSumPower, EndSumPower, Error(), FL);
            }

            /// <summary>
            /// 费率电量示值误差
            /// </summary>
            /// <returns></returns>
            public string Error()
            {
                float fError;

                float errPeriod = EndPower - StartPower;
                float errSum = EndSumPower - StartSumPower;

                if (errSum == 0.0f)
                    fError = 1.000f;
                else
                    fError = errPeriod - errSum;

                return fError.ToString("0.000");
            }
        }
    }
}
