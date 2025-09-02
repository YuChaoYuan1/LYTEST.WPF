using LYTest.Core.Enum;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 表位状态
    /// </summary>
    public class MeterState
    {

        /// <summary>
        /// 电压短路标标志
        /// </summary>
        public MeterState_U U { get; set; }

        /// <summary>
        ///  电流短路标标志
        /// </summary>
        public MeterState_I I { get; set; }

        /// <summary>
        /// 电机行程标志
        /// </summary>
        public MeterState_Motor Motor { get; set; }

        /// <summary>
        /// 挂表状态标志
        /// </summary>
        public MeterState_YesOrNo YesOrNo { get; set; }

        /// <summary>
        /// CT电量过载标志
        /// </summary>
        public MeterState_CT CT { get; set; }

        /// <summary>
        /// 跳匝指示灯标志
        /// </summary>
        public MeterState_Trip Trip { get; set; }

        /// <summary>
        /// 二级设备温度板电流过载标志
        /// </summary>
        public MeterState_TemperatureI TemperatureI { get; set; }

    }
}
