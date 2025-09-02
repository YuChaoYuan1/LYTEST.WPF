namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 数据显示功能
    /// </summary>
    public class MeterShow : MeterBase
    {
        public MeterShow()
        {
            DefaultKey = "";
            DefaultName = "";
            Name = "";
            Group = "";
            ItemType = "";
            Result = "";
            TypeId = 0;
            Type = "";
            SubItemId = 0;
            SubItem = "";
            DataFlag = "";
            DataLength = 0;
            DataDot = 0;
            DataFmt = "";
            ReadWrite = 0;
            Content = "";
            Data = "";
            Other1 = "";
            Other2 = "";
            Other3 = "";
            Other4 = "";
            Other5 = "";
            Other6 = "";
            Other7 = "";
            Other8 = "";
            Other9 = "";
            Other10 = "";
        }

        /// <summary>
        /// 1项目ID
        /// </summary>
        public string DefaultKey { get; set; }

        /// <summary>
        /// 2项目名称名称
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 组别	
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// 项目类型
        /// </summary>
        public string ItemType { get; set; }
        /// <summary>
        /// 结论	
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 实验项目类型主键
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// 实验项目类型名称
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 分项实验项目主键
        /// </summary>
        public int SubItemId { get; set; }
        /// <summary>
        /// 分项实验项目名称	
        /// </summary>
        public string SubItem { get; set; }
        /// <summary>
        /// 标示符
        /// </summary>
        public string DataFlag { get; set; }
        /// <summary>
        /// 数据长度	
        /// </summary>
        public int DataLength { get; set; }
        /// <summary>
        /// 小数位
        /// </summary>
        public int DataDot { get; set; }
        /// <summary>
        /// 数据格式	
        /// </summary>
        public string DataFmt { get; set; }
        /// <summary>
        /// 读写标志
        /// </summary>
        public int ReadWrite { get; set; }
        /// <summary>
        /// 对比内容	
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 读取数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 备用1	
        /// </summary>
        public string Other1 { get; set; }
        /// <summary>
        /// 备用2
        /// </summary>
        public string Other2 { get; set; }
        /// <summary>
        /// 备用3	
        /// </summary>
        public string Other3 { get; set; }
        /// <summary>
        /// 备用4
        /// </summary>
        public string Other4 { get; set; }
        /// <summary>
        /// 备用5	
        /// </summary>
        public string Other5 { get; set; }
        /// <summary>
        /// 备用6
        /// </summary>
        public string Other6 { get; set; }
        /// <summary>
        /// 备用7	
        /// </summary>
        public string Other7 { get; set; }
        /// <summary>
        /// 备用8
        /// </summary>
        public string Other8 { get; set; }
        /// <summary>
        /// 备用9	
        /// </summary>
        public string Other9 { get; set; }
        /// <summary>
        /// 备用10
        /// </summary>
        public string Other10 { get; set; }
    }
}

