namespace LYTest.DAL.DataBaseView
{
    /// 列名称显示模型
    /// <summary>
    /// 列名称显示模型
    /// </summary>
    public class ColumnDisplayModel
    {
        /// 列字段名
        /// <summary>
        /// 列字段名
        /// </summary>
        public string Field { get; set; }
        /// 列显示名称
        /// <summary>
        /// 列显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// 是否选中
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
