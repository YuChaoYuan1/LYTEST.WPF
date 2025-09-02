namespace LYTest.Core
{
    /// <summary>
    /// 端口信息
    /// </summary>
    public class ComPortInfo
    {
        public ComPortInfo()
        {
            Port = 0;
            LinkType = LinkType.COM;
            IP = "";
            Setting = "38400,n,8,1";
            IsExist = false;
            OtherParams = string.Empty;
        }
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 通讯串口类型
        /// </summary>
        public LinkType LinkType { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public string Setting { get; set; }
        /// <summary>
        /// 端口是否存在：0无，1有
        /// </summary>
        public bool IsExist { get; set; }

        /// <summary>
        /// Can 通讯参数
        /// </summary>
        public string OtherParams { get; set; }

        /// <summary>
        /// 远程端口
        /// </summary>
        public int RemotePort { get; set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int StartPort { get; set; }
        /// <summary>
        /// 字节最大时间间隔(ms)
        /// </summary>
        public string MaxTimePerByte { get; set; }


        /// <summary>
        /// 帧最大时间间隔(ms)
        /// </summary>
        public string MaxTimePerFrame { get; set; }

    }
}
