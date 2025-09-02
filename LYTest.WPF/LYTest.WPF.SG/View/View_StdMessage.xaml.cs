using LYTest.ViewModel;
using LYTest.WPF.SG.Model;
using System.Windows;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_StdMessage.xaml 的交互逻辑
    /// </summary>
    public partial class View_StdMessage
    {
        public View_StdMessage()
        {
            InitializeComponent();
            Name = "标准表信息";
            DockStyle.Position = DockSideE.Bottom;
            DockStyle.CanClose = false;
            DockStyle.CanFloat = false;
            DockStyle.CanDockAsDocument = false;
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
                DockStyle.FloatingSize = new Size(500, 300);
            }
        }
    }
}
