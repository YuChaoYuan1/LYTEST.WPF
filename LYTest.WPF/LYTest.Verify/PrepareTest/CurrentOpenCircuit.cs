using LYTest.Core;
using LYTest.Core.Function;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Text;
using System.Windows;

namespace LYTest.Verify.PrepareTest
{
    class CurrentOpenCircuit : VerifyBase
    {
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "检查信息", "结论" };
            return base.CheckPara();
        }
        public override void Verify()
        {
            MessageAdd(Test_Name, EnumLogType.提示与流程信息);
            base.Verify();
            int checkedCount = 0;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    ++checkedCount;
                    ResultDictionary["检查信息"][i] = "正常";
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }
            }
            if (checkedCount < 1)
            {
                MessageAdd("没有勾选的表位", EnumLogType.提示与流程信息);
                return;
            }
            float voltage = OneMeterInfo.MD_UB;
            #region 检查电压
            MessageAdd("检查电压", EnumLogType.提示与流程信息);
            {
                float limitU = OneMeterInfo.MD_UB * 0.6F;
                PowerOn(voltage, voltage, voltage, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                string errUabc = CheckVoltage(limitU);

                if (!string.IsNullOrWhiteSpace(errUabc))
                {
                    //电表、或台体问题
                    MessageBox.Show($"电压输出失败：{errUabc}{Environment.NewLine}", "提示", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;

                }
            }
            #endregion 检查电压

            #region 检查电流
            MessageAdd("检查电流", EnumLogType.提示与流程信息);
            {
                float limitI = 1 * 0.8F;
                //直接输出
                PowerOn(voltage, voltage, voltage, 1, 1, 1, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                string errIabc = CheckCurrent(limitI);

                if (!string.IsNullOrWhiteSpace(errIabc))
                {
                    try
                    {
                        //隔离输出
                        bool[] testPosi = new bool[MeterNumber];
                        testPosi.Fill(true);
                        ControlMeterRelay(testPosi, 1);

                        PowerOn(voltage, voltage, voltage, 1, 1, 1, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");

                        errIabc = CheckCurrent(limitI);
                        if (!string.IsNullOrWhiteSpace(errIabc))
                        {
                            MessageBox.Show($"电流输出失败：{errIabc}{Environment.NewLine}", "提示", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                            return;
                        }

                        MessageAdd("检查要检的表位", EnumLogType.提示与流程信息);
                        StringBuilder errMeter = new StringBuilder();

                        string[] meterIa = MeterProtocolAdapter.Instance.ReadData("A相电流");
                        string[] meterIb = MeterProtocolAdapter.Instance.ReadData("B相电流");
                        string[] meterIc = MeterProtocolAdapter.Instance.ReadData("C相电流");
                        int newLine = 0;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (MeterInfo[i].YaoJianYn)
                            {
                                string Iabc = "";
                                if (string.IsNullOrWhiteSpace(meterIa[i])
                                    || !float.TryParse(meterIa[i], out float tempI)
                                    || tempI <= 0)
                                {
                                    Iabc = "Ia,";
                                }
                                if (Clfs == Core.Enum.WireMode.三相四线)
                                {
                                    if (string.IsNullOrWhiteSpace(meterIb[i])
                                    || !float.TryParse(meterIb[i], out float tempIb)
                                    || tempIb <= 0)
                                    {
                                        Iabc += "Ib,";
                                    }
                                }
                                if (Clfs == Core.Enum.WireMode.三相四线 || Clfs == Core.Enum.WireMode.三相三线)
                                {
                                    if (string.IsNullOrWhiteSpace(meterIc[i])
                                    || !float.TryParse(meterIc[i], out float tempIc)
                                    || tempIc <= 0)
                                    {
                                        Iabc += "Ic,";
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(Iabc))
                                {
                                    ++newLine;

                                    if (Clfs == Core.Enum.WireMode.三相四线 || Clfs == Core.Enum.WireMode.三相三线)
                                        errMeter.Append("\t").Append(i + 1).Append(":").Append(Iabc).Append(" ");
                                    else
                                        errMeter.Append("\t").Append(i + 1).Append(", ");

                                    ResultDictionary["检查信息"][i] = "×";
                                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                                }

                                if (newLine > 0 && newLine % 15 == 0) errMeter.AppendLine();
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(errMeter.ToString()))
                        {
                            MessageBox.Show($"电表电流开路位置：{Environment.NewLine}{errMeter}{Environment.NewLine}", "提示", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                            return;
                        }
                    }
                    finally
                    {
                        PowerOff();
                        EquipmentData.DeviceManager.ControlYaoJianPositions();
                    }
                }

            }
            #endregion 检查电流

            RefUIData("检查信息");
            RefUIData("结论");
        }

        private string CheckCurrent(float limitI)
        {
            string errIabc = "";
            if (EquipmentData.StdInfo.Ia < limitI)
            {
                errIabc = "Ia,";
            }
            if (Clfs == Core.Enum.WireMode.三相四线 && EquipmentData.StdInfo.Ib < limitI)
            {
                errIabc += "Ib,";
            }
            if ((Clfs == Core.Enum.WireMode.三相四线 || Clfs == Core.Enum.WireMode.三相三线) && EquipmentData.StdInfo.Ic < limitI)
            {
                errIabc += "Ic,";
            }

            return errIabc;
        }

        private new string CheckVoltage(float limitU)
        {
            string errUabc = "";
            if (EquipmentData.StdInfo.Ua < limitU)
            {
                errUabc = "Ua,";
            }
            if (Clfs == Core.Enum.WireMode.三相四线 && EquipmentData.StdInfo.Ub < limitU)
            {
                errUabc += "Ub,";
            }
            if ((Clfs == Core.Enum.WireMode.三相四线 || Clfs == Core.Enum.WireMode.三相三线) && EquipmentData.StdInfo.Uc < limitU)
            {
                errUabc += "Uc,";
            }

            return errUabc;
        }
    }
}
