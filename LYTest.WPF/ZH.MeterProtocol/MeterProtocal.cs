using LYTest.MeterProtocol.DataFlag;
using System;
using System.Linq;

namespace LYTest.MeterProtocol
{
    public class MeterProtocal
    {
        public static MeterProtocalItem Select(string name)
        {
            try
            {
                var data = DataFlagS.DIS.FirstOrDefault(x => x.DataFlagDiName == name || x.DataFlagDi == name || x.DataFlagOi == name);
                if (data != null)
                {
                    return new MeterProtocalItem(data);
                }
                else
                {
                    LYTest.Utility.Log.LogManager.AddMessage($"未找到数据标识名称：{name}", LYTest.Utility.Log.EnumLogSource.检定业务日志, LYTest.Utility.Log.EnumLevel.Warning);
                    return null;
                }
            }
            catch (Exception ex)
            {
                LYTest.Utility.Log.LogManager.AddMessage($"查找数据标识名称异常：{ex}", LYTest.Utility.Log.EnumLogSource.检定业务日志, LYTest.Utility.Log.EnumLevel.Error);
                return null;
            }
        }


    }

}




