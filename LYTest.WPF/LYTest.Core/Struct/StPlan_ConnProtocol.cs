using System;

namespace LYTest.Core.Struct
{

    /// <summary>
    /// 通讯协议检查试验
    /// </summary>
    [Serializable()]
    public class StPlan_ConnProtocol
    {

        /// <summary>
        /// 项目编号
        /// </summary>
        public string PrjID { get; set; }

        /// <summary>
        /// 数据项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 645数据标识
        /// </summary>
        public string Code645 { get; set; }

        /// <summary>
        /// 698数据标识
        /// </summary>
        public string Code698 { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int DataLen { get; set; }

        /// <summary>
        /// 小数位索引
        /// </summary>
        public int PointIndex { get; set; }

        /// <summary>
        /// 数据格式
        /// </summary>
        public string StrDataType { get; set; }

        /// <summary>
        /// 操作类型,读/写
        /// </summary>
        public string OperType { get; set; }

        /// <summary>
        /// 写入内容
        /// </summary>
        public string WriteContent { get; set; }

        /// <summary>
        /// 子项目编号
        /// </summary>
        public string ChildrenItemId { get; set; }

        /// <summary>
        /// 通讯协议检查试验项目描述
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Name == null) Name = "";
            return string.Format("通讯协议检查试验：({0}){1}", OperType, Name.ToString());
        }
    }
}
