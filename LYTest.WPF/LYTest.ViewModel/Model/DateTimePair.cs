using System;

namespace LYTest.ViewModel.Model
{
    /// <summary>
    /// 电表时间和本地时间对
    /// </summary>
    public class DateTimePair
    {
        public DateTime MeterTime;
        public DateTime LocalTime = DateTime.Now;
        /// <summary>
        /// 间隔：电表时间-本地时间
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                if (MeterTime == DateTime.MinValue) return new TimeSpan(0, 0, 999);
                return MeterTime - LocalTime;
            }
        }
    }
}
