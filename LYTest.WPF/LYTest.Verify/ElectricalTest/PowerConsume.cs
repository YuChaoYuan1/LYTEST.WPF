using LYTest.Core;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.MeterProtocol.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;


//add yjt 20220805 新增功耗实验检定类

namespace LYTest.Verify.ElectricalTest
{
    /// <summary>
    /// 功耗试验检定器
    /// </summary>
    class PowerConsume : VerifyBase
    {
        /// <summary>
        /// 项目名
        /// </summary>
        public string Name;
        private float U_P_Limit = 1.5F;
        private float U_S_Limit = 6F;
        private float I_S_Limit = 0.2F;
        private bool[] bBiaoweiBz = null;

        protected override bool CheckPara()
        {
            Name = Test_Value.Split('|')[0];

            //add yjt 20220825 新增阀值
            string[] paras = Test_Value.Split('|');
            if (paras.Length > 1)
            {
                U_P_Limit = float.Parse(paras[1]); //电压线路有功(W)
                U_S_Limit = float.Parse(paras[2]); //电压线路视在(VA)
                I_S_Limit = float.Parse(paras[3]); //电流线路视在(VA)
            }

            ResultNames = new string[] { "电压A线路有功(W)", "电压B线路有功(W)", "电压C线路有功(W)", "电压A线路视在(VA)", "电压B线路视在(VA)", "电压C线路视在(VA)", "电流A线路视在(VA)", "电流B线路视在(VA)", "电流C线路视在(VA)", "结论" };
            return true;
        }


        /// <summary>
        /// 开始功耗检定
        /// </summary>
        public override void Verify()
        {
            //新增日志提示
            MessageAdd("功耗验检定开始...", EnumLogType.提示与流程信息);

            base.Verify();

            //重新发送一次隔离表位，只做选择配置做功耗的表位 20140924
            bool[] YJMeter = new bool[MeterNumber];

            bBiaoweiBz = new bool[MeterNumber];

            if (IsDemo) //演示模式
            {

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    bBiaoweiBz[i] = true;
                }
            }
            else
            {
                //增加选择表位进行读取操作处理
                //for (int i = 0; i < MeterNumber; i++)
                //{
                //    bBiaoweiBz[i] = false;
                //}
                bBiaoweiBz.Fill(false);

                if (VerifyConfig.SelectBiaoWei == "0" || string.IsNullOrEmpty(VerifyConfig.SelectBiaoWei))
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        bBiaoweiBz[i] = true;
                    }
                }
                else
                {
                    string[] bws = VerifyConfig.SelectBiaoWei.Split(','); //选中表位
                    foreach (string s in bws)
                    {
                        int bw = Convert.ToInt32(s) - 1;
                        bBiaoweiBz[bw] = true;
                    }
                }

                if (Stop) return;
                for (int i = 0; i < MeterNumber; i++)
                {
                    //TestMeterInfo m = meterInfo[i];
                    YJMeter[i] = MeterInfo[i].YaoJianYn & bBiaoweiBz[i];
                }
                if (Stop) return;
                ControlMeterRelay(YJMeter, 2);
                ControlMeterRelay(ReversalBool(YJMeter), 1);

                if (Stop) return;
                Thread.Sleep(2000);

                if (Name.IndexOf("正常") == -1)
                {
                    PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
                }
                if (Name.IndexOf("485通讯状态") != -1)
                    SwitchChannel(Cus_ChannelType.通讯485);
                else if (Name.IndexOf("载波通讯状态") != -1)
                    SwitchChannel(Cus_ChannelType.通讯载波);
                else if (Name.IndexOf("无线通讯状态") != -1)
                    SwitchChannel(Cus_ChannelType.通讯无线);

