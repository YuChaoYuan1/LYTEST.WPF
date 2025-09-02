using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LYTest.Utility
{
    public sealed class WindowProcess
    {
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        public static void FocusProcess(string procName)
        {
            Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);
            if (objProcesses.Length > 0)
            {
                SwitchToThisWindow(objProcesses[0].MainWindowHandle, true);
            }
        }

        public static void LYBackup()
        {
            try
            {
                Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName("LYbackup");
                if (objProcesses.Length <= 0)
                {
                    System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "LYbackup.exe"), Directory.GetCurrentDirectory());
                }

            }
            catch { }
        }
    }
}
