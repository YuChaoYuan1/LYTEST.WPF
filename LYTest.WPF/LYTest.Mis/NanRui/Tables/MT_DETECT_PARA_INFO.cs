namespace LYTest.Mis.NanRui.Tables
{
    //大项标识为-1时，指明改行数据标识的是一个大项数据，如“基本误差”、“外观检查试验”等，否则 改行数据标识的是“大项标识”所表示的参数项，如“基本误差”的“功率因数”、“电流负载”等，参见“检定项及分项”
    public class MT_DETECT_PARA_INFO
    {
        /// <summary>
        /// 检定项参数信息实体记录的唯一性标识
        /// </summary>
        public string PARA_NO { get; set; }

        /// <summary>
        /// 本表的参数标识，“检定项参数编码信息表”的大项参数标识
        /// </summary>
        public string P_PARA_NO { get; set; }

        /// <summary>
        /// A8      小项参数顺序号
        /// </summary>
        public string PARA_INDEX { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string PARA_NAME { get; set; }

        /// <summary>
        /// 小项的默认值
        /// </summary>
        public string PARA_DEFAULT_VALUE { get; set; }

        /// <summary>
        /// 对大项有效，大项的默认检定点数
        /// </summary>
        public string PARA_DEFAULT_POINT { get; set; }

        /// <summary>
        ///  A2  关联代码分类的设备类别实体记录，单相电能表、三相电能表、互感器、采集终端
        /// </summary>
        public string EQUIP_CATEG { get; set; }

        /// <summary>
        /// 关联代码分类的值类型实体记录，整形、浮点、字符串、日期、按字典表等
        /// </summary>
        public string PARA_VALUE_TYPE { get; set; }

        /// <summary>
        /// 平台写入时间
        /// </summary>
        public string WRITE_DATE { get; set; }

        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }

        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }

        /// <summary>
        /// 检定依据	，	检定依据规程
        /// </summary>
        public string DETECT_BASIS { get; set; }

        /// <summary>
        /// 是否可以自动系统检定，见附录D：是否标志
        /// </summary>
        public string AUTO_DETECT { get; set; }

    }
}