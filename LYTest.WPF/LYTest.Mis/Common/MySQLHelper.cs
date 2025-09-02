using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;


namespace LYTest.Mis.Common
{
    public class MySQLHelper
    {
        #region 属性
        /// <summary>
        /// 数据库IP地址
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 数据库端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 数据源名称:m_quality
        /// </summary>
        public string Database { get; set; }
        /// <summary>
        /// 数据库登陆用户
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 数据库登陆密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// WebServer路径
        /// </summary>
        public string WebServiceURL { get; set; }

        /// <summary>
        /// 获取当前联接字符串
        /// </summary>
        public string ConnectString
        {
            get
            {
                return string.Format("Server={0};port={1};Database={2};Uid={3};pwd={4};charset=utf8",
                        Ip, Port, Database, UserId, Password);
            }
        }
        #endregion

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlList">多条SQL语句</param>
        public bool Execute(List<string> sqlList)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    MySqlTransaction tx = conn.BeginTransaction();
                    cmd.Transaction = tx;
                    foreach (string sql in sqlList)
                    {
                        if (string.IsNullOrEmpty(sql)) continue;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }

                conn.Close();

                return true;

            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataSet</returns>
        public DataTable ExecuteReader(string sql)
        {
            DataTable ds = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectString))
            {
                conn.Open();
                MySqlDataAdapter command = new MySqlDataAdapter(sql, conn);
                command.Fill(ds);
                conn.Close();
            }
            return ds;
        }
    }
}
