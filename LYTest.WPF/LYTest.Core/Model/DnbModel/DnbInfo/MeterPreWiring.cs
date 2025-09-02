using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    //ADD WKW 20220527

    /// <summary>
    /// 接线检查
    /// </summary>
    [Serializable()]
    public class MeterPreWiring : MeterBase
    {




        /// <summary>
        /// 多功能项目ID	
        /// </summary>
        public string PrjID { get; set; }

        /// <summary>
        /// 表条码号
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 表地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 时钟脉冲
        /// </summary>
        public string ClockPulse { get; set; }

        /// <summary>
        /// 电能脉冲
        /// </summary>
        public string EnergyPulses { get; set; }

        /// <summary>
        /// A相电压
        /// </summary>
        public string AVoltage { get; set; }

        /// <summary>
        /// A相电流
        /// </summary>
        public string ACurrent { get; set; }

        /// <summary>
        /// 电池电压
        /// </summary>
        public string BatteryVoltage { get; set; }


        /// <summary>
        /// 运行状态字
        /// </summary>
        public string RunStatusWord { get; set; }

        /// <summary>
        /// 项目结论[合格,不合格]
        /// </summary>
        public string Result { get; set; }

    }
}
