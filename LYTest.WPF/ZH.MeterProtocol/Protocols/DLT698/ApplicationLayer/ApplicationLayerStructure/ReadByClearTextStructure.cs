

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure
{
    /// <summary>
    /// 明文读取结构
    /// </summary>
    public class ReadByClearTextStructure
    {
        /// <summary>
        /// 请求
        /// </summary>
        public struct ReadApduFrameClearText
        {
            /// <summary>
            /// 读取请求
            /// </summary>
            public string GetRequest { get; set; }

            /// <summary>
            /// 读取模式
            /// </summary>
            public string GetRequestMode { get; set; }

            /// <summary>
            /// 序号及优先级
            /// </summary>
            public string PIID { get; set; }

            /// <summary>
            /// 对象描述符个数
            /// </summary>
            public string Num { get; set; }

            /// <summary>
            /// 对象描述符
            /// </summary>
            public string Oads { get; set; }

            /// <summary>
            /// 时间标签
            /// </summary>
            public string Timeflag { get; set; }

            /// <summary>
            /// RSD（Record Selection Descriptor） 
            /// 用于选择记录型对象属性的各条记录，即二维记录表的行
            /// 选择，其通过对构成记录的某些对象属性数值进行指定来进行选
            /// 择，范围选择区间：前闭后开，即[起始值，结束值）。
            /// 例如：事件类对象的事件记录表属性、冻结类对象的冻结数据
            /// 记录表属性、采集监控类的采集数据记录表。
            /// 应用提示：
            /// 1) 对于事件记录，通常使用事件发生时间进行选择；
            /// 2) 对于冻结数据记录，通常使用冻结时间进行选择
            /// RSD∷=CHOICE Selector N
            /// </summary>
            public string Rsd { get; set; }

            /// <summary>
            /// 行选择描述符
            /// </summary>
            public string Selector { get; set; }

            /// <summary>
            /// RCSD（Record Column Selection Descriptor） 
            /// 用于选择集合类对象属性中记录的某列或某几列内容，
            /// 即二维记录表的列选择，例如：事件记录或冻结数据记录中的某关
            /// 联对象属性数据列。
            /// RCSD::=SEQUENCE CSD个数
            /// </summary>
            public string Rcsd { get; set; }

            /// <summary>
            /// 列选择描述符CSD（Column Selection Descriptor）
            /// CSD∷=CHOICE
            /// </summary>
            public string Csd { get; set; }
        }
    }
}
