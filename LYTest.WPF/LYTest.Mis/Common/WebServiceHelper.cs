using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Services.Description;
using System.Xml;

namespace LYTest.Mis.Common
{
    public class HeaderBlock
    {
        public string Name;
        public string Content;
        public string Prefix;
        public string Namespace;
        public string Actor;
        public bool MustUnderstand;
        public override string ToString()
        {
            return Name;
        }
    }
    public static class WebServiceHelper
    {
        public static string[] RequestString;
        public static string[] ResponseString;
        /// <summary>
        /// 动态调用WebService
        /// </summary>
        /// <param name="url">WebService地址</param>
        /// <param name="methodname">方法名(模块名)</param>
        /// <param name="args">参数列表,无参数为null</param>
        /// <returns>object</returns>
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            try
            {
                return InvokeWebService(url, null, methodname, args);
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 动态调用WebService
        /// </summary>
        /// <param name="url">WebService地址</param>
        /// <param name="classname">类名</param>
        /// <param name="methodname">方法名(模块名)</param>
        /// <param name="args">参数列表</param>
        /// <returns>object</returns>
        public static object InvokeWebService(string urls, string classname, string methodname, object[] args)
        {
            string @namespace = "namespaceForMds";
            string url = urls.Split('?')[0];
            if (classname == null || classname == "")
            {
                classname = GetClassName(url);
            }
            //获取服务描述语言(WSDL)
            WebClient wc = new WebClient();
            Stream stream = wc.OpenRead(url + "?WSDL");//【1】
            ServiceDescription sd = ServiceDescription.Read(stream);//【2】
            CodeNamespace cn = new CodeNamespace(@namespace);//【4】

            //生成客户端代理类代码
            CodeCompileUnit ccu = new CodeCompileUnit();//【5】
            ccu.Namespaces.Add(cn);


            ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();//【3】
            sdi.AddServiceDescription(sd, "", "");
            sdi.Import(cn, ccu);

            //设定编译器的参数
            CompilerParameters cplist = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };//【8】
            cplist.ReferencedAssemblies.Add("System.dll");
            cplist.ReferencedAssemblies.Add("System.XML.dll");
            cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
            cplist.ReferencedAssemblies.Add("System.Data.dll");

            //编译代理类
            CSharpCodeProvider complier = new CSharpCodeProvider();
            CompilerResults cr = complier.CompileAssemblyFromDom(cplist, ccu);//【9】
            if (true == cr.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError ce in cr.Errors)
                {
                    sb.Append(ce.ToString());
                    sb.Append(Environment.NewLine);
                }
                throw new Exception(sb.ToString());
            }

            //生成代理实例,并调用方法
            System.Reflection.Assembly assembly = cr.CompiledAssembly;
            Type t = assembly.GetType(@namespace + "." + classname, true, true);
            object obj = Activator.CreateInstance(t);//【10】
            System.Reflection.MethodInfo mi = t.GetMethod(methodname);//【11】
            return mi.Invoke(obj, args);

        }
        private static string GetClassName(string url)
        {
            //假如URL为"http://localhost/InvokeService/Service1.asmx"
            //最终的返回值为 Service1
            string[] parts = url.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }

        /// <summary>
        /// 解析返回结果
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool GetResultByXml(string xml)
        {
            if (xml.ToLower() == "false") return false;
            if (string.IsNullOrEmpty(xml)) return false;
            if (xml.IndexOf("DATA") < 0) return false;

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml);
            string result = (xd.GetElementsByTagName("DATA").Item(0).FirstChild).InnerText.Trim();
            if (result != "1")
                return false;

