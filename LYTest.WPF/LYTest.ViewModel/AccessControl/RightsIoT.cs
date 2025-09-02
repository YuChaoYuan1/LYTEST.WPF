#define IoT

using System.Collections.Generic;

namespace LYTest.ViewModel.AccessControl
{
    public sealed class RightsIoT
    {
        public static readonly List<string> MeterSort = new List<string>()
        {
            "智能表",
#if IoT
            "物联电能表",
#endif
        };
    }
}
