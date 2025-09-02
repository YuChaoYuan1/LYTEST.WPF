namespace LYTest.ViewModel.CheckInfo
{
    /// <summary>
    /// 表位检定结论,为了使结论总览视图有更友好的数据,创建了此类
    /// </summary>
    public class MeterResultUnit : ViewModelBase
    {
        private string result = "";
        /// <summary>
        /// 结论:"","合格","不合格"
        /// </summary>
        public string Result
        {
            get { return result; }
            set { SetPropertyValue(value, ref result); }
        }
        private string resultValue = "";
        /// <summary>
        /// 结论的值,用于结论总览的显示
        /// </summary>
        public string ResultValue
        {
            get { return resultValue; }
            set { SetPropertyValue(value, ref resultValue); }
        }
    }
}
