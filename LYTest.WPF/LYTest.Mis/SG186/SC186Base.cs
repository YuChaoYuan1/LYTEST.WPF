using System;
using System.Reflection;

namespace LYTest.Mis.SG186
{
    public abstract class SC186Base
    {
        public string ToSQL()
        {
            string s1 = "";
            string s2 = "";
            Type t = GetType();
            PropertyInfo[] propertys = t.GetProperties();
            foreach (PropertyInfo p in propertys)
            {
                object v = p.GetValue(this, null);
                if (v != null)
                {
                    if (string.IsNullOrEmpty(v.ToString())) continue;
                    s1 += "" + p.Name + ",";

                    if (p.PropertyType == typeof(DateTime))
                    {
                        DateTime dt = (DateTime)v;
                        s2 += string.Format("to_date('{0}','yyyy-MM-dd HH24:mi:ss')", dt.ToString("yyyy-MM-dd HH:mm:ss")) + ",";
                    }
                    else
                    {
                        s2 += "'" + v.ToString() + "',";
                    }
                }
            }
            s1 = s1.TrimEnd(',');
            s2 = s2.TrimEnd(',');

            return string.Format("INSERT INTO {0}({1}) VALUES ({2})", t.Name, s1, s2);

        }
    }
}
