

namespace CLOU.Enum
{
    /// <summary>
    /// 标准表界面指示
    /// </summary>
    public enum Cus_StdMeterScreen
    {
        功率测量界面=0x01,//PQS
        电压电流相位角界面=0x02,//UI
        电能误差界面=0x03,//E
        电能误差启动界面 = 0x04,//energy error starting interface
        走字测试界面 = 0x05,//register test display
        走字测试启动界面 = 0x06,//register test starting display
        矢量图 = 0x07,//vector diagram
        /// <summary>
        /// 谐波柱图界面
        /// </summary>
        谐波柱图界面 = 0x09,//harmonics, bar-graph
        /// <summary>
        /// 谐波列表界面
        /// </summary>
        谐波列表界面 = 0x0A,//harmonics, table
        /// <summary>
        /// 波形界面
        /// </summary>
        波形界面 = 0x0B,//wave
        /// <summary>
        /// 清除设置界面
        /// </summary>
        清除设置界面 = 0xFE
    }
}