                ThreadPool.QueueUserWorkItem(new WaitCallback(StartCommunication));
            }


            if (Stop) return;
            Verify_PowerD();

            if (Stop) return;
            ControlMeterRelay(GetYaoJian(), 2);
            ControlMeterRelay(ReversalBool(GetYaoJian()), 1);
            RefUIData("结论");
            if (Stop) return;
            if (Name.IndexOf("载波通讯状态") != -1)
                SwitchChannel(Cus_ChannelType.通讯485);
            MessageAdd("检定完成", EnumLogType.提示与流程信息);

            if (Stop) return;
        }

        /// <summary>
        /// 通讯配置
        /// </summary>
        public void StartCommunication(object obj)
        {
            while (true)
            {
                if (Name.IndexOf("载波通讯状态") != -1)
                {
                    for (int iBw = 0; iBw < MeterNumber; iBw++)
                    {
                        LYTest.MeterProtocol.App.Carrier_Cur_BwIndex = iBw;
                        if (Stop)
                        {
                            break;
                        }
                        //【获取指定表位电表信息】
                        TestMeterInfo curMeter = MeterInfo[iBw];
                        //【判断是否要检】
                        if (!curMeter.YaoJianYn) continue;

                        MessageAdd("添加主节点地址", EnumLogType.提示信息);
                        MeterProtocolAdapter.Instance.AddMainNode(iBw);

                    }

                    if (Stop) return;
                }
                else if (Name.IndexOf("无线通讯状态") != -1)
                {
                    for (int iBw = 0; iBw < MeterNumber; iBw++)
                    {
                        LYTest.MeterProtocol.App.Carrier_Cur_BwIndex = iBw;
                        if (Stop)
                        {
                            break;
                        }

                        TestMeterInfo meter = MeterInfo[iBw];
                        if (!meter.YaoJianYn) continue;

                        //App.Message.Show(false, "添加主节点地址");
                        MessageAdd("添加主节点地址", EnumLogType.提示信息);
                        //EquipHelper.Instance.AddMainNode(iBw);
                        MeterProtocolAdapter.Instance.AddMainNode(iBw);

                        MessageAdd("正在无线试验第" + (iBw + 1) + "表位...", EnumLogType.提示信息);
                    }

                    if (Stop) return;
                }
                else if (Name.IndexOf("485通讯状态") != -1)
                {
                    MeterProtocolAdapter.Instance.ReadAddress();
                    if (Stop) return;
                }

                Thread.Sleep(3000);
            }
        }

        /// <summary>
        /// 功耗板
        /// </summary>      
        private void Verify_PowerD()
        {
            int[] channelU = new int[3] { 1, 3, 5 };    //功耗电压通道
                                                        //if (OneMeterInfo.MD_WiringMode == "单相台")
            if (OneMeterInfo.MD_WiringMode == "单相")
            {
                channelU = new int[1] { 1 };
            }


            #region ---------------- 测试电压和电流功耗 ------------------
            if (IsDemo)
            {
                for (int i = 1; i <= MeterNumber; i++)
                {
                    ResultDictionary["结论"][i - 1] = ConstHelper.合格;

                    if (OneMeterInfo.MD_WiringMode == "单相")
                    {
                        //"电压A线路有功(W)"
                        ResultDictionary["电压A线路有功(W)"][i - 1] = "0.0111";
                        ResultDictionary["电压A线路视在(VA)"][i - 1] = "0.0222";
                        ResultDictionary["电流A线路视在(VA)"][i - 1] = "0.0333";
                        ResultDictionary["电压B线路有功(W)"][i - 1] = "0";
                        ResultDictionary["电压B线路视在(VA)"][i - 1] = "0";
                        ResultDictionary["电流B线路视在(VA)"][i - 1] = "0";
                        ResultDictionary["电压C线路有功(W)"][i - 1] = "0";
                        ResultDictionary["电压C线路视在(VA)"][i - 1] = "0";
                        ResultDictionary["电流C线路视在(VA)"][i - 1] = "0";
                    }
                    if (OneMeterInfo.MD_WiringMode == "三相四线")
                    {
                        ResultDictionary["电压A线路有功(W)"][i - 1] = "0.0111";
                        ResultDictionary["电压A线路视在(VA)"][i - 1] = "0.0222";
                        ResultDictionary["电流A线路视在(VA)"][i - 1] = "0.0333";
                        ResultDictionary["电压B线路有功(W)"][i - 1] = "0.0444";
                        ResultDictionary["电压B线路视在(VA)"][i - 1] = "0.0555";
                        ResultDictionary["电流B线路视在(VA)"][i - 1] = "0.0666";
                        ResultDictionary["电压C线路有功(W)"][i - 1] = "0.0777";
                        ResultDictionary["电压C线路视在(VA)"][i - 1] = "0.0888";
                        ResultDictionary["电流C线路视在(VA)"][i - 1] = "0.0999";
                    }
                }
            }
            else
            {
                MessageAdd("正在设置电压功耗测试参数", EnumLogType.提示信息);
                //EquipHelper.Instance.PowerOn(App.CUS.Meters.U, PowerWay.正向有功);

                if (Stop) return;

                string xIbstr = "1.0Ib";
                if (OneMeterInfo.MD_JJGC == "IR46")
                {
                    xIbstr = "10Itr";
                }

                float xIb = Number.GetCurrentByIb(xIbstr, OneMeterInfo.MD_UA, HGQ);
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                if (Stop) return;
                MessageAdd("正在进行电压功耗测试...", EnumLogType.提示信息);
                Thread.Sleep(5000);

                //string strKey = CurPlan.PrjID;
                //读取电压功耗数据

                for (int i = 1; i <= MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i - 1];
                    if (!meter.YaoJianYn || !bBiaoweiBz[i - 1]) continue;


                    MessageAdd($"正在进行{i}表位电压功耗测试...", EnumLogType.提示信息);
                    //读取电压功耗
                    for (int c = 0; c < channelU.Length; c++)
                    {
                        Thread.Sleep(800);
                        if (Stop) return;
                        float[] pd = new float[20]; //电压有效值，电流有效值，基波有功功率，基波无功功率

                        for (int j = 0; j < 10; j++)
                        {
                            if (Read_GH_Dissipation(i, out pd))
                            {
                                break;
                            }
                            Thread.Sleep(500);
                        }

                        switch (c)
                        {
                            case 0: //A相
                                ResultDictionary["电压A线路视在(VA)"][i - 1] = pd[0].ToString("F4");
                                ResultDictionary["电压A线路有功(W)"][i - 1] = pd[3].ToString("F4");
                                ResultDictionary["电流A线路视在(VA)"][i - 1] = pd[6].ToString("F4");
                                if (ResultDictionary["结论"][i - 1] == ConstHelper.不合格) continue;
                                if (pd[0] > U_S_Limit || pd[3] > U_P_Limit || pd[6] > I_S_Limit)  // A
                                {
                                    ResultDictionary["结论"][i - 1] = ConstHelper.不合格;
                                }
                                else
                                    ResultDictionary["结论"][i - 1] = ConstHelper.合格;

                                break;
                            case 1: //B相
                                ResultDictionary["电压B线路视在(VA)"][i - 1] = pd[1].ToString("F4");
                                ResultDictionary["电压B线路有功(W)"][i - 1] = pd[4].ToString("F4");
                                ResultDictionary["电流B线路视在(VA)"][i - 1] = pd[7].ToString("F4");
                                if (ResultDictionary["结论"][i - 1] == ConstHelper.不合格) continue;
                                if (pd[1] > U_S_Limit || pd[4] > U_P_Limit || pd[7] > I_S_Limit)  // B
                                {
                                    ResultDictionary["结论"][i - 1] = ConstHelper.不合格;
                                }
                                else
                                    ResultDictionary["结论"][i - 1] = ConstHelper.合格;


                                break;
                            case 2: //C相
                                ResultDictionary["电压C线路视在(VA)"][i - 1] = pd[2].ToString("F4");
                                ResultDictionary["电压C线路有功(W)"][i - 1] = pd[5].ToString("F4");
                                ResultDictionary["电流C线路视在(VA)"][i - 1] = pd[8].ToString("F4");
                                if (ResultDictionary["结论"][i - 1] == ConstHelper.不合格) continue;
                                if (pd[2] > U_S_Limit || pd[5] > U_P_Limit || pd[8] > I_S_Limit)  // C
                                {
                                    ResultDictionary["结论"][i - 1] = ConstHelper.不合格;
                                }
                                else
                                    ResultDictionary["结论"][i - 1] = ConstHelper.合格;

                                break;
                        }

                        if (c == channelU.Length - 1)
                            CheckOver = true;
                    }

                }
            }

            RefUIData("电压A线路有功(W)");
            RefUIData("电压A线路视在(VA)");
            RefUIData("电流A线路视在(VA)");
            RefUIData("电压B线路有功(W)");
            RefUIData("电压B线路视在(VA)");
            RefUIData("电流B线路视在(VA)");
            RefUIData("电压C线路有功(W)");
            RefUIData("电压C线路视在(VA)");
            RefUIData("电流C线路视在(VA)");

            #endregion

            #region  --------------  测试电流功耗  已经和电压功耗合并------------------
            //EquipHelper.Instance.PowerOn(0, App.CUS.Meters.Ib, PowerYuanJian.H, PowerWay.正向有功, "1.0", false);

            //App.Message.Show(false, "等待源稳定5秒...");
            //Thread.Sleep(5000);
            //EquipHelper.Instance.ReadStdInfo();
            //CheckOver = false;
            //App.Message.Show(false, "正在进行电流功耗测试...");
            //Thread.Sleep(5000);
            //while (true)
            //{
            //    if (App.CUS.CheckState == Cus_CheckStaute.停止检定) break;
            //    if (Stop || CheckOver) break;

            //    Thread.Sleep(1000);
            //    for (int i = 1; i <= BwCount; i++)
            //    {
            //        MeterInfo meter = App.CUS.Meters[i - 1];
            //        if (!meter.IsTest || !bBiaoweiBz[i - 1]) continue;

            //        if (meter.MeterPowers == null)
            //            meter.MeterPowers = new Dictionary<string, MeterPower>();

            //        MeterPower rst = meter.MeterPowers.GetValue(strKey);
            //        rst.PrjID = strKey;
            //        rst.Name = "功耗测试" + CurPlan.PrjID;

            //        App.Message.Show(false, "正在进行" + i + "表位电流功耗测试...");
            //        EquipHelper.Instance.ReadStdInfo();

            //        //读取电流功耗
            //        for (int j = 0; j < channelI.Length; j++)
            //        {
            //            Thread.Sleep(800);
            //            float[] pd;//电压有效值，电流有效值，基波有功功率，基波无功功率
            //            //if (!EquipHelper.Instance.ReadPowerDissipation(i, (byte)channelI[j], out pd))
            //            //    App.Message.Show("读取" + i + "表位电流功耗失败！");

            //            if (!EquipHelper.Instance.Read_GH_Dissipation(i, out pd))
            //            {
            //                App.Message.Show("读取" + i + "表位电压功耗失败！");
            //            }

            //            if (j == 0)  //A相
            //                rst.IaPowerS = (pd[6]).ToString("F4");
            //            else if (j == 1)//B相
            //                rst.IbPowerS = (pd[7]).ToString("F4");
            //            else if (j == 2)//C相
            //                rst.IcPowerS = (pd[8] ).ToString("F4");

            //            if ((pd[0] ) > I_S_Limit)
            //                rst.Value = Variable.不合格;
            //            else
            //                rst.Value = Variable.合格;

            //            if (j == channelI.Length - 1) CheckOver = true;
            //        }

            //        App.Message.Show();
            //    }
            //}
            #endregion;
        }


        /// <summary>
        /// bool数组反转
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool[] ReversalBool(bool[] Bt)
        {
            bool[] t = new bool[Bt.Length];
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = !Bt[i];
            }
            return t;
        }
    }
}
