namespace LYTest.Core.Struct
{
    /// <summary>
    /// 误差限
    /// </summary>
    public class ErrorLimit
    {
        /// <summary>
        /// 误差限
        /// </summary>
        public ErrorLimit()
        {
            MeterLevel = 2.0f;
            UpLimit = 2.0f;
            DownLimit = 2.0f;
            IsSTM = false;
        }
        /// <summary>
        /// 表等级
        /// </summary>
        public float MeterLevel { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public float UpLimit { get; set; }

        /// <summary>
        /// 误差下限
        /// </summary>
        public float DownLimit { get; set; }

        /// <summary>
        /// 是否是标准表
        /// </summary>
        public bool IsSTM { get; set; }
    }
}
