using LYTest.Core;
using System.Data;
using System.Data.SqlClient;

namespace LYTest.Mis.Common
{
    /// <summary>
	/// 数据访问基础类(基于SQL server)
	/// </summary>
    internal abstract class SqlHelper
    {
        //数据库连接字符串，可以动态更改StrConnectString支持多数据库.        
        public SqlHelper()
        {

        }
        public static void SetDataConfig(string ip, int port, string dataSource, string userId, string pwd, string webUrl)
        {
            Ip = ip;
            Port = port;
            DataSource = dataSource;
            UserId = userId;
            Password = pwd;
            WebServiceURL = webUrl;
        }

        private static string StrConnectString
        {
            get
            {
                return string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};Persist Security Info=True", $"{Ip}", DataSource, UserId, Password);
            }
        }

        /// <summary>
        /// 数据库IP地址
        /// </summary>
        public static string Ip { get; private set; }
        /// <summary>
        /// 数据库端口号
        /// </summary>
        public static int Port { get; private set; }
        /// <summary>
        /// 数据源名称
        /// </summary>
        public static string DataSource { get; private set; }
        /// <summary>
        /// 数据库登陆用户
        /// </summary>
        public static string UserId { get; private set; }
        /// <summary>
        /// 数据库登陆密码
        /// </summary>
        public static string Password { get; private set; }

        /// <summary>
        /// WebServer路径
        /// </summary>
        public static string WebServiceURL { get; private set; }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteNonQuery(string sql)
        {
            using (SqlConnection connection = new SqlConnection(StrConnectString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    int r = cmd.ExecuteNonQuery();
                    LogHelper.WriteLog($"{sql} 【{r}】");

                    return r;
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataTable Query(string sql)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(StrConnectString))
            {
                conn.Open();
                SqlDataAdapter command = new SqlDataAdapter(sql, conn);
                command.Fill(dt);
                conn.Close();

            }
            return dt;
        }


    }
}
