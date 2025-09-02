using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LYbackup
{
    class App
    {
        static void Main(string[] args)
        {
#if MY_DEBUG
            new BackupLYM2().RunOnce("");
            new BackupLYM2().RunOnce("C");
            new BackupLYM2().RunOnce("C:");
            new BackupLYM2().RunOnce(@"C:\");
            new BackupLYM2().RunOnce(@"C:\不存在");
            new BackupLYM2().RunOnce(@"C:\不存在\");
            new BackupLYM2().RunOnce(@"C:\LY8001-M2");
            new BackupLYM2().RunOnce(@"C:\LY8001-M2\");
#else
            if (args.Length > 0)
                new BackupLYM2().RunOnce(args[0]);
#endif
        }
    }
}
