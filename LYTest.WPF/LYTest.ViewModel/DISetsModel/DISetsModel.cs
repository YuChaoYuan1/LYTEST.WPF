using LYTest.MeterProtocol.DataFlag;
using LYTest.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.ViewModel.DISetsModel
{
    /// <summary>
    /// 数据标识模型
    /// </summary>
    public class DISetsModel : ViewModelBase
    {
        /// <summary>
        /// 查询的数据
        /// </summary>
        public AsyncObservableCollection<DI> SelectResult { get; } = new AsyncObservableCollection<DI>();



        private bool isVisibleSelectResult = false;
        /// <summary>
        /// 注释
        /// </summary>
        public bool IsVisibleSelectResult
        {
            get => isVisibleSelectResult;
            set => SetPropertyValue(value, ref isVisibleSelectResult);
        }



        /// <summary>
        /// 数据标识项目结合
        /// </summary>
        public AsyncObservableCollection<DIClass> DIClassModels { get; } = new AsyncObservableCollection<DIClass>();

        private DIClass selectDIClass = new DIClass();
        /// <summary>
        /// 注释
        /// </summary>
        public DIClass SelectDIClass
        {
            get => selectDIClass;
            set
            {
                IsFlag = false;
                SetPropertyValue(value, ref selectDIClass);
                IsFlag = true;
            }
        }

        private bool isFlag;
        /// <summary>
        /// 注释
        /// </summary>
        public bool IsFlag
        {
            get => isFlag;
            set => SetPropertyValue(value, ref isFlag);
        }


        public void SelectData(string text)
        {

            SelectResult.Clear();
            if (string.IsNullOrWhiteSpace(text))
            {
                IsVisibleSelectResult = false;
                return;
            }
            foreach (var item in DataFlagS.DIS.Where(item => item.DataFlagDiName.IndexOf(text) != -1))
            {
                SelectResult.Add(item);
            }
            IsVisibleSelectResult = true;
        }

        public void DiRefresh()
        {
            //软件初始化的时候初始化所有的数据标识集合
            //使用的地方有三个:方案界面选择，数据标识配置界面，协议配置界面
            //方案选中界面是动态绑定的,他需要一个选中项目
            //数据标识界面就是操作这个模型
            DIClassModels.Clear();
            List<DI> Data = DataFlagS.DIS;
            foreach (DI item in Data)
            {
                DIClass d = DIClassModels.FirstOrDefault(x => x.ClassName == item.ClassName);
                if (d == null)
                {
                    d = new DIClass() { ClassName = item.ClassName, DIModels = new AsyncObservableCollection<DI>() };
                    DIClassModels.Add(d);
                }
                d.DIModels.Add(item);
            }
        }


        /// <summary>
        /// 初始化项目集合
        /// </summary>
        public DISetsModel()
        {



        }
    }
}
