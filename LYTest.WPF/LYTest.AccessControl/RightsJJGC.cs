#define R46

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.AccessControl
{
    public sealed class RightsJJGC
    {
        public static readonly List<string> JJGC = new List<string>()
        {
            "JJG596-2012",
#if R46
            "IR46",
            "Q/GDW12175-2021",
            "Q/GDW10827-2020",
            "Q/GDW10364-2020",
#endif
        };
    }
}
