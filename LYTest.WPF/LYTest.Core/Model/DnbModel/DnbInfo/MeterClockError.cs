using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 时钟示值误差
    /// </summary>
    [Serializable()]
    public class MeterClockError : MeterBase
    {
        /// <summary>
        /// 表当前日期
        /// </summary>
        public string MeterCurrentReadDate { get; set; }

        /// <summary>
        /// 系统当前日期
        /// </summary>
        public string SystemCurrentDate { get; set; }

        /// <summary>
        /// 日期差
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 表当前时间
        /// </summary>
        public string MeterCurrentReadTime { get; set; }

        /// <summary>
        /// 系统当前时间
        /// </summary>
        public string SystemCurrentTime { get; set; }

        /// <summary>
        /// 时间差
        /// </summary>
        public string TimeDifference { get; set; }

        /// <summary>
        /// 校时后电表时间
        /// </summary>
        public string CheckedMeterTime { get; set; }
        /// <summary>
        /// 校时后系统当前时间
        /// </summary>
        public string CheckedSystemCurrentTime { get; set; }
        /// <summary>
        /// 校时后时间差
        /// </summary>
        public string CheckedTimeDifference { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }

    }
}
