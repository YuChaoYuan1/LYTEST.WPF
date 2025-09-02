using System;
using System.Threading.Tasks;
using System.Windows;

namespace initialize
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            CombRegion.ItemsSource = Enum.GetValues(typeof(Regions));
            //CombRegion.SelectedIndex = 0;
            panelRegion.Visibility = Visibility.Visible;
            TxtLog.Visibility = Visibility.Collapsed;
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            Regions? region = CombRegion.SelectedItem as Regions?;
            if (region == null) return;
            panelRegion.Visibility = Visibility.Collapsed;
            TxtLog.Visibility = Visibility.Visible;
            TxtLog.AppendText("初始化配置：");
            TxtLog.AppendText(region.ToString());
            TxtLog.AppendText(Environment.NewLine);
            Universal init;
            switch (region)
            {
                //case Regions.通用:
                //    init = new Universal();
                //    break;
                //case Regions.新疆米东计量基地单相线:
                //    init = new XinJiangSingle();
                //    break;
                case Regions.人工单相台:
                    init = new XinJiangManualSingle();
                    break;
                case Regions.人工三相台:
                    init = new XinJiangManualThree();
                    break;
                case Regions.人工改造单相台:
                    init = new XinJiangManualRefitSingle();
                    break;
                case Regions.人工改造三相台:
                    init = new XinJiangManualRefitThree();
                    break;
                case Regions.SG版本软件:
                    init = new SG_Version();
                    break;
                default:
                    init = new Universal();
                    break;
            }
            init.OutMessage += Init_OutMessage;
            Task.Run(() =>
            {
                init.Execute();
                AppendText("初始化完成。");
                AppendText(Environment.NewLine);
                AppendText(Environment.NewLine);
            });
        }

        private void AppendText(string msg)
        {
            TxtLog.Dispatcher.Invoke(() =>
            {
                TxtLog.AppendText(msg);
            });
        }

        private void Init_OutMessage(object sender, string e)
        {
            AppendText(e);
            AppendText(Environment.NewLine);
        }

    }
}
