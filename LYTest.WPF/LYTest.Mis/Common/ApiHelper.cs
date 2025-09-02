using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.Common
{
    public class ApiHelper
    {
        public static string HttpApi(string url, string jsonstr, string type, int MaxTime, out string msg)
        {
            //http://127.0.0.1:5000/api/Main/MDSToMeter?json=0
            try
            {
                msg = "";
                System.Net.ServicePointManager.Expect100Continue = false;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址
                request.Proxy = null;
                request.Timeout = MaxTime;//20秒超时时间
                request.Accept = "text/html,application/xhtml+xml,*/*";
                request.ContentType = "application/json";
                request.Method = type.ToUpper().ToString();//get或者post
                byte[] buffer = Encoding.UTF8.GetBytes(jsonstr);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("调用WebApi接口失败：" + ex.ToString());
                msg = "调用WebApi接口失败:" + ex.Message;
                return "";
            }
        }
    }
}
