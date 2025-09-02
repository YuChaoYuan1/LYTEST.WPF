using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace LYTest.WPF
{
    internal class Program
    {

        [System.STAThreadAttribute()]
        //[System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        static void Main(string[] args)
        {

            // args格式说明：软件名称;厂家名称;软件Logo;软件图标
            //   如：XJ9001;国网新疆计量中心;gw30.png;zip.ico

            if (args.Length > 0)
            {
                string[] arr = args[0].Split(';');
                if (arr.Length >= 4)
                {
                    AppInfo.SoftModel = arr[0];
                    AppInfo.Title = arr[1];
                    AppInfo.LogoImage = arr[2];
                    AppInfo.Icon = arr[3];
                }
            }

            LYTest.WPF.App app = new LYTest.WPF.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
