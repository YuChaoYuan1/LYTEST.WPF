namespace LYTest.ViewModel.Schema.Error
{
    /// 误差点数据模型
    /// <summary>
    /// 误差点数据模型
    /// </summary>
    public class ErrorModel : ViewModelBase
    {
        //数据格式:误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
        public string FangXiang { get; set; }
        public string Component { get; set; }


        public string ErrorType { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string Factor { get; set; }
        /// <summary>
        /// 电流倍数
        /// </summary>
        public string Current { get; set; }
        public bool FlagRemove { get; set; }
        private string lapCountIb = "2";
        /// <summary>
        /// 相对于Ib圈数
        /// </summary>
        public string LapCountIb
        {
            get { return lapCountIb; }
            set { lapCountIb = value; }
        }
        private string guichengMulti = "100";
        /// <summary>
        /// 规程误差限倍数
        /// </summary>
        public string GuichengMulti
        {
            get { return guichengMulti; }
            set { guichengMulti = value; }
        }
    }
}
