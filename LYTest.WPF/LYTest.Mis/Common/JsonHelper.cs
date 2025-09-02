using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.Common
{
    /// <summary>
    /// 序列化
    /// </summary>
    public class JsonHelper
    {
        public static string 序列化对象(object obj)
        {

            return JsonConvert.SerializeObject(obj);
        }

        public static T 反序列化字符串<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);  // 尖括号<>中填入对象的类名
        }
    }
}
