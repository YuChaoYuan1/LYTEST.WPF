using LYTest.MeterProtocol.DataFlag;
using LYTest.ViewModel.Model;

namespace LYTest.ViewModel.DISetsModel
{
    public class DIClass : ViewModelBase
    {
        private string className;
        /// <summary>
        /// 注释
        /// </summary>
        public string ClassName
        {
            get { return className; }
            set { SetPropertyValue(value, ref className, "ClassName"); }
        }

        private DI selectDI;
        /// <summary>
        /// 注释
        /// </summary>
        public DI SelectDI
        {
            get { return selectDI; }
            set { SetPropertyValue(value, ref selectDI, "SelectDI"); }
        }
        private AsyncObservableCollection<DI> dIModels = new AsyncObservableCollection<DI>();
        /// <summary>
        /// 数据标识项目结合
        /// </summary>
        public AsyncObservableCollection<DI> DIModels
        {
            get { return dIModels; }
            set { SetPropertyValue(value, ref dIModels, "DIModels"); }
        }
    }
}
