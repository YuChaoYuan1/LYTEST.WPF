using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using LYTest.MeterProtocol.Helper;
using LYTest.MeterProtocol.Struct;

namespace LYTest.MeterProtocol.Protocols.DgnProtocol
{
    /// <summary>
    /// 功能描述：载波方案模型
    /// </summary>
    public class CarrierList : List<CarrierWareInfo>
    {
        private static readonly string path = Application.StartupPath + "\\Xml\\CarrierConfig.xml";
        /// <summary>
        /// 读取初始化载波方案模型
        /// </summary>
        public  static CarrierWareInfo Load(string name)
        {
            //Clear();           //清空载波方案集合
            if (name=="")
            {
                name = "标准载波";
            }
            XmlNode xml = XmlControl.Load(path);
            if (xml == null)
            {
                xml = XmlControl.CreateXmlNode("CarrierProtocol");
                XmlNode node = XmlControl.CreateXmlNode("R",
                                                      "Name", "标准载波方案",
                                                      "CarrierType", "2041",
                                                      "RdType", "东软",
                                                      "CommuType", "COM",
                                                      "Baudrate", "9600,n,8,1",
                                                      "Comm", "COM1",
                                                      "CmdTime", "10",
                                                      "ByteTime", "10",
                                                      "IsRoute", "false",
                                                      "IsBroad", "false");

                xml.AppendChild(node);
                //XmlControl.Save(xml, Application.StartupPath + Const.Variable.CONST_CARRIER);
            }
            for (int i = 0; i < xml.ChildNodes.Count; i++)
            {
                XmlNode root = XmlControl.FindSencetion(xml.ChildNodes[i], XmlControl.XPath(string.Format("R,Name,{0}", name)));
                if (root==null)
                {
                    continue;
                }
                CarrierWareInfo item = new CarrierWareInfo
                {
                    Name = name, //协议库名称
                   CarrierType = XmlControl.GetValue(root, "CarrierType"), //协议库名称
                    RdType = XmlControl.GetValue(root, "RdType"), //协议库名称
                    CommuType = XmlControl.GetValue(root, "CommuType"), //协议库名称
                    Baudrate = XmlControl.GetValue(root, "BaudRate"), //协议库名称
                    Port = XmlControl.GetValue(root, "Comm"), //协议库名称
                    OutTime = XmlControl.GetValue(root, "CmdTime"), //协议库名称
                    ByteTime = XmlControl.GetValue(root, "ByteTime"), //协议库名称
                    IsRoute = XmlControl.GetValue(root, "IsRoute") == "True",
                    IsBroad = XmlControl.GetValue(root, "IsBroad") == "True",
                };
                //Add(item);
                return item;
            }
            return null;
        }
        ///// <summary>
        ///// 获取当前节点属性值
        ///// </summary>
        ///// <param name="XmlInNode">需要获取属性值的节点</param>
        ///// <param name="AttributeName">对应节点属性名称</param>
        ///// <returns></returns>
        //public static string GetNodeAttributeValue(XmlNode XmlInNode, string AttributeName)
        //{
        //    XmlAttribute findAtt = XmlInNode.Attributes[AttributeName];
        //    if (findAtt == null) return "";

        //    return XmlInNode.Attributes[AttributeName].Value;
        //}

        /// <summary>
        /// 查询一个节点对象
        /// </summary>
        /// <param name="parentNode">父域节点对象</param>
        /// <param name="queryString">XPath查询表达式</param>
        /// <returns></returns>
        public static XmlNode FindSencetion(XmlNode parentNode, string queryString)
        {
            if (queryString == "")
                return parentNode;
            try
            {
                return parentNode.SelectSingleNode(queryString);
            }
            catch
            {
                return null;
            }


        }
        /// <summary>
        /// 获取需要查询的子节点值
        /// </summary>
        /// <param name="XmlFatherNode">父域节点</param>
        /// <param name="QueryString">XPath查询表达式</param>
        /// <returns></returns>
        public static string GetValue(XmlNode XmlFatherNode, string QueryString)
        {
            return GetValue(FindSencetion(XmlFatherNode, QueryString));
        }
        /// <summary>
        /// 获取当前节点值
        /// </summary>
        /// <param name="XmlFindNode">当前节点对象</param>
        /// <returns></returns>
        public static string GetValue(XmlNode XmlFindNode)
        {
            if (XmlFindNode == null)
                return "";
            else
                return XmlFindNode.ChildNodes[0].Value;
        }
        public List<string> GetNames()
        {
            List<string> list = new List<string>();
            foreach (CarrierWareInfo info in this)
            {
                list.Add(info.Name);
            }

            return list;
        }

    }
}
