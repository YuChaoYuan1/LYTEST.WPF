using System;

namespace LYTest.MeterProtocol.Struct
{
    [Serializable()]
    public class CarrierWareInfo
    {
        /// <summary>
        /// 载波协议信息
        /// </summary>
        public CarrierWareInfo()
        {
            Name = "未知";
            CarrierType = "2041";
            RdType = "鼎信";
            CommuType = "2018";
            Baudrate = "9600,e,8,1";
            Port = "COM40";
            OutTime = "2000";
            ByteTime = "200";
            IsRoute = false;
            IsBroad = false;
        }

        /// <summary>
        /// 载波名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 通讯介质
        /// </summary>
        public string CarrierType { get; set; }

        /// <summary>
        /// 抄表器类型
        /// </summary>
        public string RdType { get; set; }

        /// <summary>
        /// 通讯方式
        /// </summary>
        public string CommuType { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        public string Baudrate { get; set; }

        /// <summary>
        /// 通讯端口
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 命令延时(ms)
        /// </summary>
        public string OutTime { get; set; }

        /// <summary>
        /// 字节延时(ms)
        /// </summary>
        public string ByteTime { get; set; }

        /// <summary>
        /// 路由标识
        /// <para>false-表示通信模块不带路由或工作在旁路模式，True-表示通信模块带路由或工作在路由模式。</para>
        /// </summary>
        public bool IsRoute { get; set; }

        /// <summary>
        /// 载波模式 false = 窄带 true =宽带
        /// </summary>
        public bool IsBroad { get; set; }



        /// <summary>
        /// 返回载波设备信息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("载波名称:{0} 通讯介质:{1} 抄表器类型:{2} 通讯方式:{3} 波特率:{4} 通讯端口:{5} 命令延时(ms):{6} 字节延时(ms):{7} 路由标识:{8}  窄带|宽带:{9}", Name, CarrierType, RdType, CommuType, Baudrate, Port, OutTime, ByteTime, IsRoute, IsBroad);
        }
    }
}
