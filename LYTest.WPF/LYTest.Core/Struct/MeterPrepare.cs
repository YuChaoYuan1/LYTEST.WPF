using System;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 预先调试试验结论
    /// </summary>
    [Serializable()]
    public class MeterPrepare
    {
        /// <summary>
        /// 电流开路结论
        /// </summary>
        public string ResultConnect { get; set; }
        /// <summary>
        /// RS485结论
        /// </summary>
        public string ResultRS485 { get; set; }
        /// <summary>
        /// 停电抄表
        /// </summary>
        public string ResultPowerOffReadMeter { get; set; }

        /// <summary>
        /// 时钟脉冲结论
        /// </summary>
        public string ResultTime { get; set; }

        /// <summary>
        /// 电能脉冲结论
        /// </summary>
        public string ResultPower { get; set; }

        /// <summary>
        /// 条形码与表地址核对结论
        /// </summary>
        public string ResultCheckBarCode { get; set; }


        private string result = null;
        /// <summary>
        /// 总结论
        /// </summary>
        public string Result
        {
            get
            {
                if (ResultConnect == "×" || ResultRS485 == "×" || ResultTime == "×" || ResultPower == "×")
                {
                    result = ConstHelper.不合格;
                }
                else if (ResultConnect == "√" || ResultRS485 == "√" || ResultTime == "√" || ResultPower == "√")
                {
                    result = ConstHelper.合格;
                }
                else
                {
                    result = null;
                }
                return result;
            }
        }
    }
}
