using System;
namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    [Serializable()]
    public class MeterZZError : MeterBase
    {
        public MeterZZError()
        {
            PowerStart = 0;
            PowerEnd = 0;
        }


        /// <summary>
        /// 4项目ID
        /// </summary>
        public string PrjID { get; set; }
        /// <summary>
        /// 10起始电量
        /// </summary>
        public FloatE PowerStart { get; set; }
        /// <summary>
        /// 11终止电量
        /// </summary>
        public FloatE PowerEnd { get; set; }

        /// <summary>
        /// 17误差电量：电表走字量-标准走字量
        /// </summary>
        public string PowerError { get; set; }
        /// <summary>
        /// 19结论：[合格,不合格]
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 5检定方向
        /// </summary>
        public string PowerWay { get; set; }
        /// <summary>
        /// 6费率
        /// </summary>
        public string Fl { get; set; }
        /// <summary>
        /// 7走字起始时间
        /// </summary>
        public string TimeStart { get; set; }
        /// <summary>
        /// 8走字需要时间
        /// </summary>
        public string TimeNeed { get; set; }
        /// <summary>
        /// 12起码总
        /// </summary>
        public string PowerSumStart { get; set; }
        /// <summary>
        /// 13止码总
        /// </summary>
        public string PowerSumEnd { get; set; }
        /// <summary>
        /// 14走字电量：起字结果电量 - 起字起始电量
        /// </summary>
        public string WarkPower { get; set; }
        /// <summary>
        /// 15所走脉冲数
        /// </summary>
        public string Pules { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string GLYS { get; set; }
        /// <summary>
        /// 电流倍数
        /// </summary>
        public string IbX { get; set; }
        /// <summary>
        /// 电流倍数字符串
        /// </summary>
        public string IbXString { get; set; }
        /// <summary>
        /// 元件
        /// </summary>
        public string YJ { get; set; }
        /// <summary>
        /// 9对应需要走字电量，或按照时间走字计算的电量
        /// </summary>
        public string NeedEnergy { get; set; }

        /// <summary>
        /// 16标准表累计电量
        /// </summary>
        public string STMEnergy { get; set; }

        /// <summary>
        /// 18试验方式，标准表法，计读脉冲法，电能表常数试验法
        /// </summary>
        public string TestWay { get; set; }

        /// <summary>
        /// 走字电量
        /// </summary>
        public string ZzCurrent { get; set; }

        /// <summary>
        /// 电能表累计电量
        /// </summary>
        public string MeterEnergy { get; set; }

        /// <summary>
        /// 误差值
        /// </summary>
        public string ErrorValue { get; set; }

        /// <summary>
        /// 误差限
        /// </summary>
        public string ErrorRate { get; set; }
    }
}