            return true;
        }

        /// <summary>
        /// 解析返回结果
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetMessageByXml(string xml)
        {
            if (string.IsNullOrEmpty(xml) || xml.ToLower() == "false") return "无返回";

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml);
            string result = xd.GetElementsByTagName("DATA").Item(0).ChildNodes[1].InnerText.Trim();
            if (result != "1")
            {
                return result;
            }
            return "无返回信息";
        }


        public static string ExeMethod(string url, string Namespace, string Methodname, HeaderBlock[] Headers, string[] Params, string Tagname)
        {
            string action = "http://server.webservice.core.epm";
            if (Namespace != null && Namespace.Length > 0)
                action = Namespace;

            byte[] soap = BuildSoap(Namespace, Methodname, Params, Headers);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.ContentType = "text/xml;charset=utf-8";
            request.Headers.Add("SOAPAction", action + "/" + Methodname.Trim());
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentLength = soap.GetLength(0);

            Stream rs = request.GetRequestStream();
            rs.Write(soap, 0, (int)(request.ContentLength));
            rs.Close();

            RequestString = DispRequest(request, Namespace, Methodname, Params, Headers);
            bool err = false;

            HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();

            string body = GetResponseBody(httpResponse);
            ResponseString = DispResponse(httpResponse, body);
            return GetResult(Methodname, body, err, Tagname);
        }
        private static byte[] BuildSoap(string namespac, string methodName, string[] paras, HeaderBlock[] headers)
        {

            string upStr = "<?xml version='1.0' encoding='utf-8'?>" +
                "<soapenv:Envelope " +
                "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                "xmlns:xsd='http://www.w3.org/2001/XMLSchema' " +
                "xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?>" +
                "<soap:Envelope " +
                "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                "xmlns:xsd='http://www.w3.org/2001/XMLSchema' " +
                "xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>" +
                "</soap:Envelope>");
            //XmlElement envelope = doc.DocumentElement;

            if (headers.Length > 0)
            {
                int i = 0;
                string[] pams = new string[] { ":username", ":password" };
                upStr += "<soapenv:Header>";
                foreach (object obj in headers)
                {
                    i++;
                    string tmp = "ns" + i.ToString();
                    HeaderBlock hb = (HeaderBlock)obj;
                    upStr += "<" + tmp + pams[i - 1] + " soapenv:actor=\"http://schemas.xmlsoap.org/soap/actor/next\" "
                        + "soapenv:mustUnderstand=\"0\" xsi:type=\"soapenc:string\" xmlns:" + tmp + "=\"Authorization\" "
                        + "xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\">" + hb.Content + "</" + tmp + pams[i - 1] + ">";

                }
            }
            upStr += "</soapenv:Header>";
            if (methodName != null && methodName.Length > 0)
            {
                XmlElement body = doc.CreateElement("soapenv", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                XmlElement method = doc.CreateElement(methodName.Trim(), namespac);
                foreach (string p in paras)
                {
                    XmlElement elem = doc.CreateElement(p.Substring(0, p.IndexOf(":")), namespac);
                    elem.InnerText = p.Substring(p.IndexOf(":") + 1);
                    method.AppendChild(elem);
                }
                body.AppendChild(method);
                upStr += body.OuterXml;
            }
            upStr += "</soapenv:Envelope>";

            byte[] buf = new UTF8Encoding().GetBytes(upStr);

            doc.LoadXml(upStr);
            return buf;
        }
        private static string[] DispRequest(HttpWebRequest req, string namespac, string methodName, string[] paras, HeaderBlock[] headers)
        {
            string[] lines = new string[req.Headers.Count + 3];
            lines[0] = req.Method + " " + req.RequestUri + " HTTP/" + req.ProtocolVersion;
            for (int i = 0; i < req.Headers.Count; i++)
            {
                string key = req.Headers.Keys[i];
                string val = ":";
                foreach (string s in req.Headers.GetValues(key))
                    val += s + " ";

                lines[1 + i] = key + val;
            }
            lines[req.Headers.Count + 1] = "\n";
            byte[] soap = BuildSoap(namespac, methodName, paras, headers);
            lines[req.Headers.Count + 2] =
                (new UTF8Encoding()).GetString(soap);

            return lines;
        }
        private static string GetResponseBody(HttpWebResponse res)
        {
            Stream stm = res.GetResponseStream();
            StreamReader reader = new StreamReader(stm);
            string body = reader.ReadToEnd();
            reader.Close();
            res.Close();
            return body;
        }
        private static string[] DispResponse(HttpWebResponse res, string body)
        {
            string[] lines = new string[res.Headers.Count + 3];
            lines[0] = " HTTP/" + res.ProtocolVersion + " " + (int)res.StatusCode + " " + res.StatusDescription;
            string sKey;
            for (int i = 0; i < res.Headers.Count; i++)
            {
                sKey = res.Headers.Keys[i];
                string sValues = ":";
                foreach (string s in res.Headers.GetValues(sKey))
                    sValues += s + " ";
                lines[1 + i] = sKey + sValues;
            }
            lines[res.Headers.Count + 1] = "\n";
            lines[res.Headers.Count + 2] = body;
            return lines;
        }
        private static string GetResult(string Methodname, string body, bool err, string tagname)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(body);
            if (!err)
            {
                string tag = Methodname.Trim() + "Response";
                if (tagname.Trim().Length > 0)
                    tag = tagname;

                XmlNodeList nodes = doc.GetElementsByTagName(tag);
                return (nodes[0].ChildNodes[0].InnerText);
            }
            else
            {
                XmlNodeList fault = doc.GetElementsByTagName("Fault");
                if (fault == null)
                    return "HTTP 错误";
                string msg = "SOAP错误(" +
                    doc.GetElementsByTagName("faultstring")[0].InnerText + ")";
                return msg;
            }
        }

        /// <summary>
        /// 登陆验证   陕西地电
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public static bool CheckLogin(String strXml)
        {
            if (string.IsNullOrEmpty(strXml)) return false;

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(strXml);
            int result = Convert.ToInt16(xd.GetElementsByTagName("PROCESSCODE").Item(0).ChildNodes[0].InnerText.Trim());
            if (result < 0)
                return false;

            return true;
        }

        /// <summary>
        /// 登陆验证2
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string CheckLogin2(String xml)
        {
            if (string.IsNullOrEmpty(xml)) return "无返回";

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml);
            return xd.GetElementsByTagName("PROCESSMSG").Item(0).ChildNodes[1].InnerText.Trim();
        }

        /// <summary>
        /// 读取身份校验码
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string GetXmlValue(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return "无返回";

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml);
            string result = xd.GetElementsByTagName("IEDNTITYCODE").Item(0).ChildNodes[0].InnerText.Trim();
            if (result != "")
                return result;

            return "无返回信息";
        }
    }
}
