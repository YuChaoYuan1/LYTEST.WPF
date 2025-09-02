using LYTest.MeterProtocol.DataFlag;
using System.Linq;

namespace LYTest.MeterProtocol
{
    public class MeterProtocol
    {
        public static MeterProtocalItem Select(string name)
        {
            var data = DataFlagS.DIS.FirstOrDefault(x => x.DataFlagDiName == name || x.DataFlagDi == name || x.DataFlagOi == name);
            if (data != null)
            {
                return new MeterProtocalItem(data);
            }
            else
                return null;
        }
    }
}
