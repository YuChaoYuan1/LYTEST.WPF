using System;
using System.Reflection;

namespace LYTest.Mis.NanRui.Tables
{
    /// <summary>
    /// 芯片ID认证信息表
    /// </summary>
    public class MT_HPLCID_CERT_INFO
    {
        /// <summary>
        /// 检定任务单号
        /// </summary>
        public string DETECT_TASK_NO { get; set; }
        /// <summary>
        /// 本次通知序列号
        /// </summary>
        public string WEB_NOTICE_NO { get; set; }
        /// <summary>
        /// 系统编号
        /// </summary>
        public string SYS_NO { get; set; }
        /// <summary>
        /// 设备条码
        /// </summary>
        public string BAR_CODE { get; set; }
        /// <summary>
        /// 通信模块类别
        /// </summary>
        public string MODULE_TYPE_CODE { get; set; }
        /// <summary>
        /// 通信模块条码
        /// </summary>
        public string MODULE_BAR_CODE { get; set; }
        /// <summary>
        /// 芯片ID
        /// </summary>
        public string HPLCID { get; set; }
        /// <summary>
        /// 是否合法，1-合法，0-不合法，此字段由MDS认证后回填
        /// </summary>
        public string IS_LEGAL { get; set; }
        /// <summary>
        /// 认证时间，MDS认证时间，此字段由MDS认证后回填
        /// </summary>
        public string CERT_DATE { get; set; }
        /// <summary>
        /// 检定线写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// <summary>
        /// 处理标记
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// 处理时间
        /// </summary>
        public string HANDLE_DATE { get; set; }

        public string ToInsertString()
        {
            string s1 = "";
            string s2 = "";
            Type t = GetType();
            PropertyInfo[] propertys = t.GetProperties();
            foreach (PropertyInfo p in propertys)
            {
                object v = p.GetValue(this, null);
                if (v == null) continue;
                if (string.IsNullOrEmpty(v.ToString())) continue;

                s1 += "" + p.Name + ",";
                s2 += "'" + v.ToString() + "',";
            }
            s1 = s1.TrimEnd(',');
            s2 = s2.TrimEnd(',');

            return string.Format("INSERT INTO {0}({1}) VALUES ({2})", t.Name, s1, s2);

        }

    }
}
