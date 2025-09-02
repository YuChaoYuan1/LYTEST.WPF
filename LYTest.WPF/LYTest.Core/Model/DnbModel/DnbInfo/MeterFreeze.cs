using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 冻结检定数据
    /// </summary>
    [Serializable()]
    public class MeterFreeze : MeterBase
    {
        public MeterFreeze() : this("") { }

        public MeterFreeze(string prjId) : base()
        {
            PrjID = prjId;
            Name = "";
            Value = "";
            TestValue = "";
        }

        /// <summary>
        /// 冻结项目ID	
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
        /// 方案信息
        /// </summary>
        public string TestValue { get; set; }
        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }
    }
}
