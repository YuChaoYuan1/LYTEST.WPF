using System;

namespace LYTest.Core.Function
{
    /// <summary>
    /// 时间相关操作
    /// </summary>
    public class DateTimes
    {
        ///// <summary>
        ///// 线程内延时
        ///// </summary>
        ///// <param name="MSec">单位：MS</param>
        //public static void Delay(int MSec)
        //{
        //    DateTime startTime = DateTime.Now;
        //    long pastTime = 0;
        //    MSec *= 10000;
        //    while (pastTime < MSec)
        //    {
        //        pastTime = DateTime.Now.Ticks - startTime.Ticks;

        //    }

        //}

        /// <summary>
        /// 格式化字符串为日期
        /// </summary>
        /// <param name="FormatDate">要格式化的字符串,如:20090909000000</param>
        /// <returns></returns>
        public static DateTime FormatStringToDateTime(string FormatDate)
        {
            string _year;
            string _month;
            string _day;
            string _hour;
            string _minute;
            string _second;

            if (FormatDate.Length >= 10 && FormatDate.Contains("0000000000"))
            {
                FormatDate = DateTime.Now.ToString("yyMMddHHmmss");
                _year = FormatDate.Substring(0, 2);
                _month = FormatDate.Substring(2, 2);
                _day = FormatDate.Substring(4, 2);
                _hour = FormatDate.Substring(6, 2);
                _minute = FormatDate.Substring(8, 2);
                _second = FormatDate.Substring(10, 2);
                return DateTime.Parse(string.Format("{0}-{1}-{2} {3}:{4}:{5}", _year, _month, _day, _hour, _minute, _second));

            }

            if (FormatDate.Length == 12)
            {
                _year = DateTime.Now.Year.ToString().Substring(0, 2) + FormatDate.Substring(0, 2);
                _month = FormatDate.Substring(2, 2);
                _day = FormatDate.Substring(4, 2);
                _hour = FormatDate.Substring(6, 2);
                _minute = FormatDate.Substring(8, 2);
                _second = FormatDate.Substring(10, 2);
                return DateTime.Parse(string.Format("{0}-{1}-{2} {3}:{4}:{5}", _year, _month, _day, _hour, _minute, _second));
            }
            else if (FormatDate.Length == 14)
            {
                _year = FormatDate.Substring(0, 4);
                _month = FormatDate.Substring(4, 2);
                _day = FormatDate.Substring(6, 2);
                _hour = FormatDate.Substring(8, 2);
                _minute = FormatDate.Substring(10, 2);
                _second = FormatDate.Substring(12, 2);
                return DateTime.Parse(string.Format("{0}-{1}-{2} {3}:{4}:{5}", _year, _month, _day, _hour, _minute, _second));

            }
            else if (FormatDate.Length == 10)
            {
                _year = DateTime.Now.Year.ToString().Substring(0, 2) + FormatDate.Substring(0, 2);
                _month = FormatDate.Substring(2, 2);
                _day = FormatDate.Substring(4, 2);
                _hour = FormatDate.Substring(6, 2);
                _minute = FormatDate.Substring(8, 2);
                _second = "59";
                return DateTime.Parse(string.Format("{0}-{1}-{2} {3}:{4}:{5}", _year, _month, _day, _hour, _minute, _second));

            }
            else
            {
                return DateTime.Now;
            }
        }


        ///// <summary>
        ///// 检测字符串是否为日期类型
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //public static bool IsDate(string str)
        //{
        //    bool isDate = false;
        //    try
        //    {
        //        DateTime d = DateTime.Parse(str);
        //        isDate = true;
        //    }
        //    catch
        //    {
        //        isDate = false;
        //    }
        //    return isDate;
        //}

        /// <summary>
        /// 计算一个时间与当前时间的绝对值差(单位：秒),
        /// </summary>
        /// <param name="CompareDate">要比较的日期</param>
        /// <returns>时间(单位：秒)</returns>
        public static double DateDiff(DateTime CompareDate)
        {
            DateTime dtNow = DateTime.Now;
            return CompareDate.Subtract(dtNow).Duration().TotalSeconds;
        }

        ///// <summary>
        ///// 将指定时间(秒)序列成时-分-秒格式
        ///// </summary>
        ///// <param name="Seconds">要序列化的时间(秒)</param>
        ///// <returns>返回Struct.stTime结构体，内包含时，分，秒</returns>
        //public static Struct.StTime TimeSerial(int Seconds)
        //{
        //    Struct.StTime ST = new Struct.StTime();
        //    ST.Hour = (int)(Seconds / 3600);
        //    ST.Minute = (int)((Seconds - ST.Hour * 3600) / 60);
        //    ST.Seconds = Seconds % 60;
        //    return ST;
        //}
        ///// <summary>
        ///// 获得当前时间的4字节时间戳
        ///// </summary>
        ///// <returns></returns>
        //public static int GetTimeStamp()
        //{
        //    DateTime timeStamp = new DateTime(1970, 1, 1); //得到1970年的时间戳 
        //    long a = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000; //注意这里有时区问题，用now就要减掉8个小时
        //    int b = (int)a;
        //    return b;
        //}
        ///// <summary>
        ///// 将4字节时间戳转换为时间
        ///// </summary>
        ///// <param name="Stamp"></param>
        ///// <returns></returns>
        //public static DateTime getTimeByStamp(int Stamp)
        //{
        //    DateTime timeStamp = new DateTime(1970, 1, 1);
        //    long d = Stamp * 10000000 + timeStamp.Ticks;
        //    DateTime e = new DateTime(d);
        //    return e;
        //}
    }
}
