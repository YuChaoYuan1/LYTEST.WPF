namespace LYTest.Core.Model.DnbModel
{
    /// <summary>
    /// 检定原始数据
    /// </summary>
    public class MeterTestData
    {
        /// <summary>
        /// 项目序号
        /// </summary>
        public int index;

        /// <summary>
        /// 项目编号
        /// </summary>
        public string ItemNo;
        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name = "";

        /// <summary>
        /// 检定参数名称，"|"分割
        /// </summary>
        public string ParameterNames = "";
        /// <summary>
        /// 检定参数值
        /// </summary>
        public string ParameterValues = "";

        /// <summary>
        /// 结论名称
        /// </summary>
        public string ResultNames = "";
        /// <summary>
        /// 结论值
        /// </summary>
        public string ResultValues = "";

        /// <summary>
        /// 结论
        /// </summary>
        public string Result;
    }
}
