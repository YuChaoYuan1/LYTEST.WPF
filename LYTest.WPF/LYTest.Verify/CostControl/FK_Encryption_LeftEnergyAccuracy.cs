using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 剩余电量递减准确度
    /// </summary>
    class FK_Encryption_LeftEnergyAccuracy: VerifyBase
    {
        /// <summary>
        /// 走字电流
        /// </summary>
        float Xib = 0f;
        /// <summary>
        /// 走字电量
        /// </summary>
        float ZZenergy = 1f;

        string[] energyStart;   //开始电量
        string[] energyEnd;      //结束电量
        string[] moneyStart;    //开始时剩余金额
        string[] moneyEnd;      //结束时剩余金额
        string[] arrayPrice;       //价格
        float[] arrayMoneyUsed; //初始金额
        float[] arrayEnergyMoney; //剩余金额
        bool[] arrayResult;      //总结论

        //误差值
        float[] arrayError;
        string[] arrayDisableReasion;
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("剩余电量递减准确度试验检定开始...", EnumLogType.流程信息);

            energyStart = new string[MeterNumber];
            energyEnd = new string[MeterNumber];
            moneyStart = new string[MeterNumber];
            moneyEnd = new string[MeterNumber];
            arrayPrice = new string[MeterNumber];
            arrayResult = new bool[MeterNumber];
            arrayMoneyUsed = new float[MeterNumber];
            arrayEnergyMoney = new float[MeterNumber];

            //add yjt
            arrayError = new float[MeterNumber];
            arrayDisableReasion = new string[MeterNumber];

            //add yjt 20220327 新增演示模式
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["当前电价(元)"][i] = "0.49";
                    ResultDictionary["开始剩余金额(元)"][i] = "100";
                    ResultDictionary["初始金额"][i] = "0";
                    ResultDictionary["开始总电量(度)"][i] = "0";
                    ResultDictionary["结束剩余金额(元)"][i] = "99.02";
                    ResultDictionary["剩余金额"][i] = "0";
                    ResultDictionary["结束总电量(度)"][i] = "2.08";
                    ResultDictionary["误差"][i] = (0.0008).ToString("0.0000");
                    ResultDictionary["不合格原因"][i] = "";
                    ResultDictionary["结论"][i] = "合格";                    
                }
                RefUIData("当前电价(元)");
                RefUIData("开始剩余金额(元)");
                RefUIData("初始金额");
                RefUIData("开始总电量(度)");
                RefUIData("结束剩余金额(元)");
                RefUIData("剩余金额");
                RefUIData("结束总电量(度)");
                RefUIData("误差");
                RefUIData("不合格原因");
                RefUIData("结论");
            }
            else
            {
                RefreshResult(false);
                #region 初始化
                base.Verify();
                if (Stop) return;

                ///升源 只升电压
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1");

                #endregion
                if (Stop) return;
                //获取所有表的表地址
                ReadMeterAddrAndNo();

                RefreshResult(false);

                #region //切换费率,以免检定时发生费率切换
                bool[] resultChangePeriod = InitialPeriod(out arrayPrice);

                for (int i = 0; i < MeterNumber; i++)
                {

                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (!resultChangePeriod[i])
                        {
                            arrayResult[i] = false;
                            arrayDisableReasion[i] = "切换表费率时段失败!!!";
                        }
                    }
                }
                #endregion

                #region 读取开始时数据
                if (Stop) return;
                MessageAdd("读取开始时的剩余金额...", EnumLogType.提示信息);
                moneyStart = MeterProtocolAdapter.Instance.ReadData("(当前)剩余金额");
                MessageAdd("读取开始时的总电量...", EnumLogType.提示信息);
                energyStart = MeterProtocolAdapter.Instance.ReadData(GetEnergyProtocal(PowerWay.正向有功));

                RefreshResult(false);
                #endregion

                #region 计算并等待:2度电
                float[] IbImax = OneMeterInfo.GetIb();
                //float currentImax = IbImax[1];

                /// 测量方式--单相-三相三线-三相四线
                WireMode wireMode = WireMode.三相四线;
                if (OneMeterInfo.MD_WiringMode == "单相")
                {
                    wireMode = WireMode.单相;
                }
                else if (OneMeterInfo.MD_WiringMode == "三相三线")
                {
                    wireMode = WireMode.三相三线;
                }
                float currentPower = base.CalculatePower(OneMeterInfo.MD_UB, Xib, wireMode, Cus_PowerYuanJian.H, "1.0", true);
                float totalTime = ((float)3600 * 1000 * ZZenergy) / currentPower;

                if (Stop) return;
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Cus_PowerYuanJian.H, PowerWay.正向有功, "1");
                DateTime timeEnd = DateTime.Now.AddSeconds(totalTime);
                while (DateTime.Now < timeEnd)
                {
                    if (Stop) return;

                    System.Threading.Thread.Sleep(1000);
                    int secondCount = (int)((timeEnd - DateTime.Now).TotalSeconds);
                    MessageAdd($"等待电表走{ZZenergy}度电,剩余时间:{secondCount}秒", EnumLogType.提示信息);
                }

                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1");
                #endregion

                #region 读取结束时数据
                if (Stop) return;

                MessageAdd("读取结束时的剩余金额...", EnumLogType.提示信息);
                moneyEnd = MeterProtocolAdapter.Instance.ReadData("(当前)剩余金额");

                MessageAdd("读取结束时的总电量...", EnumLogType.提示信息);
                energyEnd = MeterProtocolAdapter.Instance.ReadData(GetEnergyProtocal(PowerWay.正向有功));
                #endregion

                #region 计算误差,判断结论
                arrayMoneyUsed = new float[MeterNumber];
                arrayEnergyMoney = new float[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (!string.IsNullOrEmpty(moneyStart[i]) && !string.IsNullOrEmpty(moneyEnd[i]) &&
                            !string.IsNullOrEmpty(energyStart[i]) && !string.IsNullOrEmpty(energyEnd[i]) &&
                            !string.IsNullOrEmpty(arrayPrice[i]))
                        {
                            arrayMoneyUsed[i] = Convert.ToSingle(moneyStart[i]) - Convert.ToSingle(moneyEnd[i]);
                            arrayEnergyMoney[i] = (Convert.ToSingle(energyEnd[i].Split(',')[0]) - Convert.ToSingle(energyStart[i].Split(',')[0])) * Convert.ToSingle(arrayPrice[i].Split(',')[0]);
                            arrayError[i] = (arrayMoneyUsed[i] - arrayEnergyMoney[i]);// / arrayEnergyMoney[i];
                        }
                    }
                }
                RefreshResult(true);
                #endregion
                //刷新数据

                //add yjt 20220305 新增结束
                if (Stop) return;

                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                //Identity();
                //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                Identity(true);

                DateTime readTime = DateTime.Now;
                MessageAdd("开始恢复表时间......", EnumLogType.提示信息);
                MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            }
            
            //add yjt 20220305 新增日志提示
            MessageAdd("剩余电量递减准确度试验检定结束...", EnumLogType.流程信息);
        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //ResultNames = new string[] { "当前电价(元)", "开始剩余金额(元)", "开始总电量(度)", "结束剩余金额(元)", "结束总电量(度)", "误差", "结论" };
            //modify yjt 20220304 新增一个不合格原因
            string[] data = Test_Value.Split('|');
            if (data[0]=="" || Core.Function.Number.IsNumeric(data[0]))
            {
                data[0] = "Imax";
            }
            if (data[1] == "" || !Core.Function.Number.IsNumeric(data[1]))
            {
                data[1] = "1";
            }
            ZZenergy =float.Parse( data[1]);
             Xib =Core.Function. Number.GetCurrentByIb(data[0], OneMeterInfo.MD_UA, HGQ);
            ResultNames = new string[] { "当前电价(元)", "开始剩余金额(元)", "初始金额", "开始总电量(度)", "结束剩余金额(元)", "剩余金额", "结束总电量(度)", "误差", "不合格原因", "结论" };

            return true;
        }
        /// <summary>
        /// 更新结论
        /// </summary>
        private void RefreshResult(bool flagFinished)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前电价(元)"][i] = arrayPrice[i];
                ResultDictionary["开始剩余金额(元)"][i] = moneyStart[i];
                ResultDictionary["初始金额"][i] = arrayMoneyUsed[i].ToString("0.0000");
                ResultDictionary["开始总电量(度)"][i] = energyStart[i];
                ResultDictionary["结束剩余金额(元)"][i] = moneyEnd[i];
                ResultDictionary["剩余金额"][i] = arrayEnergyMoney[i].ToString("0.0000");
                ResultDictionary["结束总电量(度)"][i] = energyEnd[i];               
                ResultDictionary["误差"][i] = arrayError[i].ToString("0.0000");
                ResultDictionary["不合格原因"][i] = arrayDisableReasion[i];


                if (flagFinished)
                {
                    ResultDictionary["结论"][i] = Math.Abs(arrayError[i]) <= 0.01 ? "合格" :"不合格";
                    if (ResultDictionary["结论"][i]=="合格")
                    {
                        ResultDictionary["不合格原因"][i] = "";
                    }
                }
            }
            RefUIData("当前电价(元)");
            RefUIData("开始剩余金额(元)");
            RefUIData("初始金额");
            RefUIData("开始总电量(度)");
            RefUIData("结束剩余金额(元)");
            RefUIData("结束总电量(度)");
            RefUIData("剩余金额");
            RefUIData("结论");
            RefUIData("误差");

            //add yjt 20220304 新增一个不合格原因
            RefUIData("不合格原因");

            RefUIData("结论");
        }

        private bool[] InitialPeriod(out string[] arrayPrice)
        {
            bool[] result = new bool[MeterNumber];
            arrayPrice = new string[MeterNumber];

            if (Stop) return result;
            MessageAdd("开始读取电表第一套第一日费段表数据...", EnumLogType.提示信息);
            string[] readData = MeterProtocolAdapter.Instance.ReadData("第一套第1日时段数据");

            int firstIndex = FirstIndex;

            int indexTemp = 0;
            string s1 = "";
            while (indexTemp < (readData[firstIndex].Length / 6))
            {

                s1 = readData[firstIndex].Substring(6 * indexTemp, 6);
                if (s1.EndsWith("03"))
                {
                    break;
                }
                indexTemp++;
            }

            if (Stop) return result;

            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            //Identity();
            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            Identity(true);

            MessageAdd( "开始设置电表时间为第一个费率时段...", EnumLogType.提示信息);

            string strTemp = s1.Substring(0, 2) + ":" + s1.Substring(2, 2);
            result = MeterProtocolAdapter.Instance.WriteDateTime(DateTime.Parse(strTemp));
            WaitTime("电表时间切换", 15);

            if (Stop) return result;

           MessageAdd( "开始读取电表当前电价...", EnumLogType.提示信息);
            arrayPrice = MeterProtocolAdapter.Instance.ReadData("当前电价");

            return result;
        }

        private string GetEnergyProtocal(PowerWay way)
        {
            switch (way)
            {
                case PowerWay.正向有功:
                    return "(当前)正向有功总电能";
                case PowerWay.反向有功:
                    return "(当前)反向有功总电能";
                case PowerWay.正向无功:
                    return "(当前)组合无功1总电能";
                case PowerWay.反向无功:
                    return "(当前)组合无功2总电能";
                case PowerWay.第一象限无功:
                    return "(当前)第一象限无功总电能";
                case PowerWay.第二象限无功:
                    return "(当前)第二象限无功总电能";
                case PowerWay.第三象限无功:
                    return "(当前)第三象限无功总电能";
                case PowerWay.第四象限无功:
                    return "(当前)第四象限无功总电能";
                default:
                    return "(当前)组合有功总电能";

            }
        }
    }
}
