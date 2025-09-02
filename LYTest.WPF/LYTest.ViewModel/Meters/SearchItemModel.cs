using LYTest.ViewModel.InputPara;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.Meters
{
    /// <summary>
    /// 查询单元条件
    /// </summary>
    public class SearchItemModel : ViewModelBase
    {
        private bool isSelected = false;
        /// <summary>
        /// 条件是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetPropertyValue(value, ref isSelected, "IsSelected"); }
        }

        private string columnName;
        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName
        {
            get { return columnName; }
            set { SetPropertyValue(value, ref columnName, "ColumnName"); }
        }
        public ObservableCollection<string> ColumnNames { get; set; }
        private EnumCompare compareCondition = EnumCompare.等于;
        /// <summary>
        /// 查询条件
        /// </summary>
        public EnumCompare CompareCondition
        {
            get { return compareCondition; }
            set { SetPropertyValue(value, ref compareCondition, "CompareCondition"); }
        }
        private ObservableCollection<EnumCompare> conditions = new ObservableCollection<EnumCompare>();
        public ObservableCollection<EnumCompare> Conditions
        {
            get { return conditions; }
            set { conditions = value; }
        }
        private string searchValue;
        /// <summary>
        /// 查询的值
        /// </summary>
        public string SearchValue
        {
            get { return searchValue; }
            set { SetPropertyValue(value, ref searchValue, "SearchValue"); }
        }

        private AsyncObservableCollection<string> seachValues = new AsyncObservableCollection<string>();
        /// <summary>
        /// 查询值下拉列表
        /// </summary>
        public AsyncObservableCollection<string> SeachValues
        {
            get { return seachValues; }
            set { SetPropertyValue(value, ref seachValues, "SeachValues"); }
        }
    }
}
