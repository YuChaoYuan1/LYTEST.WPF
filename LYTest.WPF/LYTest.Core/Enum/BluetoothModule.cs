namespace LYTest.Core.Enum
{
    /// <summary>
    /// 光模块类型
    /// </summary>
    public enum BluetoothModule_OpticalModule
    {
        内置光模块 = 0,
        外置光模块 = 1,
    }
    /// <summary>
    ///发射功率
    /// </summary>
    public enum BluetoothModule_TransmitPower
    {
        挡位1 = 0,//4dnm
        挡位2 = 1,//0dnm
        挡位3 = 2, //-4dnm
        挡位4 = 3, //-8dnm
        挡位5 = 4, //-20dnm
    }
    /// <summary>
    /// 蓝牙模块通信模式
    /// </summary>
    public enum BluetoothModule_CommunicationMode
    {
        普通检定模式 = 0,
        脉冲跟随模式 = 1,
    }
    /// <summary>
    /// 表频段
    /// </summary>
    public enum BluetoothModule_TableFrequencyBand
    {
        全频段 = 0,
        带内频段 = 1,
        带外频段 = 2,

    }
    /// <summary>
    /// 脉冲类型
    /// </summary>
    public enum BluetoothModule_PulseModel
    {
        秒脉冲输出 = 0,
        需量周期 = 1,
        时段投切 = 2,
        正向谐波脉冲 = 3,
        反向谐波脉冲 = 4,
        无功脉冲 = 5,
        有功脉冲 = 6,
        退出检定模式 = 255,
    }

}
