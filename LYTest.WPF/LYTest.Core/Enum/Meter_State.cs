namespace LYTest.Core.Enum
{


    /// <summary>
    /// 电压短路标标志00正常，01短路，02继电器不工作
    /// </summary>
    public enum MeterState_U
    {
        正常,
        短路,
        继电器不工作
    }
    /// <summary>
    /// 电流短路标标志，00正常,01旁路成功，02旁路继电器不工作
    /// </summary>
    public enum MeterState_I
    {
        正常,
        旁路成功,
        旁路继电器不工作
    }
    /// <summary>
    /// 电机行程标志，00电机行程不确定，01电机行程在上取表出座位置，02电机行程在下，压表入座位置
    /// </summary>
    public enum MeterState_Motor
    {
        电机不确定,
        电机在上,
        电机行在下
    }
    /// <summary>
    /// 挂表状态标志，00没挂表，01以挂表
    /// </summary>
    public enum MeterState_YesOrNo
    {
        没挂表,
        以挂表
    }
    /// <summary>
    /// CT电量过载标志，00正常，01过载(北京改造线科陆CT)
    /// </summary>
    public enum MeterState_CT
    {
        正常,
        过载
    }
    /// <summary>
    ///跳匝指示灯标志 00正常，01以输出跳匝信号 
    /// </summary>
    public enum MeterState_Trip
    {
        正常,
        以输出跳匝信号
    }
    /// <summary>
    /// 二级设备温度板电流过载标志，00没过载，01过载(北京改造线，互感表)
    /// </summary>
    public enum MeterState_TemperatureI
    {
        没过载,
        过载
    }
}
