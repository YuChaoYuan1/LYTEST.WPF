using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 智能表功能检定数据
    /// </summary>
    [Serializable()]
    public class MeterFunction : MeterBase
    {

        public MeterFunction() : this("")
        {

        }
        public MeterFunction(string priId)
            : base()
        {
            PrjID = priId;
            Name = "";
            Value = "";
            Result = "";
        }

        ///// <summary>
        ///// 表唯一ID号	
        ///// </summary>
        //public long Id { get; set; }
        /// <summary>
        /// 智能表功能项目ID	
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
