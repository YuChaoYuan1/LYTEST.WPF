
namespace LYTest.ViewModel.Debug
{
    public class MeterStartControlViewModel : ViewModelBase
    {
        //public bool IsCheck = true;
        private bool isCheck = false;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsCheck
        {
            get { return isCheck; }
            set { SetPropertyValue(value, ref isCheck, "IsCheck"); }
        }
        private string meterNo;
        public string MeterNo
        {
            get { return meterNo; }
            set { SetPropertyValue(value, ref meterNo, "MeterNo"); }
        }
        ///三个文本框颜色，

        private bool isMeter = false;
        /// <summary>
        /// 是否有表
        /// </summary>
        public bool IsMeter
        {
            get { return isMeter; }
            set { SetPropertyValue(value, ref isMeter, "IsMeter"); }
        }
        private int motor = 0;
        /// <summary>
        /// 电机位置0不确定，1在上，2 在下
        /// </summary>
        public int Motor
        {
            get { return motor; }
            set { SetPropertyValue(value, ref motor, "Motor"); }
        }
    }
}
