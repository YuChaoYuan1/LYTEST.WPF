using System;

namespace LYTest.Mis.HeBei20.Tables
{
    //检定线完成铅封后，将检定设备的铅封条码信息传回平台系统
    public class MT_SEAL_INST
    {
        /// <summary>
        /// A1
        /// </summary>
        public string DETECT_TASK_NO { get; set; }
        /// <summary>
        /// A2
        /// </summary>
        public string SYS_NO { get; set; }

        /// <summary>
        ///  A2  关联代码分类的设备类别实体记录，单相电能表、三相电能表、互感器、采集终端
        /// </summary>
        public string EQUIP_CATEG { get; set; }

        /// <summary>
        /// A6
        /// </summary>
        public string BAR_CODE { get; set; }

        /// <summary>
        /// 铅封位置
        /// </summary>
        public string SEAL_POSITION { get; set; }

        /// <summary>
        /// 铅封条码
        /// </summary>
        public string SEAL_BAR_CODE { get; set; }

        /// <summary>
        /// 铅封时间
        /// </summary>
        public string SEAL_DATE { get; set; }

        /// <summary>
        /// 铅封人员
        /// </summary>
        public string SEALER_NO { get; set; }

        /// <summary>
        /// A14
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
        /// 铅封是否有效
        /// </summary>
        public string IS_VALID { get; set; }

    }
}