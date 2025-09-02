using LYTest.MeterProtocol.DataFlag;
using LYTest.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_ProtocolSelect.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ProtocolSelect : Window
    {
        private static Window_ProtocolSelect instance = null;

        public static Window_ProtocolSelect Instance
        {
            get
            {
                if (instance == null)
                {
                    // 在UI线程上调用实例化代码
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        instance = new Window_ProtocolSelect();
                    });
                }
                return instance;
            }
        }
        public Window_ProtocolSelect()
        {
            InitializeComponent();
            Topmost = true;
            Deactivated += MainWindow_Deactivated;
            this.DataContext = EquipmentData.DISetsInfo;

        }
        private DynamicViewModel viewModel;
        /// <summary>
        /// 注释
        /// </summary>
        public DynamicViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;

                if (ViewModel != null)
                {
                    GetSelectMoedl();
                }

            }
        }
        private void GetSelectMoedl()
        {
            //ParaItemNo
            //string ParaNo = ViewModel.GetProperty("ParaItemNo") as string;
            //string ItemName = "";
            //switch (ParaNo)
            //{
            //    case ProjectID.通讯协议检查试验2:
            //    case ProjectID.通讯协议检查试验:
            //    case ProjectID.显示功能:
            //        ItemName = "数据项名称";
            //        break;
            //    case ProjectID.红外通信试验:
            //    case ProjectID.载波通信测试:
            //        ItemName = "项目名称";
            //        break;
            //    default:
            //        break;
            //}

            string Name = ViewModel.GetProperty("数据项名称") as string;
            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = ViewModel.GetProperty("项目名称") as string;
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                for (int i = 0; i < EquipmentData.DISetsInfo.DIClassModels.Count; i++)
                {
                    var DiModel = EquipmentData.DISetsInfo.DIClassModels[i].DIModels.FirstOrDefault(x => x.DataFlagDiName == Name);
                    if (DiModel != null)
                    {
                        EquipmentData.DISetsInfo.IsFlag = false;
                        EquipmentData.DISetsInfo.SelectDIClass = EquipmentData.DISetsInfo.DIClassModels[i];
                        EquipmentData.DISetsInfo.SelectDIClass.SelectDI = DiModel;
                        EquipmentData.DISetsInfo.IsFlag = true;
                        break;
                    }
                }
            }
        }

        private bool isFormLoaded = false;
        public void ShowProtocol(double X, double Y, LYTest.ViewModel.DynamicViewModel model)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    isFormLoaded = false;
                    // 设置窗体的位置
                    this.Left = X;
                    this.Top = Y;
                    ViewModel = model;
                    Visibility = Visibility.Visible;
                    Show();
                    isFormLoaded = true;
                }
                catch
                { }
            }));
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isFormLoaded) return;

            if (ViewModel != null && EquipmentData.DISetsInfo.SelectDIClass != null && EquipmentData.DISetsInfo.IsFlag)
            {
                SetModel(EquipmentData.DISetsInfo.SelectDIClass.SelectDI);
                Visibility = Visibility.Collapsed;
            }
        }

        private void SelectData(object sender, RoutedEventArgs e)
        {
            EquipmentData.DISetsInfo.SelectData(SelectText.Text);

        }
        #region 内部
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!IsMouseOver)
            {
                Mouse.Capture(null);
            }
        }
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
        #endregion
        private void ClearSelectText(object sender, RoutedEventArgs e)
        {
            SelectText.Text = "";
            SelectData(null, null);
            SelectText.Focus();
        }

        private void SetColumnValue(string Name, string Value)
        {
            List<string> NameList = viewModel.GetAllProperyName();
            if (NameList.Contains(Name))
            {
                viewModel.SetProperty(Name, Value);
            }
        }

        private void SetModel(DI dI)
        {
            if (dI == null) return;

            SetColumnValue("数据项名称", dI.DataFlagDiName);
            SetColumnValue("项目名称", dI.DataFlagDiName);

            SetColumnValue("长度", dI.DataLength);
            SetColumnValue("小数位", dI.DataSmallNumber);
            SetColumnValue("数据格式", dI.DataFormat);

            SetColumnValue("标识编码", dI.DataFlagOi);
            SetColumnValue("标识编码698", dI.DataFlagOi);
            SetColumnValue("数据标识", dI.DataFlagOi);
            SetColumnValue("标识符", dI.DataFlagOi);

            SetColumnValue("标识编码645", dI.DataFlagDi);

            //string ParaNo = ViewModel.GetProperty("ParaItemNo") as string;
            //switch (ParaNo)
            //{
            //    case ProjectID.通讯协议检查试验2:
            //        ViewModel.SetProperty("数据项名称", dI.DataFlagDiName);
            //        ViewModel.SetProperty("标识编码", dI.DataFlagDi);
            //        ViewModel.SetProperty("长度", dI.DataLength);
            //        ViewModel.SetProperty("小数位", dI.DataSmallNumber);
            //        ViewModel.SetProperty("数据格式", dI.DataFormat);
            //        ViewModel.SetProperty("标识编码698", dI.DataFlagOi);
            //        break;
            //    case ProjectID.通讯协议检查试验:
            //        ViewModel.SetProperty("数据项名称", dI.DataFlagDiName);
            //        ViewModel.SetProperty("标识编码", dI.DataFlagOi);
            //        ViewModel.SetProperty("长度", dI.DataLength);
            //        ViewModel.SetProperty("小数位", dI.DataSmallNumber);
            //        ViewModel.SetProperty("数据格式", dI.DataFormat);
            //        break;
            //    case ProjectID.显示功能:
            //        ViewModel.SetProperty("数据项名称", dI.DataFlagDiName);
            //        ViewModel.SetProperty("长度", dI.DataLength);
            //        ViewModel.SetProperty("小数位", dI.DataSmallNumber);
            //        ViewModel.SetProperty("数据格式", dI.DataFormat);
            //        ViewModel.SetProperty("标识编码645", dI.DataFlagDi);
            //        ViewModel.SetProperty("标识编码698", dI.DataFlagOi);
            //        break;
            //    case ProjectID.红外通信试验:
            //        ViewModel.SetProperty("项目名称", dI.DataFlagDiName);
            //        ViewModel.SetProperty("标识符", dI.DataFlagOi);
            //        break;
            //    case ProjectID.载波通信测试:
            //        ViewModel.SetProperty("项目名称", dI.DataFlagDiName);
            //        ViewModel.SetProperty("数据标识", dI.DataFlagOi);
            //        break;
            //    default:
            //        break;
            //}

        }


        /// <summary>
        /// 查找出来的数据选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (selectListBox.SelectedItem == null) return;
            SetModel(selectListBox.SelectedItem as DI);
            Visibility = Visibility.Collapsed;
        }
    }
}
