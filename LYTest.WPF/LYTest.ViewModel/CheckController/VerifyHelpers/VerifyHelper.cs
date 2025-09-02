using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.CheckController.VerifyHelpers
{
    public  class VerifyHelper
    {

        //设备相关
        //电表相关
        //方法帮助
        //ui相关

        public Dictionary<string, string[]> GetCheckResult()
        {
            return EquipmentData.CheckResults.GetCheckResult();
        }

    }
}
