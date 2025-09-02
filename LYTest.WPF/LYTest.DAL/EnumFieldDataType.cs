namespace LYTest.DAL
{
    /// 数据库字段类型枚举
    /// 由于access数据库对于数据插入格式的种种限制
    /// 在自动生成sql语句时需要判断字段数据类型以后才能生成sql语句
    /// <summary>
    /// 数据库字段类型枚举
    /// 由于access数据库对于数据插入格式的种种限制
    /// 在自动生成sql语句时需要判断字段数据类型以后才能生成sql语句
    /// </summary>
    public  enum EnumFieldDataType
    {
        字符串,
        数值,
        时间
    }
}
