using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 多功能检定数据
    /// </summary>
    [Serializable()]
    public class MeterDgn : MeterBase
    {
        public MeterDgn() : this("")
        { }

        public MeterDgn(string priId)
            : base()
        {
            PrjID = priId;
            Name = "";
            Value = "";
            Result = "";
        }


        /// <summary>
        /// 多功能项目ID	
        /// </summary>
        public string PrjID { get; set; }
        /// <summary>
        /// 项目名称描述
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 项目值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 6结论Y/N
        /// </summary>
        public string Result { get; set; }


        /// <summary>
        /// 其他参数
        /// </summary>
        public string TestValue { get; set; }

        /// <summary>
        /// 误差值   误差1|误差2|.....|平均值|化整值|误差限
        /// </summary>
        public string WCData { get; set; }

        /// <summary>
        /// 误差1
        /// </summary>
        public string WC1 { get; set; }

        /// <summary>
        /// 误差2
        /// </summary>
        public string WC2 { get; set; }

        /// <summary>
        /// 误差3
        /// </summary>
        public string WC3 { get; set; }

        /// <summary>
        /// 误差4
        /// </summary>
        public string WC4 { get; set; }

        /// <summary>
        /// 误差5
        /// </summary>
        public string WC5 { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public string AvgValue { get; set; }

        /// <summary>
        /// 化整值
        /// </summary>
        public string HzValue { get; set; }

        /// <summary>
        /// 误差圈数
        /// </summary>
        public string WCRate { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string ResultValue { get; set; }

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
        /// 校时前时间差
        /// </summary>
        public string BeforeDateDifference { get; set; }

        /// <summary>
        /// 校时后时间差
        /// </summary>
        public string AfterDateDifference { get; set; }

        /// <summary>
        /// 误差限
        /// </summary>
        public string ErrorRate { get; set; }
    }
}
