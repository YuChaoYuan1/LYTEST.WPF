namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 耐压检定数据
    /// </summary>
    public class MeterInsulation : MeterBase
    {
        /// <summary>
        /// 1项目ID
        /// </summary>
        public string DefaultKey { get; set; }

        /// <summary>
        /// 2项目名称名称
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// 耐压试验类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 耐压值
        /// </summary>
        public int Voltage { get; set; }
        /// <summary>
        /// 耐压时间
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// 已测试时间
        /// </summary>
        public int TestTime { get; set; }
        /// <summary>
        /// 表位漏电流（mA）
        /// </summary>
        public string CurrentLost { get; set; }
        /// <summary>
        /// 存储读取到的多个漏电流值
        /// </summary>
        public string CurrentLosts { get; set; }
        /// <summary>
        /// 当前最大值
        /// </summary>
        public string CurrentMax { get; set; }
        /// <summary>
        /// 检定结论 合格/不合格
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 不合格原因
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 格式化的总数据
        /// </summary>
        public string Value { get; set; }
    }
}
