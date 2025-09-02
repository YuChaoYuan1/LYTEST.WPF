using LYTest.Core;
using LYTest.Utility.Log;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace LYTest.Mis.Common
{
    public class OracleHelper
    {
        //public string ErrMsg { get; private set; }
        public OracleHelper(string ip, int port, string dataSource, string userId, string pwd, string webUrl)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = webUrl;
        }

        public OracleHelper()
        {

        }


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
        /// 数据源名称
        /// </summary>
        public string DataSource { get; set; }
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
                return string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));User ID={3};Password={4};Persist Security Info=True",
                        Ip, Port, DataSource, UserId, Password);
            }
        }
        #endregion

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlList">多条SQL语句</param>
        public bool Execute(List<string> sqlList)
        {
            //LogHelper.WriteLog("===========================================================================================================");

            string str = "";
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnectString))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        OracleTransaction tran = conn.BeginTransaction();
                        cmd.Transaction = tran;

                        foreach (string s in sqlList)
                        {
                            if (string.IsNullOrWhiteSpace(s)) continue;
                            if (s.Length < 5) continue;
                            str = s;
                            cmd.CommandText = s;
                            cmd.ExecuteNonQuery();

                            //LogHelper.WriteLog(str);
                        }
                        tran.Commit();
                    }
                    conn.Close();
                }
                //LogHelper.WriteLog("数据提交成功！");
                //LogHelper.WriteLog("*******************************************************************************************************************");
                return true;
            }
            catch (Exception ex)
            {

                LogManager.AddMessage($"执行SQL语句错误:{str}\n\r{ex.StackTrace}", EnumLogSource.检定业务日志, EnumLevel.Error);

                //LogHelper.WriteLog($"数据提交失败！{ex.Message}\r\n{ex.StackTrace}");
                //LogHelper.WriteLog("*******************************************************************************************************************");
                return false;
            }

        }

        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public DataTable ExecuteReader(string sql)
        {
            DataTable dt = new DataTable();
            using (OracleConnection conn = new OracleConnection(ConnectString))
            {
                conn.Open();
                using (OracleDataAdapter adp = new OracleDataAdapter(sql, conn))
                {
                    adp.Fill(dt);
                }
                conn.Close();
            }
            return dt;

        }
        private static readonly object Lock = new object();

        public DataTable Query(string sql)
        {
            DataTable table = new DataTable();
            lock (Lock)
            {
                using (OracleConnection conn = new OracleConnection(ConnectString))
                {
                    conn.Open();
                    using (OracleDataAdapter adapter = new OracleDataAdapter(sql, conn))
                    {
                        adapter.Fill(table);
                    }

                    conn.Close();
                }
            }
            return table;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            object o = null;
            using (OracleConnection conn = new OracleConnection(ConnectString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    o = cmd.ExecuteScalar();
                }
                conn.Close();
            }

            return o;
        }

        public int ExecuteNonQuery(string sql)
        {
            int count = 0;
            using (OracleConnection conn = new OracleConnection(ConnectString))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    count = cmd.ExecuteNonQuery();
                }
                conn.Close();
            }

            return count;
        }


    }
}
