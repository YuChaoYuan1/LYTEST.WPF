using LYTest.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_RatePeriodSelect.xaml 的交互逻辑
    /// </summary>
    public partial class Window_RatePeriodSelect : Window
    {
        public enum FormatN
        {
            /// <summary>
            /// HHmmNN连续
            /// </summary>
            F1,
            /// <summary>
            /// HH:mm(平)
            /// </summary>
            F2
        }

        private static Window_RatePeriodSelect instance = null;

        public static Window_RatePeriodSelect Instance
        {
            get
            {
                if (instance == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        instance = new Window_RatePeriodSelect();
                    });
                }
                return instance;
            }
        }

        public ObservableCollection<string> RatePeriods { get; private set; } = new ObservableCollection<string>();

        private string PropName = "标准数据";
        private FormatN ValueFormat = FormatN.F1;
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

        private readonly List<string> Hours = new List<string>();
        private readonly List<string> Minutes = new List<string>();
        private readonly Dictionary<string, string> Rates = new Dictionary<string, string>();

        public Window_RatePeriodSelect()
        {
            InitializeComponent();
            Topmost = true;
            Deactivated += MainWindow_Deactivated;

            Hours = new List<string>();
            for (int i = 0; i < 24; i++)
            {
                Hours.Add(i.ToString("D2"));
            }
            CmbHour.ItemsSource = Hours;

            Minutes = new List<string>();
            for (int i = 0; i < 60; i++)
            {
                Minutes.Add(i.ToString("D2"));
            }
            CmbMinute.ItemsSource = Minutes;

            //Rate
            var tmpRates = DAL.CodeDictionary.GetLayer2("Rate");

            Rates = new Dictionary<string, string>();
            foreach (var item in tmpRates)
            {
                if (item.Key != "总")
                {
                    Rates.Add(item.Key, item.Value.PadLeft(2, '0')); //这里取的是字典配置的值，是不是要考虑别人改这个值把它写死?
                }
            }

            CmbRate.ItemsSource = Rates.Keys;

            if (CmbHour.Items.Count > 0) CmbHour.SelectedIndex = 0;
            if (CmbMinute.Items.Count > 0) CmbMinute.SelectedIndex = 0;
            if (CmbRate.Items.Count > 0) CmbRate.SelectedIndex = 0;


            //Rates = new Dictionary<string, string>();
            //var keys = CmbRate.ItemsSource as Dictionary<string, string>.KeyCollection;
            //int ki = 0;
            //foreach (var item in keys)//这里配置的第一个是总0，没总要新编号
            //{
            //    Rates.Add(item, ki.ToString("D2"));
            //    ++ki;
            //}

            selectListBox.ItemsSource = RatePeriods;
        }

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

        private void GetSelectMoedl()
        {
            RatePeriods.Clear();
            string stdData = ViewModel.GetProperty(PropName) as string;
            if (!string.IsNullOrWhiteSpace(stdData))
            {
                switch (ValueFormat)
                {
                    case FormatN.F1:
                        int count = stdData.Length / 6;
                        for (int i = 0; i < count; i++)
                        {
                            string one = stdData.Substring(i * 6, 6);
                            string hour = one.Substring(0, 2);
                            if (!Hours.Contains(hour)) continue;
                            string minute = one.Substring(2, 2);
                            if (!Minutes.Contains(minute)) continue;
                            string rate = one.Substring(4, 2);
                            if (!Rates.ContainsValue(rate)) continue;
                            RatePeriods.Add($"{hour}:{minute}({Rates.First(v => v.Value == rate).Key})");
                        }
                        break;
                    case FormatN.F2:
                        string[] stdDs = stdData.Replace("):", ",").Replace("）:", ",").Replace("）：", ",").Replace(")：", ",").Split(',', '，');
                        for (int i = 0; i < stdDs.Length; i++)
                        {
                            string[] one = stdDs[i].Replace(" ", "").Split(':', '：', '(', '（', ')', '）');
                            if (one.Length < 3) continue;
                            if (string.IsNullOrWhiteSpace(one[0])) continue;
                            string hour = one[0].PadLeft(2, '0');
                            if (!Hours.Contains(hour)) continue;
                            if (string.IsNullOrWhiteSpace(one[1])) continue;
                            string minute = one[1].PadLeft(2, '0');
                            if (!Minutes.Contains(minute)) continue;
                            if (string.IsNullOrWhiteSpace(one[2])) continue;
                            string rate = one[2];
                            if (!Rates.ContainsKey(rate)) continue;
                            RatePeriods.Add($"{hour}:{minute}({rate})");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        //private bool isFormLoaded = false;
        public void ShowRatePeriod(double X, double Y, DynamicViewModel model, string propname, FormatN formatn)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    //isFormLoaded = false;
                    // 设置窗体的位置
                    this.Left = X;
                    this.Top = Y;
                    PropName = propname;
                    ValueFormat = formatn;
                    ViewModel = model;
                    Visibility = Visibility.Visible;
                    txbMsg.Visibility = Visibility.Collapsed;
                    Show();
                    //isFormLoaded = true;
                }
                catch
                { }
            }));
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CmbHour.Text) || string.IsNullOrWhiteSpace(CmbMinute.Text) || string.IsNullOrWhiteSpace(CmbRate.Text))
            {
                txbMsg.Text = "数据不能为空";
                txbMsg.Visibility = Visibility.Visible;
                return;
            }
            if (CmbRate.Text == "总")
            {
                txbMsg.Text = "这里不能选“总”";
                txbMsg.Visibility = Visibility.Visible;
                return;
            }
            txbMsg.Text = "";
            txbMsg.Visibility = Visibility.Collapsed;
            if (RatePeriods.Count(x => x.IndexOf($"{CmbHour.Text}:{CmbMinute.Text}") != -1) <= 0)
            {
                RatePeriods.Add($"{CmbHour.Text}:{CmbMinute.Text}({CmbRate.Text})");
            }
            else
            {
                txbMsg.Text = "该时间已添加";
                txbMsg.Visibility = Visibility.Visible;
                return;
            }
        }

        private void ClearSelectText(object sender, RoutedEventArgs e)
        {
            if (selectListBox.SelectedIndex == -1) //没有选中清空所以
            {
                RatePeriods.Clear();
            }
            else  //有选中的清空，删除选中的?
            {
                RatePeriods.RemoveAt(selectListBox.SelectedIndex);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder @string = new StringBuilder();
            switch (ValueFormat)
            {
                case FormatN.F1:
                    foreach (var item in RatePeriods)
                    {
                        @string.Append($"{item.Split('(')[0].Replace(":", "")}{Rates[item.Split('(')[1].Replace(")", "")]}");
                    }
                    break;
                case FormatN.F2:
                    @string.Append(string.Join(",", RatePeriods));
                    break;
                default:
                    foreach (var item in RatePeriods)
                    {
                        @string.Append($"{item.Split('(')[0].Replace(":", "")}{Rates[item.Split('(')[1].Replace(")", "")]}");
                    }
                    break;
            }

            ViewModel.SetProperty(PropName, @string.ToString());
            Visibility = Visibility.Collapsed;
        }
    }
}
