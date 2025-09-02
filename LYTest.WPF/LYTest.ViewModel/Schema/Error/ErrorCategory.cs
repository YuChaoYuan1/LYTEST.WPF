using LYTest.ViewModel.Model;
using System;

namespace LYTest.ViewModel.Schema.Error
{
    /// <summary>
    /// 对应一个功率方向的一组误差点
    /// </summary>
    public class ErrorCategory : ViewModelBase
    {

        private string errorType = "基本误差";
        /// <summary>
        /// 误差类型
        /// </summary>
        public string ErrorType
        {
            get { return errorType; }
            set { SetPropertyValue(value, ref errorType, "ErrorType"); }
        }
        private string fangxiang = "正向有功";
        /// <summary>
        /// 功率方向
        /// </summary>
        public string Fangxiang
        {
            get { return fangxiang; }
            set { SetPropertyValue(value, ref fangxiang, "Fangxiang"); }
        }
        private string component = "H";
        /// <summary>
        /// 功率元件
        /// </summary>
        public string Component
        {
            get { return component; }
            set { SetPropertyValue(value, ref component, "Component"); }
        }

        private AsyncObservableCollection<ErrorModel> errorPoints = new AsyncObservableCollection<ErrorModel>();
        /// <summary>
        /// 在程序里面做了处理,不会出现相同的值,免得有些东西很不合常理
        /// </summary>
        public AsyncObservableCollection<ErrorModel> ErrorPoints
        {
            get { return errorPoints; }
            set { SetPropertyValue(value, ref errorPoints, "ErrorPoints"); }
        }

        /// <summary>
        /// 检定点数量发生变化时,sender:变化的检定点.e:0:移除,1:添加
        /// </summary>
        public event EventHandler PointsChanged;

        public void OnPointsChanged(ErrorModel model)
        {
            PointsChanged?.Invoke(model, null);
        }

        private bool flagLoad;

        public bool FlagLoad
        {
            get { return flagLoad; }
            set { SetPropertyValue(value, ref flagLoad, "FlagLoad"); }
        }
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
