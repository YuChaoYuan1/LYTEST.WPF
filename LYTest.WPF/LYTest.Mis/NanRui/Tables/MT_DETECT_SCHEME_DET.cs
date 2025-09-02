using System;

namespace LYTest.Mis.NanRui.Tables
{
    //检定方案数据明细信息
    public class MT_DETECT_SCHEME_DET
    {
        /// <summary>
        /// 检定方案实体记录的唯一性标识
        /// </summary>
        public decimal SCHEMA_ID { get; set; }
        /// <summary>
        /// 检测点顺序
        /// </summary>
        public string PARA_INDEX { get; set; }
        /// <summary>
        /// 按“检定项参数编码信息表”的小项顺序 用“|”分割
        /// </summary>
        public string PARA_VALUE { get; set; }
        /// <summary>
        /// 平台写入时间
        /// </summary>
        public DateTime WRITE_DATE { get; set; }
        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }
        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// 参数标识，	“检定项参数编码信息表”的检定分项的检定参数标识
        /// </summary>
        public string PARA_NO { get; set; }

        /// <summary>
        /// 参数标识，	“检定项参数编码信息表”的检定分项的检定参数标识
        /// </summary>
        public string CONTROL_METHOD { get; set; }



    }
}