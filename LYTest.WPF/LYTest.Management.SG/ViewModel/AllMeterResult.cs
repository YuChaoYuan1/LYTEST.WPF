using LYTest.DAL;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager.SG
{
    public class AllMeterResult : ViewModelBase
    {
        private SelectCollection<OneMeterResult> resultCollection = new SelectCollection<OneMeterResult>();
        /// <summary>
        /// 所有检定结论
        /// </summary>
        public SelectCollection<OneMeterResult> ResultCollection
        {
            get { return resultCollection; }
            set { SetPropertyValue(value, ref resultCollection, "ResultCollection"); }
        }
        /// <summary>
        /// 加载临时库所有表信息
        /// </summary>
        public AllMeterResult()
        {
            ResultCollection.ItemsSource.Clear();
        }
        /// <summary>
        /// 从正式库加载表信息
        /// </summary>
        /// <param name="meters"></param>
        public AllMeterResult(IEnumerable<DynamicViewModel> meters)
        {
            LoadMeters(meters);
        }

        public void LoadMeters(IEnumerable<DynamicViewModel> meters)
        {
            ResultCollection.ItemsSource.Clear();
            if (meters == null)
            {
                return;
            }
            for (int i = 0; i < meters.Count(); i++)
            {
                string meterPk = meters.ElementAt(i).GetProperty("METER_ID") as string;
                ResultCollection.ItemsSource.Add(new OneMeterResult(meterPk, false));
            }
        }
        
    }
}
