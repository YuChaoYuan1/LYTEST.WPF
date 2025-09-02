using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace LYTest.DataManager.ViewModel.Meters
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


        private Visibility visibilityText = Visibility.Collapsed;
        public Visibility VisibilityText
        {
            get { return visibilityText; }
            set { SetPropertyValue(value, ref visibilityText, "VisibilityText"); }
        }
        private Visibility visibilityDate = Visibility.Collapsed;
        public Visibility VisibilityDate
        {
            get { return visibilityDate; }
            set { SetPropertyValue(value, ref visibilityDate, "VisibilityDate"); }
        }

        private DateTime searchStartDate = DateTime.Now;
        public DateTime SearchStartDate
        {
            get => searchStartDate;
            set => SetPropertyValue(value, ref searchStartDate, "SearchStartDate");
        }
        private DateTime searchEndDate = DateTime.Now;
        public DateTime SearchEndDate
        {
            get => searchEndDate;
            set => SetPropertyValue(value, ref searchEndDate, "SearchEndDate");
        }
    }
}
