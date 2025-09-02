using LYTest.ViewModel;
using LYTest.WPF.Model;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LYTest.WPF.View
{
    /// <summary>
    /// View_MeterMessage.xaml 的交互逻辑
    /// </summary>
    public partial class View_MeterMessage
    {
        public View_MeterMessage()
        {
            InitializeComponent();
            Name = "标准表信息";
            DockStyle.Position = eDockSide.Bottom;
            DockStyle.CanClose = false;
            DockStyle.CanFloat = false;
            DockStyle.CanDockAsDocument= false;
            //DockStyle.CanDockBottom = false;
            DockStyle.CanDockLeft = false;
            DockStyle.CanDockRight = false;
            DockStyle.CanDockTop = false;

            DataContext = EquipmentData.StdInfo;


            if (EquipmentData.Equipment.EquipmentType == "三相台")
            {
                DockStyle.FloatingSize = new Size(500, 400);
            }
            else
            {
                DockStyle.FloatingSize = new Size(500, 250);
            }
        }
    }
}
