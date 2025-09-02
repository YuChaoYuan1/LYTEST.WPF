using System.Xml.Serialization;

namespace LYTest.MeterProtocol.DataFlag
{
    /// <summary>
    /// 数据标识模型
    /// </summary>
    public class DI
    {
        /// <summary>
        /// 数据标识名称
        /// </summary>
        [XmlAttribute("DataFlagDiName")]
        public string DataFlagDiName { get; set; }

        /// <summary>
        /// 数据标识645
        /// </summary>
        [XmlAttribute("DataFlagDi")]
        public string DataFlagDi { get; set; }
        /// <summary>
        ///数据标识698
        /// </summary>
        [XmlAttribute("DataFlagOi")]
        public string DataFlagOi { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        [XmlAttribute("DataLength")]
        public string DataLength { get; set; }
        /// <summary>
        /// 小数位数
        /// </summary>
        [XmlAttribute("DataSmallNumber")]
        public string DataSmallNumber { get; set; }
        /// <summary>
        /// 数据格式
        /// </summary>
        [XmlAttribute("DataFormat")]
        public string DataFormat { get; set; }
        /// <summary>
        /// 所属类名称
        /// </summary>
        [XmlAttribute("ClassName")]
        public string ClassName { get; set; }
        /// <summary>
        /// 权限两位，第一位代表写权限(10)，第二位代表读权限(01)，11代表可读可写
        /// </summary>
        [XmlAttribute("Rights")]
        public string Rights { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        [XmlAttribute("SortNo")]
        public string SortNo { get; set; }
        /// <summary>
        /// 安全模式
        /// </summary>
        [XmlAttribute("EmSecurityMode")]
        public string EmSecurityMode { get; set; }
        /// <summary>
        /// 所属的芯片，0管理芯片,1计量芯片，2-??其他扩展芯片
        /// </summary>
        [XmlAttribute("Chip")]
        public string Chip { get; set; }
    }
}
