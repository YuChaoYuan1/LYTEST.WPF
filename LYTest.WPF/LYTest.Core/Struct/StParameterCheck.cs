namespace LYTest.Core.Struct
{
    /// <summary>
    /// 参数验证信息
    /// </summary>
    public struct StParameterCheck
    {
        /// <summary>
        /// 参数验证子项项目Id
        /// </summary>
        public string DATA_ID { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string DATA_NAME { get; set; }
        /// <summary>
        /// 数据类型，1：一类数据；2：二类数据；3：三类数据；
        /// </summary>
        public string DATA_TYPE { get; set; }
        /// <summary>
        /// 数据格式
        /// </summary>
        public string DATA_FORMAT { get; set; }
        /// <summary>
        /// 数据标识
        /// </summary>
        public string DATA_TAG { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string DATA_UNIT { get; set; }
        /// <summary>
        /// 截取模式，针对一类数据块
        /// </summary>
        public string DATA_INTERCEPT { get; set; }
        /// <summary>
        /// 数据操作类型，读或写
        /// </summary>
        public string DATA_ISREAD { get; set; }
        /// <summary>
        /// 标准数据，待比对或设置的数据
        /// </summary>
        public string DATA_STANDARD { get; set; }
        /// <summary>
        /// ITEM_ID 
        /// </summary>
        public int ITEM_ID { get; set; }
        /// <summary>
        /// 参数验证子项顺序
        /// </summary>
        public int PARAM_ORDER { get; set; }
    }
}
