using System;
using System.Reflection;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 国金数据库表基类
    /// </summary>
    public abstract class M_QT_CONC_Basic
    {
        /// <summary>
        /// 基本信息标识
        /// </summary>
        public string BASIC_ID { get; set; }

        /// <summary>
        /// 质检编码
        /// </summary>
        public string ITEM_ID { get; set; }

        /// <summary>
        /// 检定任务单编号
        /// </summary>
        public string DETECT_TASK_NO { get; set; }

        /// <summary>
        /// 试品类别
        /// </summary>
        public string EXPET_CATEG { get; set; }

        /// <summary>
        /// 检定设备编号
        /// </summary>
        public string DETECT_EQUIP_NO { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string BAR_CODE { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public string PARA_INDEX { get; set; }

        /// <summary>
        /// 检定项
        /// </summary>
        public string TEST_CATEGORIES { get; set; }

        /// <summary>
        /// 检定项序号
        /// </summary>
        public string DETECT_ITEM_POINT { get; set; }


        /// <summary>
        /// 试验温度
        /// </summary>
        public string ENVIRON_TEMPER { get; set; }

        /// <summary>
        /// 试验相对湿度
        /// </summary>
        public string RELATIVE_HUM { get; set; }


        /// <summary>
        /// 测试日期
        /// </summary>
        public string VOLT_DATE { get; set; }

        /// <summary>
        /// 试验要求
        /// </summary>
        public string TEST_REQUIRE { get; set; }

        /// <summary>
        /// 试验条件
        /// </summary>
        public string TEST_CONDITION { get; set; }

        /// <summary>
        /// 写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }

        /// <summary>
        /// 平台处理标记
        /// </summary>
        public string HANDLE_FLAG { get; set; }

        /// <summary>
        /// 平台处理时间
        /// </summary>
        public string HANDLE_DATE { get; set; }


        /// <summary>
        /// 结论
        /// </summary>
        public string TEST_CONC { get; set; }

        /// <summary>
        /// 检验员
        /// </summary>
        public string TEST_USER_NAME { get; set; }

        /// <summary>
        /// 核验员
        /// </summary>
        public string AUDIT_USER_NAME { get; set; }

        public string ToInsertString()
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
                    if ((t.Name == "***" && p.Name == "****"))
                    {
                        s2 += string.Format("to_date('{0}','yyyy-MM-dd HH24:mi:ss')", v.ToString()) + ",";
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
