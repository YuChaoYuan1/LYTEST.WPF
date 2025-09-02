using System;
using System.Reflection;

namespace LYTest.Mis.HeBei20.Tables
{
    public abstract class MT_MET_CONC_Base
    {
        #region 属性
        /// <summary>
        /// 检定任务编号
        /// </summary>
        public string DETECT_TASK_NO { get; set; }

        /// <summary>
        /// 主要区分单相、三相电能表
        /// </summary>
        public string EQUIP_CATEG { get; set; }

        /// <summary>
        /// 线体编号、机台编号
        /// </summary>
        public string SYS_NO { get; set; }


        /// <summary>
        /// 检定线台编号、单元编号
        /// </summary>
        public string DETECT_EQUIP_NO { get; set; }

        /// <summary>
        /// 检定表位编号
        /// </summary>
        public string DETECT_UNIT_NO { get; set; }

        /// <summary>
        /// 表位编号
        /// </summary>
        public string POSITION_NO { get; set; }

        /// <summary>
        /// 电能表条形码
        /// </summary>
        public string BAR_CODE { get; set; }

        /// <summary>
        /// 检定时间
        /// </summary>
        public string DETECT_DATE { get; set; }

        /// <summary>
        /// 第几次检定，设备可以检定多次
        /// </summary>
        public string PARA_INDEX { get; set; }

        /// <summary>
        /// 检定点的序号，每个检定项可以检定多个点
        /// </summary>
        public string DETECT_ITEM_POINT { get; set; }

        /// <summary>
        /// 结论 1-合格，2-不合格
        /// </summary>
        public string CONC_CODE { get; set; }

        /// <summary>
        /// 检定线写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }

        /// <summary>
        /// 0-未处理（默认）；1-处理中；2-已处理3
        /// </summary>
        public string HANDLE_FLAG { get; set; }

        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }
        #endregion

        #region 公共函数
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
                    if ((t.Name == "MT_CONTROL_MET_CONC" && p.Name == "DETECT_DATE")
                     //|| (t.Name == "MT_DETECT_RSLT" && p.Name == "WRITE_DATE")//济南
                     )  
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

        #endregion
    }
}
