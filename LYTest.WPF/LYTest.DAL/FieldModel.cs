namespace LYTest.DAL
{
    /// 数据库字段模型
    /// 由于access数据库对于数据插入格式的种种限制
    /// 在自动生成sql语句时需要判断字段数据类型以后才能生成sql语句
    /// <summary>
    /// 数据库字段模型
    /// 由于access数据库对于数据插入格式的种种限制
    /// 在自动生成sql语句时需要判断字段数据类型以后才能生成sql语句
    /// </summary>
    public class FieldModel
    {
        /// 字段名称
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get;private set; }
        /// 字段数据类型
        /// <summary>
        /// 字段数据类型
        /// </summary>
        public EnumFieldDataType DataType { get;private set; }
        /// 字段名称
        /// <summary>
        /// 字段的数据类型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        public FieldModel(string name, EnumFieldDataType dataType)
        {
            FieldName=name;
            DataType = dataType;
        }
    }
}
