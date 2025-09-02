using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace LYTest.Mis.Common
{
    public class WebService
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

        private readonly string Url = "";
        private readonly string Namespace = "";

        //public string[] RequestString;
        public string[] ResponseString;
        public WebService(string url, string namespac)
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
            Url = url;
            Namespace = namespac;
        }
        public string ExeMethod(string methodName, HeaderBlock[] Headers, string[] Params, string Tagname)
        {
            //Uri theURL = new Uri(Url);
            string action = string.IsNullOrEmpty(Namespace) ? "http://server.webservice.core.epm" : Namespace;
            byte[] soap = BuildSoap(methodName, Params, Headers);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.ContentType = "text/xml;charset=utf-8";
            req.Headers.Add("SOAPAction", action + "/" + methodName.Trim());
            req.Method = "POST";
            req.KeepAlive = false;
            req.ContentLength = soap.GetLength(0);

            Stream rs = req.GetRequestStream();
            rs.Write(soap, 0, (int)req.ContentLength);
            rs.Close();

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            string body = GetResponseBody(res);
            ResponseString = DispResponse(res, body);
            bool err = false;
            return GetResult(methodName, body, err, Tagname);
        }
        private byte[] BuildSoap(string methodName, string[] paras, HeaderBlock[] headers)
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

            //登录账号
            int i = 0;
            string[] pams = new string[] { ":username", ":password" };
            upStr += "<soapenv:Header>";
            foreach (object obj in headers)
            {
                i++;
                string tmp = "ns" + i.ToString();
                HeaderBlock hb = (HeaderBlock)obj;
                upStr += string.Format("<ns{0}{1} soapenv:actor=\"http://schemas.xmlsoap.org/soap/actor/next\" "
                    + "soapenv:mustUnderstand=\"0\" xsi:type=\"soapenc:string\" xmlns:ns{2}=\"Authorization\" "
                    + "xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\">{3}</ns{4}{5}>",
                    i, pams[i - 1], i, hb.Content, i, pams[i - 1]);

            }
            upStr += "</soapenv:Header>";


            if (methodName != null && methodName.Length > 0)
            {
                XmlElement body = doc.CreateElement("soapenv", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                XmlElement method = doc.CreateElement(methodName.Trim(), Namespace);
                foreach (string p in paras)
                {
                    XmlElement elem = doc.CreateElement(p.Substring(0, p.IndexOf(":")), Namespace);
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

        private string GetResponseBody(HttpWebResponse res)
        {
            Stream stm = res.GetResponseStream();
            StreamReader reader = new StreamReader(stm);
            string body = reader.ReadToEnd();
            reader.Close();
            res.Close();
            return body;
        }
        private string[] DispResponse(HttpWebResponse res, string body)
        {
            string[] lines = new string[res.Headers.Count + 3];
            lines[0] = " HTTP/" + res.ProtocolVersion + " " +
                (int)res.StatusCode + " " +
                res.StatusDescription;
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
        private string GetResult(string Methodname, string body, bool err, string tagname)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(body);
            //doc.Save(@"e:\body.xml");
            if (!err)
            {
                string tagnamedefault = Methodname.Trim() + "Response";
                if (tagname.Trim().Length > 0)
                    tagnamedefault = tagname;

                XmlNodeList nodes = doc.GetElementsByTagName(tagnamedefault);
                return nodes[0].ChildNodes[0].InnerText;
            }
            else
            {
                XmlNodeList fault = doc.GetElementsByTagName("Fault");
                if (fault == null)
                    return "HTTP 错误";
                string msg = "SOAP错误(" + doc.GetElementsByTagName("faultstring")[0].InnerText + ")";
                return msg;
            }
        }
    }
}
