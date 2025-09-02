using System.IO;
using System.Xml;

namespace LYTest.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlControl
    {
        private readonly XmlNode _XmlNode;
        //private readonly string _FilePath = "";

        private static readonly XmlDocument doc = new XmlDocument();

        /// <summary>
        /// 空的构造函数，不做任何操作
        /// </summary>
        public XmlControl()
        {
        }
        /// <summary>
        /// 构造函数，根据xmlnode构造
        /// </summary>
        /// <param name="nodeTemp"></param>
        public XmlControl(XmlNode nodeTemp)
        {
            _XmlNode = nodeTemp;
        }
        /// 
        /// <summary>
        /// 获取XML文件主节点
        /// </summary>
        /// <param name="filePath"></param>
        public XmlControl(string filePath)
        {
            //_FilePath = filePath;
            _XmlNode = Load(filePath);

        }

        ///// <summary>
        ///// 重载追加节点，在总节点下指定子节点进行追加
        ///// </summary>
        ///// <param name="queryString">查询节点表达式</param>
        ///// <param name="nodeName">节点名称</param>
        ///// <param name="arrList">变体带属性或节点值</param>
        ///// <returns></returns>
        //public XmlNode AppendChild(string queryString, string nodeName, params string[] arrList)
        //{
        //    if (_XmlNode == null)
        //    {
        //        _XmlNode = CreateXmlNode(nodeName, arrList);
        //        return _XmlNode;
        //    }
        //    else
        //    {
        //        XmlNode findNode = FindSencetion(_XmlNode, queryString);
        //        if (findNode == null)
        //            return null;

        //        return findNode.AppendChild(CreateXmlNode(nodeName, arrList));
        //    }
        //}


        ///// <summary>
        ///// 追加节点
        ///// </summary>
        ///// <param name="nodeItem">子节点对象</param>
        ///// <returns></returns>
        //public bool AppendChild(XmlNode nodeItem)
        //{
        //    if (_XmlNode == null) return false;
        //    //_XmlNode.PrependChild(nodeItem);
        //    //doc.ImportNode(nodeItem, true);
        //    _XmlNode.AppendChild(nodeItem);
        //    return true;
        //}

        /// <summary>
        /// 返回成XML节点对象
        /// </summary>
        /// <returns></returns>
        public XmlNode RootNode()
        {
            return _XmlNode;
        }

        ///// <summary>
        ///// 保存XML文档，覆盖打开的文档
        ///// </summary>
        ///// <returns></returns>
        //public bool SaveXml()
        //{
        //    if (_FilePath == "")
        //        return false;
        //    Save(_XmlNode, _FilePath);
        //    return true;
        //}
        ///// <summary>
        ///// 保存XML文档，另存为新的文档
        ///// </summary>
        ///// <param name="filePath">文档存储路径</param>
        ///// <returns></returns>
        //public bool SaveXml(string filePath)
        //{
        //    if (filePath == "")
        //        return false;

        //    Save(_XmlNode, filePath);
        //    return true;
        //}
        ///// <summary>
        ///// 删除一个子结点
        ///// </summary>
        ///// <param name="QueryString">XPath查询表达式</param>
        //public void RemoveChild(string queryString)
        //{
        //    _XmlNode = RemoveChildNode(_XmlNode, queryString);
        //}

        /// <summary>
        /// 返回子结点个数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {

            if (_XmlNode == null)
                return 0;
            return _XmlNode.ChildNodes.Count;
        }

        ///// <summary>
        ///// 修改查询到的节点的对应属性值
        ///// </summary>
        ///// <param name="queryString">XPath查询表达式，为空的时候表示查询主节点</param>
        ///// <param name="values">格式：属性名称|属性值</param>
        ///// <returns>修改成功或失败，失败就表示不存在该属性或节点</returns>
        //public bool EditAttibuteValue(string queryString, params string[] values)
        //{
        //    return EditNodeAttributeValues(queryString, _XmlNode, values);
        //}
        ///// <summary>
        ///// 获取主节点对应子节点多个并列属性值
        ///// </summary>
        ///// <param name="queryString">XPath查询表达式，为空的时候表示查询主节点</param>
        ///// <param name="arrList">并列的属性名称</param>
        ///// <returns>属性值</returns>
        //public string[] AttributeValue(string queryString, params string[] arrList)
        //{
        //    XmlNode node;
        //    string[] ret = new string[arrList.Length];
        //    if (queryString != "")
        //    {
        //        node = FindSencetion(_XmlNode, queryString);
        //        if (node == null) return ret;
        //    }
        //    else
        //        node = _XmlNode;
        //    for (int i = 0; i < ret.Length; i++)
        //    {
        //        ret[i] = GetNodeAttributeValue(node, arrList[i]);
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 修改节点的对应属性值 
        /// </summary>
        /// <param name="QuertString">XPath查询表达式，为空的时候表示当前节点</param>
        /// <param name="XmlInNode">需要修改属性的节点对象</param>
        /// <param name="Values">格式：属性名称|属性值</param>
        /// <returns>修改成功或失败，失败就表示不存在该属性或节点</returns>
        public static bool EditNodeAttributeValues(string QuertString, XmlNode XmlInNode, params string[] Values)
        {
            XmlNode node = FindSencetion(XmlInNode, QuertString);

            if (node == null) return false;

            for (int i = 0; i < Values.Length; i++)
            {
                string[] _Value = Values[i].Split('|');
                if (_Value.Length == 0) return false;

                XmlAttribute findAtt = node.Attributes[_Value[0]];
                if (findAtt == null)
                    return false;
                findAtt.Value = _Value[1];
            }
            return true;
        }

        /// <summary>
        /// 获取当前节点属性值
        /// </summary>
        /// <param name="XmlInNode">需要获取属性值的节点</param>
        /// <param name="AttributeName">对应节点属性名称</param>
        /// <returns></returns>
        public static string GetNodeAttributeValue(XmlNode XmlInNode, string AttributeName)
        {
            XmlAttribute findAtt = XmlInNode.Attributes[AttributeName];
            if (findAtt == null) return "";

            return XmlInNode.Attributes[AttributeName].Value;
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
        /// 保存XML文档
        /// </summary>
        /// <param name="SaveNode">需要保存的XML节点</param>
        /// <param name="SaveFilePath">存储路径</param>
        public static void Save(XmlNode SaveNode, string SaveFilePath)
        {
            System.IO.FileStream stream = Create(SaveFilePath);
            stream?.Close();

            XmlWriter writer = XmlWriter.Create(SaveFilePath);

            SaveNode.WriteTo(writer);          //写入文件
            writer.Flush();
            writer.Close();
        }

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
        /// 移除子结点
        /// </summary>
        /// <param name="XmlFatherNode">父域节点</param>
        /// <param name="QueryString">XPath查询表达式</param>
        /// <returns></returns>
        public static XmlNode RemoveChildNode(XmlNode XmlFatherNode, string QueryString)
        {
            if (QueryString == "")
                return XmlFatherNode;
            XmlNode node = XmlFatherNode.SelectSingleNode(QueryString);
            if (node == null)
                return XmlFatherNode;
            return RemoveChildNode(XmlFatherNode, node);

        }
        /// <summary>
        /// 移除子结点
        /// </summary>
        /// <param name="XmlFatherNode">父域节点</param>
        /// <param name="XmlDeleteNode">需要查询的子结点</param>
        /// <returns></returns>
        public static XmlNode RemoveChildNode(XmlNode XmlFatherNode, XmlNode XmlDeleteNode)
        {
            XmlFatherNode.RemoveChild(XmlDeleteNode);
            return XmlFatherNode;
        }

        /// <summary>
        /// 创建一个XML节点
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="attrArray">List单数为节点属性，双数为节点属性值，List长度为单数则具有节点值节点值</param>
        /// <returns></returns>
        public static XmlNode CreateXmlNode(string nodeName, params string[] attrArray)
        {
            //XmlDocument doc = new XmlDocument();
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, nodeName, "");

            if (attrArray.Length == 0) return newNode;

            if (attrArray.Length % 2 == 0)
            {
                for (int i = 0; i < attrArray.Length; i += 2)
                {
                    XmlAttribute attr = doc.CreateAttribute(attrArray[i]);
                    attr.Value = attrArray[i + 1].ToString();
                    newNode.Attributes.SetNamedItem(attr);
                }
            }
            else
            {
                for (int i = 0; i < attrArray.Length - 1; i += 2)
                {
                    XmlAttribute attr = doc.CreateAttribute(attrArray[i]);
                    attr.Value = attrArray[i + 1];
                    newNode.Attributes.SetNamedItem(attr);
                }
                newNode.AppendChild(doc.CreateCDataSection(attrArray[attrArray.Length - 1]));
            }
            return newNode;
        }

        /// <summary>
        /// 加载XML文档
        /// </summary>
        /// <param name="filePath">XML文档路径</param>
        /// <param name="msg">错误信息返回</param>
        /// <returns></returns>
        public static XmlNode Load(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return null;

            //doc = new XmlDocument();
            doc.RemoveAll();
            doc.Load(filePath);

            return doc.ChildNodes[1];
        }

        /// <summary>
        /// 返回XPath节点查询表达式
        /// </summary>
        /// <param name="xPathString">格式为1,1-1,1-2,...,1-n|2,2-1,...,2-n，1，2为节点名称，1-1(节点属性名),1-2(属性值)类推为节点属性名,属性值</param>
        /// <returns></returns>
        public static string XPath(string xPathString)
        {
            //string path = "";
            //string[] arr = xPathString.Split('|');   //分割获取需要查询嵌套几层的子节点

            //for (int i = 0; i < arr.Length; i++)
            //{
            //    string[] p = arr[i].Split(',');        //分割一个节点的表达式内容
            //    if (p.Length % 2 == 0) return "";     //如果长度是2的整数倍，则表达式有错误,直接返回空

            //    if (path == "")
            //        path += string.Format("{0}", p[0]);     //组合表达式
            //    else
            //        path += string.Format("//{0}", p[0]);   //组合表达式

            //    for (int j = 1; j < p.Length; j += 2)
            //    {
            //        path += string.Format("[@{0}='{1}']", p[j], p[j + 1]);   //组合表达式属性
            //    }
            //}
            //return path;

            string _XPath = "";
            string[] _TmpPathPram = xPathString.Split('|');   //分割获取需要查询嵌套几层的子节点

            for (int int_i = 0; int_i < _TmpPathPram.Length; int_i++)
            {
                string[] _TmpNodePramString = _TmpPathPram[int_i].Split(',');        //分割一个节点的表达式内容
                if (_TmpNodePramString.Length % 2 == 0)   //如果长度是2的整数倍，则表达式有错误
                    return "";          //直接返回空
                if (_XPath == "")
                {
                    _XPath += string.Format("//{0}", _TmpNodePramString[0]);                  //组合表达式
                }
                else
                    _XPath += string.Format("//{0}", _TmpNodePramString[0]);                  //组合表达式
                for (int int_j = 1; int_j < _TmpNodePramString.Length; int_j += 2)
                {
                    _XPath += string.Format("[@{0}='{1}']", _TmpNodePramString[int_j], _TmpNodePramString[int_j + 1]);   //组合表达式属性

                }
            }
            return _XPath;

        }


        /// <summary>
        /// 创建文件、如果目录不存在则自动创建、路径既可以是绝对路径也可以是相对路径
        /// 返回文件数据流，如果创建失败在返回null、如果文件存在则打开它
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream Create(string fileName)
        {
            fileName = GetPhyPath(fileName);
            string folder = fileName.Substring(0, fileName.LastIndexOf('\\') + 1);

            string tmpFolder = folder.Substring(0, fileName.IndexOf('\\')); //磁盘跟目录
                                                                            //逐层创建文件夹
            while (tmpFolder != folder)
            {
                tmpFolder = folder.Substring(0, fileName.IndexOf('\\', tmpFolder.Length) + 1);
                if (!Directory.Exists(tmpFolder))
                    Directory.CreateDirectory(tmpFolder);
            }


            if (System.IO.File.Exists(fileName))
            {
                return System.IO.File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
            }
            else
            {
                return System.IO.File.Create(fileName);

            }
        }


        /// <summary>
        /// 根据相对路径获取文件、文件夹绝对路径
        /// </summary>
        /// <param name="fileName">相对路径</param>   
        /// <returns></returns>
        public static string GetPhyPath(string fileName)
        {
            fileName = fileName.Replace('/', '\\');             //规范路径写法
            if (fileName.IndexOf(':') != -1) return fileName;   //已经是绝对路径了
            if (fileName.Length > 0 && fileName[0] == '\\') fileName = fileName.Substring(1);
            return string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), fileName);
        }
    }
}
