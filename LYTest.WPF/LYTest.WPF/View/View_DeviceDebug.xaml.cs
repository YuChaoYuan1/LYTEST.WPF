using LYTest.ViewModel;
using LYTest.ViewModel.Debug;
using LYTest.ViewModel.Device;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_DeviceDebug.xaml 的交互逻辑
    /// </summary>
    public partial class View_DeviceDebug
    {
        private PowerViewModel ViewModel
        {
            get { return Resources["PowerViewModel"] as PowerViewModel; }
        }

        private ErrorControlViewModel ViewModel2
        {
            get { return Resources["ErrorControlViewModel"] as ErrorControlViewModel; }
        }

        //private MeterControlViewModel MeterViewModel
        //{
        //    get { return Resources["MeterControlViewModel"] as MeterControlViewModel; }
        //}

        private Debug_ProtViewModel ProtViewModel
        {
            get { return Resources["Debug_ProtViewModel"] as Debug_ProtViewModel; }
        }


        //private Debug_HarmonicViewModel HarmonicViewModel
        //{
        //    get { return Resources["Debug_HarmonicViewModel"] as Debug_HarmonicViewModel; }
        //}
        //private Debug_TimeZonePeriodViewModel Debug_TimeZonePeriodViewModel
        //{
        //    get { return Resources["Debug_TimeZonePeriodViewModel"] as Debug_TimeZonePeriodViewModel; }
        //}

        //public Dictionary<string, MeterStartControlViewModel> meterStartS = new Dictionary<string, MeterStartControlViewModel>();

        public View_DeviceDebug()
        {
            InitializeComponent();
            Name = "设备调试";

            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                Controls.MeterStartControl meterStart = new Controls.MeterStartControl();
                MeterStartControlViewModel meter = new MeterStartControlViewModel
                {
                    MeterNo = (i + 1).ToString().PadLeft(2, '0')
                };
                meterStart.DataContext = meter;
                if (!ViewModel2.meterStartS.ContainsKey((i + 1).ToString()))
                {
                    ViewModel2.meterStartS.Add((i + 1).ToString(), meter);
                }
                //if (i < EquipmentData.Equipment.MeterCount / 2)
                {
                    panel1.Children.Add(meterStart);
                }
                //else
                //{
                //    panel2.Children.Add(meterStart);
                //}
            }
            if (double.TryParse(EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_UB")?.ToString(), out double ub))
            {
                ViewModel.Ua = ub;
                ViewModel.Ub = ub;
                ViewModel.Uc = ub;
            }
            else
            {
                ViewModel.Ua = 0;
                ViewModel.Ub = 0;
                ViewModel.Uc = 0;
            }
            ViewModel.Ia = 0;
            ViewModel.Ib = 0;
            ViewModel.Ic = 0;
            EquipmentData.MeterGroupInfo.FirstMeter.PropertyChanged -= FirstMeter_PropertyChanged;
            EquipmentData.MeterGroupInfo.FirstMeter.PropertyChanged += FirstMeter_PropertyChanged;
        }

        private void FirstMeter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MD_UB")
            {
                if (double.TryParse(EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_UB")?.ToString(), out double ub))
                {
                    ViewModel.Ua = ub;
                    ViewModel.Ub = ub;
                    ViewModel.Uc = ub;
                }
                else
                {
                    ViewModel.Ua = 0;
                    ViewModel.Ub = 0;
                    ViewModel.Uc = 0;
                }
                ViewModel.Ia = 0;
                ViewModel.Ib = 0;
                ViewModel.Ic = 0;
            }
        }

        public override void Dispose()
        {
            isReadMeterStart = false;
            //isReadHarmonicData = false;

        }
        bool isReadMeterStart = false;
        //bool isReadHarmonicData = true;

        /// <summary>
        /// 读取表位状态
        /// </summary>
        public void ReadMeterStart(int start, int end)
        {

            Task task = new Task(() =>
            {
                //while (true)
                {
                    for (int i = start; i <= end; i++)
                    {
                        if (!isReadMeterStart)
                        {
                            break;
                        }
                        bool t = EquipmentData.DeviceManager.Read_Fault(03, (byte)i, out byte[] OutResult);
                        if (t)
                        {
                            if (!ViewModel2.meterStartS.ContainsKey(i.ToString()))
                            {
                                continue;
                            }
                            MeterStartControlViewModel meter = ViewModel2.meterStartS[i.ToString()];
                            meter.Motor = OutResult[2];
                            if (OutResult[3] == 1)
                                meter.IsMeter = true;
                            else
                                meter.IsMeter = false;
                        }
                        else
                        {
                        }
                        System.Threading.Thread.Sleep(10);
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            });
            task.Start();
        }

        private QueryDataBase DataBase
        {
            get { return Resources["QueryDataBase"] as QueryDataBase; }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataBase.Data = DataBase.GeneralDal.GetAllTableData(DataBase.TableName);
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            //MeterProtocolAdapter.Instance.ReadAddress(111);
            //DataBase.Data = DataBase.GeneralDal.GetAllTableData(DataBase.TableName,txt_sql.Text);
            string str = txt_sql.Text.Trim().Trim('\"'); ;
            string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", str);
            DataBase.GeneralDal = new DAL.GeneralDal(connString);
            List<string> fieldModels = DataBase.GeneralDal.GetTableNames();
            DataBase.TableNames.Clear();
            for (int i = 0; i < fieldModels.Count(); i++)
            {
                DataBase.TableNames.Add(fieldModels[i]);
            }
        }

        /// <summary>
        /// 清空接收区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProtViewModel.ReceiveData = "";
        }
        /// <summary>
        /// 清空发送区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click2(object sender, RoutedEventArgs e)
        {
            ProtViewModel.SendData = "";
        }

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{


        //    EquipmentData.Controller.UpdateMeterProtocol();
        //    EquipmentData.Controller.temVerify = new VerifyBase();
        //    EquipmentData.Controller.temVerify.ReadMeterAddrAndNo();

        //    EquipmentData.Controller.temVerify.Identity();

        //    var s2 = MeterProtocolAdapter.Instance.ReadData("校时总次数");

        //    string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("日期时间");
        //    DateTime readTime = DateTime.Now;  //读取GPS时间
        //    MeterProtocolAdapter.Instance.WriteDateTime(readTime);

        //    //MeterProtocolAdapter.Instance.ReadData("");
        //    //VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
        //    //float[] dv1 = MeterProtocolAdapter.Instance.ReadDemand(1, (byte)0);
        //    MeterProtocolAdapter.Instance.SetPulseCom(0);
        //    return;

        //    //EquipmentData.MeterGroupInfo.  AutoFieldName("MD_CARR_NAME", "测试2");
        //    //EquipmentData.MeterGroupInfo.Meters[0].SetProperty("MD_CARR_NAME", "测试2");//载波协议
        //    //var a = EquipmentData.DeviceManager.ReadHarmonicActivePower();//读取标准表谐波功率
        //    //var b = EquipmentData.DeviceManager.ReadHarmonicEnergy();//读取标准表谐波含有量--这里返回的就是小数了
        //    //float[] HarmonicContent = new float[60];
        //    //float[] HarmonicPhase = new float[60];

        //    //HarmonicContent[3] = 0.2f;
        //    //HarmonicPhase[3] = 120;
        //    //EquipmentData.DeviceManager.ZH3001SetPowerGetHarmonic("0", "0", "0", "1", "0", "0", HarmonicContent, HarmonicPhase, true);

        //    //var a=  EquipmentData.DeviceManager.PowerAngle(120,0,0,300,0,0,50);

        //}

        #region 谐波调试
        ///// <summary>
        ///// 初始化谐波数据
        ///// </summary>
        //private void initializationHarmonic()
        //{
        //    //tabHarmonic.Items.Add(new obje);
        //    string[] name = new string[] { "UA", "UB", "UC", "IA", "IB", "IC" };
        //    for (int z = 1; z <= 6; z++)
        //    {
        //        Dictionary<int, HarmonicData> keyValues = new Dictionary<int, HarmonicData>();

        //        TabItem item = new TabItem
        //        {
        //            Header = name[z - 1],
        //            Width = 100
        //        };
        //        //item.Background= System.Windows.Media.Brush.
        //        StackPanel panel = new StackPanel
        //        {
        //            Orientation = Orientation.Horizontal
        //        };
        //        for (int i = 1; i <= 4; i++)
        //        {
        //            StackPanel stackPanel = new StackPanel
        //            {
        //                Orientation = Orientation.Vertical,
        //                Margin = new Thickness(10, 5, 10, 5)
        //            };
        //            TextBlock textTips = new TextBlock
        //            {
        //                TextAlignment = TextAlignment.Center,
        //                Text = $"次数",
        //                Width = 50
        //            };

        //            TextBlock text11 = new TextBlock
        //            {
        //                TextAlignment = TextAlignment.Center,
        //                Text = "含量",
        //                Width = 70,
        //                Margin = new Thickness(5, 0, 5, 5)
        //            };

        //            TextBlock text22 = new TextBlock
        //            {
        //                TextAlignment = TextAlignment.Center,
        //                Text = "相位",
        //                Width = 70,
        //                Margin = new Thickness(5, 0, 5, 5)
        //            };

        //            StackPanel stack2 = new StackPanel
        //            {
        //                Orientation = Orientation.Horizontal
        //            };
        //            stack2.Children.Add(textTips);
        //            stack2.Children.Add(text11);
        //            stack2.Children.Add(text22);

        //            stackPanel.Children.Add(stack2);


        //            for (int j = 1; j <= 15; j++)
        //            {
        //                HarmonicData data = new HarmonicData();
        //                TextBlock text = new TextBlock
        //                {
        //                    Text = $"第{(i - 1) * 15 + j}次：",
        //                    Width = 50
        //                };
        //                TextBox text1 = new TextBox
        //                {
        //                    Width = 70,
        //                    Style = null,
        //                    Margin = new Thickness(5, 0, 5, 5)
        //                };
        //                Binding binding = new Binding("HarmonicContent");
        //                text1.SetBinding(TextBox.TextProperty, binding);

        //                TextBox text2 = new TextBox
        //                {
        //                    Width = 70,
        //                    Style = null,
        //                    Margin = new Thickness(5, 0, 5, 5)
        //                };
        //                Binding binding2 = new Binding("HarmonicPhase");
        //                text2.SetBinding(TextBox.TextProperty, binding2);
        //                StackPanel stack = new StackPanel
        //                {
        //                    DataContext = data,
        //                    Orientation = Orientation.Horizontal
        //                };
        //                stack.Children.Add(text);
        //                stack.Children.Add(text1);
        //                stack.Children.Add(text2);
        //                stackPanel.Children.Add(stack);

        //                if (!keyValues.ContainsKey((i - 1) * 15 + j))
        //                {
        //                    keyValues.Add((i - 1) * 15 + j, data);
        //                }
        //            }
        //            panel.Children.Add(stackPanel);
        //        }




        //        if (!HarmonicViewModel.harmonicData.ContainsKey(name[z - 1]))
        //        {
        //            HarmonicViewModel.harmonicData.Add(name[z - 1], keyValues);
        //        }
        //        item.Content = panel;
        //        tabHarmonic.Items.Add(item);
        //    }
        //    //ReadHarmonicData();
        //}


        ///// <summary>
        ///// 读取标准表谐波数据
        ///// </summary>
        //private void ReadHarmonicData()
        //{
        //    string[] name = new string[] { "UA", "UB", "UC", "IA", "IB", "IC" };
        //    Task task = new Task(() =>
        //    {
        //        while (true)
        //        {
        //            System.Threading.Thread.Sleep(1000);
        //            if (!isReadHarmonicData) break;
        //            if (!HarmonicViewModel.IsReadStaData) continue;

        //            float[] HarmonicEnerg = EquipmentData.DeviceManager.ReadHarmonicEnergy();
        //            float[] HarmonicAngle = EquipmentData.DeviceManager.ReadHarmonicAngle();
        //            if (HarmonicEnerg == null || HarmonicAngle == null) continue;
        //            if (HarmonicEnerg.Length >= 192 && HarmonicAngle.Length >= 192)
        //            {
        //                for (int j = 0; j < 6; j++)
        //                {
        //                    for (int i = 1; i <= 60; i++)
        //                    {
        //                        HarmonicViewModel.harmonicData[name[j]][i].HarmonicContent = HarmonicEnerg[i];
        //                        HarmonicViewModel.harmonicData[name[j]][i].HarmonicPhase = HarmonicAngle[i];
        //                    }
        //                }
        //            }
        //        }
        //    });
        //    task.Start();
        //}
        #endregion

        private void ReadStatus_Click(object sender, RoutedEventArgs e)
        {
            List<DeviceData> deviceDatas = EquipmentData.DeviceManager.Devices[LYTest.ViewModel.Device.DeviceName.误差板];
            int DeviceCount = deviceDatas.Count; //误差板485数量
            int MaxTaskCountPerThread = EquipmentData.Equipment.MeterCount / DeviceCount;//每个485备带几个误差板

            List<Task> status = new List<Task>();
            for (int i = 0; i < DeviceCount; i++)
            {
                int line = i;
                status.Add(Task.Run(() =>
                {
                    string notdown = "";
                    for (int n = 0; n < MaxTaskCountPerThread; n++)
                    {
                        int pos = (line * MaxTaskCountPerThread) + n + 1;
                        EquipmentData.DeviceManager.Read_Fault(3, (byte)pos, out byte[] outResult, line);
                        if (outResult[2] != 2 && outResult[3] == 1)
                        {
                            notdown += $"{pos},";
                        }
                        MeterStartControlViewModel meter = ViewModel2.meterStartS[pos.ToString()];
                        meter.Motor = outResult[2];
                        if (outResult[3] == 1)
                            meter.IsMeter = true;
                        else
                            meter.IsMeter = false;
                    }
                    return notdown;
                }));
            }
            Task.WaitAll(status.ToArray());

        }

        private void Chk_All3_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox ck)
            {
                bool check = ck.IsChecked == true;
                int line = int.Parse(ck.Tag.ToString());
                for (int i = EquipmentData.DeviceManager.BWCountPerLine * (line - 1); i < EquipmentData.DeviceManager.BWCountPerLine * line; i++)
                {
                    if (ViewModel2.meterStartS.ContainsKey((i + 1).ToString()))
                        ViewModel2.meterStartS[(i + 1).ToString()].IsCheck = check;
                }
            }
        }

        private void SetNaiYa_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentData.Equipment.EquipmentType != "单相台")
            {
                EquipmentData.DeviceManager.SetEquipmentPowerSupply(1, true);
            }
            else
            {
                EquipmentData.DeviceManager.SetEquipmentPowerSupply(1, false);
                EquipmentData.DeviceManager.ControlMeterInsulationRelay(1);
            }
        }

        private void ResetNaiYa_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentData.Equipment.EquipmentType != "单相台")
            {
                EquipmentData.DeviceManager.SetEquipmentPowerSupply(0, true);
            }
            else
            {
                EquipmentData.DeviceManager.SetEquipmentPowerSupply(0, false);
                EquipmentData.DeviceManager.ControlMeterInsulationRelay(0);
            }
        }

        private void DownScheme_Click(object sender, RoutedEventArgs e)
        {
            string where = Txtwhere.Text;
            if ("SCHEMA_ID" == where) return;
            EquipmentData.MeterGroupInfo.DownSchemeInfoFromMis(where);
        }
    }
}
