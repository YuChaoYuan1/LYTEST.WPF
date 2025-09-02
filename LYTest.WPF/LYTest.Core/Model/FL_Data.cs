using LYTest.Core.Enum;

namespace LYTest.Core.Model
{

    /// <summary>
    /// 费率类
    /// </summary>
    public class FL_Data
    {
        /// <summary>
        /// 费率类型
        /// </summary>
        public Cus_FeiLv Type;

        /// <summary>
        /// 费率时间 HH:mm
        /// </summary>
        public string Time;
        /// <summary>
        ///  时区 MM-dd
        /// </summary>
        public string Date;
        /// <summary>
        /// 时段表号 NN
        /// </summary>
        public int Day;
    }
}
