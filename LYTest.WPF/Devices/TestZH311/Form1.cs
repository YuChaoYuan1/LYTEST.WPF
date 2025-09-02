using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZH311;

namespace TestZH311
{
    public partial class Form1 : Form
    {
        zh311 Zh311 = new zh311();
        public Form1()
        {
            InitializeComponent();
            Zh311.InitSettingCom(3, 2000, 100);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] strFarmy = new string[0];
            Zh311.Connect(out  strFarmy);
        }
    }
}
