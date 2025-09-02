using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 负荷记录检定数据
    /// </summary>
    [Serializable()]
    public class MeterLoadRecord : MeterBase
    {
        /// <summary>
        /// 负荷记录项目ID	
        /// </summary>
        public string PrjID { get; set; }
        /// <summary>
        /// 项目名称描述
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 子项试验名称
        /// </summary>
        public string SubName { get; set; }
        /// <summary>
        /// 项目值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 方案信息
        /// </summary>
        public string TestValue { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }
    }
}
