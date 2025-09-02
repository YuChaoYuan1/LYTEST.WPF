namespace LYTest.Mis.NanRui.Tables
{
    //彩标信息表
    public class MT_CP_INFO
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
    
/// 合格标志
        /// </summary>
        public string PASS_FLAG { get; set; }
    
/// <summary>
/// 检定员
        /// </summary>
        public string DETECTER_NO { get; set; }
       
/// <summary>
/// 核验员
        /// </summary>
        public string RECHECKER_NO { get; set; }
      
/// <summary>
/// A7
        /// </summary>
        public string DETECT_DATE { get; set; }
    
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
/// 彩标标识;可选，彩标的唯一标识，用于拓展使用。
        /// </summary>
        public string CP_CODE { get; set; }
        
/// <summary>
/// 彩标位置
        /// </summary>
        public string CP_POSITION { get; set; }
            
	}
}