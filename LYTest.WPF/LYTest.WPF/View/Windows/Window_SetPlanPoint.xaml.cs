using LYTest.Core.Model.Schema;
using System;
using System.Collections.Generic;
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
    /// Window_SetPlanPoint.xaml 的交互逻辑
    /// </summary>
    public partial class Window_SetPlanPoint : Window
    {
        public JJGParams JJGParams { get; private set; }

        public Window_SetPlanPoint()
        {
            InitializeComponent();
            List<string> items = new List<string>();
            items.Add("JJG596-2012");
            items.Add("JJG596-202X");
            CmbJJG.ItemsSource = items;

            items = new List<string>();
            items.Add("单相");
            items.Add("三相四线");
            items.Add("三相三线");
            CmbxPxW.ItemsSource = items;

            items = new List<string>();
            items.Add("2");
            items.Add("1");
            items.Add("0.5S");
            items.Add("0.2S");
            items.Add("A");
            items.Add("B");
            items.Add("C");
            items.Add("D");
            items.Add("E");
            CmbClassP.ItemsSource = items;
            CmbClassQ.ItemsSource = items;

        }

        private void JJGPoint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CmbJJG.Text))
            {
                TxbMsg.Text = "请选择 规程";
                return;
            }
            if (string.IsNullOrWhiteSpace(CmbxPxW.Text))
            {
                TxbMsg.Text = "请选择 接线方式";
                return;
            }
            if (string.IsNullOrWhiteSpace(CmbClassP.Text) && ChkP.IsChecked == true)
            {
                TxbMsg.Text = "请选择 有功等级";
                return;
            }
            if (string.IsNullOrWhiteSpace(CmbClassQ.Text) && ChkQ.IsChecked == true)
            {
                TxbMsg.Text = "请选择 无功等级";
                return;
            }
            if (ChkForward.IsChecked == false && ChkReverse.IsChecked == false)
            {
                TxbMsg.Text = "请选择 正向/反向，至少选一个";
                return;
            }
            if (ChkP.IsChecked == false && ChkQ.IsChecked == false)
            {
                TxbMsg.Text = "请选择 有功/无功，至少选一个";
                return;
            }


            JJGParams = new JJGParams()
            {
                JJGName = CmbJJG.Text,
                Transformer = RdbtnHgq.IsChecked == true,
                Forward = ChkForward.IsChecked == true,
                Reverse = ChkReverse.IsChecked == true,
                Wiring = CmbxPxW.Text,
                Active = ChkP.IsChecked == true,
                ActiveClass = CmbClassP.Text,
                Reactive = ChkQ.IsChecked == true,
                ReactiveClass = CmbClassQ.Text,
                Imax4Ib = ChkImax4Ib.IsChecked == true,
            };

            DialogResult = true;
            Close();
        }
    }
}
