using System.Collections.Generic;

namespace LYTest.Core.Model
{
    public class RatePeriodTable
    {
        /// <summary>
        /// 年时区数
        /// </summary>
        public string time_zone_num;
        /// <summary>
        /// 日时段数
        /// </summary>
        public string day_period_num;
        /// <summary>
        /// 日时段表数
        /// </summary>
        public string daytime_periodtab_num;
        /// <summary>
        /// 年时区表
        /// </summary>
        public Dictionary<string, List<string>> TimeZoneTable = new Dictionary<string, List<string>>();
        /// <summary>
        /// 日时段表
        /// </summary>
        public Dictionary<string, Dictionary<string, List<string>>> PeriodTable = new Dictionary<string, Dictionary<string, List<string>>>();
    }
}
