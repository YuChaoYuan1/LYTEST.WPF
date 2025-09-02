using System.Collections.Generic;

namespace LYTest.DAL.DataBaseView
{
    /// 非主结论数据的显示
    /// 格式为Key,字段名,显示名1|显示名2|显示名|3...
    /// <summary>
    /// 非主结论数据的显示
    /// </summary>
    public class FKDisplayConfigModel
    {
        /// <summary>
        /// 详细信息数据Key值
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 详细信息字段名称
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// 字段显示出来的名称
        /// </summary>
        public List<string> DisplayNames { get; set; }
        /// <summary>
        /// 将Model转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string modelString = string.Format("{0},{1},", Key, Field);
            modelString += string.Join("|", DisplayNames);
            return modelString.TrimEnd('|');
        }

    }
}
