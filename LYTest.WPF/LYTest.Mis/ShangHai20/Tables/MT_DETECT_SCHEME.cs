using System;
using System.Collections.Generic;

namespace LYTest.Mis.ShangHai20.Tables
{
    //1）存放制定和维护室内检定方案
    public class MT_DETECT_SCHEME
    {
        /// <summary>
        /// 检定方案实体记录的唯一性标识
        /// </summary>
        public decimal SCHEMA_ID { get; set; }

        /// <summary>
        /// 检定方案编号
        /// </summary>
        public string SCHEMA_NO { get; set; }

        /// <summary>
        /// 检定方案名称
        /// </summary>
        public string SCHEMA_NAME { get; set; }

        /// <summary>
        ///  A2  关联代码分类的设备类别实体记录，单相电能表、三相电能表、互感器、采集终端
        /// </summary>
        public string EQUIP_CATEG { get; set; }
        /// <summary>
        /// 检定类别：01 样品比对，02 全性能试验，03 抽样检定检测，04 检定检测校准，05 计量标准器检定校准，06 测试设备检定校准，07 计量标准装置检定校准，08 仲裁检测，09 委托检定，10 临时检测申校检测，11 招标选型产品检测，12 招标前批次检测,13 检定质量核查，14 库存复检，15 监督抽检方案，16 适应性检查，17 人工复检
        /// </summary>
        public string DETECT_TYPE { get; set; }
        /// <summary>
        /// 人工、自动，
        /// </summary>
        public string DETECT_MODE { get; set; }
        /// <summary>
        /// 平台写入时间
        /// </summary>
        public DateTime WRITE_DATE { get; set; }
        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }

        /// <summary>
        /// 检验任务参数电能表
        /// </summary>
        public SortedDictionary<string, MT_METER> dnb = new SortedDictionary<string, MT_METER>();
        /// <summary>
        /// 检验任务参数电能表(ZH)
        /// </summary>
        public Dictionary<string, MT_METER> dnb_ZH = new Dictionary<string, MT_METER>();


    }
}