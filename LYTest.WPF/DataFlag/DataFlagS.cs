using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LYTest.DataFlag
{
    public class DataFlagS
    {
        private static readonly string DataFlagPath = string.Format(@"{0}\xml\DataFlag.xml", Directory.GetCurrentDirectory());
        public static List<DI> DIS = new List<DI>();

        /// <summary>
        /// 载入数据标识
        /// </summary>
        public static void LoadDataFlag()
        {
            DIS.Clear();
            XmlDocument doc = new XmlDocument();
            //TODO 这里后期起始可以序列化来实现,目前格式怕还是有所改动先逐个读取
            //TODO 这里先全部用字符串，统一一点,其他地方要使用的化自己转换
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreComments = true//忽略文档里面的注释
            };
            using (XmlReader reader = XmlReader.Create(DataFlagPath, settings))
            {
                //XmlReader reader = XmlReader.Create(DataFlagPath, settings);
                doc.Load(reader);
                XmlNode xn = doc.SelectSingleNode("DataFlagInfo");
                XmlNodeList xnl = xn.ChildNodes;
                int index = 0;
                foreach (XmlNode xn1 in xnl)
                {
                    XmlElement xe = (XmlElement)xn1;
                    DI dI = new DI()
                    {
                        DataFlagDiName = xe.GetAttribute("DataFlagDiName"),
                        DataFlagDi = xe.GetAttribute("DataFlagDi"),
                        DataFlagOi = xe.GetAttribute("DataFlagOi"),
                        DataLength = xe.GetAttribute("DataLength"),
                        DataSmallNumber = xe.GetAttribute("DataSmallNumber"),
                        DataFormat = xe.GetAttribute("DataFormat"),
                        ClassName = xe.GetAttribute("ClassName"),
                        EmSecurityMode = xe.GetAttribute("EmSecurityMode"),
                        Rights = xe.GetAttribute("Rights"),
                        SortNo = xe.GetAttribute("SortNo"),
                        Chip = xe.GetAttribute("Chip"),
                    };
                    xe.SetAttribute("ClassName", "电能量类");
                    xe.SetAttribute("Chip", "0");
                    xe.SetAttribute("SortNo", (++index).ToString());
                    xe.SetAttribute("EmSecurityMode", "1");
                    xe.SetAttribute("Rights", "11");

                    DIS.Add(dI);
                }
            }
        }
    }
}
