using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace LYTest.MeterProtocol.Protocols.ApplicationLayer
{
    public class ObjectInfosManage
    {

        public List<ObjectAttributes> AttributeInfos { get; set; }


        private static ObjectInfosManage obXmlInfo = null;
        /// <summary>
        /// 单例
        /// </summary>
        /// <returns></returns>
        public static ObjectInfosManage Instance()
        {
            if (obXmlInfo == null)
            {
                obXmlInfo = new ObjectInfosManage();
            }
            return obXmlInfo;
        }

        public ObjectInfosManage()
        {
            LoadObjectInfos();
        }

        private void LoadObjectInfos()
        {
            AttributeInfos = new List<ObjectAttributes>();
            //XDocument _XDoc = XDocument.Load(ConfigurationManager.AppSettings["Oad"].ToString());
            //XDocument _XDoc = XDocument.Load(@"E:\工作\LY\Resource\Xml\OadInfosConfig.xml");
            // Directory.GetCurrentDirectory()
            XDocument _XDoc = XDocument.Load(Directory.GetCurrentDirectory()+@"\Xml\OadInfosConfig.xml");

            //
            if (_XDoc != null)
            {
                foreach (XElement item in _XDoc.Descendants("OadConfig").Elements("Oad"))
                {
                    ObjectAttributes oad = new ObjectAttributes
                    {
                        Oad = item.Attribute("Oad").Value.ToString(),
                        BigType = item.Attribute("BigType").Value.ToString(),
                        ItemCount = Convert.ToInt32(item.Attribute("ItemCount").Value.ToString()),
                        DataInfo = new List<DataInfos>()
                    };
                    if (item.HasElements)
                    {
                        foreach (XElement e in item.Elements("Data"))
                        {
                            DataInfos info = new DataInfos
                            {
                                DataTypeCode = Convert.ToInt32(e.Attribute("DataTypeCode").Value.ToString().Trim()),
                                DataTypeName = e.Attribute("DataTypeName").Value.ToString(),
                                LengthFlag = e.Attribute("LengthFlag").Value.ToString().Trim() == "1",
                                DataLength = Convert.ToInt32(e.Attribute("DataLength").Value.ToString().Trim()),
                                FloatCount = Convert.ToInt32(e.Attribute("FloatCount").Value.ToString().Trim())
                            };
                            oad.DataInfo.Add(info);
                        }
                        AttributeInfos.Add(oad);
                    }


                }
            }

        }

    }
}
