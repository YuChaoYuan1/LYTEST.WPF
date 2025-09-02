//add yjt 20220306 新增默认合格的检定数据 如如外观检查，工频耐压试验，显示功能
namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 默认合格
    /// </summary>
    public class MeterDefault : MeterBase
    {
        public MeterDefault() : base()
        {
            DefaultKey = "";
            DefaultName = "";
            Result = "";
            Value = "";
        }

        /// <summary>
        /// 1项目ID
        /// </summary>
        public string DefaultKey { get; set; }

        /// <summary>
        /// 2项目名称名称
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// 3结论[--, 不合格, 合格]
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 4数据
        /// </summary>
        public string Value { get; set; }
    }
}
