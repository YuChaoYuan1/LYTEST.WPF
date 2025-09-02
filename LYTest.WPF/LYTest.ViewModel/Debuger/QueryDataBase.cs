using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.Debug
{
    /// <summary>
    /// 数据库
    /// </summary>
    public class QueryDataBase : ViewModelBase
    {
        public GeneralDal GeneralDal;

        public QueryDataBase()
        {
            DataBaseNames = new ObservableCollection<string>();
            TableNames = new ObservableCollection<string>();
            DataBaseNames.Clear();
            DataBaseNames.Add("AppData");
            DataBaseNames.Add("MeterData");
            DataBaseNames.Add("TmpMeterData");
            DataBaseNames.Add("SchemaData");
        }



        private string dataBaseName;
        /// <summary>
        /// 当前的数据库
        /// </summary>
        public string DataBaseName
        {
            get { return dataBaseName; }
            set
            {
                SetPropertyValue(value, ref dataBaseName, "DataBaseName");
                LoadTableName();
            }
        }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public ObservableCollection<string> DataBaseNames { get; set; }

        /// <summary>
        /// 表名称
        /// </summary>
        public ObservableCollection<string> TableNames { get; set; }

        private ObservableCollection<string> fieldNames = new ObservableCollection<string>();
        /// <summary>
        /// 字段名称
        /// </summary>
        public ObservableCollection<string> FieldNames
        {
            get { return fieldNames; }
            set { SetPropertyValue(value, ref fieldNames, "FieldNames"); }
        }
        private string tableName;
        /// <summary>
        /// 当前的表
        /// </summary>
        public string TableName
        {
            get { return tableName; }
            set
            {
                SetPropertyValue(value, ref tableName, "TableName");
                LoadTableFields();
            }
        }
        /// <summary>
        /// 加载所有表的所有列名称
        /// </summary>
        private void LoadTableFields()
        {
            if (TableName==null)
            {
                return;
            }
            List<FieldModel> fieldModels = GeneralDal.GetFields(TableName);
            IEnumerable<string> list = from item in fieldModels select item.FieldName;
            FieldNames.Clear();
            for (int i = 0; i < list.Count(); i++)
            {
                FieldNames.Add(list.ElementAt(i));
            }
        }
        /// <summary>
        /// 加载所有表的名称
        /// </summary>
        private void LoadTableName()
        {
            switch (DataBaseName)
            {
                case "AppData":
                    GeneralDal = DALManager.ApplicationDbDal;
                    break;
                case "MeterData":
                    GeneralDal = DALManager.MeterDbDal;
                    break;
                case "TmpMeterData":
                    GeneralDal = DALManager.MeterTempDbDal;
                    break;
                //case "WcLimit":
                //    GeneralDal = DALManager.WcLimitDal;
                //    break;
                //case "Log":
                //    GeneralDal = DALManager.LogDal;
                    //break;
                case "SchemaData":
                    GeneralDal = DALManager.SchemaDal;
                    break;
                default:
                    break;
            }
            List<string> fieldModels = GeneralDal.GetTableNames();
            TableNames.Clear();
            for (int i = 0; i < fieldModels.Count(); i++)
            {
                TableNames.Add(fieldModels[i]);
            }
        }

        //GetAllTableData
        private DataTable data;
        public DataTable Data
        {
            get { return data; }
            set { SetPropertyValue(value, ref data, "Data"); }
        }


    }
}
