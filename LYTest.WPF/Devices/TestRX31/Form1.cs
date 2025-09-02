using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZH.Rx_31;
using ZH.Rx_31.Model;

namespace TestRX31
{
    public partial class Form1 : Form
    {
        ZH_Rx_31 zh_Rx_31 = new ZH_Rx_31("COM5");
        public Form1()
        {
            InitializeComponent();
            zh_Rx_31.LinkEquip();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StandarMeterInfo tagInfo = zh_Rx_31.ReadStMeterInfo(); 
        }
    }
}
