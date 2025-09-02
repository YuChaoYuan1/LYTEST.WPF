using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 潜动启动数据
    /// </summary>
    [Serializable()]
    public class MeterQdQid : MeterBase
    {

        /// <summary>
        /// 脉冲数
        /// </summary>
        public string Pulse { get; set; }


        /// <summary>
        /// 4检定项目号
        /// </summary>
        public string PrjNo { get; set; }

        /// <summary>
        /// 5检定方向
        /// </summary>
        public string PowerWay { get; set; }
        /// <summary>
        /// 6项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 7结论
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 12电流
        /// </summary>
        public string Current { get; set; }

        /// <summary>
        /// 9开始时间
        /// </summary>
        public string TimeStart { get; set; }

        /// <summary>
        /// 10结束时间
        /// </summary>
        public string TimeEnd { get; set; }

        /// <summary>
        /// 11实际起动时间（秒）
        /// </summary>
        public string ActiveTime { get; set; }

        /// <summary>
        /// 12标准时间(秒)
        /// </summary>
        public string StandartTime { get; set; }

        /// 13电压倍数（%换成小数，例如1.15））
        /// </summary>
        public string Voltage { get; set; }

        /// <summary>
        /// 脉冲间隔1
        /// </summary>
        public string PushTime1 { get; set; }
        /// <summary>
        /// 脉冲间隔2
        /// </summary>
        public string PushTime2 { get; set; }
        /// <summary>
        /// 误差值
        /// </summary>
        public string ErrorValue { get; set; }
        /// <summary>
        /// 误差限
        /// </summary>
        public string QDLimit { get; set; }
    }
}
