using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LYTest.ViewModel.Meters
{
    /// <summary>
    ///修改表位结论模型
    /// </summary>
    public class SetMeterResoultModel : ViewModelBase
    {

        public SetMeterResoultModel()
        {
            ResoultType.Clear();
            ResoultType.Add("合格");
            ResoultType.Add("不合格");


        }
        //第一个表位的集合
        //列的集合
        //列名--值
        //
        /// 60块表的检定结论
        /// <summary>
        /// 60块表的检定结论
        /// </summary>
        /// 
        private AsyncObservableCollection<DynamicViewModel> checkResults = new AsyncObservableCollection<DynamicViewModel>();
        public AsyncObservableCollection<DynamicViewModel> CheckResults
        {
            get { return checkResults; }
            set { SetPropertyValue(value, ref checkResults, "CheckResults"); }
        }


        private ObservableCollection<string> resoultType = new ObservableCollection<string>();
        public ObservableCollection<string> ResoultType
        {
            get { return resoultType; }
            set { SetPropertyValue(value, ref resoultType, "ResoultType"); }
        }
        /// <summary>
        /// 设置值
        /// </summary>
        public void SetResoultData()
        {
            if (CheckResults.Count < 0) return;
            List<string> list = CheckResults[0].GetAllProperyName();
            string[] arrayResult = new string[CheckResults.Count];
            for (int i = 0; i < list.Count; i++)     //修改所有属性
            {
                for (int k = 0; k < CheckResults.Count; k++)
                {
                    arrayResult[k] = CheckResults[k].GetProperty(list[i]) as string;
                }
                EquipmentData.CheckResults.UpdateCheckResult(EquipmentData.CheckResults.CheckNodeCurrent.ItemKey, list[i], arrayResult);
            }
        }
    }
}
