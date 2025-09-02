namespace LYTest.Device.Struct
{
    /// <summary>
    /// 端口信息
    /// </summary>
    public class StPortInfo
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 300;
        /// <summary>
        /// 通讯是否为串口,true UDP,false COM
        /// </summary>
        public bool IsUdp { get; set; } = false;
        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; } = "";
        /// <summary>
        /// 波特率
        /// </summary>
        public string Setting { get; set; } = "38400,n,8,1";
        ///// <summary>
        ///// 0无，1有
        ///// </summary>
        //public int IsExist { get; set; } = 0;

        /// <summary>
        /// 返回：COM_1 或 Port_192.168.1.111_1
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return false == IsUdp ? $"COM_{Port}" : $"Port_{IP}_{Port}";
        }
    }
}
