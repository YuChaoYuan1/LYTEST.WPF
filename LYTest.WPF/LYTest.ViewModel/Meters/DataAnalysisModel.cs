using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Meters
{
    public class DataAnalysisModel : ViewModelBase
    {
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


        private AsyncObservableCollection<DynamicViewModel> results = new AsyncObservableCollection<DynamicViewModel>();
        public AsyncObservableCollection<DynamicViewModel> Results
        {
            get { return results; }
            set { SetPropertyValue(value, ref results, "Results"); }
        }

        private int slectIndex;
        /// <summary>
        /// 选中的
        /// </summary>
        public int SelectIndex
        {
            get { return slectIndex; }
            set
            {
                SetPropertyValue(value, ref slectIndex, "SelectIndex");
                if (SelectIndex >= 0)
                {
                    RefUI();
                }
            }
        }


        private AsyncObservableCollection<string> nameList = new AsyncObservableCollection<string>();
        /// <summary>
        /// 名称列表
        /// </summary>
        public AsyncObservableCollection<string> NameList
        {
            get { return nameList; }
            set { SetPropertyValue(value, ref nameList, "NameList"); }
        }

        private void RefUI()
        {

            Results.Clear();
            for (int index = 0; index < CheckResults.Count; index++)
            {
                DynamicViewModel model = new DynamicViewModel(index);
                List<string> names = CheckResults[index].GetAllProperyName();
                for (int i = 0; i < names.Count; i++)
                {
                    //if (names[i] == "结论") continue;
                    string valeu = CheckResults[index].GetProperty(names[i]) as string;
                    if (valeu == null) continue;
                    string[] data = valeu.Split('|');
                    if (data.Length > 0)
                    {
                        if (data.Length > SelectIndex)
                        {
                            model.SetProperty(names[i], data[SelectIndex]);
                        }
                        else
                        {
                            model.SetProperty(names[i], "");
                        }
                    }
                    else    //普通的值的情况
                    {
                        model.SetProperty(names[i], valeu);
                    }
                }
                Results.Add(model);
            }

        }


    }
}
