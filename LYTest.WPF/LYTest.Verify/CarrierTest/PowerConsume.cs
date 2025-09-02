using LYTest.Core;
using LYTest.Core.Function;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;
using LYTest.MeterProtocol.Enum;

namespace LYTest.Verify.CarrierTest
{
    class PowerConsume : VerifyBase
    {
        private readonly float U_P_Limit = 1.5F;
        private readonly float U_S_Limit = 6F;
        private readonly float I_S_Limit = 1F;
        private bool[] bBiaoweiBz = null;
        string Name = "";
        public override void Verify()
        {
            base.Verify();
            //重新发送一次隔离表位，只做选择配置做功耗的表位 20140924
            bool[] YJMeter = new bool[MeterNumber];

            if (IsDemo) //演示模式
            {
                bBiaoweiBz = new bool[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    bBiaoweiBz[i] = true;
                }
            }
            else
            {
                //增加选择表位进行读取操作处理
                bBiaoweiBz = new bool[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    bBiaoweiBz[i] = false;
                }

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

                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo m = MeterInfo[i];
                    YJMeter[i] = m.YaoJianYn & bBiaoweiBz[i];
                }
                ControlMeterRelay(YJMeter, 1);
                ControlMeterRelay(ReversalBool(YJMeter), 2);

                if (Stop) return;
                Thread.Sleep(2000);
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

            ControlMeterRelay(GetYaoJian(), 1);
            ControlMeterRelay(ReversalBool(GetYaoJian()), 2);
            RefUIData("结论");
            if (Stop) return;
            MessageAdd("检定完成", EnumLogType.提示信息);
            //EquipHelper.Instance.SetMeterOnOff(YJMeter);
        }
        protected override bool CheckPara()
        {
            Name = Test_Value.Split('|')[0];
            ResultNames = new string[] { "电压A线路有功(W)", "电压B线路有功(W)", "电压C线路有功(W)", "电压A线路视在(VA)", "电压B线路视在(VA)", "电压C线路视在(VA)",
                "电流A线路视在(VA)", "电流B线路视在(VA)", "电流C线路视在(VA)","结论" };
            return true;
        }

        #region  通讯配置
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
                        if (Stop) break;

                        TestMeterInfo curMeter = MeterInfo[iBw];
                        if (!curMeter.YaoJianYn) continue;

                        MessageAdd("添加主节点地址", EnumLogType.提示信息);
                        MeterProtocolAdapter.Instance.AddMainNode(iBw);

                        MessageAdd($"正在无线试验第{iBw + 1}表位...", EnumLogType.提示信息);
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
        #endregion

        /// <summary>
        /// 功耗板
        /// </summary>      
        #region
        private void Verify_PowerD()
        {
            int[] channelU = new int[3] { 1, 3, 5 };    //功耗电压通道
            //_ = new int[3] { 2, 4, 6 };    //功耗电流通道
            if (OneMeterInfo.MD_WiringMode == "单相")
            {
                channelU = new int[1] { 1 };
                //int[] channelI = new int[1] { 2 };
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
                float xIb = Number.GetCurrentByIb("1.0Ib", OneMeterInfo.MD_UA, HGQ);
                PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                Thread.Sleep(3000);
                MessageAdd("正在进行电压功耗测试...", EnumLogType.提示信息);
                Thread.Sleep(3000);

                //读取电压功耗数据

                for (int i = 1; i <= MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i - 1];


                    if (!meter.YaoJianYn || !bBiaoweiBz[i - 1]) continue;

                    //MeterPower rst = meter.MeterPowers.GetValue(strKey);
                    //rst.PrjID = strKey;
                    //rst.Name = "功耗测试" + CurPlan.PrjID;
                    //rst.Value = ConstHelper.合格;

                    MessageAdd("正在进行" + i + "表位电压功耗测试...", EnumLogType.提示信息);
                    //MeterProtocolAdapter.Instance.ReadStdInfo();
                    //读取电压功耗
                    for (int c = 0; c < channelU.Length; c++)
                    {
                        Thread.Sleep(800);
                        //电压有效值，电流有效值，基波有功功率，基波无功功率
                        if (!Read_GH_Dissipation(i, out float[] pd))
                        {
                            if (!Read_GH_Dissipation(i, out pd))
                            {
                                MessageAdd("读取" + i + "表位电压功耗失败！", EnumLogType.提示信息);
                            }
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
                                ResultDictionary["电压A线路视在(VA)"][i - 1] = pd[1].ToString("F4");
                                ResultDictionary["电压A线路有功(W)"][i - 1] = pd[4].ToString("F4");
                                ResultDictionary["电流A线路视在(VA)"][i - 1] = pd[7].ToString("F4");
                                if (ResultDictionary["结论"][i - 1] == ConstHelper.不合格) continue;
                                if (pd[1] > U_S_Limit || pd[4] > U_P_Limit || pd[7] > I_S_Limit)  // B
                                {
                                    ResultDictionary["结论"][i - 1] = ConstHelper.不合格;
                                }
                                else
                                    ResultDictionary["结论"][i - 1] = ConstHelper.合格;

                                break;
                            case 2: //C相
                                ResultDictionary["电压A线路视在(VA)"][i - 1] = pd[2].ToString("F4");
                                ResultDictionary["电压A线路有功(W)"][i - 1] = pd[5].ToString("F4");
                                ResultDictionary["电流A线路视在(VA)"][i - 1] = pd[8].ToString("F4");
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
        }
        #endregion

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
