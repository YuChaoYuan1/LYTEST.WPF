using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Run
{
    internal class Program
    {
        static void Main()
        {

            //XJ9001;国网新疆计量中心;gw30.png;zip.ico
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ".\\LYTest.WPF.exe",
                Arguments = "DF8001;国网江西电力;wsd.png;wsd.ico"
            };
            System.Diagnostics.Process.Start(startInfo);

        }
    }
}
