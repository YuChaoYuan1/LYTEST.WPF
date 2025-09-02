using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;

namespace LYTest.DAL
{
    /// <summary>
    /// 通用数据存取层:Access数据库和SqlServer
    /// </summary>
    public class GeneralDal
    {
        #region 私有
        /// <summary>
        /// 表的列名称字典
        /// </summary>
        private readonly Dictionary<string, List<FieldModel>> tableFields = new Dictionary<string, List<FieldModel>>();
        /// <summary>
        /// 连接字符串,仅在创立连接时赋值
        /// </summary>
        private readonly string connectionString = "";

        #endregion

        public GeneralDal(string connString)
        {
            connectionString = connString;
        }

        #region 基本查询
        /// <summary>
        /// 获取数据库中所有表的名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                DataTable table = conn.GetSchema("Tables");
                foreach (DataRow row in table.Rows)
                {
                    if (row["TABLE_TYPE"].ToString() == "TABLE" || row["TABLE_TYPE"].ToString() == "BASE TABLE")
                    {
                        tableNames.Add(row["TABLE_NAME"].ToString());
                    }
                }
            }
            return tableNames;

        }
        /// <summary>
        /// 获取表所有列的名称
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public List<FieldModel> GetFields(string tableName)
        {
            List<FieldModel> fields = new List<FieldModel>();
            if (tableFields.ContainsKey(tableName))
                return tableFields[tableName];

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                string[] res = new string[] { null, null, tableName };
                DataTable table = conn.GetSchema("Columns", res);
                foreach (DataRow row in table.Rows)
                {
                    string fieldName = row["COLUMN_NAME"].ToString();
                    string fieldType = row["DATA_TYPE"].ToString();

                    fields.Add(new FieldModel(fieldName, GetDataType(fieldType)));
                }
                tableFields.Add(tableName, fields);
            }
            return fields;

        }



        public DataTable GetAllTableData(string tableName)
        {
            DataTable table = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand command = new OleDbCommand($"select * from {tableName}", conn);
                OleDbDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                table.Load(reader);
            }
            return table;


        }
        private EnumFieldDataType GetDataType(string idType)
        {
            EnumFieldDataType dataType;
            switch (idType)
            {
                case "130":
                    dataType = EnumFieldDataType.字符串;
                    break;
                case "3":

                case "131":
                    dataType = EnumFieldDataType.数值;
                    break;
                case "7":
                    dataType = EnumFieldDataType.时间;
                    break;
                default:
                    dataType = EnumFieldDataType.字符串;
                    break;
            }
            return dataType;
        }
        #endregion

        #region 增删改查
        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="stringWhere">查询条件</param>
        /// <returns></returns>
        public DynamicModel GetByID(string tableName, string stringWhere = "")
        {
            //获取Sql语句
            string sql = string.Format("select * from {0}", tableName);
            if (!string.IsNullOrEmpty(stringWhere))
            {
                sql = string.Format("select * from {0} where {1}", tableName, stringWhere);
            }
            List<DynamicModel> models = GetModels(tableName, sql);
            if (models.Count > 0)
            {
                return models[0];
            }
            return null;
        }
        /// <summary>
        /// 根据sqlstring获取表格
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="stringWhere">查询条件</param>
        /// <returns></returns>
        public List<DynamicModel> GetList(string tableName, string stringWhere = "")
        {
            //获取Sql语句
            string sql = string.Format("select * from {0}", tableName);
            if (!string.IsNullOrEmpty(stringWhere))
            {
                sql = string.Format("select * from {0} where {1}", tableName, stringWhere);
            }
            return GetModels(tableName, sql);
        }
        /// <summary>
        /// 根据reader获取数据
        /// </summary>
        /// <param name="tableName">sql查询字符串</param>
        /// <param name="sql">表名称</param>
        /// <returns></returns>
        public List<DynamicModel> GetModels(string tableName, string sql)
        {
            List<DynamicModel> models = new List<DynamicModel>();
            List<FieldModel> fields = GetFields(tableName);
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand(sql, connection);
                OleDbDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                while (reader.Read())
                {
                    DynamicModel model = new DynamicModel();
                    for (int i = 0; i < fields.Count; i++)
                    {
                        model.SetProperty(fields[i].FieldName, reader[fields[i].FieldName]);
                    }
                    models.Add(model);
                }
            }

            return models;
        }
        /// <summary>
        /// 获取一页
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName">用于排序的字段</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="stringWhere"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public List<DynamicModel> GetPage(string tableName, string fieldName, int pageSize, int pageIndex, string stringWhere = "", bool isAsc = true)
        {
            //第0页不处理
            if (pageIndex == 0) return new List<DynamicModel>();
            //"Example:select * from (select top 10 * from (select top 100 * from meter_info order by DTM_TEST_DATE ) AS a order by DTM_TEST_DATE desc) as b order by DTM_TEST_DATE;"
            string sql;

            #region 获取sql语句
            if (isAsc)
            {
                if (string.IsNullOrEmpty(stringWhere))
                {
                    sql = string.Format("select * from (select top {2} * from (select top {3} * from {0} order by {1} ) AS a order by {1} desc) as b order by {1}", tableName, fieldName, pageSize, pageSize * pageIndex);
                }
                else
                {
                    sql = string.Format("select * from (select top {2} * from (select top {3} * from {0} where {4} order by {1} ) AS a order by {1} desc) as b order by {1}", tableName, fieldName, pageSize, pageSize * pageIndex, stringWhere);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(stringWhere))
                {
                    sql = string.Format("select * from (select top {2} * from (select top {3} * from {0} order by {1} desc) AS a order by {1}) as b order by {1} desc", tableName, fieldName, pageSize, pageSize * pageIndex);
                }
                else
                {
                    sql = string.Format("select * from (select top {2} * from (select top {3} * from {0} where {4} order by {1} desc) AS a order by {1} ) as b order by {1} desc", tableName, fieldName, pageSize, pageSize * pageIndex, stringWhere);
                }
            }
            #endregion
            return GetModels(tableName, sql);
        }
        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int GetCount(string tableName, string where = "")
        {
            int count = 0;
            string sql = $"select count(*) from {tableName}";

            if (!string.IsNullOrEmpty(where))
            {
                sql += $" where {where}";
            }
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand comm = new OleDbCommand(sql, conn);
                DbDataReader reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    count = (int)reader[0];
                }
            }

            return count;
        }
        /// <summary>
        /// 插入行
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param>查询条件</param>
        /// <param name="model">要插入的数据模型</param>
        /// <returns>操作结果</returns>
        public int Insert(string tableName, DynamicModel model)
        {
            List<string> fields = model.GetAllProperyName();
            StringBuilder sbField = new StringBuilder();
            StringBuilder sbValue = new StringBuilder();
            List<FieldModel> fieldModels = GetFields(tableName);
            for (int i = 0; i < fields.Count; i++)
            {
                if (model.GetProperty(fields[i]) == null || string.IsNullOrEmpty(model.GetProperty(fields[i]).ToString()))
                {
                    continue;
                }

                FieldModel fieldModel = fieldModels.Find(item => item.FieldName == fields[i]);
                if (fieldModel != null)
                {
                    //自增型不写入
                    if (fieldModel.FieldName.ToLower() == "id")
                    {
                        continue;
                    }
                    switch (fieldModel.DataType)
                    {
                        case EnumFieldDataType.数值:
                            sbValue.Append($"{model.GetProperty(fields[i])},");
                            break;
                        default:  // 字符串
                            sbValue.Append($"'{model.GetProperty(fields[i])}',");
                            break;
                    }
                    sbField.Append($"{fields[i]},");
                }
            }
            sbField.Remove(sbField.Length - 1, 1);
            sbValue.Remove(sbValue.Length - 1, 1);
            string sql = $"insert into {tableName} ({sbField}) values ({sbValue})";

            int count = 0;
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand comm = new OleDbCommand(sql, conn);
                count = comm.ExecuteNonQuery();
            }

            return count;
        }

        public int Insert(string tableName, List<DynamicModel> models)
        {
            List<string> sqlList = new List<string>();
            for (int j = 0; j < models.Count; j++)
            {
                List<string> fields = models[j].GetAllProperyName();
                StringBuilder sbField = new StringBuilder();
                StringBuilder sbValue = new StringBuilder();
                List<FieldModel> fieldModels = GetFields(tableName);
                for (int i = 0; i < fields.Count; i++)
                {
                    if (models[j].GetProperty(fields[i]) == null || string.IsNullOrEmpty(models[j].GetProperty(fields[i]).ToString()))
                    {
                        continue;
                    }

                    FieldModel fieldModel = fieldModels.Find(item => item.FieldName == fields[i]);
                    if (fieldModel != null)
                    {
                        //自增型不写入
                        if (fieldModel.FieldName.ToLower() == "id") continue;

                        string value = models[j].GetProperty(fields[i]).ToString();
                        if (fieldModel.DataType == EnumFieldDataType.数值)
                            sbValue.Append($"{value},");
                        else // 字符串
                            sbValue.Append($"'{value}',");

                        sbField.Append($"{fields[i]},");
                    }
                }
                sbField.Remove(sbField.Length - 1, 1);
                sbValue.Remove(sbValue.Length - 1, 1);

                sqlList.Add($"insert into {tableName} ({sbField}) values ({sbValue})");
            }
            return ExecuteOperation(sqlList);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="where">更新条件</param>
        /// <param name="model">要更新的模型</param>
        /// <param name="fields">要更新的字段</param>
        /// <returns>操作结果</returns>
        public int Update(string tableName, string where, DynamicModel model, List<string> fields)
        {
            StringBuilder sb = new StringBuilder();
            List<FieldModel> fieldModels = GetFields(tableName);
            for (int i = 0; i < fields.Count; i++)
            {
                FieldModel fieldModel = fieldModels.Find(item => item.FieldName == fields[i]);
                if (fieldModel != null)
                {
                    switch (fieldModel.DataType)
                    {
                        case EnumFieldDataType.字符串:
                            sb.Append($" [{fields[i]}] = '{model.GetProperty(fields[i])}',");
                            break;
                        case EnumFieldDataType.数值:
                            sb.Append($" [{fields[i]}] = {model.GetProperty(fields[i])},");
                            break;
                        case EnumFieldDataType.时间:
                            var timeObj = model.GetProperty(fields[i]);
                            if (timeObj is DateTime time)
                            {
                                sb.Append($" [{fields[i]}] = #{time:yyyy-MM-dd HH:mm:ss}#,");
                            }
                            break;
                        default:
                            sb.Append($" [{fields[i]}] = '{model.GetProperty(fields[i])}',");
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(sb.ToString()))
            {
                return 0;
            }
            sb.Remove(sb.Length - 1, 1);
            string sql = string.Format("update {0} set {1} where {2}", tableName, sb, where);
            int count = 0;
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand comm = new OleDbCommand(sql, conn);
                count = comm.ExecuteNonQuery();
            }

            return count;
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="fieldName">作对比的列名称</param>
        /// <param name="models">要更新的模型</param>
        /// <param name="fields">要更新的字段</param>
        /// <returns>操作结果</returns>
        public int Update(string tableName, string fieldName, List<DynamicModel> models, List<string> fields)
        {
            List<FieldModel> fieldModels = GetFields(tableName);
            List<string> sqlList = new List<string>();
            for (int j = 0; j < models.Count; j++)
            {
                StringBuilder sb = new StringBuilder();
                DynamicModel model = models[j];
                #region 条件
                FieldModel fieldModelTemp = fieldModels.Find(item => item.FieldName == fieldName);
                string where = string.Format("{0}='{1}'", fieldName, model.GetProperty(fieldName));
                switch (fieldModelTemp.DataType)
                {
                    case EnumFieldDataType.数值:
                        where = string.Format(" [{0}] = {1}", fieldName, model.GetProperty(fieldName));
                        break;
                    case EnumFieldDataType.时间:
                        var timeObj = model.GetProperty(fieldName);
                        if (timeObj is DateTime time)
                        {
                            where = string.Format(" [{0}] = #{1}#", fieldName, time.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        break;
                }
                #endregion
                #region 获取当前的sql语句
                for (int i = 0; i < fields.Count; i++)
                {
                    FieldModel fieldModel = fieldModels.Find(item => item.FieldName == fields[i]);
                    if (fieldModel != null)
                    {
                        switch (fieldModel.DataType)
                        {
                            case EnumFieldDataType.字符串:
                                sb.Append(string.Format(" [{0}] = '{1}',", fields[i], model.GetProperty(fields[i])));
                                break;
                            case EnumFieldDataType.数值:
                                sb.Append(string.Format(" [{0}] = {1},", fields[i], model.GetProperty(fields[i])));
                                break;
                            case EnumFieldDataType.时间:
                                var timeObj = model.GetProperty(fields[i]);
                                if (timeObj is DateTime time)
                                {
                                    sb.Append(string.Format(" [{0}] = #{1}#,", fields[i], time.ToString("yyyy-MM-dd HH:mm:ss")));
                                }
                                break;
                            default:
                                sb.Append(string.Format(" [{0}] = '{1}',", fields[i], model.GetProperty(fields[i])));
                                break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(sb.ToString()))
                {
                    return 0;
                }
                sb.Remove(sb.Length - 1, 1);
                #endregion

                sqlList.Add(string.Format("update {0} set {1} where {2}", tableName, sb, where));
            }
            return ExecuteOperation(sqlList);
        }
        /// <summary>
        /// 执行一系列的sql语句
        /// </summary>
        /// <param name="sqlList"></param>
        /// <returns>受影响的行数</returns>
        public int ExecuteOperation(List<string> sqlList)
        {
            #region SQL语句合法性检查
            bool isEmpty = true;
            foreach (string sql in sqlList)
            {
                if (!string.IsNullOrEmpty(sql))
                {
                    isEmpty = false;
                    break;
                }
            }
            if (isEmpty || sqlList.Count == 0)
            {
                return 0;
            }
            #endregion
            int count = 0;

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbTransaction transaction = connection.BeginTransaction();
                OleDbCommand command = new OleDbCommand
                {
                    Connection = connection,
                    Transaction = transaction
                };
                for (int i = 0; i < sqlList.Count; i++)
                {
                    string sqlError = sqlList[i];
                    command.CommandText = sqlList[i];
                    count += command.ExecuteNonQuery();
                }
                transaction.Commit();

            }

            return count;
        }
        /// <summary>
        /// 删除行
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete(string tableName, string where)
        {
            int count = 0;
            string sql = string.Format("delete from {0} where {1}", tableName, where);

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                count = cmd.ExecuteNonQuery();
            }

            return count;
        }
        /// 获取某一字段的所有值
        /// <summary>
        /// 获取某一字段的所有值
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="where">查询条件</param>
        /// <param name="topCount">选择多少条数据</param>
        /// <param name="isAsc">升序还是降序</param>
        /// <returns>获取到的值列表</returns>
        public List<string> GetDistinct(string tableName, string fieldName, string where = "", int topCount = 0, bool isAsc = true)
        {
            List<string> valueList = new List<string>();
            string sql = string.Format("select distinct {1} from {0}", tableName, fieldName);
            if (topCount > 0)
            {
                if (isAsc)
                {
                    sql = string.Format("select distinct top {3} {1} from {0} order by {1} asc", tableName, fieldName, where, topCount);
                }
                else
                {
                    sql = string.Format("select distinct top {3} {1} from {0} order by {1} desc", tableName, fieldName, where, topCount);
                }
            }
            if (!string.IsNullOrEmpty(where))
            {
                sql = string.Format("select distinct {1} from {0} where {2}", tableName, fieldName, where);
                if (topCount > 0)
                {
                    if (isAsc)
                    {
                        sql = string.Format("select distinct top {3} {1} from {0} where {2} order by {1} asc", tableName, fieldName, where, topCount);
                    }
                    else
                    {
                        sql = string.Format("select distinct top {3} {1} from {0} where {2} order by {1} desc", tableName, fieldName, where, topCount);
                    }
                }
            }
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand(sql, connection);
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    valueList.Add(reader[0].ToString());
                }
            }

            return valueList;
        }
        ///// 根据自定义的列名称获取数据列表
        ///// <summary>
        ///// 根据自定义的列名称获取数据列表
        ///// </summary>
        ///// <param name="sql">sql查询语句</param>
        ///// <param name="fields">列名称列表</param>
        ///// <returns></returns>
        //public List<DynamicModel> GetList(string sql, List<string> fields)
        //{
        //    List<DynamicModel> models = new List<DynamicModel>();
        //    using (OleDbConnection connection = new OleDbConnection(connectionString))
        //    {
        //        connection.Open();
        //        OleDbCommand command = new OleDbCommand(sql, connection);
        //        OleDbDataReader reader = command.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            DynamicModel model = new DynamicModel();
        //            for (int i = 0; i < fields.Count; i++)
        //            {
        //                model.SetProperty(fields[i], reader[fields[i]]);
        //            }
        //            models.Add(model);
        //        }
        //    }

        //    return models;
        //}

        /// <summary>
        /// 根据一系列Sql语句获取模型列表
        /// </summary>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        public List<DynamicModel> GetList(List<string> tableNames, List<string> sqlList)
        {
            List<DynamicModel> models = new List<DynamicModel>();
            if (sqlList == null)
            { return models; }

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                for (int i = 0; i < sqlList.Count; i++)
                {
                    string sql = sqlList[i];
                    string tableName = tableNames[i];

                    List<FieldModel> fields = GetFields(tableName);
                    OleDbCommand command = new OleDbCommand(sql, connection);
                    OleDbDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    while (reader.Read())
                    {
                        DynamicModel model = new DynamicModel();
                        for (int j = 0; j < fields.Count; j++)
                        {
                            model.SetProperty(fields[j].FieldName, reader[fields[j].FieldName]);
                        }
                        models.Add(model);
                    }
                }
            }

            return models;
        }
        #endregion
    }
}
