using LYTest.DAL;

namespace LYTest.ViewModel.InputPara
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public class SearchMenuItem : ViewModelBase
    {
        private string columnName;
        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName
        {
            get { return columnName; }
            set { SetPropertyValue(value, ref columnName, "ColumnName"); }
        }
        private string fieldName;
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName
        {
            get { return fieldName; }
            set { SetPropertyValue(value, ref fieldName, "FieldName"); }
        }
        /// <summary>
        /// 是否为数字
        /// </summary>
        public bool IsNumber
        {
            get
            {
                return fieldName == "METER_ID";
            }
        }

        private EnumCompare compareExpression = EnumCompare.等于;
        /// <summary>
        /// 比较表达式
        /// </summary>
        public EnumCompare CompareExpression
        {
            get { return compareExpression; }
            set { SetPropertyValue(value, ref compareExpression, "CompareExpression"); }
        }
        private string valueDisplay;
        /// <summary>
        /// 要比较的值
        /// </summary>
        public string ValueDisplay
        {
            get { return valueDisplay; }
            set { SetPropertyValue(value, ref valueDisplay, "ValueDisplay"); }
        }

        public string CodeType { get; set; }
        public  EnumValueType ValueType { get; set; }

        public SearchMenuItem SearchItemChild { get; set; }

        /// <summary>
        /// 比较字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string stringTemp = "";
            string realValue = ValueDisplay;
            string valueTemp = "";
            if (ValueType == EnumValueType.编码值)
            {
                valueTemp = CodeDictionary.GetValueLayer2(CodeType, realValue);
            }
            if (!string.IsNullOrEmpty(valueTemp))
            {
                realValue = valueTemp;
            }
            if (IsNumber)
            {
                #region 数字格式
                switch (CompareExpression)
                {
                    case EnumCompare.等于:
                        stringTemp = string.Format("{0} = {1}", FieldName, realValue);
                        break;
                    case EnumCompare.大于:
                        stringTemp = string.Format("{0} > {1}", FieldName, realValue);
                        break;
                    case EnumCompare.小于:
                        stringTemp = string.Format("{0} < {1}", FieldName, realValue);
                        break;
                    case EnumCompare.不等于:
                        stringTemp = string.Format("{0} <> {1}", FieldName, realValue);
                        break;
                    case EnumCompare.自定义筛选条件:
                        stringTemp = SearchItemChild.ToString();
                        break;
                    case EnumCompare.大于等于:
                        stringTemp = string.Format("{0} >= {1}", FieldName, realValue);
                        break;
                    case EnumCompare.小于等于:
                        stringTemp = string.Format("{0} <= {1}", FieldName, realValue);
                        break;
                }
                #endregion
            }
            else
            {
                #region 字符串格式
                switch (CompareExpression)
                {
                    case EnumCompare.等于:
                        if (string.IsNullOrEmpty(realValue))
                        {
                            stringTemp = string.Format("(isnull({0}) or {0}='')", FieldName);
                        }
                        else
                        {
                            stringTemp = string.Format("{0} = '{1}'", FieldName, realValue);
                        }
                        break;
                    case EnumCompare.大于:
                        stringTemp = string.Format("{0} > '{1}'", FieldName, realValue);
                        break;
                    case EnumCompare.小于:
                        stringTemp = string.Format("{0} < '{1}'", FieldName, realValue);
                        break;
                    case EnumCompare.不等于:
                        stringTemp = string.Format("{0} <> '{1}'", FieldName, realValue);
                        break;
                    case EnumCompare.包含:
                        stringTemp = string.Format("{0} like '%{1}%'", FieldName, realValue);
                        break;
                    case EnumCompare.开头是:
                        stringTemp = string.Format("{0} like '{1}%'", FieldName, realValue);
                        break;
                    case EnumCompare.结尾是:
                        stringTemp = string.Format("{0} = '%{1}'", FieldName, realValue);
                        break;
                    case EnumCompare.自定义筛选条件:
                        stringTemp = SearchItemChild.ToString();
                        break;
                    case EnumCompare.大于等于:
                        stringTemp = string.Format("{0} >= '{1}'", FieldName, realValue);
                        break;
                    case EnumCompare.小于等于:
                        stringTemp = string.Format("{0} <= '{1}'", FieldName, realValue);
                        break;
                }
                #endregion
            }
            return stringTemp;
        }
    }
}
