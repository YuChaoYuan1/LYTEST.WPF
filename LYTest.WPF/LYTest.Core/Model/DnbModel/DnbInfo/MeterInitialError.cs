namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    public class MeterInitialError : MeterBase
    {
        public MeterInitialError() : base()
        {
            PrjNo = "";
            Name = "";
            GLYS = "";
            GLFX = "";
            YJ = "";
            IbX = "";
            IbXString = "";

            UpWCValue = "";
            UpWCPJ = "";
            UpWCHZ = "";
            DownWCValue = "";
            DownWCPJ = "";
            DownWCHZ = "";
            WCData = "";
            WcMore = "";

            UpLimit = "";
            DownLimit = "";
            Circle = "";
            Remark = "";
            Result = "";
        }

        /// <summary>
        /// 初始固有误差项目ID
        /// </summary>
        public string PrjNo { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 功率因数[1.0, 0.5L]
        /// </summary>
        public string GLYS { get; set; }

        /// <summary>
        /// 功率方向
        /// </summary>
        public string GLFX { get; set; }

        /// <summary>
        /// 元件
        /// </summary>
        public string YJ { get; set; }

        /// <summary>
        /// 额定电流的倍数
        /// </summary>
        public string IbX { get; set; }

        /// <summary>
        /// 额定电流IB的倍数的字符串（IB、IMAX）
        /// </summary>
        public string IbXString { get; set; }

        /// <summary>
        /// 上升误差值 上升误差一|上升误差二
        /// </summary>
        public string UpWCValue { get; set; }

        /// <summary>
        /// 上升误差差平均
        /// </summary>
        public string UpWCPJ { get; set; }

        /// <summary>
        /// 上升误差化整值
        /// </summary>
        public string UpWCHZ { get; set; }

        /// <summary>
        /// 下降误差值 下降误差一|下降误差二
        /// </summary>
        public string DownWCValue { get; set; }

        /// <summary>
        /// 下降误差差平均
        /// </summary>
        public string DownWCPJ { get; set; }

        /// <summary>
        /// 下降误差化整值
        /// </summary>
        public string DownWCHZ { get; set; }

        /// <summary>
        /// 初始固有误差差值
        /// </summary>
        public string WCData { get; set; }

        /// <summary>
        /// 误差完整信息
        /// </summary>
        public string WcMore { get; set; }

        /// <summary>
        /// 误差上限
        /// </summary>
        public string UpLimit { get; set; }

        /// <summary>
        /// 误差下限
        /// </summary>
        public string DownLimit { get; set; }

        /// <summary>
        /// 圈数
        /// </summary>
        public string Circle { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 项目结论[合格,不合格]
        /// </summary>
        public string Result { get; set; }
    }
}
